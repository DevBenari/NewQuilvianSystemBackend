using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/payroll/settings")]
    [Route("api/payroll/settings")]
    [Tags("HR Payroll - Settings")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL",
        moduleName: "Human Resource Payroll",
        displayName: "Payroll Settings",
        AreaName = "Corporate",
        ControllerName = "PayrollSettingModern",
        Description = "Manajemen parameter konfigurasi master penggajian, batas plafon BPJS, dan ambang batas variansi",
        SortOrder = 1)]
    public class PayrollSettingModernController : ControllerBase
    {
        private readonly IPayrollSettingService _settingService;

        public PayrollSettingModernController(IPayrollSettingService settingService)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        [HttpGet]
        [AccessAction("View", "Melihat Pengaturan Payroll", Description = "Melihat konfigurasi tarif BPJS, plafon, cut-off, dan threshold variansi gaji aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PayrollSettingModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<HrdPayrollSettingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken = default)
        {
            var setting = await _settingService.GetSettingAsync(cancellationToken);
            return Ok(ApiResponse<HrdPayrollSettingDto>.Ok(setting, "Pengaturan master penggajian berhasil diambil."));
        }

        [HttpPut]
        [AccessAction("Update", "Memperbarui Pengaturan Payroll", Description = "Memperbarui konfigurasi tarif BPJS, plafon, cut-off, dan threshold variansi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PayrollSettingModern", "Update")]
        [ProducesResponseType(typeof(ApiResponse<HrdPayrollSettingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSettings(
            [FromBody] UpdateHrdPayrollSettingCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var setting = await _settingService.UpdateSettingAsync(command, actorUserId, cancellationToken);
            return Ok(ApiResponse<HrdPayrollSettingDto>.Ok(setting, "Pengaturan master penggajian berhasil diperbarui."));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
