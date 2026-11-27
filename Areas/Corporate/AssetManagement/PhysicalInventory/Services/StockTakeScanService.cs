using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed class StockTakeScanService
{
    public const string ScanOwnerType = "StockTakeScan";
    private const string LogCategory = "Corporate.AssetManagement.PhysicalInventory";
    private const int MaxCodeLength = 60;
    private const int MaxInventoryNumberLength = 30;
    private const int MaxNoteLength = 500;
    private const int MaxPageSize = 200;

    private static readonly AssetCondition[] ObservableConditions =
        [AssetCondition.Aktif, AssetCondition.RusakRingan, AssetCondition.RusakBerat, AssetCondition.TidakDigunakan];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetAttachmentService _attachmentService;
    private readonly StockTakeSessionService _sessionService;
    private readonly LoggerService _loggerService;

    public StockTakeScanService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetAttachmentService attachmentService,
        StockTakeSessionService sessionService,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<StockTakeLookupResponse> LookupAsync(Guid sessionId, string? code, CancellationToken cancellationToken = default)
    {
        var number = RequiredCode(code);
        var actor = await RequireActorAsync(cancellationToken);
        var session = await _context.AstStockTakeSessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDelete, cancellationToken)
            ?? throw SessionNotFound();
        await RequireMemberAsync(session.Id, actor, cancellationToken);
        EnsureRunning(session.Status);

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var asset = await FindAssetAsync(number, entity.LegalEntityId, cancellationToken);
        var expected = await _context.AstStockTakeExpectedItems
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && x.AssetId == asset.Id && !x.IsDelete)
            .Select(x => new
            {
                x.ExpectedLocationId,
                ExpectedLocationName = x.ExpectedLocation != null ? x.ExpectedLocation.LocationName : null,
                x.ExpectedCondition,
                x.LoanBorrowerOrganizationUnitId,
                BorrowerName = x.LoanBorrowerOrganizationUnit != null ? x.LoanBorrowerOrganizationUnit.UnitName : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        var response = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == asset.Id)
            .Select(x => new StockTakeLookupResponse
            {
                AssetId = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                GroupName = x.AssetGroup != null ? x.AssetGroup.GroupName : null,
                CurrentCondition = x.Condition,
                CurrentOrganizationUnitId = x.CurrentOrganizationUnitId,
                CurrentOrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null
            })
            .FirstAsync(cancellationToken);

        response.CurrentConditionName = AssetRegisterLabels.Condition(response.CurrentCondition);
        response.IsExpected = expected != null;
        response.ExpectedLocationId = expected?.ExpectedLocationId;
        response.ExpectedLocationName = expected?.ExpectedLocationName;
        response.ExpectedCondition = expected?.ExpectedCondition;
        response.ExpectedConditionName = expected != null ? AssetRegisterLabels.Condition(expected.ExpectedCondition) : null;
        response.LoanBorrowerOrganizationUnitId = expected?.LoanBorrowerOrganizationUnitId;
        response.LoanBorrowerOrganizationUnitName = expected?.BorrowerName;

        var last = await ProjectScans(_context.AstStockTakeScans.AsNoTracking()
                .Where(x => x.SessionId == session.Id && x.AssetId == asset.Id && x.IsLatest && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken);
        if (last != null)
        {
            Describe(last);
            response.LastScan = last;
        }

        return response;
    }

    public async Task<StockTakeScanResponse> ScanAsync(Guid sessionId, StockTakeScanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var number = RequiredCode(request.Code);
        var condition = RequiredCondition(request.ObservedCondition);
        var notes = OptionalText(request.Notes, AssetStockTakeMessages.NotesLength500);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var saved = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(sessionId, cancellationToken);
            var employeeId = await RequireMemberAsync(session.Id, actor, cancellationToken);
            EnsureRunning(session.Status);
            var locationId = await RequiredLocationAsync(request.FoundLocationId, cancellationToken);
            var asset = await FindAssetAsync(number, entity.LegalEntityId, cancellationToken);

            var previous = await _context.AstStockTakeScans
                .Where(x => x.SessionId == session.Id && x.AssetId == asset.Id && x.IsLatest && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;
            foreach (var old in previous)
            {
                old.IsLatest = false;
                old.UpdateBy = actor.UserId;
                old.UpdateDateTime = now;
            }

            var scan = new AstStockTakeScan
            {
                SessionId = session.Id,
                AssetId = asset.Id,
                ScannedCode = number,
                IsManualEntry = request.IsManualEntry,
                FoundLocationId = locationId,
                ObservedCondition = condition,
                Notes = notes,
                ScannedByEmployeeId = employeeId,
                ScannedAt = now,
                IsLatest = true,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            if (request.Photo is { Length: > 0 })
            {
                var photo = await _attachmentService.SaveAsync(request.Photo, ScanOwnerType, scan.Id, actor.UserId, cancellationToken);
                scan.PhotoAttachmentId = photo.Id;
            }

            _context.AstStockTakeScans.Add(scan);
            await _context.SaveChangesAsync(cancellationToken);
            await RefreshScannedCountAsync(session, actor.UserId, now, cancellationToken);
            return (scan.Id, session.SessionNumber, Replaced: previous.Count > 0, asset.InventoryNumber);
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Scan", $"Sesi {saved.SessionNumber}: aset {saved.InventoryNumber} dipindai oleh {actor.UserId}{(saved.Replaced ? " (menggantikan pindaian sebelumnya)" : string.Empty)}.");
        return await GetScanAsync(saved.Id, cancellationToken);
    }

    public async Task<StockTakeScanResponse> RecordFindingAsync(Guid sessionId, StockTakeUnregisteredFindingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var description = request.Description?.Trim();
        if (string.IsNullOrEmpty(description) || request.Photo is not { Length: > 0 })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Finding, AssetStockTakeMessages.Finding);
        }

        if (description.Length > MaxNoteLength)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Finding, AssetStockTakeMessages.FindingLength);
        }

        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim().ToUpperInvariant();
        if (code is { Length: > MaxCodeLength })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.CodeRequired);
        }

        var condition = RequiredCondition(request.ObservedCondition);
        var notes = OptionalText(request.Notes, AssetStockTakeMessages.NotesLength500);
        var actor = await RequireActorAsync(cancellationToken);

        var saved = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(sessionId, cancellationToken);
            var employeeId = await RequireMemberAsync(session.Id, actor, cancellationToken);
            EnsureRunning(session.Status);
            var locationId = await RequiredLocationAsync(request.FoundLocationId, cancellationToken);

            var now = DateTime.UtcNow;
            var scan = new AstStockTakeScan
            {
                SessionId = session.Id,
                ScannedCode = code,
                IsManualEntry = true,
                IsUnregisteredFinding = true,
                FindingDescription = description,
                FoundLocationId = locationId,
                ObservedCondition = condition,
                Notes = notes,
                ScannedByEmployeeId = employeeId,
                ScannedAt = now,
                IsLatest = true,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            var photo = await _attachmentService.SaveAsync(request.Photo, ScanOwnerType, scan.Id, actor.UserId, cancellationToken);
            scan.PhotoAttachmentId = photo.Id;
            _context.AstStockTakeScans.Add(scan);
            await _context.SaveChangesAsync(cancellationToken);
            return (scan.Id, session.SessionNumber);
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Finding", $"Sesi {saved.SessionNumber}: temuan tidak terdaftar dicatat oleh {actor.UserId}.");
        return await GetScanAsync(saved.Id, cancellationToken);
    }

    public async Task<PagedResult<StockTakeExpectedItemResponse>> GetExpectedItemsAsync(Guid sessionId, StockTakeItemQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _sessionService.GetSessionAsync(sessionId, cancellationToken);

        var latest = _context.AstStockTakeScans.Where(s => s.SessionId == sessionId && s.IsLatest && !s.IsDelete && !s.IsUnregisteredFinding);
        var query = _context.AstStockTakeExpectedItems.AsNoTracking().Where(x => x.SessionId == sessionId && !x.IsDelete);
        if (request.Scanned is { } scanned)
        {
            query = scanned
                ? query.Where(x => latest.Any(s => s.AssetId == x.AssetId))
                : query.Where(x => !latest.Any(s => s.AssetId == x.AssetId));
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                (x.Asset!.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                x.Asset.AssetName.ToUpper().Contains(keyword) ||
                (x.ExpectedLocation != null && x.ExpectedLocation.LocationName.ToUpper().Contains(keyword)));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.ExpectedLocation != null ? x.ExpectedLocation.LocationName : null)
            .ThenBy(x => x.Asset!.InventoryNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockTakeExpectedItemResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
                ExpectedLocationId = x.ExpectedLocationId,
                ExpectedLocationName = x.ExpectedLocation != null ? x.ExpectedLocation.LocationName : null,
                ExpectedCustodianName = x.ExpectedCustodianEmployee != null ? x.ExpectedCustodianEmployee.FullName : null,
                ExpectedCondition = x.ExpectedCondition,
                LoanBorrowerOrganizationUnitId = x.LoanBorrowerOrganizationUnitId,
                LoanBorrowerOrganizationUnitName = x.LoanBorrowerOrganizationUnit != null ? x.LoanBorrowerOrganizationUnit.UnitName : null,
                IsScanned = latest.Any(s => s.AssetId == x.AssetId),
                FoundLocationName = latest.Where(s => s.AssetId == x.AssetId).Select(s => s.FoundLocation!.LocationName).FirstOrDefault(),
                ObservedCondition = latest.Where(s => s.AssetId == x.AssetId).Select(s => (AssetCondition?)s.ObservedCondition).FirstOrDefault(),
                ScannedAt = latest.Where(s => s.AssetId == x.AssetId).Select(s => (DateTime?)s.ScannedAt).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.ExpectedConditionName = AssetRegisterLabels.Condition(item.ExpectedCondition);
            item.ObservedConditionName = item.ObservedCondition is { } observed ? AssetRegisterLabels.Condition(observed) : null;
        }

        return Paged(items, total, pageNumber, pageSize);
    }

    public async Task<PagedResult<StockTakeScanResponse>> GetScansAsync(Guid sessionId, StockTakeItemQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _sessionService.GetSessionAsync(sessionId, cancellationToken);

        var query = _context.AstStockTakeScans.AsNoTracking().Where(x => x.SessionId == sessionId && !x.IsDelete);
        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                (x.ScannedCode != null && x.ScannedCode.ToUpper().Contains(keyword)) ||
                (x.Asset != null && x.Asset.AssetName.ToUpper().Contains(keyword)) ||
                (x.FindingDescription != null && x.FindingDescription.ToUpper().Contains(keyword)));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await ProjectScans(query.OrderByDescending(x => x.ScannedAt).ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            Describe(item);
        }

        return Paged(items, total, pageNumber, pageSize);
    }

    private async Task<StockTakeScanResponse> GetScanAsync(Guid scanId, CancellationToken cancellationToken)
    {
        var scan = await ProjectScans(_context.AstStockTakeScans.AsNoTracking().Where(x => x.Id == scanId)).FirstAsync(cancellationToken);
        Describe(scan);
        var counts = await _context.AstStockTakeSessions
            .AsNoTracking()
            .Where(x => x.Scans.Any(s => s.Id == scanId))
            .Select(x => new { x.ScannedCount, x.ExpectedCount })
            .FirstAsync(cancellationToken);
        scan.ScannedCount = counts.ScannedCount;
        scan.ExpectedCount = counts.ExpectedCount;
        return scan;
    }

    private IQueryable<StockTakeScanResponse> ProjectScans(IQueryable<AstStockTakeScan> query) =>
        query.Select(x => new StockTakeScanResponse
        {
            Id = x.Id,
            AssetId = x.AssetId,
            InventoryNumber = x.Asset != null ? x.Asset.InventoryNumber : null,
            AssetName = x.Asset != null ? x.Asset.AssetName : null,
            ScannedCode = x.ScannedCode,
            IsManualEntry = x.IsManualEntry,
            IsUnregisteredFinding = x.IsUnregisteredFinding,
            FindingDescription = x.FindingDescription,
            FoundLocationId = x.FoundLocationId,
            FoundLocationName = x.FoundLocation != null ? x.FoundLocation.LocationName : null,
            ObservedCondition = x.ObservedCondition,
            PhotoAttachmentId = x.PhotoAttachmentId,
            Notes = x.Notes,
            ScannedByEmployeeId = x.ScannedByEmployeeId,
            ScannedByName = x.ScannedByEmployee != null ? x.ScannedByEmployee.FullName : null,
            ScannedAt = x.ScannedAt,
            IsLatest = x.IsLatest,
            IsExpected = x.AssetId != null && _context.AstStockTakeExpectedItems.Any(e => e.SessionId == x.SessionId && e.AssetId == x.AssetId && !e.IsDelete)
        });

    private static void Describe(StockTakeScanResponse scan) =>
        scan.ObservedConditionName = AssetRegisterLabels.Condition(scan.ObservedCondition);

    private async Task RefreshScannedCountAsync(AstStockTakeSession session, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        session.ScannedCount = await _context.AstStockTakeExpectedItems
            .Where(x => x.SessionId == session.Id && !x.IsDelete &&
                        _context.AstStockTakeScans.Any(s => s.SessionId == session.Id && s.AssetId == x.AssetId && s.IsLatest && !s.IsDelete))
            .CountAsync(cancellationToken);
        session.UpdateBy = actorUserId;
        session.UpdateDateTime = now;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> RequireMemberAsync(Guid sessionId, AssetActor actor, CancellationToken cancellationToken)
    {
        if (actor.EmployeeId is { } employeeId &&
            await _context.AstStockTakeTeamMembers.AsNoTracking().AnyAsync(x => x.SessionId == sessionId && x.EmployeeId == employeeId && !x.IsDelete, cancellationToken))
        {
            return employeeId;
        }

        throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.NotTeamMember, AssetStockTakeMessages.NotTeamMember);
    }

    private static void EnsureRunning(StockTakeStatus status)
    {
        if (status != StockTakeStatus.Berjalan)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.CountingClosed, AssetStockTakeMessages.CountingClosed);
        }
    }

    private async Task<Guid> RequiredLocationAsync(Guid? locationId, CancellationToken cancellationToken)
    {
        if (locationId is not { } id || id == Guid.Empty ||
            !await _context.MstAssetLocations.AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.FoundLocation, AssetStockTakeMessages.FoundLocation);
        }

        return id;
    }

    private async Task<(Guid Id, string? InventoryNumber)> FindAssetAsync(string number, Guid legalEntityId, CancellationToken cancellationToken)
    {
        var asset = number.Length > MaxInventoryNumberLength
            ? null
            : await _context.AstAssets
                .AsNoTracking()
                .Where(x => x.InventoryNumber == number && !x.IsDelete && x.LegalEntityId == legalEntityId)
                .Select(x => new { x.Id, x.InventoryNumber, x.RecordStatus })
                .FirstOrDefaultAsync(cancellationToken);
        if (asset == null)
        {
            throw AssetManagementException.NotFound(AssetStockTakeErrorCodes.UnknownCode, string.Format(AssetStockTakeMessages.UnknownCodeFormat, number));
        }

        if (asset.RecordStatus == AssetRecordStatus.Dihapus)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.AssetRemoved, string.Format(AssetStockTakeMessages.AssetRemovedFormat, asset.InventoryNumber));
        }

        return (asset.Id, asset.InventoryNumber);
    }

    private async Task<AstStockTakeSession> LockSessionAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstStockTakeSessions
            .FromSqlInterpolated($"SELECT * FROM public.\"AstStockTakeSession\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw SessionNotFound();

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string RequiredCode(string? value)
    {
        var code = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code) || code.Length > MaxCodeLength)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.CodeRequired);
        }

        return code;
    }

    private static AssetCondition RequiredCondition(AssetCondition? value)
    {
        if (value is not { } condition || !ObservableConditions.Contains(condition))
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.ObservedCondition, AssetStockTakeMessages.ObservedCondition);
        }

        return condition;
    }

    private static string? OptionalText(string? value, string message)
    {
        var text = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (text is { Length: > MaxNoteLength })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, message);
        }

        return text;
    }

    private static PagedResult<T> Paged<T>(List<T> items, int total, int pageNumber, int pageSize) => new()
    {
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalData = total,
        TotalPage = (int)Math.Ceiling(total / (double)pageSize),
        Items = items
    };

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException SessionNotFound() =>
        AssetManagementException.NotFound(AssetStockTakeErrorCodes.NotFound, AssetStockTakeMessages.NotFound);
}
