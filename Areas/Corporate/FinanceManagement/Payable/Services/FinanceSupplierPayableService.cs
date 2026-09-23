using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;

/// <summary>
/// Input manual utang supplier, koreksi, dan pembayaran langsung (Finance Management V2).
/// </summary>
public sealed class FinanceSupplierPayableService
{
    private const string LogCategory = "Corporate.FinanceManagement.Payable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceSupplierPayableService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
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

        // Stage event AP_CREATED ke Accounting Integration Outbox
        await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
        {
            EventTypeCode = FinAccountingEventTypeCodes.ApCreated,
            SourceTransactionId = payable.PayableNumber,
            EventOccurredAt = DateTimeOffset.UtcNow,
            AccountingDate = payable.SupplierInvoiceDate,
            Amount = payable.OriginalAmount,
            CorrelationId = payable.Id,
            CausationId = payable.Id,
            ActorUserId = actorUserId
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Create", payable.Id, payable.Id, actorUserId);
        return payable;
    }

    // ------------------------------------------------------------------------------------
    // Pembacaan & Daftar Berpaging (Finance Management V2)
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<FinSupplierPayable>> GetPagedAsync(SupplierPayableQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinSupplierPayables.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => !x.IsDelete);

        if (query.SupplierId.HasValue && query.SupplierId.Value != Guid.Empty)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());

        if (query.InvoiceDateFrom.HasValue)
            q = q.Where(x => x.SupplierInvoiceDate >= query.InvoiceDateFrom.Value);

        if (query.InvoiceDateTo.HasValue)
            q = q.Where(x => x.SupplierInvoiceDate <= query.InvoiceDateTo.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(x => x.PayableNumber.ToLower().Contains(s) || x.SupplierInvoiceNumber.ToLower().Contains(s));
        }

        var totalCount = await q.CountAsync(cancellationToken);

        q = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("duedate", "asc") => q.OrderBy(x => x.DueDate),
            ("duedate", "desc") => q.OrderByDescending(x => x.DueDate),
            ("amount", "asc") => q.OrderBy(x => x.OriginalAmount),
            ("amount", "desc") => q.OrderByDescending(x => x.OriginalAmount),
            ("outstanding", "asc") => q.OrderBy(x => x.OutstandingAmount),
            ("outstanding", "desc") => q.OrderByDescending(x => x.OutstandingAmount),
            ("invoicedate", "asc") => q.OrderBy(x => x.SupplierInvoiceDate),
            ("invoicedate", "desc") => q.OrderByDescending(x => x.SupplierInvoiceDate),
            (_, "asc") => q.OrderBy(x => x.CreateDateTime),
            _ => q.OrderByDescending(x => x.CreateDateTime)
        };

        var items = await q.Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FinSupplierPayable>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<FinSupplierPayable> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinSupplierPayables.AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Adjustments)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Utang supplier tidak ditemukan.");

    public async Task<SupplierPayablePaymentResponse> RecordDirectPaymentAsync(
        Guid supplierPayableId,
        decimal amount,
        Guid? bankAccountId,
        string paymentMethod,
        string? referenceNumber,
        string? notes,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateAmount(amount);
        var refNumber = string.IsNullOrWhiteSpace(referenceNumber)
            ? $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..25]
            : referenceNumber.Trim();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_PAYABLE_{supplierPayableId:N}", cancellationToken);

            var payable = await _dbContext.FinSupplierPayables
                .SingleOrDefaultAsync(x => x.Id == supplierPayableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Utang supplier tidak ditemukan.");

            if (payable.Status is FinSupplierPayableStatuses.Paid or FinSupplierPayableStatuses.Cancelled)
                throw new PayableValidationException($"Utang supplier berstatus {payable.Status} tidak dapat dibayar.");

            if (amount > payable.OutstandingAmount)
                throw new PayableValidationException($"Nilai pembayaran Rp {amount:N0} melebihi sisa utang Rp {payable.OutstandingAmount:N0}.");

            var prevOutstanding = payable.OutstandingAmount;
            payable.OutstandingAmount -= amount;
            payable.PaidAmount += amount;
            payable.Status = payable.OutstandingAmount <= 0m ? FinSupplierPayableStatuses.Paid : FinSupplierPayableStatuses.Partial;
            payable.UpdateDateTime = DateTime.UtcNow;
            payable.UpdateBy = actorUserId;
            payable.RowVersion = Guid.NewGuid();

            // Stage event AP_PAYMENT ke Accounting Outbox
            var eventOccurredAt = DateTimeOffset.UtcNow;
            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.ApPayment,
                SourceTransactionId = payable.PayableNumber,
                EventOccurredAt = eventOccurredAt,
                AccountingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = amount,
                CorrelationId = payable.Id,
                CausationId = payable.Id,
                ActorUserId = actorUserId
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("RecordDirectPayment", payable.Id, payable.Id, actorUserId);

            return new SupplierPayablePaymentResponse
            {
                PayableId = payable.Id,
                PayableNumber = payable.PayableNumber,
                SupplierInvoiceNumber = payable.SupplierInvoiceNumber,
                PaymentAmount = amount,
                PreviousOutstanding = prevOutstanding,
                CurrentOutstanding = payable.OutstandingAmount,
                TotalPaid = payable.PaidAmount,
                Status = payable.Status,
                PaymentDate = eventOccurredAt.UtcDateTime,
                ReferenceNumber = refNumber
            };
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

    public async Task<List<SupplierPayableAgingBucketResult>> GetAgingSummaryAsync(DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var date = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var outstandingPayables = await _dbContext.FinSupplierPayables.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status != FinSupplierPayableStatuses.Paid && x.Status != FinSupplierPayableStatuses.Cancelled && x.OutstandingAmount > 0)
            .Select(x => new { x.DueDate, x.OutstandingAmount })
            .ToListAsync(cancellationToken);

        var buckets = new List<SupplierPayableAgingBucketResult>
        {
            new() { BucketLabel = "0-30 Hari", DaysMin = 0, DaysMax = 30 },
            new() { BucketLabel = "31-60 Hari", DaysMin = 31, DaysMax = 60 },
            new() { BucketLabel = "61-90 Hari", DaysMin = 61, DaysMax = 90 },
            new() { BucketLabel = ">90 Hari", DaysMin = 91, DaysMax = null }
        };

        foreach (var p in outstandingPayables)
        {
            var daysPastDue = date.DayNumber - p.DueDate.DayNumber;
            if (daysPastDue < 0) daysPastDue = 0;

            var target = daysPastDue switch
            {
                <= 30 => buckets[0],
                <= 60 => buckets[1],
                <= 90 => buckets[2],
                _ => buckets[3]
            };

            target.Count++;
            target.TotalAmount += p.OutstandingAmount;
        }

        return buckets;
    }

    public async Task<SupplierPayableReportResponse> GetReportSummaryAsync(DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var date = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var payables = await _dbContext.FinSupplierPayables.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);

        var agingBuckets = await GetAgingSummaryAsync(date, cancellationToken);

        var statusBreakdown = payables
            .GroupBy(x => x.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var supplierBreakdown = payables
            .GroupBy(x => x.SupplierId.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(x => x.OutstandingAmount));

        return new SupplierPayableReportResponse
        {
            AsOfDate = date,
            TotalOriginalAmount = payables.Sum(x => x.OriginalAmount),
            TotalPaidAmount = payables.Sum(x => x.PaidAmount),
            TotalAdjustedAmount = payables.Sum(x => x.AdjustedAmount),
            TotalOutstandingAmount = payables.Sum(x => x.OutstandingAmount),
            TotalPayableCount = payables.Count,
            StatusBreakdown = statusBreakdown,
            SupplierBreakdown = supplierBreakdown,
            AgingBuckets = agingBuckets
        };
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
