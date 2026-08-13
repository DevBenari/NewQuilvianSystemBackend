# IGD — Frontend Architecture

| Field | Value |
|---|---|
| Contract version/status | `0.1.0-draft` |
| Input | Blueprint `IGD-BP-001` revision `3` |
| Owner | Frontend authority; Product/Clinical/Security constraints are mandatory |
| Approval | `—` |
| Traceability | `IGD-DEC-016`, `020`–`021`, `026`–`027`, `032`–`035`, `040`–`045`; `CAP-17`, `18` |

## Functional workspaces, not visual prescriptions

This design specifies behaviour, not sidebar placement, final routes, tabs, drawers, colours, or
component library. Those remain `DEV_DISCRETION` only after privacy, clinical, and product
constraints are approved (`IGD-UI-004`). The frontend must expose these functional capabilities:

| Functional area | User outcome | Backend source of truth |
|---|---|---|
| Emergency episode intake | Start normal or provisional emergency episode without waiting for non-critical administration | `CreateEmergencyEpisode` command and its replayable outcome |
| Clinical episode work | Record triage/retriage, observation/resuscitation/procedural detail, disposition and transfer commands | Command-specific IGD APIs plus Clinical Management references |
| Registration completion and identity | Complete administration, flag candidate identity, submit reconciliation request/approval where authorized | Registration/Patient Management workflow; never direct master-patient edit |
| Operational closure | Show independent clinical, transfer, billing-handoff, financial-clearance and administrative-release outcomes | Domain-specific read model; frontend does not infer completion |
| Late-result follow-up | Present assigned review task, acknowledgement, contact attempts, escalation and final action | Diagnostic adapter/read model; never display unapproved diagnostic facts as source truth |
| Disaster operation | Request activation/confirmation/rejection/deactivation only for actions returned as allowed | IGD incident-operation command/read model |

## Client/API contract and state handling

- Replace the existing two-call patient-encounter then emergency-visit sequence with one
  idempotent episode command. The client generates and retains one `Idempotency-Key` per user
  intent until it obtains a final/replayable response; retry does not create a new key.
- `EncounterType.Outpatient` is sent only through the target contract. Frontend-owned numeric
  enum mappings for emergency type/status and service-unit string heuristics must be removed from
  the authoritative write path (`CAP-17`).
- Each command result is explicitly rendered as `Succeeded`, `Pending`, `Rejected`,
  `OutcomeUnknown`, or `NeedsAttention`. A local accepted response is not displayed as downstream
  success when an acknowledgement is required.
- Cache keys include resource ID and read-model version/etag. Mutation success invalidates the
  episode, transfer, triage timeline, closure, and task keys affected by the command. A 409
  version conflict refreshes the authoritative record and preserves unsent user input for a
  deliberate retry.
- The UI gets permitted actions and reason codes from the backend. Hidden/disabled controls are
  usability only; authorization is never trusted from the client.

## Validation and safety behaviour

| Situation | Frontend behaviour |
|---|---|
| Provisional patient | Collect only the minimum fields required by the command; mark missing administration as pending and never block Red care for NIK/address/phone/guarantor. |
| Identity conflict | Present candidate/evidence references and status, not automatic merge. Do not offer approval to the maker. |
| Triage and SLA | Render backend category, server clock, policy version, warning/breach and `TargetUnconfigured`. Red is visually and interactionally urgent without relying on a client timer as authority. |
| Offline/timeout | Preserve draft locally only if encrypted/session-safe project convention allows it; show whether command is unsubmitted, pending, or `OutcomeUnknown`. Do not silently replay clinical commands after a fresh session. |
| Duplicate submit | Disable re-entry while a key is active; repeated intent queries/replays the same command outcome. |
| Transfer/closure | Show the independent state labels and missing gates. Do not present disposition, billing handoff, physical departure, or transfer acceptance as clinical completion. |
| Late result | Use minimum necessary display; acknowledgement/contact action is auditable and must not expose result payload in notifications or browser diagnostics. |

## Privacy, accessibility, and responsive constraints

- Fetch minimum necessary fields for the displayed task. Mask identifiers according to the future
  approved privacy policy; no PHI in URL, analytics, browser logs, unredacted error toast, or
  client telemetry.
- Recheck session/capability after refresh, token change, and privileged action; break-glass needs
  reason, target scope, time-bound indication, and post-event-review state from the server.
- Meet project accessibility conventions: keyboard-operable commands, programmatic labels,
  error summaries tied to fields, non-colour-only clinical status, announced urgent state changes,
  and usable reflow at supported device widths.
- Any local draft, printed/exported output, and cached record follows approved retention and
  disclosure controls. Until those controls are approved, use no offline PHI persistence by
  default.

## Frontend test obligations

Unit/component tests cover validation, error mapping, action visibility, masking, stale data, and
idempotency-key retention. Integration/e2e tests cover `AT-01`, `AT-02`, `AT-05`, `AT-08`,
`AT-12`, `AT-16`, and `AT-19` in the acceptance matrix. Final information architecture and visual
components are intentionally outside this blueprint.
