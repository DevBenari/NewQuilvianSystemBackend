# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
revision: 0.8
status: draft
readiness: DESIGN_DRAFT_AWAITING_APPROVAL
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
updated_at: 2026-09-04T00:00:00+07:00
last_revision_note: >
  Revisi 0.8 (4 September 2026) adalah revisi susulan berukuran kecil di atas 0.7, bukan pass
  desain baru. Isinya HANYA implementasi BKC-DEC-080 — residual non-billable
  (IsAllowExcessPaymentByPatient=false pada rule yang match dengan residual) dirutekan ke
  mekanisme Pengecualian Finansial/write-off yang sudah ada (BKC-DEC-036), bukan menjadi
  unresolved atau tanggungan pasien. Seluruh isi 0.7 yang lain TIDAK disentuh.
backend_commit_sha: ffeb45a83a6282982214668acc57e15ac0652f04
frontend_commit_sha: 00210f9a5fb2f4f69e57b8c90c57c63c788da792
previous_backend_commit_sha: a42b651d7518060dcc5e7df46cb495ef822b57f5
previous_frontend_commit_sha: 60febdcdbb39de6cebc2d825906bce949f3b5af3
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
  decisions: 0.2 (baseline) + amendment BKC-DEC-059-062 (2 Sep 2026, approved) + amendment BKC-DEC-063-069 (3 Sep 2026, approved) + amendment BKC-DEC-070-079 (4 Sep 2026, approved) + amendment BKC-DEC-080-084 (4 Sep 2026, approved) + amendment BKC-DEC-085-087 (4 Sep 2026, approved)
  capability_map: 0.2 (baseline) + impact scan section 16 (2 Sep 2026) — BELUM ada impact scan untuk 3 dan 4 September 2026, lihat catatan staleness di bawah
  requirement_gate: 0.3
  hospital_domain_architecture: 0.3
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
input_hashes:
  00-interview-decisions.md: be61b65d48afb7052b3ae12463a71829b827f6b5f6a1183708475d4bce2837e9
  01-existing-capability-map.md: e45cc98c740019f3457b454719d82be6c2468c27ec7efbcfb12aa0673e801efc
  evidence/02-requirement-completeness-gate.md: fcb8ea8a37df4b9f284234eb3b9dfea994e8d15b630e63db9416799fac948a3d
  evidence/03-hospital-domain-architecture.md: 3fdd25afeb0617174d3c7594e7fd011ba6678dbd606435a0e2e50669ade17b58
design_decision_ids: [BKC-DES-001, BKC-DES-002, BKC-DES-003, BKC-DES-004, BKC-DES-005, BKC-DES-006, BKC-DES-007, BKC-DES-008, BKC-DES-009, BKC-DES-010, BKC-DES-011, BKC-DES-012, BKC-DES-013, BKC-DES-014, BKC-DES-015, BKC-DES-016, BKC-DES-017, BKC-DES-018, BKC-DES-019, BKC-DES-020, BKC-DES-021, BKC-DES-022, BKC-DES-023, BKC-DES-024, BKC-DES-025]
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
superseded_design_decisions:
  BKC-DES-002: digantikan BKC-DES-015 — ComponentKey bertipe teks tidak jadi dipakai; bentuk (ComponentId, ComponentType) yang sudah terimplementasi BE-BKC-FIX-003 yang diadopsi
narrowed_design_decisions:
  BKC-DES-013: dipersempit (BUKAN digugurkan) oleh BKC-DES-021 — makna UnresolvedAmount menyisakan jalur rule NotCovered + IsAllowExcessPaymentByPatient=false saja; residual perhitungan pindah ke NonBillableResidualAmount
contract_versions:
  api: BIL-API-0.7 (draft, 4 Sep 2026, revisi 0.8) atas BIL-API-0.6 draft dan baseline BIL-API-0.4 approved
  state: BIL-STATE-0.7 (draft, 4 Sep 2026, revisi 0.8) atas BIL-STATE-0.6 draft dan baseline BIL-STATE-0.4 approved
  validation: BIL-VALIDATION-0.7 (draft, 4 Sep 2026, revisi 0.8) atas BIL-VALIDATION-0.6 draft dan baseline BIL-VALIDATION-0.4 approved
  integration: BIL-INTEGRATION-0.6 (draft, 4 Sep 2026) atas baseline BIL-INTEGRATION-0.4 approved — TIDAK bergerak pada revisi 0.8; isinya tidak berubah
  permission: BIL-PERMISSION-0.6 (draft, 4 Sep 2026) atas baseline BIL-PERMISSION-0.4 approved — TIDAK bergerak pada revisi 0.8; tidak ada resource/action permission baru
  testing: BIL-TEST-0.7 (draft, 4 Sep 2026, revisi 0.8) atas BIL-TEST-0.6 draft dan baseline BIL-TEST-0.4 approved
  calculation: BIL-CALCULATION-0.7 (draft, 4 Sep 2026, revisi 0.8) atas BIL-CALCULATION-0.6 draft dan BIL-CALCULATION-0.4 yang berlaku di source
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
non_blocking_questions_revision_0_8:
  - BKC-OQ-093 — DITUTUP PENUH 4 Sep 2026 oleh BKC-DEC-089. NotCovered + IsAllowExcessPaymentByPatient=false SEKARANG IKUT dirutekan ke write-off, sama seperti residual — memperluas cakupan literal BKC-DEC-080. Perlu diselaraskan ke 02-backend-architecture.md § BKC-DES-021–025 (belum tertulis di revisi 0.8, gap kecil tersisa untuk revisi berikutnya).
  - BKC-OQ-094 — DITUTUP PENUH 4 Sep 2026. (a) BKC-DEC-090: finalisasi TIDAK diblokir, warning saja. (b) BKC-DEC-091: NON_BILLABLE_RESIDUAL sepenuhnya di luar alur AR/AP, tidak jadi syarat readiness AP Dokter/klaim asuransi.
blocked_by_external_action_not_business_decision:
  - BKC-OQ-085 (asli) — analisis dampak finansial rawat inap, perlu Finance menghitung dari data invoice riil.
  - Review Security atas BillingInvoice:Read untuk dokumen berisi nomor polis.
  - Kelengkapan MstInsuranceProvider/MstInsuranceCoverageRule untuk verifikasi UAT — perlu pengecekan data langsung.
artifact_hashes:
  02-backend-architecture.md: fd40191dfd1f450e4464dc6431eafdf58cb5d3212290271f18d11842a3f27a98
  03-frontend-architecture.md: bf591ff632a4db2a8aa1ef1293f49b8449ceaa65b7b15de77f6dea1e98996ccb
  04-prd-to-mvp.md: 94d618c628363f6d34f7790d5c84aa4c2788b34cba2106f578c7980dc1ef45c1
  contracts/api-contract.md: a64d1e737128e578324dad7ed9a755bd902c80f9adb0493ffcbd6f976529e0ff
  contracts/state-transition-matrix.md: 7d0a255829ba3b3fc3f0b46c050634f70dab0ed4406eafdb49b4fa06916e7515
  contracts/validation-matrix.md: 4e21d45dac1ffc1e8c183630b09af511a718ded682bee03d9187d8677c42abcc
  contracts/integration-contract.md: 6ffa539d2fb305b8460d48ca16281f17a283d85dbe7ac36dc62b79ba059f893c
  contracts/permission-audit-matrix.md: 59f04201481e35e9403acd53c57f961fb48b9a3c04d9a1b75e7bf48a8de99948
  data/data-dictionary.md: 318275ff9dad219146c9c4d59ec103f4b8c958260b5bc6fd572749bd4a475321
  flowcharts/00-alur-utama.md: 965390037621863cabadf7e5c75b79053885fa8d214ecf5668f86eede8f5db49
  flowcharts/pembagian-tanggungan-penjamin.md: c58772681fda3a5b3f9437751c9921d21677437b5fbe13c0e7ce6effd36ddd87
  flowcharts/ppn-obat-alkes.md: 8577e3c7f998cb6d73c4e4d65de3df932a9c8c8ff674fdf07587b30b5db2bc81
  flowcharts/dokumen-invoice-asuransi.md: 47ba3992166c5700d6b981371fc44cc880c9d91dd2d7504e3ad880aeb48745ca
  erd/data-dictionary.md: e65acb8276f9d3a603d6a27f7da54e5fdca95297554ff25a04d04d0b6d2aed7a
  testing/acceptance-test-matrix.md: 5202b500b97dfc2c8bf1c74fa8b66d1c296061b7289462ec5be683722f3a187b
artifact_hashes_note: >
  Dihitung ulang pada revisi 0.8 (SHA256, isi berkas apa adanya). Delapan berkas berubah;
  03-frontend-architecture.md, contracts/integration-contract.md,
  contracts/permission-audit-matrix.md, erd/data-dictionary.md, dan tiga flowchart selain
  pembagian-tanggungan-penjamin.md TIDAK disentuh dan hash-nya identik dengan revisi 0.7.
supersedes: null
```

## Artifact register

| Kelompok | Lokasi | Status |
| --- | --- | --- |
| Keputusan dan capability | [`00-interview-decisions.md`](./00-interview-decisions.md), [`01-existing-capability-map.md`](./01-existing-capability-map.md) | Baseline `0.2 approved` + amendment `BKC-DEC-059`–`062` **approved** (2 Sep) + `BKC-DEC-063`–`069` **approved** (3 Sep) + `BKC-DEC-070`–`079` **approved** (4 Sep) |
| Backend/frontend design | [`02-backend-architecture.md`](./02-backend-architecture.md), [`03-frontend-architecture.md`](./03-frontend-architecture.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + amendment 3 Sep **approved** (`BKC-DES-001`–`009`) + amendment 4 Sep **approved** (`BKC-DES-010`–`020`) + **amendment lanjutan 4 Sep `draft`** (`BKC-DES-021`–`025`, revisi `0.8`). `03-frontend-architecture.md` **tidak disentuh** revisi `0.8` |
| PRD → MVP slice | [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | Slice `BKC-DEC-059`–`062` **approved** + slice `BKC-DEC-065`–`069` **draft** (`EPIC BKC-04`/`BKC-05`) + slice `BKC-DEC-070`–`079` **draft** (`EPIC BKC-06`/`BKC-07`/`BKC-08`) + **Bagian C `draft`** (`EPIC BKC-09`, `BKC-DEC-080`) |
| Flowchart alur proses | [`flowcharts/`](./flowcharts/00-alur-utama.md) | **Baru pada revision `0.7`** — empat berkas, seluruhnya **draft**. [`pembagian-tanggungan-penjamin.md`](./flowcharts/pembagian-tanggungan-penjamin.md) **direvisi** pada `0.8` |
| Kamus data | [`data/data-dictionary.md`](./data/data-dictionary.md) | **Baru pada revision `0.7`** — memuat delta 4 Sep dan indeks ke kamus baseline; **bertambah delta skema `0.8`** (dua kolom, satu index, satu migration). Kamus baseline tetap di [`erd/data-dictionary.md`](./erd/data-dictionary.md) |
| ERD/data baseline | [`erd/`](./erd/00-context-erd.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + catatan 3 Sep **draft** + rujukan silang 4 Sep **draft**. **Tidak disentuh revisi `0.8`**; satu ketidaksesuaian pada [`erd/03-financial-exception-adjustment.md`](./erd/03-financial-exception-adjustment.md) (kolom `AdjustmentType` yang tidak ada di source) dilaporkan pada `data/data-dictionary.md`, perapiannya revisi tersendiri |
| Kontrak dan acceptance | [`contracts/`](./contracts/api-contract.md), [`testing/`](./testing/acceptance-test-matrix.md) | Baseline `0.4 approved` + amendment 2 Sep **approved** + amendment `0.5` **draft** (3 Sep) + amendment `0.6` **draft** (4 Sep) + **amendment `0.7` `draft`** (revisi `0.8`) pada `api`, `state`, `validation`, `testing`. `integration-contract.md` dan `permission-audit-matrix.md` **tidak bergerak** |
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
