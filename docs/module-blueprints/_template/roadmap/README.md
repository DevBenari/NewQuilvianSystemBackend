# Conditional Roadmap and Phase Contract

Each phase uses `<PREFIX>-PH-###` and records outcome, dependency IDs, backend task IDs, frontend task IDs, acceptance criteria, status, evidence, blockers, and next action. Backend and frontend tasks use `<PREFIX>-BE-###` and `<PREFIX>-FE-###`.

Use phase statuses `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A later blocked phase does not block an independent `READY` phase. Use `<PREFIX>-REQ-###`, `<PREFIX>-RSK-###`, and `<PREFIX>-BLK-###` for requirements, risks, and blockers. IDs remain stable across revisions and numbers are never reused.
