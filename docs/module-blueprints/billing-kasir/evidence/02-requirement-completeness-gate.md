# Billing dan Kasir — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Assessment revision | `0.3` |
| Assessment date | 20 Agustus 2026 (`Asia/Jakarta`) |
| Assessment status | `CURRENT` |
| Overall readiness | `READY_FOR_DOMAIN_DESIGN` |
| Ready destination | `hospital-domain-architect` |
| Business evidence | [`00-interview-decisions.md`](../00-interview-decisions.md), approved revision `0.2` |
| Capability evidence | [`01-existing-capability-map.md`](../01-existing-capability-map.md), revision `0.2` |
| Attachment evidence | [`05-servicebilling-attachment-evidence.md`](./05-servicebilling-attachment-evidence.md), ZIP SHA-256 `2b948721cee4154eaecaf9ac57d7621fb34cb7b61fb31a5fd6dff04df7ad218d` |
| Backend snapshot | `e6f6ecba1537783ea2eb379ac12cc97790707303` |
| Frontend snapshot | `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Write boundary | Hanya artefak blueprint; source aplikasi tidak diubah |

## 1. Assessment scope

Gate menilai kelengkapan requirement untuk:

- satu invoice per encounter dan idempotent charge capture;
- tariff, coverage, tax, discount, administration fee, dan financial locking;
- deposit rawat inap, progress payment, split tender, reconciliation, refund, dan write-off;
- OTC pay-before-service, clinical cancellation, discharge/departure exception, dan room charge;
- patient settlement, payer allocation primary/excess, AR, AP dokter, serta post-final adjustment;
- cashier shift/register, authorization, audit, notification, dan reporting.

Gate tidak merancang entity, schema, endpoint, UI, migration, atau implementation task.

## 2. Evidence dan authority

| Evidence | Authority | Gate treatment |
| --- | --- | --- |
| Approved decision contract revision `0.2` | Product/Domain Owner untuk target business behavior | `CONFIRMED`; authority utama untuk apa yang harus dibangun |
| Current V2 capability map revision `0.2` | Current implementation evidence | `CONFIRMED` untuk kondisi as-is; tidak mengganti target requirement |
| `ServiceBilling.zip` evidence revision `0.1` | Legacy/reference evidence | Dipakai untuk gap/conflict detection; tidak menjadi policy target |
| Inpatient reference observations yang sudah dicatat | `REFERENCE_ONLY` | Hanya provenance/gap check; keputusan lokal revision `0.2` mengungguli |

Legacy auto write-off, bulk paid flag, doctor FoC, excess fallback, invoice-on-payment, dan room
`ceil(duration)` tidak diadopsi ketika bertentangan dengan approved decisions.

## 3. Completeness findings — 18 dimensions

| ID | Dimension | Current finding | Evidence status | Gap impact |
| --- | --- | --- | --- | --- |
| 01 | Purpose | Invoice menyatukan seluruh konsekuensi finansial encounter sampai settlement/finalization dan handoff AR/AP | `CONFIRMED` | Tidak ada blocker |
| 02 | Actors | Kasir, Billing, Finance/AR, dokter/unit, Kepala Kasir, clinical substitute, producer, payer, AP owner, dan debtor exception terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 03 | Trigger / Prerequisites | Billable event per producer, OTC payment gate, finalization prerequisites, departure exception, dan room occupancy source terdefinisi | `CONFIRMED` | Producer implementation tetap dependency teknis, bukan requirement blocker |
| 04 | Main Flow | Charge capture, recalculation, deposit/payment, finalization, AR/AP, shift, dan departure normal mempunyai urutan bisnis | `CONFIRMED` | Tidak ada blocker |
| 05 | Alternate / Exception Flow | Emergency OTC conversion, death/transfer/DAMA, partial tender/refund, timeout, write-off/reversal, rejected claim, variance, dan post-final correction terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 06 | Minimum Data | Encounter, source tuple, calculation provenance, actor/reason, tender/reference, debtor, occupancy, effective policy, due date, correlation/idempotency tersedia sebagai requirement | `CONFIRMED` | Physical representation milik domain design |
| 07 | Business Rules / Validation | Uniqueness, lock rules, coverage caps, tax basis, discount scope, admin-fee dedupe, room-policy effective date, dan no-overallocation terdefinisi | `CONFIRMED` | Nilai parameter tertentu configurable |
| 08 | Status / State Transition | Invoice, tender, reconciliation, refund pending, write-off settlement, shift, AR/AP readiness, dan adjustment semantics cukup untuk domain modeling | `CONFIRMED` | Nama enum teknis bukan keputusan gate |
| 09 | Role / Authorization | Read scope, mutation/approval separation, delegated cancellation, Finance authority, dan cashier supervisory authority terdefinisi | `CONFIRMED` | Permission-code mapping adalah design/implementation concern |
| 10 | Module Dependency | Registration, producers, lab, radiology, pharmacy, inpatient/bed, tariff/coverage, Finance/AR, AP/payroll, gateway, auth, audit/notification teridentifikasi | `CONFIRMED` | Dependency capability dapat memengaruhi delivery sequencing |
| 11 | Internal / External Integration | Source charge, payment provider, payer contract/claim, AR/AP adjustment, dan occupancy boundaries terdefinisi secara bisnis | `CONFIRMED` | Provider-specific protocol dapat menjadi later slice |
| 12 | Outcome | Patient settlement, invoice final/closed, AR per debtor, AP basis/readiness, refundable credit, serta reconciled shift terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 13 | Cancellation / Correction | Void tanpa delete, substitute cancellation, proportional refund, write-off reversal, immutable final adjustment, dan occupancy correction terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 14 | Audit / History | Actor, authority, timestamp, reason, before/after, source, correlation, version, payment, approval, dan adjustment harus immutable/traceable | `CONFIRMED` | Retention/export detail non-blocking |
| 15 | Notification | Event candidates untuk approval, pending reconciliation, refund failure/completion, variance, dan capture failure diketahui | `PROPOSED` | `NON_BLOCKING_STANDARD`; audience/channel/SLA belum approved |
| 16 | Billing / Charge Impact | Seluruh create/change/void/refund/write-off/tax/coverage/AR/AP consequence terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 17 | Clinical Safety Impact | Emergency OTC tidak ditahan, Billing tidak mengubah clinical truth, performed guard dan exceptional departure terdefinisi | `CONFIRMED` | Tidak ada blocker |
| 18 | Reporting / Traceability | Patient statement, deposit/payment ledger, exception/reconciliation, AR/AP posting, aging, dan shift variance facts diperlukan | `CONFIRMED` + `PROPOSED` presentation | `NON_BLOCKING_STANDARD` untuk format/export/retention |

## 4. Confirmed requirement set

- Satu encounter mempunyai satu invoice; `(SourceDomain, SourceDetailId)` unik dan idempotent.
- Current producer tetap owner clinical state; Billing owner financial representation.
- Rajal/OTC lock saat checkout; progress payment rawat inap tidak mengunci invoice.
- OTC unsettled tidak release service; emergency menjadi encounter IGD/emergency.
- Normal departure mensyaratkan settlement; death, emergency transfer, dan DAMA/APS tidak ditahan
  dan menghasilkan AR kepada debtor sah bila masih outstanding.
- Primary coverage diproses lebih dahulu; excess mengevaluasi residual dengan kontraknya sendiri;
  AR final dipisah per debtor dan rejected claim tidak otomatis pindah ke pasien.
- Tax memakai effective-dated master, post-item-discount basis, patient/payer allocation, decimal,
  dan consistent rounding; tidak ada PPN global/hardcoded.
- Occupancy timeline menjadi source kamar; charge policy configurable/effective-dated dan correction
  memakai adjustment tanpa overwrite history.
- Full write-off menghasilkan `SETTLED_BY_WRITE_OFF`, partial mengurangi balance, dan reversal
  membuka kembali AR melalui compensating entry.
- Final posting immutable; post-final correction menghasilkan idempotent AR/AP debit/credit
  adjustment serta patient outstanding/refundable credit.
- AP dokter lahir saat invoice final dan readiness mengikuti self-pay/insured settlement policy.
- Shift/register menyimpan opening, system cash, physical cash, ending, variance, handover, review,
  reopen authority, dan late noncash settlement tanpa memutasi closed physical cash.
- `InvoiceDate` berasal dari finalization; self-pay due immediately, payer due mengikuti contract;
  AR age dimulai saat posting dan overdue dihitung terhadap `DueDate`.

## 5. Gap register

### 5.1 Configurable defaults

| Item | Classification | Owner sebelum affected delivery task selesai |
| --- | --- | --- |
| Invoice number format/reset/scope | `CONFIGURABLE_DEFAULT` | Billing/Finance + engineering governance |
| Currency dan display precision | `CONFIGURABLE_DEFAULT` | Finance |
| Exact rounding mode konsisten | `CONFIGURABLE_DEFAULT` | Finance/Tax |
| Shift variance tolerance | `CONFIGURABLE_DEFAULT` | Kepala Kasir/Finance |
| Master-discount thresholds/limits | `CONFIGURABLE_DEFAULT` | Finance |
| AP readiness parameters per payer | `CONFIGURABLE_DEFAULT` | AP/Finance owner |
| Room charging policy values per hospital/payer | `CONFIGURABLE_DEFAULT` | Inpatient + Billing/Finance |

### 5.2 Non-blocking standards

| Item | Classification | Treatment |
| --- | --- | --- |
| Notification audience/channel/template/retry/SLA | `PROPOSED`, `NON_BLOCKING_STANDARD` | Preserve event facts; finalize delivery slice later |
| Report layout/filter/export/retention | `PROPOSED`, `NON_BLOCKING_STANDARD` | Preserve underlying traceability; presentation later |
| Provider-specific payment protocol and reconciliation interval | `PROPOSED`, `NON_BLOCKING_STANDARD` | Provider-neutral domain design may proceed |

Tidak ada `MISSING` atau `CONFLICT` yang berdampak `BLOCKING` pada target domain design.

## 6. Resolved Decision Log

| Decision ID | Confirmed resolution | Status |
| --- | --- | --- |
| `BKC-DEC-031` | Financial read scope dipisah untuk Kasir, Billing, Finance/AR, dokter/unit, dan Kepala Kasir | `APPROVED` |
| `BKC-DEC-032` | OTC tidak memiliki payment bypass; emergency dikonversi ke IGD/emergency | `APPROVED` |
| `BKC-DEC-033` | Normal departure setelah settlement; death/transfer/DAMA dapat departure dengan lawful-debtor AR | `APPROVED` |
| `BKC-DEC-034` | Substitute cancellation memakai same-profession/unit authority, approval, reason, dan not-performed confirmation | `APPROVED` |
| `BKC-DEC-035` | Refund split proporsional ke tender asal; partial failure tetap `REFUND_PENDING`; fallback oleh Finance | `APPROVED` |
| `BKC-DEC-036` | Write-off settlement berbeda dari paid; partial dan reversal memakai balance/compensating entry | `APPROVED` |
| `BKC-DEC-037` | Doctor-share source, discount scope, AP birth, dan readiness self-pay/insured terdefinisi | `APPROVED` |
| `BKC-DEC-038` | Shift open/close/variance/reopen/handover/late-noncash lifecycle terdefinisi | `APPROVED` |
| `BKC-DEC-039` | Stable source tuple dan billable timing per producer terdefinisi | `APPROVED` |
| `BKC-DEC-040` | Final adjustment immutable dan dipropagasi sebagai idempotent AR/AP debit/credit | `APPROVED` |
| `BKC-DEC-041` | Tax master effective-dated, basis, allocation, dan rounding contract terdefinisi | `APPROVED` |
| `BKC-DEC-042` | Primary/excess sequencing, cap, residual, debtor AR, dan rejected-claim rule terdefinisi | `APPROVED` |
| `BKC-DEC-043` | Occupancy source, configurable room policy, transfer, leave, dan correction terdefinisi | `APPROVED` |
| `BKC-DEC-044` | Invoice/due/payment date serta AR age/overdue semantics terdefinisi | `APPROVED` |

## 7. Readiness by capability slice

| Slice | Readiness | Notes/dependency |
| --- | --- | --- |
| Core invoice dan idempotent billing-item ledger | `READY_FOR_DOMAIN_DESIGN` | Producer tetap external owner |
| Pricing, coverage, tax, calculation version, snapshot, dan lock | `READY_FOR_DOMAIN_DESIGN` | Exact policy values configurable |
| Administration fee dan transfer replacement | `READY_FOR_DOMAIN_DESIGN` | Cross-encounter/date invariant required |
| Deposit, top-up, progress/final allocation, refundable credit | `READY_FOR_DOMAIN_DESIGN` | Inpatient integration dependency |
| Split tender, reconciliation, refund, dan reversal | `READY_FOR_DOMAIN_DESIGN` | Provider-neutral design; provider adapter later |
| Write-off dan patient-settlement effect | `READY_FOR_DOMAIN_DESIGN` | Finance maker/approver invariant |
| OTC pay-before-service dan emergency conversion | `READY_FOR_DOMAIN_DESIGN` | IGD/Registration integration boundary |
| Clinical cancellation dan substitute authority | `READY_FOR_DOMAIN_DESIGN` | Source-domain authority preserved |
| Doctor discount dan AP creation/readiness | `READY_FOR_DOMAIN_DESIGN` | AP/payroll remains downstream owner |
| AR per debtor, aging, claim rejection, dan final adjustment | `READY_FOR_DOMAIN_DESIGN` | AR remains downstream owner |
| Inpatient room charge | `READY_FOR_DOMAIN_DESIGN` | Occupancy source + effective policy boundary |
| Discharge/departure/final-close orchestration | `READY_FOR_DOMAIN_DESIGN` | Clinical discharge remains separate event |
| Cashier shift/register dan variance | `READY_FOR_DOMAIN_DESIGN` | Exact tolerance configurable |
| Financial authorization dan audit/history | `READY_FOR_DOMAIN_DESIGN` | Permission-code mapping downstream |
| Notification/reporting facts | `READY_FOR_DOMAIN_DESIGN` | Presentation/channel remain non-blocking later slices |

## 8. What may proceed

Seluruh capability slice pada bagian 7 dapat diserahkan ke `hospital-domain-architect`. Handoff
harus tetap:

- memisahkan clinical ownership dari financial ownership;
- mempertahankan current-V2 capability classifications dan legacy conflicts;
- tidak mengubah configurable defaults menjadi hardcoded policy;
- tidak mendesain provider-specific transport tanpa contract provider;
- tidak menganggap domain readiness sebagai implementation/write authority.

## 9. What must stop

Tidak ada slice yang berhenti karena keputusan bisnis belum tersedia. Hal berikut tetap tidak boleh
dilakukan oleh gate ini:

- membuat entity/schema/API/UI/migration atau implementation task;
- menjalankan database atau provider integration;
- menganggap current source sudah memiliki transaction Billing/Kasir;
- memulai delivery planning sebelum domain architecture dan business-module blueprint diperbarui
  serta memperoleh approval desain.

## 10. Handoff to `hospital-domain-architect`

- `blueprint_id`: `BIL-CASH-001`.
- Blueprint lifecycle revision: `0.3`.
- `decision_revision`: approved `0.2`.
- `input_revision_hash`: disinkronkan pada manifest setelah assessment ini divalidasi.
- Backend/frontend SHA: `e6f6ecba1537783ea2eb379ac12cc97790707303` /
  `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151`.
- Current phase: `BIL-CASH-PH-004-DOMAIN-ARCHITECTURE` setelah status update.
- Requirement readiness: `READY_FOR_DOMAIN_DESIGN`.
- Evidence status: target decisions `CONFIRMED`; legacy conflict tetap compatibility/migration concern;
  notification/report presentation `PROPOSED/NON_BLOCKING_STANDARD`.
- Blocking decision IDs: none.
- Dependency owners: Registration/Encounter, clinical producers, Lab, Radiology, Pharmacy,
  Inpatient/Bed, Tariff/Coverage, Finance/AR, AP/Payroll, Payment Provider, Auth, Audit/Notification.
- Baseline provenance: prior Inpatient observations remain `REFERENCE_ONLY`; canonical
  Billing/Cashier/Payer reference was `NOT_YET_AVAILABLE` in the recorded assessment.
- Expected output: revised bounded contexts, concepts, aggregates, relationships, lifecycle,
  authorization, audit, integration, billing/clinical-safety impact, traceability, and explicit
  configurable/non-blocking boundaries without implementation design.

### Gate conclusion

Billing/Kasir revision `0.3` is `READY_FOR_DOMAIN_DESIGN`. Semua former blocking decisions
`BKC-DEC-031`–`044` telah approved. Domain architecture revision `0.2` tetap stale sampai
direvisi untuk mengintegrasikan keputusan tersebut.
