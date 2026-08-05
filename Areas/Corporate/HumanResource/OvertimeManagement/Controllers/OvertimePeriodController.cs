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
    [Route("api/v1/corporate/human-resource/overtime-management/periods")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Period Closing",
        AreaName = "Corporate",
        ControllerName = "OvertimePeriod",
        Description = "Overtime period monitoring, validation, closing, and reopen control",
        SortOrder = 7)]
    [Tags("Corporate / Human Resource / Overtime Management / Period Closing")]
    public class OvertimePeriodController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimePeriodQueryService _queryService;
        private readonly OvertimePeriodService _service;
        private readonly LoggerService _loggerService;

        public OvertimePeriodController(
            OvertimePeriodQueryService queryService,
            OvertimePeriodService service,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Period", Description = "Melihat metadata filter overtime period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriod", "Read")]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<OvertimePeriodFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata overtime period berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Period", Description = "Melihat ringkasan overtime period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriod", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimePeriodSummaryResponse>.Ok(data, "Ringkasan overtime period berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Period", Description = "Melihat daftar overtime period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriod", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimePeriodListResponse>>.Ok(data, "Data overtime period berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Period", Description = "Melihat pilihan overtime period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriod", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? periodStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(search, periodStatus, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimePeriodOptionResponse>>.Ok(data, "Pilihan overtime period berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Period", Description = "Melihat detail overtime period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimePeriod", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan."))
                : Ok(ApiResponse<OvertimePeriodDetailResponse>.Ok(data, "Detail overtime period berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Overtime Period", Description = "Membuat overtime period", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OvertimePeriod", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOvertimePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CreateAsync(request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Create", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Overtime Period", Description = "Memperbarui overtime period", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimePeriod", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateOvertimePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.UpdateAsync(id, request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Update", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Overtime Period", Description = "Menghapus overtime period yang belum digunakan", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("OvertimePeriod", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.DeleteAsync(id, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Delete", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/validate")]
        [AccessAction("Validate", "Validate Overtime Period", Description = "Menjalankan final reconciliation dan readiness closing", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("OvertimePeriod", "Validate")]
        public async Task<IActionResult> Validate(
            Guid id,
            [FromBody] ValidateOvertimePeriodRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.ValidateAsync(id, request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Validate", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/close")]
        [AccessAction("Close", "Close Overtime Period", Description = "Menutup overtime period setelah final reconciliation", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OvertimePeriod", "Close")]
        public async Task<IActionResult> Close(
            Guid id,
            [FromBody] CloseOvertimePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CloseAsync(id, request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Close", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reopen")]
        [AccessAction("Reopen", "Reopen Overtime Period", Description = "Membuka kembali overtime period Closed", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("OvertimePeriod", "Reopen")]
        public async Task<IActionResult> Reopen(
            Guid id,
            [FromBody] ReopenOvertimePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.ReopenAsync(id, request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Reopen", result.Data, result.Success);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Overtime Period", Description = "Membatalkan overtime period Open atau Reopened", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("OvertimePeriod", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelOvertimePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();
            var result = await _service.CancelAsync(id, request, actor, cancellationToken);
            await LogMutationAsync("OvertimePeriod.Cancel", result.Data, result.Success);
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

        private async Task LogMutationAsync(string action, object? data, bool success)
        {
            if (!success || data == null) return;
            await _loggerService.InfoAsync(LogCategory, action, action, data);
        }
    }
}
