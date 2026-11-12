using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Layanan Manajemen Pemeriksaan Kesehatan Berkala Staf (MCU) Berbasis Profil Risiko & Kepatuhan STARKES KPS (BE-HRD-299, VR-OHS-12, AT-HRD-OHS-22)
    /// </summary>
    public interface IMedicalExaminationService
    {
        Task<MedicalExaminationResponseDto> ScheduleMcuAsync(ScheduleMcuRequestDto request, Guid scheduledByUserId, CancellationToken cancellationToken = default);
        Task<PagedResult<MedicalExaminationSummaryDto>> GetMcuListAsync(string? examType, string? riskProfile, string? status, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<MedicalExaminationResponseDto> RecordMcuResultsAsync(Guid examinationId, RecordMcuResultsRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default);
    }
}
