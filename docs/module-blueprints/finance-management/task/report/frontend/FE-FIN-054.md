# Laporan Perubahan Frontend — `FE-FIN-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-054` |
| Judul | Petugas Finance mencari, menyaring, dan membuka kembali Invoice Supplier dan Retur Pembelian lewat daftar berpaging, tanpa harus mengetahui ID dokumen |
| Slice | `REV-22` (Finance AP mengikuti BLM, sisi frontend) — paket `FE-AP-03` pada dokumen konsolidasi audit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22", tabel Task REV-22 Frontend |
| Trace | `AP-UAT-21`; `AUD-AP-04` (filter jatuh tempo aktual, estimasi terpisah); `FIN-API-1.1` §B.4 dan §B.5; `FIN-DEC-045` |
| Contract version | `FIN-API-1.1` §B.4 `GET /purchasing-invoices` (filter supplier, status, jatuh tempo; respons bertambah `estimatedDueDate`, `actualDueDate`, `dueDateBasis`, `payableStatus`) dan §B.5 `GET /supplier-returns` — delta kontrak `BE-FIN-122`/`BE-FIN-127` |
| Wewenang UI | `DEV_DISCRETION` untuk pilihan kolom; mengikat untuk pemisahan kolom Estimasi dan Aktual jatuh tempo, label "Jatuh tempo aktual", dan pemilihan invoice sumber retur dari invoice berstatus Disetujui |
| Dependency | `BE-FIN-122` 🟡 [BE], `BE-FIN-127` 🟡 [BE] — source ada, belum di-build dan belum dijalankan terhadap PostgreSQL |
| Klasifikasi | `MEDIUM` — 2 route, 2 view, 2 berkas kolom, 3 hook (2 daftar, 1 resolver), 2 berkas konstanta, 1 registri select, 2 view yang disesuaikan (form retur, deposit) |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `7257982bb` (branch `yasmina`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `cfe22fe8` (branch `Yasmina`; perubahan `BE-FIN-127` belum di-commit) |
| Tanggal | 26 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — daftar Invoice Supplier dan Retur Pembelian selesai di source; **daftar Pembayaran belum dikerjakan**; build dan uji manual belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

1. `/finance/purchasing/purchasing-invoices` (menu **Invoice Supplier**) hanya halaman petunjuk berisi tautan, bukan tabel dokumen. Dokumen yang sudah dibuat hanya bisa dibuka bila petugas tahu ID-nya
   atau datang dari Tukar Faktur / laporan.
2. `/finance/purchasing/supplier-returns` (menu **Retur Pembelian Supplier**) langsung dialihkan (`redirect`) ke Daftar **Deposit** Retur. Dokumen retur itu sendiri tidak punya daftar.
3. Form **Catat Retur Pembelian** hanya dapat dipakai bila dibuka dari tombol pada detail invoice (komentar kode: "backend tidak punya GET / Purchasing Invoice"); tanpa itu layar hanya menampilkan peringatan.
4. Backend sekarang menyediakan `GET /purchasing-invoices` (`BE-FIN-122`, filter jatuh tempo `BE-FIN-127`) dan `GET /supplier-returns` (`BE-FIN-122`).
5. Daftar Pembayaran (`GET /payments`, `BE-FIN-122`) juga tersedia di backend, tetapi **tidak dikerjakan di task ini** (lihat bagian 8).

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: staf Finance (hak `FinancePurchasingInvoice : Read`, `FinanceSupplierReturn : Read`).

**Daftar Invoice Supplier**
1. Staf membuka **Transaksi A/P → Invoice & Pengakuan Utang → Invoice Supplier**. Tabel menampilkan nomor invoice, supplier, total, **Estimasi Jatuh Tempo** (dari Tukar Faktur), **Jatuh Tempo Aktual**
   (dari utang yang dibentuk invoice, beserta dasarnya: Tanggal Faktur Supplier / Tanggal Barang Diterima), Status Utang, dan Status invoice.
2. Staf menyaring menurut supplier, status, dan rentang **jatuh tempo aktual**. Contoh: rentang 1–30 November 2026 hanya menampilkan invoice yang utangnya jatuh tempo pada rentang itu;
   invoice yang belum disetujui (belum membentuk utang) tidak tampil selama rentang diisi — keterangan ini tertulis di bawah judul filter.
3. Klik dua kali sebuah baris membuka detail invoice (termasuk invoice Dibatalkan — histori tidak hilang). **+ Susun Purchasing Invoice** membuka form; **Laporan Pembelian** membuka laporan.
4. **Jalur tidak normal:** tanggal awal melebihi tanggal akhir → "Tanggal awal jatuh tempo tidak boleh melebihi tanggal akhir." tampil, tabel dikosongkan, tidak ada permintaan; gagal memuat → pesan dan notifikasi galat; tanpa hak → halaman akses ditolak; data kosong → "Data Invoice Supplier tidak ditemukan."

**Daftar Retur Pembelian**
5. Staf membuka **Pengadaan & Penerimaan → Retur Pembelian Supplier**. Tabel menampilkan nomor retur, invoice asal beserta nama supplier, alasan, nilai pokok, PPN retur, dan status; dapat disaring menurut supplier dan status.
   Klik dua kali membuka detail retur. Tombol **Deposit Retur** menuju Daftar Deposit Retur (yang kini di `/supplier-returns/deposits`, judulnya "Deposit Retur Supplier").
6. **+ Catat Retur Pembelian** tanpa invoice terpilih menampilkan **pemilih Purchasing Invoice berstatus Disetujui**; memilih satu invoice melanjutkan ke form rincian retur seperti sebelumnya.

**Penerimaan Invoice**
7. `?stage=intake` tetap membuka halaman petunjuk lama. Lihat bagian 8 mengenai batas butir menu ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/frontend/*`; pola daftar referensi `invoice-exchange-view.jsx`, `use-invoice-exchange-list.jsx`, `invoice-exchange-table-columns.jsx`, `supplier-return-deposit-*`, `goods-receipt-view.jsx`;
`purchasing-invoice-landing-view.jsx`, `menu-items.jsx` (butir menu), `left-sidebar-items-virtualized.jsx`/`left-sidebar-menu-handle.jsx` (penanganan query string), `finance-select-resources.js`, `use-supplier-name-resolver.jsx`;
backend `FinancePurchasingInvoiceDtos.cs` (`PurchasingInvoiceQuery`, `PurchasingInvoiceListItemResponse`), `FinanceSupplierReturnDtos.cs` (`SupplierReturnQuery`), `FinancePurchasingInvoiceService.GetPagedAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/finance/purchasing/purchasing-invoices/page.jsx` | Indeks route menampilkan Daftar Invoice Supplier; `?stage=intake` tetap menampilkan halaman petunjuk lama |
| `src/components/view/finance/purchasing/purchasing-invoice/purchasing-invoice-view.jsx` (**baru**) | Hero, `DataFilter` (dua `FilterDatePicker`, supplier, status, jumlah baris, reset), `InformationAlert`, `DataTable` + `Pagination`, `ToastStack`, `AccessDeniedGate` |
| `src/components/view/finance/purchasing/purchasing-invoice/purchasing-invoice-table-columns.jsx` (**baru**) | Kolom: No, Nomor Invoice, Supplier, Total, Estimasi Jatuh Tempo, Jatuh Tempo Aktual (+ dasar), Status Utang, Status |
| `src/lib/hooks/finance/purchasing/use-purchasing-invoice-list.jsx` (**baru**) | Filter `supplierId`, `status`, `dueDateFrom`, `dueDateTo`, paging; penjaga rentang terbalik; navigasi buka/susun/laporan |
| `src/app/finance/purchasing/supplier-returns/page.jsx` | Tidak lagi `redirect`; menampilkan Daftar Retur Pembelian |
| `src/components/view/finance/purchasing/supplier-return/supplier-return-view.jsx` dan `supplier-return-table-columns.jsx` (**baru**) | Daftar retur: filter supplier dan status, tombol Deposit Retur dan Catat Retur |
| `src/lib/hooks/finance/purchasing/use-supplier-return-list.jsx` dan `use-purchasing-invoice-resolver.jsx` (**baru**) | Hook daftar retur; resolver nomor dan supplier invoice asal per Id (pola identik resolver PO) |
| `src/components/view/finance/purchasing/supplier-return/deposit/supplier-return-deposit-view.jsx` | Judul "Deposit Retur Supplier" dan breadcrumb dua tingkat, supaya tidak ganda dengan Daftar Retur |
| `src/components/view/finance/purchasing/supplier-return/supplier-return-form-view.jsx` dan `src/lib/hooks/finance/purchasing/use-supplier-return-editor.jsx` | Pemilih invoice sumber bila belum ada `purchasingInvoiceId`; `selectSourceInvoice` hanya mengganti query string |
| `src/lib/hooks/select/finance/finance-select-resources.js` dan `use-finance-select.js` | Resource `purchasingInvoices` dan `useSelectPurchasingInvoices` |
| `src/lib/constants/finance/purchasing/purchasing-invoice-constants.jsx`, `supplier-return-constants.jsx` | Opsi status, status utang, label dasar jatuh tempo, route dasar; komentar "backend tanpa daftar" diperbarui |
| `src/lib/hooks/finance/purchasing/use-supplier-return-detail.jsx` | Komentar basis diperbarui |

### 3.3 Kepatuhan arsitektur frontend

Urutan section daftar mengikuti pola baku (Hero, DataFilter, InformationAlert, DataTable, ToastStack) dan dibangun persis seperti `invoice-exchange-view.jsx`; definisi kolom di berkas `*-table-columns.jsx`;
route tipis di `src/app`. Resolver nomor invoice mengikuti pola resolver PO/supplier yang sudah ada (Id → rincian satu per satu karena tidak ada endpoint batch). Tidak ada CSS, token, atau komponen dasar baru.

**UI GATE: 11 elemen — REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Header halaman + breadcrumb | `Hero`, `FinanceBreadcrumb` | `invoice-exchange-view.jsx` | REUSE |
| Filter, reset, tombol aksi | `DataFilter`, `BaseButton` | `invoice-exchange-view.jsx` | REUSE |
| Rentang tanggal | `FilterDatePicker` | `supplier-payable-movements-view.jsx` | REUSE |
| Supplier | `ResourceFilterSelect` + `useSelectSuppliers` | `invoice-exchange-view.jsx` | REUSE |
| Status dan jumlah baris | `FilterSelect` | `invoice-exchange-view.jsx` | REUSE |
| Tabel + paginasi | `DataTable`, `Pagination` | `invoice-exchange-view.jsx` | REUSE |
| Badge status | `StatusBadge` | `invoice-exchange-table-columns.jsx` | REUSE |
| Galat halaman, akses, notifikasi | `InformationAlert`, `AccessDeniedGate`, `ToastStack` | `invoice-exchange-view.jsx` | REUSE |
| Pemilih invoice sumber retur | `ResourceFilterSelect` | form Tukar Faktur memakai pola yang sama | REUSE |
| Pemberitahuan invoice dibatalkan | `InformationAlert` (`closable={false}`) | `information-alert.jsx` | REUSE |
| Dialog batalkan invoice | `ConfirmModal` | `purchasing-invoice-detail-view.jsx` | REUSE |

Nol elemen non-`REUSE`, sehingga tidak ada keputusan yang menunggu pengguna.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel dengan teks "Mengambil data Invoice Supplier..." / "Mengambil data Retur Pembelian..." |
| Kosong | "Data Invoice Supplier tidak ditemukan." / "Data Retur Pembelian tidak ditemukan." beserta saran mengganti filter |
| Gagal | Kotak galat merah berisi pesan backend dan notifikasi; rentang tanggal terbalik menampilkan pesan khusus |
| Tanpa hak akses | `AccessDeniedGate` menampilkan halaman akses ditolak standar |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/purchasing-invoices` | Daftar berpaging (`supplierId`, `status`, `dueDateFrom`, `dueDateTo`, `pageNumber`, `pageSize`); juga sumber pemilih invoice sumber retur (`status=APPROVED`) | `FinancePurchasingInvoice : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}` | Nomor dan supplier invoice asal per baris retur | `FinancePurchasingInvoice : Read` |

#### Corporate / Finance Management / Purchasing / Supplier Return

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/supplier-returns` | Daftar retur berpaging (`supplierId`, `status`, `pageNumber`, `pageSize`) | `FinanceSupplierReturn : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/supplier-returns/deposits` | Daftar Deposit Retur (tidak diubah) | `FinanceSupplierReturn : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 28 berkas yang diubah/ditambah (seluruh `REV-22` frontend) | 0 error; 8 warning: `react-hooks/set-state-in-effect` x7 dan `react-hooks/refs` x1 | `PASS` untuk error; `EXISTING WARNING` | Hook sejenis yang **tidak diubah** memberi aturan dan pola yang sama (`use-invoice-exchange-list.jsx`, `use-purchase-order-number-resolver.jsx`, `use-supplier-return-deposit-list.jsx`). Tiga warning berasal dari berkas baru yang menyalin pola itu (`use-purchasing-invoice-list.jsx`, `use-supplier-return-list.jsx`, `use-purchasing-invoice-resolver.jsx`) |
| `node --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-ap-blm.test.mjs` | 16 test, 16 lulus | `PASS` | Task ini tidak punya logika murni baru, jadi tidak ada test khusus |
| `npm run test:unit` | Skrip npm gagal: "Could not find `…\tests\unit\*.test.mjs`" (Node 20.20.2 pada Windows tidak meng-expand glob) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Suite penuh dengan glob yang di-expand shell | 3851 test: 3809 lulus, 42 gagal pada 28 berkas test (CRM, bangsal/pra-operasi, hemodialisa, rawat inap, IGD, menu) | `UNRELATED EXISTING ISSUE` (tanpa pembanding baseline) | Tidak satu pun dari 28 berkas itu memuat rujukan ke berkas purchasing yang diubah; baseline bersih tidak dijalankan karena `git stash`/`checkout` dilarang `AGENTS.md` |
| Grep anti-regresi pada JSX baru (tombol non-base, `<table>` mentah, `fw-*`/`fs-*`, inline style, warna literal, `!important`) | 0 temuan pada empat berkas JSX baru; tidak ada berkas CSS yang ditambah atau diubah | `PASS` | Perintah `grep` pada empat berkas |
| Grep pada baris yang ditambahkan ke berkas lama (`git diff -U0`) | 5 temuan dipertahankan: tiga label `form-label small fw-semibold` (Bea Materai, Ongkos Kirim, pemilih invoice sumber retur) mengikuti label tetangga pada form yang sama; satu `<div className="fw-semibold">` nomor GR yang hanya bergeser indentasi (baris lama); satu `<table>` baru (rincian GR) yang membawa `data-flat-table="true"`. Tidak ada `<button>`, `btn-*`, `fs-*`, inline style baru, warna literal, atau `!important` | Dipertahankan dengan alasan | Alasan: konsistensi dengan markup berdampingan; tidak ada pengganti base component untuk tabel isian padat (lihat komentar berkas) |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pengguna: build dilakukan sendiri; build juga menimpa `.next` yang dipakai server lokal |
| Uji manual di browser (filter, rentang tanggal, paging, reset, buka detail, pemilih invoice sumber retur) | Tidak dijalankan | `NOT FEASIBLE` | Dev server dan skrip browser tidak dinyalakan tanpa diminta; backend `BE-FIN-122`/`127` belum di-build |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, Playwright, `npm run test:e2e`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Indeks Invoice Supplier berupa tabel berpaging bersaring (supplier, status, rentang jatuh tempo aktual) dengan Estimasi dan Aktual terpisah, dasar jatuh tempo, dan status utang; klik dua kali membuka detail | Terpenuhi (source) | `purchasing-invoice-view.jsx`, `purchasing-invoice-table-columns.jsx`, `use-purchasing-invoice-list.jsx` |
| 2. Rentang tanggal terbalik dicegah di layar | Terpenuhi (source) | `fetchList` (penjaga) |
| 3. `?stage=intake` tetap membuka halaman petunjuk lama | Terpenuhi (source) | `purchasing-invoices/page.jsx` |
| 4. `/supplier-returns` berupa daftar retur berpaging (supplier, status), tidak lagi `redirect`; Daftar Deposit tetap tersedia | Terpenuhi (source) | `supplier-returns/page.jsx`, `supplier-return-view.jsx`, tombol Deposit Retur |
| 5. Form Catat Retur tanpa invoice menampilkan pemilih invoice berstatus Disetujui | Terpenuhi (source) | `supplier-return-form-view.jsx`, resource `purchasingInvoices` |
| 6. Memuat, kosong, galat, dan akses ditolak tampil | Terpenuhi (source) | Bagian 4 |
| 7. Daftar Pembayaran operasional | **Belum terpenuhi** | Tidak dikerjakan; lihat bagian 8 |
| DoD: lint PASS, laporan tracked | Terpenuhi untuk error lint (0) | Bagian 6 |
| DoD: build PASS, uji manual runtime | **Belum terpenuhi** | `NOT RUN` / `NOT FEASIBLE` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemilih invoice sumber retur memuat satu halaman (maksimal 25) invoice berstatus Disetujui tanpa pencarian teks: `PurchasingInvoiceQuery` tidak punya parameter pencarian, dan pemilih tidak memakai `serverSide` (alasan sama dengan resource Finance lain). Dengan lebih dari 25 invoice Disetujui, jalur utama tetap tombol **Catat Retur Pembelian** pada detail invoice. Perilaku muat-lanjut `ResourceFilterSelect` belum diuji di browser |
| Masalah yang diketahui | **(1) Menu "Penerimaan Invoice" tidak berbeda dari "Invoice Supplier".** `menu-items.jsx` mengarahkan ke `...?stage=intake`, tetapi sidebar membuang query string (`normalizeRoutePath` memotong `?…`), sehingga klik menu itu membuka indeks yang sama dengan menu Invoice Supplier. Membedakannya memerlukan route terpisah dan keputusan proses intake (paket `FE-AP-05`), maka tidak dikerjakan; nama menu tidak diubah. **(2) Daftar Pembayaran** (`GET /payments`) tidak dikerjakan: halaman `/finance/payable/payment` adalah layar membayar utang, belum punya detail pembayaran maupun antrean persetujuan; menambah daftar tanpa itu belum menutup kebutuhan "verifikasi alur per role" (`FE-AP-07`). **(3)** Kolom Supplier pada daftar retur memerlukan satu permintaan rincian invoice per baris (tidak ada endpoint batch) |
| Dependency backend | `BE-FIN-122` 🟡 dan `BE-FIN-127` 🟡 belum di-build dan kueri `EXISTS` jatuh tempo belum dijalankan terhadap PostgreSQL |
| Perubahan sampingan | Judul halaman Daftar Deposit Retur berubah dari "Retur Pembelian Supplier" menjadi "Deposit Retur Supplier" (agar tidak ganda dengan daftar baru) |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada `QuilvianSystemFrontendDev` (branch `yasmina`, upstream `origin/yasmina`, HEAD `7257982bb`): 18 berkas `M` (src), 10 berkas `??` (9 src + 1 test); tidak ada stage, commit, push, atau perubahan branch |
| Langkah berikutnya | Jalankan `npm run build`, terapkan migration backend, lalu uji manual: filter rentang 1–30 November, invoice Draf tidak tampil saat rentang diisi, buka invoice Dibatalkan, buka daftar retur, catat retur lewat pemilih invoice |
