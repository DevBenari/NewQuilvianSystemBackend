using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed class StockTakeMatchingService
{
    private const int MaxPageSize = 200;

    private static readonly StockTakeResultCategory[] Discrepancies =
    [
        StockTakeResultCategory.TidakDitemukan,
        StockTakeResultCategory.BedaLokasi,
        StockTakeResultCategory.BedaKondisi,
        StockTakeResultCategory.TidakTerdaftar
    ];

    private readonly ApplicationDbContext _context;

    public StockTakeMatchingService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    internal async Task<StockTakeResultCountsResponse> ComputeAsync(AstStockTakeSession session, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var expected = await _context.AstStockTakeExpectedItems
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && !x.IsDelete)
            .Select(x => new { x.AssetId, x.ExpectedLocationId, x.ExpectedCondition, x.LoanBorrowerOrganizationUnitId })
            .ToListAsync(cancellationToken);

        var scans = await _context.AstStockTakeScans
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && !x.IsDelete && x.IsLatest)
            .Select(x => new
            {
                x.Id,
                x.AssetId,
                x.IsUnregisteredFinding,
                x.FoundLocationId,
                FoundUnitId = x.FoundLocation != null ? x.FoundLocation.OrganizationUnitId : Guid.Empty,
                x.ObservedCondition
            })
            .ToListAsync(cancellationToken);

        var previous = await _context.AstStockTakeResults
            .Where(x => x.SessionId == session.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var notes = previous
            .Where(x => !string.IsNullOrWhiteSpace(x.TeamNote))
            .GroupBy(NoteKey)
            .ToDictionary(x => x.Key, x => x.First().TeamNote);
        foreach (var old in previous)
        {
            old.IsDelete = true;
            old.DeleteBy = actorUserId;
            old.DeleteDateTime = now;
        }

        var expectedIds = expected.Select(x => x.AssetId).ToHashSet();
        var scanByAsset = scans.Where(x => x.AssetId.HasValue && !x.IsUnregisteredFinding)
            .GroupBy(x => x.AssetId!.Value)
            .ToDictionary(x => x.Key, x => x.First());

        var results = new List<AstStockTakeResult>();
        foreach (var item in expected)
        {
            var result = NewResult(session.Id, actorUserId, now);
            result.AssetId = item.AssetId;
            result.ExpectedLocationId = item.ExpectedLocationId;
            result.ExpectedCondition = item.ExpectedCondition;

            if (!scanByAsset.TryGetValue(item.AssetId, out var scan))
            {
                result.Category = StockTakeResultCategory.TidakDitemukan;
                results.Add(result);
                continue;
            }

            result.ScanId = scan.Id;
            result.FoundLocationId = scan.FoundLocationId;
            result.ObservedCondition = scan.ObservedCondition;
            result.IsConditionMismatch = scan.ObservedCondition != item.ExpectedCondition;

            if (item.LoanBorrowerOrganizationUnitId is { } borrower)
            {
                result.IsLocationMismatch = scan.FoundUnitId != borrower;
                result.Category = result.IsLocationMismatch
                    ? StockTakeResultCategory.BedaLokasi
                    : result.IsConditionMismatch ? StockTakeResultCategory.BedaKondisi : StockTakeResultCategory.SesuaiSedangDipinjam;
            }
            else
            {
                result.IsLocationMismatch = scan.FoundLocationId != item.ExpectedLocationId;
                result.Category = result.IsLocationMismatch
                    ? StockTakeResultCategory.BedaLokasi
                    : result.IsConditionMismatch ? StockTakeResultCategory.BedaKondisi : StockTakeResultCategory.Sesuai;
            }

            results.Add(result);
        }

        var foreignScans = scanByAsset.Values.Where(x => !expectedIds.Contains(x.AssetId!.Value)).ToList();
        if (foreignScans.Count > 0)
        {
            var foreignIds = foreignScans.Select(x => x.AssetId!.Value).ToList();
            var assets = await _context.AstAssets
                .AsNoTracking()
                .Where(x => foreignIds.Contains(x.Id))
                .Select(x => new { x.Id, x.CurrentOrganizationUnitId, x.CurrentLocationId, x.Condition })
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            foreach (var scan in foreignScans)
            {
                var asset = assets.GetValueOrDefault(scan.AssetId!.Value);
                var result = NewResult(session.Id, actorUserId, now);
                result.AssetId = scan.AssetId;
                result.ScanId = scan.Id;
                result.Category = StockTakeResultCategory.BedaLokasi;
                result.IsLocationMismatch = true;
                result.IsForeignAsset = asset?.CurrentOrganizationUnitId != session.OrganizationUnitId;
                result.ExpectedLocationId = asset?.CurrentLocationId;
                result.ExpectedCondition = asset?.Condition;
                result.FoundLocationId = scan.FoundLocationId;
                result.ObservedCondition = scan.ObservedCondition;
                result.IsConditionMismatch = asset != null && asset.Condition != scan.ObservedCondition;
                results.Add(result);
            }
        }

        foreach (var finding in scans.Where(x => x.IsUnregisteredFinding))
        {
            var result = NewResult(session.Id, actorUserId, now);
            result.ScanId = finding.Id;
            result.Category = StockTakeResultCategory.TidakTerdaftar;
            result.FoundLocationId = finding.FoundLocationId;
            result.ObservedCondition = finding.ObservedCondition;
            results.Add(result);
        }

        foreach (var result in results)
        {
            if (notes.TryGetValue(NoteKey(result), out var note))
            {
                result.TeamNote = note;
            }
        }

        _context.AstStockTakeResults.AddRange(results);
        return Count(results.Select(x => x.Category));
    }

    public async Task<StockTakeResultCountsResponse> GetCountsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var categories = await _context.AstStockTakeResults
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId && !x.IsDelete)
            .Select(x => x.Category)
            .ToListAsync(cancellationToken);
        return Count(categories);
    }

    public async Task<PagedResult<StockTakeResultResponse>> GetResultsAsync(Guid sessionId, StockTakeItemQuery request, CancellationToken cancellationToken = default, Guid? resultId = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = _context.AstStockTakeResults.AsNoTracking().Where(x => x.SessionId == sessionId && !x.IsDelete);
        if (resultId is { } onlyResult)
        {
            query = query.Where(x => x.Id == onlyResult);
        }

        var categories = (request.Category ?? []).Distinct().ToList();
        if (categories.Count > 0)
        {
            query = query.Where(x => categories.Contains(x.Category));
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                (x.Asset != null && ((x.Asset.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) || x.Asset.AssetName.ToUpper().Contains(keyword))) ||
                (x.Scan != null && ((x.Scan.ScannedCode != null && x.Scan.ScannedCode.ToUpper().Contains(keyword)) ||
                                    (x.Scan.FindingDescription != null && x.Scan.FindingDescription.ToUpper().Contains(keyword)))));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Asset != null ? x.Asset.InventoryNumber : null)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockTakeResultResponse
            {
                Id = x.Id,
                Category = x.Category,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset != null ? x.Asset.InventoryNumber : null,
                AssetName = x.Asset != null ? x.Asset.AssetName : null,
                GroupName = x.Asset != null && x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
                IsLocationMismatch = x.IsLocationMismatch,
                IsConditionMismatch = x.IsConditionMismatch,
                IsForeignAsset = x.IsForeignAsset,
                OwnerOrganizationUnitName = x.IsForeignAsset && x.Asset != null && x.Asset.CurrentOrganizationUnit != null ? x.Asset.CurrentOrganizationUnit.UnitName : null,
                ExpectedLocationId = x.ExpectedLocationId,
                ExpectedLocationName = x.ExpectedLocation != null ? x.ExpectedLocation.LocationName : null,
                FoundLocationId = x.FoundLocationId,
                FoundLocationName = x.FoundLocation != null ? x.FoundLocation.LocationName : null,
                ExpectedCondition = x.ExpectedCondition,
                ObservedCondition = x.ObservedCondition,
                ScanId = x.ScanId,
                ScannedCode = x.Scan != null ? x.Scan.ScannedCode : null,
                FindingDescription = x.Scan != null ? x.Scan.FindingDescription : null,
                PhotoAttachmentId = x.Scan != null ? x.Scan.PhotoAttachmentId : null,
                ScannedByName = x.Scan != null && x.Scan.ScannedByEmployee != null ? x.Scan.ScannedByEmployee.FullName : null,
                ScannedAt = x.Scan != null ? x.Scan.ScannedAt : null,
                TeamNote = x.TeamNote
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.CategoryName = AssetStockTakeLabels.Category(item.Category);
            item.ExpectedConditionName = item.ExpectedCondition is { } expected ? AssetRegisterLabels.Condition(expected) : null;
            item.ObservedConditionName = item.ObservedCondition is { } observed ? AssetRegisterLabels.Condition(observed) : null;
        }

        return new PagedResult<StockTakeResultResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private static AstStockTakeResult NewResult(Guid sessionId, Guid actorUserId, DateTime now) => new()
    {
        SessionId = sessionId,
        CreateBy = actorUserId,
        CreateDateTime = now
    };

    private static string NoteKey(AstStockTakeResult result) =>
        result.AssetId is { } assetId ? $"asset:{assetId}" : $"scan:{result.ScanId}";

    private static StockTakeResultCountsResponse Count(IEnumerable<StockTakeResultCategory> categories)
    {
        var list = categories.ToList();
        int Of(StockTakeResultCategory category) => list.Count(x => x == category);
        return new StockTakeResultCountsResponse
        {
            Total = list.Count,
            Sesuai = Of(StockTakeResultCategory.Sesuai),
            SesuaiSedangDipinjam = Of(StockTakeResultCategory.SesuaiSedangDipinjam),
            TidakDitemukan = Of(StockTakeResultCategory.TidakDitemukan),
            BedaLokasi = Of(StockTakeResultCategory.BedaLokasi),
            BedaKondisi = Of(StockTakeResultCategory.BedaKondisi),
            TidakTerdaftar = Of(StockTakeResultCategory.TidakTerdaftar),
            Discrepancies = list.Count(x => Discrepancies.Contains(x))
        };
    }
}
