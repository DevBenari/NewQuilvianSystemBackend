using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface IOvertimeBiometricVerificationService
    {
        Task<OvertimeBiometricVerificationResultDto> VerifyBiometricAsync(
            Guid realizationId,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default);
    }
}
