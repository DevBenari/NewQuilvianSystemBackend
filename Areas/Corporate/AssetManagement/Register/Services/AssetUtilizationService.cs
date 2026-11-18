using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetUtilizationService
{
    public const string UtilizationOwnerType = "AstUtilization";
    public const string PermissionResource = "AssetUtilization";
    private const string LogCategory = "Corporate.AssetManagement.Utilization";
    private const int MaxAssets = 200;
    private const int MaxPartnerLength = 200;
    private const int MaxAgreementNumberLength = 100;
    private const int MaxPurposeLength = 1000;
    private const int MaxReasonLength = 500;
    private const int MaxPageSize = 100;
    private const int MaxDurationMonths = 1200;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetHistoryService _historyService;
    private readonly AssetProcessLockService _lockService;
    private readonly AssetAttachmentService _attachmentService;
    private readonly AssetValuationParameterQueryService _parameterQuery;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private UtilizationPermissions? _permissions;

    public AssetUtilizationService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetHistoryService historyService,
        AssetProcessLockService lockService,
        AssetAttachmentService attachmentService,
        AssetValuationParameterQueryService parameterQuery,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _parameterQuery = parameterQuery ?? throw new ArgumentNullException(nameof(parameterQuery));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetUtilizationResponse> CreateAsync(AssetUtilizationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var fields = ValidateDraft(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        await EnsureAssetsExistAsync(fields.AssetIds, entity.LegalEntityId, actor, cancellationToken);

        var created = await InTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var utilization = new AstUtilization
            {
                Status = UtilizationStatus.Draft,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };
            Apply(utilization, fields, actor.UserId, now);
            utilization.UtilizationNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.Utilization, actor.UserId, cancellationToken);
            _context.AstUtilizations.Add(utilization);
            await _context.SaveChangesAsync(cancellationToken);
            return (utilization.Id, utilization.UtilizationNumber);
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Create", $"Draft pemanfaatan {created.UtilizationNumber} dibuat oleh {actor.UserId}.");
        return await GetUtilizationAsync(created.Id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> UpdateAsync(Guid id, AssetUtilizationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var fields = ValidateDraft(request);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        await EnsureAssetsExistAsync(fields.AssetIds, entity.LegalEntityId, actor, cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            EnsureOwner(utilization, actor);
            EnsureStatus(utilization, "diubah", UtilizationStatus.Draft);
            EnsureRowVersion(utilization, request.RowVersion);

            var now = DateTime.UtcNow;
            await _context.Entry(utilization).Collection(x => x.Items).Query().Where(x => !x.IsDelete).LoadAsync(cancellationToken);
            Apply(utilization, fields, actor.UserId, now);
            Touch(utilization, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return utilization.UtilizationNumber;
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Update", $"Draft pemanfaatan {number} diubah oleh {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> UploadAgreementAsync(Guid id, AssetUtilizationAgreementRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        var number = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            await EnsureCanSeeAsync(utilization, actor, cancellationToken);
            EnsureStatus(utilization, "dilampiri perjanjian", UtilizationStatus.Draft, UtilizationStatus.Diajukan, UtilizationStatus.MenungguDirektur, UtilizationStatus.Disetujui, UtilizationStatus.Berjalan);
            EnsureRowVersion(utilization, request.RowVersion);

            var attachment = await _attachmentService.SaveAsync(request.File, UtilizationOwnerType, utilization.Id, actor.UserId, cancellationToken);
            utilization.AgreementAttachmentId = attachment.Id;
            Touch(utilization, actor.UserId, DateTime.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);
            return utilization.UtilizationNumber;
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Agreement", $"Berkas perjanjian pemanfaatan {number} diunggah oleh {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var settings = await AssetRegisterSettingReader.ReadAsync(_context, entity.LegalEntityId, cancellationToken);
        var today = AssetRegisterSettingReader.Today();
        var parameter = await _parameterQuery.GetRequiredParameterAtAsync(entity.LegalEntityId, today, cancellationToken);

        var result = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            EnsureOwner(utilization, actor);
            EnsureStatus(utilization, "diajukan", UtilizationStatus.Draft);
            EnsureSubmittable(utilization);

            var assets = await LockAssetsAsync(utilization.Id, cancellationToken);
            if (assets.Count == 0)
            {
                throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.UtilizationAssetCount);
            }

            var ownership = await OwnershipAsync(assets, cancellationToken);
            foreach (var asset in assets)
            {
                EnsureEligible(asset, ownership, entity.LegalEntityId);
            }

            var now = DateTime.UtcNow;
            var needsDirector = utilization.AgreementValue >= parameter.UtilizationDirectorThreshold ||
                DurationMonths(utilization.StartDate, utilization.EndDate) > settings.UtilizationMaxMonthsForAssetHead;
            utilization.RequiredApprovalLevel = needsDirector ? UtilizationApprovalLevel.Direktur : UtilizationApprovalLevel.KepalaBagianAset;
            utilization.ThresholdUsed = parameter.UtilizationDirectorThreshold;
            utilization.Status = UtilizationStatus.Diajukan;
            utilization.SubmittedBy = actor.UserId;
            utilization.SubmittedAt = now;
            utilization.DecisionReason = null;
            Touch(utilization, actor.UserId, now);

            foreach (var asset in assets)
            {
                _lockService.Acquire(asset, AssetActiveProcessKind.Pemanfaatan, utilization.Id);
                TouchAsset(asset, actor.UserId, now);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return (utilization.UtilizationNumber, utilization.RequiredApprovalLevel.Value);
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Submit", $"Pemanfaatan {result.UtilizationNumber} diajukan oleh {actor.UserId}; penyetuju {AssetRegisterLabels.UtilizationApprovalLevel(result.Value)}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> ApproveAsync(Guid id, AssetUtilizationDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = OptionalReason(request.Reason);
        var today = AssetRegisterSettingReader.Today();

        var result = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            await EnsureCanSeeAsync(utilization, actor, cancellationToken);
            EnsureStatus(utilization, "disetujui Kepala Bagian Aset", UtilizationStatus.Diajukan);
            EnsureRowVersion(utilization, request.RowVersion);
            EnsureNotSubmitter(utilization, actor);

            var now = DateTime.UtcNow;
            utilization.AssetHeadDecidedBy = actor.UserId;
            utilization.AssetHeadDecidedAt = now;
            utilization.DecisionReason = reason;

            if (utilization.RequiredApprovalLevel == UtilizationApprovalLevel.Direktur)
            {
                if (!request.Forward)
                {
                    throw AssetManagementException.Forbidden(
                        AssetRegisterErrorCodes.UtilizationApprover,
                        string.Format(AssetRegisterMessages.UtilizationApproverFormat, AssetRegisterLabels.UtilizationApprovalLevel(UtilizationApprovalLevel.Direktur)));
                }

                utilization.Status = UtilizationStatus.MenungguDirektur;
                Touch(utilization, actor.UserId, now);
                await _context.SaveChangesAsync(cancellationToken);
                return (utilization.UtilizationNumber, Action: "Forward");
            }

            if (request.Forward)
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.UtilizationRoute, AssetRegisterMessages.UtilizationForwardNotNeeded);
            }

            await ApproveAndStartAsync(utilization, actor.UserId, now, today, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return (utilization.UtilizationNumber, Action: "Approve");
        }, cancellationToken);

        await AuditAsync($"AssetUtilization.{result.Action}", $"Pemanfaatan {result.UtilizationNumber} {(result.Action == "Forward" ? "diteruskan ke Direktur" : "disetujui Kepala Bagian Aset")} oleh {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> ApproveDirectorAsync(Guid id, AssetUtilizationDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Accept ? OptionalReason(request.Reason) : RequiredReason(request.Reason);
        var today = AssetRegisterSettingReader.Today();

        var result = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            await EnsureCanSeeAsync(utilization, actor, cancellationToken);
            EnsureStatus(utilization, "diputuskan Direktur", UtilizationStatus.MenungguDirektur);
            EnsureRowVersion(utilization, request.RowVersion);
            EnsureNotSubmitter(utilization, actor);

            var now = DateTime.UtcNow;
            utilization.DirectorDecidedBy = actor.UserId;
            utilization.DirectorDecidedAt = now;
            utilization.DecisionReason = reason;

            if (!request.Accept)
            {
                await CloseAsync(utilization, UtilizationStatus.Ditolak, actor.UserId, now, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return (utilization.UtilizationNumber, Action: "RejectDirector");
            }

            await ApproveAndStartAsync(utilization, actor.UserId, now, today, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return (utilization.UtilizationNumber, Action: "ApproveDirector");
        }, cancellationToken);

        await AuditAsync($"AssetUtilization.{result.Action}", $"Pemanfaatan {result.UtilizationNumber} {(result.Action == "ApproveDirector" ? "disetujui" : "ditolak")} Direktur {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> RejectAsync(Guid id, AssetUtilizationDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            await EnsureCanSeeAsync(utilization, actor, cancellationToken);
            if (utilization.Status == UtilizationStatus.MenungguDirektur)
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.UtilizationRoute, AssetRegisterMessages.UtilizationRejectAtDirector);
            }

            EnsureStatus(utilization, "ditolak", UtilizationStatus.Diajukan);
            EnsureRowVersion(utilization, request.RowVersion);
            EnsureNotSubmitter(utilization, actor);

            var now = DateTime.UtcNow;
            utilization.AssetHeadDecidedBy = actor.UserId;
            utilization.AssetHeadDecidedAt = now;
            utilization.DecisionReason = reason;
            await CloseAsync(utilization, UtilizationStatus.Ditolak, actor.UserId, now, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return utilization.UtilizationNumber;
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Reject", $"Pemanfaatan {number} ditolak Kepala Bagian Aset {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> EndAsync(Guid id, AssetUtilizationDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            await EnsureCanSeeAsync(utilization, actor, cancellationToken);
            EnsureStatus(utilization, "diakhiri", UtilizationStatus.Disetujui, UtilizationStatus.Berjalan);
            EnsureRowVersion(utilization, request.RowVersion);

            var now = DateTime.UtcNow;
            utilization.DecisionReason = reason;
            await FinishAsync(utilization, actor.UserId, now, reason, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return utilization.UtilizationNumber;
        }, cancellationToken);

        await AuditAsync("AssetUtilization.End", $"Pemanfaatan {number} diakhiri lebih awal oleh {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetUtilizationResponse> CancelAsync(Guid id, AssetUtilizationDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var utilization = await LockUtilizationAsync(id, cancellationToken);
            EnsureOwner(utilization, actor);
            EnsureStatus(utilization, "dibatalkan", UtilizationStatus.Draft, UtilizationStatus.Diajukan);
            EnsureRowVersion(utilization, request.RowVersion);

            var now = DateTime.UtcNow;
            utilization.DecisionReason = reason;
            utilization.CancelBy = actor.UserId;
            utilization.CancelDateTime = now;
            await CloseAsync(utilization, UtilizationStatus.Dibatalkan, actor.UserId, now, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return utilization.UtilizationNumber;
        }, cancellationToken);

        await AuditAsync("AssetUtilization.Cancel", $"Pemanfaatan {number} dibatalkan oleh {actor.UserId}.");
        return await GetUtilizationAsync(id, cancellationToken);
    }

    public async Task<AssetDailyStatusResult> RunDailyStatusAsync(DateOnly today, Guid? systemActorUserId, CancellationToken cancellationToken = default)
    {
        var started = 0;
        var ended = 0;
        var dueIds = await _context.AstUtilizations
            .AsNoTracking()
            .Where(x => !x.IsDelete &&
                ((x.Status == UtilizationStatus.Disetujui && x.StartDate <= today) ||
                 (x.Status == UtilizationStatus.Berjalan && x.EndDate < today)))
            .OrderBy(x => x.StartDate)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in dueIds)
        {
            var outcome = await InTransactionAsync(async () =>
            {
                var utilization = await LockUtilizationAsync(id, cancellationToken);
                var actorUserId = systemActorUserId is { } system && system != Guid.Empty
                    ? system
                    : utilization.DirectorDecidedBy ?? utilization.AssetHeadDecidedBy ?? utilization.SubmittedBy ?? utilization.CreateBy;
                var now = DateTime.UtcNow;
                var didStart = false;
                var didEnd = false;

                if (utilization.Status == UtilizationStatus.Disetujui && utilization.StartDate <= today)
                {
                    await StartAsync(utilization, actorUserId, now, cancellationToken);
                    didStart = true;
                }

                if (utilization.Status == UtilizationStatus.Berjalan && utilization.EndDate < today)
                {
                    await FinishAsync(utilization, actorUserId, now, AssetRegisterMessages.UtilizationEndedBySchedule, cancellationToken);
                    didEnd = true;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return (didStart, didEnd, utilization.UtilizationNumber);
            }, cancellationToken);

            started += outcome.didStart ? 1 : 0;
            ended += outcome.didEnd ? 1 : 0;
            if (outcome.didStart || outcome.didEnd)
            {
                await AuditAsync("AssetUtilization.DailyStatus", $"Pemanfaatan {outcome.UtilizationNumber} diperbarui pekerja harian ({(outcome.didStart ? "mulai berjalan" : string.Empty)}{(outcome.didStart && outcome.didEnd ? ", " : string.Empty)}{(outcome.didEnd ? "berakhir" : string.Empty)}).");
            }
        }

        return new AssetDailyStatusResult(dueIds.Count, started, ended);
    }

    private async Task ApproveAndStartAsync(AstUtilization utilization, Guid actorUserId, DateTime now, DateOnly today, CancellationToken cancellationToken)
    {
        utilization.Status = UtilizationStatus.Disetujui;
        Touch(utilization, actorUserId, now);
        if (utilization.StartDate <= today)
        {
            await StartAsync(utilization, actorUserId, now, cancellationToken);
        }
    }

    private async Task StartAsync(AstUtilization utilization, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        utilization.Status = UtilizationStatus.Berjalan;
        Touch(utilization, actorUserId, now);
        var assets = await LockAssetsAsync(utilization.Id, cancellationToken);
        foreach (var asset in assets)
        {
            _historyService.Append(new AssetHistoryEntry(
                asset.Id,
                AssetHistoryEventType.Dimanfaatkan,
                AssetSubModules.Register,
                actorUserId,
                ToValue: AssetRegisterLabels.UtilizationType(utilization.UtilizationType),
                SourceDocumentType: UtilizationOwnerType,
                SourceDocumentId: utilization.Id,
                SourceDocumentNumber: utilization.UtilizationNumber,
                Reason: $"{utilization.StartDate:dd-MM-yyyy} s.d. {utilization.EndDate:dd-MM-yyyy}",
                OccurredAt: now));
        }
    }

    private async Task FinishAsync(AstUtilization utilization, Guid actorUserId, DateTime now, string reason, CancellationToken cancellationToken)
    {
        var wasRunning = utilization.Status == UtilizationStatus.Berjalan;
        utilization.Status = UtilizationStatus.Berakhir;
        utilization.EndedBy = actorUserId;
        utilization.EndedAt = now;
        Touch(utilization, actorUserId, now);

        var assets = await LockAssetsAsync(utilization.Id, cancellationToken);
        foreach (var asset in assets)
        {
            ReleaseLock(asset, utilization.Id, actorUserId, now);
            if (wasRunning)
            {
                _historyService.Append(new AssetHistoryEntry(
                    asset.Id,
                    AssetHistoryEventType.PemanfaatanBerakhir,
                    AssetSubModules.Register,
                    actorUserId,
                    FromValue: AssetRegisterLabels.UtilizationType(utilization.UtilizationType),
                    SourceDocumentType: UtilizationOwnerType,
                    SourceDocumentId: utilization.Id,
                    SourceDocumentNumber: utilization.UtilizationNumber,
                    Reason: reason,
                    OccurredAt: now));
            }
        }
    }

    private async Task CloseAsync(AstUtilization utilization, UtilizationStatus status, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var heldLocks = utilization.Status == UtilizationStatus.Diajukan || utilization.Status == UtilizationStatus.MenungguDirektur;
        utilization.Status = status;
        Touch(utilization, actorUserId, now);
        if (!heldLocks)
        {
            return;
        }

        var assets = await LockAssetsAsync(utilization.Id, cancellationToken);
        foreach (var asset in assets)
        {
            ReleaseLock(asset, utilization.Id, actorUserId, now);
        }
    }

    private void ReleaseLock(AstAsset asset, Guid utilizationId, Guid actorUserId, DateTime now)
    {
        if (asset.ActiveProcessKind != AssetActiveProcessKind.Pemanfaatan || asset.ActiveProcessId != utilizationId)
        {
            return;
        }

        _lockService.Release(asset, AssetActiveProcessKind.Pemanfaatan, utilizationId);
        TouchAsset(asset, actorUserId, now);
    }

    private static void EnsureEligible(AstAsset asset, IReadOnlyDictionary<Guid, bool> ownership, Guid legalEntityId)
    {
        var label = string.IsNullOrWhiteSpace(asset.InventoryNumber) ? asset.AssetName : asset.InventoryNumber;
        if (asset.LegalEntityId != legalEntityId)
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        }

        if (!ownership.TryGetValue(asset.OwnershipTypeId, out var capitalizable) || !capitalizable)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.UtilizationNotHospitalOwned,
                string.Format(AssetRegisterMessages.UtilizationNotHospitalOwnedFormat, label));
        }

        string? cause = null;
        if (asset.RecordStatus != AssetRecordStatus.Terdaftar)
        {
            cause = $"berstatus {AssetRegisterLabels.RecordStatus(asset.RecordStatus).ToLowerInvariant()}";
        }
        else if (asset.Condition is AssetCondition.Hilang or AssetCondition.RusakBerat)
        {
            cause = $"berkondisi {AssetRegisterLabels.Condition(asset.Condition).ToLowerInvariant()}";
        }
        else if (asset.ActiveProcessKind != AssetActiveProcessKind.Tidak)
        {
            cause = $"sedang dalam proses {AssetProcessLockService.ProcessName(asset.ActiveProcessKind)}";
        }

        if (cause != null)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.UtilizationAssetIneligible,
                string.Format(AssetRegisterMessages.UtilizationAssetIneligibleFormat, label, cause));
        }
    }

    private static void EnsureSubmittable(AstUtilization utilization)
    {
        if (string.IsNullOrWhiteSpace(utilization.PartnerName) || string.IsNullOrWhiteSpace(utilization.AgreementNumber) || utilization.AgreementValue < 0)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationAgreement, AssetRegisterMessages.UtilizationAgreement);
        }

        if (utilization.EndDate <= utilization.StartDate)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationDates, AssetRegisterMessages.UtilizationDates);
        }
    }

    private static DraftFields ValidateDraft(AssetUtilizationUpsertRequest request)
    {
        if (request.UtilizationType is not { } type || !Enum.IsDefined(type))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationType, AssetRegisterMessages.UtilizationType);
        }

        if (request.StartDate is not { } start || request.EndDate is not { } end)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationDates, AssetRegisterMessages.UtilizationDatesRequired);
        }

        if (end <= start)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationDates, AssetRegisterMessages.UtilizationDates);
        }

        var value = request.AgreementValue ?? 0m;
        if (value < 0 || decimal.Round(value, 2) != value || value >= 10_000_000_000_000_000m)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationAgreement, AssetRegisterMessages.UtilizationAgreementValue);
        }

        var partner = request.PartnerName?.Trim() ?? string.Empty;
        var agreementNumber = request.AgreementNumber?.Trim() ?? string.Empty;
        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? null : request.Purpose.Trim();
        if (partner.Length > MaxPartnerLength || agreementNumber.Length > MaxAgreementNumberLength || purpose is { Length: > MaxPurposeLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.UtilizationAgreement, AssetRegisterMessages.UtilizationAgreementLength);
        }

        var assetIds = (request.AssetIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        if (assetIds.Count is < 1 or > MaxAssets)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.UtilizationAssetCount);
        }

        return new DraftFields(type, partner, agreementNumber, value, start, end, purpose, assetIds);
    }

    private void Apply(AstUtilization utilization, DraftFields fields, Guid actorUserId, DateTime now)
    {
        utilization.UtilizationType = fields.UtilizationType;
        utilization.PartnerName = fields.PartnerName;
        utilization.AgreementNumber = fields.AgreementNumber;
        utilization.AgreementValue = fields.AgreementValue;
        utilization.StartDate = fields.StartDate;
        utilization.EndDate = fields.EndDate;
        utilization.Purpose = fields.Purpose;

        var wanted = fields.AssetIds.ToHashSet();
        foreach (var item in utilization.Items.Where(x => !x.IsDelete && !wanted.Contains(x.AssetId)))
        {
            item.IsDelete = true;
            item.DeleteBy = actorUserId;
            item.DeleteDateTime = now;
        }

        var existing = utilization.Items.Where(x => !x.IsDelete).Select(x => x.AssetId).ToHashSet();
        foreach (var assetId in fields.AssetIds.Where(x => !existing.Contains(x)))
        {
            utilization.Items.Add(new AstUtilizationItem
            {
                UtilizationId = utilization.Id,
                AssetId = assetId,
                CreateBy = actorUserId,
                CreateDateTime = now
            });
        }
    }

    private async Task EnsureAssetsExistAsync(List<Guid> assetIds, Guid legalEntityId, AssetActor actor, CancellationToken cancellationToken)
    {
        var assets = await _context.AstAssets
            .AsNoTracking()
            .Where(x => assetIds.Contains(x.Id) && !x.IsDelete && x.LegalEntityId == legalEntityId)
            .Select(x => new { x.Id, x.CurrentOrganizationUnitId })
            .ToListAsync(cancellationToken);
        if (assets.Count != assetIds.Count)
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.UtilizationAssetNotFound);
        }

        foreach (var asset in assets)
        {
            actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);
        }
    }

    private async Task<Dictionary<Guid, bool>> OwnershipAsync(IEnumerable<AstAsset> assets, CancellationToken cancellationToken)
    {
        var ids = assets.Select(x => x.OwnershipTypeId).Distinct().ToList();
        return await _context.MstAssetOwnershipTypes
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.IsCapitalizable, cancellationToken);
    }

    private async Task<List<AstAsset>> LockAssetsAsync(Guid utilizationId, CancellationToken cancellationToken) =>
        await _context.AstAssets
            .FromSqlInterpolated($"SELECT a.* FROM public.\"AstAsset\" a WHERE a.\"IsDelete\" = false AND a.\"Id\" IN (SELECT i.\"AssetId\" FROM public.\"AstUtilizationItem\" i WHERE i.\"UtilizationId\" = {utilizationId} AND i.\"IsDelete\" = false) ORDER BY a.\"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);

    private async Task<AstUtilization> LockUtilizationAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstUtilizations
            .FromSqlInterpolated($"SELECT * FROM public.\"AstUtilization\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw UtilizationNotFound();

    private async Task EnsureCanSeeAsync(AstUtilization utilization, AssetActor actor, CancellationToken cancellationToken)
    {
        if (actor.CanReadAllUnits || utilization.CreateBy == actor.UserId)
        {
            return;
        }

        var unitIds = actor.UnitIds.ToList();
        var visible = await _context.AstUtilizationItems
            .AsNoTracking()
            .AnyAsync(x => x.UtilizationId == utilization.Id && !x.IsDelete &&
                x.Asset!.CurrentOrganizationUnitId.HasValue && unitIds.Contains(x.Asset.CurrentOrganizationUnitId.Value), cancellationToken);
        if (!visible)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }
    }

    private static void EnsureOwner(AstUtilization utilization, AssetActor actor)
    {
        if (utilization.CreateBy != actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.UtilizationOwner, AssetRegisterMessages.UtilizationOwner);
        }
    }

    private static void EnsureNotSubmitter(AstUtilization utilization, AssetActor actor)
    {
        if (utilization.SubmittedBy == actor.UserId || utilization.CreateBy == actor.UserId)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.UtilizationApprover, AssetRegisterMessages.UtilizationSelfDecision);
        }
    }

    private static void EnsureStatus(AstUtilization utilization, string actionLabel, params UtilizationStatus[] allowed)
    {
        if (!allowed.Contains(utilization.Status))
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.UtilizationStatus,
                string.Format(AssetRegisterMessages.UtilizationStatusFormat, AssetRegisterLabels.UtilizationStatus(utilization.Status).ToLowerInvariant(), actionLabel));
        }
    }

    private static void EnsureRowVersion(AstUtilization utilization, Guid? rowVersion)
    {
        if (rowVersion is { } expected && expected != Guid.Empty && expected != utilization.RowVersion)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
        }
    }

    public static int DurationMonths(DateOnly start, DateOnly end)
    {
        var months = 0;
        while (start.AddMonths(months) < end && months < MaxDurationMonths)
        {
            months++;
        }

        return months;
    }

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<UtilizationPermissions> GetPermissionsAsync()
    {
        if (_permissions != null)
        {
            return _permissions;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return _permissions = new UtilizationPermissions(false, false, false, false, false);
        }

        return _permissions = new UtilizationPermissions(
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Create"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Submit"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "Approve"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "ApproveDirector"),
            await _accessPermissionService.HasAccessAsync(user, PermissionResource, "End"));
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
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch, exception);
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
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static string? OptionalReason(string? value)
    {
        var reason = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (reason is { Length: > MaxReasonLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static void Touch(AstUtilization utilization, Guid actorUserId, DateTime now)
    {
        utilization.UpdateBy = actorUserId;
        utilization.UpdateDateTime = now;
        utilization.RowVersion = Guid.NewGuid();
    }

    private static void TouchAsset(AstAsset asset, Guid actorUserId, DateTime now)
    {
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException UtilizationNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.UtilizationNotFound, AssetRegisterMessages.UtilizationNotFound);

    private sealed record DraftFields(
        UtilizationType UtilizationType,
        string PartnerName,
        string AgreementNumber,
        decimal AgreementValue,
        DateOnly StartDate,
        DateOnly EndDate,
        string? Purpose,
        List<Guid> AssetIds);

    private sealed record UtilizationPermissions(bool CanCreate, bool CanSubmit, bool CanApprove, bool CanApproveDirector, bool CanEnd);
}

public sealed record AssetDailyStatusResult(int Due, int Started, int Ended);
