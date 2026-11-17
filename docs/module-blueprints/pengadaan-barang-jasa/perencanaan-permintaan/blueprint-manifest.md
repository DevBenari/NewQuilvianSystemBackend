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
backend_commit_sha: 9f396063025f71d86f466553cfd8ccfda6e2a545   # branch sukmagp, 2026-11-16
frontend_commit_sha: 608e46ba4dcb69dd73d0e4ca0600311abfe8ceaa  # branch sukmagpV2, 2026-11-16
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; tanpa data klinis/billing
last_verified_at: 2026-11-17
readiness: READY_WITH_CONDITIONS   # testing/readiness-report.md; syarat K-1..K-5 terbuka, K-6 selesai 2026-11-17
contract_versions:
  set: "1.0"
  api-contract: PBJ-PP-API 1.3 (approved, PBJ-DEC-037, PBJ-DEC-040, PBJ-DEC-041, PBJ-DEC-042)
  state-transition-matrix: PBJ-PP-STATE 1.0 (approved)
  validation-matrix: PBJ-PP-VAL 1.1 (approved, PBJ-DEC-041)
  integration-contract: PBJ-PP-INT 1.1 (approved, PBJ-DEC-038)
  permission-audit-matrix: PBJ-PP-PERM 1.3 (approved, PBJ-DEC-036/037/039/040)
  acceptance-test-matrix: PBJ-PP-TEST 1.1 (approved, PBJ-DEC-041, PBJ-DEC-042)
  prd-to-mvp: PBJ-PP-MVP 1.1 (approved; 1.1 = teks PBJ-OQ-019 diselaraskan PBJ-DEC-038)
input_revisions:
  00-interview-decisions.md: decision_revision 11 (PBJ-DEC-001..042)
  01-existing-capability-map.md: rev 1
  02-requirement-completeness-assessment.md: rev 2
  02-module-map.md: rev 2
input_hashes:            # SHA256 dipotong 16 karakter, dihitung ulang 17 November 2026
  00-interview-decisions.md: 1ab25f31640ca8f0
  01-existing-capability-map.md: c6072a7c98459c3f
  02-requirement-completeness-assessment.md: 8edccb99ec732d9f
  02-module-map.md: be513bb46c0fef07
artifact_hashes:         # dihitung ulang 2026-11-17 (pemeliharaan sesudah audit kesiapan)
  02-backend-architecture.md: 48b84054d56b6f58
  03-frontend-architecture.md: b62338003b42a000
  04-prd-to-mvp.md: 9c3885e5be8b6ca2
  data/data-dictionary.md: 82447bc13e5fb9c9
  contracts/api-contract.md: 226dc8dd66d2e264
  contracts/state-transition-matrix.md: eb2c48c11772c078
  contracts/validation-matrix.md: 8a49ed6a6ece94e2
  contracts/integration-contract.md: 5c289c98fbb87bc2
  contracts/permission-audit-matrix.md: 2770cc6bc402a1ba
  testing/acceptance-test-matrix.md: 8ab3ad7bc8cc7d41
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
  (AT-PP-35 PASS dengan dokumen HR pengganti; UAT-16 cuti menunggu master cuti).
```
