# Laporan Perubahan Backend — `BE-PBJ-PS-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-005` |
| Judul | Paket draf dan tiga metode baris PR (menyentuh `perencanaan-permintaan`) |
| Slice | `MVP-PS1` — `PS-02` bagian draf; tiga metode baris PR (`04-prd-to-mvp.md` bagian 20; roadmap "Pemetaan ke gelombang MVP") |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-005`, gelombang 4 |
| Trace | `PBJ-DEC-020`, `022`, `071`, `077`, `079`, `082`, `090`, `091`, `095`; FR-PS-010, 011; `PBJ-PP-INT` 1.2 §2; `PBJ-PS-VAL` `PBJ-VAL-090`, `120`..`123`, `128`; `AT-PS-01`..`06` |
| Contract version | Kontrak `PBJ-PS-*` set `1.0` (`approved`, `PBJ-DEC-095`) |
| Dependency | `BE-PBJ-PS-004` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika tinggi (2), kontrak API baru (1), menyentuh service sub-modul lain (1), keamanan harga dan cakupan unit (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-005`, 27 November 2026) |
| Wewenang database | Tanpa migration (kolom dan tabel sudah ada sejak `BE-PBJ-PS-003`/`004`). Uji runtime dan SQL langsung (konfigurasi `Wfl` uji, pembersihan) atas konfirmasi terpisah pemilik, 27 November 2026: "Ya, jalankan dan bersihkan" |
| Target tulis | `Sourcing/Controllers`, `Sourcing/DTOs`, `Sourcing/Services`, `Sourcing/ProcurementSourcingExplicitPermissions.cs`, DI; `Request/Services/ProcurementPurchaseRequestService.cs`, `ProcurementPurchaseRequestVerificationService.cs`, `Request/DTOs/PurchaseRequestPackagingDtos.cs`; konstanta `Common` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `de41c752` (bersih sebelum task), frontend `sukmagpV2` @ `3e5386679` |
| Tanggal | 27 November 2026 |
| Status | ✅ **SELESAI.** Sembilan endpoint paket draf dan tiga metode baris PR berjalan; build Release 0 error; QBE Strict `PASS` 11 berkas; uji runtime 32/33 cek `PASS` di putaran akhir, satu-satunya `FAIL` (`T-23`, prasyarat data kosong) dibuktikan ulang lewat uji susulan `T-23b` `PASS`; fixture dan konfigurasi `Wfl` uji dibersihkan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Sourcing` (controller, DTO, service baru); `Request` (tiga metode baru di service yang sudah ada); `Common` (konstanta) |
| Registry | `Corporate / Procurement · ProcurementManagement / Sourcing / Pemilihan Penyedia · BUSINESS DOMAIN / MODULE · Prc · ACTIVE` (`PBJ-DEC-096`); `Request` dan `Common` memakai baris `Prc` yang sudah `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk seluruh berkas `Sourcing` dan `PurchaseRequestPackagingDtos.cs`; `TOUCHED LEGACY` untuk `ProcurementPurchaseRequestService.cs`, `ProcurementPurchaseRequestVerificationService.cs`, `ProcurementStatuses.cs`, `ProcurementDocumentHistoryActions.cs`, `ProcurementManagementServiceCollectionExtensions.cs` |
| Arketipe endpoint | Transaksi — aggregate ber-lifecycle (`transaction-endpoint-standard.md`): `filters/metadata`, `summary`, daftar, detail, `POST /`, aksi sub-resource, riwayat. Tanpa `GET /options`, `PATCH /{id}/status`, `DELETE /{id}` |
| Akses | `[AccessController(SourcingModuleCode, …, ControllerName = "ProcurementPackage")]`; setiap action punya `[AccessAction]` dan `[AccessPermission]` yang argumennya sama persis; tanpa role hardcode. `ReadAllUnits` dan `ReadPrice` adalah penanda data (`AccessExplicitPermission`), ditanyakan lewat `ProcurementDataScopeService` dan `AccessPermissionService` |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-001..003`, `QBE-SVC-001` (controller hanya meneruskan ke service), `QBE-CODE-004`, `QBE-DEL-001` (hapus lunak) |
| Hasil checker | QBE Strict `PASS` (11 berkas, 0 temuan), dijalankan ulang sesudah perbaikan putaran 1 |

## 1. Masalah yang diselesaikan

Tim Pengadaan belum bisa membentuk paket dari PR yang sudah disetujui. Task ini membangun langkah pertama itu: memilih baris
PR siap paket, menggabungkannya per barang katalog, menambah atau melepas baris selama paket masih draf, dan membaca
riwayatnya. Contoh dari kriteria `AT-PS-01`: PR IGD berisi 10 kursi roda (harga perkiraan Rp 2.800.000) dan PR Rawat Inap
berisi 5 kursi roda (Rp 3.100.000) menjadi satu barang paket 15 unit dengan HPS satuan Rp 2.900.000. Paket itu tetap
menyimpan dua sumber, masing-masing dengan unit dan sumber dananya, supaya komitmen pagu kelak dapat dicatat per unit.

Di sisi `perencanaan-permintaan`, PR kini tahu barisnya masuk paket. PR menjadi "Masuk Paket" (`CONVERTED_TO_PACKAGE`) hanya
bila semua baris belinya sudah berpaket, dan kembali "Disetujui" bila salah satu barisnya dilepas (`PBJ-DEC-077`).

## 2. Proses bisnis yang dilayani

| Langkah | Pelaku | Endpoint | Hasil |
| --- | --- | --- | --- |
| Melihat baris PR yang siap dipaketkan | Tim Pengadaan (semua unit) atau petugas unit (unit sendiri) | `GET /procurement-packages/packageable-request-lines` | Baris PR `APPROVED`, jumlah beli > 0, belum berpaket |
| Membuat paket draf | Tim Pengadaan | `POST /procurement-packages` | Paket `DRAFT` tanpa nomor; barang digabung per kode katalog; PIC = pembuat |
| Menambah baris PR | Tim Pengadaan | `POST /procurement-packages/{id}/lines` | Barang yang sama bertambah kuantitas; barang baru mendapat nomor urut berikutnya |
| Melepas satu baris PR | Tim Pengadaan | `DELETE /procurement-packages/{id}/sources/{sourceId}` | Sumber ditandai dilepas; baris PR siap dipaketkan lagi; PR kembali `APPROVED` bila tadinya `CONVERTED_TO_PACKAGE` |
| Membaca daftar, ringkasan, detail, riwayat | Pemegang `ProcurementPackage : Read` | `GET /`, `/summary`, `/{id}`, `/{id}/history` | Harga `null` tanpa `ReadPrice`; paket di luar cakupan 404 |

Aturan yang dijaga service (kode `PBJ-PS-VAL` 1.0):

| Aturan | Bila dilanggar | Contoh |
| --- | --- | --- |
| Baris PR harus `APPROVED` dan punya jumlah beli | 409 `PBJ-VAL-120` | Baris PR yang masih `SUBMITTED` |
| Satu baris PR hanya di satu paket hidup | 409 `PBJ-VAL-121` | "Baris 1 permintaan PR-000024 sudah masuk paket TEST-PBJ PS005 Kursi Roda 2027." |
| Satu jenis pengadaan per paket (`PBJ-DEC-090`) | 422 `PBJ-VAL-122` | Kursi roda (Barang Stok) dicampur servis alat (jenis lain) |
| Satu tahun anggaran per paket (`PBJ-DEC-091`) | 422 `PBJ-VAL-123` | PR 2027 dicampur PR 2028 |
| Tambah/lepas hanya saat `DRAFT`/`PREPARATION_RETURNED` | 409 `PBJ-VAL-128` | — |
| Versi data harus cocok | 409 `PBJ-VAL-090` | Dua petugas menyunting paket bersamaan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task dan tabel roadmap; `api-contract.md`, `state-transition-matrix.md` bagian 1, `validation-matrix.md`,
`permission-audit-matrix.md` bagian 1–6, `integration-contract.md`, `acceptance-test-matrix.md`; kamus data
`PrcPackage*`; arsitektur §3; `00-interview-decisions.md` `PBJ-DEC-028`/`082`. Source: model `PrcPackage`,
`PrcPackageItem`, `PrcPackageItemSource`, `PrcTechnicalCriterion`, `PrcPurchaseRequestLine`, `PrcPackagePlan`; konfigurasi
EF Sourcing (index dan CHECK); `ProcurementPurchaseRequestService`, `ProcurementPurchaseRequestVerificationService`,
`ProcurementRequestExceptions`, `PurchaseRequestsController`, `PurchaseRequestVerificationsController`;
`VendorEvaluationsController`/`ProcurementVendorEvaluationService` (pola 201, `SaveAsync`, pelanggaran unik);
`ProcurementDataScopeService`, `ProcurementDocumentHistoryWriter`, `ProcurementPeriodFilter`, `DocumentHistoryDtos`;
`ProcurementRequestExplicitPermissions.cs`; `ProcurementPackagePlanService` (pembuatan draf tanpa baris).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Sourcing/Controllers/ProcurementPackagesController.cs` | Baru | Sembilan endpoint (bagian 4); pemetaan pengecualian Sourcing dan Request ke 400/404/409/422 |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackageService.cs` | Baru | Baca, buat, tambah baris, lepas baris; cakupan unit; penutup harga; transaksi; riwayat `PBJ_PACKAGE`; audit tanpa harga |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementSourcingExceptions.cs` | Baru | `ProcurementSourcingValidationException` (422), `ProcurementSourcingConflictException` (409), `ProcurementSourcingBadRequestException` (400) |
| `Areas/Corporate/ProcurementManagement/Sourcing/DTOs/PackageDtos.cs` | Baru | `PackageQuery`, `PackageFilterMetadataResponse`, `PackageSummaryResponse`, `PackageListItem`, `CreatePackageRequest`, `AddPackageLinesRequest`, `RemovePackageSourceQuery`, `PackageResponse`, `PackageItemResponse`, `PackageItemSourceResponse` |
| `Areas/Corporate/ProcurementManagement/Sourcing/ProcurementSourcingExplicitPermissions.cs` | Baru | Penanda `ProcurementPackage : ReadAllUnits` dan `ReadPrice` (dipindah dari `BE-PBJ-PS-004` D-2) |
| `Areas/Corporate/ProcurementManagement/ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | `AddScoped<ProcurementPackageService>()` |
| `Areas/Corporate/ProcurementManagement/Request/DTOs/PurchaseRequestPackagingDtos.cs` | Baru | `PackageableRequestLineQuery`, `PackageableRequestLineResponse` (`PBJ-PP-INT` 1.2 §2) |
| `Areas/Corporate/ProcurementManagement/Request/Services/ProcurementPurchaseRequestService.cs` | Diperbarui | `GetPackageableLinesAsync`, `MarkLinesPackagedAsync`, `ReleaseLinesAsync` — ikut transaksi pemanggil, tidak memanggil `SaveChanges` |
| `Areas/Corporate/ProcurementManagement/Request/Services/ProcurementPurchaseRequestVerificationService.cs` | Diperbarui | `CancelApprovedAsync` menolak PR yang **salah satu** barisnya berpaket (409 `PBJ-VAL-092`), bukan hanya PR `CONVERTED_TO_PACKAGE` |
| `Areas/Corporate/ProcurementManagement/Common/Constants/ProcurementStatuses.cs` | Diperbarui | `PurchaseRequest.Final` tidak lagi memuat `CONVERTED_TO_PACKAGE` (`PBJ-DEC-077`) |
| `Areas/Corporate/ProcurementManagement/Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `ReleaseFromPackage = "RELEASE_FROM_PACKAGE"` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sembilan endpoint Sourcing baru (delapan sesuai kontrak, `GET /summary` tambahan — D-1). Perilaku PR berubah aditif: baris berpaket mengunci pembatalan PR (D-12), dan `RowVersion` PR berganti saat barisnya dipaketkan atau dilepas (D-10) |
| Database | Tanpa migration dan tanpa perubahan skema. Menulis `PrcPackage`, `PrcPackageItem`, `PrcPackageItemSource`, `PrcTechnicalCriterion` (hapus lunak saat barang terlepas), `PrcPurchaseRequestLine.PackageId`/`PackagedAt`, `PrcPurchaseRequest.Status`/`RowVersion`, `PrcDocumentHistory` |
| Keamanan/Auth | Cakupan unit lewat `ReadAllUnits` (404 di luar cakupan, termasuk baris PR unit lain saat membuat paket). Harga ditutup di satu titik (`MaskPrices`) bagi pengguna tanpa `ReadPrice`; urut "Nilai HPS" diabaikan bagi mereka (D-16). Payload logger dan cuplikan riwayat tidak memuat harga (`PBJ-PS-PERM` bagian 6) |
| Transaksi | Buat, tambah, lepas: satu transaksi `BeginTransactionAsync`; metode PR ikut transaksi itu lewat `DbContext` yang sama. Pelanggaran index unik sumber paket → 409 `PBJ-VAL-121`; konkurensi `RowVersion` → 409 `PBJ-VAL-090` |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Kontrak tidak memuat `GET /summary` | `GET /procurement-packages/summary` (total dan jumlah per status, dalam cakupan) | Standar endpoint transaksi; pola `PurchaseRequestsController`. Permukaan teknis, bukan kebijakan baru |
| D-2 | Kontrak: riwayat `PagedResult<DocumentHistoryItem>` | `ApiResponse<List<DocumentHistoryResponse>>` | Bentuk riwayat yang sudah dipakai PR dan seluruh Vendor (`PBJ-VD-API` menulis `List<DocumentHistoryResponse>`); FE memakai satu komponen riwayat |
| D-3 | Kamus data: `HpsUnitPrice` bawaan "harga perkiraan PR" | Rata-rata tertimbang kuantitas dari harga perkiraan baris PR, dibulatkan 2 desimal. Tambah baris memadukan HPS satuan saat ini dengan harga baris baru; lepas baris **tidak** menghitung ulang HPS satuan | Beberapa baris PR dengan harga berbeda tidak punya satu "harga PR". Hasil tambah baris sama dengan membuat paket sekaligus. Lepas baris tidak menimpa HPS yang mungkin sudah disunting Tim Pengadaan (`PUT /{id}`, `BE-PBJ-PS-007`) |
| D-4 | — | `Specification` barang = gabungan spesifikasi berbeda dari baris PR, dipisah "; ", dipotong 2000 karakter | Tidak ada spesifikasi yang hilang; Tim Pengadaan merapikannya di persiapan |
| D-5 | `PBJ-DEC-020`: cakupan unit | Tanpa `ReadAllUnits`: paket yang memuat baris PR (belum dilepas) dari unit penempatan pengguna, **atau** paket yang PIC-nya pengguna itu | PIC tidak kehilangan paketnya sendiri; ini kepemilikan data, bukan role hardcode |
| D-6 | — | Baris PR di luar cakupan saat membuat/menambah → 404, sama dengan baris yang tidak ada | Pola 404 di luar cakupan (keberadaan data tidak bocor) |
| D-7 | `PBJ-DEC-082`: `PackagePlanId` rujukan | Hanya diperiksa ada dan tidak terhapus (400 bila tidak); status, tahun, dan jenis rencana tidak dicocokkan | Aturan kecocokan rencana belum diputuskan; tidak dikarang |
| D-8 | Class diagram: paket berisi 1..* barang | Melepas baris terakhir → 400 "Batalkan paket bila seluruh barisnya tidak jadi diproses" | Pembatalan paket adalah aksi tersendiri (`BE-PBJ-PS-015`) |
| D-9 | Kontrak: request `expectedRowVersion` pada `DELETE` | Dikirim sebagai query string (`?expectedRowVersion=…`) | `DELETE` tanpa body |
| D-10 | — | `MarkLinesPackagedAsync` dan `ReleaseLinesAsync` mengganti `RowVersion` PR yang barisnya berubah | Kunci optimistis: pembatalan PR yang berjalan bersamaan dengan pemaketan tidak lolos keduanya. Akibatnya layar PR yang terbuka mendapat 409 `PBJ-VAL-090` bila barisnya baru dipaketkan; muat ulang menyelesaikannya |
| D-11 | `ProcurementStatuses.PurchaseRequest.Final` memuat `CONVERTED_TO_PACKAGE` | Dihapus dari himpunan itu | `PBJ-DEC-077` mengembalikan PR ke `APPROVED`. Himpunan ini tidak dipakai kode mana pun (dicari dengan `rg`), jadi tanpa efek runtime |
| D-12 | Penjaga lama `PBJ-VAL-092` hanya untuk `CONVERTED_TO_PACKAGE` | Juga untuk PR `APPROVED` yang sebagian barisnya berpaket | Paket per baris (`PBJ-DEC-071`) membuat PR berpaket sebagian tetap `APPROVED`; tanpa penjaga ini sumber paket kehilangan barisnya |
| D-13 | Pesan `PBJ-VAL-121` menyebut "{nomor}" | Nomor paket, atau judul bila paket masih draf tanpa nomor | Nomor `PKT` baru lahir saat persiapan diajukan (`BE-PBJ-PS-008`) |
| D-14 | — | Label status paket berbahasa Indonesia di `filters/metadata` (mis. "Persiapan Dikembalikan", "Siap RFQ") | Kontrak tidak memuat label; FE boleh mengganti |
| D-15 | — | Penanda izin `SortOrder` 10 dan 11 | Urutan tampil di layar Akses Role, sesudah aksi endpoint |
| D-16 | — | Urut `hpsTotal` hanya berlaku bagi pemegang `ReadPrice`; selain itu kembali ke tanggal dibuat | Urutan nilai membocorkan peringkat harga |
| D-17 | — | Controller Sourcing juga memetakan `ProcurementRequestConflictException` (409) | Metode baris PR milik `perencanaan-permintaan` melempar pengecualian Request; jaring pengaman di belakang validasi Sourcing |

## 4. Dokumentasi endpoint

### Corporate / Procurement Management / Sourcing / Procurement Package

Base URL: `api/v1/corporate/procurement-management/procurement-packages` — **tersedia** sejak task ini.

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Status (11) beserta label, jenis pengadaan aktif, tahun, periode, urutan | `ProcurementPackage : Read` | — | `ApiResponse<PackageFilterMetadataResponse>` | 200 |
| `GET` | `/summary` | Total dan jumlah per status dalam cakupan (D-1) | `ProcurementPackage : Read` | — | `ApiResponse<PackageSummaryResponse>` | 200 |
| `GET` | `/` | Daftar paket: cari nomor/judul, status, jenis, tahun, `startDate`/`endDate`/`customPeriod`, urut, halaman | `ProcurementPackage : Read` | `PackageQuery` | `ApiResponse<PagedResult<PackageListItem>>` | 200, 422 (`PBJ-VAL-094`) |
| `GET` | `/packageable-request-lines` | Baris PR `APPROVED` belum berpaket: tahun, jenis, kategori, unit, cari | `ProcurementPackage : Create` | `PackageableRequestLineQuery` | `ApiResponse<PagedResult<PackageableRequestLineResponse>>` | 200 |
| `POST` | `/` | Buat paket draf dari baris PR | `ProcurementPackage : Create` | `CreatePackageRequest` | `ApiResponse<PackageResponse>` | 201, 400, 404, 409 (`120`, `121`), 422 (`122`, `123`) |
| `GET` | `/{id}` | Detail paket, barang, sumber | `ProcurementPackage : Read` | — | `ApiResponse<PackageResponse>` | 200, 404 |
| `GET` | `/{id}/history` | Riwayat `PBJ_PACKAGE` (D-2) | `ProcurementPackage : Read` | — | `ApiResponse<List<DocumentHistoryResponse>>` | 200, 404 |
| `POST` | `/{id}/lines` | Tambah baris PR | `ProcurementPackage : Update` | `AddPackageLinesRequest` | `ApiResponse<PackageResponse>` | 200, 400, 404, 409 (`090`, `120`, `121`, `128`), 422 (`122`, `123`) |
| `DELETE` | `/{id}/sources/{sourceId}` | Lepas satu baris PR | `ProcurementPackage : Update` | `?expectedRowVersion=` | `ApiResponse<PackageResponse>` | 200, 400, 404, 409 (`090`, `128`) |

Contoh `POST /`:

```json
{ "title": "Kursi Roda 2027", "packagePlanId": null,
  "purchaseRequestLineIds": ["<baris PR IGD>", "<baris PR Rawat Inap>"] }
```

Respons ringkas (pemegang `ReadPrice`): `status: "DRAFT"`, `packageNumber: null`, `hpsTotal: 43500000.00`,
`items[0]: { quantity: 15, hpsUnitPrice: 2900000.00, sources: [IGD 10, Rawat Inap 5] }`. Tanpa `ReadPrice`:
`hpsTotal`, `approvedHpsTotal`, `hpsUnitPrice`, `hpsAmount`, dan `estimatedUnitPrice` bernilai `null`, `canViewPrice: false`.

Penanda data (bukan endpoint): `ProcurementPackage : ReadAllUnits` (cakupan semua unit) dan `ProcurementPackage : ReadPrice`
(harga terlihat), keduanya muncul di layar Akses Role modul `CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release --no-incremental` (sesudah perbaikan putaran 1) | 0 error, 620 warning (= baseline `BE-PBJ-PS-003`/`004`); 0 warning dari berkas yang diubah | `PASS` | Log build scratchpad sesi |
| QBE Strict, 11 berkas (dijalankan ulang sesudah perbaikan) | 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO` | `PASS` | Keluaran checker |
| Uji runtime putaran 1 (`https://localhost:7185` → `QuilvianNewDevSukma`) | Berhenti di `T-18`: lepas baris terakhir sebuah barang → 409 generik karena `CK_PrcPackageItem_Quantity` (`Quantity > 0`); kode mengurangi kuantitas menjadi 0 sebelum menghapus lunak barangnya | `FAIL` → diperbaiki | Log server: `23514 … violates check constraint "CK_PrcPackageItem_Quantity"`. Perbaikan: kuantitas hanya dikurangi bila barang masih bersumber. Fixture putaran 1 dibersihkan |
| Uji runtime putaran 2 (fixture baru, build baru) | 31/32 `PASS`; `T-23` `FAIL` karena tabel `PrcPackagePlan` di DB uji kosong (`plan: null`), bukan karena kode | `PASS` dengan satu prasyarat data | `ps005.results.json` scratchpad sesi |
| Uji susulan `T-23b` (rencana paket draf uji dibuat lewat API SuperAdmin) | `PASS` | `PASS` | Bagian 5.1 |
| Pembersihan fixture dan konfigurasi `Wfl` uji | Hapus lunak dalam satu transaksi; `PBJ-PR` kembali `IsActive = false`, `Draft`, 0 baris matriks uji hidup | `PASS` | Keluaran `cleanup_ps005.py apply` (bagian 5.2) |

**Fixture uji (`TEST-PBJ PS005`, lewat API SuperAdmin).** Tiga unit (IGD, Rawat Inap, Pengadaan), enam akun `Employee`
tanpa role: pemohon IGD, pemohon Rawat Inap, staf IGD (`ProcurementPackage : Read, Create, Update` saja), verifikator,
penyetuju (`ApprovalInbox : Read, Approve`), Tim Pengadaan (`ProcurementPackage : Read, Create, Update, ReadAllUnits,
ReadPrice`). Tiga barang katalog uji (kursi roda, tongkat, servis alat berjenis lain). Lima PR: A (IGD 2027, kursi 10 @
Rp 2.800.000 + tongkat 3 @ Rp 150.000), B (Rawat Inap 2027, kursi 5 @ Rp 3.100.000), C (IGD 2028), D (IGD 2027, servis
alat), E (IGD 2027, dibiarkan `SUBMITTED`). PR A–D dibawa ke `APPROVED` lewat alur sebenarnya: ajukan → cek stok →
selesaikan verifikasi → kotak masuk `Wfl`. Untuk itu definisi `PBJ-PR` diaktifkan sementara dengan satu baris
`MstApprovalMatrix` fallback `SpecificUser` per langkah (4 langkah), bertanda `DATA UJI PBJ PS005 (bukan nilai SOP)` (pola
`BE-PBJ-VD-007`), lalu dicabut.

### 5.1 Skenario runtime

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `F-00` | Penanda `ProcurementPackage : ReadAllUnits` dan `ReadPrice` terdaftar di registry Akses Role saat start | `PASS` | Kedua aksi ada di `SysActionAccess` |
| `F-01` | Regresi PR: empat PR `APPROVED` lewat `Wfl` uji; PR-E tetap `SUBMITTED` | `PASS` | PR-000024, PR-000025, … |
| `T-00` | `filters/metadata` | `PASS` | 200; 11 status |
| `T-01` | Ringkasan 200 bagi Tim Pengadaan; pemohon tanpa izin paket → 403 di daftar dan baris siap paket | `PASS` | 200 / 403 / 403 |
| `T-02` | Baris siap paket (Tim Pengadaan): A1, A2, B1, C1, D1 tampil; E1 (PR `SUBMITTED`) tidak; harga terisi | `PASS` | 5 baris; A1 Rp 2.800.000 |
| `T-03` | `AT-PS-06` (baris): staf IGD hanya melihat baris IGD; harga `null` | `PASS` | 4 baris; B1 tidak ada |
| `T-04` | Judul kosong / tanpa baris → 400 | `PASS` | 400 / 400 |
| `T-05` | `AT-PS-04` (buat): jenis berbeda 422 `PBJ-VAL-122`; tahun berbeda 422 `PBJ-VAL-123`; tidak ada paket terbentuk | `PASS` | Jumlah paket uji tetap |
| `T-06` | `AT-PS-01`: 201; satu barang 15 unit, dua sumber (10 + 5); HPS satuan 2.900.000; jumlah 43.500.000; `DRAFT`; PIC = pembuat; tanpa nomor | `PASS` | 201 |
| `T-07` | `AT-PS-01` (DB): A1 dan B1 ber-`PackageId` paket baru, `PackagedAt` terisi | `PASS` | Query DB |
| `T-08` | `AT-PS-03` (1): PR-A tetap `APPROVED`; PR-B → `CONVERTED_TO_PACKAGE` dengan riwayat `CONVERT_TO_PACKAGE`; detail PR membawa `packageId` | `PASS` | Riwayat PR-B: `WF_APPROVED`, `CONVERT_TO_PACKAGE` |
| `T-09` | `AT-PS-02`: A1 ke paket kedua → 409 `PBJ-VAL-121`; paket kedua tidak terbentuk | `PASS` | Pesan menyebut PR-000024 dan judul paket |
| `T-10` | Baris siap paket tidak lagi memuat A1 dan B1 | `PASS` | 3 baris |
| `T-11` | Tambah baris dengan `expectedRowVersion` basi → 409 `PBJ-VAL-090` | `PASS` | 409 |
| `T-12` | `AT-PS-04` (tambah): 422 `PBJ-VAL-122` / `PBJ-VAL-123` | `PASS` | 422 / 422 |
| `T-13` | Tambah baris PR `SUBMITTED` → 409 `PBJ-VAL-120` | `PASS` | 409 |
| `T-14` | `AT-PS-03` (2): tambah tongkat → 200; dua barang; HPS total 43.950.000; PR-A → `CONVERTED_TO_PACKAGE` | `PASS` | 200 |
| `T-15` | Staf IGD tanpa `ReadPrice` membuka paket berisi baris IGD → 200; 9 field harga semuanya `null`; `canViewPrice: false` | `PASS` | Respons mentah |
| `T-16` | Daftar paket staf IGD memuat paket itu, `hpsTotal: null`, dua unit | `PASS` | 200 |
| `T-17` | Regresi PR: batalkan PR `CONVERTED_TO_PACKAGE` → 409 `PBJ-VAL-092` | `PASS` | 409 |
| `T-18` | `AT-PS-03` (3): lepas tongkat → 200; barang tongkat hilang; PR-A kembali `APPROVED` dengan riwayat `RELEASE_FROM_PACKAGE`; A2 siap dipaketkan lagi; HPS total 43.500.000 | `PASS` | Riwayat PR-A: `WF_APPROVED`, `CONVERT_TO_PACKAGE`, `RELEASE_FROM_PACKAGE` |
| `T-19` | DB: sumber tongkat `IsReleased` + `ReleasedAt`; barang tongkat dihapus lunak | `PASS` | Query DB |
| `T-20` | Regresi PR (penjaga baru D-12): PR `APPROVED` yang sebagian barisnya berpaket → 409 `PBJ-VAL-092` | `PASS` | 409; PR-A tetap `APPROVED` |
| `T-21` | Lepas B1 → 200; kursi 10 unit, HPS satuan tetap 2.900.000, jumlah 29.000.000; PR-B kembali `APPROVED` | `PASS` | 200 |
| `T-22` | Lepas baris terakhir → 400; sumber tak dikenal → 404 | `PASS` | 400 / 404 |
| `T-23` | `AT-PS-05` memakai rencana paket yang sudah ada di DB; rencana tak dikenal → 400 | `FAIL` | Tabel `PrcPackagePlan` kosong (`plan: null`); paket tetap 201 tanpa rujukan; rencana tak dikenal 400. Dibuktikan ulang di `T-23b` |
| `T-24` | `AT-PS-06` (buat): staf IGD memaketkan baris Rawat Inap → 404 | `PASS` | 404 |
| `T-25` | `AT-PS-06`: staf IGD membuka paket berisi baris Rawat Inap saja → 404 (detail dan riwayat), tidak tampil di daftar; Tim Pengadaan 200 | `PASS` | 404 / 404 / 200 |
| `T-26` | Riwayat paket: `CREATE`, `LINE_ADD`, `LINE_REMOVE`, `LINE_REMOVE` berurutan | `PASS` | 200 |
| `T-27` | Ringkasan staf IGD menghitung paket yang terlihat saja | `PASS` | `total: 1` |
| `T-28` | Regresi PR: PR `APPROVED` tanpa baris berpaket tetap dapat dibatalkan → `CANCELLED` | `PASS` | 200 |
| `T-29` | DB: tidak ada baris PR dengan dua sumber paket hidup | `PASS` | 0 |
| `T-23b` | `AT-PS-05` (susulan): rencana paket draf uji tanpa baris dibuat lewat API SuperAdmin; paket dari baris A2 merujuknya → 201; `PackagePlanId` tersimpan di DB; judul rencana terbaca di detail; isi paket tetap satu baris PR; rencana paket tetap `DRAFT` tanpa baris | `PASS` | 201 |

### 5.2 Pembersihan

| Tabel | Baris (putaran 2) |
| --- | --- |
| `PrcPackageItemSource` / `PrcPackageItem` / `PrcPackage` | 5 / 3 / 3 dihapus lunak |
| `PrcPackagePlan` | 1 dihapus lunak |
| `PrcPurchaseRequestLine` | 6 dilepas dari paket, lalu dihapus lunak |
| `PrcPurchaseRequest` | 5 dihapus lunak |
| `MstProcurementItem` | 3 dihapus lunak |
| `SysAccessPolicy`, `MstEmployee`, `MstPosition`, `MstDepartment` | 25, 6, 6, 3 dihapus lunak; kode unit diberi akhiran `-DEL-<run>` |
| `AspNetUsers` | 6 nonaktif + lockout |
| `MstApprovalMatrix` uji | Dihapus lunak; `PBJ-PR` kembali `IsActive = false`, `Draft` |

Putaran 1 dibersihkan dengan cara yang sama sebelum putaran 2. Yang sengaja tidak dihapus: baris `PrcDocumentHistory`
dan instance `Wfl` milik PR uji (jejak audit, pola pembersihan Pengadaan sebelumnya), serta nomor seri `PR-000019`..`000028`
yang sudah terpakai.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PS-01` 15 kursi roda dari IGD 10 + Rawat Inap 5 | Terpenuhi | `T-06`, `T-07` |
| `AT-PS-02` 409 `PBJ-VAL-121` | Terpenuhi | `T-09`, `T-29` |
| `AT-PS-03` PR dua baris: `APPROVED` → `CONVERTED_TO_PACKAGE`; lepas baris → kembali `APPROVED` | Terpenuhi | `T-08`, `T-14`, `T-18`, `T-21` |
| `AT-PS-04` 422 `PBJ-VAL-122`/`123` | Terpenuhi (buat dan tambah) | `T-05`, `T-12` |
| `AT-PS-05` `PackagePlanId` tersimpan, isi tetap baris PR | Terpenuhi lewat uji susulan | `T-23` `FAIL` (prasyarat data kosong), `T-23b` `PASS` |
| `AT-PS-06` 404 di luar cakupan | Terpenuhi (baris, buat, detail, riwayat, daftar) | `T-03`, `T-24`, `T-25` |
| Harga `null` tanpa `ReadPrice` | Terpenuhi | `T-03`, `T-15`, `T-16` |
| Regresi PR | Terpenuhi | `F-01`, `T-08`, `T-17`, `T-20`, `T-28` |
| Penanda izin eksplisit (D-2 `BE-PBJ-PS-004`) dan DI | Terpenuhi | `F-00`; aplikasi start bersih |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `RowVersion` PR kini berganti saat barisnya dipaketkan atau dilepas (D-10). Layar PR yang terbuka lama mendapat 409 `PBJ-VAL-090` dan perlu muat ulang |
| Masalah yang diketahui | Kecocokan rencana paket (status, tahun, jenis) belum diputuskan (D-7). Temuan lama tetap terbuka: status `PBJ-VAL-091` 409 di `PBJ-PP-VAL` vs 422 di `PBJ-PS-VAL` (putuskan sebelum `BE-PBJ-PS-008`); kamus data perlu diselaraskan (`SortOrder` → `CriterionNo`, FK `PrcQuotation.SupplierId`); cabang "metode dipakai paket" di `BE-PBJ-PS-007`; cabang "kode terkunci oleh lampiran paket" di `BE-PBJ-PS-006`. Kamus data `HpsUnitPrice` sebaiknya menyebut rata-rata tertimbang (D-3) |
| Risiko tersisa | Status `PBJ-VAL-128` hanya dapat diuji setelah paket bisa berpindah status (`BE-PBJ-PS-008`); jalurnya ada di `LoadEditableAsync` tetapi belum dibuktikan runtime |
| Perubahan sampingan | `NONE` di source. Di DB uji: fixture dua putaran dihapus lunak; konfigurasi `Wfl` uji dicabut |
| Interupsi | Putaran 1 berhenti karena cacat CHECK (bagian 5); diperbaiki, build dan QBE diulang, putaran 2 dari fixture baru. Satu query baca DB ditolak penjaga izin sebelum konfirmasi pemilik; dijalankan sesudah konfirmasi |
| Status Git | Belum di-commit. Baru: 5 berkas `Sourcing`, `PurchaseRequestPackagingDtos.cs`, laporan ini. Diubah: `ProcurementPurchaseRequestService.cs`, `ProcurementPurchaseRequestVerificationService.cs`, `ProcurementStatuses.cs`, `ProcurementDocumentHistoryActions.cs`, `ProcurementManagementServiceCollectionExtensions.cs`, roadmap backend dan frontend, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-PS-006` (lampiran paket dan lampiran harga tertutup; menyentuh `Common`) — gelombang 5. `FE-PBJ-PS-001` boleh paralel |
