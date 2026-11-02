using System.Threading.Channels;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Antrean dalam proses untuk penilaian kelengkapan pertama sesudah tutup klinis (`BE-39`).
    /// </summary>
    /// <remarks>
    /// <b>Kenapa antrean, bukan penilaian langsung.</b> Penutupan klinis berjalan di dalam transaksi
    /// enam jalur milik modul lain (Registrasi, IGD, Rawat Inap, Tagihan). Menilai di sana berarti
    /// membaca belasan sumber di tengah transaksi orang lain, dan satu kegagalan dapat membatalkan
    /// penutupan — bertentangan dengan `RM-DEC-046`. Jalur penutupan karena itu hanya memanggil
    /// <see cref="Enqueue"/>, yang tidak menyentuh basis data dan tidak pernah melempar galat.
    ///
    /// <b>Batasnya disengaja.</b> Antrean hidup di memori: isinya hilang bila server dinyalakan ulang,
    /// dan bila penuh, id baru dibuang. Kunjungan yang terlewat disusul pemeriksaan berkala (`BE-40`)
    /// atau tombol nilai ulang. Pilihan ini disahkan pemilik modul 29 Oktober 2026.
    /// </remarks>
    public sealed class MedicalRecordCompletenessQueue
    {
        public const int Capacity = 10_000;

        private readonly Channel<QueuedEvaluation> _channel = Channel.CreateBounded<QueuedEvaluation>(
            new BoundedChannelOptions(Capacity)
            {
                // Wait + TryWrite: tidak pernah menunggu, dan menjawab false bila penuh. DropWrite
                // justru menjawab true walau itemnya dibuang.
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });

        internal ChannelReader<QueuedEvaluation> Reader => _channel.Reader;

        /// <summary>Jumlah id yang menunggu; untuk uji dan pemantauan.</summary>
        public int Count => _channel.Reader.Count;

        /// <summary>
        /// Menaruh id kunjungan yang baru ditutup klinis. Bila penutupannya belum terlihat (belum
        /// di-commit), penilaian diulang. Tidak menunggu, tidak melempar galat.
        /// </summary>
        public bool Enqueue(Guid encounterId)
            => encounterId != Guid.Empty
               && _channel.Writer.TryWrite(new QueuedEvaluation(encounterId, 0, DateTime.UtcNow, RetryUntilClosed: true));

        /// <summary>
        /// Pemicu penilaian ulang dari tanda tangan atau addendum pengesahan (`BE-40`). Kunjungan
        /// yang belum tutup klinis dilepas tanpa diulang — penilaiannya menunggu penutupan.
        /// </summary>
        public bool EnqueueReevaluation(Guid encounterId)
            => encounterId != Guid.Empty
               && _channel.Writer.TryWrite(new QueuedEvaluation(encounterId, 0, DateTime.UtcNow, RetryUntilClosed: false));

        internal bool Requeue(QueuedEvaluation item, DateTime dueAtUtc)
            => _channel.Writer.TryWrite(item with { Attempt = item.Attempt + 1, DueAtUtc = dueAtUtc });
    }

    internal sealed record QueuedEvaluation(Guid EncounterId, int Attempt, DateTime DueAtUtc, bool RetryUntilClosed);

    /// <summary>
    /// Pembaca antrean <see cref="MedicalRecordCompletenessQueue"/>. Menilai setiap kunjungan dalam
    /// scope sendiri, di luar transaksi penutupan.
    /// </summary>
    /// <remarks>
    /// Id kunjungan masuk antrean SEBELUM transaksi penutupan di-commit, sehingga barisnya mungkin
    /// belum terlihat. Karena itu penilaian ditunda <c>MedicalRecordCompleteness:QueueDelaySeconds</c>
    /// (bawaan 3 detik) dan diulang dengan jeda berlipat sampai <c>QueueMaxAttempts</c> (bawaan 5).
    /// Penutupan yang akhirnya dibatalkan tidak pernah terlihat, dan id-nya dilepas sesudah batas itu.
    /// Log hanya memuat id kunjungan dan jumlah — tidak pernah isi kekurangan.
    /// </remarks>
    public sealed class MedicalRecordCompletenessQueueWorker : BackgroundService
    {
        private readonly MedicalRecordCompletenessQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MedicalRecordCompletenessQueueWorker> _logger;
        private readonly TimeSpan _jedaAwal;
        private readonly int _batasPercobaan;

        public MedicalRecordCompletenessQueueWorker(
            MedicalRecordCompletenessQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<MedicalRecordCompletenessQueueWorker> logger,
            IConfiguration configuration)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _jedaAwal = TimeSpan.FromSeconds(Math.Max(0, configuration.GetValue("MedicalRecordCompleteness:QueueDelaySeconds", 3)));
            _batasPercobaan = Math.Max(1, configuration.GetValue("MedicalRecordCompleteness:QueueMaxAttempts", 5));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                var jatuhTempo = item.DueAtUtc + (item.Attempt == 0 ? _jedaAwal : TimeSpan.Zero);
                var tunggu = jatuhTempo - DateTime.UtcNow;
                if (tunggu > TimeSpan.Zero)
                    await Task.Delay(tunggu, stoppingToken);

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<MedicalRecordCompletenessService>();

                    if (await service.EvaluateQueuedAsync(item.EncounterId, stoppingToken)
                        || !item.RetryUntilClosed)
                        continue;

                    if (item.Attempt + 1 < _batasPercobaan)
                    {
                        var jeda = TimeSpan.FromTicks(Math.Max(_jedaAwal.Ticks, TimeSpan.FromSeconds(1).Ticks) << (item.Attempt + 1));
                        _queue.Requeue(item, DateTime.UtcNow + jeda);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Penilaian kelengkapan kunjungan {EncounterId} dilepas dari antrean: penutupan klinis tidak terlihat sesudah {Attempts} percobaan.",
                            item.EncounterId, _batasPercobaan);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    // Satu kunjungan yang gagal tidak menghentikan antrean.
                    _logger.LogWarning(ex, "Penilaian kelengkapan kunjungan {EncounterId} dari antrean gagal.", item.EncounterId);
                }
            }
        }
    }
}
