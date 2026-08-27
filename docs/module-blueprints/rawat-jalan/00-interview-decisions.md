# Dokter / Rawat Jalan Billing - Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Revision | `10` |
| Status | `draft` |
| Interview mode | `Closure pass` |
| Final accountable owner | Direksi Rumah Sakit atau pejabat eksekutif dengan delegasi formal; assignment belum dilampirkan |
| Backend SHA | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Input revision/hash | Initial closure `ad344a...`; ownership `44a839...`; multi-payer `a57c3f...`; Laboratory `4d4447...`; Radiology `3da3a2...`; Actual Consumption `e16a81...`; Financial Governance `f9125f...`; Pharmacy Ownership `d86aba...`; Reliability `7ffa20...`; Payer Integration `d9fc83e18bdc5a209fa152b83eae733b090f62ddd8284836dc565efa49450e73` |

## Scope and Outcome

- **Fact `RJ-BIL-FACT-001`:** Menurut pemilik kebutuhan, menu Dokter/Rawat Jalan telah memiliki resep, tindakan, laboratorium, radiologi, dan layanan penunjang lain, tetapi alurnya belum lengkap sampai billing.
- **Decision `RJ-BIL-DEC-001` (draft):** `Clinical Order` tidak sama dengan `Final Financial Charge`. Tarif dan coverage dapat dihitung atau di-snapshot ketika order dibuat, sedangkan charge mengikuti milestone layanan.
- **Decision `RJ-BIL-DEC-003` (draft):** Satu `EncounterId` menjadi satu billing account/folio Rawat Jalan yang mengonsolidasikan administrasi, registrasi, poli, dokter, tindakan, obat, laboratorium, radiologi, dan layanan penunjang lain.
- **Decision `RJ-BIL-DEC-003A` (draft):** Settlement dalam folio yang sama dapat dipisahkan menurut payer, termasuk asuransi, perusahaan, patient excess, tunai, dan FOC.
- **Assumption `RJ-BIL-ASM-001`:** Nama canonical blueprint adalah `rawat-jalan` dengan prefix `RJ-BIL`; perubahan identitas memerlukan revisi eksplisit sebelum kontrak turunannya diterbitkan.
- **Assumption `RJ-BIL-ASM-002`:** Pernyataan bahwa capability sudah tersedia merupakan evidence wawancara, bukan fakta source code, sampai capability audit selesai.

## Actors, Ownership, and Invariants

| ID | Type | Item | Owner | Status |
|---|---|---|---|---|
| `RJ-BIL-OWN-001` | Decision | Direksi Rumah Sakit atau pejabat eksekutif dengan delegasi formal adalah final accountable owner | Direksi/delegated executive | `draft` |
| `RJ-BIL-OWN-002` | Decision | Clinical sign-off melibatkan Pelayanan Medis/Clinical Governance, Rawat Jalan, Farmasi, Laboratorium, Radiologi, dan Komite Medis bila diperlukan | Clinical governance | `draft` |
| `RJ-BIL-OWN-003` | Decision | Financial sign-off melibatkan Finance, Billing/Revenue Cycle, serta Insurance/Corporate Relation bila menyangkut payer atau claim | Financial governance | `draft` |
| `RJ-BIL-OWN-004` | Decision | IT/HIS/Developer mengimplementasikan keputusan dan bukan pemilik business rule klinis atau finansial | Final accountable owner | `draft` |

Invariants:

1. Order klinis tidak otomatis menjadi final charge.
2. Seluruh transaksi tetap tertaut ke `EncounterId` yang sama; pemisahan payer tidak membuat encounter baru.
3. Modul klinis tidak boleh membatalkan charge secara langsung setelah item masuk billing.
4. Koreksi finansial memakai void, adjustment, reversal, refund, FOC, atau write-off yang sesuai; histori tidak boleh dihapus.
5. Semua cancellation dan correction wajib mencatat actor, waktu, reason, status sebelum/sesudah, serta audit trail.
6. Transaksi `paid`, `posted`, atau `claimed` memerlukan approval supervisor sesuai kewenangan yang disahkan.
7. Repeat atau kegagalan akibat kesalahan internal rumah sakit tidak otomatis dibebankan kepada pasien.

## States, Exceptions, and Acceptance Criteria

### Billing milestones

| Service | Sebelum final charge | Trigger/milestone charge | Bukan trigger |
|---|---|---|---|
| Resep | Harga dapat dihitung saat item resep dibuat | Resep difinalkan bersama konsultasi dokter dan billing generated | Penyerahan obat; ini fulfillment |
| Tindakan | Harga dapat dihitung saat tindakan dipilih | Tindakan benar-benar dieksekusi dan `Completed` | Pemilihan/order saja |
| Laboratorium | Order dokter belum menjadi final charge | Order/specimen diterima dan divalidasi Laboratorium untuk diproses | Terbitnya hasil |
| Radiologi | Charge dapat dipersiapkan setelah acceptance/validation | Pemeriksaan/acquisition benar-benar dilakukan | Terbitnya hasil |

### Cancellation and correction authority

1. Dokter dapat mengubah atau membatalkan order ketika masih draft atau belum diambil alih unit pelaksana.
2. Setelah unit pelaksana mulai memproses, dokter hanya dapat mengajukan pembatalan; unit pelaksana memutuskan kelayakannya berdasarkan kondisi aktual.
3. Setelah item masuk billing, koreksi dilakukan oleh Billing/Finance melalui workflow finansial.
4. Item `paid`, `posted`, atau `claimed` wajib melalui approval supervisor sesuai kewenangan.

### Actual Consumption Rule

- Belum ada pelayanan atau material terpakai: full void atau tidak ditagihkan.
- Sebagian pelayanan atau material sudah dikonsumsi: partial charge berdasarkan konsumsi aktual dan kebijakan rumah sakit.
- Pelayanan selesai: full charge.
- Repeat/kegagalan internal rumah sakit: FOC/write-off melalui workflow berwenang.
- Imaging/acquisition yang sudah dilakukan dianggap layanan terlaksana walaupun hasil belum terbit.

### Testable acceptance criteria

1. Membuat order klinis tidak menghasilkan final charge sebelum milestone layanan terkait tercapai.
2. Sistem menghasilkan charge tepat satu kali ketika milestone terpenuhi; retry atau event duplikat tidak menggandakan charge.
3. Semua charge dari satu kunjungan tertaut ke satu `EncounterId` dan dapat dialokasikan ke lebih dari satu payer tanpa membuat encounter baru.
4. Cancellation sebelum konsumsi menghasilkan full void; partial processing menghasilkan partial charge; completed service menghasilkan full charge.
5. Dokter tidak dapat melakukan direct financial cancellation setelah billing terbentuk.
6. Void, adjustment, reversal, refund, FOC, dan write-off mempertahankan histori dan audit trail.
7. Koreksi atas item `paid`, `posted`, atau `claimed` ditolak tanpa approval supervisor yang valid.
8. Kegagalan internal rumah sakit dapat diarahkan ke FOC/write-off dan tidak otomatis menjadi patient responsibility.
9. Status hasil Lab/Radiologi tidak menjadi syarat pembentukan charge apabila milestone operasional yang ditetapkan sudah tercapai.
10. Partial failure antara layanan klinis dan billing memiliki status yang dapat direkonsiliasi tanpa silent loss atau duplicate charge.

## UI Decision Authority

- Security, privacy, invariant, dan kontrak backend yang disetujui mengungguli keputusan presentasi.
- Dokumen ini tidak memutus menu, route, layout, visual treatment, wording tombol, atau komponen UI.
- UI harus membedakan order klinis, fulfillment/progress layanan, billing status, payer allocation, dan financial correction; detail interaksi menunggu capability audit dan UI authority yang sah.

## Decision Log

| Decision ID | Type | Item | Owner | Status | Approval evidence |
|---|---|---|---|---|---|
| `RJ-BIL-DEC-001` | Decision | Clinical Order tidak langsung menjadi Final Financial Charge | Joint clinical-financial governance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-002` | Decision | Billing milestone berbeda untuk resep, tindakan, laboratorium, dan radiologi | Pemilik klinis tiap layanan + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-003` | Decision | Seluruh charge Rawat Jalan dikonsolidasikan berdasarkan satu `EncounterId` | Rawat Jalan + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-004` | Decision | Cancellation/correction memakai status-based authority dan pemisahan clinical correction dari financial adjustment | Clinical governance + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-005` | Decision | Partial processing menggunakan Actual Consumption Rule | Unit pelaksana + Billing/Finance | `draft` | Jawaban closure pengguna; approval formal belum dilampirkan |
| `RJ-BIL-DEC-006` | Decision | Joint governance dengan Direksi/delegated executive sebagai final accountable owner | Direksi/delegated executive | `draft` | Jawaban closure pengguna; governance assignment belum dilampirkan |
| `RJ-BIL-DEC-007` | Decision | **Sukma Giri ditunjuk sebagai Product/Domain Owner `LaboratoryManagement`**, dan lifecycle modul itu pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dinaikkan dari `PLANNED` menjadi `ACTIVE`. Prefix `Lab` yang sudah terdaftar tidak berubah. Sejak titik ini `QBE-MOD-002` tidak lagi menahan pembuatan entity operasional `Lab*`, sehingga `LabSpecimen` dan `LabTransitionHistory` dari `RJ-BIL-BE-003` menjadi sah kepemilikannya. **Wewenang eksekusi database di luar lokal dan deployment tetap terpisah** dan tidak diberikan oleh keputusan ini. `FORMAL_LAB_GOVERNANCE_SIGNOFF` dan `CLINICAL_GOVERNANCE_SIGNOFF` tetap `OPEN` | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026. Penunjukan dilakukan oleh pemilik blueprint `RJ-BIL-BP-001` atas modul yang sebelumnya tidak bertuan; **tidak ada countersignature sponsor governance terpisah**. Menjawab permintaan `IGD-REQ-001` baris `80` dan melepas alasan `draft` pada `IGD-DEC-087`, keduanya milik blueprint IGD — pemilik IGD perlu diberi tahu, dan berkas IGD sengaja tidak disunting dari sisi ini |
| `RJ-BIL-DEC-008` | Decision | Urutan pengerjaan backend mengikuti **kolom Dependency pada tabel task**, bukan baris `Dependency sequence` naratif yang bertentangan dengannya. `RJ-BIL-BE-007` hanya bergantung pada `RJ-BIL-BE-001` dan Integration owner, sehingga berjalan mendahului `RJ-BIL-BE-005` yang `BLOCKED`. Dasar teknisnya: acceptance criteria `RJ-BIL-BE-006` berbunyi *close ditolak saat reconciliation pending*, sehingga `BE-006` justru bergantung pada `BE-007`; dan rekonsiliasi `BE-007` bekerja pada outcome pemrosesan fakta klinis, tidak menyentuh alokasi multi-payer sama sekali. `BUILDER_EXECUTION` untuk `RJ-BIL-BE-007` dinaikkan menjadi `AUTHORIZED` | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026 |
| `RJ-BIL-DEC-009` | Decision | Integration test Billing boleh berjalan terhadap database dev bersama `QuilvianNewDevTim01`, **hanya** melalui opt-in kedua `QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED` yang harus diketik sengaja. Perilaku bawaan tetap fail-closed. Penanda `prod`, `production`, `live`, `staging`, `stage`, dan `uat` ditolak **mutlak** dan tidak mengenal opt-in. Fallback diam ke `appsettings.Development.json` tetap tidak dihidupkan kembali. Membalik larangan sebelumnya pada handoff `RJ-BIL-BE-003`; yang dipertahankan dari larangan itu adalah intinya, yaitu tidak boleh ada migration yang terpasang ke database tim tanpa seseorang menyatakannya lebih dulu | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026. Dasar: hanya 2 migration tertunda dan keduanya milik blueprint ini — satu aditif, satu rename murni tanpa mutasi baris |
| `RJ-BIL-DEC-010` | Decision | Ambang **financially material** pada gerbang penutupan folio `RJ-BIL-GATE-DEC-008` diwujudkan sebagai master data yang dapat diubah admin tanpa rilis, dengan nilai awal **nol** — sehingga untuk sekarang setiap permanent failure memblokir penutupan folio. Nol dipilih karena merupakan perilaku paling aman dan bukan angka karangan. Angka sebenarnya tetap `OWNER_DECISION_REQUIRED`. Pola yang sama diterapkan pada durasi SLA dan skala prioritas reconciliation case, yang menurut `RJ-BIL-GATE-DEC-008` hanya memicu peringatan, eskalasi, dan visibilitas serta tidak pernah menyentuh keputusan finansial | Sukma Giri | `approved` | Sukma Giri, 26 Agustus 2026 |
| `RJ-BIL-DEC-011` | Decision | **`RJ-BIL-BE-006` memiliki entity approval-nya sendiri dengan prefix `Bil`**, bukan menumpang mesin Workflow milik `Areas/Corporate/HumanResource/`. Dasarnya: dua invariant `RJ-BIL-GATE-DEC-006` tidak dapat ditegakkan di sana. Pertama, `MstWorkflowStep.AllowSelfApproval` adalah `bool` per step yang dapat diubah menjadi `true` lewat `WorkflowStepController`, sedangkan larangan self-approval pada `GATE-DEC-006` bersifat tanpa syarat — artinya invariant finansial dapat dimatikan dari layar konfigurasi modul lain tanpa sepengetahuan Billing. Kedua, penyaringan maker hanya terjadi sekali saat assignment dibuat (`WorkflowService.cs:543`); `ApproveAsync` tidak pernah membandingkan penyetuju dengan `RequestedByUserId`, dan `ApprovalDelegationService` tidak merujuk `RequestedByUserId` sama sekali, sehingga delegasi dapat mengembalikan persetujuan kepada pengajunya — kasus yang justru dilarang eksplisit oleh `GATE-DEC-006`. Ketiga, ketiadaan approver valid menggagalkan permintaan dengan `400` (`WorkflowService.cs:556`), sedangkan `GATE-DEC-006` menuntut permintaan **bertahan** sebagai `PendingApproval` atau `BlockedByPolicyConfiguration`. Duplikasi kapabilitas persetujuan diterima **secara sadar** sebagai harganya. Keputusan ini **tidak** menutup jalan ke mesin bersama: bila kelak tersedia, permintaan approval Billing dapat dicerminkan ke sana tanpa memindahkan kewenangan finansialnya | Sukma Giri | `approved` | Sukma Giri, 27 Agustus 2026. Bukti dan penelusurannya pada [preflight-RJ-BIL-BE-006.md](preflight-RJ-BIL-BE-006.md) bagian `4`. Temuan terhadap modul Kepegawaian adalah **pengamatan read-only** yang dibatasi pada tiga titik penegakan di atas, bukan laporan cacat modul itu; tidak ada satu berkas pun miliknya yang disunting |
| `RJ-BIL-DEC-012` | Decision | **`BUILDER_EXECUTION` untuk `RJ-BIL-BE-006` dinaikkan menjadi `AUTHORIZED`** atas otoritas Sukma Giri selaku pemilik blueprint `RJ-BIL-BP-001`. `FORMAL_FINANCE_SIGNOFF` dan `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN`, **tidak** diberikan oleh keputusan ini, dan menjadi syarat sebelum aktivasi production — mengikuti pola yang sudah dipakai `RJ-BIL-BE-003` dan `RJ-BIL-BE-007`. Owner Workflow keluar dari jalur kritis karena `RJ-BIL-DEC-011` membuat Billing tidak lagi bergantung pada mesin Workflow modul lain. Wewenang ini mencakup penulisan code, pembuatan migration, dan eksekusi test; **tidak** mencakup deployment, commit, push, merge, maupun penerapan migration ke database di luar yang sudah diizinkan `RJ-BIL-DEC-009`. `IMPLEMENTATION COMPLETE` tetap **tidak** sama dengan `PRODUCTION GOVERNANCE APPROVED` | Sukma Giri | `approved` | Sukma Giri, 27 Agustus 2026. Menutup satu dari dua gate yang gagal pada [preflight-RJ-BIL-BE-006.md](preflight-RJ-BIL-BE-006.md) bagian `6`; gate sign-off Finance dan Security/Privacy **sengaja dibiarkan gagal** dan tetap tercatat sebagai blocker production |

## Open Questions and Blockers

| ID | Open question/blocker | Owner | Blocks |
|---|---|---|---|
| `RJ-BIL-OQ-001` | Nama, jabatan, masa berlaku, dan bukti delegasi final accountable owner belum tersedia | Direksi Rumah Sakit | Approval formal dan production activation |
| `RJ-BIL-OQ-002` | Bukti sign-off dari tiap clinical dan financial owner belum tersedia | Joint clinical-financial governance | Status `approved` dan implementation authorization |
| `RJ-BIL-OQ-003` | Rumus, unit konsumsi, pembulatan, minimum charge, dan tariff component untuk partial charge belum ditetapkan | Unit layanan + Finance/Billing | Desain detail partial charge |
| `RJ-BIL-OQ-004` | Matriks nominal/risiko yang menentukan supervisor approval untuk void, reversal, refund, FOC, dan write-off belum ditetapkan | Finance/Billing | Desain approval finansial detail |
| `RJ-BIL-OQ-005` | Perilaku outage, event terlambat, duplicate request, dan rekonsiliasi partial failure belum dipetakan terhadap capability existing | IT architecture melalui capability audit; business owner untuk conflict | Desain integrasi dan implementasi |
| `RJ-BIL-OQ-006` | Nama status, endpoint, entity, payer allocation, prescription finalization, dan billing generation yang benar belum dibuktikan dari source | Capability audit | Desain berbasis source dan implementation planning |

## Next Step

Jalankan `trace-existing-capabilities` terhadap snapshot backend dan frontend yang tercatat di manifest. Audit harus memetakan resep, tindakan, Lab, Radiologi, encounter, billing/folio, payer allocation, cancellation/correction, authorization, audit, idempotency, dan reconciliation sebagai `READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`. Jangan mulai implementasi sebelum hasil audit dan approval yang relevan tersedia.

## Closure Amendment 2026-08-19 - Financial Ownership

### `RJ-BIL-GATE-DEC-001`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Canonical ownership untuk clinical facts, billing, payer approval, payment, dan accounting |
| Requirement owner | Product/domain owner response |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH CLARIFICATION`, SHA-256 `44a8394596f169de6240536d1beaafe053411a643ae9ab29544ad2895b32113c` |
| Formal governance status | `OPEN` - nama/jabatan/delegasi dan joint governance sign-off belum dilampirkan |

Keputusan requirement yang dikunci:

1. `Clinical Domain` memiliki clinical order, clinical lifecycle, execution/fulfillment, clinical cancellation/correction, actual service quantity/material, dan fakta milestone billable.
2. `Billing/Revenue Cycle` adalah satu-satunya source of truth untuk billing account/folio, charge line, financial identifier, charge aggregation berdasarkan `EncounterId`, billing generation, payer allocation, patient responsibility/excess, adjustment, void sesuai kewenangan, serta charge lifecycle.
3. `Insurance/Payer Management` memiliki coverage/authorization outcome, termasuk `InsuranceApproved`.
4. `Cashier` memiliki payment collection, settlement, receipt, payment cancellation sesuai kewenangan, dan refund execution setelah approval.
5. `Finance` memiliki accounting posting, financial reconciliation, reversal/refund accounting, closing, dan GL consequence.
6. `EncounterId` hanya correlation/aggregation key. Encounter boleh membaca financial summary, tetapi tidak membuat atau memutasi charge.
7. Clinical module tidak boleh menetapkan `Paid`, `InsuranceApproved`, `PaymentWaived`, `BillingVoided`, `Refunded`, `Reversed`, `Settled`, atau `Reconciled`.
8. Clinical module menyerahkan facts seperti prescription submission, verification/dispensing, procedure completion, Lab acceptance/performance, Radiology performance, cancellation, dan actual-consumption change. Nama event final belum diputuskan sebagai kontrak implementasi.
9. Clinical cancellation dan financial cancellation adalah transaksi berbeda. Clinical change setelah charge terbentuk menghasilkan fact; Billing menentukan no adjustment, additional/partial adjustment, void, atau reversal menurut state dan Actual Consumption Rule.
10. `PaymentWaived` adalah financial authorization dengan reason, actor, timestamp, serta approval berdasarkan threshold/kebijakan; clinical module hanya dapat mengajukan clinical rationale.

### Acceptance criteria tambahan

1. Tidak ada clinical endpoint yang authoritative untuk financial status atau nominal final charge.
2. Financial status yang dibaca modul klinis berasal dari canonical Billing/Payer/Finance owner.
3. Clinical cancellation tidak menghapus charge dan tidak otomatis menentukan financial outcome.
4. Billing dapat menelusuri setiap charge ke `EncounterId`, clinical source, dan source milestone.
5. Cashier tidak dapat mengubah fakta klinis atau menghapus clinical order.

### Conflict yang terkonfirmasi

`RJ-BIL-CONFLICT-006`: Current Pharmacy flow masih mempunyai `MarkBillingGenerated`, `MarkPaid`, `MarkInsuranceApproved`, `MarkPaymentWaived`, dan cancellation yang mengubah payment status. Perilaku ini bertentangan dengan ownership decision. Conflict dicatat sebagai implementation evidence untuk downstream design; source tidak diubah pada closure pass.

## Closure Amendment 2026-08-19 - Multi-Payer

### `RJ-BIL-GATE-DEC-002`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Multi-payer allocation dan settlement semantics |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH CLARIFICATIONS`, SHA-256 `a57c3f85ec1d97f6ecf07cecfac9c8f33f39c6ed0ecd0ad6033a041a2baadd00` |
| Formal governance status | `OPEN` - joint governance sign-off belum dilampirkan |

Keputusan requirement yang dikunci:

1. Encounter menyimpan satu `Primary Registration Payer` untuk konteks registrasi, eligibility, estimasi coverage, dan authorization awal; ini bukan source of truth settlement.
2. Billing account/folio adalah canonical financial container multi-payer untuk satu `EncounterId`.
3. Allocation berlaku pada folio level dan charge-line level karena coverage dapat berbeda per layanan/item.
4. Canonical allocation menggunakan nominal absolut. Persentase hanya boleh menjadi rule pendukung, bukan satu-satunya sumber settlement.
5. Setiap allocation minimal menelusuri payer, priority/coordination order, requested/eligible/approved/allocated/rejected amount, patient responsibility, authorization/coverage/settlement status, effective/decision time, reason, actor, dan audit timestamps.
6. Payer berikutnya memproses eligible residual setelah hasil payer sebelumnya sesuai coordination-of-benefits policy.
7. Partial approval/rejection satu payer tidak membatalkan allocation lain. Residual yang tidak ditanggung menjadi official `Patient Responsibility` kecuali financial adjustment sah.
8. Untuk setiap finalized charge line, total active finalized allocations tidak boleh melebihi eligible charge. Residual tidak boleh hilang secara diam-diam.
9. Perubahan payer memakai version/superseding allocation dengan previous reference, reason, actor, timestamp, evidence, dan status history; histori tidak boleh ditimpa.
10. Perubahan payer tidak mengubah clinical fact atau gross charge secara historis; yang berubah adalah allocation dan financial adjustment yang sah.
11. FOC adalah financial waiver/adjustment, bukan payer. FOC membutuhkan amount, reason, approval, authorization level, actor, timestamp, charge reference, dan audit trail.
12. Membership hanya menjadi payer jika mempunyai fund/program yang benar-benar membayar. Discount/benefit/tariff privilege diproses sebagai pricing benefit sebelum allocation.
13. Billing/Revenue Cycle memiliki allocation, patient responsibility, adjudication result, dan allocation version; Payer Management memiliki eligibility/authorization/guarantee/adjudication facts; Cashier dan Finance mengikuti ownership `RJ-BIL-GATE-DEC-001`.

Financial invariants:

- `Gross Charge - Approved Adjustment - Discount/Benefit = Net Eligible Charge`.
- `SUM(Payer Allocations) + Patient Responsibility = Net Eligible Charge` untuk finalized charge line.
- Settlement tidak boleh menghasilkan over-allocation, silent write-off, payer overwrite, histori hilang, payment tanpa source allocation, atau overpayment tanpa workflow overpayment/refund.

Acceptance criteria tambahan:

1. Coverage dapat dialokasikan berbeda pada setiap charge line dalam folio yang sama.
2. Payer rejection/partial approval menghasilkan residual yang eksplisit dan dapat diteruskan ke payer berikutnya atau patient responsibility.
3. Mengganti payer membuat version baru dan mempertahankan prior decision evidence.
4. FOC tidak muncul sebagai payer allocation.
5. Membership discount mengurangi net eligible charge sebelum payer allocation; funded membership dapat menjadi payer.

Conflict disposition:

`RJ-BIL-REQ-X001` ditutup pada level requirement: as-is encounter payment source one-to-one dipertahankan hanya sebagai primary registration payer, sedangkan multi-payer menjadi ownership Billing. Source tetap belum sesuai dan memerlukan downstream design/implementation terpisah.

## Closure Amendment 2026-08-19 - Laboratory

### `RJ-BIL-GATE-DEC-003`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Laboratory order, specimen, result, cancellation, safety, dan charge eligibility lifecycle |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH STRUCTURAL CLARIFICATIONS`, SHA-256 `4d44472028622f6a9c460f78c4ff61c6fa29d57b6ce5f46f8c6d2b820373c2cc` |
| Formal governance status | `OPEN` - Lab, Clinical Governance, dan Billing/Finance sign-off belum dilampirkan |

Lifecycle requirement yang dikunci:

1. Lab Order: `Draft → Requested → Accepted → InProcess → Completed`; exceptions `OnHold`, `CancelRequested`, `Cancelled`.
2. Specimen: `Planned → Collected → Received → Accepted`; exceptions `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold`.
3. Result: `Pending → InProcess → Completed → Validated → Released`; correction path mempertahankan released history melalui `Corrected/Amended → Revalidated → Released`.
4. `Validated` dan `Released` adalah result states, bukan Lab Order states dan bukan trigger awal billing.
5. Satu Lab Order dapat memiliki lebih dari satu specimen. Recollection membuat specimen identity baru dan mempertahankan specimen sebelumnya serta causal link.

Authority dan safety invariants:

- Dokter memiliki direct mutation sampai `Requested`. Setelah handoff, dokter hanya melihat, menambah informasi klinis, atau mengajukan cancellation/correction.
- Collection, receipt, acceptance/rejection, processing, validation, dan release menggunakan capability berbeda; jabatan organisasi tidak otomatis memberi authority.
- Receipt tidak sama dengan specimen acceptance. Acceptance/rejection merekam decision, actor, time, controlled reason, dan optional note.
- Setiap specimen memiliki identity/barcode operasional sendiri dan deterministic trace ke Lab Order, Encounter, dan Patient; barcode text tidak menggantikan relational identity.
- Normal processing menolak specimen tanpa patient/order identity valid kecuali audited exception workflow disetujui terpisah.
- `OnHold` mempertahankan operational state sebelumnya serta hold/resume reason, actor, dan time.
- Semua transition material menghasilkan immutable history dengan entity/source identifiers, from/to status, action, reason, actor, time, encounter/order/specimen, dan correlation ID.

Billing dan cancellation invariants:

- Untuk specimen-based examination, specimen/order `Accepted` adalah charge eligibility milestone; `Requested`, `Collected`, dan `Received` bukan examination-charge trigger.
- Lab mengirim clinical/consumption facts; Billing menjadi satu-satunya owner charge consequence.
- Cancellation sebelum `Accepted` tidak menghasilkan examination charge. Collection/material yang benar-benar dikonsumsi dinilai terpisah menurut Actual Consumption Rule.
- Cancellation setelah `Accepted/InProcess` tidak menghapus charge otomatis; Lab mencatat operational state/consumption dan Billing menentukan void, partial/full charge, atau adjustment.
- `Rejected` secara default tidak menghasilkan examination charge. Konsumsi terpisah hanya dapat dinilai bila memiliki dasar kebijakan.
- Recollection akibat internal hospital error memakai FOC/write-off/internal adjustment dan tidak dibebankan otomatis kepada pasien.
- Recollection karena patient/specimen condition atau external cause memerlukan reason, authorization, serta dasar clinical/payer policy sebelum charge baru.
- Lab tidak memiliki `Paid`, settlement, payer approval, void, refund, atau reversal authority.

Acceptance criteria tambahan:

1. Order, specimen, dan result tidak berbagi satu destructive status field.
2. Rejected/recollected specimen tetap terlihat dalam history dan tertaut ke replacement specimen.
3. Hanya authorized capability yang dapat melakukan collection, receipt, acceptance/rejection, validation, atau release.
4. Charge eligibility diterbitkan tepat pada valid `Accepted` transition dan tidak bergantung pada result release.
5. Direct doctor cancellation ditolak setelah `Requested`; cancellation request tetap dapat diproses Lab.
6. Internal-error recollection tidak menambah patient responsibility secara otomatis.

Capability disposition:

Existing `LabOrder` tetap dapat menjadi evidence/start point, tetapi capability diklasifikasikan sebagai major `Extend`: current model hanya memiliki `EncounterId`, `ProcedureId`, dan base cancellation tanpa lifecycle guard, specimen, result, charge eligibility, atau actual-consumption integration. Ini bukan izin implementasi.

## Closure Amendment 2026-08-19 - Radiology

### `RJ-BIL-GATE-DEC-004`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Radiology order, study/acquisition, report, safety, repeat/abort, dan charge eligibility lifecycle |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH STRUCTURAL AND SAFETY CLARIFICATIONS`, SHA-256 `3da3a2fdc2854cd6a77e6947fb8454fcb2347cc4899f438ff88e226a39701a40` |
| Formal governance status | `OPEN` - Radiology, Clinical Governance, dan Billing/Finance sign-off belum dilampirkan |

Lifecycle requirement yang dikunci:

1. Order: `Draft → Requested → Accepted → Scheduled → InProgress → Completed`; exceptions/supporting concepts `OnHold`, `CancelRequested`, `Cancelled`, `Rejected`, dan reschedule.
2. Study/acquisition: `Planned → PatientVerified → SafetyCleared → AcquisitionStarted → Acquired → QualityAccepted`; exceptions `OnHold`, `Aborted`, `QualityRejected`, `RepeatRequired`, `Cancelled`.
3. Report: `Pending → Drafted → Validated → Released`; released correction menggunakan versioned `AmendmentDrafted → AmendmentValidated → AmendmentReleased` dan tidak menimpa versi sebelumnya.
4. Order completion berbeda dari report release. Study dan report memiliki identity/version sendiri serta deterministic trace `Patient → Encounter → Order → Study → Report`.

Authority dan safety invariants:

- Dokter memiliki direct mutation sampai `Requested`; setelah handoff, cancellation/correction menjadi controlled request kepada Radiology.
- Radiology memiliki acceptance, scheduling, verification, safety, acquisition, technical quality, repeat/abort, study, reporting, dan amendment facts; tidak memiliki financial status authority.
- Normal `AcquisitionStarted` wajib didahului patient/encounter/order/procedure/modality verification dan seluruh mandatory safety gate yang relevan.
- Safety gate bersifat modality/procedure-specific, termasuk contrast, radiation, MRI device/implant, sedation, atau interventional prerequisites bila relevan; final checklist mengikuti SOP/clinical authority, bukan keputusan dokumen ini.
- Mandatory safety state `Pending/Failed` memblokir normal acquisition. Emergency override hanya boleh ada bila disahkan terpisah dan wajib reason, responsible clinician, authorization, time, serta audit.
- Report validity/content tidak dikendalikan Billing atau payment status. Result-distribution policy, bila ada, harus terpisah dari clinical validity.

Billing, abort, dan repeat invariants:

- `Requested`, `Accepted`, `Scheduled`, dan report release bukan initial charge trigger.
- Normal charge eligibility terjadi setelah actual acquisition menghasilkan usable study. Quality failure masuk exception workflow, bukan otomatis full charge.
- Cancellation sebelum `AcquisitionStarted` menghasilkan examination charge nol secara default; legitimate consumed preparation/material dinilai terpisah.
- `Aborted`/partial acquisition tidak otomatis full void atau full charge. Radiology merekam exposure, contrast, material/BHP, performed portion, usability, cause, actor, dan consumption facts; Billing menentukan outcome.
- Repeat mempertahankan original Study dan membuat Study baru dengan causal classification.
- Internal hospital error repeat tidak menambah patient charge secara otomatis. New clinical requirement memerlukan valid order/additional-order mechanism, reason, actor, linkage, dan authorization.
- Radiology menyerahkan performed, aborted, repeated, cancelled, dan actual-consumption facts; Billing tetap canonical owner charge/adjustment.

Acceptance criteria tambahan:

1. Order, Study, dan Report tidak menggunakan satu destructive status.
2. Acquisition ditolak ketika identity/procedure verification atau mandatory safety gate belum valid.
3. Normal charge eligibility hanya terjadi untuk performed acquisition dengan usable study.
4. Aborted/quality-rejected/repeat study mempertahankan original facts dan causal responsibility.
5. Released report hanya berubah melalui versioned amendment dengan reason, author/validator, time, dan previous-version reference.
6. Radiology tidak dapat mengubah paid, payer approval, waiver, settlement, void, refund, atau reversal.

Capability disposition:

Core Radiology workflow memerlukan bounded operational capability sendiri dan tidak boleh direduksi menjadi generic `PatientProcedure` lifecycle karena membutuhkan modality, scheduling, safety, acquisition, quality, repeat, report/amendment, dan optional RIS/PACS/DICOM identifiers. Shared procedure/tariff serta Encounter/Billing linkage tetap dapat direuse. Catatan historis bahwa actual-consumption calculation diblokir `RJ-BIL-GATE-DEC-005` telah disupersesi oleh decision revision 6.

## Closure Amendment 2026-08-19 - Actual Consumption

### `RJ-BIL-GATE-DEC-005`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Component-based actual-consumption calculation, partial service, correction, traceability, dan rounding |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH FINANCIAL CALCULATION CLARIFICATIONS`, SHA-256 `e16a812ae92ee2c7214663d365acf73c60065f6562188345da17f2ce85490dcd` |
| Formal governance status | `OPEN` - Billing/Finance dan service-owner sign-off belum dilampirkan |

Core calculation requirement yang dikunci:

1. Clinical Unit memiliki fakta aktual; Billing memiliki keputusan dan kalkulasi finansial.
2. Pelayanan dipecah menjadi measurable charge components, termasuk professional, facility/equipment, procedure, drug, reagent, contrast, BHP/material/device, administration yang sah, dan authorized other component.
3. Material terukur memakai `ActualQuantity × ApplicableUnitPriceSnapshot`. Clinical facts tidak mengirim nominal Rupiah final.
4. Billing menyimpan immutable tariff/version/effective-date/unit-price/currency/pricing-context/calculation-date snapshot; historical charge tidak dihitung ulang dari mutable master.
5. Patient billable price berbeda dari inventory/acquisition cost atau COGS.
6. Service belum dimulai menghasilkan zero service charge. Completed service memakai applicable full charge.
7. Partial service hanya dihitung melalui active approved service/catalog rule; generic percentage, full-charge, full-void, time, atau milestone fallback dilarang.
8. Partial non-material tanpa active approved rule wajib menjadi `PendingFinancialReview` pada affected component/line, bukan otomatis seluruh folio.
9. Folio tidak dapat `FullySettled/Closed` selama mandatory component masih unresolved kecuali authorized exception policy berlaku.

Rule governance dan causality:

- Reusable partial rule memiliki service/catalog, component, version, effective period, formula type/parameters, rounding, optional minimum/maximum yang sah, approval evidence, dan active status; penerapan retroaktif diam-diam dilarang.
- Formula yang dapat didukung hanya bila disahkan: actual quantity, milestone/step, time-based, fixed partial, atau explicit percentage.
- Consumption cause dipisahkan dari payer/financial disposition. Cause minimal: `InternalHospitalError`, `PatientOrClinicalCondition`, `NewClinicalRequirement`, `ExternalOrOther`.
- Financial disposition seperti covered, partial, payer rejection, patient responsibility, waived, atau internal write-off berada pada axis terpisah.
- `InternalHospitalError` tetap merekam actual quantity, tetapi additional cost akibat error tidak menjadi patient responsibility; Billing memakai FOC/write-off/internal adjustment sesuai governance.
- Actual consumption tidak otomatis berarti patient liability.

Correction, review, dan audit invariants:

- Clinical fact correction membuat version/superseding fact; financial correction membuat versioned adjustment. Original fact dan charge tidak dioverwrite.
- Setiap calculation dapat direkonstruksi dari source fact/version, encounter/service/component, quantity/unit, tariff snapshot, rule/version, raw amount, rounding/version/difference, final amount, cause, actor/time, dan correlation ID.
- Rounding dilakukan per charge component menggunakan centrally configured/versioned Billing rule sebelum aggregation; rounding difference disimpan.
- Manual review tidak mengizinkan arbitrary amount: wajib source facts, reason, prior state, decision/final amount, policy reference, reviewer, time, comment, dan approver bila threshold mensyaratkan.
- Keputusan manual satu kasus tidak otomatis menjadi reusable formula.
- Stable source fact/version/component reference wajib mencegah duplicate charge pada event replay.

Acceptance criteria tambahan:

1. Partial non-material tanpa rule tetap pending dan tidak mengubah komponen final lain.
2. Historical calculation tetap reproducible setelah master tariff/rule berubah.
3. Internal-error consumption terlihat sebagai clinical/costing truth tanpa menambah patient responsibility otomatis.
4. Correction menghasilkan delta/adjustment yang tertaut ke original, bukan destructive update.
5. Event/fact replay tidak menghasilkan charge component kedua untuk source/version/type yang sama.
6. Folio closure ditolak ketika mandatory `PendingFinancialReview` belum diselesaikan tanpa authorized exception.

Configuration boundary:

Actual Consumption core model siap untuk domain design. Formula per service/catalog tetap membutuhkan configuration/governance evidence; ketiadaan formula ditangani secara eksplisit melalui `PendingFinancialReview`, bukan blocker untuk core model dan bukan izin menebak nilai.

## Closure Amendment 2026-08-19 - Financial Governance

### `RJ-BIL-GATE-DEC-006`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Financial capabilities, maker-checker, approval lifecycle, thresholds, fail-closed, close/reopen, dan correction governance |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH FINANCIAL GOVERNANCE CLARIFICATIONS`, SHA-256 `f9125f5eba1dd0350309933a6ff0bf0a286c93135dabeb316a8fff7e293cc685` |
| Formal governance status | `OPEN` - Finance, Security/Privacy, dan delegated executive sign-off belum dilampirkan |

Capability dan ownership requirement:

1. Financial capabilities dipisahkan untuk charge create/finalize, adjustment, void create/approve, reversal create/approve, refund create/approve/execute, waiver create/approve, write-off create/approve, financial-review resolve/approve, manual override, serta folio close/reopen.
2. Billing posting/finalization berbeda dari `Finance.Accounting.Post` atau GL posting.
3. Valid deterministic system charge boleh efektif tanpa approval per transaksi bila source fact, approved/effective rule, tariff snapshot, system actor, time, audit, correlation, dan idempotency valid.
4. Draft, expired, future, unapproved, atau unversioned rule tidak boleh menghasilkan final charge.
5. Organizational title tidak di-hardcode; role/organization mapping memberi capability melalui authorization administration.

Maker-checker dan high-risk invariants:

- Maker dan checker wajib berbeda authenticated/effective `UserId`. Delegation tidak boleh membuat orang efektif yang sama menjadi keduanya.
- Memiliki create dan approve capability sekaligus tidak memberi self-approval.
- Void/reversal terhadap `Paid`, `Posted`, `Claimed`, atau `Settled`; refund settled payment; closed-folio reopen; dan cross-encounter correction selalu high-risk tanpa memandang nominal.
- Waiver, write-off, manual review/override, discretionary adjustment, dan action lain mengikuti versioned/effective threshold policy yang dapat mempertimbangkan amount, percentage, risk, transaction/payer/service type, serta hospital/unit.
- Checker hanya approve, reject, atau return for revision. Material change menghasilkan maker revision baru; request lama immutable.
- Approval terjadi sebelum financial mutation efektif. Pending approval tidak mengubah canonical financial state.

Approval policy dan fail-closed invariants:

- Approval record menelusuri request/action, maker/checker, requested/approved amount, impact, prior/requested/final status, reason/decision, policy/version, evidence, encounter/folio/charge references, correlation, dan timestamps.
- Lifecycle minimum: `Draft → Submitted → PendingApproval → Approved`, dengan `Rejected`, `ReturnedForRevision`, `Cancelled`, dan optional `Expired`; expired tidak berarti approved.
- Tidak adanya checker, valid policy/threshold, atau determinable authority mempertahankan `PendingApproval` atau `BlockedByPolicyConfiguration`; SLA hanya memicu escalation dan tidak pernah authorization bypass.
- Fail-closed hanya berlaku pada high-risk/approval-required operation dan tidak memblokir normal deterministic charge yang sah.
- Finance memiliki threshold/authority policy yang versioned, effective-dated, approved, auditable, dan non-destructive. Historical request tetap mereferensikan policy version asal.

Correction dan execution invariants:

- Void, reversal, dan refund berbeda secara semantik, lifecycle, approval, audit, dan accounting consequence.
- Waiver/write-off/manual override tidak menghapus original charge; financial effect diterapkan melalui approved adjustment/action.
- Folio close memerlukan seluruh mandatory financial prerequisites. Reopen selalu controlled high-risk request dan mempertahankan closing history.
- Cross-encounter correction dilarang mengubah `EncounterId` pada original charge; gunakan approved reversal/reallocation yang menelusuri source/target encounter, folio, charge, reason, maker/checker, approval, dan correlation.
- Approved execution wajib idempotent serta me-revalidate current target state. State conflict menghasilkan controlled revalidation state, bukan blind execution.

Acceptance criteria tambahan:

1. Self-approval gagal walaupun user memiliki create dan approve capabilities.
2. High-risk action bernilai kecil tetap memerlukan maker-checker.
3. Checker material edit tidak dapat disetujui sebagai request lama.
4. Invalid/missing approval policy tidak memakai default approver/threshold dan tidak memblokir normal deterministic charge.
5. Replayed approved request tidak menghasilkan duplicate refund/reversal/waiver/write-off/adjustment.
6. Cross-encounter correction mempertahankan original encounter and charge history.

Configuration boundary:

Core financial capability, approval, maker-checker, threshold-policy, close/reopen, dan cross-encounter correction models siap untuk domain design. Nilai threshold dan approval matrix final tetap membutuhkan Finance configuration/approval; high-risk operation tanpa valid policy harus fail-closed.

## Closure Amendment 2026-08-19 - Pharmacy and Financial State

### `RJ-BIL-GATE-DEC-007`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Pharmacy clinical ownership, read-only financial projection, dispensing prerequisite, outage, dan compatibility migration |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH OWNERSHIP AND INTEGRATION CLARIFICATIONS`, SHA-256 `d86aba5b592790a1ba592a6136b8aa2fec7bd190c8d8b7d818fac9a46cf8885b` |
| Formal governance status | `OPEN` - Pharmacy, Billing/Payer, dan Clinical Governance sign-off belum dilampirkan |

Ownership requirement:

1. Pharmacy memiliki prescription clinical lifecycle, review, verification/clarification, preparation/compounding, fulfillment, actual prepared/dispensed/returned/wasted quantities, substitution, cancellation, dan responsible actor/timestamps.
2. Billing/Payer/Cashier/Finance memiliki seluruh canonical financial state sesuai `RJ-BIL-GATE-DEC-001`.
3. Pharmacy dapat menyimpan read-only financial projection dengan financial reference/status/version, source domain/event, source update time, dan sync time; projection bukan financial source of truth.
4. Pharmacy endpoint dilarang melakukan canonical financial mutation.
5. Existing `MarkBillingGenerated`, `MarkPaid`, `MarkInsuranceApproved`, dan `MarkPaymentWaived` disupersesi. Temporary legacy endpoint hanya boleh menjadi restricted compatibility adapter yang memanggil canonical Billing/Payer contract dan tidak memutasi independent Pharmacy financial truth.

Workflow dan cancellation invariants:

- Financial prerequisite untuk dispensing bersifat approved-policy-driven berdasarkan payer/service/patient/medication/emergency/guarantee context dan tidak universal `Paid` hardcode.
- `ReadyForPharmacy` dipisah menjadi clinical readiness, applicable financial prerequisite, dan Pharmacy operational readiness.
- Valid clinical urgency exception dapat melanjutkan service ketika financial prerequisite unresolved, tetapi wajib approved policy, reason, authorizing actor, time, encounter/prescription, financial state, dan audit.
- Clinical urgency override bukan `PaymentWaived` dan tidak menghapus financial obligation.
- Prescription cancellation/partial dispensing mengirim clinical/operational and actual-consumption facts; Billing menentukan no charge, void, adjustment, full charge, refund/reversal, atau write-off.
- Pharmacy tidak menghitung nominal canonical dari current price.

Outage, projection, dan reconciliation invariants:

- Billing/Payer unavailable menghasilkan explicit `Unknown`, `PendingVerification`, atau `Stale`; unknown bukan paid maupun unpaid.
- Tanpa authorized clinical exception, Pharmacy mengikuti approved downtime/hold policy dan tidak membuat financial assumption.
- Downtime clinical exception mencatat exception type/reason/authorizer/time, last-known financial state, source availability, prescription/encounter, serta wajib direkonsiliasi setelah recovery.
- Projection sync idempotent, version-aware, auditable, reconcilable, dan menolak stale/out-of-order version menimpa versi baru.
- Billing/Payer canonical truth memenangkan financial projection conflict; Pharmacy canonical clinical/dispensing facts tidak boleh ditimpa oleh financial reconciliation.

Acceptance criteria tambahan:

1. Pharmacy tidak dapat membuat prescription terlihat paid/approved/waived tanpa canonical financial transaction.
2. Legacy financial endpoint merutekan ke canonical owner dan tidak menulis financial truth langsung pada Pharmacy.
3. `FinancialStatusUnknown` tidak diperlakukan sebagai paid atau unpaid.
4. Incoming older financial version tidak menimpa Pharmacy projection yang lebih baru.
5. Paid status tidak membuat `Dispensed=true`, dan dispensed status tidak membuat `Paid=true`.
6. Authorized urgent dispensing tetap mempertahankan outstanding financial workflow.

Migration disposition:

Pharmacy clinical/fulfillment capability adalah reuse/extend. Existing financial mutation adalah ownership redesign. Existing payment fields boleh dipertahankan sementara hanya sebagai compatibility projection/cache; tidak boleh ada dua canonical owners. Physical persistence dan deprecation sequencing diputuskan downstream, bukan pada closure pass.

## Closure Amendment 2026-08-19 - Reliability and Reconciliation

### `RJ-BIL-GATE-DEC-008`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Fact identity, idempotency, retry/failure outcomes, partial success, reconciliation, downtime recovery, dan folio closure gates |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH RELIABILITY AND RECONCILIATION CLARIFICATIONS`, SHA-256 `7ffa205e209d7818bb29563fa8d327b34dac1bf6d5c86f230b2ab40457854100` |
| Formal governance status | `OPEN` - Billing/Finance dan integration owner sign-off belum dilampirkan |

Fact identity dan processing invariants:

1. Setiap clinical fact memiliki stable source fact ID/version/domain/entity, encounter, occurred time, correlation, dan idempotency key. Retry tidak membuat identity baru.
2. Durable, restart-safe, concurrency-safe idempotency record mengikat source/version/operation ke satu processing outcome/result reference.
3. Duplicate/replay/concurrent duplicate mengembalikan existing result dan tidak membuat duplicate charge, component, adjustment, void, reversal, refund, atau financial event.
4. Validation rejection final untuk fact version tersebut; incorrect source fact diperbaiki melalui newer superseding version. Valid source fact yang gagal diproses tetap di-retry dengan version yang sama.
5. Processing outcome dipisahkan dari financial status dan mendukung `Received`, `Processing`, `Applied`, `PartiallyApplied`, `RejectedValidation`, `TransientFailure`, `PermanentFailure`, `OutcomeUnknown`, `PendingReconciliation`, dan `Reconciled` atau vocabulary setara.

Timeout, retry, dan exception invariants:

- Timeout/response loss menghasilkan `OutcomeUnknown`, bukan assumed failure/success. Recovery memakai original identity melalui status query; retry hanya dilakukan dengan key yang sama setelah outcome diverifikasi.
- Transient failure dapat retry dengan bounded/backoff policy. Permanent failure berhenti dari automatic retry dan masuk controlled exception/dead-letter review.
- Dead letter bukan resolution; case tetap mempunyai failure reason, owner, attempts, next action, resolution status, dan correlation.
- Missing Billing configuration dapat diperbaiki lalu fact valid/version yang sama diproses ulang; source-data correction membutuhkan fact version baru.

Partial, late, dan correction invariants:

- Partial success dicatat pada charge-component level; independent component yang applied tidak dihapus karena component lain gagal.
- Failed component tidak dianggap zero dan harus explicit pending/failure/reconciliation state.
- Atomic business dependency harus dinyatakan eksplisit; tidak semua component dipaksa satu business rollback.
- Older/stale/out-of-order version tidak menimpa applied newer version. Valid newer correction menghasilkan versioned adjustment dan mempertahankan original charge.

Reconciliation dan closure invariants:

- Reconciliation membandingkan canonical clinical facts dengan canonical Billing state untuk missing/orphan facts, amount/quantity/version mismatch, duplicate charge, missing adjustment, stale projection, unknown outcome, partial failure, dan unresolved emergency exception.
- Reconciliation tidak memutasi clinical truth agar cocok dengan financial state. Correction tetap ditentukan pada owning domain dan versioned.
- Non-deterministic mismatch membuat reconciliation case dengan type, source/version, billing/encounter reference, impact, status, owner, priority/risk, age/SLA, actions, resolution, dan audit.
- Deterministic no-impact duplicate dapat auto-resolve. Manual financial outcome tetap tunduk pada `RJ-BIL-GATE-DEC-006`.
- Folio close diblokir oleh mandatory `OutcomeUnknown`, pending reconciliation/approval/review, financially material permanent failure, failed mandatory component, unresolved allocation, atau exception. Authorized closure exception harus explicit, auditable, dan governed.
- SLA breach hanya memicu warning/escalation/priority/visibility dan tidak pernah auto-approve, write-off, charge, void, atau resolve.

Downtime recovery invariants:

- Clinical modules mempertahankan durable pending-delivery/backlog facts dengan stable identity/version/key.
- Recovery memproses backlog secara version/dependency-aware dan idempotent, tanpa mengandalkan arrival order atau distributed transaction.
- Recovery menghasilkan operational report berisi counts per outcome, affected encounter/folio, unresolved impact, owners, dan next actions.
- Billing menyediakan canonical processing-status lookup berdasarkan stable source identity dengan outcome/result/applied version/financial references/failure/reconciliation case.

Acceptance criteria tambahan:

1. Response loss setelah committed charge diselesaikan melalui status query dan tidak membuat charge kedua.
2. Concurrent duplicate fact menghasilkan tepat satu canonical processing result.
3. Invalid payload correction memakai version baru; infrastructure retry memakai version yang sama.
4. Partial applied components tetap final sementara failed component visible dan unresolved.
5. Folio close ditolak ketika mandatory reconciliation exception belum selesai tanpa authorized closure exception.
6. Recovery report mengidentifikasi seluruh unresolved encounter/folio dan assigned next action.

Readiness disposition:

Clinical fact identity, Billing idempotency, `OutcomeUnknown`, retry/exception, partial component processing, reconciliation, downtime backlog, dan folio reconciliation gates siap untuk domain design. Exact persistence and API mechanisms tetap downstream decisions.

## Closure Amendment 2026-08-19 - Payer and Claim Integration

### `RJ-BIL-GATE-DEC-009`

| Field | Value |
|---|---|
| Type | Decision |
| Item | Internal payer orchestration, authorization/claim lifecycles, external adapters, manual release scope, dan production gates |
| Status | `locked-draft` |
| Approval evidence | User response berlabel `APPROVED WITH PAYER INTEGRATION AND RELEASE-SCOPE CLARIFICATIONS`, SHA-256 `d9fc83e18bdc5a209fa152b83eae733b090f62ddd8284836dc565efa49450e73` |
| Formal governance status | `OPEN` - Payer/Insurance, Billing/Finance, Security, dan integration owner sign-off belum dilampirkan |

Ownership dan lifecycle requirement:

1. Internal Payer/Insurance Management menjadi canonical orchestrator eligibility, coverage verification, authorization, guarantee, coordination of benefits, claim preparation/submission/status, adjudication, payer allocation outcome, reconciliation, dan external integration state.
2. Clinical, Billing, Payer, Cashier, dan Finance ownership tetap terpisah; external response tidak langsung memutasi clinical facts, prescription, diagnostic orders/studies, procedure, encounter, atau charge database.
3. Eligibility berbeda dari service authorization. Authorization lifecycle: `Draft → Submitted → Pending → Approved`, dengan `PartiallyApproved`, `Rejected`, `Expired`, `Cancelled`, dan optional `NeedMoreInformation`; semua versioned.
4. Claim processing dipisahkan dari settlement. Processing: `Draft → Ready → Submitted → Acknowledged → InAdjudication`, dengan need-info/approved/partial/rejected/cancelled. Settlement: `NotDue → PaymentPending → PartiallyPaid → Paid → Reconciled`, dengan dispute/mismatch/reconciliation exceptions.
5. Claim approval tidak sama dengan payment. Payer rejection tidak menghapus charge; partial/rejected residual mengikuti coordination-of-benefits dan patient responsibility.

Adapter dan reliability invariants:

- Setiap named external system memakai dedicated adapter ke normalized internal contract; core domain tidak berisi payer-specific branching.
- Adapter capability profile menyatakan dukungan idempotency, status query, cancellation, amendment, partial approval, realtime eligibility/authorization, dan claim submission.
- Outbound request memiliki stable internal/version/external-system identity, idempotency key, correlation, encounter/folio/authorization/claim references, dan time.
- Timeout menjadi `OutcomeUnknown`; same-identity query/reconciliation/retry rules mengikuti `RJ-BIL-GATE-DEC-008`. Payer outage tidak otomatis approved/rejected.
- External decision dinormalisasi dan versioned sebelum menghasilkan allocation adjustment; external business rejection berbeda dari integration failure.
- Audit menyimpan metadata/reference/hash/code/amount/time/actor/correlation, bukan raw sensitive payload. Raw evidence, bila wajib, menggunakan secured storage dengan access, encryption, masking, retention, dan access audit.

Manual workflow dan governance:

- Manual payer decision sah bila transparan sebagai `ManualOperator`, bukan synthetic external API success.
- Manual record memerlukan payer/reference, decision/amount/reason, evidence, decision/record times, actor, dan audit.
- Discretionary/high-risk manual financial impact tunduk pada `RJ-BIL-GATE-DEC-006`.
- Payer reconciliation memakai `RJ-BIL-GATE-DEC-008`; financial consequence memakai `RJ-BIL-GATE-DEC-006`.

Release scope yang dikunci:

1. Release 1 production core mencakup Internal Payer/Insurance Management, cash/patient, manual commercial insurance, manual corporate guarantee, optional BPJS/JKN payer master/context, multi-payer allocation, patient responsibility, authorization/guarantee recording, manual claim/adjudication/settlement/reconciliation, evidence/audit, maker-checker, dan adapter interfaces.
2. AdMedika adalah priority external adapter candidate, tetapi production activation tetap gated sampai exact product/system, owner, contract/version, credentials/security, sandbox/UAT, idempotency/status-query, dan reconciliation evidence tersedia.
3. BPJS/JKN adalah conditional external candidate. Activation membutuhkan verified hospital participation scope, owner, credentials, official interface/version, sandbox/UAT, and reconciliation evidence.
4. BPJS operational/administrative flow tidak disatukan sembarangan dengan E-Klaim/INA-CBG/claim interoperability family.
5. Unnamed insurer/corporate/TPA APIs tidak termasuk direct Release 1 integration dan memakai internal manual workflow.
6. Promotion mengikuti named candidate → verified contract → sandbox → UAT → reconciliation proven → production approval → adapter activation tanpa mengubah canonical payer domain.

Production integration gate:

External adapter fail-closed sampai named system, business/technical owners, API contract/version/environment, credential/security/certificate authority, sandbox/UAT evidence, idempotency/duplicate/status-query/timeout/retry/error mapping, reconciliation owner, support escalation, dan cutover approval tersedia.

Acceptance criteria tambahan:

1. Approved claim tanpa received payment tetap `PaymentPending` dan bukan `Paid`.
2. External rejection mempertahankan charge dan menghasilkan versioned residual allocation.
3. Manual outcome tidak dilabeli sebagai external integration success.
4. Timeout mempertahankan unknown state dan original request identity.
5. Adapter tanpa critical production evidence tetap disabled, sementara manual core workflow tetap beroperasi.
6. External mismatch membuat reconciliation case dan tidak melakukan silent overwrite.

## Closure Pass Result 2026-08-19

Seluruh owner-dependent requirement blockers `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` telah dijawab dan dicatat sebagai `LOCKED-DRAFT`. Status ini mengunci requirement intent untuk assessment berikutnya, tetapi belum merupakan formal joint clinical-financial-security approval karena nama, jabatan, delegasi, dan sign-off evidence belum dilampirkan.

## Approval Amendment 2026-08-20 - Release 1 Internal/Manual

Pengguna memberikan instruksi eksplisit: `DRAFT_FOR_OWNER_APPROVAL -> OWNER_APPROVED -> IMPLEMENTATION_AUTHORIZED -> DELIVERY_PLANNING_ALLOWED`.

Disposition:

1. `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` dipromosikan dari historical `LOCKED-DRAFT` menjadi `OWNER_APPROVED` untuk kontrak Release 1 internal/manual `RJ-BIL-CONTRACT-001` version `1.0.0`.
2. Blueprint Release 1 internal/manual menjadi `IMPLEMENTATION_AUTHORIZED` dan boleh diproses oleh `plan-module-delivery`.
3. Authorization ini tidak memasukkan named external adapters, tidak menetapkan nilai formula/threshold/SOP yang belum tersedia, dan tidak menyatakan production activation.
4. Source implementation tetap memerlukan satu approved roadmap Task ID, repository write authority, dan execution-time preflight dari builder terkait.
5. Historical labels `LOCKED-DRAFT` di bagian sebelumnya dipertahankan sebagai rekam status pada saat masing-masing jawaban diterima; amendment ini adalah status efektif terbaru.

Next required step adalah menjalankan ulang `requirement-completeness-gate` terhadap decision revision 10. Domain architecture tidak boleh menganggap assessment revision 1 otomatis current setelah perubahan material ini.
