using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/recruitment/mcu-clearance")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "MCU Pra-Kerja",
        AreaName = "Corporate",
        ControllerName = "OccupationalHealthClearance",
        Description = "Registrasi dan hasil MCU pra-kerja kandidat",
        SortOrder = 17)]
    [Tags("Corporate / Human Resource / Recruitment Management / MCU Clearance")]
    public sealed class McuClearanceController : ControllerBase
    {
        private readonly IMcuClearanceService _mcuService;

        public McuClearanceController(IMcuClearanceService mcuService)
        {
            _mcuService = mcuService ?? throw new ArgumentNullException(nameof(mcuService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat MCU Pra-Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthClearance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<McuClearanceListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] McuClearanceQuery query, CancellationToken cancellationToken)
        {
            var result = await _mcuService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat MCU Pra-Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthClearance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mcuService.GetByIdAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("register")]
        [AccessAction("Create", "Mendaftarkan MCU Pra-Kerja", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OccupationalHealthClearance", "Create")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegisterMcu([FromBody] RegisterMcuRequest request, CancellationToken cancellationToken)
        {
            var result = await _mcuService.RegisterMcuAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/record-result")]
        [AccessAction("Evaluate", "Mencatat Hasil MCU Pra-Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OccupationalHealthClearance", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RecordResult(
            Guid id,
            [FromBody] RecordMcuResultRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mcuService.RecordResultAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(RecruitmentResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
