# Kamus Data — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` · revision `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| `last_changed_in` | `0.1.0` |
| Masukan | `02-backend-architecture.md` rev `1` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `NvwDisplayDevice` | **Baru** | Nurse View | Prefix `Nvw` disetujui pemilik modul (`NV-DEC-029`); baris registry ditulis di MVP-0 |
| `NvwVitalSignInterval` | **Baru** | Nurse View | Dibuat kosong (`G-01`) |
| `MstBed`, `MstRoom`, `MstServiceUnit` | Sudah ada | Master Data HealthServices | Dibaca |
| `InpEpisode`, `InpBedPlacement`, `InpBedReservation`, `InpBedReadiness`, `InpNurseAssignment` | Sudah ada | Rawat Inap | Dibaca, **MUST NOT** disalin |
| `TrxPatientVitalSign`, `TrxPatientAssessment`, `CliAssessmentInstrumentResponse`, `CliClinicalInstrumentVersion`, `CliClinicalInstrument`, `TrxPatientAllergy` | Sudah ada | Clinical Management | Dibaca |
| `MstPatient` | Sudah ada | Patient Management | Dibaca lalu disamarkan |
| `TrxShiftAssignment`, `TrxRosterAssignment`, `MstProfession`, `MstEmployee`, `MstWorkforceProfile` | Sudah ada | HR | Dibaca |
| `ApplicationUser` | Sudah ada | Platform Identity | Satu baris per perangkat |

## 2. Tabel baru

### 2.1 `NvwDisplayDevice`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DeviceCode` | `string(30)` | Ya | dari number series | **Unique** | — | — | Tidak | Contoh `NVW-000001`. Tidak dapat diubah |
| `DeviceName` | `string(100)` | Ya | — | — | — | — | Tidak | Contoh "TV Nurse Station Lantai 3" |
| `FloorCode` | `string(20)` | Ya | — | Index | — (cocok dengan `MstRoom.FloorCode`, tanpa FK) | — | Tidak | Disimpan huruf besar tanpa spasi tepi. Contoh `3` |
| `LocationName` | `string(150)` | Tidak | — | — | — | — | Tidak | Contoh "Nurse station sayap timur" |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | Catatan bebas admin |
| `DeviceStatus` | `int` (enum `NvwDisplayDeviceStatus`) | Ya | `1` `PendingLogin` | Index | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` |
| `LoginUserId` | `Guid?` | Tidak | — | **Unique (filter `IS NOT NULL`)** | FK ke `ApplicationUser.Id` | `Restrict` | Tidak | Terisi sesudah login dibuat |
| `LoginIssuedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | UTC; terakhir login dibuat/diatur ulang |
| `LoginIssuedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Admin yang membuat/mengatur ulang login |
| `LastOnlineAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | UTC; diperbarui paling sering 1×/60 detik |
| `DeactivatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | UTC |
| `DeactivatedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `DeactivationReason` | `string(500)` | Tidak | — | — | — | — | Tidak | Wajib diisi saat nonaktifkan |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Concurrency token; naik pada setiap perubahan |

### 2.2 `NvwVitalSignInterval`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EwsRiskLevel` | `int` (enum `EwsRiskLevel` milik Clinical) | Ya | — | **Unique (filter `IsDelete = false`)** | — | — | Tidak | Hanya `Low=1`, `Medium=2`, `High=3`, `Critical=4`; `Unknown=0` ditolak |
| `IntervalMinutes` | `int` | Ya | **tanpa bawaan** | — | — | — | Tidak | 1–1440. Contoh `Medium` = 240 (4 jam) |
| `RatificationReference` | `string(200)` | Ya | — | — | — | — | Tidak | Nomor SK/notulen Komite. Contoh "SK-KOMKEP/012/X/2026" |
| `RatifiedOn` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal pengesahan, tidak boleh di masa depan |

## 3. Tabel sudah ada — kolom yang dipakai aturan modul ini

| Entity | Kolom kunci yang dipakai | File model |
|---|---|---|
| `MstRoom` | `Id`, `ServiceUnitId`, `FloorCode`, `RoomName`, `RoomNumber`, `SortOrder`, `IsActive`, `IsIsolationRoom` | `Areas/HealthServices/MasterData/Models/MstRoom.cs` |
| `MstBed` | `Id`, `RoomId`, `BedCode`, `BedName`, `BedNumber`, `SortOrder`, `IsActive`, `IsIsolationBed`, `IsForNewborn`, `BedStatus` | `Areas/HealthServices/MasterData/Models/MstBed.cs` |
| `MstServiceUnit` | `Id`, `ServiceUnitName`, `OrganizationUnitId` | `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` |
| `InpEpisode` | `Id`, `PatientId`, `ServiceUnitId`, `EpisodeStatus`, `RequiresIsolation` | `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` |
| `InpBedPlacement`, `InpBedReservation` | `BedId`, `EpisodeId`, `EndDateTime`, status reservasi | Rawat Inap `Models/` |
| `InpNurseAssignment` | `EpisodeId`, `EmployeeId`, `IsActive`, `EndDateTime` | `.../InPatientManagement/Models/InpNurseAssignment.cs` |
| `TrxPatientVitalSign` | `InpEpisodeId`, `ObservationDateTime`, `VitalSignStatus`, `IsActive`, `EarlyWarningScore`, `EwsRiskLevel`, `IsCritical`, `BloodPressureSystolic`, `BloodPressureDiastolic`, `PulseRate`, `RespiratoryRate`, `Temperature`, `OxygenSaturation` | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs` |
| `TrxPatientAssessment` | `Id`, `InpEpisodeId`, `AssessmentStatus` | `.../ClinicalManagement/Models/TrxPatientAssessment.cs` |
| `CliAssessmentInstrumentResponse` | `AssessmentId`, `InstrumentVersionId`, `BandCode`, `IsAlertBand`, `ComputedAt` | `.../ClinicalManagement/Models/CliAssessmentInstrumentResponse.cs` |
| `CliClinicalInstrument` | `Id`, `InstrumentKind` | `.../ClinicalManagement/Models/CliClinicalInstrument.cs` |
| `TrxPatientAllergy` | `PatientId`, `AllergyStatus`, `IsDelete` | `.../ClinicalManagement/Models/TrxPatientAllergy.cs` |
| `MstPatient` | `Id`, `FullName` **(Sensitif)**, `Gender`, `BirthDate` **(Sensitif)**, `MedicalRecordNumber` **(Sensitif, tidak dibaca)** | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` |
| `TrxShiftAssignment` | `RosterAssignmentId`, `OrganizationUnitId`, `ScheduledStartAt`, `ScheduledEndAt`, `AssignmentStatus`, `IsDayOff`, `WorkforceProfileId` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxShiftAssignment.cs` |
| `TrxRosterAssignment` | `Id`, `ProfessionId` | `.../SchedulingManagement/Models/TrxRosterAssignment.cs` |
| `ApplicationUser` | `Id`, `UserName`, `UserCode`, `DisplayName`, `LockoutEnd` | `Models/ApplicationUser.cs` |

**Catatan sensitif.** `FullName` dan `BirthDate` dibaca **hanya** di dalam
`NvwFloorBoardQueryService` untuk dihitung menjadi inisial dan umur. Keduanya **MUST NOT** keluar
lewat response, sinyal realtime, maupun log.

## 4. Skema DDL

> **Peringatan.** Basis data proyek ini dibentuk EF Core Migrations. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Sumber kebenarannya nanti file
> configuration di `Repositories/Configurations/HealthServices/NurseViewManagement/`. Kolom audit
> `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."NvwDisplayDevice" (
    "Id"                  uuid          NOT NULL,
    "DeviceCode"          varchar(30)   NOT NULL,
    "DeviceName"          varchar(100)  NOT NULL,
    "FloorCode"           varchar(20)   NOT NULL,
    "LocationName"        varchar(150),
    "Description"         varchar(500),
    "DeviceStatus"        integer       NOT NULL DEFAULT 1,  -- enum, HasConversion<int>
    "LoginUserId"         uuid,
    "LoginIssuedAt"       timestamp with time zone,
    "LoginIssuedBy"       uuid,
    "LastOnlineAt"        timestamp with time zone,
    "DeactivatedAt"       timestamp with time zone,
    "DeactivatedBy"       uuid,
    "DeactivationReason"  varchar(500),
    "Version"             integer       NOT NULL DEFAULT 0,  -- concurrency token
    CONSTRAINT "PK_NvwDisplayDevice" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NvwDisplayDevice_AspNetUsers_LoginUserId"
        FOREIGN KEY ("LoginUserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_NvwDisplayDevice_DeviceCode" ON public."NvwDisplayDevice" ("DeviceCode");
CREATE UNIQUE INDEX "IX_NvwDisplayDevice_LoginUserId" ON public."NvwDisplayDevice" ("LoginUserId")
    WHERE "LoginUserId" IS NOT NULL;
CREATE INDEX "IX_NvwDisplayDevice_FloorCode_DeviceStatus"
    ON public."NvwDisplayDevice" ("FloorCode", "DeviceStatus");

CREATE TABLE public."NvwVitalSignInterval" (
    "Id"                     uuid          NOT NULL,
    "EwsRiskLevel"           integer       NOT NULL,  -- enum EwsRiskLevel, 1..4
    "IntervalMinutes"        integer       NOT NULL,  -- 1..1440, tanpa bawaan
    "RatificationReference"  varchar(200)  NOT NULL,
    "RatifiedOn"             date          NOT NULL,
    CONSTRAINT "PK_NvwVitalSignInterval" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_NvwVitalSignInterval_Level" CHECK ("EwsRiskLevel" BETWEEN 1 AND 4),
    CONSTRAINT "CK_NvwVitalSignInterval_Minutes" CHECK ("IntervalMinutes" BETWEEN 1 AND 1440)
);
CREATE UNIQUE INDEX "IX_NvwVitalSignInterval_EwsRiskLevel_Active"
    ON public."NvwVitalSignInterval" ("EwsRiskLevel") WHERE "IsDelete" = false;
```

Nama tabel Identity (`AspNetUsers`) dan tipe kolom tanggal dicek ulang terhadap configuration
yang ada saat implementasi; DDL ini hanya menggambarkan bentuk.
