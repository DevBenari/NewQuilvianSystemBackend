using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstGracePeriodPolicy", Schema = "public")]
    public class MstGracePeriodPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string GracePeriodPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string GracePeriodPolicyName { get; set; } = string.Empty;

        public int EarlyCheckInMinutes { get; set; } = 0;

        public int LateCheckInGraceMinutes { get; set; } = 0;

        public int EarlyCheckOutGraceMinutes { get; set; } = 0;

        public int LateCheckOutMinutes { get; set; } = 0;

        public int? MaximumLateOccurrencesPerMonth { get; set; }

        public bool CountLateAfterGrace { get; set; } = true;

        public bool CountEarlyLeaveAfterGrace { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public ICollection<MstAttendancePolicy> AttendancePolicies { get; set; }
            = new List<MstAttendancePolicy>();
    }
}
