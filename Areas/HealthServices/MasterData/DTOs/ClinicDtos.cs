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
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForKiosk { get; set; }
        public bool IsAvailableForAppointment { get; set; }
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
        public ClinicDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ClinicSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<ClinicEnumOptionResponse> ClinicTypeOptions { get; set; } = new();
    }

    public class ClinicDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public ClinicType? ClinicType { get; set; }
        public bool? IsAvailableForRegistration { get; set; }
        public bool? IsAvailableForKiosk { get; set; }
        public bool? IsAvailableForAppointment { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ClinicSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateClinicRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClinicCode { get; set; } = string.Empty;

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

    public class UpdateClinicRequest : CreateClinicRequest
    {
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
}