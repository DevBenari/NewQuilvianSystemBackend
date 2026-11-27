# Manajemen Vendor — Kamus Data

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `manajemen-vendor` · status `approved` — pengguna sesi, 17 November 2026 (`PBJ-DEC-062`) |
| Masukan | `02-backend-architecture.md` (approved, 17 November 2026) |

Seluruh tabel baru mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`.
Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan `IsDelete`. Schema `public`. Index unik
"aktif" berarti unik parsial dengan syarat `"IsDelete" = false`.

## 1. Status dan kepemilikan tabel

| Entity | Status | Pemilik | Catatan |
| --- | --- | --- | --- |
| `MstProcurementVendorDocumentType` | Baru | `manajemen-vendor` | Master jenis dokumen legalitas |
| `MstProcurementCategoryDocumentRequirement` | Baru | `manajemen-vendor` | Dokumen wajib per kategori; dimulai kosong |
| `MstProcurementEvaluationCriterion` | Baru | `manajemen-vendor` | Kriteria penilaian per kategori |
| `PrcVendorProfile` | Baru | `manajemen-vendor` | Pendamping `MstSupplier` |
| `PrcVendorCategory` | Baru | `manajemen-vendor` | Vendor × kategori |
| `PrcVendorDocument` | Baru | `manajemen-vendor` | Dokumen legalitas |
| `PrcVendorStatusRequest` | Baru | `manajemen-vendor` | Usulan blokir/buka blokir |
| `PrcVendorEvaluation`, `PrcVendorEvaluationScore` | Baru | `manajemen-vendor` | Penilaian berkala |
| `MstSupplier` | Sudah ada | Administrator Master Data | Skema tetap; `IsBlacklisted` hanya ditulis lewat jalur Pengadaan (`PBJ-DEC-060`) |
| `MstProcurementItemCategory` | Sudah ada | `perencanaan-permintaan` | Dirujuk sebagai kategori vendor |
| `PrcAttachment`, `PrcDocumentHistory` | Sudah ada | `perencanaan-permintaan` | Dipakai dengan jenis dokumen vendor |
| `TrxWorkflowInstance`, `MstWorkflowDefinition`, `MstApprovalMatrix` | Sudah ada | `Wfl` | Dirujuk |

## 2. Tabel baru — konfigurasi

### `MstProcurementVendorDocumentType`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `DocumentTypeCode` | `string(30)` | Ya | — | Unik (aktif, tanpa beda huruf) | — | — | Tidak | Mis. `IZIN_PBF` |
| `DocumentTypeName` | `string(150)` | Ya | — | — | — | — | Tidak | Label |
| `RequiresExpiryDate` | `bool` | Ya | `false` | — | — | — | Tidak | `true` → tanggal berlaku wajib diisi |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Nonaktif = tidak bisa dipilih untuk dokumen baru |

### `MstProcurementCategoryDocumentRequirement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `ItemCategoryId` | `Guid` | Ya | — | Unik bersama `DocumentTypeId` (aktif) | FK `MstProcurementItemCategory` | `Restrict` | Tidak | — |
| `DocumentTypeId` | `Guid` | Ya | — | idem | FK `MstProcurementVendorDocumentType` | `Restrict` | Tidak | — |
| `IsActive` | `bool` | Ya | `false` | — | — | — | Tidak | Baris dibuat nonaktif; diaktifkan bertahap (`PBJ-DEC-061`) |
| `ActivatedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Diisi saat pertama kali aktif |
| `ActivatedByUserId` | `Guid` | Tidak | — | — | — | — | Tidak | — |

### `MstProcurementEvaluationCriterion`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `ItemCategoryId` | `Guid` | Ya | — | Index; unik bersama `CriterionName` (aktif, tanpa beda huruf) | FK `MstProcurementItemCategory` | `Restrict` | Tidak | — |
| `CriterionName` | `string(150)` | Ya | — | idem | — | — | Tidak | Mis. "Ketepatan waktu" |
| `WeightPercent` | `numeric(5,2)` | Ya | — | — | — | — | Tidak | `0 < bobot ≤ 100`; CHECK |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | Panduan memberi nilai |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Total bobot aktif per kategori harus 100 saat penilaian dibuat |

## 3. Tabel baru — profil dan dokumen

### `PrcVendorProfile`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `SupplierId` | `Guid` | Ya | — | Unik (aktif) | FK `MstSupplier` | `Restrict` | Tidak | Satu profil per supplier |
| `IsRestricted` | `bool` | Ya | `false` | Index | — | — | Tidak | Status Dibatasi |
| `RestrictedUntil` | `date` | Tidak | — | — | — | — | Tidak | Kosong = sampai dicabut |
| `RestrictionReason` | `string(1000)` | Tidak | — | — | — | — | Tidak | Wajib saat `IsRestricted = true` (CHECK) |
| `RestrictedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | — |
| `RestrictedByUserId` | `Guid` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | Token kesamaan waktu |

### `PrcVendorCategory`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `VendorProfileId` | `Guid` | Ya | — | Unik bersama `ItemCategoryId` (aktif) | FK `PrcVendorProfile` | `Cascade` | Tidak | — |
| `ItemCategoryId` | `Guid` | Ya | — | Index | FK `MstProcurementItemCategory` | `Restrict` | Tidak | — |

### `PrcVendorDocument`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `VendorProfileId` | `Guid` | Ya | — | Index bersama `DocumentTypeId`, `VerificationStatus` | FK `PrcVendorProfile` | `Restrict` | Tidak | — |
| `DocumentTypeId` | `Guid` | Ya | — | idem | FK `MstProcurementVendorDocumentType` | `Restrict` | Tidak | — |
| `DocumentNumber` | `string(100)` | Tidak | — | — | — | — | Tidak | Nomor terbitan instansi |
| `IssuedDate` | `date` | Tidak | — | — | — | — | Tidak | — |
| `ExpiryDate` | `date` | Kondisional | — | Index | — | — | Tidak | Wajib bila jenisnya `RequiresExpiryDate`; ≥ `IssuedDate` |
| `VerificationStatus` | `string(20)` | Ya | `PENDING` | — | — | — | Tidak | `PENDING`, `VERIFIED`, `REJECTED`, `CANCELLED` |
| `VerifiedByUserId` | `Guid` | Tidak | — | — | — | — | Tidak | ≠ `CreateBy` (`PBJ-DEC-046`) |
| `VerifiedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | — |
| `RejectionReason` | `string(1000)` | Tidak | — | — | — | — | Tidak | Wajib bila `REJECTED` |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

Berkas dokumen disimpan di `PrcAttachment` dengan `DocumentType = PBJ_VENDOR_DOCUMENT`, `DocumentId = PrcVendorDocument.Id`.

## 4. Tabel baru — usulan status

### `PrcVendorStatusRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `RequestNumber` | `string(30)` | Tidak | — | Unik (bila terisi) | — | — | Tidak | `UBV-…` saat diajukan |
| `VendorProfileId` | `Guid` | Ya | — | Unik parsial: satu baris berstatus `DRAFT`/`PENDING_APPROVAL`/`RETURNED` per vendor | FK `PrcVendorProfile` | `Restrict` | Tidak | **[USULAN `G-V07`]** |
| `RequestType` | `string(10)` | Ya | — | — | — | — | Tidak | `BLOCK`, `UNBLOCK` |
| `Reason` | `string(1000)` | Ya | — | — | — | — | Tidak | Disalin ke `MstSupplier.BlacklistReason` saat blokir disetujui |
| `EvidenceEvaluationId` | `Guid` | Tidak | — | — | FK `PrcVendorEvaluation` | `Restrict` | Tidak | Penilaian sebagai bukti (`PBJ-DEC-048`) |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | Lihat matriks status |
| `WorkflowInstanceId` | `Guid` | Tidak | — | — | `TrxWorkflowInstance` (tanpa FK fisik, pola PR) | — | Tidak | — |
| `SubmissionCount` | `int` | Ya | `0` | — | — | — | Tidak | Kunci idempotensi `Wfl` |
| `SubmittedAt`, `DecidedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

## 5. Tabel baru — penilaian

### `PrcVendorEvaluation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `VendorProfileId` | `Guid` | Ya | — | Unik parsial bersama `ItemCategoryId`, `PeriodYear`, `PeriodType`, `PeriodNo` untuk `Status IN (DRAFT, FINAL)` dan `PreviousEvaluationId IS NULL`; revisi dijaga service | FK `PrcVendorProfile` | `Restrict` | Tidak | `PBJ-DEC-059` |
| `ItemCategoryId` | `Guid` | Ya | — | idem | FK `MstProcurementItemCategory` | `Restrict` | Tidak | Harus kategori milik vendor |
| `PeriodYear` | `int` | Ya | — | idem | — | — | Tidak | Mis. 2027 |
| `PeriodType` | `string(10)` | Ya | `SEMESTER` | idem | — | — | Tidak | **[NILAI AWAL `G-V05`]** |
| `PeriodNo` | `int` | Ya | — | idem | — | — | Tidak | Semester 1–2, kuartal 1–4, tahun 1 |
| `SourceType` | `string(10)` | Ya | `MANUAL` | — | — | — | Tidak | `CONTRACT` dicadangkan untuk `S-V7` (`PBJ-DEC-044`) |
| `Status` | `string(12)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `FINAL`, `SUPERSEDED`, `CANCELLED` |
| `FinalScore` | `numeric(4,2)` | Tidak | — | — | — | — | Tidak | Rata-rata berbobot, dihitung server |
| `Recommendation` | `string(10)` | Tidak | — | — | — | — | Tidak | Wajib saat final: `CONTINUE`, `RESTRICT`, `BLOCK` |
| `Notes` | `string(2000)` | Tidak | — | — | — | — | Tidak | — |
| `PreviousEvaluationId` | `Guid` | Tidak | — | Index | FK `PrcVendorEvaluation` | `Restrict` | Tidak | Diisi pada revisi |
| `RevisionReason` | `string(1000)` | Tidak | — | — | — | — | Tidak | Wajib pada revisi |
| `FinalizedByUserId` | `Guid` | Tidak | — | — | — | — | Tidak | ≠ `CreateBy` (`PBJ-DEC-049`) |
| `FinalizedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

### `PrcVendorEvaluationScore`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `EvaluationId` | `Guid` | Ya | — | Unik bersama `CriterionId` | FK `PrcVendorEvaluation` | `Cascade` | Tidak | — |
| `CriterionId` | `Guid` | Ya | — | idem | FK `MstProcurementEvaluationCriterion` | `Restrict` | Tidak | — |
| `CriterionName` | `string(150)` | Ya | — | — | — | — | Tidak | Disalin saat dibuat |
| `WeightPercent` | `numeric(5,2)` | Ya | — | — | — | — | Tidak | Disalin saat dibuat |
| `Score` | `int` | Tidak | — | — | — | — | Tidak | 1–5 (CHECK); wajib terisi semua sebelum final |
| `Note` | `string(500)` | Tidak | — | — | — | — | Tidak | — |

## 6. Tabel sudah ada yang dirujuk (kolom kunci)

| Tabel | Kolom yang dipakai | Model |
| --- | --- | --- |
| `MstSupplier` | `Id`, `SupplierCode`, `SupplierName`, `IsActive`, `IsBlacklisted`, `BlacklistReason`, `BankName`, `BankAccountNumber` (**Sensitif**; hanya 4 digit terakhir keluar), `BankAccountName` | `Areas/Administrator/MasterData/Models/MstSupplier.cs` |
| `MstProcurementItemCategory` | `Id`, `CategoryCode`, `CategoryName`, `IsActive` | `.../ProcurementManagement/MasterData/Models/MstProcurementItemCategory.cs` |
| `PrcAttachment` | `DocumentType`, `DocumentId`, `IsCurrent` | `.../ProcurementManagement/Common/Models/PrcAttachment.cs` |
| `PrcDocumentHistory` | `DocumentType`, `DocumentId`, `ActionCode`, `Reason` | `.../ProcurementManagement/Common/Models/PrcDocumentHistory.cs` |

## 7. Jenis dokumen pada tabel bersama

| `DocumentType` | `DocumentId` | Dipakai untuk |
| --- | --- | --- |
| `PBJ_VENDOR` | `PrcVendorProfile.Id` | Riwayat vendor (kategori, dokumen, Dibatasi, keputusan blokir, penilaian final, status awal) |
| `PBJ_VENDOR_DOCUMENT` | `PrcVendorDocument.Id` | Berkas dokumen legalitas |
| `PBJ_VENDOR_STATUS` | `PrcVendorStatusRequest.Id` | Riwayat usulan dan `ReferenceType` `Wfl` |
| `PBJ_VENDOR_EVALUATION` | `PrcVendorEvaluation.Id` | Riwayat penilaian dan lampiran pendukung |
