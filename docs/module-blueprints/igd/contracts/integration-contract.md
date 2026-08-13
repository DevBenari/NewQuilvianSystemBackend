# IGD — Integration Contract

| Field | Value |
|---|---|
| `contract_version` | `0.1.0-draft` |
| Owner | Integration plus affected data owner |
| Approval | Technical Integration **and** affected Clinical/Finance/Privacy/Security authority |

## Universal envelope and delivery rule

Each command/event carries `MessageId`, `IdempotencyKey`, `CorrelationId`, `CausationId` when
applicable, source/destination, schema version, aggregate/resource ID, `EncounterId` where
relevant, and safe timestamps. Payloads contain minimum necessary data. Delivery is at-least-once
with idempotent receiver side effects; it is not claimed as exactly-once.

| Integration | Truth owner / target role | Pattern | Production activation gate |
|---|---|---|---|
| Registration ↔ Emergency | Registration owns encounter; Emergency owns visit extension | Local atomic episode creation in current monolith; outbox if boundary separates | Unique active encounter visit, idempotency, migration and controller/DI proof |
| Emergency ↔ Clinical / Pharmacy | Clinical/Pharmacy own facts | Context adapter using canonical encounter, versioned request/event | Clinical/Registration/Pharmacy approval of provisional context (`DEC-040`) |
| Emergency ↔ Finance | Finance owns charge/clearance/release outcome | Billing handoff/reference; async delivery/reconciliation | Named transactional Finance contract; current self-pay outstanding release flag false |
| Diagnostic Services ↔ IGD | Diagnostic Services owns order/result | Stable external order/result IDs feed IGD review-task/reference adapter | Named system/owner, semantic/critical rules, approved Reliability Profile (`DEC-043`) |
| Emergency ↔ incident/disaster | IGD owns temporary operational state | No automatic synchronisation; future adapter only | External incident owner/contract/approver/reliability evidence (`DEC-044`) |

## Reliability profile

| Rule | Draft baseline from `IGD-DEC-033` |
|---|---|
| Internal synchronous call | 10-second timeout; at most 2 safe/idempotent interactive attempts |
| External/vendor call | 30-second timeout |
| Retry | Transient-only, bounded exponential backoff with jitter, same idempotency key |
| State-changing vendor timeout | Retry only when native idempotency/status-query is proven; otherwise `OutcomeUnknown` + reconciliation |
| Failure terminal | Dead-letter/`NeedsReconciliation`, owner queue, age/attempt metrics and audited reprocess |
| Compensation | Domain command such as cancel/correct; no destructive cross-module rollback |

The vendor matrix must record native idempotency, status-query, correlation echo, async ack,
retry-after, deduplication, cancellation/reversal, webhook callback, PHI classification, and
retention. Unknown means unsupported and prevents production activation.
