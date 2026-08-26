using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingRefundService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const string InpatientServiceType = "RANAP";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingPaymentProviderAdapter _providerAdapter;
    private readonly LoggerService _loggerService;

    public BillingRefundService(
        ApplicationDbContext dbContext,
        IBillingPaymentProviderAdapter providerAdapter,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _providerAdapter = providerAdapter;
        _loggerService = loggerService;
    }

    public async Task<RefundResponse> CreateAsync(
        CreateRefundRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request, idempotencyKey, actorUserId);
        var payloadHash = ComputeCreatePayloadHash(request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_REFUND_CREDIT_{request.RefundableCreditId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilRefundCases
                .Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingRefundConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditCaseAsync("BillingRefund.Create", prior, actorUserId, true);
                return MapCase(prior, true);
            }

            if (await _dbContext.BilRefundCases.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingRefundConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            if (invoice.ServiceType == InpatientServiceType)
                throw new BillingRefundValidationException(
                    "Refund normal tidak berlaku untuk invoice rawat inap.");

            var credit = await _dbContext.BilRefundableCredits
                .SingleOrDefaultAsync(
                    x => x.Id == request.RefundableCreditId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Refundable credit tidak ditemukan.");
            if (credit.InvoiceId != invoice.Id)
                throw new BillingRefundValidationException(
                    "Refundable credit tidak terkait dengan invoice yang diajukan.");
            if (credit.Status != BillingRefundableCreditStatuses.Available || credit.AvailableAmount <= 0)
                throw new BillingRefundValidationException(
                    "Refundable credit tidak lagi tersedia untuk direfund.");
            if (request.RequestedAmount > credit.AvailableAmount)
                throw new BillingRefundValidationException(
                    "Nominal refund melebihi saldo dana yang dapat dikembalikan.");

            var activeExists = await _dbContext.BilRefundCases.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.RefundableCreditId == credit.Id
                    && x.Status != BillingRefundCaseStatuses.Rejected
                    && x.Status != BillingRefundCaseStatuses.Executed,
                    cancellationToken);
            if (activeExists)
                throw new BillingRefundConflictException(
                    "Refundable credit ini masih memiliki refund case aktif; lanjutkan case yang sudah ada.");

            var eligibleTenders = await LoadEligibleFundingTendersAsync(invoice.Id, cancellationToken);
            if (eligibleTenders.Count == 0)
                throw new BillingRefundValidationException(
                    "Tidak ada tender berhasil pada invoice ini yang dapat dijadikan dasar proporsi refund.");
            if (eligibleTenders.Sum(x => x.Amount) < request.RequestedAmount)
                throw new BillingRefundValidationException(
                    "Metode pembayaran asal tidak cukup mendukung refund untuk nominal ini; ajukan penggantian metode melalui Finance.");

            var now = DateTimeOffset.UtcNow;
            var refundCase = new BilRefundCase
            {
                InvoiceId = invoice.Id,
                RefundableCreditId = credit.Id,
                RequestedAmount = request.RequestedAmount,
                Status = BillingRefundCaseStatuses.Submitted,
                RequestedBy = actorUserId,
                Reason = request.Reason.Trim(),
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                SubmittedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            foreach (var line in BuildProportionalLines(eligibleTenders, request.RequestedAmount, actorUserId))
            {
                line.RefundCaseId = refundCase.Id;
                refundCase.Lines.Add(line);
            }

            _dbContext.BilRefundCases.Add(refundCase);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditCaseAsync("BillingRefund.Create", refundCase, actorUserId, false);
            return MapCase(refundCase, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Refund case tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
                exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<RefundResponse> GetByIdAsync(
        Guid refundCaseId,
        CancellationToken cancellationToken)
    {
        var refundCase = await _dbContext.BilRefundCases.AsNoTracking()
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == refundCaseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Refund case tidak ditemukan.");
        return MapCase(refundCase, false);
    }

    public async Task<IReadOnlyList<RefundResponse>> ListByInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var refundCases = await _dbContext.BilRefundCases.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);
        return refundCases.Select(x => MapCase(x, false)).ToList();
    }

    public async Task<IReadOnlyList<RefundableCreditResponse>> ListRefundableCreditsByInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderByDescending(x => x.RecognizedAt)
            .Select(x => new RefundableCreditResponse
            {
                Id = x.Id,
                InvoiceId = x.InvoiceId,
                SourceType = x.SourceType,
                SourceId = x.SourceId,
                OriginalAmount = x.OriginalAmount,
                AvailableAmount = x.AvailableAmount,
                Status = x.Status,
                RecognizedAt = x.RecognizedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RefundResponse> ApproveAsync(
        Guid refundCaseId,
        RefundApprovalRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateApprovalRequest(request, actorUserId);
        var isRetry = false;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_REFUND_CASE_{refundCaseId:N}", cancellationToken);
            }

            var refundCase = await _dbContext.BilRefundCases
                .SingleOrDefaultAsync(x => x.Id == refundCaseId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Refund case tidak ditemukan.");
            if (refundCase.RowVersion != request.ExpectedRowVersion)
                throw new BillingRefundConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            switch (refundCase.Status)
            {
                case BillingRefundCaseStatuses.Submitted:
                    if (refundCase.RequestedBy == actorUserId)
                        throw new BillingRefundForbiddenException(
                            "Pengaju refund tidak boleh menyetujui pengajuannya sendiri.");
                    refundCase.Status = BillingRefundCaseStatuses.Approved;
                    refundCase.ApprovedBy = actorUserId;
                    refundCase.ApprovedAt = DateTimeOffset.UtcNow;
                    break;
                case BillingRefundCaseStatuses.Approved:
                case BillingRefundCaseStatuses.PartiallyExecuted:
                    isRetry = true;
                    break;
                default:
                    throw new BillingRefundConflictException(
                        "Refund case tidak lagi berada pada status yang dapat diproses.");
            }
            refundCase.RowVersion = Guid.NewGuid();
            refundCase.UpdateDateTime = DateTime.UtcNow;
            refundCase.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditApprovalAsync(refundCase, actorUserId, isRetry);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Approval refund tidak dapat disimpan karena data telah berubah.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }

        var caseInfo = await _dbContext.BilRefundCases.AsNoTracking()
            .Where(x => x.Id == refundCaseId)
            .Select(x => new { x.RefundableCreditId })
            .SingleAsync(cancellationToken);
        var pendingLineIds = await _dbContext.BilRefundLines.AsNoTracking()
            .Where(x => x.RefundCaseId == refundCaseId && !x.IsDelete
                && x.Status == BillingRefundLineStatuses.Pending)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        foreach (var lineId in pendingLineIds)
            await ExecuteLineAsync(lineId, caseInfo.RefundableCreditId, actorUserId, cancellationToken);

        return await FinalizeCaseStatusAsync(refundCaseId, actorUserId, cancellationToken);
    }

    private async Task<List<BilTender>> LoadEligibleFundingTendersAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var fundingTenders = await _dbContext.BilTenders.AsNoTracking()
            .Include(x => x.Settlement)
            .Where(x => !x.IsDelete && x.Status == BillingTenderStatuses.Succeeded
                && x.Settlement.Purpose == BillingSettlementPurposes.InvoicePayment
                && x.Settlement.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);
        if (fundingTenders.Count == 0) return fundingTenders;

        var paymentMethodIds = fundingTenders.Select(x => x.PaymentMethodId).Distinct().ToList();
        var refundEligibility = await _dbContext.MstPaymentMethods.AsNoTracking()
            .Where(x => paymentMethodIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.IsAvailableForRefund, cancellationToken);
        return fundingTenders
            .Where(x => refundEligibility.TryGetValue(x.PaymentMethodId, out var isEligible) && isEligible)
            .OrderByDescending(x => x.Amount).ThenBy(x => x.Id)
            .ToList();
    }

    private async Task ExecuteLineAsync(
        Guid lineId,
        Guid creditId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var line = await _dbContext.BilRefundLines.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == lineId && !x.IsDelete, cancellationToken);
        if (line is null || line.Status != BillingRefundLineStatuses.Pending) return;

        var paymentMethod = await LoadPaymentMethodAsync(line.PaymentMethodId, cancellationToken);
        BillingPaymentProviderResult result;
        if (paymentMethod.IsCash)
        {
            result = new BillingPaymentProviderResult(
                $"cash-refund:{line.Id:N}",
                BillingPaymentProviderOutcome.Succeeded,
                null,
                "CASH_REFUNDED",
                DateTimeOffset.UtcNow,
                line.CorrelationId,
                line.CausationId);
        }
        else
        {
            try
            {
                result = await _providerAdapter.SubmitAsync(
                    new BillingPaymentProviderRequest(
                        line.Id,
                        paymentMethod.PaymentMethodCode,
                        paymentMethod.IntegrationCode,
                        line.Amount,
                        line.IdempotencyKey,
                        line.CorrelationId,
                        line.CausationId),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Hasil provider belum dapat dipastikan; baris tetap PENDING untuk dicoba ulang.
                return;
            }
        }

        try
        {
            await PersistLineResultAsync(lineId, creditId, result, actorUserId, cancellationToken);
        }
        catch (BillingRefundException)
        {
            // Baris tetap PENDING dan dapat dicoba ulang pada approve berikutnya;
            // satu baris gagal tidak boleh membatalkan baris lain yang sudah berhasil.
        }
    }

    private async Task PersistLineResultAsync(
        Guid lineId,
        Guid creditId,
        BillingPaymentProviderResult result,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_REFUND_LINE_{lineId:N}", cancellationToken);
                await AcquireLockAsync($"BIL_REFUND_CREDIT_{creditId:N}", cancellationToken);
            }

            var line = await _dbContext.BilRefundLines.SingleOrDefaultAsync(
                x => x.Id == lineId && !x.IsDelete, cancellationToken);
            if (line is null || line.Status != BillingRefundLineStatuses.Pending)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return;
            }

            var beforeStatus = line.Status;
            line.Status = result.Outcome switch
            {
                BillingPaymentProviderOutcome.Succeeded => BillingRefundLineStatuses.Succeeded,
                BillingPaymentProviderOutcome.Failed => BillingRefundLineStatuses.Failed,
                BillingPaymentProviderOutcome.Expired => BillingRefundLineStatuses.Failed,
                _ => BillingRefundLineStatuses.Pending
            };
            line.ProviderReference ??= result.ProviderReference;
            line.ProviderStatusCode = result.ProviderStatusCode;
            line.AttemptedAt ??= DateTimeOffset.UtcNow;
            line.SettledAt = line.Status is BillingRefundLineStatuses.Succeeded or BillingRefundLineStatuses.Failed
                ? result.OccurredAt
                : null;
            line.UpdateDateTime = DateTime.UtcNow;
            line.UpdateBy = actorUserId;

            if (line.Status == BillingRefundLineStatuses.Succeeded)
            {
                var credit = await _dbContext.BilRefundableCredits.SingleAsync(
                    x => x.Id == creditId, cancellationToken);
                if (line.Amount > credit.AvailableAmount)
                    throw new BillingRefundConflictException(
                        "Saldo refundable credit tidak lagi mencukupi untuk baris refund ini.");
                credit.AvailableAmount -= line.Amount;
                credit.Status = credit.AvailableAmount == 0
                    ? BillingRefundableCreditStatuses.Exhausted
                    : BillingRefundableCreditStatuses.Available;
                credit.UpdateDateTime = DateTime.UtcNow;
                credit.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditLineResultAsync(line, beforeStatus, actorUserId);
        }
        catch (BillingRefundException)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingRefundConflictException(
                "Hasil eksekusi refund tidak dapat disimpan.", exception);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<RefundResponse> FinalizeCaseStatusAsync(
        Guid refundCaseId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_REFUND_CASE_{refundCaseId:N}", cancellationToken);
            }

            var refundCase = await _dbContext.BilRefundCases
                .Include(x => x.Lines)
                .SingleAsync(x => x.Id == refundCaseId, cancellationToken);
            var beforeStatus = refundCase.Status;
            if (refundCase.Status is BillingRefundCaseStatuses.Approved or BillingRefundCaseStatuses.PartiallyExecuted)
            {
                var lines = refundCase.Lines.Where(x => !x.IsDelete).ToList();
                var succeededCount = lines.Count(x => x.Status == BillingRefundLineStatuses.Succeeded);
                refundCase.Status = succeededCount == lines.Count
                    ? BillingRefundCaseStatuses.Executed
                    : succeededCount > 0
                        ? BillingRefundCaseStatuses.PartiallyExecuted
                        : BillingRefundCaseStatuses.Approved;
                refundCase.CompletedAt = refundCase.Status == BillingRefundCaseStatuses.Executed
                    ? DateTimeOffset.UtcNow
                    : null;
                if (refundCase.Status != beforeStatus)
                {
                    refundCase.RowVersion = Guid.NewGuid();
                    refundCase.UpdateDateTime = DateTime.UtcNow;
                    refundCase.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            if (refundCase.Status != beforeStatus)
                await AuditExecutionAsync(refundCase, beforeStatus, actorUserId);
            return MapCase(refundCase, false);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private static List<BilRefundLine> BuildProportionalLines(
        IReadOnlyList<BilTender> eligibleTendersDescending,
        decimal requestedAmount,
        Guid actorUserId)
    {
        var total = eligibleTendersDescending.Sum(x => x.Amount);
        var lines = new List<BilRefundLine>(eligibleTendersDescending.Count);
        var runningTotal = 0m;
        for (var i = 1; i < eligibleTendersDescending.Count; i++)
        {
            var share = Money(requestedAmount * eligibleTendersDescending[i].Amount / total);
            runningTotal += share;
            lines.Add(NewLine(eligibleTendersDescending[i], share, actorUserId));
        }
        var firstShare = Money(requestedAmount - runningTotal);
        lines.Insert(0, NewLine(eligibleTendersDescending[0], firstShare, actorUserId));
        return lines;
    }

    private static BilRefundLine NewLine(BilTender tender, decimal amount, Guid actorUserId) => new()
    {
        OriginalTenderId = tender.Id,
        PaymentMethodId = tender.PaymentMethodId,
        Amount = amount,
        Status = BillingRefundLineStatuses.Pending,
        IdempotencyKey = Guid.NewGuid(),
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid(),
        CreateDateTime = DateTime.UtcNow,
        CreateBy = actorUserId
    };

    private async Task<MstPaymentMethod> LoadPaymentMethodAsync(
        Guid paymentMethodId,
        CancellationToken cancellationToken) =>
        await _dbContext.MstPaymentMethods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == paymentMethodId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");

    private static void ValidateCreateRequest(
        CreateRefundRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.InvoiceId == Guid.Empty || request.RefundableCreditId == Guid.Empty)
            throw new BillingRefundValidationException("InvoiceId dan RefundableCreditId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingRefundValidationException("Idempotency-Key wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingRefundForbiddenException("Identitas pengguna tidak valid.");
        ValidateMoney(request.RequestedAmount, "Nominal refund");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingRefundValidationException(
                "Alasan refund wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingRefundValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateApprovalRequest(RefundApprovalRequest request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingRefundValidationException("ExpectedRowVersion wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingRefundForbiddenException("Identitas pengguna tidak valid.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingRefundValidationException(
                "Alasan approval wajib diisi dan maksimal 500 karakter.");
    }

    private static void ValidateMoney(decimal amount, string fieldName)
    {
        if (amount <= 0 || amount > MaxMoneyAmount || decimal.Round(amount, 2) != amount)
            throw new BillingRefundValidationException(
                $"{fieldName} harus positif dan maksimal memiliki dua angka desimal.");
    }

    private static string ComputeCreatePayloadHash(CreateRefundRequest request)
    {
        var canonical = string.Join('|',
            request.InvoiceId.ToString("N"),
            request.RefundableCreditId.ToString("N"),
            request.RequestedAmount.ToString(CultureInfo.InvariantCulture),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditCaseAsync(
        string action,
        BilRefundCase refundCase,
        Guid actorUserId,
        bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            action,
            "Refund case diajukan dengan alokasi proporsional ke tender asal.",
            new
            {
                RefundCaseId = refundCase.Id,
                refundCase.InvoiceId,
                refundCase.RefundableCreditId,
                refundCase.RequestedAmount,
                refundCase.Status,
                Lines = refundCase.Lines.Select(x => new { x.OriginalTenderId, x.Amount }),
                refundCase.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private Task AuditApprovalAsync(BilRefundCase refundCase, Guid actorUserId, bool isRetry) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingRefund.Approve",
            "Refund case disetujui oleh approver berbeda dari pengaju.",
            new
            {
                RefundCaseId = refundCase.Id,
                refundCase.RequestedBy,
                refundCase.ApprovedBy,
                refundCase.Status,
                IsRetry = isRetry,
                ActorUserId = actorUserId
            });

    private Task AuditLineResultAsync(BilRefundLine line, string beforeStatus, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingRefund.LineExecuted",
            "Eksekusi baris refund direkonsiliasi tanpa menyimpan payload provider.",
            new
            {
                RefundLineId = line.Id,
                line.RefundCaseId,
                line.OriginalTenderId,
                line.Amount,
                StatusBefore = beforeStatus,
                StatusAfter = line.Status,
                line.ProviderStatusCode,
                ActorUserId = actorUserId
            });

    private Task AuditExecutionAsync(BilRefundCase refundCase, string beforeStatus, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingRefund.Finalized",
            $"RefundCase={refundCase.Id:N}; status {beforeStatus}->{refundCase.Status}.",
            new
            {
                RefundCaseId = refundCase.Id,
                StatusBefore = beforeStatus,
                StatusAfter = refundCase.Status,
                ExecutedAmount = refundCase.Lines
                    .Where(x => x.Status == BillingRefundLineStatuses.Succeeded)
                    .Sum(x => x.Amount),
                ActorUserId = actorUserId
            });

    private static RefundResponse MapCase(BilRefundCase refundCase, bool isReplay) => new()
    {
        Id = refundCase.Id,
        InvoiceId = refundCase.InvoiceId,
        RefundableCreditId = refundCase.RefundableCreditId,
        RequestedAmount = refundCase.RequestedAmount,
        ExecutedAmount = refundCase.Lines
            .Where(x => !x.IsDelete && x.Status == BillingRefundLineStatuses.Succeeded)
            .Sum(x => x.Amount),
        Status = refundCase.Status,
        RequestedBy = refundCase.RequestedBy,
        ApprovedBy = refundCase.ApprovedBy,
        Reason = refundCase.Reason,
        AdjustmentId = refundCase.AdjustmentId,
        CorrelationId = refundCase.CorrelationId,
        RowVersion = refundCase.RowVersion,
        SubmittedAt = refundCase.SubmittedAt,
        ApprovedAt = refundCase.ApprovedAt,
        CompletedAt = refundCase.CompletedAt,
        IsReplay = isReplay,
        Lines = refundCase.Lines.Where(x => !x.IsDelete)
            .OrderByDescending(x => x.Amount).ThenBy(x => x.Id)
            .Select(MapLine).ToList()
    };

    private static RefundLineResponse MapLine(BilRefundLine line) => new()
    {
        Id = line.Id,
        OriginalTenderId = line.OriginalTenderId,
        PaymentMethodId = line.PaymentMethodId,
        Amount = line.Amount,
        Status = line.Status,
        ProviderReferenceMasked = MaskProviderReference(line.ProviderReference),
        ProviderStatusCode = line.ProviderStatusCode,
        AttemptedAt = line.AttemptedAt,
        SettledAt = line.SettledAt
    };

    private static string? MaskProviderReference(string? providerReference)
    {
        if (string.IsNullOrWhiteSpace(providerReference)) return null;
        return providerReference.Length <= 4
            ? "****"
            : $"****{providerReference[^4..]}";
    }
}

public abstract class BillingRefundException : Exception
{
    protected BillingRefundException(string message) : base(message) { }
    protected BillingRefundException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingRefundValidationException(string message) : BillingRefundException(message);
public sealed class BillingRefundForbiddenException(string message) : BillingRefundException(message);

public sealed class BillingRefundConflictException : BillingRefundException
{
    public BillingRefundConflictException(string message) : base(message) { }
    public BillingRefundConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
