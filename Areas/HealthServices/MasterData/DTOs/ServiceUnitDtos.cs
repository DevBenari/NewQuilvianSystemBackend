using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class ServiceUnitSummaryResponse
    {
        public int TotalServiceUnit { get; set; }
        public int ActiveServiceUnit { get; set; }
        public int InactiveServiceUnit { get; set; }
        public int RegistrationAvailableServiceUnit { get; set; }
        public int KioskAvailableServiceUnit { get; set; }
        public int AppointmentAvailableServiceUnit { get; set; }
        public int DoctorRequiredServiceUnit { get; set; }
        public int ScreeningRequiredServiceUnit { get; set; }
    }

    public class ServiceUnitResponse
    {
        public Guid Id { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public ServiceUnitType ServiceUnitType { get; set; }
        public string? ShortName { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForKiosk { get; set; }
        public bool IsAvailableForAppointment { get; set; }
        public bool IsQueueRequired { get; set; }
        public bool IsDoctorRequired { get; set; }
        public bool IsScreeningRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class ServiceUnitDetailResponse : ServiceUnitResponse
    {
        public string? Description { get; set; }
    }

    public class ServiceUnitOptionResponse
    {
        public Guid Id { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public ServiceUnitType ServiceUnitType { get; set; }
        public string? ShortName { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForKiosk { get; set; }
        public bool IsAvailableForAppointment { get; set; }
    }

    public class ServiceUnitOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ServiceUnitOptionResponse> Items { get; set; } = new();
    }

    public class ServiceUnitEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ServiceUnitFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public ServiceUnitDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ServiceUnitCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ServiceUnitSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<ServiceUnitEnumOptionResponse> ServiceUnitTypeOptions { get; set; } = new();
        public List<ServiceUnitQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<ServiceUnitFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<ServiceUnitFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class ServiceUnitDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public ServiceUnitType? ServiceUnitType { get; set; }
        public bool? IsAvailableForRegistration { get; set; }
        public bool? IsAvailableForKiosk { get; set; }
        public bool? IsAvailableForAppointment { get; set; }
        public bool? IsDoctorRequired { get; set; }
        public bool? IsScreeningRequired { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ServiceUnitCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class ServiceUnitSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ServiceUnitQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class ServiceUnitFormFieldMetadataResponse
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

    public class CreateServiceUnitRequest
    {
        [Required]
        [MaxLength(150)]
        public string ServiceUnitName { get; set; } = string.Empty;

        public ServiceUnitType ServiceUnitType { get; set; } = ServiceUnitType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;
        public bool IsAvailableForKiosk { get; set; } = false;
        public bool IsAvailableForAppointment { get; set; } = false;
        public bool IsQueueRequired { get; set; } = true;
        public bool IsDoctorRequired { get; set; } = false;
        public bool IsScreeningRequired { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateServiceUnitRequest
    {
        [Required]
        [MaxLength(150)]
        public string ServiceUnitName { get; set; } = string.Empty;

        public ServiceUnitType ServiceUnitType { get; set; } = ServiceUnitType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;
        public bool IsAvailableForKiosk { get; set; } = false;
        public bool IsAvailableForAppointment { get; set; } = false;
        public bool IsQueueRequired { get; set; } = true;
        public bool IsDoctorRequired { get; set; } = false;
        public bool IsScreeningRequired { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ServiceUnitCreateResponse
    {
        public Guid Id { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public ServiceUnitType ServiceUnitType { get; set; }
        public bool IsActive { get; set; }
    }
}
