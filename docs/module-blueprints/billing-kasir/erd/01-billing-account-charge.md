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
    guid TariffId FK "opsional, baru 2 Sep 2026 - katalog"
    decimal Quantity
    decimal UnitPrice
    string Status
  }
  MstTariff {
    guid Id PK
    string TariffName
    guid TariffCategoryId FK
    decimal NormalPrice
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
  MstTariff |o--o{ BilInvoiceItem : "0:N — Sudah ada (Health Services Master Data), FK baru opsional"
```

UK item adalah komposit `(SourceDomain, SourceDetailId)` untuk representasi aktif. Snapshot kalkulasi menyimpan breakdown patient/primary/excess pada dokumen terstruktur atau child rows saat implementasi; pilihan fisik final harus mempertahankan query dan auditability.

**Amendment 2 September 2026** (`BKC-DEC-059`–`062`): `BilInvoiceItem.TariffId` — kolom baru, `Guid?`, `DeleteBehavior.Restrict`, index non-unique (bukan bagian UK). Diisi hanya untuk item yang berasal dari entri katalog tarif (`SourceDomain="ADHOC_CATALOG"`); tetap `null` untuk item free-form (`SourceDomain="ADHOC"`) dan item asal klinis lain. `MstTariff` sudah ada (Health Services Master Data, `Areas/HealthServices/MasterData/Models/MstTariff.cs`) — direferensikan, tidak dibuat ulang. Detail desain lengkap di [`02-backend-architecture.md`](../02-backend-architecture.md#amendment-2-september-2026--entri-manual-berbasis-katalog-tarif--coverage-per-item).

