# IGD — Validation Matrix

| Field | Value |
|---|---|
| `contract_version` | `0.1.0-draft` |
| Owner | API and bounded-context owners |

| Command | Server validation | Rejection / safe outcome | Trace / test |
|---|---|---|---|
| Create emergency episode | `Outpatient`, IGD unit, arrival, one identity form, provisional minimum, idempotency key | Reject both/neither identity and duplicate active visit; replay same intent | `IGD-DEC-001`, `016`, `041`; `AT-01`, `02` |
| Complete registration | Same encounter/version; configured required fields | Remain incomplete; do not change clinical state | `DEC-016`; `AT-03` |
| Reconcile identity | Candidate search/evidence; strong-match or maker-checker | No auto merge; pending/rejected is retained | `DEC-017`–`019`, `031`; `AT-04` |
| Triage/retriage | Authorized clinician, valid colour, server effective time, prior sequence | No overwrite/delete; invalid colour/actor denied | `DEC-004`, `007`, `035`; `AT-05` |
| Observation/resuscitation | Visit context; start ≤ end; credential | Invalid chronology/privilege denied | `DEC-003`, `020`; `AT-06` |
| Disposition | Doctor privilege, in-service state, reason | Not completion; post-execution mutation becomes controlled correction | `DEC-005`, `020`; `AT-07` |
| Transfer action | Current state/version; actor source/destination unit | Reject invalid ordering and sender arrival | `DEC-020`, `034`; `AT-08` |
| Complete encounter | Executed disposition, required clinical/transfer gates | Return missing-gate codes; billing final is not universal gate | `DEC-021`, `042`; `AT-09` |
| Administrative release | Finance clearance reference and payer policy | Deny self-pay outstanding without enabled exception; physical departure stays separate | `DEC-022`, `030`, `038`; `AT-10` |
| Correction/cancel/reopen | Impact classification, SOD, reason/evidence, affected-domain approvals | Material clinical cancellation always denied; reopen scoped | `DEC-029`, `037`; `AT-11` |
| Late-result action | Stable source order/result ID, assigned review owner, critical policy version | Repeat is idempotent; no close for critical unreachable | `DEC-024`, `032`, `043`; `AT-12` |
| Incident action | Legal actor/state, reason/source, evidence reference, server time | Reject unauthorized deactivation; rejection does not deactivate | `DEC-010`–`015`, `044`; `AT-13` |
| Any privileged action | Capability, resource/unit, state, credential, assignment validity, SOD | Deny by default; backend result wins | `DEC-026`, `034`, `045`; `AT-14` |
| External integration dispatch | Approved profile, idempotency/correlation, safe retry classification | Missing profile blocks production; uncertain timeout is `OutcomeUnknown` | `DEC-025`, `033`, `043`; `AT-15` |
