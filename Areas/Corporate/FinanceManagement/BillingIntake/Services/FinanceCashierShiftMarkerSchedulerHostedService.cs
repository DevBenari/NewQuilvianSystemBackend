using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Penjadwal sinkronisasi penanda shift kasir berkala (BE-FIN-073, FIN-DES-078, FIN-DEC-118,
/// FR-FIN-156).
///
/// Memanggil <see cref="FinanceBillingIntakeService.SyncCashierShiftClosureMarkersAsync"/> — method
/// yang SUDAH ADA sejak BE-FIN-045 dan diperluas BE-FIN-070 — setiap <c>PollIntervalSeconds</c> tanpa
/// seorang pun menekan tombol sinkronisasi manual (`POST .../cashier-shift-closure-markers/sync`).
/// Method itu TIDAK DIUBAH sama sekali oleh task ini: ia sudah idempoten (shift yang penandanya sudah
/// ada tidak diterbitkan ulang) dan sudah MEMBACA SAJA tabel `Bil*` — nol tulisan ke tabel `Bil*`
/// mana pun, persis seperti jalur manual yang sudah berjalan.
///
/// DIBANGUN MATI (FIN-DES-078): <see cref="FinanceCashierShiftMarkerSchedulerOptions.Enabled"/> = false
/// adalah nilai bawaan. Tanpa konfigurasi eksplisit, penjadwal ini tidak pernah memicu sinkronisasi
/// apa pun — perilaku lingkungan mana pun tidak berubah hanya karena kelas ini ditambahkan.
///
/// Ini adalah hosted service PALING SEDERHANA dari ketiga yang dibangun FIN-DES-078 (dibandingkan
/// <c>FinanceAccountingDispatchWorker</c> BE-FIN-071 dan <c>FinanceSubledgerSnapshotSchedulerHostedService</c>
/// BE-FIN-072): ia murni interval polling tanpa jam spesifik, tanpa gerbang kode kedua, dan tanpa
/// panggilan HTTP keluar — risiko terendah pada REV-14C.
/// </summary>
public sealed class FinanceCashierShiftMarkerSchedulerHostedService : BackgroundService
{
    private const int MinimumPollIntervalSeconds = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FinanceCashierShiftMarkerSchedulerOptions _options;
    private readonly ILogger<FinanceCashierShiftMarkerSchedulerHostedService> _logger;

    public FinanceCashierShiftMarkerSchedulerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<FinanceCashierShiftMarkerSchedulerOptions> options,
        ILogger<FinanceCashierShiftMarkerSchedulerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Gerbang konfigurasi — nilai bawaan false = dibangun mati (FIN-DES-078).
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Penjadwal penanda shift kasir Finance dinonaktifkan melalui konfigurasi " +
                "(Finance:CashierShiftMarkerScheduler:Enabled = false). Nol sinkronisasi dipicu.");
            return;
        }

        var jeda = TimeSpan.FromSeconds(Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds));
        using var timer = new PeriodicTimer(jeda);

        _logger.LogInformation(
            "Penjadwal penanda shift kasir Finance aktif. Poll={Jeda}s.", jeda.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await JalankanSatuSiklusAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siklus penjadwal penanda shift kasir Finance gagal secara keseluruhan.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task JalankanSatuSiklusAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FinanceBillingIntakeService>();

        var hasil = await service.SyncCashierShiftClosureMarkersAsync(
            _options.SystemActorUserId ?? Guid.Empty, ct);

        var total = hasil.ClosureIssued + hasil.ReversalIssued + hasil.OpeningIssued;
        if (total == 0) return;

        _logger.LogInformation(
            "Siklus penjadwal penanda shift kasir selesai. Penutupan={Penutupan}, Pembalik={Pembalik}, Pembukaan={Pembukaan}.",
            hasil.ClosureIssued, hasil.ReversalIssued, hasil.OpeningIssued);
    }
}
