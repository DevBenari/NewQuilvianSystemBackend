using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    /// <summary>
    /// Controller pemanggilan cuti darurat (Leave Recall) saat situasi genting rumah sakit,
    /// inisiasi Kepala Ruangan, dan otorisasi HR Manager (BE-HRD-126, BE-HRD-127, HRD-DEC-113).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/recalls")]
    [Route("api/corporate/human-resource/leave/recalls")]
    [Route("api/v1/leave/recalls")]
    [Route("api/leave/recalls")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Recall Management",
        AreaName = "Corporate",
        ControllerName = "LeaveRecallWorkflow",
        Description = "Inisiasi dan persetujuan pemanggilan cuti darurat rumah sakit",
        SortOrder = 12)]
    [Tags("HR Leave - Recall Management")]
    public class LeaveRecallWorkflowController : ControllerBase
    {
        private readonly LeaveRecallWorkflowService _recallWorkflowService;

        public LeaveRecallWorkflowController(LeaveRecallWorkflowService recallWorkflowService)
        {
            _recallWorkflowService = recallWorkflowService;
        }

        /// <summary>
        /// Monitoring daftar kasus pemanggilan darurat pegawai cuti.
        /// </summary>
        [HttpGet]
        [AccessAction("View", "Melihat Daftar Recall", Description = "Melihat kasus recall dinas darurat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveRecall", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveRecallDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecalls(
            [FromQuery] Guid? unitId,
            [FromQuery] string? status,
            [FromQuery] int? year,
            CancellationToken cancellationToken)
        {
            var result = await _recallWorkflowService.GetRecallsAsync(unitId, status, year, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<List<LeaveRecallDto>>.Ok(result.Data, result.Message)
                : ApiResponse<List<LeaveRecallDto>>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Inisiasi pemanggilan darurat oleh Kepala Ruangan / Kepala Unit.
        /// </summary>
        [HttpPost]
        [AccessAction("Create", "Inisiasi Recall Darurat", Description = "Membuat tiket pemanggilan cuti", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LeaveRecall", "Create")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRecallDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Initiate(
            [FromBody] InitiateLeaveRecallCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _recallWorkflowService.InitiateRecallAsync(command, userId, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveRecallDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveRecallDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Persetujuan resmi recall oleh HR Manager & pemulihan saldo otomatis.
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Menyetujui Recall", Description = "Persetujuan pemanggilan cuti darurat oleh HR Manager", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveRecall", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRecallExecutionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveLeaveRecallCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            bool isHrManager = User.IsInRole("HRManager") || User.IsInRole("HrManager") || User.HasClaim("role", "HRManager") || User.HasClaim("permission", "LeaveRecall:Approve");
            var result = await _recallWorkflowService.ApproveRecallAsync(id, userId, isHrManager, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveRecallExecutionResultDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveRecallExecutionResultDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Menolak usulan pemanggilan darurat oleh HR Manager.
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Approve", "Menolak Recall", Description = "Penolakan tiket recall oleh HR Manager", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveRecall", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<LeaveRecallDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectLeaveRecallCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            bool isHrManager = User.IsInRole("HRManager") || User.IsInRole("HrManager") || User.HasClaim("role", "HRManager") || User.HasClaim("permission", "LeaveRecall:Approve");
            var result = await _recallWorkflowService.RejectRecallAsync(id, userId, isHrManager, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveRecallDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveRecallDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
