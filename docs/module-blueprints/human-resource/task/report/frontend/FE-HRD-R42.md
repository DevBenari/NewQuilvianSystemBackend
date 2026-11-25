# Laporan Perubahan Frontend — `FE-HRD-R42`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R42` |
| Judul | Verifikasi `S-D1` |
| Slice | `S-D1` (Perencanaan Tenaga Kerja), Gelombang 4 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R42`; task asal `FE-HRD-043` |
| Trace | API Contract v7; laporan `FE-HRD-R12`, `R26`, `R27`, `R36` |
| Contract version | API Contract v7 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Verifikasi menu, hak akses, dan keterjangkauan (`live-browser-testing`) |
| Dependency | `FE-HRD-R27` 🟡, `FE-HRD-R36` 🟡 |
| Klasifikasi | `LIGHT` — verifikasi 10 rute, 0 berkas source diubah |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | Laporan ini, laporan uji live, dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f095e2e0a` → `f7a1f0917` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `604cac6a` → `fcdd9f4d` (branch `MHamzah`) |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 10/10 layar `S-D1` terjangkau (9 dari menu, 1 dari tombol), hak akses benar untuk admin dan non-admin, bukti browser tersedia |

---

## 1. Keadaan yang ditemukan di awal

Sepuluh rute `S-D1` sudah dibangun pada `R12`, `R26`, `R27`, dan `R36`, tetapi belum pernah dibuka di browser per peran karena autentikasi lokal tertahan.

## 2. Proses bisnis dari sisi pengguna

1. Petugas perencanaan SDM membuka **Sumber Daya Manusia → Perencanaan Tenaga Kerja**, lalu memilih salah satu dari 9 butir menu: pengaturan, formasi, batch formasi, beban kerja, standar, gap, permintaan headcount, antrean rekrutmen, atau rencana tahunan.
2. Dari **Permintaan Headcount**, tombol **+ Buat Permintaan** membuka formulir permintaan baru (`/headcount-requests/baru`).
3. Jalur tidak normal: pegawai tanpa hak tidak melihat menu. Bila ia membuka alamat langsung, layar data menampilkan pesan "belum punya hak".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`hrw4-1118-routes.json` (10 rute), `menu-items.jsx`, `headcount-requests-view.jsx`, view `S-D1`, dan sembilan controller `Workforce Planning` di backend.

### 3.2 Berkas yang berubah

Tidak ada source frontend yang berubah. Artefak uji (di luar source): `test-with-agy/scripts/hrw4-1118-*.mjs` dan `test-with-agy/screenshots/hrw4-1118/R42-*`.

### 3.3 Kepatuhan arsitektur frontend

`NOT APPLICABLE` — tidak ada perubahan source.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` untuk komponen baru. Screenshot layar yang diverifikasi:

| Peran | Folder | Isi |
| --- | --- | --- |
| Admin | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw4-1118/R42-admin/` | 10 screenshot + `hasil.jsonl` |
| Non-admin | `.../R42-nonadmin/` | 10 screenshot + `hasil.jsonl` |
| Menu | `.../menu-admin/menu.json`, `.../menu-nonadmin/menu.json` | Keterlihatan butir menu |

`VISUAL CHECK: PASS` — kesepuluh layar dibuka di Chrome headed dan disimpan screenshot-nya.

## 4. State yang ditangani di layar

| State | Bukti |
| --- | --- |
| Memuat dan berhasil | 10/10 layar admin |
| Tanpa hak | 9/10 layar non-admin menampilkan "belum punya hak"; Pengaturan Perencanaan tidak (LBT-W4-D1-01, P3) |

## 5. Endpoint yang dikonsumsi

Tidak berubah. Matriks keterjangkauan:

| Rute | Menu (admin / non-admin) | Admin | Non-admin |
| --- | --- | --- | --- |
| `/hr/workforce-planning/settings` | ✅ / tersembunyi | ✅ | ⚠️ formulir tampil tanpa pesan tanpa hak (tanpa tombol simpan) |
| `/hr/workforce-planning/position-formations` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/initial-formation-batches` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/staffing-workloads` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/staffing-standards` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/staffing-gaps` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/headcount-requests` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/headcount-requests/baru` | dari tombol **+ Buat Permintaan** | ✅ | 🔒 |
| `/hr/workforce-planning/recruitment-queue` | ✅ / tersembunyi | ✅ | 🔒 |
| `/hr/workforce-planning/manpower-plans` | ✅ / tersembunyi | ✅ | 🔒 |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `live-browser-testing` admin, 10 rute | 10/10 tanpa 5xx/crash/blank/pesan Axios | `PASS` | `R42-admin/hasil.jsonl` |
| `live-browser-testing` non-admin, 10 rute | 9 pesan tanpa hak, 1 catatan P3 | `PASS` | `R42-nonadmin/hasil.jsonl` |
| Pencarian menu admin / non-admin | 9/9 terlihat / 0/9 terlihat | `PASS` | `menu.json` |
| `npx eslint` | Tidak ada berkas yang diubah | `NOT APPLICABLE` | — |
| `npm run build` pada HEAD `f7a1f0917` | **FAIL** di berkas di luar cakupan: `employee-job-descriptions-view.jsx:19` mengimpor `getTodayDate` yang tidak diekspor `workforce-core-display-utils.js` (pekerjaan Menu 07, commit `f7a1f0917`). Turbopack tidak melaporkan galat lain. | `FAIL (di luar cakupan)` | `scratchpad/build4.log` |
| `check-css-usage.mjs` | Tidak ada berkas yang diubah | `NOT APPLICABLE` | — |

`MANUAL TEST: PASS` — uji per peran dijalankan agent di browser.
`AUTOMATED TEST: SKIPPED (opsional) — task verifikasi tanpa perubahan source.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh layar terjangkau dari menu | ✅ Terpenuhi | 9 dari menu + 1 dari tombol; tabel bagian 5 |
| Hak akses benar per peran | ✅ Terpenuhi | Menu non-admin 0/9 terlihat; 9/10 layar menolak |
| Laporan dan status roadmap | ✅ | Laporan ini; [laporan uji live](../../testing/test-by-agy/laporan-live-testing-gelombang4-perencanaan-tenaga-kerja.md) |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` pada HEAD gagal karena impor `getTodayDate` di berkas Menu 07; bukan bagian R42, tetapi build wajib hijau sebelum rilis |
| Masalah yang diketahui | LBT-W4-D1-01: Pengaturan Perencanaan tanpa pesan tanpa hak (P3) |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Data uji tertinggal | Tidak ada |
| Langkah berikutnya | Uji alur tulis `S-D1` setelah pemilik mengizinkan data uji |
