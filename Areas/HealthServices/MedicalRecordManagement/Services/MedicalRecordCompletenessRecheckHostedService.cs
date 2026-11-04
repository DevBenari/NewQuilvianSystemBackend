using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Pemeriksaan berkala kelengkapan berkas (`BE-40`, `GAP-ARCH-RM-04`, desain A4.13).
    /// </summary>
    /// <remarks>
    /// <b>Kenapa perlu.</b> Modul sumber tidak menerbitkan event "dokumen final". Hasil kultur yang
    /// dirilis tiga hari sesudah pasien pulang tidak memberi tahu rekam medis. Tanpa pemeriksaan
    /// berkala, kunjungan itu tetap "Menunggu hasil" sampai ada petugas yang menekan nilai ulang.
    ///
    /// <b>Irama.</b> Setiap <c>MedicalRecordCompleteness:RecheckIntervalSeconds</c> (bawaan 300 detik,
    /// paling cepat 30) dijalankan satu putaran
    /// <see cref="MedicalRecordCompletenessService.RunRecheckRoundAsync"/>. Kapan sebuah kunjungan
    /// mendapat giliran ditentukan <c>NextCheckAt</c>: 60 menit untuk kunjungan yang tutup klinis
    /// ≤ 30 hari, 24 jam untuk yang lebih lama. Angka-angka ini bawaan teknis, bukan kebijakan rumah
    /// sakit, dan dapat diubah per lingkungan.
    ///
    /// <b>Dimatikan</b> dengan <c>MedicalRecordCompleteness:RecheckEnabled = false</c>.
    ///
    /// Log hanya ringkasan jumlah (kriteria 4) — tidak pernah id penulis maupun rincian kekurangan.
    /// </remarks>
    public sealed class MedicalRecordCompletenessRecheckHostedService : BackgroundService
    {
        private const int JedaMinimumDetik = 30;
        private const int JedaBawaanDetik = 300;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MedicalRecordCompletenessRecheckHostedService> _logger;
        private readonly bool _aktif;
        private readonly TimeSpan _jeda;

        public MedicalRecordCompletenessRecheckHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<MedicalRecordCompletenessRecheckHostedService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _aktif = configuration.GetValue("MedicalRecordCompleteness:RecheckEnabled", true);
            _jeda = TimeSpan.FromSeconds(Math.Max(JedaMinimumDetik,
                configuration.GetValue("MedicalRecordCompleteness:RecheckIntervalSeconds", JedaBawaanDetik)));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_aktif)
            {
                _logger.LogInformation("Pemeriksaan berkala kelengkapan berkas dinonaktifkan melalui konfigurasi.");
                return;
            }

            using var timer = new PeriodicTimer(_jeda);

            _logger.LogInformation("Pemeriksaan berkala kelengkapan berkas aktif. Jeda={Jeda}s.", (int)_jeda.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Satu putaran gagal (mis. basis data sesaat tak terjangkau) tidak menghentikan layanan.
                    _logger.LogError(ex, "Putaran pemeriksaan berkala kelengkapan berkas gagal.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
        }

        /// <summary>Satu putaran dalam scope sendiri. Dipisahkan supaya dapat diuji tanpa menunggu timer.</summary>
        public async Task<MedicalRecordCompletenessRecheckSummary> RunOnceAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<MedicalRecordCompletenessService>();

            var hasil = await service.RunRecheckRoundAsync(cancellationToken);

            if (hasil.Considered > 0)
            {
                _logger.LogInformation(
                    "Putaran pemeriksaan kelengkapan selesai. Dinilai={Dinilai}, PenilaianPertama={Pertama}, "
                    + "Lengkap={Lengkap}, BelumLengkap={BelumLengkap}, MenungguHasil={Menunggu}, "
                    + "TidakDapatDipastikan={TakPasti}, Dilewati={Dilewati}, Gagal={Gagal}.",
                    hasil.Considered,
                    hasil.FirstEvaluations,
                    hasil.ByState.GetValueOrDefault(CompletenessState.Complete),
                    hasil.ByState.GetValueOrDefault(CompletenessState.Incomplete),
                    hasil.ByState.GetValueOrDefault(CompletenessState.AwaitingResult),
                    hasil.ByState.GetValueOrDefault(CompletenessState.Undetermined),
                    hasil.Skipped,
                    hasil.Failed);
            }

            return hasil;
        }
    }
}
