using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Context.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/context")]
    [Tags("Self Services / Human Resource / Context")]
    public class HumanResourceContextController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource";

        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly LoggerService _loggerService;

        public HumanResourceContextController(
            HumanResourceContextService humanResourceContextService,
            LoggerService loggerService)
        {
            _humanResourceContextService = humanResourceContextService;
            _loggerService = loggerService;
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(
            typeof(ApiResponse<HumanResourceUserContextDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrent(
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _humanResourceContextService.GetCurrentAsync(
                    cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "HumanResourceContext.GetCurrent",
                    "Context human resource user aktif berhasil diambil.",
                    new
                    {
                        result.UserId,
                        result.WorkforceProfileId,
                        result.ProfileType,
                        result.OrganizationAssignmentId,
                        result.ManagerWorkforceProfileId,
                        result.IsManager,
                        result.DirectReportCount,
                        result.IsContextComplete,
                        WarningCount = result.Warnings.Count
                    });

                return Ok(
                    ApiResponse<HumanResourceUserContextDto>.Ok(
                        result,
                        "Context human resource berhasil diambil."));
            }
            catch (UnauthorizedAccessException ex)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "HumanResourceContext.GetCurrent",
                    ex.Message);

                return Unauthorized(
                    ApiResponse<object>.Fail(
                        StatusCodes.Status401Unauthorized,
                        ex.Message));
            }
        }
    }

}
