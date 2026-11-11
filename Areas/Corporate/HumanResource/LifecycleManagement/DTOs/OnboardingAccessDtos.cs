using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class OnboardingAccessQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OnboardingAccessRequestType? RequestType { get; set; }
        public OnboardingAccessRequestStatus? RequestStatus { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? Search { get; set; }
    }

    public sealed class CompleteAccessRequest
    {
        public string? CompletionNote { get; set; }
    }

    public sealed class OnboardingAccessListItemResponse
    {
        public Guid Id { get; set; }
        public Guid OnboardingCaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public OnboardingAccessRequestType RequestType { get; set; }
        public string RequestTypeName => RequestType.ToString();
        public OnboardingAccessRequestStatus RequestStatus { get; set; }
        public string RequestStatusName => RequestStatus.ToString();
        public Guid? ApplicationUserId { get; set; }
        public string? FailureMessage { get; set; }
        public DateTime? SelfServiceActivatedAt { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? CompletedByUserName { get; set; }
        public string? CompletionNote { get; set; }
    }
}
