using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Workers;

/// <summary>
/// Mengirim ulang fakta klinis yang hasil penyerahannya ke Billing belum pasti (<c>BE-RJE-011</c>,
/// <c>RJ-E2E-DEC-009</c>, <c>AC-RJ-014</c>).
/// </summary>
/// <remarks>
/// <para>
/// Pekerja hanya memilih fakta yang jatuh tempo dan menyerahkannya ke
/// <see cref="ClinicalMilestoneFactProducer.RedispatchAsync"/>, yang memakai identitas dan
/// <c>IdempotencyKey</c> yang sama. Penghitungan percobaan dan jadwal berada di producer, di satu
/// tempat dengan penyerahan pertama. Billing menjamin kirim ulang tidak menggandakan apa pun.
/// </para>
/// <para>
/// Jalur ini juga mewujudkan AC <c>5</c>–<c>6</c> <c>RJ-DOC-BE-005</c>: fakta yang belum terkirim
/// dapat ditemukan dan dikirim ulang dengan identitas sama. Deteksi <i>producer gap</i> (AC
/// <c>1</c>–<c>4</c>) tetap milik roadmap Dokter dan wajib memakai ulang
/// <see cref="ClinicalMilestoneFactProducer.RedispatchAsync"/>, bukan membangun jalur kedua.
/// </para>
/// <para>
/// <b>Fail-closed:</b> tanpa kebijakan <c>FACT_DISPATCH</c> aktif, fakta yang jatuh tempo tidak
/// dikirim ulang; <c>ReconciliationRequiredAt</c> diisi dan fakta menunggu petugas Billing.
/// </para>
/// </remarks>
public sealed class ClinicalFactDispatchWorker : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClinicalFactDispatchWorker> _logger;

    public ClinicalFactDispatchWorker(IServiceScopeFactory scopeFactory, ILogger<ClinicalFactDispatchWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClinicalFactDispatchWorker aktif. Poll={Poll}s, Batch={Batch}.",
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
                _logger.LogError(exception, "Siklus kirim ulang fakta klinis gagal.");
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

        _logger.LogInformation("ClinicalFactDispatchWorker dihentikan.");
    }

    /// <summary>Satu siklus; mengembalikan jumlah fakta yang diproses.</summary>
    public async Task<int> ProcessDueBatchAsync(CancellationToken cancellationToken)
    {
        List<Guid> due;
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;
            var policy = await db.Set<MstBillingSyncPolicy>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.PolicyCode == BillingSyncPolicyCodes.FactDispatch && x.IsActive && !x.IsDelete,
                    cancellationToken);

            // Fakta Pending tanpa jadwal biasanya sedang diserahkan oleh pemanggil klinisnya. Ia baru
            // dianggap tertinggal setelah melewati jeda dasar kebijakan (atau 60 detik).
            var pendingCutoff = now.AddSeconds(-(policy?.BaseDelaySeconds ?? 60));
            var facts = await db.Set<CliClinicalMilestoneFact>()
                .Where(x => !x.IsDelete
                    && x.ReconciliationRequiredAt == null
                    && (x.DispatchStatus == ClinicalFactDispatchStatus.OutcomeUnknown
                        || x.DispatchStatus == ClinicalFactDispatchStatus.Pending)
                    && (x.NextDispatchAttemptAt != null
                        ? x.NextDispatchAttemptAt <= now
                        : (x.UpdateDateTime ?? x.CreateDateTime) <= pendingCutoff))
                .OrderBy(x => x.NextDispatchAttemptAt ?? x.UpdateDateTime ?? x.CreateDateTime)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
            if (facts.Count == 0) return 0;

            if (policy == null)
                return await ParkAsync(db, facts, now, cancellationToken);

            due = facts.Select(x => x.Id).ToList();
        }

        var dispatched = 0;
        foreach (var factId in due)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Scope per fakta: BillingFolioService dapat membersihkan change tracker saat konflik,
            // dan kegagalan satu fakta tidak boleh mengotori fakta berikutnya.
            using var scope = _scopeFactory.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<ClinicalMilestoneFactProducer>();
            try
            {
                var result = await producer.RedispatchAsync(factId, cancellationToken);
                if (result.DispatchStatus == ClinicalFactDispatchStatus.Dispatched) dispatched++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Kirim ulang fakta klinis {FactId} gagal.", factId);
            }
        }

        _logger.LogInformation("Kirim ulang fakta klinis: {Count} fakta diproses, {Dispatched} terkirim.",
            due.Count, dispatched);
        return due.Count;
    }

    private async Task<int> ParkAsync(
        ApplicationDbContext db,
        List<CliClinicalMilestoneFact> facts,
        DateTime now,
        CancellationToken cancellationToken)
    {
        foreach (var fact in facts)
        {
            fact.NextDispatchAttemptAt = null;
            fact.ReconciliationRequiredAt = now;
            fact.BillingOutcomeCode = BillingBridgeCodes.SyncPolicyInactive;
            fact.BillingOutcomeMessage = "Pengiriman ulang otomatis fakta klinis sedang dimatikan. Fakta menunggu penanganan manual.";
            fact.UpdateDateTime = now;
            fact.Version += 1;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return 0;
        }

        _logger.LogWarning("Kebijakan FACT_DISPATCH tidak aktif: {Count} fakta dipindah ke antrean rekonsiliasi.", facts.Count);
        return facts.Count;
    }
}
