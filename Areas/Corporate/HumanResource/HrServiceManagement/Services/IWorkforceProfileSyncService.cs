using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IWorkforceProfileSyncService
    {
        Task<ProfileSyncResultDto> ApplyApprovedProfileChangesAsync(Guid hrServiceRequestId, Guid approvedByUserId, CancellationToken cancellationToken = default);
        Task<List<ProfileChangeHistoryItemDto>> GetChangeHistoryByEmployeeAsync(Guid workforceProfileId, CancellationToken cancellationToken = default);
        Task<List<ProfileChangeHistoryItemDto>> GetChangeHistoryByTicketAsync(Guid hrServiceRequestId, CancellationToken cancellationToken = default);
    }
}
