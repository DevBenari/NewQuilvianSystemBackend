using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstShift", Schema = "public")]
    public class MstShift : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkScheduleId { get; set; }

        public Guid? ShiftGroupId { get; set; }

        public Guid? OnCallTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShiftCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ShiftName { get; set; } = string.Empty;

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int BreakDurationMinutes { get; set; } = 0;

        public int PaidWorkMinutes { get; set; } = 0;

        public bool IsOvernight { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        public bool IsOffShift { get; set; } = false;

        public bool AllowOvertime { get; set; } = true;

        [MaxLength(20)]
        public string? ColorCode { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstWorkSchedule? WorkSchedule { get; set; }

        public MstShiftGroup? ShiftGroup { get; set; }

        public MstOnCallType? OnCallType { get; set; }
    }
}
