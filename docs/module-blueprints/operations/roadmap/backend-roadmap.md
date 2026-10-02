# Backend Roadmap — Modul Operasi

Kontrak terkunci: `opr-api-v1`, `opr-state-v1`, `opr-integration-v1`.

## Status per 28 September 2026

Bukti: `docs/testing/OPR-backend-runtime-validation.md` (runtime/API) dan
`Tests/QuilvianSystemBackend.OperatingRoomTests` (70 uji, seluruhnya lulus).

| Task ID | Status | Dasar |
|---|---|---|
| `BE-OPR-001` | Selesai | Struktur terverifikasi di basis data: 14 tabel `Opr*`, 23 FK seluruhnya `RESTRICT`/`NO ACTION`, 29 unique index, 4 kolom concurrency |
| `BE-OPR-002` | Selesai | Migration additive sudah dijalankan pada basis data pengembangan; tidak ada migration baru pada tahap ini |
| `BE-OPR-003` | Selesai | Happy/invalid/duplicate/idempotency/concurrency lulus di runtime dan di uji otomatis; `OPR002`, `OPR013` terbukti |
| `BE-OPR-004` | Selesai | Penjadwalan, tabrakan `OPR003`, lima permintaan paralel menyisakan satu jadwal, riwayat jadwal lama tersimpan, dan pembuatan kasus di bawah aturan klinis penuh kini terbukti memakai akun `opr.bedah` |
| `BE-OPR-005` | Selesai | Seluruh gerbang terbukti, termasuk `Scheduled` → `Ready` di bawah aturan penuh dengan tiga peran dipegang tiga akun berbeda |
| `BE-OPR-006` | Selesai | Illegal start ditolak, catatan final immutable, addendum append-only, pembatalan sesudah mulai ditolak |
| `BE-OPR-007` | Selesai | `Completed` hanya setelah catatan final, recovery `Released`, dan serah terima diterima; jalur kegagalan keselamatan ditolak dengan kode yang benar |
| `BE-OPR-008` | Selesai | Validasi quantity/serial, retry tidak menggandakan, koreksi beralasan, dan keunikan serial implant per kasus (`OPR014`) terbukti di service maupun runtime |
| `BE-OPR-009` | Selesai | Amplop kejadian outbox lengkap dan dibekukan pada `PayloadJson`; satu kejadian satu pesan dijaga indeks unik `EventId` dan `(Destination, IdempotencyKey)`; pengulangan tidak menggandakan; kegagalan pengiriman tidak membatalkan transaksi klinis; pesan dapat dibaca dan diantrekan ulang. Mapping ke consumer eksternal tetap milik integration layer |
| `BE-OPR-010` | Selesai | Filter, rentang tanggal, paging, batas `pageSize`, validasi rentang utilization, dan laporan material terbukti |
| `BE-OPR-011` | **Selesai — kurang tes** | Proyek uji ada dan hijau untuk permission matrix, regresi state, konkurensi, idempotensi, audit/privasi, serta kontrak outbox. Sisi pembangunan tidak menyisakan pekerjaan. Yang tersisa hanya pembuktian penolakan `403` per peran, dan itu diserahkan ke analis penguji: membuktikannya menuntut akun per peran beserta pemetaan izin milik lingkungan, bukan sesuatu yang boleh dibuat modul ini sendiri. Temuan kolom pengguna pada baris audit dipindahkan menjadi task Core/shared |

Satu hal yang menyertai `BE-OPR-009` dan bukan pekerjaan modul Operasi: Billing masih terdaftar
pada `BlockedDestinations`. Itu menyatakan kesiapan **penerima**, bukan kesiapan modul ini.
Pesannya sudah terbentuk lengkap dan menunggu di antrean; yang belum ada adalah pihak yang
mengambilnya. Selama itu, rekonsiliasi tujuan Billing masih dilakukan orang.

Dua catatan yang tidak boleh hilang saat task ini ditutup. Keduanya sudah punya pemilik, dan
tidak satu pun berupa pekerjaan pembangunan modul Operasi yang tertinggal.

1. **Kolom pengguna pada baris audit terisi id kasus.** `LoggerService` mengambil nilainya dari
   `UserId` lalu `Id` pada data yang dikirim, sedangkan data audit Operasi memuat `Id` kasus.
   Pertanggungjawaban pelaku karena itu hanya terbaca dari `OprStatusHistory.CreateBy`.
   `LoggerService` dipakai seluruh modul, sehingga perbaikannya bukan keputusan modul Operasi
   sendiri. Dipindahkan menjadi task Core/shared:
   `docs/engineering/task-core-logger-audit-user.md`.
2. **Penolakan izin per peran diserahkan ke analis penguji.** `opr.bedah`, `opr.anestesi`, dan
   `opr.perawat` semuanya dibuat berperan SuperAdmin oleh seeder demo — disengaja, supaya alur
   klinis dapat dicoba tanpa menyiapkan pemetaan izin lebih dulu — sehingga seluruhnya selalu
   lolos. Membuktikan `403` menuntut akun per peran beserta pemetaan izin milik lingkungan.
   Membuatnya sendiri dari dalam modul berarti mengarang data privilege, dan itu tidak dilakukan.
   Kontrak izin yang terpasang pada setiap endpoint sudah dijaga otomatis oleh
   `PermissionMatrixTests`; yang diserahkan adalah pembuktian penolakannya saat berjalan.

| Task ID | Outcome | Trace | Cakupan dan reuse | Dependency | Acceptance criteria/verifikasi | Risiko/DoD |
|---|---|---|---|---|---|---|
| `BE-OPR-001` | Foundation domain dapat dikompilasi | `OPS-REQ-001/002`, `OPS-CON-001..015` | Folder canonical, enum, model, configuration, DbSet, services skeleton; reference existing patient/encounter/procedure/consent/room/workforce | Blueprint approved; QBE preflight | Build berhasil; configuration test memeriksa FK Restrict, unique/index, concurrency; tidak ada master duplikat | DoD: source+targeted test, tanpa migration execution |
| `BE-OPR-002` | Schema additive tersedia | Semua entity baru | Generate dan review migration untuk seluruh `Opr*` | `BE-OPR-001`; **otorisasi migration generation terpisah** | Migration hanya additive; snapshot sesuai; rollback plan direview; tidak menjalankan database | DoD: migration source + review QBE database |
| `BE-OPR-003` | Dokter dapat membuat, membaca, dan memperbarui permintaan | `OPS-REQ-001`, `OPS-DEC-003/014` | POST/GET/PUT case; reuse `TrxPatientProcedure`; history `Requested`; availableActions | `BE-OPR-001`, database schema tersedia untuk runtime test | Happy/invalid/duplicate/idempotency/concurrency tests; `OPR001/002/012`; Swagger sesuai contract | DoD: endpoint+auth+audit+tests |
| `BE-OPR-004` | Koordinator dapat menjadwalkan ruang dan tim tanpa benturan | `OPS-REQ-003/004`, `OPS-DEC-004/016/017` | Schedule/postpone/reschedule, revision history, overlap termasuk buffer, minimum team | `BE-OPR-003`; credential adapter penuh **BLOCKED** bila owner belum tersedia | Parallel schedule test hanya satu berhasil; `OPR003/004`; jadwal lama tetap ada | DoD: service transaksi+indexes+tests; unresolved credential terlihat |
| `BE-OPR-005` | Tim menyelesaikan persiapan hingga `Ready` | `OPS-REQ-005`, `OPS-DEC-005/006/018` | Preparation workspace, consent adapter, checklist phases, sign-off, emergency bypass, automatic Ready | `BE-OPR-004`; consent existing | Tidak bisa Ready bila prasyarat kurang; bypass beralasan; tiga sign-off menghasilkan satu transition/history | DoD: endpoint+state/negative tests+privacy log test |
| `BE-OPR-006` | Operasi dapat dimulai, dicatat, difinalisasi, dan diamandemen | `OPS-REQ-006/009`, `OPS-DEC-010/011/019/022` | Start, execution record, outcome `StoppedEarly`, finalization, addendum | `BE-OPR-005` | Illegal start ditolak; final immutable; addendum append-only; cancel setelah start ditolak | DoD: authorization dokter bedah+audit+tests |
| `BE-OPR-007` | Anestesi, recovery, dan handover menutup kasus | `OPS-REQ-006/008`, `OPS-DEC-012/019/021/025` | Anesthesia record, recovery decision, handover send/accept, automatic Completed | `BE-OPR-006`; downstream unit consumer dapat menyusul | Completed hanya setelah execution final + release + accepted; penerima/timestamp tercatat | DoD: end-to-end state test dan safety failure paths |
| `BE-OPR-008` | Pemakaian material/implant terlacak secara lokal | `OPS-REQ-007`, `OPS-DEC-009/020` | Usage/return/waste/correction, batch/serial, idempotency, pending delivery | `BE-OPR-006`; item resolver **BLOCKED** jika belum ada | Quantity/serial validation; retry tidak menggandakan usage; koreksi beralasan | DoD: ledger lokal+contract test, bukan mutasi stok langsung |
| `BE-OPR-009` | Handoff Billing/Inventory idempotent dan dapat direkonsiliasi | `OPS-REQ-007/010`, `OPS-INT-001/002` | Integration delivery, retry, reconciliation, create/correct/reverse | `BE-OPR-007/008`; **BLOCKED** owner API Billing dan Inventory | Consumer contract tests; duplicate key satu efek; failure/retry visible | DoD: adapter nyata hanya setelah kontrak consumer disahkan |
| `BE-OPR-010` | Laporan operasional tersedia | `OPS-REQ-011`, `OPS-DEC-024` | Query laporan kasus, utilization, material/implant; AsNoTracking/paging | `BE-OPR-003..008` sesuai laporan | Filter/date/paging/permission/privacy/performance query tests | DoD: GET tanpa custom mutation log |
| `BE-OPR-011` | Modul hardened dan siap diuji ujung-ke-ujung | Semua requirement | Permission matrix, audit, privacy, observability, full state/concurrency/idempotency regression | Semua task backend yang tidak blocked | Semua acceptance matrix relevan lulus; build bersih; dependency blocked dilaporkan | DoD: evidence report, migration execution tetap terpisah |

Setiap task builder hanya boleh mengerjakan satu task ID dengan acceptance criteria dan write authority eksplisit.

---

## Audit status pelaksanaan — 24 September 2026

Audit **read-only** terhadap source pada commit `3e1de652`. Nol baris implementasi diubah.

> **Bagian di bawah sudah usang.** Ia adalah audit 24 September 2026, ditulis ketika repository
> belum memiliki test project sama sekali. Sejak 29 September 2026 test project sudah ada —
> `Tests/QuilvianSystemBackend.OperatingRoomTests`, 70 uji dan seluruhnya lulus — dan seluruh
> penilaian di bawah sudah digantikan tabel **Status per 29 September 2026** di awal berkas ini.
> Bagian ini disimpan sebagai jejak, bukan sebagai status yang berlaku.

Satu keterangan yang berlaku untuk seluruh task: **repository ini tidak memiliki test
project** — solution hanya memuat `QuilvianSystemBackend.csproj`. Setiap acceptance criteria
yang menuntut test karena itu tidak dapat dipenuhi siapa pun saat ini, dan itulah sebab
utama banyak task berstatus `Sebagian` walaupun sourcenya lengkap. Statusnya sengaja tidak
dinaikkan menjadi `Selesai`, karena menyatakan lulus tanpa bukti berarti mengarang.

| Task | Status | Bukti | Alasan penilaian |
| --- | --- | --- | --- |
| `BE-OPR-001` | **Sebagian** | 14 model `Models/Opr*.cs`; 14 configuration `Repositories/Configurations/HealthServices/OperatingRoomManagement/Opr*Configuration.cs`; enum `Enums/`; DbSet terdaftar; 10 service | Fondasi berdiri dan terkompilasi. Yang kurang hanya bukti test configuration (FK Restrict, unique/index, concurrency) yang diminta acceptance; concurrency token sendiri terpasang pada 4 configuration |
| `BE-OPR-002` | **Selesai** | `Migrations/20260821060256_AddOperatingRoomFoundation.cs` (13 CreateTable, 28 CreateIndex); `Migrations/20260904045422_AddOperatingRoomStockSourceAndMaterialUnit.cs` (1 CreateTable, 5 CreateIndex, 1 AddForeignKey) | Seluruhnya additive — nol DropTable/DropColumn pada `Up()`. 14 tabel `Opr*` pada snapshot cocok dengan 14 entity. Sesuai DoD, database tidak dijalankan |
| `BE-OPR-003` | **Sebagian** | `OperatingRoomCaseController` (`GET`, `GET {id}`, `POST`, `PUT {id}`, `PATCH {id}/start`, `PATCH {id}/cancel`); `OperatingRoomCaseService`; `AvailableActions` pada DTO dan service; `OprStatusHistory` ditulis | Seluruh endpoint dan `availableActions` ada, idempotency terpasang. Acceptance menuntut happy/invalid/duplicate/idempotency/concurrency **tests** — tidak ada |
| `BE-OPR-004` | **Sebagian**, dependency **Blocked** | `OperatingRoomScheduleController` (`GET schedule`, `GET schedule/history`, `PATCH schedule`, `PATCH postpone`); `OperatingRoomSchedulingService` dengan `bufferBefore`/`bufferAfter` dan `EnsureNoOverlapAsync`; `OperatingRoomCredentialResolver` | Penjadwalan, riwayat revisi, dan benturan termasuk buffer sudah ada. `OperatingRoomCredentialResolver` sengaja melaporkan tenaga tanpa data privilege apa adanya karena **owner integrasi kredensial belum ditetapkan** — persis dependency yang roadmap tandai BLOCKED |
| `BE-OPR-005` | **Sebagian** | `OperatingRoomPreparationController` (`GET`, `PUT checklists/{phase}`, `POST sign-offs`, `POST emergency-bypass`); `OperatingRoomPreparationService` memakai `PatientConsentType.Surgery/Anesthesia`; transisi otomatis ke `OprCaseStatus.Ready` | Workspace, consent adapter, fase checklist, sign-off, bypass darurat, dan `Ready` otomatis seluruhnya ada. Yang kurang bukti state/negative test dan privacy log test |
| `BE-OPR-006` | **Sebagian** | `OperatingRoomExecutionController` (`GET/PUT operation-record`, `POST operation-record/addenda`); `OperatingRoomExecutionService` dengan `FinalizeAction`, `EnsureFinalizable`, `FinalizedBy`; enum `OprCaseOutcome.StoppedEarly` | Start, pencatatan, finalisasi, outcome berhenti dini, dan addendum ada. Yang kurang bukti test illegal start, immutability final, dan append-only addendum |
| `BE-OPR-007` | **Sebagian** | `OperatingRoomRecoveryController` (`GET/PUT anesthesia-record`, `GET/PUT recovery`, `GET/POST handovers`, `PATCH handovers/{id}/accept`); `OperatingRoomRecoveryService` menetapkan `OprCaseStatus.Completed` beserta `OprStatusHistory` | Anestesi, recovery, handover kirim/terima, dan `Completed` otomatis ada. Yang kurang bukti test end-to-end state dan safety failure path |
| `BE-OPR-008` | **Sebagian** | `OperatingRoomMaterialController` (`GET/POST materials`); `OperatingRoomMaterialService`; enum `OprMaterialOutcome { Used, Returned, Wasted, Corrected }`; `OprMaterialUsage.BatchNumber`, `.SerialNumber`; idempotency terpasang | Keempat jenis pencatatan, batch/serial, dan koreksi setelah `Completed` ada. Yang kurang bukti test quantity/serial dan test retry tidak menggandakan usage. Item resolver yang roadmap tandai BLOCKED tidak terlihat sebagai adapter tersendiri |
| `BE-OPR-009` | **Blocked** | `OperatingRoomIntegrationController` (`POST inventory/dispatch`, `GET reconciliation`, `PATCH deliveries/{id}/attempts`, `PATCH deliveries/{id}/retry`); `OperatingRoomIntegrationService` memuat catatan eksplisit: *"Adapter nyata ke kedua consumer **belum dibangun** karena owner API-nya belum…"* | Kerangka pengiriman, percobaan ulang, dan rekonsiliasi ada dan idempotent. Adapter nyata ke Billing dan Inventory **tidak dapat dibangun** sampai kontrak consumer disahkan pemiliknya — sesuai DoD task ini |
| `BE-OPR-010` | **Sebagian** | `OperatingRoomReportController` (`GET operations`, `GET utilization`, `GET materials`); `OperatingRoomReportService` memakai `AsNoTracking` (5) dan paging (8 rujukan) | Ketiga laporan, `AsNoTracking`, dan paging ada. Yang kurang bukti test filter/date/paging/permission/privacy/performance |
| `BE-OPR-011` | **Belum** | `AccessPermission` terpasang pada seluruh controller (6/3/4/2/4/7/3/4/2 butir); nol berkas test | Permission matrix dan audit sudah berjalan, tetapi inti task ini adalah **regression penuh state/concurrency/idempotency beserta evidence report** — keduanya tidak ada, dan tidak mungkin ada tanpa test project |

### Ringkasan

> **Disegarkan 1 Oktober 2026.** Tabel audit per-task di atas mencatat keadaan 21 September dan
> dipertahankan sebagai catatan sejarah. Ringkasan di bawah ini keadaan sekarang.

| Status | Jumlah | Task |
| --- | :---: | --- |
| Selesai | 10 | `BE-OPR-001` sampai `BE-OPR-010` |
| Selesai — kurang tes pihak ketiga | 1 | `BE-OPR-011` |

Yang mencabut ketiga penghalang lama:

| Penghalang lama | Keadaan sekarang |
|---|---|
| **Tidak ada test project** — menahan delapan task di `Sebagian` | `Tests/QuilvianSystemBackend.OperatingRoomTests` berdiri dengan **70 uji**: permission matrix (25), transisi status (9), concurrency dan idempotensi (8), serial implant (8), audit dan privasi (3), outbox integrasi (10), smoke seed (4) |
| **`BE-OPR-009` Blocked** — adapter consumer belum berkontrak | Dicabut pemilik kebutuhan. Diselesaikan sebagai **kontrak event outbox internal**, bukan adapter ke consumer tertentu: `OprIntegrationDelivery` berkolom `EventId` (indeks unik), `EventType`, `EventVersion`, `OccurredAt`, `PayloadJson`, dengan `EventId` deterministik dari `destination` + `idempotencyKey`. Migration `20260929000000_AddOperatingRoomOutboxEventContract`, aditif. Terbukti runtime: 3 permintaan menghasilkan 2 pesan |
| **`BE-OPR-008` serial implant** — lingkup keunikan belum diputuskan | Aturan V1 disahkan pemilik kebutuhan: serial implant unik minimal dalam satu kasus operasi, ditegakkan `OPR014` pada `OperatingRoomMaterialService`. Indeks unik basis data **tidak** dibuat karena syarat "belum digantikan koreksi" tidak dapat dinyatakan sebagai indeks tersaring — dilaporkan sebagai keputusan bisnis terpisah, bukan diakali |

`BE-OPR-011` berstatus **Selesai — kurang tes**: regression state/concurrency/idempotency beserta
evidence report sudah ada, dan pengujian penerimaannya didelegasikan pemilik kebutuhan kepada
analisnya. Bukti runtime pada
[`docs/testing/OPR-backend-runtime-validation.md`](../../../testing/OPR-backend-runtime-validation.md).

Dua temuan yang sengaja dikeluarkan dari modul Operasi dan dicatat sebagai task tersendiri:
[rantai migration](../../../engineering/blocker-rantai-migration.md) dan
[`LoggerService` audit user](../../../engineering/task-core-logger-audit-user.md).

### Penghalang yang tidak dapat dicabut oleh modul Operasi sendiri

1. **Tidak ada test project.** Selama ini belum ada, delapan task tidak dapat naik ke `Selesai`
   dan `BE-OPR-011` tidak dapat dimulai. Pengadaannya keputusan tersendiri — `PHA-BE-002` pada
   modul Farmasi mencatat kendala yang sama.
2. **Owner API Billing dan Inventory belum menetapkan kontrak consumer** (`BE-OPR-009`).
3. **Owner integrasi kredensial tenaga belum ditetapkan** (`BE-OPR-004`).
