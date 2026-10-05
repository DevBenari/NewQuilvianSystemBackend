# Laporan Perubahan Frontend — `FE-FIN-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-011` |
| Judul | Empat laporan Purchasing/AP dapat dibaca |
| Slice | `REV-4` — Purchasing/AP (`EPIC FIN-15`), gelombang `R4-3` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` baris `FE-FIN-011` |
| Trace | `FR-FIN-088`; `FIN-API-1.1` §B.6 (kecuali `/aging` — `FIN-DEC-059`); `FIN-PERM-1.1` §B.5; `FIN-DES-044`; `03-frontend-architecture.md` bagian 12.1 |
| Contract version | `FIN-API-1.1` (`locked` 25 September 2026) |
| Wewenang UI | Product Owner — tata letak `DEV_DISCRETION` (`03-frontend-architecture.md` §12.1 & §12.6). Pola halaman laporan yang sudah ada (`/finance/ap-report`) dipakai ulang; nol perhitungan di klien; read-only |
| Dependency | `BE-FIN-037` ✅ (`FinancePurchasingReportsController.cs` & `FinancePurchasingReportService.cs` dengan 4 endpoint `GET /summary`, `/invoice-exchanges`, `/due-dates`, `/reconciliation`) |
| Klasifikasi | `MEDIUM` — Membangun antarmuka terintegrasi untuk 4 laporan analitik Purchasing/AP, penyaringan tanggal & supplier, visualisasi DaysUntilDue & status, serta navigasi responsif; zero backend changes |
| Task mode | `FRONTEND MODE` — repository backend `NewQuilvianSystemBackend` diperiksa read-only, nol modifikasi kode C# backend |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/finance/purchasing/reports/**`, `src/lib/hooks/finance/purchasing/use-purchasing-reports.jsx`, `src/lib/constants/finance/purchasing/purchasing-report-constants.jsx`, `src/app/finance/purchasing/reports/**` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI.** Empat laporan Purchasing/AP telah selesai penuh, mematuhi kontrak read-only dan formula backend, tervalidasi 100% lulus `npm run lint:errors` (0 error) dan `npm run build` Turbopack (0 error) |

---

## 1. Ringkasan Eksekutif & Proses Bisnis

Sesuai rancangan arsitektur `03-frontend-architecture.md` §12.1 dan spesifikasi kontrak `FIN-API-1.1` §B.6, manajemen rumah sakit dan staf Account Payable (AP) membutuhkan visibilitas menyeluruh atas alur siklus belanja mulai dari Purchase Order hingga menjadi kewajiban utang dagang.

Melalui task `FE-FIN-011`, telah dibangun antarmuka terpadu di rute `/finance/purchasing/reports` yang menyajikan **empat laporan analitik read-only**:

1. **Rekap Purchasing AP (`GET .../reports/summary`):**
   - Menyajikan kartu eksekutif ringkasan:
     - Total nilai dan jumlah Purchase Order aktif.
     - Jumlah dokumen Tanda Terima Barang (GR).
     - Jumlah Tukar Faktur berstatus menggantung (`PendingExchangeCount`).
     - Jumlah dan total nilai Faktur Pembelian (Purchasing Invoice) yang telah disetujui (`ApprovedInvoiceTotal`).
     - Sisa saldo utang supplier (`OutstandingPayableTotal`) yang bersumber langsung dari Faktur Pembelian.
   - Panel breakdown runtut 4 tahapan alur belanja.
   - Filter opsional: rentang tanggal (`PeriodFrom`, `PeriodTo`) dan rekanan supplier (`SupplierId`).

2. **Laporan Tukar Faktur (`GET .../reports/invoice-exchanges`):**
   - Menyajikan daftar seluruh dokumen Tukar Faktur berpaging (`PageNumber`, `PageSize`).
   - Menampilkan status keterkaitan ke Faktur Pembelian:
     - Jika sudah diproses: menyajikan nomor invoice, status, dan nilai tagihan.
     - Jika belum: menampilkan badge peringatan `Belum Terhubung`.
   - Filter: Rekanan supplier, status (`RECEIVED`, `LINKED_TO_INVOICE`, `CANCELLED`), dan rentang tanggal penerimaan.

3. **Laporan Jatuh Tempo (`GET .../reports/due-dates`):**
   - Memantau Faktur Pembelian yang mendekati atau telah melampaui tanggal jatuh tempo pembayaran.
   - Menampilkan tanggal estimasi jatuh tempo (dari Tukar Faktur) dan tanggal jatuh tempo aktual (dari utang supplier AP).
   - Menyajikan indikator urgensi `DaysUntilDue` yang dihitung oleh backend:
     - `< 0`: badge bahaya merah (*Terlambat N hari*).
     - `=== 0`: badge kuning (*Jatuh tempo hari ini*).
     - `> 0`: badge info (*N hari lagi*).
   - Filter: Rekanan supplier, status invoice, rentang tanggal jatuh tempo, dan opsi *Hanya yang Telah Lewat Jatuh Tempo (OverdueOnly)*.

4. **Rekonsiliasi Tagihan AP (`GET .../reports/reconciliation`):**
   - Dirancang khusus untuk membantu tim AP mendeteksi dokumen Tukar Faktur berstatus `RECEIVED` yang belum diproses menjadi Purchasing Invoice.
   - Mencegah terjadinya dokumen menggantung yang terlambat dibayar kepada supplier rekanan.
   - Menyediakan tombol aksi cepat *"Susun Invoice"* pada tiap baris dokumen untuk segera menindaklanjuti proses tagihan.

5. **Pencabutan `/aging` Sesuai `FIN-DEC-059`:**
   - Sesuai keputusan resmi `FIN-DEC-059`, endpoint `/aging` tidak diduplikasi pada modul Purchasing. Laporan umur utang (AP Aging) tetap terpusat di `/finance/ap-aging` untuk menghindari perbedaan angka agregat utang.

---

## 2. Gerbang Komponen UI (`base-component-decision-gate.md`)

| Kebutuhan Antarmuka | Komponen Kandidat | Bukti Pemakaian | Status | Catatan Implementasi |
| :--- | :--- | :--- | :--- | :--- |
| Header & Breadcrumb | `Hero`, `FinanceBreadcrumb` | `finance-ap-report-view.jsx` | `REUSE` | Konsisten dengan hierarki Finance > Purchasing > Laporan Pembelian |
| Navigasi 4 Tab Laporan | Tab Navigation responsif | `finance-monitoring-view.jsx`, `finance-cash-management-view.jsx` | `COMPOSE` | Merangkai 4 tab interaktif (Rekap, Tukar Faktur, Jatuh Tempo, Rekonsiliasi) menggunakan token warna dan ikon standar |
| Ringkasan Statistik | `SummaryCards` | `summary-grid.jsx` | `REUSE` | Menampilkan 5 kartu statistik agregat Purchasing AP |
| Filter & Dropdown Supplier | `DataFilter`, `FilterSelect` | `data-filter.jsx`, `use-administrator-select.js` | `REUSE` | Menggunakan `useSelectSuppliers` untuk memilih supplier rekanan |
| Tabel Laporan | `DataTable` | `data-table.jsx` | `REUSE` | Menampilkan baris data Tukar Faktur, Jatuh Tempo, dan Rekonsiliasi |
| Paginasi Tabel | `Pagination` | `pagination.jsx` | `REUSE` | Navigasi halaman dan batas baris standar (25 data/halaman) |
| Gerbang Keamanan | `AccessDeniedGate` | `access-denied-gate.jsx` | `REUSE` | Menjaga halaman dari galat otorisasi `FinancePurchasingReport : Read` |
| Notifikasi Sistem | `ToastStack` | `toast-stack.jsx` | `REUSE` | Menampilkan pesan toast galat/sukses |

`UI GATE FE-FIN-011: 8 elemen dievaluasi — REUSE 7, COMPOSE 1, NEW 0, EXTEND 0`

---

## 3. Berkas yang Dibuat & Dimodifikasi

### Repository Frontend (`QuilvianSystemFrontendDev`)
1. [`src/lib/constants/finance/purchasing/purchasing-report-constants.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/lib/constants/finance/purchasing/purchasing-report-constants.jsx) *(BARU)* — Definisi konstanta endpoint `/v1/corporate/finance-management/purchasing/reports`, enum tab laporan, filter status, dan fungsi kalkulasi badge urgensi jatuh tempo.
2. [`src/lib/hooks/finance/purchasing/use-purchasing-reports.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/lib/hooks/finance/purchasing/use-purchasing-reports.jsx) *(BARU)* — Custom hook pengelolaan state tab aktif, filter pencarian per laporan, pemanggilan Axios instance ke 4 endpoint backend, dan penanganan galat.
3. [`src/components/view/finance/purchasing/reports/purchasing-summary-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/reports/purchasing-summary-report-view.jsx) *(BARU)* — Subview Rekap Purchasing AP dengan SummaryCards dan 4 panel alur belanja.
4. [`src/components/view/finance/purchasing/reports/invoice-exchange-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/reports/invoice-exchange-report-view.jsx) *(BARU)* — Subview Laporan Tukar Faktur dengan status keterkaitan invoice dan filter tanggal.
5. [`src/components/view/finance/purchasing/reports/due-date-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/reports/due-date-report-view.jsx) *(BARU)* — Subview Laporan Jatuh Tempo dengan indikator `DaysUntilDue` dan filter overdue.
6. [`src/components/view/finance/purchasing/reports/reconciliation-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/reports/reconciliation-report-view.jsx) *(BARU)* — Subview Rekonsiliasi Tagihan AP untuk mendeteksi Tukar Faktur menggantung sebelum batas waktu.
7. [`src/components/view/finance/purchasing/reports/purchasing-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/reports/purchasing-report-view.jsx) *(BARU)* — Komponen utama wrapper tab laporan Purchasing AP.
8. [`src/app/finance/purchasing/reports/page.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/app/finance/purchasing/reports/page.jsx) *(BARU)* — Rute App Router Next.js untuk `/finance/purchasing/reports`.
9. [`src/components/view/finance/purchasing/purchasing-invoice/purchasing-invoice-landing-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/purchasing/purchasing-invoice/purchasing-invoice-landing-view.jsx) *(MODIFIKASI)* — Penambahan tombol dan kartu navigasi alur ke Laporan Pembelian.
10. [`src/components/view/finance/payable/report/finance-ap-report-view.jsx`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/QuilvianSystemFrontendDev/src/components/view/finance/payable/report/finance-ap-report-view.jsx) *(MODIFIKASI)* — Penambahan tombol tautan silang ke Laporan Pembelian & Utang.

### Repository Dokumen & Backend (`NewQuilvianSystemBackend`)
- [`docs/module-blueprints/finance-management/task/report/frontend/FE-FIN-011.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/task/report/frontend/FE-FIN-011.md) *(BARU)* — Laporan tracked ini.
- [`docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md) *(MODIFIKASI)* — Penandaan status `FE-FIN-011` menjadi `✅` pada seluruh diagram, tabel gelombang, dan kartu task.
- [`docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md) *(MODIFIKASI)* — Penandaan status `FE-FIN-011` dan kebutuhan `FR-FIN-088` menjadi `✅`.

---

## 4. Bukti Verifikasi Mutu

| Perintah Uji | Hasil | Keterangan |
| :--- | :---: | :--- |
| `npm run lint:errors` | **PASS (0 error)** | Lolos pemeriksaan ESLint di seluruh repository frontend tanpa peringatan. |
| `npm run build` | **PASS (0 error)** | Kompilasi Turbopack Next.js App Router sukses untuk seluruh 416 rute halaman tanpa regresi. |
| Backend Read-Only Safety | **PASS (0 file C# diubah)** | Repository backend tetap *strictly read-only*. |

---

## 5. Kepatuhan Aturan Bisnis & Rekayasa

1. **Zero Client Calculation:** Nilai rupiah (`TotalAmount`, `PurchaseOrderTotal`, `OutstandingPayableTotal`, dll.) dan sisa hari jatuh tempo (`DaysUntilDue`) murni dibaca dari respons backend.
2. **Read-Only Invariant:** Tidak ada tombol pengubah data atau aksi mutasi pada laporan, kecuali tombol pintasan untuk menyusun faktur baru dari tukar faktur pada layar rekonsiliasi.
3. **Penyelarasan Menu (`FIN-DEC-060`):** Menyiapkan rute dan antarmuka laporan pembelian yang siap dihubungkan langsung oleh penataan menu sidebar pada `FE-FIN-014`.
