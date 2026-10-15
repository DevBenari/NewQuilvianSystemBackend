# Frontend Roadmap — Modul Operasi

Semua task frontend memakai `opr-api-v1` dan tidak boleh mengubah kontrak backend. Layout detail adalah `DEV_DISCRETION` mengikuti design system existing.

> **Status dimutakhirkan 15 Oktober 2026.** Kolom `Dependency` lama menandai seluruh task
> `BLOCKED BY BE-OPR-00x`; kesebelas task backend itu kini **Selesai**, sehingga penandaan itu
> tidak lagi berlaku dan diganti kolom Status beserta bukti acceptance-nya. Kriteria acceptance
> aslinya tetap utuh di `testing/fe-acceptance-evidence.md`.

| Task ID | Outcome | Trace | Status | Bukti acceptance |
|---|---|---|---|---|
| `FE-OPR-001` | Shell Modul Operasi dan daftar/detail kasus | `OPS-REQ-001/002` | **SELESAI** | `operating-room-screen-acceptance` AC-1..4; `operating-room-reject-case` (4 AC); `operating-room-hardening` (10) |
| `FE-OPR-002` | Workspace jadwal ruang dan tim | `OPS-REQ-003/004` | **SELESAI** | `operating-room-screen-acceptance`: `OPR003`, Riwayat Jadwal, `disabled={submitting}` |
| `FE-OPR-003` | Workspace persiapan/checklist | `OPS-REQ-005` | **SELESAI** | `operating-room-screen-acceptance`; `ward-pre-op` (3 AC) |
| `FE-OPR-004` | Workspace pelaksanaan dan addendum | `OPS-REQ-006/009` | **SELESAI** | `operating-room-execution-addendum` (7 AC) |
| `FE-OPR-005` | Workspace anestesi, recovery, handover | `OPS-REQ-006/008` | **SELESAI** | `operating-room-screen-acceptance`; `post-op-summary` (5 AC) |
| `FE-OPR-006` | Pencatatan material/implant | `OPS-REQ-007` | **SELESAI** | `operating-room-screen-acceptance`: batch/serial wajib untuk implan |
| `FE-OPR-007` | Monitoring integrasi | `OPS-REQ-007/010` | **SELESAI** | `operating-room-screen-acceptance`: payload tidak ditampilkan |
| `FE-OPR-008` | Laporan Operasi | `OPS-REQ-011` | **SELESAI** | `operating-room-screen-acceptance`: filter, paging, kosong, galat |
| `FE-OPR-009` | E2E dan hardening frontend | Semua | **SEDANG PROSES** | hardening 10 lulus; E2E Playwright tertahan bootstrap sesi — `testing/fe-acceptance-evidence.md` |

Setiap task frontend memerlukan `TASK MODE: FRONTEND` dan backend dibaca sebagai source of truth.
