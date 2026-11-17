# Sub-modul `penghapusan` — Blueprint Manifest

Sub-modul dari modul [`manajemen-aset`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`AST-DEC-001`. Identitas modul, snapshot SHA, dan registry sub-modul dipegang manifest tingkat modul.
Berkas ini memegang **status desain, `contract_versions`, `artifact_hashes`, approval, dan dependency
sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `penghapusan` |
| `judul` | Penghapusan & Pelepasan Aset |
| `blueprint_id` | `AST-BP-001` — satu untuk seluruh modul |
| `revision` | `1` — dipegang tingkat modul |
| `status` | **`approved`** — Rizki, 2026-11-12, `AST-DEC-051` |
| `current_phase` | `AST-PH-004` — implementasi per task (roadmap `REV-1` ditulis); setiap task butuh wewenang implementasi eksplisit |
| `contract_versions` | `AST-HPS-1.0.0` — `approved` 2026-11-12 (api, state-transition, validation, integration, permission-audit, acceptance-test, prd-to-mvp) |
| `tabel baru` | 4 |
| `keputusan desain menunggu persetujuan` | AST-DES-013, 019, 037..041 |
| `epic` | EPIC AST-16, AST-17 |
| `gelombang` | MVP-5 |
| `dependency` | nilai MVP-4; DEP-AST-01, 02 untuk kejadian |
| `prefix` | Entity `Ast` (master `Mst`); task `BE-AST-###` / `FE-AST-###`, deret bersama seluruh modul |
| `input_revision` | decision revision `4`; capability map `1.0`; `AST-RCG-001` rev 1; `02-module-map.md` rev 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` (`AST-RCG-001` bagian 7) |
| `approved_by` | Rizki (Product/Domain Owner Aset dan Akuntansi) |
| `approved_at` | 2026-11-12 |
| `roadmap` | `REV-1` 2026-11-12 — `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`; task BE-AST-033..037, FE-AST-024..026 |
| `updated_at` | 2026-11-12 |

## Artifact hashes

SHA-256 dipotong 16 karakter, dihitung sesudah desain ditulis (2026-11-12).

| Artefak | Hash |
|---|---|
| `02-backend-architecture.md` | `9ced0cb91062f148` |
| `03-frontend-architecture.md` | `0fc834327baf00c3` |
| `04-prd-to-mvp.md` | `40ed962199a45cd6` |
| `contracts/api-contract.md` | `f3b5fdcad1ab1925` |
| `contracts/integration-contract.md` | `6a5d242cdb93725b` |
| `contracts/permission-audit-matrix.md` | `cc3a757dbfc0b7e6` |
| `contracts/state-transition-matrix.md` | `5521110901db1531` |
| `contracts/validation-matrix.md` | `fd900ae54f0fc656` |
| `data/data-dictionary.md` | `5323ad9cd961037e` |
| `flowcharts/00-alur-utama.md` | `b7c949175ef6e65e` |
| `flowcharts/01-usulan-sampai-final.md` | `99c02e83461359e9` |
| `flowcharts/02-pemulihan-aset.md` | `02220799cf30a465` |
| `testing/acceptance-test-matrix.md` | `c6c43752478a3c68` |
