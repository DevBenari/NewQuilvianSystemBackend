# Kontrak API (API Contract) — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** — menunggu approval Muhammad Hamzah dan Yasmina |
| Base Path | `/api/v1/health-services/inpatient-management` |
| Autentikasi | Bearer Token (JWT), `Authorization: Bearer <token>` |
| Aturan Swagger | Seluruh endpoint dikelompokkan dengan atribut `[Tags(...)]` dan dilengkapi model DTO presisi. |

---

## 1. Grup Tag: `[Tags("Inpatient Billing Operational")]`

Grup endpoint ini melayani kebutuhan pemantauan status penagihan kasir pada layar bangsal rawat inap, memisahkan pandangan operasional non-finansial dari rincian nominal uang.

### 1.1 Tabel Endpoint Spesifikasi

| Method | Path | Status Ketersediaan | Deskripsi & Tujuan | Hak Akses (Permission) | Request DTO | Response DTO | Kode Status |
|---|---|:---:|---|---|:---:|:---:|:---:|
| `GET` | `/episodes/{episodeId}/billing-status` | `Rencana (belum tersedia)` | Mengambil ringkasan status operasional kasir untuk perawat bangsal (steril dari nominal rupiah). | `InpatientNurse:Read` atau `InpatientEpisode:Read` | — | `InpatientBillingStatusResponseDto` | `200 OK`, `404 Not Found` |
| `GET` | `/episodes/{episodeId}/billing-details` | `Rencana (belum tersedia)` | Mengambil rincian akumulasi biaya finansial lengkap beserta nominal rupiah (khusus staf berizin). | `InpatientBilling:View` | — | `InpatientBillingDetailsResponseDto` | `200 OK`, `403 Forbidden`, `404 Not Found` |

### 1.2 Detail Spesifikasi Endpoint

#### `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/billing-status`
- **Kegunaan:** Menampilkan lencana warna status kasir dan daftar kendala blocker pada layar perawat.
- **Contoh Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "encounterId": "ENC-2026-08891",
    "patientName": "Budi Santoso",
    "medicalRecordNumber": "RM-2026-08891",
    "folioStatus": "OPEN",
    "clearanceStatus": "PENDING",
    "operationalStatusText": "Menunggu Penyelesaian Kasir",
    "statusColor": "amber",
    "canPhysicallyDischarge": false,
    "blockerReasons": [
      "Keluarga pasien belum menyelesaikan administrasi pelunasan di kasir utama",
      "Menunggu konfirmasi verifikasi resep farmasi sore"
    ],
    "lastCheckedAtUtc": "2026-09-17T04:15:30Z"
  },
  "message": "Status operasional kasir berhasil diambil."
}
```

#### `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/billing-details`
- **Kegunaan:** Menampilkan breakdown biaya rupiah pada drawer rincian finansial (khusus pemegang izin `InpatientBilling:View`).
- **Contoh Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "encounterId": "ENC-2026-08891",
    "totalCharges": 6741000.00,
    "coveredAmount": 5500000.00,
    "patientExcess": 1241000.00,
    "depositPaid": 1000000.00,
    "outstandingAmount": 241000.00,
    "clearanceStatus": "PENDING",
    "items": [
      {
        "category": "ROOM_CHARGE",
        "description": "Sewa Kamar Melati Kelas 2 (2 Hari)",
        "amount": 1600000.00
      },
      {
        "category": "DOCTOR_VISIT",
        "description": "Visite Dokter Spesialis Penyakit Dalam (dr. Anwar, 3x)",
        "amount": 750000.00
      },
      {
        "category": "PHARMACY",
        "description": "Obat & Alkes Rawat Inap",
        "amount": 2850000.00
      },
      {
        "category": "ADMIN_FEE",
        "description": "Biaya Administrasi Rawat Inap (7%)",
        "amount": 441000.00
      }
    ]
  },
  "message": "Rincian finansial kasir berhasil diambil."
}
```

---

## 2. Grup Tag: `[Tags("Inpatient Discharge Clearance")]`

Grup endpoint ini melayani gerbang pemulangan pasien, webhook sinyal clearance dari kasir, dan penanganan darurat klinis *Supervisor Override*.

### 2.1 Tabel Endpoint Spesifikasi

| Method | Path | Status Ketersediaan | Deskripsi & Tujuan | Hak Akses (Permission) | Request DTO | Response DTO | Kode Status |
|---|---|:---:|---|---|:---:|:---:|:---:|
| `POST` | `/episodes/{episodeId}/discharge-clearance/webhook` | `Rencana (belum tersedia)` | Menerima sinyal clearance dari kasir (ClearanceApproved / ClearanceRevoked). | Internal System Token | `ClearanceSignalWebhookDto` | `BaseResponse` | `200 OK`, `400 Bad Request` |
| `POST` | `/episodes/{episodeId}/supervisor-override` | `Rencana (belum tersedia)` | Mengeksekusi override pelepasan darurat saat clearance dicabut/terblokir. | `InpatientSupervisor:Override` | `SupervisorOverrideRequestDto` | `BaseResponse` | `200 OK`, `403 Forbidden`, `422 Unprocessable` |
| `POST` | `/episodes/{episodeId}/confirm-physical-discharge` | `Rencana (belum tersedia)` | Mengonfirmasi pasien meninggalkan ruangan kamar secara fisik (`PhysicallyLeftAt`). | `InpatientNurse:Write` | `ConfirmPhysicalDischargeRequestDto` | `BaseResponse` | `200 OK`, `422 Unprocessable` |

### 2.2 Detail Spesifikasi Endpoint

#### `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/supervisor-override`
- **Kegunaan:** Otorisasi pelepasan darurat untuk pasien rujukan kritis saat kasir belum menerbitkan clearance atau clearance dibatalkan.
- **Request Body:**
```json
{
  "reason": "Pasien darurat syok kardiogenik, rujukan ambulans prioritas 1 ke RS Harapan Kita. Administrasi kasir dilanjutkan pihak keluarga penjamin di loket kasir utama.",
  "supervisorPin": "123456"
}
```
- **Response `200 OK`:**
```json
{
  "success": true,
  "message": "Supervisor override berhasil disahkan. Tombol pelepasan fisik pasien kini aktif atas izin darurat medis."
}
```

#### `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/confirm-physical-discharge`
- **Kegunaan:** Mengunci waktu keluar fisik pasien (`PhysicallyLeftAt`), menutup hunian kamar, dan menerbitkan event outbox `BED_RELEASED` ke kasir.
- **Request Body:**
```json
{
  "physicalDischargeDateTime": "2026-09-17T05:15:00Z",
  "notes": "Pasien telah dijemput keluarga dengan ambulans RS, resume medis dan obat pulang telah diserahkan."
}
```
- **Response `200 OK`:**
```json
{
  "success": true,
  "message": "Pasien berhasil dipulangkan secara fisik. Jam hunian tempat tidur ditutup presisi dan event kepulangan telah dikirim ke Kasir."
}
```

---

## 3. Perubahan pada `contract_version` `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `1.1.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Owner | Muhammad Hamzah (sisi Rawat Inap); Yasmina (endpoint Billing, disetujui lewat `RWI-DEC-192`) |
| `approved_by` / `approved_at` | — / — |
| `input_revision` | Decision log revision `30`; `PRD-RWI-FINISHING-001` v`0.4`; gate `1.9`; capability map `1.6` bagian 19 |
| Dampak kompatibilitas | **Breaking** untuk empat endpoint yang dihapus (bagian 3.1) dan untuk `record-departure` yang kini dapat menjawab 409 peringatan. Frontend yang memanggil endpoint lama wajib diganti pada gelombang yang sama (`FE-INT-01`, `FE-INT-02`) |
| Traceability | `FR-RWF-001` s.d. `024`; `RWI-DEC-166`, `167`, `169`, `170`, `186`, `187`, `192`, `195` |

Respons sukses selalu `ApiResponse<T>`. Bagian 1 dan 2 di atas **digantikan** bagian ini untuk setiap endpoint yang disebut di bawah.

### 3.1 Endpoint yang dihapus

| Method | Path lama | Tag lama | Pengganti | Dasar |
|---|---|---|---|---|
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/discharge-clearance/webhook` | `Inpatient Discharge Clearance` | Tidak ada; Rawat Inap membaca Billing | `RWI-DEC-167` butir 3 |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/supervisor-override` | `Inpatient Discharge Clearance` | `POST discharges/{episodeId}/close-with-override` | `RWI-DEC-187` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/confirm-physical-discharge` | `Inpatient Discharge Clearance` | `POST discharges/{episodeId}/record-departure` | `RWI-DEC-186` butir 1 |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/billing-details` | `Inpatient Billing Operational` | `GET billing-management/patient-billing-summaries/episodes/{episodeId}/breakdown` | `RWI-DEC-170` butir 5 |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/financial-clearance` | `Health Services / Inpatient Management / Inpatient Discharge` | Tidak ada; riwayat tetap dibaca lewat `GET` | `RWI-DEC-167` butir 4 |
| `POST` | `/api/v1/health-services/billing-management/billing/invoices/occupancy-charges` | `BillingInpatientIntegration` | Tidak ada; tarif kamar dihitung `BillingCalculationService` | `RWI-DEC-192` butir (e) |

Setelah dihapus, path lama menjawab **404**. Tidak ada pengalihan diam-diam.

### 3.2 Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{episodeId}/record-departure` | **Satu-satunya** tindakan keluar ruangan. Melepas bed seketika, mencatat status kasir saat itu, mengantrekan `BED_RELEASED`, lalu menutup pemakaian alat yang masih berjalan | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest { DepartedAt?, ClearanceWarningAcknowledged }` | `ApiResponse<InpatientDepartureResponse>` | **Diubah** — sudah ada; kini membaca Billing dan dapat menjawab 409 peringatan |
| `GET` | `/{episodeId}/closure-readiness` | Syarat penutupan; syarat keuangan dibaca langsung dari Billing | `InpatientDischarge : Read` | — | `ApiResponse<ClosureReadinessResponse>` | **Diubah** |
| `POST` | `/{episodeId}/close` | Penutupan normal; wajib `CLEARED` dari bacaan langsung | `InpatientDischarge : Close` | `CloseEpisodeRequest { ExpectedVersion }` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Diubah** |
| `POST` | `/{episodeId}/close-with-override` | Penutupan tanpa izin kasir; permission dan alasan saja; menyimpan status kasir saat itu | `InpatientDischarge : CloseOverride` | `CloseEpisodeOverrideRequest { Reason, ExpectedVersion }` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Diubah** — pemeriksaan nama peran dihapus |
| `GET` | `/{episodeId}/financial-clearance` | Riwayat tanda keuangan manual, baca saja | `InpatientDischarge : ReadFinancialClearance` | — | `ApiResponse<FinancialClearanceResponse>` | Tetap |

**`RecordDepartureRequest`**

| Field | Tipe | Wajib | Aturan |
|---|---|:---:|---|
| `DepartedAt` | `DateTime?` | Tidak | Bawaan waktu server. Tidak boleh melewati sekarang dan tidak boleh mendahului keputusan pulang (aturan yang sudah ada) |
| `ClearanceWarningAcknowledged` | `bool` | Ya | `true` hanya setelah layar menampilkan peringatan kasir. Bila status kasir bukan `CLEARED` atau tidak terbaca dan nilainya `false`, server menjawab 409 `INP-DEP-001` tanpa mengubah apa pun |

**`InpatientDepartureResponse`**: `EpisodeId`, `EpisodeStatus`, `PhysicallyLeftAt`, `PhysicallyLeftByUserName`, `ClearanceObserved` (`Cleared`/`Pending`/`Blocked`/`Revoked`/`Unreadable`), `ClearanceObservedAt`, `ReleasedBedCode`, `RunningEquipmentUsageClosedCount`, `FollowUpWarnings[]`. **Tanpa rupiah.**

Kode status:

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Kepergian tercatat; bed langsung kosong |
| `400` | Isian tidak valid, misalnya waktu kepergian melewati waktu sekarang |
| `401` / `403` | Belum login / tidak berhak |
| `404` | Episode tidak ditemukan |
| `409` `INP-DEP-001` | Kasir belum memberi izin pulang, atau status kasir tidak dapat dibaca. Tampilkan peringatan, lalu kirim ulang dengan pengakuan |
| `409` | Kepergian sudah dicatat sebelumnya (aturan yang sudah ada) |
| `422` | Episode belum berstatus `DischargePending` |

Kode khusus penutupan: `422` `INP-CLS-010` "Episode belum dapat ditutup: kasir belum memberi izin"; `422` `INP-CLS-011` "Status kasir tidak dapat dibaca"; `400` `INP-CLS-012` alasan override kosong atau hanya tanda baca.

### 3.3 Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{episodeId}/billing-status` | Status kasir dan kendala tanpa rupiah, dibaca langsung dari Billing | `InpatientBillingOperational : Read` | — | `ApiResponse<InpatientBillingStatusResponse>` | **Diubah** |

**`InpatientBillingStatusResponse`**: `EpisodeId`, `EncounterId`, `IsReadable`, `ClearanceStatus` (`PENDING`/`BLOCKED`/`CLEARED`/`REVOKED`, atau `null` bila tidak terbaca), `Reasons[] { Code, Label }`, `EvaluatedAt`, `InvoiceStatus` (`OPEN`/`FINAL`/`CLOSED`/`NONE`), `IsClosedWithoutFinancialClearance`. Tidak ada field rupiah sama sekali.

### 3.4 Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/placements/{placementId}/corrections` | Koreksi salah catat kamar, bed, kelas, atau waktu selama invoice `OPEN`; versi lama tersimpan | `InpatientBedOccupancy : Correct` | `CorrectPlacementRequest` | `ApiResponse<BedPlacementResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/placements/by-episode/{episodeId}` | Riwayat penempatan; kini memuat `CorrectsPlacementId`, `SupersededByCorrectionId`, dan `IsCorrection` | `InpatientBedOccupancy : Read` | — | Tetap, ditambah tiga field | **Diubah** — tambahan field, tidak breaking |
| `POST` | `/placements/transfer` | Transfer biasa; kini menerbitkan `BED_OCCUPIED` untuk penempatan baru, bukan `OCCUPANCY_CORRECTED` | `InpatientBedOccupancy : Transfer` | Tetap | Tetap | **Diubah** — perilaku event saja |

**`CorrectPlacementRequest`**

| Field | Tipe | Wajib | Aturan |
|---|---|:---:|---|
| `CorrectedBedId` | `Guid?` | Tidak | Bila diisi, wajib lolos kelayakan penempatan yang sama dengan transfer |
| `CorrectedPatientClassId` | `Guid?` | Tidak | — |
| `CorrectedStartDateTime` | `DateTime?` | Tidak | Tidak boleh mendahului `AdmittedAt` dan tidak boleh menimpa penempatan lain pada episode yang sama |
| `CorrectedEndDateTime` | `DateTime?` | Tidak | Hanya untuk penempatan yang sudah berakhir |
| `Reason` | `string(500)` | Ya | Tidak kosong dan tidak hanya tanda baca |
| `ExpectedVersion` | `int` | Ya | Versi penempatan yang dikoreksi |

Minimal satu field koreksi wajib diisi. Kode khusus: `422` `INP-COR-001` invoice rawat inap bukan `OPEN`; `422` `INP-COR-002` status invoice tidak dapat dibaca; `409` `INP-COR-003` versi berubah; `422` `INP-COR-004` bed tujuan tidak layak.

### 3.5 Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/departures-before-clearance` | Episode yang keluar ruangan dengan status kasir selain `CLEARED`; menampilkan status saat keluar dan status sekarang | `InpatientMonitoring : Read` | `PagedQuery { ServiceUnitId?, From?, To?, IncludeClosed? }` | `ApiResponse<PagedResult<DepartureBeforeClearanceItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/closures-without-financial-clearance` | Laporan penutupan tanpa kelayakan keuangan; kini memuat status kasir saat penutupan | `InpatientMonitoring : Read` | Tetap | Tetap, ditambah `ClosureClearanceObserved` | **Diubah** |

**`DepartureBeforeClearanceItem`**: `EpisodeId`, `EpisodeNumber`, `PatientName`, `MedicalRecordNumber`, `ServiceUnitName`, `PhysicallyLeftAt`, `RecordedByUserName`, `ClearanceObservedAtDeparture`, `CurrentClearanceStatus` (atau `Unreadable`), `EpisodeStatus`. Tanpa rupiah.

### 3.6 Health Services / Inpatient Management / Integration Outbox

Base URL: `api/v1/health-services/inpatient-management/integration-outbox`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Jumlah pesan per status dan daftar pesan gagal atau tersangkut | `InpatientIntegrationOutbox : Read` | `PagedQuery { Status?, EventType?, From?, To? }` | `ApiResponse<OutboxMonitorResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/replay` | Putar ulang episode aktif saat rilis | `InpatientIntegrationOutbox : Replay` | `ReplayRequest { DryRun, Reason }` | `ApiResponse<ReplayResultResponse>` | **Rencana (belum tersedia)** |

`ReplayResultResponse`: `ReplayBatchId`, `DryRun`, `EpisodeCount`, `Items[] { EpisodeId, EpisodeNumber, EventsQueued[], AlreadyPublishedRequeued }`. `Reason` wajib diisi. Endpoint ini dicatat logger walaupun `DryRun = true`.

### 3.7 Health Services / Billing Management / Patient Billing Summary

Base URL: `api/v1/health-services/billing-management/patient-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Ringkasan tagihan bangsal dari hitungan invoice; **tanpa rupiah** | `PatientBillingSummary : Read` | — | `ApiResponse<PatientBillingSummaryResponse>` | **Diubah** — field rupiah dipindah ke `/amounts` |
| `GET` | `/episodes/{episodeId}/amounts` | Total berjalan dalam rupiah | `PatientBillingSummary : ViewAmount` | — | `ApiResponse<PatientBillingAmountResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/breakdown` | Rincian per kelompok V1 **tanpa rupiah**; tidak pernah harga per item | `PatientBillingSummary : Read` | — | `ApiResponse<PatientBillingBreakdownResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/breakdown/amounts` | Subtotal rupiah per kelompok dan total berjalan; tidak pernah harga per item | `PatientBillingSummary : ViewAmount` | — | `ApiResponse<PatientBillingBreakdownAmountResponse>` | **Rencana (belum tersedia)** |

**Kenapa rupiah dipisah ke endpoint sendiri.** Registry hak akses repository ini membuat baris permission **hanya** dari atribut `[AccessPermission]` pada endpoint (`Services/Security/PermissionRegistryDescriptor.cs`). Permission yang hanya diperiksa di dalam service tidak pernah punya baris, sehingga tidak dapat dicentang di layar Akses Role. Dengan endpoint terpisah, "tanpa izin tidak ada rupiah" dijaga mesin hak akses yang sama dengan endpoint lain, dan respons `/breakdown` memang tidak pernah memuat rupiah (`RWI-DEC-170` butir 4).

**`PatientBillingBreakdownResponse`**

| Field | Tipe | Keterangan |
|---|---|---|
| `EpisodeId`, `EncounterId` | `Guid` | — |
| `InvoiceState` | `string` | `NOT_FORMED` bila invoice belum ada; layar menampilkan "Tagihan belum terbentuk", **bukan** "Rp 0" |
| `CalculatedAt` | `DateTimeOffset?` | Waktu versi hitungan yang dipakai |
| `Groups[]` | daftar | Urut: Kamar Rawat Inap, Tindakan, Penunjang Medis, Obat & Alkes, Pemakaian Alat, Operasi, Biaya Administrasi. Kelompok tanpa baris tidak dikirim |
| `Groups[].Lines[]` | daftar | `Label`, `PeriodLabel` atau `ServiceDate`, `Quantity`, `UnitLabel`, `LineStatus` (`ACTIVE`, `TARIFF_NOT_FOUND`) |


**`PatientBillingBreakdownAmountResponse`**: `EpisodeId`, `CalculatedAt`, `Groups[] { GroupCode, SubtotalAmount }`, `RunningTotalAmount`. Tidak ada `UnitPrice`, `Quantity × tarif`, maupun baris per item.

### 3.8 BillingInpatientIntegration

Base URL: `api/v1/health-services/billing-management/billing`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/invoices/encounter/{encounterId}/inpatient-summary` | Ringkasan tagihan rawat inap untuk kasir, dengan rupiah | `BillingInpatient : Read` | — | Tetap | **Diubah** — penyaringan rupiah berdasar `Contains("Admin")`, `"Cashier"`, `"Finance"` dihapus. Endpoint ini milik kasir, sehingga hak aksesnya sendiri yang menentukan; `BillingInpatient : Read` **tidak** diberikan kepada peran bangsal (`permission-audit-matrix.md` 5.3) |
| `GET` | `/inpatient-clearance/encounter/{encounterId}/latest` | Status izin kasir terakhir untuk kasir | `BillingInpatient : ReadLatest` | — | Tetap | Tetap. Rawat Inap **tidak** memakai endpoint ini; ia memakai service di dalam aplikasi (`integration-contract.md` bagian 4) |
| `POST` | `/inpatient-clearance/reevaluate` | Kasir menilai ulang izin kasir | `BillingInpatient : Clearance` | Tetap | Tetap | Tetap |

### 3.9 Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/review-queue` | Invoice yang ditandai "perlu diperiksa" | `BillingInvoice : Read` | `PagedQuery { ServiceType?, ReasonCode? }` | `ApiResponse<PagedResult<InvoiceReviewItem>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/review-resolution` | Kasir menyatakan pemeriksaan selesai setelah membatalkan baris yang dobel | `BillingInvoice : Update` | `ResolveInvoiceReviewRequest { Note, RowVersion }` | `ApiResponse<InvoiceDetailResponse>` | **Rencana (belum tersedia)** |

Kode khusus: `422` `BIL-REV-001` masih ada biaya kamar manual dan otomatis yang sama-sama aktif; `422` `BIL-FIN-020` finalisasi ditolak karena invoice "perlu diperiksa"; `422` `BIL-FIN-021` finalisasi ditolak karena masih ada baris "tarif belum ada".

### 3.10 Penerima di dalam aplikasi — bukan endpoint HTTP

| Service | Method | Masukan | Keluaran | Pemanggil |
|---|---|---|---|---|
| `BillingInpatientEventReceiver` | `ReceiveAsync` | `InpatientBillingEventEnvelope { EventType, EpisodeId, EncounterId, SourceType, SourceId, Version, OccurredAtUtc, IdempotencyKey }` | `InpatientEventReceipt { Accepted, ReceiptId, Outcome, Message }` | `InpatientIntegrationOutboxWorker` |
| `InpatientClearanceService` | `GetLatestStatusAsync` | `encounterId` | `InpatientClearanceStatusView { Status, Reasons[], EvaluatedAt, InvoiceStatus }` tanpa rupiah | `InpBillingClearanceAdapter` |

Keduanya **sengaja** tidak dibuat sebagai endpoint publik (`02-backend-architecture.md` 9.12).

### 3.11 Penyelarasan decision log revision `31` — baris kunjungan tertaut ★ 2 Oktober 2026

**Perubahan respons** pada `GET …/patient-billing-summaries/episodes/{episodeId}/breakdown` dan `…/breakdown/amounts` (`RWI-DEC-207`; desain `02-backend-architecture.md` 9.14). Hak akses tidak berubah. Status tetap **Rencana (belum tersedia)**.

| Field baru | Pada | Tipe | Isi |
|---|---|---|---|
| `LinkedEncounter` | Baris `breakdown` | Objek atau `null` | `{ EncounterId, EncounterTypeName ("Poliklinik"/"ODC"), ServiceUnitName, VisitDate }` — terisi hanya pada baris operasi dari kunjungan asal yang tertaut |
| `LinkedEncounters` | Kepala `breakdown` | Daftar | Kunjungan yang tertaut ke invoice `RANAP` episode, untuk keterangan "dibayar bersama saat pulang" |
| `IncludesLinkedEncounter` | Subtotal kelompok pada `amounts` | `bool` | `true` bila subtotal memuat baris kunjungan tertaut |

Contoh baris: `{ "group": "Operasi", "name": "Kolesistektomi", "quantity": 1, "linkedEncounter": { "encounterTypeName": "Poliklinik", "serviceUnitName": "Poli Bedah", "visitDate": "2026-10-01" } }` — tanpa satu pun field rupiah.

Tidak ada endpoint baru untuk membuat tautan: tautan lahir dari pemrosesan ketukan pintu (`contracts/integration-contract.md` `INT-RWF-29`).
