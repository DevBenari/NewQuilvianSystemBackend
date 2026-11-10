using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/human-resource/lifecycle-management/onboarding-access-requests")]
    [Route("api/v1/corporate/human-resource/onboarding-access-requests")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Permintaan Akses Onboarding",
        AreaName = "Corporate",
        ControllerName = "WorkforceOnboardingAccessRequest",
        Description = "Permintaan pembuatan akun aplikasi pegawai baru",
        SortOrder = 35)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Onboarding Access Request")]
    public class HrdOnboardingAccessRequestController : ControllerBase
    {
        private readonly IHrdOnboardingAccessService _accessService;

        public HrdOnboardingAccessRequestController(IHrdOnboardingAccessService accessService)
        {
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Permintaan Akses Onboarding", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceOnboardingAccessRequest", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] OnboardingAccessQuery query, CancellationToken cancellationToken)
        {
            var result = await _accessService.GetPagedAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [AccessAction("Read", "Melihat Permintaan Akses Onboarding", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceOnboardingAccessRequest", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _accessService.GetByIdAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/complete")]
        [AccessAction("Complete", "Menyelesaikan Permintaan Akses", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkforceOnboardingAccessRequest", "Complete")]
        public async Task<IActionResult> MarkCompleted(
            Guid id,
            [FromBody] CompleteAccessRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _accessService.MarkCompletedAsync(id, request, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/retry")]
        [AccessAction("Retry", "Mengulang Pembuatan Akun", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceOnboardingAccessRequest", "Retry")]
        public async Task<IActionResult> RetryAccountCreation(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _accessService.RetryAccountCreationAsync(id, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
