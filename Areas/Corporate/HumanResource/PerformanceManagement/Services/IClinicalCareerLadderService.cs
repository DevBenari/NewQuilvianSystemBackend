using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Layanan Evaluasi Kenaikan Jenjang Karier Klinis Nakes (PK I-V) & Integrasi Lintas Sub-Modul (BE-HRD-264, BE-HRD-265, BE-HRD-266, PRF-B8, INT-HR-81, INT-HR-82, INT-HR-83, VR-PRF-14)
    /// </summary>
    public interface IClinicalCareerLadderService
    {
        Task<ClinicalCareerLadderDto> ApplyClinicalLadderAsync(Guid workforceProfileId, ApplyClinicalLadderRequestDto dto, CancellationToken cancellationToken = default);
        Task<ClinicalCareerLadderDto> DeliverVerdictAsync(Guid reviewId, VerdictClinicalLadderRequestDto dto, Guid assessorId, CancellationToken cancellationToken = default);
    }
}
