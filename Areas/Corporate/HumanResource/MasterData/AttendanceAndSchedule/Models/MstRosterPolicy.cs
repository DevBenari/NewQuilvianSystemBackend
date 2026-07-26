using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstRosterPolicy", Schema = "public")]
    public class MstRosterPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? HospitalSiteId { get; set; }

        public Guid? ShiftGroupId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RosterPolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RosterPolicyName { get; set; } = string.Empty;

        public int MinimumStaffPerShift { get; set; } = 0;

        public int? MaximumStaffPerShift { get; set; }

        public int MinimumWeeklyHours { get; set; } = 0;

        public int MaximumWeeklyHours { get; set; } = 40;

        public int MaximumConsecutiveWorkDays { get; set; } = 6;

        public int MaximumConsecutiveNightShifts { get; set; } = 3;

        public int MinimumDaysOffPerMonth { get; set; } = 4;

        public int PublishLeadDays { get; set; } = 7;

        public int LockLeadDays { get; set; } = 1;

        public bool RequireApproval { get; set; } = true;

        public bool RequireSkillMixValidation { get; set; } = true;

        public bool AllowShiftSwap { get; set; } = true;

        public bool AllowEmergencyOverride { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstHospitalSite? HospitalSite { get; set; }

        public MstShiftGroup? ShiftGroup { get; set; }
    }
}
