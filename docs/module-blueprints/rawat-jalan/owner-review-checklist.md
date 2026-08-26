# Owner Review Checklist — Rawat Jalan Billing Blueprint revision 11

Dokumen ini adalah daftar pemeriksaan untuk approval manusia. Mengisi atau membaca dokumen ini
tidak otomatis mengubah status blueprint menjadi `approved` dan tidak memberi wewenang
implementasi.

## Identitas review

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` |
| Revision | `11` |
| Contract | `RJ-BIL-CONTRACT-001@1.0.0` |
| Requirement decision revision | `10` |
| Domain architecture revision | `1` (`DOMAIN_ARCHITECTURE_PARTIAL`) |
| Scope review | Core internal/manual Rawat Jalan Billing |
| Scope excluded | Aktivasi adapter payer eksternal (`RJ-BIL-DEP-009`) |
| Status saat ini | `OWNER_APPROVED` untuk delivery planning |

## Checklist keputusan

| # | Hal yang harus dikonfirmasi | Bukti | Keputusan owner | Catatan |
|---:|---|---|---|---|
| 1 | Billing menjadi satu-satunya source of truth folio, charge, allocation, patient responsibility, dan financial action | `hospital-domain-architecture.md`, `RJ-BIL-GATE-DEC-001` | `APPROVED` |  |
| 2 | `EncounterId` hanya menjadi correlation/aggregation key dan tidak dimutasi Billing | `RJ-BIL-GATE-DEC-001` | `APPROVED` |  |
| 3 | Core internal/manual boleh dirancang walaupun adapter eksternal belum aktif | `RJ-BIL-GATE-DEC-009` | `APPROVED` | External adapter tetap inactive/out of scope |
| 4 | Clinical cancellation tidak otomatis menentukan void, adjustment, reversal, refund, FOC, atau write-off | `RJ-BIL-GATE-DEC-001`, `005` | `APPROVED` |  |
| 5 | Multi-payer allocation memakai nominal absolut dan menyimpan versi histori | `RJ-BIL-GATE-DEC-002` | `APPROVED` |  |
| 6 | Lab `Accepted` dan Radiology performed acquisition menjadi boundary charge eligibility sesuai rule | `RJ-BIL-GATE-DEC-003`, `004` | `APPROVED` |  |
| 7 | Partial charge tanpa rule aktif masuk `PendingFinancialReview`, bukan memakai persentase tebakan | `RJ-BIL-GATE-DEC-005` | `APPROVED` |  |
| 8 | High-risk financial action memakai maker-checker dan fail-closed bila policy belum tersedia | `RJ-BIL-GATE-DEC-006` | `APPROVED` |  |
| 9 | Pharmacy hanya memiliki clinical/fulfillment truth dan financial projection read-only | `RJ-BIL-GATE-DEC-007` | `APPROVED` |  |
| 10 | Idempotency, `OutcomeUnknown`, partial processing, dan reconciliation menjadi invariant delivery | `RJ-BIL-GATE-DEC-008` | `APPROVED` |  |
| 11 | Manual payer outcome tidak dilabeli sebagai external integration success | `RJ-BIL-GATE-DEC-009` | `APPROVED` |  |
| 12 | UI route/layout/visual detail tetap menunggu UI authority dan tidak mengubah backend invariant | `03-frontend-architecture.md` | `APPROVED` | Detail visual tetap `DEV_DISCRETION` |

## Checklist evidence dan risiko

| Item | Status | Tindakan sebelum planning |
|---|---|---|
| Working tree Billing Operational | `PROVISIONAL` | Builder melakukan QBE preflight, build, test, migration review, dan permission review |
| Billing allocation/financial correction | `MISSING` dari source | Masuk task delivery terpisah; tidak boleh dianggap sudah tersedia |
| External payer adapter | `BLOCKED` | Lengkapi contract/security/sandbox/UAT/reconciliation sebelum activation |
| Threshold, tariff rule, SOP checklist | `CONFIGURATION PENDING` | Owner masing-masing menetapkan nilai; tidak di-hardcode |
| Frontend Billing consumer | `MISSING` pada snapshot audited | Delivery FE harus memakai contract backend yang disetujui |

## Approval record

| Field | Nilai |
|---|---|
| Accountable owner name | `User-provided approval authority` |
| Title/delegation evidence | `User message approval record; formal named delegation remains outside current delivery scope` |
| Decision | `OWNER_APPROVED` |
| Approved scope | `core internal/manual` |
| Approved contract versions | `RJ-BIL-CONTRACT-001@1.0.0` |
| Approval date/time | `2026-08-21` |
| Approval evidence/hash | `User message approval record; external adapter explicitly inactive/out of scope` |

### Hasil yang diperlukan untuk membuka `plan-module-delivery`

Owner perlu memberikan keputusan eksplisit yang menyebut:

`OWNER_APPROVED` untuk blueprint `RJ-BIL-BP-001` revision `11`, scope core internal/manual,
contract `RJ-BIL-CONTRACT-001@1.0.0`, serta pengecualian bahwa external adapter tetap tidak aktif.

Setelah record tersebut tersedia, manifest dapat diperbarui dan roadmap backend/frontend dapat
dibuat. Tanpa record itu, roadmap hanya boleh berupa draft blocked dan tidak boleh dikirim ke
builder.
