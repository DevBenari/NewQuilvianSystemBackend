# Kamus Data — Rawat Jalan sampai Billing V2

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revisi `27` |
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.2` (`RJ-E2E-DEC-016`) |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Sumber | [02-backend-architecture.md](../02-backend-architecture.md) bagian V2; file model dan configuration pada backend `063d38b` |
| Kamus data lama | [erd/data-dictionary.md](../erd/data-dictionary.md) — desain `RJ-BIL` revisi `11`, dipertahankan sebagai riwayat |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`,
`IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah. Penghapusan bersifat
penandaan melalui `IsDelete`, bukan penghapusan baris.


> **Koreksi kontrak `1.0.2` (`RJ-E2E-DEC-016`, 28 September 2026).** Kolom sinkron invoice dan rekonsiliasi yang semula dirancang pada `BilChargeLine` **dipindah ke `BilProcessingEffect`**, karena folio mencatat setiap versi fakta sebagai satu `BilProcessingEffect`, bukan sebagai `BilChargeLine` baru (`BillingFolioService.cs:183-266`). `BilChargeLine` kini berstatus **Sudah ada** dan tidak diubah. Ditambah satu kolom konkurensi `InvoiceSyncVersion`. Koreksi lain: kolom waktu kedua tabel bertipe `timestamp with time zone`; `TariffSnapshot`/`RuleSnapshot` bertipe `text`, bukan `jsonb`; unique gabungan `BilChargeLine` tidak memuat versi.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk custom logger dan tidak boleh dipakai sebagai
contoh berisi data asli.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `CliClinicalMilestoneFact` | Diperbarui | Clinical Integration | 5 kolom rekonsiliasi |
| `BilProcessingEffect` | Diperbarui | Billing (Operational) | 16 kolom sinkron invoice dan rekonsiliasi (`1.0.2`) |
| `BilChargeLine` | Sudah ada | Billing (Operational) | **Tidak diubah** (`1.0.2`) |
| `MstBillingSyncPolicy` | Baru | Billing (master) | Prefix `Mst` dari registry |
| `BilFolio` | Sudah ada | Billing (Operational) | — |
| `BilInvoice` | Sudah ada | Billing | Unique `EncounterId` |
| `BilInvoiceItem` | Sudah ada | Billing | — |
| `BilChargeReceipt` | Sudah ada | Billing | Penjaga idempotency upsert |
| `BilAdjustment` | Sudah ada | Billing | Dibuat lewat `CreateAdjustmentAsync` |
| `RegPatientEncounter` | Sudah ada | Registration Management | Direferensikan, **tidak** disalin |
| `TrxDoctorConsultation` | Sudah ada | Clinical Management | Legacy `Trx*`, tidak dinormalisasi di sini |
| `TrxPatientProcedure` | Sudah ada | Clinical Management | Legacy `Trx*` |
| `PhmPrescription`, `PhmPrescriptionItem` | Sudah ada | Pharmacy Management | — |
| `LabExamination` | Sudah ada | Laboratory Management | — |
| `RadOrder`, `RadStudy` | Sudah ada | Radiology Management | — |
| `MstTariff`, `MstDoctorServiceRule`, `MstPatientClass` | Sudah ada | Master Data | Hanya dibaca |

## 2. Tabel `Baru` dan `Diperbarui` — seluruh kolom

### 2.1 `CliClinicalMilestoneFact` — Diperbarui

Model: `Areas/HealthServices/ClinicalManagement/Models/CliClinicalMilestoneFact.cs`. Kolom lama
tetap; tabel di bawah memuat **seluruh** kolom agar migration dapat direncanakan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci baris ledger |
| `SourceContext` | `string(50)` | Ya | — | Unique gabungan | — | — | Tidak | `Consultation` (baru), `Procedure`, `Laboratory`, `Radiology`, `Prescription`, `BloodBank`, … |
| `SourceAggregateId` | `Guid` | Ya | — | Unique gabungan | — | — | Tidak | Id pelayanan sumber |
| `SourceItemId` | `Guid?` | Tidak | — | Unique gabungan | — | — | Tidak | Id butir (pemeriksaan Lab, study) |
| `EffectType` | `string(100)` | Ya | — | Unique gabungan | — | — | Tidak | `ConsultationCharge` (baru), `ProcedureCharge`, … |
| `MilestoneFactId` | `Guid` | Ya | — | Index | — | — | Tidak | Identitas stabil lintas versi |
| `MilestoneFactVersion` | `int` | Ya | `1` | Unique gabungan | — | — | Tidak | Naik setiap revisi |
| `MilestoneKind` | `int` (enum) | Ya | `ChargeEligibility` | — | — | — | Tidak | `ChargeEligibility = 1`, `ClinicalCancellation = 2` |
| `EncounterId` | `Guid` | Ya | — | Index | kunjungan | — | Tidak | Akar korelasi |
| `OccurredAt` | `DateTime` | Ya | — | — | — | — | Tidak | Waktu pelayanan; dipakai memilih tarif yang berlaku |
| `Quantity` | `decimal?` | Tidak | — | — | — | — | Tidak | Jumlah klinis |
| `Unit` | `string(50)?` | Tidak | — | — | — | — | Tidak | Satuan |
| `TariffSnapshot` | `text?` | Tidak | — | — | — | — | Tidak | Rujukan saja; **tidak** dipakai sebagai harga |
| `RuleSnapshot` | `text?` | Tidak | — | — | — | — | Tidak | Memuat `milestone`, `repeatCause`, jumlah diserahkan per item |
| `IdempotencyKey` | `string(128)` | Ya | — | Unique | — | — | Tidak | Kunci penyerahan ke folio |
| `PayloadFingerprint` | `string(64)` | Ya | — | — | — | — | Tidak | Sidik jari isi |
| `DispatchStatus` | `int` (enum) | Ya | `Pending` | Index; **index gabungan baru** dengan `NextDispatchAttemptAt` | — | — | Tidak | 5 nilai, tidak berubah |
| `DispatchAttemptCount` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `DispatchedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `BillingProcessingEffectId`, `BillingFolioId`, `BillingChargeLineId` | `Guid?` | Tidak | — | — | — | — | Tidak | Hasil di folio |
| `BillingOutcomeCode` | `string(100)?` | Tidak | — | — | — | — | Tidak | — |
| `BillingOutcomeMessage` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `CorrelationId` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |
| `CausationId` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `ActorUserId` | `Guid` | Ya | — | — | pengguna | — | Tidak | Pelaku pelayanan |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | Konkurensi optimistik |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |
| **`NextDispatchAttemptAt`** | `DateTime?` | Tidak | — | Index gabungan | — | — | Tidak | **Baru.** Jadwal kirim ulang berikutnya |
| **`ReconciliationRequiredAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru.** Terisi bila berhenti dicoba otomatis |
| **`ReconciliationResolvedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu penyelesaian manual |
| **`ReconciliationResolvedByUserId`** | `Guid?` | Tidak | — | — | pengguna | — | Tidak | **Baru.** Petugas Billing |
| **`ReconciliationResolutionNote`** | `string(500)?` | Tidak | — | — | — | — | Tidak | **Baru.** Tanpa isi klinis |

### 2.2 `BilProcessingEffect` — Diperbarui (`1.0.2`)

Model: `Areas/HealthServices/BillingManagement/Operational/Models/BilProcessingEffect.cs`. Satu baris
per (konteks, fakta, versi, jenis efek) — unique index yang sudah ada. Inilah unit yang diteruskan
jembatan ke invoice.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `Consumer` | `string(100)` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `OperationType` | `string(100)` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `IdempotencyKey` | `string(128)` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `RequestFingerprint` | `string(64)` | Ya | — | — | — | — | Tidak | — |
| `SourceContext` | `string(50)` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `MilestoneFactId` | `Guid` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `MilestoneFactVersion` | `int` | Ya | — | Unique gabungan | — | — | Tidak | Versi fakta |
| `EffectType` | `string(100)` | Ya | — | Unique gabungan | — | — | Tidak | — |
| `OccurredAt` | `DateTime` (timestamptz) | Ya | — | — | — | — | Tidak | — |
| `Outcome` | `int` (enum) | Ya | `Received` | — | — | — | Tidak | `Succeeded`, `OutcomeUnknown`, … |
| `FolioId` | `Guid?` | Tidak | — | — | FK `BilFolio` | `Restrict` | Tidak | Jalan ke encounter |
| `ChargeLineId` | `Guid?` | Tidak | — | — | FK `BilChargeLine` | `Restrict` | Tidak | Revisi menunjuk charge line versi 1 |
| `CalculationStatus` | `int?` (enum) | Tidak | — | — | — | — | Tidak | `PendingFinancialReview` untuk revisi |
| `ErrorCode` / `ErrorMessage` | `string(100)?` / `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `CorrelationId` / `CausationId` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| **`IsClinicalCancellation`** | `bool` | Ya | `false` | — | — | — | Tidak | **Baru** |
| **`InvoiceSyncStatus`** | `int` (`BillingInvoiceSyncStatus`) | Ya | `0` | Index gabungan dengan `InvoiceSyncNextAttemptAt` | — | — | Tidak | **Baru** |
| **`InvoiceSyncVersion`** | `int` | Ya | `0` | — | — | — | Tidak | **Baru.** Token konkurensi untuk kolom sinkron |
| **`InvoiceSourceDomain`** | `string(50)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`InvoiceSourceDetailId`** | `string(100)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`InvoiceId`** | `Guid?` | Tidak | — | — | FK `BilInvoice` | `Restrict` | Tidak | **Baru** |
| **`InvoiceItemId`** | `Guid?` | Tidak | — | Index | FK `BilInvoiceItem` | `Restrict` | Tidak | **Baru** |
| **`InvoiceAdjustmentId`** | `Guid?` | Tidak | — | — | FK `BilAdjustment` | `Restrict` | Tidak | **Baru** |
| **`InvoiceSyncAttemptCount`** | `int` | Ya | `0` | — | — | — | Tidak | **Baru** |
| **`InvoiceSyncNextAttemptAt`** | `DateTime?` | Tidak | — | Index gabungan | — | — | Tidak | **Baru** |
| **`InvoiceSyncedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`InvoiceSyncErrorCode`** | `string(100)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`InvoiceSyncErrorMessage`** | `string(1000)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`ReconciliationResolvedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`ReconciliationResolvedByUserId`** | `Guid?` | Tidak | — | — | pengguna | — | Tidak | **Baru** |
| **`ReconciliationResolutionNote`** | `string(500)?` | Tidak | — | — | — | — | Tidak | **Baru.** Tanpa isi klinis |

Kolom `DateTime` baru seluruhnya `timestamp with time zone`, mengikuti kolom waktu yang sudah ada.

### 2.3 `MstBillingSyncPolicy` — Baru

Model: `Areas/HealthServices/BillingManagement/MasterData/Models/MstBillingSyncPolicy.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PolicyCode` | `string(50)` | Ya | — | **Unique** (baris hidup) | — | — | Tidak | `FACT_DISPATCH`, `INVOICE_SYNC` |
| `PolicyName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama terbaca |
| `MaxAttemptCount` | `int` | Ya | — | — | — | — | Tidak | 0–20; `0` = tanpa kirim ulang otomatis |
| `BaseDelaySeconds` | `int` | Ya | — | — | — | — | Tidak | 10–3600 |
| `MaxDelaySeconds` | `int` | Ya | — | — | — | — | Tidak | ≥ `BaseDelaySeconds`, ≤ 86400 |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | `false` = fail-closed |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Konkurensi optimistik (pola Billing) |

## 3. Tabel `Sudah ada` — kolom kunci

| Tabel | Kolom kunci yang dipakai amendment ini | File model |
|---|---|---|
| `BilFolio` | `Id` (PK), `EncounterId` (unique), `Status` | `Areas/HealthServices/BillingManagement/Operational/Models/BilFolio.cs` |
| `BilInvoice` | `Id`, `EncounterId` (unique), `InvoiceNumber`, `ServiceType`, `Status` (`OPEN`, `FINAL`, `CLOSED`, `SETTLED_BY_WRITE_OFF`), `RowVersion` | `.../Billing/Models/BilInvoice.cs` |
| `BilInvoiceItem` | `Id`, `InvoiceId` (FK), `SourceDomain`, `SourceDetailId`, `SourceVersion`, `SourceStatus`, `TariffId`, `CategoryId`, `Quantity`, `UnitPrice`, `DoctorShare`, `Status` (`ACTIVE`, `VOIDED`), `SourcePayloadHash` | `.../Billing/Models/BilInvoiceItem.cs` |
| `BilChargeReceipt` | `IdempotencyKey`, `InvoiceItemId` | `.../Billing/Models/BilChargeReceipt.cs` |
| `BilAdjustment` | `Id`, `InvoiceId`, `Direction` (`DEBIT`/`CREDIT`), `Amount`, `Status` (`SUBMITTED`/`POSTED`), `IdempotencyKey` | `.../Billing/Models/BilAdjustment.cs` |
| `RegPatientEncounter` | `Id`, `EncounterType`, `EncounterStatus`, `ClinicId`, `DoctorId`, `PatientClassId`, `CompletedAt` | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` |
| `TrxDoctorConsultation` | `Id`, `EncounterId`, `DoctorId`, `ClinicId`, `ConsultationStatus`, `CompletedAt` | `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` |
| `TrxPatientProcedure` | `Id`, `EncounterId`, `ProcedureId`, `TariffId`, `ClinicId`, `Quantity`, `IsFreeOfCharge`, `IsBillable`, `ProcedureStatus` | `.../ClinicalManagement/Models/TrxPatientProcedure.cs` |
| `PhmPrescription` | `Id`, `EncounterId`, `ConsultationId`, `PrescriptionStatus`, `FulfillmentStatus` | `Areas/HealthServices/PharmacyManagement/Models/PhmPrescription.cs` |
| `PhmPrescriptionItem` | `Id`, `PrescriptionId`, `DrugId`, `TariffId`, `Quantity` | `.../PharmacyManagement/Models/PhmPrescriptionItem.cs` |
| `LabExamination` | `Id`, `LabOrderId`, `ProcedureId`, `TariffId` | `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs` |
| `RadOrder` | `Id`, `EncounterId`, `ProcedureId` | `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs` |
| `MstTariff` | `Id`, `TariffCategoryId`, `ClinicId`, `PatientClassId`, `ProcedureId`, `DrugId`, `IsConsultationFee`, `NormalPrice`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | `Areas/HealthServices/MasterData/Models/MstTariff.cs` |
| `MstDoctorServiceRule` | `DoctorId`, `ServiceUnitId`, `ClinicId`, `PatientClassId`, `TariffId`, `RuleStatus`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | `Areas/HealthServices/MasterData/Models/MstDoctorServiceRule.cs` |

## 4. Skema DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di
> bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya akan
> berbenturan dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."MstBillingSyncPolicy" (
    "Id"                uuid          NOT NULL,
    "PolicyCode"        varchar(50)   NOT NULL,
    "PolicyName"        varchar(150)  NOT NULL,
    "MaxAttemptCount"   integer       NOT NULL,
    "BaseDelaySeconds"  integer       NOT NULL,
    "MaxDelaySeconds"   integer       NOT NULL,
    "IsActive"          boolean       NOT NULL DEFAULT true,
    "Description"       varchar(500),
    "RowVersion"        uuid          NOT NULL,
    CONSTRAINT "PK_MstBillingSyncPolicy" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_MstBillingSyncPolicy_PolicyCode"
    ON public."MstBillingSyncPolicy" ("PolicyCode") WHERE "IsDelete" = false;

-- Kolom tambahan pada tabel yang sudah ada
ALTER TABLE public."BilProcessingEffect"
    ADD "IsClinicalCancellation"          boolean       NOT NULL DEFAULT false,
    ADD "InvoiceSyncStatus"               integer       NOT NULL DEFAULT 0,
    ADD "InvoiceSyncVersion"              integer       NOT NULL DEFAULT 0,
    ADD "InvoiceSourceDomain"             varchar(50),
    ADD "InvoiceSourceDetailId"           varchar(100),
    ADD "InvoiceId"                       uuid,
    ADD "InvoiceItemId"                   uuid,
    ADD "InvoiceAdjustmentId"             uuid,
    ADD "InvoiceSyncAttemptCount"         integer       NOT NULL DEFAULT 0,
    ADD "InvoiceSyncNextAttemptAt"        timestamptz,
    ADD "InvoiceSyncedAt"                 timestamptz,
    ADD "InvoiceSyncErrorCode"            varchar(100),
    ADD "InvoiceSyncErrorMessage"         varchar(1000),
    ADD "ReconciliationResolvedAt"        timestamptz,
    ADD "ReconciliationResolvedByUserId"  uuid,
    ADD "ReconciliationResolutionNote"    varchar(500),
    ADD CONSTRAINT "FK_BilProcessingEffect_BilInvoice_InvoiceId"
        FOREIGN KEY ("InvoiceId") REFERENCES public."BilInvoice" ("Id") ON DELETE RESTRICT,
    ADD CONSTRAINT "FK_BilProcessingEffect_BilInvoiceItem_InvoiceItemId"
        FOREIGN KEY ("InvoiceItemId") REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT,
    ADD CONSTRAINT "FK_BilProcessingEffect_BilAdjustment_InvoiceAdjustmentId"
        FOREIGN KEY ("InvoiceAdjustmentId") REFERENCES public."BilAdjustment" ("Id") ON DELETE RESTRICT;
CREATE INDEX "IX_BilProcessingEffect_InvoiceSyncStatus_InvoiceSyncNextAttemptAt"
    ON public."BilProcessingEffect" ("InvoiceSyncStatus", "InvoiceSyncNextAttemptAt");
CREATE INDEX "IX_BilProcessingEffect_InvoiceItemId" ON public."BilProcessingEffect" ("InvoiceItemId");

ALTER TABLE public."CliClinicalMilestoneFact"
    ADD "NextDispatchAttemptAt"           timestamptz,
    ADD "ReconciliationRequiredAt"        timestamptz,
    ADD "ReconciliationResolvedAt"        timestamptz,
    ADD "ReconciliationResolvedByUserId"  uuid,
    ADD "ReconciliationResolutionNote"    varchar(500);
CREATE INDEX "IX_CliClinicalMilestoneFact_DispatchStatus_NextDispatchAttemptAt"
    ON public."CliClinicalMilestoneFact" ("DispatchStatus", "NextDispatchAttemptAt");
```

Tidak ada kolom `-- SENSITIF` pada amendment ini.
