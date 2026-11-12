using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Membuat 15 tabel HR yang sudah ada di model EF tetapi tidak pernah dibuat oleh migration mana pun
    /// (OPPE, FPPE, kredensialing SPK/RKK, ikatan dinas, SKP, dan lainnya). Ditemukan 11 November 2026 saat
    /// <c>20261110050000</c> gagal karena <c>trx_fppe_engagements</c> tidak ada.
    /// </summary>
    /// <remarks>
    /// DDL diturunkan dari <c>dotnet ef dbcontext script</c>, lalu diberi <c>IF NOT EXISTS</c>. Tidak mengubah tabel yang sudah ada.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261110040000_AddMissingHrdTablesFromModel")]
    public partial class AddMissingHrdTablesFromModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- hrd_clinical_peer_review_audit_logs
CREATE TABLE IF NOT EXISTS public.hrd_clinical_peer_review_audit_logs (
    ""Id"" uuid NOT NULL,
    ""AccessedWorkforceProfileId"" uuid NOT NULL,
    ""RecordType"" character varying(50) NOT NULL DEFAULT 'OPPE_REPORT',
    ""RecordId"" uuid NOT NULL,
    ""AccessedByUserId"" uuid NOT NULL,
    ""AccessReason"" character varying(250) NOT NULL,
    ""AccessedAt"" timestamp with time zone NOT NULL,
    ""IpAddress"" character varying(50) NOT NULL DEFAULT '127.0.0.1',
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_hrd_clinical_peer_review_audit_logs"" PRIMARY KEY (""Id"")
);
-- mst_oppe_metric_catalogs
CREATE TABLE IF NOT EXISTS public.mst_oppe_metric_catalogs (
    ""Id"" uuid NOT NULL,
    ""MetricCode"" character varying(30) NOT NULL,
    ""MetricName"" character varying(150) NOT NULL,
    ""CompetencyDomain"" character varying(50) NOT NULL,
    ""SpecialtyGroup"" character varying(50) NOT NULL DEFAULT 'ALL',
    ""DataSourceType"" character varying(30) NOT NULL DEFAULT 'SIMRS_VOLUME',
    ""TargetStandardValue"" numeric(8,2) NOT NULL DEFAULT 85.0,
    ""MetricWeightPercentage"" numeric(5,2) NOT NULL DEFAULT 10.0,
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_mst_oppe_metric_catalogs"" PRIMARY KEY (""Id"")
);
-- trx_fppe_engagements
CREATE TABLE IF NOT EXISTS public.trx_fppe_engagements (
    ""Id"" uuid NOT NULL,
    ""EngagementNumber"" character varying(100) NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""PrivilegeCatalogId"" uuid,
    ""TriggerSource"" character varying(50) NOT NULL,
    ""InitialReasonJustification"" text NOT NULL,
    ""AssignedProctorWorkforceId"" uuid NOT NULL,
    ""StartDate"" timestamp with time zone NOT NULL,
    ""TargetEndDate"" timestamp with time zone NOT NULL,
    ""TargetCaseCount"" integer NOT NULL,
    ""CompletedCaseCount"" integer NOT NULL,
    ""Status"" character varying(30) NOT NULL,
    ""CreateDateTime"" timestamp with time zone NOT NULL,
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL,
    ""IsDelete"" boolean NOT NULL,
    CONSTRAINT ""PK_trx_fppe_engagements"" PRIMARY KEY (""Id"")
);
-- trx_oppe_evaluation_periods
CREATE TABLE IF NOT EXISTS public.trx_oppe_evaluation_periods (
    ""Id"" uuid NOT NULL,
    ""PeriodCode"" character varying(50) NOT NULL,
    ""PeriodName"" character varying(150) NOT NULL,
    ""EvaluationYear"" integer NOT NULL,
    ""Semester"" integer NOT NULL DEFAULT 1,
    ""StartDate"" timestamp with time zone NOT NULL,
    ""EndDate"" timestamp with time zone NOT NULL,
    ""Status"" character varying(30) NOT NULL DEFAULT 'OPEN',
    ""QuantitativeWeightPercentage"" numeric(5,2) NOT NULL DEFAULT 60.0,
    ""QualitativeWeightPercentage"" numeric(5,2) NOT NULL DEFAULT 40.0,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_oppe_evaluation_periods"" PRIMARY KEY (""Id"")
);
-- trx_fppe_case_logs
CREATE TABLE IF NOT EXISTS public.trx_fppe_case_logs (
    ""Id"" uuid NOT NULL,
    ""FppeEngagementId"" uuid NOT NULL,
    ""SimrsPatientEpisodeId"" character varying(100) NOT NULL,
    ""ProcedurePerformed"" character varying(200) NOT NULL,
    ""ProcedureDate"" timestamp with time zone NOT NULL,
    ""TechnicalSkillScore"" integer NOT NULL DEFAULT 3,
    ""CriticalDecisionScore"" integer NOT NULL DEFAULT 3,
    ""HasComplication"" boolean NOT NULL DEFAULT FALSE,
    ""ComplicationDescription"" text,
    ""ProctorFeedbackNotes"" text NOT NULL,
    ""ObservedIndependenceLevel"" integer NOT NULL DEFAULT 2,
    ""IsVerifiedByProctor"" boolean NOT NULL DEFAULT TRUE,
    ""ProctorVerifiedAt"" timestamp with time zone,
    ""TrxFppeEngagementId"" uuid,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_fppe_case_logs"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_trx_fppe_case_logs_trx_fppe_engagements_FppeEngagementId"" FOREIGN KEY (""FppeEngagementId"") REFERENCES public.trx_fppe_engagements (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_trx_fppe_case_logs_trx_fppe_engagements_TrxFppeEngagementId"" FOREIGN KEY (""TrxFppeEngagementId"") REFERENCES public.trx_fppe_engagements (""Id"")
);
-- trx_fppe_evaluation_conclusions
CREATE TABLE IF NOT EXISTS public.trx_fppe_evaluation_conclusions (
    ""Id"" uuid NOT NULL,
    ""FppeEngagementId"" uuid NOT NULL,
    ""RecommendedAction"" character varying(50) NOT NULL DEFAULT 'GRANT_INDEPENDENT',
    ""PlenaryReviewSummary"" text NOT NULL,
    ""RemediationPlan"" text,
    ""ApprovedByCommitteeChairmanId"" uuid NOT NULL,
    ""FinalizedAt"" timestamp with time zone NOT NULL,
    ""SignatureHash"" character varying(256) NOT NULL,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_fppe_evaluation_conclusions"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_trx_fppe_evaluation_conclusions_trx_fppe_engagements_FppeEn~"" FOREIGN KEY (""FppeEngagementId"") REFERENCES public.trx_fppe_engagements (""Id"") ON DELETE RESTRICT
);
-- trx_oppe_staff_evaluations
CREATE TABLE IF NOT EXISTS public.trx_oppe_staff_evaluations (
    ""Id"" uuid NOT NULL,
    ""PeriodId"" uuid NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""CommitteeId"" uuid,
    ""QuantitativeScore"" numeric(5,2) NOT NULL DEFAULT 0.0,
    ""QualitativeScore"" numeric(5,2) NOT NULL DEFAULT 0.0,
    ""FinalCompositeScore"" numeric(5,2) NOT NULL DEFAULT 0.0,
    ""PerformanceGrade"" character varying(20) NOT NULL DEFAULT 'C',
    ""EvaluatorKsmHeadId"" uuid,
    ""EvaluatorRecommendationNotes"" text,
    ""IsAcknowledgedByStaff"" boolean NOT NULL DEFAULT FALSE,
    ""AcknowledgedAt"" timestamp with time zone,
    ""Status"" character varying(30) NOT NULL,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_oppe_staff_evaluations"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_trx_oppe_staff_evaluations_trx_oppe_evaluation_periods_Peri~"" FOREIGN KEY (""PeriodId"") REFERENCES public.trx_oppe_evaluation_periods (""Id"") ON DELETE RESTRICT
);
-- trx_oppe_staff_evaluation_details
CREATE TABLE IF NOT EXISTS public.trx_oppe_staff_evaluation_details (
    ""Id"" uuid NOT NULL,
    ""StaffEvaluationId"" uuid NOT NULL,
    ""MetricCatalogId"" uuid NOT NULL,
    ""TargetValue"" numeric(8,2) NOT NULL,
    ""ActualValue"" numeric(8,2) NOT NULL,
    ""ComputedScore"" numeric(5,2) NOT NULL,
    ""EvaluatorNotes"" text,
    ""RawMetricEvidenceJson"" text,
    ""TrxOppeStaffEvaluationId"" uuid,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_oppe_staff_evaluation_details"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_trx_oppe_staff_evaluation_details_mst_oppe_metric_catalogs_~"" FOREIGN KEY (""MetricCatalogId"") REFERENCES public.mst_oppe_metric_catalogs (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_trx_oppe_staff_evaluation_details_trx_oppe_staff_evaluation~"" FOREIGN KEY (""StaffEvaluationId"") REFERENCES public.trx_oppe_staff_evaluations (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_trx_oppe_staff_evaluation_details_trx_oppe_staff_evaluatio~1"" FOREIGN KEY (""TrxOppeStaffEvaluationId"") REFERENCES public.trx_oppe_staff_evaluations (""Id"")
);
-- HrdBenefitGradeEligibility
CREATE TABLE IF NOT EXISTS public.""HrdBenefitGradeEligibility"" (
    ""Id"" uuid NOT NULL,
    ""BenefitPlanId"" uuid NOT NULL,
    ""GradeLevelCode"" character varying(50) NOT NULL,
    ""EmploymentTypeCode"" character varying(50) NOT NULL DEFAULT 'Permanent',
    ""SpecificAnnualLimit"" numeric(18,2),
    ""InpatientRoomClass"" character varying(50) NOT NULL DEFAULT 'Kelas 1',
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL,
    ""IsDelete"" boolean NOT NULL,
    CONSTRAINT ""PK_HrdBenefitGradeEligibility"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_HrdBenefitGradeEligibility_MstBenefitPlan_BenefitPlanId"" FOREIGN KEY (""BenefitPlanId"") REFERENCES public.""MstBenefitPlan"" (""Id"") ON DELETE RESTRICT
);
-- hrd_training_bond_contracts
CREATE TABLE IF NOT EXISTS public.hrd_training_bond_contracts (
    ""Id"" uuid NOT NULL,
    ""ContractNumber"" character varying(60) NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""TrainingSessionId"" uuid,
    ""TrainingEnrollmentRequestId"" uuid,
    ""TrainingProgramName"" character varying(250) NOT NULL,
    ""TotalTrainingCost"" numeric(15,2) NOT NULL DEFAULT 0.0,
    ""HospitalFinancedAmount"" numeric(15,2) NOT NULL DEFAULT 0.0,
    ""RetentionPeriodMonths"" integer NOT NULL DEFAULT 12,
    ""BondStartDate"" timestamp with time zone NOT NULL,
    ""BondEndDate"" timestamp with time zone NOT NULL,
    ""BondStatus"" character varying(40) NOT NULL DEFAULT 'ACTIVE',
    ""IsBreached"" boolean NOT NULL DEFAULT FALSE,
    ""BreachRecordedAt"" timestamp with time zone,
    ""CalculatedPenaltyAmount"" numeric(15,2),
    ""BreachReason"" character varying(1000),
    ""PenaltyInvoiceNumber"" character varying(50),
    ""DigitalSignatureHash"" character varying(256),
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_hrd_training_bond_contracts"" PRIMARY KEY (""Id"")
);
-- HrdProfileChangeHistory
CREATE TABLE IF NOT EXISTS public.""HrdProfileChangeHistory"" (
    ""Id"" uuid NOT NULL,
    ""HrServiceRequestId"" uuid NOT NULL,
    ""EmployeeId"" uuid NOT NULL,
    ""ComponentModified"" character varying(100) NOT NULL,
    ""OldValuesJson"" jsonb NOT NULL,
    ""NewValuesJson"" jsonb NOT NULL,
    ""ApprovedByUserId"" uuid NOT NULL,
    ""AppliedAtUtc"" timestamp with time zone NOT NULL,
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_HrdProfileChangeHistory"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_HrdProfileChangeHistory_AspNetUsers_ApprovedByUserId"" FOREIGN KEY (""ApprovedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT
);
-- MstCredentialingCommittee
CREATE TABLE IF NOT EXISTS public.""MstCredentialingCommittee"" (
    ""Id"" uuid NOT NULL,
    ""CommitteeCode"" character varying(50) NOT NULL,
    ""CommitteeName"" character varying(200) NOT NULL,
    ""SubCommitteeCredentialName"" character varying(100) NOT NULL DEFAULT 'Sub-Komite Kredensial',
    ""ChairpersonWorkforceProfileId"" uuid,
    ""SecretaryWorkforceProfileId"" uuid,
    ""MinimumPeerReviewersCount"" integer NOT NULL DEFAULT 2,
    ""Description"" character varying(1000),
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_MstCredentialingCommittee"" PRIMARY KEY (""Id"")
);
-- trx_plataran_sehat_skp_records
CREATE TABLE IF NOT EXISTS public.trx_plataran_sehat_skp_records (
    ""Id"" uuid NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""TrainingCertificateId"" uuid,
    ""ActivityName"" character varying(100) NOT NULL,
    ""SkpCategory"" character varying(50) NOT NULL DEFAULT 'Pembelajaran',
    ""SkpPoints"" numeric(6,2) NOT NULL DEFAULT 1.0,
    ""ActivityDate"" timestamp with time zone NOT NULL,
    ""OrganizerName"" character varying(100),
    ""PlataranSehatCertificateId"" character varying(100),
    ""IsVerifiedByPlataranSehat"" boolean NOT NULL DEFAULT FALSE,
    ""PlataranSehatVerifiedAt"" timestamp with time zone,
    ""IsSyncedToOppe"" boolean NOT NULL DEFAULT FALSE,
    ""OppeSyncedAt"" timestamp with time zone,
    ""EvidenceDocumentUrl"" character varying(1000),
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_trx_plataran_sehat_skp_records"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_trx_plataran_sehat_skp_records_MstWorkforceProfile_Workforc~"" FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT
);
-- TrxClinicalPrivilegeEmergencyOverrideLog
CREATE TABLE IF NOT EXISTS public.""TrxClinicalPrivilegeEmergencyOverrideLog"" (
    ""Id"" uuid NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""PatientId"" uuid,
    ""EncounterId"" uuid,
    ""PrivilegeItemCode"" character varying(100) NOT NULL,
    ""ProcedureName"" character varying(250) NOT NULL,
    ""OverrideReason"" character varying(2000) NOT NULL,
    ""OverrideAtUtc"" timestamp with time zone NOT NULL,
    ""WitnessUserId"" uuid,
    ""WitnessName"" character varying(150),
    ""IsReviewedByKmkp"" boolean NOT NULL DEFAULT FALSE,
    ""ReviewedAtUtc"" timestamp with time zone,
    ""ReviewedByUserId"" uuid,
    ""KmkpReviewNotes"" character varying(2000),
    ""KmkpVerdict"" character varying(50) NOT NULL DEFAULT 'PendingReview',
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_TrxClinicalPrivilegeEmergencyOverrideLog"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_TrxClinicalPrivilegeEmergencyOverrideLog_AspNetUsers_Review~"" FOREIGN KEY (""ReviewedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TrxClinicalPrivilegeEmergencyOverrideLog_AspNetUsers_Witnes~"" FOREIGN KEY (""WitnessUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TrxClinicalPrivilegeEmergencyOverrideLog_MstWorkforceProfil~"" FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT
);
-- TrxSpkRkkIssuance
CREATE TABLE IF NOT EXISTS public.""TrxSpkRkkIssuance"" (
    ""Id"" uuid NOT NULL,
    ""WorkforceProfileId"" uuid NOT NULL,
    ""CommitteeId"" uuid,
    ""CredentialingApplicationId"" uuid,
    ""SpkNumber"" character varying(80) NOT NULL,
    ""RkkNumber"" character varying(80) NOT NULL,
    ""IssuedAt"" timestamp with time zone NOT NULL,
    ""ValidFrom"" timestamp with time zone NOT NULL,
    ""ValidUntil"" timestamp with time zone NOT NULL,
    ""ClinicalPrivilegeType"" character varying(50) NOT NULL DEFAULT 'Regular',
    ""DirectorGeneralUserId"" uuid,
    ""DirectorGeneralName"" character varying(150) NOT NULL,
    ""IsSignedByDirectorGeneral"" boolean NOT NULL DEFAULT FALSE,
    ""SignedAt"" timestamp with time zone,
    ""VerificationHash"" character varying(64),
    ""QrCodeImagePath"" character varying(500),
    ""SpkPdfFilePath"" character varying(500),
    ""RkkPdfFilePath"" character varying(500),
    ""SpkFileSizeBytes"" bigint,
    ""SpkFileChecksum"" text,
    ""EDossierDocumentId"" uuid,
    ""IssuanceStatus"" character varying(30) NOT NULL DEFAULT 'Active',
    ""IsSuspended"" boolean NOT NULL,
    ""SuspendedAt"" timestamp with time zone,
    ""SuspensionReason"" text,
    ""IsRevoked"" boolean NOT NULL,
    ""RevokedAt"" timestamp with time zone,
    ""RevocationBapNumber"" text,
    ""RevocationReason"" text,
    ""ApprovedPrivilegesSnapshotJson"" text,
    ""IsActive"" boolean NOT NULL,
    ""CreateDateTime"" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ""CreateBy"" uuid NOT NULL,
    ""UpdateDateTime"" timestamp with time zone,
    ""UpdateBy"" uuid NOT NULL,
    ""DeleteDateTime"" timestamp with time zone,
    ""DeleteBy"" uuid NOT NULL,
    ""CancelDateTime"" timestamp with time zone,
    ""CancelBy"" uuid NOT NULL,
    ""IsCancel"" boolean NOT NULL DEFAULT FALSE,
    ""IsDelete"" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT ""PK_TrxSpkRkkIssuance"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_TrxSpkRkkIssuance_AspNetUsers_DirectorGeneralUserId"" FOREIGN KEY (""DirectorGeneralUserId"") REFERENCES ""AspNetUsers"" (""Id""),
    CONSTRAINT ""FK_TrxSpkRkkIssuance_MstCredentialingCommittee_CommitteeId"" FOREIGN KEY (""CommitteeId"") REFERENCES public.""MstCredentialingCommittee"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TrxSpkRkkIssuance_MstWorkforceProfile_WorkforceProfileId"" FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TrxSpkRkkIssuance_TrxCredentialingApplication_Credentialing~"" FOREIGN KEY (""CredentialingApplicationId"") REFERENCES public.""TrxCredentialingApplication"" (""Id"")
);
CREATE INDEX IF NOT EXISTS ""IX_hrd_clinical_peer_review_audit_logs_AccessedWorkforceProfil~"" ON public.hrd_clinical_peer_review_audit_logs (""AccessedWorkforceProfileId"");
CREATE INDEX IF NOT EXISTS ""IX_hrd_clinical_peer_review_audit_logs_RecordType_RecordId"" ON public.hrd_clinical_peer_review_audit_logs (""RecordType"", ""RecordId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_hrd_training_bond_contracts_ContractNumber"" ON public.hrd_training_bond_contracts (""ContractNumber"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_hrd_training_bond_contracts_WorkforceProfileId"" ON public.hrd_training_bond_contracts (""WorkforceProfileId"");
CREATE UNIQUE INDEX IF NOT EXISTS uq_benefit_grade_plan ON public.""HrdBenefitGradeEligibility"" (""BenefitPlanId"", ""GradeLevelCode"", ""EmploymentTypeCode"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProfileChangeHistory_ApprovedByUserId"" ON public.""HrdProfileChangeHistory"" (""ApprovedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProfileChangeHistory_EmployeeId"" ON public.""HrdProfileChangeHistory"" (""EmployeeId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProfileChangeHistory_EmployeeId_ComponentModified_Applie~"" ON public.""HrdProfileChangeHistory"" (""EmployeeId"", ""ComponentModified"", ""AppliedAtUtc"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProfileChangeHistory_HrServiceRequestId"" ON public.""HrdProfileChangeHistory"" (""HrServiceRequestId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_mst_oppe_metric_catalogs_MetricCode"" ON public.mst_oppe_metric_catalogs (""MetricCode"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_MstCredentialingCommittee_ChairpersonWorkforceProfileId"" ON public.""MstCredentialingCommittee"" (""ChairpersonWorkforceProfileId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MstCredentialingCommittee_CommitteeCode"" ON public.""MstCredentialingCommittee"" (""CommitteeCode"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_MstCredentialingCommittee_SecretaryWorkforceProfileId"" ON public.""MstCredentialingCommittee"" (""SecretaryWorkforceProfileId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_fppe_case_logs_FppeEngagementId"" ON public.trx_fppe_case_logs (""FppeEngagementId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_fppe_case_logs_TrxFppeEngagementId"" ON public.trx_fppe_case_logs (""TrxFppeEngagementId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_trx_fppe_engagements_EngagementNumber"" ON public.trx_fppe_engagements (""EngagementNumber"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_trx_fppe_evaluation_conclusions_FppeEngagementId"" ON public.trx_fppe_evaluation_conclusions (""FppeEngagementId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_trx_oppe_evaluation_periods_PeriodCode"" ON public.trx_oppe_evaluation_periods (""PeriodCode"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_trx_oppe_staff_evaluation_details_MetricCatalogId"" ON public.trx_oppe_staff_evaluation_details (""MetricCatalogId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_oppe_staff_evaluation_details_StaffEvaluationId"" ON public.trx_oppe_staff_evaluation_details (""StaffEvaluationId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_oppe_staff_evaluation_details_TrxOppeStaffEvaluationId"" ON public.trx_oppe_staff_evaluation_details (""TrxOppeStaffEvaluationId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_oppe_staff_evaluations_PeriodId"" ON public.trx_oppe_staff_evaluations (""PeriodId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_oppe_staff_evaluations_WorkforceProfileId"" ON public.trx_oppe_staff_evaluations (""WorkforceProfileId"");
CREATE INDEX IF NOT EXISTS ""IX_trx_plataran_sehat_skp_records_WorkforceProfileId"" ON public.trx_plataran_sehat_skp_records (""WorkforceProfileId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxClinicalPrivilegeEmergencyOverrideLog_PrivilegeItemCode"" ON public.""TrxClinicalPrivilegeEmergencyOverrideLog"" (""PrivilegeItemCode"");
CREATE INDEX IF NOT EXISTS ""IX_TrxClinicalPrivilegeEmergencyOverrideLog_ReviewedByUserId"" ON public.""TrxClinicalPrivilegeEmergencyOverrideLog"" (""ReviewedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxClinicalPrivilegeEmergencyOverrideLog_WitnessUserId"" ON public.""TrxClinicalPrivilegeEmergencyOverrideLog"" (""WitnessUserId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxClinicalPrivilegeEmergencyOverrideLog_WorkforceProfileId"" ON public.""TrxClinicalPrivilegeEmergencyOverrideLog"" (""WorkforceProfileId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_CommitteeId"" ON public.""TrxSpkRkkIssuance"" (""CommitteeId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_CredentialingApplicationId"" ON public.""TrxSpkRkkIssuance"" (""CredentialingApplicationId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_DirectorGeneralUserId"" ON public.""TrxSpkRkkIssuance"" (""DirectorGeneralUserId"");
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_RkkNumber"" ON public.""TrxSpkRkkIssuance"" (""RkkNumber"") WHERE ""IsDelete"" = false;
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_SpkNumber"" ON public.""TrxSpkRkkIssuance"" (""SpkNumber"") WHERE ""IsDelete"" = false;
CREATE INDEX IF NOT EXISTS ""IX_TrxSpkRkkIssuance_WorkforceProfileId"" ON public.""TrxSpkRkkIssuance"" (""WorkforceProfileId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS public.""TrxSpkRkkIssuance"";
                DROP TABLE IF EXISTS public.""TrxClinicalPrivilegeEmergencyOverrideLog"";
                DROP TABLE IF EXISTS public.""trx_plataran_sehat_skp_records"";
                DROP TABLE IF EXISTS public.""MstCredentialingCommittee"";
                DROP TABLE IF EXISTS public.""HrdProfileChangeHistory"";
                DROP TABLE IF EXISTS public.""hrd_training_bond_contracts"";
                DROP TABLE IF EXISTS public.""HrdBenefitGradeEligibility"";
                DROP TABLE IF EXISTS public.""trx_oppe_staff_evaluation_details"";
                DROP TABLE IF EXISTS public.""trx_oppe_staff_evaluations"";
                DROP TABLE IF EXISTS public.""trx_fppe_evaluation_conclusions"";
                DROP TABLE IF EXISTS public.""trx_fppe_case_logs"";
                DROP TABLE IF EXISTS public.""trx_oppe_evaluation_periods"";
                DROP TABLE IF EXISTS public.""trx_fppe_engagements"";
                DROP TABLE IF EXISTS public.""mst_oppe_metric_catalogs"";
                DROP TABLE IF EXISTS public.""hrd_clinical_peer_review_audit_logs"";
            ");
        }
    }
}
