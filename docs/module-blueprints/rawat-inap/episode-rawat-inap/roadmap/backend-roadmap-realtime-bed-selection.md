# Roadmap Backend Notifikasi Realtime Konflik Pemilihan Bed

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `RTB-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; desain disetujui lewat `RWI-DEC-337`. Eksekusi memakai `build-module-backend`, tepat satu task per pemanggilan |
| kontrak terkunci | `0.16.0`; [Backend 19](../02-backend-architecture.md), [API 17](../contracts/api-contract.md), [Validation 19](../contracts/validation-matrix.md), [PRD 30](../04-prd-to-mvp.md), [Flow 21](../flowcharts/21-notifikasi-realtime-pemilihan-bed.md) |
| source snapshot | Backend `MHamzah@d420c2fbe094a2d70776c44955233bce9c87bda8`; frontend `HamzahV2@0accf387138a3fcc1ff40fbb5bdeb960076b4d59` |
| hash lock | [realtime-bed-selection-delivery-lock.json](realtime-bed-selection-delivery-lock.json) |
| scope / ID | 2 task: `BE-RWI-250` dan `BE-RWI-251`; berikutnya `BE-RWI-252` |
| roadmap pasangan | [Frontend](frontend-roadmap-realtime-bed-selection.md) · [Traceability](requirement-traceability-realtime-bed-selection.md) |
| production gate | `RWI-OQ-151` wajib CLOSED sebelum payload nama lengkap diaktifkan di produksi; bukan dependency pengerjaan nonproduksi |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai dan seluruh acceptance/DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` |

## Grafik Urutan Dependency

```text
BE-RWI-250 🟡 ──> BE-RWI-251 🟡
```

| Gelombang | Boleh mulai setelah | Task ID | Hasil gelombang |
|---:|---|---|---|
| 1 | — | `BE-RWI-250` | Subscription hub aman, kontrak payload, publisher best-effort, dan DI tersedia |
| 2 | `BE-RWI-250` | `BE-RWI-251` | Tiga mutation existing menerbitkan event non-replay setelah commit |

Garis berarti prasyarat → task. Setiap task muncul tepat sekali dan pasangan dependency grafik
sama dengan kolom dependency pada register.

## Kontrak HTTP Existing yang Tidak Boleh Berubah

Swagger tag: `Health Services / Inpatient Management / Bed Occupancy`. Base route:
`/api/v1/health-services/inpatient-management/bed-occupancies`.

| Method | Path | Permission existing | Request | Response sukses existing | Error existing | Dampak task |
|---|---|---|---|---|---|---|
| `POST` | `/reservations` | `InpatientBedOccupancy:Create` | `ReserveBedRequest` | `200 ApiResponse<BedReservationResponse>` | `400`, `404`, `409`, `422` | Sesudah commit baru, coba publish `BedReserved`; envelope/status tidak berubah |
| `POST` | `/placements` | `InpatientBedOccupancy:Create` | `PlacePatientRequest` | `200 ApiResponse<BedPlacementResponse>` | `400`, `404`, `409`, `422` | Sesudah commit baru, coba publish `PatientPlaced`; envelope/status tidak berubah |
| `POST` | `/placements/transfer` | `InpatientBedOccupancy:Transfer` | `TransferPatientRequest` | `200 ApiResponse<BedPlacementResponse>` | `400`, `403`, `404`, `409`, `422` | Sesudah commit baru, coba publish `PatientTransferred` ke unit target; envelope/status tidak berubah |

Tidak ada endpoint HTTP baru. Conflict `409`, validasi, idempotency, dan atomicity existing tetap
menjadi pengaman kebenaran; SignalR hanya awareness/invalidation.

## Kontrak SignalR Terkunci

| Arah | Hub / nama | Kontrak |
|---|---|---|
| Client → hub | `/hubs/queues` · `JoinInpatientBedSelection(Guid serviceUnitId)` | Wajib authenticated, `InpatientBedOccupancy:Read`, `InpatientEpisode:Read`, dan unit berada dalam scope actor; gagal dengan `HubException` generik |
| Client → hub | `/hubs/queues` · `LeaveInpatientBedSelection(Guid serviceUnitId)` | Keluar dari grup unit yang sama |
| Server → client | `InpatientBedSelectionChanged` | `InpatientBedRealtimeEventResponse`; grup `inpatient-bed-selection:{serviceUnitId:D}`; dilarang `Clients.All` |

Payload persis: `eventId`, `eventType`, `bedId`, `bedCode`, `bedName`, `roomId`, `roomName`,
`serviceUnitId`, `serviceUnitName`, `episodeId`, `patientName`, `actorUserId`,
`actorDisplayName`, `occurredAtUtc`, `bedOperationalVersion`. `eventType` hanya `BedReserved`,
`PatientPlaced`, atau `PatientTransferred`.

## Register Task

| Task ID / Status | Outcome | Requirement / keputusan | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / owner | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| 🟡 [`BE-RWI-250`](../task/report/backend/BE-RWI-250.md) | Subscription SignalR per unit dan fondasi publisher tersedia tanpa broad broadcast atau kebocoran PHI | FR-RI-202, 203; DEC-334, 336 | 0.16.0; VAL-RTB-01, 02, 04, 05, 12 | `QueueHub`, `InpBedAccessService`, `IHubContext<QueueHub>`, auth existing | Join/leave, group builder, authorization scope, DTO, publisher best-effort, DI | — | AC-472–474; UAT-904, 907 | `V-BE`; pemeriksaan hub/DTO/log; client SignalR join allow/deny bila runtime tersedia | InPatientManagement + Security/Privacy; risiko audience/PHI | `D-BE`; [laporan](../task/report/backend/BE-RWI-250.md) |
| 🟡 [`BE-RWI-251`](../task/report/backend/BE-RWI-251.md) | Reservation, placement, dan target transfer committed mengirim satu event baru; kegagalan publikasi tidak mengubah transaksi | FR-RI-204–207 | 0.16.0; VAL-RTB-03–05, 10, 11 | `InpBedOccupancyService.BedManagement`, `InpBedMutationCoordinator`, publisher BE-RWI-250 | Hook sesudah commit, suppress replay/failure, target unit, logging non-PHI | `BE-RWI-250` | AC-469–471, 478–480; UAT-901–909 | `V-BE`; API/hub dua client, race, replay, failure injection bila runtime tersedia | InPatientManagement; risiko urutan commit dan false failure | `D-BE`; [laporan](../task/report/backend/BE-RWI-251.md) |

## Batas Handoff dan Definition of Done

### V-BE — bukti wajib setiap task backend

1. Jalankan QBE preflight saat eksekusi berdasarkan `AGENTS.md`, ownership/prefix, kontrak lock,
   dan scope file task.
2. Review `git diff --check`, diff/scope, dan pastikan tidak ada schema, migration, permission
   string, response HTTP, atau package baru.
3. Jalankan `dotnet build QuilvianSystemBackend.csproj`; `dotnet restore` hanya bila dependency atau
   project berubah.
4. Verifikasi kontrak API/SignalR dan proses bisnis normal, negative, replay, race, dan partial
   failure yang relevan terhadap task. Bukti runtime dilakukan bila lingkungan/dependency tersedia.
5. Automated test baru **NOT APPLICABLE / tidak diwajibkan** oleh kebijakan backend. Test existing
   yang relevan boleh dijalankan sebagai bukti tambahan.

### D-BE — selesai berarti

- Outcome, acceptance criteria, dan seluruh bukti `V-BE` task terpenuhi.
- Nama pasien/petugas tidak muncul di application log atau metric label.
- Task report tracked ditulis pada `task/report/backend/<TASK-ID>.md` dan roadmap hanya diberi ✅
  setelah bukti benar-benar ada.
- Bila runtime dua client/failure injection belum dapat dijalankan, task maksimal 🟡; source selesai
  bukan alasan memberi ✅.

## Kartu Task

### 🟡 BE-RWI-250 — Secure subscription dan fondasi publisher realtime bed

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 17 Oktober 2026.** Tiga acceptance criteria terpetakan ke source dan build terisolasi lulus 0 error; `UAT-904` serta delivery runtime `UAT-907` belum dijalankan karena proses backend pengguna masih memakai binary lama dan data dua identity/unit uji belum tersedia. Bukti: [laporan](../task/report/backend/BE-RWI-250.md) |
| Outcome | Client berwenang dapat join/leave tepat unit aktif dan backend memiliki DTO/publisher kontraktual yang hanya mengirim ke grup unit |
| Trace | FR-RI-202, 203; RWI-DEC-334, 336; RWI-AC-472–474; VAL-RTB-01, 02, 04, 05, 12; UAT-904, 907 |
| Reuse / buat baru | Extend `Hubs/QueueHub.cs`, adapt `InpBedAccessService.cs`, extend DI `Program.cs`; buat DTO `InpatientBedRealtimeDtos.cs` dan service `InpBedRealtimeService.cs` mengikuti konvensi aktual |
| Scope source | `Hubs/QueueHub.cs`; `Program.cs`; `Areas/HealthServices/InPatientManagement/DTOs/`; `Areas/HealthServices/InPatientManagement/Services/InpBedAccessService.cs`; service realtime baru |
| Cakupan | Validasi GUID, dua permission read, scope unit fail-closed; group name deterministik; DTO 15 field; event name persis; publisher menangkap error dan log hanya ID teknis |
| Larangan | Tidak ada `Clients.All`, join-all, endpoint/controller baru, permission baru, schema/migration, PHI di log, retry/outbox/replay |
| Dependency | — |
| Verifikasi wajib | `V-BE`, termasuk join valid, unit luar scope, permission kurang, payload shape, target group, dan log redaction |
| DoD | `D-BE` dan laporan `task/report/backend/BE-RWI-250.md` |

### 🟡 BE-RWI-251 — Publikasi pasca-commit untuk reserve, place, dan transfer

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 17 Oktober 2026.** Enam acceptance criteria terpetakan ke source dan build terisolasi lulus 0 error; `UAT-901`–`UAT-909` belum dijalankan karena runtime build baru, dua identity, data bed, race/replay, dan failure injection belum tersedia. Bukti: [laporan](../task/report/backend/BE-RWI-251.md) |
| Outcome | Tepat satu event sukses diterbitkan setelah commit baru untuk tiga mutation; transaksi tetap sukses saat publish gagal |
| Trace | FR-RI-204–207; RWI-DEC-333, 335, 336; RWI-AC-469–471, 478–480; VAL-RTB-03–05, 10, 11; UAT-901–909 |
| Reuse / extend | Extend `InpBedOccupancyService.BedManagement.cs`; reuse `InpBedMutationCoordinator` dan publisher `BE-RWI-250` |
| Scope source | `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.BedManagement.cs` dan perubahan minimum pendukung yang dibuktikan saat QBE preflight |
| Cakupan | `ReserveBedAsync` → `BedReserved`; `PlacePatientAsync` → `PatientPlaced`; `TransferAsync` → `PatientTransferred` hanya unit target; cek `!meta.IsReplay`; snapshot berasal dari state committed |
| Failure policy | Request gagal/rollback/replay tidak publish. Query/publish gagal sesudah commit dicatat tanpa PHI, tidak rollback, tidak menghasilkan `500` semu, dan tidak menjalankan ulang mutation |
| Dependency | `BE-RWI-250` |
| Verifikasi wajib | `V-BE`; tiga happy path; request gagal; replay; race satu pemenang/409; publish failure setelah commit; response HTTP tidak berubah |
| DoD | `D-BE` dan laporan `task/report/backend/BE-RWI-251.md` |
