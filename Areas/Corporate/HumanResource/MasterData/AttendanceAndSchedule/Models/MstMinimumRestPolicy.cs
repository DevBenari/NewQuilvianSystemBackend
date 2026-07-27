using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstMinimumRestPolicy", Schema = "public")]
    public class MstMinimumRestPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ShiftGroupId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MinimumRestPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MinimumRestPolicyName { get; set; } = string.Empty;

        public decimal MinimumRestHours { get; set; } = 8;

        public decimal MinimumRestHoursAfterNightShift { get; set; } = 10;

        public decimal MinimumRestHoursAfterOvertime { get; set; } = 8;

        public decimal MaximumDailyWorkHours { get; set; } = 12;

        public decimal MaximumWeeklyWorkHours { get; set; } = 40;

        public decimal MinimumWeeklyRestHours { get; set; } = 24;

        public bool ApplyToAllShifts { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstShiftGroup? ShiftGroup { get; set; }
    }
}
