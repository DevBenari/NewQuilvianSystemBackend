using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class SupplierSummaryResponse
    {
        public int TotalSupplier { get; set; }
        public int ActiveSupplier { get; set; }
        public int InactiveSupplier { get; set; }
        public int PreferredSupplier { get; set; }
        public int BlacklistedSupplier { get; set; }
        public int TaxableSupplier { get; set; }
        public int PrincipalSupplier { get; set; }
        public int DistributorSupplier { get; set; }
        public int ManufacturerSupplier { get; set; }
        public int PharmacySupplier { get; set; }
        public int MedicalDeviceSupplier { get; set; }
        public int LaboratorySupplier { get; set; }
        public int ConsumableSupplier { get; set; }
    }

    public class SupplierResponse
    {
        public Guid Id { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string SupplierType { get; set; } = string.Empty;
        public string? SupplierGroupName { get; set; }
        public string? TaxNumber { get; set; }
        public string? BusinessLicenseNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? CityName { get; set; }
        public string? ProvinceName { get; set; }
        public string? CountryName { get; set; }
        public int PaymentTermDays { get; set; }
        public int LeadTimeDays { get; set; }
        public decimal MinimumPurchaseAmount { get; set; }
        public decimal? CreditLimitAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? TaxPercent { get; set; }
        public bool IsPrincipal { get; set; }
        public bool IsDistributor { get; set; }
        public bool IsManufacturer { get; set; }
        public bool IsPharmacySupplier { get; set; }
        public bool IsMedicalDeviceSupplier { get; set; }
        public bool IsLaboratorySupplier { get; set; }
        public bool IsConsumableSupplier { get; set; }
        public bool IsPreferredSupplier { get; set; }
        public bool IsBlacklisted { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class SupplierDetailResponse : SupplierResponse
    {
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BlacklistReason { get; set; }
        public string? Description { get; set; }
    }

    public class SupplierOptionResponse
    {
        public Guid Id { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string SupplierType { get; set; } = string.Empty;
        public string? SupplierGroupName { get; set; }
        public string? ContactPersonName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public int PaymentTermDays { get; set; }
        public int LeadTimeDays { get; set; }
        public bool IsTaxable { get; set; }
        public decimal? TaxPercent { get; set; }
        public bool IsPreferredSupplier { get; set; }
        public bool IsBlacklisted { get; set; }
    }

    public class SupplierOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<SupplierOptionResponse> Items { get; set; } = new();
    }

    public class SupplierFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public SupplierDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<SupplierCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<SupplierSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<SupplierTypeOptionResponse> SupplierTypeOptions { get; set; } = new();
    }

    public class SupplierDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class SupplierCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SupplierSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SupplierTypeOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateSupplierRequest
    {
        [Required]
        [MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? LegalName { get; set; }

        [Required]
        [MaxLength(50)]
        public string SupplierType { get; set; } = "General";

        [MaxLength(100)]
        public string? SupplierGroupName { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        [MaxLength(100)]
        public string? ContactPersonName { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? CityName { get; set; }

        [MaxLength(100)]
        public string? ProvinceName { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? CountryName { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(150)]
        public string? BankAccountName { get; set; }

        public int PaymentTermDays { get; set; } = 0;
        public int LeadTimeDays { get; set; } = 0;
        public decimal MinimumPurchaseAmount { get; set; } = 0;
        public decimal? CreditLimitAmount { get; set; }
        public bool IsTaxable { get; set; } = false;
        public decimal? TaxPercent { get; set; }
        public bool IsPrincipal { get; set; } = false;
        public bool IsDistributor { get; set; } = false;
        public bool IsManufacturer { get; set; } = false;
        public bool IsPharmacySupplier { get; set; } = true;
        public bool IsMedicalDeviceSupplier { get; set; } = false;
        public bool IsLaboratorySupplier { get; set; } = false;
        public bool IsConsumableSupplier { get; set; } = false;
        public bool IsPreferredSupplier { get; set; } = false;
        public bool IsBlacklisted { get; set; } = false;

        [MaxLength(250)]
        public string? BlacklistReason { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateSupplierRequest : CreateSupplierRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class SupplierCreateResponse
    {
        public Guid Id { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierType { get; set; } = string.Empty;
        public bool IsPreferredSupplier { get; set; }
        public bool IsBlacklisted { get; set; }
        public bool IsActive { get; set; }
    }
}
