using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class ApprovalDelegationPolicySummaryResponse
    {
        public int TotalPolicy { get; set; }
        public int ActivePolicy { get; set; }
        public int InactivePolicy { get; set; }
        public int TemporaryPolicy { get; set; }
        public int PermanentPolicy { get; set; }
        public int AutomaticOutOfOfficePolicy { get; set; }
        public int ManagerApprovalRequiredPolicy { get; set; }
        public int HrVerificationRequiredPolicy { get; set; }
    }

    public class ApprovalDelegationPolicyResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public string? WorkflowStepCode { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string DelegationPolicyCode { get; set; } = string.Empty;
        public string DelegationPolicyName { get; set; } = string.Empty;
        public string DelegationType { get; set; } = string.Empty;
        public int MaximumDelegationDays { get; set; }
        public int MinimumNoticeHours { get; set; }
        public bool RequireManagerApproval { get; set; }
        public bool RequireHrVerification { get; set; }
        public bool AllowCrossOrganizationUnit { get; set; }
        public bool AllowCrossHospitalSite { get; set; }
        public bool AllowCrossLegalEntity { get; set; }
        public bool AllowSubDelegation { get; set; }
        public bool AllowSelfDelegation { get; set; }
        public bool PreserveDelegatorAccountability { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ApprovalDelegationCount { get; set; }
    }

    public class ApprovalDelegationPolicyDetailResponse : ApprovalDelegationPolicyResponse
    {
    }

    public class ApprovalDelegationPolicyOptionResponse
    {
        public Guid Id { get; set; }
        public string DelegationPolicyCode { get; set; } = string.Empty;
        public string DelegationPolicyName { get; set; } = string.Empty;
        public string DelegationType { get; set; } = string.Empty;
        public int MaximumDelegationDays { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
    }

    public class ApprovalDelegationPolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ApprovalDelegationPolicyOptionResponse> Items { get; set; } = new();
    }

    public class ApprovalDelegationPolicyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ApprovalDelegationPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> DelegationTypes { get; set; } = new();
    }

    public class ApprovalDelegationPolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? DelegationType { get; set; }
        public bool? RequireManagerApproval { get; set; }
        public bool? RequireHrVerification { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "delegationPolicyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateApprovalDelegationPolicyRequest
    {
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required, MaxLength(50)]
        public string DelegationPolicyCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string DelegationPolicyName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string DelegationType { get; set; } = "Temporary";

        [Range(1, int.MaxValue)]
        public int MaximumDelegationDays { get; set; } = 30;

        [Range(0, int.MaxValue)]
        public int MinimumNoticeHours { get; set; }

        public bool RequireManagerApproval { get; set; }
        public bool RequireHrVerification { get; set; }
        public bool AllowCrossOrganizationUnit { get; set; }
        public bool AllowCrossHospitalSite { get; set; }
        public bool AllowCrossLegalEntity { get; set; }
        public bool AllowSubDelegation { get; set; }
        public bool AllowSelfDelegation { get; set; }
        public bool PreserveDelegatorAccountability { get; set; } = true;

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateApprovalDelegationPolicyRequest : CreateApprovalDelegationPolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class ApprovalDelegationPolicyCreateResponse
    {
        public Guid Id { get; set; }
        public string DelegationPolicyCode { get; set; } = string.Empty;
        public string DelegationPolicyName { get; set; } = string.Empty;
        public string DelegationType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
