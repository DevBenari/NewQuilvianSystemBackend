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
    [Route("api/v1/corporate/human-resource/recruitment/offers")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "Penawaran Kerja",
        AreaName = "Corporate",
        ControllerName = "JobOffer",
        Description = "Pembuatan, pengiriman, dan tanggapan penawaran kerja",
        SortOrder = 15)]
    [Tags("Corporate / Human Resource / Recruitment Management / Job Offer")]
    public sealed class JobOfferController : ControllerBase
    {
        private readonly IJobOfferService _service;

        public JobOfferController(IJobOfferService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Penawaran Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobOffer", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<JobOfferListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] JobOfferQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Penawaran Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JobOffer", "Read")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [AccessAction("Create", "Membuat Penawaran Kerja", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JobOffer", "Create")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create(
            [FromBody] CreateJobOfferRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data surat penawaran tidak valid."));
            }

            var result = await _service.CreateAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Penawaran Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobOffer", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateJobOfferRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data pembaruan surat penawaran tidak valid."));
            }

            var result = await _service.UpdateAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/send-conditional")]
        [AccessAction("Update", "Mengubah Penawaran Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobOffer", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SendConditionalOffer(
            [FromRoute] Guid id,
            [FromBody] SendConditionalOfferRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SendConditionalOfferAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/candidate-respond")]
        [AccessAction("Update", "Mengubah Penawaran Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JobOffer", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CandidateRespond(
            [FromRoute] Guid id,
            [FromBody] CandidateOfferResponseRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data respons kandidat tidak valid."));
            }

            var result = await _service.CandidateRespondAsync(id, request, User, cancellationToken);
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
