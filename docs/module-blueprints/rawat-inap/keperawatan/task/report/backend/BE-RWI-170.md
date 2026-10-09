# Laporan Perubahan Backend — `BE-RWI-170`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-170` |
| Judul | Monitoring transfusi per kantong (`K9` bagian transfusi) |
| Slice | `MVP-3` / `RWF-W7` (pemilihan kantong maju dari `MVP-4`) |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-170` |
| Trace | `FR-RWF-085`, `FR-RWF-093`; `RWI-DEC-203`, `RWI-DEC-209`; `INV-RWF-17`, `18`; `INT-RWF-11`; `AC-RWF-084`, `097`, `098`; `UAT-RWF-19` (bagian Clinical), `UAT-RWF-29` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.6; integrasi 9.4; backend 12.6.4, 12.7, 12.12 |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 1, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kedelapan acceptance criteria terpetakan ke source; build akhir `PASS`; tabel diterapkan ke database development. Gerbang produksi G-21/G-22 tetap terbuka |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`; `BbkBloodUnit` milik Bank Darah dibaca saja (`RWI-DEC-209`) |
| Prefix registry | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` (tiga model, tiga enum, service, controller, DTO) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`/`002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Bangsal tidak punya catatan monitoring transfusi di sistem. Perawat menulis nomor kantong dan tanda vital per titik ukur di kertas, sehingga kantong yang salah pasien tidak tertahan sistem dan titik ukur yang terlewat tidak terlihat.

## 2. Proses bisnis

**Tujuan.** Perawat memantau transfusi per kantong yang sudah diserahkan Bank Darah kepada pasien itu: empat titik ukur dengan tanda terlambat, hentikan, selesai, batal, dan catat reaksi.

**Pelaku.** Perawat bangsal berwenang pada episode (`TransfusionMonitoring : Read/Create/Update`).

**Langkah utama.**

1. Perawat membuka daftar kantong yang dapat dipilih: kantong `BbkBloodUnit` dengan penerima pasien itu, diserahkan sejak admisi episode, dan belum dipantau (monitoring yang dibatalkan tidak dihitung).
2. Perawat memulai monitoring satu kantong dengan waktu terima di bangsal dan waktu mulai transfusi. Nomor kantong dan komponen dibaca dari Bank Darah, tidak diketik.
3. Server membentuk empat titik ukur: sebelum transfusi (menit 0), menit 15, jam 1, dan jam 4 sesudah mulai.
4. Perawat mencatat tekanan darah, suhu, dan nadi per titik. Titik yang dicatat melewati batas waktu + toleransi wajib diberi keterangan terlambat.
5. Transfusi diselesaikan, dihentikan dengan alasan (titik sesudahnya bertanda dihentikan), atau dibatalkan karena salah pilih kantong sebelum ada titik ukur.
6. Reaksi dicatat kapan saja dan disimpan dengan status pemberitahuan `Pending`; pengirimannya ke Bank Darah dikerjakan `BE-RWI-171`.

**Contoh berangka.** Transfusi dimulai 10.00. Titik jam 1 jatuh pada 11.00 dengan toleransi 10 menit (`LateToleranceMinutes = 10`, gate G-22). Pengukuran pukul 11.08 tidak terlambat; pengukuran pukul 11.15 terlambat dan wajib memuat keterangan, misalnya "pasien sedang ke kamar mandi". Bila transfusi dihentikan pukul 10.40 karena menggigil, titik jam 1 dan jam 4 bertanda dihentikan dan tidak dapat diisi.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Kantong belum diserahkan kepada pasien ini, termasuk pemanggilan langsung dengan id kantong pasien lain | `422` `CLI-TRF-001` |
| Kantong yang sama dipantau dua kali | `409` `CLI-TRF-002` |
| Titik terlambat tanpa keterangan | `422` `CLI-TRF-003` |
| Titik sesudah transfusi dihentikan atau sesudah selesai/dibatalkan | `422` `CLI-TRF-004` |
| Pembatalan sesudah ada titik ukur atau reaksi | `422` `CLI-TRF-005` |
| Mengubah titik yang sudah tercatat | Alasan koreksi wajib |
| Volume darah tidak disalin ke cairan | Tidak ada entri `CliFluidBalanceEntry` dari modul ini |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Mulai | `InProgress` | `TransfusionMonitoring : Create` | Kantong sah untuk pasien |
| `InProgress` | Selesai | `Completed` | `TransfusionMonitoring : Update` | Waktu tidak mendahului titik tercatat |
| `InProgress` | Hentikan dengan alasan | `Stopped` | `TransfusionMonitoring : Update` | Alasan wajib |
| `InProgress` | Batal dengan alasan | `Cancelled` | `TransfusionMonitoring : Update` | Belum ada titik ukur maupun reaksi |

**Jalur tidak normal.** Dua perawat memulai monitoring kantong yang sama bersamaan diantre kunci advisory per kantong dan unique index; yang kedua mendapat `CLI-TRF-002`. Pasien yang sudah keluar ruangan tidak menerima titik ukur baru.

**Hasil akhir.** Monitoring per kantong lengkap dengan titik ukur, status keterlambatan, dan reaksi yang siap diteruskan ke Bank Darah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.6, integrasi 9.4, backend 12.6.4 dan 12.12, `BbkBloodUnit` (`IssuedToPatientId`, `IssuedAt`, `PmiBagNumber`, `BloodComponent`), `NursingEpisodeWriteGuard`, dan persetujuan Bank Darah `RWI-DEC-209`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliTransfusionMonitoring.cs` | Baru. Monitoring per kantong |
| `Areas/HealthServices/ClinicalManagement/Models/CliTransfusionMonitoringPoint.cs` | Baru. Empat titik ukur |
| `Areas/HealthServices/ClinicalManagement/Models/CliTransfusionReaction.cs` | Baru. Reaksi dengan status pemberitahuan |
| `Areas/HealthServices/ClinicalManagement/Enums/CliTransfusionEnums.cs` | Baru. Status monitoring, jenis titik, status pemberitahuan |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliTransfusionMonitoringConfigurations.cs` | Baru. Unique kantong untuk monitoring tidak batal, unique `(MonitoringId, PointType)`, FK `Restrict` ke `BbkBloodUnit` |
| `Areas/HealthServices/ClinicalManagement/DTOs/TransfusionMonitoringDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/CliTransfusionMonitoringService.cs` | Baru. Kantong terpilih, mulai, titik ukur, reaksi, hentikan, selesai, batal |
| `Areas/HealthServices/ClinicalManagement/Controllers/TransfusionMonitoringController.cs` | Baru. Delapan endpoint |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | DbSet jamak; registrasi service |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | Tiga tabel transfusi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delapan endpoint API 8.6. Delta: `stop`, `complete`, `cancel` menerima `PATCH` sesuai kontrak dan alias `POST` sesuai standar endpoint transaksi; respons menambah `LateToleranceMinutes` |
| Database | Tabel baru `CliTransfusionMonitoring`, `CliTransfusionMonitoringPoint`, `CliTransfusionReaction`; FK ke `BbkBloodUnit` tanpa menulis tabel Bank Darah. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Resource baru `TransfusionMonitoring` (`Read`, `Create`, `Update`) di modul `HEALTH_SERVICE_CLINICAL` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Transfusion Monitoring

Base URL: `api/v1/health-services/clinical-management/transfusion-monitorings`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Monitoring per episode | `TransfusionMonitoring : Read` | Query `episodeId` | `List<TransfusionMonitoringResponse>` |
| `GET` | `/selectable-units` | Kantong yang sudah diserahkan kepada pasien dan belum dipantau | `TransfusionMonitoring : Read` | Query `episodeId` | `List<SelectableBloodUnit>` |
| `POST` | `/` | Mulai monitoring satu kantong | `TransfusionMonitoring : Create` | `StartTransfusionMonitoringRequest` | `TransfusionMonitoringResponse` (`201`) |
| `PUT` | `/{id}/points/{pointType}` | Catat atau koreksi titik ukur | `TransfusionMonitoring : Update` | `PutTransfusionPointRequest` | `TransfusionPointResponse` |
| `POST` | `/{id}/reactions` | Catat reaksi; Bank Darah diberi tahu | `TransfusionMonitoring : Update` | `RecordTransfusionReactionRequest` | `TransfusionReactionResponse` (`201`) |
| `PATCH` / `POST` | `/{id}/stop` | Hentikan transfusi | `TransfusionMonitoring : Update` | `StopTransfusionRequest` | `TransfusionMonitoringResponse` |
| `PATCH` / `POST` | `/{id}/complete` | Selesai | `TransfusionMonitoring : Update` | `CompleteTransfusionRequest` | `TransfusionMonitoringResponse` |
| `PATCH` / `POST` | `/{id}/cancel` | Batal karena salah pilih kantong | `TransfusionMonitoring : Update` | `CancelClinicalMeasurementRequest` | `TransfusionMonitoringResponse` |

Kode status: `400` isian tidak sah; `403` perawat tidak berwenang; `404` tidak ditemukan; `409` `CLI-TRF-002`, monitoring sudah diakhiri, atau revisi berubah; `422` `CLI-TRF-001`, `003`, `004`, `005`, atau pasien sudah keluar ruangan.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; nol warning di berkas baru |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | Tiga tabel transfusi di migration `20261005071042`; unique kantong terfilter `"Status" <> 4`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah dan isi migration |
| Pemeriksaan statis atribut hak akses | Delapan endpoint cocok `TransfusionMonitoring` ↔ action | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — kantong pasien lain | Filter `IssuedToPatientId = PatientId` dan `IssuedAt ≥ AdmittedAt` pada daftar dan saat mulai → `CLI-TRF-001` | `PASS` | `CliTransfusionMonitoringService.Selectable`, `StartAsync` |
| Verifikasi proses bisnis — titik jam 1 terlambat | `MeasuredAt > DueAt + toleransi` tanpa keterangan → `CLI-TRF-003` | `PASS` | `PutPointAsync` |
| Verifikasi proses bisnis — dihentikan, batal sesudah titik | `CLI-TRF-004`; `CLI-TRF-005` | `PASS` | `PutPointAsync`, `EndAsync` |
| Uji dengan kantong yang sudah diserahkan di lingkungan uji dan UAT `UAT-RWF-29` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Hanya kantong ber-`IssuedToPatientId` pasien itu, diserahkan sejak admisi, dan belum dipantau yang tampil; kantong pasien lain tidak tampil dan pemanggilan langsung → 422 `CLI-TRF-001` | Terpenuhi | `Selectable`, `StartAsync` |
| 2. Kantong yang sama dua kali → 409 `CLI-TRF-002` | Terpenuhi | `StartAsync`; unique `IX_CliTransfusionMonitoring_BloodUnitId` |
| 3. Titik 1 jam terlewat → bertanda terlambat; isian tanpa keterangan → 422 `CLI-TRF-003` | Terpenuhi | `PutPointAsync`; tanda terlambat juga dihitung pada respons titik yang belum diisi |
| 4. Titik sesudah transfusi dihentikan → 422 `CLI-TRF-004` | Terpenuhi | `PutPointAsync` |
| 5. Batal sesudah ada titik ukur → 422 `CLI-TRF-005` | Terpenuhi | `EndAsync` |
| 6. Reaksi tersimpan dengan status pemberitahuan `Pending` | Terpenuhi | `CliTransfusionReaction.NoticeDelivery` bawaan `Pending`, disimpan sebelum pengiriman |
| 7. Nomor kantong tidak dapat diketik | Terpenuhi | Request tanpa field nomor; `PmiBagNumber` dibaca dari `BbkBloodUnit` |
| 8. Volume darah tidak disalin ke cairan | Terpenuhi | Service tidak membuat `CliFluidBalanceEntry` |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi dengan kantong di lingkungan uji | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Gerbang produksi G-21 (titik ukur) dan G-22 (toleransi 10 menit)** wajib disahkan pemilik klinis sebelum dipakai pasien sungguhan; toleransi dapat diubah lewat konfigurasi `Clinical:TransfusionMonitoring:LateToleranceMinutes` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Belum diuji dengan data penyerahan Bank Darah sungguhan |
| Perubahan sampingan | DbSet dinamai jamak pada review |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `BE-RWI-171` (sudah dikerjakan bersama); `FE-RWI-190`; pengesahan G-21/G-22 |
