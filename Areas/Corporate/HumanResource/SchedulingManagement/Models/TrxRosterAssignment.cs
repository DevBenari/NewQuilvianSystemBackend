using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxRosterAssignment", Schema = "public")]
    public class TrxRosterAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RosterPeriodId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? ShiftGroupId { get; set; }

        [Required]
        [MaxLength(30)]
        public string AssignmentStatus { get; set; } = "Draft";
        // Draft, Validated, Approved, Published, Cancelled

        public int PlannedShiftCount { get; set; } = 0;
        public int PlannedWorkMinutes { get; set; } = 0;
        public int PlannedNightShiftCount { get; set; } = 0;
        public int PlannedOnCallCount { get; set; } = 0;
        public int PlannedDayOffCount { get; set; } = 0;

        public bool HasConflict { get; set; } = false;
        public int ConflictCount { get; set; } = 0;
        public bool IsValidationPassed { get; set; } = false;
        public DateTime? ValidatedAt { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxRosterPeriod? RosterPeriod { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstShiftGroup? ShiftGroup { get; set; }

        public ICollection<TrxShiftAssignment> ShiftAssignments { get; set; } = new List<TrxShiftAssignment>();
        public ICollection<TrxOnCallAssignment> OnCallAssignments { get; set; } = new List<TrxOnCallAssignment>();
    }
}
