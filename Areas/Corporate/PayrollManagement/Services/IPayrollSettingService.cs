using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public interface IPayrollSettingService
    {
        Task<HrdPayrollSettingDto> GetSettingAsync(CancellationToken cancellationToken = default);
        Task<HrdPayrollSettingDto> UpdateSettingAsync(UpdateHrdPayrollSettingCommand command, Guid actorUserId, CancellationToken cancellationToken = default);
        Task<string> GetSettingSnapshotJsonAsync(CancellationToken cancellationToken = default);
    }
}
