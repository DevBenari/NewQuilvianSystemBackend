using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Konfigurasi <c>OpticalManagement:ExamChargeRetry</c>. Nilai di luar batas dipakai pada batas terdekatnya.</summary>
    public sealed class OptExamChargeRetryOptions
    {
        public const string SectionName = "OpticalManagement:ExamChargeRetry";

        /// <summary>
        /// Saklar. Bawaan menyala karena tarif yang tertinggal berarti pendapatan hilang (<c>OPT-DEC-082</c>); dimatikan saat
        /// menguji modul lain terhadap basis data bersama, karena pekerjaan ini menulis ke basis data begitu aplikasi menyala.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Jeda antarputaran dalam menit. Paling kecil 1.</summary>
        public int IntervalMinutes { get; set; } = 5;

        /// <summary>Batas pemeriksaan per putaran, 1–500.</summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>Jeda sesudah final dalam menit, agar tidak merebut kiriman pertama yang masih berjalan. Paling kecil 1.</summary>
        public int GraceMinutes { get; set; } = 5;
    }

    /// <summary>Pemeriksaan final yang tarifnya belum tercatat.</summary>
    public sealed record OptExamAwaitingCharge(Guid Id, DateTime FinalizedAt);

    public enum OptExamChargeRetryOutcome
    {
        /// <summary>Producer mencatat fakta tarifnya; penanda terisi.</summary>
        Recorded = 1,

        /// <summary>Belum tercatat; penanda tetap kosong dan dicoba lagi pada giliran berikutnya.</summary>
        NotRecorded = 2,

        /// <summary>Sudah tercatat atau tidak lagi memenuhi syarat sejak dipilih.</summary>
        NotEligible = 3,

        /// <summary>Tidak ada pelaku untuk fakta, sebab <c>FinalizedBy</c> kosong.</summary>
        MissingActor = 4,
    }

    /// <summary>Hasil kirim ulang satu pemeriksaan.</summary>
    public sealed record OptExamChargeRetryResult(OptExamChargeRetryOutcome Outcome, string? ExamNumber, string? Message);

    /// <summary>Ringkasan satu putaran.</summary>
    public sealed record OptExamChargeRetryRound(int Selected, int Recorded, int Waiting, bool ProcedureMissing);

    /// <summary>
    /// Mengirim ulang tarif pemeriksaan yang belum sempat tercatat (<c>OPT-DEC-082</c>, <c>083</c>; menutup risiko <c>R2</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>ClinicalFactDispatchWorker</c> hanya mengirim ulang fakta yang sudah tercatat. Pekerjaan ini menangani yang belum
    /// tercatat sama sekali, yaitu saat tindakan belum diatur atau basis data putus sebelum producer mencatat.
    /// </para>
    /// <para>
    /// Satu scope per pemeriksaan, karena producer membersihkan change tracker saat penyerahan terputus. Identitas fakta sama
    /// dengan kiriman pertama, sehingga putaran berulang atau dua proses tidak menghasilkan dua baris tarif.
    /// </para>
    /// <para>
    /// Penunjuk posisi hanya memakai <c>FinalizedAt</c> dan hidup di memori. Pemeriksaan dengan waktu final persis sama di batas
    /// halaman terlewat satu giliran dan terambil saat penunjuk kembali ke awal; tidak ada yang hilang.
    /// </para>
    /// </remarks>
    public sealed class OptExamChargeRetryHostedService : BackgroundService
    {
        private const string LogModule = "Optical";
        private const string LogAction = "OptBilling.ExamChargeRetry";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<OptExamChargeRetryOptions> _options;
        private readonly ILogger<OptExamChargeRetryHostedService> _logger;
        private readonly TimeProvider _timeProvider;
        private DateTime? _cursor;

        public OptExamChargeRetryHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<OptExamChargeRetryOptions> options,
            ILogger<OptExamChargeRetryHostedService> logger,
            TimeProvider? timeProvider = null)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.CurrentValue.Enabled)
            {
                _logger.LogInformation("Kirim ulang tarif pemeriksaan Optik dinonaktifkan melalui konfigurasi.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_options.CurrentValue.Enabled)
                        await ProcessRoundAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Putaran kirim ulang tarif pemeriksaan Optik gagal; dicoba lagi pada putaran berikutnya.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(Math.Max(1, _options.CurrentValue.IntervalMinutes)), _timeProvider, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>Satu putaran. Terbuka agar dapat diuji tanpa menjalankan hosted service.</summary>
        public async Task<OptExamChargeRetryRound> ProcessRoundAsync(CancellationToken cancellationToken = default)
        {
            var options = _options.CurrentValue;
            var batchSize = Math.Clamp(options.BatchSize, 1, 500);
            var finalizedBefore = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-Math.Max(1, options.GraceMinutes));

            List<OptExamAwaitingCharge> exams;
            using (var scope = _scopeFactory.CreateScope())
            {
                exams = await scope.ServiceProvider.GetRequiredService<OptRefractionExamService>()
                    .GetExamsAwaitingChargeAsync(finalizedBefore, _cursor, batchSize, cancellationToken);
                if (exams.Count == 0)
                {
                    _cursor = null;
                    return new OptExamChargeRetryRound(0, 0, 0, false);
                }

                // Diperiksa sekali di awal; tanpa ini setiap pemeriksaan menulis peringatan yang sama pada setiap putaran.
                if (!await scope.ServiceProvider.GetRequiredService<OptBillingGateway>().IsExamProcedureConfiguredAsync(cancellationToken))
                {
                    _logger.LogWarning(
                        "Kirim ulang tarif pemeriksaan Optik dilewati: tindakan Pemeriksaan Refraksi belum diatur. {Count} pemeriksaan menunggu pada halaman ini.",
                        exams.Count);
                    return new OptExamChargeRetryRound(exams.Count, 0, exams.Count, true);
                }
            }

            var recorded = 0;
            var waiting = 0;
            foreach (var exam in exams)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await RetryOneAsync(exam.Id, cancellationToken))
                    recorded++;
                else
                    waiting++;
            }

            // Halaman penuh: lanjut dari posisi ini, sehingga pemeriksaan yang terus gagal tidak menghalangi yang lain.
            _cursor = exams.Count < batchSize ? null : exams[^1].FinalizedAt;

            _logger.LogInformation(
                "Kirim ulang tarif pemeriksaan Optik: {Selected} dipilih, {Recorded} tercatat, {Waiting} menunggu.",
                exams.Count, recorded, waiting);
            return new OptExamChargeRetryRound(exams.Count, recorded, waiting, false);
        }

        /// <summary>Galat satu pemeriksaan dicatat dan tidak menghentikan putaran.</summary>
        private async Task<bool> RetryOneAsync(Guid examId, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                var result = await scope.ServiceProvider.GetRequiredService<OptRefractionExamService>()
                    .RetryExamChargeAsync(examId, cancellationToken);
                var logger = scope.ServiceProvider.GetRequiredService<LoggerService>();

                if (result.Outcome == OptExamChargeRetryOutcome.Recorded)
                {
                    await logger.InfoAsync(LogModule, LogAction,
                        $"Tarif {result.ExamNumber} dikirim ulang pekerja latar atas nama pemeriksa yang memfinalkan.");
                    return true;
                }

                if (result.Outcome == OptExamChargeRetryOutcome.MissingActor)
                {
                    await logger.WarningAsync(LogModule, LogAction,
                        $"Tarif {result.ExamNumber} tidak dikirim ulang karena pelaku final tidak tercatat.");
                }

                return false;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kirim ulang tarif pemeriksaan Optik {ExamId} gagal; dicoba lagi pada giliran berikutnya.", examId);
                return false;
            }
        }
    }
}
