# Sub-modul `pergerakan` — Blueprint Manifest

Sub-modul dari modul [`manajemen-aset`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`AST-DEC-001`. Identitas modul, snapshot SHA, dan registry sub-modul dipegang manifest tingkat modul.
Berkas ini memegang **status desain, `contract_versions`, `artifact_hashes`, approval, dan dependency
sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `pergerakan` |
| `judul` | Mutasi & Peminjaman Aset |
| `blueprint_id` | `AST-BP-001` — satu untuk seluruh modul |
| `revision` | `1` — dipegang tingkat modul |
| `status` | **`approved`** — Rizki, 2026-11-12, `AST-DEC-051` |
| `current_phase` | `AST-PH-004` — implementasi per task (roadmap `REV-1` ditulis); setiap task butuh wewenang implementasi eksplisit |
| `contract_versions` | `AST-MOV-1.0.0` — `approved` 2026-11-12 (api, state-transition, validation, integration, permission-audit, acceptance-test, prd-to-mvp) |
| `tabel baru` | 5 |
| `keputusan desain menunggu persetujuan` | AST-DES-021..026 |
| `epic` | EPIC AST-08, AST-09 |
| `gelombang` | MVP-2 |
| `dependency` | register MVP-1; TS-04 (AST-UNK-004); gerbang opname dari inventarisasi (MVP-3) |
| `prefix` | Entity `Ast` (master `Mst`); task `BE-AST-###` / `FE-AST-###`, deret bersama seluruh modul |
| `input_revision` | decision revision `4`; capability map `1.0`; `AST-RCG-001` rev 1; `02-module-map.md` rev 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` (`AST-RCG-001` bagian 7) |
| `approved_by` | Rizki (Product/Domain Owner Aset dan Akuntansi) |
| `approved_at` | 2026-11-12 |
| `roadmap` | `REV-1` 2026-11-12 — `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`; task BE-AST-016..019, FE-AST-012..014 |
| `updated_at` | 2026-11-12 |

## Artifact hashes

SHA-256 dipotong 16 karakter, dihitung sesudah desain ditulis (2026-11-12).

| Artefak | Hash |
|---|---|
| `02-backend-architecture.md` | `afb29cd0c325f1a3` |
| `03-frontend-architecture.md` | `1e46815fbabea961` |
| `04-prd-to-mvp.md` | `1b3ae899a8d3ed47` |
| `contracts/api-contract.md` | `4582c8e884ff9538` |
| `contracts/integration-contract.md` | `a3726b0e16a93f2d` |
| `contracts/permission-audit-matrix.md` | `4261f80264121871` |
| `contracts/state-transition-matrix.md` | `e6564b4cbab49b86` |
| `contracts/validation-matrix.md` | `8a1db87999028226` |
| `data/data-dictionary.md` | `d5f2d3e1030db6b4` |
| `flowcharts/00-alur-utama.md` | `8655cc2ac87ac986` |
| `flowcharts/01-mutasi.md` | `25c95c675324d4af` |
| `flowcharts/02-peminjaman.md` | `c68a6ca0feebe473` |
| `testing/acceptance-test-matrix.md` | `6618481fcd3364d1` |
