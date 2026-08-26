# Billing dan Kasir — Integration Contract

`contract_version: BIL-INTEGRATION-0.4` · status **approved** · owner masing-masing producer + Billing + AR/AP · approved 20 Agustus 2026. Transport final boleh in-process/outbox/message, tetapi semantics berikut terkunci.

| ID | Producer → Consumer | Trigger/payload minimum | Idempotency | Failure/retry | Security/privacy |
| --- | --- | --- | --- | --- | --- |
| `BIL-INT-001` | Registration → Billing | encounter opened/transfer; EncounterId, patient ref, service type, payer context | EncounterId+version | retry; satu invoice | ID sensitif, least privilege |
| `BIL-INT-002` | Clinical/Lab/Radiology → Billing | order billable/completed/cancelled; SourceDomain, SourceDetailId, qty, status, timestamps | source tuple | duplicate no-op; out-of-order version check | tanpa clinical narrative |
| `BIL-INT-003` | Pharmacy → Billing | dispensed final; actual qty, item/tariff ref | dispense detail ID | correction sebagai event baru | item obat minimum |
| `BIL-INT-004` | Inpatient/Bed → Billing | occupancy timeline/transfer/correction | occupancy segment+version | reject overlap; adjustment after posting | room/episode ref |
| `BIL-INT-005` | Pricing/Coverage → Billing | effective tariff/share/primary/excess result | policy/version ID | snapshot calculation; recalc while open | contract detail dibatasi |
| `BIL-INT-006` | Payment Provider → Billing | attempt result/reference/status/time | provider reference+idempotency key | timeout remains pending; reconciliation callback | token/credential dilarang log |
| `BIL-INT-007` | Billing → AR | per debtor, amount, invoice/due date, finalization/version | handoff key | at-least-once safe; ack stored | debtor sensitive |
| `BIL-INT-008` | Billing → AP | doctor, share amount, readiness policy/status | handoff key | at-least-once safe | doctor ID sensitive |
| `BIL-INT-009` | Billing → AR/AP | debit/credit adjustment, original ref, correlation | correlation key | immutable retry | reason minimum |

Urutan coverage adalah primary dahulu, excess hanya residual, lalu patient. Klaim ditolak tidak memindahkan debtor tanpa contract policy. InvoiceDate tidak berubah karena pembayaran; self-pay due pada invoice date, penjamin mengikuti term. Late noncash settlement tetap dikaitkan ke tender asal dan tidak mengubah physical cash shift closed.

Setiap message menyertakan `ContractVersion`, `OccurredAt`, `CorrelationId`, `CausationId`, source version, dan schema validation. Dead-letter/replay wajib terlihat operasional. Tidak ada distributed transaction; producer mempertahankan source of truth, Billing menyimpan receipt/outbox dan reconciliation status. Tests `BIL-AT-002`,`004`,`009`,`017`,`019`,`021`.
