# Kamus Data — Penghapusan & Pelepasan Aset

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit; tidak diulang). Schema `public`; enum integer;
uang `numeric(18,2)`; semua FK `Restrict`.

## 1. Status dan kepemilikan

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `AstDisposalProposal`, `AstDisposalItem`, `AstDisposalReviewer`, `AstAssetRestoration` | Baru | Aset / `penghapusan` | — |
| `AstAsset`, `AstAttachment`, `AstConditionReport` | Sudah ada | Aset / `register` | — |
| `AstAssetBook` | Sudah ada | Aset / `nilai` | Kunci: `AssetId`, `CapitalizedCost`, `AccumulatedDepreciation`, `AccumulatedImpairment`, `BookStatus` |
| `MstEmployee`, `MstOrganizationUnit` | Sudah ada | HR | `Id` |

## 2. `AstDisposalProposal`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | — |
| `ProposalNumber` | `string(30)` | Ya | — | Unik | — | Tidak | `AST-HPS-######` |
| `Reason` | `DisposalReason` | Ya | — | Index | — | Tidak | — |
| `Description` | `string(1000)` | Ya | — | — | — | Tidak | — |
| `ProposerOrganizationUnitId` | `Guid?` | Tidak | — | Index | FK `MstOrganizationUnit` | Tidak | Terisi bila diusulkan Kepala Unit |
| `Status` | `DisposalStatus` | Ya | `Diusulkan` | Index | — | Tidak | — |
| `TotalNetBookValueAtSubmission` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | Dibekukan (`AST-DES-040`) |
| `DirectorThresholdUsed` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | — |
| `RequiresDirector` | `bool` | Ya | `false` | — | — | Tidak | — |
| `ProposedBy`/`At`, `ReviewersAssignedBy`/`At`, `FeasibilitySubmittedBy`/`At`, `AssetHeadDecidedBy`/`At`, `DirectorDecidedBy`/`At`, `FinalizedBy`/`At`, `CancelledBy`/`At` | `Guid?`/`DateTime?` | Tidak (pengusul wajib) | — | — | — | Tidak | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | Tidak | — |

## 3. `AstDisposalItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | — |
| `ProposalId` | `Guid` | Ya | — | Unik `(ProposalId, AssetId)` | FK usulan | Tidak | — |
| `AssetId` | `Guid` | Ya | — | Index | FK `AstAsset` | Tidak | — |
| `ConditionAtProposal` | `AssetCondition` | Ya | — | — | — | Tidak | Snapshot |
| `LossReportId` | `Guid?` | Tidak | — | — | FK `AstConditionReport` | Tidak | Untuk alasan Hilang |
| `NetBookValueAtProposal` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | — |
| `FeasibilityResult` | `FeasibilityResult?` | Tidak | — | — | — | Tidak | — |
| `FeasibilityNote` | `string(1000)` | Tidak | — | — | — | Tidak | — |
| `RecommendedMethod` | `DisposalMethod?` | Tidak | — | — | — | Tidak | Saran tim |
| `IsExcluded` | `bool` | Ya | `false` | — | — | Tidak | Dikeluarkan karena Tidak Layak |
| `DisposalMethod` | `DisposalMethod?` | Tidak | — | Index | — | Tidak | — |
| `TransferDate` | `DateOnly?` | Tidak | — | — | — | Tidak | — |
| `CounterpartyName` | `string(200)` | Tidak | — | — | — | **Ya** | Pembeli/penerima |
| `ProceedsAmount` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | — |
| `ProceedsReference` | `string(100)` | Tidak | — | — | — | Tidak | Nomor penerimaan Finance |
| `TransferAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Tidak | Berita acara pelaksanaan |
| `NetBookValueAtFinal` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | Dibekukan saat final |
| `GainLossAmount` | `decimal(18,2)?` | Tidak | — | — | — | **Ya** | Positif = laba |
| `IsRestored` | `bool` | Ya | `false` | — | — | Tidak | — |

## 4. `AstDisposalReviewer` dan `AstAssetRestoration`

| Kolom (`AstDisposalReviewer`) | Tipe | Wajib | Index | Relasi |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | PK | — |
| `ProposalId` | `Guid` | Ya | Unik `(ProposalId, EmployeeId)` | FK usulan Restrict |
| `EmployeeId` | `Guid` | Ya | Index | FK `MstEmployee` Restrict |

| Kolom (`AstAssetRestoration`) | Tipe | Wajib | Bawaan | Index | Relasi | Keterangan |
|---|---|:---:|---|---|---|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — |
| `RestorationNumber` | `string(30)` | Ya | — | Unik | — | `AST-PLH-######` |
| `DisposalItemId` | `Guid` | Ya | — | Unik parsial `WHERE Status = Diajukan` | FK `AstDisposalItem` Restrict | — |
| `AssetId` | `Guid` | Ya | — | Index | FK `AstAsset` Restrict | — |
| `Reason` | `string(1000)` | Ya | — | — | — | — |
| `EvidenceAttachmentId` | `Guid?` | Tidak | — | — | FK `AstAttachment` | Wajib menurut `AST-DES-037` |
| `Status` | `RestorationStatus` | Ya | `Diajukan` | Index | — | — |
| `RequestedBy`/`At`, `DecidedBy`/`At` | `Guid?`/`DateTime?` | — | — | — | — | — |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — |

## 5. Skema DDL

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip untuk dijalankan. Kolom audit tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."AstDisposalProposal" (
    "Id"                            uuid          NOT NULL,
    "ProposalNumber"                varchar(30)   NOT NULL,
    "Reason"                        integer       NOT NULL,
    "Description"                   varchar(1000) NOT NULL,
    "Status"                        integer       NOT NULL,
    "TotalNetBookValueAtSubmission" numeric(18,2),           -- SENSITIF
    "DirectorThresholdUsed"         numeric(18,2),           -- SENSITIF
    "RequiresDirector"              boolean       NOT NULL DEFAULT false,
    "RowVersion"                    uuid          NOT NULL,
    CONSTRAINT "PK_AstDisposalProposal" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_AstDisposalProposal_ProposalNumber" ON public."AstDisposalProposal" ("ProposalNumber");

CREATE TABLE public."AstDisposalItem" (
    "Id"                  uuid          NOT NULL,
    "ProposalId"          uuid          NOT NULL,
    "AssetId"             uuid          NOT NULL,
    "FeasibilityResult"   integer,
    "IsExcluded"          boolean       NOT NULL DEFAULT false,
    "DisposalMethod"      integer,
    "ProceedsAmount"      numeric(18,2),                     -- SENSITIF
    "NetBookValueAtFinal" numeric(18,2),                     -- SENSITIF
    "GainLossAmount"      numeric(18,2),                     -- SENSITIF
    CONSTRAINT "PK_AstDisposalItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AstDisposalItem_AstDisposalProposal_ProposalId" FOREIGN KEY ("ProposalId")
        REFERENCES public."AstDisposalProposal" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AstDisposalItem_AstAsset_AssetId" FOREIGN KEY ("AssetId")
        REFERENCES public."AstAsset" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_AstDisposalItem_Proposal_Asset" ON public."AstDisposalItem" ("ProposalId", "AssetId");
```
