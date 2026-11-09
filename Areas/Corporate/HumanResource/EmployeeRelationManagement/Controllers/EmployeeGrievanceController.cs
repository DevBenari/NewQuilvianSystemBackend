using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Tags("Corporate - HRD Grievance & Confidential Whistleblowing")]
    [Route("api/v1/hrd/grievances")]
    public class EmployeeGrievanceController : ControllerBase
    {
        private readonly IEmployeeGrievanceService _grievanceService;
        private readonly IWhistleblowerCaseService _whistleblowerService;

        public EmployeeGrievanceController(
            IEmployeeGrievanceService grievanceService,
            IWhistleblowerCaseService whistleblowerService)
        {
            _grievanceService = grievanceService ?? throw new ArgumentNullException(nameof(grievanceService));
            _whistleblowerService = whistleblowerService ?? throw new ArgumentNullException(nameof(whistleblowerService));
        }

        [HttpPost]
        public async Task<IActionResult> SubmitGrievance([FromBody] SubmitGrievanceRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _grievanceService.SubmitGrievanceAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<GrievanceTicketResponseDto>.Ok(result, "Tiket keluhan kerja pegawai berhasil diajukan dengan target SLA 14 hari kerja."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrievanceTicketResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("whistleblower")]
        public async Task<IActionResult> SubmitWhistleblower([FromBody] SubmitWhistleblowerRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "anonymous@rs-mmc.co.id";
                var result = await _whistleblowerService.SubmitWhistleblowerCaseAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<WhistleblowerTicketReceiptDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<WhistleblowerTicketReceiptDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<WhistleblowerTicketReceiptDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] Guid employeeId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _grievanceService.GetMyTicketsAsync(employeeId, status, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<GrievanceTicketSummaryDto>>.Ok(result, "Daftar tiket keluhan kerja berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<GrievanceTicketSummaryDto>>.Fail(500, ex.Message));
            }
        }

        [HttpGet("compliance-inbox")]
        public async Task<IActionResult> GetComplianceInbox(
            [FromQuery] string? category,
            [FromQuery] string? severityLevel,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _whistleblowerService.GetComplianceInboxAsync(category, severityLevel, status, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<WhistleblowerCaseDto>>.Ok(result, "Kotak masuk investigasi rahasia Kepatuhan berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<WhistleblowerCaseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpPut("{grievanceId}/mediation")]
        public async Task<IActionResult> RecordMediation(Guid grievanceId, [FromBody] RecordMediationResolutionRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = User?.Identity?.Name ?? "system@rs-mmc.co.id";
                var result = await _grievanceService.RecordMediationResolutionAsync(grievanceId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<GrievanceTicketResponseDto>.Ok(result, "Hasil risalah mediasi hubungan kerja berhasil diperbarui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<GrievanceTicketResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrievanceTicketResponseDto>.Fail(500, ex.Message));
            }
        }
    }
}
