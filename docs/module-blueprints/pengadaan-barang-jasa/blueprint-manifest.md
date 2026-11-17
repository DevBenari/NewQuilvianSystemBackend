# Pengadaan Barang & Jasa — Blueprint Manifest

```yaml
blueprint_id: PBJ-BP-001
module_name: Pengadaan Barang & Jasa
module_slug: pengadaan-barang-jasa
module_prefix: PBJ            # ID dokumen blueprint saja (PBJ-DEC-001, PBJ-OQ-001, PBJ-BE-001, ...)
entity_prefix: Prc             # PBJ-DEC-035 — terdaftar ACTIVE di registry 2026-11-12
module_area: Areas/Corporate/ProcurementManagement   # PBJ-DEC-035
revision: 2                   # rev 2 = desain sub-modul perencanaan-permintaan (12 Nov 2026); PBJ-DEC-035..042 dibawa versi kontrak sub-modul
status: partial               # DITURUNKAN: perencanaan-permintaan approved, tiga lainnya NOT_STARTED
current_phase: PBJ-PH-007     # audit kesiapan perencanaan-permintaan; sign-off menunggu K-1..K-4
blueprint_shape: COMPOSITE      # PBJ-DEC-004; sub-modul: perencanaan-permintaan, pemilihan-penyedia, manajemen-vendor, kontrak-pelaksanaan (folder dibuat oleh design-business-module)
shape_decided_by: USER_CONFIRMED
created_at: 2026-11-12
updated_at: 2026-11-17
last_verified_at: 2026-11-17   # verify-module-readiness: perencanaan-permintaan READY_WITH_CONDITIONS, modul NOT_READY
backend_source_sha: 9f396063025f71d86f466553cfd8ccfda6e2a545   # branch sukmagp
frontend_source_sha: 608e46ba4dcb69dd73d0e4ca0600311abfe8ceaa  # branch sukmagpV2
skill_suite_version: quilvian-engineering-skills 1.19.1
input_revision_hash: BRD-PRD-PBJ-1.0-DRAFT-2026-11-12   # dokumen BRD & PRD v1.0 dari pengguna (lihat 00-business-overview.md)
decision_revision: 11         # PBJ-DEC-001..042
contract_versions: per sub-modul (lihat manifest sub-modul)
active_dependency_ids: [PBJ-DEP-001, PBJ-DEP-002, PBJ-DEP-003, PBJ-DEP-004, PBJ-DEP-005]
active_roadmap_revision: perencanaan-permintaan/roadmap rev 1 (2026-11-12)
supersedes: null
submodules:
  - slug: perencanaan-permintaan
    prefix: PBJ-PP
    status: approved
    approved_by: Pengguna sesi
    approved_at: 2026-11-12
    contract_versions: "1.0 (API 1.3, PERM 1.3, INT 1.1, VAL 1.1, TEST 1.1, MVP 1.1; lihat manifest sub-modul)"
    manifest: perencanaan-permintaan/blueprint-manifest.md
    readiness: READY_WITH_CONDITIONS
    readiness_report: perencanaan-permintaan/testing/readiness-report.md
  - slug: pemilihan-penyedia
    prefix: PBJ-PS
    status: NOT_STARTED
    blocked_by: PBJ-OQ-005 (metode pengadaan) sebelum gerbang kelengkapan
  - slug: manajemen-vendor
    prefix: PBJ-VD
    status: NOT_STARTED
  - slug: kontrak-pelaksanaan
    prefix: PBJ-KP
    status: NOT_STARTED
    blocked_by: bergantung pemilihan-penyedia
requirement_readiness: READY_FOR_DOMAIN_DESIGN   # perencanaan-permintaan saja; 02-requirement-completeness-assessment.md rev 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # alasan di 02-requirement-completeness-assessment.md bagian 6
module_readiness: NOT_READY   # diturunkan dari sub-modul terlemah: tiga sub-modul NOT_STARTED (2026-11-17)
```

## Catatan identitas

- Modul ini **baru**. Tidak ada blueprint lain bernama Pengadaan/Procurement di `docs/module-blueprints/`.
- Modul ini **bersinggungan** dengan blueprint `finance-management` (`FIN-BP-001`), yang sudah memiliki
  Purchase Order (PO), Tanda Terima Barang (GR), Tukar Faktur, Faktur Pembelian, dan Retur Pembelian
  (`FIN-DEC-045`). PO dan GR bersumber dari sistem BLM (`FIN-DEC-261`). Batas antara kedua modul
  ditetapkan oleh `PBJ-DEC-001`.
- Manifest ini dibuat dalam `MODULE BLUEPRINT MODE`. Wewenangnya diberikan pengguna pada 12 November 2026
  ("saya setuju A") atas langkah yang diusulkan. Mode ini tidak memberi wewenang menulis kode, migration,
  database, registry prefix, maupun publikasi Git.
