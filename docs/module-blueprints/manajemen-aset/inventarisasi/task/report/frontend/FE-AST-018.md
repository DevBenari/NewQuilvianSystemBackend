# Laporan Perubahan Frontend — `FE-AST-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-018` |
| Judul | Kartu 1.5 dan laporan inventaris |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/frontend-roadmap.md` — kartu `FE-AST-018` |
| Trace | `CAP-01.5`, `CAP-11.4`; `AST-DEC-031`; kartu di `SCR-AST-REG-01`, `SCR-AST-INV-07` |
| Contract version | `AST-INV-1.0.0`; layar mengikuti **source** `BE-AST-024` termasuk deltanya (`cards[]`, `followUpCounts`, saringan `overdue`/`hasOpenFollowUps`, `filters/metadata`) |
| Wewenang UI | Tata letak `DEV_DISCRETION` dalam pola dashboard dan laporan aset yang sudah ada (`FE-AST-014`) |
| Dependency | `FE-AST-017` ✅, `BE-AST-024` ✅, `FE-AST-010` [FE-REG] ✅ |
| Klasifikasi | `STANDARD` — 1 bagian dashboard + 1 hook baru, varian `report` pada daftar sesi, 1 route, 1 butir menu, 2 tab jadwal |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-24, berpasangan dengan `BE-AST-024` |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `736911664` (branch `RizkiV2`) |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar (Playwright **15/15** + uji hak akses **6/6**); build produksi Rizki `BUILD_ID` 08:51:56 |

---

## 1. Proses bisnis dari sisi pengguna

| Tempat | Yang dilihat / dilakukan |
|---|---|
| **Dashboard Aset** (`AssetStockTake : Read`) | Bagian **Inventaris Tertunda** sesudah Mutasi & Peminjaman Berjalan: kalimat ringkas "N inventaris tertunda: X opname lewat jadwal dan Y selisih belum ditindaklanjuti" dan dua kartu. Ikut saringan unit dashboard. Tanpa hak opname, bagian ini tidak tampil dan API-nya tidak dipanggil |
| Kartu **Opname Lewat Jadwal** | Membuka Jadwal Inventaris tab **Lewat Jadwal** (`overdue=true`) |
| Kartu **Selisih Belum Ditindaklanjuti** | Membuka Jadwal Inventaris tab **Selisih Belum Ditindaklanjuti** (`hasOpenFollowUps=true`); dari rincian sesi pengguna menuju Selisih Inventaris |
| **Laporan Inventaris** (menu grup Laporan Manajemen Aset, `AssetReport : Read`) | Tabel per sesi: nomor, unit/ruangan, rencana, status (+ Lewat Jadwal), dipindai/diharapkan, Sesuai (+ dipinjam), Tidak Ditemukan, Beda Lokasi, Beda Kondisi, Tidak Terdaftar, Usulan Tindak Lanjut (terbuka · diproses · diabaikan). Tab status yang sama dengan jadwal, saringan unit/tahun/cari, **Ekspor Excel** mengikuti saringan. Klik dua kali membuka rincian sesi bila pengguna punya hak baca opname |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/asset-management/stock-take/asset-stock-take-dashboard-section.jsx` | Baru — bagian Inventaris Tertunda |
| `src/lib/hooks/corporate/asset-management/stock-take/use-asset-stock-take-dashboard.jsx` | Baru — muat ringkasan bila berhak, kartu bertautan, coba lagi |
| `src/components/view/corporate/asset-management/register/asset-dashboard-view.jsx` | Sisipkan bagian sesudah bagian mutasi; kartu lain tidak diubah |
| `src/app/corporate/asset-management/reports/stock-takes/page.jsx` | Baru — route laporan (`?tab=` didukung) |
| `asset-stock-take-list-view.jsx`, `use-asset-stock-take-list.jsx` | Varian `report`: sumber data laporan, tanpa kartu ringkasan jadwal, tombol Ekspor Excel, klik dua kali hanya bila berhak |
| `asset-stock-take-table-columns.jsx` | + `buildStockTakeReportColumns` |
| `asset-stock-take-constants.jsx` | + base API dashboard/laporan, `ASSET_STOCK_TAKE_LIST_VARIANTS` (salinan teks jadwal dipindah ke sini), tab Lewat Jadwal dan Selisih Belum Ditindaklanjuti, kartu dashboard dan tautannya |
| `asset-stock-take-utils.js` | Tab baru → `overdue` / `hasOpenFollowUps` |
| `asset-stock-take-service.js` | + ringkasan dashboard, laporan, ekspor (pola `asset-transfer-service`) |
| `src/utils/menu-sidebar/menu-items.jsx` | + **Laporan Inventaris** (`RiSurveyLine`, `AssetReport : Read`) |

Tanpa baris komentar; tanpa paket baru; tanpa CSS baru.

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Bagian dashboard, kartu bertautan, pesan galat + coba lagi | `BaseDetailSection`, `SummaryGrid`, `InformationAlert`, `BaseButton` (sama dengan bagian mutasi) | `REUSE` |
| Hero, tab + cari + saringan, tabel, paginasi, toast, gerbang hak | `Hero`, `DataFilter`, `FilterSelect`, `AssetMasterResourceFilter`, `DataTable`, `Pagination`, `ToastStack`, `AccessDeniedGate` (view daftar sesi yang sudah ada) | `REUSE` |
| Lencana status | `StatusBadge` lewat `stockTakeStatusBadge` | `REUSE` |
| Ekspor Excel | `BaseButton` + `saveAssetFile` (pola laporan mutasi) | `REUSE` |

`UI GATE: 4 elemen — REUSE 4, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0`

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 11 berkas task (`--max-warnings=0`) | 0 galat, 0 peringatan |
| `npm run build` (Rizki) | Berhasil, `BUILD_ID` 2026-11-24 08:51:56; manifest memuat `/corporate/asset-management/reports/stock-takes` |
| Grep anti-regresi | Tanpa CSS baru, tanpa `<button>`/`<table>` mentah, tanpa komentar |

**MANUAL TEST (Playwright, Edge, 1600×900): 15/15** —
`QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-018-20261124/uji-fe-018.mjs`, `hasil-uji.jsonl`,
`jaringan-api.json`, tangkapan `01..04-*.png` (diperiksa langsung). Server frontend yang dipakai adalah server Rizki
(`next dev`, dinyalakan 08:54 sesudah build); data kartu disiapkan lewat API dengan sesi bertanggal lampau `AST-OPN-000023`.

| Langkah | Hasil |
| --- | --- |
| `AC1` Dashboard menampilkan Inventaris Tertunda; kartu = API (lewat jadwal 1, selisih 0) | Lulus |
| `AC1` Kondisi Aset, Mutasi & Peminjaman Berjalan, Aset Per Unit tetap tampil | Lulus |
| Kartu Opname Lewat Jadwal → `schedules?tab=lewat-jadwal`, permintaan `overdue=true`, sesi tampil | Lulus |
| Tab Selisih Belum Ditindaklanjuti → `hasOpenFollowUps=true` | Lulus |
| `AC2` Laporan Inventaris tampil (23 sesi) | Lulus |
| Baris `AST-OPN-000020` memuat hitungan dari API (tidak ditemukan 20; "0 terbuka · 1 diproses · 20 diabaikan") | Lulus |
| `AC3` butir menu Laporan Inventaris | Lulus |
| Tab Diverifikasi → laporan `status=Diverifikasi` | Lulus |
| `AC2` Ekspor Excel → `export?status=Diverifikasi` tanpa `pageSize`, berkas `.xlsx` terunduh, toast berhasil | Lulus |
| Klik dua kali baris → rincian sesi | Lulus |
| Halaman laporan tidak memanggil ringkasan jadwal | Lulus |
| Akun tanpa hak → kartu dan menu tidak tampil | Lulus, tetapi halaman dashboard seluruhnya ditolak (akun tanpa `AssetDashboard : Read`) — lihat uji tambahan |
| Tanpa galat halaman dan tanpa respons 5xx | Lulus |
| Rapikan sesi; kartu kembali ke angka awal | Lulus |

**Uji hak akses tambahan: 6/6** — `cek-hak-dashboard.mjs`, `cek-hak-dashboard.json`, tangkapan `05`, `06`. Policy
akun penyetuju aset dibaca, ditambah sementara, lalu dipulihkan (6 izin awal = 6 izin akhir):

| Policy sementara | Hasil |
| --- | --- |
| + `AssetDashboard : Read` saja | Dashboard terbuka, Kondisi Aset tampil, Inventaris Tertunda **tidak** tampil, nol panggilan ke `physical-inventory/dashboard` |
| + `AssetDashboard : Read` + `AssetStockTake : Read` | Inventaris Tertunda tampil, ringkasan 200 |

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

Catatan tangkapan: footer menimpa sebagian tabel pada `02`, `03`, `06` adalah artefak `fullPage` pada elemen
berposisi tetap. Kolom laporan paling kanan tergulir di dalam tabel (perilaku bawaan `DataTable`).

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Kartu "Inventaris Tertunda" disisipkan ke dashboard tanpa mengubah kartu lain | Bagian + hook dashboard opname; `asset-dashboard-view.jsx` hanya +1 baris komponen | `AC1` ×2, tangkapan `01`, uji hak 6/6 | ✅ |
| 2. Laporan dan ekspor | Varian `report`, kolom laporan, ekspor | `AC2` ×2, baris hitungan, tab, tangkapan `03` | ✅ |
| 3. Butir menu "Inventaris" pada grup Laporan | `menu-items.jsx` — label **Laporan Inventaris** | `AC3` | ✅ (label, lihat 5) |

| DoD | Status |
|---|---|
| `V-FE` lint, build produksi, uji layar | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `BE-AST-024`, menunggu izin Rizki |

---

## 5. Catatan

- Label menu memakai **Laporan Inventaris** (nama `SCR-AST-INV-07` di `03-frontend-architecture.md` dan sejajar dengan
  "Laporan Mutasi"); kriteria roadmap menulis "Inventaris". Grup yang sama sudah punya "Daftar Inventaris Per-unit",
  sehingga "Inventaris" saja mudah tertukar. Bila Rizki memilih "Inventaris", cukup ubah satu label.
- Jadwal Inventaris kini punya 10 tab dan terbungkus dua baris pada lebar 1600 px.
- Nama berkas unduhan memakai cadangan frontend (`laporan-inventaris-aset-YYYYMMDD.xlsx`) karena `Content-Disposition`
  tidak diekspos CORS — temuan yang sama dengan laporan register dan mutasi.

---

## 6. Langkah berikutnya

- Sub-modul `inventarisasi` lengkap (BE 5/5, FE 4/4). Berikutnya jalur `nilai`.
