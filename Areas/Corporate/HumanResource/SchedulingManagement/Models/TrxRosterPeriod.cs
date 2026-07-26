using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxRosterPeriod", Schema = "public")]
    public class TrxRosterPeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RosterPeriodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RosterPeriodName { get; set; } = string.Empty;

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RosterPolicyId { get; set; }
        public Guid? MinimumRestPolicyId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public DateTime? SubmissionDeadlineAt { get; set; }
        public DateTime? PublicationPlannedAt { get; set; }

        [Required]
        [MaxLength(30)]
        public string RosterStatus { get; set; } = "Draft";
        // Draft, Validation, Submitted, Approved, Published, Locked, Closed, Cancelled

        public int VersionNumber { get; set; } = 1;
        public int TotalAssignmentCount { get; set; } = 0;
        public int TotalConflictCount { get; set; } = 0;
        public int BlockingConflictCount { get; set; } = 0;
        public bool IsValidationPassed { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? ValidatedAt { get; set; }
        public Guid? ValidatedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByUserId { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ValidationSummaryJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstRosterPolicy? RosterPolicy { get; set; }
        public MstMinimumRestPolicy? MinimumRestPolicy { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ValidatedByUser { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
        public ApplicationUser? LockedByUser { get; set; }

        public ICollection<TrxRosterAssignment> RosterAssignments { get; set; } = new List<TrxRosterAssignment>();
        public ICollection<TrxRosterApproval> RosterApprovals { get; set; } = new List<TrxRosterApproval>();
        public ICollection<TrxRosterPublication> Publications { get; set; } = new List<TrxRosterPublication>();
    }
}
