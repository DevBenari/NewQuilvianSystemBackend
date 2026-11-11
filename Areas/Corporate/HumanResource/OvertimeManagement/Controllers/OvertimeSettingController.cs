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
    [Route("api/v1/corporate/human-resource/overtime-settings")]
    [Route("api/overtime-settings")]
    [Tags("HR Overtime - Settings")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Settings",
        AreaName = "Corporate",
        ControllerName = "OvertimeSetting",
        Description = "Manajemen kebijakan kuota dan parameter operasional lembur rumah sakit",
        SortOrder = 10)]
    public class OvertimeSettingController : ControllerBase
    {
        private readonly IOvertimeSettingService _settingService;

        public OvertimeSettingController(IOvertimeSettingService settingService)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        [HttpGet]
        [AccessAction("View", "View Overtime Settings", Description = "Melihat konfigurasi kuota dan parameter operasional lembur RS", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeSetting", "View")]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken = default)
        {
            var settings = await _settingService.GetSettingsAsync(cancellationToken);
            return Ok(ApiResponse<OvertimeSettingDto>.Ok(settings, "Pengaturan operasional lembur berhasil diambil."));
        }

        [HttpPut]
        [AccessAction("Manage", "Update Overtime Settings", Description = "Mengubah konfigurasi kuota dan parameter operasional lembur RS", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OvertimeSetting", "Manage")]
        public async Task<IActionResult> UpdateSettings(
            [FromBody] UpdateOvertimeSettingCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetCurrentUserId();
            var settings = await _settingService.UpdateSettingsAsync(command, actorId, cancellationToken);
            return Ok(ApiResponse<OvertimeSettingDto>.Ok(settings, "Pengaturan operasional lembur berhasil diperbarui."));
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
