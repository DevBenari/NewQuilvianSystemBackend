using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan perhitungan estimasi biaya perencanaan tenaga kerja dan penyamaran biaya (BE-HRD-038).
    /// </summary>
    public interface IHrdWorkforcePlanningCostService
    {
        /// <summary>
        /// Menghitung estimasi biaya berdasarkan gaji tengah grade pada tanggal acuan (kriteria 1-2).
        /// Estimasi = porsi kerja * gaji tengah * jumlah bulan.
        /// </summary>
        Task<CostEstimationResult> CalculateEstimatedCostAsync(
            Guid? salaryGradeId,
            decimal workloadPortion,
            int months,
            DateOnly asOfDate,
            ClaimsPrincipal? user,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Memeriksa apakah pengguna memiliki hak akses melihat kolom biaya (WorkforcePlanningCost : Read).
        /// </summary>
        bool HasCostReadPermission(ClaimsPrincipal? user);

        /// <summary>
        /// Menerapkan aturan penyamaran biaya secara terpusat pada angka biaya yang akan dikembalikan (kriteria 3).
        /// Bila pengguna tidak berhak, biaya disamarkan menjadi null dan CostVisible = false.
        /// </summary>
        decimal? MaskCost(decimal? cost, ClaimsPrincipal? user);
    }
}
