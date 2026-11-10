# Rehab Medik — Blueprint Manifest

| Field | Nilai |
|---|---|
| `blueprint_id` | `RHB-BP-001` |
| `revision` | `1` |
| `status` | `approved` — disetujui owner pada sesi 2026-11-05 (instruksi "lanjutkan" setelah review desain r1). Keputusan klinis tetap butuh sign-off pengesah klinis sebelum aktivasi produksi (RHB-DEC-002). |
| `blueprint_shape` | `SINGLE` |
| `shape_decided_by` | `USER_CONFIRMED` (RHB-DEC-003) |
| `owners` | Product/domain: Sukma Giri Pratama · API/security: Sukma Giri Pratama · Frontend authority: Sukma Giri Pratama (RHB-FE-01..04) · Pengesah klinis: **belum ditunjuk** (RHB-OQ-001) |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-05 |
| `backend_commit_sha` | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) |
| `frontend_commit_sha` | `0c06c09305e6ee9c4f21e85393ebc7de10576550` (`sukmagpV2`) |
| `contract_versions` | `v1` (approved 2026-11-05) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN`. Alasannya ada di `02-requirement-completeness-assessment.md`. |
| `requirement_readiness` | Semua slice S1–S8 `READY_FOR_DOMAIN_DESIGN` |
| Registry | `HealthServices / RehabilitationManagement / Rhb / ACTIVE` (2026-11-05, RHB-DEC-014) |

## Input hulu

| Input | Revision | Hash (sha256, 16 karakter) |
|---|---|---|
| `rehab-medik-deep-analysis.md` | 2026-09-16 | `4857e5c48cdb134c` |
| `PRD_Rehab_Medik_Quilvian_V2.md` | v1.0, 2026-11-05 | `3971dbd2d47549f3` |

## Artifact hashes (snapshot penulisan revision 1)

| Artefak | Hash |
|---|---|
| `00-interview-decisions.md` | `dc867be6e5475ada` |
| `01-existing-capability-map.md` | `d3f1d3de5271b163` |
| `02-requirement-completeness-assessment.md` | `2c25d3ad5b87e3ce` |
| `02-backend-architecture.md` | `c3327308123a92cf` |
| `03-frontend-architecture.md` | `5d6bf75827d9a91b` |
| `04-prd-to-mvp.md` | `0cef22bdc0911c74` |
| `contracts/api-contract.md` | `95f48c69295eaea9` |
| `contracts/integration-contract.md` | `87a1f7734182745d` |
| `contracts/permission-audit-matrix.md` | `3a6f57d92160adfa` |
| `contracts/state-transition-matrix.md` | `1d34c130a61c3e65` |
| `contracts/validation-matrix.md` | `0f08280709f88ec5` |
| `data/data-dictionary.md` | `3a80f4b50d74bd72` |
| `flowcharts/00-alur-utama.md` | `ce741a3fc12e9feb` |
| `flowcharts/permintaan-dan-disposisi.md` | `3c783563bf7c58a6` |
| `flowcharts/sesi-terapi.md` | `9b088f57c2e445f4` |
| `flowcharts/penutupan-episode.md` | `d869fe4e3a3f25a4` |
| `testing/acceptance-test-matrix.md` | `7b010153a31afd3f` |

## Dependency lintas modul yang harus disetujui sebelum task terkait ✅

| ID | Modul | Kebutuhan | Status |
|---|---|---|---|
| RHB-OQ-012 | MedicalRecordManagement | Jenis dokumen 47–52 + `JenisYangDitegakkan` | Disetujui. Bukti: konfirmasi owner 2026-11-06, [`evidence/RHB-OQ-012-persetujuan-rekam-medis.md`](evidence/RHB-OQ-012-persetujuan-rekam-medis.md). |
| RHB-DEC-018 | ClinicalManagement | `ClinicalInstrumentKind` 8–13 | Disetujui menurut owner |
| RHB-OQ-015 | ClinicalManagement / Billing | `TrxPatientProcedure`, `PatientProcedureSource.RehabilitationSession`, fakta charge | Disetujui. Bukti: konfirmasi owner 2026-11-09, [`evidence/RHB-OQ-015-persetujuan-clinical-billing.md`](evidence/RHB-OQ-015-persetujuan-clinical-billing.md). `DoctorId` = dokter PJ episode; sumber = `RehabilitationSession = 5`. |

Perubahan setelah approval wajib menaikkan `revision`/`contract_versions` dan memicu impact scan
kedua repository.
