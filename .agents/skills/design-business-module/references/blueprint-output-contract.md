# Blueprint Output Contract

Gunakan struktur minimum berikut; hilangkan artefak yang tidak relevan daripada membuat
file kosong.

```text
docs/module-blueprints/<module>/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── erd/
│   ├── 00-context-erd.md
│   ├── <bounded-context>.md
│   └── data-dictionary.md
├── contracts/
│   ├── api-contract.md
│   ├── state-transition-matrix.md
│   ├── validation-matrix.md
│   ├── integration-contract.md
│   └── permission-audit-matrix.md
└── testing/
    └── acceptance-test-matrix.md
```

Manifest minimum:

| Field | Requirement |
|---|---|
| `blueprint_id` | Stable across revisions |
| `revision` | Increment on material design change |
| `status` | `draft`, `approved`, `superseded` |
| `owners` | Product/domain/API/security/frontend authorities |
| `approved_by`, `approved_at` | Human approval evidence |
| `backend_commit_sha`, `frontend_commit_sha` | Discovery/design snapshots |
| `contract_versions` | API, integration, state contract versions |
| `artifact_hashes` | Drift detection |
| `input_revisions`, `input_hashes` | Upstream lineage |

Setiap architecture/contract menyebut requirement ID, decision ID, owner, exception path,
security/privacy impact, dan acceptance test yang membuktikannya.
