# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
revision: 0.5
status: approved
readiness: READY_FOR_TASK_APPROVAL
owners:
  product_domain: Billing Owner
  finance: Finance/AR/AP Owner
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Frontend Owner
approved_by: Product/Domain Owner (user approval in conversation) — BKC-DEC-062 tanpa konfirmasi terpisah Payer/Insurance+Finance/AR, lihat caveat 00-interview-decisions.md
approved_at: 2026-09-02T13:53:34+07:00
created_at: 2026-08-20T11:22:42+07:00
updated_at: 2026-09-02T13:53:34+07:00
backend_commit_sha: 17b9c0e21e32b41a8dfd6dbde31462d52717646b
frontend_commit_sha: 60febdcdbb39de6cebc2d825906bce949f3b5af3
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
input_revisions:
  decisions: 0.2 (baseline) + amendment BKC-DEC-059-062 (2 Sep 2026, draft)
  capability_map: 0.2 (baseline) + impact scan section 16 (2 Sep 2026)
  requirement_gate: 0.3
  hospital_domain_architecture: 0.3
input_hashes:
  00-interview-decisions.md: af8be2d7c98faacc50ee2de45880ce9edd30a892c92936bdc04e8761f6954f9f
  01-existing-capability-map.md: e45cc98c740019f3457b454719d82be6c2468c27ec7efbcfb12aa0673e801efc
  evidence/02-requirement-completeness-gate.md: fcb8ea8a37df4b9f284234eb3b9dfea994e8d15b630e63db9416799fac948a3d
  evidence/03-hospital-domain-architecture.md: 3fdd25afeb0617174d3c7594e7fd011ba6678dbd606435a0e2e50669ade17b58
contract_versions:
  api: BIL-API-0.4 (baseline approved) + amendment draft 2 Sep 2026
  state: BIL-STATE-0.4 (baseline approved) + amendment draft 2 Sep 2026
  validation: BIL-VALIDATION-0.4 (baseline approved) + amendment draft 2 Sep 2026
  integration: BIL-INTEGRATION-0.4 (baseline approved) + amendment draft 2 Sep 2026
  permission: BIL-PERMISSION-0.4 (baseline approved) + amendment draft 2 Sep 2026
  testing: BIL-TEST-0.4 (baseline approved) + amendment draft 2 Sep 2026
active_dependency_ids: [BKC-BLK-FE-001, BKC-BLK-INT-001, BKC-BLK-PROV-001, BKC-BLK-DATA-001]
artifact_hashes:
  02-backend-architecture.md: c9a81ded420a720a193e5eb6e76387962e8b5e5f6c2788e88d6588d25d748232
  03-frontend-architecture.md: 7c6693eafb43aa6e330209a45efdb0bcd4683d37f73081179499a92d76a53a26
  04-prd-to-mvp.md: 5359c65e67b42b1d0d8dab4029b0236f994ec8890d32d599bae7fca71cfc3c31
  contracts/api-contract.md: e7b1e9ec1df003fc3135a5bf5ff0a6ecc2c38e5f921e5fbc17dbf3c6751b4b07
  contracts/state-transition-matrix.md: b761710ae8f323465c272a7b8f58fa1986f5d4f227333a63fc78bcad79acb318
  contracts/validation-matrix.md: f35436888569e63955c00331087915e94a4955adc03bec41cdece7a54d9dff64
  contracts/integration-contract.md: db7a2ce7fbef18f1459e6d08378dd064c91315c8c774d72a1e40c10129d4c422
  contracts/permission-audit-matrix.md: 1d599e5cf85f24a2db3c68b0dab8ec3ad951d0e97a02ea7585c6d24330d1012d
  erd/00-context-erd.md: 9409620bd731afed36ae5936f4775ce765280732d6bdccf8d34dbfad6585df03
  erd/01-billing-account-charge.md: 8e3573a54837f8441bddce6f2a4066c2ba108656cd60df09f4151dd9693f5f57
  erd/02-patient-funds-settlement.md: e79e97ed4d46f1a9e9bd9578b6590b8b7187d0c8f141967c7dc6202ae857bcf3
  erd/03-financial-exception-adjustment.md: 546fe293aa6313d78745836930f1cd27b31d98230c424894c2b3b5a6741bc14d
  erd/04-cashier-operations.md: 42120bb26a9722e25eec7a2bffb9869c30c2e7a2f926aa4b55dd6e69f67f07f6
  erd/05-finalization-handoff.md: bbad265521c2f8b078fe772b6fb256a0d6f6ebb1a344a536d855996620b2fe2c
  erd/data-dictionary.md: 68b88bb20a0973b805ef998a467deb5169656844bff527bffd13147c1671a4cb
  testing/acceptance-test-matrix.md: 795ea7df1e3f5ca0f4da87e78dd7232ccd6bf79162924057b10813d31fd1b8a4
  roadmap/README.md: dccc54efa81f22f77c2a977481e4ec454cd1e180c778618754da397ef51dc40d
  roadmap/backend-roadmap.md: 4d09c75af06fad0d2a86cffa8c7c1fcbe73a415b676961a020fd83d79cb5a938
  roadmap/frontend-roadmap.md: 795f2e504859cfb9cdde605d37a5a4aa22bf53ea2d43f3db3f6298d74f12328a
  roadmap/requirement-traceability.md: 005f597e24dfa5b16a61bed9d532e57ad2972f5443293973201848f284b09ca6
  MODULE-STATUS.md: 114f1db0aa12424babfa51867897111ac3581841451962850e68fffe5ced2186
```

## Artifact register

| Kelompok | Lokasi | Status |
| --- | --- | --- |
| Keputusan dan capability | [`00-interview-decisions.md`](./00-interview-decisions.md), [`01-existing-capability-map.md`](./01-existing-capability-map.md) | Baseline `0.2 approved` + amendment `BKC-DEC-059`–`062` **approved** (2 Sep 2026 13:53 WIB) |
| Backend/frontend design | [`02-backend-architecture.md`](./02-backend-architecture.md), [`03-frontend-architecture.md`](./03-frontend-architecture.md) | Baseline `0.4 approved` + amendment **approved** (2 Sep 2026 13:53 WIB) |
| PRD → MVP slice | [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | **Baru, approved** (2 Sep 2026 13:53 WIB) — PRD→MVP pertama modul ini, cakupan slice `BKC-DEC-059`–`062` saja |
| ERD/data | [`erd/`](./erd/00-context-erd.md) | Baseline `0.4 approved` + amendment **approved** pada `01-billing-account-charge.md`/`data-dictionary.md` (2 Sep 2026) |
| Kontrak dan acceptance | [`contracts/`](./contracts/api-contract.md), [`testing/`](./testing/acceptance-test-matrix.md) | Baseline `0.4 approved` + amendment **approved** (2 Sep 2026 13:53 WIB) |
| Delivery roadmap | [`roadmap/`](./roadmap/README.md) | Revision `1`, menunggu task approval — kini mencakup `BKC-PH-008`/`BE-BKC-018`–`021`/`FE-BKC-014`–`016` untuk slice `BKC-DEC-059`–`062` |
| Evidence/arsip | [`evidence/`](./evidence/02-requirement-completeness-gate.md) | Preserved |
| Status | [`MODULE-STATUS.md`](./MODULE-STATUS.md) | `READY_FOR_TASK_APPROVAL` (baseline modul) — slice baru ini belum masuk status roadmap manapun |

Revision `0.5` menambah, tidak menggantikan, baseline `0.4 approved`. Disetujui Product/Domain Owner 2 September 2026 13:53 WIB dalam satu pernyataan approval untuk `BKC-DEC-059`–`062` beserta seluruh dokumen desain yang mengoperasikannya. **Caveat tetap berlaku dan tercatat**: `BKC-DEC-062` mengamendemen sebagian `BKC-DEC-042` yang owner tercatatnya adalah Payer/Insurance + Finance/AR — approval yang diberikan adalah dari Product/Domain Owner, TANPA konfirmasi terpisah dari owner asli tsb (lihat `00-interview-decisions.md` dan `04-prd-to-mvp.md` § 20 untuk detail dan risiko provenance). Source SHA berubah atau kontrak material berubah lagi harus memicu impact scan baru sebelum revision berikutnya.
