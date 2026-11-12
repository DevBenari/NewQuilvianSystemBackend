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
    /// Controller penutupan periode cuti pra-payroll, rekonsiliasi retrospektif cuti pending,
    /// dan proteksi immutabilitas cuti (BE-HRD-128, BE-HRD-129, HRD-DEC-114, HRD-DEC-115, VR-LEV-19, VR-LEV-20).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave")]
    [Route("api/corporate/human-resource/leave")]
    [Route("api/v1/leave")]
    [Route("api/leave")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Reconciliations & Close",
        AreaName = "Corporate",
        ControllerName = "LeaveReconciliation",
        Description = "Penutupan periode cuti pra-payroll dan rekonsiliasi retrospektif cuti pending",
        SortOrder = 13)]
    [Tags("HR Leave - Reconciliations & Close")]
    public class LeaveReconciliationController : ControllerBase
    {
        private readonly LeavePayrollReconciliationService _reconciliationService;

        public LeaveReconciliationController(LeavePayrollReconciliationService reconciliationService)
        {
            _reconciliationService = reconciliationService;
        }

        /// <summary>
        /// Daftar cuti pending yang akan direkonsiliasi secara retrospektif pada periode payroll berikutnya.
        /// </summary>
        [HttpGet("reconciliations/pending")]
        [AccessAction("View", "Melihat Cuti Pending Rekonsiliasi", Description = "Melihat cuti pending pra/pasca closing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveReconciliation", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<PendingLeavePayrollDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPending(
            [FromQuery] int? periodYear,
            [FromQuery] int? periodMonth,
            CancellationToken cancellationToken)
        {
            var result = await _reconciliationService.GetPendingReconciliationsAsync(periodYear, periodMonth, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<List<PendingLeavePayrollDto>>.Ok(result.Data, result.Message)
                : ApiResponse<List<PendingLeavePayrollDto>>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Mengunci periode cuti bulanan dan serahkan snapshot ke Payroll (IsPayrollClosed = true).
        /// </summary>
        [HttpPost("reconciliations/close-period")]
        [AccessAction("Close", "Menutup Periode Cuti", Description = "Mengunci cuti selesai dan kirim snapshot ke payroll", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LeaveReconciliation", "Close")]
        [ProducesResponseType(typeof(ApiResponse<CloseLeavePeriodResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClosePeriod(
            [FromBody] CloseLeavePeriodCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _reconciliationService.CloseLeavePeriodAsync(command, userId, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<CloseLeavePeriodResultDto>.Ok(result.Data, result.Message)
                : ApiResponse<CloseLeavePeriodResultDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Percobaan pembalikan cuti — Ditolak HTTP 409 bila status Completed atau PayrollClosed (HRD-DEC-115, VR-LEV-19, VR-LEV-20).
        /// </summary>
        [HttpPost("execution/{id:guid}/reverse")]
        [AccessAction("Reverse", "Pembalikan Eksekusi Cuti", Description = "Mencoba membalikkan cuti yang telah dieksekusi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveExecution", "Reverse")]
        [ProducesResponseType(typeof(ApiResponse<LeaveExecutionActionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReverseExecution(
            Guid id,
            [FromBody] ReverseExecutionCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _reconciliationService.ReverseExecutionAsync(id, command, userId, cancellationToken);
            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveExecutionActionResponse>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveExecutionActionResponse>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
