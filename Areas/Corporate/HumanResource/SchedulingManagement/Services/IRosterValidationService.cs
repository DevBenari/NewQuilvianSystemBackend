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

        /// <summary>
        /// Memeriksa satu shift calon (Ganti Petugas, tambah shift, tukar shift) dengan aturan yang sama
        /// dengan terbit, tanpa menyimpan apa pun. Null bila periode roster tidak ditemukan.
        /// </summary>
        Task<RosterSingleShiftValidationResult?> ValidateSingleShiftAsync(
            RosterSingleShiftValidationRequest request,
            CancellationToken cancellationToken = default);
    }
}
