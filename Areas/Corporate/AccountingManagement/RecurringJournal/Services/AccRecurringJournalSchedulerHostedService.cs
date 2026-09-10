using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services
{
    /// <summary>
    /// Menerbitkan jurnal dari template berulang yang sudah jatuh tempo, sekali sehari.
    /// Cakupan <c>BE-ACC-P2-008</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Meniru <c>LeaveAccrualSchedulerHostedService</c> yang sudah ada:
    /// <c>IServiceScopeFactory</c> untuk scope per siklus, <c>IOptions</c> dengan tombol
    /// <c>Enabled</c>, dan penjaga tanggal terakhir diproses supaya satu hari hanya dikerjakan
    /// sekali walaupun siklusnya berjalan berkali-kali.
    /// </para>
    /// <para>
    /// <b>Penjaga tanggal di sini bukan penjaga terbit ganda.</b> Ia hanya menghemat pekerjaan
    /// sia-sia dalam satu proses, dan akan gagal begitu aplikasi berjalan lebih dari satu
    /// instance — masing-masing punya <c>_tanggalTerakhirDiproses</c>-nya sendiri. Penjaga terbit
    /// ganda yang sesungguhnya adalah unique index <c>(TemplateId, AccountingPeriodId)</c> di
    /// database, dan itu tetap benar berapa pun jumlah instance-nya.
    /// </para>
    /// </remarks>
    public class AccRecurringJournalSchedulerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AccRecurringJournalSchedulerOptions _options;
        private readonly ILogger<AccRecurringJournalSchedulerHostedService> _logger;
        private DateOnly? _tanggalTerakhirDiproses;

        public AccRecurringJournalSchedulerHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<AccRecurringJournalSchedulerOptions> options,
            ILogger<AccRecurringJournalSchedulerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Acceptance (4). Dikembalikan lebih dahulu sebelum timer dibuat, sehingga penjadwal
            // yang dimatikan benar-benar tidak berjalan sama sekali.
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Penjadwal jurnal berulang dinonaktifkan melalui konfigurasi.");
                return;
            }

            var jeda = Math.Max(60, _options.PollIntervalSeconds);
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(jeda));

            _logger.LogInformation(
                "Penjadwal jurnal berulang aktif. Worker={Worker}, Poll={Jeda}s, Jam={Jam:00}:{Menit:00}.",
                _options.WorkerInstanceName,
                jeda,
                _options.DailyRunHour,
                _options.DailyRunMinute);

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
                    // Siklus yang gagal TIDAK menghentikan penjadwal. Kegagalan sekali — koneksi
                    // database terputus sesaat, misalnya — tidak boleh membuat jurnal berulang
                    // berhenti terbit sampai aplikasi dimuat ulang.
                    _logger.LogError(ex, "Siklus penjadwal jurnal berulang gagal.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        private async Task JalankanSatuSiklusAsync(CancellationToken ct)
        {
            var sekarangLokal = KeZonaWaktuTerpasang(DateTime.UtcNow);
            var tanggalLokal = DateOnly.FromDateTime(sekarangLokal);

            var jamJatuhTempo = sekarangLokal.TimeOfDay >= new TimeSpan(
                Math.Clamp(_options.DailyRunHour, 0, 23),
                Math.Clamp(_options.DailyRunMinute, 0, 59),
                0);

            if (!jamJatuhTempo || _tanggalTerakhirDiproses == tanggalLokal) return;

            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AccRecurringJournalService>();

            var hasil = await service.TerbitkanYangJatuhTempoAsync(
                sekarangLokal.Date,
                _options.SystemActorUserId ?? Guid.Empty,
                ct);

            _tanggalTerakhirDiproses = tanggalLokal;

            // Nomor jurnal ikut dicatat karena ia identitas, bukan nominal. Nilai jurnalnya tidak
            // pernah masuk log (`ACC-PERMISSION-0.4` bagian 4).
            _logger.LogInformation(
                "Siklus jurnal berulang selesai. Diperiksa={Diperiksa}, Terbit={Terbit}, "
                + "SudahTerbit={SudahTerbit}, Bermasalah={Bermasalah}.",
                hasil.Considered,
                hasil.Published.Count,
                hasil.AlreadyPublishedCount,
                hasil.ProblemCount);

            // Hanya yang BERMASALAH yang diperingatkan. "Sudah terbit" adalah keadaan normal dan
            // memperingatkannya akan memenuhi log dengan tanda bahwa sistemnya bekerja benar.
            foreach (var dilewati in hasil.Skipped.Where(x => !x.AlreadyPublished))
            {
                _logger.LogWarning(
                    "Template {Kode} dilewati penjadwal: {Alasan}",
                    dilewati.TemplateCode,
                    dilewati.Reason);
            }
        }

        private DateTime KeZonaWaktuTerpasang(DateTime utcNow)
        {
            try
            {
                var zona = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zona);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                _logger.LogWarning(
                    "Zona waktu {Zona} tidak dikenali. Memakai UTC.", _options.TimeZoneId);
                return utcNow;
            }
        }
    }
}
