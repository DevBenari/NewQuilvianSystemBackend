# ERD — Billing Intake, Piutang, dan Penerimaan

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Backend SHA | `09101d05` |

Kolom audit warisan `IdentityModel` tidak digambar. Penjelasannya ada di `data-dictionary.md`.

## 1. Billing Intake

```mermaid
erDiagram
    BilArHandoff {
        uuid Id PK
        uuid InvoiceId FK
        varchar DebtorType "PAYER, PATIENT_GUARANTOR, EMPLOYEE_BENEFIT"
        uuid DebtorReferenceId "boleh kosong"
        uuid BenefitOwnerId FK "BARU - pemilik manfaat karyawan"
        varchar BenefitRelationship "BARU - SELF, SPOUSE, CHILD"
        numeric Amount
        timestamp DueDate
        varchar Status "CREATED, ACKNOWLEDGED"
        uuid HandoffKey UK "kunci idempotensi milik Billing"
    }
    BilCollectionHandoff {
        uuid Id PK
        uuid TenderId UK "tender Billing yang berhasil"
        uuid SettlementId FK
        uuid InvoiceId FK
        numeric Amount
        varchar KwitansiNumber
        uuid CashierShiftId "wajib untuk tunai"
        varchar SourceInvoiceStatus "penentu tahan atau kirim"
        uuid HandoffKey UK
    }
    FinBillingHandoffIntake {
        uuid Id PK
        varchar HandoffType "AR, AP, COLLECTION, ADJUSTMENT"
        uuid SourceHandoffId
        uuid SourceHandoffKey UK "unik bersama HandoffType"
        varchar Status "NEW, CONSUMED, ACKNOWLEDGED, ERROR"
        uuid TargetEntityId "piutang atau penerimaan yang lahir"
        int RetryCount
        varchar ErrorMessage
        uuid CorrelationId
        uuid RowVersion
    }
    BilArHandoff ||--o| FinBillingHandoffIntake : "1:0..1 — Diperbarui / Baru"
    BilCollectionHandoff ||--o| FinBillingHandoffIntake : "1:0..1 — Baru, milik Billing"
```

## 2. Piutang

```mermaid
erDiagram
    FinReceivable {
        uuid Id PK
        varchar ReceivableNumber UK
        uuid SourceHandoffKey UK "idempotensi, dari BilArHandoff"
        uuid SourceHandoffId
        uuid InvoiceId FK
        varchar DebtorType "PAYER, PATIENT_GUARANTOR, EMPLOYEE_BENEFIT"
        uuid DebtorReferenceId
        uuid BenefitOwnerId "terisi hanya untuk EMPLOYEE_BENEFIT"
        varchar BenefitRelationship
        numeric OriginalAmount
        numeric OutstandingAmount "tidak boleh negatif"
        numeric AllocatedAmount
        numeric AdjustedAmount
        numeric WrittenOffAmount
        date DueDate
        varchar Status "OUTSTANDING, PARTIAL, SETTLED, WRITTEN_OFF, CANCELLED"
        varchar ClaimStatus "tidak mengubah nilai piutang"
        timestamp RecognizedAt
        uuid CorrelationId
        uuid RowVersion
    }
    FinReceivableItem {
        uuid Id PK
        uuid ReceivableId FK
        uuid PatientId "SENSITIF"
        uuid EncounterId
        uuid InvoiceId
        varchar Description
        numeric Amount
    }
    FinReceivableDocument {
        uuid Id PK
        uuid ReceivableId FK
        varchar DocumentType
        varchar DocumentNumber
        boolean IsReceived
        timestamp ReceivedAt
    }
    FinReceivableAdjustment {
        uuid Id PK
        varchar AdjustmentNumber UK
        uuid ReceivableId FK
        uuid SourceHandoffAdjustmentId "dari BilHandoffAdjustment"
        varchar Direction "DEBIT, CREDIT"
        numeric Amount
        varchar Status "REQUESTED, APPROVED, REJECTED"
        uuid RequestedBy
        uuid ApprovedBy "wajib beda dari RequestedBy"
        uuid RowVersion
    }
    FinReceivableWriteOff {
        uuid Id PK
        varchar WriteOffNumber UK
        uuid ReceivableId FK
        numeric Amount
        varchar Status "REQUESTED, APPROVED, REJECTED"
        uuid RequestedBy
        uuid ApprovedBy "wajib beda dari RequestedBy"
        uuid RowVersion
    }
    FinReceivable ||--o{ FinReceivableItem : "1:N — Baru"
    FinReceivable ||--o{ FinReceivableDocument : "1:N — Baru"
    FinReceivable ||--o{ FinReceivableAdjustment : "1:N — Baru"
    FinReceivable ||--o{ FinReceivableWriteOff : "1:N — Baru"
```

## 3. Penerimaan

```mermaid
erDiagram
    FinReceipt {
        uuid Id PK
        varchar ReceiptNumber UK
        varchar SourceType "BILLING_TENDER, AR_COLLECTION, MANUAL"
        uuid SourceTenderId UK "unik parsial, kosong untuk manual"
        uuid SourceCollectionHandoffId
        uuid SettlementId
        uuid InvoiceId
        uuid PaymentMethodId FK
        uuid PaymentMethodAccountId FK
        numeric Amount "disalin apa adanya dari BilTender"
        numeric AllocatedAmount
        numeric UnallocatedAmount
        varchar KwitansiNumber
        uuid CashierShiftId "wajib untuk tunai"
        varchar ProviderReference
        timestamp OccurredAt
        varchar SourceInvoiceStatus
        varchar Status "RECEIVED, ALLOCATED, RECONCILED, REVERSED"
        uuid ReversalOfReceiptId FK "menunjuk penerimaan yang dibalik"
        uuid RowVersion
    }
    FinReceiptAllocation {
        uuid Id PK
        uuid ReceiptId FK
        uuid ReceivableId FK "kosong bila bayar lunas tanpa piutang"
        uuid SourceAllocationId "dari BilPaymentAllocation"
        varchar TargetType "RECEIVABLE, INVOICE_DIRECT"
        numeric Amount
        boolean IsReversal
        uuid ReversalOfAllocationId FK
        uuid AllocatedBy
        timestamp AllocatedAt
    }
    FinReceivable {
        uuid Id PK
        numeric OutstandingAmount
        varchar Status
    }
    FinReceipt ||--o{ FinReceiptAllocation : "1:N — Baru"
    FinReceivable ||--o{ FinReceiptAllocation : "1:N — Baru, boleh kosong"
    FinReceipt |o--o| FinReceipt : "0:1 — Baru, pembalikan"
    FinReceiptAllocation |o--o| FinReceiptAllocation : "0:1 — Baru, pembalikan"
```

## 4. Status entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `BilArHandoff` | Diperbarui | Billing | Tambah `BenefitOwnerId` dan `BenefitRelationship`; migration milik tim Billing |
| `BilCollectionHandoff` | Baru | Billing | Bentuknya dikunci `integration-contract.md`; tabelnya dibangun tim Billing (`FIN-DES-023`) |
| `BilHandoffAdjustment` | Sudah ada | Billing | Direferensikan, **MUST NOT** disalin |
| `BilTender`, `BilSettlement`, `BilPaymentAllocation` | Sudah ada | Billing | Sumber nilai penerimaan; hanya dibaca |
| `FinBillingHandoffIntake` | Baru | Finance | Pintu masuk tunggal seluruh handoff |
| `FinReceivable` | Baru | Finance | Aggregate root piutang |
| `FinReceivableItem` | Baru | Finance | Menyimpan `PatientId` yang **sensitif** |
| `FinReceivableDocument` | Baru | Finance | Tidak pernah mengubah nilai piutang |
| `FinReceivableAdjustment` | Baru | Finance | Maker-checker |
| `FinReceivableWriteOff` | Baru | Finance | Maker-checker |
| `FinReceipt` | Baru | Finance | Aggregate root penerimaan |
| `FinReceiptAllocation` | Baru | Finance | Satu-satunya jembatan penerimaan ke piutang |

## 5. Tiga hal yang paling sering salah dibaca dari ERD ini

1. **`FinReceipt` tidak menunjuk `FinReceivable` secara langsung.** Hubungan keduanya selalu
   lewat `FinReceiptAllocation`. Ini disengaja: pasien yang membayar lunas di kasir
   menghasilkan penerimaan **tanpa piutang sama sekali** (`FIN-DES-011`).

2. **`FinReceiptAllocation.ReceivableId` boleh kosong.** Kolom kosong di sini bukan data yang
   belum diisi, melainkan keadaan sah — uangnya langsung melunasi tagihan Billing, bukan
   melunasi piutang Finance.

3. **Pembalikan tidak menghapus apa pun.** Baik `FinReceipt` maupun `FinReceiptAllocation`
   menunjuk dirinya sendiri lewat kolom `ReversalOf...`. Baris lama tetap utuh dan tetap
   terbaca; yang baru yang menetralkan (`FIN-DES-012`).
