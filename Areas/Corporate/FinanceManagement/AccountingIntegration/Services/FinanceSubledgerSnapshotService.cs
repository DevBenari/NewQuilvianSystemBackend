using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan kalkulasi dan penerbitan snapshot saldo subledger bulanan (BE-FIN-049, BE-FIN-068, FIN-DEC-112..114, FIN-DEC-122, FIN-DES-080).
/// Mengagregasi saldo Kas Kasir, Kas Kecil, Piutang, Utang Supplier, dan Utang Jasa Medis per akhir bulan,
/// serta menerbitkan kejadian SALDO-SUBLEDGER ke FinAccountingEventOutbox untuk setiap akun control aktif.
/// Prinsip: Sumber angka murni dari FinanceSubledgerBalanceCalculator, gagal tertutup bila pemetaan tidak lengkap,
/// nilai negatif dikirim apa adanya, dan jumlah baris mengikuti pemetaan aktif.
/// </summary>
public sealed class FinanceSubledgerSnapshotService
{
    private static readonly Regex AccountingPeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceAccountingOutboxService _outboxService;
    private readonly FinanceSubledgerBalanceCalculator _balanceCalculator;
    private readonly FinanceSubledgerControlAccountService _controlAccountService;

    public FinanceSubledgerSnapshotService(
        ApplicationDbContext dbContext,
        FinanceAccountingOutboxService outboxService,
        FinanceSubledgerBalanceCalculator balanceCalculator,
        FinanceSubledgerControlAccountService controlAccountService)
    {
        _dbContext = dbContext;
        _outboxService = outboxService;
        _balanceCalculator = balanceCalculator;
        _controlAccountService = controlAccountService;
    }

    /// <summary>
    /// Menghitung posisi saldo subledger dan menerbitkan kejadian SALDO-SUBLEDGER ke outbox dalam satu transaksi serializable (FIN-DEC-112..114).
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
        var periodEndDate = FinanceBusinessDate.GetPeriodEndDate(periodYear, periodMonth);

        // 1. Audit Cakupan Pemetaan Akun Control (FIN-DES-080, FIN-VAL-177, FIN-VAL-178 - Gagal Tertutup / Fail-Closed)
        var coverage = await _controlAccountService.GetCoverageAsync(cancellationToken);
        if (!coverage.IsComplete)
        {
            var uncompletedGroup = coverage.GroupSummaries.FirstOrDefault(x => !x.IsComplete);
            if (uncompletedGroup != null)
            {
                if (uncompletedGroup.MappingMode == "UNMAPPED")
                {
                    // FIN-VAL-177: Kelompok tanpa pemetaan aktif
                    throw new FinanceSubledgerSnapshotValidationException(
                        $"Snapshot tidak dapat diterbitkan: kelompok {uncompletedGroup.GroupName} belum punya kode akun.");
                }

                if (uncompletedGroup.MappingMode == "SEGMENTED" && uncompletedGroup.MissingSegments.Count > 0)
                {
                    // FIN-VAL-178: Segmen terpetakan sebagian
                    var missingSegment = uncompletedGroup.MissingSegments.First();
                    throw new FinanceSubledgerSnapshotValidationException(
                        $"Snapshot tidak dapat diterbitkan: segmen {missingSegment} pada kelompok {uncompletedGroup.GroupName} belum punya kode akun.");
                }
            }

            throw new FinanceSubledgerSnapshotValidationException(
                "Snapshot tidak dapat diterbitkan: pemetaan akun control subledger belum lengkap.");
        }

        // 2. Ambil seluruh pemetaan akun control aktif
        var activeMappings = await _dbContext.FinSubledgerControlAccountMaps
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.BalanceGroup)
            .ThenBy(x => x.SegmentKey ?? string.Empty)
            .ToListAsync(cancellationToken);

        if (activeMappings.Count == 0)
        {
            throw new FinanceSubledgerSnapshotValidationException(
                "Snapshot tidak dapat diterbitkan: tidak ada pemetaan akun control aktif yang terkonfigurasi.");
        }

        // 3. Hitung Posisi Saldo Menggunakan Kalkulator (BE-FIN-067, FIN-VAL-170, FIN-DEC-112)
        // Nol pembacaan OutstandingAmount atau ClosingBalance!
        SubledgerPositionResponse positionResponse;
        try
        {
            positionResponse = await _balanceCalculator.CalculatePositionAsync(periodEndDate, cancellationToken);
        }
        catch (FinanceSubledgerValidationException ex)
        {
            throw new FinanceSubledgerSnapshotValidationException(ex.Message);
        }

        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_SUBLEDGER_SNAPSHOT_{request.AccountingPeriodCode}", cancellationToken);

            var stagedItems = new List<SubledgerAccountSnapshotItemResponse>();

            // Ambil grup-grup dari hasil kalkulasi
            var kasKasirGroup = positionResponse.Groups.FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.KasKasir);
            var kasKecilGroup = positionResponse.Groups.FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.KasKecil);
            var piutangGroup = positionResponse.Groups.FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.Piutang);
            var utangSupplierGroup = positionResponse.Groups.FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.UtangSupplier);

            foreach (var mapping in activeMappings)
            {
                decimal amount;
                string category;
                string accountName;

                if (mapping.BalanceGroup == FinSubledgerBalanceGroups.KasKasir)
                {
                    amount = kasKasirGroup?.CalculatedPosition ?? 0.00m;
                    category = SubledgerAccountCategories.CashierCash;
                    accountName = "Kas Kasir";
                }
                else if (mapping.BalanceGroup == FinSubledgerBalanceGroups.KasKecil)
                {
                    amount = kasKecilGroup?.CalculatedPosition ?? 0.00m;
                    category = SubledgerAccountCategories.PettyCash;
                    accountName = "Kas Kecil";
                }
                else if (mapping.BalanceGroup == FinSubledgerBalanceGroups.Piutang)
                {
                    category = SubledgerAccountCategories.Receivables;
                    if (mapping.SegmentKey == null)
                    {
                        amount = piutangGroup?.CalculatedPosition ?? 0.00m;
                        accountName = "Piutang Usaha / Pasien & Penjamin";
                    }
                    else
                    {
                        var segmentItem = piutangGroup?.Segments.FirstOrDefault(s => s.SegmentKey == mapping.SegmentKey);
                        amount = segmentItem?.Amount ?? 0.00m;
                        accountName = segmentItem != null ? $"Piutang - {segmentItem.SegmentName}" : $"Piutang - {mapping.SegmentKey}";
                    }
                }
                else if (mapping.BalanceGroup == FinSubledgerBalanceGroups.UtangSupplier)
                {
                    amount = utangSupplierGroup?.CalculatedPosition ?? 0.00m;
                    category = SubledgerAccountCategories.SupplierPayables;
                    accountName = "Utang Usaha / Supplier";
                }
                else if (mapping.BalanceGroup == FinSubledgerBalanceGroups.UtangJasaMedis)
                {
                    // FIN-DEC-122: Utang jasa medis dikirim 0.00 selama tabel belum ada penulisnya
                    amount = 0.00m;
                    category = SubledgerAccountCategories.MedicalServicePayables;
                    accountName = mapping.SegmentKey != null ? $"Utang Jasa Medis - {mapping.SegmentKey}" : "Utang Jasa Medis";
                }
                else
                {
                    amount = 0.00m;
                    category = mapping.BalanceGroup;
                    accountName = mapping.BalanceGroup;
                }

                var sourceTransactionId = $"SUBLEDGER-{request.AccountingPeriodCode}-{mapping.ControlAccountCode}";

                // 4. Jalur Pernyataan Ulang (FIN-DEC-114): Periksa baris outbox terakhir untuk akun ini
                var latestExistingEvent = await _dbContext.Set<FinAccountingEventOutbox>().AsNoTracking()
                    .Where(x => !x.IsDelete &&
                                x.SourceModule == FinAccountingEventSourceModules.Finance &&
                                x.SourceTransactionId == sourceTransactionId &&
                                x.EventTypeCode == FinAccountingEventTypeCodes.SaldoSubledger)
                    .OrderByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);

                FinAccountingEventOutbox stagedEvent;

                if (latestExistingEvent != null && latestExistingEvent.Amount == amount)
                {
                    // Nilai tidak berubah: jangan terbitkan versi baru (FIN-DEC-114)
                    stagedEvent = latestExistingEvent;
                }
                else
                {
                    // Belum pernah terbit atau nilainya berubah: pementasan versi baru ke outbox
                    var outboxRequest = new AccountingOutboxEventRequest
                    {
                        EventTypeCode = FinAccountingEventTypeCodes.SaldoSubledger,
                        SourceTransactionId = sourceTransactionId,
                        EventOccurredAt = DateTimeOffset.UtcNow,
                        AccountingDate = periodEndDate,
                        Amount = amount, // Boleh negatif apa adanya, tanpa Math.Max (FIN-DEC-112, FIN-VAL-211)
                        CorrelationId = correlationId,
                        CausationId = causationId,
                        ActorUserId = actorUserId,
                        SubledgerBalance = new SubledgerBalanceRequest
                        {
                            AccountingPeriodCode = request.AccountingPeriodCode,
                            ControlAccountCode = mapping.ControlAccountCode
                        }
                    };

                    stagedEvent = await _outboxService.StageEventAsync(outboxRequest, cancellationToken);
                }

                stagedItems.Add(new SubledgerAccountSnapshotItemResponse
                {
                    AccountCategory = category,
                    AccountName = accountName,
                    ControlAccountCode = mapping.ControlAccountCode,
                    Amount = stagedEvent.Amount,
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
                Message = $"Snapshot saldo subledger untuk periode {request.AccountingPeriodCode} berhasil dikalkulasi dan {stagedItems.Count} akun control subledger berhasil diproses."
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
    /// Mengambil rincian snapshot saldo subledger per periode yang tercatat di FinAccountingEventOutbox (FIN-DEC-113).
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
        var periodEndDate = FinanceBusinessDate.GetPeriodEndDate(periodYear, periodMonth);

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

        // Ambil pemetaan aktif untuk resolve nama dan evaluasi kelengkapan
        var activeMappings = await _dbContext.FinSubledgerControlAccountMaps.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var activeControlAccountCodes = activeMappings
            .Select(x => x.ControlAccountCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var items = new List<SubledgerAccountSnapshotItemResponse>();
        foreach (var ev in latestPerSource)
        {
            var controlAccountCode = ev.SourceTransactionId.Length > prefix.Length
                ? ev.SourceTransactionId[prefix.Length..]
                : string.Empty;

            var (category, name) = ResolveAccountInfo(controlAccountCode, activeMappings);

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

        // FIN-DEC-113 & Catatan Desain 4654:
        // IsComplete bukan lagi items.Count >= 4, melainkan apakah seluruh pemetaan aktif sudah terbit
        var isComplete = activeControlAccountCodes.Count > 0 &&
                         activeControlAccountCodes.All(code => items.Any(it => string.Equals(it.ControlAccountCode, code, StringComparison.OrdinalIgnoreCase)));

        return new SubledgerPeriodSnapshotsResponse
        {
            AccountingPeriodCode = accountingPeriodCode,
            AccountingDate = periodEndDate,
            IsComplete = isComplete,
            TotalBalance = items.Sum(x => x.Amount),
            Items = items
        };
    }

    private static (string Category, string Name) ResolveAccountInfo(
        string controlAccountCode,
        List<FinSubledgerControlAccountMap> activeMappings)
    {
        var mapping = activeMappings.FirstOrDefault(x => string.Equals(x.ControlAccountCode, controlAccountCode, StringComparison.OrdinalIgnoreCase));
        if (mapping != null)
        {
            var category = mapping.BalanceGroup;
            var groupName = GetBalanceGroupName(mapping.BalanceGroup);
            var name = mapping.SegmentKey != null
                ? $"{groupName} - {mapping.SegmentKey}"
                : groupName;
            return (category, name);
        }

        return controlAccountCode switch
        {
            SubledgerControlAccountDefaults.CashierCash => (SubledgerAccountCategories.CashierCash, "Kas Kasir"),
            SubledgerControlAccountDefaults.PettyCash => (SubledgerAccountCategories.PettyCash, "Kas Kecil"),
            SubledgerControlAccountDefaults.Receivables => (SubledgerAccountCategories.Receivables, "Piutang Usaha / Pasien & Penjamin"),
            SubledgerControlAccountDefaults.SupplierPayables => (SubledgerAccountCategories.SupplierPayables, "Utang Usaha / Supplier"),
            _ => ("UNKNOWN", $"Akun Kontrol ({controlAccountCode})")
        };
    }

    private static string GetBalanceGroupName(string balanceGroup) => balanceGroup switch
    {
        FinSubledgerBalanceGroups.KasKasir => "Kas Kasir",
        FinSubledgerBalanceGroups.KasKecil => "Kas Kecil",
        FinSubledgerBalanceGroups.Piutang => "Piutang Usaha / Pasien & Penjamin",
        FinSubledgerBalanceGroups.UtangSupplier => "Utang Usaha / Supplier",
        FinSubledgerBalanceGroups.UtangJasaMedis => "Utang Jasa Medis",
        _ => balanceGroup
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
