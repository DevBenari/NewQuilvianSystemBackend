using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Tanggap Darurat Tertusuk Jarum Cito & Pajanan Okupasional (Kontrak API v23 - Tag 29.3)
    /// </summary>
    [ApiController]
    [Route("api/v1/hrd/occupational-health")]
    [Tags("Corporate - HRD Needle-Stick & Occupational Exposure Emergency")]
    public class NeedleStickEmergencyController : ControllerBase
    {
        private readonly INeedleStickEmergencyService _needleStickService;
        private readonly IOccupationalExposureService _exposureService;

        public NeedleStickEmergencyController(
            INeedleStickEmergencyService needleStickService,
            IOccupationalExposureService exposureService)
        {
            _needleStickService = needleStickService ?? throw new ArgumentNullException(nameof(needleStickService));
            _exposureService = exposureService ?? throw new ArgumentNullException(nameof(exposureService));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Tombol Darurat ESS: Laporkan insiden tertusuk jarum suntik bekas cito
        /// </summary>
        [HttpPost("needle-stick/report-cito")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<NeedleStickReceiptDto>), 201)]
        public async Task<IActionResult> ReportNeedleStickCito([FromBody] ReportNeedleStickCitoRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _needleStickService.ReportCitoAsync(userId, request, cancellationToken);
            return StatusCode(201, ApiResponse<NeedleStickReceiptDto>.Ok(result, "Laporan cito tertusuk jarum berhasil diterima. Notifikasi darurat dikirim ke tim K3RS & PPI."));
        }

        /// <summary>
        /// Daftar antrean insiden tertusuk jarum dan status pemberian PEP
        /// </summary>
        [HttpGet("needle-stick")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NeedleStickIncidentDto>>), 200)]
        public async Task<IActionResult> GetNeedleStickQueue(
            [FromQuery] string? status = null,
            [FromQuery] bool? isPepAdministered = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _needleStickService.GetNeedleStickQueueAsync(status, isPepAdministered, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<NeedleStickIncidentDto>>.Ok(result, "Antrean insiden tertusuk jarum berhasil diambil."));
        }

        /// <summary>
        /// Rekam tindakan inisiasi Profilaksis Pasca Pajanan (PEP) < 4 jam
        /// </summary>
        [HttpPost("needle-stick/{incidentId:guid}/administer-pep")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<NeedleStickIncidentDto>), 200)]
        public async Task<IActionResult> AdministerPep(Guid incidentId, [FromBody] AdministerPepRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _needleStickService.AdministerPepAsync(incidentId, request, userId, cancellationToken);
            return Ok(ApiResponse<NeedleStickIncidentDto>.Ok(result, "Tindakan Profilaksis Pasca Pajanan (PEP) berhasil dicatat."));
        }

        /// <summary>
        /// Catat hasil pemantauan tes darah berkala Bulan ke-1, 3, dan 6
        /// </summary>
        [HttpPost("exposures/{exposureId:guid}/serial-serology")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OccupationalExposureDto>), 200)]
        public async Task<IActionResult> RecordSerialSerology(Guid exposureId, [FromBody] RecordSerologyFollowupRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _exposureService.RecordSerologyFollowupAsync(exposureId, request, userId, cancellationToken);
            return Ok(ApiResponse<OccupationalExposureDto>.Ok(result, "Hasil pemantauan serologi serial berhasil dicatat."));
        }

        /// <summary>
        /// Radar pemantauan nakes yang sedang dalam jadwal kontrol serologi 6 bulan
        /// </summary>
        [HttpGet("exposures/radar-active")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ActiveExposureRadarSummaryDto>>), 200)]
        public async Task<IActionResult> GetActiveExposureRadar(CancellationToken cancellationToken = default)
        {
            var result = await _exposureService.GetActiveExposureRadarAsync(cancellationToken);
            return Ok(ApiResponse<List<ActiveExposureRadarSummaryDto>>.Ok(result, "Radar nakes dalam surveilans serologi aktif berhasil diambil."));
        }
    }
}
