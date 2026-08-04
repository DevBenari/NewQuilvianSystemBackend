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
    [Route("api/v1/corporate/human-resource/leave/accrual-runs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Accrual Run",
        AreaName = "Corporate",
        ControllerName = "LeaveAccrualRun",
        Description = "Leave accrual preview, processing, scheduler queue, retry, and reconciliation",
        SortOrder = 4)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Accrual")]
    public class LeaveAccrualController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";
        private readonly LeaveAccrualProcessorService _processor;
        private readonly LeaveAccrualSchedulerService _scheduler;
        private readonly LoggerService _loggerService;

        public LeaveAccrualController(
            LeaveAccrualProcessorService processor,
            LeaveAccrualSchedulerService scheduler,
            LoggerService loggerService)
        {
            _processor = processor;
            _scheduler = scheduler;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Accrual Run", Description = "Melihat metadata filter leave accrual run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeaveAccrualRunFilterMetadataResponse>.Ok(
                _processor.GetMetadata(),
                "Metadata filter leave accrual run berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Accrual Run", Description = "Melihat ringkasan leave accrual run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveAccrualRunQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _processor.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveAccrualRunSummaryResponse>.Ok(
                data,
                "Ringkasan leave accrual run berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Accrual Run", Description = "Melihat daftar leave accrual run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveAccrualRunQueryRequest request,
            CancellationToken cancellationToken)
        {
            var data = await _processor.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveAccrualRunPagedResponse>.Ok(
                data,
                "Data leave accrual run berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Accrual Run", Description = "Melihat detail leave accrual run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("{id:guid}/reconciliation")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualReconciliationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Accrual Reconciliation", Description = "Melihat reconciliation leave accrual run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public async Task<IActionResult> GetReconciliation(
            Guid id,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.ReconcileAsync(id, cancellationToken));
        }

        [HttpPost("preview")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualPreviewResponse>), StatusCodes.Status200OK)]
        [AccessAction("Preview", "Preview Leave Accrual", Description = "Menghitung preview accrual tanpa posting saldo", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LeaveAccrualRun", "Read")]
        public async Task<IActionResult> Preview(
            [FromBody] LeaveAccrualPreviewRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.PreviewAsync(request, cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunActionResponse>), StatusCodes.Status201Created)]
        [AccessAction("Create", "Create Leave Accrual Run", Description = "Membuat draft atau antrean leave accrual run", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LeaveAccrualRun", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLeaveAccrualRunRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _processor.CreateRunAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "LeaveAccrualRun.Create",
                    "Membuat leave accrual run.",
                    new { result.Data?.Id, result.Data?.RunNumber, result.Data?.RunStatus });
            }

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Execute", "Execute Leave Accrual Run", Description = "Menjalankan kalkulasi dan posting leave accrual", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveAccrualRun", "Execute")]
        public async Task<IActionResult> Execute(
            Guid id,
            [FromBody] ExecuteLeaveAccrualRunRequest request,
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
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Retry", "Retry Leave Accrual Run", Description = "Menjadwalkan ulang leave accrual run yang gagal", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveAccrualRun", "Retry")]
        public async Task<IActionResult> Retry(
            Guid id,
            [FromBody] RetryLeaveAccrualRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.RetryAsync(
                id,
                GetCurrentUserId(),
                request.Reason,
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualRunActionResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Leave Accrual Run", Description = "Membatalkan leave accrual run Draft atau Queued", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveAccrualRun", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLeaveAccrualRunRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _processor.CancelAsync(
                id,
                GetCurrentUserId(),
                request.Reason,
                cancellationToken));
        }

        [HttpPost("scheduler/enqueue-due")]
        [ProducesResponseType(typeof(ApiResponse<LeaveAccrualEnqueueResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Enqueue Due Leave Accrual", Description = "Membuat run accrual yang jatuh tempo berdasarkan policy", AccessType = AccessTypes.Create, SortOrder = 7)]
        [AccessPermission("LeaveAccrualRun", "Create")]
        public async Task<IActionResult> EnqueueDue(
            [FromBody] EnqueueDueLeaveAccrualRequest request,
            CancellationToken cancellationToken)
        {
            var date = request.AccrualDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var result = await _scheduler.EnqueueDueRunsAsync(
                date,
                GetCurrentUserId(),
                request.QueueForProcessing,
                cancellationToken);

            return Ok(ApiResponse<LeaveAccrualEnqueueResponse>.Ok(
                result,
                "Due leave accrual run berhasil dievaluasi."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveAccrualServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);
            return StatusCode(result.StatusCode, response);
        }
    }
}
