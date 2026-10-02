using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Label order turunan <c>resultProgress</c> (<c>LAB-API-v1</c> <c>r34</c> 29.5,
    /// <c>LAB-DEC-135</c>): <c>InProgress</c> — <i>Dalam Pemeriksaan</i> — selama masih ada
    /// pemeriksaan yang dihitung dan belum dirilis; <c>AllReleased</c> — <i>Selesai</i> — bila
    /// seluruhnya sudah dirilis; kosong bila order tidak punya pemeriksaan yang dihitung.
    ///
    /// <para>
    /// <b>Nol kolom tersimpan</b> (<c>AC-199</c>) dan <b>nol sentuhan pada <c>orderStatus</c></b>.
    /// Namanya sengaja bukan <c>Completed</c>: <c>LabOrderStatus.Completed</c> sudah ada dengan
    /// arti <i>ditandai selesai secara manual</i> (<c>LAB-CONFLICT-014</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Satu definisi "pemeriksaan yang dihitung"</b> bagi setiap pembaca — daftar Pemeriksaan,
    /// detail order, dan kelak penjaga penyelesaian order (<c>LAB-DEC-154</c>, rancangan 22
    /// langkah 3). Label dan penjaga yang memakai dua definisi akan berbeda pendapat tentang
    /// order yang sama.
    /// </para>
    /// </summary>
    internal static class LabOrderResultProgressRules
    {
        /// <summary>
        /// Pemeriksaan yang ikut menentukan label: tidak terhapus, tidak <b>gugur</b>, dan tidak
        /// <b>batal</b>. Pemeriksaan gugur tidak pernah dapat dirilis; bila ikut dihitung, order
        /// yang wadahnya pernah ditolak tidak akan pernah <i>Selesai</i>.
        /// </summary>
        internal static readonly Expression<Func<LabExamination, bool>> Counted = x =>
            !x.IsDelete &&
            x.ExaminationStatus != LabExaminationStatus.Voided &&
            x.ExaminationStatus != LabExaminationStatus.Cancelled;

        /// <summary>
        /// Label untuk banyak order sekaligus, <b>satu kueri</b> berapa pun jumlahnya. Order yang
        /// tidak punya pemeriksaan yang dihitung tidak muncul pada hasil — labelnya kosong.
        /// </summary>
        internal static async Task<Dictionary<Guid, LabOrderResultProgress>> ReadAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> labOrderIds,
            CancellationToken cancellationToken)
        {
            if (labOrderIds.Count == 0)
            {
                return new Dictionary<Guid, LabOrderResultProgress>();
            }

            var rows = await dbContext.LabExaminations
                .AsNoTracking()
                .Where(Counted)
                .Where(x => labOrderIds.Contains(x.LabOrderId))
                .GroupBy(x => x.LabOrderId)
                .Select(g => new
                {
                    LabOrderId = g.Key,
                    BelumDirilis = g.Count(x => x.ReleasedAt == null)
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                x => x.LabOrderId,
                x => x.BelumDirilis == 0 ? LabOrderResultProgress.AllReleased : LabOrderResultProgress.InProgress);
        }
    }
}
