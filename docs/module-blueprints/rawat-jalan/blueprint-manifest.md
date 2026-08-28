# Rawat Jalan Billing — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RJ-BIL-BP-001` |
| `module_name` | Dokter / Rawat Jalan Billing |
| `module_slug` | `rawat-jalan` |
| `module_prefix` | `RJ-BIL` |
| `revision` | `11` |
| `status` | `PARTIAL` |
| `current_phase` | `RJ-BIL-PH-008` — Delivery Planning |
| `created_at` | `2026-08-20T15:06:30+07:00` |
| `updated_at` | `2026-08-21T00:00:00+07:00` |
| `last_verified_at` | `2026-08-21T00:00:00+07:00` |
| `backend_source_sha` | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| `frontend_source_sha` | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| `skill_suite_version` | `1.0.0-rc2` |
| `input_revision_hash` | `decisions:sha256:115509A84A681646E800D7F6C3382345F31F79C13B2800B6727F356C680D4B0E; capability:sha256:A91E5EB7A507D8AF6A31B87782D84423B41C284F76CD748D01CFCB262C4213B4` |
| `decision_revision` | `10` |
| `contract_versions` | `RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)` |
| `active_dependency_ids` | `RJ-BIL-DEP-001` s.d. `RJ-BIL-DEP-009` |
| `active_roadmap_revision` | `1` |
| `supersedes` | `null` |
| `domain_architecture_revision` | `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; core internal/manual siap independen |
| `owners` | Product/Domain owner; Billing/Revenue Cycle; API authority; Security/Privacy; Frontend authority |
| `approved_by` | `User-provided approval authority` |
| `approved_at` | `2026-08-21` |

## Artifact hashes

Hash artefak target dihitung pada `2026-08-20` untuk mendeteksi drift. Semua artefak desain masih
berstatus `draft` dan belum menjadi izin implementasi.

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
| `roadmap/backend-roadmap.md` | `0D63B348FBD4C6AB0B896E2DA33670F6E161935E7D02119EC73C3100F4A7D264` |
| `roadmap/frontend-roadmap.md` | `375675E85AEE6897E71CABCE4C7C3BE098576639BA9A4AB578A22E0697CC0865` |
| `roadmap/requirement-traceability.md` | `71E68DEB04D3D1C5D31B9A6F8077CFB998958FD439A0D4FCC3874D74B2AE2CCE` |

Arsitektur domain revision `1` sudah tersedia. Fase berikutnya adalah `design-business-module`
untuk slice core internal/manual yang siap. Aktivasi adapter eksternal tetap terblokir oleh
`RJ-BIL-DEP-009`.
