# IGD — API Contract

| Field | Value |
|---|---|
| `contract_version` | `0.1.0-draft` |
| Owner | Registration, Emergency Installation, Clinical, Finance, Diagnostic Services, Security, Integration |
| Compatibility | Proposed v2 write contract; existing generic v1 writes are migration-only, not target contract |
| Approval | `—` |

## Common command envelope

All state-changing endpoints require authenticated actor, `Idempotency-Key`, `Correlation-Id`, and
`If-Match`/expected aggregate version except initial creation. Responses include `commandId`,
`resourceId`, `resourceVersion`, `outcome`, `serverTime`, `correlationId`, and safe `reasonCode`.

| Outcome | HTTP intent | Meaning |
|---|---|---|
| `Succeeded` | 200/201 | Authoritative command completed. |
| `Accepted` / `Pending` | 202 | Local accepted; downstream status is not implied. |
| `OutcomeUnknown` | 202 | Remote outcome may exist; query/reconcile using same identifiers. |
| `Rejected` | 400/403/409/422 | Validation, permission, concurrency, or state guard failed. |
| `IdempotencyConflict` | 409 | Key was reused for different normalized intent. |

## Proposed operations

| Operation | Method / logical path | Owner | Essential request | Response / guard |
|---|---|---|---|---|
| Create episode | `POST /v2/health-services/emergency-installation-management/episodes` | Registration + Emergency | arrival, IGD service unit, `Outpatient`, known `PatientId` **or** provisional minimum identity, reason, arrival mode | Returns `EncounterId` + `EmergencyVisitId`; one local atomic outcome; validates exactly-one identity and no active duplicate. |
| Complete registration | `POST /v2/.../episodes/{encounterId}/registration-completion` | Registration | administrative data/version | Same encounter; no clinical-state transition. |
| Start service / triage / retriage | command endpoints beneath episode | Emergency / Clinical | current version, assessment/event, clinical actor context | Append-only. Red/provisional is not blocked by incomplete administration. |
| Record observation/resuscitation | command endpoints beneath visit | Emergency / Clinical | start/end/event and source references | End cannot precede start; correction is amendment. |
| Set or change disposition | `POST /v2/.../episodes/{id}/disposition-commands` | Emergency / Clinical | type/reason, expected state/version | Doctor privilege/context required; post-execution change invokes controlled correction. |
| Transfer command | `POST /v2/.../transfers/{id}/accept|reject|depart|arrive` | Emergency / receiving unit | reason where applicable, expected version | Backend validates sender/receiver and destination unit. |
| Request completion | `POST /v2/.../episodes/{id}/completion-commands` | Emergency | expected version | Evaluates clinical/transfer gates; returns missing gates without treating billing as clinical state. |
| Request administrative release | `POST /v2/.../encounters/{id}/administrative-release-commands` | Registration + Finance adapter | expected version, clearance reference | Release denied if Finance outcome not cleared/allowed; self-pay outstanding feature is disabled. |
| Identity reconciliation | `POST /v2/.../identity-reconciliation-cases` and approve/reject/reverse commands | Registration / Patient | provisional ID, candidates/evidence refs, expected version | Simple/ambiguous rules and maker-checker enforced. |
| Late-result action | `POST /v2/.../diagnostic-follow-ups/{id}/acknowledge|review|contact|close` | Clinical / Diagnostic adapter | review/action/contact metadata | Requires named diagnostic contract before production activation. |
| Incident operation | `POST /v2/.../incident-operations/{id}/activate|confirm|reject-confirmation|deactivate` | Emergency / Incident command | reason, scope/evidence ref, expected version | Legal actor/state guards; external sync disabled by default. |

## Read contract

`GET` episode reads return independent `clinicalState`, `administrativeState`, `billingHandoff`,
`financialClearance` reference, `transferState`, policy/version, allowed actions, etag/version,
and timeline entries. They do not return more PHI than the caller's capability/context permits.
Clinical facts, prescriptions, diagnostic payloads, and billing details are fetched from their owner
through separately authorized read contracts.

## Deprecation and errors

Deprecate direct generic update/delete/status endpoints after command consumers and migration
evidence exist. Every error uses a safe, localized `reasonCode`; implementation diagnostics and
PHI stay server-side. `409` state/version errors include current safe version and next permitted
actions, not hidden clinical data.
