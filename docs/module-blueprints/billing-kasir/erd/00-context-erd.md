# Billing dan Kasir — Context ERD

Revision `0.4`, status **approved**. Garis menunjukkan referensi lintas context, bukan satu transaksi database raksasa.

```mermaid
erDiagram
  BILLING_ACCOUNT ||--o{ PATIENT_FUNDS : "settled by"
  BILLING_ACCOUNT ||--o{ FINANCIAL_EXCEPTION : "corrected by"
  CASHIER_OPERATIONS ||--o{ PATIENT_FUNDS : "receives tender"
  BILLING_ACCOUNT ||--|| FINALIZATION_HANDOFF : "finalizes"
  FINANCIAL_EXCEPTION ||--o{ FINALIZATION_HANDOFF : "adjusts"
```

| Context | ERD | Owner |
| --- | --- | --- |
| Billing Account & Charge | [01](./01-billing-account-charge.md) | Billing |
| Patient Funds & Settlement | [02](./02-patient-funds-settlement.md) | Treasury/Billing |
| Financial Exception & Adjustment | [03](./03-financial-exception-adjustment.md) | Finance |
| Cashier Operations | [04](./04-cashier-operations.md) | Cashier Operations |
| Finalization & Handoff | [05](./05-finalization-handoff.md) | Billing/Finance Integration |

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

Tidak ada bounded context baru. Amendment ini menambah **dua arah bacaan** dari Billing ke konteks yang sudah ada, keduanya panggilan langsung dalam proses yang sama (satu assembly), tanpa penulisan dan tanpa pesan:

```mermaid
erDiagram
    TrxPatientEncounterGuarantor {
        uuid Id PK
        uuid EncounterId FK "milik Registration"
        int PaymentType "enum: Cash / Insurance / CompanyGuarantor"
        uuid InsuranceProviderId FK "wajib untuk Insurance"
        varchar PolicyNumberSnapshot "snapshot registrasi"
        varchar MemberNumberSnapshot "snapshot registrasi"
        varchar PlanNameSnapshot "snapshot registrasi"
        boolean IsPolicyActive
    }
    MstInsuranceProvider {
        uuid Id PK
        varchar InsuranceProviderCode UK
        varchar InsuranceProviderName
        varchar ContractNumber
        varchar OfficeAddress
        boolean IsActive
    }
    BilInvoice {
        uuid Id PK
        uuid EncounterId FK "milik Registration"
        varchar InvoiceNumber UK "dipakai sebagai nomor dokumen"
        varchar Status "OPEN / FINAL / CLOSED / SETTLED_BY_WRITE_OFF"
        int CurrentCalculationVersion
    }
    BilCalculationVersion {
        uuid Id PK
        uuid InvoiceId FK
        int VersionNo "unik bersama InvoiceId"
        numeric PrimaryAmount "total tanggungan penjamin"
        text BreakdownSnapshot "JSON, memuat alokasi per baris sejak BIL-CALCULATION-0.5"
    }
    MstInsuranceProvider ||--o{ TrxPatientEncounterGuarantor : "1:N — Sudah ada, milik Registration/Master Data"
    BilInvoice ||--o{ BilCalculationVersion : "1:N — Sudah ada, milik Billing"
```

Arah ketergantungannya satu arah: **Billing membaca** Registration dan Administrator/Master Data. Tidak ada konteks lain yang membaca tabel `Bil*` karena amendment ini, dan tidak ada tabel `Bil*` yang menyalin kolom milik konteks lain (`BIL-INT-011`, `BIL-INT-012` pada [`../contracts/integration-contract.md`](../contracts/integration-contract.md)).

Kolom audit warisan `IdentityModel` tidak digambar pada diagram di atas.
