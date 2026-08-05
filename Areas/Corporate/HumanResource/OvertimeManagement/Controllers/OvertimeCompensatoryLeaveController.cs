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
    [Route("api/v1/corporate/human-resource/overtime-management/compensatory-leaves")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Compensatory Leave",
        AreaName = "Corporate",
        ControllerName = "OvertimeCompensatoryLeave",
        Description = "Posting verified overtime as compensatory leave credit into the Leave balance ledger",
        SortOrder = 5)]
    [Tags("Corporate / Human Resource / Overtime Management / Compensatory Leave")]
    public class OvertimeCompensatoryLeaveController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeCompensatoryLeaveQueryService _queryService;
        private readonly OvertimeCompensatoryLeaveService _service;
        private readonly LoggerService _loggerService;

        public OvertimeCompensatoryLeaveController(
            OvertimeCompensatoryLeaveQueryService queryService,
            OvertimeCompensatoryLeaveService service,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Compensatory Leave", Description = "Melihat metadata filter compensatory leave", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeCompensatoryLeave", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<OvertimeCompensatoryLeaveFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata overtime compensatory leave berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Compensatory Leave", Description = "Melihat ringkasan compensatory leave", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeCompensatoryLeave", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimeCompensatoryLeaveQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimeCompensatoryLeaveSummaryResponse>.Ok(
                data,
                "Ringkasan overtime compensatory leave berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Compensatory Leave", Description = "Melihat daftar compensatory leave", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeCompensatoryLeave", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimeCompensatoryLeaveQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeCompensatoryLeaveListResponse>>.Ok(
                data,
                "Data overtime compensatory leave berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Compensatory Leave", Description = "Melihat pilihan compensatory leave", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeCompensatoryLeave", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? compensatoryStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(
                search,
                compensatoryStatus,
                pageNumber,
                pageSize,
                cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeCompensatoryLeaveOptionResponse>>.Ok(
                data,
                "Pilihan overtime compensatory leave berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Compensatory Leave", Description = "Melihat detail compensatory leave dan leave ledger", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeCompensatoryLeave", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Compensatory leave tidak ditemukan."))
                : Ok(ApiResponse<OvertimeCompensatoryLeaveDetailResponse>.Ok(data, "Detail overtime compensatory leave berhasil diambil."));
        }

        [HttpPost("realizations/{realizationId:guid}/preview")]
        [AccessAction("Preview", "Preview Overtime Compensatory Leave", Description = "Menghitung preview konversi verified overtime menjadi compensatory leave", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimeCompensatoryLeave", "Preview")]
        public async Task<IActionResult> Preview(
            Guid realizationId,
            [FromBody] PreviewOvertimeCompensatoryLeaveRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.PreviewAsync(realizationId, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/post")]
        [AccessAction("Post", "Post Overtime Compensatory Leave", Description = "Membuat credit dan memposting earned days ke leave balance", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OvertimeCompensatoryLeave", "Post")]
        public async Task<IActionResult> Post(
            Guid realizationId,
            [FromBody] PostOvertimeCompensatoryLeaveRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.PostAsync(realizationId, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeCompensatoryLeave.Post",
                    "Memposting verified overtime menjadi compensatory leave.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reverse")]
        [AccessAction("Reverse", "Reverse Overtime Compensatory Leave", Description = "Membatalkan credit yang belum digunakan dan membuat reversal leave ledger", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeCompensatoryLeave", "Reverse")]
        public async Task<IActionResult> Reverse(
            Guid id,
            [FromBody] ReverseOvertimeCompensatoryLeaveRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.ReverseAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeCompensatoryLeave.Reverse",
                    "Mereversal overtime compensatory leave.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reconcile")]
        [AccessAction("Reconcile", "Reconcile Overtime Compensatory Leave", Description = "Memeriksa konsistensi credit, realization, dan leave balance transaction", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OvertimeCompensatoryLeave", "Reconcile")]
        public async Task<IActionResult> Reconcile(
            Guid id,
            [FromBody] ReconcileOvertimeCompensatoryLeaveRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.ReconcileAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeCompensatoryLeave.Reconcile",
                    "Menjalankan reconciliation overtime compensatory leave.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimeCompensatoryLeaveServiceResult<T> result) =>
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
    }
}
