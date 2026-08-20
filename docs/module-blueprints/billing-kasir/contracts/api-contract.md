# Billing dan Kasir — API Contract

`contract_version: BIL-API-0.4` · status **approved** · owner API/Billing/Security · approved 20 Agustus 2026 · input decision `0.2`/hash tercatat di manifest · kompatibilitas: additive API baru. Semua endpoint transaksi berikut **Rencana (belum tersedia)**.

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Cari invoice | `BillingInvoice : Read` | query filter | `ApiResponse<Paged<InvoiceSummaryResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail dan versi | `BillingInvoice : Read` | — | `ApiResponse<InvoiceDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/from-source` | Tambah/update charge idempotent | `BillingInvoice : Create` | `UpsertChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/recalculate` | Buat calculation version | `BillingInvoice : Update` | `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/items/{itemId}/void` | Void item eligible | `BillingInvoice : Update` | `VoidInvoiceItemRequest` | `ApiResponse<InvoiceDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/discounts` | Terapkan diskon | `BillingDiscount : Create` | `ApplyDiscountRequest` | `ApiResponse<DiscountResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/discounts/{discountId}/approve` | Approve diskon dokter | `BillingDoctorDiscount : Approve` | `ApproveDiscountRequest` | `ApiResponse<DiscountResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Lihat saldo/ledger | `BillingDeposit : Read` | — | `ApiResponse<DepositResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/deposits/{encounterId}/top-ups` | Top-up deposit | `BillingDeposit : Create` | `DepositTopUpRequest` | `ApiResponse<SettlementResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/deposits/{encounterId}/allocations` | Progress allocation | `BillingDeposit : Allocate` | `DepositAllocationRequest` | `ApiResponse<AllocationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/settlements` | Mulai pembayaran split | `BillingPayment : Create` | `CreateSettlementRequest` | `ApiResponse<SettlementResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/settlements/{id}/tenders` | Tambah attempt tender | `BillingPayment : Create` | `CreateTenderRequest` | `ApiResponse<TenderResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/settlements/{id}` | Status tender/alokasi | `BillingPayment : Read` | — | `ApiResponse<SettlementResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Billing / Financial Exceptions

Base URL: `api/v1/health-services/billing-management/billing/financial-exceptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/adjustments` | Ajukan koreksi | `BillingAdjustment : Create` | `CreateAdjustmentRequest` | `ApiResponse<AdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{id}/approve` | Approve Finance | `BillingAdjustment : Approve` | `ApprovalRequest` | `ApiResponse<AdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/refunds` | Ajukan refund | `BillingRefund : Create` | `CreateRefundRequest` | `ApiResponse<RefundResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/refunds/{id}/approve` | Approve refund | `BillingRefund : Approve` | `ApprovalRequest` | `ApiResponse<RefundResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/write-offs` | Ajukan write-off | `BillingWriteOff : Create` | `CreateWriteOffRequest` | `ApiResponse<WriteOffResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/write-offs/{id}/approve` | Approve write-off | `BillingWriteOff : Approve` | `ApprovalRequest` | `ApiResponse<WriteOffResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{type}/{id}/reverse` | Entry reversal | `BillingFinancialException : Reverse` | `ReverseExceptionRequest` | `ApiResponse<AdjustmentResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Cashier / Shifts

Base URL: `api/v1/health-services/billing-management/cashier/shifts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/open` | Buka shift | `CashierShift : Create` | `OpenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/current` | Shift aktif | `CashierShift : Read` | — | `ApiResponse<CashierShiftResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/handover` | Serah-terima dua kasir | `CashierShift : Handover` | `HandoverShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close` | Catat fisik dan tutup | `CashierShift : Close` | `CloseShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/variance-reviews` | Review selisih | `CashierShift : Review` | `ReviewVarianceRequest` | `ApiResponse<CashVarianceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reopen` | Reopen berotorisasi | `CashierShift : Reopen` | `ReopenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/invoices/{invoiceId}/preview` | Validasi kesiapan | `BillingFinalization : Read` | — | `ApiResponse<FinalizationPreviewResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/invoices/{invoiceId}` | Finalisasi dan handoff | `BillingFinalization : Create` | `FinalizeInvoiceRequest` | `ApiResponse<FinalizationResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/handoffs` | Status AR/AP handoff | `BillingFinalization : Read` | — | `ApiResponse<HandoffStatusResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Master Data / Administration Fee Policy

Base URL: `api/v1/health-services/billing-management/master-data/administration-fee-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar versi policy | `AdministrationFeePolicy : Read` | filter tanggal/status | `ApiResponse<Paged<AdministrationFeePolicyResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat versi effective-dated | `AdministrationFeePolicy : Create` | `CreateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Koreksi versi belum efektif | `AdministrationFeePolicy : Update` | `UpdateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan tanpa hapus histori | `AdministrationFeePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Master Data / Discount Policy

Base URL: `api/v1/health-services/billing-management/master-data/discount-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar policy | `DiscountPolicy : Read` | filter | `ApiResponse<Paged<DiscountPolicyResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat policy | `DiscountPolicy : Create` | `CreateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `DiscountPolicy : Update` | `UpdateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `DiscountPolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Master Data / Tax Rule

Base URL: `api/v1/health-services/billing-management/master-data/tax-rules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar tax rule | `TaxRule : Read` | filter | `ApiResponse<Paged<TaxRuleResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat tax rule | `TaxRule : Create` | `CreateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `TaxRule : Update` | `UpdateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `TaxRule : Update` | `DeactivatePolicyRequest` | `ApiResponse<TaxRuleResponse>` | **Rencana (belum tersedia)** |

### Health Services / Billing Management / Master Data / Room Charge Policy

Base URL: `api/v1/health-services/billing-management/master-data/room-charge-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar room rule | `RoomChargePolicy : Read` | filter | `ApiResponse<Paged<RoomChargePolicyResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat room rule | `RoomChargePolicy : Create` | `CreateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `RoomChargePolicy : Update` | `UpdateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `RoomChargePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Rencana (belum tersedia)** |

Existing master tetap memakai tags existing `Health Services / Billing Management / Master Data / Payment Method` dan `... / Billing Item Category`; kontraknya direuse dan tidak diklaim sebagai endpoint baru.

## HTTP semantics

`200/201` sukses; `400` input tidak valid; `403` hak tidak ada; `404` resource tidak ditemukan; `409` version/state/idempotency conflict; `422` aturan bisnis tidak terpenuhi; `502/504` provider belum memberi hasil definitif dan tender tetap `PENDING`. Semua command membawa `Idempotency-Key` dan version token bila aggregate mutable. Trace: `BKC-DEC-031`–`044`, validation `BIL-VAL-*`, tests `BIL-AT-001`–`024`.

Security/privacy: response memakai data pasien minimum, field sensitif dimask, dan provider token/payload tidak pernah menjadi DTO atau custom log. Exception provider, concurrency, dan unauthorized harus mempertahankan correlation ID tanpa membocorkan payload.
