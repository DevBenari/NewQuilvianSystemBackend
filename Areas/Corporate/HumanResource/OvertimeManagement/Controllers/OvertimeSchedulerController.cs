using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime-management/scheduler-jobs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Scheduler",
        AreaName = "Corporate",
        ControllerName = "OvertimeScheduler",
        Description = "Overtime lifecycle scheduler job queue and execution history",
        SortOrder = 8)]
    [Tags("Corporate / Human Resource / Overtime Management / Scheduler")]
    public class OvertimeSchedulerController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeSchedulerService _service;
        private readonly LoggerService _loggerService;

        public OvertimeSchedulerController(
            OvertimeSchedulerService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Scheduler", Description = "Melihat metadata scheduler", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeScheduler", "Read")]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<OvertimeSchedulerFilterMetadataResponse>.Ok(_service.GetMetadata(), "Metadata scheduler berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Scheduler", Description = "Melihat ringkasan scheduler job", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeScheduler", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimeSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimeSchedulerSummaryResponse>.Ok(data, "Ringkasan scheduler berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Scheduler", Description = "Melihat daftar scheduler job", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeScheduler", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimeSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeSchedulerJobListResponse>>.Ok(data, "Data scheduler job berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Scheduler", Description = "Melihat detail scheduler job", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeScheduler", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await _service.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Scheduler job tidak ditemukan."))
                : Ok(ApiResponse<OvertimeSchedulerJobDetailResponse>.Ok(data, "Detail scheduler job berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Enqueue", "Enqueue Overtime Scheduler", Description = "Membuat scheduler job baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimeScheduler", "Enqueue")]
        public async Task<IActionResult> Enqueue(
            [FromBody] EnqueueOvertimeSchedulerJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.EnqueueAsync(request, actor, cancellationToken);
            await LogAsync("OvertimeScheduler.Enqueue", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("periods/{periodId:guid}")]
        [AccessAction("Enqueue", "Enqueue Overtime Period Scheduler", Description = "Membuat scheduler job berdasarkan overtime period", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimeScheduler", "Enqueue")]
        public async Task<IActionResult> EnqueuePeriod(
            Guid periodId,
            [FromBody] EnqueueOvertimePeriodJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.EnqueuePeriodAsync(periodId, request, actor, cancellationToken);
            await LogAsync("OvertimeScheduler.EnqueuePeriod", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Overtime Scheduler", Description = "Membatalkan scheduler job yang belum Running", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimeScheduler", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelOvertimeSchedulerJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CancelAsync(id, request, actor, cancellationToken);
            await LogAsync("OvertimeScheduler.Cancel", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/retry")]
        [AccessAction("Retry", "Retry Overtime Scheduler", Description = "Menjadwalkan ulang scheduler job Failed atau CompletedWithIssues", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeScheduler", "Retry")]
        public async Task<IActionResult> Retry(
            Guid id,
            [FromBody] RetryOvertimeSchedulerJobRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.RetryAsync(id, request, actor, cancellationToken);
            await LogAsync("OvertimeScheduler.Retry", result.Data, result.Success);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimeClosingServiceResult<T> result) =>
            result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private IActionResult UnauthorizedResult() =>
            Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private async Task LogAsync(string action, object? data, bool success)
        {
            if (!success || data == null) return;
            await _loggerService.InfoAsync(LogCategory, action, action, data);
        }
    }
}
