using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Tags("Corporate - HRD Employee Disciplinary & Just Culture")]
    [Route("api/v1/hrd/disciplinary")]
    public class EmployeeDisciplinaryController : ControllerBase
    {
        private readonly IDisciplinaryIncidentService _incidentService;
        private readonly IDisciplinaryInvestigationService _investigationService;
        private readonly IDisciplinarySanctionService _sanctionService;
        private readonly IDisciplinarySuspensionService _suspensionService;

        public EmployeeDisciplinaryController(
            IDisciplinaryIncidentService incidentService,
            IDisciplinaryInvestigationService investigationService,
            IDisciplinarySanctionService sanctionService,
            IDisciplinarySuspensionService suspensionService)
        {
            _incidentService = incidentService ?? throw new ArgumentNullException(nameof(incidentService));
            _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
            _sanctionService = sanctionService ?? throw new ArgumentNullException(nameof(sanctionService));
            _suspensionService = suspensionService ?? throw new ArgumentNullException(nameof(suspensionService));
        }

        [HttpPost("incidents")]
        public async Task<IActionResult> ReportIncident([FromBody] CreateIncidentReportRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _incidentService.ReportIncidentAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<IncidentReportResponseDto>.Ok(result, "Laporan insiden berhasil dicatat dengan klasifikasi Just Culture."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<IncidentReportResponseDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IncidentReportResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("incidents")]
        public async Task<IActionResult> GetIncidents(
            [FromQuery] Guid? employeeId,
            [FromQuery] string? justCultureClassification,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _incidentService.GetIncidentsAsync(employeeId, justCultureClassification, status, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<IncidentReportResponseDto>>.Ok(result, "Daftar laporan insiden berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<IncidentReportResponseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpGet("incidents/{incidentId}")]
        public async Task<IActionResult> GetIncidentById(Guid incidentId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _incidentService.GetIncidentByIdAsync(incidentId, cancellationToken);
                if (result == null)
                {
                    return NotFound(ApiResponse<IncidentReportDetailDto>.Fail(404, $"Laporan insiden dengan ID {incidentId} tidak ditemukan."));
                }
                return Ok(ApiResponse<IncidentReportDetailDto>.Ok(result, "Detail laporan insiden dan histori investigasi berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IncidentReportDetailDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("investigations")]
        public async Task<IActionResult> InitiateInvestigation([FromBody] InitiateInvestigationRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _investigationService.InitiateInvestigationAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<InvestigationRecordResponseDto>.Ok(result, "Berkas Berita Acara Pemeriksaan (BAP) formal berhasil diinisiasi."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<InvestigationRecordResponseDto>.Fail(422, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<InvestigationRecordResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InvestigationRecordResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPut("investigations/{investigationId}/findings")]
        public async Task<IActionResult> RecordFindings(Guid investigationId, [FromBody] RecordBapFindingsRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _investigationService.RecordFindingsAsync(investigationId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<InvestigationRecordResponseDto>.Ok(result, "Temuan BAP dan rekomendasi sanksi berhasil dicatat dan disahkan Direktur SDM."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<InvestigationRecordResponseDto>.Fail(422, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<InvestigationRecordResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InvestigationRecordResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("sanctions")]
        public async Task<IActionResult> IssueSanction([FromBody] IssueSanctionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _sanctionService.IssueSanctionAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<SanctionResponseDto>.Ok(result, "Surat sanksi disiplin berhasil diterbitkan dan diarsipkan permanen."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<SanctionResponseDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SanctionResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("sanctions/employee/{employeeId}")]
        public async Task<IActionResult> GetSanctionsByEmployee(Guid employeeId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _sanctionService.GetSanctionsByEmployeeIdAsync(employeeId, cancellationToken);
                return Ok(ApiResponse<List<SanctionResponseDto>>.Ok(result, "Riwayat sanksi pegawai berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SanctionResponseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpPost("sanctions/{sanctionId}/appeal")]
        public async Task<IActionResult> AppealSanction(Guid sanctionId, [FromBody] AppealSanctionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _sanctionService.AppealSanctionAsync(sanctionId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<SanctionAppealResponseDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<SanctionAppealResponseDto>.Fail(422, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SanctionAppealResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SanctionAppealResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("suspensions")]
        public async Task<IActionResult> IssueSuspension([FromBody] IssueSuspensionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _suspensionService.IssueSuspensionAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<SuspensionResponseDto>.Ok(result, "Surat penetapan skorsing investigasi (paid suspension) diterbitkan dan akses SIMRS dibekukan."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<SuspensionResponseDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SuspensionResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("suspensions/{suspensionId}/reinstate")]
        public async Task<IActionResult> ReinstateEmployee(Guid suspensionId, [FromBody] ReinstateEmployeeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _suspensionService.ReinstateEmployeeAsync(suspensionId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<SuspensionResponseDto>.Ok(result, "Skorsing berakhir, status pegawai dipulihkan dan akses SIMRS diaktifkan kembali."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SuspensionResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SuspensionResponseDto>.Fail(500, ex.Message));
            }
        }
    }
}
