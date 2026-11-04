using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface ITalentPoolService
    {
        Task<RecruitmentResult<PagedResult<TalentPoolListItemResponse>>> GetPagedAsync(
            TalentPoolQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<TalentPoolResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<TalentPoolResponse>> EnrollAsync(
            EnrollTalentPoolRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<TalentPoolResponse>> FastTrackPromoteAsync(
            Guid id,
            FastTrackPromoteRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
