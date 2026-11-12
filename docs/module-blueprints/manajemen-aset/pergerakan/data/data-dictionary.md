# Kamus Data — Pergerakan Aset

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |

Seluruh tabel mewarisi `IdentityModel` (`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`,
`DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`); tidak diulang.
Schema `public`; enum integer; semua FK `Restrict`.

## 1. Status dan kepemilikan

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `AstTransfer`, `AstTransferItem`, `AstLoan`, `AstLoanItem`, `AstLoanExtension` | Baru | Aset / `pergerakan` | — |
| `AstAsset` | Sudah ada (sejak urutan 1) | Aset / `register` | Kunci: `Id`, `InventoryNumber`, `RecordStatus`, `Condition`, `CurrentOrganizationUnitId`, `ActiveProcessKind` — `../../register/data/data-dictionary.md` 3.1 |
| `MstAssetLocation` | Sudah ada | Aset / `register` | `Id`, `OrganizationUnitId` |
| `MstOrganizationUnit`, `MstEmployee` | Sudah ada | HR | `Id` |

## 2. `AstTransfer`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `TransferNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-MUT-######` |
| `SourceOrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `TargetOrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `TargetLocationId` | `Guid?` | Tidak | — | — | FK `MstAssetLocation` | Restrict | Tidak | Wajib saat diterima |
| `TargetCustodianEmployeeId` | `Guid?` | Tidak | — | — | FK `MstEmployee` | Restrict | Tidak | Wajib saat diterima |
| `Reason` | `TransferReason` | Ya | — | Index | — | — | Tidak | — |
| `ReasonNote` | `string(1000)` | Ya | — | — | — | — | Tidak | — |
| `CorrectsTransferId` | `Guid?` | Tidak | — | Index | FK `AstTransfer` | Restrict | Tidak | `AST-DES-025` |
| `SourceStockTakeResultId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Hasil opname asal |
| `Status` | `TransferStatus` | Ya | `Diajukan` | Index | — | — | Tidak | — |
| `RequestedBy`, `RequestedAt` | `Guid`, `DateTime` | Ya | — | Index | — | — | Tidak | — |
| `UnitApprovedBy`, `UnitApprovedAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `IsUnitApprovalByRequester` | `bool` | Ya | `false` | — | — | — | Tidak | `AST-DEC-048` |
| `AssetHeadApprovedBy`, `AssetHeadApprovedAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ReceivedBy`, `ReceivedAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RejectedBy`, `RejectedAt`, `CancelledBy`, `CancelledAt` | `Guid?`, `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token konkurensi |

## 3. `AstTransferItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `TransferId` | `Guid` | Ya | — | Unik `(TransferId, AssetId)` | FK `AstTransfer` | Restrict | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Index | FK `AstAsset` | Restrict | Tidak | — |
| `ConditionAtSubmit` | `AssetCondition` | Ya | — | — | — | — | Tidak | Snapshot |
| `ConditionAtReceipt` | `AssetCondition?` | Tidak | — | — | — | — | Tidak | Diisi penerima |

## 4. `AstLoan`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `LoanNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-PJM-######` |
| `OwnerOrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `BorrowerOrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | Restrict | Tidak | — |
| `Purpose` | `string(1000)` | Ya | — | — | — | — | Tidak | — |
| `StartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `PlannedReturnDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Dasar Terlambat |
| `OriginalPlannedReturnDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Sebelum perpanjangan |
| `Status` | `LoanStatus` | Ya | `Diajukan` | Index | — | — | Tidak | — |
| `RequestedBy`/`At`, `ApprovedBy`/`At`, `PickedUpBy`/`At`, `ReturnedBy`/`At`, `ReturnConfirmedBy`/`At`, `RejectedBy`/`At`, `CancelledBy`/`At` | `Guid?`/`DateTime?` | Tidak (pengaju wajib) | — | — | — | — | Tidak | — |
| `IsApprovalByRequester` | `bool` | Ya | `false` | — | — | — | Tidak | `AST-DES-021` |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

## 5. `AstLoanItem` dan `AstLoanExtension`

| Kolom (`AstLoanItem`) | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `LoanId` | `Guid` | Ya | Unik `(LoanId, AssetId)` | FK `AstLoan` Restrict | — |
| `AssetId` | `Guid` | Ya | Index | FK `AstAsset` Restrict | — |
| `ConditionAtPickup` | `AssetCondition` | Ya | — | — | Snapshot |
| `ReturnCondition` | `AssetCondition?` | Tidak | — | — | Diisi pemilik |

| Kolom (`AstLoanExtension`) | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `LoanId` | `Guid` | Ya | Unik parsial `(LoanId) WHERE Status = Diajukan` | FK `AstLoan` Restrict | Satu terbuka |
| `PreviousReturnDate`, `RequestedReturnDate` | `DateOnly` | Ya | — | — | — |
| `Reason` | `string(500)` | Ya | — | — | — |
| `Status` | `LoanExtensionStatus` | Ya | Index | — | — |
| `RequestedBy`/`At`, `DecidedBy`/`At` | `Guid?`/`DateTime?` | Tidak | — | — | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — |

Tidak ada kolom sensitif.

## 6. Skema DDL

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip untuk dijalankan. Kolom audit `IdentityModel`
> tidak ditulis ulang; kolom lain mengikuti bagian 2–5.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."AstTransfer" (
    "Id"                       uuid        NOT NULL,
    "TransferNumber"           varchar(30) NOT NULL,
    "SourceOrganizationUnitId" uuid        NOT NULL,
    "TargetOrganizationUnitId" uuid        NOT NULL,
    "TargetLocationId"         uuid,
    "TargetCustodianEmployeeId" uuid,
    "Reason"                   integer     NOT NULL,
    "CorrectsTransferId"       uuid,
    "Status"                   integer     NOT NULL,
    "RequestedBy"              uuid        NOT NULL,
    "RowVersion"               uuid        NOT NULL,
    CONSTRAINT "PK_AstTransfer" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstTransfer_MstOrganizationUnit_SourceOrganizationUnitId" FOREIGN KEY ("SourceOrganizationUnitId")
        REFERENCES public."MstOrganizationUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AstTransfer_AstTransfer_CorrectsTransferId" FOREIGN KEY ("CorrectsTransferId")
        REFERENCES public."AstTransfer" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_AstTransfer_DifferentUnits" CHECK ("SourceOrganizationUnitId" <> "TargetOrganizationUnitId")
);
CREATE UNIQUE INDEX "IX_AstTransfer_TransferNumber" ON public."AstTransfer" ("TransferNumber");
CREATE INDEX "IX_AstTransfer_Status_Units" ON public."AstTransfer" ("Status", "SourceOrganizationUnitId", "TargetOrganizationUnitId");

CREATE TABLE public."AstLoan" (
    "Id"                         uuid        NOT NULL,
    "LoanNumber"                 varchar(30) NOT NULL,
    "OwnerOrganizationUnitId"    uuid        NOT NULL,
    "BorrowerOrganizationUnitId" uuid        NOT NULL,
    "StartDate"                  date        NOT NULL,
    "PlannedReturnDate"          date        NOT NULL,
    "Status"                     integer     NOT NULL,
    "RowVersion"                 uuid        NOT NULL,
    CONSTRAINT "PK_AstLoan" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_AstLoan_DifferentUnits" CHECK ("OwnerOrganizationUnitId" <> "BorrowerOrganizationUnitId"),
    CONSTRAINT "CK_AstLoan_ReturnAfterStart" CHECK ("PlannedReturnDate" >= "StartDate")
);
CREATE INDEX "IX_AstLoan_Status_PlannedReturnDate" ON public."AstLoan" ("Status", "PlannedReturnDate");
```
