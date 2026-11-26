using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/holidays")]
    [Tags("Corporate / Human Resource / Master Data / Holiday")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Holiday", AreaName = "Corporate", ControllerName = "Holiday", Description = "Master hari libur", SortOrder = 20)]
    public class HolidayController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private readonly HolidayService _service;
        private readonly LoggerService _loggerService;

        public HolidayController(HolidayService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Holiday", Description = "Melihat pilihan kalender kerja dan jenis hari libur", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Holiday", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<HolidayFilterMetadataResponse>.Ok(await _service.GetFilterMetadataAsync(cancellationToken), "Metadata hari libur berhasil diambil."));

        [HttpGet]
        [AccessAction("Read", "Read Holiday", Description = "Melihat daftar hari libur", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Holiday", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] HolidayQueryRequest request, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<HolidayResponse>>.Ok(await _service.GetPagedAsync(request, cancellationToken), "Daftar hari libur berhasil diambil."));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Holiday", Description = "Melihat detail hari libur", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Holiday", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Hari libur tidak ditemukan."))
                : Ok(ApiResponse<HolidayResponse>.Ok(result, "Detail hari libur berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Holiday", Description = "Menambah hari libur", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Holiday", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateHolidayRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.CreateAsync(request, actor, cancellationToken);
            if (result.Success) await AuditAsync("Holiday.Create", "Menambah", result.Data!);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Holiday", Description = "Mengubah hari libur", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Holiday", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHolidayRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.UpdateAsync(id, request, actor, cancellationToken);
            if (result.Success) await AuditAsync("Holiday.Update", "Mengubah", result.Data!);
            return ToActionResult(result);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Holiday", Description = "Mengaktifkan atau menonaktifkan hari libur", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Holiday", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateHolidayStatusRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.UpdateStatusAsync(id, request.IsActive, actor, cancellationToken);
            if (result.Success) await AuditAsync("Holiday.UpdateStatus", request.IsActive ? "Mengaktifkan" : "Menonaktifkan", result.Data!);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Holiday", Description = "Menghapus hari libur", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Holiday", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.DeleteAsync(id, actor, cancellationToken);
            if (!result.Success) return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            await AuditAsync("Holiday.Delete", "Menghapus", result.Data!);
            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }

        // LoggerService tidak menyimpan payload; seluruh isi audit ditulis di teks pesan. Pelaku diambil dari sesi (HRD-DEC-210 butir 1).
        private Task AuditAsync(string action, string verb, HolidayResponse holiday) =>
            _loggerService.AuditAsync(
                LogCategory,
                action,
                $"{verb} hari libur {holiday.HolidayCode} ({holiday.HolidayName}), id {holiday.Id}, kalender {holiday.WorkCalendarCode ?? holiday.WorkCalendarId.ToString()}, tanggal {holiday.StartDate:yyyy-MM-dd} s.d. {holiday.EndDate:yyyy-MM-dd}, jenis {holiday.HolidayType}, nasional {holiday.IsNationalHoliday}, aktif {holiday.IsActive}.");

        private IActionResult ToActionResult(HolidayServiceResult result) =>
            result.Success
                ? Ok(ApiResponse<HolidayResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
