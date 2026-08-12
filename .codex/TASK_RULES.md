# Codex Task Rules

This document defines the repeatable operating lifecycle for implementation work. `AGENTS.md` remains the authoritative repository constitution; repository-specific safeguards always take precedence.

## Standard lifecycle

`CLASSIFY → INSPECT → PLAN → IMPLEMENT → VALIDATE → REVIEW → REPORT`

1. **CLASSIFY** the task using `TASK_CLASSIFICATION.md`; confirm task mode, write target, branch, and scope.
2. **INSPECT** the nearest comparable implementation and all directly affected contracts, authorization, workflow, persistence, and rules required by `AGENTS.md`.
3. **PLAN** a short, bounded implementation plan before writing.
4. **IMPLEMENT** only the authorized change, following the existing architecture and nearest pattern.
5. **VALIDATE** with commands proportionate to the change and the repository requirements; record actual command outcomes.
6. **REVIEW** the diff and completion criteria using `REVIEW_RULES.md`.
7. **REPORT** evidence, risks, migration state when relevant, and the next step using `REPORT_TEMPLATE.md` in ignored `.quilvian-local/` artifacts when required.

## Context efficiency

- Start with targeted files and the nearest existing implementation.
- Avoid repository-wide scans unless the task requires them to establish scope or safety.
- Do not reread unrelated modules after scope is understood.
- Avoid repeated validation unless the code or relevant configuration changed.
- Prefer existing controllers, DTOs, services, data access, validation, authorization, and workflow patterns over new patterns.
- Stop and report when required branch, write-target, security, database, or contract conditions are not satisfied.

## Boundaries

This workflow never authorizes Git publication, deployment, dependency upgrades, migration generation or execution, database operations, or changes outside the active task's explicit write scope.
