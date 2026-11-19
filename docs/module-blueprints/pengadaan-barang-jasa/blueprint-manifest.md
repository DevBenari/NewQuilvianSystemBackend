# Pengadaan Barang & Jasa — Blueprint Manifest

```yaml
blueprint_id: PBJ-BP-001
module_name: Pengadaan Barang & Jasa
module_slug: pengadaan-barang-jasa
module_prefix: PBJ            # ID dokumen blueprint saja (PBJ-DEC-001, PBJ-OQ-001, PBJ-BE-001, ...)
entity_prefix: Prc             # PBJ-DEC-035 — terdaftar ACTIVE di registry 2026-11-12
module_area: Areas/Corporate/ProcurementManagement   # PBJ-DEC-035
revision: 2                   # rev 2 = desain sub-modul perencanaan-permintaan (12 Nov 2026); PBJ-DEC-035..058 dibawa versi kontrak sub-modul
status: partial               # DITURUNKAN: perencanaan-permintaan dan manajemen-vendor approved, dua NOT_STARTED
current_phase: PBJ-PH-007     # audit kesiapan: perencanaan-permintaan dan manajemen-vendor READY_WITH_CONDITIONS; sign-off menunggu syarat masing-masing (vendor K-4 selesai 2026-11-19)
blueprint_shape: COMPOSITE      # PBJ-DEC-004; sub-modul: perencanaan-permintaan, pemilihan-penyedia, manajemen-vendor, kontrak-pelaksanaan (folder dibuat oleh design-business-module)
shape_decided_by: USER_CONFIRMED
created_at: 2026-11-12
updated_at: 2026-11-19
last_verified_at: 2026-11-19   # verify-module-readiness: manajemen-vendor READY_WITH_CONDITIONS (perencanaan-permintaan 2026-11-17), modul NOT_READY
backend_source_sha: a6df5619f4ce72c04a929ac43e7e08b7c40332e4   # branch sukmagp; perubahan manajemen-vendor belum di-commit
frontend_source_sha: b1b360efa8990caafa389c38d3ce0346b2cdff50  # branch sukmagpV2; perubahan manajemen-vendor belum di-commit
skill_suite_version: quilvian-engineering-skills 1.19.1
input_revision_hash: BRD-PRD-PBJ-1.0-DRAFT-2026-11-12   # dokumen BRD & PRD v1.0 dari pengguna (lihat 00-business-overview.md)
decision_revision: 17         # PBJ-DEC-001..065, PBJ-FE-004
contract_versions: per sub-modul (lihat manifest sub-modul)
active_dependency_ids: [PBJ-DEP-001, PBJ-DEP-002, PBJ-DEP-003, PBJ-DEP-004, PBJ-DEP-005]
active_roadmap_revision: perencanaan-permintaan/roadmap rev 2 (2026-11-17); manajemen-vendor/roadmap rev 1 (2026-11-17; 13/13 ✅ 2026-11-19)
supersedes: null
submodules:
  - slug: perencanaan-permintaan
    prefix: PBJ-PP
    status: approved
    approved_by: Pengguna sesi
    approved_at: 2026-11-12
    contract_versions: "1.0 (API 1.4, PERM 1.3, INT 1.1, VAL 1.2, TEST 1.2, MVP 1.1; lihat manifest sub-modul)"
    manifest: perencanaan-permintaan/blueprint-manifest.md
    readiness: READY_WITH_CONDITIONS
    readiness_report: perencanaan-permintaan/testing/readiness-report.md
  - slug: pemilihan-penyedia
    prefix: PBJ-PS
    status: NOT_STARTED
    blocked_by: PBJ-OQ-005 (metode pengadaan) sebelum gerbang kelengkapan
  - slug: manajemen-vendor
    prefix: PBJ-VD
    status: approved
    approved_by: Pengguna sesi
    approved_at: 2026-11-17   # PBJ-DEC-062; registry Vendor PBJ-DEC-063
    contract_versions: "1.1 (API, STATE, VAL, INT 1.1 PBJ-DEC-064; PERM 1.1 PBJ-DEC-065; TEST, MVP 1.0)"
    manifest: manajemen-vendor/blueprint-manifest.md
    readiness: READY_WITH_CONDITIONS
    readiness_report: manajemen-vendor/testing/readiness-report.md
  - slug: kontrak-pelaksanaan
    prefix: PBJ-KP
    status: NOT_STARTED
    blocked_by: bergantung pemilihan-penyedia
requirement_readiness: READY_FOR_DOMAIN_DESIGN   # perencanaan-permintaan saja; 02-requirement-completeness-assessment.md rev 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # alasan di 02-requirement-completeness-assessment.md bagian 6
module_readiness: NOT_READY   # diturunkan dari sub-modul terlemah: pemilihan-penyedia dan kontrak-pelaksanaan NOT_STARTED (2026-11-19)
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
