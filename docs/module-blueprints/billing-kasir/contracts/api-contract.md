# Billing dan Kasir — API Contract

`contract_version: BIL-API-0.4` · status **approved** · owner API/Billing/Security · approved 20 Agustus 2026 · input decision `0.2`/hash tercatat di manifest · kompatibilitas: additive API baru.

**Rekonsiliasi 25 Agustus 2026 (`ISSUE-FE-003`)**: seluruh endpoint transaksi di bawah **sudah diimplementasikan** di backend source sejak commit `1d61a5b` (part 1) dan `22bf9cf` (part 2) pada branch `Yasmina`/`AgentCodexBackend` — dokumen ini sebelumnya masih menandainya "Rencana (belum tersedia)" secara menyeluruh, padahal source sudah ada. Status "Diimplementasikan" pada tabel di bawah berarti **source backend ada dan sudah dibaca langsung dari controller/service terkait**, BUKAN berarti sudah diverifikasi lewat klik-coba ter-autentikasi atau migration sudah dieksekusi ke database bersama — keduanya masih tertunda untuk sebagian besar slice (lihat `MODULE-STATUS.md`). Endpoint `GET` tambahan pada Cashier Shifts (`{id}`) dan Financial Exceptions (`invoices/{invoiceId}`, `invoices/{invoiceId}/refundable-credits`, `refunds/{id}`, `adjustments/{id}`, `write-offs/{id}`), serta seluruh Master Data / Register, ditambahkan hari ini (`ISSUE-FE-006`, `ISSUE-FE-007`, `ISSUE-FE-008`) dan **belum tercantum** di tabel endpoint di bawah karena dokumen ini belum ditulis ulang penuh — lihat laporan task masing-masing untuk detail endpoint baru tersebut.

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Cari invoice | `BillingInvoice : Read` | query filter | `ApiResponse<Paged<InvoiceSummaryResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/{id}` | Detail dan versi | `BillingInvoice : Read` | — | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/from-source` | Tambah/update charge idempotent | `BillingInvoice : Create` | `UpsertChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/recalculate` | Buat calculation version | `BillingInvoice : Update` | `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/items/{itemId}/void` | Void item eligible | `BillingInvoice : Update` | `VoidInvoiceItemRequest` | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/discounts` | Terapkan diskon | `BillingDiscount : Create` | `ApplyDiscountRequest` | `ApiResponse<DiscountResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/discounts/{discountId}/approve` | Approve diskon dokter | `BillingDoctorDiscount : Approve` | `ApproveDiscountRequest` | `ApiResponse<DiscountResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Lihat saldo/ledger | `BillingDeposit : Read` | — | `ApiResponse<DepositResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/deposits/{encounterId}/top-ups` | Top-up deposit | `BillingDeposit : Create` | `DepositTopUpRequest` | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/deposits/{encounterId}/allocations` | Progress allocation | `BillingDeposit : Allocate` | `DepositAllocationRequest` | `ApiResponse<AllocationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/settlements` | Mulai pembayaran split | `BillingPayment : Create` | `CreateSettlementRequest` | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/settlements/{id}/tenders` | Tambah attempt tender | `BillingPayment : Create` | `CreateTenderRequest` | `ApiResponse<TenderResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/settlements/{id}` | Status tender/alokasi | `BillingPayment : Read` | — | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Financial Exceptions

Base URL: `api/v1/health-services/billing-management/billing/financial-exceptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/adjustments` | Ajukan koreksi | `BillingAdjustment : Create` | `CreateAdjustmentRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/adjustments/{id}/approve` | Approve Finance | `BillingAdjustment : Approve` | `ApprovalRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/refunds` | Ajukan refund | `BillingRefund : Create` | `CreateRefundRequest` | `ApiResponse<RefundResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/refunds/{id}/approve` | Approve refund | `BillingRefund : Approve` | `ApprovalRequest` | `ApiResponse<RefundResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/write-offs` | Ajukan write-off | `BillingWriteOff : Create` | `CreateWriteOffRequest` | `ApiResponse<WriteOffResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/write-offs/{id}/approve` | Approve write-off | `BillingWriteOff : Approve` | `ApprovalRequest` | `ApiResponse<WriteOffResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{type}/{id}/reverse` | Entry reversal | `BillingFinancialException : Reverse` | `ReverseExceptionRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Cashier / Shifts

Base URL: `api/v1/health-services/billing-management/cashier/shifts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/open` | Buka shift | `CashierShift : Create` | `OpenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/current` | Shift aktif | `CashierShift : Read` | — | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/handover` | Serah-terima dua kasir | `CashierShift : Handover` | `HandoverShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/close` | Catat fisik dan tutup | `CashierShift : Close` | `CloseShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/variance-reviews` | Review selisih | `CashierShift : Review` | `ReviewVarianceRequest` | `ApiResponse<CashVarianceResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/reopen` | Reopen berotorisasi | `CashierShift : Reopen` | `ReopenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/invoices/{invoiceId}/preview` | Validasi kesiapan | `BillingFinalization : Read` | — | `ApiResponse<FinalizationPreviewResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/invoices/{invoiceId}` | Finalisasi dan handoff | `BillingFinalization : Create` | `FinalizeInvoiceRequest` | `ApiResponse<FinalizationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/{id}/handoffs` | Status AR/AP handoff | `BillingFinalization : Read` | — | `ApiResponse<HandoffStatusResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Administration Fee Policy

Base URL: `api/v1/health-services/billing-management/master-data/administration-fee-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar versi policy | `AdministrationFeePolicy : Read` | filter tanggal/status | `ApiResponse<Paged<AdministrationFeePolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat versi effective-dated | `AdministrationFeePolicy : Create` | `CreateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi versi belum efektif | `AdministrationFeePolicy : Update` | `UpdateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan tanpa hapus histori | `AdministrationFeePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Discount Policy

Base URL: `api/v1/health-services/billing-management/master-data/discount-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar policy | `DiscountPolicy : Read` | filter | `ApiResponse<Paged<DiscountPolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat policy | `DiscountPolicy : Create` | `CreateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `DiscountPolicy : Update` | `UpdateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `DiscountPolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Tax Rule

Base URL: `api/v1/health-services/billing-management/master-data/tax-rules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar tax rule | `TaxRule : Read` | filter | `ApiResponse<Paged<TaxRuleResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat tax rule | `TaxRule : Create` | `CreateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `TaxRule : Update` | `UpdateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `TaxRule : Update` | `DeactivatePolicyRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Room Charge Policy

Base URL: `api/v1/health-services/billing-management/master-data/room-charge-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar room rule | `RoomChargePolicy : Read` | filter | `ApiResponse<Paged<RoomChargePolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat room rule | `RoomChargePolicy : Create` | `CreateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `RoomChargePolicy : Update` | `UpdateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `RoomChargePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

Existing master tetap memakai tags existing `Health Services / Billing Management / Master Data / Payment Method` dan `... / Billing Item Category`; kontraknya direuse dan tidak diklaim sebagai endpoint baru.

## HTTP semantics

`200/201` sukses; `400` input tidak valid; `403` hak tidak ada; `404` resource tidak ditemukan; `409` version/state/idempotency conflict; `422` aturan bisnis tidak terpenuhi; `502/504` provider belum memberi hasil definitif dan tender tetap `PENDING`. Semua command membawa `Idempotency-Key` dan version token bila aggregate mutable. Trace: `BKC-DEC-031`–`044`, validation `BIL-VAL-*`, tests `BIL-AT-001`–`024`.

Security/privacy: response memakai data pasien minimum, field sensitif dimask, dan provider token/payload tidak pernah menjadi DTO atau custom log. Exception provider, concurrency, dan unauthorized harus mempertahankan correlation ID tanpa membocorkan payload.
