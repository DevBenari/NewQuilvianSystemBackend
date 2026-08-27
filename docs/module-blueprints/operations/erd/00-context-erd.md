# Context ERD — Modul Operasi

```mermaid
erDiagram
 Patient ||--o{ OprCase : "Existing → New"
 TrxPatientEncounter ||--o{ OprCase : "Existing → New"
 OprCase ||--|{ TrxPatientProcedure : "reference 1:N"
 OprCase }o--o{ TrxPatientConsent : "reference 0:N"
 MstRoom ||--o{ OprSchedule : "Existing → New"
 Workforce ||--o{ OprTeamMember : "Existing → New"
 OprMaterialUsage }o--|| InventoryItem : "New → external owner"
 OprCase ||--o{ BillingCharge : "integration; Billing owner"
 OprCase ||--o{ DestinationEncounter : "handover; unit owner"
```

`Patient`, encounter, procedure, consent, room, workforce, item, charge, dan unit tujuan tetap dimiliki context masing-masing. Garis menunjukkan dependency, bukan pemindahan ownership.
