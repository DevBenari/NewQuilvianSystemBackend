using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.System;

namespace QuilvianSystemBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("02-Version")]
    public class VersionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly ApplicationVersionService _applicationVersionService;

        public VersionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            ApplicationVersionService applicationVersionService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _applicationVersionService = applicationVersionService;
        }

        [HttpGet("/api/v1/system/version")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<ApplicationVersionInfoResponse>), StatusCodes.Status200OK)]
        public IActionResult GetApplicationVersion()
        {
            var response = _applicationVersionService.GetCurrentVersion();

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
        public async Task<IActionResult> GetVersion()
        {
            var latestVersion = await _dbContext.SysAppVersions
                .Where(x => x.IsActive && x.IsLatest)
                .OrderByDescending(x => x.ReleaseDateTime)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync();

            if (latestVersion == null)
            {
                latestVersion = await _dbContext.SysAppVersions
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.ReleaseDateTime)
                    .ThenByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync();
            }

            if (latestVersion == null)
            {
                await _loggerService.WarningAsync(
                    "System",
                    "GetVersion",
                    "Version aplikasi belum tersedia di database."
                );

                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Version aplikasi belum tersedia."
                ));
            }

            var response = new AppVersionResponse
            {
                AppName = latestVersion.AppName,
                BackendVersion = latestVersion.BackendVersion,
                ApiVersion = latestVersion.ApiVersion,
                FrontendMinimumVersion = latestVersion.FrontendMinimumVersion,
                FrontendRecommendedVersion = latestVersion.FrontendRecommendedVersion,
                ReleaseName = latestVersion.ReleaseName,
                Description = latestVersion.Description,
                ReleaseDateTime = latestVersion.ReleaseDateTime,
                ServerDateTime = DateTime.Now
            };

            await _loggerService.InfoAsync(
                "System",
                "GetVersion",
                "Mengambil informasi version aplikasi.",
                new
                {
                    response.AppName,
                    response.BackendVersion,
                    response.ApiVersion
                }
            );

            return Ok(ApiResponse<AppVersionResponse>.Ok(
                response,
                "Informasi version aplikasi berhasil diambil."
            ));
        }
    }
}
