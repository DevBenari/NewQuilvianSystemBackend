# Laporan Perubahan Backend — `RJ-DOC-REV-BE-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-015` |
| Judul | Klasifikasi dan alokator nomor antrean |
| Slice | Amendment AQ — antrean prioritas, member, dan privasi layar publik |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `17` |
| Trace | `RJ-DOC-DEC-055`, `056`, `059`, `060`, `061`; fakta `F-AQ-1`..`6` |
| Contract version | Delta aditif terhadap kontrak Patient Encounter, Membership Tier, Nurse/Doctor Queue (field baru saja) |
| Dependency | — |
| Klasifikasi | `HEAVY` — dua repository 2, berkas diperiksa >20 2, berkas diubah >8 2, logika kompleks 2, kontrak berubah 2, schema 2, keamanan terkait 1 = 13; diturunkan ke `HEAVY` karena dipecah per task (`BE-015`, `BE-016`, `FE-016`) |
| Task mode | `CROSS-REPO MODE`, backend lebih dulu (`RJ-DOC-DEC-060`) |
| Target tulis | `NewQuilvianSystemBackend` (source, migration, laporan, roadmap) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7d5f354f` (`sukmagp`), belum di-commit |
| Tanggal | 2026-10-08 |
| Status | Selesai — build, EF, QBE Strict, migration diterapkan ke `QuilvianNewDevSukma`, uji runtime `26/26 PASS` (bersama `BE-016`) |

## 1. Masalah yang diperbaiki

- Nomor antrean dibuat dengan `MAX(QueueNumber)+1` di controller. Tidak ada nomor cadangan untuk pasien prioritas, dan dua poliklinik dalam satu Nurse Station Cluster tidak terlindungi database dari nomor ganda (`F-AQ-3`).
- `IsPriorityQueue` tidak pernah diisi, walaupun tier membership sudah punya `PriorityQueue` (`F-AQ-1`).
- Atas instruksi pemilik saat pengerjaan ("jangan gunakan prefix Trx"), entity `TrxQueue` dinormalkan menjadi `RegQueue` (`RJ-DOC-DEC-061`).

## 2. Proses bisnis

1. Petugas (`POST /patient-encounters/admin`) atau kiosk (`POST /patient-encounters/kiosk`) mendaftarkan pasien. Keduanya melewati proses `CreateEncounterCoreAsync` yang sama.
2. Backend mencari membership aktif pasien pada tanggal kunjungan: status `Active`, belum dihapus, `JoinDate` ≤ tanggal, `ExpiredDate` kosong atau ≥ tanggal, tier aktif. Bila lebih dari satu: `ActivePatientMembershipId` pasien, lalu `IsPrimary`, lalu `PriorityLevel` tertinggi.
3. Hasilnya: member atau bukan, prioritas (`PriorityQueue` tier), audience (`QueueAudience` tier), dan privasi layar (`PublicDisplayMode` tier). Bukan member = Regular, bukan prioritas, `Default`.
4. Alokator mengunci tanggal + service unit (advisory lock transaksi), lalu membaca semua nomor yang pernah terbit di cakupan yang sama dengan perilaku lama (cluster → poliklinik → service unit), termasuk yang batal, tidak hadir, selesai, atau dihapus.
5. Prioritas mengambil nomor cadangan terkecil yang belum terpakai. Bila habis, mengambil nomor biasa berikutnya dan tetap `IsPriorityQueue = true`. Reguler mengambil nomor non-cadangan terkecil yang belum terpakai.
6. Klasifikasi disimpan sebagai snapshot di `RegQueue`. Perubahan membership sesudahnya tidak mengubah antrean.

Contoh pada antrean kosong (cadangan `1, 3, 5, 10, 15`): Reguler A → `02`, Reguler B → `04`, Prioritas A → `01`, Reguler C → `06`, Prioritas B → `03`, Prioritas C → `05`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientEncounterController` (alur create, `GenerateQueueNumberAsync`), `TrxQueue`/configuration, `MstMembershipTier`, `MstPatientMembership`, `MstPatient`, `MembershipTierController`/DTO, `NurseStationQueueController`, `DoctorQueueController`, `QueueRealtimeService`, `PatientEncounterNumberService` (pola lock), `Program.cs`, `appsettings.json`, migration rename `RenameTrxPatientEncounterGuarantorToRegPrefix` (preseden).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Administrator/MasterData/Enums/QueueAudience.cs`, `PublicDisplayMode.cs` | Enum baru policy tier |
| `Areas/Administrator/MasterData/Models/MstMembershipTier.cs` + `MstMembershipTierConfiguration.cs` | Kolom `QueueAudience` (bawaan Member) dan `PublicDisplayMode` (bawaan Default) |
| `Areas/Administrator/MasterData/DTOs/MembershipTierDtos.cs`, `Controllers/MembershipTierController.cs` | Field response, request opsional (update kosong = nilai lama), validasi enum, opsi pada `filters/metadata`, metadata form |
| `Areas/HealthServices/RegistrationManagement/Models/RegQueue.cs` (dulu `TrxQueue.cs`) + `RegQueueConfiguration.cs` | Rename `TrxQueue` → `RegQueue`; kolom snapshot (`QueuePriorityLevelSnapshot`, `QueueAudienceSnapshot`, `PublicDisplayModeSnapshot`, `PatientMembershipIdSnapshot`, `MembershipTierIdSnapshot`, `MembershipTierCodeSnapshot`, `PriorityReasonCode`), `QueueScopeKey`, unique index `(QueueDate, ServiceUnitId, QueueScopeKey, QueueNumber)` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueClassificationService.cs` | Service klasifikasi canonical |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueNumberAllocator.cs` | Alokator + `OutpatientQueueNumberOptions` (satu-satunya tempat nilai bawaan cadangan) |
| `Controllers/PatientEncounterController.cs`, `DTOS/PatientEncounterDtos.cs` | Memakai service baru, menghapus `GenerateQueueNumberAsync`/`GetNextQueueNumberAsync`, response `queueClassification` |
| `Program.cs`, `appsettings.json` | Registrasi DI dan `HealthServices:Registration:QueueNumber:ReservedPriorityNumbers` |
| 27 berkas lain (klinis, antrean, kiosk, seeder OR, `ApplicationDbContext`) | Rename mekanis `TrxQueue`/`TrxQueues` → `RegQueue`/`RegQueues`; perilaku tidak berubah |
| `Migrations/20261008043304_AddOutpatientQueueClassificationAndRenameRegQueue.cs` + Designer + snapshot | Lihat 3.3 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif. `PatientEncounterCreateResponse.queueClassification` (`isMember`, `isPriorityQueue`, `isReservedPriorityNumber`, `queueAudience(Name)`, `publicDisplayMode(Name)`). Membership Tier: `queueAudience`, `publicDisplayMode` (+`Name`) pada response dan request (opsional), `queueAudienceOptions`, `publicDisplayModeOptions` pada metadata. Nomor antrean tetap `int`, format kode tetap `{huruf}{nomor:D3}` |
| Database | Migration `20261008043304_AddOutpatientQueueClassificationAndRenameRegQueue`: (1) rename tabel `TrxQueue` → `RegQueue` beserta PK, 21 FK, dan index lewat katalog Postgres — scaffold EF `DropTable`+`CreateTable` diganti agar data utuh (QBE-DB-002); (2) 8 kolom `RegQueue`, backfill `QueueScopeKey = COALESCE(ClinicId, ServiceUnitId)`, unique index baru; (3) 2 kolom `MstMembershipTier` (tier `TierType = Regular` → audience Regular); (4) 1 kolom `MstQueueDisplayDevice` (`BE-016`). Diterapkan ke `QuilvianNewDevSukma` saja: 213 baris antrean utuh, tanpa sisa nama `TrxQueue` |
| Keamanan/Auth | Tidak ada endpoint baru; atribut akses tidak berubah. Klasifikasi tidak dapat dikirim dari frontend |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/admin` | Pendaftaran petugas; kini mengembalikan `queueClassification` | `PatientEncounter : Create` |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/kiosk` (dan tanpa sufiks) | Pendaftaran kiosk; aturan nomor sama | Policy `KioskRead`, `PatientEncounter : Create` |

#### Administrator / Master Data / Membership Tier

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/administrator/master-data/membership-tiers/filters/metadata` | Opsi `queueAudienceOptions`, `publicDisplayModeOptions` | `MembershipTier : Read` |
| `POST`/`PUT` | `/api/v1/administrator/master-data/membership-tiers[/{id}]` | Menyimpan `queueAudience`, `publicDisplayMode` | `MembershipTier : Create`/`Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false` | `0 Error(s)`, `239 Warning(s)` (sama dengan baseline laporan sebelumnya) | `PASS` | Keluaran build |
| `dotnet build` Debug | Salin `bin/Debug/.../QuilvianSystemBackend.exe` gagal `MSB3021`, dikunci server dev pemilik (port 7184) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `dotnet ef database update` (`QuilvianNewDevSukma`) | Hanya migration ini yang pending; `Done.` | `PASS` | Keluaran perintah |
| Pemeriksaan katalog sesudah migration | 0 relasi bernama `TrxQueue`; 213 baris `RegQueue`; `QueueScopeKey` kosong 0; 22 constraint ber-nama `RegQueue` | `PASS` | Query baca-saja |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict` × 54 berkas | `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` | Keluaran checker |
| `AUTOMATED TEST` | Pola Bank Darah, tanpa project test | `NOT RUN` | — |

### Uji runtime

Lingkungan: build Release di scratchpad, `https://localhost:7185`, `Development`, `Runtime__Role=Web`,
`QueueVoice__Enabled=false`, `HealthServices__Registration__BlockActiveEncounter=false` (agar satu pasien uji
dapat didaftarkan berulang), DB `QuilvianNewDevSukma`, login SuperAdmin seed (kredensial tidak dicetak).
Tier dan membership uji bertanda `UJI-AQ-*`; poliklinik Poli Bedah (cluster 6 poliklinik), tanggal kosong
`2026-11-20`/`2026-11-21`. Script: `rt_aq.py`, `rt_aq_t15.py`, `rt_aq_cleanup.py`; hasil `rt_aq_result.json` (scratchpad).

| ID | Skenario wajib | Hasil | Status |
| --- | --- | --- | --- |
| T1 | Antrean kosong + Reguler | `B002` | `PASS` |
| T2 | Antrean kosong + Prioritas | `B001`, `isReservedPriorityNumber=true` | `PASS` |
| T3 | Reguler 02 ada + Prioritas (kiosk) | `B001` | `PASS` |
| T4 | 01 prioritas + Reguler | `B002` | `PASS` |
| T5 | 01 prioritas + prioritas berikut | `B003`; urutan lanjutan `3, 5, 10, 15` | `PASS` |
| T6 | Cadangan habis | `B007`, `isPriorityQueue=true`, `isReservedPriorityNumber=false` | `PASS` |
| T7 | Membership kedaluwarsa (`ExpiredDate 2026-10-01`) | `B008`, bukan member, Regular | `PASS` |
| T8 | Member non-prioritas | `B009`, Member, bukan prioritas | `PASS` |
| T12 | Kiosk dan petugas bergantian | Kiosk `04`, petugas `06`, campuran `3, 5, 10, 15` | `PASS` |
| T13 | 4 pendaftaran simultan (kiosk + petugas) | `13, 14, 16, 17` — unik, melompati `15` | `PASS` |
| T14 | Kunjungan nomor `02` dibatalkan | Reguler berikut `B012`, `02` tidak dipakai ulang | `PASS` |
| T15a | Snapshot: membership prioritas dihapus sesudah antrean terbit | 6 antrean tetap `isPriorityQueue`/`isMemberQueue` | `PASS` |
| R0 | Metadata tier | Opsi `Regular, Member` dan 4 mode | `PASS` |

Skenario 9–11 dan 15 dicatat di [`RJ-DOC-REV-BE-016`](RJ-DOC-REV-BE-016.md). Total run gabungan: **26/26 `PASS`**.

**Keadaan sesudah run:**
- **Database:** 21 kunjungan uji dibatalkan (`PATCH cancel`, `200`); antreannya tersisa sebagai baris batal pada `2026-11-20`, `2026-11-21`, dan hari ini. 7 membership, 6 tier (termasuk sisa run pertama yang berhenti karena dokter nonaktif), dan 3 display dihapus lewat `DELETE` (`200`, soft-delete). 3 akun login display uji tetap ada di `AspNetUsers`.
- **Proses:** app uji 7185 dihentikan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Skenario wajib 1–8 | Terpenuhi | T1–T8 |
| Skenario 12 kiosk dan petugas sama | Terpenuhi | T3, T12, T5b; satu proses `CreateEncounterCoreAsync` |
| Skenario 13 simultan tanpa duplikat | Terpenuhi | T13; advisory lock + unique index |
| Skenario 14 batal tidak dipakai ulang | Terpenuhi | T14 |
| Nomor cadangan configurable, tidak tersebar | Terpenuhi | `OutpatientQueueNumberOptions` + `appsettings.json` |
| Cakupan nomor tidak berubah | Terpenuhi | `ResolveScopeAsync` = logika lama `GenerateQueueNumberAsync` |
| Snapshot tidak berubah oleh membership | Terpenuhi | T15a |
| Tanpa prefix `Trx` (`RJ-DOC-DEC-061`) | Terpenuhi | Rename `RegQueue`, QBE `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Scaffold EF untuk rename bersifat destruktif; sudah diganti rename katalog. Jangan men-scaffold ulang migration ini |
| Masalah yang diketahui | Entity klinis `TrxDoctorConsultation`, `TrxPatientAssessment`, dll. masih ber-prefix `Trx` (di luar cakupan; hanya tipe navigasinya yang ikut berubah) |
| Risiko tersisa | (1) Antrean lama sebelum migration memakai kunci cakupan per poliklinik; antrean baru per cluster. Alokator tetap membaca seluruh cakupan, jadi tidak ada nomor ganda, tetapi index baru hanya melindungi baris baru per cluster. (2) Lock per tanggal + service unit menyerialkan pendaftaran satu service unit — wajar untuk volume pendaftaran |
| Perubahan sampingan | `NONE` |
| Interupsi | Run uji pertama berhenti (dokter template nonaktif); datanya dibersihkan pada pembersihan akhir |
| Status Git | 62 entri `git status --short` (backend), belum di-commit |
| Langkah berikutnya | `RJ-DOC-REV-FE-016`; isi data induk membership MMC setelah terverifikasi (`RJ-DOC-DEC-059`) |
