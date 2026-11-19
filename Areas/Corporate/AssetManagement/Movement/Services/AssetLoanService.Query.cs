using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed partial class AssetLoanService
{
    public const string ActionApprove = "approve";
    public const string ActionReject = "reject";
    public const string ActionPickup = "pickup";
    public const string ActionReturn = "return";
    public const string ActionConfirmReturn = "confirmReturn";
    public const string ActionExtend = "extend";
    public const string ActionDecideExtension = "decideExtension";
    public const string ActionCancel = "cancel";
    public const string ActionDocument = "document";

    private static readonly string[] AwaitingActions = [ActionApprove, ActionPickup, ActionConfirmReturn, ActionDecideExtension];

    public static AssetMasterFilterMetadataResponse GetMetadata() => new()
    {
        DefaultFilter = new AssetMasterDefaultFilterResponse { SortBy = "requestedAt", SortDirection = "desc" },
        SortOptions =
        [
            new() { Value = "requestedAt", Label = "Tanggal Diajukan" },
            new() { Value = "plannedReturnDate", Label = "Rencana Tanggal Kembali" }
        ],
        FlagOptions =
        [
            EnumOptions("status", "Status Pinjaman", Enum.GetValues<LoanStatus>(), AssetMovementLabels.LoanStatus),
            EnumOptions("extensionStatus", "Status Perpanjangan", Enum.GetValues<LoanExtensionStatus>(), AssetMovementLabels.LoanExtensionStatus)
        ],
        QueryParameters =
        [
            new() { Name = "search", Type = "string", Description = "Mencari pada nomor pinjaman, nomor inventaris, dan nama aset" },
            new() { Name = "status", Type = "integer[]", Description = "Lihat flagOptions status; boleh diulang, misalnya status=3&status=4", Example = "3" },
            new() { Name = "ownerOrganizationUnitId", Type = "guid", Description = "Unit pemilik aset" },
            new() { Name = "borrowerOrganizationUnitId", Type = "guid", Description = "Unit peminjam" },
            new() { Name = "isOverdue", Type = "boolean", Description = "true = hanya pinjaman Dipinjam/Menunggu Konfirmasi Kembali yang lewat rencana tanggal kembali (WIB)", Example = "true" },
            new() { Name = "awaitingMe", Type = "boolean", Description = "true = hanya pinjaman yang menunggu tindakan pengguna ini", Example = "true" },
            new() { Name = "from", Type = "date", Description = "Tanggal diajukan paling awal (WIB), inklusif", Example = "2026-11-19" },
            new() { Name = "to", Type = "date", Description = "Tanggal diajukan paling akhir (WIB), inklusif", Example = "2026-11-19" },
            new() { Name = "sortBy", Type = "string", Description = "requestedAt atau plannedReturnDate", Example = "requestedAt" },
            new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
            new() { Name = "pageNumber", Type = "integer", Description = "Halaman, mulai 1", Example = "1" },
            new() { Name = "pageSize", Type = "integer", Description = $"Jumlah baris per halaman, maksimal {MaxPageSize}", Example = "25" }
        ]
    };

    public async Task<AssetLoanSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var today = AssetRegisterSettingReader.Today();
        var query = Scoped(actor);
        var counts = await query
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        var overdue = await Overdue(query, today).CountAsync(cancellationToken);
        var awaiting = await AwaitingMe(query, actor).CountAsync(cancellationToken);

        int CountOf(LoanStatus status) => counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
        return new AssetLoanSummaryResponse
        {
            Total = counts.Sum(x => x.Count),
            Submitted = CountOf(LoanStatus.Diajukan),
            Approved = CountOf(LoanStatus.Disetujui),
            Borrowed = CountOf(LoanStatus.Dipinjam),
            AwaitingReturnConfirmation = CountOf(LoanStatus.MenungguKonfirmasiKembali),
            Returned = CountOf(LoanStatus.Dikembalikan),
            Rejected = CountOf(LoanStatus.Ditolak),
            Cancelled = CountOf(LoanStatus.Dibatalkan),
            Overdue = overdue,
            AwaitingMe = awaiting
        };
    }

    public async Task<PagedResult<AssetLoanResponse>> GetLoansAsync(AssetLoanPagedQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = await RequireActorAsync(cancellationToken);
        var today = AssetRegisterSettingReader.Today();
        var query = Scoped(actor);

        var statuses = (request.Status ?? []).Distinct().ToList();
        if (statuses.Count > 0)
        {
            query = query.Where(x => statuses.Contains(x.Status));
        }

        if (request.OwnerOrganizationUnitId is { } ownerId && ownerId != Guid.Empty)
        {
            query = query.Where(x => x.OwnerOrganizationUnitId == ownerId);
        }

        if (request.BorrowerOrganizationUnitId is { } borrowerId && borrowerId != Guid.Empty)
        {
            query = query.Where(x => x.BorrowerOrganizationUnitId == borrowerId);
        }

        if (request.IsOverdue is { } isOverdue)
        {
            query = isOverdue
                ? Overdue(query, today)
                : query.Where(x => !((x.Status == LoanStatus.Dipinjam || x.Status == LoanStatus.MenungguKonfirmasiKembali) && x.PlannedReturnDate < today));
        }

        if (request.AwaitingMe == true)
        {
            query = AwaitingMe(query, actor);
        }

        if (request.From is { } from)
        {
            var fromUtc = AppDateTimeHelper.OperationalDateToUtc(from.ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.RequestedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtcExclusive = AppDateTimeHelper.OperationalDateToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue));
            query = query.Where(x => x.RequestedAt < toUtcExclusive);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                x.LoanNumber.ToUpper().Contains(keyword) ||
                x.Items.Any(i => !i.IsDelete && (
                    (i.Asset!.InventoryNumber != null && i.Asset.InventoryNumber.ToUpper().Contains(keyword)) ||
                    i.Asset.AssetName.ToUpper().Contains(keyword))));
        }

        var ascending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var byReturnDate = string.Equals(request.SortBy, "plannedReturnDate", StringComparison.OrdinalIgnoreCase);
        var ordered = (byReturnDate, ascending) switch
        {
            (true, true) => query.OrderBy(x => x.PlannedReturnDate),
            (true, false) => query.OrderByDescending(x => x.PlannedReturnDate),
            (false, true) => query.OrderBy(x => x.RequestedAt),
            _ => query.OrderByDescending(x => x.RequestedAt)
        };

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await Project(ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
        var loanIds = items.Select(x => x.Id).ToList();
        var openExtensions = await ExtensionsOf(_context.AstLoanExtensions.AsNoTracking()
                .Where(x => loanIds.Contains(x.LoanId) && !x.IsDelete && x.Status == LoanExtensionStatus.Diajukan))
            .ToListAsync(cancellationToken);
        foreach (var loan in items)
        {
            loan.OpenExtension = openExtensions.FirstOrDefault(x => x.LoanId == loan.Id);
        }

        await DescribeAsync(items, actor, today, cancellationToken);

        return new PagedResult<AssetLoanResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetLoanResponse> GetLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await RequireActorAsync(cancellationToken);
        var loan = await Project(_context.AstLoans.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw LoanNotFound();

        if (!CanSee(loan.OwnerOrganizationUnitId, loan.BorrowerOrganizationUnitId, loan.Requested?.UserId ?? Guid.Empty, actor))
        {
            throw AssetManagementException.Forbidden(AssetMovementErrorCodes.OutsideScope, AssetMovementMessages.LoanOutsideScope);
        }

        loan.Items = await _context.AstLoanItems
            .AsNoTracking()
            .Where(x => x.LoanId == id && !x.IsDelete)
            .OrderBy(x => x.Asset!.InventoryNumber)
            .Select(x => new AssetLoanItemResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                InventoryNumber = x.Asset!.InventoryNumber,
                AssetName = x.Asset.AssetName,
                GroupName = x.Asset.AssetGroup != null ? x.Asset.AssetGroup.GroupName : null,
                ConditionAtPickup = x.ConditionAtPickup,
                ReturnCondition = x.ReturnCondition
            })
            .ToListAsync(cancellationToken);
        foreach (var item in loan.Items)
        {
            item.ConditionAtPickupName = AssetRegisterLabels.Condition(item.ConditionAtPickup);
            item.ReturnConditionName = item.ReturnCondition is { } returned ? AssetRegisterLabels.Condition(returned) : null;
        }

        loan.Extensions = await ExtensionsOf(_context.AstLoanExtensions.AsNoTracking()
                .Where(x => x.LoanId == id && !x.IsDelete)
                .OrderByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.CreateDateTime))
            .ToListAsync(cancellationToken);
        loan.OpenExtension = loan.Extensions.FirstOrDefault(x => x.Status == LoanExtensionStatus.Diajukan);

        await DescribeAsync([loan], actor, AssetRegisterSettingReader.Today(), cancellationToken);
        return loan;
    }

    public async Task<PagedResult<AssetLoanableAssetResponse>> GetLoanableAssetsAsync(AssetLoanableAssetQuery request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await RequireActorAsync(cancellationToken);
        var entity = await _entityResolver.ResolveAsync(cancellationToken);

        var query = _context.AstAssets.AsNoTracking().Where(x =>
            !x.IsDelete &&
            x.LegalEntityId == entity.LegalEntityId &&
            x.RecordStatus == AssetRecordStatus.Terdaftar &&
            x.PlacementState == AssetPlacementState.Ditempatkan &&
            x.CurrentOrganizationUnitId != null &&
            x.ActiveProcessKind == AssetActiveProcessKind.Tidak &&
            x.Condition != AssetCondition.Hilang &&
            x.Condition != AssetCondition.RusakBerat);

        if (request.OwnerOrganizationUnitId is { } ownerId && ownerId != Guid.Empty)
        {
            query = query.Where(x => x.CurrentOrganizationUnitId == ownerId);
        }

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToUpper();
        if (keyword != null)
        {
            query = query.Where(x =>
                (x.InventoryNumber != null && x.InventoryNumber.ToUpper().Contains(keyword)) ||
                x.AssetName.ToUpper().Contains(keyword));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, MaxPageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.InventoryNumber)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetLoanableAssetResponse
            {
                AssetId = x.Id,
                InventoryNumber = x.InventoryNumber,
                AssetName = x.AssetName,
                GroupName = x.AssetGroup != null ? x.AssetGroup.GroupName : null,
                Condition = x.Condition,
                OrganizationUnitId = x.CurrentOrganizationUnitId!.Value,
                OrganizationUnitName = x.CurrentOrganizationUnit != null ? x.CurrentOrganizationUnit.UnitName : null,
                LocationName = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null
            })
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.ConditionName = AssetRegisterLabels.Condition(item.Condition);
        }

        return new PagedResult<AssetLoanableAssetResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<AssetLabelDocument> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await GetLoanAsync(id, cancellationToken);
        if (loan.Status is not (LoanStatus.Dipinjam or LoanStatus.MenungguKonfirmasiKembali or LoanStatus.Dikembalikan))
        {
            throw StatusConflict(loan.StatusName);
        }

        var entity = await _entityResolver.ResolveAsync(cancellationToken);
        var content = _documentService.RenderLoanReport(loan, entity.LegalEntityName, entity.HospitalSiteName);
        return new AssetLabelDocument(content, $"berita-acara-{loan.LoanNumber}.pdf", loan.ItemCount);
    }

    private IQueryable<AstLoan> Scoped(AssetActor actor)
    {
        var query = _context.AstLoans.AsNoTracking().Where(x => !x.IsDelete);
        if (actor.CanReadAllUnits)
        {
            return query;
        }

        var unitIds = actor.UnitIds.Concat(actor.HeadOfUnitIds).Distinct().ToList();
        var userId = actor.UserId;
        return query.Where(x =>
            unitIds.Contains(x.OwnerOrganizationUnitId) ||
            unitIds.Contains(x.BorrowerOrganizationUnitId) ||
            x.RequestedBy == userId);
    }

    private static IQueryable<AstLoan> Overdue(IQueryable<AstLoan> query, DateOnly today) =>
        query.Where(x => (x.Status == LoanStatus.Dipinjam || x.Status == LoanStatus.MenungguKonfirmasiKembali) && x.PlannedReturnDate < today);

    private static IQueryable<AstLoan> AwaitingMe(IQueryable<AstLoan> query, AssetActor actor)
    {
        var headUnitIds = actor.HeadOfUnitIds.ToList();
        var userId = actor.UserId;
        return query.Where(x =>
            (x.Status == LoanStatus.Diajukan && headUnitIds.Contains(x.OwnerOrganizationUnitId) && x.RequestedBy != userId) ||
            (x.Status == LoanStatus.Disetujui && headUnitIds.Contains(x.BorrowerOrganizationUnitId)) ||
            (x.Status == LoanStatus.MenungguKonfirmasiKembali && headUnitIds.Contains(x.OwnerOrganizationUnitId)) ||
            ((x.Status == LoanStatus.Dipinjam || x.Status == LoanStatus.MenungguKonfirmasiKembali) &&
             headUnitIds.Contains(x.OwnerOrganizationUnitId) &&
             x.Extensions.Any(e => !e.IsDelete && e.Status == LoanExtensionStatus.Diajukan && e.RequestedBy != userId)));
    }

    private static bool CanSee(Guid ownerUnitId, Guid borrowerUnitId, Guid requestedBy, AssetActor actor) =>
        actor.CanReadAllUnits ||
        actor.UnitIds.Contains(ownerUnitId) ||
        actor.UnitIds.Contains(borrowerUnitId) ||
        actor.HeadOfUnitIds.Contains(ownerUnitId) ||
        actor.HeadOfUnitIds.Contains(borrowerUnitId) ||
        requestedBy == actor.UserId;

    private static IQueryable<AssetLoanResponse> Project(IQueryable<AstLoan> query) =>
        query.Select(x => new AssetLoanResponse
        {
            Id = x.Id,
            LoanNumber = x.LoanNumber,
            OwnerOrganizationUnitId = x.OwnerOrganizationUnitId,
            OwnerOrganizationUnitName = x.OwnerOrganizationUnit != null ? x.OwnerOrganizationUnit.UnitName : null,
            BorrowerOrganizationUnitId = x.BorrowerOrganizationUnitId,
            BorrowerOrganizationUnitName = x.BorrowerOrganizationUnit != null ? x.BorrowerOrganizationUnit.UnitName : null,
            Purpose = x.Purpose,
            StartDate = x.StartDate,
            PlannedReturnDate = x.PlannedReturnDate,
            OriginalPlannedReturnDate = x.OriginalPlannedReturnDate,
            Status = x.Status,
            Requested = new AssetMovementActorResponse { UserId = x.RequestedBy, At = x.RequestedAt },
            Approved = x.ApprovedBy != null ? new AssetMovementActorResponse { UserId = x.ApprovedBy.Value, At = x.ApprovedAt!.Value } : null,
            IsApprovalByRequester = x.IsApprovalByRequester,
            PickedUp = x.PickedUpBy != null ? new AssetMovementActorResponse { UserId = x.PickedUpBy.Value, At = x.PickedUpAt!.Value } : null,
            Returned = x.ReturnedBy != null ? new AssetMovementActorResponse { UserId = x.ReturnedBy.Value, At = x.ReturnedAt!.Value } : null,
            ReturnConfirmed = x.ReturnConfirmedBy != null ? new AssetMovementActorResponse { UserId = x.ReturnConfirmedBy.Value, At = x.ReturnConfirmedAt!.Value } : null,
            Rejected = x.RejectedBy != null ? new AssetMovementActorResponse { UserId = x.RejectedBy.Value, At = x.RejectedAt!.Value } : null,
            Cancelled = x.CancelledBy != null ? new AssetMovementActorResponse { UserId = x.CancelledBy.Value, At = x.CancelledAt!.Value } : null,
            DecisionReason = x.DecisionReason,
            ItemCount = x.Items.Count(i => !i.IsDelete),
            RowVersion = x.RowVersion
        });

    private static IQueryable<AssetLoanExtensionResponse> ExtensionsOf(IQueryable<AstLoanExtension> query) =>
        query.Select(x => new AssetLoanExtensionResponse
        {
            Id = x.Id,
            LoanId = x.LoanId,
            PreviousReturnDate = x.PreviousReturnDate,
            RequestedReturnDate = x.RequestedReturnDate,
            Reason = x.Reason,
            Status = x.Status,
            Requested = x.RequestedBy != null ? new AssetMovementActorResponse { UserId = x.RequestedBy.Value, At = x.RequestedAt!.Value } : null,
            Decided = x.DecidedBy != null ? new AssetMovementActorResponse { UserId = x.DecidedBy.Value, At = x.DecidedAt!.Value } : null,
            DecisionReason = x.DecisionReason
        });

    private async Task DescribeAsync(IReadOnlyCollection<AssetLoanResponse> loans, AssetActor actor, DateOnly today, CancellationToken cancellationToken)
    {
        var extensions = loans
            .SelectMany(x => x.Extensions)
            .Concat(loans.Select(x => x.OpenExtension).OfType<AssetLoanExtensionResponse>())
            .Distinct()
            .ToList();
        var steps = loans
            .SelectMany(x => new[] { x.Requested, x.Approved, x.PickedUp, x.Returned, x.ReturnConfirmed, x.Rejected, x.Cancelled })
            .Concat(extensions.SelectMany(x => new[] { x.Requested, x.Decided }))
            .OfType<AssetMovementActorResponse>()
            .ToList();
        var userIds = steps.Select(x => x.UserId).Where(x => x != Guid.Empty).Distinct().ToList();
        var profiles = new Dictionary<Guid, (string DisplayName, string? PositionName)>();
        if (userIds.Count > 0)
        {
            var rows = await _context.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, PositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                profiles[row.Id] = (row.DisplayName, row.PositionName);
            }
        }

        foreach (var step in steps)
        {
            if (profiles.TryGetValue(step.UserId, out var profile))
            {
                step.Name = profile.DisplayName;
                step.PositionName = profile.PositionName;
            }
        }

        foreach (var extension in extensions)
        {
            extension.StatusName = AssetMovementLabels.LoanExtensionStatus(extension.Status);
        }

        foreach (var loan in loans)
        {
            loan.StatusName = AssetMovementLabels.LoanStatus(loan.Status);
            loan.IsOverdue = (loan.Status is LoanStatus.Dipinjam or LoanStatus.MenungguKonfirmasiKembali) && loan.PlannedReturnDate < today;
            loan.OverdueDays = loan.IsOverdue ? today.DayNumber - loan.PlannedReturnDate.DayNumber : 0;

            var requestedBy = loan.Requested?.UserId ?? Guid.Empty;
            var isOwnerHead = actor.IsHeadOfUnit(loan.OwnerOrganizationUnitId);
            var isBorrowerHead = actor.IsHeadOfUnit(loan.BorrowerOrganizationUnitId);
            var open = loan.OpenExtension;
            var actions = new List<string>();

            switch (loan.Status)
            {
                case LoanStatus.Diajukan:
                    loan.AwaitingParty = $"Kepala Unit {loan.OwnerOrganizationUnitName}";
                    if (isOwnerHead && requestedBy != actor.UserId)
                    {
                        actions.Add(ActionApprove);
                        actions.Add(ActionReject);
                    }

                    break;
                case LoanStatus.Disetujui:
                    loan.AwaitingParty = $"Kepala Unit {loan.BorrowerOrganizationUnitName}";
                    if (isBorrowerHead)
                    {
                        actions.Add(ActionPickup);
                    }

                    break;
                case LoanStatus.Dipinjam:
                    loan.AwaitingParty = open != null
                        ? $"Kepala Unit {loan.OwnerOrganizationUnitName}"
                        : $"Kepala Unit {loan.BorrowerOrganizationUnitName}";
                    if (isBorrowerHead)
                    {
                        actions.Add(ActionReturn);
                        if (open == null)
                        {
                            actions.Add(ActionExtend);
                        }
                    }

                    break;
                case LoanStatus.MenungguKonfirmasiKembali:
                    loan.AwaitingParty = $"Kepala Unit {loan.OwnerOrganizationUnitName}";
                    if (isOwnerHead)
                    {
                        actions.Add(ActionConfirmReturn);
                    }

                    break;
            }

            if (open != null && isOwnerHead && open.Requested?.UserId != actor.UserId &&
                (loan.Status is LoanStatus.Dipinjam or LoanStatus.MenungguKonfirmasiKembali))
            {
                actions.Add(ActionDecideExtension);
            }

            if ((loan.Status is LoanStatus.Diajukan or LoanStatus.Disetujui) && CanCancel(requestedBy, loan.OwnerOrganizationUnitId, actor))
            {
                actions.Add(ActionCancel);
            }

            if (loan.Status is LoanStatus.Dipinjam or LoanStatus.MenungguKonfirmasiKembali or LoanStatus.Dikembalikan)
            {
                actions.Add(ActionDocument);
            }

            loan.IsAwaitingMe = actions.Any(AwaitingActions.Contains);
            loan.AvailableActions = actions;
        }
    }

    private static AssetMasterOptionSourceResponse EnumOptions<TEnum>(string name, string label, IEnumerable<TEnum> values, Func<TEnum, string> describe)
        where TEnum : struct, Enum => new()
    {
        Name = name,
        Label = label,
        Options = values
            .Select(x => new AssetMasterSortOptionResponse { Value = Convert.ToInt32(x).ToString(), Label = describe(x) })
            .ToList()
    };
}
