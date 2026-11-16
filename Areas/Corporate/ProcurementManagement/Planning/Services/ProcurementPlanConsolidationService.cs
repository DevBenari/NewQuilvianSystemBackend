using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>
/// Konsolidasi kebutuhan unit: gabungan baris rencana berlaku per barang, beserta unit asal dan sisa
/// yang belum dialokasikan ke rencana paket (BE-PBJ-011; layar 2.2). Hanya baca.
/// </summary>
/// <remarks>
/// Contoh: rencana 2027 IGD 3, Rawat Inap 5, dan Poli 22 kursi roda → satu baris <c>KTL-000045</c> total
/// 30 dengan tiga unit asal. Tim Pengadaan yang memegang <c>ProcurementPackagePlan : ReadAllUnits</c>
/// melihat seluruh unit; pengguna lain hanya unit penempatannya.
/// </remarks>
public sealed class ProcurementPlanConsolidationService
{
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementPlanQuotaService _quotaService;

    public ProcurementPlanConsolidationService(
        ProcurementDataScopeService scopeService,
        ProcurementPlanQuotaService quotaService)
    {
        _scopeService = scopeService;
        _quotaService = quotaService;
    }

    public async Task<List<PlanConsolidationRow>> GetAsync(ClaimsPrincipal user, PlanConsolidationQuery request, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PackagePlanResource, cancellationToken);
        if (scope.IsEmpty) return new List<PlanConsolidationRow>();

        var lines = await _quotaService.GetCurrentLinesForYearAsync(
            request.FiscalYear, request.ItemCategoryId, request.ProcurementTypeId,
            scope.IsAllUnits ? null : scope.DepartmentIds.ToList(), cancellationToken);
        var usage = await _quotaService.GetUsageAsync(lines.Select(x => x.LineKey).ToList(), null, cancellationToken);

        return lines
            .GroupBy(x => x.ItemId)
            .Select(g =>
            {
                var first = g.First();
                var units = g.Select(x =>
                {
                    var allocated = (usage.GetValueOrDefault(x.LineKey) ?? PlanLineUsage.None).AllocatedToPackage;
                    return new PlanConsolidationUnit
                    {
                        LineKey = x.LineKey,
                        NeedPlanId = x.NeedPlanId,
                        PlanNumber = x.PlanNumber,
                        VersionNo = x.VersionNo,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.DepartmentName,
                        Quantity = x.Quantity,
                        EstimatedUnitPrice = x.EstimatedUnitPrice,
                        AllocatedQuantity = allocated,
                        RemainingQuantity = Math.Max(0, x.Quantity - allocated)
                    };
                }).OrderBy(x => x.DepartmentName).ToList();

                return new PlanConsolidationRow
                {
                    ItemId = first.ItemId,
                    ItemCode = first.ItemCode,
                    ItemName = first.ItemName,
                    ProcurementTypeId = first.ProcurementTypeId,
                    ProcurementTypeCode = first.ProcurementTypeCode,
                    ProcurementTypeName = first.ProcurementTypeName,
                    ItemCategoryId = first.ItemCategoryId,
                    ItemCategoryName = first.ItemCategoryName,
                    MeasurementName = first.MeasurementName,
                    TotalQuantity = units.Sum(x => x.Quantity),
                    AllocatedQuantity = units.Sum(x => x.AllocatedQuantity),
                    RemainingQuantity = units.Sum(x => x.RemainingQuantity),
                    EstimatedValue = Math.Round(units.Sum(x => x.Quantity * x.EstimatedUnitPrice), 2, MidpointRounding.AwayFromZero),
                    Units = units
                };
            })
            .OrderBy(x => x.ItemCode)
            .ToList();
    }
}
