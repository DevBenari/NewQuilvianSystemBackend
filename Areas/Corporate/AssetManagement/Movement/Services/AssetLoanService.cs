using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed partial class AssetLoanService
{
    public const string LoanDocumentType = "AstLoan";
    private const string LogCategory = "Corporate.AssetManagement.Movement";
    private const string OpenExtensionIndexName = "IX_AstLoanExtension_OpenPerLoan";
    private const int MaxItems = 200;
    private const int MaxPurposeLength = 1000;
    private const int MaxReasonLength = 500;
    private const int MaxPageSize = 100;

    private static readonly AssetCondition[] ReturnableConditions =
        [AssetCondition.Aktif, AssetCondition.RusakRingan, AssetCondition.RusakBerat, AssetCondition.TidakDigunakan];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetProcessLockService _lockService;
    private readonly AssetStockTakeGate _stockTakeGate;
    private readonly AssetHistoryService _historyService;
    private readonly AssetConditionService _conditionService;
    private readonly AssetMovementDocumentService _documentService;
    private readonly LoggerService _loggerService;

    public AssetLoanService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetProcessLockService lockService,
        AssetStockTakeGate stockTakeGate,
        AssetHistoryService historyService,
        AssetConditionService conditionService,
        AssetMovementDocumentService documentService,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        _stockTakeGate = stockTakeGate ?? throw new ArgumentNullException(nameof(stockTakeGate));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _conditionService = conditionService ?? throw new ArgumentNullException(nameof(conditionService));
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetLoanResponse> CreateAsync(AssetLoanCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var purpose = request.Purpose?.Trim();
        if (string.IsNullOrEmpty(purpose) || purpose.Length > MaxPurposeLength)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.LoanPurpose);
        }

        var (startDate, returnDate) = RequireLoanDates(request.StartDate, request.PlannedReturnDate);
        var assetIds = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (assetIds.Count is < 1 or > MaxItems)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ItemCount, AssetMovementMessages.ItemCount);
        }

        if (request.BorrowerOrganizationUnitId is not { } borrowerUnitId || borrowerUnitId == Guid.Empty ||
            !await _context.MstOrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == borrowerUnitId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.BorrowerUnitRequired);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var created = await InTransactionAsync(async () =>
        {
            var assets = await LockAssetsAsync(assetIds, entity.LegalEntityId, cancellationToken);
            var ownerUnitId = EnsureEligibleAndSingleOwner(assets, borrowerUnitId);
            if (!IsParty(actor, ownerUnitId, borrowerUnitId))
            {
                throw AssetManagementException.Forbidden(AssetMovementErrorCodes.OutsideScope, AssetMovementMessages.LoanCreateScope);
            }

            await _stockTakeGate.EnsureUnitsNotInStockTakeAsync([ownerUnitId, borrowerUnitId], cancellationToken);

            var now = DateTime.UtcNow;
            var autoApproval = actor.IsHeadOfUnit(ownerUnitId);
            var loan = new AstLoan
            {
                LoanNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Loan, actor.UserId, cancellationToken),
                OwnerOrganizationUnitId = ownerUnitId,
                BorrowerOrganizationUnitId = borrowerUnitId,
                Purpose = purpose,
                StartDate = startDate,
                PlannedReturnDate = returnDate,
                OriginalPlannedReturnDate = returnDate,
                Status = autoApproval ? LoanStatus.Disetujui : LoanStatus.Diajukan,
                RequestedBy = actor.UserId,
                RequestedAt = now,
                ApprovedBy = autoApproval ? actor.UserId : null,
                ApprovedAt = autoApproval ? now : null,
                IsApprovalByRequester = autoApproval,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            foreach (var asset in assets)
            {
                _lockService.Acquire(asset, AssetActiveProcessKind.Peminjaman, loan.Id);
                TouchAsset(asset, actor.UserId, now);
                loan.Items.Add(new AstLoanItem
                {
                    LoanId = loan.Id,
                    AssetId = asset.Id,
                    ConditionAtPickup = asset.Condition,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            _context.AstLoans.Add(loan);
            await _context.SaveChangesAsync(cancellationToken);
            return (loan.Id, loan.LoanNumber, autoApproval);
        }, cancellationToken);

        await AuditAsync("AssetLoan.Create", $"Pinjaman {created.LoanNumber} ({assetIds.Count} aset) diajukan oleh {actor.UserId}{(created.autoApproval ? ", disetujui langsung oleh Kepala Unit pemilik" : string.Empty)}.");
        return await GetLoanAsync(created.Id, cancellationToken);
    }

    public async Task<AssetLoanResponse> DecideAsync(Guid id, AssetLoanDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Approve ? OptionalReason(request.Reason) : RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            EnsureStatus(loan, LoanStatus.Diajukan);
            await EnsureOwnerHeadAsync(loan, actor, AssetMovementMessages.UnitApproverFormat, cancellationToken);
            EnsureNotRequester(loan.RequestedBy, actor);

            var now = DateTime.UtcNow;
            if (request.Approve)
            {
                loan.Status = LoanStatus.Disetujui;
                loan.ApprovedBy = actor.UserId;
                loan.ApprovedAt = now;
                loan.DecisionReason = reason;
            }
            else
            {
                await ReleaseLocksAsync(loan, actor.UserId, now, cancellationToken);
                loan.Status = LoanStatus.Ditolak;
                loan.RejectedBy = actor.UserId;
                loan.RejectedAt = now;
                loan.DecisionReason = reason;
            }

            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return loan.LoanNumber;
        }, cancellationToken);

        await AuditAsync(request.Approve ? "AssetLoan.Approve" : "AssetLoan.Reject", $"Pinjaman {number} {(request.Approve ? "disetujui" : "ditolak")} Kepala Unit pemilik {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> PickupAsync(Guid id, AssetLoanStepRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            EnsureStatus(loan, LoanStatus.Disetujui);
            EnsureBorrowerHead(loan, actor);

            var now = DateTime.UtcNow;
            var items = await _context.AstLoanItems.Where(x => x.LoanId == loan.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var assets = await LockAssetsAsync(items.Select(x => x.AssetId).ToList(), null, cancellationToken);
            var ownerName = await UnitNameAsync(loan.OwnerOrganizationUnitId, cancellationToken);
            var borrowerName = await UnitNameAsync(loan.BorrowerOrganizationUnitId, cancellationToken);

            foreach (var asset in assets)
            {
                var item = items.First(x => x.AssetId == asset.Id);
                item.ConditionAtPickup = asset.Condition;
                item.UpdateBy = actor.UserId;
                item.UpdateDateTime = now;
                TouchAsset(asset, actor.UserId, now);
                _historyService.Append(new AssetHistoryEntry(
                    asset.Id,
                    AssetHistoryEventType.Dipinjam,
                    AssetSubModules.Movement,
                    actor.UserId,
                    FromValue: ownerName,
                    ToValue: borrowerName,
                    SourceDocumentType: LoanDocumentType,
                    SourceDocumentId: loan.Id,
                    SourceDocumentNumber: loan.LoanNumber,
                    Reason: $"{loan.StartDate:dd-MM-yyyy} s.d. {loan.PlannedReturnDate:dd-MM-yyyy}",
                    OccurredAt: now));
            }

            loan.Status = LoanStatus.Dipinjam;
            loan.PickedUpBy = actor.UserId;
            loan.PickedUpAt = now;
            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return loan.LoanNumber;
        }, cancellationToken);

        await AuditAsync("AssetLoan.Pickup", $"Aset pinjaman {number} diambil Kepala Unit peminjam {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> ReturnAsync(Guid id, AssetLoanStepRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            EnsureStatus(loan, LoanStatus.Dipinjam);
            EnsureBorrowerHead(loan, actor);

            var now = DateTime.UtcNow;
            loan.Status = LoanStatus.MenungguKonfirmasiKembali;
            loan.ReturnedBy = actor.UserId;
            loan.ReturnedAt = now;
            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return loan.LoanNumber;
        }, cancellationToken);

        await AuditAsync("AssetLoan.Return", $"Aset pinjaman {number} diserahkan kembali oleh Kepala Unit peminjam {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> ConfirmReturnAsync(Guid id, AssetLoanReturnRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var result = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            EnsureStatus(loan, LoanStatus.MenungguKonfirmasiKembali);
            await EnsureOwnerHeadAsync(loan, actor, AssetMovementMessages.OwnerConfirmFormat, cancellationToken);

            var items = await _context.AstLoanItems.Where(x => x.LoanId == loan.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var conditions = ReturnConditions(request.Items, items);
            var assets = await LockAssetsAsync(items.Select(x => x.AssetId).ToList(), null, cancellationToken);
            var ownerName = await UnitNameAsync(loan.OwnerOrganizationUnitId, cancellationToken);
            var borrowerName = await UnitNameAsync(loan.BorrowerOrganizationUnitId, cancellationToken);
            var now = DateTime.UtcNow;
            var changed = new List<(Guid AssetId, AssetCondition Condition)>();

            foreach (var asset in assets)
            {
                var item = items.First(x => x.AssetId == asset.Id);
                var condition = conditions[asset.Id];
                item.ReturnCondition = condition;
                item.UpdateBy = actor.UserId;
                item.UpdateDateTime = now;
                if (condition != asset.Condition)
                {
                    changed.Add((asset.Id, condition));
                }

                _lockService.Release(asset, AssetActiveProcessKind.Peminjaman, loan.Id);
                TouchAsset(asset, actor.UserId, now);
                _historyService.Append(new AssetHistoryEntry(
                    asset.Id,
                    AssetHistoryEventType.Dikembalikan,
                    AssetSubModules.Movement,
                    actor.UserId,
                    FromValue: borrowerName,
                    ToValue: ownerName,
                    SourceDocumentType: LoanDocumentType,
                    SourceDocumentId: loan.Id,
                    SourceDocumentNumber: loan.LoanNumber,
                    Reason: $"Kondisi kembali {AssetRegisterLabels.Condition(condition)}",
                    OccurredAt: now));
            }

            var openExtensions = await _context.AstLoanExtensions
                .Where(x => x.LoanId == loan.Id && !x.IsDelete && x.Status == LoanExtensionStatus.Diajukan)
                .ToListAsync(cancellationToken);
            foreach (var extension in openExtensions)
            {
                extension.Status = LoanExtensionStatus.Ditolak;
                extension.DecidedBy = actor.UserId;
                extension.DecidedAt = now;
                extension.DecisionReason = AssetMovementMessages.ExtensionClosedOnReturn;
                extension.UpdateBy = actor.UserId;
                extension.UpdateDateTime = now;
            }

            loan.Status = LoanStatus.Dikembalikan;
            loan.ReturnConfirmedBy = actor.UserId;
            loan.ReturnConfirmedAt = now;
            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);

            var reports = new List<string>();
            foreach (var (assetId, condition) in changed)
            {
                var report = await _conditionService.OpenReportAsync(
                    new AssetConditionOpenRequest(
                        assetId,
                        ConditionReportSource.PengembalianPinjaman,
                        condition,
                        loan.Id,
                        $"Kondisi saat kembali dari pinjaman {loan.LoanNumber} berbeda dari kondisi tercatat.",
                        actor.UserId),
                    cancellationToken);
                reports.Add(report.ReportNumber);
            }

            return (loan.LoanNumber, Reports: reports);
        }, cancellationToken);

        await AuditAsync("AssetLoan.ConfirmReturn", $"Pengembalian pinjaman {result.LoanNumber} dikonfirmasi {actor.UserId}; laporan kondisi dibuka: {(result.Reports.Count == 0 ? "-" : string.Join(", ", result.Reports))}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> RequestExtensionAsync(Guid id, AssetLoanExtensionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);
        if (request.RequestedReturnDate is not { } requestedDate)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.LoanDate, AssetMovementMessages.ExtensionDate);
        }

        var number = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            EnsureStatus(loan, LoanStatus.Dipinjam);
            EnsureBorrowerHead(loan, actor);
            if (requestedDate <= loan.PlannedReturnDate)
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.LoanDate, AssetMovementMessages.ExtensionDate);
            }

            if (await _context.AstLoanExtensions.AnyAsync(x => x.LoanId == loan.Id && !x.IsDelete && x.Status == LoanExtensionStatus.Diajukan, cancellationToken))
            {
                throw OpenExtensionConflict();
            }

            var now = DateTime.UtcNow;
            _context.AstLoanExtensions.Add(new AstLoanExtension
            {
                LoanId = loan.Id,
                PreviousReturnDate = loan.PlannedReturnDate,
                RequestedReturnDate = requestedDate,
                Reason = reason,
                Status = LoanExtensionStatus.Diajukan,
                RequestedBy = actor.UserId,
                RequestedAt = now,
                CreateBy = actor.UserId,
                CreateDateTime = now
            });
            Touch(loan, actor.UserId, now);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: OpenExtensionIndexName })
            {
                throw OpenExtensionConflict(exception);
            }

            return loan.LoanNumber;
        }, cancellationToken);

        await AuditAsync("AssetLoan.Extend", $"Perpanjangan pinjaman {number} sampai {requestedDate:yyyy-MM-dd} diminta {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> DecideExtensionAsync(Guid id, Guid extensionId, AssetLoanDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Approve ? OptionalReason(request.Reason) : RequiredReason(request.Reason);

        var result = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            if (loan.Status is not (LoanStatus.Dipinjam or LoanStatus.MenungguKonfirmasiKembali))
            {
                throw StatusConflict(AssetMovementLabels.LoanStatus(loan.Status));
            }

            await EnsureOwnerHeadAsync(loan, actor, AssetMovementMessages.UnitApproverFormat, cancellationToken);
            var extension = await _context.AstLoanExtensions.FirstOrDefaultAsync(x => x.Id == extensionId && x.LoanId == loan.Id && !x.IsDelete, cancellationToken)
                ?? throw AssetManagementException.NotFound(AssetMovementErrorCodes.NotFound, AssetMovementMessages.ExtensionNotFound);
            if (extension.Status != LoanExtensionStatus.Diajukan)
            {
                throw StatusConflict(AssetMovementLabels.LoanExtensionStatus(extension.Status));
            }

            EnsureNotRequester(extension.RequestedBy ?? Guid.Empty, actor);

            var now = DateTime.UtcNow;
            extension.Status = request.Approve ? LoanExtensionStatus.Disetujui : LoanExtensionStatus.Ditolak;
            extension.DecidedBy = actor.UserId;
            extension.DecidedAt = now;
            extension.DecisionReason = reason;
            extension.UpdateBy = actor.UserId;
            extension.UpdateDateTime = now;
            if (request.Approve)
            {
                loan.PlannedReturnDate = extension.RequestedReturnDate;
            }

            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return (loan.LoanNumber, extension.RequestedReturnDate);
        }, cancellationToken);

        await AuditAsync(
            request.Approve ? "AssetLoan.ApproveExtension" : "AssetLoan.RejectExtension",
            $"Perpanjangan pinjaman {result.LoanNumber} sampai {result.RequestedReturnDate:yyyy-MM-dd} {(request.Approve ? "disetujui" : "ditolak")} {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    public async Task<AssetLoanResponse> CancelAsync(Guid id, AssetLoanReasonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var loan = await LockLoanAsync(id, cancellationToken);
            EnsureVersion(loan, request.RowVersion);
            if (loan.Status is not (LoanStatus.Diajukan or LoanStatus.Disetujui))
            {
                throw StatusConflict(AssetMovementLabels.LoanStatus(loan.Status));
            }

            if (!CanCancel(loan.RequestedBy, loan.OwnerOrganizationUnitId, actor))
            {
                throw AssetManagementException.Forbidden(AssetMovementErrorCodes.OutsideScope, AssetMovementMessages.LoanCancelScope);
            }

            var now = DateTime.UtcNow;
            await ReleaseLocksAsync(loan, actor.UserId, now, cancellationToken);
            loan.Status = LoanStatus.Dibatalkan;
            loan.CancelledBy = actor.UserId;
            loan.CancelledAt = now;
            loan.DecisionReason = reason;
            Touch(loan, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return loan.LoanNumber;
        }, cancellationToken);

        await AuditAsync("AssetLoan.Cancel", $"Pinjaman {number} dibatalkan oleh {actor.UserId}.");
        return await GetLoanAsync(id, cancellationToken);
    }

    private async Task ReleaseLocksAsync(AstLoan loan, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var assetIds = await _context.AstLoanItems
            .AsNoTracking()
            .Where(x => x.LoanId == loan.Id && !x.IsDelete)
            .Select(x => x.AssetId)
            .ToListAsync(cancellationToken);
        var assets = await LockAssetsAsync(assetIds, null, cancellationToken);
        foreach (var asset in assets)
        {
            _lockService.Release(asset, AssetActiveProcessKind.Peminjaman, loan.Id);
            TouchAsset(asset, actorUserId, now);
        }
    }

    private static (DateOnly Start, DateOnly Return) RequireLoanDates(DateOnly? start, DateOnly? plannedReturn)
    {
        if (start is not { } startDate || plannedReturn is not { } returnDate)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.LoanDate, AssetMovementMessages.LoanDateRequired);
        }

        if (returnDate < startDate)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.LoanDate, AssetMovementMessages.LoanReturnBeforeStart);
        }

        if (startDate < AssetRegisterSettingReader.Today().AddDays(-1))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.LoanDate, AssetMovementMessages.LoanStartTooEarly);
        }

        return (startDate, returnDate);
    }

    private static Guid EnsureEligibleAndSingleOwner(List<AstAsset> assets, Guid borrowerUnitId)
    {
        foreach (var asset in assets)
        {
            var label = asset.InventoryNumber ?? asset.AssetName;
            if (asset.RecordStatus != AssetRecordStatus.Terdaftar || asset.PlacementState != AssetPlacementState.Ditempatkan || !asset.CurrentOrganizationUnitId.HasValue)
            {
                throw AssetManagementException.Conflict(AssetMovementErrorCodes.AssetNotPlaced, string.Format(AssetMovementMessages.AssetNotPlacedFormat, label));
            }

            if (asset.Condition is AssetCondition.Hilang or AssetCondition.RusakBerat)
            {
                throw AssetManagementException.Conflict(
                    AssetMovementErrorCodes.AssetCondition,
                    string.Format(AssetMovementMessages.AssetConditionFormat, label, AssetRegisterLabels.Condition(asset.Condition)));
            }

            if (AssetProcessLockService.IsLocked(asset))
            {
                throw AssetManagementException.Conflict(
                    AssetMovementErrorCodes.ProcessLocked,
                    string.Format(AssetMovementMessages.ProcessLockedFormat, label, AssetProcessLockService.ProcessName(asset.ActiveProcessKind).ToLowerInvariant()));
            }
        }

        var ownerUnits = assets.Select(x => x.CurrentOrganizationUnitId!.Value).Distinct().ToList();
        if (ownerUnits.Count != 1)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.SingleSourceUnit, AssetMovementMessages.LoanSingleOwnerUnit);
        }

        if (ownerUnits[0] == borrowerUnitId)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.SameUnit, AssetMovementMessages.LoanSameUnit);
        }

        return ownerUnits[0];
    }

    private static Dictionary<Guid, AssetCondition> ReturnConditions(List<AssetLoanReturnItemRequest>? requested, List<AstLoanItem> items)
    {
        var result = new Dictionary<Guid, AssetCondition>();
        foreach (var row in requested ?? [])
        {
            if (!items.Any(x => x.AssetId == row.AssetId))
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.ReturnItems);
            }

            if (row.ReturnCondition is not { } condition)
            {
                continue;
            }

            if (!ReturnableConditions.Contains(condition))
            {
                throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.ReturnConditionValue);
            }

            result[row.AssetId] = condition;
        }

        if (items.Any(x => !result.ContainsKey(x.AssetId)))
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReturnCondition, AssetMovementMessages.ReturnConditionRequired);
        }

        return result;
    }

    private async Task EnsureOwnerHeadAsync(AstLoan loan, AssetActor actor, string messageFormat, CancellationToken cancellationToken)
    {
        if (!actor.IsHeadOfUnit(loan.OwnerOrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(
                AssetMovementErrorCodes.UnitApprover,
                string.Format(messageFormat, await UnitNameAsync(loan.OwnerOrganizationUnitId, cancellationToken)));
        }
    }

    private static void EnsureBorrowerHead(AstLoan loan, AssetActor actor)
    {
        if (!actor.IsHeadOfUnit(loan.BorrowerOrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(AssetMovementErrorCodes.Borrower, AssetMovementMessages.Borrower);
        }
    }

    private static void EnsureNotRequester(Guid requestedBy, AssetActor actor)
    {
        if (requestedBy == actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetMovementErrorCodes.SelfApproval, AssetMovementMessages.SelfApproval);
        }
    }

    private static void EnsureStatus(AstLoan loan, LoanStatus expected)
    {
        if (loan.Status != expected)
        {
            throw StatusConflict(AssetMovementLabels.LoanStatus(loan.Status));
        }
    }

    private static AssetManagementException StatusConflict(string statusName) =>
        AssetManagementException.Conflict(
            AssetMovementErrorCodes.Status,
            string.Format(AssetMovementMessages.StatusFormat, statusName.ToLowerInvariant()));

    private static AssetManagementException OpenExtensionConflict(Exception? inner = null) =>
        inner == null
            ? AssetManagementException.Conflict(AssetMovementErrorCodes.OpenExtension, AssetMovementMessages.OpenExtension)
            : AssetManagementException.Conflict(AssetMovementErrorCodes.OpenExtension, AssetMovementMessages.OpenExtension, inner);

    private static void EnsureVersion(AstLoan loan, Guid? rowVersion)
    {
        if (rowVersion is not { } version || version == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.Field, AssetMovementMessages.RowVersionRequired);
        }

        if (loan.RowVersion != version)
        {
            throw AssetManagementException.Conflict(AssetMovementErrorCodes.VersionMismatch, AssetMovementMessages.VersionMismatch);
        }
    }

    private async Task<List<AstAsset>> LockAssetsAsync(List<Guid> ids, Guid? legalEntityId, CancellationToken cancellationToken)
    {
        var ordered = ids.Distinct().OrderBy(x => x).ToArray();
        var assets = await _context.AstAssets
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAsset\" WHERE \"Id\" = ANY({ordered}) AND \"IsDelete\" = false ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);

        if (assets.Count != ordered.Length || (legalEntityId.HasValue && assets.Any(x => x.LegalEntityId != legalEntityId.Value)))
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        }

        return assets;
    }

    private async Task<AstLoan> LockLoanAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstLoans
            .FromSqlInterpolated($"SELECT * FROM public.\"AstLoan\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw LoanNotFound();

    private async Task<string> UnitNameAsync(Guid unitId, CancellationToken cancellationToken) =>
        await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken) ?? "-";

    private static bool IsParty(AssetActor actor, Guid ownerUnitId, Guid borrowerUnitId) =>
        actor.CanAccessUnit(ownerUnitId) ||
        actor.CanAccessUnit(borrowerUnitId) ||
        actor.IsHeadOfUnit(ownerUnitId) ||
        actor.IsHeadOfUnit(borrowerUnitId);

    private static bool CanCancel(Guid requestedBy, Guid ownerUnitId, AssetActor actor) =>
        requestedBy == actor.UserId || actor.IsHeadOfUnit(ownerUnitId);

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
        catch (DbUpdateConcurrencyException exception)
        {
            _context.ChangeTracker.Clear();
            throw AssetManagementException.Conflict(AssetMovementErrorCodes.VersionMismatch, AssetMovementMessages.VersionMismatch, exception);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string RequiredReason(string? value)
    {
        var reason = value?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonRequired, AssetMovementMessages.ReasonRequired);
        }

        return reason;
    }

    private static string? OptionalReason(string? value)
    {
        var reason = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (reason is { Length: > MaxReasonLength })
        {
            throw AssetManagementException.Invalid(AssetMovementErrorCodes.ReasonRequired, AssetMovementMessages.ReasonRequired);
        }

        return reason;
    }

    private static void Touch(AstLoan loan, Guid actorUserId, DateTime now)
    {
        loan.UpdateBy = actorUserId;
        loan.UpdateDateTime = now;
        loan.RowVersion = Guid.NewGuid();
    }

    private static void TouchAsset(AstAsset asset, Guid actorUserId, DateTime now)
    {
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
        asset.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException LoanNotFound() =>
        AssetManagementException.NotFound(AssetMovementErrorCodes.NotFound, AssetMovementMessages.LoanNotFound);
}
