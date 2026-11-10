using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pembuatan, penerbitan, dan akses slip gaji (PAY-BE-09).
///
/// - Slip dibangun dari snapshot run yang sudah DIBEKUKAN (komponen dan total penerima), bukan dari data gaji yang berjalan. Isi snapshot
///   tidak memuat cap waktu, sehingga membuat ulang slip yang belum terbit menghasilkan checksum identik (idempoten).
/// - Slip yang sudah diterbitkan immutable: tidak dibuat ulang. Koreksi pascapenerbitan lewat run tambahan, bukan menimpa slip.
/// - Penerbitan mengikuti kebijakan akses slip (NEEDS CONFIRMATION): PaymentPaid (setelah pembayaran terkonfirmasi) atau RunApproved.
/// - Akses penerima diturunkan dari pengguna yang login (ApplicationUser.WorkforceProfileId); identitas penerima tidak pernah diterima
///   dari klien. Slip milik orang lain dijawab 404, bukan 403, agar keberadaannya tidak bocor. Setiap unduhan dicatat audit.
/// </summary>
public sealed class PayrollPayslipService
{
    private const string LogCategory = "Corporate.PayrollManagement.Payslip";
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollPayslipService(ApplicationDbContext dbContext, LoggerService loggerService, IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _policy = policyOptions.Value;
    }

    // ------------------------------------------------------------------------------------
    // Buat
    // ------------------------------------------------------------------------------------

    public Task<PayrollPayslipGenerateResponse> GenerateAsync(Guid runId, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(runId), async () =>
        {
            var run = await _dbContext.TrxPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod)
                .SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
            if (!PayrollRunStatuses.AllowsPayslipGeneration(run.RunStatus))
                throw new PayrollInvalidTransitionException($"Slip hanya dapat dibuat dari payroll run yang sudah disahkan (saat ini {run.RunStatus}).");

            var employees = await _dbContext.TrxPayrollRunEmployees.AsNoTracking()
                .Include(x => x.Components)
                .Where(x => x.PayrollRunId == runId && !x.IsDelete && x.IsFinalized)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
            var existing = (await _dbContext.TrxPayrollPayslips.Where(x => x.PayrollRunId == runId && !x.IsDelete).ToListAsync(cancellationToken))
                .ToDictionary(x => x.PayrollRunEmployeeId);

            var response = new PayrollPayslipGenerateResponse { PayrollRunId = run.Id, RunNumber = run.RunNumber, TotalEmployees = employees.Count };
            var now = DateTime.UtcNow;
            var ordinal = 0;
            foreach (var employee in employees)
            {
                ordinal++;
                var number = $"SLP-{run.RunNumber}-{ordinal:D4}";
                var snapshot = BuildSnapshot(run, employee, number);
                var json = JsonSerializer.Serialize(snapshot);
                var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));

                if (!existing.TryGetValue(employee.Id, out var payslip))
                {
                    payslip = new TrxPayrollPayslip
                    {
                        PayrollRunId = run.Id,
                        PayrollRunEmployeeId = employee.Id,
                        PayslipNumber = number,
                        VersionNumber = 1,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    };
                    Apply(payslip, run, employee, snapshot, json, checksum, actorUserId, now);
                    _dbContext.TrxPayrollPayslips.Add(payslip);
                    response.GeneratedCount++;
                    continue;
                }

                if (payslip.PayslipStatus == PayrollPayslipStatuses.Published || payslip.PayslipStatus == PayrollPayslipStatuses.Downloaded)
                {
                    response.SkippedPublishedCount++;
                    continue;
                }
                if (string.Equals(payslip.FileChecksum, checksum, StringComparison.Ordinal))
                {
                    response.UnchangedCount++;
                    continue;
                }

                payslip.VersionNumber++;
                Apply(payslip, run, employee, snapshot, json, checksum, actorUserId, now);
                payslip.UpdateDateTime = now;
                payslip.UpdateBy = actorUserId;
                response.RegeneratedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "Payslip.Generated",
                $"Slip payroll run {run.RunNumber}: {response.GeneratedCount} dibuat, {response.RegeneratedCount} dibuat ulang, {response.UnchangedCount} tidak berubah, {response.SkippedPublishedCount} sudah terbit.",
                new { run.Id, response.GeneratedCount, response.RegeneratedCount, response.UnchangedCount, response.SkippedPublishedCount, ActorUserId = actorUserId });
            return response;
        }, cancellationToken);

    private static void Apply(
        TrxPayrollPayslip payslip, TrxPayrollRun run, TrxPayrollRunEmployee employee, PayrollPayslipSnapshot snapshot,
        string json, string checksum, Guid actorUserId, DateTime now)
    {
        payslip.PayslipStatus = PayrollPayslipStatuses.Generated;
        payslip.CurrencyCode = snapshot.CurrencyCode;
        payslip.PeriodStartDateSnapshot = run.PeriodStartDateSnapshot;
        payslip.PeriodEndDateSnapshot = run.PeriodEndDateSnapshot;
        payslip.EmployeeNumberSnapshot = employee.EmployeeNumberSnapshot;
        payslip.EmployeeNameSnapshot = employee.EmployeeNameSnapshot;
        payslip.DepartmentNameSnapshot = employee.DepartmentNameSnapshot;
        payslip.PositionNameSnapshot = employee.PositionNameSnapshot;
        payslip.BaseSalary = employee.BaseSalary;
        payslip.TotalEarning = snapshot.TotalEarning;
        payslip.TotalDeduction = snapshot.TotalDeduction;
        payslip.TotalTax = employee.TotalTax;
        payslip.GrossPay = employee.GrossPay;
        payslip.NetPay = employee.NetPay;
        payslip.FileName = $"Slip-{snapshot.PayslipNumber}.pdf";
        payslip.ContentType = "application/pdf";
        payslip.FileChecksum = checksum;
        payslip.PayslipSnapshotJson = json;
        payslip.GeneratedAt = now;
        payslip.GeneratedByUserId = actorUserId;
        payslip.IsSnapshotFrozen = true;
    }

    /// <summary>
    /// Isi slip dari komponen run yang dibekukan. Bila jumlah baris komponen tidak sama dengan total penerima (mis. run lama yang
    /// dihitung sebelum ada rincian komponen), selisihnya ditampilkan sebagai satu baris "tanpa rincian" supaya slip tetap
    /// menjumlah tepat ke total run dan tidak ada angka yang disembunyikan.
    /// </summary>
    private PayrollPayslipSnapshot BuildSnapshot(TrxPayrollRun run, TrxPayrollRunEmployee employee, string payslipNumber)
    {
        var live = employee.Components.Where(c => !c.IsDelete && c.Amount != 0m).OrderBy(c => c.SortOrder).ThenBy(c => c.ComponentCodeSnapshot).ToList();
        PayrollPayslipLine ToLine(TrxPayrollEmployeeComponent c) => new()
        {
            Code = c.ComponentCodeSnapshot,
            Name = c.ComponentCodeSnapshot == InstallmentComponentCode ? "Potongan Cicilan Piutang RS" : c.ComponentNameSnapshot,
            Amount = c.Amount,
            IsProrated = c.IsProrated
        };

        var earnings = live.Where(c => IsType(c, "Earning") || IsType(c, "Benefit")).Select(ToLine).ToList();
        var deductions = live.Where(c => IsType(c, "Deduction") || IsType(c, "Tax") || IsType(c, "Insurance")).Select(ToLine).ToList();

        var earningDifference = Math.Round(employee.GrossPay - earnings.Sum(l => l.Amount), 2);
        if (earningDifference != 0m)
            earnings.Add(new PayrollPayslipLine { Code = "TANPA_RINCIAN", Name = "Penghasilan lain (tanpa rincian komponen)", Amount = earningDifference });
        var deductionDifference = Math.Round(employee.TotalDeduction - deductions.Sum(l => l.Amount), 2);
        if (deductionDifference != 0m)
            deductions.Add(new PayrollPayslipLine { Code = "TANPA_RINCIAN", Name = "Potongan lain (tanpa rincian komponen)", Amount = deductionDifference });

        var account = employee.BankAccountNumberSnapshot;
        return new PayrollPayslipSnapshot
        {
            PayslipNumber = payslipNumber,
            RunNumber = run.RunNumber,
            PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            PeriodName = run.PayrollPeriod?.PayrollPeriodName ?? string.Empty,
            PeriodStartDate = run.PeriodStartDateSnapshot,
            PeriodEndDate = run.PeriodEndDateSnapshot,
            PaymentDate = run.PaymentDateSnapshot,
            EmployeeNumber = employee.EmployeeNumberSnapshot ?? string.Empty,
            EmployeeName = employee.EmployeeNameSnapshot,
            DepartmentName = employee.DepartmentNameSnapshot,
            PositionName = employee.PositionNameSnapshot,
            TaxStatus = employee.TaxStatusSnapshot,
            BankName = employee.BankNameSnapshot,
            BankAccountMasked = string.IsNullOrWhiteSpace(account) ? null
                : account.Trim().Length <= 4 ? new string('*', account.Trim().Length) : new string('*', account.Trim().Length - 4) + account.Trim()[^4..],
            CurrencyCode = employee.CurrencyCode,
            Earnings = earnings,
            Deductions = deductions,
            TotalEarning = employee.GrossPay,
            TotalDeduction = employee.TotalDeduction,
            NetPay = employee.NetPay,
            PolicyVersion = _policy.PolicyVersion
        };
    }

    private static bool IsType(TrxPayrollEmployeeComponent component, string type) =>
        string.Equals(component.ComponentTypeSnapshot, type, StringComparison.OrdinalIgnoreCase);

    // ------------------------------------------------------------------------------------
    // Terbit
    // ------------------------------------------------------------------------------------

    public Task<PayrollPayslipPublishResponse> PublishAsync(Guid runId, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(runId), async () =>
        {
            var run = await _dbContext.TrxPayrollRuns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
            if (!PayrollRunStatuses.AllowsPayslipGeneration(run.RunStatus))
                throw new PayrollInvalidTransitionException($"Slip hanya dapat diterbitkan dari payroll run yang sudah disahkan (saat ini {run.RunStatus}).");

            var trigger = string.Equals(_policy.PayslipPublishTrigger, PayrollPayslipPublishTriggers.RunApproved, StringComparison.OrdinalIgnoreCase)
                ? PayrollPayslipPublishTriggers.RunApproved
                : PayrollPayslipPublishTriggers.PaymentPaid;

            var payslips = await _dbContext.TrxPayrollPayslips
                .Include(x => x.PayrollRunEmployee)
                .Where(x => x.PayrollRunId == runId && !x.IsDelete)
                .ToListAsync(cancellationToken);
            if (payslips.Count == 0)
                throw new PayrollInvalidTransitionException("Belum ada slip yang dibuat untuk run ini; buat slip terlebih dahulu.");

            var paidEmployees = (await _dbContext.TrxPayrollPayments.AsNoTracking()
                    .Where(p => p.PayrollRunId == runId && !p.IsDelete
                        && (p.PaymentStatus == PayrollPaymentStatuses.Paid || p.PaymentStatus == PayrollPaymentStatuses.PartiallyReversed))
                    .Select(p => p.PayrollRunEmployeeId).ToListAsync(cancellationToken)).ToHashSet();

            var response = new PayrollPayslipPublishResponse { PayrollRunId = run.Id, RunNumber = run.RunNumber, PublishTrigger = trigger };
            var now = DateTime.UtcNow;
            foreach (var payslip in payslips)
            {
                if (payslip.PayslipStatus == PayrollPayslipStatuses.Published || payslip.PayslipStatus == PayrollPayslipStatuses.Downloaded)
                {
                    response.AlreadyPublishedCount++;
                    continue;
                }

                var nothingToPay = (payslip.PayrollRunEmployee?.PaymentAmount ?? 0m) <= 0m;
                if (trigger == PayrollPayslipPublishTriggers.PaymentPaid && !nothingToPay && !paidEmployees.Contains(payslip.PayrollRunEmployeeId))
                {
                    response.HeldCount++;
                    if (response.HeldReasons.Count < 5)
                        response.HeldReasons.Add($"{payslip.EmployeeNameSnapshot}: pembayaran belum terkonfirmasi lunas.");
                    continue;
                }

                payslip.PayslipStatus = PayrollPayslipStatuses.Published;
                payslip.IsEmployeeVisible = true;
                payslip.PublishedAt = now;
                payslip.PublishedByUserId = actorUserId;
                payslip.UpdateDateTime = now;
                payslip.UpdateBy = actorUserId;
                response.PublishedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "Payslip.Published",
                $"Slip payroll run {run.RunNumber}: {response.PublishedCount} diterbitkan, {response.HeldCount} ditahan (kebijakan {trigger}).",
                new { run.Id, response.PublishedCount, response.HeldCount, trigger, ActorUserId = actorUserId });
            return response;
        }, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    /// <summary>Daftar slip satu run untuk administrator Payroll. Nominal hanya bila berwenang ViewAmount; PII tidak ikut dikirim.</summary>
    public async Task<PagedResult<PayslipSummaryResponse>> GetByRunAsync(
        Guid runId, PayrollPayslipQuery query, bool includeAmounts, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.TrxPayrollRuns.AsNoTracking().AnyAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var rows = _dbContext.TrxPayrollPayslips.AsNoTracking().Where(x => x.PayrollRunId == runId && !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(query.Status)) rows = rows.Where(x => x.PayslipStatus == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            rows = rows.Where(x => EF.Functions.ILike(x.EmployeeNameSnapshot, term)
                || EF.Functions.ILike(x.EmployeeNumberSnapshot!, term) || EF.Functions.ILike(x.PayslipNumber, term));
        }
        return await PageAsync(rows, query, includeAmounts, cancellationToken);
    }

    /// <summary>Slip MILIK SENDIRI yang sudah diterbitkan. Pemilik diturunkan dari pengguna login, bukan dari parameter.</summary>
    public async Task<PagedResult<PayslipSummaryResponse>> GetMineAsync(Guid userId, PayrollPayslipQuery query, CancellationToken cancellationToken)
    {
        var workforceProfileId = await ResolveWorkforceProfileIdAsync(userId, cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        if (workforceProfileId is null)
            return new PagedResult<PayslipSummaryResponse> { PageNumber = pageNumber, PageSize = pageSize };

        var rows = _dbContext.TrxPayrollPayslips.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsEmployeeVisible
                && (x.PayslipStatus == PayrollPayslipStatuses.Published || x.PayslipStatus == PayrollPayslipStatuses.Downloaded)
                && x.PayrollRunEmployee!.WorkforceProfileId == workforceProfileId.Value);
        if (!string.IsNullOrWhiteSpace(query.Status)) rows = rows.Where(x => x.PayslipStatus == query.Status);
        return await PageAsync(rows, query, includeAmounts: true, cancellationToken);
    }

    private async Task<PagedResult<PayslipSummaryResponse>> PageAsync(
        IQueryable<TrxPayrollPayslip> rows, PayrollPayslipQuery query, bool includeAmounts, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await rows.CountAsync(cancellationToken);
        var page = await rows
            .Include(x => x.PayrollRun).ThenInclude(r => r!.PayrollPeriod)
            .OrderByDescending(x => x.PeriodEndDateSnapshot).ThenBy(x => x.EmployeeNameSnapshot)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<PayslipSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = page.Select(p => MapSummary(new PayslipSummaryResponse(), p, includeAmounts)).ToList()
        };
    }

    public async Task<PayrollPayslipDetailResponse> GetDetailAsync(
        Guid payslipId, Guid? ownerUserId, bool includeAmounts, CancellationToken cancellationToken)
    {
        var payslip = await LoadAuthorizedAsync(payslipId, ownerUserId, cancellationToken);
        var detail = MapSummary(new PayrollPayslipDetailResponse(), payslip, includeAmounts || ownerUserId.HasValue);
        detail.FileChecksum = payslip.FileChecksum;
        detail.Snapshot = ReadSnapshot(payslip.PayslipSnapshotJson);
        if (detail.Snapshot is not null && !(includeAmounts || ownerUserId.HasValue))
        {
            // Administrator tanpa ViewAmount tidak boleh melihat nominal pada isi slip.
            foreach (var line in detail.Snapshot.Earnings.Concat(detail.Snapshot.Deductions)) line.Amount = 0m;
            detail.Snapshot.TotalEarning = 0m;
            detail.Snapshot.TotalDeduction = 0m;
            detail.Snapshot.NetPay = 0m;
        }
        return detail;
    }

    // ------------------------------------------------------------------------------------
    // Unduh (beraudit)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollPayslipFile> DownloadAsync(Guid payslipId, Guid actorUserId, bool ownerOnly, CancellationToken cancellationToken)
    {
        var payslip = await LoadAuthorizedAsync(payslipId, ownerOnly ? actorUserId : null, cancellationToken, tracking: true);
        var snapshot = ReadSnapshot(payslip.PayslipSnapshotJson)
            ?? throw new PayrollInvalidTransitionException("Isi slip tidak tersedia.");

        var now = DateTime.UtcNow;
        payslip.DownloadCount++;
        payslip.FirstDownloadedAt ??= now;
        payslip.LastDownloadedAt = now;
        // Status penerima berubah menjadi Downloaded hanya saat PEMILIK mengunduh, bukan saat administrator mengunduh.
        if (ownerOnly && payslip.PayslipStatus == PayrollPayslipStatuses.Published) payslip.PayslipStatus = PayrollPayslipStatuses.Downloaded;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "Payslip.Downloaded",
            $"Slip {payslip.PayslipNumber} diunduh ({(ownerOnly ? "oleh pemilik" : "oleh administrator")}).",
            new { payslip.Id, payslip.PayrollRunId, payslip.PayrollRunEmployeeId, ByOwner = ownerOnly, ActorUserId = actorUserId });

        return new PayrollPayslipFile
        {
            Content = PayrollPayslipDocuments.BuildPdf(snapshot),
            FileName = payslip.FileName ?? $"Slip-{payslip.PayslipNumber}.pdf",
            ContentType = payslip.ContentType ?? "application/pdf"
        };
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Memuat slip. Bila <paramref name="ownerUserId"/> diisi: hanya slip yang sudah diterbitkan dan milik pengguna itu; selain itu 404
    /// (tidak membedakan "bukan milik Anda" dari "tidak ada" agar keberadaan slip orang lain tidak bocor).
    /// </summary>
    private async Task<TrxPayrollPayslip> LoadAuthorizedAsync(
        Guid payslipId, Guid? ownerUserId, CancellationToken cancellationToken, bool tracking = false)
    {
        var query = tracking ? _dbContext.TrxPayrollPayslips.AsQueryable() : _dbContext.TrxPayrollPayslips.AsNoTracking();
        var payslip = await query
            .Include(x => x.PayrollRun).ThenInclude(r => r!.PayrollPeriod)
            .Include(x => x.PayrollRunEmployee)
            .SingleOrDefaultAsync(x => x.Id == payslipId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Slip gaji tidak ditemukan.");
        if (!ownerUserId.HasValue) return payslip;

        var workforceProfileId = await ResolveWorkforceProfileIdAsync(ownerUserId.Value, cancellationToken);
        var published = payslip.IsEmployeeVisible
            && (payslip.PayslipStatus == PayrollPayslipStatuses.Published || payslip.PayslipStatus == PayrollPayslipStatuses.Downloaded);
        if (!workforceProfileId.HasValue || !published || payslip.PayrollRunEmployee?.WorkforceProfileId != workforceProfileId.Value)
            throw new KeyNotFoundException("Slip gaji tidak ditemukan.");
        return payslip;
    }

    private async Task<Guid?> ResolveWorkforceProfileIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken);

    private static T MapSummary<T>(T target, TrxPayrollPayslip p, bool includeAmounts) where T : PayslipSummaryResponse
    {
        target.Id = p.Id;
        target.PayrollRunId = p.PayrollRunId;
        target.RunNumber = p.PayrollRun?.RunNumber ?? string.Empty;
        target.PeriodCode = p.PayrollRun?.PayrollPeriod?.PayrollPeriodCode ?? string.Empty;
        target.PayslipNumber = p.PayslipNumber;
        target.VersionNumber = p.VersionNumber;
        target.PayslipStatus = p.PayslipStatus;
        target.IsEmployeeVisible = p.IsEmployeeVisible;
        target.EmployeeNumber = p.EmployeeNumberSnapshot ?? string.Empty;
        target.EmployeeName = p.EmployeeNameSnapshot;
        target.NetPay = includeAmounts ? (decimal?)p.NetPay : null;
        target.GeneratedAt = p.GeneratedAt;
        target.PublishedAt = p.PublishedAt;
        target.DownloadCount = p.DownloadCount;
        target.FirstDownloadedAt = p.FirstDownloadedAt;
        target.LastDownloadedAt = p.LastDownloadedAt;
        return target;
    }

    private static PayrollPayslipSnapshot? ReadSnapshot(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<PayrollPayslipSnapshot>(json); }
        catch (JsonException) { return null; }
    }
}
