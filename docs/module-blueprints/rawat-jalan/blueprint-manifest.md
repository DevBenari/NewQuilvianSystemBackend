# Rawat Jalan Billing — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RJ-BIL-BP-001` |
| `module_name` | Dokter / Rawat Jalan Billing |
| `module_slug` | `rawat-jalan` |
| `module_prefix` | `RJ-BIL` |
| `revision` | `14` |
| `status` | `PARTIAL` |
| `current_phase` | `RJ-BIL-PH-008` — Delivery Planning |
| `created_at` | `2026-08-20T15:06:30+07:00` |
| `updated_at` | `2026-08-27T12:52:04+07:00` |
| `last_verified_at` | `2026-08-27T12:52:04+07:00` |
| `backend_source_sha` | `6b25e6049e60e055593968abe463262b59842527` cabang `sukmagp`; working tree `RJ-BIL-BE-003` beserta remediasi penamaan belum di-commit |
| `frontend_source_sha` | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| `skill_suite_version` | `1.0.0-rc2` |
| `input_revision_hash` | `decisions:sha256:59723FA1F8D84298152632C7021B5C101E5E033096BFEB5AD324B4A67AAFB056; capability:sha256:D1CB1D052474FA96F0BE801F7CEA277AEB0604A9969247B7323F82D23F5152B7` |
| `decision_revision` | `12` |
| `contract_versions` | `RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)` |
| `active_dependency_ids` | `RJ-BIL-DEP-001` s.d. `RJ-BIL-DEP-009` |
| `active_roadmap_revision` | `1` |
| `supersedes` | `null` |
| `domain_architecture_revision` | `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; core internal/manual siap independen |
| `owners` | Product/Domain: **Sukma Giri**. Juga Product/Domain Owner `LaboratoryManagement` sejak `RJ-BIL-DEC-007`. Billing/Revenue Cycle, API authority, Security/Privacy, Frontend authority: `OPEN` |
| `approved_by` | `User-provided approval authority` |
| `approved_at` | `2026-08-21` |

## Artifact hashes

Hash artefak target dihitung ulang pada `2026-08-26`. Semua artefak desain masih berstatus
`draft` dan belum menjadi izin implementasi.

**Catatan verifikasi `2026-08-26`.** Perhitungan sebelumnya dilakukan `2026-08-20`, sedangkan
ketiga berkas `roadmap/` baru dibuat commit `fe6d15c` pada `2026-08-21`. Akibatnya ketiga hash
roadmap tidak pernah cocok sejak awal — bukan drift desain, melainkan hash yang dicatat sebelum
berkasnya ada. Sembilan artefak desain dan kontrak diverifikasi ulang pada tanggal ini dan
**seluruhnya cocok**, sehingga tidak ada satu pun artefak desain yang berubah diam-diam.

| Artifact | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `524AF2A661A77092092A9A896571415DA0FA45576CDB5D01B6C624DCA3FFA22E` |
| `03-frontend-architecture.md` | `05304BDFC6323930E80516AC12A73A7CB34FD3A0378C0449EB560E58BADFA4E8` |
| `hospital-domain-architecture.md` | `E7B0B0F08DB9CFB2B7FEF6727F705FA09ED0720CC6FEE32F7EE0C1DD8EADF96E` |
| `contracts/api-contract.md` | `9508986836A537DD88A664F052003AE1A8509EE0555DD6DC2F3F83AB6B871FD6` |
| `contracts/state-transition-matrix.md` | `1688209BFEEAFA3F5A48A70376FCE4CA291239F189343FFF615D05C2C39B5807` |
| `contracts/validation-matrix.md` | `8C3FF0A302BAB10BC6DC904630F3FE0BD2DA675F1DE2491586F08CF596209E53` |
| `contracts/integration-contract.md` | `5743FC7B31A27500360FDE9E3EA61856D1CB4290539B07CDA8640F6F9E112DB6` |
| `contracts/permission-audit-matrix.md` | `1424ADE6BB5084C8A77105477C65B347959C6C17570A88E2E9663C38C23FF093` |
| `testing/acceptance-test-matrix.md` | `FEC2E3816EF086540FB85EAB9242A559906BF65154B1DC311068A5C999840EF6` |
| `owner-review-checklist.md` | `review artifact; hash dihitung setelah owner mengisi record approval` |
| `roadmap/backend-roadmap.md` | `1454222F93792E3305415181B6E978189411DAA83F0ED7EAAE540976344B0B51` |
| `roadmap/frontend-roadmap.md` | `09500DE9A72A941594767CA2FC74E9A3F1A3210A9647F29086F60A5B206CF354` |
| `roadmap/requirement-traceability.md` | `E60274E7C7D10B425E5F8B9CBF91F1265C3AE7B305C7991923E8E6B798DC2937` |

Catatan verifikasi `2026-08-27` (revision `14`). Seluruh hash artefak dihitung ulang. Sebelas
artefak cocok tanpa perubahan. Satu artefak berubah dan hash-nya diperbarui di sini:
`roadmap/backend-roadmap.md`, karena penandaan status task dan koreksi keadaan migration. Baris
`owner-review-checklist.md` sengaja tidak memuat hash; isinya catatan, dan tetap demikian sampai
owner mengisi record approval. `active_roadmap_revision` tetap `1` karena pembaruan itu tidak
menyentuh cakupan, acceptance criteria, maupun dependency task mana pun.

Arsitektur domain revision `1` sudah tersedia. Fase berikutnya adalah `design-business-module`
untuk slice core internal/manual yang siap. Aktivasi adapter eksternal tetap terblokir oleh
`RJ-BIL-DEP-009`.
