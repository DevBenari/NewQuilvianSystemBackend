using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class PatientClassSummaryResponse
    {
        public int TotalPatientClass { get; set; }
        public int ActivePatientClass { get; set; }
        public int InactivePatientClass { get; set; }
        public int OutpatientClass { get; set; }
        public int InpatientClass { get; set; }
        public int EmergencyClass { get; set; }
        public int IntensiveCareClass { get; set; }
        public int NewbornClass { get; set; }
        public int RoomChargeClass { get; set; }
        public int TariffMappingClass { get; set; }
        public int DefaultClass { get; set; }
    }

    public class PatientClassResponse
    {
        public Guid Id { get; set; }
        public string PatientClassCode { get; set; } = string.Empty;
        public string PatientClassName { get; set; } = string.Empty;
        public PatientClassType PatientClassType { get; set; }
        public string? ExternalClassCode { get; set; }
        public string? ClassAlias { get; set; }
        public int ClassLevel { get; set; }
        public bool IsForOutpatient { get; set; }
        public bool IsForInpatient { get; set; }
        public bool IsForEmergency { get; set; }
        public bool IsForIntensiveCare { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsForRoomCharge { get; set; }
        public bool IsForTariffMapping { get; set; }
        public bool IsDefault { get; set; }
        public decimal? DefaultDailyRoomRate { get; set; }
        public decimal? DefaultRegistrationFee { get; set; }
        public decimal? DefaultConsultationFee { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientClassDetailResponse : PatientClassResponse
    {
        public string? Description { get; set; }
    }

    public class PatientClassOptionResponse
    {
        public Guid Id { get; set; }
        public string PatientClassCode { get; set; } = string.Empty;
        public string PatientClassName { get; set; } = string.Empty;
        public PatientClassType PatientClassType { get; set; }
        public string? ClassAlias { get; set; }
        public int ClassLevel { get; set; }
        public bool IsForOutpatient { get; set; }
        public bool IsForInpatient { get; set; }
        public bool IsForEmergency { get; set; }
        public bool IsForIntensiveCare { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsForRoomCharge { get; set; }
        public bool IsForTariffMapping { get; set; }
        public bool IsDefault { get; set; }
    }

    public class PatientClassEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientClassFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public PatientClassDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientClassCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PatientClassSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientClassEnumOptionResponse> PatientClassTypeOptions { get; set; } = new();
        public List<PatientClassQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<PatientClassFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<PatientClassFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class PatientClassDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientClassCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class PatientClassSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientClassQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class PatientClassFormFieldMetadataResponse
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

    public class CreatePatientClassRequest
    {
        [Required]
        [MaxLength(150)]
        public string PatientClassName { get; set; } = string.Empty;

        public PatientClassType PatientClassType { get; set; } = PatientClassType.Unknown;

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? ClassAlias { get; set; }

        public int ClassLevel { get; set; } = 0;

        public bool IsForOutpatient { get; set; } = false;
        public bool IsForInpatient { get; set; } = false;
        public bool IsForEmergency { get; set; } = false;
        public bool IsForIntensiveCare { get; set; } = false;
        public bool IsForNewborn { get; set; } = false;
        public bool IsForRoomCharge { get; set; } = false;
        public bool IsForTariffMapping { get; set; } = true;
        public bool IsDefault { get; set; } = false;

        public decimal? DefaultDailyRoomRate { get; set; }
        public decimal? DefaultRegistrationFee { get; set; }
        public decimal? DefaultConsultationFee { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdatePatientClassRequest : CreatePatientClassRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class PatientClassCreateResponse
    {
        public Guid Id { get; set; }
        public string PatientClassCode { get; set; } = string.Empty;
        public string PatientClassName { get; set; } = string.Empty;
        public PatientClassType PatientClassType { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class PatientClassUpdateResponse : PatientClassCreateResponse
    {
    }
}
