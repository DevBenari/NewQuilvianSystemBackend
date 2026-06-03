using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class TariffSummaryResponse
    {
        public int TotalTariff { get; set; }
        public int ActiveTariff { get; set; }
        public int InactiveTariff { get; set; }
        public int SurgeryRelatedTariff { get; set; }
        public int RoomChargeTariff { get; set; }
        public int AdministrationFeeTariff { get; set; }
        public int RegistrationFeeTariff { get; set; }
        public int ConsultationFeeTariff { get; set; }
        public int PackageTariff { get; set; }
        public int NeedDoctorTariff { get; set; }
        public int NeedApprovalTariff { get; set; }
        public int TaxableTariff { get; set; }
        public int EffectiveTariff { get; set; }
        public int ExpiredTariff { get; set; }
    }

    public class TariffResponse
    {
        public Guid Id { get; set; }

        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;

        public Guid TariffCategoryId { get; set; }
        public string TariffCategoryCode { get; set; } = string.Empty;
        public string TariffCategoryName { get; set; } = string.Empty;
        public string? TariffGroupName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? DrugId { get; set; }
        public string? DrugCode { get; set; }
        public string? DrugName { get; set; }

        public string? ExternalServiceCode { get; set; }
        public string? ExternalClassCode { get; set; }
        public string? ProviderName { get; set; }

        public bool IsSurgeryRelated { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsRegistrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsPackageTariff { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }

        public decimal NormalPrice { get; set; }
        public decimal? MemberPrice { get; set; }
        public decimal? InsurancePrice { get; set; }
        public decimal? CompanyPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsTaxable { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class TariffDetailResponse : TariffResponse
    {
        public string? Description { get; set; }
    }

    public class TariffOptionResponse
    {
        public Guid Id { get; set; }

        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;

        public Guid TariffCategoryId { get; set; }
        public string TariffCategoryName { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? DrugId { get; set; }
        public string? DrugName { get; set; }

        public decimal NormalPrice { get; set; }
        public decimal? MemberPrice { get; set; }
        public decimal? InsurancePrice { get; set; }
        public decimal? CompanyPrice { get; set; }

        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsTaxable { get; set; }
    }

    public class TariffFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public TariffDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<TariffCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<TariffSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<TariffQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<TariffFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<TariffFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class TariffDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? TariffCategoryId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class TariffCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class TariffSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class TariffQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class TariffFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool IsReadonly { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
    }

    public class CreateTariffRequest
    {
        [Required]
        [MaxLength(250)]
        public string TariffName { get; set; } = string.Empty;

        [Required]
        public Guid TariffCategoryId { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? PatientClassId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? DrugId { get; set; }

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? ProviderName { get; set; }

        public bool IsSurgeryRelated { get; set; } = false;
        public bool IsRoomCharge { get; set; } = false;
        public bool IsAdministrationFee { get; set; } = false;
        public bool IsRegistrationFee { get; set; } = false;
        public bool IsConsultationFee { get; set; } = false;
        public bool IsPackageTariff { get; set; } = false;
        public bool IsNeedDoctor { get; set; } = false;
        public bool IsNeedApproval { get; set; } = false;

        public decimal NormalPrice { get; set; } = 0;
        public decimal? MemberPrice { get; set; }
        public decimal? InsurancePrice { get; set; }
        public decimal? CompanyPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTaxable { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateTariffRequest : CreateTariffRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class TariffCreateResponse
    {
        public Guid Id { get; set; }
        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;
        public Guid TariffCategoryId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? PatientClassId { get; set; }
        public decimal NormalPrice { get; set; }
        public bool IsActive { get; set; }
    }

    public class TariffUpdateResponse : TariffCreateResponse
    {
    }
}
