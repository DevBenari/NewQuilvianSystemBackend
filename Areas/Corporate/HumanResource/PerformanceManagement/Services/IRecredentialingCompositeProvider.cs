using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Penyedia Rapor Komposit 3 Tahun & Kualitas Rekredensialing Dokter (INT-HR-75, VR-OPP-15..20)
    /// </summary>
    public interface IRecredentialingCompositeProvider
    {
        /// <summary>
        /// Mengambil rapor komposit 3 tahunan dokter beserta klasifikasi tier eligibilitas
        /// </summary>
        Task<RecredentialingScorecardDto> GetScorecardAsync(Guid workforceProfileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menghitung kalkulasi komposit 3 tahun (25%-35%-40%) dari data historis OPPE & insiden FPPE
        /// </summary>
        Task<RecredentialingScorecardDto> CalculateScorecardAsync(Guid workforceProfileId, CalculateRecredScorecardRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menerapkan penyesuaian diskresi tier kelayakan oleh Ketua Komite Medik dengan justifikasi tertulis (VR-OPP-19)
        /// </summary>
        Task<RecredentialingScorecardDto> OverrideTierAsync(Guid workforceProfileId, OverrideRecredTierRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menetapkan putusan sah sidang pleno rekredensialing (4 Putusan: RenewAsIs, Upgrade, Restricted, Revoked, VR-OPP-20)
        /// </summary>
        Task<RecredVerdictResponseDto> IssueVerdictAsync(CreateRecredVerdictRequestDto request, CancellationToken cancellationToken = default);
    }
}
