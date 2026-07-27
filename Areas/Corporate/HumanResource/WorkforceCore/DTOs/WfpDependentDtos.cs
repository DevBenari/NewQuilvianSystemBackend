using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpDependentSummaryResponse
    {
        public int TotalDependent { get; set; }
        public int ActiveDependent { get; set; }
        public int InactiveDependent { get; set; }
        public int TaxDependent { get; set; }
        public int BenefitEligibleDependent { get; set; }
        public int InsuranceEligibleDependent { get; set; }
        public int CurrentlyEffectiveDependent { get; set; }
        public int EndedDependent { get; set; }
    }

    public class WfpDependentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? FamilyMemberId { get; set; }
        public string? FamilyMemberName { get; set; }
        public string? FamilyRelationship { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string DependentType { get; set; } = string.Empty;
        public string DependentStatus { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public bool IsTaxDependent { get; set; }
        public bool IsBenefitEligible { get; set; }
        public bool IsInsuranceEligible { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpDependentDetailResponse : WfpDependentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpDependentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpDependentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpDependentStringOptionResponse> DependentTypeOptions { get; set; } = new();
        public List<WfpDependentStringOptionResponse> DependentStatusOptions { get; set; } = new();
        public List<WfpDependentFamilyMemberOptionResponse> FamilyMemberOptions { get; set; } = new();
        public List<WfpDependentBenefitPlanOptionResponse> BenefitPlanOptions { get; set; } = new();
        public List<WfpDependentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpDependentDefaultFilterResponse
    {
        public Guid? FamilyMemberId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public string? DependentType { get; set; }
        public string? DependentStatus { get; set; }
        public bool? IsTaxDependent { get; set; }
        public bool? IsBenefitEligible { get; set; }
        public bool? IsInsuranceEligible { get; set; }
        public bool? IsCurrentlyEffective { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpDependentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpDependentFamilyMemberOptionResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpDependentBenefitPlanOptionResponse
    {
        public Guid Id { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpDependentRequest
    {
        public Guid? FamilyMemberId { get; set; }
        public Guid? BenefitPlanId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DependentType { get; set; } = "Family";

        [Required]
        [MaxLength(50)]
        public string DependentStatus { get; set; } = "Active";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTaxDependent { get; set; }
        public bool IsBenefitEligible { get; set; } = true;
        public bool IsInsuranceEligible { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpDependentRequest : CreateWfpDependentRequest
    {
    }

    public class UpdateWfpDependentStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(50)]
        public string? DependentStatus { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
    }
}
