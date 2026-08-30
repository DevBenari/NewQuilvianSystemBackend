# Module Blueprint Template

This directory is the canonical contract/template for persistent Quilvian module blueprints. It is not a real module and must not be developed in place.

Create a real blueprint at `docs/module-blueprints/<module-slug>/`. Initialize its architecture once from verified evidence, then revise the same blueprint with its stable identity and revision; do not regenerate it on every request.

Only create conditional artifacts when they materially apply. Blueprint documentation never grants application implementation authority. Create or update blueprint artifacts only in `MODULE BLUEPRINT MODE`, whose write scope is limited to `docs/module-blueprints/**`.

Required starting files are `MODULE-STATUS.md`, `blueprint-manifest.md`, `00-business-overview.md`, `01-prerequisite-readiness.md`, and `02-existing-capability-map.md`.

## Canonical artifact contract

The authoritative list of blueprint artifacts is
`design-business-module/references/blueprint-output-contract.md` in the
`quilvian-engineering-skills` plugin. It defines thirteen files that MUST exist:

```text
docs/module-blueprints/<module>/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── 04-prd-to-mvp.md
├── flowcharts/
│   ├── 00-alur-utama.md
│   └── <proses>.md
├── data/
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

`roadmap/` (from `plan-module-delivery`) and `task/report/**` (from the build skills) join the
same module folder later; they are owned by those skills, not by this contract.

**`erd/` is retired.** Do not create it. Entity relationships live as a Mermaid `classDiagram`
in `02-backend-architecture.md`; table and column structure lives in `data/data-dictionary.md`;
business process flow lives in `flowcharts/**`; status lifecycle lives in
`contracts/state-transition-matrix.md`. See [`erd/README.md`](./erd/README.md) for the
supersession notice and for why existing `erd/` folders in older module blueprints are kept.
