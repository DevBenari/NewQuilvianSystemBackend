# Logistik & Manajemen Persediaan — Peta Kemampuan yang Sudah Ada (Existing Capability Map)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `LGS-BP-001` |
| Revision blueprint | `1` |
| Revision dokumen | `r2` — 2026-11-17, impact scan sesudah sinkron integration (bagian 8). `r1` diaudit pada `6011bb32` / `72ff3a95` |
| Status | `DRAFT` — bukti audit; bukan arsitektur target dan tidak memberi wewenang implementasi |
| SHA backend | `d946b20b` (branch `Ikbal`, setara `origin/QuilvianIntegrationBackend` per 2026-11-17) |
| SHA frontend | `1e9adc790` (branch `Ikbalv2`, memuat seluruh `origin/QuilvianIntegrationFrontend` per 2026-11-17) ditambah perubahan menu Logistik dan route `drug-stock/summary`, `drug-stock/expiring` yang belum di-commit |
| Masukan | `00-interview-decisions.md` r1; seluruh `LGS-OQ-001`..`016` masih `open` |
| Kontrak yang berlaku | Belum ada kontrak target Logistik. Kontrak as-is di bagian 3 adalah perilaku kode saat ini |
| Penulisan | `MODULE BLUEPRINT MODE`; source aplikasi dibaca saja |

Format bukti: `phm/<path>#<simbol>@6011bb32` untuk backend dan `fe/<path>#<simbol>@72ff3a95` untuk
frontend. `phm` = repository `NewQuilvianSystemBackend` (worktree `C:\tmp\phm`), `fe` =
`QuilvianSystemFrontendDev`.

## 1. Batas audit

- **Diperiksa:** model, `ApplicationDbContext`, controller, service, enum, dan DTO untuk stok obat
  (`Areas/HealthServices/PharmacyManagement`), master obat dan lokasi
  (`Areas/HealthServices/MasterData`), Purchasing Finance
  (`Areas/Corporate/FinanceManagement/Purchasing`), pencarian istilah persediaan pada
  `Areas/Corporate/AccountingManagement`, serta service dan menu frontend yang memanggilnya.
- **Tidak diperiksa:** isi database dan data seeder di luar keberadaannya, test otomatis, perilaku
  runtime lewat API langsung, dan modul Gizi selain pencarian tabel stok bahan (tidak ditemukan).
- **Klaster yang relevan:** Identity/Master Owner (katalog, lokasi), Location/Resource (gudang,
  rak), Workflow/Status (permintaan, transfer, retur), Documentation/Record (kartu stok, berita
  acara), Financial (nilai persediaan, jurnal), Authorization/Audit (izin per aksi, mutasi).

## 2. Ringkasan

| Status | Jumlah | Contoh |
| --- | --- | --- |
| Ready to reuse | 12 | Saldo per lokasi, transfer antar gudang, retur obat, kartu stok, kebijakan min/max |
| Reuse with adapter | 8 | Master lokasi untuk barang non-medis, batch/lot, penempatan rak, penelusuran lokasi, kondisi barang dari BLM |
| Extend | 4 | Validasi atasan, serah terima ke unit, maker-checker penyesuaian, permintaan non-medis |
| Missing | 8 | Katalog non-medis, alur stock opname, nilai persediaan, jurnal persediaan, rekomendasi pengisian |
| Conflict | 1 | Goods Receipt Finance tidak tersambung ke stok gudang |
| Unknown | 2 | Pemenuhan sebagian, klasifikasi alkes/BMHP di master obat |

**Fakta yang paling memengaruhi usulan alur:**

1. **Barang yang diterima lewat Goods Receipt Finance tidak masuk ke saldo gudang.** Purchase Order
   dan Goods Receipt bersumber dari sistem luar **BLM** (`FIN-DEC-261`). Item PO memakai produk
   BLM (`ExternalProductId`), bukan master obat Quilvian, dan belum ada pemetaan keduanya, sehingga
   penerimaan tidak tahu obat mana yang bertambah. Stok obat saat ini hanya bertambah lewat saldo
   awal, transfer, dan retur. Apakah BLM juga mengurus gudang belum diketahui (`LGS-OQ-018`).
2. **Permintaan obat tidak punya langkah persetujuan.** Status `Approved` dan `Rejected` ada di
   enum, tetapi tidak ada endpoint yang memakainya.
3. **Penyerahan sebagian langsung menutup permintaan**, dan tujuan barang belum dicatat, sehingga
   stok per unit dan serah terima di unit belum ada.
4. **Batch obat tidak menyimpan harga**, jadi nilai persediaan belum bisa dihitung.
5. **Belum ada barang non-medis** sama sekali: katalog, saldo, maupun permintaannya.

> **Contoh dampak.** Gudang menerima 100 box spuit dari penyedia dan Finance mencatat Goods
> Receipt. Saldo spuit di Gudang Farmasi Utama tetap tidak bertambah. Petugas harus mencatat
> saldo awal atau penyesuaian secara terpisah, dan jejak bahwa barang itu berasal dari PO tertentu
> hilang dari kartu stok.

## 3. Kontrak as-is yang relevan

### 3.1 Stok obat — `api/v1/health-services/pharmacy-management/drug-stock`

| Method | Path | Izin | Fungsi |
| --- | --- | --- | --- |
| `GET` | `/summary` | `DrugStock:Read` | Ringkasan saldo per obat |
| `GET` | `/balances` | `DrugStock:Read` | Saldo per batch per lokasi, termasuk status |
| `GET` | `/batches` | `DrugStock:Read` | Daftar batch dan tanggal kedaluwarsa |
| `GET` | `/expiring` | `DrugStock:Read` | Saldo yang mendekati kedaluwarsa |
| `GET` | `/mutations` | `DrugStock:Read` | Kartu stok (riwayat mutasi) |
| `POST` | `/opening-balance` | — | Mencatat saldo awal |
| `POST` | `/adjust` | `DrugStock:Adjust` | Penyesuaian saldo dengan alasan, `ExpectedVersion`, dan `IdempotencyKey` |
| `POST` | `/change-status` | — | Mengubah status saldo: Tersedia, Karantina, Kedaluwarsa, Rusak |
| `POST` | `/mutations/correct` | — | Koreksi mutasi |

Kolom izin yang bertanda "—" belum diperiksa per baris pada audit ini.

**Contoh request penyesuaian:**

```json
POST /api/v1/health-services/pharmacy-management/drug-stock/adjust
{
  "drugBatchId": "8f1c…",
  "storageLocationId": "2b7e…",
  "status": "Available",
  "quantityChange": -3,
  "reason": "Selisih hitung fisik opname November",
  "expectedVersion": 12,
  "idempotencyKey": "opname-2026-11-gfu-001"
}
```

### 3.2 Permintaan obat — `api/v1/health-services/pharmacy-management/stock-requests`

| Method | Path | Izin | Fungsi |
| --- | --- | --- | --- |
| `GET` | `/`, `/{id}` | `StockRequest:Read` | Daftar dan detail |
| `POST` | `/` | `StockRequest:Create` | Membuat draft |
| `PUT` | `/{id}` | `StockRequest:Update` | Mengubah draft |
| `POST` | `/{id}/submit` | `StockRequest:Update` | Mengirim ke gudang |
| `POST` | `/{id}/fulfill` | `StockRequest:Fulfill` | Gudang mencatat jumlah yang diserahkan; FEFO; mengurangi saldo gudang asal |
| `POST` | `/{id}/cancel` | `StockRequest:Cancel` | Membatalkan |

Status: `Draft`, `Submitted`, `Approved`, `Completed`, `Rejected`, `Cancelled`. Prioritas:
`Routine`, `Urgent`, `Emergency`.

### 3.3 Status lain

| Proses | Status as-is |
| --- | --- |
| Transfer stok | `Draft`, `Requested`, `Approved`, `InTransit`, `Completed`, `Rejected`, `Cancelled` |
| Retur obat | `Draft`, `Submitted`, `Verified`, `Rejected`, `Cancelled` |
| Pemakaian obat | `Draft`, `NotBilled`, `Billed`, `Cancelled` |
| Status saldo | `Available`, `Quarantine`, `Expired`, `Damaged` |
| Jenis mutasi | `OpeningBalance`, `StockIn`, `StockOut`, `Adjustment`, `StatusChange` |
| Sumber mutasi | `StockRequest`, `GoodsReceipt`, `StockTransfer`, `DrugUsage`, `PrescriptionReturn`, `StockOpname`, `ManualAdjustment`, `OpticalOrder` (baru di `d946b20b`, milik Optik `OPT-DEC-054`) |

Sumber `GoodsReceipt` dan `StockOpname` sudah didefinisikan, tetapi tidak ada kode yang memakainya
(`LGS-CAP-07`, `LGS-CAP-24`).

## 4. Peta kemampuan

### 4.1 Master barang dan gudang

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-01` | Katalog barang medis (obat) | Farmasi / Master Data | `phm/Repositories/ApplicationDbContext.cs#MstDrugs@6011bb32`; `fe/src/utils/menu-sidebar/menu-items.jsx#healthServicesLogisticsMasterItemCatalog@72ff3a95` | Ready to reuse | — | Menu memberi label "Katalog Barang" untuk halaman yang isinya obat |
| `LGS-CAP-02` | Klasifikasi alkes dan BMHP di katalog | Farmasi / Master Data | `phm/Areas/HealthServices/MasterData/Models/MstDrug.cs#MstDrug@6011bb32` — tidak ditemukan kolom jenis barang | Unknown | Perlu dicek apakah kategori obat sudah membedakan alkes/BMHP | Laporan per jenis barang tidak bisa dipisah |
| `LGS-CAP-03` | Katalog barang non-medis | — | Tidak ada entity katalog/stok non-medis di `phm/Repositories/ApplicationDbContext.cs@6011bb32` | Missing | Bergantung `LGS-OQ-001` dan `LGS-OQ-016` | Seluruh alur non-medis tertahan |
| `LGS-CAP-04` | Kategori, satuan, dan konversi satuan (obat) | Master Data | `phm/Repositories/ApplicationDbContext.cs#MstDrugCategories,MstMeasurements,MstDrugUnitConversions@6011bb32` | Ready to reuse | — | — |
| `LGS-CAP-05` | Gudang dan lokasi bertingkat, rak/shelf/bin, syarat suhu dan kelembapan | Master Data | `phm/Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs#ParentStorageLocationId,RackCode,MinimumTemperatureCelsius@6011bb32` (baris 12, 43–57) | Reuse with adapter | Entity bernama "DrugStorageLocation" dan `IsPharmacyLocation` default `true`; perlu penanda peruntukan non-medis (`LGS-OQ-002`) | Nama dan default bisa menyesatkan bila dipakai barang non-medis |
| `LGS-CAP-06` | Penanggung jawab gudang | — | Tidak ada kolom penanggung jawab di `MstDrugStorageLocation.cs@6011bb32` | Missing | Bergantung `LGS-OQ-013` | Persetujuan per gudang tidak bisa diarahkan |

### 4.2 Penerimaan barang

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-07` | Penerimaan dari pengadaan menambah saldo gudang | Finance + Farmasi | `phm/Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrderItem.cs#ProductName@6011bb32` (teks bebas, baris 20–22); `phm/Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs#FinanceGoodsReceiptService@6011bb32` (tidak memanggil stok); `phm/Areas/HealthServices/PharmacyManagement/Enums/DrugStockEnums.cs#GoodsReceipt@6011bb32` (baris 78, tidak dipakai) | Conflict | PO dan GR bersumber BLM (`FIN-DEC-261`, `FIN-DEC-263`, `FIN-DEC-265`); item PO memakai produk BLM tanpa pemetaan ke master obat. Di `d946b20b` `FinanceGoodsReceiptService` tetap tidak menyentuh stok Farmasi. Perlu keputusan bersama Finance dan Farmasi (`LGS-OQ-017`, `LGS-OQ-018`) | Saldo gudang tidak mencerminkan barang yang sudah diterima; jejak PO hilang dari kartu stok |
| `LGS-CAP-08` | Pemeriksaan kondisi dan pencatatan barang tidak sesuai saat penerimaan | Finance (data dari BLM) | `phm/Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceiptItem.cs#ConditionCode,AcceptedBaseQuantity@d946b20b`; `FIN-DEC-268` (barang rusak dan menunggu pemeriksaan tidak dihitung sebagai penerimaan sah) | Reuse with adapter | Kondisi dicatat di Goods Receipt Finance untuk dokumen BLM, belum diteruskan ke status saldo gudang (karantina/rusak). Di `r1` berstatus Missing | Barang rusak dari penyedia tercatat di Finance tetapi tidak terlihat di gudang |
| `LGS-CAP-09` | Batch/lot dan tanggal kedaluwarsa | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Models/PhmDrugBatch.cs#ExpiryDate@6011bb32`; `GET /drug-stock/batches`; `fe/src/lib/services/health-services/pharmacy-management/drug-stock.service.js#getDrugBatches@72ff3a95` dipakai di 0 file | Reuse with adapter | Backend siap; belum ada halaman frontend yang memakai `getDrugBatches` | — |

### 4.3 Penyimpanan dan penempatan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-10` | Saldo per batch per lokasi beserta status | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Models/PhmDrugStock.cs#PhmDrugStockBalance@6011bb32`; `GET /balances`, `POST /change-status` | Ready to reuse | — | — |
| `LGS-CAP-11` | Penempatan ke rak dan perpindahan lokasi dalam gudang | Farmasi | Lokasi bertingkat `MstDrugStorageLocation.cs#ParentStorageLocationId@6011bb32`; transfer `phm/Areas/HealthServices/PharmacyManagement/Services/StockTransferService.cs@6011bb32` | Reuse with adapter | Rak dimodelkan sebagai lokasi anak, lalu perpindahan memakai transfer. Transfer saat ini berjalur persetujuan dan Dalam Perjalanan, terlalu berat untuk pindah rak | Petugas enggan mencatat pindah rak bila prosesnya panjang |
| `LGS-CAP-12` | Menolak penempatan yang melanggar syarat penyimpanan (rantai dingin) | — | `IsColdChain` tidak dipakai di service mana pun (pencarian `phm/Areas/HealthServices/PharmacyManagement`, `OperatingRoomManagement` @6011bb32) | Missing | Data syarat sudah ada (`LGS-CAP-05`); penegakannya belum | Obat rantai dingin bisa tercatat di lokasi bersuhu ruang |
| `LGS-CAP-13` | Penelusuran lokasi barang | Farmasi | `GET /drug-stock/balances` | Reuse with adapter | Perlu tampilan "cari barang → lokasi" | — |

### 4.4 Permintaan barang unit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-14` | Pengajuan permintaan obat dengan prioritas dan riwayat | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs#Submit@6011bb32` (baris 122); `phm/Repositories/ApplicationDbContext.cs#PhmStockRequestHistories@6011bb32`; `fe/src/lib/services/health-services/pharmacy-management/stock-request.service.js@72ff3a95` | Ready to reuse | — | — |
| `LGS-CAP-15` | Permintaan barang non-medis | — | Tidak ada | Extend | Bisa memakai pola `PhmStockRequest` bila `LGS-OQ-001` memilih satu pola | — |
| `LGS-CAP-16` | Validasi atasan | Farmasi | Enum `StockRequestStatus.Approved/Rejected` ada; tidak ada endpoint approve/reject di `StockRequestController.cs@6011bb32` | Extend | Tambah langkah persetujuan; aturan menunggu `LGS-OQ-004` | Status `Approved` di enum tidak pernah terisi; laporan yang menyaringnya akan kosong |

### 4.5 Distribusi dan pengeluaran

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-17` | Penyiapan dan pengeluaran dengan FEFO | Farmasi | `StockRequestController.cs#Fulfill@6011bb32` (baris 140–166: "Batch yang paling dekat kedaluwarsa diambil lebih dahulu") | Ready to reuse | — | — |
| `LGS-CAP-18` | Pengiriman dan serah terima di unit | Farmasi | `StockRequestController.cs@6011bb32` baris 157–158: "Ke mana barang mendarat belum dicatat sistem"; status transfer `InTransit`/`Completed` | Extend | Bisa memakai pola transfer bila unit punya lokasi stok sendiri (`LGS-OQ-003`, `LGS-OQ-006`) | Tanpa serah terima, tanggung jawab barang berhenti di gudang |
| `LGS-CAP-19` | Pemenuhan sebagian | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs@6011bb32` baris 399: "Penyerahan sebagian tetap menutup permintaan" | Unknown | Perilaku as-is: tidak ada backorder. Statusnya bergantung keputusan `LGS-OQ-005` | Bila diputuskan ada backorder, perilaku ini harus diubah |

### 4.6 Mutasi dan retur

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-20` | Transfer antar gudang dengan status Dalam Perjalanan | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Services/StockTransferService.cs@6011bb32` (baris 529 memanggil `ReceiveBatchAsync`); `PHA-DEC-031` | Ready to reuse | — | — |
| `LGS-CAP-21` | Retur unit dengan pemeriksaan kondisi | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Controllers/DrugReturnController.cs@6011bb32` (baris 12–17, 98–102); `DrugReturnService.cs` baris 367 | Ready to reuse | Hanya obat | — |
| `LGS-CAP-22` | Retur ke penyedia mengurangi saldo gudang | Finance | `phm/Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs@6011bb32` tidak menyentuh stok | Reuse with adapter | Dokumen retur ada; pengurangan saldo gudang belum. Akar masalahnya sama dengan `LGS-CAP-07` | Saldo gudang lebih besar dari fisik setelah retur |
| `LGS-CAP-23` | Pemakaian obat oleh unit (dasar laporan pemakaian per unit) | Farmasi | `phm/Repositories/ApplicationDbContext.cs#PhmDrugUsages@6011bb32`; `fe/src/lib/services/health-services/pharmacy-management/drug-usage.service.js@72ff3a95` | Reuse with adapter | Datanya ada; laporan per unit belum | — |

### 4.7 Stock opname dan penyesuaian

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-24` | Alur stock opname: jadwal, cut-off, hitung fisik, verifikasi, persetujuan, berita acara | Farmasi | Hanya konstanta `DrugStockEnums.cs#StockOpname@6011bb32` (baris 82) dan kolom `MstDrugStockPolicy.IsStockOpnameRequired/StockOpnameIntervalDays` yang dipakai CRUD saja | Missing | Keputusan polanya sudah ada (`PHA-DEC-030`) | Selisih stok tidak terdeteksi secara terstruktur |
| `LGS-CAP-25` | Penyesuaian saldo dengan alasan, kendali versi, idempotency, dan jejak mutasi | Farmasi | `phm/Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs#Adjust@6011bb32` (baris 174–184, `DrugStock:Adjust`); `DrugStockDtos.cs#AdjustStockRequest` | Extend | Satu orang langsung memposting; `PHA-DEC-030` mensyaratkan maker-checker dan persetujuan sebelum penyesuaian | Penyesuaian besar bisa diposting tanpa pemeriksa kedua |

### 4.8 Pengendalian persediaan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-26` | Kebijakan min/max, reorder point, safety stock, peringatan kedaluwarsa per obat per lokasi | Master Data | `phm/Areas/HealthServices/MasterData/Models/MstDrugStockPolicy.cs@6011bb32` (baris 32–60) | Ready to reuse | — | — |
| `LGS-CAP-27` | Rekomendasi pengisian ulang dan kebutuhan pengadaan | — | `ReorderPointQuantity`, `IsAutoReorderEnabled` hanya dipakai di `DrugStockPolicyController.cs@6011bb32` untuk CRUD | Missing | Bergantung `LGS-OQ-011` | Kolom `IsAutoReorderEnabled` memberi kesan fitur sudah ada |
| `LGS-CAP-28` | Pemantauan kedaluwarsa | Farmasi | `GET /drug-stock/expiring`; `fe/src/app/health-services/pharmacy-management/drug-stock/expiring/page.jsx` (belum di-commit) | Ready to reuse | — | — |
| `LGS-CAP-29` | Pemantauan barang rusak dan karantina | Farmasi | Status `Damaged`/`Quarantine` pada `PhmDrugStockBalance` | Reuse with adapter | Perlu tampilan yang menyaring status itu | — |
| `LGS-CAP-30` | Stok lambat bergerak | — | Tidak ditemukan | Missing | Data mutasi (`LGS-CAP-31`) cukup untuk menghitungnya; definisi menunggu `LGS-OQ-014` | — |

### 4.9 Laporan dan rekonsiliasi

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-31` | Kartu stok dan riwayat mutasi | Farmasi | `GET /drug-stock/mutations`; `fe/src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-card-view.jsx@72ff3a95` | Ready to reuse | — | — |
| `LGS-CAP-32` | Nilai persediaan | — | `PhmDrugBatch` dan `PhmDrugStockBalance` tidak menyimpan harga; `FinPurchaseOrderItem.UnitPrice` tidak tertaut ke batch | Missing | Bergantung `LGS-OQ-008` dan penyelesaian `LGS-CAP-07` | Laporan nilai persediaan tidak bisa dibuat |
| `LGS-CAP-33` | Jurnal persediaan dan rekonsiliasi dengan buku besar | Akuntansi | Pencarian `inventory`/`persediaan`/`stock` di `phm/Areas/Corporate/AccountingManagement@6011bb32` tidak menemukan apa pun | Missing | Bergantung `LGS-OQ-012` | — |

### 4.10 Otorisasi dan audit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `LGS-CAP-34` | Izin per aksi dan jejak mutasi per pelaku | Platform Authorization + Farmasi | `[AccessPermission("StockRequest","Fulfill")]` di `StockRequestController.cs@6011bb32` baris 165; `PhmDrugStockMutation.ActorUserId/OccurredAt` | Ready to reuse | Pola izin dan audit dapat diikuti modul Logistik | — |
| `LGS-CAP-35` | Menu Logistik di sidebar | Frontend | `fe/src/utils/menu-sidebar/menu-items.jsx#healthServicesLogisticsManagement` (belum di-commit) | Ready to reuse | Struktur menu mungkin perlu disesuaikan setelah keputusan | — |

Total baris: 35. Dua baris menu dan otorisasi (`LGS-CAP-34`, `LGS-CAP-35`) dihitung dalam
"Ready to reuse" pada ringkasan bersama sepuluh baris lain: `01`, `04`, `10`, `14`, `17`, `20`,
`21`, `26`, `28`, `31`.

## 5. Conflict dan Unknown

| ID | Jenis | Uraian | Yang dibutuhkan |
| --- | --- | --- | --- |
| `LGS-CAP-07` | Conflict | Purchasing memakai item teks bebas (`FIN-DEC-045`), alur logistik butuh item tertaut katalog agar penerimaan menambah saldo | Keputusan bersama pemilik Finance dan Farmasi; dicatat sebagai pertanyaan baru `LGS-OQ-017` saat wawancara |
| `LGS-CAP-02` | Unknown | Belum jelas apakah master obat membedakan obat, alkes, dan BMHP | Pemeriksaan data kategori obat di database |
| `LGS-CAP-19` | Unknown | Penyerahan sebagian menutup permintaan; belum diputuskan apakah itu yang diinginkan | Jawaban `LGS-OQ-005` |

## 6. Pertanyaan penutup untuk wawancara

1. Bagaimana barang dari Goods Receipt masuk ke saldo gudang: item PO ditautkan ke katalog,
   atau gudang mencatat penerimaan sendiri yang merujuk Goods Receipt? (usulan pertanyaan baru
   `LGS-OQ-017`, menyangkut `LGS-CAP-07` dan `LGS-CAP-22`)
2. Apakah perilaku as-is "penyerahan sebagian menutup permintaan" dipertahankan? (`LGS-OQ-005`)
3. Apakah lokasi unit (ruangan) akan punya saldo sendiri sehingga serah terima memakai pola
   transfer? (`LGS-OQ-003`, `LGS-OQ-006`)
4. Dari mana harga persediaan diambil bila nilai persediaan dibutuhkan? (`LGS-OQ-008`)
5. Apakah master obat dipakai juga untuk alkes dan BMHP, atau ada kategori yang membedakannya?
   (`LGS-CAP-02`)
6. Apakah BLM, sumber PO dan Goods Receipt, juga mengelola gudang, distribusi, atau stock opname?
   Bila ya, modul Logistik di Quilvian harus menjadi penampung atau integrasi, bukan duplikasi.
   (`LGS-OQ-018`)

## 7. Pemicu impact scan

Peta ini menjadi `STALE` dan perlu impact scan terbatas bila salah satu hal berikut berubah:

- SHA backend berubah pada `Areas/HealthServices/PharmacyManagement/**`,
  `Areas/HealthServices/MasterData/Models/MstDrug*.cs`, atau
  `Areas/Corporate/FinanceManagement/Purchasing/**`.
- SHA frontend berubah pada `src/utils/menu-sidebar/menu-items.jsx`,
  `src/lib/services/health-services/pharmacy-management/**`, atau
  `src/app/health-services/pharmacy-management/drug-stock/**`.
- Keputusan baru pada `PHA-DEC-*`, `PHA-OQ-003`, `FIN-DEC-045`, atau keputusan integrasi BLM
  (`FIN-DEC-258`..`FIN-DEC-276` dan seterusnya).

## 8. Impact scan 2026-11-17

Pemicu: sinkron dengan integration. Backend `6011bb32` → `d946b20b` (132 commit), frontend
`72ff3a95` → `1e9adc790` (101 commit dari integration).

| Area bukti | Berubah? | Dampak pada peta |
| --- | --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/**` | Ya — integrasi BLM: ID eksternal, satuan dasar, kode kondisi, sinkron dan pembatalan eksternal | `LGS-CAP-07` tetap Conflict dengan bukti baru; `LGS-CAP-08` Missing → Reuse with adapter |
| `Areas/HealthServices/PharmacyManagement/Enums/DrugStockEnums.cs` | Ya — sumber mutasi baru `OpticalOrder` | Tidak mengubah status; Optik kini ikut memakai stok Farmasi |
| Bagian lain `Areas/HealthServices/PharmacyManagement/**`, `MstDrug*.cs` | Tidak | Tidak ada |
| `Areas/Corporate/AccountingManagement/**` (istilah persediaan) | Tidak ada baris baru | `LGS-CAP-33` tetap Missing |
| `src/lib/services/health-services/pharmacy-management/**`, `drug-stock/**`, `drug-stock-view/**` | Tidak | Tidak ada |
| `src/utils/menu-sidebar/menu-items.jsx` | Ya — menu Purchasing ditata ulang (`FIN-DEC-258`) dan ada menu baru "Katalog Barang Optik" | Ke-14 route tujuan tautan Logistik masih ada. "Katalog Barang Optik" adalah katalog barang lain yang perlu dipertimbangkan pada `LGS-OQ-001` |

Ringkasan sesudah scan: 12 Ready to reuse, 8 Reuse with adapter, 4 Extend, 8 Missing, 1 Conflict,
2 Unknown (total 35).

## 9. Implementasi ad-hoc 2026-11-17 (belum terverifikasi runtime)

Perubahan berikut dibuat sesudah `r2`. Status pada tabel di atas **sengaja belum dinaikkan**:
kontrak bukti melarang status siap tanpa verifikasi, sedangkan build dan dev server tidak boleh
dijalankan pada sesi ini. Status disegarkan setelah verifikasi runtime.

| Kemampuan | Perubahan | Laporan |
| --- | --- | --- |
| `LGS-CAP-09` Batch/lot | Konsumen FE dibuat: tab `Daftar Batch`, route `/drug-stock/batches` | `task/report/frontend/LGS-FE-ADH-01.md` |
| `LGS-CAP-26` + kebutuhan baru "peringatan stok di bawah reorder point" | Endpoint baru `GET /drug-stock/low-stock`; tab `Hampir Habis`, route `/drug-stock/low-stock` | `task/report/backend/LGS-BE-ADH-01.md`, `LGS-FE-ADH-01.md` |
| `LGS-CAP-29` Barang rusak | Route `/drug-stock/damaged` membuka saldo berstatus Rusak | `LGS-FE-ADH-01.md` |
| `LGS-CAP-13` Penelusuran lokasi | Respons saldo ditambah kode rak/shelf/bin; tab `Lokasi Barang`, route `/drug-stock/locations` | `LGS-BE-ADH-01.md`, `LGS-FE-ADH-01.md` |
| `LGS-CAP-19` Pemenuhan sebagian | Detail permintaan menampilkan kolom Kekurangan (diminta − diserahkan); perilaku as-is tidak diubah | `LGS-FE-ADH-01.md` |
| `LGS-CAP-26` Kebijakan min/max | Audit data 2026-11-17: 0 kebijakan aktif di database tim dan lokal, sehingga Barang Hampir Habis selalu kosong sampai master diisi | `evidence/03-audit-barang-hampir-habis.md` |
| `LGS-CAP-07` GR → saldo | Investigasi BLM: integrasi penerima kiriman BLM belum dibangun; 0 dokumen PO/GR di database tim | `evidence/LGS-BLM-INVESTIGATION.md` |
| `LGS-CAP-35` Menu Logistik | Menjadi bagian navigasi sendiri (`LGS-DEC-001`) | `LGS-FE-ADH-01.md` |
| `LGS-CAP-16` Validasi atasan | `LGS-DEC-002`: endpoint `approve`, `reject`, `authorize-emergency`; penyerahan hanya untuk status Disetujui; halaman Persetujuan dan Daftar Permintaan Disetujui. Disempurnakan: kewenangan atasan unit dibaca dari `MstServiceUnit.OrganizationUnitId` + penempatan manajerial `WfpOrganizationAssignment`; data keduanya masih 0 di basis data tim (`LGS-OQ-020`) | `task/report/backend/LGS-BE-ADH-02.md`, `LGS-BE-ADH-03.md`, `LGS-BE-ADH-04.md`, `task/report/frontend/LGS-FE-ADH-02.md`, `LGS-FE-ADH-03.md`, `LGS-FE-ADH-04.md`. r5: verifikasi susulan CITO (`LGS-DEC-005`) |
