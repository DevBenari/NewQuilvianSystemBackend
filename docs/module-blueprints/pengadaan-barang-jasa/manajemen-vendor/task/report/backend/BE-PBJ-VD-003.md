# Laporan Perubahan Backend — `BE-PBJ-VD-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-003` |
| Judul | Profil, kategori, rekening, riwayat vendor |
| Slice | `MVP-V0` — fondasi, master konfigurasi, dan profil vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-003` |
| Trace | `PBJ-DEC-010`, `015`, `047` (status dihitung), `051`, `052`, `053`, `055`, `060`; `PBJ-ASM-002`, `004`; FR-VD-010, 011, 012, 050; `PBJ-VAL-090`, `094`, `100`; `AT-VD-01`..`04` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor (tanpa `restriction` dan `eligibility`), `PBJ-VD-STATE` 1.0 bagian 1–2, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-001` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 2, database 0, keamanan 2 — rekening tersamarkan) |
| Task mode | `BACKEND` (pemilik: "saya izinkan dan lanjutkan", 17 November 2026) |
| Target tulis | `Vendor/Controllers`, `Vendor/Services`, `Vendor/DTOs`, konstanta status dan aksi riwayat, extension DI, `appsettings.json` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** Enam endpoint vendor berjalan; uji runtime 25/25 `PASS` dengan aktor non-SuperAdmin; tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` untuk controller, service, DTO, exception vendor; `TOUCHED LEGACY` untuk `ProcurementStatuses.cs`, `ProcurementDocumentHistoryActions.cs`, extension DI, `appsettings.json` |
| Arketipe endpoint | Read-only (daftar, ringkasan, detail, riwayat) + satu sub-resource ubah `PUT /{supplierId}/categories` |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (7 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Sebelum task ini, Tim Pengadaan belum bisa melihat supplier dari sudut pandang Pengadaan. Belum ada status gabungan
Master Data dan Pengadaan, belum ada kategori yang dilayani vendor, dan belum ada riwayat. Task ini membuka:
- daftar dan detail vendor;
- pengaturan kategori vendor;
- riwayat vendor.

Nomor rekening tidak pernah keluar utuh.

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Melihat semua vendor beserta statusnya | Tim Pengadaan | `GET /vendors` | PT Alkes Maju tampil **Diblokir** karena Master Data mencatatnya diblokir |
| Membuka satu vendor | Tim Pengadaan | `GET /vendors/{supplierId}` | Rekening tampil `****7890`, bank dan nama pemilik, "Tidak tersedia di Master Data" untuk verifikasi |
| Mengatur kategori yang dilayani pertama kali | Tim Pengadaan | `PUT /vendors/{supplierId}/categories` | Profil Pengadaan lahir; riwayat pertama mencatat "Status blokir awal dari Master Data: Diblokir (…)" |
| Mengganti kategori | Tim Pengadaan | `PUT …/categories` dengan `expectedRowVersion` | Alat Kesehatan → Obat: riwayat "Kategori Obat ditambahkan", "Kategori Alat Kesehatan dilepas" |
| Membaca linimasa vendor | Tim Pengadaan, Direktur | `GET /vendors/{supplierId}/history` | Pembuatan profil, perubahan kategori, dan kelak dokumen/status/penilaian |

**Status vendor dihitung saat dibaca** (`PBJ-VD-STATE` bagian 1). Urutannya: Nonaktif di Master Data → Diblokir →
Dibatasi → Aktif. Dibatasi hanya berlaku bila tanggal berakhirnya kosong atau belum lewat.

**Keadaan dokumen vendor** diambil dari dokumen terverifikasi terakhir per jenis. Urutannya: ada yang kedaluwarsa →
segera kedaluwarsa (≤ `Procurement:Vendor:ExpiryWarningDays` = 30 hari) → menunggu verifikasi → berlaku → belum ada
dokumen.

**Jalur gagal:**

| Kejadian | Respons | Yang dilakukan petugas |
| --- | --- | --- |
| Menambah kategori nonaktif | 422 `PBJ-VAL-100` "Kategori {nama} tidak aktif." | Pilih kategori aktif, atau aktifkan kategorinya di Pengaturan |
| Dua petugas mengubah kategori vendor yang sama | Simpanan kedua 409 `PBJ-VAL-090` | Muat ulang, lalu ulangi |
| Pengguna tanpa `ProcurementVendor : Read` (mis. Unit Peminta) | 403 | — (menu tidak tampil) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstSupplier`, `SupplierController` (pembuatan supplier uji), `ProcurementDocumentHistoryWriter`,
`DocumentHistoryResponse`, `ProcurementStatuses`, `ProcurementRequestExceptions` (pola 409 `PBJ-VAL-090`),
`ProcurementPeriodFilter`, `AppDateTimeHelper`, model vendor dari `BE-PBJ-VD-001`, kontrak API §Vendor, matriks status
bagian 1–2, matriks hak akses, dan arsitektur frontend layar `-01`, `-02`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Controllers/VendorsController.cs` | Baru | 6 endpoint, resource `ProcurementVendor`, modul Akses Role `CORPORATE_PROCUREMENT_MANAGEMENT_VENDOR` |
| `.../Vendor/Services/ProcurementVendorProfileService.cs` | Baru | Status dihitung, keadaan dokumen, rekening tersamarkan, kategori + riwayat |
| `.../Vendor/Services/ProcurementVendorExceptions.cs` | Baru | Validasi 422 dan bentrok 409 berkode |
| `.../Vendor/DTOs/VendorDtos.cs` | Baru | Query, respons daftar/detail/ringkasan/metadata, permintaan kategori, nilai status dan keadaan dokumen |
| `.../Common/Constants/ProcurementStatuses.cs` | Diperbarui | `VendorDocument` (`PENDING`/`VERIFIED`/`REJECTED`/`CANCELLED`), `VendorEvaluation` (`DRAFT`/`FINAL`/`SUPERSEDED`/`CANCELLED`) |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `CATEGORY_ADD`, `CATEGORY_REMOVE` |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | `ProcurementVendorProfileService` |
| `appsettings.json` | Diperbarui | `Procurement:Vendor:ExpiryWarningDays = 30` (`PBJ-ASM-002`, arsitektur bagian 10) |

### 3.3 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Penulisan hanya ke `PrcVendorProfile`, `PrcVendorCategory`, dan
`PrcDocumentHistory`. `MstSupplier` hanya dibaca (`AT-VD-01` membuktikan barisnya tidak berubah).

**Keamanan.**
- Nomor rekening utuh tidak pernah keluar. Respons hanya membawa `maskedBankAccount`, dan log audit hanya mencatat id
  dan jumlah perubahan.
- Data vendor tidak dibatasi per unit (`PBJ-VD-PERM` bagian 1).
- Aksi `Restrict` belum didaftarkan; aksi itu milik `BE-PBJ-VD-006`.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | API: daftar = "supplier aktif Master Data + data Pengadaan" | Semua supplier yang belum dihapus tampil, termasuk yang nonaktif, dengan status **Nonaktif di Master Data** | Saringan status pada layar `-01` memuat pilihan Nonaktif (FE §4.1, STATE bagian 1); menyembunyikannya membuat pilihan itu selalu kosong |
| D-2 | Keadaan dokumen tidak dirinci di kontrak | Lima nilai `EXPIRED`, `EXPIRING_SOON`, `PENDING`, `VALID`, `NONE`, diambil dari keadaan terburuk dokumen terverifikasi terakhir per jenis | Saringan "Keadaan dokumen" dan kartu "Dokumen kedaluwarsa" (FE §4.1) membutuhkan satu nilai per vendor |
| D-3 | Ringkasan: "dokumen kedaluwarsa, segera kedaluwarsa" | `expiredDocuments`/`expiringSoonDocuments` = jumlah **dokumen** (terverifikasi terakhir per jenis), ditambah `expiryWarningDays` | Satu vendor bisa punya dua dokumen kedaluwarsa; jumlah vendor tetap bisa dibaca dari saringan |
| D-4 | `UpdateVendorCategoriesRequest.expectedRowVersion` | Wajib bila profil sudah ada. Kosong atau berbeda → 409 `PBJ-VAL-090`. Diabaikan saat profil pertama dibuat | Profil yang belum ada tidak punya versi; dua pembuatan bersamaan dijaga index unik → 409 |
| D-5 | — | Kategori yang **ditambahkan** harus aktif (`PBJ-VAL-100`); kategori lama yang kini nonaktif boleh dipertahankan | Mencegah petugas dipaksa melepas kategori lama hanya untuk menambah kategori lain |
| D-6 | — | Simpan tanpa perubahan → 200 tanpa riwayat dan tanpa versi baru | Riwayat hanya untuk perubahan nyata |
| D-7 | — | Detail memuat identitas baca saja dari Master Data (nama resmi, jenis, NPWP, NIB, kontak, alamat) | Tab "Profil & Kategori" (FE §4.2) |
| D-8 | — | Daftar dihitung di memori setelah saringan dasar dijalankan di basis data | Status dan keadaan dokumen bergantung tanggal operasional hari ini. Aman untuk ratusan supplier; dicatat sebagai risiko bila kelak ribuan |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor

Base URL: `api/v1/corporate/procurement-management/vendors`. Semua respons dibungkus `ApiResponse<T>`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, status vendor (4), keadaan dokumen (5), kategori, batas peringatan | `ProcurementVendor : Read` | — | `VendorFilterMetadataResponse` |
| `GET` | `/summary` | Total, Aktif, Dibatasi, Diblokir, Nonaktif, dokumen kedaluwarsa, segera kedaluwarsa | `ProcurementVendor : Read` | — | `VendorSummaryResponse` |
| `GET` | `/` | Daftar vendor | `ProcurementVendor : Read` | `startDate`, `endDate`, `customPeriod`, `search`, `itemCategoryId`, `vendorStatus`, `documentState`, `sortBy` (`code`/`name`/`status`), `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<VendorListItemResponse>` |
| `GET` | `/{supplierId}` | Detail: identitas, status, kategori, ringkasan dokumen, rekening tersamarkan, penilaian final terakhir per kategori | `ProcurementVendor : Read` | — | `VendorDetailResponse` |
| `PUT` | `/{supplierId}/categories` | Atur kategori; profil dibuat bila belum ada | `ProcurementVendor : Update` | `{ itemCategoryIds: [guid], expectedRowVersion?: guid }` | `VendorDetailResponse` |
| `GET` | `/{supplierId}/history` | Riwayat vendor `PBJ_VENDOR` | `ProcurementVendor : Read` | — | `List<DocumentHistoryResponse>` |

Kode status: 200; 400 (validasi model); 401; 403; 404 (supplier tidak ada atau terhapus); 409 `PBJ-VAL-090`; 422
(`PBJ-VAL-100`, `PBJ-VAL-094`, atau nilai saringan tidak dikenal).

Contoh potongan respons `GET /{supplierId}`:

```json
{ "supplierCode": "SUP-000012", "supplierName": "PT Alkes Maju", "vendorStatus": "BLOCKED", "vendorStatusLabel": "Diblokir",
  "isBlacklisted": true, "isRestricted": false, "profileId": "…", "rowVersion": "…",
  "categories": [ { "itemCategoryId": "…", "code": "ALKES", "name": "Alat Kesehatan", "isActive": true } ],
  "documents": { "documentState": "NONE", "documentStateLabel": "Belum ada dokumen", "expiryWarningDays": 30 },
  "bankAccount": { "bankName": "Bank Uji", "maskedBankAccount": "****7890", "bankAccountName": "PT …",
                   "bankAccountVerification": "Tidak tersedia di Master Data" },
  "latestEvaluations": [] }
```

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah (satu galat kompilasi saat pertama dibangun — `using` dan nama tuple — diperbaiki) | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 7 berkas `.cs` | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | 25 skenario, 25 `PASS` | `PASS` | `vd003.results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ VD003 20261117153820`, lewat API SuperAdmin).** Satu unit dan dua jabatan, dengan akun
`Employee` **tanpa role**:
- `proc`: `ProcurementVendor : Read, Update` dan `ProcurementItemCategory : Read`.
- `req`: `ProcurementRequest : Read, Create` saja, mewakili Unit Peminta.

Ada juga tiga kategori uji (A, B, X nonaktif) dan tiga supplier uji di Master Data:
- S1 dengan rekening `1234567890`;
- S2 diblokir lewat Master Data, yang masih bisa dilakukan sebelum `BE-PBJ-VD-007`;
- S3 dinonaktifkan.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role `ProcurementVendor` terdaftar | `PASS` | `Read`, `Update` |
| `AT-VD-04` | Aktor Unit Peminta membuka daftar dan ringkasan vendor | `PASS` | 403, 403 |
| `AU-01` | Tanpa login | `PASS` | 401 |
| `V-01` | Metadata | `PASS` | 4 status, 5 keadaan dokumen, kategori uji, 6 periode, `expiryWarningDays = 30` |
| `V-02` | Tiga supplier uji tampil dengan status dihitung | `PASS` | Aktif, Diblokir, Nonaktif; belum ada profil; keadaan dokumen `NONE` |
| `V-03` | Saring status Diblokir (huruf kecil) | `PASS` | Hanya S2 |
| `V-04` | Status vendor tidak dikenal | `PASS` | 422 |
| `V-05` | Periode bulan lalu | `PASS` | 0 baris |
| `V-06` | Tanggal awal > akhir | `PASS` | 422 `PBJ-VAL-094` |
| `AT-VD-03` | Detail S1 | `PASS` | `maskedBankAccount = "****7890"`, bank, nama pemilik, "Tidak tersedia di Master Data"; `1234567890` tidak ada di respons |
| `AT-VD-03b` | Nomor utuh di daftar, ringkasan, riwayat, metadata | `PASS` | Tidak ditemukan di keempatnya |
| `V-07` | Detail vendor tanpa profil | `PASS` | `profileId`/`rowVersion` kosong, kategori kosong |
| `V-08` | Riwayat vendor tanpa profil | `PASS` | `[]` |
| `V-09` | Vendor tak dikenal | `PASS` | 404 detail dan riwayat |
| `AT-VD-02` | Atur kategori nonaktif | `PASS` | 422 `PBJ-VAL-100` "Kategori … tidak aktif."; profil tidak dibuat |
| `V-10` | Kategori tidak ada | `PASS` | 422 `PBJ-VAL-100` |
| `AT-VD-01` | Atur kategori S2 (belum punya profil) | `PASS` | Profil 1 baris; baris `MstSupplier` identik sebelum/sesudah; riwayat `CREATE` (ke `BLOCKED`, alasan memuat "Diblokir") lalu `CATEGORY_ADD` |
| `V-11` | `expectedRowVersion` basi | `PASS` | 409 `PBJ-VAL-090` |
| `V-12` | Profil ada tanpa `expectedRowVersion` | `PASS` | 409 `PBJ-VAL-090` |
| `V-13` | Ganti A → B | `PASS` | Riwayat + `CATEGORY_ADD`, `CATEGORY_REMOVE`; `rowVersion` baru |
| `V-14` | Kategori B dinonaktifkan, lalu simpan [B, A] | `PASS` | 200 (kategori lama nonaktif dipertahankan) |
| `V-15` | Simpan tanpa perubahan | `PASS` | 200; riwayat tetap 5 baris; `rowVersion` tetap |
| `V-16` | Saring kategori A | `PASS` | Hanya S2 |
| `V-17` | Riwayat lewat API | `PASS` | 5 baris, pertama `CREATE`, nama pelaku "TEST-PBJ proc VD003", jenis `PBJ_VENDOR` |
| `V-18` | Ringkasan | `PASS` | total 5 = supplier Master Data, diblokir 1, nonaktif 1, dokumen 0 |

25 skenario, 25 `PASS`.

**Belum terbukti runtime.** Ada tiga cabang logika yang datanya belum bisa dibuat lewat API:
- **Status Dibatasi**, termasuk yang tanggal berakhirnya sudah lewat. Diuji di `BE-PBJ-VD-006`.
- **Keadaan dokumen** (`EXPIRED`, `EXPIRING_SOON`, `PENDING`, `VALID`). Diuji di `BE-PBJ-VD-004`.
- **Penilaian final terakhir per kategori.** Diuji di `BE-PBJ-VD-008`.

Kodenya sudah ada di service ini, dan ketiga task itu wajib menguji ulang daftar dan detail vendor.

**Sesudah uji.** Supplier dan kategori uji dihapus lunak lewat API Master Data. Sisanya dibersihkan dengan
`cleanup_vd003.py` atas izin pemilik (17 November 2026, "lanjutkan" atas tawaran pembersihan). Dry run dijalankan lebih
dulu, lalu apply dalam satu transaksi. Yang dihapus lunak:
- 1 profil vendor dan 2 kategori vendor;
- 5 policy, 2 Employee, 2 jabatan, dan 1 unit (kodenya diberi akhiran `-DEL-20261117153820`).

Dua akun uji dinonaktifkan dan dikunci. Riwayat vendor uji tidak disentuh karena riwayat hanya ditambah; barisnya tidak
tampil karena profilnya sudah terhapus. Sesudahnya `PrcVendorProfile` aktif = 0.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-01` profil lahir satu baris, `MstSupplier` tidak berubah, riwayat status blokir awal | Terpenuhi | `AT-VD-01` |
| `AT-VD-02` kategori nonaktif → 422 `PBJ-VAL-100` | Terpenuhi | `AT-VD-02` |
| `AT-VD-03` `****7890`, nomor utuh tidak ada di respons mana pun | Terpenuhi | `AT-VD-03`, `AT-VD-03b` |
| `AT-VD-04` Unit Peminta → 403 (bagian API; menu adalah bagian `FE-PBJ-VD-001`) | Terpenuhi (API) | `AT-VD-04` |
| Endpoint kartu task: metadata, summary, daftar, detail, history, PUT categories | Terpenuhi | `V-01`..`V-18` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `ExpiryWarningDays = 30` adalah nilai awal (`PBJ-ASM-002`) |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Perhitungan daftar di memori (delta D-8). Cabang Dibatasi, keadaan dokumen, dan penilaian terakhir baru terbukti runtime di `VD-004`/`006`/`008` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: `VendorsController.cs`, `ProcurementVendorProfileService.cs`, `ProcurementVendorExceptions.cs`, `VendorDtos.cs`, laporan ini. Diubah: `ProcurementStatuses.cs`, `ProcurementDocumentHistoryActions.cs`, extension DI, `appsettings.json`, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-004` (dokumen legalitas), `BE-PBJ-VD-006` (Dibatasi), `BE-PBJ-VD-007` (usulan blokir), `BE-PBJ-VD-008` (penilaian) — semuanya kini tidak terhalang; `FE-PBJ-VD-001` dan `FE-PBJ-VD-002` siap dari sisi backend |

---

## Pembaruan 18 November 2026 — rekening tersamarkan di daftar vendor (untuk `FE-PBJ-VD-002` layar `-05`)

### Mengapa dikerjakan ulang

Layar `-05` Rekening Vendor (`03-frontend-architecture.md` bagian 4.6) meminta daftar rekening dengan saringan **Bank**. Kontrak `PBJ-VD-API` 1.0 hanya menaruh rekening tersamarkan di detail (`GET /vendors/{supplierId}`), sehingga daftar vendor tidak bisa menampilkannya. Pilihan lainnya adalah satu panggilan detail per baris, sampai 25 request tambahan per halaman, tanpa saringan bank.

Pemilik memilih "Perluas backend" pada 18 November 2026: daftar vendor membawa rekening tersamarkan dan bisa disaring per bank.

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Vendor` `ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` — berkas milik `BE-PBJ-VD-003` sendiri |
| Klasifikasi | `SMALL` — 2 berkas, tanpa endpoint baru, tanpa migration |
| QBE yang berlaku | Konformitas berkas yang disentuh (QBE Strict); hak akses tidak berubah (endpoint dan `[AccessPermission]` sama) |

### Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/DTOs/VendorDtos.cs` | `VendorQuery.BankName` (opsional, maks. 100); `VendorListItemResponse.BankName`, `MaskedBankAccount`, `BankAccountName`; `VendorDefaultFilterResponse.BankName`; `VendorFilterMetadataResponse.BankOptions` |
| `Areas/Corporate/ProcurementManagement/Vendor/Services/ProcurementVendorProfileService.cs` | `LoadAsync` membaca kolom bank supplier dan langsung menyamarkan nomor (`MaskBankAccount` yang sama dengan detail), sehingga snapshot tidak pernah menyimpan nomor utuh. `GetPagedAsync` menyaring `bankName` tanpa beda huruf besar/kecil dan spasi tepi. Metadata mengirim daftar nama bank unik dari supplier Master Data, urut abjad |

### Endpoint yang berubah

`[Tags("Corporate / Procurement Management / Vendor / Vendor")]` — base URL `api/v1/corporate/procurement-management/vendors`

| Method | Path | Perubahan | Hak akses | Contoh |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Query baru `bankName`; setiap baris membawa `bankName`, `maskedBankAccount`, `bankAccountName` | `ProcurementVendor : Read` (tetap) | `GET /vendors?bankName=Bank%20Mandiri` → baris `{ "bankName": "Bank Mandiri", "maskedBankAccount": "****7890", "bankAccountName": "PT Sehat Medika" }` |
| `GET` | `/filters/metadata` | Field baru `bankOptions: [{ value, label }]`; `defaultFilter.bankName` | `ProcurementVendor : Read` (tetap) | `"bankOptions": [{ "value": "Bank Mandiri", "label": "Bank Mandiri" }]` |

Kode status tidak berubah:

- 200 sukses.
- 400 bila `bankName` lebih dari 100 karakter (validasi model).
- 403 tanpa izin.
- 422 periode salah.

Nama bank yang tidak dikenal menghasilkan 0 baris, bukan galat.

### Delta kontrak

| ID | Delta | Status |
| --- | --- | --- |
| D-9 | `PBJ-VD-API` §Vendor: `GET /` dan `GET /filters/metadata` bertambah field dan query bank (aditif, tidak memecah klien lama) | Usulan `PBJ-VD-API` 1.1; menunggu pembaruan dokumen kontrak oleh pemilik bersama `PBJ-VD-INT` 1.1 |

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` | 0 error; 0 warning dari dua berkas yang diubah (603 warning lama di berkas lain) | `PASS` | `be-vd003r-build.log` scratchpad |
| QBE Strict (2 berkas) | VIOLATION 0, REVIEW 0 | `PASS` | keluaran perintah |
| Migration | Tidak ada — kolom bank sudah ada di `MstSupplier` | `N/A` | — |
| Uji runtime `vd003r.py` (backend uji 7185, `QuilvianNewDevSukma`, aktor non-SuperAdmin) | 10/10 | `PASS` | `vd003r.results.json` |

Rincian uji runtime:

| ID | Skenario | Hasil |
| --- | --- | --- |
| `R-01` | Metadata `bankOptions` memuat dua bank uji, urut abjad, tanpa duplikat | `PASS` |
| `R-02` | `bankName` bank S1 → hanya S1, `****7890`, nama bank dan pemilik terisi | `PASS` |
| `R-03` | `bankName` huruf kecil + spasi tepi → hasil sama | `PASS` |
| `R-04` | Daftar tiga supplier uji: nomor utuh `1234567890`/`9988776655` tidak ada di respons (`PBJ-DEC-015`) | `PASS` |
| `R-05` | Supplier tanpa rekening → tiga field `null` | `PASS` |
| `R-06` | Supplier kedua → `****6655` | `PASS` |
| `R-07` | `bankName` + `vendorStatus` digabung → satu baris; bank tak dikenal → 0 baris | `PASS` |
| `R-08` | `bankName` 101 karakter → 400 | `PASS` |
| `R-09` | Aktor `ProcurementVendor : Read` → 200; Unit Peminta → 403 (`AT-VD-04`) | `PASS` |
| `R-10` | Detail tetap `****7890`, nomor utuh tidak ada (`AT-VD-03`) | `PASS` |

### Catatan penutup pembaruan

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Saringan bank memakai nama bank teks bebas di Master Data. Ejaan berbeda untuk bank yang sama (mis. "BCA" dan "Bank BCA") tampil sebagai dua pilihan, dan ini milik Master Data Supplier |
| Data uji | Fixture `TEST-PBJ … FEVD002` (run `20261118140409`) dipakai bersama uji layar `FE-PBJ-VD-002`; pembersihan dicatat di laporan frontend |
| Status Git | Belum di-commit. Diubah: `VendorDtos.cs`, `ProcurementVendorProfileService.cs`, laporan ini |

## Pembaruan 19 November 2026 — saringan gabungan "semua selain Aktif" (untuk `FE-PBJ-VD-004` layar `-06`)

### Mengapa dikerjakan ulang

Layar `-06` Status Vendor (`03-frontend-architecture.md` bagian 4.4) memuat daftar **vendor non-Aktif**: Dibatasi, Diblokir, dan Nonaktif di Master Data sekaligus. `GET /vendors` hanya menerima satu nilai `vendorStatus` per panggilan, sehingga daftar gabungan dengan paginasi yang benar tidak bisa dimuat.

Dua jalan frontend saja tidak sesuai rancangan:

- Saringan status wajib dipilih satu per satu, sehingga tidak ada tampilan gabungan.
- Semua vendor diurutkan per status, sehingga vendor Aktif ikut tampil.

Pemilik memilih "Perluas backend" pada 19 November 2026.

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Vendor` `ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` — berkas milik `BE-PBJ-VD-003` sendiri |
| Klasifikasi | `SMALL` — 2 berkas, tanpa endpoint baru, tanpa migration |
| QBE yang berlaku | Konformitas berkas yang disentuh (QBE Strict); hak akses tidak berubah (endpoint dan `[AccessPermission]` sama) |

### Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/DTOs/VendorDtos.cs` | Konstanta `VendorStatusValues.NonActive = "NON_ACTIVE"` dengan label "Semua selain Aktif". Ini nilai saringan, bukan status vendor, jadi tidak pernah muncul di field `vendorStatus` respons. Keterangan `VendorQuery.VendorStatus` diperbarui |
| `Areas/Corporate/ProcurementManagement/Vendor/Services/ProcurementVendorProfileService.cs` | `GetPagedAsync` menerima `NON_ACTIVE` dan menyaring vendor yang statusnya bukan `ACTIVE`. Nilai tunggal lama tetap berlaku. Metadata menambah pilihan `NON_ACTIVE` di akhir `vendorStatusOptions` |

### Endpoint yang berubah

`[Tags("Corporate / Procurement Management / Vendor / Vendor")]` — base URL `api/v1/corporate/procurement-management/vendors`

| Method | Path | Perubahan | Hak akses | Contoh |
| --- | --- | --- | --- | --- |
| `GET` | `/` | `vendorStatus` menerima nilai gabungan `NON_ACTIVE` (tanpa beda huruf besar/kecil) | `ProcurementVendor : Read` (tetap) | `GET /vendors?vendorStatus=NON_ACTIVE` → hanya baris `RESTRICTED`, `BLOCKED`, `INACTIVE`; `totalData` sesuai jumlah gabungan |
| `GET` | `/filters/metadata` | `vendorStatusOptions` bertambah `{ "value": "NON_ACTIVE", "label": "Semua selain Aktif" }` | `ProcurementVendor : Read` (tetap) | Lima pilihan: Aktif, Dibatasi, Diblokir, Nonaktif di Master Data, Semua selain Aktif |

Kode status tidak berubah: 200 sukses; 403 tanpa izin; 422 untuk nilai status yang tidak dikenal ("Status vendor 'X' tidak dikenal.").

### Delta kontrak

| ID | Delta | Status |
| --- | --- | --- |
| D-10 | `PBJ-VD-API` §Vendor: `GET /` menerima `vendorStatus=NON_ACTIVE`; `GET /filters/metadata` menambah pilihan itu. Bersifat aditif dan tidak memecah klien lama. Layar `-01`, `-03`, `-05` ikut menampilkan pilihan baru di saringan Status | Usulan `PBJ-VD-API` 1.1 bersama D-9; menunggu pembaruan dokumen kontrak oleh pemilik |

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release` | 0 error, 603 warning (baseline); 0 warning dari berkas vendor | `PASS` | `be-vd003r2-build.log` scratchpad |
| QBE Strict (2 berkas) | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | keluaran perintah |
| Migration | Tidak ada | `N/A` | — |
| Uji runtime `vd003r2.py` (backend uji 7185, `QuilvianNewDevSukma`) | 8/8 | `PASS` | `vd003r2.results.json` |

Data uji: tiga supplier `TEST-PBJ VD003R2` (run `20261119100610`) dibuat lewat API.

- Satu tetap Aktif.
- Satu dibatasi lewat `POST /vendors/{id}/restriction`.
- Satu dinonaktifkan lewat `PATCH` status Master Data.

Status Diblokir hanya bisa lahir dari usulan yang disetujui `Wfl`, jadi dibuktikan pada uji layar `FE-PBJ-VD-004`.

| ID | Skenario | Hasil |
| --- | --- | --- |
| `R2-01` | Metadata memuat lima pilihan, termasuk `NON_ACTIVE` "Semua selain Aktif" | `PASS` |
| `R2-02` | Himpunan `NON_ACTIVE` = gabungan hasil `RESTRICTED` + `BLOCKED` + `INACTIVE` (2 = 1 + 0 + 1) | `PASS` |
| `R2-03` | Tidak ada baris `ACTIVE` di hasil `NON_ACTIVE` | `PASS` |
| `R2-04` | `ACTIVE` + `NON_ACTIVE` = semua vendor (3 + 2 = 5), tanpa irisan | `PASS` |
| `R2-05` | `" non_active "` (huruf kecil + spasi tepi) → hasil sama | `PASS` |
| `R2-06` | Nilai tidak dikenal → 422 "Status vendor 'BUKAN_STATUS' tidak dikenal." | `PASS` |
| `R2-07` | Paginasi: `pageSize=2` → `totalData` 2 dan 2 baris | `PASS` |
| `R2-08` | `sortBy=status desc` tetap berlaku pada hasil gabungan | `PASS` |

### Catatan penutup pembaruan

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Tidak ada perubahan hak akses atau data. Pilihan "Semua selain Aktif" ikut tampil di saringan Status layar `-01`, `-03`, `-05` dan berguna di sana juga |
| Data uji | Fixture `vd003r2.fixture.json` (run `20261119100610`) sudah dibersihkan 19 November 2026 bersama data uji `FE-PBJ-VD-004`; rinciannya di laporan frontend |
| Status Git | Belum di-commit. Diubah: `VendorDtos.cs`, `ProcurementVendorProfileService.cs`, laporan ini |
