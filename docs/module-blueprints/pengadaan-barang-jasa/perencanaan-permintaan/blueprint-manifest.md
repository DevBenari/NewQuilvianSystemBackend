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
backend_commit_sha: 5e7778334faa85210c81513327da9a7fa6425a86   # branch sukmagp; commit terakhir yang menyentuh kode Pengadaan. Sejak 9f396063: BE-PBJ-012 (a6df5619) + tambahan aditif manajemen-vendor di Common. Diperiksa ulang di HEAD c97642bb 2026-11-26
frontend_commit_sha: af8a430aca6ac8946dc44e0980f9d370b2e2716b  # branch sukmagpV2; sejak 608e46ba4: FE-PBJ-009 (b1b360efa) + perluasan komponen bersama oleh manajemen-vendor. Diperiksa ulang di HEAD 3e5386679 2026-11-26
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; tanpa data klinis/billing
last_verified_at: 2026-11-26   # pemeriksaan ulang read-only (git, hash, SELECT dev); build/runtime tidak diulang — readiness-report bagian 9
readiness: READY_WITH_CONDITIONS   # testing/readiness-report.md; syarat K-1..K-4 terbuka (dicek ulang 2026-11-26), K-5 dan K-6 selesai 2026-11-17
contract_versions:
  set: "1.0"
  api-contract: PBJ-PP-API 1.4 (approved, PBJ-DEC-037, PBJ-DEC-040..042, PBJ-DEC-055)
  state-transition-matrix: PBJ-PP-STATE 1.0 (approved)
  validation-matrix: PBJ-PP-VAL 1.2 (approved, PBJ-DEC-041, PBJ-DEC-055)
  integration-contract: PBJ-PP-INT 1.1 (approved, PBJ-DEC-038)
  permission-audit-matrix: PBJ-PP-PERM 1.3 (approved, PBJ-DEC-036/037/039/040)
  acceptance-test-matrix: PBJ-PP-TEST 1.2 (approved, PBJ-DEC-041, PBJ-DEC-042, PBJ-DEC-055, PBJ-FE-004)
  prd-to-mvp: PBJ-PP-MVP 1.1 (approved; 1.1 = teks PBJ-OQ-019 diselaraskan PBJ-DEC-038)
input_revisions:
  00-interview-decisions.md: decision_revision 17 (PBJ-DEC-001..065, PBJ-FE-004; 064/065 hanya menyangkut manajemen-vendor)
  01-existing-capability-map.md: rev 1
  02-requirement-completeness-assessment.md: rev 4
  02-module-map.md: rev 2
input_hashes:            # SHA256 dipotong 16 karakter, dihitung ulang 17 November 2026
  00-interview-decisions.md: 516be260f1b5a189
  01-existing-capability-map.md: c6072a7c98459c3f
  02-requirement-completeness-assessment.md: f1868946ad6f6db2
  02-module-map.md: 14ba073a9cbec854
artifact_hashes:         # dihitung ulang 2026-11-17 (pemeliharaan sesudah audit kesiapan)
  02-backend-architecture.md: 48b84054d56b6f58
  03-frontend-architecture.md: b9737cf85984fad7
  04-prd-to-mvp.md: 9c3885e5be8b6ca2
  data/data-dictionary.md: 82447bc13e5fb9c9
  contracts/api-contract.md: eeb444f23f05d0c3
  contracts/state-transition-matrix.md: eb2c48c11772c078
  contracts/validation-matrix.md: bed447ccdd62d2f8
  contracts/integration-contract.md: 5c289c98fbb87bc2
  contracts/permission-audit-matrix.md: 2770cc6bc402a1ba
  testing/acceptance-test-matrix.md: fdc61d22dcd99cbb
  flowcharts/00-alur-utama.md: c5e713fdce1e0c6c
  flowcharts/pengajuan-dan-verifikasi-pr.md: 6bc5bf4023d1fe64
  flowcharts/rencana-kebutuhan-dan-revisi.md: 01473caf3a8d9b91
  flowcharts/pagu-anggaran.md: 14b9ae43b55ae67f
  flowcharts/konsolidasi-rencana-paket.md: 97420c9936a09fdc
roadmap: roadmap/backend-roadmap.md, roadmap/frontend-roadmap.md, roadmap/requirement-traceability.md (rev 2, draft; 21/21 task ✅ 2026-11-17)
dependencies:
approval_scope: >
  Approval desain saja (PBJ-DEC-034). Tidak memberi wewenang migration, kode, maupun pendaftaran registry.
compatibility_impact: >
  Seluruh endpoint baru. Perubahan pada Wfl: satu kolom nullable di MstWorkflowStep, satu interface
  penangan, dan dua titik di WorkflowReferenceLifecycleService/WorkflowService. Perilaku HR tetap
  (AT-PP-35 PASS dengan dokumen HR pengganti; UAT-16 cuti menunggu master cuti).
```
