using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdContractDecision", Schema = "public")]
    public class HrdContractDecision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string DecisionNumber { get; set; } = string.Empty;

        [Required]
        public Guid ContractHistoryId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public ContractDecisionType DecisionType { get; set; }

        public int? ExtensionMonths { get; set; }

        [Required]
        public DateOnly ProposedStartDate { get; set; }

        public DateOnly? ProposedEndDate { get; set; }

        [MaxLength(2000)]
        public string? ReasonText { get; set; }

        public ContractDecisionStatus DecisionStatus { get; set; } = ContractDecisionStatus.Submitted;

        [Required]
        public Guid SubmittedByUserId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        public Guid? FollowUpContractHistoryId { get; set; }

        public DateOnly? EmployeeNotifiedOn { get; set; }

        public Guid? NotificationRecordedByUserId { get; set; }

        public Guid? EmployeeSeparationId { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public WfpContractHistory? ContractHistory { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? DecidedByUser { get; set; }
        public WfpContractHistory? FollowUpContractHistory { get; set; }
        public ApplicationUser? NotificationRecordedByUser { get; set; }
    }
}
