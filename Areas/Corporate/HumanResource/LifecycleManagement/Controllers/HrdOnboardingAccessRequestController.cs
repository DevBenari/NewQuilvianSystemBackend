using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-management/onboarding-access-requests")]
    [Route("api/v1/corporate/human-resource/onboarding-access-requests")]
    [Authorize]
    [Tags("Corporate / Human Resource / Lifecycle Management / Onboarding Access Request")]
    public class HrdOnboardingAccessRequestController : ControllerBase
    {
        private readonly IHrdOnboardingAccessService _accessService;

        public HrdOnboardingAccessRequestController(IHrdOnboardingAccessService accessService)
        {
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] OnboardingAccessQuery query, CancellationToken cancellationToken)
        {
            var result = await _accessService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _accessService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> MarkCompleted(
            Guid id,
            [FromBody] CompleteAccessRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _accessService.MarkCompletedAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/retry")]
        public async Task<IActionResult> RetryAccountCreation(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _accessService.RetryAccountCreationAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
