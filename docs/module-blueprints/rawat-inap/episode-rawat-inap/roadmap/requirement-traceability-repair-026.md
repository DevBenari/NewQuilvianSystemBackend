# Traceability Repair 026 — Konfirmasi Masuk pada Modal Denah

| Metadata | Nilai |
| --- | --- |
| blueprint_id / planning_id | `RWI-BP-001` / `RPR-026-DEL-20261017-01` |
| revision / tanggal / status | 1 / 2026-10-17 Asia/Jakarta / IMPLEMENTED_VERIFIED |
| kontrak / lock | `0.13.0` / [repair-026-delivery-lock.json](repair-026-delivery-lock.json) |
| issue / plan | [ISSUE-EPS-026](../docs/issue/issue-026-konfirmasi-masuk-hilang-di-modal-denah.md) / [PLAN-REPAIR-EPS-026](../docs/plan-repair/plan-repair-026-konfirmasi-masuk-hilang-di-modal-denah.md) |
| roadmap | [Frontend Repair 026](frontend-roadmap-repair-026.md) |

---

## 1. Temuan → Perbaikan → Task

| Temuan | Perilaku target | Perbaikan | Backend | Frontend | Status bukti |
| --- | --- | --- | :---: | :---: | --- |
| `ISS-EPS-026-01` | Petugas berhak dapat mengonfirmasi bed `Dipesan` menjadi placement | `FIX-EPS-026-01` | Reuse endpoint existing | `FE-RWI-263` ✅ | TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-263.md)) |
| `ISS-EPS-026-T1` | Petugas berhak dapat membatalkan reservasi aktif dengan alasan wajib | `FIX-EPS-026-01` | Reuse endpoint existing | `FE-RWI-263` ✅ | TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-263.md)) |
| `ISS-EPS-026-T2` | Refresh denah diterjemahkan ke kontrak hook tanpa stale state | `FIX-EPS-026-01` | — | `FE-RWI-263` ✅ | TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-263.md)) |
| `ISS-EPS-026-T3` | Test mencegah capability placement/cancel hilang lagi saat view berubah | `FIX-EPS-026-01` | — | `FE-RWI-263` ✅ | TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-263.md)) |

---

## 2. Acceptance Criteria Index

| AC | Ringkasan | Task | Bukti target |
| --- | --- | :---: | --- |
| `RWI-AC-R026-01` | Tombol placement guarded oleh status, metadata, dan permission Create | `FE-RWI-263` ✅ | TERVERIFIKASI source/unit/UAT pemilik |
| `RWI-AC-R026-02` | Tombol cancel guarded oleh reservation, permission Update, dan alasan wajib | `FE-RWI-263` ✅ | TERVERIFIKASI source/unit/UAT pemilik |
| `RWI-AC-R026-03` | Denied/unknown/redacted aman | `FE-RWI-263` ✅ | TERVERIFIKASI source/unit |
| `RWI-AC-R026-04` | Refresh terbaru dan adapter `rooms[].beds` dipakai sebelum dialog | `FE-RWI-263` ✅ | TERVERIFIKASI source/unit |
| `RWI-AC-R026-05` | Sukses placement memakai versi terbaru, auto-close, toast, dan refresh | `FE-RWI-263` ✅ | TERVERIFIKASI reuse hook/source/UAT pemilik |
| `RWI-AC-R026-06` | 409/422/outcome ambigu mengikuti komponen recovery existing | `FE-RWI-263` ✅ | TERVERIFIKASI source/unit |
| `RWI-AC-R026-07` | Aksi dan mode existing tidak regresi | `FE-RWI-263` ✅ | TERVERIFIKASI unit/build |
| `RWI-AC-R026-08` | Quality gates terdokumentasi | `FE-RWI-263` ✅ | [Laporan task](../task/report/frontend/FE-RWI-263.md) |

---

## 3. Coverage

- Temuan: 4/4 terpetakan ke `FIX-EPS-026-01` dan `FE-RWI-263`.
- Acceptance criteria: 8/8 memiliki task dan bukti target.
- Backend task baru: 0; capability server existing dipakai read-only.
- Coverage gap perencanaan: 0.

