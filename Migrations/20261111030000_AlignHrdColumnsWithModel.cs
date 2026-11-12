using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menyelaraskan tabel HR dengan model EF: menambah kolom yang ada di model tetapi tidak pernah dibuat migration,
    /// beserta index dan foreign key-nya (ditemukan 11 November 2026 lewat perbandingan model dan database).
    /// </summary>
    /// <remarks>
    /// Hanya menambah, memakai <c>IF NOT EXISTS</c>. Kolom wajib diberi nilai bawaan supaya aman pada database yang tabelnya
    /// sudah berisi. Foreign key pada kolom wajib sengaja tidak dibuat, karena nilai bawaan tidak merujuk baris mana pun.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261111030000_AlignHrdColumnsWithModel")]
    public partial class AlignHrdColumnsWithModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE public.""trx_bipartite_negotiation_forums"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_bipartite_negotiation_forums"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_bipartite_negotiation_forums"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_disciplinary_incident_reports"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_disciplinary_incident_reports"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_disciplinary_incident_reports"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_exit_clearance_processes"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_exit_clearance_processes"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_exit_clearance_processes"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_severance_calculations"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_severance_calculations"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_severance_calculations"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_disciplinary_investigation_records"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_disciplinary_investigation_records"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_disciplinary_investigation_records"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_exit_clearance_checklists"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_exit_clearance_checklists"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_exit_clearance_checklists"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_disciplinary_sanctions"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_disciplinary_sanctions"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_disciplinary_sanctions"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""trx_disciplinary_suspensions"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""trx_disciplinary_suspensions"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""trx_disciplinary_suspensions"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""HrdEmploymentLifecycleSetting"" ADD COLUMN IF NOT EXISTS ""ContractReminderDays"" integer NOT NULL DEFAULT 60;
ALTER TABLE public.""HrdEmploymentLifecycleSetting"" ADD COLUMN IF NOT EXISTS ""ContractEscalationDays"" integer NOT NULL DEFAULT 14;
ALTER TABLE public.""HrdEmploymentLifecycleSetting"" ADD COLUMN IF NOT EXISTS ""ProbationReminderDays"" integer NOT NULL DEFAULT 14;
ALTER TABLE public.""HrdEmploymentLifecycleSetting"" ADD COLUMN IF NOT EXISTS ""LateStartFlagDays"" integer NOT NULL DEFAULT 3;
ALTER TABLE public.""HrdPayrollSetting"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""HrdPayrollSetting"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdPayrollSetting"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""MstBenefitPlan"" ADD COLUMN IF NOT EXISTS ""BenefitCategory"" character varying(50) NOT NULL DEFAULT 'BPJS_Mandatory';
ALTER TABLE public.""MstBenefitPlan"" ADD COLUMN IF NOT EXISTS ""CoverageTier"" character varying(50) NOT NULL DEFAULT 'Basic';
ALTER TABLE public.""MstBenefitPlan"" ADD COLUMN IF NOT EXISTS ""HospitalDiscountPercentage"" numeric(5,2) NOT NULL DEFAULT 0.0;
ALTER TABLE public.""MstBenefitPlan"" ADD COLUMN IF NOT EXISTS ""RequiresBPJSPrimaryCOB"" boolean NOT NULL DEFAULT TRUE;
ALTER TABLE public.""MstBenefitPlan"" ADD COLUMN IF NOT EXISTS ""MinimumServiceMonths"" integer NOT NULL DEFAULT 3;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""ValidityPeriodDays"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""NumberingPattern"" character varying(100) NOT NULL DEFAULT '';
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""LastSequenceNumber"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""CurrentYear"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""TemplateBodyHtml"" text;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""SupportedTokensJson"" text;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""IsFinancialSensitive"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""RequiresCbApproval"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""MstEmployeeDocumentType"" ADD COLUMN IF NOT EXISTS ""AuthorizedSignerTitle"" character varying(150) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""TotalRows"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""ValidRows"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""InvalidRows"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""SubmittedByUserId"" uuid;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""SubmittedAt"" timestamp with time zone;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""DecidedByUserId"" uuid;
ALTER TABLE public.""HrdInitialContractBatch"" ADD COLUMN IF NOT EXISTS ""DecidedAt"" timestamp with time zone;
ALTER TABLE public.""TrxPayrollHandoffBatch"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""TrxPayrollHandoffBatch"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""TrxPayrollHandoffBatch"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""HrdBenefitBalanceMovement"" ADD COLUMN IF NOT EXISTS ""WorkforceProfileId"" uuid;
ALTER TABLE public.""HrdBenefitBalanceMovement"" ADD COLUMN IF NOT EXISTS ""BenefitPlanId"" uuid;
ALTER TABLE public.""HrdBenefitBalanceMovement"" ADD COLUMN IF NOT EXISTS ""ReferenceNumber"" character varying(100);
ALTER TABLE public.""HrdBenefitBalanceMovement"" ADD COLUMN IF NOT EXISTS ""Description"" character varying(1000);
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""AccompaniedSince"" date;
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""IndependentOn"" date;
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""DeclaredByUserId"" uuid;
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""EvidenceStoragePath"" character varying(500);
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""EvidenceFileName"" character varying(250);
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""EvidenceContentType"" character varying(150);
ALTER TABLE public.""HrdClinicalPreceptorship"" ADD COLUMN IF NOT EXISTS ""AssessmentNote"" character varying(2000);
ALTER TABLE public.""HrdClinicalPreceptorshipPreceptor"" ADD COLUMN IF NOT EXISTS ""AssignedUntil"" date;
ALTER TABLE public.""HrdClinicalPreceptorshipPreceptor"" ADD COLUMN IF NOT EXISTS ""AssignedByUserId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdClinicalPreceptorshipPreceptor"" ADD COLUMN IF NOT EXISTS ""ChangeReason"" character varying(500);
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""BeforeContractNumber"" character varying(100) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""BeforeContractTypeId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""BeforeStartDate"" date NOT NULL DEFAULT CURRENT_DATE;
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""BeforeEndDate"" date;
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""BeforeDocumentPath"" character varying(500);
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""AfterContractNumber"" character varying(100) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""AfterContractTypeId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""AfterStartDate"" date NOT NULL DEFAULT CURRENT_DATE;
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""AfterEndDate"" date;
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""ReasonText"" character varying(2000) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""EvidenceStoragePath"" character varying(500) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""EvidenceFileName"" character varying(250) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""EvidenceContentType"" character varying(150) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""SubmittedByUserId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""SubmittedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""DecisionNote"" character varying(1000);
ALTER TABLE public.""HrdContractCorrection"" ADD COLUMN IF NOT EXISTS ""AppliedAt"" timestamp with time zone;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""ExtensionMonths"" integer;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""ReasonText"" character varying(2000);
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""SubmittedByUserId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""SubmittedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""DecisionNote"" character varying(1000);
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""FollowUpContractHistoryId"" uuid;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""EmployeeNotifiedOn"" date;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""NotificationRecordedByUserId"" uuid;
ALTER TABLE public.""HrdContractDecision"" ADD COLUMN IF NOT EXISTS ""EmployeeSeparationId"" uuid;
ALTER TABLE public.""HrdInitialContractBatchRow"" ADD COLUMN IF NOT EXISTS ""ValidationMessage"" character varying(500);
ALTER TABLE public.""HrdInitialContractBatchRow"" ADD COLUMN IF NOT EXISTS ""CreatedContractHistoryId"" uuid;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""StartDate"" date NOT NULL DEFAULT CURRENT_DATE;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""EndDate"" date NOT NULL DEFAULT CURRENT_DATE;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""ReminderFromDate"" date NOT NULL DEFAULT CURRENT_DATE;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""ProbationStatus"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""ClosedAt"" timestamp with time zone;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""ClosedByUserId"" uuid;
ALTER TABLE public.""HrdProbation"" ADD COLUMN IF NOT EXISTS ""EmployeeSeparationId"" uuid;
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""Result"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""ReasonText"" character varying(2000) NOT NULL DEFAULT '';
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""EvidenceStoragePath"" character varying(500);
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""EvidenceFileName"" character varying(250);
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""EvidenceContentType"" character varying(150);
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""SubmittedByUserId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""SubmittedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""DecidedByUserId"" uuid;
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""DecidedAt"" timestamp with time zone;
ALTER TABLE public.""HrdProbationAssessment"" ADD COLUMN IF NOT EXISTS ""DecisionNote"" character varying(1000);
ALTER TABLE public.""TrxEmployeeSeparation"" ADD COLUMN IF NOT EXISTS ""NoticeDate"" timestamp with time zone;
ALTER TABLE public.""TrxEmployeeSeparation"" ADD COLUMN IF NOT EXISTS ""ResignationLetterUrl"" character varying(512);
ALTER TABLE public.""TrxEmployeeSeparation"" ADD COLUMN IF NOT EXISTS ""TerminationDecisionNumber"" character varying(128);
ALTER TABLE public.""TrxEmployeeSeparation"" ADD COLUMN IF NOT EXISTS ""ApprovalStatus"" character varying(32) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxPayrollVarianceAlert"" ADD COLUMN IF NOT EXISTS ""CancelDateTime"" timestamp with time zone;
ALTER TABLE public.""TrxPayrollVarianceAlert"" ADD COLUMN IF NOT EXISTS ""CancelBy"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE public.""TrxPayrollVarianceAlert"" ADD COLUMN IF NOT EXISTS ""IsCancel"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""IsLifetimeStr"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""IsPsvVerified"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""PsvVerifiedAt"" timestamp with time zone;
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""PsvVerificationUrl"" character varying(500);
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""PsvVerificationSource"" character varying(100);
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""PsvAuditLogJson"" text;
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""SipFaskesPracticeNumber"" character varying(10);
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""SipFaskesName"" character varying(200);
ALTER TABLE public.""WfpCredentialLicense"" ADD COLUMN IF NOT EXISTS ""ExpiryRadarStatus"" character varying(30) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxCredentialingApplication"" ADD COLUMN IF NOT EXISTS ""MstCredentialingCommitteeId"" uuid;
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""IsWhistleblowerCase"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""IsAnonymous"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""EncryptedReportPayload"" text;
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""Category"" character varying(64);
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""TargetUnit"" character varying(128);
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""SeverityLevel"" character varying(32) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""TargetResolutionDate"" timestamp with time zone;
ALTER TABLE public.""TrxEmployeeGrievance"" ADD COLUMN IF NOT EXISTS ""ResolutionNotes"" text;
ALTER TABLE public.""TrxTrainingEnrollmentRequest"" ADD COLUMN IF NOT EXISTS ""TrxTrainingSessionId"" uuid;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""SepBpjsNumber"" character varying(100);
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""ServiceType"" character varying(50);
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""HospitalName"" character varying(150);
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""TotalHospitalBill"" numeric NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""PrimaryPayerCoveredAmount"" numeric NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""SecondaryClaimAmount"" numeric NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""OutOfPocketExcessAmount"" numeric NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""DependentId"" uuid;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""SettledAt"" timestamp with time zone;
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""SettlementReference"" character varying(100);
ALTER TABLE public.""TrxBenefitClaim"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" character varying(1000);
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""NationalIdNumber"" character varying(16);
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""FamilyCardNumber"" character varying(16);
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""IsActiveStudent"" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""StudentVerificationLetterPath"" character varying(255);
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""StudentVerificationExpiryDate"" timestamp with time zone;
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""VerificationStatus"" character varying(50);
ALTER TABLE public.""TrxEmployeeBenefitDependent"" ADD COLUMN IF NOT EXISTS ""RejectionReason"" character varying(1000);
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""BpjsKesehatanNumber"" character varying(50);
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""BpjsKetenagakerjaanNumber"" character varying(50);
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""PrivateInsurancePolicyNumber"" character varying(100);
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""ProbationEvaluationId"" uuid;
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""TerminationDate"" timestamp with time zone;
ALTER TABLE public.""TrxEmployeeInsuranceEnrollment"" ADD COLUMN IF NOT EXISTS ""TerminationReason"" character varying(255);
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""PriorityTier"" character varying(30) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""SlaTargetHours"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""SlaDeadlineUtc"" timestamp with time zone;
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""SlaStatus"" character varying(30) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""SlaPausedAtUtc"" timestamp with time zone;
ALTER TABLE public.""TrxHrServiceRequest"" ADD COLUMN IF NOT EXISTS ""TotalPausedMinutes"" integer NOT NULL DEFAULT 0;
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""OfficialDocumentNumber"" character varying(100) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""RevocationBapNumber"" character varying(100);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SignerName"" character varying(150);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SignerTitle"" character varying(150);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SignerSignatureHash"" character varying(256);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""VerificationHash"" character varying(64);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""QrCodeImagePath"" character varying(500);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""PdfFilePath"" character varying(500);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""IsFinancialSensitive"" boolean NOT NULL DEFAULT false;
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SecurityPinHash"" character varying(255);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SalaryBrutoAmount"" numeric(18,2);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""SalaryNettoAmount"" numeric(18,2);
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""PayrollPayslipId"" uuid;
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""IssuanceStatus"" character varying(30) NOT NULL DEFAULT '';
ALTER TABLE public.""TrxEmployeeDocumentIssuance"" ADD COLUMN IF NOT EXISTS ""EDossierDocumentId"" uuid;
CREATE INDEX IF NOT EXISTS ""IX_HrdClinicalPreceptorship_DeclaredByUserId"" ON public.""HrdClinicalPreceptorship"" (""DeclaredByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdClinicalPreceptorshipPreceptor_AssignedByUserId"" ON public.""HrdClinicalPreceptorshipPreceptor"" (""AssignedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractCorrection_AfterContractTypeId"" ON public.""HrdContractCorrection"" (""AfterContractTypeId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractCorrection_BeforeContractTypeId"" ON public.""HrdContractCorrection"" (""BeforeContractTypeId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractCorrection_SubmittedByUserId"" ON public.""HrdContractCorrection"" (""SubmittedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractDecision_FollowUpContractHistoryId"" ON public.""HrdContractDecision"" (""FollowUpContractHistoryId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractDecision_NotificationRecordedByUserId"" ON public.""HrdContractDecision"" (""NotificationRecordedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdContractDecision_SubmittedByUserId"" ON public.""HrdContractDecision"" (""SubmittedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdInitialContractBatch_DecidedByUserId"" ON public.""HrdInitialContractBatch"" (""DecidedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdInitialContractBatch_SubmittedByUserId"" ON public.""HrdInitialContractBatch"" (""SubmittedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdInitialContractBatchRow_CreatedContractHistoryId"" ON public.""HrdInitialContractBatchRow"" (""CreatedContractHistoryId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProbation_ClosedByUserId"" ON public.""HrdProbation"" (""ClosedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProbation_Status_EndDate"" ON public.""HrdProbation"" (""ProbationStatus"", ""EndDate"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProbationAssessment_DecidedByUserId"" ON public.""HrdProbationAssessment"" (""DecidedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_HrdProbationAssessment_SubmittedByUserId"" ON public.""HrdProbationAssessment"" (""SubmittedByUserId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxCredentialingApplication_MstCredentialingCommitteeId"" ON public.""TrxCredentialingApplication"" (""MstCredentialingCommitteeId"");
CREATE INDEX IF NOT EXISTS ""IX_TrxTrainingEnrollmentRequest_TrxTrainingSessionId"" ON public.""TrxTrainingEnrollmentRequest"" (""TrxTrainingSessionId"");
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdContractDecision_WfpContractHistory_FollowUpContractHist~') THEN ALTER TABLE public.""HrdContractDecision"" ADD CONSTRAINT ""FK_HrdContractDecision_WfpContractHistory_FollowUpContractHist~"" FOREIGN KEY (""FollowUpContractHistoryId"") REFERENCES public.""WfpContractHistory"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdInitialContractBatchRow_WfpContractHistory_CreatedContra~') THEN ALTER TABLE public.""HrdInitialContractBatchRow"" ADD CONSTRAINT ""FK_HrdInitialContractBatchRow_WfpContractHistory_CreatedContra~"" FOREIGN KEY (""CreatedContractHistoryId"") REFERENCES public.""WfpContractHistory"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_hrd_training_bond_contracts_MstWorkforceProfile_WorkforcePr~') THEN ALTER TABLE public.hrd_training_bond_contracts ADD CONSTRAINT ""FK_hrd_training_bond_contracts_MstWorkforceProfile_WorkforcePr~"" FOREIGN KEY (""WorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdProfileChangeHistory_MstWorkforceProfile_EmployeeId') THEN ALTER TABLE public.""HrdProfileChangeHistory"" ADD CONSTRAINT ""FK_HrdProfileChangeHistory_MstWorkforceProfile_EmployeeId"" FOREIGN KEY (""EmployeeId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdProfileChangeHistory_TrxHrServiceRequest_HrServiceReques~') THEN ALTER TABLE public.""HrdProfileChangeHistory"" ADD CONSTRAINT ""FK_HrdProfileChangeHistory_TrxHrServiceRequest_HrServiceReques~"" FOREIGN KEY (""HrServiceRequestId"") REFERENCES public.""TrxHrServiceRequest"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstCredentialingCommittee_MstWorkforceProfile_ChairpersonWo~') THEN ALTER TABLE public.""MstCredentialingCommittee"" ADD CONSTRAINT ""FK_MstCredentialingCommittee_MstWorkforceProfile_ChairpersonWo~"" FOREIGN KEY (""ChairpersonWorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_MstCredentialingCommittee_MstWorkforceProfile_SecretaryWork~') THEN ALTER TABLE public.""MstCredentialingCommittee"" ADD CONSTRAINT ""FK_MstCredentialingCommittee_MstWorkforceProfile_SecretaryWork~"" FOREIGN KEY (""SecretaryWorkforceProfileId"") REFERENCES public.""MstWorkforceProfile"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdInitialContractBatch_AspNetUsers_DecidedByUserId') THEN ALTER TABLE public.""HrdInitialContractBatch"" ADD CONSTRAINT ""FK_HrdInitialContractBatch_AspNetUsers_DecidedByUserId"" FOREIGN KEY (""DecidedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdInitialContractBatch_AspNetUsers_SubmittedByUserId') THEN ALTER TABLE public.""HrdInitialContractBatch"" ADD CONSTRAINT ""FK_HrdInitialContractBatch_AspNetUsers_SubmittedByUserId"" FOREIGN KEY (""SubmittedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdClinicalPreceptorship_AspNetUsers_DeclaredByUserId') THEN ALTER TABLE public.""HrdClinicalPreceptorship"" ADD CONSTRAINT ""FK_HrdClinicalPreceptorship_AspNetUsers_DeclaredByUserId"" FOREIGN KEY (""DeclaredByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdContractDecision_AspNetUsers_NotificationRecordedByUserId') THEN ALTER TABLE public.""HrdContractDecision"" ADD CONSTRAINT ""FK_HrdContractDecision_AspNetUsers_NotificationRecordedByUserId"" FOREIGN KEY (""NotificationRecordedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdProbation_AspNetUsers_ClosedByUserId') THEN ALTER TABLE public.""HrdProbation"" ADD CONSTRAINT ""FK_HrdProbation_AspNetUsers_ClosedByUserId"" FOREIGN KEY (""ClosedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_HrdProbationAssessment_AspNetUsers_DecidedByUserId') THEN ALTER TABLE public.""HrdProbationAssessment"" ADD CONSTRAINT ""FK_HrdProbationAssessment_AspNetUsers_DecidedByUserId"" FOREIGN KEY (""DecidedByUserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE RESTRICT; END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_TrxCredentialingApplication_MstCredentialingCommittee_MstCr~') THEN ALTER TABLE public.""TrxCredentialingApplication"" ADD CONSTRAINT ""FK_TrxCredentialingApplication_MstCredentialingCommittee_MstCr~"" FOREIGN KEY (""MstCredentialingCommitteeId"") REFERENCES public.""MstCredentialingCommittee"" (""Id""); END IF; END $$;
DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_TrxTrainingEnrollmentRequest_TrxTrainingSession_TrxTraining~') THEN ALTER TABLE public.""TrxTrainingEnrollmentRequest"" ADD CONSTRAINT ""FK_TrxTrainingEnrollmentRequest_TrxTrainingSession_TrxTraining~"" FOREIGN KEY (""TrxTrainingSessionId"") REFERENCES public.""TrxTrainingSession"" (""Id""); END IF; END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sengaja kosong: kolom yang ditambahkan dipakai model EF; menghapusnya akan merusak pembacaan entity.
        }
    }
}
