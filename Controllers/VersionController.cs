using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("02-Version")]
    public class VersionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public VersionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
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
