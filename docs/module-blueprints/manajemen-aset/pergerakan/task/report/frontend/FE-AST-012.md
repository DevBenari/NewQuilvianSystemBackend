# Laporan Perubahan Frontend — `FE-AST-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-012` |
| Judul | Mutasi |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/frontend-roadmap.md` — kartu `FE-AST-012` |
| Trace | `CAP-05.1`–`05.3`; `SCR-AST-MOV-01`, `02`, `03`, `06`; `03-frontend-architecture.md` §3.1–3.3; `02-module-map.md` §3.1 (grup Mutasi & Peminjaman Aset) |
| Contract version | `AST-MOV-1.0.0`; layar mengikuti **source** `BE-AST-017` termasuk deltanya (body keputusan, `rowVersion`, `availableActions`) |
| Wewenang UI | Rupa daftar/rincian/tab `DEV_DISCRETION`; tombol dari `availableActions` (wajib) |
| Dependency | `FE-AST-004` [REG] ✅, `BE-AST-017` ✅ |
| Klasifikasi | `STANDARD` — 5 route, 1 aggregate dengan 8 aksi; tanpa perubahan backend di luar `BE-AST-017` |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-19 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `f58875e43` (branch `RizkiV2`) |
| Tanggal | 19 November 2026 |
| Status | ✅ SELESAI 19 November 2026 — kelima kriteria terpetakan ke source dan teruji di layar **25/25**; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-19 12:37:20, sesudah commit `5d9af2ae1`) |

---

## 1. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terjadi di layar |
|---|---|---|
| 1 | Petugas Aset | **Pengajuan Pemindahan**: kartu ringkasan, tab per status dan "Menunggu Anda", **+ Ajukan Mutasi** |
| 2 | Petugas Aset | Formulir: Unit Asal (menyaring tabel aset), Unit Tujuan, Alasan, Mutasi yang Dikoreksi (hanya untuk Koreksi Mutasi), Catatan, Ruangan/Penanggung Jawab tujuan opsional; tabel aset unit asal dengan centang; aset rusak berat/hilang/dalam proses lain tidak dapat dicentang beserta alasannya → **Ajukan Mutasi N Aset** → rincian |
| 3 | Kepala Unit asal | **Persetujuan Mutasi**, tab Tahap Unit Asal → rincian → **Setujui** / **Tolak** |
| 4 | Kepala Bagian Aset | Tab Tahap Bagian Aset → **Setujui** / **Tolak** |
| 5 | Kepala Unit tujuan | **Serah Terima Antar Unit**, tab Menunggu Diterima Unit Anda → rincian → **Terima**: modal ruangan, penanggung jawab, kondisi per aset (terisi kondisi saat diajukan); tombol aktif setelah lengkap → toast menyebut ruangan dan jumlah laporan kondisi yang dibuka |
| 6 | Siapa pun yang berhak | **Cetak Berita Acara** (tab baru; bila diblokir, diunduh) |

| Keadaan tidak normal | Perilaku |
|---|---|
| Data diubah orang lain (409 `AST-VAL-MOV-016`/`015`) | Toast pesan backend, rincian dimuat ulang, tombol yang tidak berlaku hilang |
| Galat per isian (`005`, `008`, `013`) | Pesan di bawah isian |
| Galat per aset (`001..004`, `006`) | Pesan di atas tabel, tabel aset dimuat ulang |
| Tanpa `AssetTransfer : Read` | Menu tidak tampil; URL → Akses Ditolak |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/movement/asset-transfer-constants.jsx` | Baru — endpoint, route, aksi→hak, 3 varian daftar, kartu, isian formulir |
| `src/lib/services/corporate/asset-management/asset-transfer-service.js` | Baru |
| `src/utils/corporate/asset-management/movement/asset-transfer-utils.js` | Baru — parameter daftar per varian, payload, validasi, alasan blokir pemilih |
| `src/utils/corporate/asset-management/asset-status-labels.js` | + enum `transferStatus`, `transferReason`; `conditionReportSource` 5 Penerimaan Mutasi |
| `src/lib/hooks/corporate/asset-management/movement/use-asset-transfer-{route,actions,list,detail,editor}.jsx` | Baru — satu hook aksi untuk daftar dan rincian |
| `src/components/view/corporate/asset-management/movement/asset-transfer-{list-view,detail-view,form-view,action-modals,table-columns}.jsx` | Baru |
| `src/app/corporate/asset-management/movement/{transfers,transfers/create,transfers/[slug],transfer-approvals,transfer-receipts}/page.jsx` | Baru — 5 route |
| `src/utils/menu-sidebar/menu-items.jsx` | + grup "Mutasi & Peminjaman Aset" (3 butir, `AssetTransfer : Read`, ikon `RiArrowLeftRightLine`) |

Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah. Pemilih aset memakai
`buildHandoverPickerColumns` (prop `readyLabel`/`selectAllLabel` yang sudah ada).

`UI GATE: 12 elemen — REUSE 11, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
|---|---|---|---|
| Kepala layar, kartu, tab, saringan | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `AssetMasterResourceFilter` | REUSE | — |
| Tabel, aksi baris, badge | `DataTable`, `RowActionMenu`, `StatusBadge` | REUSE | — |
| Formulir pengajuan + pemilih aset | `BaseGroupedEditorView` + `afterForm` tabel | REUSE | Pola `FE-AST-006` |
| Rincian | `BaseDetailView`, `BaseDetailSection` | REUSE | — |
| Setujui / tolak / batal | `ConfirmModal` (+ `requireReason`) | REUSE | — |
| Modal Terima (ruangan, penanggung jawab, kondisi per aset) | `ConfirmModal children` + `AssetMasterResourceFilter` + `FilterSelect` | COMPOSE | Pola modal penetapan `FE-AST-007` |
| PDF, toast, akses ditolak | `useAssetPdfTab`, `ToastStack`, `AccessDeniedGate` | REUSE | — |

**Delta dari rancangan:** "Unit asal otomatis dari aset pertama" diganti isian **Unit Asal** yang dipilih lebih dulu untuk
menyaring tabel aset; backend tetap menentukan unit asal dari aset (`AST-VAL-MOV-004`). Tombol **Terima** hanya di
rincian (butuh daftar aset untuk kondisi per aset); dari daftar, aksi baris "Terima di detail" membuka rincian.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` (20 berkas termasuk menu dan label status) | `0 errors, 0 warnings`; ulang menu sesudah ganti ikon 0/0 | `PASS` | Keluaran perintah |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-19 12:37:20; manifest memuat 5 route `movement/*` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-012-20261119/uji-fe-012.mjs`) | **25/25**; 0 galat halaman, 0 respons 5xx (superadmin, Siti, penyetuju, Dimas, LOKET) | `PASS` | `hasil-uji.jsonl`, 7 tangkapan layar |

Langkah yang lulus: tiga butir menu; daftar dan tombol Ajukan; formulir menunggu unit asal (tombol nonaktif); pemilih
memanggil `organizationUnitId=<Rawat Inap>&placementState=Ditempatkan`; `AST-MBL-000035` Rusak Berat tak dapat dicentang
dengan alasan; ajukan lewat layar → `AST-MUT-000008`, rincian "Kepala Unit Instalasi Rawat Inap", hanya tombol Batalkan;
Siti menemukannya di Persetujuan Mutasi, melihat Setujui (tanpa Terima), menyetujui; penyetuju tab Tahap Bagian Aset
(`status=DisetujuiUnitAsal&awaitingMe=true`) menyetujui; Dimas menemukannya di Serah Terima, modal Terima nonaktif sampai
ruangan dan penanggung jawab dipilih, menerima → Diserahterimakan di Ruang Uji U94W3 C; Cetak Berita Acara → PDF 45.305
byte; data basi: mutasi dibatalkan dari tab lain lalu Batalkan di layar → 409, rincian dimuat ulang menjadi Dibatalkan
dan tombol hilang; LOKET Akses Ditolak tanpa menu. Kebijakan Akses Role dipulihkan identik.

Kegagalan skrip yang sudah diperbaiki: kunjungan pertama rute baru di `next dev` (kompilasi lebih dari 60 detik), klik
grup menu yang sudah terbuka justru menutupnya, `StatusBadge` mengapitalkan tiap kata, dan `id` isian catatan ada pada
`textarea` itu sendiri.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
|---|---|---|
| 1. Skema `03-frontend-architecture.md` §3.2–3.3 | Formulir, rincian dengan langkah dan pelaku, tabel kondisi diajukan/diterima | ✅ |
| 2. Tombol mengikuti `AvailableActions` | Pengaju hanya Batalkan; Siti Setujui/Tolak; Dimas Terima/Tolak Terima | ✅ |
| 3. Terima mewajibkan ruangan dan penanggung jawab tujuan | Tombol modal nonaktif → aktif setelah dipilih | ✅ |
| 4. 409 data basi memuat ulang detail | Uji data basi | ✅ |
| 5. Tiga butir menu terdaftar | `menu-items.jsx`; tautan ditemukan | ✅ |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi |
| `UAT-MOV-01..03` | Milik tim UAT |
| Commit | Bersama `BE-AST-017` |
