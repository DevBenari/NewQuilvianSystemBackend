using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models
{
    [Table("MstOvertimeRate", Schema = "public")]
    public class MstOvertimeRate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimePolicyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OvertimeRateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OvertimeRateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DayType { get; set; } = "Workday";
        // Workday, RestDay, Holiday, SpecialHoliday.

        [Required]
        [MaxLength(50)]
        public string TimeBand { get; set; } = "AllDay";
        // AllDay, FirstHour, NextHour, Night, Custom.

        [Required]
        [MaxLength(50)]
        public string CalculationMethod { get; set; } = "Multiplier";
        // Multiplier, FixedAmount, HigherOfMultiplierOrFixed.

        public decimal RateMultiplier { get; set; } = 1;

        public decimal? FixedAmount { get; set; }

        public int StartMinute { get; set; } = 0;

        public int? EndMinute { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public int MinimumEligibleMinutes { get; set; } = 0;

        public int? MaximumEligibleMinutes { get; set; }

        public int Priority { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstOvertimePolicy? OvertimePolicy { get; set; }
    }
}
