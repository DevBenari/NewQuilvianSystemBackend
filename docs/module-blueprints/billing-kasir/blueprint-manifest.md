# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
revision: 0.6
status: draft
readiness: DESIGN_DRAFT_AWAITING_APPROVAL
baseline_revision: 0.5
baseline_status: approved
owners:
  product_domain: Billing Owner
  finance: Finance/AR/AP Owner
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Frontend Owner
approved_by: null
approved_at: null
baseline_approved_by: Product/Domain Owner (user approval in conversation) — BKC-DEC-062 tanpa konfirmasi terpisah Payer/Insurance+Finance/AR, lihat caveat 00-interview-decisions.md
baseline_approved_at: 2026-09-02T13:53:34+07:00
created_at: 2026-08-20T11:22:42+07:00
updated_at: 2026-09-03T00:00:00+07:00
backend_commit_sha: a42b651d7518060dcc5e7df46cb495ef822b57f5
frontend_commit_sha: 00210f9a5fb2f4f69e57b8c90c57c63c788da792
previous_backend_commit_sha: 17b9c0e21e32b41a8dfd6dbde31462d52717646b
previous_frontend_commit_sha: 60febdcdbb39de6cebc2d825906bce949f3b5af3
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
input_revisions:
  decisions: 0.2 (baseline) + amendment BKC-DEC-059-062 (2 Sep 2026, approved) + amendment BKC-DEC-063-069 (3 Sep 2026, approved)
  capability_map: 0.2 (baseline) + impact scan section 16 (2 Sep 2026) — BELUM ada impact scan untuk 3 Sep 2026, lihat catatan staleness di bawah
  requirement_gate: 0.3
  hospital_domain_architecture: 0.3
input_hashes:
  00-interview-decisions.md: 170b2442246a4cd0d6f8f2b9e6c5faede1f5555600ea6442725030b9647177c3
  01-existing-capability-map.md: e45cc98c740019f3457b454719d82be6c2468c27ec7efbcfb12aa0673e801efc
  evidence/02-requirement-completeness-gate.md: fcb8ea8a37df4b9f284234eb3b9dfea994e8d15b630e63db9416799fac948a3d
  evidence/03-hospital-domain-architecture.md: 3fdd25afeb0617174d3c7594e7fd011ba6678dbd606435a0e2e50669ade17b58
design_decision_ids: [BKC-DES-001, BKC-DES-002, BKC-DES-003, BKC-DES-004, BKC-DES-005, BKC-DES-006, BKC-DES-007, BKC-DES-008, BKC-DES-009]
design_decision_status: draft
contract_versions:
  api: BIL-API-0.5 (draft, 3 Sep 2026) atas baseline BIL-API-0.4 approved
  state: BIL-STATE-0.5 (draft, 3 Sep 2026) atas baseline BIL-STATE-0.4 approved
  validation: BIL-VALIDATION-0.5 (draft, 3 Sep 2026) atas baseline BIL-VALIDATION-0.4 approved
  integration: BIL-INTEGRATION-0.5 (draft, 3 Sep 2026) atas baseline BIL-INTEGRATION-0.4 approved
  permission: BIL-PERMISSION-0.5 (draft, 3 Sep 2026) atas baseline BIL-PERMISSION-0.4 approved
  testing: BIL-TEST-0.5 (draft, 3 Sep 2026) atas baseline BIL-TEST-0.4 approved
  calculation: BIL-CALCULATION-0.5 (draft, 3 Sep 2026) atas BIL-CALCULATION-0.4 yang berlaku di source
compatibility_impact: additive — satu endpoint GET baru, field aditif pada empat DTO kalkulasi, tanpa tabel/kolom/migration baru, tanpa field yang dihapus atau berubah arti
active_dependency_ids: [BKC-BLK-FE-001, BKC-BLK-INT-001, BKC-BLK-PROV-001, BKC-BLK-DATA-001, BKC-BLK-SEC-001, BKC-BLK-MASTER-001]
blocking_questions:
  - Approval BKC-DES-001..009 oleh Product/Domain Owner (khususnya BKC-DES-007, nomor dokumen)
  - Penilaian Security atas pemakaian ulang BillingInvoice:Read untuk dokumen berisi nomor polis (BKC-BLK-SEC-001)
  - Kelengkapan MstInsuranceProvider dan MstInsuranceCoverageRule untuk verifikasi UAT (BKC-BLK-MASTER-001)
artifact_hashes:
  02-backend-architecture.md: 6ab59563a787ee749e5c4f7489fed388dac269dcb13ffdf6833805f8e32b51b3
  03-frontend-architecture.md: 09c686ab73f96e750cc4d2c3ccdc760608af4081873363c9c39984347ad7a135
  04-prd-to-mvp.md: c1c1a060c6f7e233429d734197f6dd413006da589ee6b449c666c1a924c48c37
  contracts/api-contract.md: 136de236396a03f18eede09e57049f5476c2d9d612e70175888de25e44ab35fe
  contracts/state-transition-matrix.md: 7e6df5415ee29fd3dc1ecab3e39f83a47c451f9a14621c7fedb0107edc2acf0d
  contracts/validation-matrix.md: 931e24671c1107b573cc6c25cfa2ca9e5b3b9a3222dbd349423f7c173a8c92d0
  contracts/integration-contract.md: 64a59c9be4f768e8ab46ac1b44415532eb27f33305336da408f0aa7b300649f9
  contracts/permission-audit-matrix.md: 610a1c30a3bd048df5f40f7c8057536d33c87b3b6fcc08970e7cc73c5c0f43b4
  erd/00-context-erd.md: 2064be534c0396212eae10edb3251ed119655618d1a3f41e3f5616b91b9633a2
  erd/01-billing-account-charge.md: 0b2ab769655b3adf70eed48e5b4cc50ad672bc8995ec539d9920ce4ed7d43792
  erd/02-patient-funds-settlement.md: e79e97ed4d46f1a9e9bd9578b6590b8b7187d0c8f141967c7dc6202ae857bcf3
  erd/03-financial-exception-adjustment.md: 546fe293aa6313d78745836930f1cd27b31d98230c424894c2b3b5a6741bc14d
  erd/04-cashier-operations.md: 42120bb26a9722e25eec7a2bffb9869c30c2e7a2f926aa4b55dd6e69f67f07f6
  erd/05-finalization-handoff.md: bbad265521c2f8b078fe772b6fb256a0d6f6ebb1a344a536d855996620b2fe2c
  erd/data-dictionary.md: 2891abf956e56b5bd2d877cdfe1f5f18a44a3ae19a3b41a89021e629b6593002
  testing/acceptance-test-matrix.md: 008d6965be65d224592eeab90fa1305b3a935c6ef7403f41b89762884b389122
  roadmap/README.md: dccc54efa81f22f77c2a977481e4ec454cd1e180c778618754da397ef51dc40d
  roadmap/backend-roadmap.md: 4d09c75af06fad0d2a86cffa8c7c1fcbe73a415b676961a020fd83d79cb5a938
  roadmap/frontend-roadmap.md: 795f2e504859cfb9cdde605d37a5a4aa22bf53ea2d43f3db3f6298d74f12328a
  roadmap/requirement-traceability.md: 005f597e24dfa5b16a61bed9d532e57ad2972f5443293973201848f284b09ca6
  MODULE-STATUS.md: 114f1db0aa12424babfa51867897111ac3581841451962850e68fffe5ced2186
supersedes: null
```

## Artifact register

| Kelompok | Lokasi | Status |
| --- | --- | --- |
| Keputusan dan capability | [`00-interview-decisions.md`](./00-interview-decisions.md), [`01-existing-capability-map.md`](./01-existing-capability-map.md) | Baseline `0.2 approved` + amendment `BKC-DEC-059`–`062` **approved** (2 Sep 2026) + amendment `BKC-DEC-063`–`069` **approved** (3 Sep 2026) |
| Backend/frontend design | [`02-backend-architecture.md`](./02-backend-architecture.md), [`03-frontend-architecture.md`](./03-frontend-architecture.md) | Baseline `0.4 approved` + amendment 2 Sep 2026 **approved** + amendment 3 Sep 2026 **draft** (Dokumen Invoice Asuransi, `BKC-DES-001`–`009`) |
| PRD → MVP slice | [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | Slice `BKC-DEC-059`–`062` **approved** (2 Sep 2026) + amendment slice `BKC-DEC-065`–`069` **draft** (3 Sep 2026, `EPIC BKC-04`/`BKC-05`) |
| ERD/data | [`erd/`](./erd/00-context-erd.md) | Baseline `0.4 approved` + amendment 2 Sep 2026 **approved** + catatan 3 Sep 2026 **draft** pada `00-context-erd.md`/`01-billing-account-charge.md`/`data-dictionary.md` (tanpa tabel/kolom baru) |
| Kontrak dan acceptance | [`contracts/`](./contracts/api-contract.md), [`testing/`](./testing/acceptance-test-matrix.md) | Baseline `0.4 approved` + amendment 2 Sep 2026 **approved** + amendment `0.5` **draft** (3 Sep 2026) |
| Delivery roadmap | [`roadmap/`](./roadmap/README.md) | Revision `1` — slice `BKC-DEC-065`–`069` (`MVP-4`–`MVP-6`) **belum** masuk roadmap; itu keluaran `/plan-module-delivery` |
| Evidence/arsip | [`evidence/`](./evidence/02-requirement-completeness-gate.md) | Preserved |
| Status | [`MODULE-STATUS.md`](./MODULE-STATUS.md) | Belum diperbarui untuk revision `0.6` — pemeliharaannya milik `/manage-module-blueprint` |

## Catatan revision `0.6`

Revision `0.6` **menambah**, tidak menggantikan, baseline `0.5 approved`. Isinya adalah amendment 3 September 2026 "Dokumen Invoice Asuransi dan pecahan rupiah coverage per item", turunan keputusan bisnis `BKC-DEC-065`–`069` yang sudah disetujui Product/Domain Owner pada tanggal yang sama.

Field `status` bernilai `draft` **hanya untuk revision `0.6`**. Baseline `0.5` beserta seluruh amendment 2 September 2026 tetap `approved` dan tidak dicabut — lihat `baseline_revision`/`baseline_status`. Keputusan arsitektur baru (`BKC-DES-001`–`009`) belum di-approve; approval tetap tindakan manusia dan tidak dapat diberikan oleh skill desain.

**Caveat yang masih berlaku dari revision sebelumnya**: `BKC-DEC-062` mengamendemen sebagian `BKC-DEC-042` yang owner tercatatnya adalah Payer/Insurance + Finance/AR, sementara approval yang diberikan berasal dari Product/Domain Owner tanpa konfirmasi terpisah dari owner asli tersebut. Lihat `00-interview-decisions.md` dan `04-prd-to-mvp.md` § 20.

### Staleness dan impact scan

`backend_commit_sha` dan `frontend_commit_sha` pada revision `0.5` sudah usang saat revision ini disusun. SHA baru dicatat di atas, dan SHA lama dipertahankan sebagai `previous_*` agar pergerakannya terbaca.

Perubahan di antara kedua SHA itu adalah pekerjaan modul ini sendiri (`BE-BKC-018`–`021`, `BE-BKC-FIX-001`/`002`, `FE-BKC-014`–`017`, `FE-BKC-FIX-001`–`007`), yang sudah tercatat pada `roadmap/requirement-traceability.md`. Area yang terdampak amendment 3 September 2026 sudah dibaca ulang langsung dari source pada SHA baru; buktinya tercatat pada `02-backend-architecture.md` § Bukti as-is.

Pembacaan itu **bukan** pengganti impact scan resmi. `01-existing-capability-map.md` masih berhenti pada impact scan § 16 (2 September 2026) dan belum punya bagian untuk 3 September 2026 — pembaruannya milik `/trace-existing-capabilities`, bukan skill desain, sehingga `input_hashes` untuk berkas itu tidak berubah. Jalankan impact scan itu sebelum revision berikutnya, dan sebelum `/plan-module-delivery` bila jarak SHA melebar lagi.
