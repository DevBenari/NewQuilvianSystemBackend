using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bacaan lokasi bed pasien untuk modul lain (<c>BE-RWI-177</c>, backend 12.7). Satu-satunya cara
    /// Kamar Operasi membaca lokasi bed — OK tidak membaca tabel penempatan Rawat Inap secara langsung.
    /// </summary>
    /// <remarks>
    /// "Menempati bed aktif di unit X" berarti: episode pasien masih hadir (<c>Admitted</c>, atau
    /// <c>DischargePending</c> yang kepergiannya belum dicatat — sama dengan
    /// <c>IX_InpEpisode_PatientId_Present</c>) dan penempatan berjalannya — belum berakhir, belum
    /// digantikan koreksi — berada di unit X.
    ///
    /// Contoh <c>UAT-RWF-13</c>: Budi masih di Melati (bangsal) saat OK mengirim serah terima ke ICU.
    /// <c>IsPatientInUnitAsync(Budi, ICU)</c> = <c>false</c>, sehingga perawat ICU diminta memindahkan
    /// pasien lewat Transfer Pasien lebih dulu.
    /// </remarks>
    public sealed class InpPatientLocationQuery
    {
        private readonly ApplicationDbContext _dbContext;

        public InpPatientLocationQuery(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Apakah pasien sedang menempati bed aktif di unit layanan yang disebut.</summary>
        public Task<bool> IsPatientInUnitAsync(Guid patientId, Guid serviceUnitId,
            CancellationToken cancellationToken = default)
        {
            if (patientId == Guid.Empty || serviceUnitId == Guid.Empty)
                return Task.FromResult(false);

            return ActivePlacements(patientId)
                .AnyAsync(x => x.ServiceUnitId == serviceUnitId, cancellationToken);
        }

        /// <summary>Unit layanan bed aktif pasien saat ini; kosong bila pasien tidak sedang menempati bed.</summary>
        public Task<InpPatientCurrentLocation?> GetCurrentLocationAsync(Guid patientId,
            CancellationToken cancellationToken = default)
        {
            return ActivePlacements(patientId)
                .OrderByDescending(x => x.StartDateTime)
                .Select(x => new InpPatientCurrentLocation(
                    x.EpisodeId,
                    x.ServiceUnitId,
                    x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    x.Room != null ? x.Room.RoomName : null,
                    x.Bed != null ? x.Bed.BedName : null))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Lokasi bed aktif beberapa pasien sekaligus, untuk daftar serah terima per unit.
        /// </summary>
        public async Task<Dictionary<Guid, InpPatientCurrentLocation>> GetCurrentLocationsAsync(
            IReadOnlyCollection<Guid> patientIds, CancellationToken cancellationToken = default)
        {
            if (patientIds.Count == 0)
                return new Dictionary<Guid, InpPatientCurrentLocation>();

            var rows = await _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.Episode != null && patientIds.Contains(x.Episode.PatientId) &&
                    x.EndDateTime == null && !x.IsDelete && x.IsActive && !x.IsSuperseded &&
                    x.SupersededByCorrectionId == null && !x.Episode.IsDelete &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     (x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending && x.Episode.PhysicallyLeftAt == null)))
                .Select(x => new
                {
                    PatientId = x.Episode!.PatientId,
                    x.StartDateTime,
                    Location = new InpPatientCurrentLocation(
                        x.EpisodeId,
                        x.ServiceUnitId,
                        x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                        x.Room != null ? x.Room.RoomName : null,
                        x.Bed != null ? x.Bed.BedName : null)
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.PatientId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StartDateTime).First().Location);
        }

        /// <summary>
        /// Apakah pasien sedang menempati bed aktif pada episode ini — <c>BE-RWI-193</c>,
        /// <c>INV-RWA-11</c>, <c>RWI-DEC-255</c>.
        /// </summary>
        /// <remarks>
        /// Aturan "menempati bed" sama dengan method lain di kelas ini: episode masih hadir dan
        /// penempatan berjalannya belum berakhir maupun digantikan koreksi. Pemesanan bed saja
        /// <b>tidak</b> dihitung. Contoh: bed Melati 03 B masih dipesan untuk Tn. Budi pukul 10.20 →
        /// <c>false</c>; Budi ditempatkan pukul 10.35 → <c>true</c>.
        /// </remarks>
        public Task<bool> HasActivePlacementAsync(Guid episodeId, CancellationToken cancellationToken = default)
        {
            if (episodeId == Guid.Empty)
                return Task.FromResult(false);

            return _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .AnyAsync(x => x.EpisodeId == episodeId && x.Episode != null && !x.Episode.IsDelete &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     (x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending && x.Episode.PhysicallyLeftAt == null)) &&
                    x.EndDateTime == null && !x.IsDelete && x.IsActive && !x.IsSuperseded &&
                    x.SupersededByCorrectionId == null, cancellationToken);
        }

        private IQueryable<InpBedPlacement> ActivePlacements(Guid patientId) =>
            _dbContext.Set<InpBedPlacement>()
                .AsNoTracking()
                .Where(x => x.Episode != null && x.Episode.PatientId == patientId && !x.Episode.IsDelete &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     (x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending && x.Episode.PhysicallyLeftAt == null)) &&
                    x.EndDateTime == null && !x.IsDelete && x.IsActive && !x.IsSuperseded &&
                    x.SupersededByCorrectionId == null);
    }

    /// <summary>Lokasi bed aktif seorang pasien.</summary>
    public sealed record InpPatientCurrentLocation(
        Guid EpisodeId,
        Guid ServiceUnitId,
        string? ServiceUnitName,
        string? RoomName,
        string? BedName);
}
