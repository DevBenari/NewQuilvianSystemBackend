using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Handoff pembayaran gaji ke Finance dan penerimaan hasil bank (PAY-BE-08).
///
/// Prinsip:
/// - Hanya run yang sudah DISAHKAN (terkunci, seluruh penerima dibekukan) yang boleh mengirim instruksi. Satu instruksi hidup
///   (Scheduled/Processing/Paid) per penerima; kirim ulang tidak pernah membuat instruksi kedua untuk penerima yang sama
///   (dijaga transaksi + advisory lock, dan indeks unik database pada PrlPayrollPayment).
/// - Rekening wajib terverifikasi. Kecuali kebijakan mengizinkan handoff sebagian, satu rekening bermasalah menahan SELURUH handoff.
/// - Status Paid hanya lahir dari aksi ACK (nomor instruksi + bukti referensi + nominal yang sama persis dengan instruksi), dan
///   aksi itu butuh izin Acknowledge yang diberikan admin lewat Akses Role: oleh Finance dengan hasil bank (sumber FINANCE_ACK)
///   atau, selama belum ada koneksi bank, oleh petugas Payroll berwenang lewat konfirmasi manual (sumber MANUAL, PAY-BE-11).
///   Gagal wajib beralasan; instruksi gagal dikirim ulang sebagai instruksi BARU.
/// - Payroll tidak menulis ke tabel Finance/Accounting. Sisi Finance (jenis pembayaran PAYROLL, mapping COA, jurnal) adalah NEEDS
///   CONFIRMATION dan belum ada di Finance; Payroll menyediakan kontrak instruksi (GET) dan penerima ACK (POST) saja.
/// </summary>
public sealed class PayrollPaymentService
{
    public const string FinanceConsumptionNote =
        "NEEDS CONFIRMATION: Finance belum memiliki jenis pembayaran PAYROLL dan mapping COA gaji belum disahkan. Instruksi tersedia " +
        "pada daftar ini; hasil bank diterima lewat ACK oleh pengguna Finance yang berwenang atau, selama belum ada koneksi bank, lewat " +
        "konfirmasi manual oleh petugas Payroll yang diberi izin Acknowledge (sumber MANUAL).";

    /// <summary>Catatan instruksi Paid hasil konfirmasi manual (PAY-BE-11), diisi hanya bila Notes masih kosong; tanpa kolom baru.</summary>
    private const string ManualPaidNote = "Dikonfirmasi manual (bukan hasil bank)";

    private const string LogCategory = "Corporate.PayrollManagement.Payment";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollPaymentService(ApplicationDbContext dbContext, LoggerService loggerService, IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _policy = policyOptions.Value;
    }

    // ------------------------------------------------------------------------------------
    // Handoff ke Finance
    // ------------------------------------------------------------------------------------

    public Task<PayrollFinanceHandoffResponse> HandoffAsync(
        Guid runId, PayrollFinanceHandoffRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(runId), async () =>
        {
            var run = await _dbContext.PrlPayrollRuns.SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

            var inPaymentStage = PayrollRunStatuses.IsPaymentStage(run.RunStatus);
            var isClosed = string.Equals(run.RunStatus, PayrollRunStatuses.Closed, StringComparison.OrdinalIgnoreCase);
            if (!inPaymentStage && !isClosed)
            {
                PayrollVersionToken.EnsureMatches(request.ExpectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
                if (!PayrollRunStatuses.AllowsFinanceHandoff(run.RunStatus) || !run.IsLocked)
                    throw new PayrollInvalidTransitionException(
                        $"Instruksi pembayaran hanya dapat dikirim dari payroll run yang sudah disahkan (saat ini {run.RunStatus}).");
            }

            var employees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
            if (employees.Any(e => !e.IsFinalized))
                throw new PayrollInvalidTransitionException("Masih ada penerima yang belum dibekukan pada run ini; instruksi pembayaran belum dapat dikirim.");

            var payments = await _dbContext.PrlPayrollPayments.Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var bankIds = employees.Where(e => e.BankAccountId.HasValue).Select(e => e.BankAccountId!.Value).Distinct().ToList();
            var banks = await _dbContext.WfpBankAccounts.AsNoTracking().Where(b => bankIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, cancellationToken);

            var created = new List<PrlPayrollPayment>();
            var skipped = new List<string>();
            var ordinal = 0;
            foreach (var employee in employees)
            {
                ordinal++;
                if (employee.PaymentAmount <= 0m) continue; // tidak ada yang dibayarkan (mis. seluruh gaji terpakai potongan)
                if (payments.Any(p => p.PayrollRunEmployeeId == employee.Id && PayrollPaymentStatuses.IsLive(p.PaymentStatus))) continue;

                PayeeBankSnapshot? bank = null;
                if (employee.BankAccountId.HasValue && banks.TryGetValue(employee.BankAccountId.Value, out var account)
                    && account.IsActive && account.IsPayrollAccount && account.IsVerified)
                    bank = new PayeeBankSnapshot(account.Id, account.BankName, account.AccountNumber, account.AccountHolderName);
                if (bank is null)
                {
                    skipped.Add($"{employee.EmployeeNameSnapshot}: belum memiliki rekening payroll terverifikasi.");
                    continue;
                }

                var attempt = payments.Count(p => p.PayrollRunEmployeeId == employee.Id) + created.Count(p => p.PayrollRunEmployeeId == employee.Id);
                created.Add(NewPayment(run, employee, bank, ordinal, attempt, actorUserId));
            }

            if (skipped.Count > 0 && !_policy.AllowPartialFinanceHandoff)
                throw new PayrollRunValidationException(
                    $"{skipped.Count} penerima belum memiliki rekening payroll terverifikasi sehingga instruksi pembayaran ditahan: "
                    + $"{string.Join("; ", skipped.Take(3))}{(skipped.Count > 3 ? "; ..." : string.Empty)}");

            // Idempoten: tidak ada instruksi baru yang perlu dibuat -> kembalikan keadaan yang ada tanpa mengubah apa pun.
            if (created.Count == 0)
            {
                if (inPaymentStage || isClosed || payments.Any(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)))
                    return BuildResponse(run, payments, created, skipped, replay: true);
                throw new PayrollRunValidationException(
                    "Tidak ada instruksi pembayaran yang dapat dikirim: tidak ada penerima dengan nominal bayar lebih dari nol.");
            }

            _dbContext.PrlPayrollPayments.AddRange(created);
            payments.AddRange(created);
            // Run baru pindah ke SentToFinance; run yang sudah di tahap pembayaran dihitung ulang statusnya (instruksi tambahan
            // yang belum dibayar mengembalikan Paid ke PartiallyPaid).
            if (!inPaymentStage) run.RunStatus = PayrollRunStatuses.SentToFinance;
            await RecomputeRunPaymentStateAsync(run, payments, actorUserId, cancellationToken);
            AddApproval(run, actorUserId, "FinanceHandoff", "Approved",
                $"{created.Count} instruksi, total Rp {created.Sum(p => p.PaymentAmount):N2}"
                + (string.IsNullOrWhiteSpace(request.IdempotencyKey) ? string.Empty : $" (kunci {request.IdempotencyKey.Trim()})"));
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "PayrollRun.FinanceHandoff",
                $"{created.Count} instruksi pembayaran payroll run {run.RunNumber} dikirim ke Finance (Rp {created.Sum(p => p.PaymentAmount):N2}); {skipped.Count} ditahan.",
                new { run.Id, Count = created.Count, Skipped = skipped.Count, request.IdempotencyKey, ActorUserId = actorUserId });
            return BuildResponse(run, payments, created, skipped, replay: false);
        }, cancellationToken);

    private sealed record PayeeBankSnapshot(Guid Id, string? BankName, string AccountNumber, string AccountHolderName);

    private static PrlPayrollPayment NewPayment(
        PrlPayrollRun run, PrlPayrollRunEmployee employee, PayeeBankSnapshot bank, int ordinal, int priorAttempts, Guid actorUserId)
    {
        var baseNumber = $"PAYI-{run.RunNumber}-{ordinal:D4}";
        return new PrlPayrollPayment
        {
            PayrollRunId = run.Id,
            PayrollRunEmployeeId = employee.Id,
            BankAccountId = bank.Id,
            PaymentNumber = priorAttempts == 0 ? baseNumber : $"{baseNumber}-R{priorAttempts + 1}",
            PaymentStatus = PayrollPaymentStatuses.Scheduled,
            CurrencyCode = string.IsNullOrWhiteSpace(employee.CurrencyCode) ? "IDR" : employee.CurrencyCode,
            PaymentAmount = employee.PaymentAmount,
            PayeeNameSnapshot = bank.AccountHolderName,
            BankNameSnapshot = bank.BankName,
            BankAccountNumberSnapshot = bank.AccountNumber,
            ScheduledPaymentAt = run.PaymentDateSnapshot ?? DateTime.UtcNow,
            PostedToFinanceAt = DateTime.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
    }

    /// <summary>Respons handoff: instruksi BARU bila ada, selain itu (replay) seluruh instruksi hidup yang sudah ada.</summary>
    private static PayrollFinanceHandoffResponse BuildResponse(
        PrlPayrollRun run, List<PrlPayrollPayment> payments, List<PrlPayrollPayment> created, List<string> skipped, bool replay)
    {
        var reported = created.Count > 0 ? created : payments.Where(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)).ToList();
        return new PayrollFinanceHandoffResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            RunStatus = run.RunStatus,
            RowVersion = PayrollVersionToken.From(run.CreateDateTime, run.UpdateDateTime),
            IsIdempotentReplay = replay,
            InstructionCount = reported.Count,
            TotalAmount = reported.Sum(p => p.PaymentAmount),
            SkippedCount = skipped.Count,
            SkippedReasons = skipped,
            FinanceConsumptionNote = FinanceConsumptionNote
        };
    }

    // ------------------------------------------------------------------------------------
    // Daftar instruksi dan hasil bank (hanya baca)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollPaymentListResponse> GetPaymentsAsync(Guid runId, PayrollPaymentQuery query, CancellationToken cancellationToken)
    {
        var run = await _dbContext.PrlPayrollRuns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var all = await _dbContext.PrlPayrollPayments.AsNoTracking()
            .Where(x => x.PayrollRunId == runId && !x.IsDelete)
            .Select(x => new { x.PayrollRunEmployeeId, x.PaymentStatus, x.PaymentAmount, x.NetPaidAmount })
            .ToListAsync(cancellationToken);
        var live = all.Where(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)).ToList();
        var summary = new PayrollRunPaymentSummaryResponse
        {
            InstructionCount = live.Count,
            ScheduledCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Scheduled || p.PaymentStatus == PayrollPaymentStatuses.Processing),
            PaidCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Paid || p.PaymentStatus == PayrollPaymentStatuses.PartiallyReversed),
            FailedCount = all.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Failed),
            TotalInstructionAmount = live.Sum(p => p.PaymentAmount),
            TotalPaidAmount = live.Sum(p => p.NetPaidAmount)
        };

        // PAY-BE-11: potongan piutang yang sudah dikonfirmasi saat run disahkan, tetapi gaji penerimanya belum lunas atau gagal.
        var deductionAttention = await PayrollSalaryPaymentStatusResolver.CountConfirmedDeductionAttentionAsync(_dbContext, runId, cancellationToken);
        summary.ConfirmedDeductionPaymentPendingCount = deductionAttention.Pending;
        summary.ConfirmedDeductionPaymentFailedCount = deductionAttention.Failed;

        var employeesWithLive = live.Select(p => p.PayrollRunEmployeeId).ToHashSet();

        var rows = _dbContext.PrlPayrollPayments.AsNoTracking().Where(x => x.PayrollRunId == runId && !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(query.Status)) rows = rows.Where(x => x.PaymentStatus == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            rows = rows.Where(x => EF.Functions.ILike(x.PaymentNumber, term)
                || EF.Functions.ILike(x.PayrollRunEmployee!.EmployeeNameSnapshot, term)
                || EF.Functions.ILike(x.PayrollRunEmployee!.EmployeeNumberSnapshot!, term));
        }

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows.Include(x => x.PayrollRunEmployee)
            .OrderBy(x => x.PayrollRunEmployee!.EmployeeNameSnapshot).ThenBy(x => x.PaymentNumber)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PayrollPaymentListResponse
        {
            PayrollRunId = run.Id,
            RunNumber = run.RunNumber,
            RunStatus = run.RunStatus,
            FinancePaymentBatchId = run.FinancePaymentBatchId,
            Summary = summary,
            Payments = new PagedResult<PayrollPaymentInstructionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page.Select(p => new PayrollPaymentInstructionResponse
                {
                    Id = p.Id,
                    PayrollRunId = p.PayrollRunId,
                    PayrollRunEmployeeId = p.PayrollRunEmployeeId,
                    PaymentNumber = p.PaymentNumber,
                    PaymentStatus = p.PaymentStatus,
                    EmployeeNumber = p.PayrollRunEmployee?.EmployeeNumberSnapshot ?? string.Empty,
                    EmployeeName = p.PayrollRunEmployee?.EmployeeNameSnapshot ?? string.Empty,
                    PayeeName = p.PayeeNameSnapshot,
                    BankName = p.BankNameSnapshot,
                    BankAccountMasked = MaskTail(p.BankAccountNumberSnapshot, 4),
                    PaymentAmount = p.PaymentAmount,
                    NetPaidAmount = p.NetPaidAmount,
                    ReversedAmount = p.ReversedAmount,
                    PaymentReferenceNumber = p.PaymentReferenceNumber,
                    FinancePaymentId = p.FinancePaymentId,
                    ScheduledPaymentAt = p.ScheduledPaymentAt,
                    PostedToFinanceAt = p.PostedToFinanceAt,
                    PaidAt = p.PaidAt,
                    Note = p.Notes,
                    CanRetry = p.PaymentStatus == PayrollPaymentStatuses.Failed
                        && PayrollRunStatuses.IsPaymentStage(run.RunStatus)
                        && !employeesWithLive.Contains(p.PayrollRunEmployeeId)
                }).ToList()
            }
        };
    }

    // ------------------------------------------------------------------------------------
    // ACK hasil pembayaran: hasil bank dari Finance (FINANCE_ACK) atau konfirmasi manual petugas berwenang (MANUAL)
    // ------------------------------------------------------------------------------------

    public Task<PayrollPaymentAcknowledgeResponse> AcknowledgeAsync(
        Guid runId, PayrollPaymentAcknowledgeRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(runId), async () =>
        {
            // PAY-BE-11: sumber divalidasi lebih dulu, sebelum data apa pun dibaca atau diubah. Kosong = FINANCE_ACK (perilaku lama).
            var source = PayrollConfirmationSources.Normalize(request.ConfirmationSource)
                ?? throw new PayrollRunValidationException(
                    $"Sumber konfirmasi tidak dikenal. Gunakan {PayrollConfirmationSources.FinanceAck} atau {PayrollConfirmationSources.Manual}.");
            var isManual = source == PayrollConfirmationSources.Manual;

            var run = await _dbContext.PrlPayrollRuns.SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
            if (!PayrollRunStatuses.IsPaymentStage(run.RunStatus))
                throw new PayrollInvalidTransitionException(
                    $"Hasil pembayaran hanya diterima untuk run yang instruksinya sudah dikirim ke Finance (saat ini {run.RunStatus}).");

            var numbers = request.Items.Select(i => (i.PaymentNumber ?? string.Empty).Trim()).ToList();
            if (numbers.Any(string.IsNullOrEmpty) || numbers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != numbers.Count)
                throw new PayrollRunValidationException("Setiap item wajib memiliki nomor instruksi dan tidak boleh ada nomor yang dobel dalam satu pengiriman.");

            var payments = await _dbContext.PrlPayrollPayments.Where(x => x.PayrollRunId == runId && !x.IsDelete).ToListAsync(cancellationToken);
            var byNumber = payments.ToDictionary(p => p.PaymentNumber, StringComparer.OrdinalIgnoreCase);

            var response = new PayrollPaymentAcknowledgeResponse { PayrollRunId = run.Id, ConfirmationSource = source };
            var now = DateTime.UtcNow;
            foreach (var item in request.Items)
            {
                var number = item.PaymentNumber.Trim();
                var result = new PayrollPaymentAcknowledgeItemResult { PaymentNumber = number };
                response.Items.Add(result);

                if (!byNumber.TryGetValue(number, out var payment))
                {
                    Reject(result, response, string.Empty, "Nomor instruksi tidak ditemukan pada payroll run ini.");
                    continue;
                }
                result.PaymentStatus = payment.PaymentStatus;

                if (string.Equals(item.Status, PayrollPaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(item.PaymentReferenceNumber))
                    { Reject(result, response, payment.PaymentStatus, "Referensi/bukti bank wajib diisi untuk status Paid."); continue; }
                    if ((item.PaidAmount ?? payment.PaymentAmount) != payment.PaymentAmount)
                    { Reject(result, response, payment.PaymentStatus, $"Nominal dibayar harus sama dengan nominal instruksi (Rp {payment.PaymentAmount:N2})."); continue; }

                    if (payment.PaymentStatus == PayrollPaymentStatuses.Paid)
                    {
                        if (string.Equals(payment.PaymentReferenceNumber, item.PaymentReferenceNumber.Trim(), StringComparison.Ordinal))
                        { result.Outcome = "Idempoten"; result.Message = "Instruksi sudah berstatus Paid dengan referensi yang sama."; response.IdempotentCount++; }
                        else Reject(result, response, payment.PaymentStatus, "Instruksi sudah berstatus Paid dengan referensi bank yang berbeda.");
                        continue;
                    }
                    if (payment.PaymentStatus != PayrollPaymentStatuses.Scheduled && payment.PaymentStatus != PayrollPaymentStatuses.Processing)
                    { Reject(result, response, payment.PaymentStatus, $"Instruksi berstatus {payment.PaymentStatus} dan tidak dapat ditandai Paid; kirim ulang instruksi baru bila perlu."); continue; }

                    payment.PaymentStatus = PayrollPaymentStatuses.Paid;
                    payment.NetPaidAmount = payment.PaymentAmount;
                    payment.PaymentReferenceNumber = item.PaymentReferenceNumber.Trim();
                    payment.FinancePaymentId = item.FinancePaymentId ?? payment.FinancePaymentId;
                    payment.PaidAt = item.PaidAt ?? now;
                    payment.PaidByUserId = actorUserId;
                    // Hanya konfirmasi manual yang menandai Notes; hasil bank Finance tidak berubah dari perilaku lama.
                    if (isManual && string.IsNullOrWhiteSpace(payment.Notes)) payment.Notes = ManualPaidNote;
                    Touch(payment, actorUserId, now);
                    result.PaymentStatus = payment.PaymentStatus;
                    result.Outcome = "Diterapkan";
                    result.Message = "Pembayaran terkonfirmasi.";
                    response.AppliedCount++;
                }
                else if (string.Equals(item.Status, PayrollPaymentStatuses.Failed, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(item.FailureReason))
                    { Reject(result, response, payment.PaymentStatus, "Alasan gagal wajib diisi untuk status Failed."); continue; }

                    if (payment.PaymentStatus == PayrollPaymentStatuses.Failed)
                    { result.Outcome = "Idempoten"; result.Message = "Instruksi sudah berstatus Failed."; response.IdempotentCount++; continue; }
                    if (payment.PaymentStatus != PayrollPaymentStatuses.Scheduled && payment.PaymentStatus != PayrollPaymentStatuses.Processing)
                    { Reject(result, response, payment.PaymentStatus, $"Instruksi berstatus {payment.PaymentStatus} dan tidak dapat ditandai Failed."); continue; }

                    payment.PaymentStatus = PayrollPaymentStatuses.Failed;
                    payment.Notes = item.FailureReason.Trim();
                    payment.PaymentReferenceNumber = item.PaymentReferenceNumber?.Trim() ?? payment.PaymentReferenceNumber;
                    Touch(payment, actorUserId, now);
                    result.PaymentStatus = payment.PaymentStatus;
                    result.Outcome = "Diterapkan";
                    result.Message = "Kegagalan pembayaran dicatat; kirim ulang instruksi baru setelah rekening diperbaiki.";
                    response.AppliedCount++;
                }
                else
                {
                    Reject(result, response, payment.PaymentStatus, "Status harus Paid atau Failed.");
                }
            }

            if (response.AppliedCount > 0)
            {
                if (request.FinancePaymentBatchId.HasValue) run.FinancePaymentBatchId = request.FinancePaymentBatchId;
                await RecomputeRunPaymentStateAsync(run, payments, actorUserId, cancellationToken);
                AddApproval(run, actorUserId, "PaymentAck", "Approved",
                    $"{response.AppliedCount} diterapkan, {response.IdempotentCount} idempoten, {response.RejectedCount} ditolak"
                    + (isManual ? $" (sumber {source})" : string.Empty)
                    + (string.IsNullOrWhiteSpace(request.IdempotencyKey) ? string.Empty : $" (kunci {request.IdempotencyKey.Trim()})"));
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _loggerService.AuditAsync(LogCategory, "PayrollRun.PaymentAcknowledged",
                    $"Hasil pembayaran payroll run {run.RunNumber}: {response.AppliedCount} diterapkan, {response.RejectedCount} ditolak"
                    + (isManual ? $" (sumber {source})." : "."),
                    new { run.Id, response.AppliedCount, response.IdempotentCount, response.RejectedCount, request.IdempotencyKey, ConfirmationSource = source, ActorUserId = actorUserId });
            }

            response.RunStatus = run.RunStatus;
            response.TotalPaidAmount = run.TotalPaidAmount;
            return response;
        }, cancellationToken);

    private static void Reject(PayrollPaymentAcknowledgeItemResult result, PayrollPaymentAcknowledgeResponse response, string status, string message)
    {
        result.Outcome = "Ditolak";
        result.PaymentStatus = status;
        result.Message = message;
        response.RejectedCount++;
    }

    private static void Touch(PrlPayrollPayment payment, Guid actorUserId, DateTime now)
    {
        payment.UpdateDateTime = now;
        payment.UpdateBy = actorUserId;
    }

    // ------------------------------------------------------------------------------------
    // Kirim ulang instruksi yang gagal
    // ------------------------------------------------------------------------------------

    public Task<PayrollFinanceHandoffResponse> RetryAsync(Guid runId, Guid paymentId, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(runId), async () =>
        {
            var run = await _dbContext.PrlPayrollRuns.SingleOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
            if (!PayrollRunStatuses.IsPaymentStage(run.RunStatus))
                throw new PayrollInvalidTransitionException("Instruksi hanya dapat dikirim ulang pada run yang sudah berada di tahap pembayaran.");

            var payments = await _dbContext.PrlPayrollPayments.Where(x => x.PayrollRunId == runId && !x.IsDelete).ToListAsync(cancellationToken);
            var failed = payments.SingleOrDefault(p => p.Id == paymentId)
                ?? throw new KeyNotFoundException("Instruksi pembayaran tidak ditemukan pada payroll run ini.");
            if (failed.PaymentStatus != PayrollPaymentStatuses.Failed)
                throw new PayrollInvalidTransitionException($"Hanya instruksi berstatus Failed yang dapat dikirim ulang (saat ini {failed.PaymentStatus}).");
            if (payments.Any(p => p.PayrollRunEmployeeId == failed.PayrollRunEmployeeId && PayrollPaymentStatuses.IsLive(p.PaymentStatus)))
                throw new PayrollRunConflictException("Penerima ini sudah memiliki instruksi pembayaran yang aktif.");

            var employee = await _dbContext.PrlPayrollRunEmployees.SingleOrDefaultAsync(
                x => x.Id == failed.PayrollRunEmployeeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Penerima pada instruksi ini tidak ditemukan.");
            var account = employee.BankAccountId.HasValue
                ? await _dbContext.WfpBankAccounts.AsNoTracking().SingleOrDefaultAsync(b => b.Id == employee.BankAccountId.Value, cancellationToken)
                : null;
            if (account is null || !account.IsActive || !account.IsPayrollAccount || !account.IsVerified)
                throw new PayrollRunValidationException("Rekening payroll penerima belum terverifikasi; perbaiki dan verifikasi rekening di HRD sebelum mengirim ulang.");

            // Nomor dasar diambil dari instruksi yang gagal (tanpa akhiran -Rn) agar urutan percobaan tetap terbaca.
            var failedNumber = failed.PaymentNumber;
            var marker = failedNumber.LastIndexOf("-R", StringComparison.Ordinal);
            var baseNumber = marker > 0 && int.TryParse(failedNumber[(marker + 2)..], out _) ? failedNumber[..marker] : failedNumber;
            var attempts = payments.Count(p => p.PayrollRunEmployeeId == employee.Id);
            var next = new PrlPayrollPayment
            {
                PayrollRunId = run.Id,
                PayrollRunEmployeeId = employee.Id,
                BankAccountId = account.Id,
                PaymentNumber = $"{baseNumber}-R{attempts + 1}",
                PaymentStatus = PayrollPaymentStatuses.Scheduled,
                CurrencyCode = failed.CurrencyCode,
                PaymentAmount = employee.PaymentAmount,
                PayeeNameSnapshot = account.AccountHolderName,
                BankNameSnapshot = account.BankName,
                BankAccountNumberSnapshot = account.AccountNumber,
                ScheduledPaymentAt = DateTime.UtcNow,
                PostedToFinanceAt = DateTime.UtcNow,
                Notes = $"Pengiriman ulang dari {failed.PaymentNumber}",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollPayments.Add(next);
            payments.Add(next);
            await RecomputeRunPaymentStateAsync(run, payments, actorUserId, cancellationToken);
            AddApproval(run, actorUserId, "PaymentRetry", "Approved", $"{failed.PaymentNumber} -> {next.PaymentNumber}");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.PaymentRetried",
                $"Instruksi {failed.PaymentNumber} dikirim ulang sebagai {next.PaymentNumber}.", new { run.Id, ActorUserId = actorUserId });
            return BuildResponse(run, payments, new List<PrlPayrollPayment> { next }, new List<string>(), replay: false);
        }, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    /// <summary>Status run mengikuti hasil bank: Paid bila SEMUA penerima bernominal sudah dibayar, PartiallyPaid bila sebagian, selain itu SentToFinance.</summary>
    private async Task RecomputeRunPaymentStateAsync(
        PrlPayrollRun run, List<PrlPayrollPayment> payments, Guid actorUserId, CancellationToken cancellationToken)
    {
        var payable = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete && x.PaymentAmount > 0m)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        bool IsSettled(PrlPayrollPayment p) =>
            p.PaymentStatus == PayrollPaymentStatuses.Paid || p.PaymentStatus == PayrollPaymentStatuses.PartiallyReversed;
        var paidEmployees = payments.Where(IsSettled).Select(p => p.PayrollRunEmployeeId).ToHashSet();
        var paidCount = payable.Count(id => paidEmployees.Contains(id));

        run.TotalPaidAmount = payments.Where(IsSettled).Sum(p => p.NetPaidAmount);
        if (PayrollRunStatuses.IsPaymentStage(run.RunStatus))
        {
            run.RunStatus = payable.Count > 0 && paidCount == payable.Count ? PayrollRunStatuses.Paid
                : paidCount > 0 ? PayrollRunStatuses.PartiallyPaid
                : PayrollRunStatuses.SentToFinance;
        }
        run.UpdateDateTime = DateTime.UtcNow;
        run.UpdateBy = actorUserId;
    }

    private void AddApproval(PrlPayrollRun run, Guid actorUserId, string actionType, string status, string? comments) =>
        _dbContext.PrlPayrollApprovals.Add(new PrlPayrollApproval
        {
            PayrollRunId = run.Id,
            ApprovalLevel = "Payment",
            ApprovalStatus = status,
            ActionType = actionType,
            ActionAt = DateTime.UtcNow,
            ActualActionByUserId = actorUserId,
            Comments = comments,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        });

    private static string? MaskTail(string? value, int visible)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length <= visible ? new string('*', text.Length) : new string('*', text.Length - visible) + text[^visible..];
    }
}
