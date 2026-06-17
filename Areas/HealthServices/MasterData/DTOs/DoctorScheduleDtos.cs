using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DoctorScheduleSummaryResponse
    {
        public int TotalSchedule { get; set; }
        public int ActiveSchedule { get; set; }
        public int InactiveSchedule { get; set; }
        public int AppointmentAvailableSchedule { get; set; }
        public int WalkInAvailableSchedule { get; set; }
        public int KioskAvailableSchedule { get; set; }
        public int TelemedicineAvailableSchedule { get; set; }
        public int SubstituteSchedule { get; set; }
        public int SuspendedSchedule { get; set; }
        public int ClosedSchedule { get; set; }
    }

    public class DoctorScheduleResponse
    {
        public Guid Id { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public DoctorScheduleType ScheduleType { get; set; }
        public DoctorScheduleStatus ScheduleStatus { get; set; }

        public Guid DoctorId { get; set; }
        public string DoctorCode { get; set; } = string.Empty;
        public string DoctorNumber { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialistName { get; set; }
        public string? SubSpecialistName { get; set; }
        public string? MedicalStaffGroup { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? DoctorPhotoPath { get; set; }
        public string? DoctorPhotoUrl { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid ClinicId { get; set; }
        public string ClinicCode { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;

        public Guid? RoomId { get; set; }
        public string? RoomCode { get; set; }
        public string? RoomMasterName { get; set; }

        public DayOfWeek PracticeDay { get; set; }
        public DateTime? PracticeDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOvernight { get; set; }

        public string? SessionName { get; set; }
        public string? PracticeLocation { get; set; }
        public string? RoomName { get; set; }

        public int MaxPatientQuota { get; set; }
        public int MaxAppointmentQuota { get; set; }
        public int MaxWalkInQuota { get; set; }
        public int EstimatedServiceMinutes { get; set; }

        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowKioskRegistration { get; set; }
        public bool IsTelemedicineAvailable { get; set; }

        public bool IsSubstituteSchedule { get; set; }
        public Guid? SubstituteDoctorId { get; set; }
        public string? SubstituteDoctorCode { get; set; }
        public string? SubstituteDoctorName { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DoctorScheduleDetailResponse : DoctorScheduleResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DoctorScheduleOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialistName { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? DoctorPhotoPath { get; set; }
        public string? DoctorPhotoUrl { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;

        public Guid? RoomId { get; set; }
        public string? RoomMasterName { get; set; }

        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public DoctorScheduleType ScheduleType { get; set; }
        public DoctorScheduleStatus ScheduleStatus { get; set; }

        public DayOfWeek PracticeDay { get; set; }
        public DateTime? PracticeDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string? SessionName { get; set; }

        public int MaxPatientQuota { get; set; }
        public int MaxAppointmentQuota { get; set; }
        public int MaxWalkInQuota { get; set; }
        public int EstimatedServiceMinutes { get; set; }

        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowKioskRegistration { get; set; }
        public bool IsTelemedicineAvailable { get; set; }
    }

    public class DoctorScheduleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DoctorScheduleOptionResponse> Items { get; set; } = new();
    }

    public class DoctorScheduleEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorScheduleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeFormat { get; set; } = "HH:mm:ss";
        public DoctorScheduleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DoctorScheduleCustomPeriodResponse> CustomPeriods { get; set; } = new();
        public List<DoctorScheduleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DoctorScheduleEnumOptionResponse> ScheduleTypeOptions { get; set; } = new();
        public List<DoctorScheduleEnumOptionResponse> ScheduleStatusOptions { get; set; } = new();
        public List<DoctorScheduleEnumOptionResponse> PracticeDayOptions { get; set; } = new();
        public List<DoctorScheduleQueryParameterResponse> QueryParameters { get; set; } = new();
        public List<DoctorScheduleFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DoctorScheduleFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DoctorScheduleDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DoctorScheduleCustomPeriodResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorScheduleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorScheduleQueryParameterResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DoctorScheduleFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public bool IsVisible { get; set; } = true;
        public object? DefaultValue { get; set; }
        public string? OptionsSource { get; set; }
    }

    public class CreateDoctorScheduleRequest
    {
        [Required]
        [MaxLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        public DoctorScheduleType ScheduleType { get; set; } = DoctorScheduleType.WeeklyRecurring;

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid ClinicId { get; set; }

        public Guid? RoomId { get; set; }

        public DayOfWeek PracticeDay { get; set; } = DayOfWeek.Monday;

        public DateTime? PracticeDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        [MaxLength(100)]
        public string? SessionName { get; set; }

        [MaxLength(100)]
        public string? PracticeLocation { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public int MaxPatientQuota { get; set; } = 0;

        public int MaxAppointmentQuota { get; set; } = 0;

        public int MaxWalkInQuota { get; set; } = 0;

        public int EstimatedServiceMinutes { get; set; } = 15;

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowKioskRegistration { get; set; } = true;

        public bool IsTelemedicineAvailable { get; set; } = false;

        public bool IsSubstituteSchedule { get; set; } = false;

        public Guid? SubstituteDoctorId { get; set; }

        public DoctorScheduleStatus ScheduleStatus { get; set; } = DoctorScheduleStatus.Active;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDoctorScheduleRequest : CreateDoctorScheduleRequest
    {
    }

    public class UpdateDoctorScheduleStatusRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DeleteDoctorScheduleRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DoctorScheduleCreateResponse
    {
        public Guid Id { get; set; }
        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public DoctorScheduleType ScheduleType { get; set; }
        public DoctorScheduleStatus ScheduleStatus { get; set; }
        public Guid DoctorId { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid ClinicId { get; set; }
        public Guid? RoomId { get; set; }
        public DayOfWeek PracticeDay { get; set; }
        public DateTime? PracticeDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }

    public class DoctorScheduleUpdateResponse
    {
        public Guid Id { get; set; }
        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public DoctorScheduleStatus ScheduleStatus { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DoctorScheduleStatusResponse
    {
        public Guid Id { get; set; }
        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DoctorScheduleDeleteResponse
    {
        public Guid Id { get; set; }
        public string ScheduleCode { get; set; } = string.Empty;
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
