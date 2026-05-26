using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientIdentityDocument", Schema = "public")]
    public class MstPatientIdentityDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IdentityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IdentityNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DocumentName { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(100)]
        public string? IssuedBy { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(250)]
        public string? VerificationNote { get; set; }

        public bool IsFromKioskScan { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }
    }
}