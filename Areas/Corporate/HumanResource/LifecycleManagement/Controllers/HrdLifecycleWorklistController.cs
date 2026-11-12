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
    [Route("api/v1/corporate/human-resource/lifecycle-management/worklist")]
    [Route("api/v1/corporate/human-resource/worklist")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Worklist Lifecycle Pegawai",
        AreaName = "Corporate",
        ControllerName = "WorkforceLifecycleWorklist",
        Description = "Daftar kerja lifecycle pegawai dan kelayakan penjadwalan",
        SortOrder = 36)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Lifecycle Worklist")]
    public class HrdLifecycleWorklistController : ControllerBase
    {
        private readonly IHrdLifecycleWorklistService _worklistService;
        private readonly IHrdEmploymentReadinessReadService _readinessService;

        public HrdLifecycleWorklistController(
            IHrdLifecycleWorklistService worklistService,
            IHrdEmploymentReadinessReadService readinessService)
        {
            _worklistService = worklistService ?? throw new ArgumentNullException(nameof(worklistService));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Worklist Lifecycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceLifecycleWorklist", "Read")]
        public async Task<IActionResult> GetWorklist([FromQuery] LifecycleWorklistQuery query, CancellationToken cancellationToken)
        {
            var result = await _worklistService.GetWorklistAsync(query, User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("counts")]
        [AccessAction("Read", "Melihat Worklist Lifecycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceLifecycleWorklist", "Read")]
        public async Task<IActionResult> GetCounts(CancellationToken cancellationToken)
        {
            var result = await _worklistService.GetCountsAsync(User, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("eligibility/{workforceProfileId:guid}")]
        [AccessAction("Read", "Melihat Worklist Lifecycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceLifecycleWorklist", "Read")]
        public async Task<IActionResult> GetSchedulingEligibility(
            Guid workforceProfileId,
            [FromQuery] DateOnly? targetDate,
            CancellationToken cancellationToken)
        {
            var date = targetDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await _readinessService.GetSchedulingEligibilityAsync(workforceProfileId, date, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
