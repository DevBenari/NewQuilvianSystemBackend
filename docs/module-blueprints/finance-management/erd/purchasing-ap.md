# ERD — Purchasing/AP (Purchase Order, Tukar Faktur, Purchasing Invoice, Retur)

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `4` |
| Status | `draft` |
| Backend SHA | `96bf9746` |
| Ditambahkan | AMENDMENT REVISI 4, 25 September 2026 — `FIN-SC-008`, `FIN-DES-037`..`040` |

Bounded context baru — sebelum amendment ini nol tabelnya ada di backend (`FIN-CAP-028`,
`FIN-CAP-029`). Kolom audit warisan `IdentityModel` tidak digambar.

## 1. Alur dokumen — PO sampai Purchasing Invoice

```mermaid
erDiagram
    MstSupplier {
        uuid Id PK
        varchar SupplierCode UK
        varchar SupplierName
        int PaymentTermDays
        int LeadTimeDays
        numeric TaxPercent
        boolean IsTaxable
        numeric CreditLimitAmount
    }
    FinPurchaseOrder {
        uuid Id PK
        varchar PONumber UK
        uuid SupplierId FK
        varchar Status "DRAFT..CLOSED, lihat data-dictionary"
        numeric TotalAmount
        varchar ApprovalTier "TIER_1, TIER_2"
        uuid RequestedByUserId
        timestamp RequestedAt
        uuid ApprovedByUserId "nullable"
        timestamp ApprovedAt "nullable"
        uuid RowVersion
    }
    FinPurchaseOrderItem {
        uuid Id PK
        uuid PurchaseOrderId FK
        varchar ProductCategory "teks bebas, bukan FK master"
        varchar ProductName
        varchar Unit
        numeric Quantity
        numeric UnitPrice
        numeric LineTotal
    }
    FinGoodsReceipt {
        uuid Id PK
        varchar GRNumber UK
        uuid PurchaseOrderId FK "wajib — GR tanpa PO sengaja tidak dibuat"
        date ReceivedDate
        varchar Status "RECEIVED, CANCELLED"
        uuid RowVersion
    }
    FinGoodsReceiptItem {
        uuid Id PK
        uuid GoodsReceiptId FK
        uuid PurchaseOrderItemId FK
        numeric ReceivedQuantity
        varchar Notes
    }
    FinInvoiceExchange {
        uuid Id PK
        varchar ExchangeNumber UK
        uuid SupplierId FK
        uuid PurchaseOrderId FK "nullable — boleh tanpa PO"
        uuid GoodsReceiptId FK "nullable"
        varchar SupplierInvoiceNumber
        date SupplierInvoiceDate
        date ReceivedDate
        date EstimatedDueDate "ReceivedDate + MstSupplier.PaymentTermDays"
        varchar Status "RECEIVED, LINKED_TO_INVOICE, CANCELLED"
        uuid RowVersion
    }
    FinPurchasingInvoice {
        uuid Id PK
        varchar InvoiceNumber UK
        uuid InvoiceExchangeId FK UK "1:1 dengan Tukar Faktur"
        uuid SupplierId FK
        numeric SubtotalAmount
        numeric DiscountAmount
        numeric PPNAmount "Pajak Masukan"
        numeric DownPaymentAmount
        numeric OtherDeductionAmount
        numeric TotalAmount
        varchar Status "DRAFT..CANCELLED"
        varchar ApprovalTier "TIER_1, TIER_2"
        uuid RequestedByUserId
        timestamp RequestedAt
        uuid ApprovedByUserId "nullable"
        timestamp ApprovedAt "nullable"
        uuid RowVersion
    }
    FinPurchasingInvoiceItem {
        uuid Id PK
        uuid PurchasingInvoiceId FK
        varchar ProductName
        numeric Quantity
        numeric UnitPrice
        numeric LineTotal
    }
    FinSupplierPayable {
        uuid Id PK
        uuid SupplierId FK
        uuid SourcePurchasingInvoiceId FK "nullable — kosong untuk baris manual lama"
        numeric OutstandingAmount
    }

    MstSupplier ||--o{ FinPurchaseOrder : "1:N"
    FinPurchaseOrder ||--o{ FinPurchaseOrderItem : "1:N"
    FinPurchaseOrder ||--o{ FinGoodsReceipt : "1:N"
    FinGoodsReceipt ||--o{ FinGoodsReceiptItem : "1:N"
    FinPurchaseOrderItem ||--o{ FinGoodsReceiptItem : "1:N"
    FinPurchaseOrder |o--o{ FinInvoiceExchange : "0..1:N — opsional"
    FinGoodsReceipt |o--o{ FinInvoiceExchange : "0..1:N — opsional"
    MstSupplier ||--o{ FinInvoiceExchange : "1:N"
    FinInvoiceExchange ||--|| FinPurchasingInvoice : "1:1"
    FinPurchasingInvoice ||--o{ FinPurchasingInvoiceItem : "1:N"
    FinPurchasingInvoice |o--o| FinSupplierPayable : "0..1:0..1 — dibuat saat Approved"
    MstSupplier ||--o{ FinPurchasingInvoice : "1:N"
```

## 2. Alur Retur Pembelian dan Deposit Retur

```mermaid
erDiagram
    FinPurchasingInvoice {
        uuid Id PK
        uuid SupplierId FK
    }
    FinSupplierReturn {
        uuid Id PK
        varchar ReturnNumber UK
        uuid PurchasingInvoiceId FK
        varchar Reason
        numeric TotalAmount
        varchar Status "DRAFT, CONFIRMED, CANCELLED"
        uuid RowVersion
    }
    FinSupplierReturnItem {
        uuid Id PK
        uuid SupplierReturnId FK
        varchar Description
        numeric Quantity
        numeric LineTotal
    }
    FinSupplierReturnDeposit {
        uuid Id PK
        uuid SupplierId FK
        uuid SourceReturnId FK UK "1 retur = 1 deposit"
        numeric OriginalAmount
        numeric AvailableAmount "tidak boleh negatif"
        varchar Status "AVAILABLE, EXHAUSTED, CANCELLED"
        uuid RowVersion
    }
    FinSupplierReturnDepositUsage {
        uuid Id PK
        uuid SupplierReturnDepositId FK
        uuid PaymentId FK "REVISI 5 — menggantikan PurchasingInvoiceId"
        numeric UsedAmount
        varchar Status "RESERVED, APPLIED, RELEASED"
        timestamp UsedAt
        timestamp ReleasedAt "nullable"
        uuid RowVersion
    }
    FinPayment {
        uuid Id PK
        numeric DepositAppliedAmount "REVISI 5"
        numeric NetTransferAmount
    }

    FinPurchasingInvoice ||--o{ FinSupplierReturn : "1:N"
    FinSupplierReturn ||--o{ FinSupplierReturnItem : "1:N"
    FinSupplierReturn ||--o| FinSupplierReturnDeposit : "1:1"
    FinSupplierReturnDeposit ||--o{ FinSupplierReturnDepositUsage : "1:N"
    FinPayment ||--o{ FinSupplierReturnDepositUsage : "1:N — sumber dana, REVISI 5"
```

**Diperbarui AMENDMENT REVISI 5 (25 September 2026).** Pemakaian deposit tidak lagi menunjuk
Purchasing Invoice, melainkan pembayaran (`FinPayment`) tempat deposit dipakai sebagai sumber
dana di samping transfer bank (`FIN-DEC-057`, `FIN-DES-045`). Deposit dicadangkan saat
ditambahkan ke pembayaran `DRAFT` dan dilepas bila pembayaran ditolak/dibatalkan
(`FIN-DES-046`). Rincian kolom: `data-dictionary.md` D.2.

## 3. Status entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstSupplier` | Existing | Administrator | Sudah punya `PaymentTermDays`/`LeadTimeDays`/`TaxPercent`/`IsTaxable`/`CreditLimitAmount` — nol kolom baru (`FIN-CAP-027`) |
| `FinPurchaseOrder`, `FinPurchaseOrderItem` | New | Finance | — |
| `FinGoodsReceipt`, `FinGoodsReceiptItem` | New | Finance | GR selalu terhadap satu PO (`DEV_DISCRETION`, lihat `02-backend-architecture.md` C.2 `FIN-DES-037`) |
| `FinInvoiceExchange` | New | Finance | Boleh tanpa PO/GR (`FIN-DEC-051`) |
| `FinPurchasingInvoice`, `FinPurchasingInvoiceItem` | New | Finance | Tepat satu per `FinInvoiceExchange` |
| `FinSupplierReturn`, `FinSupplierReturnItem` | New | Finance | — |
| `FinSupplierReturnDeposit`, `FinSupplierReturnDepositUsage` | New | Finance | Meniru pola `BilRefundableCredit` (Billing) secara struktural, arah aliran berlawanan — bukan tabel yang sama (`FIN-DEC-047`) |
| `FinSupplierPayable` | Extend | Finance | Tambah `SourcePurchasingInvoiceId` nullable; jalur manual tetap ada (`FIN-DES-040`) |

## 4. Cardinality dan invariant yang perlu diperhatikan

| Relasi | Aturan |
|---|---|
| `FinInvoiceExchange` → `FinPurchasingInvoice` | Tepat 1:1 — satu Tukar Faktur menghasilkan tepat satu Purchasing Invoice, dijaga `UNIQUE` pada `InvoiceExchangeId` (`FIN-DEC-051`) |
| `FinPurchaseOrder`/`FinPurchasingInvoice` → `ApprovalTier` | Dihitung `FinanceApprovalTierResolver` dari `TotalAmount`: `< Rp 50.000.000 → TIER_1`, `>= Rp 50.000.000 → TIER_2` (`FIN-DEC-052`) — **MUST NOT** dihitung ulang di frontend |
| `FinSupplierReturnDeposit.AvailableAmount` | Berkurang hanya lewat baris `FinSupplierReturnDepositUsage`, tidak pernah ditulis langsung — pola audit yang sama dengan `FinPaymentDeduction` |
| `FinSupplierPayable.SourcePurchasingInvoiceId` | `NULL` sah untuk baris lama/manual; terisi otomatis saat dibuat sistem dari Purchasing Invoice `Approved` |

## 5. Rujukan lintas berkas

| Kebutuhan | Berkas |
|---|---|
| Rincian kolom penuh dan DDL | `data-dictionary.md` AMENDMENT REVISI 4 bagian C.1-C.12, C.16 |
| Status transition PO/GR/Tukar Faktur/Purchasing Invoice/Retur | `contracts/state-transition-matrix.md` |
| Endpoint | `contracts/api-contract.md` |
| Kejadian akuntansi `PPN-MASUKAN-PEMBELIAN` | `accounting-integration.md`, `evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md` |
