using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed partial class StockTakeSessionService
{
    public const string ActionEdit = "edit";
    public const string ActionStart = "start";
    public const string ActionCancel = "cancel";
    public const string ActionScan = "scan";
    public const string ActionCloseCounting = "closeCounting";
    public const string ActionReopenCounting = "reopenCounting";
    public const string ActionEditNote = "editNote";
    public const string ActionSubmit = "submit";
    public const string ActionVerify = "verify";
    public const string ActionDocument = "document";

    public static AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "plannedStartDate", SortDirection = "desc" },
        SortOptions = [new() { Value = "plannedStartDate", Label = "Tanggal Mulai Rencana" }],
        FlagOptions =
        [
            AssetPlacementService.EnumOptions("status", "Status Sesi", Enum.GetValues<StockTakeStatus>(), AssetStockTakeLabels.Status)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor sesi, nama unit, dan nama ruangan" },
            new() { Name = "status", Type = "integer", Description = "Lihat flagOptions status; boleh diulang (status=1&status=2)" },
            new() { Name = "organizationUnitId", Type = "guid", Description = "Unit yang diinventarisasi" },
            new() { Name = "year", Type = "integer", Description = "Tahun tanggal mulai rencana", Example = "2026" },
            new() { Name = "mine", Type = "boolean", Description = "true = hanya sesi tempat pengguna menjadi anggota tim", Example = "true" },
            new() { Name = "overdue", Type = "boolean", Description = "true = hanya sesi yang lewat tanggal selesai rencana dan belum diverifikasi atau dibatalkan", Example = "true" },
            new() { Name = "hasOpenFollowUps", Type = "boolean", Description = "true = hanya sesi yang masih punya usulan tindak lanjut berstatus Usulan", Example = "true" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<StockTakeSessionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var query = Scoped(actor);
        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        var mine = await Mine(query, actor)
            .CountAsync(x => x.Status != StockTakeStatus.Diverifikasi && x.Status != StockTakeStatus.Dibatalkan, cancellationToken);

        int CountOf(StockTakeStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new StockTakeSessionSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Scheduled = CountOf(StockTakeStatus.Dijadwalkan),
            Running = CountOf(StockTakeStatus.Berjalan),
            Matching = CountOf(StockTakeStatus.Pencocokan),
            AwaitingVerification = CountOf(StockTakeStatus.MenungguVerifikasi),
            Verified = CountOf(StockTakeStatus.Diverifikasi),
            Cancelled = CountOf(StockTakeStatus.Dibatalkan),
            Mine = mine
        };
    }

    public async Task<PagedResult<StockTakeSessionResponse>> GetSessionsAsync(StockTakePagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = Filtered(request, actor);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.PlannedStartDate).ThenBy(x => x.SessionNumber)
            : query.OrderByDescending(x => x.PlannedStartDate).ThenByDescending(x => x.SessionNumber);
        var items = await Project(ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, false, cancellationToken);

        return new PagedResult<StockTakeSessionResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<StockTakeSessionResponse> GetSessionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var session = await Project(_context.AstStockTakeSessions.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw SessionNotFound();

        await DescribeAsync([session], actor, true, cancellationToken);
        if (!CanSee(session, actor))
        {
            throw AssetManagementException.Forbidden(AssetStockTakeErrorCodes.OutsideScope, AssetStockTakeMessages.OutsideScope);
        }

        return session;
    }

    public async Task<PagedResult<StockTakeResultResponse>> GetResultsAsync(Guid id, StockTakeItemQuery request, CancellationToken cancellationToken = default)
    {
        await GetSessionAsync(id, cancellationToken);
        var page = await _matchingService.GetResultsAsync(id, request, cancellationToken);
        var briefs = await _followUpService.BriefsForResultsAsync(page.Items.Select(x => x.Id).ToList(), cancellationToken);
        foreach (var item in page.Items)
        {
            item.FollowUps = briefs.GetValueOrDefault(item.Id) ?? [];
        }

        return page;
    }

    private IQueryable<AstStockTakeSession> Filtered(StockTakePagedQuery request, AssetActor actor)
    {
        var query = Scoped(actor);

        var statuses = (request.Status ?? []).Distinct().ToList();
        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.OrganizationUnitId == unitId);
        }

        if (request.Year is { } year && year is >= 2000 and <= 2100)
        {
            var from = new DateOnly(year, 1, 1);
            var to = from.AddYears(1);
            query = query.Where(x => x.PlannedStartDate >= from && x.PlannedStartDate < to);
        }

        if (request.Mine == true)
        {
            query = Mine(query, actor);
        }

        if (request.Overdue == true)
        {
            query = Overdue(query, AssetRegisterSettingReader.Today());
        }

        if (request.HasOpenFollowUps == true)
        {
            query = WithOpenFollowUps(query);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.SessionNumber.ToUpper().Contains(keyword) ||
                (x.OrganizationUnit != null && x.OrganizationUnit.UnitName.ToUpper().Contains(keyword)) ||
                (x.Location != null && x.Location.LocationName.ToUpper().Contains(keyword)));
        }

        return query;
    }

    private IQueryable<AstStockTakeSession> Scoped(AssetActor actor)
    {
        var query = _context.AstStockTakeSessions.AsNoTracking().Where(x => !x.IsDelete);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        var employeeId = actor.EmployeeId;
        var userId = actor.UserId;
        return query.Where(x =>
            unitIds.Contains(x.OrganizationUnitId) ||
            x.CreateBy == userId ||
            (employeeId.HasValue && x.TeamMembers.Any(m => !m.IsDelete && m.EmployeeId == employeeId.Value)));
    }

    internal IQueryable<AstStockTakeSession> ScopedFor(AssetActor actor) => Scoped(actor);

    internal static IQueryable<AstStockTakeSession> Overdue(IQueryable<AstStockTakeSession> query, DateOnly today) =>
        query.Where(x => CancellableStatuses.Contains(x.Status) && x.PlannedEndDate < today);

    internal static IQueryable<AstStockTakeSession> WithOpenFollowUps(IQueryable<AstStockTakeSession> query) =>
        query.Where(x => x.FollowUps.Any(f => !f.IsDelete && f.Status == StockTakeFollowUpStatus.Usulan));

    internal async Task<(int Total, List<StockTakeSessionResponse> Items)> ListForExportAsync(StockTakePagedQuery request, int maxRows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var query = Filtered(request, actor);
        var total = await query.CountAsync(cancellationToken);
        if (total > maxRows)
        {
            return (total, []);
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.PlannedStartDate).ThenBy(x => x.SessionNumber)
            : query.OrderByDescending(x => x.PlannedStartDate).ThenByDescending(x => x.SessionNumber);
        var items = await Project(ordered).ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, false, cancellationToken);
        return (total, items);
    }

    private static IQueryable<AstStockTakeSession> Mine(IQueryable<AstStockTakeSession> query, AssetActor actor)
    {
        if (actor.EmployeeId is not { } employeeId)
        {
            return query.Where(x => false);
        }

        return query.Where(x => x.TeamMembers.Any(m => !m.IsDelete && m.EmployeeId == employeeId));
    }

    private static bool CanSee(StockTakeSessionResponse session, AssetActor actor) =>
        actor.CanAccessUnit(session.OrganizationUnitId) ||
        session.Created?.UserId == actor.UserId ||
        session.IsTeamMember;

    private static IQueryable<StockTakeSessionResponse> Project(IQueryable<AstStockTakeSession> query) =>
        query.Select(x => new StockTakeSessionResponse
        {
            Id = x.Id,
            SessionNumber = x.SessionNumber,
            OrganizationUnitId = x.OrganizationUnitId,
            OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
            LocationId = x.LocationId,
            LocationName = x.Location != null ? x.Location.LocationName : null,
            PlannedStartDate = x.PlannedStartDate,
            PlannedEndDate = x.PlannedEndDate,
            Status = x.Status,
            ExpectedCount = x.ExpectedCount,
            ScannedCount = x.ScannedCount,
            Notes = x.Notes,
            DecisionReason = x.DecisionReason,
            TeamSize = x.TeamMembers.Count(m => !m.IsDelete),
            Created = new StockTakeActorResponse { UserId = x.CreateBy, At = x.CreateDateTime },
            Started = x.StartedBy != null ? new StockTakeActorResponse { UserId = x.StartedBy.Value, At = x.StartedAt!.Value } : null,
            CountingClosed = x.CountingClosedBy != null ? new StockTakeActorResponse { UserId = x.CountingClosedBy.Value, At = x.CountingClosedAt!.Value } : null,
            Submitted = x.SubmittedBy != null ? new StockTakeActorResponse { UserId = x.SubmittedBy.Value, At = x.SubmittedAt!.Value } : null,
            Verified = x.VerifiedBy != null ? new StockTakeActorResponse { UserId = x.VerifiedBy.Value, At = x.VerifiedAt!.Value } : null,
            Cancelled = x.CancelledBy != null ? new StockTakeActorResponse { UserId = x.CancelledBy.Value, At = x.CancelledAt!.Value } : null,
            RowVersion = x.RowVersion
        });

    private async Task DescribeAsync(IReadOnlyCollection<StockTakeSessionResponse> sessions, AssetActor actor, bool withDetail, CancellationToken cancellationToken)
    {
        if (sessions.Count == 0)
        {
            return;
        }

        var sessionIds = sessions.Select(x => x.Id).ToList();
        var members = await _context.AstStockTakeTeamMembers
            .AsNoTracking()
            .Where(x => sessionIds.Contains(x.SessionId) && !x.IsDelete)
            .OrderByDescending(x => x.IsLeader)
            .ThenBy(x => x.Employee!.FullName)
            .Select(x => new
            {
                x.SessionId,
                Member = new StockTakeTeamMemberResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.FullName : null,
                    EmployeeNumber = x.Employee != null ? x.Employee.EmployeeNumber : null,
                    IsLeader = x.IsLeader
                }
            })
            .ToListAsync(cancellationToken);

        await FillActorNamesAsync(sessions, cancellationToken);
        var canCreate = await HasPermissionAsync(CreateAction);
        var canStart = await HasPermissionAsync(StartAction);
        var canCancel = await HasPermissionAsync(CancelAction);
        var canScan = await HasPermissionAsync(ScanAction);
        var canClose = await HasPermissionAsync(CloseAction);
        var canSubmit = await HasPermissionAsync(SubmitAction);
        var canVerify = await HasPermissionAsync(VerifyAction);
        var today = AssetRegisterSettingReader.Today();

        foreach (var session in sessions)
        {
            var team = members.Where(x => x.SessionId == session.Id).Select(x => x.Member).ToList();
            var leader = team.FirstOrDefault(x => x.IsLeader);
            session.StatusName = AssetStockTakeLabels.Status(session.Status);
            session.LeaderEmployeeId = leader?.EmployeeId;
            session.LeaderName = leader?.EmployeeName;
            session.IsTeamMember = actor.EmployeeId is { } employeeId && team.Any(x => x.EmployeeId == employeeId);
            session.IsLeader = actor.EmployeeId is { } leaderId && leader?.EmployeeId == leaderId;
            session.IsPastPlannedEnd = session.PlannedEndDate < today && CancellableStatuses.Contains(session.Status);
            if (withDetail)
            {
                session.TeamMembers = team;
            }

            var actions = new List<string>();
            if (session.Status == StockTakeStatus.Dijadwalkan)
            {
                if (canCreate && actor.CanAccessUnit(session.OrganizationUnitId))
                {
                    actions.Add(ActionEdit);
                }

                if (canStart && CanStart(actor, session.OrganizationUnitId, session.LeaderEmployeeId, canCreate))
                {
                    actions.Add(ActionStart);
                }
            }

            var countingActor = canClose && CanStart(actor, session.OrganizationUnitId, session.LeaderEmployeeId, canCreate);
            if (session.Status == StockTakeStatus.Berjalan)
            {
                if (canScan && session.IsTeamMember)
                {
                    actions.Add(ActionScan);
                }

                if (countingActor)
                {
                    actions.Add(ActionCloseCounting);
                }
            }

            if (session.Status == StockTakeStatus.Pencocokan && countingActor)
            {
                actions.Add(ActionReopenCounting);
                actions.Add(ActionEditNote);
            }

            if (session.Status == StockTakeStatus.Pencocokan && canSubmit && CanStart(actor, session.OrganizationUnitId, session.LeaderEmployeeId, canCreate))
            {
                actions.Add(ActionSubmit);
            }

            if (session.Status == StockTakeStatus.MenungguVerifikasi && canVerify && !session.IsTeamMember && actor.CanAccessUnit(session.OrganizationUnitId))
            {
                actions.Add(ActionVerify);
            }

            if (session.Status == StockTakeStatus.Diverifikasi)
            {
                actions.Add(ActionDocument);
            }

            if (canCancel && CancellableStatuses.Contains(session.Status))
            {
                actions.Add(ActionCancel);
            }

            session.AvailableActions = actions;
        }

        if (!withDetail)
        {
            return;
        }

        foreach (var session in sessions.Where(x => x.Started != null))
        {
            session.FindingCount = await _context.AstStockTakeScans
                .AsNoTracking()
                .CountAsync(x => x.SessionId == session.Id && !x.IsDelete && x.IsLatest && x.IsUnregisteredFinding, cancellationToken);
            if (session.CountingClosed != null || session.Status is StockTakeStatus.MenungguVerifikasi or StockTakeStatus.Diverifikasi)
            {
                session.ResultCounts = await _matchingService.GetCountsAsync(session.Id, cancellationToken);
            }
        }

        foreach (var session in sessions.Where(x => x.Status == StockTakeStatus.Dijadwalkan))
        {
            var blockers = await FindStartBlockersAsync(session.OrganizationUnitId, cancellationToken);
            blockers.AvailableFrom = session.PlannedStartDate;
            blockers.CanStartToday = today >= session.PlannedStartDate;
            blockers.ActiveSessionNumber = await _context.AstStockTakeSessions
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.OrganizationUnitId == session.OrganizationUnitId && x.Id != session.Id &&
                            AssetStockTakeGate.HoldingStatuses.Contains(x.Status))
                .Select(x => x.SessionNumber)
                .FirstOrDefaultAsync(cancellationToken);
            session.StartBlockers = blockers;
        }
    }

    private async Task FillActorNamesAsync(IReadOnlyCollection<StockTakeSessionResponse> sessions, CancellationToken cancellationToken)
    {
        var steps = sessions
            .SelectMany(x => new[] { x.Created, x.Started, x.CountingClosed, x.Submitted, x.Verified, x.Cancelled })
            .OfType<StockTakeActorResponse>()
            .ToList();
        var userIds = steps.Select(x => x.UserId).Where(x => x != Guid.Empty).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var profiles = await _context.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DisplayName, PositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var step in steps)
        {
            if (profiles.TryGetValue(step.UserId, out var profile))
            {
                step.Name = profile.DisplayName;
                step.PositionName = profile.PositionName;
            }
        }
    }
}
