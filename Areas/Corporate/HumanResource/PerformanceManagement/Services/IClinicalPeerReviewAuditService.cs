using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    public interface IClinicalPeerReviewAuditService
    {
        Task<ClinicalPeerReviewAuditLogDto> LogPeerReviewAccessAsync(LogPeerReviewAccessRequestDto request, CancellationToken cancellationToken = default);
        Task<List<ClinicalPeerReviewAuditLogDto>> GetAuditLogsAsync(Guid? workforceProfileId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    }
}
