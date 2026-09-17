using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Menutup kunjungan yang terbentuk dari kiosk tetapi <b>tidak pernah dilanjutkan</b> di
    /// unit tujuannya, ketika hari layanan berakhir (<c>BE-EXT-05</c>, <c>LAB-DEC-054</c>,
    /// <c>LAB-DEC-058</c>, <c>LAB-DEC-059</c>).
    ///
    /// <b>Kesalahan di sini tidak muncul sebagai galat.</b> Ia muncul sebagai pasien yang
    /// pendaftarannya hilang saat ia sedang duduk menunggu dipanggil, dan yang pertama
    /// mengetahuinya adalah pasien itu — bukan sistem. Karena itu penyaringnya dibangun
    /// sedemikian rupa sehingga <b>tidak mungkin melebar</b>, bukan sekadar ditulis sempit:
    ///
    /// <list type="number">
    /// <item>Sasaran diambil dari <see cref="IEncounterContinuationProbe.TargetService"/>,
    /// bukan dari konfigurasi maupun konstanta di berkas ini. Unit yang tidak mendaftarkan
    /// penjawab tidak punya nilai yang dapat disaring, sehingga kunjungannya tidak dapat
    /// tersentuh walaupun seseorang salah menyetel konfigurasi.</item>
    /// <item>Nol penjawab terdaftar berarti <b>nol kunjungan ditutup</b>. Tanpa penjawab tidak
    /// ada cara membedakan yang dilanjutkan dari yang ditinggalkan, dan menebak berarti menutup
    /// kunjungan pasien yang sedang menunggu.</item>
    /// <item>Kunjungan yang tidak berasal dari sesi kiosk tidak pernah ikut terbaca.</item>
    /// </list>
    ///
    /// Pada database hari ini penyaring itu cocok <b>nol baris</b>, sementara penyaring yang
    /// lebih longgar akan menyapu 15 kunjungan kiosk nyata dan menyentuh 91 kunjungan berstatus
    /// <c>WaitingForNurse</c> — pasien poliklinik yang sedang menunggu dipanggil.
    ///
    /// <b>Biaya pendaftaran tidak digugurkan di sini, dan itu bukan kelalaian.</b> Registrasi
    /// nol menerbitkan fakta kelayakan tagih — penerbitnya hanya Klinis, Laboratorium, Farmasi,
    /// dan Radiologi — dan <c>DefaultRegistrationFee</c> hanya hidup sebagai data induk. Tidak
    /// ada tagihan yang perlu digugurkan karena tidak pernah ada yang terbit. Ditulis terang
    /// supaya tidak ada yang menambahkan pembatalan tagihan yang tidak punya sasaran.
    /// </summary>
    public class KioskEncounterClosureService
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly QueueRealtimeService _queueRealtimeService;
        private readonly LoggerService _loggerService;
        private readonly IReadOnlyList<IEncounterContinuationProbe> _probes;

        public KioskEncounterClosureService(
            ApplicationDbContext dbContext,
            QueueRealtimeService queueRealtimeService,
            LoggerService loggerService,
            IEnumerable<IEncounterContinuationProbe> probes)
        {
            _dbContext = dbContext;
            _queueRealtimeService = queueRealtimeService;
            _loggerService = loggerService;
            _probes = probes.ToList();
        }

        /// <summary>
        /// Menutup kunjungan kiosk yang tidak dilanjutkan untuk hari layanan
        /// <paramref name="serviceDate"/> dan beberapa hari sebelumnya.
        /// </summary>
        /// <param name="serviceDate">
        /// Tanggal hari layanan yang sudah berakhir, dalam kalender WIB. Kunjungan yang
        /// tanggalnya <b>melewati</b> tanggal ini tidak pernah ikut tersentuh — hari layanannya
        /// belum berakhir.
        /// </param>
        /// <param name="options">Pengaturan yang berlaku pada putaran ini.</param>
        /// <param name="cancellationToken">Token penghentian dari penjadwal.</param>
        public async Task<KioskEncounterClosureResult> CloseAbandonedAsync(
            DateTime serviceDate,
            KioskEncounterClosureOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);

            var result = new KioskEncounterClosureResult();

            // Penjagaan pertama, dan yang paling menentukan. Tanpa penjawab, "tidak
            // dilanjutkan" tidak dapat dibedakan dari "sedang menunggu".
            if (_probes.Count == 0)
            {
                return result;
            }

            var upperBound = ToUtcDate(serviceDate);
            var lowerBound = upperBound.AddDays(-Math.Max(0, options.LookBackDays));
            var batchSize = Math.Clamp(options.BatchSize, 1, 1000);
            var actorUserId = options.SystemActorUserId ?? Guid.Empty;
            var reason = string.IsNullOrWhiteSpace(options.NoShowReason)
                ? "Tidak dilanjutkan sampai hari layanan berakhir."
                : options.NoShowReason.Trim();

            foreach (var probe in _probes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var target = probe.TargetService;

                // Sasaran yang belum dinyatakan tidak menunjuk unit mana pun, sehingga tidak
                // ada kunjungan yang boleh ditutup atas namanya.
                if (target == KioskServiceTarget.Unknown)
                {
                    continue;
                }

                var closed = await CloseForTargetAsync(
                    probe,
                    target,
                    lowerBound,
                    upperBound,
                    batchSize,
                    actorUserId,
                    reason,
                    cancellationToken);

                result.Add(target, closed);
            }

            return result;
        }

        private async Task<int> CloseForTargetAsync(
            IEncounterContinuationProbe probe,
            KioskServiceTarget target,
            DateTime lowerBound,
            DateTime upperBound,
            int batchSize,
            Guid actorUserId,
            string reason,
            CancellationToken cancellationToken)
        {
            // Kandidat, bukan sasaran. Yang benar-benar ditutup baru ditentukan sesudah
            // penjawab unitnya ditanya.
            var candidates = await _dbContext.Set<RegPatientEncounter>()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.CancelledAt == null &&
                    x.NoShowAt == null &&
                    x.CompletedAt == null &&
                    // Kedatangan yang sudah dicatat petugas adalah tanda dilanjutkan yang
                    // dimiliki Registrasi sendiri, terlepas dari jawaban unitnya.
                    x.CheckedInAt == null &&
                    x.EncounterStatus != EncounterStatus.Completed &&
                    x.EncounterStatus != EncounterStatus.Cancelled &&
                    x.EncounterStatus != EncounterStatus.NoShow &&
                    x.KioskScanSessionId != null &&
                    x.EncounterDate >= lowerBound &&
                    x.EncounterDate <= upperBound &&
                    _dbContext.Set<TrxKioskScanSession>().Any(s =>
                        s.Id == x.KioskScanSessionId!.Value &&
                        !s.IsDelete &&
                        s.TargetService == target))
                .OrderBy(x => x.EncounterDate)
                .ThenBy(x => x.RegisteredAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (candidates.Count == 0)
            {
                return 0;
            }

            var candidateIds = candidates.Select(x => x.Id).ToList();
            var continuedIds = await probe.FindContinuedEncounterIdsAsync(candidateIds, cancellationToken);

            var abandoned = candidates
                .Where(x => !continuedIds.Contains(x.Id))
                .ToList();

            if (abandoned.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            var abandonedIds = abandoned.Select(x => x.Id).ToList();

            var queues = await _dbContext.Set<TrxQueue>()
                .Where(x =>
                    abandonedIds.Contains(x.EncounterId) &&
                    !x.IsDelete &&
                    x.CompletedAt == null &&
                    x.CancelledAt == null &&
                    x.NoShowAt == null)
                .ToListAsync(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            foreach (var encounter in abandoned)
            {
                encounter.EncounterStatus = EncounterStatus.NoShow;
                encounter.NoShowAt = now;
                encounter.NoShowByUserId = actorUserId;
                encounter.NoShowReason = reason;
                encounter.IsActive = false;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;
            }

            foreach (var queue in queues)
            {
                // Tidak dibatalkan, melainkan ditandai tidak datang. Keduanya berbeda sebab,
                // dan laporan antrean membedakannya.
                queue.QueueStatus = QueueStatus.NoShow;
                queue.NoShowAt = now;
                queue.NoShowByUserId = actorUserId;
                queue.NoShowReason = reason;
                queue.IsActive = false;
                queue.UpdateDateTime = now;
                queue.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "KioskEncounterClosure.CloseAbandoned",
                "Kunjungan kiosk yang tidak dilanjutkan ditutup otomatis pada akhir hari layanan.",
                new
                {
                    TargetService = target.ToString(),
                    ClosedEncounterCount = abandoned.Count,
                    ClosedQueueCount = queues.Count,
                    ContinuedEncounterCount = continuedIds.Count,
                    ServiceDateUpperBound = upperBound,
                    ServiceDateLowerBound = lowerBound,
                    Reason = reason
                });

            // Kegagalan notifikasi realtime tidak boleh membatalkan penutupan yang sudah
            // commit, dan tidak boleh menghentikan sisa putaran.
            foreach (var queue in queues)
            {
                try
                {
                    await _queueRealtimeService.NotifyQueueChangedAsync(
                        "QueueNoShow",
                        queue,
                        actorUserId,
                        "Kunjungan kiosk ditutup otomatis karena tidak dilanjutkan.");
                }
                catch (Exception ex)
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "KioskEncounterClosure.Notify",
                        "Notifikasi antrean gagal dikirim setelah penutupan otomatis.",
                        ex,
                        new { queue.Id, queue.EncounterId });
                }
            }

            return abandoned.Count;
        }

        private static DateTime ToUtcDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }
    }

    /// <summary>Hasil satu putaran penutupan, dipecah menurut unit tujuannya.</summary>
    public sealed class KioskEncounterClosureResult
    {
        private readonly Dictionary<KioskServiceTarget, int> _closedByTarget = new();

        public IReadOnlyDictionary<KioskServiceTarget, int> ClosedByTarget => _closedByTarget;

        public int TotalClosed => _closedByTarget.Values.Sum();

        public void Add(KioskServiceTarget target, int count)
        {
            if (count <= 0)
            {
                return;
            }

            _closedByTarget[target] = _closedByTarget.TryGetValue(target, out var existing)
                ? existing + count
                : count;
        }
    }
}
