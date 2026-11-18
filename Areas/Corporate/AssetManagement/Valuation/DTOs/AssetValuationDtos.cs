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
