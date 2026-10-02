using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Workers;

/// <summary>
/// Mengirim ulang efek folio yang belum sampai ke invoice canonical (<c>BE-RJE-010</c>,
/// <c>RJ-E2E-DEC-009</c>).
/// </summary>
/// <remarks>
/// <para>
/// Pola mengikuti <c>InpatientIntegrationOutboxWorker</c>: satu scope per siklus, paling banyak
/// 50 baris, dan siklus yang gagal tidak menghentikan pekerja. Bedanya, batas percobaan dan jeda
/// dibaca dari <see cref="MstBillingSyncPolicy"/> <c>INVOICE_SYNC</c> sehingga dapat diubah admin
/// tanpa rilis.
/// </para>
/// <para>
/// Pekerja hanya <b>memilih</b> baris yang jatuh tempo. Penghitungan percobaan, jadwal
/// <c>min(Base × 2^(n−1), Max)</c>, dan <c>RETRY_EXHAUSTED</c> dicatat
/// <see cref="BillingClinicalChargeBridgeService"/> — di satu tempat yang sama dengan pemanggilan
/// langsung setelah commit, sehingga keduanya tidak pernah menghitung dengan aturan berbeda.
/// Pemrosesan ganda (pekerja dan pemanggilan langsung bersamaan, atau lebih dari satu instance)
/// aman: hasil ditulis dengan penjaga <c>InvoiceSyncVersion</c>, dan kunci idempotency ke invoice
/// deterministik per fakta dan versi.
/// </para>
/// <para>
/// <b>Fail-closed:</b> tanpa kebijakan aktif, baris yang jatuh tempo tidak dikirim ulang; baris itu
/// langsung masuk antrean rekonsiliasi dengan sebab <c>SYNC_POLICY_INACTIVE</c>.
/// </para>
/// </remarks>
public sealed class BilInvoiceSyncWorker : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BilInvoiceSyncWorker> _logger;

    public BilInvoiceSyncWorker(IServiceScopeFactory scopeFactory, ILogger<BilInvoiceSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BilInvoiceSyncWorker aktif. Poll={Poll}s, Batch={Batch}.",
            PollInterval.TotalSeconds, BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // Siklus yang gagal (mis. database terputus sesaat) tidak menghentikan pekerja.
                _logger.LogError(exception, "Siklus kirim ulang sinkron invoice gagal.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("BilInvoiceSyncWorker dihentikan.");
    }

    /// <summary>Satu siklus; mengembalikan jumlah baris yang diproses.</summary>
    public async Task<int> ProcessDueBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;

        var policy = await db.Set<MstBillingSyncPolicy>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.PolicyCode == BillingSyncPolicyCodes.InvoiceSync && x.IsActive && !x.IsDelete,
                cancellationToken);

        // Baris Pending tanpa jadwal biasanya sedang diproses pemanggilan langsung setelah commit.
        // Ia baru dianggap tertinggal setelah melewati jeda dasar kebijakan (atau 60 detik bila
        // kebijakan tidak aktif).
        var pendingCutoff = now.AddSeconds(-(policy?.BaseDelaySeconds ?? 60));
        var due = await db.Set<BilProcessingEffect>()
            .Where(x => !x.IsDelete
                && ((x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Failed
                        && (x.InvoiceSyncNextAttemptAt == null || x.InvoiceSyncNextAttemptAt <= now))
                    || (x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Pending
                        && (x.InvoiceSyncNextAttemptAt != null
                            ? x.InvoiceSyncNextAttemptAt <= now
                            : (x.UpdateDateTime ?? x.CreateDateTime) <= pendingCutoff))))
            .OrderBy(x => x.InvoiceSyncNextAttemptAt ?? x.UpdateDateTime ?? x.CreateDateTime)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        if (due.Count == 0) return 0;

        if (policy is null)
            return await ParkAsync(db, due, now, cancellationToken);

        var bridge = scope.ServiceProvider.GetRequiredService<BillingClinicalChargeBridgeService>();
        var synced = 0;
        foreach (var effect in due)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var status = await bridge.TrySyncEffectAsync(effect.Id, cancellationToken);
            if (status == BillingInvoiceSyncStatus.Synced) synced++;
        }

        _logger.LogInformation("Kirim ulang sinkron invoice: {Count} baris diproses, {Synced} tersinkron.",
            due.Count, synced);
        return due.Count;
    }

    private async Task<int> ParkAsync(
        ApplicationDbContext db,
        List<BilProcessingEffect> due,
        DateTime now,
        CancellationToken cancellationToken)
    {
        foreach (var effect in due)
        {
            effect.InvoiceSyncStatus = BillingInvoiceSyncStatus.ReconciliationRequired;
            effect.InvoiceSyncErrorCode = BillingBridgeCodes.SyncPolicyInactive;
            effect.InvoiceSyncErrorMessage = "Pengiriman ulang otomatis sedang dimatikan. Item menunggu penanganan manual.";
            effect.InvoiceSyncNextAttemptAt = null;
            effect.InvoiceSyncVersion += 1;
            effect.UpdateDateTime = now;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Pemroses lain lebih dulu menulis baris ini; hasilnya yang dipakai.
            return 0;
        }

        _logger.LogWarning("Kebijakan INVOICE_SYNC tidak aktif: {Count} baris dipindah ke antrean rekonsiliasi.", due.Count);
        return due.Count;
    }
}
