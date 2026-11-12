using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdInitialContractBatchService
    {
        Task<RecruitmentResult<PagedResult<InitialContractBatchListItemResponse>>> GetPagedAsync(
            InitialContractBatchQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<InitialContractBatchDetailResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<InitialContractBatchDetailResponse>> CreateDraftBatchAsync(
            CreateInitialContractBatchRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<InitialContractBatchRowResponse>> AddOrUpdateRowAsync(
            Guid batchId,
            AddInitialContractBatchRowRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> DeleteRowAsync(
            Guid batchId,
            Guid rowId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<InitialContractBatchDetailResponse>> SubmitBatchAsync(
            Guid batchId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<InitialContractBatchDetailResponse>> CancelBatchAsync(
            Guid batchId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> ApplyInitialBatchAsync(
            Guid batchId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default);
    }
}
