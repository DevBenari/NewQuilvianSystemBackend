using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Antarmuka integrasi sinkronisasi kartu penjamin pasien dari HR Benefit ke Patient Management
    /// (BE-FIN-130, FIN-DES-113, FIN-INTEGRATION-1.11 V.2).
    /// HR mendefinisikan antarmuka ini di wilayahnya sendiri dan hanya memanggilnya di luar transaksi komit database (post-commit).
    /// </summary>
    public interface IEmployeeBenefitPatientCardSync
    {
        /// <summary>
        /// Dipanggil sesudah aktivasi enrollment berhasil di-commit (baik lewat Setujui, Aktifkan manual, atau penjadwal).
        /// </summary>
        Task<PatientCardSyncSummaryResponse> OnEnrollmentActivatedAsync(
            Guid enrollmentId,
            Guid actorUserId,
            string triggerSource,
            CancellationToken cancellationToken);

        /// <summary>
        /// Dipanggil sesudah pembatalan enrollment berhasil di-commit untuk menonaktifkan kartu penjamin terkait.
        /// </summary>
        Task OnEnrollmentCancelledAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Permintaan sinkronisasi ulang manual oleh HRD (POST /{id}/patient-card-sync, BE-FIN-138).
        /// </summary>
        Task<PatientCardSyncSummaryResponse> RequestResyncAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Mengambil ringkasan status sinkronisasi kartu untuk sekumpulan enrollment dalam satu batch (hanya baca).
        /// </summary>
        Task<Dictionary<Guid, PatientCardSyncSummaryResponse>> GetSummariesAsync(
            IReadOnlyCollection<Guid> enrollmentIds,
            CancellationToken cancellationToken);
    }
}
