using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime/payroll-handoff")]
    [Route("api/overtime/payroll-handoff")]
    [Tags("HR Overtime - Payroll Handoff & Export")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Payroll Handoff",
        AreaName = "Corporate",
        ControllerName = "OvertimePayrollHandoffModern",
        Description = "Pratinjau agregasi lembur cut-off bulanan dan eksekusi serah terima data terkunci ke Modul 10 Penggajian",
        SortOrder = 7)]
    public class OvertimePayrollHandoffModernController : ControllerBase
    {
        private readonly IOvertimePayrollHandoffModernService _handoffService;

        public OvertimePayrollHandoffModernController(IOvertimePayrollHandoffModernService handoffService)
        {
            _handoffService = handoffService ?? throw new ArgumentNullException(nameof(handoffService));
        }

        [HttpGet("preview/{periodId:guid}")]
        [AccessAction("View", "Melihat Pratinjau Paket Handoff Payroll", Description = "Melihat rekapitulasi data lembur finansial reguler dan rapel retrospektif per pegawai sebelum serah terima", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoffModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePayrollHandoffPackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PreviewPackage(
            Guid periodId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var package = await _handoffService.PreviewHandoffPackageAsync(periodId, cancellationToken);
                return Ok(ApiResponse<OvertimePayrollHandoffPackageDto>.Ok(package, "Pratinjau paket serah terima Payroll berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpPost("execute/{periodId:guid}")]
        [AccessAction("Execute", "Mengeksekusi Serah Terima ke Payroll", Description = "Mengunci dan menyerahkan seluruh rekaman lembur berbayar ke Modul 10 Penggajian", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OvertimePayrollHandoffModern", "Execute")]
        [ProducesResponseType(typeof(ApiResponse<OvertimePayrollHandoffPackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ExecuteHandoff(
            Guid periodId,
            [FromBody] ExecutePayrollHandoffCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _handoffService.ExecuteHandoffAsync(periodId, command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimePayrollHandoffPackageDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimePayrollHandoffPackageDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
