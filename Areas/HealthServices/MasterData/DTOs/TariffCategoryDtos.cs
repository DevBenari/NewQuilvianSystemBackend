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
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class TariffCategoryDetailResponse : TariffCategoryResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
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
        public int SortOrder { get; set; }
    }

    public class TariffCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<TariffCategoryOptionResponse> Items { get; set; } = new();
    }

    public class TariffCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public TariffCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<TariffCategoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<TariffCategorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<TariffCategoryQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<TariffCategoryFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<TariffCategoryFormFieldMetadataResponse> UpdateFields { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class TariffCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
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

    public class TariffCategoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class TariffCategorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class TariffCategoryQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class TariffCategoryFormFieldMetadataResponse
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

    public class CreateTariffCategoryRequest
    {
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

    public class UpdateTariffCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteTariffCategoryRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
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

    public class TariffCategoryUpdateResponse : TariffCategoryCreateResponse
    {
    }

    public class TariffCategoryDeleteResponse
    {
        public Guid Id { get; set; }
        public string TariffCategoryCode { get; set; } = string.Empty;
        public string TariffCategoryName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
        public DateTime? DeleteDateTime { get; set; }
    }
}
