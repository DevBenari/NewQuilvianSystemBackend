using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;

/// <summary>
/// Permintaan pengadaan (PR) — draf, daftar, ringkasan, detail, riwayat (BE-PBJ-006;
/// <c>state-transition-matrix.md</c> bagian 1, <c>validation-matrix.md</c> bagian 1).
/// </summary>
/// <remarks>
/// <para><b>Cakupan unit (PBJ-DEC-020/036).</b> Setiap query disaring
/// <see cref="ProcurementDataScopeService"/> dengan resource <c>ProcurementRequest</c>. PR di luar
/// cakupan dijawab 404, bukan 403, agar keberadaannya tidak bocor. Contoh: pengguna IGD membuka
/// tautan PR Radiologi → "Pengajuan tidak ditemukan atau bukan milik unit Anda."</para>
/// <para><b>Membuat PR</b> hanya untuk unit penempatan pengusul sendiri (PBJ-VAL-010), walaupun
/// pengguna boleh melihat semua unit.</para>
/// <para><b>Mengubah draf</b> hanya oleh pembuatnya, saat <c>DRAFT</c>/<c>RETURNED</c>, dengan
/// <c>ExpectedRowVersion</c> yang cocok. Baris lama ditandai terhapus (tidak dihapus fisik) dan
/// baris baru ditulis, sehingga riwayat isi tetap dapat ditelusuri.</para>
/// <para>Aksi ajukan, batal, dan tarik dikerjakan BE-PBJ-007; verifikasi BE-PBJ-008.</para>
/// </remarks>
public sealed class ProcurementPurchaseRequestService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Request";
    private const string NotFoundMessage = "Pengajuan tidak ditemukan atau bukan milik unit Anda.";

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        [ProcurementStatuses.PurchaseRequest.Draft] = "Draf",
        [ProcurementStatuses.PurchaseRequest.Submitted] = "Menunggu Verifikasi",
        [ProcurementStatuses.PurchaseRequest.Returned] = "Dikembalikan",
        [ProcurementStatuses.PurchaseRequest.PendingApproval] = "Menunggu Persetujuan",
        [ProcurementStatuses.PurchaseRequest.Approved] = "Disetujui",
        [ProcurementStatuses.PurchaseRequest.FulfilledFromStock] = "Dipenuhi dari Stok",
        [ProcurementStatuses.PurchaseRequest.Rejected] = "Ditolak",
        [ProcurementStatuses.PurchaseRequest.Cancelled] = "Dibatalkan",
        [ProcurementStatuses.PurchaseRequest.ConvertedToPackage] = "Masuk Paket"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDataScopeService _scopeService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly LoggerService _loggerService;

    public ProcurementPurchaseRequestService(
        ApplicationDbContext dbContext,
        ProcurementDataScopeService scopeService,
        ProcurementDocumentHistoryWriter historyWriter,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _scopeService = scopeService;
        _historyWriter = historyWriter;
        _loggerService = loggerService;
    }

    public static bool IsEditableStatus(string status) =>
        status == ProcurementStatuses.PurchaseRequest.Draft ||
        status == ProcurementStatuses.PurchaseRequest.Returned;

    // ------------------------------------------------------------------ baca

    public PurchaseRequestFilterMetadataResponse GetFilterMetadata() => new()
    {
        DefaultFilter = new PurchaseRequestQuery(),
        StatusOptions = StatusLabels.Select(x => new PurchaseRequestOptionValue { Value = x.Key, Label = x.Value }).ToList(),
        UrgencyOptions =
        [
            new() { Value = ProcurementUrgencyLevels.Normal, Label = "Biasa" },
            new() { Value = ProcurementUrgencyLevels.Urgent, Label = "Mendesak" }
        ],
        SortOptions =
        [
            new() { Value = "createdAt", Label = "Tanggal dibuat" },
            new() { Value = "requestNumber", Label = "Nomor PR" },
            new() { Value = "neededByDate", Label = "Tanggal dibutuhkan" },
            new() { Value = "purchaseTotal", Label = "Nilai beli" }
        ]
    };

    public async Task<PurchaseRequestSummaryResponse> GetSummaryAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var byStatus = await query
            .GroupBy(x => x.Status)
            .Select(g => new PurchaseRequestStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new PurchaseRequestSummaryResponse
        {
            Total = byStatus.Sum(x => x.Count),
            Urgent = await query.CountAsync(x => x.UrgencyLevel == ProcurementUrgencyLevels.Urgent, cancellationToken),
            OutOfPlan = await query.CountAsync(x => x.IsOutOfPlan, cancellationToken),
            OverBudget = await query.CountAsync(x => x.IsOverBudget, cancellationToken),
            ByStatus = byStatus.OrderBy(x => x.Status).ToList()
        };
    }

    public async Task<PagedResult<PurchaseRequestListItem>> GetPagedAsync(
        ClaimsPrincipal user, PurchaseRequestQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            throw new ProcurementRequestValidationException("PBJ-VAL-007", "Tanggal akhir tidak boleh sebelum tanggal awal.");

        var query = await ScopedAsync(user, cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status.Trim());
        if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        if (request.FiscalYear.HasValue) query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        if (!string.IsNullOrWhiteSpace(request.UrgencyLevel)) query = query.Where(x => x.UrgencyLevel == request.UrgencyLevel.Trim());
        if (request.IsOutOfPlan.HasValue) query = query.Where(x => x.IsOutOfPlan == request.IsOutOfPlan.Value);
        if (request.IsOverBudget.HasValue) query = query.Where(x => x.IsOverBudget == request.IsOverBudget.Value);
        if (request.MineOnly == true)
        {
            var userId = ProcurementDataScopeService.GetUserId(user);
            query = query.Where(x => x.RequesterUserId == userId);
        }
        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreateDateTime >= start);
        }
        if (request.EndDate.HasValue)
        {
            // Tanggal akhir inklusif: sampai sebelum awal hari berikutnya.
            var endExclusive = request.EndDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreateDateTime < endExclusive);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.RequestNumber != null && x.RequestNumber.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "requestnumber" => descending ? query.OrderByDescending(x => x.RequestNumber) : query.OrderBy(x => x.RequestNumber),
            "neededbydate" => descending ? query.OrderByDescending(x => x.NeededByDate) : query.OrderBy(x => x.NeededByDate),
            "purchasetotal" => descending ? query.OrderByDescending(x => x.PurchaseTotal) : query.OrderBy(x => x.PurchaseTotal),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new PurchaseRequestListItem
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department!.DepartmentName,
                RequesterUserId = x.RequesterUserId,
                RequesterName = _dbContext.Users.Where(u => u.Id == x.RequesterUserId).Select(u => u.DisplayName).FirstOrDefault(),
                FiscalYear = x.FiscalYear,
                NeededByDate = x.NeededByDate,
                UrgencyLevel = x.UrgencyLevel,
                Status = x.Status,
                IsOutOfPlan = x.IsOutOfPlan,
                IsOverBudget = x.IsOverBudget,
                RequestedTotal = x.RequestedTotal,
                PurchaseTotal = x.PurchaseTotal,
                LineCount = x.Lines.Count(l => !l.IsDelete),
                SubmittedAt = x.SubmittedAt,
                CreateDateTime = x.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PurchaseRequestListItem>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<PurchaseRequestResponse> GetByIdAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var response = await Project(query.Where(x => x.Id == id)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        response.Lines = await _dbContext.PrcPurchaseRequestLines.AsNoTracking()
            .Where(x => x.PurchaseRequestId == id && !x.IsDelete)
            .OrderBy(x => x.LineNo)
            .Select(x => new PurchaseRequestLineResponse
            {
                Id = x.Id,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,
                ProcurementTypeCode = x.Item.ProcurementType!.TypeCode,
                ItemCategoryId = x.Item.ItemCategoryId,
                ItemCategoryName = x.Item.ItemCategory!.CategoryName,
                MeasurementName = x.Item.Measurement!.MeasurementName,
                Specification = x.Specification,
                RequestedQuantity = x.RequestedQuantity,
                EstimatedUnitPrice = x.EstimatedUnitPrice,
                RequestedAmount = Math.Round(x.RequestedQuantity * x.EstimatedUnitPrice, 2),
                NeedPlanLineKey = x.NeedPlanLineKey,
                InPlanQuantity = x.InPlanQuantity,
                OutOfPlanQuantity = x.OutOfPlanQuantity,
                OutOfPlanReason = x.OutOfPlanReason,
                StockAvailableQuantity = x.StockAvailableQuantity,
                StockFulfilledQuantity = x.StockFulfilledQuantity,
                StockSourceNote = x.StockSourceNote,
                StockCheckedAt = x.StockCheckedAt,
                PurchaseQuantity = x.PurchaseQuantity,
                PurchaseAmount = x.PurchaseAmount
            })
            .ToListAsync(cancellationToken);

        response.CanEdit = IsEditableStatus(response.Status) &&
                           response.RequesterUserId == ProcurementDataScopeService.GetUserId(user);
        return response;
    }

    public async Task<List<DocumentHistoryResponse>> GetStatusHistoryAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.PurchaseRequest, id, cancellationToken);
    }

    /// <summary>
    /// Hak atas PR untuk lampiran: tidak terlihat → None; pembuat saat DRAFT/RETURNED → Edit;
    /// selain itu → Read.
    /// </summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        var query = await ScopedAsync(user, cancellationToken);
        var row = await query.Where(x => x.Id == id)
            .Select(x => new { x.Status, x.RequesterUserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null) return ProcurementDocumentAccessLevel.None;
        return IsEditableStatus(row.Status) && row.RequesterUserId == ProcurementDataScopeService.GetUserId(user)
            ? ProcurementDocumentAccessLevel.Edit
            : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    public async Task<PurchaseRequestResponse> CreateAsync(ClaimsPrincipal user, CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        await RequirePlacementAsync(actorUserId, request.DepartmentId, cancellationToken);
        var header = await ValidateHeaderAsync(request, cancellationToken);
        var lines = await ValidateLinesAsync(request.Lines, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new PrcPurchaseRequest
        {
            DepartmentId = request.DepartmentId,
            RequesterUserId = actorUserId,
            Status = ProcurementStatuses.PurchaseRequest.Draft,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        ApplyHeader(entity, request, header);
        AddLines(entity, lines, actorUserId, now);

        _dbContext.PrcPurchaseRequests.Add(entity);
        _historyWriter.Record(
            ProcurementDocumentTypes.PurchaseRequest, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, entity.Status, null, new { entity.RequestedTotal, LineCount = lines.Count },
            actorUserId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("PurchaseRequest.Create", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> UpdateAsync(ClaimsPrincipal user, Guid id, UpdatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = RequireUser(user);
        var query = await ScopedAsync(user, cancellationToken);
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);

        var entity = await _dbContext.PrcPurchaseRequests
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstAsync(x => x.Id == id, cancellationToken);

        if (!IsEditableStatus(entity.Status))
            throw new ProcurementRequestConflictException("PBJ-VAL-093", "Status pengajuan tidak memungkinkan tindakan ini.");
        if (entity.RequesterUserId != actorUserId)
            throw new ProcurementRequestForbiddenException("Hanya pembuat pengajuan yang dapat mengubahnya.");
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementRequestConflictException("PBJ-VAL-090", "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.");
        if (request.DepartmentId != entity.DepartmentId)
            await RequirePlacementAsync(actorUserId, request.DepartmentId, cancellationToken);

        var header = await ValidateHeaderAsync(request, cancellationToken);
        var lines = await ValidateLinesAsync(request.Lines, cancellationToken);

        var now = DateTime.UtcNow;

        // Dua tahap dalam satu transaksi: baris lama ditandai terhapus dan versi kepala diganti lebih
        // dulu, baru baris pengganti ditulis. Urutan ini menjaga indeks unik (PR, nomor baris) yang
        // hanya berlaku untuk baris hidup, dan RowVersion pada tahap pertama menolak simpanan basi.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var old in entity.Lines.ToList())
            {
                old.IsDelete = true;
                old.DeleteDateTime = now;
                old.DeleteBy = actorUserId;
            }

            entity.DepartmentId = request.DepartmentId;
            ApplyHeader(entity, request, header);
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            AddLines(entity, lines, actorUserId, now);
            _historyWriter.Record(
                ProcurementDocumentTypes.PurchaseRequest, entity.Id, entity.RequestNumber, ProcurementDocumentHistoryActions.Update,
                entity.Status, entity.Status, null, new { entity.RequestedTotal, LineCount = lines.Count },
                actorUserId, DateTimeOffset.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementRequestConflictException("PBJ-VAL-090", "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.");
        }

        await AuditAsync("PurchaseRequest.Update", entity, actorUserId);
        return await GetByIdAsync(user, entity.Id, cancellationToken);
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<IQueryable<PrcPurchaseRequest>> ScopedAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var scope = await _scopeService.ResolveAsync(user, ProcurementPermissions.PurchaseRequestResource, cancellationToken);
        var query = _dbContext.PrcPurchaseRequests.AsNoTracking().Where(x => !x.IsDelete);
        if (scope.IsAllUnits) return query;
        var departments = scope.DepartmentIds.ToList();
        return query.Where(x => departments.Contains(x.DepartmentId));
    }

    private static Guid RequireUser(ClaimsPrincipal user)
    {
        var userId = ProcurementDataScopeService.GetUserId(user);
        if (userId == Guid.Empty)
            throw new ProcurementRequestForbiddenException("Identitas pengguna tidak dikenali.");
        return userId;
    }

    private async Task RequirePlacementAsync(Guid userId, Guid departmentId, CancellationToken cancellationToken)
    {
        var placements = await _scopeService.GetPlacementDepartmentIdsAsync(userId, cancellationToken);
        if (!placements.Contains(departmentId))
        {
            var name = await _dbContext.MstDepartments.AsNoTracking()
                .Where(x => x.Id == departmentId).Select(x => x.DepartmentName).FirstOrDefaultAsync(cancellationToken);
            throw new ProcurementRequestValidationException(
                "PBJ-VAL-010", $"Anda tidak terdaftar di unit {name ?? "yang dipilih"}.");
        }
    }

    private sealed record HeaderValues(string? UrgencyReason, string Justification);

    private async Task<HeaderValues> ValidateHeaderAsync(CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        if (!ProcurementUrgencyLevels.IsKnown(request.UrgencyLevel))
            throw new ProcurementRequestValidationException("PBJ-VAL-008", "Tingkat urgensi tidak dikenal.");
        if (request.FiscalYear is < 2000 or > 2100)
            throw new ProcurementRequestValidationException("PBJ-VAL-007", "Tahun anggaran tidak valid.");
        if (!await _dbContext.MstDepartments.AnyAsync(x => x.Id == request.DepartmentId && !x.IsDelete, cancellationToken))
            throw new ProcurementRequestValidationException("PBJ-VAL-010", "Unit tidak ditemukan.");
        if (!await _dbContext.MstProcurementFundSources.AnyAsync(x => x.Id == request.FundSourceId && !x.IsDelete, cancellationToken))
            throw new ProcurementRequestValidationException("PBJ-VAL-011", "Sumber dana tidak ditemukan.");

        // Alasan dan justifikasi boleh kosong selama draf; diwajibkan saat diajukan (PBJ-VAL-006/008, BE-PBJ-007).
        var urgencyReason = Trim(request.UrgencyReason, 1000, "Alasan mendesak");
        var justification = Trim(request.Justification, 2000, "Alasan kebutuhan") ?? string.Empty;
        return new HeaderValues(urgencyReason, justification);
    }

    private sealed record LineValues(
        Guid ItemId, string? Specification, decimal Quantity, decimal UnitPrice, Guid? NeedPlanLineKey, string? OutOfPlanReason);

    private async Task<List<LineValues>> ValidateLinesAsync(List<PurchaseRequestLineRequest>? lines, CancellationToken cancellationToken)
    {
        lines ??= new List<PurchaseRequestLineRequest>();
        var itemIds = lines.Select(x => x.ItemId).Distinct().ToList();
        var items = await _dbContext.MstProcurementItems.AsNoTracking()
            .Where(x => itemIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.ItemCode, x.IsActive, x.Measurement!.MeasurementName, x.Measurement.IsDecimalAllowed })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var result = new List<LineValues>();
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var no = index + 1;
            if (!items.TryGetValue(line.ItemId, out var item) || !item.IsActive)
                throw new ProcurementRequestValidationException(
                    "PBJ-VAL-005",
                    $"Barang pada baris {no} tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya.");
            if (line.RequestedQuantity <= 0)
                throw new ProcurementRequestValidationException("PBJ-VAL-002", $"Jumlah pada baris {no} harus lebih dari 0.");
            if (!item.IsDecimalAllowed && line.RequestedQuantity != decimal.Truncate(line.RequestedQuantity))
                throw new ProcurementRequestValidationException("PBJ-VAL-003", $"Satuan {item.MeasurementName} tidak boleh pecahan.");
            if (line.EstimatedUnitPrice < 0)
                throw new ProcurementRequestValidationException("PBJ-VAL-004", "Perkiraan harga tidak boleh negatif.");

            result.Add(new LineValues(
                line.ItemId,
                Trim(line.Specification, 1000, $"Spesifikasi baris {no}"),
                line.RequestedQuantity,
                line.EstimatedUnitPrice,
                line.NeedPlanLineKey,
                Trim(line.OutOfPlanReason, 1000, $"Alasan di luar rencana baris {no}")));
        }

        return result;
    }

    private static void ApplyHeader(PrcPurchaseRequest entity, CreatePurchaseRequestRequest request, HeaderValues header)
    {
        entity.FiscalYear = request.FiscalYear;
        entity.FundSourceId = request.FundSourceId;
        entity.NeededByDate = request.NeededByDate;
        entity.UrgencyLevel = request.UrgencyLevel;
        entity.UrgencyReason = request.UrgencyLevel == ProcurementUrgencyLevels.Urgent ? header.UrgencyReason : null;
        entity.UrgencyDeadline = request.UrgencyLevel == ProcurementUrgencyLevels.Urgent ? request.UrgencyDeadline : null;
        entity.Justification = header.Justification;
    }

    private void AddLines(PrcPurchaseRequest entity, List<LineValues> lines, Guid actorUserId, DateTime now)
    {
        decimal total = 0;
        var lineNo = 0;
        foreach (var line in lines)
        {
            lineNo++;
            var amount = Math.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
            total += amount;
            var row = new PrcPurchaseRequestLine
            {
                PurchaseRequestId = entity.Id,
                LineNo = lineNo,
                ItemId = line.ItemId,
                Specification = line.Specification,
                RequestedQuantity = line.Quantity,
                EstimatedUnitPrice = line.UnitPrice,
                NeedPlanLineKey = line.NeedPlanLineKey,
                OutOfPlanReason = line.OutOfPlanReason,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.PrcPurchaseRequestLines.Add(row);
        }

        entity.RequestedTotal = total;
    }

    private static string? Trim(string? value, int maxLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementRequestValidationException("PBJ-VAL-006", $"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private IQueryable<PurchaseRequestResponse> Project(IQueryable<PrcPurchaseRequest> query) =>
        query.Select(x => new PurchaseRequestResponse
        {
            Id = x.Id,
            RequestNumber = x.RequestNumber,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department!.DepartmentName,
            RequesterUserId = x.RequesterUserId,
            RequesterName = _dbContext.Users.Where(u => u.Id == x.RequesterUserId).Select(u => u.DisplayName).FirstOrDefault(),
            FiscalYear = x.FiscalYear,
            FundSourceId = x.FundSourceId,
            FundSourceName = x.FundSource!.FundSourceName,
            NeededByDate = x.NeededByDate,
            UrgencyLevel = x.UrgencyLevel,
            UrgencyReason = x.UrgencyReason,
            UrgencyDeadline = x.UrgencyDeadline,
            Justification = x.Justification,
            Status = x.Status,
            IsOutOfPlan = x.IsOutOfPlan,
            RequestedTotal = x.RequestedTotal,
            PurchaseTotal = x.PurchaseTotal,
            IsOverBudget = x.IsOverBudget,
            OverBudgetAmount = x.OverBudgetAmount,
            SubmissionCount = x.SubmissionCount,
            SubmittedAt = x.SubmittedAt,
            VerifiedAt = x.VerifiedAt,
            ApprovedAt = x.ApprovedAt,
            WorkflowInstanceId = x.WorkflowInstanceId,
            CancelReason = x.CancelReason,
            RowVersion = x.RowVersion,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        });

    private Task AuditAsync(string action, PrcPurchaseRequest entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan permintaan pengadaan.", new
        {
            EntityId = entity.Id,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>
/// Penyedia akses lampiran untuk PR (BE-PBJ-006; <see cref="IProcurementDocumentAccessProvider"/>).
/// </summary>
public sealed class PurchaseRequestAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementPurchaseRequestService _service;

    public PurchaseRequestAttachmentAccessProvider(ProcurementPurchaseRequestService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.PurchaseRequest;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}
