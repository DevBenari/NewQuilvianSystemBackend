using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Layanan Tata Kelola Sanggah Banding Nilai Kinerja Pegawai SLA 7/14 Hari (BE-HRD-267, VR-PRF-12, VR-PRF-13)
    /// </summary>
    public interface IPerformanceDisputeService
    {
        Task<AppraisalDisputeDto> SubmitDisputeAsync(SubmitAppraisalDisputeRequestDto dto, Guid staffId, CancellationToken cancellationToken = default);
        Task<AppraisalDisputeDto> AdjudicateDisputeAsync(Guid disputeId, AdjudicateDisputeRequestDto dto, Guid directorId, CancellationToken cancellationToken = default);
    }
}
