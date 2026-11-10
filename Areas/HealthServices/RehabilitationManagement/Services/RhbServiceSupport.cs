using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>Hasil pembacaan satu kunjungan untuk konteks permintaan atau sesi rehab.</summary>
    /// <remarks>
    /// <see cref="PhysicallyDischarged"/> dipisah dari <see cref="IsOpen"/> supaya pemanggil dapat
    /// memilih kode penolakan yang tepat: kunjungan berakhir <c>RHB-VAL-051</c>, pasien rawat inap
    /// sudah pulang fisik <c>RHB-VAL-052</c> (<c>validation-matrix.md</c> V-SES-02/03).
    /// </remarks>
    public sealed record RhbEncounterCheck(
        bool Exists,
        bool BelongsToPatient,
        bool IsOpen,
        bool PhysicallyDischarged,
        EncounterType? EncounterType)
    {
        public bool IsValid => Exists && BelongsToPatient && IsOpen && !PhysicallyDischarged;
    }

    /// <summary>
    /// Penolong bersama service Rehab Medik (<c>BE-RHB-006</c>, <c>02-backend-architecture.md</c>):
    /// pemeriksaan kunjungan, kunci antrean, dan penerbitan nomor bisnis — pola
    /// <c>HmdServiceSupport</c>.
    /// </summary>
    /// <remarks>
    /// Tidak memegang state dan tidak didaftarkan di DI; setiap method menerima
    /// <see cref="ApplicationDbContext"/> milik service pemanggil sehingga tetap berjalan di dalam
    /// transaksi pemanggilnya. Kunjungan hanya <b>dibaca</b> (INT-RHB-01/02); rehab tidak pernah
    /// mengubah data pendaftaran.
    /// </remarks>
    public static class RhbServiceSupport
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;
        public const int SequenceDigits = 6;

        /// <summary>
        /// Status kunjungan yang menyatakan kunjungan sudah berakhir — sama dengan
        /// <c>HmdServiceSupport</c> dan <c>BbkEncounterStatusReader</c>.
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
        /// Menormalkan waktu kiriman klien menjadi UTC. Nilai tanpa zona dibaca sebagai waktu rumah
        /// sakit, bukan waktu server — pola <c>HmdServiceSupport.ToUtc</c>.
        /// </summary>
        public static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), BusinessTimeZone())
        };

        /// <summary>Tanggal hari ini menurut zona waktu rumah sakit.</summary>
        public static DateOnly TodayLocal() =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZone()));

        /// <summary>
        /// Menerjemahkan periode siap pakai (<c>today</c>, <c>last7days</c>, <c>thisMonth</c>,
        /// <c>thisYear</c>) atau tanggal awal/akhir menjadi rentang UTC setengah terbuka
        /// <c>[start, end)</c>. Mengembalikan pesan galat bila rentang tidak sah.
        /// </summary>
        public static (DateTime? Start, DateTime? End, string? Error) ResolveRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            switch (customPeriod?.Trim().ToLowerInvariant())
            {
                case "today":
                    return (today, today.AddDays(1), null);
                case "last7days":
                    return (today.AddDays(-6), today.AddDays(1), null);
                case "thismonth":
                    var month = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    return (month, month.AddMonths(1), null);
                case "thisyear":
                    var year = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return (year, year.AddYears(1), null);
            }

            var start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
            var end = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : (DateTime?)null;

            if (start.HasValue && end.HasValue && start >= end)
                return (null, null, "Tanggal awal tidak boleh melewati tanggal akhir.");

            return (start, end, null);
        }

        /// <summary>
        /// Kunjungan pasien yang masih terbuka: belum dihapus atau dibatalkan, statusnya belum
        /// berakhir, dan — untuk rawat inap — pasiennya belum pulang secara fisik (RHB-DEC-016).
        /// </summary>
        /// <remarks>
        /// Selalu tersaring ke satu pasien. Tidak ada jalur untuk menelusuri kunjungan pasien lain
        /// lewat modul rehab.
        /// </remarks>
        public static IQueryable<RegPatientEncounter> OpenEncountersOfPatient(ApplicationDbContext dbContext, Guid patientId) =>
            dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    !ClosedEncounterStatuses.Contains(x.EncounterStatus) &&
                    !(x.EncounterType == EncounterType.Inpatient &&
                      dbContext.Set<InpEpisode>()
                          .Where(e => e.EncounterId == x.Id && !e.IsDelete)
                          .OrderByDescending(e => e.CreateDateTime)
                          .Select(e => e.PhysicallyLeftAt)
                          .FirstOrDefault() != null));

        /// <summary>
        /// Memeriksa kunjungan pasien: ada, milik pasien itu, belum berakhir, dan — untuk rawat
        /// inap — pasiennya belum pulang secara fisik.
        /// </summary>
        public static async Task<RhbEncounterCheck> CheckEncounterAsync(
            ApplicationDbContext dbContext,
            Guid encounterId,
            Guid patientId,
            CancellationToken cancellationToken)
        {
            if (encounterId == Guid.Empty)
                return new RhbEncounterCheck(false, false, false, false, null);

            var encounter = await dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == encounterId && !x.IsDelete)
                .Select(x => new { x.PatientId, x.EncounterStatus, x.EncounterType, x.IsCancel })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return new RhbEncounterCheck(false, false, false, false, null);

            var belongs = encounter.PatientId == patientId;
            var open = !encounter.IsCancel && !ClosedEncounterStatuses.Contains(encounter.EncounterStatus);
            var physicallyDischarged = false;

            if (open && encounter.EncounterType == EncounterType.Inpatient)
            {
                var physicallyLeftAt = await dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => x.PhysicallyLeftAt)
                    .FirstOrDefaultAsync(cancellationToken);

                physicallyDischarged = physicallyLeftAt.HasValue;
            }

            return new RhbEncounterCheck(true, belongs, open, physicallyDischarged, encounter.EncounterType);
        }

        /// <summary>
        /// Mengantrekan pekerjaan pada kunci yang sama selama transaksi pemanggil
        /// (<c>pg_advisory_xact_lock</c>), polanya sama dengan <c>HmdServiceSupport</c>.
        /// </summary>
        /// <remarks>
        /// Kunci ini <b>mengantre</b>, bukan menolak. Beberapa kunci diambil berurutan menurut
        /// abjad supaya dua permintaan yang merebut kunci yang sama tidak saling menunggu. Di luar
        /// PostgreSQL langkah ini dilewati.
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
        /// Menerbitkan satu nomor bisnis rehab lewat provider bersama, dengan deret tahunan
        /// (contoh <c>RHB-REQ-2026-000001</c>). <c>null</c> berarti gagal dan tidak ada nomor yang
        /// terbit.
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
                        ResetPolicy: NumberSeriesResetPolicies.Yearly,
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

        public static bool IsUniqueViolation(DbUpdateException exception) =>
            (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
