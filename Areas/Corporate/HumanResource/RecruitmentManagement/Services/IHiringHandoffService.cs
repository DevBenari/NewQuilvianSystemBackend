using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IHiringHandoffService
    {
        Task<RecruitmentResult<PagedResult<HiringHandoffListItemResponse>>> GetPagedAsync(
            HiringHandoffQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<HiringHandoffResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<HiringHandoffResponse>> GenerateHandoffAsync(
            GenerateHiringHandoffRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<HiringHandoffResponse>> ConfirmTransferAsync(
            Guid id,
            ConfirmHiringHandoffRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
