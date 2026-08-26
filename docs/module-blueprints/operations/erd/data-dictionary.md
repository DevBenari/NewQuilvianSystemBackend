# Kamus Data — Modul Operasi

Seluruh tabel baru mewarisi `IdentityModel`: `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom tersebut tidak diulang. Penghapusan bersifat penandaan, bukan penghapusan baris.

## Tabel Baru

| Tabel | Kolom utama lengkap di luar audit | Index/constraint | Sensitif |
|---|---|---|:---:|
| `OprCase` | `Id uuid PK`; `CaseNumber varchar(50)`; `PatientId uuid`; `EncounterId uuid`; `RequesterDoctorId uuid`; `PrimarySurgeonId uuid`; `CaseType int`; `Priority int`; `Status int`; `Outcome int?`; `Indication text`; `Laterality varchar(30)?`; `EstimatedMinutes int`; `RequestedAt timestamp`; `PreferredAt timestamp?`; `Version int` | UK `CaseNumber`; index patient/encounter/status/date; concurrency `Version` | Ya untuk indication |
| `OprCaseProcedure` | `Id`; `OprCaseId`; `PatientProcedureId`; `IsPrimary bool`; `Sequence int` | UK procedure; UK case+sequence; filtered unique primary per case | Tidak |
| `OprSchedule` | `Id`; `OprCaseId`; `RoomId`; `StartAt`; `EndAt`; `BufferBeforeMinutes`; `BufferAfterMinutes`; `Revision`; `IsCurrent`; `ChangeReason varchar(500)?`; `ChangedByUserId` | UK case+revision; filtered unique current; index room/time/current | Tidak |
| `OprTeamMember` | `Id`; `OprCaseId`; `ScheduleId`; `WorkforceId`; `Role int`; `IsLead`; `CredentialCheckStatus int`; `CredentialCheckedAt?`; `IsCurrent` | UK schedule+workforce+role; index workforce/current | Tidak |
| `OprSafetyChecklist` | `Id`; `OprCaseId`; `Phase int`; `TemplateVersion`; `Revision`; `Status int`; `ItemsJson jsonb`; `SignedByUserId?`; `SignedAt?`; `IsEmergencyBypass`; `BypassReason text?`; `BypassResponsibleUserId?`; `CompletedAfterStableAt?` | UK case+phase+revision | Ya |
| `OprExecutionRecord` | `Id`; `OprCaseId`; `Status int`; `PreDiagnosis text`; `PostDiagnosis text`; `Findings text`; `Technique text`; `Complications text?`; `BloodLossMl decimal?`; `SpecimenNote text?`; `ImplantDrainNote text?`; `PostPlan text`; `StartedAt`; `FinishedAt?`; `FinalizedBy?`; `FinalizedAt?`; `Version int` | UK case; concurrency version | Ya |
| `OprExecutionAddendum` | `Id`; `ExecutionRecordId`; `Content text`; `Reason text`; `AuthoredBy`; `AuthoredAt` | index record/time | Ya |
| `OprAnesthesiaRecord` | `Id`; `OprCaseId`; `Status int`; `AssessmentSummary text`; `Technique text`; `MedicationFluidSummary text`; `AirwaySummary text`; `MonitoringSummary text`; `EventSummary text?`; `FinalCondition text`; `FinalizedBy?`; `FinalizedAt?`; `Version int` | UK case; concurrency version | Ya |
| `OprMaterialUsage` | `Id`; `OprCaseId`; `ExternalItemId`; `ItemType int`; `Quantity decimal`; `UnitCode varchar(30)`; `Outcome int`; `BatchNumber varchar(100)?`; `SerialNumber varchar(150)?`; `OccurredAt`; `RecordedBy`; `Revision`; `CorrectionReason text?` | index case/item; index batch/serial; UK case+id+revision logical | Ya untuk traceability pasien |
| `OprRecovery` | `Id`; `OprCaseId`; `Status int`; `ScoreSystem varchar(100)`; `ScoreValue decimal?`; `ObservationJson jsonb`; `Decision int`; `DecisionNote text?`; `ReleasedBy?`; `ReleasedAt?`; `Version int` | UK case; concurrency version | Ya |
| `OprHandover` | `Id`; `OprCaseId`; `DestinationUnitId`; `Status int`; `ConditionSummary text`; `DeviceTherapySummary text?`; `RiskSummary text?`; `InstructionSummary text?`; `SentBy`; `SentAt`; `ReceivedBy?`; `AcceptedAt?`; `RejectionReason text?`; `Revision` | UK case+revision; index destination/status | Ya |
| `OprStatusHistory` | `Id`; `OprCaseId`; `FromStatus int?`; `ToStatus int`; `Action varchar(50)`; `Reason varchar(1000)?`; `ActorUserId`; `OccurredAt`; `Source varchar(50)`; `CorrelationId varchar(100)?` | index case/time; append-only | Alasan dapat sensitif |

### Pemakaian `OprStatusHistory` di luar perpindahan status

Tabel ini tidak hanya menyimpan perpindahan status. Beberapa kejadian dicatat sebagai baris
histori dengan `FromStatus` dan `ToStatus` yang sama, sehingga jejaknya tetap append-only tanpa
menambah tabel baru.

| Nilai `Action` | Kejadian yang dicatat | Isi `Reason` | Dasar |
|---|---|---|---|
| `UpdateRequest` | Perbaikan data permintaan sebelum dijadwalkan | Kosong | Implementasi `BE-OPR-003` |
| `SaveChecklist` | Penyimpanan checklist keselamatan per fase | `<Fase>:<Draft\|Completed>` | `BE-OPR-005` |
| `ReadinessSignOff` | **Sign-off kesiapan satu peran** | `<Peran>` atau `<Peran>\|<catatan>` | `OPS-DEC-026` |
| `EmergencyBypass` | Pencatatan jalur darurat | `EmergencyBypass` | `BE-OPR-005` |

Nilai peran yang sah pada `ReadinessSignOff` hanya `PrimarySurgeon`, `Anesthesiologist`, dan
`Nurse`. Gerbang menuju `Ready` menghitung peran unik dari baris-baris tersebut; baris kedua
untuk peran yang sama ditolak dengan kode `OPR006`.

> **Contoh.** Perawat sirkuler memberi sign-off ketiga. Sistem menulis satu baris
> `Action = "ReadinessSignOff"`, `Reason = "Nurse"`, lalu satu baris
> `Action = "CompleteReadiness"` dengan `FromStatus = Scheduled` dan `ToStatus = Ready`.
> Jadi tiga sign-off menghasilkan **satu** perpindahan status, bukan tiga.

Keterbatasan yang diterima: peran bukan kolom tersendiri, sehingga penyaringan sign-off
berupa pencocokan teks. Bila kebutuhan pelaporan sign-off bertambah, gantikan dengan tabel
`OprReadinessSignOff` beserta migration-nya.
| `OprIntegrationDelivery` | `Id`; `OprCaseId`; `Destination varchar(50)`; `MessageType varchar(100)`; `IdempotencyKey varchar(150)`; `CorrelationId varchar(100)`; `PayloadReference varchar(250)`; `Status int`; `RetryCount`; `LastAttemptAt?`; `LastErrorCode varchar(100)?`; `AcceptedReference varchar(150)?` | UK destination+key; index status/retry | Tidak; payload klinis tidak disimpan di log |

Semua ID wajib, kecuali ditandai `?`. Enum disimpan sebagai integer. Timestamp disimpan UTC. String klinis diberi batas panjang pada configuration dan tidak boleh masuk custom logger.

## Existing Reference

| Model | Kolom yang dipakai | Sumber |
|---|---|---|
| `TrxPatientEncounter` | `Id`, patient, service context/status | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs` |
| `TrxPatientProcedure` | `Id`, encounter/patient/doctor/procedure, status, surgery flag | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` |
| `TrxPatientConsent` | `Id`, patient/encounter/procedure, type/status, validity | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientConsent.cs` |
| `MstRoom` | `Id`, service unit, type, active | `Areas/HealthServices/MasterData/Models/MstRoom.cs` |
| `MstDoctor`/workforce | ID, active/credential references | HR Workforce owner |

## DDL Dokumentasi Target

Basis data dibentuk EF Core Migration. DDL berikut hanya dokumentasi bentuk, **bukan skrip untuk dijalankan**. Configuration target belum dibuat oleh blueprint; implementer wajib menghasilkan DDL melalui configuration EF canonical dan memverifikasinya sebelum migration.

```sql
-- Ringkasan pola target; bukan skrip eksekusi.
CREATE TABLE public."OprCase" (
  "Id" uuid NOT NULL,
  "CaseNumber" varchar(50) NOT NULL,
  "PatientId" uuid NOT NULL,
  "EncounterId" uuid NOT NULL,
  "Status" integer NOT NULL,
  "Indication" text NOT NULL, -- SENSITIF
  "Version" integer NOT NULL,
  CONSTRAINT "PK_OprCase" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_OprCase_CaseNumber" ON public."OprCase" ("CaseNumber");

-- Tabel anak mengikuti seluruh kolom pada kamus di atas, PK Id,
-- FK OprCaseId ON DELETE RESTRICT, enum integer, serta index/UK yang disebutkan.
-- Kolom audit IdentityModel tidak ditulis ulang di sini.
```

DDL lengkap diblokir sampai file `*Configuration.cs` benar-benar dibuat pada task backend; blueprint tidak boleh mengarang hasil final migration.
