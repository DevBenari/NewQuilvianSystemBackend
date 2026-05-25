using QuilvianSystemBackend.Enum;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.SelfServices.DTOs
{
    public class SelfServiceProfileResponse
    {
        public Guid UserId { get; set; }

        public string UserCode { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string UserTypeName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string? PrimaryDepartmentName { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionName { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public bool CanChangeProfilePhoto { get; set; }

        public int MaxProfilePhotoSizeMb { get; set; }

        public List<string> AllowedProfilePhotoExtensions { get; set; } = new();
    }

    public class UpdateSelfServiceProfilePhotoRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;
    }

    public class SelfServiceProfileMetadataResponse
    {
        public int MaxProfilePhotoSizeMb { get; set; }

        public List<string> AllowedProfilePhotoExtensions { get; set; } = new();

        public string DefaultUserProfilePhotoPath { get; set; } = string.Empty;

        public string DefaultDoctorProfilePhotoPath { get; set; } = string.Empty;

        public string ProfilePhotoFolderName { get; set; } = string.Empty;

        public string PublicRequestPath { get; set; } = string.Empty;

        public string PublicBaseUrl { get; set; } = string.Empty;
    }

    public class ChangeSelfServicePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class GenerateSelfServicePasswordResponse
    {
        public string UsernameSource { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public string SuggestedPassword { get; set; } = string.Empty;

        public string Pattern { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }
    }
}
