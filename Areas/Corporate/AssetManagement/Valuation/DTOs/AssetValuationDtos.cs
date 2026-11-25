namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;

public sealed class ValuationPagedQuery
{
    public string? Search { get; set; }
    public Guid? AssetGroupId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ValuationParameterSetRequest
{
    public DateOnly? EffectiveFromPeriod { get; set; }
    public decimal CapitalizationThreshold { get; set; }
    public decimal DisposalDirectorThreshold { get; set; }
    public decimal UtilizationDirectorThreshold { get; set; }
    public string? Note { get; set; }
}

public sealed class ValuationParameterResponse
{
    public Guid Id { get; set; }
    public Guid LegalEntityId { get; set; }
    public DateOnly EffectiveFromPeriod { get; set; }
    public decimal CapitalizationThreshold { get; set; }
    public decimal DisposalDirectorThreshold { get; set; }
    public decimal UtilizationDirectorThreshold { get; set; }
    public string? Note { get; set; }
    public Guid SetBy { get; set; }
    public DateTime SetAt { get; set; }
}

public sealed class ValuationParameterCurrentResponse
{
    public Guid LegalEntityId { get; set; }
    public string LegalEntityName { get; set; } = string.Empty;
    public DateOnly CurrentPeriod { get; set; }
    public DateOnly NextAllowedPeriod { get; set; }
    public ValuationParameterResponse? Current { get; set; }
    public ValuationParameterResponse? Upcoming { get; set; }
}

public sealed class DepreciationPolicySetRequest
{
    public Guid? AssetGroupId { get; set; }
    public DateOnly? EffectiveFromPeriod { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal ResidualValuePercent { get; set; }
}

public sealed class DepreciationPolicyResponse
{
    public Guid Id { get; set; }
    public Guid AssetGroupId { get; set; }
    public DateOnly EffectiveFromPeriod { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal ResidualValuePercent { get; set; }
    public Guid SetBy { get; set; }
    public DateTime SetAt { get; set; }
}

public sealed class DepreciationPolicyGroupResponse
{
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DepreciationPolicyResponse? Current { get; set; }
    public DepreciationPolicyResponse? Upcoming { get; set; }
}

public sealed class SubledgerAccountSetupRequest
{
    public string? CostControlAccountCode { get; set; }
    public string? AccumulatedDepreciationControlAccountCode { get; set; }
    public string? AccumulatedImpairmentControlAccountCode { get; set; }
}

public sealed class SubledgerAccountRefResponse
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsKnown { get; set; }
    public bool IsControlAccount { get; set; }
}

public sealed class SubledgerAccountSetupResponse
{
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsDepreciable { get; set; }
    public bool IsActive { get; set; }
    public bool IsConfigured { get; set; }
    public SubledgerAccountRefResponse? CostControlAccount { get; set; }
    public SubledgerAccountRefResponse? AccumulatedDepreciationControlAccount { get; set; }
    public SubledgerAccountRefResponse? AccumulatedImpairmentControlAccount { get; set; }
    public List<string> Warnings { get; set; } = [];
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class UnitCostCenterQuery
{
    public string? Search { get; set; }
    public bool? UnclearOnly { get; set; }
}

public sealed class UnitCostCenterRequest
{
    public Guid? CostCenterId { get; set; }
}

public sealed class UnitCostCenterResponse
{
    public Guid OrganizationUnitId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public bool IsMapped { get; set; }
    public bool IsUnclear { get; set; }
    public int ActiveCostCenterCount { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public string? UnclearReason { get; set; }
}

public sealed class CostCenterOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? OrganizationUnitId { get; set; }
    public string? UnitName { get; set; }
}
