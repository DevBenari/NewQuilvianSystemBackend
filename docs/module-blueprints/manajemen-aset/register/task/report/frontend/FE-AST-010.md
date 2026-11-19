# Laporan Perubahan Frontend — `FE-AST-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-010` |
| Judul | Dashboard dan laporan register |
| Slice | `REG-FE-S2` — Fitur register |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-010` |
| Trace | `CAP-01.*`, `11.1`, `11.3`, `11.7`; `SCR-AST-REG-01`, `27`, `33`, `34`, `35`; `03-frontend-architecture.md` §3.1, §3.5; `02-module-map.md` §3.1 (Dashboard Aset, grup Laporan Manajemen Aset) |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** `BE-AST-013` termasuk deltanya (kartu ber-`filter`, `filters/metadata`, `AssetHistoryReportItemResponse`) |
| Wewenang UI | Tata letak dashboard, kartu, grafik `DEV_DISCRETION` (`AST-FE-005`); nama dan urutan menu `AST-FE-002` |
| Dependency | `FE-AST-006` ✅, `FE-AST-007` ✅, `BE-AST-013` 🟡 (endpoint lengkap; sisa uji positif `AT-REG-12` tidak mengubah layar) |
| Klasifikasi | `STANDARD` — 4 route baru, 1 route dialihkan, 1 route diperluas; tanpa perubahan backend |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-18 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `0d9b0140d` (branch `RizkiV2`) |
| Tanggal | 18–19 November 2026 |
| Status | ✅ SELESAI 19 November 2026 — keempat kriteria terpetakan ke source dan teruji di layar **24/24**; dua perbaikan kecil sesudah uji pertama diuji ulang 24/24 di server dev (`next dev`), belum masuk `BUILD_ID` |

---

## 1. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Yang terjadi di layar |
|---|---|---|
| 1 | Bagian Aset / Kepala Unit / Direktur | Menu **Manajemen Aset → Dashboard Aset** (`/corporate/asset-management`): kartu Total Aset, Menunggu Nomor Inventaris, Belum Ditempatkan, Sudah Ditempatkan, Aset Menganggur; bagian Kondisi Aset (5 kartu); tabel Aset Per Unit; tabel Aset Rusak Terbaru |
| 2 | Sama | Saring **unit** → semua kartu dihitung ulang untuk unit itu |
| 3 | Sama | Klik kartu → **Daftar Inventaris** terbuka dengan saringan yang sama (mis. `?recordStatus=Terdaftar&placementState=BelumDitempatkan`), jumlah baris = angka kartu. Klik dua kali baris per unit → Daftar Inventaris unit itu; klik dua kali aset rusak → detail aset |
| 4 | Bagian Aset / Auditor | Grup **Laporan Manajemen Aset**: Daftar Inventaris Per-unit, Kondisi Aset, History & Audit. Saringan unit/ruangan/kelompok/kondisi/penempatan (History: kejadian, sub-modul, periode), lalu **Ekspor Excel** mengikuti saringan |
| 5 | Unit / Bagian Aset | **Kondisi & Status Aset → Riwayat Perubahan Status** kini membaca History & Audit dengan kejadian "Kondisi Berubah" (keputusan Rizki saat `FE-AST-007`) |

| Keadaan tidak normal | Perilaku |
|---|---|
| Ringkasan gagal dimuat | Pesan + **Coba lagi**; kartu bernilai "–"; tabel Aset Per Unit tetap tampil |
| Per unit gagal dimuat | Pesan + **Coba lagi** di bagian itu; kartu tetap tampil |
| Tanpa hak `AssetRegister : Read` | Kartu dan baris tidak dapat diklik ke daftar |
| Tanpa `AssetDashboard`/`AssetReport : Read` | Menu tidak tampil; URL → Akses Ditolak |
| Ekspor gagal (mis. > 10.000 baris) | Toast pesan backend |
| Daftar kosong | "Belum ada aset terdaftar pada saringan ini." / "Belum ada aset ditempatkan." / "Tidak ada aset rusak." |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/register/asset-report-constants.jsx` | Baru — endpoint, route, varian laporan (`inventoryByUnit`, `condition`, `history`, `conditionHistory`), saringan, pilihan sub-modul |
| `src/lib/services/corporate/asset-management/asset-report-service.js` | Baru — ringkasan, per unit, laporan, ekspor (blob, batas waktu 5 menit) |
| `src/utils/corporate/asset-management/register/asset-report-utils.js` | Baru — `buildAssetInventoryHref` (saringan kartu → URL daftar) dan `parseAssetInventoryQuery` (URL → saringan awal, enum dan GUID divalidasi) |
| `src/lib/hooks/corporate/asset-management/register/use-asset-dashboard.jsx` | Baru — dua bagian dimuat terpisah dengan coba lagi masing-masing |
| `src/lib/hooks/corporate/asset-management/register/use-asset-report-list.jsx` | Baru — satu hook untuk keempat varian laporan + ekspor |
| `src/components/view/corporate/asset-management/register/asset-dashboard-view.jsx` | Baru |
| `src/components/view/corporate/asset-management/register/asset-report-list-view.jsx`, `asset-report-table-columns.jsx` | Baru |
| `src/app/corporate/asset-management/page.jsx` | Baru — Dashboard Aset di root modul sesuai `02-module-map.md` |
| `src/app/corporate/asset-management/reports/{inventory-by-unit,condition,history}/page.jsx` | Baru |
| `src/app/corporate/asset-management/condition/history/page.jsx` | Dialihkan ke varian `conditionHistory` |
| `src/app/corporate/asset-management/register/assets/page.jsx`, `asset-inventory-list-view.jsx`, `use-asset-inventory-list.jsx` | Daftar Inventaris menerima saringan awal dari URL |
| `src/utils/menu-sidebar/menu-items.jsx` | + Dashboard Aset (`AssetDashboard : Read`), + grup Laporan Manajemen Aset (3 butir, `AssetReport : Read`); hak Riwayat Perubahan Status → `AssetReport : Read` |

Nol baris komentar, nol inline style, tanpa CSS baru, tanpa base component yang diubah. Varian `history` lama di
`asset-condition-constants.jsx` tidak lagi dipakai route mana pun dan dibiarkan.

`UI GATE: 12 elemen — REUSE 11, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
|---|---|---|---|
| Kepala layar, saringan unit | `Hero`, `DataFilter`, `AssetMasterResourceFilter` | REUSE | — |
| Kartu ringkasan dan kondisi, kartu bertaut | `SummaryGrid` (`href`, `testId`) | REUSE | — |
| Bagian per unit dan aset rusak | `BaseDetailSection` + `DataTable` (`pagination={false}`) | COMPOSE | Pola `asset-handover-detail-view` |
| Badge kondisi | `StatusBadge` | REUSE | — |
| Tabel laporan, saringan, tanggal, jumlah baris | `DataTable`, `FilterSelect`, `FilterDatePicker`, `Pagination` | REUSE | Kolom 11.1/11.3 = `buildAssetInventoryColumns` |
| Ekspor, coba lagi | `BaseButton` | REUSE | — |
| Pesan, toast, akses ditolak | `InformationAlert`, `ToastStack`, `AccessDeniedGate` | REUSE | — |

**Grafik:** Aset Per Unit ditampilkan sebagai tabel. Rancangan menulis "tabel/grafik" dengan pilihan `DEV_DISCRETION`.
Pustaka grafik yang ada (`recharts`) dipakai modul lain dengan warna literal, bukan design token, jadi tabel dipilih.
Grafik bisa ditambah kemudian tanpa mengubah backend.

---

## 3. Endpoint yang dikonsumsi

`GET /register/dashboard/summary` (`organizationUnitId`), `/register/dashboard/by-unit`;
`GET /register/reports/{inventory-by-unit,condition,history}`, `/register/reports/{kode}/export` (blob);
`GET /register/assets` (Daftar Inventaris dengan saringan dari URL).

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` (17 berkas termasuk menu) | `0 errors, 0 warnings`; ulang atas 3 berkas sesudah perbaikan juga 0/0 | `PASS` | Keluaran perintah |
| `npm run build` | Dijalankan Rizki dan berhasil; `BUILD_ID` 2026-11-19 08:29:10; manifest memuat `/corporate/asset-management`, `/reports/inventory-by-unit`, `/reports/condition`, `/reports/history` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-010-20261119/uji-fe-010.mjs`) | **24/24**; 0 galat halaman, 0 respons 5xx nyata (superadmin, LOKET) | `PASS` | `hasil-uji.jsonl`, 9 tangkapan layar, 3 berkas ekspor |

Langkah yang lulus: menu Dashboard Aset + 3 laporan; dashboard tampil; **10 kartu = API** (79, 2, 32, 47, 0, 76, 1, 1,
1, 0); baris per unit 47; aset rusak terbaru; klik kartu Belum Ditempatkan → daftar 32 baris; saring unit Instalasi
Rawat Inap → 47; klik dua kali baris unit → daftar 47; **ringkasan dipaksa gagal (`page.route` 500) → kartu "–",
Coba lagi, tabel per unit tetap tampil**; Coba lagi → 79; per unit dipaksa gagal → kartu tetap tampil;
Daftar Inventaris Per-unit 79 baris tanpa kolom rupiah, ekspor 79 baris; Kondisi Aset saring Rusak Ringan → 1 baris,
ekspor 1 baris dengan keterangan saringan; History & Audit 101 → Kondisi Berubah 7 → sub-modul Register, ekspor 7 baris,
klik dua kali → detail aset; Riwayat Perubahan Status membaca `/reports/history?eventType=KondisiBerubah` tanpa
saringan kejadian; LOKET → Akses Ditolak dan tanpa menu laporan.

**Perbaikan sesudah uji pertama** (diuji ulang 24/24 di `next dev`, belum masuk `BUILD_ID`):

1. Kartu cadangan saat ringkasan gagal belum membawa `data-testid`. Ditemukan oleh skrip uji; tampilan pengguna tidak
   terpengaruh.
2. Keterangan "Aset Rusak Terbaru" menulis "0 aset" saat ringkasan gagal (`03-ringkasan-gagal.png`). Kini angka hanya
   ditulis bila data termuat.

Pada uji pertama, tiga langkah gagal karena skrip uji sendiri: grup menu tertutup tidak merender tautannya; cakupan baris
tabel salah; dan pemilih kondisi ikut mengetik ke kotak cari daftar. Ketiganya diperbaiki di skrip, bukan di source.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Bukti | Status |
|---|---|---|
| 1. Skema §3.5; kartu 1.4 dan 1.5 disisipkan kemudian oleh `FE-AST-014` dan `FE-AST-018` | Kartu ringkasan, kondisi, per unit, aset rusak; kartu dibangun dari daftar `PLACEHOLDER_CARDS` sehingga kartu Mutasi/Opname dapat ditambah sebagai bagian terpisah | ✅ |
| 2. Kartu yang gagal dimuat tidak menjatuhkan kartu lain | Dua bagian dimuat terpisah; uji gagal paksa dua arah + Coba lagi | ✅ |
| 3. Laporan 11.1, 11.3, 11.7 dengan ekspor | Tiga layar + ekspor mengikuti saringan, isi berkas = API | ✅ |
| 4. Butir menu Dashboard Aset dan tiga butir laporan terdaftar | `menu-items.jsx`; tautan ditemukan di sidebar | ✅ |

| DoD | Status |
|---|---|
| ESLint, build, uji layar, laporan, roadmap, traceability | Terpenuhi; dua perbaikan kecil sesudah uji belum masuk build produksi |
| `UAT-REG-11` | Milik tim UAT terpisah |
| Commit | Dilakukan bersama `BE-AST-013` atas perintah Rizki |
