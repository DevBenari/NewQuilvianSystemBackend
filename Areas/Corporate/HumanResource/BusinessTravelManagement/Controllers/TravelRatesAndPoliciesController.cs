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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel")]
    [Tags("Corporate - HRD Travel Rates & Policies")]
    public class TravelRatesAndPoliciesController : ControllerBase
    {
        private readonly IMstTravelAllowanceRateService _rateService;
        private readonly ITravelSettlementService _settlementService;

        public TravelRatesAndPoliciesController(
            IMstTravelAllowanceRateService rateService,
            ITravelSettlementService settlementService)
        {
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
        [ProducesResponseType(typeof(ApiResponse<List<TravelOverdueReportDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverdueReports([FromQuery] Guid? departmentId, [FromQuery] DateOnly? cutoffDate)
        {
            var reports = await _settlementService.GetOverdueReportAsync(departmentId, cutoffDate);
            return Ok(ApiResponse<List<TravelOverdueReportDto>>.Ok(reports, $"Total {reports.Count} pegawai dengan tunggakan settlement terdeteksi"));
        }
    }
}
