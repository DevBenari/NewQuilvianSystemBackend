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
    [Route("api/v1/corporate/human-resource/overtime-management/verifications")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Verification",
        AreaName = "Corporate",
        ControllerName = "OvertimeVerification",
        Description = "Admin review, domain verification, adjustment, and final approval of actual overtime",
        SortOrder = 4)]
    [Tags("Corporate / Human Resource / Overtime Management / Overtime Verification")]
    public class OvertimeVerificationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.OvertimeManagement";
        private readonly OvertimeVerificationQueryService _queryService;
        private readonly OvertimeVerificationService _verificationService;
        private readonly LoggerService _loggerService;

        public OvertimeVerificationController(
            OvertimeVerificationQueryService queryService,
            OvertimeVerificationService verificationService,
            LoggerService loggerService)
        {
            _queryService = queryService;
            _verificationService = verificationService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Overtime Verification", Description = "Melihat metadata filter verification queue", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeVerification", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<OvertimeVerificationFilterMetadataResponse>.Ok(
                _queryService.GetMetadata(),
                "Metadata overtime verification berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Overtime Verification", Description = "Melihat ringkasan verification queue", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeVerification", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] OvertimeVerificationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<OvertimeVerificationSummaryResponse>.Ok(
                data,
                "Ringkasan overtime verification berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Overtime Verification", Description = "Melihat verification queue", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeVerification", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OvertimeVerificationQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeVerificationListResponse>>.Ok(
                data,
                "Data overtime verification berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Overtime Verification", Description = "Melihat pilihan overtime realization untuk verification", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeVerification", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] string? verificationStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetOptionsAsync(
                search,
                verificationStatus,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<OvertimeVerificationOptionResponse>>.Ok(
                data,
                "Pilihan overtime verification berhasil diambil."));
        }

        [HttpGet("realizations/{realizationId:guid}")]
        [AccessAction("Read", "Read Overtime Verification", Description = "Melihat attendance evidence, calculation, dan verification history", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeVerification", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var data = await _queryService.GetDetailAsync(realizationId, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Overtime realization tidak ditemukan."))
                : Ok(ApiResponse<OvertimeVerificationDetailResponse>.Ok(
                    data,
                    "Detail overtime verification berhasil diambil."));
        }

        [HttpPost("realizations/{realizationId:guid}/start")]
        [AccessAction("Start", "Start Overtime Verification", Description = "Mengambil realization dari queue dan membuat pending verification secara idempotent", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OvertimeVerification", "Start")]
        public async Task<IActionResult> Start(
            Guid realizationId,
            [FromBody] StartOvertimeVerificationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _verificationService.StartAsync(
                realizationId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeVerification.Start",
                    "Memulai admin review overtime realization.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/approve")]
        [AccessAction("Approve", "Approve Overtime Verification", Description = "Memverifikasi menit aktual, menerapkan adjustment terkontrol, dan menyelesaikan realization", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimeVerification", "Approve")]
        public async Task<IActionResult> Approve(
            Guid realizationId,
            [FromBody] ApproveOvertimeVerificationRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _verificationService.ApproveAsync(
                realizationId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeVerification.Approve",
                    "Menyelesaikan final overtime verification.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/need-revision")]
        [AccessAction("NeedRevision", "Request Overtime Revision", Description = "Mengembalikan realization untuk attendance correction atau recalculation", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OvertimeVerification", "NeedRevision")]
        public async Task<IActionResult> NeedRevision(
            Guid realizationId,
            [FromBody] RequestOvertimeVerificationRevisionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _verificationService.RequestRevisionAsync(
                realizationId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeVerification.NeedRevision",
                    "Mengembalikan overtime realization untuk perbaikan.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPost("realizations/{realizationId:guid}/reject")]
        [AccessAction("Reject", "Reject Overtime Verification", Description = "Menolak actual overtime pada final verification", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OvertimeVerification", "Reject")]
        public async Task<IActionResult> Reject(
            Guid realizationId,
            [FromBody] RejectOvertimeVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return UnauthorizedResult();

            var result = await _verificationService.RejectAsync(
                realizationId,
                request,
                actor,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "OvertimeVerification.Reject",
                    "Menolak overtime realization pada final verification.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(
            OvertimeVerificationServiceResult<T> result) =>
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
