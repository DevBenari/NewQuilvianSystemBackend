using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;

/// <summary>
/// BE-FIN-112 (Revisi 21, EPIC FIN-04, FIN-DEC-169, FIN-DEC-170, FIN-DEC-193, FIN-DEC-194).
/// Layanan pengelolaan Exit Clearance HR dengan penegakan gerbang bebas tanggungan piutang Finance:
/// 1. Status bebas tanggungan dihitung authoritative dari Finance (FinanceReceivableClearanceService), bukan diisi tangan (AC 3, FIN-DEC-193).
/// 2. Pegawai yang masih memiliki piutang (misal Rp 1.500.000) terkunci pada status "Belum bebas tanggungan" dan penutupan clearance DITOLAK (AC 1).
/// 3. Setelah piutang lunas (saldo Rp 0), pemeriksaan ulang membuka kunci dan exit clearance dapat disahkan (AC 2).
/// </summary>
public sealed class ExitClearanceService
{
    private const string LogCategory = "Corporate.HumanResource.LifecycleManagement.ExitClearance";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivableClearanceService _financeClearanceService;

    public ExitClearanceService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivableClearanceService financeClearanceService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _financeClearanceService = financeClearanceService;
    }

    public async Task<ExitClearanceFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        return new ExitClearanceFilterMetadataResponse();
    }

    public async Task<ExitClearanceSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var all = await _dbContext.TrxExitClearances.AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => new { x.ClearanceStatus, x.IsFinanceCleared })
            .ToListAsync(cancellationToken);

        return new ExitClearanceSummaryResponse
        {
            TotalData = all.Count,
            Draft = all.Count(x => x.ClearanceStatus == "Draft"),
            InProgress = all.Count(x => x.ClearanceStatus == "InProgress"),
            Completed = all.Count(x => x.ClearanceStatus == "Completed"),
            Cancelled = all.Count(x => x.ClearanceStatus == "Cancelled"),
            PendingFinance = all.Count(x => !x.IsFinanceCleared && x.ClearanceStatus != "Completed" && x.ClearanceStatus != "Cancelled")
        };
    }

    public async Task<PagedResult<ExitClearanceListResponse>> GetPagedAsync(
        ExitClearanceQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _dbContext.TrxExitClearances.AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.WorkforceProfile)
            .Include(x => x.EmployeeSeparation)
            .Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.ClearanceStatus))
        {
            dbQuery = dbQuery.Where(x => x.ClearanceStatus == query.ClearanceStatus);
        }

        if (query.EmployeeId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.EmployeeId == query.EmployeeId.Value);
        }

        if (query.IsFinanceCleared.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsFinanceCleared == query.IsFinanceCleared.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(x =>
                x.ClearanceNumber.ToLower().Contains(search) ||
                (x.Employee != null && (x.Employee.FullName.ToLower().Contains(search) || x.Employee.EmployeeNumber.ToLower().Contains(search))) ||
                (x.EmployeeSeparation != null && x.EmployeeSeparation.SeparationNumber.ToLower().Contains(search)));
        }

        var totalItems = await dbQuery.CountAsync(cancellationToken);

        var descending = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        dbQuery = query.SortBy.Trim().ToLowerInvariant() switch
        {
            "clearancenumber" => descending ? dbQuery.OrderByDescending(x => x.ClearanceNumber) : dbQuery.OrderBy(x => x.ClearanceNumber),
            "clearancestatus" => descending ? dbQuery.OrderByDescending(x => x.ClearanceStatus) : dbQuery.OrderBy(x => x.ClearanceStatus),
            "startdate" => descending ? dbQuery.OrderByDescending(x => x.StartDate) : dbQuery.OrderBy(x => x.StartDate),
            _ => descending ? dbQuery.OrderByDescending(x => x.CreateDateTime) : dbQuery.OrderBy(x => x.CreateDateTime)
        };

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Ambil status finance clearance secara batch untuk item pada halaman ini
        var employeeIds = items
            .Where(x => x.EmployeeId.HasValue)
            .Select(x => x.EmployeeId!.Value)
            .Distinct()
            .ToList();

        var clearanceDict = new Dictionary<Guid, decimal>();
        if (employeeIds.Count > 0)
        {
            var clearanceResults = await _financeClearanceService.GetBatchAsync(employeeIds, cancellationToken);
            foreach (var r in clearanceResults)
            {
                clearanceDict[r.BenefitOwnerId] = r.OutstandingAmount;
            }
        }

        var listResponses = items.Select(x =>
        {
            var outstanding = x.EmployeeId.HasValue && clearanceDict.TryGetValue(x.EmployeeId.Value, out var val) ? val : 0m;
            return new ExitClearanceListResponse
            {
                Id = x.Id,
                ClearanceNumber = x.ClearanceNumber,
                EmployeeSeparationId = x.EmployeeSeparationId,
                SeparationNumber = x.EmployeeSeparation?.SeparationNumber ?? string.Empty,
                WorkforceProfileId = x.WorkforceProfileId,
                EmployeeId = x.EmployeeId,
                EmployeeNumber = x.Employee?.EmployeeNumber ?? string.Empty,
                EmployeeName = x.Employee?.FullName ?? string.Empty,
                DepartmentName = string.Empty,
                PositionName = string.Empty,
                StartDate = x.StartDate,
                DueDate = x.DueDate,
                CompletedAt = x.CompletedAt,
                ClearanceStatus = x.ClearanceStatus,
                ProgressPercentage = x.ProgressPercentage,
                IsDepartmentCleared = x.IsDepartmentCleared,
                IsAssetCleared = x.IsAssetCleared,
                IsFinanceCleared = x.IsFinanceCleared,
                IsPayrollCleared = x.IsPayrollCleared,
                IsItAccessCleared = x.IsItAccessCleared,
                IsHrCleared = x.IsHrCleared,
                OutstandingReceivableAmount = outstanding,
                CanComplete = x.IsFinanceCleared && x.ClearanceStatus != "Completed" && x.ClearanceStatus != "Cancelled",
                CreateDateTime = x.CreateDateTime
            };
        }).ToList();

        return new PagedResult<ExitClearanceListResponse>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalData = totalItems,
            TotalPage = (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = listResponses
        };
    }

    public async Task<ExitClearanceDetailResponse> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .Include(x => x.Employee)
            .Include(x => x.WorkforceProfile)
            .Include(x => x.EmployeeSeparation)
            .Include(x => x.CompletedByUser)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance dengan Id {id} tidak ditemukan.");

        // AC 2 & FIN-DEC-193: Verifikasi live status bebas tanggungan dari Finance
        decimal outstanding = 0m;
        int openCount = 0;
        DateOnly? oldestDue = null;

        if (clearance.EmployeeId.HasValue)
        {
            var financeClearance = await _financeClearanceService.GetAsync(clearance.EmployeeId.Value, cancellationToken);
            outstanding = financeClearance.OutstandingAmount;
            openCount = financeClearance.OpenReceivableCount;
            oldestDue = financeClearance.OldestDueDate;

            if (clearance.IsFinanceCleared != financeClearance.IsCleared)
            {
                clearance.IsFinanceCleared = financeClearance.IsCleared;
                RecalculateProgress(clearance);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return MapToDetailResponse(clearance, outstanding, openCount, oldestDue);
    }

    public async Task<ExitClearanceDetailResponse> GetBySeparationIdAsync(Guid separationId, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .Include(x => x.Employee)
            .Include(x => x.WorkforceProfile)
            .Include(x => x.EmployeeSeparation)
            .Include(x => x.CompletedByUser)
            .FirstOrDefaultAsync(x => x.EmployeeSeparationId == separationId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance untuk Separation Id {separationId} tidak ditemukan.");

        decimal outstanding = 0m;
        int openCount = 0;
        DateOnly? oldestDue = null;

        if (clearance.EmployeeId.HasValue)
        {
            var financeClearance = await _financeClearanceService.GetAsync(clearance.EmployeeId.Value, cancellationToken);
            outstanding = financeClearance.OutstandingAmount;
            openCount = financeClearance.OpenReceivableCount;
            oldestDue = financeClearance.OldestDueDate;

            if (clearance.IsFinanceCleared != financeClearance.IsCleared)
            {
                clearance.IsFinanceCleared = financeClearance.IsCleared;
                RecalculateProgress(clearance);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return MapToDetailResponse(clearance, outstanding, openCount, oldestDue);
    }

    public async Task<ExitClearanceDetailResponse> CreateAsync(
        CreateExitClearanceRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var separation = await _dbContext.TrxEmployeeSeparations
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.Id == request.EmployeeSeparationId && !s.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"EmployeeSeparation dengan Id {request.EmployeeSeparationId} tidak ditemukan.");

        var existing = await _dbContext.TrxExitClearances
            .FirstOrDefaultAsync(x => x.EmployeeSeparationId == request.EmployeeSeparationId && !x.IsDelete, cancellationToken);

        if (existing is not null)
        {
            return await GetDetailAsync(existing.Id, cancellationToken);
        }

        var now = DateTime.UtcNow;
        bool isFinanceCleared = false;
        decimal outstanding = 0m;
        int openCount = 0;
        DateOnly? oldestDue = null;

        if (separation.EmployeeId.HasValue)
        {
            var financeClearance = await _financeClearanceService.GetAsync(separation.EmployeeId.Value, cancellationToken);
            isFinanceCleared = financeClearance.IsCleared;
            outstanding = financeClearance.OutstandingAmount;
            openCount = financeClearance.OpenReceivableCount;
            oldestDue = financeClearance.OldestDueDate;
        }

        var clearance = new TrxExitClearance
        {
            Id = Guid.NewGuid(),
            ClearanceNumber = $"EXC-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            EmployeeSeparationId = separation.Id,
            WorkforceProfileId = separation.WorkforceProfileId,
            EmployeeId = separation.EmployeeId,
            StartDate = request.StartDate ?? now,
            DueDate = request.DueDate ?? separation.EffectiveSeparationDate,
            ClearanceStatus = "Draft",
            IsFinanceCleared = isFinanceCleared,
            IsDepartmentCleared = false,
            IsAssetCleared = false,
            IsPayrollCleared = false,
            IsItAccessCleared = false,
            IsHrCleared = false,
            TotalItem = 6,
            ClearedItem = isFinanceCleared ? 1 : 0,
            ProgressPercentage = isFinanceCleared ? (1m / 6m * 100m) : 0m,
            Notes = request.Notes,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.TrxExitClearances.Add(clearance);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "ExitClearance.Created",
            $"Exit clearance {clearance.ClearanceNumber} dibuat untuk pegawai {separation.EmployeeId}. Status Finance Bebas Tanggungan: {isFinanceCleared}.",
            new { ClearanceId = clearance.Id, SeparationId = separation.Id, EmployeeId = separation.EmployeeId, IsFinanceCleared = isFinanceCleared });

        return await GetDetailAsync(clearance.Id, cancellationToken);
    }

    /// <summary>
    /// Memperbarui checklist aspek departemen, aset, IT, dan HR.
    /// AC 3: Flag IsFinanceCleared TIDAK DAPAT diubah secara manual di sini.
    /// </summary>
    public async Task<ExitClearanceDetailResponse> UpdateChecklistAsync(
        Guid clearanceId, UpdateExitClearanceChecklistRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .FirstOrDefaultAsync(x => x.Id == clearanceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance dengan Id {clearanceId} tidak ditemukan.");

        if (clearance.ClearanceStatus == "Completed" || clearance.ClearanceStatus == "Cancelled")
        {
            throw new InvalidOperationException($"Exit clearance berstatus {clearance.ClearanceStatus} tidak dapat diubah.");
        }

        if (request.IsDepartmentCleared.HasValue) clearance.IsDepartmentCleared = request.IsDepartmentCleared.Value;
        if (request.IsAssetCleared.HasValue) clearance.IsAssetCleared = request.IsAssetCleared.Value;
        if (request.IsPayrollCleared.HasValue) clearance.IsPayrollCleared = request.IsPayrollCleared.Value;
        if (request.IsItAccessCleared.HasValue) clearance.IsItAccessCleared = request.IsItAccessCleared.Value;
        if (request.IsHrCleared.HasValue) clearance.IsHrCleared = request.IsHrCleared.Value;
        if (request.Notes != null) clearance.Notes = request.Notes;

        if (clearance.ClearanceStatus == "Draft")
        {
            clearance.ClearanceStatus = "InProgress";
        }

        clearance.UpdateDateTime = DateTime.UtcNow;
        clearance.UpdateBy = actorUserId;

        RecalculateProgress(clearance);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetDetailAsync(clearance.Id, cancellationToken);
    }

    /// <summary>
    /// Memeriksa ulang status bebas tanggungan secara langsung ke Finance API (AC 2, INT-HR-FIN-002 §P.4).
    /// </summary>
    public async Task<ExitClearanceFinanceCheckResponse> VerifyFinanceClearanceAsync(
        Guid clearanceId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == clearanceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance dengan Id {clearanceId} tidak ditemukan.");

        if (!clearance.EmployeeId.HasValue)
        {
            throw new InvalidOperationException("Exit clearance tidak memiliki data pegawai yang valid.");
        }

        var financeClearance = await _financeClearanceService.GetAsync(clearance.EmployeeId.Value, cancellationToken);
        var previousState = clearance.IsFinanceCleared;

        clearance.IsFinanceCleared = financeClearance.IsCleared;
        clearance.UpdateDateTime = DateTime.UtcNow;
        clearance.UpdateBy = actorUserId;

        RecalculateProgress(clearance);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "ExitClearance.FinanceVerified",
            $"Verifikasi tanggungan Finance untuk clearance {clearance.ClearanceNumber}: IsCleared={financeClearance.IsCleared}, Saldo=Rp {financeClearance.OutstandingAmount:N2}.",
            new { clearance.Id, EmployeeId = clearance.EmployeeId, PreviousState = previousState, NewState = financeClearance.IsCleared, financeClearance.OutstandingAmount });

        return new ExitClearanceFinanceCheckResponse
        {
            ClearanceId = clearance.Id,
            EmployeeId = clearance.EmployeeId.Value,
            EmployeeName = clearance.Employee?.FullName ?? string.Empty,
            IsFinanceCleared = financeClearance.IsCleared,
            OutstandingAmount = financeClearance.OutstandingAmount,
            OpenReceivableCount = financeClearance.OpenReceivableCount,
            OldestDueDate = financeClearance.OldestDueDate,
            CheckedAt = financeClearance.CalculatedAt,
            Message = financeClearance.IsCleared
                ? "Pegawai telah bebas dari seluruh kewajiban piutang Finance."
                : $"Pegawai masih memiliki sisa piutang sebesar Rp {financeClearance.OutstandingAmount:N2} ({financeClearance.OpenReceivableCount} kartu piutang)."
        };
    }

    /// <summary>
    /// Menyelesaikan exit clearance (AC 1 & AC 2).
    /// Gerbang penegakan: DITOLAK bila IsFinanceCleared == false / pegawai masih memiliki sisa piutang di Finance.
    /// </summary>
    public async Task<ExitClearanceDetailResponse> CompleteAsync(
        Guid clearanceId, CompleteExitClearanceRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .Include(x => x.Employee)
            .Include(x => x.EmployeeSeparation)
            .FirstOrDefaultAsync(x => x.Id == clearanceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance dengan Id {clearanceId} tidak ditemukan.");

        if (clearance.ClearanceStatus == "Completed")
        {
            return await GetDetailAsync(clearance.Id, cancellationToken);
        }

        if (clearance.ClearanceStatus == "Cancelled")
        {
            throw new InvalidOperationException("Exit clearance yang sudah dibatalkan tidak dapat diselesaikan.");
        }

        // AC 1 & AC 2 (FIN-DEC-170, FIN-DEC-193): Verifikasi live status Finance
        if (clearance.EmployeeId.HasValue)
        {
            var liveClearance = await _financeClearanceService.GetAsync(clearance.EmployeeId.Value, cancellationToken);
            clearance.IsFinanceCleared = liveClearance.IsCleared;

            if (!liveClearance.IsCleared)
            {
                RecalculateProgress(clearance);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // AC 1: TOLAK PENYELESAIAN BILA BELUM BEBAS TANGGUNGAN
                throw new InvalidOperationException(
                    $"Pegawai belum bebas tanggungan piutang RS (Sisa piutang: Rp {liveClearance.OutstandingAmount:N2}, " +
                    $"{liveClearance.OpenReceivableCount} kartu piutang aktif). Exit clearance terkunci dan tidak dapat diselesaikan.");
            }
        }

        var now = DateTime.UtcNow;
        clearance.ClearanceStatus = "Completed";
        clearance.CompletedAt = now;
        clearance.CompletedByUserId = actorUserId;
        if (request.Notes != null) clearance.Notes = request.Notes;
        clearance.UpdateDateTime = now;
        clearance.UpdateBy = actorUserId;

        RecalculateProgress(clearance);

        // Perbarui status separation di HR
        if (clearance.EmployeeSeparation != null)
        {
            clearance.EmployeeSeparation.IsExitClearanceCompleted = true;
            clearance.EmployeeSeparation.UpdateDateTime = now;
            clearance.EmployeeSeparation.UpdateBy = actorUserId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "ExitClearance.Completed",
            $"Exit clearance {clearance.ClearanceNumber} berhasil diselesaikan. Pegawai {clearance.EmployeeId} terverifikasi bebas tanggungan Finance.",
            new { clearance.Id, clearance.EmployeeSeparationId, EmployeeId = clearance.EmployeeId, ActorUserId = actorUserId });

        return await GetDetailAsync(clearance.Id, cancellationToken);
    }

    public async Task<ExitClearanceDetailResponse> CancelAsync(
        Guid clearanceId, CancelExitClearanceRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var clearance = await _dbContext.TrxExitClearances
            .FirstOrDefaultAsync(x => x.Id == clearanceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"ExitClearance dengan Id {clearanceId} tidak ditemukan.");

        if (clearance.ClearanceStatus == "Completed")
        {
            throw new InvalidOperationException("Exit clearance yang sudah selesai tidak dapat dibatalkan.");
        }

        clearance.ClearanceStatus = "Cancelled";
        clearance.Notes = string.IsNullOrWhiteSpace(clearance.Notes)
            ? $"Dibatalkan: {request.Reason}"
            : $"{clearance.Notes} | Dibatalkan: {request.Reason}";
        clearance.UpdateDateTime = DateTime.UtcNow;
        clearance.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "ExitClearance.Cancelled",
            $"Exit clearance {clearance.ClearanceNumber} dibatalkan. Alasan: {request.Reason}",
            new { clearance.Id, request.Reason, ActorUserId = actorUserId });

        return await GetDetailAsync(clearance.Id, cancellationToken);
    }

    private static void RecalculateProgress(TrxExitClearance clearance)
    {
        var clearedCount = 0;
        if (clearance.IsDepartmentCleared) clearedCount++;
        if (clearance.IsAssetCleared) clearedCount++;
        if (clearance.IsFinanceCleared) clearedCount++;
        if (clearance.IsPayrollCleared) clearedCount++;
        if (clearance.IsItAccessCleared) clearedCount++;
        if (clearance.IsHrCleared) clearedCount++;

        clearance.TotalItem = 6;
        clearance.ClearedItem = clearedCount;
        clearance.ProgressPercentage = Math.Round((decimal)clearedCount / 6m * 100m, 2);
    }

    private static ExitClearanceDetailResponse MapToDetailResponse(
        TrxExitClearance x, decimal outstanding, int openCount, DateOnly? oldestDue)
    {
        return new ExitClearanceDetailResponse
        {
            Id = x.Id,
            ClearanceNumber = x.ClearanceNumber,
            EmployeeSeparationId = x.EmployeeSeparationId,
            SeparationNumber = x.EmployeeSeparation?.SeparationNumber ?? string.Empty,
            WorkforceProfileId = x.WorkforceProfileId,
            EmployeeId = x.EmployeeId,
            EmployeeNumber = x.Employee?.EmployeeNumber ?? string.Empty,
            EmployeeName = x.Employee?.FullName ?? string.Empty,
            DepartmentName = string.Empty,
            PositionName = string.Empty,
            StartDate = x.StartDate,
            DueDate = x.DueDate,
            CompletedAt = x.CompletedAt,
            ClearanceStatus = x.ClearanceStatus,
            ProgressPercentage = x.ProgressPercentage,
            IsDepartmentCleared = x.IsDepartmentCleared,
            IsAssetCleared = x.IsAssetCleared,
            IsFinanceCleared = x.IsFinanceCleared,
            IsPayrollCleared = x.IsPayrollCleared,
            IsItAccessCleared = x.IsItAccessCleared,
            IsHrCleared = x.IsHrCleared,
            OutstandingReceivableAmount = outstanding,
            OpenReceivableCount = openCount,
            OldestDueDate = oldestDue,
            CanComplete = x.IsFinanceCleared && x.ClearanceStatus != "Completed" && x.ClearanceStatus != "Cancelled",
            Notes = x.Notes,
            CompletedByUserId = x.CompletedByUserId,
            CompletedByName = x.CompletedByUser?.UserName ?? string.Empty,
            TotalItem = x.TotalItem,
            ClearedItem = x.ClearedItem,
            OffboardingChecklistId = x.OffboardingChecklistId,
            CreateDateTime = x.CreateDateTime
        };
    }
}

