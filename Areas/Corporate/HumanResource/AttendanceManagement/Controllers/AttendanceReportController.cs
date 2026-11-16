using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/reports")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Report",
        AreaName = "Corporate",
        ControllerName = "AttendanceReport",
        Description = "Laporan kehadiran M12 sesuai cakupan unit pengguna",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Report")]
    public class AttendanceReportController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly AttendanceReportService _service;
        private readonly LoggerService _loggerService;

        public AttendanceReportController(AttendanceReportService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("{reportCode}")]
        [AccessAction("Read", "Read Attendance Report", Description = "Melihat laporan kehadiran unit dalam cakupan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceReport", "Read")]
        public async Task<IActionResult> GetReport(string reportCode, [FromQuery] AttendanceReportQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetReportAsync(reportCode, request, DateTime.UtcNow, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<AttendanceReportResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        // Berkas dibentuk dari fungsi yang SAMA dengan layar (paging diabaikan), sehingga isinya identik dengan tabel layar
        // untuk filter yang sama. Setiap ekspor yang berhasil menulis tepat satu log audit (HRD-DEC-206 butir 4); isi log
        // ditulis di teks pesan karena LoggerService tidak menyimpan payload, dan pelakunya diambil dari sesi.
        [HttpGet("{reportCode}/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, AttendanceReportService.XlsxContentType)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Export", "Export Attendance Report", Description = "Mengunduh laporan kehadiran (.xlsx) sesuai filter layar", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("AttendanceReport", "Export")]
        public async Task<IActionResult> Export(string reportCode, [FromQuery] AttendanceReportQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.ExportAsync(reportCode, request, DateTime.UtcNow, cancellationToken);
            if (!result.Success) return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

            var report = result.Report!;
            await _loggerService.AuditAsync(
                LogCategory,
                "AttendanceReport.Export",
                $"Ekspor {report.ReportName} ({report.ReportCode}); unit: {result.UnitLabel}; rentang {report.FromDate:yyyy-MM-dd} s.d. {report.ToDate:yyyy-MM-dd}; " +
                $"status: {(string.IsNullOrWhiteSpace(request.Status) ? "-" : request.Status.Trim())}; periode: {(report.PeriodState == "Closed" ? "ditutup" : "masih terbuka")}; " +
                $"{result.RowCount} baris; berkas {result.FileName}.");

            return File(result.Content, AttendanceReportService.XlsxContentType, result.FileName);
        }
    }
}
