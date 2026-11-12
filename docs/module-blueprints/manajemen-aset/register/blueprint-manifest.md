# Sub-modul `register` — Blueprint Manifest

Sub-modul dari modul [`manajemen-aset`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`AST-DEC-001`. Identitas modul, snapshot SHA, dan registry sub-modul dipegang manifest tingkat modul.
Berkas ini memegang **status desain, `contract_versions`, `artifact_hashes`, approval, dan dependency
sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `register` |
| `judul` | Register Aset |
| `blueprint_id` | `AST-BP-001` — satu untuk seluruh modul |
| `revision` | `1` — dipegang tingkat modul |
| `status` | **`approved`** — Rizki, 2026-11-12, `AST-DEC-051` |
| `current_phase` | `AST-PH-004` — implementasi per task (roadmap `REV-1` ditulis); setiap task butuh wewenang implementasi eksplisit |
| `contract_versions` | `AST-REG-1.0.0` — `approved` 2026-11-12 (api, state-transition, validation, integration, permission-audit, acceptance-test, prd-to-mvp) |
| `tabel baru` | 20 |
| `keputusan desain menunggu persetujuan` | AST-DES-001..006, 014..018, 020 |
| `epic` | EPIC AST-01..07 |
| `gelombang` | MVP-0 (AST-01), MVP-1 |
| `dependency` | Registry Ast (task pertama); TS-01, TS-03, TS-04, TS-06, TS-07, TS-12; nilai tabel dasar + EPIC AST-11 |
| `prefix` | Entity `Ast` (master `Mst`); task `BE-AST-###` / `FE-AST-###`, deret bersama seluruh modul |
| `input_revision` | decision revision `4`; capability map `1.0`; `AST-RCG-001` rev 1; `02-module-map.md` rev 1 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` (`AST-RCG-001` bagian 7) |
| `approved_by` | Rizki (Product/Domain Owner Aset dan Akuntansi) |
| `approved_at` | 2026-11-12 |
| `roadmap` | `REV-1` 2026-11-12 — `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`; task BE-AST-001..013, FE-AST-001..010 |
| `updated_at` | 2026-11-12 |

## Artifact hashes

SHA-256 dipotong 16 karakter, dihitung sesudah desain ditulis (2026-11-12).

| Artefak | Hash |
|---|---|
| `02-backend-architecture.md` | `a1c45e99b09d649b` |
| `03-frontend-architecture.md` | `227c7deff3705b06` |
| `04-prd-to-mvp.md` | `5ee37abc5f1f96db` |
| `contracts/api-contract.md` | `16bb5022fc051872` |
| `contracts/integration-contract.md` | `5ac3daa53cf1090b` |
| `contracts/permission-audit-matrix.md` | `b93dab52e5f7923c` |
| `contracts/state-transition-matrix.md` | `fab02591765931b7` |
| `contracts/validation-matrix.md` | `334bb0e53f601a23` |
| `data/data-dictionary.md` | `dc372e8de791326a` |
| `flowcharts/00-alur-utama.md` | `6e63a9e3db283094` |
| `flowcharts/01-pencatatan-dan-penomoran.md` | `c4f1b52ece5b655d` |
| `flowcharts/02-penempatan-dan-penyerahan.md` | `26b678b41e00504a` |
| `flowcharts/03-laporan-kondisi.md` | `b1b5d816e23863fa` |
| `flowcharts/04-pemanfaatan.md` | `7098847910197854` |
| `flowcharts/05-impor-data-awal.md` | `e9c5964af7985905` |
| `testing/acceptance-test-matrix.md` | `23f3072deee5329e` |
