# Billing dan Kasir — Hospital Domain Architecture

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Architecture ID | `BIL-CASH-HDA-001` |
| Architecture revision | `0.3` |
| Status | `DRAFT` — menunggu composition/approval blueprint |
| Architecture readiness | `DOMAIN_ARCHITECTURE_READY` |
| Requirement input | [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) revision `0.3`, `READY_FOR_DOMAIN_DESIGN` |
| Decision basis | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `0.2`, `BKC-DEC-001`–`044` approved |
| Current-state evidence | [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `0.2` dan [`05-servicebilling-attachment-evidence.md`](./05-servicebilling-attachment-evidence.md) |
| Backend/frontend SHA | `e6f6ecba1537783ea2eb379ac12cc97790707303` / `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Blocking Decision IDs | None |

## 1. Scope dan trace

Arsitektur mencakup invoice/charge, calculation policy, deposit, settlement, refund, write-off,
cashier shift, finalization, AR/AP handoff, post-final adjustment, payer coordination, tax,
administration fee, room charge, OTC release, serta departure exception.

Current V2 hanya menyediakan master dan adapter candidates; konsep target di bawah adalah logical
domain concepts, bukan table, class, endpoint, menu, atau keputusan deployment.

| Architecture concern | Approved trace |
| --- | --- |
| Invoice/charge uniqueness dan correction | `BKC-DEC-003`–`005`, `039`, `040` |
| Locking, deposit, settlement, split tender | `BKC-DEC-006`, `009`, `010`, `016`, `022` |
| Payer, tax, AR/AP, invoice dates | `BKC-DEC-007`, `024`, `025`, `029`, `037`, `041`, `042`, `044` |
| Cancellation/refund/write-off | `BKC-DEC-005`, `012`, `020`, `034`–`036` |
| Administration/discount/room policy | `BKC-DEC-014`, `015`, `017`–`019`, `028`, `043` |
| OTC/discharge/close | `BKC-DEC-011`, `023`, `026`, `027`, `032`, `033` |
| Authorization/shift/audit | `BKC-DEC-021`, `031`, `038`, `040` |

Prior Inpatient observations remain `REFERENCE_ONLY`; canonical Billing/Cashier/Payer reference
was `NOT_YET_AVAILABLE`. Approved local decisions are authoritative target evidence.

## 2. Ubiquitous language

| Term | Business meaning |
| --- | --- |
| Billing Invoice | Financial account tunggal untuk satu encounter |
| Charge Source | Authoritative service/usage/occupancy fact dari domain pemilik |
| Billing Item | Financial representation dari satu source detail atau internal fee/adjustment |
| Calculation Version | Immutable tariff/coverage/tax/discount/patient-payer calculation with provenance |
| Financial Snapshot | Calculation version yang dikunci untuk settlement/finalization |
| Patient Responsibility | Kewajiban pasien setelah payer allocation dan adjustment |
| Payer Portion | Kewajiban satu debtor penjamin yang menjadi basis AR |
| Deposit | Dana rawat inap yang diterima tetapi belum otomatis dialokasikan |
| Progress Payment | Allocation deposit ke invoice berjalan tanpa financial lock |
| Tender | Satu payment attempt dengan satu method dan lifecycle mandiri |
| Pending Reconciliation | Outcome external tender belum determinate dan tidak boleh ditagih ulang |
| Refundable Credit | Hak pengembalian dana yang belum dieksekusi |
| Write-off | Non-cash approved settlement terhadap AR; tidak pernah berarti paid |
| Finalization | Lock dan idempotent financial handoff setelah charge completeness terpenuhi |
| Debtor | Patient/penanggung/primary/excess entity yang secara sah menanggung AR portion |
| Service Day Policy | Effective rule untuk mengubah occupancy timeline menjadi room charge |

## 3. Bounded-context map

| Context ID | Context | Responsibility | Owned concepts | Relationship |
| --- | --- | --- | --- | --- |
| `BIL-CTX-01` | Billing Account & Charge | Satu invoice encounter, item idempotent, calculation versions, fee/discount/tax, lock, close prerequisites | Invoice, Item, Calculation, Snapshot, Policy Applications | Consumes encounter/source/tariff/coverage/occupancy facts |
| `BIL-CTX-02` | Patient Funds & Settlement | Deposit ledger, settlement, split tender, allocation, reconciliation, refundable credit | Deposit Account, Settlement, Tender, Allocation, Refundable Credit | Consumes Payment Method/Provider; informs Invoice settlement |
| `BIL-CTX-03` | Financial Exception & Adjustment | Refund, write-off, reversal, post-final adjustment dengan maker-checker dan immutable history | Refund Case, Write-off Case, Financial Adjustment | Updates financial outcomes through compensating effects |
| `BIL-CTX-04` | Cashier Operations | Shift/register, opening/closing, physical/system cash, handover, variance/review/reopen | Cashier Shift, Register Session, Cash Movement, Variance Review | Receipts reference open shift; late noncash stays external reconciliation |
| `BIL-CTX-05` | Billing Finalization & Handoff | Finalization record, AR per debtor, AP doctor basis/readiness facts, idempotent corrections | Finalization Record, AR/AP Handoff, Handoff Adjustment | Publishes to AR/AP; downstream owners retain receivable/payable lifecycle |

## 4. External ownership boundaries

| External owner | Authoritative responsibility | Billing treatment | Ownership |
| --- | --- | --- | --- |
| Registration/Encounter | Patient, encounter identity, episode, payer references, emergency conversion | Reference/snapshot only | `Existing` + `Adapter/View` |
| Clinical/service producer | Order/detail identity, performed/completed/void meaning | `(SourceDomain, SourceDetailId)` and lifecycle facts | `Existing` + `Adapter/View` |
| Inpatient/Bed | Occupancy timeline, transfer, bed release/leave facts | Input to effective room policy | `Existing` + `Adapter/View` |
| Tariff/Coverage/Payer Contract | Effective tariff, benefit/contract/claim terms | Versioned calculation provenance | `Existing` / `Extend` + `Adapter/View` |
| Finance/AR | Receivable lifecycle, claim collection, debtor settlement | Receives idempotent AR basis/adjustment | `Adapter/View` downstream |
| AP/Payroll | Payable lifecycle dan payment | Receives doctor-share basis/readiness facts | `Adapter/View` downstream |
| Payment Provider | External payment/refund outcome | Attempt/reference/webhook/inquiry evidence | `EXTERNAL_CONTRACT` |
| Identity/Authorization | Actor identity dan permission enforcement | Authority reference and verification | `Existing` + `Adapter/View` |

## 5. Domain concept catalog

| ID | Concept | Classification | Owner | Ownership | Identity/lifecycle role |
| --- | --- | --- | --- | --- | --- |
| `BIL-CPT-001` | Billing Invoice | `AGGREGATE_ROOT` | `BIL-CTX-01` | `New` | Satu per encounter; financial lifecycle boundary |
| `BIL-CPT-002` | Billing Item | `ENTITY` | Invoice | `New` | Unique active source representation; void without delete |
| `BIL-CPT-003` | Financial Calculation Version | `ENTITY` | Invoice | `New` | Immutable calculation/provenance history |
| `BIL-CPT-004` | Financial Breakdown | `VALUE_OBJECT` | Invoice | `New` | Gross, discount, tax, patient, primary, excess/debtor portions |
| `BIL-CPT-005` | Financial Snapshot | `VALUE_OBJECT` | Invoice | `New` | Locked version for financial outcome |
| `BIL-CPT-006` | Financial Adjustment | `ENTITY` | `BIL-CTX-03` | `New` | Append-only correction before/after finalization |
| `BIL-CPT-007` | Administration Fee Policy | `REFERENCE_DATA` | Billing policy; Finance owner, IT configurator | `New` | Effective eligibility/nominal/replacement rule |
| `BIL-CPT-008` | Discount Policy | `REFERENCE_DATA` | Billing/Finance | `New` | Effective master/ad-hoc/doctor-share eligibility |
| `BIL-CPT-009` | Discount Application | `ENTITY` | Invoice | `New` | Applied policy/approval and financial effect |
| `BIL-CPT-010` | Encounter Reference | `EXTERNAL_CONTRACT` | Registration | `Existing` / `Adapter/View` | Episode correlation and payer context |
| `BIL-CPT-011` | Charge Source Reference | `EXTERNAL_CONTRACT` | Producer | `Adapter/View` | Stable source tuple, lifecycle, quantity |
| `BIL-CPT-012` | Inpatient Deposit Account | `AGGREGATE_ROOT` | `BIL-CTX-02` | `New` | Unallocated fund ledger per inpatient episode |
| `BIL-CPT-013` | Deposit Movement | `ENTITY` | Deposit Account | `New` | Top-up/allocation/release/compensation |
| `BIL-CPT-014` | Settlement | `AGGREGATE_ROOT` | `BIL-CTX-02` | `New` | Groups attempts and allocations to financial purpose |
| `BIL-CPT-015` | Tender | `ENTITY` | Settlement | `New` | Independent payment attempt and idempotency identity |
| `BIL-CPT-016` | Payment Allocation | `ENTITY` | Settlement | `New` | Settled funds applied to invoice/deposit purpose |
| `BIL-CPT-017` | Refundable Credit | `ENTITY` | `BIL-CTX-02` | `New` | Recognized refundable balance, not executed refund |
| `BIL-CPT-018` | Finalization Record | `AGGREGATE_ROOT` | `BIL-CTX-05` | `New` | One final business effect per invoice version |
| `BIL-CPT-019` | AR Handoff | `DOMAIN_EVENT` / `EXTERNAL_CONTRACT` | Billing -> AR | `New` output | Per debtor/version idempotent AR basis |
| `BIL-CPT-020` | Billing Audit Fact | `DOMAIN_EVENT` | Context owner | `New` | Durable actor/reason/before-after/correlation fact |
| `BIL-CPT-021` | Tax Rule | `REFERENCE_DATA` | Finance/Tax policy | `New` | Effective taxable basis/rate/allocation rule |
| `BIL-CPT-022` | Payer Allocation | `VALUE_OBJECT` | Invoice Calculation | `New` | Primary then excess then patient residual; capped |
| `BIL-CPT-023` | Room Charge Policy | `REFERENCE_DATA` | Inpatient + Billing/Finance policy | `New` | Effective service-day/minimum/rounding/tariff/leave rule |
| `BIL-CPT-024` | Occupancy Reference | `EXTERNAL_CONTRACT` | Inpatient/Bed | `Adapter/View` | Non-overlapping occupancy/transfer timeline |
| `BIL-CPT-025` | Refund Case | `AGGREGATE_ROOT` | `BIL-CTX-03` | `New` | Proportional original-method refund and partial-failure state |
| `BIL-CPT-026` | Write-off Case | `AGGREGATE_ROOT` | `BIL-CTX-03` | `New` | Maker/approver, partial/full settlement, reversal |
| `BIL-CPT-027` | Cashier Shift | `AGGREGATE_ROOT` | `BIL-CTX-04` | `New` | One active shift per cashier; close/reopen/handover |
| `BIL-CPT-028` | Cash Variance Review | `ENTITY` | Cashier Shift | `New` | Persistent variance and Head Cashier resolution |
| `BIL-CPT-029` | AP Doctor Handoff | `DOMAIN_EVENT` / `EXTERNAL_CONTRACT` | Billing -> AP | `New` output | Doctor-share basis and readiness condition |
| `BIL-CPT-030` | Handoff Adjustment | `DOMAIN_EVENT` / `EXTERNAL_CONTRACT` | Billing -> AR/AP | `New` output | Debit/credit correction linked to original posting |
| `BIL-CPT-031` | Departure Financial Exception | `VALUE_OBJECT` | Invoice/Finalization | `New` | Death/transfer/DAMA reason and lawful debtor |

## 6. Aggregate model dan invariants

### 6.1 Billing Invoice

- Exactly one invoice per encounter.
- At most one active item for `(SourceDomain, SourceDetailId)`; retry is idempotent.
- Recalculation appends version; lock selects a snapshot and never overwrites history.
- Primary then excess allocation cannot exceed eligible charge; remaining value is patient portion.
- Tax applies only through effective rule after eligible item discount.
- Administration fee is non-discountable and deduplicated/replaced per approved policy.
- Closed/final facts change only through authorized adjustment.
- Close/finalization requires source reconciliation and no indeterminate tender.

### 6.2 Deposit Account

- Top-up remains unallocated until explicit movement.
- Progress allocation cannot exceed available balance and does not lock invoice.
- Every correction is compensating; no balance-affecting delete.
- Final excess creates Refundable Credit.

### 6.3 Settlement

- Each tender has independent lifecycle/idempotency identity.
- Successful tender persists when another fails; only settled allocation reduces outstanding.
- Pending reconciliation cannot be recharged and blocks normal close.
- Allocation cannot exceed settled/available funds or target without recognized refundable credit.

### 6.4 Refund Case

- Refund amount is allocated proportionally to original successful tenders.
- Partial success remains successful; failed remainder is `REFUND_PENDING`.
- Alternative method requires Finance authority and verified original-method failure/patient identity.

### 6.5 Write-off Case

- Maker Billing/AR cannot self-approve as Finance approver.
- Full approved write-off yields `SETTLED_BY_WRITE_OFF`, never `PAID`.
- Partial write-off reduces balance; reversal appends compensation and reopens AR balance.

### 6.6 Cashier Shift

- One active shift per cashier; receipt requiring cashier cash handling references an open shift.
- Close records opening/system cash/physical cash/ending/variance.
- Variance remains unresolved until reviewed; reopen is authorized and audited.
- Handover requires outgoing/incoming acknowledgements.
- Late noncash settlement keeps original attempt correlation and never mutates closed physical cash.

### 6.7 Finalization Record

- One finalization business effect per locked invoice version.
- Normal finalization requires patient settlement; approved departure exception may create patient/
  lawful-debtor AR without holding departure.
- AR handoff is one per debtor/version; AP basis is one per eligible doctor-share item.
- Retry and post-final adjustment use correlation/idempotency keys.

## 7. Relationship model

| Source | Relationship | Target | Cardinality/semantics |
| --- | --- | --- | --- |
| Encounter | Own financial account | Invoice | `1 : 1` |
| Invoice | Contains | Billing Item | `1 : 0..*`, append/void |
| Billing Item | References | Charge Source | `0..1 : 1`; internal fee/adjustment may have internal source |
| Billing Item/Invoice | Has | Calculation Version | `1 : 1..*` after calculation |
| Invoice | Locks | Financial Snapshot | `1 : 0..1` active locked snapshot |
| Patient/local date | Eligible for | Outpatient Admin Fee | At most one active assessment per policy day |
| Inpatient episode | Has | Deposit Account | `1 : 0..1` logical account |
| Invoice | Settled through | Settlement | `1 : 0..*` attempts |
| Settlement | Contains | Tender | `1 : 1..*` after initiation |
| Settled funds | Applied by | Allocation | Total cannot exceed available funds |
| Refund Case | Reverses | Original Tender(s) | `1 : 1..*`, proportional |
| Invoice final version | Produces | AR Handoff | `1 : 0..*`, one per debtor |
| Eligible doctor item | Produces | AP Handoff | `1 : 0..1` per final version |
| Occupancy timeline | Evaluated by | Room Policy | Non-overlap input -> deterministic charge periods |
| Cashier Shift | Groups | Cash receipts/movements | `1 : 0..*` |

## 8. Lifecycle model

### Invoice and responsibility

`OPEN -> FINANCIAL_LOCKED -> FINALIZED -> CLOSED`

- Rajal/OTC lock when checkout begins; inpatient progress allocation stays `OPEN`.
- Inpatient locks when final settlement begins.
- Responsibility outcome is tracked separately: `OUTSTANDING`, `PARTIALLY_SETTLED`, `SETTLED`,
  `SETTLED_BY_WRITE_OFF`, or `TRANSFERRED_TO_AR` for approved exception.
- Administrative non-financial reopen does not unlock/change financial amounts.

### Tender

`CREATED -> PENDING -> SETTLED | FAILED | EXPIRED | PENDING_RECONCILIATION`

`PENDING_RECONCILIATION -> SETTLED | FAILED | EXPIRED` only through provider evidence/reconciliation.

### Refund

`REQUESTED -> AUTHORIZED -> PROCESSING -> COMPLETED | PARTIALLY_COMPLETED | REFUND_PENDING | REJECTED`

### Write-off

`DRAFT -> SUBMITTED -> APPROVED | REJECTED -> POSTED -> REVERSED` through compensating entry.

### Cashier shift

`OPEN -> CLOSING -> CLOSED`; `CLOSED -> REOPENED -> CLOSED` only by authorized Head Cashier path.

## 9. Authorization responsibility

| Action/data scope | Authority |
| --- | --- |
| Invoice/patient portion/deposit/payment/discount/reference | Kasir within operational scope |
| Full invoice/coverage/adjustment/reconciliation across units | Billing |
| AR/refund/write-off/full financial audit | Finance/AR |
| Own service items and settlement release status only | Doctor/service unit |
| All cashier transactions, shift review/reopen/variance | Head Cashier |
| Original clinical cancellation | Original owner before performed |
| Substitute cancellation | Same profession/unit + head/shift approval + executor not-performed confirmation |
| Refund execution, fallback, post-final adjustment approval | Finance |
| Master configuration | IT with Finance-approved provenance |

Backend permission enforcement is authoritative; role-name substring or UI visibility is not.

## 10. Audit dan history

Material financial actions retain actor, authority context, timestamp, reason, source identity,
before/after value, policy/calculation version, approval, correlation/idempotency key, provider
reference, original posting/tender link, and downstream result. Invoice/item/payment/deposit/refund/
write-off/shift/AR/AP facts are never hard-deleted when financially material.

## 11. Integration dan event boundaries

| Boundary | Direction/source of truth | Idempotency/failure responsibility |
| --- | --- | --- |
| Encounter/emergency conversion | Registration -> Billing; Registration authoritative | Unknown encounter held/rejected; no duplicate patient |
| Charge source lifecycle | Producer -> Billing; producer authoritative clinically | Stable tuple; retry one item; capture failure visible/reconciled |
| Occupancy timeline | Inpatient/Bed -> Billing | Segment identity/version; overlap rejected/reconciled |
| Tariff/coverage/tax/policy | Masters -> Billing; Billing owns calculation version | Persist provenance/effective rule; failed resolution prevents lock |
| Payment/refund provider | Settlement <-> Provider | Unique attempt/reference, idempotent callback/inquiry, reconciliation |
| AR handoff/adjustment | Billing -> AR | One effect per debtor/version/correlation; retry safe |
| AP handoff/adjustment | Billing -> AP | One basis per eligible item/version; readiness fact, retry safe |
| Notification/reporting | Domain events -> external consumers | Event facts required; channel/SLA remain non-blocking |

Provider transport, endpoint, broker, serialization, retry interval, and report/UI layout remain
outside logical domain architecture.

## 12. Billing dan clinical-safety impact

Classification: `charge-impacting` and `safety-relevant` at service-release/cancellation/departure
boundaries.

- Billing owns financial truth, not clinical truth or patient identity.
- Emergency care cannot be held by OTC payment; encounter conversion is explicit.
- Performed/complete/void facts come only from producer.
- Death, emergency transfer, and DAMA departure are not held by payment; lawful debtor/reason is
  auditable and remaining responsibility becomes AR.
- Financial adjustment cannot rewrite clinical facts or historical financial postings.

## 13. Architecture gaps

No blocking business or ownership gaps remain.

| Remaining item | Classification | Architecture treatment |
| --- | --- | --- |
| Invoice format, currency/display, exact rounding mode | `CONFIGURABLE_DEFAULT` | Effective policy/configuration prerequisite |
| Room policy values, shift tolerance, discount thresholds, AP readiness payer parameters | `CONFIGURABLE_DEFAULT` | Model as effective parameters, never hardcoded assumption |
| Notification/report format/channel/SLA | `NON_BLOCKING_STANDARD` | Preserve event/data facts; later presentation slice |
| Payment-provider protocol | `NON_BLOCKING_STANDARD` / external dependency | Provider-neutral port; concrete adapter requires contract |
| Missing/repair current producer capabilities | Delivery dependency | Preserve current capability classification and sequence roadmap |

## 14. Architecture readiness dan handoff

Overall status: `DOMAIN_ARCHITECTURE_READY`.

- Architecture revision: `0.3`.
- Requirement gate: revision `0.3`, `READY_FOR_DOMAIN_DESIGN`.
- Decision contract: revision `0.2`, approved.
- Blocking decisions: none.
- All contexts/slices above may proceed to `design-business-module`.
- Compatibility risks: current-V2 transaction Billing missing; legacy paid/write-off/coverage/room
  behaviors conflict and require explicit reconciliation/migration planning.
- Expected output: implementation-neutral as-is/to-be ownership, lifecycle, validation,
  permission, integration, frontend behavior, compatibility, acceptance, and versioned contracts.

This readiness is not human design approval and does not authorize source implementation,
migration/database execution, deployment, or Git publication.
