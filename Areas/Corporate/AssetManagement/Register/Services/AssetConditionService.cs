using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetConditionService
{
    public const string ConditionReportDocumentType = "AstConditionReport";
    public const string PermissionResource = "AssetCondition";
    public const string DecideAction = "Decide";
    public const string ApproveLossAction = "ApproveLoss";
    private const string OpenReportIndexName = "IX_AstConditionReport_Open";
    private const string LogCategory = "Corporate.AssetManagement.Condition";
    private const int MaxNotesLength = 1000;
    private const int MaxReasonLength = 500;
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetHistoryService _historyService;
    private readonly AssetAttachmentService _attachmentService;
    private readonly AssetLabelDocumentService _documentService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private ConditionPermissions? _permissions;

    public AssetConditionService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        AssetHistoryService historyService,
        AssetAttachmentService attachmentService,
        AssetLabelDocumentService documentService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<AssetConditionReportResponse> ReportAsync(AssetConditionReportCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);

        if (request.AssetId is not { } assetId || assetId == Guid.Empty || request.ReportedCondition is not { } reported || !Enum.IsDefined(reported))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionRequired);
        }

        var source = request.Source ?? ConditionReportSource.LaporanUnit;
        if (source is not (ConditionReportSource.LaporanUnit or ConditionReportSource.PemeriksaanBagianAset))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionSourceManual);
        }

        if (source == ConditionReportSource.PemeriksaanBagianAset && !(await GetPermissionsAsync()).CanDecide)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.ConditionSource, AssetRegisterMessages.ConditionSource);
        }

        var notes = NormalizeNotes(request.Notes);
        if (source == ConditionReportSource.LaporanUnit && (request.Photo == null || request.Photo.Length <= 0))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ConditionPhotoRequired, AssetRegisterMessages.ConditionPhotoRequired);
        }

        if (request.LossReport != null && reported != AssetCondition.Hilang)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionLossReportOnlyLost);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var created = await InTransactionAsync(async () =>
        {
            var asset = await LockAssetAsync(assetId, entity.LegalEntityId, cancellationToken);
            EnsureCanReportOn(asset, actor);
            EnsureReportable(asset, reported, request.IsFoundAgain);
            await EnsureNoOpenReportAsync(asset.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var report = new AstConditionReport
            {
                AssetId = asset.Id,
                Source = source,
                PreviousCondition = asset.Condition,
                ReportedCondition = reported,
                IsFoundAgain = request.IsFoundAgain,
                Notes = notes,
                Status = ConditionReportStatus.Dilaporkan,
                ReportedBy = actor.UserId,
                ReportedAt = now,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            if (request.Photo is { Length: > 0 })
            {
                var photo = await _attachmentService.SaveAsync(request.Photo, ConditionReportDocumentType, report.Id, actor.UserId, cancellationToken);
                report.PhotoAttachmentId = photo.Id;
            }

            if (request.LossReport != null)
            {
                var lossReport = await _attachmentService.SaveAsync(request.LossReport, ConditionReportDocumentType, report.Id, actor.UserId, cancellationToken);
                report.LossReportAttachmentId = lossReport.Id;
            }

            var number = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.ConditionReport, actor.UserId, cancellationToken);
            report.ReportNumber = number;
            _context.AstConditionReports.Add(report);
            await SaveReportAsync(cancellationToken);
            return (report.Id, number);
        }, cancellationToken);

        await AuditAsync("AssetCondition.Report", $"Laporan kondisi {created.number} ({AssetRegisterLabels.Condition(reported)}) dibuat oleh {actor.UserId}.");
        return await GetReportAsync(created.Id, cancellationToken);
    }

    public async Task<AstConditionReport> OpenReportAsync(AssetConditionOpenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Source is not (ConditionReportSource.PengembalianPinjaman or ConditionReportSource.TindakLanjutOpname))
        {
            throw new ArgumentException("Laporan sistem hanya bersumber pengembalian pinjaman atau tindak lanjut opname.", nameof(request));
        }

        if (!Enum.IsDefined(request.ReportedCondition) || request.ActorUserId == Guid.Empty)
        {
            throw new ArgumentException("Kondisi dan pelaku laporan sistem wajib diisi.", nameof(request));
        }

        var asset = await _context.AstAssets.FirstOrDefaultAsync(x => x.Id == request.AssetId && !x.IsDelete, cancellationToken)
            ?? throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        var foundAgain = asset.Condition == AssetCondition.Hilang && request.ReportedCondition != AssetCondition.Hilang;
        EnsureReportable(asset, request.ReportedCondition, foundAgain);
        await EnsureNoOpenReportAsync(asset.Id, cancellationToken);

        var now = DateTime.UtcNow;
        var report = new AstConditionReport
        {
            ReportNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.ConditionReport, request.ActorUserId, cancellationToken),
            AssetId = asset.Id,
            Source = request.Source,
            SourceDocumentId = request.SourceDocumentId,
            PreviousCondition = asset.Condition,
            ReportedCondition = request.ReportedCondition,
            IsFoundAgain = foundAgain,
            Notes = NormalizeNotes(request.Notes),
            Status = ConditionReportStatus.Dilaporkan,
            ReportedBy = request.ActorUserId,
            ReportedAt = now,
            CreateBy = request.ActorUserId,
            CreateDateTime = now
        };

        _context.AstConditionReports.Add(report);
        await SaveReportAsync(cancellationToken);
        await AuditAsync("AssetCondition.OpenReport", $"Laporan kondisi {report.ReportNumber} dibuka otomatis ({AssetRegisterLabels.ConditionReportSource(request.Source)}).");
        return report;
    }

    public async Task<AssetConditionReportResponse> DecideAsync(Guid id, AssetConditionDecideRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Accept ? OptionalReason(request.Reason) : RequiredReason(request.Reason);

        if (request.Accept && request.DecidedCondition is { } chosen && (!Enum.IsDefined(chosen) || chosen == AssetCondition.Hilang))
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionDecideNotLost);
        }

        var result = await InTransactionAsync(async () =>
        {
            var report = await LockReportAsync(id, cancellationToken);
            var asset = await LockAssetAsync(report.AssetId, null, cancellationToken);
            EnsureCanSee(report, asset, actor);
            EnsureOpen(report, request.RowVersion);

            var now = DateTime.UtcNow;
            if (!request.Accept)
            {
                report.Status = ConditionReportStatus.Ditolak;
                report.DecidedBy = actor.UserId;
                report.DecidedAt = now;
                report.DecisionReason = reason;
                TouchReport(report, actor.UserId, now);
                await _context.SaveChangesAsync(cancellationToken);
                return (report.ReportNumber, Action: "Reject");
            }

            if (RequiresLossApproval(report))
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionRoute, AssetRegisterMessages.ConditionDecideLossRoute);
            }

            ApplyDecision(report, asset, request.DecidedCondition ?? report.ReportedCondition, reason, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return (report.ReportNumber, Action: "Decide");
        }, cancellationToken);

        await AuditAsync($"AssetCondition.{result.Action}", $"Laporan kondisi {result.ReportNumber} {(result.Action == "Reject" ? "ditolak" : "ditetapkan")} oleh {actor.UserId}.");
        return await GetReportAsync(id, cancellationToken);
    }

    public async Task<AssetConditionReportResponse> ApproveLossAsync(Guid id, AssetConditionApproveLossRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = OptionalReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var report = await LockReportAsync(id, cancellationToken);
            var asset = await LockAssetAsync(report.AssetId, null, cancellationToken);
            EnsureCanSee(report, asset, actor);
            EnsureOpen(report, request.RowVersion);

            if (!RequiresLossApproval(report))
            {
                throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionRoute, AssetRegisterMessages.ConditionApproveLossRoute);
            }

            var now = DateTime.UtcNow;
            if (report.ReportedCondition == AssetCondition.Hilang)
            {
                if (report.ReportedBy == actor.UserId)
                {
                    throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.LossSelfApproval, AssetRegisterMessages.LossSelfApproval);
                }

                if (request.LossReport is { Length: > 0 })
                {
                    var lossReport = await _attachmentService.SaveAsync(request.LossReport, ConditionReportDocumentType, report.Id, actor.UserId, cancellationToken);
                    report.LossReportAttachmentId = lossReport.Id;
                }

                if (report.LossReportAttachmentId == null)
                {
                    throw AssetManagementException.Invalid(AssetRegisterErrorCodes.LossDocumentRequired, AssetRegisterMessages.LossDocumentRequired);
                }

                ApplyDecision(report, asset, AssetCondition.Hilang, reason, actor.UserId, now);
            }
            else
            {
                var decided = request.DecidedCondition ?? report.ReportedCondition;
                if (!Enum.IsDefined(decided) || decided == AssetCondition.Hilang)
                {
                    throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionFoundAgainNotLost);
                }

                ApplyDecision(report, asset, decided, reason, actor.UserId, now);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return report.ReportNumber;
        }, cancellationToken);

        await AuditAsync("AssetCondition.ApproveLoss", $"Laporan kondisi {number} disetujui Kepala Bagian Aset {actor.UserId}.");
        return await GetReportAsync(id, cancellationToken);
    }

    public async Task<AssetAttachmentContent> OpenAttachmentAsync(Guid id, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var report = await GetReportAsync(id, cancellationToken);
        if (report.Photo?.Id != attachmentId && report.LossReport?.Id != attachmentId)
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.AttachmentNotFound, AssetRegisterMessages.AttachmentNotFound);
        }

        var actor = await RequireActorAsync(cancellationToken);
        await _attachmentService.GetForOwnerAsync(attachmentId, ConditionReportDocumentType, id, cancellationToken);
        return await _attachmentService.OpenReadAsync(attachmentId, actor.UserId, cancellationToken);
    }

    public async Task<AssetLabelDocument> GetLossDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await GetReportAsync(id, cancellationToken);
        if (report.Status != ConditionReportStatus.Ditetapkan || report.DecidedCondition != AssetCondition.Hilang)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionDocumentStatus, AssetRegisterMessages.ConditionDocumentStatus);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var content = _documentService.RenderLossReport(report, entity.LegalEntityName, entity.HospitalSiteName);
        return new AssetLabelDocument(content, $"berita-acara-kehilangan-{report.ReportNumber}.pdf", 1);
    }

    private void ApplyDecision(AstConditionReport report, AstAsset asset, AssetCondition decided, string? reason, Guid actorUserId, DateTime now)
    {
        var previous = asset.Condition;
        report.Status = ConditionReportStatus.Ditetapkan;
        report.DecidedCondition = decided;
        report.DecidedBy = actorUserId;
        report.DecidedAt = now;
        report.DecisionReason = reason;
        TouchReport(report, actorUserId, now);

        if (previous == decided)
        {
            return;
        }

        asset.Condition = decided;
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
        asset.RowVersion = Guid.NewGuid();

        _historyService.Append(new AssetHistoryEntry(
            asset.Id,
            AssetHistoryEventType.KondisiBerubah,
            AssetSubModules.Register,
            actorUserId,
            FromValue: AssetRegisterLabels.Condition(previous),
            ToValue: AssetRegisterLabels.Condition(decided),
            SourceDocumentType: ConditionReportDocumentType,
            SourceDocumentId: report.Id,
            SourceDocumentNumber: report.ReportNumber,
            Reason: reason ?? report.Notes,
            OccurredAt: now));
    }

    private static bool RequiresLossApproval(AstConditionReport report) =>
        report.ReportedCondition == AssetCondition.Hilang || report.IsFoundAgain;

    private static void EnsureReportable(AstAsset asset, AssetCondition reported, bool isFoundAgain)
    {
        if (asset.RecordStatus != AssetRecordStatus.Terdaftar)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.Status,
                string.Format(AssetRegisterMessages.ConditionAssetStatusFormat, AssetRegisterLabels.RecordStatus(asset.RecordStatus).ToLowerInvariant()));
        }

        if (asset.Condition == AssetCondition.Hilang && !isFoundAgain)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ConditionAssetLost, AssetRegisterMessages.ConditionAssetLost);
        }

        if (isFoundAgain && asset.Condition != AssetCondition.Hilang)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionFoundAgainOnlyLost);
        }

        if (isFoundAgain && reported == AssetCondition.Hilang)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionFoundAgainNotLost);
        }
    }

    private static void EnsureCanReportOn(AstAsset asset, AssetActor actor)
    {
        var isCustodian = actor.EmployeeId is { } employeeId && asset.CurrentCustodianEmployeeId == employeeId;
        if (!isCustodian && !actor.CanAccessUnit(asset.CurrentOrganizationUnitId))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }
    }

    private static void EnsureCanSee(AstConditionReport report, AstAsset asset, AssetActor actor)
    {
        if (!CanSeeReport(asset.CurrentOrganizationUnitId, asset.CurrentCustodianEmployeeId, report.ReportedBy, actor))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.ConditionReportOutsideScope);
        }
    }

    private static bool CanSeeReport(Guid? unitId, Guid? custodianEmployeeId, Guid reportedBy, AssetActor actor) =>
        actor.CanAccessUnit(unitId) ||
        (actor.EmployeeId is { } employeeId && custodianEmployeeId == employeeId) ||
        reportedBy == actor.UserId;

    private static void EnsureOpen(AstConditionReport report, Guid? rowVersion)
    {
        if (report.Status != ConditionReportStatus.Dilaporkan)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionReportDecided, AssetRegisterMessages.ConditionReportDecided);
        }

        if (rowVersion is { } expected && expected != Guid.Empty && expected != report.RowVersion)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
        }
    }

    private async Task EnsureNoOpenReportAsync(Guid assetId, CancellationToken cancellationToken)
    {
        var open = await _context.AstConditionReports
            .AsNoTracking()
            .AnyAsync(x => x.AssetId == assetId && x.Status == ConditionReportStatus.Dilaporkan && !x.IsDelete, cancellationToken);
        if (open)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionReportOpen, AssetRegisterMessages.ConditionReportOpen);
        }
    }

    private async Task SaveReportAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: OpenReportIndexName })
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.ConditionReportOpen, AssetRegisterMessages.ConditionReportOpen, exception);
        }
    }

    private async Task<AstAsset> LockAssetAsync(Guid assetId, Guid? legalEntityId, CancellationToken cancellationToken)
    {
        var asset = await _context.AstAssets
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAsset\" WHERE \"Id\" = {assetId} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (asset == null || (legalEntityId.HasValue && asset.LegalEntityId != legalEntityId.Value))
        {
            throw AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
        }

        return asset;
    }

    private async Task<AstConditionReport> LockReportAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstConditionReports
            .FromSqlInterpolated($"SELECT * FROM public.\"AstConditionReport\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw ReportNotFound();

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<ConditionPermissions> GetPermissionsAsync()
    {
        if (_permissions != null)
        {
            return _permissions;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return _permissions = new ConditionPermissions(false, false);
        }

        var canDecide = await _accessPermissionService.HasAccessAsync(user, PermissionResource, DecideAction);
        var canApproveLoss = await _accessPermissionService.HasAccessAsync(user, PermissionResource, ApproveLossAction);
        return _permissions = new ConditionPermissions(canDecide, canApproveLoss);
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

    private static string? NormalizeNotes(string? value)
    {
        var notes = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (notes is { Length: > MaxNotesLength })
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.Field, AssetRegisterMessages.ConditionNotes);
        }

        return notes;
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

    private static void TouchReport(AstConditionReport report, Guid actorUserId, DateTime now)
    {
        report.UpdateBy = actorUserId;
        report.UpdateDateTime = now;
        report.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException ReportNotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.ConditionReportNotFound, AssetRegisterMessages.ConditionReportNotFound);

    private sealed record ConditionPermissions(bool CanDecide, bool CanApproveLoss);
}
