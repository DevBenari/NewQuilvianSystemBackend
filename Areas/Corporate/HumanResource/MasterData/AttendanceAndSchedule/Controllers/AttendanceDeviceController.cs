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
    [Route("api/v1/corporate/human-resource/master-data/attendance-devices")]
    [Tags("Corporate / Human Resource / Master Data / Attendance Device")]
    [AccessController(moduleCode: "HUMAN_RESOURCE_MASTER_DATA", moduleName: "Human Resource Master Data", displayName: "Attendance Device", AreaName = "Corporate", ControllerName = "AttendanceDevice", Description = "Master mesin absen", SortOrder = 20)]
    public class AttendanceDeviceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private readonly AttendanceDeviceService _service;
        private readonly LoggerService _loggerService;

        public AttendanceDeviceController(AttendanceDeviceService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Attendance Device", Description = "Melihat metadata filter mesin absen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDevice", "Read")]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<AttendanceDeviceFilterMetadataResponse>.Ok(_service.GetFilterMetadata(), "Metadata mesin absen berhasil diambil."));

        [HttpGet]
        [AccessAction("Read", "Read Attendance Device", Description = "Melihat daftar mesin absen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDevice", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] AttendanceDeviceQueryRequest request, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<AttendanceDeviceResponse>>.Ok(await _service.GetPagedAsync(request, cancellationToken), "Daftar mesin absen berhasil diambil."));

        [HttpGet("unregistered")]
        [AccessAction("Read", "Read Attendance Device", Description = "Melihat mesin tak dikenal yang mengirim scan tertahan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDevice", "Read")]
        public async Task<IActionResult> GetUnregistered(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<UnregisteredDeviceResponse>>.Ok(await _service.GetUnregisteredAsync(cancellationToken), "Daftar mesin tak dikenal berhasil diambil."));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Attendance Device", Description = "Melihat detail mesin absen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDevice", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Mesin absen tidak ditemukan."))
                : Ok(ApiResponse<AttendanceDeviceResponse>.Ok(result, "Detail mesin absen berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Attendance Device", Description = "Mendaftarkan mesin absen", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceDevice", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceDeviceRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.CreateAsync(request, actor, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDevice.Create", "Mendaftarkan", result.Data!);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Attendance Device", Description = "Mengubah mesin absen", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceDevice", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttendanceDeviceRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.UpdateAsync(id, request, actor, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDevice.Update", "Mengubah", result.Data!);
            return ToActionResult(result);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Attendance Device", Description = "Mengaktifkan atau menonaktifkan mesin absen", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceDevice", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAttendanceDeviceStatusRequest request, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.UpdateStatusAsync(id, request.IsActive, actor, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDevice.UpdateStatus", request.IsActive ? "Mengaktifkan" : "Menonaktifkan", result.Data!);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Attendance Device", Description = "Menghapus mesin absen yang belum punya log", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("AttendanceDevice", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.DeleteAsync(id, actor, cancellationToken);
            if (!result.Success) return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            await AuditAsync("AttendanceDevice.Delete", "Menghapus", result.Data!);
            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }

        // LoggerService tidak menyimpan payload; seluruh isi audit ditulis di teks pesan. Pelaku diambil dari sesi.
        private Task AuditAsync(string action, string verb, AttendanceDeviceResponse device)
        {
            var release = device.HeldScanRelease == null
                ? string.Empty
                : $" Scan tertahan: {device.HeldScanRelease.HeldLogCount} ditemukan, {device.HeldScanRelease.MatchedCount} cocok, {device.HeldScanRelease.PendingCount} menunggu identitas, {device.HeldScanRelease.DuplicateCount} ganda, {device.HeldScanRelease.RejectedCount} ditolak, {device.HeldScanRelease.ScheduledReprocessJobCount} hitung ulang dijadwalkan.";
            var failure = device.HeldScanReleaseMessage == null ? string.Empty : $" Pelepasan: {device.HeldScanReleaseMessage}";
            return _loggerService.AuditAsync(
                LogCategory,
                action,
                $"{verb} mesin absen {device.AttendanceDeviceCode} ({device.AttendanceDeviceName}), id {device.Id}, ID eksternal {device.ExternalDeviceId ?? "-"}, nomor seri {device.SerialNumber ?? "-"}, aktif {device.IsActive}.{release}{failure}");
        }

        private IActionResult ToActionResult(AttendanceDeviceServiceResult<AttendanceDeviceResponse> result) =>
            result.Success
                ? Ok(ApiResponse<AttendanceDeviceResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
