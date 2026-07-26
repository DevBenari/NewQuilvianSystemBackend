using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("WfpWorkScheduleAssignment", Schema = "public")]
    public class WfpWorkScheduleAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }

        [Required]
        public Guid WorkScheduleId { get; set; }

        public Guid? ShiftGroupId { get; set; }
        public Guid? ShiftPatternId { get; set; }
        public Guid? RosterPolicyId { get; set; }
        public Guid? MinimumRestPolicyId { get; set; }

        [Required]
        [MaxLength(30)]
        public string AssignmentType { get; set; } = "Primary";
        // Primary, Temporary, Rotation, Project, OnCall

        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }

        public int WeekStartDay { get; set; } = 1;

        public bool IsPrimary { get; set; } = true;
        public bool IsRotating { get; set; } = false;
        public bool IsTemporary { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShiftGroup? ShiftGroup { get; set; }
        public MstShiftPattern? ShiftPattern { get; set; }
        public MstRosterPolicy? RosterPolicy { get; set; }
        public MstMinimumRestPolicy? MinimumRestPolicy { get; set; }
    }
}
