# Backend Roadmap — Modul Operasi

Kontrak terkunci: `opr-api-v1`, `opr-state-v1`, `opr-integration-v1`.

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

| Status | Jumlah | Task |
| --- | :---: | --- |
| Selesai | 1 | `BE-OPR-002` |
| Sebagian | 8 | `BE-OPR-001`, `003`, `004`, `005`, `006`, `007`, `008`, `010` |
| Blocked | 1 | `BE-OPR-009` |
| Belum | 1 | `BE-OPR-011` |

### Penghalang yang tidak dapat dicabut oleh modul Operasi sendiri

1. **Tidak ada test project.** Selama ini belum ada, delapan task tidak dapat naik ke `Selesai`
   dan `BE-OPR-011` tidak dapat dimulai. Pengadaannya keputusan tersendiri — `PHA-BE-002` pada
   modul Farmasi mencatat kendala yang sama.
2. **Owner API Billing dan Inventory belum menetapkan kontrak consumer** (`BE-OPR-009`).
3. **Owner integrasi kredensial tenaga belum ditetapkan** (`BE-OPR-004`).
