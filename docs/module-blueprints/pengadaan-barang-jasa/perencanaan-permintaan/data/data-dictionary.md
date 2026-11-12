# Perencanaan & Permintaan — Kamus Data

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `perencanaan-permintaan` · status `approved` (`PBJ-DEC-034`) |
| Masukan | `02-backend-architecture.md` revisi 2 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan
`IsDelete`. Kolom-kolom itu tidak diulang di bawah.

Penghapusan bersifat penandaan lewat `IsDelete`, bukan penghapusan baris. Semua tabel memakai schema
`public`, dan semua nominal memakai `numeric(18,2)`, bukan bilangan pecahan biasa. Prefix `Prc` terdaftar
`ACTIVE` di registry (`PBJ-DEC-035`).

## 1. Status dan kepemilikan tabel

| Entity | Status | Pemilik | Catatan |
| --- | --- | --- | --- |
| `MstProcurementType` | Baru | `perencanaan-permintaan` | Master |
| `MstProcurementItemCategory` | Baru | `perencanaan-permintaan` | Master |
| `MstProcurementFundSource` | Baru | `perencanaan-permintaan` | Master |
| `MstProcurementItem` | Baru | `perencanaan-permintaan` | Katalog |
| `PrcNeedPlan`, `PrcNeedPlanLine` | Baru | `perencanaan-permintaan` | — |
| `PrcPackagePlan`, `PrcPackagePlanLine` | Baru | `perencanaan-permintaan` | — |
| `PrcBudget`, `PrcBudgetCommitment` | Baru | `perencanaan-permintaan` | Komitmen ditulis sub-modul lain lewat service |
| `PrcPurchaseRequest`, `PrcPurchaseRequestLine` | Baru | `perencanaan-permintaan` | — |
| `PrcAttachment`, `PrcDocumentHistory` | Baru | `perencanaan-permintaan` | Dipakai semua sub-modul |
| `MstWorkflowStep` | Diperbarui | WorkflowManagement (`Wfl`) | Satu kolom baru |
| `MstDepartment` | Sudah ada | HR Organisasi | Dirujuk, tidak disalin |
| `AspNetUserOrganization` | Sudah ada | Platform Authorization / Workforce | Dirujuk |
| `MstMeasurement`, `MstDrug` | Sudah ada | Health Services Master Data | Dirujuk |
| `TrxWorkflowInstance`, `MstWorkflowDefinition`, `MstApprovalMatrix` | Sudah ada | `Wfl` | Dirujuk |

## 2. Tabel baru — master

### `MstProcurementType`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `TypeCode` | `string(30)` | Ya | — | Unik (aktif) | — | — | Tidak | `STOCK`, `ASSET`, `NON_STOCK`, `SERVICE` |
| `TypeName` | `string(100)` | Ya | — | — | — | — | Tidak | Label |
| `IsGoods` | `bool` | Ya | `true` | — | — | — | Tidak | `false` untuk jasa |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### `MstProcurementItemCategory` dan `MstProcurementFundSource`

Bentuknya sama: `Id` (PK), `CategoryCode`/`FundSourceCode` `string(30)` (unik di antara baris aktif),
`CategoryName`/`FundSourceName` `string(150)`, `Description` `string(500)?`, `IsActive bool`. **Tanpa `SortOrder`**: kontrak engineering melarang `SortOrder` generik yang dipersistensi untuk kode baru (ditemukan saat `BE-PBJ-003`); urutan memakai nama.
Tidak ada kolom sensitif.

### `MstProcurementItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `ItemCode` | `string(30)` | Ya | dari NumberSeries | Unik | — | — | Tidak | `KTL-000045` |
| `ItemName` | `string(200)` | Ya | — | Index | — | — | Tidak | "Kursi Roda Standar" |
| `ProcurementTypeId` | `Guid` | Ya | — | Index | FK `MstProcurementType` | `Restrict` | Tidak | — |
| `ItemCategoryId` | `Guid` | Ya | — | Index | FK `MstProcurementItemCategory` | `Restrict` | Tidak | — |
| `MeasurementId` | `Guid` | Ya | — | Index | FK `MstMeasurement` | `Restrict` | Tidak | Satuan |
| `DrugId` | `Guid?` | Tidak | — | Index | FK `MstDrug` | `Restrict` | Tidak | Hanya barang (`PBJ-VAL-072`) |
| `DefaultSpecification` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

Unik tambahan: `(lower(ItemName), ProcurementTypeId, ItemCategoryId, MeasurementId)` di antara baris tidak
dihapus (`PBJ-VAL-070`).

## 3. Tabel baru — rencana dan pagu

### `PrcNeedPlan`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `PlanNumber` | `string(30)?` | Tidak | — | Unik (bila terisi) | — | — | Tidak | Diisi saat versi 1 diajukan; versi revisi memakai nomor sama |
| `FiscalYear` | `int` | Ya | — | Index | — | — | Tidak | — |
| `DepartmentId` | `Guid` | Ya | — | Index | FK `MstDepartment` | `Restrict` | Tidak | Unit |
| `VersionNo` | `int` | Ya | `1` | — | — | — | Tidak | — |
| `PreviousVersionId` | `Guid?` | Tidak | — | Index | FK `PrcNeedPlan` | `Restrict` | Tidak | — |
| `IsCurrentVersion` | `bool` | Ya | `false` | Unik parsial `(DepartmentId, FiscalYear) WHERE IsCurrentVersion AND NOT IsDelete` | — | — | Tidak | Hanya satu versi berlaku |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | Lihat state matrix |
| `RevisionReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Wajib pada versi ≥ 2 |
| `EstimatedTotal` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | Σ baris, dihitung server |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | ke `TrxWorkflowInstance` (tanpa FK fisik lintas modul) | — | Tidak | — |
| `SubmittedAt`, `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | Konkurensi |

### `PrcNeedPlanLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `NeedPlanId` | `Guid` | Ya | — | Index | FK `PrcNeedPlan` | `Cascade` (baris milik kepala) | Tidak | — |
| `LineKey` | `Guid` | Ya | `NewGuid` pada versi 1; disalin pada revisi | Unik `(NeedPlanId, LineKey)`; index `LineKey` | — | — | Tidak | Identitas baris lintas versi |
| `ItemId` | `Guid` | Ya | — | Unik `(NeedPlanId, ItemId)` | FK `MstProcurementItem` | `Restrict` | Tidak | `PBJ-VAL-044` |
| `Quantity` | `numeric(18,4)` | Ya | — | — | — | — | Tidak | > 0 |
| `EstimatedUnitPrice` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | ≥ 0 |
| `EstimatedTotal` | `numeric(18,2)` | Ya | — | — | — | — | Tidak | `Quantity × EstimatedUnitPrice`, server |
| `NeededMonth` | `int?` | Tidak | — | — | — | — | Tidak | 1–12 |
| `FundSourceId` | `Guid?` | Tidak | — | — | FK `MstProcurementFundSource` | `Restrict` | Tidak | — |
| `Justification` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `Specification` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |

### `PrcPackagePlan`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `PackagePlanNumber` | `string(30)?` | Tidak | — | Unik (bila terisi) | — | — | Tidak | Diisi saat diajukan |
| `FiscalYear` | `int` | Ya | — | Index | — | — | Tidak | — |
| `Title` | `string(200)` | Ya | — | — | — | — | Tidak | "Alat Bantu Mobilitas 2027" |
| `ProcurementTypeId` | `Guid` | Ya | — | — | FK `MstProcurementType` | `Restrict` | Tidak | `PBJ-VAL-051` |
| `IndicativeMethod` | `string(100)?` | Tidak | — | — | — | — | Tidak | Teks bebas sampai `PBJ-OQ-005` diputuskan |
| `TargetMonth` | `int?` | Tidak | — | — | — | — | Tidak | — |
| `EstimatedValue` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | Σ alokasi × harga estimasi baris rencana, server |
| `PicUserId` | `Guid?` | Tidak | — | Index | — | — | Tidak | PIC |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | — |
| `CancelReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

### `PrcPackagePlanLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `PackagePlanId` | `Guid` | Ya | — | Index | FK `PrcPackagePlan` | `Cascade` | Tidak | — |
| `NeedPlanLineKey` | `Guid` | Ya | — | Index; unik `(PackagePlanId, NeedPlanLineKey)` | ke `PrcNeedPlanLine.LineKey` (tanpa FK fisik) | — | Tidak | — |
| `DepartmentId` | `Guid` | Ya | — | Index | FK `MstDepartment` | `Restrict` | Tidak | Unit asal tersimpan (konsolidasi) |
| `ItemId` | `Guid` | Ya | — | Index | FK `MstProcurementItem` | `Restrict` | Tidak | — |
| `AllocatedQuantity` | `numeric(18,4)` | Ya | — | — | — | — | Tidak | `PBJ-VAL-050` |

### `PrcBudget`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `FiscalYear` | `int` | Ya | — | Index | — | — | Tidak | — |
| `DepartmentId` | `Guid` | Ya | — | Index | FK `MstDepartment` | `Restrict` | Tidak | — |
| `ItemCategoryId` | `Guid` | Ya | — | — | FK `MstProcurementItemCategory` | `Restrict` | Tidak | — |
| `FundSourceId` | `Guid` | Ya | — | — | FK `MstProcurementFundSource` | `Restrict` | Tidak | — |
| `Amount` | `numeric(18,2)` | Ya | — | — | — | — | **Ya** (rahasia bisnis) | > 0 |
| `VersionNo` | `int` | Ya | `1` | — | — | — | Tidak | — |
| `PreviousVersionId` | `Guid?` | Tidak | — | — | FK `PrcBudget` | `Restrict` | Tidak | — |
| `LineageKey` | `Guid` | Ya | `Id` versi 1; disalin pada revisi | Index | — | — | Tidak | Garis keturunan pagu, kunci komitmen |
| `IsCurrentVersion` | `bool` | Ya | `false` | Unik parsial `(FiscalYear, DepartmentId, ItemCategoryId, FundSourceId) WHERE IsCurrentVersion AND NOT IsDelete` | — | — | Tidak | `PBJ-VAL-060` |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | — |
| `RevisionReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

### `PrcBudgetCommitment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `BudgetLineageKey` | `Guid` | Ya | — | Index | — | — | Tidak | `Id` versi 1 pagu; komitmen tetap menempel saat pagu direvisi |
| `SourceDocumentType` | `string(40)` | Ya | — | Unik parsial `(BudgetLineageKey, SourceDocumentType, SourceDocumentId) WHERE IsActive` | — | — | Tidak | mis. `PBJ_AWARD` (kelak) |
| `SourceDocumentId` | `Guid` | Ya | — | idem | — | — | Tidak | — |
| `Amount` | `numeric(18,2)` | Ya | — | — | — | — | **Ya** | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | `false` saat dilepas |
| `ReleasedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |

> Komitmen menunjuk **garis keturunan pagu** (`BudgetLineageKey`), bukan `Id` versi, agar komitmen tidak
> "hilang" ketika pagu direvisi. **Contoh:** pagu v1 (lineage `A`) Rp 500 juta, komitmen Rp 120 juta. Revisi
> v2 Rp 600 juta tetap ber-lineage `A`, sehingga sisa = Rp 600 juta − Rp 120 juta = Rp 480 juta.
## 4. Tabel baru — permintaan pengadaan

### `PrcPurchaseRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `RequestNumber` | `string(30)?` | Tidak | — | Unik (bila terisi) | — | — | Tidak | Saat pertama diajukan |
| `DepartmentId` | `Guid` | Ya | — | Index | FK `MstDepartment` | `Restrict` | Tidak | Unit peminta |
| `RequesterUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Pembuat |
| `FiscalYear` | `int` | Ya | — | Index | — | — | Tidak | — |
| `FundSourceId` | `Guid` | Ya | — | — | FK `MstProcurementFundSource` | `Restrict` | Tidak | — |
| `NeededByDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `UrgencyLevel` | `string(20)` | Ya | `NORMAL` | Index | — | — | Tidak | `NORMAL`, `URGENT` |
| `UrgencyReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Wajib bila `URGENT` |
| `UrgencyDeadline` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Wajib bila `URGENT` |
| `Justification` | `string(2000)` | Ya | — | — | — | — | **Ya** | — |
| `Status` | `string(30)` | Ya | `DRAFT` | Index `(Status, DepartmentId)` | — | — | Tidak | — |
| `IsOutOfPlan` | `bool` | Ya | `false` | Index | — | — | Tidak | Ada baris dengan kuantitas di luar rencana |
| `RequestedTotal` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | Σ diminta × harga |
| `PurchaseTotal` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | Nominal approval (`PBJ-DEC-027`) |
| `IsOverBudget` | `bool` | Ya | `false` | Index | — | — | Tidak | `PBJ-DEC-030` |
| `OverBudgetAmount` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | — |
| `SubmissionCount` | `int` | Ya | `0` | — | — | — | Tidak | Naik setiap diajukan; bagian `IdempotencyKey` `Wfl` |
| `SubmittedAt`, `VerifiedAt`, `ApprovedAt`, `ClosedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `VerifiedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Instance terakhir |
| `ConvertedPackageId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Diisi `pemilihan-penyedia` |
| `CancelReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `RowVersion` | `Guid` | Ya | `NewGuid` | — | — | — | Tidak | — |

### `PrcPurchaseRequestLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `PurchaseRequestId` | `Guid` | Ya | — | Index | FK `PrcPurchaseRequest` | `Cascade` | Tidak | — |
| `LineNo` | `int` | Ya | — | Unik `(PurchaseRequestId, LineNo)` | — | — | Tidak | — |
| `ItemId` | `Guid` | Ya | — | Index | FK `MstProcurementItem` | `Restrict` | Tidak | — |
| `Specification` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `RequestedQuantity` | `numeric(18,4)` | Ya | — | — | — | — | Tidak | > 0 |
| `EstimatedUnitPrice` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | ≥ 0 |
| `NeedPlanLineKey` | `Guid?` | Tidak | — | Index | ke `PrcNeedPlanLine.LineKey` | — | Tidak | — |
| `InPlanQuantity` | `numeric(18,4)` | Ya | `0` | — | — | — | Tidak | Dihitung saat diajukan |
| `OutOfPlanQuantity` | `numeric(18,4)` | Ya | `0` | — | — | — | Tidak | Diminta − dalam rencana |
| `OutOfPlanReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | `PBJ-VAL-009` |
| `StockAvailableQuantity` | `numeric(18,4)?` | Tidak | — | — | — | — | Tidak | `null` = belum dicek |
| `StockFulfilledQuantity` | `numeric(18,4)?` | Tidak | — | — | — | — | Tidak | — |
| `StockSourceNote` | `string(300)?` | Tidak | — | — | — | — | Tidak | Gudang/sumber (teks, `PBJ-DEC-008`) |
| `StockCheckedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `StockCheckedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `PurchaseQuantity` | `numeric(18,4)` | Ya | `0` | — | — | — | Tidak | Diminta − dipenuhi stok, server |
| `PurchaseAmount` | `numeric(18,2)` | Ya | `0` | — | — | — | Tidak | Kebutuhan beli × harga |

## 5. Tabel baru — dokumen bersama

### `PrcAttachment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `DocumentType` | `string(40)` | Ya | — | Index `(DocumentType, DocumentId)` | — | — | Tidak | `PBJ_PR`, dst. |
| `DocumentId` | `Guid` | Ya | — | idem | — | — | Tidak | — |
| `AttachmentCategory` | `string(50)?` | Tidak | — | — | — | — | Tidak | mis. "Foto kondisi", "KAK" |
| `AttachmentKey` | `Guid` | Ya | `NewGuid` (versi 1) | Index | — | — | Tidak | Identitas lampiran lintas versi |
| `VersionNo` | `int` | Ya | `1` | Unik `(AttachmentKey, VersionNo)` | — | — | Tidak | — |
| `IsCurrent` | `bool` | Ya | `true` | — | — | — | Tidak | — |
| `OriginalFileName` | `string(255)` | Ya | — | — | — | — | **Ya** | — |
| `StoredFileName` | `string(100)` | Ya | — | Unik | — | — | Tidak | `Guid` + ekstensi |
| `RelativePath` | `string(500)` | Ya | — | — | — | — | Tidak | Di bawah `Procurement:Attachment:RelativeRootPath` |
| `ContentType` | `string(100)` | Ya | — | — | — | — | Tidak | Ditentukan server dari tanda tangan berkas |
| `FileSizeBytes` | `long` | Ya | — | — | — | — | Tidak | — |
| `Sha256` | `string(64)` | Ya | — | Index | — | — | Tidak | Deteksi berkas sama |
| `UploadedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | — |

### `PrcDocumentHistory`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid` | PK | — | — | Tidak | — |
| `DocumentType` | `string(40)` | Ya | — | Index `(DocumentType, DocumentId, OccurredAt)` | — | — | Tidak | — |
| `DocumentId` | `Guid` | Ya | — | idem | — | — | Tidak | — |
| `DocumentNumber` | `string(30)?` | Tidak | — | Index | — | — | Tidak | Untuk pencarian riwayat |
| `ActionCode` | `string(40)` | Ya | — | — | — | — | Tidak | `CREATE`, `SUBMIT`, `STOCK_CHECK`, `VERIFY_COMPLETE`, `RETURN`, `REJECT`, `CANCEL`, `REVISE`, `WF_APPROVED`, `ATTACH`, … |
| `FromStatus`, `ToStatus` | `string(30)?` | Tidak | — | — | — | — | Tidak | — |
| `Reason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `SnapshotJson` | `jsonb?` | Tidak | — | — | — | — | **Ya** | Cuplikan nominal dan kuantitas saat aksi |
| `ActorUserId` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `OccurredAt` | `DateTimeOffset` | Ya | `UtcNow` | idem | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | — | — | — | Tidak | Bila berasal dari `Wfl` |

## 6. Tabel diperbarui

### `MstWorkflowStep` (`Wfl`)

Kolom yang sudah ada: lihat `BE/Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowStep.cs`
(`Id`, `WorkflowDefinitionId`, `StepCode`, `StepName`, `StepOrder`, `ApproverSourceType`, `ApproverRoleCode`,
`IsRequired`, `AllowSelfApproval`, `IsActive`, dll.). Seluruh kolom lama **tidak berubah**.

| Kolom baru | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `ActivationConditionCode` | `string(50)?` | Tidak | `null` | — | Tidak | `null` = langkah selalu aktif (perilaku sekarang). Diisi = aktif hanya bila `RequestContext.conditionCodes` memuatnya |

## 7. Tabel sudah ada yang dirujuk (kolom kunci)

| Tabel | Kolom yang dipakai | File |
| --- | --- | --- |
| `MstDepartment` | `Id`, `DepartmentCode`, `DepartmentName`, `IsActive` | `BE/Areas/Corporate/HumanResource/MasterData/Organization/Models/MstDepartment.cs` |
| `AspNetUserOrganization` | `UserId`, `DepartmentId`, `IsActive`, `EffectiveStartDate`, `EffectiveEndDate` | `BE/Models/ApplicationUserOrganization.cs` |
| `MstMeasurement` | `Id`, `MeasurementCode`, `MeasurementName`, `IsDecimalAllowed` | `BE/Areas/HealthServices/MasterData/Models/MstMeasurement.cs` |
| `MstDrug` | `Id` | `BE/Areas/HealthServices/MasterData/Models/MstDrug.cs` |
| `TrxWorkflowInstance` | `Id`, `ReferenceType`, `ReferenceId`, `WorkflowStatus`, `RequestedByUserId`, `IdempotencyKey` | `BE/Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs` |

## 8. Bentuk DDL

> **Peringatan:** basis data dibentuk EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk tabel**,
> bukan skrip untuk dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
ALTER TABLE public."MstWorkflowStep" ADD COLUMN "ActivationConditionCode" varchar(50) NULL;

CREATE TABLE public."PrcPurchaseRequest" (
    "Id"               uuid           NOT NULL,
    "RequestNumber"    varchar(30),
    "DepartmentId"     uuid           NOT NULL,
    "RequesterUserId"  uuid           NOT NULL,
    "FiscalYear"       integer        NOT NULL,
    "FundSourceId"     uuid           NOT NULL,
    "NeededByDate"     date           NOT NULL,
    "UrgencyLevel"     varchar(20)    NOT NULL DEFAULT 'NORMAL',
    "UrgencyReason"    varchar(1000),           -- SENSITIF
    "UrgencyDeadline"  date,
    "Justification"    varchar(2000)  NOT NULL, -- SENSITIF
    "Status"           varchar(30)    NOT NULL DEFAULT 'DRAFT',
    "IsOutOfPlan"      boolean        NOT NULL DEFAULT false,
    "RequestedTotal"   numeric(18,2)  NOT NULL DEFAULT 0,
    "PurchaseTotal"    numeric(18,2)  NOT NULL DEFAULT 0,
    "IsOverBudget"     boolean        NOT NULL DEFAULT false,
    "OverBudgetAmount" numeric(18,2)  NOT NULL DEFAULT 0,
    "SubmissionCount"  integer        NOT NULL DEFAULT 0,
    "WorkflowInstanceId" uuid,
    "ConvertedPackageId" uuid,
    "CancelReason"     varchar(1000),           -- SENSITIF
    "RowVersion"       uuid           NOT NULL,
    -- SubmittedAt, VerifiedAt, VerifiedByUserId, ApprovedAt, ClosedAt: timestamptz/uuid NULL
    CONSTRAINT "PK_PrcPurchaseRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PrcPurchaseRequest_MstDepartment_DepartmentId" FOREIGN KEY ("DepartmentId")
        REFERENCES public."MstDepartment" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrcPurchaseRequest_MstProcurementFundSource_FundSourceId" FOREIGN KEY ("FundSourceId")
        REFERENCES public."MstProcurementFundSource" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_PrcPurchaseRequest_RequestNumber" ON public."PrcPurchaseRequest" ("RequestNumber")
    WHERE "RequestNumber" IS NOT NULL AND "IsDelete" = false;
CREATE INDEX "IX_PrcPurchaseRequest_Status_DepartmentId" ON public."PrcPurchaseRequest" ("Status", "DepartmentId");

CREATE TABLE public."PrcPurchaseRequestLine" (
    "Id" uuid NOT NULL, "PurchaseRequestId" uuid NOT NULL, "LineNo" integer NOT NULL,
    "ItemId" uuid NOT NULL, "Specification" varchar(1000),
    "RequestedQuantity" numeric(18,4) NOT NULL, "EstimatedUnitPrice" numeric(18,2) NOT NULL DEFAULT 0,
    "NeedPlanLineKey" uuid, "InPlanQuantity" numeric(18,4) NOT NULL DEFAULT 0,
    "OutOfPlanQuantity" numeric(18,4) NOT NULL DEFAULT 0, "OutOfPlanReason" varchar(1000), -- SENSITIF
    "StockAvailableQuantity" numeric(18,4), "StockFulfilledQuantity" numeric(18,4),
    "StockSourceNote" varchar(300), "StockCheckedAt" timestamptz, "StockCheckedByUserId" uuid,
    "PurchaseQuantity" numeric(18,4) NOT NULL DEFAULT 0, "PurchaseAmount" numeric(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT "PK_PrcPurchaseRequestLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PrcPurchaseRequestLine_PrcPurchaseRequest_PurchaseRequestId" FOREIGN KEY ("PurchaseRequestId")
        REFERENCES public."PrcPurchaseRequest" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_PrcPurchaseRequestLine_MstProcurementItem_ItemId" FOREIGN KEY ("ItemId")
        REFERENCES public."MstProcurementItem" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_PrcPurchaseRequestLine_Quantity" CHECK ("RequestedQuantity" > 0)
);
CREATE UNIQUE INDEX "IX_PrcPurchaseRequestLine_Request_LineNo" ON public."PrcPurchaseRequestLine" ("PurchaseRequestId", "LineNo");
CREATE INDEX "IX_PrcPurchaseRequestLine_NeedPlanLineKey" ON public."PrcPurchaseRequestLine" ("NeedPlanLineKey");

CREATE UNIQUE INDEX "IX_PrcNeedPlan_CurrentVersion" ON public."PrcNeedPlan" ("DepartmentId", "FiscalYear")
    WHERE "IsCurrentVersion" = true AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_PrcBudget_CurrentVersion" ON public."PrcBudget"
    ("FiscalYear", "DepartmentId", "ItemCategoryId", "FundSourceId")
    WHERE "IsCurrentVersion" = true AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_PrcBudgetCommitment_ActiveSource" ON public."PrcBudgetCommitment"
    ("BudgetLineageKey", "SourceDocumentType", "SourceDocumentId") WHERE "IsActive" = true;
```

Tabel lain mengikuti kolom pada bagian 2–5 dengan pola yang sama: PK `Id`, FK `RESTRICT` ke master, dan
`CASCADE` hanya dari baris ke kepala dokumennya sendiri.
