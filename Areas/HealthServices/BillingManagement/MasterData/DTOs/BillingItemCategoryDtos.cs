using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs
{
    public class BillingItemCategorySummaryResponse
    {
        public int TotalBillingItemCategory { get; set; }
        public int ActiveBillingItemCategory { get; set; }
        public int InactiveBillingItemCategory { get; set; }
        public int RegistrationFeeCategory { get; set; }
        public int AdministrationFeeCategory { get; set; }
        public int ConsultationFeeCategory { get; set; }
        public int RoomChargeCategory { get; set; }
        public int ProcedureCategory { get; set; }
        public int LaboratoryCategory { get; set; }
        public int RadiologyCategory { get; set; }
        public int PharmacyCategory { get; set; }
        public int DrugCategory { get; set; }
        public int PackageCategory { get; set; }
        public int DiscountCategory { get; set; }
        public int TaxCategory { get; set; }
        public int DepositCategory { get; set; }
        public int RefundCategory { get; set; }
        public int CoveredByInsuranceDefaultCategory { get; set; }
        public int NeedDoctorCategory { get; set; }
        public int NeedApprovalCategory { get; set; }
        public int EditableInBillingCategory { get; set; }
        public int SystemCategory { get; set; }
    }

    public class BillingItemCategoryResponse
    {
        public Guid Id { get; set; }

        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
        public string? BillingGroupName { get; set; }
        public string ItemSourceType { get; set; } = string.Empty;

        public bool IsRegistrationFee { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsPharmacy { get; set; }
        public bool IsDrug { get; set; }
        public bool IsPackage { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsTax { get; set; }
        public bool IsDeposit { get; set; }
        public bool IsRefund { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsEditableInBilling { get; set; }
        public bool IsSystemCategory { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class BillingItemCategoryDetailResponse : BillingItemCategoryResponse
    {
        public string? Description { get; set; }
    }

    public class BillingItemCategoryOptionResponse
    {
        public Guid Id { get; set; }

        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
        public string? BillingGroupName { get; set; }
        public string ItemSourceType { get; set; } = string.Empty;

        public bool IsRegistrationFee { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsPharmacy { get; set; }
        public bool IsDrug { get; set; }
        public bool IsPackage { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsTax { get; set; }
        public bool IsDeposit { get; set; }
        public bool IsRefund { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsEditableInBilling { get; set; }
        public bool IsSystemCategory { get; set; }
    }

    public class BillingItemCategoryFilterMetadataResponse
    {
        public BillingItemCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BillingItemCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> ItemSourceTypeOptions { get; set; } = new();
    }

    public class BillingItemCategoryDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? BillingGroupName { get; set; }
        public string? ItemSourceType { get; set; }

        public bool? IsRegistrationFee { get; set; }
        public bool? IsAdministrationFee { get; set; }
        public bool? IsConsultationFee { get; set; }
        public bool? IsRoomCharge { get; set; }
        public bool? IsProcedure { get; set; }
        public bool? IsLaboratory { get; set; }
        public bool? IsRadiology { get; set; }
        public bool? IsPharmacy { get; set; }
        public bool? IsDrug { get; set; }
        public bool? IsPackage { get; set; }
        public bool? IsDiscount { get; set; }
        public bool? IsTax { get; set; }
        public bool? IsDeposit { get; set; }
        public bool? IsRefund { get; set; }
        public bool? IsCoveredByInsuranceDefault { get; set; }
        public bool? IsNeedDoctor { get; set; }
        public bool? IsNeedApproval { get; set; }
        public bool? IsEditableInBilling { get; set; }
        public bool? IsSystemCategory { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BillingItemCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateBillingItemCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string BillingItemCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BillingItemCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BillingGroupName { get; set; }

        [MaxLength(50)]
        public string ItemSourceType { get; set; } = "Manual";

        public bool IsRegistrationFee { get; set; } = false;
        public bool IsAdministrationFee { get; set; } = false;
        public bool IsConsultationFee { get; set; } = false;
        public bool IsRoomCharge { get; set; } = false;
        public bool IsProcedure { get; set; } = false;
        public bool IsLaboratory { get; set; } = false;
        public bool IsRadiology { get; set; } = false;
        public bool IsPharmacy { get; set; } = false;
        public bool IsDrug { get; set; } = false;
        public bool IsPackage { get; set; } = false;
        public bool IsDiscount { get; set; } = false;
        public bool IsTax { get; set; } = false;
        public bool IsDeposit { get; set; } = false;
        public bool IsRefund { get; set; } = false;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;
        public bool IsNeedDoctor { get; set; } = false;
        public bool IsNeedApproval { get; set; } = false;
        public bool IsEditableInBilling { get; set; } = true;
        public bool IsSystemCategory { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateBillingItemCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string BillingItemCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BillingItemCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BillingGroupName { get; set; }

        [MaxLength(50)]
        public string ItemSourceType { get; set; } = "Manual";

        public bool IsRegistrationFee { get; set; } = false;
        public bool IsAdministrationFee { get; set; } = false;
        public bool IsConsultationFee { get; set; } = false;
        public bool IsRoomCharge { get; set; } = false;
        public bool IsProcedure { get; set; } = false;
        public bool IsLaboratory { get; set; } = false;
        public bool IsRadiology { get; set; } = false;
        public bool IsPharmacy { get; set; } = false;
        public bool IsDrug { get; set; } = false;
        public bool IsPackage { get; set; } = false;
        public bool IsDiscount { get; set; } = false;
        public bool IsTax { get; set; } = false;
        public bool IsDeposit { get; set; } = false;
        public bool IsRefund { get; set; } = false;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;
        public bool IsNeedDoctor { get; set; } = false;
        public bool IsNeedApproval { get; set; } = false;
        public bool IsEditableInBilling { get; set; } = true;
        public bool IsSystemCategory { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BillingItemCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
    }

    public class BillingItemCategoryUpdateResponse
    {
        public Guid Id { get; set; }
        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class BillingItemCategoryStatusResponse
    {
        public Guid Id { get; set; }
        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class BillingItemCategoryDeleteResponse
    {
        public Guid Id { get; set; }
        public string BillingItemCategoryCode { get; set; } = string.Empty;
        public string BillingItemCategoryName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
