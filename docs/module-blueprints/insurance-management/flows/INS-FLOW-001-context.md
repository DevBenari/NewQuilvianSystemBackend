# INS-FLOW-001 — Existing Context Flow

Status: `DRAFT`; source evidence only.

```mermaid
flowchart LR
  ADM[Administrator Master Data\nMstInsuranceProvider] --> PI[Patient Management\nMstPatientInsurance]
  PI --> REG[Registration Management\nTrxPatientEncounterGuarantor]
  ADM --> TAR[Health Services Master Data\nInsurance tariff and coverage rule]
  TAR --> COV[Clinical Management\nInsuranceCoverageService]
  REG --> COV
  COV --> CLIN[Clinical and Pharmacy consumers]
  OPS[Insurance operations lifecycle\neligibility / GL / claim]:::blocked
  EXT[External provider]:::blocked
  REG -. approved future contract required .-> OPS
  OPS -. approved external contract required .-> EXT
  classDef blocked fill:#fff3cd,stroke:#856404,color:#111;
```

The dotted paths are not existing integrations and must not be implemented without the blocked dependencies being resolved.
