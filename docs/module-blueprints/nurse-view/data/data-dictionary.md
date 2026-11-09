# Kamus Data — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` · revision `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| `last_changed_in` | `0.1.2` (`NV-DEC-033`, 2026-11-03). Sebelumnya `0.1.0` |
| Masukan | `02-backend-architecture.md` rev `1` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

> **Revision 2 (`approved` `NV-DEC-055`, `0.2.0`) — 2026-11-05.** Tabel tablet bed dan panggilan pasien ada di
> [Revision 2](#revision-2--tablet-bed-dan-panggilan-pasien). Bagian 1–4 tetap revision 1.

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
| `TrxRosterAssignment` | `Id`, `ProfessionId`, `RosterPeriodId`, `IsActive` | `.../SchedulingManagement/Models/TrxRosterAssignment.cs` |
| `TrxRosterPeriod` | `OrganizationUnitId` (cadangan unit, `NV-DEC-033`) | `.../SchedulingManagement/Models/TrxRosterPeriod.cs` |
| `MstWorkforceProfile`, `MstEmployee`, `MstProfession`, `MstShift` | `DisplayName`; `ProfessionId` (cadangan profesi, `NV-DEC-033`); `ProfessionCode`, `ProfessionName`, `ProfessionGroup`, `IsClinicalProfession`; `ShiftName` | `Areas/Corporate/HumanResource/MasterData/**` |
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

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.1` · status `approved` — Rizki Gunawan, 2026-11-09 (`NV-DEC-057`): pemetaan `EndReason` R2.3 dan nilai `NvwPatientCallCloseReason.AdmissionCancelled`. Sebelumnya `0.2.0`, 2026-11-05 (`NV-DEC-055`) |
| Masukan | `02-backend-architecture.md` Revision 2; `00-interview-decisions.md` rev `5` (`NV-DEC-034`–`054`) |

Kepala dokumen tetap berlaku: keempat tabel baru mewarisi `IdentityModel`, dan kolom auditnya
tidak diulang. Seluruh waktu disimpan UTC dan ditampilkan dalam jam Asia/Jakarta.

### R2.1 Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `NvwBedTablet` | **Baru** | Nurse View | Prefix `Nvw` `ACTIVE` di registry |
| `NvwPatientCall` | **Baru** | Nurse View | Satu panggilan aktif per bed |
| `NvwPatientCallEvent` | **Baru** | Nurse View | Jejak; hanya ditambah |
| `NvwPatientCallWaitThreshold` | **Baru** | Nurse View | Dibuat kosong (`G-15`) |
| `InpBedPlacement` | Sudah ada | Rawat Inap | Dibaca: penempatan aktif dan akhir penempatan. **MUST NOT** disalin |
| `MstBed`, `MstRoom` | Sudah ada | Master Data HealthServices | Dibaca: kode bed, kamar, lantai |
| `MstPatient` | Sudah ada | Patient Management | Dibaca untuk inisial di konsol |
| `ApplicationUser` | Sudah ada | Platform Identity | Satu baris per tablet; `DisplayName` perawat pengklaim |

### R2.2 Tabel baru

#### `NvwBedTablet`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DeviceCode` | `string(30)` | Ya | dari number series | **Unique** | — | — | Tidak | Contoh `NVT-000001` (`AD-03`). Tidak dapat diubah |
| `DeviceName` | `string(100)` | Ya | — | — | — | — | Tidak | Contoh "Tablet Bed 305-A" |
| `BedId` | `Guid` | Ya | — | **Unique (filter `"DeviceStatus" <> 3 AND "IsDelete" = false`)**; index `(BedId, DeviceStatus)` | FK ke `MstBed.Id` | `Restrict` | Tidak | Satu bed paling banyak satu tablet yang belum nonaktif (`AD-01`) |
| `LocationNote` | `string(150)` | Tidak | — | — | — | — | Tidak | Contoh "Dinding kanan kepala bed" |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | Catatan admin |
| `DeviceStatus` | `int` (enum `NvwBedTabletStatus`) | Ya | `1` `PendingLogin` | ikut index di atas | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` §4 |
| `LoginUserId` | `Guid?` | Tidak | — | **Unique (filter `IS NOT NULL`)** | FK ke `ApplicationUser.Id` | `Restrict` | Tidak | Terisi sesudah login dibuat |
| `LoginIssuedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Dasar stempel sesi; sesi lama gugur sesudah atur ulang login |
| `LoginIssuedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Admin pembuat login |
| `LastOnlineAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Ditulis paling sering 1×/60 detik |
| `DeactivatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DeactivatedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `DeactivationReason` | `string(500)` | Tidak | — | — | — | — | Tidak | Wajib saat nonaktifkan (5–500) |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Concurrency token |

#### `NvwPatientCall`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BedId` | `Guid` | Ya | — | **Unique (filter `"CallStatus" IN (1, 2) AND "IsDelete" = false`)** `IX_NvwPatientCall_BedId_Active` | FK ke `MstBed.Id` | `Restrict` | Tidak | Satu panggilan aktif per bed (`NV-DEC-039`) |
| `BedTabletId` | `Guid` | Ya | — | Index | FK ke `NvwBedTablet.Id` | `Restrict` | Tidak | Tablet asal panggilan |
| `InpBedPlacementId` | `Guid` | Ya | — | Index | FK ke `InpBedPlacement.Id` | `Restrict` | Tidak | Penempatan aktif saat panggilan dibuat; dipakai pemeriksaan pindah/pulang |
| `InpEpisodeId` | `Guid` | Ya | — | — | — (salinan dari penempatan, tanpa FK) | — | Tidak | Untuk membaca inisial konsol |
| `FloorCode` | `string(20)` | Ya | — | Index `(FloorCode, CallStatus)` | — | — | Tidak | Salinan lantai kamar saat dibuat, huruf besar |
| `Priority` | `int` (enum `NvwPatientCallPriority`) | Ya | — | — | — | — | Tidak | `1` Panggilan, `2` Darurat; hanya naik |
| `CallStatus` | `int` (enum `NvwPatientCallStatus`) | Ya | `1` `Waiting` | ikut index di atas | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` §5 |
| `FirstPressedAt` | `DateTime` | Ya | jam server | Index | — | — | Tidak | Jam awal; tidak pernah berubah |
| `LastPressedAt` | `DateTime` | Ya | = `FirstPressedAt` | — | — | — | Tidak | Tekan terakhir |
| `PressCount` | `int` | Ya | `1` | — | — | — | Tidak | Contoh "ditekan 3×" |
| `EscalatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat naik menjadi Darurat; sama dengan `FirstPressedAt` bila sejak awal Darurat |
| `ClaimedByUserId` | `Guid?` | Tidak | — | — | — (tanpa FK; nama dibaca dari `ApplicationUser`) | — | Tidak | Pengklaim saat ini |
| `ClaimedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Klaim atau ambil alih terakhir |
| `ClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Terisi saat `Handled`, `Cancelled`, `ClosedBySystem` |
| `ClosedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Perawat yang menutup dari konsol, atau pengklaim saat "Perawat hadir"; kosong untuk pasien dan sistem |
| `CloseReason` | `int?` (enum `NvwPatientCallCloseReason`) | Tidak | — | — | — | — | Tidak | Wajib bila selesai |
| `CloseNote` | `string(250)` | Tidak | — | — | — | — | **Ya** | Wajib bila `CloseReason = Other` (5–250). Bisa memuat keterangan kondisi pasien; **MUST NOT** masuk log |
| `IsBedsideConfirmed` | `bool` | Ya | `false` | — | — | — | Tidak | `true` hanya untuk "Perawat hadir" di tablet (`NV-DEC-053`) |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Concurrency token |

#### `NvwPatientCallEvent`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PatientCallId` | `Guid` | Ya | — | Index `(PatientCallId, OccurredAt)` | FK ke `NvwPatientCall.Id` | `Restrict` | Tidak | — |
| `EventType` | `int` (enum `NvwPatientCallEventType`) | Ya | — | — | — | — | Tidak | Sepuluh jenis kejadian |
| `ActorKind` | `int` (enum `NvwPatientCallActorKind`) | Ya | — | — | — | — | Tidak | Tablet, perawat, atau sistem |
| `ActorUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Akun tablet atau perawat; kosong untuk sistem |
| `PreviousClaimerUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Diisi pada `TakenOver` dan `Released` |
| `CloseReason` | `int?` | Tidak | — | — | — | — | Tidak | Diisi pada kejadian penutupan |
| `Note` | `string(250)` | Tidak | — | — | — | — | **Ya** | Salinan `CloseNote`; **MUST NOT** masuk log |
| `OccurredAt` | `DateTime` | Ya | jam server | ikut index | — | — | Tidak | — |
| `IdempotencyKey` | `Guid?` | Tidak | — | **Unique (filter `IS NOT NULL`)** | — | — | Tidak | Dari header `Idempotency-Key` permintaan tablet |

#### `NvwPatientCallWaitThreshold`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ScopeCode` | `string(20)` | Ya | `GLOBAL` | **Unique (filter `"IsDelete" = false`)** | — | — | Tidak | MVP hanya `GLOBAL` |
| `WarningAfterMinutes` | `int` | Ya | **tanpa bawaan** | — | — | — | Tidak | 1–240. Lewat angka ini item kuning |
| `CriticalAfterMinutes` | `int` | Ya | **tanpa bawaan** | — | — | — | Tidak | Lebih besar dari `WarningAfterMinutes`, paling banyak 480. Lewat angka ini item merah |
| `RatificationReference` | `string(200)` | Ya | — | — | — | — | Tidak | Nomor SK/notulen Komite Keperawatan |
| `RatifiedOn` | `DateOnly` | Ya | — | — | — | — | Tidak | Tidak boleh di masa depan |

**Contoh perhitungan warna.** Komite mengesahkan kuning 3 menit dan merah 5 menit (angka
contoh). Panggilan 305-A ditekan 10.00.00. Pukul 10.02.30 item tanpa warna (2,5 menit); pukul
10.03.00 kuning; pukul 10.05.00 merah. Selama tabel kosong, ketiganya tampil tanpa warna.

### R2.3 Tabel sudah ada — kolom yang dipakai revision 2

| Tabel | Kolom | Dipakai untuk | Rujukan |
|---|---|---|---|
| `InpBedPlacement` | `Id`, `BedId`, `EpisodeId`, `EndDateTime`, `EndReason`, `SupersededByCorrectionId`, `IsDelete` | Penempatan aktif (`EndDateTime IS NULL`, unik per bed `IX_InpBedPlacement_BedId_Active`); akhir penempatan untuk pemeriksaan pindah/pulang | `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs`; `Enums/InpBedPlacementEndReason.cs` |
| `MstBed` | `Id`, `RoomId`, `BedCode`, `BedName`, `IsActive`, `IsDelete` | Label bed di tablet, strip, dan konsol | `Areas/HealthServices/MasterData/Models/MstBed.cs` |
| `MstRoom` | `Id`, `RoomName`, `FloorCode`, `IsActive`, `IsDelete` | Lantai bed | `Areas/HealthServices/MasterData/Models/MstRoom.cs` |
| `MstPatient` | `FullName`, `Gender`, `BirthDate` | Inisial, jenis kelamin, umur di konsol — dibaca di memori, langsung disamarkan | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` |
| `ApplicationUser` | `Id`, `DisplayName`, `UserName` | Akun tablet; nama perawat pengklaim | `Models/ApplicationUser.cs` |

Pemetaan `EndReason` ke penutupan panggilan (`NV-DEC-050`, `057`). Pemeriksaan lebih dulu mengikuti `SupersededByCorrectionId` sampai baris penempatan terkini, lalu menilai baris itu:

| `EndReason` | Panggilan aktif | Alasan tercatat |
|---|---|---|
| `Transfer` | Ditutup sistem | `PatientTransferred` ("pasien pindah") |
| `EpisodeClosed`, `PatientDeparted` | Ditutup sistem | `PatientDischarged` ("pasien pulang") |
| `AdmissionCancelled` | Ditutup sistem (`NV-DEC-057`) | `AdmissionCancelled` ("admisi dibatalkan") |
| Baris terkini di **bed lain** (koreksi salah pilih bed) | Ditutup sistem (`NV-DEC-057`) | `PatientTransferred` ("pasien pindah") |
| Baris terkini di bed yang sama dan masih aktif (koreksi kelas atau jam) | **Tidak** ditutup; dinilai ulang tiap putaran | — |

### R2.4 Skema DDL

> **Peringatan.** Sama seperti bagian 4: dokumentasi bentuk tabel, **bukan** skrip yang
> dijalankan. Sumber kebenaran nanti configuration di
> `Repositories/Configurations/HealthServices/NurseViewManagement/`. Kolom audit `IdentityModel`
> tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."NvwBedTablet" (
    "Id"                  uuid          NOT NULL,
    "DeviceCode"          varchar(30)   NOT NULL,
    "DeviceName"          varchar(100)  NOT NULL,
    "BedId"               uuid          NOT NULL,
    "LocationNote"        varchar(150),
    "Description"         varchar(500),
    "DeviceStatus"        integer       NOT NULL DEFAULT 1,  -- enum NvwBedTabletStatus
    "LoginUserId"         uuid,
    "LoginIssuedAt"       timestamp with time zone,
    "LoginIssuedBy"       uuid,
    "LastOnlineAt"        timestamp with time zone,
    "DeactivatedAt"       timestamp with time zone,
    "DeactivatedBy"       uuid,
    "DeactivationReason"  varchar(500),
    "Version"             integer       NOT NULL DEFAULT 0,  -- concurrency token
    CONSTRAINT "PK_NvwBedTablet" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NvwBedTablet_MstBed_BedId"
        FOREIGN KEY ("BedId") REFERENCES public."MstBed" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_NvwBedTablet_AspNetUsers_LoginUserId"
        FOREIGN KEY ("LoginUserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_NvwBedTablet_DeviceCode" ON public."NvwBedTablet" ("DeviceCode");
CREATE UNIQUE INDEX "IX_NvwBedTablet_LoginUserId" ON public."NvwBedTablet" ("LoginUserId")
    WHERE "LoginUserId" IS NOT NULL;
CREATE UNIQUE INDEX "IX_NvwBedTablet_BedId_InUse" ON public."NvwBedTablet" ("BedId")
    WHERE "DeviceStatus" <> 3 AND "IsDelete" = false;

CREATE TABLE public."NvwPatientCall" (
    "Id"                  uuid          NOT NULL,
    "BedId"               uuid          NOT NULL,
    "BedTabletId"         uuid          NOT NULL,
    "InpBedPlacementId"   uuid          NOT NULL,
    "InpEpisodeId"        uuid          NOT NULL,
    "FloorCode"           varchar(20)   NOT NULL,
    "Priority"            integer       NOT NULL,            -- 1 Normal, 2 Emergency
    "CallStatus"          integer       NOT NULL DEFAULT 1,  -- 1 Waiting .. 5 ClosedBySystem
    "FirstPressedAt"      timestamp with time zone NOT NULL,
    "LastPressedAt"       timestamp with time zone NOT NULL,
    "PressCount"          integer       NOT NULL DEFAULT 1,
    "EscalatedAt"         timestamp with time zone,
    "ClaimedByUserId"     uuid,
    "ClaimedAt"           timestamp with time zone,
    "ClosedAt"            timestamp with time zone,
    "ClosedByUserId"      uuid,
    "CloseReason"         integer,
    "CloseNote"           varchar(250),                      -- SENSITIF
    "IsBedsideConfirmed"  boolean       NOT NULL DEFAULT false,
    "Version"             integer       NOT NULL DEFAULT 0,  -- concurrency token
    CONSTRAINT "PK_NvwPatientCall" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NvwPatientCall_MstBed_BedId"
        FOREIGN KEY ("BedId") REFERENCES public."MstBed" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_NvwPatientCall_NvwBedTablet_BedTabletId"
        FOREIGN KEY ("BedTabletId") REFERENCES public."NvwBedTablet" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_NvwPatientCall_InpBedPlacement_InpBedPlacementId"
        FOREIGN KEY ("InpBedPlacementId") REFERENCES public."InpBedPlacement" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_NvwPatientCall_BedId_Active" ON public."NvwPatientCall" ("BedId")
    WHERE "CallStatus" IN (1, 2) AND "IsDelete" = false;
CREATE INDEX "IX_NvwPatientCall_FloorCode_CallStatus" ON public."NvwPatientCall" ("FloorCode", "CallStatus");
CREATE INDEX "IX_NvwPatientCall_InpBedPlacementId" ON public."NvwPatientCall" ("InpBedPlacementId");
CREATE INDEX "IX_NvwPatientCall_FirstPressedAt" ON public."NvwPatientCall" ("FirstPressedAt");

CREATE TABLE public."NvwPatientCallEvent" (
    "Id"                     uuid          NOT NULL,
    "PatientCallId"          uuid          NOT NULL,
    "EventType"              integer       NOT NULL,
    "ActorKind"              integer       NOT NULL,
    "ActorUserId"            uuid,
    "PreviousClaimerUserId"  uuid,
    "CloseReason"            integer,
    "Note"                   varchar(250),                   -- SENSITIF
    "OccurredAt"             timestamp with time zone NOT NULL,
    "IdempotencyKey"         uuid,
    CONSTRAINT "PK_NvwPatientCallEvent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_NvwPatientCallEvent_NvwPatientCall_PatientCallId"
        FOREIGN KEY ("PatientCallId") REFERENCES public."NvwPatientCall" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_NvwPatientCallEvent_PatientCallId_OccurredAt"
    ON public."NvwPatientCallEvent" ("PatientCallId", "OccurredAt");
CREATE UNIQUE INDEX "IX_NvwPatientCallEvent_IdempotencyKey"
    ON public."NvwPatientCallEvent" ("IdempotencyKey") WHERE "IdempotencyKey" IS NOT NULL;

CREATE TABLE public."NvwPatientCallWaitThreshold" (
    "Id"                     uuid          NOT NULL,
    "ScopeCode"              varchar(20)   NOT NULL DEFAULT 'GLOBAL',
    "WarningAfterMinutes"    integer       NOT NULL,  -- tanpa bawaan
    "CriticalAfterMinutes"   integer       NOT NULL,  -- tanpa bawaan
    "RatificationReference"  varchar(200)  NOT NULL,
    "RatifiedOn"             date          NOT NULL,
    CONSTRAINT "PK_NvwPatientCallWaitThreshold" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_NvwPatientCallWaitThreshold_Order"
        CHECK ("WarningAfterMinutes" BETWEEN 1 AND 240 AND "CriticalAfterMinutes" > "WarningAfterMinutes" AND "CriticalAfterMinutes" <= 480)
);
CREATE UNIQUE INDEX "IX_NvwPatientCallWaitThreshold_ScopeCode_Active"
    ON public."NvwPatientCallWaitThreshold" ("ScopeCode") WHERE "IsDelete" = false;
```

Nama tabel `MstBed`, `InpBedPlacement`, dan `AspNetUsers` dicek ulang terhadap configuration
yang ada saat implementasi.
