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
