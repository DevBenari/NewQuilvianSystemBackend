using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Service Evaluasi Kinerja Berjenjang Tiga Tingkat (Self -> Karu -> Kabid) & Formula Baku 60:40 (BE-HRD-259, BE-HRD-260, PRF-B3, PRF-B4, PRF-B5)
    /// </summary>
    public interface IPerformanceAppraisalService
    {
        Task<PerformanceAppraisalDto> GetMyAppraisalAsync(Guid workforceProfileId, Guid cycleId, CancellationToken cancellationToken = default);
        Task<PerformanceAppraisalDto> SubmitSelfAppraisalAsync(Guid workforceProfileId, SubmitSelfAppraisalRequestDto dto, CancellationToken cancellationToken = default);
        Task<List<PerformanceAppraisalDto>> GetSubordinateAppraisalsAsync(Guid managerId, Guid cycleId, CancellationToken cancellationToken = default);
        Task<PerformanceAppraisalDto> SubmitManagerAppraisalAsync(Guid appraisalId, Guid managerId, SubmitManagerAppraisalRequestDto dto, CancellationToken cancellationToken = default);
        Task<PerformanceAppraisalDto> ProcessSecondLineReviewAsync(Guid appraisalId, Guid secondLineId, ProcessSecondLineReviewDto dto, CancellationToken cancellationToken = default);
        Task<PerformanceAppraisalDto> AcknowledgeAppraisalAsync(Guid appraisalId, Guid staffId, AcknowledgeAppraisalRequestDto dto, CancellationToken cancellationToken = default);
    }
}
