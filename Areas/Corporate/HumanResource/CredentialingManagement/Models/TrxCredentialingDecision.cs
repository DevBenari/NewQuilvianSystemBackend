using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxCredentialingDecision", Schema = "public")]
    public class TrxCredentialingDecision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CredentialingApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DecisionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DecisionType { get; set; } = "Credentialing";

        [Required]
        [MaxLength(30)]
        public string DecisionStatus { get; set; } = "Pending";

        public DateTime DecisionDate { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(2000)]
        public string? ApprovedScope { get; set; }

        [MaxLength(2000)]
        public string? Conditions { get; set; }

        [MaxLength(2000)]
        public string? DecisionReason { get; set; }

        public Guid? DecisionByUserId { get; set; }

        public Guid? CommitteeChairUserId { get; set; }


        public bool BlocksScheduling { get; set; } = false;

        public bool BlocksClinicalService { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public ApplicationUser? DecisionByUser { get; set; }
        public ApplicationUser? CommitteeChairUser { get; set; }

    }
}
