using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxEmergencyStaffingRequest", Schema = "public")]
    public class TrxEmergencyStaffingRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CompetencyId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        public DateOnly RequirementDate { get; set; }
        public DateTime? RequiredStartAt { get; set; }
        public DateTime? RequiredEndAt { get; set; }
        public int RequiredHeadcount { get; set; } = 1;
        public int AssignedHeadcount { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "High";
        // Normal, High, Critical

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, PartiallyFilled, Fulfilled, Rejected, Cancelled

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? FulfilledAt { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxShiftAssignment? ShiftAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCompetency? Competency { get; set; }
        public MstShift? Shift { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
