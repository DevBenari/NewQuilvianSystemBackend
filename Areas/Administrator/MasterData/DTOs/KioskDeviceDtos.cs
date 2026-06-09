using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class KioskDeviceSummaryResponse
    {
        public int TotalDevice { get; set; }
        public int ActiveDevice { get; set; }
        public int InactiveDevice { get; set; }
        public int OnlineDevice { get; set; }
        public int OfflineDevice { get; set; }
        public int MaintenanceDevice { get; set; }
        public int RegistrationAvailableDevice { get; set; }
        public int CheckInAvailableDevice { get; set; }
        public int PaymentAvailableDevice { get; set; }
        public int WalkInAllowedDevice { get; set; }
        public int AppointmentAllowedDevice { get; set; }
        public int InsuranceRegistrationAllowedDevice { get; set; }
        public int WithServiceUnitDevice { get; set; }
        public int WithClinicDevice { get; set; }
        public int WithScannerProfileDevice { get; set; }
    }

    public class KioskDeviceResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public KioskDeviceType DeviceType { get; set; }
        public string DeviceTypeName { get; set; } = string.Empty;
        public KioskDeviceStatus DeviceStatus { get; set; }
        public string DeviceStatusName { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid? DefaultScannerProfileId { get; set; }
        public string? DefaultScannerProfileCode { get; set; }
        public string? DefaultScannerProfileName { get; set; }

        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? IpAddress { get; set; }
        public string? SerialNumber { get; set; }
        public string? DeviceModel { get; set; }
        public string? VendorName { get; set; }

        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForCheckIn { get; set; }
        public bool IsAvailableForPayment { get; set; }
        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowInsuranceRegistration { get; set; }

        public DateTime? LastOnlineAt { get; set; }
        public DateTime? LastOfflineAt { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class KioskDeviceDetailResponse : KioskDeviceResponse
    {
        public string? MacAddress { get; set; }
        public string? LastErrorMessage { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class KioskDeviceOptionResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public KioskDeviceType DeviceType { get; set; }
        public string DeviceTypeName { get; set; } = string.Empty;
        public KioskDeviceStatus DeviceStatus { get; set; }
        public string DeviceStatusName { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid? DefaultScannerProfileId { get; set; }
        public string? DefaultScannerProfileCode { get; set; }
        public string? DefaultScannerProfileName { get; set; }

        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForCheckIn { get; set; }
        public bool IsAvailableForPayment { get; set; }
        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowInsuranceRegistration { get; set; }
    }

    public class KioskDeviceOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<KioskDeviceOptionResponse> Items { get; set; } = new();
    }

    public class KioskDeviceEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class KioskDeviceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public KioskDeviceDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<KioskDeviceCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<KioskDeviceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<KioskDeviceEnumOptionResponse> DeviceTypeOptions { get; set; } = new();
        public List<KioskDeviceEnumOptionResponse> DeviceStatusOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class KioskDeviceDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? DefaultScannerProfileId { get; set; }
        public KioskDeviceType? DeviceType { get; set; }
        public KioskDeviceStatus? DeviceStatus { get; set; }
        public bool? IsAvailableForRegistration { get; set; }
        public bool? IsAvailableForCheckIn { get; set; }
        public bool? IsAvailableForPayment { get; set; }
        public bool? IsAllowWalkIn { get; set; }
        public bool? IsAllowAppointment { get; set; }
        public bool? IsAllowInsuranceRegistration { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class KioskDeviceCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class KioskDeviceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateKioskDeviceRequest
    {
        [Required]
        [MaxLength(50)]
        public string DeviceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DeviceName { get; set; } = string.Empty;

        public KioskDeviceType DeviceType { get; set; } = KioskDeviceType.Unknown;
        public KioskDeviceStatus DeviceStatus { get; set; } = KioskDeviceStatus.Active;

        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? DefaultScannerProfileId { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? DeviceModel { get; set; }

        [MaxLength(100)]
        public string? VendorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;
        public bool IsAvailableForCheckIn { get; set; } = true;
        public bool IsAvailableForPayment { get; set; } = false;
        public bool IsAllowWalkIn { get; set; } = true;
        public bool IsAllowAppointment { get; set; } = true;
        public bool IsAllowInsuranceRegistration { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateKioskDeviceRequest : CreateKioskDeviceRequest
    {
        public DateTime? LastOnlineAt { get; set; }
        public DateTime? LastOfflineAt { get; set; }

        [MaxLength(250)]
        public string? LastErrorMessage { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateKioskDeviceStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteKioskDeviceRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class KioskDeviceCreateResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public KioskDeviceType DeviceType { get; set; }
        public string DeviceTypeName { get; set; } = string.Empty;
        public KioskDeviceStatus DeviceStatus { get; set; }
        public string DeviceStatusName { get; set; } = string.Empty;
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? DefaultScannerProfileId { get; set; }
        public bool IsAvailableForRegistration { get; set; }
        public bool IsAvailableForCheckIn { get; set; }
        public bool IsAvailableForPayment { get; set; }
        public bool IsActive { get; set; }
    }

    public class KioskDeviceUpdateResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public KioskDeviceStatus DeviceStatus { get; set; }
        public string DeviceStatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class KioskDeviceDeleteResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}
