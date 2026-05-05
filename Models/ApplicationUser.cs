using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Enum;

namespace QuilvianSystemBackend.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string UserCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IdentityNumber { get; set; }

        public Guid? HospitalId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool MustChangePassword { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDateTime { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }

        public MstEmployee? Employee { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstExternalUser? ExternalUser { get; set; }
    }
}
