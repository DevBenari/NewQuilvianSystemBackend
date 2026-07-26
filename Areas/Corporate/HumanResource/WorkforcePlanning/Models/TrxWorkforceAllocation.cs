using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxWorkforceAllocation", Schema = "public")]
    public class TrxWorkforceAllocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? DailyStaffingRequirementId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? ShiftId { get; set; }

        public DateTime AllocationDate { get; set; }
        public DateTime? AllocationStartDateTime { get; set; }
        public DateTime? AllocationEndDateTime { get; set; }

        [MaxLength(30)]
        public string AllocationType { get; set; } = "Regular";
        // Regular, Temporary, Floating, Emergency, Replacement, Overtime.

        [MaxLength(30)]
        public string AllocationSource { get; set; } = "Manual";
        // Manual, Roster, StaffingEngine, ManagerAssignment, Integration.

        [MaxLength(100)]
        public string? AllocationRole { get; set; }

        [MaxLength(30)]
        public string AllocationStatus { get; set; } = "Planned";
        // Planned, Confirmed, Published, InProgress, Completed, Cancelled.

        public bool IsPrimaryAllocation { get; set; } = false;
        public bool IsOvertimeAllocation { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public TrxDailyStaffingRequirement? DailyStaffingRequirement { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstShift? Shift { get; set; }
    }
}
