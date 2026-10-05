# Laporan Perubahan Frontend — `FE-FIN-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-018` |
| Judul | Lima layar pandangan tersaring rumpun A/P beserta daftar tanda terima barang terjangkau dari menu |
| Slice | `REV-13A` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`; `FIN-DES-073`; `03-frontend-architecture.md` §17.2, §17.3 |
| Contract version | `FIN-API-1.4` — endpoint `GET /purchasing/reports/*` (empat sub-path) dan `GET /purchasing/goods-receipts`, seluruhnya `approved`, nol endpoint baru dikonsumsi |
| Wewenang UI | `FIN-DEC-094` approved product brief — mengunci layar mana yang berdiri sendiri (§17.2/§17.3). Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/P" harus sudah berdiri) — source lengkap, lint PASS, build belum dikonfirmasi; dilanjutkan atas arahan eksplisit pengguna, sama seperti `FE-FIN-017` |
| Klasifikasi | `HEAVY` — satu repository (skor 0); 23 berkas baru + 1 diubah (skor 2); 9 hook baru + 9 view baru (skor 2); nol endpoint baru dikonsumsi (skor 0); database — tidak relevan (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — 5 layar baru (skor 2). Total 6 → `HEAVY` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk memverifikasi `[Route]`/`[AccessPermission]` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/purchasing/**`, `src/components/view/finance/purchasing/**`, `src/lib/hooks/finance/purchasing/**`, `src/lib/constants/finance/purchasing/**`, `src/utils/menu-sidebar/menu-items.jsx`, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `99df41e1` — branch `Yasmina` (read-only) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS**. `npm run test:unit`/`npm run build` **NOT RUN** — menunggu konfirmasi eksplisit pengguna |

---

## 1. Keadaan yang ditemukan di awal

`FE-FIN-016` membangun grup menu "Transaksi A/P" dengan 9 butir yang rutenya sudah ada, tetapi 5
butir lain pada peta menu V1 (§17.2) tidak terjangkau: Penerima Pesanan, Rekap Purchasing AP,
Laporan Tukar Faktur, Laporan Jatuh Tempo, dan Rekonsiliasi Tagihan.

**Temuan penting sebelum implementasi (dibuktikan dari source, bukan diasumsikan):** empat dari
lima layar itu ("pecahan dari tab" — Rekap Purchasing AP, Laporan Tukar Faktur, Laporan Jatuh
Tempo, Rekonsiliasi Tagihan) **sudah sepenuhnya dibangun** sejak `FE-FIN-011`, hidup sebagai empat
tab di dalam satu halaman gabungan `/finance/purchasing/reports`
(`purchasing-report-view.jsx`, memakai hook tunggal `usePurchasingReports`). Keempat
sub-view-nya (`purchasing-summary-report-view.jsx`, `invoice-exchange-report-view.jsx`,
`due-date-report-view.jsx`, `reconciliation-report-view.jsx`) menerima props murni
presentasional — siap dipakai ulang langsung tanpa menulis ulang satu baris pun UI tabelnya.

Layar kelima (Penerima Pesanan) genuinely baru — dikonfirmasi lewat pencarian menyeluruh
(`grep -rln "goods-receipt"`) bahwa belum ada hook/view untuk `GET /goods-receipts` sebagai daftar
berdiri sendiri; yang ada hanya riwayat tanda terima tertanam di dalam modal detail Purchase Order
(`purchase-order-detail-view.jsx`, cakupan berbeda — riwayat satu PO, bukan daftar lintas PO).

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AP Finance, Supervisor/Manajer Finance (seluruhnya **baca saja**).

**Pemicu:** Pengguna membuka salah satu dari lima butir baru pada grup "Transaksi A/P".

**Langkah normal (empat layar "pecahan dari tab"):**

1. Layar terbuka sebagai halaman berdiri sendiri (bukan tab), langsung menampilkan laporan yang
   relevan tanpa perlu mengeklik tab lain dulu.
2. Pengguna mengisi saringan bebas (supplier, status, rentang tanggal, dsb. — sama persis dengan
   yang tersedia di tab aslinya).
3. Tabel/ringkasan menampilkan data sesuai saringan, dengan paginasi di backend (kecuali Rekap
   Purchasing AP yang berbentuk ringkasan, bukan tabel berpaginasi).

**Langkah normal (Penerima Pesanan):**

1. Pengguna membuka daftar Tanda Terima Barang lintas Purchase Order.
2. Pengguna dapat menyaring berdasarkan Purchase Order, Supplier, atau Status.
3. Tabel menampilkan Nomor GR, Nomor PO (diresolusi dari `PurchaseOrderId`), tanggal diterima,
   dan status.
4. **Nol aksi tulis di layar ini** — pengguna yang ingin mencatat tanda terima baru diarahkan
   (lewat teks pada Hero) untuk membukanya dari detail Purchase Order seperti sekarang.

**Jalur tidak normal:** Kosong/gagal/tanpa hak akses — identik pola `FE-FIN-017` (lihat §4).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `03-frontend-architecture.md` §17.2 (tabel butir menu Transaksi A/P) dan §17.3 (pola
  "pandangan tersaring" dan subbagian Penerima Pesanan)
- `src/components/view/finance/purchasing/reports/purchasing-report-view.jsx` — halaman tab
  gabungan existing, dibaca penuh untuk memastikan **tidak** perlu diubah/dihapus
- `src/components/view/finance/purchasing/reports/{purchasing-summary,invoice-exchange,due-date,
  reconciliation}-report-view.jsx` — keempat sub-view, dikonfirmasi props murni presentasional
  (`data`/`filter`/`setFilter`/`loading`, kecuali summary: `summaryData`/`summaryFilter`/
  `setSummaryFilter`/`loading`/`onRefresh`)
- `src/lib/hooks/finance/purchasing/use-purchasing-reports.jsx` — hook orkestrator existing,
  dibaca penuh untuk menyalin persis bentuk query tiap fetch function ke hook terfokus baru
- `src/lib/constants/finance/purchasing/purchasing-report-constants.jsx` — `ENDPOINT_BASE`
  dipakai ulang, nol endpoint baru
- `src/components/features/base-features/toast-stack.jsx` — **ditemukan** bahwa
  `purchasing-report-view.jsx` memanggilnya dengan prop `onCloseToast`, padahal prop yang benar
  adalah `onClose` (dibaca langsung dari signature komponen) — bug lama pada halaman tab
  gabungan, **tidak disalin** ke layar baru (lihat §3.3)
- `src/components/view/finance/purchasing/purchase-order/detail/purchase-order-detail-view.jsx`,
  `use-purchase-order-detail.jsx` — dibaca untuk memastikan riwayat GR tertanam di sana adalah
  cakupan berbeda (satu PO) dari daftar lintas-PO yang dibutuhkan task ini
- `src/lib/hooks/select/finance/finance-select-resources.js`,
  `use-finance-select.js` — dikonfirmasi resource `purchaseOrders`/`goodsReceipts` sudah
  terdaftar untuk `useFinanceSelect`, **tanpa** `serverSide` (endpoint backend tidak punya
  parameter pencarian bebas — dicatat eksplisit di komentar berkas itu)
- `src/lib/hooks/finance/purchasing/use-supplier-name-resolver.jsx` — pola resolver Id->nama
  dipakai ulang persis untuk `use-purchase-order-number-resolver.jsx` baru
- `NewQuilvianSystemBackend/Areas/.../FinanceGoodsReceiptsController.cs` (read-only) — dibaca
  penuh: `[Route("api/v1/.../purchasing/goods-receipts")]`, `[AccessPermission("FinanceGoodsReceipt",
  "Read")]` untuk `GET /` dan `GET /{id}` — **dikonfirmasi cocok** dengan dugaan dokumen, nol
  koreksi dibutuhkan (berbeda dari temuan `FE-FIN-017`)
- `.../Dtos/FinanceGoodsReceiptDtos.cs` (read-only) — `GoodsReceiptQuery`
  (`PurchaseOrderId`/`SupplierId`/`Status`/`PageNumber`/`PageSize`) dan `GoodsReceiptResponse`
  (`Id`/`GRNumber`/`PurchaseOrderId`/`ReceivedDate`/`Status`/`RowVersion`) dibaca persis
- `.../Models/FinGoodsReceipt.cs` (read-only) — `FinGoodsReceiptStatuses`
  (`RECEIVED`/`CANCELLED`) dibaca persis, bukan ditebak
- `.../Controllers/FinancePurchasingReportsController.cs` (read-only) — keempat endpoint
  (`summary`/`invoice-exchanges`/`due-dates`/`reconciliation`) dikonfirmasi `[AccessPermission
  ("FinancePurchasingReport", "Read")]` persis sama untuk semuanya

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| **Penerima Pesanan (genuinely baru)** | |
| `src/lib/constants/finance/purchasing/goods-receipt-constants.jsx` **(baru)** | `ENDPOINT_BASE`, status (`RECEIVED`/`CANCELLED`), opsi filter |
| `src/lib/hooks/finance/purchasing/use-goods-receipt-list.jsx` **(baru)** | Hook state lokal — `GET /goods-receipts` |
| `src/lib/hooks/finance/purchasing/use-purchase-order-number-resolver.jsx` **(baru)** | Resolver `PurchaseOrderId` → nomor PO, pola identik `use-supplier-name-resolver.jsx` |
| `src/components/view/finance/purchasing/goods-receipt/goods-receipt-table-columns.jsx` **(baru)** | Definisi kolom: No, Nomor GR, Nomor PO (diresolusi), Tanggal Diterima, Status |
| `src/components/view/finance/purchasing/goods-receipt/goods-receipt-view.jsx` **(baru)** | Layar daftar baca-saja |
| `src/app/finance/purchasing/goods-receipts/page.jsx` **(baru)** | Route tipis |
| **Empat layar "pecahan dari tab" (REUSE penuh atas view existing)** | |
| `src/lib/hooks/finance/purchasing/use-purchasing-summary-report.jsx` **(baru)** | Hook terfokus, diekstrak dari `usePurchasingReports` — fetch function sama persis |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-report.jsx` **(baru)** | idem |
| `src/lib/hooks/finance/purchasing/use-due-date-report.jsx` **(baru)** | idem |
| `src/lib/hooks/finance/purchasing/use-reconciliation-report.jsx` **(baru)** | idem |
| `src/components/view/finance/purchasing/reports/standalone/purchasing-summary-report-page-view.jsx` **(baru)** | Wrapper Hero/Breadcrumb/AccessDeniedGate/ToastStack + `PurchasingSummaryReportView` (REUSE, nol modifikasi) |
| `src/components/view/finance/purchasing/reports/standalone/invoice-exchange-report-page-view.jsx` **(baru)** | idem + `InvoiceExchangeReportView` |
| `src/components/view/finance/purchasing/reports/standalone/due-date-report-page-view.jsx` **(baru)** | idem + `DueDateReportView` |
| `src/components/view/finance/purchasing/reports/standalone/reconciliation-report-page-view.jsx` **(baru)** | idem + `ReconciliationReportView` |
| `src/app/finance/purchasing/reports/summary/page.jsx` **(baru)** | Route tipis |
| `src/app/finance/purchasing/reports/invoice-exchanges/page.jsx` **(baru)** | Route tipis |
| `src/app/finance/purchasing/reports/due-dates/page.jsx` **(baru)** | Route tipis |
| `src/app/finance/purchasing/reports/reconciliation/page.jsx` **(baru)** | Route tipis |
| **Menu** | |
| `src/utils/menu-sidebar/menu-items.jsx` | 5 butir baru disisipkan ke grup "Transaksi A/P" pada posisi persis urutan V1 |

Halaman tab gabungan `/finance/purchasing/reports` (`purchasing-report-view.jsx`) **tidak
disentuh sama sekali** — dikonfirmasi lewat `git status --short` (nol perubahan pada berkas itu)
dan `git diff` kosong. Nol berkas backend disentuh.

### 3.3 Kepatuhan arsitektur frontend

**Base component — seluruhnya `REUSE`, nol `NEW`/`EXTEND`:**

| Elemen | Status | Sumber |
| --- | --- | --- |
| `AccessDeniedGate`, `DataFilter`, `DataTable`, `FilterSelect`, `ResourceFilterSelect`, `Hero`, `InformationAlert`, `ToastStack`, `Pagination`, `FinanceBreadcrumb`, `StatusBadge` | `REUSE` | Dipakai apa adanya, sama seperti `FE-FIN-017` |
| `PurchasingSummaryReportView`, `InvoiceExchangeReportView`, `DueDateReportView`, `ReconciliationReportView` | `REUSE` | Empat komponen **domain** existing (`FE-FIN-011`) dipakai ulang **seutuhnya tanpa modifikasi satu baris pun** — bukan sekadar base component, melainkan seluruh layar tabel+filter |

Karena seluruh elemen `REUSE`, gerbang keputusan **tidak menghasilkan pilihan bernomor apa pun**.

**Bug lama ditemukan dan sengaja TIDAK disalin:** `purchasing-report-view.jsx` (halaman tab
gabungan, `FE-FIN-011`) memanggil `<ToastStack toasts={toasts} onCloseToast={removeToast} />` —
`ToastStack` **tidak pernah memiliki prop bernama `onCloseToast`** (signature aslinya: `toasts`,
`onClose`, dst. — dibaca langsung dari `toast-stack.jsx`). Tombol tutup toast pada halaman tab
gabungan itu **secara diam-diam tidak berfungsi** sejak `FE-FIN-011`. Task ini **tidak mengubah**
halaman tab gabungan (di luar wewenang — itu bukan task ini), tetapi **tidak mewarisi bug itu**:
keempat wrapper baru memakai `onClose={removeToast}` yang benar. Dicatat di `§8` sebagai temuan
untuk ditindaklanjuti task tersendiri.

**Reuse data-layer:** empat hook baru (`use-purchasing-summary-report.jsx` dkk.) adalah hasil
ekstraksi **fetch function yang sama persis** dari `usePurchasingReports` — nol perubahan bentuk
query, nol field baru, nol endpoint baru. `use-goods-receipt-list.jsx` dan
`use-purchase-order-number-resolver.jsx` adalah satu-satunya data-layer genuinely baru pada task
ini, keduanya mengikuti pola hook per-layar yang sudah mapan (`use-receivable-invoice-batch-
list.jsx` dan `use-supplier-name-resolver.jsx`), bukan Redux, bukan hook generik lintas-layar.

---

## 4. State yang ditangani di layar

Identik pola `FE-FIN-017`: memuat (kerangka `DataTable`), kosong (kalimat khusus per layar),
gagal (`InformationAlert`), tanpa hak akses (`AccessDeniedGate`). Keempat layar "pecahan dari tab"
mewarisi penanganan state ini **langsung dari komponen yang di-reuse** — nol penanganan state
baru ditulis untuk layar-layar itu.

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Goods Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/goods-receipts` | Penerima Pesanan | `FinanceGoodsReceipt : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}` | Resolusi Nomor PO per baris (sudah ada, `FE-FIN-008`) | `FinancePurchaseOrder : Read` |

#### Corporate / Finance Management / Purchasing / Reports

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/reports/summary` | Rekap Purchasing AP | `FinancePurchasingReport : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/reports/invoice-exchanges` | Laporan Tukar Faktur | `FinancePurchasingReport : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/reports/due-dates` | Laporan Jatuh Tempo | `FinancePurchasingReport : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/reports/reconciliation` | Rekonsiliasi Tagihan | `FinancePurchasingReport : Read` |

Seluruh enam endpoint **sudah ada dan tidak diubah** oleh task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`eslint . --quiet`, exit code 0) | `PASS` | Keluaran perintah |
| `npm run test:unit` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| `npm run build` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| Review diff/scope | 23 berkas baru + 1 diubah, persis sesuai §17.2/§17.3 | `PASS` | `git status --short` |
| Review setiap import path | Seluruhnya dikonfirmasi ada di disk | `PASS` | §3.1, pemeriksaan manual |
| Review halaman tab gabungan tidak tersentuh | `git status --short` tidak menunjukkan `purchasing-report-view.jsx` atau empat sub-view-nya sebagai berubah | `PASS` | §3.2 |
| Review permission `FinanceGoodsReceipt`/`FinancePurchasingReport` terhadap backend | Dibaca langsung dari `[AccessPermission]` kedua controller, cocok persis dengan dokumen (berbeda dari `FE-FIN-017` yang menemukan ketidakcocokan) | `PASS` | §3.1 |
| Grep anti-regresi UI (6 pola) | Nol temuan pada seluruh 5 file view baru | `PASS` | Keluaran grep kosong di keenam pola |
| Review urutan butir menu grup "Transaksi A/P" | Urutan persis §17.2 V1 | `PASS` | `grep -oP 'label: "\K[^"]+'` atas blok grup |
| Review `key` unik file-wide | Satu duplikat ditemukan (`healthServicesDoctorQueue`) — **pre-existing, di luar modul Finance, tidak disentuh task ini** | Dicatat, bukan diperbaiki (di luar cakupan) | `grep -oP 'key: "\K[^"]+' \| sort \| uniq -d` |

Uji manual: `NOT FEASIBLE` — server tidak dijalankan pada task ini.

**AUTOMATED TEST: NOT APPLICABLE — repository ini tidak memakai Jest; menulis test baru bersifat
opsional, tidak diminta eksplisit pada task ini.**

**Tidak dijalankan:** `npm run test:unit`, `npm run build` (menunggu konfirmasi eksplisit
pengguna); verifikasi manual/visual di browser (server tidak dijalankan).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-018`) | Status | Bukti |
| --- | --- | --- |
| Keempat laporan memakai endpointnya masing-masing, bukan satu endpoint disaring klien | Terpenuhi | §5 — empat endpoint terpisah, sama seperti hook orkestrator asalnya |
| Penerima Pesanan **hanya membaca** — pencatatan tanda terima tetap dari detail Purchase Order | Terpenuhi | §2, §3.2 — nol tombol/aksi tulis pada `goods-receipt-view.jsx` |
| Lint PASS | Terpenuhi | `npm run lint:errors` PASS |
| Build PASS | **Belum terpenuhi** | `npm run build` `NOT RUN`, menunggu konfirmasi pengguna |
| Butir menu terdaftar | Terpenuhi | §3.2, urutan diverifikasi |
| Laporan task tracked ada | Terpenuhi | Laporan ini |

Task ini **belum** dapat ditandai ✅ selama `npm run build` belum dikonfirmasi pengguna.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` belum dijalankan sama sekali untuk task ini — sengaja, mengikuti konvensi sesi saat ini. Risiko dinilai rendah (lint PASS, empat dari lima layar murni REUSE komponen existing yang sudah terbukti berjalan di halaman tab gabungan) |
| Masalah yang diketahui | **(1)** `purchasing-report-view.jsx` (halaman tab gabungan, `FE-FIN-011`) memanggil `ToastStack` dengan prop `onCloseToast` yang tidak pernah ada pada komponennya — tombol tutup toast di halaman itu tidak berfungsi sejak awal. **Di luar cakupan task ini untuk diperbaiki** (halaman itu tidak disentuh), tetapi **MUST** diperbaiki pada task terpisah kecil karena memengaruhi pengguna yang masih memakai jalan masuk tab gabungan. **(2)** Penerima Pesanan me-resolve Nomor PO dengan satu panggilan `GET /purchase-orders/{id}` per baris unik per halaman (bukan batch) — proporsional untuk ukuran halaman (pageSize), pola yang sama persis dengan `use-supplier-name-resolver.jsx` yang sudah dipakai produksi |
| Dependency backend | Tidak ada endpoint baru — seluruh 6 endpoint yang dirujuk sudah ada dan tidak disentuh task ini. `FE-FIN-016`/`017` masih 🟡, belum dikonfirmasi build — risiko ditanggung bersama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan 23 berkas baru + 1 berkas diubah, seluruhnya sesuai §3.2 |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run build` dan mengonfirmasi hasilnya untuk `FE-FIN-016`, `017`, **dan** `018` sekaligus; (2) Task kecil terpisah memperbaiki bug `onCloseToast` pada `purchasing-report-view.jsx`; (3) `FE-FIN-019` (`REV-13B`, bergantung `BE-FIN-053`/`054` — keduanya sudah ✅) sebagai lanjutan berikutnya |
