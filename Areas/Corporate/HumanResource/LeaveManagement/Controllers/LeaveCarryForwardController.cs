using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/carry-forward-runs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Carry Forward Run",
        AreaName = "Corporate",
        ControllerName = "LeaveCarryForwardRun",
        Description = "Leave carry-forward, expiry, scheduler queue, retry, reversal, and reconciliation",
        SortOrder = 5)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Carry Forward")]
    public class LeaveCarryForwardController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";
        private readonly LeaveCarryForwardProcessorService _processor;
        private readonly LeaveCarryForwardSchedulerService _scheduler;
        private readonly LoggerService _loggerService;

        public LeaveCarryForwardController(
            LeaveCarryForwardProcessorService processor,
            LeaveCarryForwardSchedulerService scheduler,
            LoggerService loggerService)
        {
            _processor = processor;
            _scheduler = scheduler;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Carry Forward Run", Description = "Melihat metadata filter leave carry-forward run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeaveCarryForwardRunFilterMetadataResponse>.Ok(
                _processor.GetMetadata(),
                "Metadata filter leave carry-forward run berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Carry Forward Run", Description = "Melihat ringkasan leave carry-forward run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveCarryForwardRunQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _processor.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveCarryForwardRunSummaryResponse>.Ok(
                data,
                "Ringkasan leave carry-forward run berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Carry Forward Run", Description = "Melihat daftar leave carry-forward run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveCarryForwardRunQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _processor.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveCarryForwardRunPagedResponse>.Ok(
                data,
                "Data leave carry-forward run berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Carry Forward Run", Description = "Melihat detail leave carry-forward run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("{id:guid}/reconciliation")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardReconciliationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Carry Forward Reconciliation", Description = "Melihat reconciliation leave carry-forward run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> GetReconciliation(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.ReconcileAsync(id, cancellationToken));
        }

        [HttpPost("preview")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardPreviewResponse>), StatusCodes.Status200OK)]
        [AccessAction("Preview", "Preview Leave Carry Forward", Description = "Menghitung preview carry forward tanpa posting saldo", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> Preview(
            [FromBody] LeaveCarryForwardPreviewRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.PreviewAsync(request, cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunActionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Create", "Create Leave Carry Forward Run", Description = "Membuat draft atau antrean leave carry-forward run", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LeaveCarryForwardRun", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveCarryForwardRunRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _processor.CreateRunAsync(request, GetCurrentUserId(), cancellationToken);
            if (result.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "LeaveCarryForwardRun.Create",
                    "Membuat leave carry-forward run.",
                    new { result.Data?.Id, result.Data?.RunNumber, result.Data?.RunStatus });
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Execute", "Execute Leave Carry Forward Run", Description = "Menjalankan kalkulasi dan posting leave carry forward", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveCarryForwardRun", "Execute")]
        public async Task<IActionResult> Execute(
            Guid id,
            [FromBody] ExecuteLeaveCarryForwardRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.ExecuteRunAsync(
                id,
                GetCurrentUserId(),
                request.ForceReprocess,
                request.Notes,
                cancellationToken));
        }

        [HttpPost("{id:guid}/retry")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Retry", "Retry Leave Carry Forward Run", Description = "Menjadwalkan ulang leave carry-forward run yang gagal", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveCarryForwardRun", "Retry")]
        public async Task<IActionResult> Retry(
            Guid id,
            [FromBody] RetryLeaveCarryForwardRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.RetryAsync(id, GetCurrentUserId(), request.Reason, cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Leave Carry Forward Run", Description = "Membatalkan leave carry-forward run Draft atau Queued", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveCarryForwardRun", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLeaveCarryForwardRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.CancelAsync(id, GetCurrentUserId(), request.Reason, cancellationToken));
        }

        [HttpPost("{id:guid}/reverse")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Reverse", "Reverse Leave Carry Forward Run", Description = "Membuat reversal ledger untuk carry-forward run", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LeaveCarryForwardRun", "Reverse")]
        public async Task<IActionResult> Reverse(
            Guid id,
            [FromBody] ReverseLeaveCarryForwardRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.ReverseRunAsync(id, GetCurrentUserId(), request.Reason, cancellationToken));
        }

        [HttpPost("expiry/preview")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardExpiryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Preview", "Preview Leave Carry Forward Expiry", Description = "Menghitung saldo carry forward yang jatuh tempo", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("LeaveCarryForwardRun", "Read")]
        public async Task<IActionResult> PreviewExpiry(
            [FromBody] LeaveCarryForwardExpiryRequest request,
            CancellationToken cancellationToken)
        {
            request.IsDryRun = true;
            return ToActionResult(await _processor.ProcessExpiryAsync(request, GetCurrentUserId(), cancellationToken));
        }

        [HttpPost("expiry/execute")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardExpiryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Expire", "Execute Leave Carry Forward Expiry", Description = "Memposting expiry saldo carry forward yang jatuh tempo", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("LeaveCarryForwardRun", "Expire")]
        public async Task<IActionResult> ExecuteExpiry(
            [FromBody] LeaveCarryForwardExpiryRequest request,
            CancellationToken cancellationToken)
        {
            request.IsDryRun = false;
            return ToActionResult(await _processor.ProcessExpiryAsync(request, GetCurrentUserId(), cancellationToken));
        }

        [HttpPost("scheduler/enqueue-due")]
        [ProducesResponseType(typeof(ApiResponse<LeaveCarryForwardEnqueueResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Enqueue Due Leave Carry Forward", Description = "Membuat run carry forward yang jatuh tempo berdasarkan policy", AccessType = AccessTypes.Create, SortOrder = 10)]
        [AccessPermission("LeaveCarryForwardRun", "Create")]
        public async Task<IActionResult> EnqueueDue(
            [FromBody] EnqueueDueLeaveCarryForwardRequest request,
            CancellationToken cancellationToken)
        {
            var date = request.ExecutionDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var result = await _scheduler.EnqueueDueRunsAsync(
                date,
                GetCurrentUserId(),
                request.QueueForProcessing,
                cancellationToken);
            return Ok(ApiResponse<LeaveCarryForwardEnqueueResponse>.Ok(
                result,
                "Due leave carry-forward run berhasil dievaluasi."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveCarryForwardServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);
            return StatusCode(result.StatusCode, response);
        }
    }
}
