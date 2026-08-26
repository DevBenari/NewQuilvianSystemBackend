# Pharmacy Blueprint Manifest

```yaml
blueprint_id: PHA-BP-001
module_name: Farmasi
module_slug: pharmacy
module_prefix: PHA
revision: 3
status: approved
current_phase: PHA-PH-008
created_at: 2026-08-18T00:00:00+07:00
updated_at: 2026-08-21T00:15:00+07:00
last_verified_at: 2026-08-20T14:36:54+07:00
backend_source_sha: 767470f742bc6f2eebadbd653a873f69d6f93121
frontend_source_sha: 400104f2a0f3239c14c40f5905b419977a538450
skill_suite_version: 1.0.0-rc2
input_revision_hash: pharmacy-decisions-r2+capability-audit-2026-08-20
decision_revision: 2
owners:
  product_domain: user
  api: Pharmacy Backend owner
  security: pending formal approval
  frontend_authority: pending formal approval
approved_by: product/domain owner (user)
approved_at: 2026-08-21
backend_commit_sha: 767470f742bc6f2eebadbd653a873f69d6f93121
frontend_commit_sha: 400104f2a0f3239c14c40f5905b419977a538450
contract_versions:
  - PHA-DEPOT-ROUTING-v1: approved
  - PHA-API-ROUTING-v1: approved
  - PHA-STATE-ROUTING-v1: approved
  - PHA-VAL-ROUTING-v1: approved
  - PHA-INT-ROUTING-v1: approved
  - PHA-PERM-ROUTING-v1: approved
active_dependency_ids:
  - PHA-DEP-001
  - PHA-DEP-002
  - PHA-DEP-003
active_roadmap_revision: PHA-RM-001-r1-draft
domain_architecture_revision: PHA-DA-001-r1
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY_FOR_ROUTING_DEPOT
input_revisions:
  decisions: 2
  requirement_gate: PHA-RCG-001
  domain_architecture: PHA-DA-001-r1
input_hashes:
  decisions_and_capability: pharmacy-decisions-r2+capability-audit-2026-08-20
artifact_hashes:
  00-interview-decisions.md: 18c87934f166efec0ff406b4924446f20aaaf5f282ca8ac10e2caf49f61bed50
  01-existing-capability-map.md: b7d61a63d15fd8c5a670337df303a064730534548f206ca48bf4bab8b1a41f85
  02-backend-architecture.md: 5690ee4b67ae0cc621b792875b833137c9b24f495dd315b5c08a203c6e96e4aa
  03-frontend-architecture.md: f5ca61cf00ab1b65ed7acd62a29107ab3ca8e6d4430909410902b44e4fc16194
  erd/00-context-erd.md: a3951a28ac86c2ea245e4e086306c252503cead35cf60b1a5c050587d7bb6276
  erd/pharmacy-routing.md: 2799d099e08e59e3f38c7c7d81a6e53c2973a8e00d4718aa8138cdc0f267a66c
  erd/data-dictionary.md: 4961346845ba5d33d707c2eca1a7fde8d5434e6569378ae6e391e973ec773203
  contracts/api-contract.md: 7f883462bdc8f319b4fb62d2bae561a45ff6821f2d2547096e9ff964629bd78d
  contracts/state-transition-matrix.md: e62fcd83aca2e1516c5fcae8b13e6b5beb302564b4c7150b551523d32d5bc1f1
  contracts/validation-matrix.md: 19818f000a8ea31fd9a239c8c3285675a4718400af7f00c64299dc21c9b2e088
  contracts/integration-contract.md: baa888eead9eaa8a8112e2639a1be09f3c1f218f3f790ec5eeba3c3dbf64fec7
  contracts/permission-audit-matrix.md: 00c9a0f752b3388a191c01382c83c53cb2c87f408a0a22acd7c0f1e153ef2cea
  testing/acceptance-test-matrix.md: e89f149f436c62c2f9f5093267a5d13e3db06c8367982e5663887b365551c46f
  roadmap/README.md: 11ccb89808393c6a2c543f69ff84570b5a5b387df76757bf96bda13150f14ef6
supersedes: null
```
