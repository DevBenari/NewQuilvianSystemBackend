using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeRevocation", Schema = "public")]
    public class TrxClinicalPrivilegeRevocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClinicalPrivilegeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RevocationNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RevocationStatus { get; set; } = "Final";

        public DateTime RevocationDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string RevocationReason { get; set; } = string.Empty;

        public bool BlocksScheduling { get; set; } = true;

        public bool BlocksClinicalService { get; set; } = true;

        public Guid? RevokedByUserId { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool IsAppealable { get; set; } = false;

        public DateTime? AppealDeadline { get; set; }

        [MaxLength(30)]
        public string? AppealStatus { get; set; }

        [MaxLength(100)]
        public string? DecisionReferenceNumber { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpClinicalPrivilege? ClinicalPrivilege { get; set; }
        public ApplicationUser? RevokedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

    }
}
