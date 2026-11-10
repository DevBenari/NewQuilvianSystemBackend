using System;
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
        displayName: "Kotak Masuk & Permohonan Workflow",
        AreaName = "Corporate",
        ControllerName = "WorkflowInboxInstance",
        Description = "Kotak masuk persetujuan terpadu, rincian permohonan, pengajuan, dan pembatalan",
        SortOrder = 20)]
    [Tags("Corporate - HRD Workflow Inbox & Instances")]
    public class WorkflowInboxAndInstancesController : ControllerBase
    {
        private readonly IApprovalInboxQueryService _inboxService;
        private readonly IWorkflowInstanceCreationService _instanceService;

        public WorkflowInboxAndInstancesController(
            IApprovalInboxQueryService inboxService,
            IWorkflowInstanceCreationService instanceService)
        {
            _inboxService = inboxService;
            _instanceService = instanceService;
        }

        private Guid GetCurrentEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var empId)) return empId;
            return Guid.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil daftar permohonan persetujuan yang pending untuk approver login (FR-WF-03)
        /// </summary>
        [HttpGet("inbox")]
        [AccessAction("ViewInbox", "Melihat Kotak Masuk Persetujuan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowInboxInstance", "ViewInbox")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ApprovalInboxItemV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInbox(
            [FromQuery] string? status,
            [FromQuery] string? requestType,
            [FromQuery] bool? isUrgent,
            [FromQuery] bool? isDelegated,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var empId = GetCurrentEmployeeId();
            var result = await _inboxService.GetInboxAsync(empId, status, requestType, isUrgent, isDelegated, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ApprovalInboxItemV2ResponseDto>>.Ok(result));
        }

        /// <summary>
        /// Mengambil ringkasan metrik kotak masuk (pending, urgent H-4 SLA, delegated) (FR-WF-04)
        /// </summary>
        [HttpGet("inbox/summary")]
        [AccessAction("ViewInbox", "Melihat Kotak Masuk Persetujuan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowInboxInstance", "ViewInbox")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalInboxSummaryV2ResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInboxSummary()
        {
            var empId = GetCurrentEmployeeId();
            var result = await _inboxService.GetInboxSummaryAsync(empId);
            return Ok(ApiResponse<ApprovalInboxSummaryV2ResponseDto>.Ok(result));
        }

        /// <summary>
        /// Mengambil detail lengkap tiket permohonan, tahapan step, dan checklist validasi (FR-WF-01)
        /// </summary>
        [HttpGet("instances/{id}")]
        [AccessAction("ViewInstanceDetail", "Melihat Rincian Permohonan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkflowInboxInstance", "ViewInstanceDetail")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstanceDetail([FromRoute] Guid id)
        {
            var result = await _instanceService.GetInstanceDetailAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<WorkflowInstanceDetailV2ResponseDto>.Fail(StatusCodes.Status404NotFound, "Tiket workflow tidak ditemukan"));
            }
            return Ok(ApiResponse<WorkflowInstanceDetailV2ResponseDto>.Ok(result));
        }

        /// <summary>
        /// Mendaftarkan dan memulai workflow persetujuan baru dari ESS (FR-WF-01)
        /// </summary>
        [HttpPost("instances")]
        [AccessAction("CreateInstance", "Mengajukan Permohonan", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("WorkflowInboxInstance", "CreateInstance")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceV2ResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInstance([FromBody] CreateWorkflowInstanceRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var empId = GetCurrentEmployeeId();
                if (!request.ApplicantEmployeeId.HasValue && empId != Guid.Empty)
                {
                    request.ApplicantEmployeeId = empId;
                }

                var result = await _instanceService.CreateInstanceAsync(request, userId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<WorkflowInstanceV2ResponseDto>.Ok(result, "Permohonan alur kerja berhasil didaftarkan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Membatalkan permohonan oleh pemohon sebelum tiket disetujui final (FR-WF-01)
        /// </summary>
        [HttpPut("instances/{id}/cancel")]
        [AccessAction("CancelMyInstance", "Membatalkan Permohonan Sendiri", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkflowInboxInstance", "CancelMyInstance")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelInstance([FromRoute] Guid id, [FromBody] CancelWorkflowRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _instanceService.CancelInstanceAsync(id, request, userId);
                return Ok(ApiResponse<WorkflowInstanceV2ResponseDto>.Ok(result, "Permohonan berhasil dibatalkan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil riwayat permohonan mandiri yang diajukan staf login di ESS (FR-WF-01)
        /// </summary>
        [HttpGet("my-requests")]
        [AccessAction("ViewOwn", "Melihat Permohonan Sendiri", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("WorkflowInboxInstance", "ViewOwn")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkflowInstanceV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] string? status,
            [FromQuery] string? requestType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var empId = GetCurrentEmployeeId();
            var result = await _instanceService.GetMyRequestsAsync(empId, status, requestType, startDate, endDate, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WorkflowInstanceV2ResponseDto>>.Ok(result));
        }
    }
}
