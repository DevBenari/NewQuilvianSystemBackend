# Laporan Perubahan Backend — `BE-RWI-122`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-122` |
| Judul | Intake obat tertaut dosis MAR |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-122` |
| Trace | `FR-KEP-058`, `FR-KEP-063`; `RWI-DEC-149`, `RWI-DEC-150` (`G-26`, `G-27`); `VAL-KEP-24d`–`g`, `VAL-KEP-36c`, `VAL-KEP-36f`; `INT-KEP-10`; api-contract 7.5, 7.6 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-119` ✅, `BE-RWI-115` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — tautan lintas modul, penandaan dalam transaksi koreksi |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Areas/HealthServices/PharmacyManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — enam kriteria terpetakan; `dotnet build` lolos; verifikasi proses bisnis `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` (pemilik entri cairan) membaca `PharmacyManagement` (pemilik dosis) |
| Registry / prefix | `Cli`, `Phm` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-MOD-001` |
| Database | Nol migration tambahan — FK dosis, check constraint `CK_CliFluidBalanceEntry_MedicationLink`, unique `UX_CliFluidBalanceEntry_MedicationAdministration_Active` dari K5 |

## 1. Masalah yang diperbaiki

Obat yang masuk lewat infus menambah cairan masuk. Tanpa tautan ke dosis yang benar-benar diberikan, volume obat bisa
dihitung dua kali, tercatat untuk dosis yang belum diberikan, atau hilang dari balance.

## 2. Proses bisnis

1. Ceftriaxone 1 g dalam NaCl 100 ml `Administered` 08.05. Perawat mencatat `POST fluid-balance-entries` sumber `Medication`,
   `MedicationAdministrationId` dosis itu, dan **mengetik** 100 ml — volume aktual termasuk pelarut (`RWI-DEC-149`).
2. Entri ikut dijumlah pada balance `BE-RWI-120` seperti entri masuk lain.
3. Ringkasan Pengawasan Harian memuat `AdministeredDosesWithoutIntake` — dosis `Administered` hari itu tanpa entri
   intake aktif — sebagai **pengingat**, tidak mewajibkan (`G-27`).
4. Dosis dikoreksi menjadi `Held` "tercatat pada pasien salah" → di dalam transaksi koreksi MAR,
   `DailyMonitoringService.FlagLinkedFluidEntryAsync` mengisi `DoseCorrectionFlaggedAt`; entri tampil `NeedsReview`,
   volumenya **tidak** diubah (`G-26`). Setelah perawat mengoreksi entri itu, `NeedsReview` padam.
5. **Jalur tidak normal:** sumber Obat tanpa dosis, atau dosis pada sumber lain → `400` "Intake obat wajib memilih dosis
   yang diberikan dari MAR."; dosis belum diberikan → `409` "Dosis ini belum tercatat diberikan."; dosis sudah punya entri
   aktif → `409` "Dosis ini sudah punya entri intake pukul 08.10. Koreksi entri itu bila volumenya salah."; dosis pasien
   lain → `422` "Dosis ini bukan milik pasien ini."; dua perawat menautkan dosis yang sama bersamaan → unique menolak yang
   kedua dan jawabannya `409` yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Integration contract 8.4, validation matrix 6.3 dan 6.7, kamus data 11.8.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.Fluid.cs` | `ValidateMedicationLinkAsync`, `FlagLinkedFluidEntryAsync`, `NeedsReview` |
| `Areas/HealthServices/ClinicalManagement/Services/DailyMonitoringService.cs` | `AdministeredDosesWithoutIntake` pada ringkasan |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Recording.cs` | `CorrectAsync` memanggil penanda bila status lama `Administered` dan status baru bukan |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.cs` | `HasIntakeEntry` pada sel MAR, `LinkedFluidEntryId` pada detail dosis |
| `Areas/HealthServices/ClinicalManagement/DTOs/DailyMonitoringDtos.cs` | `MedicationAdministrationNumber`, `MedicationDrugName`, `DoseCorrectionFlaggedAt`, `NeedsReview`, `AdministeredDoseWithoutIntakeItem` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai 7.5–7.6; isian tampil tambahan `NeedsReview`, `HasIntakeEntry`, `LinkedFluidEntryId` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Arah ketergantungan satu arah: `PharmacyManagement` hanya memanggil satu metode penanda; `ClinicalManagement` membaca dosis tanpa menyalin datanya |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Fluid Balance

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Sumber Obat wajib menunjuk tepat satu dosis `Administered` | `FluidBalance : Create` |

#### Health Services / Clinical Management / Daily Monitoring

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/summary` | Memuat pengingat dosis tanpa entri intake | `DailyObservation : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran kriteria 1–6 | Terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi kontrak API dan proses bisnis keenam kriteria | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Menunjuk tepat satu dosis `Administered` | Terpenuhi | `VAL-KEP-24d`–`g` pada `RecordFluidAsync` |
| 2. Volume diketik, termasuk pelarut | Terpenuhi | `VolumeMl` dari request; MAR tidak punya kolom volume |
| 3. Satu dosis tidak ditunjuk dua entri | Terpenuhi | Pemeriksaan service + unique parsial |
| 4. Pengingat dosis tanpa entri, tanpa mewajibkan | Terpenuhi | `AdministeredDosesWithoutIntake`; pencatatan MAR tidak menuntut entri |
| 5. Entri yang dosisnya dikoreksi ditandai, nilai tetap | Terpenuhi | `FlagLinkedFluidEntryAsync` di transaksi `CorrectAsync` |
| 6. Ikut dihitung pada balance | Terpenuhi | `GetFluidTotalsAsync` menjumlah seluruh entri masuk aktif |
| DoD: `dotnet build`, verifikasi proses bisnis | `dotnet build` lolos; **verifikasi proses bisnis dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengingat memuat seluruh dosis diberikan tanpa entri, tidak disaring rute — tidak semua obat masuk lewat cairan, dan itu disengaja `G-27` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` |
| Langkah berikutnya | Frontend Pengawasan Harian |

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
