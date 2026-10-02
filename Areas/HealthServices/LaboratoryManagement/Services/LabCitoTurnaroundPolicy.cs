using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Batas waktu cito yang berlaku bagi setiap jenis pemeriksaan — <b>satu-satunya</b> pembaca
    /// <c>LabValueBound.CitoTurnaroundMinutes</c> untuk menilai keterlambatan (<c>INV-57</c>,
    /// <c>02-backend-architecture.md</c> 23.4).
    ///
    /// <para>
    /// Dipindah <b>apa adanya</b> dari <c>LabWorklistService.BatasWaktuCitoAsync</c>
    /// (<c>BE-LAB-82</c>) supaya daftar pantau cito dan laporan waktu penyelesaian memakai batas
    /// yang sama. Dua salinan fungsi ini akan bercabang tanpa satu galat pun.
    /// </para>
    ///
    /// <para>
    /// <b>Aturan pemilihan — jangan dirapikan tanpa keputusan.</b> <c>LabValueBound</c> dipecah
    /// menurut jenis kelamin dan kelompok umur untuk keperluan batas nilai, sementara batas waktu
    /// cito adalah janji layanan yang tidak bergantung pada keduanya. Blueprint tidak menyebut baris
    /// mana yang berlaku, sehingga yang dipakai adalah baris umum — <c>All</c> tanpa kelompok umur —
    /// dan bila baris itu tidak mengisinya, nilai terkecil di antara baris aktif lainnya. Memilih
    /// yang terkecil berarti memilih janji yang paling ketat, bukan yang paling longgar. Mengubah
    /// aturan ini mengubah daftar pantau <b>dan</b> laporan sekaligus.
    /// </para>
    ///
    /// <para>
    /// Batas yang dipakai adalah yang berlaku <b>saat dibaca</b>, bukan batas historis
    /// (23.10 butir 5).
    /// </para>
    /// </summary>
    public class LabCitoTurnaroundPolicy
    {
        private readonly ApplicationDbContext _dbContext;

        public LabCitoTurnaroundPolicy(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Batas waktu cito dalam menit bagi setiap jenis pemeriksaan pada
        /// <paramref name="procedureIds"/>. Setiap id muncul pada hasil; nilainya <c>null</c> bila
        /// batasnya belum diatur (<c>VAL-39</c>).
        /// </summary>
        public async Task<Dictionary<Guid, int?>> GetLimitsAsync(
            IReadOnlyList<Guid> procedureIds,
            CancellationToken cancellationToken = default)
        {
            var bounds = await _dbContext.LabValueBounds
                .AsNoTracking()
                .Where(x =>
                    procedureIds.Contains(x.ProcedureId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.CitoTurnaroundMinutes != null)
                .Select(x => new
                {
                    x.ProcedureId,
                    x.GenderScope,
                    x.AgeCategoryId,
                    x.CitoTurnaroundMinutes
                })
                .ToListAsync(cancellationToken);

            var hasil = new Dictionary<Guid, int?>();

            foreach (var procedureId in procedureIds)
            {
                var milikProcedure = bounds.Where(x => x.ProcedureId == procedureId).ToList();

                if (milikProcedure.Count == 0)
                {
                    hasil[procedureId] = null;
                    continue;
                }

                var umum = milikProcedure.FirstOrDefault(x =>
                    x.GenderScope == LabGenderScope.All && x.AgeCategoryId == null);

                hasil[procedureId] = umum?.CitoTurnaroundMinutes
                    ?? milikProcedure.Min(x => x.CitoTurnaroundMinutes);
            }

            return hasil;
        }
    }
}
