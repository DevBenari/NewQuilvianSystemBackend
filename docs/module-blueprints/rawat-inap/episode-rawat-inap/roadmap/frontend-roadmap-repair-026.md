# Roadmap Frontend Repair 026 — Konfirmasi Masuk pada Modal Denah

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `RPR-026-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | **✅ SELESAI**; disetujui Pemilik Sistem pada 2026-10-17 dan UAT dikonfirmasi dengan pernyataan "saya sudah coba di testing tadi sudah bisa" |
| issue / rencana | [ISSUE-EPS-026](../docs/issue/issue-026-konfirmasi-masuk-hilang-di-modal-denah.md) / [PLAN-REPAIR-EPS-026](../docs/plan-repair/plan-repair-026-konfirmasi-masuk-hilang-di-modal-denah.md) |
| kontrak terkunci | Kontrak `0.13.0`; placement/cancel existing; keputusan `K-026-01` dan `K-026-02` |
| source SHA frontend / backend | `15ae73984ad26a408e3038566d7e1d1533fe10c3` / `c2bc563ed9332f08fa95b9fe35786fea7a647279` |
| hash lock | [repair-026-delivery-lock.json](repair-026-delivery-lock.json) |
| scope / ID | Tepat 1 task `FE-RWI-263`; pemindaian global memakai `FE-RWI-262` sebagai ID tertinggi; berikutnya `FE-RWI-264` |
| traceability | [requirement-traceability-repair-026.md](requirement-traceability-repair-026.md) |

---

## Arti Tanda Status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai dan acceptance criteria terbukti pada laporan task |
| 🟡 | Implementasi sebagian atau bukti belum lengkap |
| ⛔ | Terblokir oleh prasyarat yang belum terpenuhi |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
FE-RWI-247 ✅ [DNH] ──> FE-RWI-263
```

`[DNH]` adalah cermin baca-saja task yang sudah selesai pada [frontend-roadmap-integrasi-denah.md](frontend-roadmap-integrasi-denah.md). Garis berarti prasyarat → task. Repair tidak membentuk task backend karena endpoint placement/cancel, DTO, permission, dan validasi server existing dipakai tanpa perubahan.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 (repair) | `FE-RWI-247` ✅ [DNH] | `FE-RWI-263` | Pulihkan parity aksi placement/cancel pada modal denah dan kunci dengan test regresi |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Temuan | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-263` ✅ Selesai | Bed `Reserved/Dipesan` pada modal denah menyediakan konfirmasi masuk dan pembatalan reservasi dengan permission, refresh, version guard, dialog, error, recovery, dan toast existing | `FIX-EPS-026-01`; `ISS-EPS-026-01`, `T1`, `T2`, `T3`; `K-026-01`, `K-026-02` | `0.13.0`; `POST /placements`; `PATCH /reservations/{id}/cancel` | `BaseButton`, `ConfirmModal`, `PlacementFailureList`, `BedOperationRecovery`, `ToastStack`, `useInpatientBedBoardActions` | Adapter `rooms[].beds`; wiring action hook pada monitoring; tombol guarded pada pop-up; koordinasi dialog; test regresi | `FE-RWI-247` ✅ [DNH] | `RWI-AC-R026-01` s.d. `RWI-AC-R026-08` | [Laporan FE-RWI-263](../task/report/frontend/FE-RWI-263.md): test fokus/lint/build lulus; UAT pemilik lulus | Operasional Admisi/Bed Management; privacy dan optimistic-concurrency | D-FE-R026 ✅ |

---

## Kartu Task

### FE-RWI-263 — Pulihkan Konfirmasi Masuk dan Pembatalan Reservasi pada Modal Denah

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai — source, test fokus, lint, build, dan UAT pemilik terbukti pada [laporan task](../task/report/frontend/FE-RWI-263.md) |
| Outcome | Petugas berhak dapat menuntaskan atau membatalkan reservasi langsung dari bed `Dipesan` tanpa kembali ke UI monitoring lama |
| Jejak | `FIX-EPS-026-01`; `ISS-EPS-026-01`, `ISS-EPS-026-T1`, `ISS-EPS-026-T2`, `ISS-EPS-026-T3`; `K-026-01`, `K-026-02` |
| UI authority | Dua tombol ditempatkan pada baris bed `Dipesan`, berdampingan dengan `Riwayat`, dan boleh wrap responsif. Visual denah lain tidak berubah |
| Reuse / disposisi | `BaseButton`, `ConfirmModal`, `PlacementFailureList`, `BedOperationRecovery`, `ToastStack`: **REUSE**; `useInpatientBedBoardActions`: **REUSE_WITH_ADAPTER**; `FloorPlanRoomPopup`: **EXTEND**; base component baru: **TIDAK ADA** |
| Cakupan source | `floor-plan-monitoring.jsx`; `floor-plan-room-popup.jsx`; `floor-plan.module.css` hanya bila perlu; `tests/unit/floor-plan-monitoring.test.mjs` |
| Dependency | `FE-RWI-247` ✅ [DNH] |
| Batas | Frontend saja; dilarang mengubah endpoint, DTO, permission, database, migrasi, atau aturan bisnis backend |

#### Acceptance criteria terkunci

- `RWI-AC-R026-01`: `Konfirmasi Masuk` tampil hanya untuk bed `Reserved` dengan `episodeId`, `reservationId`, dan permission Create yang diizinkan.
- `RWI-AC-R026-02`: `Batalkan Pesanan` tampil hanya untuk reservasi aktif dengan metadata lengkap dan permission Update yang diizinkan; alasan wajib.
- `RWI-AC-R026-03`: permission `unknown/denied` atau `HoldingContext` yang disamarkan tidak pernah menghasilkan tombol maupun identitas yang dilarang.
- `RWI-AC-R026-04`: setiap aksi menjalankan refresh terbaru melalui adapter `{ beds: rooms.flatMap(...) }` sebelum dialog; perubahan status/version mencegah dialog dengan pesan aman.
- `RWI-AC-R026-05`: placement sukses memakai `episodeId`, `bedId`, dan `expectedBedVersion` terbaru, menutup pop-up, menampilkan toast, dan me-refresh denah.
- `RWI-AC-R026-06`: cancel memakai `reservationId` dan versi terbaru; 409 me-refresh, 422 placement menampilkan seluruh failure, outcome ambigu memakai recovery existing.
- `RWI-AC-R026-07`: history, cleaning, buka/tutup bed, mode Pilih/Master, dan privacy redaction tidak regresi.
- `RWI-AC-R026-08`: test fokus, lint error, unit test, production build, dan `git diff --check` lulus; browser test dilakukan bila runtime dan akun uji tersedia atau dicatat objektif sebagai tidak feasible.

#### Definition of Done `D-FE-R026`

1. Seluruh acceptance criteria memiliki bukti dalam `task/report/frontend/FE-RWI-263.md`.
2. Tidak ada base component baru dan tidak ada kontrak backend yang diubah.
3. Perubahan source hanya dalam batas task dan tidak menimpa perubahan user yang tidak terkait.
4. Roadmap dan traceability diperbarui ke status akhir berdasarkan bukti, bukan klaim.

