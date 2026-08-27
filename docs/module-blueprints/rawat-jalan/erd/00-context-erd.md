# Context ERD — Rawat Jalan Billing

Diagram ini menunjukkan relasi logis antar bounded context. Ia bukan DDL dan bukan instruksi
migration.

```mermaid
erDiagram
    RegistrationEncounter ||--o| BillingFolio : "1:0..1 — Existing/Extend"
    BillingFolio ||--o{ BillingChargeLine : "1:N — New/Provisional"
    ClinicalFact ||--o{ BillingChargeLine : "1:N versioned — Adapter/View"
    BillingChargeLine ||--o{ BillingAllocation : "1:N — New"
    PayerDecision ||--o{ BillingAllocation : "0:N input — Adapter/View"
    BillingAllocation ||--o{ PaymentSettlement : "0:N — Cashier owner"
    BillingChargeLine ||--o{ FinancialAction : "0:N versioned — New"
    ClinicalFact ||--o{ ProcessingEffect : "1:N operation — New"
    ProcessingEffect ||--o{ ReconciliationCase : "0:N — New"
```

Patient, Encounter, clinical order, payer master, payment, dan accounting tetap dimiliki
context asalnya; Billing tidak membuat salinan owner.

