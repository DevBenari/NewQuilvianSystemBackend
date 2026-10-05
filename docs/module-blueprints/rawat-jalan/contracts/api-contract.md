# API Contract — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-API-001@1.0.0` |
| Status | `draft` |
| Owner | Billing/Revenue Cycle + API authority |
| Input | Decision revision `10`, domain architecture revision `1`, capability map revision `2` |
| Compatibility impact | Endpoint working tree sudah ada; endpoint target baru diberi label rencana |

### Health Services / Billing Management / Billing Folio

Base URL: `api/v1/health-services/billing-management/folios`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-encounter/{encounterId}` | Melihat folio berdasarkan encounter | `BillingFolio : Read` | `encounterId` path | `ApiResponse<BillingFolioDetailResponse>` | AS-IS working tree |
| `GET` | `/{folioId}` | Melihat folio berdasarkan ID | `BillingFolio : Read` | `folioId` path | `ApiResponse<BillingFolioDetailResponse>` | AS-IS working tree |
| `POST` | `/internal/milestones/recognize` | Menerima milestone internal yang telah diotorisasi dan memprosesnya idempotent | `BillingMilestone : RecognizeInternal` | `RecognizeBillingMilestoneRequest` | `ApiResponse<RecognizeBillingMilestoneResponse>` | AS-IS working tree; system-only |
| `POST` | `/{folioId}/allocations` | Membuat versi allocation multi-payer | `BillingAllocation : Create` | `CreateAllocationRequest` | `ApiResponse<AllocationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{folioId}/financial-actions` | Mengajukan void/adjustment/reversal/refund/FOC/write-off | `BillingFinancialAction : Create` | `CreateFinancialActionRequest` | `ApiResponse<FinancialActionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{folioId}/close` | Mengajukan penutupan folio setelah semua prerequisite terpenuhi | `BillingFolio : Close` | `CloseFolioRequest` | `ApiResponse<BillingFolioDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{folioId}/reopen` | Mengajukan reopen folio melalui high-risk approval | `BillingFolio : Reopen` | `ReopenFolioRequest` | `ApiResponse<FinancialActionResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Payer Allocation

Base URL: `api/v1/health-services/billing-management/payer-allocations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{folioId}` | Melihat allocation dan patient responsibility per versi | `BillingAllocation : Read` | `folioId` path | `ApiResponse<AllocationDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{folioId}/supersede` | Membuat versi allocation baru dengan evidence | `BillingAllocation : Supersede` | `SupersedeAllocationRequest` | `ApiResponse<AllocationDetailResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Financial Action

Base URL: `api/v1/health-services/billing-management/financial-actions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Melihat request dan histori financial action | `BillingFinancialAction : Read` | `id` path | `ApiResponse<FinancialActionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/execute` | Menjalankan action yang sudah disetujui setelah revalidasi state | `BillingFinancialAction : Execute` | `ExecuteFinancialActionRequest` | `ApiResponse<FinancialActionResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Reconciliation

Base URL: `api/v1/health-services/billing-management/reconciliation`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Melihat mismatch, owner, dan tindakan berikutnya | `BillingReconciliation : Read` | `id` path | `ApiResponse<ReconciliationCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/resolve` | Menyelesaikan case dengan evidence dan outcome | `BillingReconciliation : Resolve` | `ResolveReconciliationRequest` | `ApiResponse<ReconciliationCaseResponse>` | **Rencana (belum tersedia)** |

Kode status: `200` berhasil atau replay canonical; `400` input fact tidak valid; `401` identitas
tidak tersedia; `403` capability tidak diberikan; `404` encounter/folio/case tidak ditemukan;
`409` idempotency/version/outcome conflict; `422` policy atau konfigurasi finansial belum valid;
`500` kegagalan server yang harus masuk observability, bukan dianggap financial failure otomatis.



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.0` |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Owner | Billing/Revenue Cycle + API authority; titik sentuh Clinical, Pharmacy, Registration |
| `approved_by` / `approved_at` | — (belum disetujui) |
| Input | `00-interview-decisions.md` revisi `18`; `02-backend-architecture.md` bagian V2 |
| Traceability | `RJ-E2E-DEC-001`, `003`, `005`, `006`, `008`, `009`, `010`, `012` |
| Compatibility impact | Tiga grup endpoint baru; satu endpoint lama menambah jawaban `422` untuk domain klinis Rawat Jalan; respons finalisasi konsultasi tidak berubah bentuk |

Pembungkus respons seluruh endpoint: `ApiResponse<T>`.

### Health Services / Billing Management / Encounter Billing Summary

Base URL: `api/v1/health-services/billing-management/encounter-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{encounterId}` | Ringkasan tagihan satu kunjungan untuk workspace dokter: status, nomor invoice, total, bagian penjamin dan pasien, serta daftar pelayanan **tanpa harga per item** | `EncounterBillingSummary : Read` | path `encounterId` | `EncounterBillingSummaryResponse` | **Rencana (belum tersedia)** |

`EncounterBillingSummaryResponse`:

| Field | Tipe | Keterangan |
|---|---|---|
| `EncounterId` | `Guid` | — |
| `BillingStatus` | `string` | `NO_INVOICE`, `OPEN`, `FINAL`, `CLOSED`, `SETTLED_BY_WRITE_OFF` |
| `InvoiceId`, `InvoiceNumber` | `Guid?`, `string?` | Kosong bila `NO_INVOICE` |
| `ServiceCount` | `int` | Jumlah item invoice aktif |
| `GrossAmount`, `PayerAmount`, `PatientAmount` | `decimal` | Dari `BillingCalculationService.PreviewCalculationAsync` (`PrimaryAmount` → `PayerAmount`); `0` bila `NO_INVOICE` |
| `PaymentSourceLabel` | `string` | Mis. *Tunai*, *Asuransi — Nama Asuransi*; dari adapter coverage registrasi yang sudah ada |
| `LastUpdatedAt` | `DateTimeOffset?` | Waktu perubahan invoice terakhir |
| `PendingSyncCount` | `int` | Baris folio kunjungan ini yang `Pending`/`Failed` |
| `ReconciliationCount` | `int` | Baris/fakta kunjungan ini yang butuh rekonsiliasi |
| `Services[]` | list | `Description`, `Quantity`, `ServiceStatusLabel` (mis. *Dikerjakan*, *Diterima Lab*, *Diresepkan*, *Diserahkan*, *Selesai*), `BillingState` (`RECORDED`, `PENDING`, `RECONCILIATION`, `NOT_BILLED`) — **tanpa** `UnitPrice`/`TotalPrice` |

Kode status:

- `200` — ringkasan berhasil dibaca, **termasuk** saat belum ada invoice (`BillingStatus = NO_INVOICE`). Di awal kunjungan ini keadaan normal, bukan galat.
- `401` — sesi tidak berlaku; pengguna harus masuk ulang.
- `403` — pengguna tidak memegang `EncounterBillingSummary : Read`.
- `404` — kunjungan tidak ditemukan.
- `422` — angka tagihan tidak dapat dihitung, misalnya kalkulasi Billing menolak data invoice.

### Health Services / Billing Management / Billing / Charge Reconciliations

Base URL: `api/v1/health-services/billing-management/billing/charge-reconciliations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar antrean: fakta klinis yang belum pasti sampai, dan baris folio yang belum masuk invoice | `BillingChargeReconciliation : Read` | query `ChargeReconciliationQuery` | `PagedResult<ChargeReconciliationItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{itemType}/{id}/retry` | Mengirim ulang satu item memakai identitas yang sama | `BillingChargeReconciliation : Update` | path `itemType` (`CLINICAL_FACT`/`CHARGE_LINE`), `id` | `ChargeReconciliationItemResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{itemType}/{id}/resolve` | Menyatakan item selesai secara manual beserta alasannya | `BillingChargeReconciliation : Update` | body `ResolveChargeReconciliationRequest` | `ChargeReconciliationItemResponse` | **Rencana (belum tersedia)** |

`ChargeReconciliationQuery`: `Status` (`FAILED`, `RECONCILIATION_REQUIRED`, `RESOLVED`), `SourceDomain`, `EncounterId`, `ErrorCode`, `DateFrom`, `DateTo`, `Page`, `PageSize` (maks. 100), `Search` (maks. 100 karakter: nomor invoice atau nomor kunjungan).

`ChargeReconciliationItemResponse`: `ItemType`, `Id`, `EncounterId`, `EncounterNumber`, `SourceDomain`, `SourceDetailId`, `SourceVersion`, `ErrorCode`, `ErrorMessage`, `AttemptCount`, `LastAttemptAt`, `NextAttemptAt`, `ReconciliationRequiredAt`, `ResolvedAt`, `ResolvedByUserName`, `ResolutionNote`, `InvoiceNumber`. Tanpa nama pasien lengkap; hanya nomor rekam medis tersamar pada layar.

`ResolveChargeReconciliationRequest`: `Resolution` (`BILLED_MANUALLY`, `NOT_BILLABLE`, `DUPLICATE`), `Note` (wajib, 10–500 karakter).

Kode status:

- `200` — daftar dibaca atau tindakan berhasil.
- `403` — tidak memegang hak akses yang disebut.
- `404` — item tidak ditemukan.
- `409` — item sudah diselesaikan atau sedang diproses pekerja latar; muat ulang.
- `422` — isian tidak sah, misalnya catatan kurang dari 10 karakter, atau item `Synced` tidak boleh dikirim ulang.

### Health Services / Billing Management / Master Data / Billing Sync Policy

Base URL: `api/v1/health-services/billing-management/master-data/billing-sync-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Membaca kedua kebijakan kirim ulang | `BillingSyncPolicy : Read` | — | `List<BillingSyncPolicyResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah batas percobaan dan jeda | `BillingSyncPolicy : Update` | `UpdateBillingSyncPolicyRequest` (`MaxAttemptCount` 0–20, `BaseDelaySeconds` 10–3600, `MaxDelaySeconds` ≥ `BaseDelaySeconds` dan ≤ 86400, `IsActive`, `RowVersion`) | `BillingSyncPolicyResponse` | **Rencana (belum tersedia)** |

Kode status: `200`; `403`; `404`; `409` — kebijakan sudah diubah orang lain, muat ulang; `422` — angka di luar batas.

### Health Services / Billing Management / Billing / Invoices — perubahan

Base URL: `api/v1/health-services/billing-management/billing/invoices` (sudah ada)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/from-source` | Mencatat item dari sistem sumber | `BillingInvoice : Create` | `UpsertChargeRequest` | `InvoiceDetailResponse` | **Sudah ada — diubah:** menjawab `422` (`RJE-VAL-010`) bila `SourceDomain` adalah `PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, atau `CONSULTATION` untuk kunjungan `Outpatient` |

### Endpoint klinis yang perilakunya berubah tanpa perubahan bentuk

| Endpoint | Hak akses | Perubahan |
|---|---|---|
| `PATCH /doctor-consultations/{id}/complete` | tidak berubah | Menerbitkan juga fakta konsultasi. `BillingHandoffIssues` dapat memuat masalah penyerahan konsultasi |
| Endpoint dispensing resep (`PrescriptionDispensingService.DispenseAsync`) | tidak berubah | Setelah `Dispensed`/`PartiallyDispensed`, menerbitkan fakta resep tahap 2 |
| Endpoint selesai skrining perawat (`NurseStationQueueController`) | tidak berubah | Kunjungan tanpa dokter berakhir di `Billing`, bukan `Completed` |

## V2.1 — Usulan `RJ-E2E-CONTRACT-001@1.0.1` (`draft`, menunggu approval)

| Field | Nilai |
|---|---|
| Status | `draft` — ditemukan saat `plan-module-delivery` 2026-09-28 |
| Sifat | Aditif; konsumen lama tidak terdampak |
| Menahan | `BE-RJE-013`, `FE-RJE-02` |

**Sebab.** Kontrak `1.0.0` menulis bahwa `BillingHandoffIssues` dibaca dari respons
`PATCH /doctor-consultations/{id}/complete`. Frontend Rawat Jalan ternyata memanggil
`POST /doctor-queues/{id}/finish-consultation` (`doctor-queue.service.js:121`), yang sejak
`RJ-DOC-BE-001` mengorkestrasi finalisasi canonical tetapi mengembalikan `DoctorQueueActionResponse`
**tanpa** `BillingHandoffIssues` (`DoctorQueueController.cs`). Tanpa perubahan ini dokter tidak
pernah melihat masalah penyerahan (`RJ-E2E-FE-004`).

### Health Services / Registration Management / Doctor Queue — perubahan

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{id}/finish-consultation` | Menyelesaikan konsultasi dari antrean dokter | tidak berubah | tidak berubah | `DoctorQueueActionResponse` **+ `BillingHandoffIssues: string[]`** (kosong bila tidak ada masalah) | **Rencana (belum tersedia)** — menunggu approval `1.0.1` |

**Contoh:** Billing sempat tidak dapat dihubungi saat dr. B menekan Selesai. Respons `200` memuat
`"billingHandoffIssues": ["Resep R/0912: CLIN_FACT_DISPATCH_PENDING"]`, dan layar menampilkan
"penyerahan ke tagihan akan dicoba ulang otomatis". Konsultasi tetap selesai.


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`)

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-DOC-ENCLIST-001@1.0.0` |
| Status | `draft` |
| Owner | Sukma Giri (Product/Domain, API authority) |
| `approved_by` / `approved_at` | — / — |
| Input | `00-interview-decisions.md` (Amendment Pass + Closure 2026-10-02), `01-capability-impact-scan-daftar-pasien-rj.md` |
| Compatibility | **Aditif.** Endpoint baru; endpoint lama tidak berubah. Satu perubahan perilaku: pemblokir `POST /patient-encounters`, `/admin`, `/kiosk` menjadi lebih longgar (`RJ-DOC-DEC-019`/`022`) |
| Traceability | `RJ-DOC-DEC-012`..`023` |

## Health Services / Registration Management / Outpatient Encounter

Base URL: `api/v1/health-services/registration-management/outpatient-encounters` — **Rencana (belum tersedia).**
Seluruh respons dibungkus `ApiResponse<T>`.

| Method | Path | Kegunaan | Hak akses | Kode status |
|---|---|---|---|---|
| `GET` | `/` | Daftar kunjungan RJ berklinik dalam cakupan pengguna, berhalaman | `[AccessPermission("OutpatientEncounter", "Read")]` | `200`, `400`, `401`, `403` |
| `GET` | `/summary` | Jumlah per kelompok status untuk saringan yang sama | `[AccessPermission("OutpatientEncounter", "Read")]` | `200`, `401`, `403` |
| `GET` | `/filters/metadata` | Cakupan pengguna dan opsi filter | `[AccessPermission("OutpatientEncounter", "Read")]` | `200`, `401`, `403` |
| `PATCH` | `/{id}/cancel` | Membatalkan satu kunjungan | `[AccessPermission("OutpatientEncounter", "Cancel")]` | `200`, `400`, `401`, `403`, `404` |

### `GET /` — query `OutpatientEncounterListQuery` (PagedQuery)

| Parameter | Tipe | Wajib | Bawaan | Aturan |
|---|---|---|---|---|
| `mode` | `string` | Tidak | `today` | `today` = tanggal kunjungan hari ini (zona waktu RS); `active` = semua tanggal, hanya yang memblokir (DP.3.2); `range` = pakai `dateFrom`/`dateTo` |
| `dateFrom`, `dateTo` | `date` | Wajib bila `mode=range` | — | `dateFrom ≤ dateTo`, rentang maks 31 hari |
| `hangingOnly` | `bool` | Tidak | `false` | Bila `true`: hanya kunjungan yang memblokir dengan tanggal sebelum hari ini; memaksa `mode=active` |
| `encounterStatus` | `int[]` | Tidak | semua | Nilai `EncounterStatus` 0-11 |
| `clinicId` | `Guid` | Tidak | — | Mempersempit; di luar cakupan → hasil kosong |
| `doctorId` | `Guid` | Tidak | — | Sama |
| `search` | `string` | Tidak | — | Maks 100; no. kunjungan, no. RM, nama pasien |
| `pageNumber`, `pageSize` | `int` | Tidak | `1`, `20` | `pageSize` maks 100 (nama mengikuti `GET /patient-encounters`) |

Urutan: `EncounterDate` menurun, lalu `RegisteredAt` menurun.

**Response** `PagedResult<OutpatientEncounterListItem>` (`Responses/PagedResult.cs`: `pageNumber`, `pageSize`, `totalData`, `totalPage`, `items`):

| Field | Tipe | Sensitif | Keterangan |
|---|---|:---:|---|
| `id` | `Guid` | | |
| `encounterNumber` | `string` | | |
| `encounterDate` | `datetime` | | |
| `patientId` | `Guid` | | |
| `patientName` | `string` | Ya | |
| `medicalRecordNumber` | `string` | Ya | |
| `clinicId`, `clinicName` | `Guid`, `string` | | |
| `doctorId`, `doctorName` | `Guid?`, `string?` | | Kosong bila belum ditetapkan |
| `paymentLabel` | `string` | | "Tunai" atau nama penjamin |
| `encounterStatus` | `int` | | |
| `encounterStatusName` | `string` | | Dari `[Display]` enum |
| `isCancelled` | `bool` | | |
| `isHanging` | `bool` | | Memblokir dan bertanggal sebelum hari ini |
| `hasActiveConsultation` | `bool` | | |
| `canCancel` | `bool` | | Dihitung server: pengguna memegang `Cancel` **dan** DP.3.4 terpenuhi |
| `cancelBlockedReason` | `string?` | | Diisi bila `canCancel=false` karena status, mis. "Konsultasi masih aktif. Selesaikan atau batalkan konsultasi lewat workspace dokter." |

**Contoh** (`GET /?mode=active&hangingOnly=true`, petugas pendaftaran):

```json
{
  "success": true,
  "data": {
    "items": [{
      "id": "…", "encounterNumber": "ENC-RSMMC-00146", "encounterDate": "2026-07-30T02:10:00Z",
      "patientName": "(sensitif)", "medicalRecordNumber": "(sensitif)",
      "clinicName": "Poli Penyakit Dalam", "doctorName": "dr. …", "paymentLabel": "Tunai",
      "encounterStatus": 3, "encounterStatusName": "Menunggu Perawat",
      "isCancelled": false, "isHanging": true, "hasActiveConsultation": false,
      "canCancel": true, "cancelBlockedReason": null
    }],
    "pageNumber": 1, "pageSize": 20, "totalData": 1, "totalPage": 1
  }
}
```

### `GET /summary` — query sama dengan `GET /` tanpa `encounterStatus`, `pageNumber`, `pageSize`

| Field | Isi |
|---|---|
| `waiting` | Status 0-5, belum batal |
| `inConsultation` | Status 6, belum batal |
| `readyForBilling` | Status 7-8, belum batal |
| `closed` | Batal, Selesai, atau Tidak Hadir |
| `hanging` | Memblokir dan bertanggal sebelum hari ini — **selalu dihitung lintas tanggal**, tidak terpengaruh `mode` |

### `GET /filters/metadata`

| Field | Isi |
|---|---|
| `scope.canReadAll` | `bool` |
| `scope.label` | "Pasien Anda", "Klinik cluster Anda", "Pasien Anda dan klinik cluster Anda", atau "Semua klinik" |
| `statusOptions` | `{ value, label }` 0-11 |
| `clinicOptions` | Klinik dalam cakupan (semua klinik aktif bila `canReadAll`) |
| `doctorOptions` | Hanya bila `canReadAll`; selain itu kosong |

### `PATCH /{id}/cancel` — body `OutpatientEncounterCancelRequest`

| Field | Tipe | Wajib | Aturan |
|---|---|:---:|---|
| `cancelReason` | `string` | Ya | Dipangkas spasi; 1-250 karakter |

Respons `200`: `{ "success": true, "data": { "id": "…", "cancelledAt": "…", "cancelledQueueCount": 1 }, "message": "Kunjungan berhasil dibatalkan." }`.
Pesan penolakan ada di `contracts/validation-matrix.md` *Amendment DP*.

## Perubahan perilaku endpoint yang sudah ada

| Endpoint | Sebelum | Sesudah |
|---|---|---|
| `POST /patient-encounters`, `/admin`, `/kiosk` | Ditolak `400` bila pasien punya kunjungan apa pun yang belum selesai/batal/tidak hadir | Ditolak `400` **hanya** bila pasien punya kunjungan RJ berklinik berstatus 0-6 (DP.3.2). Bunyi pesan tidak berubah |
| `PATCH /patient-encounters/{id}/cancel` | — | Tidak berubah |

---

# Amendment KT — Konsultasi Tertunda (`RJ-DOC-PENDCONS-001@1.0.0`)

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-DOC-PENDCONS-001@1.0.0` |
| Status | `draft` |
| Owner | Sukma Giri (Product/Domain, API authority) |
| `approved_by` / `approved_at` | — / — |
| Input | `00-interview-decisions.md` (Amendment Pass 2026-10-05), `02-backend-architecture.md` *Amendment KT* |
| Compatibility | **Aditif.** Satu endpoint baru. Endpoint lama tidak berubah bentuk maupun perilakunya. Satu perubahan bunyi pesan `RJDP-VAL-005` (`validation-matrix.md` *Amendment KT*) |
| Traceability | `RJ-DOC-DEC-028`..`031`, `RJ-DOC-FE-010`..`012`; menjawab `RJ-DOC-OQ-012`, `RJ-DOC-OQ-013` |

## Health Services / Registration Management / Doctor Queue

Base URL: `api/v1/health-services/registration-management/doctor-queues`. Respons dibungkus
`ApiResponse<T>`.

| Method | Path | Kegunaan | Hak akses | Kode status | Keadaan |
|---|---|---|---|---|---|
| `GET` | `/pending-consultations` | Konsultasi tertunda milik dokter yang login, lintas tanggal | `[AccessPermission("DoctorQueue", "Read")]` | `200`, `401`, `403` | **Rencana (belum tersedia)** |

### `GET /pending-consultations` — query

| Parameter | Tipe | Wajib | Bawaan | Aturan |
|---|---|---|---|---|
| `doctorId` | `Guid` | Tidak | dokter yang login | Diproses `ResolveAllowedDoctorIdAsync`: dokter lain diabaikan, tidak melebarkan hasil |
| `queueId` | `Guid` | Tidak | — | Mempersempit ke satu antrean (baca ulang hitungan saat modal Simpan). Antrean di luar syarat → hasil kosong, bukan `404` |
| `search` | `string` | Tidak | — | Sama dengan `GET /doctor-queues` (kode antrean, nama pasien, no. RM, no. kunjungan, poli, ruang) |
| `pageNumber`, `pageSize` | `int` | Tidak | `1`, `25` | Dinormalkan `NormalizePaging`, sama dengan `GET /doctor-queues` |

Urutan: `QueueDate` menaik (paling lama dulu), lalu nomor antrean.

**Response** `PagedResult<DoctorPendingConsultationResponse>` (`pageNumber`, `pageSize`, `totalData`,
`totalPage`, `items`). Setiap item berisi **seluruh field `DoctorQueueResponse`** (bentuk sama dengan
`GET /doctor-queues`, termasuk `queueDate` dan `consultationId`) ditambah:

| Field | Tipe | Sensitif | Keterangan |
|---|---|:---:|---|
| `draftPrescriptionCount` | `int` | | Resep draf aktif milik konsultasi |
| `procedureCount` | `int` | | Tindakan aktif milik konsultasi |
| `pendingDays` | `int` | | Hari sejak `queueDate` sampai tanggal operasional hari ini; minimal `1` |
| `canCancelConsultation` | `bool` | | Pengguna memegang `DoctorConsultation : Cancel`. Penanda tampilan saja |

**Contoh respons (dipangkas, data rekaan):**

```json
{
  "success": true,
  "message": "Konsultasi tertunda berhasil diambil.",
  "data": {
    "pageNumber": 1, "pageSize": 25, "totalData": 1, "totalPage": 1,
    "items": [{
      "id": "6f1c…", "queueDate": "2026-09-30T00:00:00",
      "patientName": "Pasien Contoh", "medicalRecordNumber": "00-00-00-99",
      "clinicName": "Poli Anak", "doctorName": "dr. Contoh",
      "consultationId": "a8e2…",
      "draftPrescriptionCount": 1, "procedureCount": 0,
      "pendingDays": 5, "canCancelConsultation": true
    }]
  }
}
```

| Kode | Kapan |
|---|---|
| `200` | Berhasil, termasuk daftar kosong |
| `401` | Tidak login |
| `403` | Tidak memegang `DoctorQueue : Read`, atau bukan super admin dan tidak terhubung ke data dokter (`Forbid()`, sama dengan `GET /doctor-queues`) |

### Endpoint lama yang dipakai, tanpa perubahan

| Method | Path | Dipakai untuk |
|---|---|---|
| `POST` | `/doctor-queues/{id}/finish-consultation` | Simpan konsultasi tertunda |
| `PATCH` | `api/v1/health-services/clinical-management/doctor-consultations/{id}/cancel` | Batalkan konsultasi tertunda; body `{ "cancelReason": "…" }` |
| `PATCH` | `api/v1/health-services/registration-management/outpatient-encounters/{id}/cancel` | Petugas membatalkan kunjungan sesudah konsultasi dibatalkan |
