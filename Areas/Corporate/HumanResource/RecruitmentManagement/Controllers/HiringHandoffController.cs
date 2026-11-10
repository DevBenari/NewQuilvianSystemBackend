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
    [Route("api/v1/corporate/human-resource/recruitment/handoffs")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Serah Terima Rekrutmen",
        AreaName = "Corporate",
        ControllerName = "HiringHandoff",
        Description = "Serah terima kandidat diterima ke onboarding",
        SortOrder = 19)]
    [Tags("Corporate / Human Resource / Recruitment Management / Hiring Handoff")]
    public sealed class HiringHandoffController : ControllerBase
    {
        private readonly IHiringHandoffService _handoffService;

        public HiringHandoffController(IHiringHandoffService handoffService)
        {
            _handoffService = handoffService ?? throw new ArgumentNullException(nameof(handoffService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Serah Terima Rekrutmen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HiringHandoff", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HiringHandoffListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] HiringHandoffQuery query, CancellationToken cancellationToken)
        {
            var result = await _handoffService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Serah Terima Rekrutmen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HiringHandoff", "Read")]
        [ProducesResponseType(typeof(ApiResponse<HiringHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _handoffService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("generate")]
        [AccessAction("Execute", "Menjalankan Serah Terima Rekrutmen", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HiringHandoff", "Execute")]
        [ProducesResponseType(typeof(ApiResponse<HiringHandoffResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GenerateHandoff([FromBody] GenerateHiringHandoffRequest request, CancellationToken cancellationToken)
        {
            var result = await _handoffService.GenerateHandoffAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/confirm-transfer")]
        [AccessAction("Execute", "Menjalankan Serah Terima Rekrutmen", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HiringHandoff", "Execute")]
        [ProducesResponseType(typeof(ApiResponse<HiringHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmTransfer(
            Guid id,
            [FromBody] ConfirmHiringHandoffRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _handoffService.ConfirmTransferAsync(id, request, User, cancellationToken);
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
