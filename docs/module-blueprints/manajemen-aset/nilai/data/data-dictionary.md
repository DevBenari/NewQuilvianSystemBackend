# Kamus Data — Nilai Aset

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit; tidak diulang). Schema `public`; enum integer;
uang `numeric(18,2)`; semua FK `Restrict`. **Seluruh kolom rupiah bertanda sensitif.**

## 1. Status dan kepemilikan

| Entity | Status | Owner | Migration |
|---|---|---|---|
| `AstValuationParameter`, `AstDepreciationPolicy`, `AstAssetBook`, `AstAssetValueEntry`, `AstAccountingOutbox` | Baru | Aset / `nilai` | Urutan 1 |
| `AstDepreciationRun`, `AstDepreciationRunLine`, `AstValueChangeRequest`, `AstSubledgerAccountSetup`, `AstUnitCostCenter` | Baru | Aset / `nilai` | Urutan 4 |
| `AstAsset`, `MstAssetGroup`, `AstAttachment` | Sudah ada | Aset / `register` | — |
| `MstCostCenter` | Sudah ada | HR | Kunci: `Id`, `CostCenterCode`, `OrganizationUnitId`, `IsActive` — `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs` |
| `AccAccountingEvent` | Sudah ada → **Diperbarui oleh modul Akuntansi** (`DEP-AST-01`) | Akuntansi | Kolom pusat biaya; bukan migration Aset |

## 2. Parameter

### 2.1 `AstValuationParameter`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | — |
| `LegalEntityId` | `Guid` | Ya | — | Unik `(LegalEntityId, EffectiveFromPeriod)` | FK `MstLegalEntity` | Tidak | — |
| `EffectiveFromPeriod` | `DateOnly` | Ya | — | — | — | Tidak | Tanggal 1 bulan berlaku |
| `CapitalizationThreshold` | `decimal(18,2)` | Ya | — | — | — | **Ya** | — |
| `DisposalDirectorThreshold` | `decimal(18,2)` | Ya | — | — | — | **Ya** | `AST-DEC-049` |
| `UtilizationDirectorThreshold` | `decimal(18,2)` | Ya | — | — | — | **Ya** | `AST-DEC-029` |
| `Note` | `string(500)` | Tidak | — | — | — | Tidak | Dasar penetapan |
| `SetBy`, `SetAt` | `Guid`, `DateTime` | Ya | — | — | — | Tidak | — |

### 2.2 `AstDepreciationPolicy`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | — |
| `AssetGroupId` | `Guid` | Ya | — | Unik `(AssetGroupId, EffectiveFromPeriod)` | FK `MstAssetGroup` | Tidak | — |
| `EffectiveFromPeriod` | `DateOnly` | Ya | — | — | — | Tidak | — |
| `UsefulLifeMonths` | `int` | Ya | — | — | — | Tidak | 1–600 |
| `ResidualValuePercent` | `decimal(5,2)` | Ya | `0` | — | — | Tidak | 0–100 |
| `SetBy`, `SetAt` | `Guid`, `DateTime` | Ya | — | — | — | Tidak | — |

### 2.3 `AstSubledgerAccountSetup` dan `AstUnitCostCenter`

| Kolom (`AstSubledgerAccountSetup`) | Tipe | Wajib | Index | Keterangan |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | PK | — |
| `AssetGroupId` | `Guid` | Ya | Unik; FK `MstAssetGroup` | — |
| `CostControlAccountCode` | `string(50)` | Ya | — | Kode akun Akuntansi |
| `AccumulatedDepreciationControlAccountCode` | `string(50)` | Ya | — | — |
| `AccumulatedImpairmentControlAccountCode` | `string(50)` | Tidak | — | — |

| Kolom (`AstUnitCostCenter`) | Tipe | Wajib | Index | Keterangan |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | PK | — |
| `OrganizationUnitId` | `Guid` | Ya | Unik; FK `MstOrganizationUnit` | — |
| `CostCenterId` | `Guid` | Ya | FK `MstCostCenter` | — |

## 3. Buku

### 3.1 `AstAssetBook`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Unik | FK `AstAsset` | Tidak | Satu buku per aset |
| `IsCapitalized` | `bool` | Ya | — | Index | — | Tidak | `AST-RULE-001` |
| `IsDepreciable` | `bool` | Ya | — | — | — | Tidak | Dari kategori |
| `AcquisitionCost` | `decimal(18,2)` | Ya | — | — | — | **Ya** | Harga awal saat buku dibuat |
| `CapitalizedCost` | `decimal(18,2)` | Ya | — | — | — | **Ya** | Biaya tercatat berjalan |
| `ResidualValue` | `decimal(18,2)` | Ya | `0` | — | — | **Ya** | Rupiah hasil % × biaya, atau estimasi aset |
| `UsefulLifeMonths` | `int` | Ya | — | — | — | Tidak | — |
| `DepreciatedMonths` | `int` | Ya | `0` | — | — | Tidak | — |
| `DepreciationStartPeriod` | `DateOnly` | Ya | — | Index | — | Tidak | Bulan sesudah siap pakai |
| `LastDepreciatedPeriod` | `DateOnly?` | Tidak | — | — | — | Tidak | — |
| `AccumulatedDepreciation` | `decimal(18,2)` | Ya | `0` | — | — | **Ya** | — |
| `AccumulatedImpairment` | `decimal(18,2)` | Ya | `0` | — | — | **Ya** | — |
| `HasEstimateOverride` | `bool` | Ya | `false` | — | — | Tidak | `AST-DES-033` |
| `DepreciationPolicyId` | `Guid?` | Tidak | — | — | FK `AstDepreciationPolicy` | Tidak | Kebijakan terakhir yang dipakai |
| `BookStatus` | `AssetBookStatus` | Ya | `Aktif` | Index | — | Tidak | — |
| `StoppedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Saat dihentikan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | Tidak | — |

Nilai buku tidak disimpan; dihitung `CapitalizedCost − AccumulatedDepreciation − AccumulatedImpairment`.

### 3.2 `AstAssetValueEntry`

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | PK | — | Tidak | — |
| `AssetId` | `Guid` | Ya | Index `(AssetId, Period)` | FK `AstAsset` | Tidak | — |
| `EntryType` | `ValueEntryType` | Ya | Index | — | Tidak | — |
| `Period` | `DateOnly` | Ya | — | — | Tidak | Periode akuntansi |
| `EntryDate` | `DateTime` | Ya | — | — | Tidak | — |
| `CostDelta` | `decimal(18,2)` | Ya | — | — | **Ya** | — |
| `AccumulatedDepreciationDelta` | `decimal(18,2)` | Ya | — | — | **Ya** | — |
| `AccumulatedImpairmentDelta` | `decimal(18,2)` | Ya | — | — | **Ya** | — |
| `CostCenterId` | `Guid?` | Tidak | — | FK `MstCostCenter` | Tidak | — |
| `SourceDocumentType`, `SourceDocumentId`, `SourceDocumentNumber` | `string(40)`, `Guid?`, `string(30)` | Tidak | Index `SourceDocumentId` | — | Tidak | — |
| `Note` | `string(500)` | Tidak | — | — | Tidak | — |
| `ActorUserId` | `Guid` | Ya | — | — | Tidak | — |

## 4. Proses dan perubahan

### 4.1 `AstDepreciationRun` dan `AstDepreciationRunLine`

| Kolom (`AstDepreciationRun`) | Tipe | Wajib | Index | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `Id` | `Guid` | Ya | PK | Tidak | — |
| `LegalEntityId` | `Guid` | Ya | Unik `(LegalEntityId, Period)` | Tidak | — |
| `Period` | `DateOnly` | Ya | — | Tidak | — |
| `AssetCount` | `int` | Ya | — | Tidak | — |
| `TotalAmount`, `AdjustmentAmount` | `decimal(18,2)` | Ya | — | **Ya** | — |
| `ProcessedBy`, `ProcessedAt` | `Guid`, `DateTime` | Ya | — | Tidak | — |

| Kolom (`AstDepreciationRunLine`) | Tipe | Wajib | Index | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `Id` | `Guid` | Ya | PK | Tidak | — |
| `RunId` | `Guid` | Ya | Unik `(RunId, AssetId, AdjustmentReason)` | Tidak | — |
| `AssetId` | `Guid` | Ya | Index | Tidak | — |
| `AssetGroupId` | `Guid` | Ya | Index `(RunId, AssetGroupId, CostCenterId)` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Tidak | Unit pada akhir bulan |
| `CostCenterId` | `Guid` | Ya | — | Tidak | — |
| `Amount` | `decimal(18,2)` | Ya | — | **Ya** | — |
| `AdjustmentReason` | `DepreciationAdjustmentReason` | Ya | — | Tidak | `Tidak` untuk baris normal |
| `AdjustmentPeriod` | `DateOnly?` | Tidak | — | Tidak | Bulan yang dikejar |

### 4.2 `AstValueChangeRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | Tidak | — |
| `RequestNumber` | `string(30)` | Ya | — | Unik | Tidak | `AST-NLI-######` |
| `AssetId` | `Guid` | Ya | — | Unik parsial `WHERE Status = Diajukan` | Tidak | Satu terbuka per aset |
| `ChangeType` | `ValueChangeType` | Ya | — | Index | Tidak | — |
| `Amount` | `decimal(18,2)?` | Tidak | — | — | **Ya** | Nilai perbaikan/penurunan, atau harga baru |
| `AdditionalLifeMonths` | `int?` | Tidak | — | — | Tidak | Perbaikan besar |
| `NewUsefulLifeMonths` | `int?` | Tidak | — | — | Tidak | Perubahan estimasi |
| `NewResidualValue` | `decimal(18,2)?` | Tidak | — | — | **Ya** | — |
| `InvoiceNumber` | `string(60)` | Tidak | — | — | Tidak | Wajib untuk perbaikan besar |
| `EvidenceAttachmentId` | `Guid` | Ya | — | — | Tidak | FK `AstAttachment` |
| `Reason` | `string(1000)` | Ya | — | — | Tidak | — |
| `Status` | `ValueChangeStatus` | Ya | `Diajukan` | Index | Tidak | — |
| `RequestedBy`/`At`, `DecidedBy`/`At` | `Guid?`/`DateTime?` | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | Tidak | — |
| `AppliedEntryId` | `Guid?` | Tidak | — | — | Tidak | FK `AstAssetValueEntry` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | Tidak | — |

### 4.3 `AstAccountingOutbox`

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | Tidak | — |
| `EventTypeCode` | `string(50)` | Ya | — | Index | Tidak | — |
| `SourceTransactionId` | `string(150)` | Ya | — | Unik `(SourceTransactionId, SourceVersion)` | Tidak | — |
| `SourceVersion` | `string(10)` | Ya | `1` | — | Tidak | — |
| `AccountingDate`, `DocumentDate` | `DateOnly` | Ya | — | — | Tidak | — |
| `Amount` | `decimal(18,2)` | Ya | — | — | **Ya** | — |
| `CostCenterId` | `Guid?` | Tidak | — | — | Tidak | `AST-DEC-047` |
| `ComponentsJson` | `text` | Tidak | — | — | **Ya** | Daftar komponen |
| `SubledgerJson` | `text` | Tidak | — | — | **Ya** | Saldo subledger |
| `SourceDocumentType`, `SourceDocumentId` | `string(40)`, `Guid` | Ya | — | Index | Tidak | — |
| `Status` | `AccountingOutboxStatus` | Ya | `Pending` | Index `(Status, CreateDateTime)` | Tidak | — |
| `AttemptCount` | `int` | Ya | `0` | — | Tidak | — |
| `LastAttemptAt` | `DateTime?` | Tidak | — | — | Tidak | — |
| `LastError` | `string(1000)` | Tidak | — | — | Tidak | Tanpa kredensial/payload |
| `AccountingEventId` | `Guid?` | Tidak | — | — | Tidak | Dari balasan Akuntansi |

## 5. Skema DDL

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip untuk dijalankan. Kolom audit tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."AstAssetBook" (
    "Id"                      uuid          NOT NULL,
    "AssetId"                 uuid          NOT NULL,
    "IsCapitalized"           boolean       NOT NULL,
    "IsDepreciable"           boolean       NOT NULL,
    "AcquisitionCost"         numeric(18,2) NOT NULL,  -- SENSITIF
    "CapitalizedCost"         numeric(18,2) NOT NULL,  -- SENSITIF
    "ResidualValue"           numeric(18,2) NOT NULL,  -- SENSITIF
    "UsefulLifeMonths"        integer       NOT NULL,
    "DepreciatedMonths"       integer       NOT NULL DEFAULT 0,
    "DepreciationStartPeriod" date          NOT NULL,
    "LastDepreciatedPeriod"   date,
    "AccumulatedDepreciation" numeric(18,2) NOT NULL DEFAULT 0,  -- SENSITIF
    "AccumulatedImpairment"   numeric(18,2) NOT NULL DEFAULT 0,  -- SENSITIF
    "BookStatus"              integer       NOT NULL,
    "RowVersion"              uuid          NOT NULL,
    CONSTRAINT "PK_AstAssetBook" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstAssetBook_AstAsset_AssetId" FOREIGN KEY ("AssetId")
        REFERENCES public."AstAsset" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_AstAssetBook_NotBelowResidual"
        CHECK ("CapitalizedCost" - "AccumulatedDepreciation" - "AccumulatedImpairment" >= "ResidualValue" OR "IsCapitalized" = false)
);
CREATE UNIQUE INDEX "IX_AstAssetBook_AssetId" ON public."AstAssetBook" ("AssetId");

CREATE TABLE public."AstDepreciationRun" (
    "Id"               uuid          NOT NULL,
    "LegalEntityId"    uuid          NOT NULL,
    "Period"           date          NOT NULL,
    "AssetCount"       integer       NOT NULL,
    "TotalAmount"      numeric(18,2) NOT NULL,  -- SENSITIF
    "AdjustmentAmount" numeric(18,2) NOT NULL,  -- SENSITIF
    "ProcessedBy"      uuid          NOT NULL,
    "ProcessedAt"      timestamp     NOT NULL,
    CONSTRAINT "PK_AstDepreciationRun" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_AstDepreciationRun_Entity_Period" ON public."AstDepreciationRun" ("LegalEntityId", "Period");

CREATE TABLE public."AstAccountingOutbox" (
    "Id"                  uuid          NOT NULL,
    "EventTypeCode"       varchar(50)   NOT NULL,
    "SourceTransactionId" varchar(150)  NOT NULL,
    "SourceVersion"       varchar(10)   NOT NULL,
    "AccountingDate"      date          NOT NULL,
    "Amount"              numeric(18,2) NOT NULL,  -- SENSITIF
    "CostCenterId"        uuid,
    "ComponentsJson"      text,                    -- SENSITIF
    "Status"              integer       NOT NULL,
    "AttemptCount"        integer       NOT NULL DEFAULT 0,
    CONSTRAINT "PK_AstAccountingOutbox" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_AstAccountingOutbox_Source" ON public."AstAccountingOutbox" ("SourceTransactionId", "SourceVersion");
```
