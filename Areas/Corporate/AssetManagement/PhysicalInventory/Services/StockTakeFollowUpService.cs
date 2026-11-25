using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed class StockTakeFollowUpService
{
    public const string PermissionResource = "AssetStockTakeFollowUp";
    public const string TargetTransfer = "Transfer";
    public const string TargetHandover = "Handover";
    public const string TargetConditionReport = "ConditionReport";
    public const string TargetAssetDraft = "AssetDraft";
    public const string ActionProcess = "process";
    public const string ActionDismiss = "dismiss";
    private const string LogCategory = "Corporate.AssetManagement.PhysicalInventory";
    private const int MaxReasonLength = 500;
    private const int MaxNoteLength = 1000;
    private const int MaxPageSize = 200;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetTransferService _transferService;
    private readonly AssetPlacementService _placementService;
    private readonly AssetConditionService _conditionService;
    private readonly AssetRecordService _recordService;
    private readonly LoggerService _loggerService;

    public StockTakeFollowUpService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetTransferService transferService,
        AssetPlacementService placementService,
        AssetConditionService conditionService,
        AssetRecordService recordService,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
        _conditionService = conditionService ?? throw new ArgumentNullException(nameof(conditionService));
        _recordService = recordService ?? throw new ArgumentNullException(nameof(recordService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    internal async Task<int> AddForSessionAsync(AstStockTakeSession session, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var results = await _context.AstStockTakeResults
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && !x.IsDelete)
            .Select(x => new { x.Id, x.Category, x.IsConditionMismatch })
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var result in results)
        {
            foreach (var type in TypesFor(result.Category, result.IsConditionMismatch))
            {
                _context.AstStockTakeFollowUps.Add(new AstStockTakeFollowUp
                {
                    SessionId = session.Id,
                    ResultId = result.Id,
                    Type = type,
                    Status = StockTakeFollowUpStatus.Usulan,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
                count++;
            }
        }

        return count;
    }

    internal static IEnumerable<StockTakeFollowUpType> TypesFor(StockTakeResultCategory category, bool conditionMismatch) => category switch
    {
        StockTakeResultCategory.TidakDitemukan => [StockTakeFollowUpType.UsulanHilang],
        StockTakeResultCategory.BedaLokasi when conditionMismatch => [StockTakeFollowUpType.KoreksiLokasi, StockTakeFollowUpType.PerubahanKondisi],
        StockTakeResultCategory.BedaLokasi => [StockTakeFollowUpType.KoreksiLokasi],
        StockTakeResultCategory.BedaKondisi => [StockTakeFollowUpType.PerubahanKondisi],
        StockTakeResultCategory.TidakTerdaftar => [StockTakeFollowUpType.PendaftaranTemuan],
        _ => []
    };

    public async Task<PagedResult<StockTakeFollowUpResponse>> GetFollowUpsAsync(StockTakeFollowUpQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = Scoped(actor);

        if (request.SessionId is { } sessionId && sessionId != Guid.Empty)
        {
            query = query.Where(x => x.SessionId == sessionId);
        }

        var statuses = (request.Status ?? []).Distinct().ToList();
        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        var types = (request.Type ?? []).Distinct().ToList();
        if (types.Count > 0)
        {
            query = query.Where(x => types.Contains(x.Type));
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.Session!.SessionNumber.ToUpper().Contains(keyword) ||
                (x.TargetDocumentNumber != null && x.TargetDocumentNumber.ToUpper().Contains(keyword)) ||
                (x.Result!.Asset != null && ((x.Result.Asset.InventoryNumber != null && x.Result.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                                             x.Result.Asset.AssetName.ToUpper().Contains(keyword))));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var rows = await Project(query.OrderBy(x => x.Status).ThenBy(x => x.Session!.SessionNumber).ThenBy(x => x.Type).ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        var items = await DescribeAsync(rows, cancellationToken);

        return new PagedResult<StockTakeFollowUpResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<StockTakeFollowUpResponse> GetFollowUpAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var row = await Project(_context.AstStockTakeFollowUps.AsNoTracking().Where(x => x.Id == id && !x.IsDelete)).FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();
        if (!actor.CanAccessUnit(row.OrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.FollowUpOutsideScope);
        }

        return (await DescribeAsync([row], cancellationToken))[0];
    }

    internal async Task<Dictionary<Guid, List<StockTakeFollowUpBriefResponse>>> BriefsForResultsAsync(IReadOnlyCollection<Guid> resultIds, CancellationToken cancellationToken)
    {
        if (resultIds.Count == 0)
        {
            return new Dictionary<Guid, List<StockTakeFollowUpBriefResponse>>();
        }

        var rows = await _context.AstStockTakeFollowUps
            .AsNoTracking()
            .Where(x => resultIds.Contains(x.ResultId) && !x.IsDelete)
            .OrderBy(x => x.Type)
            .Select(x => new { x.ResultId, x.Id, x.Type, x.Status, x.TargetDocumentType, x.TargetDocumentId, x.TargetDocumentNumber })
            .ToListAsync(cancellationToken);
        return rows
            .GroupBy(x => x.ResultId)
            .ToDictionary(x => x.Key, x => x.Select(r => new StockTakeFollowUpBriefResponse
            {
                Id = r.Id,
                Type = r.Type,
                TypeName = AssetStockTakeLabels.FollowUpType(r.Type),
                Status = r.Status,
                StatusName = AssetStockTakeLabels.FollowUpStatus(r.Status),
                TargetDocumentType = r.TargetDocumentType,
                TargetDocumentId = r.TargetDocumentId,
                TargetDocumentNumber = r.TargetDocumentNumber
            }).ToList());
    }

    public async Task<StockTakeFollowUpResponse> ProcessAsync(Guid id, StockTakeFollowUpProcessRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var row = await Project(_context.AstStockTakeFollowUps.AsNoTracking().Where(x => x.Id == id && !x.IsDelete)).FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();
        if (!actor.CanAccessUnit(row.OrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.FollowUpOutsideScope);
        }

        EnsureOpen(row.Status);
        var notes = OptionalNote(request.Notes);
        var plan = await PlanAsync(row, request, cancellationToken);
        row.Draft = request.Draft;

        await ClaimAsync(id, actor.UserId, cancellationToken);
        (Guid Id, string? Number) target;
        try
        {
            target = await CreateTargetAsync(row, plan, notes, actor.UserId, cancellationToken);
        }
        catch
        {
            await ReleaseClaimAsync(id, cancellationToken);
            throw;
        }

        await InTransactionAsync(async () =>
        {
            var followUp = await LockAsync(id, cancellationToken);
            followUp.TargetDocumentType = plan.TargetType;
            followUp.TargetDocumentId = target.Id;
            followUp.TargetDocumentNumber = target.Number;
            followUp.UpdateBy = actor.UserId;
            followUp.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

        await AuditAsync("AssetStockTakeFollowUp.Process",
            $"Usulan {AssetStockTakeLabels.FollowUpType(row.Type)} sesi {row.SessionNumber} diproses oleh {actor.UserId} menjadi {plan.TargetType} {target.Number ?? target.Id.ToString()}.");
        return await GetFollowUpAsync(id, cancellationToken);
    }

    public async Task<StockTakeFollowUpResponse> DismissAsync(Guid id, StockTakeDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Reason?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.ReasonRequired, AssetStockTakeMessages.ReasonRequired);
        }

        var sessionNumber = await InTransactionAsync(async () =>
        {
            var followUp = await LockAsync(id, cancellationToken);
            var scope = await _context.AstStockTakeSessions.AsNoTracking()
                .Where(x => x.Id == followUp.SessionId)
                .Select(x => new { x.OrganizationUnitId, x.SessionNumber })
                .FirstAsync(cancellationToken);
            if (!actor.CanAccessUnit(scope.OrganizationUnitId))
            {
                throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.FollowUpOutsideScope);
            }

            EnsureOpen(followUp.Status);
            var now = DateTime.UtcNow;
            followUp.Status = StockTakeFollowUpStatus.Diabaikan;
            followUp.DismissedBy = actor.UserId;
            followUp.DismissedAt = now;
            followUp.DismissReason = reason;
            followUp.UpdateBy = actor.UserId;
            followUp.UpdateDateTime = now;
            await _context.SaveChangesAsync(cancellationToken);
            return scope.SessionNumber;
        }, cancellationToken);

        await AuditAsync("AssetStockTakeFollowUp.Dismiss", $"Usulan {id} sesi {sessionNumber} diabaikan oleh {actor.UserId}: {reason}");
        return await GetFollowUpAsync(id, cancellationToken);
    }

    private async Task<ProcessPlan> PlanAsync(FollowUpRow row, StockTakeFollowUpProcessRequest request, CancellationToken cancellationToken)
    {
        if (row.Type == StockTakeFollowUpType.PendaftaranTemuan)
        {
            var draft = request.Draft;
            if (draft == null || string.IsNullOrWhiteSpace(draft.AssetName) || draft.AssetGroupId is null || draft.OwnershipTypeId is null ||
                draft.AcquisitionType is null || draft.AcquisitionDate is null || draft.ReadyForUseDate is null)
            {
                throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.FollowUpDraftRequired);
            }

            return new ProcessPlan(TargetAssetDraft, null, null);
        }

        if (row.AssetId is not { } assetId)
        {
            throw NotFound();
        }

        var asset = await _context.AstAssets
            .AsNoTracking()
            .Where(x => x.Id == assetId && !x.IsDelete)
            .Select(x => new { x.InventoryNumber, x.AssetName, x.RecordStatus, x.ActiveProcessKind, x.CurrentOrganizationUnitId, x.CurrentCustodianEmployeeId })
            .FirstOrDefaultAsync(cancellationToken);
        var label = asset?.InventoryNumber ?? row.InventoryNumber ?? "-";
        if (asset == null || asset.RecordStatus == AssetRecordStatus.Dihapus)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.FollowUpBlocked, string.Format(AssetStockTakeMessages.FollowUpAssetRemovedFormat, label));
        }

        if (asset.ActiveProcessKind != AssetActiveProcessKind.Tidak)
        {
            throw AssetManagementException.Conflict(
                AssetStockTakeErrorCodes.FollowUpBlocked,
                string.Format(AssetStockTakeMessages.FollowUpBlockedFormat, label, AssetProcessLockService.ProcessName(asset.ActiveProcessKind).ToLowerInvariant()));
        }

        if (row.Type != StockTakeFollowUpType.KoreksiLokasi)
        {
            return new ProcessPlan(TargetConditionReport, null, null);
        }

        if (row.FoundLocationId is not { } locationId || row.FoundOrganizationUnitId is not { } foundUnitId)
        {
            throw NotFound();
        }

        if (asset.CurrentOrganizationUnitId != foundUnitId)
        {
            return new ProcessPlan(TargetTransfer, foundUnitId, null);
        }

        var custodian = request.TargetCustodianEmployeeId is { } chosen && chosen != Guid.Empty ? chosen : asset.CurrentCustodianEmployeeId;
        if (custodian is not { } custodianId)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.FollowUpCustodianRequired);
        }

        return new ProcessPlan(TargetHandover, foundUnitId, custodianId);
    }

    private async Task<(Guid Id, string? Number)> CreateTargetAsync(FollowUpRow row, ProcessPlan plan, string? notes, Guid actorUserId, CancellationToken cancellationToken)
    {
        var reason = $"Koreksi hasil opname {row.SessionNumber}: {notes ?? row.TeamNote ?? AssetStockTakeLabels.Category(row.Category)}";
        if (reason.Length > MaxNoteLength)
        {
            reason = reason[..MaxNoteLength];
        }

        switch (plan.TargetType)
        {
            case TargetTransfer:
            {
                var transferId = await _transferService.CreateFromStockTakeAsync(
                    new AssetTransferStockTakeRequest(row.AssetId!.Value, plan.TargetUnitId!.Value, row.FoundLocationId, null, row.ResultId, reason, actorUserId),
                    cancellationToken);
                var number = await _context.AstTransfers.AsNoTracking().Where(x => x.Id == transferId).Select(x => x.TransferNumber).FirstAsync(cancellationToken);
                return (transferId, number);
            }
            case TargetHandover:
                return await _placementService.CreateStockTakeCorrectionAsync(
                    new AssetHandoverStockTakeRequest(row.AssetId!.Value, row.FoundLocationId!.Value, plan.CustodianEmployeeId!.Value, row.Id, reason, actorUserId),
                    cancellationToken);
            case TargetConditionReport:
            {
                var condition = row.Type == StockTakeFollowUpType.UsulanHilang ? AssetCondition.Hilang : row.ObservedCondition ?? AssetCondition.Aktif;
                var report = await _conditionService.OpenReportAsync(
                    new AssetConditionOpenRequest(row.AssetId!.Value, ConditionReportSource.TindakLanjutOpname, condition, row.Id, reason, actorUserId),
                    cancellationToken);
                return (report.Id, report.ReportNumber);
            }
            default:
            {
                var draft = row.Draft;
                var description = $"Temuan opname {row.SessionNumber}{(string.IsNullOrWhiteSpace(row.ScannedCode) ? string.Empty : $" (nomor terbaca {row.ScannedCode})")}: {row.FindingDescription}";
                var created = await _recordService.CreateDraftsAsync(new AssetDraftCreateRequest
                {
                    Quantity = 1,
                    AssetName = draft!.AssetName,
                    Description = description.Length > MaxNoteLength ? description[..MaxNoteLength] : description,
                    AssetGroupId = draft.AssetGroupId,
                    OwnershipTypeId = draft.OwnershipTypeId,
                    Brand = draft.Brand,
                    ModelType = draft.ModelType,
                    SerialNumber = draft.SerialNumber,
                    AcquisitionType = draft.AcquisitionType,
                    AcquisitionDate = draft.AcquisitionDate,
                    ReadyForUseDate = draft.ReadyForUseDate,
                    RegisteredAcquisitionCost = draft.RegisteredAcquisitionCost
                }, cancellationToken);
                return (created[0].Id, null);
            }
        }
    }

    private async Task ClaimAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken) =>
        await InTransactionAsync(async () =>
        {
            var followUp = await LockAsync(id, cancellationToken);
            EnsureOpen(followUp.Status);
            var now = DateTime.UtcNow;
            followUp.Status = StockTakeFollowUpStatus.Diproses;
            followUp.ProcessedBy = actorUserId;
            followUp.ProcessedAt = now;
            followUp.UpdateBy = actorUserId;
            followUp.UpdateDateTime = now;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

    private async Task ReleaseClaimAsync(Guid id, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        await InTransactionAsync(async () =>
        {
            var followUp = await LockAsync(id, cancellationToken);
            if (followUp.Status == StockTakeFollowUpStatus.Diproses && followUp.TargetDocumentId == null)
            {
                followUp.Status = StockTakeFollowUpStatus.Usulan;
                followUp.ProcessedBy = null;
                followUp.ProcessedAt = null;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }, cancellationToken);
    }

    private IQueryable<AstStockTakeFollowUp> Scoped(AssetActor actor)
    {
        var query = _context.AstStockTakeFollowUps.AsNoTracking().Where(x => !x.IsDelete);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        return query.Where(x => unitIds.Contains(x.Session!.OrganizationUnitId));
    }

    private static IQueryable<FollowUpRow> Project(IQueryable<AstStockTakeFollowUp> query) =>
        query.Select(x => new FollowUpRow
        {
            Id = x.Id,
            SessionId = x.SessionId,
            SessionNumber = x.Session!.SessionNumber,
            OrganizationUnitId = x.Session.OrganizationUnitId,
            OrganizationUnitName = x.Session.OrganizationUnit != null ? x.Session.OrganizationUnit.UnitName : null,
            ResultId = x.ResultId,
            Category = x.Result!.Category,
            Type = x.Type,
            Status = x.Status,
            AssetId = x.Result.AssetId,
            InventoryNumber = x.Result.Asset != null ? x.Result.Asset.InventoryNumber : null,
            AssetName = x.Result.Asset != null ? x.Result.Asset.AssetName : null,
            AssetOrganizationUnitId = x.Result.Asset != null ? x.Result.Asset.CurrentOrganizationUnitId : null,
            IsForeignAsset = x.Result.IsForeignAsset,
            ExpectedLocationName = x.Result.ExpectedLocation != null ? x.Result.ExpectedLocation.LocationName : null,
            FoundLocationId = x.Result.FoundLocationId,
            FoundLocationName = x.Result.FoundLocation != null ? x.Result.FoundLocation.LocationName : null,
            FoundOrganizationUnitId = x.Result.FoundLocation != null ? x.Result.FoundLocation.OrganizationUnitId : null,
            FoundOrganizationUnitName = x.Result.FoundLocation != null && x.Result.FoundLocation.OrganizationUnit != null ? x.Result.FoundLocation.OrganizationUnit.UnitName : null,
            ExpectedCondition = x.Result.ExpectedCondition,
            ObservedCondition = x.Result.ObservedCondition,
            TeamNote = x.Result.TeamNote,
            FindingDescription = x.Result.Scan != null ? x.Result.Scan.FindingDescription : null,
            ScannedCode = x.Result.Scan != null ? x.Result.Scan.ScannedCode : null,
            PhotoAttachmentId = x.Result.Scan != null ? x.Result.Scan.PhotoAttachmentId : null,
            TargetDocumentType = x.TargetDocumentType,
            TargetDocumentId = x.TargetDocumentId,
            TargetDocumentNumber = x.TargetDocumentNumber,
            ProcessedBy = x.ProcessedBy,
            ProcessedAt = x.ProcessedAt,
            DismissedBy = x.DismissedBy,
            DismissedAt = x.DismissedAt,
            DismissReason = x.DismissReason
        });

    private async Task<List<StockTakeFollowUpResponse>> DescribeAsync(List<FollowUpRow> rows, CancellationToken cancellationToken)
    {
        var userIds = rows.SelectMany(x => new[] { x.ProcessedBy, x.DismissedBy }).OfType<Guid>().Distinct().ToList();
        var names = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _context.Users.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

        StockTakeActorResponse? Actor(Guid? userId, DateTime? at) =>
            userId is { } id && at is { } when ? new StockTakeActorResponse { UserId = id, At = when, Name = names.GetValueOrDefault(id) } : null;

        return rows.Select(row => new StockTakeFollowUpResponse
        {
            Id = row.Id,
            SessionId = row.SessionId,
            SessionNumber = row.SessionNumber,
            OrganizationUnitId = row.OrganizationUnitId,
            OrganizationUnitName = row.OrganizationUnitName,
            ResultId = row.ResultId,
            Category = row.Category,
            CategoryName = AssetStockTakeLabels.Category(row.Category),
            Type = row.Type,
            TypeName = AssetStockTakeLabels.FollowUpType(row.Type),
            Status = row.Status,
            StatusName = AssetStockTakeLabels.FollowUpStatus(row.Status),
            AssetId = row.AssetId,
            InventoryNumber = row.InventoryNumber,
            AssetName = row.AssetName,
            IsForeignAsset = row.IsForeignAsset,
            ExpectedLocationName = row.ExpectedLocationName,
            FoundLocationId = row.FoundLocationId,
            FoundLocationName = row.FoundLocationName,
            FoundOrganizationUnitName = row.FoundOrganizationUnitName,
            ExpectedCondition = row.ExpectedCondition,
            ExpectedConditionName = row.ExpectedCondition is { } expected ? AssetRegisterLabels.Condition(expected) : null,
            ObservedCondition = row.ObservedCondition,
            ObservedConditionName = row.ObservedCondition is { } observed ? AssetRegisterLabels.Condition(observed) : null,
            TeamNote = row.TeamNote,
            FindingDescription = row.FindingDescription,
            ScannedCode = row.ScannedCode,
            PhotoAttachmentId = row.PhotoAttachmentId,
            ProposedAction = ProposedAction(row),
            TargetDocumentType = row.TargetDocumentType,
            TargetDocumentId = row.TargetDocumentId,
            TargetDocumentNumber = row.TargetDocumentNumber,
            Processed = Actor(row.ProcessedBy, row.ProcessedAt),
            Dismissed = Actor(row.DismissedBy, row.DismissedAt),
            DismissReason = row.DismissReason,
            AvailableActions = row.Status == StockTakeFollowUpStatus.Usulan ? [ActionProcess, ActionDismiss] : []
        }).ToList();
    }

    private static string ProposedAction(FollowUpRow row) => row.Type switch
    {
        StockTakeFollowUpType.KoreksiLokasi when row.FoundOrganizationUnitId != row.AssetOrganizationUnitId =>
            $"Mutasi koreksi opname ke {row.FoundOrganizationUnitName ?? "-"} / {row.FoundLocationName ?? "-"}",
        StockTakeFollowUpType.KoreksiLokasi => $"Penyerahan koreksi opname ke ruangan {row.FoundLocationName ?? "-"}",
        StockTakeFollowUpType.UsulanHilang => "Laporan kondisi Hilang (tetap butuh berita acara kehilangan)",
        StockTakeFollowUpType.PerubahanKondisi => $"Laporan kondisi {(row.ObservedCondition is { } observed ? AssetRegisterLabels.Condition(observed) : "-")}",
        StockTakeFollowUpType.PendaftaranTemuan => "Draft aset baru dari temuan",
        _ => "-"
    };

    private static void EnsureOpen(StockTakeFollowUpStatus status)
    {
        if (status != StockTakeFollowUpStatus.Usulan)
        {
            throw AssetManagementException.Conflict(
                AssetStockTakeErrorCodes.FollowUpClosed,
                string.Format(AssetStockTakeMessages.FollowUpClosedFormat, AssetStockTakeLabels.FollowUpStatus(status).ToLowerInvariant()));
        }
    }

    private static string? OptionalNote(string? value)
    {
        var note = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (note is { Length: > MaxReasonLength })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.NotesLength500);
        }

        return note;
    }

    private async Task<AstStockTakeFollowUp> LockAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstStockTakeFollowUps
            .FromSqlInterpolated($"SELECT * FROM public.\"AstStockTakeFollowUp\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw NotFound();

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

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException NotFound() =>
        AssetManagementException.NotFound(AssetStockTakeErrorCodes.FollowUpNotFound, AssetStockTakeMessages.FollowUpNotFound);

    private sealed record ProcessPlan(string TargetType, Guid? TargetUnitId, Guid? CustodianEmployeeId);

    private sealed class FollowUpRow
    {
        public Guid Id { get; init; }
        public Guid SessionId { get; init; }
        public string? SessionNumber { get; init; }
        public Guid OrganizationUnitId { get; init; }
        public string? OrganizationUnitName { get; init; }
        public Guid ResultId { get; init; }
        public StockTakeResultCategory Category { get; init; }
        public StockTakeFollowUpType Type { get; init; }
        public StockTakeFollowUpStatus Status { get; init; }
        public Guid? AssetId { get; init; }
        public string? InventoryNumber { get; init; }
        public string? AssetName { get; init; }
        public Guid? AssetOrganizationUnitId { get; init; }
        public bool IsForeignAsset { get; init; }
        public string? ExpectedLocationName { get; init; }
        public Guid? FoundLocationId { get; init; }
        public string? FoundLocationName { get; init; }
        public Guid? FoundOrganizationUnitId { get; init; }
        public string? FoundOrganizationUnitName { get; init; }
        public AssetCondition? ExpectedCondition { get; init; }
        public AssetCondition? ObservedCondition { get; init; }
        public string? TeamNote { get; init; }
        public string? FindingDescription { get; init; }
        public string? ScannedCode { get; init; }
        public Guid? PhotoAttachmentId { get; init; }
        public string? TargetDocumentType { get; init; }
        public Guid? TargetDocumentId { get; init; }
        public string? TargetDocumentNumber { get; init; }
        public Guid? ProcessedBy { get; init; }
        public DateTime? ProcessedAt { get; init; }
        public Guid? DismissedBy { get; init; }
        public DateTime? DismissedAt { get; init; }
        public string? DismissReason { get; init; }
        public StockTakeFindingDraftRequest? Draft { get; set; }
    }
}
