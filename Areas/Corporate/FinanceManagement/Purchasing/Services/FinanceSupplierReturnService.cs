using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;

/// <summary>
/// Layanan Retur Pembelian dan Deposit Retur (BE-FIN-035, FIN-DEC-047, 061;
/// 02-backend-architecture.md §C.1-C.2, D.1-D.2). Retur TIDAK mengubah FinSupplierPayable
/// apa pun secara langsung — efeknya ke utang lewat pemakaian Deposit Retur di dalam FinPayment
/// (BE-FIN-036, ReserveAsync/ReleaseAsync/MarkAppliedAsync ditambahkan pada layanan ini oleh
/// task itu, BUKAN task ini). AvailableAmount deposit HANYA ditulis service ini (FIN-DES-046).
/// Pola maker-checker/transaksi meniru FinancePurchasingInvoiceService (BE-FIN-034) persis.
/// </summary>
public sealed class FinanceSupplierReturnService
{
    private const string LogCategory = "Corporate.FinanceManagement.Purchasing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceAccountingOutboxService _accountingOutboxService;

    public FinanceSupplierReturnService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceAccountingOutboxService accountingOutboxService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _accountingOutboxService = accountingOutboxService;
    }

    public async Task<FinSupplierReturn?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.FinSupplierReturns
            .Include(x => x.Items)
            .Include(x => x.Deposit)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

    // ------------------------------------------------------------------------------------
    // Pencatatan (DRAFT) — FIN-VAL-110, FIN-VAL-111, state-transition-matrix.md §B.5
    // ------------------------------------------------------------------------------------

    public async Task<FinSupplierReturn> CreateAsync(
        Guid purchasingInvoiceId,
        string reason,
        IReadOnlyList<SupplierReturnItemRequestDto> items,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var purchasingInvoice = await _dbContext.FinPurchasingInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == purchasingInvoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Purchasing Invoice tidak ditemukan.");

        // FIN-VAL-110: retur hanya atas invoice yang sudah disetujui.
        if (purchasingInvoice.Status != FinPurchasingInvoiceStatuses.Approved)
            throw new PurchasingValidationException("Retur hanya dapat diajukan atas Purchasing Invoice yang sudah disetujui.");

        var validatedReason = ValidateText(reason, "Alasan retur", 500);

        var (returnItems, totalAmount) = BuildItems(items, actorUserId);

        // state-transition-matrix.md §B.5: minimal satu baris item — 400, bukan 422 (pola sama
        // dengan FIN-VAL-100 milik Purchase Order, BE-FIN-032).
        if (returnItems.Count == 0)
            throw new PurchasingBadRequestException("Retur Pembelian wajib memiliki minimal satu baris item.");

        // Bukan FIN-VAL bernomor — jaminan teknis supaya ConfirmAsync selalu dapat menerbitkan
        // outbox RETUR-PEMBELIAN (FinanceAccountingOutboxService menolak Amount <= 0 tanpa
        // pengecualian, beda dari PPNAmount yang boleh nol pada BE-FIN-034).
        if (totalAmount <= 0)
            throw new PurchasingBadRequestException("Nilai retur harus lebih dari nol.");

        // FIN-VAL-111: nilai retur tidak boleh melebihi nilai invoice sumber. Dibandingkan
        // terhadap TotalAmount invoice saja (kontrak tidak mensyaratkan akumulasi lintas retur
        // lain atas invoice yang sama) — dicatat sebagai risiko pada laporan task.
        if (Math.Round(totalAmount, 2) > Math.Round(purchasingInvoice.TotalAmount, 2))
            throw new PurchasingValidationException("Nilai retur melebihi nilai Purchasing Invoice sumber.");

        var supplierReturn = new FinSupplierReturn
        {
            Id = Guid.NewGuid(),
            ReturnNumber = GenerateReturnNumber(),
            PurchasingInvoiceId = purchasingInvoiceId,
            Reason = validatedReason,
            TotalAmount = totalAmount,
            Status = FinSupplierReturnStatuses.Draft,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            UpdateBy = actorUserId,
            DeleteBy = actorUserId,
            CancelBy = actorUserId,
            Items = returnItems
        };
        foreach (var item in returnItems) item.SupplierReturnId = supplierReturn.Id;

        _dbContext.FinSupplierReturns.Add(supplierReturn);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Create", supplierReturn.Id, actorUserId);
        return supplierReturn;
    }

    // ------------------------------------------------------------------------------------
    // Konfirmasi (DRAFT -> CONFIRMED) — state-transition-matrix.md §B.5. SATU TRANSAKSI:
    // menerbitkan FinSupplierReturnDeposit AVAILABLE sebesar TotalAmount + outbox
    // RETUR-PEMBELIAN (FIN-DES-047, integration-contract.md §5.9 kode 28). Gagal di mana pun ->
    // status TETAP DRAFT (rollback penuh).
    // ------------------------------------------------------------------------------------

    public async Task<FinSupplierReturn> ConfirmAsync(
        Guid supplierReturnId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_SUPPLIER_RETURN_{supplierReturnId:N}", cancellationToken);

            var supplierReturn = await _dbContext.FinSupplierReturns
                .SingleOrDefaultAsync(x => x.Id == supplierReturnId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Retur Pembelian tidak ditemukan.");

            EnsureCurrent(supplierReturn.RowVersion, expectedRowVersion);

            if (supplierReturn.Status != FinSupplierReturnStatuses.Draft)
                throw new PurchasingValidationException($"Hanya Retur Pembelian berstatus DRAFT yang dapat dikonfirmasi. Status saat ini: {supplierReturn.Status}.");

            var purchasingInvoice = await _dbContext.FinPurchasingInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == supplierReturn.PurchasingInvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Purchasing Invoice sumber tidak ditemukan.");

            var deposit = new FinSupplierReturnDeposit
            {
                Id = Guid.NewGuid(),
                SupplierId = purchasingInvoice.SupplierId,
                SourceReturnId = supplierReturn.Id,
                OriginalAmount = supplierReturn.TotalAmount,
                AvailableAmount = supplierReturn.TotalAmount,
                Status = FinSupplierReturnDepositStatuses.Available,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            };
            _dbContext.FinSupplierReturnDeposits.Add(deposit);

            await _accountingOutboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.ReturPembelian,
                SourceTransactionId = supplierReturn.ReturnNumber,
                EventOccurredAt = DateTimeOffset.UtcNow,
                AccountingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = supplierReturn.TotalAmount,
                CorrelationId = supplierReturn.Id,
                CausationId = deposit.Id,
                ActorUserId = actorUserId
            }, cancellationToken);

            supplierReturn.Status = FinSupplierReturnStatuses.Confirmed;
            supplierReturn.UpdateDateTime = DateTime.UtcNow;
            supplierReturn.UpdateBy = actorUserId;
            supplierReturn.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Confirm", supplierReturn.Id, actorUserId, new { DepositId = deposit.Id });
            return supplierReturn;
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
    // Pembatalan (DRAFT -> CANCELLED) — state-transition-matrix.md §B.5. CONFIRMED adalah
    // status akhir; retur yang sudah menerbitkan deposit tidak dapat dibatalkan lewat jalur ini.
    // ------------------------------------------------------------------------------------

    public async Task<FinSupplierReturn> CancelAsync(
        Guid supplierReturnId, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var supplierReturn = await _dbContext.FinSupplierReturns
            .SingleOrDefaultAsync(x => x.Id == supplierReturnId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Retur Pembelian tidak ditemukan.");

        EnsureCurrent(supplierReturn.RowVersion, expectedRowVersion);

        if (supplierReturn.Status != FinSupplierReturnStatuses.Draft)
            throw new PurchasingValidationException($"Hanya Retur Pembelian berstatus DRAFT yang dapat dibatalkan. Status saat ini: {supplierReturn.Status}.");

        supplierReturn.Status = FinSupplierReturnStatuses.Cancelled;
        supplierReturn.UpdateDateTime = DateTime.UtcNow;
        supplierReturn.UpdateBy = actorUserId;
        supplierReturn.CancelDateTime = DateTime.UtcNow;
        supplierReturn.CancelBy = actorUserId;
        supplierReturn.IsCancel = true;
        supplierReturn.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw Stale(ex);
        }

        await AuditAsync("Cancel", supplierReturn.Id, actorUserId);
        return supplierReturn;
    }

    // ------------------------------------------------------------------------------------
    // Daftar Deposit Retur — GET /deposits, disaring per supplier (api-contract.md §B.5)
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<FinSupplierReturnDeposit>> GetDepositsPagedAsync(
        SupplierReturnDepositQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinSupplierReturnDeposits.AsNoTracking().Where(x => !x.IsDelete);

        if (query.SupplierId.HasValue && query.SupplierId.Value != Guid.Empty)
            q = q.Where(x => x.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q.OrderByDescending(x => x.CreateDateTime)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FinSupplierReturnDeposit>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    // ------------------------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------------------------

    private static (List<FinSupplierReturnItem> Items, decimal TotalAmount) BuildItems(
        IReadOnlyList<SupplierReturnItemRequestDto> items, Guid actorUserId)
    {
        var returnItems = new List<FinSupplierReturnItem>();
        decimal totalAmount = 0m;

        foreach (var item in items ?? [])
        {
            var description = ValidateText(item.Description, "Deskripsi baris retur", 300);
            if (item.Quantity <= 0)
                throw new PurchasingBadRequestException("Kuantitas baris retur harus lebih dari nol.");
            if (item.UnitPrice < 0)
                throw new PurchasingBadRequestException("Harga satuan baris retur tidak boleh negatif.");

            var lineTotal = item.Quantity * item.UnitPrice;
            totalAmount += lineTotal;

            returnItems.Add(new FinSupplierReturnItem
            {
                Id = Guid.NewGuid(),
                Description = description,
                Quantity = item.Quantity,
                LineTotal = lineTotal,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                DeleteBy = actorUserId,
                CancelBy = actorUserId
            });
        }

        return (returnItems, totalAmount);
    }

    private static string ValidateText(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PurchasingBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new PurchasingBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string GenerateReturnNumber()
    {
        // KNOWN ISSUE (bersama BE-FIN-029/030/032/033/034): belum memakai provider number-series
        // atomik (QBE-CODE-001..006).
        var candidate = $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static PurchasingConflictException Stale(Exception? inner = null) =>
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

    private Task AuditAsync(string action, Guid supplierReturnId, Guid actorUserId, object? extra = null) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceSupplierReturn.{action}",
            $"Perubahan Retur Pembelian dicatat. SupplierReturnId={supplierReturnId}",
            extra ?? new { SupplierReturnId = supplierReturnId, ActorUserId = actorUserId });
}
