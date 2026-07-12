using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDoctorSchedule", Schema = "public")]
    public class MstDoctorSchedule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ScheduleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        public DoctorScheduleType ScheduleType { get; set; } = DoctorScheduleType.WeeklyRecurring;

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid ClinicId { get; set; }

        public Guid? RoomId { get; set; }

        public DayOfWeek PracticeDay { get; set; } = DayOfWeek.Monday;

        public DateTime? PracticeDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        [MaxLength(100)]
        public string? SessionName { get; set; }

        [MaxLength(100)]
        public string? PracticeLocation { get; set; }


        public int MaxPatientQuota { get; set; } = 0;

        public int MaxAppointmentQuota { get; set; } = 0;

        public int MaxWalkInQuota { get; set; } = 0;

        public int EstimatedServiceMinutes { get; set; } = 15;

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowKioskRegistration { get; set; } = true;

        public bool IsTelemedicineAvailable { get; set; } = false;

        public bool IsSubstituteSchedule { get; set; } = false;

        public Guid? SubstituteDoctorId { get; set; }

        public DoctorScheduleStatus ScheduleStatus { get; set; } = DoctorScheduleStatus.Active;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDoctor? Doctor { get; set; }

        public MstDoctor? SubstituteDoctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstRoom? Room { get; set; }
    }
}
