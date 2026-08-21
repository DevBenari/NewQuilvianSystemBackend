# Integration Contract — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-INT-001@1.0.0` |
| Status | `draft` |
| Owner | Billing Integration + domain owners |
| External activation | `BLOCKED` oleh `RJ-BIL-DEP-009` |

## Internal clinical fact contract

Produsen: Clinical, Pharmacy, Laboratory, Radiology. Consumer: Billing Integration.

Minimum identity: `SourceContext`, `SourceAggregateId`, optional `SourceItemId`,
`MilestoneFactId`, `MilestoneFactVersion`, `EncounterId`, `EffectType`, `OccurredAt`,
`CorrelationId`, `CausationId`, dan `IdempotencyKey`.

Processing harus idempotent. Retry infrastructure memakai key/version yang sama. Correction
source memakai version baru. Timeout menjadi `OutcomeUnknown`; tidak boleh diasumsikan gagal atau
berhasil.

## Payer contract

Payer Management mengirim eligibility/authorization/claim/adjudication decision yang versioned.
Billing mengubahnya menjadi allocation. External rejection tidak menghapus charge. Manual
decision wajib diberi label `ManualOperator` dan menyertakan evidence, actor, reason, amount, dan
waktu.

## Cashier/Finance contract

Billing memberikan financial reference. Cashier mengirim payment/refund outcome. Finance mengirim
posting/reversal/accounting outcome. Tidak satu pun boleh mengubah clinical fact.

## External adapter contract

### Normalized adapter (Rencana, belum tersedia)

Adapter wajib menyatakan dukungan idempotency, status query, cancellation, amendment, partial
approval, claim submission, timeout, retry, dan reconciliation. Nama vendor, endpoint,
credential, certificate, payload, dan environment tidak boleh ditebak.

Production activation hanya setelah contract owner, security, sandbox/UAT, duplicate/status-query,
reconciliation, support escalation, dan cutover approval tersedia.

