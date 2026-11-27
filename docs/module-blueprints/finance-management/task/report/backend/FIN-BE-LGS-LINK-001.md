# Laporan Perubahan Backend — `FIN-BE-LGS-LINK-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-BE-LGS-LINK-001` |
| Judul | Penghubung Penerimaan Barang (GR Finance) → Logistik: status turunan baca-saja, ringkasan Monitoring Purchasing, dan penjaga koreksi/pembatalan GR yang sudah punya bukti mutasi stok |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (permintaan "Analisis GAP dan Perbaikan Integrasi AP → Logistik → Penerimaan Barang", 27 Nov 2026). Tidak ada baris roadmap; karena itu **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DEC-268` (rusak / menunggu periksa bukan penerimaan sah), `FIN-DEC-261` (BLM sumber PO/GR), `LGS-CAP-07` (GR Finance belum tersambung saldo gudang), `LGS-OQ-017`, `LGS-OQ-018`, `LGS-OQ-015` (terbuka), `LGS-DEC-001` (Logistik mandiri; ledger stok Farmasi tidak digandakan) |
| Contract version | Aditif saja: satu endpoint baca baru dan satu properti nullable baru pada tiga respons. Nomor versi kontrak tidak dinaikkan; `contracts/api-contract.md` belum diperbarui |
| Dependency | Tidak ada. FE `FIN-FE-LGS-LINK-001` membaca kontrak ini dan aman terhadap backend lama (status tampil "Tidak dapat ditentukan") |
| Task mode | `BACKEND`, tanpa migration, tanpa eksekusi database |
| Target tulis | `NewQuilvianSystemBackend` (source) + laporan ini. Repository Logistik/BLM tidak disentuh |
| Branch / SHA saat dikerjakan | `Yasmina` @ `1cd7b477dc48799acc68792aea534d348b902975` (working tree bersih sebelum task) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-27 |
| Status | 🟡 **SEBAGIAN** — source selesai; `dotnet build` **TIDAK DIJALANKAN** (dilarang pada task ini) sehingga kompilasi belum terbukti; posting stok sengaja tidak dibangun (lihat bagian 5) |

---

## 1. Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik / prefix registry | `Fin` (sudah terdaftar). Tidak ada entity baru, sehingga tidak ada keputusan prefix |
| Keberlakuan | `NEW CODE`: berkas DTO dan layanan baru. `TOUCHED LEGACY`: controller GR, `FinanceGoodsReceiptService`, `FinancePurchasingDocumentTrailService`, registrasi DI |
| QBE | Controller tidak memakai `ApplicationDbContext` (dibungkus `FinanceGoodsReceiptLogisticsLinkService`); tidak ada entity/tabel/kolom baru; tidak ada hardcode role; `AccessAction`/`AccessPermission` mengikuti action baca GR yang sudah ada |
| Migration / database | **Tidak ada migration dan tidak ada DDL/DML.** Tidak ada perubahan `ApplicationDbContext` maupun snapshot |

---

## 2. Masalah yang ditangani (hasil audit)

Rantai yang berjalan: **PO → GR (Finance) → Tukar Faktur → Purchasing Invoice → AP → Pembayaran**. Penerimaan fisik, batch, dan stok adalah milik **Logistik/Farmasi** (`DrugStockService`, `PhmDrugStockMutation`). Hasil audit pada SHA di atas:

- `DrugStockSourceDocumentTypes.GoodsReceipt` ada sebagai konstanta tetapi **tidak ada kode yang menulis mutasi bersumber GR**. GR Finance tercatat tidak berarti stok bertambah (`LGS-CAP-07`).
- Finance tidak mempunyai cara melihat apakah satu GR sudah dikonfirmasi/diposting oleh Logistik; status itu tidak ada di respons GR, jejak dokumen, maupun Monitoring.
- GR yang kelak punya mutasi stok masih bisa dikoreksi atau dibatalkan dari Finance tanpa penjaga, sehingga Finance dan saldo gudang berpotensi tidak konsisten.

### Matriks GAP (ringkas; versi lengkap ada di laporan akhir task)

| Gap | Kondisi | Klasifikasi | Ditangani di task ini |
| --- | --- | --- | --- |
| GAP-01 | GR Finance tidak menambah saldo gudang | `INTEGRATION_GAP` + `BLOCKED_BY_DECISION` (`LGS-OQ-017`) | Tidak (hanya dijaga & ditampilkan jujur) |
| GAP-02 | Pemetaan produk BLM → produk Quilvian tidak punya penyimpanan | `BLOCKED_BY_SCHEMA` | Tidak |
| GAP-03 | Konversi satuan hanya sampai satuan dasar BLM; produk tak terpetakan | `PARTIAL` (bergantung GAP-02) | Tidak |
| GAP-04 | Tidak ada dokumen/status konfirmasi penerimaan Logistik yang dapat dibaca Finance | `BLOCKED_BY_SCHEMA` + `BLOCKED_BY_DECISION` | Dilaporkan sebagai "tidak dapat ditentukan" |
| GAP-05 | Status penghubung GR → Logistik tidak terlihat | `INTEGRATION_GAP` | **Ya** (read model turunan) |
| GAP-06 | Koreksi/pembatalan GR tidak memperhatikan mutasi stok | `INTEGRATION_GAP` | **Ya** (penjaga) |
| GAP-07 | Monitoring tanpa indikator penghubung | `MISSING` | **Ya** (`logistics-summary`) |
| GAP-08 | Jejak dokumen tidak memuat status Logistik | `PARTIAL` | **Ya** (status; tanpa tautan ke dokumen Logistik karena dokumennya tidak ada) |
| GAP-09 | Three-way matching hanya sebagian | `PARTIAL` | Tidak diubah |
| GAP-10 | Alokasi invoice multi-GR "P04 NOT ACTIVATED" | `BLOCKED_BY_DECISION` | Tidak diubah |
| GAP-11 | Retur supplier tanpa hubungan stok | `MISSING` + `BLOCKED_BY_DECISION` (`LGS-OQ-015`) | Tidak |
| GAP-12 | Rusak / menunggu periksa bukan penerimaan sah | `EXISTING` (`FIN-DEC-268`) | Dipakai sebagai dasar status |
| GAP-13 | Idempotensi & koreksi posting stok belum ada | `BLOCKED_BY_SCHEMA` | Tidak |
| GAP-14 | Kegagalan sinkronisasi BLM tak dapat dilacak | `BLOCKED_BY_EXTERNAL_SYSTEM` | Dilaporkan sebagai "tidak dapat ditentukan" |

---

## 3. Proses bisnis (as-built di task ini)

Hanya **membaca** dan **menjaga**; tidak ada aliran data baru yang mengubah angka.

1. Daftar GR, detail GR, dan jejak dokumen kini memuat `logisticsLink` (turunan, nullable, aditif).
2. Status diturunkan dengan urutan: GR `CANCELLED` → `CANCELLED`; ada mutasi StockIn bersumber GR → `POSTED_TO_STOCK`; ada baris BLM `PENDING_INSPECTION` → `AWAITING_INSPECTION`; ada baris `DAMAGED` atau kuantitas sah < fisik → `HAS_DISCREPANCY`; selain itu → `NOT_LINKED`.
3. **Satu-satunya bukti hubungan** adalah `PhmDrugStockMutation` (`!IsDelete`, `SourceDocumentType = "GoodsReceipt"`, `QuantityChange > 0`) yang `SourceDocumentId`-nya id GR atau id baris GR. Konvensi mana yang dipilih belum diputuskan (`LGS-OQ-017`), jadi keduanya dibaca.
4. Selama belum ada penulis mutasi seperti itu, **semua GR akan `NOT_LINKED`** — itu benar dan disengaja; `reasons` menjelaskan bahwa GR Finance tidak otomatis menambah stok.
5. `GET goods-receipts/logistics-summary` menghitung GR `RECEIVED` per status. Kategori yang butuh data yang belum ada (`AWAITING_PRODUCT_MAPPING`, `AWAITING_LOGISTICS_CONFIRMATION`, `SYNC_FAILED`) dikembalikan di `undetermined` **tanpa angka**; klien wajib menampilkannya sebagai tidak dapat ditentukan, bukan 0.
6. `POST {id}/cancel` dan `PUT {id}` ditolak dengan `422` bila GR sudah punya mutasi stok bersumber GR. Penjaga ini tidak aktif selama tidak ada mutasi.

| Kejadian | Hasil |
| --- | --- |
| GR lokal (QUILVIAN), tanpa mutasi | `NOT_LINKED`; jumlah kondisi `null` (tidak dilacak) |
| GR BLM dengan baris `PENDING_INSPECTION` | `AWAITING_INSPECTION`; bukan penerimaan sah |
| GR BLM dengan baris rusak / diterima sebagian | `HAS_DISCREPANCY` |
| Ada mutasi StockIn yang menunjuk GR | `POSTED_TO_STOCK`; GR tidak dapat dibatalkan/diperbarui dari Finance |
| Mutasi bertanda negatif atau terhapus | Tidak dihitung sebagai bukti |
| Pembayaran / pengakuan AP | **Tidak berubah** dan tidak menyentuh stok |

---

## 4. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Purchasing/Dtos/FinancePurchasingLogisticsLinkDtos.cs` (baru) | Konstanta `PurchasingLogisticsLinkStatuses`, `PurchasingLogisticsCapabilityStatuses`; DTO `GoodsReceiptLogisticsLinkResponse`, `GoodsReceiptLogisticsSummaryQuery`, `GoodsReceiptLogisticsUndeterminedCategory`, `GoodsReceiptLogisticsSummaryResponse` |
| `Purchasing/Services/PurchasingGoodsReceiptLogisticsLink.cs` (baru) | `Derive` (murni), `GetLinksAsync`, `HasPostedStockAsync`, `GetSummaryAsync`; pembungkus DI `FinanceGoodsReceiptLogisticsLinkService`. Hanya `AsNoTracking()`; tanpa `SaveChanges`; tanpa `DrugStockService` |
| `Purchasing/Dtos/FinanceGoodsReceiptDtos.cs` | `GoodsReceiptResponse.LogisticsLink` (nullable) |
| `Purchasing/Dtos/FinancePurchasingDocumentTrailDtos.cs` | `PurchasingTrailGoodsReceipt.LogisticsLink` |
| `Purchasing/Services/FinancePurchasingDocumentTrailService.cs` | Mengisi `LogisticsLink` pada bagian GR (satu pembacaan bukti untuk seluruh GR) |
| `Purchasing/Services/FinanceGoodsReceiptService.cs` | Penjaga `HasPostedStockAsync` di `CancelAsync` dan `UpdateAsync` (konstruktor tidak berubah) |
| `Purchasing/Controllers/FinanceGoodsReceiptsController.cs` | Mengisi `LogisticsLink` pada daftar dan detail; endpoint baru `GET logistics-summary` (`Read`, izin `FinanceGoodsReceipt:Read` — tidak ada izin baru untuk diatur di Akses Role) |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `AddScoped<FinanceGoodsReceiptLogisticsLinkService>()` |

Pengamatan (tidak diperbaiki, di luar wewenang): `[AccessController]` GR memakai `ControllerName = "GoodsReceipt"` sedangkan seluruh `[AccessPermission]` memakai `"FinanceGoodsReceipt"`. Endpoint baru mengikuti pola yang sama dengan action baca yang sudah ada.

---

## 5. Yang sengaja TIDAK dibangun

| Butir | Alasan | Klasifikasi |
| --- | --- | --- |
| Posting stok dari GR Finance (`ReceiveBatchAsync`) | Item PO/GR berupa teks bebas atau produk BLM yang belum terpetakan; stok tidak boleh diposting dari tebakan produk, satuan, atau lokasi. Pemilik jalur belum diputuskan (`LGS-OQ-017`) | `BLOCKED_BY_DECISION` + `BLOCKED_BY_SCHEMA` |
| Tabel pemetaan produk BLM ↔ produk Quilvian | Butuh tabel baru → migration dilarang | `BLOCKED_BY_SCHEMA` |
| Tabel penerimaan Logistik / kunci idempotensi posting | Butuh tabel baru → migration dilarang | `BLOCKED_BY_SCHEMA` |
| Ubah three-way matching, alokasi invoice, atau pengakuan AP | Di luar lingkup; pengakuan AP tetap satu kali pada persetujuan Purchasing Invoice | — |
| Ubah GR BLM | GR BLM tidak boleh dibuat/diubah/dibatalkan lokal (`PurchasingExternalDocumentException`) | — |

---

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet restore` / `dotnet build` | **NOT RUN** (dilarang pada task ini). Kompilasi belum terbukti |
| Verifikasi simbol tanpa build | Terbaca di source: `FinPurchasingSourceSystems.Blm`, `FinGoodsReceiptConditionCodes`, `FinGoodsReceiptStatuses`, `DrugStockSourceDocumentTypes.GoodsReceipt` (`...PharmacyManagement.Enums`), `PhmDrugStockMutation.{SourceDocumentType, SourceDocumentId, QuantityChange}`, `DbSet` `FinGoodsReceiptItems` dan `PhmDrugStockMutations` |
| Tes statis FE membaca source BE (`finance-purchasing-logistics-link.test.mjs`) | PASS (route, izin, kode status, tanpa tulis/stok, penjaga di dua jalur) — ini bukti teks, **bukan** eksekusi |
| Migration / DDL / DML | Tidak ada |
| Tes runtime / UAT | **NOT RUN** |
| Automated test backend | NOT APPLICABLE — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) |

---

## 7. Risiko dan hal terbuka

1. **Kompilasi belum teruji.** Jalankan `dotnet build` sebelum deploy.
2. Konvensi `SourceDocumentId` (id GR atau id baris GR) harus diputuskan bersama pemilik Logistik sebelum penulis mutasi dibangun; sampai itu, status `POSTED_TO_STOCK` hanya akan muncul bila pihak lain menulis mutasi dengan salah satu bentuk tersebut.
3. `GetSummaryAsync` membaca semua GR `RECEIVED` pada filter; aman untuk volume saat ini, perlu ditinjau bila GR bertambah jauh.
4. Keputusan tersisa: `LGS-OQ-017` (jalur GR → saldo gudang), `LGS-OQ-018` (pemakaian Warehousing BLM), `LGS-OQ-015` (pemilik produk/approver Logistik). Rancangan skema untuk butir `BLOCKED_BY_SCHEMA` tercantum di laporan akhir task.
5. Task berikutnya yang disarankan: setelah `LGS-OQ-017` diputuskan — penulis penerimaan Logistik yang idempoten (migration oleh pemilik), lalu penyambungan retur supplier.
