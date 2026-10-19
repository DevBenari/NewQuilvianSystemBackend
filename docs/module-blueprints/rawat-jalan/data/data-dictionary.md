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


---

# Amendment DP — Daftar Pasien Rawat Jalan (revisi `28`, `draft`)

Tidak ada tabel atau kolom `Baru`/`Diperbarui`. Kolom kunci tabel `Sudah ada` yang dipakai aturan
fitur ini:

| Tabel (pemilik) | Kolom | Dipakai untuk | Sensitif |
|---|---|---|:---:|
| `RegPatientEncounter` (Registration) — `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` | `Id` (PK) | Kunci | |
| | `EncounterType` | Penyaring RJ (= 1) | |
| | `ClinicId` (FK `MstClinic`, nullable) | Penyaring RJ berklinik, cakupan perawat | |
| | `DoctorId` (FK `MstDoctor`, nullable) | Cakupan dokter | |
| | `EncounterStatus` | Kelompok summary, boleh batal, pemblokir | |
| | `EncounterDate` | Mode hari ini, kunjungan menggantung | |
| | `IsCancel`, `CancelledAt`, `CancelledByUserId`, `CancelReason` (maks 250) | Diisi saat batal | `CancelReason`: Ya |
| | `CompletedAt` | Pemblokir | |
| | `IsActive` | Diisi `false` saat batal | |
| | `PatientId` (FK `MstPatient`) | Nama pasien, no. RM | — (nama dan no. RM di `MstPatient`: Ya) |
| `EmgVisit` (Emergency) | `EncounterId` | Membuang kunjungan IGD | |
| `TrxDoctorConsultation` (Clinical) | `EncounterId`, `IsCancel`, `IsDelete` | Konsultasi aktif | |
| `TrxQueue` (Registration) | `EncounterId`, `CompletedAt`, `CancelledAt`, `NoShowAt` | Antrean yang ikut batal | |
| `MstNurseStationClusterStaff` | `EmployeeId`, `NurseStationClusterId` | Cakupan perawat | |
| `MstDoctor` | `WorkforceProfileId`, `Email` | Pengenal dokter | `Email`: Ya |

Seluruh tabel mewarisi `IdentityModel`. Tidak ada DDL.

---

# Amendment KT — Konsultasi Tertunda (revisi `29`, `draft`)

Tidak ada tabel baru atau tabel yang diperbarui. Kolom kunci yang dibaca fitur ini (semua tabel
`Sudah ada`):

| Tabel | Pemilik | Kolom yang dipakai | Untuk | Model |
|---|---|---|---|---|
| `TrxQueue` | Registration | `Id`, `DoctorId`, `QueueDate`, `QueueStatus`, `EncounterId`, `IsDoctorRequired`, `IsActive`, `IsDelete` | Dasar daftar, syarat KT.3.1 | `TrxQueue` |
| `RegPatientEncounter` | Registration | `Id`, `EncounterType`, `ClinicId`, `EncounterStatus`, `IsCancel`, `CompletedAt`, `IsDelete` | Syarat kunjungan tertahan | `RegPatientEncounter` |
| `TrxDoctorConsultation` | Clinical | `Id`, `QueueId`, `ConsultationStatus`, `IsCancel`, `IsDelete` | Konsultasi aktif | `TrxDoctorConsultation` |
| `PhmPrescription` | Pharmacy | `ConsultationId`, `PrescriptionStatus`, `IsActive`, `IsCancel`, `IsDelete` | `draftPrescriptionCount` | `PhmPrescription` |
| `TrxPatientProcedure` | Clinical | `ConsultationId`, `IsActive`, `IsCancel`, `IsDelete` | `procedureCount` | `TrxPatientProcedure` |
| `EmgVisit` | Emergency | `EncounterId` | Penyaring IGD | `EmgVisit` |

Kolom sensitif yang ikut dikembalikan sama dengan `GET /doctor-queues` (nama pasien, no. RM).


## PM-B. Amendment PM-B — Pendaftaran Rujukan (`approved` 2026-10-08)

Sepuluh kolom warisan `IdentityModel` tidak diulang (lihat kepala dokumen).

### PM-B.1 Status tabel

| Tabel | Status | Pemilik |
|---|---|---|
| `RegEncounterReferral` | Baru | Registration |
| `RegEncounterReferralDocument` | Baru | Registration |
| `RegEncounterReferralRevision` | Baru | Registration |
| `MstReferralInstitution` | Diperbarui (`IsPartner`) | Health Service Master Data |
| `MstReferralDoctor` | Sudah ada | Health Service Master Data |
| `RegPatientEncounter` | Sudah ada | Registration |

### PM-B.2 `RegEncounterReferral` (schema `public`)

| Kolom | Tipe | Wajib | Bawaan | Batas / validasi | Sensitif |
|---|---|:---:|---|---|:---:|
| `Id` | `uuid` PK | Ya | baru | — | Tidak |
| `PatientEncounterId` | `uuid` FK → `RegPatientEncounter`, Restrict | Ya | — | Unique (filter `IsDeleted = false`) | Tidak |
| `ReferralDateTime` | `timestamptz` | Ya | waktu layar dibuka | ≤ sekarang | Tidak |
| `TargetUnitType` | `int` (`ReferralTargetUnitType`) | Ya | — | `1` Clinic, `2` Laboratory; `3` ditolak | Tidak |
| `TargetServiceUnitId` | `uuid` FK → `MstServiceUnit`, Restrict | Ya | — | Unit aktif | Tidak |
| `TargetClinicId` | `uuid` FK → `MstClinic`, Restrict | Bila Clinic | null | Sama dengan klinik kunjungan | Tidak |
| `DiagnosisId` | `uuid` FK → `MstDiagnosis`, Restrict | Tidak (wajib untuk lengkap) | null | Diagnosis aktif | **Ya** |
| `DiagnosisNote` | `varchar(500)` | Tidak | null | — | **Ya** |
| `ReferralReason` | `varchar(1000)` | Tidak (wajib untuk lengkap) | null | — | **Ya** |
| `InstitutionIsPartnerSnapshot` | `boolean` | Ya | `false` | Disalin dari institusi saat disimpan | Tidak |
| `CaptureSource` | `int` (`ReferralCaptureSource`) | Ya | — | `1` Staff, `2` Kiosk | Tidak |
| `IsComplete` | `boolean` | Ya | `false` | Hasil hitung `PM.3.1` | Tidak |
| `CompletedAt` | `timestamptz` | Tidak | null | Diisi saat pertama `true` | Tidak |
| `RowVersion` | `uuid` | Ya | baru | Concurrency token, diganti setiap update | Tidak |

Index: `IX_RegEncounterReferral_IsComplete`, unique `IX_RegEncounterReferral_PatientEncounterId`.

### PM-B.3 `RegEncounterReferralDocument`

| Kolom | Tipe | Wajib | Batas / validasi | Sensitif |
|---|---|:---:|---|:---:|
| `Id` | `uuid` PK | Ya | — | Tidak |
| `EncounterReferralId` | `uuid` FK → `RegEncounterReferral`, Cascade | Ya | — | Tidak |
| `OriginalFileName` | `varchar(255)` | Ya | Dibersihkan dari path | **Ya** |
| `ContentType` | `varchar(100)` | Ya | `application/pdf`, `image/jpeg`, `image/png` | Tidak |
| `SizeBytes` | `bigint` | Ya | ≤ 5 MB | Tidak |
| `StoragePath` | `varchar(500)` | Ya | Relatif terhadap `FileStorage:PrivateRootPath` | Tidak |
| `PageOrder` | `int` | Ya | Mulai 1 | Tidak |

Index `(EncounterReferralId, IsDeleted)`. Hapus = soft delete; berkas fisik dihapus pada task
pembersihan terpisah (tidak dalam scope).

### PM-B.4 `RegEncounterReferralRevision`

| Kolom | Tipe | Wajib | Keterangan | Sensitif |
|---|---|:---:|---|:---:|
| `Id` | `uuid` PK | Ya | — | Tidak |
| `EncounterReferralId` | `uuid` FK Cascade | Ya | — | Tidak |
| `RevisionType` | `int` (`ReferralRevisionType`) | Ya | `1` Completed, `2` Corrected, `3` DocumentAdded, `4` DocumentRemoved | Tidak |
| `OldValuesJson` | `jsonb` | Tidak | Ruas yang berubah saja | **Ya** |
| `NewValuesJson` | `jsonb` | Tidak | Ruas yang berubah saja | **Ya** |
| `ChangedAt` | `timestamptz` | Ya | — | Tidak |
| `ChangedBy` | `uuid` | Tidak | Pengguna | Tidak |

Index `(EncounterReferralId, ChangedAt)`.

### PM-B.5 `MstReferralInstitution` — kolom baru

| Kolom | Tipe | Wajib | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `IsPartner` | `boolean` | Ya | `false` | Bermitra dengan rumah sakit (`RJ-DOC-DEC-073`) |

### PM-B.6 Tabel `Sudah ada` — kolom kunci

| Tabel | Kolom dipakai | Aturan | Model |
|---|---|---|---|
| `RegPatientEncounter` | `IsReferral`, `ReferralNumber`, `ReferralInstitutionId`, `ReferralDoctorId`, `EncounterStatus`, `IsFromKiosk`, `CreateDateTime` | Sumber kebenaran identitas rujukan; penguncian `PM.3.3`; batas Kiosk `PM.3.5` | `RegPatientEncounter.cs` |
| `MstReferralDoctor` | `ReferralInstitutionId`, `IsActive` | Dokter milik institusi | `MstReferralDoctor.cs` |
| `MstPatientInsurance` | `InsuranceProviderId`, `PolicyNumber` | Dicocokkan dengan `cardScan` | `MstPatientInsurance.cs` |
| `MstInsuranceProvider` | `InsuranceProviderName`, `InsuranceProviderCode`, `InsuranceGroupName` | Pencocokan nama | `MstInsuranceProvider.cs` |

---

## RP. Amendment RP — Reservasi Pasien Prioritas (revisi `33`, `approved` 2026-10-16)

Kontrak `RJ-PRI-RESV-001@1.0.0`. Keputusan `RJ-PRI-DEC-001` s.d. `029`. Kedua tabel baru mewarisi `IdentityModel`,
sehingga sepuluh kolom audit tidak ditulis ulang di sini. Penghapusan bersifat penandaan (`IsDelete`). Riwayat tidak
pernah dihapus.

### RP-1. Tabel status dan kepemilikan

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `RegPriorityReservation` | Baru | Registration (`Reg`) | Satu janji kedatangan pasien prioritas |
| `RegPriorityReservationHistory` | Baru | Registration (`Reg`) | Garis waktu kejadian reservasi |
| `RegPatientEncounter` | Sudah ada | Registration | Direferensikan lewat `EncounterId`, **tidak** disalin |
| `RegQueue` | Sudah ada | Registration | `IsPriorityQueue` dipakai penyaring display |
| `MstPatient` | Sudah ada | Patient Management | Direferensikan, tidak disalin |
| `MstPatientMembership` | Sudah ada | `crm-membership` | Snapshot menunjuk barisnya. Status `Cancelled` mencabut prioritas |

### RP-2. `RegPriorityReservation` — seluruh kolom

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReservationNumber` | `string(30)` | Ya | Dari `NumberSeriesAllocator` | Unique | — | — | Tidak | Contoh `RSV-261016-0001`. Tidak pernah diketik petugas |
| `PatientId` | `Guid` | Ya | — | Index (bagian unique bersyarat) | FK `MstPatient` | `Restrict` | Tidak | Pasien pemilik reservasi |
| `VisitDate` | `DateTime` (`date`) | Ya | — | Index (bagian unique bersyarat) | — | — | Tidak | Tanggal kunjungan, kalender WIB, tanpa jam |
| `Status` | `int` (`PriorityReservationStatus`) | Ya | `1` (`AwaitingArrival`) | Index `(VisitDate, Status)` | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` *Amendment RP* |
| `ArrivalCodeHash` | `string(64)` | Ya | — | Unique | — | — | **Ya** | Hex SHA-256 dari kode link. Kode asli tidak disimpan |
| `ArrivalCodeIssuedAt` | `DateTime` | Ya | Saat dibuat | — | — | — | Tidak | Waktu kode terakhir diterbitkan (dibuat atau dikirim ulang) |
| `ArrivedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu konfirmasi tiba yang berlaku. Dikosongkan saat dikembalikan |
| `ArrivalSource` | `int?` (`PriorityReservationArrivalSource`) | Tidak | — | — | — | — | Tidak | `PatientLink` atau `Staff` |
| `ArrivalMarkedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Diisi bila petugas yang menandai tiba |
| `ScreeningCough` | `bool?` | Tidak | — | — | — | — | **Ya** | Checklist skrining awal: batuk |
| `ScreeningFever` | `bool?` | Tidak | — | — | — | — | **Ya** | Checklist skrining awal: demam |
| `ScreeningFallRisk` | `bool?` | Tidak | — | — | — | — | **Ya** | Checklist skrining awal: risiko jatuh |
| `ScreenedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu checklist disimpan |
| `ScreenedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas skrining |
| `EncounterId` | `Guid?` | Tidak | — | Unique bersyarat (`EncounterId IS NOT NULL`) | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunjungan hasil pelengkapan atau jalur lain |
| `ClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu masuk status akhir |
| `ClosedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong bila ditutup sistem |
| `ClosureReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan batal (10–500 karakter). Bisa memuat keterangan kesehatan dari pasien |
| `IsPrioritySnapshot` | `bool` | Ya | `false` | — | — | — | Tidak | Salinan `PriorityQueue` tingkat saat reservasi dibuat |
| `PatientMembershipIdSnapshot` | `Guid?` | Tidak | — | Index | FK `MstPatientMembership` | `Restrict` | Tidak | Membership Aktif saat dibuat |
| `MembershipTierIdSnapshot` | `Guid?` | Tidak | — | — | — | — | Tidak | Tingkat saat dibuat |
| `MembershipTierCodeSnapshot` | `string(50)?` | Tidak | — | — | — | — | Tidak | Kode tingkat saat dibuat, untuk tampilan |
| `QueuePriorityLevelSnapshot` | `int` | Ya | `0` | — | — | — | Tidak | `PriorityLevel` tingkat saat dibuat |
| `QueueAudienceSnapshot` | `int` (`QueueAudience`) | Ya | `Regular` | — | — | — | Tidak | Diteruskan ke antrean saat dilengkapi |
| `PublicDisplayModeSnapshot` | `int` (`PublicDisplayMode`) | Ya | `Default` | — | — | — | Tidak | Diteruskan ke antrean saat dilengkapi |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token concurrency. Diganti setiap perubahan |

Index tambahan:

| Index | Kolom | Syarat | Tujuan |
|---|---|---|---|
| `UX_RegPriorityReservation_Open_Patient_VisitDate` | `(PatientId, VisitDate)` | Unique, `"Status" IN (1, 2) AND NOT "IsDelete"` | `INV-RP-01` |
| `IX_RegPriorityReservation_VisitDate_Status` | `(VisitDate, Status)` | — | Daftar per tanggal dan penutupan akhir hari |

### RP-3. `RegPriorityReservationHistory` — seluruh kolom

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PriorityReservationId` | `Guid` | Ya | — | Index `(PriorityReservationId, OccurredAt)` | FK `RegPriorityReservation` | `Restrict` | Tidak | Reservasi induk |
| `EventCode` | `string(40)` | Ya | — | — | — | — | Tidak | `CREATED`, `ARRIVED`, `ARRIVAL_REVERTED`, `SCREENED`, `CODE_REISSUED`, `CONVERTED`, `CANCELLED`, `NO_SHOW`, `REGISTERED_ELSEWHERE` |
| `FromStatus` | `int?` | Tidak | — | — | — | — | Tidak | Kosong pada `CREATED` |
| `ToStatus` | `int` | Ya | — | — | — | — | Tidak | Status sesudah kejadian. Sama dengan `FromStatus` untuk `SCREENED` dan `CODE_REISSUED` |
| `Source` | `int` (`PriorityReservationEventSource`) | Ya | — | — | — | — | Tidak | `Staff`, `PatientLink`, atau `System` |
| `ActorUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong untuk `PatientLink` dan `System` |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan batal atau alasan mengembalikan tiba |
| `EncounterId` | `Guid?` | Tidak | — | — | — | — | Tidak | Diisi pada `CONVERTED` dan `REGISTERED_ELSEWHERE` |
| `OccurredAt` | `DateTime` | Ya | Saat kejadian (UTC) | Index (bersama) | — | — | Tidak | Waktu kejadian |

### RP-4. Tabel `Sudah ada` — kolom kunci yang dipakai

| Tabel | Kolom | Dipakai untuk | File model |
|---|---|---|---|
| `RegQueue` | `IsPriorityQueue`, `QueueNumber`, `QueueAudienceSnapshot`, `PublicDisplayModeSnapshot`, `PatientMembershipIdSnapshot`, `MembershipTierIdSnapshot` | Penyaring display. Snapshot reservasi disalin ke sini saat dilengkapi | `Areas/HealthServices/RegistrationManagement/Models/RegQueue.cs` |
| `RegPatientEncounter` | `Id`, `PatientId`, `EncounterDate`, `EncounterType`, `ClinicId`, `RegistrationSource` | Pemicu penutupan jalur lain; `RegistrationSource = 6` untuk hasil reservasi | `.../Models/RegPatientEncounter.cs` |
| `MstPatientMembership` | `Id`, `MembershipStatus`, `PeriodStartDate`, `ExpiredDate` | Kelayakan lewat resolver; status `Cancelled` mencabut prioritas | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs` |
| `MstMembershipTier` | `PriorityQueue`, `PriorityLevel`, `QueueAudience`, `PublicDisplayMode` | Isi snapshot | `Areas/Administrator/MasterData/Models/MstMembershipTier.cs` |

### RP-5. Skema DDL

> **Peringatan.** Basis data dibentuk lewat EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan
> skrip yang dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."RegPriorityReservation" (
    "Id"                          uuid          NOT NULL,
    "ReservationNumber"           varchar(30)   NOT NULL,
    "PatientId"                   uuid          NOT NULL,
    "VisitDate"                   date          NOT NULL,
    "Status"                      integer       NOT NULL DEFAULT 1,  -- enum PriorityReservationStatus
    "ArrivalCodeHash"             varchar(64)   NOT NULL,            -- SENSITIF
    "ArrivalCodeIssuedAt"         timestamp     NOT NULL,
    "ArrivedAt"                   timestamp,
    "ArrivalSource"               integer,
    "ArrivalMarkedByUserId"       uuid,
    "ScreeningCough"              boolean,                           -- SENSITIF
    "ScreeningFever"              boolean,                           -- SENSITIF
    "ScreeningFallRisk"           boolean,                           -- SENSITIF
    "ScreenedAt"                  timestamp,
    "ScreenedByUserId"            uuid,
    "EncounterId"                 uuid,
    "ClosedAt"                    timestamp,
    "ClosedByUserId"              uuid,
    "ClosureReason"               varchar(500),                      -- SENSITIF
    "IsPrioritySnapshot"          boolean       NOT NULL DEFAULT false,
    "PatientMembershipIdSnapshot" uuid,
    "MembershipTierIdSnapshot"    uuid,
    "MembershipTierCodeSnapshot"  varchar(50),
    "QueuePriorityLevelSnapshot"  integer       NOT NULL DEFAULT 0,
    "QueueAudienceSnapshot"       integer       NOT NULL,
    "PublicDisplayModeSnapshot"   integer       NOT NULL,
    "RowVersion"                  uuid          NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_RegPriorityReservation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RegPriorityReservation_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegPriorityReservation_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegPriorityReservation_MstPatientMembership_PatientMembershipIdSnapshot"
        FOREIGN KEY ("PatientMembershipIdSnapshot") REFERENCES public."MstPatientMembership" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_RegPriorityReservation_ReservationNumber"
    ON public."RegPriorityReservation" ("ReservationNumber");
CREATE UNIQUE INDEX "IX_RegPriorityReservation_ArrivalCodeHash"
    ON public."RegPriorityReservation" ("ArrivalCodeHash");
CREATE UNIQUE INDEX "IX_RegPriorityReservation_EncounterId"
    ON public."RegPriorityReservation" ("EncounterId") WHERE "EncounterId" IS NOT NULL;
CREATE UNIQUE INDEX "UX_RegPriorityReservation_Open_Patient_VisitDate"
    ON public."RegPriorityReservation" ("PatientId", "VisitDate")
    WHERE "Status" IN (1, 2) AND NOT "IsDelete";
CREATE INDEX "IX_RegPriorityReservation_VisitDate_Status"
    ON public."RegPriorityReservation" ("VisitDate", "Status");
CREATE INDEX "IX_RegPriorityReservation_PatientMembershipIdSnapshot"
    ON public."RegPriorityReservation" ("PatientMembershipIdSnapshot");

CREATE TABLE public."RegPriorityReservationHistory" (
    "Id"                     uuid          NOT NULL,
    "PriorityReservationId"  uuid          NOT NULL,
    "EventCode"              varchar(40)   NOT NULL,
    "FromStatus"             integer,
    "ToStatus"               integer       NOT NULL,
    "Source"                 integer       NOT NULL,
    "ActorUserId"            uuid,
    "Reason"                 varchar(500),                -- SENSITIF
    "EncounterId"            uuid,
    "OccurredAt"             timestamp     NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_RegPriorityReservationHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RegPriorityReservationHistory_RegPriorityReservation_PriorityReservationId"
        FOREIGN KEY ("PriorityReservationId") REFERENCES public."RegPriorityReservation" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_RegPriorityReservationHistory_PriorityReservationId_OccurredAt"
    ON public."RegPriorityReservationHistory" ("PriorityReservationId", "OccurredAt");
```
