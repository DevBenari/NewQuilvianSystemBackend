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
}
