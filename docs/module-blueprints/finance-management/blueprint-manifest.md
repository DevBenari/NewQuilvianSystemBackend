# Finance Management — Blueprint Manifest

```yaml
blueprint_id: FIN-BP-001
module_name: Finance Management
module_slug: finance-management
module_prefix: Fin
module_area: Areas/Corporate/FinanceManagement
revision: 5
revision_5_note: >
  Revisi 5 (25 September 2026) adalah /design-business-module kecil yang menggambar skema empat
  keputusan yang sudah approved: FIN-DEC-057 (Deposit Retur sebagai sumber dana di dalam
  FinPayment), FIN-DEC-058 (kode potongan AR), serta FIN-DEC-061 dan FIN-DEC-062 yang dijawab
  owner di awal pass ini (kode RETUR-PEMBELIAN, PEMAKAIAN-DEPOSIT-RETUR,
  PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI). Keputusan arsitekturnya FIN-DES-045..050 — APPROVED
  26 September 2026: owner memilih "Setujui FIN-DES-045..050 dan kunci kontraknya" saat memanggil
  /plan-module-delivery. Dicatat apa adanya; BUKAN otorisasi membuat/menjalankan migration.
  DUA KOREKSI atas bentuk REVISI 4 yang belum dibangun: FinSupplierReturnDepositUsage menunjuk
  PaymentId (bukan PurchasingInvoiceId), dan FinReceiptDeduction melekat pada
  ReceiptAllocationId (tanpa itu tidak diketahui piutang mana yang dikurangi).
  SATU tabel yang SUDAH BERJALAN berubah: FinPayment — kolom DepositAppliedAmount dan check
  constraint CK_FinPayment_NetTransfer diganti (migration baru
  AddDepositAppliedAmountToFinPayment). SATU perilaku berjalan berubah: nilai AP_PAYMENT pada
  pembayaran yang memakai deposit turun sebesar porsi deposit; pembayaran tanpa deposit identik.
  Dua endpoint FIN-API-1.1 dicabut (deposits/{id}/apply, POST receipts/{id}/deductions) —
  keduanya belum pernah punya kode.
revision_4_note: >
  Revisi 4 (25 September 2026) adalah /design-business-module untuk TIGA rumpun kapabilitas
  BARU yang sebelumnya nol barisnya di backend: FIN-SC-008 Purchasing/AP siklus penuh (Purchase
  Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur,
  approval dua jenjang), FIN-SC-009 AR Invoice Agregat ke penjamin, dan FIN-SC-010 Potongan sisi
  penerimaan piutang. Keputusan bisnisnya FIN-DEC-045..055 (approved 25 September 2026);
  keputusan arsitekturnya FIN-DES-037..044 (draft, menunggu approval owner).
  EMPAT BELAS tabel baru dan SATU tabel diperbarui (FinSupplierPayable, kolom
  SourcePurchasingInvoiceId). Berkas ERD baru: erd/purchasing-ap.md.
  EPIC FIN-15/16/17 ditambahkan pada 04-prd-to-mvp.md; EPIC FIN-07 (lama) digantikan EPIC FIN-15.
  GERBANG KERAS BARU: kode kejadian PPN-MASUKAN-PEMBELIAN (usulan ke-25, evidence/06) MUST
  diratifikasi Accounting (FIN-OQ-020) sebelum EPIC FIN-15 boleh masuk /plan-module-delivery —
  diminta sendiri oleh owner Finance (FIN-DEC-046), bukan menunggu pihak luar yang belum diminta.
  Enam kontrak turunan naik versi (api, integration, state-transition, validation, permission,
  test) karena revisi ini menambah endpoint/status/aturan/hak-akses baru; prd-to-mvp naik ke
  FIN-MVP-1.2. Seluruhnya berstatus draft, BELUM dikunci owner.
  YANG TIDAK BERUBAH: FIN-DES-001..036 tetap berlaku; rumpun AR/Collection/Cash/Master Data/
  Accounting Integration existing tidak disentuh sama sekali, kecuali FinSupplierPayable
  (kolom baru, nullable, tidak mengubah perilaku baris lama).
revision_3_note: >
  Revisi 3 (25 September 2026) adalah AMENDMENT atas rumpun Accounting Integration dan Collection,
  dipicu balasan owner Accounting (evidence 13, ACC-DEC-082..091) dan koreksi hasil impact scan.
  Keputusan bisnisnya FIN-DEC-030..044 (approved); keputusan arsitekturnya FIN-DES-029..036
  (draft, menunggu approval owner).
  SATU PERUBAHAN PERILAKU pada source yang SUDAH BERJALAN: FIN-DEC-004 (HELD_FOR_FINALIZATION)
  digantikan FIN-DEC-030 — penerimaan sebelum tagihan final terbit segera sebagai
  PENERIMAAN-UANG-MUKA, tidak lagi ditahan. Ini satu-satunya amendment blueprint ini yang
  menyentuh kode berjalan (FinanceReceiptService, FinanceAccountingOutboxService).
  TUJUH KODE KEJADIAN BARU diusulkan (katalog naik 17 -> 24), seluruhnya MENUNGGU RATIFIKASI
  ACCOUNTING (FIN-OQ-017) dan karena itu EPIC FIN-14 berstatus OPEN DECISION di luar seluruh
  gelombang.
  TEMUAN YANG MENGUNTUNGKAN: deposit pasien, kelebihan bayar, dan selisih kas shift ternyata
  SUDAH dimiliki Billing sepenuhnya dan Finance sudah punya akses bacanya (FIN-CAP-022..024),
  sehingga NOL tabel baru dan NOL kontrak baru dari Billing. Hanya SATU migration untuk seluruh
  amendment: mengubah check constraint CK_FinBillingHandoffIntake_HandoffType dari 4 menjadi 8
  nilai.
  YANG TIDAK BERUBAH: seluruh FIN-DES-001..028 tetap berlaku; rumpun AR/AP/Payable/Cash/Master
  tidak disentuh sama sekali.
revision_2_note: >
  Revisi 2 (20 September 2026) adalah AMENDMENT atas rumpun Payable, dipicu keputusan modul
  Medical Fee (MF-DEC-002, MF-DEC-005, MF-DEC-008). Menutup FIN-OQ-013 dan FIN-OQ-014.
  Isinya: FinDoctorPayable DIGANTIKAN FinMedicalServicePayable yang melayani dokter maupun
  tenaga kesehatan lain; entity baru FinPaymentDeduction untuk potongan dan tambahan per
  pembayaran; kolom NetTransferAmount pada FinPayment. FIN-DES-025..028.
  YANG TIDAK BERUBAH, dan itu penting: seluruh rumpun MVP (data induk, intake Billing, piutang,
  penerimaan, alokasi/koreksi/write-off, kotak keluar) TIDAK disentuh sama sekali. Amendment ini
  hanya menyentuh EPIC FIN-08 dan FIN-09 yang keduanya POST-MVP dan NOL baris kode.
  Baseline revisi 1 beserta approval FIN-DES-001..024 TETAP berlaku apa adanya.
status: approved
status_note_revision_4: >
  `approved` sejak 25 September 2026. Saat memanggil /plan-module-delivery, owner memilih
  "Setujui FIN-DES-037..044 dan kunci kontraknya" atas pertanyaan eksplisit — mencakup (1)
  KEPUTUSAN ARSITEKTUR FIN-DES-037..044 dan (2) PENGUNCIAN FIN-API-1.1, FIN-INTEGRATION-1.2,
  FIN-STATE-1.2, FIN-VAL-1.2, FIN-PERM-1.1, FIN-TEST-1.2, FIN-MVP-1.3. Dicatat apa adanya,
  TIDAK ditetapkan skill. Approval terpisah dari keputusan bisnis FIN-DEC-045..056, konsisten
  dengan pola revisi 2 dan 3.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan tiga migration revisi 4
  (02-backend-architecture.md C.9); mendaftarkan folder submodul Purchasing baru ke registry
  kepemilikan modul; mengubah source; maupun mengaktifkan worker pengiriman
  PPN-MASUKAN-PEMBELIAN (tetap menunggu FIN-OQ-020).
status_note_revision_3: >
  `approved` sejak 25 September 2026. Owner menyatakan "Saya approve semua" atas dua hal yang
  ditawarkan penutup pass desain revisi 3: (1) KEPUTUSAN ARSITEKTUR FIN-DES-029..036, dan
  (2) PENGUNCIAN lima kontrak turunan ke 1.1. Dicatat apa adanya, TIDAK ditetapkan skill.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan migration (satu migration check
  constraint pada FIN-DES-029); mengubah source aplikasi; maupun mengaktifkan pengiriman kejadian
  ke Accounting. Ketiganya tetap memerlukan wewenang terpisah sesuai AGENTS.md.
  SATU HAL TIDAK IKUT TERANGKAT approval ini, karena bukan wewenang owner Finance:
  ratifikasi owner Accounting atas tujuh kode kejadian baru (FIN-OQ-017). EPIC FIN-14 tetap
  OPEN DECISION dan tetap di luar seluruh gelombang sampai Rizki menjawab.
status_scope_note: >
  Owner memberi DUA approval terpisah pada 20 September 2026:
    (1) KEPUTUSAN ARSITEKTUR FIN-DES-001..024 — "Saya setuju FIN-DES-001-024";
    (2) CAKUPAN MVP dan urutan gelombang pada 04-prd-to-mvp.md bagian 7, 8, dan 20.1 —
        dikunci apa adanya, tanpa perubahan epic.
  Pada hari yang sama owner memberi DUA approval tambahan sesudah /plan-module-delivery:
    (3) KEPUTUSAN ARSITEKTUR FIN-DES-025..028 (amendment revisi 2);
    (4) PENGUNCIAN versi kontrak turunan ke 1.0 — lihat contract_lock_note.
  Dengan demikian tidak ada lagi keputusan arsitektur Finance yang berstatus `draft`.
blueprint_shape: SINGLE
shape_decided_by: DESIGN_PASS_20_SEP_2026
shape_evidence: >
  Satu himpunan artefak tingkat modul untuk seluruh rumpun (AR, Collection, AP, Cash Management,
  Master Data, Accounting Integration). Tidak ada folder sub-modul dan tidak ada
  blueprint-manifest.md di dalam folder anak. Uji pemecahan dijalankan dan hasilnya SINGLE:
  keenam rumpun berbagi satu decision log, satu tabel kepemilikan data, dan satu rantai
  integrasi ke Accounting — memecahnya akan menduplikasi ketiganya. Mengikuti preseden
  billing-kasir (BIL-CASH-001) yang juga SINGLE dengan empat rumpun.
created_at: 2026-09-20T00:00:00+07:00
updated_at: 2026-09-23T00:00:00+07:00
last_owner_action: >
  25 September 2026 — owner MENYETUJUI FIN-DES-029..036 dan MENGUNCI lima kontrak turunan ke 1.1
  ("Saya approve semua"), lalu meminta /plan-module-delivery dijalankan. Sebelumnya pada hari yang
  sama: owner menjawab lima butir koreksi hasil impact scan (FIN-DEC-040..044,
  /grill-me Amendment pass lanjutan), menyetujui pengiriman surat koreksi ke owner Accounting
  (evidence/05), dan meminta impact scan formal dijalankan sebelum blocker
  BILLING-COLLECTION-HANDOFF ditutup. Sebelumnya pada hari yang sama: FIN-DEC-030..039 dijawab.
  Sebelumnya, 23 September 2026 — UI brief frontend FE-FIN-* dijawab lengkap (FIN-DEC-024..029, /grill-me
  Amendment pass), menutup roadmap_status ACTIVE_BLOCKED_ON_UI_BRIEF di 02-frontend-roadmap.md.
  Sebelumnya, 20 September 2026 — approval FIN-DES-025..028, penguncian tujuh kontrak turunan
  ke 1.0, dan penegasan kepemilikan modul oleh Yasmin.
owners:
  product_domain: >
    Yasmin — Product/Domain Owner modul Finance Management (AR/AP). Yasmin juga owner modul
    Medical Fee (MF-BP-001). Kepemilikan ganda ini DITEGASKAN owner 20 September 2026 dan TIDAK
    meruntuhkan batas antar keduanya: Finance tetap MENGONSUMSI MdfFinanceHandoff dan tidak
    pernah menulis ke tabel Medical Fee, sebagaimana FIN-DES-025 menetapkannya.
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: >
    Product Owner (Yasmin) — UI brief closed 23 September 2026 lewat /grill-me, tercatat
    FIN-DEC-024..029 di 00-interview-decisions.md bagian Frontend Decision Authority. Lihat
    juga 02-frontend-roadmap.md bagian 5 dan 03-frontend-architecture.md untuk kontrak
    fungsional yang tidak berubah oleh amendment ini.
  cross_module_billing: Billing Owner (FIN-DEC-005, FIN-DEC-006 menunggu konfirmasi)
  cross_module_accounting: Rizki (owner Accounting)
  cross_module_hr: HR Owner (FIN-DEC-016 menunggu konfirmasi)
approved_by: Yasmin (Product/Domain Owner Finance)
approved_at: 2026-09-20
approval_note: >
  Owner menyatakan "Saya setuju FIN-DES-001-024" pada 20 September 2026. Approval itu dicatat
  apa adanya, TIDAK ditetapkan skill.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan migration; mengubah source
  aplikasi backend maupun frontend; mendaftarkan sendiri enam submodul baru ke registry
  kepemilikan modul; maupun mengaktifkan pengiriman kejadian ke Accounting. Keempatnya tetap
  memerlukan wewenang terpisah sesuai AGENTS.md.
  Lihat `status_scope_note` untuk tiga hal yang TIDAK ikut terangkat approval ini.

backend_commit_sha: 96bf9746
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: abed49b03
frontend_branch: (branch aktif QuilvianSystemFrontendDev saat audit)
sha_verification_note_revision_3_pass2: >
  SHA backend DINAIKKAN ke 96bf9746 pada 25 September 2026 sesudah /trace-existing-capabilities
  untuk tiga scope baru (01-existing-capability-map.md bagian 12) — FIN-SC-008 Purchasing/AP,
  FIN-SC-009 AR Invoice Agregat, FIN-SC-010 Potongan sisi AR. Dipicu evidence eksternal
  Keuangan.md dan keputusan bisnis FIN-DEC-045..055.
  Verifikasi staleness: d6cdfaf9 -> 96bf9746 adalah 45 commit fast-forward (bukan diverged);
  git diff --stat dikonfirmasi NOL perubahan pada Areas/Corporate/FinanceManagement/Payable/,
  MstSupplier.cs, atau area Purchasing manapun — seluruh 45 commit itu di luar boundary rumpun
  yang diaudit di sini (BillingPayerEditService.cs/BillingRefundService.cs/CashierShiftService.cs,
  bertepatan dengan implementasi BE-BUI-001/002 pada blueprint billing-kasir).
  SEBELAS entri kemampuan baru ditambahkan (FIN-CAP-026..036), termasuk TEMUAN PENTING:
  FinancePaymentService.ResolveApprovalTier SUDAH memakai placeholder Rp 50.000.000 - angka yang
  PERSIS SAMA dengan FIN-DEC-052 yang baru disepakati, dan BillingCompanyGuarantorInvoiceDocumentService
  (per-invoice, bukan agregat) dapat dipakai ulang sebagai rincian di balik AR Invoice Agregat.
sha_verification_note_revision_3: >
  SHA backend DINAIKKAN ke d6cdfaf9 pada 25 September 2026 sesudah impact scan read-only
  (01-existing-capability-map.md bagian 9.3). Baseline 09101d05 dipertahankan pada field
  backend_commit_sha_baseline karena bagian 1-10 02-backend-architecture.md, AMENDMENT REVISI 2,
  dan bagian 1-8 capability map masih mencerminkan SHA itu dan TIDAK diaudit ulang menyeluruh.
  Yang diverifikasi pada d6cdfaf9: keberadaan BilCollectionHandoff beserta konsumennya,
  BilDepositAccount/BilDepositMovement, BilRefundableCredit/BilRefundCase,
  BilCashVarianceReview/BilCashierShift, check constraint FinBillingHandoffIntake dan
  FinAccountingEventOutbox, serta ketiadaan endpoint penerima Accounting Event.
  BELUM diaudit field-per-field: rumpun AR/AP/Payable Finance yang dibangun BE-FIN-001..021
  (59 berkas, 9062 baris) — lihat "Yang TIDAK ditutup" pada capability map bagian 9.3.
  Frontend SHA TIDAK diverifikasi ulang pada revisi 3; revisi ini tidak menyentuh frontend.
sha_verification_note: >
  Kedua SHA diverifikasi ulang pada awal pass desain 20 September 2026 dan TIDAK bergerak dari
  baseline capability map. Working tree backend bersih kecuali folder
  docs/module-blueprints/finance-management/ yang belum ter-track (keluaran pass ini sendiri).
  Karena SHA tidak bergerak, impact scan tambahan TIDAK dijalankan dan memang tidak diperlukan.

input_revisions:
  decisions: >
    FIN-DEC-001..055. FIN-DEC-001..023 approved 20 September 2026; FIN-DEC-024..029 approved
    23 September 2026; FIN-DEC-030..039 approved 25 September 2026; FIN-DEC-040..044 approved
    25 September 2026 (Amendment pass lanjutan); FIN-DEC-045..055 approved 25 September 2026
    (Amendment pass rumpun Purchasing/AP + AR Invoice Agregat, dipicu Keuangan.md).
    SUPERSEDED: FIN-DEC-004 (oleh 030), FIN-DEC-007 sebagian (oleh 036), FIN-DEC-015 (oleh 045),
    FIN-DEC-023 (oleh 035), FIN-DEC-032 (oleh 040 dan 041), FIN-DEC-033 (oleh 042).
    CLOSED pada closure pass lanjutan: FIN-OQ-010 dan FIN-OQ-019 (oleh 052), FIN-OQ-021 (oleh
    054 dan 055); FIN-OQ-020 dipersempit (oleh 053) — murni menunggu ratifikasi Accounting.
  capability_map: >
    revisi 1 — 20 September 2026 diaudit pada 09101d05 / abed49b03; bagian 9.3 ditambahkan
    25 September 2026 dari impact scan pada d6cdfaf9 (FIN-CAP-007/008/009 diperbarui dari
    Missing menjadi Ready to reuse; FIN-CAP-022..025 ditambahkan); bagian 12-14 ditambahkan
    25 September 2026 (pass kedua) dari /trace-existing-capabilities pada 96bf9746 untuk
    FIN-SC-008/009/010 (FIN-CAP-026..036 ditambahkan).
  requirement_gate: NOT_RUN — lihat requirement_completeness_gate_note
  hospital_domain_architecture: NOT_RUN — lihat domain_architecture_readiness
  owner_analysis_documents: >
    FIN-BRD-V2-0.3, FIN-PRD-V2-0.3, FIN-MVP-PRD-V2-0.3, FIN-ACC-XMOD-V2-0.3 — keempatnya
    bertanggal analisis 18 September 2026, dipakai sebagai masukan yang SUDAH diratifikasi
    sebagian lewat wawancara 20 September 2026. Dokumen itu BUKAN kontrak; yang mengikat adalah
    00-interview-decisions.md.
    Keuangan.md — evidence eksternal (analisis video sistem rujukan), TIDAK otoritatif, dipakai
    murni sebagai peta area untuk deteksi gap pada Amendment pass 25 September 2026.
input_hashes:
  00-interview-decisions.md: eb776fb181ee1f46f0cf0eac7f82df323e2e82b4b6c2698d61a3e76b6730da71  # revisi 5: FIN-DEC-061/062, FIN-OQ-026 diperluas
  01-existing-capability-map.md: 42e95d720d42a288220f3919cd2e80fa2fe8361cb7362df1cbc92f4431e7e2b9
input_hashes_note_revision_4: >
  Hash decision log BERGERAK TIGA KALI 25 September 2026, bukan drift: (1) FIN-DEC-056 dan
  FIN-OQ-022..025 ditambahkan oleh /plan-module-delivery (nilai 5fd9d74e...); (2) FIN-DEC-057..060
  ditambahkan oleh /grill-me closure pass yang menutup keempat FIN-OQ itu dan membuka FIN-OQ-026
  (nilai sebelumnya 8c932826..., lalu 5fd9d74e...). Hash capability map tidak bergerak sejak
  trace pass kedua.
input_hashes_note: >
  SHA256 atas isi berkas apa adanya. Nilai awal dihitung 20 September 2026; DIPERBARUI
  25 September 2026 sesudah Amendment pass rumpun Purchasing/AP + AR Invoice Agregat (FIN-DEC-045..055)
  dan /trace-existing-capabilities pass kedua (bagian 12-14 capability map). Bila salah satu
  hash berubah lagi tanpa revisi manifest ini ikut naik, artefak desain di bawahnya berpotensi
  drift dan MUST diperiksa ulang sebelum dipakai /plan-module-delivery.
  Hash capability map SUDAH BERGERAK DUA KALI pada pass desain ini, dan keduanya bukan drift:
    c5fd6705... -> 704194cd...  menambahkan FIN-CAP-020 dan FIN-CAP-021 hasil pemeriksaan
                                terarah, menutup FIN-CQ-02, membuka FIN-CQ-03 (bagian 9.1);
    704194cd... -> 0576bf65...  MENGOREKSI FIN-CAP-020 dari `Ready to reuse` menjadi `Conflict`
                                setelah isi MstDoctorServiceRule dibaca langsung dan ternyata
                                bukan aturan perhitungan fee (bagian 9.2).
  Nilai di atas adalah hash SESUDAH kedua pembaruan tersebut.

contract_versions:
  api-contract: FIN-API-1.1 — draft 2026-09-25, naik dari 1.0 (locked). AMENDMENT REVISI 4 —
    tujuh grup endpoint baru (Purchase Order, Goods Receipt, Invoice Exchange, Purchasing
    Invoice, Supplier Return, Purchasing Reports, Receivable Invoice Batch), dua endpoint baru
    pada grup Receipt
  integration-contract: FIN-INTEGRATION-1.2 — draft 2026-09-25, naik dari 1.1 (locked). Bagian
    5.8 baru (kode ke-25 PPN-MASUKAN-PEMBELIAN, diusulkan), bagian 6 dan 7 diperbarui
  state-transition-matrix: FIN-STATE-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT
    REVISI 4 — tujuh entity baru (bagian B.1-B.8)
  validation-matrix: FIN-VAL-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT REVISI 4 —
    FIN-VAL-100..122 baru
  permission-audit-matrix: FIN-PERM-1.1 — draft 2026-09-25, naik dari 1.0 (locked). AMENDMENT
    REVISI 4 — tujuh Resource baru, dua Action baru pada grup Receipt
  prd-to-mvp: FIN-MVP-1.3 — draft 2026-09-25, naik dari 1.1 (locked untuk MVP-0..5). AMENDMENT
    REVISI 4 (1.2) — EPIC FIN-15/16/17 baru, FR-FIN-081..095, EPIC FIN-07 digantikan FIN-15;
    (1.3) FIN-DEC-056 mempersempit gerbang FIN-OQ-020 — EPIC FIN-15 boleh masuk
    /plan-module-delivery tanpa menunggu ratifikasi Accounting
  acceptance-test-matrix: FIN-TEST-1.2 — draft 2026-09-25, naik dari 1.1 (locked). AMENDMENT
    REVISI 4 — bagian B.1-B.7 baru
contract_versions_revision_5: >
  Seluruhnya locked 26 September 2026 bersama FIN-DES-045..050: FIN-API-1.2, FIN-INTEGRATION-1.3,
  FIN-STATE-1.3, FIN-VAL-1.3, FIN-PERM-1.2, FIN-TEST-1.3, FIN-MVP-1.4. Nilai pada blok
  contract_versions di atas adalah versi revisi 4 dan sudah digantikan baris ini.
contract_versions_note_revision_4: >
  Enam kontrak turunan plus prd-to-mvp naik versi 25 September 2026 karena revisi 4 menambah
  endpoint/status/aturan/hak-akses/epic baru. SELURUHNYA DIKUNCI owner 25 September 2026
  bersama approval FIN-DES-037..044 (lihat status_note_revision_4). Label `draft` pada entri
  contract_versions di atas sudah tidak berlaku — status yang berlaku adalah `locked`.
  Konsekuensi: kerja paralel backend-frontend DIIZINKAN untuk task EPIC FIN-15/16/17.
contract_versions_note_revision_3: >
  Lima kontrak naik ke 1.1 pada 25 September 2026; dua TIDAK bergerak dan sengaja tidak disunting
  (api-contract, permission-audit-matrix) sesuai aturan bahwa file contract yang isinya tidak
  berubah MUST NOT disunting hanya untuk menaikkan angka. `last_changed_in` masing-masing tertulis
  di kepala berkasnya.
  Kelima yang naik DIKUNCI owner pada 25 September 2026 lewat pernyataan "Saya approve semua",
  sama seperti penguncian 1.0 pada 20 September 2026. Dicatat apa adanya.
  Konsekuensi langsung: kerja paralel backend-frontend DIIZINKAN untuk task yang kontraknya
  terkunci, termasuk rumpun Accounting Integration — KECUALI bagian yang bergantung pada nama
  tujuh kode kejadian, yang tetap menunggu FIN-OQ-017.
contract_lock_note_revision_3: >
  Diperbarui 25 September 2026. Dari tiga permukaan yang tidak terkunci pada 1.0, SATU sudah
  tertutup: BilCollectionHandoff kini ada dan sudah dikonsumsi (FIN-CAP-007, FIN-CAP-025).
  Yang tersisa dua — perluasan BilArHandoff untuk manfaat karyawan, dan pengaktifan pengiriman
  ke Accounting — ditambah SATU permukaan baru: katalog tujuh kode kejadian (FIN-OQ-017), yang
  bentuk pesannya sudah final tetapi nama kodenya belum diratifikasi Accounting.
  Kelima kontrak yang naik ke 1.1 BELUM dikunci; penguncian adalah tindakan owner terpisah.
contract_lock_note: >
  Owner menyetujui penguncian ke 1.0 pada 20 September 2026, atas usulan
  roadmap/00-delivery-roadmap.md bagian 2. Dicatat apa adanya, TIDAK ditetapkan skill.
  TIGA PERMUKAAN TETAP TIDAK TERKUNCI, dan alasannya bukan status FIN-DES melainkan
  ketergantungan pada pihak lain:
    (1) BilCollectionHandoff — bentuknya milik owner Billing, belum dikonfirmasi (FIN-DEC-005)
        — CATATAN REVISI 3: butir ini sudah TERTUTUP, lihat contract_lock_note_revision_3;
    (2) perluasan BilArHandoff untuk manfaat karyawan — FIN-DEC-006/FIN-DEC-016 belum turun;
    (3) pengiriman kejadian ke Accounting — EPIC FIN-12 OPEN DECISION (FIN-CAP-018, FIN-DEC-007).
  Perubahan pada ketiganya TIDAK menaikkan versi kontrak; ia menambah permukaan baru yang
  dikunci tersendiri saat jawabannya turun.
  Konsekuensi langsung penguncian: kerja paralel backend-frontend kini DIIZINKAN untuk task
  yang kontraknya terkunci.
external_contract_dependencies:
  ACC-XMOD-0.2: >
    Kontrak Finance → Accounting milik modul Accounting. Diratifikasi sisi Finance lewat
    FIN-DEC-001. Finance MUST mengikuti bentuk 12 field kontrak itu; setiap perubahan bentuk
    pesan adalah wewenang Accounting, bukan blueprint ini.
  BIL-CASH-001: >
    Blueprint billing-kasir. Sumber BilArHandoff/BilApHandoff/BilTender/BilSettlement/
    BilPaymentAllocation/BilCashierShift, DAN pemilik seluruh keputusan operasional Petty Cash
    (PC-DEC-*/PC-DES-*) per FIN-DEC-009.

design_decision_ids: [FIN-DES-001, FIN-DES-002, FIN-DES-003, FIN-DES-004, FIN-DES-005, FIN-DES-006, FIN-DES-007, FIN-DES-008, FIN-DES-009, FIN-DES-010, FIN-DES-011, FIN-DES-012, FIN-DES-013, FIN-DES-014, FIN-DES-015, FIN-DES-016, FIN-DES-017, FIN-DES-018, FIN-DES-019, FIN-DES-020, FIN-DES-021, FIN-DES-022, FIN-DES-023, FIN-DES-024, FIN-DES-025, FIN-DES-026, FIN-DES-027, FIN-DES-028, FIN-DES-029, FIN-DES-030, FIN-DES-031, FIN-DES-032, FIN-DES-033, FIN-DES-034, FIN-DES-035, FIN-DES-036, FIN-DES-037, FIN-DES-038, FIN-DES-039, FIN-DES-040, FIN-DES-041, FIN-DES-042, FIN-DES-043, FIN-DES-044, FIN-DES-045, FIN-DES-046, FIN-DES-047, FIN-DES-048, FIN-DES-049, FIN-DES-050]
design_decision_ids_note_revision_4: >
  Array ini sebelumnya berhenti di FIN-DES-028 walau revisi 3 sudah menambah FIN-DES-029..036 —
  kesenjangan pencatatan dari pass sebelumnya, DIPERBAIKI di sini sekaligus menambah
  FIN-DES-037..044 dari revisi 4. Status approval masing-masing tetap mengikuti catatan di
  design_decision_status/_revision_2/_revision_3 di atas dan status_note_revision_4 (dekat
  bagian atas berkas ini) — FIN-DES-029..044 seluruhnya `draft`, belum disetujui owner.
design_decision_status_revision_2: >
  FIN-DES-025..028 `approved` 20 September 2026 oleh Yasmin (Product/Domain Owner Finance),
  lewat pernyataan langsung "Setujui FIN-DES-025..028". Dicatat apa adanya, TIDAK ditetapkan
  skill. Approval ini diberikan TERPISAH dari approval revisi 1, sebagaimana seharusnya —
  keempatnya tidak pernah dianggap ikut terangkat oleh approval keputusan Medical Fee di
  hulunya.
  Approval ini BUKAN otorisasi untuk membuat atau menjalankan migration, dan BUKAN otorisasi
  memulai BE-FIN-020: task itu masih tertahan FIN-OQ-010 (ambang nominal approval AP).
  SATU keputusan revisi 1 DIPERSEMPIT: FIN-DES-015 (satu entity FinPayment untuk supplier dan
  dokter) tetap berlaku, tetapi invariant "TotalAmount = jumlah alokasi" kini didampingi
  invariant kedua untuk nilai transfer bersih — lihat FIN-DES-027. Tidak ada keputusan revisi 1
  yang dicabut.
design_decision_status: >
  SELURUH FIN-DES-001..024 `approved` 20 September 2026 oleh Yasmin (Product/Domain Owner
  Finance), lewat pernyataan langsung "Saya setuju FIN-DES-001-024".
  SATU di antaranya diperkuat penegasan terpisah pada hari yang sama: FIN-DES-003 (prefix `Mst`
  untuk entity yang berperan sebagai data induk di dalam FinanceManagement, `Fin` untuk entity
  lainnya). Owner menyatakannya sendiri sebagai aturan, bukan sekadar menyetujui usulan desain —
  sehingga aturan itu kini berlaku sebagai KETENTUAN MODUL, tidak hanya sebagai keputusan
  arsitektur satu pass. Konsekuensinya: penamaan `FinBank`/`FinBankAccount`/`FinCurrency` pada
  FIN-PRD-V2-0.3 bagian 11 resmi digantikan `MstBank`/`MstBankAccount`/`MstCurrency`/
  `MstExchangeRate`, dan entity master Finance berikutnya mengikuti aturan yang sama.

requirement_completeness_gate_note: >
  NOT_RUN, dan ini DEVIASI YANG DICATAT atas keputusan owner 20 September 2026, bukan langkah
  yang terlewat. Sebagai gantinya dipakai: (1) 00-interview-decisions.md dengan 23 keputusan
  approved lengkap beserta owner, evidence, dan tanggal; (2) 01-existing-capability-map.md
  dengan 19 kemampuan berbukti source langsung beserta path dan SHA. Pola pencatatan mengikuti
  preseden billing-kasir (requirement_completeness_gate_multi_payer).
  RISIKO YANG DITERIMA: requirement sisi HR untuk employee benefit belum pernah dinilai
  kelengkapannya oleh pemilik HR. Konsekuensinya diisolasi — lihat domain_architecture_readiness.

domain_architecture_readiness: >
  DOMAIN_ARCHITECTURE_NOT_RUN, deviasi tercatat atas keputusan owner 20 September 2026.
  Alasan yang dapat diuji:
    (1) Finance Management TIDAK mengambil keputusan klinis apa pun dan tidak berdampak pada
        keselamatan pasien — ia subledger keuangan di hilir.
    (2) Finance TIDAK PERNAH menulis balik ke modul sumber. Arahnya satu arah masuk
        (Billing → Finance, Medical Fee → Finance) dan satu arah keluar (Finance → Accounting).
        Larangan menulis ini eksplisit pada aturan bisnis #9 dan #12 serta FIN-OOS-001..004.
    (3) Seluruh batas lintas bounded context yang relevan SUDAH diputus eksplisit oleh keputusan
        bisnis: FIN-DEC-001/002/008/023 (Accounting), FIN-DEC-005/006/016 (Billing),
        FIN-DEC-003/019 (Medical Fee), FIN-DEC-009 (Petty Cash/billing-kasir),
        FIN-DEC-014 (Administrator/MstSupplier). Tidak ada batas domain tersisa yang perlu
        diselesaikan hospital-domain-architect.
  SATU RISIKO TERBUKA YANG TIDAK DITUTUP DEVIASI INI: rumpun employee benefit menyentuh HR
  (identitas pemilik manfaat dan eligibilitas), dan FIN-DEC-006 serta FIN-DEC-016 keduanya
  bertanda "butuh konfirmasi Billing + HR" yang belum turun. Karena itu seluruh rumpun employee
  benefit ditandai OPEN DECISION pada 04-prd-to-mvp.md dan MUST NOT masuk gelombang pengiriman
  mana pun sampai konfirmasi itu ada. Pemisahan ini yang membuat deviasi dapat diterima:
  bagian yang requirement-nya paling tipis justru yang paling tegas dikeluarkan dari MVP.

readiness_revision_3: >
  DESIGN_PARTIAL. Baseline revisi 1-2 tetap DESIGN_APPROVED dan CONTRACTS_LOCKED pada 1.0;
  revisi 3 berstatus DESIGN_DRAFT dan CONTRACTS_UNLOCKED pada 1.1.
  YANG SUDAH TERBUKA sejak readiness lama ditulis:
    - Blocker BILLING-COLLECTION-HANDOFF TERTUTUP (impact scan 25 September 2026), sehingga
      MVP-2 dan MVP-3 kini juga dapat direncanakan. Seluruh MVP-0..MVP-5 tidak lagi menunggu
      pihak luar.
    - FIN-OQ-011 (nama field saldo subledger) TERTUTUP oleh FIN-DEC-035.
  YANG MENAHAN revisi 3, dan HANYA menyentuh rumpun Accounting Integration + Collection:
    (i)  Approval owner atas FIN-DES-029..036 dan atas penguncian lima kontrak ke 1.1 — belum ada.
    (ii) Ratifikasi owner Accounting atas TUJUH kode kejadian baru (FIN-OQ-017). Ini yang membuat
         EPIC FIN-14 berstatus OPEN DECISION dan membuat perubahan FIN-DEC-030 belum boleh
         dieksekusi di source, walaupun keputusan bisnisnya sudah approved dan buktinya lengkap.
  PRASYARAT IMPLEMENTASI TAMBAHAN revisi 3, di luar yang sudah tercatat di readiness lama:
    (f) Otorisasi terpisah untuk SATU migration pengubah check constraint
        CK_FinBillingHandoffIntake_HandoffType (FIN-DES-029). Desain ini TIDAK memberi wewenang itu.
    (g) Pembacaan jumlah baris berstatus HELD_FOR_FINALIZATION sebelum pengiriman diaktifkan
        (02-backend-architecture.md bagian B.6) — pembacaan database, wewenangnya tetap terpisah.
    (h) Audit field-per-field rumpun AR/AP/Payable Finance lewat trace-existing-capabilities
        sebelum blueprint dipakai sebagai acuan penuh AR/AP.
readiness: >
  DESIGN_APPROVED dan CONTRACTS_LOCKED. Seluruh FIN-DES-001..028 disetujui owner 20 September
  2026, dan tujuh kontrak turunan dikunci ke 1.0 pada hari yang sama.
  Roadmap pengiriman FIN-ROADMAP-001 sudah diturunkan; MVP-0, MVP-1, MVP-4, dan MVP-5 siap
  dimulai.
  YANG MASIH MENAHAN /plan-module-delivery: satu ketergantungan lintas modul, yaitu konfirmasi
  owner Billing atas bentuk BilCollectionHandoff (FIN-DEC-005). Ia menahan gelombang MVP-2 saja.
  Gelombang MVP-0 (data induk) dan MVP-1 (pintu masuk fakta Billing + buku piutang) TIDAK
  bergantung padanya dan sudah dapat direncanakan.
  PRASYARAT IMPLEMENTASI yang TERSISA dan MUST diminta terpisah saat eksekusi (BUKAN blocker
  perencanaan):
    (a) Pendaftaran folder submodul baru pada docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
        sebelum file model pertama ditulis (QBE-MOD-003, QBE-NAM-004) — lihat FIN-DES-002.
    (b) Otorisasi terpisah untuk membuat dan menjalankan migration (AGENTS.md, Keselamatan
        Database). Desain ini TIDAK memberi wewenang itu.
    (c) Konfirmasi owner Billing atas FIN-DEC-005 (BilCollectionHandoff) dan FIN-DEC-006
        (perluasan BilArHandoff) sebelum task lintas repository/lintas modul dimulai.
    (d) Konfirmasi owner HR atas FIN-DEC-016 sebelum rumpun employee benefit dimulai.
    (e) ~~FIN-OQ-010 (ambang nominal approval AP) sebelum aturan validasi angka dikunci.~~
        DITUTUP 25 September 2026 — Rp 50.000.000, FIN-DEC-052. Konsisten dengan placeholder
        yang SUDAH ditulis di FinancePaymentService.ResolveApprovalTier (FIN-CAP-035).
readiness_note_rumpun_baru: >
  25 September 2026 (pass ketiga — /design-business-module). Tiga scope BARU (FIN-SC-008
  Purchasing/AP, FIN-SC-009 AR Invoice Agregat, FIN-SC-010 Potongan AR) kini berstatus
  DESIGN_DRAFT — 02-backend-architecture.md AMENDMENT REVISI 4 (FIN-DES-037..044),
  03-frontend-architecture.md bagian 12, erd/purchasing-ap.md (baru) dan amendment pada
  erd/payable.md serta erd/receivable-collection.md, enam kontrak turunan naik versi,
  04-prd-to-mvp.md EPIC FIN-15/16/17. DESIGN_APPROVED dan CONTRACTS_LOCKED 25 September 2026
  (status_note_revision_4); roadmap diturunkan pada pass /plan-module-delivery yang sama.
  DIPERBARUI SESUDAH pass desain ini, 25 September 2026: FIN-OQ-020 (ratifikasi Accounting atas
  kode PPN-MASUKAN-PEMBELIAN) semula memblokir SELURUH EPIC FIN-15 masuk /plan-module-delivery
  (FIN-DEC-046). Owner meninjau ulang dan meminta gerbang itu dipersempit (FIN-DEC-056) —
  alasannya: Finance adalah titik asal (upstream) bagi Accounting, dan pola outbox transaksional
  yang sama persis dengan EPIC FIN-11 (kotak keluar sudah berjalan MVP-0..5 tanpa endpoint
  Accounting ada) sudah cukup mengisolasi risikonya. Sekarang EPIC FIN-15 BOLEH diteruskan penuh
  ke /plan-module-delivery; yang TETAP tertahan FIN-OQ-020 hanya AKTIVASI WORKER pengiriman
  kode PPN Masukan itu sendiri, bukan epic-nya. EPIC FIN-16 (AR Invoice Agregat) dan FIN-17
  (Potongan AR) tetap tidak pernah tertahan gerbang ini — keduanya murni internal Finance.

blocking_questions:
  - id: BILLING-COLLECTION-HANDOFF
    status: CLOSED 2026-09-25
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Ditutup lewat impact scan 25 September 2026: BilCollectionHandoff
      sudah dibangun owner Billing (BKC-DES-037, 22 September 2026) DAN sudah dikonsumsi
      FinanceBillingIntakeService.SyncNewFactsAsync/ProcessAsync (BE-FIN-016). Bukti:
      01-existing-capability-map.md FIN-CAP-007 dan FIN-CAP-025, diverifikasi pada d6cdfaf9.
      Gelombang MVP-2 dan MVP-3 kini setara MVP-0/MVP-1 dari sisi ketergantungan luar.
      Sisa yang belum diverifikasi: kecocokan field-per-field bentuk kolomnya terhadap
      contracts/integration-contract.md bagian 2.1 — bukan blocker, cukup pemeriksaan saat task
      MVP-2 dimulai.
  - id: FIN-OQ-010
    blocking_for: EPIC FIN-09 (Pembayaran AP) — hanya aturan validasi angkanya, bukan modelnya. POST-MVP
  - id: FIN-OQ-011
    status: CLOSED 2026-09-25
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Accounting menetapkan bentuk pesan saldo subledger pada 24 September
      2026 dan Finance menerimanya apa adanya (FIN-DEC-035). Bentuk finalnya ada di
      contracts/integration-contract.md bagian 5.6.
  - id: FIN-OQ-017
    status: OPEN — dibuka 2026-09-25
    blocking_for: >
      EPIC FIN-14 (uang muka, deposit, kelebihan bayar, selisih kas) DAN implementasi FIN-DEC-030
      pada source yang sudah berjalan. Menunggu ratifikasi owner Accounting atas TUJUH kode
      kejadian baru; sudah dikirim lewat evidence/04 dan dikoreksi evidence/05.
      TIDAK memblokir MVP-0..MVP-5: EPIC FIN-14 sudah dikeluarkan dari seluruh gelombang, dan
      kotak keluar tetap aman terisi tanpa pengiriman aktif.
      Ini juga gerbang G6 milik Accounting.
  - id: FIN-OQ-016
    status: OPEN
    blocking_for: >
      Pengaktifan pengiriman (EPIC FIN-12) dan gerbang cutover G3. Mekanisme token/kredensial akun
      layanan menunggu Platform; TIGA syarat organisasinya sudah ditetapkan FIN-DEC-036 dan
      tercatat di contracts/integration-contract.md bagian 5.1.
  - id: FIN-OQ-018
    status: OPEN — dibuka 2026-09-25
    blocking_for: >
      Tidak memblokir apa pun saat ini. Lawan jurnal refund Billing bersumber SETTLEMENT dan
      REFERRED_OUTPATIENT_ADMIN sengaja DIKELUARKAN dari cakupan PENGEMBALIAN-UANG-MUKA
      (FIN-DEC-041), dan gerbang G6 tidak menuntutnya.
  - id: FIN-CQ-03
    blocking_for: EPIC FIN-04 (Employee benefit AR) — menunggu konfirmasi Billing + HR
  - id: FIN-CAP-021
    blocking_for: EPIC FIN-08 (Utang dokter) — menunggu modul Medical Fee dibangun
  - id: FIN-OQ-020
    status: OPEN — dipersempit DUA KALI, 2026-09-25 (FIN-DEC-053, lalu FIN-DEC-056), dikirim evidence/06
    blocking_for: >
      TIDAK LAGI memblokir EPIC FIN-15 masuk /plan-module-delivery. Dipersempit FIN-DEC-056
      (25 September 2026) atas permintaan owner: Finance adalah titik asal (upstream) bagi
      Accounting, sehingga kesiapan Finance MUST NOT digantungkan pada kecepatan ratifikasi
      hilir — preseden EPIC FIN-11/FIN-CAP-018 (kotak keluar sudah berjalan penuh MVP-0..5 tanpa
      endpoint Accounting ada). Yang TERSISA, sempit: menunggu ratifikasi owner Accounting atas
      SATU kode kejadian baru (PPN-MASUKAN-PEMBELIAN, kode ke-25) sebelum WORKER PENGIRIMANNYA
      diaktifkan. Baris outbox-nya tetap ditulis PENDING sejak Purchasing Invoice pertama
      disetujui, tidak menahan pembangunan atau perencanaan EPIC FIN-15 sama sekali.
      TIDAK memblokir /design-business-module (sudah selesai, revisi 4) maupun EPIC FIN-16/17.
  - id: FIN-OQ-022
    status: CLOSED 2026-09-25 (/grill-me, FIN-DEC-060)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. Submenu "Pembelian" + butir flat "Tagihan Gabungan Penjamin" +
      label "Faktur Pembelian" diputuskan. Implementasi menyusul BE-FIN-032/033/037, BE-FIN-039.
  - id: FIN-OQ-023
    status: CLOSED sisi keputusan bisnis 2026-09-25 (/grill-me, FIN-DEC-057)
    blocking_for: >
      Deposit Retur dipakai sebagai baris alokasi non-tunai di dalam FinPayment — keputusan
      bisnis SUDAH ADA. YANG MASIH MENAHAN BE-FIN-036: konsekuensi skema (FinPaymentAllocation/
      FinSupplierReturnDepositUsage perlu penanda sumber alokasi) BELUM digambar
      /design-business-module. BE-FIN-036 tetap ⛔ sampai amendment arsitektur berikutnya.
  - id: FIN-OQ-024
    status: CLOSED sisi Finance 2026-09-25 (/grill-me, FIN-DEC-058)
    blocking_for: >
      Kode POTONGAN-PIUTANG-NON-TUNAI (ke-26) diusulkan. Ratifikasi Rizki dicatat terpisah
      sebagai FIN-OQ-026 — hanya menahan aktivasi worker (pola FIN-DEC-056), TIDAK menahan
      BE-FIN-040/FE-FIN-013 dari /plan-module-delivery. Surat evidence belum dikirim.
  - id: FIN-OQ-025
    status: CLOSED 2026-09-25 (/grill-me, FIN-DEC-059)
    blocking_for: >
      TIDAK LAGI MEMBLOKIR. GET /purchasing/reports/aging dicabut dari FIN-API-1.1; layar
      memakai GET api/finance/payable/aging existing. BE-FIN-037 membangun empat laporan lain.
  - id: FIN-OQ-026
    status: OPEN — dibuka /grill-me 2026-09-25
    blocking_for: >
      Ratifikasi Rizki (Accounting) atas kode POTONGAN-PIUTANG-NON-TUNAI. Hanya menahan aktivasi
      worker pengiriman kode ini (pola FIN-DEC-056), tidak menahan EPIC FIN-17 masuk
      /plan-module-delivery. Surat evidence belum dikirim — menyusul saat diminta eksplisit.
```

## Daftar artefak blueprint

| Berkas | Status | Keterangan |
|---|---|---|
| `blueprint-manifest.md` | Ada | Berkas ini |
| `00-interview-decisions.md` | Ada | Keluaran `/grill-me`, **44 keputusan** `FIN-DEC-001`..`044`; lima `superseded` (`004`, `007` sebagian, `023`, `032`, `033`) |
| `01-existing-capability-map.md` | Ada | Keluaran `/trace-existing-capabilities`, **25 kemampuan** berbukti; bagian 9.3 memuat impact scan `d6cdfaf9` (25 September 2026) |
| `02-backend-architecture.md` | Ada | Arsitektur backend, `FIN-DES-001`..`044`. **AMENDMENT REVISI 4** (`FIN-DES-037`..`044`, `draft`) — Purchasing/AP, AR Invoice Agregat, Potongan AR, 14 tabel baru + 1 diperbarui, di akhir dokumen |
| `03-frontend-architecture.md` | Ada | Kontrak fungsional frontend dan matriks kewenangan UI. **Bagian 12 baru** (revisi 4) — layar Purchasing/AP, Batch Tagihan AR, Potongan Penerimaan |
| `04-prd-to-mvp.md` | Ada | `FIN-MVP-1.3` `draft` — batas rilis, epic, UAT, DoD. **`EPIC FIN-15`/`16`/`17` baru** (`POST-MVP`, revisi 4), `FR-FIN-081`..`095` baru; `EPIC FIN-07` digantikan `FIN-15`; `EPIC FIN-14` (revisi 3) tetap `OPEN DECISION`. **`EPIC FIN-15` boleh masuk `/plan-module-delivery` tanpa menunggu `FIN-OQ-020`** (`FIN-DEC-056`) |
| `erd/00-context-erd.md` | Ada | Peta antar bounded context. **Diperbarui revisi 4** — konteks `FIN_PURCHASING` ditambahkan |
| `erd/receivable-collection.md` | Ada | ERD rumpun Piutang dan Penerimaan. **AMENDMENT REVISI 4** (bagian D) — AR Invoice Agregat, Potongan AR |
| `erd/payable.md` | Ada | ERD rumpun Utang dan Pembayaran. **AMENDMENT REVISI 4** — perluasan `FinSupplierPayable` |
| `erd/purchasing-ap.md` | **Ada (baru, revisi 4)** | ERD rumpun Purchasing/AP — Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur |
| `erd/cash-and-master-data.md` | Ada | ERD rumpun Kas dan Master Data Finance |
| `erd/accounting-integration.md` | Ada | ERD rumpun Integrasi Accounting. Bagian 4 (`HELD_FOR_FINALIZATION`) **dicabut** revisi 3, dipertahankan sebagai jejak sejarah |
| `erd/data-dictionary.md` | Ada | Kamus data seluruh kolom dan bentuk DDL. `HandoffType` naik dari 4 ke **8 nilai** (revisi 3). **AMENDMENT REVISI 4** (bagian C.1-C.16) — 14 tabel baru + 1 diperbarui, DDL lengkap |
| `contracts/api-contract.md` | Ada | **`FIN-API-1.1`** `draft` (revisi 4) — tujuh grup endpoint baru Purchasing/AP + Receivable Invoice Batch, dua endpoint baru grup Receipt |
| `contracts/state-transition-matrix.md` | Ada | **`FIN-STATE-1.2`** `draft` — satu transisi dicabut (bagian 9), empat `HandoffType` ditambah (bagian 1, revisi 1.1); **tujuh entity baru** (bagian B, revisi 4) |
| `contracts/validation-matrix.md` | Ada | **`FIN-VAL-1.2`** `draft` — `FIN-VAL-076` dicabut, `FIN-VAL-078`..`086` ditambah (revisi 1.1); **`FIN-VAL-100`..`122` ditambah** (revisi 4) |
| `contracts/integration-contract.md` | Ada | **`FIN-INTEGRATION-1.2`** `draft` — katalog 17→24 kode, bagian 2a baru, bagian 5.5 diganti total (revisi 1.1); **bagian 5.8 baru, kode ke-25** (revisi 4) |
| `contracts/permission-audit-matrix.md` | Ada | **`FIN-PERM-1.1`** `draft` (revisi 4) — tujuh Resource baru, dua Action baru grup Receipt |
| `testing/acceptance-test-matrix.md` | Ada | **`FIN-TEST-1.2`** `draft` — bagian 8a baru (25 skenario uang muka/deposit/selisih kas, revisi 1.1); **bagian B.1-B.7 baru** (revisi 4) |
| `evidence/01-jawaban-untuk-owner-accounting.md` | Ada | Jawaban Finance atas enam pertanyaan Rizki (paket 15 September 2026), berdiri sendiri |
| `evidence/02-permintaan-kontrak-untuk-owner-billing.md` | Ada | Permintaan `BilCollectionHandoff` dan perluasan `BilArHandoff`, berdiri sendiri. Bagian 3 juga ditujukan ke owner HR |
| `evidence/04-jawaban-atas-balasan-accounting.md` | Ada | Jawaban Finance atas `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md`, 25 September 2026 — persetujuan `FIN-DEC-030`, empat usulan kode baru (dikoreksi `evidence/05`), berdiri sendiri |
| `evidence/05-koreksi-jawaban-atas-balasan-accounting.md` | Ada | Koreksi atas `evidence/04` — kode `PEMAKAIAN-UANG-MUKA-DEPOSIT` dipersempit, dua kode baru (`PENGEMBALIAN-UANG-MUKA`, `PENGAKUAN-KELEBIHAN-BAYAR`) dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` ditambah, total tujuh kode menunggu ratifikasi Accounting (`FIN-OQ-017`), berdiri sendiri |
| `evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md` | Ada | Usulan kode kejadian baru `PPN-MASUKAN-PEMBELIAN` (kode ke-25) untuk rumpun Purchasing/AP, dipicu evidence `Keuangan.md` (`FIN-DEC-045`, `046`, `053`), menutup sisi Finance `FIN-OQ-020`, berdiri sendiri, 25 September 2026 |
| `evidence/07-usulan-empat-kode-potongan-retur-deposit-untuk-accounting.md` | Ada | Usulan kode ke-26 s.d. 29 (`POTONGAN-PIUTANG-NON-TUNAI`, pembaliknya, `RETUR-PEMBELIAN`, `PEMAKAIAN-DEPOSIT-RETUR`), menutup sisi Finance `FIN-OQ-026`, 26 September 2026. Juga memberi tahu Accounting soal nama kode alias yang sudah ditulis source |
| `roadmap/00-delivery-roadmap.md` | Ada | `FIN-ROADMAP-001` revisi 7, status `ACTIVE` — payung: gelombang, traceability, coverage gap, risiko. Revisi 7 menurunkan AMENDMENT REVISI 5 |
| `roadmap/01-backend-roadmap.md` | Ada | `FIN-ROADMAP-BE-001` revisi 5 — 41 task `BE-FIN-*` (`027`..`041` untuk revisi 4-5, **nol ⛔**), urutan eksekusi, DoD backend |
| `roadmap/02-frontend-roadmap.md` | Ada | `FIN-ROADMAP-FE-001` revisi 5 — 13 task `FE-FIN-*` (`008`..`013`; `010` dibuka revisi 5, `013` menunggu `FE-FIN-004`), kebutuhan UI brief, `DEV_DISCRETION`, DoD frontend |

Sub-pohon `task/report/` belum ada dan memang bukan keluaran pass perencanaan; ia menyusul dari
kedua skill build.

## Pemicu impact scan

Blueprint ini menjadi **stale** dan MUST diperiksa ulang bila salah satu terjadi:

| Pemicu | Yang harus diperiksa ulang |
|---|---|
| Backend SHA bergerak dari **`d6cdfaf9`** (bukan lagi `09101d05`) | Seluruh klaim as-is pada `01-existing-capability-map.md`, khususnya `FIN-CAP-007`, `022`..`025` yang baru diverifikasi 25 September 2026 |
| Rumpun AR/AP/Payable Finance dipakai sebagai acuan desain | **Audit field-per-field belum dilakukan** untuk 59 berkas yang dibangun `BE-FIN-001`..`021`. Jalankan `trace-existing-capabilities` lebih dulu — lihat capability map bagian 9.3 "Yang TIDAK ditutup" |
| Accounting meratifikasi atau mengoreksi nama tujuh kode baru | `contracts/integration-contract.md` bagian 5.4, `contracts/validation-matrix.md` `FIN-VAL-075`, `testing/acceptance-test-matrix.md` bagian 8a, dan `EPIC FIN-14` pada `04-prd-to-mvp.md` |
| Accounting meratifikasi atau mengoreksi kode `PPN-MASUKAN-PEMBELIAN` (kode ke-25) | `contracts/integration-contract.md` bagian 5.8, `contracts/validation-matrix.md` `FIN-VAL-122`; membuka `EPIC FIN-15` masuk `/plan-module-delivery` (`FIN-OQ-020`, `FIN-DEC-046`) |
| Frontend SHA bergerak dari `abed49b03` | `03-frontend-architecture.md` bagian rute dan menu yang sudah ada |
| `billing-kasir` menaikkan revisi yang menyentuh `BilArHandoff`, `BilTender`, `BilSettlement`, atau `BilPaymentAllocation` | `contracts/integration-contract.md` bagian intake Billing |
| `accounting` menaikkan `ACC-XMOD` di atas `0.2` | `contracts/integration-contract.md` bagian Finance → Accounting, dan `FIN-DEC-001` |
| Endpoint penerima Accounting Event mulai dibangun | `FIN-CAP-018` pada capability map berubah dari `Missing`; gelombang `MVP-5` bisa dimulai |
| Salah satu `input_hashes` berubah | Seluruh artefak desain — dokumen hulu bergerak tanpa revisi manifest |
