using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Employee.Models
{
    [Table("EmpTransportAllowancePolicy", Schema = "public")]
    public class EmpTransportAllowancePolicy : IdentityModel
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

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeSpan? NightStartTime { get; set; }

        public TimeSpan? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
