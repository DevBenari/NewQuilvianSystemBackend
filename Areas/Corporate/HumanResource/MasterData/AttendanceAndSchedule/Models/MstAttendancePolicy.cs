using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstAttendancePolicy", Schema = "public")]
    public class MstAttendancePolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? WorkScheduleId { get; set; }

        public Guid? GracePeriodPolicyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AttendancePolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AttendancePolicyName { get; set; } = string.Empty;

        public bool RequireCheckIn { get; set; } = true;

        public bool RequireCheckOut { get; set; } = true;

        public bool AllowMultipleCheckInOut { get; set; } = false;

        public bool AutoCloseOpenAttendance { get; set; } = false;

        public int? AutoCloseAfterMinutes { get; set; }

        public int MinimumWorkMinutes { get; set; } = 0;

        public int MaximumWorkMinutes { get; set; } = 1440;

        public bool IsOvertimeEnabled { get; set; } = true;

        public int OvertimeThresholdMinutes { get; set; } = 0;

        public bool IsAttendanceLocationRequired { get; set; } = false;

        public bool AllowManualCorrection { get; set; } = true;

        public int CorrectionRequestLimitDays { get; set; } = 7;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstWorkSchedule? WorkSchedule { get; set; }

        public MstGracePeriodPolicy? GracePeriodPolicy { get; set; }
    }
}
