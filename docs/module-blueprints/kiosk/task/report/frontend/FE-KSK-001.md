# Laporan Perubahan Frontend — `FE-KSK-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-001` |
| Judul | Dropdown Jadwal Dokter di atas card |
| Slice | EPIC KSK-06 — Cek Jadwal Dokter, gelombang `MVP-0`, gelombang eksekusi 1 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-001` |
| Trace | `FR-KSK-050/051`; KSK-SCH-001; `KSK-UI-005` (`DEV_DISCRETION`); `KSK-CAP-040`; `03-frontend-architecture.md` §3 `FE-KSK-04` |
| Contract version | `NOT APPLICABLE` — tanpa API |
| Wewenang UI | `KSK-UI-005` — teknik perbaikan `DEV_DISCRETION`; komponen dasar `filter-select.jsx` tidak boleh diubah |
| Dependency | `NONE` |
| Klasifikasi | `LIGHT` — 1 berkas CSS, 2 properti |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("oke lanjutkan" setelah `FE-KSK-006`) |
| Target tulis | `V2QuilvianSystemFrontendDev`: `kiosk-doctor-schedule.module.css`. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `NOT APPLICABLE` |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 4 dari 4 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

Di layar **Cek Jadwal Dokter**, menu dropdown **Poliklinik** dan **Spesialis** tertutup card jadwal dokter di bawahnya. Pasien hanya melihat potongan tipis kotak cari menu; daftar poliklinik tertimbun card "dr. Rendy Pangalila" dan card lain (screenshot `before-1920x1080-Poliklinik.png`).

Penyebabnya:
- `.filterPanel` memakai `backdrop-filter`, sehingga membentuk *stacking context* sendiri tanpa `z-index`.
- Menu dropdown memang ber-`z-index` 10001, tetapi angka itu hanya berlaku **di dalam** panel.
- Card jadwal yang muncul sesudahnya di halaman (juga ber-`backdrop-filter`, `position: relative`, dan `transform` saat hover) digambar di atas panel, sekaligus menu di dalamnya.

Pengukuran sebelum perbaikan: dari 9 titik di dalam area menu, pada 1080×1920 hanya 6 (Poliklinik) dan 3 (Spesialis) yang mengenai menu; pada 1920×1080, **0 dari 9** untuk keduanya.

---

## 2. Proses bisnis dari sisi pengguna

1. Pasien membuka **Cek Jadwal Dokter** dari Beranda Kiosk.
2. Pasien menekan kotak **Poliklinik** atau **Spesialis**.
3. Menu terbuka utuh di atas card jadwal: kotak cari, "Semua Poliklinik", dan daftar poliklinik terlihat dan bisa disentuh.
4. Bila daftarnya panjang, pasien menggulir **di dalam menu**, dan halamannya tidak ikut bergeser.
5. Menu tetap di atas walau jari/kursor berada di atas card di bawahnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`src/style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css` (`.filterPanel`, `.scheduleGrid`, `.scheduleCard`, `backdrop-filter`); `src/components/view/kiosk/registration/doctor-schedule/kiosk-doctor-schedule-view.jsx`; `src/components/features/base-features/filter-select.jsx`, `resource-filter-select.jsx`, dan `src/style/components/features/base-features/filter-select.module.css` (baca-saja: `.menu` `z-index: 10001`, `.optionListScrollable` `max-height: 238px; overflow-y: auto`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css` | `.filterPanel` diberi `position: relative; z-index: 20;` beserta komentar alasan. Panel filter (dan menu di dalamnya) kini digambar di atas grid card |

`kiosk-doctor-schedule-view.jsx` tidak perlu diubah.

### 3.3 Kepatuhan arsitektur frontend

- Komponen dasar `filter-select.jsx`, `resource-filter-select.jsx`, dan CSS-nya **tidak disentuh** (`git status` untuk `src/components/features` dan `src/style/components`: kosong). Komponen itu dipakai 33 berkas (`KSK-CAP-040`).
- Tidak ada elemen UI baru — `UI GATE: N/A — hanya perbaikan urutan tumpukan CSS lokal pada layar existing`.
- Berkas CSS ini adalah stylesheet kiosk lama berpalet lokal (`--schedule-*`); perbaikan hanya menambah `position` dan `z-index` (tidak ada token z-index global), tanpa warna/ukuran baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Existing, tidak berubah |
| Kosong | Existing, tidak berubah |
| Gagal | Existing, tidak berubah |
| Tanpa hak akses | Existing, tidak berubah |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — perubahan CSS saja.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` pada berkas yang diubah | ESLint mengabaikan berkas `.css` ("no matching configuration"); repository tidak memakai stylelint | `NOT APPLICABLE` | Keluaran perintah |
| `npm run build` | Exit `0`, "Compiled successfully in 68s" | `PASS` | Keluaran `next build` |
| `git diff` / `git status` komponen dasar | Hanya `kiosk-doctor-schedule.module.css` berubah (+2 properti, +komentar); `src/components/features` dan `src/style/components` tidak berubah | `PASS` | Keluaran Git |
| Uji browser Playwright sebelum perbaikan | 2/8 PASS — bug terbukti (6 FAIL) | Bukti "sebelum" | `before.log`, screenshot |
| Uji browser Playwright sesudah perbaikan | 8/8 PASS | `PASS` | `after.log`, screenshot |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser (sebelum/sesudah)

Lingkungan: `next dev` port 3000 → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma` (baca-saja, 6 jadwal dokter hari Rabu). Login akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Cara ukur objektif:
- Menu dibuka, lalu `document.elementFromPoint` diambil pada 9 titik di dalam kotak menu.
- Lulus hanya bila **seluruh** titik mengenai menu itu sendiri.
- Pemeriksaan diulang setelah kursor diletakkan di atas card yang berada tepat di bawah tepi menu.
- Scroll diuji dengan `scrollBy` pada daftar opsi sambil memastikan halaman tidak ikut bergeser.

| ID | Skenario | Sebelum | Sesudah |
| --- | --- | --- | --- |
| AC1 | 1080×1920 — menu Poliklinik | FAIL: 6/9 titik, sisanya mengenai `cardTitleRow` | PASS 9/9 |
| AC1 | 1080×1920 — menu Spesialis | FAIL: 3/9 titik (`doctorPhoto`, judul card) | PASS 9/9 |
| AC1 | 1920×1080 — menu Poliklinik | FAIL: 0/9 titik (`H2`, `infoBlock`, `cardTitleRow`) | PASS 9/9 |
| AC1 | 1920×1080 — menu Spesialis | FAIL: 0/9 titik | PASS 9/9 |
| AC2 | 1080×1920 — card di bawah menu di-hover | FAIL: 6/9 (`cardHeader`) | PASS 9/9 |
| AC2 | 1920×1080 — card di bawah menu di-hover | FAIL: 0/9 | PASS 9/9 |
| AC3 | Daftar panjang di-scroll di dalam menu (kedua resolusi) | PASS (`scrollTop` 120, halaman diam) | PASS (sama) |

Screenshot tersimpan untuk setiap resolusi × dropdown, sebelum dan sesudah, ditambah kondisi hover. Contoh pada 1920×1080:
- **Sebelum:** menu terpotong di bawah card; yang terlihat hanya tepi kotak cari.
- **Sesudah:** menu utuh berisi "Cari poliklinik...", "Semua Poliklinik", "Klinik Fisioterapi", "Klinik Medical Check Up", "Poli Anak", "Poli Bedah", dan "Poli Gigi" di atas card.

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dan pemeriksaan visual screenshot.

**Tidak dijalankan:** perangkat kiosk fisik dengan layar sentuh (coverage gap roadmap, dibuktikan saat UAT).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Dropdown Poliklinik dan Spesialis tampil utuh di atas seluruh card pada 1080×1920 dan 1920×1080 | Terpenuhi | AC1 (4 kombinasi), sesudah 9/9 |
| 2. Tetap di atas saat card di-hover | Terpenuhi | AC2 (2 resolusi) |
| 3. Daftar panjang bisa di-scroll di dalam menu | Terpenuhi (sudah didukung komponen dasar; tetap berfungsi) | AC3 |
| 4. Diff tidak menyentuh berkas komponen dasar | Terpenuhi | `git status` komponen dasar kosong |
| DoD: AC 1–4 di laporan `FE-KSK-001.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Stylesheet Jadwal Dokter memakai palet lokal ber-nilai literal (`--schedule-*`), bukan token global — kondisi lama, tidak diubah di task ini |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi agent terputus setelah uji sesudah-perbaikan selesai. Dilanjutkan dari keadaan terverifikasi: server sisa dihentikan, diff dan build diperiksa, tanpa mengulang uji |
| Status Git | Frontend (task ini): ` M src/style/kiosk/registration/doctor-schedule/kiosk-doctor-schedule.module.css`, di samping perubahan `FE-KSK-003..006` yang belum di-commit |
| Langkah berikutnya | `FE-KSK-002` (format tanggal lahir dan nama utuh) |
