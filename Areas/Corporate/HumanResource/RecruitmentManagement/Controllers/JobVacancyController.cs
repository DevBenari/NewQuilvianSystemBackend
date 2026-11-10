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
    [Route("api/v1/corporate/human-resource/recruitment/vacancies")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Lowongan Kerja",
        AreaName = "Corporate",
        ControllerName = "JobVacancy",
        Description = "Pembuatan, publikasi, dan penutupan lowongan",
        SortOrder = 11)]
    [Tags("Corporate / Human Resource / Recruitment Management / Job Vacancy")]
    public sealed class JobVacancyController : ControllerBase
    {
        private readonly IJobVacancyService _service;

        public JobVacancyController(IJobVacancyService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Lowongan Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobVacancy", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<JobVacancyListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] JobVacancyQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Lowongan Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobVacancy", "Read")]
        [ProducesResponseType(typeof(ApiResponse<JobVacancyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [AccessAction("Create", "Membuat Lowongan Kerja", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JobVacancy", "Create")]
        [ProducesResponseType(typeof(ApiResponse<JobVacancyResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create(
            [FromBody] CreateJobVacancyRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload tidak valid."));
            }

            var result = await _service.CreateAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Lowongan Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobVacancy", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobVacancyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateJobVacancyRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload tidak valid."));
            }

            var result = await _service.UpdateAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/publish")]
        [AccessAction("Publish", "Memublikasikan Lowongan Kerja", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("JobVacancy", "Publish")]
        [ProducesResponseType(typeof(ApiResponse<JobVacancyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Publish(
            [FromRoute] Guid id,
            [FromBody] PublishJobVacancyRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.PublishAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/close")]
        [AccessAction("Close", "Menutup Lowongan Kerja", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("JobVacancy", "Close")]
        [ProducesResponseType(typeof(ApiResponse<JobVacancyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Close(
            [FromRoute] Guid id,
            [FromBody] CloseJobVacancyRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CloseAsync(id, request, User, cancellationToken);
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
