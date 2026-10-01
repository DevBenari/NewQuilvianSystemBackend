# Kiosk — Blueprint Manifest

```yaml
blueprint_id: KSK-BP-001
module_name: Kiosk (Revisi Module Kiosk & Cek Nomor Rekam Medis)
module_slug: kiosk
module_prefix: KSK
entity_prefix: Reg                   # RegistrationManagement / Registration — MODULE_OWNERSHIP_PREFIX_REGISTRY.md baris 23
revision: 1
status: approved                     # disetujui Sukma Giri Pratama 30 Sep 2026
blueprint_shape: SINGLE
shape_decided_by: USER_CONFIRMED     # KSK-DEC-004, 30 Sep 2026
created_at: 2026-09-30T00:00:00+07:00
updated_at: 2026-09-30T16:00:00+07:00
last_verified_at: null

tahap_saat_ini: KSK-PH-003           # roadmap approved — build BE-KSK-001
tahap_selesai:
  - intake                           # PRD Tim Bisnis 30 Sep 2026 dicatat sebagai bukti input
  - audit_awal                       # audit read-only singkat, belum capability map resmi
  - scope_pass                       # grill-me, 30 Sep 2026 — KSK-DEC-001..012
  - capability_audit                 # trace-existing-capabilities, 30 Sep 2026 — 01-existing-capability-map.md r1
  - closure_pass                     # grill-me, 30 Sep 2026 — KSK-DEC-013..019
  - requirement_completeness_gate    # KSK-RCG-001 r1, 30 Sep 2026 — READY_FOR_DOMAIN_DESIGN (9/9 slice)
  - design_business_module           # 30 Sep 2026 — 14 berkas + flowchart proses, KSK-CONTRACT-v1
  - approval_blueprint               # Sukma Giri Pratama, 30 Sep 2026 (termasuk KSK-DSN-001..009, KSK-GAP-001..011)
  - plan_module_delivery             # 30 Sep 2026 — KSK-RM-BE-001 r1 (3 task), KSK-RM-FE-001 r1 (8 task), KSK-TRACE-001 r1
tahap_belum_dijalankan:
  - build_module_backend
  - build_module_frontend

kesiapan_requirement: READY_FOR_DOMAIN_DESIGN
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # opsional; alasan di KSK-RCG-001 §9

owners:
  product_domain: Sukma Giri Pratama # atas nama Tim Bisnis — KSK-DEC-005
  implementation: Sukma Giri Pratama # sukmagp
  security_privacy: BELUM DITUNJUK
  approver_blueprint: Sukma Giri Pratama   # KSK-DEC-005

backend_source_sha: 419b910fca5188285946850a95976ebff83ae8ce   # branch sukmagp
frontend_source_sha: 4ec51b0bf5e724e899b95f118e273351437b71e7

skill_suite_version: 1.18.0
input_revision_hash: KSK-PRD-2026-09-30-r1
decision_revision: 2                 # 00-interview-decisions.md r2
contract_versions: KSK-CONTRACT-v1     # approved — api, state-transition, validation, integration, permission-audit
active_dependency_ids: [KSK-OQ-004, KSK-OQ-005]
approved_by: Sukma Giri Pratama
approved_at: 2026-09-30
active_roadmap_revision: KSK-RM-BE-001 r1 / KSK-RM-FE-001 r1 (approved Sukma 2026-09-30)
supersedes: null

input_revisions:
  interview_decisions: 2
  existing_capability_map: 1
  requirement_completeness_assessment: 1   # KSK-RCG-001

artifact_hashes:                     # sha256, 12 karakter pertama — dihitung ulang setelah approval & roadmap
  00-interview-decisions.md: 186c4182779a
  01-existing-capability-map.md: 0b28195886ca
  02-requirement-completeness-assessment.md: f85c26e4fc76
  02-backend-architecture.md: 6a4c89bda80a
  03-frontend-architecture.md: 7b3ac2d05c4f
  04-prd-to-mvp.md: 757640f112aa
  data/data-dictionary.md: 3b2b251c3d1a
  contracts/api-contract.md: 9642e4257658
  contracts/integration-contract.md: 369bf2ef9a96
  contracts/permission-audit-matrix.md: 3340e791c350
  contracts/state-transition-matrix.md: d733049b6d49
  contracts/validation-matrix.md: c142511582b9
  flowcharts/00-alur-utama.md: 369814a6b192
  flowcharts/cek-nomor-rekam-medis.md: 0ecbbdcf9f30
  flowcharts/pendaftaran-pasien-lama.md: a74474511656
  flowcharts/penjamin-utama.md: a18ff2a78dc2
  testing/acceptance-test-matrix.md: f7df3f3fa9c6
  roadmap/backend-roadmap.md: ac54a31226e7
  roadmap/frontend-roadmap.md: 4d3e936e5a93
  roadmap/requirement-traceability.md: 6b6ed116f838
```

## Bukti input

- [evidence/2026-09-30-prd-revisi-kiosk.md](evidence/2026-09-30-prd-revisi-kiosk.md) — register requirement PRD Tim Bisnis beserta temuan audit awal.
