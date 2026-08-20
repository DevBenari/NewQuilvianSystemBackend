# Module Blueprint Template

This directory is the canonical contract/template for persistent Quilvian module blueprints. It is not a real module and must not be developed in place.

Create a real blueprint at `docs/module-blueprints/<module-slug>/`. Initialize its architecture once from verified evidence, then revise the same blueprint with its stable identity and revision; do not regenerate it on every request.

Only create conditional artifacts when they materially apply. Blueprint documentation never grants application implementation authority. Create or update blueprint artifacts only in `MODULE BLUEPRINT MODE`, whose write scope is limited to `docs/module-blueprints/**`.

Required starting files are `MODULE-STATUS.md`, `blueprint-manifest.md`, `00-business-overview.md`, `01-prerequisite-readiness.md`, and `02-existing-capability-map.md`.
