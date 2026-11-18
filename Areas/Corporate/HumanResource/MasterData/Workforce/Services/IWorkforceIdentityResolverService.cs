using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    public record IdentityLookupResult(
        bool Found,
        Guid? WorkforceProfileId,
        MstWorkforceProfile? WorkforceProfile,
        bool HasEmployeeRole,
        Guid? EmployeeId,
        bool HasDoctorRole,
        Guid? DoctorId,
        bool HasExternalUserRole,
        Guid? ExternalUserId,
        string? DisplayName,
        List<string> ExistingRoles
    );

    public record ResolvedWorkforceProfileResult(
        MstWorkforceProfile Profile,
        bool IsNewProfile,
        bool IsExistingLinked
    );

    public interface IWorkforceIdentityResolverService
    {
        string NormalizeIdentityNumber(string? identityType, string? identityNumber);
        bool IsValidIdentity(string? identityType, string? identityNumber, out string? errorMessage);

        Task<IdentityLookupResult> FindByIdentityAsync(
            string? identityType,
            string? identityNumber,
            CancellationToken cancellationToken = default);

        Task<ResolvedWorkforceProfileResult> ResolveOrCreateProfileAsync(
            string? identityType,
            string? identityNumber,
            string fullName,
            string? email,
            string? phoneNumber,
            string? whatsAppNumber,
            Guid? primaryDepartmentId,
            Guid? primaryPositionId,
            UserType userType,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
