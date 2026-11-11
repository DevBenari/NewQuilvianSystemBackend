using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka layanan kalkulasi langsung gap tenaga kerja per unit dan jabatan (BE-HRD-037).
    /// Tanpa transaksi dan read-only terhadap data sumber.
    /// </summary>
    public interface IHrdStaffingGapCalculationService
    {
        /// <summary>
        /// Menghitung formasi, terisi, lapisan gap, dan tiga angka gap per kombinasi unit dan jabatan pada tanggal acuan.
        /// </summary>
        Task<StaffingGapResponse> CalculateStaffingGapsAsync(
            StaffingGapQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
