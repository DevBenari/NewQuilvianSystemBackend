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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/recruitment/talent-pool")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Corporate / Human Resource / Recruitment Management / Talent Pool")]
    public sealed class TalentPoolController : ControllerBase
    {
        private readonly ITalentPoolService _talentPoolService;

        public TalentPoolController(ITalentPoolService talentPoolService)
        {
            _talentPoolService = talentPoolService ?? throw new ArgumentNullException(nameof(talentPoolService));
        }

        [HttpGet]
        [AccessPermission("Recruitment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<TalentPoolListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] TalentPoolQuery query, CancellationToken cancellationToken)
        {
            var result = await _talentPoolService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessPermission("Recruitment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<TalentPoolResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _talentPoolService.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("enroll")]
        [AccessPermission("Recruitment", "Create")]
        [ProducesResponseType(typeof(ApiResponse<TalentPoolResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enroll([FromBody] EnrollTalentPoolRequest request, CancellationToken cancellationToken)
        {
            var result = await _talentPoolService.EnrollAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/fast-track-promote")]
        [AccessPermission("Recruitment", "Update")]
        [ProducesResponseType(typeof(ApiResponse<TalentPoolResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> FastTrackPromote(
            Guid id,
            [FromBody] FastTrackPromoteRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _talentPoolService.FastTrackPromoteAsync(id, request, User, cancellationToken);
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
