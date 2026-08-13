# IGD — Backend Architecture

| Field | Value |
|---|---|
| Contract version/status | `0.1.0-draft` |
| Input | Blueprint `IGD-BP-001` revision `3` |
| Owner | Product/Domain Owner with bounded-context owners below |
| Approval | `—`; mandatory domain approval per `IGD-DEC-028` and `IGD-DEC-045` |
| Traceability | `IGD-DEC-001`–`045`; `CAP-01`–`19` |

## Boundary and ownership

| Bounded context | Target responsibility | Existing reuse / target change | Accountable owner |
|---|---|---|---|
| Registration & identity | One canonical `EncounterId`, provisional representation, registration completion, temporary-identity reconciliation orchestration | Extend `TrxPatientEncounter` and reuse `MstPatient`; replace generic merge/update as the reconciliation workflow. No `PatientIGD` master. | Registration Management; Patient Management approves master-patient boundary (`IGD-DEC-039`) |
| Emergency episode | IGD visit, triage/retriage, observation, resuscitation, IGD procedure detail, disposition, transfer, episode closure | Extend existing `TrxEmergencyVisit` aggregate and children. | Emergency Installation (`IGD-DEC-042`) |
| Shared clinical context | Vital signs, CPPT, assessments, diagnosis, procedures, clinical orders | Reference Clinical Management facts through canonical `EncounterId`; add a provisional-context adapter rather than clone clinical tables. | Clinical Management (`IGD-DEC-040`) |
| Pharmacy | Prescription/dispensing against the same approved clinical context | Reuse existing prescription contract through adapter; no IGD prescription store. | Pharmacy Management (`IGD-DEC-040`) |
| Financial release | Billing handoff and administrative-release decision only | Reference Billing facts. Do not create financial facts in IGD; initial self-pay outstanding release is disabled. | Finance/Billing (`IGD-DEC-021`, `038`, `042`) |
| Diagnostics follow-up | Result references, review task, acknowledgement/handover reference | Adapter only; Diagnostic Services remains source of truth for order/result. | Named Diagnostic Services owner required (`IGD-DEC-043`) |
| Incident operation | Local mass-casualty activation and confirmed operation state | New IGD operational aggregate, no automatic enterprise incident synchronisation. | Emergency Installation (`IGD-DEC-044`) |
| Platform controls | Contextual authorization, approval adapter, immutable audit, outbox/inbox, reconciliation | Extend Security/HR Workflow/logging only after adapter checks; no broad `SuperAdmin` bypass for business commands. | Security, Privacy, Integration |

## Aggregate and transaction design

| Aggregate | Root and local transaction | Invariants / rollback boundary |
|---|---|---|
| Emergency episode | `TrxPatientEncounter` + exactly one active `TrxEmergencyVisit`; at the current shared `ApplicationDbContext`, `CreateEmergencyEpisode` commits both atomically in a Registration-owned application transaction. | A request has one idempotency key, one encounter, and at most one active visit. Failure rolls back both local writes. If contexts split into databases later, use outbox/reconciliation rather than distributed ACID. |
| Triage | `TrxEmergencyTriage` sequence on an emergency visit | Append-only assessment/retriage; no update/delete of effective clinical facts. A correction is an amendment linked to the original. |
| Disposition and transfer | Existing disposition and transfer records, each with command history | Disposition is not completion; transfer acceptance/departure/arrival have separate authorities. `Arrived` is required before relevant completion. |
| Identity reconciliation | New controlled reconciliation case referencing `TemporaryPatientId`, candidate/target `PatientId`, and encounter | Same request replays outcome; ambiguous/reverse operations require different maker/checker. A failed/rejected case remains historical and does not change `EncounterId`. |
| Incident operation | New `EmergencyIncidentOperation` root plus immutable activation events | One currently active operation per scoped incident/location. Rejection of confirmation does not deactivate it. External sync is disabled by default. |
| High-impact governance | Adapter around workflow assignment plus IGD approval request/evidence reference | Maker and checker are different users. Approval expiry escalates; it does not approve automatically. |

## Target entity changes

See [ERD context](erd/00-context-erd.md) and [data dictionary](erd/data-dictionary.md). The physical
table/column names are deliberately not final until each data owner approves, but the following
logical changes are mandatory:

- Extend the Registration encounter representation so it can reference **either** a definitive
  patient or a controlled provisional identity without creating a second encounter (`CAP-02`,
  `IGD-DEC-016`, `039`). Enforce exactly one active identity reference and retain the historical
  provisional-to-definitive linkage.
- Retain existing IGD entities as `Extend`; replace generic status/update/delete mutation with
  command handlers and append-only amendments (`CAP-01`, `04`, `05`, `07`, `08`).
- Add an IGD-owned state ledger/audit reference for episode, transfer, and incident commands. It
  stores identifiers, state/version, actor/capability, reason, timestamp, correlation and evidence
  reference—never a clinical payload.
- Add transactional outbox/inbox and reconciliation records as platform entities. They are not
  diagnostic or billing facts (`CAP-14`).
- Financial clearance, invoices, diagnostic results, master patients, prescriptions, and shared
  clinical records remain references/adapters, not new IGD tables (`CAP-06`, `09`, `10`, `11`).

## Application and persistence responsibilities

| Layer | Responsibility |
|---|---|
| API | Authenticate, parse command/version/idempotency headers, return command outcome. No state transition logic in controller. |
| Application command handler | Load aggregate with concurrency token; validate capability/context/state; execute local transaction, append audit/outbox, return replayable result. |
| Domain policy | Enforce state guards, closure gates, identity/financial/clinical separation, and classify high-impact operations. It accepts policy versions/configuration as input. |
| Persistence | Optimistic concurrency, unique keys, restricted foreign keys, soft retention where clinically required, append-only amendments/ledgers. |
| Integration adapter | Translate versioned commands/events, use inbox/outbox, maintain delivery state, and create reconciliation—not clinical compensation by deletion. |

## State and closure enforcement

The canonical matrix is [state-transition-matrix.md](contracts/state-transition-matrix.md).

- `EncounterType.Outpatient` is canonical for the IGD episode. The current frontend emergency
  enum is a known provider/consumer conflict, not a target variant (`IGD-DEC-041`).
- Clinical completion requires a final executed disposition, required clinical gates, and transfer
  `Arrived` where applicable. It does not require billing `Final`.
- Administrative release has a separate Registration-owned state and consults Finance's clearance
  outcome. Initial self-pay outstanding release is denied; `PhysicalDeparture` is a factual event,
  not a release bypass.
- Generic PUT/DELETE endpoints must be deprecated behind command-specific replacements. Existing
  data is never hard-deleted in the target correction/cancellation/reopen paths.

## Reliability, privacy, and observability

- All mutating commands require `Idempotency-Key`, `Correlation-Id`, expected aggregate version,
  and an authenticated actor. Same key + same normalized request replays; same key + different
  request returns `IdempotencyConflict`.
- Outbox writes occur in the same local transaction as their source aggregate. Receivers process
  at least once with a unique processed-message record. A possibly delivered timeout becomes
  `OutcomeUnknown`, then reconciliation; it is never blindly resent with a fresh key.
- Internal synchronous calls use the `IGD-DEC-033` draft baseline (10 seconds, at most two safe
  attempts); external/vendor calls use 30 seconds. A missing approved profile blocks production
  activation.
- Audit/logs retain minimum necessary identifiers and metadata. Diagnostic payload, full clinical
  text, PHI, secrets, and evidence files stay out of generic logs. Evidence content is stored in
  access-controlled owning storage.
- Metrics: command success/replay/conflict, authorization denial, transition rejection,
  outbox age, inbox duplicate, delivery/reconciliation age, dependency availability, SLA
  warning/breach, and privileged-action review backlog.

## Migration, deployment, and rollback

1. **Expand:** add nullable/compatible references, state ledgers, outbox/inbox, policy/configuration
   tables, indices, and new command endpoints while preserving current read paths.
2. **Verify runtime:** prove DI registration and controller activation for every `Emergency*Service`
   in an environment before enabling any new workflow (`CAP-16`).
3. **Migrate/backfill:** create immutable baseline records only from evidenced historical state;
   unresolvable legacy states are marked for reconciliation, not guessed.
4. **Cut over:** route new clients through the versioned episode command; keep legacy endpoints
   read-compatible during migration and reject unsafe generic clinical mutations.
5. **Contract:** remove legacy write paths only after data, consumer, and rollback evidence exists.
   Rollback disables new command/UI feature flags and leaves committed clinical/audit records intact;
   it never restores data by deleting history.

## Backend test obligations

The executable scenarios and owners are in
[acceptance-test-matrix.md](testing/acceptance-test-matrix.md). Minimum gates include `AT-01` through
`AT-20`, migration compatibility, DI activation, authorization/SOD, idempotency, and fault injection.
