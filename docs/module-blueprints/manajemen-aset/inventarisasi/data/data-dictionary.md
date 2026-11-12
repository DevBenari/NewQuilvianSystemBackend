# Kamus Data — Inventarisasi Fisik

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit; tidak diulang). Schema `public`; enum integer;
semua FK `Restrict`.

## 1. Status dan kepemilikan

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `AstStockTakeSession`, `AstStockTakeTeamMember`, `AstStockTakeExpectedItem`, `AstStockTakeScan`, `AstStockTakeResult`, `AstStockTakeFollowUp` | Baru | Aset / `inventarisasi` | — |
| `AstAsset`, `MstAssetLocation`, `AstAttachment` | Sudah ada | Aset / `register` | `../../register/data/data-dictionary.md` |
| `MstOrganizationUnit`, `MstEmployee` | Sudah ada | HR | `Id` |

## 2. `AstStockTakeSession`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | `AST-OPN-######` |
| `OrganizationUnitId` | `Guid` | Ya | — | Unik parsial `WHERE Status IN (2,3,4) AND IsDelete=false` | FK `MstOrganizationUnit` | Restrict | Tidak | Satu sesi aktif per unit |
| `LocationId` | `Guid?` | Tidak | — | — | FK `MstAssetLocation` | Restrict | Tidak | Batasi ke satu ruangan |
| `PlannedStartDate`, `PlannedEndDate` | `DateOnly` | Ya | — | Index `PlannedEndDate` | — | — | Tidak | Dasar "tertunda" |
| `Status` | `StockTakeStatus` | Ya | `Dijadwalkan` | Index | — | — | Tidak | — |
| `ExpectedCount`, `ScannedCount` | `int` | Ya | `0` | — | — | — | Tidak | Ringkasan |
| `StartedBy`/`At`, `CountingClosedBy`/`At`, `SubmittedBy`/`At`, `VerifiedBy`/`At`, `CancelledBy`/`At` | `Guid?`/`DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `Notes` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

## 3. `AstStockTakeTeamMember`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `SessionId` | `Guid` | Ya | Unik `(SessionId, EmployeeId)` | FK sesi Restrict | — |
| `EmployeeId` | `Guid` | Ya | Index | FK `MstEmployee` Restrict | — |
| `IsLeader` | `bool` | Ya | Unik parsial `(SessionId) WHERE IsLeader` | — | Tepat satu ketua |

## 4. `AstStockTakeExpectedItem`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `SessionId` | `Guid` | Ya | Unik `(SessionId, AssetId)` | FK sesi Restrict | — |
| `AssetId` | `Guid` | Ya | Index | FK `AstAsset` Restrict | — |
| `ExpectedLocationId` | `Guid` | Ya | — | FK `MstAssetLocation` Restrict | — |
| `ExpectedCustodianEmployeeId` | `Guid` | Ya | — | FK `MstEmployee` Restrict | — |
| `ExpectedCondition` | `AssetCondition` | Ya | — | — | — |
| `LoanBorrowerOrganizationUnitId` | `Guid?` | Tidak | — | FK `MstOrganizationUnit` Restrict | Terisi bila sedang dipinjam keluar |

## 5. `AstStockTakeScan`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index `(SessionId, AssetId, IsLatest)` | FK sesi | Restrict | Tidak | — |
| `AssetId` | `Guid?` | Tidak | — | — | FK `AstAsset` | Restrict | Tidak | Null untuk temuan tidak terdaftar |
| `ScannedCode` | `string(60)` | Tidak | — | — | — | — | Tidak | Teks QR atau ketikan |
| `IsManualEntry` | `bool` | Ya | `false` | — | — | — | Tidak | Diketik, bukan dipindai |
| `IsUnregisteredFinding` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `FindingDescription` | `string(500)` | Tidak | — | — | — | — | Tidak | Wajib bila temuan |
| `FoundLocationId` | `Guid` | Ya | — | Index | FK `MstAssetLocation` | Restrict | Tidak | — |
| `ObservedCondition` | `AssetCondition` | Ya | — | — | — | — | Tidak | — |
| `PhotoAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Restrict | Tidak | Wajib bila temuan |
| `Notes` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `ScannedByEmployeeId` | `Guid` | Ya | — | Index | FK `MstEmployee` | Restrict | Tidak | — |
| `ScannedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `IsLatest` | `bool` | Ya | `true` | — | — | — | Tidak | `AST-DES-029` |

## 6. `AstStockTakeResult`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `SessionId` | `Guid` | Ya | Index `(SessionId, Category)` | FK sesi Restrict | — |
| `AssetId` | `Guid?` | Tidak | Index | FK `AstAsset` Restrict | Null untuk temuan |
| `ScanId` | `Guid?` | Tidak | — | FK `AstStockTakeScan` Restrict | Pindaian terakhir |
| `Category` | `StockTakeResultCategory` | Ya | — | — | — |
| `IsLocationMismatch`, `IsConditionMismatch` | `bool` | Ya | — | — | — |
| `IsForeignAsset` | `bool` | Ya | — | — | Aset unit lain ditemukan di sini (`AST-DES-028`) |
| `ExpectedLocationId`, `FoundLocationId` | `Guid?` | Tidak | — | FK lokasi Restrict | — |
| `ExpectedCondition`, `ObservedCondition` | `AssetCondition?` | Tidak | — | — | — |
| `TeamNote` | `string(1000)` | Tidak | — | — | Wajib untuk selisih sebelum diajukan |

## 7. `AstStockTakeFollowUp`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|
| `Id` | `Guid` | Ya | PK | — | — |
| `SessionId` | `Guid` | Ya | Index | FK sesi Restrict | — |
| `ResultId` | `Guid` | Ya | Unik `(ResultId, Type)` | FK hasil Restrict | — |
| `Type` | `StockTakeFollowUpType` | Ya | Index | — | — |
| `Status` | `StockTakeFollowUpStatus` | Ya | Index | — | Bawaan `Usulan` |
| `TargetDocumentType` | `string(40)` | Tidak | — | — | `Transfer`, `Handover`, `ConditionReport`, `AssetDraft` |
| `TargetDocumentId` | `Guid?` | Tidak | — | — | — |
| `TargetDocumentNumber` | `string(30)` | Tidak | — | — | — |
| `ProcessedBy`/`At`, `DismissedBy`/`At` | `Guid?`/`DateTime?` | Tidak | — | — | — |
| `DismissReason` | `string(500)` | Tidak | — | — | — |

Tidak ada kolom sensitif.

## 8. Skema DDL

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip untuk dijalankan. Kolom audit tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."AstStockTakeSession" (
    "Id"                 uuid        NOT NULL,
    "SessionNumber"      varchar(30) NOT NULL,
    "OrganizationUnitId" uuid        NOT NULL,
    "LocationId"         uuid,
    "PlannedStartDate"   date        NOT NULL,
    "PlannedEndDate"     date        NOT NULL,
    "Status"             integer     NOT NULL,
    "RowVersion"         uuid        NOT NULL,
    CONSTRAINT "PK_AstStockTakeSession" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstStockTakeSession_MstOrganizationUnit_OrganizationUnitId" FOREIGN KEY ("OrganizationUnitId")
        REFERENCES public."MstOrganizationUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_AstStockTakeSession_Dates" CHECK ("PlannedEndDate" >= "PlannedStartDate")
);
CREATE UNIQUE INDEX "IX_AstStockTakeSession_ActivePerUnit" ON public."AstStockTakeSession" ("OrganizationUnitId")
    WHERE "Status" IN (2, 3, 4) AND "IsDelete" = false;

CREATE TABLE public."AstStockTakeScan" (
    "Id"                  uuid        NOT NULL,
    "SessionId"           uuid        NOT NULL,
    "AssetId"             uuid,
    "ScannedCode"         varchar(60),
    "FoundLocationId"     uuid        NOT NULL,
    "ObservedCondition"   integer     NOT NULL,
    "ScannedByEmployeeId" uuid        NOT NULL,
    "ScannedAt"           timestamp   NOT NULL,
    "IsLatest"            boolean     NOT NULL DEFAULT true,
    CONSTRAINT "PK_AstStockTakeScan" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstStockTakeScan_AstStockTakeSession_SessionId" FOREIGN KEY ("SessionId")
        REFERENCES public."AstStockTakeSession" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_AstStockTakeScan_Session_Asset_Latest" ON public."AstStockTakeScan" ("SessionId", "AssetId", "IsLatest");
```
