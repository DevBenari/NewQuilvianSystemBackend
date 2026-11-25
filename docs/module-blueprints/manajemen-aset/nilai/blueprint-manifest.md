# Sub-modul `nilai` — Blueprint Manifest

Sub-modul dari modul [`manajemen-aset`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`AST-DEC-001`. Identitas modul, snapshot SHA, dan registry sub-modul dipegang manifest tingkat modul.
Berkas ini memegang **status desain, `contract_versions`, `artifact_hashes`, approval, dan dependency
sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `nilai` |
| `judul` | Nilai Aset & Rekonsiliasi |
| `blueprint_id` | `AST-BP-001` — satu untuk seluruh modul |
| `revision` | `1` — dipegang tingkat modul |
| `status` | **`approved`** — Rizki, 2026-11-12, `AST-DEC-051` |
| `current_phase` | `AST-PH-004` — implementasi per task (roadmap `REV-1` ditulis); setiap task butuh wewenang implementasi eksplisit |
| `contract_versions` | `AST-NLI-1.0.0` — `approved` 2026-11-12 (api, state-transition, validation, integration, permission-audit, acceptance-test, prd-to-mvp) |
| `tabel baru` | 10 |
| `keputusan desain menunggu persetujuan` | AST-DES-009..012, 032..036 |
| `epic` | EPIC AST-11..14 (AST-15 Revaluasi OPEN DECISION) |
| `gelombang` | MVP-0 (tabel dasar + AST-11), MVP-1 (hibah), MVP-4 |
| `dependency` | DEP-AST-01..04 (Akuntansi) untuk penyalaan kiriman; AST-UNK-002 |
| `prefix` | Entity `Ast` (master `Mst`); task `BE-AST-###` / `FE-AST-###`, deret bersama seluruh modul |
| `input_revision` | decision revision `4`; capability map `1.0`; `AST-RCG-001` rev 1; `02-module-map.md` rev 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` (`AST-RCG-001` bagian 7) |
| `approved_by` | Rizki (Product/Domain Owner Aset dan Akuntansi) |
| `approved_at` | 2026-11-12 |
| `roadmap` | `REV-1` 2026-11-12 — `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`; task BE-AST-014, 015, 025..032 (032 ⛔ GATE-AST-ACC), FE-AST-011, 019..023 |
| `updated_at` | 2026-11-12 |

## Artifact hashes

SHA-256 dipotong 16 karakter, dihitung sesudah desain ditulis (2026-11-12).

| Artefak | Hash |
|---|---|
| `02-backend-architecture.md` | `bb795b41518d65f7` |
| `03-frontend-architecture.md` | `22a07a7d61bc3e5d` |
| `04-prd-to-mvp.md` | `647adb9d5fdba6d8` |
| `contracts/api-contract.md` | `efed059fb4bb3597` |
| `contracts/integration-contract.md` | `7cee4b8265eaa095` |
| `contracts/permission-audit-matrix.md` | `1d350fd86a29a8e4` |
| `contracts/state-transition-matrix.md` | `e5e332f1d77929f1` |
| `contracts/validation-matrix.md` | `51a146e899fdad27` |
| `data/data-dictionary.md` | `536f9de88324e0d7` |
| `flowcharts/00-alur-utama.md` | `28f80494973e6c76` |
| `flowcharts/01-proses-penyusutan.md` | `c12553c2ba1868da` |
| `flowcharts/02-perubahan-nilai.md` | `c214dda7efa0d1e0` |
| `testing/acceptance-test-matrix.md` | `d5555348df3b65b1` |
