# <Module Name> — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `<PREFIX>-BP-001` |
| Module name | `<Module Name>` |
| Revision | `<n>` |
| Module status | `DRAFT` |
| Current phase | `<PREFIX>-PH-001` |
| Last verified at | `<ISO-8601 or not yet verified>` |
| Backend source SHA | `<SHA or not yet captured>` |
| Frontend source SHA | `<SHA or not yet captured>` |

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| None | None | None |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |

## Next recommended task

`<task ID or concrete discovery/approval action>`

## Optional deterministic delivery progress

Show a percentage only as `DONE approved tasks / total approved tasks`, with its denominator and excluded blocked/unapproved tasks. Never use a manual estimate as readiness.

## Status contract

`DRAFT` has identity but incomplete intake. `DISCOVERY` is collecting decisions/evidence. `READY` means planned phases may start. `PARTIAL` means at least one phase is ready while another is blocked or unknown. `BLOCKED` means no material phase can safely proceed. `IN_PROGRESS` has authorized active work. `VERIFYING` awaits readiness evidence. `DONE` requires appropriate verification evidence. `SUPERSEDED` records the successor blueprint.

Phase statuses are `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A phase becomes `DONE` only when its acceptance/readiness evidence is recorded; file existence is insufficient.
