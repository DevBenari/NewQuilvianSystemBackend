using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstWorkforceRequirement", Schema = "public")]
    public class MstWorkforceRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;
        // Document, Education, Training, Certification, License

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;
        // KTP, NPWP, IJAZAH_TERAKHIR, STR, SIP, BLS

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
