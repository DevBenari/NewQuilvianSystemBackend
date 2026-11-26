using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;

public sealed class AssetMovementSummaryQuery
{
    public Guid? OrganizationUnitId { get; set; }
}

public sealed class AssetMovementSummaryResponse
{
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public bool CanReadAllUnits { get; set; }
    public int TransfersInFlight { get; set; }
    public List<AssetDashboardCardResponse> Transfers { get; set; } = [];
    public bool CanReadLoans { get; set; }
    public List<AssetDashboardCardResponse> Loans { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public sealed record AssetMovementInFlightResult(
    IReadOnlyList<string> TransferNumbers,
    IReadOnlyList<string> LoanNumbers)
{
    public bool HasAny => TransferNumbers.Count > 0 || LoanNumbers.Count > 0;
}
