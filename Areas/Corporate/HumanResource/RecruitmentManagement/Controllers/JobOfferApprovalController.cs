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
    [Route("api/v1/corporate/human-resource/recruitment/offers/{offerId:guid}/approvals")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Corporate / Human Resource / Recruitment Management / Job Offer Approval")]
    public sealed class JobOfferApprovalController : ControllerBase
    {
        private readonly IJobOfferApprovalService _approvalService;

        public JobOfferApprovalController(IJobOfferApprovalService approvalService)
        {
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
        }

        [HttpGet]
        [AccessPermission("Recruitment", "Read")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferApprovalHistoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApprovals(Guid offerId, CancellationToken cancellationToken)
        {
            var result = await _approvalService.GetApprovalsByOfferIdAsync(offerId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("approve")]
        [AccessPermission("Recruitment", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferApprovalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ApproveStep(
            Guid offerId,
            [FromBody] ApproveJobOfferRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _approvalService.ApproveOfferStepAsync(offerId, request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("reject")]
        [AccessPermission("Recruitment", "Update")]
        [ProducesResponseType(typeof(ApiResponse<JobOfferApprovalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RejectStep(
            Guid offerId,
            [FromBody] RejectJobOfferRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _approvalService.RejectOfferStepAsync(offerId, request, User, cancellationToken);
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
