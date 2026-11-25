# LGS-BLM-INVESTIGATION — Investigasi Pemakaian BLM untuk Logistik

| Field | Nilai |
| --- | --- |
| Blueprint ID | `LGS-BP-001` r2 |
| Pertanyaan | `LGS-OQ-018` — status tetap **`open`**; laporan ini bukti, bukan keputusan |
| Tanggal | 2026-11-17 |
| Sifat | Read-only: source code, dokumen blueprint, dan query baca ke database pengembangan |
| Menggantikan | Catatan singkat `evidence/02-penjelasan-blm.md` (dihapus; isinya dilebur ke sini) |

## 1. Repository, branch, SHA

| Repository | Branch | SHA | Akses |
| --- | --- | --- | --- |
| `DevBenari/NewQuilvianSystemBackend` (`C:\tmp\phm`) | `Ikbal` | `d946b20b` (= `origin/QuilvianIntegrationBackend` per 2026-11-17) | Dibaca langsung |
| `DevBenari/QuilvianSystemFrontendDev` | `Ikbalv2` | `1e9adc790` | Dibaca langsung |
| Repository BLM | `PurchasingSystemDeveloper` | `08e8a9ae` (menurut dokumen Finance) | **Tidak tersedia di mesin ini.** Fakta BLM hanya dari hasil audit Finance yang tercatat |
| Database tim `QuilvianNewDevMaster` | — | — | Query baca |
| Database lokal `QuilvianNewDevIkbalFr` | — | — | Query baca |
| Konfigurasi, log, atau transaksi operasional BLM | — | — | **Tidak ada akses** |

## 2. Jawaban atas sepuluh pertanyaan

Kolom "Jenis bukti" membedakan **keberadaan fitur** (kode atau dokumen) dari **pemakaian
operasional** (transaksi nyata rumah sakit).

| No | Pertanyaan | Jawaban | Jenis bukti |
| --- | --- | --- | --- |
| 1 | Apakah BLM hanya sumber dokumen PO dan GR? | **Tidak hanya.** BLM adalah sumber utama PO dan GR bagi Finance (`FIN-DEC-261`), dan menurut audit Finance BLM juga punya fondasi Warehousing (`FIN-DEC-289`). Di sisi Quilvian, hanya PO, GR, dan pembatalannya yang dimodelkan dari BLM | Keberadaan (dokumen + kode Quilvian) |
| 2 | Apakah BLM mengelola saldo persediaan? | **BELUM TERVERIFIKASI.** `FIN-DEC-289` menyebut "Warehousing" dan "riwayat transaksi" tanpa merinci adanya buku saldo stok. Repository BLM tidak dapat dibaca dari sini | — |
| 3 | Apakah transaksi gudang BLM dipakai operasional rumah sakit? | **BELUM TERVERIFIKASI.** Tidak ada akses ke konfigurasi, log, atau data transaksi BLM | — |
| 4 | Apakah ada endpoint sinkronisasi stok BLM ↔ Quilvian? | **Tidak ada di Quilvian.** Kotak masuk `FinPurchasingInboundMessage` hanya dirancang untuk kejadian PO/GR (`PO_UPSERTED`, `GR_UPSERTED`, `PO_CANCELLED`, `GR_CANCELLED`), dan **belum ada controller atau service yang memprosesnya** | Kode Quilvian |
| 5 | Apakah barang BLM dipetakan ke master obat/barang Quilvian? | **Tidak.** Item PO hanya menyimpan `ExternalProductId`/`ExternalProductCode` milik BLM. Ada tabel pemetaan supplier (`FinPurchasingSupplierMap`), tetapi **tidak ada tabel pemetaan produk** | Kode Quilvian |
| 6 | Apakah GR dapat menambah saldo otomatis? | **Tidak.** `FinanceGoodsReceiptService` tidak memanggil stok Farmasi. GR bersumber BLM bahkan ditolak bila dicatat dari Quilvian (`FIN-VAL-315`) | Kode Quilvian |
| 7 | Apakah retur ke supplier mengurangi saldo? | **Tidak.** `FinanceSupplierReturnService` tidak menyentuh stok | Kode Quilvian |
| 8 | Apakah ada anti-duplikasi dan sinkronisasi ulang? | **Sebagian, baru di skema.** Indeks unik `(SourceSystem, EventId)` pada kotak masuk dan tabel `FinPurchasingIdempotencyRecord` sudah ada, tetapi pengolahnya belum ada. Audit Finance mencatat BLM sendiri belum punya event ID, idempotency, dan outbox (`FIN-DEC-289`) | Kode Quilvian + dokumen |
| 9 | Siapa pemilik data dan sumber kebenaran tiap jenis persediaan? | Dokumen PO/GR: BLM, dicerminkan Finance (`FIN-DEC-261`). Saldo obat di Quilvian: Farmasi lewat `PhmDrugStockBalance` (`PHA-DEC-003`, `LGS-DEC-001`). Barang non-medis: **belum ditetapkan** (`LGS-OQ-015`). Saldo gudang di BLM: **belum diketahui** | Dokumen + kode |
| 10 | Warehousing BLM hanya di kode atau terbukti dipakai? | **Hanya terbukti ada di kode**, berdasarkan audit statis Finance (`FIN-DEC-289`). Pemakaian operasionalnya **BELUM TERVERIFIKASI** | Keberadaan |

## 3. Bukti file dan baris

| Bukti | Lokasi |
| --- | --- |
| BLM sumber utama PO dan GR; BLM mengirim (push) ke Quilvian | `docs/module-blueprints/finance-management/00-interview-decisions.md`, `FIN-DEC-261` |
| BLM punya fondasi Purchasing dan Warehousing; yang belum ada: event ID, idempotency, outbox, API rekonsiliasi | idem, baris 3573 (`FIN-DEC-289`) |
| Risiko di BLM: respons pembatalan GR bisa tidak mencerminkan database; satuan PO/GR tidak konsisten | idem, `FIN-DEC-290` |
| BLM sistem luar, hanya dirujuk lewat identitas eksternal dan snapshot | `finance-management/02-backend-architecture.md` baris 7356 |
| Penanda sumber dokumen PO | `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs` baris 43–64 (`SourceSystem`, `ExternalPurchaseOrderId`, `LastSyncedAt`, `FinPurchasingSourceSystems`) |
| Penanda sumber dokumen GR | `Models/FinGoodsReceipt.cs` baris 31–41 |
| ID produk BLM pada item PO | `Models/FinPurchaseOrderItem.cs` (`ExternalProductId`, `ExternalProductCode`, `QuantityPerBaseUnit`, `BaseQuantity`) |
| Kondisi barang dan jumlah sah pada item GR | `Models/FinGoodsReceiptItem.cs` (`ConditionCode`, `AcceptedBaseQuantity`) |
| GR/pembatalan/koreksi bersumber BLM ditolak bila dari Quilvian | `Services/FinanceGoodsReceiptService.cs` baris 100–102, 206–208, 319–320 |
| Status PO BLM tidak boleh memakai rumus lokal | idem baris 458–463 |
| Kotak masuk BLM dan indeks anti-duplikasi | `Models/FinPurchasingInboundMessage.cs`; `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingInboundMessageConfiguration.cs` baris 62–64 |
| Kotak masuk, pemetaan supplier, konflik sinkron, pembatalan eksternal tidak punya pemakai di controller/service | Pencarian `FinPurchasingInboundMessage|FinPurchasingSupplierMap|FinPurchasingSyncConflict|FinPurchasingExternalCancellation` di luar `Models/` dan konfigurasi: 0 hasil |
| Tidak ada konfigurasi integrasi BLM | Pencarian `blm` pada `appsettings*.json`: 0 hasil |
| Frontend menyatakan integrasi belum tersedia | `QuilvianSystemFrontendDev/src/lib/constants/finance/purchasing/purchasing-monitoring-constants.jsx` baris 24–28: "Integrasi otomatis BLM ke Quilvian belum tersedia" |
| Stok obat bertambah hanya lewat saldo awal, transfer, dan retur | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` (`ReceiveBatchAsync` dipanggil `StockTransferService` dan `DrugReturnService` saja); sumber `GoodsReceipt` di `DrugStockEnums.cs` baris 78 tidak dipakai |

### Data lingkungan pengembangan (bukan bukti operasional rumah sakit)

| Database | Hasil query baca 2026-11-17 |
| --- | --- |
| `QuilvianNewDevMaster` | `FinPurchaseOrder`: 0 baris; `FinGoodsReceipt`: 0 baris; `FinPurchasingInboundMessage`, `FinPurchasingSupplierMap`, `FinPurchasingSyncConflict`, `FinPurchasingExternalCancellation`: masing-masing 0 baris; item PO ber-`ExternalProductId`: 0 |

Artinya belum ada data BLM yang mengalir ke lingkungan pengembangan tim. Ini **tidak**
membuktikan rumah sakit tidak memakai BLM.

## 4. Endpoint dan kontrak integrasi yang ada

| Endpoint Quilvian | Izin | Kaitan BLM |
| --- | --- | --- |
| `GET/POST/PUT /api/v1/corporate/finance-management/purchasing/purchase-orders` (+ `submit`, `approve`, `reject`, `cancel`, `close`, `document-trail`) | `FinancePurchaseOrder:*` | Untuk dokumen lokal; PO BLM hanya dibaca |
| `GET/POST/PUT /purchasing/goods-receipts` (+ `cancel`, `document-trail`) | `FinanceGoodsReceipt:*` | Menolak pencatatan, pembatalan, dan koreksi GR bersumber BLM |
| Endpoint penerima kiriman BLM | — | **Belum ada** |
| Endpoint sinkron stok | — | **Belum ada** |

## 5. Kesenjangan integrasi

1. Pengolah kotak masuk BLM belum dibangun (prioritas P0 Finance, `FIN-DEC-271`).
2. Tidak ada pemetaan produk BLM ↔ `MstDrug` atau katalog non-medis.
3. Tidak ada jalur posting GR → saldo gudang.
4. Retur ke supplier tidak mengurangi saldo.
5. Tidak ada informasi apakah BLM memegang saldo gudang dan apakah saldonya dipakai.

## 6. Risiko saldo ganda

| Skenario | Risiko |
| --- | --- |
| BLM memegang saldo gudang obat **dan** Farmasi juga memposting saldo di `PhmDrugStockBalance` | Dua buku saldo untuk barang yang sama; selisih tidak terhindarkan. **Inilah yang dilarang `LGS-DEC-001`** |
| GR BLM diposting ke saldo Quilvian **dan** petugas juga mencatat saldo awal/penyesuaian untuk barang yang sama | Barang terhitung dua kali |
| Kiriman BLM diproses ulang (sinkron ulang/retry) tanpa kunci idempotensi per baris | Posting berulang |
| Kondisi saat ini: barang masuk lewat pengadaan tetapi saldo Quilvian hanya bertambah lewat saldo awal/penyesuaian | Saldo Quilvian mudah menyimpang dari fisik; jejak asal barang hilang |

## 7. Pilihan arsitektur

| Pilihan | Isi | Syarat | Risiko |
| --- | --- | --- | --- |
| **P1. Quilvian (Farmasi) satu-satunya buku saldo** untuk obat; dokumen BLM menjadi sebab posting lewat pemetaan produk | Penerimaan gudang merujuk baris GR BLM, posting sekali via `ReceiveBatchAsync` (sumber `GoodsReceipt`) | Warehousing BLM terbukti **tidak** dipakai untuk saldo obat; `LGS-OQ-017` diputuskan | Rendah bila syarat terpenuhi |
| **P2. BLM buku saldo, Quilvian cermin baca** | Logistik menampilkan saldo dari BLM | BLM mengirim mutasi gudang; Farmasi harus berhenti memposting saldo sendiri | **Tinggi:** bertentangan dengan alur Farmasi yang sudah berjalan (resep, penyerahan, reservasi memakai `PhmDrugStockBalance`) |
| **P3. Dibagi per jenis barang** | Obat/alkes/BMHP di Quilvian (Farmasi); barang non-medis di BLM bila memang dipakai, Logistik menampilkan | Pemakaian BLM per jenis barang terverifikasi; `LGS-OQ-015` diputuskan | Sedang; perlu batas jenis barang yang tegas |

## 8. Rekomendasi teknis

1. **Sampai `LGS-OQ-018` terverifikasi, jangan memposting saldo dari dokumen BLM** dan jangan membangun cermin saldo BLM.
2. Pertahankan `PhmDrugStockBalance` sebagai satu-satunya jalur posting saldo obat di Quilvian (sudah berlaku).
3. Verifikasi pemakaian BLM dengan bukti operasional, bukan source code:
   - daftar gudang dan jenis barang yang aktif bertransaksi di BLM 90 hari terakhir;
   - jumlah transaksi masuk/keluar/opname per gudang di BLM pada periode yang sama;
   - apakah unit rumah sakit meminta barang lewat BLM atau lewat Quilvian;
   - akses baca ke laporan atau replika database BLM untuk pengecekan.
4. Siapkan desain pemetaan produk dan penerimaan gudang untuk P1 dan P3 (lihat `evidence/01-daftar-pekerjaan-backend-first.md`, bagian D), tanpa implementasi.

## 9. Keputusan bisnis yang masih diperlukan

| ID | Keputusan | Pemilik |
| --- | --- | --- |
| `LGS-OQ-018` | Hasil verifikasi pemakaian Warehousing BLM dan pilihan P1/P2/P3 | Pemilik Finance + pengelola BLM + pemilik Farmasi |
| `LGS-OQ-017` | Cara GR masuk ke saldo gudang | Pemilik Finance + pemilik Farmasi |
| `LGS-OQ-015` | Pemilik barang non-medis dan approver Logistik | Manajemen |
