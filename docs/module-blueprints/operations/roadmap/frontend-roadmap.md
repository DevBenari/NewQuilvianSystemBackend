# Frontend Roadmap — Modul Operasi

Semua task frontend memakai `opr-api-v1` dan tidak boleh mengubah kontrak backend. Layout detail adalah `DEV_DISCRETION` mengikuti design system existing.

| Task ID | Outcome | Trace | Dependency | Acceptance/DoD |
|---|---|---|---|---|
| `FE-OPR-001` | Shell Modul Operasi dan daftar/detail kasus | `OPS-REQ-001/002` | `BLOCKED BY BE-OPR-003` | loading/empty/error/retry/stale, permission, paging, tanpa data klinis di storage/log |
| `FE-OPR-002` | Workspace jadwal ruang dan tim | `OPS-REQ-003/004` | `BLOCKED BY BE-OPR-004` | Konflik `409` terbaca, histori jadwal terlihat, duplicate submit dicegah |
| `FE-OPR-003` | Workspace persiapan/checklist | `OPS-REQ-005` | `BLOCKED BY BE-OPR-005` | Fase/sign-off/bypass sesuai availableActions; error prasyarat jelas |
| `FE-OPR-004` | Workspace pelaksanaan dan addendum | `OPS-REQ-006/009` | `BLOCKED BY BE-OPR-006` | Final read-only, addendum terpisah, status `StoppedEarly`, akses dokter benar |
| `FE-OPR-005` | Workspace anestesi, recovery, handover | `OPS-REQ-006/008` | `BLOCKED BY BE-OPR-007` | Recovery/handover tidak disamakan; Completed hanya dari response backend |
| `FE-OPR-006` | Pencatatan material/implant | `OPS-REQ-007` | `BLOCKED BY BE-OPR-008` | Batch/serial/quantity/outcome validation; pending integration terlihat |
| `FE-OPR-007` | Monitoring integrasi | `OPS-REQ-007/010` | `BLOCKED BY BE-OPR-009` | Status delivery/retry sesuai permission; tidak menampilkan payload sensitif |
| `FE-OPR-008` | Laporan Operasi | `OPS-REQ-011` | `BLOCKED BY BE-OPR-010` | Filter/paging/empty/error/export bila backend mendukung; responsive/a11y |
| `FE-OPR-009` | E2E dan hardening frontend | Semua | `BLOCKED BY BE-OPR-011` dan FE sebelumnya | E2E normal/darurat/cancel/stale/403/409; tidak memakai `dataOperasi.jsx` sebagai source |

Setiap task frontend memerlukan `TASK MODE: FRONTEND` dan backend dibaca sebagai source of truth.
