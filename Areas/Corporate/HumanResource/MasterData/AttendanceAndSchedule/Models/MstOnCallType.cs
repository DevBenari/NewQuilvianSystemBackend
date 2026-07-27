using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstOnCallType", Schema = "public")]
    public class MstOnCallType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string OnCallTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OnCallTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int ResponseTimeMinutes { get; set; } = 0;

        public int MinimumCallHours { get; set; } = 0;

        public int MaximumCallHours { get; set; } = 24;

        public bool IsRemoteAllowed { get; set; } = false;

        public bool RequiresOnSitePresence { get; set; } = true;

        public bool CountsAsWorkingTime { get; set; } = true;

        public bool IsAllowanceEligible { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public ICollection<MstShift> Shifts { get; set; }
            = new List<MstShift>();
    }
}
