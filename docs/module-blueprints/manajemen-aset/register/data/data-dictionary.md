# Kamus Data — Register Aset

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`,
dan `IsDelete`. Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan `IsDelete`.

Schema seluruh tabel `public`. Enum disimpan sebagai integer. Semua FK `Restrict`. Uang `decimal(18,2)`.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstAssetCategory`, `MstAssetClassification`, `MstAssetGroup`, `MstAssetOwnershipType`, `MstAssetDocumentType`, `MstAssetCategoryDocumentRequirement`, `MstAssetLocation`, `MstAssetSetting` | Baru | Aset / `register` | Master |
| `AstAsset`, `AstAssetPlacement`, `AstAssetHandover`, `AstAssetHandoverItem`, `AstConditionReport`, `AstAssetHistory`, `AstAssetDocument`, `AstAttachment`, `AstUtilization`, `AstUtilizationItem`, `AstImportBatch`, `AstImportBatchRow` | Baru | Aset / `register` | Transaksi |
| `MstOrganizationUnit` | Sudah ada | HR MasterData | Kunci: `Id`, `UnitCode`, `UnitName`, `ParentOrganizationUnitId`, `IsActive` — `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs` |
| `MstRoom` | Sudah ada | HealthServices MasterData | Kunci: `Id`, `ServiceUnitId`, `RoomCode`, `RoomName`, `IsActive` — `Areas/HealthServices/MasterData/Models/MstRoom.cs` |
| `MstEmployee` | Sudah ada | HR | Kunci: `Id`, `EmployeeNumber`, `FullName`, `WorkforceProfileId` — `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs` |
| `WfpOrganizationAssignment` | Sudah ada | HR WorkforceCore | Kunci: `WorkforceProfileId`, `OrganizationUnitId`, `IsManagerialAssignment`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` — `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs` |
| `MstLegalEntity`, `MstHospitalSite` | Sudah ada | HR MasterData | Kunci: `Id` |
| `FinGoodsReceipt`, `FinPurchaseOrder` | Sudah ada | Finance Purchasing | Kunci: `Id`, `GRNumber`, `ReceivedDate`, `Status` — `Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceipt.cs` |

## 2. Master

### 2.1 `MstAssetCategory`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `CategoryCode` | `string(30)` | Ya | — | Unik (aktif, `IsDelete=false`) | — | — | Tidak | Contoh `ALAT_MEDIS` |
| `CategoryName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `IsDepreciable` | `bool` | Ya | `true` | — | — | — | Tidak | `false` untuk Tanah |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.2 `MstAssetClassification`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ClassificationCode` | `string(30)` | Ya | — | Unik | — | — | Tidak | `MEDIS`, `NON_MEDIS` |
| `ClassificationName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `IsMedical` | `bool` | Ya | `false` | — | — | — | Tidak | Memicu atribut wajib `AST-RULE-006` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.3 `MstAssetGroup`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `CategoryId` | `Guid` | Ya | — | Index | FK `MstAssetCategory` | Restrict | Tidak | — |
| `ClassificationId` | `Guid` | Ya | — | Index | FK `MstAssetClassification` | Restrict | Tidak | — |
| `InventoryCode` | `string(11)` | Ya | — | Unik | — | — | Tidak | 2–11 huruf besar/angka; terkunci setelah dipakai (`AST-DES-015`) |
| `GroupName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.4 `MstAssetOwnershipType`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OwnershipCode` | `string(30)` | Ya | — | Unik | — | — | Tidak | `MILIK`, `SEWA`, `KSO`, `PINJAM_PAKAI` |
| `OwnershipName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `IsCapitalizable` | `bool` | Ya | `false` | — | — | — | Tidak | `AST-RCG-G25` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.5 `MstAssetDocumentType`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `DocumentTypeCode` | `string(30)` | Ya | — | Unik | — | — | Tidak | `STNK`, `BPKB`, … |
| `DocumentTypeName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `HasExpiry` | `bool` | Ya | `false` | — | — | — | Tidak | Tanggal berakhir wajib bila `true` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.6 `MstAssetCategoryDocumentRequirement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `CategoryId` | `Guid` | Ya | — | Unik `(CategoryId, DocumentTypeId)` | FK `MstAssetCategory` | Restrict | Tidak | — |
| `DocumentTypeId` | `Guid` | Ya | — | Index | FK `MstAssetDocumentType` | Restrict | Tidak | — |

### 2.7 `MstAssetLocation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `LocationCode` | `string(30)` | Ya | — | Unik | — | — | Tidak | Contoh `IGD-RESUS` |
| `LocationName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | Unit pemilik ruangan |
| `BuildingName` | `string(100)` | Tidak | — | — | — | — | Tidak | — |
| `FloorName` | `string(50)` | Tidak | — | — | — | — | Tidak | — |
| `RoomName` | `string(100)` | Tidak | — | — | — | — | Tidak | — |
| `RoomId` | `Guid?` | Tidak | — | Index | FK `MstRoom` | Restrict | Tidak | Tautan opsional ruangan klinis |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.8 `MstAssetSetting`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `LegalEntityId` | `Guid` | Ya | — | Unik | FK `MstLegalEntity` | Restrict | Tidak | Satu baris per badan hukum |
| `HospitalSiteId` | `Guid` | Ya | — | — | FK `MstHospitalSite` | Restrict | Tidak | `AST-DEC-035` |
| `IdleThresholdDays` | `int` | Ya | `90` | — | — | — | Tidak | 1–3650 |
| `DocumentExpiryWarningDays` | `int` | Ya | `30` | — | — | — | Tidak | 1–365 |
| `UtilizationMaxMonthsForAssetHead` | `int` | Ya | `12` | — | — | — | Tidak | `AST-DEC-029` |
| `CutoverDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal awal (`AST-OQ-006`) |

## 3. Transaksi

### 3.1 `AstAsset`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `LegalEntityId` | `Guid` | Ya | — | Index | FK `MstLegalEntity` | Restrict | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | — | FK `MstHospitalSite` | Restrict | Tidak | — |
| `InventoryNumber` | `string(30)` | Tidak | — | Unik (bila tidak null) | — | — | Tidak | Terbit saat disetujui |
| `AssetGroupId` | `Guid` | Ya | — | Index | FK `MstAssetGroup` | Restrict | Tidak | — |
| `OwnershipTypeId` | `Guid` | Ya | — | Index | FK `MstAssetOwnershipType` | Restrict | Tidak | — |
| `AssetName` | `string(250)` | Ya | — | Index (pencarian) | — | — | Tidak | — |
| `Description` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `Brand` | `string(100)` | Tidak | — | — | — | — | Tidak | Wajib bila medis |
| `ModelType` | `string(100)` | Tidak | — | — | — | — | Tidak | Wajib bila medis |
| `SerialNumber` | `string(100)` | Tidak | — | Index `(Brand, SerialNumber)` | — | — | Tidak | Wajib bila medis |
| `Manufacturer` | `string(150)` | Tidak | — | — | — | — | Tidak | Wajib bila medis |
| `DistributionPermitNumber` | `string(60)` | Tidak | — | — | — | — | Tidak | AKL/AKD; wajib bila medis |
| `ManufactureYear` | `int?` | Tidak | — | — | — | — | Tidak | — |
| `DistributorName` | `string(150)` | Tidak | — | — | — | — | Tidak | — |
| `AcquisitionType` | `AssetAcquisitionType` | Ya | — | Index | — | — | Tidak | — |
| `AcquisitionDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `ReadyForUseDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Dasar awal penyusutan |
| `RegisteredAcquisitionCost` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Beku setelah disetujui (`AST-DES-002`) |
| `SupplierName` | `string(200)` | Tidak | — | — | — | — | Tidak | Snapshot nama pemasok |
| `PurchaseOrderId` | `Guid?` | Tidak | — | Index | FK `FinPurchaseOrder` | Restrict | Tidak | — |
| `GoodsReceiptId` | `Guid?` | Tidak | — | Index | FK `FinGoodsReceipt` | Restrict | Tidak | — |
| `GoodsReceiptNumber` | `string(50)` | Tidak | — | — | — | — | Tidak | Snapshot |
| `DonationReference` | `string(100)` | Tidak | — | — | — | — | Tidak | Nomor BA hibah; wajib bila `HibahMasuk` |
| `RecordStatus` | `AssetRecordStatus` | Ya | `Draft` | Index | — | — | Tidak | — |
| `Condition` | `AssetCondition` | Ya | `Aktif` | Index | — | — | Tidak | — |
| `ConditionChangedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Dasar hitung menganggur |
| `PlacementState` | `AssetPlacementState` | Ya | `BelumDitempatkan` | Index | — | — | Tidak | — |
| `CurrentOrganizationUnitId` | `Guid?` | Tidak | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | Diubah hanya `AssetPlacementService` |
| `CurrentLocationId` | `Guid?` | Tidak | — | Index | FK `MstAssetLocation` | Restrict | Tidak | — |
| `CurrentCustodianEmployeeId` | `Guid?` | Tidak | — | Index | FK `MstEmployee` | Restrict | Tidak | — |
| `ActiveProcessKind` | `AssetActiveProcessKind` | Ya | `Tidak` | Index | — | — | Tidak | `AST-DES-005` |
| `ActiveProcessId` | `Guid?` | Tidak | — | — | — | — | Tidak | Dokumen pemegang kunci |
| `SubmittedBy`, `ApprovedBy`, `RejectedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengguna |
| `SubmittedAt`, `ApprovedAt`, `RejectedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | Alasan tolak/batal |
| `SourceImportBatchId` | `Guid?` | Tidak | — | Index | FK `AstImportBatch` | Restrict | Tidak | Asal impor |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token konkurensi |

### 3.2 `AstAssetPlacement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Unik parsial `(AssetId) WHERE EndAt IS NULL AND IsDelete=false` | FK `AstAsset` | Restrict | Tidak | Satu penempatan berjalan |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `LocationId` | `Guid` | Ya | — | Index | FK `MstAssetLocation` | Restrict | Tidak | — |
| `CustodianEmployeeId` | `Guid` | Ya | — | Index | FK `MstEmployee` | Restrict | Tidak | — |
| `SourceType` | `string(30)` | Ya | — | — | — | — | Tidak | `PENYERAHAN`, `MUTASI`, `IMPOR`, `PEMULIHAN` |
| `SourceDocumentId` | `Guid?` | Tidak | — | Index | — | — | Tidak | — |
| `StartAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Saat konfirmasi |
| `EndAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |

### 3.3 `AstAssetHandover` dan `AstAssetHandoverItem`

| Kolom (`AstAssetHandover`) | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HandoverNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-PNY-######` |
| `Purpose` | `HandoverPurpose` | Ya | — | Index | — | — | Tidak | — |
| `Status` | `HandoverStatus` | Ya | `Diserahkan` | Index | — | — | Tidak | — |
| `TargetOrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `TargetLocationId` | `Guid` | Ya | — | — | FK `MstAssetLocation` | Restrict | Tidak | — |
| `TargetCustodianEmployeeId` | `Guid` | Ya | — | Index | FK `MstEmployee` | Restrict | Tidak | — |
| `SourceDocumentId` | `Guid?` | Tidak | — | — | — | — | Tidak | Hasil opname / pemulihan |
| `Notes` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `HandedOverBy`, `HandedOverAt` | `Guid`, `DateTime` | Ya | — | — | — | — | Tidak | — |
| `ConfirmedBy`, `ConfirmedAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RejectedBy`, `RejectedAt`, `CancelledBy`, `CancelledAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

| Kolom (`AstAssetHandoverItem`) | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HandoverId` | `Guid` | Ya | — | Unik `(HandoverId, AssetId)` | FK `AstAssetHandover` | Restrict | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Index | FK `AstAsset` | Restrict | Tidak | — |
| `ConditionAtHandover` | `AssetCondition` | Ya | — | — | — | — | Tidak | Snapshot untuk berita acara |

### 3.4 `AstConditionReport`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ReportNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-KND-######` |
| `AssetId` | `Guid` | Ya | — | Unik parsial `WHERE Status = Dilaporkan AND IsDelete=false` | FK `AstAsset` | Restrict | Tidak | Satu laporan terbuka |
| `Source` | `ConditionReportSource` | Ya | — | Index | — | — | Tidak | — |
| `SourceDocumentId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pinjaman / hasil opname |
| `PreviousCondition` | `AssetCondition` | Ya | — | — | — | — | Tidak | — |
| `ReportedCondition` | `AssetCondition` | Ya | — | — | — | — | Tidak | — |
| `IsFoundAgain` | `bool` | Ya | `false` | — | — | — | Tidak | Aset hilang ditemukan |
| `Notes` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `PhotoAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Restrict | Tidak | — |
| `LossReportAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Restrict | Tidak | Wajib untuk Hilang |
| `Status` | `ConditionReportStatus` | Ya | `Dilaporkan` | Index | — | — | Tidak | — |
| `ReportedBy`, `ReportedAt` | `Guid`, `DateTime` | Ya | — | — | — | — | Tidak | — |
| `DecidedCondition` | `AssetCondition?` | Tidak | — | — | — | — | Tidak | — |
| `DecidedBy`, `DecidedAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 3.5 `AstAssetHistory`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Index `(AssetId, OccurredAt)` | FK `AstAsset` | Restrict | Tidak | — |
| `EventType` | `AssetHistoryEventType` | Ya | — | Index | — | — | Tidak | — |
| `SubModule` | `string(20)` | Ya | — | Index | — | — | Tidak | `register`, `pergerakan`, … |
| `FromValue` | `string(250)` | Tidak | — | — | — | — | Tidak | Teks terbaca, tanpa rupiah |
| `ToValue` | `string(250)` | Tidak | — | — | — | — | Tidak | Teks terbaca, tanpa rupiah |
| `SourceDocumentType` | `string(40)` | Tidak | — | — | — | — | Tidak | — |
| `SourceDocumentId` | `Guid?` | Tidak | — | Index | — | — | Tidak | — |
| `SourceDocumentNumber` | `string(30)` | Tidak | — | — | — | — | Tidak | — |
| `ActorUserId` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `Reason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `OccurredAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |

### 3.6 `AstAssetDocument`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Index | FK `AstAsset` | Restrict | Tidak | — |
| `DocumentTypeId` | `Guid` | Ya | — | Index | FK `MstAssetDocumentType` | Restrict | Tidak | — |
| `DocumentNumber` | `string(100)` | Ya | — | — | — | — | **Ya** | Nomor sertifikat/BPKB |
| `IssuedDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | — |
| `ExpiryDate` | `DateOnly?` | Tidak | — | Index | — | — | Tidak | Wajib bila `HasExpiry` |
| `HolderName` | `string(200)` | Tidak | — | — | — | — | Tidak | Atas nama |
| `AttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Restrict | Tidak | — |
| `Notes` | `string(500)` | Tidak | — | — | — | — | Tidak | — |

### 3.7 `AstAttachment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OwnerType` | `string(40)` | Ya | — | Index `(OwnerType, OwnerId)` | — | — | Tidak | Contoh `ConditionReport`, `StockTakeScan` |
| `OwnerId` | `Guid` | Ya | — | — | — | — | Tidak | — |
| `OriginalFileName` | `string(255)` | Ya | — | — | — | — | Tidak | — |
| `ContentType` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `SizeBytes` | `long` | Ya | — | — | — | — | Tidak | — |
| `Sha256` | `string(64)` | Ya | — | Index | — | — | Tidak | — |
| `StoredPath` | `string(500)` | Ya | — | — | — | — | **Ya** | Tidak pernah dikirim ke frontend |

### 3.8 `AstUtilization` dan `AstUtilizationItem`

| Kolom (`AstUtilization`) | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `UtilizationNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-PMF-######` |
| `UtilizationType` | `UtilizationType` | Ya | — | Index | — | — | Tidak | — |
| `PartnerName` | `string(200)` | Ya | — | — | — | — | **Ya** | — |
| `AgreementNumber` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `AgreementValue` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Total nilai perjanjian |
| `StartDate`, `EndDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | — |
| `Purpose` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `Status` | `UtilizationStatus` | Ya | `Draft` | Index | — | — | Tidak | — |
| `RequiredApprovalLevel` | `UtilizationApprovalLevel?` | Tidak | — | — | — | — | Tidak | Dikunci saat diajukan |
| `ThresholdUsed` | `decimal(18,2)?` | Tidak | — | — | — | — | Tidak | Batas yang dipakai saat diajukan |
| `SubmittedBy`/`At`, `AssetHeadDecidedBy`/`At`, `DirectorDecidedBy`/`At`, `EndedBy`/`At` | `Guid?`/`DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `AgreementAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Restrict | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

`AstUtilizationItem`: `Id` (PK), `UtilizationId` (FK, unik bersama `AssetId`), `AssetId` (FK `AstAsset`).

### 3.9 `AstImportBatch` dan `AstImportBatchRow`

| Kolom (`AstImportBatch`) | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ImportNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-IMP-######` |
| `CutoverDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Disalin dari pengaturan saat unggah |
| `FileAttachmentId` | `Guid` | Ya | — | — | FK `AstAttachment` | Restrict | Tidak | Berkas asli |
| `Status` | `ImportBatchStatus` | Ya | `Diunggah` | Index | — | — | Tidak | — |
| `RowCount`, `ErrorCount` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `TotalAcquisitionCost`, `TotalAccumulatedDepreciation` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Ringkasan untuk Akuntansi |
| `UploadedBy`/`At`, `AssetHeadApprovedBy`/`At`, `AccountingApprovedBy`/`At`, `RejectedBy`/`At` | `Guid?`/`DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

| Kolom (`AstImportBatchRow`) | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ImportBatchId` | `Guid` | Ya | — | Unik `(ImportBatchId, RowNumber)` | FK `AstImportBatch` | Restrict | Tidak | — |
| `RowNumber` | `int` | Ya | — | — | — | — | Tidak | Nomor baris Excel |
| `RawJson` | `text` | Ya | — | — | — | — | **Ya** | Isi baris apa adanya |
| `ErrorMessages` | `string(2000)` | Tidak | — | — | — | — | Tidak | Gabungan pesan `AST-VAL-REG-062..066` |
| `CreatedAssetId` | `Guid?` | Tidak | — | Index | FK `AstAsset` | Restrict | Tidak | Terisi saat diterapkan |

## 4. Skema DDL

> **Peringatan.** Basis data dibentuk EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk
> tabel**, bukan skrip untuk dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang. Tabel master
> lain dan tabel anak sederhana mengikuti pola yang sama dan bersumber dari kolom pada bagian 2–3.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."MstAssetGroup" (
    "Id"               uuid         NOT NULL,
    "CategoryId"       uuid         NOT NULL,
    "ClassificationId" uuid         NOT NULL,
    "InventoryCode"    varchar(11)  NOT NULL,
    "GroupName"        varchar(150) NOT NULL,
    "Description"      varchar(500),
    "IsActive"         boolean      NOT NULL DEFAULT true,
    CONSTRAINT "PK_MstAssetGroup" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstAssetGroup_MstAssetCategory_CategoryId" FOREIGN KEY ("CategoryId")
        REFERENCES public."MstAssetCategory" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MstAssetGroup_MstAssetClassification_ClassificationId" FOREIGN KEY ("ClassificationId")
        REFERENCES public."MstAssetClassification" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_MstAssetGroup_InventoryCode" ON public."MstAssetGroup" ("InventoryCode")
    WHERE "IsDelete" = false;

CREATE TABLE public."AstAsset" (
    "Id"                         uuid          NOT NULL,
    "LegalEntityId"              uuid          NOT NULL,
    "HospitalSiteId"             uuid          NOT NULL,
    "InventoryNumber"            varchar(30),
    "AssetGroupId"               uuid          NOT NULL,
    "OwnershipTypeId"            uuid          NOT NULL,
    "AssetName"                  varchar(250)  NOT NULL,
    "Brand"                      varchar(100),
    "ModelType"                  varchar(100),
    "SerialNumber"               varchar(100),
    "Manufacturer"               varchar(150),
    "DistributionPermitNumber"   varchar(60),
    "AcquisitionType"            integer       NOT NULL,
    "AcquisitionDate"            date          NOT NULL,
    "ReadyForUseDate"            date          NOT NULL,
    "RegisteredAcquisitionCost"  numeric(18,2) NOT NULL DEFAULT 0,   -- SENSITIF
    "GoodsReceiptId"             uuid,
    "RecordStatus"               integer       NOT NULL,
    "Condition"                  integer       NOT NULL,
    "PlacementState"             integer       NOT NULL,
    "CurrentOrganizationUnitId"  uuid,
    "CurrentLocationId"          uuid,
    "CurrentCustodianEmployeeId" uuid,
    "ActiveProcessKind"          integer       NOT NULL DEFAULT 0,
    "ActiveProcessId"            uuid,
    "RowVersion"                 uuid          NOT NULL,
    -- kolom lain pada bagian 3.1 mengikuti pola yang sama
    CONSTRAINT "PK_AstAsset" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstAsset_MstAssetGroup_AssetGroupId" FOREIGN KEY ("AssetGroupId")
        REFERENCES public."MstAssetGroup" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AstAsset_FinGoodsReceipt_GoodsReceiptId" FOREIGN KEY ("GoodsReceiptId")
        REFERENCES public."FinGoodsReceipt" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AstAsset_MstOrganizationUnit_CurrentOrganizationUnitId" FOREIGN KEY ("CurrentOrganizationUnitId")
        REFERENCES public."MstOrganizationUnit" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_AstAsset_InventoryNumber" ON public."AstAsset" ("InventoryNumber")
    WHERE "InventoryNumber" IS NOT NULL;
CREATE INDEX "IX_AstAsset_Unit_Status" ON public."AstAsset" ("CurrentOrganizationUnitId", "RecordStatus", "Condition");

CREATE TABLE public."AstAssetPlacement" (
    "Id"                  uuid        NOT NULL,
    "AssetId"             uuid        NOT NULL,
    "OrganizationUnitId"  uuid        NOT NULL,
    "LocationId"          uuid        NOT NULL,
    "CustodianEmployeeId" uuid        NOT NULL,
    "SourceType"          varchar(30) NOT NULL,
    "SourceDocumentId"    uuid,
    "StartAt"             timestamp   NOT NULL,
    "EndAt"               timestamp,
    CONSTRAINT "PK_AstAssetPlacement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstAssetPlacement_AstAsset_AssetId" FOREIGN KEY ("AssetId")
        REFERENCES public."AstAsset" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_AstAssetPlacement_Current" ON public."AstAssetPlacement" ("AssetId")
    WHERE "EndAt" IS NULL AND "IsDelete" = false;

CREATE TABLE public."AstConditionReport" (
    "Id"                     uuid         NOT NULL,
    "ReportNumber"           varchar(30)  NOT NULL,
    "AssetId"                uuid         NOT NULL,
    "Source"                 integer      NOT NULL,
    "PreviousCondition"      integer      NOT NULL,
    "ReportedCondition"      integer      NOT NULL,
    "Status"                 integer      NOT NULL,
    "LossReportAttachmentId" uuid,
    "RowVersion"             uuid         NOT NULL,
    CONSTRAINT "PK_AstConditionReport" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstConditionReport_AstAsset_AssetId" FOREIGN KEY ("AssetId")
        REFERENCES public."AstAsset" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_AstConditionReport_Open" ON public."AstConditionReport" ("AssetId")
    WHERE "Status" = 1 AND "IsDelete" = false;
```
