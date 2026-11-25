# Laporan Perubahan Frontend — `FE-AST-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-009` |
| Judul | Impor data awal |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-009` |
| Trace | `CAP-EXT-01`; `AST-DEC-023`, `037`, `050`; `SCR-AST-REG-16`; `03-frontend-architecture.md` §3.6; `02-module-map.md` §3.1 (butir "Impor Data Awal" di Registrasi & Inventaris) |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** `BE-AST-012` termasuk deltanya (`/rows`, `summary`, `amountsVisible`) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-002` ✅, `BE-AST-012` ✅ |
| Klasifikasi | `STANDARD` — 2 route, 1 aggregate dengan 5 aksi + unggah/unduh; 12 berkas baru, 1 berkas diubah; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-18 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a2ee4c22c` (branch `RizkiV2`) |
| Tanggal | 18 November 2026 |
| Status | ✅ SELESAI 18 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar (21/21); dua perbaikan tampilan sesudah uji terbukti pada build ulang (`BUILD_ID` 15:30:16) |

---

## 1. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terjadi di layar |
|---|---|---|
| 1 | Petugas Aset | Impor Data Awal: kartu (Menunggu Bagian Aset, Menunggu Akuntansi, Diterapkan, Gagal Validasi), keterangan tanggal awal dan batas baris |
| 2 | Petugas Aset | **Unduh Templat** → `templat-impor-aset.xlsx` |
| 3 | Petugas Aset | **+ Unggah Berkas** → modal berkas (tombol nonaktif sampai berkas dipilih) → **Unggah dan Validasi** → rincian batch |
| 4a | Petugas Aset | Gagal Validasi: peringatan "10 dari 12 baris salah, sehingga seluruh berkas ditolak…", tabel **Baris Berkas** dengan kolom *Baris Excel* dan pesan + kode per baris; saringan **Hanya baris salah** |
| 4b | Petugas Aset | Tervalidasi: ringkasan total harga, akumulasi, nilai buku awal (bila berhak lihat rupiah); hanya tombol Batalkan dan Unduh Berkas Asli |
| 5 | Kepala Bagian Aset | Tab **Menunggu Persetujuan Anda** → rincian → **Setujui Data Aset** / **Tolak** |
| 6 | Kepala Akuntansi | **Setujui Angka dan Terapkan** (modal menyebut jumlah aset, total harga dan akumulasi) / **Tolak (Akuntansi)** → baris menampilkan nomor inventaris; klik dua kali membuka aset |

| Keadaan tidak normal | Perilaku |
|---|---|
| Tanggal awal belum diatur | Peringatan "Tanggal awal belum diatur…", tombol Unggah nonaktif |
| Unggahan ditolak (`060`, `061`, `068`) | Toast pesan backend |
| Pengunggah | Tidak melihat tombol Setujui (backend juga menolak 403 `067`) |
| Tanpa hak `AssetImport : Read` | Menu tidak tampil; URL → Akses Ditolak |
| Tanpa `ViewAmount` | Ringkasan rupiah dan kolom Harga/Akumulasi tidak ditampilkan |
| Penerapan besar | Batas waktu request unggah 10 menit, penerapan 15 menit (temuan kinerja di `BE-AST-012` 5.3) |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/register/asset-import-constants.jsx` | Baru — endpoint, route, aksi→hak, tab, kartu, saringan baris, isian berkas |
| `src/lib/services/corporate/asset-management/asset-import-service.js` | Baru — daftar (status berulang), summary, rincian, baris, unggah, keputusan, templat (blob) |
| `src/utils/corporate/asset-management/register/asset-import-utils.js` | Baru |
| `src/lib/hooks/corporate/asset-management/register/use-asset-import-{actions,list,detail}.jsx` | Baru — satu hook aksi untuk daftar dan rincian |
| `src/components/view/corporate/asset-management/register/asset-import-{list-view,detail-view,action-modals,table-columns}.jsx` | Baru |
| `src/app/corporate/asset-management/register/imports/{,[slug]}/page.jsx` | Baru — 2 route |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir "Impor Data Awal" (`AssetImport : Read`) di Registrasi & Inventaris |

Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah.

`UI GATE: 11 elemen — REUSE 10, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
|---|---|---|---|
| Kepala layar, kartu, tab, saringan | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect` | REUSE | — |
| Tabel batch dan tabel baris, aksi baris, badge | `DataTable`, `RowActionMenu`, `StatusBadge` | REUSE | — |
| Rincian + bagian tabel | `BaseDetailView`, `BaseDetailSection` | REUSE | — |
| Unggah berkas | `ConfirmModal children` + `BaseGroupedEditorField` berkas | COMPOSE | Pola `FE-AST-007`/`008` |
| Konfirmasi beralasan | `ConfirmModal requireReason` | REUSE | — |
| Unduh berkas | `saveAssetFile` | REUSE | — |
| Pesan, toast, akses ditolak | `InformationAlert`, `ToastStack`, `AccessDeniedGate` | REUSE | — |

---

## 3. Endpoint yang dikonsumsi

`GET /register/imports` (`status`, `awaitingMyDecision`), `/summary`, `/template` (blob), `/{id}`, `/{id}/rows`
(`onlyErrors`), `POST /` (multipart), `/{id}/approve-asset`, `/{id}/approve-value`, `/{id}/reject`, `/{id}/cancel`;
`GET /register/attachments/{id}` (berkas asli).

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` (13 berkas, termasuk menu) | `0 errors, 0 warnings`; ulang atas 2 berkas sesudah perbaikan tampilan juga 0/0 | `PASS` | Keluaran perintah |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-18 15:05:36; manifest memuat 2 route impor | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-009-20261118/uji-fe-009.mjs`) | **21/21**; 0 galat halaman, 0 respons 5xx (superadmin 14, penyetuju 12, LOKET 2 panggilan) | `PASS` | `hasil-uji.jsonl`, 5 tangkapan layar |

Langkah yang lulus: menu; tanpa tanggal awal → peringatan dan tombol nonaktif; daftar dan kartu; unduh templat
(12 KB); unggah `campur-salah.xlsx` → Gagal Validasi "10 dari 12 baris salah", 12 baris, saringan → 10 baris dengan
nomor baris Excel dan kode (`AST-VAL-REG-065` di baris 7); unggah 5 baris → Tervalidasi dengan total Rp 12.500.000 dan
nilai buku awal; pengunggah hanya melihat Batalkan; penyetuju: tab "Menunggu Persetujuan Anda" → Setujui Data Aset →
Setujui Angka dan Terapkan (modal menyebut Rp 12.500.000 dan Rp 7.500.000) → 5 nomor `AST-MBL-…`; unduh berkas asli;
LOKET Akses Ditolak. Tanggal awal dan policy penyetuju dipulihkan.

**Temuan tampilan dari tangkapan layar (`03-gagal-validasi-baris-salah.png`), diperbaiki dan dibuktikan pada build ulang Rizki (`BUILD_ID` 15:30:16; `cek-perbaikan.mjs` 3/3 + `06-perbaikan-sesudah-build.png`):**

1. Strip audit menampilkan "Tanggal dibuat 0": `BaseDetailCard` menebak baris audit dari label, dan label "Aset
   Dibuat" terbaca sebagai tanggal dibuat. Label diganti "Aset Terbentuk".
2. Pesan galat ditampilkan sebagai `StatusBadge`. Badge itu mengubah tiap kata menjadi huruf kapital dan memotong
   teks panjang. Sekarang pesan tampil sebagai teks biasa beserta kodenya, satu baris per galat.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
|---|---|---|
| 1. Unduh templat, unggah, tabel galat per nomor baris Excel | Langkah 2–4a; pesan galat utuh sesudah build ulang | ✅ |
| 2. Dua tombol persetujuan sesuai hak | Pengunggah tanpa Setujui; penyetuju Setujui Data Aset → Setujui Angka | ✅ |
| 3. Ringkasan total untuk Kepala Akuntansi | Total harga, akumulasi, nilai buku awal di rincian dan modal Terapkan; disembunyikan tanpa `ViewAmount` (diuji di `BE-AST-012`) | ✅ |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi (termasuk build ulang perbaikan tampilan) |
| Commit | Belum |
