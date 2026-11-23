using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed partial class StockTakeSessionService
{
    public const string PermissionResource = "AssetStockTake";
    public const string CreateAction = "Create";
    public const string StartAction = "Start";
    public const string CancelAction = "Cancel";
    public const string ScanAction = "Scan";
    public const string CloseAction = "Close";
    public const string ActivePerUnitIndexName = "IX_AstStockTakeSession_ActivePerUnit";
    private const string LogCategory = "Corporate.AssetManagement.PhysicalInventory";
    private const int MaxTeamSize = 30;
    private const int MaxReasonLength = 500;
    private const int MaxNotesLength = 1000;
    private const int MaxPageSize = 100;
    private const int MaxBlockerNumbers = 50;

    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

    private static readonly StockTakeStatus[] CancellableStatuses =
        [StockTakeStatus.Dijadwalkan, StockTakeStatus.Berjalan, StockTakeStatus.Pencocokan, StockTakeStatus.MenungguVerifikasi];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly MovementQueryService _movementQueryService;
    private readonly AssetPlacementService _placementService;
    private readonly StockTakeMatchingService _matchingService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly Dictionary<string, bool> _permissions = new(StringComparer.Ordinal);

    public StockTakeSessionService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetEntityResolver entityResolver,
        AssetNumberingService numberingService,
        MovementQueryService movementQueryService,
        AssetPlacementService placementService,
        StockTakeMatchingService matchingService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _movementQueryService = movementQueryService ?? throw new ArgumentNullException(nameof(movementQueryService));
        _placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
        _matchingService = matchingService ?? throw new ArgumentNullException(nameof(matchingService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    public async Task<StockTakeSessionResponse> ScheduleAsync(StockTakeScheduleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var input = await ValidateScheduleAsync(request, cancellationToken);
        actor.EnsureCanAccessUnit(input.UnitId);
        await EnsureNoActiveSessionAsync(input.UnitId, null, cancellationToken);

        var created = await InTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var session = new AstStockTakeSession
            {
                SessionNumber = await _numberingService.AllocateDocumentNumberAsync(AssetDocumentNumberSeries.StockTake, actor.UserId, cancellationToken),
                OrganizationUnitId = input.UnitId,
                LocationId = input.LocationId,
                PlannedStartDate = input.Start,
                PlannedEndDate = input.End,
                Status = StockTakeStatus.Dijadwalkan,
                Notes = input.Notes,
                CreateBy = actor.UserId,
                CreateDateTime = now
            };

            foreach (var member in input.Members)
            {
                session.TeamMembers.Add(new AstStockTakeTeamMember
                {
                    SessionId = session.Id,
                    EmployeeId = member.EmployeeId,
                    IsLeader = member.IsLeader,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            _context.AstStockTakeSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);
            return (session.Id, session.SessionNumber);
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Schedule", $"Sesi inventarisasi {created.SessionNumber} dijadwalkan {input.Start:yyyy-MM-dd}..{input.End:yyyy-MM-dd} dengan {input.Members.Count} anggota tim oleh {actor.UserId}.");
        return await GetSessionAsync(created.Id, cancellationToken);
    }

    public async Task<StockTakeSessionResponse> UpdateAsync(Guid id, StockTakeScheduleRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var input = await ValidateScheduleAsync(request, cancellationToken);
        actor.EnsureCanAccessUnit(input.UnitId);

        var number = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            actor.EnsureCanAccessUnit(session.OrganizationUnitId);
            EnsureStatus(session, StockTakeStatus.Dijadwalkan);
            EnsureVersion(session, request.RowVersion);
            if (session.OrganizationUnitId != input.UnitId)
            {
                await EnsureNoActiveSessionAsync(input.UnitId, session.Id, cancellationToken);
            }

            var now = DateTime.UtcNow;
            session.OrganizationUnitId = input.UnitId;
            session.LocationId = input.LocationId;
            session.PlannedStartDate = input.Start;
            session.PlannedEndDate = input.End;
            session.Notes = input.Notes;
            Touch(session, actor.UserId, now);

            var existing = await _context.AstStockTakeTeamMembers
                .Where(x => x.SessionId == session.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var requested = input.Members.ToDictionary(x => x.EmployeeId);
            foreach (var member in existing)
            {
                if (member.IsLeader || !requested.ContainsKey(member.EmployeeId))
                {
                    member.IsLeader = false;
                    member.UpdateBy = actor.UserId;
                    member.UpdateDateTime = now;
                }

                if (!requested.ContainsKey(member.EmployeeId))
                {
                    member.IsDelete = true;
                    member.DeleteBy = actor.UserId;
                    member.DeleteDateTime = now;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var member in input.Members)
            {
                var current = existing.FirstOrDefault(x => !x.IsDelete && x.EmployeeId == member.EmployeeId);
                if (current != null)
                {
                    if (member.IsLeader)
                    {
                        current.IsLeader = true;
                        current.UpdateBy = actor.UserId;
                        current.UpdateDateTime = now;
                    }

                    continue;
                }

                _context.AstStockTakeTeamMembers.Add(new AstStockTakeTeamMember
                {
                    SessionId = session.Id,
                    EmployeeId = member.EmployeeId,
                    IsLeader = member.IsLeader,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return session.SessionNumber;
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Update", $"Jadwal sesi inventarisasi {number} diubah oleh {actor.UserId}.");
        return await GetSessionAsync(id, cancellationToken);
    }

    public async Task<StockTakeSessionResponse> StartAsync(Guid id, StockTakeVersionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var canCreate = await HasPermissionAsync(CreateAction);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var started = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            EnsureStatus(session, StockTakeStatus.Dijadwalkan);
            EnsureVersion(session, request.RowVersion);

            var leaderEmployeeId = await _context.AstStockTakeTeamMembers
                .AsNoTracking()
                .Where(x => x.SessionId == session.Id && !x.IsDelete && x.IsLeader)
                .Select(x => (Guid?)x.EmployeeId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!CanStart(actor, session.OrganizationUnitId, leaderEmployeeId, canCreate))
            {
                throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.StartScope);
            }

            var today = AssetRegisterSettingReader.Today();
            if (today < session.PlannedStartDate)
            {
                throw AssetManagementException.Conflict(
                    AssetStockTakeErrorCodes.NotYetStartable,
                    string.Format(AssetStockTakeMessages.NotYetStartableFormat, FormatDate(session.PlannedStartDate)));
            }

            var unitAssets = await LockUnitAssetsAsync(session.OrganizationUnitId, cancellationToken);
            await EnsureNoActiveSessionAsync(session.OrganizationUnitId, session.Id, cancellationToken);

            var blockers = await FindStartBlockersAsync(session.OrganizationUnitId, cancellationToken);
            var blockerNumbers = blockers.TransferNumbers.Concat(blockers.LoanNumbers).Concat(blockers.HandoverNumbers).ToList();
            if (blockerNumbers.Count > 0)
            {
                throw AssetManagementException.Conflict(
                    AssetStockTakeErrorCodes.ProcessInFlight,
                    string.Format(AssetStockTakeMessages.ProcessInFlightFormat, blockerNumbers.Count, string.Join(", ", blockerNumbers)));
            }

            var loanedOut = await _movementQueryService.FindLoanedOutAssetsAsync(session.OrganizationUnitId, cancellationToken);
            var expected = unitAssets
                .Where(x =>
                    x.LegalEntityId == entity.LegalEntityId &&
                    x.RecordStatus == AssetRecordStatus.Terdaftar &&
                    x.PlacementState == AssetPlacementState.Ditempatkan &&
                    x.CurrentLocationId.HasValue &&
                    x.CurrentCustodianEmployeeId.HasValue &&
                    (!session.LocationId.HasValue || x.CurrentLocationId == session.LocationId))
                .ToList();

            var now = DateTime.UtcNow;
            foreach (var asset in expected)
            {
                _context.AstStockTakeExpectedItems.Add(new AstStockTakeExpectedItem
                {
                    SessionId = session.Id,
                    AssetId = asset.Id,
                    ExpectedLocationId = asset.CurrentLocationId!.Value,
                    ExpectedCustodianEmployeeId = asset.CurrentCustodianEmployeeId!.Value,
                    ExpectedCondition = asset.Condition,
                    LoanBorrowerOrganizationUnitId = loanedOut.TryGetValue(asset.Id, out var borrower) ? borrower : null,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                });
            }

            session.Status = StockTakeStatus.Berjalan;
            session.StartedBy = actor.UserId;
            session.StartedAt = now;
            session.ExpectedCount = expected.Count;
            session.ScannedCount = 0;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return (session.SessionNumber, Expected: expected.Count, LoanedOut: expected.Count(x => loanedOut.ContainsKey(x.Id)));
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Start", $"Sesi inventarisasi {started.SessionNumber} dimulai oleh {actor.UserId}; {started.Expected} aset diharapkan, {started.LoanedOut} sedang dipinjam keluar.");
        return await GetSessionAsync(id, cancellationToken);
    }

    public async Task<StockTakeSessionResponse> CancelAsync(Guid id, StockTakeDecisionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var reason = RequiredReason(request.Reason);

        var number = await InTransactionAsync(async () =>
        {
            var session = await LockSessionAsync(id, cancellationToken);
            await EnsureCanSeeAsync(session, actor, cancellationToken);
            if (!CancellableStatuses.Contains(session.Status))
            {
                throw StatusConflict(session.Status);
            }

            EnsureVersion(session, request.RowVersion);
            var now = DateTime.UtcNow;
            session.Status = StockTakeStatus.Dibatalkan;
            session.CancelledBy = actor.UserId;
            session.CancelledAt = now;
            session.DecisionReason = reason;
            Touch(session, actor.UserId, now);
            await _context.SaveChangesAsync(cancellationToken);
            return session.SessionNumber;
        }, cancellationToken);

        await AuditAsync("AssetStockTake.Cancel", $"Sesi inventarisasi {number} dibatalkan oleh {actor.UserId}: {reason}");
        return await GetSessionAsync(id, cancellationToken);
    }

    internal async Task<StockTakeStartBlockersResponse> FindStartBlockersAsync(Guid organizationUnitId, CancellationToken cancellationToken)
    {
        var movement = await _movementQueryService.FindInFlightForUnitAsync(organizationUnitId, cancellationToken);
        var handovers = await _placementService.FindPendingHandoverNumbersForUnitAsync(organizationUnitId, MaxBlockerNumbers, cancellationToken);
        var response = new StockTakeStartBlockersResponse
        {
            TransferNumbers = movement.TransferNumbers.ToList(),
            LoanNumbers = movement.LoanNumbers.ToList(),
            HandoverNumbers = handovers.ToList()
        };
        response.ProcessCount = response.TransferNumbers.Count + response.LoanNumbers.Count + response.HandoverNumbers.Count;
        return response;
    }

    private static bool CanStart(AssetActor actor, Guid organizationUnitId, Guid? leaderEmployeeId, bool canCreate) =>
        (leaderEmployeeId.HasValue && actor.EmployeeId == leaderEmployeeId) ||
        (canCreate && actor.CanAccessUnit(organizationUnitId));

    private async Task<ScheduleInput> ValidateScheduleAsync(StockTakeScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request.OrganizationUnitId is not { } unitId || unitId == Guid.Empty ||
            !await _context.MstOrganizationUnits.AsNoTracking().AnyAsync(x => x.Id == unitId && x.IsActive && !x.IsDelete, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.UnitInactive, AssetStockTakeMessages.UnitInactive);
        }

        if (request.PlannedStartDate is not { } start || request.PlannedEndDate is not { } end)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.DatesRequired);
        }

        if (end < start)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Dates, AssetStockTakeMessages.Dates);
        }

        var locationId = request.LocationId is { } location && location != Guid.Empty ? location : (Guid?)null;
        if (locationId is { } chosen &&
            !await _context.MstAssetLocations.AsNoTracking().AnyAsync(x => x.Id == chosen && x.IsActive && !x.IsDelete && x.OrganizationUnitId == unitId, cancellationToken))
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.LocationUnit, AssetStockTakeMessages.LocationUnit);
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes is { Length: > MaxNotesLength })
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.NotesLength);
        }

        var members = (request.TeamMembers ?? [])
            .Where(x => x.EmployeeId != Guid.Empty)
            .GroupBy(x => x.EmployeeId)
            .Select(x => new StockTakeTeamMemberRequest { EmployeeId = x.Key, IsLeader = x.Any(m => m.IsLeader) })
            .ToList();
        if (members.Count == 0 || members.Count(x => x.IsLeader) != 1)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Team, AssetStockTakeMessages.Team);
        }

        if (members.Count > MaxTeamSize)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Team, AssetStockTakeMessages.TeamSize);
        }

        var employeeIds = members.Select(x => x.EmployeeId).ToList();
        var activeCount = await _context.MstEmployees.AsNoTracking().CountAsync(x => employeeIds.Contains(x.Id) && x.IsActive && !x.IsDelete, cancellationToken);
        if (activeCount != employeeIds.Count)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Team, AssetStockTakeMessages.TeamEmployee);
        }

        return new ScheduleInput(unitId, locationId, start, end, notes, members);
    }

    private async Task EnsureNoActiveSessionAsync(Guid organizationUnitId, Guid? exceptSessionId, CancellationToken cancellationToken)
    {
        var active = await _context.AstStockTakeSessions
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.OrganizationUnitId == organizationUnitId && AssetStockTakeGate.HoldingStatuses.Contains(x.Status) &&
                        (!exceptSessionId.HasValue || x.Id != exceptSessionId.Value))
            .Select(x => x.SessionNumber)
            .FirstOrDefaultAsync(cancellationToken);
        if (active != null)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.ActiveSession, string.Format(AssetStockTakeMessages.ActiveSessionFormat, active));
        }
    }

    private async Task<List<AstAsset>> LockUnitAssetsAsync(Guid organizationUnitId, CancellationToken cancellationToken) =>
        await _context.AstAssets
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAsset\" WHERE \"CurrentOrganizationUnitId\" = {organizationUnitId} AND \"IsDelete\" = false ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);

    private async Task<AstStockTakeSession> LockSessionAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.AstStockTakeSessions
            .FromSqlInterpolated($"SELECT * FROM public.\"AstStockTakeSession\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw SessionNotFound();

    private async Task EnsureCanSeeAsync(AstStockTakeSession session, AssetActor actor, CancellationToken cancellationToken)
    {
        if (actor.CanAccessUnit(session.OrganizationUnitId) || session.CreateBy == actor.UserId)
        {
            return;
        }

        if (actor.EmployeeId is { } employeeId &&
            await _context.AstStockTakeTeamMembers.AsNoTracking().AnyAsync(x => x.SessionId == session.Id && !x.IsDelete && x.EmployeeId == employeeId, cancellationToken))
        {
            return;
        }

        throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.OutsideScope);
    }

    private static void EnsureStatus(AstStockTakeSession session, StockTakeStatus expected)
    {
        if (session.Status != expected)
        {
            throw StatusConflict(session.Status);
        }
    }

    private static AssetManagementException StatusConflict(StockTakeStatus status) =>
        AssetManagementException.Conflict(
            AssetStockTakeErrorCodes.Status,
            string.Format(AssetStockTakeMessages.StatusFormat, AssetStockTakeLabels.Status(status).ToLowerInvariant()));

    private static void EnsureVersion(AstStockTakeSession session, Guid? rowVersion)
    {
        if (rowVersion is not { } version || version == Guid.Empty)
        {
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.Field, AssetStockTakeMessages.RowVersionRequired);
        }

        if (session.RowVersion != version)
        {
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.VersionMismatch, AssetStockTakeMessages.VersionMismatch);
        }
    }

    private async Task<bool> HasPermissionAsync(string action)
    {
        if (_permissions.TryGetValue(action, out var cached))
        {
            return cached;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        var allowed = user != null && await _accessPermissionService.HasAccessAsync(user, PermissionResource, action);
        _permissions[action] = allowed;
        return allowed;
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
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.VersionMismatch, AssetStockTakeMessages.VersionMismatch, exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: ActivePerUnitIndexName })
        {
            _context.ChangeTracker.Clear();
            throw AssetManagementException.Conflict(AssetStockTakeErrorCodes.ActiveSession, string.Format(AssetStockTakeMessages.ActiveSessionFormat, "lain"), exception);
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
            throw AssetManagementException.Invalid(AssetStockTakeErrorCodes.ReasonRequired, AssetStockTakeMessages.ReasonRequired);
        }

        return reason;
    }

    private static string FormatDate(DateOnly date) => date.ToString("d MMMM yyyy", Indonesian);

    private static void Touch(AstStockTakeSession session, Guid actorUserId, DateTime now)
    {
        session.UpdateBy = actorUserId;
        session.UpdateDateTime = now;
        session.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException SessionNotFound() =>
        AssetManagementException.NotFound(AssetStockTakeErrorCodes.NotFound, AssetStockTakeMessages.NotFound);

    private sealed record ScheduleInput(
        Guid UnitId,
        Guid? LocationId,
        DateOnly Start,
        DateOnly End,
        string? Notes,
        List<StockTakeTeamMemberRequest> Members);
}
