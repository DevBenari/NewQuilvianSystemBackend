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
    [Route("api/v1/corporate/human-resource/recruitment/requisitions")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Permintaan Tenaga Kerja",
        AreaName = "Corporate",
        ControllerName = "JobRequisition",
        Description = "Pengajuan dan pembatalan permintaan tenaga kerja",
        SortOrder = 10)]
    [Tags("Corporate / Human Resource / Recruitment Management / Job Requisition")]
    public sealed class JobRequisitionController : ControllerBase
    {
        private readonly IJobRequisitionService _service;

        public JobRequisitionController(IJobRequisitionService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Permintaan Tenaga Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobRequisition", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<JobRequisitionListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] JobRequisitionQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Permintaan Tenaga Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobRequisition", "Read")]
        [ProducesResponseType(typeof(ApiResponse<JobRequisitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [AccessAction("Create", "Membuat Permintaan Tenaga Kerja", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JobRequisition", "Create")]
        [ProducesResponseType(typeof(ApiResponse<JobRequisitionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create(
            [FromBody] CreateJobRequisitionRequest request,
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
        [AccessAction("Update", "Mengubah Permintaan Tenaga Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobRequisition", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobRequisitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateJobRequisitionRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload tidak valid."));
            }

            var result = await _service.UpdateAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Mengajukan Permintaan Tenaga Kerja", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("JobRequisition", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<JobRequisitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(
            [FromRoute] Guid id,
            [FromBody] SubmitJobRequisitionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Membatalkan Permintaan Tenaga Kerja", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("JobRequisition", "Cancel")]
        [ProducesResponseType(typeof(ApiResponse<JobRequisitionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelJobRequisitionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, request, User, cancellationToken);
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
