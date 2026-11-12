# Payroll Gaji — Blueprint Manifest (sub-modul)

```yaml
blueprint_id: PRL-BP-001
submodule_slug: payroll-gaji
submodule_prefix: PAY
status: approved
approved_by: Yasmin
approved_at: 2026-11-12
approval_evidence: "Pernyataan pemilik pada sesi 12 November 2026: \"saya setujui payroll dan gaji\""
owners:
  product_domain: Yasmin
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Product Owner — belum ada UI brief yang disetujui
contract_versions:
  current: PGJ-C1
  status: approved
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
domain_architecture_reason: >
  Slice yang didesain hanya memperluas master gaji dan run Payroll yang sudah ada; tidak ada
  bounded context baru, tidak menyentuh billing pasien atau keselamatan klinis.
requirement_readiness_source: "../00-interview-decisions.md bagian 6 (requirement-completeness-gate 12 November 2026)"
designed_slices:
  - EPIC PGJ-01 Tunjangan tetap dari Struktur Gaji
  - EPIC PGJ-02 Batas struktur di dalam grade
  - EPIC PGJ-03 Tunjangan transport bulanan tetap
  - EPIC PGJ-04 Penghasilan variabel manual
  - EPIC PGJ-05 Simulasi
  - EPIC PGJ-06 Laporan beban gaji dan PPh/BPJS
  - EPIC PGJ-07 Lembur baca-saja
open_decision_epics: [PGJ-08, PGJ-09, PGJ-10, PGJ-11, PGJ-12, PGJ-13, PGJ-14, PGJ-15, PGJ-16, PGJ-17]
pending_external_sign_off:   # tidak memblokir pengembangan MVP; wajib sebelum produksi
  - "HRD: PRL-DEC-13 (metode tunjangan), PRL-DEC-42 (lembur baca-saja)"
  - "HRD, Finance, Direksi: DEC-PRL-004 (nominal tunjangan sebagai isi master)"
  - "Pemilik bisnis: DEC-PRL-015 (kapan saklar cadangan profil dimatikan)"
dependencies:
  - "Migration 20261104071700 dan 20261111100000 diterapkan lebih dulu di DB uji"
  - "Build backend hasil PAY-BE-11/PAY-BE-12 dinyatakan hijau oleh pemilik"
artifact_hashes:   # sha256, 12 karakter pertama, dihitung setelah pencatatan approval 12 November 2026
  02-backend-architecture.md: d565a3869c13
  03-frontend-architecture.md: b99cb00e59f8
  04-prd-to-mvp.md: 6f757733e1ce
  contracts/api-contract.md: 811c5af5096f
  contracts/integration-contract.md: efeb64ffd438
  contracts/permission-audit-matrix.md: ee3c0cbd7571
  contracts/state-transition-matrix.md: 3672bbdd86c0
  contracts/validation-matrix.md: 23749ce7de04
  data/data-dictionary.md: f3985ee0f505
  flowcharts/00-alur-utama.md: 80d106313526
  flowcharts/penghasilan-variabel.md: 7fa651654127
  flowcharts/simulasi.md: 9db773593036
  flowcharts/tunjangan-tetap-dan-transport.md: 51d568fadc48
  testing/acceptance-test-matrix.md: c5727ab6ff6c
next_step: "plan-module-delivery untuk EPIC PGJ-01 s.d. PGJ-07"
```

Perubahan isi setelah approval ini membuat revision atau contract version baru dan memicu
impact scan kedua repository.
