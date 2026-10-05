# Requirement Traceability — Rawat Jalan

> Bagian `0` memuat matriks kepemilikan lintas scope. Bagian `1` dan seterusnya adalah
> traceability **scope Billing** (`RJ-BIL`), yang berlabel
> `DOWNSTREAM — NOT PART OF DOCTOR DEFINITION OF DONE`. Traceability scope Dokter ada pada
> [doctor-consultation-roadmap.md](doctor-consultation-roadmap.md) bagian `2.2` dan `4`.

## Metadata

> **Metadata di bawah adalah `HISTORICAL SNAPSHOT` per `2026-08-28` — jangan dipakai sebagai
> status saat ini.** Angka progress, bukti test, dan SHA di dalamnya belum diverifikasi ulang
> terhadap backend `HEAD` `801a4f5` / frontend `HEAD` `baca965`. Bagian `0` adalah satu-satunya
> bagian dokumen ini yang berstatus `CURRENT STATE`.

```yaml
blueprint_id: RJ-BIL-BP-001
module_slug: rawat-jalan
roadmap_revision: 2
snapshot_kind: HISTORICAL_SNAPSHOT
snapshot_observed_at: "2026-08-28"
current_state_section: "0 — matriks kepemilikan, diaudit 2026-08-31"
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

## 0. Kepemilikan — dibaca lebih dulu

> **Scope Dokter: `OWNER_APPROVED` `2026-08-31` dan kontrak `FROZEN`.**
>
> | Governance | Keadaan |
> |---|---|
> | Roadmap `RJ-DOC` revision `4` | `OWNER_APPROVED` — `RJ-DOC-DEC-001` |
> | `RJ-DOC-INT-001` `RJ-DOC-COMPLETION-001@1.0.0` | `COMPLETE / FROZEN` — `RJ-DOC-DEC-006` |
> | `RJ-DOC-INT-002` `RJ-DOC-HANDOFF-001@1.0.0` | `COMPLETE / FROZEN` — `RJ-DOC-DEC-006` |
> | Open question `RJ-DOC-OQ-001` s.d. `OQ-006` | Seluruhnya **tertutup** |
> | `IMPLEMENTATION_AUTHORITY` | **`GRANTED — RJ-DOC-BE-001` dan `RJ-DOC-BE-002`**; task lain `NOT_GRANTED` |
> | `RJ-DOC-BE-001` | ✅ **`COMPLETE`** `2026-08-31` — [laporan](../task/report/backend/RJ-DOC-BE-001.md) |
> | `RJ-DOC-BE-002` | ✅ **`COMPLETE`** `2026-08-31` — [laporan](../task/report/backend/RJ-DOC-BE-002.md) |
>
> Keputusan owner yang mengikat traceability ini: `RJ-DOC-DEC-002` menempatkan Lab dan Radiologi
> sebagai `CONDITIONAL`, bukan mandatory baseline; `RJ-DOC-DEC-003` melarang reopen generik;
> `RJ-DOC-DEC-004` memisahkan *doctor order creation* dari *ancillary execution*; `RJ-DOC-DEC-005`
> membatasi `CompleteImmediately`.

Blueprint `rawat-jalan` memuat **dua scope kepemilikan**. Setiap requirement wajib dapat dijawab
*siapa pemiliknya* dan *apakah ia menahan Definition of Done Dokter*, tanpa membaca dokumen lain.

| `OWNER` | Arti | Roadmap |
|---|---|---|
| `DOCTOR / CLINICAL` | Menghasilkan clinical intent, order, dan fakta klinis; berhenti pada `Selesai Konsultasi` | [doctor-consultation-roadmap.md](doctor-consultation-roadmap.md) — `OWNER_APPROVED` |
| `BILLING` | Menafsirkan fakta klinis menjadi konsekuensi finansial | [backend-roadmap.md](backend-roadmap.md), [frontend-roadmap.md](frontend-roadmap.md) |
| `PHARMACY` | Fulfillment resep setelah finalisasi klinis | Modul Farmasi |
| `LAB` | Lifecycle specimen dan penerimaan pemeriksaan | `RJ-BIL-BE-003` |
| `RADIOLOGY` | Safety gate, acquisition, dan study | `RJ-BIL-BE-004` |
| `SHARED INTEGRATION` | Kontrak dan reliability antar modul | `contracts/integration-contract.md` |

### 0.1 Matriks kepemilikan capability

Kolom `Blocks Doctor DoD` dan `Blocks Billing DoD` adalah inti tabel ini. Keduanya menjawab
pertanyaan yang sebelumnya tidak dapat dijawab blueprint: *apakah butir ini menahan pekerjaan
saya, atau menahan pekerjaan orang lain?*

Dua kolom terakhir memakai terminologi yang dipertegas pada revisi `2`, karena "blocking" ke arah
downstream bukan hal yang sama dengan "blocking" terhadap DoD sendiri:

| Nilai kolom | Artinya |
|---|---|
| `Blocks Doctor DoD` | Butir ini menahan pernyataan *pekerjaan Dokter selesai* |
| `Blocks Billing DoD` | Butir ini menahan pernyataan *pekerjaan Billing selesai* |
| `Blocks downstream <X> readiness` | Output Dokter yang ditunggu Billing. **Bukan** berarti implementasi Billing menahan Dokter — arah ketergantungannya justru sebaliknya |

| Requirement | Capability | Owner | Producer | Consumer | Kelas | Status | Evidence | Dependency | Blocks Doctor DoD | Blocks Billing DoD |
|---|---|---|---|---|---|---|---|---|:---:|:---:|
| `RJ-DOC-CAP-003` | Mulai/lanjutkan konsultasi | `DOCTOR / CLINICAL` | Doctor | Clinical | `MANDATORY` | `COMPLETE` | `DoctorConsultationLifecycleService` | — | **YES** | `NO` |
| `RJ-DOC-CAP-008A` | Prescription — pembuatan/draft clinical order | `DOCTOR / CLINICAL` | Doctor | Pharmacy | `MANDATORY` | `COMPLETE` | `PrescriptionController`; `PrescriptionWorkspaceService` | — | **YES** | `NO` |
| `RJ-DOC-CAP-008B` | Prescription — finalisasi pada penyelesaian konsultasi | `DOCTOR / CLINICAL` | Doctor | Pharmacy, Billing | `MANDATORY` | ✅ `COMPLETE` | `RJ-DOC-BE-001`: finalisasi resep kini benar-benar tercapai dari jalur dokter | — | **TERTUTUP** | — |
| `RJ-DOC-CAP-014` | Validasi authoritative sebelum konsultasi ditutup | `DOCTOR / CLINICAL` | Doctor | Pharmacy, Billing, RM | `MANDATORY` | ✅ `COMPLETE` | `RJ-DOC-BE-002` selesai `2026-08-31`; kedua permukaan memakai validator yang sama, ditambah tiga aturan keutuhan pesanan klinis | — | **TERTUTUP** | — |
| `RJ-BIL-CAP-005` | Charge finansial resep | `BILLING` | Billing | Finance, Cashier | `DOWNSTREAM` | `PARTIAL` | `RJ-BIL-BE-002` | fact resep dari `RJ-DOC-BE-001` | `NO` | **YES** |
| `RJ-DOC-CAP-009` | Procedure clinical order + eksekusi | `DOCTOR / CLINICAL` | Doctor | Billing | `MANDATORY` | `COMPLETE` | `PatientProcedureController` | — | **YES** | `NO` |
| `RJ-BIL-CAP-008` | Tarif dan charge tindakan | `BILLING` | Billing | Finance | `DOWNSTREAM` | `PARTIAL` | `RJ-BIL-BE-002` | fact tindakan — **sudah terbit** | `NO` | **YES** |
| `RJ-DOC-CAP-010` | Lab order dari workspace dokter | `DOCTOR / CLINICAL` | Doctor | Lab | `CONDITIONAL` | `MISSING` (FE) | BE `LabOrderController` ada; consumer frontend nihil | `RJ-DOC-OQ-003` | `CONDITIONAL` | `NO` |
| `RJ-BIL-CAP-010` | Lifecycle specimen dan eligibility Lab | `LAB` | Lab | Billing | `DOWNSTREAM` | `COMPLETE` (BE) | `LabSpecimenService` ada pada `HEAD` | — | `NO` | **YES** |
| `RJ-DOC-CAP-011` | Radiology order dari workspace dokter | `DOCTOR / CLINICAL` | Doctor | Radiology | `CONDITIONAL` | `MISSING` (FE) | BE `RadOrderController` ada; consumer frontend nihil | `RJ-DOC-OQ-003` | `CONDITIONAL` | `NO` |
| `RJ-BIL-CAP-011` | Safety gate, acquisition, study | `RADIOLOGY` | Radiology | Billing | `DOWNSTREAM` | `SOURCE EXISTS — ROADMAP TASK STATUS NEEDS REVERIFICATION` | `RadiologyManagement` `17` berkas + `RadiologySafetyGateTests`; acceptance `RJ-BIL-BE-004` **belum** dinilai ulang | owner `RadiologyManagement` | `NO` | **YES** |
| `RJ-DOC-CAP-015` | `Selesai Konsultasi` ke canonical finalization | `DOCTOR / CLINICAL` | Doctor | Billing, Pharmacy, RM | `MANDATORY` | ✅ `COMPLETE` | `RJ-DOC-BE-001` selesai `2026-08-31`; jalur antrean mendelegasikan ke finalisasi canonical | — | **TERTUTUP** | downstream prescription handoff readiness **terbuka** |
| `RJ-DOC-CAP-018` | Idempotency finalisasi | `DOCTOR / CLINICAL` | Doctor | — | `MANDATORY` | `PARTIAL` | penjaga status TOCTOU | `RJ-DOC-BE-003` | **YES** | `NO` |
| `RJ-DOC-CAP-020` | Completed-state protection | `DOCTOR / CLINICAL` | Doctor | RM | `MANDATORY` | `PARTIAL` | penjaga ada tetapi inert | `RJ-DOC-BE-004` | **YES** | `NO` |
| `RJ-DOC-CAP-023` | Durabilitas dan recoverability producer handoff | `DOCTOR / CLINICAL` | Doctor | Billing | `MANDATORY` | `PARTIAL` | ledger durable; tanpa pembaca ulang | `RJ-DOC-BE-005` | **YES** | `NO` — tetapi **`Blocks downstream reliable consumption readiness: YES`** |
| `RJ-DOC-CAP-030` | Satu canonical completion path | `DOCTOR / CLINICAL` | Doctor | Billing, RM, Registration | `MANDATORY` | ✅ `COMPLETE` | `RJ-DOC-BE-001`: jalur antrean menjadi orkestrasi, `CompleteImmediately` ditutup untuk Rawat Jalan, `EncounterStatus` seragam `ConsultationCompleted` | — | **TERTUTUP** | downstream prescription handoff readiness **terbuka** |
| `RJ-DOC-INV-001` | Kegagalan Billing tidak me-rollback consultation | `SHARED INTEGRATION` | Doctor | Billing | `ARCHITECTURAL INVARIANT` | `VERIFIED` | `ClinicalMilestoneFactProducer:83-89` | — | tidak dihitung | tidak dihitung |
| `RJ-DOC-INV-002` | Clinical endpoint tidak menetapkan status finansial | `SHARED INTEGRATION` | Doctor | Billing | `ARCHITECTURAL INVARIANT` | `VERIFIED` | `PrescriptionWorkflowService.cs:62-63` | — | tidak dihitung | tidak dihitung |
| `RJ-DOC-INV-003` | Clinical module tidak menghitung tarif/total/alokasi | `SHARED INTEGRATION` | Doctor | Billing | `ARCHITECTURAL INVARIANT` | `VERIFIED` | `BuildPrescriptionSnapshot` | — | tidak dihitung | tidak dihitung |
| `RJ-BIL-CAP-017` | Reconciliation case dan recovery report | `BILLING` | Billing Integration | Billing, Finance | `DOWNSTREAM` | `SOURCE ABSENT — ROADMAP TASK STATUS NEEDS REVERIFICATION` | Tidak ada model/service/endpoint rekonsiliasi pada `HEAD`; `BillingFolioController` hanya `3` route baseline | `RJ-BIL-BE-007` | `NO` | **YES** |
| `RJ-BIL-CAP-013` | Payer allocation, patient responsibility | `BILLING` | Billing | Payer, Cashier | `DOWNSTREAM` | `MISSING` | `RJ-BIL-CONFLICT-001` `CONFIRMED` | `RJ-BIL-OQ-001/002/005` | `NO` | **YES** |
| `RJ-BIL-CAP-014` | Void, adjustment, refund, write-off | `BILLING` | Billing | Finance | `DOWNSTREAM` | `SOURCE ABSENT — ROADMAP TASK STATUS NEEDS REVERIFICATION` | Tidak ada `BilFinancialAction` maupun approval policy pada `HEAD` | `RJ-BIL-BE-006` | `NO` | **YES** |
| `RJ-BIL-CAP-022` | Klaim, settlement, payer eksternal | `BILLING` | Payer | Finance | `DOWNSTREAM` | `MISSING` | `RJ-BIL-DEP-009` `INACTIVE` | `RJ-BIL-BE-005` | `NO` | **YES** |
| `RJ-BIL-CAP-020` | Layar Billing, folio, payer split | `BILLING` | Frontend Billing | Kasir, petugas | `DOWNSTREAM` | `PARTIAL` | Layar `billing-folio` dan `clinical-boundary` ada pada `HEAD`; berkas test yang diklaim tidak ditemukan | — | `NO` | **YES** |
| — | `PrescriptionResponse.paymentStatus` terbaca dari endpoint klinis | `PHARMACY` + `BILLING` | Pharmacy | layar mana pun | `REFERENCE` | `REFERENCE` — bukan mutasi | `PrescriptionController.cs:217`, `:599`, `:671`, `:691` | — | `NO` | `NO` |

**Tidak ada satu pun butir Dokter yang `Blocks Billing DoD = YES`, dan tidak ada satu pun butir
Billing yang `Blocks Doctor DoD = YES`.** Kedua scope terpisah bersih.

Yang ada adalah **downstream readiness dependency** — output Dokter yang ditunggu Billing. Tiga
butir memilikinya: `RJ-DOC-CAP-015` dan `RJ-DOC-CAP-030` menahan *prescription handoff readiness*,
`RJ-DOC-CAP-023` menahan *reliable consumption readiness*. Arah ketergantungannya Dokter → Billing,
bukan sebaliknya, sehingga tidak satu pun membuat Billing menjadi blocker Dokter.

### 0.2 Istilah finansial yang muncul di artefak Dokter

Setiap istilah berikut wajib punya klasifikasi eksplisit supaya tidak terbaca sebagai pekerjaan
Dokter.

| Istilah | Klasifikasi | Alasan |
|---|---|---|
| `Paid`, `Settled`, `InsuranceApproved`, `PaymentWaived` | `OUT OF SCOPE` | Modul klinis tidak boleh menetapkannya; endpoint penulisnya sudah dihapus `RJ-BIL-BE-002` |
| `Payment`, `Cashier`, `Invoice`, `Receipt` | `OUT OF SCOPE` | Tidak ada permukaan klinis yang menyentuhnya |
| `Billing Folio`, `Charge`, `Tariff` | `DOWNSTREAM` | Consumer clinical fact; `RJ-BIL-BE-001` |
| `Payer`, `Allocation`, `Patient responsibility` | `DOWNSTREAM` | `RJ-BIL-BE-005` |
| `Financial Action`, approval finansial | `DOWNSTREAM` | `RJ-BIL-BE-006` |
| `Reconciliation` (finansial) | `DOWNSTREAM` | `RJ-BIL-BE-007` |
| `Claim`, `Settlement` | `DOWNSTREAM` | `RJ-BIL-BE-008` |
| `TariffSnapshot` pada clinical fact | `REFERENCE` | Harga kotor sebagai rujukan; **bukan** perhitungan. Pembagian tanggungan sengaja tidak disertakan |
| `PaymentStatus` pada response klinis | `REFERENCE` | Hanya dibaca; pemiliknya Farmasi bersama Billing |
| Idempotency dan `OutcomeUnknown` pada handoff | `DEPENDENCY` | Milik Dokter di sisi produser (`RJ-DOC-BE-005`), milik Billing di sisi consumer (`RJ-BIL-BE-007`) |

---

## 1. Cara membaca tabel ini

> ### `HISTORICAL SNAPSHOT — DO NOT USE AS CURRENT STATUS`
>
> Bagian `1` sampai `4` adalah potret scope **Billing** per `2026-08-28`. Ia dipertahankan sebagai
> jejak, **bukan** sebagai status saat ini.
>
> Audit `2026-08-31` menemukan tiga pernyataan di bawah tidak lagi cocok dengan backend `HEAD`
> `801a4f5`. Menilai ulang status task Billing adalah **wewenang pemilik Billing** dan sengaja
> tidak dilakukan dari task koreksi roadmap Dokter:
>
> | Pernyataan snapshot | Keadaan pada `HEAD` |
> |---|---|
> | `RJ-BIL-BE-004` Radiologi `⛔ Terblokir`, *"area belum ada pada source"* | `SOURCE EXISTS` — `17` berkas beserta `RadiologySafetyGateTests` dan `RadiologyStudyLifecycleTests`. Acceptance task **belum** dinilai ulang, sehingga statusnya **bukan** `COMPLETE` |
> | `RJ-BIL-BE-006` `✅ Selesai`, *"46 test lulus"* | `SOURCE ABSENT` — tidak ada `BilFinancialAction`, approval policy, maupun endpoint financial action |
> | `RJ-BIL-BE-007` `✅ Selesai`, *"delapan endpoint rekonsiliasi"* | `SOURCE ABSENT` — `BillingFolioController` hanya memuat `3` route baseline `BE-001`; tidak ada model/service rekonsiliasi |
>
> Manifest revisi `21` sendiri mencatat `BE-006` dan `BE-007` sebagai *working tree yang belum
> di-commit*. Penjelasan yang paling sesuai bukti adalah working tree itu tidak pernah masuk ke
> cabang ini. Pemilik Billing perlu memutuskan apakah ia dipulihkan atau dikerjakan ulang.

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
