# Roadmap Frontend — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Roadmap revision | `1` |
| Status roadmap | `APPROVED_FOR_EXECUTION` — approval seluruh task diberikan; handoff dan wewenang tulis tetap wajib saat eksekusi |
| Scope | Core internal/manual |
| Backend prerequisite | `RJ-BIL-BE-001` contract backend terkunci dan tersedia untuk consumer |
| Frontend source SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Approval | `OWNER_APPROVED` pada `2026-08-21` |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE` |
| Task approval | `RJ-BIL-FE-001` s.d. `RJ-BIL-FE-007` disetujui pengguna pada `2026-08-21` |
| Status seluruh task revision 1 | `APPROVED_FOR_EXECUTION` |
| IMPLEMENTATION_AUTHORITY | `NOT_GRANTED` |
| BUILDER_EXECUTION | `NOT_AUTHORIZED` |

## Aturan UI

Route, layout, warna, sidebar, dan komponen visual yang belum disetujui tetap `DEV_DISCRETION`.
Frontend tidak boleh menjadi source of truth financial dan tidak boleh mengaktifkan external
adapter.

## Task frontend

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `RJ-BIL-FE-001` | Menyediakan consumer read-only Folio dan milestone status | `RJ-BIL-GATE-DEC-001`, `008`; `RJ-BIL-CAP-020` | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` | Axios/Redux/loading-error convention existing | Query folio by encounter/id, charge/component display, processing outcome, refresh, stale guard; UI detail `DEV_DISCRETION` | `RJ-BIL-BE-001`, frontend API authority | `OutcomeUnknown` bukan failed/success; 404 bukan empty; 409 menampilkan conflict/reload; UUID bukan satu-satunya label | Component/API/mock test, accessibility check | Frontend authority | Contract consumer tested, no financial mutation |
| `RJ-BIL-FE-002` | Menampilkan clinical milestone dan financial boundary | `RJ-BIL-GATE-DEC-001`, `003`, `004`, `007` | State `RJ-BIL-STATE-001@1.0.0` | Doctor queue prescription/procedure existing | Bedakan order, fulfillment, milestone, charge, projection; Lab/Radiology status hanya bila endpoint tersedia | `RJ-BIL-BE-002/003/004` | UI tidak menampilkan order sebagai Paid; status source/version terlihat; stale response ditolak | UI state/error/accessibility test | Clinical/Pharmacy/Frontend | Boundary reviewed by domain owners |
| `RJ-BIL-FE-003` | Menampilkan allocation dan patient responsibility | `RJ-BIL-GATE-DEC-002`; `RJ-BIL-CAP-020` | API/Validation `RJ-BIL-API-001@1.0.0` | Existing payer/encounter references | Read-only allocation version, payer nominal, residual, reason, history; no overwrite | `RJ-BIL-BE-005` | Total allocation + patient responsibility = net eligible; superseded version tetap dapat dilihat | Component/property/API test | Billing/Payer/Frontend | Multi-payer display reviewed; no new payer decision in UI |
| `RJ-BIL-FE-004` | Menyediakan form financial action dan approval status | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `015` | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` | Existing permission/action patterns | Submit reason/amount, pending approval, checker decision, self-approval error, audit reference | `RJ-BIL-BE-006`, Workflow/Security | Pending request tidak mengubah canonical charge; self-approval tampil sebagai error; retry tidak duplicate | Form/API/accessibility/security test | Finance/Security/Frontend | Permission matrix consumed exactly; no hidden bypass |
| `RJ-BIL-FE-005` | Menampilkan reconciliation/outage state | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017`, `021` | Integration `RJ-BIL-INT-001@1.0.0` | Existing loading/error/retry patterns | OutcomeUnknown, pending reconciliation, owner/next action, recovery refresh; no blind retry | `RJ-BIL-BE-007` | Timeout mempertahankan identity; failed component bukan zero; close block terlihat | Failure/mock/recovery/accessibility test | Billing/Integration/Frontend | Recovery UX approved; no auto-resolution |
| `RJ-BIL-FE-006` | Menampilkan manual payer/claim/settlement status | `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-022` | Integration `RJ-BIL-INT-001@1.0.0` | Manual workflow status components | Label `ManualOperator`, claim/payment separation, adapter inactive indicator | `RJ-BIL-BE-008` | Approved claim tetap PaymentPending; external adapter tidak memiliki activation action | Component/API/security test | Payer/Finance/Frontend | Manual workflow consumer reviewed |
| `RJ-BIL-FE-007` | Menutup coverage gap dan regression UI | `RJ-BIL-CAP-021` | Acceptance matrix | Existing frontend test conventions | Test duplicate submit, stale response, permission, privacy, responsive/accessibility | FE-001..006 | Critical UI acceptance memiliki evidence atau gap owner | Test report and traceability review | QA/frontend authority | Coverage report and known gaps |

## Dependency sequence

`FE-001 → FE-002/FE-003 → FE-004/FE-005/FE-006 → FE-007`.

FE work may run in parallel with backend only after the specific backend contract is approved,
versioned, hash-locked, and available as consumer fixture. No frontend task may activate
`RJ-BIL-DEP-009`.
