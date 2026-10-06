using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;

/// <summary>
/// Layanan pembayaran keluar dan potongan AP (BE-FIN-020, FIN-DES-015, FIN-DES-026..028, 02-backend-architecture.md §4.22).
/// Diperluas oleh BE-FIN-036 (FIN-DES-045..047, FIN-DEC-057): pemakaian Deposit Retur sebagai sumber dana.
/// </summary>
public sealed class FinancePaymentService
{
    private const string LogCategory = "Corporate.FinanceManagement.Payable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;
    private readonly FinanceSupplierReturnService _supplierReturnService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;

    public FinancePaymentService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceAccountingOutboxService accountingOutboxService,
        FinanceSupplierReturnService supplierReturnService,
        FinanceSubledgerMovementService subledgerMovementService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
        _supplierReturnService = supplierReturnService;
        _subledgerMovementService = subledgerMovementService;
    }

    // ------------------------------------------------------------------------------------
    // 1. Pembuatan Draft Pembayaran (FIN-DEC-019, FIN-DES-015, state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> CreateDraftAsync(
        string paymentType,
        Guid payeeReferenceId,
        Guid bankAccountId,
        string paymentMethod,
        string? notes,
        IReadOnlyList<PaymentAllocationRequest> allocations,
        IReadOnlyList<PaymentDeductionRequest>? deductions,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var normalizedPaymentType = ValidatePaymentType(paymentType);
        var normalizedPaymentMethod = ValidatePaymentMethod(paymentMethod);
        if (payeeReferenceId == Guid.Empty)
            throw new PaymentBadRequestException("Identitas penerima pembayaran (PayeeReferenceId) wajib diisi.");
        if (allocations is not { Count: > 0 })
            throw new PaymentBadRequestException("Rincian alokasi utang wajib diisi minimal satu baris.");

        // FIN-VAL-055: Rekening sumber harus aktif.
        var bankAccount = await _dbContext.MstBankAccounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == bankAccountId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Rekening bank sumber pembayaran tidak ditemukan.");
        if (!bankAccount.IsActive)
            throw new PaymentValidationException("Rekening sumber pembayaran sedang tidak aktif.");

        // FIN-VAL-057: Utang yang sama tidak boleh dibayar dua kali dalam satu pembayaran.
        var duplicatePayable = allocations.GroupBy(x => x.PayableId).FirstOrDefault(g => g.Count() > 1);
        if (duplicatePayable is not null)
            throw new PaymentBadRequestException($"Utang {duplicatePayable.Key} muncul lebih dari sekali dalam pembayaran ini.");

        // Validasi dan susun baris alokasi
        var allocationEntities = new List<FinPaymentAllocation>(allocations.Count);
        decimal totalAmount = 0m;

        foreach (var alloc in allocations)
        {
            var allocPayableType = ValidatePayableType(alloc.PayableType);
            if (alloc.Amount <= 0)
                throw new PaymentBadRequestException("Nilai alokasi pembayaran harus lebih dari nol.");

            if (allocPayableType == FinPaymentAllocationPayableTypes.Supplier)
            {
                var payable = await _dbContext.FinSupplierPayables.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == alloc.PayableId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException($"Utang supplier {alloc.PayableId} tidak ditemukan.");

                if (payable.Status is FinSupplierPayableStatuses.Cancelled or FinSupplierPayableStatuses.Paid)
                    throw new PaymentValidationException(
                        $"Utang {payable.PayableNumber} berstatus {payable.Status} sehingga tidak dapat dibayar.");

                // FIN-VAL-053: Alokasi tidak boleh melebihi sisa utang.
                if (alloc.Amount > payable.OutstandingAmount)
                    throw new PaymentValidationException(
                        $"Nilai pembayaran melebihi sisa utang {payable.PayableNumber}. Sisa Rp {payable.OutstandingAmount:N0}.");

                allocationEntities.Add(new FinPaymentAllocation
                {
                    PayableType = FinPaymentAllocationPayableTypes.Supplier,
                    SupplierPayableId = payable.Id,
                    MedicalServicePayableId = null,
                    Amount = alloc.Amount,
                    IsReversal = false,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
            else
            {
                // BE-FIN-021 (FinMedicalServicePayable) belum aktif — modul Medical Fee belum ada.
                throw new PaymentValidationException("Jenis utang MEDICAL_SERVICE belum didukung pada task ini.");
            }

            totalAmount += alloc.Amount;
        }

        if (totalAmount <= 0)
            throw new PaymentBadRequestException("Total pembayaran harus lebih dari nol.");

        // Validasi dan susun baris potongan/tambahan (FR-FIN-050, Amendment A.3)
        var deductionEntities = new List<FinPaymentDeduction>();
        decimal deductionAmount = 0m;
        decimal additionAmount = 0m;

        if (deductions is { Count: > 0 })
        {
            foreach (var ded in deductions)
            {
                var dedType = ValidateDeductionType(ded.DeductionType);
                var dedDirection = ValidateDeductionDirection(ded.Direction);
                if (ded.Amount <= 0)
                    throw new PaymentBadRequestException("Nilai potongan/tambahan harus lebih dari nol.");

                string? reason = null;
                if (!string.IsNullOrWhiteSpace(ded.Reason))
                    reason = ValidateText(ded.Reason, "Alasan", maxLength: 500);

                // CK_FinPaymentDeduction_OtherReason: pos lain-lain wajib menyebut alasan.
                if (dedType == FinPaymentDeductionTypes.Other && string.IsNullOrWhiteSpace(reason))
                    throw new PaymentBadRequestException("Alasan wajib diisi untuk jenis potongan/tambahan OTHER.");

                string? refNum = null;
                if (!string.IsNullOrWhiteSpace(ded.ReferenceNumber))
                    refNum = ValidateText(ded.ReferenceNumber, "Nomor referensi potongan", maxLength: 100);

                if (dedDirection == FinPaymentDeductionDirections.Deduction)
                    deductionAmount += ded.Amount;
                else
                    additionAmount += ded.Amount;

                deductionEntities.Add(new FinPaymentDeduction
                {
                    DeductionType = dedType,
                    Direction = dedDirection,
                    Amount = ded.Amount,
                    Reason = reason,
                    ReferenceNumber = refNum,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }

        // FR-FIN-050, CK_FinPayment_NetTransfer: NetTransferAmount = TotalAmount - DeductionAmount + AdditionAmount
        var netTransferAmount = totalAmount - deductionAmount + additionAmount;
        if (netTransferAmount < 0)
            throw new PaymentValidationException(
                $"Nilai transfer bersih tidak boleh kurang dari nol. Total utang Rp {totalAmount:N0}, potongan Rp {deductionAmount:N0}, tambahan Rp {additionAmount:N0}.");

        // FIN-DEC-022, FIN-DEC-052: Penentuan jenjang persetujuan
        var approvalTier = FinanceApprovalTierResolver.Resolve(totalAmount);

        var payment = new FinPayment
        {
            PaymentNumber = GeneratePaymentNumber(),
            PaymentType = normalizedPaymentType,
            PayeeReferenceId = payeeReferenceId,
            BankAccountId = bankAccountId,
            PaymentMethod = normalizedPaymentMethod,
            TotalAmount = totalAmount,
            AllocatedAmount = totalAmount, // Teralokasi penuh pada saat penyusunan draft
            DeductionAmount = deductionAmount,
            AdditionAmount = additionAmount,
            NetTransferAmount = netTransferAmount,
            Status = FinPaymentStatuses.Draft,
            ApprovalTier = approvalTier,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        foreach (var item in allocationEntities) payment.Allocations.Add(item);
        foreach (var item in deductionEntities) payment.Deductions.Add(item);

        _dbContext.FinPayments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("CreateDraft", payment.Id, actorUserId);
        return payment;
    }

    // ------------------------------------------------------------------------------------
    // 2. Pengubahan Draft Pembayaran (state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> UpdateDraftAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        Guid bankAccountId,
        string paymentMethod,
        string? notes,
        IReadOnlyList<PaymentAllocationRequest> allocations,
        IReadOnlyList<PaymentDeductionRequest>? deductions,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var payment = await _dbContext.FinPayments
            .Include(x => x.Allocations)
            .Include(x => x.Deductions)
            .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

        EnsureCurrent(payment.RowVersion, expectedRowVersion);

        if (payment.Status != FinPaymentStatuses.Draft)
            throw new PaymentValidationException($"Hanya pembayaran berstatus DRAFT yang dapat diubah. Status saat ini: {payment.Status}.");

        var normalizedPaymentMethod = ValidatePaymentMethod(paymentMethod);
        var bankAccount = await _dbContext.MstBankAccounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == bankAccountId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Rekening bank sumber pembayaran tidak ditemukan.");
        if (!bankAccount.IsActive)
            throw new PaymentValidationException("Rekening sumber pembayaran sedang tidak aktif.");

        if (allocations is not { Count: > 0 })
            throw new PaymentBadRequestException("Rincian alokasi utang wajib diisi minimal satu baris.");

        var duplicatePayable = allocations.GroupBy(x => x.PayableId).FirstOrDefault(g => g.Count() > 1);
        if (duplicatePayable is not null)
            throw new PaymentBadRequestException($"Utang {duplicatePayable.Key} muncul lebih dari sekali dalam pembayaran ini.");

        // Bersihkan alokasi & potongan lama
        _dbContext.FinPaymentAllocations.RemoveRange(payment.Allocations);
        _dbContext.FinPaymentDeductions.RemoveRange(payment.Deductions);

        decimal totalAmount = 0m;
        foreach (var alloc in allocations)
        {
            var allocPayableType = ValidatePayableType(alloc.PayableType);
            if (alloc.Amount <= 0)
                throw new PaymentBadRequestException("Nilai alokasi pembayaran harus lebih dari nol.");

            if (allocPayableType == FinPaymentAllocationPayableTypes.Supplier)
            {
                var payable = await _dbContext.FinSupplierPayables.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == alloc.PayableId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException($"Utang supplier {alloc.PayableId} tidak ditemukan.");

                if (payable.Status is FinSupplierPayableStatuses.Cancelled or FinSupplierPayableStatuses.Paid)
                    throw new PaymentValidationException($"Utang {payable.PayableNumber} berstatus {payable.Status} sehingga tidak dapat dibayar.");

                if (alloc.Amount > payable.OutstandingAmount)
                    throw new PaymentValidationException($"Nilai pembayaran melebihi sisa utang {payable.PayableNumber}. Sisa Rp {payable.OutstandingAmount:N0}.");

                payment.Allocations.Add(new FinPaymentAllocation
                {
                    PaymentId = payment.Id,
                    PayableType = FinPaymentAllocationPayableTypes.Supplier,
                    SupplierPayableId = payable.Id,
                    Amount = alloc.Amount,
                    IsReversal = false,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
            else
            {
                throw new PaymentValidationException("Jenis utang MEDICAL_SERVICE belum didukung pada task ini.");
            }
            totalAmount += alloc.Amount;
        }

        decimal deductionAmount = 0m;
        decimal additionAmount = 0m;
        if (deductions is { Count: > 0 })
        {
            foreach (var ded in deductions)
            {
                var dedType = ValidateDeductionType(ded.DeductionType);
                var dedDirection = ValidateDeductionDirection(ded.Direction);
                if (ded.Amount <= 0)
                    throw new PaymentBadRequestException("Nilai potongan/tambahan harus lebih dari nol.");

                string? reason = null;
                if (!string.IsNullOrWhiteSpace(ded.Reason))
                    reason = ValidateText(ded.Reason, "Alasan", maxLength: 500);

                if (dedType == FinPaymentDeductionTypes.Other && string.IsNullOrWhiteSpace(reason))
                    throw new PaymentBadRequestException("Alasan wajib diisi untuk jenis potongan/tambahan OTHER.");

                string? refNum = null;
                if (!string.IsNullOrWhiteSpace(ded.ReferenceNumber))
                    refNum = ValidateText(ded.ReferenceNumber, "Nomor referensi potongan", maxLength: 100);

                if (dedDirection == FinPaymentDeductionDirections.Deduction)
                    deductionAmount += ded.Amount;
                else
                    additionAmount += ded.Amount;

                payment.Deductions.Add(new FinPaymentDeduction
                {
                    PaymentId = payment.Id,
                    DeductionType = dedType,
                    Direction = dedDirection,
                    Amount = ded.Amount,
                    Reason = reason,
                    ReferenceNumber = refNum,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }

        // FR-FIN-050, CK_FinPayment_NetTransfer: NetTransferAmount = TotalAmount - DeductionAmount + AdditionAmount - DepositAppliedAmount
        var netTransferAmount = totalAmount - deductionAmount + additionAmount - payment.DepositAppliedAmount;
        if (netTransferAmount < 0)
            throw new PaymentValidationException(
                $"Nilai transfer bersih tidak boleh kurang dari nol. Total utang Rp {totalAmount:N0}, potongan Rp {deductionAmount:N0}, tambahan Rp {additionAmount:N0}, deposit retur Rp {payment.DepositAppliedAmount:N0}.");

        payment.BankAccountId = bankAccountId;
        payment.PaymentMethod = normalizedPaymentMethod;
        payment.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        payment.TotalAmount = totalAmount;
        payment.AllocatedAmount = totalAmount;
        payment.DeductionAmount = deductionAmount;
        payment.AdditionAmount = additionAmount;
        payment.NetTransferAmount = netTransferAmount;
        payment.ApprovalTier = FinanceApprovalTierResolver.Resolve(totalAmount);
        payment.UpdateDateTime = DateTime.UtcNow;
        payment.UpdateBy = actorUserId;
        payment.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("UpdateDraft", payment.Id, actorUserId);
        return payment;
    }

    // ------------------------------------------------------------------------------------
    // 3. Pengajuan Pembayaran (DRAFT -> SUBMITTED) (state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> SubmitAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var payment = await _dbContext.FinPayments
            .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

        EnsureCurrent(payment.RowVersion, expectedRowVersion);

        if (payment.Status != FinPaymentStatuses.Draft)
            throw new PaymentValidationException($"Hanya pembayaran berstatus DRAFT yang dapat diajukan. Status saat ini: {payment.Status}.");

        // FIN-VAL-050: Total pembayaran harus sama dengan rincian utang yang dilunasi
        if (payment.TotalAmount != payment.AllocatedAmount)
            throw new PaymentValidationException(
                $"Total pembayaran Rp {payment.TotalAmount:N0} tidak sama dengan jumlah utang yang dipilih Rp {payment.AllocatedAmount:N0}.");

        // FIN-VAL-091: Nilai transfer harus lebih dari nol bila tidak memakai deposit retur (FIN-VAL-091 diubah BE-FIN-036)
        if (payment.NetTransferAmount == 0m && payment.DepositAppliedAmount == 0m)
            throw new PaymentValidationException(
                "Seluruh jasa habis oleh potongan, sehingga tidak ada uang yang perlu ditransfer. Selesaikan lewat koreksi utang, bukan pembayaran.");

        payment.Status = FinPaymentStatuses.Submitted;
        payment.UpdateDateTime = DateTime.UtcNow;
        payment.UpdateBy = actorUserId;
        payment.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Submit", payment.Id, actorUserId);
        return payment;
    }

    // ------------------------------------------------------------------------------------
    // 4. Persetujuan Pembayaran (SUBMITTED -> APPROVED) (state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> ApproveAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var payment = await _dbContext.FinPayments
            .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

        EnsureCurrent(payment.RowVersion, expectedRowVersion);

        if (payment.Status != FinPaymentStatuses.Submitted)
            throw new PaymentValidationException($"Hanya pembayaran berstatus SUBMITTED yang dapat disetujui. Status saat ini: {payment.Status}.");

        // FIN-VAL-051 & CK_FinPayment_MakerChecker: Pengaju tidak boleh menyetujui pembayarannya sendiri
        if (actorUserId == payment.RequestedBy)
            throw new PaymentValidationException("Pengaju tidak boleh menyetujui pembayarannya sendiri.");

        payment.Status = FinPaymentStatuses.Approved;
        payment.ApprovedBy = actorUserId;
        payment.ApprovedAt = DateTimeOffset.UtcNow;
        payment.UpdateDateTime = DateTime.UtcNow;
        payment.UpdateBy = actorUserId;
        payment.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Approve", payment.Id, actorUserId);
        return payment;
    }

    // ------------------------------------------------------------------------------------
    // 5. Penolakan Pembayaran (SUBMITTED -> REJECTED) (state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> RejectAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        string rejectionReason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYMENT_{paymentId:N}", cancellationToken);

            var payment = await _dbContext.FinPayments
                .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

            EnsureCurrent(payment.RowVersion, expectedRowVersion);

            if (payment.Status != FinPaymentStatuses.Submitted)
                throw new PaymentValidationException($"Hanya pembayaran berstatus SUBMITTED yang dapat ditolak. Status saat ini: {payment.Status}.");

            var reason = ValidateText(rejectionReason, "Alasan penolakan", maxLength: 500);

            payment.Status = FinPaymentStatuses.Rejected;
            payment.RejectionReason = reason;
            payment.ApprovedBy = actorUserId;
            payment.ApprovedAt = DateTimeOffset.UtcNow;

            // FIN-DES-046: pelepasan deposit saat REJECTED
            if (payment.DepositAppliedAmount > 0)
            {
                await _supplierReturnService.ReleaseReservedByPaymentAsync(payment.Id, actorUserId, cancellationToken);
                payment.DepositAppliedAmount = 0m;
                payment.NetTransferAmount = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount;
            }

            payment.UpdateDateTime = DateTime.UtcNow;
            payment.UpdateBy = actorUserId;
            payment.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Reject", payment.Id, actorUserId);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // 6. Penandaan Pembayaran Lunas / Uang Keluar (APPROVED -> PAID)
    //    (FR-FIN-050, FR-FIN-051, state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> MarkPaidAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        string? referenceNumber,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYMENT_{paymentId:N}", cancellationToken);

            var payment = await _dbContext.FinPayments
                .Include(x => x.Allocations)
                .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

            EnsureCurrent(payment.RowVersion, expectedRowVersion);

            // state-transition-matrix.md §6: DRAFT/SUBMITTED ditolak ("Wajib lewat pengajuan dan persetujuan")
            // PAID ditolak ("Status akhir")
            if (payment.Status != FinPaymentStatuses.Approved)
                throw new PaymentValidationException(
                    $"Pembayaran berstatus {payment.Status} tidak dapat ditandai lunas. Pembayaran harus berstatus APPROVED terlebih dahulu.");

            // FIN-VAL-056: Nomor bukti transfer wajib hanya bila NetTransferAmount > 0 (diubah BE-FIN-036)
            string? refNumber;
            if (payment.NetTransferAmount > 0)
            {
                refNumber = ValidateText(referenceNumber, "Nomor bukti transfer", maxLength: 150);
            }
            else
            {
                refNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim();
            }

            // FR-FIN-051: Sisa utang berkurang sebesar alokasinya, bukan sebesar uang yang ditransfer.
            // FinancePaymentService adalah SATU-SATUNYA penulis PaidAmount pada FinSupplierPayable (BE-FIN-019 note 5).
            foreach (var alloc in payment.Allocations)
            {
                if (alloc.SupplierPayableId.HasValue)
                {
                    await AcquireLockAsync($"FIN_PAYABLE_{alloc.SupplierPayableId.Value:N}", cancellationToken);
                    var payable = await _dbContext.FinSupplierPayables
                        .SingleOrDefaultAsync(x => x.Id == alloc.SupplierPayableId.Value && !x.IsDelete, cancellationToken)
                        ?? throw new KeyNotFoundException($"Utang supplier {alloc.SupplierPayableId.Value} tidak ditemukan.");

                    if (payable.Status is FinSupplierPayableStatuses.Cancelled)
                        throw new PaymentValidationException($"Utang supplier {payable.PayableNumber} telah dibatalkan.");

                    if (alloc.Amount > payable.OutstandingAmount)
                        throw new PaymentValidationException(
                            $"Nilai alokasi Rp {alloc.Amount:N0} melebihi sisa utang {payable.PayableNumber} (Rp {payable.OutstandingAmount:N0}).");

                    var prevOutstanding = payable.OutstandingAmount;
                    payable.OutstandingAmount -= alloc.Amount;
                    payable.PaidAmount += alloc.Amount;

                    if (payable.OutstandingAmount == 0m)
                        payable.Status = FinSupplierPayableStatuses.Paid;
                    else
                        payable.Status = FinSupplierPayableStatuses.Partial;

                    payable.UpdateDateTime = DateTime.UtcNow;
                    payable.UpdateBy = actorUserId;
                    payable.RowVersion = Guid.NewGuid();

                    // BE-FIN-061, FIN-DES-079: Mutasi subledger PEMBAYARAN-DOKUMEN (satu baris per alokasi utang supplier)
                    // Tanggal bisnis disalin dari FinPayment.ApprovedAt dalam WIB (FIN-DEC-130), BUKAN dari PaidAt!
                    var approvedAt = payment.ApprovedAt ?? DateTimeOffset.UtcNow;
                    var approvedAtWibDate = FinanceBusinessDate.ToDateOnly(approvedAt);
                    var fundingSourceType = string.Equals(payment.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase) ? "CASH" : "BANK_ACCOUNT";

                    await _subledgerMovementService.RecordSupplierPayableMovementAsync(
                        payable: payable,
                        movementType: FinSupplierPayableMovementTypes.PembayaranDokumen,
                        deltaAmount: -alloc.Amount,
                        balanceBefore: prevOutstanding,
                        occurredAt: DateTimeOffset.UtcNow,
                        actorUserId: actorUserId,
                        correlationId: payable.Id,
                        causationId: payment.Id,
                        businessDateOverride: approvedAtWibDate,
                        paymentId: payment.Id,
                        paymentAllocationId: alloc.Id,
                        paymentMethodCode: payment.PaymentMethod,
                        fundingSourceType: fundingSourceType,
                        fundingSourceId: payment.BankAccountId,
                        referenceNumber: payment.PaymentNumber,
                        notes: $"Pembayaran dokumen {payment.PaymentNumber} alokasi {alloc.Amount:N0}",
                        cancellationToken: cancellationToken);
                }
            }

            // FIN-DES-046: baris pemakaian deposit berstatus RESERVED berubah menjadi APPLIED
            if (payment.DepositAppliedAmount > 0)
            {
                await _supplierReturnService.MarkAppliedByPaymentAsync(payment.Id, actorUserId, cancellationToken);
            }

            payment.Status = FinPaymentStatuses.Paid;
            payment.PaidAt = DateTimeOffset.UtcNow;
            payment.ReferenceNumber = refNumber;
            payment.UpdateDateTime = DateTime.UtcNow;
            payment.UpdateBy = actorUserId;
            payment.RowVersion = Guid.NewGuid();

            // BE-FIN-062, FIN-DES-081: Mutasi kas keluar PEMBAYARAN-TUNAI-DOKUMEN untuk FinPayment CASH (Sumber 4)
            // PENTING: Nilai mutasi kas adalah NetTransferAmount, BUKAN jumlah alokasi! (FIN-VAL-168)
            if (string.Equals(payment.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase) && payment.NetTransferAmount > 0)
            {
                var approvedAt = payment.ApprovedAt ?? DateTimeOffset.UtcNow;
                var approvedAtWibDate = FinanceBusinessDate.ToDateOnly(approvedAt);
                await _subledgerMovementService.RecordCashMovementAsync(
                    movementType: FinCashMovementTypes.PembayaranTunaiDokumen,
                    direction: FinCashMovementDirections.Out,
                    amount: payment.NetTransferAmount,
                    businessDate: approvedAtWibDate,
                    occurredAt: payment.PaidAt ?? DateTimeOffset.UtcNow,
                    sourceReferenceType: FinCashMovementSourceReferenceTypes.Payment,
                    sourceReferenceId: payment.Id.ToString(),
                    actorUserId: actorUserId,
                    correlationId: payment.Id,
                    causationId: payment.Id,
                    paymentMethodCode: "CASH",
                    notes: $"Pembayaran kas dokumen {payment.PaymentNumber} net {payment.NetTransferAmount:N0}",
                    cancellationToken: cancellationToken);
            }

            var eventOccurredAt = DateTimeOffset.UtcNow;
            var apPaymentAmount = payment.TotalAmount - payment.DepositAppliedAmount;

            // FIN-DES-047, FIN-VAL-131: Stage event PEMBAYARAN-HUTANG-SUPPLIER hanya jika Amount > 0 (dilewati bila nol)
            if (apPaymentAmount > 0)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PembayaranHutangSupplier,
                    SourceTransactionId = payment.PaymentNumber,
                    EventOccurredAt = eventOccurredAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(eventOccurredAt),
                    Amount = apPaymentAmount,
                    CorrelationId = payment.Id,
                    CausationId = payment.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            // FIN-DES-047, FIN-DEC-061/066: Stage event PEMAKAIAN-KREDIT-RETUR-PEMBELIAN jika DepositAppliedAmount > 0
            if (payment.DepositAppliedAmount > 0)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PemakaianKreditReturPembelian,
                    SourceTransactionId = payment.PaymentNumber,
                    EventOccurredAt = eventOccurredAt,
                    AccountingDate = FinanceBusinessDate.ToDateOnly(eventOccurredAt),
                    Amount = payment.DepositAppliedAmount,
                    CorrelationId = payment.Id,
                    CausationId = payment.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("MarkPaid", payment.Id, actorUserId);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // 7. Pembatalan Pembayaran (state-transition-matrix.md §6)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> CancelAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYMENT_{paymentId:N}", cancellationToken);

            var payment = await _dbContext.FinPayments
                .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

            EnsureCurrent(payment.RowVersion, expectedRowVersion);

            // state-transition-matrix.md §6: PAID adalah status akhir, tidak dapat dibatalkan.
            if (payment.Status == FinPaymentStatuses.Paid)
                throw new PaymentValidationException("Pembayaran yang sudah dibayar tidak dapat dibatalkan. Kekeliruan dibetulkan lewat koreksi utang.");

            if (payment.Status == FinPaymentStatuses.Cancelled)
                throw new PaymentValidationException("Pembayaran sudah dibatalkan sebelumnya.");

            payment.Status = FinPaymentStatuses.Cancelled;

            // FIN-DES-046: pelepasan deposit saat CANCELLED
            if (payment.DepositAppliedAmount > 0)
            {
                await _supplierReturnService.ReleaseReservedByPaymentAsync(payment.Id, actorUserId, cancellationToken);
                payment.DepositAppliedAmount = 0m;
                payment.NetTransferAmount = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount;
            }

            payment.UpdateDateTime = DateTime.UtcNow;
            payment.UpdateBy = actorUserId;
            payment.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Cancel", payment.Id, actorUserId);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // 8. Pemakaian Deposit Retur (BE-FIN-036, FIN-DES-045, FIN-DES-046, FIN-DEC-057)
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment> AddReturnDepositAsync(
        Guid paymentId,
        Guid expectedRowVersion,
        Guid supplierReturnDepositId,
        decimal usedAmount,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (supplierReturnDepositId == Guid.Empty)
            throw new PaymentBadRequestException("Deposit Retur (SupplierReturnDepositId) wajib diisi.");
        if (usedAmount <= 0)
            throw new PaymentBadRequestException("Nilai pemakaian deposit harus lebih dari nol.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYMENT_{paymentId:N}", cancellationToken);
            await AcquireLockAsync($"FIN_RETURN_DEPOSIT_{supplierReturnDepositId:N}", cancellationToken);

            var payment = await _dbContext.FinPayments
                .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

            EnsureCurrent(payment.RowVersion, expectedRowVersion);

            // FIN-VAL-124: Deposit hanya ditambah/dilepas selama draf
            if (payment.Status != FinPaymentStatuses.Draft)
                throw new PaymentValidationException("Pembayaran ini sudah diajukan, sehingga sumber dananya tidak dapat diubah.");

            // FIN-VAL-123: Deposit hanya untuk pembayaran supplier
            if (payment.PaymentType != FinPaymentTypes.Supplier)
                throw new PaymentValidationException("Deposit Retur hanya dapat dipakai pada pembayaran supplier.");

            // FIN-VAL-125: Deposit yang sama tidak boleh aktif dua kali dalam satu pembayaran
            var existingActiveUsage = await _dbContext.FinSupplierReturnDepositUsages
                .AnyAsync(x => x.PaymentId == paymentId
                    && x.SupplierReturnDepositId == supplierReturnDepositId
                    && x.Status != FinSupplierReturnDepositUsageStatuses.Released
                    && !x.IsDelete, cancellationToken);
            if (existingActiveUsage)
                throw new PaymentConflictException("Deposit Retur ini sudah dipakai di pembayaran ini.");

            // FIN-VAL-126: Nilai transfer tidak boleh negatif karena deposit
            var maxAllowedDeposit = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount - payment.DepositAppliedAmount;
            if (usedAmount > maxAllowedDeposit)
            {
                var sisa = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount - payment.DepositAppliedAmount;
                throw new PaymentValidationException($"Nilai deposit melebihi yang perlu dibayar. Maksimum Rp {sisa:N0}.");
            }

            // Panggil FinanceSupplierReturnService untuk mencadangkan deposit (ikut transaksi ini)
            await _supplierReturnService.ReserveAsync(
                supplierReturnDepositId, usedAmount, payment.Id, payment.PayeeReferenceId, actorUserId, cancellationToken);

            payment.DepositAppliedAmount += usedAmount;
            payment.NetTransferAmount = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount - payment.DepositAppliedAmount;
            payment.UpdateDateTime = DateTime.UtcNow;
            payment.UpdateBy = actorUserId;
            payment.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("AddReturnDeposit", payment.Id, actorUserId);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<FinPayment> ReleaseReturnDepositAsync(
        Guid paymentId,
        Guid usageId,
        Guid expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYMENT_{paymentId:N}", cancellationToken);

            var payment = await _dbContext.FinPayments
                .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembayaran tidak ditemukan.");

            EnsureCurrent(payment.RowVersion, expectedRowVersion);

            // FIN-VAL-124: Deposit hanya ditambah/dilepas selama draf
            if (payment.Status != FinPaymentStatuses.Draft)
                throw new PaymentValidationException("Pembayaran ini sudah diajukan, sehingga sumber dananya tidak dapat diubah.");

            var releasedUsage = await _supplierReturnService.ReleaseUsageAsync(usageId, payment.Id, actorUserId, cancellationToken);

            payment.DepositAppliedAmount = Math.Max(0m, payment.DepositAppliedAmount - releasedUsage.UsedAmount);
            payment.NetTransferAmount = payment.TotalAmount - payment.DeductionAmount + payment.AdditionAmount - payment.DepositAppliedAmount;
            payment.UpdateDateTime = DateTime.UtcNow;
            payment.UpdateBy = actorUserId;
            payment.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("ReleaseReturnDeposit", payment.Id, actorUserId);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(transaction);
            throw Stale(ex);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<List<PaymentReturnDepositResponse>> GetReturnDepositsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var usages = await _dbContext.FinSupplierReturnDepositUsages
            .Include(x => x.SupplierReturnDeposit)
                .ThenInclude(d => d!.SourceReturn)
            .Where(x => x.PaymentId == paymentId && !x.IsDelete)
            .OrderBy(x => x.CreateDateTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return usages.Select(u => new PaymentReturnDepositResponse
        {
            Id = u.Id,
            SupplierReturnDepositId = u.SupplierReturnDepositId,
            ReturnNumber = u.SupplierReturnDeposit?.SourceReturn?.ReturnNumber ?? string.Empty,
            UsedAmount = u.UsedAmount,
            Status = u.Status,
            UsedAt = u.UsedAt,
            ReleasedAt = u.ReleasedAt
        }).ToList();
    }


    // ------------------------------------------------------------------------------------
    // Query Helpers
    // ------------------------------------------------------------------------------------

    public async Task<FinPayment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken) =>
        await _dbContext.FinPayments
            .Include(x => x.BankAccount)
            .Include(x => x.Allocations)
                .ThenInclude(a => a.SupplierPayable)
            .Include(x => x.Deductions)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Internal Resolvers & Validators
    // ------------------------------------------------------------------------------------
    //
    // FIN-DEC-022: Approval pembayaran AP memakai approval berjenjang berdasarkan total nominal.
    // Ambang nominalnya sudah diratifikasi (FIN-DEC-052) dan resolvernya diekstrak ke
    // FinanceApprovalTierResolver (BE-FIN-028) supaya dipakai bersama oleh PO dan Purchasing
    // Invoice — lihat FinanceApprovalTierResolver.cs.

    private static string ValidatePaymentType(string? paymentType)
    {
        var normalized = (paymentType ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPaymentTypes.Supplier or FinPaymentTypes.MedicalService))
            throw new PaymentBadRequestException("PaymentType harus SUPPLIER atau MEDICAL_SERVICE.");
        return normalized;
    }

    private static string ValidatePaymentMethod(string? paymentMethod)
    {
        var normalized = (paymentMethod ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPaymentMethods.Transfer or FinPaymentMethods.Cash or FinPaymentMethods.Cheque))
            throw new PaymentBadRequestException("PaymentMethod harus TRANSFER, CASH, atau CHEQUE.");
        return normalized;
    }

    private static string ValidatePayableType(string? payableType)
    {
        var normalized = (payableType ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPaymentAllocationPayableTypes.Supplier or FinPaymentAllocationPayableTypes.MedicalService))
            throw new PaymentBadRequestException("PayableType harus SUPPLIER atau MEDICAL_SERVICE.");
        return normalized;
    }

    private static string ValidateDeductionType(string? deductionType)
    {
        var normalized = (deductionType ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPaymentDeductionTypes.Pph21 or FinPaymentDeductionTypes.Kasbon
            or FinPaymentDeductionTypes.PatientDebt or FinPaymentDeductionTypes.SittingFee
            or FinPaymentDeductionTypes.Kso or FinPaymentDeductionTypes.Iuran or FinPaymentDeductionTypes.Other))
            throw new PaymentBadRequestException($"DeductionType '{deductionType}' tidak valid.");
        return normalized;
    }

    private static string ValidateDeductionDirection(string? direction)
    {
        var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPaymentDeductionDirections.Deduction or FinPaymentDeductionDirections.Addition))
            throw new PaymentBadRequestException("Direction potongan/tambahan harus DEDUCTION atau ADDITION.");
        return normalized;
    }

    private static string ValidateText(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PaymentBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new PaymentBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string GeneratePaymentNumber()
    {
        var candidate = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static PaymentConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private Task AuditAsync(string action, Guid paymentId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinancePayment.{action}",
            $"Perubahan pembayaran keluar dicatat. PaymentId={paymentId}",
            new { PaymentId = paymentId, ActorUserId = actorUserId });
}

public sealed record PaymentAllocationRequest(string PayableType, Guid PayableId, decimal Amount);

public sealed record PaymentDeductionRequest(string DeductionType, string Direction, decimal Amount, string? Reason, string? ReferenceNumber);

public sealed class PaymentBadRequestException(string message) : Exception(message);
public sealed class PaymentValidationException(string message) : Exception(message);
public sealed class PaymentConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
