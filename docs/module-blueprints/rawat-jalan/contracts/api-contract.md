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

# Amendment PM-B — `RJ-DOC-REFERRAL-001@1.0.0`

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-DOC-REFERRAL-001@1.0.0` — `approved` |
| Owner | Sukma Giri |
| `approved_by` / `approved_at` | Sukma Giri / 2026-10-08 |
| `input_revision` | Decision log *Amendment PM-B* (`RJ-DOC-DEC-068`..`082`) |
| Compatibility | Aditif. Ruas baru opsional; endpoint lama tanpa ruas baru berperilaku sama |

Base URL: `/api/v1/health-services`. Semua endpoint di bawah berlabel **Rencana (belum tersedia)**
kecuali disebut lain.

## Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `POST` | `/registration-management/patient-encounters/admin` | Buat kunjungan; **ditambah** `referralInstitutionId`, `referralDoctorId`, `referral` | `PatientEncounter : Create` | Sudah ada — diperbarui (Rencana) |
| `POST` | `/registration-management/patient-encounters/kiosk` | Sama, dari Kiosk (`referral` tanpa diagnosa/alasan) | Policy `KioskRead` | Sudah ada — diperbarui (Rencana) |

Blok request baru (berlaku bila `isReferral = true` dan `referral` diisi):

```json
{
  "isReferral": true,
  "referralNumber": "RJK/2026/0991",
  "referralInstitutionId": "6b1e…",
  "referralDoctorId": "0c55…",
  "referral": {
    "referralDateTime": "2026-10-08T09:10:00+07:00",
    "targetUnitType": 1,
    "diagnosisId": "a1f0…",
    "diagnosisNote": "Gula darah puasa 210 mg/dL",
    "referralReason": "Kontrol gula darah tidak stabil"
  }
}
```

`targetUnitType` pada endpoint ini hanya `1` (Clinic). Unit tujuan poli = `clinicId` +
`serviceUnitId` kunjungan itu sendiri, jadi tidak dikirim dua kali. Respons tetap sama, ditambah
`referral: { id, isComplete, rowVersion }`.

| Kode | Kapan |
|---|---|
| `400` `RJ-VAL-PM-03` | `isReferral = true` tetapi `referralNumber` atau `referralInstitutionId` kosong |
| `400` `RJ-VAL-PM-04` | Dokter perujuk bukan milik institusi terpilih, atau institusi/dokter tidak aktif |
| `400` `RJ-VAL-PM-05` | `targetUnitType` bukan `1` di endpoint ini, atau `3` (Radiologi) di mana pun |
| `400` `RJ-VAL-PM-06` | Jalur admin: `diagnosisId` atau `referralReason` kosong |
| `400` `RJ-VAL-PM-07` | `referralDateTime` di masa depan |

## Health Services / Registration Management / Encounter Referral

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/registration-management/patient-encounters/{encounterId}/referral` | Rincian rujukan + berkas aktif + `isComplete` + `isLocked` | `PatientEncounter : Read` | Rencana (belum tersedia) |
| `PUT` | `/registration-management/patient-encounters/{encounterId}/referral` | Buat rincian (jalur Lab), lengkapi, atau koreksi | `PatientEncounter : Update` | Rencana (belum tersedia) |
| `POST` | `/registration-management/patient-encounters/{encounterId}/referral/documents` | Unggah 1–10 berkas (`multipart/form-data`, field `files`) | `PatientEncounter : Update` | Rencana (belum tersedia) |
| `POST` | `/registration-management/patient-encounters/kiosk/{encounterId}/referral/documents` | Unggah hasil scan Kiosk | Policy `KioskRead` | Rencana (belum tersedia) |
| `GET` | `/registration-management/patient-encounters/{encounterId}/referral/documents/{documentId}/content` | Isi berkas (`inline`, `no-store`) | `PatientEncounter : Read` | Rencana (belum tersedia) |
| `DELETE` | `/registration-management/patient-encounters/{encounterId}/referral/documents/{documentId}` | Soft delete berkas | `PatientEncounter : Update` | Rencana (belum tersedia) |

`PUT` request:

```json
{
  "expectedRowVersion": "9d2c…",
  "referralNumber": "RJK/2026/0991",
  "referralDateTime": "2026-10-08T09:10:00+07:00",
  "referralInstitutionId": "6b1e…",
  "referralDoctorId": null,
  "targetUnitType": 2,
  "targetServiceUnitId": "lab-unit…",
  "diagnosisId": "a1f0…",
  "diagnosisNote": null,
  "referralReason": "Pemeriksaan HbA1c"
}
```

`expectedRowVersion` kosong hanya sah saat rincian **belum ada** (jalur Lab). `targetUnitType`,
`targetServiceUnitId`, dan `targetClinicId` hanya dipakai saat membuat; pada rincian yang sudah ada
nilainya harus sama.

`GET` respons:

```json
{
  "id": "…", "encounterId": "…", "referralNumber": "RJK/2026/0991",
  "referralDateTime": "2026-10-08T09:10:00+07:00",
  "referralInstitution": { "id": "…", "name": "Klinik Sehat Sentosa", "isPartner": true },
  "referralDoctor": { "id": "…", "name": "dr. Rina" },
  "institutionIsPartnerSnapshot": true,
  "targetUnitType": 1, "targetUnitTypeName": "Poliklinik",
  "targetServiceUnitId": "…", "targetClinicId": "…", "targetUnitName": "Poli Penyakit Dalam",
  "diagnosis": { "id": "…", "code": "E11.9", "name": "Diabetes melitus tipe 2" },
  "diagnosisNote": null, "referralReason": "Kontrol gula darah tidak stabil",
  "captureSource": 1, "isComplete": true, "missingFields": [], "isLocked": false,
  "rowVersion": "9d2c…",
  "documents": [ { "id": "…", "originalFileName": "surat.jpg", "contentType": "image/jpeg", "sizeBytes": 412331, "pageOrder": 1 } ]
}
```

`missingFields` memakai nilai tetap: `referralNumber`, `referralDateTime`, `referralInstitution`,
`targetUnit`, `diagnosis`, `referralReason`, `documents`.

| Kode | Kapan |
|---|---|
| `404` | Kunjungan atau berkas tidak ada |
| `409` `RJ-VAL-PM-09` | Kunjungan `InConsultation` ke atas, `Cancelled`, atau `NoShow` |
| `400` `RJ-VAL-PM-10` | Mengubah unit tujuan rujukan yang sudah ada |
| `409` `RJ-VAL-PM-11` | `expectedRowVersion` basi |
| `400` `RJ-VAL-PM-12` | Format berkas bukan PDF/JPG/PNG (dicek dari isi) |
| `400` `RJ-VAL-PM-13` | Berkas > 5 MB, atau total berkas aktif > 10 |
| `403` `RJ-VAL-PM-14` | Kiosk: kunjungan bukan dari Kiosk, lebih dari 30 menit, atau status > `Queued` |

## Health Services / Registration Management / Outpatient Encounter

| Method | Path | Perubahan | Status |
|---|---|---|---|
| `GET` | `/registration-management/outpatient-encounters` | Respons per baris: `referralStatus` = `NotReferral` / `Complete` / `Incomplete`. Query baru `referralStatus` | Sudah ada — diperbarui (Rencana) |

## Health Services / Patient Management / Patient Insurance

| Method | Path | Perubahan | Status |
|---|---|---|---|
| `POST` | `/patient-management/master-data/patient-insurances`, `/kiosk`, `/admin` | Ruas opsional `cardScan: { scannedProviderName, scannedPolicyNumber }`; bila diisi, divalidasi PM.3.4 | Sudah ada — diperbarui (Rencana) |

| Kode | Kapan |
|---|---|
| `400` `RJ-VAL-PM-01` | Nama asuransi atau No. polis hasil scan tidak cocok — "Data tidak match" |
| `400` `RJ-VAL-PM-02` | `cardScan` diisi tetapi salah satu nilainya kosong |

## Health Services / Master Data / Referral Institution

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/master-data/referral-institutions/filters/metadata` | Metadata filter | `ReferralInstitution : Read` | Rencana |
| `GET` | `/master-data/referral-institutions/summary` | Ringkasan (total, aktif, mitra) | `ReferralInstitution : Read` | Rencana |
| `GET` | `/master-data/referral-institutions` | List + filter `isActive`, `isPartner`, `search` | `ReferralInstitution : Read` | Rencana |
| `GET` | `/master-data/referral-institutions/options` | Pilihan; **ditambah** `isPartner` | `ReferralInstitution : Read` | Sudah ada — diperbarui |
| `GET` | `/master-data/referral-institutions/kiosk/options` | Pilihan untuk Kiosk | Policy `KioskRead` | Rencana |
| `GET` | `/master-data/referral-institutions/{id}` | Detail | `ReferralInstitution : Read` | Rencana |
| `POST` | `/master-data/referral-institutions` | Create (`institutionCode`, `institutionName`, `address`, `phoneNumber`, `isPartner`) | `ReferralInstitution : Create` | Rencana |
| `PUT` | `/master-data/referral-institutions/{id}` | Update | `ReferralInstitution : Update` | Rencana |
| `PATCH` | `/master-data/referral-institutions/{id}/status` | Aktif/nonaktif | `ReferralInstitution : Update` | Rencana |
| `DELETE` | `/master-data/referral-institutions/{id}` | Soft delete; `409` bila dipakai kunjungan | `ReferralInstitution : Delete` | Rencana |

## Health Services / Master Data / Referral Doctor

Sama dengan Referral Institution (sembilan endpoint + `kiosk/options`), resource `ReferralDoctor`.
Ruas: `referralInstitutionId` (wajib, institusi aktif), `doctorName`, `isActive`. List menerima
filter `referralInstitutionId`.

## Amendment REF-REV — `RJ-DOC-REFERRAL-001@1.1.0` (aditif, 16 Okt 2026)

| Endpoint | Perubahan |
| --- | --- |
| `POST /patient-encounters/admin` | `referral.sourceType` (`1` Eksternal, `2` Internal; kosong = `1`), `referral.sourceDoctorCertificateId` (wajib bila Internal). Internal: nomor, dokter perujuk, tanggal diambil dari surat; `referralInstitutionId`/`referralNumber` diabaikan. Eksternal RJ: `referralNumber`, `diagnosisId`, `referralReason` opsional. Konflik dalam transaksi → `409` |
| `GET /patient-encounters/internal-referral-letters?patientId=&clinicId=` (baru) | `PatientEncounter : Read`. Surat Rujukan terbit milik pasien, bertujuan poliklinik, belum dipakai kunjungan aktif. Tanpa diagnosa/alasan |
| `GET /patient-encounters/{id}/referral` | Tambahan `sourceType`, `sourceTypeName`, `sourceLetter`; `missingFields` dapat berisi `sourceLetter` |
| `PUT /patient-encounters/{id}/referral`, `POST …/referral/documents` | Rujukan internal ditolak `RJ-VAL-PM-22` |
| `GET /outpatient-encounters/{id}` | Tambahan `referral` (= `EncounterReferralResponse`) dan `referralLetters[]` (`certificateId`, `certificateNumber`, `direction` `outgoing`/`incoming`, `issuedDate`, `doctorName`, `sourceClinicName`, `targetName`, `certificateStatus`, `canPrint`, `followUpEncounterNumber`) |
| `GET /outpatient-encounters/{id}/referral-letters/{certificateId}` (baru) | `OutpatientEncounter : Read`, cakupan daftar. Surat harus tertaut (diterbitkan pada kunjungan atau dasar rujukannya), status Terbit. `200` `DoctorCertificateResponse`, `400` dibatalkan, `404` tidak tertaut/di luar cakupan. `Cache-Control: no-store` |
| `POST /doctor-certificates` tipe `inpatientReferral` | DPJP saja (`403`), kunjungan RJ poli status ≥ Sedang Konsultasi (`409`), tujuan `targetClinicId` poli aktif ≠ poli kunjungan, diagnosa & alasan wajib; surat ke tujuan sama yang masih terbit `409`; identitas/SIP/tanggal dari server; `requestedRoomClass` diabaikan |
| `PUT /doctor-certificates/{id}`, `POST …/cancel` | Surat Rujukan yang menjadi dasar kunjungan aktif `409` |
| `GET /doctor-certificates/referral-targets?certificateType=inpatientReferral` | Hanya `kind = clinic` |

### Tambahan 16 Okt 2026 — Surat Pengantar Rawat Inap

`POST /doctor-certificates` tipe `inpatientAdmission`: `inpatientAdmission.patientDependencyLevel` **wajib** (`1` Low, `2` Moderate, `3` High Dependency); selain itu `400`. Respons `inpatientAdmission` menambah `patientDependencyLevel` (int, `null` untuk surat lama) dan `patientDependencyLevelName`.

---

## Amendment RP — `RJ-PRI-RESV-001@1.1.0` (`approved`, 19 Okt 2026)

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-PRI-RESV-001@1.1.0` |
| Status | `approved`. Owner Sukma Giri. `1.0.0`: `approved_by` Sukma Giri, `approved_at` 2026-10-16 (`RJ-PRI-DEC-030`). `1.1.0`: `approved_by` Sukma Giri, `approved_at` 2026-10-19 (`RJ-PRI-DEC-032`) |
| Riwayat versi | `1.0.0` — kontrak awal. `1.1.0` — **aditif, kompatibel mundur**: item daftar menambah `createdAt`, `createdByName`, `updatedAt`, `updatedByName`; `GET /summary` menerima `visitDateFrom`/`visitDateTo` dan respons menambah keduanya (`visitDate` tunggal tetap diterima). Dipicu revisi layar Daftar Reservasi Prioritas |
| `input_revision` | Keputusan `RJ-PRI-DEC-001` s.d. `029`; impact scan `CAP-RP-01` s.d. `22` (BE `551e1cbb`, FE `1fd858322`) |
| Dampak kompatibilitas | Aditif untuk grup baru. **Mengubah perilaku**: `POST /patient-encounters/admin` (field baru opsional; klasifikasi booking dinilai pada hari pendaftaran), Queue Display Runtime (antrean prioritas hilang dari respons), dan hub `/hubs/queues` (akun display ditolak di group perawat dan dokter) |

Semua respons memakai `ApiResponse<T>`. Tanggal kalender (`visitDate`) memakai format `yyyy-MM-dd` dalam WIB.

### Health Services / Registration Management / Outpatient Priority Reservation

Base URL: `api/v1/health-services/registration-management/outpatient-priority-reservations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar reservasi pada rentang tanggal kunjungan | `OutpatientPriorityReservation : Read` | `PriorityReservationListQuery` | `ApiResponse<PagedResult<PriorityReservationListItemResponse>>` | Tersedia (`BE-RJP-006`; diperbarui `1.1.0`) |
| `GET` | `/summary` | Jumlah per status pada rentang tanggal kunjungan | `OutpatientPriorityReservation : Read` | `visitDateFrom`, `visitDateTo` (bawaan hari ini); `visitDate` (`1.0.0`, satu hari) tetap diterima | `ApiResponse<PriorityReservationSummaryResponse>` | Tersedia (`BE-RJP-006`; diperbarui `1.1.0`) |
| `GET` | `/{id}` | Detail + riwayat | `OutpatientPriorityReservation : Read` | — | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `GET` | `/eligibility` | Cek apakah pasien boleh reservasi prioritas hari ini | `OutpatientPriorityReservation : Read` | `patientId`, `visitDate` | `ApiResponse<PriorityReservationEligibilityResponse>` | Tersedia (`BE-RJP-006`) |
| `POST` | `/` | Buat reservasi | `OutpatientPriorityReservation : Create` | `CreatePriorityReservationRequest` | `ApiResponse<PriorityReservationIssuedResponse>` | Tersedia |
| `POST` | `/{id}/arrival-code` | Kirim ulang: terbitkan kode baru, kode lama mati | `OutpatientPriorityReservation : Create` | `PriorityReservationRowVersionRequest` | `ApiResponse<PriorityReservationIssuedResponse>` | Tersedia |
| `POST` | `/{id}/arrival` | Petugas menandai tiba | `OutpatientPriorityReservation : Complete` | `PriorityReservationRowVersionRequest` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `POST` | `/{id}/arrival-reversal` | Kembalikan ke Belum Tiba | `OutpatientPriorityReservation : Complete` | `PriorityReservationReasonRequest` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `PUT` | `/{id}/screening` | Simpan checklist skrining awal | `OutpatientPriorityReservation : Screen` | `PriorityReservationScreeningRequest` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `POST` | `/{id}/cancel` | Batalkan dengan alasan | `OutpatientPriorityReservation : Cancel` | `PriorityReservationReasonRequest` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |

**DTO**

| Class | Jenis | Field |
|---|---|---|
| `PriorityReservationListQuery` | PagedQuery | `visitDateFrom`, `visitDateTo` (bawaan hari ini), `status?`, `search?` (No. RM, nama, nomor reservasi; minimal 3 karakter), `pageNumber`, `pageSize` |
| `PriorityReservationListItemResponse` | Response | `id`, `reservationNumber`, `patientId`, `medicalRecordNumber`, `patientName`, `birthDate`, `visitDate`, `status`, `statusName`, `arrivedAt?`, `arrivalSource?`, `isScreened`, `membershipTierCodeSnapshot?`, `isPriorityRevoked`, `encounterId?`, `rowVersion`. **`1.1.0`:** `createdAt` (UTC), `createdByName?`, `updatedAt?` (kosong bila belum pernah berubah sejak dibuat), `updatedByName?` (nama petugas pada riwayat terakhir; `Link Pasien` atau `Sistem` bila perubahan terakhir dari link konfirmasi atau penutupan otomatis) |
| `PriorityReservationSummaryResponse` | Response | `visitDate` (= `visitDateFrom`, untuk `1.0.0`), **`1.1.0`:** `visitDateFrom`, `visitDateTo`; `awaitingArrival`, `arrived`, `converted`, `cancelled`, `noShow`, `registeredElsewhere` |
| `PriorityReservationDetailResponse` | Response | Field list item + `screening { cough, fever, fallRisk, screenedAt, screenedByName }`, `closedAt?`, `closureReason?`, `arrivalCodeIssuedAt`, `canMarkArrival`, `canRevertArrival`, `canScreen`, `canReissueCode`, `canComplete`, `canCancel`, `histories[] { eventCode, eventName, fromStatus?, toStatus, source, actorName?, reason?, occurredAt }` |
| `PriorityReservationEligibilityResponse` | Response | `isEligible`, `reasonCode` (`ELIGIBLE`, `NOT_ACTIVE_MEMBER`, `OPEN_RESERVATION_EXISTS`, `ACTIVE_ENCOUNTER_EXISTS`, `DATE_OUT_OF_RANGE`), `message`, `membershipTierCode?`, `existingReservationNumber?` |
| `CreatePriorityReservationRequest` | Create | `patientId` (wajib), `visitDate` (wajib) |
| `PriorityReservationIssuedResponse` | Response | `reservation` (`PriorityReservationDetailResponse`), `arrivalLink` (URL lengkap, **hanya** di respons ini), `whatsAppMessageText` (teks siap pakai tanpa poli/dokter), `patientPhoneNumber?` (dari master, untuk tombol `wa.me`) |
| `PriorityReservationRowVersionRequest` | Status | `expectedRowVersion` (wajib) |
| `PriorityReservationReasonRequest` | Status | `expectedRowVersion` (wajib), `reason` (wajib, 10–500 karakter) |
| `PriorityReservationScreeningRequest` | Update | `expectedRowVersion` (wajib), `cough`, `fever`, `fallRisk` (ketiganya wajib, `bool`) |

**Kode status**

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Berhasil |
| `400` | Isian tidak lengkap atau salah (misalnya alasan kurang dari 10 karakter, tanggal lampau) |
| `403` | Pengguna tidak punya hak untuk tindakan ini |
| `404` | Reservasi atau pasien tidak ditemukan |
| `409` | Tindakan tidak sesuai keadaan reservasi saat ini, atau data baru saja diubah petugas lain |

### Health Services / Registration Management / Priority Reservation Arrival

Base URL: `api/v1/health-services/registration-management/priority-reservation-arrivals`

Tanpa login (`[AllowAnonymous]`), dibatasi `PriorityReservationArrivalPublic` (bawaan 20 permintaan per menit per IP).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{code}` | Halaman link: tampilkan keadaan reservasi | Publik (kode acak) | — | `ApiResponse<PriorityReservationArrivalPublicResponse>` | Tersedia (`BE-RJP-012`) |
| `POST` | `/{code}/confirm` | Pasien menekan "Saya sudah tiba" | Publik (kode acak) | — | `ApiResponse<PriorityReservationArrivalPublicResponse>` | Tersedia (`BE-RJP-012`) |

| Class | Jenis | Field |
|---|---|---|
| `PriorityReservationArrivalPublicResponse` | Response | `reservationNumber`, `visitDate`, `state` (`NOT_YET_OPEN`, `READY`, `CONFIRMED`), `confirmedAt?`, `message`. **Tidak** memuat nama, No. RM, NIK, tingkat member, poli, dokter, atau ID internal |

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Link berlaku. `state` menjelaskan apakah konfirmasi sudah dapat dilakukan |
| `404` | "Link tidak berlaku. Silakan hubungi petugas pendaftaran." Jawaban sama untuk kode salah, diganti, dibatalkan, tidak datang, atau didaftarkan lewat jalur lain |
| `429` | "Terlalu banyak percobaan. Coba lagi sebentar lagi." |

Contoh: link Ibu Sari dibuka Kamis 15 Okt. Responsnya `200` dengan `state = NOT_YET_OPEN` dan pesan "Konfirmasi
kedatangan baru bisa dilakukan pada 16 Okt 2026". Pada Jumat pukul 08.40, `POST /confirm` mengembalikan
`state = CONFIRMED`, `confirmedAt` 08.40. Bila link dibuka lagi pukul 08.45, `GET` mengembalikan `CONFIRMED` dengan
jam 08.40.

### Health Services / Registration Management / Patient Encounter (diperbarui)

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/admin` | Daftar kunjungan. Bila `priorityReservationId` diisi, sekaligus melengkapi reservasi | `PatientEncounter : Create`. Bila `priorityReservationId` diisi, juga `OutpatientPriorityReservation : Complete` (diperiksa service) | `PatientEncounterCreateRequest` + `priorityReservationId?` | `ApiResponse<PatientEncounterCreateResponse>`; `queueClassification.priorityWaitingLocationName?` | Diperbarui (tersedia) |
| `POST` | `/`, `/kiosk` | Daftar kunjungan dari kiosk | Policy kiosk | `PatientEncounterCreateRequest`. `priorityReservationId` **ditolak** (400) | Sama | Diperbarui (tersedia) |

Perubahan perilaku pada ketiga path: klasifikasi member dinilai pada hari pendaftaran (WIB). Kunjungan Rawat Jalan
berpoliklinik tanpa `priorityReservationId` menutup reservasi terbuka pasien pada tanggal yang sama sebagai Didaftarkan
lewat Jalur Lain.

### Health Services / Registration Management / Queue Display Runtime (diperbarui)

| Method | Path | Perubahan | Status |
|---|---|---|---|
| `GET` | `/summary`, `/items`, `/called` | Antrean dengan `IsPriorityQueue = true` tidak disertakan dan tidak dihitung | Diperbarui (tersedia) |

### Nurse Station Queue dan Doctor Queue (diperbarui)

| Response | Field baru | Isi |
|---|---|---|
| Item antrean Nurse Station dan Doctor Queue | `priorityWaitingLocationName?` | Nama ruang tunggu prioritas dari konfigurasi bila `isPriorityQueue = true`, selain itu `null` |

### Hub realtime `/hubs/queues` (diperbarui)

| Method / event | Pemanggil | Isi | Status |
|---|---|---|---|
| `JoinQueueDisplayCluster(clusterId)` | Akun display | Bergabung ke `queue-display-cluster:{clusterId}`. Ditolak bila bukan akun display atau tanpa klaim cluster | Tersedia |
| `JoinPriorityReservationBoard()` | Petugas pemegang `OutpatientPriorityReservation : Read` | Bergabung ke `priority-reservation-board` | Tersedia |
| `JoinNurseStationCluster`, `JoinAccessibleNurseStationClusters`, `JoinDoctor*` | Petugas | **Menolak** akun display | Diperbarui (tersedia) |
| Event `QueueDisplayChanged` | Ke group display | `QueueDisplayRealtimeEventResponse`: `eventType`, `queueId`, `serviceUnitId`, `clinicId`, `queueDate`, `queueNumber`, `queueCode`, `queueStatus`, `queueStatusName`, `occurredAt`. Tidak dikirim untuk antrean prioritas | Tersedia |
| Event `PriorityReservationChanged` | Ke `priority-reservation-board` | `PriorityReservationRealtimeEventResponse`: `reservationId`, `visitDate`, `status`, `eventCode`, `occurredAt`. Tanpa nama pasien; layar memuat ulang daftar | Tersedia |

## Amendment RP-GT — `RJ-PRI-RESV-001@1.2.0` (`approved`, 21 Okt 2026)

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-PRI-RESV-001@1.2.0` |
| Status | `approved`. Owner Sukma Giri. `approved_by` Sukma Giri, `approved_at` 2026-10-21 (`RJ-PRI-DEC-039`) |
| Riwayat versi | `1.2.0` — **aditif, kompatibel mundur**: endpoint baru `POST /{id}/reschedule`; field baru `canReschedule` pada detail, `fromVisitDate`/`toVisitDate` pada riwayat, dan `previousVisitDate` pada event realtime; kode riwayat baru `RESCHEDULED` |
| `input_revision` | Keputusan `RJ-PRI-DEC-034` s.d. `038` (21 Okt 2026); fakta `F-GT-1` s.d. `F-GT-7` (BE `b0285ff1`, FE `037c975b7`) |
| Dampak kompatibilitas | Klien lama tetap jalan. Field baru dapat diabaikan. Layar daftar yang belum membaca `previousVisitDate` tetap benar, tetapi baris di tanggal lama baru hilang saat muat ulang berkala |

### Health Services / Registration Management / Outpatient Priority Reservation

Base URL: `api/v1/health-services/registration-management/outpatient-priority-reservations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{id}/reschedule` | Ganti tanggal kunjungan sebelum hari kunjungan. Nomor tetap, status member dinilai ulang, link baru terbit dan link lama mati | `OutpatientPriorityReservation : Create` | `ReschedulePriorityReservationRequest` | `ApiResponse<PriorityReservationIssuedResponse>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail + riwayat | `OutpatientPriorityReservation : Read` | — | `ApiResponse<PriorityReservationDetailResponse>` (+ `canReschedule`, riwayat + `fromVisitDate?`/`toVisitDate?`) | Diperbarui (rencana) |

**DTO**

| Class | Jenis | Field |
|---|---|---|
| `ReschedulePriorityReservationRequest` | Status | `expectedRowVersion` (`Guid`, wajib), `newVisitDate` (`yyyy-MM-dd`, wajib), `reason` (wajib, 10–500 karakter setelah spasi di tepi dibuang) |
| `PriorityReservationDetailResponse` | Response | **`1.2.0`:** `canReschedule` = status Menunggu Kedatangan **dan** hari ini (WIB) < `visitDate`. Nilai ini tidak memeriksa status member atau tanggal baru; keduanya diperiksa saat simpan |
| `PriorityReservationHistoryResponse` | Response | **`1.2.0`:** `fromVisitDate?`, `toVisitDate?` (`yyyy-MM-dd`). Hanya terisi pada `eventCode = RESCHEDULED`; `eventName` "Tanggal kunjungan diganti" |
| `PriorityReservationIssuedResponse` | Response | Tidak berubah bentuknya. Pada ganti tanggal, `reservation.visitDate` sudah tanggal baru, `arrivalLink` adalah link baru (hanya di respons ini), dan `whatsAppMessageText` memakai versi ganti tanggal (`RJ-PRI-FE-005`) |

Contoh request:

```json
{
  "expectedRowVersion": "5c1b0f0e-2a8e-4d55-9a54-1f3c2d9e7a10",
  "newVisitDate": "2026-10-28",
  "reason": "Pasien ada acara keluarga pada tanggal semula"
}
```

Contoh isi `whatsAppMessageText` (kalimat persisnya `DEV_DISCRETION`):

```text
Tanggal reservasi pasien prioritas Anda sudah diganti.
Nomor reservasi: RSV-261020-0003
Tanggal kunjungan baru: Senin, 28 Oktober 2026

Link yang dikirim sebelumnya tidak berlaku lagi. Saat tiba di rumah sakit pada tanggal tersebut, buka link berikut lalu tekan "Saya sudah tiba":
https://…/priority-arrival/<kode baru>

Petugas kami akan menghampiri Anda di Ruang Tunggu Prioritas.
```

**Kode status `POST /{id}/reschedule`**

| Kode | Arti bagi pengguna | Aturan |
|---|---|---|
| `200` | Tanggal diganti. Kirim link baru ke pasien | — |
| `400` | Isian kurang atau salah: tanggal baru kosong, sama dengan tanggal lama, sebelum hari ini, terlalu jauh, atau alasan kurang dari 10 karakter | `VAL-RP-03`, `04`, `12`, `23`, `24` |
| `403` | Pengguna tidak punya hak `Create` pada Reservasi Prioritas | — |
| `404` | Reservasi tidak ditemukan | — |
| `409` | Tidak sesuai keadaan: bukan Menunggu Kedatangan, sudah hari kunjungan, pasien bukan member Aktif lagi, tanggal baru sudah terisi reservasi atau kunjungan, data baru saja diubah petugas lain, atau fitur dinonaktifkan | `VAL-RP-05`, `06`, `11`, `14`, `22`, `25` |

Pada semua kode selain `200`, reservasi, snapshot, dan link lama **tidak berubah**.

### Hub realtime `/hubs/queues` (diperbarui `1.2.0`)

| Event | Isi | Status |
|---|---|---|
| `PriorityReservationChanged` | `PriorityReservationRealtimeEventResponse` + **`previousVisitDate?`**. Pada `eventCode = RESCHEDULED`, `visitDate` berisi tanggal baru dan `previousVisitDate` berisi tanggal lama. Pada event lain, `previousVisitDate` kosong | Diperbarui (rencana) |

Layar daftar memuat ulang bila `visitDate` **atau** `previousVisitDate` berada di rentang saringannya. Contoh: layar yang
menyaring 25 Okt menerima event `visitDate = 2026-10-28`, `previousVisitDate = 2026-10-25`, lalu memuat ulang, sehingga
baris Ibu Sari hilang dari daftar 25 Okt.

### Health Services / Registration Management / Priority Reservation Arrival

Tidak berubah. Kode lama yang sudah diganti oleh ganti tanggal dijawab `404` "Link tidak berlaku…", sama seperti setelah
kirim ulang (`VAL-RP-07`). Kode baru dijawab `200` `NOT_YET_OPEN` dengan tanggal baru sampai hari kunjungan tiba.
