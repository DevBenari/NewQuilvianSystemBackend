using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Pelaku sebuah tindakan, diturunkan dari pengguna terautentikasi — tidak pernah dari isian
    /// permintaan. <see cref="Principal"/> dibawa supaya identitas dokter dapat dibaca dari klaim.
    /// </summary>
    public sealed record HmdActor(Guid UserId, System.Security.Claims.ClaimsPrincipal? Principal);

    /// <summary>Hasil pembacaan satu kunjungan untuk konteks sesi atau permintaan HD.</summary>
    public sealed record HmdEncounterCheck(bool Exists, bool BelongsToPatient, bool IsOpen)
    {
        public bool IsValid => Exists && BelongsToPatient && IsOpen;
    }

    /// <summary>
    /// Penolong bersama service Hemodialisa. Tidak memegang state dan tidak didaftarkan di DI;
    /// setiap method menerima <see cref="ApplicationDbContext"/> milik service pemanggil sehingga
    /// tetap berjalan di dalam transaksi pemanggilnya.
    /// </summary>
    public static class HmdServiceSupport
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        /// <summary>Deret nomor bisnis milik Hemodialisa pada provider bersama (<c>DEC-PLT-005</c>).</summary>
        public const string OrderSequenceKey = "HMD_ORDER";
        public const string OrderNumberPrefix = "HD-ORD";
        public const string EpisodeSequenceKey = "HMD_EPISODE";
        public const string EpisodeNumberPrefix = "HD-EP";
        public const string SessionSequenceKey = "HMD_SESSION";
        public const string SessionNumberPrefix = "HD-SES";

        /// <summary>
        /// Deret nomor catatan tanda vital yang ditulis Hemodialisa ke <c>TrxPatientVitalSign</c>.
        /// Pembentuk nomor lama milik Clinical Management memakai <c>Count + 1</c> dan tidak boleh
        /// ditiru kode baru (<c>QBE-CODE-003</c>); deret ini tetap memakai provider atomik.
        /// </summary>
        public const string VitalSignSequenceKey = "HMD_VITAL_SIGN";
        public const string VitalSignNumberPrefix = "VTS-HD";

        public const int SequenceDigits = 8;

        /// <summary>
        /// Status kunjungan yang menyatakan kunjungan sudah berakhir — sama dengan
        /// <c>BbkEncounterStatusReader</c> (<c>DEC-BD-014</c>).
        /// </summary>
        private static readonly EncounterStatus[] ClosedEncounterStatuses =
        {
            EncounterStatus.Completed,
            EncounterStatus.Cancelled,
            EncounterStatus.NoShow
        };

        public static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;
            return (pageNumber, pageSize);
        }

        public static int TotalPage(int totalData, int pageSize) =>
            pageSize <= 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize);

        public static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>Zona waktu rumah sakit — sama dengan <c>NumberSeriesAllocator</c>.</summary>
        public static TimeZoneInfo BusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        /// <summary>
        /// Menormalkan waktu kiriman klien menjadi UTC sebelum disimpan ke kolom
        /// <c>timestamp with time zone</c>. Nilai tanpa zona dibaca sebagai waktu rumah sakit,
        /// bukan waktu server — supaya hasilnya sama di mesin mana pun aplikasi berjalan.
        /// </summary>
        public static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), BusinessTimeZone())
        };

        /// <summary>Tanggal menurut zona waktu rumah sakit dari sebuah waktu UTC.</summary>
        public static DateOnly LocalDate(DateTime utc) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), BusinessTimeZone()));

        /// <summary>Tanggal hari ini menurut zona waktu rumah sakit.</summary>
        public static DateOnly TodayLocal() => LocalDate(DateTime.UtcNow);

        /// <summary>
        /// Memeriksa kunjungan pasien: ada, milik pasien itu, dan belum berakhir.
        /// </summary>
        /// <remarks>
        /// Untuk kunjungan rawat inap, pasien yang sudah benar-benar pulang dianggap kunjungannya
        /// berakhir walaupun status administratifnya belum ditutup — mengikuti
        /// <c>BbkEncounterStatusReader</c>.
        /// </remarks>
        public static async Task<HmdEncounterCheck> CheckEncounterAsync(
            ApplicationDbContext dbContext,
            Guid encounterId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (encounterId == Guid.Empty)
                return new HmdEncounterCheck(false, false, false);

            var encounter = await dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new { x.PatientId, x.EncounterStatus, x.EncounterType })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return new HmdEncounterCheck(false, false, false);

            var belongs = encounter.PatientId == patientId;
            var open = !ClosedEncounterStatuses.Contains(encounter.EncounterStatus);

            if (open && encounter.EncounterType == EncounterType.Inpatient)
            {
                var physicallyLeft = await dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => x.PhysicallyLeftAt)
                    .FirstOrDefaultAsync(cancellationToken);

                open = !physicallyLeft.HasValue;
            }

            return new HmdEncounterCheck(true, belongs, open);
        }

        /// <summary>
        /// Mengantrekan pekerjaan pada kunci yang sama selama transaksi pemanggil —
        /// <c>pg_advisory_xact_lock</c>, polanya sama dengan <c>NumberSeriesAllocator</c> dan
        /// <c>BbkBloodOrderService</c>.
        /// </summary>
        /// <remarks>
        /// Kunci ini <b>mengantre</b>, bukan menolak. Beberapa kunci diambil berurutan menurut
        /// abjad supaya dua permintaan yang merebut kunci yang sama tidak saling menunggu
        /// (deadlock). Di luar PostgreSQL langkah ini dilewati.
        /// </remarks>
        public static async Task AcquireLocksAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken,
            params string[] keys)
        {
            if (!dbContext.Database.IsNpgsql())
                return;

            foreach (var key in keys.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [key],
                    cancellationToken);
            }
        }

        /// <summary>
        /// Menerbitkan satu nomor bisnis lewat provider bersama. <c>null</c> berarti gagal; nol
        /// nomor terbit pada kegagalan itu (<c>INV-PLT-002</c>).
        /// </summary>
        public static async Task<string?> AllocateNumberAsync(
            NumberSeriesAllocator allocator,
            string sequenceKey,
            string prefix,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                return await allocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: sequenceKey,
                        Prefix: prefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: SequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return null;
            }
        }

        public static bool IsUniqueViolation(DbUpdateException exception)
        {
            var postgres = exception.InnerException as PostgresException;
            return postgres?.SqlState == PostgresErrorCodes.UniqueViolation;
        }

        /// <summary>Menandai ulang entity yang tertinggal di change tracker setelah penyimpanan gagal.</summary>
        public static void DetachFailedEntries(ApplicationDbContext dbContext)
        {
            foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    entry.State = EntityState.Detached;
            }
        }

        /// <summary>Keputusan isolasi yang sedang berlaku pada episode, atau <c>None</c>.</summary>
        public static async Task<Enums.HmdIsolationRequirement> GetActiveIsolationAsync(
            ApplicationDbContext dbContext,
            Guid episodeId,
            DateOnly onDate,
            CancellationToken cancellationToken)
        {
            var requirement = await dbContext.Set<HmdIsolationDecision>()
                .AsNoTracking()
                .Where(x =>
                    x.EpisodeId == episodeId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveFrom <= onDate &&
                    (x.EffectiveTo == null || x.EffectiveTo >= onDate))
                .OrderByDescending(x => x.DecidedAt)
                .Select(x => (Enums.HmdIsolationRequirement?)x.Requirement)
                .FirstOrDefaultAsync(cancellationToken);

            return requirement ?? Enums.HmdIsolationRequirement.None;
        }

        /// <summary>Pengaturan unit HD, atau <c>null</c> bila belum ada — tidak pernah diisi nilai bawaan diam-diam.</summary>
        public static Task<HmdSetting?> FindSettingAsync(
            ApplicationDbContext dbContext,
            Guid serviceUnitId,
            CancellationToken cancellationToken) =>
            dbContext.Set<HmdSetting>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ServiceUnitId == serviceUnitId && !x.IsDelete, cancellationToken);
    }
}
