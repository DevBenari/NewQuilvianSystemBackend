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
    [Route("api/v1/corporate/human-resource/overtime-management/payroll-handoffs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Payroll Handoff",
        AreaName = "Corporate",
        ControllerName = "OvertimePayrollHandoff",
        Description = "Handoff verified cash-paid overtime to payroll input without calculating final monetary amount",
        SortOrder = 6)]
    [Tags("Corporate / Human Resource / Overtime Management / Payroll Handoff")]
    public class OvertimePayrollHandoffController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimePayrollHandoffQueryService _queryService;
        private readonly OvertimePayrollHandoffService _service;
        private readonly LoggerService _loggerService;

        public OvertimePayrollHandoffController(
            OvertimePayrollHandoffQueryService queryService,
            OvertimePayrollHandoffService service,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Payroll Handoff", Description = "Melihat metadata filter overtime payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoff", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<OvertimePayrollHandoffFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata overtime payroll handoff berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Payroll Handoff", Description = "Melihat ringkasan overtime payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoff", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimePayrollHandoffSummaryResponse>.Ok(
                data,
                "Ringkasan overtime payroll handoff berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Payroll Handoff", Description = "Melihat daftar overtime payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoff", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimePayrollHandoffQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimePayrollHandoffListResponse>>.Ok(
                data,
                "Data overtime payroll handoff berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Payroll Handoff", Description = "Melihat pilihan overtime realization untuk payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoff", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? handoffStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(
                search,
                handoffStatus,
                pageNumber,
                pageSize,
                cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimePayrollHandoffOptionResponse>>.Ok(
                data,
                "Pilihan overtime payroll handoff berhasil diambil."));
        }

        [HttpGet("realizations/{realizationId:guid}")]
        [AccessAction("Read", "Read Overtime Payroll Handoff", Description = "Melihat detail overtime payroll handoff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePayrollHandoff", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(realizationId, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Overtime realization tidak ditemukan."))
                : Ok(ApiResponse<OvertimePayrollHandoffDetailResponse>.Ok(data, "Detail overtime payroll handoff berhasil diambil."));
        }

        [HttpPost("realizations/{realizationId:guid}/preview")]
        [AccessAction("Preview", "Preview Overtime Payroll Handoff", Description = "Memvalidasi kesiapan verified overtime sebelum masuk payroll input", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimePayrollHandoff", "Preview")]
        public async Task<IActionResult> Preview(
            Guid realizationId,
            [FromBody] PreviewOvertimePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.PreviewAsync(realizationId, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/post")]
        [AccessAction("Post", "Post Overtime Payroll Handoff", Description = "Membuat TrxPayrollOvertimeInput dan menandai sumber Overtime sudah diposting", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OvertimePayrollHandoff", "Post")]
        public async Task<IActionResult> Post(
            Guid realizationId,
            [FromBody] PostOvertimePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.PostAsync(realizationId, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimePayrollHandoff.Post",
                    "Memposting verified overtime ke payroll overtime input.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/reconcile")]
        [AccessAction("Reconcile", "Reconcile Overtime Payroll Handoff", Description = "Memeriksa dan memperbaiki konsistensi source Overtime dengan payroll input", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimePayrollHandoff", "Reconcile")]
        public async Task<IActionResult> Reconcile(
            Guid realizationId,
            [FromBody] ReconcileOvertimePayrollHandoffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.ReconcileAsync(realizationId, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimePayrollHandoff.Reconcile",
                    "Menjalankan reconciliation overtime payroll handoff.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/rollback")]
        [AccessAction("Rollback", "Rollback Overtime Payroll Handoff", Description = "Membatalkan payroll input sebelum payroll run dikunci atau difinalisasi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OvertimePayrollHandoff", "Rollback")]
        public async Task<IActionResult> Rollback(
            Guid realizationId,
            [FromBody] RollbackOvertimePayrollHandoffRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.RollbackAsync(realizationId, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimePayrollHandoff.Rollback",
                    "Melakukan rollback overtime payroll handoff.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(OvertimePayrollHandoffServiceResult<T> result) =>
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
