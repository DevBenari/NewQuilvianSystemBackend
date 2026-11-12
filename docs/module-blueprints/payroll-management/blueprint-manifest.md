# Payroll Management — Blueprint Manifest

```yaml
blueprint_id: PRL-BP-001
module_name: Payroll Management
module_slug: payroll-management
module_prefix: Prl
module_area: Areas/Corporate/PayrollManagement
module_area_basis: >
  Registry MODULE_OWNERSHIP_PREFIX_REGISTRY.md baris "PayrollManagement / Payroll / Prl" ACTIVE
  sejak 11 November 2026. Master gaji (Mst*) tetap berkas di
  Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit.
revision: 1
status: partial          # diturunkan: payroll-gaji approved, remunerasi-jasa BLOCKED
blueprint_shape: COMPOSITE
shape_decided_by: USER_CONFIRMED   # PRL-DEC-21, menggantikan PRL-DEC-01
submodules:
  - slug: payroll-gaji
    prefix: PAY
    status: approved
    approved_by: Yasmin
    approved_at: 2026-11-12
  - slug: remunerasi-jasa
    prefix: REM
    status: BLOCKED
    blocked_by: "DEC-PRL-011, DEC-PRL-012, DEC-PRL-013, DEC-PRL-014"
owners:
  product_domain: Yasmin — pemilik Payroll, juga pemilik Medical Fee dan Finance
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Product Owner — belum ada UI brief yang disetujui
  cross_module_hr: Pemilik HRD (DEC-PRL-002, 003, 005, 010)
  cross_module_integration: MHamzah — mesin Payroll Modern (DEC-PRL-001)
  cross_module_finance: Finance (DEC-PRL-007, 008)
  directors: Direksi (DEC-PRL-013)
approved_by: null
approved_at: null
backend_commit_sha: 89b9fd69
frontend_commit_sha: bfa3bae69
created_at: 2026-11-12
input_revisions:
  00-interview-decisions.md: 1
  01-existing-capability-map.md: 1
  02-module-map.md: 1
input_hashes:      # sha256, 12 karakter pertama
  00-interview-decisions.md: 87f302b04484
  01-existing-capability-map.md: d122368de5a6
  02-module-map.md: 2877e0320fd1
  evidence/01-permintaan-untuk-mhamzah-mesin-payroll.md: 737e5d8ece3b
legacy_artifacts:
  files: [PAYROLL_V2_01..06, task/report/backend/, task/report/frontend/]
  note: >
    Artefak sebelum blueprint canonical. Pada bentuk COMPOSITE semestinya pindah ke payroll-gaji/;
    sengaja belum dipindah karena dirujuk dengan path absolut oleh laporan dan roadmap
    PAYROLL_V2_04. Usulan: task dokumentasi tersendiri setelah blueprint disetujui.
    Keputusan pemilik.
  doc_correction_pending: "PAYROLL_V2_02 bagian 2 menyatakan batas grade ditegakkan layar HRD; kode tidak (F-11)"
```
