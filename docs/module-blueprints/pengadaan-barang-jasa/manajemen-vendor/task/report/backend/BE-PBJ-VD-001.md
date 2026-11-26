# Laporan Perubahan Backend — `BE-PBJ-VD-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-001` |
| Judul | Fondasi vendor |
| Slice | `MVP-V0` — fondasi, master konfigurasi, dan profil vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-001` |
| Trace | `PBJ-DEC-010`, `052`, `061`, `063`; `PBJ-ASM-001`; FR-VD-001, FR-VD-002; `AT-VD-60`, `61`, `62`, `64` |
| Contract version | Kontrak `PBJ-VD-*` set `1.0` (`approved`, `PBJ-DEC-062`); kamus data 1.0; arsitektur backend bagian 5–10 |
| Dependency | — (gelombang 1) |
| Klasifikasi | `MEDIUM` (logika 1, kontrak API 0, database 2, keamanan 1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-VD-001`, 17 November 2026) |
| Wewenang database | Migration dibuat dan diterapkan **hanya** ke `QuilvianNewDevSukma`, sesuai izin pemilik pada tawaran yang sama |
| Target tulis | Model `MasterData` dan `Vendor`, konfigurasi EF, `ApplicationDbContext.cs`, konstanta `Common/Constants`, seeder, `Program.cs`, migration `AddProcurementVendorManagement` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** Sembilan tabel terpasang di `QuilvianNewDevSukma` dan cocok dengan kamus data; 14 uji batasan basis data `PASS`; seeder dua kali tanpa baris berlipat; QBE Strict `PASS` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` (transaksi vendor) dan `MasterData` (tiga master vendor) |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`, change log 2026-11-17); master `MstProcurement*` di baris `MasterData` yang sudah `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk sembilan model, konfigurasi, seeder vendor, migration; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs`, `Program.cs`, empat konstanta, dan seeder definisi workflow Pengadaan |
| Arketipe endpoint | Tidak ada endpoint (di luar cakupan task) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`, `Mst`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-CODE-002..004`, `QBE-DB-001` |
| Hasil checker | QBE Strict `PASS` (25 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Sub-modul Manajemen Vendor belum punya tempat menyimpan data. Sebelum task ini, Pengadaan hanya bisa membaca identitas
supplier dari Master Data; tidak ada tempat untuk mencatat kategori yang dilayani vendor, dokumen legalitasnya, status
Dibatasi, usulan blokir, maupun nilai kinerjanya. Task ini membangun fondasinya: sembilan tabel, nama-nama tetap
(konstanta), dan data awal yang dibutuhkan task berikutnya.

## 2. Proses bisnis yang dilayani fondasi ini

| Langkah bisnis | Tabel yang menampung | Contoh |
| --- | --- | --- |
| Tim Pengadaan mencatat kategori yang dilayani vendor | `PrcVendorProfile`, `PrcVendorCategory` | PT Sehat Medika melayani kategori Alat Kesehatan |
| Petugas mengunggah dokumen legalitas, petugas lain memeriksanya | `PrcVendorDocument` (+ berkas di `PrcAttachment`) | Izin PBF berlaku sampai 30-06-2027, status `PENDING` → `VERIFIED` |
| Admin menentukan dokumen wajib per kategori, bertahap | `MstProcurementCategoryDocumentRequirement` | Kategori Obat mewajibkan Izin PBF mulai 01-03-2027; awalnya kosong |
| Manajer membatasi vendor sementara | `PrcVendorProfile.IsRestricted` | Dibatasi sampai 30-06-2027 karena keterlambatan berulang |
| Manajer mengusulkan blokir, Direktur memutus lewat approval | `PrcVendorStatusRequest` | `UBV-000003` blokir PT Alkes Maju |
| Tim menilai vendor per kategori per semester, Manajer memfinalkan | `PrcVendorEvaluation`, `PrcVendorEvaluationScore`, `MstProcurementEvaluationCriterion` | Semester I 2027, nilai akhir 4,20 |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstProcurementItemCategory` dan konfigurasinya, `PrcBudgetConfiguration`, `PrcPurchaseRequestConfiguration`,
`MstSupplier`, `ProcurementMasterDataSeeder`, `ProcurementWorkflowDefinitionSeeder`, `NumberSeriesAllocator`,
`NumNumberSeriesConfiguration`, `ProcurementAttachmentService` (perilaku jenis dokumen tanpa penyedia akses),
`ApplicationDbContextFactory`, kamus data, matriks validasi, dan matriks hak akses manajemen-vendor.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/MasterData/Models/MstProcurementVendorDocumentType.cs` | Baru | Jenis dokumen legalitas |
| `.../MasterData/Models/MstProcurementCategoryDocumentRequirement.cs` | Baru | Dokumen wajib per kategori |
| `.../MasterData/Models/MstProcurementEvaluationCriterion.cs` | Baru | Kriteria penilaian berbobot |
| `.../Vendor/Models/PrcVendorProfile.cs` | Baru | Profil vendor, status Dibatasi |
| `.../Vendor/Models/PrcVendorCategory.cs` | Baru | Kategori yang dilayani vendor |
| `.../Vendor/Models/PrcVendorDocument.cs` | Baru | Dokumen legalitas dan verifikasinya |
| `.../Vendor/Models/PrcVendorStatusRequest.cs` | Baru | Usulan blokir/buka blokir |
| `.../Vendor/Models/PrcVendorEvaluation.cs` | Baru | Penilaian vendor |
| `.../Vendor/Models/PrcVendorEvaluationScore.cs` | Baru | Nilai per kriteria |
| `Repositories/Configurations/Corporate/ProcurementManagement/MasterData/` (3 berkas) | Baru | Konfigurasi EF tiga master |
| `Repositories/Configurations/Corporate/ProcurementManagement/Vendor/` (6 berkas) | Baru | Konfigurasi EF enam tabel vendor |
| `.../MasterData/Seeders/ProcurementVendorMasterSeeder.cs` | Baru | Lima jenis dokumen, tiga kriteria `UMUM` |
| `.../Common/Seeders/ProcurementWorkflowDefinitionSeeder.cs` | Diperbarui | Definisi `PBJ-VND-BLOKIR`, `PBJ-VND-BUKA` |
| `.../Common/Constants/ProcurementDocumentTypes.cs` | Diperbarui | `PBJ_VENDOR`, `PBJ_VENDOR_DOCUMENT`, `PBJ_VENDOR_STATUS`, `PBJ_VENDOR_EVALUATION` (+ himpunan `Known`) |
| `.../Common/Constants/ProcurementNumberSeries.cs` | Diperbarui | Deret `PBJ_VENDOR_STATUS`, awalan `UBV` |
| `.../Common/Constants/ProcurementWorkflowCodes.cs` | Diperbarui | `PBJ-VND-BLOKIR`, `PBJ-VND-BUKA` |
| `.../Common/Constants/ProcurementPermissions.cs` | Diperbarui | Kode modul `CORPORATE_PROCUREMENT_MANAGEMENT_VENDOR`, aksi `Restrict`, `Finalize`, tujuh resource vendor |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | Sembilan `DbSet` + `using ...Vendor.Models` |
| `Program.cs` | Diperbarui | Pemanggilan `ProcurementVendorMasterSeeder` sesudah seeder master Pengadaan |
| `Migrations/20261117074530_AddProcurementVendorManagement.cs` + `.Designer.cs` | Baru | Sembilan `CreateTable`, 22 index, 8 CHECK |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | Hasil generator EF |

### 3.3 Dampak kontrak API, database, dan keamanan

**API.** Tidak ada endpoint baru atau berubah.

**Database.** Sembilan tabel baru di schema `public`, tidak ada tabel lama yang berubah. Migration dapat dijalankan
tanpa mematikan layanan karena hanya menambah tabel kosong. Langkah mundur: `Down` menghapus kesembilan tabel dengan
urutan yang aman terhadap FK.

**Keamanan.** Konstanta resource dan aksi disiapkan supaya argumen `[AccessController]`, `[AccessAction]`, dan
`[AccessPermission]` pada task berikutnya tidak berselisih satu huruf. Belum ada butir yang muncul di layar Akses Role
karena belum ada controller. Jenis dokumen vendor yang baru dikenal belum punya penyedia akses lampiran, sehingga
permintaan lampiran untuk jenis itu dijawab 404 (fail-closed) sampai `BE-PBJ-VD-004`.

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Arsitektur bagian 10: `NumNumberSeries` di-seed `PBJ_VENDOR_STATUS`/`UBV` | Tidak di-seed. Hanya konstanta kunci dan awalan | `NumberSeriesAllocator` membuat deret otomatis pada alokasi pertama, sama dengan deret `PR`, `RKB`, `RPK`, `KTL` yang juga tidak di-seed. Index unik `(SequenceKey, ScopeKey)` menjamin deret tidak berlipat (`AT-VD-64`) |
| D-2 | Kamus data: unik "tanpa beda huruf" untuk kode jenis dokumen dan nama kriteria | Index unik biasa (peka huruf) | Pola master Pengadaan: service menyimpan kode dalam huruf besar. Untuk nama kriteria, service `BE-PBJ-VD-002` wajib memeriksa duplikat tanpa beda huruf sebelum simpan |
| D-3 | Nilai bawaan kolom status (`PENDING`, `DRAFT`, `SEMESTER`, `MANUAL`) | Diisi oleh model, bukan default basis data | Sama dengan konfigurasi Pengadaan lain (`PrcPurchaseRequest`, `PrcBudget`) |
| D-4 | CHECK di kamus data: bobot, nilai 1–5, alasan Dibatasi | Ketiganya dibuat, ditambah lima CHECK pengaman: tanggal berlaku ≥ tanggal terbit (`PBJ-VAL-101`), nilai akhir 1–5, `PeriodNo` 1–4, `RequestType` `BLOCK`/`UNBLOCK`, alasan usulan tidak kosong | Semua turunan aturan yang sudah disetujui di matriks validasi dan matriks status; tidak ada aturan baru |
| D-5 | Arsitektur folder: extension DI "Diperbarui" | Tidak berubah pada task ini | Task ini belum membuat service. Pendaftaran DI masuk task yang membuat service-nya |

## 4. Dokumentasi endpoint

Tidak ada endpoint pada task ini (lihat kartu task, "Di luar cakupan").

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 25 berkas `.cs` baru/berubah | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet ef migrations list` sebelum membuat migration | Semua migration sebelumnya sudah terpasang, tidak ada yang `(Pending)` | `PASS` | Keluaran perintah |
| Isi migration `AddProcurementVendorManagement` | Hanya 9 `CreateTable`; tidak ada perubahan tabel modul lain; tidak ada kolom FK bayangan | `PASS` | Pemeriksaan berkas migration |
| `dotnet ef database update AddProcurementVendorManagement` ke `QuilvianNewDevSukma` | "Applying migration '20261117074530_AddProcurementVendorManagement'. Done." | `PASS` | Keluaran perintah; baris `__EFMigrationsHistory` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Frontend | Tidak disentuh task ini | `NOT APPLICABLE` | — |

Target basis data diperiksa sebelum perintah EF: `appsettings.Development.json` menunjuk `QuilvianNewDevSukma`, user
secrets dan variabel lingkungan tidak menimpa nama basis data.

### 5.1 Skema terhadap kamus data (`AT-VD-62`)

Diperiksa dari `information_schema.columns`, `pg_indexes`, dan `pg_constraint`.

| Tabel | Kolom bisnis | Index | CHECK | FK (perilaku hapus) | Cocok |
| --- | --- | --- | --- | --- | :---: |
| `MstProcurementVendorDocumentType` | `DocumentTypeCode` varchar(30), `DocumentTypeName` varchar(150), `RequiresExpiryDate`, `Description` varchar(500), `IsActive` | Kode unik `WHERE "IsDelete" = false` | — | — | Ya |
| `MstProcurementCategoryDocumentRequirement` | `ItemCategoryId`, `DocumentTypeId`, `IsActive`, `ActivatedAt` timestamptz, `ActivatedByUserId` | Pasangan kategori × jenis unik aktif | — | Kategori, jenis (`RESTRICT`) | Ya |
| `MstProcurementEvaluationCriterion` | `CriterionName` varchar(150), `WeightPercent` numeric(5,2) | Kategori × nama unik aktif | `0 < bobot ≤ 100` | Kategori (`RESTRICT`) | Ya |
| `PrcVendorProfile` | `SupplierId`, `IsRestricted`, `RestrictedUntil` date, `RestrictionReason` varchar(1000), `RestrictedAt`, `RestrictedByUserId`, `RowVersion` | Supplier unik aktif; `IsRestricted` | Alasan wajib bila Dibatasi | `MstSupplier` (`RESTRICT`) | Ya |
| `PrcVendorCategory` | `VendorProfileId`, `ItemCategoryId` | Pasangan unik aktif; `ItemCategoryId` | — | Profil (`CASCADE`), kategori (`RESTRICT`) | Ya |
| `PrcVendorDocument` | `DocumentNumber` varchar(100), `IssuedDate`/`ExpiryDate` date, `VerificationStatus` varchar(20), `VerifiedByUserId`, `VerifiedAt`, `RejectionReason` varchar(1000), `RowVersion` | (profil, jenis, status verifikasi); `ExpiryDate` | Tanggal berlaku ≥ terbit | Profil, jenis (`RESTRICT`) | Ya |
| `PrcVendorStatusRequest` | `RequestNumber` varchar(30), `RequestType` varchar(10), `Reason` varchar(1000), `EvidenceEvaluationId`, `Status` varchar(20), `WorkflowInstanceId` (tanpa FK), `SubmissionCount`, `SubmittedAt`, `DecidedAt`, `RowVersion` | **`IX_PrcVendorStatusRequest_OpenPerVendor`** unik `WHERE Status IN (DRAFT, PENDING_APPROVAL, RETURNED) AND IsDelete = false`; nomor unik bila terisi; `Status`; `WorkflowInstanceId` | Jenis usulan; alasan tidak kosong | Profil, penilaian bukti (`RESTRICT`) | Ya |
| `PrcVendorEvaluation` | `PeriodYear`, `PeriodType` varchar(10), `PeriodNo`, `SourceType` varchar(10), `Status` varchar(12), `FinalScore` numeric(4,2), `Recommendation` varchar(10), `Notes` varchar(2000), `PreviousEvaluationId`, `RevisionReason` varchar(1000), `FinalizedByUserId`, `FinalizedAt`, `RowVersion` | **`IX_PrcVendorEvaluation_OriginalPerPeriod`** unik (profil, kategori, tahun, jenis periode, nomor periode) `WHERE Status IN (DRAFT, FINAL) AND PreviousEvaluationId IS NULL AND IsDelete = false`; `Status`; `PreviousEvaluationId` | Nilai akhir 1–5; `PeriodNo` 1–4 | Profil, kategori, penilaian sebelumnya (`RESTRICT`) | Ya |
| `PrcVendorEvaluationScore` | `CriterionName` varchar(150), `WeightPercent` numeric(5,2), `Score` int, `Note` varchar(500) | Penilaian × kriteria unik aktif | Nilai 1–5 | Penilaian (`CASCADE`), kriteria (`RESTRICT`) | Ya |

### 5.2 Uji batasan basis data

Dijalankan di `QuilvianNewDevSukma` dalam satu transaksi yang **di-rollback** di akhir; sesudahnya jumlah baris profil,
usulan, dan penilaian tetap 0.

| # | Skenario | Diharapkan | Hasil | Status |
| ---: | --- | --- | --- | :---: |
| 1 | Profil Dibatasi tanpa alasan | Ditolak CHECK | `CK_PrcVendorProfile_RestrictionReason` | `PASS` |
| 2 | Profil sah | Tersimpan | Tersimpan | `PASS` |
| 3 | Profil kedua untuk supplier yang sama | Ditolak unik | `IX_PrcVendorProfile_SupplierId` | `PASS` |
| 4 | Kriteria bobot 0 | Ditolak CHECK | `CK_MstProcurementEvaluationCriterion_WeightPercent` | `PASS` |
| 5 | Usulan blokir `DRAFT` pertama | Tersimpan | Tersimpan | `PASS` |
| 6 | Usulan kedua `PENDING_APPROVAL` saat masih ada yang terbuka (`PBJ-VAL-107`) | Ditolak unik | `IX_PrcVendorStatusRequest_OpenPerVendor` | `PASS` |
| 7 | Usulan kedua berstatus selesai (`REJECTED`) | Tersimpan | Tersimpan | `PASS` |
| 8 | Penilaian asli `FINAL` Semester I 2027 | Tersimpan | Tersimpan | `PASS` |
| 9 | Penilaian asli kedua periode sama (`PBJ-VAL-109`) | Ditolak unik | `IX_PrcVendorEvaluation_OriginalPerPeriod` | `PASS` |
| 10 | Revisi `DRAFT` periode sama dengan `PreviousEvaluationId` | Tersimpan | Tersimpan | `PASS` |
| 11 | `PeriodNo` 5 | Ditolak CHECK | `CK_PrcVendorEvaluation_PeriodNo` | `PASS` |
| 12 | Nilai kriteria 6 | Ditolak CHECK | `CK_PrcVendorEvaluationScore_Score` | `PASS` |
| 13 | Nilai kriteria 4 | Tersimpan | Tersimpan | `PASS` |
| 14 | Kriteria yang sama dinilai dua kali | Ditolak unik | `IX_PrcVendorEvaluationScore_EvaluationId_CriterionId` | `PASS` |

14 skenario, 14 `PASS`.

### 5.3 Seeder dijalankan dua kali (`AT-VD-64`)

Build baru dijalankan dua kali di `https://localhost:7185` terhadap `QuilvianNewDevSukma`.

| Run | Log seeder master vendor | Log seeder definisi workflow |
| --- | --- | --- |
| 1 | "5 jenis dokumen, 3 kriteria penilaian ditambahkan." | "2 definisi, 2 langkah ditambahkan" |
| 2 | "0 jenis dokumen, 0 kriteria penilaian ditambahkan." | "0 definisi, 0 langkah ditambahkan" |

Isi basis data sesudah run 2:

| Data | Isi | Cocok |
| --- | --- | :---: |
| Jenis dokumen | `NIB`, `NPWP`, `LAIN` (tanpa tanggal berlaku); `IZIN_PBF`, `IZIN_DISTRIBUSI_ALKES` (dengan tanggal berlaku); masing-masing 1 baris, aktif | Ya |
| Kriteria `UMUM` | Ketepatan waktu 40, Kualitas 40, Layanan purnajual 20; total 100 | Ya |
| Dokumen wajib per kategori | 0 baris (`PBJ-DEC-061`) | Ya |
| Definisi `PBJ-VND-BLOKIR`, `PBJ-VND-BUKA` | `RequestType = PBJ_VENDOR_STATUS`, `IsActive = false`, `Draft`, versi 1, satu langkah `Position`, `AllowSelfApproval = false` | Ya |
| Matriks approval untuk `PBJ-VND-*` | 0 baris (tanpa matriks) | Ya |
| Deret `PBJ_VENDOR_STATUS` | 0 baris; dibuat otomatis pada pengajuan pertama (delta D-1) | Ya |

Seeder juga mencari menurut Id, sehingga bila admin mengganti nama kriteria seed, start ulang tidak menabrak kunci
primer dan tidak menambah kriteria baru.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-60` registry memuat submodule `Vendor` sebelum model pertama; QBE lulus | Terpenuhi | Baris registry `ProcurementManagement / Vendor · Prc · ACTIVE` (2026-11-17); QBE Strict `PASS` |
| `AT-VD-61` build | Terpenuhi untuk backend; frontend tidak disentuh | Bagian 5 |
| `AT-VD-62` skema sesuai kamus data, termasuk unik parsial usulan terbuka dan penilaian | Terpenuhi | Bagian 5.1, 5.2 |
| `AT-VD-64` seeder dua kali tidak berlipat; kewajiban dokumen kosong | Terpenuhi | Bagian 5.3 |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Jenis dokumen dan kriteria adalah nilai awal (`PBJ-ASM-001`, contoh `PBJ-DEC-050`); nilai final menunggu SOP (`PBJ-OQ-019`, `PBJ-OQ-034`). Definisi `PBJ-VND-*` tidak aktif sampai admin mengisi jabatan penyetuju |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Kolom `VendorProfileId` pada usulan dan penilaian hanya tercakup index unik parsial. Pencarian riwayat seluruh usulan/penilaian satu vendor tidak memakai index; dampaknya kecil karena jumlah baris per vendor sedikit. Bila daftar riwayat lambat, tambahkan index biasa lewat migration baru |
| Perubahan sampingan | `NONE`. Generator EF hanya menyentuh berkas migration baru dan snapshot |
| Interupsi | Satu jeda ringkasan konteks sebelum model ditulis; dilanjutkan dari kondisi terverifikasi (belum ada kode VD-001) |
| Status Git | Belum di-commit. Baru: 9 model, 9 konfigurasi EF, `ProcurementVendorMasterSeeder.cs`, migration + Designer, laporan ini. Diubah: `ApplicationDbContext.cs`, `Program.cs`, `ApplicationDbContextModelSnapshot.cs`, empat konstanta, `ProcurementWorkflowDefinitionSeeder.cs`, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-002` (master konfigurasi vendor) dan `BE-PBJ-VD-003` (daftar dan detail vendor), keduanya gelombang 2 |
