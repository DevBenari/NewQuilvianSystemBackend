using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed partial class StockTakeSessionService
{
    public const string SessionDocumentType = "AstStockTakeSession";
    private const int MaxTeamNoteLength = 1000;
    private const int MaxDocumentRows = 5000;
    private const int DocumentPageSize = 200;

    internal static readonly StockTakeResultCategory[] DiscrepancyCategories =
    [
        StockTakeResultCategory.TidakDitemukan,
        StockTakeResultCategory.BedaLokasi,
        StockTakeResultCategory.BedaKondisi,
        StockTakeResultCategory.TidakTerdaftar
    ];

    public async Task<StockTakeResultResponse> UpdateResultNoteAsync(Guid id, Guid resultId, StockTakeResultNoteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var canCreate = await HasPermissionAsync(CreateAction);
        var note = string.IsNullOrWhiteSpace(request.TeamNote) ? null : request.TeamNote.Trim();
        if (note is { Length: > MaxTeamNoteLength })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.TeamNoteLength);
        }

        await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            await EnsureSubmitActorAsync(session.Id, session.OrganizationUnitId, actor, canCreate, cancellationToken);
            EnsureStatus(session, StockTakeStatus.Pencocokan);
            var result = await _context.AstStockTakeResults.FirstOrDefaultAsync(x => x.Id == resultId && x.SessionId == session.Id && !x.IsDelete, cancellationToken)
                ?? throw AssetManagementException.NotFound(AssetStockTakeErrorCodes.ResultNotFound, AssetStockTakeMessages.ResultNotFound);
            var now = DateTime.UtcNow;
            result.TeamNote = note;
            result.UpdateBy = actor.UserId;
            result.UpdateDateTime = now;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

        var page = await _matchingService.GetResultsAsync(id, new StockTakeItemQuery { PageSize = 1 }, cancellationToken, resultId);
        return page.Items[0];
    }

    public async Task<StockTakeSessionResponse> SubmitAsync(Guid id, StockTakeVersionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var canCreate = await HasPermissionAsync(CreateAction);

        var number = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            await EnsureSubmitActorAsync(session.Id, session.OrganizationUnitId, actor, canCreate, cancellationToken);
            EnsureStatus(session, StockTakeStatus.Pencocokan);
            EnsureVersion(session, request.RowVersion);

            var missing = await _context.AstStockTakeResults
                .CountAsync(x => x.SessionId == session.Id && !x.IsDelete && DiscrepancyCategories.Contains(x.Category) &&
                                 (x.TeamNote == null || x.TeamNote.Trim() == string.Empty), cancellationToken);
            if (missing > 0)
            {
                throw AssetManagementException.Invalid(
                    AssetStockTakeErrorCodes.DiscrepancyNote,
                    string.Format(AssetStockTakeMessages.DiscrepancyNoteFormat, missing));
            }

            var now = DateTime.UtcNow;
            session.Status = StockTakeStatus.MenungguVerifikasi;
            session.SubmittedBy = actor.UserId;
            session.SubmittedAt = now;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return session.SessionNumber;
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Submit", $"Sesi inventarisasi {number} diajukan untuk verifikasi oleh {actor.UserId}.");
        return await GetSessionAsync(id, cancellationToken);
    }

    public async Task<StockTakeSessionResponse> VerifyAsync(Guid id, StockTakeVerifyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = request.Approve ? null : RequiredReason(request.Reason);

        var outcome = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            EnsureStatus(session, StockTakeStatus.MenungguVerifikasi);
            var isMember = actor.EmployeeId is { } employeeId &&
                await _context.AstStockTakeTeamMembers.AsNoTracking().AnyAsync(x => x.SessionId == session.Id && x.EmployeeId == employeeId && !x.IsDelete, cancellationToken);
            if (isMember || !actor.CanAccessUnit(session.OrganizationUnitId))
            {
                throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.Verifier, AssetStockTakeMessages.Verifier);
            }

            EnsureVersion(session, request.RowVersion);
            var now = DateTime.UtcNow;
            if (!request.Approve)
            {
                session.Status = StockTakeStatus.Pencocokan;
                session.SubmittedBy = null;
                session.SubmittedAt = null;
                session.DecisionReason = reason;
                Touch(session, actor.UserId, now);
                await _context.SaveChangesAsync(cancellationToken);
                return (session.SessionNumber, FollowUps: 0, Assets: 0);
            }

            session.Status = StockTakeStatus.Diverifikasi;
            session.VerifiedBy = actor.UserId;
            session.VerifiedAt = now;
            Touch(session, actor.UserId, now);
            var followUps = await _followUpService.AddForSessionAsync(session, actor.UserId, now, cancellationToken);
            var assetIds = await _context.AstStockTakeExpectedItems
                .AsNoTracking()
                .Where(x => x.SessionId == session.Id && !x.IsDelete)
                .Select(x => x.AssetId)
                .ToListAsync(cancellationToken);
            foreach (var assetId in assetIds)
            {
                _historyService.Append(new AssetHistoryEntry(
                    assetId,
                    AssetHistoryEventType.Diopname,
                    AssetSubModules.PhysicalInventory,
                    actor.UserId,
                    SourceDocumentType: SessionDocumentType,
                    SourceDocumentId: session.Id,
                    SourceDocumentNumber: session.SessionNumber,
                    OccurredAt: now));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return (session.SessionNumber, FollowUps: followUps, Assets: assetIds.Count);
        }, cancellationToken);

        await AuditAsync(
            request.Approve ? "AssetStockTake.Verify" : "AssetStockTake.Return",
            request.Approve
                ? $"Sesi inventarisasi {outcome.SessionNumber} diverifikasi oleh {actor.UserId}: {outcome.FollowUps} usulan tindak lanjut, {outcome.Assets} linimasa Diopname."
                : $"Sesi inventarisasi {outcome.SessionNumber} dikembalikan ke pencocokan oleh {actor.UserId}: {reason}");
        return await GetSessionAsync(id, cancellationToken);
    }

    public async Task<(byte[] Content, string FileName)> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(id, cancellationToken);
        if (session.Status != StockTakeStatus.Diverifikasi)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.Status, AssetStockTakeMessages.DocumentNotReady);
        }

        var discrepancies = new List<StockTakeResultResponse>();
        for (var pageNumber = 1; discrepancies.Count < MaxDocumentRows; pageNumber++)
        {
            var page = await _matchingService.GetResultsAsync(
                id,
                new StockTakeItemQuery { Category = DiscrepancyCategories.ToList(), PageNumber = pageNumber, PageSize = DocumentPageSize },
                cancellationToken);
            discrepancies.AddRange(page.Items);
            if (pageNumber >= page.TotalPage)
            {
                break;
            }
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var content = _documentService.RenderReport(session, discrepancies, entity.LegalEntityName, entity.HospitalSiteName);
        return (content, $"berita-acara-{session.SessionNumber}.pdf");
    }

    private async Task EnsureSubmitActorAsync(Guid sessionId, Guid organizationUnitId, AssetActor actor, bool canCreate, CancellationToken cancellationToken)
    {
        var leaderEmployeeId = await _context.AstStockTakeTeamMembers
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId && !x.IsDelete && x.IsLeader)
            .Select(x => (Guid?)x.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!CanStart(actor, organizationUnitId, leaderEmployeeId, canCreate))
        {
            throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.SubmitScope);
        }
    }
}
