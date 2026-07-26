using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("TrxEmployeeProfileChangeVerification", Schema = "public")]
    public class TrxEmployeeProfileChangeVerification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProfileChangeRequestId { get; set; }

        public Guid? ProfileChangeDetailId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationType { get; set; } = "HR";

        [Required]
        [MaxLength(50)]
        public string VerificationStatus { get; set; } = "Pending";

        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsFinalVerification { get; set; } = false;

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        [MaxLength(500)]
        public string? EvidenceFilePath { get; set; }

        public TrxEmployeeProfileChangeRequest? ProfileChangeRequest { get; set; }
        public TrxEmployeeProfileChangeDetail? ProfileChangeDetail { get; set; }
    }
}
