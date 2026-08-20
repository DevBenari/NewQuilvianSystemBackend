# Billing dan Kasir — Business Module Blueprint

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Blueprint revision | `0.3-draft` |
| Status | `DRAFT_PENDING_APPROVAL` |
| Decision contract | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `0.2`, approved |
| Capability evidence | [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `0.2` |
| Requirement gate | [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) revision `0.3`, `READY_FOR_DOMAIN_DESIGN` |
| Domain architecture | [`03-hospital-domain-architecture.md`](./03-hospital-domain-architecture.md) revision `0.3`, `DOMAIN_ARCHITECTURE_READY` |
| Legacy evidence | [`05-servicebilling-attachment-evidence.md`](./05-servicebilling-attachment-evidence.md) revision `0.1` |
| Backend/frontend SHA | `e6f6ecba1537783ea2eb379ac12cc97790707303` / `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Input revision hash | `dfa3541bee4943660a9aac51dfb2e0e254ea527c84fb4299a935c870523c8c12` |
| Compatibility impact | Additive new transaction capabilities plus producer/legacy repair and migration reconciliation |
| Approval owner | Product/Domain Owner bersama affected Finance, Clinical, AR/AP, dan Cashier owners |

## 1. Design statement

Target menyediakan satu financial account per encounter, menerima idempotent charge facts tanpa
mengambil clinical ownership, menyimpan versioned financial calculations, memisahkan invoice,
deposit, tender, write-off, dan AR/AP, serta menjaga seluruh correction sebagai immutable adjustment.

Blueprint ini implementation-neutral: tidak menetapkan physical schema, class, endpoint, route,
message broker, page hierarchy, visual design, atau implementation task.

## 2. Target scope

- Invoice, billing item, calculation/snapshot, tax, payer allocation, admin fee, discount, room charge.
- Deposit, progress payment, split tender, provider reconciliation, allocation, refundable credit.
- Refund, write-off, reversal, dan post-final AR/AP adjustment.
- OTC pay-before-service, emergency conversion, cancellation, discharge/departure exception.
- AR per debtor, AP dokter basis/readiness, invoice/due-date/aging semantics.
- Cashier shift/register, variance, handover, reopen, authorization, audit, notification/report facts.

Provider-specific transport, concrete UI design, report layout, notification channel/SLA, migration
execution, dan deployment remain outside this design contract.

## 3. As-is contract

| Area | Current evidence | Classification | Target treatment |
| --- | --- | --- | --- |
| Billing masters | Payment Method dan Billing Item Category authorized CRUD | `Extend` | Reuse as references; transaction behavior remains new |
| Encounter/guarantor | Episode dan payer snapshot inputs | `REUSE WITH ADAPTER` | Preserve Registration ownership |
| Pricing/coverage | Existing resolver calculates tariff/coverage | `REUSE WITH ADAPTER` | Persist version/provenance; add primary/excess/tax target rules |
| Procedure marker | Billing marker without authoritative item writer | `REPAIR` | Replace marker-only behavior with stable source contract |
| Prescription marker | Can announce Billing without authoritative invoice/item | `REPAIR` | Reconcile orphan states; dispensed quantity is financial final quantity |
| Lab Order | Minimal source lifecycle | `EXTEND` | Add producer contract needed by Billing without moving clinical ownership |
| Radiology source | Transaction source not evidenced | `MISSING` | Producer-owner delivery dependency |
| Transaction invoice/payment/deposit/refund/shift | Not found current V2 | `MISSING` | New logical capabilities |
| AP doctor shell | Payroll models without final invoice source | `REUSE WITH ADAPTER` | Downstream AP adapter; no duplicate AP owner |
| Frontend cashier | No transaction workspace | `MISSING` | New behavior/workspace after backend contract locks |
| Legacy ServiceBilling | Partial services with conflicting paid/write-off/excess/room semantics | `CONFLICT` / reference | Data profiling/reconciliation; never copy as target policy |

## 4. Ownership contract

| ID | Concept | Authoritative owner | Class | Consumer rule |
| --- | --- | --- | --- | --- |
| `BIL-OWN-001` | Patient/Encounter/emergency conversion | Registration | `Existing` | Billing keeps reference and financial snapshot only |
| `BIL-OWN-002` | Order/detail performed/completed/void | Each producer | `Existing` / `Adapter/View` | Billing cannot mutate clinical state |
| `BIL-OWN-003` | Occupancy timeline | Inpatient/Bed | `Existing` / `Adapter/View` | Billing applies effective room policy to facts |
| `BIL-OWN-004` | Tariff/coverage/payer contract | Master owners | `Existing` / `Extend` | Billing stores calculation provenance/output |
| `BIL-OWN-005` | Invoice/item/calculation/snapshot | Billing Account | `New` | Financial source of truth per encounter |
| `BIL-OWN-006` | Tax/admin/discount/room policy | Finance/Inpatient policy; IT config | `New` / `Extend` | Effective-dated with approval provenance |
| `BIL-OWN-007` | Deposit/settlement/tender/allocation/credit | Patient Funds | `New` | Balance from ledger, never audit-log calculation |
| `BIL-OWN-008` | Refund/write-off/final adjustment | Financial Exception | `New` | Maker-checker and compensating effects |
| `BIL-OWN-009` | Cashier shift/register/variance | Cashier Operations | `New` | Receipts/cash movement link to open shift |
| `BIL-OWN-010` | Finalization and AR/AP handoff facts | Billing Finalization | `New` output | AR/AP retain downstream lifecycle ownership |
| `BIL-OWN-011` | Permissions/actor identity | Identity/Authorization | `Existing` / `Adapter/View` | Backend enforcement authoritative |

## 5. Target contracts

### `BIL-CON-001` — Invoice and charge identity

- Exactly one invoice per encounter.
- At most one active item per `(SourceDomain, SourceDetailId)`; retry returns same effect.
- Source facts remain owned by producer; financial item never proves clinical completion.
- Void/correction preserves original history; no financial hard delete.

### `BIL-CON-002` — Versioned financial calculation

- Every version records tariff/coverage/discount/tax/patient/payer inputs, results, policy/effective
  references, actor/source, and old/new values.
- Primary is evaluated before excess; excess uses its own contract; total coverage is capped.
- Tax applies only via effective rule after eligible item discount and allocates by responsibility/
  contract using configured consistent decimal rounding.
- Unlocked invoices recalculate on eligible master change; old versions remain queryable.

### `BIL-CON-003` — Financial locking

- Rajal/OTC locks when checkout begins.
- Inpatient progress allocation does not lock; final settlement initiation locks.
- Locked/closed facts change only through authorized adjustment/reversal.
- Administrative reopen is non-financial and cannot unlock amounts.

### `BIL-CON-004` — Administration fee and discount

- Outpatient admin fee applies at most once per patient/local Jakarta date; inpatient once per admission.
- Transfer in the same encounter replaces outpatient with inpatient fee and records difference.
- Admin fee can be patient/payer responsibility but is not discountable.
- Finance-approved master discount is automatic; ad-hoc requires Finance maker-checker.
- Doctor discount only reduces doctor share and requires doctor approval per approved flow.

### `BIL-CON-005` — Room charge

- Occupancy timeline is authoritative and segments cannot overlap.
- Effective policy configures 24-hour/service-day method, minimum, residual rounding, tariff point,
  leave treatment, and payer variation.
- Transfer does not reset admission minimum or silently duplicate duration.
- Correction appends adjustment and preserves prior charge/history.

### `BIL-CON-006` — Deposit and progress allocation

- Top-up increases unallocated deposit only.
- Progress allocation cannot exceed available deposit and keeps invoice open for new charges.
- Final settlement uses eligible remaining deposit before collecting shortfall.
- Excess becomes Refundable Credit; correction uses compensating movement.

### `BIL-CON-007` — Split tender and reconciliation

- Each tender has independent identity/state and successful tender survives sibling failure.
- Outstanding decreases only through settled allocation.
- Timeout/uncertain outcome becomes `PENDING_RECONCILIATION`, cannot be charged again, and blocks
  normal close until reconciled.
- Provider callback/inquiry/retry must be idempotent.

### `BIL-CON-008` — Refund

- Refund is proportional to original settled tenders and executed by Finance.
- Partial success remains recorded; failed remainder becomes `REFUND_PENDING`.
- Alternative method requires confirmed original-method failure and patient identity verification.
- Refund never erases original tender/allocation.

### `BIL-CON-009` — Write-off

- Billing/AR maker and Finance approver are separated.
- Full write-off yields `SETTLED_BY_WRITE_OFF`, never `PAID`; partial reduces outstanding only.
- Reversal appends correction and reopens AR balance without deleting write-off history.

### `BIL-CON-010` — OTC, cancellation, and departure

- OTC unsettled cannot release service; emergency converts to IGD/emergency without delaying care.
- Original owner may cancel before performed; substitute needs same profession/unit authority,
  head/shift approval, reason, and not-performed confirmation.
- Normal departure requires settlement.
- Death, emergency transfer, and DAMA/APS may depart with exception reason/lawful debtor; remaining
  balance becomes AR and departure is not held.

### `BIL-CON-011` — Finalization, AR, AP, dates, and adjustment

- Finalization is idempotent per locked invoice version after source/tender reconciliation.
- `InvoiceDate` is finalization date and never payment date.
- Self-pay due date equals invoice date; payer due follows claim acceptance/contract term.
- AR age starts on posting and overdue is evaluated against due date.
- AR is produced once per debtor/version; claim rejection stays AR unless valid contract/policy
  explicitly permits responsibility transfer.
- AP basis is one per eligible doctor item/version; readiness follows self-pay/insured policy.
- Post-final correction produces idempotent AR/AP debit/credit adjustment and patient outstanding/
  refundable credit without mutating original posting.

### `BIL-CON-012` — Cashier shift

- One active shift per cashier with opening balance.
- Close stores system cash, physical cash, ending balance, and variance.
- Variance persists until Head Cashier review; reopen is authorized/reasoned/audited.
- Handover requires outgoing/incoming acknowledgement.
- Late noncash settlement retains original attempt and does not mutate closed physical cash.

### `BIL-CON-013` — Authorization, audit, and privacy

- Kasir sees operational invoice/patient/deposit/payment/discount/reference fields.
- Billing sees full invoice/coverage/adjustment/reconciliation; Finance/AR sees all financial facts.
- Doctor/unit sees own service items and settlement status, not deposit/payment/AR details.
- Head Cashier sees cashier transactions/shift controls.
- Material mutation exposes actor, authority, timestamp, reason, before/after, source, policy/version,
  correlation, original link, approval, and downstream result.

## 6. Lifecycle contract

| Lifecycle | States/outcome |
| --- | --- |
| Invoice | `OPEN -> FINANCIAL_LOCKED -> FINALIZED -> CLOSED`; administrative reopen is non-financial |
| Responsibility | `OUTSTANDING`, `PARTIALLY_SETTLED`, `SETTLED`, `SETTLED_BY_WRITE_OFF`, `TRANSFERRED_TO_AR` |
| Tender | `CREATED -> PENDING -> SETTLED/FAILED/EXPIRED/PENDING_RECONCILIATION` |
| Refund | `REQUESTED -> AUTHORIZED -> PROCESSING -> COMPLETED/PARTIALLY_COMPLETED/REFUND_PENDING/REJECTED` |
| Write-off | `DRAFT -> SUBMITTED -> APPROVED/REJECTED -> POSTED -> REVERSED` |
| Shift | `OPEN -> CLOSING -> CLOSED`; authorized `CLOSED -> REOPENED -> CLOSED` |

No additional business state may be treated as approved without contract revision/impact review.

## 7. Integration contract

| ID | Boundary | Minimum business payload | Failure/idempotency | Status |
| --- | --- | --- | --- | --- |
| `BIL-INT-001` | Registration -> Billing | Encounter, patient, episode, payer refs, emergency conversion | Unknown held/rejected; no duplicate patient | Ready abstract boundary |
| `BIL-INT-002` | Producer -> Billing | Source tuple, encounter, quantity/unit, lifecycle fact, correlation | One active item; capture failure visible | Ready per approved producer timing |
| `BIL-INT-003` | Inpatient/Bed -> Billing | Occupancy/transfer/leave identity and timeline version | Overlap rejected/reconciled | Ready abstract boundary |
| `BIL-INT-004` | Tariff/Coverage/Policy -> Billing | Effective refs and calculation inputs/results | Provenance required; failure prevents lock | Ready with adapters |
| `BIL-INT-005` | Settlement/Refund <-> Provider | Attempt/refund identity, provider ref, amount, status evidence | Idempotent callback/inquiry/reconciliation | Provider-neutral ready |
| `BIL-INT-006` | Billing Finalization -> AR | Invoice/version, debtor, amount, invoice/due date, correlation | One effect per debtor/version | Ready abstract boundary |
| `BIL-INT-007` | Billing Finalization -> AP | Invoice/item/version, doctor share, readiness condition, correlation | One basis per eligible item/version | Ready abstract boundary |
| `BIL-INT-008` | Billing Adjustment -> AR/AP | Original posting, debit/credit delta, approval, correlation | Retry no duplicate effect | Ready abstract boundary |
| `BIL-INT-009` | Billing events -> Notification/Reporting | Event identity, subject, status/reason, timestamps | Delivery policy external | Facts ready; channel/layout proposed |

## 8. Frontend behavior needs

Contract version: `BIL-FE-BEHAVIOR-0.3-draft`.

- Worklist must distinguish encounter/invoice state, responsibility status, reconciliation, and
  departure exception without exposing unauthorized financial fields.
- Invoice detail must display source, quantity, calculation version, patient/primary/excess portion,
  tax, discount, admin/room charge, adjustment, and history based on backend output.
- Inpatient view separates deposit balance, unallocated funds, progress allocations, current
  outstanding, and refundable credit.
- Checkout supports split tender with independent outcomes; successful portions remain visible and
  uncertain portions disable duplicate collection while offering authorized reconciliation action.
- Refund/write-off/adjustment views preserve maker/approver separation, reasons, original links,
  partial outcomes, and audit history.
- Shift workspace shows opening/system/physical/ending/variance, handover, review, and authorized reopen.
- Loading, empty, validation, unauthorized, stale/concurrent, partial-failure, retry, accessibility,
  responsive, and privacy states are required behaviors.
- Backend permission/action availability remains authoritative. Route, layout, component composition,
  and visual treatment are `DEV_DISCRETION` within approved privacy/invariant constraints.

## 9. Validation and concurrency contract

- Concurrent invoice creation, charge retry, policy recalculation, deposit allocation, tender callback,
  refund, finalization, AR/AP handoff, and shift close must not duplicate business effects.
- Monetary mutations require expected version/concurrency handling and return current state on conflict.
- Financial operations spanning multiple owned records are atomic or expose durable recovery state.
- Integration delivery can be at-least-once only when consumers are idempotent and reconciliation exists.

## 10. Compatibility and migration impact

| Existing/legacy area | Required treatment |
| --- | --- |
| Procedure/prescription Billing markers | Profile/reconcile orphan/false markers; authoritative item wins |
| Legacy `MainKasir` invoice/date | Map identity carefully; never use payment date as invoice date |
| Legacy bulk paid boolean | Split into tender, responsibility, invoice, and AR states |
| Legacy primary/excess flags | Recalculate/validate against contracts; do not trust auto-excess |
| Legacy 90-day auto write-off | Do not migrate as approved write-off; require maker/approver evidence |
| Legacy room `ceil(duration)` | Convert only through approved effective policy and occupancy evidence |
| Legacy doctor FoC | Restrict effect to doctor-share component |

No data migration/cutover strategy or database execution is authorized. Profiling, mapping,
reconciliation report, compatibility window, rollback, and cutover require delivery tasks/authority.

## 11. Acceptance strategy

| ID | Scenario | Contract trace | Required evidence |
| --- | --- | --- | --- |
| `BIL-AT-001` | Concurrent/retry encounter create yields one invoice | `001` | Persistence/concurrency test |
| `BIL-AT-002` | Same source tuple yields one active item | `001` | Idempotency test |
| `BIL-AT-003` | Repricing appends version; old version remains | `002` | Domain/history test |
| `BIL-AT-004` | Primary/excess/tax allocation respects contracts/caps/effective rules | `002` | Calculation matrix test |
| `BIL-AT-005` | Rajal checkout locks; inpatient progress payment does not | `003`, `006` | State integration test |
| `BIL-AT-006` | Admin fee dedupes/replaces and rejects discount | `004` | Date/concurrency/transfer test |
| `BIL-AT-007` | Doctor discount changes only doctor share after approval | `004`, `011` | Approval/calculation test |
| `BIL-AT-008` | Occupancy transfer produces deterministic non-overlap room charge | `005` | Policy/timeline test |
| `BIL-AT-009` | Deposit top-up/progress/final/excess ledger balances | `006` | Ledger/concurrency test |
| `BIL-AT-010` | Cash settled + QRIS failed retains cash and correct outstanding | `007` | Split-tender test |
| `BIL-AT-011` | Timeout blocks duplicate collection until reconciliation | `007` | Provider failure/recovery test |
| `BIL-AT-012` | Split refund partial failure preserves success and pending remainder | `008` | Refund allocation test |
| `BIL-AT-013` | Full/partial/reversed write-off never appears as paid | `009` | Workflow/ledger test |
| `BIL-AT-014` | OTC unsettled blocked; emergency conversion allows clinical care | `010` | Cross-context safety test |
| `BIL-AT-015` | Substitute cancellation requires all authority/evidence guards | `010` | Authorization/source test |
| `BIL-AT-016` | Death/transfer/DAMA departure creates lawful-debtor AR without hold | `010`, `011` | Exception end-to-end test |
| `BIL-AT-017` | Finalization retry produces one AR per debtor and one AP basis per item | `011` | Idempotency/integration test |
| `BIL-AT-018` | Payment never changes invoice/AR dates; overdue follows due date | `011` | Date/aging test |
| `BIL-AT-019` | Final adjustment appends AR/AP debit/credit without mutation | `011` | History/recovery test |
| `BIL-AT-020` | Shift close/variance/review/reopen/handover remains auditable | `012` | Lifecycle/authorization test |
| `BIL-AT-021` | Late noncash settlement does not alter closed physical cash | `012` | Reconciliation test |
| `BIL-AT-022` | Each actor receives only approved financial scope | `013` | Authorization/privacy test |
| `BIL-AT-023` | All material mutations expose required audit provenance | All | Audit evidence test |

## 12. Contract versions

| Contract version | Scope | Status |
| --- | --- | --- |
| `BIL-CORE-0.3-draft` | `BIL-CON-001`–`005` | `DRAFT_PENDING_APPROVAL` |
| `BIL-SETTLEMENT-0.3-draft` | `BIL-CON-006`, `007` | `DRAFT_PENDING_APPROVAL` |
| `BIL-EXCEPTION-0.3-draft` | `BIL-CON-008`–`010` | `DRAFT_PENDING_APPROVAL` |
| `BIL-FINALIZATION-0.3-draft` | `BIL-CON-011` | `DRAFT_PENDING_APPROVAL` |
| `BIL-CASHIER-0.3-draft` | `BIL-CON-012` | `DRAFT_PENDING_APPROVAL` |
| `BIL-FE-BEHAVIOR-0.3-draft` | Frontend behavior/privacy/error states | `DRAFT_PENDING_APPROVAL` |

## 13. Approval gates dan handoff

Before delivery planning:

1. Product/Domain Owner approves blueprint revision `0.3` and all contract versions above.
2. Finance/Billing, Clinical/Inpatient, AR/AP, Cashier, Registration/producer, dan Security owners
   confirm their boundaries and acceptance obligations.
3. Configurable defaults receive owner/value/effective-date prerequisites in affected roadmap tasks.
4. `plan-module-delivery` creates small BE/FE task IDs with locked contract/acceptance/dependencies.
5. Builder preflight verifies branches, source snapshots, governance, explicit write targets, and task authority.

Current outcome: `DRAFT_COMPLETE_BLUEPRINT`.

- Requirement readiness: `READY_FOR_DOMAIN_DESIGN`.
- Architecture readiness: `DOMAIN_ARCHITECTURE_READY`.
- Blocking business decisions: none.
- Approval blocker: `BIL-APR-001`.
- Build status: not authorized until approval and delivery roadmap exist.
