using System;
using System.Collections.Generic;
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
    [Route("api/v1/corporate/human-resource/recruitment/screenings")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Seleksi Administrasi Kandidat",
        AreaName = "Corporate",
        ControllerName = "CandidateScreening",
        Description = "Penyaringan berkas kandidat",
        SortOrder = 13)]
    [Tags("Corporate / Human Resource / Recruitment Management / Candidate Screening")]
    public sealed class CandidateScreeningController : ControllerBase
    {
        private readonly ICandidateScreeningService _service;

        public CandidateScreeningController(ICandidateScreeningService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Seleksi Administrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CandidateScreening", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CandidateScreeningListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CandidateScreeningQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Seleksi Administrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CandidateScreening", "Read")]
        [ProducesResponseType(typeof(ApiResponse<CandidateScreeningResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [AccessAction("Evaluate", "Menilai Seleksi Administrasi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("CandidateScreening", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<CandidateScreeningResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Initiate(
            [FromBody] InitiateScreeningRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data penapisan tidak valid."));
            }

            var result = await _service.InitiateScreeningAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("evaluate")]
        [AccessAction("Evaluate", "Menilai Seleksi Administrasi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("CandidateScreening", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<CandidateScreeningResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Evaluate(
            [FromBody] EvaluateScreeningRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data evaluasi tidak valid."));
            }

            var result = await _service.EvaluateScreeningAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("candidates/{candidateId:guid}/documents")]
        [AccessAction("Read", "Melihat Seleksi Administrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CandidateScreening", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<CandidateDocumentResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocuments(
            [FromRoute] Guid candidateId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetCandidateDocumentsAsync(candidateId, cancellationToken);
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
