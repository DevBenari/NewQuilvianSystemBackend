# Laporan Perubahan Backend — `BE-RWI-169`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-169` |
| Judul | Surveilans infeksi luka operasi (`K9` bagian surveilans, `K12`) |
| Slice | `MVP-3` / `RWF-W7` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-169` |
| Trace | `FR-RWF-083`, `084`, `091`, `092`; `RWI-DEC-202`; `INV-RWF-15`, `16`; `INT-RWF-09`, `10`; `AC-RWF-083`, `095`, `096`; `UAT-RWF-18`, `UAT-RWF-28` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.5; integrasi 9.3; backend 12.6.3, 12.7, 12.12 |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 12: repository 0, berkas diperiksa 2, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kesembilan acceptance criteria terpetakan ke source; build akhir `PASS`; tabel diterapkan ke database development. Gerbang produksi G-21 tetap terbuka |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`; membaca `OperatingRoomManagement` dan `InPatientManagement` tanpa mengubahnya |
| Prefix registry | `Cli` — `ACTIVE / LEGACY` (entity legacy `TrxNosocomialInfection` dipakai ulang, tidak dinamai ulang) |
| Keberlakuan | `NEW CODE` (tiga model, enum, service, worker, controller, DTO, layanan nomor); `TOUCHED LEGACY` (`ClinicalInstrumentKind`, `ClinicalInstrumentDraftSeeder`, `NosocomialInfectionController`) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`/`002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-001`/`003`/`006` (nomor register nosokomial) |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Tim PPI (Pencegahan dan Pengendalian Infeksi) tidak punya formulir surveilans luka operasi di sistem. Pemantauan 15 hari setelah operasi dilakukan di kertas, suhu disalin manual dari catatan tanda vital, dan dugaan infeksi tidak otomatis masuk register infeksi nosokomial. Selain itu, nomor register nosokomial lama dibuat dengan cara "hitung baris hari ini + 1", yang dapat menghasilkan nomor ganda bila dua petugas menyimpan bersamaan.

## 2. Proses bisnis

**Tujuan.** Formulir surveilans per kasus operasi terbentuk otomatis sesudah operasi selesai, diisi harian dengan suhu dibaca dari tanda vital, berhenti saat pasien keluar ruangan, dan dapat ditandai "dicurigai" oleh tim PPI.

**Pelaku.**

| Pelaku | Peran |
| --- | --- |
| Sistem (worker tiap 5 menit) | Membentuk formulir dan menghentikan/menyelesaikan formulir |
| Perawat bangsal | Mengisi isian harian dan ringkasan kultur/serologi (`SurgicalSiteSurveillance : Update`) |
| Tim PPI | Menandai dicurigai (`SurgicalSiteSurveillance : Review`) |
| Pemilik klinis / komite PPI | Mengesahkan versi formulir sebelum dipakai pasien sungguhan (G-21) |

**Prasyarat.** Ada versi instrumen `SurgicalSiteSurveillanceForm` berstatus `Approved` dengan nama pengesah. Data awal `K12` sengaja `Draft`.

**Langkah utama.**

1. Worker mencari kasus operasi `Completed` yang kunjungannya milik episode `Admitted`/`DischargePending` dan belum punya formulir.
2. Untuk setiap kasus, worker mengunci kunci kasus, membaca waktu selesai dari riwayat status OK, lalu membentuk formulir dengan versi `Approved` terbaru. Hari ke-1 adalah tanggal kalender Asia/Jakarta sesudah tanggal operasi selesai.
3. Perawat mengisi hari ke-N. Isian divalidasi terhadap definisi versi formulir; suhu tidak diketik, melainkan dibaca dari tanda vital berstatus tercatat pada kunjungan dan hari yang sama.
4. Mengubah isian hari yang sudah ada wajib beralasan dan menyertakan nomor revisi; nilai lama disimpan.
5. Tim PPI menandai dicurigai; sistem membuat satu `TrxNosocomialInfection` jenis `SurgicalSiteInfection` berstatus `Suspected` dengan nomor dari allocator bersama.
6. Bila pasien keluar ruangan sebelum hari ke-15 berakhir, formulir berhenti (`StoppedOnDeparture`) dengan nomor hari saat itu; bila hari ke-15 lewat, formulir `Completed`.

**Contoh berangka.** Operasi selesai 1 Oktober pukul 14.00 WIB, sehingga hari ke-1 jatuh pada 2 Oktober. Tanda vital 3 Oktober (hari ke-2) mencatat 38,5 °C, sehingga indikator demam hari ke-2 bernilai "ya" (ambang ≥ 38 °C, gate G-20). Pasien keluar ruangan 6 Oktober, yaitu hari ke-6 − 2 + 1 = **hari ke-5**; formulir menjadi `StoppedOnDeparture`, dan isian baru ditolak `CLI-SSI-001`.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Belum ada versi `Approved` | Formulir tidak dibentuk; `GET form-readiness` mengembalikan peringatan |
| Formulir sudah berhenti | `422` `CLI-SSI-001` |
| Hari di luar 1–15 atau belum tiba | `422` `CLI-SSI-002` |
| Isian tidak sesuai definisi versi, termasuk suhu yang diketik | `422` `CLI-SSI-003` |
| Revisi berubah | `409` |
| Sudah ditandai dicurigai | `409` `CLI-SSI-004` |
| Satu formulir per kasus operasi | Unique `OprCaseId` dan kunci per kasus |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Dibentuk | `Active` | Sistem | Versi `Approved` ada |
| `Active` | Pasien keluar sebelum hari ke-15 | `StoppedOnDeparture` | Sistem (worker atau saat menyimpan) | `PhysicallyLeftAt` terisi |
| `Active` | Hari ke-15 berakhir | `Completed` | Sistem | — |
| Status apa pun | Tandai dicurigai | Status tetap; `NosocomialInfectionId` terisi | Tim PPI | Belum pernah ditandai |

**Jalur tidak normal.** Versi formulir tidak berganti di tengah jalan. Kegagalan worker pada satu putaran dicatat dan diulang pada putaran berikutnya.

**Hasil akhir.** Formulir surveilans dan register infeksi nosokomial terhubung; tidak ada kode modul Kamar Operasi yang diubah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.5, integrasi 9.3, backend 12.6.3 dan 12.12, `ClinicalInstrumentDefinitionEngine`, `CliClinicalInstrument`/`Version`, `TrxPatientVitalSign`, `OprCase`/`OprStatusHistory`, `NosocomialInfectionController`, `NumberSeriesAllocator` (transaksi dan `DbContext` sendiri), `HospitalTimeZone`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliSurgicalSiteSurveillance.cs` | Baru. Formulir per kasus operasi |
| `Areas/HealthServices/ClinicalManagement/Models/CliSurgicalSiteSurveillanceEntry.cs` | Baru. Isian per hari dengan cuplikan suhu |
| `Areas/HealthServices/ClinicalManagement/Models/CliSurgicalSiteSurveillanceEntryRevision.cs` | Baru. Nilai lama setiap koreksi isian |
| `Areas/HealthServices/ClinicalManagement/Enums/CliSurveillanceStatus.cs` | Baru |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliSurgicalSiteSurveillanceConfigurations.cs` | Baru. Unique `OprCaseId`, unique `(SurveillanceId, DayNumber)`, FK `Restrict` |
| `Areas/HealthServices/ClinicalManagement/DTOs/SurgicalSiteSurveillanceDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/CliSurgicalSiteSurveillanceService.cs` | Baru. Pembentukan, sinkron status, isian, ringkasan, tanda dicurigai, pembacaan suhu |
| `Areas/HealthServices/ClinicalManagement/Services/CliSurgicalSiteSurveillanceWorker.cs` | Baru. Interval `Clinical:SurgicalSiteSurveillance:WorkerIntervalMinutes` (bawaan 5) |
| `Areas/HealthServices/ClinicalManagement/Controllers/SurgicalSiteSurveillanceController.cs` | Baru. Enam endpoint |
| `Areas/HealthServices/ClinicalManagement/Services/NosocomialRecordNumberService.cs` | Baru. Nomor `NOS-yyyyMMdd-0001` lewat allocator bersama, menjembatani nomor legacy hari itu |
| `Areas/HealthServices/ClinicalManagement/Controllers/NosocomialInfectionController.cs` | `TOUCHED LEGACY`: pembuatan nomor Count+1 diganti layanan nomor di atas |
| `Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs` | `SurgicalSiteSurveillanceForm = 7` |
| `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` | Data awal `K12`: instrumen surveilans versi `Draft` tanpa isian suhu |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | DbSet jamak; registrasi service, layanan nomor, dan worker |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | Tiga tabel surveilans |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint API 8.5. Delta: endpoint tambahan `GET /form-readiness` untuk peringatan "formulir belum disahkan" pada daftar PPI (`UAT-RWF-28`) |
| Database | Tabel baru `CliSurgicalSiteSurveillance`, `CliSurgicalSiteSurveillanceEntry`, `CliSurgicalSiteSurveillanceEntryRevision`. Data awal `K12` dibuat seeder saat aplikasi berjalan, bukan lewat migration. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Resource baru `SurgicalSiteSurveillance` (`Read`, `Update`, `Review`) di modul `HEALTH_SERVICE_CLINICAL` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Surgical Site Surveillance

Base URL: `api/v1/health-services/clinical-management/surgical-site-surveillances`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar PPI dan daftar per episode | `SurgicalSiteSurveillance : Read` | `SurveillanceQuery` | `PagedResult<SurveillanceListItem>` |
| `GET` | `/form-readiness` | Apakah versi formulir sudah disahkan, beserta peringatannya | `SurgicalSiteSurveillance : Read` | — | `SurveillanceFormReadiness` |
| `GET` | `/{id}` | Formulir lengkap, isian harian, dan suhu per hari dari tanda vital | `SurgicalSiteSurveillance : Read` | — | `SurveillanceDetailResponse` |
| `PUT` | `/{id}/entries/{dayNumber}` | Mengisi hari ke-N; mengubah isian wajib beralasan dan berversi | `SurgicalSiteSurveillance : Update` | `PutSurveillanceEntryRequest` | `SurveillanceEntryResponse` |
| `PUT` | `/{id}/summary` | Kultur dan serologi | `SurgicalSiteSurveillance : Update` | `PutSurveillanceSummaryRequest` | `SurveillanceDetailResponse` |
| `POST` | `/{id}/flag-suspected` | Tim PPI menandai dicurigai | `SurgicalSiteSurveillance : Review` | `FlagSurveillanceSuspectedRequest` | `SurveillanceDetailResponse` (memuat `NosocomialInfectionId`) |

Kode status: `400` alasan atau isian tanda dicurigai tidak sah; `403` perawat tidak berwenang; `404` tidak ditemukan; `409` revisi/versi berubah atau `CLI-SSI-004`; `422` `CLI-SSI-001`, `CLI-SSI-002`, `CLI-SSI-003`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; nol warning di berkas baru |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | Tiga tabel surveilans di migration `20261005071042`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan statis atribut hak akses | Enam endpoint cocok `SurgicalSiteSurveillance` ↔ action | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — hari ke-1 | `HospitalTimeZone.TodayLocal(completedAt).AddDays(1)` | `PASS` | `CliSurgicalSiteSurveillanceService.ProcessAsync` |
| Verifikasi proses bisnis — suhu dari tanda vital dan tidak dapat diketik | Suhu maksimum dibaca dari `TrxPatientVitalSign`; definisi `K12` tanpa butir suhu, sehingga butir `temperature` yang diketik gagal validasi | `PASS` | `TemperatureAsync`, `ValidateResponsesAsync`; seeder `K12` |
| Verifikasi proses bisnis — keluar hari ke-5 | `StoppedOnDayNumber` = hari keluar − hari ke-1 + 1 | `PASS` | `SynchronizeStatusAsync` |
| Verifikasi format nomor nosokomial | Allocator membentuk `{prefix}-{yyyyMMdd}-{0001}` dengan transaksi dan `DbContext` sendiri | `PASS` | `NumberSeriesAllocator.cs:84-166` |
| Uji dengan kasus OK `Completed` di lingkungan uji dan UAT `UAT-RWF-18`/`28` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi dan worker tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Operasi selesai 1 Okt → hari ke-1 jatuh pada 2 Okt | Terpenuhi | `ProcessAsync` (`TodayLocal(...).AddDays(1)`) |
| 2. Versi formulir masih `Draft` → formulir tidak dibentuk dan peringatan tampil di daftar PPI | Terpenuhi | `Approved()` mensyaratkan `Approved` + pengesah; `ReadinessAsync` |
| 3. Suhu 38,5 °C hari ke-2 terbaca dari tanda vital dan tidak dapat diketik | Terpenuhi | `TemperatureAsync`; `FeverIndicator` = suhu ≥ 38 |
| 4. Tim PPI menandai → satu `TrxNosocomialInfection` `SurgicalSiteInfection` `Suspected`; tanda kedua → 409 `CLI-SSI-004` | Terpenuhi | `FlagSuspectedAsync` |
| 5. Pasien keluar hari ke-5 → `StoppedOnDeparture`; isian baru → 422 `CLI-SSI-001` | Terpenuhi | `SynchronizeStatusAsync`, `PutEntryAsync` |
| 6. Hari di luar 1–15 → 422 `CLI-SSI-002` | Terpenuhi | `PutEntryAsync` |
| 7. Isian tidak sesuai definisi versi → 422 `CLI-SSI-003` | Terpenuhi | `ValidateResponsesAsync` |
| 8. Satu formulir per kasus operasi | Terpenuhi | Unique `IX_CliSurgicalSiteSurveillance_OprCaseId` + kunci advisory per kasus |
| 9. Tidak ada kode modul Kamar Operasi yang diubah | Terpenuhi | `git status` tanpa berkas `OperatingRoomManagement` |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi dengan kasus OK di lingkungan uji | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Gerbang produksi G-21 terbuka:** isi formulir `K12` wajib ditinjau dan disahkan pemilik klinis atau komite PPI (versi `Approved` beserta nama pengesah) sebelum dipakai pasien sungguhan. Tanpa pengesahan, worker tidak membentuk formulir apa pun |
| Masalah yang diketahui | Pada hari penerapan, nomor nosokomial legacy memakai tanggal UTC sedangkan allocator memakai tanggal lokal; jembatan nilai minimum hanya berlaku pada alokasi pertama tiap tanggal |
| Risiko tersisa | Belum diuji dengan kasus OK sungguhan; worker belum terlihat berjalan di runtime |
| Perubahan sampingan | `NosocomialInfectionController` (legacy) kini memakai allocator bersama; perubahan disengaja agar dua jalur pembuat register tidak menghasilkan nomor ganda |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-189`; pengesahan formulir oleh komite PPI (G-21); UAT `UAT-RWF-18`/`28` |
