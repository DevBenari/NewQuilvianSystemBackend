using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingFinancialExceptionService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly LoggerService _loggerService;

    public BillingFinancialExceptionService(
        ApplicationDbContext dbContext,
        BillingArApHandoffService arApHandoffService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _arApHandoffService = arApHandoffService;
        _loggerService = loggerService;
    }

    public async Task<AdjustmentResponse> CreateAdjustmentAsync(
        CreateAdjustmentRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAdjustmentRequest(request, idempotencyKey, actorUserId);
        var payloadHash = ComputeAdjustmentPayloadHash(request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_LEDGER_{request.InvoiceId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilAdjustments
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingFinancialExceptionConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditAdjustmentAsync("BillingAdjustment.Create", prior, actorUserId, true);
                return MapAdjustment(prior, true);
            }

            if (await _dbContext.BilAdjustments.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingFinancialExceptionConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == request.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            EnsureLedgerMutableInvoice(invoice);
            if (invoice.RowVersion != request.ExpectedInvoiceRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            var now = DateTimeOffset.UtcNow;
            var adjustment = new BilAdjustment
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                Direction = request.Direction,
                Amount = request.Amount,
                Status = BillingAdjustmentStatuses.Submitted,
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
            _dbContext.BilAdjustments.Add(adjustment);
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditAdjustmentAsync("BillingAdjustment.Create", adjustment, actorUserId, false);
            return MapAdjustment(adjustment, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Adjustment tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
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

    public async Task<AdjustmentResponse> ApproveAdjustmentAsync(
        Guid adjustmentId,
        AdjustmentApprovalRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateApprovalRequest(request, actorUserId);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_ADJUSTMENT_{adjustmentId:N}", cancellationToken);
            }

            var adjustment = await _dbContext.BilAdjustments
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.Id == adjustmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Adjustment tidak ditemukan.");
            if (adjustment.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            if (adjustment.Status != BillingAdjustmentStatuses.Submitted)
                throw new BillingFinancialExceptionConflictException(
                    "Adjustment tidak lagi menunggu approval.");
            if (adjustment.RequestedBy == actorUserId)
                throw new BillingFinancialExceptionForbiddenException(
                    "Pengaju adjustment tidak boleh menyetujui pengajuannya sendiri.");

            var beforeStatus = adjustment.Status;
            adjustment.Status = BillingAdjustmentStatuses.Posted;
            adjustment.ApprovedBy = actorUserId;
            adjustment.PostedAt = DateTimeOffset.UtcNow;
            adjustment.RowVersion = Guid.NewGuid();
            adjustment.UpdateDateTime = DateTime.UtcNow;
            adjustment.UpdateBy = actorUserId;
            adjustment.Invoice.RowVersion = Guid.NewGuid();
            adjustment.Invoice.UpdateDateTime = DateTime.UtcNow;
            adjustment.Invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditAdjustmentApprovalAsync(adjustment, beforeStatus, actorUserId);
            await _arApHandoffService.RecordCorrectionIfLinkedAsync(
                adjustment.InvoiceId, adjustment.Id, null, adjustment.Direction, adjustment.Amount,
                adjustment.Reason, actorUserId, cancellationToken);
            return MapAdjustment(adjustment, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Approval adjustment tidak dapat disimpan.", exception);
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

    public async Task<WriteOffResponse> CreateWriteOffAsync(
        CreateWriteOffRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateCreateWriteOffRequest(request, idempotencyKey, actorUserId);
        var payloadHash = ComputeWriteOffPayloadHash(request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_LEDGER_{request.InvoiceId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilWriteOffCases
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingFinancialExceptionConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditWriteOffAsync("BillingWriteOff.Create", prior, actorUserId, true, null, null);
                return MapWriteOff(prior, null, null, true);
            }

            if (await _dbContext.BilWriteOffCases.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingFinancialExceptionConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == request.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            EnsureLedgerMutableInvoice(invoice);
            if (invoice.RowVersion != request.ExpectedInvoiceRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            var outstanding = await CalculateOutstandingAsync(invoice, cancellationToken);
            if (request.Amount > outstanding)
                throw new BillingFinancialExceptionValidationException(
                    "Nominal write-off melebihi saldo outstanding invoice saat ini.");

            var now = DateTimeOffset.UtcNow;
            var writeOffCase = new BilWriteOffCase
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                Amount = request.Amount,
                Status = BillingWriteOffCaseStatuses.Submitted,
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
            _dbContext.BilWriteOffCases.Add(writeOffCase);
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditWriteOffAsync(
                "BillingWriteOff.Create", writeOffCase, actorUserId, false, outstanding, outstanding);
            return MapWriteOff(writeOffCase, outstanding, outstanding, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Write-off tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
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

    public async Task<WriteOffResponse> ApproveWriteOffAsync(
        Guid writeOffCaseId,
        WriteOffApprovalRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateApprovalRequest(request, actorUserId);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_WRITEOFF_{writeOffCaseId:N}", cancellationToken);
            }

            var writeOffCase = await _dbContext.BilWriteOffCases
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.Id == writeOffCaseId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Write-off case tidak ditemukan.");
            if (writeOffCase.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            if (writeOffCase.Status != BillingWriteOffCaseStatuses.Submitted)
                throw new BillingFinancialExceptionConflictException(
                    "Write-off case tidak lagi menunggu approval.");
            if (writeOffCase.RequestedBy == actorUserId)
                throw new BillingFinancialExceptionForbiddenException(
                    "Pengaju write-off tidak boleh menyetujui pengajuannya sendiri.");

            var outstandingBefore = await CalculateOutstandingAsync(writeOffCase.Invoice, cancellationToken);
            if (writeOffCase.Amount > outstandingBefore)
                throw new BillingFinancialExceptionValidationException(
                    "Saldo outstanding telah berubah dan tidak lagi mencukupi nominal write-off ini; ajukan ulang.");

            var beforeStatus = writeOffCase.Status;
            var outstandingAfter = Math.Max(outstandingBefore - writeOffCase.Amount, 0);
            writeOffCase.IsFullSettlement = outstandingAfter == 0;
            writeOffCase.Status = BillingWriteOffCaseStatuses.Posted;
            writeOffCase.ApprovedBy = actorUserId;
            writeOffCase.PostedAt = DateTimeOffset.UtcNow;
            writeOffCase.RowVersion = Guid.NewGuid();
            writeOffCase.UpdateDateTime = DateTime.UtcNow;
            writeOffCase.UpdateBy = actorUserId;
            if (writeOffCase.IsFullSettlement)
                writeOffCase.Invoice.Status = BillingInvoiceStatuses.SettledByWriteOff;
            writeOffCase.Invoice.RowVersion = Guid.NewGuid();
            writeOffCase.Invoice.UpdateDateTime = DateTime.UtcNow;
            writeOffCase.Invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditWriteOffApprovalAsync(
                writeOffCase, beforeStatus, outstandingBefore, outstandingAfter, actorUserId);
            await _arApHandoffService.RecordCorrectionIfLinkedAsync(
                writeOffCase.InvoiceId, null, writeOffCase.Id, BillingAdjustmentDirections.Credit,
                writeOffCase.Amount, writeOffCase.Reason, actorUserId, cancellationToken);
            return MapWriteOff(writeOffCase, outstandingBefore, outstandingAfter, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Approval write-off tidak dapat disimpan.", exception);
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

    public Task<AdjustmentResponse> ReverseAsync(
        string type,
        Guid id,
        ReverseExceptionRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Reason))
            throw new BillingFinancialExceptionValidationException("Alasan reversal wajib diisi.");
        return type.Trim().ToLowerInvariant() switch
        {
            "adjustments" => ReverseAdjustmentAsync(id, request!, actorUserId, cancellationToken),
            "write-offs" => ReverseWriteOffAsync(id, request!, actorUserId, cancellationToken),
            _ => throw new BillingFinancialExceptionValidationException(
                "Tipe financial exception tidak didukung untuk reversal.")
        };
    }

    private async Task<AdjustmentResponse> ReverseAdjustmentAsync(
        Guid adjustmentId,
        ReverseExceptionRequest request,
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
                await AcquireLockAsync($"BIL_ADJUSTMENT_{adjustmentId:N}", cancellationToken);
            }

            var existingReversal = await _dbContext.BilAdjustments.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.ReversesAdjustmentId == adjustmentId && !x.IsDelete, cancellationToken);
            if (existingReversal is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapAdjustment(existingReversal, true);
            }

            var original = await _dbContext.BilAdjustments
                .SingleOrDefaultAsync(x => x.Id == adjustmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Adjustment tidak ditemukan.");
            if (original.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            if (original.Status != BillingAdjustmentStatuses.Posted)
                throw new BillingFinancialExceptionValidationException(
                    "Hanya adjustment yang sudah POSTED dapat direversal.");

            var invoice = await _dbContext.BilInvoices
                .SingleAsync(x => x.Id == original.InvoiceId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var reversal = new BilAdjustment
            {
                InvoiceId = original.InvoiceId,
                Invoice = invoice,
                Direction = original.Direction == BillingAdjustmentDirections.Credit
                    ? BillingAdjustmentDirections.Debit
                    : BillingAdjustmentDirections.Credit,
                Amount = original.Amount,
                Status = BillingAdjustmentStatuses.Posted,
                RequestedBy = actorUserId,
                ApprovedBy = actorUserId,
                Reason = request.Reason.Trim(),
                ReversesAdjustmentId = original.Id,
                IdempotencyKey = Guid.NewGuid(),
                PayloadHash = ComputeReversalPayloadHash(original.Id, request.Reason),
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid(),
                SubmittedAt = now,
                PostedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilAdjustments.Add(reversal);
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditReversalAsync("BillingAdjustment.Reverse", reversal, original.Id, actorUserId);
            await _arApHandoffService.RecordCorrectionIfLinkedAsync(
                reversal.InvoiceId, reversal.Id, null, reversal.Direction, reversal.Amount,
                reversal.Reason, actorUserId, cancellationToken);
            return MapAdjustment(reversal, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Reversal adjustment tidak dapat disimpan; entry ini mungkin sudah direversal.", exception);
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

    private async Task<AdjustmentResponse> ReverseWriteOffAsync(
        Guid writeOffCaseId,
        ReverseExceptionRequest request,
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
                await AcquireLockAsync($"BIL_WRITEOFF_{writeOffCaseId:N}", cancellationToken);
            }

            var existingReversal = await _dbContext.BilAdjustments.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.ReversesWriteOffCaseId == writeOffCaseId && !x.IsDelete, cancellationToken);
            if (existingReversal is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapAdjustment(existingReversal, true);
            }

            var original = await _dbContext.BilWriteOffCases
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.Id == writeOffCaseId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Write-off case tidak ditemukan.");
            if (original.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinancialExceptionConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");
            if (original.Status != BillingWriteOffCaseStatuses.Posted)
                throw new BillingFinancialExceptionValidationException(
                    "Hanya write-off case yang sudah POSTED dapat direversal.");

            var now = DateTimeOffset.UtcNow;
            var reversal = new BilAdjustment
            {
                InvoiceId = original.InvoiceId,
                Invoice = original.Invoice,
                Direction = BillingAdjustmentDirections.Debit,
                Amount = original.Amount,
                Status = BillingAdjustmentStatuses.Posted,
                RequestedBy = actorUserId,
                ApprovedBy = actorUserId,
                Reason = request.Reason.Trim(),
                ReversesWriteOffCaseId = original.Id,
                IdempotencyKey = Guid.NewGuid(),
                PayloadHash = ComputeReversalPayloadHash(original.Id, request.Reason),
                CorrelationId = Guid.NewGuid(),
                CausationId = Guid.NewGuid(),
                SubmittedAt = now,
                PostedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilAdjustments.Add(reversal);
            if (original.IsFullSettlement
                && original.Invoice.Status == BillingInvoiceStatuses.SettledByWriteOff)
                original.Invoice.Status = BillingInvoiceStatuses.Open;
            original.Invoice.RowVersion = Guid.NewGuid();
            original.Invoice.UpdateDateTime = DateTime.UtcNow;
            original.Invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditReversalAsync("BillingWriteOff.Reverse", reversal, original.Id, actorUserId);
            await _arApHandoffService.RecordCorrectionIfLinkedAsync(
                reversal.InvoiceId, reversal.Id, null, reversal.Direction, reversal.Amount,
                reversal.Reason, actorUserId, cancellationToken);
            return MapAdjustment(reversal, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinancialExceptionConflictException(
                "Reversal write-off tidak dapat disimpan; case ini mungkin sudah direversal.", exception);
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

    private async Task<decimal> CalculateOutstandingAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken)
    {
        var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.InvoiceId == invoice.Id
                    && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete,
                cancellationToken)
            ?? throw new BillingFinancialExceptionValidationException(
                "Invoice belum memiliki hasil perhitungan terkini.");
        var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoice.Id && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount),
                cancellationToken) ?? 0;
        var allocationExcess = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess
                && !x.IsDelete)
            .SumAsync(x => (decimal?)x.AvailableAmount, cancellationToken) ?? 0;
        var writeOffTotal = await _dbContext.BilWriteOffCases.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingWriteOffCaseStatuses.Posted && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        var adjustmentNet = await _dbContext.BilAdjustments.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingAdjustmentStatuses.Posted && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.Direction == BillingAdjustmentDirections.Credit ? x.Amount : -x.Amount),
                cancellationToken) ?? 0;
        return Math.Max(
            calculation.PatientAmount - paidAmount + allocationExcess - writeOffTotal - adjustmentNet, 0);
    }

    private static void EnsureLedgerMutableInvoice(BilInvoice invoice)
    {
        if (invoice.Status is BillingInvoiceStatuses.Closed or BillingInvoiceStatuses.SettledByWriteOff)
            throw new BillingFinancialExceptionValidationException(
                "Invoice sudah closed atau settled by write-off dan tidak dapat menerima adjustment/write-off baru.");
    }

    private static void ValidateCreateAdjustmentRequest(
        CreateAdjustmentRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.InvoiceId == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("InvoiceId wajib diisi.");
        if (request.Direction is not (BillingAdjustmentDirections.Debit or BillingAdjustmentDirections.Credit))
            throw new BillingFinancialExceptionValidationException("Direction harus DEBIT atau CREDIT.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("Idempotency-Key wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingFinancialExceptionForbiddenException("Identitas pengguna tidak valid.");
        if (request.ExpectedInvoiceRowVersion == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("ExpectedInvoiceRowVersion wajib diisi.");
        ValidateMoney(request.Amount, "Nominal adjustment");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingFinancialExceptionValidationException(
                "Alasan adjustment wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateCreateWriteOffRequest(
        CreateWriteOffRequest request,
        Guid idempotencyKey,
        Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.InvoiceId == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("InvoiceId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("Idempotency-Key wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingFinancialExceptionForbiddenException("Identitas pengguna tidak valid.");
        if (request.ExpectedInvoiceRowVersion == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("ExpectedInvoiceRowVersion wajib diisi.");
        ValidateMoney(request.Amount, "Nominal write-off");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingFinancialExceptionValidationException(
                "Alasan write-off wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateApprovalRequest(object request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (expectedRowVersion, reason) = request switch
        {
            AdjustmentApprovalRequest adjustmentRequest =>
                (adjustmentRequest.ExpectedRowVersion, adjustmentRequest.Reason),
            WriteOffApprovalRequest writeOffRequest =>
                (writeOffRequest.ExpectedRowVersion, writeOffRequest.Reason),
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };
        if (expectedRowVersion == Guid.Empty)
            throw new BillingFinancialExceptionValidationException("ExpectedRowVersion wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingFinancialExceptionForbiddenException("Identitas pengguna tidak valid.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            throw new BillingFinancialExceptionValidationException(
                "Alasan approval wajib diisi dan maksimal 500 karakter.");
    }

    private static void ValidateMoney(decimal amount, string fieldName)
    {
        if (amount <= 0 || amount > MaxMoneyAmount || decimal.Round(amount, 2) != amount)
            throw new BillingFinancialExceptionValidationException(
                $"{fieldName} harus positif dan maksimal memiliki dua angka desimal.");
    }

    private static string ComputeAdjustmentPayloadHash(CreateAdjustmentRequest request)
    {
        var canonical = string.Join('|',
            request.InvoiceId.ToString("N"),
            request.Direction,
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Hash(canonical);
    }

    private static string ComputeWriteOffPayloadHash(CreateWriteOffRequest request)
    {
        var canonical = string.Join('|',
            request.InvoiceId.ToString("N"),
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Hash(canonical);
    }

    private static string ComputeReversalPayloadHash(Guid originalId, string reason) =>
        Hash(string.Join('|', originalId.ToString("N"), reason.Trim()));

    private static string Hash(string canonical) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditAdjustmentAsync(
        string action, BilAdjustment adjustment, Guid actorUserId, bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            action,
            "Adjustment diajukan sebagai koreksi append-only terhadap invoice.",
            new
            {
                AdjustmentId = adjustment.Id,
                adjustment.InvoiceId,
                adjustment.Direction,
                adjustment.Amount,
                adjustment.Status,
                adjustment.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private Task AuditAdjustmentApprovalAsync(
        BilAdjustment adjustment, string beforeStatus, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingAdjustment.Approve",
            $"Adjustment={adjustment.Id:N}; status {beforeStatus}->{adjustment.Status}.",
            new
            {
                AdjustmentId = adjustment.Id,
                adjustment.RequestedBy,
                adjustment.ApprovedBy,
                adjustment.Direction,
                adjustment.Amount,
                StatusBefore = beforeStatus,
                StatusAfter = adjustment.Status,
                ActorUserId = actorUserId
            });

    private Task AuditWriteOffAsync(
        string action,
        BilWriteOffCase writeOffCase,
        Guid actorUserId,
        bool isReplay,
        decimal? outstandingBefore,
        decimal? outstandingAfter) =>
        _loggerService.AuditAsync(
            LogCategory,
            action,
            "Write-off case diajukan; belum menyelesaikan tagihan sebagai pembayaran.",
            new
            {
                WriteOffCaseId = writeOffCase.Id,
                writeOffCase.InvoiceId,
                writeOffCase.Amount,
                writeOffCase.Status,
                OutstandingBefore = outstandingBefore,
                OutstandingAfter = outstandingAfter,
                writeOffCase.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private Task AuditWriteOffApprovalAsync(
        BilWriteOffCase writeOffCase,
        string beforeStatus,
        decimal outstandingBefore,
        decimal outstandingAfter,
        Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingWriteOff.Approve",
            $"WriteOffCase={writeOffCase.Id:N}; status {beforeStatus}->{writeOffCase.Status}; outstanding {outstandingBefore:0.00}->{outstandingAfter:0.00}.",
            new
            {
                WriteOffCaseId = writeOffCase.Id,
                writeOffCase.RequestedBy,
                writeOffCase.ApprovedBy,
                writeOffCase.IsFullSettlement,
                OutstandingBefore = outstandingBefore,
                OutstandingAfter = outstandingAfter,
                InvoiceStatus = writeOffCase.Invoice.Status,
                ActorUserId = actorUserId
            });

    private Task AuditReversalAsync(
        string action, BilAdjustment reversal, Guid originalId, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            action,
            "Reversal menghasilkan entry baru; posting asal tidak dimutasi.",
            new
            {
                ReversalAdjustmentId = reversal.Id,
                OriginalEntryId = originalId,
                reversal.InvoiceId,
                reversal.Direction,
                reversal.Amount,
                reversal.CorrelationId,
                ActorUserId = actorUserId
            });

    private static AdjustmentResponse MapAdjustment(BilAdjustment adjustment, bool isReplay) => new()
    {
        Id = adjustment.Id,
        InvoiceId = adjustment.InvoiceId,
        Direction = adjustment.Direction,
        Amount = adjustment.Amount,
        Status = adjustment.Status,
        RequestedBy = adjustment.RequestedBy,
        ApprovedBy = adjustment.ApprovedBy,
        Reason = adjustment.Reason,
        ReversesAdjustmentId = adjustment.ReversesAdjustmentId,
        ReversesWriteOffCaseId = adjustment.ReversesWriteOffCaseId,
        CorrelationId = adjustment.CorrelationId,
        RowVersion = adjustment.RowVersion,
        SubmittedAt = adjustment.SubmittedAt,
        PostedAt = adjustment.PostedAt,
        IsReplay = isReplay
    };

    private static WriteOffResponse MapWriteOff(
        BilWriteOffCase writeOffCase,
        decimal? outstandingBefore,
        decimal? outstandingAfter,
        bool isReplay) => new()
        {
            Id = writeOffCase.Id,
            InvoiceId = writeOffCase.InvoiceId,
            Amount = writeOffCase.Amount,
            IsFullSettlement = writeOffCase.IsFullSettlement,
            OutstandingBefore = outstandingBefore ?? 0,
            OutstandingAfter = outstandingAfter ?? 0,
            Status = writeOffCase.Status,
            RequestedBy = writeOffCase.RequestedBy,
            ApprovedBy = writeOffCase.ApprovedBy,
            Reason = writeOffCase.Reason,
            CorrelationId = writeOffCase.CorrelationId,
            RowVersion = writeOffCase.RowVersion,
            SubmittedAt = writeOffCase.SubmittedAt,
            PostedAt = writeOffCase.PostedAt,
            IsReplay = isReplay
        };
}

public abstract class BillingFinancialExceptionException : Exception
{
    protected BillingFinancialExceptionException(string message) : base(message) { }
    protected BillingFinancialExceptionException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingFinancialExceptionValidationException(string message)
    : BillingFinancialExceptionException(message);

public sealed class BillingFinancialExceptionForbiddenException(string message)
    : BillingFinancialExceptionException(message);

public sealed class BillingFinancialExceptionConflictException : BillingFinancialExceptionException
{
    public BillingFinancialExceptionConflictException(string message) : base(message) { }
    public BillingFinancialExceptionConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
