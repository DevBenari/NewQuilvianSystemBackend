# Roadmap Backend — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Roadmap revision | `1` |
| Status roadmap | `APPROVED_FOR_EXECUTION` — approval seluruh task diberikan; handoff dan wewenang tulis tetap wajib saat eksekusi |
| Scope | Core internal/manual |
| Contract | `RJ-BIL-CONTRACT-001@1.0.0` |
| Decision revision | `10` |
| Domain architecture | revision `1`, core independen dari `DOMAIN_ARCHITECTURE_PARTIAL` |
| Backend source SHA | `36456ead5d8d116e5631aef859df3d55b0ec7e81` cabang `sukmagp` |
| Frontend source SHA | `29422c83eaf6fd231cbb72f2ba04e306367934e1` cabang `QuilvianDevV2` |
| Approval | `OWNER_APPROVED` pada `2026-08-21` |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE` |
| Task approval | `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-009` disetujui pengguna pada `2026-08-21` |
| Status seluruh task revision 1 | `APPROVED_FOR_EXECUTION` |
| IMPLEMENTATION_AUTHORITY | `GRANTED` untuk `RJ-BIL-BE-001` saja |
| BUILDER_EXECUTION | `EXECUTED` untuk `RJ-BIL-BE-001`; task lain `NOT_AUTHORIZED` |
| Progress | `1` dari `9` task backend selesai per `2026-08-24` |

## Progress eksekusi

| Task | Status | Bukti |
|---|---|---|
| `RJ-BIL-BE-001` | `COMPLETE` | [execution-evidence-RJ-BIL-BE-001.md](../execution-evidence-RJ-BIL-BE-001.md) |
| `RJ-BIL-BE-002` | `BLOCKED` — menunggu keputusan owner atas `RJ-BIL-CONFLICT-006` | [owner-decision-request-RJ-BIL-001.md](../owner-decision-request-RJ-BIL-001.md) pertanyaan `1A` dan `1B` |
| `RJ-BIL-BE-005` | `BLOCKED` — menunggu keputusan owner atas `RJ-BIL-CONFLICT-001` | [RJ-BIL-CONFLICT-001-source-audit.md](../RJ-BIL-CONFLICT-001-source-audit.md); pertanyaan `RJ-BIL-OQ-001` s.d. `OQ-007` |
| `RJ-BIL-BE-003`, `RJ-BIL-BE-004` | `NOT_STARTED` — tidak diblokir konflik mana pun | Memerlukan owner Lab atau Radiology beserta Clinical Governance |
| `RJ-BIL-BE-006` s.d. `RJ-BIL-BE-009` | `NOT_STARTED` | Menunggu dependency sequence |

Audit read-only `RJ-BIL-CONFLICT-001` per `2026-08-24` menyimpulkan konflik `CONFIRMED` dengan
source confidence `HIGH`, tanpa memerlukan perubahan code saat ini. Cakupan `RJ-BIL-BE-005` pada
tabel di bawah — allocation multi-payer dan patient responsibility — belum dapat dirancang sebelum
`RJ-BIL-OQ-001`, `OQ-002`, dan `OQ-005` dijawab, karena bentuk allocation-nya ditentukan jawaban
tersebut. `RJ-BIL-BE-006` dan `RJ-BIL-BE-008` terdampak tidak langsung karena keduanya bekerja di
atas hasil allocation.

Migration `20260821033911_AddBillingOperationalBaseline` sudah diterapkan ke database
`QuilvianNewDevTim01` atas otorisasi terpisah yang diberikan pengguna pada `2026-08-21`.
Otorisasi tersebut terbatas pada satu migration itu dan tidak berlaku untuk task berikutnya.

## Aturan eksekusi

Roadmap ini telah disetujui untuk eksekusi task. Builder tetap memerlukan handoff task,
wewenang tulis backend, dan QBE preflight pada waktu eksekusi. Tidak ada task di bawah ini yang memberi
izin migration apply, database mutation, deployment, commit, atau publish.

## Task backend

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `RJ-BIL-BE-001` | Menetapkan baseline dan memperkeras Billing Folio/Charge Operational | `RJ-BIL-GATE-DEC-001`, `008`; `RJ-BIL-CAP-012`, `017` | API/State/Validation `1.0.0` | `BilFolio`, `BilChargeLine`, `BilChargeComponent`, `BilProcessingEffect` working tree | Preflight QBE, configuration path, DbContext, unique/index/concurrency, API validation, audit, migration plan; tidak menjalankan migration | `RJ-BIL-DEP-001`, `006`, `008` | Folio unik per encounter; duplicate key menghasilkan replay; stale version ditolak; tidak ada clinical financial mutation | Build backend, targeted integration test, migration review, permission review | Working tree belum committed; Backend owner | Source/build/test evidence, migration artifact reviewed, no unauthorized DB apply |
| `RJ-BIL-BE-002` | Menyediakan clinical fact handoff yang idempotent untuk Prescription dan Procedure | `RJ-BIL-GATE-DEC-001`, `005`, `008`; `RJ-BIL-CAP-005`, `008` | Integration `RJ-BIL-INT-001@1.0.0` | Prescription/procedure lifecycle existing | Adapter producer fact, stable source/version, milestone mapping, retry/outcome contract; deprecate financial authority legacy secara bertahap | `RJ-BIL-BE-001`, Pharmacy, Clinical | Clinical endpoint tidak menetapkan `Paid`; retry tidak menggandakan charge; correction memakai version baru | Contract test + replay/concurrency test + audit evidence | Pharmacy/Clinical/Billing owners | Producer contract reviewed, tests pass, compatibility notes recorded |
| `RJ-BIL-BE-003` | Menyediakan Lab milestone minimal sampai `Accepted` | `RJ-BIL-GATE-DEC-003`; `RJ-BIL-CAP-010` | State/Validation `RJ-BIL-STATE-001@1.0.0` | Existing `LabOrder` sebagai start point | Extend lifecycle order/specimen/acceptance boundary dan emit fact; result release tetap scope Lab | `RJ-BIL-BE-001`, Lab owner | `Requested/Collected/Received` tidak menjadi initial charge; `Accepted` menghasilkan eligibility fact; rejected/recollection berhistori | Domain/state test, safety test, integration replay | Lab/Clinical Governance | Lifecycle evidence, acceptance test matrix updated, no invented SOP |
| `RJ-BIL-BE-004` | Menetapkan Radiology operational boundary dan acquisition fact | `RJ-BIL-GATE-DEC-004`; `RJ-BIL-CAP-011` | State/Integration `RJ-BIL-STATE-001@1.0.0`, `RJ-BIL-INT-001@1.0.0` | Shared procedure/tariff/Encounter reference | New Radiology capability design/implementation contract; safety gate, study, repeat/abort, usable acquisition fact; tidak mengaktifkan external RIS/PACS | `RJ-BIL-BE-001`, Radiology owner, Clinical Governance | Acquisition ditolak tanpa identity/safety gate; performed usable study menjadi eligibility; repeat mempertahankan original | Domain/integration/safety test | Radiology/Clinical Governance | Scope/owner/SOP evidence approved; external integration remains inactive |
| `RJ-BIL-BE-005` | Menyediakan allocation multi-payer dan patient responsibility | `RJ-BIL-GATE-DEC-002`; `RJ-BIL-CAP-013` | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` | `EncounterId`, payer reference, tariff snapshot | New allocation version, nominal absolute, residual, payer decision reference, over-allocation guard | `RJ-BIL-BE-001`, Payer owner | Rp1.000.000 dapat menjadi A Rp600.000 + B Rp250.000 + patient Rp150.000; superseding version tidak menimpa histori | Domain/API/property test | Billing/Payer/Finance | Allocation contract, invariants, tests, audit evidence |
| `RJ-BIL-BE-006` | Menyediakan financial action, approval, close/reopen | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `015` | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` | Workflow maker-checker existing | Void/adjustment/reversal/refund/FOC/write-off, approval policy reference, revalidation, close gates; tidak menghapus original charge | `RJ-BIL-BE-001`, Workflow, Finance, Security | Self-approval ditolak; pending approval tidak mengubah state; close ditolak saat reconciliation pending | Authorization/integration/audit test | Finance/Security | Policy/version evidence, SOD test, rollback/replay evidence |
| `RJ-BIL-BE-007` | Menyediakan reconciliation case dan recovery status | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017` | Integration `RJ-BIL-INT-001@1.0.0` | `BilProcessingEffect` provisional | OutcomeUnknown, partial component, dead-letter/review, status query, case owner/SLA, recovery report | `RJ-BIL-BE-001`, Integration owner | Timeout tidak menggandakan charge; failed component visible; folio close blocked sampai case resolved | Failure-injection/recovery/concurrency test | Billing/Integration | Reconciliation contract, report evidence, unresolved cases visible |
| `RJ-BIL-BE-008` | Menyediakan manual payer/claim/settlement workflow internal | `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-022` | Integration `RJ-BIL-INT-001@1.0.0` | Manual operator flow | Authorization, claim, adjudication, settlement status; label `ManualOperator`; external adapter interface only | `RJ-BIL-BE-005`, `007`, Payer/Finance | Claim approved tetap `PaymentPending`; manual outcome tidak disebut external success; rejection mempertahankan charge | Workflow/API/integration test | Payer/Finance/Integration | Manual contract and audit accepted; adapter remains disabled |
| `RJ-BIL-BE-009` | Menutup coverage gap automated verification | `RJ-BIL-GATE-DEC-001..009`; `RJ-BIL-CAP-021` | Acceptance `testing/acceptance-test-matrix.md` | Existing source tests bila tersedia | Add targeted test project/spec for lifecycle, duplicate, allocation, correction, approval, outage | BE-001..008 | Semua acceptance critical memiliki bukti test atau gap owner | Test report and traceability review | QA/domain owners | Coverage report, known gaps assigned, no false DONE |

## Dependency sequence

`BE-001 → BE-002/BE-003/BE-004 → BE-005 → BE-006/BE-007 → BE-008 → BE-009`.

`BE-003` dan `BE-004` dapat berjalan paralel setelah kontrak baseline `BE-001` tersedia.
`RJ-BIL-DEP-009` tidak termasuk sequence dan tetap inactive.

## Handoff builder

Setiap handoff ke `build-module-backend` wajib menyertakan task ID, approval task, contract hash,
source SHA/working tree state, dependency state, QBE preflight, dan bukti acceptance yang diminta.
