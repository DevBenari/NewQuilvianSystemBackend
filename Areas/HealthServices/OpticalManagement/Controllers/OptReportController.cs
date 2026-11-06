using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Laporan Optik — <c>OPT-API-v1</c> grup Opt Report (<c>BE-OPT-052</c>, <c>OPT-DEC-025</c>): kunjungan per pemeriksa,
    /// penjualan, pesanan terbuka, dan garansi. Baca-saja; hanya pemegang <c>OptReport : Read</c> (<c>UAT-23</c>).
    /// </summary>
    /// <remarks>Action sengaja tanpa <c>&lt;summary&gt;</c> XML.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/reports")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Report",
        AreaName = P.AreaName,
        ControllerName = P.Report.Resource,
        Description = "Laporan Optik: kunjungan per pemeriksa, penjualan, pesanan terbuka, garansi",
        SortOrder = 8
    )]
    [Tags("Health Services / Optical Management / Opt Report")]
    public class OptReportController : ControllerBase
    {
        private readonly OptReportService _service;

        public OptReportController(OptReportService service)
        {
            _service = service;
        }

        [HttpGet("visits-by-examiner")]
        [ProducesResponseType(typeof(ApiResponse<OptVisitReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, "Read Opt Report", Description = "Melihat laporan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Report.Resource, P.Read)]
        public async Task<IActionResult> GetVisitsByExaminer([FromQuery] OptReportPeriodQuery query, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetVisitsByExaminerAsync(query, cancellationToken), "Laporan kunjungan per pemeriksa berhasil diambil.");

        [HttpGet("sales")]
        [ProducesResponseType(typeof(ApiResponse<OptSalesReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, "Read Opt Report", Description = "Melihat laporan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Report.Resource, P.Read)]
        public async Task<IActionResult> GetSales([FromQuery] OptReportPeriodQuery query, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetSalesAsync(query, cancellationToken), "Laporan penjualan berhasil diambil.");

        [HttpGet("open-orders")]
        [ProducesResponseType(typeof(ApiResponse<OptOpenOrderReportResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Report", Description = "Melihat laporan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Report.Resource, P.Read)]
        public async Task<IActionResult> GetOpenOrders([FromQuery] OptOpenOrderReportQuery query, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetOpenOrdersAsync(query, cancellationToken), "Laporan pesanan terbuka berhasil diambil.");

        [HttpGet("warranty")]
        [ProducesResponseType(typeof(ApiResponse<OptWarrantyReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, "Read Opt Report", Description = "Melihat laporan Optik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Report.Resource, P.Read)]
        public async Task<IActionResult> GetWarranty([FromQuery] OptReportPeriodQuery query, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetWarrantyAsync(query, cancellationToken), "Laporan garansi berhasil diambil.");
    }
}
