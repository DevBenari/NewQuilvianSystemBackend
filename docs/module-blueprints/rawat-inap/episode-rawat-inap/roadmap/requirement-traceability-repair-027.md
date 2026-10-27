# Traceability Repair 027 — Aksi Pembersihan pada Denah Mode Pilih

| Metadata | Nilai |
| --- | --- |
| blueprint_id / planning_id | `RWI-BP-001` / `RPR-027-DEL-20261017-01` |
| revision / tanggal / status | 1 / 2026-10-17 Asia/Jakarta / IMPLEMENTED_BUILD_PASS_UAT_PENDING |
| kontrak / lock | `0.13.0` + amandemen `PLAN-REPAIR-EPS-027` / [repair-027-delivery-lock.json](repair-027-delivery-lock.json) |
| issue / plan | [ISSUE-EPS-027](../docs/issue/issue-027-aksi-pembersihan-hilang-mode-pilih-bed.md) / [PLAN-REPAIR-EPS-027](../docs/plan-repair/plan-repair-027-aksi-pembersihan-hilang-mode-pilih-bed.md) |
| roadmap | [Frontend Repair 027](frontend-roadmap-repair-027.md) |

---

## 1. Temuan → Perbaikan → Task

| Temuan | Perilaku target | Perbaikan | Backend | Frontend | Status bukti |
| --- | --- | --- | :---: | :---: | --- |
| `ISS-EPS-027-01` | Pembersihan dapat dibuka dari mode Pilih hanya melalui aksi server dan permission yang sah | `FIX-EPS-027-01`, `FIX-EPS-027-02` | Reuse endpoint existing | `FE-RWI-264` ✅ | SOURCE + BUILD TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-264.md)) |
| `ISS-EPS-027-T1` | Setelah tindakan, denah serta eligibility dimuat ulang dan pilihan stale dilepas | `FIX-EPS-027-02` | — | `FE-RWI-264` ✅ | SOURCE + BUILD TERVERIFIKASI ([laporan](../task/report/frontend/FE-RWI-264.md)) |

---

## 2. Acceptance Criteria Index

| AC | Ringkasan | Task | Bukti target |
| --- | --- | --- | --- |
| `RWI-AC-R027-01` | Aksi server + permission allowed menampilkan Pembersihan pada mode Pilih | `FE-RWI-264` ✅ | Source terbukti; build lulus |
| `RWI-AC-R027-02` | Aksi kosong/denied/unknown fail closed | `FE-RWI-264` ✅ | Source terbukti; build lulus |
| `RWI-AC-R027-03` | Detail pembersihan existing dan fresh snapshot dipakai ulang | `FE-RWI-264` ✅ | Source terbukti; build lulus |
| `RWI-AC-R027-04` | Refresh state efektif menunggu denah dan overlay transfer | `FE-RWI-264` ✅ | Source terbukti; build lulus |
| `RWI-AC-R027-05` | Pilihan stale dibersihkan; bed belum eligible tidak dapat dipilih | `FE-RWI-264` ✅ | Source terbukti; build lulus |
| `RWI-AC-R027-06` | Error/version/cycle guard existing dipertahankan | `FE-RWI-264` ✅ | Reuse hook/source terbukti |
| `RWI-AC-R027-07` | Mode Monitoring dan aksi existing tidak dihapus | `FE-RWI-264` ✅ | Source/build terbukti |
| `RWI-AC-R027-08` | Frontend-only dan production build lulus | `FE-RWI-264` ✅ | [Laporan task](../task/report/frontend/FE-RWI-264.md) |

---

## 3. Coverage

- Temuan: 2/2 terpetakan ke dua fix dan tepat satu task frontend.
- Acceptance criteria: 8/8 memiliki task dan bukti target.
- Backend task baru: 0; capability server existing dipakai tanpa perubahan.
- Coverage gap perencanaan: 0.
- Evidence gap implementasi: 1 — UAT pemilik belum dilaporkan; karena itu task tetap `🟡`.
