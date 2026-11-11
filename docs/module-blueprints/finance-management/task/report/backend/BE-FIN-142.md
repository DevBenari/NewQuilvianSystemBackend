# Laporan Perubahan Backend — `BE-FIN-142`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-142` |
| Judul | PO dan GR dapat menyimpan sumber `QUILVIAN`/`BLM`, identitas BLM, snapshot produk dan konversi satuan, serta kondisi barang dan jumlah sah, tanpa mengubah dokumen lama |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FR-FIN-272`, `273`, `274`, `275`; `FIN-DEC-263`, `265`, `268`, `274`, `275`, `276`, `277`; `FIN-DES-128`..`131`, `134`, `135`, `140`; `FIN-VAL-300`..`306` |
| Contract version | `data/data-dictionary.md` bagian 13.2–13.5 (Revisi 23, approved 2026-11-09); `FIN-VAL-1.15` bagian W; `FIN-API-1.13` (tidak ada endpoint pada task ini) |
| Dependency | Tidak ada (gelombang 1) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1 (±20), berkas diubah 1 (8), logika bisnis 1 (deklaratif), kontrak API 0, database 2 (dampak schema/entity), keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree bersih sebelum task; seluruh pekerjaan sebelumnya sudah di-commit pemilik) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai (10 November 2026).** Source selesai; `dotnet build` berhasil menurut laporan pemilik (keluaran build Release kemudian dilampirkan pemilik (10 November 2026): `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`). Migration sudah diterapkan (laporan pemilik) dan uji tulis W.T14–W.T16 lulus 22 dari 22 skenario (total W.T14–W.T17: 34 dari 34; rincian di laporan `BE-FIN-144`, bagian 5.1) |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik dan prefix (registry) | `Fin`, baris "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", lifecycle `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `TOUCHED LEGACY` (empat model dan empat konfigurasi yang sudah ada). Tidak ada entity baru pada task ini |
| QBE yang berlaku | `QBE-MOD-001` (capability tetap di pemiliknya), `QBE-NAM-002` (prefix registry; tidak ada nama baru). `QBE-NAM-001` (tanpa `Trx*`), `QBE-DB-001`/`002` (`LEGACY MIGRATION`) **tidak berlaku** |
| Branch | `Yasmina` |
| Selisih governance | Salinan registry di suite skill (`Finance / Finance`) lebih ringkas daripada salinan di repository (`docs/engineering/`, memuat sub-modul `Purchasing`). Sesuai aturan, salinan repository yang dipakai. Salinan `BACKEND_ENGINEERING_CONTRACT.md` identik. Folder `agents/rules/` peninggalan **tidak ditemukan** |
| Working tree sebelum task | Bersih |

---

## 1. Masalah yang Diperbaiki

BLM akan menjadi sumber utama PO dan Penerimaan Barang (GR) (`FIN-DEC-261`), tetapi tabel PO/GR di Quilvian belum mampu:

1. **Membedakan dokumen BLM dari dokumen buatan Quilvian.** Tanpa penanda sumber, Quilvian tidak bisa menolak perubahan lokal atas dokumen BLM tanpa juga menolaknya untuk dokumen lama.
2. **Menyimpan identitas BLM.** Kiriman yang sama bisa membuat dokumen ganda, dan baris GR tidak dapat ditempelkan ke item PO yang benar tanpa mencocokkan nama produk (dilarang `FIN-DEC-275`).
3. **Menyimpan konversi satuan per produk.** Contoh: PO 1 BOX berisi 12 PCS, GR pertama 6 PCS. Tanpa konversi, kolom `ReceivedQuantity` (6) dibandingkan dengan `Quantity` (1) dan PO terbaca "sudah lebih dari penuh".
4. **Mengenal kondisi barang.** Barang rusak atau menunggu pemeriksaan tidak boleh dihitung sebagai penerimaan sah (`FIN-DEC-268`).

Task ini menyiapkan **kolomnya saja**. Penghitungan, penolakan, dan pengisian dari BLM dikerjakan task lain (`BE-FIN-145`, `146`, dan slice integrasi).

---

## 2. Proses Bisnis

Task ini tidak mengubah proses bisnis yang berjalan. Aturan yang ditanam di database:

| Aturan | Cara ditegakkan | Contoh |
| --- | --- | --- |
| Dokumen baru dan lama bertanda `QUILVIAN` secara bawaan | Kolom `SourceSystem` NOT NULL bawaan `'QUILVIAN'` | PO lama yang sudah ada otomatis `QUILVIAN` saat migration nanti diterapkan |
| Identitas BLM terisi jika dan hanya jika sumbernya `BLM` | `CK_FinPurchaseOrder_ExternalIdentity`, `CK_FinGoodsReceipt_ExternalIdentity` | PO `BLM` tanpa `ExternalPurchaseOrderId` ditolak database |
| PO `BLM` berjenjang `EXTERNAL`, PO lain tidak | `CK_FinPurchaseOrder_ExternalTier` + check tier yang diperluas | PO `QUILVIAN` dengan jenjang `EXTERNAL` ditolak |
| Satu dokumen BLM hanya masuk sekali | Index unik terfilter pada identitas eksternal (PO, item PO, GR, item GR) | Dua PO `BLM` ber-ID sama: yang kedua ditolak |
| Kolom snapshot BLM terisi semua atau kosong semua | `CK_FinPurchaseOrderItem_BlmFields`, `CK_FinGoodsReceiptItem_BlmFields` | Item PO BLM tanpa isi per satuan ditolak; item lama (semua kosong) lolos |
| Tidak ada konversi bawaan 1 | `QuantityPerBaseUnit` nullable dan wajib `> 0` bila terisi | Isi per satuan 0 atau negatif ditolak; kosong = bukan baris BLM |
| Sah tidak melebihi fisik; nol bila kondisi bukan `GOOD` | `CK_FinGoodsReceiptItem_AcceptedBaseQuantity` | Baris 5 PCS `DAMAGED` dengan sah 5 ditolak; sah 80 dari fisik 100 `GOOD` lolos |
| Kuantitas satuan dasar tanpa pembulatan dini | `numeric(18,6)`; `Quantity`/`ReceivedQuantity` lama tetap `(18,2)` | 1 PCS dari BOX isi 12 = 0,083333 BOX tersimpan sebagai 1,000000 PCS |

**Jalur tidak normal.** Baris GR lama dan GR lokal baru **tidak** mengisi kolom BLM (semuanya `NULL`) dan tetap dihitung dengan aturan lama (`FIN-DEC-277`); tidak ada backfill.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TEST_POLICY.md`, `TASK_CLASSIFICATION.md`, `REPORT_TEMPLATE.md`; `data/data-dictionary.md` 13; `contracts/validation-matrix.md` W; `02-backend-architecture.md` W; roadmap REV-24; empat model dan empat konfigurasi PO/GR; `FinanceApprovalTierResolver.cs` (nilai `TIER_1`/`TIER_2`); model BLM `MstProductConversion` (read-only).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs` | **Diperbarui.** + `SourceSystem`, `ExternalPurchaseOrderId`, `ExternalUpdatedAt`, `LastSyncedAt`; + kelas konstanta `FinPurchasingSourceSystems` (`QUILVIAN`, `BLM`) dan `FinPurchaseOrderExternalApprovalTier` (`EXTERNAL`); komentar `ApprovalTier` diperbarui |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrderItem.cs` | **Diperbarui.** + `ExternalPurchaseOrderItemId`, `ExternalProductId`, `ExternalProductCode`, `SourceUomCode`, `QuantityPerBaseUnit`, `BaseUomCode`, `BaseQuantity` (semua nullable) |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceipt.cs` | **Diperbarui.** + `SourceSystem`, `ExternalReceiveOrderId`, `ExternalUpdatedAt`, `LastSyncedAt` |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceiptItem.cs` | **Diperbarui.** + `ExternalReceiveOrderItemId`, `ConditionCode`, `SourceQuantity`, `SourceUomCode`, `QuantityPerBaseUnit`, `BaseUomCode`, `BaseQuantity`, `AcceptedBaseQuantity` (semua nullable); + kelas konstanta `FinGoodsReceiptConditionCodes` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchaseOrderConfiguration.cs` | **Diperbarui.** Check tier diperluas ke `EXTERNAL`; + `CK_…_SourceSystem`, `CK_…_ExternalIdentity`, `CK_…_ExternalTier`; `SourceSystem` NOT NULL bawaan `'QUILVIAN'`; kolom waktu `timestamptz`; index unik terfilter `IX_FinPurchaseOrder_SourceSystem_ExternalPurchaseOrderId` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchaseOrderItemConfiguration.cs` | **Diperbarui.** + `CK_…_BlmFields`, `CK_…_QuantityPerBaseUnit`; panjang kolom teks; `numeric(18,6)`; index unik terfilter `ExternalPurchaseOrderItemId` dan index terfilter `ExternalProductId` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinGoodsReceiptConfiguration.cs` | **Diperbarui.** + `CK_…_SourceSystem`, `CK_…_ExternalIdentity`; `SourceSystem` bawaan `'QUILVIAN'`; kolom waktu; index unik terfilter `IX_FinGoodsReceipt_SourceSystem_ExternalReceiveOrderId` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinGoodsReceiptItemConfiguration.cs` | **Diperbarui.** + `CK_…_BlmFields`, `CK_…_ConditionCode`, `CK_…_QuantityPerBaseUnit`, `CK_…_AcceptedBaseQuantity`; `numeric(18,6)`; index unik terfilter `ExternalReceiveOrderItemId` |

Total: 8 berkas, +205 baris, −4 baris (hanya penggantian baris `ToTable`/komentar/check tier yang diperluas). Tidak ada berkas baru, tidak ada migration, tidak ada perubahan service, controller, atau DTO.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint, DTO, atau mapping yang berubah (DTO pada `BE-FIN-147`) |
| Database | Dampak schema pada empat tabel (lihat `02-backend-architecture.md` W.8). Migration dibuat pada `BE-FIN-144` dan, menurut laporan pemilik (10 November 2026), sudah diterapkan. Selama migration belum diterapkan, **kode ini tidak boleh dirilis atau digabung ke branch rilis**: EF akan memilih kolom baru pada setiap query PO/GR dan daftar/detail PO/GR yang sudah berjalan akan gagal (`FIN-DES-138`, disetujui `FIN-DEC-273`) |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| QBE preflight / conformance | Lulus: prefix `Fin` terdaftar `ACTIVE` untuk `FinanceManagement / Purchasing`; nama entity, konfigurasi, dan tabel tidak berubah; tidak ada `Trx*`, `DbContext` di controller, atau generator nomor baru | `PASS` | Bacaan registry dan diff |
| Review diff / scope | Hanya 8 berkas dalam cakupan; tidak ada service, controller, DTO, migration, seed, atau konfigurasi aplikasi | `PASS` | `git status --short`, `git diff --stat` |
| Keseimbangan kurung kurawal/kurung (delapan berkas) | Seimbang | `PASS` | Pemeriksaan skrip statis (bukan kompilasi) |
| Kesesuaian dengan kamus data 13.2–13.5 | Kolom, tipe, panjang, presisi, bawaan, check, dan index sama dengan kamus data (`CK_FinPurchaseOrder_ExternalTier` termasuk) | `PASS` | Perbandingan diff dengan kamus data |
| Kolom lama tidak berubah tipe, presisi, atau arti | Tidak ada baris lama yang diubah selain check tier dan `ToTable` | `PASS` | Diff |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Penerapan constraint pada PostgreSQL (W.T14–W.T16) | Lulus: semua skenario yang menyangkut tabel PO/GR ditolak atau diterima sesuai harapan; rincian di `BE-FIN-144` bagian 5.1 | `PASS` (laporan pemilik) | Pemilik, 10 November 2026 |

Uji manual: `NOT APPLICABLE` pada task ini (tidak ada perilaku runtime yang berubah sebelum migration).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** `dotnet restore`/`dotnet build` (instruksi pemilik), `dotnet ef migrations add`, `dotnet ef database update` (wewenang terpisah, `FIN-DEC-273`), SQL pada database uji.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom, tipe, panjang, presisi `(18,6)`, bawaan, dan check persis kamus data 13 | Terpenuhi (review diff; kompilasi lulus menurut laporan pemilik) | Diff terhadap kamus data 13.2–13.5 |
| 2. `SourceSystem` bawaan `QUILVIAN` | Terpenuhi | Properti bawaan `FinPurchasingSourceSystems.Quilvian` dan `HasDefaultValue` pada kedua konfigurasi header |
| 3. Check "semua atau tidak sama sekali" pada item PO dan item GR | Terpenuhi | `CK_FinPurchaseOrderItem_BlmFields` (enam kolom), `CK_FinGoodsReceiptItem_BlmFields` (delapan kolom) |
| 4. `CK_FinPurchaseOrder_ApprovalTier` menerima `EXTERNAL` dan `CK_FinPurchaseOrder_ExternalTier` ada | Terpenuhi | `FinPurchaseOrderConfiguration.cs` |
| 5. Kolom lama tidak berubah tipe, presisi, maupun arti | Terpenuhi | Diff: tidak ada baris properti lama yang diubah |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | Terpenuhi (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| DoD: bukti penerapan constraint di database | Bukan DoD task ini | Roadmap menempatkan W.T14–W.T18 pada `BE-FIN-144`. Butir ini sebelumnya tertulis di laporan ini sebagai kehati-hatian tambahan; kini dipindahkan ke pemiliknya |

Status akhir **✅**: kriteria 1–5 terbukti dari diff, kompilasi lulus (laporan pemilik), dan laporan tracked ada, sesuai DoD roadmap. Bukti constraint di PostgreSQL ada pada `BE-FIN-144`.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kode ini **tidak boleh dirilis atau digabung ke branch rilis** sebelum `BE-FIN-144` (migration) selesai dan diterapkan ke database (`FIN-DES-138`) |
| Masalah yang diketahui | (1) Check `CK_FinPurchaseOrder_ApprovalTier` diganti isinya; pada migration, EF akan men-drop lalu menambah constraint dengan nama sama (aman bagi data lama). (2) Panjang `ExternalProductCode` (100), `SourceUomCode` (30), `BaseUomCode` (30), `ConditionCode` (30) adalah asumsi sampai kontrak BLM diterima (`FIN-OQ-141`). (3) Pemeriksaan "satuan dasar GR = satuan dasar item PO" tidak bisa dibuat sebagai check satu tabel; ditegakkan pengolah kiriman nanti |
| Risiko tersisa | Kompilasi dan constraint di PostgreSQL lulus (laporan pemilik). Catatan: check `CK_FinGoodsReceiptItem_ConditionCode` belum diuji sendirian (lihat `BE-FIN-144` 5.1) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` pada 8 berkas di atas; tidak ada berkas staged atau tidak terlacak dari task ini |
| Langkah berikutnya | `BE-FIN-144`: penerapan migration ke database uji dan W.T1, W.T2, W.T14–W.T18 |
