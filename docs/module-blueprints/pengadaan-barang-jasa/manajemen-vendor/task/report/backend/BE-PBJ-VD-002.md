# Laporan Perubahan Backend — `BE-PBJ-VD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-002` |
| Judul | Master konfigurasi vendor |
| Slice | `MVP-V0` — fondasi, master konfigurasi, dan profil vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-002` |
| Trace | `PBJ-ASM-001`, `PBJ-DEC-041`, `045`, `050`, `055`, `061`; FR-VD-001, FR-VD-002, FR-VD-003; `PBJ-VAL-071`, `094`, `113` |
| Contract version | `PBJ-VD-API` 1.0 (dengan delta di bagian 3.3), `PBJ-VD-VAL` 1.0, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-001` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 2, database 0, keamanan 1) |
| Task mode | `BACKEND` (pemilik menjawab "oke lanjutkan" atas tawaran `BE-PBJ-VD-002`, 17 November 2026) |
| Target tulis | `MasterData/Controllers` (3 baru), `MasterData/Services` (3 baru + exception), `MasterData/DTOs` (1 baru), extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** 27 endpoint (3 × 9) berjalan; uji runtime 49/49 `PASS` dengan aktor non-SuperAdmin; tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `MasterData` |
| Registry | `Corporate / Procurement · ProcurementManagement / MasterData · Mst · ACTIVE` |
| Keberlakuan | `NEW CODE` untuk 3 controller, 3 service, DTO; `TOUCHED LEGACY` untuk `ProcurementMasterDataExceptions.cs` dan extension DI |
| Arketipe endpoint | Master data — sembilan endpoint baku per master |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (9 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Tabel konfigurasi vendor sudah ada sejak `BE-PBJ-VD-001`, tetapi belum bisa diisi atau diubah dari aplikasi. Task ini
membuka tiga master supaya Tim Pengadaan dapat mengelola:

- jenis dokumen legalitas yang boleh dicatat untuk vendor;
- dokumen apa yang wajib untuk kategori tertentu, dan kapan kewajiban itu mulai berlaku;
- kriteria penilaian beserta bobotnya per kategori.

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Menambah jenis dokumen yang dikenal | Tim Pengadaan | `POST /master-data/vendor-document-types` | `SERTIFIKAT_CDAKB`, wajib tanggal berlaku |
| Mencatat bahwa kategori Obat akan mewajibkan Izin PBF, belum diberlakukan | Tim Pengadaan | `POST /master-data/category-document-requirements` (tanpa `isActive`) | Kewajiban lahir nonaktif |
| Memberlakukan kewajiban setelah dokumen vendor Obat terkumpul | Tim Pengadaan | `PATCH /category-document-requirements/{id}/status` `{ "isActive": true }` | `activatedAt` terisi 01-03-2027 |
| Mengatur kriteria dan bobot kategori Alat Kesehatan | Tim Pengadaan | `POST /master-data/evaluation-criteria` | Ketepatan waktu 50, Kualitas 30, Layanan purnajual 20 |
| Memeriksa apakah bobot sudah 100% sebelum penilaian semester | Tim Pengadaan | `GET /evaluation-criteria/summary?itemCategoryId=` | Total 80 → layar memperingatkan "belum lengkap" |

**Jalur gagal yang paling sering ditemui petugas:**

| Kejadian | Respons | Yang dilakukan petugas |
| --- | --- | --- |
| Bobot 0 atau 120 | 422 `PBJ-VAL-113` "Bobot harus lebih dari 0 dan paling tinggi 100." | Isi bobot 1–100 |
| Menghapus kewajiban yang sudah pernah berlaku | 400 `PBJ-VAL-071` | Nonaktifkan saja |
| Menonaktifkan jenis dokumen yang masih diwajibkan | 409, menyebut kategori yang mewajibkan | Nonaktifkan kewajibannya lebih dulu |
| Mengaktifkan kewajiban untuk jenis dokumen nonaktif | 422 | Aktifkan jenis dokumennya lebih dulu |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ProcurementItemCategoriesController`, `ProcurementItemCategoryService`, `ProcurementMasterDataDtos`,
`ProcurementMasterDataExceptions`, `ProcurementPeriodFilter`, `ProcurementPeriodOptionResponse`,
`ProcurementManagementServiceCollectionExtensions`, model dan konfigurasi tiga master dari `BE-PBJ-VD-001`, kontrak API,
matriks validasi, matriks hak akses, dan arsitektur frontend layar `-11`, `-12`, `-13` (saringan yang dibutuhkan layar).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/MasterData/Controllers/ProcurementVendorDocumentTypesController.cs` | Baru | 9 endpoint, resource `ProcurementVendorDocumentType` |
| `.../MasterData/Controllers/ProcurementCategoryDocumentRequirementsController.cs` | Baru | 9 endpoint, resource `ProcurementCategoryDocumentRequirement` |
| `.../MasterData/Controllers/ProcurementEvaluationCriteriaController.cs` | Baru | 9 endpoint, resource `ProcurementEvaluationCriterion` |
| `.../MasterData/Services/ProcurementVendorDocumentTypeService.cs` | Baru | CRUD + penjaga nonaktif/hapus |
| `.../MasterData/Services/ProcurementCategoryDocumentRequirementService.cs` | Baru | CRUD + aktivasi pertama mengisi `ActivatedAt` |
| `.../MasterData/Services/ProcurementEvaluationCriterionService.cs` | Baru | CRUD + total bobot per kategori |
| `.../MasterData/DTOs/ProcurementVendorMasterDtos.cs` | Baru | Query, upsert, respons, option, ringkasan, metadata tiga master |
| `.../MasterData/Services/ProcurementMasterDataExceptions.cs` | Diperbarui | `ProcurementMasterValidationException` menerima kode opsional (mis. `PBJ-VAL-113`); pemanggil lama tidak berubah |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Tiga service `AddScoped` |

### 3.3 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Tidak ada migration.

**Keamanan.** Setiap action memakai `[AccessAction]` dan `[AccessPermission]` dengan argumen dari konstanta
`ProcurementPermissions`, sehingga resource dan aksi sama persis. Ketiga controller muncul di layar Akses Role di bawah
modul `CORPORATE_PROCUREMENT_MANAGEMENT_MASTER_DATA` (urutan 5–7, setelah empat master Pengadaan). Tidak ada nama peran
atau `IsInRole` di kode.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | API: "saringan wajib `itemCategoryId`" untuk dokumen wajib | `itemCategoryId` tersedia di daftar, ringkasan, dan pilihan, tetapi **tidak wajib**; tanpa saringan, semua kategori tampil | Layar `-12` menyaring per kategori dan juga menampilkan semua; parameter wajib akan memblokir tampilan awal |
| D-2 | Matriks validasi `PBJ-VAL-071` untuk kewajiban: "sudah dipakai" | Kewajiban dianggap terpakai bila **pernah berlaku** (`ActivatedAt` terisi). Kewajiban yang belum pernah berlaku boleh dihapus | Tidak ada tabel yang menunjuk kewajiban; yang memakainya adalah penilaian kelayakan vendor selama kewajiban itu aktif |
| D-3 | — | Pasangan kategori × jenis pada kewajiban yang pernah berlaku tidak dapat diganti lewat `PUT` (409) | Menjaga arti "berlaku sejak"; ganti = nonaktifkan, lalu buat kewajiban baru |
| D-4 | — | Kewajiban hanya dapat diaktifkan bila kategori dan jenis dokumennya aktif (422). Jenis dokumen yang masih diwajibkan kewajiban aktif tidak dapat dinonaktifkan (409) | Mencegah keadaan buntu: vendor diwajibkan dokumen yang tidak bisa dicatat |
| D-5 | — | Kategori kriteria yang sudah dipakai penilaian tidak dapat dipindah (409). Bobot dengan lebih dari dua desimal ditolak (422, tanpa kode) | Kolom `numeric(5,2)` akan membulatkan diam-diam |
| D-6 | Kontrak: ringkasan kriteria memuat total bobot | `GET /summary` memuat `categories[]` (total bobot aktif dan `isComplete` per kategori); dengan `?itemCategoryId=`, juga `activeWeightTotal`. Kategori tak dikenal → 404 | Kriteria penerimaan meminta total per kategori |
| D-7 | — | `filters/metadata` dokumen wajib dan kriteria memuat `itemCategoryOptions` (dan `documentTypeOptions` untuk dokumen wajib) | Layar tidak perlu izin baca master lain hanya untuk mengisi saringan |
| D-8 | Konvensi 422 untuk isian tidak sah | Isian yang gagal anotasi DTO (`[Required]`, `[MaxLength]`) dijawab **400** `ValidationProblemDetails` oleh ASP.NET sebelum service | Sama dengan master Pengadaan yang sudah ada (diuji: `item-categories` nama kosong juga 400) |

## 4. Dokumentasi endpoint

Base URL bersama: `api/v1/corporate/procurement-management`. Semua respons dibungkus `ApiResponse<T>`.

#### Corporate / Procurement Management / Master Data / Vendor Document Type

Base URL: `/master-data/vendor-document-types`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan periode, urutan, ukuran halaman | `ProcurementVendorDocumentType : Read` | — | `ProcurementVendorDocumentTypeFilterMetadataResponse` |
| `GET` | `/summary` | Total, aktif, nonaktif, wajib tanggal berlaku | `… : Read` | — | `ProcurementVendorDocumentTypeSummaryResponse` |
| `GET` | `/` | Daftar | `… : Read` | `startDate`, `endDate`, `customPeriod`, `search`, `isActive`, `requiresExpiryDate`, `sortBy` (`code`/`name`/`isActive`), `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<ProcurementVendorDocumentTypeResponse>` |
| `GET` | `/options` | Pilihan | `… : Read` | `onlyActive` (bawaan `true`), `search` | `List<ProcurementVendorDocumentTypeOptionResponse>` |
| `GET` | `/{id}` | Detail | `… : Read` | — | `ProcurementVendorDocumentTypeResponse` |
| `POST` | `/` | Tambah | `… : Create` | `{ code, name, requiresExpiryDate, description?, isActive }` | idem |
| `PUT` | `/{id}` | Ubah | `… : Update` | idem | idem |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `… : Update` | `{ isActive }` | idem |
| `DELETE` | `/{id}` | Hapus lunak | `… : Delete` | — | `ProcurementMasterDeleteResponse` |

Contoh respons `GET /{id}`:

```json
{ "id": "…", "code": "IZIN_PBF", "name": "Izin Pedagang Besar Farmasi", "requiresExpiryDate": true,
  "description": "Wajib bagi penyalur obat; memiliki tanggal berlaku", "isActive": true, "activeRequirementCount": 1 }
```

#### Corporate / Procurement Management / Master Data / Category Document Requirement

Base URL: `/master-data/category-document-requirements`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, pilihan kategori dan jenis dokumen | `ProcurementCategoryDocumentRequirement : Read` | — | `…FilterMetadataResponse` |
| `GET` | `/summary` | Total, aktif, nonaktif, kategori berkewajiban aktif | `… : Read` | `itemCategoryId?` | `…SummaryResponse` |
| `GET` | `/` | Daftar | `… : Read` | Saringan periode, `search`, `isActive`, `itemCategoryId`, `documentTypeId`, `sortBy` (`category`/`documentType`/`activatedAt`/`isActive`) | `PagedResult<…Response>` |
| `GET` | `/options` | Pilihan | `… : Read` | `onlyActive`, `itemCategoryId?`, `search` | `List<…OptionResponse>` |
| `GET` | `/{id}` | Detail | `… : Read` | — | `ProcurementCategoryDocumentRequirementResponse` |
| `POST` | `/` | Tambah (lahir nonaktif kecuali `isActive: true`) | `… : Create` | `{ itemCategoryId, documentTypeId, isActive }` | idem |
| `PUT` | `/{id}` | Ubah | `… : Update` | idem | idem |
| `PATCH` | `/{id}/status` | Berlakukan/hentikan kewajiban; aktivasi pertama mengisi `activatedAt` | `… : Update` | `{ isActive }` | idem |
| `DELETE` | `/{id}` | Hapus lunak (hanya yang belum pernah berlaku) | `… : Delete` | — | `ProcurementMasterDeleteResponse` |

#### Corporate / Procurement Management / Master Data / Evaluation Criterion

Base URL: `/master-data/evaluation-criteria`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, pilihan kategori | `ProcurementEvaluationCriterion : Read` | — | `…FilterMetadataResponse` |
| `GET` | `/summary` | Hitungan + total bobot aktif per kategori | `… : Read` | `itemCategoryId?` | `ProcurementEvaluationCriterionSummaryResponse` |
| `GET` | `/` | Daftar | `… : Read` | Saringan periode, `search`, `isActive`, `itemCategoryId`, `sortBy` (`name`/`category`/`weightPercent`/`isActive`) | `PagedResult<…Response>` |
| `GET` | `/options` | Pilihan | `… : Read` | `onlyActive`, `itemCategoryId?`, `search` | `List<…OptionResponse>` |
| `GET` | `/{id}` | Detail | `… : Read` | — | `ProcurementEvaluationCriterionResponse` |
| `POST` | `/` | Tambah | `… : Create` | `{ itemCategoryId, name, weightPercent, description?, isActive }` | idem |
| `PUT` | `/{id}` | Ubah | `… : Update` | idem | idem |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `… : Update` | `{ isActive }` | idem |
| `DELETE` | `/{id}` | Hapus lunak (hanya yang belum dipakai penilaian) | `… : Delete` | — | `ProcurementMasterDeleteResponse` |

Contoh respons `GET /summary?itemCategoryId=…`:

```json
{ "total": 3, "active": 2, "inactive": 1, "activeWeightTotal": 80.0,
  "categories": [ { "itemCategoryId": "…", "itemCategoryCode": "ALKES", "itemCategoryName": "Alat Kesehatan",
                    "activeCriterionCount": 2, "activeWeightTotal": 80.0, "isComplete": false } ] }
```

Kode status bersama ketiga master: 200, 400 (validasi model; atau `errors.code` `PBJ-VAL-071` saat hapus master
terpakai), 401, 403, 404, 409 (bentrok), 422 (aturan isian; `errors.code` `PBJ-VAL-094` untuk rentang tanggal terbalik,
`PBJ-VAL-113` untuk bobot).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 9 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | 49 skenario, 49 `PASS` (satu harapan skrip dikoreksi, lihat `DT-05`) | `PASS` | `vd002.results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ VD002 20261117151321`, dibuat lewat API sebagai SuperAdmin).** Satu unit, dua jabatan, dua akun
`Employee` **tanpa role**:
- `adm`: Read/Create/Update/Delete tiga master vendor dan `ProcurementItemCategory : Read`.
- `ro`: hanya `ProcurementVendorDocumentType : Read`.

Ada juga satu kategori uji. Semua uji fungsi memakai `adm`, bukan SuperAdmin.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role tiga resource terdaftar saat start | `PASS` | 4 aksi per resource di `SysControllerAccess`/`SysActionAccess` |
| `AU-01` | Aktor hanya-baca membaca jenis dokumen | `PASS` | 200 |
| `AU-02` | Aktor tanpa `Create` menambah jenis dokumen | `PASS` | 403 |
| `AU-03` | Aktor tanpa izin kriteria | `PASS` | 403 |
| `AU-04` | Aktor tanpa izin dokumen wajib | `PASS` | 403 |
| `AU-05` | Tanpa login | `PASS` | 401 |
| `DT-01` | Metadata: 6 periode, urutan kode/nama/status | `PASS` | 200 |
| `DT-02` | Ringkasan = hitungan basis data | `PASS` | total 5, aktif 5, wajib tanggal berlaku 2 |
| `DT-03` | Tambah jenis → kode disimpan huruf besar | `PASS` | `tvd…` → `TVD17151321` |
| `DT-04` | Kode sama beda huruf | `PASS` | 409 |
| `DT-05` | Nama kosong | `PASS` | 400 `errors.Name` — harapan awal skrip 422 keliru; `item-categories` juga 400 (delta D-8) |
| `DT-06` | Cari + wajib tanggal berlaku + periode hari ini | `PASS` | 1 baris |
| `DT-07` | Periode bulan lalu | `PASS` | 0 baris |
| `DT-08` | Tanggal awal > akhir | `PASS` | 422 `PBJ-VAL-094` |
| `DT-09` | Pilihan aktif memuat 5 jenis seed + jenis baru | `PASS` | 6 pilihan |
| `DT-10` | Detail; id tak dikenal | `PASS` | 200; 404 |
| `DT-11` | Ubah nama | `PASS` | 200 |
| `RQ-01` | Metadata memuat pilihan kategori dan jenis dokumen | `PASS` | 200 |
| `RQ-02` | Tambah kewajiban tanpa `isActive` → lahir nonaktif (`PBJ-DEC-061`) | `PASS` | `isActive=false`, `activatedAt=null` |
| `RQ-03` | Pasangan ganda | `PASS` | 409 |
| `RQ-04` | `PATCH /status` aktif → `activatedAt`, `activatedByUserId`, `activatedByName` terisi | `PASS` | `2026-11-17T08:13:28Z`, "TEST-PBJ adm VD002" |
| `RQ-05` | Nonaktif lalu aktif lagi → `activatedAt` tetap | `PASS` | Nilai sama |
| `RQ-06` | Basis data menyimpan `ActivatedAt`/`ActivatedByUserId` | `PASS` | Query DB |
| `RQ-07` | Nonaktifkan jenis yang masih diwajibkan | `PASS` | 409, menyebut kategori |
| `RQ-08` | Hapus jenis yang tercatat sebagai kewajiban | `PASS` | 400 `PBJ-VAL-071` |
| `RQ-09` | Hapus kewajiban yang pernah berlaku | `PASS` | 400 `PBJ-VAL-071` |
| `RQ-10` | Pindah kategori kewajiban yang pernah berlaku | `PASS` | 409 |
| `RQ-11` | Tambah kewajiban aktif dengan jenis nonaktif | `PASS` | 422 |
| `RQ-12` | Kewajiban nonaktif + jenis nonaktif tersimpan; aktivasinya | `PASS` | 200; 422 |
| `RQ-13` | Daftar saring kategori | `PASS` | 2 baris |
| `RQ-14` | Ringkasan kategori uji | `PASS` | total 2, aktif 1, kategori berkewajiban aktif 1 |
| `RQ-15` | Pilihan aktif kategori uji | `PASS` | 1 |
| `RQ-16` | Hapus kewajiban yang belum pernah berlaku | `PASS` | 200 |
| `CR-01` ×3 | Bobot 0, 100,5, −5 | `PASS` | 422 `PBJ-VAL-113` |
| `CR-02` | Bobot tiga desimal | `PASS` | 422 |
| `CR-03` | Tiga kriteria 50/30/20 | `PASS` | Tersimpan |
| `CR-04` | Ringkasan kategori uji | `PASS` | `activeWeightTotal = 100`, `isComplete = true` |
| `CR-05` | Nonaktifkan bobot 20 | `PASS` | 80, `isComplete = false` |
| `CR-06` | Ringkasan tanpa kategori | `PASS` | UMUM 100 lengkap; kategori uji 80 |
| `CR-07` | Nama sama tanpa beda huruf | `PASS` | 409 |
| `CR-08` | Kategori tidak ada | `PASS` | 422 |
| `CR-09` | Daftar urut bobot menurun | `PASS` | 50, 30, 20 |
| `CR-10` | Pilihan aktif kategori uji | `PASS` | 2 |
| `CR-11` | Ubah nama dan bobot | `PASS` | 200 |
| `CR-12` | Ringkasan kategori tak dikenal | `PASS` | 404 |
| `CR-13` | Hapus kriteria belum dipakai | `PASS` | 200 |
| `CR-14` | Kriteria terhapus | `PASS` | 404 |

49 skenario, 49 `PASS`.

**Belum terbukti runtime.** Dua cabang penolakan hapus master terpakai belum bisa diuji lewat HTTP, karena data
pemakainya belum dapat dibuat:
- jenis dokumen yang dipakai `PrcVendorDocument`;
- kriteria yang dipakai `PrcVendorEvaluationScore`.

Kodenya memakai pola yang sama dengan `RQ-08`, yang sudah terbukti. Bukti runtime menyusul di `BE-PBJ-VD-004`
(dokumen) dan `BE-PBJ-VD-008` (penilaian).

**Sesudah uji, data uji dinetralkan lewat API:**
- kewajiban uji dinonaktifkan;
- kriteria uji dan jenis nonaktif dihapus;
- jenis uji yang dipakai kewajiban dinonaktifkan;
- kategori uji dinonaktifkan.

**Pembersihan penuh (izin pemilik 17 November 2026, "saya izinkan").** Dijalankan dengan `cleanup_vd002.py`: dry run
lebih dulu, lalu apply dalam satu transaksi. Yang dihapus lunak:
- 1 kewajiban, 1 jenis dokumen, dan 1 kategori uji;
- 14 policy, 2 Employee, 2 jabatan, dan 1 unit (kodenya diberi akhiran `-DEL-20261117151321`).

Dua akun uji dinonaktifkan dan dikunci. Sesudahnya tersisa 5 jenis dokumen seed, 0 kewajiban, dan 0 kategori `TEST-PBJ`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Sembilan endpoint per master berjalan | Terpenuhi | `DT-*`, `RQ-*`, `CR-*` (27 endpoint dipanggil) |
| `summary` kriteria memuat total bobot aktif per kategori | Terpenuhi | `CR-04`..`CR-06` |
| `PATCH /status` kewajiban mengisi `ActivatedAt` pada aktivasi pertama | Terpenuhi | `RQ-04`..`RQ-06` |
| Bobot di luar 0–100 → `PBJ-VAL-113` | Terpenuhi | `CR-01` |
| Saringan tanggal/periode `PBJ-DEC-055` | Terpenuhi | `DT-06`..`DT-08` |
| Hapus master terpakai 400 + `PBJ-VAL-071` | Terpenuhi untuk jenis dokumen (via kewajiban) dan kewajiban; cabang dokumen/penilaian menyusul di `VD-004`/`VD-008` | `RQ-08`, `RQ-09` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta D-2..D-5 adalah penjaga integritas data yang tidak tertulis eksplisit di kontrak. Bila pemilik tidak setuju, masing-masing bisa dilepas tanpa perubahan skema |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Menonaktifkan **kategori** (master kategori Pengadaan) tidak memeriksa kewajiban aktif kategori itu. Kelayakan vendor (`BE-PBJ-VD-005`) harus mengabaikan kategori nonaktif |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: 3 controller, 3 service, `ProcurementVendorMasterDtos.cs`, laporan ini. Diubah: `ProcurementMasterDataExceptions.cs`, `ProcurementManagementServiceCollectionExtensions.cs`, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-003` (profil, kategori, rekening, riwayat vendor). `FE-PBJ-VD-001` sudah tidak terhalang backend |
