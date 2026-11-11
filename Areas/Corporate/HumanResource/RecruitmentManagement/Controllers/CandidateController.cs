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
    [Route("api/v1/corporate/human-resource/recruitment/candidates")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Kandidat",
        AreaName = "Corporate",
        ControllerName = "Candidate",
        Description = "Registrasi kandidat, lamaran lowongan, dan dokumen kandidat",
        SortOrder = 12)]
    [Tags("Corporate / Human Resource / Recruitment Management / Candidate")]
    public sealed class CandidateController : ControllerBase
    {
        private readonly ICandidateService _service;

        public CandidateController(ICandidateService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Kandidat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Candidate", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CandidateListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CandidateQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Kandidat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Candidate", "Read")]
        [ProducesResponseType(typeof(ApiResponse<CandidateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [AccessAction("Create", "Mendaftarkan Kandidat", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Candidate", "Create")]
        [ProducesResponseType(typeof(ApiResponse<CandidateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCandidateRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi formulir tidak valid."));
            }

            var result = await _service.RegisterAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Data Kandidat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Candidate", "Update")]
        [ProducesResponseType(typeof(ApiResponse<CandidateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateCandidateRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi formulir tidak valid."));
            }

            var result = await _service.UpdateAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/apply")]
        [AccessAction("Update", "Mengubah Data Kandidat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Candidate", "Update")]
        [ProducesResponseType(typeof(ApiResponse<CandidateApplicationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ApplyVacancy(
            [FromRoute] Guid id,
            [FromBody] ApplyVacancyRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi lamaran tidak valid."));
            }

            var result = await _service.ApplyVacancyAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/documents")]
        [AccessAction("UploadDocument", "Mengunggah Dokumen Kandidat", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Candidate", "UploadDocument")]
        [ProducesResponseType(typeof(ApiResponse<CandidateDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadDocument(
            [FromRoute] Guid id,
            [FromBody] UploadCandidateDocumentRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data dokumen tidak valid."));
            }

            var result = await _service.UploadDocumentAsync(id, request, User, cancellationToken);
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
