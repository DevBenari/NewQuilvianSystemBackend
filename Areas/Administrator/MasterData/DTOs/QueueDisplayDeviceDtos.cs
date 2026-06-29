using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class QueueDisplayDeviceSummaryResponse
    {
        public int TotalDevice { get; set; }
        public int ActiveDevice { get; set; }
        public int InactiveDevice { get; set; }
        public int VoiceCallingEnabledDevice { get; set; }
        public int ShowPatientNameDevice { get; set; }
        public int WithPairingCodeDevice { get; set; }
        public int WithDeviceTokenDevice { get; set; }
    }

    public class QueueDisplayDeviceResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterCode { get; set; }
        public string? ClusterName { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public QueueDisplayDeviceType DisplayDeviceType { get; set; }
        public string DisplayDeviceTypeName { get; set; } = string.Empty;
        public QueueDisplayLayoutType LayoutType { get; set; }
        public string LayoutTypeName { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? PairingCode { get; set; }
        public bool EnableVoiceCalling { get; set; }
        public bool ShowPatientName { get; set; }
        public bool ShowDoctorName { get; set; }
        public bool ShowClinicName { get; set; }
        public int RefreshIntervalSeconds { get; set; }
        public DateTime? LastOnlineDateTime { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class QueueDisplayDeviceDetailResponse : QueueDisplayDeviceResponse
    {
        public string? DeviceToken { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class QueueDisplayDeviceOptionResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterName { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public QueueDisplayDeviceType DisplayDeviceType { get; set; }
        public string DisplayDeviceTypeName { get; set; } = string.Empty;
        public QueueDisplayLayoutType LayoutType { get; set; }
        public string LayoutTypeName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class QueueDisplayDeviceOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<QueueDisplayDeviceOptionResponse> Items { get; set; } = new();
    }

    public class QueueDisplayDeviceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public QueueDisplayDeviceDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<QueueDisplayDeviceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<QueueDisplayDeviceEnumMetadataResponse> EnumOptions { get; set; } = new();
    }

    public class QueueDisplayDeviceDefaultFilterResponse
    {
        public Guid? NurseStationClusterId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public QueueDisplayDeviceType? DisplayDeviceType { get; set; }
        public QueueDisplayLayoutType? LayoutType { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class QueueDisplayDeviceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class QueueDisplayDeviceEnumMetadataResponse
    {
        public string EnumName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public List<QueueDisplayDeviceEnumOptionResponse> Options { get; set; } = new();
    }

    public class QueueDisplayDeviceEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateQueueDisplayDeviceRequest
    {
        [Required]
        public Guid NurseStationClusterId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        [Required]
        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;
        public QueueDisplayDeviceType DisplayDeviceType { get; set; } = QueueDisplayDeviceType.TvDisplay;
        public QueueDisplayLayoutType LayoutType { get; set; } = QueueDisplayLayoutType.ClusterBoard;
        [MaxLength(100)]
        public string? LocationName { get; set; }
        [MaxLength(50)]
        public string? FloorName { get; set; }
        [MaxLength(50)]
        public string? RoomName { get; set; }
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        [MaxLength(100)]
        public string? MacAddress { get; set; }
        [MaxLength(100)]
        public string? PairingCode { get; set; }
        [MaxLength(200)]
        public string? DeviceToken { get; set; }
        public bool EnableVoiceCalling { get; set; } = true;
        public bool ShowPatientName { get; set; } = false;
        public bool ShowDoctorName { get; set; } = true;
        public bool ShowClinicName { get; set; } = true;
        public int RefreshIntervalSeconds { get; set; } = 5;
        public int SortOrder { get; set; } = 0;
        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateQueueDisplayDeviceRequest : CreateQueueDisplayDeviceRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateQueueDisplayDeviceStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteQueueDisplayDeviceRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class QueueDisplayDeviceCreateResponse
    {
        public Guid Id { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class QueueDisplayDeviceLoginSummaryResponse
    {
        public int TotalDevice { get; set; }
        public int DeviceWithLogin { get; set; }
        public int DeviceWithoutLogin { get; set; }
        public int EnabledLogin { get; set; }
        public int DisabledOrLockedLogin { get; set; }
        public int ActiveDeviceWithoutLogin { get; set; }
    }

    public class QueueDisplayDeviceLoginInfoResponse
    {
        public Guid QueueDisplayDeviceId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public Guid NurseStationClusterId { get; set; }
        public string? ClusterName { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public QueueDisplayDeviceType DisplayDeviceType { get; set; }
        public string DisplayDeviceTypeName { get; set; } = string.Empty;
        public QueueDisplayLayoutType LayoutType { get; set; }
        public string LayoutTypeName { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public string? IpAddress { get; set; }
        public bool IsActive { get; set; }
        public Guid? LoginUserId { get; set; }
        public string? LoginUserCode { get; set; }
        public string? LoginUserName { get; set; }
        public string? LoginEmail { get; set; }
        public string? LoginDisplayName { get; set; }
        public bool IsLoginCreated { get; set; }
        public bool IsLoginEnabled { get; set; }
        public bool IsLoginLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public bool CanLogin { get; set; }
    }

    public class QueueDisplayDeviceLoginInfoPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<QueueDisplayDeviceLoginInfoResponse> Items { get; set; } = new();
    }

    public class GenerateQueueDisplayDeviceLoginRequest
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Password { get; set; }

        public bool IsEnabled { get; set; } = true;
    }

    public class QueueDisplayDeviceGenerateLoginResponse
    {
        public Guid QueueDisplayDeviceId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        // Alias kompatibilitas agar frontend yang sudah memakai pola KioskDevice
        // tetap bisa membaca deviceId/deviceCode/deviceName secara generik.
        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public Guid LoginUserId { get; set; }
        public string LoginUserCode { get; set; } = string.Empty;
        public string LoginUserName { get; set; } = string.Empty;
        public string? LoginEmail { get; set; }
        public string GeneratedPassword { get; set; } = string.Empty;
        public bool IsLoginEnabled { get; set; }
        public bool IsLoginLocked { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ResetQueueDisplayDeviceLoginRequest
    {
        [MaxLength(100)]
        public string? NewPassword { get; set; }
    }

    public class QueueDisplayDeviceResetLoginResponse
    {
        public Guid QueueDisplayDeviceId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public Guid LoginUserId { get; set; }
        public string LoginUserName { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class UpdateQueueDisplayDeviceLoginStatusRequest
    {
        public bool IsEnabled { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }
    }

    public class QueueDisplayDeviceLoginStatusResponse
    {
        public Guid QueueDisplayDeviceId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public Guid? LoginUserId { get; set; }
        public string? LoginUserName { get; set; }
        public bool IsLoginCreated { get; set; }
        public bool IsLoginEnabled { get; set; }
        public bool IsLoginLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class QueueDisplayDeviceLoginRequest
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DisplayCode { get; set; }

        // Alias kompatibilitas bila frontend mengirim field generik seperti KioskDevice.
        [MaxLength(50)]
        public string? DeviceCode { get; set; }
    }

    public class QueueDisplayDeviceLoginResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? QueueDisplayDeviceId { get; set; }
        public string? DisplayCode { get; set; }
        public string? DisplayName { get; set; }

        // Alias kompatibilitas agar pola response login mirip KioskDevice.
        public Guid? DeviceId { get; set; }
        public string? DeviceCode { get; set; }
        public string? DeviceName { get; set; }

        public Guid? NurseStationClusterId { get; set; }
        public string? ClusterName { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public QueueDisplayDeviceType? DisplayDeviceType { get; set; }
        public string? DisplayDeviceTypeName { get; set; }
        public QueueDisplayLayoutType? LayoutType { get; set; }
        public string? LayoutTypeName { get; set; }
        public bool EnableVoiceCalling { get; set; }
        public bool ShowPatientName { get; set; }
        public bool ShowDoctorName { get; set; }
        public bool ShowClinicName { get; set; }
        public int RefreshIntervalSeconds { get; set; }
        public Guid? LoginUserId { get; set; }
        public string? LoginUserName { get; set; }
        public bool IsLoginEnabled { get; set; }
        public bool IsLoginLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        public string LoginContext { get; set; } = "QueueDisplay";
        public string FrontendRouteName { get; set; } = "QUEUE_DISPLAY";
        public string? RedirectPath { get; set; }
    }

}
