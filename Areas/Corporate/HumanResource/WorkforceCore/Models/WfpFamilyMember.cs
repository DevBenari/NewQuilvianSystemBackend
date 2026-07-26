using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpFamilyMember", Schema = "public")]
    public class WfpFamilyMember : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Relationship { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public Gender? Gender { get; set; }
        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? MaritalStatusText { get; set; }

        [MaxLength(200)]
        public string? Occupation { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        public bool IsEmergencyContact { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ICollection<WfpDependent> Dependents { get; set; } = new List<WfpDependent>();
    }
}
