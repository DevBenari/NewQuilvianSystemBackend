# Hemodialisa — Blueprint Manifest

```yaml
blueprint_id: HMD-BP-001
module_name: Hemodialisa
module_slug: hemodialisa
module_prefix: HMD
entity_prefix: Hmd
revision: 1
status: approved
blueprint_shape: SINGLE
shape_decided_by: USER_CONFIRMED        # HMD-DEC-004, 18 September 2026
created_at: 2026-09-18T00:00:00+07:00
updated_at: 2026-09-18T15:40:07+07:00

tahap_saat_ini: SIAP_IMPLEMENTASI_MENUNGGU_REGISTRY
tahap_selesai:
  - scope_pass                       # /qv-grill, 2026-09-18
  - capability_audit                 # /qv-trace, 2026-09-18, revision 2 setelah impact scan
  - requirement_completeness_gate     # HMD-RCG-001 r3, 2026-09-18, READY_FOR_DOMAIN_DESIGN
  - closure_pass                     # /qv-grill, 2026-09-18, menutup DEC-HMD-001 dan DEC-HMD-002
  - impact_scan                      # 2026-09-18, BE 69b256ca→190c91a0, FE 8143874d8→a38683142
  - design_business_module           # 2026-09-18, 14 berkas
  - plan_module_delivery             # 2026-09-18, 3 berkas roadmap
tahap_belum_dijalankan:
  - build_module_backend
  - build_module_frontend

kesiapan_requirement: READY_FOR_DOMAIN_DESIGN
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
domain_architecture_alasan: >
  hospital-domain-architect bersifat opsional dan bukan gerbang wajib. Batas bounded context,
  kepemilikan data, dampak billing, dan batas keselamatan klinis sudah dapat diselesaikan dari
  bukti yang ada — capability map revision 2 memetakan seluruh kelompok data ke modul pemiliknya
  tanpa satu pun UNKNOWN atau CONFLICT yang tersisa.

owners:
  product_domain: Muhammad Hamzah          # HMD-DEC-006
  implementation: Muhammad Hamzah          # HMD-DEC-006
  registry: Muhammad Hamzah                # HMD-FACT-019
  api: Muhammad Hamzah
  frontend: Muhammad Hamzah
  security_privacy: BELUM DITUNJUK
  clinical_governance: BELUM DITUNJUK      # HMD-DEC-010, syarat go-live HMD-GATE-001
  clinical_account_holder: Muhammad Hamzah # HMD-DEC-011, sementara (acting)

approved_by: Muhammad Hamzah               # disetujui pengguna 18 September 2026
approved_at: 2026-09-18T15:40:07+07:00

backend_commit_sha: 190c91a0               # branch MHamzah
frontend_commit_sha: a38683142             # branch HamzahV2
backend_audit_sha: 69b256ca                # SHA audit asli, divalidasi ulang lewat impact scan
frontend_audit_sha: 8143874d8

contract_versions: HMD-CONTRACT-v1         # satu set kontrak, status approved

input_revisions:
  interview_decisions: 12
  existing_capability_map: 2
  requirement_completeness_assessment: 3
input_sources:
  - docs/Modul-RS/Hemodialisa/PRD TO MVP—Hemodialisa Quilvian V2.md      # DRAFT, baseline 896014ec
  - docs/Modul-RS/Hemodialisa/PRD Phase 2 — ... .md                       # konteks saja, di luar scope
  - docs/Modul-RS/Hemodialisa/PRD Phase 3 — ... .md                       # konteks saja, di luar scope

cakupan: Phase 1 saja — 25 Feature MUST HAVE + HMD-CAP-001 = 26 kemampuan   # HMD-DEC-001
slices_ready_for_design: [S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12]
slices_business_decision_required: []

blocker_design: []
blocker_implementation:
  - registry_row_belum_ditambahkan        # HMD-DEC-007 disetujui, 5 tindakan lanjutan belum dikerjakan
gerbang_go_live:
  - HMD-GATE-001                          # badan klinis ditunjuk
  - HMD-GATE-002                          # batas override Pra-HD disahkan, menggantikan HMD-ASM-001
  - HMD-GATE-003                          # kewenangan finalisasi disahkan, menggantikan HMD-ASM-002
  - HMD-GATE-004                          # kewenangan menolak permintaan disahkan, menggantikan HMD-ASM-003
  - HMD-GATE-005                          # pemegang akun dikukuhkan badan klinis
  - HMD-GATE-006                          # orang kedua ditunjuk bila penyusun dilarang mengesahkan

dependency_lintas_modul:
  - id: HMD-DEP-001
    ke: Laboratorium
    kebutuhan: pembacaan hasil per pasien per jenis pemeriksaan
    memblokir: tidak — S3 berjalan dengan pencatatan manual
  - id: HMD-DEP-002
    ke: Human Resource / Credentialing
    kebutuhan: pembacaan kewenangan klinis yang masih berlaku
    memblokir: tidak — HMD-DEC-013 memakai status "Belum dapat diverifikasi"
```

---

## Berkas blueprint

Bentuk `SINGLE`, 14 berkas pasti ditambah satu berkas gerbang requirement.

| Berkas | Ditulis oleh | Status |
|---|---|---|
| `blueprint-manifest.md` | `design-business-module` | `approved` |
| `00-interview-decisions.md` | `grill-me` | `approved` r12 |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | `approved` r2 |
| `02-requirement-completeness-assessment.md` | `requirement-completeness-gate` | `approved` r3 |
| `02-backend-architecture.md` | `design-business-module` | `approved` |
| `03-frontend-architecture.md` | `design-business-module` | `approved` |
| `04-prd-to-mvp.md` | `design-business-module` | `approved` |
| `flowcharts/00-alur-utama.md` | `design-business-module` | `approved` |
| `flowcharts/permintaan-dan-penjadwalan.md` | `design-business-module` | `approved` |
| `flowcharts/pelaksanaan-sesi.md` | `design-business-module` | `approved` |
| `flowcharts/finalisasi-dan-koreksi.md` | `design-business-module` | `approved` |
| `data/data-dictionary.md` | `design-business-module` | `approved` |
| `contracts/api-contract.md` | `design-business-module` | `approved` |
| `contracts/state-transition-matrix.md` | `design-business-module` | `approved` |
| `contracts/validation-matrix.md` | `design-business-module` | `approved` |
| `contracts/integration-contract.md` | `design-business-module` | `approved` |
| `contracts/permission-audit-matrix.md` | `design-business-module` | `approved` |
| `testing/acceptance-test-matrix.md` | `design-business-module` | `approved` |
| `roadmap/backend-roadmap.md` | `plan-module-delivery` | `approved` |
| `roadmap/frontend-roadmap.md` | `plan-module-delivery` | `approved` |
| `roadmap/requirement-traceability.md` | `plan-module-delivery` | `approved` |

`task/report/` menyusul saat implementasi berjalan. Ketiadaannya bukan blueprint yang
belum lengkap.

---

## Keputusan yang mengikat desain

| ID | Isi ringkas |
|---|---|
| `HMD-DEC-001` | Scope Phase 1 saja, 25 Feature |
| `HMD-DEC-002` | Phase 2 dan 3 di-*grill* terpisah |
| `HMD-DEC-004` | Bentuk `SINGLE` |
| `HMD-DEC-007` | Registry `HealthServices / HemodialysisManagement / Hemodialysis / Hmd / ACTIVE` disetujui |
| `HMD-DEC-008` | Ada entity permintaan HD masuk, mengikuti bentuk `LabOrder`/`RadOrder` |
| `HMD-DEC-009` | `DoctorId` = dokter penanggung jawab sesi; `InstructingDoctorId` = dokter pembuat resep |
| `HMD-DEC-010` | Kewenangan klinis `OPEN`, digeser jadi syarat go-live |
| `HMD-DEC-011` | Pemegang akun klinis sementara: Muhammad Hamzah |
| `HMD-DEC-012` | Sesi `Stopped` tidak menagih otomatis |
| `HMD-DEC-013` | Kewenangan petugas ditampilkan dan ditandai, ditegakkan lewat pengaturan |

## Lima syarat yang mengikat desain

Dari `HMD-RCG-001` bagian 7 dan keputusan di atas. Desain yang melanggarnya dihitung belum
memenuhi gerbang.

| # | Syarat | Diwujudkan pada |
|---:|---|---|
| 1 | Daftar item Pra-HD yang boleh dilewati berupa data | `HmdChecklistItem.IsOverridable` |
| 2 | Catatan sesi menyimpan dua pelaku terpisah | `HmdSession.DocumentedByUserId` dan `SignedByUserId` |
| 3 | Rasio perawat dan masa berlaku hasil air berupa pengaturan | `HmdSetting` |
| 4 | Sesi `Stopped` tetap menerbitkan tindakan bertanda tidak dapat ditagih | `HmdSession.StopReason` → `TrxPatientProcedure.IsBillable = false` |
| 5 | Status verifikasi kewenangan bertiga nilai, penegakan berupa pengaturan | `HmdSessionStaffAssignment.CompetencyVerificationStatus` dan `HmdSetting.EnforceCompetencyCheck` |
