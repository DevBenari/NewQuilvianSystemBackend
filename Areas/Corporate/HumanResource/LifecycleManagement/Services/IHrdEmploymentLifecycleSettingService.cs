using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdEmploymentLifecycleSettingService
    {
        Task<RecruitmentResult<HrdEmploymentLifecycleSettingResponse>> GetByHospitalSiteIdAsync(Guid hospitalSiteId, CancellationToken cancellationToken = default);
        Task<RecruitmentResult<HrdEmploymentLifecycleSettingResponse>> UpdateAsync(Guid hospitalSiteId, UpdateEmploymentLifecycleSettingRequest request, Guid userId, CancellationToken cancellationToken = default);
    }
}
