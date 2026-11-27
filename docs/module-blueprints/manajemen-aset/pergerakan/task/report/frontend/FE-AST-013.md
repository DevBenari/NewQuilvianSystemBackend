# Laporan Perubahan Frontend — `FE-AST-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-013` |
| Judul | Peminjaman |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/frontend-roadmap.md` — kartu `FE-AST-013` |
| Trace | `CAP-05.4`, `05.5`; `SCR-AST-MOV-04`, `05`, `07`; `03-frontend-architecture.md` §3.1, §3.4, §5; `02-module-map.md` §3.1 (grup Mutasi & Peminjaman Aset) |
| Contract version | `AST-MOV-1.0.0`; layar mengikuti **source** `BE-AST-018` (`b7bc4a5f`) termasuk deltanya (`loanable-assets`, `summary`, body `rowVersion`, status ganda, `openExtension`, `overdueDays`) |
| Wewenang UI | Rupa daftar/rincian/tab `DEV_DISCRETION`; tanda Terlambat dari `isOverdue` (wajib, §5); tombol dari `availableActions` (wajib) |
| Dependency | `FE-AST-004` [REG] ✅, `BE-AST-018` ✅ |
| Klasifikasi | `STANDARD` — 4 route, 1 aggregate dengan 10 aksi layar; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-19 ("commit semua nya lalu lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `5d9af2ae1` (branch `RizkiV2`) |
| Tanggal | 19 November 2026 |
| Status | ✅ SELESAI 20 November 2026 — keempat kriteria terpetakan ke source dan teruji di layar: Playwright **32/34 + cek ulang 6/6** (dua FAIL awal = harapan skrip peka huruf, diperiksa ulang lewat `textContent`); `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-20 11:18:04); source di commit `9e205e827` |

---

## 1. Proses bisnis dari sisi pengguna

| Layar | Siapa | Yang dilakukan |
|---|---|---|
| **Peminjaman Sementara** (`/movement/loans`) | Semua pemegang `AssetLoan : Read` | Kartu Menunggu Anda, Terlambat, Diajukan, Dipinjam, Menunggu Konfirmasi Kembali, Dikembalikan; tab per status + Menunggu Anda + Terlambat; peringatan merah bila ada pinjaman terlambat; tombol **+ Ajukan Pinjaman** bagi pemegang `Create` |
| **Ajukan Pinjaman** (`/movement/loans/create`) | Petugas Aset, Kepala Unit | Pilih unit pemilik (menyaring tabel aset dari `loanable-assets`), unit peminjam, tanggal mulai (bawaan hari ini), rencana kembali, tujuan; centang 1–200 aset; Ajukan → rincian |
| **Pengembalian Aset** (`/movement/loan-returns`) | Semua pemegang `AssetLoan : Read` | Pinjaman `Dipinjam` + `Menunggu Konfirmasi Kembali`, urut rencana kembali terdekat; tab Terlambat |
| **Rincian Pinjaman** (`/movement/loans/[slug]`) | Pihak pinjaman | Ringkasan dengan pelaku tiap langkah; tabel aset (kondisi diambil/kembali) dan tabel perpanjangan; tombol sesuai `availableActions` |

Tombol rincian: Setujui/Tolak (Kepala Unit pemilik), Ambil Aset (Kepala Unit peminjam), Minta Perpanjangan (peminjam;
modal tanggal + alasan, tombol aktif hanya bila tanggal baru sesudah rencana kembali), Setujui/Tolak Perpanjangan
(pemilik), Kembalikan (peminjam), Konfirmasi Kembali (pemilik; modal kondisi per aset tanpa nilai bawaan, tombol aktif
hanya bila semua terisi, pilihan tanpa Hilang), Batalkan, Cetak Berita Acara.

Tanda di kolom status: **Terlambat N hari** (merah, dari `isOverdue`/`overdueDays`, tidak dihitung di browser),
**Perpanjangan diminta**, **Menunggu Anda**. Rincian terlambat menampilkan peringatan merah.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/movement/asset-loan-constants.jsx` | Baru — endpoint, route, aksi layar→aksi backend→hak, 2 varian daftar, kartu, isian formulir, kondisi kembali |
| `src/lib/services/corporate/asset-management/asset-loan-service.js` | Baru — 13 panggilan; `status` diulang (`status=3&status=4`) lewat serializer seperti layanan pemanfaatan |
| `src/utils/corporate/asset-management/movement/asset-loan-utils.js` | Baru — parameter daftar per tab, validasi formulir (mulai ≥ kemarin, kembali ≥ mulai), payload, label periode/terlambat |
| `src/utils/corporate/asset-management/asset-status-labels.js` | + enum `loanStatus`, `loanExtensionStatus` |
| `src/lib/hooks/corporate/asset-management/movement/use-asset-loan-{route,actions,list,detail,editor}.jsx` | Baru — satu hook aksi untuk daftar dan rincian |
| `src/components/view/corporate/asset-management/movement/asset-loan-{list-view,detail-view,form-view,action-modals,table-columns}.jsx` | Baru |
| `src/app/corporate/asset-management/movement/{loans,loans/create,loans/[slug],loan-returns}/page.jsx` | Baru — 4 route |
| `src/utils/menu-sidebar/menu-items.jsx` | + 2 butir di grup "Mutasi & Peminjaman Aset": Peminjaman Sementara (`RiHandCoinLine`), Pengembalian Aset (`RiTimerLine`), keduanya `AssetLoan : Read` |

Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah, tanpa nilai warna/ukuran literal.

`UI GATE: 13 elemen — REUSE 11, COMPOSE 2, EXTEND 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
|---|---|---|---|
| Kepala layar, kartu, tab, saringan | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `AssetMasterResourceFilter` | REUSE | — |
| Tabel, aksi baris | `DataTable`, `RowActionMenu` | REUSE | — |
| Tanda Terlambat / Perpanjangan diminta / Menunggu Anda | `StatusBadge` (`danger`, `pending`, `info`) | REUSE | — |
| Formulir + pemilih aset | `BaseGroupedEditorView` (isian `date`) + `afterForm` tabel `buildHandoverPickerColumns` (`extraColumns` Ruangan) | REUSE | Prop yang sudah ada |
| Rincian + dua tabel bagian | `BaseDetailView`, `BaseDetailSection` | REUSE | — |
| Setujui/ambil/kembalikan, tolak/batal beralasan | `ConfirmModal` (+ `requireReason`) | REUSE | — |
| Modal Minta Perpanjangan (tanggal + alasan) | `ConfirmModal children` + `FilterDatePicker` + `requireReason` | COMPOSE | Pola modal Terima `FE-AST-012` |
| Modal Konfirmasi Kembali (kondisi per aset) | `ConfirmModal children` + `FilterSelect` per aset | COMPOSE | Pola modal Terima `FE-AST-012` |
| PDF, toast, akses ditolak | `useAssetPdfTab`, `ToastStack`, `AccessDeniedGate` | REUSE | — |

Kedua `COMPOSE` mengikuti preseden yang sudah diterima di `FE-AST-012`/`FE-AST-007`; alternatifnya (halaman terpisah untuk
perpanjangan/konfirmasi) memecah alur dan menambah route tanpa manfaat, sehingga tidak dipilih.

**Delta dari rancangan:** pemilih aset memakai `GET /movement/loans/loanable-assets` (delta `BE-AST-018` no. 2), bukan
`/register/assets`, karena peminjam tidak berhak melihat daftar aset unit lain; unit pemilik dipilih dulu untuk
menyaringnya. Konfirmasi kembali hanya dari rincian (butuh daftar aset); dari daftar, aksi baris "Konfirmasi di detail"
membuka rincian. Kolom daftar memakai satu kolom tanggal **Mulai – Rencana Kembali** (rancangan §3.1 "Tanggal"); pengaju
tampil di rincian.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (19 berkas termasuk menu dan label status) | `0 errors, 0 warnings` (satu peringatan `exhaustive-deps` diperbaiki dengan `useMemo`); ulang berkas kolom sesudah ganti kolom 0/0 | `PASS` | Keluaran perintah |
| Grep komentar dan nilai gaya literal | 0 | `PASS` | Keluaran perintah |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-20 11:18:04; manifest memuat `movement/loans`, `loans/create`, `loans/[slug]`, `loan-returns` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright di `next dev` (`test-with-agy/asset-management/fe-ast-013-20261119/uji-fe-013.mjs`) | **32/34**; 0 galat halaman, 0 respons 5xx (superadmin, Siti, Dimas, LOKET) | `PASS` dengan 2 FAIL skrip | `hasil-uji.jsonl`, 10 tangkapan layar |
| Cek ulang dua FAIL (`cek-ulang.mjs`) | **6/6** | `PASS` | `hasil-uji.jsonl` (`CEK-ULANG …`), `09-tab-terlambat.png`, `04b-rincian-perpanjangan-diminta.png` |

Langkah yang lulus: dua butir menu baru; daftar dengan tab Terlambat dan tombol Ajukan; formulir menunggu unit pemilik
(tombol nonaktif); pemilih memanggil `loanable-assets?ownerOrganizationUnitId=<Rawat Inap>`; ajukan lewat layar →
`AST-PJM-000005` (body `startDate`/`plannedReturnDate` = 2026-11-19, 2 aset), rincian menunggu "Kepala Unit Instalasi
Rawat Inap", hanya Batalkan; Siti menemukannya di tab Menunggu Anda, melihat Setujui tanpa Ambil, menyetujui; Dimas
mengambil → tombol Kembalikan, Minta Perpanjangan, Cetak Berita Acara; **modal perpanjangan nonaktif saat kosong dan saat
tanggal sama dengan rencana kembali, aktif untuk 21 Nov**; permintaan terkirim (`requestedReturnDate` 2026-11-21); Siti
melihat Setujui/Tolak Perpanjangan, menyetujui → "21 Nov 2026 (semula 19 Nov 2026)"; Dimas Kembalikan; Pengembalian Aset
memanggil `status=Dipinjam&status=MenungguKonfirmasiKembali`; **modal Konfirmasi Kembali nonaktif tanpa kondisi, tetap
nonaktif dengan satu dari dua kondisi, aktif setelah keduanya; pilihan Aktif/Rusak Ringan/Rusak Berat/Tidak Digunakan**;
konfirmasi (Aktif + Rusak Ringan) → Dikembalikan, toast "1 laporan kondisi dibuka"; PDF 46.906 byte; pinjaman terlambat
`AST-PJM-000006` (disiapkan lewat API) muncul di tab Terlambat dengan `isOverdue=true`, kartu Terlambat = 1 dan
peringatan merah; rincian terlambat menampilkan peringatan; Siti menolak perpanjangan (alasan wajib) dari layar; LOKET
Akses Ditolak tanpa menu. Kebijakan Akses Role dipulihkan identik.

Dua FAIL di skrip utama (`AC1 TANDA-TERLAMBAT-DARI-ISOVERDUE`, `AC3 MINTA-PERPANJANGAN-LEWAT-LAYAR`) membandingkan
`innerText` dengan "Terlambat 1 hari"/"Perpanjangan diminta", padahal `StatusBadge` menampilkan kapital tiap kata
("Terlambat 1 Hari"). Cek ulang pada `AST-PJM-000007` membaca `textContent` badge: `["Dipinjam","Terlambat 1 hari",
"Perpanjangan diminta"]` di baris tab Terlambat, dan rincian peminjam memuat badge itu, "menunggu keputusan", tanpa tombol
Minta Perpanjangan — keduanya lulus.

Sesudah uji, kolom "Diajukan" dikeluarkan dari daftar pinjaman karena di 1600 px kolom Status terpotong; diperiksa ulang
(`lihat-daftar.mjs`): 8 kolom, tepi kanan Aksi 1569 px, tangkapan `01-peminjaman-sementara.png`.

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

**Data uji di `QuilvianNewDevRizki`:** `AST-PJM-000005`, `000006`, `000007` Dikembalikan; `000005` membuka laporan
kondisi Rusak Ringan untuk `AST-MBL-000069`.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Tanda Terlambat dari `isOverdue` | `LoanStatusBadges`, `overdueLabel`, tab/kartu Terlambat | Baris tab Terlambat dan rincian; parameter `isOverdue=true` | ✅ |
| 2. Konfirmasi kembali mewajibkan kondisi per aset | `asset-loan-action-modals.jsx`, `confirmReady` | Tombol nonaktif sampai semua kondisi diisi | ✅ |
| 3. Perpanjangan dan keputusannya | Modal Minta Perpanjangan, Setujui/Tolak Perpanjangan, tabel Perpanjangan | Minta, setujui, tolak beralasan lewat layar | ✅ |
| 4. Dua butir menu terdaftar | `menu-items.jsx` | Tautan `loans` dan `loan-returns` ditemukan | ✅ |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi |
| Commit | Source oleh Rizki di `9e205e827` (bersama tampilan jurnal dan jurnal berulang) |
| `UAT-MOV-04..06` | Milik tim UAT |
