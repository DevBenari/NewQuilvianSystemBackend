using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxAnnualManpowerPlan", Schema = "public")]
    public class TrxAnnualManpowerPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PlanNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PlanName { get; set; } = string.Empty;

        public int PlanYear { get; set; }

        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        [MaxLength(30)]
        public string PlanStatus { get; set; } = "Draft";
        // Draft, Submitted, UnderReview, Approved, Rejected, Closed, Cancelled.

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal TotalCurrentHeadcount { get; set; } = 0m;
        public decimal TotalTargetHeadcount { get; set; } = 0m;
        public decimal TotalRequestedHeadcount { get; set; } = 0m;
        public decimal? TotalEstimatedAnnualCost { get; set; }
        public decimal? ApprovedBudgetAmount { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ClosedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxManpowerPlanDetail> Details { get; set; }
            = new List<TrxManpowerPlanDetail>();
    }
}
