# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `3` |
| `status` | `draft` |
| `module` | `igd` |
| `design_snapshot_at` | `2026-08-13` |
| `backend_commit_sha` | `fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| `frontend_commit_sha` | `e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` |
| `owners` | Product/Domain `OPEN`; Registration, Emergency Installation, Clinical, Pharmacy, Finance, Diagnostic Services, Privacy, Security, Integration, and Frontend authorities as defined by the decision log |
| `approved_by` | `—` |
| `approved_at` | `—` |
| `input_revisions` | `00-interview-decisions.md` revision `3`; `01-existing-capability-map.md` revision `1` |
| `input_hashes` | Decisions `sha256:576d5f3da646432d930bc3387512a64ca984f52718d295ab3708cd2b7c15eb8a`; capability map `sha256:09962e71f1480ddd9cdddda5f08e2de59ee9cc1669335a4c5e51d117f390c0f0` |
| `contract_versions` | API `0.1.0-draft`; state `0.1.0-draft`; validation `0.1.0-draft`; integration `0.1.0-draft`; permission/audit `0.1.0-draft` |
| `compatibility_impact` | Breaking replacement is required for the current two-call registration path, frontend encounter-type/status enums, generic clinical mutation endpoints, and broad authorization assumptions. |

## Design gate

This is a target design, not an approved implementation specification. `IGD-DEC-039` through
`IGD-DEC-045` are draft decisions and no formal `GovernanceAssignment` is evidenced. The following
remain production gates: named Diagnostic Services system/owner and its contract, Finance policy
for self-pay `Outstanding + Released`, MMC governance/capability assignments, runtime DI/controller
proof for `Emergency*Service`, and effective SOP configuration. Missing gate means deny the affected
privileged, integration, or financial action; it never blocks emergency clinical care.

## Artifact hashes

| Artifact | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `2bafe22cd63ecd4354eafd21f5c04b75827f124e88d72995950065e9223dc3d7` |
| `03-frontend-architecture.md` | `301a6923adda884565c79ea44167a062c4a310f61c588fe50b246fcc982773ea` |
| `erd/00-context-erd.md` | `1244818324d95e38a0f414a2171643a496af1ac7690b14c8be899df439b89cbf` |
| `erd/emergency-episode.md` | `28e7bc8e680ff3c8a4aaee289d80bc1d2cf69b30a6d7cd5054fa04fedf10bf5d` |
| `erd/data-dictionary.md` | `0a4f966d55b26d350b6be10c1801ecab2d9f866925dcd320d8c3218ff4ce2341` |
| `contracts/api-contract.md` | `311c742ed8c71ec78b1647af9db392964dd0400eeb664ed6b670497ef0ba1fa1` |
| `contracts/state-transition-matrix.md` | `637142defe9eef11b705b6e36a175ef30be76fb7b5f5e167f3b8b24945124909` |
| `contracts/validation-matrix.md` | `62b78ee3e5cd0ede3551c8291540a75d2ad26d3b58a0650dcd1953e6e8e7ff2d` |
| `contracts/integration-contract.md` | `c011b741cb276d5a33b94631c19c2717cf65027da8266a8b7b180de0e632deb9` |
| `contracts/permission-audit-matrix.md` | `81723a68b4b6696d093394cc62e846c83be67c398d0c984fc4e9f1c1e2747e92` |
| `testing/acceptance-test-matrix.md` | `bb5d1262336fff60a2956e3dd821edeb4e759e41404ec9959ff0682a671a9dd6` |

The manifest itself is not self-hashed. Any material change after human approval requires a new
revision and a backend/frontend impact scan under the rules in `01-existing-capability-map.md`.
