using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan Batch Tagihan AR (BE-FIN-039, FIN-DEC-048, 054; 02-backend-architecture.md,
/// erd/data-dictionary.md §C.13-C.14). Menggabungkan beberapa FinReceivable milik satu penjamin
/// menjadi satu dokumen tagihan resmi. BUKAN pengganti FinReceivable — status batch murni
/// mengikuti/meringkas status anggotanya (FIN-STATE-1.2 §B.7); FinanceReceivableService TETAP
/// satu-satunya penulis OutstandingAmount. Service ini HANYA MEMBACA FinReceivable, tidak pernah
/// menulis kolomnya.
/// </summary>
public sealed class FinanceReceivableInvoiceBatchService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly BillingCompanyGuarantorInvoiceDocumentService _documentService;

    public FinanceReceivableInvoiceBatchService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        BillingCompanyGuarantorInvoiceDocumentService documentService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _documentService = documentService;
    }

    // ------------------------------------------------------------------------------------
    // Daftar batch — GET /
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<ReceivableInvoiceBatchResponse>> GetPagedAsync(
        ReceivableInvoiceBatchQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinReceivableInvoiceBatches.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DebtorReferenceId.HasValue)
            query = query.Where(x => x.DebtorReferenceId == request.DebtorReferenceId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "batchnumber" => descending ? query.OrderByDescending(x => x.BatchNumber) : query.OrderBy(x => x.BatchNumber),
            "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReceivableInvoiceBatchResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Rincian batch — GET /{id}. Status disegarkan di sini (FIN-STATE-1.2 §B.7: "murni
    // mengikuti/meringkas status anggotanya") sebelum dikembalikan ke pemanggil.
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        await RefreshStatusAsync(batch, cancellationToken);

        var mapped = Map(batch);
        return new ReceivableInvoiceBatchDetailResponse
        {
            Id = mapped.Id,
            BatchNumber = mapped.BatchNumber,
            DebtorType = mapped.DebtorType,
            DebtorReferenceId = mapped.DebtorReferenceId,
            PeriodStart = mapped.PeriodStart,
            PeriodEnd = mapped.PeriodEnd,
            TotalAmount = mapped.TotalAmount,
            Status = mapped.Status,
            IssuedAt = mapped.IssuedAt,
            RowVersion = mapped.RowVersion,
            Members = batch.Items
                .Where(x => !x.IsDelete && x.Receivable is not null)
                .Select(x => new ReceivableInvoiceBatchMemberResponse
                {
                    Id = x.Id,
                    ReceivableId = x.ReceivableId,
                    ReceivableNumber = x.Receivable!.ReceivableNumber,
                    InvoiceId = x.Receivable.InvoiceId,
                    OriginalAmount = x.Receivable.OriginalAmount,
                    OutstandingAmount = x.Receivable.OutstandingAmount,
                    ReceivableStatus = x.Receivable.Status
                })
                .ToList()
        };
    }

    // ------------------------------------------------------------------------------------
    // Piutang yang memenuhi syarat digabung — GET /eligible-receivables. "Memenuhi syarat"
    // ditafsirkan sebagai: penjamin sama, DebtorType PAYER (satu-satunya yang didukung batch,
    // FIN-DEC-048), belum tergabung batch aktif, dan masih punya sisa tagihan (OUTSTANDING/
    // PARTIAL) — piutang yang sudah SETTLED/WRITTEN_OFF/CANCELLED tidak ada gunanya ditagih
    // ulang. Bukan FIN-VAL bernomor; murni definisi teknis "layak digabung".
    // ------------------------------------------------------------------------------------

    public async Task<List<EligibleReceivableResponse>> GetEligibleReceivablesAsync(
        Guid debtorReferenceId, CancellationToken cancellationToken)
    {
        var activeBatchMemberIds = _dbContext.FinReceivableInvoiceBatchItems
            .Where(x => !x.IsDelete && x.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            .Select(x => x.ReceivableId);

        return await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.DebtorType == FinReceivableDebtorTypes.Payer
                && x.DebtorReferenceId == debtorReferenceId
                && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial)
                && !activeBatchMemberIds.Contains(x.Id))
            .OrderBy(x => x.DueDate)
            .Select(x => new EligibleReceivableResponse
            {
                Id = x.Id,
                ReceivableNumber = x.ReceivableNumber,
                InvoiceId = x.InvoiceId,
                OriginalAmount = x.OriginalAmount,
                OutstandingAmount = x.OutstandingAmount,
                DueDate = x.DueDate,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Pembuatan batch (DRAFT) — FIN-VAL-114, FIN-VAL-115, state-transition-matrix.md §B.7
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableInvoiceBatch> CreateAsync(
        DateOnly periodStart, DateOnly periodEnd, IReadOnlyList<Guid> receivableIds,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var distinctIds = receivableIds.Distinct().ToList();
        var receivables = await _dbContext.FinReceivables
            .Where(x => distinctIds.Contains(x.Id) && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var missing = distinctIds.Except(receivables.Select(x => x.Id)).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException($"Piutang tidak ditemukan: {string.Join(", ", missing)}.");

        // Bukan FIN-VAL bernomor — batas skema §C.13: DebtorType batch dipatok PAYER (FIN-DEC-048).
        var nonPayer = receivables.Where(x => x.DebtorType != FinReceivableDebtorTypes.Payer).ToList();
        if (nonPayer.Count > 0)
            throw new ReceivableInvoiceBatchBadRequestException(
                $"Batch Tagihan AR hanya menerima piutang berjenis PAYER. Piutang {nonPayer[0].ReceivableNumber} berjenis {nonPayer[0].DebtorType}.");

        // FIN-VAL-115: seluruh piutang dalam satu batch harus milik penjamin yang sama.
        var debtorReferenceIds = receivables.Select(x => x.DebtorReferenceId).Distinct().ToList();
        if (debtorReferenceIds.Count > 1 || debtorReferenceIds[0] is null)
            throw new ReceivableInvoiceBatchBadRequestException("Seluruh piutang dalam satu batch harus milik penjamin yang sama.");
        var debtorReferenceId = debtorReferenceIds[0]!.Value;

        // FIN-VAL-114: piutang yang sudah tergabung batch aktif tidak boleh digabung lagi.
        var alreadyBatched = await _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
            .Where(x => !x.IsDelete
                && distinctIds.Contains(x.ReceivableId)
                && x.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            .Select(x => x.ReceivableId)
            .ToListAsync(cancellationToken);
        if (alreadyBatched.Count > 0)
            throw new ReceivableInvoiceBatchConflictException(
                $"Piutang {string.Join(", ", alreadyBatched)} sudah tergabung dalam batch tagihan lain.");

        var batch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = GenerateBatchNumber(),
            DebtorType = FinReceivableDebtorTypes.Payer,
            DebtorReferenceId = debtorReferenceId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalAmount = receivables.Sum(x => x.OriginalAmount),
            Status = FinReceivableInvoiceBatchStatuses.Draft,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId
        };

        foreach (var receivable in receivables)
        {
            batch.Items.Add(new FinReceivableInvoiceBatchItem
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                ReceivableId = receivable.Id,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        _dbContext.FinReceivableInvoiceBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Create", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Penerbitan (DRAFT -> ISSUED) — FIN-VAL-116
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableInvoiceBatch> IssueAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT yang dapat diterbitkan. Status saat ini: {batch.Status}.");

        // FIN-VAL-116: batch kosong tidak boleh diterbitkan.
        if (batch.Items.Count(x => !x.IsDelete) == 0)
            throw new ReceivableInvoiceBatchValidationException("Batch tidak memiliki anggota piutang untuk diterbitkan.");

        batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
        batch.IssuedAt = DateTimeOffset.UtcNow;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Issue", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Pembatalan (DRAFT -> CANCELLED) — anggotanya otomatis bebas digabung batch lain karena
    // FIN-VAL-114/eligible-query menyaring berdasarkan Batch.Status <> CANCELLED.
    // ------------------------------------------------------------------------------------

    public async Task<FinReceivableInvoiceBatch> CancelAsync(
        Guid batchId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        EnsureCurrent(batch.RowVersion, expectedRowVersion);

        if (batch.Status != FinReceivableInvoiceBatchStatuses.Draft)
            throw new ReceivableInvoiceBatchValidationException($"Hanya batch berstatus DRAFT yang dapat dibatalkan. Status saat ini: {batch.Status}.");

        batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.CancelDateTime = DateTime.UtcNow;
        batch.CancelBy = actorUserId;
        batch.IsCancel = true;
        batch.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Cancel", batch.Id, actorUserId);
        return batch;
    }

    // ------------------------------------------------------------------------------------
    // Dokumen tagihan gabungan — GET /{id}/document. Memanggil
    // BillingCompanyGuarantorInvoiceDocumentService (FIN-CAP-030) sekali per FinReceivable
    // anggota (lewat InvoiceId-nya), tidak menyalin logikanya.
    // ------------------------------------------------------------------------------------

    public async Task<ReceivableInvoiceBatchDocumentResponse> GetDocumentAsync(
        Guid batchId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches
            .Include(x => x.Items).ThenInclude(x => x.Receivable)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch Tagihan AR tidak ditemukan.");

        var response = new ReceivableInvoiceBatchDocumentResponse
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            DebtorReferenceId = batch.DebtorReferenceId,
            PeriodStart = batch.PeriodStart,
            PeriodEnd = batch.PeriodEnd
        };

        foreach (var item in batch.Items.Where(x => !x.IsDelete && x.Receivable is not null))
        {
            var invoiceDocument = await _documentService.GetDocumentAsync(item.Receivable!.InvoiceId, actorUserId, cancellationToken);
            response.Invoices.Add(invoiceDocument);
            response.Warnings.AddRange(invoiceDocument.Warnings);
        }

        response.GrandTotalCoveredAmount = response.Invoices.Sum(x => x.Totals.TotalCoveredAmount);
        return response;
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// FIN-STATE-1.2 §B.7: "Status FinReceivableInvoiceBatch tidak pernah mengubah status
    /// FinReceivable anggotanya secara langsung. Pelunasan tetap tercatat lewat alokasi
    /// penerimaan pada FinReceivable masing-masing — status batch murni mengikuti/meringkas
    /// status anggotanya." Disegarkan lazy setiap batch dibaca satu-satu (GetByIdAsync),
    /// bukan lewat hook di FinanceReceivableService — nol perubahan pada service itu.
    /// </summary>
    private async Task RefreshStatusAsync(FinReceivableInvoiceBatch batch, CancellationToken cancellationToken)
    {
        if (batch.Status != FinReceivableInvoiceBatchStatuses.Issued
            && batch.Status != FinReceivableInvoiceBatchStatuses.PartiallyPaid)
        {
            return;
        }

        var members = batch.Items.Where(x => !x.IsDelete && x.Receivable is not null).ToList();
        if (members.Count == 0) return;

        var settledCount = members.Count(x => x.Receivable!.Status == FinReceivableStatuses.Settled);
        var newStatus = settledCount == members.Count
            ? FinReceivableInvoiceBatchStatuses.Paid
            : settledCount > 0
                ? FinReceivableInvoiceBatchStatuses.PartiallyPaid
                : FinReceivableInvoiceBatchStatuses.Issued;

        if (newStatus == batch.Status) return;

        batch.Status = newStatus;
        batch.UpdateDateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ReceivableInvoiceBatchResponse Map(FinReceivableInvoiceBatch batch) => new()
    {
        Id = batch.Id,
        BatchNumber = batch.BatchNumber,
        DebtorType = batch.DebtorType,
        DebtorReferenceId = batch.DebtorReferenceId,
        PeriodStart = batch.PeriodStart,
        PeriodEnd = batch.PeriodEnd,
        TotalAmount = batch.TotalAmount,
        Status = batch.Status,
        IssuedAt = batch.IssuedAt,
        RowVersion = batch.RowVersion
    };

    private static string GenerateBatchNumber()
    {
        // KNOWN ISSUE (bersama seluruh generator nomor rumpun ini): belum memakai provider
        // number-series atomik (QBE-CODE-001..006).
        var candidate = $"BAR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static ReceivableInvoiceBatchConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private Task AuditAsync(string action, Guid batchId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceReceivableInvoiceBatch.{action}",
            $"Perubahan Batch Tagihan AR dicatat. BatchId={batchId}",
            new { BatchId = batchId, ActorUserId = actorUserId });
}

public sealed class ReceivableInvoiceBatchBadRequestException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchValidationException(string message) : Exception(message);
public sealed class ReceivableInvoiceBatchConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
