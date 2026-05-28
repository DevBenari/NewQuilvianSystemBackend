using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class TariffCategorySummaryResponse
    {
        public int TotalTariffCategory { get; set; }
        public int ActiveTariffCategory { get; set; }
        public int InactiveTariffCategory { get; set; }
        public int RegistrationFeeCategory { get; set; }
        public int AdministrationFeeCategory { get; set; }
        public int ConsultationFeeCategory { get; set; }
        public int RoomChargeCategory { get; set; }
        public int ProcedureCategory { get; set; }
        public int LaboratoryCategory { get; set; }
        public int RadiologyCategory { get; set; }
        public int PharmacyCategory { get; set; }
        public int SurgeryCategory { get; set; }
        public int PackageCategory { get; set; }
        public int InsuranceCoveredDefaultCategory { get; set; }
    }

    public class TariffCategoryResponse
    {
        public Guid Id { get; set; }
        public string TariffCategoryCode { get; set; } = string.Empty;
        public string TariffCategoryName { get; set; } = string.Empty;
        public string? TariffGroupName { get; set; }
        public bool IsRegistrationFee { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsPharmacy { get; set; }
        public bool IsSurgery { get; set; }
        public bool IsPackage { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class TariffCategoryDetailResponse : TariffCategoryResponse
    {
        public string? Description { get; set; }
    }

    public class TariffCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string TariffCategoryCode { get; set; } = string.Empty;
        public string TariffCategoryName { get; set; } = string.Empty;
        public string? TariffGroupName { get; set; }
        public bool IsRegistrationFee { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsPharmacy { get; set; }
        public bool IsSurgery { get; set; }
        public bool IsPackage { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
    }

    public class TariffCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public TariffCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<TariffCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class TariffCategoryDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? TariffGroupName { get; set; }
        public bool? IsRegistrationFee { get; set; }
        public bool? IsAdministrationFee { get; set; }
        public bool? IsConsultationFee { get; set; }
        public bool? IsRoomCharge { get; set; }
        public bool? IsProcedure { get; set; }
        public bool? IsLaboratory { get; set; }
        public bool? IsRadiology { get; set; }
        public bool? IsPharmacy { get; set; }
        public bool? IsSurgery { get; set; }
        public bool? IsPackage { get; set; }
        public bool? IsCoveredByInsuranceDefault { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class TariffCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateTariffCategoryRequest
    {
        [Required]
        [MaxLength(50)]
        public string TariffCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TariffCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TariffGroupName { get; set; }

        public bool IsRegistrationFee { get; set; } = false;
        public bool IsAdministrationFee { get; set; } = false;
        public bool IsConsultationFee { get; set; } = false;
        public bool IsRoomCharge { get; set; } = false;
        public bool IsProcedure { get; set; } = false;
        public bool IsLaboratory { get; set; } = false;
        public bool IsRadiology { get; set; } = false;
        public bool IsPharmacy { get; set; } = false;
        public bool IsSurgery { get; set; } = false;
        public bool IsPackage { get; set; } = false;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateTariffCategoryRequest : CreateTariffCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class TariffCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string TariffCategoryCode { get; set; } = string.Empty;
        public string TariffCategoryName { get; set; } = string.Empty;
        public string? TariffGroupName { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
