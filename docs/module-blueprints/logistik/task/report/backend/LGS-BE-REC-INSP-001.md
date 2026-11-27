# Laporan Perubahan Backend — `LGS-BE-REC-INSP-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `LGS-BE-REC-INSP-001` (task ad-hoc, atas instruksi pemilik: "Pisahkan Pemeriksaan Hasil Pengadaan Logistik dari Penerimaan Barang Finance", 27 November 2026) |
| Judul | API baca khusus Logistik untuk "Pemeriksaan Hasil Pengadaan" dengan izin Logistik tersendiri |
| Slice | Pemisahan halaman Logistik dari Finance (pasangan frontend: `LGS-FE-REC-INSP-001`) |
| Roadmap | Tidak ada. Blueprint Logistik belum memiliki roadmap (`status: DISCOVERY`); task ini ad-hoc seperti `LGS-BE-ADH-01..04`, sehingga tidak ada tanda status yang dipasang |
| Trace | `LGS-DEC-001` (Logistik modul mandiri; Finance tetap pemilik Purchasing dan GR), `FIN-DEC-261`, `FIN-DEC-268`, `LGS-OQ-015`, `LGS-OQ-017`, `LGS-OQ-018` |
| Contract version | Belum ada kontrak bertversi (Logistik belum punya blueprint kontrak). Kontrak di laporan ini adalah **kontrak baca baru**, belum disetujui sebagai kontrak target |
| Dependency | Tidak ada task lain yang harus selesai lebih dulu |
| Klasifikasi | `MEDIUM` — skor 7 (repository 0, diperiksa 1, diubah 1, logika 1, API 1, database 1, keamanan 2, UI 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (source) dan laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `1cd7b477dc48799acc68792aea534d348b902975` (branch `Yasmina`), ditambah perubahan yang belum di-commit dari task `FIN-BE-LGS-LINK-001` |
| Tanggal | 27 November 2026 |
| Status | 🟡 **Source selesai, belum dibuild.** `dotnet build` **NOT RUN** (dilarang pada sesi ini). Tanpa migration, tanpa perubahan database |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `LogisticsManagement` / `Receiving` (folder baru, sesuai penempatan yang diminta pemilik pada instruksi task) |
| Pemilik / prefix registry | **Belum terdaftar.** `LogisticsManagement` tidak ada pada Module Ownership & Prefix Registry. Pemilik proses Logistik juga belum ditetapkan (`LGS-OQ-015`), dan prefix `Lgs` belum diajukan (`LGS-OQ-016`) |
| Keberlakuan | `NEW CODE` |
| Dampak QBE-MOD-002/003 | **Tidak memblokir task ini.** Kedua aturan hanya berlaku untuk folder yang memuat **model persisted**. Task ini tidak membuat entity, `DbSet`, configuration, maupun tabel. Setiap entity pemeriksaan di kemudian hari berstatus `BLOCKED` sampai pemilik registry menyetujui entri `LogisticsManagement` |
| QBE ID yang berlaku | QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-DTO-001, QBE-VAL-001, QBE-PAGE-001, QBE-MOD-001 |
| Hasil `Invoke-QbeConformanceCheck.ps1 -Path` | 4 berkas (controller, service, DTO, extension DI): `VIOLATION 0`, `REVIEW 0`, `INFO 0`, `Final result: PASS` |
| Arketipe endpoint | **Read-only** (monitoring/laporan), standar transaksi bagian 2.4. Tidak ada `POST`, `PUT`, `PATCH`, maupun `DELETE` |

---

## 1. Masalah yang diperbaiki

Pada sidebar **Logistik & Manajemen Persediaan → Penerimaan Barang → Pemeriksaan Hasil Pengadaan**, butir menu membuka halaman Finance (`/finance/purchasing/goods-receipts`) dan dijaga izin Finance `FinanceGoodsReceipt : Read`. Akibatnya:

- petugas Logistik harus diberi izin Finance hanya untuk membuka menu Logistik;
- halaman Finance menampilkan dokumen Goods Receipt (nomor GR, nomor PO, status dokumen), bukan hasil pemeriksaan barang;
- tidak ada API Logistik yang bisa dipakai halaman Logistik.

> **Contoh:** Petugas gudang membuka menu itu dan hanya melihat "GR-2026-0001 — Diterima". Ia tidak bisa tahu apakah dari 100 box yang tiba ada 20 box rusak dan 30 box masih menunggu pemeriksaan, padahal data itu sudah tersimpan di baris GR bersumber BLM.

Backend belum memiliki controller atau service Logistik untuk keperluan ini (dicek: tidak ada `LogisticsReceiving*` maupun `LogisticsInspection*` sebelum task ini).

---

## 2. Proses bisnis

**Tujuan.** Petugas Logistik dapat melihat hasil pengadaan yang sudah diterima: berapa yang dipesan, berapa yang diterima secara fisik, berapa yang diterima sah, kondisi barang, batch/lot, dan tanggal kedaluwarsa, tanpa memerlukan izin Finance.

**Pelaku.** Petugas Logistik/Gudang (membaca). Finance tetap pemilik dokumen PO dan GR. Sistem BLM tetap pemilik data kondisi untuk dokumen bersumber BLM.

**Pemicu.** Petugas membuka halaman Pemeriksaan Hasil Pengadaan, mengganti saringan, atau membuka detail satu dokumen.

**Langkah (urut).**

1. Backend memeriksa izin `LogisticsProcurementInspection : Read` (daftar, ringkasan, pilihan supplier) atau `ReadDetail` (detail).
2. Query membaca baris Goods Receipt (`FinGoodsReceiptItem`) yang belum dihapus, beserta GR induk, PO, item PO, dan supplier, dalam satu proyeksi (tanpa N+1).
3. Baris dicocokkan ke item PO **hanya lewat `PurchaseOrderItemId`**, tidak pernah lewat nama produk.
4. Untuk baris yang membawa kode kondisi (bersumber BLM), kuantitas diturunkan oleh `PurchasingReceiptQuantityCalculator` milik Finance (rumus yang sama persis, bukan formula baru).
5. Hasil dikirim apa adanya; besaran yang tidak dapat dipastikan dikirim `null`.

**Aturan bisnis yang dijaga.**

| Aturan | Contoh |
| --- | --- |
| Status dokumen ≠ hasil pemeriksaan | GR `RECEIVED` tidak berarti barang lulus. Satu GR berstatus `RECEIVED` dapat berisi baris `GOOD`, `DAMAGED`, dan `PENDING_INSPECTION` |
| Kondisi kosong **bukan** GOOD | Baris GR lokal/lama tanpa `ConditionCode` dilaporkan `NOT_RECORDED` ("Belum ada data pemeriksaan") |
| Rusak dan menunggu periksa bukan penerimaan sah (`FIN-DEC-268`) | Baris `DAMAGED` 10 TAB dan `PENDING_INSPECTION` 20 TAB → jumlah sah 0 TAB, rusak 10, menunggu periksa 20 |
| Satu baris, satu satuan | Baris BLM memakai satuan dasar (`BaseUomCode`). Jumlah dipesan, fisik, sah, menunggu, dan rusak dalam satuan yang sama |
| Konversi tidak pasti tidak ditebak | Bila isi per satuan kosong atau satuan dasar baris berbeda dari item PO, status baris `UNRESOLVED`; jumlah sah/menunggu/rusak `null`, hanya fakta sumber yang ditampilkan |
| GR dibatalkan bukan penerimaan | Baris dari GR `CANCELLED` berstatus `CANCELLED`; semua kuantitas `null` |
| Tidak ada nilai finansial | Harga satuan, total baris, dan total PO tidak dibaca maupun dikirim |

**Jalur tidak normal.**

| Kejadian | Yang terjadi |
| --- | --- |
| Saringan tidak sah (kondisi/sumber/status di luar daftar, tanggal awal > tanggal akhir) | `400` dengan pesan yang bisa ditampilkan apa adanya |
| Detail untuk GR yang tidak ada atau sudah dihapus | `404` |
| Pengguna tanpa izin | `403` dari filter izin |
| Backend menerima ID GR yang bukan GUID | Tertolak oleh constraint rute `{goodsReceiptId:guid}` |

**Hasil akhir.** Tidak ada perubahan data. Dokumen PO/GR, status, stok, dan jurnal tidak disentuh.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`FinanceGoodsReceiptsController.cs`, `FinanceGoodsReceiptService.cs`, `PurchasingReceiptQuantityCalculator.cs`, `FinGoodsReceipt.cs`, `FinGoodsReceiptItem.cs`, `FinPurchaseOrder.cs`, `FinPurchaseOrderItem.cs`, `MstSupplier.cs`, `PurchasingGoodsReceiptLogisticsLink.cs`, `ApplicationDbContext.cs` (DbSet), `BillingManagementServiceCollectionExtensions.cs`, `AccessPermissionService.cs`, `AccessControllerAttribute.cs`, `PagedResult.cs`, `ApiResponse.cs`, `docs/module-blueprints/logistik/00-interview-decisions.md`, `blueprint-manifest.md`, laporan `LGS-BE-ADH-04.md`, registry dan kontrak QBE.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/LogisticsManagement/Receiving/Controllers/LogisticsProcurementInspectionsController.cs` | **Baru.** Empat `GET`; izin `LogisticsProcurementInspection` |
| `Areas/Corporate/LogisticsManagement/Receiving/Services/LogisticsProcurementInspectionQueryService.cs` | **Baru.** Service baca: daftar, ringkasan, pilihan supplier, detail. Hanya `AsNoTracking` |
| `Areas/Corporate/LogisticsManagement/Receiving/Dtos/LogisticsProcurementInspectionDtos.cs` | **Baru.** DTO query dan respons khusus Logistik |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Satu baris `AddScoped<LogisticsProcurementInspectionQueryService>()` di samping registrasi layanan Goods Receipt Finance |

**Tidak diubah (dan tidak perlu diubah):** `FinanceGoodsReceiptsController`, `FinanceGoodsReceiptService`, seluruh DTO Finance, `PurchasingReceiptQuantityCalculator`. Perubahan Finance yang tampak di `git status` berasal dari task terpisah `FIN-BE-LGS-LINK-001`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru (aditif). Kontrak Finance tidak berubah |
| Database | **NONE.** Tanpa entity, tabel, kolom, migration, DDL, maupun DML. Hanya `SELECT` melalui EF |
| Keamanan/Auth | Resource izin baru `LogisticsProcurementInspection` (`Read`, `ReadDetail`) muncul di layar Akses Role setelah aplikasi dijalankan. Petugas Logistik tidak memerlukan `FinanceGoodsReceipt : Read`. Izin Finance tidak dilonggarkan |

**Kecocokan atribut izin** (diperiksa huruf demi huruf, sesuai `role-access-rules.md`):

| Action | `[AccessAction]` | `[AccessPermission]` | `ControllerName` |
| --- | --- | --- | --- |
| `GetList`, `GetSummary`, `GetSupplierOptions` | `Read` | `("LogisticsProcurementInspection", "Read")` | `LogisticsProcurementInspection` |
| `GetDetail` | `ReadDetail` (`AccessTypes.Read`, `SortOrder = 2`) | `("LogisticsProcurementInspection", "ReadDetail")` | `LogisticsProcurementInspection` |

Preseden aksi kedua bertipe `Read`: `AssetsController` (`ViewAmount`, `AccessTypes.Read`, `SortOrder = 2`). Tidak ada `IsInRole`, daftar peran, atau nama departemen. Tidak ada `[AllowAnonymous]`.

---

## 4. Dokumentasi endpoint

#### Corporate / Logistics Management / Receiving / Procurement Inspection

Base URL: `api/v1/corporate/logistics-management/receiving/procurement-inspections`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar baris pemeriksaan (satu baris = satu baris Goods Receipt), berpaging | `LogisticsProcurementInspection : Read` | Query: `receivedDateFrom`, `receivedDateTo`, `supplierId`, `conditionCode`, `sourceSystem`, `documentStatus`, `search`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<ProcurementInspectionItemResponse>>` |
| `GET` | `/summary` | Hitungan dokumen dan item menurut saringan yang sama (tanpa paging) | `LogisticsProcurementInspection : Read` | Query yang sama dengan daftar | `ApiResponse<ProcurementInspectionSummaryResponse>` |
| `GET` | `/supplier-options` | Pilihan supplier untuk saringan (hanya supplier yang punya GR) | `LogisticsProcurementInspection : Read` | Query: `search`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<ProcurementInspectionSupplierOptionResponse>>` |
| `GET` | `/{goodsReceiptId}` | Detail satu GR beserta seluruh barisnya, identitas PO/supplier, dan status penghubung persediaan | `LogisticsProcurementInspection : ReadDetail` | Path: `goodsReceiptId` (GUID) | `ApiResponse<ProcurementInspectionDetailResponse>` |

**Filter.**

| Parameter | Nilai | Arti |
| --- | --- | --- |
| `conditionCode` | `GOOD`, `DAMAGED`, `PENDING_INSPECTION`, `NOT_RECORDED` | `NOT_RECORDED` = baris tanpa kode kondisi |
| `sourceSystem` | `QUILVIAN`, `BLM` | Sumber dokumen GR |
| `documentStatus` | `RECEIVED`, `CANCELLED` | Status DOKUMEN GR |
| `search` | teks, maks. 200 karakter | Mengandung nama barang, nomor GR, atau nomor PO; tanpa membedakan huruf besar/kecil; karakter `%`, `_`, dan `!` dilarikan |
| `pageSize` | 1–100 (bawaan 25) | `totalPage` dihitung backend |

Urutan: tanggal penerimaan terbaru, lalu waktu dibuat GR terbaru, nomor GR, nama barang, ID (stabil untuk paging).

**Kode status.**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Data tersedia |
| `400` | Saringan tidak sah (nilai tidak dikenal atau tanggal awal lebih besar dari tanggal akhir) |
| `401` | Sesi habis |
| `403` | Tidak punya hak akses `LogisticsProcurementInspection : Read` / `ReadDetail` |
| `404` | Dokumen penerimaan tidak ditemukan atau sudah dihapus |

**Bentuk satu baris** (data samaran). Satu baris GR membawa **satu** kode kondisi, sehingga hanya satu dari tiga besaran (sah, menunggu periksa, rusak) yang berisi angka untuk barisnya:

```json
{
  "goodsReceiptNumber": "GR-2026-0001", "receivedDate": "2026-11-27",
  "documentStatus": "RECEIVED", "sourceSystem": "BLM",
  "purchaseOrderNumber": "PO-2026-0007", "supplierName": "PT Contoh Farma",
  "productName": "Parasetamol 500 mg", "quantityUnit": "TAB",
  "quantityBasis": "BASE_UNIT", "quantityState": "RESOLVED",
  "orderedQuantity": 1000, "physicalQuantity": 200,
  "acceptedQuantity": 0, "pendingInspectionQuantity": 200, "damagedQuantity": 0,
  "sourceQuantity": 2, "sourceUomCode": "BOX",
  "conditionCode": "PENDING_INSPECTION", "conditionStatus": "PENDING_INSPECTION",
  "batchNumber": "B-001", "expiredDate": "2028-05-31"
}
```

Contoh di atas: dipesan 1.000 TAB, tiba 2 BOX (= 200 TAB) yang masih menunggu periksa. Jumlah diterima sah 0 TAB karena barang menunggu periksa belum penerimaan sah (`FIN-DEC-268`). Untuk GR lokal/lama tanpa kode kondisi, `quantityState` bernilai `NOT_RECORDED`, `conditionStatus` bernilai `NOT_RECORDED`, dan `acceptedQuantity`/`pendingInspectionQuantity`/`damagedQuantity` bernilai `null` (bukan 0).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan (dilarang pada sesi ini) | `NOT RUN` | Kompilasi **belum terbukti**. Simbol dan tanda tangan metode diperiksa dengan membaca source (`ReceiptOrderItemInput`, `ReceiptLineInput`, `ReceiptItemProgress`, `PurchasingGoodsReceiptLogisticsLink.GetLinksAsync`, DbSet) |
| QBE conformance, `-Path` pada 4 berkas | `VIOLATION 0`, `REVIEW 0` | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| Kecocokan `[AccessController]` / `[AccessAction]` / `[AccessPermission]` | Cocok huruf demi huruf; tidak ada `IsInRole` | `PASS` (statis) | Bagian 3.3; tes FE `logistics-procurement-inspection.test.mjs` membaca controller |
| Rute dan nilai enum FE ↔ BE | Cocok | `PASS` (statis) | Tes membandingkan `[Route]` dan nilai kode pada source BE |
| Service murni baca | Tidak ada `SaveChanges`, transaksi, `DrugStockService`, `.Add(`, `.Remove(`, `ExecuteUpdate/Delete`, `UnitPrice`, `LineTotal`, `TotalAmount` | `PASS` (statis) | Tes pada berkas yang sama |
| Penelusuran aturan: kondisi kosong → `NOT_RECORDED`; GR dibatalkan → `CANCELLED`; konversi tidak pasti → `UNRESOLVED` | Sesuai | `PASS` (penelusuran source) | `Populate(...)` pada service |
| Jalur sukses/gagal lewat HTTP (`200`, `400`, `403`, `404`) | Tidak dijalankan | `NOT RUN` | Tidak ada lingkungan runtime pada sesi ini |
| Terjemahan LINQ ke SQL (`ILike` dengan karakter pelarian, `GroupBy` pada kolom nullable, `Distinct().Count()`) | Belum dibuktikan | `NOT RUN` | Menunggu eksekusi nyata |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — build dan server dilarang pada sesi ini.

**Tidak dijalankan:** `dotnet build`, `dotnet restore`, migration, eksekusi database, Swagger/HTTP.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (dari instruksi pemilik) | Status | Bukti |
| --- | --- | --- |
| API baca khusus Logistik tersedia | Terpenuhi di source | Controller + service + DTO baru |
| Data dari PO dan GR existing; tanpa tabel/salinan baru | Terpenuhi | Bagian 3.3; tanpa migration |
| Izin Logistik terpisah dari Finance | Terpenuhi di source | Bagian 3.3 |
| Pembacaan tidak mengubah saldo/dokumen Finance | Terpenuhi di source | Service murni baca |
| Kuantitas memakai aturan Finance yang ada | Terpenuhi di source | `PurchasingReceiptQuantityCalculator.Calculate` dipanggil per baris |
| Kondisi kosong tidak dianggap GOOD | Terpenuhi di source | `ConditionStatus = NOT_RECORDED` |
| Tidak ada harga/nilai finansial | Terpenuhi di source | Tes statis |
| Kompilasi berhasil | **Belum terbukti** | `dotnet build` NOT RUN |
| Aksi pencatatan hasil pemeriksaan | **BLOCKED / NEEDS CONFIRMATION** | Bagian 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi belum terbukti. Jalankan `dotnet build` sebelum dipakai |
| Masalah yang diketahui | Pemeriksaan LINQ→SQL belum dijalankan; `GroupBy` pada `ConditionCode` nullable dan `ILike` berkarakter pelarian `!` perlu dilihat pada eksekusi pertama |
| **BLOCKED / NEEDS CONFIRMATION — pencatatan hasil pemeriksaan** | Tidak dibangun. Butuh: tabel pemeriksaan baru dan `PhmDrugStockMutation`/penerimaan Logistik (skema, `BLOCKED_BY_SCHEMA`), entri registry `LogisticsManagement` + prefix (QBE-MOD-002/003, `LGS-OQ-016`), pemilik proses dan penyetuju (`LGS-OQ-015`), jawaban apakah Quilvian berwenang mencatat atau hanya membaca dari BLM (kepemilikan hasil pemeriksaan), aturan koreksi/pembatalan pemeriksaan, dan lifecycle (usulan: Menunggu Pemeriksaan → Dalam Pemeriksaan → Selesai Diperiksa; keputusan per item: Diterima, Diterima Sebagian, Ditahan/Karantina, Ditolak — **hanya usulan**, belum enum resmi) |
| **BLOCKED — integrasi stok** | Tidak ada penambahan saldo gudang dari halaman ini. Menunggu `LGS-OQ-017` (jalur GR → saldo gudang) dan `LGS-OQ-018` (apakah Warehousing BLM dipakai). Tidak ada jurnal Accounting |
| Temuan pada kode lama (dicatat, tidak diperbaiki) | `FinanceGoodsReceiptsController`: `ControllerName = "GoodsReceipt"` sedangkan `[AccessPermission]` memakai `"FinanceGoodsReceipt"`. Padanan resource Finance perlu diverifikasi di layar Akses Role oleh pemilik Finance |
| Risiko tersisa | Resource `LogisticsProcurementInspection` hanya muncul di Akses Role setelah backend dijalankan; admin harus mencentang `Read` dan `ReadDetail`. Tanpa itu, petugas Logistik menerima `403` |
| Perubahan sampingan | `NONE` oleh task ini |
| Interupsi | `NONE` |
| Status Git | Berkas baru `Areas/Corporate/LogisticsManagement/**` dan satu baris di `BillingManagementServiceCollectionExtensions.cs`; tidak ada stage/commit/push |
| Langkah berikutnya | (1) `dotnet build`; (2) admin mencentang izin baru di Akses Role; (3) pemilik registry memutuskan entri `LogisticsManagement`; (4) pemilik Logistik memutuskan `LGS-OQ-015/017/018` sebelum aksi tulis apa pun dirancang |
