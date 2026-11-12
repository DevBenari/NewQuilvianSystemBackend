using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public interface IRosterValidationService
    {
        Task<RosterValidationResultResponse> ValidateRosterPeriodAsync(
            Guid rosterPeriodId,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<RosterValidationConflictItemDto?> ValidateSingleShiftAsync(
            Guid shiftAssignmentId,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
