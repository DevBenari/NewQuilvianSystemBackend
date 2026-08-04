using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimePlanDetail", Schema = "public")]
    public class TrxOvertimePlanDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimePlanId { get; set; }

        public int SequenceNumber { get; set; } = 1;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? OvertimePolicyId { get; set; }

        public DateOnly OvertimeDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }

        public int PlannedMinutes { get; set; } = 0;
        public int EstimatedBreakMinutes { get; set; } = 0;

        [Required, MaxLength(30)]
        public string DayType { get; set; } = "Workday";

        [Required, MaxLength(40)]
        public string OvertimeCategory { get; set; } = "AfterShift";

        [Required, MaxLength(2000)]
        public string WorkDescription { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool HasScheduleConflict { get; set; } = false;
        public bool HasLeaveConflict { get; set; } = false;
        public bool HasTrainingConflict { get; set; } = false;
        public bool HasMinimumRestConflict { get; set; } = false;
        public bool HasWorkHourLimitConflict { get; set; } = false;
        public bool IsPolicyCompliant { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string DetailStatus { get; set; } = "Draft";

        public bool IsActive { get; set; } = true;

        public TrxOvertimePlan? OvertimePlan { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public MstOvertimePolicy? OvertimePolicy { get; set; }
        public WfpOvertimeRequest? GeneratedOvertimeRequest { get; set; }
    }
}
