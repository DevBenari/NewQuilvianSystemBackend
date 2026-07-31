using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/periods")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Period",
        AreaName = "Corporate",
        ControllerName = "AttendancePeriod",
        Description = "Corporate human resource attendance period closing",
        SortOrder = 8)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Period")]
    public class AttendancePeriodController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly AttendancePeriodService _service;
        private readonly AttendanceSchedulerService _schedulerService;
        private readonly LoggerService _loggerService;

        public AttendancePeriodController(
            AttendancePeriodService service,
            AttendanceSchedulerService schedulerService,
            LoggerService loggerService)
        {
            _service = service;
            _schedulerService = schedulerService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Attendance Period", Description = "Melihat metadata attendance period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<AttendancePeriodMetadataResponse>.Ok(_service.GetMetadata(), "Metadata attendance period berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Attendance Period", Description = "Melihat ringkasan attendance period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendancePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendancePeriodSummaryResponse>.Ok(data, "Ringkasan attendance period berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Attendance Period", Description = "Melihat daftar attendance period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] AttendancePeriodQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendancePeriodPagedResponse>.Ok(data, "Data attendance period berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Attendance Period", Description = "Melihat pilihan attendance period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] bool onlyOpen = false,
            [FromQuery] int take = 100,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetOptionsAsync(search, onlyOpen, take, cancellationToken);
            return Ok(ApiResponse<List<AttendancePeriodOptionResponse>>.Ok(data, "Pilihan attendance period berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Attendance Period", Description = "Melihat detail attendance period", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await _service.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Attendance period tidak ditemukan."))
                : Ok(ApiResponse<AttendancePeriodDetailResponse>.Ok(data, "Detail attendance period berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Attendance Period", Description = "Membuat attendance period", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendancePeriod", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAttendancePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.CreateAsync(request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(LogCategory, "AttendancePeriod.Create", "Membuat attendance period.", result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Attendance Period", Description = "Mengubah attendance period", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendancePeriod", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateAttendancePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.UpdateAsync(id, request, actor, cancellationToken));
        }

        [HttpGet("{id:guid}/close-preview")]
        [AccessAction("Read", "Preview Attendance Period Close", Description = "Melihat validasi sebelum attendance period ditutup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendancePeriod", "Read")]
        public async Task<IActionResult> PreviewClose(Guid id, CancellationToken cancellationToken = default) =>
            ToActionResult(await _service.PreviewCloseAsync(id, cancellationToken));

        [HttpPost("{id:guid}/enqueue-processing")]
        [AccessAction("Process", "Enqueue Attendance Period Processing", Description = "Menjadwalkan processing untuk attendance period", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AttendancePeriod", "Process")]
        public async Task<IActionResult> EnqueueProcessing(
            Guid id,
            [FromBody] EnqueueAttendancePeriodProcessingRequest? request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _schedulerService.EnqueuePeriodAsync(id, request ?? new EnqueueAttendancePeriodProcessingRequest(), actor, cancellationToken));
        }

        [HttpPost("{id:guid}/close")]
        [AccessAction("Close", "Close Attendance Period", Description = "Menutup dan mengunci attendance period", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AttendancePeriod", "Close")]
        public async Task<IActionResult> Close(
            Guid id,
            [FromBody] CloseAttendancePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.CloseAsync(id, request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(LogCategory, "AttendancePeriod.Close", "Menutup attendance period.", result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reopen")]
        [AccessAction("Reopen", "Reopen Attendance Period", Description = "Membuka kembali attendance period", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("AttendancePeriod", "Reopen")]
        public async Task<IActionResult> Reopen(
            Guid id,
            [FromBody] ReopenAttendancePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.ReopenAsync(id, request, actor, cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Attendance Period", Description = "Membatalkan attendance period", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("AttendancePeriod", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelAttendancePeriodRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.CancelAsync(id, request, actor, cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Attendance Period", Description = "Menghapus attendance period", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("AttendancePeriod", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.DeleteAsync(id, actor, cancellationToken));
        }

        private IActionResult ToActionResult<T>(AttendancePeriodSchedulerServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
