using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Investigasi Kecelakaan Kerja (KAK/PAK), Rekomendasi CAPA & Integrasi Klaim BPJS JKK (BE-HRD-300, VR-OHS-18..21, INT-HR-90)
    /// </summary>
    public interface IOccupationalInjuryService
    {
        Task<EmployeeInjuryResponseDto> ReportInjuryAsync(ReportEmployeeInjuryRequestDto request, Guid reportedByUserId, CancellationToken cancellationToken = default);
        Task<BpjsJkkClaimFormDto> GenerateBpjsFormAsync(Guid injuryId, int stage, CancellationToken cancellationToken = default);
    }
}
