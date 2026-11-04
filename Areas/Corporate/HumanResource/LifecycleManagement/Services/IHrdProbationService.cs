using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdProbationService
    {
        Task<HrdProbation?> CreateForContractAsync(WfpContractHistory contract, Guid hospitalSiteId, Guid userId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<PagedResult<HrdProbationResponse>>> GetPagedAsync(HrdProbationQuery query, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<HrdProbationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
