using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>Hasil satu pemeriksaan kewenangan klinis petugas.</summary>
    public sealed record HmdCompetencyResult(
        HmdCompetencyVerificationStatus Status,
        string SourceReference,
        DateTime CheckedAt);

    /// <summary>
    /// Memeriksa kewenangan klinis petugas untuk menjalankan dialisis dan mengembalikan salah satu
    /// dari tiga status (<c>BE-HMD-11</c>, <c>HMD-DEC-013</c>, syarat 5).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Jawaban jujurnya saat ini selalu <see cref="HmdCompetencyVerificationStatus.NotVerifiable"/>.</b>
    /// Human Resource belum menyediakan pembacaan "kewenangan dialisis yang masih berlaku"
    /// (<c>HMD-DEP-002</c>). Tabel <c>WfpClinicalPrivilege</c> memang ada, tetapi belum ada pemetaan
    /// yang disetujui antara butir kewenangannya dan tindakan dialisis — menebak pemetaan itu berarti
    /// mengarang kebijakan kredensial. Karena itu status ini <b>tidak pernah</b> ditulis
    /// <see cref="HmdCompetencyVerificationStatus.Verified"/>: perbedaan antara "sudah diperiksa dan
    /// aman" dan "belum pernah diperiksa" justru yang dicari auditor.
    /// </para>
    /// <para>
    /// Ketika pembacaan HR tersedia, hanya method ini yang berubah. Penegakannya sudah dinyalakan
    /// lewat <c>HmdSetting.EnforceCompetencyCheck</c>, tanpa perubahan tabel.
    /// </para>
    /// </remarks>
    public class HmdCompetencyGateService
    {
        public const string NotVerifiableReference =
            "HMD-DEP-002: pembacaan kewenangan klinis dari Human Resource belum tersedia.";

        public Task<HmdCompetencyResult> EvaluateAsync(
            Guid workforceProfileId,
            HmdStaffRole staffRole,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HmdCompetencyResult(
                HmdCompetencyVerificationStatus.NotVerifiable,
                NotVerifiableReference,
                DateTime.UtcNow));
        }
    }
}
