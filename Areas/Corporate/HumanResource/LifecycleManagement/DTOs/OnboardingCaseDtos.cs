using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class OnboardingCaseSummaryResponse
    {
        public int TotalCases { get; set; }
        public int InProgressCases { get; set; }
        public int ReadyForWorkCases { get; set; }
        public int CancelledCases { get; set; }
    }

    public sealed class OnboardingCaseListItemResponse
    {
        public Guid Id { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string OrganizationUnitName { get; set; } = string.Empty;
        public bool IsClinical { get; set; }
        public DateOnly PlannedStartDate { get; set; }
        public OnboardingCaseStatus CaseStatus { get; set; }
        public string CaseStatusName => CaseStatus.ToString();
        public int CompletedItemCount { get; set; }
        public int TotalItemCount { get; set; }
    }

    public sealed class OnboardingCaseItemResponse
    {
        public Guid Id { get; set; }
        public Guid OnboardingCaseId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public OnboardingTemplateLayer TemplateLayer { get; set; }
        public string TemplateLayerName => TemplateLayer.ToString();
        public string? TaskCategory { get; set; }
        public string? Description { get; set; }
        public string? SystemItemCode { get; set; }
        public bool IsSystemItem => !string.IsNullOrWhiteSpace(SystemItemCode);
        public OnboardingResponsibleRole ResponsibleRole { get; set; }
        public string ResponsibleRoleName => ResponsibleRole.ToString();
        public OnboardingItemKind ItemKind { get; set; }
        public string ItemKindName => ItemKind.ToString();
        public OnboardingAutomaticSource? AutomaticSource { get; set; }
        public string? AutomaticSourceName => AutomaticSource?.ToString();
        public Guid? CertificationTypeId { get; set; }
        public string? CertificationTypeName { get; set; }
        public bool IsRequired { get; set; }
        public int DueDayOffset { get; set; }
        public DateOnly DueDate { get; set; }
        public OnboardingItemStatus ItemStatus { get; set; }
        public string ItemStatusName => ItemStatus.ToString();
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? CompletedByUserName { get; set; }
        public string? CompletionNote { get; set; }
        public Guid? SelectedWorkforceProfileId { get; set; }
        public string? SelectedWorkforceFullName { get; set; }
        public string? EvidenceStoragePath { get; set; }
        public string? EvidenceFileName { get; set; }
        public bool HasEvidence => !string.IsNullOrWhiteSpace(EvidenceStoragePath);
        public DateTime? AutomaticCheckedAt { get; set; }
        public string? AutomaticResultNote { get; set; }
        public bool CanComplete { get; set; }
        public bool CanUncomplete { get; set; }
        public bool IsBlockedByRole { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class OnboardingAccessRequestResponse
    {
        public Guid Id { get; set; }
        public OnboardingAccessRequestType RequestType { get; set; }
        public string RequestTypeName => RequestType.ToString();
        public OnboardingAccessRequestStatus RequestStatus { get; set; }
        public string RequestStatusName => RequestStatus.ToString();
        public Guid? ApplicationUserId { get; set; }
        public string? FailureMessage { get; set; }
        public DateTime? SelfServiceActivatedAt { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public sealed class OnboardingCaseProgressDto
    {
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public int RequiredTotal { get; set; }
        public int RequiredCompleted { get; set; }
        public int OptionalTotal { get; set; }
        public int OptionalCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public string ProgressLabel { get; set; } = string.Empty;
        public bool LateStartFlagged { get; set; }
        public int DaysPastPlannedStart { get; set; }
    }

    public sealed class OnboardingBlockerDto
    {
        public string Code { get; set; } = string.Empty;
        public string StageKey { get; set; } = string.Empty;
        public string Severity { get; set; } = "blocking"; // blocking | warning | info
        public string Message { get; set; } = string.Empty;
        public string? ActionHint { get; set; }
        public string? TargetRoute { get; set; }
    }

    public sealed class OnboardingDeepLinkDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? PermissionResource { get; set; }
        public string? PermissionAction { get; set; }
        public string? Note { get; set; }
    }

    public sealed class OnboardingWizardStageDto
    {
        public int Order { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // completed | in_progress | pending | blocked | warning | cancelled
        public string ResponsibleRoleLabel { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string NextActionLabel { get; set; } = string.Empty;
        public string NextActionHint { get; set; } = string.Empty;
        public List<string> AllowedActions { get; set; } = new();
        public List<OnboardingBlockerDto> Blockers { get; set; } = new();
        public List<OnboardingDeepLinkDto> DeepLinks { get; set; } = new();
    }

    public sealed class OnboardingReadinessDto
    {
        public bool Eligible { get; set; }
        public bool CanDeclare { get; set; }
        public bool AlreadyReady { get; set; }
        public bool FourEyesViolation { get; set; }
        public string? FourEyesMessage { get; set; }
        public DateTime EvaluatedAtUtc { get; set; }
        public Guid? SupervisorAssignedByUserId { get; set; }
        public string? SupervisorAssignedByName { get; set; }
        public Guid? SupervisorSelectedProfileId { get; set; }
        public string? SupervisorSelectedName { get; set; }
        public List<OnboardingBlockerDto> Blockers { get; set; } = new();
        public List<OnboardingBlockerDto> Warnings { get; set; } = new();
    }

    public sealed class OnboardingAcceptanceSummaryDto
    {
        public Guid HandoffId { get; set; }
        public string HandoffNumber { get; set; } = string.Empty;
        public DateTime? HandoffDate { get; set; }
        public string HandoffStatus { get; set; } = string.Empty;
        public Guid? OfferId { get; set; }
        public string? OfferNumber { get; set; }
        public string? OfferStatus { get; set; }
        public Guid? CandidateId { get; set; }
        public string? CandidateNumber { get; set; }
        public string? VacancyTitle { get; set; }
        public string? RequisitionNumber { get; set; }
        public bool IsRehire { get; set; }
        public Guid? PreviousWorkforceProfileId { get; set; }
        public string McuClearanceStatus { get; set; } = string.Empty;
        public string McuClearanceLabel { get; set; } = string.Empty;
        public bool HasMedicalRestrictions { get; set; }
        public string? CandidateRoute { get; set; }
    }

    public sealed class OnboardingEmployeeDataGroupDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = "NeedsCompletion"; // Complete | NeedsCompletion | NotApplicable | Unverified | AccessRestricted
        public string OwnerLabel { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public int VerifiedCount { get; set; }
        public string Summary { get; set; } = string.Empty;
        public bool IsMasked { get; set; }
        public string? TargetRoute { get; set; }
    }

    public sealed class OnboardingEmployeeDataSummaryDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string UserTypeName { get; set; } = string.Empty;
        public bool IsProfileActive { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public bool HasMasterRecord => EmployeeId.HasValue || DoctorId.HasValue;
        public string? MaskedIdentityNumber { get; set; }
        public string? MaskedEmail { get; set; }
        public string? MaskedPhoneNumber { get; set; }
        public string? PrimaryWorkforceRoute { get; set; }
        public string? Employee360Route { get; set; }
        public int CompletedGroupCount { get; set; }
        public int ApplicableGroupCount { get; set; }
        public List<OnboardingEmployeeDataGroupDto> Groups { get; set; } = new();
    }

    public sealed class OnboardingContractSummaryDto
    {
        public Guid? ContractId { get; set; }
        public string? ContractNumber { get; set; }
        public string? ContractTypeName { get; set; }
        public string? HistoryType { get; set; }
        public string? ContractStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public bool IsProbationApplicable { get; set; }
        public bool HasSignedDocument { get; set; }
        public string? DocumentFileName { get; set; }
        public bool IsContractValidForOnboarding { get; set; }
        public bool CanViewAmount { get; set; }
        public string? SalaryDisplay { get; set; }
        public decimal? BaseSalaryAmount { get; set; }
        public string? ContractRoute { get; set; }
    }

    public sealed class OnboardingPlacementSummaryDto
    {
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public string? WorkLocation { get; set; }
        public bool HasActiveOrganizationAssignment { get; set; }
        public bool HasActivePositionAssignment { get; set; }
        public Guid? JobDescriptionId { get; set; }
        public string? JobDescriptionCode { get; set; }
        public string? JobDescriptionPurpose { get; set; }
        public Guid? DirectSupervisorProfileId { get; set; }
        public string? DirectSupervisorName { get; set; }
        public bool HasActiveManagerAssignment { get; set; }
        public string? ActiveManagerName { get; set; }
        public bool HasManagerConflict { get; set; }
        public string? ManagerConflictNote { get; set; }
    }

    public sealed class OnboardingPreceptorshipSummaryDto
    {
        public Guid? PreceptorshipId { get; set; }
        public string PreceptorshipStatus { get; set; } = "NotAssigned";
        public string? CurrentPreceptorName { get; set; }
        public Guid? CurrentPreceptorProfileId { get; set; }
        public DateOnly? AccompaniedSince { get; set; }
        public DateOnly? IndependentOn { get; set; }
        public bool HasCredentialLicense { get; set; }
        public string? CredentialWarning { get; set; }
        public string? PreceptorshipRoute { get; set; }
    }

    public sealed class OnboardingActorPermissionsDto
    {
        public bool CanViewAllUnits { get; set; }
        public bool IsHrOperator { get; set; }
        public bool IsUnitHeadForCase { get; set; }
        public bool CanRecordAttendance { get; set; }
        public bool CanCancelAttendance { get; set; }
        public bool CanCompleteHrItems { get; set; }
        public bool CanCompleteUnitItems { get; set; }
        public bool CanCheckAutomaticItems { get; set; }
        public bool CanReassignCoordinator { get; set; }
        public bool CanDeclareReady { get; set; }
        public bool CanCancelJoin { get; set; }
        public bool CanViewContractAmount { get; set; }
        public bool CanRetryAccountCreation { get; set; }
        public bool CanCompleteWorkAccess { get; set; }
        public bool CanAssignPreceptor { get; set; }
    }

    public sealed class OnboardingCoordinatorOptionDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Guid? WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
    }

    public sealed class OnboardingCaseDetailResponse
    {
        public Guid Id { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public Guid HiringHandoffId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public bool IsClinical { get; set; }
        public DateOnly PlannedStartDate { get; set; }
        public Guid CoordinatorUserId { get; set; }
        public string CoordinatorUserName { get; set; } = string.Empty;
        public OnboardingCaseStatus CaseStatus { get; set; }
        public string CaseStatusName => CaseStatus.ToString();
        public DateOnly? FirstDayAttendedOn { get; set; }
        public DateTime? ReadyForWorkAt { get; set; }
        public OnboardingCancelReason? CancelReason { get; set; }
        public string? CancelReasonName => CancelReason?.ToString();
        public string? CancelNote { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool HasGeneralTemplate { get; set; }
        public List<string> AvailableActions { get; set; } = new();
        public List<OnboardingCaseItemResponse> Items { get; set; } = new();
        public List<OnboardingAccessRequestResponse> AccessRequests { get; set; } = new();
        public Guid RowVersion { get; set; }

        // Extended 7-stage wizard fields (FIN-HRD-ONB-WIZ-01)
        public OnboardingCaseProgressDto Progress { get; set; } = new();
        public OnboardingReadinessDto Readiness { get; set; } = new();
        public List<OnboardingWizardStageDto> Stages { get; set; } = new();
        public OnboardingAcceptanceSummaryDto? AcceptanceSummary { get; set; }
        public OnboardingEmployeeDataSummaryDto? EmployeeDataSummary { get; set; }
        public OnboardingContractSummaryDto? ContractSummary { get; set; }
        public OnboardingPlacementSummaryDto? PlacementSummary { get; set; }
        public OnboardingPreceptorshipSummaryDto? PreceptorshipSummary { get; set; }
        public OnboardingActorPermissionsDto ActorContext { get; set; } = new();
    }

    public sealed class OnboardingCaseQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OnboardingCaseStatus? CaseStatus { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public bool? IsClinical { get; set; }
        public string? Search { get; set; }
    }

    public sealed class RecordFirstDayAttendanceRequest
    {
        public DateOnly AttendedDate { get; set; }
        public Guid? RowVersion { get; set; }
    }

    public sealed class CompleteChecklistItemRequest
    {
        public string? Note { get; set; }
        public Guid? SelectedWorkforceProfileId { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? EvidenceFile { get; set; }
    }

    public sealed class ReassignCoordinatorRequest
    {
        public Guid NewCoordinatorUserId { get; set; }
    }

    public sealed class CancelOnboardingJoinRequest
    {
        public OnboardingCancelReason CancelReason { get; set; }
        public string CancelNote { get; set; } = string.Empty;
        public Guid? RowVersion { get; set; }
    }

    public sealed class DeclareReadyForWorkRequest
    {
        public string? Note { get; set; }
        public Guid? RowVersion { get; set; }
    }
}
