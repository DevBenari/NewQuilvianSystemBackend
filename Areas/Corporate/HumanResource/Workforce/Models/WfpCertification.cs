using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpCertification", Schema = "public")]
    public class WfpCertification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty;
        // Clinical, NonClinical, Safety, Quality, IT

        [Required]
        [MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;
        // BLS, ACLS, BTCLS, PPGD

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}