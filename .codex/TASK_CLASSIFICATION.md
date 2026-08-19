# Task Classification

Classify before planning. Use the highest applicable factor; increase one level when two or more factors fall in the next level. If the task is uncertain, classify conservatively and inspect before lowering it.

| Level | Deterministic factors |
| --- | --- |
| LIGHT | One repository; usually 1–3 files inspected and 1–2 files modified; no material business logic, API, database, security/auth, or UI/workflow change. |
| MEDIUM | One or two repositories; commonly 4–10 files inspected or 3–6 files modified; bounded logic, UI/workflow, or non-breaking API consumer impact; no material database or security/auth redesign. |
| HEAVY | Multiple related modules or more than 10 files inspected / 6 files modified; substantial business logic; API contract change, database/schema consideration, security/auth impact, or broad workflow scope requiring coordinated review. |
| EPIC | Multiple independently deployable domains, an architecture-wide or multi-phase workflow, broad API/database/security redesign, or scope that cannot be safely reviewed and validated as one bounded change. |

## Deterministic scoring model

The score below determines the classification. Score every factor, add the total, then apply the classification bands.

| Factor | Score 0 | Score 1 | Score 2 |
| --- | --- | --- | --- |
| Repository scope | One repository | — | Two repositories |
| Files inspected | ≤ 8 | 9–20 | > 20 |
| Files modified | ≤ 3 | 4–8 | > 8 |
| Business logic | Simple | Moderate | Complex |
| API contract | None | Consumes existing contract | Changes contract |
| Database | None | Existing query/persistence behavior only | Schema/entity/migration impact |
| Security/Auth | None | Related but not core | Core authorization/authentication/security impact |
| UI/Workflow | Minor/local | Single page or bounded workflow | Multi-page or broad workflow |

| Total score | Classification |
| --- | --- |
| 0–3 | LIGHT |
| 4–8 | MEDIUM |
| 9–12 | HEAVY |
| 13+ | EPIC |

## Required factors

Assess repository count; files inspected; files modified; business-logic complexity; API-contract impact; database impact; security/auth impact; and UI/workflow scope. Existing `AGENTS.md` rules determine whether any factor is permitted.

## Module blueprint work

A pure `MODULE BLUEPRINT MODE` task may inspect both application repositories while writing only tracked blueprint documentation. Do not classify it as HEAVY solely because of that cross-repository inspection; assess the documentation scope, architecture/dependency complexity, and unresolved decision risk in addition to the normal factors. Application implementation scoring remains unchanged.

Classify blueprint work as HEAVY when it covers many modules or material dependencies, unresolved contracts, or high-risk security, financial, clinical, privacy, or regulatory decisions. Treat a broad architecture redesign or multi-module lifecycle change as EPIC: stop, decompose into bounded blueprint phases, and reclassify before writing.

## Execution rule

Any architecture-wide redesign, multi-domain implementation, or scope that cannot be safely reviewed and validated as one bounded change is EPIC regardless of score.

EPIC tasks are never directly implemented: `STOP → DECOMPOSE → reclassify each phase.` Decompose them into independently reviewable phases, then classify and execute each phase separately.

## Model guidance

- **GPT-5.6 Terra** is the default model.
- **GPT-5.6 Sol** is an escalation only for genuinely difficult HEAVY tasks after the task has been bounded.
