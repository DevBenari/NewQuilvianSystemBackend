# ERD — Cashier Operations

```mermaid
erDiagram
  BilCashierShift { guid Id PK guid CashierId guid RegisterId decimal OpeningCash decimal SystemCash decimal PhysicalCash decimal Variance string Status datetime OpenedAt datetime ClosedAt binary RowVersion }
  BilCashVarianceReview { guid Id PK guid ShiftId FK guid ReviewerId decimal Variance string Resolution string Reason datetime ReviewedAt }
  BilCashierShift ||--o{ BilCashVarianceReview : reviewed_by
```

Handover dicatat sebagai audit transition pada shift asal dan shift penerima; implementer boleh menambah child `BilCashierShiftHandover` hanya bila acceptance membutuhkan tanda tangan dua pihak yang tidak cukup direpresentasikan audit fact.

