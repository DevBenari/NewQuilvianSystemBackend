using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimeSettingService
    {
        Task<OvertimeSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task<OvertimeSettingDto> UpdateSettingsAsync(UpdateOvertimeSettingCommand command, Guid? actorUserId = null, CancellationToken cancellationToken = default);
    }
}
