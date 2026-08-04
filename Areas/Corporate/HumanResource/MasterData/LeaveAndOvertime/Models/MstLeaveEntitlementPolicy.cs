using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstLeaveEntitlementPolicy", Schema = "public")]
    public class MstLeaveEntitlementPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeavePolicyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntitlementPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EntitlementPolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EntitlementMethod { get; set; }
            = LeaveValueConstants.EntitlementMethod.AnnualGrant;

        [Required]
        [MaxLength(50)]
        public string PeriodBasis { get; set; }
            = LeaveValueConstants.PeriodBasis.CalendarYear;

        [Required]
        [MaxLength(50)]
        public string GrantTiming { get; set; }
            = LeaveValueConstants.GrantTiming.StartOfPeriod;

        public decimal AnnualEntitlementDays { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string AccrualFrequency { get; set; } = "Annual";

        [Required]
        [MaxLength(50)]
        public string AccrualTiming { get; set; }
            = LeaveValueConstants.AccrualTiming.EndOfPeriod;

        public decimal AccrualAmountDays { get; set; } = 0;
        public int? AccrualStartMonth { get; set; }
        public int? AccrualStartDay { get; set; }
        public int? AccrualDayOfMonth { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstAccrualRule { get; set; }
            = LeaveValueConstants.FirstAccrualRule.Prorated;

        [Required]
        [MaxLength(50)]
        public string FinalAccrualRule { get; set; }
            = LeaveValueConstants.FinalAccrualRule.Prorated;

        public decimal? AccrualMaximumPerPeriodDays { get; set; }

        public bool IsProratedOnJoin { get; set; } = true;
        public bool IsProratedOnSeparation { get; set; } = true;
        public int MinimumServiceMonths { get; set; } = 0;
        public decimal? MaximumBalanceDays { get; set; }

        public int? ResetMonth { get; set; }
        public int? ResetDay { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoundingMethod { get; set; }
            = LeaveValueConstants.RoundingMethod.None;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public MstLeavePolicy? LeavePolicy { get; set; }

        public ICollection<MstLeaveCarryForwardPolicy> CarryForwardPolicies { get; set; }
            = new List<MstLeaveCarryForwardPolicy>();
    }
}
