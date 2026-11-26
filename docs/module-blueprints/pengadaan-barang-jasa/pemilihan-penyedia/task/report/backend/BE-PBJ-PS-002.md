# Laporan Perubahan Backend — `BE-PBJ-PS-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-002` |
| Judul | Master pemilihan |
| Slice | `MVP-PS0` — `PS-01` konfigurasi pemilihan (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-002` |
| Trace | `PBJ-DEC-037`, `055`, `068`, `095`; `PBJ-ASM-007`; FR-PS-001, FR-PS-002; `UAT-PS-01`, `UAT-PS-02` (cabang jenis dokumen) |
| Contract version | `PBJ-PS-API` 1.0 (`approved`, `PBJ-DEC-095`) grup Master Data: Procurement Method, Package Document Type, Package Document Requirement; `PBJ-PS-VAL` 1.0 (`PBJ-VAL-071`) |
| Dependency | `BE-PBJ-PS-001` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 9–20 (1), berkas diubah > 8 (2), logika sedang (1), kontrak API baru dari kontrak yang ada (1), keamanan berkaitan lewat atribut akses (1), database hanya query/persistence (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-002`, 26 November 2026) |
| Wewenang database | Tanpa migration. Uji runtime di `https://localhost:7185` terhadap `QuilvianNewDevSukma`, fixture lewat API, dan pembersihannya disetujui pemilik 26 November 2026 ("Ya, jalankan dan bersihkan") |
| Target tulis | Tiga controller, tiga service, satu berkas DTO, satu exception tambahan, pendaftaran DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `45b22777` (perubahan `BE-PBJ-PS-001` dan dokumen blueprint belum di-commit), frontend `3e5386679` |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI.** 27 endpoint (3 master × 9); build Release 0 error; QBE Strict `PASS` 9 berkas; uji runtime 61/61 `PASS` dengan aktor non-SuperAdmin (satu ekspektasi skrip dikoreksi, bagian 5); fixture dibersihkan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `MasterData` (tiga master pemilihan); kode modul Akses Role `CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING` (arsitektur §4.3) |
| Registry | Master `MstProcurement*` di baris `MasterData` (`Mst`, `ACTIVE`); submodule `Sourcing` (`Prc`, `ACTIVE`, `PBJ-DEC-096`) |
| Keberlakuan | `NEW CODE` untuk tiga controller, tiga service, DTO; `TOUCHED LEGACY` untuk `ProcurementMasterDataExceptions.cs` (tambah satu exception) dan `ProcurementManagementServiceCollectionExtensions.cs` |
| Arketipe endpoint | Master data — sembilan endpoint baku (`master-data-endpoint-standard.md`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001`, `QBE-LOG-001` |
| Hasil checker | QBE Strict `PASS` (9 berkas, 0 temuan) |
| Hak akses | Tidak ada `IsInRole`, nama peran, departemen, atau jabatan di kode. Argumen ke-1 `[AccessPermission]` = `ControllerName`, argumen ke-2 = argumen ke-1 `[AccessAction]` pada method yang sama; terbukti di uji `AU-00`..`AU-05` |

## 1. Masalah yang diselesaikan

Tiga master pemilihan sudah punya tabel dan data contoh (`BE-PBJ-PS-001`), tetapi belum bisa dibaca maupun diubah lewat
API. Akibatnya admin belum bisa mengganti metode contoh dengan metode SOP, mengatur jenis dokumen paket, atau menentukan
dokumen wajib. Frontend Pengaturan (`FE-PBJ-PS-001`) dan task persiapan paket juga belum punya sumber data. Task ini
membuka sembilan endpoint baku untuk masing-masing master, lengkap dengan aturan penjaganya.

## 2. Proses bisnis

**Mengganti metode contoh dengan metode SOP (`UAT-PS-01`).** Admin Pengadaan:

1. Membuat metode SOP dalam keadaan **nonaktif**, misalnya "Penunjukan Langsung" sampai Rp 50.000.000. Kalau langsung
   dibuat aktif, sistem menolak dengan 409 karena rentangnya tumpang tindih dengan metode contoh yang masih aktif.
2. Menonaktifkan metode contoh "Penunjukan Langsung (contoh)".
3. Mengaktifkan metode SOP. Kali ini diterima, karena rentangnya (sampai 50 juta) tidak beririsan dengan "Pembandingan
   Penawaran (contoh)" (di atas 50 juta).
4. Ringkasan menunjukkan jumlah metode contoh yang masih aktif berkurang satu. Target sebelum produksi adalah 0
   (`PBJ-DEC-069`).

**Aturan yang dijaga:**

| Master | Aturan | Jawaban |
| --- | --- | --- |
| Metode | Kode huruf besar/angka/garis bawah, unik | 422 / 409 |
| Metode | Batas bawah ≥ 0, batas atas > 0, bawah < atas, minimum penawaran 1–10 | 422 |
| Metode | Rentang `Min < HPS ≤ Max` tidak tumpang tindih di antara metode **aktif**; metode nonaktif boleh tumpang tindih | 409, pesan menyebut metode yang bentrok |
| Metode | `IsSample` hanya dari seeder; metode dari API selalu bukan contoh, dan ubahan tidak menghapus tanda contoh | — |
| Metode | Hapus metode yang dipakai dokumen wajib | 400 `PBJ-VAL-071` |
| Jenis dokumen | Tahap hanya `PREPARATION`/`QUOTATION`/`NEGOTIATION`; kode unik | 422 / 409 |
| Jenis dokumen | Nonaktifkan jenis yang masih diwajibkan dokumen wajib aktif | 409 |
| Jenis dokumen | Ganti kode setelah ada lampiran paket berkategori kode itu | 409 |
| Jenis dokumen | Hapus jenis yang dipakai dokumen wajib atau lampiran paket | 400 `PBJ-VAL-071` |
| Dokumen wajib | Tepat satu dari metode dan jenis pengadaan | 400 |
| Dokumen wajib | Aktif hanya bila jenis dokumen dan metode/jenisnya aktif | 422 |
| Dokumen wajib | Satu pasangan jenis dokumen × metode (atau × jenis) | 409 |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ProcurementVendorDocumentTypesController`, `ProcurementVendorDocumentTypeService`,
`ProcurementCategoryDocumentRequirementService`, `ProcurementVendorMasterDtos.cs`, `ProcurementMasterDataDtos.cs`,
`ProcurementMasterDataExceptions.cs`, `ProcurementPeriodFilter.cs`, `ProcurementManagementServiceCollectionExtensions.cs`,
`PrcAttachment.cs`, `Responses/ApiResponse.cs`, `Program.cs` (penanganan model state), `master-data-endpoint-standard.md`,
`role-access-rules.md`, kontrak API, validasi, dan hak akses pemilihan-penyedia.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/MasterData/Controllers/ProcurementMethodsController.cs` | Baru | 9 endpoint metode pengadaan |
| `.../MasterData/Controllers/ProcurementPackageDocumentTypesController.cs` | Baru | 9 endpoint jenis dokumen paket |
| `.../MasterData/Controllers/ProcurementPackageDocumentRequirementsController.cs` | Baru | 9 endpoint dokumen wajib paket |
| `.../MasterData/Services/ProcurementMethodService.cs` | Baru | CRUD + cek tumpang tindih rentang HPS |
| `.../MasterData/Services/ProcurementPackageDocumentTypeService.cs` | Baru | CRUD + penjaga kode/lampiran/dokumen wajib |
| `.../MasterData/Services/ProcurementPackageDocumentRequirementService.cs` | Baru | CRUD + aturan tepat satu cakupan dan syarat aktif |
| `.../MasterData/DTOs/ProcurementSourcingMasterDtos.cs` | Baru | Query, Upsert, Response, Option, Summary, FilterMetadata ketiga master |
| `.../MasterData/Services/ProcurementMasterDataExceptions.cs` | Diperbarui | `ProcurementMasterBadRequestException` (HTTP 400) |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Tiga `AddScoped` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 27 endpoint baru; tidak ada endpoint lama yang berubah |
| Database | Tidak ada perubahan skema; tidak ada migration. Hanya query dan simpan pada tiga tabel `BE-PBJ-PS-001` |
| Keamanan/Auth | Tiga resource baru muncul di layar Akses Role di bawah modul `CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING`, masing-masing dengan aksi Read/Create/Update/Delete. Pemegangnya ditentukan admin; bundel `PBJ-DEC-080` belum diterapkan ke jabatan nyata |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain / standar | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Master vendor memakai kode modul `MasterDataModuleCode` | Ketiga controller memakai `SourcingModuleCode` | Arsitektur pemilihan-penyedia §4.3 menyatakan seluruh controller sub-modul memakai kode modul Sourcing. Di layar Akses Role, ketiganya tampil di bawah modul Sourcing |
| D-2 | Standar master: `/filters/metadata` memuat `QueryParameters`, `CreateFields`, `UpdateFields`, `DateFormat` | Bentuk metadata mengikuti master Pengadaan yang sudah ada: `CustomPeriods`, `DefaultFilter`, `SortOptions`, `SortDirections`, `PageSizeOptions`, `ResetButtonLabel`, ditambah pilihan enum (`StageOptions`, `ScopeOptions`, pilihan relasi) | Source repository yang berlaku (`ProcurementVendorDocumentTypeService`, PP master) tidak memuat empat field itu; frontend Pengadaan sudah memakai bentuk ini |
| D-3 | Kontrak: dokumen wajib tanpa/dengan dua cakupan → 400 | `ProcurementMasterBadRequestException` → 400 `ApiResponse`; validasi isian lain tetap 422 | Kontrak menetapkan 400 untuk aturan ini; `[ApiController]` tanpa `InvalidModelStateResponseFactory` akan mengembalikan ProblemDetails, bukan `ApiResponse` |
| D-4 | Kontrak tidak memberi kode untuk tumpang tindih rentang | 409 tanpa `errors.code`, pesan menyebut metode yang bentrok | Bentrok antarbaris = konflik (pola 409 master Pengadaan); tidak ada `PBJ-VAL` untuk aturan ini |
| D-5 | — | Kode jenis dokumen tidak dapat diganti, dan jenisnya tidak dapat dihapus, bila sudah ada lampiran `PBJ_PACKAGE`/`PBJ_RFQ` berkategori kode itu | Kamus data: kode = `AttachmentCategory`; mengganti kode akan memutus checklist paket. Cabang ini belum bisa diuji runtime karena lampiran paket baru bisa diunggah setelah `BE-PBJ-PS-006`; kode diperiksa lewat review |
| D-6 | — | Jenis dokumen yang masih diwajibkan dokumen wajib aktif tidak dapat dinonaktifkan (409) | Pola master vendor (`ProcurementVendorDocumentTypeService`); mencegah checklist yang tidak bisa dipenuhi |
| D-7 | Kartu roadmap: hapus metode yang dipakai paket → 400 | Pemeriksaan pemakaian saat ini hanya oleh dokumen wajib | Tabel paket belum ada; pemeriksaan "dipakai paket" wajib ditambahkan di `BE-PBJ-PS-007` (kartu roadmap sudah menyebut `UAT-PS-02` di sana) |
| D-8 | — | Filter tambahan `options`: jenis dokumen `stage`; dokumen wajib `methodId`, `procurementTypeId` | Dibutuhkan checklist persiapan (`BE-PBJ-PS-007`) dan layar Pengaturan |
| D-9 | — | Batas HPS dibulatkan dua desimal saat disimpan | Kolom `numeric(18,2)` |

## 4. Dokumentasi endpoint

Base URL `api/v1/corporate/procurement-management`. Semua respons `ApiResponse<T>`; daftar `PagedResult<T>`.

#### Corporate / Procurement Management / Master Data / Procurement Method

Base `/master-data/procurement-methods`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan (`hps`, `code`, `name`, `isActive`), bawaan | `ProcurementMethod : Read` |
| `GET` | `/summary` | Total, aktif, nonaktif, contoh aktif, wajib evaluasi teknis | `ProcurementMethod : Read` |
| `GET` | `/` | Daftar; saring periode, `search`, `isActive`, `isSample`, `requiresTechnicalEvaluation` | `ProcurementMethod : Read` |
| `GET` | `/options` | Metode aktif (bawaan `onlyActive=true`), `search` | `ProcurementMethod : Read` |
| `GET` | `/{id}` | Detail | `ProcurementMethod : Read` |
| `POST` | `/` | Buat; 409 bila aktif dan rentangnya tumpang tindih | `ProcurementMethod : Create` |
| `PUT` | `/{id}` | Ubah penuh | `ProcurementMethod : Update` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif; aktivasi dicek tumpang tindih | `ProcurementMethod : Update` |
| `DELETE` | `/{id}` | Hapus lunak; 400 `PBJ-VAL-071` bila dipakai dokumen wajib | `ProcurementMethod : Delete` |

Contoh `POST /master-data/procurement-methods`:

```json
{ "code": "PENUNJUKAN_LANGSUNG", "name": "Penunjukan Langsung", "minHpsAmount": null, "maxHpsAmount": 50000000,
  "minimumQuotationCount": 1, "requiresTechnicalEvaluation": false, "isActive": false }
```

#### Corporate / Procurement Management / Master Data / Package Document Type

Base `/master-data/package-document-types`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, pilihan tahap (Persiapan/Penawaran/Negosiasi) | `ProcurementPackageDocumentType : Read` |
| `GET` | `/summary` | Total, aktif, nonaktif, tertutup harga, jumlah per tahap | `ProcurementPackageDocumentType : Read` |
| `GET` | `/` | Daftar; saring periode, `search`, `isActive`, `stage`, `isPriceSensitive` | `ProcurementPackageDocumentType : Read` |
| `GET` | `/options` | Jenis aktif, `stage`, `search` | `ProcurementPackageDocumentType : Read` |
| `GET` | `/{id}` | Detail, termasuk label tahap dan jumlah dokumen wajib aktif | `ProcurementPackageDocumentType : Read` |
| `POST` | `/` | Buat | `ProcurementPackageDocumentType : Create` |
| `PUT` | `/{id}` | Ubah penuh; kode terkunci bila sudah dipakai lampiran paket | `ProcurementPackageDocumentType : Update` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif; 409 bila masih diwajibkan | `ProcurementPackageDocumentType : Update` |
| `DELETE` | `/{id}` | Hapus lunak; 400 `PBJ-VAL-071` bila dipakai | `ProcurementPackageDocumentType : Delete` |

#### Corporate / Procurement Management / Master Data / Package Document Requirement

Base `/master-data/package-document-requirements`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, cakupan, pilihan jenis dokumen/metode/jenis pengadaan | `ProcurementPackageDocumentRequirement : Read` |
| `GET` | `/summary` | Total, aktif, nonaktif, per metode, per jenis | `ProcurementPackageDocumentRequirement : Read` |
| `GET` | `/` | Daftar; saring periode, `search`, `isActive`, `documentTypeId`, `methodId`, `procurementTypeId`, `scope` | `ProcurementPackageDocumentRequirement : Read` |
| `GET` | `/options` | Dokumen wajib aktif, `methodId`, `procurementTypeId` | `ProcurementPackageDocumentRequirement : Read` |
| `GET` | `/{id}` | Detail | `ProcurementPackageDocumentRequirement : Read` |
| `POST` | `/` | Buat; 400 bila cakupan tidak tepat satu | `ProcurementPackageDocumentRequirement : Create` |
| `PUT` | `/{id}` | Ubah penuh (boleh pindah cakupan) | `ProcurementPackageDocumentRequirement : Update` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif; aktivasi dicek syarat aktif | `ProcurementPackageDocumentRequirement : Update` |
| `DELETE` | `/{id}` | Hapus lunak (paket menyimpan salinan checklist sendiri) | `ProcurementPackageDocumentRequirement : Delete` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release` | 0 error, 620 warning; **0** dari berkas baru/berubah (sama dengan garis dasar `BE-PBJ-PS-001`) | `PASS` | Log build disaring per berkas |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <9 berkas>` | VIOLATION 0, REVIEW 0, INFO 0; `Final result: PASS` | `PASS` | Laporan QBE |
| Uji runtime utama (`ps002.py`, 59 pemeriksaan) | 58 `PASS` tercetak; `DB-01` tercetak FAIL | `PASS` setelah koreksi | `DB-01` membandingkan `PEMBANDINGAN_PENAWARAN_CONTOH:True,...` padahal PostgreSQL menulis `true`. Datanya memenuhi syarat (0 master UJI hidup, kedua metode contoh `true`); `SELECT` ulang sesudah pembersihan menunjukkan hal yang sama. Bukan cacat kode |
| Uji pelengkap (`ps002b.py`, 2 pemeriksaan) | `GET /{id}` jenis dokumen dan dokumen wajib | `PASS` | Bagian 5.1 |
| Pembersihan fixture | 13 policy, 2 employee, 2 jabatan, 1 unit dihapus lunak; kode unit diberi `-DEL-`; 2 akun nonaktif + lockout | `PASS` | `SELECT` sesudahnya: 0 unit uji hidup, 0 akun uji aktif |
| `dotnet test` | Tidak ada project test | `NOT RUN` | — |

Uji manual: `PASS` (uji API dengan aktor non-SuperAdmin; tanpa layar karena frontend task terpisah).

**Tidak dijalankan:** cabang "kode/hapus diblok lampiran paket" (D-5), karena lampiran `PBJ_PACKAGE`/`PBJ_RFQ` belum bisa
dibuat sebelum `BE-PBJ-PS-006`; cabang "metode dipakai paket" (D-7), karena tabel paket belum ada.

### 5.1 Rincian uji runtime

Lingkungan: build Release di `https://localhost:7185` terhadap `QuilvianNewDevSukma`. Aktor dibuat lewat API SuperAdmin:
unit `TEST-PBJ Pengadaan Uji PS002`, akun **admin** (CRUD ketiga resource) dan **baca-saja** (`ProcurementMethod : Read`).

| Kelompok | ID | Isi | Hasil |
| --- | --- | --- | --- |
| Hak akses | `AU-00`..`05` | Tiga resource terdaftar di modul Sourcing dengan Create/Delete/Read/Update; baca-saja 200 di metode, 403 saat membuat dan saat membaca dua master lain; tanpa login 401 | 6 `PASS` |
| Metode | `PM-01`..`04` | Metadata (urutan `hps` bawaan), ringkasan, daftar urut batas bawah (Penunjukan Langsung lebih dulu), tanggal terbalik 422 `PBJ-VAL-094` | 4 `PASS` |
| Metode | `PM-05`..`11` | Kode tidak sah, bawah = atas, atas 0, bawah negatif, minimum 11 → 422; rentang 40–60 juta aktif → 409 "tumpang tindih dengan metode aktif Penunjukan Langsung (contoh) (sampai Rp 50.000.000)"; kode ganda → 409 | 7 `PASS` |
| `UAT-PS-01` | `UAT-01a`..`e` | SOP aktif → 409; SOP nonaktif dibuat (kode dinormalkan, `isSample=false`); contoh PL dinonaktifkan; SOP diaktifkan tanpa bentrok dengan Pembandingan; contoh aktif 2 → 1 | 5 `PASS` |
| Metode | `PM-12`..`19` | Pilihan hanya aktif; mengaktifkan contoh PL selagi SOP aktif → 409; pemulihan; `PUT` ubah penuh; detail; hapus metode yang dipakai dokumen wajib → 400 `PBJ-VAL-071`; hapus lalu 404 | 8 `PASS` |
| Jenis dokumen | `DT-01`..`08`, `13` | Metadata tiga tahap; ringkasan (3 tertutup harga, jumlah per tahap = total); tahap tidak sah 422; kode tidak sah 422; `KAK_TOR` ganda 409; buat (kode/tahap dinormalkan, label Penawaran); daftar bersaring; pilihan tahap Negosiasi; detail `KAK_TOR` | 9 `PASS` |
| Jenis dokumen | `DT-09`..`12` | Nonaktifkan jenis yang diwajibkan → 409; hapus jenis yang dipakai dokumen wajib → 400 `PBJ-VAL-071`; ganti kode (belum ada lampiran) → 200; hapus | 5 `PASS` |
| Dokumen wajib | `RQ-01`..`15` | Metadata dua cakupan; ringkasan; metode + jenis → 400; tanpa keduanya → 400; jenis dokumen nonaktif → 422; metode nonaktif → 422; buat cakupan METHOD; ganda → 409; daftar per jenis; pilihan per metode; pindah ke cakupan jenis; status off/on; hapus; 404; detail Dasar HPS | 16 `PASS` |
| Sesudah uji | `DB-01` | 0 metode/jenis/dokumen wajib UJI hidup; kedua metode contoh aktif | `PASS` (koreksi ekspektasi) |

Jumlah: **61 pemeriksaan, 61 `PASS`**.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Sembilan endpoint per master berjalan | Terpenuhi | Ketiga master: metadata, ringkasan, daftar, pilihan, detail, buat, ubah, status, hapus diuji (bagian 5.1) |
| (2) Rentang HPS dua metode aktif tidak boleh tumpang tindih | Terpenuhi | `PM-10`, `UAT-01a`, `PM-13` → 409; `UAT-01d` membuktikan batas Rp 50.000.000 tidak dianggap bentrok |
| (3) Dokumen wajib menolak isian kosong atau dua cakupan sekaligus (400) | Terpenuhi | `RQ-03`, `RQ-04` |
| (4) Hapus master terpakai → 400 `PBJ-VAL-071`, dibuktikan lewat jenis dokumen yang dipakai dokumen wajib | Terpenuhi | `DT-10`, juga `PM-17` |
| (5) `UAT-PS-01` ganti metode contoh dengan metode SOP | Terpenuhi | `UAT-01a`..`e` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Metode contoh tetap aktif sampai admin memasukkan metode SOP (`PBJ-OQ-005`). Bundel izin `PBJ-DEC-080` belum diterapkan ke jabatan nyata, jadi butir Akses Role baru belum dimiliki siapa pun selain SuperAdmin |
| Masalah yang diketahui | Pemeriksaan "metode dipakai paket" (D-7) dan cabang lampiran paket (D-5) baru dapat dibuktikan setelah `BE-PBJ-PS-006`/`007` |
| Risiko tersisa | Cek tumpang tindih dilakukan di service, bukan constraint basis data. Dua admin yang mengaktifkan dua metode bentrok pada detik yang sama masih bisa lolos; dampaknya kecil (master jarang diubah, satu admin) dan terlihat di daftar |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu jeda batas pemakaian sesudah `ProcurementMethodService` ditulis; dilanjutkan dari kondisi terverifikasi (`git status`, isi berkas). Percobaan uji pertama berhenti di query sebelum fixture dibuat (nama kolom modul), sehingga tidak ada data tertinggal |
| Status Git | Belum di-commit. Baru: 3 controller, 3 service, `ProcurementSourcingMasterDtos.cs`, laporan ini. Diubah: `ProcurementMasterDataExceptions.cs`, `ProcurementManagementServiceCollectionExtensions.cs`, roadmap, traceability, `MODULE-STATUS.md`. Ditambah seluruh perubahan `BE-PBJ-PS-001` dan dokumen blueprint yang belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-003` (kolom paket pada baris PR, migration `AddProcurementPurchaseRequestLinePackage`) — gelombang 2; `FE-PBJ-PS-001` sudah boleh dimulai karena prasyarat backend-nya (`BE-PBJ-PS-002`) selesai |
