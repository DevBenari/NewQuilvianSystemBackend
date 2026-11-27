using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed class MovementQueryService
{
    public const string LoanPermissionResource = "AssetLoan";
    public const string ReadAction = "Read";
    private const int MaxInFlightNumbers = 50;

    private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

    private static readonly TransferStatus[] InFlightTransferStatuses =
        [TransferStatus.Diajukan, TransferStatus.DisetujuiUnitAsal, TransferStatus.DisetujuiBagianAset];

    private static readonly LoanStatus[] InFlightLoanStatuses =
        [LoanStatus.Diajukan, LoanStatus.Disetujui, LoanStatus.Dipinjam, LoanStatus.MenungguKonfirmasiKembali];

    private static readonly LoanStatus[] StockTakeBlockingLoanStatuses =
        [LoanStatus.Diajukan, LoanStatus.Disetujui, LoanStatus.MenungguKonfirmasiKembali];

    private readonly ApplicationDbContext _context;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetTransferService _transferService;
    private readonly AssetLoanService _loanService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MovementQueryService(
        ApplicationDbContext context,
        AssetActorResolver actorResolver,
        AssetTransferService transferService,
        AssetLoanService loanService,
        AccessPermissionService accessPermissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
        _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<AssetMovementSummaryResponse> GetSummaryAsync(AssetMovementSummaryQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var unitId = request.OrganizationUnitId is { } requested && requested != Guid.Empty ? requested : (Guid?)null;
        if (unitId.HasValue)
        {
            actor.EnsureCanAccessUnit(unitId);
        }

        var transfers = _transferService.ScopedFor(actor).Where(x => InFlightTransferStatuses.Contains(x.Status));
        if (unitId is { } transferUnit)
        {
            transfers = transfers.Where(x => x.SourceOrganizationUnitId == transferUnit || x.TargetOrganizationUnitId == transferUnit);
        }

        var transferCounts = await transfers
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        int TransferCount(TransferStatus status) => transferCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        var response = new AssetMovementSummaryResponse
        {
            OrganizationUnitId = unitId,
            OrganizationUnitName = unitId is { } id ? await UnitNameAsync(id, cancellationToken) : null,
            CanReadAllUnits = actor.CanReadAllUnits,
            TransfersInFlight = transferCounts.Sum(x => x.Count),
            Transfers =
            [
                Card("transferSubmitted", "Mutasi Menunggu Unit Asal", TransferCount(TransferStatus.Diajukan), ("status", nameof(TransferStatus.Diajukan))),
                Card("transferUnitApproved", "Mutasi Menunggu Bagian Aset", TransferCount(TransferStatus.DisetujuiUnitAsal), ("status", nameof(TransferStatus.DisetujuiUnitAsal))),
                Card("transferAwaitingReceipt", "Mutasi Menunggu Diterima", TransferCount(TransferStatus.DisetujuiBagianAset), ("status", nameof(TransferStatus.DisetujuiBagianAset)))
            ],
            GeneratedAt = DateTime.UtcNow
        };

        response.CanReadLoans = await HasAccessAsync(LoanPermissionResource, ReadAction);
        if (!response.CanReadLoans)
        {
            return response;
        }

        var loans = _loanService.ScopedFor(actor);
        if (unitId is { } loanUnit)
        {
            loans = loans.Where(x => x.OwnerOrganizationUnitId == loanUnit || x.BorrowerOrganizationUnitId == loanUnit);
        }

        var loanCounts = await loans
            .Where(x => InFlightLoanStatuses.Contains(x.Status))
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        int LoanCount(params LoanStatus[] statuses) => loanCounts.Where(x => statuses.Contains(x.Status)).Sum(x => x.Count);
        var overdue = await AssetLoanService.Overdue(loans, AssetRegisterSettingReader.Today()).CountAsync(cancellationToken);

        response.Loans =
        [
            Card("loanSubmitted", "Pinjaman Menunggu Persetujuan", LoanCount(LoanStatus.Diajukan), ("status", nameof(LoanStatus.Diajukan))),
            Card("loanApproved", "Pinjaman Menunggu Diambil", LoanCount(LoanStatus.Disetujui), ("status", nameof(LoanStatus.Disetujui))),
            Card("loanActive", "Pinjaman Aktif", LoanCount(LoanStatus.Dipinjam, LoanStatus.MenungguKonfirmasiKembali),
                ("status", $"{nameof(LoanStatus.Dipinjam)},{nameof(LoanStatus.MenungguKonfirmasiKembali)}")),
            Card("loanOverdue", "Pinjaman Terlambat", overdue, ("isOverdue", "true"))
        ];
        return response;
    }

    public static AssetMasterFilterMetadataResponse GetTransferReportMetadata() => AssetTransferService.GetMetadata();

    public Task<PagedResult<AssetTransferResponse>> GetTransferReportAsync(AssetTransferPagedQuery request, CancellationToken cancellationToken = default) =>
        _transferService.GetTransfersAsync(request, cancellationToken);

    public async Task<AssetReportFile> ExportTransferReportAsync(AssetTransferPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var (total, items) = await _transferService.ListForExportAsync(request, AssetReportQueryService.MaxExportRows, cancellationToken);
        if (total > AssetReportQueryService.MaxExportRows)
        {
            throw AssetManagementException.Invalid(
                AssetRegisterErrorCodes.ReportExportLimit,
                string.Format(Indonesian, AssetRegisterMessages.ReportExportLimitFormat, total, AssetReportQueryService.MaxExportRows));
        }

        var printedBy = await _context.Users.AsNoTracking().Where(x => x.Id == actor.UserId).Select(x => x.DisplayName).FirstOrDefaultAsync(cancellationToken) ?? "-";
        var infoLines = new List<string>
        {
            $"Dicetak: {ToReportTime(DateTime.UtcNow).ToString("dd MMMM yyyy HH:mm", Indonesian)} WIB oleh {printedBy}",
            await FilterDescriptionAsync(request, cancellationToken),
            $"Jumlah mutasi: {total.ToString("N0", Indonesian)}"
        };

        var sheet = new AssetReportSheet(
            "Laporan Mutasi Aset",
            infoLines,
            [
                new("No"), new("Nomor Mutasi"), new("Tanggal Diajukan"), new("Unit Asal"), new("Unit Tujuan"), new("Ruangan Tujuan"),
                new("Penanggung Jawab Tujuan"), new("Alasan"), new("Catatan Alasan"), new("Mutasi yang Dikoreksi"), new("Jumlah Aset"),
                new("Status"), new("Diajukan Oleh"), new("Disetujui Unit Asal"), new("Disetujui Bagian Aset"), new("Diterima Oleh"),
                new("Tanggal Diterima"), new("Ditolak/Dibatalkan Oleh"), new("Alasan Keputusan")
            ],
            items.Select((x, index) => new object?[]
            {
                index + 1,
                x.TransferNumber,
                x.Requested is { } requested ? ToReportTime(requested.At) : null,
                x.SourceOrganizationUnitName,
                x.TargetOrganizationUnitName,
                x.TargetLocationName,
                x.TargetCustodianEmployeeName,
                x.ReasonName,
                x.ReasonNote,
                x.CorrectsTransferNumber,
                x.ItemCount,
                x.StatusName,
                x.Requested?.Name,
                x.UnitApproved?.Name,
                x.AssetHeadApproved?.Name,
                x.Received?.Name,
                x.Received is { } received ? ToReportTime(received.At) : null,
                (x.Rejected ?? x.Cancelled)?.Name,
                x.DecisionReason
            }).ToList());

        var stamp = ToReportTime(DateTime.UtcNow).ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture);
        return new AssetReportFile(AssetReportWorkbook.Build(sheet), $"laporan-mutasi-aset-{stamp}.xlsx");
    }

    public async Task<bool> HasInFlightForUnitAsync(Guid organizationUnitId, CancellationToken cancellationToken = default)
    {
        if (organizationUnitId == Guid.Empty)
        {
            return false;
        }

        var transfer = await _context.AstTransfers.AsNoTracking().AnyAsync(
            x => !x.IsDelete && InFlightTransferStatuses.Contains(x.Status) &&
                 (x.SourceOrganizationUnitId == organizationUnitId || x.TargetOrganizationUnitId == organizationUnitId),
            cancellationToken);
        return transfer || await _context.AstLoans.AsNoTracking().AnyAsync(
            x => !x.IsDelete && StockTakeBlockingLoanStatuses.Contains(x.Status) &&
                 (x.OwnerOrganizationUnitId == organizationUnitId || x.BorrowerOrganizationUnitId == organizationUnitId),
            cancellationToken);
    }

    public async Task<AssetMovementInFlightResult> FindInFlightForUnitAsync(Guid organizationUnitId, CancellationToken cancellationToken = default)
    {
        if (organizationUnitId == Guid.Empty)
        {
            return new AssetMovementInFlightResult([], []);
        }

        var transfers = await _context.AstTransfers.AsNoTracking()
            .Where(x => !x.IsDelete && InFlightTransferStatuses.Contains(x.Status) &&
                        (x.SourceOrganizationUnitId == organizationUnitId || x.TargetOrganizationUnitId == organizationUnitId))
            .OrderBy(x => x.RequestedAt)
            .Select(x => x.TransferNumber)
            .Take(MaxInFlightNumbers)
            .ToListAsync(cancellationToken);
        var loans = await _context.AstLoans.AsNoTracking()
            .Where(x => !x.IsDelete && StockTakeBlockingLoanStatuses.Contains(x.Status) &&
                        (x.OwnerOrganizationUnitId == organizationUnitId || x.BorrowerOrganizationUnitId == organizationUnitId))
            .OrderBy(x => x.RequestedAt)
            .Select(x => x.LoanNumber)
            .Take(MaxInFlightNumbers)
            .ToListAsync(cancellationToken);
        return new AssetMovementInFlightResult(transfers, loans);
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> FindLoanedOutAssetsAsync(Guid ownerOrganizationUnitId, CancellationToken cancellationToken = default)
    {
        if (ownerOrganizationUnitId == Guid.Empty)
        {
            return new Dictionary<Guid, Guid>();
        }

        var rows = await _context.AstLoanItems.AsNoTracking()
            .Where(x => !x.IsDelete && !x.Loan!.IsDelete &&
                        x.Loan.Status == LoanStatus.Dipinjam &&
                        x.Loan.OwnerOrganizationUnitId == ownerOrganizationUnitId)
            .Select(x => new { x.AssetId, x.Loan!.BorrowerOrganizationUnitId })
            .ToListAsync(cancellationToken);
        return rows
            .GroupBy(x => x.AssetId)
            .ToDictionary(x => x.Key, x => x.First().BorrowerOrganizationUnitId);
    }

    private async Task<string> FilterDescriptionAsync(AssetTransferPagedQuery request, CancellationToken cancellationToken)
    {
        var parts = new List<string>();
        if (request.Status is { } status)
        {
            parts.Add($"Status: {AssetMovementLabels.TransferStatus(status)}");
        }

        if (request.Reason is { } reason)
        {
            parts.Add($"Alasan: {AssetMovementLabels.TransferReason(reason)}");
        }

        if (request.SourceOrganizationUnitId is { } sourceId && sourceId != Guid.Empty)
        {
            parts.Add($"Unit asal: {await UnitNameAsync(sourceId, cancellationToken)}");
        }

        if (request.TargetOrganizationUnitId is { } targetId && targetId != Guid.Empty)
        {
            parts.Add($"Unit tujuan: {await UnitNameAsync(targetId, cancellationToken)}");
        }

        if (request.From.HasValue || request.To.HasValue)
        {
            var from = request.From?.ToString("dd MMMM yyyy", Indonesian) ?? "awal";
            var to = request.To?.ToString("dd MMMM yyyy", Indonesian) ?? "sekarang";
            parts.Add($"Periode diajukan: {from} s.d. {to}");
        }

        if (request.AwaitingMe == true)
        {
            parts.Add("Hanya yang menunggu tindakan pencetak");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            parts.Add($"Kata kunci: {request.Search.Trim()}");
        }

        return parts.Count == 0 ? "Saringan: seluruh mutasi yang dapat Anda lihat" : $"Saringan: {string.Join("; ", parts)}";
    }

    private async Task<string> UnitNameAsync(Guid unitId, CancellationToken cancellationToken) =>
        await _context.MstOrganizationUnits.AsNoTracking().Where(x => x.Id == unitId).Select(x => x.UnitName).FirstOrDefaultAsync(cancellationToken) ?? "-";

    private async Task<bool> HasAccessAsync(string resource, string action)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && await _accessPermissionService.HasAccessAsync(user, resource, action);
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
