using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Layanan Program Pembinaan Formal PIP 60 Hari & Integrasi Pembekuan Mutasi Sub-06 (BE-HRD-262, BE-HRD-263, PRF-B7, INT-HR-84, VR-PRF-09, VR-PRF-10, VR-PRF-11)
    /// </summary>
    public interface IPerformanceImprovementPlanService
    {
        Task<List<PipActiveSummaryDto>> GetActivePipListAsync(CancellationToken cancellationToken = default);
        Task<PipActiveSummaryDto> IssueContractAsync(IssuePipContractRequestDto dto, Guid issuedBy, CancellationToken cancellationToken = default);
        Task<bool> RecordWeeklyLogAsync(Guid pipId, RecordPipWeeklyLogRequestDto dto, Guid mentorId, CancellationToken cancellationToken = default);
        Task<bool> DeliverFinalVerdictAsync(Guid pipId, VerdictPipRequestDto dto, Guid approverId, CancellationToken cancellationToken = default);
    }
}
