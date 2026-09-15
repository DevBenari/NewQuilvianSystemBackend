using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class CompanyGuarantorReimbursementRouteFilterMetadataResponse
    {
        public CompanyGuarantorReimbursementRouteDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CompanyGuarantorReimbursementRouteCustomPeriodResponse> CustomPeriods { get; set; } = new();
        public List<CompanyGuarantorReimbursementRouteSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<CompanyGuarantorReimbursementRouteOptionItemResponse> RouteTypeOptions { get; set; } = new();
        public List<CompanyGuarantorReimbursementRouteQueryParameterResponse> QueryParameters { get; set; } = new();
        public List<CompanyGuarantorReimbursementRouteFormFieldResponse> CreateFields { get; set; } = new();
        public List<CompanyGuarantorReimbursementRouteFormFieldResponse> UpdateFields { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class CompanyGuarantorReimbursementRouteDefaultFilterResponse
    {
        public string? Search { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public string? RouteType { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public Guid? InsuranceProviderId { get; set; }
        public bool? IsDefault { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CompanyGuarantorReimbursementRouteCustomPeriodResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorReimbursementRouteOptionItemResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorReimbursementRouteQueryParameterResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteFormFieldResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = string.Empty;
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteSummaryResponse
    {
        public int TotalRoute { get; set; }
        public int ActiveRoute { get; set; }
        public int InactiveRoute { get; set; }
        public int SelfRoute { get; set; }
        public int InsuranceProviderRoute { get; set; }
        public int DefaultRoute { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteResponse
    {
        public Guid Id { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string RouteType { get; set; } = string.Empty;
        public string RouteTypeName { get; set; } = string.Empty;
        public Guid? InsuranceProviderId { get; set; }
        public string? InsuranceProviderCode { get; set; }
        public string? InsuranceProviderName { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteDetailResponse : CompanyGuarantorReimbursementRouteResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string RouteType { get; set; } = string.Empty;
        public string RouteTypeName { get; set; } = string.Empty;
        public Guid? InsuranceProviderId { get; set; }
        public string? InsuranceProviderName { get; set; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCompanyGuarantorReimbursementRouteRequest
    {
        [Required(ErrorMessage = "Perusahaan penjamin wajib dipilih.")]
        public Guid CompanyGuarantorId { get; set; }

        [Required(ErrorMessage = "Tipe rute wajib dipilih.")]
        [MaxLength(30, ErrorMessage = "Tipe rute maksimal 30 karakter.")]
        public string RouteType { get; set; } = "SELF";

        public Guid? InsuranceProviderId { get; set; }

        public int Priority { get; set; } = 1;

        public bool IsDefault { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan maksimal 500 karakter.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompanyGuarantorReimbursementRouteRequest
    {
        [Required(ErrorMessage = "Tipe rute wajib dipilih.")]
        [MaxLength(30, ErrorMessage = "Tipe rute maksimal 30 karakter.")]
        public string RouteType { get; set; } = "SELF";

        public Guid? InsuranceProviderId { get; set; }

        public int Priority { get; set; } = 1;

        public bool IsDefault { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan maksimal 500 karakter.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompanyGuarantorReimbursementRouteStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CompanyGuarantorReimbursementRouteValidationException : Exception
    {
        public int StatusCode { get; }

        public CompanyGuarantorReimbursementRouteValidationException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
