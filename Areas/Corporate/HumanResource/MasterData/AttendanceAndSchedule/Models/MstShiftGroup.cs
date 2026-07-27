using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstShiftGroup", Schema = "public")]
    public class MstShiftGroup : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ShiftGroupCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ShiftGroupName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsRotating { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public ICollection<MstShift> Shifts { get; set; }
            = new List<MstShift>();

        public ICollection<MstShiftPattern> ShiftPatterns { get; set; }
            = new List<MstShiftPattern>();

        public ICollection<MstRosterPolicy> RosterPolicies { get; set; }
            = new List<MstRosterPolicy>();

        public ICollection<MstMinimumRestPolicy> MinimumRestPolicies { get; set; }
            = new List<MstMinimumRestPolicy>();
    }
}
