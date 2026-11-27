using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public interface IRecruitmentJourneyService
    {
        Task<RecruitmentResult<RecruitmentJourneyResponse>> GetApplicationJourneyAsync(
            Guid applicationId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
