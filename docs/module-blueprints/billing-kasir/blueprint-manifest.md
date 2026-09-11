# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
revision: 1.1
revision_note_field_vs_prose: >
  KETIDAKSESUAIAN YANG SUDAH DIKETAHUI DAN KINI DITUTUP. Sebelum revisi 1.0, field `revision`
  bernilai 0.8 sementara badan dokumen ini beserta seluruh berkas kontrak sudah menyebut revisi
  0.9 (amendment BKC-DES-026/027, approved 5 September 2026). Sebabnya: revisi 0.9 dikerjakan
  sebagai amendment susulan yang menaikkan contract_versions dan menulis prosa, tetapi field
  `revision` tidak ikut dinaikkan. Pass 7 September 2026 menaikkan field ini langsung ke 1.0,
  yang menaungi BAIK amendment 0.9 yang sudah approved MAUPUN rumpun baru Petty Cash. Angka 0.9
  DILEWATI sebagai nilai field — bukan karena isinya batal, melainkan karena isinya sudah
  approved dan sudah tercermin pada contract_versions. Tidak ada isi yang hilang.
status: approved
status_derivation: >
  Baseline 0.5 beserta seluruh amendment sampai revisi 0.9 tetap `approved`. Rumpun BARU Petty
  Cash (revisi 1.0) DISETUJUI 7 September 2026 lewat `PC-DEC-014`–`015` (00-interview-decisions.md)
  — `PC-OQ-001` (penamaan MstPettyCashCategory vs BilPettyCashCategory) ditutup memilih
  `MstPettyCashCategory`, dan `PC-DES-001`–`014` disetujui penuh. Kedua kelompok kini sama-sama
  `approved` di dalam satu blueprint SINGLE.
readiness: >
  DESIGN_APPROVED untuk seluruh rumpun sampai Petty Cash (revisi 1.0) — kontrak terkunci;
  wewenang tulis backend/frontend tetap terpisah (`BKC-GATE-09`/`BKC-OQ-092`). Satu prasyarat
  implementasi TERSISA sebelum file model Petty Cash pertama ditulis (BUKAN blocker perencanaan):
  `PC-OQ-003`, baris registry kepemilikan modul untuk folder `PettyCash/` (`QBE-MOD-003`).
  RUMPUN BARU revisi 1.1 (Edit Tagihan & Multi-Payer Coverage) kini DESIGN_APPROVED:
  keputusan bisnis `MPY-DEC-001`–`012` dan keputusan arsitektur `MPY-DES-001`–`017` SELURUHNYA
  `approved` 11 September 2026. Pertanyaan memblokir `MPY-OQ-004` DITUTUP oleh `MPY-DEC-011`.
  Keempat gelombang `MVP-16`–`MVP-19` siap diteruskan ke /plan-module-delivery.
  Prasyarat implementasi yang TERSISA (BUKAN blocker perencanaan): otorisasi terpisah untuk
  membuat dan menjalankan migration lima tabel baru; pemeriksaan `MPY-OQ-005` sebelum `MVP-18`;
  pengisian master aturan tanggungan `MPY-OQ-006` sebelum fitur diaktifkan; dan koordinasi urutan
  commit `MPY-CQ-03` dengan pekerjaan "Payment Reminder".
approved_by_revision_0_8: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) — mengunci seluruh dokumen kontrak, 4 September 2026
approved_at_revision_0_8: 2026-09-04
baseline_revision: 0.5
baseline_status: approved
blueprint_shape: SINGLE
shape_decided_by: EXISTING_STRUCTURE_PENDING_RATIFICATION
shape_evidence: >
  Bentuk tidak pernah tercatat eksplisit pada manifest revisi mana pun, tetapi strukturnya
  sudah SINGLE sejak revision 0.2 — tidak ada satu pun folder sub-modul (tidak ada
  blueprint-manifest.md di dalam folder anak), dan enam revisi berturut-turut memakai satu
  himpunan artefak tingkat modul. Uji pemecahan dijalankan ulang pada pass ini dan hasilnya
  memperkuat SINGLE; rinciannya di bawah. Nilai ini MENCATAT struktur yang sudah terwujud,
  BUKAN pilihan baru yang diambil agent. Ratifikasi pemilik modul diminta lewat BKC-OQ-091.
owners:
  product_domain: Billing Owner
  finance: Finance/AR/AP Owner
  tax: Finance/Tax Owner
  payer_insurance: Payer/Insurance Owner
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Frontend Owner
approved_by: null
approved_at: null
baseline_approved_by: Product/Domain Owner (user approval in conversation) — BKC-DEC-062 tanpa konfirmasi terpisah Payer/Insurance+Finance/AR, lihat caveat 00-interview-decisions.md
baseline_approved_at: 2026-09-02T13:53:34+07:00
created_at: 2026-08-20T11:22:42+07:00
updated_at: 2026-09-11T00:00:00+07:00
last_revision_note_revision_1_1: >
  Revisi 1.1 (11 September 2026) adalah pass desain PENUH untuk satu RUMPUN BARU: Edit Tagihan &
  Multi-Payer Coverage. Isinya: koreksi penanggung kunjungan sebelum pembayaran (`MPY-DEC-003`),
  penanggung per baris biaya (`MPY-DEC-004`), penebusan obat rawat jalan/IGD/OTC (`MPY-DEC-009`),
  aturan tanggungan dan rute reimbursement perusahaan penjamin (`MPY-DEC-008`), serta lembar
  tagihan perusahaan (`MPY-DEC-006`).
  Turunannya: 5 tabel baru, 1 migration, 10 endpoint baru, 2 Resource hak akses baru, 2 butir
  menu baru, 3 flowchart baru, `MPY-DES-001`–`017`, `BIL-VAL-059`–`097`, `BIL-AT-081`–`100`,
  `EPIC BKC-13`–`17`, `FR-BKC-064`–`086`, `UAT-43`–`54`, gelombang `MVP-16`–`MVP-19`.
  BERBEDA dari Petty Cash: rumpun ini MENYENTUH mesin kalkulasi (sumbu `BIL-CALCULATION` naik ke
  0.9) dan punya permukaan lintas modul terlebar di modul ini — satu layanan baru WAJIB dibangun
  di `RegistrationManagement` (`MPY-DES-004`), menunggu persetujuan Muhammad Hamzah
  (`MPY-DEC-010`, `MPY-OQ-004`).
  Yang TIDAK berubah: nol kolom pada tabel yang sudah ada, nol endpoint existing berubah bentuk,
  nol status invoice baru, dan `ExcessAmount`/`ExcessStatus` TETAP terkunci nol (`BKC-DES-014`)
  karena `MPY-DEC-001` mempertahankan satu payer aktif per kunjungan.
  Satu perubahan NILAI yang MUST disosialisasikan: kunjungan berpenjamin perusahaan yang selama
  ini menghasilkan anomali `INSURANCE_PROVIDER_MISSING` dan membebankan seluruh biaya ke pasien
  kini menghasilkan perhitungan tanggungan yang sebenarnya — angka tagihan kunjungan seperti itu
  AKAN berubah, dan itulah perbaikannya.
  Bentuk blueprint TIDAK berubah: tetap SINGLE, mengikuti preseden Petty Cash (`MPY-DEC-002`).
last_revision_note: >
  Revisi 1.0 (7 September 2026) adalah pass desain PENUH untuk satu RUMPUN BARU: Petty Cash
  (Voucher Kas Kecil). Ini berbeda dari revisi 0.6-0.9 yang seluruhnya amendment atas rumpun
  yang sudah ada.
  Isinya: siklus hidup voucher lima status (PC-DEC-013), satu kolam anggaran dengan saldo
  berjalan (PC-DEC-002/010), penjaga saldo pada persetujuan dan pencairan (PC-DEC-008/009),
  data induk kategori yang dikelola Finance (PC-DEC-012), dan jejak perintah yang tahan lama
  (PC-DEC-003). Turunannya: 5 tabel baru, 1 migration, 11 endpoint pada 3 grup Tags baru,
  3 Resource hak akses baru, 3 butir menu baru, dan 2 flowchart baru.
  Yang TIDAK disentuh: seluruh rumpun sebelumnya. Petty Cash tidak menyentuh satu pun berkas
  source maupun tabel yang dirancang revisi 0.4-0.9, dan secara khusus TIDAK terhubung ke kas
  fisik Shift Kasir (PC-DEC-001) maupun ke tagihan pasien.
  Bentuk blueprint TIDAK berubah: tetap SINGLE. Uji pemecahan tidak dijalankan ulang; keputusan
  menaruh Petty Cash sebagai rumpun di dalam struktur SINGLE yang sudah ada sudah diambil pada
  00-interview-decisions.md amendment 7 September 2026, mengikuti preseden Shift Kasir, Diskon,
  Deposit, Refund, dan Pengecualian Finansial.
backend_commit_sha: d295c4d59b68d223edc597c8b165b7ef4282b49f
frontend_commit_sha: 0eafa76bf397a47ceb9d44a6f69006ee25f8ba51
previous_backend_commit_sha: dd31bc91818566c0b53e1b68c0129f5a6cf01a2b
previous_frontend_commit_sha: 12f9242ce62e4d80dbdb719f80bb0e7a2848474c
sha_move_note_revision_1_1: >
  Kedua SHA naik pada revisi 1.1. Impact scan untuk pergerakan ini SUDAH dijalankan dan hasilnya
  ada di 01-existing-capability-map.md § 19, diaudit persis pada d295c4d5/0eafa76b — tetapi scan
  itu DIBATASI pada sembilan klaster yang relevan bagi rumpun Edit Tagihan & Multi-Payer
  (`CAP-33`–`CAP-41`), BUKAN audit ulang section 1-18.
  Satu temuan pergerakan SHA yang MUST dibaca sebelum implementasi: dokumentasi internal modul
  ini (`erd/00-context-erd.md` dan sebagian prosa lama) masih memakai nama `Trx*` untuk entity
  yang di source sudah di-rename menjadi `Reg*` — khususnya `RegPatientEncounterGuarantor`.
  Selisih itu ditemukan saat wawancara rumpun ini, dikoreksi pada 00-interview-decisions.md, dan
  dicatat sebagai temuan pemeliharaan dokumen yang perapiannya milik /manage-module-blueprint.
working_tree_uncommitted_revision_1_1:
  backend:
    - Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
    - Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs
    - Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs
    - Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs
    - Repositories/ApplicationDbContext.cs
    - Services/Logging/LoggerService.cs
    - Areas/HealthServices/BillingManagement/Billing/Dtos/CashierBillingInvoiceDtos.cs (baru, belum ter-track)
    - Areas/HealthServices/BillingManagement/Billing/Models/BilPaymentReminder.cs (baru, belum ter-track)
    - Areas/HealthServices/BillingManagement/Billing/Services/BillingReminderService.cs (baru, belum ter-track)
    - Repositories/Configurations/HealthServices/BillingManagement/Billing/BilPaymentReminderConfiguration.cs (baru, belum ter-track)
  keterangan: >
    Berdasarkan penamaan, kesepuluh berkas ini adalah pekerjaan "Payment Reminder" yang sedang
    berjalan TERPISAH dari rumpun Edit Tagihan & Multi-Payer, dan isinya TIDAK diverifikasi pada
    pass ini karena di luar scope. Yang MUST diperhitungkan: tiga di antaranya
    (`BillingInvoicesController.cs`, `BillingInvoiceDtos.cs`, `BillingInvoiceService.cs`) adalah
    berkas yang rumpun ini juga akan sentuh. Risiko konflik merge NYATA, bukan hipotetis.
    Koordinasikan urutan commit dengan pemilik pekerjaan itu sebelum build-module-backend menulis
    ke ketiga berkas tersebut (`MPY-CQ-03`). JANGAN membuang atau menimpa perubahan itu.
sha_move_note: >
  Kedua SHA naik pada revisi 1.0. Impact scan untuk pergerakan ini SUDAH dijalankan dan
  hasilnya ada di 01-existing-capability-map.md § 18, yang diaudit persis pada dd31bc9 dan
  12f9242c — tetapi scan itu SENGAJA DIBATASI pada permukaan Petty Cash atas instruksi pemilik
  modul, BUKAN audit ulang section 1-17.
  Konsekuensi yang MUST dibaca: bukti as-is untuk rumpun Petty Cash (revisi 1.0) TERKINI pada
  SHA baru. Bukti as-is untuk rumpun-rumpun sebelumnya (revisi 0.6-0.9) masih berpijak pada
  ffeb45a8 beserta working tree yang belum di-commit saat itu, dan BELUM diverifikasi ulang
  terhadap dd31bc9. Jalankan /trace-existing-capabilities impact scan penuh sebelum
  /plan-module-delivery untuk slice revisi 0.6-0.9. Rumpun Petty Cash TIDAK menunggu scan itu:
  ia tidak menyentuh satu pun berkas yang dirancang amendment-amendment tersebut.
working_tree_uncommitted:
  backend:
    - Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs
    - Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs
    - Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs
    - Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs
  frontend:
    - src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx
    - src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js
  keterangan: >
    Keenam berkas ini adalah hasil BE-BKC-FIX-003 dan FE-BKC-FIX-008 (ad-hoc, di luar roadmap,
    4 September 2026) yang BELUM di-commit dan BELUM pernah dibangun sekalipun
    (AUTOMATED TEST: BLOCKED pada kedua laporannya). Desain revision 0.7 membacanya sebagai
    keadaan as-is. Bila perubahan itu dibuang, sebagian besar amendment 4 September 2026
    berubah dari "menyempurnakan yang sudah ada" menjadi "membangun dari nol" — lihat
    BKC-OQ-090.
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
input_revisions:
  decisions: 0.2 (baseline) + amendment BKC-DEC-059-062 (2 Sep 2026, approved) + amendment BKC-DEC-063-069 (3 Sep 2026, approved) + amendment BKC-DEC-070-079 (4 Sep 2026, approved) + amendment BKC-DEC-080-084 (4 Sep 2026, approved) + amendment BKC-DEC-085-087 (4 Sep 2026, approved) + amendment BKC-DEC-088-092 (4-5 Sep 2026, approved) + amendment PC-DEC-001-013 (7 Sep 2026, approved — rumpun Petty Cash) + amendment PC-DEC-014-015 (7 Sep 2026, approved) + amendment MPY-DEC-001-010 (11 Sep 2026, approved — rumpun Edit Tagihan & Multi-Payer, tiga sub-amendment bertanggal sama)
  capability_map: 0.2 (baseline) + impact scan section 16 (2 Sep 2026) + impact scan section 17 (4 Sep 2026) + audit kapabilitas baru section 18 (7 Sep 2026, Petty Cash, CAP-29-CAP-32, diaudit pada dd31bc9/12f9242c) + audit kapabilitas baru section 19 (11 Sep 2026, Edit Tagihan & Multi-Payer, CAP-33-CAP-41, diaudit pada d295c4d5/0eafa76b)
  requirement_gate: 0.3
  hospital_domain_architecture: 0.3
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
domain_architecture_readiness_petty_cash: >
  DOMAIN_ARCHITECTURE_NOT_RUN untuk rumpun Petty Cash, dan itu keputusan yang dicatat, bukan
  langkah yang terlewat. Alasannya: Petty Cash adalah kapabilitas keuangan internal yang tidak
  melintasi bounded context rumah sakit mana pun, tidak menyentuh satu pun data pasien, tidak
  berdampak pada tagihan pasien, dan tidak berdampak pada keselamatan klinis. Kedua titik
  singgung yang berpotensi lintas konteks — kas fisik Shift Kasir dan master pegawai — sudah
  DIPUTUS SECARA EKSPLISIT oleh PC-DEC-001 dan PC-DEC-011, sehingga tidak ada batas domain yang
  tersisa untuk diselesaikan hospital-domain-architect. Nilai DOMAIN_ARCHITECTURE_READY di atas
  tetap berlaku untuk rumpun-rumpun sebelumnya.
input_hashes:
  00-interview-decisions.md: 6d74e4fbe954782895df0c89d441b4a8357dac38e20f945224778ed8440c3414
  01-existing-capability-map.md: b33911fb655fb71beda296affdc2a8980f0c5f0fd05dd4d01301e30c05a866e8
  evidence/02-requirement-completeness-gate.md: ede9101e57dea4615880094ee0e5b3ef6c4755992da68bbfe968eac45a5006ec
  evidence/03-hospital-domain-architecture.md: 5879248de943c18d50e7a955cc23b8e151c3289d01113f109350c37b76de2b06
input_hashes_note: >
  Dihitung ulang pada revisi 1.0 (SHA256, isi berkas apa adanya). Keempat nilai BERUBAH dari
  revisi 0.8. Untuk kedua berkas hulu, perubahannya memang diharapkan: 00-interview-decisions.md
  bertambah amendment PC-DEC-001-013, dan 01-existing-capability-map.md bertambah section 18.
  Untuk kedua berkas evidence, hash lama pada revisi 0.8 TIDAK PERNAH dihitung ulang sejak
  baseline sehingga selisihnya tidak dapat dijelaskan pass ini; nilainya diperbarui apa adanya
  dan dilaporkan sebagai temuan pemeliharaan manifest, bukan sebagai perubahan isi yang
  disengaja pass ini.
design_decision_ids: [BKC-DES-001, BKC-DES-002, BKC-DES-003, BKC-DES-004, BKC-DES-005, BKC-DES-006, BKC-DES-007, BKC-DES-008, BKC-DES-009, BKC-DES-010, BKC-DES-011, BKC-DES-012, BKC-DES-013, BKC-DES-014, BKC-DES-015, BKC-DES-016, BKC-DES-017, BKC-DES-018, BKC-DES-019, BKC-DES-020, BKC-DES-021, BKC-DES-022, BKC-DES-023, BKC-DES-024, BKC-DES-025, BKC-DES-026, BKC-DES-027]
design_decision_ids_petty_cash: [PC-DES-001, PC-DES-002, PC-DES-003, PC-DES-004, PC-DES-005, PC-DES-006, PC-DES-007, PC-DES-008, PC-DES-009, PC-DES-010, PC-DES-011, PC-DES-012, PC-DES-013, PC-DES-014]
design_decision_ids_multi_payer: [MPY-DES-001, MPY-DES-002, MPY-DES-003, MPY-DES-004, MPY-DES-005, MPY-DES-006, MPY-DES-007, MPY-DES-008, MPY-DES-009, MPY-DES-010, MPY-DES-011, MPY-DES-012, MPY-DES-013, MPY-DES-014, MPY-DES-015, MPY-DES-016, MPY-DES-017]
design_decision_status_multi_payer: >
  MPY-DES-001-017: seluruhnya `approved` 11 September 2026 lewat MPY-DEC-012 (Product/Domain
  Owner, wewenang ganda Finance/AR BKC-DEC-085, "Sayapun setuju"). Persetujuan pemilik
  RegistrationManagement atas MPY-DES-004 ditutup terpisah lewat MPY-DEC-011 (Muhammad Hamzah,
  "Muhammad Hamzah dah setuju", disampaikan melalui Product/Domain Owner — lihat catatan
  provenance pada 00-interview-decisions.md).
  Approval ini BUKAN otorisasi membuat maupun menjalankan migration; keduanya tetap memerlukan
  konfirmasi terpisah saat implementasi.
requirement_completeness_gate_multi_payer: >
  NOT_RUN untuk rumpun Edit Tagihan & Multi-Payer, dan itu DEVIASI YANG DICATAT, bukan langkah
  yang terlewat. Sebagai gantinya dipakai: 00-interview-decisions.md (MPY-DEC-001-010, sepuluh
  keputusan `approved` beserta owner dan evidence, termasuk penutupan satu konflik lintas modul
  nyata RWI-ENC-PAYER-001 dan tiga open question sampai tuntas) serta 01-existing-capability-map.md
  § 19 (sembilan kemampuan berbukti source langsung). Pola pencatatan mengikuti
  domain_architecture_readiness_petty_cash di bawah.
domain_architecture_readiness_multi_payer: >
  DOMAIN_ARCHITECTURE_NOT_RUN untuk rumpun ini. Alasannya: seluruh batas lintas konteks yang
  relevan sudah diputus EKSPLISIT oleh keputusan bisnis — kepemilikan payer kunjungan tetap di
  RegistrationManagement (MPY-DEC-007), penempatan kedua master data baru (MPY-DEC-008), dan batas
  terhadap PharmacyManagement (MPY-DEC-009) — sehingga tidak ada batas domain tersisa untuk
  diselesaikan hospital-domain-architect.
design_decision_prefix_convention: >
  Rumpun Petty Cash memakai prefix PC-DES-* , BUKAN melanjutkan sekuens BKC-DES-* tingkat modul.
  Ini mengikuti konvensi per-rumpun yang sudah ditetapkan 00-interview-decisions.md sendiri saat
  memilih PC-DEC-* alih-alih BKC-DEC-* untuk keputusan bisnisnya, dengan alasan yang sama:
  rumpun baru dengan kosakata sendiri di dalam blueprint yang sama. Sekuens BKC-DES-* berhenti
  di BKC-DES-027 dan TIDAK dilanjutkan rumpun ini. Jejak traceability tetap ke blueprint_id
  BIL-CASH-001 yang sama.
design_decision_status: >
  BKC-DES-001–020: approved (kecuali BKC-DES-002 yang superseded, lihat baris di bawah).
  BKC-DES-010–020 approved lewat BKC-DEC-084 (menutup BKC-OQ-082 sebagian) lalu
  BKC-DEC-085 (menutup caveat wewenang Finance/AR sepenuhnya — pengguna mengonfirmasi memegang
  wewenang ganda Product/Domain Owner + Finance/AR). BKC-DES-001, 003–006, 008–009 approved
  blanket lewat BKC-DEC-087. BKC-DES-007 approved khusus (naik dari DEV_DISCRETION) lewat
  BKC-DEC-086. Seluruhnya 4 September 2026 — lihat 00-interview-decisions.md amendment
  "Penutupan caveat Finance/AR dan approval BKC-DES-001–009".
  BKC-DES-021–025 (BARU pada revisi 0.8): approved lewat BKC-DEC-088 (Product/Domain Owner +
  Finance/AR, "saya approve", 4 September 2026) — termasuk BKC-DES-023 (pemicu write-off
  manual) dan BKC-DES-024 (kolom Category pada BilWriteOffCase, satu migration; approval ini
  BUKAN otorisasi membuat/menjalankan migration itu, tetap perlu konfirmasi terpisah saat
  implementasi). Dengan ini SELURUH BKC-DES-001–025 approved (kecuali BKC-DES-002 superseded).
  BKC-DES-026–027 (BARU pada revisi 0.9): approved 5 September 2026 oleh Product/Domain Owner
  ("Saya approve untuk case diatas", wewenang ganda Finance/AR BKC-DEC-085) — menutup BKC-GATE-06
  bersama penulisan kontrak BIL-API-0.8/BIL-TEST-0.8 dan BIL-AT-062–063 pada sesi yang sama.
  Dengan ini SELURUH BKC-DES-001–027 approved (kecuali BKC-DES-002 superseded).
superseded_design_decisions:
  BKC-DES-002: digantikan BKC-DES-015 — ComponentKey bertipe teks tidak jadi dipakai; bentuk (ComponentId, ComponentType) yang sudah terimplementasi BE-BKC-FIX-003 yang diadopsi
narrowed_design_decisions:
  BKC-DES-013: dipersempit dua kali (BUKAN digugurkan). Revisi 0.8 (BKC-DES-021): makna UnresolvedAmount menyisakan jalur rule NotCovered + IsAllowExcessPaymentByPatient=false saja; residual perhitungan pindah ke NonBillableResidualAmount. Revisi 0.9 (BKC-DES-026/027, approved 5 Sep 2026): dipersempit lagi menjadi NOL jalur — UnresolvedAmount selalu 0 pada versi kalkulasi baru; field/kolomnya tetap dipertahankan sebagai bukti perhitungan versi lama
contract_versions:
  api: BIL-API-1.0 (draft, revisi 1.1 — rumpun Edit Tagihan & Multi-Payer; ENAM endpoint invoice baru + DUA grup CRUD master baru) atas BIL-API-0.9 draft dan BIL-API-0.8 approved
  state: BIL-STATE-0.9 (draft, revisi 1.1 — perpindahan jenis payer, penanggung item, disposisi penebusan; NOL status invoice baru) atas BIL-STATE-0.8 draft dan BIL-STATE-0.7 approved
  validation: BIL-VALIDATION-0.9 (draft, revisi 1.1 — BIL-VAL-059-097) atas BIL-VALIDATION-0.8 draft dan BIL-VALIDATION-0.7 approved
  integration: BIL-INTEGRATION-0.8 (draft, revisi 1.1 — permukaan lintas modul TERLEBAR di modul ini: satu titik TULIS ke RegistrationManagement, tiga titik baca) atas BIL-INTEGRATION-0.7 draft dan BIL-INTEGRATION-0.6 approved
  permission: BIL-PERMISSION-0.8 (draft, revisi 1.1 — DUA Resource baru; NOL permission baru pada BillingInvoice) atas BIL-PERMISSION-0.7 draft dan BIL-PERMISSION-0.6 approved
  testing: BIL-TEST-1.0 (draft, revisi 1.1 — BIL-AT-081-100, tiga di antaranya regresi) atas BIL-TEST-0.9 draft dan BIL-TEST-0.8 approved
  calculation: BIL-CALCULATION-0.9 (draft, revisi 1.1) atas BIL-CALCULATION-0.8 approved. BERGERAK pada revisi ini — BERBEDA dari Petty Cash. Sebabnya: adapter tanggungan menjadi dispatcher per jenis payer, menghormati penanggung per item dan disposisi penebusan, membawa penanda jenis payer pada hasil, dan BERHENTI mengeluarkan anomali INSURANCE_PROVIDER_MISSING untuk kunjungan berpenjamin perusahaan
contract_versions_note_revision_1_1: >
  KETUJUH sumbu naik pada revisi 1.1. Seluruh amendment rumpun Edit Tagihan & Multi-Payer pada
  ketujuh sumbu itu berstatus `approved` sejak 11 September 2026 lewat MPY-DEC-012, mengikuti
  approval MPY-DES-001-017 yang menurunkannya. Ini TIDAK mencabut status approved isi sebelumnya:
  setiap berkas kontrak memuat amendment rumpun ini sebagai bagian TERPISAH di ujungnya,
  sementara seluruh bagian di atasnya tetap approved apa adanya.
  Sumbu `calculation` ikut naik — inilah pembeda utama dari revisi 1.0, dan alasannya perilaku
  perhitungan memang berubah untuk kunjungan berpenjamin perusahaan.
  Label `(draft, revisi 1.1 ...)` pada ketujuh baris di atas dibaca sebagai keadaan SAAT DITULIS;
  status yang berlaku sekarang adalah `approved` menurut baris ini.
contract_versions_note_revision_1_0: >
  Enam dari tujuh sumbu naik satu tingkat pada revisi 1.0, dan seluruh kenaikan itu berstatus
  `draft`. Ini TIDAK mencabut status approved isi sebelumnya: setiap berkas kontrak memuat
  amendment Petty Cash sebagai bagian TERPISAH di ujungnya, sementara seluruh bagian di atasnya
  tetap approved apa adanya. Sumbu `integration` ikut naik walaupun tidak ada baris BIL-INT-*
  baru, karena isinya memang bergerak — ia kini mencatat secara eksplisit KETIADAAN sambungan ke
  Shift Kasir, ke tagihan pasien, dan ke master pegawai, beserta dasar keputusannya. Mencatat
  ketiadaan itu adalah isi, bukan penomoran kosong.
contract_lock_note: >
  Seluruh enam dokumen kontrak (api, state, validation, integration, permission, testing) dikunci
  4 September 2026 oleh Product/Domain Owner ("saya kunci dokumen kontrak" / "kunci semua
  dokumennya"), dengan wewenang ganda Finance/AR sesuai BKC-DEC-085. Cakupan kunci ini adalah
  seluruh amendment sampai revisi 0.7/0.8 yang menaungi BKC-DES-001–025 (approved).
  Revisi 0.9 (BKC-DES-026/027, perluasan perutean jalur NotCovered) DITUTUP TERPISAH 5 September
  2026 oleh Product/Domain Owner ("Saya approve untuk case diatas", wewenang ganda Finance/AR
  BKC-DEC-085) — BKC-GATE-06 kini tertutup penuh. api dan testing naik ke 0.8; state, validation,
  integration, permission TIDAK bergerak (isinya tidak berubah, lihat 02-backend-architecture.md
  § "Kontrak yang tidak bergerak" amendment revisi 0.9). Impact scan pendahulu ada di
  01-existing-capability-map.md § 17.
contract_version_note: >
  Angka 0.5 DILEWATI pada seluruh sumbu. Amendment 3 September 2026 merancangnya, tetapi
  implementasi yang benar-benar mendarat (BE-BKC-FIX-003) tidak menaikkan
  BillingCalculationContract.Version, sehingga source tidak pernah memuat 0.5. Menaikkan
  langsung ke 0.6 mencegah dua bentuk berbeda memakai nomor versi yang sama.
compatibility_impact: >
  (revisi 0.7) additive pada bentuk — tidak ada endpoint baru, tidak ada tabel/kolom/migration,
  tidak ada field yang dihapus. TETAPI ada dua perubahan yang MUST disosialisasikan: (1) makna
  coverage.unresolvedAmount dipersempit tanpa berubah nama atau tipe; (2) nilai
  coverage.primaryAmount naik dan taxAmount rawat inap menjadi nol untuk invoice OPEN yang
  dihitung ulang. Keduanya perubahan NILAI, bukan perubahan bentuk, dan justru karena itu
  sulit ditemukan konsumen.
  (revisi 0.8) additive pada bentuk — tidak ada endpoint baru, tidak ada field yang dihapus atau
  diganti nama, dan field request baru (CreateWriteOffRequest.category) bersifat OPSIONAL
  berbawaan "PATIENT_AR" sehingga konsumen existing tidak rusak. BERBEDA dari 0.7: revisi ini
  MENYENTUH SKEMA — dua kolom baru (BilWriteOffCase.Category, BilCalculationVersion.
  NonBillableResidualAmount) beserta satu index dan SATU MIGRATION. Keduanya NOT NULL berdefault
  sehingga aman dijalankan tanpa downtime dan backfill-nya benar secara bisnis. Yang MUST
  disosialisasikan: makna coverage.unresolvedAmount dipersempit LAGI (residual perhitungan
  pindah ke coverage.nonBillableResidualAmount). TIDAK ADA nilai tagihan yang bergeser —
  Total Tagihan, Subtotal Mandiri, Subtotal Asuransi, dan outstanding pasien identik sebelum
  dan sesudah; yang berpindah hanya nama ember.
active_dependency_ids: [BKC-BLK-FE-001, BKC-BLK-INT-001, BKC-BLK-PROV-001, BKC-BLK-DATA-001, BKC-BLK-SEC-001, BKC-BLK-MASTER-001, BKC-BLK-TAX-001, BKC-BLK-GIT-001]
blocking_questions:
  - BKC-OQ-082 — DITUTUP PENUH 4 Sep 2026. BKC-DEC-084 (approval Product/Domain Owner) lalu BKC-DEC-085 (caveat wewenang Finance/AR ditutup — pengguna mengonfirmasi wewenang ganda).
  - BKC-OQ-085 — SEBAGIAN. Mitigasi risiko salah kategori DITUTUP oleh BKC-DEC-082 (validasi/warning saat ServiceType RANAP diubah setelah item obat/alkes ditambahkan). Pertanyaan ASLI (hitungan dampak penurunan total tagihan/PPN rawat inap) TETAP TERBUKA — butuh analisis Finance, bukan keputusan bisnis wawancara.
  - (revision 0.6) Approval BKC-DES-001..009 — DITUTUP PENUH 4 Sep 2026 oleh BKC-DEC-086 (BKC-DES-007) dan BKC-DEC-087 (sisanya, blanket).
  - (revision 0.6, masih berlaku) Penilaian Security atas pemakaian ulang BillingInvoice:Read untuk dokumen berisi nomor polis
  - (revision 0.6, masih berlaku) Kelengkapan MstInsuranceProvider dan MstInsuranceCoverageRule untuk verifikasi UAT
  - (revision 0.7) BKC-DEC-080 belum tertulis implementasinya — DITUTUP oleh revisi 0.8. Desainnya ada di 02-backend-architecture.md § "Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off" beserta BKC-DES-021–025, dan turunannya di contracts/, data/, testing/, flowcharts/, serta 04-prd-to-mvp.md Bagian C. Yang tersisa hanyalah approval manusia atas BKC-DES-021–025, sama seperti keputusan desain lain.
  - (revision 0.8) Approval BKC-DES-021–025 — DITUTUP PENUH 4 Sep 2026 oleh BKC-DEC-088 (Product/Domain Owner + Finance/AR, "saya approve"). Catatan: approval ini BUKAN otorisasi membuat/menjalankan migration BKC-DES-024 — itu tetap butuh konfirmasi terpisah saat implementasi.
blocking_questions_revision_1_0:
  - TIDAK ADA. Rumpun Petty Cash tidak melahirkan satu pun pertanyaan terbuka bertanda memblokir. Yang tersisa sebelum /plan-module-delivery untuk rumpun ini HANYA approval manusia atas PC-DES-001-014.
blocking_questions_revision_1_1:
  - MPY-OQ-004 — DITUTUP PENUH 11 September 2026 oleh MPY-DEC-011 (Muhammad Hamzah, pemilik RegistrationManagement). Bentuk yang disetujui, satu layanan generik yang memperbarui baris di tempat dan ikut transaksi pemanggil. Gelombang MVP-17 TIDAK lagi terblokir.
  - TIDAK ADA pertanyaan bertanda memblokir yang tersisa untuk rumpun ini. Keempat gelombang MVP-16 sampai MVP-19 siap diteruskan ke /plan-module-delivery.
non_blocking_questions_revision_1_1:
  - MPY-OQ-005 — Kolom penanda "sudah ditagih" pada catatan penyerahan obat milik PharmacyManagement sudah ada tetapi belum diketahui dipakai proses apa. MUST dicek lewat pembacaan source sebelum MVP-18 dimulai, supaya rumpun ini tidak membuat mekanisme paralel yang bertentangan. Penjawab, pemilik arsitektur backend bersama pemilik Pharmacy.
  - MPY-OQ-006 — Siapa mengisi aturan tanggungan untuk setiap perusahaan penjamin yang sudah terdaftar, dan kapan. Memblokir AKTIVASI fitur, bukan pembangunannya, karena tanpa isi master seluruh tanggungan perusahaan terhitung nol. Penjawab, Product/Domain Owner bersama Admin Master Data.
  - MPY-CQ-03 — Koordinasi urutan commit dengan pekerjaan "Payment Reminder" yang working tree-nya sudah menyentuh tiga berkas yang juga akan disentuh rumpun ini. Murni koordinasi tim, bukan keputusan arsitektur. Penjawab, pemilik modul billing-kasir.
non_blocking_questions_revision_1_0:
  - PC-OQ-001 — Nama entity data induk kategori, MstPettyCashCategory (dirancang) versus BilPettyCashCategory (tanda kurung pada 01-existing-capability-map.md § 18.8). Desain memilih Mst karena registry menetapkan prefix Mst untuk MASTER/REFERENCE dan enam dari enam data induk modul ini memakainya; § 18.8 sendiri menunjuk MstPaymentMethod/MstTaxRule sebagai pola yang tepat ditiru pada kalimat yang sama. Perubahan nama, bila diputuskan, adalah satu paket sebelum berkas model pertama dibuat sehingga tidak menimbulkan rename tabel. Penjawab: pemilik arsitektur backend.
  - PC-OQ-002 — Apakah koreksi nomor nota pada voucher Selesai perlu wewenang lebih tinggi. Bawaan desain: petugas yang sama, setiap koreksi tercatat. Penjawab: Kepala Kasir/Finance Operations.
  - PC-OQ-003 — Apakah baris registry BillingManagement/Billing yang ada dianggap mencakup submodule PettyCash/, sebagaimana ia sudah mencakup Cashier/ dan Operational/. Prefix Bil sendiri SUDAH tidak dipertanyakan (QBE-NAM-002 langkah 2 — pemiliknya terdaftar ACTIVE). MEMBLOKIR BERKAS MODEL PERTAMA berdasarkan QBE-MOD-003; TIDAK memblokir desain maupun perencanaan task. Penjawab: pemilik arsitektur backend.
  - PC-OQ-004 — Apakah pengaju voucher boleh menyetujui pengajuannya sendiri. PC-DEC-004 menetapkan satu jenjang tanpa menyebut pemeriksaan dua orang, berbeda dari write-off yang punya BIL-VAL-017. Desain MENGIKUTI keputusan apa adanya dan TIDAK menambahkan aturan yang tidak diminta; risikonya dicatat terbuka pada contracts/permission-audit-matrix.md § "Kewenangan yang tidak dapat dijaga mesin hak akses". Mitigasi yang tersedia sekarang murni administratif. Penjawab: Product/Domain Owner + Kepala Kasir/Finance Operations.
  - PC-OQ-005 — Prefix dan kebijakan reset nomor voucher. Bawaan desain PTC/DAILY/4 digit, seluruhnya dibaca dari pengaturan aplikasi seperti keempat jenis nomor yang sudah ada. Melanjutkan PC-CQ-02 pada 01-existing-capability-map.md § 18.10. Keputusan konfigurasi, bukan arsitektur. Penjawab: Product/Domain Owner + Finance.
non_blocking_questions_revision_0_8:
  - BKC-OQ-093 — DITUTUP PENUH 4 Sep 2026 oleh BKC-DEC-089. NotCovered + IsAllowExcessPaymentByPatient=false SEKARANG IKUT dirutekan ke write-off, sama seperti residual — memperluas cakupan literal BKC-DEC-080. Perlu diselaraskan ke 02-backend-architecture.md § BKC-DES-021–025 (belum tertulis di revisi 0.8, gap kecil tersisa untuk revisi berikutnya).
  - BKC-OQ-094 — DITUTUP PENUH 4 Sep 2026. (a) BKC-DEC-090: finalisasi TIDAK diblokir, warning saja. (b) BKC-DEC-091: NON_BILLABLE_RESIDUAL sepenuhnya di luar alur AR/AP, tidak jadi syarat readiness AP Dokter/klaim asuransi.
blocked_by_external_action_not_business_decision:
  - BKC-OQ-085 (asli) — analisis dampak finansial rawat inap, perlu Finance menghitung dari data invoice riil.
  - Review Security atas BillingInvoice:Read untuk dokumen berisi nomor polis.
  - Kelengkapan MstInsuranceProvider/MstInsuranceCoverageRule untuk verifikasi UAT — perlu pengecekan data langsung.
artifact_hashes:
  02-backend-architecture.md: 93fbd2928453a4e21162716495ef03249a2e673ebac6183cb76a866f12690651
  03-frontend-architecture.md: b06b61ec105a15e3c5bbafdbf7e4be57df18b47405adb6137b078ac315975712
  04-prd-to-mvp.md: 0ffbf788bd0618b6b746f5b75f232eb43809735d37219cee5e33e2ac1cab465e
  contracts/api-contract.md: 12566ff846c0dbfff622e8ee0920be7a95d6d161071b796302a5b11c83d012b7
  contracts/state-transition-matrix.md: acddc50c9c491c35e7ea6eb8aba48d2d7ad220b5d5be2f6dcdd80c20c9dc7530
  contracts/validation-matrix.md: 727013380e52928fa5a6be5ae2be82b445d8c14b0fb3560864e099412487b754
  contracts/integration-contract.md: 165b1e95cc80cf3f2880c9855482701e36bff403024c2f5a998f310eec95d8f6
  contracts/permission-audit-matrix.md: 6ea7430ee74266c242d0f5300c26c4a73b5b53532ae22d33b603fb86017c83ca
  data/data-dictionary.md: c95d020a03230d9a812fd542bbf6d08c594092cfd73271e00cad33bf8a54e679
  testing/acceptance-test-matrix.md: 6b03ce3322c30f71279c34cd9fdee168606a762be6aac586ad1e583af63d95dd
  flowcharts/00-alur-utama.md: 4efffec07b85ff73b852491ed9266a5717c76529777e1908f967cb200cdb38f7
  flowcharts/ganti-payer-kunjungan.md: f177c0850754d074a713165b7db02c6cc242ed9a8892aa913e92188e08e119b2
  flowcharts/penanggung-per-item-tagihan.md: 6565c1773f01494dc19b721c87a51809fb2fa12bcf44db0b3c937866bf440e3a
  flowcharts/penebusan-obat-rawat-jalan.md: b278ed0cb0feaad67d83fe8611075761211aaca5ae2e6311b586b7befaa8c840
  flowcharts/pembagian-tanggungan-penjamin.md: 5017a2dde74e944062813cff1cca055ac0e71190b55034051feec51d9199eb86
  flowcharts/ppn-obat-alkes.md: 87393bbd87a297551f601695cc6630f111a9877c6a37a011a4ea8e6a376c5c9e
  flowcharts/dokumen-invoice-asuransi.md: 2db82496543972720b7dfdccd560e3d05db0e074b23d4ebc8a0c48f31e2de32c
  flowcharts/voucher-petty-cash.md: 6e4e3eaa828022175c9756686efeab7780f7440ca885f4396149dd6478eb2028
  flowcharts/anggaran-petty-cash.md: 886c9ef3bba9a5dc73350092c134022e8ab34b396da6e8362a87a637a6209d4d
  erd/data-dictionary.md: e65acb8276f9d3a603d6a27f7da54e5fdca95297554ff25a04d04d0b6d2aed7a
artifact_hashes_note_revision_1_1: >
  Dihitung ulang pada revisi 1.1 (SHA256, isi berkas apa adanya), benar-benar dihitung dari
  berkasnya, bukan ditulis tangan.
  SEBELAS berkas berubah pada revisi 1.1: 02-backend-architecture.md, 03-frontend-architecture.md,
  04-prd-to-mvp.md, kelima berkas contracts/, data/data-dictionary.md,
  testing/acceptance-test-matrix.md, dan flowcharts/00-alur-utama.md.
  TIGA berkas BARU ditambahkan ke daftar: flowcharts/ganti-payer-kunjungan.md,
  flowcharts/penanggung-per-item-tagihan.md, dan flowcharts/penebusan-obat-rawat-jalan.md.
  Satu baris ganda testing/acceptance-test-matrix.md yang tertinggal dari revisi 1.0 DIHAPUS pada
  pass ini; nilai yang berlaku adalah yang dihitung revisi 1.1.
  Lima berkas TIDAK disentuh dan hash-nya tetap sebagaimana revisi 1.0: keempat flowchart rumpun
  sebelumnya dan erd/data-dictionary.md.
artifact_hashes_note: >
  Dihitung ulang pada revisi 1.0 (SHA256, isi berkas apa adanya), dan kali ini BENAR-BENAR
  dihitung, bukan ditulis tangan. Ini menutup keadaan STALE yang dicatat revisi 0.8, ketika
  empat berkas berubah untuk revisi 0.9 tetapi hash-nya tidak ikut dihitung.
  Sebelas berkas berubah pada revisi 1.0: 02-backend-architecture.md, 03-frontend-architecture.md,
  04-prd-to-mvp.md, keempat berkas contracts yang tersisa, contracts/permission-audit-matrix.md,
  data/data-dictionary.md, flowcharts/00-alur-utama.md, dan testing/acceptance-test-matrix.md.
  Dua berkas BARU ditambahkan ke daftar: flowcharts/voucher-petty-cash.md dan
  flowcharts/anggaran-petty-cash.md.
  Satu berkas TIDAK disentuh dan hash-nya identik dengan revisi 0.8: erd/data-dictionary.md.
  Tiga flowchart lama berubah hash-nya dibanding nilai tercatat revisi 0.8 walaupun tidak
  disentuh pass ini — selisih itu berasal dari revisi 0.9 yang hash-nya memang tidak pernah
  dihitung ulang, bukan dari perubahan isi pass ini.
supersedes: null
```

## Artifact register

| Kelompok | Lokasi | Status |
| --- | --- | --- |
| Keputusan dan capability | [`00-interview-decisions.md`](./00-interview-decisions.md), [`01-existing-capability-map.md`](./01-existing-capability-map.md) | Baseline `0.2 approved` + amendment `BKC-DEC-059`–`062` **approved** (2 Sep) + `BKC-DEC-063`–`069` **approved** (3 Sep) + `BKC-DEC-070`–`079` **approved** (4 Sep) |
| Backend/frontend design | [`02-backend-architecture.md`](./02-backend-architecture.md), [`03-frontend-architecture.md`](./03-frontend-architecture.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + amendment 3 Sep **approved** (`BKC-DES-001`–`009`) + amendment 4 Sep **approved** (`BKC-DES-010`–`020`) + amendment lanjutan 4 Sep **approved** (`BKC-DES-021`–`025`, revisi `0.8`) + **amendment lanjutan 4 Sep `approved` 5 Sep 2026** (`BKC-DES-026`–`027`, revisi `0.9`). `03-frontend-architecture.md` **tidak disentuh** revisi `0.8`/`0.9` |
| PRD → MVP slice | [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | Slice `BKC-DEC-059`–`062` **approved** + slice `BKC-DEC-065`–`069` **draft** (`EPIC BKC-04`/`BKC-05`) + slice `BKC-DEC-070`–`079` **draft** (`EPIC BKC-06`/`BKC-07`/`BKC-08`) + **Bagian C `draft`** (`EPIC BKC-09`, `BKC-DEC-080`) |
| Flowchart alur proses | [`flowcharts/`](./flowcharts/00-alur-utama.md) | **Baru pada revision `0.7`** — empat berkas, seluruhnya **draft**. [`pembagian-tanggungan-penjamin.md`](./flowcharts/pembagian-tanggungan-penjamin.md) **direvisi** pada `0.8`. **Bertambah dua berkas pada revision `1.0`** (`draft`): [`voucher-petty-cash.md`](./flowcharts/voucher-petty-cash.md) dan [`anggaran-petty-cash.md`](./flowcharts/anggaran-petty-cash.md); [`00-alur-utama.md`](./flowcharts/00-alur-utama.md) bertambah satu bagian penunjuk ke keduanya |
| Kamus data | [`data/data-dictionary.md`](./data/data-dictionary.md) | **Baru pada revision `0.7`** — memuat delta 4 Sep dan indeks ke kamus baseline; delta skema `0.8` (dua kolom, satu index, satu migration) **approved** 4 Sep 2026 (koreksi status draft yang tertinggal, diperbaiki 5 Sep); **bertambah amendment `0.9` `approved`** (nol perubahan skema, murni keterangan peran kolom). Kamus baseline tetap di [`erd/data-dictionary.md`](./erd/data-dictionary.md) |
| ERD/data baseline | [`erd/`](./erd/00-context-erd.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + catatan 3 Sep **draft** + rujukan silang 4 Sep **draft**. **Tidak disentuh revisi `0.8`/`0.9`**; satu ketidaksesuaian pada [`erd/03-financial-exception-adjustment.md`](./erd/03-financial-exception-adjustment.md) (kolom `AdjustmentType` yang tidak ada di source) dilaporkan pada `data/data-dictionary.md`, perapiannya revisi tersendiri |
| Kontrak dan acceptance | [`contracts/`](./contracts/api-contract.md), [`testing/`](./testing/acceptance-test-matrix.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + amendment `0.5` **draft** (3 Sep) + amendment `0.6` **draft** (4 Sep) + amendment `0.7` **approved** (revisi `0.8`) pada `api`, `state`, `validation`, `testing` + **amendment `0.8` `approved` 5 Sep 2026** (revisi `0.9`) pada `api`, `testing`. `state`, `validation`, `integration-contract.md`, dan `permission-audit-matrix.md` **tidak bergerak** pada revisi `0.9` |
| Delivery roadmap | [`roadmap/`](./roadmap/README.md) | Revision `1` — slice `MVP-4`–`MVP-10` **belum** masuk roadmap; itu keluaran `/plan-module-delivery` |
| Evidence/arsip | [`evidence/`](./evidence/02-requirement-completeness-gate.md) | Preserved |
| Status | [`MODULE-STATUS.md`](./MODULE-STATUS.md) | Belum diperbarui untuk revision `0.7` — pemeliharaannya milik `/manage-module-blueprint` |

## Catatan revision `0.7`

Revision `0.7` **menambah**, tidak menggantikan, baseline `0.5 approved`. Isinya adalah amendment 4 September 2026 "Pembagian tanggungan penjamin, anomali data, dan gerbang PPN rawat jalan/rawat inap", turunan keputusan bisnis `BKC-DEC-070`–`079` yang sudah disetujui Product/Domain Owner pada tanggal yang sama.

Field `status` bernilai `draft` **hanya untuk revision `0.6`, `0.7`, dan `0.8`**. Baseline `0.5` beserta seluruh amendment 2 September 2026 tetap `approved` dan tidak dicabut. Catatan susulan: `BKC-DES-001`–`020` sendiri sudah **approved** lewat `BKC-DEC-084`–`087` (4 September 2026); yang masih `draft` pada revisi `0.6`/`0.7` adalah label dokumennya, bukan status keputusannya. Yang benar-benar `draft` sebagai keputusan hanyalah `BKC-DES-021`–`025` pada revisi `0.8`.

### Dua blocker desain yang ditutup pada revisi ini

Amendment 3 September 2026 sengaja meninggalkan dua pertanyaan arsitektur terbuka. Keduanya ditutup sekarang:

| Blocker | Ditutup oleh | Ringkas |
| --- | --- | --- |
| Bentuk kontrak pecahan rupiah per baris (`BKC-DEC-069`) | `BKC-DES-015`, `BKC-DES-016`, `BKC-DES-017` | Mengadopsi bentuk yang **sudah terimplementasi** `BE-BKC-FIX-003` — `BillingCoverageComponentOutcome` ber-kunci `(ComponentId, ComponentType)`, dipersist di dalam `BreakdownSnapshot` yang sudah ada, ditandai `IsPerItemAllocationAvailable`. Menggantikan `BKC-DES-002` |
| Bentuk kategori anomali data (`BKC-DEC-073`) | `BKC-DES-010`, `BKC-DES-011`, `BKC-DES-012`, `BKC-DES-013` | Field tersendiri `DataAnomalyAmount` beserta `AnomalyCodes`/`AnomalyMessages`, **bukan** memakai ulang `UnresolvedAmount`. Nominalnya jatuh ke pasien agar tagihan tetap menjumlah dan tetap dapat dibayar; penanda tampil sebagai peringatan, bukan baris subtotal. Penjaga `REJECTED` diretarget ke nominal anomali (`BIL-VAL-036`) |

### Bentuk blueprint — hasil uji pemecahan

`blueprint_shape` tidak pernah tercatat pada manifest revisi mana pun, sehingga uji pemecahan dijalankan ulang pada pass ini. Lima rumpun kemampuan modul ini diuji terhadap kelima syarat `COMPOSITE`:

| Rumpun kemampuan | Konteks sendiri | Kosakata status sendiri | Resource hak akses dan pemilik peran sendiri | Dapat dirilis sendiri | Master dan pemilik approval sendiri | Skor |
| --- | :---: | :---: | :---: | :---: | :---: | ---: |
| Tagihan dan biaya | Ya | Tidak — memakai status invoice bersama | Tidak — `BillingInvoice` | Tidak — seluruh rumpun lain bergantung padanya | Tidak | 1 dari 5 |
| Dana pasien dan penyelesaian | Tidak — beroperasi pada `BilInvoice` yang sama | Tidak — statusnya menempel pada invoice | Tidak — sebagian besar `BillingInvoice` | Tidak — tanpa tagihan tidak ada yang diselesaikan | Tidak | 0 dari 5 |
| Pengecualian finansial | Tidak | Sebagian — punya status kasus sendiri | Tidak | Tidak — mengoreksi tagihan yang sudah ada | Tidak | 1 dari 5 |
| Operasi shift kasir | Ya | Ya | Ya — `CashierShift` | Tidak — penerimaan tunai menuntut shift aktif, dan sebaliknya | Tidak | 3 dari 5 |
| Finalisasi dan penyerahan | Tidak | Tidak — status invoice yang sama | Tidak | Tidak | Tidak | 0 dari 5 |

Hanya satu rumpun yang mencapai ambang tiga dari lima, dan `COMPOSITE` **MUST NOT** dipakai dengan kurang dari dua sub-modul. Seluruh kemampuan berbagi satu aggregate `BilInvoice`, satu kosakata status, dan satu pemilik proses. **Kesimpulan: `SINGLE`.**

Nilai `shape_decided_by` sengaja **tidak** ditulis `USER` maupun `USER_CONFIRMED`, karena tidak ada keputusan pemilik yang tercatat untuk diklaim. Ia mencatat struktur yang sudah terwujud sejak revision `0.2` dan meminta ratifikasi lewat `BKC-OQ-091`. Ratifikasi itu **tidak memblokir** apa pun: strukturnya tidak berubah, dan tidak ada berkas yang berpindah.

### Struktur berkas — dua penyimpangan yang tercatat

| Penyimpangan | Keadaan | Sikap |
| --- | --- | --- |
| Kamus data ada di `erd/data-dictionary.md`, bukan `data/data-dictionary.md` | Modul ini dibangun sebelum struktur keluaran yang berlaku sekarang ditetapkan | `data/data-dictionary.md` **dibuat** pada revisi ini berisi delta 4 September 2026 dan indeks ke kamus baseline. Isi baseline **tidak disalin**, supaya tidak ada dua sumber yang dapat menyimpang. Penyatuannya adalah revisi tersendiri milik `/manage-module-blueprint` — `BKC-OQ-089` |
| Folder `flowcharts/` belum pernah ada | Struktur lama memakai `erd/` sebagai satu-satunya artefak visual | Empat berkas flowchart **dibuat** pada revisi ini, memuat alur pokok dan tiga proses bercabang beserta jalur pengecualiannya. Rujukan ke `erd/` tetap dipertahankan untuk relasi antar entity baseline |

### Staleness dan impact scan

`backend_commit_sha` naik dari `a42b651d` ke `ffeb45a8`. Satu-satunya commit di antara keduanya adalah `ffeb45a8` itu sendiri, yang isinya **murni dokumentasi** — tiga belas berkas di bawah `docs/module-blueprints/billing-kasir/`, tanpa satu pun berkas source aplikasi. Artinya source aplikasi pada kedua SHA itu identik, dan pergerakan SHA ini **tidak** membuat bukti as-is revision `0.6` basi.

Yang **membuatnya basi** justru bukan commit, melainkan **working tree yang belum di-commit** (lihat `working_tree_uncommitted` di atas). Enam berkas source pada kedua repository sudah berubah tanpa pernah masuk Git, dan perubahannya menyentuh persis area yang dirancang amendment 3 September 2026. Karena itu:

- bagian "Bukti as-is" pada amendment 3 September 2026 **sebagian sudah tidak akurat**, dan koreksinya ditulis pada amendment 4 September 2026 § "Bukti as-is";
- `BKC-DES-002` digugurkan dan digantikan `BKC-DES-015` karena masalah yang hendak diselesaikannya sudah diselesaikan dengan cara lain di working tree.

`frontend_commit_sha` tidak bergerak sama sekali; perubahan frontend juga belum di-commit.

Pembacaan langsung yang dilakukan pass ini **bukan** pengganti impact scan resmi. `01-existing-capability-map.md` masih berhenti pada impact scan § 16 (2 September 2026) dan belum punya bagian untuk 3 maupun 4 September 2026 — pembaruannya milik `/trace-existing-capabilities`, bukan skill desain, sehingga `input_hashes` untuk berkas itu tidak berubah. Jalankan impact scan itu sebelum `/plan-module-delivery`, terutama setelah `BKC-OQ-090` dijawab dan working tree diselesaikan.

### Caveat wewenang yang masih berlaku

`BKC-DEC-062` mengamendemen sebagian `BKC-DEC-042` yang owner tercatatnya adalah Payer/Insurance + Finance/AR, sementara approval yang diberikan berasal dari Product/Domain Owner tanpa konfirmasi terpisah dari owner asli tersebut. **Caveat yang sama menurun ke `BKC-DEC-071`**, yang menggantikan sisa bagian `BKC-DEC-062` (pencabutan gerbang limit bulanan). Dicatat apa adanya sebagai provenance, bukan disembunyikan; bila pemilik asli keberatan di kemudian hari, keduanya perlu ditinjau ulang, bukan dianggap final selamanya.

Ketegangan `BKC-DEC-070`/`BKC-DEC-074` (residual tanpa syarat ke pasien vs `IsAllowExcessPaymentByPatient` yang menyatakan sebaliknya untuk sebagian aturan) **SUDAH DITUTUP** 4 September 2026 lewat `BKC-DEC-080`: ketika `IsAllowExcessPaymentByPatient=false`, residual menjadi write-off/Pengecualian Finansial (`BKC-DEC-036`), BUKAN tanggungan pasien — bukan lagi penafsiran "perilaku tidak berubah" yang diambil agent, melainkan keputusan eksplisit Product/Domain Owner. **Desainnya sudah ditulis pada revision `0.8`** (lihat catatan di bawah); gap ini tidak lagi terbuka.

## Catatan revision `0.8`

Revision `0.8` adalah **revisi susulan berukuran kecil** di atas `0.7`, bukan pass desain baru. `0.7` beserta seluruh isinya tetap berlaku utuh; yang ditambahkan hanyalah satu hal yang tertinggal.

### Apa yang ditutup

| Gap | Ditutup oleh | Ringkas |
| --- | --- | --- |
| `BKC-DEC-080` belum tertulis implementasinya di revisi `0.7` | `BKC-DES-021`–`BKC-DES-025` | Residual perhitungan dengan `IsAllowExcessPaymentByPatient = false` pindah dari `unresolved` ke ember tersendiri `NonBillableResidualAmount`, lalu dirutekan ke mekanisme write-off yang sudah ada lewat kategori baru pada `BilWriteOffCase` |

### Lima keputusan arsitektur revisi ini

| ID | Yang diputuskan | Dasar |
| --- | --- | --- |
| `BKC-DES-021` | Field tersendiri `NonBillableResidualAmount`, terpisah dari `DataAnomalyAmount` **dan** dari `UnresolvedAmount`. Ketiganya sama-sama "bukan tanggungan pasien" tetapi menuntut orang yang berbeda untuk bertindak | `BKC-DEC-080`, disiplin `BKC-DES-010` |
| `BKC-DES-022` | Titik tangkapnya cabang residual di dalam `ResolveAsync` — satu-satunya tempat yang memegang `rule` | `BKC-DEC-080` |
| `BKC-DES-023` | **Pemicu write-off MANUAL**, bukan otomatis. Sistem mendeteksi, memberi nilai, menandai, dan menyiapkan pre-fill; manusia berwenang yang mengajukan dan orang kedua yang menyetujui | `BKC-DEC-036`, `BKC-DEC-080` |
| `BKC-DES-024` | Kolom `Category` pada `BilWriteOffCase` (`PATIENT_AR` \| `NON_BILLABLE_RESIDUAL`) menentukan plafon, keikutsertaan pada outstanding pasien, dan boleh tidaknya memindahkan status invoice | `BKC-DEC-036`, `BKC-DEC-080` |
| `BKC-DES-025` | Plafon dibaca dari kolom `BilCalculationVersion.NonBillableResidualAmount`, bukan dari JSON `BreakdownSnapshot` | `BKC-DES-024` |

### Kenapa pemicunya manual — pertanyaan desain paling berbobot pada revisi ini

Ini satu-satunya pilihan nyata pada revisi ini, dan jawabannya bukan preferensi gaya. Lima buktinya, berurut dari yang paling menentukan:

1. **Pemeriksaan dua orang akan runtuh.** `BIL-VAL-017` melarang pengaju menyetujui pengajuannya sendiri. Bila pengajunya mesin, satu-satunya manusia dalam alur itu adalah penyetujunya.
2. **Jalur baca akan berubah menjadi jalur tulis.** `CalculateAsync` dipanggil setiap kali layar pembayaran dibuka, dengan hak akses `BillingInvoice : Read`. Membuat kasus di sana memberi setiap kasir kewenangan `BillingWriteOff : Create` secara diam-diam, dan setiap muat ulang melahirkan satu kasus baru.
3. **Angkanya belum final.** Residual berubah setiap item ditambah atau dibatalkan.
4. **Tidak ada status untuk menampungnya.** `BilWriteOffCase` hanya mengenal `SUBMITTED`/`POSTED`/`REJECTED`.
5. **Alasannya wajib kalimat manusia.** `Reason` dibaca auditor; kalimat bangkitan mesin akan seragam dan berhenti berguna.

### Perubahan skema — berbeda dari revisi `0.7`

Revisi `0.7` sama sekali tidak menyentuh skema. Revisi `0.8` **menyentuh**: dua kolom, satu index, satu migration. Keduanya `NOT NULL` berdefault sehingga dapat dijalankan tanpa mematikan layanan, dan backfill-nya benar secara bisnis — seluruh write-off yang sudah ada memang write-off piutang pasien.

### Yang TIDAK berubah pada revisi ini

Tidak ada nilai tagihan yang bergeser. Total Tagihan, Subtotal Mandiri, Subtotal Asuransi, nominal yang ditagihkan kasir, dan outstanding pasien **identik** sebelum dan sesudah — nominal yang berpindah ember memang sudah dikeluarkan dari tagihan pasien sejak revisi `0.7`. Tidak ada endpoint baru, tidak ada permission baru, tidak ada kelebihan bayar yang timbul, dan tidak ada refund yang perlu disiapkan.

### Berkas yang disunting revisi `0.8`

| Berkas | Yang berubah |
| --- | --- |
| `02-backend-architecture.md` | Baris (5) tabel perilaku `ResolveAsync` dikoreksi; catatan pada `BKC-DES-013`; satu bagian amendment baru berisi `BKC-DES-021`–`025`, bukti as-is, dua class diagram, penjelasan sebelas class, folder, skema, migration, dan yang sengaja tidak dibuat |
| `contracts/validation-matrix.md` | `BIL-VAL-040`–`043` baru; `BIL-VAL-018` dan `BIL-VAL-023` dipertegas cakupannya |
| `contracts/state-transition-matrix.md` | Dampak posting dan reversal per kategori; penegasan tidak ada status baru |
| `contracts/api-contract.md` | Field `category` pada `CreateWriteOffRequest`/`WriteOffResponse`, `nonBillableResidualRemaining`, dan field kalkulasi baru. Tidak ada endpoint baru |
| `data/data-dictionary.md` | Dua kolom baru beserta index, backfill, dan ketidaksesuaian `AdjustmentType` yang dilaporkan apa adanya |
| `testing/acceptance-test-matrix.md` | `BIL-AT-055`–`061` baru; `BIL-AT-040` **dikoreksi** karena masih menguji perilaku lama |
| `flowcharts/pembagian-tanggungan-penjamin.md` | Cabang "kontrak tidak mengizinkan" tidak lagi berakhir buntu; alur lanjutan penanggungan digambar beserta jalur gagalnya |
| `04-prd-to-mvp.md` | Bagian C baru (`EPIC BKC-09`, `FR-BKC-038`–`044`, `UAT-21`–`27`, DoD, `MVP-11`/`MVP-12`); baris `BKC-OQ-084` ditutup |
| `blueprint-manifest.md` | Berkas ini |

Berkas lain pada revisi `0.7` **tidak disentuh**, termasuk `03-frontend-architecture.md`, `contracts/integration-contract.md`, `contracts/permission-audit-matrix.md`, `00-interview-decisions.md`, `01-existing-capability-map.md`, dan seluruh `erd/`.

## Catatan revision `1.0`

Revision `1.0` adalah **pass desain penuh untuk satu rumpun baru**, bukan amendment atas rumpun yang sudah ada. Ini pass pertama sejak baseline yang menambahkan tabel ke dalam modul ini.

### Kenapa langsung `1.0`, dan ke mana perginya `0.9`

Field `revision` tertinggal di `0.8` sementara badan dokumen ini beserta seluruh berkas kontrak sudah menyebut revisi `0.9` — amendment `BKC-DES-026`/`027` yang `approved` 5 September 2026. Ketidaksesuaian itu sudah tercatat pada `artifact_hashes_note` revisi `0.8` dan **bukan** temuan baru pass ini.

Pass ini menutupnya dengan menaikkan field langsung ke `1.0`. Angka `0.9` **dilewati sebagai nilai field**, bukan sebagai isi: seluruh amendment revisi `0.9` tetap berlaku, tetap `approved`, dan sudah tercermin pada `contract_versions` (`BIL-API-0.8`, `BIL-TEST-0.8`, `BIL-CALCULATION-0.8`). Tidak ada satu pun isi yang hilang atau dicabut.

Kenaikan ke angka bulat dipilih karena revisi ini memang bukan penambahan kecil: modul bertambah satu rumpun kemampuan yang berdiri sendiri, lengkap dengan tabelnya, endpointnya, hak aksesnya, dan menunya.

### Apa yang ditambahkan

| Kelompok | Isi |
| --- | --- |
| Keputusan bisnis masukan | `PC-DEC-001`–`PC-DEC-013`, seluruhnya `approved` 7 September 2026 |
| Keputusan arsitektur | `PC-DES-001`–`PC-DES-014`, seluruhnya **draft** |
| Kemampuan asal | `CAP-29` (Missing), `CAP-30` (Ready to reuse), `CAP-31` (Conflict pola), `CAP-32` (Ready to reuse) |
| Tabel | Lima baru: `BilPettyCashVoucher`, `BilPettyCashVoucherCommand`, `BilPettyCashBudget`, `BilPettyCashBudgetMovement`, `MstPettyCashCategory`. **Nol** tabel existing yang diubah |
| Endpoint | Sebelas baru pada tiga grup `[Tags(...)]` baru. **Nol** endpoint existing yang berubah |
| Hak akses | Tiga Resource baru (`PettyCashVoucher`, `PettyCashBudget`, `PettyCashCategory`) beserta dua belas Action |
| Butir menu | Tiga baru |
| Flowchart | Dua berkas baru beserta satu bagian penunjuk pada alur utama |
| Validasi | `BIL-VAL-044`–`BIL-VAL-058` |
| Acceptance test | `BIL-AT-064`–`BIL-AT-080` |
| Epic dan requirement | `EPIC BKC-10`–`BKC-12`, `FR-BKC-045`–`063`, `UAT-28`–`42`, gelombang `MVP-13`–`MVP-15` |

### Lima keputusan arsitektur yang paling menentukan

| ID | Yang diputuskan | Kenapa ia yang paling menentukan |
| --- | --- | --- |
| `PC-DES-001` | Konteks tersendiri tanpa satu pun foreign key ke `BilInvoice`, `BilCashierShift`, maupun `BilSettlement` | Satu foreign key saja akan mengundang orang berikutnya menjumlahkan dua kas yang berbeda, dan saat itu terjadi selisih kas shift ikut bergerak setiap kali kurir menerima ongkos transport — persis yang `PC-DEC-001` larang |
| `PC-DES-005` | Penjaga persetujuan memakai saldo **dikurangi** nominal yang sudah disetujui tetapi belum dicairkan | Konsekuensi aritmetis dari dua keputusan yang berbeda titik waktunya: `PC-DEC-008` memeriksa saat persetujuan, `PC-DEC-009` mengurangi saat pencairan. Tanpa komitmen, sepuluh voucher dapat disetujui di atas saldo yang hanya cukup untuk tujuh |
| `PC-DES-007` | Pembatalan memakai penandaan `IsCancel` warisan `IdentityModel`, bukan status keenam | Satu-satunya cara memenuhi `PC-DEC-007` dan `PC-DEC-013` bersamaan, tanpa mengarang status yang pemiliknya sudah menyatakan kosakatanya lengkap |
| `PC-DES-008` | Penomoran memakai ulang `BilNumberSeries`/`BillingNumberSeriesService` | Mewarisi gratis penguncian yang sudah menjamin nomor tidak kembar. Format `PC-1786239462244` pada rujukan tampilan **ditolak** — ia cap waktu, bukan nomor bisnis |
| `PC-DES-013` | Voucher `Ditolak` immutable, ditegakkan dengan **meniadakan endpointnya** | Aturan yang ditegakkan dengan pemeriksaan nilai dapat lupa disalin saat endpoint baru ditambahkan. Tidak adanya endpoint sama sekali tidak dapat lupa disalin |

### Selisih yang dicatat apa adanya — prefix data induk kategori

`01-existing-capability-map.md` § 18.8 merekomendasikan entity data induk baru "prefix `Bil`", tetapi pada kalimat yang sama menunjuk `MstPaymentMethod`, `MstBillingItemCategory`, `MstTaxRule`, dan `MstInsuranceCoverageRule` sebagai pola yang tepat ditiru. Kedua bagian kalimat itu bertentangan.

Desain memilih **`MstPettyCashCategory`**, dengan tiga alasan yang seluruhnya tertulis pada `02-backend-architecture.md` § "Catatan `PC-DES-002`": registry memisahkan prefix `Mst` untuk `MASTER / REFERENCE` dari prefix `Bil` untuk `BUSINESS DOMAIN / MODULE`; `BACKEND_ENGINEERING_CONTRACT.md` menyatakan `Mst*` masih berlaku dan `QBE-NAM-002` memerintahkan memakai prefix registry; dan enam dari enam data induk modul ini memakai `Mst`.

Yang **tidak** berubah dari rekomendasi § 18.8: kategori ini dimiliki `billing-kasir`, tidak dibagi dengan tabel HR mana pun, dan tidak memakai ulang `MstExpenseCategory`. Selisih penamaannya diangkat sebagai `PC-OQ-001` untuk diratifikasi, dan **tidak memblokir** apa pun selama berkas model pertama belum dibuat.

### Yang **tidak** berubah pada revisi ini

Tidak satu pun rumpun sebelumnya disentuh. Secara khusus: tidak ada nilai tagihan pasien yang bergeser, tidak ada endpoint existing yang berubah bentuk, tidak ada field yang dihapus atau berganti arti, tidak ada kolom yang ditambahkan pada tabel yang sudah ada, dan tidak ada butir hak akses existing yang berubah. Konsumen yang sudah ada tidak terpengaruh sama sekali.

`erd/` **tidak disentuh**. Ketidaksesuaian `AdjustmentType` pada `erd/03-financial-exception-adjustment.md` yang dilaporkan revisi `0.8` **masih terbuka**, dan perapiannya tetap revisi tersendiri milik `/manage-module-blueprint`.

### Kemandirian rumpun ini terhadap pertanyaan terbuka sebelumnya

Rumpun Petty Cash **tidak bergantung** pada `BKC-OQ-083` maupun `BKC-OQ-085`, dua pertanyaan terbuka yang masih menahan `/plan-module-delivery` untuk rumpun-rumpun sebelumnya. Keduanya menyangkut PPN dan tanggungan penjamin pada tagihan pasien; kas kecil tidak bersinggungan dengan keduanya sama sekali.

Konsekuensinya: begitu `PC-DES-001`–`014` disetujui, gelombang `MVP-13`–`MVP-15` dapat direncanakan dan dikerjakan **secara mandiri**, tanpa menunggu sisa pertanyaan terbuka rumpun lain terjawab. Inilah keuntungan praktis dari `PC-DEC-001` yang memutus sambungan ke kas shift kasir sejak awal.
