# Laporan Perubahan Backend — `BE-PAT-MIG-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PAT-MIG-003` |
| Judul | RSMMC Normal Patient Identifier Reservation Guard |
| Slice | S3 — Guard reservasi nomor pasien normal |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md), kartu `BE-PAT-MIG-003` |
| Trace | `PAT-DEC-042`, `054`, `055`, `057`, `064`, `065`, `066`, `067`, `068`; `PAT-GAP-002`–`008`; requirement `PAT-MIG3-AC-01`–`PAT-MIG3-AC-14` |
| Contract version | `contracts/api-contract.md` **`1.2.2`** — `approved` 16 Oktober 2026 (`PAT-DEC-068`); bagian B14–B14.3 dan B15 |
| Dependency | Tidak ada task prasyarat. Keputusan `PAT-DEC-033`, `PAT-DEC-067`, `PAT-DEC-068` `approved` |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas aplikasi diubah 4–8 (1), logika bisnis kompleks (2), perubahan perilaku endpoint yang ada (1), perilaku persistence dan serialisasi (1), berkaitan dengan keamanan (1), alur pendaftaran kiosk dan admin (2), satu repository (0) |
| Task mode | `BACKEND` — pengerjaan pertama dan *source reconciliation / review fix pass* 16 Oktober 2026; review pra-provider dan perbaikan harness provider-real 16–17 Oktober 2026 |
| Target tulis | Repository `NewQuilvianSystemBackend`, working tree `NewQuilvianSystemBackendIntegerationBranch`, branch `QuilvianIntegrationBackend`. Tidak ada *commit* atau *push* |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | **Saat ini:** `0c6e314594766fe7e55a833a0ce88095dc665a7c` (`QuilvianIntegrationBackend`) — perubahan belum di-commit. Riwayat: pengerjaan pertama di atas `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` |
| Tanggal | 16 Oktober 2026; diperbarui 17 Oktober 2026 (bukti provider-real) |
| Status | 🟡 **SIAP REVIEW PEMILIK — seluruh acceptance criteria tahap build terpenuhi, termasuk bukti PostgreSQL provider-real.** Di atas Integration `0c6e3145`: build, test SQLite (`143`/`0`/`7`), regresi `BE-PAT-MIG-001` (`73`/`73`), QBE `Strict`, pemeriksaan model EF lulus; uji provider-real PostgreSQL 15.15 **`10`/`10` PASS** (dijalankan pemilik 17 Oktober 2026). **Belum:** review source leader/pemilik (butir DoD), lalu `PAT-MIG3-AC-12` yang memang milik runtime `PAT-GATE-005` |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `PatientManagement` |
| Submodule | `MasterData` |
| Pemilik / prefix registry | `PatientManagement operational`, prefix `Pat`, `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 24) |
| Keberlakuan | `NEW CODE`: `PatientIdentifierAllocationService.cs`, `RsmmcIdentifierReservationProfile.cs`, `RsmmcMigrationReservationGuardOptions.cs`, `RsmmcReservationGuardDtos.cs`. `TOUCHED LEGACY`: `PatientController.cs`, `Program.cs` |
| Entity persisted baru | Tidak ada — tanpa entri registry baru, EF migration, maupun perubahan skema |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-CODE-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-DTO-001` |
| Catatan kontrak engineering | Task ini sengaja **tidak** memakai provider number-series bersama: keputusan pemilik `PAT-DEC-054`/`064` menuntut "nomor kosong pertama ≥ lantai", bukan penghitung monotonik. Alokasi tetap di module service, diserialkan dengan kunci database, dan dijaga indeks unik |
| Governance yang dibaca | `AGENTS.md`, `CLAUDE.md`, `rules/backend/TASK_RULES.md`, `TEST_POLICY.md`, `REPORT_TEMPLATE.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry modul, `tooling/qbe/README.md` |
| Automated test | **Diminta pemilik secara eksplisit** ("Automated tests are mandatory"), sehingga pengecualian `TEST_POLICY` bagian 3 berlaku. Ditulis di project yang sudah ada `Tests/QuilvianSystemBackend.PatientManagementTests/`; tanpa project, paket, entri `.sln`, atau workflow CI baru |

---

## 1. Masalah yang diperbaiki

Target preview migrasi RSMMC sudah "memesan" `PatientCode` `PAT-RSMMC-00742` sampai
`PAT-RSMMC-705249` dan MRN numerik sampai `00-89-01-87`. Pembangkit nomor pendaftaran pasien normal
tidak tahu pemesanan itu dan selalu memilih nomor kosong terkecil. Pendaftaran berikutnya di Staging
akan mendapat `PAT-RSMMC-00742` dan `00-00-00-27` — keduanya milik pasien RSMMC kanonik `legacy_pid`
-667995 (`PAT-FACT-072`, `073`, `089`). Pasien migrasi itu kelak tidak dapat dibuat, dan QR pasien
normal ditulis ke folder `00-00-00-27` milik riwayat Pilot. Pembangkit itu juga tinggal di controller
sebagai method `private`, sehingga tidak ada satu titik service yang dapat dijaga.

---

## 2. Proses bisnis

**Tujuan.** Selama kampanye migrasi RSMMC, pendaftaran pasien normal di Staging tidak pernah memakai
nomor yang dicadangkan migrasi. Di Produksi dan environment lain penomoran tidak berubah.

**Pelaku.** Petugas pendaftaran (`POST /`), kiosk (`POST /kiosk`), dan admin (`POST /admin`). Pemilik
mengisi `RsmmcMigrationReservationGuard__Enabled` saat deployment dan memakai endpoint pemeriksa untuk
`PAT-GATE-005`.

**Langkah pendaftaran normal, berurutan.**

1. Request divalidasi seperti sebelumnya.
2. Transaksi database dibuka (`PatientController.cs` baris 538, tidak berubah).
3. Controller memanggil `PatientIdentifierAllocationService.AllocateAsync()` (baris 547).
4. Service menilai guard: environment persis `Staging` (ordinal) dan konfigurasi `Enabled`.
5. Menurut hasil penilaian:
   - **Staging persis, `Enabled = true`:** ambil kunci `pg_advisory_xact_lock(1346458697, 1313821261)`, lalu pilih suffix `PatientCode` pertama yang belum dipakai ≥ 705.250 dan MRN numerik pertama yang belum dipakai ≥ 890.188.
   - **Staging persis, `Enabled` bernilai `false`, hilang, salah bentuk, atau tidak terbaca:** tolak. Controller me-*rollback* dan mengembalikan `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` — sebelum insert pasien, penulisan QR, dan penulisan foto.
   - **Bukan Staging persis:** algoritma lama apa adanya — nomor kosong terkecil mulai 1, tanpa kunci baru, tanpa membaca konfigurasi guard.
6. QR dan foto ditulis, pasien disimpan, transaksi di-*commit* (tidak berubah). Kunci lepas saat *commit*.

**Contoh berangka.**

| Keadaan data | Staging, `Enabled = true` | Produksi |
| --- | --- | --- |
| Belum ada kode ≥ 705.250 dan MRN ≥ 890.188; kode tertinggi `PAT-RSMMC-705249` | `PAT-RSMMC-705250`, `00-89-01-88` | Nomor kosong terkecil, misalnya `PAT-RSMMC-00742`, `00-00-00-27` bila keduanya kosong |
| `705250`, `705251`, `705253` dan MRN `890188`, `890190` sudah dipakai | `PAT-RSMMC-705252`, `00-89-01-89` | Tidak berubah dari dulu |
| `PAT-RSMMC-705250` milik pasien yang ditandai terhapus | `PAT-RSMMC-705251` — nomor pasien terhapus tetap terpakai karena indeks unik tanpa syarat | — |
| Ada MRN berformat lain `00890188` | `00-89-01-89` — angka yang sama dalam format lain dihitung terpakai | — |

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Staging, `Enabled = false` | `503`, `guardState = DISABLED`; tidak ada pasien, QR, maupun foto |
| Staging, bagian atau kunci konfigurasi tidak ada | `503`, `guardState = CONFIG_MISSING` |
| Staging, nilai `yes`/`1`/kosong, atau penyedia konfigurasi gagal dibaca | `503`, `guardState = CONFIG_MALFORMED` |
| Dua pendaftaran bersamaan di Staging | Yang kedua menunggu kunci, lalu mendapat nomor berikutnya |
| MRN melewati 99.999.999 | Galat yang sama dengan pembangkit lama → `500` |
| `staging`, `STAGING`, `Production`, `Development` | Penomoran lama — guard tidak berlaku |

**Hasil akhir.** Sesudah deploy ke Staging dengan `Enabled = true`, pendaftaran normal mulai dari
`PAT-RSMMC-705250` / `00-89-01-88`. `GET /admin/migration/rsmmc-reservation-guard` melaporkan
`overallGuardReady = true`; barulah pemilik boleh mencabut pembekuan pendaftaran (`PAT-GATE-005`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `CLAUDE.md`, `rules/backend/*` di atas, kontrak engineering, registry.
- Blueprint: `00-interview-decisions.md` revision `5`, `contracts/api-contract.md` `1.2.2`, kartu `BE-PAT-MIG-003`, `requirement-traceability.md` bagian 6.
- Source pada `0c6e3145`: `PatientController.cs`, `MstPatient.cs`, `MstPatientConfiguration.cs`, `PatientDtos.cs`, `ApplicationDbContext.cs` (diff upstream), `Program.cs`, `ApplicationDbContextFactory.cs`, `PatientQrPayloadBuilder.cs`, `RsmmcPilotEnvironmentGate.cs`, berkas enum pasien, `LoggerService.cs`, atribut akses, `ApiResponse.cs`.
- Impact scan: seluruh `*.cs` dan `*.sql` untuk `new MstPatient`, `Set<MstPatient>().Add`, pembangkit nomor, `INSERT INTO "MstPatient"`, dan seluruh pemakaian `pg_advisory_*lock` pada `a52b3c98` dan `0c6e3145`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientIdentifierAllocationService.cs` | **Baru.** Satu-satunya allocator `PatientCode`/MRN pasien normal: `AllocateAsync` (atomik untuk kedua nomor), `CheckAsync` (pemeriksa baca-saja), mode guard berlantai di bawah kunci database dua bagian (baris 116), dan mode lama yang dipindah apa adanya dari controller |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcIdentifierReservationProfile.cs` | **Baru.** Lantai 705.250 / 890.188, awalan, konstanta kunci `AllocationLockNamespace` (baris 107) dan `AllocationLockId` (baris 113), kode galat, format, dan penilaian guard `Evaluate` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcMigrationReservationGuardOptions.cs` | **Baru.** Konfigurasi bertipe `RsmmcMigrationReservationGuard` dengan `bool? Enabled` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcReservationGuardDtos.cs` | **Baru.** `RsmmcReservationGuardStatusResponse` |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | `CreatePatient` memakai allocator (baris 544–572) dan menolak `503` lewat `RejectPatientCreateByReservationGuardAsync` (baris 829); `[ProducesResponseType(503)]` pada `CreatePatient` dan `CreatePatientForAdmin`; endpoint `GetRsmmcReservationGuardStatus` (baris 1168), diletakkan sebelum endpoint Pilot supaya urutan label aksi `Update` tidak berubah; konstruktor menerima service; **dihapus**: `GeneratePatientCodeAsync`, `GenerateRunningCodeAsync`, `GenerateMedicalRecordNumberAsync`, `CodePrefix`, `CodeNumberLength`, dan dua helper MRN yang hanya dipakai pembangkit itu |
| `Program.cs` | Registrasi DI `AddScoped<PatientIdentifierAllocationService>()` (3 baris) |
| `.gitignore` | Pengecualian sempit `Tests/QuilvianSystemBackend.PatientManagementTests/` — bagian 8 |
| `Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/TestDatabase.cs` | Model uji SQLite melepas **dua** CHECK constraint Finance yang disebut namanya (`CK_MstPayrollDeductionCutover_Period`, `CK_FinPayrollDeductionBatch_Period`) — bagian 7.1. *Versi pertama pass rekonsiliasi memakai deteksi generik operator regex; dipersempit pada review pra-provider* |
| `Tests/…/Infrastructure/ReservasiHarness.cs` | **Baru.** Harness SQLite + konfigurasi guard + controller dan service asli; salinan acuan pembangkit lama `a52b3c98` |
| `Tests/…/Infrastructure/PostgresSekaliPakai.cs` | **Baru.** Harness PostgreSQL 15 sekali pakai dan atribut `FactPostgresSungguhan`/`TheoryPostgresSungguhan`. Basis data bukti dibuat `CREATE DATABASE "pat_proof_<guid>" WITH TEMPLATE template0` — bagian 7.2 |
| `Tests/…/ReservasiNomor/PenilaianGuardReservasiTests.cs` | **Baru.** Matriks environment × konfigurasi; konstanta kunci |
| `Tests/…/ReservasiNomor/AlokasiNomorReservasiTests.cs` | **Baru.** Lantai, celah, pasien terhapus, format berbeda, gagal tertutup, pembanding pembangkit lama, pemeriksa |
| `Tests/…/ReservasiNomor/JalurPendaftaranReservasiTests.cs` | **Baru.** Ketiga rute lewat action controller asli, QR, foto, `503`, endpoint pemeriksa, izin |
| `Tests/…/ReservasiNomor/AlokasiPostgresSungguhanTests.cs` | **Baru.** Bukti provider-real `PAT-MIG3-AC-13` — **10/10 PASS** pada PostgreSQL 15.15 |
| `docs/module-blueprints/patient-management/**` | Amandemen `PAT-DEC-067` (revision `4`, kontrak `1.2.1`) dan `PAT-DEC-068` (revision `5`, kontrak `1.2.2`), `PAT-SRC-030`, `PAT-STALE-009`, laporan ini, status roadmap, traceability, manifest |

**Tidak berubah:** `MstPatient.cs`, `MstPatientConfiguration.cs` (termasuk `HasSentinel(true)`),
`ApplicationDbContext.cs`, `Migrations/**`, `appsettings*`, seluruh `RsmmcPilot*`,
`QuilvianSystemBackend.sln`, `.csproj` test, project test lain, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /`, `/kiosk`, `/admin`: request dan respons sukses **tidak berubah**; tambah penolakan `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` hanya di Staging persis. Baru: `GET /admin/migration/rsmmc-reservation-guard` (kontrak `1.2.2` B14.2, dikonfirmasi pemilik `PAT-DEC-068`). Nama berkas allocator berbeda dari rencana kartu (`PatientIdentifierAllocator.cs` → `PatientIdentifierAllocationService.cs`, mengikuti saran pemilik) |
| Database | Tidak ada perubahan skema, entity, konfigurasi EF, maupun migration. `dotnet ef migrations has-pending-model-changes`: "No changes have been made to the model since the last migration." Tidak ada perintah database yang dijalankan terhadap basis data mana pun |
| Keamanan/Auth | Otorisasi rute pendaftaran tidak diubah (`KioskRead` dan `Patient : Create` tetap). Perlindungan nomor berada **di bawah** otorisasi rute. Endpoint pemeriksa: `[Authorize]` controller + `[AccessAction("Update")]` + `[AccessPermission("Patient", "Update")]`, khusus Staging persis. Log baru tidak memuat nama, NIK, telepon, atau alamat |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pasien baru — nomor dari allocator; di Staging persis tanpa guard sah ditolak `503` | Login + policy `KioskRead` |
| `POST` | `/kiosk` | Sama, dari kiosk | Login + policy `KioskRead` |
| `POST` | `/admin` | Sama, dari layar admin; meneruskan ke `CreatePatient` | `Patient : Create` |
| `GET` | `/admin/migration/rsmmc-reservation-guard` | Melaporkan keadaan guard dan nomor yang **akan** diberikan, tanpa mencadangkan apa pun; `403` di luar Staging persis | `Patient : Update` |

**Kontrak final endpoint pemeriksa (`PAT-DEC-068`).**

| Syarat pemilik | Pemenuhan |
| --- | --- |
| Khusus `Staging` persis | `EvaluateGuard().IsExactStaging`; selain itu `403` / `RSMMC_RESERVATION_GUARD_ENVIRONMENT_NOT_ALLOWED` sebelum membaca tabel |
| Dijaga `Patient : Update` | `[AccessAction("Update", …)]` + `[AccessPermission("Patient", "Update")]`; diuji `Pemeriksa_RuteDanIzinSesuaiKontrak` |
| Baca-saja, nol mutasi database | `CheckAsync` hanya `SELECT` `AsNoTracking`, tanpa transaksi dan tanpa kunci; diuji jumlah pasien, *change tracker*, dan transaksi tidak berubah |
| Nol mutasi filesystem | Tidak memanggil penyimpanan QR/foto maupun `GetFileStoragePaths`; diuji jumlah berkas unggahan tetap 0 |
| Tanpa mencadangkan nomor | Tanpa kunci dan tanpa insert; `Pemeriksa_HasilnyaSamaDenganAlokasiBerikutnya` |
| Tanpa data pribadi | Keluaran hanya metadata di bawah |
| Hanya metadata `PAT-GATE-005` | `environment`, `guardState`, `guardEnabled`, `guardConfigurationValid`, lantai kode/MRN (angka dan format), `nextPatientCodeNumeric`/`Formatted`, `nextMrnNumeric`/`Formatted`, `patientCodeFloorSatisfied`, `mrnFloorSatisfied`, `overallGuardReady`, `checkedAtUtc` |

**Semantik database pemeriksa — review pra-provider 16 Oktober 2026.** Pemeriksa **membaca** database
untuk menghitung nomor bebas yang sebenarnya saat ini; ia tidak sekadar mengulang konstanta lantai.
Rumusan "tanpa menyentuh database" pada ringkasan sebelumnya keliru dan dikoreksi di sini.

| Perilaku | Pemeriksa | Bukti source |
| --- | --- | --- |
| Baca database | **Ya** — dua query `SELECT` `AsNoTracking` pada `MstPatient` | `CheckAsync` (`PatientIdentifierAllocationService.cs` baris 153) memanggil `LoadPatientCodeNumbersAtOrAboveFloorAsync` (baris 222) dan `LoadMedicalRecordNumbersAtOrAboveFloorAsync` (baris 263), lalu `FirstUnusedAtOrAbove` (baris 299) — tiga fungsi yang **sama persis** dengan yang dipakai `AllocateAsync` sesudah mengambil kunci |
| Tulis database | Tidak | Tidak ada `Add`, `SaveChanges`, `ExecuteSql`, maupun transaksi pada jalur pemeriksa |
| Advisory lock | Tidak | Satu-satunya `pg_advisory_xact_lock` ada di `AllocateAsync` baris 115–121, sesudah syarat transaksi; `CheckAsync` tidak melewatinya |
| Mencadangkan nomor | Tidak | Tidak ada insert; nomor yang dilaporkan baru terpakai bila pendaftaran sungguhan menyimpannya |
| Baca/tulis filesystem | Tidak | Jalur pemeriksa tidak memanggil penyimpanan QR/foto maupun `GetFileStoragePaths`. Catatan jujur: seperti setiap request HTTP, Serilog aplikasi menulis log request (`UseSerilogRequestLogging`) dan satu baris info `Patient.RsmmcReservationGuard.Check` ke sink konsol dan berkas log bergulir — infrastruktur log global, bukan mutasi data atau berkas bisnis |

Contoh: bila `PAT-RSMMC-705250` dan `00-89-01-88` sudah ada, pemeriksa melaporkan
`PAT-RSMMC-705251` dan `00-89-01-89` (`AlokasiNomorReservasiTests.Pemeriksa_StagingAktif_SiapDanTidakMenulis`);
nilai yang dilaporkan sama dengan hasil alokasi berikutnya (`Pemeriksa_HasilnyaSamaDenganAlokasiBerikutnya`).
Pada PostgreSQL, `Pemeriksa_BacaSajaPadaPostgres` memastikan tidak ada advisory lock terpegang — **PASS** pada PostgreSQL 15.15, 17 Oktober 2026.
Karena itu `PAT-GATE-005` membuktikan kesiapan database yang sebenarnya, bukan hanya kesiapan konfigurasi.

---

## 5. Verifikasi

Seluruh baris di bawah dijalankan di atas Integration `0c6e3145` sesudah rekonsiliasi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false --no-incremental` | `0 Error(s)`, `286 Warning(s)`; **tidak ada** warning dari berkas yang diubah | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests` (seluruh project) | Lulus `143`, gagal `0`, dilewati `7`, total `150` | `PASS` | Keluaran perintah |
| Hanya test baru (`FullyQualifiedName~ReservasiNomor`) | Lulus `70`, gagal `0`, dilewati `7` | `PASS` | Keluaran perintah |
| Regresi `BE-PAT-MIG-001` (`FullyQualifiedName!~ReservasiNomor`) | Lulus `73`, gagal `0` | `PASS` | Keluaran perintah |
| Provider-real, run normal tanpa server (`--filter Kategori=ProviderReal`) | Dilewati `7` — satu entri per method | `NOT RUN` — perilaku yang dimaksud | Alasan skip: "Bukti PostgreSQL sungguhan tidak diminta …" |
| Provider-real, `QUILVIAN_PG_PROOF_REQUIRED=1`, tanpa server | Gagal `10`, lulus `0` — "… QUILVIAN_PG_PROOF_CONNECTION tidak diisi" | `EXISTING / ENVIRONMENT ISSUE` — riwayat 16 Oktober 2026 | Membuktikan uji **gagal**, tidak dilewati diam-diam, saat bukti diminta |
| Provider-real percobaan pertama pemilik, PostgreSQL 15.15 | Seluruh 10 kasus gagal saat persiapan: `55006 source database "template1" is being accessed by other users` — bukan galat allocator | `EXISTING / ENVIRONMENT ISSUE` | Dilaporkan pemilik; diperbaiki harness bagian 7.2 |
| **Provider-real final, PostgreSQL 15.15, `QUILVIAN_PG_PROOF_REQUIRED=1`, `--filter "Kategori=ProviderReal"`** | **Total `10`, lulus `10`, gagal `0`, dilewati `0`** | **`PASS`** | Dijalankan pemilik di terminalnya terhadap server PostgreSQL **15.15** sekali pakai (basis data pemeliharaan `postgres`, role `quilvian_patmig003_proof`: `CREATEDB`, bukan `SUPERUSER`, bukan `CREATEROLE`), 17 Oktober 2026, dengan `QUILVIAN_PG_PROOF_REQUIRED=1`. Ringkasan diserahkan pemilik; agent tidak melihat keluaran mentahnya dan tidak memegang connection string |
| Pembersihan basis data bukti | Query read-only di `postgres`: `SELECT datname FROM pg_database WHERE datname LIKE 'pat\_proof\_%'` → **0 baris** | `PASS` | Dilaporkan pemilik, 17 Oktober 2026 |
| Konfirmasi ulang agent sesudah perbaikan harness, 17 Oktober 2026 | Suite normal `143`/`0`/`7`; test baru `70`/`0`/`7`; regresi `BE-PAT-MIG-001` `73`/`0`; QBE `Strict` 13 berkas `PASS`; `has-pending-model-changes` tanpa perubahan; `Migrations/` tanpa berkas baru | `PASS` | Keluaran perintah agent |
| QBE `Strict` *working tree* | 13 berkas (6 aplikasi, 7 test); `VIOLATION 0`, `REVIEW 0`; `Final result: PASS` | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah; *factory* design-time tidak membuka koneksi |
| Review pra-provider: penyaring `TestDatabase.cs` dipersempit ke dua constraint bernama, lalu `dotnet test` dan QBE diulang | Lulus `143` / gagal `0` / dilewati `7`; test baru `70`, regresi `BE-PAT-MIG-001` `73`; QBE `Strict` 13 berkas `PASS` | `PASS` | Keluaran perintah. Tidak ada source aplikasi yang berubah, sehingga build aplikasi tidak diulang secara terpisah (project aplikasi ikut dikompilasi `dotnet test`) |
| Review diff/scope | Hanya berkas pada bagian 3.2; tidak ada `Migrations/`, `Models/`, `Repositories/`, `appsettings*`, `.sln` | `PASS` | `git status --short`, `git diff --stat` |

`AUTOMATED TEST: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests — PASS (143 lulus, 0 gagal, 7 dilewati)`

`AUTOMATED TEST PROVIDER-REAL: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter Kategori=ProviderReal — PASS (10 lulus, 0 gagal, 0 dilewati; PostgreSQL 15.15; dijalankan pemilik 17 Oktober 2026)`

Uji manual: `NOT FEASIBLE` — aplikasi tidak dijalankan dan endpoint tidak dipanggil (larangan task).

**Tidak dijalankan:** deployment, pemanggilan endpoint, dan uji runtime Staging (di luar wewenang — `PAT-GATE-005`).

### 5.1 Inventaris uji provider-real

Seluruhnya di `ReservasiNomor/AlokasiPostgresSungguhanTests.cs`, trait `Kategori=ProviderReal`.

| No | Method | Jenis | Kasus | Skenario pemilik | Hasil 17 Oktober 2026 |
| ---: | --- | --- | ---: | --- | --- |
| 1 | `A_B_StagingAktif_LantaiDanKunciDatabase` | `Fact` | 1 | A, B, dan kunci `(1346458697, 1313821261)` terpegang lalu lepas | `PASS` — lantai `PAT-RSMMC-705250` / `00-89-01-88`; kunci `(1346458697, 1313821261)` terpegang lalu lepas sesudah rollback |
| 2 | `E_StagingAktif_CelahDiAtasLantai` | `Fact` | 1 | E | `PASS` — nomor kosong pertama di atas lantai, termasuk angka sama dalam format lain |
| 3 | `C_D_Bersamaan_TidakAdaKodeAtauMrnGanda` | `Fact` | 1 | C, D — delapan pendaftaran bersamaan | `PASS` — delapan pendaftaran bersamaan memperoleh kode dan MRN berbeda, berurutan, tanpa ganda |
| 4 | `F_G_H_StagingTidakSah_GagalTertutupTanpaKunci` | `Theory` × 3 baris: `"false"`, `null`, `"yes"` | 3 | F, G, H | `PASS` × 3 — `false`, hilang, `yes` gagal tertutup tanpa kunci dan tanpa pasien baru |
| 5 | `I_BukanStaging_SamaDenganPembangkitLamaDanTanpaKunci` | `Theory` × 2 baris: `"Production"`, `"Development"` | 2 | I | `PASS` × 2 — Production dan Development sama dengan pembangkit lama, tanpa kunci |
| 6 | `IndeksUnikTetapJaringTerakhir` | `Fact` | 1 | Indeks unik `PatientCode` dan MRN (`QBE-CODE-004`) | `PASS` — kode ganda dan MRN ganda ditolak `23505` |
| 7 | `Pemeriksa_BacaSajaPadaPostgres` | `Fact` | 1 | Pemeriksa baca-saja tanpa kunci | `PASS` — pemeriksa membaca nomor bebas sebenarnya tanpa kunci dan tanpa menulis |
| | **Jumlah** | 7 method | **10** | | **10/10 `PASS`** |

**Mengapa "7 dilewati" dan "10 kasus".** Pada run normal tanpa server, atribut menandai setiap method
sebagai *skip*, dan xUnit melaporkan `Theory` yang di-*skip* sebagai **satu** entri tanpa memecah baris
datanya. Hasilnya 7 entri dilewati. Dengan `QUILVIAN_PG_PROOF_REQUIRED=1`, `Theory` dipecah per baris
data, sehingga `dotnet test` menjalankan **10 kasus uji** dari 7 method. Itu yang harus lulus 10/10
pada server PostgreSQL 15.

### 5.2 Peta test ke skenario pemilik A–M

| Skenario pemilik | Test |
| --- | --- |
| A — lantai `PatientCode` | `AlokasiNomorReservasiTests.StagingAktif_TanpaPasienDiAtasLantai_MemberiLantai`, `…_DatabaseKosong_TidakPernahDiBawahLantai`; provider-real no. 1 |
| B — lantai MRN | Sama dengan A |
| C, D — tanpa kode/MRN ganda saat bersamaan | Provider-real no. 3 — **PASS** pada PostgreSQL 15.15. SQLite tidak dapat membuktikan kunci PostgreSQL |
| E — celah di atas lantai | `StagingAktif_Celah_…`, `…_PasienTerhapusTetapDihitungTerpakai`, `…_NomorSamaDalamFormatBerbeda_…`, `…_KodeLimaDigitBesar…`; provider-real no. 2 |
| F, G, H — Staging `false` / hilang / tidak sah | `PenilaianGuardReservasiTests`, `AlokasiNomorReservasiTests.StagingTidakSah_…`, `JalurPendaftaranReservasiTests.StagingTidakSah_Ditolak503_SebelumInsertQrDanFoto`; provider-real no. 4 |
| I — di luar Staging tidak berubah | `BukanStaging_HasilSamaDenganPembangkitLama` (enam data × tiga environment × empat bentuk konfigurasi terhadap salinan pembangkit lama), `BukanStaging_PenomoranLamaTetap`, `BukanStaging_KonfigurasiTidakTerbaca_TidakDibaca`; provider-real no. 5 |
| J, K, L — rute kiosk, admin, biasa | `RuteBiasaDanKiosk_StagingAktif_MemakaiLantai`, `RuteAdmin_StagingAktif_MemakaiLantai`, `KetigaRutePendaftaran_BermuaraPadaDuaActionYangDiketahui`, `Controller_TidakLagiMemilikiPembangkitNomorLama` |
| M — regresi `IsMember` provider-real | **Tidak di-*host* di task ini.** Blueprint menetapkannya milik `BE-PAT-MIG-002` (`PAT-MIG2-AC-47`, kontrak B15); kepemilikan tidak dipindah. Lihat bagian 7 butir `PAT-STALE-009` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `PAT-MIG3-AC-01` — allocator di module service; controller tidak mengalokasikan; pembangkit lama tidak dipanggil jalur mana pun | Terpenuhi | Pembangkit dihapus dari controller; `Controller_TidakLagiMemilikiPembangkitNomorLama`; QBE `PASS` |
| `PAT-MIG3-AC-02` — `POST /`, `/kiosk`, `/admin` memakai allocator yang sama | Terpenuhi | `CreatePatient` baris 547; `CreatePatientForAdmin` meneruskan ke action itu; uji tiga rute |
| `PAT-MIG3-AC-03` — suffix pertama yang belum dipakai ≥ 705.250 | Terpenuhi | Uji lantai dan celah |
| `PAT-MIG3-AC-04` — tidak pernah 742–705.249, termasuk dua kode tertunda | Terpenuhi | Uji dengan `PAT-RSMMC-00742`, `101416`, `705249` di database |
| `PAT-MIG3-AC-05` — serialisasi database; indeks unik jaring terakhir; tanpa lock process-local, Count+1, Max+1 | Terpenuhi | `pg_advisory_xact_lock(1346458697, 1313821261)` di dalam transaksi pemanggil; provider-real no. 1, 3, 6 **PASS** pada PostgreSQL 15.15 |
| `PAT-MIG3-AC-06` — lantai MRN ≥ 890.188; tidak membaca `migration.*` | Terpenuhi | Uji lantai dan celah MRN; source tidak memuat `migration.` |
| `PAT-MIG3-AC-07` — pemeriksa baca-saja, keluaran B14.2, endpoint khusus Staging dengan `Patient : Update` | Terpenuhi | `Pemeriksa_*`; bagian 4 |
| `PAT-MIG3-AC-08` — Staging persis wajib allocator aman reservasi; di luar itu algoritma lama tanpa perubahan hasil | Terpenuhi | Matriks penilaian; pembanding pembangkit lama |
| `PAT-MIG3-AC-09` — bentuk endpoint, perilaku `IsMember`, dan QR pendaftaran tidak berubah oleh task ini | Terpenuhi | DTO dan penulisan QR tidak disentuh. `IsMember` mengikuti Integration terbaru (`IsMember = false` pada `CreatePatient`, `HasSentinel(true)` di konfigurasi, `BE-CRM-015`) dan tidak diubah task ini |
| `PAT-MIG3-AC-10` — test terfokus wajib | Terpenuhi | 70 test baru lulus |
| `PAT-MIG3-AC-11` — build `0` error dan QBE `Strict` `PASS` | Terpenuhi | Bagian 5 |
| `PAT-MIG3-AC-12` (runtime, `PAT-GATE-005`) | Belum — **bukan** bagian build | Menunggu deploy terotorisasi dan konfigurasi `Enabled = true` |
| `PAT-MIG3-AC-13` — uji PostgreSQL 15 provider-real | Terpenuhi | 10/10 `PASS` pada PostgreSQL 15.15, dijalankan pemilik 17 Oktober 2026; basis data bukti terhapus (0 `pat_proof_*` tersisa) |
| `PAT-MIG3-AC-14` — gagal tertutup sebelum mutasi; lantai tidak dapat diubah lewat konfigurasi | Terpenuhi | `StagingTidakSah_Ditolak503_SebelumInsertQrDanFoto`; lantai berupa konstanta |

| Definition of Done — fase implementasi | Status |
| --- | --- |
| `PAT-DEC-033`, `PAT-OQ-031`, `PAT-OQ-037` diputuskan | ✅ (ditambah `PAT-DEC-067`, `068`) |
| Allocator, pemeriksa, dan aturan environment/konfigurasi sesuai B14–B14.3 | ✅ |
| Build backend lulus dan QBE `Strict` lulus | ✅ |
| Test terfokus lulus | ✅ |
| Uji PostgreSQL provider-real lulus (`PAT-MIG3-AC-13`) | ✅ — 10/10, 17 Oktober 2026 |
| Tidak ada berkas di luar cakupan yang berubah | ✅ — `.gitignore` dan `TestDatabase.cs` atas instruksi/kebutuhan pass ini, bagian 7 dan 8 |
| Laporan task lengkap | ✅ — berkas ini |
| Source sudah direview leader | **Belum** — siap direview |
| `PAT-GATE-005` diminta terpisah sesudah review | Belum — langkah berikutnya |

---

## 7. Pass rekonsiliasi dan review fix — 16 Oktober 2026

| Butir | Isi |
| --- | --- |
| A. HEAD lama | `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923` |
| B. HEAD Integration terbaru | `0c6e314594766fe7e55a833a0ce88095dc665a7c` (`Merge pull request #280`). Hubungan: 0 commit lokal di depan, **41** commit di belakang — fast-forward murni. Tidak ada `git fetch` |
| Cara pengamanan | `git stash` dan `git merge` ditolak pengaturan izin sesi agent. Pengamanan memakai salinan 20 berkas pekerjaan (termasuk 6 berkas test yang saat itu diabaikan) dan patch lokal di scratchpad sesi. Dua berkas yang beririsan dengan upstream dikembalikan ke isi `a52b3c98`; pemilik menjalankan `git merge --ff-only origin/QuilvianIntegrationBackend`; perubahan diterapkan ulang dengan `git merge-file` tiga arah (base `a52b3c98`, sisi Integration, sisi `BE-PAT-MIG-003`) |
| Berkas PatientManagement yang diubah upstream | `Controllers/PatientCompanyGuarantorController.cs`, `PatientController.cs`, `PatientIdentityDocumentController.cs`, `PatientMembershipController.cs`; `DTOs/PatientCompanyGuarantorDtos.cs`, `PatientDtos.cs`, `PatientIdentityDocumentDtos.cs`, `PatientMembershipDtos.cs`; `Enums/PatientCitizenship.cs` (baru); `Models/MstPatient.cs`; `Services/PatientIdentityDocumentFileService.cs` (baru), `PatientProfileQueryService.cs`; Membership `MembershipVerificationDecision.cs`, `PatientMembershipLifecycleHostedService.cs`, `PatientMembershipService.cs`, `PatientMembershipStatusResolver.cs`; ditambah `MstPatientConfiguration.cs`, `ApplicationDbContext.cs` (+10 baris), dan `Program.cs` (+26 baris) |
| C. Konflik dan penyelesaian | Hanya `PatientController.cs` dan `Program.cs` yang beririsan. Keduanya **0 konflik**. Perilaku Integration dipertahankan seluruhnya: filter pencarian MRN, field `Citizenship`/`NationalityCountryId` pada `CreatePatient`, dan `IsMember = false`. Tidak ada source lama yang menimpa perilaku baru. Upstream tidak menambah jalur insert `MstPatient` lain, tidak memanggil pembangkit lama, dan tidak memuat `INSERT INTO "MstPatient"` |
| D. Berkas yang berubah sesudah rekonsiliasi | `PatientIdentifierAllocationService.cs` dan `RsmmcIdentifierReservationProfile.cs` (kunci dua bagian), `PenilaianGuardReservasiTests.cs` dan `AlokasiPostgresSungguhanTests.cs` (kunci), `.gitignore`, `TestDatabase.cs` (penyaring regex), dokumen blueprint (`PAT-DEC-068`, `PAT-SRC-030`, `PAT-STALE-009`), laporan ini |
| E. `IsMember` `HasSentinel(true)` | **Dipertahankan.** `MstPatientConfiguration.cs` baris 99–101 tetap `.HasDefaultValue(true).HasSentinel(true)`; berkas itu tidak disentuh. Dampak ke `BE-PAT-MIG-002` dicatat `PAT-SRC-030` dan `PAT-STALE-009`, tanpa implementasi |
| G. Kunci advisory final | `SELECT pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE); bentuk transaksi; tanpa `hashtext()`; tanpa lock process-local; konstanta di `RsmmcIdentifierReservationProfile` sehingga seluruh instance memakai kunci yang sama. Impact scan: seluruh kunci lain di repository berbentuk satu kunci 64-bit (`hashtext` atau bigint dari Guid di `BbkBloodOrderService`, `NutritionOrderService`), yang tidak berbagi ruang kunci dengan bentuk dua kunci; tidak ada konvensi namespace terpusat; namespace worker `BE-PAT-MIG-002` `1173286842` berbeda |
| Temuan baseline: test SQLite rusak | Pada `0c6e3145`, **seluruh** 150 test project ini — termasuk 73 test `BE-PAT-MIG-001` yang tidak diubah — gagal di `EnsureCreated` SQLite: `SQLite Error 1: 'near "~": syntax error'`. Penyebab dan perbaikannya pada bagian 7.1. Project test lain (`OperatingRoomTests`, `PharmacyTests`, `NutritionTests`) memakai penyaring yang sama dan **kemungkinan** terdampak — tidak diubah, karena di luar cakupan |

### 7.1 `TestDatabase.cs` — penyaring model uji SQLite

| Hal | Isi |
| --- | --- |
| Constraint penyebab | `CK_MstPayrollDeductionCutover_Period` — `"AutomaticFromPeriod" ~ '^\d{4}-\d{2}$'` (`Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstPayrollDeductionCutoverConfiguration.cs` baris 13), dan `CK_FinPayrollDeductionBatch_Period` — `"DeductionPeriod" ~ '^[0-9]{4}-(0[1-9]\|1[0-2])$'` (`…/Receivable/FinPayrollDeductionBatchConfiguration.cs` baris 17). Keduanya datang dari upstream Finance di antara `a52b3c98` dan `0c6e3145` |
| Cara kerja | `ModelUjiSqlite` adalah `IModelCustomizer` yang **hanya** dipasang `TestDatabase.CreateContext` pada project test ini. Saat model uji dibentuk, ia melepas constraint yang `ModelName`-nya ada di daftar `CheckConstraintRegexFinance` — tepat dua nama di atas. Penyaring lama (`::`, `num_nonnulls`) tidak berubah |
| Source/konfigurasi produksi | **Tidak** diubah. Kedua konfigurasi Finance, migration, dan model aplikasi tetap; PostgreSQL tetap menegakkan kedua constraint |
| Cakupan pelepasan | Hanya dari model SQLite di memori milik project test ini. Hasil scan seluruh konfigurasi EF: hanya dua constraint itu yang memakai operator regex PostgreSQL; temuan `~` lain (`LabOrderNumberService`, `LabReportNumberService`) adalah SQL mentah di service, bukan model |
| Mengapa tidak generik | Versi pertama pass rekonsiliasi melepas **setiap** SQL ber-`~`. Itu dipersempit: constraint regex baru kelak — misalnya pada `MstPatient` — sengaja tetap menggagalkan `EnsureCreated`, sehingga tidak ada validasi model yang hilang diam-diam |
| Dapatkah menyembunyikan regresi Patient Management? | Tidak. Kedua constraint ada pada tabel Finance (`MstPayrollDeductionCutover`, `FinPayrollDeductionBatch`) yang tidak dibaca maupun ditulis oleh test Patient Management. Constraint pada `MstPatient` dan tabel Patient Management lain tidak tersentuh, termasuk `CK_MstPatientCompanyGuarantor_BenefitLink` |
| Mengapa 73 test `BE-PAT-MIG-001` tetap bermakna | Mereka menguji service rekonsiliasi, `MstPatient`, QR, dan sumber migrasi tiruan — tidak satu pun bergantung pada tabel payroll Finance. Tanpa perbaikan, mereka tidak dapat berjalan sama sekali karena skema uji gagal dibentuk; sesudah perbaikan, skema Patient Management yang mereka pakai identik dengan sebelum upstream, dan 73/73 lulus |

**Diff persis terhadap `0c6e3145`:**

```diff
@@ -77,6 +77,18 @@
         private const string CastPostgres = "::";

+        // Dua CHECK constraint modul Finance pada Integration 0c6e3145 memakai operator regex
+        // PostgreSQL (~) yang tidak dikenal SQLite, sehingga EnsureCreated gagal untuk seluruh uji
+        // project ini (BE-PAT-MIG-003, rekonsiliasi baseline). Hanya keduanya — disebut namanya —
+        // yang dilepas dari model uji SQLite; tabel Patient Management tidak memakainya. Constraint
+        // regex lain yang muncul kelak sengaja tetap menggagalkan EnsureCreated, supaya tidak ada
+        // validasi model yang hilang diam-diam.
+        private static readonly HashSet<string> CheckConstraintRegexFinance =
+        [
+            "CK_MstPayrollDeductionCutover_Period",
+            "CK_FinPayrollDeductionBatch_Period"
+        ];
+
         public override void Customize(ModelBuilder modelBuilder, DbContext context)
@@ -84,7 +96,7 @@
                 var bermasalah = entitas.GetDeclaredCheckConstraints()
-                    .Where(x => KhasPostgres(x.Sql))
+                    .Where(x => KhasPostgres(x.Sql) || CheckConstraintRegexFinance.Contains(x.ModelName))
                     .Select(x => x.ModelName)
                     .ToList();
```

### 7.2 Harness provider-real — `TEMPLATE template0` (17 Oktober 2026)

| Hal | Isi |
| --- | --- |
| Gejala | Percobaan pertama pemilik: seluruh 10 kasus gagal saat persiapan dengan `55006 source database "template1" is being accessed by other users`. `CREATE DATABASE` tanpa template menyalin `template1`, dan PostgreSQL menolak salinan selama ada sesi lain di `template1`. Bukan galat allocator; pemilik mereproduksi hal yang sama secara manual |
| Perbaikan | Hanya baris pembuatan basis data bukti di `PostgresSekaliPakai.BuatAsync`: `CREATE DATABASE "pat_proof_<guid>" WITH TEMPLATE template0;`. `template1` tidak diubah, sesinya tidak dihentikan, konfigurasi server tidak diubah |
| Tinjauan keamanan | Nama tetap `pat_proof_<guid>`; pemilik basis data tetap role yang membuatnya (tanpa klausa `OWNER`); pembersihan tetap `DROP DATABASE IF EXISTS … WITH (FORCE)`; `EnsureCreatedAsync` tetap membentuk skema dari model EF aplikasi. Model hanya memakai fungsi inti PostgreSQL (`CURRENT_TIMESTAMP`, cast `jsonb`) tanpa ekstensi, metode indeks, atau collation khusus, sehingga tidak bergantung pada isi `template1`. Tidak ada source aplikasi maupun EF migration yang berubah |

```diff
-            await JalankanAdminAsync(admin.ConnectionString, $"CREATE DATABASE \"{namaDatabase}\";");
+            // TEMPLATE template0, bukan template1 bawaan: … (komentar 4 baris)
+            await JalankanAdminAsync(
+                admin.ConnectionString,
+                $"CREATE DATABASE \"{namaDatabase}\" WITH TEMPLATE template0;");
```

---

## 8. `.gitignore` — pelacakan berkas test

Aturan yang ditambahkan, tepat di bawah blok `NutritionTests`, mengikuti pola yang sudah ada:

```gitignore
# Begitu pula Patient Management: guard reservasi nomor pasien normal `BE-PAT-MIG-003` dan
# rekonsiliasi MRN Pilot `BE-PAT-MIG-001` hanya tetap terjaga bila ujinya ikut masuk repositori.
!/Tests/QuilvianSystemBackend.PatientManagementTests/
/Tests/QuilvianSystemBackend.PatientManagementTests/bin/
/Tests/QuilvianSystemBackend.PatientManagementTests/obj/
```

| Pemeriksaan | Hasil |
| --- | --- |
| Berkas test baru | Tampil normal di `git status` sebagai `??` — `Infrastructure/PostgresSekaliPakai.cs`, `Infrastructure/ReservasiHarness.cs`, `ReservasiNomor/` |
| `bin/`, `obj/` project itu | Tetap diabaikan — `git check-ignore` menunjuk `.gitignore` baris 417 dan 418 |
| Folder `Tests/` lain yang tidak terdaftar | Tetap diabaikan oleh `/Tests/*` (baris 396) |

---

## 9. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Lihat bagian 7: test SQLite project lain kemungkinan rusak pada `0c6e3145`; desain `BE-PAT-MIG-002` perlu impact scan (`PAT-STALE-009`) |
| Masalah yang diketahui | **(1)** ~~Bukti PostgreSQL provider-real belum ada (`PAT-RSK-038`)~~ — terpenuhi 17 Oktober 2026, 10/10. **(2)** Kunci allocator dipegang sampai *commit*, sedangkan `CreatePatient` menulis QR dan foto di dalam transaksi; di Staging, pendaftaran bersamaan menunggu selama penulisan berkas itu — menata ulang alur `CreatePatient` di luar cakupan. **(3)** Temuan lama di luar cakupan, tidak diubah: log Docker `PATIENT_CREATE_STEP` memuat `FullName`; log hasil `Patient.CreatePatient` memuat properti `Id`; `SavePatientQrCodeFile` menimpa berkas (`PAT-SRC-016`); pembangkit lama (mode non-Staging) memuat seluruh nomor ke memori dan `int.Parse` MRN dapat melempar galat pada data angka sangat panjang — dipertahankan karena perilaku Produksi tidak boleh berubah |
| Risiko tersisa | Sesudah deploy ke Staging tanpa `Enabled = true`, **seluruh** pendaftaran normal Staging ditolak `503` sampai pemilik mengisinya — disengaja (`PAT-DEC-067`); pembekuan `PAT-DEC-055` tetap berlaku sampai `PAT-GATE-005`. Kode normal baru 16 karakter (`PAT-RSMMC-705250`) |
| Perubahan sampingan | `NONE`. Akhir baris berkas disamakan ke CRLF sesuai *working tree* |
| Interupsi | Perintah `git stash` dan `git merge` ditolak pengaturan izin; tidak diulang. Pemilik menjalankan fast-forward sendiri; pemulihan memakai cadangan scratchpad dan merge tiga arah, tanpa kehilangan berkas |
| Status Git | Pada `0c6e3145`: `M .gitignore`, `M Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs`, `M Program.cs`, `M Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/TestDatabase.cs`, `M` enam berkas `docs/module-blueprints/patient-management/**`; `??` empat berkas source baru, `??` `Tests/…/Infrastructure/PostgresSekaliPakai.cs`, `??` `Tests/…/Infrastructure/ReservasiHarness.cs`, `??` folder `Tests/…/ReservasiNomor/` (4 berkas), `??` `requirement-completeness-assessment.md`, `??` laporan ini. Tidak ada *stage*, *commit*, atau *push* |
| O. Blocker tersisa | **(1)** Review source leader/pemilik. **(2)** Otorisasi terpisah untuk *commit*/*push*/PR dan branch fitur. **(3)** `PAT-GATE-005`: deploy Staging terotorisasi dengan `RsmmcMigrationReservationGuard__Enabled=true`, lalu `GET /admin/migration/rsmmc-reservation-guard` → `overallGuardReady = true`. *Blocker server PostgreSQL provider-real selesai 17 Oktober 2026* |
| Langkah berikutnya | Review source final oleh pemilik/leader. Sesudah disetujui: keputusan *commit* dan branch, otorisasi deployment Staging, lalu `PAT-GATE-005`. `BE-PAT-MIG-002` belum boleh dimulai sebelum `PAT-GATE-005` `PASS` dan impact scan `PAT-STALE-009` |

**BE-PAT-MIG-003 PROVIDER-REAL PROOF PASS — READY FOR FINAL OWNER REVIEW**

---

## 10. Adendum 19 Oktober 2026 — runtime dan tindak lanjut

Ditambahkan oleh `build-module-backend` saat mengerjakan `BE-PAT-MIG-003A`. Isi bagian 1–9 tidak diubah.

| Hal | Isi |
| --- | --- |
| Publikasi source | Di-*merge* pemilik ke `QuilvianIntegrationBackend` lewat PR #290: `a9f2fda2` (dokumen), `ab45dd3c` (source), merge `3c457ab4` |
| Deployment | Dilaporkan pemilik: dideploy ke DEV dan STAGING; Staging `RsmmcMigrationReservationGuard__Enabled = true`; health `PASS` |
| `PAT-GATE-005` generasi 1 | **`PASS`** (dilaporkan pemilik, `PAT-FACT-090`): `guardState = ACTIVE`, `nextPatientCode` 705250 / `PAT-RSMMC-705250`, `nextMrn` 890188 / `00-89-01-88`, `overallGuardReady = true`. Memenuhi `PAT-MIG3-AC-12`. Agent tidak memanggil endpoint maupun membuat pasien |
| Status task | ✅ — seluruh 14 acceptance criteria terbukti, termasuk runtime `AC-12` |
| Temuan sesudahnya | Impact scan `BE-PAT-MIG-002` 19 Oktober 2026 menemukan jalur pendaftaran bayi `POST /newborns` (upstream `3a0189c2`) yang mengalokasikan nomor lewat `NumberSeries` di luar allocator task ini (`PAT-SRC-031`). Jalur itu **di luar cakupan** task ini — saat task ini dibangun jalur tersebut belum ada di baseline `0c6e3145` — dan ditutup task prasyarat `BE-PAT-MIG-003A` ([laporan](BE-PAT-MIG-003A.md)). Generasi 1 gerbang hanya membuktikan `CreatePatient` |

**BE-PAT-MIG-003 DEPLOYED — PAT-GATE-005 GENERATION 1 PASS**
