# Pemilihan Penyedia — Manifest Sub-modul

```yaml
submodule_slug: pemilihan-penyedia
parent_blueprint_id: PBJ-BP-001
parent_revision: 2
id_prefix: PBJ-PS
entity_prefix: Prc                 # PBJ-DEC-035; submodule Sourcing BELUM terdaftar di registry (prasyarat implementasi, PBJ-CAP-030)
status: draft                      # menunggu approval pengguna sesi
owners:
  product_domain: Pengguna sesi (Product Owner Pengadaan, PBJ-DEC-017)
  perencanaan_permintaan: Pengguna sesi — menyetujui perluasan baris PR, lampiran, dan amendment PBJ-PP-INT 1.2
  workflow_engine: Pengguna sesi (PBJ-DEC-026)
  api: Pengguna sesi
  security: belum ditetapkan — sama dengan sub-modul lain (K-3 / VD K-5)
  frontend_authority: PBJ-FE-001 (nama 11 submenu menunggu PBJ-OQ-038), PBJ-FE-004 (approved), PBJ-FE-002 (DEV_DISCRETION)
approved_by: null
approved_at: null
backend_commit_sha: d5fdc8faefb8724810fcd1fe42f9128a2fcc0ede   # branch sukmagp; working tree: dokumen blueprint belum di-commit
frontend_commit_sha: 3e538667994feba533d2c93a142a6f333092066d  # branch sukmagpV2, bersih
last_verified_at: null
readiness: null
requirement_readiness: READY_FOR_DOMAIN_DESIGN    # 02-requirement-completeness-assessment.md rev 6 bagian 11 (S-P1..S-P10)
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # administratif; alasan di penilaian bagian 10.6
contract_versions:
  set: "1.0"
  api-contract: PBJ-PS-API 1.0 (draft)
  state-transition-matrix: PBJ-PS-STATE 1.0 (draft)
  validation-matrix: PBJ-PS-VAL 1.0 (draft)
  integration-contract: PBJ-PS-INT 1.0 (draft) — menuntut amendment PBJ-PP-INT 1.1 → 1.2
  permission-audit-matrix: PBJ-PS-PERM 1.0 (draft)
  acceptance-test-matrix: PBJ-PS-TEST 1.0 (draft)
  prd-to-mvp: PBJ-PS-MVP 1.0 (draft)
input_revisions:
  00-interview-decisions.md: decision_revision 18 (PBJ-DEC-001..085, PBJ-ASM-001..009, PBJ-FE-004)
  01-existing-capability-map.md: rev 2 (impact scan pemilihan-penyedia, bagian 7)
  02-requirement-completeness-assessment.md: rev 6
  02-module-map.md: rev 2 (bagian pemilihan-penyedia diperbarui 2026-11-26)
input_hashes:            # SHA256 dipotong 16 karakter, 26 November 2026
  00-interview-decisions.md: 07347fb05b69760a
  01-existing-capability-map.md: 76c6082ff69a2e36
  02-requirement-completeness-assessment.md: 9bdfc26becf804f4
  02-module-map.md: ecd3f6c0c9e50106
artifact_hashes:
  02-backend-architecture.md: d0bf5d5b35dd8045
  03-frontend-architecture.md: efec370a7ad983e4
  04-prd-to-mvp.md: 4cec3e1c65ab62f7
  data/data-dictionary.md: bdb4067a1dafa5af
  contracts/api-contract.md: fb8824bbc0c3737f
  contracts/state-transition-matrix.md: 042f3894e38b01c2
  contracts/validation-matrix.md: ee4636d509e4003f
  contracts/integration-contract.md: 0fadde2b078fa590
  contracts/permission-audit-matrix.md: 229095f95b7ca6f5
  testing/acceptance-test-matrix.md: b7209d4abdacf2f9
  flowcharts/00-alur-utama.md: dc6ee3751738c1b5
  flowcharts/persiapan-paket.md: 854173087b33a10d
  flowcharts/penawaran-dan-evaluasi.md: 058d3ac043854df1
  flowcharts/penetapan-dan-pesanan.md: 10a5c61cf9bfc154
roadmap: null            # dibuat plan-module-delivery setelah approval
dependencies:
  - perencanaan-permintaan: PrcPurchaseRequestLine (Diperbarui, PackageId), ProcurementPurchaseRequestService (3 metode baru), ProcurementBudgetCommitmentService, ProcurementAttachmentService (saringan harga), ProcurementApprovalGateway, ProcurementDataScopeService, MstProcurementItem/Category/Type
  - manajemen-vendor: ProcurementVendorEligibilityService (CheckAsync, GetEligibleAsync)
  - Administrator Master Data: MstSupplier (baca, termasuk penanda pajak)
  - Registry: baris ProcurementManagement / Sourcing (Prc) — BELUM ADA
approval_scope: >
  Approval desain saja. Tidak memberi wewenang migration, kode, database, maupun pendaftaran registry.
compatibility_impact: >
  Seluruh endpoint Sourcing baru. Perubahan pada perencanaan-permintaan: dua kolom nullable di PrcPurchaseRequestLine,
  tiga metode integrasi baru (menggantikan janji PBJ-PP-INT 1.1 yang belum dibangun), label "termasuk PPN" pada harga
  perkiraan PR dan pagu, dan saringan lampiran harga di ProcurementAttachmentService. PR dapat kembali dari
  CONVERTED_TO_PACKAGE ke APPROVED saat paket batal (PBJ-DEC-077).
open_items_for_approval:
  - G-P08 penetapan disetujui tetapi vendor tidak layak saat diterapkan → NOT_APPLIED (usulan)
  - G-P09 cabut penetapan sebelum PO oleh Manajer Pengadaan (usulan)
  - G-P15 komitmen butuh garis pagu yang ada; tanpa pagu penetapan tidak diterapkan (usulan, muncul saat desain)
  - Registry submodule Sourcing (Prc) — keputusan terpisah, pola PBJ-DEC-063
```
