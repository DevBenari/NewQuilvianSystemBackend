# Sub-modul `inventarisasi` — Blueprint Manifest

Sub-modul dari modul [`manajemen-aset`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`AST-DEC-001`. Identitas modul, snapshot SHA, dan registry sub-modul dipegang manifest tingkat modul.
Berkas ini memegang **status desain, `contract_versions`, `artifact_hashes`, approval, dan dependency
sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `inventarisasi` |
| `judul` | Inventarisasi & Pemeriksaan Fisik |
| `blueprint_id` | `AST-BP-001` — satu untuk seluruh modul |
| `revision` | `1` — dipegang tingkat modul |
| `status` | **`approved`** — Rizki, 2026-11-12, `AST-DEC-051` |
| `current_phase` | `AST-PH-004` — implementasi per task (roadmap `REV-1` ditulis); setiap task butuh wewenang implementasi eksplisit |
| `contract_versions` | `AST-INV-1.0.0` — `approved` 2026-11-12 (api, state-transition, validation, integration, permission-audit, acceptance-test, prd-to-mvp) |
| `tabel baru` | 6 |
| `keputusan desain menunggu persetujuan` | AST-DES-007, 008, 027..031 |
| `epic` | EPIC AST-10 |
| `gelombang` | MVP-3 |
| `dependency` | register, pergerakan; komponen pemindai QR (TS-18) |
| `prefix` | Entity `Ast` (master `Mst`); task `BE-AST-###` / `FE-AST-###`, deret bersama seluruh modul |
| `input_revision` | decision revision `4`; capability map `1.0`; `AST-RCG-001` rev 1; `02-module-map.md` rev 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` (`AST-RCG-001` bagian 7) |
| `approved_by` | Rizki (Product/Domain Owner Aset dan Akuntansi) |
| `approved_at` | 2026-11-12 |
| `roadmap` | `REV-1` 2026-11-12 — `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`; task BE-AST-020..024, FE-AST-015..018 |
| `updated_at` | 2026-11-12 |

## Artifact hashes

SHA-256 dipotong 16 karakter, dihitung sesudah desain ditulis (2026-11-12).

| Artefak | Hash |
|---|---|
| `02-backend-architecture.md` | `c880419042c3fddc` |
| `03-frontend-architecture.md` | `8449bbaebbb5af24` |
| `04-prd-to-mvp.md` | `c9da8cd709ad4dab` |
| `contracts/api-contract.md` | `604dc1b12f860de8` |
| `contracts/integration-contract.md` | `413b61babf294c82` |
| `contracts/permission-audit-matrix.md` | `f72e386b32e0fde5` |
| `contracts/state-transition-matrix.md` | `70d3c14e2becfa99` |
| `contracts/validation-matrix.md` | `29443c6a58418d99` |
| `data/data-dictionary.md` | `9ca8176aa6e51e69` |
| `flowcharts/00-alur-utama.md` | `52bed6a09786a627` |
| `flowcharts/01-pemeriksaan-dan-pencocokan.md` | `99d64db51670b439` |
| `flowcharts/02-tindak-lanjut-selisih.md` | `542988b71fa8ec05` |
| `testing/acceptance-test-matrix.md` | `d13088186b7b4cf8` |
