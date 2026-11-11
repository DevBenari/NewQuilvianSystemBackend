using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IDisciplinaryIncidentService
    {
        Task<IncidentReportResponseDto> ReportIncidentAsync(CreateIncidentReportRequestDto request, string actorEmail, CancellationToken cancellationToken = default);
        Task<PagedResult<IncidentReportResponseDto>> GetIncidentsAsync(Guid? employeeId, string? justCultureClassification, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IncidentReportDetailDto?> GetIncidentByIdAsync(Guid incidentId, CancellationToken cancellationToken = default);
    }
}
