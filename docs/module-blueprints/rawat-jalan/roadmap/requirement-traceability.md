# Requirement Traceability — Rawat Jalan Billing Roadmap

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
module_slug: rawat-jalan
roadmap_revision: 1
status: APPROVED_FOR_EXECUTION
approval: "OWNER_APPROVED untuk planning dan seluruh task pada 2026-08-21; handoff dan writer authority tetap wajib"
contract_versions:
  - "RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)"
input_revisions:
  blueprint-manifest.md: 21
  00-interview-decisions.md: 14
implementation_authority:
  granted: [RJ-BIL-BE-001, RJ-BIL-BE-002, RJ-BIL-BE-003, RJ-BIL-BE-006, RJ-BIL-BE-007]
  granted_frontend: "seluruh roadmap frontend sejak RJ-BIL-DEC-013 (2026-08-28)"
builder_execution:
  executed: [RJ-BIL-BE-001, RJ-BIL-BE-002, RJ-BIL-BE-003, RJ-BIL-BE-006, RJ-BIL-BE-007]
  executed_frontend: [RJ-BIL-FE-001, RJ-BIL-FE-002]
  authorized_frontend: [RJ-BIL-FE-004, RJ-BIL-FE-005]
  not_authorized: [RJ-BIL-BE-004, RJ-BIL-BE-005, RJ-BIL-BE-008, RJ-BIL-BE-009, RJ-BIL-FE-003, RJ-BIL-FE-006, RJ-BIL-FE-007]
external_adapter: "RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE"
progress:
  backend: "5 dari 9 task selesai"
  frontend: "2 dari 7 task selesai; bagian Radiologi RJ-BIL-FE-002 tetap terblokir"
test_evidence:
  backend: "157 lulus, 0 gagal per 2026-08-27"
  frontend: "88 lulus, 0 gagal per 2026-08-28; build next exit 0. Render test belum mungkin pada harness node --test"
last_updated: "2026-08-28"
```

---

## 1. Cara membaca tabel ini

Satu baris menjawab satu pertanyaan: **keputusan ini sudah menjadi apa, dan buktinya di mana.**

Kolom **Keadaan** menilai **build**, bukan restu governance. `SELESAI` berarti code ada, build
lulus, dan test acceptance-nya lulus — **bukan** berarti boleh dipakai untuk pasien sungguhan.
Kolom **Governance** memisahkan keduanya, dan sembilan dari sembilan baris masih punya sesuatu
yang terbuka di sana.

| Tanda | Artinya |
| :---: | --- |
| ✅ | Task selesai dan terbukti |
| ⛔ | Task terblokir |
| — | Task belum dimulai; tidak terblokir |

---

## 2. Matriks traceability

| Requirement/decision | Desain/kontrak | Backend | Frontend | Keadaan | Governance |
|---|---|---|---|---|---|
| `RJ-BIL-GATE-DEC-001` ownership financial | Domain architecture §3–5; API/permission contract | ✅ `BE-001`, ✅ `BE-002`, ✅ `BE-006` | ✅ `FE-001`, ✅ `FE-002`; — `FE-004` | **Backend selesai.** Tidak ada clinical endpoint yang menetapkan status finansial; terbukti test source-of-truth | Sign-off Finance dan Security/Privacy `OPEN` |
| `RJ-BIL-GATE-DEC-002` multi-payer allocation | Allocation aggregate; API/validation | ⛔ `BE-005` | ⛔ `FE-003` | **Terblokir.** `RJ-BIL-CONFLICT-001` `CONFIRMED`; bentuk allocation belum dapat dirancang | Direksi Rumah Sakit; jawaban `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005` |
| `RJ-BIL-GATE-DEC-003` Laboratory milestone | Lab boundary/lifecycle | ✅ `BE-003` | ✅ `FE-002` | **Backend selesai.** `Accepted` menghasilkan eligibility; riwayat specimen terbukti; `39` test berbasis database lulus | Sign-off Lab dan Clinical Governance `OPEN`; `QBE-MOD-002` `CLOSED` |
| `RJ-BIL-GATE-DEC-004` Radiology safety/acquisition | Radiology boundary/lifecycle | ⛔ `BE-004` | ⛔ bagian Radiologi `FE-002` | **Terblokir.** Area `RadiologyManagement` belum ada pada source; greenfield penuh | Owner `RadiologyManagement` **belum ditunjuk**; prefix `Rad` masih `PLANNED` |
| `RJ-BIL-GATE-DEC-005` actual consumption | Charge component/rule boundary | ✅ `BE-001`, ✅ `BE-002`, ✅ `BE-003`; ⛔ `BE-004` | ✅ `FE-001`, ✅ `FE-002` | **Sebagian.** Resep, tindakan, dan Lab terbukti; Radiologi menunggu `BE-004` | Clinical Governance `OPEN` |
| `RJ-BIL-GATE-DEC-006` financial governance | Financial Action/Approval | ✅ `BE-006` | — `FE-004`, kini terbuka karena `FE-002` selesai | **Backend selesai.** Maker-checker, penolakan self-approval, dan gerbang penutupan terbukti lewat `46` test | `locked-draft`. Sign-off Finance, Security/Privacy, delegated executive `OPEN`. `RJ-BIL-OQ-004` belum ditetapkan |
| `RJ-BIL-GATE-DEC-007` Pharmacy ownership | Projection/clinical fact boundary | ✅ `BE-002` | ✅ `FE-002` | **Backend selesai.** `Paid` ≠ `Dispensed`; projection read-only terbukti. `RJ-BIL-CONFLICT-006` `CLOSED` | `RJ-BIL-BE-002-BLOCKER-001` terbuka — kebijakan farmasi, tidak menahan task |
| `RJ-BIL-GATE-DEC-008` reliability/reconciliation | Processing/Reconciliation | ✅ `BE-001`, ✅ `BE-007`; ⛔ `BE-009` | ✅ `FE-001`; — `FE-005`; ⛔ `FE-007` | **Sebagian.** Replay, timeout, partial, dan laporan pemulihan terbukti lewat `37` test; penutupan coverage gap menunggu `BE-009` | Sign-off Billing/Finance dan Integration `OPEN` |
| `RJ-BIL-GATE-DEC-009` payer/manual release scope | Manual claim/integration contract | ⛔ `BE-008` | ⛔ `FE-006` | **Terblokir.** Menunggu `BE-005`, ditambah `RJ-BIL-OQ-007` | Payer, Finance, dan Integration owner |

---

## 3. Coverage gap

| Gap | Keadaan per `2026-08-28` | Dampak | Owner/tindakan |
|---|---|---|---|
| ~~Tidak ada test project maupun evidence pada snapshot audit~~ | **DITUTUP.** `Tests/QuilvianSystemBackend.BillingTests/` berdiri; `157` test lulus, `0` gagal | — | — |
| Cakupan test belum menyentuh tiga skenario | **Terbuka.** Multi-payer allocation (`BE-005`), klaim dan settlement manual (`BE-008`), dan acquisition Radiologi (`BE-004`) belum diuji karena task-nya belum dikerjakan | Ketiganya tetap menjadi cakupan `RJ-BIL-BE-009` | QA bersama builder |
| Ambang approval belum bernilai final | **Terbuka.** `RJ-BIL-OQ-004` belum ditetapkan; `MstBillingApprovalPolicy` sengaja kosong tanpa seed karena `RJ-BIL-GATE-DEC-006` melarang default approver/threshold | Tindakan yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration` — **fail-closed yang disengaja**, bukan kerusakan | Finance |
| SOP safety Radiologi belum ada | **Terbuka.** Tidak ada capability Radiologi operasional pada source | `BE-004` tidak boleh dimulai; SOP tidak boleh dikarang | Owner `RadiologyManagement` — belum ditunjuk |
| Working tree Billing Operational belum di-commit | **Terbuka.** `BE-002`, `BE-003`, `BE-006`, `BE-007`, dan remediasi penamaan QBE | Bukti bersifat provisional; builder wajib preflight ulang | Backend owner |
| Kontrak dan UAT adapter eksternal belum tersedia | **Terbuka.** `RJ-BIL-DEP-009` berstatus `UNKNOWN` | Adapter tetap `INACTIVE`; hanya alur manual yang berjalan | Payer dan Integration |
| Kolom pembayaran warisan modul Farmasi masih terbaca dari API | **Terbuka.** `PrescriptionResponse.paymentStatus` masih mengirim nilai `Lunas` walau `RJ-BIL-BE-002` sudah menghapus endpoint yang menulisnya | Layar `RJ-BIL-FE-002` menjinakkannya untuk dirinya sendiri; layar lain yang membaca `prescriptions/{id}` tanpa peringatan serupa akan mengulangi kesalahan yang sama | Pemilik Farmasi bersama Billing |
| `GET /lab-orders` tidak punya satu pun parameter penyaring | **Terbuka.** Tidak ada `encounterId`, tidak ada paginasi | `RJ-BIL-FE-002` terpaksa menyaring per kunjungan di sisi klien; berjalan sekarang, berhenti berjalan seiring pertumbuhan data | Backend owner Laboratorium |
| UI visual authority belum dikunci | **Terbuka.** Frontend authority `OPEN` | Detail visual tetap `DEV_DISCRETION` | Frontend authority |
| ~~Wewenang tulis frontend belum diberikan~~ | **Tertutup `2026-08-28` oleh `RJ-BIL-DEC-013`.** `IMPLEMENTATION_AUTHORITY` frontend menjadi `GRANTED`; `BUILDER_EXECUTION` `AUTHORIZED` untuk `FE-001`, `FE-002` bagian Lab, `FE-004`, dan `FE-005` | Keempat task itu boleh dimulai. `FE-003`, `FE-006`, dan `FE-007` tetap `NOT_AUTHORIZED` karena endpoint-nya belum ada | Pemilik blueprint |

---

## 4. Yang menahan penyelesaian modul

Empat task backend dan lima task frontend belum selesai. **Tidak satu pun terhenti karena
kesulitan teknis.** Setelah `RJ-BIL-DEC-013`, tinggal dua hal yang hanya dapat dibuka pemilik:

| Yang dibutuhkan | Membuka |
|---|---|
| Jawaban `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005` pada [owner-decision-request-RJ-BIL-001.md](../owner-decision-request-RJ-BIL-001.md) | `RJ-BIL-BE-005`, lalu `RJ-BIL-BE-008`, lalu `RJ-BIL-FE-003` dan `FE-006` |
| Penunjukan owner `RadiologyManagement` dan kenaikan prefix `Rad` ke `ACTIVE` | `RJ-BIL-BE-004`, lalu bagian Radiologi `RJ-BIL-FE-002` |
| ~~Wewenang tulis frontend~~ | **Sudah diberikan `2026-08-28` lewat `RJ-BIL-DEC-013`.** `RJ-BIL-FE-001`, `FE-002` bagian Lab, `FE-004`, dan `FE-005` kini boleh dikerjakan |

| Requirement/decision | Desain/contract | Backend task | Frontend task | Acceptance evidence | Status |
|---|---|---|---|---|---|
| `RJ-BIL-GATE-DEC-001` ownership financial | Domain architecture §3-5; API/permission contracts | `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-006` | `RJ-BIL-FE-001`, `RJ-BIL-FE-002`, `RJ-BIL-FE-004` | No clinical endpoint authoritative financial; source-of-truth test | Approved for execution |
| `RJ-BIL-GATE-DEC-002` multi-payer allocation | Allocation aggregate; API/validation | `RJ-BIL-BE-005` | `RJ-BIL-FE-003` | Allocation equation and version history test | Approved for execution |
| `RJ-BIL-GATE-DEC-003` Laboratory milestone | Lab boundary/lifecycle | `RJ-BIL-BE-003` | `RJ-BIL-FE-002` | Accepted eligibility; specimen history test | Approved for execution |
| `RJ-BIL-GATE-DEC-004` Radiology safety/acquisition | Radiology boundary/lifecycle | `RJ-BIL-BE-004` | `RJ-BIL-FE-002` | Safety gate and performed acquisition test | Approved for execution |
| `RJ-BIL-GATE-DEC-005` actual consumption | Charge component/rule boundary | `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-004` | `RJ-BIL-FE-001`, `RJ-BIL-FE-002` | Rule missing → review; quantity calculation test | Approved for execution |
| `RJ-BIL-GATE-DEC-006` financial governance | Financial Action/Approval | `RJ-BIL-BE-006` | `RJ-BIL-FE-004` | Maker-checker/self-approval/close gate test | Approved for execution |
| `RJ-BIL-GATE-DEC-007` Pharmacy ownership | Projection/clinical fact boundary | `RJ-BIL-BE-002` | `RJ-BIL-FE-002` | Paid != Dispensed; read-only projection test | Approved for execution |
| `RJ-BIL-GATE-DEC-008` reliability/reconciliation | Processing/Reconciliation | `RJ-BIL-BE-001`, `RJ-BIL-BE-007`, `RJ-BIL-BE-009` | `RJ-BIL-FE-001`, `RJ-BIL-FE-005`, `RJ-BIL-FE-007` | Replay, timeout, partial, recovery report | Approved for execution |
| `RJ-BIL-GATE-DEC-009` payer/manual release scope | Manual claim/integration contract | `RJ-BIL-BE-008` | `RJ-BIL-FE-006` | Manual label, adapter inactive, payment separation | Approved for execution |

## Coverage gap

| Gap | Dampak | Owner/tindakan |
|---|---|---|
| Tidak ada test project/evidence pada snapshot audited | Semua task harus membuat evidence test sebagai DoD | QA + builder |
| Threshold approval, tariff rule, SOP safety belum bernilai final | High-risk fail-closed; partial charge review; safety config gate | Finance/Clinical Governance |
| Working tree Billing Operational belum committed | Evidence provisional; builder wajib preflight ulang | Backend owner |
| External adapter contract/UAT belum tersedia | Adapter tetap inactive; manual flow saja | Payer/Integration |
| UI visual authority belum dikunci | Detail visual tetap `DEV_DISCRETION` | Frontend authority |
