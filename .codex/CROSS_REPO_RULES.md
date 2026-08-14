# Cross-Repository Rules

`AGENTS.md` is authoritative. Task mode and explicit write targets determine repository access; a missing or ambiguous mode/write target defaults to **AUDIT MODE**.

| Mode | Frontend | Backend |
| --- | --- | --- |
| AUDIT | Read-only | Read-only |
| MODULE BLUEPRINT | Read-only evidence source | `docs/module-blueprints/**` only |
| FRONTEND | Write target | Strict read-only source of truth for contracts and business behavior |
| BACKEND | Read-only reference | Write target |
| CROSS-REPO | Only if explicitly named as a write target | Only if explicitly named as a write target |

- Inspect the backend before guessing a frontend API contract.
- Frontend code is a consumer reference and does not override backend business or security rules.
- Never silently modify the other repository after discovering a defect there; report it unless the active task explicitly authorizes that write target.
- Do not use a cross-repository task to broaden source, configuration, migration, Git, or deployment scope.
- Governance/documentation changes are permitted only when both the task mode and explicit write targets authorize them.
- In `MODULE BLUEPRINT MODE`, a blueprint may cite evidence as `repository/path#symbol@source-SHA`. Evidence from frontend never grants frontend write authority, and backend evidence never grants backend application-source write authority.
- A changed backend or frontend source SHA makes dependent blueprint evidence stale. Perform a scoped impact review before treating that evidence as current or reusing it for design, planning, or readiness.
