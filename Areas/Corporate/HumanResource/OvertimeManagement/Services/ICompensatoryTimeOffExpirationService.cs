using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface ICompensatoryTimeOffExpirationService
    {
        Task<CompensatoryExpirationRunResultDto> ExecuteExpirationRunAsync(CancellationToken cancellationToken = default);
    }
}
