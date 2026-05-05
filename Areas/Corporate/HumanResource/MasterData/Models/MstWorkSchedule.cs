using QuilvianSystemBackend.Enum;
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

        public Guid? UserId { get; set; }

        public UserType? UserType { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public TimeOnly WorkStartTime { get; set; }

        public TimeOnly WorkEndTime { get; set; }

        public int CheckInToleranceMinutes { get; set; } = 0;

        public int CheckOutToleranceMinutes { get; set; } = 0;

        public DateOnly? EffectiveStartDate { get; set; }

        public DateOnly? EffectiveEndDate { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }
    }
}
