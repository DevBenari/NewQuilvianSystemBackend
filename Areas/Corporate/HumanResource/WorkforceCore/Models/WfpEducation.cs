using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpEducation", Schema = "public")]
    public class WfpEducation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Major { get; set; }

        public int? GraduationYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CountryId { get; set; }

        [MaxLength(150)]
        public string? CertificateNumber { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public bool IsHighestEducation { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCountry? Country { get; set; }
    }
}
