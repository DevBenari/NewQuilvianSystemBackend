using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed partial class StockTakeSessionService
{
    public async Task<StockTakeSessionResponse> CloseCountingAsync(Guid id, StockTakeVersionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var canCreate = await HasPermissionAsync(CreateAction);

        var closed = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            await EnsureCountingActorAsync(session, actor, canCreate, cancellationToken);
            EnsureStatus(session, StockTakeStatus.Berjalan);
            EnsureVersion(session, request.RowVersion);

            var now = DateTime.UtcNow;
            var counts = await _matchingService.ComputeAsync(session, actor.UserId, now, cancellationToken);
            session.Status = StockTakeStatus.Pencocokan;
            session.CountingClosedBy = actor.UserId;
            session.CountingClosedAt = now;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return (session.SessionNumber, counts);
        }, cancellationToken);

        var c = closed.counts;
        await AuditAsync("AssetStockTake.CloseCounting",
            $"Pemindaian sesi {closed.SessionNumber} ditutup oleh {actor.UserId}: {c.Sesuai} sesuai, {c.SesuaiSedangDipinjam} sesuai sedang dipinjam, {c.TidakDitemukan} tidak ditemukan, {c.BedaLokasi} beda lokasi, {c.BedaKondisi} beda kondisi, {c.TidakTerdaftar} tidak terdaftar.");
        return await GetSessionAsync(id, cancellationToken);
    }

    public async Task<StockTakeSessionResponse> ReopenCountingAsync(Guid id, StockTakeDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);
        var canCreate = await HasPermissionAsync(CreateAction);

        var number = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            await EnsureCountingActorAsync(session, actor, canCreate, cancellationToken);
            EnsureStatus(session, StockTakeStatus.Pencocokan);
            EnsureVersion(session, request.RowVersion);

            var now = DateTime.UtcNow;
            session.Status = StockTakeStatus.Berjalan;
            session.CountingClosedBy = null;
            session.CountingClosedAt = null;
            session.DecisionReason = reason;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return session.SessionNumber;
        }, cancellationToken);

        await AuditAsync("AssetStockTake.ReopenCounting", $"Pemindaian sesi {number} dibuka kembali oleh {actor.UserId}: {reason}");
        return await GetSessionAsync(id, cancellationToken);
    }

    private async Task EnsureCountingActorAsync(AstStockTakeSession session, AssetActor actor, bool canCreate, CancellationToken cancellationToken)
    {
        var leaderEmployeeId = await _context.AstStockTakeTeamMembers
            .AsNoTracking()
            .Where(x => x.SessionId == session.Id && !x.IsDelete && x.IsLeader)
            .Select(x => (Guid?)x.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!CanStart(actor, session.OrganizationUnitId, leaderEmployeeId, canCreate))
        {
            throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.CountingScope);
        }
    }
}
