using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpTaxSummaryResponse
    {
        public int TotalTaxProfile { get; set; }
        public int ActiveTaxProfile { get; set; }
        public int InactiveTaxProfile { get; set; }
        public int NpwpRegisteredProfile { get; set; }
        public int TaxResidentProfile { get; set; }
        public int PreviousEmployerProfile { get; set; }
        public int EmployerBorneTaxProfile { get; set; }
    }

    public class WfpTaxResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string? NpwpNumber { get; set; }
        public string TaxStatus { get; set; } = string.Empty;
        public string TaxMethod { get; set; } = string.Empty;
        public string TaxCountryCode { get; set; } = string.Empty;
        public string? TaxOfficeCode { get; set; }
        public bool IsNpwpRegistered { get; set; }
        public bool IsTaxResident { get; set; }
        public bool HasPreviousEmployer { get; set; }
        public bool IsEmployerBorneTax { get; set; }
        public decimal PreviousEmployerTaxableIncome { get; set; }
        public decimal PreviousEmployerTaxPaid { get; set; }
        public decimal AnnualNonTaxableIncome { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTaxDetailResponse : WfpTaxResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTaxFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpTaxDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTaxStringOptionResponse> TaxStatusOptions { get; set; } = new();
        public List<WfpTaxStringOptionResponse> TaxMethodOptions { get; set; } = new();
        public List<WfpTaxSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpTaxDefaultFilterResponse
    {
        public string? TaxStatus { get; set; }
        public string? TaxMethod { get; set; }
        public bool? IsNpwpRegistered { get; set; }
        public bool? IsTaxResident { get; set; }
        public bool? HasPreviousEmployer { get; set; }
        public bool? IsEmployerBorneTax { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTaxStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTaxSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpTaxRequest
    {
        [MaxLength(50)]
        public string? NpwpNumber { get; set; }

        [Required, MaxLength(30)]
        public string TaxStatus { get; set; } = "TK/0";

        [Required, MaxLength(30)]
        public string TaxMethod { get; set; } = "Gross";

        [Required, MaxLength(3)]
        public string TaxCountryCode { get; set; } = "ID";

        [MaxLength(50)]
        public string? TaxOfficeCode { get; set; }

        public bool IsNpwpRegistered { get; set; }
        public bool IsTaxResident { get; set; } = true;
        public bool HasPreviousEmployer { get; set; }
        public bool IsEmployerBorneTax { get; set; }
        public decimal PreviousEmployerTaxableIncome { get; set; }
        public decimal PreviousEmployerTaxPaid { get; set; }
        public decimal AnnualNonTaxableIncome { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpTaxRequest : CreateWfpTaxRequest
    {
    }

    public class UpdateWfpTaxStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
