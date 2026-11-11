# Laporan Perubahan Backend — `BE-FIN-147`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-147` |
| Judul | Respons PO, GR, dan jejak dokumen membawa sumber dokumen dan angka satuan dasar, tanpa mengubah field lama |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FR-FIN-284` (dan sisi respons dari `FR-FIN-281`); `FIN-DEC-273`; `FIN-DES-139`, `140`, `142`; `FIN-API-1.13` W.1–W.3 |
| Contract version | `contracts/api-contract.md` W.1–W.3 (Revisi 23, approved 2026-11-09) |
| Dependency | `BE-FIN-145` 🟡 |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1 (±12), berkas diubah 1 (6), logika bisnis 1 (cabang BLM pada jejak dokumen), kontrak API 1 (menambah field pada kontrak yang disetujui), database 0, keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh (`FE-FIN-062`) |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree memuat pekerjaan `BE-FIN-142`..`146` yang belum di-commit; tidak ditimpa) |
| Tanggal | 10 November 2026 |
| Status | 🟡 **Source selesai; `dotnet build` berhasil menurut laporan pemilik (10 November 2026, keluaran build Release kemudian dilampirkan pemilik (10 November 2026): `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`).** Perbandingan respons sebelum dan sesudah (W.T4) pada runtime belum dijalankan |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik dan prefix (registry) | `Fin`, "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (DTO, controller, dan service yang sudah ada). Tidak ada entity, tabel, migration, atau endpoint baru |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-API-001` (respons memakai `ApiResponse<T>` yang ada; DTO tetap berkas `Dtos/` yang sama) |
| Role/akses | Tidak ada atribut akses yang berubah. Bagian jejak dokumen tetap disaring oleh `ResolveAllowedSectionsAsync`; field baru hanya mengisi bagian yang memang boleh dibaca pemanggil |
| Branch | `Yasmina` |
| Catatan peringatan tugas | Pesan tugas mengingatkan bahwa endpoint ringkasan PO (`summary`) belum di-commit. **Sudah usang:** kode itu kini ada di riwayat commit (`FinancePurchaseOrderDtos.cs` terakhir diubah pada `98a9140c`) dan berkas PO tidak tercantum sebagai berubah di working tree sebelum task. Perubahan task ini bersifat tambahan, tidak menyentuh kelas ringkasan |
| Working tree sebelum task | Perubahan `BE-FIN-142`..`146` dan dokumen; tidak disentuh |

---

## 1. Masalah yang Diperbaiki

Sesudah `BE-FIN-142` PO dan GR dapat bertanda `BLM` dan baris GR menyimpan kondisi barang serta angka satuan dasar, tetapi API belum menampilkannya:

1. **Sumber dokumen tak terlihat.** Layar tidak dapat membedakan PO atau GR dari BLM dengan yang dibuat di Quilvian.
2. **Kondisi dan konversi tak terlihat.** Baris GR 80 baik, 15 menunggu periksa, 5 rusak tampil hanya sebagai satu angka `ReceivedQuantity`.
3. **Kemajuan penerimaan PO BLM salah.** `ItemProgress` pada jejak dokumen menjumlah `ReceivedQuantity` mentah, sehingga PO 1 BOX berisi 12 PCS dengan GR 6 PCS tampil "diterima 0,50 BOX" tanpa konteks, dan barang rusak ikut terhitung.

---

## 2. Proses Bisnis

Task ini hanya mengubah **bentuk data yang dikirim ke klien**; tidak ada aturan bisnis baru.

| Respons | Field baru | Nilai untuk dokumen lama | Nilai untuk dokumen BLM |
| --- | --- | --- | --- |
| `GET` PO (daftar dan detail) | `sourceSystem` | `"QUILVIAN"` | `"BLM"` |
| `GET` GR (daftar, detail, dan hasil aksi) | `sourceSystem` | `"QUILVIAN"` | `"BLM"` |
| Item GR | `conditionCode`, `sourceQuantity`, `sourceUomCode`, `quantityPerBaseUnit`, `baseUomCode`, `baseQuantity`, `acceptedBaseQuantity` | `null` semua | Nilai tersimpan dari BLM |
| Jejak dokumen › `itemProgress` | `sourceSystem` | `"QUILVIAN"` | `"BLM"` |
| Jejak dokumen › `itemProgress` | `baseUomCode`, `orderedBaseQuantity`, `physicalBaseQuantity`, `acceptedBaseQuantity`, `pendingInspectionBaseQuantity`, `damagedBaseQuantity`, `remainingBaseQuantity`, `isConversionResolved` | `null` semua | Dari `PurchasingReceiptQuantityCalculator` (`BE-FIN-145`) |

**Kolom lama tidak berubah.** `orderedQuantity`, `receivedQuantity`, `remainingQuantity` pada `itemProgress` tetap dihitung dengan rumus lama untuk **semua** PO (untuk PO BLM, `receivedQuantity` berisi jumlah sah dalam satuan PO yang dibulatkan 2 desimal, sesuai `FIN-DES-140`). `ReceivedQuantity` pada item GR tetap kolom lama.

**Contoh (PO 1 BOX = 12 PCS, GR-1 6 PCS baik).** `itemProgress`: `orderedQuantity` 1, `receivedQuantity` 0,50, `remainingQuantity` 0,50 (kolom lama, satuan PO); dan `baseUomCode` "PCS", `orderedBaseQuantity` 12, `physicalBaseQuantity` 6, `acceptedBaseQuantity` 6, `pendingInspectionBaseQuantity` 0, `damagedBaseQuantity` 0, `remainingBaseQuantity` 6, `isConversionResolved` true. Klien memformat angka satuan dasar apa adanya dan **tidak menghitung ulang**.

**Jalur tidak normal.** Bila ada baris GR yang konversinya tidak pasti, item itu `isConversionResolved = false` dan baris itu tidak dijumlahkan ke angka satuan dasar (aturan penghitung); angka lama tidak terpengaruh.

**Cara jejak dokumen mengisi kolom baru.** Query baris GR yang sudah ada diperluas dengan lima kolom BLM (nilai `NULL` untuk dokumen lama). Hanya bila PO bertanda `BLM`, baris-baris itu diteruskan ke `PurchasingReceiptQuantityCalculator.Calculate` bersama item PO; GR `CANCELLED` disaring oleh penghitung. PO lama tidak menyentuh cabang itu.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; registry; `contracts/api-contract.md` W.1–W.3; `FinanceGoodsReceiptDtos.cs`, `FinancePurchaseOrderDtos.cs`, `FinancePurchasingDocumentTrailDtos.cs`; `FinanceGoodsReceiptsController.cs`, `FinancePurchaseOrdersController.cs`; `FinancePurchasingDocumentTrailService.cs` (bagian muat PO/GR, akumulasi, dan `ItemProgress`); `PurchasingReceiptQuantityCalculator.cs`; seluruh pemanggil konstruktor DTO yang diubah.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceGoodsReceiptDtos.cs` | **Diperbarui** (+16/−0). `GoodsReceiptItemResponse` + 7 field nullable; `GoodsReceiptResponse` + `SourceSystem` (bawaan `"QUILVIAN"`; `GoodsReceiptDetailResponse` mewarisinya) |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchaseOrderDtos.cs` | **Diperbarui** (+4/−0). `PurchaseOrderResponse` + `SourceSystem` (bawaan `"QUILVIAN"`; detail mewarisi). Kelas `PurchaseOrderSummaryResponse` dan lainnya tidak disentuh |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingDocumentTrailDtos.cs` | **Diperbarui** (+16/−0). `PurchasingTrailItemProgress` + `SourceSystem` (bawaan `"QUILVIAN"`), 7 angka nullable, dan `IsConversionResolved` (`bool?`) |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingDocumentTrailService.cs` | **Diperbarui** (+46/−2; dua baris yang berubah hanya penambahan koma pada proyeksi dan `return new` → `var progress = new`). Cabang BLM untuk `ItemProgress`; rumus lama tidak diubah |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceGoodsReceiptsController.cs` | **Diperbarui.** `Map`, `MapDetail`, dan pemetaan item GR mengisi field baru (mengikuti pekerjaan `BE-FIN-146` pada berkas yang sama) |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchaseOrdersController.cs` | **Diperbarui.** `Map` dan `MapDetail` mengisi `SourceSystem` (+3/−1: baris `RowVersion` mendapat koma) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Hanya **penambahan** field pada respons yang ada (`FIN-API-1.13` W.1–W.3). Tidak ada field lama yang berubah nama, tipe, atau nilai; tidak ada endpoint baru |
| Database | `NOT APPLICABLE` (query membaca lima kolom tambahan dari `BE-FIN-142`; butuh migration `BE-FIN-144` diterapkan sebelum aplikasi aktif) |
| Keamanan/Auth | Tidak berubah |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Respons endpoint berikut bertambah field (hak akses tidak berubah).

#### Corporate / Finance Management / Purchasing / Goods Receipt

Base URL `api/v1/corporate/finance-management/purchasing/goods-receipts`.

| Method | Path | Kegunaan | Hak akses | Field respons baru |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar GR | `FinanceGoodsReceipt : Read` | `sourceSystem` |
| `GET` | `/{id}` | Detail GR | `FinanceGoodsReceipt : Read` | `sourceSystem`; per item `conditionCode`, `sourceQuantity`, `sourceUomCode`, `quantityPerBaseUnit`, `baseUomCode`, `baseQuantity`, `acceptedBaseQuantity` |
| `GET` | `/{id}/document-trail` | Jejak dokumen | `FinanceGoodsReceipt : Read` | `itemProgress[]`: `sourceSystem`, `baseUomCode`, `orderedBaseQuantity`, `physicalBaseQuantity`, `acceptedBaseQuantity`, `pendingInspectionBaseQuantity`, `damagedBaseQuantity`, `remainingBaseQuantity`, `isConversionResolved` |
| `POST`, `PUT`, `POST /{id}/cancel` | — | Hasil aksi memakai `GoodsReceiptResponse` | tetap | `sourceSystem` |

#### Corporate / Finance Management / Purchasing / Purchase Order

Base URL `api/v1/corporate/finance-management/purchasing/purchase-orders`.

| Method | Path | Kegunaan | Hak akses | Field respons baru |
| --- | --- | --- | --- | --- |
| `GET` | `/`, `/{id}` dan hasil aksi | Daftar, detail, dan hasil aksi PO | tetap | `sourceSystem` |
| `GET` | `/{id}/document-trail` | Jejak dokumen PO | tetap | `itemProgress[]` seperti di atas |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| QBE preflight / conformance | Lulus | `PASS` | DTO tetap di `Dtos/`; respons memakai `ApiResponse<T>`; tidak ada hardcode role |
| Review scope | Enam berkas dalam cakupan; tidak menyentuh model, konfigurasi, migration, atau FE | `PASS` | `git status --short` |
| Keseimbangan kurung (enam berkas) | Seimbang | `PASS` | Skrip statis (bukan kompilasi) |
| Nama field dan tipe baru sama dengan kontrak W | 8 field GR + `sourceSystem`, 9 field `itemProgress`, `sourceSystem` PO: ada; semuanya nullable kecuali `sourceSystem` | `PASS` | Skrip pencocokan terhadap `api-contract.md` bagian W |
| W.T4: field lama tidak berubah untuk dokumen lama | Pada tiga DTO: **0 baris dihapus**; pada service jejak dokumen hanya dua baris yang diubah, keduanya tanpa efek nilai (koma tambahan, `return new` → `var progress = new`); nilai `orderedQuantity`/`receivedQuantity`/`remainingQuantity` tetap dihitung oleh kode yang sama; kolom baru bernilai `null` atau `"QUILVIAN"` untuk PO lama | `PASS` (review diff) | `git diff` |
| Tidak ada tabrakan nama variabel lokal pada `GetTrailAsync` | Tidak ada | `PASS` | `grep` |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Eksekusi runtime (query EF yang diperluas, W.T4) | Belum. Migration sudah diterapkan (laporan pemilik), tetapi respons belum dibandingkan | `NOT RUN` | Menunggu pengujian pemilik |

Uji manual: `NOT APPLICABLE` (dokumen BLM belum dapat ada sebelum migration diterapkan dan integrasi dibangun).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** semua perintah `dotnet`; perbandingan respons sebelum dan sesudah pada server hidup.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Field baru persis kontrak W, boleh `null` | Terpenuhi | Pencocokan nama dan tipe terhadap kontrak |
| 2. Field lama tidak berubah nama/tipe/nilai untuk dokumen lama (W.T4) | Terpenuhi menurut review diff; **belum dibandingkan pada respons nyata** | Bagian 5 |
| 3. `ItemProgress` PO BLM dari penghitung, PO lama rumus lama | Terpenuhi menurut review diff | Cabang `externalProgressByItem` hanya aktif untuk `SourceSystem = BLM` |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | Terpenuhi (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| DoD: perbandingan respons W.T4 pada runtime | **Belum terpenuhi** | Migration sudah diterapkan (laporan pemilik); perbandingan belum dilakukan |

Status akhir **🟡**: build lulus (laporan pemilik) dan kriteria 1–3 terpenuhi menurut review diff, tetapi W.T4 (perbandingan respons nyata) dan query EF yang diperluas belum dijalankan.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Keputusan implementasi | (1) Query baris GR yang sudah ada **diperluas** (lima kolom) alih-alih menambah query kedua, supaya tidak ada perjalanan basis data tambahan; untuk dokumen lama kolomnya `NULL` dan tidak dipakai. (2) `receivedQuantity` lama untuk PO BLM tidak diganti dengan angka penghitung: ia tetap rumus lama (jumlah sah dalam satuan PO) sehingga klien lama tidak berubah |
| Masalah yang diketahui | (1) Jejak dokumen `GoodsReceipts` (ringkasan per GR) dan item PO pada detail PO **tidak** memuat field BLM karena kontrak W tidak menyebutnya. (2) Setelah migration `BE-FIN-144` belum diterapkan, **semua** query PO/GR (termasuk jejak dokumen yang kini membaca lima kolom BLM) akan gagal; ini risiko yang sama dengan `BE-FIN-142` dan tercakup aturan rilis satu paket (`FIN-DES-138`) |
| Risiko tersisa | Kompilasi lulus (laporan pemilik). Proyeksi anonim yang diperluas belum dijalankan pada EF dan basis data |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` enam berkas source (dua di antaranya juga disentuh `BE-FIN-146`); ditambah pekerjaan `BE-FIN-142`..`146` yang belum di-commit. Tidak ada berkas staged |
| Langkah berikutnya | Setelah migration diterapkan ke database uji: bandingkan respons PO/GR lama sebelum dan sesudah (W.T4), lalu task ini dapat ditandai ✅; `FE-FIN-062`/`063` |
