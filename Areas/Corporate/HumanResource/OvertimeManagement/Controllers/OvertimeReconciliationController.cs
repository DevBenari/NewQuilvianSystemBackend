using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime-management/reconciliation")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Final Reconciliation",
        AreaName = "Corporate",
        ControllerName = "OvertimeReconciliation",
        Description = "Cross-module final reconciliation for Workflow, Attendance, Overtime, Leave, and Payroll",
        SortOrder = 9)]
    [Tags("Corporate / Human Resource / Overtime Management / Final Reconciliation")]
    public class OvertimeReconciliationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeFinalReconciliationService _service;
        private readonly LoggerService _loggerService;

        public OvertimeReconciliationController(
            OvertimeFinalReconciliationService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpPost("run")]
        [AccessAction("Reconcile", "Reconcile Overtime", Description = "Menjalankan final reconciliation lintas modul", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeReconciliation", "Reconcile")]
        public async Task<IActionResult> Run(
            [FromBody] OvertimeReconciliationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!request.OvertimePeriodId.HasValue &&
                (request.StartDate == default || request.EndDate == default || request.EndDate < request.StartDate))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "StartDate dan EndDate wajib valid ketika OvertimePeriodId tidak dipilih."));
            }

            var actor = GetCurrentUserId();
            if (request.AllowRepair && actor == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login diperlukan untuk mode repair."));
            }

            var data = await _service.ReconcileAsync(request, actor, cancellationToken);
            await _loggerService.InfoAsync(
                LogCategory,
                "OvertimeReconciliation.Run",
                request.AllowRepair ? "Final reconciliation dan safe repair dijalankan." : "Final reconciliation dijalankan.",
                data);

            return Ok(ApiResponse<OvertimeFinalReconciliationResponse>.Ok(
                data,
                data.IsCloseReady
                    ? "Final reconciliation selesai dan tidak menemukan blocking issue."
                    : "Final reconciliation selesai dan masih menemukan blocking issue."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
