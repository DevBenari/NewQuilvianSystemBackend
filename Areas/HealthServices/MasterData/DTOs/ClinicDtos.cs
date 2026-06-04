using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class ClinicSummaryResponse
    {
        public int TotalClinic { get; set; }
        public int ActiveClinic { get; set; }
        public int InactiveClinic { get; set; }
        public int RegistrationAvailableClinic { get; set; }
        public int KioskAvailableClinic { get; set; }
        public int AppointmentAvailableClinic { get; set; }
        public int DoctorRequiredClinic { get; set; }
        public int ScreeningRequiredClinic { get; set; }
    }

    public class ClinicResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public string ClinicCode { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public ClinicType ClinicType { get; set; }
        public string? ShortName { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForKiosk { get; set; }
        public bool IsAvailableForAppointment { get; set; }
        public bool IsDoctorRequired { get; set; }
        public bool IsScreeningRequired { get; set; }
        public bool IsQueueRequired { get; set; }
        public int DefaultEstimatedServiceMinutes { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class ClinicDetailResponse : ClinicResponse
    {
        public string? Description { get; set; }
    }

    public class ClinicOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public string ClinicCode { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public ClinicType ClinicType { get; set; }
        public string? ShortName { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForKiosk { get; set; }
        public bool IsAvailableForAppointment { get; set; }
    }

    public class ClinicOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ClinicOptionResponse> Items { get; set; } = new();
    }

    public class ClinicEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public ClinicDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ClinicCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ClinicSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<ClinicEnumOptionResponse> ClinicTypeOptions { get; set; } = new();
        public List<ClinicQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<ClinicFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<ClinicFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class ClinicDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ClinicCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class ClinicSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class ClinicFormFieldMetadataResponse
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

    public class CreateClinicRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ClinicName { get; set; } = string.Empty;

        public ClinicType ClinicType { get; set; } = ClinicType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;
        public bool IsAvailableForKiosk { get; set; } = true;
        public bool IsAvailableForAppointment { get; set; } = true;
        public bool IsDoctorRequired { get; set; } = true;
        public bool IsScreeningRequired { get; set; } = true;
        public bool IsQueueRequired { get; set; } = true;
        public int DefaultEstimatedServiceMinutes { get; set; } = 15;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateClinicRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ClinicName { get; set; } = string.Empty;

        public ClinicType ClinicType { get; set; } = ClinicType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;
        public bool IsAvailableForKiosk { get; set; } = true;
        public bool IsAvailableForAppointment { get; set; } = true;
        public bool IsDoctorRequired { get; set; } = true;
        public bool IsScreeningRequired { get; set; } = true;
        public bool IsQueueRequired { get; set; } = true;
        public int DefaultEstimatedServiceMinutes { get; set; } = 15;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ClinicCreateResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ClinicCode { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public ClinicType ClinicType { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClinicUpdateResponse : ClinicCreateResponse
    {
    }
}
