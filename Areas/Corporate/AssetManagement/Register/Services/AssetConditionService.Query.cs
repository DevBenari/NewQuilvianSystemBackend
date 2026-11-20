using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetConditionService
{
    public AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "reportedAt", SortDirection = "desc" },
        SortOptions = [new() { Value = "reportedAt", Label = "Tanggal Dilaporkan" }],
        FlagOptions =
        [
            AssetPlacementService.EnumOptions("status", "Status Laporan", Enum.GetValues<ConditionReportStatus>(), AssetRegisterLabels.ConditionReportStatus),
            AssetPlacementService.EnumOptions("reportedCondition", "Kondisi Dilaporkan", Enum.GetValues<AssetCondition>(), AssetRegisterLabels.Condition),
            AssetPlacementService.EnumOptions("source", "Sumber Laporan", Enum.GetValues<ConditionReportSource>(), AssetRegisterLabels.ConditionReportSource)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor laporan, nomor inventaris, dan nama aset" },
            new() { Name = "status", Type = "integer", Description = "Lihat flagOptions status" },
            new() { Name = "reportedCondition", Type = "integer", Description = "Lihat flagOptions reportedCondition" },
            new() { Name = "source", Type = "integer", Description = "Lihat flagOptions source" },
            new() { Name = "isFoundAgain", Type = "boolean", Description = "true = hanya laporan aset hilang yang ditemukan kembali" },
            new() { Name = "awaitingLossApproval", Type = "boolean", Description = "true = laporan hilang atau ditemukan kembali yang menunggu persetujuan Kepala Bagian Aset", Example = "true" },
            new() { Name = "assetId", Type = "guid", Description = "Laporan untuk satu aset" },
            new() { Name = "organizationUnitId", Type = "guid", Description = "Unit aset saat ini" },
            new() { Name = "from", Type = "date", Description = "Tanggal dilaporkan paling awal (WIB), inklusif", Example = "2026-11-18" },
            new() { Name = "to", Type = "date", Description = "Tanggal dilaporkan paling akhir (WIB), inklusif", Example = "2026-11-18" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetConditionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = ScopedReports(actor, entity.LegalEntityId);

        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        var open = query.Where(x => x.Status == ConditionReportStatus.Dilaporkan);
        var awaitingLoss = await open.CountAsync(x => x.ReportedCondition == AssetCondition.Hilang || x.IsFoundAgain, cancellationToken);
        var awaitingDecision = await open.CountAsync(x => x.ReportedCondition != AssetCondition.Hilang && !x.IsFoundAgain, cancellationToken);

        int CountOf(ConditionReportStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new AssetConditionSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Reported = CountOf(ConditionReportStatus.Dilaporkan),
            Decided = CountOf(ConditionReportStatus.Ditetapkan),
            Rejected = CountOf(ConditionReportStatus.Ditolak),
            AwaitingDecision = awaitingDecision,
            AwaitingLossApproval = awaitingLoss
        };
    }

    public async Task<PagedResult<AssetConditionReportResponse>> GetReportsAsync(AssetConditionPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = ScopedReports(actor, entity.LegalEntityId);

        if (request.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (request.ReportedCondition is { } reported)
        {
            query = query.Where(x => x.ReportedCondition == reported);
        }

        if (request.Source is { } source)
        {
            query = query.Where(x => x.Source == source);
        }

        if (request.IsFoundAgain is { } foundAgain)
        {
            query = query.Where(x => x.IsFoundAgain == foundAgain);
        }

        if (request.AwaitingLossApproval == true)
        {
            query = query.Where(x => x.Status == ConditionReportStatus.Dilaporkan && (x.ReportedCondition == AssetCondition.Hilang || x.IsFoundAgain));
        }

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            query = query.Where(x => x.AssetId == assetId);
        }

        if (request.OrganizationUnitId is { } unitId && unitId != Guid.Empty)
        {
            query = query.Where(x => x.Asset!.CurrentOrganizationUnitId == unitId);
        }

        if (request.From is { } from)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(from.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.ReportedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.ReportedAt < toUtcExclusive);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.ReportNumber.ToUpper().Contains(keyword) ||
                (x.Asset!.InventoryNumber != null && x.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                x.Asset.AssetName.ToUpper().Contains(keyword));
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.ReportedAt)
            : query.OrderByDescending(x => x.ReportedAt);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, cancellationToken);

        return new PagedResult<AssetConditionReportResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetConditionReportResponse> GetReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var report = await Project(_context.AstConditionReports.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete && x.Asset!.LegalEntityId == entity.LegalEntityId))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ReportNotFound();

        if (!CanSeeReport(report.OrganizationUnitId, report.CustodianEmployeeId, report.Reported?.UserId ?? Guid.Empty, actor))
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.ConditionReportOutsideScope);
        }

        await DescribeAsync([report], actor, cancellationToken);
        return report;
    }

    private IQueryable<AstConditionReport> ScopedReports(AssetActor actor, Guid legalEntityId)
    {
        var query = _context.AstConditionReports
            .AsNoTracking()
            .Where(x => !x.IsDelete && !x.Asset!.IsDelete && x.Asset.LegalEntityId == legalEntityId);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        var employeeId = actor.EmployeeId;
        var userId = actor.UserId;
        return query.Where(x =>
            (x.Asset!.CurrentOrganizationUnitId.HasValue && unitIds.Contains(x.Asset.CurrentOrganizationUnitId.Value)) ||
            (employeeId.HasValue && x.Asset.CurrentCustodianEmployeeId == employeeId.Value) ||
            x.ReportedBy == userId);
    }

    private static IQueryable<AssetConditionReportResponse> Project(IQueryable<AstConditionReport> query) =>
        query.Select(x => new AssetConditionReportResponse
        {
            Id = x.Id,
            ReportNumber = x.ReportNumber,
            AssetId = x.AssetId,
            InventoryNumber = x.Asset!.InventoryNumber,
            AssetName = x.Asset.AssetName,
            GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
            OrganizationUnitId = x.Asset.CurrentOrganizationUnitId,
            OrganizationUnitName = x.Asset.CurrentOrganizationUnit != null ? x.Asset.CurrentOrganizationUnit.UnitName : null,
            LocationName = x.Asset.CurrentLocation != null ? x.Asset.CurrentLocation.LocationName : null,
            CustodianEmployeeId = x.Asset.CurrentCustodianEmployeeId,
            CustodianEmployeeName = x.Asset.CurrentCustodianEmployee != null ? x.Asset.CurrentCustodianEmployee.FullName : null,
            CurrentCondition = x.Asset.Condition,
            Source = x.Source,
            SourceDocumentId = x.SourceDocumentId,
            PreviousCondition = x.PreviousCondition,
            ReportedCondition = x.ReportedCondition,
            IsFoundAgain = x.IsFoundAgain,
            Notes = x.Notes,
            Photo = x.PhotoAttachment != null
                ? new AssetConditionAttachmentResponse
                {
                    Id = x.PhotoAttachment.Id,
                    FileName = x.PhotoAttachment.OriginalFileName,
                    ContentType = x.PhotoAttachment.ContentType,
                    SizeBytes = x.PhotoAttachment.SizeBytes
                }
                : null,
            LossReport = x.LossReportAttachment != null
                ? new AssetConditionAttachmentResponse
                {
                    Id = x.LossReportAttachment.Id,
                    FileName = x.LossReportAttachment.OriginalFileName,
                    ContentType = x.LossReportAttachment.ContentType,
                    SizeBytes = x.LossReportAttachment.SizeBytes
                }
                : null,
            Status = x.Status,
            Reported = new AssetConditionActorResponse { UserId = x.ReportedBy, At = x.ReportedAt },
            Decided = x.DecidedBy != null ? new AssetConditionActorResponse { UserId = x.DecidedBy.Value, At = x.DecidedAt!.Value } : null,
            DecidedCondition = x.DecidedCondition,
            DecisionReason = x.DecisionReason,
            RowVersion = x.RowVersion
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetConditionReportResponse> reports, AssetActor actor, CancellationToken cancellationToken)
    {
        var steps = reports
            .SelectMany(x => new[] { x.Reported, x.Decided })
            .OfType<AssetConditionActorResponse>()
            .ToList();
        var userIds = steps.Select(x => x.UserId).Where(x => x != Guid.Empty).Distinct().ToList();
        if (userIds.Count > 0)
        {
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

        var permissions = await GetPermissionsAsync();
        foreach (var report in reports)
        {
            report.CurrentConditionName = AssetRegisterLabels.Condition(report.CurrentCondition);
            report.SourceName = AssetRegisterLabels.ConditionReportSource(report.Source);
            report.PreviousConditionName = AssetRegisterLabels.Condition(report.PreviousCondition);
            report.ReportedConditionName = AssetRegisterLabels.Condition(report.ReportedCondition);
            report.DecidedConditionName = report.DecidedCondition is { } decided ? AssetRegisterLabels.Condition(decided) : null;
            report.StatusName = AssetRegisterLabels.ConditionReportStatus(report.Status);
            report.RequiresLossApproval = report.ReportedCondition == AssetCondition.Hilang || report.IsFoundAgain;
            report.AvailableActions = [];

            if (report.Status == ConditionReportStatus.Dilaporkan)
            {
                if (report.RequiresLossApproval)
                {
                    var ownLoss = report.ReportedCondition == AssetCondition.Hilang && report.Reported?.UserId == actor.UserId;
                    if (permissions.CanApproveLoss && !ownLoss)
                    {
                        report.AvailableActions.Add("approveLoss");
                    }
                }
                else if (permissions.CanDecide)
                {
                    report.AvailableActions.Add("decide");
                }

                if (permissions.CanDecide)
                {
                    report.AvailableActions.Add("reject");
                }
            }

            if (report.Status == ConditionReportStatus.Ditetapkan && report.DecidedCondition == AssetCondition.Hilang)
            {
                report.AvailableActions.Add("lossDocument");
            }
        }
    }
}
