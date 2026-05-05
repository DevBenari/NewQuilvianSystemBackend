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

        public Guid? HospitalId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }
    }
}
