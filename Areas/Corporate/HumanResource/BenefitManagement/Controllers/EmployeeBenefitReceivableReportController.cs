using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    /// <summary>
    /// Laporan HR "Piutang Benefit Pegawai" (hanya baca). Data finansialnya milik Finance (FinReceivable dan jadwal
    /// angsuran); HR tidak mengubah piutang, tidak menjadwalkan potongan, dan tidak memicu payroll dari sini.
    /// Izin dipisah dari modul lain (<c>EmployeeBenefitReceivableReport:Read</c>) supaya hanya petugas HR yang diberi
    /// akses yang dapat membaca identitas pemilik benefit dan pasien pada laporan ini.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/benefit-management/employee-benefit-receivables")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BENEFIT_MANAGEMENT",
        moduleName: "Human Resource Benefit Management",
        displayName: "Employee Benefit Receivable Report",
        AreaName = "Corporate",
        ControllerName = "EmployeeBenefitReceivableReport",
        Description = "Laporan baca-saja piutang benefit pegawai (AR Karyawan) dari Finance",
        SortOrder = 12)]
    [Tags("Corporate / Human Resource / Benefit Management / Employee Benefit Receivable Report")]
    public sealed class EmployeeBenefitReceivableReportController : ControllerBase
    {
        private readonly EmployeeBenefitReceivableReportService _service;

        public EmployeeBenefitReceivableReportController(EmployeeBenefitReceivableReportService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Employee Benefit Receivable Report", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitReceivableReport", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitReceivableReportSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] EmployeeBenefitReceivableReportQuery query,
            CancellationToken cancellationToken) =>
            Ok(ApiResponse<EmployeeBenefitReceivableReportSummaryResponse>.Ok(
                await _service.GetSummaryAsync(query, cancellationToken),
                "Ringkasan piutang benefit pegawai berhasil diambil."));

        [HttpGet]
        [AccessAction("Read", "Read Employee Benefit Receivable Report", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("EmployeeBenefitReceivableReport", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeBenefitReceivableReportRow>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] EmployeeBenefitReceivableReportQuery query,
            CancellationToken cancellationToken) =>
            Ok(ApiResponse<PagedResult<EmployeeBenefitReceivableReportRow>>.Ok(
                await _service.GetPagedAsync(query, cancellationToken),
                "Daftar piutang benefit pegawai berhasil diambil."));

        [HttpGet("{receivableId:guid}")]
        [AccessAction("Read", "Read Employee Benefit Receivable Report", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("EmployeeBenefitReceivableReport", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitReceivableReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(Guid receivableId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetDetailAsync(receivableId, cancellationToken);
                return Ok(ApiResponse<EmployeeBenefitReceivableReportDetailResponse>.Ok(
                    result, "Rincian piutang benefit pegawai berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
