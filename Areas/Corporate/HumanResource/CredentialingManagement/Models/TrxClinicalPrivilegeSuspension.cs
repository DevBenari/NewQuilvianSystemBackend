using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeSuspension", Schema = "public")]
    public class TrxClinicalPrivilegeSuspension : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClinicalPrivilegeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SuspensionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string SuspensionStatus { get; set; } = "Active";

        public DateTime SuspensionStartDate { get; set; }

        public DateTime? SuspensionEndDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string SuspensionReason { get; set; } = string.Empty;

        public bool BlocksScheduling { get; set; } = true;

        public bool BlocksClinicalService { get; set; } = true;

        public Guid? SuspendedByUserId { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public Guid? ReinstatedByUserId { get; set; }

        public DateTime? ReinstatedAt { get; set; }

        [MaxLength(2000)]
        public string? ReinstatementReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpClinicalPrivilege? ClinicalPrivilege { get; set; }
        public ApplicationUser? SuspendedByUser { get; set; }
        public ApplicationUser? ReinstatedByUser { get; set; }

    }
}
