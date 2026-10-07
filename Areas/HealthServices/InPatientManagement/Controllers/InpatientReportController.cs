using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Laporan Rawat Inap (<c>BE-RWI-184</c>, API 11.7, <c>P2</c>). Hari ini satu laporan: transfer
    /// ruangan per periode, beserta ekspor Excel.
    /// </summary>
    /// <remarks>
    /// Hak akses per laporan, bukan per peran (<c>RWI-DEC-214</c>, <c>RWI-DEC-215</c>):
    /// <c>InpatientReport : ReadRoomTransfer</c> untuk membaca dan <c>: ExportRoomTransfer</c> untuk
    /// mengekspor. Tanpa salah satunya → <c>403</c>, apa pun nama perannya.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/reports")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Report",
        AreaName = "HealthServices",
        ControllerName = "InpatientReport",
        Description = "Laporan rawat inap: transfer ruangan per periode",
        SortOrder = 16
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Report")]
    public class InpatientReportController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.Report";

        private readonly InpRoomTransferReportService _roomTransferReportService;
        private readonly LoggerService _loggerService;

        public InpatientReportController(InpRoomTransferReportService roomTransferReportService, LoggerService loggerService)
        {
            _roomTransferReportService = roomTransferReportService;
            _loggerService = loggerService;
        }

        /// <summary>Laporan transfer ruangan per periode (paling lama 31 hari).</summary>
        /// <remarks>
        /// Memuat waktu, No. RM, pasien, asal, tujuan, alasan, pencatat, dan jenis (Transfer/Koreksi).
        /// Periode kosong atau lebih dari 31 hari → <c>400</c> "Pilih periode paling lama 31 hari".
        /// </remarks>
        [HttpGet("room-transfers")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RoomTransferReportRow>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("ReadRoomTransfer", "Read Room Transfer Report", Description = "Melihat laporan transfer ruangan rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientReport", "ReadRoomTransfer")]
        public async Task<IActionResult> GetRoomTransfers([FromQuery] RoomTransferReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var invalid = InpRoomTransferReportService.ValidatePeriod(query, out _, out _);
            if (invalid != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, invalid));

            var result = await _roomTransferReportService.GetAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<RoomTransferReportRow>>.Ok(result,
                "Laporan transfer ruangan berhasil diambil."));
        }

        /// <summary>Ekspor Excel laporan transfer ruangan dengan saringan yang sama, tanpa paging.</summary>
        /// <remarks>Setiap ekspor dicatat logger beserta pelaku, periode, dan jumlah baris.</remarks>
        [HttpGet("room-transfers/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("ExportRoomTransfer", "Export Room Transfer Report", Description = "Mengekspor laporan transfer ruangan rawat inap ke Excel", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientReport", "ExportRoomTransfer")]
        public async Task<IActionResult> ExportRoomTransfers([FromQuery] RoomTransferReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var invalid = InpRoomTransferReportService.ValidatePeriod(query, out _, out _);
            if (invalid != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, invalid));

            var (content, rowCount) = await _roomTransferReportService.ExportAsync(query, cancellationToken);

            // Ekspor data pasien wajib tercatat (API 11.7). Isi baris tidak ikut ke log.
            await _loggerService.AuditAsync(LogCategory, "InpatientReport.ExportRoomTransfer",
                "Mengekspor laporan transfer ruangan.",
                new
                {
                    ActorUserId = User.GetUserId(),
                    query.PeriodFrom,
                    query.PeriodTo,
                    query.FromServiceUnitId,
                    query.ToServiceUnitId,
                    query.ClassId,
                    query.IncludeCorrections,
                    RowCount = rowCount
                });

            var fileName = $"laporan-transfer-ruangan-{query.PeriodFrom:yyyyMMdd}-{query.PeriodTo:yyyyMMdd}.xlsx";
            return File(content, InpXlsxWriter.ContentType, fileName);
        }
    }
}
