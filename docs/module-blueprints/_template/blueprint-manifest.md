# Blueprint Manifest Contract

```yaml
blueprint_id: <PREFIX>-BP-001
module_name: <human-readable module name>
module_slug: <kebab-case-module-name>
module_prefix: <UPPERCASE 2-12 character prefix>
revision: 1
status: DRAFT
current_phase: <PREFIX>-PH-001
created_at: <ISO-8601 timestamp>
updated_at: <ISO-8601 timestamp>
last_verified_at: null
backend_source_sha: <Git SHA>
frontend_source_sha: <Git SHA>
skill_suite_version: 1.0.0-rc2
input_revision_hash: <hash or revision of decision/evidence inputs>
decision_revision: <decision artifact revision>
contract_versions: []
active_dependency_ids: []
active_roadmap_revision: null
supersedes: null
```

`blueprint_id`, `module_slug`, and `module_prefix` are assigned once. `input_revision_hash` identifies the decision/evidence basis of the revision. `contract_versions` identifies active approved or draft contracts without replacing their artifact-level version. `supersedes` is null unless this blueprint replaces another blueprint.

Increase `revision` only for a material target architecture, contract, dependency, or approved-decision change. Do not increase it for status-only changes, evidence references that do not change the target, or marking an already defined task done. Every update changes `updated_at`; verification changes `last_verified_at`.

When a recorded source SHA changes, mark dependent evidence/artifacts `STALE` and complete a scoped impact review before using them as current.
