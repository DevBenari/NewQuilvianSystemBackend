# INS — Conditional Phase Roadmap

| Phase | Outcome | Dependencies | Status | Backend task IDs | Frontend task IDs | Acceptance / next action |
| --- | --- | --- | --- | --- | --- | --- |
| `INS-PH-001` | Owner-approved scope, lifecycle, actor, state, exception, and integration decisions. | `INS-BLK-001`, `INS-BLK-002` | `IN_PROGRESS` | None | None | No action is authorized after the G5-H pilot. If Insurance is explicitly activated as a production initiative, resume this phase and then use a business-module interview to close `INS-DEC-001`–`INS-DEC-006`. |
| `INS-PH-002` | Approved target contract for internal eligibility/coverage and operational lifecycle. | `INS-DEP-001`, `INS-DEP-002`, resolved `INS-DEP-003` | `BLOCKED` | Deferred | Deferred | Data ownership, state transitions, permissions, audit, snapshots, and acceptance matrix approved. |
| `INS-PH-003` | Provider integration/GL/claim architecture where explicitly in scope. | resolved `INS-DEP-004`, approved PH-002 contract | `BLOCKED` | Deferred | Deferred | External protocol, idempotency, security/privacy, retry, failure/reconciliation, and test environment approved. |
| `INS-PH-004` | Independently approved vertical delivery slices and readiness evidence. | Approved PH-002; PH-003 only if external scope selected | `BLOCKED` | Deferred | Deferred | Use delivery planning; no task is created from this draft roadmap. |

`INS-PH-001` is the only safe active phase if Insurance is explicitly activated as a production initiative. It is discovery/decision work, not application implementation; the G5-H pilot does not authorize it.
