# Medical Fee — Blueprint Manifest

```yaml
blueprint_id: MF-BP-001
module_name: Medical Fee
module_slug: medical-fee
module_prefix: Mdf
module_area: Areas/HealthServices/MedicalFeeManagement
module_area_basis: >
  FIN-BRD-V2-0.3 bagian 4 menempatkan Medical Fee di bawah Health Services / Medical Fee sebagai
  business owner. Penempatan folder mengikuti itu, bukan Corporate — walaupun keluarannya
  bermuara ke Finance.
revision: 2
status: approved
blueprint_shape: SINGLE
shape_decided_by: DESIGN_PASS_20_SEP_2026
shape_evidence: >
  Tiga rumpun (aturan tarif sharing, perhitungan jasa, penyerahan ke Finance) berbagi satu
  decision log, satu tabel kepemilikan data, dan satu rantai periode. Memecahnya akan
  menduplikasi ketiganya. Mengikuti preseden FIN-BP-001 dan BIL-CASH-001.
created_at: 2026-09-20T00:00:00+07:00
updated_at: 2026-09-20T00:00:00+07:00
last_owner_action: >
  20 September 2026 — approval MDF-DES-001..018, penguncian tujuh kontrak turunan ke 1.0,
  dan penegasan kepemilikan modul oleh Yasmin.
owners:
  product_domain: >
    Yasmin — Product/Domain Owner modul Medical Fee. Yasmin juga owner modul Finance
    Management (FIN-BP-001). Kepemilikan ganda ini DITEGASKAN owner 20 September 2026.
    CATATAN YANG PENTING: satu owner untuk kedua modul TIDAK meruntuhkan batas antar keduanya.
    Medical Fee tetap berhenti pada jasa KOTOR (MF-DEC-005), tetap menyerahkan lewat tabel
    handoff MdfFinanceHandoff (MDF-DES-018), dan tetap TIDAK PERNAH menulis ke tabel Finance.
    Yang hilang hanyalah kebutuhan negosiasi antar orang, bukan disiplin batas modulnya.
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Product Owner — belum ada UI brief yang disetujui
  cross_module_billing: Billing Owner (MF-CQ-05, MF-CQ-08 menunggu)
  cross_module_hr: HR Owner (MF-CQ-06 menunggu)
  cross_module_clinical: Owner Clinical + Laboratory (MF-CQ-07 menunggu)
  cross_module_finance: >
    Yasmin — owner yang sama. FIN-BP-001 revisi 2 (FIN-DES-025..028) sudah menyesuaikan diri
    terhadap modul ini dan approved 20 September 2026. Nol permintaan terbuka ke Finance.
approved_by: Yasmin (Product/Domain Owner, bertindak sebagai owner Medical Fee)
approved_at: 2026-09-20
approval_note: >
  DUA lapisan kini approved, dan keduanya diberikan TERPISAH sebagaimana seharusnya:
    (1) 17 keputusan BISNIS pada 00-interview-decisions.md (MF-DEC-001..010, MF-DEC-012..018)
        — approved 20 September 2026;
    (2) 18 keputusan ARSITEKTUR MDF-DES-001..018 pada 02-backend-architecture.md
        — approved 20 September 2026 lewat pernyataan langsung "Setujui MDF-DES-001..018".
  Dicatat apa adanya; approval adalah tindakan manusia dan TIDAK PERNAH ditetapkan skill.
  Approval ini BUKAN otorisasi untuk: membuat atau menjalankan migration; mendaftarkan sendiri
  modul MedicalFeeManagement ke registry kepemilikan; maupun memulai task yang masih menunggu
  owner Billing (MF-CQ-05, MF-CQ-08) atau owner Clinical/Laboratorium (MF-CQ-07).

backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: 66f36b432
frontend_sha_note: >
  Frontend bergerak dari abed49b03 ke 66f36b432 sebelum pass desain ini dimulai. Diperiksa dan
  TIDAK berdampak: commit itu menambah unit test UI consistency, nol perubahan pada src/app,
  menu sidebar, Redux slice, maupun hooks yang relevan. Capability map modul ini juga nol klaim
  frontend.

input_revisions:
  decisions: revisi 2 — MF-DEC-001..018, 17 approved dan 1 superseded, 20 September 2026
  capability_map: revisi 1 — 20 September 2026, 14 kemampuan berbukti
  requirement_gate: NOT_RUN — lihat requirement_completeness_gate_note
  hospital_domain_architecture: NOT_RUN — lihat domain_architecture_readiness
input_hashes:
  00-interview-decisions.md: 7d95443ad12eb56cf4a7b449604880ad62baff2be1801b56e7e00c56b9b358b5
  01-existing-capability-map.md: 30d6fce01475444c2cbae360e335d000cf0d7ebe6892c43a4cca58a80f39cee0

contract_versions:
  api-contract: MDF-API-1.0 — locked 2026-09-20
  integration-contract: MDF-INTEGRATION-1.0 — locked 2026-09-20 (lihat contract_lock_note)
  state-transition-matrix: MDF-STATE-1.0 — locked 2026-09-20
  validation-matrix: MDF-VAL-1.0 — locked 2026-09-20
  permission-audit-matrix: MDF-PERM-1.0 — locked 2026-09-20
  prd-to-mvp: MDF-MVP-1.0 — locked 2026-09-20
  acceptance-test-matrix: MDF-TEST-1.0 — locked 2026-09-20
contract_lock_note: >
  Owner mengunci ketujuh kontrak turunan pada 20 September 2026. Dicatat apa adanya, TIDAK
  ditetapkan skill.
  DUA PERMUKAAN TETAP TIDAK TERKUNCI, dan alasannya bukan status MDF-DES melainkan
  ketergantungan pada owner Billing:
    (1) arah alir nilai jasa ke BilInvoiceItem.DoctorShare — MF-CQ-08, bentuk A/B/C belum
        dipilih (integration-contract.md bagian 4);
    (2) rujukan pelaksana pada entri ADHOC dan ADHOC_CATALOG — MF-CQ-05
        (integration-contract.md bagian 5).
  Keduanya kelak MENAMBAH permukaan baru yang dikunci tersendiri; kedatangannya tidak
  menaikkan versi ketujuh kontrak di atas.
  Pembagian tim di luar kamar operasi (MF-CQ-07) TIDAK termasuk pengecualian ini: bentuk
  rincian jasa sudah terkunci, dan yang ditunggu hanyalah modul sumber mulai mencatat timnya.
  Konsekuensi langsung: kerja paralel backend-frontend Medical Fee kini DIIZINKAN.
external_contract_dependencies:
  FIN-BP-001 revisi 2: >
    Finance sudah menyesuaikan diri terhadap modul ini lewat FIN-DES-025..028:
    FinMedicalServicePayable menggantikan FinDoctorPayable, dan FinPaymentDeduction menampung
    potongan yang MF-DEC-005 serahkan ke Finance. Kontrak keluar modul ini menyerahkan jasa
    KOTOR.
  BIL-INTEGRATION-0.4: >
    Kontrak charge Billing. Modul ini menuntut dua perubahan padanya (MF-CQ-05 field pelaksana
    untuk entri kasir, MF-CQ-08 arah alir DoctorShare) — keduanya MENUNGGU owner Billing.
  BIL-CASH-001: >
    Blueprint billing-kasir. Sumber BilInvoiceItem, BilDiscountApplication, dan kontrak charge.

design_decision_ids: [MDF-DES-001, MDF-DES-002, MDF-DES-003, MDF-DES-004, MDF-DES-005, MDF-DES-006, MDF-DES-007, MDF-DES-008, MDF-DES-009, MDF-DES-010, MDF-DES-011, MDF-DES-012, MDF-DES-013, MDF-DES-014, MDF-DES-015, MDF-DES-016, MDF-DES-017, MDF-DES-018]
design_decision_status: >
  SELURUH MDF-DES-001..018 `approved` 20 September 2026 oleh Yasmin. Approval diberikan pada
  lapisan arsitektur secara tersendiri, bukan diturunkan dari approval keputusan bisnis di
  hulunya.

requirement_completeness_gate_note: >
  NOT_RUN, deviasi tercatat atas keputusan owner 20 September 2026. Sebagai gantinya dipakai:
  (1) 00-interview-decisions.md dengan 17 keputusan approved beserta owner, evidence, dan
  tanggal, ditambah 21 fakta bernomor; (2) 01-existing-capability-map.md dengan 14 kemampuan
  berbukti source langsung; (3) Referensi_Meeting_FIN-OQ_dan_Medical_Fee.pdf, rangkuman
  transcript meeting RS MMC yang memuat praktik berjalan beserta keluhannya.
  Pola pencatatan mengikuti preseden billing-kasir dan FIN-BP-001.

domain_architecture_readiness: >
  DOMAIN_ARCHITECTURE_NOT_RUN, deviasi tercatat atas keputusan owner 20 September 2026.
  Alasan yang dapat diuji:
    (1) Modul ini TIDAK mengambil keputusan klinis apa pun dan tidak berdampak pada keselamatan
        pasien — ia menghitung penghasilan atas layanan yang SUDAH selesai diberikan.
    (2) Seluruh batas lintas bounded context sudah diputus eksplisit oleh 17 keputusan bisnis:
        MF-DEC-001/003/017 (Billing), MF-DEC-005/008/010 (Finance), MF-DEC-012 (HR),
        MF-DEC-015 (Radiologi), MF-DEC-016 (daftar peran lintas sumber).
    (3) Tidak ada data klinis yang disimpan modul ini — hanya rujukan layanan dan identitas
        tenaga medis.
  RISIKO YANG DITERIMA, DAN INI LEBIH BESAR DARIPADA FIN-BP-001: berbeda dari Finance yang
  murni satu arah, modul ini MENULIS BALIK ke Billing (MF-DEC-001 memindahkan sumber kebenaran
  BilInvoiceItem.DoctorShare) dan menuntut perubahan di empat modul lain. Empat konfirmasi
  tetangga (MF-CQ-05..08) belum satu pun turun.
  Mitigasi: setiap rumpun yang bergantung pada konfirmasi itu ditandai OPEN DECISION pada
  04-prd-to-mvp.md dan MUST NOT masuk gelombang pengiriman mana pun. Rumpun yang berdiri
  sendiri — aturan tarif, perhitungan jasa operasi, verifikasi, persetujuan, periode,
  penyerahan ke Finance — tidak tertahan.

readiness: >
  DESIGN_APPROVED dan CONTRACTS_LOCKED. Seluruh 16 berkas kanonik ada dan MDF-DES-001..018 sudah disetujui owner
  20 September 2026. Roadmap MDF-ROADMAP-001 sudah diturunkan.
  CONTRACTS_LOCKED: ketujuh kontrak turunan dikunci ke 1.0 pada 20 September 2026, sehingga
  kerja paralel backend-frontend kini diizinkan. Yang masih menahan task FE-MDF-* hanyalah
  ketiadaan UI brief, dan itu keputusan Product Owner - bukan keterikatan kontrak.
  PRASYARAT IMPLEMENTASI yang MUST diminta terpisah:
    (a) Pendaftaran modul MedicalFeeManagement beserta prefix `Mdf` pada
        docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md sebelum file model pertama ditulis
        (QBE-MOD-002, QBE-MOD-003, QBE-NAM-004). Prefix `Mdf` diverifikasi belum dipakai.
    (b) Otorisasi terpisah untuk membuat dan menjalankan migration.
    (c) Konfirmasi owner Billing atas MF-CQ-05 dan MF-CQ-08.
    (d) Sepengetahuan owner HR atas MF-CQ-06.
    (e) Konfirmasi owner Clinical dan Laboratory atas MF-CQ-07.

blocking_questions:
  - id: MF-CQ-08
    blocking_for: >
      Arah alir nilai jasa kembali ke Billing. TERBESAR di antara keempatnya. Perhitungan
      internal modul ini tidak tertahan.
  - id: MF-CQ-05
    blocking_for: Rumpun jasa dari entri bebas kasir (EPIC MDF-07)
  - id: MF-CQ-07
    blocking_for: Rumpun pembagian tim di luar operasi (EPIC MDF-05 sebagian)
  - id: MF-CQ-06
    blocking_for: Perlu sepengetahuan HR, bukan persetujuan bentuk. Tidak menahan desain
```

## Daftar artefak blueprint

| Berkas | Status | Keterangan |
|---|---|---|
| `blueprint-manifest.md` | Ada | Berkas ini |
| `00-interview-decisions.md` | Ada | 18 keputusan, 17 berlaku |
| `01-existing-capability-map.md` | Ada | 14 kemampuan berbukti |
| `02-backend-architecture.md` | Ada | `MDF-DES-001`..`018` |
| `03-frontend-architecture.md` | Ada | Kontrak fungsional dan matriks kewenangan UI |
| `04-prd-to-mvp.md` | Ada | Batas rilis pertama, epic, UAT, Definition of Done |
| `erd/00-context-erd.md` | Ada | Peta antar bounded context |
| `erd/sharing-rule.md` | Ada | ERD rumpun aturan tarif sharing |
| `erd/fee-calculation.md` | Ada | ERD rumpun perhitungan dan hasil jasa |
| `erd/data-dictionary.md` | Ada | Kamus data seluruh kolom dan bentuk DDL |
| `contracts/api-contract.md` | Ada | `MDF-API-1.0` — locked |
| `contracts/state-transition-matrix.md` | Ada | `MDF-STATE-1.0` — locked |
| `contracts/validation-matrix.md` | Ada | `MDF-VAL-1.0` — locked |
| `contracts/integration-contract.md` | Ada | `MDF-INTEGRATION-1.0` — locked, dua permukaan dikecualikan |
| `contracts/permission-audit-matrix.md` | Ada | `MDF-PERM-1.0` — locked |
| `testing/acceptance-test-matrix.md` | Ada | `MDF-TEST-1.0` — locked |
| `evidence/01-permintaan-untuk-owner-billing.md` | Ada | `MF-CQ-08` (sumber `DoctorShare`) dan `MF-CQ-05` (pelaksana entri kasir), berdiri sendiri |
| `evidence/02-pemberitahuan-untuk-owner-hr.md` | Ada | `MF-CQ-06` — pemberitahuan dan tiga pertanyaan, berdiri sendiri |
| `roadmap/00-delivery-roadmap.md` | Ada | `MDF-ROADMAP-001` revisi 4, status `ACTIVE` — payung: gelombang, traceability, coverage gap |
| `roadmap/01-backend-roadmap.md` | Ada | `MDF-ROADMAP-BE-001` — 17 task `BE-MDF-*`; `MVP-0`..`MVP-5` siap, hanya `BE-MDF-015`/`016` tertahan owner Billing |
| `roadmap/02-frontend-roadmap.md` | Ada | `MDF-ROADMAP-FE-001` — 6 task `FE-MDF-*`, kemampuan `F-01`..`F-17`, DoD frontend |
| `evidence/03-permintaan-untuk-owner-clinical-dan-laboratorium.md` | Ada | `MF-CQ-07` — pencatatan tim dan peran; bagian 5 tembusan untuk owner Radiologi |

## Pemicu impact scan

| Pemicu | Yang diperiksa ulang |
|---|---|
| Backend SHA bergerak dari `09101d05` | `MF-CAP-002`..`008` pada capability map |
| Billing mengubah perlakuan `DoctorShare` atau diskon dokter | `MF-DEC-013`, `contracts/integration-contract.md` bagian Billing |
| Modul radiologi menambahkan pencatatan pelaksana | `MF-CAP-008`; `MF-DEC-015` dapat ditinjau ulang |
| HR mengubah `WfpContractHistory` | `MF-DEC-012`, rumpun aturan tarif sharing |
| `FIN-BP-001` menaikkan revisi yang menyentuh `FinMedicalServicePayable` | `contracts/integration-contract.md` bagian Finance |
| Salah satu `input_hashes` berubah | Seluruh artefak desain |
