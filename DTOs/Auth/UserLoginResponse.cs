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