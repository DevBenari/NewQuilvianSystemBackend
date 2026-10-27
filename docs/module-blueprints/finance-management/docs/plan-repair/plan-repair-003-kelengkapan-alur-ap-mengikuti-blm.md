# PLAN-REPAIR-003 — Kelengkapan alur Finance AP mengikuti perilaku BLM

```yaml
plan_id: PLAN-REPAIR-FIN-003
status_rencana: SEBAGIAN_DIKERJAKAN
tanggal: "2026-10-27"
diputuskan_oleh: "pemilik modul (instruksi 2026-10-27: implementasikan dan perbaiki sesuai Master Prompt Implementasi AP Quilvian Mengikuti BLM BE-FE)"
basis_source_backend: "edcd362a (Yasmina)"
basis_source_frontend: "e59f55f7 (yasmina)"
referensi_bisnis_read_only: "BLM PurchasingSystemRSMMCNew @ 08e8a9ae (PurchasingSystemDeveloper) — tidak diubah"
task_id: "BELUM DIALOKASIKAN — ID task canonical diberikan lewat plan-module-delivery; dokumen ini sengaja tidak mengarangnya"
migration: "TIDAK ADA yang dibuat atau dijalankan"
build_dan_uji_runtime: "NOT RUN"
```

Dokumen ini mencatat apa yang **benar-benar** diubah, apa yang **tidak diaktifkan** karena
menunggu schema atau keputusan, dan apa yang **belum dibuktikan** karena build, migration, server,
dan UAT memang tidak dijalankan pada pekerjaan ini. Jangan membacanya sebagai "AP sudah sama dengan
BLM" — itu belum terbukti.

---

## 1. Ringkasan dalam bahasa sederhana

Alur AP Quilvian dipisah menjadi beberapa dokumen (Pesanan Pembelian, Penerimaan Barang, Tukar
Faktur, Invoice Supplier, Utang, Pembayaran). BLM menyimpan sebagian besar fakta itu dalam satu
dokumen penerimaan. Pemisahan Quilvian tetap dipertahankan; yang ditambahkan adalah **penghubung
dan alat kerja** supaya petugas bisa mengikuti satu transaksi dari awal sampai akhir.

Yang sudah bisa dipakai setelah kode ini di-build dan di-deploy oleh pihak berwenang:

1. **Jejak dokumen** — dari satu dokumen, petugas melihat seluruh rantainya beserta tautan, tanpa
   menyalin ID.
2. **Antrean Penerimaan Invoice** — menu "Penerimaan Invoice" kini berisi pekerjaan nyata, bukan halaman petunjuk.
3. **Tutup PO** — penutupan administratif yang tidak lolos hanya karena belum ada Tukar Faktur.
4. **Koreksi Penerimaan Barang dan Tukar Faktur** — hanya selama belum dipakai dokumen turunan yang berlaku.
5. **Jalur catat ulang** Tukar Faktur setelah invoice dibatalkan atau ditolak.
6. **Pengaman**: nomor faktur ganda ditolak lebih awal, kunci idempotensi terikat ke aksi dan dokumen, dan pembatalan utang dinonaktifkan sampai kontrak pembalikan Accounting ada.

Yang **belum** bisa dipakai (butuh schema dan/atau keputusan): satu faktur untuk beberapa
Penerimaan Barang, penyimpanan Surat Jalan (SJ) terpisah, UOM/kondisi/produk belum terpetakan, dan
pembalikan buku saat utang dibatalkan. Rinciannya di bagian 6 dan 9.

---

## 2. Peta alur akhir

```text
Pesanan Pembelian (PO)                 /finance/purchasing/purchase-orders[/{id}]
   │  setujui (maker-checker, tier)
   ▼
Penerimaan Barang (GR) — sebagian/penuh, batch & kedaluwarsa
   │  dicatat & dikoreksi dari halaman PO     (tidak punya halaman detail sendiri)
   │  daftar: /finance/purchasing/goods-receipts (klik dua kali → PO induk)
   ▼
Penerimaan Invoice (antrean)           /finance/purchasing/purchasing-invoices?stage=intake
   │  "Faktur Menyusul" → "Siap Disusun Invoice" → "Dalam Verifikasi" → "Perlu Dicatat Ulang"
   ▼
Tukar Faktur Supplier                  /finance/purchasing/invoice-exchanges[/new|/{id}]
   │  koreksi data · batalkan · batalkan & catat ulang
   ▼
Invoice Supplier (verifikasi)          /finance/purchasing/purchasing-invoices[/new|/{id}|/{id}/edit]
   │  ajukan → setujui (SATU transaksi: utang + movement + outbox + TF LINKED)
   ▼
Utang Supplier                         /finance/payable/invoice · /finance/payable/{id}/movements
   ▼
Pembayaran Supplier / Retur / Penyesuaian   /finance/payment-ap · /finance/purchasing/supplier-returns
   ▼
Tutup PO (administratif)               tombol "Tutup PO" di halaman PO — tidak membayar apa pun
```

Action kontekstual yang tersedia (semua tetap dijaga permission dan status di backend):

| Halaman | Action baru | Syarat tampil |
|---|---|---|
| PO | Jejak Dokumen, Kemajuan Penerimaan per Baris | selalu |
| PO | **Tutup PO** | status `FULLY_RECEIVED`; tombol nonaktif + alasan bila belum siap |
| PO › riwayat GR | **Koreksi** | GR `RECEIVED` tanpa Tukar Faktur aktif |
| Tukar Faktur | Jejak Dokumen, **Koreksi Data** | `RECEIVED` tanpa invoice yang berlaku |
| Tukar Faktur | **Batalkan & Catat Ulang** | `RECEIVED` dan seluruh invoice-nya `CANCELLED`/`REJECTED` |
| Invoice Supplier | Jejak Dokumen | selalu |
| Antrean Penerimaan Invoice | Catat Tukar Faktur / Susun Invoice / Lanjutkan Verifikasi / Tinjau & Catat Ulang | sesuai kategori baris |

---

## 3. Register pengerjaan P01–P07

Label: **REUSE** (dipakai apa adanya), **EXTEND**, **REPAIR**, **BLOCKED** (menunggu schema/keputusan),
**NOT ACTIVATED** (source disiapkan, tidak terhubung ke alur aktif).

| Paket | Isi | Label | Status |
|---|---|---|---|
| P01 Penelusuran dokumen | Endpoint jejak dokumen + panel FE di PO/Tukar Faktur/Invoice; kemajuan penerimaan per baris PO | EXTEND | Source selesai · build/runtime **NOT RUN** |
| P02 Intake & kelengkapan | Antrean 4 kategori dari dokumen existing; isian awal formulir Tukar Faktur/Invoice dari antrean | EXTEND | Source selesai. **SJ terpisah: BLOCKED** (schema) |
| P03 Sumber, produk, UOM, kondisi | Hanya spesifikasi (bagian 6) | BLOCKED | Tidak ada kode; menunggu schema + keputusan katalog & kondisi |
| P04 Multi-GR & alokasi invoice | 2 entity + konfigurasi + `PurchasingInvoiceAllocationPlanner` (murni) + tes | NOT ACTIVATED | Tidak dipakai alur mana pun; menunggu schema + kebijakan tanggal |
| P05 PO, update, rebuild | Tutup PO; koreksi GR & Tukar Faktur; guard batal GR/TF; duplikat nomor faktur; jalur catat ulang | EXTEND / REPAIR | Source selesai. **Rebuild di atas TF yang sama: BLOCKED** (unique index) |
| P06 Cancel finansial & retry | Pembatalan utang dinonaktifkan (flag) + guard; replay idempotensi terikat aksi/dokumen; JSON replay disamakan | REPAIR | Source selesai. **Pembalikan mutasi/outbox: BLOCKED** (kontrak Accounting) |
| P07 Rekonsiliasi & dokumentasi | Dokumen ini; tampilan estimasi vs aktual terpisah pada jejak | REUSE / EXTEND | Selesai (dokumen) |

### Fakta source yang direuse (tidak dibangun ulang)

| Fitur | Bukti pemeriksaan statis | Label |
|---|---|---|
| Biaya invoice (materai/ongkir) | `FinPurchasingInvoice.StampDutyAmount/ShippingCostAmount`; `ValidateBalance`; `use-purchasing-invoice-editor.jsx`; tes `finance-purchasing-ap-blm.test.mjs` (lulus) | REUSE |
| Batch/kedaluwarsa & multi-batch per baris PO | `FinGoodsReceiptItem.BatchNumber/ExpiredDate`; `goods-receipt-utils.jsx` | REUSE |
| Supplier alokasi payment/TF + tier payment | `FinanceInvoiceExchangeService.CreateAsync`, `FinancePaymentService`, `FinanceApprovalAuthorizationService` | REUSE |
| Daftar invoice/payment/retur | `GetPagedAsync` ketiganya; FE `purchasing-invoice-view.jsx` | REUSE |
| Cancel invoice DRAFT/PENDING | `FinancePurchasingInvoiceService.CancelAsync` | REUSE |
| Jatuh tempo aktual vs estimasi | BE-FIN-124/127; jejak menampilkan `estimatedDueDate` (TF) dan `dueDate`+`dueDateBasis` (utang) terpisah | REUSE |
| Persetujuan invoice satu transaksi | `ApproveAsync` membuka transaksi Serializable; `FinanceSupplierPayableService.CreateAsync` bergabung bila `CurrentTransaction` ada | REUSE |

---

## 4. Kontrak endpoint (gaya Swagger)

Semua route di bawah `api/v1/corporate/finance-management/purchasing`. Envelope `ApiResponse<T>`.
Kode status: `400` isian salah · `403` tanpa hak · `404` tidak ditemukan · `409` data basi (RowVersion) atau bentrok · `422` aturan bisnis.

### Grup: Corporate / Finance Management / Purchasing / Purchase Order

| Method | Path | Permission | Request | Response | Catatan |
|---|---|---|---|---|---|
| POST | `/purchase-orders/{id}/close` | `FinancePurchaseOrder : Close` **(baru)** | `{ expectedRowVersion }` | `PurchaseOrderResponse` | 422 + alasan bila belum siap. Tanpa `Idempotency-Key` (lihat 4.6) |
| GET | `/purchase-orders/{id}/document-trail` | `FinancePurchaseOrder : Read` | — | `PurchasingDocumentTrailResponse` | **baru** |

### Grup: … / Goods Receipt

| Method | Path | Permission | Request | Response | Catatan |
|---|---|---|---|---|---|
| PUT | `/goods-receipts/{id}` | `FinanceGoodsReceipt : Update` **(baru)** | `{ expectedRowVersion, receivedDate, items[] }` (item sama dengan create) | `GoodsReceiptDetailResponse` | 422 bila ada Tukar Faktur aktif / PO tidak boleh / melebihi pesanan |
| GET | `/goods-receipts/{id}/document-trail` | `FinanceGoodsReceipt : Read` | — | `PurchasingDocumentTrailResponse` | **baru** |
| POST | `/goods-receipts/{id}/cancel` | `FinanceGoodsReceipt : Cancel` | tetap | tetap | **guard diubah**, lihat 5 |

### Grup: … / Invoice Exchange

| Method | Path | Permission | Request | Response | Catatan |
|---|---|---|---|---|---|
| PUT | `/invoice-exchanges/{id}` | `FinanceInvoiceExchange : Update` **(baru)** | `{ expectedRowVersion, supplierInvoiceNumber, supplierInvoiceDate, receivedDate }` | `InvoiceExchangeResponse` | Supplier & rujukan PO/GR tidak dapat diubah |
| GET | `/invoice-exchanges/{id}/document-trail` | `FinanceInvoiceExchange : Read` | — | `PurchasingDocumentTrailResponse` | **baru** |
| POST | `/invoice-exchanges` | tetap | tetap | tetap | **ditolak 409** bila nomor faktur sudah dipakai |
| POST | `/invoice-exchanges/{id}/cancel` | tetap | tetap | tetap | **ditolak 422** bila ada invoice DRAFT/PENDING/APPROVED |

### Grup: … / Purchasing Invoice

| Method | Path | Permission | Request | Response | Catatan |
|---|---|---|---|---|---|
| GET | `/purchasing-invoices/intake-queue` | `FinancePurchasingInvoice : Read` | `?category&supplierId&pageNumber&pageSize` | `PagedResult<PurchasingIntakeRow>` | **baru**; 403 bila tak berhak atas dokumen sumber kategori |
| GET | `/purchasing-invoices/intake-queue/summary` | `FinancePurchasingInvoice : Read` | `?supplierId` | `PurchasingIntakeSummaryResponse` | **baru**; kategori terlarang masuk `restrictedCategories` |
| GET | `/purchasing-invoices/{id}/document-trail` | `FinancePurchasingInvoice : Read` | — | `PurchasingDocumentTrailResponse` | **baru** |

### Grup: … / Supplier Payable (modul Payable)

| Method | Path | Perubahan |
|---|---|---|
| POST | `/payables/{id}/cancel` | Selalu `422` sampai kontrak pembalikan disahkan (4.5) |

### 4.1 `PurchasingDocumentTrailResponse`

```json
{
  "focusDocumentType": "INVOICE_EXCHANGE",
  "focusDocumentId": "…",
  "purchaseOrder": { "id": "…", "poNumber": "PO-…", "status": "FULLY_RECEIVED", "supplierId": "…", "supplierName": "PT Kimia Farma", "totalAmount": 1000000, "rowVersion": "…" },
  "itemProgress": [ { "purchaseOrderItemId": "…", "productName": "Parasetamol", "unit": "BOX", "orderedQuantity": 100, "receivedQuantity": 60, "remainingQuantity": 40 } ],
  "goodsReceipts":   [ { "id": "…", "grNumber": "GR-…", "receivedDate": "2026-10-10", "status": "RECEIVED", "totalReceivedQuantity": 60, "itemCount": 2 } ],
  "invoiceExchanges":[ { "id": "…", "exchangeNumber": "TF-…", "supplierInvoiceNumber": "INV-KF-1", "estimatedDueDate": "2026-11-09", "status": "LINKED_TO_INVOICE", "goodsReceiptId": "…" } ],
  "invoices":        [ { "id": "…", "invoiceNumber": "PI-…", "status": "APPROVED", "totalAmount": 1024000 } ],
  "payables":        [ { "id": "…", "payableNumber": "AP-…", "status": "PARTIAL", "originalAmount": 1024000, "outstandingAmount": 524000, "dueDate": "2026-11-09", "dueDateBasis": "GOODS_RECEIVED_DATE" } ],
  "payments":        [ { "paymentId": "…", "paymentNumber": "PAY-…", "status": "PAID", "payableId": "…", "allocatedAmount": 500000 } ],
  "supplierReturns": [],
  "closure": { "canClose": false, "blockingReasons": ["Tanda Terima Barang GR-… belum memiliki Tukar Faktur."] },
  "restrictedSections": ["PAYMENTS"]
}
```

Aturan akses (AC-20): controller jangkar menjaga hak baca dokumen jangkar. Bagian lain dikosongkan bila
`AccessPermissionService.HasAccessAsync` untuk resource itu (`FinancePurchaseOrder`, `FinanceGoodsReceipt`,
`FinanceInvoiceExchange`, `FinancePurchasingInvoice`, `FinanceSupplierPayable`, `FinancePayment`,
`FinanceSupplierReturn`, aksi `Read`) bernilai salah, dan namanya dicantumkan di `restrictedSections`.
Alasan penghambat penutupan memuat nomor dokumen, jadi hanya dirinci bila pemanggil boleh membaca GR,
Tukar Faktur, dan invoice sekaligus.

### 4.2 Kategori antrean Penerimaan Invoice

| Kategori | Sumber (proyeksi, bukan tabel baru) | Tindak lanjut |
|---|---|---|
| `AWAITING_INVOICE` | GR `RECEIVED` tanpa Tukar Faktur aktif | Catat Tukar Faktur (formulir terisi supplier/PO/GR) |
| `READY_FOR_INVOICE` | Tukar Faktur `RECEIVED` tanpa invoice sama sekali | Susun Invoice (`?invoiceExchangeId=`) |
| `IN_VERIFICATION` | Invoice `DRAFT`/`PENDING_APPROVAL` | Lanjutkan verifikasi |
| `NEEDS_REBUILD` | Tukar Faktur `RECEIVED` yang semua invoice-nya `CANCELLED`/`REJECTED` | Batalkan & Catat Ulang |

Contoh: GR-A (10 Okt) belum punya faktur → muncul di "Faktur Menyusul" dengan umur "17 hari". Faktur
datang 12 Okt → petugas klik "Catat Tukar Faktur" → baris pindah ke "Siap Disusun Invoice" → setelah invoice
diajukan pindah ke "Dalam Verifikasi" → setelah disetujui hilang dari antrean (utang terbentuk).

### 4.3 Aturan penutupan PO (`PurchaseOrderClosurePolicy`)

PO boleh ditutup hanya bila **semua** benar: status `FULLY_RECEIVED`; ada minimal satu GR aktif; setiap GR
aktif punya minimal satu Tukar Faktur aktif yang `LINKED_TO_INVOICE` dengan invoice `APPROVED`; tidak ada Tukar
Faktur aktif `RECEIVED`; tidak ada invoice `DRAFT`/`PENDING_APPROVAL`.

Contoh: PO 100 unit diterima penuh lewat GR-1 dan GR-2, hanya GR-1 yang sudah menjadi invoice disetujui →
ditolak dengan alasan "GR-2 belum memiliki Tukar Faktur". Menurut matriks lama ("semua Tukar Faktur turunan
selesai") PO ini akan lolos karena hanya melihat Tukar Faktur yang ada.

### 4.4 Koreksi GR dan Tukar Faktur

* GR: seluruh isi diganti; baris lama **dihapus lunak** (nilai lama tetap di tabel + audit before/after); akumulasi per
  baris PO dihitung ulang tanpa GR ini; status PO dihitung ulang; dikunci `FIN_PURCHASE_ORDER_{id}` (sama dengan create/cancel GR dan create TF).
* Tukar Faktur: hanya nomor, tanggal faktur, tanggal diterima. `EstimatedDueDate` dihitung ulang **hanya** bila tanggal diterima berubah.

### 4.5 Pembatalan utang dinonaktifkan

`FinanceSupplierPayableService.CancelAsync` kini berhenti di `PayableCancellationReversalRatified = false`. Alasan:
pembatalan hanya mengganti `Status`; ia tidak menulis mutasi pembalik (posisi as-of dihitung dari
`FinSupplierPayableMovement`, jadi utang `CANCELLED` tetap terhitung) dan tidak menulis kejadian pembalik ke outbox
Accounting (kejadian `PENGAKUAN-HUTANG-SUPPLIER` yang sudah dibuat tetap berlaku di buku besar). Koreksi sementara: Penyesuaian
Utang (maker-checker) atau Retur Pembelian. Guard dependensi (utang dari invoice, alokasi pembayaran berjalan, penyesuaian,
mutasi selain pengakuan awal) sudah ditulis di bawah saklar itu. **Mengubah saklar menjadi `true` tidak cukup** — mutasi
dan outbox pembalik harus ditambahkan atomik terlebih dulu.

### 4.6 Idempotensi

* `TryReplayAsync(key, entityType, action, entityId?)` — kunci hanya mengulang hasil aksi yang **sama**. Dipakai untuk aksi lain / dokumen lain → `409`.
* Badan replay kini ditulis camelCase seperti keluaran MVC (sebelumnya PascalCase → klien membaca `data.grNumber` = `undefined`). Baris lama tetap PascalCase.
* FE kini mengirim `Idempotency-Key` per niat pada: catat GR, batalkan GR, catat Tukar Faktur, batalkan Tukar Faktur, susun invoice, ajukan/setujui invoice, ajukan/setujui/batalkan PO.
* **Belum**: pembandingan isi permintaan (fingerprint) dan penutupan jendela antara commit bisnis dan penyimpanan baris replay — butuh schema (6.6). `Close` dan `Update` **tidak** memakai ledger karena check constraint `CK_FinPurchasingIdempotencyRecord_Action` hanya memuat `Create/Submit/Approve/Cancel/Confirm`.

---

## 5. Delta kontrak terhadap dokumen terkunci (belum bernomor)

`contracts/api-contract.md`, `permission-audit-matrix.md`, dan `state-transition-matrix.md` adalah kontrak terkunci yang
diubah lewat task bernomor; dokumen ini **tidak** menyuntingnya. Perubahan yang harus dimasukkan saat task dialokasikan:

| Kontrak | Delta |
|---|---|
| `api-contract.md` §B.1–B.4 | Endpoint pada bagian 4 di atas (9 baru: tutup PO, 4 jejak dokumen, 2 antrean, PUT GR, PUT Tukar Faktur; 4 berubah perilaku: catat Tukar Faktur, batal Tukar Faktur, batal GR, batal utang) |
| `permission-audit-matrix.md` | Pasangan baru `FinancePurchaseOrder:Close`, `FinanceGoodsReceipt:Update`, `FinanceInvoiceExchange:Update`. Terdaftar otomatis oleh seeder; **tidak diberikan ke role mana pun** sampai admin mencentangnya |
| `state-transition-matrix.md` §B.1 | `FULLY_RECEIVED → CLOSED` ditambah syarat "setiap GR aktif tertutup Tukar Faktur `LINKED` ber-invoice `APPROVED`; tanpa GR aktif tidak dapat ditutup" |
| §B.2 | "Batalkan GR": Tukar Faktur `CANCELLED` tidak menahan **selama** tak ada invoice aktif di bawahnya. Ditambah transisi "Koreksi isi" (RECEIVED → RECEIVED) |
| §B.3 | "Batalkan TF" ditolak bila ada invoice DRAFT/PENDING/APPROVED. Ditambah "Koreksi data". Nomor faktur unik per supplier di antara TF aktif dan utang |
| §5 (utang) | "Batalkan utang" dinonaktifkan sampai kontrak pembalikan ada |

---

## 6. Spesifikasi schema yang masih diperlukan

**Tidak ada migration yang dibuat atau dijalankan, tidak ada SQL/DDL yang dieksekusi, dan tidak ada kolom yang ditambahkan ke entity existing.**
Alasan yang terakhir: menambah kolom ke entity yang sedang dipakai membuat setiap query existing memilih kolom yang belum ada di database
(alur existing langsung rusak sebelum migration diterapkan — AC-19).

### 6.1 Sudah ada sebagai source (NOT ACTIVATED), tabel belum ada

| Tabel | Kolom | FK | Index / constraint |
|---|---|---|---|
| `FinInvoiceExchangeGoodsReceipt` | `Id` (PK), `InvoiceExchangeId`, `GoodsReceiptId`, kolom audit `IdentityModel` | → `FinInvoiceExchange.Id` (Restrict), → `FinGoodsReceipt.Id` (Restrict) | unik `(InvoiceExchangeId, GoodsReceiptId)` filter `IsDelete=false`; index `GoodsReceiptId` |
| `FinPurchasingInvoiceItemAllocation` | `Id`, `PurchasingInvoiceItemId`, `GoodsReceiptItemId`, `AllocatedQuantity numeric(18,2)`, `AllocatedAmount numeric(18,2)`, audit | → `FinPurchasingInvoiceItem.Id`, → `FinGoodsReceiptItem.Id` (Restrict) | unik `(PurchasingInvoiceItemId, GoodsReceiptItemId)` filter `IsDelete=false`; index `GoodsReceiptItemId`; `CHECK AllocatedQuantity > 0`; `CHECK AllocatedAmount >= 0` |

File: `Models/FinInvoiceExchangeGoodsReceipt.cs`, `Models/FinPurchasingInvoiceItemAllocation.cs`, dua `*Configuration.cs`,
`Services/PurchasingInvoiceAllocationPlanner.cs`. **Tanpa DbSet, tanpa query, tanpa pemanggil.**

> Karena konfigurasi dimuat `ApplyConfigurationsFromAssembly`, `dotnet ef migrations add` berikutnya oleh siapa pun akan memasukkan dua tabel ini ke
> migration. Itu disengaja, tetapi pemilik harus meninjau isi migration sebelum menerapkannya. Snapshot model **belum** diperbarui.

Syarat aktivasi (urutan): (1) kebijakan multi-GR & basis tanggal disahkan → (2) pemilik membuat & menerapkan migration → (3) tulis service
aktivasi (kunci `FIN_PURCHASE_ORDER_{id}`, baca `InvoicedQuantity` di dalam kunci, panggil planner, tulis bundel + alokasi dalam satu transaksi) →
(4) satu **penyelesai kanonik** "himpunan GR sebuah Tukar Faktur" = baris `FinInvoiceExchangeGoodsReceipt` ∪ kolom lama `GoodsReceiptId`;
jejak, guard batal/koreksi GR, tutup PO, antrean, dan jatuh tempo membaca dari penyelesai itu, bukan dari dua sumber → (5) FE pilih GR/baris/qty.
Cakupan awal usulan: satu supplier, satu PO, beberapa GR aktif. Pemilik dependensi: Pemilik Finance (kebijakan), pemilik repositori/deployment (migration).

### 6.2 Spesifikasi saja — belum ada kode

| # | Kebutuhan | Rancangan | Pemilik keputusan |
|---|---|---|---|
| 6.2.1 | Surat Jalan & kelengkapan faktur terpisah (P02/P03) | `FinGoodsReceiptDocument`: `GoodsReceiptId` FK, `DocumentType` (`DELIVERY_NOTE`/`SUPPLIER_INVOICE`), `DocumentNumber`, `DocumentDate`, `ReceivedAt`, `EvidenceReference`, `Status`. SJ **bukan** nomor faktur dan tidak boleh menjadi dasar utang | Pemilik Finance |
| 6.2.2 | Identitas sumber BLM / AdMedika (P03) | `FinPurchasingSourceIdentity`: `SourceSystem`, `SourceDocumentType`, `SourceDocumentId`, `LocalDocumentType`, `LocalDocumentId`; unik `(SourceSystem, SourceDocumentType, SourceDocumentId)`; menjadi pengaman duplikat di samping unik nomor faktur per supplier. `SourcePurchasingInvoiceId` tetap rujukan invoice internal | Pemilik Finance + pemilik integrasi |
| 6.2.3 | Produk, UOM, kondisi per baris GR (P03) | `FinGoodsReceiptItemSourceTrace`: `GoodsReceiptItemId`, `SourceProductId/Code`, `ProductMappingStatus` (`UNMAPPED`/`MAPPED`), `MappedProductId?` (nullable, tanpa FK sampai katalog sah), `SourceUomCode`, `ComparisonUomCode`, `ConversionFactor`, `NormalizedQuantity`, `ConditionCode` (fakta saja). 1 box = 10 tablet: Rp100.000/box ≡ Rp10.000/tablet | Pemilik katalog + pemilik persediaan |
| 6.2.4 | Susun ulang invoice di atas Tukar Faktur yang sama (P05) | Ubah `IX_FinPurchasingInvoice_InvoiceExchangeId` menjadi unik hanya untuk invoice berlaku (`"IsDelete" = false AND "Status" NOT IN ('CANCELLED','REJECTED')`) **dan** ubah `alreadyUsed` di `FinancePurchasingInvoiceService.CreateAsync` serasi. Sampai itu terjadi jalurnya batalkan TF → catat ulang | Pemilik Finance |
| 6.2.5 | Pembalikan utang (P06) | Jenis mutasi pembalik pada `FinSupplierPayableMovement.MovementType` (+ check constraint bila ada); kode kejadian pembalik & perlakuan PPN/tanggal bisnis/periode dari Accounting; mutasi + outbox + status ditulis atomik; idempoten | Accounting + Pemilik Finance |
| 6.2.6 | Idempotensi penuh (P06) | Kolom/tabel samping `RequestFingerprint` (hash payload) pada ledger; perluas `CK_…_Action` dengan `Close`,`Update`; tulis baris ledger **dalam transaksi bisnis yang sama** (butuh service yang bergabung ke transaksi ambient) | Pemilik repositori |
| 6.2.7 | Metadata penutupan PO | Opsional: `ClosedAt`, `ClosedByUserId`, `CloseReason` pada `FinPurchaseOrder`. Sekarang jejaknya hanya audit log `FinancePurchaseOrder.Close` | Pemilik Finance |

Urutan deploy untuk kolom/tabel pada entity yang sudah dipakai: **migration diterapkan dulu, baru kode yang memetakan kolom itu di-deploy.**
Backfill: tidak ada yang wajib — semua kolom baru nullable atau tabel baru kosong; dokumen lama tetap valid (batch/kedaluwarsa/`DueDateBasis` boleh kosong).

---

## 7. Matriks padanan BLM → Quilvian

| BLM | Entity / service BE | Route / komponen FE | Label |
|---|---|---|---|
| `PchPurchaseOrder/Item` | `FinPurchaseOrder/Item` · `FinancePurchaseOrderService` | `/finance/purchasing/purchase-orders` · `PurchaseOrderDetailView` | REUSE + EXTEND (tutup PO) |
| Persetujuan Pembelian | `FinanceApprovalAuthorizationService` (tier Finance) | tombol Setujui/Tolak di PO | REUSE. Approval pengadaan khusus: **BLOCKED** (keputusan) |
| `WrhReceiveOrder/Item` | `FinGoodsReceipt/Item` · `FinanceGoodsReceiptService` | PO › Catat/Koreksi Penerimaan; `/goods-receipts` | EXTEND (koreksi) |
| `PchPurchaseOrderReceipt` (akumulasi) | dihitung dari GR aktif · `FinancePurchasingDocumentTrailService` | panel "Kemajuan Penerimaan per Baris" | EXTEND |
| Faktur/SJ (`UpdateInvoiceReceiveOrder`) | `FinInvoiceExchange` · `FinanceInvoiceExchangeService` | `/invoice-exchanges`; antrean Penerimaan Invoice | EXTEND. SJ terpisah **BLOCKED** |
| Biaya & snapshot faktur | `FinPurchasingInvoice/Item` | `/purchasing-invoices` | REUSE |
| DueDate dokumen sumber | `FinSupplierPayable.DueDate/DueDateBasis` | jejak + daftar invoice | REUSE |
| Selisih qty / approval edit qty | bukan padanan adjustment nominal / deposit retur | — | tidak digabung |
| Satu faktur banyak penerimaan | `FinInvoiceExchangeGoodsReceipt` + `FinPurchasingInvoiceItemAllocation` | — | NOT ACTIVATED / BLOCKED |
| Produk/UOM/kondisi BLM | spesifikasi 6.2.3 | — | BLOCKED |

---

## 8. Matriks acceptance AC-01 – AC-20

Kolom "Status" hanya memakai: **SOURCE** (kode ada, ditinjau statis), **SEBAGIAN**, **BLOCKED**, **NOT ACTIVATED**.
**Tidak ada satu pun AC yang dinyatakan lulus runtime/UAT.** Tes ditulis tetapi `NOT RUN` kecuali dinyatakan lain.

| AC | Status | Bukti statis (file/method) | Tes / skenario |
|---|---|---|---|
| 01 | SOURCE | `FinanceGoodsReceiptService.CreateAsync` (akumulasi) tetap; `FinancePurchasingDocumentTrailService` (`receivedByPoItem`, GR batal tidak dihitung); `goods-receipt-utils.jsx` | FE `AC-01…` **lulus (unit)**; BE `GoodsReceipt_Update_ReplacesItems…` NOT RUN |
| 02 | SOURCE | `GoodsReceiptItemRequest.BatchNumber/ExpiredDate`; koreksi mengganti batch utuh; `buildGoodsReceiptRowsFromDetail` | FE `AC-02…` **lulus (unit)**; BE test NOT RUN |
| 03 | SEBAGIAN | Kategori `AWAITING_INVOICE` (GR tanpa faktur); SJ tidak menjadi AP (AP hanya dari invoice disetujui) | **SJ sebagai fakta terpisah: BLOCKED** (6.2.1) |
| 04 | NOT ACTIVATED | `PurchasingInvoiceAllocationPlanner`, 2 entity + konfigurasi | `PurchasingPureRulesTests.Allocation_*` NOT RUN; aktivasi BLOCKED (6.1) |
| 05 | SOURCE (reuse) | `FinanceInvoiceExchangeService.CreateAsync` (supplier PO/GR); `FinancePaymentService`; FE menampilkan `message` backend (`getApiErrorMessage`) | `FinanceInvoiceExchangeSupplierConsistencyTests` (existing) NOT RUN |
| 06 | BLOCKED (simpan) | Planner mencocokkan **per ID baris**; nilai = qty × harga satuan baris | `Allocation_ValueFollows…` NOT RUN; penyimpanan UOM butuh 6.2.3 |
| 07 | BLOCKED | Tidak ada pemetaan ke `MstMedicalEquipment`; tidak ada kode yang memilih master | Menunggu 6.2.3 |
| 08 | SOURCE (reuse) | `ValidateBalance`; `use-purchasing-invoice-editor.jsx` | `finance-purchasing-ap-blm.test.mjs` **lulus** |
| 09 | SOURCE (reuse) | `FinSupplierPayable.DueDate/DueDateBasis` vs `FinInvoiceExchange.EstimatedDueDate`; jejak & daftar invoice menampilkannya terpisah | UAT manual |
| 10 | SOURCE (reuse) | `ApproveAsync`: transaksi Serializable + `CurrentTransaction` dipakai bersama oleh `FinanceSupplierPayableService.CreateAsync`; rollback pada `catch` | Butuh PostgreSQL (kegagalan di tengah) — NOT RUN |
| 11 | SOURCE | `FinanceInvoiceExchangeService.EnsureSupplierInvoiceNumberUnusedAsync` (TF aktif + utang, tanpa beda huruf) | `FinanceApLifecycleGuardTests.Exchange_Create_*` NOT RUN |
| 12 | SOURCE (reuse) | Alur payment existing tidak diubah; jejak menampilkan utang & pembayaran | UAT manual |
| 13 | SOURCE (reuse) | `ApproveAsync` (self-approval, tier); tidak ada bypass baru. **Tutup PO bukan persetujuan** — dijaga permission `Close` saja | `FinancePaymentApprovalTierTests` (existing) NOT RUN |
| 14 | SEBAGIAN | `FinanceInvoiceExchangeService.CancelAsync` (guard) + FE "Batalkan & Catat Ulang" memakai `expectedRowVersion` dan riwayat tetap (`CANCELLED`) | `Exchange_Cancel_IsAllowedWhenOnlyCancelledInvoiceRemains` NOT RUN. **Susun ulang di TF yang sama: BLOCKED** (6.2.4) |
| 15 | BLOCKED | `FinanceSupplierPayableService.PayableCancellationReversalRatified = false` | Menunggu 6.2.5 |
| 16 | SOURCE | `PurchaseOrderClosurePolicy`; `FinancePurchaseOrderService.CloseAsync` (tidak menyentuh payment/saldo) | `PurchasingPureRulesTests.Closure_*`, `PurchaseOrder_Close_*` NOT RUN |
| 17 | SOURCE | Kunci `FIN_PURCHASE_ORDER_{id}` sama pada `UpdateAsync` GR, `CancelAsync` GR, `CreateAsync` TF; cek dependensi di dalam kunci | `GoodsReceipt_Update_IsRejectedWhileActiveExchange…` NOT RUN. **Perilaku bersamaan di PostgreSQL tidak dibuktikan** |
| 18 | SEBAGIAN | `PurchasingIdempotencyService.TryReplayAsync` mengikat entity/aksi/dokumen → 409 | `Idempotency_ReplaysSameAction…` NOT RUN. **Fingerprint payload & jendela commit: BLOCKED** (6.2.6) |
| 19 | SOURCE | Tidak ada kolom baru pada entity existing; entity P04 tanpa DbSet/query; `GetByIdAsync` GR hanya menyaring `IsDelete` | Review diff |
| 20 | SOURCE | `[AccessPermission]` pada setiap endpoint baru; bagian jejak/antrean diperiksa `HasAccessAsync`; permission baru tidak diberikan otomatis | UAT manual dengan role terbatas |

---

## 9. Keputusan dan blocker tersisa

| Item | Pemilik | Dampak | Yang tetap bisa dipakai |
|---|---|---|---|
| Kontrak pembalikan utang (kode kejadian, PPN, tanggal bisnis, periode) | Accounting + Pemilik Finance | Pembatalan utang tetap nonaktif | Penyesuaian Utang, Retur Pembelian |
| Kebijakan multi-GR & basis tanggal jatuh tempo agregat | Pemilik Finance | Satu faktur untuk banyak GR tidak aktif | Satu GR satu faktur (existing) |
| Penyimpanan SJ & status kelengkapan | Pemilik Finance | Antrean hanya menurunkan "faktur menyusul" dari keadaan dokumen | Antrean 4 kategori |
| Katalog alkes & perlakuan kondisi barang | Pemilik katalog/persediaan | Produk/UOM/kondisi BLM belum tersimpan terstruktur | Teks bebas existing |
| Matriks lifecycle: field editable, buka-kembali PO, susun ulang di TF yang sama | Pemilik Finance | Koreksi dibatasi ke aturan konservatif di bagian 4.4 | Batalkan & Catat Ulang |
| **Tukar Faktur tingkat PO (tanpa GR) yang sudah `LINKED`** | Pemilik Finance | Tidak dihitung menutup GR mana pun → PO dengan GR di bawahnya tidak dapat ditutup lewat jalur ini | Keputusan: apakah TF tingkat PO menutup semua GR PO tersebut |
| Approval pengadaan (PO/penerimaan) dan resolver tier | Pemilik pengadaan | Tier Finance bersama tidak diubah | Approval PO existing |
| Jadwal & pemilik penerapan schema | Pemilik repositori/deployment | Tabel P04 belum ada | — |

---

## 10. Skenario uji untuk pihak berwenang

Dijalankan setelah build, migration (bila ada), dan deploy oleh pihak berwenang. Permission baru harus dicentang dulu pada role uji.

1. **Jejak PO.** PO 100 unit → GR-1 60 (batch A 40, B 20) → buka PO. *Harapan:* panel menampilkan sisa 40; GR-1 bertaut ke PO; tanpa Tukar Faktur.
2. **Antrean.** Buka Penerimaan Invoice. *Harapan:* GR-1 di "Faktur Menyusul". Klik "Catat Tukar Faktur" → formulir terisi supplier/PO/GR.
3. **Nomor ganda.** Catat Tukar Faktur INV-1, lalu catat lagi `inv-1` (huruf kecil). *Harapan:* 409. Batalkan yang pertama → INV-1 dapat dicatat lagi.
4. **Koreksi GR.** Sebelum ada Tukar Faktur: ubah GR-1 jadi batch A 30 + B 30. *Harapan:* tersimpan, PO tetap sebagian, riwayat lama ada di database (baris `IsDelete`). Setelah Tukar Faktur dicatat: tombol Koreksi hilang; PUT langsung → 422.
5. **Tukar Faktur → invoice → catat ulang.** Susun invoice, batalkan invoice. *Harapan:* TF muncul di "Perlu Dicatat Ulang"; "Batalkan & Catat Ulang" → formulir baru terisi; invoice baru dapat disusun.
6. **Tutup PO.** PO diterima penuh lewat dua GR, hanya satu yang berinvoice disetujui. *Harapan:* tombol nonaktif + alasan GR kedua. Setujui invoice kedua → tombol aktif → PO `CLOSED`; utang tidak berubah.
7. **Pembatalan utang.** `POST /payables/{id}/cancel`. *Harapan:* 422 dengan penjelasan.
8. **Idempotensi.** Kirim `POST goods-receipts/{id}/cancel` dengan kunci K, lalu `POST goods-receipts` (create) dengan kunci K. *Harapan:* permintaan kedua 409. Ulang cancel dengan K → respons pertama (camelCase).
9. **Hak akses terbatas.** Role hanya `FinancePurchaseOrder:Read`. *Harapan:* panel jejak PO menyebut bagian terbatas (GR, Tukar Faktur, Invoice, …); tombol Koreksi/Tutup tidak berfungsi tanpa permission.
10. **Dokumen lama.** Buka draft invoice dan GR lama sebelum perubahan ini. *Harapan:* tampil dan dapat diproses seperti sebelumnya.

---

## 11. Batas validasi (jujur)

* **Tidak dijalankan:** `dotnet build`, migration, SQL/DDL, server BE/FE, `npm run build/dev`, UAT, perilaku kunci bersamaan di PostgreSQL.
* **Dijalankan:** `node --test` hanya untuk `finance-purchasing-ap-flow.test.mjs` (baru) dan `finance-purchasing-ap-blm.test.mjs` (existing) — **32 tes lulus**, seluruhnya fungsi murni; `npx eslint` pada berkas FE yang diubah — **0 error**, sisa peringatan `react-hooks/set-state-in-effect` pada pola fetch-di-effect yang sama dengan hook existing.
* **Kode C# belum dikompilasi.** Ditinjau manual; pembacaan IDE tidak melaporkan masalah tetapi itu bukan bukti kompilasi. Risiko terbesar ada pada kode C# baru — kompilasi dulu sebelum apa pun.
* Tes C# (`PurchasingPureRulesTests`, `FinanceApLifecycleGuardTests`) ditulis, **belum dijalankan**.
* Peringatan lingkungan: working tree FE memuat perubahan pengguna lain (Medical Fee, Receivable, `store.jsx`, `menu-items.jsx`) — tidak disentuh.
