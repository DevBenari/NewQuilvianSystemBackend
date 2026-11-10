# Rehab Medik — Kamus Data

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1, `draft` |
| Contract version | `last_changed_in: v1` |
| Backend SHA | `6bb3fba7` |

**Kolom warisan.** Seluruh tabel mewarisi `IdentityModel`: `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`,
`IsCancel`, `IsDelete`. Kolom-kolom ini tidak diulang di bawah.

**Aturan umum.** Semua tabel memakai schema `public` dengan nama tunggal PascalCase. PK `Id`
bertipe `uuid` dengan bawaan `Guid.NewGuid()`. FK ke tabel modul lain memakai
`DeleteBehavior.Restrict`. FK anak ke induk di dalam rehab juga memakai `Restrict`, karena
penghapusan hanya berupa penanda `IsDelete`.

**Concurrency.** Selain kolom `Version` yang tertulis eksplisit, tabel dokumen klinis yang
dapat disunting — `RhbAssessment`, `RhbGoal`, `RhbEducationRecord`, `RhbProgressReview`,
`RhbDischargeSummary` — juga memiliki `Version int NOT NULL DEFAULT 0` sebagai concurrency token
(pola `HmdEpisodeConfiguration`). Tabel anak dan konfigurasi tidak memilikinya.

**Penanda Sensitif.** Kolom bertanda **S** berisi data klinis atau pribadi. Kolom ini tidak
boleh masuk custom logger, tidak boleh dipakai sebagai contoh berisi data asli, dan tidak boleh
ditampilkan di daftar/worklist. Bagi petugas, kolom S hanya muncul di layar detail.

---

## 1. Status dan kepemilikan tabel

| Tabel | Status | Pemilik |
|---|---|---|
| `RhbRequest`, `RhbRequestDiscipline`, `RhbEpisode`, `RhbEpisodeDiscipline`, `RhbAssessment`, `RhbMeasurement`, `RhbGoal`, `RhbProgram`, `RhbProgramIntervention`, `RhbProgramGoal`, `RhbEducationRecord`, `RhbSession`, `RhbSessionIntervention`, `RhbProgressReview`, `RhbGoalReview`, `RhbDischargeSummary`, `RhbStatusHistory`, `RhbDiscipline`, `RhbDisciplineProcedure`, `RhbDisciplineInstrument` | Baru | Rehab (`Rhb`) |
| `MstPatient` | Sudah ada | PatientManagement |
| `RegPatientEncounter` | Sudah ada | Registration |
| `InpEpisode` | Sudah ada | InPatient |
| `MstDoctor`, `MstProfession` | Sudah ada | HR |
| `MstProcedure` | Sudah ada | MasterData |
| `TrxPatientProcedure` | Sudah ada (legacy) | ClinicalManagement |
| `CliClinicalInstrumentVersion` | Sudah ada | ClinicalManagement |
| `MrcClinicalDocumentIntegrity` | Sudah ada | MedicalRecord |

---

## 2. Tabel baru

Singkatan kolom: **W** = wajib, **Bawaan** = nilai bawaan, **Idx** = index/unik/relasi.

### 2.1 `RhbRequest` — permintaan rehab

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `RequestNumber` | `varchar(30)` | ✓ | dari deret `RHB_REQUEST` | dibuat server | **unik** | |
| `PatientId` | `uuid` | ✓ | | pasien aktif | FK `MstPatient`, idx | |
| `SourceType` | `int` (`RehabRequestSource`) | ✓ | | enum sah | idx | |
| `SourceEncounterId` | `uuid` | bila `SourceType` ≠ `ExternalReferral` | null | milik pasien yang sama | FK `RegPatientEncounter` | |
| `InpEpisodeId` | `uuid` | bila `SourceType = Inpatient` | null | episode milik encounter | FK `InpEpisode` | |
| `InternalReferralLetterId` | `uuid` | | null | surat milik pasien | rujukan logis ke `CliDoctorCertificate` | |
| `ExternalInstitutionName` | `varchar(200)` | bila `ExternalReferral` | null | | | ✓ |
| `ExternalReferralNumber` | `varchar(100)` | | null | | | ✓ |
| `RequestingDoctorId` | `uuid` | bila sumber internal | null | dokter aktif | FK `MstDoctor` | |
| `RequestingPractitionerName` | `varchar(150)` | bila `ExternalReferral` | null | | | ✓ |
| `IndicationText` | `varchar(2000)` | ✓ | | ≥ 5 karakter | | ✓ |
| `RequestedAt` | `timestamptz` | ✓ | | ≤ waktu server | idx | |
| `RequestStatus` | `int` (`RehabRequestStatus`) | ✓ | `Draft` | lihat state matrix | idx | |
| `DuplicateOverrideReason` | `varchar(500)` | bila ada duplikasi | null | | | ✓ |
| `ReturnReason` | `varchar(500)` | saat `return` / `request-clarification` | null | | | ✓ |
| `DispositionReason` | `varchar(1000)` | saat `redirect`/`reject` | null | | | ✓ |
| `RedirectTargetText` | `varchar(200)` | saat `redirect` | null | | | |
| `DispositionByUserId` / `DispositionAt` | `uuid` / `timestamptz` | | null | dari server | | |
| `SubmittedByUserId` / `SubmittedAt` | `uuid` / `timestamptz` | | null | dari server | | |
| `CancelReason` | `varchar(500)` | saat `cancel` | null | | | |
| `Version` | `int` | ✓ | 0 | concurrency token | | |

Check constraint: `SourceType = ExternalReferral` ⇔ `SourceEncounterId IS NULL`.

### 2.2 `RhbRequestDiscipline`

| Kolom | Tipe | W | Idx/relasi |
|---|---|:---:|---|
| `RequestId` | `uuid` | ✓ | FK `RhbRequest` |
| `DisciplineId` | `uuid` | ✓ | FK `RhbDiscipline`; **unik** `(RequestId, DisciplineId)` |

### 2.3 `RhbEpisode`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeNumber` | `varchar(30)` | ✓ | deret `RHB_EPISODE` | | **unik** | |
| `RequestId` | `uuid` | ✓ | | | FK `RhbRequest`, **unik** | |
| `PatientId` | `uuid` | ✓ | salinan dari permintaan | | FK `MstPatient`, idx | |
| `ResponsibleDoctorId` | `uuid` | ✓ | dipilih saat `accept` | dokter aktif | FK `MstDoctor`, idx | |
| `EpisodeStatus` | `int` (`RehabEpisodeStatus`) | ✓ | `AwaitingMedicalAssessment` | | idx | |
| `OpenedAt` | `timestamptz` | ✓ | waktu `accept` | | | |
| `ClosureReason` | `int?` (`RehabEpisodeClosureReason`) | bila `Closed` | null | | | |
| `ClosedAt` / `ClosedByUserId` | `timestamptz` / `uuid` | bila `Closed` | null | | | |
| `Version` | `int` | ✓ | 0 | concurrency | | |

### 2.4 `RhbEpisodeDiscipline`

| Kolom | Tipe | W | Idx/relasi |
|---|---|:---:|---|
| `EpisodeId` | `uuid` | ✓ | FK `RhbEpisode` |
| `DisciplineId` | `uuid` | ✓ | FK `RhbDiscipline`; **unik** `(EpisodeId, DisciplineId)` |
| `PrescribedByAssessmentId` | `uuid` | ✓ | FK `RhbAssessment` (asesmen medis) |
| `MedicalPrecautionText` | `varchar(1000)` | | — **S** |

### 2.5 `RhbAssessment`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK `RhbEpisode`, idx | |
| `AssessmentType` | `int` (`RehabAssessmentType`) | ✓ | | | | |
| `DisciplineId` | `uuid` | bila `DisciplineInitial` | null | sudah diresepkan di episode | FK `RhbDiscipline` | |
| `EncounterId` | `uuid` | ✓ | | terbuka, milik pasien | FK `RegPatientEncounter` | |
| `InpEpisodeId` | `uuid` | | null | | FK `InpEpisode` | |
| `AuthorUserId` | `uuid` | ✓ | pengguna masuk | tidak pernah berubah | | |
| `AssessedAt` | `timestamptz` | ✓ | | ≤ waktu server | | |
| `HistoryText` | `varchar(4000)` | ✓ saat tanda tangan | | | | ✓ |
| `PhysicalFunctionalFindingsText` | `varchar(4000)` | ✓ saat tanda tangan | | | | ✓ |
| `RiskAndContraindicationText` | `varchar(2000)` | | | | | ✓ |
| `ProblemOrDiagnosisText` | `varchar(2000)` | ✓ saat tanda tangan | | diagnosis medis atau diagnosis profesi | | ✓ |
| `PrognosisText` | `varchar(1000)` | | | | | ✓ |
| `ConclusionText` | `varchar(2000)` | ✓ saat tanda tangan | | | | ✓ |
| `PatientGoalText` | `varchar(1000)` | | | harapan pasien | | ✓ |
| `RecordStatus` | `int` (`RehabRecordStatus`) | ✓ | `Draft` | sejalan dengan status keutuhan RM | idx | |
| `SignedAt` | `timestamptz` | | null | dari `SignAsync` | | |

Unik parsial: satu `AssessmentType = MedicalInitial` dengan `RecordStatus = Final` per `EpisodeId`.

### 2.6 `RhbMeasurement`

| Kolom | Tipe | W | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|:---:|
| `AssessmentId` | `uuid` | salah satu | | FK `RhbAssessment` | |
| `ProgressReviewId` | `uuid` | salah satu | | FK `RhbProgressReview` | |
| `Source` | `int` (`RehabMeasurementSource`) | ✓ | | | |
| `InstrumentVersionId` | `uuid` | bila `Instrument` | versi `Approved` (atau konsep bila lingkungan mengizinkan) | FK `CliClinicalInstrumentVersion` | |
| `InstrumentKind` | `int` | bila `Instrument` | | | |
| `InstrumentName` | `varchar(150)` | ✓ | | | |
| `BodyPartOrDomain` | `varchar(150)` | | contoh "Bahu kanan — fleksi" | | ✓ |
| `AnswersJson` | `jsonb` | bila `Instrument` | divalidasi mesin instrumen | | ✓ |
| `NumericValue` | `numeric(10,2)` | | | | ✓ |
| `ValueText` | `varchar(200)` | | | | ✓ |
| `Unit` | `varchar(30)` | | contoh "derajat", "detik", "skor" | | |
| `ScoreBandCode` | `varchar(50)` | | dari mesin instrumen | | ✓ |
| `MeasuredAt` | `timestamptz` | ✓ | | | |

Check constraint: tepat satu dari `AssessmentId` / `ProgressReviewId` terisi.

### 2.7 `RhbGoal`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK `RhbEpisode`, idx | |
| `DisciplineId` | `uuid` | | null | null = tujuan medis/bersama | FK `RhbDiscipline` | |
| `Term` | `int` (`RehabGoalTerm`) | ✓ | | | | |
| `DescriptionText` | `varchar(1000)` | ✓ | | | | ✓ |
| `BaselineText` | `varchar(500)` | ✓ | | | | ✓ |
| `TargetText` | `varchar(500)` | ✓ | | | | ✓ |
| `TargetDate` | `date` | ✓ | | ≥ tanggal dibuat | | |
| `GoalStatus` | `int` (`RehabGoalStatus`) | ✓ | `Active` | | idx | |
| `SupersedesGoalId` | `uuid` | | null | | FK `RhbGoal` | |
| `AuthorUserId` | `uuid` | ✓ | | | | |
| `AgreedWithPatient` | `bool` | ✓ | false | FEAT-012: ditinjau bersama pasien | | |

### 2.8 `RhbProgram`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK, idx | |
| `DisciplineId` | `uuid` | ✓ | | diresepkan di episode | FK `RhbDiscipline` | |
| `ProgramVersion` | `int` | ✓ | 1 | versi baru = versi tertinggi disiplin itu + 1, dihitung di bawah advisory lock (bukan nomor bisnis) | | |
| `SupersedesProgramId` | `uuid` | | null | | FK `RhbProgram` | |
| `AuthoredEncounterId` | `uuid` | ✓ | | encounter tempat program disusun | FK `RegPatientEncounter` | |
| `FrequencyText` | `varchar(200)` | ✓ saat aktivasi | | isian bebas, misalnya "2× seminggu, 4 minggu" | | ✓ |
| `PlannedSessionCount` | `int` | | null | informatif, **bukan** batas sistem (RHB-INV-10) | | |
| `PrecautionText` | `varchar(1000)` | | | | | ✓ |
| `StartDate` | `date` | ✓ saat aktivasi | | | | |
| `EvaluationDueDate` | `date` | ✓ saat aktivasi | | ≥ `StartDate` | idx | |
| `ProgramStatus` | `int` (`RehabProgramStatus`) | ✓ | `Draft` | | idx | |
| `NeedsReview` | `bool` | ✓ | false | diset oleh deviasi sesi atau keputusan `Modify` | | |
| `ModificationReason` | `varchar(500)` | bila versi > 1 | | | | ✓ |
| `AuthorUserId` | `uuid` | ✓ | | | | |
| `ActivatedAt` / `StoppedAt` | `timestamptz` | | null | | | |
| `StopReason` | `varchar(500)` | bila `Stopped` | | | | ✓ |
| `Version` | `int` | ✓ | 0 | concurrency | | |

Unik parsial: satu `ProgramStatus = Active` per `(EpisodeId, DisciplineId)`.

### 2.9 `RhbProgramIntervention`

| Kolom | Tipe | W | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|:---:|
| `ProgramId` | `uuid` | ✓ | | FK `RhbProgram` | |
| `ProcedureId` | `uuid` | ✓ | ada di `RhbDisciplineProcedure` aktif disiplin itu | FK `MstProcedure` | |
| `ParametersText` | `varchar(1000)` | ✓ | dosis/durasi/intensitas yang direncanakan | | ✓ |

### 2.10 `RhbProgramGoal`

| Kolom | Tipe | W | Idx/relasi |
|---|---|:---:|---|
| `ProgramId` | `uuid` | ✓ | FK |
| `GoalId` | `uuid` | ✓ | FK; **unik** `(ProgramId, GoalId)` |

### 2.11 `RhbEducationRecord`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK | |
| `ProgramId` | `uuid` | | null | program episode yang sama | FK | |
| `EncounterId` | `uuid` | ✓ | | | FK `RegPatientEncounter` | |
| `MaterialText` | `varchar(2000)` | ✓ | | tujuan, manfaat, risiko, alternatif, peran pasien | | ✓ |
| `RecipientName` | `varchar(150)` | ✓ | | | | ✓ |
| `RecipientRelation` | `varchar(50)` | ✓ | | "Pasien", "Anak", dan seterusnya | | |
| `MethodText` | `varchar(200)` | ✓ | | lisan, demonstrasi, leaflet | | |
| `UnderstandingLevel` | `int` (`RehabUnderstandingLevel`) | ✓ | | `NotAssessed` tidak boleh saat tanda tangan | | |
| `BarrierText` | `varchar(500)` | | | hambatan bahasa/kognitif/sensorik | | ✓ |
| `ConsentStatus` | `int` (`RehabConsentStatus`) | ✓ | | | idx | |
| `ConsentNote` | `varchar(500)` | bila `Refused`/`PartiallyAgreed` | | | | ✓ |
| `EducatorUserId` | `uuid` | ✓ | | | | |
| `EducatedAt` | `timestamptz` | ✓ | | | | |
| `RecordStatus` | `int` | ✓ | `Draft` | | | |

### 2.12 `RhbSession`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `SessionNumber` | `varchar(30)` | ✓ | deret `RHB_SESSION` | | **unik** | |
| `EpisodeId` | `uuid` | ✓ | | | FK, idx | |
| `ProgramId` | `uuid` | ✓ | | program `Active` saat sesi dibuat | FK, idx | |
| `EncounterId` | `uuid` | ✓ | | terbuka, milik pasien (pola HD) | FK `RegPatientEncounter`, idx | |
| `InpEpisodeId` | `uuid` | | null | belum pulang secara fisik | FK `InpEpisode` | |
| `TherapistUserId` | `uuid` | ✓ | pengguna yang memulai | profesi = disiplin program | idx | |
| `SessionStatus` | `int` (`RehabSessionStatus`) | ✓ | | | idx | |
| `NotDoneReason` | `int?` (`RehabNotDoneReason`) | bila `NotDone` | null | | | |
| `IdentityVerified` | `bool` | ✓ | false | harus true untuk `InProgress` | | |
| `ProgramVerified` | `bool` | ✓ | false | harus true untuk `InProgress` | | |
| `CurrentConditionText` | `varchar(1000)` | ✓ untuk `InProgress` | | | | ✓ |
| `ContraindicationFound` | `bool` | ✓ | false | | | |
| `ContraindicationText` | `varchar(1000)` | bila `ContraindicationFound` | | dicatat, tidak ditimpa | | ✓ |
| `StartedAt` / `EndedAt` | `timestamptz` | `StartedAt` untuk `InProgress` | null | `EndedAt ≥ StartedAt` | idx `StartedAt` | |
| `ResponseText` | `varchar(2000)` | ✓ saat tanda tangan bila ada intervensi | | toleransi/respons pasien | | ✓ |
| `ImmediateOutcomeText` | `varchar(1000)` | | | | | ✓ |
| `DeviationType` | `int` (`RehabDeviationType`) | ✓ | `None` | | | |
| `DeviationReasonText` | `varchar(1000)` | bila ≠ `None` | | | | ✓ |
| `ImmediateActionText` | `varchar(1000)` | bila `Escalated`/`StoppedEarly` | | | | ✓ |
| `ContactedPartyText` | `varchar(200)` | bila `Escalated` | | | | ✓ |
| `RecordStatus` | `int` | ✓ | `Draft` | | idx | |
| `SignedAt` | `timestamptz` | | null | | | |
| `BillingHandoffStatus` | `int` (`RehabBillingHandoffStatus`) | ✓ | `NotRequired` | | idx | |
| `BillingHandoffAt` | `timestamptz` | | null | | | |
| `BillingHandoffError` | `varchar(500)` | | null | tanpa data klinis | | |
| `Version` | `int` | ✓ | 0 | concurrency | | |

### 2.13 `RhbSessionIntervention`

| Kolom | Tipe | W | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|:---:|
| `SessionId` | `uuid` | ✓ | | FK | |
| `ProgramInterventionId` | `uuid` | | null bila intervensi di luar rencana (wajib `DeviationType = Modified`) | FK | |
| `ProcedureId` | `uuid` | ✓ | ada di `RhbDisciplineProcedure` aktif | FK `MstProcedure` | |
| `ActualParametersText` | `varchar(1000)` | ✓ | tindakan **aktual**, bukan salinan rencana | | ✓ |
| `DurationMinutes` | `int` | | 1–600 | | |
| `Quantity` | `int` | ✓ | 1–99, bawaan 1 | | |
| `PatientProcedureId` | `uuid` | | diisi saat tanda tangan | FK `TrxPatientProcedure`, **unik** bila terisi | |

### 2.14 `RhbProgressReview`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK, idx | |
| `DisciplineId` | `uuid` | | null | null = reasesmen medis Sp.KFR | FK | |
| `ProgramId` | `uuid` | bila `DisciplineId` terisi | | program aktif disiplin itu | FK | |
| `EncounterId` | `uuid` | ✓ | | | FK | |
| `AuthorUserId` | `uuid` | ✓ | | | | |
| `ReviewedAt` | `timestamptz` | ✓ | | | | |
| `ProgressSummaryText` | `varchar(4000)` | ✓ saat tanda tangan | | | | ✓ |
| `BarrierText` | `varchar(1000)` | | | | | ✓ |
| `InstrumentChangeNote` | `varchar(500)` | bila instrumen berbeda dari baseline | | FEAT-024 | | |
| `Decision` | `int` (`RehabProgressDecision`) | ✓ saat tanda tangan | | | | |
| `DecisionReasonText` | `varchar(1000)` | ✓ saat tanda tangan | | | | ✓ |
| `ReferralTargetText` | `varchar(200)` | bila `Refer` | | | | |
| `RecordStatus` | `int` | ✓ | `Draft` | | | |

### 2.15 `RhbGoalReview`

| Kolom | Tipe | W | Idx/relasi | S |
|---|---|:---:|---|:---:|
| `ProgressReviewId` | `uuid` | ✓ | FK | |
| `GoalId` | `uuid` | ✓ | FK; **unik** `(ProgressReviewId, GoalId)` | |
| `AchievementStatus` | `int` (`RehabGoalStatus`) | ✓ | | |
| `NoteText` | `varchar(500)` | | | ✓ |

### 2.16 `RhbDischargeSummary`

| Kolom | Tipe | W | Bawaan | Batas/validasi | Idx/relasi | S |
|---|---|:---:|---|---|---|:---:|
| `EpisodeId` | `uuid` | ✓ | | | FK, **unik** | |
| `EncounterId` | `uuid` | ✓ | | | FK | |
| `ClosureReason` | `int` (`RehabEpisodeClosureReason`) | ✓ | | | | |
| `MainServicesText` | `varchar(2000)` | ✓ | | | | ✓ |
| `GoalAchievementText` | `varchar(2000)` | ✓ | | | | ✓ |
| `FinalConditionText` | `varchar(2000)` | ✓ | | | | ✓ |
| `FollowUpType` | `int` (`RehabFollowUpType`) | ✓ | | | | |
| `FollowUpInstructionText` | `varchar(2000)` | bila ≠ `None` | | | | ✓ |
| `FollowUpTargetText` | `varchar(200)` | bila `ReferBack`/`ReferOther` | | | | |
| `FollowUpDueDate` | `date` | | | | | |
| `AuthorUserId` | `uuid` | ✓ | | | | |
| `RecordStatus` | `int` | ✓ | `Draft` | | | |

### 2.17 `RhbStatusHistory`

| Kolom | Tipe | W | Idx | S |
|---|---|:---:|---|:---:|
| `AggregateType` | `varchar(40)` | ✓ | idx `(AggregateType, AggregateId, OccurredAt)` | |
| `AggregateId` | `uuid` | ✓ | | |
| `FromStatus` | `varchar(40)` | | | |
| `ToStatus` | `varchar(40)` | ✓ | | |
| `Action` | `varchar(40)` | ✓ | | |
| `ReasonText` | `varchar(1000)` | | | ✓ |
| `ActorUserId` | `uuid` | ✓ | | |
| `OccurredAt` | `timestamptz` | ✓ | | |

### 2.18 `RhbDiscipline`

| Kolom | Tipe | W | Bawaan | Validasi | Idx |
|---|---|:---:|---|---|---|
| `DisciplineCode` | `varchar(40)` | ✓ | | huruf besar + `_` | **unik** |
| `DisciplineName` | `varchar(100)` | ✓ | | | |
| `ProfessionId` | `uuid` | ✓ saat diaktifkan | null | `MstProfession.IsClinicalProfession` | FK `MstProfession` |
| `IsActive` | `bool` | ✓ | false | | |

### 2.19 `RhbDisciplineProcedure`

| Kolom | Tipe | W | Bawaan | Idx |
|---|---|:---:|---|---|
| `DisciplineId` | `uuid` | ✓ | | FK |
| `ProcedureId` | `uuid` | ✓ | | FK `MstProcedure`; **unik** `(DisciplineId, ProcedureId)` |
| `IsActive` | `bool` | ✓ | true | |

### 2.20 `RhbDisciplineInstrument`

| Kolom | Tipe | W | Bawaan | Idx |
|---|---|:---:|---|---|
| `DisciplineId` | `uuid` | | null = asesmen medis | FK |
| `InstrumentKind` | `int` (`ClinicalInstrumentKind`) | ✓ | | **unik** `(DisciplineId, InstrumentKind)` (null diperlakukan sebagai satu nilai, `NULLS NOT DISTINCT`) |
| `IsActive` | `bool` | ✓ | true | |

---

## 3. Enum

| Enum | Nilai | Bawaan |
|---|---|---|
| `RehabRequestSource` | `Outpatient=1`, `Inpatient=2`, `Emergency=3`, `ExternalReferral=4` | — |
| `RehabRequestStatus` | `Draft=1`, `Submitted=2`, `NeedsClarification=3`, `Accepted=4`, `Redirected=5`, `Rejected=6`, `Cancelled=7` | `Draft` |
| `RehabEpisodeStatus` | `AwaitingMedicalAssessment=1`, `Planning=2`, `Active=3`, `Closed=4` | `AwaitingMedicalAssessment` |
| `RehabEpisodeClosureReason` | `ClinicalCompletion=1`, `Discontinued=2`, `Referred=3`, `Transferred=4`, `LostToFollowUp=5`, `Deceased=6`, `PatientRefused=7` | — |
| `RehabAssessmentType` | `MedicalInitial=1`, `DisciplineInitial=2` | — |
| `RehabRecordStatus` | `Draft=1`, `Final=2`, `Cancelled=3` | `Draft` |
| `RehabMeasurementSource` | `Instrument=1`, `Structured=2` | — |
| `RehabGoalTerm` | `ShortTerm=1`, `LongTerm=2` | — |
| `RehabGoalStatus` | `Active=1`, `Achieved=2`, `PartiallyAchieved=3`, `NotAchieved=4`, `Discontinued=5`, `Superseded=6` | `Active` |
| `RehabProgramStatus` | `Draft=1`, `Active=2`, `Superseded=3`, `Stopped=4`, `Completed=5`, `Cancelled=6` | `Draft` |
| `RehabConsentStatus` | `Agreed=1`, `PartiallyAgreed=2`, `Refused=3`, `NotRequired=4` | — |
| `RehabUnderstandingLevel` | `Understood=1`, `PartiallyUnderstood=2`, `NotUnderstood=3`, `NotAssessed=4` | — |
| `RehabSessionStatus` | `InProgress=1`, `Completed=2`, `StoppedEarly=3`, `NotDone=4` | — |
| `RehabNotDoneReason` | `NoShow=1`, `CancelledByPatient=2`, `Postponed=3`, `PatientRefused=4`, `ClinicalCondition=5`, `ResourceUnavailable=6` | — |
| `RehabDeviationType` | `None=0`, `Modified=1`, `StoppedEarly=2`, `Escalated=3` | `None` |
| `RehabBillingHandoffStatus` | `NotRequired=0`, `Pending=1`, `Succeeded=2`, `Failed=3` | `NotRequired` |
| `RehabProgressDecision` | `Continue=1`, `Modify=2`, `Stop=3`, `Refer=4`, `Complete=5` | — |
| `RehabFollowUpType` | `None=0`, `Control=1`, `HomeProgram=2`, `ReferBack=3`, `ReferOther=4` | — |

---

## 4. Tabel yang sudah ada — kolom kunci yang dipakai

| Tabel | Kolom | Dipakai untuk | File model |
|---|---|---|---|
| `RegPatientEncounter` | `Id`, `PatientId`, `EncounterType`, `EncounterStatus`, `ClinicId` | Validasi encounter terbuka milik pasien | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` |
| `InpEpisode` | `EncounterId`, `PhysicallyLeftAt` | Menolak sesi untuk pasien yang sudah pulang | `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` |
| `MstProcedure` | `Id`, `ProcedureCode`, `ProcedureName`, `IsActive` | Intervensi dan tagihan | MasterData |
| `MstProfession` | `Id`, `IsClinicalProfession` | Kewenangan disiplin | `Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstProfession.cs` |
| `TrxPatientProcedure` | `Id`, `EncounterId`, `DoctorId`, `InstructingDoctorId`, `ProcedureId`, `IdempotencyKey`, `ProcedureSource`, `IsBillable` | Sumber charge (pola HD) | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` |
| `MrcClinicalDocumentIntegrity` | `DocumentKind`, `DocumentId`, `IntegrityStatus`, `AuthorUserId` | Kunci dan tanda tangan | `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs` |
| `CliClinicalInstrumentVersion` | `Id`, `VersionStatus`, `DefinitionHash` | Pengukuran berinstrumen | `Areas/HealthServices/ClinicalManagement/Models/CliClinicalInstrumentVersion.cs` |

---

## 5. Skema DDL (dokumentasi bentuk)

> **Peringatan.** Bagian ini dokumentasi bentuk tabel, **bukan** skrip yang dijalankan. Skema
> sebenarnya dihasilkan EF Core dari configuration di
> `Repositories/Configurations/HealthServices/RehabilitationManagement/`. Kolom `IdentityModel`
> tidak ditulis.

```sql
CREATE TABLE public."RhbRequest" (
  "Id" uuid PRIMARY KEY,
  "RequestNumber" varchar(30) NOT NULL UNIQUE,
  "PatientId" uuid NOT NULL REFERENCES public."MstPatient"("Id") ON DELETE RESTRICT,
  "SourceType" int NOT NULL,
  "SourceEncounterId" uuid NULL REFERENCES public."RegPatientEncounter"("Id") ON DELETE RESTRICT,
  "InpEpisodeId" uuid NULL REFERENCES public."InpEpisode"("Id") ON DELETE RESTRICT,
  "IndicationText" varchar(2000) NOT NULL,
  "RequestStatus" int NOT NULL DEFAULT 1,
  "Version" int NOT NULL DEFAULT 0,
  CONSTRAINT "CK_RhbRequest_Source" CHECK (("SourceType" = 4) = ("SourceEncounterId" IS NULL))
  -- kolom lain sesuai §2.1
);

CREATE UNIQUE INDEX "UX_RhbProgram_ActivePerDiscipline"
  ON public."RhbProgram" ("EpisodeId", "DisciplineId") WHERE "ProgramStatus" = 2 AND NOT "IsDelete";

CREATE UNIQUE INDEX "UX_RhbAssessment_MedicalFinal"
  ON public."RhbAssessment" ("EpisodeId") WHERE "AssessmentType" = 1 AND "RecordStatus" = 2 AND NOT "IsDelete";

ALTER TABLE public."RhbMeasurement" ADD CONSTRAINT "CK_RhbMeasurement_Owner"
  CHECK (("AssessmentId" IS NULL) <> ("ProgressReviewId" IS NULL));
```
