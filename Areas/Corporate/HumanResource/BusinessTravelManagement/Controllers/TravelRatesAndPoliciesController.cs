using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BUSINESS_TRAVEL",
        moduleName: "Human Resource Business Travel",
        displayName: "Tarif & Kebijakan Perjalanan Dinas",
        AreaName = "Corporate",
        ControllerName = "TravelRatePolicy",
        Description = "Matriks tarif uang harian, zonasi, dan radar keterlambatan settlement",
        SortOrder = 14)]
    [Tags("Corporate - HRD Travel Rates & Policies")]
    public class TravelRatesAndPoliciesController : ControllerBase
    {
        private readonly IMstTravelAllowanceRateService _rateService;
        private readonly ITravelSettlementService _settlementService;
        private readonly ITravelAuditTrailService _auditTrailService;

        public TravelRatesAndPoliciesController(
            IMstTravelAllowanceRateService rateService,
            ITravelSettlementService settlementService,
            ITravelAuditTrailService auditTrailService)
        {
            _auditTrailService = auditTrailService;
            _rateService = rateService;
            _settlementService = settlementService;
        }

        private Guid GetCurrentUserId()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userClaim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil matriks tarif baku lumpsum uang harian per zonasi &amp; grade (Kontrak v25)
        /// </summary>
        [HttpGet("rates")]
        [AccessAction("ViewRate", "Melihat Tarif & Zonasi Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TravelRatePolicy", "ViewRate")]
        [ProducesResponseType(typeof(ApiResponse<List<TravelAllowanceRateResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRates([FromQuery] Guid? zoneId, [FromQuery] Guid? gradeId)
        {
            var rates = await _rateService.GetRatesAsync(zoneId, gradeId);
            return Ok(ApiResponse<List<TravelAllowanceRateResponseDto>>.Ok(rates));
        }

        /// <summary>
        /// Mengonfigurasi tarif uang harian dinas (per diem) rumah sakit (Kontrak v25)
        /// </summary>
        [HttpPost("rates")]
        [AccessAction("ManageRate", "Mengonfigurasi Tarif Dinas", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("TravelRatePolicy", "ManageRate")]
        [ProducesResponseType(typeof(ApiResponse<TravelAllowanceRateResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureRate([FromBody] ConfigureTravelRateDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _rateService.ConfigureRateAsync(dto, userId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<TravelAllowanceRateResponseDto>.Ok(result, "Tarif per diem dinas berhasil dikonfigurasi"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TravelAllowanceRateResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil daftar zonasi tujuan perjalanan dinas (Zona 1, Zona 2, Zona 3) (Kontrak v25)
        /// </summary>
        [HttpGet("zones")]
        [AccessAction("ViewRate", "Melihat Tarif & Zonasi Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TravelRatePolicy", "ViewRate")]
        [ProducesResponseType(typeof(ApiResponse<List<TravelDestinationZoneResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetZones()
        {
            var zones = await _rateService.GetZonesAsync();
            return Ok(ApiResponse<List<TravelDestinationZoneResponseDto>>.Ok(zones));
        }

        /// <summary>
        /// Mengunduh radar laporan pegawai menunggak settlement &gt; 7, 14, dan 30 hari (Kontrak v25)
        /// </summary>
        [HttpGet("overdue-reports")]
        [AccessAction("MonitorOverdue", "Memantau Keterlambatan Settlement", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("TravelRatePolicy", "MonitorOverdue")]
        [ProducesResponseType(typeof(ApiResponse<List<TravelOverdueReportDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverdueReports([FromQuery] Guid? departmentId, [FromQuery] DateOnly? cutoffDate)
        {
            var reports = await _settlementService.GetOverdueReportAsync(departmentId, cutoffDate);
            return Ok(ApiResponse<List<TravelOverdueReportDto>>.Ok(reports, $"Total {reports.Count} pegawai dengan tunggakan settlement terdeteksi"));
        }

        /// <summary>
        /// Log audit transaksi perjalanan dinas untuk auditor internal.
        /// </summary>
        [HttpGet("audit-logs")]
        [AccessAction("ViewAudit", "Melihat Log Audit Perjalanan Dinas", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("TravelRatePolicy", "ViewAudit")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<TravelAuditEventDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] Guid? travelRequestId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _auditTrailService.GetAuditTrailAsync(travelRequestId, from, to, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TravelAuditEventDto>>.Ok(result, "Log audit perjalanan dinas berhasil diambil"));
        }
    }
}
