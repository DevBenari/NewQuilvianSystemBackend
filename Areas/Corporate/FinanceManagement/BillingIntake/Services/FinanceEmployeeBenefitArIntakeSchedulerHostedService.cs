using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>
/// Penjadwal intake otomatis AR Karyawan (EMPLOYEE_BENEFIT): setiap <c>PollIntervalSeconds</c> menemukan handoff AR
/// Karyawan yang sudah melewati <c>GraceMinutes</c> lalu mengolahnya menjadi piutang lewat jalur manual yang sudah ada
/// (<see cref="FinanceEmployeeBenefitArAutoIntakeService"/>). Jenis fakta lain tetap manual.
///
/// DIBANGUN MATI: <see cref="FinanceEmployeeBenefitArIntakeSchedulerOptions.Enabled"/> = false adalah nilai bawaan,
/// pola yang sama dengan <see cref="FinanceCashierShiftMarkerSchedulerHostedService"/>. Kegagalan pengolahan sebuah fakta
/// tersimpan pada barisnya (status ERROR) dan terlihat di Pemantauan Finance; siklus berikutnya tidak mengulangnya.
/// </summary>
public sealed class FinanceEmployeeBenefitArIntakeSchedulerHostedService : BackgroundService
{
    private const int MinimumPollIntervalSeconds = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FinanceEmployeeBenefitArIntakeSchedulerOptions _options;
    private readonly ILogger<FinanceEmployeeBenefitArIntakeSchedulerHostedService> _logger;

    public FinanceEmployeeBenefitArIntakeSchedulerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<FinanceEmployeeBenefitArIntakeSchedulerOptions> options,
        ILogger<FinanceEmployeeBenefitArIntakeSchedulerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Penjadwal intake AR Karyawan Finance dinonaktifkan melalui konfigurasi " +
                "(Finance:EmployeeBenefitArIntakeScheduler:Enabled = false). Nol fakta diolah otomatis.");
            return;
        }

        var jeda = TimeSpan.FromSeconds(Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds));
        using var timer = new PeriodicTimer(jeda);

        _logger.LogInformation(
            "Penjadwal intake AR Karyawan Finance aktif. Poll={Jeda}s, MasaTunggu={Tunggu}menit, MaksPerSiklus={Maks}, " +
            "AktorSistem={Aktor}.",
            jeda.TotalSeconds, Math.Max(0, _options.GraceMinutes), Math.Clamp(_options.MaxPerCycle, 1, 500),
            _options.SystemActorUserId?.ToString() ?? "(kosong: Guid.Empty)");

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
                _logger.LogError(ex, "Siklus penjadwal intake AR Karyawan Finance gagal secara keseluruhan.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task JalankanSatuSiklusAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<FinanceEmployeeBenefitArAutoIntakeService>();

        var hasil = await service.RunAsync(
            _options.SystemActorUserId ?? Guid.Empty,
            TimeSpan.FromMinutes(Math.Max(0, _options.GraceMinutes)),
            _options.MaxPerCycle,
            ct);

        if (hasil.Total == 0) return;

        _logger.LogInformation(
            "Siklus intake AR Karyawan selesai. Ditemukan={Ditemukan}, Diolah={Diolah}, Gagal={Gagal}, Dilewati={Dilewati}.",
            hasil.Discovered, hasil.Processed, hasil.Failed, hasil.Skipped);

        if (hasil.Failed > 0)
        {
            _logger.LogWarning(
                "{Gagal} fakta AR Karyawan gagal diolah otomatis dan berstatus ERROR di Pemantauan Finance; tidak dicoba ulang otomatis.",
                hasil.Failed);
        }
    }
}
