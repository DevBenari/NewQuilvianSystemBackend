# IGD — Acceptance Test Matrix

| Field | Value |
|---|---|
| Contract version/status | `0.1.0-draft` |
| Test ownership | Backend/API, Frontend, Integration, Security, Clinical/Finance/UAT owners |

| ID | Scenario and expected observable outcome | Decision / capability trace | Test level |
|---|---|---|---|
| AT-01 | Unknown Red patient creates exactly one provisional `Outpatient` encounter and one emergency visit with one idempotency key; no NIK/address/guarantor blocker. | `DEC-001`, `016`, `041`; CAP-02/17 | API + e2e |
| AT-02 | Repeated create and timeout retry replay one outcome; same key with altered intent yields `IdempotencyConflict`. | `DEC-025`; CAP-14 | API + fault injection |
| AT-03 | Completing administration keeps encounter/visit/clinical links; no second encounter is created. | `DEC-016`; CAP-02 | Integration |
| AT-04 | Ambiguous identity merge requires different checker; missing assignment leaves case pending; reversal is auditable. | `DEC-017`–`019`, `031`; CAP-03/13 | API + security |
| AT-05 | Retriage adds immutable history and backend SLA policy/version; Yellow/Green displays `TargetUnconfigured` without SOP target. | `DEC-004`, `035`; CAP-04 | API + frontend |
| AT-06 | Observation/resuscitation end before start is rejected; amendment preserves original. | `DEC-003`, `020`; CAP-05 | Domain/API |
| AT-07 | Only credentialed IGD doctor sets disposition; executed disposition alone cannot complete episode. | `DEC-005`, `020`; CAP-07/12 | Security/API |
| AT-08 | Sender cannot mark arrived; destination unit accepts/rejects/arrives and rejected transfer does not alter disposition automatically. | `DEC-020`, `034`; CAP-08 | API + e2e |
| AT-09 | Completion requires disposition/clinical/transfer gates but permits billing pending/outstanding with a billing handoff. | `DEC-021`, `042`; CAP-07/09 | API/UAT |
| AT-10 | Self-pay outstanding administrative release is denied while flag/policy is absent; physical departure is independently recorded. | `DEC-030`, `038`; CAP-09 | API/UAT |
| AT-11 | Material clinical encounter cancellation is denied even with approval; manual reopen uses maker-checker and scoped re-close. | `DEC-029`, `037`; CAP-15 | API/security |
| AT-12 | Repeated diagnostic result creates one follow-up; critical unreachable cannot close; late result does not reopen source episode. | `DEC-024`, `032`, `043`; CAP-11 | Adapter/integration |
| AT-13 | Head of IGD activation is pending; confirmation rejection leaves it active; only authorized incident commander/director deactivates. | `DEC-010`–`015`, `044`; CAP-19 | API/UAT |
| AT-14 | Broad technical role without contextual capability is denied; break-glass is time/target/reason-audited. | `DEC-026`, `034`, `045`; CAP-12 | Security |
| AT-15 | Missing diagnostic/billing Reliability Profile blocks activation; vendor timeout enters `OutcomeUnknown`, not blind retry. | `DEC-025`, `033`, `043`; CAP-14 | Integration fault injection |
| AT-16 | Frontend never claims downstream success for pending/unknown outcome and retains same idempotency key for user retry. | `DEC-025`; CAP-17/18 | Component/e2e |
| AT-17 | Shared Clinical and Pharmacy consumers use canonical encounter context without an IGD duplicate clinical/prescription record. | `DEC-003`, `040`; CAP-06/10 | Contract/integration |
| AT-18 | Audit and logs exclude full PHI/clinical payload while retaining actor/capability/version/reason/correlation evidence. | `DEC-006`, `026`; CAP-15 | Security/log inspection |
| AT-19 | `Emergency*Service` registrations resolve and relevant controller endpoints activate in deployed composition root. | CAP-16 | Runtime smoke test |
| AT-20 | Expand/migrate/cutover rollback preserves clinical history and prevents legacy generic write bypass. | Backend migration plan; CAP-01/04/07/08 | Migration/e2e |

Production UAT cannot pass until governance assignments, relevant SOP policies, named Diagnostic
Services contract, Finance gates, and runtime evidence are supplied. These tests are acceptance
criteria, not evidence that the current repositories already implement the target behaviour.
