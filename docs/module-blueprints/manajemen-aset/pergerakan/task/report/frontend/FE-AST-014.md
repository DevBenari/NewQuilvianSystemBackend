# Laporan Perubahan Frontend — `FE-AST-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-014` |
| Judul | Kartu dashboard 1.4 dan laporan mutasi |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/frontend-roadmap.md` — kartu `FE-AST-014` |
| Trace | `CAP-01.4`, `CAP-11.2`; `SCR-AST-REG-01` (kartu 1.4), `SCR-AST-MOV-08`; `register/03-frontend-architecture.md` §3.5; `02-module-map.md` §3.1 (grup Laporan Manajemen Aset) |
| Contract version | `AST-MOV-1.0.0`; layar mengikuti **source** `BE-AST-019` termasuk deltanya (kartu pinjaman bila `canReadLoans`, `filters/metadata` laporan) |
| Wewenang UI | Rupa kartu/laporan `DEV_DISCRETION`; kartu 1.4 disembunyikan tanpa `AssetTransfer : Read` (wajib, §3.5) |
| Dependency | `FE-AST-010` [FE-REG] ✅, `FE-AST-012` ✅, `FE-AST-013` ✅, `BE-AST-019` ✅ |
| Klasifikasi | `STANDARD` — 1 route baru, 1 bagian dashboard, varian daftar; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-23 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `bc4d23807` (branch `RizkiV2`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar: Playwright **20/22 + cek ulang 2/2** (dua FAIL awal = harapan skrip peka huruf/`innerText`); `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-23 08:59:19) |

---

## 1. Proses bisnis dari sisi pengguna

| Layar | Siapa | Yang dilihat |
|---|---|---|
| **Dashboard Aset** (`/corporate/asset-management`) | Pemegang `AssetDashboard : Read` **dan** `AssetTransfer : Read` | Bagian baru **Mutasi & Peminjaman Berjalan** di bawah Kondisi Aset: kartu Mutasi Menunggu Unit Asal / Bagian Aset / Diterima; bila juga berhak membaca pinjaman, kartu Pinjaman Menunggu Persetujuan / Menunggu Diambil / Aktif / Terlambat. Ikut saringan unit dashboard. Klik kartu → daftar dengan tab yang sesuai |
| **Laporan Mutasi** (`/reports/transfers`) | Pemegang `AssetReport : Read` | Tab per status, saringan alasan/unit asal/unit tujuan/periode diajukan/kata kunci, tombol **Ekspor Excel** yang mengunduh baris yang sama |

Tautan kartu: mutasi → `movement/transfers?tab=<status>`; pinjaman diajukan/disetujui → `movement/loans?tab=<status>`;
pinjaman aktif → `movement/loan-returns`; terlambat → `movement/loan-returns?tab=terlambat`.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/movement/asset-transfer-constants.jsx` | + API laporan/dashboard, route laporan, varian daftar `report`, peta tautan dan placeholder kartu 1.4 |
| `src/lib/services/corporate/asset-management/asset-transfer-service.js` | + `fetchTransferReport`, `exportTransferReport` (blob + nama berkas), `fetchMovementDashboardSummary` |
| `src/lib/hooks/corporate/asset-management/movement/use-asset-transfer-list.jsx` | Sumber data per varian, ringkasan dilewati untuk varian laporan, `exportReport`, tab awal dari URL |
| `src/components/view/corporate/asset-management/movement/asset-transfer-list-view.jsx` | Eyebrow per varian, tombol Ekspor Excel, prop `initialTab` |
| `src/lib/hooks/corporate/asset-management/movement/use-asset-movement-dashboard.jsx` | Baru — hanya memanggil API bila `AssetTransfer : Read` |
| `src/components/view/corporate/asset-management/movement/asset-movement-dashboard-section.jsx` | Baru — bagian kartu 1.4 |
| `src/components/view/corporate/asset-management/register/asset-dashboard-view.jsx` | + satu baris penyisipan bagian 1.4 sesudah Kondisi Aset; kartu lain tidak diubah |
| `src/lib/hooks/corporate/asset-management/movement/use-asset-loan-list.jsx`, `asset-loan-list-view.jsx` | Tab awal dari URL |
| `src/app/corporate/asset-management/movement/{transfers,loans,loan-returns}/page.jsx` | Membaca `searchParams.tab` |
| `src/app/corporate/asset-management/reports/transfers/page.jsx` | Baru |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir "Laporan Mutasi" (`AssetReport : Read`, ikon `RiArrowLeftRightLine`) di grup Laporan Manajemen Aset |

Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah.

`UI GATE: 8 elemen — REUSE 8, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
|---|---|---|---|
| Bagian dashboard dan kartu bertaut | `BaseDetailSection`, `SummaryGrid` (`href`, `testId`) | REUSE | Pola kartu `FE-AST-010` |
| Galat + coba lagi | `InformationAlert`, `BaseButton` | REUSE | Pola `SectionError` dashboard |
| Laporan: kepala, tab, saringan, tabel | `Hero`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `AssetMasterResourceFilter`, `DataTable` | REUSE | Varian daftar mutasi `FE-AST-012` |
| Ekspor + toast | `BaseButton loading`, `ToastStack`, `saveAssetFile` | REUSE | Pola laporan register |

**Delta dari rancangan:** Laporan Mutasi memakai daftar mutasi sebagai varian (kolom sama dengan Pengajuan Pemindahan),
bukan layar laporan register, karena barisnya `AssetTransferResponse` dan saringannya saringan mutasi.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (14 berkas termasuk menu) | `0 errors, 0 warnings` | `PASS` | Keluaran perintah |
| Grep komentar dan nilai gaya literal | 0 | `PASS` | Keluaran perintah |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-23 08:59:19; manifest memuat `/corporate/asset-management/reports/transfers`; tidak ada source yang lebih baru dari build | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-014-20261123/uji-fe-014.mjs`) | **20/22**; 0 galat halaman, 0 respons 5xx (superadmin, Siti, penyetuju, LOKET) | `PASS` dengan 2 FAIL skrip | `hasil-uji.jsonl`, 5 tangkapan layar |
| Cek ulang dua FAIL (`cek-ulang.mjs`) | **2/2** | `PASS` | `hasil-uji.jsonl` (`CEK-ULANG …`), `01-dashboard-admin.png`, `03-ekspor-berhasil.png` |

Langkah yang lulus: kartu register lama tetap ada (Total Aset 79 dst., Kondisi Aset, Aset Per Unit); tautan tiga kartu
(`transfers?tab=Diajukan`, `loan-returns?tab=terlambat`, `loan-returns`); klik kartu Terlambat → tab Terlambat dengan
`isOverdue=true` dan pinjaman `AST-PJM-000010` tampil; klik kartu mutasi → tab Diajukan (`status=Diajukan`) memuat
`AST-MUT-000011`; saringan unit dashboard mengirim `organizationUnitId` ke ringkasan 1.4; butir menu Laporan Mutasi;
layar laporan tanpa tombol Ajukan, dengan Ekspor Excel; tab Diserahterimakan → `status=Diserahterimakan`, 2 baris; Siti
(tanpa `AssetLoan : Read`) melihat kartu mutasi saja; akun penyetuju (tanpa `AssetTransfer : Read`) tidak melihat bagian
1.4 dan **tidak ada panggilan** ke ringkasan 1.4, kartu register tetap; LOKET Akses Ditolak di laporan dan tanpa butir menu.
Data uji dibatalkan/dikembalikan, kebijakan Akses Role dipulihkan identik.

Dua FAIL di skrip utama: `AC1 BAGIAN-MUTASI-DI-DASHBOARD` mencocokkan label kartu dengan `innerText`, padahal kartu
menampilkan label kapital ("MUTASI MENUNGGU UNIT ASAL 1") — angka ketujuh kartu sudah sama dengan API; `AC2 EKSPOR-DARI-LAYAR`
membaca toast lewat `innerText`. Cek ulang memakai `textContent` tanpa peka huruf dan mencetak tiap syarat: ketujuh kartu =
label dan angka API; ekspor dari tab Diserahterimakan → permintaan `status=Diserahterimakan` tanpa `pageNumber`, Excel 2
baris = total 2 dengan nomor `AST-MUT-000008,AST-MUT-000002` sama urutannya dengan layar, baris info "Status:
Diserahterimakan", toast "Ekspor Berhasil".

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

**Temuan:** nama berkas yang diunduh `laporan-mutasi-aset-20261123.xlsx` (nama cadangan frontend), bukan nama server
`…-20261123-0933.xlsx`, karena header `Content-Disposition` tidak diekspos CORS ke JavaScript. Perilaku sama dengan ekspor
laporan register; isi berkas tidak terpengaruh. Dicatat, tidak dibuka sebagai task.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Kartu disisipkan ke dashboard `FE-AST-010` tanpa mengubah kartu lain; tersembunyi tanpa `AssetTransfer : Read` | `asset-movement-dashboard-section.jsx`, `use-asset-movement-dashboard.jsx`, 1 baris di `asset-dashboard-view.jsx` | Kartu register tetap; penyetuju tanpa hak tidak melihat bagian dan tidak memanggil API | ✅ |
| 2. Laporan dan ekspor | Varian `report`, `exportReport` | Tab status, Excel = layar | ✅ |
| 3. Butir menu "Laporan Mutasi" | `menu-items.jsx` | Tautan ditemukan; LOKET tidak melihatnya | ✅ |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi |
| `UAT` | Milik tim UAT |
