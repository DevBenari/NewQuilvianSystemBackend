using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpInsuranceSummaryResponse
    {
        public int TotalInsuranceProfile { get; set; }
        public int ActiveInsuranceProfile { get; set; }
        public int InactiveInsuranceProfile { get; set; }
        public int BpjsKesehatanEnabledProfile { get; set; }
        public int BpjsKetenagakerjaanEnabledProfile { get; set; }
        public int PrivateInsuranceEnabledProfile { get; set; }
    }

    public class WfpInsuranceResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public bool IsBpjsKesehatanEnabled { get; set; }
        public string? BpjsKesehatanNumber { get; set; }
        public bool IsBpjsKetenagakerjaanEnabled { get; set; }
        public string? BpjsKetenagakerjaanNumber { get; set; }
        public bool IsPrivateInsuranceEnabled { get; set; }
        public string? PrivateInsuranceProvider { get; set; }
        public string? PrivateInsuranceNumber { get; set; }
        public decimal BpjsHealthEmployeeRate { get; set; }
        public decimal BpjsHealthEmployerRate { get; set; }
        public decimal BpjsEmploymentEmployeeRate { get; set; }
        public decimal BpjsEmploymentEmployerRate { get; set; }
        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpInsuranceDetailResponse : WfpInsuranceResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpInsuranceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpInsuranceDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpInsuranceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpInsuranceDefaultFilterResponse
    {
        public bool? IsBpjsKesehatanEnabled { get; set; }
        public bool? IsBpjsKetenagakerjaanEnabled { get; set; }
        public bool? IsPrivateInsuranceEnabled { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpInsuranceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; }

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }

        [MaxLength(200)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public decimal BpjsHealthEmployeeRate { get; set; }
        public decimal BpjsHealthEmployerRate { get; set; }
        public decimal BpjsEmploymentEmployeeRate { get; set; }
        public decimal BpjsEmploymentEmployerRate { get; set; }
        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpInsuranceRequest : CreateWfpInsuranceRequest
    {
    }

    public class UpdateWfpInsuranceStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
