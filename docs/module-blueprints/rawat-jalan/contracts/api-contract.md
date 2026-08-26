# API Contract — Rawat Jalan Billing

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

