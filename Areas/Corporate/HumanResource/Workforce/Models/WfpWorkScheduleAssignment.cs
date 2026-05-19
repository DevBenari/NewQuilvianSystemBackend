using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpWorkScheduleAssignment", Schema = "public")]
    public class WfpWorkScheduleAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid WorkScheduleId { get; set; }

        [Required]
        public DateOnly ScheduleDate { get; set; }

        public bool IsOffDay { get; set; } = false;

        public bool IsOvertimePlanned { get; set; } = false;

        public bool IsOnCall { get; set; } = false;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public MstWorkSchedule? WorkSchedule { get; set; }
    }
}