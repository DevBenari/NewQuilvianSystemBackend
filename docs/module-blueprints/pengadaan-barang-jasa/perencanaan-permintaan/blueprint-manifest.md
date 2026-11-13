# Perencanaan & Permintaan — Manifest Sub-modul

```yaml
submodule_slug: perencanaan-permintaan
parent_blueprint_id: PBJ-BP-001
parent_revision: 2
id_prefix: PBJ-PP
entity_prefix: Prc                 # PBJ-DEC-035, ACTIVE 2026-11-12
status: approved                   # PBJ-DEC-034
owners:
  product_domain: Pengguna sesi (Product Owner Pengadaan, PBJ-DEC-017)
  workflow_engine: Pengguna sesi (pemilik WorkflowManagement, PBJ-DEC-026)
  api: Pengguna sesi
  security: belum ditetapkan — penjaga lampiran dan cakupan unit perlu tinjauan
  frontend_authority: PBJ-FE-001 (approved), PBJ-FE-002 (DEV_DISCRETION)
approved_by: Pengguna sesi (Product Owner Pengadaan)
approved_at: 2026-11-12
backend_commit_sha: 100c011d54147bb19ca4bd874fdfb200448ba46a
frontend_commit_sha: 763d665b2db3f31cf370ddba68623dc8aa57cf18
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; tanpa data klinis/billing
contract_versions:
  set: "1.0"
  api-contract: PBJ-PP-API 1.1 (approved, PBJ-DEC-037)
  state-transition-matrix: PBJ-PP-STATE 1.0 (approved)
  validation-matrix: PBJ-PP-VAL 1.0 (approved)
  integration-contract: PBJ-PP-INT 1.1 (approved, PBJ-DEC-038)
  permission-audit-matrix: PBJ-PP-PERM 1.2 (approved, PBJ-DEC-036/037)
  acceptance-test-matrix: PBJ-PP-TEST 1.0 (approved)
  prd-to-mvp: PBJ-PP-MVP 1.0 (approved)
input_revisions:
  00-interview-decisions.md: decision_revision 5 (PBJ-DEC-001..034)
  01-existing-capability-map.md: rev 1
  02-requirement-completeness-assessment.md: rev 2
  02-module-map.md: rev 2
input_hashes:            # SHA256 dipotong 16 karakter, 12 November 2026
  00-interview-decisions.md: acbf1a0c1d665d55
  01-existing-capability-map.md: c4f5244e3d2a5103
  02-requirement-completeness-assessment.md: 8edccb99ec732d9f
  02-module-map.md: be513bb46c0fef07
artifact_hashes:         # dihitung ulang 2026-11-12 setelah delta PBJ-DEC-038 (INT 1.1) dan catatan delta BE-PBJ-006 (API)
  02-backend-architecture.md: 3d3f553aabaf9bce
  03-frontend-architecture.md: d2309091234a3bd9
  04-prd-to-mvp.md: fd60f3468cedc8b2
  data/data-dictionary.md: 82447bc13e5fb9c9
  contracts/api-contract.md: abb1093c7d393c0e
  contracts/state-transition-matrix.md: eb2c48c11772c078
  contracts/validation-matrix.md: 8a81846861c10f36
  contracts/integration-contract.md: 76612787abef1cc7
  contracts/permission-audit-matrix.md: e369770743218d4a
  testing/acceptance-test-matrix.md: 2bc19798685e1463
  flowcharts/00-alur-utama.md: c5e713fdce1e0c6c
  flowcharts/pengajuan-dan-verifikasi-pr.md: 6bc5bf4023d1fe64
  flowcharts/rencana-kebutuhan-dan-revisi.md: 01473caf3a8d9b91
  flowcharts/pagu-anggaran.md: 14b9ae43b55ae67f
  flowcharts/konsolidasi-rencana-paket.md: 97420c9936a09fdc
roadmap: roadmap/backend-roadmap.md, roadmap/frontend-roadmap.md, roadmap/requirement-traceability.md (rev 1, draft)
dependencies:
approval_scope: >
  Approval desain saja (PBJ-DEC-034). Tidak memberi wewenang migration, kode, maupun pendaftaran registry.
compatibility_impact: >
  Seluruh endpoint baru. Perubahan pada Wfl: satu kolom nullable di MstWorkflowStep, satu interface
  penangan, dan dua titik di WorkflowReferenceLifecycleService/WorkflowService. Perilaku HR tetap
  (diuji AT-PP-35/UAT-16).
```
