using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>Pemakaian satu baris rencana (per <c>LineKey</c>).</summary>
/// <param name="UsedByPurchaseRequest">Σ kuantitas dalam rencana pada PR berstatus pemakan (PBJ-DEC-029).</param>
/// <param name="AllocatedToPackage">Σ alokasi pada rencana paket yang masih hidup (BE-PBJ-011).</param>
public sealed record PlanLineUsage(decimal UsedByPurchaseRequest, decimal AllocatedToPackage)
{
    public static PlanLineUsage None { get; } = new(0, 0);
}

/// <summary>Baris rencana berlaku yang disetujui, identitas lintas versi = <see cref="LineKey"/>.</summary>
public sealed record CurrentPlanLine(
    Guid LineKey,
    Guid NeedPlanId,
    string? PlanNumber,
    int VersionNo,
    Guid DepartmentId,
    string DepartmentName,
    int FiscalYear,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string MeasurementName,
    decimal Quantity,
    decimal EstimatedUnitPrice,
    Guid ItemCategoryId,
    string ItemCategoryName,
    Guid ProcurementTypeId,
    string ProcurementTypeCode,
    string ProcurementTypeName);

/// <summary>
/// Sisa dan pemakaian baris rencana kebutuhan (<c>02-backend-architecture.md</c> bagian 4.2).
/// </summary>
/// <remarks>
/// <para>
/// Pemakaian dihitung dari data saat itu, bukan disimpan, agar PR yang dibatalkan otomatis
/// mengembalikan sisa. Contoh: rencana IGD 10 monitor, PR-1 6 unit <c>APPROVED</c> → terpakai 6,
/// sisa 4; PR-1 dibatalkan → terpakai 0, sisa kembali 10.
/// </para>
/// <para>
/// Dipakai rencana kebutuhan (PBJ-VAL-042, BE-PBJ-009) dan pengajuan PR (pembagian dalam/luar
/// rencana, BE-PBJ-007).
/// </para>
/// </remarks>
public sealed class ProcurementPlanQuotaService
{
    private readonly ApplicationDbContext _dbContext;

    public ProcurementPlanQuotaService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Baris rencana berlaku (versi <c>APPROVED</c> dengan <c>IsCurrentVersion</c>) untuk kunci yang
    /// diminta. Kunci yang tidak menunjuk baris rencana berlaku tidak ikut dikembalikan.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, CurrentPlanLine>> GetCurrentLinesAsync(
        IReadOnlyCollection<Guid> lineKeys,
        CancellationToken cancellationToken)
    {
        if (lineKeys.Count == 0)
            return new Dictionary<Guid, CurrentPlanLine>();
        var keys = lineKeys.Distinct().ToList();
        var lines = await ProjectCurrent(CurrentLines().Where(x => keys.Contains(x.LineKey))).ToListAsync(cancellationToken);
        return lines.GroupBy(x => x.LineKey).ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>
    /// Baris rencana berlaku untuk satu tahun (seluruh unit), untuk konsolidasi (BE-PBJ-011).
    /// </summary>
    public async Task<List<CurrentPlanLine>> GetCurrentLinesForYearAsync(
        int fiscalYear, Guid? itemCategoryId, Guid? procurementTypeId, IReadOnlyCollection<Guid>? departmentIds,
        CancellationToken cancellationToken)
    {
        var query = CurrentLines().Where(x => x.NeedPlan!.FiscalYear == fiscalYear);
        if (itemCategoryId.HasValue) query = query.Where(x => x.Item!.ItemCategoryId == itemCategoryId.Value);
        if (procurementTypeId.HasValue) query = query.Where(x => x.Item!.ProcurementTypeId == procurementTypeId.Value);
        if (departmentIds != null)
        {
            var departments = departmentIds.ToList();
            query = query.Where(x => departments.Contains(x.NeedPlan!.DepartmentId));
        }
        return await ProjectCurrent(query).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Baris rencana berlaku untuk unit × tahun (opsional satu barang) beserta pemakaiannya, untuk
    /// pilihan baris rencana di form PR (<c>GET /plan-line-options</c>).
    /// </summary>
    public async Task<List<(CurrentPlanLine Line, PlanLineUsage Usage)>> GetOptionsAsync(
        Guid departmentId, int fiscalYear, Guid? itemId, CancellationToken cancellationToken)
    {
        var query = CurrentLines().Where(x => x.NeedPlan!.DepartmentId == departmentId && x.NeedPlan.FiscalYear == fiscalYear);
        if (itemId.HasValue) query = query.Where(x => x.ItemId == itemId.Value);
        var lines = await ProjectCurrent(query.OrderBy(x => x.Item!.ItemCode)).ToListAsync(cancellationToken);
        var usage = await GetUsageAsync(lines.Select(x => x.LineKey).ToList(), null, cancellationToken);
        return lines.Select(x => (x, usage.GetValueOrDefault(x.LineKey) ?? PlanLineUsage.None)).ToList();
    }

    /// <summary>
    /// Mengunci baris rencana dengan <c>pg_advisory_xact_lock</c> per <c>LineKey</c> sampai transaksi
    /// pemanggil selesai (<c>02-backend-architecture.md</c> bagian 1). Wajib dipanggil di dalam transaksi.
    /// Kunci diurutkan agar dua pengajuan yang memakai kunci sama tidak saling mengunci.
    /// </summary>
    /// <remarks>
    /// Contoh: dua PR masing-masing 4 unit diajukan hampir bersamaan pada sisa rencana 4. PR kedua menunggu
    /// kunci, lalu membaca sisa 0, sehingga total dalam rencana kedua PR tetap 4 (AT-PP-07).
    /// </remarks>
    public async Task LockAsync(IReadOnlyCollection<Guid> lineKeys, CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Penguncian baris rencana wajib di dalam transaksi.");
        foreach (var key in lineKeys.Distinct().OrderBy(x => x))
        {
            var lockName = "PBJ_PLAN_LINE:" + key.ToString("N");
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockName}, 0))", cancellationToken);
        }
    }

    private IQueryable<PrcNeedPlanLine> CurrentLines() =>
        _dbContext.PrcNeedPlanLines.AsNoTracking()
            .Where(x =>
                !x.IsDelete &&
                !x.NeedPlan!.IsDelete &&
                x.NeedPlan.IsCurrentVersion &&
                x.NeedPlan.Status == ProcurementStatuses.Plan.Approved);

    private static IQueryable<CurrentPlanLine> ProjectCurrent(IQueryable<PrcNeedPlanLine> query) =>
        query.Select(x => new CurrentPlanLine(
                x.LineKey,
                x.NeedPlanId,
                x.NeedPlan!.PlanNumber,
                x.NeedPlan.VersionNo,
                x.NeedPlan.DepartmentId,
                x.NeedPlan.Department!.DepartmentName,
                x.NeedPlan.FiscalYear,
                x.ItemId,
                x.Item!.ItemCode,
                x.Item.ItemName,
                x.Item.Measurement!.MeasurementName,
                x.Quantity,
                x.EstimatedUnitPrice,
                x.Item.ItemCategoryId,
                x.Item.ItemCategory!.CategoryName,
                x.Item.ProcurementTypeId,
                x.Item.ProcurementType!.TypeCode,
                x.Item.ProcurementType.TypeName));

    /// <summary>
    /// Pemakaian per <c>LineKey</c>. <paramref name="excludePurchaseRequestId"/> mengecualikan satu PR,
    /// dipakai saat PR itu sendiri sedang dihitung ulang pembagiannya; <paramref name="excludePackagePlanId"/>
    /// mengecualikan satu rencana paket saat alokasinya sendiri sedang diperiksa (PBJ-VAL-050).
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, PlanLineUsage>> GetUsageAsync(
        IReadOnlyCollection<Guid> lineKeys,
        Guid? excludePurchaseRequestId,
        CancellationToken cancellationToken,
        Guid? excludePackagePlanId = null)
    {
        if (lineKeys.Count == 0)
            return new Dictionary<Guid, PlanLineUsage>();

        var keys = lineKeys.Distinct().ToList();
        var consuming = ProcurementStatuses.PurchaseRequest.ConsumesPlanQuota.ToList();

        var used = await _dbContext.PrcPurchaseRequestLines
            .AsNoTracking()
            .Where(x =>
                x.NeedPlanLineKey.HasValue &&
                keys.Contains(x.NeedPlanLineKey.Value) &&
                !x.IsDelete &&
                !x.PurchaseRequest!.IsDelete &&
                consuming.Contains(x.PurchaseRequest.Status) &&
                (!excludePurchaseRequestId.HasValue || x.PurchaseRequestId != excludePurchaseRequestId.Value))
            .GroupBy(x => x.NeedPlanLineKey!.Value)
            .Select(g => new { Key = g.Key, Quantity = g.Sum(x => x.InPlanQuantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Quantity, cancellationToken);

        // BE-PBJ-011: alokasi rencana paket yang masih hidup (bukan CANCELLED/REJECTED).
        var allocated = await _dbContext.PrcPackagePlanLines
            .AsNoTracking()
            .Where(x =>
                keys.Contains(x.NeedPlanLineKey) &&
                !x.IsDelete &&
                !x.PackagePlan!.IsDelete &&
                x.PackagePlan.Status != ProcurementStatuses.Plan.Cancelled &&
                x.PackagePlan.Status != ProcurementStatuses.Plan.Rejected &&
                (!excludePackagePlanId.HasValue || x.PackagePlanId != excludePackagePlanId.Value))
            .GroupBy(x => x.NeedPlanLineKey)
            .Select(g => new { Key = g.Key, Quantity = g.Sum(x => x.AllocatedQuantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Quantity, cancellationToken);

        return keys.ToDictionary(
            key => key,
            key => new PlanLineUsage(used.GetValueOrDefault(key), allocated.GetValueOrDefault(key)));
    }
}
