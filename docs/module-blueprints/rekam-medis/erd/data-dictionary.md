# Kamus Data — Rekam Medis Existing-First

| Field | Nilai |
| --- | --- |
| Status | `draft` |
| Scope | Kolom kunci entity existing yang dipakai fondasi Rekam Medis |
| Snapshot source | Backend `5103e68eec5529540d369673c8a4e2651be0344b` |

Seluruh tabel mewarisi `IdentityModel`: `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`,
`DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom itu
tidak diulang. Penghapusan existing berupa penandaan, tetapi hal tersebut belum memenuhi kebutuhan
histori immutable Rekam Medis untuk record final.

Semua tabel di bawah berstatus `Sudah ada`, sehingga hanya PK, FK, status, waktu, dan kolom aturan
bisnis yang dicatat. Isi SOAP, diagnosis, hasil pemeriksaan, identitas penanda tangan, dan lokasi file
ditandai sensitif dan tidak boleh masuk custom logger.

## `TrxPatientAssessment` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID assessment owner. |
| `EncounterId` | `Guid` | Ya | Config owner | Encounter | **Ya** | Anchor pelayanan. |
| `QueueId` | `Guid` | Ya | Config owner | Queue | Tidak | Antrean pelayanan. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Harus cocok dengan encounter. |
| `ServiceUnitId` | `Guid` | Ya | Config owner | Service unit | Tidak | Konteks unit. |
| `ClinicId` / `DoctorId` | `Guid?` | Tidak | Config owner | Clinic/doctor | **Ya** | Konteks klinik dan dokter. |
| `AssessmentStatus` | Enum/int | Ya | Config owner | — | Tidak | `Draft`, `InProgress`, `Completed`, `Cancelled`. |
| `AssessmentDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu assessment. |
| `ChiefComplaint` dan kelompok assessment klinis | Beragam | Tidak | — | — | **Ya** | Isi assessment; tidak disalin ke log. |
| `CompletedAt` / `CompletedByUserId` | `DateTime?` / `Guid?` | Tidak | — | User | **Ya** | Completion existing, bukan signature evidence RM. |

## `TrxDoctorConsultation` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID konsultasi owner. |
| `EncounterId` / `QueueId` / `PatientId` | `Guid` | Ya | Config owner | Encounter/queue/patient | **Ya** | Konteks pelayanan. |
| `AssessmentId` | `Guid?` | Tidak | Config owner | Assessment | **Ya** | Assessment sumber bila tersedia. |
| `DoctorId` / `ServiceUnitId` | `Guid` | Ya | Config owner | Doctor/service unit | **Ya** | Pelaksana dan unit. |
| `ClinicId` | `Guid?` | Tidak | Config owner | Clinic | Tidak | Klinik bila relevan. |
| `ConsultationStatus` | Enum/int | Ya | Config owner | — | Tidak | `Draft`, `InProgress`, `Completed`, `Cancelled`. |
| `Subjective`, `Objective`, `Assessment`, `Plan` | `string?` | Tidak | — | — | **Ya** | Isi SOAP. |
| `HasPrimaryDiagnosis` / `DiagnosisCount` | `bool` / `int` | Ya | — | — | Tidak | Ringkasan finalization existing. |
| `CompletedAt` / `CompletedByUserId` | `DateTime?` / `Guid?` | Tidak | — | User | **Ya** | Completion owner; belum membuktikan reauth/hash RM. |

## `TrxPatientDiagnosis` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID diagnosis owner. |
| `EncounterId` / `ConsultationId` / `PatientId` / `DoctorId` | `Guid` | Ya | Config owner | Owner terkait | **Ya** | Konteks diagnosis. |
| `DiagnosisId` | `Guid?` | Tidak | Config owner | Master diagnosis | Tidak | Null untuk entri non-master. |
| `DiagnosisCode` / `DiagnosisName` | `string` | Ya | Config owner | — | **Ya** | Snapshot diagnosis. |
| `DiagnosisStatus` | Enum/int | Ya | Config owner | — | Tidak | `Active`, `Resolved`, `RuledOut`, `Cancelled`. |
| `IsPrimary` | `bool` | Ya | Config owner | — | Tidak | Penanda diagnosis utama. |
| `DiagnosisDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu diagnosis. |

## `TrxPatientProcedure` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID tindakan owner. |
| `EncounterId` / `ConsultationId` / `PatientId` / `DoctorId` | `Guid` | Ya | Config owner | Owner terkait | **Ya** | Konteks tindakan. |
| `ProcedureId` | `Guid` | Ya | Config owner | Master procedure | Tidak | Tindakan master. |
| `ProcedureStatus` | Enum/int | Ya | Config owner | — | Tidak | `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled`. |
| `ProcedureDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu tindakan. |
| `IsNeedApproval` / `IsApproved` / `IsExecuted` | `bool` | Ya | — | — | Tidak | Workflow tindakan existing. |
| `BillingItemId` / `IsBillingGenerated` | `Guid?` / `bool` | Tidak/Ya | Config owner | Billing | Tidak | Touchpoint finansial; bukan owner closure RM. |
| `ClinicalNote` / `ResultNote` | `string?` | Tidak | — | — | **Ya** | Isi klinis tindakan. |

## `TrxPatientAllergy` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID alergi owner. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Pasien pemilik alergi. |
| `EncounterId` / `ConsultationId` / `AssessmentId` | `Guid?` | Tidak | Config owner | Owner terkait | **Ya** | Provenance pelayanan. |
| `AllergenName` / `ReactionDescription` | `string` / `string?` | Ya/Tidak | — | — | **Ya** | Fakta keselamatan. |
| `AllergyStatus` | Enum/int | Ya | Config owner | — | Tidak | `Active`, `Inactive`, `Resolved`, `EnteredInError`, `Cancelled`. |
| `IsHighRisk` / `IsLifeThreatening` / `IsAlertEnabled` | `bool` | Ya | Config owner | — | **Ya** | Mengatur alert keselamatan existing. |
| `ReportedDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu pelaporan. |

## `TrxPatientVitalSign` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID observasi owner. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Pasien. |
| `EncounterId` / `QueueId` / `AssessmentId` / `ConsultationId` | `Guid?` | Tidak | Config owner | Owner terkait | **Ya** | Provenance observasi. |
| `VitalSignStatus` | Enum/int | Ya | Config owner | — | Tidak | `Draft`, `Recorded`, `Verified`, `Corrected`, `Cancelled`, `EnteredInError`. |
| `ObservationDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu observasi. |
| Kelompok tekanan darah, nadi, napas, suhu, saturasi, GCS | Beragam | Tidak | — | — | **Ya** | Nilai klinis; jangan log payload. |

## `TrxPatientIntegratedProgressNote` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID CPPT owner. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Pasien. |
| `EncounterId` / `QueueId` / `ConsultationId` / `AssessmentId` / `VitalSignId` | `Guid?` | Tidak | Config owner | Owner terkait | **Ya** | Provenance catatan. |
| `ProviderUserId` / `DoctorId` | `Guid?` | Tidak | Config owner | User/doctor | **Ya** | Pembuat/provider. |
| `NoteDateTime` | `DateTime` | Ya | Config owner | — | **Ya** | Waktu catatan. |
| `SubjectiveSummary`, `ObjectiveSummary`, `AssessmentSummary`, `PlanSummary`, `NoteText` | `string?` | Tidak | — | — | **Ya** | Isi CPPT. |
| `SourceModule` / `SourceReferenceId` | `string?` / `Guid?` | Tidak | Config owner | Source record | Tidak | Provenance existing. |
| `IsReadOnlyGenerated` | `bool` | Ya | — | — | Tidak | Hanya melindungi hasil generated; bukan finality universal. |

Model tidak memiliki kolom status khusus untuk draft/signed/corrected. Karena itu endpoint `PUT`
dan `DELETE` existing wajib diperlakukan sebagai konflik desain untuk catatan final.

## `TrxPatientClinicalDocument` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID dokumen owner. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Pasien. |
| `EncounterId` / `AssessmentId` / `ConsultationId` | `Guid?` | Tidak | Config owner | Owner terkait | **Ya** | Provenance pelayanan. |
| `PatientDiagnosisId` / `PatientProcedureId` | `Guid?` | Tidak | Config owner | Diagnosis/procedure | **Ya** | Kaitan klinis. |
| `DocumentType` / `DocumentSource` / `DocumentStatus` | Enum/int | Ya | Config owner | — | Tidak | Jenis, sumber, dan lifecycle existing. |
| `DocumentDateTime` / `ReceivedDateTime` / `UploadedDateTime` | `DateTime` / `DateTime?` | Ya/Tidak | Config owner | — | **Ya** | Waktu dokumen. |
| `FilePath` / `FileName` / `FileHash` | `string` / `string` / `string?` | Ya/Ya/Tidak | Config owner | Storage | **Ya** | Lokasi dan bukti isi file. |
| `IsConfidential` / `IsShareable` / `IsPartOfMedicalRecord` | `bool` | Ya | Config owner | — | **Ya** | Flag privacy existing; bukan klasifikasi sensitif approved. |
| `ReviewedByUserId` / `VerifiedByUserId` / `ApprovedByUserId` | `Guid?` | Tidak | Config owner | User | **Ya** | Pelaku workflow existing. |

## `TrxPatientConsent` — Sudah ada

Sumber: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientConsent.cs`.

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | ID consent owner. |
| `ConsentNumber` | `string` | Ya | Config owner | — | **Ya** | Nomor consent. |
| `PatientId` | `Guid` | Ya | Config owner | Patient | **Ya** | Pasien. |
| `EncounterId` / `AssessmentId` / `ConsultationId` / `PatientProcedureId` | `Guid?` | Tidak | Config owner | Owner terkait | **Ya** | Provenance consent. |
| `ConsentType` / `ConsentStatus` / `ConsentMethod` | Enum/int | Ya | Config owner | — | Tidak | Jenis, status, dan metode existing. |
| `SignerType` / `SignerName` / `SignerIdentityNumber` | Enum/string/string? | Ya | Config owner | — | **Ya** | Identitas penanda tangan consent. |
| `SignedAt` | `DateTime?` | Tidak | Config owner | — | **Ya** | Waktu signature existing. |
| `ConsentFileHash` | `string?` | Tidak | Config owner | Storage | **Ya** | Hash file bila tersedia. |
| `VerifiedByUserId` / `ApprovedByUserId` | `Guid?` | Tidak | Config owner | User | **Ya** | Pelaku workflow existing. |

Signature consent existing tidak otomatis membuktikan reauth pengguna klinis, profesi/peran,
makna signature, dan content hash catatan sebagaimana invariant RM.

## Configuration dan DDL

Configuration existing berada di `Repositories/Configurations/HealthService/`. Bentuk folder
`HealthService` tunggal adalah utang teknis existing dan tidak boleh ditiru oleh modul baru.

Tidak ada DDL dalam revision ini. EF Core Migration tidak berubah karena tidak ada tabel `Baru`
atau `Diperbarui`.
