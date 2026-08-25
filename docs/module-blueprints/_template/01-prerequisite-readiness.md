# <Module Name> — Prerequisite Readiness

Use one dependency record per material prerequisite. Capability status must use exactly one existing taxonomy value: `READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, or `UNKNOWN`.

| dependency_id | capability_or_module | dependency_type | owner | evidence | capability_status | required_by | blocking_impact | independent_continuation | source_sha | next_owner_or_action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `<PREFIX>-DEP-001` |  | `MODULE_FOUNDATION` |  | `repo/path#symbol@SHA` | `UNKNOWN` | `<PREFIX>-PH-001` |  |  |  |  |

Allowed `dependency_type` values are `MODULE_FOUNDATION`, `PHASE`, `INTEGRATION`, and `EXTERNAL`. A blocked dependency blocks only dependent phases; document phases that remain safe to continue.
