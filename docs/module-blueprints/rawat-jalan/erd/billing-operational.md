# ERD Billing Operational — Rawat Jalan

DDL di bawah adalah dokumentasi bentuk berdasarkan model/configuration working tree, bukan
skrip yang boleh dijalankan. Migration belum ditemukan dan belum diberi otorisasi.

```mermaid
erDiagram
    BilFolio {
        uuid Id PK
        uuid EncounterId FK UK "unik selama IsDelete=false"
        int Status
        int Version
        boolean IsActive
    }
    BilChargeLine {
        uuid Id PK
        uuid FolioId FK
        varchar SourceContext
        uuid SourceAggregateId
        uuid SourceItemId
        uuid MilestoneFactId
        int MilestoneFactVersion
        varchar EffectType
        int CalculationStatus
        numeric GrossAmount
        numeric EligibleAmount
        int Version
    }
    BilChargeComponent {
        uuid Id PK
        uuid ChargeLineId FK
        varchar ComponentKey UK
        numeric Quantity
        varchar Unit
        jsonb TariffSnapshot
        jsonb RuleSnapshot
        jsonb RoundingSnapshot
        numeric CalculatedAmount
        int CalculationVersion
    }
    BilProcessingEffect {
        uuid Id PK
        varchar Consumer
        varchar OperationType
        varchar IdempotencyKey UK
        varchar RequestFingerprint
        varchar SourceContext
        uuid MilestoneFactId
        int MilestoneFactVersion
        varchar EffectType
        int Outcome
        uuid FolioId FK
        uuid ChargeLineId FK
    }
    BilFolio ||--o{ BilChargeLine : "1:N — provisional"
    BilChargeLine ||--o{ BilChargeComponent : "1:N — provisional"
    BilFolio ||--o{ BilProcessingEffect : "0:N — reference"
```

## Status entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `BilFolio` | Provisional working tree | Billing | Unique active encounter; `DeleteBehavior.Restrict` |
| `BilChargeLine` | Provisional working tree | Billing | Unique source/effect tuple; `DeleteBehavior.Restrict` |
| `BilChargeComponent` | Provisional working tree | Billing | Unique charge/component key |
| `BilProcessingEffect` | Provisional working tree | Billing Integration | Unique consumer/operation/idempotency key dan source/version/effect |

