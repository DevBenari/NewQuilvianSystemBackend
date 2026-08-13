# IGD — State-Transition Matrix

| Field | Value |
|---|---|
| `contract_version` | `0.1.0-draft` |
| Owner | Federated per `IGD-DEC-042` |
| Approval | `—` |

| Aggregate | From → to | Command / authority | Mandatory guard | Illegal / exception path |
|---|---|---|---|---|
| Encounter admin | `Provisional → Complete` | Registration completion | Same `EncounterId`; configured required data | Does not start/stop clinical service. |
| Clinical episode | `Provisional|Registered → InService` | Authorized nurse/doctor | Server time, encounter/unit/credential context | Incomplete admin is not a blocker. |
| Triage | initial/retriage → new assessment | Authorized nurse/clinician | New sequence, prior reference if retriage, policy version | Update/delete effective record prohibited; amendment only. |
| Disposition | `Draft → Confirmed → Executed` | IGD doctor | Clinical privilege, reason, version | Execution does not equal completion; post-execution change is high-impact as applicable. |
| Transfer | `None → Requested → Accepted → Departed → Arrived` | Requester → receiving unit → sender → receiving unit | Separate source/destination contexts; timestamps ordered | `Requested → Rejected` requires reason and leaves disposition for new clinical decision. No sender self-arrival. |
| Clinical completion | `InService/Disposition → Completed` | Backend evaluation triggered by authorized actor | Executed disposition; clinical gates; transfer arrived if relevant; no active mandatory clinical workflow | Billing pending/outstanding alone is not a denial. Reopen is scoped maker-checker exception. |
| Administrative release | `NotReady|Waiting → Cleared → Released` | Registration based on Finance outcome | Finance clearance/approved exception reference | `PhysicalDeparture` is separate; current self-pay outstanding release is denied. |
| Identity | `Temporary → IdentityFound → ReconciliationPending → Merged|Resolved` | Registration/Patient workflow | Candidate search, evidence, SOD where ambiguous | Rejection remains pending; reverse is distinct high-impact workflow. |
| Diagnostic follow-up | `Final → ReviewPending → Reviewed → FollowUpClosed` | Review owner/fallback | Source result ID, ownership, action/audit | Late result never reopens episode. Critical path adds acknowledgement/contact/escalation. |
| Incident operation | `Inactive → ActivePending|ActiveConfirmed → Deactivated` | Head of IGD / incident commander / director as decided | Actor authority, reason/source, audit/evidence ref | Confirmation rejection marks active pending state; it does not deactivate. |
| High-impact change | `Draft → UnderReview → PartiallyApproved → Approved|Rejected → Effective` | Maker/checker/impacted domains | Different maker/checker, all mandatory domain approvals, effective assignment | Expiry is overdue/escalated, never auto-approved. |

Every command checks expected version and is idempotent. Any external acknowledgement is delivery
state, separate from these business states; uncertain delivery enters `OutcomeUnknown` then
reconciliation.
