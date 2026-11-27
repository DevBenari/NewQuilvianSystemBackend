using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IMcuClearanceService
    {
        Task<RecruitmentResult<PagedResult<McuClearanceListItemResponse>>> GetPagedAsync(
            McuClearanceQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<List<EligibleMcuOfferResponse>>> GetEligibleOffersAsync(
            string? search,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<McuClearanceResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<McuClearanceResponse>> RegisterMcuAsync(
            RegisterMcuRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<McuClearanceResponse>> RecordResultAsync(
            Guid id,
            RecordMcuResultRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
