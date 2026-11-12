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
        public DateTime? AutomaticCheckedAt { get; set; }
        public string? AutomaticResultNote { get; set; }
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
    }

    public sealed class DeclareReadyForWorkRequest
    {
        public string? Note { get; set; }
    }
}
