using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan kalkulasi snapshot saldo subledger bulanan untuk 4 akun kontrol Finance (BE-FIN-049, FIN-DEC-090).
/// Mengagregasi saldo Kas Kasir, Kas Kecil, Piutang, dan Utang Supplier per akhir bulan,
/// serta menerbitkan tepat 4 kejadian SALDO-SUBLEDGER ke FinAccountingEventOutbox (termasuk yang bernilai 0.00).
/// Membuka transaksi database eksplisit (Serializable) dan memanggil SaveChangesAsync sesudah pementasan outbox.
/// </summary>
public sealed class FinanceSubledgerSnapshotService
{
    private static readonly Regex AccountingPeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceAccountingOutboxService _outboxService;

    public FinanceSubledgerSnapshotService(
        ApplicationDbContext dbContext,
        FinanceAccountingOutboxService outboxService)
    {
        _dbContext = dbContext;
        _outboxService = outboxService;
    }

    /// <summary>
    /// Menghitung posisi saldo 4 akun kontrol dan menerbitkan 4 pesan SALDO-SUBLEDGER ke outbox dalam satu transaksi serializable.
    /// </summary>
    public async Task<GenerateSubledgerSnapshotsResponse> GenerateMonthlySnapshotsAsync(
        GenerateSubledgerSnapshotsRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.AccountingPeriodCode) || !AccountingPeriodRegex.IsMatch(request.AccountingPeriodCode))
        {
            throw new FinanceSubledgerSnapshotValidationException("Kode periode harus berbentuk YYYY-MM, contoh: 2026-09.");
        }

        var periodParts = request.AccountingPeriodCode.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);
        var periodEndDate = new DateOnly(periodYear, periodMonth, DateTime.DaysInMonth(periodYear, periodMonth));

        var cashierCode = !string.IsNullOrWhiteSpace(request.CashierControlAccountCode)
            ? request.CashierControlAccountCode.Trim()
            : SubledgerControlAccountDefaults.CashierCash;

        var pettyCashCode = !string.IsNullOrWhiteSpace(request.PettyCashControlAccountCode)
            ? request.PettyCashControlAccountCode.Trim()
            : SubledgerControlAccountDefaults.PettyCash;

        var receivableCode = !string.IsNullOrWhiteSpace(request.ReceivableControlAccountCode)
            ? request.ReceivableControlAccountCode.Trim()
            : SubledgerControlAccountDefaults.Receivables;

        var payableCode = !string.IsNullOrWhiteSpace(request.PayableControlAccountCode)
            ? request.PayableControlAccountCode.Trim()
            : SubledgerControlAccountDefaults.SupplierPayables;

        if (cashierCode.Length > 50 || pettyCashCode.Length > 50 || receivableCode.Length > 50 || payableCode.Length > 50)
        {
            throw new FinanceSubledgerSnapshotValidationException("Kode akun kontrol melebihi panjang maksimal 50 karakter.");
        }

        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_SUBLEDGER_SNAPSHOT_{request.AccountingPeriodCode}", cancellationToken);

            // 1. Kas Kasir (KAS-KASIR)
            // Closing balance kas harian terakhir pada atau sebelum tanggal akhir periode yang sudah berstatus CLOSED
            var lastClosedCashSnapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
                .Where(x => !x.IsDelete && x.CashDate <= periodEndDate && x.Status == FinDailyCashSnapshotStatuses.Closed)
                .OrderByDescending(x => x.CashDate)
                .FirstOrDefaultAsync(cancellationToken);

            var cashierBalance = lastClosedCashSnapshot != null ? Math.Max(0m, lastClosedCashSnapshot.ClosingBalance) : 0.00m;

            // 2. Kas Kecil (KAS-KECIL)
            // Total saldo berjalan kas kecil aktif yang dimulai pada atau sebelum tanggal akhir periode
            var pettyCashBalance = await _dbContext.FinPettyCashBudgets.AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == PettyCashBudgetStatuses.Active && x.PeriodStart <= periodEndDate)
                .SumAsync(x => (decimal?)x.CurrentBalance, cancellationToken) ?? 0.00m;

            pettyCashBalance = Math.Max(0m, pettyCashBalance);

            // 3. Piutang Usaha / Pasien & Penjamin (PIUTANG)
            // Total sisa piutang penjamin dan pasien (OutstandingAmount) yang diakui sampai akhir periode
            var endOfPeriodUtc = new DateTimeOffset(periodEndDate.Year, periodEndDate.Month, periodEndDate.Day, 23, 59, 59, 999, TimeSpan.Zero);
            var receivableBalance = await _dbContext.FinReceivables.AsNoTracking()
                .Where(x => !x.IsDelete &&
                            (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial) &&
                            x.RecognizedAt <= endOfPeriodUtc)
                .SumAsync(x => (decimal?)x.OutstandingAmount, cancellationToken) ?? 0.00m;

            receivableBalance = Math.Max(0m, receivableBalance);

            // 4. Utang Usaha / Supplier (UTANG-SUPPLIER)
            // Total sisa tagihan utang supplier (OutstandingAmount) sampai akhir periode (nilai positif sesuai ACC-DEC-109)
            var payableBalance = await _dbContext.FinSupplierPayables.AsNoTracking()
                .Where(x => !x.IsDelete &&
                            (x.Status == FinSupplierPayableStatuses.Outstanding || x.Status == FinSupplierPayableStatuses.Partial) &&
                            x.SupplierInvoiceDate <= periodEndDate)
                .SumAsync(x => (decimal?)x.OutstandingAmount, cancellationToken) ?? 0.00m;

            payableBalance = Math.Max(0m, payableBalance);

            var targetAccounts = new[]
            {
                new { Category = SubledgerAccountCategories.CashierCash, Name = "Kas Kasir", Code = cashierCode, Amount = cashierBalance },
                new { Category = SubledgerAccountCategories.PettyCash, Name = "Kas Kecil", Code = pettyCashCode, Amount = pettyCashBalance },
                new { Category = SubledgerAccountCategories.Receivables, Name = "Piutang Usaha / Pasien & Penjamin", Code = receivableCode, Amount = receivableBalance },
                new { Category = SubledgerAccountCategories.SupplierPayables, Name = "Utang Usaha / Supplier", Code = payableCode, Amount = payableBalance }
            };

            var stagedItems = new List<SubledgerAccountSnapshotItemResponse>();

            foreach (var account in targetAccounts)
            {
                var outboxRequest = new AccountingOutboxEventRequest
                {
                    EventTypeCode = FinAccountingEventTypeCodes.SaldoSubledger,
                    SourceTransactionId = $"SUBLEDGER-{request.AccountingPeriodCode}-{account.Code}",
                    EventOccurredAt = DateTimeOffset.UtcNow,
                    AccountingDate = periodEndDate,
                    Amount = account.Amount,
                    CorrelationId = correlationId,
                    CausationId = causationId,
                    ActorUserId = actorUserId,
                    SubledgerBalance = new SubledgerBalanceRequest
                    {
                        AccountingPeriodCode = request.AccountingPeriodCode,
                        ControlAccountCode = account.Code
                    }
                };

                var stagedEvent = await _outboxService.StageEventAsync(outboxRequest, cancellationToken);

                stagedItems.Add(new SubledgerAccountSnapshotItemResponse
                {
                    AccountCategory = account.Category,
                    AccountName = account.Name,
                    ControlAccountCode = account.Code,
                    Amount = account.Amount,
                    EventNumber = stagedEvent.EventNumber,
                    OutboxEventId = stagedEvent.Id,
                    SourceTransactionId = stagedEvent.SourceTransactionId,
                    SourceVersion = stagedEvent.SourceVersion,
                    AccountingDate = stagedEvent.AccountingDate,
                    DeliveryStatus = stagedEvent.DeliveryStatus,
                    EventOccurredAt = stagedEvent.EventOccurredAt
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            return new GenerateSubledgerSnapshotsResponse
            {
                AccountingPeriodCode = request.AccountingPeriodCode,
                AccountingDate = periodEndDate,
                TotalAccounts = stagedItems.Count,
                TotalBalance = stagedItems.Sum(x => x.Amount),
                GeneratedAt = DateTimeOffset.UtcNow,
                Items = stagedItems,
                Message = $"Snapshot saldo subledger untuk periode {request.AccountingPeriodCode} berhasil dikalkulasi dan {stagedItems.Count} kejadian SALDO-SUBLEDGER berhasil diterbitkan ke kotak keluar."
            };
        }
        catch (Exception)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Mengambil rincian snapshot saldo subledger per periode yang tercatat di FinAccountingEventOutbox.
    /// </summary>
    public async Task<SubledgerPeriodSnapshotsResponse> GetSnapshotsByPeriodAsync(
        string accountingPeriodCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountingPeriodCode) || !AccountingPeriodRegex.IsMatch(accountingPeriodCode))
        {
            throw new FinanceSubledgerSnapshotValidationException("Kode periode harus berbentuk YYYY-MM, contoh: 2026-09.");
        }

        var periodParts = accountingPeriodCode.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);
        var periodEndDate = new DateOnly(periodYear, periodMonth, DateTime.DaysInMonth(periodYear, periodMonth));

        var prefix = $"SUBLEDGER-{accountingPeriodCode}-";

        var events = await _dbContext.Set<FinAccountingEventOutbox>().AsNoTracking()
            .Where(x => !x.IsDelete &&
                        x.EventTypeCode == FinAccountingEventTypeCodes.SaldoSubledger &&
                        x.AccountingDate == periodEndDate &&
                        x.SourceTransactionId.StartsWith(prefix))
            .OrderBy(x => x.SourceTransactionId)
            .ThenByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        // Ambil versi terbaru per SourceTransactionId
        var latestPerSource = events
            .GroupBy(x => x.SourceTransactionId)
            .Select(g => g.First())
            .ToList();

        var items = new List<SubledgerAccountSnapshotItemResponse>();
        foreach (var ev in latestPerSource)
        {
            var controlAccountCode = ev.SourceTransactionId.Length > prefix.Length
                ? ev.SourceTransactionId[prefix.Length..]
                : string.Empty;

            var (category, name) = ResolveAccountInfo(controlAccountCode);

            items.Add(new SubledgerAccountSnapshotItemResponse
            {
                AccountCategory = category,
                AccountName = name,
                ControlAccountCode = controlAccountCode,
                Amount = ev.Amount,
                EventNumber = ev.EventNumber,
                OutboxEventId = ev.Id,
                SourceTransactionId = ev.SourceTransactionId,
                SourceVersion = ev.SourceVersion,
                AccountingDate = ev.AccountingDate,
                DeliveryStatus = ev.DeliveryStatus,
                EventOccurredAt = ev.EventOccurredAt
            });
        }

        return new SubledgerPeriodSnapshotsResponse
        {
            AccountingPeriodCode = accountingPeriodCode,
            AccountingDate = periodEndDate,
            IsComplete = items.Count >= 4,
            TotalBalance = items.Sum(x => x.Amount),
            Items = items
        };
    }

    private static (string Category, string Name) ResolveAccountInfo(string controlAccountCode) =>
        controlAccountCode switch
        {
            SubledgerControlAccountDefaults.CashierCash => (SubledgerAccountCategories.CashierCash, "Kas Kasir"),
            SubledgerControlAccountDefaults.PettyCash => (SubledgerAccountCategories.PettyCash, "Kas Kecil"),
            SubledgerControlAccountDefaults.Receivables => (SubledgerAccountCategories.Receivables, "Piutang Usaha / Pasien & Penjamin"),
            SubledgerControlAccountDefaults.SupplierPayables => (SubledgerAccountCategories.SupplierPayables, "Utang Usaha / Supplier"),
            _ => ("UNKNOWN", $"Akun Kontrol ({controlAccountCode})")
        };

    // ------------------------------------------------------------------------------------
    // Concurrency & Database Helpers
    // ------------------------------------------------------------------------------------

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
}

public sealed class FinanceSubledgerSnapshotValidationException(string message) : Exception(message);
