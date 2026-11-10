using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/workflow")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Matriks Alur & SLA Workflow",
        AreaName = "Corporate",
        ControllerName = "WorkflowSlaMatrix",
        Description = "Matriks alur persetujuan, radar SLA, eskalasi manual, dan laporan kepatuhan",
        SortOrder = 24)]
    [Tags("Corporate - HRD Workflow SLA & Matrix Management")]
    public class WorkflowSlaAndMatrixController : ControllerBase
    {
        private readonly IWorkflowRoutingMatrixService _matrixService;
        private readonly IWorkflowSlaEnforcementService _slaService;

        public WorkflowSlaAndMatrixController(
            IWorkflowRoutingMatrixService matrixService,
            IWorkflowSlaEnforcementService slaService)
        {
            _matrixService = matrixService;
            _slaService = slaService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil konfigurasi matriks alur persetujuan per unit kerja (FR-WF-02, HRD-DEC-182)
        /// </summary>
        [HttpGet("matrix")]
        [AccessAction("ViewMatrix", "Melihat Matriks Alur Persetujuan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowSlaMatrix", "ViewMatrix")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkflowMatrixResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMatrix(
            [FromQuery] Guid? departmentId,
            [FromQuery] string? requestType)
        {
            var result = await _matrixService.GetMatrixConfigurationsAsync(departmentId, requestType);
            return Ok(ApiResponse<List<WorkflowMatrixResponseDto>>.Ok(result));
        }

        /// <summary>
        /// Membuat atau memperbarui konfigurasi tahapan alur persetujuan dinamis (FR-WF-02)
        /// </summary>
        [HttpPost("matrix")]
        [AccessAction("ConfigureMatrix", "Mengonfigurasi Matriks Alur Persetujuan", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkflowSlaMatrix", "ConfigureMatrix")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowMatrixResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureMatrix([FromBody] ConfigureWorkflowMatrixRequestDto request)
        {
            try
            {
                var result = await _matrixService.ConfigureMatrixAsync(request);
                return Ok(ApiResponse<WorkflowMatrixResponseDto>.Ok(result, "Konfigurasi matriks alur kerja berhasil diperbarui"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorkflowMatrixResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil daftar permohonan yang telah melampaui batas waktu SLA (FR-WF-14, FR-WF-15)
        /// </summary>
        [HttpGet("sla/overdue")]
        [AccessAction("ViewSlaOverdue", "Melihat Radar SLA Terlambat", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WorkflowSlaMatrix", "ViewSlaOverdue")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OverdueWorkflowItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverdue(
            [FromQuery] Guid? departmentId,
            [FromQuery] string? requestType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _slaService.GetOverdueTicketsAsync(departmentId, requestType, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<OverdueWorkflowItemDto>>.Ok(result));
        }

        /// <summary>
        /// Melakukan intervensi eskalasi manual tiket macet ke atasan lebih tinggi (FR-WF-16)
        /// </summary>
        [HttpPost("sla/escalate/{id}")]
        [AccessAction("ManualEscalate", "Mengeskalasi Permohonan Manual", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkflowSlaMatrix", "ManualEscalate")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Escalate([FromRoute] Guid id, [FromBody] ManualEscalationRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _slaService.ManualEscalateAsync(id, request, userId);
                return Ok(ApiResponse<WorkflowInstanceV2ResponseDto>.Ok(result, "Tiket berhasil dieskalasi ke atasan target"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Mengunduh laporan kepatuhan SLA kecepatan respon persetujuan rumah sakit (FR-WF-14)
        /// </summary>
        [HttpGet("sla/compliance-report")]
        [AccessAction("ExportSlaReport", "Mengunduh Laporan Kepatuhan SLA", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("WorkflowSlaMatrix", "ExportSlaReport")]
        [ProducesResponseType(typeof(ApiResponse<SlaComplianceReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSlaComplianceReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] Guid? departmentId)
        {
            var result = await _slaService.GetSlaComplianceReportAsync(startDate, endDate, departmentId);
            return Ok(ApiResponse<SlaComplianceReportDto>.Ok(result));
        }
    }
}
