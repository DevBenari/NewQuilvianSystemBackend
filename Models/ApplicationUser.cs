using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string UserCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public bool IsGeolocationBypassEnabled { get; set; } = false;

        public string? GeolocationBypassReason { get; set; }

        public DateTime? GeolocationBypassUntil { get; set; }

        public bool IsActive { get; set; } = true;

        public bool MustChangePassword { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDateTime { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstExternalUser? ExternalUser { get; set; }

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public bool IsFingerprintRegistrationEnabled { get; set; } = false;

        public string? FingerprintRegistrationReason { get; set; }

        public DateTime? FingerprintRegistrationEnabledAt { get; set; }

        public Guid? FingerprintRegistrationEnabledByUserId { get; set; }

        public ICollection<ApplicationUserOrganization> DepartmentPositions { get; set; } = new List<ApplicationUserOrganization>();
        public ICollection<ApplicationUserFingerprintCredential> FingerprintCredentials { get; set; } = new List<ApplicationUserFingerprintCredential>();
    }
}
