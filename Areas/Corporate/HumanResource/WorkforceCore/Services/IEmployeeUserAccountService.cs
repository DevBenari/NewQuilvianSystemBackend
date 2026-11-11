using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IEmployeeUserAccountService
    {
        Task<(bool Success, string Message, ApplicationUser? User, string? InitialPassword)> CreateForWorkforceProfileAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            bool isFingerprintRegistrationEnabled = false,
            string? fingerprintRegistrationReason = null,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string Message)> ActivateAsync(
            Guid applicationUserId,
            Guid actorUserId,
            CancellationToken cancellationToken = default);
    }
}
