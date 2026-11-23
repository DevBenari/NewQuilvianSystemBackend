# Laporan Perubahan Frontend — `FE-AST-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-016` |
| Judul | Pemindaian QR |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/frontend-roadmap.md` — kartu `FE-AST-016` |
| Trace | `CAP-07.2`, `AST-FE-007` (menggantikan `AST-FE-003`); `SCR-AST-INV-02`; `03-frontend-architecture.md` §3.1, §7; `AT-INV-13`, `38` |
| Contract version | `AST-INV-1.0.0`; layar mengikuti **source** `BE-AST-022` termasuk deltanya (`GET /{id}/lookup`, `expected-items` berbentuk `StockTakeExpectedItemResponse`, `findingCount`) |
| Wewenang UI | Tata letak `DEV_DISCRETION`; acuan tablet ±768 px (`AST-FE-007`, keputusan Rizki 2026-11-23); pustaka pemindai `DEV_DISCRETION` dengan syarat tanpa layanan luar |
| Dependency | `FE-AST-015` ✅, `BE-AST-022` ✅ |
| Klasifikasi | `STANDARD` — 1 route, 1 komponen baru (ditetapkan arsitektur), 1 hook, 1 view, tambahan kolom/konstanta/service/utilitas, 1 butir menu |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-23 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `0658715f5` (branch `RizkiV2`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — kelima kriteria terpetakan ke source dan teruji di layar (Playwright **16/16**, viewport tablet + kamera tiruan); `npm run build` ulang Rizki `BUILD_ID` 15:02:39 memuat perbaikan `qr-scanner.jsx` (bagian 5.1, commit `7baebf2f4`) |

---

## 1. Proses bisnis dari sisi pengguna

| Langkah | Yang dilihat / dilakukan anggota tim |
|---|---|
| Buka **Pemeriksaan Fisik** (menu, butuh `AssetStockTake : Scan`) atau tombol **Buka Pemeriksaan Fisik** di detail/daftar sesi | Tanpa `?sessionId=`: pemilih sesi berisi sesi `Berjalan` tempat pengguna menjadi anggota tim |
| Penghitung | Dipindai X / Y, Belum Dipindai, Temuan Tidak Terdaftar, Belum Terkirim |
| Pilih **Ruangan saat ini** | Dipakai sebagai ruangan ditemukan untuk setiap pindaian sampai diganti |
| Arahkan kamera ke QR label | Nomor terbaca → **Hasil Pindai**: nomor, nama, kelompok, ruangan & kondisi tercatat, status pinjaman, pindaian sebelumnya. Isi ruangan ditemukan (otomatis = ruangan saat ini; untuk aset yang dipinjam keluar, pilihan ruangan unit peminjam), kondisi (bawaan = kondisi tercatat), foto opsional, catatan → **Simpan Pindaian** |
| Kamera tidak bisa dipakai | Pesan "Kamera tidak dapat dipakai. Ketik nomor aset."; ketik nomor → **Cari** |
| Nomor tidak dikenal | **Nomor Tidak Dikenal** → **Catat sebagai Temuan**: nomor terisi, keterangan + foto wajib, ruangan, kondisi |
| Gagal kirim (koneksi/server) | Pindaian masuk **Belum Terkirim** beserta kendalanya; **Kirim ulang** / **Buang** di menu baris |
| Daftar | **Pindaian Terakhir** (10 terbaru, kamera/diketik, digantikan) dan **Lihat yang Belum Dipindai** |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/asset-management/qr-scanner.jsx` | Baru — membungkus `react-qr-scanner` (dimuat tanpa SSR), kamera belakang (`facingMode: environment`), hanya dirender bila browser memberi akses kamera; kode sama diabaikan 2,5 detik |
| `src/style/corporate/asset-management/qr-scanner.module.css` | Baru — bingkai kamera dan placeholder memakai token |
| `src/components/view/corporate/asset-management/stock-take/asset-stock-take-scan-view.jsx` | Baru — layar `SCR-AST-INV-02` |
| `src/lib/hooks/corporate/asset-management/stock-take/use-asset-stock-take-scan.jsx` | Baru — sesi, pemilih sesi, kamera, lookup, form pindaian, temuan, antrean belum terkirim, daftar |
| `src/app/corporate/asset-management/stock-take/scan/page.jsx` | Baru — route `?sessionId=` (divalidasi UUID) |
| `asset-stock-take-table-columns.jsx` | + kolom belum terkirim, pindaian, belum dipindai; aksi baris "Buka pemeriksaan fisik" |
| `asset-stock-take-detail-view.jsx`, `-list-view.jsx`, `use-asset-stock-take-{actions,detail,list,route}.jsx` | Tombol/aksi ke layar pindai; hak `Scan` |
| `asset-stock-take-service.js`, `-constants.jsx`, `-utils.js` | Panggilan lookup/pindai/temuan/riwayat/diharapkan; `FormData`; konstanta field |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir **Pemeriksaan Fisik** (`RiQrScan2Line`, `AssetStockTake : Scan`) |

Tanpa baris komentar; tanpa paket baru (`react-qr-scanner` sudah ada di `package.json`).

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Hero, penghitung, bagian, tabel, toast | `Hero`, `SummaryCards`, `BaseDetailSection`, `DataTable`, `ToastStack` | `REUSE` |
| Data tercatat aset | `BaseDetailCard` (`showHeader=false`) | `REUSE` |
| Pemilih sesi, kondisi | `FilterSelect` | `REUSE` |
| Ruangan saat ini / ditemukan | `AssetMasterResourceFilter` dalam `BaseFormControl` | `REUSE` |
| Nomor manual, catatan, keterangan | `BaseInputField`, `BaseTextAreaField` | `REUSE` |
| Foto | `AssetConditionPhotoField` (form laporan kondisi) | `REUSE` |
| Formulir temuan | `ConfirmModal` dengan isi field (pola terima mutasi) | `REUSE` |
| Aksi baris belum terkirim | `RowActionMenu` | `REUSE` |
| Pemindai QR kamera | `qr-scanner.jsx` | `NEW` — ditetapkan `03-frontend-architecture.md` (Komponen baru), bukan keputusan task ini |

Pustaka pemindai (`DEV_DISCRETION`):
1. **`react-qr-scanner` 1.0.0-alpha.11 yang sudah terpasang** (dipilih, rekomendasi): dekoder zxing di web worker,
   `playsinline` untuk Safari iPadOS, tanpa layanan luar, tanpa unduhan paket. Risiko: alpha, peer React ≤ 18 (app
   router berjalan React 19 — lihat 5.1).
2. API `BarcodeDetector` bawaan browser: tanpa pustaka, tetapi tidak tersedia di Safari iPadOS maupun Chrome Windows,
   sehingga tablet iPad hanya bisa input manual.
3. Memakai `@zxing/library` langsung: kendali penuh, tetapi hanya dependensi turunan (tidak tercantum di
   `package.json`), jadi perlu mengubah manifest paket.

`UI GATE: 10 elemen — REUSE 9, COMPOSE 0, EXTEND 0, WRAP 0, NEW 1 (qr-scanner, ditetapkan arsitektur)`

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint berkas task | 0 galat, 0 peringatan (termasuk sesudah perbaikan 5.1) |
| `npm run build` (Rizki) | Berhasil, `BUILD_ID` 2026-11-23 14:34:59 (manifest memuat `/stock-take/scan`), lalu diulang `BUILD_ID` 15:02:39 sesudah perbaikan 5.1; bundel memuat `qrArea` |
| Grep anti-regresi | CSS baru tanpa warna literal, ukuran huruf hanya token, tanpa `!important`; tanpa `<button>`/`<table>` mentah |

**MANUAL TEST (Playwright, Edge, Dimas sebagai anggota tim, viewport 768×1024 + sentuhan): 16/16** —
`QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-016-20261123/uji-fe-016.mjs`, `hasil-uji.jsonl`,
tangkapan `01..05-*.png`. Kamera tiruan Edge (`--use-file-for-fake-video-capture`) memutar Y4M berisi QR nomor aset
nyata yang dibuat dari pustaka `qrcode` terpasang; layar `next dev` di `localhost`. Uji di tablet sungguhan lewat HTTPS
adalah bagian UAT (`AST-FE-007`).

| Langkah | Hasil |
| --- | --- |
| Menu Pemeriksaan Fisik + Jadwal Inventaris; pemilih sesi; sesi dipilih → URL `?sessionId=`, penghitung 0 / 44 | Lulus |
| `AT-INV-13` kamera membaca QR `AST-MBL-000034` → lookup → Hasil Pindai | Lulus |
| Simpan → multipart, `isManualEntry=false`, ruangan = ruangan saat ini, toast + penghitung 1 / 44 | Lulus |
| Nomor tak dikenal `AST-TIDAK-ADA-777` → 404 → Nomor Tidak Dikenal → Catat sebagai Temuan (nomor terisi), keterangan + foto → 200 | Lulus |
| `POST /scans` pertama digagalkan (`page.route` abort) → baris Belum Terkirim; Kirim ulang → 200, baris hilang | Lulus |
| Lihat yang Belum Dipindai → `scanned=false`, judul (42) | Lulus |
| Pindaian Terakhir memuat pindaian kamera dan diketik | Lulus |
| Lebar 768 px tanpa gulir samping halaman | Lulus |
| `AT-INV-38` `getUserMedia` ditolak (`NotAllowedError`) → pesan kamera; ketik nomor huruf kecil → lookup 200 → simpan 200 `isManualEntry=true` | Lulus |
| Tanpa galat halaman dan tanpa respons 5xx | Lulus |

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

Data uji dirapikan: sesi `AST-OPN-000011..000016` dibatalkan.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Skema §3.1; nyaman dipakai di tablet ±768 px | `asset-stock-take-scan-view.jsx` | Viewport tablet, tanpa gulir samping, tangkapan 01/03/04 | ✅ |
| 2. Kamera ditolak → input nomor manual tetap bekerja | `qr-scanner.jsx`, `use-asset-stock-take-scan.jsx` | `AT-INV-38` | ✅ |
| 3. Nomor tak dikenal → formulir temuan | View + hook | Langkah temuan | ✅ |
| 4. Pindaian gagal kirim tetap terlihat dengan tombol kirim ulang | Antrean `pending` | Langkah abort → kirim ulang | ✅ |
| 5. Butir menu "Pemeriksaan Fisik" | `menu-items.jsx` | Langkah menu | ✅ |

| DoD | Status |
|---|---|
| `V-FE` lint, uji layar | Terpenuhi |
| `V-FE` build produksi | Terpenuhi (`BUILD_ID` 15:02:39) |
| `UAT-INV-06`, tablet sungguhan | Milik tim UAT |
| Laporan, roadmap, traceability | Ditulis |

---

## 5. Temuan

### 5.1 `react-qr-scanner` tidak mendekode di React 19

Pustaka mengandalkan `defaultProps` untuk `qrArea` dan `resolution`. App router Next menjalankan React 19 yang tidak
lagi menerapkan `defaultProps` pada komponen fungsi, sehingga setiap frame melempar `"undefined" is not valid JSON` dan
tidak ada QR yang terbaca (video tetap tampil). Ditemukan lewat `probe-kamera.mjs`; diperbaiki dengan mengirim
`resolution={640}` dan `qrArea={[]}` secara eksplisit. Sesudahnya QR terbaca di percobaan pertama.

### 5.2 Perilaku kamera yang diperbaiki sebelum uji

- Kamera yang tetap mengarah ke label yang sama tidak lagi memicu pencarian ulang setiap 2,5 detik dan mengosongkan
  form: deteksi diabaikan selama panel hasil terbuka, dan kode yang baru disimpan diabaikan 10 detik.
- Memilih ruangan saat ini sesudah aset terbaca langsung mengisi ruangan ditemukan.

### 5.3 Tabel di lebar 768 px

Halaman tidak bergulir ke samping, tetapi kolom paling kanan tabel Belum Terkirim dan Pindaian Terakhir perlu digeser
di dalam tabel (perilaku bawaan `DataTable`). Menu aksi tetap terjangkau.

---

## 6. Langkah berikutnya

- `BE-AST-023` + `FE-AST-017` (pencocokan, selisih, verifikasi, berita acara, tindak lanjut).
