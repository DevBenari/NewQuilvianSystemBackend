using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimePolicyResolveRequest
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    public class OvertimePolicyResolutionContextResponse
    {
        public Guid? WorkforceProfileId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class OvertimePolicyResolutionCandidateResponse
    {
        public Guid Id { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public bool IsDefault { get; set; }
        public int SpecificityScore { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    public class OvertimePolicyResolutionResponse
    {
        public bool IsResolved { get; set; }
        public bool IsAmbiguous { get; set; }
        public string ResolutionSource { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public OvertimePolicyResolutionContextResponse Context { get; set; } = new();
        public OvertimePolicyResolutionCandidateResponse? SelectedPolicy { get; set; }
        public List<OvertimePolicyResolutionCandidateResponse> Candidates { get; set; } = new();
    }

    public class OvertimePolicyDefinitionInput
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    public class OvertimePolicyOverlapResponse
    {
        public bool HasAmbiguousOverlap { get; set; }
        public Guid? ConflictingPolicyId { get; set; }
        public string? ConflictingPolicyCode { get; set; }
        public string? ConflictingPolicyName { get; set; }
    }

    public class OvertimeRateResolveRequest
    {
        [Required]
        public Guid OvertimePolicyId { get; set; }

        [Required, MaxLength(50)]
        public string DayType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? PreferredTimeBand { get; set; }

        public DateTime? EffectiveDate { get; set; }

        [Range(0, int.MaxValue)]
        public int MinutePosition { get; set; }

        [Range(0, int.MaxValue)]
        public int EligibleMinutes { get; set; }

        public TimeOnly? OccurrenceTime { get; set; }
    }

    public class OvertimeRateResolutionCandidateResponse
    {
        public Guid Id { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public string DayType { get; set; } = string.Empty;
        public string TimeBand { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal RateMultiplier { get; set; }
        public decimal? FixedAmount { get; set; }
        public int Priority { get; set; }
        public int ApplicabilityScore { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    public class OvertimeRateResolutionResponse
    {
        public bool IsResolved { get; set; }
        public bool IsAmbiguous { get; set; }
        public string Message { get; set; } = string.Empty;
        public OvertimeRateResolutionCandidateResponse? SelectedRate { get; set; }
        public List<OvertimeRateResolutionCandidateResponse> Candidates { get; set; } = new();
    }

    public class OvertimeRateDefinitionInput
    {
        public Guid OvertimePolicyId { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string TimeBand { get; set; } = string.Empty;
        public int StartMinute { get; set; }
        public int? EndMinute { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    public class OvertimeRateOverlapResponse
    {
        public bool HasAmbiguousOverlap { get; set; }
        public Guid? ConflictingRateId { get; set; }
        public string? ConflictingRateCode { get; set; }
        public string? ConflictingRateName { get; set; }
    }
}
