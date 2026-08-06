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
    [Route("api/v1/corporate/human-resource/overtime-management/realizations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Realization",
        AreaName = "Corporate",
        ControllerName = "OvertimeRealization",
        Description = "Attendance matching, actual overtime calculation, and realization monitoring",
        SortOrder = 3)]
    [Tags("Corporate / Human Resource / Overtime Management / Overtime Realization")]
    public class OvertimeRealizationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeActualCalculationService _calculationService;
        private readonly OvertimeRealizationQueryService _queryService;
        private readonly LoggerService _loggerService;

        public OvertimeRealizationController(
            OvertimeActualCalculationService calculationService,
            OvertimeRealizationQueryService queryService,
            LoggerService loggerService)
        {
            _calculationService = calculationService;
            _queryService = queryService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Realization", Description = "Melihat metadata filter overtime realization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRealization", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<OvertimeRealizationFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata overtime realization berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Realization", Description = "Melihat ringkasan overtime realization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRealization", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimeRealizationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimeRealizationSummaryResponse>.Ok(
                data,
                "Ringkasan overtime realization berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Realization", Description = "Melihat daftar overtime realization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRealization", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimeRealizationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeRealizationListResponse>>.Ok(
                data,
                "Data overtime realization berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Realization", Description = "Melihat pilihan overtime realization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRealization", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? realizationStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(
                search,
                realizationStatus,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<OvertimeRealizationOptionResponse>>.Ok(
                data,
                "Pilihan overtime realization berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Overtime Realization", Description = "Melihat detail overtime realization", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeRealization", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime realization tidak ditemukan."))
                : Ok(ApiResponse<OvertimeRealizationDetailResponse>.Ok(
                    data,
                    "Detail overtime realization berhasil diambil."));
        }

        [HttpPost("requests/{requestId:guid}/preview")]
        [AccessAction("Preview", "Preview Overtime Realization", Description = "Melakukan preview attendance matching dan actual overtime calculation", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimeRealization", "Preview")]
        public async Task<IActionResult> Preview(
            Guid requestId,
            [FromBody] PreviewOvertimeRealizationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _calculationService.PreviewAsync(
                requestId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("requests/{requestId:guid}/calculate")]
        [AccessAction("Calculate", "Calculate Overtime Realization", Description = "Membuat actual overtime realization secara idempotent", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OvertimeRealization", "Calculate")]
        public async Task<IActionResult> Calculate(
            Guid requestId,
            [FromBody] CalculateOvertimeRealizationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            request ??= new CalculateOvertimeRealizationRequest();
            request.ForceNewVersion = false;

            var result = await _calculationService.CalculateAsync(
                requestId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeRealization.Calculate",
                    "Membuat actual overtime realization dari attendance.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("requests/{requestId:guid}/recalculate")]
        [AccessAction("Recalculate", "Recalculate Overtime Realization", Description = "Membuat realization version baru setelah perubahan attendance", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeRealization", "Recalculate")]
        public async Task<IActionResult> Recalculate(
            Guid requestId,
            [FromBody] CalculateOvertimeRealizationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            request ??= new CalculateOvertimeRealizationRequest();
            request.ForceNewVersion = true;

            var result = await _calculationService.CalculateAsync(
                requestId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeRealization.Recalculate",
                    "Membuat actual overtime realization version baru.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit-verification")]
        [AccessAction("SubmitVerification", "Submit Overtime Verification", Description = "Mengirim realization Draft atau NeedRevision ke proses verifikasi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OvertimeRealization", "SubmitVerification")]
        public async Task<IActionResult> SubmitVerification(
            Guid id,
            [FromBody] SubmitOvertimeRealizationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _calculationService.SubmitForVerificationAsync(
                id,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeRealization.SubmitVerification",
                    "Mengirim overtime realization ke verifikasi.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Overtime Realization", Description = "Membatalkan realization yang belum verified atau posted", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OvertimeRealization", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelOvertimeRealizationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _calculationService.CancelAsync(
                id,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeRealization.Cancel",
                    "Membatalkan overtime realization.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(
            OvertimeRealizationServiceResult<T> result) =>
            result.Success
                ? StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private IActionResult UnauthorizedResult() =>
            Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }
    }
}
