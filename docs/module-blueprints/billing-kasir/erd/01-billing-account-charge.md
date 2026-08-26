# ERD — Billing Account & Charge

```mermaid
erDiagram
  BilInvoice {
    guid Id PK
    guid EncounterId UK
    string InvoiceNumber UK
    string Status
    int CurrentCalculationVersion
    binary RowVersion
  }
  BilInvoiceItem {
    guid Id PK
    guid InvoiceId FK
    string SourceDomain UK
    string SourceDetailId UK
    decimal Quantity
    decimal UnitPrice
    string Status
  }
  BilCalculationVersion {
    guid Id PK
    guid InvoiceId FK
    int VersionNo UK
    decimal GrossAmount
    decimal PatientAmount
    decimal GuarantorAmount
    bool IsLocked
  }
  BilDiscountApplication {
    guid Id PK
    guid InvoiceId FK
    guid DiscountPolicyId FK
    decimal Amount
    string ApprovalStatus
  }
  BilInvoice ||--o{ BilInvoiceItem : contains
  BilInvoice ||--o{ BilCalculationVersion : calculates
  BilInvoice ||--o{ BilDiscountApplication : applies
```

UK item adalah komposit `(SourceDomain, SourceDetailId)` untuk representasi aktif. Snapshot kalkulasi menyimpan breakdown patient/primary/excess pada dokumen terstruktur atau child rows saat implementasi; pilihan fisik final harus mempertahankan query dan auditability.

