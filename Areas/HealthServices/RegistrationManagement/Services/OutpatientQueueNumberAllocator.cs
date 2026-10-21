using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Konfigurasi penomoran antrean Rawat Jalan, section
    /// <c>HealthServices:Registration:QueueNumber</c>.
    /// </summary>
    /// <remarks>
    /// RJ-PRI-DEC-004 (menggantikan RJ-DOC-DEC-056): nomor prioritas adalah kelipatan N tanpa
    /// batas atas, dan nomor Reguler tidak pernah kelipatan N. Daftar cadangan tetap
    /// <c>ReservedPriorityNumbers</c> sudah tidak dipakai.
    /// </remarks>
    public class OutpatientQueueNumberOptions
    {
        public const string SectionName = "HealthServices:Registration:QueueNumber";

        /// <summary>Dipakai bila konfigurasi kosong. Satu-satunya tempat nilai bawaan ini.</summary>
        public const int DefaultPriorityNumberMultiple = 5;

        /// <summary>Batas bawah yang sah; kelipatan 1 akan menghabiskan seluruh nomor Reguler.</summary>
        public const int MinimumPriorityNumberMultiple = 2;

        /// <summary>Kelipatan nomor prioritas. Kosong berarti memakai bawaan.</summary>
        public int? PriorityNumberMultiple { get; set; }

        public int ResolvePriorityNumberMultiple() =>
            PriorityNumberMultiple ?? DefaultPriorityNumberMultiple;
    }

    public sealed record OutpatientQueueNumberAllocation(
        int QueueNumber,
        Guid QueueScopeKey,
        bool IsReservedPriorityNumber);

    /// <summary>
    /// Alokator nomor antrean Rawat Jalan yang dipakai bersama kiosk dan petugas
    /// (RJ-DOC-DEC-056). Menggantikan <c>MAX(QueueNumber)+1</c> di controller.
    ///
    /// Cakupan nomor sama dengan perilaku lama: per tanggal dan service unit, lalu digabung
    /// per Nurse Station Cluster bila poliklinik masuk cluster, per poliklinik bila tidak,
    /// atau seluruh service unit bila tanpa poliklinik.
    ///
    /// Wajib dipanggil di dalam transaction: advisory lock dilepas saat transaction selesai,
    /// sehingga pendaftaran simultan antre dan tidak mendapat nomor yang sama. Unique index
    /// <c>(QueueDate, ServiceUnitId, QueueScopeKey, QueueNumber)</c> menjadi pengaman terakhir.
    /// </summary>
    public class OutpatientQueueNumberAllocator
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OutpatientQueueNumberOptions _options;

        public OutpatientQueueNumberAllocator(
            ApplicationDbContext dbContext,
            IOptions<OutpatientQueueNumberOptions>? options = null)
        {
            _dbContext = dbContext;
            _options = options?.Value ?? new OutpatientQueueNumberOptions();
        }

        public async Task<OutpatientQueueNumberAllocation> AllocateAsync(
            DateTime queueDate,
            Guid serviceUnitId,
            Guid? clinicId,
            bool isPriorityQueue,
            CancellationToken cancellationToken = default)
        {
            var date = DateTime.SpecifyKind(queueDate.Date, DateTimeKind.Utc);
            var normalizedClinicId = clinicId.HasValue && clinicId.Value != Guid.Empty
                ? clinicId
                : null;

            if (_dbContext.Database.IsNpgsql())
            {
                if (_dbContext.Database.CurrentTransaction == null)
                {
                    throw new InvalidOperationException(
                        "Alokasi nomor antrean harus berjalan di dalam transaction.");
                }

                // Satu kunci per tanggal dan service unit: mencakup semua bentuk cakupan,
                // termasuk poliklinik yang masuk lebih dari satu cluster.
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));",
                    [$"REG_QUEUE_NUMBER_{date:yyyyMMdd}_{serviceUnitId:N}"],
                    cancellationToken);
            }

            var scope = await ResolveScopeAsync(serviceUnitId, normalizedClinicId, cancellationToken);

            // Nomor yang pernah terbit tetap terpakai walau batal, tidak hadir, selesai,
            // atau dihapus — tidak pernah dipakai ulang.
            var scopeQuery = _dbContext.Set<RegQueue>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.QueueDate == date && x.ServiceUnitId == serviceUnitId);

            if (scope.ClinicIds != null)
            {
                var clinicIds = scope.ClinicIds;
                scopeQuery = scopeQuery.Where(x => x.ClinicId.HasValue && clinicIds.Contains(x.ClinicId.Value));
            }

            var consumed = (await scopeQuery
                    .Select(x => x.QueueNumber)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var number = PickNumber(consumed, _options.ResolvePriorityNumberMultiple(), isPriorityQueue);

            return new OutpatientQueueNumberAllocation(number, scope.ScopeKey, isPriorityQueue);
        }

        /// <summary>
        /// RJ-PRI-DEC-004. Prioritas: kelipatan N terkecil yang belum terpakai (tidak pernah
        /// habis). Reguler: nomor terkecil yang belum terpakai dan bukan kelipatan N.
        /// Contoh N=5 dengan 1, 2, 3, 5 terpakai: prioritas 10, Reguler 4.
        /// </summary>
        internal static int PickNumber(IReadOnlySet<int> consumed, int multiple, bool isPriorityQueue)
        {
            if (isPriorityQueue)
            {
                var priorityNumber = multiple;
                while (consumed.Contains(priorityNumber))
                {
                    priorityNumber += multiple;
                }

                return priorityNumber;
            }

            var candidate = 1;
            while (candidate % multiple == 0 || consumed.Contains(candidate))
            {
                candidate++;
            }

            return candidate;
        }

        private async Task<(Guid ScopeKey, List<Guid>? ClinicIds)> ResolveScopeAsync(
            Guid serviceUnitId,
            Guid? clinicId,
            CancellationToken cancellationToken)
        {
            if (!clinicId.HasValue)
            {
                return (serviceUnitId, null);
            }

            var clusterIds = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ClinicId == clinicId.Value)
                .Select(x => x.NurseStationClusterId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (clusterIds.Count == 0)
            {
                return (clinicId.Value, new List<Guid> { clinicId.Value });
            }

            var clinicIdsInCluster = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && clusterIds.Contains(x.NurseStationClusterId))
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!clinicIdsInCluster.Contains(clinicId.Value))
            {
                clinicIdsInCluster.Add(clinicId.Value);
            }

            return (clusterIds.Min(), clinicIdsInCluster);
        }
    }
}
