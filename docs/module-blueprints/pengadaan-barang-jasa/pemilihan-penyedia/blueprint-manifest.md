# Pemilihan Penyedia — Manifest Sub-modul

```yaml
submodule_slug: pemilihan-penyedia
parent_blueprint_id: PBJ-BP-001
parent_revision: 2
id_prefix: PBJ-PS
entity_prefix: Prc                 # PBJ-DEC-035; submodule ProcurementManagement / Sourcing ACTIVE 2026-11-26 (PBJ-DEC-096)
status: approved                   # PBJ-DEC-095
owners:
  product_domain: Pengguna sesi (Product Owner Pengadaan, PBJ-DEC-017)
  perencanaan_permintaan: Pengguna sesi — menyetujui perluasan baris PR, lampiran, dan amendment PBJ-PP-INT 1.2 (PBJ-DEC-095)
  workflow_engine: Pengguna sesi (PBJ-DEC-026)
  api: Pengguna sesi
  security: belum ditetapkan — sama dengan sub-modul lain (K-3 / VD K-5)
  frontend_authority: PBJ-FE-001 (nama 11 submenu menunggu PBJ-OQ-038), PBJ-FE-004 (approved), PBJ-FE-002 (DEV_DISCRETION)
approved_by: Pengguna sesi (Product Owner Pengadaan)
approved_at: 2026-11-26
backend_commit_sha: 45b227773ce446703ab30b99393511d22d28a097   # branch sukmagp; desain draft di-commit dan di-push; approval dicatat di working tree sesudahnya
frontend_commit_sha: 3e538667994feba533d2c93a142a6f333092066d  # branch sukmagpV2, bersih
last_verified_at: null
readiness: null                    # belum dibangun
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 6 bagian 11 (S-P1..S-P10); gap PROPOSED ditutup PBJ-DEC-086..095
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; alasan di penilaian bagian 10.6
contract_versions:
  set: "1.0"
  api-contract: PBJ-PS-API 1.0 (approved, PBJ-DEC-095)
  state-transition-matrix: PBJ-PS-STATE 1.0 (approved, PBJ-DEC-095)
  validation-matrix: PBJ-PS-VAL 1.0 (approved, PBJ-DEC-095)
  integration-contract: PBJ-PS-INT 1.0 (approved, PBJ-DEC-095) — bersama amendment PBJ-PP-INT 1.1 → 1.2
  permission-audit-matrix: PBJ-PS-PERM 1.0 (approved, PBJ-DEC-095)
  acceptance-test-matrix: PBJ-PS-TEST 1.0 (approved, PBJ-DEC-095)
  prd-to-mvp: PBJ-PS-MVP 1.0 (approved, PBJ-DEC-095)
input_revisions:
  00-interview-decisions.md: decision_revision 19 (PBJ-DEC-001..096, PBJ-ASM-001..009, PBJ-FE-004)
  01-existing-capability-map.md: rev 2 (impact scan pemilihan-penyedia, bagian 7)
  02-requirement-completeness-assessment.md: rev 6 (+ catatan penutupan saat approval)
  02-module-map.md: rev 2 (bagian pemilihan-penyedia approved 2026-11-26)
input_hashes:            # SHA256 dipotong 16 karakter, dihitung ulang saat approval 26 November 2026
  00-interview-decisions.md: 87a0135099b182ed
  01-existing-capability-map.md: 76c6082ff69a2e36
  02-requirement-completeness-assessment.md: 2263e90d532dfaba
  02-module-map.md: 87697c894114d4c8
artifact_hashes:         # dihitung ulang saat approval 26 November 2026 (penanda [USULAN] diganti nomor keputusan)
  02-backend-architecture.md: 205ac71a60af699a
  03-frontend-architecture.md: b46a6d9a2096ec95
  04-prd-to-mvp.md: 39dd342a8d659f73
  data/data-dictionary.md: 4e16e572fee2730a
  contracts/api-contract.md: 67c939c66ff75be1
  contracts/state-transition-matrix.md: cb6f38dd9a58fa0f
  contracts/validation-matrix.md: cd8d087f66e10044
  contracts/integration-contract.md: ab0b71713712a18b
  contracts/permission-audit-matrix.md: 4f4d321a70b45660
  testing/acceptance-test-matrix.md: 184902989a58cde2
  flowcharts/00-alur-utama.md: dc6ee3751738c1b5
  flowcharts/persiapan-paket.md: 854173087b33a10d
  flowcharts/penawaran-dan-evaluasi.md: 058d3ac043854df1
  flowcharts/penetapan-dan-pesanan.md: 10a5c61cf9bfc154
roadmap: roadmap/backend-roadmap.md, roadmap/frontend-roadmap.md, roadmap/requirement-traceability.md (rev 1, draft, 2026-11-26; 15 task BE, 8 task FE; FE-PBJ-PS-008 ⛔ PBJ-OQ-038)
dependencies:
  - perencanaan-permintaan: PrcPurchaseRequestLine (Diperbarui, PackageId), ProcurementPurchaseRequestService (3 metode baru, PBJ-PP-INT 1.2), ProcurementBudgetCommitmentService, ProcurementAttachmentService (saringan harga), ProcurementApprovalGateway, ProcurementDataScopeService, MstProcurementItem/Category/Type
  - manajemen-vendor: ProcurementVendorEligibilityService (CheckAsync, GetEligibleAsync)
  - Administrator Master Data: MstSupplier (baca, termasuk penanda pajak)
  - Registry: baris ProcurementManagement / Sourcing (Prc) — ACTIVE 2026-11-26 (PBJ-DEC-096)
approval_scope: >
  Approval desain saja (PBJ-DEC-095). Tidak memberi wewenang migration, kode, maupun database. Registry Sourcing
  diputuskan terpisah (PBJ-DEC-096) dan hanya memberi wewenang penamaan dan kepemilikan.
compatibility_impact: >
  Seluruh endpoint Sourcing baru. Perubahan pada perencanaan-permintaan: dua kolom nullable di PrcPurchaseRequestLine,
  tiga metode integrasi baru (PBJ-PP-INT 1.2, menggantikan dua metode 1.1 yang belum dibangun), label "termasuk PPN" pada
  harga perkiraan PR dan pagu, dan saringan lampiran harga di ProcurementAttachmentService. PR dapat kembali dari
  CONVERTED_TO_PACKAGE ke APPROVED saat paket batal (PBJ-DEC-077).
open_items:
  - PBJ-OQ-038 nama 11 submenu menu 4 dan 6 — label menu frontend saja
  - PBJ-OQ-005, PBJ-OQ-019 nilai SOP (metode, batas nilai, penyetuju) — konfigurasi awal sebelum pemakaian nyata
  - PBJ-OQ-015 SLA dan notifikasi aktif — POST-MVP
```
