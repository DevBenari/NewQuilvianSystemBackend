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
