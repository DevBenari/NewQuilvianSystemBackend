using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pembaca indikator ketepatan ringkasan pulang untuk rekam medis (`BE-50`, `INT-RM-18`;
    /// persetujuan pemilik Rawat Inap `RM-DEC-109`). Aturan baris: `02-backend-architecture.md`
    /// A6.9.1 rekam medis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu baris per episode yang tidak dihapus dan tidak batal, yang pasiennya sudah pulang —
    /// keluar ruangan (<c>PhysicallyLeftAt</c>) atau, bila itu kosong, episode ditutup
    /// (<c>ClosedAt</c>) — dan jam mulainya jatuh di rentang. Episode <c>DischargePending</c> yang
    /// pasiennya belum keluar tidak terbaca (`RM-DEC-107`).
    /// </para>
    /// <para>
    /// Tanda tangan pertama = nilai terkecil dari <c>PreviousSignedAt</c> setiap versi dan
    /// <c>SignedAt</c> saat ini (`RM-DEC-105`). DPJP aktif hanya dibaca untuk ringkasan yang belum
    /// ditandatangani, dengan aturan yang sama dengan <c>GUARD-INP-03</c>
    /// (<see cref="InpEpisodeService.GetActiveDoctorIdAsync"/>).
    /// </para>
    /// <para>
    /// Baca saja; tidak memeriksa izin modul Rawat Inap; tidak mengembalikan identitas pasien,
    /// kunjungan, maupun isi ringkasan.
    /// </para>
    /// </remarks>
    public sealed class InpDischargeSummaryTimelinessSource : IDischargeSummaryTimelinessSource
    {
        private readonly ApplicationDbContext _dbContext;

        public InpDischargeSummaryTimelinessSource(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<DischargeSummaryTimelinessRow>> ReadAsync(
            DateTime startFromUtc,
            DateTime startToUtcExclusive,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.EpisodeStatus != InpEpisodeStatus.Cancelled
                            && (x.PhysicallyLeftAt != null || x.ClosedAt != null)
                            && (x.PhysicallyLeftAt ?? x.ClosedAt) >= startFromUtc
                            && (x.PhysicallyLeftAt ?? x.ClosedAt) < startToUtcExclusive)
                .Select(x => new { x.Id, x.PhysicallyLeftAt, x.ClosedAt })
                .ToListAsync(cancellationToken);

            if (episode.Count == 0)
                return [];

            var idEpisode = episode.Select(x => x.Id).ToList();

            var ringkasan = await _dbContext.Set<InpDischargeSummary>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && idEpisode.Contains(x.EpisodeId))
                .Select(x => new { x.Id, x.EpisodeId, x.SignedAt, x.SignedByDoctorId })
                .ToListAsync(cancellationToken);

            var idRingkasan = ringkasan.Select(x => x.Id).ToList();
            var versi = await _dbContext.Set<InpDischargeSummaryRevision>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && idRingkasan.Contains(x.DischargeSummaryId))
                .Select(x => new { x.DischargeSummaryId, x.PreviousSignedAt, x.PreviousSignedByDoctorId })
                .ToListAsync(cancellationToken);

            var ringkasanPerEpisode = ringkasan
                .GroupBy(x => x.EpisodeId)
                .ToDictionary(g => g.Key, g => g.First());
            var versiPerRingkasan = versi.ToLookup(x => x.DischargeSummaryId);

            // Tanda tangan pertama per episode (null = belum pernah ditandatangani).
            var pertama = new Dictionary<Guid, (DateTime Waktu, Guid? Dokter)>();
            foreach (var r in ringkasan)
            {
                var kandidat = versiPerRingkasan[r.Id]
                    .Select(v => (Waktu: v.PreviousSignedAt, Dokter: v.PreviousSignedByDoctorId == Guid.Empty ? (Guid?)null : v.PreviousSignedByDoctorId))
                    .ToList();
                if (r.SignedAt.HasValue)
                    kandidat.Add((r.SignedAt.Value, r.SignedByDoctorId));

                if (kandidat.Count > 0)
                    pertama[r.EpisodeId] = kandidat.OrderBy(x => x.Waktu).First();
            }

            var belumDitandatangani = idEpisode.Where(id => !pertama.ContainsKey(id)).ToList();
            var dpjp = belumDitandatangani.Count == 0
                ? new Dictionary<Guid, Guid>()
                : (await _dbContext.Set<InpDoctorAssignment>()
                    .AsNoTracking()
                    .Where(x => belumDitandatangani.Contains(x.EpisodeId)
                                && x.AssignmentRole == InpDoctorAssignmentRole.Dpjp
                                && x.EndDateTime == null
                                && !x.IsDelete)
                    .Select(x => new { x.EpisodeId, x.DoctorId, x.SequenceNumber })
                    .ToListAsync(cancellationToken))
                    .GroupBy(x => x.EpisodeId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SequenceNumber).First().DoctorId);

            return episode
                .Select(e =>
                {
                    var dariTutup = e.PhysicallyLeftAt == null;
                    var mulai = e.PhysicallyLeftAt ?? e.ClosedAt!.Value;
                    if (pertama.TryGetValue(e.Id, out var tanda))
                        return new DischargeSummaryTimelinessRow(e.Id, mulai, dariTutup, tanda.Waktu, tanda.Dokter, null);

                    return new DischargeSummaryTimelinessRow(
                        e.Id, mulai, dariTutup, null, null, dpjp.TryGetValue(e.Id, out var d) ? d : null);
                })
                .ToList();
        }
    }
}
