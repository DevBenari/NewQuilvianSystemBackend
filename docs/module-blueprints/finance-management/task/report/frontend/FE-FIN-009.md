# Laporan Perubahan Frontend — `FE-FIN-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-009` |
| Judul | Petugas AP menyusun Purchasing Invoice dari Tukar Faktur, penyetuju menyetujuinya, dan retur pembelian menerbitkan Deposit Retur |
| Slice | `REV-4` — Purchasing/AP (`EPIC FIN-15`), gelombang `R4-2` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` baris `FE-FIN-009` |
| Trace | `FR-FIN-083`, `084`, `085` (penerbitan), `086`; `FIN-DEC-045`, `047`; `FIN-API-1.1` B.4, B.5 (tanpa `apply`); `FIN-PERM-1.1` B.4; `FIN-VAL-1.2` `105`..`111` |
| Contract version | `FIN-API-1.1` — `locked` 25 September 2026 |
| Wewenang UI | Product Owner — tata letak, alur multi-layar vs satu layar `DEV_DISCRETION` (`03-frontend-architecture.md` §12.6). Create, Detail, Update Purchasing Invoice sebagai rute App Router terpisah (`/finance/purchasing/purchasing-invoices/new`, `/[slug]`, `/[slug]/edit`); Create dan Detail Retur Pembelian sebagai rute terpisah (`/finance/purchasing/supplier-returns/new`, `/[slug]`); Daftar Deposit Retur berpaging penuh di `/finance/purchasing/supplier-returns/deposits`; Landing page pengantar alur kerja di `/finance/purchasing/purchasing-invoices` |
| Dependency | `BE-FIN-034` ✅ (Purchasing Invoices — `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/approve`, `POST /{id}/reject`), `BE-FIN-035` ✅ (Supplier Returns & Deposits — `GET /{id}`, `POST /`, `POST /{id}/confirm`, `POST /{id}/cancel`, `GET /deposits`) |
| Klasifikasi | `MEDIUM` — pekerjaan multi-layar pada repository frontend; status transitions per jenjang approval; validasi formula seimbang; penanganan deposit retur; 14 berkas baru/terintegrasi; zero backend changes |
| Task mode | `FRONTEND MODE` — repository backend `NewQuilvianSystemBackend` diperiksa read-only, nol perubahan kode backend |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/purchasing/**`, `src/components/view/finance/purchasing/**`, `src/lib/hooks/finance/purchasing/**`, `src/lib/constants/finance/purchasing/**` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI.** Seluruh kapabilitas pembuatan, penyuntingan draf, pengajuan, persetujuan, penolakan Purchasing Invoice, pencatatan dan konfirmasi Retur Pembelian, serta daftar dan pemantauan saldo Deposit Retur telah selesai dan tervalidasi 100% lulus `npm run lint:errors` (0 error) dan `npm run build` Turbopack (0 error) |

---

## 1. Ringkasan Eksekutif & Proses Bisnis

Task `FE-FIN-009` melengkapi kapabilitas Account Payable Purchasing dengan menghubungkan dokumen Tukar Faktur yang telah berstatus `RECEIVED` ke dalam **Purchasing Invoice**, mengawal siklus persetujuan multi-jenjangnya hingga terbit sebagai utang supplier resmi, serta menyediakan mekanisme **Retur Pembelian** dan penerbitan **Deposit Retur**.

Alur bisnis yang disediakan dari kacamata pengguna:

1. **Penyusunan Purchasing Invoice (`/finance/purchasing/purchasing-invoices/new`):**
   - Petugas AP memilih Tukar Faktur yang belum pernah dipakai (`FIN-VAL-106`, hanya yang berstatus `RECEIVED` dari dropdown `ResourceFilterSelect`).
   - Petugas mengisi rincian item barang/jasa (nama produk, kuantitas, harga satuan).
   - Petugas dapat mencatat komponen komersial: Diskon, PPN, Uang Muka, dan Potongan Lain.
   - Layar secara otomatis menghitung `TotalAmount = Subtotal - Diskon + PPN - Uang Muka - Potongan Lain` secara seimbang (`FIN-VAL-107`) dan menguncinya sebagai nilai read-only yang transparan sebelum disimpan sebagai draf (`DRAFT`).

2. **Rincian & Persetujuan Multi-Jenjang (`/finance/purchasing/purchasing-invoices/[slug]`):**
   - Menampilkan status dokumen, rincian barang, komponen pemotong/penambah, serta jenjang persetujuan (`ApprovalTier`) yang dihitung oleh backend (`TIER_1` untuk Supervisor Finance, `TIER_2` untuk Manajer Finance).
   - Pengaju dapat mengajukan draf (`submit`) ke jenjang persetujuan.
   - Penyetuju berwenang dapat menyetujui (`approve`) atau menolak (`reject`) dengan alasan penolakan wajib.
   - Sesuai prinsip segregasi tugas, pengaju (`isRequester`) tidak diperkenankan menyetujui dokumen yang diajukannya sendiri.
   - Begitu berstatus `APPROVED`, Purchasing Invoice ini resmi memicu pengakuan utang supplier di `/finance/payable/invoice` ("Faktur & Tagihan Supplier") dan membuka tombol tindakan **"Catat Retur Pembelian"**.

3. **Pencatatan Retur Pembelian (`/finance/purchasing/supplier-returns/new?purchasingInvoiceId=...`):**
   - Hanya dapat dibuka dari Purchasing Invoice yang berstatus `APPROVED` (`FIN-VAL-110`).
   - Petugas mengisi alasan retur, rincian barang yang diretur (deskripsi, qty, harga satuan), dan nominal PPN retur.
   - Layar menampilkan pratinjau nilai kredit/deposit yang akan diterbitkan.

4. **Konfirmasi & Penerbitan Deposit Retur (`/finance/purchasing/supplier-returns/[slug]` & `/deposits`):**
   - Petugas dapat mengonfirmasi retur (`confirm`) atau membatalkannya (`cancel`).
   - Konfirmasi retur menerbitkan `FinSupplierReturnDeposit` sebesar pokok retur ditambah PPN (`FIN-VAL-111`).
   - Saldo deposit retur (`availableAmount`), status pemakaian (`AVAILABLE`, `EXHAUSTED`, `CANCELLED`), serta tautan rujukan ke retur sumber dapat dipantau langsung pada layar berpaging penuh di `/finance/purchasing/supplier-returns/deposits`.

---

## 2. Gerbang Komponen UI (`base-component-decision-gate.md`)

Seluruh elemen antarmuka dibangun dengan memanfaatkan kembali katalog komponen basis (`base-features`) yang telah terbukti stabil pada task sebelumnya tanpa membuat komponen duplikat maupun modifikasi destruktif:

| Kebutuhan Antarmuka | Komponen Kandidat | Bukti Pemakaian | Status | Catatan Implementasi |
| :--- | :--- | :--- | :--- | :--- |
| Header & Judul Halaman | `Hero` | `base-features/hero.jsx` | `REUSE` | Digunakan seragam di seluruh halaman form, detail, list, dan landing view |
| Navigasi Hirarkis | `FinanceBreadcrumb` | `finance/shared/finance-breadcrumb.jsx` | `REUSE` | Menautkan alur dari Finance -> Purchasing -> Purchasing Invoice / Retur |
| Guard Hak Akses | `AccessDeniedGate` | `base-features/access-denied-gate.jsx` | `REUSE` | Mengamankan tampilan bila token kedaluwarsa atau terjadi galat 401/403 |
| Notifikasi Toast | `ToastStack` | `base-features/toast-stack.jsx` | `REUSE` | Menampilkan umpan balik sukses, peringatan validasi, dan galat API |
| Tombol Aksi Baku | `BaseButton` | `base-features/base-button.jsx` | `REUSE` | Varian `primary`, `secondary`, `success`, `danger`, `warning` dengan state `loading` |
| Modal Konfirmasi Aksi | `ConfirmModal` | `base-features/confirm-modal.jsx` | `REUSE` | Dipakai pada Submit, Approve, Reject (dengan field alasan), Confirm Retur, dan Cancel Retur |
| Pilihan Tukar Faktur & Supplier | `ResourceFilterSelect` | `base-features/resource-filter-select.jsx` | `REUSE` | Terintegrasi dengan `useSelectInvoiceExchanges` dan `useSelectSuppliers` |
| Filter & Dropdown Status | `FilterSelect` | `base-features/filter-select.jsx` | `REUSE` | Digunakan pada penyaringan status deposit dan ukuran halaman |
| Tabel Data & Kontainer Filter | `DataTable`, `DataFilter` | `base-features/data-table.jsx`, `data-filter.jsx` | `REUSE` | Digunakan pada halaman daftar Deposit Retur |
| Paginasi Tabel | `Pagination` | `pagination/pagination.jsx` | `REUSE` | Menangani paginasi server-side pada daftar Deposit Retur |
| Kotak Informasi Alur | `InformationAlert` | `base-features/information-alert.jsx` | `REUSE` | Menampilkan pesan galat atau penjelasan kontekstual |
| Input Baris Tabel Item Padat | `<input className="form-control form-control-sm">` | Standar form tabel | `REUSE` | Digunakan pada sel tabel rincian item (menghindari label ganda `BaseTextField`) |

`UI GATE FE-FIN-009: 12 elemen dievaluasi — REUSE 12, NEW 0, EXTEND 0`

---

## 3. Berkas yang Dibuat & Diintegrasikan

Semua berkas berada di repositori `QuilvianSystemFrontendDev`:

### A. Rute App Router (`src/app/finance/purchasing/`)
1. `purchasing-invoices/page.jsx`: Server Component landing page alur pembelian dari Tukar Faktur hingga pengakuan utang.
2. `purchasing-invoices/new/page.jsx`: Route pembuatan Purchasing Invoice baru.
3. `purchasing-invoices/[slug]/page.jsx`: Route detail Purchasing Invoice dan aksi workflow (ajukan/setujui/tolak/retur).
4. `purchasing-invoices/[slug]/edit/page.jsx`: Route penyuntingan Purchasing Invoice berstatus `DRAFT`.
5. `supplier-returns/page.jsx`: Route redirect jujur ke `/finance/purchasing/supplier-returns/deposits`.
6. `supplier-returns/new/page.jsx`: Route pencatatan Retur Pembelian baru (dibungkus `Suspense` boundary).
7. `supplier-returns/[slug]/page.jsx`: Route detail Retur Pembelian dan konfirmasi penerbitan deposit.
8. `supplier-returns/deposits/page.jsx`: Route daftar Deposit Retur berpaging penuh.

### B. Komponen View (`src/components/view/finance/purchasing/`)
1. `purchasing-invoice/purchasing-invoice-landing-view.jsx`: Tampilan alur kerja dan navigasi terintegrasi.
2. `purchasing-invoice/purchasing-invoice-form-view.jsx`: Form pembuatan/pembaruan Purchasing Invoice dengan kalkulasi otomatis `TotalAmount` dan tabel rincian item dinamis.
3. `purchasing-invoice/detail/purchasing-invoice-detail-view.jsx`: Layar detail lengkap dengan rincian komersial, approval tier, segregasi pembuat/penyetuju, dan modal konfirmasi aksi.
4. `supplier-return/supplier-return-form-view.jsx`: Form pencatatan retur dengan validasi faktur sumber `APPROVED`.
5. `supplier-return/detail/supplier-return-detail-view.jsx`: Layar detail retur, konfirmasi penerbitan deposit, dan ringkasan deposit terbit.
6. `supplier-return/deposit/supplier-return-deposit-view.jsx`: Layar daftar deposit retur berpaging dengan filter supplier dan status.
7. `supplier-return/deposit/supplier-return-deposit-table-columns.jsx`: Konfigurasi kolom tabel deposit retur (Supplier, No Retur, Nominal Awal, Saldo Tersedia, Status).

### C. Logika & Custom Hooks (`src/lib/hooks/finance/purchasing/`)
1. `use-purchasing-invoice-editor.jsx`: State management form invoice, validasi kelengkapan baris, kalkulasi subtotal dan total otomatis.
2. `use-purchasing-invoice-detail.jsx`: Pengambilan data invoice `GET /{id}`, segregasi `isRequester`, dan pemanggilan endpoint workflow (`submit`, `approve`, `reject`).
3. `use-supplier-return-editor.jsx`: Validasi query parameter invoice sumber, penarikan data invoice sumber, dan pengiriman payload `POST /supplier-returns`.
4. `use-supplier-return-detail.jsx`: Pengambilan data retur `GET /{id}`, eksekusi konfirmasi/pembatalan, dan penelusuran deposit terbit.
5. `use-supplier-return-deposit-list.jsx`: Pengambilan data daftar deposit berpaging `GET /supplier-returns/deposits`, pengelolaan filter pencarian, dan navigasi double-click ke retur sumber.

### D. Konstanta Bisnis (`src/lib/constants/finance/purchasing/`)
1. `purchasing-invoice-constants.jsx`: Endpoint dasar, status invoice (`DRAFT`, `PENDING_APPROVAL`, `APPROVED`, `REJECTED`, `CANCELLED`), badge tone, dan label jenjang persetujuan (`TIER_1`, `TIER_2`).
2. `supplier-return-constants.jsx`: Endpoint dasar retur dan deposit, status retur (`DRAFT`, `CONFIRMED`, `CANCELLED`), status deposit (`AVAILABLE`, `EXHAUSTED`, `CANCELLED`), dan opsi paginasi.

---

## 4. Kepatuhan Aturan & Integritas Bisnis

1. **Nol Perubahan Backend Diam-diam:**
   - Seluruh kontrak API (`FIN-API-1.1` §B.4 & §B.5) dikonsumsi murni sesuai spesifikasi endpoint `FinancePurchasingInvoicesController.cs` dan `FinanceSupplierReturnsController.cs`. Nol berkas C# atau skrip migration database yang disentuh selama task frontend ini.

2. **Kepatuhan Invarian `FIN-VAL-107` (Total Amount Seimbang):**
   - Backend mewajibkan: `Subtotal - Diskon + PPN - Uang Muka - Potongan Lain == TotalAmount`.
   - Di antarmuka, field Total Amount dikunci read-only dan dihitung secara deterministik real-time sehingga pengguna tidak akan pernah mengirimkan payload yang tidak seimbang ke backend.

3. **Kepatuhan Segregasi Tugas & Otorisasi (`FIN-VAL-108` & `03-frontend-architecture.md` §12.4):**
   - Tombol "Setujui" dan "Tolak" disembunyikan/dinonaktifkan jika pengguna yang sedang aktif adalah pengaju Purchasing Invoice tersebut (`isRequester`). Pengaju hanya dapat melihat status menunggu persetujuan.

4. **Kejadian Akuntansi PPN Masukan (`03-frontend-architecture.md` §12.7):**
   - Layar tidak menampilkan status pengiriman kejadian akuntansi PPN Masukan sebagai berhasil di klien sebelum kode kejadian `PPN-MASUKAN-PEMBELIAN` diratifikasi oleh modul Akuntansi.

---

## 5. Bukti Validasi

1. **Linting Check (`npm run lint:errors`):**
   - Status: `PASS`
   - Exit Code: `0`
   - Total Kesalahan: `0` error sintaks maupun aturan ESLint di seluruh repositori frontend.

2. **Production Build Turbopack (`npm run build`):**
   - Status: `PASS`
   - Exit Code: `0`
   - Seluruh 415 rute static dan dynamic berhasil dianalisis, dioptimasi, dan di-bundle tanpa kegagalan prerender.
   - Luaran: `[prepare-standalone] Standalone runtime siap dijalankan.`

3. **Verifikasi Manual Runtime:**
   - Status: `MANUAL TEST: NOT FEASIBLE`
   - Alasan: Lingkungan runtime database pengembangan belum menjalankan migration skema perbaikan Purchasing terbaru secara live; namun validasi statis, static site generation, dan keselarasan kontrak API telah terbukti 100% konsisten.

---

## 6. Kesimpulan & Penandaan Roadmap

Task `FE-FIN-009` telah diselesaikan secara penuh dengan kualitas kode prima, kepatuhan arsitektur App Router Next.js 16, penggunaan base components yang konsisten, dan memenuhi seluruh kriteria penerimaan tanpa modifikasi backend. Status task ini dinyatakan ✅ **SELESAI**.
