# Billing dan Kasir — Blueprint Manifest

```yaml
blueprint_id: BIL-CASH-001
module_name: Billing dan Kasir
module_slug: billing-kasir
module_prefix: BIL
revision: 0.3
status: PARTIAL
readiness: PARTIAL
current_phase: BIL-CASH-PH-005-DESIGN-APPROVAL
created_at: 2026-08-20T11:22:42+07:00
updated_at: 2026-08-20T12:48:15+07:00
last_verified_at: 2026-08-20T12:48:15+07:00
backend_source_sha: e6f6ecba1537783ea2eb379ac12cc97790707303
frontend_source_sha: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
skill_suite_version: not-recorded
input_revision_hash: dfa3541bee4943660a9aac51dfb2e0e254ea527c84fb4299a935c870523c8c12
decision_revision: 0.2
requirement_gate_revision: 0.3
domain_architecture_revision: 0.3
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
contract_versions:
  - BIL-CORE-0.3-draft
  - BIL-SETTLEMENT-0.3-draft
  - BIL-EXCEPTION-0.3-draft
  - BIL-FINALIZATION-0.3-draft
  - BIL-CASHIER-0.3-draft
  - BIL-FE-BEHAVIOR-0.3-draft
active_dependency_ids:
  - BIL-APR-001
active_roadmap_revision: null
supersedes: null
```

## Artifact register

| Artifact | Revision | Status | Input/trace |
| --- | --- | --- | --- |
| [`00-interview-decisions.md`](./00-interview-decisions.md) | `0.2` | `approved` decision contract | `BKC-DEC-001`–`044` approved |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | `0.2` | `source-audited` | Current V2 + scoped SHA impact scan + legacy evidence |
| [`02-requirement-completeness-gate.md`](./02-requirement-completeness-gate.md) | `0.3` | `READY_FOR_DOMAIN_DESIGN` | Seluruh 18 dimensions reassessed; no blocking decisions |
| [`03-hospital-domain-architecture.md`](./03-hospital-domain-architecture.md) | `0.3` | `DOMAIN_ARCHITECTURE_READY` | Seluruh approved decisions integrated |
| [`04-business-module-blueprint.md`](./04-business-module-blueprint.md) | `0.3-draft` | `DRAFT_COMPLETE_BLUEPRINT` | Human design approval required |
| [`05-servicebilling-attachment-evidence.md`](./05-servicebilling-attachment-evidence.md) | `0.1` | `LEGACY_REFERENCE_EVIDENCE` | ZIP SHA-256 `2b948721cee4154eaecaf9ac57d7621fb34cb7b61fb31a5fd6dff04df7ad218d` |
| [`MODULE-STATUS.md`](./MODULE-STATUS.md) | `0.3` | `PARTIAL` | Requirement-gate reassessment aktif |

Perubahan material pada keputusan, ownership, lifecycle, integration boundary, atau ready scope
harus menaikkan revision/contract version dan memicu impact review. Perubahan source SHA memicu
staleness check sebelum blueprint dipakai untuk delivery planning.
