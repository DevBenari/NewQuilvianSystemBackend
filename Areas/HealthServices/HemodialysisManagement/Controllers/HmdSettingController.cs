using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants.HemodialysisPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Pengaturan unit HD — satu baris per unit (<c>BE-HMD-05</c>).
    /// </summary>
    /// <remarks>
    /// Varian sah "master data pengaturan tunggal" pada <c>master-data-endpoint-standard.md</c>
    /// bagian 4: tidak ada koleksi data, sehingga metadata, summary, options, dan delete tidak
    /// berlaku. Batas rasio perawat, masa berlaku hasil air, dan sakelar penegakan kewenangan
    /// disimpan di sini dan tidak pernah ditanam di kode maupun di frontend.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/master-data/hemodialysis-settings")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Setting",
        AreaName = P.AreaName,
        ControllerName = P.Setting.Resource,
        Description = "Pengaturan unit hemodialisa",
        SortOrder = 17
    )]
    [Tags("Health Services / Hemodialysis Management / Master Data / Hemodialysis Setting")]
    public class HmdSettingController : ControllerBase
    {
        private readonly HmdResourceService _service;
        private readonly LoggerService _logger;

        public HmdSettingController(HmdResourceService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{serviceUnitId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Setting", Description = "Membaca pengaturan unit HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Setting.Resource, P.Read)]
        public async Task<IActionResult> Get(Guid serviceUnitId, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetSettingAsync(serviceUnitId, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pengaturan unit hemodialisa belum tersedia untuk unit ini."))
                : Ok(ApiResponse<HmdSettingResponse>.Ok(result, "Pengaturan unit HD berhasil diambil."));
        }

        [HttpPut("{serviceUnitId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Update, "Update Hemodialysis Setting", Description = "Memperbarui pengaturan unit HD", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.Setting.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid serviceUnitId, [FromBody] UpdateHmdSettingRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.UpdateSettingAsync(serviceUnitId, request, HmdHttp.ActorId(User), cancellationToken),
                "Pengaturan unit HD berhasil diperbarui.", _logger, P.Setting.Resource, P.Update,
                x => new
                {
                    x.Id,
                    x.ServiceUnitId,
                    x.MaxPatientsPerNurse,
                    x.EnforceNurseRatio,
                    x.WaterResultValidityHours,
                    x.EnforceCompetencyCheck,
                    x.AllowMultipleActiveEpisodePerPatient,
                    x.RequireDifferentSigner
                });
    }
}
