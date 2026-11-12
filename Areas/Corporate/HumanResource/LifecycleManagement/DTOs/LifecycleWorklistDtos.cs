using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class SchedulingEligibilityResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly TargetDate { get; set; }
        public string EligibilityStatus { get; set; } = "Eligible"; // "Eligible", "NotReady", "ContractHold"
        public bool IsEligible => EligibilityStatus == "Eligible";
        public bool IsAccompanied { get; set; }
        public string? AccompanyingNote { get; set; }
        public string? CredentialWarning { get; set; }
        public string? Reason { get; set; }
    }

    public sealed class LifecycleWorklistQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Category { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? Search { get; set; }
    }

    public sealed class LifecycleWorklistItemResponse
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty; // "Contract", "Probation", "OnboardingCase"
        public Guid TargetId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string OrganizationUnitName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly? DueDate { get; set; }
        public string UrgencyLevel { get; set; } = "Normal"; // "Low", "Normal", "High", "Critical"
    }

    public sealed class LifecycleWorklistCountResponse
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
    }
}
