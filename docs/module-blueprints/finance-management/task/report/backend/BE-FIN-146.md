# Laporan Perubahan Backend — `BE-FIN-146`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-146` |
| Judul | Petugas tidak dapat membuat, mengubah, atau membatalkan GR dokumen BLM dari Quilvian, dan status PO BLM mengikuti penghitung; dokumen lama tetap seperti sekarang |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FR-FIN-281`, `282`, `283`; `FIN-DEC-262`, `265`, `277`; `FIN-DES-141`, `142`; `FIN-VAL-315`, `316`; `FIN-API-1.13` W.1; `FIN-STATE-1.12` W.4 |
| Contract version | `contracts/api-contract.md` W.1, `contracts/validation-matrix.md` W (Revisi 23, approved 2026-11-09) |
| Dependency | `BE-FIN-145` 🟡 |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1 (±12), berkas diubah 0 (3 berkas, tambahan kecil), logika bisnis 1, kontrak API 1 (memakai kontrak yang disetujui, menambah kode 409), database 0, keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. DTO tidak disentuh (`BE-FIN-147`) |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree memuat pekerjaan `BE-FIN-142`..`145` yang belum di-commit; tidak ditimpa) |
| Tanggal | 10 November 2026 |
| Status | 🟡 **Source selesai; `dotnet build` berhasil menurut laporan pemilik (10 November 2026, keluaran build Release kemudian dilampirkan pemilik (10 November 2026): `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`).** Belum ada eksekusi runtime, kriteria 2 baru sebagian, dan selisih bentuk respons 409 dengan kontrak `W.1` sudah diputuskan pemilik pada 10 November 2026: contoh kontrak dikoreksi, kode tidak diubah (`FIN-DEC-292`) |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik dan prefix (registry) | `Fin`, "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (`FinanceGoodsReceiptService`, `FinanceGoodsReceiptsController`, `PurchasingExceptions`). Tidak ada entity, tabel, atau migration baru |
| QBE yang berlaku | `QBE-MOD-001`. `QBE-API-001`: respons memakai `ApiResponse<T>` yang sudah ada. Tidak ada endpoint baru, jadi tidak ada atribut akses baru |
| Role/akses | Tidak ada hardcode role. Penolakan memakai **data dokumen** (`SourceSystem`), yaitu kelayakan data yang melekat pada dokumen, bukan penentu kewenangan. `[AccessAction]`/`[AccessPermission]` pada ketiga action tidak berubah |
| Branch | `Yasmina` |
| Selisih governance | Sama dengan task sebelumnya |
| Working tree sebelum task | 27 entri berubah milik `BE-FIN-142`..`145` dan dokumen; tidak disentuh |

---

## 1. Masalah yang Diperbaiki

Sesudah `BE-FIN-142` PO dan GR dapat bertanda `BLM`, tetapi layanan GR belum membedakannya:

1. **Petugas Quilvian bisa mengubah dokumen yang pemiliknya BLM.** Contoh: GR-2026-1103 diterima di BLM (80 baik, 15 menunggu periksa, 5 rusak). Bila petugas mengeklik "Batalkan" di Quilvian, GR itu berstatus `CANCELLED` di Quilvian tetapi tetap berlaku di BLM, dan kedua sistem berselisih (`FIN-DEC-262`: pembatalan harus dilakukan di BLM lalu dikonfirmasi ke Quilvian).
2. **Petugas bisa mencatat GR baru untuk PO BLM** di Quilvian, yang tidak punya jejak di BLM.
3. **Status PO BLM bisa dihitung dengan rumus lokal.** Rumus lama menjumlah `ReceivedQuantity` mentah lintas item (lihat `BE-FIN-145`), yang salah untuk satuan berbeda dan kondisi barang.

---

## 2. Proses Bisnis

| Skenario | Sebelum | Sesudah |
| --- | --- | --- |
| Catat GR untuk PO lokal | Berhasil | **Sama** |
| Ubah GR lokal | Berhasil | **Sama** |
| Batalkan GR lokal | Berhasil | **Sama** |
| Catat GR untuk PO bertanda `BLM` | Berhasil (salah) | **409**, tidak ada yang tersimpan |
| Ubah GR bertanda `BLM` | Berhasil (salah) | **409**, tidak ada yang berubah |
| Batalkan GR bertanda `BLM` | Berhasil (salah) | **409**, tidak ada yang berubah |
| Status PO lokal | Rumus lama | **Rumus lama, tidak diubah** |
| Status PO `BLM` | Rumus lama (salah) | Tidak pernah dari rumus lama; dihitung `RecomputeExternalPurchaseOrderStatus` lewat `PurchasingReceiptQuantityCalculator` |

**Contoh penolakan.** Petugas menekan "Catat Penerimaan" pada PO "PO-2026-0412" bertanda `BLM`. Jawabannya 409 dengan pesan persis *"Penerimaan barang untuk PO dari BLM dicatat di BLM. Perubahan dan pembatalannya juga dilakukan dari BLM."* dan `errors.code = "PURCHASING_EXTERNAL_DOCUMENT"`. Tidak ada GR tersimpan, status PO tidak berubah, dan permintaan itu **tidak** direkam di ledger idempotensi (ledger hanya menyimpan hasil sukses), sehingga pengulangan dengan kunci yang sama tetap ditolak dengan alasan yang sama.

**Urutan pemeriksaan.**
- **Catat:** dalam kunci PO, sesudah PO dimuat dan sebelum pemeriksaan status serta penulisan apa pun.
- **Ubah:** dalam kunci PO, sesudah GR dimuat dan **sebelum** pemeriksaan `RowVersion`, supaya versi basi tidak menutupi penyebab sebenarnya.
- **Batalkan:** sesudah GR dimuat, **sebelum** transaksi dibuka dan sebelum `RowVersion`.

**Cabang status BLM.** `RecomputeExternalPurchaseOrderStatus(purchaseOrder, goodsReceipts, actorUserId)` mengambil kemajuan dari `CalculateForPurchaseOrder`, hanya menyetel status bila berubah dan hanya untuk PO berstatus `APPROVED`/`PARTIALLY_RECEIVED`/`FULLY_RECEIVED` (sama seperti jalur lokal). Metode ini **belum punya pemanggil**: jalur lokal menolak dokumen BLM sebelum sampai ke penghitungan status, sehingga satu-satunya pemanggil kelak adalah pengolah kiriman BLM (slice integrasi). Rumus lama kini punya pagar di awal: bila disentuh PO `BLM`, ia melempar `InvalidOperationException` (transaksi digulung) alih-alih menulis status yang salah. Pagar ini tidak terjangkau pada jalur normal.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; registry dan kontrak engineering; `contracts/api-contract.md` W.1; `contracts/validation-matrix.md` `FIN-VAL-315`, `316`; `FinanceGoodsReceiptService.cs` (seluruh berkas); `FinanceGoodsReceiptsController.cs`; `PurchasingExceptions.cs`; `Responses/ApiResponse.cs`; seluruh pemanggil `FinanceGoodsReceiptService` (hanya `FinanceGoodsReceiptsController` di kode aplikasi); `PurchasingReceiptQuantityCalculator.cs` (`BE-FIN-145`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/PurchasingExceptions.cs` | **Diperbarui.** + `PurchasingExternalDocumentException` (konstanta `Code = "PURCHASING_EXTERNAL_DOCUMENT"` dan `DefaultMessage` sesuai `FIN-VAL-315`) |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs` | **Diperbarui** (+53 baris, 0 baris dihapus). Penolakan dokumen BLM di `CreateAsync`, `CancelAsync`, `UpdateAsync`; pagar di awal `RecomputePurchaseOrderStatus`; metode baru `RecomputeExternalPurchaseOrderStatus` (internal, statis); helper `IsExternal`. Badan rumus lama **tidak diubah** |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceGoodsReceiptsController.cs` | **Diperbarui.** `Failure` memetakan exception baru ke 409 dengan `errors.code`; `IsHandled` mengenalinya. Tanpa ini exception baru akan lolos sebagai 500 (`_ => throw`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Tiga action GR mendapat jawaban 409 baru untuk dokumen BLM (sesuai `FIN-API-1.13` W.1). **Selisih bentuk respons, lihat 7** |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Atribut akses tidak berubah. Penolakan adalah aturan kelayakan data, bukan izin |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Goods Receipt

Base URL `api/v1/corporate/finance-management/purchasing/goods-receipts`. Hanya jawaban baru yang ditampilkan.

| Method | Path | Kegunaan | Hak akses | Jawaban baru |
| --- | --- | --- | --- | --- |
| `POST` | `/` | Catat penerimaan barang | `FinanceGoodsReceipt : Create` | 409 bila PO bertanda `BLM` |
| `PUT` | `/{id}` | Koreksi penerimaan barang | `FinanceGoodsReceipt : Update` | 409 bila GR bertanda `BLM` |
| `POST` | `/{id}/cancel` | Batalkan penerimaan barang | `FinanceGoodsReceipt : Cancel` | 409 bila GR bertanda `BLM` |

**Contoh respons 409 (bentuk yang benar-benar dihasilkan kode):**

```json
{
  "success": false,
  "statusCode": 409,
  "message": "Penerimaan barang untuk PO dari BLM dicatat di BLM. Perubahan dan pembatalannya juga dilakukan dari BLM.",
  "data": null,
  "errors": { "code": "PURCHASING_EXTERNAL_DOCUMENT" },
  "timestamp": "2026-11-10T10:15:00"
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| QBE preflight / conformance | Lulus | `PASS` | Respons memakai `ApiResponse<T>`; tidak ada hardcode role; tidak ada akses `DbContext` di controller |
| Review scope | Tiga berkas; DTO tidak disentuh | `PASS` | `git status --short` |
| Keseimbangan kurung (tiga berkas) | Seimbang | `PASS` | Skrip statis (bukan kompilasi) |
| W.T3: perilaku GR lama tidak berubah | `FinanceGoodsReceiptService`: **0 baris dihapus**, penambahan hanya pemeriksaan `IsExternal(...)` yang bernilai `false` untuk `QUILVIAN`; badan `RecomputePurchaseOrderStatus` utuh | `PASS` (review diff) | `git diff` |
| W.T13: tiga aksi menolak dokumen BLM sebelum perubahan apa pun | Penolakan berada sebelum `Add`/`SaveChangesAsync` pada ketiga metode | `PASS` (penelusuran kode) | Posisi pemeriksaan pada bagian 2 |
| Pemetaan 409 dan kode | `Failure` dan `IsHandled` mengenali exception baru | `PASS` (review diff) | `FinanceGoodsReceiptsController.cs` |
| Pemanggil lain `FinanceGoodsReceiptService` di kode aplikasi | Hanya satu controller | `PASS` | `grep` |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Eksekusi runtime (W.T3, W.T13) | Belum. Migration sudah diterapkan (laporan pemilik), tetapi W.T3/W.T13 belum dijalankan | `NOT RUN` | Menunggu pengujian pemilik |

Uji manual: `NOT APPLICABLE` pada task ini (dokumen BLM belum dapat ada sebelum migration diterapkan dan integrasi dibangun).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** semua perintah `dotnet`. Tidak ada tes baru dibuat.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga aksi menolak dokumen BLM dengan 409 dan pesan persis `FIN-VAL-315`, tanpa mengubah data | Terpenuhi menurut review; **belum dieksekusi** | Bagian 2; `PurchasingExternalDocumentException.DefaultMessage` memuat pesan persis kontrak |
| 2. `RecomputePurchaseOrderStatus` bercabang BLM vs QUILVIAN | Terpenuhi sebagian: **pagar** pada rumus lama dan **metode BLM terpisah** `RecomputeExternalPurchaseOrderStatus` ada; metode BLM belum punya pemanggil (menunggu pengolah kiriman) | Bagian 2 |
| 3. Tidak ada perubahan perilaku untuk GR lama (W.T3) | Terpenuhi menurut review diff | 0 baris dihapus pada service |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | Terpenuhi (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| DoD: W.T13 pada runtime | **Belum terpenuhi** | Migration sudah diterapkan (laporan pemilik); W.T13 belum dijalankan |

Status akhir **🟡**: build lulus (laporan pemilik), tetapi (1) kriteria 2 baru sebagian (metode BLM belum punya pemanggil), (2) W.T13 dan W.T3 belum dijalankan pada runtime, dan (3) bentuk respons 409 pada kontrak `W.1` sudah diputuskan pemilik pada 10 November 2026 (contoh kontrak dikoreksi, `FIN-DEC-292`), jadi bukan lagi alasan 🟡.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| **Selisih dengan kontrak yang disetujui** | `contracts/api-contract.md` W.1 menulis contoh respons `{ "success": false, "code": "PURCHASING_EXTERNAL_DOCUMENT", "message": ... }`. Bentuk itu **tidak sesuai** `ApiResponse<T>` yang ada (tidak punya field `code`; field yang tersedia `statusCode`, `message`, `errors`). Kode saya mengikuti `ApiResponse`: `errors.code`. Kontrak **tidak saya ubah** (wewenang builder tidak mencakup redefinisi kontrak yang disetujui). **Diputuskan pemilik 10 November 2026 (`FIN-DEC-292`)**: contoh W.1 dikoreksi menjadi bentuk pada bagian 4; kode tidak diubah. Selisih ini selesai |
| Keputusan implementasi | (1) Exception baru, bukan `PurchasingConflictException`, agar kode mesin dapat dibawa dan tidak tercampur dengan konflik versi (`409` yang sudah ada tanpa kode). (2) Pemeriksaan pada metode Update dan Cancel diletakkan sebelum `RowVersion`. (3) Pagar pada rumus lama melempar `InvalidOperationException` (gagal keras, transaksi digulung) bukan `return` diam-diam |
| Masalah yang diketahui | (1) `RecomputeExternalPurchaseOrderStatus` belum punya pemanggil dan belum dapat dibuktikan end-to-end sampai pengolah kiriman ada. (2) Tombol di FE belum disembunyikan; petugas baru tahu setelah 409 (`FE-FIN-063`) |
| Risiko tersisa | Kompilasi lulus (laporan pemilik). Anomali data (GR lokal yang menunjuk PO bertanda `BLM`) tidak mungkin dibuat lewat jalur resmi karena `Create` kini menolaknya, tetapi bila ada data yang dibuat manual, `CancelAsync`/`UpdateAsync` pada GR itu akan berakhir 500 lewat pagar (aman bagi data) |
| Catatan lain | Repository memuat folder `Tests/QuilvianSystemBackend.PharmacyTests/` yang berisi tes untuk `FinanceGoodsReceiptService` (`FinanceApLifecycleGuardTests`, `FinanceInvoiceExchangeSupplierConsistencyTests`). Tidak dijalankan dan tidak diubah di sini; konstruktor service tidak berubah sehingga tes itu tetap dapat membangun layanan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` tiga berkas source; ditambah pekerjaan `BE-FIN-142`..`145` yang belum di-commit. Tidak ada berkas staged |
| Langkah berikutnya | Setelah migration diterapkan, jalankan W.T3 dan W.T13; `FE-FIN-062`/`063` |
