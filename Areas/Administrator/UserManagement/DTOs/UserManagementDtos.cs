using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Enum;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.DTOs
{
    public class UserManagementResponse
    {
        public Guid Id { get; set; }

        public string UserCode { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string UserTypeName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? DepartmentId { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public Guid? PositionId { get; set; }

        public string? PositionCode { get; set; }

        public string? PositionName { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public string? ProfileCode { get; set; }

        public string? ProfileName { get; set; }

        public string? ProfileType { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public List<string> Roles { get; set; } = new();

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateUserResponse
    {
        public UserManagementResponse User { get; set; } = new();

        public string InitialPassword { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "User type wajib dipilih.")]
        [EnumDataType(typeof(UserType))]
        public UserType UserType { get; set; }

        [Required(ErrorMessage = "Tanggal lahir wajib diisi.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Nomor identitas wajib diisi.")]
        [MaxLength(100)]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor handphone wajib diisi.")]
        [Phone(ErrorMessage = "Format nomor handphone tidak valid.")]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department wajib dipilih.")]
        public Guid DepartmentId { get; set; }

        [Required(ErrorMessage = "Position wajib dipilih.")]
        public Guid PositionId { get; set; }

        public Gender? Gender { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
    }

    public class CreateUserFieldRuleResponse
    {
        public int UserType { get; set; }

        public string UserTypeName { get; set; } = string.Empty;

        public List<string> RequiredFields { get; set; } = new();

        public List<string> OptionalFields { get; set; } = new();

        public List<string> NotRequiredFields { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "User type wajib dipilih.")]
        public UserType UserType { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [Phone(ErrorMessage = "Format nomor handphone tidak valid.")]
        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public DateTime? AccessValidUntil { get; set; }
    }

    public class ResetUserPasswordRequest
    {
        [Required(ErrorMessage = "Password baru wajib diisi.")]
        public string NewPassword { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; } = true;
    }

    public class EnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }

    public class CreateUserOptionsResponse
    {
        public string UsernameSource { get; set; } = "Email";

        public string PasswordRule { get; set; } =
            "Password otomatis dari tanggal lahir format ddMMMyyyy Indonesia, contoh 18Des1990.";

        public string DefaultProfilePhotoPath { get; set; } = string.Empty;

        public List<EnumOptionResponse> UserTypes { get; set; } = new();

        public List<EnumOptionResponse> Genders { get; set; } = new();

        public List<string> RequiredFields { get; set; } = new();

        public List<string> OptionalFields { get; set; } = new();

        public List<string> AutoGeneratedFields { get; set; } = new();
    }
}