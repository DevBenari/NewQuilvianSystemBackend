using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed class StockTakeQueryService
{
    public const string OverdueCardKey = "overdueSessions";
    public const string OpenFollowUpsCardKey = "openFollowUps";

    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly StockTakeSessionService _sessionService;

    public StockTakeQueryService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        StockTakeSessionService sessionService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public async Task<StockTakeSummaryResponse> GetSummaryAsync(StockTakeDashboardQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var unitId = request.OrganizationUnitId is { } requested && requested != Guid.Empty ? requested : (Guid?)null;
        if (unitId.HasValue)
        {
            actor.EnsureCanAccessUnit(unitId);
        }

        var sessions = _sessionService.ScopedFor(actor);
        if (unitId is { } scopedUnit)
        {
            sessions = sessions.Where(x => x.OrganizationUnitId == scopedUnit);
        }

        var overdue = await StockTakeSessionService.Overdue(sessions, AssetRegisterSettingReader.Today()).CountAsync(cancellationToken);
        var sessionIds = sessions.Select(x => x.Id);
        var openFollowUps = _context.AstStockTakeFollowUps
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == StockTakeFollowUpStatus.Usulan && sessionIds.Contains(x.SessionId));
        var open = await openFollowUps.CountAsync(cancellationToken);
        var sessionsWithOpen = await openFollowUps.Select(x => x.SessionId).Distinct().CountAsync(cancellationToken);

        return new StockTakeSummaryResponse
        {
            OrganizationUnitId = unitId,
            OrganizationUnitName = unitId is { } id ? await UnitNameAsync(id, cancellationToken) : null,
            CanReadAllUnits = actor.CanReadAllUnits,
            PendingTotal = overdue + open,
            OverdueSessions = overdue,
            OpenFollowUps = open,
            SessionsWithOpenFollowUps = sessionsWithOpen,
            Cards =
            [
                Card(OverdueCardKey, "Opname Lewat Jadwal", overdue, ("overdue", "true")),
                Card(OpenFollowUpsCardKey, "Selisih Belum Ditindaklanjuti", open, ("hasOpenFollowUps", "true"))
            ],
            GeneratedAt = DateTime.UtcNow
        };
    }

    public static AssetMasterFilterMetadataResponse GetReportMetadata() => StockTakeSessionService.GetMetadata();

    public async Task<PagedResult<StockTakeSessionResponse>> GetReportAsync(StockTakePagedQuery request, CancellationToken cancellationToken = default)
    {
        var page = await _sessionService.GetSessionsAsync(request, cancellationToken);
        await FillCountsAsync(page.Items, cancellationToken);
        return page;
    }

    public async Task<AssetReportFile> ExportReportAsync(StockTakePagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var (total, items) = await _sessionService.ListForExportAsync(request, AssetReportQueryService.MaxExportRows, cancellationToken);
        if (total > AssetReportQueryService.MaxExportRows)
        {
            throw AssetManagementException.Invalid(
                AssetRegisterErrorCodes.ReportExportLimit,
                string.Format(Indonesian, AssetRegisterMessages.ReportExportLimitFormat, total, AssetReportQueryService.MaxExportRows));
        }

        await FillCountsAsync(items, cancellationToken);
        var printedBy = await _context.Users.AsNoTracking().Where(x => x.Id == actor.UserId).Select(x => x.DisplayName).FirstOrDefaultAsync(cancellationToken) ?? "-";
        var infoLines = new List<string>
        {
            $"Dicetak: {ToReportTime(DateTime.UtcNow).ToString("dd MMMM yyyy HH:mm", Indonesian)} WIB oleh {printedBy}",
            await FilterDescriptionAsync(request, cancellationToken),
            $"Jumlah sesi: {total.ToString("N0", Indonesian)}"
        };

        var sheet = new AssetReportSheet(
            "Laporan Inventaris Aset",
            infoLines,
            [
                new("No"), new("Nomor Sesi"), new("Unit"), new("Ruangan"), new("Rencana Mulai"), new("Rencana Selesai"), new("Status"),
                new("Lewat Jadwal"), new("Ketua Tim"), new("Jumlah Tim"), new("Aset Diharapkan"), new("Aset Dipindai"), new("Sesuai"),
                new("Sesuai Sedang Dipinjam"), new("Tidak Ditemukan"), new("Beda Lokasi"), new("Beda Kondisi"), new("Tidak Terdaftar"),
                new("Jumlah Selisih"), new("Usulan Terbuka"), new("Usulan Diproses"), new("Usulan Diabaikan"), new("Dimulai"),
                new("Diajukan Oleh"), new("Diverifikasi Oleh"), new("Tanggal Verifikasi"), new("Alasan Keputusan")
            ],
            items.Select((x, index) => new object?[]
            {
                index + 1,
                x.SessionNumber,
                x.OrganizationUnitName,
                x.LocationName ?? "Seluruh unit",
                x.PlannedStartDate,
                x.PlannedEndDate,
                x.StatusName,
                x.IsPastPlannedEnd ? "Ya" : "Tidak",
                x.LeaderName,
                x.TeamSize,
                x.ExpectedCount,
                x.ScannedCount,
                x.ResultCounts?.Sesuai,
                x.ResultCounts?.SesuaiSedangDipinjam,
                x.ResultCounts?.TidakDitemukan,
                x.ResultCounts?.BedaLokasi,
                x.ResultCounts?.BedaKondisi,
                x.ResultCounts?.TidakTerdaftar,
                x.ResultCounts?.Discrepancies,
                x.FollowUpCounts?.Open,
                x.FollowUpCounts?.Processed,
                x.FollowUpCounts?.Dismissed,
                x.Started is { } started ? ToReportTime(started.At) : null,
                x.Submitted?.Name,
                x.Verified?.Name,
                x.Verified is { } verified ? ToReportTime(verified.At) : null,
                x.DecisionReason
            }).ToList());

        var stamp = ToReportTime(DateTime.UtcNow).ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture);
        return new AssetReportFile(AssetReportWorkbook.Build(sheet), $"laporan-inventaris-aset-{stamp}.xlsx");
    }

    private async Task FillCountsAsync(IReadOnlyCollection<StockTakeSessionResponse> sessions, CancellationToken cancellationToken)
    {
        if (sessions.Count == 0)
        {
            return;
        }

        var ids = sessions.Select(x => x.Id).ToList();
        var results = (await _context.AstStockTakeResults
                .AsNoTracking()
                .Where(x => ids.Contains(x.SessionId) && !x.IsDelete)
                .GroupBy(x => new { x.SessionId, x.Category })
                .Select(x => new { x.Key.SessionId, x.Key.Category, Count = x.Count() })
                .ToListAsync(cancellationToken))
            .ToLookup(x => x.SessionId);
        var followUps = (await _context.AstStockTakeFollowUps
                .AsNoTracking()
                .Where(x => ids.Contains(x.SessionId) && !x.IsDelete)
                .GroupBy(x => new { x.SessionId, x.Status })
                .Select(x => new { x.Key.SessionId, x.Key.Status, Count = x.Count() })
                .ToListAsync(cancellationToken))
            .ToLookup(x => x.SessionId);

        foreach (var session in sessions)
        {
            var categories = results[session.Id].ToList();
            if (categories.Count > 0)
            {
                int CategoryCount(StockTakeResultCategory category) => categories.Where(x => x.Category == category).Sum(x => x.Count);
                session.ResultCounts = new StockTakeResultCountsResponse
                {
                    Total = categories.Sum(x => x.Count),
                    Sesuai = CategoryCount(StockTakeResultCategory.Sesuai),
                    SesuaiSedangDipinjam = CategoryCount(StockTakeResultCategory.SesuaiSedangDipinjam),
                    TidakDitemukan = CategoryCount(StockTakeResultCategory.TidakDitemukan),
                    BedaLokasi = CategoryCount(StockTakeResultCategory.BedaLokasi),
                    BedaKondisi = CategoryCount(StockTakeResultCategory.BedaKondisi),
                    TidakTerdaftar = CategoryCount(StockTakeResultCategory.TidakTerdaftar),
                    Discrepancies = categories.Where(x => StockTakeSessionService.DiscrepancyCategories.Contains(x.Category)).Sum(x => x.Count)
                };
            }

            var statuses = followUps[session.Id].ToList();
            if (statuses.Count > 0)
            {
                int StatusCount(StockTakeFollowUpStatus status) => statuses.Where(x => x.Status == status).Sum(x => x.Count);
                session.FollowUpCounts = new StockTakeFollowUpCountsResponse
                {
                    Total = statuses.Sum(x => x.Count),
                    Open = StatusCount(StockTakeFollowUpStatus.Usulan),
                    Processed = StatusCount(StockTakeFollowUpStatus.Diproses),
                    Dismissed = StatusCount(StockTakeFollowUpStatus.Diabaikan)
                };
            }
        }
    }

    private async Task<string> FilterDescriptionAsync(StockTakePagedQuery request, CancellationToken cancellationToken)
    {
        var parts = new List<string>();
        var statuses = (request.Status ?? []).Distinct().ToList();
        if (statuses.Count > 0)
        {
            parts.Add($"Status: {string.Join(", ", statuses.Select(AssetStockTakeLabels.Status))}");
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            parts.Add($"Unit: {await UnitNameAsync(unitId, cancellationToken)}");
        }

        if (request.Year is { } year && year is >= 2000 and <= 2100)
        {
            parts.Add($"Tahun rencana mulai: {year}");
        }

        if (request.Overdue == true)
        {
            parts.Add("Hanya sesi lewat jadwal yang belum diverifikasi");
        }

        if (request.HasOpenFollowUps == true)
        {
            parts.Add("Hanya sesi dengan selisih belum ditindaklanjuti");
        }

        if (request.Mine == true)
        {
            parts.Add("Hanya sesi tempat pencetak menjadi anggota tim");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            parts.Add($"Kata kunci: {request.Search.Trim()}");
        }

        return parts.Count == 0 ? "Saringan: seluruh sesi inventarisasi yang dapat Anda lihat" : $"Saringan: {string.Join("; ", parts)}";
    }

    private async Task<string> UnitNameAsync(Guid unitId, CancellationToken cancellationToken) =>
        await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken) ?? "-";

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private static AssetDashboardCardResponse Card(string key, string label, int count, params (string Name, string Value)[] filters)
    {
        var card = new AssetDashboardCardResponse { Key = key, Label = label, Count = count };
        foreach (var (name, value) in filters)
        {
            card.Filter[name] = value;
        }

        return card;
    }

    private static DateTime ToReportTime(DateTime value) =>
        TimeZoneInfo.ConvertTimeFromUtc(AppDateTimeHelper.ToUtc(value), ReportTimeZone);
}
