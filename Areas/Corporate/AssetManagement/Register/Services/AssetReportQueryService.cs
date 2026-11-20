using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetReportQueryService
{
    public const string InventoryByUnitCode = "inventory-by-unit";
    public const string ConditionCode = "condition";
    public const string HistoryCode = "history";
    public const int MaxExportRows = 10000;

    private const int MaxPageSize = 100;

    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetRecordService _recordService;

    public AssetReportQueryService(ApplicationDbContext context, AssetActorResolver actorResolver, AssetRecordService recordService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _recordService = recordService ?? throw new ArgumentNullException(nameof(recordService));
    }

    public static AssetReportMetadataResponse GetMetadata()
    {
        List<string> assetParameters = ["search", "organizationUnitId", "locationId", "categoryId", "groupId", "condition", "placementState", "sortBy", "sortDirection", "pageNumber", "pageSize"];

        return new AssetReportMetadataResponse
        {
            Reports =
            [
                new()
                {
                    Code = InventoryByUnitCode,
                    Title = TitleOf(InventoryByUnitCode),
                    QueryParameters = assetParameters,
                    SortOptions = [Option("unit", "Unit dan Ruangan"), Option("inventoryNumber", "Nomor Inventaris"), Option("name", "Nama Aset"), Option("condition", "Kondisi")]
                },
                new()
                {
                    Code = ConditionCode,
                    Title = TitleOf(ConditionCode),
                    QueryParameters = assetParameters,
                    SortOptions = [Option("condition", "Kondisi"), Option("unit", "Unit dan Ruangan"), Option("inventoryNumber", "Nomor Inventaris"), Option("name", "Nama Aset")]
                },
                new()
                {
                    Code = HistoryCode,
                    Title = TitleOf(HistoryCode),
                    QueryParameters = ["search", "organizationUnitId", "locationId", "categoryId", "groupId", "assetId", "eventType", "subModule", "actorUserId", "occurredFrom", "occurredTo", "sortDirection", "pageNumber", "pageSize"],
                    SortOptions = [Option("occurredAt", "Waktu Kejadian")]
                }
            ],
            EventTypes = Enum.GetValues<AssetHistoryEventType>()
                .Select(x => Option(((int)x).ToString(CultureInfo.InvariantCulture), AssetRegisterLabels.HistoryEvent(x)))
                .ToList(),
            SubModules = AssetSubModules.All
                .OrderBy(SubModuleOrder)
                .Select(x => Option(x, SubModuleName(x)))
                .ToList(),
            Conditions = Enum.GetValues<AssetCondition>()
                .Select(x => Option(((int)x).ToString(CultureInfo.InvariantCulture), AssetRegisterLabels.Condition(x)))
                .ToList(),
            PlacementStates = Enum.GetValues<AssetPlacementState>()
                .Select(x => Option(((int)x).ToString(CultureInfo.InvariantCulture), AssetRegisterLabels.PlacementState(x)))
                .ToList(),
            MaxExportRows = MaxExportRows
        };
    }

    public async Task<PagedResult<AssetListItemResponse>> GetInventoryByUnitAsync(AssetReportQuery request, CancellationToken cancellationToken = default)
    {
        var query = await AssetReportBaseAsync(request, cancellationToken);
        return await PageAssetsAsync(OrderAssets(query, request.SortBy, request.SortDirection, "unit"), request, cancellationToken);
    }

    public async Task<PagedResult<AssetListItemResponse>> GetConditionAsync(AssetReportQuery request, CancellationToken cancellationToken = default)
    {
        var query = await AssetReportBaseAsync(request, cancellationToken);
        return await PageAssetsAsync(OrderAssets(query, request.SortBy, request.SortDirection, "condition"), request, cancellationToken);
    }

    public async Task<PagedResult<AssetHistoryReportItemResponse>> GetHistoryAsync(AssetReportQuery request, CancellationToken cancellationToken = default)
    {
        var query = await HistoryBaseAsync(request, cancellationToken);
        var (pageNumber, pageSize) = Paging(request);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        await DescribeHistoryAsync(items, cancellationToken);

        return new PagedResult<AssetHistoryReportItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetReportFile> ExportAsync(string reportCode, AssetReportQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = (reportCode ?? string.Empty).Trim().ToLowerInvariant();
        if (code is not (InventoryByUnitCode or ConditionCode or HistoryCode))
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.ReportNotFound, AssetRegisterMessages.ReportNotFound);
        }

        var actor = await RequireActorAsync(cancellationToken);
        var printedBy = (await _recordService.UserNamesAsync([actor.UserId], cancellationToken)).GetValueOrDefault(actor.UserId) ?? "-";
        var infoLines = new List<string>
        {
            $"Dicetak: {ToReportTime(DateTime.UtcNow).ToString("dd MMMM yyyy HH:mm", Indonesian)} WIB oleh {printedBy}",
            await FilterDescriptionAsync(request, cancellationToken)
        };

        AssetReportSheet sheet;
        if (code == HistoryCode)
        {
            var query = await HistoryBaseAsync(request, cancellationToken);
            await EnsureExportLimitAsync(query.CountAsync(cancellationToken));
            var items = await query.ToListAsync(cancellationToken);
            await DescribeHistoryAsync(items, cancellationToken);
            sheet = HistorySheet(items, infoLines);
        }
        else
        {
            var baseQuery = await AssetReportBaseAsync(request, cancellationToken);
            var ordered = OrderAssets(baseQuery, request.SortBy, request.SortDirection, code == ConditionCode ? "condition" : "unit");
            await EnsureExportLimitAsync(ordered.CountAsync(cancellationToken));
            var items = await AssetRecordService.ProjectList(ordered).ToListAsync(cancellationToken);
            items.ForEach(AssetRecordService.Describe);
            sheet = code == ConditionCode ? ConditionSheet(items, infoLines) : InventorySheet(items, infoLines);
        }

        var stamp = ToReportTime(DateTime.UtcNow).ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture);
        return new AssetReportFile(AssetReportWorkbook.Build(sheet), $"laporan-aset-{code}-{stamp}.xlsx");
    }

    private async Task<IQueryable<AstAsset>> AssetReportBaseAsync(AssetReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await RequireActorAsync(cancellationToken);
        return await _recordService.FilteredAssetsAsync(
            new AssetPagedQuery
            {
                Search = request.Search,
                OrganizationUnitId = request.OrganizationUnitId,
                LocationId = request.LocationId,
                CategoryId = request.CategoryId,
                GroupId = request.GroupId,
                Condition = request.Condition,
                PlacementState = request.PlacementState,
                RecordStatus = AssetRecordStatus.Terdaftar
            },
            cancellationToken);
    }

    private async Task<IQueryable<AssetHistoryReportItemResponse>> HistoryBaseAsync(AssetReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await RequireActorAsync(cancellationToken);

        if (request.OccurredFrom is { } from && request.OccurredTo is { } to && from > to)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReportField, AssetRegisterMessages.ReportPeriod);
        }

        var subModule = string.IsNullOrWhiteSpace(request.SubModule) ? null : request.SubModule.Trim().ToLowerInvariant();
        if (subModule != null && !AssetSubModules.All.Contains(subModule))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReportField, AssetRegisterMessages.ReportSubModule);
        }

        var assets = await _recordService.FilteredAssetsAsync(
            new AssetPagedQuery
            {
                Search = request.Search,
                OrganizationUnitId = request.OrganizationUnitId,
                LocationId = request.LocationId,
                CategoryId = request.CategoryId,
                GroupId = request.GroupId
            },
            cancellationToken);

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            assets = assets.Where(x => x.Id == assetId);
        }

        var histories = _context.AstAssetHistories.AsNoTracking().Where(x => !x.IsDelete);

        if (request.EventType is { } eventType)
        {
            histories = histories.Where(x => x.EventType == eventType);
        }

        if (subModule != null)
        {
            histories = histories.Where(x => x.SubModule == subModule);
        }

        if (request.ActorUserId is { } actorUserId && actorUserId != Guid.Empty)
        {
            histories = histories.Where(x => x.ActorUserId == actorUserId);
        }

        if (request.OccurredFrom is { } occurredFrom)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(occurredFrom.ToDateTime(TimeOnly.MinValue));
            histories = histories.Where(x => x.OccurredAt >= fromUtc);
        }

        if (request.OccurredTo is { } occurredTo)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(occurredTo.AddDays(1).ToDateTime(TimeOnly.MinValue));
            histories = histories.Where(x => x.OccurredAt < toUtcExclusive);
        }

        var joined = from history in histories
                     join asset in assets on history.AssetId equals asset.Id
                     select new { History = history, Asset = asset };

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? joined.OrderBy(x => x.History.OccurredAt).ThenBy(x => x.History.CreateDateTime).ThenBy(x => x.History.Id)
            : joined.OrderByDescending(x => x.History.OccurredAt).ThenByDescending(x => x.History.CreateDateTime).ThenBy(x => x.History.Id);

        return ordered.Select(x => new AssetHistoryReportItemResponse
        {
            Id = x.History.Id,
            AssetId = x.Asset.Id,
            InventoryNumber = x.Asset.InventoryNumber,
            AssetName = x.Asset.AssetName,
            CurrentOrganizationUnitName = x.Asset.CurrentOrganizationUnit != null ? x.Asset.CurrentOrganizationUnit.UnitName : null,
            EventType = x.History.EventType,
            SubModule = x.History.SubModule,
            FromValue = x.History.FromValue,
            ToValue = x.History.ToValue,
            SourceDocumentType = x.History.SourceDocumentType,
            SourceDocumentId = x.History.SourceDocumentId,
            SourceDocumentNumber = x.History.SourceDocumentNumber,
            ActorUserId = x.History.ActorUserId,
            Reason = x.History.Reason,
            OccurredAt = x.History.OccurredAt
        });
    }

    private static IOrderedQueryable<AstAsset> OrderAssets(IQueryable<AstAsset> query, string? sortBy, string? sortDirection, string defaultSort)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var key = string.IsNullOrWhiteSpace(sortBy) ? defaultSort : sortBy.Trim().ToLowerInvariant();

        IOrderedQueryable<AstAsset> ordered = key switch
        {
            "condition" => (descending ? query.OrderByDescending(x => x.Condition) : query.OrderBy(x => x.Condition))
                .ThenBy(x => x.CurrentOrganizationUnit!.UnitName)
                .ThenBy(x => x.InventoryNumber),
            "inventorynumber" => descending ? query.OrderByDescending(x => x.InventoryNumber) : query.OrderBy(x => x.InventoryNumber),
            "name" => (descending ? query.OrderByDescending(x => x.AssetName) : query.OrderBy(x => x.AssetName)).ThenBy(x => x.InventoryNumber),
            _ => (descending ? query.OrderByDescending(x => x.CurrentOrganizationUnit!.UnitName) : query.OrderBy(x => x.CurrentOrganizationUnit!.UnitName))
                .ThenBy(x => x.CurrentLocation!.LocationName)
                .ThenBy(x => x.InventoryNumber)
        };

        return ordered.ThenBy(x => x.Id);
    }

    private static async Task<PagedResult<AssetListItemResponse>> PageAssetsAsync(IOrderedQueryable<AstAsset> query, AssetReportQuery request, CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Paging(request);
        var total = await query.CountAsync(cancellationToken);
        var items = await AssetRecordService.ProjectList(query.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        items.ForEach(AssetRecordService.Describe);

        return new PagedResult<AssetListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    private async Task DescribeHistoryAsync(List<AssetHistoryReportItemResponse> items, CancellationToken cancellationToken)
    {
        var names = await _recordService.UserNamesAsync(items.Select(x => (Guid?)x.ActorUserId), cancellationToken);
        foreach (var item in items)
        {
            item.EventTypeName = AssetRegisterLabels.HistoryEvent(item.EventType);
            item.SubModuleName = SubModuleName(item.SubModule);
            item.ActorName = names.GetValueOrDefault(item.ActorUserId);
        }
    }

    private async Task<string> FilterDescriptionAsync(AssetReportQuery request, CancellationToken cancellationToken)
    {
        var parts = new List<string>();

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            var unitName = await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken);
            parts.Add($"Unit: {unitName ?? "-"}");
        }

        if (request.LocationId is { } locationId && locationId != Guid.Empty)
        {
            var locationName = await _context.MstAssetLocations.AsNoTracking().Where(x => x.Id == locationId).Select(x => x.LocationName).FirstOrDefaultAsync(cancellationToken);
            parts.Add($"Ruangan: {locationName ?? "-"}");
        }

        if (request.GroupId is { } groupId && groupId != Guid.Empty)
        {
            var groupName = await _context.MstAssetGroups.AsNoTracking().Where(x => x.Id == groupId).Select(x => x.GroupName).FirstOrDefaultAsync(cancellationToken);
            parts.Add($"Kelompok: {groupName ?? "-"}");
        }

        if (request.CategoryId is { } categoryId && categoryId != Guid.Empty)
        {
            var categoryName = await _context.MstAssetCategories.AsNoTracking().Where(x => x.Id == categoryId).Select(x => x.CategoryName).FirstOrDefaultAsync(cancellationToken);
            parts.Add($"Kategori: {categoryName ?? "-"}");
        }

        if (request.Condition is { } condition)
        {
            parts.Add($"Kondisi: {AssetRegisterLabels.Condition(condition)}");
        }

        if (request.PlacementState is { } placementState)
        {
            parts.Add($"Penempatan: {AssetRegisterLabels.PlacementState(placementState)}");
        }

        if (request.EventType is { } eventType)
        {
            parts.Add($"Kejadian: {AssetRegisterLabels.HistoryEvent(eventType)}");
        }

        if (!string.IsNullOrWhiteSpace(request.SubModule))
        {
            parts.Add($"Sub-modul: {SubModuleName(request.SubModule.Trim().ToLowerInvariant())}");
        }

        if (request.OccurredFrom.HasValue || request.OccurredTo.HasValue)
        {
            var from = request.OccurredFrom?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "awal";
            var to = request.OccurredTo?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "sekarang";
            parts.Add($"Periode: {from} s.d. {to}");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            parts.Add($"Kata kunci: {request.Search.Trim()}");
        }

        return parts.Count == 0 ? "Saringan: seluruh unit yang dapat Anda lihat" : $"Saringan: {string.Join("; ", parts)}";
    }

    private static async Task EnsureExportLimitAsync(Task<int> countTask)
    {
        var count = await countTask;
        if (count > MaxExportRows)
        {
            throw AssetManagementException.Invalid(
                AssetRegisterErrorCodes.ReportExportLimit,
                string.Format(Indonesian, AssetRegisterMessages.ReportExportLimitFormat, count, MaxExportRows));
        }
    }

    private static AssetReportSheet InventorySheet(List<AssetListItemResponse> items, List<string> infoLines) => new(
        TitleOf(InventoryByUnitCode),
        infoLines,
        [
            new("No"), new("Unit"), new("Ruangan"), new("Nomor Inventaris"), new("Nama Aset"), new("Kategori"), new("Kelompok"),
            new("Merek"), new("Nomor Seri"), new("Jenis Kepemilikan"), new("Jenis Perolehan"), new("Tanggal Perolehan"),
            new("Penanggung Jawab"), new("Kondisi"), new("Penempatan")
        ],
        items.Select((x, index) => new object?[]
        {
            index + 1, x.CurrentOrganizationUnitName ?? "Belum ditempatkan", x.CurrentLocationName, x.InventoryNumber, x.AssetName,
            x.CategoryName, x.GroupName, x.Brand, x.SerialNumber, x.OwnershipTypeName, x.AcquisitionTypeName, x.AcquisitionDate,
            x.CurrentCustodianEmployeeName, x.ConditionName, x.PlacementStateName
        }).ToList());

    private static AssetReportSheet ConditionSheet(List<AssetListItemResponse> items, List<string> infoLines) => new(
        TitleOf(ConditionCode),
        infoLines,
        [
            new("No"), new("Kondisi"), new("Nomor Inventaris"), new("Nama Aset"), new("Kelompok"), new("Merek"), new("Nomor Seri"),
            new("Unit"), new("Ruangan"), new("Penanggung Jawab"), new("Penempatan")
        ],
        items.Select((x, index) => new object?[]
        {
            index + 1, x.ConditionName, x.InventoryNumber, x.AssetName, x.GroupName, x.Brand, x.SerialNumber,
            x.CurrentOrganizationUnitName ?? "Belum ditempatkan", x.CurrentLocationName, x.CurrentCustodianEmployeeName, x.PlacementStateName
        }).ToList());

    private static AssetReportSheet HistorySheet(List<AssetHistoryReportItemResponse> items, List<string> infoLines) => new(
        TitleOf(HistoryCode),
        infoLines,
        [
            new("No"), new("Waktu (WIB)"), new("Nomor Inventaris"), new("Nama Aset"), new("Unit Saat Ini"), new("Kejadian"), new("Sub-modul"),
            new("Dari"), new("Menjadi"), new("Dokumen Sumber"), new("Pelaku"), new("Alasan")
        ],
        items.Select((x, index) => new object?[]
        {
            index + 1, ToReportTime(x.OccurredAt), x.InventoryNumber, x.AssetName, x.CurrentOrganizationUnitName, x.EventTypeName, x.SubModuleName,
            x.FromValue, x.ToValue, x.SourceDocumentNumber, x.ActorName, x.Reason
        }).ToList());

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private static (int PageNumber, int PageSize) Paging(AssetReportQuery request) =>
        (request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize));

    private static DateTime ToReportTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeFromUtc(AppDateTimeHelper.ToUtc(value), ReportTimeZone);

    private static string TitleOf(string code) => code switch
    {
        InventoryByUnitCode => "Daftar Inventaris Per Unit",
        ConditionCode => "Laporan Kondisi Aset",
        HistoryCode => "History & Audit Aset",
        _ => code
    };

    private static string SubModuleName(string subModule) => subModule switch
    {
        AssetSubModules.Register => "Register Aset",
        AssetSubModules.Movement => "Pergerakan Aset",
        AssetSubModules.PhysicalInventory => "Inventarisasi Aset",
        AssetSubModules.Valuation => "Nilai Aset",
        AssetSubModules.Disposal => "Penghapusan Aset",
        _ => subModule
    };

    private static int SubModuleOrder(string subModule) => subModule switch
    {
        AssetSubModules.Register => 1,
        AssetSubModules.Movement => 2,
        AssetSubModules.PhysicalInventory => 3,
        AssetSubModules.Valuation => 4,
        AssetSubModules.Disposal => 5,
        _ => 9
    };

    private static AssetReportOptionResponse Option(string value, string label) => new() { Value = value, Label = label };
}
