# Laporan Perubahan Frontend — `FE-FIN-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-008` |
| Judul | Petugas AP mencatat Purchase Order, Tanda Terima Barang, dan Tukar Faktur; penyetuju menyetujui PO sesuai jenjangnya |
| Slice | `REV-4` — Purchasing/AP (`EPIC FIN-15`), gelombang `R4-1` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` baris `FE-FIN-008` |
| Trace | `FR-FIN-081`, `082`; `FIN-DEC-050`, `051`, `052`; `FIN-API-1.1` B.1-B.3; `FIN-PERM-1.1` B.2-B.3; `FIN-VAL-1.2` `100`..`104` |
| Contract version | `FIN-API-1.1` — `locked` 25 September 2026. Sejak pembaruan ini, source backend juga sudah selaras dengan kolom `GET /` daftar berpaging pada kontrak (sebelumnya kontrak menandainya "Rencana (belum tersedia)" padahal murni belum diimplementasikan — lihat `BE-FIN-032`/`BE-FIN-033` AMENDMENT 30 September 2026) |
| Wewenang UI | Product Owner — tata letak, wizard vs layar terpisah `DEV_DISCRETION` (`03-frontend-architecture.md` 12.6). Diputuskan di sini: Create dan Detail sebagai layar terpisah (bukan wizard satu layar), Tanda Terima Barang sebagai modal di dalam Detail PO (bukan layar terpisah) — dijelaskan alasannya di bagian 1. **Lanjutan 30 September 2026**: Daftar PO dan Daftar Tukar Faktur dibangun mengikuti pola `finance-receivable-view.jsx` (susunan Hero/DataFilter/DataTable/Pagination) tetapi dengan state lokal (bukan Redux), demi konsistensi dengan hook lain pada feature ini sendiri |
| Dependency | `BE-FIN-032` 🟡 (PO + GR — `GET /` berpaging dan riwayat GR pada detail PO ditambahkan 30 September 2026, `dotnet build` PASS 0 error; sisa gap: penugasan role dan kolom `RejectionReason`, di luar cakupan frontend), `BE-FIN-033` 🟡 (Tukar Faktur — `GET /` berpaging ditambahkan 30 September 2026, `dotnet build` PASS 0 error) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); domain terdekat diperiksa < 8 berkas (skor 0), > 8 total dibaca lintas kontrak dan pola base component (skor 1); 15 berkas baru + 9 berkas lanjutan (skor 1); logika bisnis sedang — perhitungan subtotal/total klien murni tampilan, nol perhitungan uang otoritatif (skor 1); kontrak API — endpoint sudah ada, perlu verifikasi manual (skor 1); database — tidak relevan frontend (skor 0); keamanan/auth — aksi bergantung status dan kepemilikan (`isRequester`), bukan sekadar tampil/sembunyi (skor 1). Total 4 → `MEDIUM` |
| Task mode | `FRONTEND MODE` — backend `NewQuilvianSystemBackend` diperiksa read-only, nol perubahan source backend pada lanjutan ini |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/purchasing/**`, `src/components/view/finance/purchasing/**`, `src/lib/hooks/finance/purchasing/**`, `src/lib/hooks/select/finance/**` (baru), `src/lib/hooks/select/select-resource-registry.js` (tambahan wiring, bukan perubahan struktur), `src/lib/constants/finance/purchasing/**`, `src/utils/finance/purchasing/**` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `a31da3c21` (branch `yasmina`) — lanjutan ini masih di atas commit yang sama, belum ada commit baru |
| Commit backend yang dijadikan rujukan | `5ee4be9a` (branch `Yasmina`) + perubahan working tree backend yang belum di-commit (`BE-FIN-032`/`BE-FIN-033` AMENDMENT 30 September 2026 — `GET /` berpaging, riwayat GR pada detail PO) |
| Tanggal | 30 September 2026 |
| Status | 🟡 **SEBAGIAN.** Create, Detail, dan **Daftar PO** selesai; pencatatan GR terhadap PO (riwayat persisten lintas sesi); Create, Detail, Cancel, dan **Daftar Tukar Faktur** selesai; kolom Supplier pada Daftar dan dropdown PO/GR pada form Tukar Faktur diperbaiki sesi ini (lihat bagian 1 "Lanjutan kedua"). Lolos lint. Satu-satunya butir DoD yang belum terpenuhi: `Idempotency-Key` (backend belum membaca header ini sama sekali) — **memerlukan task backend tersendiri**, bukan sesuatu yang bisa diselesaikan dari sisi frontend, lihat bagian 7/8 |

---

## 1. Keadaan yang ditemukan di awal

**Nol UI existing untuk rumpun Purchasing.** Pencarian di seluruh `src/app` dan `src/components/view` untuk `purchas*` dan `supplier-return*` menghasilkan nol berkas sebelum task ini.

**Kontrak API (`FIN-API-1.1` §B.1-B.3) sudah usang dibanding source.** Setiap baris menandai endpoint sebagai "Rencana (belum tersedia)". Pembacaan langsung ke `FinancePurchaseOrdersController.cs`, `FinanceGoodsReceiptsController.cs`, `FinanceInvoiceExchangesController.cs` di `NewQuilvianSystemBackend` (`5ee4be9a`) menunjukkan:

| Endpoint | Kontrak bilang | Source sebenarnya |
| --- | --- | --- |
| `GET /` (daftar berpaging), ketiga resource | Rencana (belum tersedia) | **Tidak ada sama sekali** — dikonfirmasi sampai level service (`FinancePurchaseOrderService`, `FinanceGoodsReceiptService`, `FinanceInvoiceExchangeService`), nol method query/list |
| `GET /{id}`, `POST /`, `POST /{id}/submit`, `/approve`, `/reject`, `/cancel` (PO) | Rencana (belum tersedia) | **Sudah ada dan berfungsi** |
| `GET /{id}`, `POST /`, `POST /{id}/cancel` (GR, Tukar Faktur) | Rencana (belum tersedia) | **Sudah ada dan berfungsi** |

Komentar di source `FinancePurchaseOrdersController.cs` sendiri mengonfirmasi ini bukan kelalaian: *"`GET /` (daftar berpaging) belum ada service-nya — gap terbuka yang sama seperti `FinancePaymentsController`, dicatat di laporan task, bukan dikarang di sini."*

**Konsekuensi terhadap scope task ini, dikonfirmasi ke pemilik sebelum menulis kode.** Acceptance criteria roadmap menuntut "Daftar/buat/rincian PO" dan "daftar/buat/batal Tukar Faktur". Tanpa `GET /` berpaging, layar Daftar tidak dapat dibangun tanpa menebak payload backend yang belum ada — dilarang eksplisit oleh governance frontend. Pemilik diberi pilihan (bangun bagian yang didukung dan tunda Daftar, atau hentikan seluruh task) dan memilih **melanjutkan bagian yang didukung**. Layar Daftar dicatat sebagai delta terbuka pada bagian 8, bukan diselesaikan dengan menebak.

**Lanjutan 30 September 2026 — kedua gap di atas sudah diperbaiki backend (`BE-FIN-032`/`BE-FIN-033` AMENDMENT), dikerjakan lewat `build-module-backend` sebagai penyelesaian kontrak yang sudah terkunci, bukan kapabilitas baru:**

| Gap | Status sebelumnya | Status sekarang |
| --- | --- | --- |
| `GET /` (daftar berpaging), ketiga resource | Nol method service | Tersedia untuk `purchase-orders` dan `invoice-exchanges` (dipakai layar Daftar di bagian ini); tersedia juga untuk `goods-receipts` tapi tidak dikonsumsi sebagai layar sendiri — riwayat GR sudah cukup ditampilkan di dalam Detail PO (lihat baris berikutnya) |
| `PurchaseOrderDetailResponse` tidak menyertakan `GoodsReceipts` anaknya | Riwayat GR hanya terlihat pada sesi browser yang sama | `GoodsReceipts` (ringkasan: id, grNumber, receivedDate, status) kini disertakan — riwayat GR persisten lintas sesi, dibaca di `use-purchase-order-detail.jsx` sebagai `goodsReceiptHistory` |

Konsekuensinya terhadap task ini: **Daftar PO dan Daftar Tukar Faktur, sebelumnya "Belum terpenuhi", sekarang dibangun** (bagian 3, 5, 7). Keputusan tata letak interim yang sebelumnya diambil karena ketiadaan Daftar (poin 1-3 di bawah) **dipertahankan apa adanya** — tidak ada acceptance criteria yang menuntutnya diubah menjadi wizard atau digabung ulang, dan mengubahnya sekarang akan menjadi refactor di luar cakupan permintaan lanjutan ini. Yang diperbarui hanya breadcrumb (poin "Finance/Pembelian" yang sebelumnya mengarah ke `/finance/payable` karena tidak ada tujuan lain, sekarang mengarah ke Daftar PO/Daftar Tukar Faktur masing-masing) dan tombol Kembali pada Detail PO/Tukar Faktur (sekarang kembali ke Daftar, bukan ke `/finance/payable`).

**Keputusan tata letak turunan dari temuan itu (`DEV_DISCRETION`, Product Owner belum menetapkan sebaliknya) — berlaku untuk Create/Detail/GR, ditulis 30 September 2026 sebelum Daftar ada:**

1. Create dan Detail PO sebagai **layar terpisah**, bukan wizard satu layar — karena tanpa Daftar, halaman Detail (`/finance/purchasing/purchase-orders/[id]`) adalah satu-satunya cara petugas kembali ke sebuah PO; memisahkannya dari Create membuat URL itu dapat disimpan/dibagikan petugas secara manual.
2. Pencatatan GR dijadikan **modal di dalam Detail PO**, bukan layar/route sendiri — karena `PurchaseOrderDetailResponse` **tidak menyertakan daftar GR yang sudah tercatat** (lihat gap kedua di bagian 8), sehingga GR hanya bisa dirujuk balik selama sesi yang sama. Route terpisah untuk GR tidak akan pernah bisa dibuka ulang tanpa ID yang sudah diketahui.
3. Tukar Faktur dibangun **berdiri sendiri** (PurchaseOrderId/GoodsReceiptId kosong secara default, diisi manual dengan menyalin ID dari URL PO/GR terkait) — karena tanpa Daftar PO/GR, tidak ada cara membangun dropdown pencarian yang jujur. Ini keterbatasan interim, dicatat di bagian 8.

**Gerbang komponen (`base-component-decision-gate.md`) dijalankan sebelum baris JSX pertama.** Tabel keputusan:

| Kebutuhan UI | Kandidat | Bukti | Status | Keterangan |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `finance-payable-view.jsx` | REUSE | — |
| Breadcrumb | `FinanceBreadcrumb` | `finance-payable-view.jsx` | REUSE | — |
| Notifikasi | `ToastStack` | `toast-stack.jsx` | REUSE | Prop asli `onClose`, **bukan** `onCloseToast` — sibling `finance-payable-view.jsx`/`finance-payment-ap-view.jsx` memakai nama prop yang salah (tidak terdaftar di komponen, diam-diam tidak berefek); tidak direplikasi di sini |
| Guard akses | `AccessDeniedGate` | `access-denied-gate.jsx` + `access-denied-utils.jsx` | REUSE | Diverifikasi: string error validasi biasa (mis. "Pilih supplier terlebih dahulu.") tidak memicu tampilan akses ditolak — deteksinya berbasis status 401/403 atau kata kunci spesifik |
| Dropdown Supplier | `ResourceFilterSelect` + `useSelectSuppliers` | `administrator-select-resources.js` baris 522 | REUSE | Resource `suppliers` sudah terdaftar lengkap dengan endpoint opsi |
| Tombol aksi | `BaseButton` | `base-button.jsx` | REUSE (deviasi dari sibling lama) | Katalog: *"Satu-satunya cara membuat tombol untuk fitur baru... Jangan memakai `.btn`/`.btn-primary` Bootstrap"*. Sibling `finance-payable-view.jsx`/`finance-payment-ap-view.jsx` masih pakai `.btn` Bootstrap — utang lama sebelum katalog ditegakkan, bukan pola untuk kode baru |
| Konfirmasi Approve/Reject/Cancel | `ConfirmModal` | `confirm-modal.jsx` | REUSE | `requireReason` dipakai untuk Reject PO (alasan penolakan wajib) |
| Field tunggal (supplier date, nomor faktur, dsb.) | `BaseTextField`/`BaseDateField` | `base-form-control.jsx` | REUSE | — |
| **Baris tabel item PO dan GR (dapat diedit)** | `BaseTextField` di dalam sel tabel | dicoba, ditemukan tidak cocok | **DITOLAK → input polos** | `BaseFormControl` (dipakai `BaseTextField`) **selalu merender `<label>`** — `useFieldIdentity` jatuh balik ke nama field bila `label` kosong, tidak ada jalan menyembunyikannya tanpa mengubah base component (mengubahnya berarti `EXTEND` yang berdampak ke seluruh pemakai lain, dilarang tanpa persetujuan terpisah). Sel tabel padat dengan header kolom tidak butuh label berulang. Jatuh ke `<input className="form-control form-control-sm">` polos — pola yang **sama** dengan tabel item pada modal `finance-payable-view.jsx` yang sudah ada |

`UI GATE (Create/Detail/GR): 9 elemen — REUSE 8, ditolak-dan-diganti-input-polos 1 (item tabel), NEW 0`

**Gerbang komponen lanjutan (Daftar PO, Daftar Tukar Faktur, 30 September 2026):**

| Kebutuhan UI | Kandidat | Bukti | Status | Keterangan |
| --- | --- | --- | --- | --- |
| Susunan halaman list | `DataFilter` + `DataTable` + `Pagination` | `finance-receivable-view.jsx` | REUSE | Dipilih sebagai referensi visual, **bukan** `finance-payable-view.jsx` — ditemukan `finance-payable-view.jsx` memakai kontrak prop `DataTable` yang sudah usang (`isLoading`, `emptyMessage`, kolom `label`) yang TIDAK dikenali `data-table.jsx` saat ini (kontrak sebenarnya: `loading`, `emptyTitle`/`emptyDescription`, kolom `header`) — dicek langsung ke source `data-table.jsx` sebelum menulis, bukan menyalin sibling apa adanya. Cacat ini tidak direplikasi di sini, dan dicatat sebagai temuan pada bagian 8, bukan diperbaiki di file yang bukan miliknya |
| Filter status | `FilterSelect` | `finance-receivable-view.jsx` | REUSE | `onChange` menerima `(value, option)` langsung — bukan event DOM. `finance-payable-view.jsx` memakai `onChange={(e) => ...e.target.value}` yang juga sudah salah terhadap kontrak `filter-select.jsx` saat ini; tidak direplikasi |
| Filter tanggal (Daftar PO) | `FilterDatePicker` | `finance-receivable-view.jsx` | REUSE | Hanya dipasang pada Daftar PO (`PurchaseOrderQuery.DateFrom/DateTo`) — Daftar Tukar Faktur tidak memakainya karena `InvoiceExchangeQuery` backend tidak punya field rentang tanggal |
| Filter supplier | `ResourceFilterSelect` + `useSelectSuppliers` | Sudah dipakai Create PO/Tukar Faktur pada bagian sebelumnya | REUSE | Instance terpisah per layar Daftar, `serverSide` (pencarian nyata ke `GET /suppliers/options`, diperbaiki sesi ini — sebelumnya `pageSize: 200` tanpa `serverSide`, lihat "Lanjutan kedua") |
| Kotak pencarian bebas | `DataFilter` prop `searchKeyword`/`onSearchChange` | — | **TIDAK DIPAKAI** | `PurchaseOrderQuery`/`InvoiceExchangeQuery` backend tidak punya parameter search bebas — menambahkannya berarti pura-pura ada fitur yang sebenarnya cuma filter klien pada satu halaman data, menyesatkan pengguna. Dilaporkan sebagai keterbatasan, bukan ditutupi |
| Status baris tabel | `StatusBadge` | `finance-receivable-table-columns.jsx` | REUSE | Pertama kali dipakai pada feature Purchasing ini — sebelumnya Detail PO/Tukar Faktur memakai `<span className="badge bg-...">` polos (masih dipertahankan di Detail, tidak diubah karena di luar cakupan lanjutan ini) |
| Kolom Supplier pada tabel | Peta `supplierId -> nama`, di-resolve `use-supplier-name-resolver.jsx` lewat `GET /suppliers/{id}` per baris | — | REUSE (endpoint sudah ada) | Diperbaiki sesi ini — lihat "Lanjutan kedua" bagian ini untuk riwayat keterbatasan sebelumnya (`pageSize: 200`) |

`UI GATE (Daftar, lanjutan): 6 elemen — REUSE 6 (satu di antaranya sengaja TIDAK dipasang karena kontrak backend tidak mendukungnya), NEW 0`

**Lanjutan kedua (30 September 2026, sesi terpisah) — dua keterbatasan interim di atas diperbaiki, satu keterbatasan (`Idempotency-Key`) dikonfirmasi butuh task backend tersendiri:**

| Keterbatasan sebelumnya | Perbaikan |
| --- | --- |
| Kolom Supplier pada Daftar PO/Tukar Faktur hanya resolve nama untuk 200 supplier pertama (`useSelectSuppliers({ pageSize: 200 })`, gagal untuk supplier ke-201 dst.) | Diganti `use-supplier-name-resolver.jsx` — resolve nama per Id secara lazy lewat `GET /v1/administrator/master-data/suppliers/{id}` (endpoint detail yang sudah ada), dengan cache in-memory per komponen. Nol batasan jumlah supplier; jumlah panggilan dibatasi wajar oleh ukuran halaman (pageSize Daftar), bukan oleh total supplier di database |
| Filter supplier pada Daftar PO/Tukar Faktur hanya mencari di antara opsi yang sudah termuat (client-side), tidak `serverSide` seperti pada form Create | Ditambahkan `serverSide` pada kedua `ResourceFilterSelect` filter supplier — sekarang konsisten dengan pola yang sudah dipakai `invoice-exchange-form-view.jsx`/`purchase-order-form-view.jsx` |
| Form Tukar Faktur: PO/GR diisi manual dengan menyalin ID dari URL halaman detail, bukan dropdown pencarian | Diganti dropdown nyata — dua select resource baru terdaftar di `src/lib/hooks/select/finance/finance-select-resources.js` ("purchaseOrders", "goodsReceipts"), dipakai lewat `ResourceFilterSelect` yang sama seperti dropdown supplier. **Bukan kotak pencarian bebas** (kontrak `PurchaseOrderQuery`/`GoodsReceiptQuery` tidak punya parameter search) — melainkan cascading filter yang benar-benar didukung kontrak: dropdown PO disaring `supplierId` (dari supplier yang sudah dipilih), dropdown GR disaring `purchaseOrderId` (dari PO yang dipilih) atau `supplierId` saja bila PO dikosongkan. Memilih supplier baru mengosongkan PO dan GR; memilih PO baru mengosongkan GR — mencegah kombinasi yang secara data tidak mungkin valid (GR yang bukan milik PO terpilih) dipilih dari UI, sebelum sempat ditolak `400` oleh backend |
| `Idempotency-Key` belum dikirim karena backend belum membacanya | **Belum diperbaiki — dikonfirmasi perlu task backend tersendiri, bukan perluasan kecil.** Pola nyata idempotency HTTP header SUDAH ada di backend (`[FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey` dipakai luas di controller Billing dan satu controller Finance — `PettyCashBudgetController`), tapi mekanismenya menyimpan `IdempotencyKey` sebagai KOLOM PERSISTEN pada entity yang dibuat, dicek via query sebelum insert (`PettyCashBudgetService.cs`). Menerapkannya ke Purchasing berarti menambah kolom baru pada lima entity (`FinPurchaseOrder`, `FinGoodsReceipt`, `FinInvoiceExchange`, `FinPurchasingInvoice`, `FinSupplierReturn`) plus migration dan keputusan desain per jenis aksi (create vs status-transition punya semantik idempotency berbeda) — di luar wewenang `FRONTEND MODE` dan di luar cakupan "perbaiki keterbatasan FE-FIN-008" tanpa otorisasi task backend terpisah |

---

## 2. Proses bisnis dari sisi pengguna

**Melihat Daftar Purchase Order (lanjutan 30 September 2026).** Petugas AP membuka `/finance/purchasing/purchase-orders`, melihat tabel PO berpaging dengan kolom nomor PO, supplier, total, jenjang persetujuan, tanggal diajukan, dan status. Filter yang tersedia: supplier, status, dan rentang tanggal diajukan — persis sesuai kemampuan backend, nol kotak pencarian bebas (backend tidak menyediakannya). Klik dua kali pada baris membuka halaman detail PO tersebut. Tombol **+ Tambah Purchase Order** membuka layar Create.

**Melihat Daftar Tukar Faktur (lanjutan 30 September 2026).** Sama seperti Daftar PO, dibuka di `/finance/purchasing/invoice-exchanges`, kolom nomor Tukar Faktur, supplier, nomor & tanggal faktur supplier, estimasi jatuh tempo, dan status. Filter: supplier dan status saja (backend tidak punya filter tanggal untuk daftar ini).

**Menyusun Purchase Order.** Petugas AP membuka `/finance/purchasing/purchase-orders/new`, memilih supplier, mengisi satu atau lebih baris item (kategori, nama produk, satuan, kuantitas, harga satuan — subtotal dan total dihitung otomatis di layar untuk tampilan, bukan sebagai nilai otoritatif), lalu menekan **Simpan Purchase Order**. Sistem mengarahkan ke halaman detail PO yang baru dibuat.

**Mengajukan dan menyetujui.** Pada halaman detail, PO berstatus `DRAFT` menampilkan tombol **Ajukan**. Sesudah diajukan (`PENDING_APPROVAL`), penyetuju (bukan pengaju) melihat tombol **Setujui** dan **Tolak**; pengaju sendiri melihat keterangan bahwa ia tidak dapat menyetujui pengajuannya sendiri (dibandingkan `requestedByUserId` dengan ID pengguna saat ini dari cookie `userId`). Menolak mewajibkan alasan tertulis. Jenjang persetujuan (`ApprovalTier`) ditampilkan apa adanya dari response backend — **tidak dihitung ulang di klien**.

**Mencatat Tanda Terima Barang.** Setelah PO `APPROVED` (atau `PARTIALLY_RECEIVED`), tombol **Catat Penerimaan Barang** membuka modal berisi baris item PO beserta kuantitas yang sudah dipesan; petugas mengisi kuantitas yang benar-benar diterima per baris (boleh sebagian) beserta tanggal penerimaan. Sesudah disimpan, PO dimuat ulang untuk melihat status terbaru (`PARTIALLY_RECEIVED`/`FULLY_RECEIVED`, dihitung backend). **Lanjutan 30 September 2026**: seluruh GR yang sudah tercatat terhadap PO ini — dari sesi kapan pun, bukan hanya sesi berjalan — tampil di daftar "Tanda Terima Barang" beserta tombol **Batalkan** untuk yang masih `RECEIVED`, karena `PurchaseOrderDetailResponse` kini menyertakan riwayatnya (lihat bagian 1).

**Membatalkan PO.** Tersedia selama status `DRAFT`/`PENDING_APPROVAL`/`APPROVED` dan belum ada GR tercatat sama sekali terhadap PO ini (pembatalan PO ber-GR ditolak backend; frontend menyembunyikan tombolnya begitu satu GR tercatat, kini dicek dari riwayat penuh — bukan hanya sesi berjalan — sebagai penjaga tambahan, bukan pengganti validasi backend).

**Mencatat Tukar Faktur.** Petugas membuka `/finance/purchasing/invoice-exchanges/new`, memilih supplier, mengisi nomor dan tanggal faktur supplier serta tanggal dokumen diterima. PO/GR terkait bersifat opsional. **Lanjutan 30 September 2026**: begitu supplier dipilih, dropdown Purchase Order aktif dan menampilkan PO milik supplier tersebut; memilih satu PO mengaktifkan dropdown Tanda Terima Barang yang disaring untuk PO itu saja (atau, bila PO dikosongkan, seluruh Tanda Terima Barang milik supplier yang sama). Mengganti supplier mengosongkan pilihan PO dan GR; mengganti PO mengosongkan pilihan GR — mencegah kombinasi yang tidak mungkin valid dipilih dari layar. `EstimatedDueDate` **dihitung backend**, tidak diminta atau ditampilkan sebagai input. Sesudah disimpan, diarahkan ke halaman detail yang menampilkan jatuh tempo hasil hitungan backend beserta tombol **Batalkan Tukar Faktur** (hanya selama status `RECEIVED`).

**Jalur tidak normal:**

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Memuat detail PO/Tukar Faktur | Spinner beserta teks "Memuat detail..." |
| PO/Tukar Faktur tidak ditemukan (404) | Alert kuning "...tidak ditemukan." |
| Validasi form gagal (mis. supplier belum dipilih, kuantitas GR melebihi PO) | Alert merah di atas form, form tidak terkirim |
| Aksi ditolak backend (mis. `403` penyetuju = pengaju, `409` versi data berubah) | Toast merah berisi pesan backend apa adanya |
| Tanpa hak akses (401/403 murni akses) | `AccessDeniedGate` menggantikan seluruh konten dengan alert "Ups! Akses Ditolak" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Backend: `FinancePurchaseOrdersController.cs`, `FinanceGoodsReceiptsController.cs`, `FinanceInvoiceExchangesController.cs` beserta seluruh DTO dan model `FinPurchaseOrder`/`FinGoodsReceipt`/`FinInvoiceExchange` di bawah `Areas/Corporate/FinanceManagement/Purchasing/`; ketiga service-nya (untuk memastikan nol method list); `contracts/api-contract.md` §B.1-B.5, `contracts/permission-audit-matrix.md`, `02-frontend-roadmap.md`, `01-backend-roadmap.md` (status `BE-FIN-032`/`033`).
- Frontend: `finance-payable-view.jsx`, `finance-payment-ap-view.jsx` (modul referensi visual terdekat); `InstanceAxios.jsx` (konvensi base URL dan prefix `/v1/...`); `base-component-catalog.md`, `base-component-decision-gate.md`, `page-composition-patterns.md`; `base-button.jsx`, `base-form-control.jsx`, `confirm-modal.jsx`, `toast-stack.jsx`, `access-denied-gate.jsx`, `access-denied-utils.jsx`, `resource-filter-select.jsx` (kontrak props sebenarnya, bukan diasumsikan dari katalog); `administrator-select-resources.js`, `use-administrator-select.js` (resource `suppliers` sudah terdaftar); `AGENTS.md` frontend.
- **Lanjutan 30 September 2026** (Daftar PO/Tukar Faktur): `BE-FIN-032.md`/`BE-FIN-033.md` AMENDMENT (kontrak `GET /` yang sudah tersedia); `PurchaseOrderQuery`/`GoodsReceiptQuery`/`InvoiceExchangeQuery`, `PurchaseOrderGoodsReceiptSummaryResponse` pada DTO backend (dibaca read-only untuk bentuk query/response persis); `finance-receivable-view.jsx` + `finance-receivable-table-columns.jsx` (referensi visual list yang benar-benar dipakai — bukan `finance-payable-view.jsx`, lihat temuan bagian 1); source `data-filter.jsx`, `data-table.jsx`, `filter-select.jsx`, `filter-date-picker.jsx`, `resource-filter-select.jsx`, `status-badge.jsx`, `pagination.jsx`, `base-button.jsx`, `summary-grid.jsx`, `use-select-resource.jsx` (kontrak prop sebenarnya, bukan diasumsikan dari sibling); `page-composition-patterns.md` bagian "Halaman List" dan "Definisi Kolom Tabel".

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/purchasing/purchase-order-constants.jsx` | Baru — endpoint base, status, label, ambang jenjang persetujuan (tampilan saja) |
| `src/lib/constants/finance/purchasing/invoice-exchange-constants.jsx` | Baru — endpoint base, status, label |
| `src/utils/finance/purchasing/purchasing-display-utils.jsx` | Baru — `formatCurrency`, `formatDateDisplay`, `getApiErrorMessage` (pola lokal per-domain, konsisten sibling Finance) |
| `src/lib/hooks/finance/purchasing/use-purchase-order-editor.jsx` | Baru — state form Create PO, baris item dinamis, validasi klien, submit ke `POST /purchase-orders` |
| `src/lib/hooks/finance/purchasing/use-purchase-order-detail.jsx` | Baru — fetch detail, aksi submit/approve/reject/cancel, modal dan aksi GR (create + cancel) |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-editor.jsx` | Baru — state form Create Tukar Faktur |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-detail.jsx` | Baru — fetch detail, aksi cancel |
| `src/components/view/finance/purchasing/purchase-order/purchase-order-form-view.jsx` | Baru — layar Create PO |
| `src/components/view/finance/purchasing/purchase-order/detail/purchase-order-detail-view.jsx` | Baru — layar Detail PO + modal GR |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-form-view.jsx` | Baru — layar Create Tukar Faktur |
| `src/components/view/finance/purchasing/invoice-exchange/detail/invoice-exchange-detail-view.jsx` | Baru — layar Detail Tukar Faktur |
| `src/app/finance/purchasing/purchase-orders/new/page.jsx` | Baru — route tipis |
| `src/app/finance/purchasing/purchase-orders/[slug]/page.jsx` | Baru — route tipis |
| `src/app/finance/purchasing/invoice-exchanges/new/page.jsx` | Baru — route tipis |
| `src/app/finance/purchasing/invoice-exchanges/[slug]/page.jsx` | Baru — route tipis |

**Lanjutan 30 September 2026 — Daftar PO dan Daftar Tukar Faktur:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/purchasing/purchase-order-constants.jsx` | Diperluas — `PURCHASE_ORDER_STATUS_OPTIONS`, `DEFAULT_PAGE_SIZE_OPTIONS` untuk filter Daftar |
| `src/lib/constants/finance/purchasing/invoice-exchange-constants.jsx` | Diperluas — `INVOICE_EXCHANGE_STATUS_OPTIONS`, `DEFAULT_PAGE_SIZE_OPTIONS` |
| `src/lib/hooks/finance/purchasing/use-purchase-order-list.jsx` | Baru — filter, paginasi, fetch `GET /purchase-orders` |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-list.jsx` | Baru — filter, paginasi, fetch `GET /invoice-exchanges` |
| `src/lib/hooks/finance/purchasing/use-purchase-order-detail.jsx` | Diperbarui — `recordedGoodsReceipts` (state sesi, dihapus) diganti `goodsReceiptHistory` (dibaca dari `purchaseOrder.goodsReceipts`, persisten lintas sesi); `canCancel` kini memakai riwayat penuh; pembatalan GR mengambil rincian penuh (`GET /goods-receipts/{id}`) dulu untuk memperoleh `rowVersion` (ringkasan pada detail PO tidak menyertakannya); `goBack` kini menuju Daftar PO |
| `src/components/view/finance/purchasing/purchase-order/purchase-order-view.jsx` | Baru — layar Daftar PO |
| `src/components/view/finance/purchasing/purchase-order/purchase-order-table-columns.jsx` | Baru — definisi kolom Daftar PO |
| `src/components/view/finance/purchasing/purchase-order/detail/purchase-order-detail-view.jsx` | Diperbarui — breadcrumb ke Daftar PO; seksi riwayat GR memakai `goodsReceiptHistory`, teks "sesi ini" dihapus |
| `src/components/view/finance/purchasing/purchase-order/purchase-order-form-view.jsx` | Diperbarui — breadcrumb ke Daftar PO |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-view.jsx` | Baru — layar Daftar Tukar Faktur |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-table-columns.jsx` | Baru — definisi kolom Daftar Tukar Faktur |
| `src/components/view/finance/purchasing/invoice-exchange/detail/invoice-exchange-detail-view.jsx` | Diperbarui — breadcrumb ke Daftar Tukar Faktur |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-form-view.jsx` | Diperbarui — breadcrumb ke Daftar Tukar Faktur |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-detail.jsx` | Diperbarui — `goBack` kini menuju Daftar Tukar Faktur |
| `src/app/finance/purchasing/purchase-orders/page.jsx` | Baru — route tipis Daftar PO |
| `src/app/finance/purchasing/invoice-exchanges/page.jsx` | Baru — route tipis Daftar Tukar Faktur |

**Lanjutan kedua (30 September 2026, sesi terpisah) — kolom Supplier, filter supplier, dan dropdown PO/GR pada Tukar Faktur:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/purchasing/use-supplier-name-resolver.jsx` | Baru — resolve nama supplier per Id lazy lewat `GET /suppliers/{id}`, cache in-memory; menggantikan pendekatan `useSelectSuppliers({ pageSize: 200 })` yang terbatas 200 supplier pertama |
| `src/lib/hooks/finance/purchasing/use-purchase-order-list.jsx` | Diperbarui — mengembalikan `supplierNameById` dari `use-supplier-name-resolver.jsx` |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-list.jsx` | Diperbarui — sama |
| `src/components/view/finance/purchasing/purchase-order/purchase-order-view.jsx` | Diperbarui — `useSelectSuppliers()` default (bukan `pageSize: 200`) khusus untuk filter, `serverSide` ditambahkan; kolom tabel memakai `supplierNameById` dari hook, bukan dihitung di view |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-view.jsx` | Diperbarui — sama |
| `src/lib/hooks/select/finance/finance-select-resources.js` | Baru — registrasi select resource `purchaseOrders`/`goodsReceipts` (endpoint `GET /purchase-orders`, `GET /goods-receipts`, filter relasi `supplierId`/`purchaseOrderId`/`status`, nol parameter search — lihat komentar berkas) |
| `src/lib/hooks/select/finance/use-finance-select.js` | Baru — `useSelectPurchaseOrders`/`useSelectGoodsReceipts`, pola persis `use-administrator-select.js` |
| `src/lib/hooks/select/select-resource-registry.js` | Diperbarui — wiring `FINANCE_SELECT_RESOURCES` ke `getSelectResource`/`SELECT_RESOURCES` (berkas bersama seluruh app, di luar folder Purchasing — perubahan aditif murni: satu import baru, satu baris pada rantai `||`, satu spread pada merge object; nol baris existing diubah) |
| `src/components/view/finance/purchasing/invoice-exchange/invoice-exchange-form-view.jsx` | Diperbarui — dua `BaseTextField` ID manual diganti `ResourceFilterSelect` (PO di-cascade dari supplier, GR di-cascade dari PO/supplier); memilih supplier baru mengosongkan PO+GR, memilih PO baru mengosongkan GR |
| `src/lib/hooks/finance/purchasing/use-invoice-exchange-editor.jsx` | Diperbarui — komentar berkas saja, logika submit tidak berubah (`purchaseOrderId`/`goodsReceiptId` tetap string kosong → `undefined`) |

**Satu berkas di luar feature Purchasing tersentuh pada lanjutan kedua** (`select-resource-registry.js`) — aditif murni seperti dijelaskan di atas, bukan perubahan struktur; seluruh resource lain pada berkas itu tidak tersentuh.

### 3.3 Kepatuhan arsitektur frontend

Struktur folder mengikuti `page-composition-patterns.md`: `src/app/<domain>/<feature>/[slug]/page.jsx` → `src/components/view/<domain>/<feature>/detail/<feature>-detail-view.jsx`, `src/lib/hooks/<domain>/<feature>/`, `src/lib/constants/<domain>/<feature>/`. Route hanya merangkai (`import` + `return`), nol pemanggilan Axios atau markup lain di dalamnya. Domain `finance`, feature `purchasing/purchase-order` dan `purchasing/invoice-exchange`, mengikuti struktur `Areas/.../Purchasing/` pada backend.

Pola state **lokal per-view** (bukan Redux) dipilih mengikuti sibling terdekat di domain yang sama (`finance-payable-view.jsx`, `finance-payment-ap-view.jsx`) — keduanya juga transaksional dan sama-sama memakai `useState`/`InstanceAxios` langsung, bukan Redux slice. Ini **bukan** pola `BaseEditorView`/`BaseDetailView` (dipakai modul master data seperti `hr/master-data/job-level`) karena domain ini transaksional dengan baris item dinamis dan alur approve/reject/cancel multi-status — bentuk yang tidak dipetakan `heroActions` (`onBack`/`onUpdate`/`onDelete`) milik `BaseDetailView`.

Pola baris item yang dapat diedit **tidak** memakai abstraksi generik baru (`AGENTS.md`: *"jangan membuat hook atau view master-data generik... abstraksi luas lainnya"*) — array baris dikelola langsung di hook masing-masing fitur, tanpa factory atau komponen "repeater" bersama.

**Lanjutan 30 September 2026 — Daftar PO/Tukar Faktur.** Struktur mengikuti persis `page-composition-patterns.md` bagian "Halaman List": `<feature>-view.jsx` (bukan `<feature>-list-view.jsx`) bersebelahan dengan `<feature>-table-columns.jsx`, di root folder feature (sejajar dengan `<feature>-form-view.jsx` yang sudah ada, bukan di dalam subfolder `list/` — pola ini tidak dipakai modul mana pun yang diperiksa). Route baru `src/app/finance/purchasing/purchase-orders/page.jsx` dan `.../invoice-exchanges/page.jsx` hidup berdampingan dengan `new/` dan `[slug]/` yang sudah ada — App Router menyelesaikan segmen statis sebelum dinamis, pola yang sama sudah dipakai modul lain.

**Keputusan state lokal, bukan Redux, untuk Daftar (berbeda dari `finance-receivable-view.jsx`).** `finance-receivable-view.jsx` — kandidat referensi visual paling dekat untuk layar list — memakai Redux slice (`financeReceivable`). Feature Purchasing pada task ini sepenuhnya memakai state lokal (`useState`/`InstanceAxios` langsung) sejak Create/Detail dibangun sebelumnya. Membuat HANYA layar Daftar memakai Redux akan memecah satu feature menjadi dua arsitektur state berbeda tanpa alasan teknis — dipilih tetap state lokal untuk `use-purchase-order-list.jsx`/`use-invoice-exchange-list.jsx`, mengambil pola *susunan visual* `finance-receivable-view.jsx` (Hero/DataFilter/DataTable/Pagination, prop `DataTable`/`FilterSelect` yang benar) tanpa mengambil pola *state management*-nya. Ini bukan pola baru — persis `finance-payable-view.jsx` (list + state lokal), hanya dengan kontrak prop `DataTable`/`FilterSelect` yang diperbaiki mengikuti source sebenarnya (lihat temuan bagian 1).

**Kolom tabel di file terpisah, konvensi diadopsi baru pada feature ini.** `<feature>-table-columns.jsx` (dengan `header`, bukan `label`) belum dipakai Create/Detail PO/Tukar Faktur sebelumnya (keduanya tidak punya tabel list). Diperkenalkan di sini karena memang dipakai layar list, bukan perluasan pola ke layar lain.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner Bootstrap + teks "Memuat detail Purchase Order..." / "...Tukar Faktur..."; Daftar: `DataTable` menampilkan `loadingText` "Mengambil data Purchase Order/Tukar Faktur..." pada badan tabel |
| Kosong | Tidak berlaku untuk Create (form selalu siap diisi); Detail: "Purchase Order/Tukar Faktur tidak ditemukan." bila `404`; Daftar: `emptyTitle` "Data ... tidak ditemukan." + `emptyDescription` menyarankan mengganti filter atau menambah data baru |
| Gagal | Alert merah di atas form untuk validasi klien/`400`/`422`; toast merah untuk aksi (submit/approve/reject/cancel/GR) yang ditolak backend, pesan diambil apa adanya dari `response.data.message`; Daftar: `InformationAlert` merah di atas tabel bila `fetchList` gagal, disertai toast yang sama |
| Tanpa hak akses | `AccessDeniedGate` menggantikan seluruh konten dengan alert "Ups! Akses Ditolak" ketika error terdeteksi `401`/`403` atau memuat kata kunci akses — berlaku juga pada Daftar (dibungkus `AccessDeniedGate` yang sama) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Purchase Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/purchase-orders` | Daftar PO berpaging, disaring supplier/status/tanggal diajukan (lanjutan 30 September 2026) | `FinancePurchaseOrder : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}` | Memuat detail PO | `FinancePurchaseOrder : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchase-orders` | Menyusun PO baru | `FinancePurchaseOrder : Create` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}/submit` | Mengajukan PO | `FinancePurchaseOrder : Submit` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}/approve` | Menyetujui PO | `FinancePurchaseOrder : Approve` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}/reject` | Menolak PO | `FinancePurchaseOrder : Approve` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}/cancel` | Membatalkan PO | `FinancePurchaseOrder : Cancel` |

#### Corporate / Finance Management / Purchasing / Goods Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/goods-receipts` | Opsi dropdown Tanda Terima Barang pada form Create Tukar Faktur, disaring `purchaseOrderId`/`supplierId` (lanjutan 30 September 2026 — konsumsi kedua endpoint ini setelah Daftar PO/Tukar Faktur, lewat select resource `goodsReceipts`) | `FinanceGoodsReceipt : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/goods-receipts` | Mencatat penerimaan barang terhadap satu PO | `FinanceGoodsReceipt : Create` |
| `GET` | `/v1/corporate/finance-management/purchasing/goods-receipts/{id}` | Mengambil rincian penuh (termasuk `rowVersion`) sebelum membatalkan GR dari riwayat pada Detail PO (lanjutan 30 September 2026 — ringkasan `GoodsReceipts` pada `PurchaseOrderDetailResponse` tidak menyertakan `rowVersion`) | `FinanceGoodsReceipt : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/goods-receipts/{id}/cancel` | Membatalkan GR mana pun pada riwayat PO yang masih berstatus `RECEIVED` (sebelumnya hanya GR sesi berjalan) | `FinanceGoodsReceipt : Cancel` |

#### Corporate / Finance Management / Purchasing / Invoice Exchange

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/invoice-exchanges` | Daftar Tukar Faktur berpaging, disaring supplier/status (lanjutan 30 September 2026) | `FinanceInvoiceExchange : Read` |
| `GET` | `/v1/corporate/finance-management/purchasing/invoice-exchanges/{id}` | Memuat detail Tukar Faktur | `FinanceInvoiceExchange : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/invoice-exchanges` | Mencatat Tukar Faktur baru | `FinanceInvoiceExchange : Create` |
| `POST` | `/v1/corporate/finance-management/purchasing/invoice-exchanges/{id}/cancel` | Membatalkan Tukar Faktur | `FinanceInvoiceExchange : Cancel` |

#### Administrator — Supplier (dipakai ulang, bukan endpoint baru)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | endpoint resource `suppliers` (`administrator-select-resources.js`) | Opsi dropdown supplier pada Create PO, Create Tukar Faktur, dan filter Daftar PO/Tukar Faktur (lanjutan 30 September 2026) | Mengikuti hak akses resource `suppliers` yang sudah ada |
| `GET` | `/v1/administrator/master-data/suppliers/{id}` | **Lanjutan 30 September 2026** — resolve nama supplier per Id untuk kolom Supplier pada Daftar PO/Tukar Faktur (`use-supplier-name-resolver.jsx`), dipanggil lazy hanya untuk Id yang belum diketahui | Mengikuti hak akses resource `suppliers` yang sudah ada |

**Purchasing (dipakai ulang untuk dropdown, bukan endpoint baru — lihat bagian 5 di atas untuk kontrak lengkapnya):** `GET /purchasing/purchase-orders` dan `GET /purchasing/goods-receipts` juga dikonsumsi sebagai select resource (`finance-select-resources.js`) pada form Create Tukar Faktur (lanjutan 30 September 2026), disaring `supplierId`/`purchaseOrderId` — bukan panggilan terpisah di luar dua endpoint yang sudah terdaftar untuk Daftar PO/GR.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | `eslint . --quiet` selesai, exit code 0, nol output error | `PASS` | Log command (dijalankan latar belakang, keluaran kosong berarti nol error) |
| `npm run lint:errors` (lanjutan kedua — supplier resolver, select resource Purchasing, dropdown Tukar Faktur) | `eslint . --quiet` selesai, exit code 0, nol output error | `PASS` | Log command, dijalankan ulang setelah seluruh berkas lanjutan kedua ditulis |
| `npm run build` | — | `NOT RUN` | Tidak dijalankan pada task ini maupun lanjutannya; keputusan build eksplisit menunggu instruksi terpisah dari pemilik, konsisten dengan preferensi yang sudah dinyatakan sebelumnya di sesi ini |
| `npm run test:unit` | — | `NOT RUN` | Repository tidak memiliki test unit untuk domain Finance existing (`test-policy.md`: menulis test baru bersifat opsional, bukan gerbang selesai); tidak diminta eksplisit pada task ini |
| Uji manual runtime (create PO, ajukan, setujui/tolak, catat GR, create/cancel Tukar Faktur, Daftar PO/Tukar Faktur berpaging/filter) | — | `NOT FEASIBLE` | Task mode `FRONTEND MODE` — backend strict read-only, tidak ada wewenang menjalankan/memodifikasi database untuk menyediakan data uji hidup (supplier nyata, akun bertingkat Supervisor/Manajer Finance, data PO/Tukar Faktur berjumlah banyak untuk menguji paginasi). Migration `BE-FIN-041` (`DepositAppliedAmount`) yang menjadi prasyarat rumpun ini juga belum dieksekusi ke database manapun (dicatat pada laporan `BE-FIN-036`) |
| Kesesuaian nama field response (`poNumber`, `grNumber`, camelCase dari `PONumber`/`GRNumber`), termasuk field baru `totalData`/`totalPage`/`items` pada `PagedResult` dan `goodsReceipts` pada `PurchaseOrderDetailResponse` | Diasumsikan mengikuti `System.Text.Json` `CamelCase` policy standar .NET, konsisten pola field lain yang sudah diverifikasi di sibling (`payableNumber`, dst.) dan pola `PagedResult` yang sudah dipakai `finance-payable-view.jsx`/`finance-receivable-view.jsx` (`totalData`, `pageNumber`, dst.) | `NOT FEASIBLE` untuk verifikasi runtime | Tidak dapat dikonfirmasi tanpa memanggil API hidup — **MUST diverifikasi manual** saat lingkungan tersedia, lihat bagian 8 |

**Tidak dijalankan:** `npm run test:e2e`/`test:uat` — tidak diminta eksplisit dan lingkungan tidak mendukung tanpa backend live.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Daftar PO | **Terpenuhi** (lanjutan 30 September 2026) | `purchase-order-view.jsx` + `use-purchase-order-list.jsx`, `GET /purchase-orders` berpaging dengan filter supplier/status/tanggal — backend menambahkan kapabilitas ini pada `BE-FIN-032` AMENDMENT, sebelumnya "Belum terpenuhi" karena nol endpoint/method |
| Buat PO | Terpenuhi | `purchase-order-form-view.jsx` + `use-purchase-order-editor.jsx`, `POST /purchase-orders` |
| Rincian PO | Terpenuhi | `purchase-order-detail-view.jsx`, `GET /purchase-orders/{id}` |
| Ajukan/setujui/tolak/batal PO | Terpenuhi | Tombol aksi + `ConfirmModal`, keempat endpoint aksi PO |
| Catat GR terhadap PO | **Terpenuhi** (keterbatasan sebelumnya diperbaiki 30 September 2026) | Modal di Detail PO, `POST /goods-receipts`. Riwayat GR kini persisten lintas sesi — `PurchaseOrderDetailResponse.GoodsReceipts` (`BE-FIN-032` AMENDMENT), dibaca sebagai `goodsReceiptHistory` |
| Daftar Tukar Faktur | **Terpenuhi** (lanjutan 30 September 2026) | `invoice-exchange-view.jsx` + `use-invoice-exchange-list.jsx`, `GET /invoice-exchanges` berpaging dengan filter supplier/status — backend menambahkan kapabilitas ini pada `BE-FIN-033` AMENDMENT |
| Buat Tukar Faktur (PO/GR opsional) | **Terpenuhi** (keterbatasan sebelumnya diperbaiki 30 September 2026) | `invoice-exchange-form-view.jsx`. PO/GR kini ditautkan lewat dropdown pencarian ter-cascade (supplier → PO → GR), bukan ID manual — lihat bagian 1 "Lanjutan kedua" |
| Batal Tukar Faktur | Terpenuhi | `invoice-exchange-detail-view.jsx`, `POST /invoice-exchanges/{id}/cancel` |
| `ApprovalTier`/`EstimatedDueDate` ditampilkan dari response, tidak dihitung layar | Terpenuhi | `purchase-order-detail-view.jsx` dan `invoice-exchange-detail-view.jsx` membaca langsung dari field response, nol perhitungan klien |
| Tombol Setujui tidak tampil bagi pengaju | Terpenuhi | `isRequester` di `use-purchase-order-detail.jsx`, dibandingkan `requestedByUserId` vs cookie `userId` |
| Penolakan `403` jenjang menampilkan pesan backend apa adanya | Terpenuhi | `getApiErrorMessage` mengutip `response.data.message` tanpa modifikasi |
| Nol perhitungan uang/tanggal di klien | Terpenuhi | Total item di form Create PO murni tampilan (dilabeli sebagai subtotal per baris + total, nilai otoritatif tetap dari `TotalAmount`/`LineTotal` response backend pada Detail); `EstimatedDueDate` dan `ApprovalTier` keduanya dari backend |
| `Idempotency-Key` pada perintah uang | **Belum terpenuhi** | Kontrak `api-contract.md` mensyaratkan header `Idempotency-Key` pada `POST`/aksi PO, GR, Tukar Faktur. Source controller backend **tidak membaca header ini sama sekali** (diverifikasi: nol referensi `Idempotency-Key` di ketiga controller) — bukan kelalaian frontend, kontrak menuntut sesuatu yang backend belum implementasikan. Frontend **tidak** mengirim header ini karena tidak ada penerimanya; ditambahkan begitu backend membacanya, supaya tidak ada header yang dikirim tanpa efek dan disalahpahami sebagai sudah aman dari duplikasi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **(Riwayat, sudah selesai)** Tiga keterbatasan yang sebelumnya dilaporkan di sini — nol `GET /` berpaging, `PurchaseOrderDetailResponse` tanpa `GoodsReceipts` (keduanya backend, `BE-FIN-032`/`BE-FIN-033` AMENDMENT), dan resolusi nama supplier terbatas 200 pertama plus PO/GR ID manual (keduanya frontend, sesi ini) — sudah diperbaiki. **Satu keterbatasan tersisa, dikonfirmasi bukan pekerjaan frontend**: `Idempotency-Key` — lihat Masalah yang diketahui |
| Masalah yang diketahui | (a) ~~Layar Daftar PO dan Daftar Tukar Faktur tidak dibangun~~ — **SELESAI**; (b) ~~PO/GR pada form Tukar Faktur diisi manual via ID~~ — **SELESAI**, kini dropdown cascading (lihat bagian 1 "Lanjutan kedua"); (c) `Idempotency-Key` belum dikirim karena backend belum membacanya sama sekali — **dikonfirmasi sesi ini perlu task `BACKEND MODE` tersendiri** (kolom persisten baru pada lima entity + migration + keputusan desain per jenis aksi, mengikuti pola nyata `PettyCashBudgetController`/`Service`, lihat bagian 1); (d) Kasing nama field response (`poNumber`, dst.) masih diasumsikan dari konvensi `.NET` standar, belum diverifikasi terhadap API hidup; (e) ~~nama supplier pada Daftar hanya resolve 200 pertama~~ — **SELESAI**, lihat bagian 1 |
| Dependency backend | `BE-FIN-032`/`BE-FIN-033` tetap berstatus 🟡, tapi gap yang menahan task frontend ini sudah tertutup: `GET /` berpaging tersedia, riwayat GR persisten, `dotnet build` solusi penuh PASS 0 error. Sisa KNOWN ISSUES kedua task backend itu (penugasan role, kolom `RejectionReason`, selisih dokumen kontrak action `cancel`) tidak berdampak pada layar yang dibangun task frontend ini. Migration `BE-FIN-041` (`DepositAppliedAmount` pada `FinPayment`) tetap belum dieksekusi ke database manapun — masih menahan pengujian runtime ujung-ke-ujung rumpun Purchasing/AP secara umum (dicatat sejak laporan `BE-FIN-036`), bukan spesifik task ini. **Baru**: implementasi `Idempotency-Key` pada kelima endpoint Purchasing (PO, GR, Tukar Faktur, Purchasing Invoice, Supplier Return) belum punya task ID — direkomendasikan didaftarkan terpisah sebelum dikerjakan (bagian Langkah berikutnya) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` — permintaan pengguna eksplisit memilih memprioritaskan tiga keterbatasan frontend (supplier 200, Idempotency-Key, ID manual Tukar Faktur) di tengah invokasi `build-module-frontend` untuk `FE-FIN-009`; `FE-FIN-009` dihentikan bersih sebelum ada file ditulis (baru tahap membaca laporan `BE-FIN-034`/`035`), dilanjutkan kembali sebagai task terpisah |
| Status Git | `?? src/app/finance/purchasing/`, `?? src/components/view/finance/purchasing/`, `?? src/lib/constants/finance/purchasing/`, `?? src/lib/hooks/finance/purchasing/`, `?? src/lib/hooks/select/finance/`, `M src/lib/hooks/select/select-resource-registry.js`, `?? src/utils/finance/purchasing/` — seluruhnya berkas baru/diperbarui di dalam feature Purchasing kecuali satu berkas registry bersama (aditif murni, lihat bagian 3.2) |
| Langkah berikutnya | (1) Daftarkan dan kerjakan task backend tersendiri untuk `Idempotency-Key` pada kelima endpoint Purchasing (Masalah yang diketahui butir c); (2) Otorisasi terpisah untuk eksekusi migration `BE-FIN-041` supaya uji manual runtime dapat dijalankan; (3) Verifikasi manual kasing field response begitu lingkungan tersedia; (4) Lanjutkan `FE-FIN-009` (Purchasing Invoice, Retur Pembelian/Deposit Retur) yang sempat dihentikan |
