using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("HrdPersonnelActionOrder", Schema = "public")]
    public class HrdPersonnelActionOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(60)]
        public string ActionOrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(40)]
        public string ActionCategory { get; set; } = "Mutation"; // Mutation, Promotion, Rotation, Demotion, TemporaryAssignment

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime PostingDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(30)]
        public string OrderStatus { get; set; } = "Draft"; // Draft, PendingApproval, Scheduled, Applied, Rejected, Cancelled

        [Required]
        [MaxLength(1500)]
        public string ReasonText { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? SignedDocumentPath { get; set; }

        [MaxLength(255)]
        public string? SignedDocumentFileName { get; set; }

        public Guid? TargetOrganizationUnitId { get; set; }
        public Guid? TargetDepartmentId { get; set; }
        public Guid? TargetPositionId { get; set; }
        public Guid? TargetManagerProfileId { get; set; }
        public Guid? LinkedSalaryAssignmentId { get; set; }
        public decimal? TargetBaseSalary { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public Guid? CancellationRequestedByUserId { get; set; }
        public DateTime? CancellationRequestedAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [MaxLength(60)]
        public string? CounterOrderNumber { get; set; }

        public DateTime? ExecutedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstOrganizationUnit? TargetOrganizationUnit { get; set; }
        public MstDepartment? TargetDepartment { get; set; }
        public MstPosition? TargetPosition { get; set; }
        public MstWorkforceProfile? TargetManagerProfile { get; set; }
        public WfpSalaryAssignment? LinkedSalaryAssignment { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public TrxWorkflowInstance? WorkflowInstance { get; set; }

        public ICollection<HrdPersonnelActionOrderAttachment> Attachments { get; set; } = new List<HrdPersonnelActionOrderAttachment>();
    }
}
