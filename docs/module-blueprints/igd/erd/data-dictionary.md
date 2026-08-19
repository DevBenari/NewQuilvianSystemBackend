# Kamus Data — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Commit diaudit | backend `e5331a0` |
| Aturan kedalaman | `DEC-RSK-002` — bertingkat mengikuti status tabel |

## Cara membaca dokumen ini

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

Kedalaman dokumentasi mengikuti status tabel:

| Status tabel | Yang didokumentasikan |
| --- | --- |
| `Baru` dan `Diperbarui` | Seluruh kolom |
| `Sudah ada` | Kolom kunci saja — PK, FK, status, dan kolom yang dipakai aturan bisnis modul ini — ditambah rujukan ke file model |

Kolom bertanda **Sensitif** tidak boleh masuk custom logger dan tidak boleh dipakai sebagai
contoh berisi data asli pasien.

Seluruh FK memakai `DeleteBehavior.Restrict`.

---

## 1. `TrxEmergencyTriage` — status `Diperbarui`, seluruh kolom

Sumber: `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | Kunci utama |
| `EmergencyVisitId` | `Guid` | Ya | — | Index | `TrxEmergencyVisit` | Tidak | Kunjungan IGD induk |
| `TriageLevelId` | `Guid` | Ya | — | Index | `MstEmergencyTriageLevel` | Tidak | Level triage yang ditetapkan |
| `PatientVitalSignId` | `Guid?` | Tidak | — | — | `TrxPatientVitalSign` | Tidak | Tanda vital saat triage, milik Clinical Management |
| `Sequence` | `int` | Ya | `1` | — | — | Tidak | Urutan penilaian dalam satu kunjungan |
| `IsRetriage` | `bool` | Ya | `false` | — | — | Tidak | Menandai penilaian ulang |
| `PreviousTriageId` | `Guid?` | Tidak | — | Index | `TrxEmergencyTriage` | Tidak | Penilaian yang digantikan |
| `TriageSystem` | `EmergencyTriageSystem` | Ya | `ATS` | — | — | Tidak | `ATS` atau `ESI`, keduanya skala lima level |
| `TriageStatus` | `EmergencyTriageStatus` | Ya | `Draft` | Index | — | Tidak | `Draft`, `InProgress`, `Completed`, `Superseded`, `Cancelled` |
| `StartedAt` | `DateTime` | Ya | `UtcNow` | Index | — | Tidak | Waktu penilaian dimulai |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Waktu penilaian selesai |
| `MaxWaitingMinutesSnapshot` | `int` | Ya | — | — | — | Tidak | Salinan target dari master saat penilaian dibuat |
| `ResponseDueAt` | `DateTime?` | Tidak | — | Index | — | Tidak | Batas waktu respons, dihitung server dari `StartedAt` + target master |
| `ImmediateCareAllowed` | `bool` | Ya | `false` | — | — | Tidak | Pelayanan boleh dimulai tanpa menunggu administrasi |
| `TriageReason` | `string(1000)` | Tidak | — | — | — | **Ya** | Alasan klinis penetapan level |
| `AirwaySummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan jalan napas |
| `BreathingSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan pernapasan |
| `CirculationSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan sirkulasi |
| `DisabilitySummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan kesadaran dan neurologis |
| `ExposureSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan pemeriksaan paparan |
| `RedFlagSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Tanda bahaya yang ditemukan |
| `PerformedByUserId` | `Guid` | Ya | — | Index | `ApplicationUser` | Tidak | Petugas yang menilai |
| `ReviewedByUserId` | `Guid?` | Tidak | — | — | `ApplicationUser` | Tidak | Peninjau penilaian |
| `ReviewedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Waktu peninjauan |
| `Notes` | `string(1000)` | Tidak | — | — | — | **Ya** | Catatan tambahan |
| `IsActive` | `bool` | Ya | `true` | — | — | Tidak | Penanda aktif |
| **`IsSlaBreached`** | **`bool`** | **Ya** | **`false`** | **Index** | — | **Tidak** | **Baru — target respons terlampaui** |
| **`SlaBreachedAt`** | **`DateTime?`** | **Tidak** | — | — | — | **Tidak** | **Baru — waktu pelampauan tercatat** |

Index gabungan baru: `(EmergencyVisitId, ResponseDueAt, IsSlaBreached)` untuk pencarian
pasien yang melewati batas waktu tunggu.

Delapan kolom ringkasan klinis bertanda sensitif seluruhnya berukuran maksimal 1000 karakter.

---

## 2. Tabel berstatus `Sudah ada` — kolom kunci

Untuk kolom lengkap, buka file model yang dirujuk.

### `TrxEmergencyVisit`

Sumber: `.../EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs`

| Kolom | Tipe | Wajib | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | Kunci utama |
| `EmergencyVisitNumber` | `string` | Ya | Unique | Tidak | Nomor kunjungan, dibentuk `EmergencyDocumentNumberService` |
| `EncounterId` | `Guid?` | Tidak | `TrxPatientEncounter`, unique | Tidak | Penghubung ke seluruh data klinis |
| `PatientId` | `Guid?` | Tidak | Patient | Tidak | Kosong bila pasien belum teridentifikasi |
| `ServiceUnitId` | `Guid` | Ya | Master unit | Tidak | Menentukan konteks IGD |
| `ArrivalModeId` | `Guid?` | Tidak | `MstEmergencyArrivalMode` | Tidak | Cara pasien datang |
| `CaseTypeId` | `Guid?` | Tidak | `MstEmergencyCaseType` | Tidak | Klasifikasi kasus |
| `IsUnknownPatient` | `bool` | Ya | — | Tidak | Pasien tak dikenal, jalur keselamatan |
| `IsImmediateCareAllowed` | `bool` | Ya | — | Tidak | Pelayanan boleh mendahului administrasi |
| `RegistrationStatus` | `EmergencyRegistrationStatus` | Ya | Index | Tidak | `Pending`, `Provisional`, `Registered`, `Completed`, `Cancelled` |
| `VisitStatus` | `EmergencyVisitStatus` | Ya | Index | Tidak | Lihat catatan perubahan enum di bawah |
| `VisitCompletedAt` | `DateTime?` | Tidak | — | Tidak | Terisi saat seluruh kewajiban klinis tuntas |
| `ChiefComplaint` | `string` | Tidak | — | **Ya** | Keluhan utama pasien |

### `TrxEmergencyTriageDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyTriageId`, `TriageIndicatorId` | Kunci dan relasi |
| `IsMatched`, `Sequence` | Indikator cocok atau tidak, beserta urutannya |
| Kolom snapshot master | Menjaga histori tetap utuh saat master indikator berubah |

### `TrxEmergencyResuscitation`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `ResuscitationNumber` | Unique |
| `ResuscitationStatus` | `Planned`, `InProgress`, `Completed`, `Stopped`, `Cancelled` |
| `TeamLeaderDoctorId`, `RecordedByUserId` | Penanggung jawab |

### `TrxEmergencyObservation`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `ObservationNumber` | Unique |
| `ObservationStatus` | `Active`, `Completed`, `Escalated`, `Cancelled` |
| `ResponsibleDoctorId`, `ResponsibleNurseUserId` | Penanggung jawab |

### `TrxEmergencyObservationDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyObservationId` | Kunci dan induk |
| `PatientVitalSignId`, `ProgressNoteId` | Rujukan ke Clinical Management, bukan salinan |
| `RecordedByUserId` | Pencatat |
| Kolom kondisi klinis dan intervensi | **Sensitif** |

### `TrxEmergencyProcedureDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId`, `PatientProcedureId` | `PatientProcedureId` unique, satu banding satu |
| `EmergencyResuscitationId`, `EmergencyObservationId` | Konteks opsional |
| `DetailType` | `General`, `SkinTest`, `TetanusToxoid`, `AntiTetanusSerum`, `EmergencyMedication`, `Resuscitation`, `Other` |

### `TrxEmergencyDisposition`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId`, `DispositionTypeId` | Kunci dan relasi |
| `DispositionStatus` | `Draft`, `Confirmed`, `Executed`, `Cancelled` |
| `DecidedByDoctorId`, `ConfirmedByUserId` | Pemutus dan pengesah |
| `DestinationServiceUnitId`, `ReferralNumber` | Tujuan internal atau rujukan keluar |
| `IsPatientDeceased`, `IsVisumRequested` | Penanda khusus |
| Kolom kondisi saat keluar dan instruksi | **Sensitif** |

### `TrxEmergencyTransfer`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `TransferNumber` | Unique |
| `FromServiceUnitId`, `ToServiceUnitId`, `FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId` | Asal dan tujuan |
| `TransferStatus` | `Requested`, `Accepted`, `InTransit`, `Completed`, `Rejected`, `Cancelled` |
| `RequestedByUserId`, `AcceptedByUserId`, `SendingNurseUserId`, `ReceivingNurseUserId` | Pihak yang terlibat |

---

## 3. Master — kolom kunci

| Tabel | Kolom kunci | Catatan |
| --- | --- | --- |
| `MstEmergencyTriageLevel` | `Id`, `Level`, `Code`, `Name`, `ColorName`, `ColorHex`, `MaxWaitingMinutes`, `Sequence`, `IsActive` | Warna dan target waktu wajib dari sini, tidak boleh di-hardcode |
| `MstEmergencyTriageIndicator` | `Id`, `TriageLevelId`, `Code`, `Name`, `IndicatorGroup`, `Sequence`, `IsActive` | Checklist saat triage |
| `MstEmergencyArrivalMode` | `Id`, `Code`, `Name`, `IsAmbulance`, `IsReferral`, `Sequence`, `IsActive` | Dasar pelaporan rujukan dan ambulans |
| `MstEmergencyCaseType` | `Id`, `Code`, `Name`, `Sequence`, `IsActive` | Klasifikasi kasus |
| `MstEmergencyDispositionType` | `Id`, `Code`, `Name`, `Sequence`, `IsActive` | Menentukan kebutuhan unit tujuan atau rujukan |
| `MstEmergencySetting` | `Id`, `Code`, `Name`, `DefaultEmergencyServiceUnitId`, `IsDefault`, `IsActive` | Hanya satu baris boleh `IsDefault` |

Seluruh master memakai `Code` unique.

---

## 4. Perubahan enum

| Enum | Status | Perubahan |
| --- | --- | --- |
| `EmergencyVisitStatus` | **Diperbarui** | Tambah `Completed = 9` setelah `Disposed = 7` dan `Cancelled = 8`. Nilai lama tidak digeser agar data tersimpan tetap sahih |

Nilai lengkap setelah perubahan: `Arrived`, `WaitingForTriage`, `Triaged`, `InTreatment`,
`UnderObservation`, `AwaitingDisposition`, `Disposed`, `Cancelled`, `Completed`.

---

## 5. Ringkasan kolom sensitif

Kolom berikut memuat informasi klinis pasien. Seluruhnya dilarang masuk custom logger dan
dilarang dipakai sebagai contoh berisi data asli.

| Tabel | Kolom |
| --- | --- |
| `TrxEmergencyVisit` | `ChiefComplaint` |
| `TrxEmergencyTriage` | `TriageReason`, `AirwaySummary`, `BreathingSummary`, `CirculationSummary`, `DisabilitySummary`, `ExposureSummary`, `RedFlagSummary`, `Notes` |
| `TrxEmergencyObservationDetail` | Kondisi klinis, intervensi, dan respons pasien |
| `TrxEmergencyDisposition` | Kondisi saat keluar, instruksi tindak lanjut, alasan penolakan |
| `TrxEmergencyResuscitation` | Pemicu dan hasil resusitasi |

Payload log hanya boleh memuat `EntityId`, nama controller, nama action, dan status.

---

## 6. Skema tabel dalam bentuk DDL

### Peringatan

> **DDL di bawah adalah dokumentasi bentuk tabel, bukan skrip untuk dijalankan.**
>
> Basis data project ini dibentuk EF Core Migrations. Menjalankan DDL ini secara manual akan
> berbenturan dengan riwayat migration. Perubahan struktur dilakukan dengan menambah migration
> baru, bukan dengan menjalankan SQL langsung.

Sumber seluruh detail di bawah adalah file configuration pada
`Repositories/Configurations/HealthService/EmergencyInstallationManagement/`, bukan tebakan.

Basis data PostgreSQL, schema `public`, identifier dikutip karena EF Core memakai penamaan
PascalCase.

### Kolom audit yang berlaku untuk semua tabel

Tidak ditulis ulang pada setiap DDL:

```sql
"CreateDateTime"  timestamp    NOT NULL,
"CreateBy"        uuid         NOT NULL,
"UpdateDateTime"  timestamp,
"UpdateBy"        uuid         NOT NULL,
"DeleteDateTime"  timestamp,
"DeleteBy"        uuid         NOT NULL,
"CancelDateTime"  timestamp,
"CancelBy"        uuid         NOT NULL,
"IsCancel"        boolean      NOT NULL DEFAULT false,
"IsDelete"        boolean      NOT NULL DEFAULT false
```

### 6.1 `TrxEmergencyTriage` — status `Diperbarui`

Dua kolom terakhir adalah penambahan pada revisi ini.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."TrxEmergencyTriage" (
    "Id"                        uuid          NOT NULL,
    "EmergencyVisitId"          uuid          NOT NULL,
    "TriageLevelId"             uuid          NOT NULL,
    "PatientVitalSignId"        uuid,
    "Sequence"                  integer       NOT NULL DEFAULT 1,
    "IsRetriage"                boolean       NOT NULL DEFAULT false,
    "PreviousTriageId"          uuid,
    "TriageSystem"              integer       NOT NULL DEFAULT 1,  -- enum ATS=1, ESI=2
    "TriageStatus"              integer       NOT NULL DEFAULT 1,  -- enum Draft=1 .. Cancelled=5
    "StartedAt"                 timestamp     NOT NULL,
    "CompletedAt"               timestamp,
    "MaxWaitingMinutesSnapshot" integer       NOT NULL,
    "ResponseDueAt"             timestamp,
    "ImmediateCareAllowed"      boolean       NOT NULL DEFAULT false,
    "TriageReason"              varchar(1000),                     -- SENSITIF
    "AirwaySummary"             varchar(1000),                     -- SENSITIF
    "BreathingSummary"          varchar(1000),                     -- SENSITIF
    "CirculationSummary"        varchar(1000),                     -- SENSITIF
    "DisabilitySummary"         varchar(1000),                     -- SENSITIF
    "ExposureSummary"           varchar(1000),                     -- SENSITIF
    "RedFlagSummary"            varchar(1000),                     -- SENSITIF
    "PerformedByUserId"         uuid          NOT NULL,
    "ReviewedByUserId"          uuid,
    "ReviewedAt"                timestamp,
    "Notes"                     varchar(1000),                     -- SENSITIF
    "IsActive"                  boolean       NOT NULL DEFAULT true,
    "IsSlaBreached"             boolean       NOT NULL DEFAULT false,  -- BARU
    "SlaBreachedAt"             timestamp,                             -- BARU
    -- kolom audit IdentityModel, lihat di atas

    CONSTRAINT "PK_TrxEmergencyTriage" PRIMARY KEY ("Id"),

    CONSTRAINT "FK_TrxEmergencyTriage_TrxEmergencyVisit_EmergencyVisitId"
        FOREIGN KEY ("EmergencyVisitId")
        REFERENCES public."TrxEmergencyVisit" ("Id") ON DELETE RESTRICT,

    CONSTRAINT "FK_TrxEmergencyTriage_MstEmergencyTriageLevel_TriageLevelId"
        FOREIGN KEY ("TriageLevelId")
        REFERENCES public."MstEmergencyTriageLevel" ("Id") ON DELETE RESTRICT,

    CONSTRAINT "FK_TrxEmergencyTriage_TrxPatientVitalSign_PatientVitalSignId"
        FOREIGN KEY ("PatientVitalSignId")
        REFERENCES public."TrxPatientVitalSign" ("Id") ON DELETE RESTRICT,

    CONSTRAINT "FK_TrxEmergencyTriage_TrxEmergencyTriage_PreviousTriageId"
        FOREIGN KEY ("PreviousTriageId")
        REFERENCES public."TrxEmergencyTriage" ("Id") ON DELETE RESTRICT,

    CONSTRAINT "FK_TrxEmergencyTriage_AspNetUsers_PerformedByUserId"
        FOREIGN KEY ("PerformedByUserId")
        REFERENCES public."AspNetUsers" ("Id") ON DELETE RESTRICT,

    CONSTRAINT "FK_TrxEmergencyTriage_AspNetUsers_ReviewedByUserId"
        FOREIGN KEY ("ReviewedByUserId")
        REFERENCES public."AspNetUsers" ("Id") ON DELETE RESTRICT
);

-- Index yang sudah ada
CREATE UNIQUE INDEX "IX_TrxEmergencyTriage_EmergencyVisitId_Sequence"
    ON public."TrxEmergencyTriage" ("EmergencyVisitId", "Sequence");

CREATE INDEX "IX_TrxEmergencyTriage_EmergencyVisitId_TriageStatus_StartedAt"
    ON public."TrxEmergencyTriage" ("EmergencyVisitId", "TriageStatus", "StartedAt");

CREATE INDEX "IX_TrxEmergencyTriage_PatientVitalSignId"
    ON public."TrxEmergencyTriage" ("PatientVitalSignId");

CREATE INDEX "IX_TrxEmergencyTriage_PreviousTriageId"
    ON public."TrxEmergencyTriage" ("PreviousTriageId");

CREATE INDEX "IX_TrxEmergencyTriage_ResponseDueAt"
    ON public."TrxEmergencyTriage" ("ResponseDueAt");

-- Index BARU untuk pencarian pasien yang melampaui batas waktu
CREATE INDEX "IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBreached"
    ON public."TrxEmergencyTriage" ("EmergencyVisitId", "ResponseDueAt", "IsSlaBreached");
```

Index unik `("EmergencyVisitId", "Sequence")` adalah pengaman penting: ia mencegah dua
penilaian dengan urutan sama pada satu kunjungan, sehingga rantai retriage tidak bercabang.

### 6.2 Migration yang diperlukan

```sql
-- Setara dengan migration AddTriageSlaBreachMarker
ALTER TABLE public."TrxEmergencyTriage"
    ADD COLUMN "IsSlaBreached" boolean NOT NULL DEFAULT false,
    ADD COLUMN "SlaBreachedAt" timestamp;

CREATE INDEX "IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBreached"
    ON public."TrxEmergencyTriage" ("EmergencyVisitId", "ResponseDueAt", "IsSlaBreached");
```

Dapat dijalankan tanpa mematikan layanan. Baris lama terisi `false` melalui nilai bawaan;
breach historis tidak dihitung ulang karena `ResponseDueAt` lama tidak selalu terisi.

Cara mundur:

```sql
DROP INDEX public."IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBreached";
ALTER TABLE public."TrxEmergencyTriage"
    DROP COLUMN "SlaBreachedAt",
    DROP COLUMN "IsSlaBreached";
```

### 6.3 Perubahan enum `EmergencyVisitStatus`

Tidak memerlukan perubahan skema. Enum disimpan sebagai `integer` melalui `HasConversion<int>`
dan tidak ada check constraint pada kolom tersebut.

```text
Arrived = 1, WaitingForTriage = 2, Triaged = 3, InTreatment = 4,
UnderObservation = 5, AwaitingDisposition = 6, Disposed = 7, Cancelled = 8,
Completed = 9   <- BARU
```

Nilai `9` dipilih agar nilai yang sudah tersimpan tidak bergeser.

Bila kemudian check constraint ditambahkan pada kolom ini, ia **wajib** memuat nilai `9`.

### 6.4 Tabel berstatus `Sudah ada`

DDL tidak ditulis ulang karena tidak ada perubahan. Bentuk tabelnya dapat dibaca langsung
pada file configuration berikut:

| Tabel | File configuration |
| --- | --- |
| `TrxEmergencyVisit` | `Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyVisitConfiguration.cs` |
| `TrxEmergencyTriageDetail` | `.../TrxEmergencyTriageDetailConfiguration.cs` |
| `TrxEmergencyResuscitation` | `.../TrxEmergencyResuscitationConfiguration.cs` |
| `TrxEmergencyObservation` | `.../TrxEmergencyObservationConfiguration.cs` |
| `TrxEmergencyObservationDetail` | `.../TrxEmergencyObservationDetailConfiguration.cs` |
| `TrxEmergencyProcedureDetail` | `.../TrxEmergencyProcedureDetailConfiguration.cs` |
| `TrxEmergencyDisposition` | `.../TrxEmergencyDispositionConfiguration.cs` |
| `TrxEmergencyTransfer` | `.../TrxEmergencyTransferConfiguration.cs` |
| Enam tabel master `MstEmergency*` | Configuration master pada folder yang sama |
