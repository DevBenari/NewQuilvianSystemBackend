# IGD — Target Data Dictionary

| Field | Value |
|---|---|
| Contract version/status | `0.1.0-draft` |
| Privacy | Minimum necessary; generic audit/log records exclude PHI and clinical payloads |

| Logical record | Required material attributes | Integrity / retention |
|---|---|---|
| `ProvisionalIdentity` | ID, alias/name if known, sex, estimated DOB/age if known, lifecycle state, created actor/time, resolution reference | Immutable lifecycle events; temporary record survives merge/resolution. No automatic merge. |
| `IdentityReconciliationCase` | Case ID, temporary ID, candidate/target patient IDs, match evidence references, maker/checker, decision/reason, case/version/state | Unique active case per temporary identity/action; maker ≠ checker for ambiguous/reverse; rejected case retained. |
| `IgdEpisodeStateLedger` | Ledger ID, resource IDs, command/idempotency/correlation IDs, prior/result states and versions, actor/capability, reason, server time, evidence reference | Append-only; unique idempotency key within command/resource scope; no PHI payload. |
| `IgdOutboxMessage` | Message ID, aggregate/version, schema, destination, safe payload reference, delivery status/attempts | Created atomically with business change; retain until policy-approved archival. |
| `IgdInboxReceipt` | Consumer/destination, message ID, processed time, outcome reference | Unique `(Consumer, MessageId)`; write with resulting business mutation. |
| `IgdReconciliationRecord` | Message/idempotency/correlation, source/destination, external reference, safe error, owner/queue, outcome/action | No full payload; requires authorized resolution/audit. |
| `DiagnosticFollowUpRef` | External order/result/source IDs, encounter ID, review owner/fallback, criticality rule/version, review/contact/escalation state/times | References Diagnostic Services source; repeated external result ID is idempotent. |
| `BillingHandoffRef` | Encounter ID, billing status/reason, owner queue, next action/SLA, billing reference, clearance outcome reference | Does not duplicate invoice/payment facts; financial exception remains disabled until gate approval. |
| `EmergencyIncidentOperation` | Operation ID, scope/location/type, state, activation source/actor/time/reason/evidence reference, policy version | One active scoped operation; deactivation is immutable event, not overwrite. |
| `GovernanceAssignmentRef` | Domain, assignee UserId, capability, scope, primary/delegate, validity, assigner/evidence reference | Owner is MMC governance; historical decision holds effective assignment reference. |

All material records use server timestamps, actor IDs, optimistic concurrency/version fields where
mutated, and restricted deletion. Exact column types, indexes, and retention duration require the
data/privacy owner and deployment evidence.
