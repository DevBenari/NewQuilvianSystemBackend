using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddHrdDurableAuditAndPinLockouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstEmployeeCategory_EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstEmploymentType_EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstPosition_PositionId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstWorkflowDefinition_WorkflowDefinit~",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplate_MstWorkforceType_WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstOrganizationUnit_ResponsibleOr~",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstPosition_ResponsiblePositionId",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxCalibrationSession_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxCalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxEmployeeGoal_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxEmployeeKpiTarget_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeKpiTarget");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxIndividualDevelopmentPlan_TrxPerformanceCycle_Performanc~",
                schema: "public",
                table: "TrxIndividualDevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxManagerAssessment_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxManagerAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPeerFeedback_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxPeerFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCheckIn_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxPerformanceCheckIn");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_AspNetUsers_PublishedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstDepartment_DepartmentId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstOrganizationUnit_OrganizationUnitId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceCycle_MasterPerformanceCy~",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceRatingScale_RatingScaleId",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceTemplate_PerformanceTempl~",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCycle_MstWorkflowDefinition_WorkflowDefinitio~",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_ApprovedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_ManagerUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_SubmittedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstEmployee_EmployeeId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstWorkflowDefinition_Workflo~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstWorkforceProfile_Workforce~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_TrxPerformanceCycle_Performan~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_WfpPerformanceReview_Performa~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxSelfAssessment_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxSelfAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpContractHistory_MstWorkforceProfile_MstWorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpPerformanceReview_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "WfpPerformanceReview");

            migrationBuilder.DropTable(
                name: "TrxBackgroundCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateScreening",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxContractNonRenewal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmployeeOnboardingTask",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxInterviewEvaluation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxJobRequisitionApproval",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPreEmploymentMedicalCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxProbationReview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxReferenceCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOnboardingTask",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssessmentMethod",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateInterview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmployeeOnboarding",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpOnboardingChecklist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstInterviewTemplate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateHiring",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxJobOffer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidateApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstRecruitmentStage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCandidate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxJobVacancy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCandidateStatus",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstRecruitmentSource",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxJobRequisition",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpContractHistory_MstWorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropIndex(
                name: "IX_TrxOvertimePlan_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimePlan");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplateTask_OnboardingTemplateId_SortOrder_Is~",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplateTask_ResponsibleOrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_LegalEntityId_HospitalSiteId_Template~",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_OrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_PositionId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_TemplateCode_Version",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_WorkflowDefinitionId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrxPerformanceImprovementPlan",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_ApprovedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_EmployeeId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_ManagerUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_PerformanceReviewId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_SubmittedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_WorkflowDefinitionId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceImprovementPlan_WorkforceProfileId_PipStatus_~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrxPerformanceCycle",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceCycle_CycleStartDate_CycleEndDate",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropIndex(
                name: "IX_TrxPerformanceCycle_CycleYear_CycleStatus",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropColumn(
                name: "CompletionSource",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "RequiresDocument",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "RequiresVerification",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "ResponsibleOrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "ResponsiblePartyType",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "AutoCreateChecklist",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "EffectiveEndDate",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "EffectiveStartDate",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "LegalEntityId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "PositionId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "ProbationReviewAfterDays",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "RequiresProbationReview",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "PipStatus",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "public",
                table: "TrxPerformanceImprovementPlan");

            migrationBuilder.DropColumn(
                name: "CycleName",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.DropColumn(
                name: "CycleStatus",
                schema: "public",
                table: "TrxPerformanceCycle");

            migrationBuilder.RenameTable(
                name: "TrxPerformanceImprovementPlan",
                schema: "public",
                newName: "trx_performance_improvement_plans",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "TrxPerformanceCycle",
                schema: "public",
                newName: "trx_performance_cycles",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "MstWorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory",
                newName: "SourceDocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxOvertimePeriod_StartDate_EndDate_PeriodStatus",
                schema: "public",
                table: "TrxOvertimePeriod",
                newName: "idx_ot_period_lookup");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "ResponsibleRole");

            migrationBuilder.RenameColumn(
                name: "ResponsiblePositionId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "CertificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_MstOnboardingTemplateTask_ResponsiblePositionId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "IX_MstOnboardingTemplateTask_CertificationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_MstOnboardingTemplateTask_OnboardingTemplateId_TaskCode",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "IX_MstOnboardingTemplateTask_TemplateId_TaskCode");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceImprovementPlan_PipNumber",
                schema: "public",
                table: "trx_performance_improvement_plans",
                newName: "IX_trx_performance_improvement_plans_PipNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceImprovementPlan_PerformanceCycleId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                newName: "IX_trx_performance_improvement_plans_PerformanceCycleId");

            migrationBuilder.RenameColumn(
                name: "CycleYear",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "EvaluationYear");

            migrationBuilder.RenameColumn(
                name: "CycleStartDate",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "CycleEndDate",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "SelfAppraisalDeadline");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_WorkflowDefinitionId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_WorkflowDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_RatingScaleId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_PublishedByUserId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_PerformanceTemplateId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_PerformanceTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_OrganizationUnitId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_MasterPerformanceCycleId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_MasterPerformanceCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_LegalEntityId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_LegalEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_HospitalSiteId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_DepartmentId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_CycleCode",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_CycleCode");

            migrationBuilder.RenameIndex(
                name: "IX_TrxPerformanceCycle_ClosedByUserId",
                schema: "public",
                table: "trx_performance_cycles",
                newName: "IX_trx_performance_cycles_ClosedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "CoverageMitigationNotes",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverageWarningDetails",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorNoteFaskesName",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorSipNumber",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasStaffingCoverageWarning",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDoctorNoteVerified",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayrollClosed",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPendingPayrollReconciliation",
                schema: "public",
                table: "WfpLeaveRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryRadarStatus",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsLifetimeStr",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPsvVerified",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PsvAuditLogJson",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsvVerificationSource",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsvVerificationUrl",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PsvVerifiedAt",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SipFaskesName",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SipFaskesPracticeNumber",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentContentType",
                schema: "public",
                table: "WfpContractHistory",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentFileName",
                schema: "public",
                table: "WfpContractHistory",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FixedTermSeriesStartDate",
                schema: "public",
                table: "WfpContractHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                schema: "public",
                table: "WfpContractHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedRecordedAt",
                schema: "public",
                table: "WfpContractHistory",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignedRecordedByUserId",
                schema: "public",
                table: "WfpContractHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocumentType",
                schema: "public",
                table: "WfpContractHistory",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelegatedAction",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverdue",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OnBehalfOfEmployeeId",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityLevel",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaDeadline",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSteps",
                schema: "public",
                table: "TrxWorkflowInstance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ApproverEmployeeId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DelegatedEmployeeId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DefaultedAt",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverdue",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentOrRefundRef",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptDocUrl",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundVoucherNumber",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                schema: "public",
                table: "TrxTravelSettlement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMissingReceiptDeclared",
                schema: "public",
                table: "TrxTravelExpenseItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MissingReceiptApprovedBy",
                schema: "public",
                table: "TrxTravelExpenseItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissingReceiptReason",
                schema: "public",
                table: "TrxTravelExpenseItem",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrxTrainingSessionId",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutedAt",
                schema: "public",
                table: "TrxShiftReplacement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "public",
                table: "TrxShiftReplacement",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonCategory",
                schema: "public",
                table: "TrxShiftReplacement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Sakit Mendadak");

            migrationBuilder.AddColumn<Guid>(
                name: "RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsManualOverride",
                schema: "public",
                table: "TrxShiftAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccompanied",
                schema: "public",
                table: "TrxShiftAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                schema: "public",
                table: "TrxRosterPeriod",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecheckPassed",
                schema: "public",
                table: "TrxRosterPeriod",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WarningCount",
                schema: "public",
                table: "TrxRosterPeriod",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BiometricCheckIn",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BiometricCheckOut",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BiometricExcessMinutes",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DiscrepancyReason",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualAdjustmentNotes",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByHrAdminId",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedPayableMinutes",
                schema: "public",
                table: "TrxOvertimeVerification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalActionSummary",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompensationType",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "FinancialPayout");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyClinicalReferenceNo",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMedicalDirectorEscalation",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetrospectiveAdjustment",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalDirectorApprovedById",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OvertimeCategory",
                schema: "public",
                table: "TrxOvertimeRealization",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Planned");

            migrationBuilder.AddColumn<string>(
                name: "CompensationType",
                schema: "public",
                table: "TrxOvertimePlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "FinancialPayout");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyEscalation",
                schema: "public",
                table: "TrxOvertimePlan",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyQuotaHoursAtPlan",
                schema: "public",
                table: "TrxOvertimePlan",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CutoffDate",
                schema: "public",
                table: "TrxOvertimePeriod",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHandoffExecuted",
                schema: "public",
                table: "TrxOvertimePeriod",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCtoGrantedHours",
                schema: "public",
                table: "TrxOvertimePeriod",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "ExpectedResponseMinutes",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "integer",
                nullable: true,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalTime",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallInTime",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallNotes",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCalledIn",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSlaMet",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseMinutes",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorSipNumber",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaskesName",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfidentialMedical",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMedicalCertificate",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicalDiagnosisIcd10",
                schema: "public",
                table: "TrxLeaveRequestAttachment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditDocumentNumber",
                schema: "public",
                table: "TrxLeaveAdjustment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityTier",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaDeadlineUtc",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaPausedAtUtc",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaStatus",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SlaTargetHours",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPausedMinutes",
                schema: "public",
                table: "TrxHrServiceRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                schema: "public",
                table: "TrxEmployeeSeparation",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NoticeDate",
                schema: "public",
                table: "TrxEmployeeSeparation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResignationLetterUrl",
                schema: "public",
                table: "TrxEmployeeSeparation",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationDecisionNumber",
                schema: "public",
                table: "TrxEmployeeSeparation",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BpjsKesehatanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateInsurancePolicyNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProbationEvaluationId",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationReason",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MeasuredDoseMsv",
                schema: "public",
                table: "TrxEmployeeHealthSurveillance",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedReportPayload",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhistleblowerCase",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeverityLevel",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetResolutionDate",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUnit",
                schema: "public",
                table: "TrxEmployeeGrievance",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EDossierDocumentId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IssuanceStatus",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficialDocumentNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollPayslipId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImagePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevocationBapNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryBrutoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryNettoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityPinHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerName",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerSignatureHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerTitle",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyCardNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveStudent",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StudentVerificationExpiryDate",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentVerificationLetterPath",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstCredentialingCommitteeId",
                schema: "public",
                table: "TrxCredentialingApplication",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrantedDays",
                schema: "public",
                table: "TrxCompensatoryTimeOff",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                schema: "public",
                table: "TrxCompensatoryTimeOff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingHours",
                schema: "public",
                table: "TrxCompensatoryTimeOff",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DependentId",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OutOfPocketExcessAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryPayerCoveredAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryClaimAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SepBpjsNumber",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementReference",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHospitalBill",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DelegateEmployeeId",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegatedRequestTypes",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrincipalEmployeeId",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "public",
                table: "TrxApprovalDelegation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActionEmployeeId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DigitalSignatureHash",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OnBehalfOfEmployeeId",
                schema: "public",
                table: "TrxApprovalAction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonComment",
                schema: "public",
                table: "TrxApprovalAction",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaskCategory",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "DueDayOffset",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AutomaticSource",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemKind",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SystemItemCode",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TargetCompletionDays",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TemplateLayer",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedSignerTitle",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CurrentYear",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastSequenceNumber",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NumberingPattern",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCbApproval",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupportedTokensJson",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateBodyHtml",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValidityPeriodDays",
                schema: "public",
                table: "MstEmployeeDocumentType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "EmploymentTypeId",
                schema: "public",
                table: "MstContractType",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenefitCategory",
                schema: "public",
                table: "MstBenefitPlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BPJS_Mandatory");

            migrationBuilder.AddColumn<string>(
                name: "CoverageTier",
                schema: "public",
                table: "MstBenefitPlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Basic");

            migrationBuilder.AddColumn<decimal>(
                name: "HospitalDiscountPercentage",
                schema: "public",
                table: "MstBenefitPlan",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<int>(
                name: "MinimumServiceMonths",
                schema: "public",
                table: "MstBenefitPlan",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBPJSPrimaryCOB",
                schema: "public",
                table: "MstBenefitPlan",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitPlanId",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EarlyLeavePermissionId",
                schema: "public",
                table: "HrdAttendanceDaily",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncompletePunchReason",
                schema: "public",
                table: "HrdAttendanceDaily",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficialDutyRequestId",
                schema: "public",
                table: "HrdAttendanceDaily",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PipNumber",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<Guid>(
                name: "PerformanceCycleId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerUserId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEmployeeAcknowledged",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CheckInFrequencyDays",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "ActionPlanSummary",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FinalVerdict",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MentorWorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "VerdictNotes",
                schema: "public",
                table: "trx_performance_improvement_plans",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CycleCode",
                schema: "public",
                table: "trx_performance_cycles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CalibrationDeadline",
                schema: "public",
                table: "trx_performance_cycles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CycleTitle",
                schema: "public",
                table: "trx_performance_cycles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CycleType",
                schema: "public",
                table: "trx_performance_cycles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "public",
                table: "trx_performance_cycles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerAppraisalDeadline",
                schema: "public",
                table: "trx_performance_cycles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "trx_performance_cycles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trx_performance_improvement_plans",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trx_performance_cycles",
                schema: "public",
                table: "trx_performance_cycles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "hrd_clinical_peer_review_audit_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrd_clinical_peer_review_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrd_training_bond_contracts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingEnrollmentRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingProgramName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TotalTrainingCost = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    HospitalFinancedAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    RetentionPeriodMonths = table.Column<int>(type: "integer", nullable: false),
                    BondStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BondEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BondStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsBreached = table.Column<bool>(type: "boolean", nullable: false),
                    BreachRecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalculatedPenaltyAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    BreachReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PenaltyInvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DigitalSignatureHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrd_training_bond_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrd_training_bond_contracts_MstWorkforceProfile_WorkforcePr~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdAttendancePeriodReopenLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendancePeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReopenReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreviousClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRollbackPayrollExecuted = table.Column<bool>(type: "boolean", nullable: false),
                    RollbackPayrollNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdAttendancePeriodReopenLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdAttendancePeriodReopenLog_AspNetUsers_PreviousClosedByUs~",
                        column: x => x.PreviousClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HrdAttendancePeriodReopenLog_AspNetUsers_ReopenedByUserId",
                        column: x => x.ReopenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdAttendancePeriodReopenLog_HrdAttendancePeriod_Attendance~",
                        column: x => x.AttendancePeriodId,
                        principalSchema: "public",
                        principalTable: "HrdAttendancePeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdAttendanceSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxBackdatedCorrectionDays = table.Column<int>(type: "integer", nullable: false),
                    RequireAttachmentForCorrection = table.Column<bool>(type: "boolean", nullable: false),
                    AutoConvertIncompleteToAbsent = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdAttendanceSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdBenefitGradeEligibility",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmploymentTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Permanent"),
                    SpecificAnnualLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    InpatientRoomClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Kelas 1"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdBenefitGradeEligibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdBenefitGradeEligibility_MstBenefitPlan_BenefitPlanId",
                        column: x => x.BenefitPlanId,
                        principalSchema: "public",
                        principalTable: "MstBenefitPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NationalIdNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsRehire = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidate_MstWorkforceProfile_PreviousWorkforceProfileId",
                        column: x => x.PreviousWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdContractCorrection",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforeContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BeforeContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforeStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BeforeEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BeforeDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AfterContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AfterContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AfterStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AfterEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EvidenceStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EvidenceFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    EvidenceContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CorrectionStatus = table.Column<int>(type: "integer", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdContractCorrection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_MstContractType_AfterContractTypeId",
                        column: x => x.AfterContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_MstContractType_BeforeContractTypeId",
                        column: x => x.BeforeContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractCorrection_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdContractDecision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecisionType = table.Column<int>(type: "integer", nullable: false),
                    ExtensionMonths = table.Column<int>(type: "integer", nullable: true),
                    ProposedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProposedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReasonText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DecisionStatus = table.Column<int>(type: "integer", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FollowUpContractHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeNotifiedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    NotificationRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeSeparationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdContractDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_AspNetUsers_NotificationRecordedByUserId",
                        column: x => x.NotificationRecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdContractDecision_WfpContractHistory_FollowUpContractHist~",
                        column: x => x.FollowUpContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdEarlyLeavePermission",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDailyId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedEarlyLeaveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeductible = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdEarlyLeavePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdEarlyLeavePermission_HrdAttendanceDaily_AttendanceDailyId",
                        column: x => x.AttendanceDailyId,
                        principalSchema: "public",
                        principalTable: "HrdAttendanceDaily",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdEarlyLeavePermission_MstWorkforceProfile_ApprovedByWorkf~",
                        column: x => x.ApprovedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HrdEarlyLeavePermission_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdEmploymentLifecycleSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    ContractEscalationDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 14),
                    ProbationReminderDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 14),
                    LateStartFlagDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdEmploymentLifecycleSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdEmploymentLifecycleSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdFppeTriggerEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DoctorCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SpecializationSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TriggerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetricEvidence = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecommendedAction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdFppeTriggerEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdInitialContractBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidRows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdInitialContractBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatch_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatch_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatch_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdJobRequisition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedSeats = table.Column<int>(type: "integer", nullable: false),
                    ApprovedSeats = table.Column<int>(type: "integer", nullable: false),
                    FulfilledSeats = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdJobRequisition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_HrdHeadcountRequest_HeadcountRequestId",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobRequisition_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdOfficialDutyRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DutyCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignmentOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Destination = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedByWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdOfficialDutyRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOfficialDutyRequest_MstWorkforceProfile_ApprovedByWorkfo~",
                        column: x => x.ApprovedByWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HrdOfficialDutyRequest_MstWorkforceProfile_WorkforceProfile~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdPayrollSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettingName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BpjsKesehatanCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BpjsKesehatanEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKesehatanEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJpCapAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJpEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJpEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJhtEmployeeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJhtEmployerRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJkkRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BpjsKetenagakerjaanJkmRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    VarianceThresholdPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    VarianceThresholdAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyCutoffDay = table.Column<int>(type: "integer", nullable: false),
                    DefaultPaymentDay = table.Column<int>(type: "integer", nullable: false),
                    DefaultNightShiftAllowanceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxLoanDeductionPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPayrollSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPayrollSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdPayslipPinLockout",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeKey = table.Column<Guid>(type: "uuid", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastFailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPayslipPinLockout", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdPayslipPinSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmployeeKey = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPayslipPinSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdPinAttemptRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VerificationHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ClientIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPinAttemptRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdProbation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReminderFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProbationStatus = table.Column<int>(type: "integer", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeSeparationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdProbation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProbation_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbation_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdProfileChangeHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HrServiceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentModified = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdProfileChangeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_MstWorkforceProfile_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProfileChangeHistory_TrxHrServiceRequest_HrServiceReques~",
                        column: x => x.HrServiceRequestId,
                        principalSchema: "public",
                        principalTable: "TrxHrServiceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdRecredentialingTierOverride",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculatedTier = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NewTier = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    JustificationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApprovedByCommitteeChairmanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChairmanNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OverriddenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdRecredentialingTierOverride", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdRecredentialingVerdict",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoctorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Verdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CommitteeNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedByCommitteeChairmanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignatureHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdRecredentialingVerdict", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdRestitutionSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeLevelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BenefitCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnnualLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumServiceMonthsRequired = table.Column<int>(type: "integer", nullable: false),
                    MaxDeductionPercentageOfGross = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AllowEmergencyCitoCashier = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdRestitutionSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrdSchedulingSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinRestHoursBetweenShifts = table.Column<int>(type: "integer", nullable: false, defaultValue: 11),
                    MaxConsecutiveWorkDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    MaxConsecutiveNightShifts = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    RetroactiveCorrectionWindowDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    RestHoursEscalatedToBlock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConsecutiveDaysEscalatedToBlock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NightShiftsEscalatedToBlock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdSchedulingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdSchedulingSettings_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdShiftBuddySafeguard",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsOverdueFlagged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GracePeriodDaysRemaining = table.Column<int>(type: "integer", nullable: false),
                    IsBuddySystemEnforced = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsRkkRestrictedLevel3 = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdShiftBuddySafeguard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdShiftBuddySafeguard_MstWorkforceProfile_WorkforceProfile~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mst_kpi_metric_catalogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetProfessionGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TargetPolarity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultTargetValue = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    ScoringRubricJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_kpi_metric_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mst_oppe_metric_catalogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MetricName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CompetencyDomain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SpecialtyGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataSourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetStandardValue = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    MetricWeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_oppe_metric_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstCredentialingCommittee",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CommitteeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubCommitteeCredentialName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChairpersonWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecretaryWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    MinimumPeerReviewersCount = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstCredentialingCommittee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCredentialingCommittee_MstWorkforceProfile_ChairpersonWo~",
                        column: x => x.ChairpersonWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MstCredentialingCommittee_MstWorkforceProfile_SecretaryWork~",
                        column: x => x.SecretaryWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "trx_bipartite_negotiation_forums",
                schema: "public",
                columns: table => new
                {
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    register_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    meeting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dispute_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    management_representatives = table.Column<string>(type: "text", nullable: false),
                    union_representatives = table.Column<string>(type: "text", nullable: false),
                    agenda_description = table.Column<string>(type: "text", nullable: false),
                    minutes_of_meeting = table.Column<string>(type: "text", nullable: false),
                    joint_agreement_document_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    registration_disnaker_number = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Scheduled"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_bipartite_negotiation_forums", x => x.forum_id);
                });

            migrationBuilder.CreateTable(
                name: "trx_disciplinary_incident_reports",
                schema: "public",
                columns: table => new
                {
                    incident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    incident_location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    just_culture_classification = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "HumanError"),
                    description = table.Column<string>(type: "text", nullable: false),
                    reporter_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reporter_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kmkp_audit_reference_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Reported"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_incident_reports", x => x.incident_report_id);
                });

            migrationBuilder.CreateTable(
                name: "trx_exit_clearance_processes",
                schema: "public",
                columns: table => new
                {
                    clearance_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    initiated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    target_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    overall_clearance_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "InProgress"),
                    is_final_payroll_unlocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    finalization_notes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_exit_clearance_processes", x => x.clearance_process_id);
                });

            migrationBuilder.CreateTable(
                name: "trx_fppe_engagements",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivilegeCatalogId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggerSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InitialReasonJustification = table.Column<string>(type: "text", nullable: false),
                    AssignedProctorWorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetCaseCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedCaseCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_fppe_engagements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trx_oppe_evaluation_periods",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EvaluationYear = table.Column<int>(type: "integer", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    QuantitativeWeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    QualitativeWeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_oppe_evaluation_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_appraisals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstLineManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondLineManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    KpiScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    BehaviorScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PreCalibrationScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FinalCompositeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PerformanceBand = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    CalibrationNotes = table.Column<string>(type: "text", nullable: true),
                    ManagerFeedbackNotes = table.Column<string>(type: "text", nullable: true),
                    EmployeeAcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_appraisals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisals_MstWorkforceProfile_FirstLineMan~",
                        column: x => x.FirstLineManagerId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisals_MstWorkforceProfile_SecondLineMa~",
                        column: x => x.SecondLineManagerId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisals_MstWorkforceProfile_WorkforcePro~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisals_trx_performance_cycles_Performan~",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_calibration_sessions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SessionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuidingDistributionJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActualDistributionJson = table.Column<string>(type: "jsonb", nullable: true),
                    TotalEmployeesCalibrated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalOverridesApplied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinutesOfMeetingUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Scheduled"),
                    CompletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_calibration_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_calibration_sessions_MstWorkforceProfile_Co~",
                        column: x => x.CompletedBy,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_calibration_sessions_trx_performance_cycles~",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_pip_action_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProgressSummary = table.Column<string>(type: "text", nullable: false),
                    DeficienciesObserved = table.Column<string>(type: "text", nullable: true),
                    MentorFeedback = table.Column<string>(type: "text", nullable: false),
                    IsTargetOnTrack = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LoggedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_pip_action_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_pip_action_logs_MstWorkforceProfile_LoggedBy",
                        column: x => x.LoggedBy,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_pip_action_logs_trx_performance_improvement~",
                        column: x => x.PipId,
                        principalSchema: "public",
                        principalTable: "trx_performance_improvement_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_plataran_sehat_skp_records",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingCertificateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SkpCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SkpPoints = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganizerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlataranSehatCertificateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerifiedByPlataranSehat = table.Column<bool>(type: "boolean", nullable: false),
                    PlataranSehatVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSyncedToOppe = table.Column<bool>(type: "boolean", nullable: false),
                    OppeSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvidenceDocumentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_plataran_sehat_skp_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_plataran_sehat_skp_records_MstWorkforceProfile_Workforc~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_severance_calculations",
                schema: "public",
                columns: table => new
                {
                    calculation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_monthly_wage = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    service_years = table.Column<int>(type: "integer", nullable: false),
                    service_months = table.Column<int>(type: "integer", nullable: false),
                    severance_pay_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    service_award_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    rights_compensation_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    separation_allowance_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    total_severance_gross = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    tax_deduction = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    total_severance_net = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Draft"),
                    is_approved_by_finance_director = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    finance_approval_notes = table.Column<string>(type: "text", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_severance_calculations", x => x.calculation_id);
                });

            migrationBuilder.CreateTable(
                name: "TrxClinicalPrivilegeEmergencyOverrideLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrivilegeItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcedureName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OverrideAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WitnessUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WitnessName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsReviewedByKmkp = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    KmkpReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    KmkpVerdict = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxClinicalPrivilegeEmergencyOverrideLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalPrivilegeEmergencyOverrideLog_AspNetUsers_Review~",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrxClinicalPrivilegeEmergencyOverrideLog_AspNetUsers_Witnes~",
                        column: x => x.WitnessUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrxClinicalPrivilegeEmergencyOverrideLog_MstWorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmployeeRestitutionClaim",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReceiptAttachmentUrl = table.Column<string>(type: "text", nullable: false),
                    MedicalResumeAttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    ClaimStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReasonNotes = table.Column<string>(type: "text", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    SupervisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupervisorApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HrReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HrReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCitoEmergency = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmployeeRestitutionClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_AspNetUsers_FinalApproverId",
                        column: x => x.FinalApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_AspNetUsers_HrReviewerId",
                        column: x => x.HrReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeRestitutionClaim_MstEmployee_SupervisorId",
                        column: x => x.SupervisorId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveRecallRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecallNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveRecallDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RestoredDays = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    EmergencyCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecallReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "RequestedByHeadUnit"),
                    RequestedByHeadUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovedByHrManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxLeaveRecallRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveRecallRequest_MstWorkforceProfile_ApprovedByHrManag~",
                        column: x => x.ApprovedByHrManagerId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveRecallRequest_MstWorkforceProfile_RequestedByHeadUn~",
                        column: x => x.RequestedByHeadUnitId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveRecallRequest_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveRecallRequest_WfpLeaveRequest_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPayrollDeductionSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    ExternalReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalLoanPrincipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyInstallmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInstallmentMonths = table.Column<int>(type: "integer", nullable: false),
                    PaidInstallmentCount = table.Column<int>(type: "integer", nullable: false),
                    RemainingPrincipal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduleStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AutoCarryOverOnCapExceeded = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollDeductionSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionSchedule_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPayrollHandoffBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    TotalEmployees = table.Column<int>(type: "integer", nullable: false),
                    TotalDisbursementAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HandoffStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransferredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinanceReferenceNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FinanceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PayloadSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollHandoffBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollHandoffBatch_AspNetUsers_TransferredByUserId",
                        column: x => x.TransferredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollHandoffBatch_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPayrollVarianceAlert",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PreviousNetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentNetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VariancePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AlertSeverity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AlertReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ResolutionStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JustificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollVarianceAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollVarianceAlert_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdInitialContractBatchRow",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FixedTermSeriesStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidationMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedContractHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdInitialContractBatchRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_HrdInitialContractBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "HrdInitialContractBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_MstWorkforceProfile_WorkforcePro~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdInitialContractBatchRow_WfpContractHistory_CreatedContra~",
                        column: x => x.CreatedContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdJobVacancy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    QuotaSeats = table.Column<int>(type: "integer", nullable: false),
                    IsClinical = table.Column<bool>(type: "boolean", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdJobVacancy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobVacancy_HrdJobRequisition_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "public",
                        principalTable: "HrdJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdProbationAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProbationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    ReasonText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EvidenceStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvidenceContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AssessmentStatus = table.Column<int>(type: "integer", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdProbationAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdProbationAssessment_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbationAssessment_AspNetUsers_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdProbationAssessment_HrdProbation_ProbationId",
                        column: x => x.ProbationId,
                        principalSchema: "public",
                        principalTable: "HrdProbation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdSimrsPrivilegeSyncLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VerdictId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetLevel = table.Column<int>(type: "integer", nullable: false),
                    IsHardStopBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdSimrsPrivilegeSyncLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdSimrsPrivilegeSyncLog_HrdRecredentialingVerdict_VerdictId",
                        column: x => x.VerdictId,
                        principalSchema: "public",
                        principalTable: "HrdRecredentialingVerdict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_goals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    KpiMetricCatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalDescription = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    AchievementPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Draft"),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_MstWorkforceProfile_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_mst_kpi_metric_catalogs_KpiMetricCata~",
                        column: x => x.KpiMetricCatalogId,
                        principalSchema: "public",
                        principalTable: "mst_kpi_metric_catalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_goals_trx_performance_cycles_PerformanceCyc~",
                        column: x => x.PerformanceCycleId,
                        principalSchema: "public",
                        principalTable: "trx_performance_cycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxSpkRkkIssuance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CredentialingApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpkNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RkkNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalPrivilegeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DirectorGeneralUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DirectorGeneralName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsSignedByDirectorGeneral = table.Column<bool>(type: "boolean", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QrCodeImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpkPdfFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RkkPdfFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpkFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    SpkFileChecksum = table.Column<string>(type: "text", nullable: true),
                    EDossierDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuanceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "text", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationBapNumber = table.Column<string>(type: "text", nullable: true),
                    RevocationReason = table.Column<string>(type: "text", nullable: true),
                    ApprovedPrivilegesSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxSpkRkkIssuance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxSpkRkkIssuance_AspNetUsers_DirectorGeneralUserId",
                        column: x => x.DirectorGeneralUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrxSpkRkkIssuance_MstCredentialingCommittee_CommitteeId",
                        column: x => x.CommitteeId,
                        principalSchema: "public",
                        principalTable: "MstCredentialingCommittee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrxSpkRkkIssuance_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrxSpkRkkIssuance_TrxCredentialingApplication_Credentialing~",
                        column: x => x.CredentialingApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCredentialingApplication",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "trx_disciplinary_investigation_records",
                schema: "public",
                columns: table => new
                {
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bap_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    hearing_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    investigator_names = table.Column<string>(type: "text", nullable: false),
                    witness_statements = table.Column<string>(type: "text", nullable: true),
                    evidence_document_urls = table.Column<string>(type: "text", nullable: true),
                    employee_defense_statement = table.Column<string>(type: "text", nullable: true),
                    investigation_findings = table.Column<string>(type: "text", nullable: false),
                    recommended_action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Draft"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_investigation_records", x => x.investigation_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_investigation_records_trx_disciplinary_inc~",
                        column: x => x.incident_report_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_incident_reports",
                        principalColumn: "incident_report_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_exit_clearance_checklists",
                schema: "public",
                columns: table => new
                {
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clearance_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    item_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_returned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    outstanding_liability_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    approver_notes = table.Column<string>(type: "text", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Pending"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_exit_clearance_checklists", x => x.checklist_id);
                    table.ForeignKey(
                        name: "FK_trx_exit_clearance_checklists_trx_exit_clearance_processes_~",
                        column: x => x.clearance_process_id,
                        principalSchema: "public",
                        principalTable: "trx_exit_clearance_processes",
                        principalColumn: "clearance_process_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_fppe_case_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FppeEngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimrsPatientEpisodeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcedurePerformed = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcedureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TechnicalSkillScore = table.Column<int>(type: "integer", nullable: false),
                    CriticalDecisionScore = table.Column<int>(type: "integer", nullable: false),
                    HasComplication = table.Column<bool>(type: "boolean", nullable: false),
                    ComplicationDescription = table.Column<string>(type: "text", nullable: true),
                    ProctorFeedbackNotes = table.Column<string>(type: "text", nullable: false),
                    ObservedIndependenceLevel = table.Column<int>(type: "integer", nullable: false),
                    IsVerifiedByProctor = table.Column<bool>(type: "boolean", nullable: false),
                    ProctorVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_fppe_case_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_fppe_case_logs_trx_fppe_engagements_FppeEngagementId",
                        column: x => x.FppeEngagementId,
                        principalSchema: "public",
                        principalTable: "trx_fppe_engagements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_fppe_evaluation_conclusions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FppeEngagementId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlenaryReviewSummary = table.Column<string>(type: "text", nullable: false),
                    RemediationPlan = table.Column<string>(type: "text", nullable: true),
                    ApprovedByCommitteeChairmanId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignatureHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_fppe_evaluation_conclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_fppe_evaluation_conclusions_trx_fppe_engagements_FppeEn~",
                        column: x => x.FppeEngagementId,
                        principalSchema: "public",
                        principalTable: "trx_fppe_engagements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_oppe_staff_evaluations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantitativeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    QualitativeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FinalCompositeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    PerformanceGrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EvaluatorKsmHeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluatorRecommendationNotes = table.Column<string>(type: "text", nullable: true),
                    IsAcknowledgedByStaff = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_oppe_staff_evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_oppe_staff_evaluations_trx_oppe_evaluation_periods_Peri~",
                        column: x => x.PeriodId,
                        principalSchema: "public",
                        principalTable: "trx_oppe_evaluation_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_clinical_career_ladder_reviews",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentClinicalLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetClinicalLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClinicalExperienceYears = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    MeetsPerformanceThreshold = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MeetsClinicalCompetency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FreeFromSentinelIncident = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CommitteeRecommendation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CommitteeNotes = table.Column<string>(type: "text", nullable: true),
                    FinalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Submitted"),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewSkNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_clinical_career_ladder_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_clinical_career_ladder_reviews_MstWorkforceProfile_Appr~",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_clinical_career_ladder_reviews_MstWorkforceProfile_Work~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_clinical_career_ladder_reviews_trx_performance_appraisa~",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_appraisal_details",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MetricOrCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SelfScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ManagerScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    EvidenceDocumentUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvaluatorNotes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_appraisal_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_details_trx_performance_appraisal~",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trx_performance_appraisal_disputes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformanceAppraisalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisputeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisputedComponent = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GrievanceReason = table.Column<string>(type: "text", nullable: false),
                    EvidenceDocumentUrl = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MediationNotes = table.Column<string>(type: "text", nullable: true),
                    FinalVerdict = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AdjustedCompositeScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    VerdictDeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Submitted"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_performance_appraisal_disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_disputes_MstWorkforceProfile_Deci~",
                        column: x => x.DecidedBy,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_disputes_MstWorkforceProfile_Work~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trx_performance_appraisal_disputes_trx_performance_appraisa~",
                        column: x => x.PerformanceAppraisalId,
                        principalSchema: "public",
                        principalTable: "trx_performance_appraisals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCashierDisbursementVoucher",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SecurityTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VoucherStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CashedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CashedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashierReceiptNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCashierDisbursementVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_AspNetUsers_CashedByUserId",
                        column: x => x.CashedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCashierDisbursementVoucher_TrxEmployeeRestitutionClaim_C~",
                        column: x => x.ClaimId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeRestitutionClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPayrollRestitution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestitutionNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestitutionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AffectedPayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedPayslipId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetPayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollRestitution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxEmployeeRestitutionClaim_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeRestitutionClaim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollPayslip_AffectedPayslipId",
                        column: x => x.AffectedPayslipId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollPayslip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollRun_AffectedPayrollRunId",
                        column: x => x.AffectedPayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollRestitution_TrxPayrollRun_TargetPayrollRunId",
                        column: x => x.TargetPayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxLeaveBalanceLedger",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AmountDays = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReferenceRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceRecallId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsForfeited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxLeaveBalanceLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxLeaveBalanceLedger_MstLeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalSchema: "public",
                        principalTable: "MstLeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveBalanceLedger_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveBalanceLedger_TrxLeaveAdjustment_ReferenceAdjustmen~",
                        column: x => x.ReferenceAdjustmentId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveAdjustment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveBalanceLedger_TrxLeaveRecallRequest_ReferenceRecall~",
                        column: x => x.ReferenceRecallId,
                        principalSchema: "public",
                        principalTable: "TrxLeaveRecallRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxLeaveBalanceLedger_WfpLeaveRequest_ReferenceRequestId",
                        column: x => x.ReferenceRequestId,
                        principalSchema: "public",
                        principalTable: "WfpLeaveRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPayrollDeductionItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualDeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeferredAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExecutionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeferralReason = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPayrollDeductionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_TrxPayrollDeductionSchedule_Deducti~",
                        column: x => x.DeductionScheduleId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollDeductionSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPayrollDeductionItem_TrxPayrollRun_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "TrxPayrollRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidateApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    StageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidateApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateApplication_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateApplication_HrdJobVacancy_VacancyId",
                        column: x => x.VacancyId,
                        principalSchema: "public",
                        principalTable: "HrdJobVacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdTalentPool",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalVacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RankingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RankingPosition = table.Column<int>(type: "integer", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RetentionExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PromotedToOfferId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdTalentPool", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdTalentPool_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdTalentPool_HrdJobVacancy_OriginalVacancyId",
                        column: x => x.OriginalVacancyId,
                        principalSchema: "public",
                        principalTable: "HrdJobVacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_disciplinary_sanctions",
                schema: "public",
                columns: table => new
                {
                    sanction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sanction_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sanction_level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    issued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    violation_description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                    is_appealed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    appeal_statement = table.Column<string>(type: "text", nullable: true),
                    appeal_evidence_urls = table.Column<string>(type: "text", nullable: true),
                    appeal_resolution = table.Column<string>(type: "text", nullable: true),
                    appealed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_sanctions", x => x.sanction_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_sanctions_trx_disciplinary_investigation_r~",
                        column: x => x.investigation_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_investigation_records",
                        principalColumn: "investigation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_disciplinary_suspensions",
                schema: "public",
                columns: table => new
                {
                    suspension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investigation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    suspension_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    wage_payment_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "PaidFull"),
                    is_system_access_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_physical_access_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                    reinstatement_notes = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_disciplinary_suspensions", x => x.suspension_id);
                    table.ForeignKey(
                        name: "FK_trx_disciplinary_suspensions_trx_disciplinary_investigation~",
                        column: x => x.investigation_id,
                        principalSchema: "public",
                        principalTable: "trx_disciplinary_investigation_records",
                        principalColumn: "investigation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trx_oppe_staff_evaluation_details",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffEvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCatalogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    ComputedScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EvaluatorNotes = table.Column<string>(type: "text", nullable: true),
                    RawMetricEvidenceJson = table.Column<string>(type: "text", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trx_oppe_staff_evaluation_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trx_oppe_staff_evaluation_details_mst_oppe_metric_catalogs_~",
                        column: x => x.MetricCatalogId,
                        principalSchema: "public",
                        principalTable: "mst_oppe_metric_catalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trx_oppe_staff_evaluation_details_trx_oppe_staff_evaluation~",
                        column: x => x.StaffEvaluationId,
                        principalSchema: "public",
                        principalTable: "trx_oppe_staff_evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidateDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidateDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateDocument_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrdCandidateDocument_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidateEvaluation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationType = table.Column<int>(type: "integer", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EvaluatorRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Recommendation = table.Column<int>(type: "integer", nullable: false),
                    IsClinicalVetoApplied = table.Column<bool>(type: "boolean", nullable: false),
                    VetoReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidateEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateEvaluation_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidateScreening",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScreeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsStrRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ClinicalScreeningResult = table.Column<int>(type: "integer", nullable: false),
                    StrNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidateScreening", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateScreening_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdJobOffer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferedBasicSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaximumGradeSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalTier = table.Column<int>(type: "integer", nullable: false),
                    OfferedPositionTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProposedJoinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OfferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CandidateResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CandidateResponseNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractDurationMonths = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdJobOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobOffer_HrdCandidateApplication_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobOffer_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdJobOffer_MstSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdCandidateHiringHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoffNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadcountRequestSeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    HandoffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdCandidateHiringHandoff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "HrdCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdHeadcountRequestSeat_Headcount~",
                        column: x => x.HeadcountRequestSeatId,
                        principalSchema: "public",
                        principalTable: "HrdHeadcountRequestSeat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdCandidateHiringHandoff_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdJobOfferApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    ApproverRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApproverName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdJobOfferApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdJobOfferApproval_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdPreEmploymentMedicalCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClinicOrFacility = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ClearanceResult = table.Column<int>(type: "integer", nullable: false),
                    DoctorNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CertificateFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExaminingDoctorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdPreEmploymentMedicalCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdPreEmploymentMedicalCheck_HrdJobOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "HrdJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HrdOnboardingCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HiringHandoffId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsClinical = table.Column<bool>(type: "boolean", nullable: false),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CoordinatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseStatus = table.Column<int>(type: "integer", nullable: false),
                    FirstDayAttendedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    FirstDayRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyForWorkAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyForWorkByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<int>(type: "integer", nullable: true),
                    CancelNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HasGeneralTemplate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdOnboardingCase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_AspNetUsers_CoordinatorUserId",
                        column: x => x.CoordinatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_AspNetUsers_FirstDayRecordedByUserId",
                        column: x => x.FirstDayRecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_AspNetUsers_ReadyForWorkByUserId",
                        column: x => x.ReadyForWorkByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_HrdCandidateHiringHandoff_HiringHandoffId",
                        column: x => x.HiringHandoffId,
                        principalSchema: "public",
                        principalTable: "HrdCandidateHiringHandoff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCase_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdClinicalPreceptorship",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorshipStatus = table.Column<int>(type: "integer", nullable: false),
                    AccompaniedSince = table.Column<DateOnly>(type: "date", nullable: true),
                    IndependentOn = table.Column<DateOnly>(type: "date", nullable: true),
                    DeclaredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvidenceContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AssessmentNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdClinicalPreceptorship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_AspNetUsers_DeclaredByUserId",
                        column: x => x.DeclaredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_HrdOnboardingCase_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstOrganizationUnit_OrganizationUn~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorship_MstWorkforceProfile_WorkforceProfi~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdOnboardingAccessRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SelfServiceActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdOnboardingAccessRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_HrdOnboardingCase_OnboardingCase~",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingAccessRequest_MstWorkforceProfile_WorkforcePro~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdOnboardingCaseItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateLayer = table.Column<int>(type: "integer", nullable: false),
                    SourceTemplateTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaskCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SystemItemCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ResponsibleRole = table.Column<int>(type: "integer", nullable: false),
                    ItemKind = table.Column<int>(type: "integer", nullable: false),
                    AutomaticSource = table.Column<int>(type: "integer", nullable: true),
                    CertificationTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DueDayOffset = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemStatus = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SelectedWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EvidenceContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EvidenceFileSize = table.Column<long>(type: "bigint", nullable: true),
                    AutomaticCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutomaticResultNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdOnboardingCaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_HrdOnboardingCase_OnboardingCaseId",
                        column: x => x.OnboardingCaseId,
                        principalSchema: "public",
                        principalTable: "HrdOnboardingCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstCertificationType_CertificationTyp~",
                        column: x => x.CertificationTypeId,
                        principalSchema: "public",
                        principalTable: "MstCertificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstOnboardingTemplateTask_SourceTempl~",
                        column: x => x.SourceTemplateTaskId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplateTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdOnboardingCaseItem_MstWorkforceProfile_SelectedWorkforce~",
                        column: x => x.SelectedWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrdClinicalPreceptorshipPreceptor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignedUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrdClinicalPreceptorshipPreceptor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorshipPreceptor_AspNetUsers_AssignedByUse~",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorshipPreceptor_HrdClinicalPreceptorship_~",
                        column: x => x.PreceptorshipId,
                        principalSchema: "public",
                        principalTable: "HrdClinicalPreceptorship",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrdClinicalPreceptorshipPreceptor_MstWorkforceProfile_Prece~",
                        column: x => x.PreceptorWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_SourceDocument",
                schema: "public",
                table: "WfpContractHistory",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxTrainingEnrollmentRequest_TrxTrainingSessionId",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest",
                column: "TrxTrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxShiftReplacement_ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxShiftReplacement_RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement",
                column: "RosterPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxShiftAssignment_OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                column: "OriginalEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxShiftAssignment_PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                column: "PreceptorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxShiftAssignment_ShiftDate_AssignmentStatus_IsDelete",
                schema: "public",
                table: "TrxShiftAssignment",
                columns: new[] { "ShiftDate", "AssignmentStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxRosterPeriod_ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "idx_ot_verification_state",
                schema: "public",
                table: "TrxOvertimeVerification",
                columns: new[] { "VerificationStatus", "IsAttendanceMatched", "HasVariance" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeVerification_VerifiedByHrAdminId",
                schema: "public",
                table: "TrxOvertimeVerification",
                column: "VerifiedByHrAdminId");

            migrationBuilder.CreateIndex(
                name: "idx_ot_realization_emp_date",
                schema: "public",
                table: "TrxOvertimeRealization",
                columns: new[] { "WorkforceProfileId", "ActualStartDate", "RealizationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimeRealization_MedicalDirectorApprovedById",
                schema: "public",
                table: "TrxOvertimeRealization",
                column: "MedicalDirectorApprovedById");

            migrationBuilder.CreateIndex(
                name: "idx_ot_plan_date_unit",
                schema: "public",
                table: "TrxOvertimePlan",
                columns: new[] { "OrganizationUnitId", "PlanStartDate", "PlanStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxOnCallAssignment_CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment",
                column: "CalledByUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOnCallAssignment_UnitId_AssignmentStatus_StartAt_EndAt_I~",
                schema: "public",
                table: "TrxOnCallAssignment",
                columns: new[] { "UnitId", "AssignmentStatus", "StartAt", "EndAt", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCredentialingApplication_MstCredentialingCommitteeId",
                schema: "public",
                table: "TrxCredentialingApplication",
                column: "MstCredentialingCommitteeId");

            migrationBuilder.CreateIndex(
                name: "idx_cto_emp_expiry",
                schema: "public",
                table: "TrxCompensatoryTimeOff",
                columns: new[] { "WorkforceProfileId", "ExpiryDate", "IsExpired" });

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_OrganizationUnitId_Active",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "OrganizationUnitId",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false AND \"TemplateLayer\" = 3");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_TemplateCode",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "TemplateCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_TemplateLayer_Active",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "TemplateLayer",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false AND \"TemplateLayer\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_MstContractType_EmploymentTypeId",
                schema: "public",
                table: "MstContractType",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDaily_EarlyLeavePermissionId",
                schema: "public",
                table: "HrdAttendanceDaily",
                column: "EarlyLeavePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceDaily_OfficialDutyRequestId",
                schema: "public",
                table: "HrdAttendanceDaily",
                column: "OfficialDutyRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_MentorWorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "MentorWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_Status",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_improvement_plans_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_cycles_EvaluationYear_Status",
                schema: "public",
                table: "trx_performance_cycles",
                columns: new[] { "EvaluationYear", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_cycles_StartDate_EndDate",
                schema: "public",
                table: "trx_performance_cycles",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_hrd_training_bond_contracts_WorkforceProfileId",
                schema: "public",
                table: "hrd_training_bond_contracts",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendancePeriodReopenLog_AttendancePeriodId",
                schema: "public",
                table: "HrdAttendancePeriodReopenLog",
                column: "AttendancePeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendancePeriodReopenLog_PreviousClosedByUserId",
                schema: "public",
                table: "HrdAttendancePeriodReopenLog",
                column: "PreviousClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendancePeriodReopenLog_ReopenedByUserId",
                schema: "public",
                table: "HrdAttendancePeriodReopenLog",
                column: "ReopenedByUserId");

            migrationBuilder.CreateIndex(
                name: "uq_benefit_grade_plan",
                schema: "public",
                table: "HrdBenefitGradeEligibility",
                columns: new[] { "BenefitPlanId", "GradeLevelCode", "EmploymentTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_CandidateCode",
                schema: "public",
                table: "HrdCandidate",
                column: "CandidateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_Email",
                schema: "public",
                table: "HrdCandidate",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_NationalIdNumber",
                schema: "public",
                table: "HrdCandidate",
                column: "NationalIdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_PreviousWorkforceProfileId",
                schema: "public",
                table: "HrdCandidate",
                column: "PreviousWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidate_Status",
                schema: "public",
                table: "HrdCandidate",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_ApplicationNumber",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "ApplicationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_CandidateId",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_CandidateId_VacancyId",
                schema: "public",
                table: "HrdCandidateApplication",
                columns: new[] { "CandidateId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_CurrentStage",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateApplication_VacancyId",
                schema: "public",
                table: "HrdCandidateApplication",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_ApplicationId",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_CandidateId",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateDocument_DocumentType",
                schema: "public",
                table: "HrdCandidateDocument",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_ApplicationId",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_EvaluationType",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "EvaluationType");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_EvaluatorUserId",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_IsClinicalVetoApplied",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "IsClinicalVetoApplied");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateEvaluation_Recommendation",
                schema: "public",
                table: "HrdCandidateEvaluation",
                column: "Recommendation");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_CandidateId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_HandoffNumber",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "HandoffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_HeadcountRequestSeatId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "HeadcountRequestSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_OfferId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_Status",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateHiringHandoff_WorkforceProfileId",
                schema: "public",
                table: "HrdCandidateHiringHandoff",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_ApplicationId",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_ClinicalScreeningResult",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "ClinicalScreeningResult");

            migrationBuilder.CreateIndex(
                name: "IX_HrdCandidateScreening_Status",
                schema: "public",
                table: "HrdCandidateScreening",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorship_DeclaredByUserId",
                schema: "public",
                table: "HrdClinicalPreceptorship",
                column: "DeclaredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorship_OnboardingCaseId",
                schema: "public",
                table: "HrdClinicalPreceptorship",
                column: "OnboardingCaseId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorship_OrganizationUnitId",
                schema: "public",
                table: "HrdClinicalPreceptorship",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorship_PreceptorshipStatus",
                schema: "public",
                table: "HrdClinicalPreceptorship",
                column: "PreceptorshipStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorship_WorkforceProfileId",
                schema: "public",
                table: "HrdClinicalPreceptorship",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorshipPreceptor_AssignedByUserId",
                schema: "public",
                table: "HrdClinicalPreceptorshipPreceptor",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorshipPreceptor_Open",
                schema: "public",
                table: "HrdClinicalPreceptorshipPreceptor",
                column: "PreceptorshipId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"AssignedUntil\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HrdClinicalPreceptorshipPreceptor_PreceptorWorkforceProfile~",
                schema: "public",
                table: "HrdClinicalPreceptorshipPreceptor",
                column: "PreceptorWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_AfterContractTypeId",
                schema: "public",
                table: "HrdContractCorrection",
                column: "AfterContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_BeforeContractTypeId",
                schema: "public",
                table: "HrdContractCorrection",
                column: "BeforeContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_ContractHistoryId_Open",
                schema: "public",
                table: "HrdContractCorrection",
                column: "ContractHistoryId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"CorrectionStatus\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_CorrectionNumber",
                schema: "public",
                table: "HrdContractCorrection",
                column: "CorrectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_DecidedByUserId",
                schema: "public",
                table: "HrdContractCorrection",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractCorrection_SubmittedByUserId",
                schema: "public",
                table: "HrdContractCorrection",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_ContractHistoryId_Open",
                schema: "public",
                table: "HrdContractDecision",
                column: "ContractHistoryId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"DecisionStatus\" IN (1, 2, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_DecidedByUserId",
                schema: "public",
                table: "HrdContractDecision",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_DecisionNumber",
                schema: "public",
                table: "HrdContractDecision",
                column: "DecisionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_FollowUpContractHistoryId",
                schema: "public",
                table: "HrdContractDecision",
                column: "FollowUpContractHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_NotificationRecordedByUserId",
                schema: "public",
                table: "HrdContractDecision",
                column: "NotificationRecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_SubmittedByUserId",
                schema: "public",
                table: "HrdContractDecision",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdContractDecision_WorkforceProfileId",
                schema: "public",
                table: "HrdContractDecision",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdEarlyLeavePermission_ApprovedByWorkforceId",
                schema: "public",
                table: "HrdEarlyLeavePermission",
                column: "ApprovedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdEarlyLeavePermission_AttendanceDailyId",
                schema: "public",
                table: "HrdEarlyLeavePermission",
                column: "AttendanceDailyId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdEarlyLeavePermission_WorkforceProfileId",
                schema: "public",
                table: "HrdEarlyLeavePermission",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdEmploymentLifecycleSetting_HospitalSiteId",
                schema: "public",
                table: "HrdEmploymentLifecycleSetting",
                column: "HospitalSiteId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdFppeTriggerEvent_DetectedAt",
                schema: "public",
                table: "HrdFppeTriggerEvent",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HrdFppeTriggerEvent_WorkforceProfileId_TriggerType",
                schema: "public",
                table: "HrdFppeTriggerEvent",
                columns: new[] { "WorkforceProfileId", "TriggerType" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatch_BatchNumber",
                schema: "public",
                table: "HrdInitialContractBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatch_DecidedByUserId",
                schema: "public",
                table: "HrdInitialContractBatch",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatch_HospitalSiteId",
                schema: "public",
                table: "HrdInitialContractBatch",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatch_SubmittedByUserId",
                schema: "public",
                table: "HrdInitialContractBatch",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatchRow_BatchId_WorkforceProfileId",
                schema: "public",
                table: "HrdInitialContractBatchRow",
                columns: new[] { "BatchId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatchRow_ContractTypeId",
                schema: "public",
                table: "HrdInitialContractBatchRow",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatchRow_CreatedContractHistoryId",
                schema: "public",
                table: "HrdInitialContractBatchRow",
                column: "CreatedContractHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdInitialContractBatchRow_WorkforceProfileId",
                schema: "public",
                table: "HrdInitialContractBatchRow",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_ApplicationId",
                schema: "public",
                table: "HrdJobOffer",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_ApprovalTier",
                schema: "public",
                table: "HrdJobOffer",
                column: "ApprovalTier");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_ContractTypeId",
                schema: "public",
                table: "HrdJobOffer",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_OfferNumber",
                schema: "public",
                table: "HrdJobOffer",
                column: "OfferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_SalaryGradeId",
                schema: "public",
                table: "HrdJobOffer",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOffer_Status",
                schema: "public",
                table: "HrdJobOffer",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_ApproverUserId",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_OfferId",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobOfferApproval_Status",
                schema: "public",
                table: "HrdJobOfferApproval",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_HeadcountRequestId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "HeadcountRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_HospitalSiteId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_OrganizationUnitId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_PositionId",
                schema: "public",
                table: "HrdJobRequisition",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_RequisitionNumber",
                schema: "public",
                table: "HrdJobRequisition",
                column: "RequisitionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobRequisition_Status",
                schema: "public",
                table: "HrdJobRequisition",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_IsClinical",
                schema: "public",
                table: "HrdJobVacancy",
                column: "IsClinical");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_RequisitionId",
                schema: "public",
                table: "HrdJobVacancy",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_Status",
                schema: "public",
                table: "HrdJobVacancy",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HrdJobVacancy_VacancyNumber",
                schema: "public",
                table: "HrdJobVacancy",
                column: "VacancyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdOfficialDutyRequest_ApprovedByWorkforceId",
                schema: "public",
                table: "HrdOfficialDutyRequest",
                column: "ApprovedByWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOfficialDutyRequest_WorkforceProfileId",
                schema: "public",
                table: "HrdOfficialDutyRequest",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingAccessRequest_ApplicationUserId",
                schema: "public",
                table: "HrdOnboardingAccessRequest",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingAccessRequest_Case_Type",
                schema: "public",
                table: "HrdOnboardingAccessRequest",
                columns: new[] { "OnboardingCaseId", "RequestType" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingAccessRequest_CompletedByUserId",
                schema: "public",
                table: "HrdOnboardingAccessRequest",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingAccessRequest_WorkforceProfileId",
                schema: "public",
                table: "HrdOnboardingAccessRequest",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_CancelledByUserId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_CaseNumber",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_CoordinatorUserId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "CoordinatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_FirstDayRecordedByUserId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "FirstDayRecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_HiringHandoffId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "HiringHandoffId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_HospitalSiteId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_OrganizationUnitId_CaseStatus",
                schema: "public",
                table: "HrdOnboardingCase",
                columns: new[] { "OrganizationUnitId", "CaseStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_PlannedStartDate",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "PlannedStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_PositionId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_ReadyForWorkByUserId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "ReadyForWorkByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCase_WorkforceProfileId",
                schema: "public",
                table: "HrdOnboardingCase",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_CertificationTypeId",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                column: "CertificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_CompletedByUserId",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_OnboardingCaseId_ItemCode",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                columns: new[] { "OnboardingCaseId", "ItemCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_SelectedWorkforceProfileId",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                column: "SelectedWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_SourceTemplateTaskId",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                column: "SourceTemplateTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdOnboardingCaseItem_SystemItemCode",
                schema: "public",
                table: "HrdOnboardingCaseItem",
                column: "SystemItemCode");

            migrationBuilder.CreateIndex(
                name: "idx_hrd_payroll_setting_site",
                schema: "public",
                table: "HrdPayrollSetting",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "uq_payslip_pin_lockout_employee",
                schema: "public",
                table: "HrdPayslipPinLockout",
                column: "EmployeeKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPayslipPinSession_EmployeeKey_ExpiresAt",
                schema: "public",
                table: "HrdPayslipPinSession",
                columns: new[] { "EmployeeKey", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "uq_payslip_pin_session_token",
                schema: "public",
                table: "HrdPayslipPinSession",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPinAttemptRecord_LastAttemptUtc",
                schema: "public",
                table: "HrdPinAttemptRecord",
                column: "LastAttemptUtc");

            migrationBuilder.CreateIndex(
                name: "uq_pin_attempt_record_key",
                schema: "public",
                table: "HrdPinAttemptRecord",
                column: "AttemptKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_ClearanceResult",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "ClearanceResult");

            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_OfferId",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "OfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdPreEmploymentMedicalCheck_ReferralNumber",
                schema: "public",
                table: "HrdPreEmploymentMedicalCheck",
                column: "ReferralNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_ClosedByUserId",
                schema: "public",
                table: "HrdProbation",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_ContractHistoryId",
                schema: "public",
                table: "HrdProbation",
                column: "ContractHistoryId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_Status_EndDate",
                schema: "public",
                table: "HrdProbation",
                columns: new[] { "ProbationStatus", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbation_WorkforceProfileId",
                schema: "public",
                table: "HrdProbation",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbationAssessment_DecidedByUserId",
                schema: "public",
                table: "HrdProbationAssessment",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbationAssessment_ProbationId_Open",
                schema: "public",
                table: "HrdProbationAssessment",
                column: "ProbationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"AssessmentStatus\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProbationAssessment_SubmittedByUserId",
                schema: "public",
                table: "HrdProbationAssessment",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_ApprovedByUserId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_EmployeeId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_EmployeeId_ComponentModified_Applie~",
                schema: "public",
                table: "HrdProfileChangeHistory",
                columns: new[] { "EmployeeId", "ComponentModified", "AppliedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdProfileChangeHistory_HrServiceRequestId",
                schema: "public",
                table: "HrdProfileChangeHistory",
                column: "HrServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdRecredentialingTierOverride_WorkforceProfileId_IsActive",
                schema: "public",
                table: "HrdRecredentialingTierOverride",
                columns: new[] { "WorkforceProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdRecredentialingTierOverride_WorkforceProfileId_Overridde~",
                schema: "public",
                table: "HrdRecredentialingTierOverride",
                columns: new[] { "WorkforceProfileId", "OverriddenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdRecredentialingVerdict_Verdict",
                schema: "public",
                table: "HrdRecredentialingVerdict",
                column: "Verdict");

            migrationBuilder.CreateIndex(
                name: "IX_HrdRecredentialingVerdict_WorkforceProfileId_DecidedAt",
                schema: "public",
                table: "HrdRecredentialingVerdict",
                columns: new[] { "WorkforceProfileId", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "uq_restitution_setting_grade_category",
                schema: "public",
                table: "HrdRestitutionSetting",
                columns: new[] { "GradeLevelCode", "BenefitCategory" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdSchedulingSettings_HospitalSiteId",
                schema: "public",
                table: "HrdSchedulingSettings",
                column: "HospitalSiteId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdShiftBuddySafeguard_WorkforceProfileId",
                schema: "public",
                table: "HrdShiftBuddySafeguard",
                column: "WorkforceProfileId",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HrdSimrsPrivilegeSyncLog_DispatchedAt",
                schema: "public",
                table: "HrdSimrsPrivilegeSyncLog",
                column: "DispatchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HrdSimrsPrivilegeSyncLog_VerdictId_DispatchedAt",
                schema: "public",
                table: "HrdSimrsPrivilegeSyncLog",
                columns: new[] { "VerdictId", "DispatchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_CandidateId",
                schema: "public",
                table: "HrdTalentPool",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_OriginalVacancyId",
                schema: "public",
                table: "HrdTalentPool",
                column: "OriginalVacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_RetentionExpiryDate",
                schema: "public",
                table: "HrdTalentPool",
                column: "RetentionExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_HrdTalentPool_Status",
                schema: "public",
                table: "HrdTalentPool",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_ComponentType",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_IsActive",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_mst_kpi_metric_catalogs_MetricCode",
                schema: "public",
                table: "mst_kpi_metric_catalogs",
                column: "MetricCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstCredentialingCommittee_ChairpersonWorkforceProfileId",
                schema: "public",
                table: "MstCredentialingCommittee",
                column: "ChairpersonWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MstCredentialingCommittee_SecretaryWorkforceProfileId",
                schema: "public",
                table: "MstCredentialingCommittee",
                column: "SecretaryWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_bipartite_negotiation_forums_dispute_type",
                schema: "public",
                table: "trx_bipartite_negotiation_forums",
                column: "dispute_type");

            migrationBuilder.CreateIndex(
                name: "IX_trx_bipartite_negotiation_forums_register_number",
                schema: "public",
                table: "trx_bipartite_negotiation_forums",
                column: "register_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trx_bipartite_negotiation_forums_status",
                schema: "public",
                table: "trx_bipartite_negotiation_forums",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_ApprovedBy",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_FinalStatus",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "FinalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_PerformanceAppraisalId",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_ReviewNumber",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "ReviewNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_clinical_career_ladder_reviews_WorkforceProfileId",
                schema: "public",
                table: "trx_clinical_career_ladder_reviews",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_incident_reports_employee_id",
                schema: "public",
                table: "trx_disciplinary_incident_reports",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_incident_reports_incident_number",
                schema: "public",
                table: "trx_disciplinary_incident_reports",
                column: "incident_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_incident_reports_status",
                schema: "public",
                table: "trx_disciplinary_incident_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_investigation_records_bap_number",
                schema: "public",
                table: "trx_disciplinary_investigation_records",
                column: "bap_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_investigation_records_incident_report_id",
                schema: "public",
                table: "trx_disciplinary_investigation_records",
                column: "incident_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_sanctions_employee_id",
                schema: "public",
                table: "trx_disciplinary_sanctions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_sanctions_investigation_id",
                schema: "public",
                table: "trx_disciplinary_sanctions",
                column: "investigation_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_sanctions_sanction_number",
                schema: "public",
                table: "trx_disciplinary_sanctions",
                column: "sanction_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_sanctions_status",
                schema: "public",
                table: "trx_disciplinary_sanctions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_sanctions_valid_until",
                schema: "public",
                table: "trx_disciplinary_sanctions",
                column: "valid_until");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_suspensions_employee_id",
                schema: "public",
                table: "trx_disciplinary_suspensions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_suspensions_expected_end_date",
                schema: "public",
                table: "trx_disciplinary_suspensions",
                column: "expected_end_date");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_suspensions_investigation_id",
                schema: "public",
                table: "trx_disciplinary_suspensions",
                column: "investigation_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_suspensions_status",
                schema: "public",
                table: "trx_disciplinary_suspensions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_disciplinary_suspensions_suspension_number",
                schema: "public",
                table: "trx_disciplinary_suspensions",
                column: "suspension_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_checklists_clearance_process_id",
                schema: "public",
                table: "trx_exit_clearance_checklists",
                column: "clearance_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_checklists_department_type",
                schema: "public",
                table: "trx_exit_clearance_checklists",
                column: "department_type");

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_checklists_status",
                schema: "public",
                table: "trx_exit_clearance_checklists",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_processes_employee_id",
                schema: "public",
                table: "trx_exit_clearance_processes",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_processes_overall_clearance_status",
                schema: "public",
                table: "trx_exit_clearance_processes",
                column: "overall_clearance_status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_exit_clearance_processes_separation_id",
                schema: "public",
                table: "trx_exit_clearance_processes",
                column: "separation_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_fppe_case_logs_FppeEngagementId",
                schema: "public",
                table: "trx_fppe_case_logs",
                column: "FppeEngagementId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_fppe_engagements_EngagementNumber",
                schema: "public",
                table: "trx_fppe_engagements",
                column: "EngagementNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_fppe_evaluation_conclusions_FppeEngagementId",
                schema: "public",
                table: "trx_fppe_evaluation_conclusions",
                column: "FppeEngagementId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_oppe_staff_evaluation_details_MetricCatalogId",
                schema: "public",
                table: "trx_oppe_staff_evaluation_details",
                column: "MetricCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_oppe_staff_evaluation_details_StaffEvaluationId",
                schema: "public",
                table: "trx_oppe_staff_evaluation_details",
                column: "StaffEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_oppe_staff_evaluations_PeriodId",
                schema: "public",
                table: "trx_oppe_staff_evaluations",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_details_ComponentType",
                schema: "public",
                table: "trx_performance_appraisal_details",
                column: "ComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_details_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_appraisal_details",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_DecidedBy",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "DecidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_DisputeNumber",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "DisputeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "PerformanceAppraisalId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_Status",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisal_disputes_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_appraisal_disputes",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_FinalCompositeScore",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "FinalCompositeScore");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_FirstLineManagerId",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "FirstLineManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_PerformanceBand",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "PerformanceBand");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_PerformanceCycleId_WorkforceProf~",
                schema: "public",
                table: "trx_performance_appraisals",
                columns: new[] { "PerformanceCycleId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_SecondLineManagerId",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "SecondLineManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_Status",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_appraisals_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_appraisals",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_CompletedBy",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "CompletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_PerformanceCycleId",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_SessionNumber",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "SessionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_calibration_sessions_Status",
                schema: "public",
                table: "trx_performance_calibration_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_ApprovedBy",
                schema: "public",
                table: "trx_performance_goals",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_KpiMetricCatalogId",
                schema: "public",
                table: "trx_performance_goals",
                column: "KpiMetricCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_PerformanceCycleId",
                schema: "public",
                table: "trx_performance_goals",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_Status",
                schema: "public",
                table: "trx_performance_goals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_goals_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_goals",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_pip_action_logs_LoggedBy",
                schema: "public",
                table: "trx_performance_pip_action_logs",
                column: "LoggedBy");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_pip_action_logs_PipId",
                schema: "public",
                table: "trx_performance_pip_action_logs",
                column: "PipId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_performance_pip_action_logs_PipId_WeekNumber",
                schema: "public",
                table: "trx_performance_pip_action_logs",
                columns: new[] { "PipId", "WeekNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_trx_plataran_sehat_skp_records_WorkforceProfileId",
                schema: "public",
                table: "trx_plataran_sehat_skp_records",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trx_severance_calculations_separation_id",
                schema: "public",
                table: "trx_severance_calculations",
                column: "separation_id");

            migrationBuilder.CreateIndex(
                name: "IX_trx_severance_calculations_status",
                schema: "public",
                table: "trx_severance_calculations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_voucher_claim",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "idx_cashier_voucher_status_exp",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                columns: new[] { "VoucherStatus", "ExpiredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCashierDisbursementVoucher_CashedByUserId",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "CashedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCashierDisbursementVoucher_EmployeeId",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "uq_cashier_disbursement_voucher_number",
                schema: "public",
                table: "TrxCashierDisbursementVoucher",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalPrivilegeEmergencyOverrideLog_ReviewedByUserId",
                schema: "public",
                table: "TrxClinicalPrivilegeEmergencyOverrideLog",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalPrivilegeEmergencyOverrideLog_WitnessUserId",
                schema: "public",
                table: "TrxClinicalPrivilegeEmergencyOverrideLog",
                column: "WitnessUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalPrivilegeEmergencyOverrideLog_WorkforceProfileId",
                schema: "public",
                table: "TrxClinicalPrivilegeEmergencyOverrideLog",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "idx_restitution_claim_employee_date",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                columns: new[] { "EmployeeId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "idx_restitution_claim_status",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "ClaimStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeRestitutionClaim_FinalApproverId",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "FinalApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeRestitutionClaim_HrReviewerId",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "HrReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeRestitutionClaim_SupervisorId",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "uq_employee_restitution_claim_number",
                schema: "public",
                table: "TrxEmployeeRestitutionClaim",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_ExpiryDate_IsForfeited_IsDelete",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                columns: new[] { "ExpiryDate", "IsForfeited", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_LeaveTypeId",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_PeriodYear_TransactionType_IsDelete",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                columns: new[] { "PeriodYear", "TransactionType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_ReferenceAdjustmentId",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                column: "ReferenceAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_ReferenceRecallId",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                column: "ReferenceRecallId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_ReferenceRequestId",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                column: "ReferenceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveBalanceLedger_WorkforceProfileId_LeaveTypeId_Period~",
                schema: "public",
                table: "TrxLeaveBalanceLedger",
                columns: new[] { "WorkforceProfileId", "LeaveTypeId", "PeriodYear", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveRecallRequest_ApprovedByHrManagerId",
                schema: "public",
                table: "TrxLeaveRecallRequest",
                column: "ApprovedByHrManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveRecallRequest_LeaveRequestId_Status_IsDelete",
                schema: "public",
                table: "TrxLeaveRecallRequest",
                columns: new[] { "LeaveRequestId", "Status", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveRecallRequest_RecallNumber",
                schema: "public",
                table: "TrxLeaveRecallRequest",
                column: "RecallNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveRecallRequest_RequestedByHeadUnitId",
                schema: "public",
                table: "TrxLeaveRecallRequest",
                column: "RequestedByHeadUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxLeaveRecallRequest_WorkforceProfileId_EffectiveRecallDat~",
                schema: "public",
                table: "TrxLeaveRecallRequest",
                columns: new[] { "WorkforceProfileId", "EffectiveRecallDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "idx_deduction_item_emp_priority",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                columns: new[] { "EmployeeId", "PriorityLevel" });

            migrationBuilder.CreateIndex(
                name: "idx_deduction_item_status",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                column: "ExecutionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollDeductionItem_DeductionScheduleId",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                column: "DeductionScheduleId");

            migrationBuilder.CreateIndex(
                name: "uq_deduction_item_run_schedule",
                schema: "public",
                table: "TrxPayrollDeductionItem",
                columns: new[] { "PayrollRunId", "DeductionScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deduction_schedule_emp_priority",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                columns: new[] { "EmployeeId", "PriorityLevel" });

            migrationBuilder.CreateIndex(
                name: "idx_deduction_schedule_status",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                column: "ScheduleStatus");

            migrationBuilder.CreateIndex(
                name: "uq_payroll_deduction_schedule_number",
                schema: "public",
                table: "TrxPayrollDeductionSchedule",
                column: "ScheduleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payroll_handoff_batch_number",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payroll_handoff_period",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                columns: new[] { "PeriodYear", "PeriodMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollHandoffBatch_PayrollRunId",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollHandoffBatch_TransferredByUserId",
                schema: "public",
                table: "TrxPayrollHandoffBatch",
                column: "TransferredByUserId");

            migrationBuilder.CreateIndex(
                name: "idx_payroll_restitution_emp_target",
                schema: "public",
                table: "TrxPayrollRestitution",
                columns: new[] { "EmployeeId", "TargetPayrollRunId" });

            migrationBuilder.CreateIndex(
                name: "idx_payroll_restitution_status",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollRestitution_AffectedPayrollRunId",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "AffectedPayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollRestitution_AffectedPayslipId",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "AffectedPayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollRestitution_ClaimId",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollRestitution_TargetPayrollRunId",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "TargetPayrollRunId");

            migrationBuilder.CreateIndex(
                name: "uq_payroll_restitution_number",
                schema: "public",
                table: "TrxPayrollRestitution",
                column: "RestitutionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_payroll_variance_run_emp",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                columns: new[] { "PayrollRunId", "WorkforceProfileId" });

            migrationBuilder.CreateIndex(
                name: "idx_payroll_variance_status",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                column: "ResolutionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollVarianceAlert_ResolvedByUserId",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPayrollVarianceAlert_WorkforceProfileId",
                schema: "public",
                table: "TrxPayrollVarianceAlert",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_CommitteeId",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                column: "CommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_CredentialingApplicationId",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                column: "CredentialingApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_DirectorGeneralUserId",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                column: "DirectorGeneralUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_WorkforceProfileId",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                column: "WorkforceProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_HrdAttendanceDaily_HrdEarlyLeavePermission_EarlyLeavePermis~",
                schema: "public",
                table: "HrdAttendanceDaily",
                column: "EarlyLeavePermissionId",
                principalSchema: "public",
                principalTable: "HrdEarlyLeavePermission",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HrdAttendanceDaily_HrdOfficialDutyRequest_OfficialDutyReque~",
                schema: "public",
                table: "HrdAttendanceDaily",
                column: "OfficialDutyRequestId",
                principalSchema: "public",
                principalTable: "HrdOfficialDutyRequest",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MstContractType_MstEmploymentType_EmploymentTypeId",
                schema: "public",
                table: "MstContractType",
                column: "EmploymentTypeId",
                principalSchema: "public",
                principalTable: "MstEmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstCertificationType_Certificatio~",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                column: "CertificationTypeId",
                principalSchema: "public",
                principalTable: "MstCertificationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "trx_performance_cycles",
                column: "ClosedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_AspNetUsers_PublishedByUserId",
                schema: "public",
                table: "trx_performance_cycles",
                column: "PublishedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstDepartment_DepartmentId",
                schema: "public",
                table: "trx_performance_cycles",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "trx_performance_cycles",
                column: "HospitalSiteId",
                principalSchema: "public",
                principalTable: "MstHospitalSite",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "trx_performance_cycles",
                column: "LegalEntityId",
                principalSchema: "public",
                principalTable: "MstLegalEntity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstOrganizationUnit_OrganizationUnit~",
                schema: "public",
                table: "trx_performance_cycles",
                column: "OrganizationUnitId",
                principalSchema: "public",
                principalTable: "MstOrganizationUnit",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceCycle_MasterPerformanc~",
                schema: "public",
                table: "trx_performance_cycles",
                column: "MasterPerformanceCycleId",
                principalSchema: "public",
                principalTable: "MstPerformanceCycle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceRatingScale_RatingScal~",
                schema: "public",
                table: "trx_performance_cycles",
                column: "RatingScaleId",
                principalSchema: "public",
                principalTable: "MstPerformanceRatingScale",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceTemplate_PerformanceTe~",
                schema: "public",
                table: "trx_performance_cycles",
                column: "PerformanceTemplateId",
                principalSchema: "public",
                principalTable: "MstPerformanceTemplate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_cycles_MstWorkflowDefinition_WorkflowDefini~",
                schema: "public",
                table: "trx_performance_cycles",
                column: "WorkflowDefinitionId",
                principalSchema: "public",
                principalTable: "MstWorkflowDefinition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_improvement_plans_MstWorkforceProfile_Mento~",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "MentorWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_improvement_plans_MstWorkforceProfile_Workf~",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_improvement_plans_trx_performance_appraisal~",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "PerformanceAppraisalId",
                principalSchema: "public",
                principalTable: "trx_performance_appraisals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trx_performance_improvement_plans_trx_performance_cycles_Pe~",
                schema: "public",
                table: "trx_performance_improvement_plans",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxCalibrationSession_trx_performance_cycles_PerformanceCyc~",
                schema: "public",
                table: "TrxCalibrationSession",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxCredentialingApplication_MstCredentialingCommittee_MstCr~",
                schema: "public",
                table: "TrxCredentialingApplication",
                column: "MstCredentialingCommitteeId",
                principalSchema: "public",
                principalTable: "MstCredentialingCommittee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxEmployeeGoal_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeGoal",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxEmployeeKpiTarget_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "TrxEmployeeKpiTarget",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxIndividualDevelopmentPlan_trx_performance_cycles_Perform~",
                schema: "public",
                table: "TrxIndividualDevelopmentPlan",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxManagerAssessment_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "TrxManagerAssessment",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOnCallAssignment_MstOrganizationUnit_CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment",
                column: "CalledByUnitId",
                principalSchema: "public",
                principalTable: "MstOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOnCallAssignment_MstOrganizationUnit_UnitId",
                schema: "public",
                table: "TrxOnCallAssignment",
                column: "UnitId",
                principalSchema: "public",
                principalTable: "MstOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeRealization_MstWorkforceProfile_MedicalDirectorA~",
                schema: "public",
                table: "TrxOvertimeRealization",
                column: "MedicalDirectorApprovedById",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxOvertimeVerification_MstWorkforceProfile_VerifiedByHrAdm~",
                schema: "public",
                table: "TrxOvertimeVerification",
                column: "VerifiedByHrAdminId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPeerFeedback_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxPeerFeedback",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCheckIn_trx_performance_cycles_PerformanceCyc~",
                schema: "public",
                table: "TrxPerformanceCheckIn",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxRosterPeriod_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod",
                column: "ClosedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxSelfAssessment_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxSelfAssessment",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxShiftAssignment_MstWorkforceProfile_OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                column: "OriginalEmployeeId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxShiftAssignment_MstWorkforceProfile_PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment",
                column: "PreceptorEmployeeId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxShiftReplacement_AspNetUsers_ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement",
                column: "ExecutedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxShiftReplacement_TrxRosterPeriod_RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement",
                column: "RosterPeriodId",
                principalSchema: "public",
                principalTable: "TrxRosterPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxTrainingEnrollmentRequest_TrxTrainingSession_TrxTraining~",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest",
                column: "TrxTrainingSessionId",
                principalSchema: "public",
                principalTable: "TrxTrainingSession",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WfpPerformanceReview_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "trx_performance_cycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HrdAttendanceDaily_HrdEarlyLeavePermission_EarlyLeavePermis~",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropForeignKey(
                name: "FK_HrdAttendanceDaily_HrdOfficialDutyRequest_OfficialDutyReque~",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropForeignKey(
                name: "FK_MstContractType_MstEmploymentType_EmploymentTypeId",
                schema: "public",
                table: "MstContractType");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstCertificationType_Certificatio~",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_AspNetUsers_PublishedByUserId",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstDepartment_DepartmentId",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstOrganizationUnit_OrganizationUnit~",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceCycle_MasterPerformanc~",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceRatingScale_RatingScal~",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstPerformanceTemplate_PerformanceTe~",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_cycles_MstWorkflowDefinition_WorkflowDefini~",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_improvement_plans_MstWorkforceProfile_Mento~",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_improvement_plans_MstWorkforceProfile_Workf~",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_improvement_plans_trx_performance_appraisal~",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_trx_performance_improvement_plans_trx_performance_cycles_Pe~",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxCalibrationSession_trx_performance_cycles_PerformanceCyc~",
                schema: "public",
                table: "TrxCalibrationSession");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxCredentialingApplication_MstCredentialingCommittee_MstCr~",
                schema: "public",
                table: "TrxCredentialingApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxEmployeeGoal_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeGoal");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxEmployeeKpiTarget_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "TrxEmployeeKpiTarget");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxIndividualDevelopmentPlan_trx_performance_cycles_Perform~",
                schema: "public",
                table: "TrxIndividualDevelopmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxManagerAssessment_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "TrxManagerAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOnCallAssignment_MstOrganizationUnit_CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOnCallAssignment_MstOrganizationUnit_UnitId",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeRealization_MstWorkforceProfile_MedicalDirectorA~",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxOvertimeVerification_MstWorkforceProfile_VerifiedByHrAdm~",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPeerFeedback_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxPeerFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPerformanceCheckIn_trx_performance_cycles_PerformanceCyc~",
                schema: "public",
                table: "TrxPerformanceCheckIn");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxRosterPeriod_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxSelfAssessment_trx_performance_cycles_PerformanceCycleId",
                schema: "public",
                table: "TrxSelfAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxShiftAssignment_MstWorkforceProfile_OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxShiftAssignment_MstWorkforceProfile_PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxShiftReplacement_AspNetUsers_ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxShiftReplacement_TrxRosterPeriod_RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxTrainingEnrollmentRequest_TrxTrainingSession_TrxTraining~",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_WfpPerformanceReview_trx_performance_cycles_PerformanceCycl~",
                schema: "public",
                table: "WfpPerformanceReview");

            migrationBuilder.DropTable(
                name: "hrd_clinical_peer_review_audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "hrd_training_bond_contracts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdAttendancePeriodReopenLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdAttendanceSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdBenefitGradeEligibility",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidateDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidateEvaluation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidateScreening",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdClinicalPreceptorshipPreceptor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdContractCorrection",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdContractDecision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdEarlyLeavePermission",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdEmploymentLifecycleSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdFppeTriggerEvent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdInitialContractBatchRow",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdJobOfferApproval",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOfficialDutyRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOnboardingAccessRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOnboardingCaseItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPayrollSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPayslipPinLockout",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPayslipPinSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPinAttemptRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdPreEmploymentMedicalCheck",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdProbationAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdProfileChangeHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdRecredentialingTierOverride",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdRestitutionSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdSchedulingSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdShiftBuddySafeguard",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdSimrsPrivilegeSyncLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdTalentPool",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_bipartite_negotiation_forums",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_clinical_career_ladder_reviews",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_disciplinary_sanctions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_disciplinary_suspensions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_exit_clearance_checklists",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_fppe_case_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_fppe_evaluation_conclusions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_oppe_staff_evaluation_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_appraisal_details",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_appraisal_disputes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_calibration_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_goals",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_pip_action_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_plataran_sehat_skp_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_severance_calculations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxCashierDisbursementVoucher",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxClinicalPrivilegeEmergencyOverrideLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLeaveBalanceLedger",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollDeductionItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollHandoffBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollRestitution",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollVarianceAlert",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxSpkRkkIssuance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdClinicalPreceptorship",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdInitialContractBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdProbation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdRecredentialingVerdict",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_disciplinary_investigation_records",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_exit_clearance_processes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_fppe_engagements",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mst_oppe_metric_catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_oppe_staff_evaluations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_performance_appraisals",
                schema: "public");

            migrationBuilder.DropTable(
                name: "mst_kpi_metric_catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxLeaveRecallRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPayrollDeductionSchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmployeeRestitutionClaim",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCredentialingCommittee",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdOnboardingCase",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_disciplinary_incident_reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trx_oppe_evaluation_periods",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidateHiringHandoff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdJobOffer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidateApplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdCandidate",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdJobVacancy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HrdJobRequisition",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_WfpContractHistory_SourceDocument",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropIndex(
                name: "IX_TrxTrainingEnrollmentRequest_TrxTrainingSessionId",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest");

            migrationBuilder.DropIndex(
                name: "IX_TrxShiftReplacement_ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropIndex(
                name: "IX_TrxShiftReplacement_RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropIndex(
                name: "IX_TrxShiftAssignment_OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxShiftAssignment_PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxShiftAssignment_ShiftDate_AssignmentStatus_IsDelete",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxRosterPeriod_ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropIndex(
                name: "idx_ot_verification_state",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropIndex(
                name: "IX_TrxOvertimeVerification_VerifiedByHrAdminId",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropIndex(
                name: "idx_ot_realization_emp_date",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropIndex(
                name: "IX_TrxOvertimeRealization_MedicalDirectorApprovedById",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropIndex(
                name: "idx_ot_plan_date_unit",
                schema: "public",
                table: "TrxOvertimePlan");

            migrationBuilder.DropIndex(
                name: "IX_TrxOnCallAssignment_CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxOnCallAssignment_UnitId_AssignmentStatus_StartAt_EndAt_I~",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TrxCredentialingApplication_MstCredentialingCommitteeId",
                schema: "public",
                table: "TrxCredentialingApplication");

            migrationBuilder.DropIndex(
                name: "idx_cto_emp_expiry",
                schema: "public",
                table: "TrxCompensatoryTimeOff");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_OrganizationUnitId_Active",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_TemplateCode",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstOnboardingTemplate_TemplateLayer_Active",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropIndex(
                name: "IX_MstContractType_EmploymentTypeId",
                schema: "public",
                table: "MstContractType");

            migrationBuilder.DropIndex(
                name: "IX_HrdAttendanceDaily_EarlyLeavePermissionId",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropIndex(
                name: "IX_HrdAttendanceDaily_OfficialDutyRequestId",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trx_performance_improvement_plans",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_improvement_plans_MentorWorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_improvement_plans_PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_improvement_plans_Status",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_improvement_plans_WorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trx_performance_cycles",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_cycles_EvaluationYear_Status",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropIndex(
                name: "IX_trx_performance_cycles_StartDate_EndDate",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropColumn(
                name: "CoverageMitigationNotes",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "CoverageWarningDetails",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "DoctorNoteFaskesName",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "DoctorSipNumber",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "HasStaffingCoverageWarning",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "IsDoctorNoteVerified",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "IsPayrollClosed",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "IsPendingPayrollReconciliation",
                schema: "public",
                table: "WfpLeaveRequest");

            migrationBuilder.DropColumn(
                name: "ExpiryRadarStatus",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "IsLifetimeStr",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "IsPsvVerified",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "PsvAuditLogJson",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "PsvVerificationSource",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "PsvVerificationUrl",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "PsvVerifiedAt",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "SipFaskesName",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "SipFaskesPracticeNumber",
                schema: "public",
                table: "WfpCredentialLicense");

            migrationBuilder.DropColumn(
                name: "DocumentContentType",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "DocumentFileName",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "FixedTermSeriesStartDate",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "SignedRecordedAt",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "SignedRecordedByUserId",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "SourceDocumentType",
                schema: "public",
                table: "WfpContractHistory");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "IsDelegatedAction",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "IsOverdue",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "OnBehalfOfEmployeeId",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "PriorityLevel",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "SlaDeadline",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "TotalSteps",
                schema: "public",
                table: "TrxWorkflowInstance");

            migrationBuilder.DropColumn(
                name: "ApproverEmployeeId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment");

            migrationBuilder.DropColumn(
                name: "DelegatedEmployeeId",
                schema: "public",
                table: "TrxWorkflowApproverAssignment");

            migrationBuilder.DropColumn(
                name: "DefaultedAt",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "IsOverdue",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "PaymentOrRefundRef",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "ReceiptDocUrl",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "RefundVoucherNumber",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                schema: "public",
                table: "TrxTravelSettlement");

            migrationBuilder.DropColumn(
                name: "IsMissingReceiptDeclared",
                schema: "public",
                table: "TrxTravelExpenseItem");

            migrationBuilder.DropColumn(
                name: "MissingReceiptApprovedBy",
                schema: "public",
                table: "TrxTravelExpenseItem");

            migrationBuilder.DropColumn(
                name: "MissingReceiptReason",
                schema: "public",
                table: "TrxTravelExpenseItem");

            migrationBuilder.DropColumn(
                name: "TrxTrainingSessionId",
                schema: "public",
                table: "TrxTrainingEnrollmentRequest");

            migrationBuilder.DropColumn(
                name: "ExecutedAt",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropColumn(
                name: "ExecutedByUserId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropColumn(
                name: "ReasonCategory",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropColumn(
                name: "RosterPeriodId",
                schema: "public",
                table: "TrxShiftReplacement");

            migrationBuilder.DropColumn(
                name: "IsAccompanied",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropColumn(
                name: "OriginalEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropColumn(
                name: "PreceptorEmployeeId",
                schema: "public",
                table: "TrxShiftAssignment");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropColumn(
                name: "IsRecheckPassed",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropColumn(
                name: "WarningCount",
                schema: "public",
                table: "TrxRosterPeriod");

            migrationBuilder.DropColumn(
                name: "BiometricCheckIn",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "BiometricCheckOut",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "BiometricExcessMinutes",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "DiscrepancyReason",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "ManualAdjustmentNotes",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "VerifiedByHrAdminId",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "VerifiedPayableMinutes",
                schema: "public",
                table: "TrxOvertimeVerification");

            migrationBuilder.DropColumn(
                name: "ClinicalActionSummary",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "CompensationType",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "EmergencyClinicalReferenceNo",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "IsMedicalDirectorEscalation",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "IsRetrospectiveAdjustment",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "MedicalDirectorApprovedById",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "OvertimeCategory",
                schema: "public",
                table: "TrxOvertimeRealization");

            migrationBuilder.DropColumn(
                name: "CompensationType",
                schema: "public",
                table: "TrxOvertimePlan");

            migrationBuilder.DropColumn(
                name: "IsEmergencyEscalation",
                schema: "public",
                table: "TrxOvertimePlan");

            migrationBuilder.DropColumn(
                name: "WeeklyQuotaHoursAtPlan",
                schema: "public",
                table: "TrxOvertimePlan");

            migrationBuilder.DropColumn(
                name: "CutoffDate",
                schema: "public",
                table: "TrxOvertimePeriod");

            migrationBuilder.DropColumn(
                name: "IsHandoffExecuted",
                schema: "public",
                table: "TrxOvertimePeriod");

            migrationBuilder.DropColumn(
                name: "TotalCtoGrantedHours",
                schema: "public",
                table: "TrxOvertimePeriod");

            migrationBuilder.DropColumn(
                name: "ArrivalTime",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "CallInTime",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "CallNotes",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "CalledByUnitId",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "IsCalledIn",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "IsSlaMet",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "ResponseMinutes",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "public",
                table: "TrxOnCallAssignment");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "DoctorSipNumber",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "FaskesName",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "IsConfidentialMedical",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "IsMedicalCertificate",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "MedicalDiagnosisIcd10",
                schema: "public",
                table: "TrxLeaveRequestAttachment");

            migrationBuilder.DropColumn(
                name: "AuditDocumentNumber",
                schema: "public",
                table: "TrxLeaveAdjustment");

            migrationBuilder.DropColumn(
                name: "PriorityTier",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaDeadlineUtc",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaPausedAtUtc",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaStatus",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "SlaTargetHours",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "TotalPausedMinutes",
                schema: "public",
                table: "TrxHrServiceRequest");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                schema: "public",
                table: "TrxEmployeeSeparation");

            migrationBuilder.DropColumn(
                name: "NoticeDate",
                schema: "public",
                table: "TrxEmployeeSeparation");

            migrationBuilder.DropColumn(
                name: "ResignationLetterUrl",
                schema: "public",
                table: "TrxEmployeeSeparation");

            migrationBuilder.DropColumn(
                name: "TerminationDecisionNumber",
                schema: "public",
                table: "TrxEmployeeSeparation");

            migrationBuilder.DropColumn(
                name: "BpjsKesehatanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "PrivateInsurancePolicyNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "ProbationEvaluationId",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "TerminationReason",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "MeasuredDoseMsv",
                schema: "public",
                table: "TrxEmployeeHealthSurveillance");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "EncryptedReportPayload",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "IsWhistleblowerCase",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "SeverityLevel",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "TargetResolutionDate",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "TargetUnit",
                schema: "public",
                table: "TrxEmployeeGrievance");

            migrationBuilder.DropColumn(
                name: "EDossierDocumentId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "IssuanceStatus",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "OfficialDocumentNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "PayrollPayslipId",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "QrCodeImagePath",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "RevocationBapNumber",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SalaryBrutoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SalaryNettoAmount",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SecurityPinHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerName",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerSignatureHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "SignerTitle",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "VerificationHash",
                schema: "public",
                table: "TrxEmployeeDocumentIssuance");

            migrationBuilder.DropColumn(
                name: "FamilyCardNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "IsActiveStudent",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "StudentVerificationExpiryDate",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "StudentVerificationLetterPath",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "MstCredentialingCommitteeId",
                schema: "public",
                table: "TrxCredentialingApplication");

            migrationBuilder.DropColumn(
                name: "GrantedDays",
                schema: "public",
                table: "TrxCompensatoryTimeOff");

            migrationBuilder.DropColumn(
                name: "IsExpired",
                schema: "public",
                table: "TrxCompensatoryTimeOff");

            migrationBuilder.DropColumn(
                name: "RemainingHours",
                schema: "public",
                table: "TrxCompensatoryTimeOff");

            migrationBuilder.DropColumn(
                name: "DependentId",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "OutOfPocketExcessAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "PrimaryPayerCoveredAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SecondaryClaimAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SepBpjsNumber",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SettlementReference",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "TotalHospitalBill",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "DelegateEmployeeId",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "DelegatedRequestTypes",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "PrincipalEmployeeId",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "public",
                table: "TrxApprovalDelegation");

            migrationBuilder.DropColumn(
                name: "ActionEmployeeId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "DigitalSignatureHash",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "OnBehalfOfEmployeeId",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "ReasonComment",
                schema: "public",
                table: "TrxApprovalAction");

            migrationBuilder.DropColumn(
                name: "AutomaticSource",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "ItemKind",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "SystemItemCode",
                schema: "public",
                table: "MstOnboardingTemplateTask");

            migrationBuilder.DropColumn(
                name: "TemplateLayer",
                schema: "public",
                table: "MstOnboardingTemplate");

            migrationBuilder.DropColumn(
                name: "AuthorizedSignerTitle",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "CurrentYear",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "IsFinancialSensitive",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "LastSequenceNumber",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "NumberingPattern",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "RequiresCbApproval",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "SupportedTokensJson",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "TemplateBodyHtml",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "ValidityPeriodDays",
                schema: "public",
                table: "MstEmployeeDocumentType");

            migrationBuilder.DropColumn(
                name: "EmploymentTypeId",
                schema: "public",
                table: "MstContractType");

            migrationBuilder.DropColumn(
                name: "BenefitCategory",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "CoverageTier",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "HospitalDiscountPercentage",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "MinimumServiceMonths",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "RequiresBPJSPrimaryCOB",
                schema: "public",
                table: "MstBenefitPlan");

            migrationBuilder.DropColumn(
                name: "BenefitPlanId",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "EarlyLeavePermissionId",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "IncompletePunchReason",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "OfficialDutyRequestId",
                schema: "public",
                table: "HrdAttendanceDaily");

            migrationBuilder.DropColumn(
                name: "ActionPlanSummary",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "FinalVerdict",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "MentorWorkforceProfileId",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "PerformanceAppraisalId",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "VerdictNotes",
                schema: "public",
                table: "trx_performance_improvement_plans");

            migrationBuilder.DropColumn(
                name: "CycleTitle",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropColumn(
                name: "CycleType",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropColumn(
                name: "ManagerAppraisalDeadline",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "trx_performance_cycles");

            migrationBuilder.RenameTable(
                name: "trx_performance_improvement_plans",
                schema: "public",
                newName: "TrxPerformanceImprovementPlan",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "trx_performance_cycles",
                schema: "public",
                newName: "TrxPerformanceCycle",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "SourceDocumentId",
                schema: "public",
                table: "WfpContractHistory",
                newName: "MstWorkforceProfileId");

            migrationBuilder.RenameIndex(
                name: "idx_ot_period_lookup",
                schema: "public",
                table: "TrxOvertimePeriod",
                newName: "IX_TrxOvertimePeriod_StartDate_EndDate_PeriodStatus");

            migrationBuilder.RenameColumn(
                name: "ResponsibleRole",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "CertificationTypeId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "ResponsiblePositionId");

            migrationBuilder.RenameIndex(
                name: "IX_MstOnboardingTemplateTask_TemplateId_TaskCode",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "IX_MstOnboardingTemplateTask_OnboardingTemplateId_TaskCode");

            migrationBuilder.RenameIndex(
                name: "IX_MstOnboardingTemplateTask_CertificationTypeId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                newName: "IX_MstOnboardingTemplateTask_ResponsiblePositionId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_improvement_plans_PipNumber",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                newName: "IX_TrxPerformanceImprovementPlan_PipNumber");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_improvement_plans_PerformanceCycleId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                newName: "IX_TrxPerformanceImprovementPlan_PerformanceCycleId");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "CycleStartDate");

            migrationBuilder.RenameColumn(
                name: "SelfAppraisalDeadline",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "CycleEndDate");

            migrationBuilder.RenameColumn(
                name: "EvaluationYear",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "CycleYear");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_WorkflowDefinitionId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_WorkflowDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_RatingScaleId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_RatingScaleId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_PublishedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_PublishedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_PerformanceTemplateId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_PerformanceTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_OrganizationUnitId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_OrganizationUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_MasterPerformanceCycleId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_MasterPerformanceCycleId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_LegalEntityId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_LegalEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_HospitalSiteId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_HospitalSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_DepartmentId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_CycleCode",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_CycleCode");

            migrationBuilder.RenameIndex(
                name: "IX_trx_performance_cycles_ClosedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle",
                newName: "IX_TrxPerformanceCycle_ClosedByUserId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsManualOverride",
                schema: "public",
                table: "TrxShiftAssignment",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ExpectedResponseMinutes",
                schema: "public",
                table: "TrxOnCallAssignment",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValue: 30);

            migrationBuilder.AlterColumn<string>(
                name: "TaskCategory",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "General");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DueDayOffset",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompletionSource",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDocument",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresVerification",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleOrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePartyType",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "TargetCompletionDays",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "AutoCreateChecklist",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveEndDate",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveStartDate",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LegalEntityId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProbationReviewAfterDays",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresProbationReview",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TemplateType",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowDefinitionId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PipNumber",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "PerformanceCycleId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEmployeeAcknowledged",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "CheckInFrequencyDays",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "integer",
                nullable: false,
                defaultValue: 30,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "PipStatus",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CycleCode",
                schema: "public",
                table: "TrxPerformanceCycle",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CalibrationDeadline",
                schema: "public",
                table: "TrxPerformanceCycle",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CycleName",
                schema: "public",
                table: "TrxPerformanceCycle",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CycleStatus",
                schema: "public",
                table: "TrxPerformanceCycle",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrxPerformanceImprovementPlan",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrxPerformanceCycle",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MstAssessmentMethod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOnlineSupported = table.Column<bool>(type: "boolean", nullable: false),
                    MaximumScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MethodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MethodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MethodType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PassingScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresProctor = table.Column<bool>(type: "boolean", nullable: false),
                    ResultValidityMonths = table.Column<int>(type: "integer", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstAssessmentMethod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstAssessmentMethod_MstCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "public",
                        principalTable: "MstCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstCandidateStatus",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowNewApplication = table.Column<bool>(type: "boolean", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFinalStatus = table.Column<bool>(type: "boolean", nullable: false),
                    IsHiredStatus = table.Column<bool>(type: "boolean", nullable: false),
                    IsInitialStatus = table.Column<bool>(type: "boolean", nullable: false),
                    IsRejectedStatus = table.Column<bool>(type: "boolean", nullable: false),
                    IsWithdrawnStatus = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    StatusCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StatusCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstCandidateStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstInterviewTemplate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobFamilyId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobLevelId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProfessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RatingScaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecializationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    EvaluationCriteriaJson = table.Column<string>(type: "jsonb", nullable: true),
                    InterviewType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MaximumPanelSize = table.Column<int>(type: "integer", nullable: false),
                    MinimumPanelSize = table.Column<int>(type: "integer", nullable: false),
                    PassingScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    QuestionDefinitionJson = table.Column<string>(type: "jsonb", nullable: true),
                    RequiresHiringManager = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresHrParticipation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPanelConsensus = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstInterviewTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstEmployeeGrade_EmployeeGradeId",
                        column: x => x.EmployeeGradeId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstJobFamily_JobFamilyId",
                        column: x => x.JobFamilyId,
                        principalSchema: "public",
                        principalTable: "MstJobFamily",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstJobLevel_JobLevelId",
                        column: x => x.JobLevelId,
                        principalSchema: "public",
                        principalTable: "MstJobLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstPerformanceRatingScale_RatingScaleId",
                        column: x => x.RatingScaleId,
                        principalSchema: "public",
                        principalTable: "MstPerformanceRatingScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstProfession_ProfessionId",
                        column: x => x.ProfessionId,
                        principalSchema: "public",
                        principalTable: "MstProfession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInterviewTemplate_MstSpecialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalSchema: "public",
                        principalTable: "MstSpecialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstRecruitmentSource",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DefaultCostAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmployeeReferral = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternalSource = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequiresCostTracking = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SourceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstRecruitmentSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstRecruitmentStage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowSkip = table.Column<bool>(type: "boolean", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFinalStage = table.Column<bool>(type: "boolean", nullable: false),
                    IsHireStage = table.Column<bool>(type: "boolean", nullable: false),
                    IsRejectStage = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresDocument = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresScore = table.Column<bool>(type: "boolean", nullable: false),
                    StageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StageName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StageOrder = table.Column<int>(type: "integer", nullable: false),
                    StageType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetCompletionDays = table.Column<int>(type: "integer", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstRecruitmentStage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrxContractNonRenewal",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeSeparationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DecisionDate = table.Column<DateTime>(type: "date", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeAcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NonRenewalNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NonRenewalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    NotificationDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReasonText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxContractNonRenewal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_AspNetUsers_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_MstRequestReason_RequestReasonId",
                        column: x => x.RequestReasonId,
                        principalSchema: "public",
                        principalTable: "MstRequestReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_MstWorkflowDefinition_WorkflowDefinit~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_TrxEmployeeSeparation_EmployeeSeparat~",
                        column: x => x.EmployeeSeparationId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeSeparation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxContractNonRenewal_WfpContractHistory_ContractHistoryId",
                        column: x => x.ContractHistoryId,
                        principalSchema: "public",
                        principalTable: "WfpContractHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxJobRequisition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnnualManpowerPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadcountRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobFamilyId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobLevelId = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManpowerPlanDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecializationId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkerSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedVacancyCount = table.Column<int>(type: "integer", nullable: false),
                    BusinessJustification = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    JobDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaximumSalaryBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MinimumQualification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinimumSalaryBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PriorityLevel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedVacancyCount = table.Column<int>(type: "integer", nullable: false),
                    RequiredStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequisitionStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequisitionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetFulfillmentDate = table.Column<DateTime>(type: "date", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxJobRequisition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstEmployeeCategory_EmployeeCategoryId",
                        column: x => x.EmployeeCategoryId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstEmployeeGrade_EmployeeGradeId",
                        column: x => x.EmployeeGradeId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstEmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalSchema: "public",
                        principalTable: "MstEmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstJobFamily_JobFamilyId",
                        column: x => x.JobFamilyId,
                        principalSchema: "public",
                        principalTable: "MstJobFamily",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstJobLevel_JobLevelId",
                        column: x => x.JobLevelId,
                        principalSchema: "public",
                        principalTable: "MstJobLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstProfession_ProfessionId",
                        column: x => x.ProfessionId,
                        principalSchema: "public",
                        principalTable: "MstProfession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstRequestReason_RequestReasonId",
                        column: x => x.RequestReasonId,
                        principalSchema: "public",
                        principalTable: "MstRequestReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstSpecialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalSchema: "public",
                        principalTable: "MstSpecialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstWorkerSource_WorkerSourceId",
                        column: x => x.WorkerSourceId,
                        principalSchema: "public",
                        principalTable: "MstWorkerSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstWorkflowDefinition_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstWorkforceProfile_RequestedByWorkforceP~",
                        column: x => x.RequestedByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_MstWorkforceType_WorkforceTypeId",
                        column: x => x.WorkforceTypeId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_TrxAnnualManpowerPlan_AnnualManpowerPlanId",
                        column: x => x.AnnualManpowerPlanId,
                        principalSchema: "public",
                        principalTable: "TrxAnnualManpowerPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_TrxHeadcountRequest_HeadcountRequestId",
                        column: x => x.HeadcountRequestId,
                        principalSchema: "public",
                        principalTable: "TrxHeadcountRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisition_TrxManpowerPlanDetail_ManpowerPlanDetailId",
                        column: x => x.ManpowerPlanDetailId,
                        principalSchema: "public",
                        principalTable: "TrxManpowerPlanDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostalCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruitmentSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferredByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdditionalDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    Address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    BirthPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BlacklistReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CandidateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CvFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    HasDataProcessingConsent = table.Column<bool>(type: "boolean", nullable: false),
                    IdentityNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsBlacklisted = table.Column<bool>(type: "boolean", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LinkedInUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceChannel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstCandidateStatus_CandidateStatusId",
                        column: x => x.CandidateStatusId,
                        principalSchema: "public",
                        principalTable: "MstCandidateStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstCity_CityId",
                        column: x => x.CityId,
                        principalSchema: "public",
                        principalTable: "MstCity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstCountry_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "MstCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "public",
                        principalTable: "MstDistrict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstPostalCode_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalSchema: "public",
                        principalTable: "MstPostalCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "public",
                        principalTable: "MstProvince",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstRecruitmentSource_RecruitmentSourceId",
                        column: x => x.RecruitmentSourceId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidate_MstWorkforceProfile_ReferredByWorkforceProfile~",
                        column: x => x.ReferredByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxJobRequisitionApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualActionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualActionByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedApproverWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    DelegatedFromUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RejectionReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelegated = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxJobRequisitionApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_AspNetUsers_ActualActionByUserId",
                        column: x => x.ActualActionByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_AspNetUsers_AssignedApproverUserId",
                        column: x => x.AssignedApproverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_AspNetUsers_DelegatedFromUserId",
                        column: x => x.DelegatedFromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_MstRejectionReason_RejectionReaso~",
                        column: x => x.RejectionReasonId,
                        principalSchema: "public",
                        principalTable: "MstRejectionReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_MstWorkflowStep_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_MstWorkforceProfile_ActualActionB~",
                        column: x => x.ActualActionByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_MstWorkforceProfile_AssignedAppro~",
                        column: x => x.AssignedApproverWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobRequisitionApproval_TrxJobRequisition_JobRequisitionId",
                        column: x => x.JobRequisitionId,
                        principalSchema: "public",
                        principalTable: "TrxJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxJobVacancy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRequisitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruitmentSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowExternalApplication = table.Column<bool>(type: "boolean", nullable: false),
                    AllowInternalApplication = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicationCount = table.Column<int>(type: "integer", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CloseDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmploymentLocationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FilledCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsSalaryConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "date", nullable: false),
                    PublicationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedSalaryMaximum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PublishedSalaryMinimum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Requirements = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    Responsibilities = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VacancyCount = table.Column<int>(type: "integer", nullable: false),
                    VacancyDescription = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    VacancyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VacancyTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxJobVacancy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxJobVacancy_AspNetUsers_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobVacancy_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobVacancy_MstRecruitmentSource_RecruitmentSourceId",
                        column: x => x.RecruitmentSourceId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobVacancy_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobVacancy_TrxJobRequisition_JobRequisitionId",
                        column: x => x.JobRequisitionId,
                        principalSchema: "public",
                        principalTable: "TrxJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateApplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedRecruiterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRequisitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobVacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitmentSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    WithdrawalReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CurrentStageOrder = table.Column<int>(type: "integer", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInternalCandidate = table.Column<bool>(type: "boolean", nullable: false),
                    IsPriorityCandidate = table.Column<bool>(type: "boolean", nullable: false),
                    LastStageChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverallScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_AspNetUsers_AssignedRecruiterUserId",
                        column: x => x.AssignedRecruiterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstCandidateStatus_CandidateStatusId",
                        column: x => x.CandidateStatusId,
                        principalSchema: "public",
                        principalTable: "MstCandidateStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstRecruitmentSource_RecruitmentSou~",
                        column: x => x.RecruitmentSourceId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstRecruitmentStage_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstRejectionReason_RejectionReasonId",
                        column: x => x.RejectionReasonId,
                        principalSchema: "public",
                        principalTable: "MstRejectionReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstRequestReason_WithdrawalReasonId",
                        column: x => x.WithdrawalReasonId,
                        principalSchema: "public",
                        principalTable: "MstRequestReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_MstWorkflowDefinition_WorkflowDefin~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_TrxCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "TrxCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_TrxJobRequisition_JobRequisitionId",
                        column: x => x.JobRequisitionId,
                        principalSchema: "public",
                        principalTable: "TrxJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateApplication_TrxJobVacancy_JobVacancyId",
                        column: x => x.JobVacancyId,
                        principalSchema: "public",
                        principalTable: "TrxJobVacancy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxBackgroundCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CheckStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CheckType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalReferenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Findings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HasCandidateConsent = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReportFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxBackgroundCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxBackgroundCheck_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxBackgroundCheck_TrxCandidateApplication_CandidateApplica~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruitmentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AssessmentStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvaluatorNotes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    ExternalReferenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RawScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ResultDetailJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateAssessment_AspNetUsers_EvaluatorUserId",
                        column: x => x.EvaluatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateAssessment_MstAssessmentMethod_AssessmentMethod~",
                        column: x => x.AssessmentMethodId,
                        principalSchema: "public",
                        principalTable: "MstAssessmentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateAssessment_MstRecruitmentStage_RecruitmentStage~",
                        column: x => x.RecruitmentStageId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateAssessment_TrxCandidateApplication_CandidateApp~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: true),
                    FileChecksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "date", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateDocument_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateDocument_TrxCandidateApplication_CandidateAppli~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateDocument_TrxCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "TrxCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateInterview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    InterviewTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    PanelLeadUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PanelLeadWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecruitmentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EvaluationSummaryJson = table.Column<string>(type: "jsonb", nullable: true),
                    FinalNotes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    FinalRecommendation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    InterviewMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InterviewRound = table.Column<int>(type: "integer", nullable: false),
                    InterviewStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    InterviewType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LocationDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MeetingUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PanelDefinitionJson = table.Column<string>(type: "jsonb", nullable: true),
                    RequiredEvaluationCount = table.Column<int>(type: "integer", nullable: false),
                    ScheduledEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedEvaluationCount = table.Column<int>(type: "integer", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateInterview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_AspNetUsers_PanelLeadUserId",
                        column: x => x.PanelLeadUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_MstInterviewTemplate_InterviewTemplat~",
                        column: x => x.InterviewTemplateId,
                        principalSchema: "public",
                        principalTable: "MstInterviewTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_MstRecruitmentStage_RecruitmentStageId",
                        column: x => x.RecruitmentStageId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_MstWorkforceProfile_PanelLeadWorkforc~",
                        column: x => x.PanelLeadWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateInterview_TrxCandidateApplication_CandidateAppl~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateScreening",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruitmentStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScreenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsShortlisted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ScreenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreeningDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    ScreeningResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ScreeningStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ScreeningType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateScreening", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateScreening_AspNetUsers_ScreenedByUserId",
                        column: x => x.ScreenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateScreening_MstRecruitmentStage_RecruitmentStageId",
                        column: x => x.RecruitmentStageId,
                        principalSchema: "public",
                        principalTable: "MstRecruitmentStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateScreening_TrxCandidateApplication_CandidateAppl~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxJobOffer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRequisitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RejectionReasonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllowanceConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BaseSalaryAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BenefitConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CandidateResponseNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OfferDate = table.Column<DateTime>(type: "date", nullable: false),
                    OfferDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OfferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OfferStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProbationMonths = table.Column<int>(type: "integer", nullable: true),
                    ProposedStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "date", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxJobOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstEmployeeCategory_EmployeeCategoryId",
                        column: x => x.EmployeeCategoryId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstEmployeeGrade_EmployeeGradeId",
                        column: x => x.EmployeeGradeId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstEmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalSchema: "public",
                        principalTable: "MstEmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstRejectionReason_RejectionReasonId",
                        column: x => x.RejectionReasonId,
                        principalSchema: "public",
                        principalTable: "MstRejectionReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalSchema: "public",
                        principalTable: "MstSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstSalaryStructure_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalSchema: "public",
                        principalTable: "MstSalaryStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_MstWorkflowDefinition_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_TrxCandidateApplication_CandidateApplicationId",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxJobOffer_TrxJobRequisition_JobRequisitionId",
                        column: x => x.JobRequisitionId,
                        principalSchema: "public",
                        principalTable: "TrxJobRequisition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPreEmploymentMedicalCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdministrativeNotes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExaminationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExaminedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FitnessResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MedicalCheckStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MedicalProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResultIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "date", nullable: true),
                    WorkRestrictions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPreEmploymentMedicalCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_MstWorkforceProfile_ReviewedBy~",
                        column: x => x.ReviewedByWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_TrxCandidateApplication_Candid~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPreEmploymentMedicalCheck_TrxCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "TrxCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxReferenceCheck",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CheckStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmploymentVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsPerformanceVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsRehireEligible = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReferenceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferencePosition = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RelationshipToCandidate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxReferenceCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxReferenceCheck_AspNetUsers_CheckedByUserId",
                        column: x => x.CheckedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxReferenceCheck_TrxCandidateApplication_CandidateApplicat~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxInterviewEvaluation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvaluatorWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    RatingScaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BehavioralScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Concerns = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CriteriaResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    CultureFitScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Strengths = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TechnicalScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxInterviewEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxInterviewEvaluation_AspNetUsers_EvaluatorUserId",
                        column: x => x.EvaluatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxInterviewEvaluation_MstPerformanceRatingScale_RatingScal~",
                        column: x => x.RatingScaleId,
                        principalSchema: "public",
                        principalTable: "MstPerformanceRatingScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxInterviewEvaluation_MstWorkforceProfile_EvaluatorWorkfor~",
                        column: x => x.EvaluatorWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxInterviewEvaluation_TrxCandidateInterview_CandidateInter~",
                        column: x => x.CandidateInterviewId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateInterview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxCandidateHiring",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentStatusId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkerSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmploymentStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    HireDate = table.Column<DateTime>(type: "date", nullable: false),
                    HiringNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HiringStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    OnboardingStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxCandidateHiring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstContractType_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "public",
                        principalTable: "MstContractType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstEmployeeCategory_EmployeeCategoryId",
                        column: x => x.EmployeeCategoryId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstEmployeeGrade_EmployeeGradeId",
                        column: x => x.EmployeeGradeId,
                        principalSchema: "public",
                        principalTable: "MstEmployeeGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstEmploymentStatus_EmploymentStatusId",
                        column: x => x.EmploymentStatusId,
                        principalSchema: "public",
                        principalTable: "MstEmploymentStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstEmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalSchema: "public",
                        principalTable: "MstEmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstWorkLocation_WorkLocationId",
                        column: x => x.WorkLocationId,
                        principalSchema: "public",
                        principalTable: "MstWorkLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstWorkerSource_WorkerSourceId",
                        column: x => x.WorkerSourceId,
                        principalSchema: "public",
                        principalTable: "MstWorkerSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_MstWorkforceType_WorkforceTypeId",
                        column: x => x.WorkforceTypeId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_TrxCandidateApplication_CandidateApplica~",
                        column: x => x.CandidateApplicationId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_TrxCandidate_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "public",
                        principalTable: "TrxCandidate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxCandidateHiring_TrxJobOffer_JobOfferId",
                        column: x => x.JobOfferId,
                        principalSchema: "public",
                        principalTable: "TrxJobOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmployeeOnboarding",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateHiringId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoordinatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnboardingTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "date", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    OnboardingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OnboardingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PlannedCompletionDate = table.Column<DateTime>(type: "date", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProbationEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmployeeOnboarding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_AspNetUsers_CoordinatorUserId",
                        column: x => x.CoordinatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_MstOnboardingTemplate_OnboardingTempl~",
                        column: x => x.OnboardingTemplateId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_MstWorkflowDefinition_WorkflowDefinit~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_MstWorkforceProfile_ManagerWorkforceP~",
                        column: x => x.ManagerWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_TrxCandidateHiring_CandidateHiringId",
                        column: x => x.CandidateHiringId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateHiring",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboarding_WfpOrganizationAssignment_Organizatio~",
                        column: x => x.OrganizationAssignmentId,
                        principalSchema: "public",
                        principalTable: "WfpOrganizationAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOnboardingChecklist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateHiringId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoordinatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnboardingTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "date", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChecklistNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChecklistStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CompletedRequiredTask = table.Column<int>(type: "integer", nullable: false),
                    CompletedTask = table.Column<int>(type: "integer", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MstWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OnboardingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlannedCompletionDate = table.Column<DateTime>(type: "date", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    RequiredTask = table.Column<int>(type: "integer", nullable: false),
                    TotalTask = table.Column<int>(type: "integer", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpOnboardingChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_AspNetUsers_CoordinatorUserId",
                        column: x => x.CoordinatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstOnboardingTemplate_OnboardingTemp~",
                        column: x => x.OnboardingTemplateId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstWorkforceProfile_ManagerWorkforce~",
                        column: x => x.ManagerWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstWorkforceProfile_MstWorkforceProf~",
                        column: x => x.MstWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_MstWorkforceProfile_WorkforceProfile~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingChecklist_TrxCandidateHiring_CandidateHiringId",
                        column: x => x.CandidateHiringId,
                        principalSchema: "public",
                        principalTable: "TrxCandidateHiring",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmployeeOnboardingTask",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeOnboardingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingTemplateTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RequiresDocument = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    TaskCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaskCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaskName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaskStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmployeeOnboardingTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_MstOnboardingTemplateTask_Onboard~",
                        column: x => x.OnboardingTemplateTaskId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplateTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_MstWorkforceProfile_AssignedToWor~",
                        column: x => x.AssignedToWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmployeeOnboardingTask_TrxEmployeeOnboarding_EmployeeOnb~",
                        column: x => x.EmployeeOnboardingId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeOnboarding",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxProbationReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeOnboardingId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformanceRatingScaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttendanceScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompetencyScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtendedProbationEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ImprovementAreas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OverallScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PerformanceScore = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ProbationEndDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProbationStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    Recommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReviewNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewResult = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Strengths = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxProbationReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_AspNetUsers_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstPerformanceRatingScale_PerformanceRat~",
                        column: x => x.PerformanceRatingScaleId,
                        principalSchema: "public",
                        principalTable: "MstPerformanceRatingScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstWorkflowDefinition_WorkflowDefinition~",
                        column: x => x.WorkflowDefinitionId,
                        principalSchema: "public",
                        principalTable: "MstWorkflowDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstWorkforceProfile_ReviewerWorkforcePro~",
                        column: x => x.ReviewerWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxProbationReview_TrxEmployeeOnboarding_EmployeeOnboarding~",
                        column: x => x.EmployeeOnboardingId,
                        principalSchema: "public",
                        principalTable: "TrxEmployeeOnboarding",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpOnboardingTask",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OnboardingChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingTemplateTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RequiresDocument = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    TaskCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaskCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaskName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaskStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WfpOnboardingTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_MstOnboardingTemplateTask_OnboardingTempl~",
                        column: x => x.OnboardingTemplateTaskId,
                        principalSchema: "public",
                        principalTable: "MstOnboardingTemplateTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_MstWorkforceProfile_AssignedToWorkforcePr~",
                        column: x => x.AssignedToWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpOnboardingTask_WfpOnboardingChecklist_OnboardingChecklis~",
                        column: x => x.OnboardingChecklistId,
                        principalSchema: "public",
                        principalTable: "WfpOnboardingChecklist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpContractHistory_MstWorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory",
                column: "MstWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxOvertimePlan_OrganizationUnitId",
                schema: "public",
                table: "TrxOvertimePlan",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplateTask_OnboardingTemplateId_SortOrder_Is~",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                columns: new[] { "OnboardingTemplateId", "SortOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplateTask_ResponsibleOrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                column: "ResponsibleOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_LegalEntityId_HospitalSiteId_Template~",
                schema: "public",
                table: "MstOnboardingTemplate",
                columns: new[] { "LegalEntityId", "HospitalSiteId", "TemplateType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_OrganizationUnitId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_PositionId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_TemplateCode_Version",
                schema: "public",
                table: "MstOnboardingTemplate",
                columns: new[] { "TemplateCode", "Version" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_WorkflowDefinitionId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnboardingTemplate_WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "WorkforceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_ApprovedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_EmployeeId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_ManagerUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_PerformanceReviewId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "PerformanceReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_SubmittedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_WorkflowDefinitionId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceImprovementPlan_WorkforceProfileId_PipStatus_~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                columns: new[] { "WorkforceProfileId", "PipStatus", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceCycle_CycleStartDate_CycleEndDate",
                schema: "public",
                table: "TrxPerformanceCycle",
                columns: new[] { "CycleStartDate", "CycleEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPerformanceCycle_CycleYear_CycleStatus",
                schema: "public",
                table: "TrxPerformanceCycle",
                columns: new[] { "CycleYear", "CycleStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_MstAssessmentMethod_CompetencyId",
                schema: "public",
                table: "MstAssessmentMethod",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssessmentMethod_MethodCode",
                schema: "public",
                table: "MstAssessmentMethod",
                column: "MethodCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssessmentMethod_MethodType_CompetencyId_IsActive",
                schema: "public",
                table: "MstAssessmentMethod",
                columns: new[] { "MethodType", "CompetencyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCandidateStatus_StatusCategory_IsFinalStatus_IsActive_So~",
                schema: "public",
                table: "MstCandidateStatus",
                columns: new[] { "StatusCategory", "IsFinalStatus", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCandidateStatus_StatusCode",
                schema: "public",
                table: "MstCandidateStatus",
                column: "StatusCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_EmployeeGradeId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_JobFamilyId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "JobFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_JobLevelId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "JobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_PositionId_ProfessionId_Specialization~",
                schema: "public",
                table: "MstInterviewTemplate",
                columns: new[] { "PositionId", "ProfessionId", "SpecializationId", "InterviewType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_ProfessionId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_RatingScaleId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "RatingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_SpecializationId",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInterviewTemplate_TemplateCode",
                schema: "public",
                table: "MstInterviewTemplate",
                column: "TemplateCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstRecruitmentSource_SourceCode",
                schema: "public",
                table: "MstRecruitmentSource",
                column: "SourceCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstRecruitmentSource_SourceType_IsActive_SortOrder",
                schema: "public",
                table: "MstRecruitmentSource",
                columns: new[] { "SourceType", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstRecruitmentStage_StageCode",
                schema: "public",
                table: "MstRecruitmentStage",
                column: "StageCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstRecruitmentStage_StageOrder_StageType_IsActive",
                schema: "public",
                table: "MstRecruitmentStage",
                columns: new[] { "StageOrder", "StageType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxBackgroundCheck_CandidateApplicationId_CheckType_CheckSt~",
                schema: "public",
                table: "TrxBackgroundCheck",
                columns: new[] { "CandidateApplicationId", "CheckType", "CheckStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxBackgroundCheck_ReviewedByUserId",
                schema: "public",
                table: "TrxBackgroundCheck",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_CandidateNumber",
                schema: "public",
                table: "TrxCandidate",
                column: "CandidateNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_CandidateStatusId_IsBlacklisted_IsActive",
                schema: "public",
                table: "TrxCandidate",
                columns: new[] { "CandidateStatusId", "IsBlacklisted", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_CityId",
                schema: "public",
                table: "TrxCandidate",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_CountryId",
                schema: "public",
                table: "TrxCandidate",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_DistrictId",
                schema: "public",
                table: "TrxCandidate",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_Email",
                schema: "public",
                table: "TrxCandidate",
                column: "Email",
                filter: "\"Email\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_IdentityNumber",
                schema: "public",
                table: "TrxCandidate",
                column: "IdentityNumber",
                filter: "\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_PostalCodeId",
                schema: "public",
                table: "TrxCandidate",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_ProvinceId",
                schema: "public",
                table: "TrxCandidate",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_RecruitmentSourceId",
                schema: "public",
                table: "TrxCandidate",
                column: "RecruitmentSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidate_ReferredByWorkforceProfileId",
                schema: "public",
                table: "TrxCandidate",
                column: "ReferredByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_ApplicationNumber",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "ApplicationNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_ApplicationStatus_CurrentStageId_As~",
                schema: "public",
                table: "TrxCandidateApplication",
                columns: new[] { "ApplicationStatus", "CurrentStageId", "AssignedRecruiterUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_AssignedRecruiterUserId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "AssignedRecruiterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_CandidateId_JobVacancyId",
                schema: "public",
                table: "TrxCandidateApplication",
                columns: new[] { "CandidateId", "JobVacancyId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"ApplicationStatus\" <> 'Withdrawn'");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_CandidateStatusId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "CandidateStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_CurrentStageId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_JobRequisitionId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "JobRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_JobVacancyId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "JobVacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_RecruitmentSourceId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "RecruitmentSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_RejectionReasonId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_WithdrawalReasonId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "WithdrawalReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_WorkflowDefinitionId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateApplication_WorkflowInstanceId",
                schema: "public",
                table: "TrxCandidateApplication",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateAssessment_AssessmentMethodId",
                schema: "public",
                table: "TrxCandidateAssessment",
                column: "AssessmentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateAssessment_CandidateApplicationId_AssessmentMet~",
                schema: "public",
                table: "TrxCandidateAssessment",
                columns: new[] { "CandidateApplicationId", "AssessmentMethodId", "AssessmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateAssessment_EvaluatorUserId",
                schema: "public",
                table: "TrxCandidateAssessment",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateAssessment_RecruitmentStageId",
                schema: "public",
                table: "TrxCandidateAssessment",
                column: "RecruitmentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateDocument_CandidateApplicationId",
                schema: "public",
                table: "TrxCandidateDocument",
                column: "CandidateApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateDocument_CandidateId_DocumentTypeCode_IsActive",
                schema: "public",
                table: "TrxCandidateDocument",
                columns: new[] { "CandidateId", "DocumentTypeCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateDocument_FileChecksum",
                schema: "public",
                table: "TrxCandidateDocument",
                column: "FileChecksum",
                filter: "\"FileChecksum\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateDocument_VerifiedByUserId",
                schema: "public",
                table: "TrxCandidateDocument",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_CandidateApplicationId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "CandidateApplicationId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_CandidateId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_CompletedByUserId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_ContractTypeId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_CostCenterId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_DepartmentId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_EmployeeCategoryId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_EmployeeGradeId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_EmployeeId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_EmploymentStatusId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "EmploymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_EmploymentTypeId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_HiringNumber",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "HiringNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_HiringStatus_HireDate_LegalEntityId_Hosp~",
                schema: "public",
                table: "TrxCandidateHiring",
                columns: new[] { "HiringStatus", "HireDate", "LegalEntityId", "HospitalSiteId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_HospitalSiteId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_JobOfferId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "JobOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_LegalEntityId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_OrganizationUnitId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_PositionId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_ProcessedByUserId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_WorkerSourceId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "WorkerSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_WorkforceProfileId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_WorkforceTypeId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "WorkforceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateHiring_WorkLocationId",
                schema: "public",
                table: "TrxCandidateHiring",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_CandidateApplicationId_InterviewRound~",
                schema: "public",
                table: "TrxCandidateInterview",
                columns: new[] { "CandidateApplicationId", "InterviewRound", "InterviewType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_HospitalSiteId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_InterviewTemplateId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "InterviewTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_PanelLeadUserId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "PanelLeadUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_PanelLeadWorkforceProfileId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "PanelLeadWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_RecruitmentStageId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "RecruitmentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_ScheduledStartAt_InterviewStatus_Pane~",
                schema: "public",
                table: "TrxCandidateInterview",
                columns: new[] { "ScheduledStartAt", "InterviewStatus", "PanelLeadUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateInterview_WorkLocationId",
                schema: "public",
                table: "TrxCandidateInterview",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateScreening_CandidateApplicationId_ScreeningType_~",
                schema: "public",
                table: "TrxCandidateScreening",
                columns: new[] { "CandidateApplicationId", "ScreeningType", "ScreeningStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateScreening_RecruitmentStageId",
                schema: "public",
                table: "TrxCandidateScreening",
                column: "RecruitmentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxCandidateScreening_ScreenedByUserId",
                schema: "public",
                table: "TrxCandidateScreening",
                column: "ScreenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_ApprovedByUserId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_ContractHistoryId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "ContractHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_ContractTypeId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_EmployeeId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_EmployeeSeparationId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "EmployeeSeparationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_InitiatedByUserId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_NonRenewalNumber",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "NonRenewalNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_RequestReasonId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "RequestReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_WorkflowDefinitionId",
                schema: "public",
                table: "TrxContractNonRenewal",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxContractNonRenewal_WorkforceProfileId_ContractEndDate_No~",
                schema: "public",
                table: "TrxContractNonRenewal",
                columns: new[] { "WorkforceProfileId", "ContractEndDate", "NonRenewalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_CandidateHiringId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "CandidateHiringId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_CompletedByUserId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_CoordinatorUserId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "CoordinatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_EmployeeId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_ManagerWorkforceProfileId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "ManagerWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_OnboardingNumber",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "OnboardingNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_OnboardingTemplateId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "OnboardingTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_OrganizationAssignmentId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "OrganizationAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_WorkflowDefinitionId",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboarding_WorkforceProfileId_OnboardingStatus_P~",
                schema: "public",
                table: "TrxEmployeeOnboarding",
                columns: new[] { "WorkforceProfileId", "OnboardingStatus", "PlannedStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_AssignedToUserId_TaskStatus_DueDa~",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                columns: new[] { "AssignedToUserId", "TaskStatus", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_AssignedToWorkforceProfileId",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                column: "AssignedToWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_CompletedByUserId",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_EmployeeOnboardingId_TaskCode",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                columns: new[] { "EmployeeOnboardingId", "TaskCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_OnboardingTemplateTaskId",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                column: "OnboardingTemplateTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmployeeOnboardingTask_VerifiedByUserId",
                schema: "public",
                table: "TrxEmployeeOnboardingTask",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxInterviewEvaluation_CandidateInterviewId_EvaluatorUserId",
                schema: "public",
                table: "TrxInterviewEvaluation",
                columns: new[] { "CandidateInterviewId", "EvaluatorUserId" },
                unique: true,
                filter: "\"EvaluatorUserId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxInterviewEvaluation_CandidateInterviewId_IsSubmitted_Rec~",
                schema: "public",
                table: "TrxInterviewEvaluation",
                columns: new[] { "CandidateInterviewId", "IsSubmitted", "Recommendation" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxInterviewEvaluation_EvaluatorUserId",
                schema: "public",
                table: "TrxInterviewEvaluation",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxInterviewEvaluation_EvaluatorWorkforceProfileId",
                schema: "public",
                table: "TrxInterviewEvaluation",
                column: "EvaluatorWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxInterviewEvaluation_RatingScaleId",
                schema: "public",
                table: "TrxInterviewEvaluation",
                column: "RatingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_ApprovedByUserId",
                schema: "public",
                table: "TrxJobOffer",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_CandidateApplicationId_OfferStatus_ValidUntil",
                schema: "public",
                table: "TrxJobOffer",
                columns: new[] { "CandidateApplicationId", "OfferStatus", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_ContractTypeId",
                schema: "public",
                table: "TrxJobOffer",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_CostCenterId",
                schema: "public",
                table: "TrxJobOffer",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_EmployeeCategoryId",
                schema: "public",
                table: "TrxJobOffer",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_EmployeeGradeId",
                schema: "public",
                table: "TrxJobOffer",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_EmploymentTypeId",
                schema: "public",
                table: "TrxJobOffer",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_JobRequisitionId",
                schema: "public",
                table: "TrxJobOffer",
                column: "JobRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_OfferNumber",
                schema: "public",
                table: "TrxJobOffer",
                column: "OfferNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_PositionId",
                schema: "public",
                table: "TrxJobOffer",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_RejectionReasonId",
                schema: "public",
                table: "TrxJobOffer",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_SalaryGradeId",
                schema: "public",
                table: "TrxJobOffer",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_SalaryStructureId",
                schema: "public",
                table: "TrxJobOffer",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_WorkflowDefinitionId",
                schema: "public",
                table: "TrxJobOffer",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_WorkflowInstanceId",
                schema: "public",
                table: "TrxJobOffer",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobOffer_WorkLocationId",
                schema: "public",
                table: "TrxJobOffer",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_AnnualManpowerPlanId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "AnnualManpowerPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_ApprovedByUserId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_ContractTypeId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_CostCenterId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_DepartmentId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_EmployeeCategoryId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_EmployeeGradeId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_EmploymentTypeId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_HeadcountRequestId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "HeadcountRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_HospitalSiteId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_JobFamilyId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "JobFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_JobLevelId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "JobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_LegalEntityId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_ManpowerPlanDetailId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "ManpowerPlanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_OrganizationUnitId_DepartmentId_PositionI~",
                schema: "public",
                table: "TrxJobRequisition",
                columns: new[] { "OrganizationUnitId", "DepartmentId", "PositionId", "RequisitionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_PositionId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_ProfessionId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_RequestedByUserId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_RequestedByWorkforceProfileId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "RequestedByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_RequestReasonId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "RequestReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_RequisitionNumber",
                schema: "public",
                table: "TrxJobRequisition",
                column: "RequisitionNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_RequisitionStatus_PriorityLevel_RequiredS~",
                schema: "public",
                table: "TrxJobRequisition",
                columns: new[] { "RequisitionStatus", "PriorityLevel", "RequiredStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_SpecializationId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_WorkerSourceId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "WorkerSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_WorkflowDefinitionId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_WorkflowInstanceId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_WorkforceTypeId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "WorkforceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisition_WorkLocationId",
                schema: "public",
                table: "TrxJobRequisition",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_ActualActionByUserId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "ActualActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_ActualActionByWorkforceProfileId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "ActualActionByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_AssignedApproverUserId_ApprovalSt~",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                columns: new[] { "AssignedApproverUserId", "ApprovalStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_AssignedApproverWorkforceProfileId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "AssignedApproverWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_DelegatedFromUserId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "DelegatedFromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_JobRequisitionId_StepOrder_Approv~",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                columns: new[] { "JobRequisitionId", "StepOrder", "ApprovalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_RejectionReasonId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "RejectionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobRequisitionApproval_WorkflowStepId",
                schema: "public",
                table: "TrxJobRequisitionApproval",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_HospitalSiteId",
                schema: "public",
                table: "TrxJobVacancy",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_JobRequisitionId",
                schema: "public",
                table: "TrxJobVacancy",
                column: "JobRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_PublicationStatus_OpenDate_CloseDate_IsActive",
                schema: "public",
                table: "TrxJobVacancy",
                columns: new[] { "PublicationStatus", "OpenDate", "CloseDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_PublishedByUserId",
                schema: "public",
                table: "TrxJobVacancy",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_RecruitmentSourceId",
                schema: "public",
                table: "TrxJobVacancy",
                column: "RecruitmentSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_VacancyNumber",
                schema: "public",
                table: "TrxJobVacancy",
                column: "VacancyNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxJobVacancy_WorkLocationId",
                schema: "public",
                table: "TrxJobVacancy",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_CandidateApplicationId_Medical~",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                columns: new[] { "CandidateApplicationId", "MedicalCheckStatus", "FitnessResult" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_CandidateId",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_HospitalSiteId",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_ReviewedByUserId",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_ReviewedByWorkforceProfileId",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                column: "ReviewedByWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPreEmploymentMedicalCheck_WorkLocationId",
                schema: "public",
                table: "TrxPreEmploymentMedicalCheck",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_ApprovedByUserId",
                schema: "public",
                table: "TrxProbationReview",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_DepartmentId",
                schema: "public",
                table: "TrxProbationReview",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_EmployeeId",
                schema: "public",
                table: "TrxProbationReview",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_EmployeeOnboardingId",
                schema: "public",
                table: "TrxProbationReview",
                column: "EmployeeOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_OrganizationUnitId",
                schema: "public",
                table: "TrxProbationReview",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_PerformanceRatingScaleId",
                schema: "public",
                table: "TrxProbationReview",
                column: "PerformanceRatingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_PositionId",
                schema: "public",
                table: "TrxProbationReview",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_ReviewerUserId",
                schema: "public",
                table: "TrxProbationReview",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_ReviewerWorkforceProfileId",
                schema: "public",
                table: "TrxProbationReview",
                column: "ReviewerWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_ReviewNumber",
                schema: "public",
                table: "TrxProbationReview",
                column: "ReviewNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_WorkflowDefinitionId",
                schema: "public",
                table: "TrxProbationReview",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxProbationReview_WorkforceProfileId_ProbationEndDate_Revi~",
                schema: "public",
                table: "TrxProbationReview",
                columns: new[] { "WorkforceProfileId", "ProbationEndDate", "ReviewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxReferenceCheck_CandidateApplicationId_CheckStatus_CheckR~",
                schema: "public",
                table: "TrxReferenceCheck",
                columns: new[] { "CandidateApplicationId", "CheckStatus", "CheckResult" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxReferenceCheck_CheckedByUserId",
                schema: "public",
                table: "TrxReferenceCheck",
                column: "CheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_CandidateHiringId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "CandidateHiringId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_ChecklistNumber",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "ChecklistNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_CoordinatorUserId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "CoordinatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_EmployeeId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_ManagerWorkforceProfileId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "ManagerWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_MstWorkforceProfileId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "MstWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_OnboardingTemplateId",
                schema: "public",
                table: "WfpOnboardingChecklist",
                column: "OnboardingTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingChecklist_WorkforceProfileId_ChecklistStatus_I~",
                schema: "public",
                table: "WfpOnboardingChecklist",
                columns: new[] { "WorkforceProfileId", "ChecklistStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_AssignedToUserId_TaskStatus_DueDate",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "AssignedToUserId", "TaskStatus", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_AssignedToWorkforceProfileId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "AssignedToWorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_CompletedByUserId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingChecklistId_TaskCode",
                schema: "public",
                table: "WfpOnboardingTask",
                columns: new[] { "OnboardingChecklistId", "TaskCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_OnboardingTemplateTaskId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "OnboardingTemplateTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpOnboardingTask_VerifiedByUserId",
                schema: "public",
                table: "WfpOnboardingTask",
                column: "VerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstDepartment_DepartmentId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstEmployeeCategory_EmployeeCategoryId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "EmployeeCategoryId",
                principalSchema: "public",
                principalTable: "MstEmployeeCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstEmploymentType_EmploymentTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "EmploymentTypeId",
                principalSchema: "public",
                principalTable: "MstEmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "HospitalSiteId",
                principalSchema: "public",
                principalTable: "MstHospitalSite",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "LegalEntityId",
                principalSchema: "public",
                principalTable: "MstLegalEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstPosition_PositionId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "PositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstWorkflowDefinition_WorkflowDefinit~",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "WorkflowDefinitionId",
                principalSchema: "public",
                principalTable: "MstWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplate_MstWorkforceType_WorkforceTypeId",
                schema: "public",
                table: "MstOnboardingTemplate",
                column: "WorkforceTypeId",
                principalSchema: "public",
                principalTable: "MstWorkforceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstOrganizationUnit_ResponsibleOr~",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                column: "ResponsibleOrganizationUnitId",
                principalSchema: "public",
                principalTable: "MstOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnboardingTemplateTask_MstPosition_ResponsiblePositionId",
                schema: "public",
                table: "MstOnboardingTemplateTask",
                column: "ResponsiblePositionId",
                principalSchema: "public",
                principalTable: "MstPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxCalibrationSession_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxCalibrationSession",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxEmployeeGoal_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeGoal",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxEmployeeKpiTarget_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxEmployeeKpiTarget",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxIndividualDevelopmentPlan_TrxPerformanceCycle_Performanc~",
                schema: "public",
                table: "TrxIndividualDevelopmentPlan",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxManagerAssessment_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxManagerAssessment",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPeerFeedback_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxPeerFeedback",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCheckIn_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxPerformanceCheckIn",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_AspNetUsers_ClosedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "ClosedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_AspNetUsers_PublishedByUserId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "PublishedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstDepartment_DepartmentId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "DepartmentId",
                principalSchema: "public",
                principalTable: "MstDepartment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstHospitalSite_HospitalSiteId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "HospitalSiteId",
                principalSchema: "public",
                principalTable: "MstHospitalSite",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstLegalEntity_LegalEntityId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "LegalEntityId",
                principalSchema: "public",
                principalTable: "MstLegalEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstOrganizationUnit_OrganizationUnitId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "OrganizationUnitId",
                principalSchema: "public",
                principalTable: "MstOrganizationUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceCycle_MasterPerformanceCy~",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "MasterPerformanceCycleId",
                principalSchema: "public",
                principalTable: "MstPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceRatingScale_RatingScaleId",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "RatingScaleId",
                principalSchema: "public",
                principalTable: "MstPerformanceRatingScale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstPerformanceTemplate_PerformanceTempl~",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "PerformanceTemplateId",
                principalSchema: "public",
                principalTable: "MstPerformanceTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceCycle_MstWorkflowDefinition_WorkflowDefinitio~",
                schema: "public",
                table: "TrxPerformanceCycle",
                column: "WorkflowDefinitionId",
                principalSchema: "public",
                principalTable: "MstWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_ApprovedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_ManagerUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_AspNetUsers_SubmittedByUserId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstEmployee_EmployeeId",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "EmployeeId",
                principalSchema: "public",
                principalTable: "MstEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstWorkflowDefinition_Workflo~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "WorkflowDefinitionId",
                principalSchema: "public",
                principalTable: "MstWorkflowDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_MstWorkforceProfile_Workforce~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "WorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_TrxPerformanceCycle_Performan~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPerformanceImprovementPlan_WfpPerformanceReview_Performa~",
                schema: "public",
                table: "TrxPerformanceImprovementPlan",
                column: "PerformanceReviewId",
                principalSchema: "public",
                principalTable: "WfpPerformanceReview",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxSelfAssessment_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "TrxSelfAssessment",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WfpContractHistory_MstWorkforceProfile_MstWorkforceProfileId",
                schema: "public",
                table: "WfpContractHistory",
                column: "MstWorkforceProfileId",
                principalSchema: "public",
                principalTable: "MstWorkforceProfile",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WfpPerformanceReview_TrxPerformanceCycle_PerformanceCycleId",
                schema: "public",
                table: "WfpPerformanceReview",
                column: "PerformanceCycleId",
                principalSchema: "public",
                principalTable: "TrxPerformanceCycle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
