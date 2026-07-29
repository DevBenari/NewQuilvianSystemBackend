using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class BenefitEligibilityRuleSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int ProbationAllowedData { get; set; }
        public int ContractAllowedData { get; set; }
        public int ManagerApprovalRequiredData { get; set; }
        public int HrVerificationRequiredData { get; set; }
    }

    public class BenefitEligibilityRuleResponse
    {
        public Guid Id { get; set; }
        public Guid BenefitPlanId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? SalaryGradeId { get; set; }

        public string EligibilityRuleCode { get; set; } = string.Empty;
        public string EligibilityRuleName { get; set; } = string.Empty;

        public int MinimumServiceMonths { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public bool AllowProbationEmployee { get; set; }
        public bool AllowContractEmployee { get; set; }
        public bool RequireFullTimeEmployment { get; set; }
        public decimal MinimumWeeklyHours { get; set; }
        public int CoverageStartOffsetDays { get; set; }
        public int CoverageEndAfterTerminationDays { get; set; }
        public bool RequireManagerApproval { get; set; }
        public bool RequireHrVerification { get; set; }
        public int Priority { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class BenefitEligibilityRuleDetailResponse : BenefitEligibilityRuleResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class BenefitEligibilityRuleOptionResponse
    {
        public Guid Id { get; set; }
        public Guid BenefitPlanId { get; set; }
        public string EligibilityRuleCode { get; set; } = string.Empty;
        public string EligibilityRuleName { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class BenefitEligibilityRuleFilterMetadataResponse
    {
        public BenefitEligibilityRuleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BenefitEligibilityRuleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BenefitEligibilityRuleDefaultFilterResponse
    {
        public Guid? BenefitPlanId { get; set; }
        public bool? AllowProbationEmployee { get; set; }
        public bool? AllowContractEmployee { get; set; }
        public bool? RequireFullTimeEmployment { get; set; }
        public bool? RequireManagerApproval { get; set; }
        public bool? RequireHrVerification { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateBenefitEligibilityRuleRequest
    {
        [Required]
        public Guid BenefitPlanId { get; set; }

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? SalaryGradeId { get; set; }

        [Required]
        [MaxLength(150)]
        public string EligibilityRuleName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int MinimumServiceMonths { get; set; }

        [Range(0, 150)]
        public int? MinimumAge { get; set; }

        [Range(0, 150)]
        public int? MaximumAge { get; set; }

        public bool AllowProbationEmployee { get; set; }
        public bool AllowContractEmployee { get; set; } = true;
        public bool RequireFullTimeEmployment { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinimumWeeklyHours { get; set; }

        [Range(0, int.MaxValue)]
        public int CoverageStartOffsetDays { get; set; }

        [Range(0, int.MaxValue)]
        public int CoverageEndAfterTerminationDays { get; set; }

        public bool RequireManagerApproval { get; set; }
        public bool RequireHrVerification { get; set; } = true;
        public int Priority { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateBenefitEligibilityRuleRequest : CreateBenefitEligibilityRuleRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBenefitEligibilityRuleStatusRequest
    {
        public bool IsActive { get; set; }
    }


    public class BenefitEligibilityRuleStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BenefitEligibilityRuleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BenefitEligibilityRuleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<BenefitEligibilityRuleOptionResponse> Items { get; set; } = new();
    }

}
