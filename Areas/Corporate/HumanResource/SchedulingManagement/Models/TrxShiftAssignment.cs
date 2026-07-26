using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxShiftAssignment", Schema = "public")]
    public class TrxShiftAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RosterAssignmentId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? DailyStaffingRequirementId { get; set; }
        public Guid? ShiftSkillRequirementId { get; set; }
        public Guid? ScheduleChangeRequestId { get; set; }
        public Guid? ShiftSwapRequestId { get; set; }

        public DateOnly ShiftDate { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime ScheduledEndAt { get; set; }
        public int BreakDurationMinutes { get; set; } = 0;
        public int PlannedWorkMinutes { get; set; } = 0;

        [Required]
        [MaxLength(30)]
        public string AssignmentType { get; set; } = "Regular";
        // Regular, Overtime, OnCall, Training, Remote, BusinessTrip, DayOff

        [Required]
        [MaxLength(30)]
        public string AssignmentStatus { get; set; } = "Draft";
        // Draft, Validated, Published, Confirmed, Completed, Cancelled, Replaced

        [MaxLength(30)]
        public string AssignmentSource { get; set; } = "Roster";
        // Roster, Manual, ScheduleChange, ShiftSwap, Emergency, Import

        public bool IsNightShift { get; set; } = false;
        public bool IsOnCall { get; set; } = false;
        public bool IsDayOff { get; set; } = false;
        public bool IsManualOverride { get; set; } = false;

        public bool HasDoubleShiftConflict { get; set; } = false;
        public bool HasLeaveConflict { get; set; } = false;
        public bool HasTrainingConflict { get; set; } = false;
        public bool HasMinimumRestConflict { get; set; } = false;
        public bool HasWorkHourLimitConflict { get; set; } = false;
        public bool HasLicenseConflict { get; set; } = false;
        public bool HasClinicalPrivilegeConflict { get; set; } = false;
        public bool HasMinimumStaffingConflict { get; set; } = false;
        public bool HasSkillMixConflict { get; set; } = false;
        public bool HasBlockingConflict { get; set; } = false;
        public bool IsValidationPassed { get; set; } = false;

        public DateTime? ValidatedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [MaxLength(1000)]
        public string? OverrideReason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxRosterAssignment? RosterAssignment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public TrxDailyStaffingRequirement? DailyStaffingRequirement { get; set; }
        public MstShiftSkillRequirement? ShiftSkillRequirement { get; set; }
        public WfpScheduleChangeRequest? ScheduleChangeRequest { get; set; }
        public WfpShiftSwapRequest? ShiftSwapRequest { get; set; }
        public ApplicationUser? ValidatedByUser { get; set; }
    }
}
