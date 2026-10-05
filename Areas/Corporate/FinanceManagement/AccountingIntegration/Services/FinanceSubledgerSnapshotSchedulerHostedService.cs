using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Penjadwal snapshot saldo subledger harian (BE-FIN-072, FIN-DES-078, FIN-DEC-092, FIN-DEC-114,
/// FR-FIN-154, FR-FIN-155).
///
/// Setiap hari pukul 00.05 WIB (dapat dikonfigurasi), penjadwal ini memicu
/// <see cref="FinanceSubledgerSnapshotService.GenerateMonthlySnapshotsAsync"/> untuk PERIODE
/// SEBELUMNYA relatif terhadap tanggal WIB hari itu — bulan kalender tepat sebelum bulan WIB
/// berjalan. Method itu SUDAH aman-pernyataan-ulang (FIN-DEC-114, FinanceSubledgerSnapshotService.cs
/// bagian "Jalur Pernyataan Ulang"): akun yang nilainya tidak berubah dibiarkan pada versi outbox
/// terakhirnya, dan hanya akun yang berubah mendapat baris SourceVersion baru lewat StageEventAsync
/// yang sudah menaikkan versi secara otomatis. Dengan satu mekanisme yang sama, penjadwal ini
/// menjawab dua kebutuhan sekaligus tanpa logika tambahan:
///   - Tanggal 1 WIB: periode sebelumnya belum pernah terbit -> ini publikasi PERTAMA (FIN-DEC-092).
///   - Tanggal 2 dst.: periode sebelumnya sudah terbit -> ini pemeriksaan PERNYATAAN ULANG,
///     menangkap mutasi terlambat (mis. shift malam yang baru ditutup, bertanggal akhir periode
///     sebelumnya) yang mengubah posisi periode yang snapshot-nya sudah terbit (FIN-DEC-114).
///
/// DIBANGUN MATI (FIN-DES-078): <see cref="FinanceSubledgerSnapshotSchedulerOptions.Enabled"/> = false
/// adalah nilai bawaan. Tanpa konfigurasi eksplisit, penjadwal ini tidak pernah memicu snapshot apa pun.
///
/// Gagal tertutup (FIN-DES-080) tetap berlaku di dalam service yang dipanggil: bila pemetaan akun
/// control belum lengkap pada siklus tertentu, GenerateMonthlySnapshotsAsync melempar
/// <see cref="FinanceSubledgerSnapshotValidationException"/>, dicatat di sini sebagai peringatan,
/// dan dicoba lagi siklus berikutnya — TIDAK menghentikan hosted service maupun aplikasi.
///
/// AMAN LINTAS-PROSES: GenerateMonthlySnapshotsAsync sendiri sudah memegang advisory lock Postgres
/// per periode dan membandingkan terhadap unique index outbox yang sudah ada (FIN-DES-083), sehingga
/// dua instance penjadwal (mis. dua pod) yang kebetulan jalan bersamaan untuk periode yang sama
/// TIDAK menggandakan baris — bukan tanggung jawab kelas ini untuk menjaga itu ulang.
/// </summary>
public sealed class FinanceSubledgerSnapshotSchedulerHostedService : BackgroundService
{
    private const int MinimumPollIntervalSeconds = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FinanceSubledgerSnapshotSchedulerOptions _options;
    private readonly ILogger<FinanceSubledgerSnapshotSchedulerHostedService> _logger;
    private DateOnly? _lastRunDateWib;

    public FinanceSubledgerSnapshotSchedulerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<FinanceSubledgerSnapshotSchedulerOptions> options,
        ILogger<FinanceSubledgerSnapshotSchedulerHostedService> logger)
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
                "Penjadwal snapshot saldo subledger Finance dinonaktifkan melalui konfigurasi " +
                "(Finance:SubledgerSnapshotScheduler:Enabled = false). Nol snapshot dipicu.");
            return;
        }

        var jeda = TimeSpan.FromSeconds(Math.Max(MinimumPollIntervalSeconds, _options.PollIntervalSeconds));
        using var timer = new PeriodicTimer(jeda);

        _logger.LogInformation(
            "Penjadwal snapshot saldo subledger Finance aktif. Jam={Jam:D2}:{Menit:D2} WIB, Poll={Jeda}s.",
            _options.DailyRunHourWib, _options.DailyRunMinuteWib, jeda.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // FinanceBusinessDate.BusinessTimeZone adalah satu-satunya sumber zona waktu WIB
                // Finance (FIN-DES-082) — dipakai ulang di sini, bukan disalin lagi.
                var nowWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, FinanceBusinessDate.BusinessTimeZone);
                var todayWib = DateOnly.FromDateTime(nowWib.DateTime);
                var scheduledTimeOfDay = new TimeSpan(_options.DailyRunHourWib, _options.DailyRunMinuteWib, 0);

                if (_lastRunDateWib != todayWib && nowWib.TimeOfDay >= scheduledTimeOfDay)
                {
                    await JalankanSatuSiklusAsync(todayWib, stoppingToken);
                    _lastRunDateWib = todayWib;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siklus penjadwal snapshot saldo subledger Finance gagal secara keseluruhan.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task JalankanSatuSiklusAsync(DateOnly todayWib, CancellationToken ct)
    {
        // Periode sebelumnya = bulan kalender tepat sebelum bulan WIB berjalan.
        var firstOfThisMonthWib = new DateOnly(todayWib.Year, todayWib.Month, 1);
        var lastDayOfPreviousMonth = firstOfThisMonthWib.AddDays(-1);
        var periodCode = $"{lastDayOfPreviousMonth.Year:D4}-{lastDayOfPreviousMonth.Month:D2}";

        using var scope = _scopeFactory.CreateScope();
        var snapshotService = scope.ServiceProvider.GetRequiredService<FinanceSubledgerSnapshotService>();

        try
        {
            var result = await snapshotService.GenerateMonthlySnapshotsAsync(
                new GenerateSubledgerSnapshotsRequest { AccountingPeriodCode = periodCode },
                _options.SystemActorUserId ?? Guid.Empty,
                ct);

            _logger.LogInformation(
                "Siklus penjadwal snapshot periode {Periode} selesai. TotalAkun={TotalAkun}, TotalSaldo={TotalSaldo}.",
                periodCode, result.TotalAccounts, result.TotalBalance);
        }
        catch (FinanceSubledgerSnapshotValidationException ex)
        {
            _logger.LogWarning(
                "Siklus penjadwal snapshot periode {Periode} tertahan gagal-tertutup: {Pesan}. Dicoba lagi siklus berikutnya.",
                periodCode, ex.Message);
        }
        catch (AccountingOutboxException ex)
        {
            _logger.LogWarning(
                "Siklus penjadwal snapshot periode {Periode} gagal menerbitkan kejadian outbox: {Pesan}. Dicoba lagi siklus berikutnya.",
                periodCode, ex.Message);
        }
    }
}
