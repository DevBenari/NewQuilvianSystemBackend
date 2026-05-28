using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class MembershipTierSummaryResponse
    {
        public int TotalMembershipTier { get; set; }
        public int ActiveMembershipTier { get; set; }
        public int InactiveMembershipTier { get; set; }
        public int DefaultTier { get; set; }
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
    }

    public class MembershipTierDetailResponse : MembershipTierResponse
    {
        public string? BenefitDescription { get; set; }
        public string? Description { get; set; }
    }

    public class MembershipTierOptionResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public string? CardTitle { get; set; }
        public string? CardColor { get; set; }
        public int PriorityLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSelectableInKiosk { get; set; }
        public bool IsSelectableInAdmission { get; set; }
        public bool PriorityQueue { get; set; }
        public int ValidityMonths { get; set; }
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
        public MembershipTierDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MembershipTierSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MembershipTierEnumOptionResponse> TierTypeOptions { get; set; } = new();
    }

    public class MembershipTierDefaultFilterResponse
    {
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

    public class MembershipTierSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateMembershipTierRequest
    {
        [Required]
        [MaxLength(50)]
        public string TierCode { get; set; } = string.Empty;

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

        public int PriorityLevel { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
        public bool IsSelectableInKiosk { get; set; } = false;
        public bool IsSelectableInAdmission { get; set; } = false;
        public bool IsManagedByMarketingOnly { get; set; } = true;
        public decimal RegistrationDiscountPercent { get; set; } = 0;
        public decimal ConsultationDiscountPercent { get; set; } = 0;
        public decimal ProcedureDiscountPercent { get; set; } = 0;
        public decimal LaboratoryDiscountPercent { get; set; } = 0;
        public decimal RadiologyDiscountPercent { get; set; } = 0;
        public decimal PharmacyDiscountPercent { get; set; } = 0;
        public bool PriorityQueue { get; set; } = false;
        public bool FreeAnnualCheckup { get; set; } = false;
        public bool FreeParking { get; set; } = false;
        public int ValidityMonths { get; set; } = 12;
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

    public class MembershipTierCreateResponse
    {
        public Guid Id { get; set; }
        public string TierCode { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public MembershipTierType TierType { get; set; }
        public int PriorityLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
