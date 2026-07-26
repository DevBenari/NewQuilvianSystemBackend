using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstLeaveCarryForwardPolicy", Schema = "public")]
    public class MstLeaveCarryForwardPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveEntitlementPolicyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CarryForwardPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CarryForwardPolicyName { get; set; } = string.Empty;

        public bool IsCarryForwardEnabled { get; set; } = true;

        public decimal? MaximumCarryForwardDays { get; set; }

        public decimal CarryForwardPercentage { get; set; } = 100;

        [Required]
        [MaxLength(50)]
        public string ExpiryMethod { get; set; } = "MonthsAfterCarryForward";
        // NoExpiry, MonthsAfterCarryForward, FixedDate.

        public int? ExpiryMonths { get; set; }

        public int? ExpiryMonth { get; set; }

        public int? ExpiryDay { get; set; }

        public bool IsPayoutAllowed { get; set; } = false;

        public decimal? PayoutMaximumDays { get; set; }

        [Required]
        [MaxLength(50)]
        public string ExcessBalanceAction { get; set; } = "Forfeit";
        // Forfeit, Payout, KeepWithoutExpiry, ManualReview.

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstLeaveEntitlementPolicy? LeaveEntitlementPolicy { get; set; }
    }
}
