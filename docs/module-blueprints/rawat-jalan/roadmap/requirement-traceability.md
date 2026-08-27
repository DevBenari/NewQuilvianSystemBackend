# Requirement Traceability — Rawat Jalan Billing Roadmap

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Roadmap revision | `1` |
| Status | `APPROVED_FOR_EXECUTION` |
| Approval | `OWNER_APPROVED` untuk planning dan seluruh task roadmap pada `2026-08-21`; handoff/writer authority tetap wajib |
| Contract | `RJ-BIL-CONTRACT-001@1.0.0` |
| External adapter | `RJ-BIL-DEP-009` inactive/out of scope |
| IMPLEMENTATION_AUTHORITY | `NOT_GRANTED` |
| BUILDER_EXECUTION | `NOT_AUTHORIZED` |
| Status seluruh task roadmap revision 1 | `APPROVED_FOR_EXECUTION` |

| Requirement/decision | Desain/contract | Backend task | Frontend task | Acceptance evidence | Status |
|---|---|---|---|---|---|
| `RJ-BIL-GATE-DEC-001` ownership financial | Domain architecture §3-5; API/permission contracts | `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-006` | `RJ-BIL-FE-001`, `RJ-BIL-FE-002`, `RJ-BIL-FE-004` | No clinical endpoint authoritative financial; source-of-truth test | Approved for execution |
| `RJ-BIL-GATE-DEC-002` multi-payer allocation | Allocation aggregate; API/validation | `RJ-BIL-BE-005` | `RJ-BIL-FE-003` | Allocation equation and version history test | Approved for execution |
| `RJ-BIL-GATE-DEC-003` Laboratory milestone | Lab boundary/lifecycle | `RJ-BIL-BE-003` | `RJ-BIL-FE-002` | Accepted eligibility; specimen history test | Approved for execution |
| `RJ-BIL-GATE-DEC-004` Radiology safety/acquisition | Radiology boundary/lifecycle | `RJ-BIL-BE-004` | `RJ-BIL-FE-002` | Safety gate and performed acquisition test | Approved for execution |
| `RJ-BIL-GATE-DEC-005` actual consumption | Charge component/rule boundary | `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-004` | `RJ-BIL-FE-001`, `RJ-BIL-FE-002` | Rule missing → review; quantity calculation test | Approved for execution |
| `RJ-BIL-GATE-DEC-006` financial governance | Financial Action/Approval | `RJ-BIL-BE-006` | `RJ-BIL-FE-004` | Maker-checker/self-approval/close gate test | Approved for execution |
| `RJ-BIL-GATE-DEC-007` Pharmacy ownership | Projection/clinical fact boundary | `RJ-BIL-BE-002` | `RJ-BIL-FE-002` | Paid != Dispensed; read-only projection test | Approved for execution |
| `RJ-BIL-GATE-DEC-008` reliability/reconciliation | Processing/Reconciliation | `RJ-BIL-BE-001`, `RJ-BIL-BE-007`, `RJ-BIL-BE-009` | `RJ-BIL-FE-001`, `RJ-BIL-FE-005`, `RJ-BIL-FE-007` | Replay, timeout, partial, recovery report | Approved for execution |
| `RJ-BIL-GATE-DEC-009` payer/manual release scope | Manual claim/integration contract | `RJ-BIL-BE-008` | `RJ-BIL-FE-006` | Manual label, adapter inactive, payment separation | Approved for execution |

## Coverage gap

| Gap | Dampak | Owner/tindakan |
|---|---|---|
| Tidak ada test project/evidence pada snapshot audited | Semua task harus membuat evidence test sebagai DoD | QA + builder |
| Threshold approval, tariff rule, SOP safety belum bernilai final | High-risk fail-closed; partial charge review; safety config gate | Finance/Clinical Governance |
| Working tree Billing Operational belum committed | Evidence provisional; builder wajib preflight ulang | Backend owner |
| External adapter contract/UAT belum tersedia | Adapter tetap inactive; manual flow saja | Payer/Integration |
| UI visual authority belum dikunci | Detail visual tetap `DEV_DISCRETION` | Frontend authority |
