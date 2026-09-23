using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;

/// <summary>
/// Input manual utang supplier dan koreksinya (BE-FIN-019, 02-backend-architecture.md §4.22).
/// Satu-satunya penulis FinSupplierPayable pada task ini — jalur pembayaran (FinPayment,
/// FinPaymentAllocation) adalah tanggung jawab FinancePaymentService (BE-FIN-020, belum ada task
/// pemilik). PayableType = MEDICAL_SERVICE pada FinPayableAdjustment MUST NOT dibuat dari sini —
/// itu FinanceDoctorPayableService/BE-FIN-021, menunggu FinMedicalServicePayable (BLOCKED,
/// menunggu modul Medical Fee).
///
/// Sengaja BELUM menulis kejadian ke FinAccountingEventOutbox meski EventTypeCode-nya
/// (PENGAKUAN-HUTANG-SUPPLIER, PENYESUAIAN-HUTANG) sudah ada di katalog — mengikuti pola siklus
/// hidup yang sama dengan Receivable: FinanceReceivableService (BE-FIN-006/008) dibangun tanpa
/// panggilan outbox, baru disambungkan belakangan oleh BE-FIN-011. Menyambungkannya di sini akan
/// melampaui Cakupan BE-FIN-019 (hanya FinSupplierPayable/FinSupplierPayableItem/
/// FinPayableAdjustment), jadi ditunda ke task pemilik eksplisit berikutnya.
/// </summary>
public sealed class FinanceSupplierPayableService
{
    private const string LogCategory = "Corporate.FinanceManagement.Payable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceSupplierPayableService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Input manual (FIN-DEC-015, state-transition-matrix.md §5) — tidak menunggu PO/Receive
    // Order karena modul Purchasing belum ada.
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// OriginalAmount SENGAJA tidak diterima sebagai input — dihitung dari jumlah Amount seluruh
    /// item (Quantity × UnitPrice), supaya invariant "jumlah item = OriginalAmount induk"
    /// (02-backend-architecture.md §4.10) selalu benar dengan sendirinya, bukan divalidasi lalu
    /// ditolak.
    /// </summary>
    public async Task<FinSupplierPayable> CreateAsync(
        Guid supplierId, string supplierInvoiceNumber, DateOnly supplierInvoiceDate, string? description,
        IReadOnlyList<SupplierPayableItemRequest> items, Guid actorUserId, CancellationToken cancellationToken)
    {
        var invoiceNumber = ValidateReason(supplierInvoiceNumber, "Nomor faktur supplier", maxLength: 100);
        if (items is not { Count: > 0 })
            throw new PayableBadRequestException("Rincian invoice wajib diisi minimal satu baris.");

        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == supplierId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Supplier tidak ditemukan.");

        // state-transition-matrix.md §5: pasangan supplier + nomor faktur belum pernah ada.
        var duplicate = await _dbContext.FinSupplierPayables.AsNoTracking()
            .AnyAsync(x => x.SupplierId == supplierId && x.SupplierInvoiceNumber == invoiceNumber && !x.IsDelete, cancellationToken);
        if (duplicate)
            throw new PayableConflictException("Faktur supplier ini sudah pernah diinput");

        var itemEntities = new List<FinSupplierPayableItem>(items.Count);
        decimal originalAmount = 0m;
        foreach (var item in items)
        {
            var itemDescription = ValidateReason(item.Description, "Nama barang atau jasa", maxLength: 300);
            if (item.Quantity <= 0) throw new PayableBadRequestException("Jumlah (Quantity) harus lebih dari nol.");
            if (item.UnitPrice < 0) throw new PayableBadRequestException("Harga satuan tidak boleh negatif.");

            var amount = item.Quantity * item.UnitPrice;
            originalAmount += amount;
            itemEntities.Add(new FinSupplierPayableItem
            {
                Description = itemDescription,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = amount,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }
        if (originalAmount <= 0) throw new PayableBadRequestException("Total nilai faktur harus lebih dari nol.");

        var payable = new FinSupplierPayable
        {
            PayableNumber = GeneratePayableNumber(),
            SupplierId = supplierId,
            SupplierInvoiceNumber = invoiceNumber,
            SupplierInvoiceDate = supplierInvoiceDate,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OriginalAmount = originalAmount,
            OutstandingAmount = originalAmount,
            PaidAmount = 0m,
            AdjustedAmount = 0m,
            // Disalin dari MstSupplier saat dibuat (data-dictionary.md §4.1) — perubahan termin
            // supplier setelahnya MUST NOT mengubah utang yang sudah tercatat.
            PaymentTermDays = supplier.PaymentTermDays,
            DueDate = supplierInvoiceDate.AddDays(supplier.PaymentTermDays),
            Status = FinSupplierPayableStatuses.Outstanding,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        foreach (var item in itemEntities) payable.Items.Add(item);

        _dbContext.FinSupplierPayables.Add(payable);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Create", payable.Id, payable.Id, actorUserId);
        return payable;
    }

    // ------------------------------------------------------------------------------------
    // Koreksi (FinPayableAdjustment) — FIN-DES-014, FIN-DES-016. PayableType SELALU SUPPLIER
    // dari service ini (lihat ringkasan kelas).
    // ------------------------------------------------------------------------------------

    public async Task<FinPayableAdjustment> RequestAdjustmentAsync(
        Guid payableId, string direction, decimal amount, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var normalizedDirection = ValidateDirection(direction);
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan koreksi", maxLength: 500);

        var payable = await FindPayableAsync(payableId, cancellationToken);
        // state-transition-matrix.md §5: CANCELLED adalah status akhir — tidak ada tindakan lanjutan.
        if (payable.Status == FinSupplierPayableStatuses.Cancelled)
            throw new PayableValidationException("Utang yang sudah dibatalkan tidak dapat dikoreksi.");

        var adjustment = new FinPayableAdjustment
        {
            AdjustmentNumber = GenerateAdjustmentNumber(),
            PayableType = FinPayableAdjustmentPayableTypes.Supplier,
            SupplierPayableId = payableId,
            Direction = normalizedDirection,
            Amount = amount,
            Reason = reasonText,
            Status = FinPayableAdjustmentStatuses.Requested,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinPayableAdjustments.Add(adjustment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Adjustment.Request", adjustment.Id, payableId, actorUserId);
        return adjustment;
    }

    public Task<FinPayableAdjustment> ApproveAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: true, rejectionReason: null, cancellationToken);

    public Task<FinPayableAdjustment> RejectAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: false,
            ValidateReason(rejectionReason, "Alasan penolakan", maxLength: 500), cancellationToken);

    private async Task<FinPayableAdjustment> DecideAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, bool approve, string? rejectionReason, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            var adjustment = await _dbContext.FinPayableAdjustments
                .SingleOrDefaultAsync(x => x.Id == adjustmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan koreksi utang tidak ditemukan.");
            EnsureCurrent(adjustment.RowVersion, expectedRowVersion);

            if (adjustment.Status != FinPayableAdjustmentStatuses.Requested)
                throw new PayableValidationException("Permohonan ini sudah diputuskan sebelumnya.");
            // FIN-DEC-012: pengaju tidak boleh menyetujui/menolak permohonannya sendiri.
            if (actorUserId == adjustment.RequestedBy)
                throw new PayableValidationException(
                    "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang.");
            // Task ini hanya membangun jalur SUPPLIER — lihat ringkasan kelas.
            if (adjustment.PayableType != FinPayableAdjustmentPayableTypes.Supplier || adjustment.SupplierPayableId is null)
                throw new PayableValidationException("Jenis utang pada koreksi ini belum didukung.");

            await AcquireLockAsync($"FIN_PAYABLE_{adjustment.SupplierPayableId:N}", cancellationToken);
            var payable = await _dbContext.FinSupplierPayables
                .SingleOrDefaultAsync(x => x.Id == adjustment.SupplierPayableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Utang supplier tidak ditemukan.");

            if (approve)
            {
                // state-transition-matrix.md §5: CANCELLED adalah status akhir.
                if (payable.Status == FinSupplierPayableStatuses.Cancelled)
                    throw new PayableValidationException("Utang yang sudah dibatalkan tidak dapat dikoreksi.");

                // Konvensi liabilitas (kebalikan piutang): DEBIT mengurangi utang, CREDIT menambah
                // (state-transition-matrix.md §5, ditulis literal pada kontrak). DEBIT tidak boleh
                // melebihi sisa utang, sama seperti CREDIT tidak boleh melebihi sisa piutang pada
                // FinanceReceivableService.
                if (adjustment.Direction == FinPayableAdjustmentDirections.Debit && adjustment.Amount > payable.OutstandingAmount)
                    throw new PayableValidationException(
                        $"Nilai koreksi melebihi sisa utang. Sisa saat ini Rp {payable.OutstandingAmount:N0}.");

                if (adjustment.Direction == FinPayableAdjustmentDirections.Debit)
                {
                    payable.OutstandingAmount -= adjustment.Amount;
                    payable.AdjustedAmount += adjustment.Amount;
                }
                else
                {
                    payable.OutstandingAmount += adjustment.Amount;
                    payable.AdjustedAmount -= adjustment.Amount;
                }
                payable.UpdateDateTime = DateTime.UtcNow;
                payable.UpdateBy = actorUserId;
                payable.RowVersion = Guid.NewGuid();

                adjustment.Status = FinPayableAdjustmentStatuses.Approved;
            }
            else
            {
                adjustment.Status = FinPayableAdjustmentStatuses.Rejected;
                adjustment.RejectionReason = rejectionReason;
            }
            adjustment.ApprovedBy = actorUserId;
            adjustment.ApprovedAt = DateTimeOffset.UtcNow;
            adjustment.UpdateDateTime = DateTime.UtcNow;
            adjustment.UpdateBy = actorUserId;
            adjustment.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(approve ? "Adjustment.Approve" : "Adjustment.Reject", adjustment.Id, payable.Id, actorUserId);
            return adjustment;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
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
    // Pembatalan (state-transition-matrix.md §5) — tindakan LANGSUNG oleh Penyetuju AP, BUKAN
    // maker-checker (tidak ada status REQUESTED pada FinSupplierPayable).
    // ------------------------------------------------------------------------------------

    public async Task<FinSupplierPayable> CancelAsync(Guid payableId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var payable = await _dbContext.FinSupplierPayables
            .SingleOrDefaultAsync(x => x.Id == payableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Utang supplier tidak ditemukan.");
        EnsureCurrent(payable.RowVersion, expectedRowVersion);

        // state-transition-matrix.md §5: hanya OUTSTANDING yang boleh dibatalkan ("belum ada
        // pembayaran sama sekali"); PARTIAL/PAID/CANCELLED seluruhnya ditolak di sini.
        if (payable.Status != FinSupplierPayableStatuses.Outstanding)
            throw new PayableValidationException(
                $"Utang berstatus {payable.Status} tidak dapat dibatalkan. Pembatalan hanya berlaku selama belum ada pembayaran sama sekali.");

        payable.Status = FinSupplierPayableStatuses.Cancelled;
        payable.UpdateDateTime = DateTime.UtcNow;
        payable.UpdateBy = actorUserId;
        payable.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw Stale(exception);
        }
        await AuditAsync("Cancel", payable.Id, payable.Id, actorUserId);
        return payable;
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur bersama
    // ------------------------------------------------------------------------------------

    private async Task<FinSupplierPayable> FindPayableAsync(Guid payableId, CancellationToken cancellationToken) =>
        await _dbContext.FinSupplierPayables.AsNoTracking().SingleOrDefaultAsync(x => x.Id == payableId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Utang supplier tidak ditemukan.");

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

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static PayableConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static string ValidateDirection(string? direction)
    {
        var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinPayableAdjustmentDirections.Debit or FinPayableAdjustmentDirections.Credit))
            throw new PayableBadRequestException("Arah koreksi harus DEBIT atau CREDIT.");
        return normalized;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0) throw new PayableBadRequestException("Nominal harus lebih dari nol.");
    }

    private static string ValidateReason(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new PayableBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength) throw new PayableBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    // PayableNumber/AdjustmentNumber tidak punya format baku pada kontrak yang terkunci; dibuat
    // unik lewat Guid, bukan Count/Max/Last+1 (QBE-CODE-002/003), sampai ada keputusan skema
    // penomoran resmi. Prefix "PADJ" (bukan "ADJ" milik FinReceivableAdjustment) sekadar
    // membedakan asal domain saat dua nomor dibaca berdampingan — tidak ada makna kontraktual.
    private static string GeneratePayableNumber()
    {
        var candidate = $"AP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateAdjustmentNumber()
    {
        var candidate = $"PADJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    // Privasi: Reason/RejectionReason bisa memuat keterangan bebas dan MUST NOT masuk log
    // aplikasi, mengikuti pola FinanceReceivableService.
    private Task AuditAsync(string action, Guid entityId, Guid payableId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceSupplierPayable.{action}",
            $"Perubahan utang supplier dicatat. EntityId={entityId} PayableId={payableId}",
            new { EntityId = entityId, PayableId = payableId, ActorUserId = actorUserId });
}

/// <summary>Satu baris rincian invoice pada CreateAsync — bukan DTO folder karena belum ada controller yang mengonsumsinya (lihat laporan BE-FIN-019).</summary>
public sealed record SupplierPayableItemRequest(string Description, decimal Quantity, decimal UnitPrice);

public sealed class PayableBadRequestException(string message) : Exception(message);
public sealed class PayableValidationException(string message) : Exception(message);
public sealed class PayableConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
