using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Tanggap Darurat Tertusuk Jarum Cito & Tombol Panik ESS (BE-HRD-294, VR-OHS-08, INT-HR-91)
    /// </summary>
    public interface INeedleStickEmergencyService
    {
        Task<NeedleStickReceiptDto> ReportCitoAsync(Guid employeeId, ReportNeedleStickCitoRequestDto request, CancellationToken cancellationToken = default);
        Task<PagedResult<NeedleStickIncidentDto>> GetNeedleStickQueueAsync(string? status, bool? isPepAdministered, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<NeedleStickIncidentDto> AdministerPepAsync(Guid incidentId, AdministerPepRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default);
    }
}
