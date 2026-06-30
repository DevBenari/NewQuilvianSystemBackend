namespace QuilvianSystemBackend.DTOs.Auth
{
    public class UserLoginResponse
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string UserType { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public bool HasWorkforceProfile { get; set; }

        public string ProfileType { get; set; } = string.Empty;

        public UserWorkforceContextResponse WorkforceContext { get; set; } = new();

        public bool IsKioskAccount { get; set; }

        public UserKioskContextResponse? KioskContext { get; set; }

        public bool IsQueueDisplayAccount { get; set; }

        public UserQueueDisplayContextResponse? QueueDisplayContext { get; set; }
    }

    public class UserWorkforceContextResponse
    {
        public Guid UserId { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public bool CanAccessWorkforceModules { get; set; }

        public string? WorkforceProfileBaseEndpoint { get; set; }
    }

    public class UserQueueDisplayContextResponse
    {
        public Guid UserId { get; set; }

        public Guid QueueDisplayDeviceId { get; set; }

        public Guid DeviceId { get; set; }

        public string DisplayCode { get; set; } = string.Empty;

        public string DeviceCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public Guid NurseStationClusterId { get; set; }

        public string? ClusterName { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? DisplayDeviceTypeName { get; set; }

        public string? LayoutTypeName { get; set; }

        public string? LocationName { get; set; }

        public string? FloorName { get; set; }

        public string? RoomName { get; set; }

        public bool EnableVoiceCalling { get; set; }

        public bool ShowPatientName { get; set; }

        public bool ShowDoctorName { get; set; }

        public bool ShowClinicName { get; set; }

        public int RefreshIntervalSeconds { get; set; }

        public bool IsDeviceActive { get; set; }

        public bool IsLoginCreated { get; set; }

        public bool IsLoginEnabled { get; set; }

        public bool IsLoginLocked { get; set; }

        public bool CanLogin { get; set; }

        public string LoginContext { get; set; } = "QueueDisplay";

        public string FrontendRouteName { get; set; } = "QUEUE_DISPLAY";

        public string RedirectPath { get; set; } = "/queue-display";

        public string QueueDisplayBaseEndpoint { get; set; } = "/api/v1/health-services/registration-management/queue-display-runtime";
    }

    public class UserKioskContextResponse
    {
        public Guid UserId { get; set; }

        public Guid KioskDeviceId { get; set; }

        public string DeviceCode { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public string? DeviceTypeName { get; set; }

        public string? DeviceStatusName { get; set; }

        public string? LocationName { get; set; }

        public string? FloorName { get; set; }

        public bool IsDeviceActive { get; set; }

        public bool IsLoginCreated { get; set; }

        public bool IsLoginEnabled { get; set; }

        public bool IsLoginLocked { get; set; }

        public bool CanLogin { get; set; }

        public bool IsAllowWalkIn { get; set; }

        public bool IsAllowAppointment { get; set; }

        public bool IsAllowInsuranceRegistration { get; set; }

        public string KioskBaseEndpoint { get; set; } = "/api/v1/kiosk";
    }
}