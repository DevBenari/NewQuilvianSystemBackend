# Remediasi Otorisasi Modul Human Resource — 74 Controller, 440 Endpoint

| Field | Nilai |
|---|---|
| Jenis dokumen | **Evidence remediasi keamanan** — bukan laporan task roadmap |
| Sebab | Audit roadmap backend HR (10 November 2026) menemukan endpoint HR yang bisa dipanggil tanpa login, endpoint yang hanya mewajibkan login, dan endpoint yang selalu menolak non-SuperAdmin |
| Permintaan pemilik | "mulai dari nomor satu anda kerjakan semua" — kelompok temuan 1 "Keamanan" |
| Commit yang memuat perubahan | `edfd843bac1c2860da7ae9a5ddcb68a75cf42666` pada branch `MHamzah` (lihat bagian 9) |
| Induk commit | `a1d4aa9f5a86e32d0fb7cbc062304c4aa463b2b6` |
| Tanggal | 10 November 2026 |
| Hasil | Kode selesai dan tervalidasi. Hak akses **belum diberikan** ke peran mana pun — menunggu admin lewat layar Akses Role |
| Task ID roadmap | **Belum ada.** Remediasi ini belum punya kartu task; usulan penomoran ada di bagian 10 |

---

## 1. Masalahnya, dengan bahasa sehari-hari

Aplikasi Quilvian menjaga setiap fitur dengan tiga lapis:

1. **Login** (`[Authorize]`) — pengguna harus masuk lebih dulu.
2. **Hak akses per fitur** (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`) — admin
   menentukan lewat layar **Pengaturan → Manajemen Role → Akses Role** jabatan mana yang boleh
   memakai fitur itu.
3. **Kepemilikan data** — diperiksa di dalam service, misalnya pegawai hanya boleh melihat
   pengajuannya sendiri.

Audit menemukan tiga kelompok kerusakan pada lapis 1 dan 2:

| Kelompok | Jumlah | Akibatnya di rumah sakit |
|---|---:|---|
| Tanpa login sama sekali | 5 controller, 28 endpoint | Siapa pun yang tahu alamat API — bahkan dari luar — bisa membuka kotak masuk whistleblower dan mengesahkan pencairan pesangon. Pengesahan pesangon tanpa login bahkan tercatat atas nama `cfo@rs-mmc.co.id` |
| Hanya mewajibkan login | 57 controller, 356 endpoint | Petugas mana pun yang punya akun — misalnya petugas loket pendaftaran — bisa membuka rekam medis K3 staf, menyetujui final klaim restitusi gaji, atau mencairkan uang muka perjalanan dinas |
| Selalu menolak non-SuperAdmin | 12 controller, 56 endpoint | Rekrutmen, template onboarding, dan pengaturan lifecycle memakai `[AccessPermission]` tetapi tidak pernah terdaftar di layar Akses Role. Akibatnya hanya SuperAdmin yang bisa memakainya, dan admin tidak punya cara memberikannya ke petugas rekrutmen |

Selain itu 20 controller mencatat aktor **karangan** ketika identitas pengguna kosong — GUID
`11111111-1111-1111-1111-111111111111`, `"Diklat_Admin"`, `"HR Analyst"`, `"Pegawai ESS"`,
`"system@rs-mmc.co.id"`. Jejak audit seperti itu menyesatkan: perubahan terlihat dilakukan orang
yang tidak pernah melakukannya.

---

## 2. Apa yang diubah

| Fase | Isi | Controller | Endpoint |
|---|---|---:|---:|
| 1 | Hubungan Industrial (S-C5): tambah `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, dan buang aktor cadangan | 5 | 28 |
| 2 | Controller yang hanya mewajibkan login: tambah `[AccessController]` beserta `[AccessAction]` + `[AccessPermission]` per endpoint | 57 | 356 |
| 3 | Controller yang selalu menolak: tambah `[AccessController]` + `[AccessAction]`, dan selaraskan nama resource dengan kontrak §12 dan §13 | 12 | 56 |
| 4 | Ganti aktor karangan dengan penolakan `UnauthorizedAccessException("Identitas user pada token tidak valid.")` — pola yang sama dengan `HumanResourceContextService.GetCurrentUserId` | 20 (5 di antaranya juga fase 1) | — |
| **Total** | | **74** | **440** |

Yang **tidak** diubah: logika bisnis, route, DTO, bentuk response, entity, database, dan migration.
Perubahan hanya berupa atribut pada controller dan helper aktor.

### 2.1 Aturan penamaan yang dipakai

| Aturan | Alasan |
|---|---|
| Argumen ke-1 `[AccessPermission]` sama persis dengan `ControllerName` pada `[AccessController]` | Wajib menurut `role-access-rules.md` §3. Bila beda, hasilnya 403 permanen yang tidak bisa diperbaiki dari layar Akses Role |
| Argumen ke-2 `[AccessPermission]` sama persis dengan argumen ke-1 `[AccessAction]` | Sama seperti di atas |
| Nama aksi diambil dari kata kerja kontrak (`IssueSanction`, `ApproveSeveranceFinance`, `Disburse`) | Supaya setiap kewenangan kontrak bisa dicentang terpisah. Contoh: Direktur Keuangan diberi `EmployeeSeparation : ApproveSeveranceFinance` tanpa ikut bisa `CalculateSeverance` |
| Lihat data umum memakai `Read` | Konvensi aksi standar kontrak §1.3 |
| `AccessType`: membuat record baru = `Create`, langkah alur pada record yang ada = `Update`, membaca = `Read`, menghapus = `Delete` | Layar Akses Role hanya menampilkan empat kolom ini |
| Nama resource tidak boleh dipakai modul lain | `PerformanceCycle` dan `BenefitPlan` sudah dipakai master data, sehingga dipakai `PerformanceCycleGoal` dan `BenefitPlanEligibility` |

### 2.2 Modul registry baru

Tiga kode modul belum ada sebelumnya dan lahir otomatis dari atribut saat startup:

| Kode modul | Nama |
|---|---|
| `HUMAN_RESOURCE_SERVICE_MANAGEMENT` | Human Resource Service Management |
| `HUMAN_RESOURCE_BUSINESS_TRAVEL` | Human Resource Business Travel |
| `HUMAN_RESOURCE_RECRUITMENT` | Human Resource Recruitment |

Ini hanya baris registry `SysApplicationModule`, bukan entity atau prefix baru, sehingga
`QBE-MOD-002` tidak berlaku.

---

## 3. Dampak operasional setelah deploy — **wajib dibaca admin**

**Alurnya:**

1. **Pemicu** — aplikasi versi ini dijalankan.
2. **Otomatis** — `AccessMenuSeeder` mendaftarkan 74 resource baru ke `SysControllerAccess` dan
   aksinya ke `SysActionAccess`. Seeder **tidak pernah** membuat `SysAccessPolicy`.
3. **Akibat langsung** — semua pengguna non-SuperAdmin mendapat **403 "Anda tidak memiliki akses
   ke menu atau fitur ini."** pada 440 endpoint tersebut, sampai admin memberikannya.
4. **Pelaku** — admin sistem membuka **Pengaturan → Manajemen Role → Akses Role**, memilih
   departemen dan jabatan, lalu mencentang aksi yang sesuai.
5. **Hasil akhir** — hanya jabatan yang dicentang yang bisa memakai fitur itu.

**Contoh:** Kepala Ruangan IGD yang kemarin bisa membuka antrean klaim benefit akan mendapat 403
setelah deploy. Admin perlu mencentang `BenefitClaim : Read` untuk jabatan Kepala Ruangan. Bila
Kepala Ruangan juga perlu memverifikasi klaim, admin mencentang `BenefitClaim : Verify`.

**Aksi untuk seluruh pegawai.** Beberapa aksi memang ditujukan untuk semua staf, misalnya
`EmployeeGrievance : Submit` (mengajukan keluhan), `BenefitClaim : SubmitClaim`,
`HrServiceEss : CreateRequest`, `WorkflowInboxInstance : CreateInstance`, dan
`PerformanceAppraisal : SelfAssess`. Aksi seperti ini perlu dicentang untuk setiap jabatan pegawai.

**Sebelum deploy ke lingkungan yang dipakai petugas**, siapkan matriks pemberian hak akses per
jabatan memakai kolom "Peran yang Memiliki" pada `contracts/permission-audit-matrix.md` §12–§29,
seperti yang dilakukan `BE-SEC-014` dan `BE-SEC-016` untuk modul lain.

---

## 4. Endpoint publik yang sengaja dibiarkan tanpa login

| Endpoint | Controller | Alasan |
|---|---|---|
| `GET api/v1/corporate/hrd/services/documents/public-verify/{hash}` | `EmployeeDocumentIssuanceController.VerifyPublicDocument` | Verifikasi keabsahan surat resmi dari QR code oleh pihak luar (bank, instansi). Nominal gaji disamarkan |
| `POST api/v1/corporate/hrd/services/documents/unlock-financial` | `EmployeeDocumentIssuanceController.UnlockFinancialDetails` | Membuka nominal dengan PIN 4 digit; dibatasi rate limiter 3 kali salah per 30 menit (VR-HRS-12) |
| `GET verify/document/{hash}` dan dua alias | `HrPublicDocumentVerificationController` | Portal verifikasi publik yang sama (BE-HRD-204) |

`DoctorController` (master data) memakai policy `KioskRead` yang tercantum pada
`AuthorizationPolicies.ApprovedAlternativeAuthorization`, sehingga bukan celah.

---

## 5. Pemetaan per controller

`HR_` pada kolom Modul adalah singkatan `HUMAN_RESOURCE_`. Kolom Kontrak merujuk bagian
`contracts/permission-audit-matrix.md`.

| Fase | Controller | Resource (`ControllerName`) | Modul | Aksi (`AccessType`) | Kontrak |
|---|---|---|---|---|---|
| 1 | `EmployeeDisciplinaryController` | `EmployeeDisciplinary` | `HR_EMPLOYEE_RELATION` | `CreateIncident` (Create), `ViewIncidents` (Read), `ConductInvestigation` (Update), `IssueSanction` (Create), `ViewSanctions` (Read), `AppealSanction` (Create), `IssueSuspension` (Create), `ManageSuspension` (Update) | §26 |
| 1 | `EmployeeGrievanceController` | `EmployeeGrievance` | `HR_EMPLOYEE_RELATION` | `Submit` (Create), `SubmitWhistleblower` (Create), `ViewOwn` (Read), `WhistleblowerReview` (Read), `Mediate` (Update) | §26 |
| 1 | `BipartiteForumController` | `BipartiteForum` | `HR_EMPLOYEE_RELATION` | `Schedule` (Create), `Read` (Read), `RecordMinutes` (Update), `UploadAgreement` (Update) | §26 |
| 1 | `EmployeeSeparationController` | `EmployeeSeparation` | `HR_EMPLOYEE_RELATION` | `Initiate` (Create), `Read` (Read), `CalculateSeverance` (Update), `ViewSeverance` (Read), `ApproveSeveranceFinance` (Update) | §26 |
| 1 | `MultiDeptExitClearanceController` | `MultiDeptExitClearance` | `HR_EMPLOYEE_RELATION` | `Read` (Read), `SignoffChecklist` (Update), `ViewLockStatus` (Read), `Finalize` (Update) | §26 |
| 2 | `HealthRecordsController` | `OccupationalHealthRecord` | `HR_OCCUPATIONAL_HEALTH` | `ViewClinicalDetail` (Read), `ViewSummary` (Read), `AssessFitnessToWork` (Update), `ViewOwn` (Read) | §27 |
| 2 | `MedicalExaminationsController` | `OccupationalMedicalExamination` | `HR_OCCUPATIONAL_HEALTH` | `ScheduleMcu` (Create), `Read` (Read), `UploadMcuResults` (Update), `RecordHazardSurveillance` (Create) | §27 |
| 2 | `NeedleStickEmergencyController` | `NeedleStickEmergency` | `HR_OCCUPATIONAL_HEALTH` | `ReportNeedleStickEmergency` (Create), `Read` (Read), `PrescribePep` (Update), `ManageSerialSurveillance` (Update) | §27 |
| 2 | `StaffVaccinationsController` | `StaffVaccination` | `HR_OCCUPATIONAL_HEALTH` | `RecordVaccination` (Create), `Read` (Read), `ViewImmunityRadar` (Read) | §27 |
| 2 | `WorkRestrictionsAndInjuryController` | `WorkRestrictionAndInjury` | `HR_OCCUPATIONAL_HEALTH` | `IssueWorkRestriction` (Create), `Read` (Read), `AssessReturnToWork` (Update), `ReportOccupationalInjury` (Create), `PrepareBpjsClaimStage1` (Read) | §27 |
| 2 | `ClinicalPeerReviewAuditController` | `ClinicalPeerReviewAudit` | `HR_PERFORMANCE` | `LogAccess` (Create), `Read` (Read) | §23 |
| 2 | `FppeFocusedEvaluationController` | `FppeFocusedEvaluation` | `HR_PERFORMANCE` | `ViewTrigger` (Read), `ReceiveTrigger` (Create), `CreateEngagement` (Create), `Read` (Read), `AddCaseLog` (Create), `VerifyCaseLog` (Update), `ApproveConclusion` (Update), `SyncPrivilege` (Update) | §23 |
| 2 | `OppeOngoingEvaluationController` | `OppeOngoingEvaluation` | `HR_PERFORMANCE` | `ViewPeriod` (Read), `ManagePeriod` (Update), `ViewEvaluation` (Read), `CalculateEvaluation` (Update), `FinalizeEvaluation` (Update), `AcknowledgeEvaluation` (Update), `AssessPeerReview` (Update) | §23 |
| 2 | `RecredentialingQualityController` | `RecredentialingQuality` | `HR_PERFORMANCE` | `Read` (Read), `Decide` (Update), `DispatchSimrs` (Update), `ViewSyncAudit` (Read) | §23 |
| 2 | `PerformanceAppraisalsController` | `PerformanceAppraisal` | `HR_PERFORMANCE` | `ViewOwn` (Read), `SelfAssess` (Update), `ViewSubordinates` (Read), `ManagerAssess` (Update), `SecondLineReview` (Update), `Acknowledge` (Update) | §25 |
| 2 | `PerformanceCalibrationController` | `PerformanceCalibration` | `HR_PERFORMANCE` | `ViewCalibration` (Read), `ManageCalibration` (Update), `SubmitDispute` (Create), `ManageDispute` (Update) | §25 |
| 2 | `PerformanceCyclesController` | `PerformanceCycleGoal` | `HR_PERFORMANCE` | `ViewCycle` (Read), `ManageCycle` (Update), `ViewMetric` (Read), `ManageMetric` (Update), `ViewGoal` (Read), `ManageGoal` (Update) | §25 |
| 2 | `PerformancePipAndCareerController` | `PerformancePipAndCareer` | `HR_PERFORMANCE` | `ViewPip` (Read), `ManagePip` (Update), `ApplyClinicalCareer` (Create), `ManageClinicalCareer` (Update) | §25 |
| 2 | `EmployeeDocumentIssuanceController` | `EmployeeDocumentIssuance` | `HR_SERVICE_MANAGEMENT` | `Request` (Create), `ApproveDraft` (Update), `Issue` (Update), `Read` (Read), `Download` (Read), `Revoke` (Update) | §21 |
| 2 | `HrServiceCatalogController` | `HrServiceCatalog` | `HR_SERVICE_MANAGEMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Delete` (Delete) | §21 |
| 2 | `HrServiceTicketController` | `HrServiceTicket` | `HR_SERVICE_MANAGEMENT` | `Read` (Read), `Assign` (Update), `Process` (Update) | §21 |
| 2 | `HrServiceEssController` | `HrServiceEss` | `HR_SERVICE_MANAGEMENT` | `CreateRequest` (Create), `ViewOwn` (Read) | §21 |
| 2 | `MandatoryHospitalTrainingController` | `MandatoryHospitalTraining` | `HR_LEARNING_DEVELOPMENT` | `Read` (Read), `Manage` (Update) | §24 |
| 2 | `BenefitClaimController` | `BenefitClaim` | `HR_BENEFIT_MANAGEMENT` | `Read` (Read), `ViewOwn` (Read), `SubmitClaim` (Create), `Verify` (Update), `Settle` (Update), `TerminateEnrollment` (Update) | §20 |
| 2 | `BenefitEnrollmentController` | `BenefitEnrollment` | `HR_BENEFIT_MANAGEMENT` | `Read` (Read), `Manage` (Update), `VerifyDependent` (Update), `ViewOwn` (Read), `ManageFamily` (Create) | §20 |
| 2 | `BenefitPlanController` | `BenefitPlanEligibility` | `HR_BENEFIT_MANAGEMENT` | `Read` (Read), `Manage` (Update) | §20 |
| 2 | `InHouseHealthcareController` | `InHouseHealthcare` | `HR_BENEFIT_MANAGEMENT` | `Inquiry` (Read), `Deduct` (Update), `Read` (Read), `Adjust` (Update) | §20 |
| 2 | `BusinessTravelRequestsController` | `BusinessTravelRequest` | `HR_BUSINESS_TRAVEL` | `Read` (Read), `Create` (Create), `Update` (Update), `Cancel` (Update) | §29 |
| 2 | `BusinessTravelAdvancesController` | `BusinessTravelAdvance` | `HR_BUSINESS_TRAVEL` | `Read` (Read), `Request` (Create), `Approve` (Update), `Disburse` (Update) | §29 |
| 2 | `BusinessTravelSettlementsController` | `BusinessTravelSettlement` | `HR_BUSINESS_TRAVEL` | `Read` (Read), `Submit` (Create), `Verify` (Update), `Close` (Update) | §29 |
| 2 | `DirectExpenseClaimsController` | `DirectExpenseClaim` | `HR_BUSINESS_TRAVEL` | `Read` (Read), `Create` (Create), `Verify` (Update), `Pay` (Update) | §29 |
| 2 | `TravelRatesAndPoliciesController` | `TravelRatePolicy` | `HR_BUSINESS_TRAVEL` | `ViewRate` (Read), `ManageRate` (Update), `MonitorOverdue` (Read) | §29 |
| 2 | `ClinicalPrivilegeGateController` | `ClinicalPrivilegeGate` | `HR_CREDENTIALING` | `Validate` (Read), `CoSign` (Update), `EmergencyOverride` (Create), `DecideKmkpAudit` (Update), `Read` (Read), `GrantTemporary` (Update) | §22 |
| 2 | `ProfessionalLicenseController` | `ProfessionalLicense` | `HR_CREDENTIALING` | `Create` (Create), `Read` (Read), `VerifyPsv` (Update), `Renew` (Update), `ExecuteFailClosed` (Update) | §22 |
| 2 | `SpkRkkIssuanceController` | `SpkRkkIssuance` | `HR_CREDENTIALING` | `Read` (Read), `CreateApplication` (Create), `AssignReviewer` (Update), `AssessPeer` (Update), `ApproveCommittee` (Update), `ParafDirector` (Update), `SignDirector` (Update) | §22 |
| 2 | `AnnualTrainingPlansController` | `AnnualTrainingPlan` | `HR_LEARNING_DEVELOPMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Submit` (Update), `Approve` (Update), `Reject` (Update), `Delete` (Delete) | §24 |
| 2 | `TrainingSessionsController` | `TrainingSession` | `HR_LEARNING_DEVELOPMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Manage` (Update), `Delete` (Delete) | §24 |
| 2 | `TrainingEnrollmentsController` | `TrainingEnrollment` | `HR_LEARNING_DEVELOPMENT` | `Read` (Read), `Submit` (Create), `Approve` (Update), `Delete` (Delete) | §24 |
| 2 | `TrainingAttendancesController` | `TrainingAttendance` | `HR_LEARNING_DEVELOPMENT` | `Record` (Create), `Read` (Read) | §24 |
| 2 | `TheoryPostTestAssessmentsController` | `TheoryPostTestAssessment` | `HR_LEARNING_DEVELOPMENT` | `Grade` (Update), `Read` (Read) | §24 |
| 2 | `ClinicalOsceAssessmentsController` | `ClinicalOsceAssessment` | `HR_LEARNING_DEVELOPMENT` | `Grade` (Update), `Read` (Read) | §24 |
| 2 | `TrainingCertificatesController` | `TrainingCertificate` | `HR_LEARNING_DEVELOPMENT` | `Issue` (Update), `Read` (Read) | §24 |
| 2 | `TrainingBudgetsController` | `TrainingBudget` | `HR_LEARNING_DEVELOPMENT` | `Read` (Read), `Manage` (Update) | §24 |
| 2 | `TrainingBondingsController` | `TrainingBonding` | `HR_LEARNING_DEVELOPMENT` | `Manage` (Update), `Read` (Read) | §24 |
| 2 | `PlataranSehatSkpsController` | `PlataranSehatSkp` | `HR_LEARNING_DEVELOPMENT` | `Create` (Create), `Read` (Read), `Sync` (Update), `Verify` (Update) | §24 |
| 2 | `HrdOnboardingCaseController` | `WorkforceOnboardingCase` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `CompleteItem` (Update), `Reassign` (Update), `DeclareReady` (Update), `CancelJoin` (Update) | §13 |
| 2 | `HrdEmploymentContractController` | `WorkforceEmploymentContract` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `MarkSigned` (Update), `SubmitDecision` (Update), `RecordNotification` (Update), `RequestCorrection` (Update), `UploadDocument` (Update) | §13 |
| 2 | `HrdProbationController` | `WorkforceProbation` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Submit` (Update) | §13 |
| 2 | `HrdClinicalPreceptorshipController` | `WorkforcePreceptorship` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Manage` (Update) | §13 |
| 2 | `HrdInitialContractBatchController` | `WorkforceInitialContractBatch` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Submit` (Update), `Cancel` (Update) | §13 |
| 2 | `HrdOnboardingAccessRequestController` | `WorkforceOnboardingAccessRequest` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Complete` (Update), `Retry` (Update) | §13 |
| 2 | `HrdLifecycleWorklistController` | `WorkforceLifecycleWorklist` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read) | §13 |
| 2 | `RestitutionClaimController` | `RestitutionClaim` | `HR_PAYROLL_MANAGEMENT` | `ViewOwn` (Read), `Submit` (Create), `Read` (Read), `ApproveSupervisor` (Update), `ReviewHr` (Update), `ApproveFinal` (Update), `Reject` (Update), `Correct` (Update), `InjectToPayroll` (Update) | §19 |
| 2 | `RestitutionSettingController` | `RestitutionSetting` | `HR_PAYROLL_MANAGEMENT` | `Read` (Read), `Manage` (Update) | §19 |
| 2 | `PayrollDeductionScheduleController` | `PayrollDeductionSchedule` | `HR_PAYROLL_MANAGEMENT` | `Manage` (Update), `Read` (Read), `Execute` (Update) | §19 |
| 2 | `CashierVoucherController` | `CashierVoucher` | `HR_PAYROLL_MANAGEMENT` | `Issue` (Create), `Read` (Read), `Verify` (Update), `Disburse` (Update) | §19 |
| 2 | `ExitClearanceReceivableController` | `ExitClearanceReceivable` | `HR_PAYROLL_MANAGEMENT` | `Calculate` (Update), `ConfirmSettlement` (Update) | §19 |
| 2 | `WorkflowInboxAndInstancesController` | `WorkflowInboxInstance` | `HR_WORKFLOW` | `ViewInbox` (Read), `ViewInstanceDetail` (Read), `CreateInstance` (Create), `CancelMyInstance` (Update), `ViewOwn` (Read) | §28 |
| 2 | `WorkflowApprovalActionsController` | `WorkflowApprovalAction` | `HR_WORKFLOW` | `ApproveRequest` (Update), `RejectRequest` (Update), `ReturnForRevision` (Update), `BulkApprove` (Update), `ResubmitRevision` (Update) | §28 |
| 2 | `WorkflowCommentsAndAttachmentsController` | `WorkflowCommentAttachment` | `HR_WORKFLOW` | `ViewInstanceDetail` (Read), `AddComment` (Create), `UploadAttachment` (Create) | §28 |
| 2 | `WorkflowDelegationsController` | `WorkflowDelegation` | `HR_WORKFLOW` | `ViewDelegationList` (Read), `CreateDelegation` (Create), `RevokeDelegation` (Update), `ViewOwn` (Read) | §28 |
| 2 | `WorkflowSlaAndMatrixController` | `WorkflowSlaMatrix` | `HR_WORKFLOW` | `ViewMatrix` (Read), `ConfigureMatrix` (Update), `ViewSlaOverdue` (Read), `ManualEscalate` (Update), `ExportSlaReport` (Read) | §28 |
| 3 | `JobRequisitionController` | `JobRequisition` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Submit` (Update), `Cancel` (Update) | §12 |
| 3 | `JobVacancyController` | `JobVacancy` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Publish` (Update), `Close` (Update) | §12 |
| 3 | `CandidateController` | `Candidate` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `UploadDocument` (Update) | §12 |
| 3 | `CandidateScreeningController` | `CandidateScreening` | `HR_RECRUITMENT` | `Read` (Read), `Evaluate` (Update) | §12 |
| 3 | `CandidateEvaluationController` | `CandidateEvaluation` | `HR_RECRUITMENT` | `Read` (Read), `Evaluate` (Update), `EvaluateClinical` (Update) | §12 |
| 3 | `JobOfferController` | `JobOffer` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Update` (Update) | §12 |
| 3 | `JobOfferApprovalController` | `JobOfferApproval` | `HR_RECRUITMENT` | `Read` (Read), `Approve` (Update) | §12 |
| 3 | `McuClearanceController` | `OccupationalHealthClearance` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Evaluate` (Update) | §12 |
| 3 | `TalentPoolController` | `TalentPool` | `HR_RECRUITMENT` | `Read` (Read), `Create` (Create), `Promote` (Update) | §12 |
| 3 | `HiringHandoffController` | `HiringHandoff` | `HR_RECRUITMENT` | `Read` (Read), `Execute` (Update) | §12 |
| 3 | `HrdEmploymentLifecycleSettingController` | `WorkforceLifecycleSetting` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Update` (Update) | §13 |
| 3 | `HrdOnboardingTemplateController` | `WorkforceOnboardingTemplate` | `HR_LIFECYCLE_MANAGEMENT` | `Read` (Read), `Create` (Create), `Update` (Update), `Delete` (Delete) | §13 |

---

## 6. Selisih terhadap kontrak

Kontrak memakai tiga format kode yang berbeda antarbagian (`Hrd.Disciplinary.IssueSanction`,
`Training_Plan:Approve`, `[AccessPermission("HR_Service_Ticket", "View")]`). Mesin hak akses hanya
mengenal pasangan `Resource : Action`, sehingga setiap kode diterjemahkan. Selisih yang perlu
diketahui:

| Kontrak | Di kode | Sebab |
|---|---|---|
| `Hrd.Disciplinary.WhistleblowerReview` | `EmployeeGrievance : WhistleblowerReview` | Endpoint kotak masuk whistleblower berada di `EmployeeGrievanceController`; resource wajib sama dengan controller-nya |
| `WorkforceContractDecision : Submit / RecordNotification` (§13) | `WorkforceEmploymentContract : SubmitDecision / RecordNotification` | Keputusan kontrak dilayani `HrdEmploymentContractController` yang sama |
| `JobOfferTier2 : Approve` (§12) | `JobOfferApproval : Approve` | Hanya ada satu endpoint `approve`; pemisahan tingkat 1 dan tingkat 2 ditentukan data penugasan, bukan permission. Pemeriksaan penugasan wajib tetap ada di service |
| `ClinicalAssessment : Evaluate` (§12) | `CandidateEvaluation : EvaluateClinical` | Asesmen klinis dilayani `CandidateEvaluationController` |
| `OccupationalHealthClearance : Evaluate` (§12) | `OccupationalHealthClearance : Evaluate` + `Read` + `Create` | Controller juga punya daftar, detail, dan registrasi MCU |
| `Recruitment : Read/Create/Update` (kode lama) | Sepuluh resource per controller sesuai §12 | Kode lama tidak pernah terdaftar, sehingga belum ada policy yang bergantung padanya. Mengganti nama tidak memutus hak siapa pun |
| `LifecycleSettings`, `OnboardingTemplates` (kode lama) | `WorkforceLifecycleSetting`, `WorkforceOnboardingTemplate` | Sama seperti di atas; nama mengikuti §13 |
| `WorkforceOnboardingTemplate` tanpa `Delete` (§13) | `Delete` dipertahankan | Endpoint `DELETE` sudah ada di kode |
| `Hrd.BusinessTravel.MissingReceipt.*`, `Hrd.Expense.Claim.Approve/Edit`, `Hrd.Expense.Category.Manage`, `Hrd.BusinessTravel.Audit.View` (§29) | Tidak ada | Endpoint-nya belum ada di kode. Klaim laporan `BE-HRD-333` tentang "24 permission terkunci" karena itu baru terpenuhi 19 aksi pada 5 resource |
| Aksi "Access" untuk membuka grup menu (`HR_Service:Access`, `Oppe_Governance:Access`, dll.) | Tidak ada | Mesin hak akses tidak punya konsep grup menu tanpa endpoint |

---

## 7. Validasi

| Pemeriksaan | Perintah | Hasil |
|---|---|---|
| Build | `dotnet build QuilvianSystemBackend.csproj` | **PASS** — 0 error, 578 warning (jumlah sama dengan sebelum fase 2–4; tidak ada warning baru) |
| Registry hak akses | `PermissionRegistryDescriptor.BuildFromAssembly` + `PermissionRegistryValidator.Validate` dijalankan dari program sementara di luar repository | **PASS** — `IsValid=True`; 0 endpoint tanpa metadata, 0 resource ganda, 0 `AccessType` tidak sah, 0 endpoint telanjang, 0 baseline basi; 2.267 kunci terdaftar |
| Keterlihatan di layar Akses Role | Snapshot registry yang sama | Semua aksi baru `VisibleInRoleAccess=true` dan `IsSystemOnly=false` |
| QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef a1d4aa9f -HeadRef edfd843b -Mode Strict` | **PASS** — 74 file dievaluasi, 0 violation, 0 review |
| Pindai ulang controller HR | Pemindai atribut pada 267 controller `Areas/Corporate/HumanResource` dan `Areas/SelfServices/HumanResource` | Tersisa 3 controller, ketiganya disengaja (bagian 4) |
| Aktor karangan | `grep` pola `11111111-1111`, `Identity?.Name ?? "`, `@rs-mmc.co.id` pada controller HR | 0 sisa |
| Uji runtime dengan pengguna nyata | — | **NOT RUN** — aplikasi tidak dijalankan dan database tidak disentuh |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Status migration dan database: **tidak ada** perubahan schema, **tidak ada** migration, **tidak
ada** eksekusi database. Baris registry lahir otomatis saat startup lewat `AccessMenuSeeder`.

---

## 8. Temuan terbuka yang sengaja tidak diubah

Temuan berikut menyangkut kepemilikan data atau peran yang di-hardcode. Memperbaikinya menuntut
keputusan identitas atau kebijakan, jadi dicatat, bukan ditebak.

| No | Lokasi | Masalah | Contoh dampak | Keputusan yang dibutuhkan |
|---:|---|---|---|---|
| 1 | `EmployeeGrievanceController.GetMyTickets` | `employeeId` diterima dari query | Pegawai A bisa melihat tiket keluhan pegawai B dengan mengganti `employeeId` | Identitas mana yang dipakai: klaim `employee_id` atau `workforce_profile_id`? `ComplainantEmployeeId` saat ini diisi dari body dan juga dipakai sebagai `WorkforceProfileId` |
| 2 | `EmployeeGrievanceController.SubmitGrievance` | `ComplainantEmployeeId` diambil dari body | Pegawai bisa mengajukan keluhan atas nama orang lain | Sama dengan nomor 1 |
| 3 | `HealthRecordsController.GetMyHealth` | `employeeId` diterima dari query; nilai bawaan memakai ID akun, padahal service mencari ID pegawai | Profil kesehatan pegawai lain bisa dibaca; jalur "diri sendiri" tidak menemukan data | Sama dengan nomor 1 |
| 4 | `OppeOngoingEvaluationController.GetAssignedPeerReviews` | `evaluatorId` diterima dari route | Dokter bisa melihat daftar tugas telaah sejawat dokter lain | Apakah evaluator diturunkan dari pengguna login? |
| 5 | Sepuluh controller Benefit, Perjalanan Dinas, Layanan SDM ESS, Workflow V2 | Membaca klaim `"EmployeeId"` / `"WorkforceProfileId"` yang tidak pernah ada di token (token berisi `employee_id`, `workforce_profile_id`) | Selalu jatuh ke ID akun, sehingga data "milik saya" tidak cocok dengan data pegawai | Mengganti klaim mengubah identitas aktor yang tersimpan; perlu keputusan migrasi data lama |
| 6 | `HealthRecordsController.IsDoctorRole` | `IsInRole("DokterK3RS")`, `IsInRole("OccupationalHealthDoctor")`, `IsInRole("Admin")` | Dokter K3 dengan nama peran lain hanya melihat data tersamarkan (aman, tetapi tidak sesuai matriks Akses Role) | Ganti dengan aksi `OccupationalHealthRecord : ViewClinicalDetail` saja? |

---

## 9. Status Git

Perubahan ini **sudah ter-commit dan ter-push** sebelum laporan ini ditulis, oleh commit
`edfd843b` milik `MHamzah1` pukul 10:07:49 WIB dengan pesan *"feat: Add reports for HRD tasks
FE-HRD-231 to FE-HRD-245 …"*. Commit itu memuat 16 dokumen laporan frontend **dan** 74 controller
hasil remediasi ini. Agent tidak melakukan commit, push, atau operasi Git lain. Pesan commit tidak
menggambarkan perubahan keamanan di dalamnya; riwayat Git sengaja tidak diubah.

---

## 10. Langkah berikutnya

1. **Pemilik** menetapkan task ID resmi untuk remediasi ini — usulan: `BE-SEC-019` pada roadmap
   `platform-authorization`, melanjutkan seri `BE-SEC-012` s.d. `BE-SEC-018`.
2. **Pemilik modul HR** menyusun matriks pemberian hak akses per jabatan (bagian 3) sebelum
   deploy ke lingkungan yang dipakai petugas.
3. **Admin** memberikan hak akses lewat layar Akses Role, lalu menguji minimal satu akun non-SuperAdmin
   per modul. Pengujian dengan SuperAdmin tidak membuktikan apa pun karena SuperAdmin melewati
   seluruh pemeriksaan.
4. **Pemilik** memutuskan enam temuan pada bagian 8.


---

## 11. Penutupan temuan bagian 8 (11 November 2026, Gelombang 2)

| No | Keputusan yang diterapkan | Perubahan | Status |
|---:|---|---|---|
| 1 | Identitas "milik saya" dari klaim token: `workforce_profile_id` → `employee_id` → ID akun (`HrActorClaims.ResolveWorkforceProfileId`) | `EmployeeGrievanceController.GetMyTickets` tanpa parameter `employeeId`; service memfilter per profil/karyawan/akun pelapor | ✅ |
| 2 | Sama dengan nomor 1 | `SubmitGrievance` mengisi pelapor dari token; nilai pada body diabaikan | ✅ |
| 3 | Modul kesehatan kerja memakai `MstEmployee.Id` → klaim `employee_id` (`HrActorClaims.ResolveEmployeeId`) | `HealthRecordsController.GetMyHealth` tanpa parameter query | ✅ |
| 4 | Evaluator dari klaim `workforce_profile_id`; nilai route yang berbeda ditolak 403 (route dipertahankan untuk kompatibilitas) | `OppeOngoingEvaluationController.GetAssignedPeerReviews` | ✅ |
| 5 | Sepuluh controller membaca klaim yang benar lewat helper bersama; tabel berkolom `WorkforceProfileId` (benefit, perjalanan dinas, ESS) memakai `ResolveWorkforceProfileId`, workflow V2 (`MstEmployee`) memakai `ResolveEmployeeId`. Data lama yang terlanjur berisi ID akun dipetakan ulang oleh migration `20261111020000` (perjalanan dinas, uang muka, klaim biaya, tiket ESS, keluhan) | `Shared/HumanResource/Helpers/HrActorClaims.cs` + 10 controller | 🟡 sampai migration dijalankan |
| 6 | Hak membaca diagnosis dari hak akses `OccupationalHealthRecord : ViewClinicalDetail` lewat `AccessPermissionService.HasAccessAsync`, bukan nama peran | `HealthRecordsController.IsDoctorRole` dihapus | ✅ |

Verifikasi: `dotnet build` 0 error (602 warning); QBE Strict 43 berkas 0 violation; uji runtime `NOT RUN`. Rinciannya pada [backend-unfinished-work.md](../roadmap/backend-unfinished-work.md) bagian 13.
