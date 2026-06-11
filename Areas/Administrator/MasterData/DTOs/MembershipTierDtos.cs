using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class MembershipTierSummaryResponse
    {
        public int TotalMembershipTier { get; set; }
        public int ActiveMembershipTier { get; set; }
        public int InactiveMembershipTier { get; set; }
        public int DefaultTier { get; set; }
        public int RegularTier { get; set; }
        public int SilverTier { get; set; }
        public int GoldTier { get; set; }
        public int PlatinumTier { get; set; }
        public int ExecutiveTier { get; set; }
        public int CorporateTier { get; set; }
        public int FamilyTier { get; set; }
        public int OtherTier { get; set; }
        public int SelectableInKiosk { get; set; }
        public int SelectableInAdmission { get; set; }
        public int MarketingManagedOnly { get; set; }
        public int PriorityQueueTier { get; set; }
        public int FreeAnnualCheckupTier { get; set; }
        public int FreeParkingTier { get; set; }
    }

    public class MembershipTierResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public string TierTypeName { get; set; } = string.Empty;
        public string? CardTitle { get; set; }
        public string? CardColor { get; set; }
        public string? CardImagePath { get; set; }
        public int PriorityLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSelectableInKiosk { get; set; }
        public bool IsSelectableInAdmission { get; set; }
        public bool IsManagedByMarketingOnly { get; set; }
        public decimal RegistrationDiscountPercent { get; set; }
        public decimal ConsultationDiscountPercent { get; set; }
        public decimal ProcedureDiscountPercent { get; set; }
        public decimal LaboratoryDiscountPercent { get; set; }
        public decimal RadiologyDiscountPercent { get; set; }
        public decimal PharmacyDiscountPercent { get; set; }
        public bool PriorityQueue { get; set; }
        public bool FreeAnnualCheckup { get; set; }
        public bool FreeParking { get; set; }
        public int ValidityMonths { get; set; }
        public decimal MinimumSpendAmount { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class MembershipTierDetailResponse : MembershipTierResponse
    {
        public string? BenefitDescription { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class MembershipTierOptionResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public string TierTypeName { get; set; } = string.Empty;
        public string? CardTitle { get; set; }
        public string? CardColor { get; set; }
        public int PriorityLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSelectableInKiosk { get; set; }
        public bool IsSelectableInAdmission { get; set; }
        public bool IsManagedByMarketingOnly { get; set; }
        public bool PriorityQueue { get; set; }
        public bool FreeAnnualCheckup { get; set; }
        public bool FreeParking { get; set; }
        public int ValidityMonths { get; set; }
        public decimal MinimumSpendAmount { get; set; }
        public int SortOrder { get; set; }
    }

    public class MembershipTierOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<MembershipTierOptionResponse> Items { get; set; } = new();
    }

    public class MembershipTierEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MembershipTierFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public MembershipTierDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MembershipTierCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<MembershipTierSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MembershipTierEnumOptionResponse> TierTypeOptions { get; set; } = new();
        public List<MembershipTierQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<MembershipTierFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<MembershipTierFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class MembershipTierDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public MembershipTierType? TierType { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsSelectableInKiosk { get; set; }
        public bool? IsSelectableInAdmission { get; set; }
        public bool? IsManagedByMarketingOnly { get; set; }
        public bool? PriorityQueue { get; set; }
        public bool? FreeAnnualCheckup { get; set; }
        public bool? FreeParking { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MembershipTierCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class MembershipTierSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MembershipTierQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class MembershipTierFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateMembershipTierRequest
    {
        [Required]
        [MaxLength(150)]
        public string TierName { get; set; } = string.Empty;

        public MembershipTierType TierType { get; set; } = MembershipTierType.Regular;

        [MaxLength(100)]
        public string? CardTitle { get; set; }

        [MaxLength(50)]
        public string? CardColor { get; set; }

        [MaxLength(500)]
        public string? CardImagePath { get; set; }

        [Range(0, int.MaxValue)]
        public int PriorityLevel { get; set; } = 0;

        public bool IsDefault { get; set; } = false;
        public bool IsSelectableInKiosk { get; set; } = false;
        public bool IsSelectableInAdmission { get; set; } = false;
        public bool IsManagedByMarketingOnly { get; set; } = true;

        [Range(0, 100)]
        public decimal RegistrationDiscountPercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal ConsultationDiscountPercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal ProcedureDiscountPercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal LaboratoryDiscountPercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal RadiologyDiscountPercent { get; set; } = 0;

        [Range(0, 100)]
        public decimal PharmacyDiscountPercent { get; set; } = 0;

        public bool PriorityQueue { get; set; } = false;
        public bool FreeAnnualCheckup { get; set; } = false;
        public bool FreeParking { get; set; } = false;

        [Range(1, 1200)]
        public int ValidityMonths { get; set; } = 12;

        [Range(0, 999999999999)]
        public decimal MinimumSpendAmount { get; set; } = 0;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? BenefitDescription { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateMembershipTierRequest : CreateMembershipTierRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMembershipTierStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteMembershipTierRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class MembershipTierCreateResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public string TierTypeName { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class MembershipTierUpdateResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public string TierTypeName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class MembershipTierStatusResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class MembershipTierDeleteResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
