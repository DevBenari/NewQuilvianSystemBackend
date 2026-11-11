using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public sealed class ManpowerPlanLineRequest
    {
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionId { get; set; }
        public Guid SalaryGradeId { get; set; }
        public decimal ProposedFormationQuantity { get; set; }
        public string? DeviationReason { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
    }

    public sealed class CreateAnnualPlanRequest
    {
        public Guid HospitalSiteId { get; set; }
        public int PlanYear { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public DateOnly ReferenceDate { get; set; }
        public string? Justification { get; set; }
        public List<ManpowerPlanLineRequest> Lines { get; set; } = new();
    }

    public sealed class CreatePlanAddendumRequest
    {
        public Guid ParentPlanId { get; set; }
        public DateOnly ReferenceDate { get; set; }
        public string? Justification { get; set; }
        public List<ManpowerPlanLineRequest> Lines { get; set; } = new();
    }

    public sealed class UpdateManpowerPlanRequest
    {
        public string? Justification { get; set; }
        public List<ManpowerPlanLineRequest> Lines { get; set; } = new();
    }

    public sealed class SubmitManpowerPlanRequest
    {
        public string? Note { get; set; }
    }

    public sealed class FinancePlanAdjustmentRequest
    {
        public decimal AdjustedTotal { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class ManpowerPlanLineResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public Guid SalaryGradeId { get; set; }
        public string? SalaryGradeName { get; set; }
        public decimal CurrentFormationQuantity { get; set; }
        public decimal? RecommendedQuantity { get; set; }
        public string? RecommendationBreakdown { get; set; }
        public decimal ProposedFormationQuantity { get; set; }
        public decimal ChangeQuantity { get; set; }
        public string? DeviationReason { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public decimal? MonthlyRateSnapshot { get; set; }
        public int MonthsInPeriod { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? IncrementalCost { get; set; }
        public CostCalculationStatus CostStatus { get; set; }
        public string CostStatusName => CostStatus.ToString();
        public bool CostVisible { get; set; } = true;
    }

    public sealed class ManpowerPlanListItemResponse
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public ManpowerPlanKind PlanKind { get; set; }
        public string PlanKindName => PlanKind.ToString();
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public int PlanYear { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public Guid? ParentPlanId { get; set; }
        public int? AddendumSequence { get; set; }
        public ManpowerPlanStatus PlanStatus { get; set; }
        public string PlanStatusName => PlanStatus.ToString();
        public int TotalLines { get; set; }
        public decimal? TotalEstimatedCost { get; set; }
        public decimal? FinanceAdjustedTotal { get; set; }
        public bool CostVisible { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class ManpowerPlanResponse
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public ManpowerPlanKind PlanKind { get; set; }
        public string PlanKindName => PlanKind.ToString();
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public int PlanYear { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public Guid? ParentPlanId { get; set; }
        public int? AddendumSequence { get; set; }
        public DateOnly ReferenceDate { get; set; }
        public ManpowerPlanStatus PlanStatus { get; set; }
        public string PlanStatusName => PlanStatus.ToString();
        public string? Justification { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public decimal? TotalEstimatedCost { get; set; }
        public decimal? FinanceAdjustedTotal { get; set; }
        public string? FinanceAdjustmentReason { get; set; }
        public DateTime? FinanceAdjustedAt { get; set; }
        public bool CostVisible { get; set; } = true;
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public List<ManpowerPlanLineResponse> Lines { get; set; } = new();
        public Guid RowVersion { get; set; }
    }

    public sealed class ManpowerPlanPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public int? PlanYear { get; set; }
        public ManpowerPlanKind? PlanKind { get; set; }
        public ManpowerPlanStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public sealed class ManpowerPlanFilterMetadataResponse
    {
        public List<string> Statuses { get; set; } = new();
        public List<string> PlanKinds { get; set; } = new();
    }
}
