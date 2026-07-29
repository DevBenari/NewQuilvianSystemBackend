using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class BenefitTypeSummaryResponse
    {
        public int TotalBenefitType { get; set; }
        public int ActiveBenefitType { get; set; }
        public int InactiveBenefitType { get; set; }
        public int EnrollmentRequiredType { get; set; }
        public int DependentAllowedType { get; set; }
        public int ClaimBasedType { get; set; }
        public int EvidenceRequiredType { get; set; }
    }

    public class BenefitTypeResponse
    {
        public Guid Id { get; set; }
        public string BenefitTypeCode { get; set; } = string.Empty;
        public string BenefitTypeName { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public string FundingType { get; set; } = string.Empty;
        public bool IsTaxable { get; set; }
        public bool RequiresEnrollment { get; set; }
        public bool AllowsDependents { get; set; }
        public int MaximumDependents { get; set; }
        public bool IsClaimBased { get; set; }
        public bool RequiresEvidence { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int BenefitPlanCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class BenefitTypeDetailResponse : BenefitTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class BenefitTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string BenefitTypeCode { get; set; } = string.Empty;
        public string BenefitTypeName { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public string FundingType { get; set; } = string.Empty;
        public bool IsTaxable { get; set; }
        public bool RequiresEnrollment { get; set; }
        public bool AllowsDependents { get; set; }
    }

    public class BenefitTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<BenefitTypeOptionResponse> Items { get; set; } = new();
    }

    public class BenefitTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public BenefitTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BenefitTypeStringOptionResponse> PrimaryOptions { get; set; } = new();
        public List<BenefitTypeStringOptionResponse> SecondaryOptions { get; set; } = new();
        public List<BenefitTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BenefitTypeDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BenefitTypeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BenefitTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateBenefitTypeRequest
    {
        [Required]
        [MaxLength(150)]
        public string BenefitTypeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BenefitCategory { get; set; } = "Other";

        [Required]
        [MaxLength(50)]
        public string FundingType { get; set; } = "Employer";

        public bool IsTaxable { get; set; }

        public bool RequiresEnrollment { get; set; } = true;

        public bool AllowsDependents { get; set; }

        [Range(0, int.MaxValue)]
        public int MaximumDependents { get; set; }

        public bool IsClaimBased { get; set; }

        public bool RequiresEvidence { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

    }

    public class UpdateBenefitTypeRequest : CreateBenefitTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBenefitTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class BenefitTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string BenefitTypeCode { get; set; } = string.Empty;
        public string BenefitTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}