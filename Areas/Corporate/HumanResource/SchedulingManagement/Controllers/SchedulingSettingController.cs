using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/scheduling-management/settings")]
    [Route("api/corporate/human-resource/scheduling/settings")]
    [Route("api/v1/corporate/human-resource/scheduling/settings")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "Scheduling Settings Administration",
        AreaName = "Corporate",
        ControllerName = "SchedulingSetting",
        Description = "Pengelolaan parameter korporat penjadwalan dinas, batas jeda istirahat, dan toleransi koreksi mundur rumah sakit",
        SortOrder = 23)]
    [Tags("HR Scheduling - Settings")]
    public class SchedulingSettingController : ControllerBase
    {
        private readonly ISchedulingSettingService _settingService;

        public SchedulingSettingController(ISchedulingSettingService settingService)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        /// <summary>
        /// Membaca konfigurasi kebijakan penjadwalan dan toleransi waktu kerja rumah sakit.
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Read Scheduling Settings", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrdSchedulingSetting", "Read")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<SchedulingSettingResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSetting(
            [FromQuery] Guid? hospitalSiteId,
            CancellationToken cancellationToken)
        {
            var result = await _settingService.GetSettingAsync(hospitalSiteId ?? Guid.Empty, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Memperbarui parameter aturan waktu kerja korporat rumah sakit (jeda 11 jam, hari berurutan, toleransi koreksi mundur).
        /// Mengakomodasi HRD-DEC-099, HRD-DEC-102, VR-SCH-80.
        /// </summary>
        [HttpPut]
        [AccessAction("Manage", "Manage Scheduling Settings", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HrdSchedulingSetting", "Manage")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(RecruitmentResult<SchedulingSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RecruitmentResult<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateSetting(
            [FromQuery] Guid? hospitalSiteId,
            [FromBody] UpdateSchedulingSettingRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(RecruitmentResult<SchedulingSettingResponse>.BadRequest("Validasi parameter pengaturan gagal.", errors));
            }

            var result = await _settingService.UpdateSettingAsync(hospitalSiteId ?? Guid.Empty, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
