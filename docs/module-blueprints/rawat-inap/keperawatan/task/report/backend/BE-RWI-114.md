# Laporan Perubahan Backend — `BE-RWI-114`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-114` |
| Judul | MAR dan pembentukan dosis (migration K4) |
| Slice | Gelombang 2 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-114` |
| Trace | `FR-KEP-064`, `FR-KEP-071`; `INT-KEP-08`; `VAL-KEP-34`, `VAL-KEP-36a`/`b`/`e`; kamus data 11.12–11.14; arsitektur 11.5.10–11.5.11; api-contract 7.11, 7.13 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-106` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — empat tabel baru modul lain, hosted service, idempotensi pembentukan dosis |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Areas/HealthServices/ClinicalManagement/Services/HospitalTimeZone.cs`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — enam kriteria terpetakan; `dotnet build` lolos; migration K4 **diterapkan** ke `QuilvianNewDevHamzah`, skema terverifikasi dari katalog; uji idempoten `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement` (pemilik tabel MAR) |
| Registry / prefix | `Phm` — `ACTIVE`; entity baru `PhmMedicationAdministration`, `PhmMedicationAdministrationRevision`, `PhmMedicationScheduleTime`, `PhmMedicationAdministrationSetting` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-CODE-001/003/006`, `QBE-DEL-001`, `QBE-LOG-001` |
| Hak akses baru | `MedicationAdministration : Read`; `MedicationScheduleSetting : Read`, `: Update` |
| Nomor bisnis | `AdministrationNumber` dari `NumberSeriesAllocator` deret `PHM_MEDICATION_ADMINISTRATION`, awalan `MAR`, reset harian, 6 digit |
| Database | `20260917102000_AddMedicationAdministration` (K4); `Down` menolak bila tabel berisi data; **diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |

## 1. Masalah yang diperbaiki

Rawat inap tidak punya daftar dosis yang harus diberikan perawat. Tanpa daftar yang dibentuk dari resep aktif, perawat
menyalin jadwal dari resep secara manual, dan dua pembentukan yang tidak idempoten menghasilkan dua perintah pemberian
untuk satu jam yang sama — risiko obat diberikan dua kali.

## 2. Proses bisnis

1. Farmasi mengonfigurasi jam pemberian: `PUT medication-schedule-settings/schedule-times` `{ "frequencyCode": "q12h", "serviceUnitId": null, "times": ["08:00","20:00"] }`.
   ICU boleh menimpa kode yang sama untuk unitnya; baris unit mengalahkan bawaan.
2. Dokter mengajukan resep harian Ceftriaxone 1 g IV `q12h` (status `Submitted`).
3. Perawat membuka MAR pukul 07.00 → `GET medication-administrations/episodes/{episodeId}`. Sebelum membaca, server
   membentuk dosis `Due` dari butir resep aktif yang tidak dihentikan, bukan PRN, berdosis tetap, dan kodenya punya jam,
   sampai cakrawala pengaturan (bawaan 24 jam), tidak sebelum resep diajukan → 08.00 dan 20.00.
4. MAR dibuka lagi pukul 07.05 → dua slot sudah ada → **nol dosis baru**. Dua pembukaan serentak menabrak unique
   `UX_PhmMedicationAdministration_Item_ScheduledAt`; tabrakan ditelan sebagai "sudah ada".
5. `MedicationDoseSchedulerHostedService` menjalankan pembentukan yang sama tiap 15 menit untuk episode berjalan;
   galat satu episode dicatat dan episode berikutnya tetap diproses.
6. Butir `q6h` tanpa jadwal: tidak ada dosis terbentuk, MAR menampilkan "Jadwal pemberian untuk frekuensi q6h belum
   dikonfigurasi", dan `GET medication-schedule-settings/frequency-codes-without-schedule` memuat `q6h`.
7. **Jalur tidak normal:** jam ganda → `400`; jumlah slot berbeda dari kali per hari yang dipakai resep aktif → `400`
   "Kode q8h membutuhkan 3 jam pemberian."; cakrawala di luar 1–72 jam → `400`; nomor gagal terbit → dosis yang sudah
   ada tetap tampil beserta `doseGenerationWarning`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 11.12–11.14, arsitektur 11.5.10–11.5.13, integration contract 8.2, api-contract 7.11 dan 7.13,
`PhmPrescription.cs`, `PhmPrescriptionItem.cs`, `InpatientPrescriptionService.cs`, `EmergencyTriageSlaMonitorHostedService.cs`,
`AttendanceSchedulerHostedService.cs`, `NumberSeriesAllocator.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Enums/MedicationDoseSource.cs`, `MedicationDoseStatus.cs`, `MedicationDoubleCheckStatus.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Models/PhmMedicationAdministration.cs`, `PhmMedicationAdministrationRevision.cs`, `PhmMedicationScheduleTime.cs`, `PhmMedicationAdministrationSetting.cs` | Baru |
| `Repositories/Configurations/HealthServices/PharmacyManagement/MedicationAdministrationConfigurations.cs` | Baru — FK, check constraint, unique parsial bernama |
| `Repositories/ApplicationDbContext.cs`, `Migrations/20260917102000_AddMedicationAdministration.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs` | DbSet, migration K4, snapshot |
| `Areas/HealthServices/ClinicalManagement/Services/HospitalTimeZone.cs` | Baru — jam dinding Asia/Jakarta |
| `Areas/HealthServices/PharmacyManagement/DTOs/MedicationAdministrationDtos.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.cs` | Baru — `EnsureDosesAsync`, `GetChartAsync`, `GetAsync`, `GetRevisionsAsync`, resolusi jadwal |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Settings.cs` | Baru — jadwal, kode tanpa jadwal, pengaturan |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationDoseSchedulerOptions.cs`, `MedicationDoseSchedulerHostedService.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs` | Baru — `GET episodes/{episodeId}`, `GET {id}`, `GET {id}/revisions` |
| `Areas/HealthServices/PharmacyManagement/Controllers/MedicationScheduleSettingController.cs` | Baru |
| `Program.cs` | Registrasi service, options `HealthServices:MedicationDoseScheduler`, hosted service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru sesuai 7.11 (baca) dan 7.13. Delta: `includeStopped=false` tetap menampilkan butir yang dihentikan **pada hari itu**; respons MAR membawa `frequencyCodesWithoutSchedule` dan `doseGenerationWarning` |
| Database | Empat tabel baru `PharmacyManagement` |
| Keamanan/Auth | Dua resource baru; pembentukan dosis terjadwal memakai akun pada `SystemActorUserId`, atau SuperAdmin bila kosong |
| Perilaku runtime | Hosted service **menyala bawaan** (`Enabled = true`); dapat dimatikan lewat konfigurasi tanpa menghilangkan dosis karena MAR yang dibuka tetap membentuk dosis |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Medication Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | MAR satu hari; membentuk dosis sebelum membaca | `MedicationAdministration : Read` |
| `GET` | `/{id}` | Detail dosis beserta `AvailableActions` | `MedicationAdministration : Read` |
| `GET` | `/{id}/revisions` | Riwayat koreksi dan penolakan cek ganda | `MedicationAdministration : Read` |

#### Health Services / Pharmacy Management / Medication Schedule Setting

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/schedule-times` | Jam per kode frekuensi, unit beserta bawaan | `MedicationScheduleSetting : Read` |
| `PUT` | `/schedule-times` | Mengganti slot satu kode untuk satu unit atau bawaan | `MedicationScheduleSetting : Update` |
| `GET` | `/frequency-codes-without-schedule` | Kode yang dipakai resep aktif tanpa jadwal | `MedicationScheduleSetting : Read` |
| `GET` | `/administration-setting` | Pengaturan MAR (bawaan bila belum disimpan) | `MedicationScheduleSetting : Read` |
| `PUT` | `/administration-setting` | Mengubah cakrawala, penanda lewat waktu, interval evaluasi PRN | `MedicationScheduleSetting : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran kriteria 1–6 ke source | Terpetakan | `PASS` | Bagian 6 |
| Konsistensi nama index/FK migration–konfigurasi–snapshot | Sama; dibangkitkan dari satu spesifikasi | `PASS` | Generator sesi ini |
| Pemeriksaan siklus dependency injection | `MedicationAdministrationService` → `DailyMonitoringService` satu arah; `DailyMonitoringService` membaca tabel MAR tanpa menyuntik balik | `PASS` | Review konstruktor |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Uji idempoten dua pemanggilan dengan keluaran ditempel | Tidak dijalankan | `NOT RUN` | Build dan migration lolos; belum dijalankan. Langkahnya: buka MAR dua kali, bandingkan `DoseGenerationResult.CreatedCount` (kedua = 0) |
| Verifikasi skema Postgres | Dibaca dari katalog `QuilvianNewDevHamzah` setelah `database update` | `PASS` | Lampiran |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel MAR, revisi, jadwal, pengaturan di `PharmacyManagement` | Terpenuhi | Empat model, konfigurasi, migration K4 |
| 2. Dosis `Due` dari butir resep aktif berjadwal | Terpenuhi | `ScheduledItemsQuery` + `EnsureDosesAsync` |
| 3. Pembentukan idempoten | Terpenuhi di source | Pembacaan slot + unique parsial + penelanan tabrakan per baris |
| 4. Saat MAR dibuka dan terjadwal | Terpenuhi | `GetChartAsync` memanggil `EnsureDosesAsync`; `MedicationDoseSchedulerHostedService` |
| 5. Jadwal dan pengaturan dikonfigurasi Farmasi | Terpenuhi | `MedicationScheduleSettingController` |
| 6. Frekuensi tanpa jadwal tetap terlihat | Terpenuhi | `ScheduleMessage`, `FrequencyCodesWithoutSchedule`, endpoint kode tanpa jadwal |
| DoD: `dotnet build`, verifikasi skema, uji idempoten | `dotnet build` lolos; skema terverifikasi dari katalog; **uji idempoten dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Status resep yang dianggap "boleh diberikan" dipilih `Submitted` (satu-satunya status bukan konsep dan bukan batal pada `PrescriptionStatus`); butir racikan tidak membentuk dosis karena MAR menunjuk `PhmPrescriptionItem`. `DurationValue` resep tidak membatasi pembentukan — penghentian butir oleh dokter yang menghentikan dosis |
| Masalah yang diketahui | Gate `G-12` (jam standar) dan `G-14` (interval evaluasi PRN) belum diputuskan; sampai dikonfigurasi, tidak ada dosis terjadwal terbentuk |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` `Program.cs`, `ApplicationDbContext.cs`, snapshot |
| Langkah berikutnya | `BE-RWI-115`, `BE-RWI-118`, `BE-RWI-119`; `BE-RWI-100` [BE-DOK] dan `BE-RWI-087` [BE-INP] kini tidak lagi terblokir tabel MAR |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |

Migration task ini: `20260917102000_AddMedicationAdministration` (K4) — diterapkan ke `QuilvianNewDevHamzah` 17 September 2026.

