using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstDoctor", Schema = "public")]
    public class MstDoctor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string DoctorCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public DoctorType DoctorType { get; set; } = DoctorType.PermanentDoctor;

        public Gender? Gender { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(50)]
        public string? IdentityNumber { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? SpecialistName { get; set; }

        [MaxLength(100)]
        public string? SubSpecialistName { get; set; }

        [MaxLength(100)]
        public string? MedicalStaffGroup { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsAvailableForAppointment { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public MstDepartment? PrimaryDepartment { get; set; }

        public MstPosition? PrimaryPosition { get; set; }

        public ICollection<DctLicense> Licenses { get; set; } = new List<DctLicense>();

        public DctPracticeProfile? PracticeProfile { get; set; }

        public DctFeeProfile? FeeProfile { get; set; }
    }
}