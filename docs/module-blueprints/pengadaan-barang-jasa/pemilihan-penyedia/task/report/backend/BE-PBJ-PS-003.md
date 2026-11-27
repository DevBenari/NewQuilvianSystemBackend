# Laporan Perubahan Backend — `BE-PBJ-PS-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-003` |
| Judul | Kolom paket pada baris PR (menyentuh `perencanaan-permintaan`) |
| Slice | `MVP-PS0` — kolom baris PR (`04-prd-to-mvp.md` bagian 20; roadmap "Pemetaan ke gelombang MVP") |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-003` |
| Trace | `PBJ-DEC-071`, `PBJ-DEC-095`; `PBJ-PP-INT` 1.2; kamus data pemilihan-penyedia bagian 6 (`PrcPurchaseRequestLine` diperbarui); API §"Perubahan pada sub-modul lain" |
| Contract version | `PBJ-PS-API` 1.0 dan `PBJ-PP-INT` 1.2 (`approved`, `PBJ-DEC-095`) |
| Dependency | `BE-PBJ-PS-001` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), database/migration (2), kontrak API aditif (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-003`, 26 November 2026) |
| Wewenang database | Tiga konfirmasi terpisah dari pemilik, 26 November 2026: membuat migration, menerapkannya ke `QuilvianNewDevSukma`, uji regresi PR di `https://localhost:7185` beserta fixture dan pembersihannya |
| Target tulis | `PrcPurchaseRequestLine` (model + konfigurasi), `PurchaseRequestLineResponse`, proyeksi detail PR di `ProcurementPurchaseRequestService`, migration `AddProcurementPurchaseRequestLinePackage` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `45b22777` (perubahan `BE-PBJ-PS-001`/`002` dan dokumen blueprint belum di-commit), frontend `3e5386679` |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI.** Dua kolom nullable dan index terpasang di `QuilvianNewDevSukma`; detail PR membawa `packageId`; regresi PR 10/10 `PASS` (satu ekspektasi skrip dikoreksi); QBE Strict `PASS`; model tanpa perubahan tertunda |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Request` (milik `perencanaan-permintaan`), diubah atas nama `pemilihan-penyedia` (`PBJ-DEC-095`; urutan migration 9 milik `pemilihan-penyedia` di `02-module-map.md`) |
| Registry | `ProcurementManagement / Request / Permintaan Pengadaan · Prc · ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk model, konfigurasi, DTO, dan service PR; `NEW CODE` untuk migration |
| Arketipe endpoint | Tidak ada endpoint baru; `GET /purchase-requests/{id}` mendapat field aditif |
| QBE yang berlaku | `QBE-ENT-002`, `QBE-CFG-001`, `QBE-CFG-002`, `QBE-DTO-001`, `QBE-API-001` (perubahan aditif) |
| Hasil checker | QBE Strict `PASS` (7 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Paket pengadaan dibentuk per **baris** PR (`PBJ-DEC-071`), tetapi baris PR belum punya tempat untuk mencatat paket yang
memuatnya. Penanda yang ada, `PrcPurchaseRequest.ConvertedPackageId`, berada di tingkat kepala PR. Akibatnya satu PR hanya
bisa masuk satu paket. Padahal PR berisi kursi roda dan tongkat bisa masuk dua paket berbeda. Task ini menambah kolom
`PackageId` dan `PackagedAt` pada setiap baris PR, menampilkannya di detail PR, dan memastikan alur PR yang sudah berjalan
tidak berubah.

## 2. Proses bisnis

| Langkah | Sebelum task | Sesudah task |
| --- | --- | --- |
| Unit mengajukan PR, Tim Pengadaan memverifikasi | Tidak berubah | Tidak berubah; baris PR baru berisi `packageId = null` |
| Tim Pengadaan memasukkan baris PR ke paket | Belum bisa | Kolomnya siap. Penulisnya adalah metode `MarkLinesPackagedAsync` yang dibangun `BE-PBJ-PS-005` |
| Paket batal, baris dilepas | Belum bisa | Kolom dikosongkan lagi oleh `ReleaseLinesAsync` (`BE-PBJ-PS-005`) |
| Unit peminta membuka detail PR | Tidak ada informasi paket | Setiap baris membawa `packageId`; nomor dan status paket dibaca layar dari `/procurement-packages/request-summary` (`BE-PBJ-PS-014`) |

Contoh: PR-000031 baris 1 (10 kursi roda) nanti bernilai `packageId = <PKT-000012>`, sedangkan baris 2 (5 tongkat) bisa
masuk paket lain atau tetap kosong.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PrcPurchaseRequestLine.cs`, `PrcPurchaseRequestLineConfiguration.cs`, `PurchaseRequestDtos.cs`,
`ProcurementPurchaseRequestService.cs` (proyeksi detail), `PurchaseRequestsController.cs`,
`PurchaseRequestVerificationsController.cs`, `PurchaseRequestVerificationDtos.cs`, `UpsertProcurementItemRequest`,
`perencanaan-permintaan/contracts/validation-matrix.md` (`PBJ-VAL-091`), kamus data dan kontrak API pemilihan-penyedia.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Request/Models/PrcPurchaseRequestLine.cs` | Diperbarui | `PackageId` (`Guid?`), `PackagedAt` (`DateTimeOffset?`) |
| `Repositories/Configurations/Corporate/ProcurementManagement/Request/PrcPurchaseRequestLineConfiguration.cs` | Diperbarui | Index `PackageId`; tanpa FK fisik |
| `.../Request/DTOs/PurchaseRequestDtos.cs` | Diperbarui | `PurchaseRequestLineResponse.PackageId` (aditif) |
| `.../Request/Services/ProcurementPurchaseRequestService.cs` | Diperbarui | Proyeksi detail PR mengisi `PackageId` |
| `Migrations/20261126080005_AddProcurementPurchaseRequestLinePackage.cs` + `.Designer.cs` | Baru | 2 `AddColumn` nullable + 1 `CreateIndex`; `Down` menghapus index dan kedua kolom |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | Hasil generator EF |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /purchase-requests/{id}`: setiap baris mendapat `packageId` (aditif, `null` sampai dipaketkan). Tidak ada field yang dihapus atau diganti nama. Frontend PR yang sudah ada tidak terdampak |
| Database | `PrcPurchaseRequestLine.PackageId uuid NULL` + index `IX_PrcPurchaseRequestLine_PackageId`, `PackagedAt timestamptz NULL`. Aman tanpa henti layanan, dan baris lama tetap kosong. **Diterapkan** ke `QuilvianNewDevSukma` 26 November 2026; lingkungan lain belum |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada izin baru. `packageId` bukan data harga |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Kamus data bagian 6 dan arsitektur §4.1: `PackageId uuid NULL` dengan index | Sama — tanpa FK fisik, hanya index (bukan selisih; dicatat sebagai penegasan) | Tabel `PrcPackage` baru lahir di `BE-PBJ-PS-004`; pola sama dengan `NeedPlanLineKey`. Integritas dijaga service pemilik (`MarkLinesPackagedAsync`/`ReleaseLinesAsync`) |
| D-2 | — | `PackagedAt` tidak ditampilkan di respons detail PR | Kontrak API hanya menyebut `packageId`; waktu dipaketkan dibaca dari riwayat paket |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Request / Purchase Request

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/procurement-management/purchase-requests/{id}` | Detail PR; setiap baris kini membawa `packageId` (aditif) | `ProcurementRequest : Read` |

Contoh potongan respons baris: `{ "lineNo": 1, "itemName": "Kursi Roda Standar", "purchaseQuantity": 10, "packageId": null }`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release` | 0 error, 620 warning; **0** dari berkas yang berubah | `PASS` | Log build disaring per berkas |
| `dotnet ef migrations list` | 0 `(Pending)` | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddProcurementPurchaseRequestLinePackage` | Hanya 2 `AddColumn` + 1 `CreateIndex` pada `PrcPurchaseRequestLine`; **tanpa drift** tabel lain (snapshot sudah dipulihkan `BE-PBJ-PS-001` D-9) | `PASS` | Pemeriksaan berkas migration |
| `dotnet ef database update AddProcurementPurchaseRequestLinePackage` | "Applying migration '20261126080005_AddProcurementPurchaseRequestLinePackage'. Done." | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| Skema (`information_schema`, `pg_indexes`, `pg_constraint`) | `PackageId uuid` NULL, `PackagedAt timestamp with time zone` NULL, index btree `PackageId`, 0 FK pada `PackageId`; 53 baris PR lama, 0 berisi paket | `PASS` | Keluaran `SELECT` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <7 berkas>` | VIOLATION 0, REVIEW 0, INFO 0; `PASS` | `PASS` | Laporan QBE |
| Regresi PR (bagian 5.1) | 10/10 | `PASS` | Bagian 5.1 |
| Pembersihan fixture | PR uji, barang uji, 8 policy, 2 employee, 2 jabatan, 1 unit dihapus lunak; kode unit `-DEL-`; 2 akun nonaktif + lockout | `PASS` | `SELECT` sesudahnya: 0 PR hidup, 0 fixture hidup, 0 akun aktif, 0 baris berpaket |
| `dotnet test` | Tidak ada project test | `NOT RUN` | — |

Uji manual: `PASS` (uji API dengan aktor non-SuperAdmin).

**Tidak dijalankan:** detail PR yang dibuat sebelum migration (`R-00`). Di `QuilvianNewDevSukma`, ke-40 PR lama sudah dihapus
lunak oleh pembersihan uji sebelumnya, jadi tidak ada PR lama yang hidup untuk dibuka. Bukti data lama diambil dari basis
data: 53 baris PR lama, seluruhnya `PackageId`/`PackagedAt` kosong.

### 5.1 Regresi alur PR

Build Release di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Aktor dibuat lewat API SuperAdmin di unit
`TEST-PBJ Pengadaan Uji PS003`: **pemohon** (`ProcurementRequest : Read, Create, Update, Submit, Cancel`) dan
**verifikator** (`ProcurementRequest : Read`; `ProcurementRequestVerification : Read, Verify`). Barang katalog uji dibuat
karena katalog dev tidak punya barang aktif.

| No | Skenario | Diharapkan | Hasil |
| --- | --- | --- | --- |
| R-01 | Pemohon membuat PR draf 10 unit | 200 `DRAFT`, baris `packageId = null` | `PASS` |
| R-02 | Detail PR draf | Field `packageId` ada di setiap baris | `PASS` |
| R-03 | Ubah draf menjadi 12 unit | 200 | `PASS` |
| R-04 | Ajukan PR (dengan `Idempotency-Key`) | 200 `SUBMITTED`, bernomor | `PASS` (`PR-000018`) |
| R-05 | Verifikator mencatat cek stok (tersedia 2, dipenuhi 2) | 200 | `PASS` |
| R-06 | Kebutuhan beli = 12 − 2 | `purchaseQuantity` 10, `packageId` tetap `null` | `PASS` |
| R-07 | Selesaikan verifikasi selagi definisi `PBJ-PR` tidak aktif | Ditolak `PBJ-VAL-091` (fail-closed), PR tetap `SUBMITTED` | `PASS` — 409 `PBJ-VAL-091`. Skrip semula mengharapkan 422; `PBJ-PP-VAL` menetapkan **409** untuk kode ini saat menyelesaikan verifikasi, jadi ekspektasinya yang dikoreksi |
| R-08 | Pemohon membatalkan PR | 200 `CANCELLED` | `PASS` |
| R-09 | Daftar PR memuat PR uji | 200 | `PASS` |
| R-10 | Basis data: baris PR uji | `PackageId`, `PackagedAt` kosong | `PASS` |

10 skenario, 10 `PASS`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Dua kolom nullable dan index cocok kamus data; baris PR lama kosong | Terpenuhi | Bagian 5: skema dan 53 baris lama |
| (2) Detail PR membawa `packageId: null` | Terpenuhi | `R-01`, `R-02`, `R-06` |
| (3) Regresi PR: buat, ajukan, verifikasi, batal tetap berjalan | Terpenuhi | `R-01`..`R-10`; verifikasi sampai cek stok, lalu ditolak fail-closed karena matriks `PBJ-PR` belum diisi (K-1 `perencanaan-permintaan`), sama dengan perilaku sebelum task |
| (4) Migration dapat dijalankan tanpa henti layanan; `Down` menghapus kolom | Terpenuhi | Isi migration: dua kolom nullable + index; `Down` `DropIndex` + 2 `DropColumn` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kolom belum punya penulis sampai `BE-PBJ-PS-005`. `PrcPurchaseRequest.ConvertedPackageId` dibiarkan dan tidak dipakai lagi (arsitektur §12) |
| Masalah yang diketahui | **Selisih kontrak lintas sub-modul.** `PBJ-PP-VAL` memberi `PBJ-VAL-091` status **409**, sedangkan `PBJ-PS-VAL`/`AT-PS-14` memakai **422** untuk kode yang sama saat mengajukan persiapan paket. Perlu diputuskan pemilik desain sebelum `BE-PBJ-PS-008`, supaya satu kode punya satu status. `ProcurementStatuses.PurchaseRequest.Final` masih memuat `CONVERTED_TO_PACKAGE` (lihat `BE-PBJ-PS-001`); ditangani `BE-PBJ-PS-005` |
| Risiko tersisa | Tanpa FK fisik, `PackageId` bisa menunjuk paket yang tidak ada bila ada penulis selain service pemilik. Mitigasinya: penulisan hanya lewat `MarkLinesPackagedAsync`/`ReleaseLinesAsync` (`PBJ-PP-INT` 1.2) |
| Perubahan sampingan | `NONE` — generator EF hanya menambah migration baru dan memperbarui snapshot untuk dua kolom ini |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: migration + Designer, laporan ini. Diubah: `PrcPurchaseRequestLine.cs`, `PrcPurchaseRequestLineConfiguration.cs`, `PurchaseRequestDtos.cs`, `ProcurementPurchaseRequestService.cs`, `ApplicationDbContextModelSnapshot.cs`, roadmap, traceability, `MODULE-STATUS.md`. Ditambah perubahan `BE-PBJ-PS-001`/`002` dan dokumen blueprint yang belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-004` (skema 14 tabel Sourcing, migration `AddProcurementSourcing`) — gelombang 3; `FE-PBJ-PS-001` boleh paralel |
