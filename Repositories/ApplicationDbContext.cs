using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region GLOBAL
        public DbSet<SysAppVersion> SysAppVersions { get; set; }
        public DbSet<SysApplicationModule> SysApplicationModules { get; set; }
        public DbSet<SysControllerAccess> SysControllerAccesses { get; set; }
        public DbSet<SysActionAccess> SysActionAccesses { get; set; }
        public DbSet<SysAccessPolicy> SysAccessPolicies { get; set; }

        public DbSet<ApplicationUserFingerprintCredential> ApplicationUserFingerprintCredentials { get; set; }
        public DbSet<ApplicationUserOrganization> ApplicationUserOrganizations { get; set; }

        public DbSet<MstKioskDevice> MstKioskDevices { get; set; }
        public DbSet<MstIdentityScannerProfile> MstIdentityScannerProfiles { get; set; }

        public DbSet<MstCountry> MstCountries { get; set; }
        public DbSet<MstProvince> MstProvinces { get; set; }
        public DbSet<MstCity> MstCities { get; set; }
        public DbSet<MstDistrict> MstDistricts { get; set; }
        public DbSet<MstPostalCode> MstPostalCodes { get; set; }
        public DbSet<MstBank> MstBanks { get; set; }

        public DbSet<MstNurseStationCluster> MstNurseStationClusters { get; set; }
        public DbSet<MstNurseStationClusterClinic> MstNurseStationClusterClinics { get; set; }
        public DbSet<MstNurseStationClusterStaff> MstNurseStationClusterStaffs { get; set; }
        public DbSet<MstQueueDisplayDevice> MstQueueDisplayDevices { get; set; }

        public DbSet<MstQueueVoiceProfile> MstQueueVoiceProfiles { get; set; }
        #endregion GLOBAL

        #region CORPORATE

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - ORGANIZATION
        public DbSet<MstLegalEntity> MstLegalEntities { get; set; }
        public DbSet<MstHospitalSite> MstHospitalSites { get; set; }
        public DbSet<MstOrganizationUnit> MstOrganizationUnits { get; set; }
        public DbSet<MstDepartment> MstDepartments { get; set; }
        public DbSet<MstPosition> MstPositions { get; set; }
        public DbSet<MstJobFamily> MstJobFamilies { get; set; }
        public DbSet<MstJobLevel> MstJobLevels { get; set; }
        public DbSet<MstEmployeeGrade> MstEmployeeGrades { get; set; }
        public DbSet<MstCostCenter> MstCostCenters { get; set; }
        public DbSet<MstWorkLocation> MstWorkLocations { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - ORGANIZATION

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - WORKFORCE
        public DbSet<MstWorkforceProfile> MstWorkforceProfiles { get; set; }
        public DbSet<MstEmployee> MstEmployees { get; set; }
        public DbSet<MstDoctor> MstDoctors { get; set; }
        public DbSet<MstExternalUser> MstExternalUsers { get; set; }
        public DbSet<MstWorkforceType> MstWorkforceTypes { get; set; }
        public DbSet<MstEmployeeCategory> MstEmployeeCategories { get; set; }
        public DbSet<MstEmploymentType> MstEmploymentTypes { get; set; }
        public DbSet<MstEmploymentStatus> MstEmploymentStatuses { get; set; }
        public DbSet<MstContractType> MstContractTypes { get; set; }
        public DbSet<MstWorkerSource> MstWorkerSources { get; set; }
        public DbSet<MstTerminationReason> MstTerminationReasons { get; set; }
        public DbSet<MstTransferReason> MstTransferReasons { get; set; }
        public DbSet<MstPromotionReason> MstPromotionReasons { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - WORKFORCE

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - ATTENDANCE AND SCHEDULE
        public DbSet<MstWorkSchedule> MstWorkSchedules { get; set; }
        public DbSet<MstShift> MstShifts { get; set; }
        public DbSet<MstShiftGroup> MstShiftGroups { get; set; }
        public DbSet<MstShiftPattern> MstShiftPatterns { get; set; }
        public DbSet<MstWorkCalendar> MstWorkCalendars { get; set; }
        public DbSet<MstHoliday> MstHolidays { get; set; }
        public DbSet<MstAttendanceDevice> MstAttendanceDevices { get; set; }
        public DbSet<MstAttendanceLocation> MstAttendanceLocations { get; set; }
        public DbSet<MstAttendancePolicy> MstAttendancePolicies { get; set; }
        public DbSet<MstGracePeriodPolicy> MstGracePeriodPolicies { get; set; }
        public DbSet<MstRosterPolicy> MstRosterPolicies { get; set; }
        public DbSet<MstMinimumRestPolicy> MstMinimumRestPolicies { get; set; }
        public DbSet<MstOnCallType> MstOnCallTypes { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - ATTENDANCE AND SCHEDULE

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - LEAVE AND OVERTIME
        public DbSet<MstLeaveType> MstLeaveTypes { get; set; }
        public DbSet<MstLeavePolicy> MstLeavePolicies { get; set; }
        public DbSet<MstLeaveEntitlementPolicy> MstLeaveEntitlementPolicies { get; set; }
        public DbSet<MstLeaveCarryForwardPolicy> MstLeaveCarryForwardPolicies { get; set; }
        public DbSet<MstLeaveAdjustmentReason> MstLeaveAdjustmentReasons { get; set; }
        public DbSet<MstOvertimePolicy> MstOvertimePolicies { get; set; }
        public DbSet<MstOvertimeRate> MstOvertimeRates { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - LEAVE AND OVERTIME

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - TRAVEL AND EXPENSE
        public DbSet<MstTravelType> MstTravelTypes { get; set; }
        public DbSet<MstTravelPolicy> MstTravelPolicies { get; set; }
        public DbSet<MstTravelClass> MstTravelClasses { get; set; }
        public DbSet<MstTravelExpenseCategory> MstTravelExpenseCategories { get; set; }
        public DbSet<MstTravelAllowanceRate> MstTravelAllowanceRates { get; set; }
        public DbSet<MstTravelDestinationZone> MstTravelDestinationZones { get; set; }
        public DbSet<MstExpenseCategory> MstExpenseCategories { get; set; }
        public DbSet<MstReimbursementPolicy> MstReimbursementPolicies { get; set; }
        public DbSet<MstPaymentSettlementMethod> MstPaymentSettlementMethods { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - TRAVEL AND EXPENSE

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - PAYROLL AND BENEFIT
        public DbSet<MstPayrollPeriod> MstPayrollPeriods { get; set; }
        public DbSet<MstPayrollComponent> MstPayrollComponents { get; set; }
        public DbSet<MstPayrollComponentCategory> MstPayrollComponentCategories { get; set; }
        public DbSet<MstSalaryStructure> MstSalaryStructures { get; set; }
        public DbSet<MstSalaryGrade> MstSalaryGrades { get; set; }
        public DbSet<MstAllowanceType> MstAllowanceTypes { get; set; }
        public DbSet<MstDeductionType> MstDeductionTypes { get; set; }
        public DbSet<MstShiftAllowancePolicy> MstShiftAllowancePolicies { get; set; }
        public DbSet<MstOnCallAllowancePolicy> MstOnCallAllowancePolicies { get; set; }
        public DbSet<MstHazardAllowancePolicy> MstHazardAllowancePolicies { get; set; }
        public DbSet<MstBenefitPlan> MstBenefitPlans { get; set; }
        public DbSet<MstBenefitType> MstBenefitTypes { get; set; }
        public DbSet<MstBenefitEligibilityRule> MstBenefitEligibilityRules { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - PAYROLL AND BENEFIT

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - COMPETENCY AND CREDENTIAL
        public DbSet<MstCompetency> MstCompetencies { get; set; }
        public DbSet<MstPositionCompetencyRequirement> MstPositionCompetencyRequirements { get; set; }
        public DbSet<MstTrainingCatalog> MstTrainingCatalogs { get; set; }
        public DbSet<MstTrainingCategory> MstTrainingCategories { get; set; }
        public DbSet<MstCertificationType> MstCertificationTypes { get; set; }
        public DbSet<MstLicenseType> MstLicenseTypes { get; set; }
        public DbSet<MstProfession> MstProfessions { get; set; }
        public DbSet<MstSpecialization> MstSpecializations { get; set; }
        public DbSet<MstCredentialingRequirement> MstCredentialingRequirements { get; set; }
        public DbSet<MstClinicalPrivilegeCatalog> MstClinicalPrivilegeCatalogs { get; set; }
        public DbSet<MstMandatoryTrainingRule> MstMandatoryTrainingRules { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - COMPETENCY AND CREDENTIAL

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - PERFORMANCE
        public DbSet<MstPerformanceCycle> MstPerformanceCycles { get; set; }
        public DbSet<MstPerformanceRatingScale> MstPerformanceRatingScales { get; set; }
        public DbSet<MstPerformanceTemplate> MstPerformanceTemplates { get; set; }
        public DbSet<MstPerformanceTemplateDetail> MstPerformanceTemplateDetails { get; set; }
        public DbSet<MstKpiCatalog> MstKpiCatalogs { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - PERFORMANCE

        #region CORPORATE - HUMAN RESOURCE - MASTER DATA - WORKFLOW
        public DbSet<MstApprovalDelegationPolicy> MstApprovalDelegationPolicies { get; set; }
        public DbSet<MstRequestReason> MstRequestReasons { get; set; }
        public DbSet<MstRejectionReason> MstRejectionReasons { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - MASTER DATA - WORKFLOW

        #region CORPORATE - HUMAN RESOURCE - WORKFORCE CORE
        public DbSet<WfpOrganizationAssignment> WfpOrganizationAssignments { get; set; }
        public DbSet<WfpBankAccount> WfpBankAccounts { get; set; }
        public DbSet<WfpDocument> WfpDocuments { get; set; }
        public DbSet<WfpEducation> WfpEducations { get; set; }
        public DbSet<WfpEmploymentHistory> WfpEmploymentHistories { get; set; }
        public DbSet<WfpContractHistory> WfpContractHistories { get; set; }
        public DbSet<WfpEmergencyContact> WfpEmergencyContacts { get; set; }
        public DbSet<WfpFamilyMember> WfpFamilyMembers { get; set; }
        public DbSet<WfpDependent> WfpDependents { get; set; }
        public DbSet<WfpAddress> WfpAddresses { get; set; }
        public DbSet<WfpPositionAssignment> WfpPositionAssignments { get; set; }
        public DbSet<WfpManagerAssignment> WfpManagerAssignments { get; set; }
        public DbSet<WfpSalaryAssignment> WfpSalaryAssignments { get; set; }
        public DbSet<TrxEmployeeProfileChangeRequest> TrxEmployeeProfileChangeRequests { get; set; }
        public DbSet<TrxEmployeeProfileChangeDetail> TrxEmployeeProfileChangeDetails { get; set; }
        public DbSet<TrxEmployeeProfileChangeVerification> TrxEmployeeProfileChangeVerifications { get; set; }
        public DbSet<TrxEmployeeTransfer> TrxEmployeeTransfers { get; set; }
        public DbSet<TrxEmployeePromotion> TrxEmployeePromotions { get; set; }
        public DbSet<TrxEmployeeDemotion> TrxEmployeeDemotions { get; set; }
        public DbSet<TrxEmployeeRotation> TrxEmployeeRotations { get; set; }
        public DbSet<TrxTemporaryAssignment> TrxTemporaryAssignments { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - WORKFORCE CORE

        #region CORPORATE - HUMAN RESOURCE - WORKFORCE PLANNING
        public DbSet<MstWorkforceRequirement> MstWorkforceRequirements { get; set; }
        public DbSet<MstStaffingStandard> MstStaffingStandards { get; set; }
        public DbSet<MstStaffingRatio> MstStaffingRatios { get; set; }
        public DbSet<MstShiftSkillRequirement> MstShiftSkillRequirements { get; set; }
        public DbSet<MstPositionHeadcountPlan> MstPositionHeadcountPlans { get; set; }
        public DbSet<TrxAnnualManpowerPlan> TrxAnnualManpowerPlans { get; set; }
        public DbSet<TrxManpowerPlanDetail> TrxManpowerPlanDetails { get; set; }
        public DbSet<TrxHeadcountRequest> TrxHeadcountRequests { get; set; }
        public DbSet<TrxStaffingGapAnalysis> TrxStaffingGapAnalyses { get; set; }
        public DbSet<TrxDailyStaffingRequirement> TrxDailyStaffingRequirements { get; set; }
        public DbSet<TrxWorkforceAllocation> TrxWorkforceAllocations { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - WORKFORCE PLANNING

        #region CORPORATE - HUMAN RESOURCE - RECRUITMENT MANAGEMENT
        public DbSet<MstRecruitmentSource> MstRecruitmentSources { get; set; }
        public DbSet<MstRecruitmentStage> MstRecruitmentStages { get; set; }
        public DbSet<MstCandidateStatus> MstCandidateStatuses { get; set; }
        public DbSet<MstInterviewTemplate> MstInterviewTemplates { get; set; }
        public DbSet<MstAssessmentMethod> MstAssessmentMethods { get; set; }
        public DbSet<TrxJobRequisition> TrxJobRequisitions { get; set; }
        public DbSet<TrxJobRequisitionApproval> TrxJobRequisitionApprovals { get; set; }
        public DbSet<TrxJobVacancy> TrxJobVacancies { get; set; }
        public DbSet<TrxCandidate> TrxCandidates { get; set; }
        public DbSet<TrxCandidateApplication> TrxCandidateApplications { get; set; }
        public DbSet<TrxCandidateDocument> TrxCandidateDocuments { get; set; }
        public DbSet<TrxCandidateScreening> TrxCandidateScreenings { get; set; }
        public DbSet<TrxCandidateAssessment> TrxCandidateAssessments { get; set; }
        public DbSet<TrxCandidateInterview> TrxCandidateInterviews { get; set; }
        public DbSet<TrxInterviewEvaluation> TrxInterviewEvaluations { get; set; }
        public DbSet<TrxReferenceCheck> TrxReferenceChecks { get; set; }
        public DbSet<TrxBackgroundCheck> TrxBackgroundChecks { get; set; }
        public DbSet<TrxPreEmploymentMedicalCheck> TrxPreEmploymentMedicalChecks { get; set; }
        public DbSet<TrxJobOffer> TrxJobOffers { get; set; }
        public DbSet<TrxCandidateHiring> TrxCandidateHirings { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - RECRUITMENT MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - LIFECYCLE MANAGEMENT
        public DbSet<WfpOnboardingChecklist> WfpOnboardingChecklists { get; set; }
        public DbSet<WfpOnboardingTask> WfpOnboardingTasks { get; set; }
        public DbSet<WfpOffboardingChecklist> WfpOffboardingChecklists { get; set; }
        public DbSet<WfpOffboardingTask> WfpOffboardingTasks { get; set; }
        public DbSet<MstOnboardingTemplate> MstOnboardingTemplates { get; set; }
        public DbSet<MstOnboardingTemplateTask> MstOnboardingTemplateTasks { get; set; }
        public DbSet<MstOffboardingTemplate> MstOffboardingTemplates { get; set; }
        public DbSet<MstOffboardingTemplateTask> MstOffboardingTemplateTasks { get; set; }
        public DbSet<TrxEmployeeOnboarding> TrxEmployeeOnboardings { get; set; }
        public DbSet<TrxEmployeeOnboardingTask> TrxEmployeeOnboardingTasks { get; set; }
        public DbSet<TrxProbationReview> TrxProbationReviews { get; set; }
        public DbSet<TrxEmployeeSeparation> TrxEmployeeSeparations { get; set; }
        public DbSet<TrxResignationRequest> TrxResignationRequests { get; set; }
        public DbSet<TrxRetirement> TrxRetirements { get; set; }
        public DbSet<TrxContractNonRenewal> TrxContractNonRenewals { get; set; }
        public DbSet<TrxTermination> TrxTerminations { get; set; }
        public DbSet<TrxExitClearance> TrxExitClearances { get; set; }
        public DbSet<TrxAssetReturn> TrxAssetReturns { get; set; }
        public DbSet<TrxAccessRevocation> TrxAccessRevocations { get; set; }
        public DbSet<TrxExitInterview> TrxExitInterviews { get; set; }
        public DbSet<TrxEmploymentCertificateRequest> TrxEmploymentCertificateRequests { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - LIFECYCLE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - ATTENDANCE MANAGEMENT
        public DbSet<TrxAttendance> TrxAttendances { get; set; }
        public DbSet<TrxAttendanceRawLog> TrxAttendanceRawLogs { get; set; }
        public DbSet<TrxAttendanceProcessingRun> TrxAttendanceProcessingRuns { get; set; }
        public DbSet<TrxAttendancePeriod> TrxAttendancePeriods { get; set; }
        public DbSet<TrxAttendanceSchedulerJob> TrxAttendanceSchedulerJobs { get; set; }
        public DbSet<TrxAttendanceDaily> TrxAttendanceDailies { get; set; }
        public DbSet<TrxAttendanceDailySegment> TrxAttendanceDailySegments { get; set; }
        public DbSet<TrxAttendanceException> TrxAttendanceExceptions { get; set; }
        public DbSet<TrxAttendanceCorrectionRequest> TrxAttendanceCorrectionRequests { get; set; }
        public DbSet<TrxAttendanceCorrectionDetail> TrxAttendanceCorrectionDetails { get; set; }
        public DbSet<TrxAttendanceCorrectionApproval> TrxAttendanceCorrectionApprovals { get; set; }
        public DbSet<TrxMissingAttendance> TrxMissingAttendances { get; set; }
        public DbSet<TrxBusinessTripAttendance> TrxBusinessTripAttendances { get; set; }
        public DbSet<TrxRemoteAttendance> TrxRemoteAttendances { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - ATTENDANCE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - SCHEDULING MANAGEMENT
        public DbSet<WfpWorkScheduleAssignment> WfpWorkScheduleAssignments { get; set; }
        public DbSet<WfpScheduleChangeRequest> WfpScheduleChangeRequests { get; set; }
        public DbSet<WfpShiftSwapRequest> WfpShiftSwapRequests { get; set; }
        public DbSet<TrxRosterPeriod> TrxRosterPeriods { get; set; }
        public DbSet<TrxRosterAssignment> TrxRosterAssignments { get; set; }
        public DbSet<TrxRosterApproval> TrxRosterApprovals { get; set; }
        public DbSet<TrxRosterPublication> TrxRosterPublications { get; set; }
        public DbSet<TrxShiftAssignment> TrxShiftAssignments { get; set; }
        public DbSet<TrxOnCallAssignment> TrxOnCallAssignments { get; set; }
        public DbSet<TrxShiftReplacement> TrxShiftReplacements { get; set; }
        public DbSet<TrxEmergencyStaffingRequest> TrxEmergencyStaffingRequests { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - SCHEDULING MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - LEAVE MANAGEMENT
        public DbSet<WfpLeaveBalance> WfpLeaveBalances { get; set; }
        public DbSet<TrxLeaveEntitlementPeriod> TrxLeaveEntitlementPeriods { get; set; }
        public DbSet<WfpLeaveRequest> WfpLeaveRequests { get; set; }
        public DbSet<TrxLeaveEntitlement> TrxLeaveEntitlements { get; set; }
        public DbSet<TrxLeaveAccrualRun> TrxLeaveAccrualRuns { get; set; }
        public DbSet<TrxLeaveAccrual> TrxLeaveAccruals { get; set; }
        public DbSet<TrxLeaveCarryForwardRun> TrxLeaveCarryForwardRuns { get; set; }
        public DbSet<TrxLeaveCarryForward> TrxLeaveCarryForwards { get; set; }
        public DbSet<TrxLeaveAdjustment> TrxLeaveAdjustments { get; set; }
        public DbSet<TrxLeaveBalanceTransaction> TrxLeaveBalanceTransactions { get; set; }
        public DbSet<TrxLeaveRequestApproval> TrxLeaveRequestApprovals { get; set; }
        public DbSet<TrxLeaveRequestAttachment> TrxLeaveRequestAttachments { get; set; }
        public DbSet<TrxLeaveCancellationRequest> TrxLeaveCancellationRequests { get; set; }
        public DbSet<TrxLeaveRecall> TrxLeaveRecalls { get; set; }
        public DbSet<TrxCompensatoryLeave> TrxCompensatoryLeaves { get; set; }
        public DbSet<TrxLeaveExecution> TrxLeaveExecutions { get; set; }
        public DbSet<TrxLeaveAttendanceIntegration> TrxLeaveAttendanceIntegrations { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - LEAVE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - OVERTIME MANAGEMENT
        public DbSet<TrxOvertimePlan> TrxOvertimePlans { get; set; }
        public DbSet<TrxOvertimePlanDetail> TrxOvertimePlanDetails { get; set; }
        public DbSet<WfpOvertimeRequest> WfpOvertimeRequests { get; set; }
        public DbSet<TrxOvertimeRequestDetail> TrxOvertimeRequestDetails { get; set; }
        public DbSet<TrxOvertimeRequestApproval> TrxOvertimeRequestApprovals { get; set; }
        public DbSet<TrxOvertimeRealization> TrxOvertimeRealizations { get; set; }
        public DbSet<TrxOvertimeRealizationDetail> TrxOvertimeRealizationDetails { get; set; }
        public DbSet<TrxOvertimeVerification> TrxOvertimeVerifications { get; set; }
        public DbSet<TrxCompensatoryTimeOff> TrxCompensatoryTimeOffs { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - OVERTIME MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - BUSINESS TRAVEL MANAGEMENT
        public DbSet<TrxBusinessTravelRequest> TrxBusinessTravelRequests { get; set; }
        public DbSet<TrxBusinessTravelParticipant> TrxBusinessTravelParticipants { get; set; }
        public DbSet<TrxBusinessTravelApproval> TrxBusinessTravelApprovals { get; set; }
        public DbSet<TrxTravelItinerary> TrxTravelItineraries { get; set; }
        public DbSet<TrxTravelTransportation> TrxTravelTransportations { get; set; }
        public DbSet<TrxTravelAccommodation> TrxTravelAccommodations { get; set; }
        public DbSet<TrxTravelAdvanceRequest> TrxTravelAdvanceRequests { get; set; }
        public DbSet<TrxTravelAdvancePayment> TrxTravelAdvancePayments { get; set; }
        public DbSet<TrxTravelExpenseClaim> TrxTravelExpenseClaims { get; set; }
        public DbSet<TrxTravelExpenseItem> TrxTravelExpenseItems { get; set; }
        public DbSet<TrxTravelSettlement> TrxTravelSettlements { get; set; }
        public DbSet<TrxTravelDocument> TrxTravelDocuments { get; set; }
        public DbSet<TrxTravelAttendanceLink> TrxTravelAttendanceLinks { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - BUSINESS TRAVEL MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - EXPENSE MANAGEMENT
        public DbSet<TrxExpenseClaim> TrxExpenseClaims { get; set; }
        public DbSet<TrxExpenseClaimItem> TrxExpenseClaimItems { get; set; }
        public DbSet<TrxExpenseReceipt> TrxExpenseReceipts { get; set; }
        public DbSet<TrxExpenseApproval> TrxExpenseApprovals { get; set; }
        public DbSet<TrxExpenseVerification> TrxExpenseVerifications { get; set; }
        public DbSet<TrxExpensePayment> TrxExpensePayments { get; set; }
        public DbSet<TrxExpenseReversal> TrxExpenseReversals { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - EXPENSE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - PAYROLL MANAGEMENT
        public DbSet<WfpPayroll> WfpPayrolls { get; set; }
        public DbSet<WfpTax> WfpTaxes { get; set; }
        public DbSet<WfpInsurance> WfpInsurances { get; set; }
        public DbSet<WfpTransportAllowancePolicy> WfpTransportAllowancePolicies { get; set; }
        public DbSet<WfpTransportAllowance> WfpTransportAllowances { get; set; }
        public DbSet<WfpTransportAllowanceTransaction> WfpTransportAllowanceTransactions { get; set; }
        public DbSet<TrxPayrollRun> TrxPayrollRuns { get; set; }
        public DbSet<TrxPayrollRunEmployee> TrxPayrollRunEmployees { get; set; }
        public DbSet<TrxPayrollEmployeeComponent> TrxPayrollEmployeeComponents { get; set; }
        public DbSet<TrxPayrollAttendanceInput> TrxPayrollAttendanceInputs { get; set; }
        public DbSet<TrxPayrollOvertimeInput> TrxPayrollOvertimeInputs { get; set; }
        public DbSet<TrxPayrollVariableInput> TrxPayrollVariableInputs { get; set; }
        public DbSet<TrxPayrollAdjustment> TrxPayrollAdjustments { get; set; }
        public DbSet<TrxPayrollApproval> TrxPayrollApprovals { get; set; }
        public DbSet<TrxPayrollPayment> TrxPayrollPayments { get; set; }
        public DbSet<TrxPayrollPayslip> TrxPayrollPayslips { get; set; }
        public DbSet<TrxPayrollReversal> TrxPayrollReversals { get; set; }
        public DbSet<TrxMedicalServiceFeeCalculation> TrxMedicalServiceFeeCalculations { get; set; }
        public DbSet<TrxMedicalServiceFeePayment> TrxMedicalServiceFeePayments { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - PAYROLL MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - BENEFIT MANAGEMENT
        public DbSet<TrxEmployeeBenefitEnrollment> TrxEmployeeBenefitEnrollments { get; set; }
        public DbSet<TrxEmployeeBenefitDependent> TrxEmployeeBenefitDependents { get; set; }
        public DbSet<TrxEmployeeInsuranceEnrollment> TrxEmployeeInsuranceEnrollments { get; set; }
        public DbSet<TrxBenefitClaim> TrxBenefitClaims { get; set; }
        public DbSet<TrxBenefitClaimItem> TrxBenefitClaimItems { get; set; }
        public DbSet<TrxBenefitClaimDocument> TrxBenefitClaimDocuments { get; set; }
        public DbSet<TrxBenefitClaimApproval> TrxBenefitClaimApprovals { get; set; }
        public DbSet<TrxEmployeeLoan> TrxEmployeeLoans { get; set; }
        public DbSet<TrxEmployeeLoanInstallment> TrxEmployeeLoanInstallments { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - BENEFIT MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - CREDENTIALING MANAGEMENT
        public DbSet<WfpCertification> WfpCertifications { get; set; }
        public DbSet<WfpCredentialLicense> WfpCredentialLicenses { get; set; }
        public DbSet<WfpClinicalPrivilege> WfpClinicalPrivileges { get; set; }
        public DbSet<WfpComplianceAlert> WfpComplianceAlerts { get; set; }
        public DbSet<WfpComplianceAlertLog> WfpComplianceAlertLogs { get; set; }
        public DbSet<TrxCredentialingApplication> TrxCredentialingApplications { get; set; }
        public DbSet<TrxCredentialingDocument> TrxCredentialingDocuments { get; set; }
        public DbSet<TrxCredentialingVerification> TrxCredentialingVerifications { get; set; }
        public DbSet<TrxCredentialingCommitteeReview> TrxCredentialingCommitteeReviews { get; set; }
        public DbSet<TrxCredentialingDecision> TrxCredentialingDecisions { get; set; }
        public DbSet<TrxRecredentialingApplication> TrxRecredentialingApplications { get; set; }
        public DbSet<TrxLicenseRenewalRequest> TrxLicenseRenewalRequests { get; set; }
        public DbSet<TrxCertificationRenewalRequest> TrxCertificationRenewalRequests { get; set; }
        public DbSet<TrxClinicalPrivilegeRequest> TrxClinicalPrivilegeRequests { get; set; }
        public DbSet<TrxClinicalPrivilegeAssessment> TrxClinicalPrivilegeAssessments { get; set; }
        public DbSet<TrxClinicalPrivilegeApproval> TrxClinicalPrivilegeApprovals { get; set; }
        public DbSet<TrxClinicalPrivilegeSuspension> TrxClinicalPrivilegeSuspensions { get; set; }
        public DbSet<TrxClinicalPrivilegeRevocation> TrxClinicalPrivilegeRevocations { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - CREDENTIALING MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - LEARNING AND DEVELOPMENT
        public DbSet<WfpTrainingRecord> WfpTrainingRecords { get; set; }
        public DbSet<WfpCompetencyAssessment> WfpCompetencyAssessments { get; set; }
        public DbSet<TrxTrainingPlan> TrxTrainingPlans { get; set; }
        public DbSet<TrxTrainingSession> TrxTrainingSessions { get; set; }
        public DbSet<TrxTrainingParticipant> TrxTrainingParticipants { get; set; }
        public DbSet<TrxTrainingEnrollmentRequest> TrxTrainingEnrollmentRequests { get; set; }
        public DbSet<TrxTrainingAttendance> TrxTrainingAttendances { get; set; }
        public DbSet<TrxTrainingAssessment> TrxTrainingAssessments { get; set; }
        public DbSet<TrxTrainingResult> TrxTrainingResults { get; set; }
        public DbSet<TrxTrainingEvaluation> TrxTrainingEvaluations { get; set; }
        public DbSet<TrxTrainingCertificate> TrxTrainingCertificates { get; set; }
        public DbSet<TrxIndividualDevelopmentPlan> TrxIndividualDevelopmentPlans { get; set; }
        public DbSet<TrxTrainingBudget> TrxTrainingBudgets { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - LEARNING AND DEVELOPMENT

        #region CORPORATE - HUMAN RESOURCE - PERFORMANCE MANAGEMENT
        public DbSet<WfpPerformanceReview> WfpPerformanceReviews { get; set; }
        public DbSet<WfpPerformanceReviewDetail> WfpPerformanceReviewDetails { get; set; }
        public DbSet<TrxPerformanceCycle> TrxPerformanceCycles { get; set; }
        public DbSet<TrxEmployeeGoal> TrxEmployeeGoals { get; set; }
        public DbSet<TrxEmployeeKpiTarget> TrxEmployeeKpiTargets { get; set; }
        public DbSet<TrxSelfAssessment> TrxSelfAssessments { get; set; }
        public DbSet<TrxManagerAssessment> TrxManagerAssessments { get; set; }
        public DbSet<TrxPeerFeedback> TrxPeerFeedbacks { get; set; }
        public DbSet<TrxPerformanceCheckIn> TrxPerformanceCheckIns { get; set; }
        public DbSet<TrxCalibrationSession> TrxCalibrationSessions { get; set; }
        public DbSet<TrxPerformanceImprovementPlan> TrxPerformanceImprovementPlans { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - PERFORMANCE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - OCCUPATIONAL HEALTH MANAGEMENT
        public DbSet<WfpHealthRecord> WfpHealthRecords { get; set; }
        public DbSet<TrxEmployeeMedicalExamination> TrxEmployeeMedicalExaminations { get; set; }
        public DbSet<TrxEmployeeVaccination> TrxEmployeeVaccinations { get; set; }
        public DbSet<TrxEmployeeFitnessToWork> TrxEmployeeFitnessToWorks { get; set; }
        public DbSet<TrxWorkRestriction> TrxWorkRestrictions { get; set; }
        public DbSet<TrxOccupationalExposure> TrxOccupationalExposures { get; set; }
        public DbSet<TrxNeedleStickIncident> TrxNeedleStickIncidents { get; set; }
        public DbSet<TrxEmployeeInjury> TrxEmployeeInjuries { get; set; }
        public DbSet<TrxReturnToWorkAssessment> TrxReturnToWorkAssessments { get; set; }
        public DbSet<TrxEmployeeHealthSurveillance> TrxEmployeeHealthSurveillances { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - OCCUPATIONAL HEALTH MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - EMPLOYEE RELATION MANAGEMENT
        public DbSet<MstDisciplinaryActionType> MstDisciplinaryActionTypes { get; set; }
        public DbSet<MstViolationType> MstViolationTypes { get; set; }
        public DbSet<MstSanctionType> MstSanctionTypes { get; set; }
        public DbSet<MstEmployeeRelationCaseType> MstEmployeeRelationCaseTypes { get; set; }
        public DbSet<WfpDisciplinaryAction> WfpDisciplinaryActions { get; set; }
        public DbSet<TrxEmployeeIncidentReport> TrxEmployeeIncidentReports { get; set; }
        public DbSet<TrxEmployeeGrievance> TrxEmployeeGrievances { get; set; }
        public DbSet<TrxWorkplaceInvestigation> TrxWorkplaceInvestigations { get; set; }
        public DbSet<TrxInvestigationEvidence> TrxInvestigationEvidences { get; set; }
        public DbSet<TrxDisciplinaryCase> TrxDisciplinaryCases { get; set; }
        public DbSet<TrxDisciplinaryDecision> TrxDisciplinaryDecisions { get; set; }
        public DbSet<TrxEmployeeRecognition> TrxEmployeeRecognitions { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - EMPLOYEE RELATION MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - HR SERVICE MANAGEMENT
        public DbSet<MstHrServiceCategory> MstHrServiceCategories { get; set; }
        public DbSet<MstHrServiceType> MstHrServiceTypes { get; set; }
        public DbSet<MstEmployeeDocumentType> MstEmployeeDocumentTypes { get; set; }
        public DbSet<TrxHrServiceRequest> TrxHrServiceRequests { get; set; }
        public DbSet<TrxHrServiceRequestComment> TrxHrServiceRequestComments { get; set; }
        public DbSet<TrxHrServiceRequestAttachment> TrxHrServiceRequestAttachments { get; set; }
        public DbSet<TrxEmployeeDocumentRequest> TrxEmployeeDocumentRequests { get; set; }
        public DbSet<TrxEmployeeDocumentIssuance> TrxEmployeeDocumentIssuances { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - HR SERVICE MANAGEMENT

        #region CORPORATE - HUMAN RESOURCE - WORKFLOW MANAGEMENT
        public DbSet<MstWorkflowDefinition> MstWorkflowDefinitions { get; set; }
        public DbSet<MstWorkflowStep> MstWorkflowSteps { get; set; }
        public DbSet<MstApprovalMatrix> MstApprovalMatrices { get; set; }
        public DbSet<TrxWorkflowInstance> TrxWorkflowInstances { get; set; }
        public DbSet<TrxWorkflowStepInstance> TrxWorkflowStepInstances { get; set; }
        public DbSet<TrxApprovalAction> TrxApprovalActions { get; set; }
        public DbSet<TrxApprovalDelegation> TrxApprovalDelegations { get; set; }
        public DbSet<TrxWorkflowComment> TrxWorkflowComments { get; set; }
        public DbSet<TrxWorkflowAttachment> TrxWorkflowAttachments { get; set; }
        public DbSet<TrxWorkflowStatusHistory> TrxWorkflowStatusHistories { get; set; }
        public DbSet<TrxWorkflowApproverAssignment> TrxWorkflowApproverAssignments { get; set; }
        #endregion CORPORATE - HUMAN RESOURCE - WORKFLOW MANAGEMENT

        #endregion CORPORATE

        #region HEALTH SERVICE        
        public DbSet<MstAgeCategory> MstAgeCategories { get; set; }
        public DbSet<MstServiceUnit> MstServiceUnits { get; set; }
        public DbSet<MstClinic> MstClinics { get; set; }
        public DbSet<MstPatientClass> MstPatientClasses { get; set; }
        public DbSet<MstTariffCategory> MstTariffCategories { get; set; }
        public DbSet<MstTariff> MstTariffs { get; set; }
        public DbSet<MstRoom> MstRooms { get; set; }
        public DbSet<MstBed> MstBeds { get; set; }
        public DbSet<MstMembershipTier> MstMembershipTiers { get; set; }
        public DbSet<MstPatient> MstPatients { get; set; }
        public DbSet<MstPatientMembership> MstPatientMemberships { get; set; }
        public DbSet<MstPatientIdentityDocument> MstPatientIdentityDocuments { get; set; }
        public DbSet<MstPatientRelationship> MstPatientRelationships { get; set; }
        public DbSet<MstPatientEmergencyContact> MstPatientEmergencyContacts { get; set; }
        public DbSet<MstInsuranceProvider> MstInsuranceProviders { get; set; }
        public DbSet<MstPatientInsurance> MstPatientInsurances { get; set; }
        public DbSet<MstCompanyGuarantor> MstCompanyGuarantors { get; set; }
        public DbSet<MstPatientCompanyGuarantor> MstPatientCompanyGuarantors { get; set; }
        public DbSet<MstPaymentMethod> MstPaymentMethods { get; set; }
        public DbSet<MstBillingItemCategory> MstBillingItemCategories { get; set; }
        public DbSet<MstProcedure> MstProcedures { get; set; }
        public DbSet<MstDiagnosisChapter> MstDiagnosisChapters { get; set; }
        public DbSet<MstDiagnosis> MstDiagnoses { get; set; }
        public DbSet<MstMeasurement> MstMeasurements { get; set; }
        public DbSet<MstMeasurementConversion> MstMeasurementConversions { get; set; }
        public DbSet<MstDrugUnitConversion> MstDrugUnitConversions { get; set; }
        public DbSet<MstDrugStorageLocation> MstDrugStorageLocations { get; set; }
        public DbSet<MstDrugStockPolicy> MstDrugStockPolicies { get; set; }
        public DbSet<MstDrugCategory> MstDrugCategories { get; set; }
        public DbSet<MstDrug> MstDrugs { get; set; }
        public DbSet<MstInsuranceCoverageRule> MstInsuranceCoverageRules { get; set; }
        public DbSet<MstInsuranceTariff> MstInsuranceTariffs { get; set; }
        public DbSet<MstDoctorSchedule> MstDoctorSchedules { get; set; }
        public DbSet<MstDoctorServiceRule> MstDoctorServiceRules { get; set; }
        public DbSet<TrxKioskScanSession> TrxKioskScanSessions { get; set; }
        public DbSet<TrxPatientEncounter> TrxPatientEncounters { get; set; }
        public DbSet<TrxPatientEncounterGuarantor> TrxPatientEncounterGuarantors { get; set; }
        public DbSet<TrxQueue> TrxQueues { get; set; }
        public DbSet<TrxPatientAssessment> TrxPatientAssessments { get; set; }
        public DbSet<TrxDoctorConsultation> TrxDoctorConsultations { get; set; }
        public DbSet<TrxPatientDiagnosis> TrxPatientDiagnoses { get; set; }
        public DbSet<TrxPatientProcedure> TrxPatientProcedures { get; set; }
        public DbSet<TrxPatientAllergy> TrxPatientAllergies { get; set; }
        public DbSet<TrxPatientMedicalHistory> TrxPatientMedicalHistories { get; set; }
        public DbSet<TrxPatientFamilyHistory> TrxPatientFamilyHistories { get; set; }
        public DbSet<TrxPatientVitalSign> TrxPatientVitalSigns { get; set; }
        public DbSet<TrxPatientClinicalDocument> TrxPatientClinicalDocuments { get; set; }
        public DbSet<TrxPatientConsent> TrxPatientConsents { get; set; }
        public DbSet<TrxMedicalCertificate> TrxMedicalCertificates { get; set; }
        public DbSet<TrxClinicalNoteAttachment> TrxClinicalNoteAttachments { get; set; }
        public DbSet<TrxPatientIntegratedProgressNote> TrxPatientIntegratedProgressNotes { get; set; }
        public DbSet<TrxPrescription> TrxPrescriptions { get; set; }
        public DbSet<TrxPrescriptionItem> TrxPrescriptionItems { get; set; }
        public DbSet<TrxPrescriptionCompound> TrxPrescriptionCompounds { get; set; }
        public DbSet<TrxPrescriptionCompoundItem> TrxPrescriptionCompoundItems { get; set; }
        public DbSet<MstPrescriptionTemplate> MstPrescriptionTemplates { get; set; }
        public DbSet<MstPrescriptionTemplateItem> MstPrescriptionTemplateItems { get; set; }
        public DbSet<MstPrescriptionTemplateCompound> MstPrescriptionTemplateCompounds { get; set; }
        public DbSet<MstPrescriptionTemplateCompoundItem> MstPrescriptionTemplateCompoundItems { get; set; }
        public DbSet<MstPrescriptionReviewCriterion> MstPrescriptionReviewCriterions { get; set; }
        public DbSet<TrxPrescriptionReviewItem> TrxPrescriptionReviewItems { get; set; }
        public DbSet<TrxPrescriptionReview> TrxPrescriptionReviews { get; set; }
        public DbSet<TrxPrescriptionPreparation> TrxPrescriptionPreparations { get; set; }
        public DbSet<TrxPrescriptionPreparationItem> TrxPrescriptionPreparationItems { get; set; }
        public DbSet<TrxPrescriptionFinalCheck> TrxPrescriptionFinalChecks { get; set; }
        public DbSet<TrxPrescriptionFinalCheckItem> TrxPrescriptionFinalCheckItems { get; set; }
        public DbSet<TrxPrescriptionDrugSubstitution> TrxPrescriptionDrugSubstitutions { get; set; }
        public DbSet<TrxPrescriptionClarification> TrxPrescriptionClarifications { get; set; }

        #region HEALTH SERVICE - Emergency Installation Management

        #region master
        public DbSet<MstEmergencyTriageLevel> MstEmergencyTriageLevels { get; set; }
        public DbSet<MstEmergencyTriageIndicator> MstEmergencyTriageIndicators { get; set; }
        public DbSet<MstEmergencyArrivalMode> MstEmergencyArrivalModes { get; set; }
        public DbSet<MstEmergencyCaseType> MstEmergencyCaseTypes { get; set; }
        public DbSet<MstEmergencyDispositionType> MstEmergencyDispositionTypes { get; set; }
        public DbSet<MstEmergencySetting> MstEmergencySettings { get; set; }
        #endregion

        #region transaction
        public DbSet<TrxEmergencyVisit> TrxEmergencyVisits { get; set; }
        public DbSet<TrxEmergencyTriage> TrxEmergencyTriages { get; set; }
        public DbSet<TrxEmergencyTriageDetail> TrxEmergencyTriageDetails { get; set; }
        public DbSet<TrxEmergencyResuscitation> TrxEmergencyResuscitations { get; set; }
        public DbSet<TrxEmergencyObservation> TrxEmergencyObservations { get; set; }
        public DbSet<TrxEmergencyObservationDetail> TrxEmergencyObservationDetails { get; set; }
        public DbSet<TrxEmergencyProcedureDetail> TrxEmergencyProcedureDetails { get; set; }
        public DbSet<TrxEmergencyDisposition> TrxEmergencyDispositions { get; set; }
        public DbSet<TrxEmergencyTransfer> TrxEmergencyTransfers { get; set; }
        #endregion

        #endregion

        #endregion HEALTH SERVICE

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
