using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Layanan resolusi identitas workforce (Karyawan/Dokter/Keluarga/Umum) dan kelayakan benefit
    /// pasien berdasarkan No. RM / PatientId.
    /// </summary>
    public interface IPatientWorkforceIdentityResolver
    {
        Task<PatientWorkforceStatusResponse> ResolveStatusAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);
    }
}
