using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetUtilizationService
{
    public AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "createDateTime", SortDirection = "desc" },
        SortOptions = [new() { Value = "createDateTime", Label = "Tanggal Dibuat" }],
        FlagOptions =
        [
            AssetPlacementService.EnumOptions("status", "Status Pemanfaatan", Enum.GetValues<UtilizationStatus>(), AssetRegisterLabels.UtilizationStatus),
            AssetPlacementService.EnumOptions("utilizationType", "Jenis Pemanfaatan", Enum.GetValues<UtilizationType>(), AssetRegisterLabels.UtilizationType),
            AssetPlacementService.EnumOptions("requiredApprovalLevel", "Penyetuju", Enum.GetValues<UtilizationApprovalLevel>(), AssetRegisterLabels.UtilizationApprovalLevel)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor pemanfaatan, nomor perjanjian, nama mitra, dan nomor inventaris" },
            new() { Name = "status", Type = "integer[]", Description = "Lihat flagOptions status; boleh diulang, contoh status=2&status=3" },
            new() { Name = "utilizationType", Type = "integer", Description = "Lihat flagOptions utilizationType" },
            new() { Name = "requiredApprovalLevel", Type = "integer", Description = "Lihat flagOptions requiredApprovalLevel" },
            new() { Name = "assetId", Type = "guid", Description = "Pemanfaatan yang memuat satu aset" },
            new() { Name = "awaitingMyDecision", Type = "boolean", Description = "true = pengajuan yang menunggu keputusan pengguna ini", Example = "true" },
            new() { Name = "activeOn", Type = "date", Description = "Masa perjanjian mencakup tanggal ini", Example = "2026-11-18" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetUtilizationSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = Scoped(actor, entity.LegalEntityId);
        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        int CountOf(UtilizationStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new AssetUtilizationSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Draft = CountOf(UtilizationStatus.Draft),
            Submitted = CountOf(UtilizationStatus.Diajukan),
            AwaitingDirector = CountOf(UtilizationStatus.MenungguDirektur),
            Approved = CountOf(UtilizationStatus.Disetujui),
            Running = CountOf(UtilizationStatus.Berjalan),
            Ended = CountOf(UtilizationStatus.Berakhir),
            Rejected = CountOf(UtilizationStatus.Ditolak),
            Cancelled = CountOf(UtilizationStatus.Dibatalkan),
            AwaitingMyDecision = await AwaitingDecision(query, actor, await GetPermissionsAsync()).CountAsync(cancellationToken)
        };
    }

    public async Task<PagedResult<AssetUtilizationResponse>> GetUtilizationsAsync(AssetUtilizationPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var query = Scoped(actor, entity.LegalEntityId);

        var statuses = (request.Status ?? []).Where(x => Enum.IsDefined(x)).Distinct().ToList();
        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        if (request.UtilizationType is { } type)
        {
            query = query.Where(x => x.UtilizationType == type);
        }

        if (request.RequiredApprovalLevel is { } level)
        {
            query = query.Where(x => x.RequiredApprovalLevel == level);
        }

        if (request.AssetId is { } assetId && assetId != Guid.Empty)
        {
            query = query.Where(x => x.Items.Any(i => i.AssetId == assetId && !i.IsDelete));
        }

        if (request.AwaitingMyDecision == true)
        {
            query = AwaitingDecision(query, actor, await GetPermissionsAsync());
        }

        if (request.ActiveOn is { } activeOn)
        {
            query = query.Where(x => x.StartDate <= activeOn && x.EndDate >= activeOn);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.UtilizationNumber.ToUpper().Contains(keyword) ||
                x.AgreementNumber.ToUpper().Contains(keyword) ||
                x.PartnerName.ToUpper().Contains(keyword) ||
                x.Items.Any(i => !i.IsDelete && i.Asset!.InventoryNumber != null && i.Asset.InventoryNumber.ToUpper().Contains(keyword)));
        }

        var ordered = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.CreateDateTime)
            : query.OrderByDescending(x => x.CreateDateTime);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        await DescribeAsync(items, actor, false, cancellationToken);

        return new PagedResult<AssetUtilizationResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetUtilizationResponse> GetUtilizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var utilization = await Project(Scoped(actor, entity.LegalEntityId).Where(x => x.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

        if (utilization == null)
        {
            var exists = await _context.AstUtilizations.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            throw exists
                ? AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit)
                : UtilizationNotFound();
        }

        await DescribeAsync([utilization], actor, true, cancellationToken);
        return utilization;
    }

    private IQueryable<AstUtilization> Scoped(AssetActor actor, Guid legalEntityId)
    {
        var query = _context.AstUtilizations
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.Items.Any(i => !i.IsDelete && i.Asset!.LegalEntityId == legalEntityId));
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.ToList();
        var userId = actor.UserId;
        return query.Where(x => x.CreateBy == userId ||
            x.Items.Any(i => !i.IsDelete && i.Asset!.CurrentOrganizationUnitId.HasValue && unitIds.Contains(i.Asset.CurrentOrganizationUnitId.Value)));
    }

    private static IQueryable<AstUtilization> AwaitingDecision(IQueryable<AstUtilization> query, AssetActor actor, UtilizationPermissions permissions)
    {
        var userId = actor.UserId;
        var canApprove = permissions.CanApprove;
        var canApproveDirector = permissions.CanApproveDirector;
        return query.Where(x =>
            x.CreateBy != userId &&
            x.SubmittedBy != userId &&
            ((canApprove && x.Status == UtilizationStatus.Diajukan) ||
             (canApproveDirector && x.Status == UtilizationStatus.MenungguDirektur)));
    }

    private static IQueryable<AssetUtilizationResponse> Project(IQueryable<AstUtilization> query) =>
        query.Select(x => new AssetUtilizationResponse
        {
            Id = x.Id,
            UtilizationNumber = x.UtilizationNumber,
            UtilizationType = x.UtilizationType,
            PartnerName = x.PartnerName,
            AgreementNumber = x.AgreementNumber,
            AgreementValue = x.AgreementValue,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Purpose = x.Purpose,
            Status = x.Status,
            RequiredApprovalLevel = x.RequiredApprovalLevel,
            ThresholdUsed = x.ThresholdUsed,
            Agreement = x.AgreementAttachment != null
                ? new AssetUtilizationAttachmentResponse
                {
                    Id = x.AgreementAttachment.Id,
                    FileName = x.AgreementAttachment.OriginalFileName,
                    ContentType = x.AgreementAttachment.ContentType,
                    SizeBytes = x.AgreementAttachment.SizeBytes
                }
                : null,
            Created = new AssetUtilizationActorResponse { UserId = x.CreateBy, At = x.CreateDateTime },
            Submitted = x.SubmittedBy != null ? new AssetUtilizationActorResponse { UserId = x.SubmittedBy.Value, At = x.SubmittedAt!.Value } : null,
            AssetHeadDecided = x.AssetHeadDecidedBy != null ? new AssetUtilizationActorResponse { UserId = x.AssetHeadDecidedBy.Value, At = x.AssetHeadDecidedAt!.Value } : null,
            DirectorDecided = x.DirectorDecidedBy != null ? new AssetUtilizationActorResponse { UserId = x.DirectorDecidedBy.Value, At = x.DirectorDecidedAt!.Value } : null,
            Ended = x.EndedBy != null ? new AssetUtilizationActorResponse { UserId = x.EndedBy.Value, At = x.EndedAt!.Value } : null,
            DecisionReason = x.DecisionReason,
            AssetCount = x.Items.Count(i => !i.IsDelete),
            RowVersion = x.RowVersion,
            CreateDateTime = x.CreateDateTime
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetUtilizationResponse> utilizations, AssetActor actor, bool withItems, CancellationToken cancellationToken)
    {
        var steps = utilizations
            .SelectMany(x => new[] { x.Created, x.Submitted, x.AssetHeadDecided, x.DirectorDecided, x.Ended })
            .OfType<AssetUtilizationActorResponse>()
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

        var ids = utilizations.Select(x => x.Id).ToList();
        var items = await _context.AstUtilizationItems
            .AsNoTracking()
            .Where(x => ids.Contains(x.UtilizationId) && !x.IsDelete)
            .OrderBy(x => x.Asset!.InventoryNumber)
            .ThenBy(x => x.Asset!.AssetName)
            .Select(x => new
            {
                x.UtilizationId,
                Item = new AssetUtilizationItemResponse
                {
                    AssetId = x.AssetId,
                    InventoryNumber = x.Asset!.InventoryNumber,
                    AssetName = x.Asset.AssetName,
                    GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
                    OwnershipTypeName = x.Asset.OwnershipType != null ? x.Asset.OwnershipType.OwnershipName : null,
                    IsOwnedByHospital = x.Asset.OwnershipType != null && x.Asset.OwnershipType.IsCapitalizable,
                    RecordStatus = x.Asset.RecordStatus,
                    Condition = x.Asset.Condition,
                    ActiveProcessKind = x.Asset.ActiveProcessKind,
                    ActiveProcessId = x.Asset.ActiveProcessId,
                    OrganizationUnitName = x.Asset.CurrentOrganizationUnit != null ? x.Asset.CurrentOrganizationUnit.UnitName : null,
                    LocationName = x.Asset.CurrentLocation != null ? x.Asset.CurrentLocation.LocationName : null
                }
            })
            .ToListAsync(cancellationToken);

        var permissions = await GetPermissionsAsync();
        foreach (var utilization in utilizations)
        {
            utilization.UtilizationTypeName = AssetRegisterLabels.UtilizationType(utilization.UtilizationType);
            utilization.StatusName = AssetRegisterLabels.UtilizationStatus(utilization.Status);
            utilization.RequiredApprovalLevelName = utilization.RequiredApprovalLevel is { } level ? AssetRegisterLabels.UtilizationApprovalLevel(level) : null;
            utilization.DurationMonths = DurationMonths(utilization.StartDate, utilization.EndDate);
            utilization.Items = withItems ? items.Where(x => x.UtilizationId == utilization.Id).Select(x => x.Item).ToList() : [];
            foreach (var item in utilization.Items)
            {
                item.RecordStatusName = AssetRegisterLabels.RecordStatus(item.RecordStatus);
                item.ConditionName = AssetRegisterLabels.Condition(item.Condition);
            }

            var isOwner = utilization.Created?.UserId == actor.UserId;
            var isSubmitter = isOwner || utilization.Submitted?.UserId == actor.UserId;
            var actions = new List<string>();
            switch (utilization.Status)
            {
                case UtilizationStatus.Draft:
                    if (isOwner && permissions.CanCreate)
                    {
                        actions.AddRange(["update", "uploadAgreement", "cancel"]);
                    }

                    if (isOwner && permissions.CanSubmit)
                    {
                        actions.Add("submit");
                    }

                    break;
                case UtilizationStatus.Diajukan:
                    if (permissions.CanApprove && !isSubmitter)
                    {
                        actions.Add(utilization.RequiredApprovalLevel == UtilizationApprovalLevel.Direktur ? "forward" : "approve");
                        actions.Add("reject");
                    }

                    if (isOwner && permissions.CanCreate)
                    {
                        actions.AddRange(["uploadAgreement", "cancel"]);
                    }

                    break;
                case UtilizationStatus.MenungguDirektur:
                    if (permissions.CanApproveDirector && !isSubmitter)
                    {
                        actions.AddRange(["approveDirector", "rejectDirector"]);
                    }

                    if (permissions.CanCreate)
                    {
                        actions.Add("uploadAgreement");
                    }

                    break;
                case UtilizationStatus.Disetujui:
                case UtilizationStatus.Berjalan:
                    if (permissions.CanEnd)
                    {
                        actions.Add("end");
                    }

                    if (permissions.CanCreate)
                    {
                        actions.Add("uploadAgreement");
                    }

                    break;
            }

            utilization.IsAwaitingMyDecision = actions.Contains("approve") || actions.Contains("forward") || actions.Contains("approveDirector");
            utilization.AvailableActions = actions;
        }
    }
}
