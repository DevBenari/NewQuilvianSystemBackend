using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Satu-satunya penulis FinReceivable.OutstandingAmount (FIN-DES-011, konsekuensi kolom di
/// erd/data-dictionary.md §2.1). Mengurus tiga hal sesuai roadmap BE-FIN-008: umur piutang
/// (aging, FR-FIN-022), pengajuan/keputusan koreksi (FinReceivableAdjustment), dan
/// pengajuan/keputusan penghapusan buku (FinReceivableWriteOff).
///
/// MUST NOT dipakai untuk membuat FinReceivable baru dari fakta Billing — itu tanggung jawab
/// FinanceBillingIntakeService (02-backend-architecture.md, belum ada task pemilik eksplisit,
/// lihat laporan BE-FIN-005 bagian 1). MUST NOT dipakai untuk alokasi penerimaan — itu
/// FinanceReceiptService (BE-FIN-017/018, BLOCKED menunggu owner Billing).
///
/// BE-FIN-011: koreksi/penghapusan yang DISETUJUI menulis kejadian ke FinAccountingEventOutbox
/// lewat FinanceAccountingOutboxService, di dalam transaksi Decide*Async yang sama (FIN-DES-017).
/// </summary>
public sealed class FinanceReceivableService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceReceivableService(ApplicationDbContext dbContext, LoggerService loggerService, FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
    }

    // ------------------------------------------------------------------------------------
    // Baca (BE-FIN-009: daftar, rincian, ringkasan) — ditambahkan saat controller dibangun,
    // tidak mengubah perilaku tulis yang sudah ada dari BE-FIN-008.
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableResponse>> GetPagedAsync(ReceivableQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.DebtorType)) query = query.Where(x => x.DebtorType == request.DebtorType);
        if (request.DueDateFrom.HasValue) query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
        if (request.DueDateTo.HasValue) query = query.Where(x => x.DueDate <= request.DueDateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.ReceivableNumber.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "receivablenumber" => descending ? query.OrderByDescending(x => x.ReceivableNumber) : query.OrderBy(x => x.ReceivableNumber),
            "outstandingamount" => descending ? query.OrderByDescending(x => x.OutstandingAmount) : query.OrderBy(x => x.OutstandingAmount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => Map(x)).ToListAsync(cancellationToken);
        return new PagedResult<ReceivableResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<ReceivableDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .Include(x => x.Items).Include(x => x.Documents).Include(x => x.Adjustments).Include(x => x.WriteOffs)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

        return new ReceivableDetailResponse
        {
            Receivable = Map(receivable),
            Items = receivable.Items.Select(x => new ReceivableItemResponse
            {
                Id = x.Id, EncounterId = x.EncounterId, InvoiceId = x.InvoiceId, Description = x.Description, Amount = x.Amount
            }).ToList(),
            Documents = receivable.Documents.Select(x => new ReceivableDocumentResponse
            {
                Id = x.Id, DocumentType = x.DocumentType, DocumentNumber = x.DocumentNumber, IsReceived = x.IsReceived, ReceivedAt = x.ReceivedAt
            }).ToList(),
            Adjustments = receivable.Adjustments.Select(MapAdjustment).ToList(),
            WriteOffs = receivable.WriteOffs.Select(MapWriteOff).ToList()
        };
    }

    public async Task<ReceivableSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivables.AsNoTracking().Where(x => !x.IsDelete);
        var counts = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new ReceivableSummaryResponse
        {
            TotalReceivable = counts.Sum(c => c.Count),
            OutstandingCount = Count(FinReceivableStatuses.Outstanding),
            PartialCount = Count(FinReceivableStatuses.Partial),
            SettledCount = Count(FinReceivableStatuses.Settled),
            WrittenOffCount = Count(FinReceivableStatuses.WrittenOff),
            TotalOutstandingAmount = await query.SumAsync(x => x.OutstandingAmount, cancellationToken)
        };
    }

    public Task<ReceivableFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ReceivableFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["dueDate", "receivableNumber", "outstandingAmount", "status"],
            StatusOptions =
            [
                FinReceivableStatuses.Outstanding, FinReceivableStatuses.Partial, FinReceivableStatuses.Settled,
                FinReceivableStatuses.WrittenOff, FinReceivableStatuses.Cancelled
            ]
        });

    public static ReceivableResponse Map(FinReceivable x) => new()
    {
        Id = x.Id,
        ReceivableNumber = x.ReceivableNumber,
        InvoiceId = x.InvoiceId,
        DebtorType = x.DebtorType,
        DebtorReferenceId = x.DebtorReferenceId,
        OriginalAmount = x.OriginalAmount,
        OutstandingAmount = x.OutstandingAmount,
        AllocatedAmount = x.AllocatedAmount,
        AdjustedAmount = x.AdjustedAmount,
        WrittenOffAmount = x.WrittenOffAmount,
        DueDate = x.DueDate,
        Status = x.Status,
        ClaimStatus = x.ClaimStatus,
        RecognizedAt = x.RecognizedAt,
        RowVersion = x.RowVersion
    };

    public static ReceivableAdjustmentResponse MapAdjustment(FinReceivableAdjustment x) => new()
    {
        Id = x.Id,
        AdjustmentNumber = x.AdjustmentNumber,
        Direction = x.Direction,
        Amount = x.Amount,
        Reason = x.Reason,
        Status = x.Status,
        RequestedBy = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedBy = x.ApprovedBy,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        RowVersion = x.RowVersion
    };

    public static ReceivableWriteOffResponse MapWriteOff(FinReceivableWriteOff x) => new()
    {
        Id = x.Id,
        WriteOffNumber = x.WriteOffNumber,
        Amount = x.Amount,
        Reason = x.Reason,
        Status = x.Status,
        RequestedBy = x.RequestedBy,
        RequestedAt = x.RequestedAt,
        ApprovedBy = x.ApprovedBy,
        ApprovedAt = x.ApprovedAt,
        RejectionReason = x.RejectionReason,
        RowVersion = x.RowVersion
    };

    // ------------------------------------------------------------------------------------
    // Umur piutang (FR-FIN-022, FIN-DEC-010) — dihitung saat query, tidak disimpan
    // (02-backend-architecture.md §10: menyimpannya berisiko basi).
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Empat kelompok tetap: "0-30", "31-60", "61-90", "di atas 90 hari" (FIN-DEC-010).
    /// Piutang yang belum jatuh tempo (DueDate di masa depan relatif asOfDate) tidak diatur
    /// eksplisit oleh FIN-DEC-010/FR-FIN-022 manapun — diperlakukan sebagai "0-30" (hari
    /// terlambat dianggap nol), bukan keputusan bisnis baru, sekadar nilai aman untuk kasus
    /// yang tidak diatur.
    /// </summary>
    public async Task<List<ReceivableAgingBucketResult>> GetAgingSummaryAsync(DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var referenceDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete && x.OutstandingAmount > 0)
            .Select(x => new { x.DueDate, x.OutstandingAmount })
            .ToListAsync(cancellationToken);

        var buckets = ReceivableAgingBuckets.Labels.ToDictionary(label => label, label => new ReceivableAgingBucketResult { BucketLabel = label });

        foreach (var receivable in receivables)
        {
            var daysPastDue = Math.Max(0, referenceDate.DayNumber - receivable.DueDate.DayNumber);
            var bucket = buckets[ReceivableAgingBuckets.Resolve(daysPastDue)];
            bucket.ReceivableCount++;
            bucket.TotalOutstandingAmount += receivable.OutstandingAmount;
        }

        return ReceivableAgingBuckets.Labels.Select(label => buckets[label]).ToList();
    }

    // ------------------------------------------------------------------------------------
    // Koreksi (FinReceivableAdjustment) — FIN-DES-014, FIN-VAL-020..025
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableAdjustment> RequestAdjustmentAsync(
        Guid receivableId, string direction, decimal amount, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var normalizedDirection = ValidateDirection(direction);
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan koreksi");

        await FindReceivableAsync(receivableId, cancellationToken);

        var adjustment = new FinReceivableAdjustment
        {
            AdjustmentNumber = GenerateNumber("ADJ"),
            ReceivableId = receivableId,
            Direction = normalizedDirection,
            Amount = amount,
            Reason = reasonText,
            Status = FinReceivableApprovalStatuses.Requested,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinReceivableAdjustments.Add(adjustment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Adjustment.Request", adjustment.Id, receivableId, actorUserId);
        return adjustment;
    }

    public Task<FinReceivableAdjustment> ApproveAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: true, rejectionReason: null, cancellationToken);

    public Task<FinReceivableAdjustment> RejectAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideAdjustmentAsync(adjustmentId, expectedRowVersion, actorUserId, approve: false,
            ValidateReason(rejectionReason, "Alasan penolakan"), cancellationToken);

    private async Task<FinReceivableAdjustment> DecideAdjustmentAsync(
        Guid adjustmentId, Guid expectedRowVersion, Guid actorUserId, bool approve, string? rejectionReason, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            var adjustment = await _dbContext.FinReceivableAdjustments
                .SingleOrDefaultAsync(x => x.Id == adjustmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan koreksi piutang tidak ditemukan.");
            EnsureCurrent(adjustment.RowVersion, expectedRowVersion);

            // FIN-VAL-025: keputusan hanya boleh sekali.
            if (adjustment.Status != FinReceivableApprovalStatuses.Requested)
                throw new ReceivableValidationException("Permohonan ini sudah diputuskan sebelumnya.");
            // FIN-VAL-020 / FIN-DES-014: pengaju tidak boleh menyetujui/menolak permohonannya sendiri.
            if (actorUserId == adjustment.RequestedBy)
                throw new ReceivableValidationException(
                    "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang.");

            await AcquireLockAsync($"FIN_RECEIVABLE_{adjustment.ReceivableId:N}", cancellationToken);
            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == adjustment.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            if (approve)
            {
                // FIN-VAL-024: koreksi pengurang (CREDIT) tidak boleh melebihi sisa piutang.
                if (adjustment.Direction == FinReceivableAdjustmentDirections.Credit && adjustment.Amount > receivable.OutstandingAmount)
                    throw new ReceivableValidationException(
                        $"Nilai koreksi melebihi sisa piutang. Sisa saat ini Rp {receivable.OutstandingAmount:N0}.");

                // Invariant (FIN-VAL-011): OriginalAmount = Outstanding + Allocated + Adjusted + WrittenOff.
                // CREDIT mengurangi sisa dan menambah AdjustedAmount; DEBIT sebaliknya — status
                // FinReceivable TIDAK berubah oleh koreksi (state-transition-matrix.md §2 hanya
                // memetakan perubahan status dari alokasi, write-off, dan pembatalan).
                if (adjustment.Direction == FinReceivableAdjustmentDirections.Credit)
                {
                    receivable.OutstandingAmount -= adjustment.Amount;
                    receivable.AdjustedAmount += adjustment.Amount;
                }
                else
                {
                    receivable.OutstandingAmount += adjustment.Amount;
                    receivable.AdjustedAmount -= adjustment.Amount;
                }
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                adjustment.Status = FinReceivableApprovalStatuses.Approved;
            }
            else
            {
                adjustment.Status = FinReceivableApprovalStatuses.Rejected;
                adjustment.RejectionReason = rejectionReason;
            }
            adjustment.ApprovedBy = actorUserId;
            adjustment.ApprovedAt = DateTimeOffset.UtcNow;
            adjustment.UpdateDateTime = DateTime.UtcNow;
            adjustment.UpdateBy = actorUserId;
            adjustment.RowVersion = Guid.NewGuid();

            // BE-FIN-011: kejadian hanya lahir dari koreksi yang DISETUJUI (katalog 17 kode,
            // "Koreksi piutang disetujui") — penolakan tidak mengubah apa pun secara finansial.
            if (approve)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PenyesuaianPiutang,
                    SourceTransactionId = receivable.ReceivableNumber,
                    EventOccurredAt = adjustment.ApprovedAt!.Value,
                    AccountingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = adjustment.Amount,
                    CorrelationId = receivable.CorrelationId,
                    CausationId = adjustment.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(approve ? "Adjustment.Approve" : "Adjustment.Reject", adjustment.Id, adjustment.ReceivableId, actorUserId);
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
    // Penghapusan buku (FinReceivableWriteOff) — FIN-VAL-014, FIN-DES-014
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableWriteOff> RequestWriteOffAsync(
        Guid receivableId, decimal amount, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        ValidateAmount(amount);
        var reasonText = ValidateReason(reason, "Alasan penghapusan");

        var receivable = await FindReceivableAsync(receivableId, cancellationToken);
        // FIN-VAL-014: piutang lunas tidak boleh dihapusbukukan (diperiksa pada permintaan,
        // sesuai endpoint yang dirujuk kontrak: POST /receivables/{id}/write-offs).
        if (receivable.Status == FinReceivableStatuses.Settled)
            throw new ReceivableValidationException("Piutang yang sudah lunas tidak dapat dihapusbukukan.");

        var writeOff = new FinReceivableWriteOff
        {
            WriteOffNumber = GenerateNumber("WO"),
            ReceivableId = receivableId,
            Amount = amount,
            Reason = reasonText,
            Status = FinReceivableApprovalStatuses.Requested,
            RequestedBy = actorUserId,
            RequestedAt = DateTimeOffset.UtcNow,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinReceivableWriteOffs.Add(writeOff);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("WriteOff.Request", writeOff.Id, receivableId, actorUserId);
        return writeOff;
    }

    public Task<FinReceivableWriteOff> ApproveWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideWriteOffAsync(writeOffId, expectedRowVersion, actorUserId, approve: true, rejectionReason: null, cancellationToken);

    public Task<FinReceivableWriteOff> RejectWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, string rejectionReason, Guid actorUserId, CancellationToken cancellationToken) =>
        DecideWriteOffAsync(writeOffId, expectedRowVersion, actorUserId, approve: false,
            ValidateReason(rejectionReason, "Alasan penolakan"), cancellationToken);

    private async Task<FinReceivableWriteOff> DecideWriteOffAsync(
        Guid writeOffId, Guid expectedRowVersion, Guid actorUserId, bool approve, string? rejectionReason, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            var writeOff = await _dbContext.FinReceivableWriteOffs
                .SingleOrDefaultAsync(x => x.Id == writeOffId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan penghapusan piutang tidak ditemukan.");
            EnsureCurrent(writeOff.RowVersion, expectedRowVersion);

            if (writeOff.Status != FinReceivableApprovalStatuses.Requested)
                throw new ReceivableValidationException("Permohonan ini sudah diputuskan sebelumnya.");
            if (actorUserId == writeOff.RequestedBy)
                throw new ReceivableValidationException(
                    "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang.");

            await AcquireLockAsync($"FIN_RECEIVABLE_{writeOff.ReceivableId:N}", cancellationToken);
            var receivable = await _dbContext.FinReceivables
                .SingleOrDefaultAsync(x => x.Id == writeOff.ReceivableId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

            if (approve)
            {
                if (receivable.Status == FinReceivableStatuses.Settled)
                    throw new ReceivableValidationException("Piutang yang sudah lunas tidak dapat dihapusbukukan.");
                if (writeOff.Amount > receivable.OutstandingAmount)
                    throw new ReceivableValidationException(
                        $"Nilai penghapusan melebihi sisa piutang. Sisa saat ini Rp {receivable.OutstandingAmount:N0}.");

                // state-transition-matrix.md §2: OUTSTANDING atau PARTIAL -> WRITTEN_OFF pada setiap
                // penghapusan yang disetujui, penuh maupun sebagian — tidak bersyarat sisa jadi nol.
                // Dicatat apa adanya sesuai kontrak terkunci; lihat laporan BE-FIN-008 untuk catatan
                // bahwa ini terasa ganjil untuk penghapusan sebagian dan perlu diratifikasi ulang.
                receivable.OutstandingAmount -= writeOff.Amount;
                receivable.WrittenOffAmount += writeOff.Amount;
                receivable.Status = FinReceivableStatuses.WrittenOff;
                receivable.UpdateDateTime = DateTime.UtcNow;
                receivable.UpdateBy = actorUserId;
                receivable.RowVersion = Guid.NewGuid();

                writeOff.Status = FinReceivableApprovalStatuses.Approved;
            }
            else
            {
                writeOff.Status = FinReceivableApprovalStatuses.Rejected;
                writeOff.RejectionReason = rejectionReason;
            }
            writeOff.ApprovedBy = actorUserId;
            writeOff.ApprovedAt = DateTimeOffset.UtcNow;
            writeOff.UpdateDateTime = DateTime.UtcNow;
            writeOff.UpdateBy = actorUserId;
            writeOff.RowVersion = Guid.NewGuid();

            // BE-FIN-011: kejadian hanya lahir dari penghapusan yang DISETUJUI (katalog 17 kode,
            // "Penghapusan piutang disetujui") — penolakan tidak mengubah apa pun secara finansial.
            if (approve)
            {
                await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang,
                    SourceTransactionId = receivable.ReceivableNumber,
                    EventOccurredAt = writeOff.ApprovedAt!.Value,
                    AccountingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Amount = writeOff.Amount,
                    CorrelationId = receivable.CorrelationId,
                    CausationId = writeOff.Id,
                    ActorUserId = actorUserId
                }, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(approve ? "WriteOff.Approve" : "WriteOff.Reject", writeOff.Id, writeOff.ReceivableId, actorUserId);
            return writeOff;
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
    // Infrastruktur bersama
    // ------------------------------------------------------------------------------------

    private async Task<FinReceivable> FindReceivableAsync(Guid receivableId, CancellationToken cancellationToken) =>
        await _dbContext.FinReceivables.AsNoTracking().SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Piutang tidak ditemukan.");

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

    private static ReceivableConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static string ValidateDirection(string? direction)
    {
        var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinReceivableAdjustmentDirections.Debit or FinReceivableAdjustmentDirections.Credit))
            throw new ReceivableBadRequestException("Arah koreksi harus DEBIT atau CREDIT.");
        return normalized;
    }

    // FIN-VAL-023 (dan analoginya untuk write-off): nominal harus lebih dari nol.
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0) throw new ReceivableBadRequestException("Nominal harus lebih dari nol.");
    }

    // FIN-VAL-021/022: alasan wajib diisi.
    private static string ValidateReason(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ReceivableBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > 500) throw new ReceivableBadRequestException($"{label} maksimal 500 karakter.");
        return trimmed;
    }

    // Nomor koreksi/penghapusan tidak punya format baku pada kontrak yang terkunci (berbeda
    // dari ReceivableNumber yang punya contoh "AR-2026-09-00871"); dibuat unik lewat Guid,
    // bukan Count/Max/Last+1 (QBE-CODE-002/003), sampai ada keputusan skema penomoran resmi.
    private static string GenerateNumber(string prefix)
    {
        var candidate = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    // Privasi: Reason/RejectionReason bisa memuat keterangan bebas dan MUST NOT masuk log
    // aplikasi, mengikuti pola PettyCashVoucherService.
    private Task AuditAsync(string action, Guid entityId, Guid receivableId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceReceivable.{action}",
            $"Perubahan piutang dicatat. EntityId={entityId} ReceivableId={receivableId}",
            new { EntityId = entityId, ReceivableId = receivableId, ActorUserId = actorUserId });
}

/// <summary>Nama kelompok umur tetap sesuai FIN-DEC-010 — bukan enum karena dipakai sebagai label tampilan langsung.</summary>
public static class ReceivableAgingBuckets
{
    public const string Days0To30 = "0-30";
    public const string Days31To60 = "31-60";
    public const string Days61To90 = "61-90";
    public const string Over90Days = "di atas 90 hari";

    public static readonly string[] Labels = [Days0To30, Days31To60, Days61To90, Over90Days];

    public static string Resolve(int daysPastDue) => daysPastDue switch
    {
        <= 30 => Days0To30,
        <= 60 => Days31To60,
        <= 90 => Days61To90,
        _ => Over90Days
    };
}

public sealed class ReceivableAgingBucketResult
{
    public string BucketLabel { get; set; } = string.Empty;
    public int ReceivableCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public sealed class ReceivableBadRequestException(string message) : Exception(message);
public sealed class ReceivableValidationException(string message) : Exception(message);
public sealed class ReceivableConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
