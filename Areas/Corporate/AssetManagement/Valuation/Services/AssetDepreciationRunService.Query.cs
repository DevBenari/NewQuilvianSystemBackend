using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;

public sealed partial class AssetDepreciationRunService
{
    private const int MaxPageSize = 100;

    public static string AdjustmentReasonName(DepreciationAdjustmentReason reason) => reason switch
    {
        DepreciationAdjustmentReason.KoreksiHarga => "Koreksi Harga",
        DepreciationAdjustmentReason.TerlambatDicatat => "Terlambat Dicatat",
        DepreciationAdjustmentReason.Pemulihan => "Pemulihan",
        _ => "Normal"
    };

    public async Task<PagedResult<DepreciationRunResponse>> GetRunsAsync(DepreciationRunPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = _context.AstDepreciationRuns.AsNoTracking().Where(x => x.LegalEntityId == entity.LegalEntityId && !x.IsDelete);
        if (request.Year is { } year && year is >= 2000 and <= 2100)
        {
            var from = new DateOnly(year, 1, 1);
            var to = from.AddYears(1);
            query = query.Where(x => x.Period >= from && x.Period < to);
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Period)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DepreciationRunResponse
            {
                Id = x.Id,
                LegalEntityId = x.LegalEntityId,
                Period = x.Period,
                AssetCount = x.AssetCount,
                TotalAmount = x.TotalAmount,
                AdjustmentAmount = x.AdjustmentAmount,
                ProcessedBy = x.ProcessedBy,
                ProcessedAt = x.ProcessedAt
            })
            .ToListAsync(cancellationToken);

        await DescribeRunsAsync(items, false, cancellationToken);
        return new PagedResult<DepreciationRunResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<DepreciationRunResponse> GetRunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var run = await _context.AstDepreciationRuns
            .AsNoTracking()
            .Where(x => x.Id == id && x.LegalEntityId == entity.LegalEntityId && !x.IsDelete)
            .Select(x => new DepreciationRunResponse
            {
                Id = x.Id,
                LegalEntityId = x.LegalEntityId,
                Period = x.Period,
                AssetCount = x.AssetCount,
                TotalAmount = x.TotalAmount,
                AdjustmentAmount = x.AdjustmentAmount,
                ProcessedBy = x.ProcessedBy,
                ProcessedAt = x.ProcessedAt
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetValuationErrorCodes.NotFound, AssetValuationMessages.RunNotFound);

        await DescribeRunsAsync([run], true, cancellationToken);
        return run;
    }

    public async Task<PagedResult<DepreciationRunLineResponse>> GetLinesAsync(Guid id, DepreciationRunLineQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await GetRunAsync(id, cancellationToken);

        var query = _context.AstDepreciationRunLines.AsNoTracking().Where(x => x.RunId == id && !x.IsDelete);
        if (request.AssetGroupId is { } groupId && groupId != Guid.Empty)
        {
            query = query.Where(x => x.AssetGroupId == groupId);
        }

        if (request.CostCenterId is { } costCenterId && costCenterId != Guid.Empty)
        {
            query = query.Where(x => x.CostCenterId == costCenterId);
        }

        if (request.AdjustmentOnly == true)
        {
            query = query.Where(x => x.AdjustmentReason != DepreciationAdjustmentReason.Tidak);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x => (x.Asset!.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                                     x.Asset!.AssetName.ToUpper().Contains(keyword));
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.AssetGroup!.InventoryCode)
            .ThenBy(x => x.Asset!.InventoryNumber)
            .ThenBy(x => x.AdjustmentReason)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DepreciationRunLineResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                AssetGroupId = x.AssetGroupId,
                InventoryCode = x.AssetGroup!.InventoryCode,
                OrganizationUnitId = x.OrganizationUnitId,
                OrganizationUnitName = x.OrganizationUnit!.UnitName,
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenter!.CostCenterCode,
                CostCenterName = x.CostCenter.CostCenterName,
                Amount = x.Amount,
                AdjustmentReason = x.AdjustmentReason,
                AdjustmentPeriod = x.AdjustmentPeriod
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.AdjustmentReasonName = AdjustmentReasonName(item.AdjustmentReason);
        }

        return new PagedResult<DepreciationRunLineResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private async Task DescribeRunsAsync(IReadOnlyCollection<DepreciationRunResponse> runs, bool withDetail, CancellationToken cancellationToken)
    {
        if (runs.Count == 0)
        {
            return;
        }

        var userIds = runs.Select(x => x.ProcessedBy).Distinct().ToList();
        var names = await _context.Users.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        var runIds = runs.Select(x => x.Id).ToList();
        var outbox = await _context.AstAccountingOutboxes.AsNoTracking()
            .Where(x => runIds.Contains(x.SourceDocumentId) && x.SourceDocumentType == RunDocumentType && !x.IsDelete)
            .GroupBy(x => new { x.SourceDocumentId, x.Status })
            .Select(x => new { x.Key.SourceDocumentId, x.Key.Status, Count = x.Count() })
            .ToListAsync(cancellationToken);

        foreach (var run in runs)
        {
            run.PeriodLabel = AssetValuationPeriod.Format(run.Period);
            run.ProcessedByName = names.GetValueOrDefault(run.ProcessedBy);
            int StatusCount(AccountingOutboxStatus status) => outbox.Where(x => x.SourceDocumentId == run.Id && x.Status == status).Sum(x => x.Count);
            run.Outbox = new DepreciationOutboxSummaryResponse
            {
                Held = StatusCount(AccountingOutboxStatus.Held),
                Pending = StatusCount(AccountingOutboxStatus.Pending),
                Acknowledged = StatusCount(AccountingOutboxStatus.Acknowledged),
                Failed = StatusCount(AccountingOutboxStatus.Failed),
                Total = outbox.Where(x => x.SourceDocumentId == run.Id).Sum(x => x.Count)
            };
        }

        if (!withDetail)
        {
            return;
        }

        foreach (var run in runs)
        {
            run.Groups = await _context.AstDepreciationRunLines.AsNoTracking()
                .Where(x => x.RunId == run.Id && !x.IsDelete)
                .GroupBy(x => new
                {
                    x.AssetGroupId,
                    x.AssetGroup!.InventoryCode,
                    x.AssetGroup.GroupName,
                    x.CostCenterId,
                    x.CostCenter!.CostCenterCode,
                    x.CostCenter.CostCenterName
                })
                .Select(x => new DepreciationGroupTotalResponse
                {
                    AssetGroupId = x.Key.AssetGroupId,
                    InventoryCode = x.Key.InventoryCode,
                    GroupName = x.Key.GroupName,
                    CostCenterId = x.Key.CostCenterId,
                    CostCenterCode = x.Key.CostCenterCode,
                    CostCenterName = x.Key.CostCenterName,
                    AssetCount = x.Select(l => l.AssetId).Distinct().Count(),
                    Amount = x.Sum(l => l.Amount),
                    AdjustmentAmount = x.Where(l => l.AdjustmentReason != DepreciationAdjustmentReason.Tidak).Sum(l => l.Amount)
                })
                .OrderBy(x => x.InventoryCode)
                .ThenBy(x => x.CostCenterCode)
                .ToListAsync(cancellationToken);
        }
    }
}
