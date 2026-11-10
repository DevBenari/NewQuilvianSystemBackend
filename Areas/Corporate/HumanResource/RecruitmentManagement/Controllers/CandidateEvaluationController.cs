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
    [Route("api/v1/corporate/human-resource/recruitment/evaluations")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Evaluasi Kandidat",
        AreaName = "Corporate",
        ControllerName = "CandidateEvaluation",
        Description = "Wawancara HR dan asesmen klinis kandidat",
        SortOrder = 14)]
    [Tags("Corporate / Human Resource / Recruitment Management / Candidate Evaluation")]
    public sealed class CandidateEvaluationController : ControllerBase
    {
        private readonly ICandidateEvaluationService _service;

        public CandidateEvaluationController(ICandidateEvaluationService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Evaluasi Kandidat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CandidateEvaluation", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CandidateEvaluationListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CandidateEvaluationQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Evaluasi Kandidat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CandidateEvaluation", "Read")]
        [ProducesResponseType(typeof(ApiResponse<CandidateEvaluationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("schedule")]
        [AccessAction("Evaluate", "Menilai Kandidat (HR)", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("CandidateEvaluation", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<CandidateEvaluationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Schedule(
            [FromBody] ScheduleEvaluationRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data penjadwalan evaluasi tidak valid."));
            }

            var result = await _service.ScheduleAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit-hr")]
        [AccessAction("Evaluate", "Menilai Kandidat (HR)", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("CandidateEvaluation", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<CandidateEvaluationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SubmitHr(
            [FromRoute] Guid id,
            [FromBody] SubmitHrEvaluationRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data evaluasi HR tidak valid."));
            }

            var result = await _service.SubmitHrEvaluationAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit-clinical")]
        [AccessAction("EvaluateClinical", "Menilai Asesmen Klinis Kandidat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CandidateEvaluation", "EvaluateClinical")]
        [ProducesResponseType(typeof(ApiResponse<CandidateEvaluationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SubmitClinical(
            [FromRoute] Guid id,
            [FromBody] SubmitClinicalEvaluationRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data evaluasi klinis tidak valid."));
            }

            var result = await _service.SubmitClinicalEvaluationAsync(id, request, User, cancellationToken);
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
