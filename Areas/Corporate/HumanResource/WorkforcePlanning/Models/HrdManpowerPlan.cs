using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdManpowerPlan", Schema = "public")]
    public class HrdManpowerPlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PlanNumber { get; set; } = string.Empty;

        public ManpowerPlanKind PlanKind { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        public int PlanYear { get; set; }

        public DateOnly PeriodStartDate { get; set; }

        public DateOnly PeriodEndDate { get; set; }

        public Guid? ParentPlanId { get; set; }

        public int? AddendumSequence { get; set; }

        public DateOnly ReferenceDate { get; set; }

        public ManpowerPlanStatus PlanStatus { get; set; } = ManpowerPlanStatus.Draft;

        [MaxLength(2000)]
        public string? Justification { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal? TotalEstimatedCost { get; set; }

        public decimal? FinanceAdjustedTotal { get; set; }

        [MaxLength(1000)]
        public string? FinanceAdjustmentReason { get; set; }

        public Guid? FinanceAdjustedByUserId { get; set; }

        public DateTime? FinanceAdjustedAt { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
        public HrdManpowerPlan? ParentPlan { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public ICollection<HrdManpowerPlanLine> Lines { get; set; } = new List<HrdManpowerPlanLine>();
    }

    [Table("HrdManpowerPlanLine", Schema = "public")]
    public class HrdManpowerPlanLine : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        public decimal CurrentFormationQuantity { get; set; } = 0m;

        public decimal? RecommendedQuantity { get; set; }

        [MaxLength(1000)]
        public string? RecommendationBreakdown { get; set; }

        public decimal ProposedFormationQuantity { get; set; }

        public decimal ChangeQuantity { get; set; } = 0m;

        [MaxLength(1000)]
        public string? DeviationReason { get; set; }

        public DateOnly EffectiveStartDate { get; set; }

        public decimal? MonthlyRateSnapshot { get; set; }

        public int MonthsInPeriod { get; set; }

        public decimal? EstimatedCost { get; set; }

        public decimal? IncrementalCost { get; set; }

        public CostCalculationStatus CostStatus { get; set; }

        public Guid? CreatedFormationId { get; set; }

        public HrdManpowerPlan? Plan { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public HrdPositionFormation? CreatedFormation { get; set; }
    }
}
