# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
revision: 0.4
status: approved
readiness: READY_FOR_TASK_APPROVAL
owners:
  product_domain: Billing Owner
  finance: Finance/AR/AP Owner
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Frontend Owner
approved_by: Product/Domain Owner (user approval in conversation)
approved_at: 2026-08-20T13:41:34+07:00
created_at: 2026-08-20T11:22:42+07:00
updated_at: 2026-08-20T13:41:34+07:00
backend_commit_sha: c99f0a51577456c91831870892870f9ae633b4c2
frontend_commit_sha: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
input_revisions:
  decisions: 0.2
  capability_map: 0.2
  requirement_gate: 0.3
  hospital_domain_architecture: 0.3
input_hashes:
  00-interview-decisions.md: 57f05bdc074e96054f6c2aa70c3f5f45eb5ce81859b6457ddb76b39b342f2d69
  01-existing-capability-map.md: 395db084d2f39d16b8751911a69d06b3d7dc4c39146aab8cd28afbff58db0459
  evidence/02-requirement-completeness-gate.md: fcb8ea8a37df4b9f284234eb3b9dfea994e8d15b630e63db9416799fac948a3d
  evidence/03-hospital-domain-architecture.md: 3fdd25afeb0617174d3c7594e7fd011ba6678dbd606435a0e2e50669ade17b58
contract_versions:
  api: BIL-API-0.4
  state: BIL-STATE-0.4
  validation: BIL-VALIDATION-0.4
  integration: BIL-INTEGRATION-0.4
  permission: BIL-PERMISSION-0.4
  testing: BIL-TEST-0.4
active_dependency_ids: [BKC-BLK-FE-001, BKC-BLK-INT-001, BKC-BLK-PROV-001, BKC-BLK-DATA-001]
artifact_hashes:
  02-backend-architecture.md: a60f2151be03fcbeaef2254a97cc1ac172ea01ef8299138c96a06c1099c1b23c
  03-frontend-architecture.md: a53c94c3fc61b4ae477b3734b5ec67f3d7e123f9ba233c2fca987e9f3993260f
  contracts/api-contract.md: e93c337e6930b2e61e265f268e821bc5e1f6cbf33cad6d17a8486cbda4829a5e
  contracts/state-transition-matrix.md: 299031871a86594be0145adf01d72c4db8158dfa60809b17b227a1fd6d64594b
  contracts/validation-matrix.md: a7f4b4a57c447f2bcbdab7e1dbe12871db99e56f6e2622209b1ef1ce7e840567
  contracts/integration-contract.md: 657fdefc7eea70749e21dddf2085c0efafbc96201ab872ac62c6dbcc7b1103bc
  contracts/permission-audit-matrix.md: d06ec27506325ff4ceb1798b95278168d47a411320a7db9fa394f9b0d3dcf55a
  erd/00-context-erd.md: 681a0ab179adc7b5befad4244ca9da81e6eaa527741af03773ae452f366dad17
  erd/01-billing-account-charge.md: 04dfc115bf75fdb2794d0a12e5ac4b6bfccb47bddcc6522ad5b99be75f36bca1
  erd/02-patient-funds-settlement.md: e79e97ed4d46f1a9e9bd9578b6590b8b7187d0c8f141967c7dc6202ae857bcf3
  erd/03-financial-exception-adjustment.md: 546fe293aa6313d78745836930f1cd27b31d98230c424894c2b3b5a6741bc14d
  erd/04-cashier-operations.md: 42120bb26a9722e25eec7a2bffb9869c30c2e7a2f926aa4b55dd6e69f67f07f6
  erd/05-finalization-handoff.md: bbad265521c2f8b078fe772b6fb256a0d6f6ebb1a344a536d855996620b2fe2c
  erd/data-dictionary.md: a141aedbe2ae4560eb702c50a3b4af04898bf89d2dbbfb64e9ad8d15bd2467f6
  testing/acceptance-test-matrix.md: 217e7532a8ef3ce74dc303df95ee181624fde5be6545b9471a7cdef107ca921a
  roadmap/README.md: a81fc4c386b35e7fffde7b3d6c4b00b0ae57dd294bdd5807ed8468cdb5145f64
  roadmap/backend-roadmap.md: 794509a29c3bbecc28414feaa94ceb74dd807408d15e91d7a473c6b1aa508415
  roadmap/frontend-roadmap.md: 0f4ac2e4ea3ffec44c5e9ef1d14e76df09cea67d4c1c281d9a34b58aaed65602
  roadmap/requirement-traceability.md: 5d1bbfcb7a941d36130914e1d717ffe6a340dacc792f297b365ffe9dc5be5203
  MODULE-STATUS.md: dd91707144273ccee9998e726894afa40f7925755030caeaf4d3caa42cdc7ebd
```

## Artifact register

| Kelompok | Lokasi | Status |
| --- | --- | --- |
| Keputusan dan capability | [`00-interview-decisions.md`](./00-interview-decisions.md), [`01-existing-capability-map.md`](./01-existing-capability-map.md) | Approved/audited input |
| Backend/frontend design | [`02-backend-architecture.md`](./02-backend-architecture.md), [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.4 approved` |
| ERD/data | [`erd/`](./erd/00-context-erd.md) | `0.4 approved` |
| Kontrak dan acceptance | [`contracts/`](./contracts/api-contract.md), [`testing/`](./testing/acceptance-test-matrix.md) | `0.4 approved` |
| Delivery roadmap | [`roadmap/`](./roadmap/README.md) | Revision `1`, menunggu task approval |
| Evidence/arsip | [`evidence/`](./evidence/02-requirement-completeness-gate.md) | Preserved |
| Status | [`MODULE-STATUS.md`](./MODULE-STATUS.md) | `READY_FOR_TASK_APPROVAL` |

Approval blueprint mengunci keputusan dan contract `0.4`, tetapi tidak menyetujui semua task sekaligus. Source SHA berubah atau kontrak material berubah harus memicu impact scan dan revision baru.
