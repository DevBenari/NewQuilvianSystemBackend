using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    public interface IHrdRecruitmentQueueService
    {
        Task<PagedResult<RecruitmentQueueItemResponse>> GetQueueAsync(
            RecruitmentQueuePagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
