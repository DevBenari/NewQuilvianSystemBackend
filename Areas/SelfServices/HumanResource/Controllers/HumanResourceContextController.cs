using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/context")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "Human Resource Context",
        AreaName = "SelfServices",
        ControllerName = "HumanResourceContext",
        Description = "Authenticated employee, organization, manager, and role context for self-service applications",
        SortOrder = 1,
        VisibleInRoleAccess = false,
        IsSystemOnly = true)]
    [Tags("Self Services / Human Resource / Context")]
    public class HumanResourceContextController : ControllerBase
    {
        private readonly HumanResourceContextService _service;

        public HumanResourceContextController(HumanResourceContextService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<HumanResourceUserContextDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Read",
            "Read Human Resource Context",
            Description = "Membaca konteks HR dari user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetCurrentAsync(cancellationToken);
                return Ok(ApiResponse<HumanResourceUserContextDto>.Ok(
                    result,
                    "Konteks human resource user login berhasil diambil."));
            }
            catch (UnauthorizedAccessException exception)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    exception.Message));
            }
        }
    }
}
