using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    /// <summary>
    /// Layanan Master Aturan Pelatihan Wajib Rumah Sakit STARKES KPS 8 (VR-TRN-01, VR-TRN-02)
    /// </summary>
    public interface IMandatoryTrainingRuleService
    {
        /// <summary>
        /// Mengambil daftar katalog master pelatihan wajib akreditasi STARKES KPS 8
        /// </summary>
        Task<List<MandatoryTrainingRuleDto>> GetMandatoryRulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mendaftarkan atau mengonfigurasi aturan pelatihan wajib baru dengan validasi batas masa berlaku (max 24 bulan)
        /// </summary>
        Task<MandatoryTrainingRuleDto> CreateOrUpdateRuleAsync(CreateMandatoryTrainingRuleRequestDto request, CancellationToken cancellationToken = default);
    }
}
