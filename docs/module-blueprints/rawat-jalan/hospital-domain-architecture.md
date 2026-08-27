# Rawat Jalan Billing — Hospital Domain Architecture

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `10` |
| Domain architecture revision | `1` |
| Scope | Release 1 internal/manual Rawat Jalan Billing |
| Requirement readiness | `PARTIALLY_READY`; seluruh core internal/manual `READY_FOR_DOMAIN_DESIGN` |
| Domain architecture readiness | `DOMAIN_ARCHITECTURE_PARTIAL` |
| Slice siap | Clinical fact handoff, folio/charge, actual consumption, multi-payer manual, financial correction, payment, claim manual, idempotency, dan reconciliation |
| Slice terblokir | Aktivasi adapter payer eksternal bernama |
| Decision basis | `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` |
| Target contract | `RJ-BIL-CONTRACT-001@1.0.0` (`OWNER_APPROVED`) |
| Backend evidence | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend evidence | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Baseline reference | Tidak dipakai; `NOT_YET_AVAILABLE` |

Dokumen ini menjelaskan makna bisnis, ownership, lifecycle, dan batas konsistensi. Nama konsep
di sini belum otomatis menjadi nama class, tabel, endpoint, atau event implementasi.

## 1. Scope dan traceability

### 1.1 Scope yang dirancang

1. Layanan klinis menghasilkan fakta bahwa layanan sudah mencapai milestone yang dapat dinilai
   secara finansial.
2. Billing membuat dan memelihara satu folio untuk satu `EncounterId`.
3. Billing membentuk charge berdasarkan fakta klinis, tariff snapshot, serta aturan yang
   disetujui dan sedang berlaku.
4. Payer Management mencatat eligibility, authorization, guarantee, claim, serta keputusan
   payer. Billing menerapkan keputusan tersebut menjadi allocation finansial.
5. Cashier mencatat penerimaan pembayaran dan menjalankan refund yang telah disetujui.
6. Finance mencatat posting akuntansi dan rekonsiliasi finansial.
7. Integration Reliability menjaga idempotency, retry, status processing, partial failure,
   dan reconciliation case.

### 1.2 Scope yang tidak dirancang untuk aktivasi

Adapter AdMedika, BPJS/JKN, atau sistem payer eksternal lain belum boleh diaktifkan. Arsitektur
hanya menyediakan batas kontrak umum. Detail vendor, protocol, credential, security,
sandbox/UAT, dan support escalation tetap berada pada `RJ-BIL-DEP-009`.

## 2. Ubiquitous language

| Istilah | Makna bisnis tunggal |
|---|---|
| `Encounter` | Satu episode pelayanan pasien. `EncounterId` dipakai sebagai correlation key, bukan pemilik charge |
| Primary Registration Payer | Payer utama saat registrasi untuk eligibility atau estimasi awal; bukan keputusan settlement final |
| Clinical Fact | Fakta versioned dari domain klinis tentang order, pelaksanaan, pembatalan, atau konsumsi aktual |
| Billable Milestone | Perubahan layanan yang membuat Billing boleh menilai pembentukan charge; tidak selalu berarti charge langsung final |
| Billing Folio | Container finansial canonical untuk seluruh charge dan settlement pada satu encounter |
| Charge | Konsekuensi finansial atas satu sumber layanan yang dapat ditelusuri |
| Charge Component | Bagian terukur dari charge, misalnya jasa profesional, alat, obat, reagen, contrast, atau BHP |
| Tariff Snapshot | Salinan tariff/version/unit price/effective date yang dipakai saat kalkulasi agar histori tidak berubah ketika master berubah |
| Actual Consumption | Fakta quantity atau bagian layanan yang benar-benar terlaksana, bukan nominal Rupiah final |
| Payer Decision | Fakta eligibility, authorization, guarantee, approval, partial approval, atau rejection dari Payer Management |
| Billing Allocation | Penerapan nominal payer dan patient responsibility pada folio atau charge line oleh Billing |
| Patient Responsibility | Bagian net eligible charge yang secara resmi menjadi tanggung jawab pasien |
| Financial Action | Void, adjustment, reversal, refund, waiver/FOC, write-off, atau tindakan koreksi lain yang tidak menghapus histori asli |
| Financial Projection | Salinan read-only status finansial pada domain klinis; bukan financial source of truth |
| Processing Outcome | Status pemrosesan fact seperti `Applied`, `OutcomeUnknown`, atau `PendingReconciliation`; berbeda dari status charge/payment |
| Reconciliation Case | Kasus terkontrol untuk menyelesaikan perbedaan antara fakta canonical dan keadaan finansial canonical |
| Manual Operator Outcome | Keputusan payer/claim yang dicatat manusia dan tidak diklaim sebagai respons API eksternal |

## 3. Peta bounded context

| Context ID | Bounded context | Tanggung jawab | Konsep yang dimiliki | Hubungan |
|---|---|---|---|---|
| `RJ-CTX-001` | Registration & Encounter | Memiliki episode pelayanan dan primary registration payer | Encounter, registration payer reference | Upstream untuk seluruh context; menerima ringkasan finansial read-only |
| `RJ-CTX-002` | Outpatient Clinical Service | Memiliki order, pelaksanaan tindakan, clinical correction, serta fakta layanan | Clinical order/procedure fact | Mengirim clinical fact ke Billing Integration |
| `RJ-CTX-003` | Pharmacy Fulfillment | Memiliki resep, verification, preparation, dispensing, return/waste, dan clinical cancellation | Prescription, fulfillment, actual quantity | Mengirim fact; membaca financial projection |
| `RJ-CTX-004` | Laboratory Operations | Memiliki Lab Order, Specimen, Result, validation/release, dan cancellation | Lab Order, Specimen, Result | Mengirim acceptance/consumption/correction fact |
| `RJ-CTX-005` | Radiology Operations | Memiliki Order, Study/Acquisition, safety gate, Report, repeat/abort, dan amendment | Radiology Order, Study, Report | Mengirim acquisition/consumption/correction fact |
| `RJ-CTX-006` | Billing / Revenue Cycle | Satu-satunya pemilik folio, charge, calculation, allocation finansial, patient responsibility, correction, dan close/reopen | Folio, Charge, Allocation, Financial Action | Downstream clinical facts dan payer decisions; upstream Cashier/Finance |
| `RJ-CTX-007` | Payer / Insurance Management | Memiliki eligibility, authorization, guarantee, claim, payer decision, serta external integration state | Authorization, Claim, Payer Decision | Memberi decision fact ke Billing; menerima referensi folio/charge |
| `RJ-CTX-008` | Cashier | Memiliki payment collection, receipt, payment cancellation sesuai kewenangan, serta refund execution | Payment, Receipt, Refund Execution | Membayar allocation/patient responsibility; mengirim settlement fact |
| `RJ-CTX-009` | Finance & Accounting | Memiliki accounting posting, closing consequence, refund/reversal accounting, dan financial reconciliation | Accounting Posting, GL consequence | Mengonsumsi financial fact dari Billing/Cashier |
| `RJ-CTX-010` | Authorization Workflow | Memiliki request approval, maker-checker, policy version, delegation, dan approval evidence | Approval Request, Approval Decision | Dipakai financial action berisiko tinggi |
| `RJ-CTX-011` | Billing Integration & Reconciliation | Memiliki penerimaan fact idempotent, processing outcome, backlog, exception, dan reconciliation case | Fact Processing, Reconciliation Case | Menjembatani context klinis, Billing, Payer, Cashier, dan Finance |

## 4. Model ownership

| Kelompok data | Pemilik canonical | Klasifikasi | Pemakaian context lain |
|---|---|---|---|
| Patient dan Encounter | Registration/Patient Management | `Existing` | Hanya reference; tidak disalin menjadi pasien Rawat Jalan Billing |
| Primary Registration Payer | Registration | `Extend` | Dipakai untuk konteks awal, bukan settlement truth |
| Prescription dan dispensing facts | Pharmacy | `Extend` | Billing menerima versioned clinical fact; Pharmacy membaca projection |
| Procedure execution facts | Clinical Service | `Extend` | Billing menerima completion/actual-consumption fact |
| Lab Order/Specimen/Result | Laboratory | `Extend` | Billing hanya menerima fact milestone dan consumption |
| Radiology Order/Study/Report | Radiology | `New` secara domain logis | Billing hanya menerima performed/abort/repeat/consumption fact |
| Billing Folio dan Charge | Billing/Revenue Cycle | `New` | Encounter/clinical context hanya membaca summary/projection |
| Billing Allocation dan Patient Responsibility | Billing/Revenue Cycle | `New` | Payer menyediakan decision fact; Cashier memakai nominal canonical |
| Eligibility/Authorization/Guarantee/Claim | Payer Management | `New` | Billing tidak mengambil alih lifecycle payer/claim |
| Payment dan Receipt | Cashier | `New`/`Extend` sesuai capability existing hilir | Billing menerima settlement fact, bukan mengarang pembayaran |
| Accounting Posting | Finance | `Adapter/View` dari perspektif Billing | Billing menyimpan reference/status, bukan GL truth |
| Approval Request | Authorization Workflow | `Existing` atau `Extend` | Financial Action mereferensikan approval, tidak membuat engine kedua |
| Fact Processing/Reconciliation Case | Billing Integration | `New` | Menjaga reliability tanpa mengubah clinical truth |

## 5. Katalog konsep domain

| Concept ID | Konsep | Klasifikasi | Pemilik | Identitas dan peran | Invariant utama |
|---|---|---|---|---|---|
| `RJ-DOM-001` | Encounter Reference | `VALUE_OBJECT` / reference | Registration | `EncounterId` menghubungkan seluruh proses | Tidak boleh dipakai Billing untuk mengubah encounter |
| `RJ-DOM-002` | Clinical Service Fact | `DOMAIN_EVENT` | Context klinis terkait | Stable source ID, version, domain, entity, occurred time, correlation, idempotency key | Retry memakai identity/version yang sama |
| `RJ-DOM-003` | Billing Folio | `AGGREGATE_ROOT` | Billing | Satu folio canonical per encounter dan scope billing yang disetujui | Folio tidak ditutup bila ada mandatory unresolved item |
| `RJ-DOM-004` | Charge | `AGGREGATE_ROOT` | Billing | Menelusuri source fact/version dan clinical source | Satu source fact/version/operation tidak membuat charge ganda |
| `RJ-DOM-005` | Charge Component | `ENTITY` | Billing | Bagian terukur dari charge | Failed component tidak diam-diam menjadi nol |
| `RJ-DOM-006` | Tariff Snapshot | `VALUE_OBJECT` | Billing | Version, effective date, unit price, currency, pricing context | Immutable setelah charge diterapkan |
| `RJ-DOM-007` | Partial Charge Rule | `REFERENCE_DATA` | Billing/Finance governance | Rule versioned dan effective-dated per service/component | Rule draft/expired/unapproved tidak menghasilkan final charge |
| `RJ-DOM-008` | Payer Authorization | `AGGREGATE_ROOT` | Payer Management | Satu authorization versioned untuk payer/service scope | Eligibility tidak sama dengan authorization |
| `RJ-DOM-009` | Payer Decision | `ENTITY` / `DOMAIN_EVENT` | Payer Management | Approved/partial/rejected amount, reason, source, version | External failure tidak dianggap rejection |
| `RJ-DOM-010` | Billing Allocation Plan | `AGGREGATE_ROOT` | Billing | Allocation version per folio/charge line | Total allocation + patient responsibility = net eligible charge |
| `RJ-DOM-011` | Patient Responsibility | `ENTITY` | Billing | Residual resmi setelah benefit/adjustment/allocation | Residual tidak boleh hilang secara diam-diam |
| `RJ-DOM-012` | Financial Action | `AGGREGATE_ROOT` | Billing | Request dan execution untuk correction | Original charge/history tidak dihapus |
| `RJ-DOM-013` | Approval Request | `AGGREGATE_ROOT` | Authorization Workflow | Maker, checker, policy version, requested impact | Effective maker dan checker harus berbeda |
| `RJ-DOM-014` | Payment | `AGGREGATE_ROOT` | Cashier | Collection dan settlement terhadap financial reference | Payment tidak mengubah clinical fact |
| `RJ-DOM-015` | Claim | `AGGREGATE_ROOT` | Payer Management | Claim processing dan settlement lifecycle terpisah | Claim approved tidak otomatis berarti paid |
| `RJ-DOM-016` | Fact Processing Record | `AGGREGATE_ROOT` | Billing Integration | Outcome per stable source/version/operation | Concurrent duplicate menghasilkan satu processing result |
| `RJ-DOM-017` | Reconciliation Case | `AGGREGATE_ROOT` | Billing Integration | Mismatch type, impact, owner, status, action, resolution | Reconciliation tidak menimpa clinical truth |
| `RJ-DOM-018` | Financial Projection | `Adapter/View` | Domain klinis pemakai | Financial reference, status/version, source update, sync time | Versi lama tidak menimpa projection yang lebih baru |
| `RJ-DOM-019` | External Payer Adapter Contract | `EXTERNAL_CONTRACT` | Payer/Integration | Normalized request/status/correlation capability | Tetap disabled sampai `RJ-BIL-DEP-009` selesai |

## 6. Model aggregate

### 6.1 Billing Folio

Root: `RJ-DOM-003 Billing Folio`.

Batas konsistensi:

- tepat satu folio canonical untuk satu `EncounterId` dalam scope Rawat Jalan yang disetujui;
- folio mengetahui reference charge, allocation, payment/claim, approval, dan reconciliation;
- folio close memeriksa seluruh mandatory dependency, tetapi tidak mengambil alih ownership
  aggregate lain;
- reopen membuat tindakan berisiko tinggi dan mempertahankan closing history.

Tindakan bisnis: buka folio, tautkan charge, evaluasi kesiapan settlement, tutup, ajukan reopen,
dan terapkan reopen yang telah disetujui.

### 6.2 Charge

Root: `RJ-DOM-004 Charge`; child: `RJ-DOM-005 Charge Component`; value:
`RJ-DOM-006 Tariff Snapshot`.

Invariant:

1. Charge selalu mempunyai `EncounterId`, clinical source, source fact ID/version, dan milestone.
2. Nominal historis memakai tariff snapshot, bukan master terbaru.
3. Partial service hanya memakai rule aktif yang disetujui. Tanpa rule, component masuk
   `PendingFinancialReview`.
4. Correction membuat Financial Action/adjustment versioned; original charge tetap terlihat.

Contoh: tindakan mempunyai komponen fasilitas Rp300.000 dan BHP Rp100.000. Jika BHP aktual
hanya dua dari empat unit dan rule quantity-based yang berlaku menetapkan Rp25.000 per unit,
komponen BHP menjadi Rp50.000. Contoh ini hanya ilustrasi rule yang sudah dikonfigurasi; sistem
tidak boleh mengarang rumus tersebut ketika rule belum tersedia.

### 6.3 Billing Allocation Plan

Root: `RJ-DOM-010 Billing Allocation Plan`; child: payer allocation dan
`RJ-DOM-011 Patient Responsibility`.

Invariant:

- allocation memakai nominal absolut;
- coverage boleh berbeda per charge line;
- perubahan payer membuat version baru dan mempertahankan keputusan sebelumnya;
- FOC/waiver bukan payer;
- tidak boleh terjadi over-allocation.

Contoh: net eligible charge Rp1.000.000. Payer A menyetujui Rp600.000, Payer B menyetujui
Rp250.000, sehingga patient responsibility Rp150.000. Totalnya wajib tetap Rp1.000.000.

### 6.4 Financial Action

Root: `RJ-DOM-012 Financial Action`; reference ke `RJ-DOM-013 Approval Request`.

Void, reversal, refund, waiver/FOC, write-off, discretionary adjustment, folio reopen, dan
cross-encounter correction memiliki makna terpisah. Approval selesai sebelum mutation efektif.
Execution menguji ulang current target state dan wajib idempotent.

### 6.5 Fact Processing dan Reconciliation

`RJ-DOM-016 Fact Processing Record` menjaga exactly-once business effect. `RJ-DOM-017
Reconciliation Case` mengelola outcome yang tidak dapat diselesaikan secara deterministik.

Keduanya tidak mengubah clinical fact agar cocok dengan financial state. Koreksi harus kembali
ke owning domain dan diterbitkan sebagai version baru.

## 7. Model relasi logis

| Sumber | Relasi | Tujuan | Kardinalitas | Semantik ownership |
|---|---|---|---|---|
| Encounter | memiliki folio Rawat Jalan | Billing Folio | `1 : 0..1` dalam scope target | Encounter reference; Billing memiliki folio |
| Billing Folio | mengonsolidasikan | Charge | `1 : 0..*` | Charge tetap aggregate terpisah |
| Charge | terdiri dari | Charge Component | `1 : 1..*` | Component berada dalam batas Charge |
| Clinical Service Fact | menghasilkan atau mengoreksi | Charge | `1 : 0..*` versioned | Fact tetap milik clinical context |
| Charge Line | dialokasikan melalui | Billing Allocation Plan | `1 : 0..*` versioned | Billing memiliki allocation |
| Payer Decision | menjadi input | Billing Allocation Plan | `0..* : 1` | Payer memiliki decision; Billing menerapkan nominal |
| Allocation/Patient Responsibility | diselesaikan oleh | Payment atau Claim Settlement | `1 : 0..*` | Cashier/Payer memiliki settlement lifecycle |
| Financial Action | memerlukan | Approval Request | `0..1 : 1` untuk action yang approval-required | Workflow memiliki approval evidence |
| Source Fact | mempunyai | Fact Processing Record | `1 version : 1 per operation` | Integration memiliki processing outcome |
| Fact/Charge/Allocation/Payment | dapat membuka | Reconciliation Case | `1 : 0..*` | Case menyimpan reference, bukan ownership data sumber |

Relasi ini logis. Desain foreign key, index, dan delete behavior fisik merupakan pekerjaan
`design-business-module` setelah handoff.

## 8. Lifecycle dan perubahan status

### 8.1 Clinical milestone boundary

| Sumber | Milestone yang dapat memicu eligibility | Bukan trigger final charge |
|---|---|---|
| Prescription | Submission/finalization sesuai keputusan Release 1 dan fact fulfillment/consumption untuk koreksi | Penyerahan obat semata tidak mengubah financial ownership |
| Procedure | `Completed` atau actual performed fact | Selection/order saja |
| Laboratory | Specimen/order `Accepted` untuk pemeriksaan specimen-based | Requested, Collected, Received, result Validated/Released |
| Radiology | Performed acquisition dengan usable study | Requested, Accepted, Scheduled, report Released |

Billing tetap memvalidasi rule, tariff, duplicate, dan current state sebelum charge diterapkan.

### 8.2 Fact processing

| Dari | Tindakan | Ke | Syarat utama |
|---|---|---|---|
| Belum diterima | Terima fact valid | `Received` | Stable identity/version/key tersedia |
| `Received` | Mulai proses | `Processing` | Belum ada hasil canonical untuk operation yang sama |
| `Processing` | Terapkan seluruh komponen | `Applied` | Semua mandatory component berhasil |
| `Processing` | Sebagian komponen berhasil | `PartiallyApplied` | Komponen gagal tetap visible |
| `Processing` | Validasi bisnis gagal | `RejectedValidation` | Correction memakai source version baru |
| `Processing` | Gangguan sementara | `TransientFailure` | Retry bounded memakai key yang sama |
| `Processing` | Outcome tidak dapat diketahui | `OutcomeUnknown` | Wajib status query/reconciliation sebelum retry |
| Status exception | Buka case | `PendingReconciliation` | Mismatch tidak deterministik |
| `PendingReconciliation` | Selesaikan dengan evidence | `Reconciled` | Resolution dan audit lengkap |

### 8.3 Approval request

Lifecycle minimum yang telah disetujui:

`Draft → Submitted → PendingApproval → Approved`, dengan jalur `Rejected`,
`ReturnedForRevision`, `Cancelled`, dan optional `Expired`.

Material change setelah dikembalikan menghasilkan maker revision baru. Approval yang expired
tidak boleh dianggap approved.

### 8.4 Payer authorization dan claim

Authorization: `Draft → Submitted → Pending → Approved`, dengan `PartiallyApproved`,
`Rejected`, `Expired`, `Cancelled`, dan optional `NeedMoreInformation`.

Claim processing: `Draft → Ready → Submitted → Acknowledged → InAdjudication`, dengan
need-info/approved/partial/rejected/cancelled.

Settlement: `NotDue → PaymentPending → PartiallyPaid → Paid → Reconciled`, dengan dispute,
mismatch, atau reconciliation exception. `Approved` pada claim tidak melompati
`PaymentPending` menjadi `Paid`.

### 8.5 Folio closure

Nama status teknis final masih dikunci pada kontrak hilir. Makna lifecycle-nya wajib memuat:

1. folio aktif menerima charge dan allocation;
2. folio tidak siap ditutup bila ada `OutcomeUnknown`, pending approval/review, unresolved
   allocation, financially material failure, atau reconciliation case;
3. folio dapat menjadi fully settled setelah seluruh kewajiban selesai;
4. close mempertahankan closing evidence;
5. reopen hanya melalui approved high-risk action.

## 9. Tanggung jawab authorization

| Tindakan | Pemilik kewenangan | Batas |
|---|---|---|
| Mengubah clinical order sebelum handoff | Clinical owner terkait | Sesudah handoff hanya controlled request/correction |
| Menerima/menolak specimen dan memvalidasi/release hasil | Capability Lab yang disahkan | Jabatan organisasi tidak otomatis memberi capability |
| Menjalankan acquisition dan safety clearance | Capability Radiology yang disahkan | Mandatory safety state gagal/pending memblokir normal acquisition |
| Membentuk/finalize charge deterministic | Billing system actor dengan rule valid | Tidak memerlukan approval per transaksi bila seluruh sumber/rule valid |
| Mengubah allocation/patient responsibility | Billing berdasarkan payer decision/policy | Tidak boleh over-allocation atau silent residual |
| Mengajukan financial action | Maker dengan capability sesuai jenis tindakan | Tidak boleh menyetujui request sendiri |
| Menyetujui high-risk financial action | Checker berbeda berdasarkan policy efektif | Missing policy mempertahankan fail-closed |
| Mengumpulkan payment dan menerbitkan receipt | Cashier | Tidak mengubah clinical fact |
| Menjalankan refund | Cashier setelah approval | Approval dan execution harus dapat ditelusuri |
| Accounting post/reversal | Finance | Billing finalization bukan GL posting |
| Mencatat manual payer outcome | Authorized payer operator | Wajib diberi label manual, evidence, actor, dan time |

## 10. Audit dan histori

Setiap perubahan material minimal mempertahankan:

- entity/source identity dan version;
- encounter, folio, charge, allocation, payment/claim reference yang relevan;
- prior state dan new state;
- action, reason, actor, effective actor/delegation, dan timestamp;
- policy/rule/tariff version;
- correlation dan idempotency key;
- approval evidence untuk action yang membutuhkan approval;
- superseded/correction reference tanpa menghapus histori lama.

Data sensitif eksternal tidak disalin mentah ke operational log. Bila evidence asli wajib
disimpan, gunakan penyimpanan aman dengan masking, encryption, retention, access control, dan
access audit yang ditentukan pada desain hilir.

## 11. Model integrasi

### 11.1 Internal clinical-to-billing

- Produsen: Clinical, Pharmacy, Laboratory, dan Radiology.
- Konsumen: Billing Integration, lalu Billing.
- Sumber kebenaran: produsen untuk clinical fact; Billing untuk financial consequence.
- Identity: stable source ID/version/domain/entity, encounter, occurred time, correlation,
  dan idempotency key.
- Kegagalan: timeout menjadi `OutcomeUnknown`; retry tidak membuat identity baru.
- Rekonsiliasi: membandingkan canonical clinical fact dengan canonical Billing state.

### 11.2 Payer-to-billing

Payer Management menghasilkan versioned decision fact. Billing menerapkannya menjadi Billing
Allocation. External response tidak boleh langsung memutasi clinical order, charge, atau
payment.

### 11.3 Billing-to-cashier/finance

Cashier menerima financial reference yang sah untuk collection/refund. Finance menerima
posting/reversal facts. Keduanya mengembalikan versioned outcome/reference ke Billing tanpa
mengambil alih folio ownership.

### 11.4 External payer adapters

Core domain hanya mengenal normalized adapter contract. Setiap adapter harus menyatakan
capability profile untuk idempotency, status query, cancellation, amendment, partial approval,
eligibility/authorization, claim submission, timeout, dan reconciliation.

Status: `BLOCKED_FOR_ACTIVATION`. Manual workflow tetap menjadi jalur Release 1.

## 12. Dampak billing

Seluruh slice core berdampak langsung pada charge.

Invariant finansial utama:

1. `Gross Charge - Approved Adjustment - Discount/Benefit = Net Eligible Charge`.
2. `SUM(Payer Allocations) + Patient Responsibility = Net Eligible Charge`.
3. Harga historis tidak dihitung ulang dari master yang telah berubah.
4. Internal hospital error tidak otomatis menjadi patient responsibility.
5. Missing rule/threshold tidak boleh diganti nilai tebakan; component/action masuk review atau
   fail-closed sesuai jenisnya.
6. Clinical cancellation tidak otomatis menentukan void atau reversal.

## 13. Dampak keselamatan klinis

| Area | Klasifikasi | Batas keselamatan |
|---|---|---|
| Pharmacy | Relevan terhadap keselamatan | Urgent dispensing exception tidak sama dengan payment waiver dan wajib direkonsiliasi |
| Laboratory | Relevan terhadap keselamatan | Identity specimen, acceptance/rejection, recollection, validation, release, dan amendment dipisahkan |
| Radiology | Relevan terhadap keselamatan | Patient/procedure/modality verification dan mandatory safety gate mendahului acquisition |
| Billing | Finansial, tidak menjadi clinical authority | Payment atau payer state tidak menentukan validitas hasil atau tindakan klinis |

Contoh: ketika Billing/Payer tidak tersedia dan obat harus segera diberikan berdasarkan
clinical urgency policy, Pharmacy dapat melanjutkan hanya melalui authorized exception. Sistem
tetap mencatat outstanding financial workflow; tindakan tersebut tidak boleh ditandai
`PaymentWaived` secara otomatis.

## 14. Gap dan dependency arsitektur

| ID | Gap | Klasifikasi | Dampak |
|---|---|---|---|
| `RJ-BIL-DEP-009` | Named external adapter contract/security/UAT/reconciliation evidence belum tersedia | `BLOCKING` untuk aktivasi eksternal | Tidak memblokir core internal/manual |
| `RJ-BIL-CONFLICT-001` | Encounter payment source AS-IS belum mendukung target multi-payer | Implementation dependency | Perlu adapter/migration design; ownership target sudah jelas |
| `RJ-BIL-CONFLICT-005/006` | Pharmacy financial mutation legacy | Implementation dependency | Perlu compatibility/deprecation design |
| `RJ-BIL-CONFLICT-003` | Lab/Radiology journey existing tidak terbukti | Capability dependency | Jangan mengklaim reuse; target domain tetap siap |
| `RJ-BIL-ARCH-GAP-001` | Nilai tariff/partial rule per service belum tersedia | Configuration gate | Tidak mengubah model domain; unresolved component masuk review |
| `RJ-BIL-ARCH-GAP-002` | Threshold dan approval matrix final belum tersedia | Configuration gate | High-risk action fail-closed |
| `RJ-BIL-ARCH-GAP-003` | SOP checklist klinis detail belum dilampirkan | Operational safety configuration | Boundary safety sudah siap; activation capability terkait menunggu SOP |

Tidak ada gap di atas yang memerlukan perubahan ownership core Release 1.

## 15. Kesiapan arsitektur

Status keseluruhan: `DOMAIN_ARCHITECTURE_PARTIAL`.

### Slice `DOMAIN_ARCHITECTURE_READY`

- clinical fact handoff dan billable milestone;
- Billing Folio, Charge, Charge Component, dan Tariff Snapshot;
- actual-consumption financialization;
- multi-payer manual allocation dan patient responsibility;
- financial action, maker-checker, close/reopen;
- Pharmacy financial projection;
- internal/manual payer authorization, claim, adjudication, dan settlement;
- fact processing, idempotency, downtime recovery, dan reconciliation.

### Slice `DOMAIN_ARCHITECTURE_BLOCKED`

- aktivasi adapter payer eksternal bernama sampai `RJ-BIL-DEP-009` lengkap.

### Handoff

Slice core yang siap boleh diteruskan ke `design-business-module`. Handoff wajib mempertahankan:

- blueprint `RJ-BIL-BP-001` revision `10`;
- domain architecture revision `1`;
- requirement readiness `PARTIALLY_READY` dengan daftar slice siap;
- domain readiness `DOMAIN_ARCHITECTURE_PARTIAL` dengan core independen yang siap;
- decision `RJ-BIL-GATE-DEC-001..009`;
- dependency/conflict AS-IS sebagai delta implementasi;
- larangan aktivasi adapter eksternal.

`design-business-module` boleh menyusun arsitektur backend/frontend, ERD, contract, validation,
permission, audit, dan test strategy untuk core internal/manual. Skill tersebut tidak boleh
mengaktifkan adapter eksternal atau mengubah ownership yang ditetapkan dokumen ini.

