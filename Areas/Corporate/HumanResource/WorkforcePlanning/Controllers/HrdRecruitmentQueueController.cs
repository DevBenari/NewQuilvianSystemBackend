using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Controllers
{
    /// <summary>
    /// Controller antrean pemenuhan rekrutmen luar untuk staf Recruiter (BE-HRD-042).
    /// Memenuhi kontrak API v7 Bagian 13 grup Recruitment Queue.
    /// Menyajikan kursi rekrut luar yang siap diproses tanpa menampilkan estimasi biaya.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-planning/recruitment-queue")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PLANNING",
        moduleName: "Human Resource Workforce Planning",
        displayName: "Recruitment Queue",
        AreaName = "Corporate",
        ControllerName = "WorkforceRecruitmentQueue",
        Description = "Antrean kursi formasi eksternal yang siap diproses rekrutmen",
        SortOrder = 37)]
    [Tags("Corporate / Human Resource / Workforce Planning / Recruitment Queue")]
    public sealed class HrdRecruitmentQueueController : ControllerBase
    {
        private readonly IHrdRecruitmentQueueService _queueService;

        public HrdRecruitmentQueueController(IHrdRecruitmentQueueService queueService)
        {
            _queueService = queueService;
        }

        /// <summary>
        /// Mengambil daftar kursi rekrut luar berstatus Open dari permintaan yang telah disetujui (ReadyForRecruitment atau PartiallyFulfilled).
        /// Kursi Frozen tidak ditampilkan dan respons tidak menyertakan informasi biaya gaji (kriteria 1-2).
        /// </summary>
        [HttpGet]
        [AccessAction("ReadQueue", "Read Recruitment Queue", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRecruitmentQueue", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RecruitmentQueueItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQueue(
            [FromQuery] RecruitmentQueuePagedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _queueService.GetQueueAsync(query, GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PagedResult<RecruitmentQueueItemResponse>>.Ok(result, "Antrean rekrutmen berhasil diambil."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
