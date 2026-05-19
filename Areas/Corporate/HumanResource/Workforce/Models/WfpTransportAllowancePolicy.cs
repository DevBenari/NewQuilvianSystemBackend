using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpTransportAllowancePolicy", Schema = "public")]
    public class WfpTransportAllowancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "DailyAttendance";
        // None, FixedMonthly, DailyAttendance, NightShift, MonthlyAndNightShift, Manual

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}