using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugSupplierSummaryResponse
    {
        public int TotalDrugSupplier { get; set; }
        public int ActiveDrugSupplier { get; set; }
        public int InactiveDrugSupplier { get; set; }
        public int PreferredSupplier { get; set; }
        public int ContractSupplier { get; set; }
        public int DefaultForPurchase { get; set; }
        public int AllowPurchase { get; set; }
        public int RequireQuotation { get; set; }
        public int WithPurchaseUnit { get; set; }
        public int EffectiveDrugSupplier { get; set; }
        public int ExpiredDrugSupplier { get; set; }
    }

    public class DrugSupplierResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }

        public Guid SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierType { get; set; } = string.Empty;
        public bool IsSupplierBlacklisted { get; set; }

        public string DrugSupplierCode { get; set; } = string.Empty;
        public string? SupplierDrugCode { get; set; }
        public string? SupplierDrugName { get; set; }

        public Guid? PurchaseUnitMeasurementId { get; set; }
        public string? PurchaseUnitMeasurementCode { get; set; }
        public string? PurchaseUnitMeasurementName { get; set; }
        public string? PurchaseUnitMeasurementSymbol { get; set; }

        public decimal MinimumOrderQuantity { get; set; }
        public decimal OrderMultipleQuantity { get; set; }
        public decimal? MaximumOrderQuantity { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }

        public decimal DefaultPurchasePrice { get; set; }
        public decimal? LastPurchasePrice { get; set; }
        public decimal? ContractPurchasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? TaxPercent { get; set; }

        public int LeadTimeDays { get; set; }

        public bool IsPreferredSupplier { get; set; }
        public bool IsContractSupplier { get; set; }
        public bool IsDefaultForPurchase { get; set; }
        public bool IsAllowPurchase { get; set; }
        public bool IsRequireQuotation { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugSupplierDetailResponse : DrugSupplierResponse
    {
        public string? Description { get; set; }
    }

    public class DrugSupplierOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;

        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public string DrugSupplierCode { get; set; } = string.Empty;
        public string? SupplierDrugCode { get; set; }
        public string? SupplierDrugName { get; set; }

        public Guid? PurchaseUnitMeasurementId { get; set; }
        public string? PurchaseUnitMeasurementName { get; set; }
        public string? PurchaseUnitMeasurementSymbol { get; set; }

        public decimal MinimumOrderQuantity { get; set; }
        public decimal OrderMultipleQuantity { get; set; }
        public decimal? MaximumOrderQuantity { get; set; }
        public decimal DefaultPurchasePrice { get; set; }
        public decimal? LastPurchasePrice { get; set; }
        public decimal? ContractPurchasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public int LeadTimeDays { get; set; }

        public bool IsPreferredSupplier { get; set; }
        public bool IsContractSupplier { get; set; }
        public bool IsDefaultForPurchase { get; set; }
        public bool IsAllowPurchase { get; set; }
        public bool IsRequireQuotation { get; set; }
    }

    public class DrugSupplierOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DrugSupplierOptionResponse> Items { get; set; } = new();
    }

    public class DrugSupplierFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DrugSupplierDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugSupplierCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugSupplierSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class DrugSupplierDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? SupplierId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugSupplierCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugSupplierSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugSupplierRequest
    {
        [Required]
        public Guid DrugId { get; set; }

        [Required]
        public Guid SupplierId { get; set; }

        [MaxLength(50)]
        public string? SupplierDrugCode { get; set; }

        [MaxLength(200)]
        public string? SupplierDrugName { get; set; }

        public Guid? PurchaseUnitMeasurementId { get; set; }

        public decimal MinimumOrderQuantity { get; set; } = 1;
        public decimal OrderMultipleQuantity { get; set; } = 1;
        public decimal? MaximumOrderQuantity { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }

        public decimal DefaultPurchasePrice { get; set; } = 0;
        public decimal? LastPurchasePrice { get; set; }
        public decimal? ContractPurchasePrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? TaxPercent { get; set; }

        public int LeadTimeDays { get; set; } = 0;

        public bool IsPreferredSupplier { get; set; } = false;
        public bool IsContractSupplier { get; set; } = false;
        public bool IsDefaultForPurchase { get; set; } = false;
        public bool IsAllowPurchase { get; set; } = true;
        public bool IsRequireQuotation { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDrugSupplierRequest : CreateDrugSupplierRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DrugSupplierCreateResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string DrugSupplierCode { get; set; } = string.Empty;
        public string? SupplierDrugCode { get; set; }
        public bool IsPreferredSupplier { get; set; }
        public bool IsDefaultForPurchase { get; set; }
        public bool IsAllowPurchase { get; set; }
        public bool IsActive { get; set; }
    }
}
