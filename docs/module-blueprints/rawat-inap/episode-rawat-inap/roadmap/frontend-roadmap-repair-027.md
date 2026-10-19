# Roadmap Frontend Repair 027 — Aksi Pembersihan pada Denah Mode Pilih

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `RPR-027-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | **🟡 SEBAGIAN** — disetujui Pemilik Sistem; source dan build selesai, UAT pemilik belum dilaporkan |
| issue / rencana | [ISSUE-EPS-027](../docs/issue/issue-027-aksi-pembersihan-hilang-mode-pilih-bed.md) / [PLAN-REPAIR-EPS-027](../docs/plan-repair/plan-repair-027-aksi-pembersihan-hilang-mode-pilih-bed.md) |
| kontrak terkunci | `0.13.0` + amandemen `PLAN-REPAIR-EPS-027`; endpoint dan permission backend tidak berubah |
| source SHA frontend / backend | `15ae73984ad26a408e3038566d7e1d1533fe10c3` / `c2bc563ed9332f08fa95b9fe35786fea7a647279` |
| hash lock | [repair-027-delivery-lock.json](repair-027-delivery-lock.json) |
| scope / ID | Tepat 1 task `FE-RWI-264`; ID tertinggi yang sudah dipakai adalah `FE-RWI-263`; berikutnya `FE-RWI-265` |
| traceability | [requirement-traceability-repair-027.md](requirement-traceability-repair-027.md) |

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
FE-RWI-249 ✅ [DNH] ──┬─> FE-RWI-264 🟡
                      │
FE-RWI-263 ✅ [R026] ─┘
```

`[DNH]` adalah cermin baca-saja task integrasi mode Pilih pada
[frontend-roadmap-integrasi-denah.md](frontend-roadmap-integrasi-denah.md). `[R026]` adalah
cermin baca-saja task pemulihan aksi modal Monitoring pada
[frontend-roadmap-repair-026.md](frontend-roadmap-repair-026.md). Garis berarti prasyarat → task.
Repair ini tidak membentuk task backend karena endpoint, status, permission, dan validasi server
existing dipakai tanpa perubahan.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 (repair) | `FE-RWI-249` ✅ [DNH], `FE-RWI-263` ✅ [R026] | `FE-RWI-264` 🟡 — [laporan](../task/report/frontend/FE-RWI-264.md) | Source dan production build selesai; UAT pemilik pending |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Temuan | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-264` 🟡 Sebagian | Pengguna berwenang dapat membuka alur pembersihan existing dari bed yang belum layak pada modal denah mode Pilih; hasil tindakan menyegarkan denah dan kelayakan | `FIX-EPS-027-01`, `FIX-EPS-027-02`; `ISS-EPS-027-01`, `ISS-EPS-027-T1`; `K-027-01` | `0.13.0` + amandemen repair 027; API Bed Occupancy/Bed Management existing | `BaseButton`, `InpatientBedCleaningDetail`, `useEffectivePermissions`, `FloorPlanRoomPopup` | Guard aksi per bed; komposisi mode Pilih; callback refresh; rekonsiliasi pilihan yang tidak lagi eligible | `FE-RWI-249` ✅ [DNH], `FE-RWI-263` ✅ [R026] | `RWI-AC-R027-01` s.d. `RWI-AC-R027-08` | [Laporan FE-RWI-264](../task/report/frontend/FE-RWI-264.md): production build lulus; test/lint tidak dijalankan; UAT pemilik pending | Komponen pop-up bersama; permission `unknown`; stale eligibility | `D-FE-R027` 🟡 |

---

## Kartu Task

### 🟡 FE-RWI-264 — Kembalikan Alur Pembersihan pada Denah Mode Pilih Bed

| Field | Isi |
| --- | --- |
| Status | 🟡 Sebagian — source dan `npm.cmd run build` selesai; test/lint tidak dijalankan sesuai instruksi pemilik; UAT pemilik belum dilaporkan. Bukti: [laporan task](../task/report/frontend/FE-RWI-264.md) |
| Outcome | Petugas yang mempunyai hak operasional dapat menangani pembersihan tanpa keluar dari konteks pemilihan bed, tetapi tidak dapat memilih bed sebelum snapshot server menyatakannya siap dan eligible |
| Jejak | `FIX-EPS-027-01`, `FIX-EPS-027-02`; `ISS-EPS-027-01`, `ISS-EPS-027-T1`; `K-027-01` |
| UI authority | Pada cabang mode Pilih, tampilkan `Pembersihan` sebagai aksi sekunder di samping informasi kelayakan hanya bila `AvailableActions ∩ permission` mengizinkan; struktur dan visual mode Monitoring tetap dipertahankan |
| Reuse / disposisi | `BaseButton`: **REUSE**; `InpatientBedCleaningDetail`: **REUSE**; kelayakan + aksi: **COMPOSE** pada `FloorPlanRoomPopup`; `useEffectivePermissions`: **REUSE**; base component baru: **TIDAK ADA** |
| Klasifikasi | `MEDIUM`: shared popup dan selection controller berubah, tanpa service/API/backend/CSS baru |
| Cakupan source | `floor-plan-room-popup.jsx`; `floor-plan-selection.jsx`; CSS hanya bila komposisi existing terbukti tidak memadai |
| Dependency | `FE-RWI-249` ✅ [DNH], `FE-RWI-263` ✅ [R026] |
| Batas validasi | Agent hanya menjalankan `npm run build`. Test, lint, browser testing, `dotnet build`, dan migration tidak dijalankan; UAT milik pemilik |
| Batas perubahan | Frontend dan dokumen repair saja; dilarang mengubah endpoint, DTO, permission, status bisnis, database, migrasi, atau konfigurasi backend |

#### Acceptance criteria terkunci

- `RWI-AC-R027-01`: bed `WaitingCleaning` dengan `StartCleaning` dan permission efektif yang sesuai menampilkan `Pembersihan` bersama alasan belum dapat dipilih pada mode Pilih.
- `RWI-AC-R027-02`: `AvailableActions` kosong, permission `denied`, atau permission `unknown` tidak menampilkan tombol dan tidak menghasilkan request mutasi.
- `RWI-AC-R027-03`: tombol membuka `InpatientBedCleaningDetail` existing, yang memuat snapshot monitoring terbaru sebelum menawarkan tindakan.
- `RWI-AC-R027-04`: keberhasilan tindakan mengembalikan promise refresh state efektif; jalur admisi transfer tetap me-refresh denah dan overlay eligibility bersama-sama.
- `RWI-AC-R027-05`: pilihan aktif dibersihkan bila snapshot terbaru tidak lagi memiliki bed tersebut sebagai `Eligibility.IsEligible = true`; bed pembersihan tetap tidak dapat dipilih selama belum eligible.
- `RWI-AC-R027-06`: penanganan 403/409/422, version/cycle guard, dan larangan optimistic status tetap dimiliki hook serta server existing.
- `RWI-AC-R027-07`: aksi Monitoring yang dipulihkan `FE-RWI-263`, history, buka/tutup bed, privacy, focus, dan reduced motion tidak dihapus atau diubah kontraknya.
- `RWI-AC-R027-08`: tidak ada perubahan backend/database; `npm run build` frontend selesai sukses. Test dan UAT tidak diklaim oleh agent; UAT pemilik diperlukan sebelum status penuh `✅`.

#### Definition of Done `D-FE-R027`

1. Seluruh acceptance criteria memiliki bukti jujur dalam `task/report/frontend/FE-RWI-264.md`.
2. Tidak ada base component, endpoint, permission, atau kontrak backend baru.
3. Build produksi frontend lulus dan perubahan source lain milik pengguna tetap dipertahankan.
4. Roadmap, traceability, issue, dan plan repair memakai status akhir berdasarkan bukti.
5. Status `✅` hanya diberikan setelah pemilik menyampaikan hasil UAT; sebelum itu source yang sudah dibangun berstatus `🟡`.
