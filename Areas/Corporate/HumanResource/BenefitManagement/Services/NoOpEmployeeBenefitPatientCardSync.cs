using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Implementasi no-op IEmployeeBenefitPatientCardSync (BE-FIN-130).
    /// Berfungsi sebagai implementasi default sementara sebelum EmployeeBenefitCardSyncService
    /// di Patient Management diimplementasikan secara penuh pada BE-FIN-135.
    /// Mengembalikan status NOT_STARTED tanpa memodifikasi tabel atau memicu exception.
    /// </summary>
    public sealed class NoOpEmployeeBenefitPatientCardSync : IEmployeeBenefitPatientCardSync
    {
        public Task<PatientCardSyncSummaryResponse> OnEnrollmentActivatedAsync(
            Guid enrollmentId,
            Guid actorUserId,
            string triggerSource,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new PatientCardSyncSummaryResponse
            {
                Status = EmployeeBenefitCardSyncStatuses.NotStarted,
                ReasonCode = null,
                ReasonLabel = "Sinkronisasi kartu pasien belum aktif (no-op).",
                AttemptCount = 0
            });
        }

        public Task OnEnrollmentCancelledAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<PatientCardSyncSummaryResponse> RequestResyncAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new PatientCardSyncSummaryResponse
            {
                Status = EmployeeBenefitCardSyncStatuses.NotStarted,
                ReasonCode = null,
                ReasonLabel = "Sinkronisasi kartu pasien belum aktif (no-op).",
                AttemptCount = 0
            });
        }

        public Task<Dictionary<Guid, PatientCardSyncSummaryResponse>> GetSummariesAsync(
            IReadOnlyCollection<Guid> enrollmentIds,
            CancellationToken cancellationToken)
        {
            var result = enrollmentIds.Distinct().ToDictionary(
                id => id,
                _ => new PatientCardSyncSummaryResponse
                {
                    Status = EmployeeBenefitCardSyncStatuses.NotStarted,
                    ReasonCode = null,
                    ReasonLabel = "Sinkronisasi kartu pasien belum aktif (no-op).",
                    AttemptCount = 0
                });
            return Task.FromResult(result);
        }
    }
}
