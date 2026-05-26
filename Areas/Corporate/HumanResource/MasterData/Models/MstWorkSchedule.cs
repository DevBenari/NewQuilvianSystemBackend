using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstWorkSchedule", Schema = "public")]
    public class MstWorkSchedule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ScheduleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ScheduleType { get; set; } = "Shift";
        // Shift, NonShift, OnCall, Off

        public TimeOnly WorkStartTime { get; set; }

        public TimeOnly WorkEndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        public int CheckInToleranceMinutes { get; set; } = 0;

        public int CheckOutToleranceMinutes { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
