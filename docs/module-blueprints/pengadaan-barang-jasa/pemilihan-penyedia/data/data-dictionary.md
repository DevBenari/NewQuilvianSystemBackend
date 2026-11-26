# Pemilihan Penyedia — Kamus Data

| Field | Nilai |
| --- | --- |
| Sub-modul | `pemilihan-penyedia` (`PBJ-PS`), status `draft` |
| Masukan | `02-backend-architecture.md` (draft); `PBJ-DEC-066`..`085`; `PBJ-ASM-005`..`009` |
| Schema | `public`; nama tabel tunggal PascalCase sama dengan nama class |

Seluruh tabel mewarisi `IdentityModel` (`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`,
`DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`). Kolom itu tidak diulang di bawah. Hapus = penandaan
`IsDelete`; index unik di bawah selalu berlaku untuk baris `IsDelete = false`. Semua relasi `DeleteBehavior.Restrict`
kecuali ditulis lain. Semua angka uang `numeric(18,2)` **termasuk PPN** kecuali nama kolomnya `...BeforeTax` (`PBJ-DEC-084`).

Penanda **Sensitif**: `S` = harga/HPS (tertutup bagi evaluator, `PBJ-DEC-079`, tidak masuk payload log); `—` = tidak sensitif.

## 1. Status dan kepemilikan tabel

| Tabel | Status | Pemilik |
| --- | --- | --- |
| `MstProcurementMethod` | Baru | `pemilihan-penyedia` |
| `MstProcurementPackageDocumentType` | Baru | `pemilihan-penyedia` |
| `MstProcurementPackageDocumentRequirement` | Baru | `pemilihan-penyedia` |
| `PrcPackage`, `PrcPackageItem`, `PrcPackageItemSource`, `PrcPackageChecklistItem`, `PrcPackageEvaluator` | Baru | `pemilihan-penyedia` |
| `PrcRfq`, `PrcRfqInvitation`, `PrcQuotation`, `PrcQuotationLine` | Baru | `pemilihan-penyedia` |
| `PrcTechnicalCriterion`, `PrcTechnicalEvaluationResult`, `PrcNegotiation`, `PrcNegotiationLine` | Baru | `pemilihan-penyedia` |
| `PrcPackageAward` | Baru | `pemilihan-penyedia` |
| `PrcPurchaseRequestLine` | **Diperbarui** | `perencanaan-permintaan` (ditulis hanya lewat service pemilik) |
| `PrcPurchaseRequest`, `PrcPackagePlan`, `PrcBudgetCommitment`, `MstProcurementItem`, `MstProcurementType`, `MstSupplier`, `PrcAttachment`, `PrcDocumentHistory` | Sudah ada | Lihat bagian 6 |

## 2. Master

### `MstProcurementMethod`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `MethodCode` | `varchar(30)` | Ya | — | Unik | huruf besar, angka, `_` | — |
| `MethodName` | `varchar(150)` | Ya | — | — | — | — |
| `MinHpsAmount` | `numeric(18,2)` | Tidak | — | — | ≥ 0; < `MaxHpsAmount` bila keduanya terisi | — |
| `MaxHpsAmount` | `numeric(18,2)` | Tidak | — | — | > 0 | — |
| `MinimumQuotationCount` | `int` | Ya | `1` | — | 1–10 | — |
| `RequiresTechnicalEvaluation` | `bool` | Ya | `false` | — | — | — |
| `IsSample` | `bool` | Ya | `false` | — | `true` hanya dari seeder contoh (`PBJ-DEC-069`) | — |
| `Description` | `varchar(500)` | Tidak | — | — | — | — |
| `SortOrder` | `int` | Ya | `0` | — | — | — |
| `IsActive` | `bool` | Ya | `true` | — | Rentang metode aktif tidak tumpang tindih **[USULAN]** | — |

### `MstProcurementPackageDocumentType`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `DocumentTypeCode` | `varchar(30)` | Ya | — | Unik | dipakai sebagai `PrcAttachment.AttachmentCategory` | — |
| `DocumentTypeName` | `varchar(150)` | Ya | — | — | — | — |
| `Stage` | `varchar(20)` | Ya | `PREPARATION` | — | `PREPARATION`/`QUOTATION`/`NEGOTIATION` | — |
| `IsPriceSensitive` | `bool` | Ya | `false` | — | — | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — |

### `MstProcurementPackageDocumentRequirement`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `DocumentTypeId` | `uuid` | Ya | — | FK `MstProcurementPackageDocumentType` | aktif | — |
| `MethodId` | `uuid` | Tidak | — | FK `MstProcurementMethod` | tepat satu dari `MethodId`/`ProcurementTypeId` terisi | — |
| `ProcurementTypeId` | `uuid` | Tidak | — | FK `MstProcurementType` | idem | — |
| `IsActive` | `bool` | Ya | `true` | Unik (`DocumentTypeId`, `MethodId`, `ProcurementTypeId`) | — | — |

## 3. Paket

### `PrcPackage`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageNumber` | `varchar(20)` | Tidak | — | Unik bila terisi | dialokasikan saat persiapan pertama diajukan | — |
| `Title` | `varchar(200)` | Ya | — | — | — | — |
| `ProcurementTypeId` | `uuid` | Ya | — | FK `MstProcurementType`, index | sama untuk semua item **[USULAN `G-P11`]** | — |
| `FiscalYear` | `int` | Ya | — | index | sama untuk semua sumber **[USULAN `G-P12`]** | — |
| `PackagePlanId` | `uuid` | Tidak | — | FK `PrcPackagePlan` | rujukan saja (`PBJ-DEC-082`) | — |
| `SplitFromPackageId` | `uuid` | Tidak | — | FK `PrcPackage` | terisi pada paket hasil pecah | — |
| `Status` | `varchar(40)` | Ya | `DRAFT` | index | `state-transition-matrix.md` bagian 1 | — |
| `MethodId` | `uuid` | Tidak | — | FK `MstProcurementMethod` | wajib saat persiapan diajukan | — |
| `MethodCode`, `MethodName` | `varchar(30)`, `varchar(150)` | Tidak | — | — | salinan saat dipilih | — |
| `MethodMinHpsAmount`, `MethodMaxHpsAmount` | `numeric(18,2)` | Tidak | — | — | salinan | — |
| `MethodMinimumQuotationCount` | `int` | Tidak | — | — | salinan | — |
| `MethodRequiresTechnicalEvaluation` | `bool` | Ya | `false` | — | salinan | — |
| `IsMethodOutOfRange` | `bool` | Ya | `false` | — | dihitung server | — |
| `MethodDeviationReason` | `varchar(500)` | Tidak | — | — | wajib bila `IsMethodOutOfRange` | — |
| `HpsTotal` | `numeric(18,2)` | Ya | `0` | — | = Σ `PrcPackageItem.HpsAmount` | S |
| `ApprovedHpsTotal` | `numeric(18,2)` | Tidak | — | — | terkunci saat persiapan disetujui | S |
| `PreparationWorkflowInstanceId` | `uuid` | Tidak | — | index | — | — |
| `PreparationSubmissionCount` | `int` | Ya | `0` | — | — | — |
| `PreparationSubmittedAt`, `PreparationApprovedAt` | `timestamptz` | Tidak | — | — | — | — |
| `PicUserId` | `uuid` | Ya | pembuat | index | penanggung jawab paket | — |
| `PurchaseOrderNumber` | `varchar(50)` | Tidak | — | **Unik bila terisi** (`PBJ-DEC-011`) | hanya saat `AWARDED` | — |
| `PurchaseOrderRecordedAt` | `timestamptz` | Tidak | — | — | — | — |
| `PurchaseOrderRecordedByUserId` | `uuid` | Tidak | — | — | — | — |
| `CancelReason` | `varchar(500)` | Tidak | — | — | wajib saat batal/pecah tercatat di riwayat | — |
| `RowVersion` | `uuid` | Ya | baru | concurrency token | — | — |

### `PrcPackageItem`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`, Unik (`PackageId`, `ItemId`) | satu baris per kode katalog | — |
| `LineNo` | `int` | Ya | — | — | — | — |
| `ItemId` | `uuid` | Ya | — | FK `MstProcurementItem` | — | — |
| `ItemCategoryId` | `uuid` | Ya | — | index | salinan kategori barang saat dibuat | — |
| `Specification` | `varchar(2000)` | Tidak | dari PR/katalog | — | — | — |
| `Quantity` | `numeric(18,3)` | Ya | — | — | = Σ sumber aktif | — |
| `HpsUnitPrice` | `numeric(18,2)` | Ya | harga perkiraan PR | — | > 0 saat diajukan | S |
| `HpsAmount` | `numeric(18,2)` | Ya | — | — | = round(`Quantity` × `HpsUnitPrice`, 2) | S |

### `PrcPackageItemSource`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageItemId` | `uuid` | Ya | — | FK `PrcPackageItem` | — | — |
| `PurchaseRequestLineId` | `uuid` | Ya | — | FK `PrcPurchaseRequestLine`; **Unik bila `IsReleased = false`** | satu baris PR di satu paket hidup (`PBJ-AC-026`) | — |
| `PurchaseRequestId` | `uuid` | Ya | — | FK `PrcPurchaseRequest`, index | — | — |
| `DepartmentId` | `uuid` | Ya | — | index | salinan PR, kunci garis pagu dan cakupan unit | — |
| `FundSourceId` | `uuid` | Ya | — | — | salinan PR | — |
| `Quantity` | `numeric(18,3)` | Ya | — | — | = `PurchaseQuantity` baris PR | — |
| `IsReleased` | `bool` | Ya | `false` | — | `true` saat paket batal/ditolak atau baris dilepas | — |
| `ReleasedAt` | `timestamptz` | Tidak | — | — | — | — |

### `PrcPackageChecklistItem`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`, Unik (`PackageId`, `DocumentTypeId`) | — | — |
| `DocumentTypeId` | `uuid` | Ya | — | FK `MstProcurementPackageDocumentType` | — | — |
| `DocumentTypeCode`, `DocumentTypeName` | `varchar(30)`, `varchar(150)` | Ya | — | — | salinan | — |
| `IsRequired` | `bool` | Ya | `true` | — | — | — |

### `PrcPackageEvaluator`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`, Unik (`PackageId`, `EvaluatorUserId`) bila aktif | — | — |
| `EvaluatorUserId` | `uuid` | Ya | — | index | pengguna aktif | — |
| `AssignedByUserId` | `uuid` | Ya | — | — | — | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — |

## 4. RFQ, penawaran, evaluasi, negosiasi

### `PrcRfq`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`, index | satu RFQ `DRAFT`/`SENT` per paket | — |
| `RfqNumber` | `varchar(20)` | Tidak | — | Unik bila terisi | saat dikirim | — |
| `Status` | `varchar(20)` | Ya | `DRAFT` | index | — | — |
| `SentAt` | `timestamptz` | Tidak | — | — | — | — |
| `Deadline` | `timestamptz` | Ya | — | — | > waktu kirim | — |
| `ExtensionCount` | `int` | Ya | `0` | — | perpanjangan di riwayat | — |
| `ClosedAt` | `timestamptz` | Tidak | — | — | — | — |
| `FailureReason` | `varchar(500)` | Tidak | — | — | wajib saat `FAILED` | — |
| `InheritedFromRfqId` | `uuid` | Tidak | — | FK `PrcRfq` | paket hasil pecah | — |
| `RowVersion` | `uuid` | Ya | baru | concurrency token | — | — |

### `PrcRfqInvitation`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `RfqId` | `uuid` | Ya | — | FK `PrcRfq`, Unik (`RfqId`, `SupplierId`) | — | — |
| `SupplierId` | `uuid` | Ya | — | FK `MstSupplier` | layak semua kategori paket saat diundang | — |
| `WasRestricted` | `bool` | Ya | `false` | — | — | — |
| `RestrictedReason` | `varchar(500)` | Tidak | — | — | wajib bila `WasRestricted` | — |

### `PrcQuotation`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `RfqId` | `uuid` | Ya | — | FK `PrcRfq` | RFQ `SENT` atau `CLOSED` | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`, index | — | — |
| `SupplierId` | `uuid` | Ya | — | Unik (`RfqId`, `SupplierId`) untuk `RECORDED` | vendor diundang | — |
| `ReceivedAt` | `timestamptz` | Ya | — | — | ≤ sekarang | — |
| `RecordedByUserId` | `uuid` | Ya | pencatat | index | `PBJ-DEC-074` | — |
| `IsLate` | `bool` | Ya | — | — | `ReceivedAt > Deadline` saat dicatat | — |
| `IsComplete` | `bool` | Ya | — | — | menawar semua item paket (`G-P02`) | — |
| `TaxPercent` | `numeric(5,2)` | Ya | dari `MstSupplier` bila kena pajak, selain itu `0` | — | 0–100 | — |
| `TotalBeforeTax`, `TaxAmount`, `TotalAmount` | `numeric(18,2)` | Ya | — | — | dihitung server | S |
| `Status` | `varchar(20)` | Ya | `RECORDED` | — | `RECORDED`/`VOID` | — |
| `VoidReason` | `varchar(500)` | Tidak | — | — | wajib saat `VOID` | — |
| `SourceQuotationId` | `uuid` | Tidak | — | FK `PrcQuotation` | salinan pada paket hasil pecah | — |
| `RowVersion` | `uuid` | Ya | baru | concurrency token | — | — |

### `PrcQuotationLine`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `QuotationId` | `uuid` | Ya | — | FK `PrcQuotation`, Unik (`QuotationId`, `PackageItemId`) | — | — |
| `PackageItemId` | `uuid` | Ya | — | FK `PrcPackageItem` | item paket yang sama | — |
| `OfferedUnitPriceBeforeTax` | `numeric(18,2)` | Ya | — | — | > 0 | S |
| `CurrentUnitPriceBeforeTax` | `numeric(18,2)` | Ya | = penawaran | — | ≤ nilai sebelumnya (`PBJ-ASM-005`) | S |
| `FinalUnitPrice` | `numeric(18,2)` | Ya | — | — | = round(terkini × (1 + PPN%), 2) | S |
| `LineAmount` | `numeric(18,2)` | Ya | — | — | = round(`FinalUnitPrice` × jumlah, 2) | S |
| `SourceQuotationLineId` | `uuid` | Tidak | — | FK `PrcQuotationLine` | jejak harga pada paket hasil pecah | — |

### `PrcTechnicalCriterion`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageItemId` | `uuid` | Ya | — | FK `PrcPackageItem`, index | — | — |
| `SortOrder` | `int` | Ya | `0` | — | — | — |
| `CriterionText` | `varchar(500)` | Ya | — | — | — | — |

### `PrcTechnicalEvaluationResult`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `QuotationLineId` | `uuid` | Ya | — | FK `PrcQuotationLine`; Unik (`QuotationLineId`, `CriterionId`, `EvaluatorUserId`) | — | — |
| `CriterionId` | `uuid` | Ya | — | FK `PrcTechnicalCriterion` | kriteria item yang sama | — |
| `EvaluatorUserId` | `uuid` | Ya | — | index | evaluator aktif, ≠ `RecordedByUserId` penawaran | — |
| `Result` | `varchar(10)` | Ya | — | — | `PASS`/`FAIL` | — |
| `Reason` | `varchar(500)` | Tidak | — | — | wajib bila `FAIL` | — |
| `EvaluatedAt` | `timestamptz` | Ya | — | — | — | — |
| `SourceResultId` | `uuid` | Tidak | — | FK | salinan pada paket hasil pecah | — |

### `PrcNegotiation`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `QuotationId` | `uuid` | Ya | — | FK `PrcQuotation`, Unik (`QuotationId`, `RoundNo`) | penawaran sah dan *Lulus* | — |
| `RoundNo` | `int` | Ya | berikutnya | — | — | — |
| `NegotiatedAt` | `timestamptz` | Ya | — | — | — | — |
| `Participants` | `varchar(500)` | Ya | — | — | — | — |
| `Summary` | `varchar(2000)` | Ya | — | — | — | S |

### `PrcNegotiationLine`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `NegotiationId` | `uuid` | Ya | — | FK `PrcNegotiation` | — | — |
| `QuotationLineId` | `uuid` | Ya | — | FK `PrcQuotationLine` | — | — |
| `PreviousUnitPriceBeforeTax`, `NewUnitPriceBeforeTax` | `numeric(18,2)` | Ya | — | — | baru ≤ sebelumnya, > 0 | S |

## 5. Penetapan

### `PrcPackageAward`

| Kolom | Tipe | Wajib | Bawaan | Index/relasi | Validasi | Sensitif |
| --- | --- | --- | --- | --- | --- | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — |
| `PackageId` | `uuid` | Ya | — | FK `PrcPackage`; **Unik bila status `PENDING_APPROVAL`/`RETURNED`/`APPROVED`** | satu penetapan hidup per paket | — |
| `QuotationId` | `uuid` | Ya | — | FK `PrcQuotation` | sah, lengkap, *Lulus* | — |
| `SupplierId` | `uuid` | Ya | — | FK `MstSupplier` | layak saat diajukan dan diterapkan | — |
| `Status` | `varchar(20)` | Ya | `PENDING_APPROVAL` | index | `state-transition-matrix.md` bagian 4 | — |
| `AwardTotal` | `numeric(18,2)` | Ya | — | — | = `PrcQuotation.TotalAmount` saat diajukan | S |
| `HpsTotalSnapshot` | `numeric(18,2)` | Ya | — | — | = `ApprovedHpsTotal` | S |
| `IsOverHps` | `bool` | Ya | — | — | `AwardTotal > HpsTotalSnapshot` | — |
| `OverHpsReason` | `varchar(500)` | Tidak | — | — | wajib bila `IsOverHps` | — |
| `IsLowest` | `bool` | Ya | — | — | dihitung server | — |
| `NonLowestReason` | `varchar(500)` | Tidak | — | — | wajib bila tidak terendah (`PBJ-DEC-085`) | — |
| `IsRestrictedVendor` | `bool` | Ya | — | — | — | — |
| `RestrictedReason` | `varchar(500)` | Tidak | — | — | wajib bila vendor Dibatasi | — |
| `RankingSnapshotJson` | `jsonb` | Ya | — | — | peringkat saat diajukan | S |
| `WorkflowInstanceId` | `uuid` | Tidak | — | index | — | — |
| `SubmissionCount` | `int` | Ya | `1` | — | — | — |
| `SubmittedAt`, `ApprovedAt`, `RevokedAt` | `timestamptz` | Tidak | — | — | — | — |
| `RevokeReason`, `NotAppliedReason` | `varchar(500)` | Tidak | — | — | — | — |
| `RowVersion` | `uuid` | Ya | baru | concurrency token | — | — |

## 6. Tabel sudah ada yang dirujuk (kolom kunci)

| Tabel | Kolom kunci yang dipakai | Model |
| --- | --- | --- |
| `PrcPurchaseRequestLine` **(Diperbarui)** | `Id`, `PurchaseRequestId`, `ItemId`, `PurchaseQuantity`, `EstimatedUnitPrice` (dibaca sebagai termasuk PPN, `PBJ-DEC-084`), `NeedPlanLineKey`; **baru:** `PackageId` `uuid NULL` (index), `PackagedAt` `timestamptz NULL` | `Areas/Corporate/ProcurementManagement/Request/Models/PrcPurchaseRequestLine.cs` |
| `PrcPurchaseRequest` | `Id`, `RequestNumber`, `DepartmentId`, `FiscalYear`, `FundSourceId`, `Status` (`APPROVED` → `CONVERTED_TO_PACKAGE`) | `.../Request/Models/PrcPurchaseRequest.cs` |
| `PrcPackagePlan`, `PrcPackagePlanLine` | `Id`, `NeedPlanLineKey`, `AllocatedQuantity` (rujukan "direncanakan vs dipaketkan") | `.../Planning/Models/` |
| `PrcBudgetCommitment` | `BudgetLineageKey`, `SourceDocumentType` (`PBJ_PACKAGE_AWARD`), `SourceDocumentId`, `IsActive` | `.../Planning/Models/PrcBudgetCommitment.cs` |
| `MstProcurementItem` | `Id`, `ItemCode`, `ItemName`, `ItemCategoryId`, `ProcurementTypeId` | `.../MasterData/Models/MstProcurementItem.cs` |
| `MstSupplier` | `Id`, `SupplierCode`, `IsActive`, penanda kena pajak dan `taxPercent` | `Areas/Administrator/MasterData/Models/MstSupplier.cs` |
| `PrcAttachment` | `DocumentType` (`PBJ_PACKAGE`, `PBJ_RFQ`), `DocumentId`, `AttachmentCategory` (= `DocumentTypeCode`) | `.../Common/Models/PrcAttachment.cs` |
| `PrcDocumentHistory` | `DocumentType`, `DocumentId`, aksi, alasan | `.../Common/Models/PrcDocumentHistory.cs` |

## 7. Jenis dokumen pada tabel bersama

| `DocumentType` / `ReferenceType` | Dipakai di | Keterangan |
| --- | --- | --- |
| `PBJ_PACKAGE` | `PrcAttachment`, `PrcDocumentHistory`, `Wfl` (persiapan) | Paket |
| `PBJ_RFQ` | `PrcAttachment`, `PrcDocumentHistory` | RFQ dan penawarannya |
| `PBJ_PACKAGE_AWARD` | `Wfl` (penetapan), `PrcBudgetCommitment.SourceDocumentType`, `PrcDocumentHistory` | Penetapan |

## 8. Skema DDL (dokumentasi bentuk)

> Bagian ini dokumentasi bentuk tabel, **bukan** skrip yang dijalankan. Skema sebenarnya dibuat migration EF Core dari file
> Configuration. Kolom `IdentityModel` tidak ditulis.

```sql
CREATE TABLE public."PrcPackage" (
  "Id" uuid PRIMARY KEY, "PackageNumber" varchar(20) NULL, "Title" varchar(200) NOT NULL,
  "ProcurementTypeId" uuid NOT NULL, "FiscalYear" int NOT NULL, "PackagePlanId" uuid NULL, "SplitFromPackageId" uuid NULL,
  "Status" varchar(40) NOT NULL, "MethodId" uuid NULL, "MethodCode" varchar(30) NULL, "MethodName" varchar(150) NULL,
  "MethodMinHpsAmount" numeric(18,2) NULL, "MethodMaxHpsAmount" numeric(18,2) NULL, "MethodMinimumQuotationCount" int NULL,
  "MethodRequiresTechnicalEvaluation" boolean NOT NULL, "IsMethodOutOfRange" boolean NOT NULL,
  "MethodDeviationReason" varchar(500) NULL, "HpsTotal" numeric(18,2) NOT NULL, "ApprovedHpsTotal" numeric(18,2) NULL,
  "PreparationWorkflowInstanceId" uuid NULL, "PreparationSubmissionCount" int NOT NULL,
  "PreparationSubmittedAt" timestamptz NULL, "PreparationApprovedAt" timestamptz NULL, "PicUserId" uuid NOT NULL,
  "PurchaseOrderNumber" varchar(50) NULL, "PurchaseOrderRecordedAt" timestamptz NULL,
  "PurchaseOrderRecordedByUserId" uuid NULL, "CancelReason" varchar(500) NULL, "RowVersion" uuid NOT NULL);
CREATE UNIQUE INDEX "IX_PrcPackage_PackageNumber" ON public."PrcPackage" ("PackageNumber")
  WHERE "PackageNumber" IS NOT NULL AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_PrcPackage_PurchaseOrderNumber" ON public."PrcPackage" ("PurchaseOrderNumber")
  WHERE "PurchaseOrderNumber" IS NOT NULL AND "IsDelete" = false;

CREATE TABLE public."PrcPackageItemSource" (
  "Id" uuid PRIMARY KEY, "PackageItemId" uuid NOT NULL REFERENCES public."PrcPackageItem" ("Id"),
  "PurchaseRequestLineId" uuid NOT NULL REFERENCES public."PrcPurchaseRequestLine" ("Id"),
  "PurchaseRequestId" uuid NOT NULL, "DepartmentId" uuid NOT NULL, "FundSourceId" uuid NOT NULL,
  "Quantity" numeric(18,3) NOT NULL, "IsReleased" boolean NOT NULL, "ReleasedAt" timestamptz NULL);
CREATE UNIQUE INDEX "IX_PrcPackageItemSource_PurchaseRequestLineId_Live" ON public."PrcPackageItemSource"
  ("PurchaseRequestLineId") WHERE "IsReleased" = false AND "IsDelete" = false;

CREATE UNIQUE INDEX "IX_PrcPackageAward_PackageId_Live" ON public."PrcPackageAward" ("PackageId")
  WHERE "Status" IN ('PENDING_APPROVAL','RETURNED','APPROVED') AND "IsDelete" = false;

ALTER TABLE public."PrcPurchaseRequestLine" ADD COLUMN "PackageId" uuid NULL, ADD COLUMN "PackagedAt" timestamptz NULL;
CREATE INDEX "IX_PrcPurchaseRequestLine_PackageId" ON public."PrcPurchaseRequestLine" ("PackageId");
```

Tabel lain mengikuti kolom pada bagian 2–5 dengan pola yang sama: PK `uuid`, FK `Restrict`, index unik terfilter
`"IsDelete" = false`.
