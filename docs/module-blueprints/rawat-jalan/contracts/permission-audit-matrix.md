# Permission and Audit Matrix — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-PERM-001@1.0.0` |
| Status | `draft` |
| Catatan | String permission AS-IS dipertahankan; permission target baru perlu registry/security approval |

| Endpoint | Resource | Action | String yang dipakai | Audit logger |
|---|---|---|---|:---:|
| `GET /by-encounter/{encounterId}` | `BillingFolio` | `Read` | `[AccessPermission("BillingFolio", "Read")]` | Tidak |
| `GET /{folioId}` | `BillingFolio` | `Read` | `[AccessPermission("BillingFolio", "Read")]` | Tidak |
| `POST /internal/milestones/recognize` | `BillingMilestone` | `RecognizeInternal` | `[AccessPermission("BillingMilestone", "RecognizeInternal")]` | Ya |
| `POST /{folioId}/allocations` | `BillingAllocation` | `Create` | `[AccessPermission("BillingAllocation", "Create")]` | Ya; rencana |
| `POST /{folioId}/financial-actions` | `BillingFinancialAction` | `Create` | `[AccessPermission("BillingFinancialAction", "Create")]` | Ya; rencana |
| `POST /{folioId}/close` | `BillingFolio` | `Close` | `[AccessPermission("BillingFolio", "Close")]` | Ya; rencana |
| `POST /{folioId}/reopen` | `BillingFolio` | `Reopen` | `[AccessPermission("BillingFolio", "Reopen")]` | Ya; rencana |
| `POST /{id}/execute` | `BillingFinancialAction` | `Execute` | `[AccessPermission("BillingFinancialAction", "Execute")]` | Ya; rencana |
| `POST /{id}/resolve` | `BillingReconciliation` | `Resolve` | `[AccessPermission("BillingReconciliation", "Resolve")]` | Ya; rencana |

Audit wajib menyimpan actor, action, entity/reference ID, prior/new status, reason, policy/rule
version, correlation, dan timestamp. Idempotency key hanya dicatat sebagai hash/reference;
payload klinis sensitif dan raw payer payload tidak masuk custom logger.

