# Manajemen Vendor — Manifest Sub-modul

```yaml
submodule_slug: manajemen-vendor
parent_blueprint_id: PBJ-BP-001
parent_revision: 2
id_prefix: PBJ-VD
entity_prefix: Prc                 # PBJ-DEC-035; submodule Vendor ACTIVE 2026-11-17 (PBJ-DEC-063)
status: approved                   # PBJ-DEC-062
owners:
  product_domain: Pengguna sesi (Product Owner Pengadaan, PBJ-DEC-017)
  master_data_supplier: Pengguna sesi (PBJ-DEC-026) — menyetujui PBJ-DEC-060
  workflow_engine: Pengguna sesi (PBJ-DEC-026)
  api: Pengguna sesi
  security: belum ditetapkan — sama dengan perencanaan-permintaan (K-3)
  frontend_authority: PBJ-FE-001, PBJ-FE-004 (approved), PBJ-FE-002 (DEV_DISCRETION)
approved_by: Pengguna sesi (Product Owner Pengadaan)
approved_at: 2026-11-17
backend_commit_sha: a6df5619f4ce72c04a929ac43e7e08b7c40332e4   # branch sukmagp HEAD 2026-11-19 + BE-PBJ-VD-001..008 (termasuk D-9/D-10) belum di-commit
frontend_commit_sha: b1b360efa8990caafa389c38d3ce0346b2cdff50  # branch sukmagpV2 HEAD 2026-11-19 + FE-PBJ-VD-001..005 belum di-commit
last_verified_at: 2026-11-19
readiness: READY_WITH_CONDITIONS   # testing/readiness-report.md; syarat K-1..K-3, K-5..K-7 terbuka; K-4 selesai 2026-11-19
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 4 (S-V1..S-V6)
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; alasan di penilaian bagian 8.6
contract_versions:
  set: "1.1"   # PBJ-DEC-064, PBJ-DEC-065 (2026-11-19)
  api-contract: PBJ-VD-API 1.1 (approved, PBJ-DEC-062; 1.1 PBJ-DEC-064)
  state-transition-matrix: PBJ-VD-STATE 1.1 (approved, PBJ-DEC-062; 1.1 PBJ-DEC-064)
  validation-matrix: PBJ-VD-VAL 1.1 (approved, PBJ-DEC-062; 1.1 PBJ-DEC-064)
  integration-contract: PBJ-VD-INT 1.1 (approved, PBJ-DEC-062; 1.1 PBJ-DEC-064)
  permission-audit-matrix: PBJ-VD-PERM 1.1 (approved, PBJ-DEC-062; 1.1 PBJ-DEC-065)
  acceptance-test-matrix: PBJ-VD-TEST 1.0 (approved, PBJ-DEC-062)
  prd-to-mvp: PBJ-VD-MVP 1.0 (approved, PBJ-DEC-062)
input_revisions:
  00-interview-decisions.md: decision_revision 17 (PBJ-DEC-001..065, PBJ-FE-004)
  01-existing-capability-map.md: rev 1 (PBJ-CAP-009..011 dicek ulang terhadap c6b86a6e)
  02-requirement-completeness-assessment.md: rev 4
  02-module-map.md: rev 2
input_hashes:            # SHA256 dipotong 16 karakter, 17 November 2026
  00-interview-decisions.md: 516be260f1b5a189   # 2026-11-19 (PBJ-DEC-064, 065)
  01-existing-capability-map.md: c6072a7c98459c3f
  02-requirement-completeness-assessment.md: f1868946ad6f6db2
  02-module-map.md: 14ba073a9cbec854
artifact_hashes:
  02-backend-architecture.md: 9d7f9f92c3daa7ee
  03-frontend-architecture.md: 6935bea17074bd22
  04-prd-to-mvp.md: ac593d3b69e43f27
  data/data-dictionary.md: d754d4edc1f6499c
  contracts/api-contract.md: 2997eb86f7833960
  contracts/state-transition-matrix.md: 974540a83d0599cb
  contracts/validation-matrix.md: a067223d9e0805b5
  contracts/integration-contract.md: ccc002f34a4df8ea
  contracts/permission-audit-matrix.md: 0ce5fa0d1dfc6465
  testing/acceptance-test-matrix.md: ef57347273d5b282
  flowcharts/00-alur-utama.md: 49a10558b3489e5c
  flowcharts/dokumen-legalitas.md: 4fa4569518035f5b
  flowcharts/status-vendor.md: 491f77a28ef05eaf
  flowcharts/penilaian-vendor.md: a37c96fc9fe98e13
  testing/readiness-report.md: 615c5fd34e4a6f3d   # 2026-11-19
roadmap: roadmap/backend-roadmap.md, roadmap/frontend-roadmap.md, roadmap/requirement-traceability.md (rev 1, draft, 2026-11-17; 13/13 task ✅ 2026-11-19: 8 task BE, 5 task FE)
dependencies:
  - perencanaan-permintaan: MstProcurementItemCategory, PrcAttachment, PrcDocumentHistory, ProcurementApprovalGateway
  - Administrator Master Data: MstSupplier, SupplierController (PBJ-DEC-060)
approval_scope: >
  Approval desain saja. Tidak memberi wewenang migration, kode, maupun pendaftaran registry.
compatibility_impact: >
  Seluruh endpoint Pengadaan baru. Satu perubahan perilaku modul Administrator: Master Data Supplier berhenti menulis
  IsBlacklisted/BlacklistReason (PBJ-DEC-060); skema MstSupplier tidak berubah.
```
