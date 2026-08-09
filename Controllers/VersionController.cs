using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.System;

namespace QuilvianSystemBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("02-Version")]
    public class VersionController : ControllerBase
    {
        private readonly ApplicationVersionService _applicationVersionService;

        public VersionController(ApplicationVersionService applicationVersionService)
        {
            _applicationVersionService = applicationVersionService;
        }

        [HttpGet("/api/v1/system/version")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ApplicationVersionInfoResponse>), StatusCodes.Status200OK)]
        public IActionResult GetApplicationVersion()
        {
            var response = _applicationVersionService.GetRuntimeVersionInfo();

            return Ok(ApiResponse<ApplicationVersionInfoResponse>.Ok(
                response,
                "Informasi version aplikasi berhasil diambil."));
        }

        [HttpGet("/api/v1/system/version/history")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<ApplicationReleaseHistoryResponse>>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApplicationVersionHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var response = await _applicationVersionService.GetHistoryAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ApplicationReleaseHistoryResponse>>.Ok(
                response,
                "Riwayat version aplikasi berhasil diambil."));
        }

        [HttpGet("version")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<AppVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVersion(CancellationToken cancellationToken)
        {
            var response = await _applicationVersionService.GetCurrentVersionAsync(cancellationToken);

            return Ok(ApiResponse<AppVersionResponse>.Ok(
                response,
                "Informasi version aplikasi berhasil diambil."
            ));
        }
    }
}
