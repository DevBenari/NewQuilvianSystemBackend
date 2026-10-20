# Laporan Perubahan Backend — `BE-PAT-MIG-003A`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PAT-MIG-003A` |
| Judul | Newborn Identifier Reservation Guard |
| Slice | S4 — Guard reservasi nomor pendaftaran bayi |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md), kartu `BE-PAT-MIG-003A` |
| Trace | `PAT-DEC-069`–`PAT-DEC-077`; `PAT-SRC-031`–`033`; `PAT-FACT-090`, `091`; `PAT-MIG3A-AC-01`–`PAT-MIG3A-AC-12`; kontrak B14.2, B14.4, B15 |
| Contract version | `1.3.0` — `approved` 19 Oktober 2026 (instruksi tertulis pemilik `TASK: BE-PAT-MIG-003A`) |
| Dependency | `BE-PAT-MIG-003` ✅ — dideploy, `PAT-GATE-005` generasi 1 `PASS` (`PAT-FACT-090`) |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas diubah > 8 termasuk test dan dokumen (2), logika bisnis kompleks — konkurensi dan urutan kunci (2), kontrak pemeriksa berubah aditif (2), perilaku transaksi yang sudah ada (1), berkaitan dengan keamanan (1), satu repository (0), tanpa UI (0) |
| Task mode | `BACKEND` — instruksi pemilik `TASK: BE-PAT-MIG-003A`, lewat `quilvian-engineering-skills:build-module-backend` |
| Target tulis | `NewQuilvianSystemBackend`: source PatientManagement `MasterData`, project test `Tests/QuilvianSystemBackend.PatientManagementTests/`, `docs/module-blueprints/patient-management/**` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Dibangun di atas `c6537ee4` (HEAD = origin, 19 Oktober 2026), lalu di-*rebase* ke `QuilvianIntegrationBackend` @ `98fdc69ff0dcf97622d0c9c27779e9277e65d220` sebagai commit WIP `6ae9c251` di branch `BE-PAT-MIG-003A`; delta upstream `c6537ee4..98fdc69f` hanya `BillingInvoiceService.cs`. Koreksi review final `F-07`/`F-13`/`F-14` di atas `6ae9c251`, belum di-*commit* |
| Tanggal | 19 Oktober 2026 |
| Status | 🟡 **Siap review PR pemilik — pra-runtime.** Build Release, test SQLite, QBE, dan pemeriksaan model lulus; uji PostgreSQL 15 provider-real `23`/`23` PASS (smoke `1`/`1`; 0 gagal, 0 dilewati; 4259,7 detik ≈ 1 jam 10 menit; 0 basis data `pat_proof_*` tersisa; kontainer dan image `postgres:15` sekali pakai dihapus) — dijalankan pemilik 19 Oktober 2026 pada commit `6ae9c251`. Koreksi `F-07` sesudahnya mengubah `AllocateReservedAsync`, sehingga kasus provider-real yang memanggil method itu wajib dijalankan ulang (bagian 9). `PAT-MIG3A-AC-12` milik runtime `PAT-GATE-005` generasi 2 |

---

## 1. Masalah yang diperbaiki

`BE-PAT-MIG-003` melindungi nomor pasien yang sudah dipesan migrasi RSMMC dari pendaftaran pasien
biasa. Sesudah task itu dibangun, fitur **pendaftaran bayi baru lahir** (`POST /newborns`, commit
upstream `3a0189c2`) masuk ke Integration. Fitur ini membagikan `PatientCode` dan MRN dengan deret
nomor platform (`NumberSeries`) yang dimulai dari nomor tertinggi yang sudah ada, tanpa lantai
reservasi dan tanpa kunci allocator normal (`PAT-SRC-031`).

*Contoh.* Di Staging, kode pasien tertinggi adalah `PAT-RSMMC-00741`. Bidan mendaftarkan bayi "By. Ny.
Sari". Sebelum task ini, bayi itu mendapat `PAT-RSMMC-00742` — kode yang sudah dipesan untuk pasien lama
RSMMC `legacy_pid` -667995, pasien ke-5 Canary-A. Saat worker migrasi kelak membuat pasien itu, kodenya
sudah terpakai dan Canary-A berhenti. Pemeriksa `PAT-GATE-005` generasi 1 tetap melaporkan
`overallGuardReady = true`, karena ia hanya mengukur jalur `CreatePatient`.

Sesudah task ini, di Staging dengan guard aktif bayi yang sama mendapat `PAT-RSMMC-705250` /
`00-89-01-88` — atau nomor kosong berikutnya di atas lantai — lewat allocator dan kunci yang sama
dengan pendaftaran biasa. Di Produksi tidak ada yang berubah.

---

## 2. Proses bisnis

**Tujuan.** Pendaftaran bayi di Staging tidak pernah memakai nomor milik pasien lama RSMMC, tanpa
mengubah perilaku pendaftaran bayi di Produksi.

**Pelaku.** Petugas yang punya izin `Patient : Create` (bidan, admisi rawat inap).

**Pemicu.** `POST /newborns` dengan header `Idempotency-Key`, dari kasus persalinan obstetri.

**Langkah berurutan** (`NewbornPatientRegistrationService.RegisterAsync`):

1. Validasi isian tanpa basis data — ibu, kasus, kunci idempotency 8–200 karakter, tanggal dan jam
   lahir, jenis kelamin, berat/panjang. Gagal → `400`. *Tidak berubah.*
2. **Baru:** guard reservasi dinilai satu kali (`EvaluateGuard`), lalu cara alokasi ditentukan
   (`ResolveIdentifierAllocationMode`, baris 104–105):
   - `Staging` persis + `Enabled = true` → `RESERVED_FLOOR`;
   - `Staging` persis + `false` / hilang / salah bentuk / tidak terbaca → `FAIL_CLOSED`: kembali
     `ReservationGuardRejected` (baris 106–110) **sebelum** basis data dibaca, dikunci, atau ditulis;
   - environment lain → `LEGACY`; konfigurasi guard tidak dibaca.
3. Cek idempotency di luar transaksi; bila ada → pemutaran ulang atau `409`. *Tidak berubah.*
4. Buka transaksi; kunci `hashtext('PAT_NEWBORN_KEY_…')` lalu `hashtext('PAT_NEWBORN_CASE_…')`; cek
   ulang idempotency; validasi ibu, kasus persalinan, dan NIK; naikkan urutan kelahiran. *Tidak
   berubah.*
5. **Alokasi nomor** (baris 206–223):
   - `RESERVED_FLOOR` → `PatientIdentifierAllocationService.AllocateReservedAsync` di transaksi yang
     sama: kunci `pg_advisory_xact_lock(1346458697, 1313821261)`, lalu `PatientCode` dan MRN pertama
     yang belum dipakai ≥ 705.250 / 890.188. Deret `NumberSeries` tidak dibaca maupun dinaikkan;
   - `LEGACY` → `AllocatePatientCodeAsync` dan `AllocateMedicalRecordNumberAsync` apa adanya.
6. Insert `MstPatient`, *commit*; seluruh kunci transaksi lepas. *Tidak berubah.*

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Staging persis, guard tidak sah | `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` + `guardState`; log peringatan `Patient.CreateNewbornPatient.ReservationGuard` tanpa data pribadi; nol perintah SQL |
| Kunci idempotency sama, data sama | Hasil sebelumnya (`WasExisting = true`), tanpa nomor baru |
| Kunci idempotency sama, data berbeda | `409` |
| Dua permintaan bersamaan | Diserialkan kunci; nomor tidak pernah ganda |
| MRN melewati 99-99-99-99 (guard aktif) | Galat sama dengan pendaftaran normal — `InvalidOperationException`, transaksi dibatalkan |

**Hasil akhir.** Satu pasien bayi dengan nomor yang aman reservasi di Staging, atau nomor
`NumberSeries` seperti sebelumnya di environment lain.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TEST_POLICY`, `REVIEW_RULES`, `DATABASE_RULES`,
`TASK_CLASSIFICATION`, `API_RULES`, `REPORT_TEMPLATE`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`
(identik dengan suite) dan registry; `NewbornPatientRegistrationService.cs`; `NumberSeriesAllocator.cs`;
`PatientIdentifierAllocationService.cs`; `RsmmcIdentifierReservationProfile.cs`;
`RsmmcMigrationReservationGuardOptions.cs`; `RsmmcReservationGuardDtos.cs`; `PatientController.cs`
(`CreatePatient`, `CreateNewbornPatient`, pemeriksa, helper penolakan); `Program.cs` (DI);
`MstPatientConfiguration.cs`; `PatNewbornBirthSequenceConfiguration.cs`; `OprCase.cs` dan
konfigurasinya; `PatientDtos.cs` (`CreateNewbornPatientRequest`); seluruh pemakai
`pg_advisory_xact_lock` di repository; inventaris insert `MstPatient`; harness test `BE-PAT-MIG-003`;
dokumen blueprint `patient-management`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcIdentifierReservationProfile.cs` | `RsmmcIdentifierAllocationMode` (`LEGACY`, `RESERVED_FLOOR`, `FAIL_CLOSED`); aturan tunggal `ResolveAllocationMode`; `IsMigrationReservationSafe`; `IsGuardReady` dua jalur; `ToAllocationModeCode`; komentar kunci `NORM` mencakup bayi |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientIdentifierAllocationService.cs` | Cabang berlantai `AllocateAsync` dipindah ke method publik `AllocateReservedAsync(guard)` tanpa perubahan algoritma — dipakai bersama pendaftaran normal dan bayi; `ResolveAllocationMode`; `CheckAsync` melaporkan mode dan perlindungan kedua jalur dan memakai `IsGuardReady` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs` | Konstruktor menerima `PatientIdentifierAllocationService`; status `ReservationGuardRejected`; `ResolveIdentifierAllocationMode`; guard dinilai di awal; cabang alokasi berlantai. Method `NumberSeries`, kunci, idempotency, dan urutan kelahiran tidak diubah |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | `CreateNewbornPatient`: `503` untuk `ReservationGuardRejected`, `ProducesResponseType` 503, `GuardState` pada log sukses; helper penolakan dipakai bersama dengan nama event per jalur; log pemeriksa memuat kedua flag |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcReservationGuardDtos.cs` | Empat field aditif: `NormalPatientCreateAllocationMode`, `NormalPatientCreateProtected`, `NewbornPatientCreateAllocationMode`, `NewbornPatientCreateProtected` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/BayiUji.cs` | **Baru** — fixture ibu + kasus persalinan, pabrik `NumberSeries` yang menolak dipakai, penghitung perintah SQL |
| `Tests/QuilvianSystemBackend.PatientManagementTests/ReservasiNomor/PendaftaranBayiReservasiTests.cs` | **Baru** — 19 method / 46 kasus SQLite |
| `Tests/QuilvianSystemBackend.PatientManagementTests/ReservasiNomor/PendaftaranBayiPostgresSungguhanTests.cs` | **Baru** — 9 method / 13 kasus provider-real PostgreSQL 15 |
| `Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/ReservasiHarness.cs` | `BuatController` menerima layanan bayi opsional |
| `docs/module-blueprints/patient-management/00-interview-decisions.md` | Revision `6`: `PAT-DEC-069`–`077`, `PAT-SRC-031`–`033`, `PAT-FACT-090`–`091`, bagian 15, log koreksi |
| `docs/module-blueprints/patient-management/contracts/api-contract.md` | `1.3.0`: B4.2, B4.3, B10, B14, B14.1, B14.2, B14.4 (baru), B15 |
| `docs/module-blueprints/patient-management/roadmap/backend-roadmap.md` | Revision `6`: kartu, slice, baris task/register `BE-PAT-MIG-003A`; `BE-PAT-MIG-003` ✅; `BE-PAT-MIG-002` ⛔ dengan prasyarat baru; `PAT-GATE-005` generasi 1/2; `PAT-RSK-039`–`042` |
| `docs/module-blueprints/patient-management/roadmap/requirement-traceability.md` | `BE-PAT-MIG-003` ✅; amandemen `PAT-MIG2-AC-46`/`47`; bagian 7 baru |
| `docs/module-blueprints/patient-management/blueprint-manifest.md` | Revision `6`, hash, `PAT-STALE-009` dan `010` ditutup |
| `docs/module-blueprints/patient-management/task/report/backend/BE-PAT-MIG-003.md` | Adendum runtime bagian 10 |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientIdentifierAllocationService.cs` — koreksi `F-07` | `AllocateReservedAsync` memanggil `EnsureReservedAllocationAllowed` sebelum SQL apa pun: environment berjalan harus `Staging` persis, `guard.EnvironmentName` harus sama persis (ordinal), guard harus `ReservedFloor`, dan konfigurasi saat ini harus aktif |
| `Tests/QuilvianSystemBackend.PatientManagementTests/ReservasiNomor/AlokasiBerlantaiGuardBuatanTests.cs` — koreksi `F-07` | **Baru** — T-F07-01 sampai 06 dan guard `null`: 7 method / 17 kasus |
| `Tests/QuilvianSystemBackend.PatientManagementTests/Infrastructure/PostgresSekaliPakai.cs` — koreksi `F-14` | Remarks: prasyarat `max_locks_per_transaction = 2048` khusus server bukti sekali pakai |
| Laporan ini | **Baru** |

`Program.cs` **tidak** berubah: `NewbornPatientRegistrationService` dan `PatientIdentifierAllocationService`
sudah terdaftar `Scoped`, sehingga keduanya menerima `ApplicationDbContext` yang sama dalam satu request —
syarat agar kunci diambil di transaksi bayi.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /newborns`: bentuk request/response tidak berubah; tambahan `503` khusus `Staging` persis. `GET /admin/migration/rsmmc-reservation-guard`: empat field aditif, `overallGuardReady` kini mensyaratkan kedua jalur |
| Database | Tidak ada perubahan entity, konfigurasi, maupun migration. `has-pending-model-changes`: tanpa perubahan. Tidak ada eksekusi database selain test provider-real yang tidak dijalankan di sesi ini |
| Keamanan/Auth | Tidak ada perubahan izin: `POST /newborns` tetap `Patient : Create`, pemeriksa tetap `Patient : Update` khusus Staging. Log dan respons penolakan tanpa data pribadi |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/newborns` | Mendaftarkan satu bayi dari kasus persalinan; di Staging persis nomor berlantai atau `503` bila guard tidak sah | `Patient : Create` |
| `GET` | `/admin/migration/rsmmc-reservation-guard` | Pemeriksa baca-saja `PAT-GATE-005`; kini melaporkan perlindungan pendaftaran normal dan bayi | `Patient : Update` (khusus Staging persis) |

Contoh keluaran pemeriksa generasi 2 di Staging dengan `Enabled = true`:

```json
{
  "environment": "Staging",
  "guardState": "ACTIVE",
  "guardEnabled": true,
  "guardConfigurationValid": true,
  "nextPatientCodeFormatted": "PAT-RSMMC-705250",
  "nextMrnFormatted": "00-89-01-88",
  "patientCodeFloorSatisfied": true,
  "mrnFloorSatisfied": true,
  "normalPatientCreateAllocationMode": "RESERVED_FLOOR",
  "normalPatientCreateProtected": true,
  "newbornPatientCreateAllocationMode": "RESERVED_FLOOR",
  "newbornPatientCreateProtected": true,
  "overallGuardReady": true
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Baseline `dotnet test` PatientManagementTests pada `c6537ee4`, sebelum perubahan | `143` lulus / `0` gagal / `7` dilewati | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.csproj -c Release --no-incremental` | Berhasil, `0` error, `298` warning; **0 warning dari berkas yang diubah** | `PASS` (warning `EXISTING`) | Log build di scratchpad, disaring per berkas |
| `dotnet test` PatientManagementTests sesudah perubahan | `189` lulus / `0` gagal / `16` dilewati | `PASS` | Keluaran perintah |
| `--filter FullyQualifiedName~PendaftaranBayi` (test baru) | `46` lulus / `0` gagal / `9` dilewati | `PASS` | 9 dilewati = method provider-real baru |
| `--filter ReservasiNomor` tanpa test baru (regresi `BE-PAT-MIG-003`) | `70` lulus / `0` gagal / `7` dilewati | `PASS` | Sama dengan sebelum task |
| Regresi `BE-PAT-MIG-001` | `73` lulus (189 − 46 − 70) | `PASS` | Hitungan |
| Uji PostgreSQL 15 provider-real `--filter Kategori=ProviderReal`, dijalankan pemilik pada `6ae9c251` | Smoke `1`/`1`; penuh `23` lulus / `0` gagal / `0` dilewati (Total 23); 4259,7 detik; 0 basis data `pat_proof_*` tersisa; kontainer dan image `postgres:15` dihapus | `PASS` | Bukti pemilik 19 Oktober 2026. Server bukti: `max_locks_per_transaction = 2048` (bagian 5.2) |
| *Riwayat:* uji provider-real di sesi build agent | Tidak dijalankan: `QUILVIAN_PG_PROOF_CONNECTION` tidak tersedia | `NOT RUN` (riwayat) | 16 dilewati dengan alasan yang terlihat |
| `dotnet ef migrations has-pending-model-changes --context ApplicationDbContext` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | 9 berkas dievaluasi, `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` | Keluaran perintah |
| Review diff: method `NumberSeries` bayi | Tidak ada hunk pada `AcquireLockAsync`, `FindByIdempotencyAsync`, `ReplayOrConflict`, `AllocatePatientCodeAsync`, `AllocateMedicalRecordNumberAsync`, `GetLegacyMaximumAsync`; dua pemanggilannya hanya dipindah ke cabang `else` | `PASS` | `git diff -U0` |
| Audit kunci repository | Satu-satunya pemakai kunci dua bagian: `AllocateReservedAsync`; `PAT_NEWBORN_*` hanya dipakai `RegisterAsync` | `PASS` | Pencarian `pg_advisory` dan `AllocationLockNamespace` |

`AUTOMATED TEST: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests — PASS (189/0/16 dilewati; test diminta eksplisit pemilik pada task ini, T1–T17)`

Uji manual: `NOT APPLICABLE` — task melarang deploy, pemanggilan endpoint Staging, dan pembuatan pasien.

**Tidak dijalankan oleh agent:** uji PostgreSQL provider-real (server tidak tersedia di sesi agent — dijalankan
pemilik, 23/23 `PASS`); `dotnet ef database update` dan eksekusi database apa pun (dilarang); deployment
(dilarang).

**Cara pemilik menjalankan bukti provider-real** — sama dengan `BE-PAT-MIG-003`, terhadap server
PostgreSQL 15 sekali pakai (bukan Staging, bukan Produksi). Connection string tidak dicatat di mana pun:

```text
QUILVIAN_PG_PROOF_CONNECTION=<basis data pemeliharaan server sekali pakai>
QUILVIAN_PG_PROOF_REQUIRED=1
dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter Kategori=ProviderReal
```

Diharapkan **23** kasus: 10 milik `BE-PAT-MIG-003` dan 13 milik task ini (9 method; dua di antaranya
`Theory` tiga kasus), seluruhnya lulus, dan `0` basis data `pat_proof_*` tersisa. **Hasil pemilik 19 Oktober
2026: persis 23/23 lulus, 0 tersisa.**

### 5.2 Prasyarat server bukti sekali pakai (`F-14`)

Model `ApplicationDbContext` kini cukup besar sehingga `EnsureCreatedAsync` — pembentukan skema basis data
uji — gagal pada nilai bawaan PostgreSQL 15 `max_locks_per_transaction = 64`, dengan SQLSTATE `53200` *out of
shared memory*; server sendiri memberi petunjuk *increase max_locks_per_transaction*. Server bukti yang lulus
23/23 memakai `max_locks_per_transaction = 2048`, misalnya:

```text
docker run … postgres:15 -c max_locks_per_transaction=2048
```

**Ini hanya syarat server uji sekali pakai.** Ini **bukan** syarat maupun anjuran mengubah PostgreSQL DEV,
STAGING, atau PRODUCTION; `appsettings*` dan infrastruktur deployment tidak disentuh. Prasyarat yang sama
tercatat pada remarks `PostgresSekaliPakai` dan kontrak B15.

### 5.1 Pemetaan T1–T17 ke bukti

| ID | Bukti SQLite (dijalankan) | Bukti provider-real (`PASS` 23/23 pada `6ae9c251`, dijalankan pemilik) |
| --- | --- | --- |
| T1 | `T1_T2_BukanStagingPersis_MemakaiNumberSeriesSepertiSebelumnya` (Production ×3) — cabang `NumberSeries` tercapai | `T1_T2_BukanStagingPersis_NumberSeriesSepertiSebelumnya`: `00741` → `PAT-RSMMC-00742` / `00-00-07-42`, pemutaran ulang, lalu `00743`; deret tersimpan 743 |
| T2 | Kasus Development, `staging`, `Test`; `T2_BukanStaging_KonfigurasiGuardTidakDibaca` | Kasus Development dan `staging`, sambil sesi lain memegang kunci `(PATI, NORM)` — tidak menunggu |
| T3–T5 | `T3_T4_T5_*` (4 kasus, 0 perintah SQL), `T5_*TidakTerbaca`, `Rute_StagingTidakSah_Ditolak503*` (3 kasus) | `T3_T4_T5_*` (3 kasus): tanpa pasien, deret, urutan, maupun kunci |
| T6 | `T6_StagingAktif_*` (lantai; celah) | `T6_StagingAktif_*`: `705250` / `00-89-01-88`, deret 0 |
| T7 | `T7_NormalLaluBayi_*`, `T7_BayiLaluNormal_*` | `T7_T9_NormalDanBayiBersamaan_*` |
| T8 | — | `T8_BayiBersamaan_KasusBerbeda_*` (6 bersamaan, jeda 150 ms) |
| T9 | — | `T7_T9_*` (4 normal + 4 bayi bersamaan) |
| T10 | Struktur: cabang memanggil `AllocateReservedAsync`; `KunciAdvisory_*` lama | `T10_StagingAktif_BayiMenungguKunciPatiNormLaluMelepasnyaSaatCommit` — permintaan kunci `(1346458697, 1313821261)` terlihat menunggu di `pg_locks`, lalu 0 sesudah *commit* |
| T11, T12 | `T11_T12_T14_*`, `T11_T12_Pemeriksa_StagingTidakSah_*`, `Pemeriksa_BukanStaging_*` | `T11_T12_T14_Pemeriksa_*` |
| T13 | `T13_*` (7 + 4 kasus), `CaraAlokasiBayi_SamaDenganPendaftaranNormal` (5 kasus), `AlokasiBerlantai_MenolakGuardYangTidakAktif` (3 kasus) | — |
| T14 | `T11_T12_T14_*`: pasien, berkas, deret, urutan tetap | `T11_T12_T14_*`: 0 kunci sesi, pasien dan deret tetap |
| T15 | `T15_*` (data sama; data berbeda) | `T15_*KunciIdempotencySamaBersamaan_*` |
| T16 | `T16_*` (kembar satu kasus) | `T16_*KembarBersamaanSatuKasus_*` |
| T17 | Seluruh 143 test lama lulus | 10 kasus lama ikut dijalankan |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `PAT-MIG3A-AC-01` — Staging aktif: nomor bayi ≥ lantai lewat `AllocateReservedAsync`; `NumberSeries` tidak disentuh | Terpenuhi | `NewbornPatientRegistrationService.cs` baris 206–218; T6 SQLite dan provider-real |
| `PAT-MIG3A-AC-02` — Staging tidak sah → `503` sebelum akses basis data | Terpenuhi | Baris 104–110; `PatientController.cs` baris 884–892; T3–T5 (0 perintah SQL) |
| `PAT-MIG3A-AC-03` — di luar Staging persis `NumberSeries` tidak berubah, tanpa kunci baru | Terpenuhi | Baris 219–223; review diff; T1, T2 SQLite dan provider-real; koreksi `F-07`: allocator berlantai menolak guard buatan di luar Staging persis sebelum SQL (T-F07-01 sampai 04, 06) |
| `PAT-MIG3A-AC-04` — kunci yang disetujui, urutan tanpa siklus | Terpenuhi | `PatientIdentifierAllocationService.cs` baris 159–168; audit kunci; kontrak B14.4; T10 provider-real |
| `PAT-MIG3A-AC-05` — konkurensi tiga pasangan tanpa nomor ganda | Terpenuhi | T7 SQLite; T8, T9 dan `C_D_*` provider-real |
| `PAT-MIG3A-AC-06` — idempotency tetap | Terpenuhi | T15 SQLite dan provider-real |
| `PAT-MIG3A-AC-07` — urutan kelahiran tetap | Terpenuhi | T16 SQLite dan provider-real |
| `PAT-MIG3A-AC-08` — pemeriksa dua jalur; `overallGuardReady` mensyaratkan keduanya | Terpenuhi | `PatientIdentifierAllocationService.cs` baris 209–285; `RsmmcIdentifierReservationProfile.cs` baris 193–204; T11–T14 |
| `PAT-MIG3A-AC-09` — bukti provider-real PostgreSQL 15 | Terpenuhi — 23/23 `PASS` pada `6ae9c251` (pemilik, 19 Oktober 2026); kasus yang memanggil `AllocateReservedAsync` dijalankan ulang sesudah koreksi `F-07` | `PendaftaranBayiPostgresSungguhanTests.cs`, `AlokasiPostgresSungguhanTests.cs`; bagian 5 dan 9 |
| `PAT-MIG3A-AC-10` — tanpa regresi; build; QBE | Terpenuhi | Sesudah koreksi: `70` lulus / `0` gagal / `7` dilewati regresi `BE-PAT-MIG-003` dan `73` lulus / `0` gagal regresi `BE-PAT-MIG-001` lulus; Release `0` error; QBE `PASS` (bagian 9) |
| `PAT-MIG3A-AC-11` — tanpa EF migration | Terpenuhi | `has-pending-model-changes` |
| `PAT-MIG3A-AC-12` — runtime `PAT-GATE-005` generasi 2 | **Belum** — milik gerbang runtime | — |

| Definition of Done | Status |
| --- | --- |
| Keputusan dikunci pemilik | Terpenuhi — 19 Oktober 2026 |
| Source sesuai kontrak `1.3.0` | Terpenuhi |
| Build, QBE, test SQLite | Terpenuhi |
| Uji provider-real | Terpenuhi — 23/23 pada `6ae9c251`; uji ulang kasus terdampak `F-07` diminta (bagian 9) |
| Tanpa EF migration | Terpenuhi |
| Laporan | Terpenuhi — berkas ini |
| Review source pemilik/leader | **Belum** — review final pemilik menghasilkan `F-07`/`F-13`/`F-14`, dikoreksi pada bagian 9 |
| `PAT-GATE-005` generasi 2 | **Belum** — sesudah review dan deploy terotorisasi |

---

## 7. Arsitektur dan keputusan desain

**Mengapa bukan "panggil allocator normal di semua environment".** Itu akan mengganti `NumberSeries`
bayi di Development dan Produksi dengan algoritma "nomor kosong terkecil" milik pendaftaran normal —
dilarang `PAT-DEC-070` (b). Karena itu cabangnya ditentukan aturan tunggal `ResolveAllocationMode`:
hanya `RESERVED_FLOOR` (Staging persis, guard aktif) yang memakai allocator bersama.

**Mengapa satu method alokasi bersama, bukan salinan.** `AllocateReservedAsync` adalah cabang berlantai
`AllocateAsync` yang sudah terbukti provider-real 10/10, dipindah tanpa perubahan algoritma. Dengan satu
method, pendaftaran normal dan bayi memakai kunci, lantai, dan pembacaan nomor terpakai yang sama, sehingga
tidak ada dua algoritma yang dapat menyimpang.

**Mengapa guard dinilai di awal.** Penolakan terjadi sebelum basis data disentuh — dibuktikan dengan nol
perintah SQL — dan keputusan yang sama dipakai sampai alokasi, sehingga konfigurasi yang dimuat ulang di
tengah pendaftaran tidak dapat menghasilkan keputusan campuran.

**Urutan kunci di Staging dengan guard aktif.**

| No | Kunci | Bentuk | Ruang lingkup | Dilepas |
| ---: | --- | --- | --- | --- |
| 1 | `hashtext('PAT_NEWBORN_KEY_<hash>')` | Satu kunci 64-bit, `xact` | Transaksi bayi | *Commit*/*rollback* |
| 2 | `hashtext('PAT_NEWBORN_CASE_<id>')` | Satu kunci 64-bit, `xact` | Transaksi bayi | *Commit*/*rollback* |
| 3 | `(1346458697, 1313821261)` | Dua kunci Int32, `xact` | Transaksi bayi, sesudah validasi | *Commit*/*rollback* |
| — | Baris `PatNewbornBirthSequence`, indeks unik `MstPatient` | Kunci baris/indeks | `SaveChanges`, sesudah kunci 3 | *Commit*/*rollback* |

Pendaftaran normal hanya mengambil kunci 3, lalu insert. Tidak ada jalur yang memegang kunci 3 lalu
meminta kunci 1 atau 2, dan pendaftaran bayi tidak memegang kunci baris apa pun saat menunggu kunci 3
(entity baru masih di *change tracker*). Ruang kunci satu-kunci dan dua-kunci PostgreSQL tidak tumpang
tindih. Kesimpulan: **tidak ada siklus *deadlock*.** Di luar Staging persis kunci 3 tidak diambil;
`NumberSeriesAllocator` memakai koneksi sendiri dengan kunci `NUM_SERIES_*` yang lepas sebelum kembali.

**Apa yang dibuktikan pemeriksa.** Pemeriksa tidak menjalankan alokasi (baca-saja, tanpa kunci). Ia
melaporkan keputusan cabang yang dipakai masing-masing jalur — `ResolveAllocationMode` untuk normal,
`NewbornPatientRegistrationService.ResolveIdentifierAllocationMode` untuk bayi — untuk konfigurasi yang
berlaku. Build tanpa task ini tidak mengembalikan field itu sama sekali.

**Allocator berlantai tidak memercayai guard pemanggil (`F-07`).** `AllocateReservedAsync` publik dan
menerima objek guard. Agar guard buatan — misalnya `Active` di Production — tidak dapat menjalankan
alokasi berlantai di luar `Staging` persis, method itu memeriksa ulang sebelum SQL apa pun: environment
yang sedang berjalan, asal guard, keadaan guard, dan konfigurasi saat ini. Pemanggil sah — `AllocateAsync`
dan `RegisterAsync` — selalu memakai guard hasil `EvaluateGuard()` pada service yang sama, sehingga
perilakunya tidak berubah. Konstruktor record guard tidak dipersempit: record berposisi tidak dapat diberi
konstruktor `internal` tanpa mengubah bentuknya, dan test memakainya; pertahanan utamanya pemeriksaan runtime.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `298` warning build lama di berkas lain; tidak ada dari berkas yang diubah |
| Masalah yang diketahui | **(1)** ~~Bukti provider-real belum dijalankan (`PAT-RSK-039`)~~ — ditutup: 23/23 `PASS`. **(2)** Di luar Staging persis, pendaftaran normal dan bayi tetap tidak saling diserialkan, seperti sebelum task (`PAT-RSK-040`, disengaja). **(3)** Koreksi `F-07` belum dibuktikan ulang pada PostgreSQL (bagian 9) |
| Risiko tersisa | Bila build `3a0189c2` sudah berjalan di Staging sebelum task ini dideploy, bayi yang sempat didaftarkan mungkin memakai nomor cadangan (`PAT-RSK-042`) — perlu pemeriksaan read-only pemilik. Jalur insert baru di masa depan tidak otomatis terlihat pemeriksa (`PAT-RSK-041`). Sesudah deploy tanpa `Enabled = true`, pendaftaran normal **dan** bayi Staging ditolak `503` — disengaja |
| Perubahan sampingan | `NONE`. Tiga berkas test baru disamakan ke CRLF tanpa BOM seperti berkas sekitarnya |
| Interupsi | `NONE` |
| Status Git | Branch `BE-PAT-MIG-003A`, commit WIP `6ae9c251` di atas `98fdc69f`. Koreksi review final belum di-*commit*: lihat bagian 9. Tidak ada *stage*, *commit*, atau *push* oleh agent |
| Langkah berikutnya | **(1)** Pemilik menjalankan ulang uji provider-real untuk kasus yang memanggil `AllocateReservedAsync` (bagian 9). **(2)** Review PR pemilik. **(3)** Otorisasi terpisah *commit*/PR, lalu deploy Staging dengan migration `20261016080745_ReviseNewbornAdmissionFlow` sudah diterapkan. **(4)** `PAT-GATE-005` generasi 2. **(5)** Pemeriksaan read-only `PAT-RSK-042` dan pengukuran baseline `MstPatient` (`PAT-DEC-077`). `BE-PAT-MIG-002` belum boleh dimulai |

---

## 9. Koreksi review final `F-07` / `F-13` / `F-14` — 19 Oktober 2026

Review final read-only pemilik atas `6ae9c251` (rebase ke `98fdc69f`) menghasilkan tiga temuan `WARNING`.
Ketiganya dikoreksi di sini; tidak ada perubahan source lain.

| Temuan | Koreksi | Bukti |
| --- | --- | --- |
| `F-07` — `AllocateReservedAsync` memercayai guard buatan pemanggil | `EnsureReservedAllocationAllowed` (`PatientIdentifierAllocationService.cs` baris 293–322), dipanggil di baris 146 sebelum transaksi diperiksa dan sebelum SQL apa pun: (1) `RsmmcPilotEnvironmentGate.IsAllowed(_environment.EnvironmentName)`; (2) `guard.EnvironmentName` sama persis (ordinal) dengan environment berjalan; (3) guard `ReservedFloor`; (4) `EvaluateGuard()` saat ini aktif. Pelanggaran → `InvalidOperationException` deterministik; guard `null` → `ArgumentNullException` | `AlokasiBerlantaiGuardBuatanTests`: `17` lulus / `0` gagal / `0` dilewati |
| `F-13` — dokumen bukti basi | Laporan ini, roadmap, traceability, dan manifest mencatat 23/23 `PASS`, smoke 1/1, 4259,7 detik, 0 `pat_proof_*` tersisa, rebase `98fdc69f`, commit `6ae9c251`; `PAT-MIG3A-AC-09` terpenuhi; `PAT-RSK-039` ditutup | Diff dokumen |
| `F-14` — prasyarat server bukti tidak terdokumentasi | `max_locks_per_transaction = 2048` dicatat pada remarks `PostgresSekaliPakai`, bagian 5.2 laporan ini, dan kontrak B15 (klarifikasi non-API, versi tetap `1.3.0`; hash manifest dihitung ulang) — hanya untuk server uji sekali pakai | Diff dokumen |

**Uji koreksi T-F07.** Setiap penolakan diuji dengan transaksi terbuka dan penghitung perintah SQL: 0
perintah SQL, tanpa hasil alokasi, jumlah pasien tetap, 0 baris `NumberSeries`.

| ID | Skenario | Hasil yang dibuktikan |
| --- | --- | --- |
| T-F07-01 | Runtime `Production`, guard buatan `Active` bernama `Staging` / `Production` | Ditolak sebelum SQL |
| T-F07-02 | Runtime `Development`, guard buatan `Active` bernama `Staging` / `Development` | Ditolak sebelum SQL |
| T-F07-03 | Runtime `staging`, guard buatan `Active` bernama `Staging` / `staging` | Ditolak sebelum SQL |
| T-F07-04 | Runtime `Staging` persis, guard bernama `staging`, `STAGING`, `Staging `, `Production`, kosong, `null` | Ditolak sebelum SQL |
| T-F07-05 | Runtime `Staging` persis, guard hasil `EvaluateGuard()` | Tetap `PAT-RSMMC-705250` / `00-89-01-88`; jalur sah menjalankan SQL (pembanding penghitung) |
| T-F07-06 | Runtime `Staging` persis, konfigurasi `false` / hilang / salah bentuk, guard buatan `Active` | Ditolak sebelum SQL (pertahanan berlapis) |
| Guard `null` | Runtime `Staging` persis | `ArgumentNullException`, 0 perintah SQL |

**Validasi sesudah koreksi.**

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet test` PatientManagementTests (seluruhnya) | `206` lulus / `0` gagal / `16` dilewati (Total 222; 16 = provider-real tanpa server) | `PASS` |
| `--filter AlokasiBerlantaiGuardBuatanTests` | `17` lulus / `0` gagal / `0` dilewati | `PASS` |
| `--filter PendaftaranBayi` (test SQLite `BE-PAT-MIG-003A`) | `46` lulus / `0` gagal / `9` dilewati | `PASS` |
| Regresi `BE-PAT-MIG-003` (ReservasiNomor lama) | `70` lulus / `0` gagal / `7` dilewati | `PASS` |
| Regresi `BE-PAT-MIG-001` | `73` lulus / `0` gagal | `PASS` |
| `dotnet build -c Release --no-incremental` | Berhasil, `0` error, `297` warning; 0 warning dari berkas yang diubah | `PASS` |
| QBE `Strict` (WorkingTree) | WorkingTree: 3 berkas, `VIOLATION 0`, `PASS`; GitRange `98fdc69f..6ae9c251`: 9 berkas, `VIOLATION 0`, `PASS` | `PASS` |
| `has-pending-model-changes --context ApplicationDbContext` | "No changes have been made to the model since the last migration." | `PASS` |
| Uji provider-real sesudah koreksi | Tidak dijalankan agent — server tidak tersedia | `NOT RUN` |

**Uji provider-real yang wajib dijalankan ulang** karena `AllocateReservedAsync` berubah — seluruh kasus yang
memanggilnya, langsung atau lewat `AllocateAsync` / `RegisterAsync` di `Staging` persis aktif:

| Kelas | Kasus |
| --- | --- |
| `AlokasiPostgresSungguhanTests` (`BE-PAT-MIG-003`) | `A_B_StagingAktif_LantaiDanKunciDatabase`, `E_StagingAktif_CelahDiAtasLantai`, `C_D_Bersamaan_TidakAdaKodeAtauMrnGanda` |
| `PendaftaranBayiPostgresSungguhanTests` (`BE-PAT-MIG-003A`) | `T6_StagingAktif_NomorBayiDiAtasLantaiTanpaNumberSeries`, `T10_StagingAktif_BayiMenungguKunciPatiNormLaluMelepasnyaSaatCommit`, `T8_BayiBersamaan_KasusBerbeda_TidakAdaNomorGanda`, `T7_T9_NormalDanBayiBersamaan_TidakAdaNomorGandaDanTidakDiBawahLantai`, `T15_StagingAktif_KunciIdempotencySamaBersamaan_SatuPasien`, `T16_StagingAktif_KembarBersamaanSatuKasus_UrutanSatuDanDua` |

Sembilan kasus itu wajib. Empat belas kasus lainnya tidak memanggil `AllocateReservedAsync` — jalur di luar
Staging, gagal tertutup, pemeriksa baca-saja, dan indeks unik — tetapi menjalankan seluruh 23 kasus lebih
sederhana dan dianjurkan, karena satu perintah `--filter Kategori=ProviderReal` mencakup semuanya.

**BE-PAT-MIG-003A SOURCE READY — FINAL REVIEW CORRECTIONS APPLIED, PROVIDER-REAL RERUN REQUIRED**
