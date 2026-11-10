using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Kontrak layanan evaluasi kuota ketersediaan staf klinis unit 24/7 (INT-HR-45, HRD-DEC-111).
    /// </summary>
    public interface ISafeStaffingQueryService
    {
        /// <summary>
        /// Mengevaluasi apakah persetujuan cuti berpotensi melanggar kuota minimal shift dinas unit.
        /// </summary>
        Task<StaffingCoverageEvaluationResult> EvaluateCoverageAsync(
            Guid unitId,
            DateOnly targetDate,
            Guid? shiftId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Hasil evaluasi rasio aman kuota staf unit 24/7 (Safe Staffing Coverage Result).
    /// </summary>
    public record StaffingCoverageEvaluationResult(
        bool IsWarningTriggered,
        int MinimumRequiredStaff,
        int ScheduledStaffCount,
        int RemainingStaffIfApproved,
        string ShiftName,
        string UnitName,
        string AdvisoryMessage
    );
}
