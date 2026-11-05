using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Impor data HRD ke payroll run (PAY-BE-04). HRD tetap pemilik absensi, cuti, dan lembur; Payroll hanya meminta HRD menyerahkan
/// snapshot yang sudah disahkan ke run ini lewat layanan handoff HRD yang sudah ada (idempoten: kunci sumber + versi + penerima run).
/// Payroll TIDAK memodifikasi data sumber HRD dan tidak membuat ulang logika kelayakannya.
///
/// PENTING: orkestrator ini sengaja TIDAK membungkus pemanggilan dalam transaksi. Setiap layanan handoff HRD membuka transaksi
/// Serializable sendiri; transaksi bersarang pada DbContext yang sama akan gagal. Atomisitas per sumber dijaga masing-masing
/// layanan HRD, dan kegagalan satu sumber tidak membatalkan sumber lain (hasil dilaporkan per sumber dan per item gagal).
/// </summary>
public sealed class PayrollHrdImportService
{
    private const string LogCategory = "Corporate.PayrollManagement.HrdImport";
    private const int MaxOvertimeItemsPerImport = 2000;
    private const int MaxReportedItemsPerSource = 200;

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly AttendancePayrollHandoffService _attendanceHandoff;
    private readonly LeavePayrollIntegrationService _leaveIntegration;
    private readonly OvertimePayrollHandoffQueryService _overtimeQuery;
    private readonly OvertimePayrollHandoffService _overtimeHandoff;

    public PayrollHrdImportService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        AttendancePayrollHandoffService attendanceHandoff,
        LeavePayrollIntegrationService leaveIntegration,
        OvertimePayrollHandoffQueryService overtimeQuery,
        OvertimePayrollHandoffService overtimeHandoff)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _attendanceHandoff = attendanceHandoff;
        _leaveIntegration = leaveIntegration;
        _overtimeQuery = overtimeQuery;
        _overtimeHandoff = overtimeHandoff;
    }

    // ------------------------------------------------------------------------------------
    // Kesiapan (hanya baca)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunHrdReadinessResponse> GetReadinessAsync(
        Guid runId, PayrollHrdReadinessQuery query, CancellationToken cancellationToken)
    {
        var run = await _dbContext.TrxPayrollRuns.AsNoTracking()
            .Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var employees = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
            .Where(x => x.PayrollRunId == runId && !x.IsDelete)
            .OrderBy(x => x.EmployeeNameSnapshot).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumberSnapshot, x.EmployeeNameSnapshot, x.PayrollProfileId })
            .ToListAsync(cancellationToken);
        var runEmployeeIds = employees.Select(x => x.Id).ToList();
        var workforceIds = employees.Select(x => x.WorkforceProfileId).Distinct().ToList();

        var attendanceCounts = await CountByRunEmployeeAsync(
            _dbContext.TrxPayrollAttendanceInputs.AsNoTracking()
                .Where(a => !a.IsDelete && a.IsActive && runEmployeeIds.Contains(a.PayrollRunEmployeeId))
                .Select(a => a.PayrollRunEmployeeId), cancellationToken);
        var overtimeCounts = await CountByRunEmployeeAsync(
            _dbContext.TrxPayrollOvertimeInputs.AsNoTracking()
                .Where(o => !o.IsDelete && o.IsActive && runEmployeeIds.Contains(o.PayrollRunEmployeeId))
                .Select(o => o.PayrollRunEmployeeId), cancellationToken);
        var variableCounts = await CountByRunEmployeeAsync(
            _dbContext.TrxPayrollVariableInputs.AsNoTracking()
                .Where(v => !v.IsDelete && v.IsActive && runEmployeeIds.Contains(v.PayrollRunEmployeeId)
                    && (v.SourceType == null || v.SourceType != PayrollVariableInputSources.FinanceReceivableInstallment))
                .Select(v => v.PayrollRunEmployeeId), cancellationToken);

        var periodStart = run.PeriodStartDateSnapshot;
        var periodEnd = run.PeriodEndDateSnapshot;
        var approvedSalary = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(a => !a.IsDelete && a.IsActive && a.IsPrimary && a.ApprovedAt != null
                    && workforceIds.Contains(a.WorkforceProfileId)
                    && a.EffectiveStartDate <= periodEnd && (a.EffectiveEndDate == null || a.EffectiveEndDate >= periodStart))
                .Select(a => a.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var verifiedBank = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(b => !b.IsDelete && b.IsActive && b.IsPayrollAccount && b.IsVerified && workforceIds.Contains(b.WorkforceProfileId))
                .Select(b => b.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        var rows = employees.Select(e =>
        {
            var item = new PayrollHrdReadinessItemResponse
            {
                PayrollRunEmployeeId = e.Id,
                WorkforceProfileId = e.WorkforceProfileId,
                EmployeeNumber = e.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = e.EmployeeNameSnapshot,
                HasPayrollProfile = e.PayrollProfileId.HasValue,
                HasApprovedSalaryAssignment = approvedSalary.Contains(e.WorkforceProfileId),
                HasVerifiedBankAccount = verifiedBank.Contains(e.WorkforceProfileId),
                AttendanceDayCount = attendanceCounts.GetValueOrDefault(e.Id),
                OvertimeItemCount = overtimeCounts.GetValueOrDefault(e.Id),
                VariableInputCount = variableCounts.GetValueOrDefault(e.Id)
            };
            if (!item.HasPayrollProfile) item.Issues.Add("Profil payroll belum terhubung (akan dilengkapi saat run dihitung bila profil aktif ada).");
            if (!item.HasApprovedSalaryAssignment) item.Issues.Add("Belum ada penetapan gaji utama yang disetujui dan berlaku pada periode.");
            if (!item.HasVerifiedBankAccount) item.Issues.Add("Belum ada rekening payroll terverifikasi (menahan instruksi pembayaran).");
            if (item.AttendanceDayCount == 0) item.Issues.Add("Belum ada data kehadiran yang diserahkan HRD.");
            return item;
        }).ToList();

        var response = new PayrollRunHrdReadinessResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            RunStatus = run.RunStatus,
            IsLocked = run.IsLocked,
            PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            AttendanceCutoffDate = run.AttendanceCutoffDateSnapshot,
            VariableInputCutoffDate = run.VariableInputCutoffDateSnapshot,
            TotalEmployees = rows.Count,
            EmployeesWithAttendance = rows.Count(r => r.AttendanceDayCount > 0),
            EmployeesWithOvertime = rows.Count(r => r.OvertimeItemCount > 0),
            EmployeesWithVariableInput = rows.Count(r => r.VariableInputCount > 0),
            EmployeesWithoutApprovedSalary = rows.Count(r => !r.HasApprovedSalaryAssignment),
            EmployeesWithoutVerifiedBank = rows.Count(r => !r.HasVerifiedBankAccount),
            EmployeesWithIssues = rows.Count(r => r.Issues.Count > 0),
            CanImport = !run.IsLocked && PayrollRunStatuses.AcceptsInputs(run.RunStatus) && rows.Count > 0
        };

        IEnumerable<PayrollHrdReadinessItemResponse> filtered = rows;
        if (query.OnlyIssues) filtered = filtered.Where(r => r.Issues.Count > 0);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = filtered.Where(r => r.EmployeeName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.EmployeeNumber.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        var list = filtered.ToList();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        response.Employees = new PagedResult<PayrollHrdReadinessItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = list.Count,
            TotalPage = (int)Math.Ceiling(list.Count / (double)pageSize),
            Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
        return response;
    }

    private static async Task<Dictionary<Guid, int>> CountByRunEmployeeAsync(IQueryable<Guid> ids, CancellationToken cancellationToken)
    {
        var rows = await ids.GroupBy(id => id).Select(g => new { Id = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        return rows.ToDictionary(x => x.Id, x => x.Count);
    }

    // ------------------------------------------------------------------------------------
    // Impor
    // ------------------------------------------------------------------------------------

    public async Task<PayrollHrdImportResponse> ImportAsync(
        Guid runId, PayrollHrdImportRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var run = await _dbContext.TrxPayrollRuns.AsNoTracking()
            .Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        PayrollVersionToken.EnsureMatches(request.ExpectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
        if (run.IsLocked || !PayrollRunStatuses.AcceptsInputs(run.RunStatus))
            throw new PayrollInvalidTransitionException($"Payroll run {run.RunNumber} berstatus {run.RunStatus} dan tidak lagi menerima impor data HRD.");
        if (run.PayrollPeriod is null || run.PayrollPeriod.IsLocked)
            throw new PayrollInvalidTransitionException("Periode payroll sudah terkunci.");

        var response = new PayrollHrdImportResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            StartedAt = DateTime.UtcNow
        };
        var periodStart = DateOnly.FromDateTime(run.PeriodStartDateSnapshot);
        var periodEnd = DateOnly.FromDateTime(run.PeriodEndDateSnapshot);

        if (request.IncludeAttendance) response.Sources.Add(await ImportAttendanceAsync(runId, request, actorUserId, cancellationToken));
        if (request.IncludeLeave) response.Sources.Add(await ImportLeaveAsync(runId, request, actorUserId, cancellationToken));
        if (request.IncludeOvertime)
            response.Sources.Add(await ImportOvertimeAsync(runId, periodStart, periodEnd, request, actorUserId, cancellationToken));

        response.TotalFailed = response.Sources.Sum(s => s.FailedCount);
        response.HasIssues = response.TotalFailed > 0 || response.Sources.Any(s => s.Status == "Failed");

        // Run maju ke CollectingInput hanya bila ada sumber yang benar-benar berhasil diproses (bukan semuanya gagal).
        var anySucceeded = response.Sources.Any(s => s.Status is "Completed" or "CompletedWithIssues");
        var tracked = await _dbContext.TrxPayrollRuns.SingleAsync(x => x.Id == runId, cancellationToken);
        if (anySucceeded && string.Equals(tracked.RunStatus, PayrollRunStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            tracked.RunStatus = PayrollRunStatuses.CollectingInput;
            tracked.UpdateDateTime = DateTime.UtcNow;
            tracked.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        response.RunStatus = tracked.RunStatus;
        response.CompletedAt = DateTime.UtcNow;
        await _loggerService.AuditAsync(LogCategory, "PayrollRun.HrdImported",
            $"Impor data HRD ke payroll run {run.RunNumber}: {response.Sources.Count} sumber, {response.TotalFailed} item gagal.",
            new { runId, Sources = response.Sources.Select(s => new { s.Source, s.Status, s.FailedCount }), ActorUserId = actorUserId });
        return response;
    }

    private async Task<PayrollHrdSourceResultResponse> ImportAttendanceAsync(
        Guid runId, PayrollHrdImportRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var result = new PayrollHrdSourceResultResponse { Source = "Attendance" };
        try
        {
            var handoff = await _attendanceHandoff.ExecuteAsync(runId,
                new ExecuteAttendancePayrollHandoffRequest { ContinueOnValidationError = true, Notes = request.Notes },
                actorUserId, cancellationToken);
            if (!handoff.Success || handoff.Data is null)
            {
                result.Status = "Failed";
                result.Message = handoff.Message;
                return result;
            }

            var data = handoff.Data;
            result.TotalTarget = data.TotalTarget;
            result.CreatedCount = data.CreatedCount;
            result.UpdatedCount = data.UpdatedCount;
            result.IdempotentCount = data.IdempotentCount;
            result.FailedCount = data.FailedCount;
            if (data.LockedAttendanceCount > 0)
                result.Warnings.Add($"{data.LockedAttendanceCount} data kehadiran sudah terkunci di HRD dan tidak diubah.");
            result.Items = data.Items.Where(i => !i.Success).Take(MaxReportedItemsPerSource).Select(i => new PayrollHrdImportItemResponse
            {
                Reference = i.AttendanceDailyId.ToString(),
                WorkforceName = i.WorkforceDisplayName,
                Date = i.AttendanceDate,
                Success = false,
                Result = i.ResultStatus,
                Message = i.Message
            }).ToList();
            result.Status = result.FailedCount > 0 ? "CompletedWithIssues" : "Completed";
            result.Message = handoff.Message;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await FailSourceAsync(result, ex);
        }
        return result;
    }

    private async Task<PayrollHrdSourceResultResponse> ImportLeaveAsync(
        Guid runId, PayrollHrdImportRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var result = new PayrollHrdSourceResultResponse { Source = "Leave" };
        try
        {
            var integration = await _leaveIntegration.ExecuteAsync(runId,
                new ExecuteLeavePayrollIntegrationRequest { ContinueOnValidationError = true, Notes = request.Notes },
                actorUserId, cancellationToken);
            if (!integration.Success || integration.Data is null)
            {
                result.Status = "Failed";
                result.Message = integration.Message;
                return result;
            }

            var data = integration.Data;
            result.TotalTarget = data.TotalTarget;
            result.CreatedCount = data.VariableInputCreatedCount;
            result.UpdatedCount = data.VariableInputUpdatedCount + data.AttendanceInputUpdatedCount;
            result.IdempotentCount = data.IdempotentCount;
            result.FailedCount = data.FailedCount;
            result.Warnings.AddRange(data.Warnings);
            result.Items = data.Items.Where(i => !i.Success).Take(MaxReportedItemsPerSource).Select(i => new PayrollHrdImportItemResponse
            {
                Reference = i.SourceId?.ToString() ?? i.LeaveAttendanceIntegrationId?.ToString() ?? string.Empty,
                Date = i.EffectiveDate,
                Success = false,
                Result = i.ResultStatus,
                Message = i.Message
            }).ToList();
            result.Status = result.FailedCount > 0 ? "CompletedWithIssues" : "Completed";
            result.Message = integration.Message;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await FailSourceAsync(result, ex);
        }
        return result;
    }

    private async Task<PayrollHrdSourceResultResponse> ImportOvertimeAsync(
        Guid runId, DateOnly periodStart, DateOnly periodEnd, PayrollHrdImportRequest request, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var result = new PayrollHrdSourceResultResponse { Source = "Overtime" };
        try
        {
            // Kumpulkan dulu seluruh realisasi lembur "Ready" (tanpa memutasi), baru diposting satu per satu. Memposting sambil
            // membaca halaman akan menggeser isi halaman karena status berpindah dari Ready ke Posted.
            var candidates = new List<OvertimePayrollHandoffListResponse>();
            var query = new OvertimePayrollHandoffQueryRequest
            {
                HandoffStatus = OvertimeValueConstants.PayrollHandoffStatus.Ready,
                StartDate = periodStart,
                EndDate = periodEnd,
                ExcludeCompensatoryLeave = true,
                SortBy = "overtimeDate",
                SortDirection = "asc",
                PageNumber = 1,
                PageSize = 200
            };
            var first = await _overtimeQuery.GetPagedAsync(query, cancellationToken);
            candidates.AddRange(first.Items);
            for (var page = 2; page <= first.TotalPage && candidates.Count < MaxOvertimeItemsPerImport; page++)
            {
                query.PageNumber = page;
                candidates.AddRange((await _overtimeQuery.GetPagedAsync(query, cancellationToken)).Items);
            }
            if (first.TotalData > candidates.Count)
                result.Warnings.Add($"Hanya {candidates.Count} dari {first.TotalData} realisasi lembur diproses pada impor ini; jalankan impor lagi untuk sisanya.");

            result.TotalTarget = candidates.Count;
            foreach (var candidate in candidates.Take(MaxOvertimeItemsPerImport))
            {
                var post = await _overtimeHandoff.PostAsync(candidate.OvertimeRealizationId,
                    new PostOvertimePayrollHandoffRequest
                    {
                        PayrollRunId = runId,
                        Notes = request.Notes,
                        IdempotencyKey = $"payroll-import-{runId:N}-{candidate.OvertimeRealizationId:N}"
                    }, actorUserId, cancellationToken);

                if (post.Success && post.Data is not null)
                {
                    if (post.Data.IsExisting) result.IdempotentCount++;
                    else result.CreatedCount++;
                    continue;
                }

                result.FailedCount++;
                if (result.Items.Count < MaxReportedItemsPerSource)
                {
                    result.Items.Add(new PayrollHrdImportItemResponse
                    {
                        Reference = candidate.RealizationNumber,
                        WorkforceName = candidate.EmployeeName,
                        Date = candidate.OvertimeDate,
                        Success = false,
                        Result = "Gagal",
                        Message = post.Message
                    });
                }
            }
            result.Status = result.FailedCount > 0 ? "CompletedWithIssues" : "Completed";
            result.Message = candidates.Count == 0
                ? "Tidak ada realisasi lembur terverifikasi yang menunggu diserahkan pada periode ini."
                : $"{result.CreatedCount} realisasi lembur diserahkan, {result.IdempotentCount} sudah ada, {result.FailedCount} gagal.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await FailSourceAsync(result, ex);
        }
        return result;
    }

    /// <summary>
    /// Kegagalan tak terduga satu sumber tidak boleh merusak sumber berikutnya: state DbContext dibersihkan (transaksi HRD sudah
    /// dibatalkan oleh layanan itu sendiri), kegagalan dicatat ke log, dan pengguna menerima pesan umum tanpa detail internal.
    /// </summary>
    private async Task<PayrollHrdSourceResultResponse> FailSourceAsync(PayrollHrdSourceResultResponse result, Exception exception)
    {
        _dbContext.ChangeTracker.Clear();
        await _loggerService.ErrorAsync(LogCategory, "PayrollRun.HrdImportFailed",
            $"Impor {result.Source} dari HRD gagal.", exception, new { result.Source });
        result.Status = "Failed";
        result.Message = $"Impor {result.Source} dari HRD gagal karena kesalahan tak terduga. Coba lagi; bila berulang, hubungi administrator (lihat log).";
        return result;
    }
}
