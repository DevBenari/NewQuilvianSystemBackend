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
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Pemeriksaan Kesehatan Berkala Staf (MCU) & Surveilans Risiko Radiasi (Kontrak API v23 - Tag 29.2)
    /// </summary>
    [ApiController]
    [Route("api/v1/hrd/occupational-health")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OCCUPATIONAL_HEALTH",
        moduleName: "Human Resource Occupational Health",
        displayName: "MCU & Surveilans Bahaya",
        AreaName = "Corporate",
        ControllerName = "OccupationalMedicalExamination",
        Description = "Penjadwalan dan hasil MCU staf serta surveilans dosis radiasi",
        SortOrder = 11)]
    [Tags("Corporate - HRD Medical Check-Up & Risk Surveillance")]
    public class MedicalExaminationsController : ControllerBase
    {
        private readonly IMedicalExaminationService _mcuService;
        private readonly IHazardSurveillanceService _hazardService;

        public MedicalExaminationsController(
            IMedicalExaminationService mcuService,
            IHazardSurveillanceService hazardService)
        {
            _mcuService = mcuService ?? throw new ArgumentNullException(nameof(mcuService));
            _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(raw, out var uid) && uid != Guid.Empty)
            {
                return uid;
            }

            // Tidak memakai aktor cadangan: jejak audit tanpa identitas asli lebih berbahaya
            // daripada permintaan yang ditolak.
            throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
        }

        /// <summary>
        /// Daftarkan jadwal pemeriksaan MCU tahunan/khusus berdasar risiko ruangan
        /// </summary>
        [HttpPost("mcu/schedule")]
        [AccessAction("ScheduleMcu", "Menjadwalkan MCU Staf", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("OccupationalMedicalExamination", "ScheduleMcu")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<MedicalExaminationResponseDto>), 201)]
        public async Task<IActionResult> ScheduleMcu([FromBody] ScheduleMcuRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _mcuService.ScheduleMcuAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<MedicalExaminationResponseDto>.Ok(result, "Jadwal MCU pegawai berhasil didaftarkan."));
        }

        /// <summary>
        /// Daftar pelaksanaan MCU pegawai dengan filter risiko dan unit
        /// </summary>
        [HttpGet("mcu")]
        [AccessAction("Read", "Melihat MCU & Radar Surveilans", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OccupationalMedicalExamination", "Read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicalExaminationSummaryDto>>), 200)]
        public async Task<IActionResult> GetMcuList(
            [FromQuery] string? examType = null,
            [FromQuery] string? riskProfile = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _mcuService.GetMcuListAsync(examType, riskProfile, status, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<MedicalExaminationSummaryDto>>.Ok(result, "Daftar pelaksanaan MCU berhasil diambil."));
        }

        /// <summary>
        /// Catat hasil pemeriksaan fisik dan laboratorium terenkripsi oleh dokter
        /// </summary>
        [HttpPost("mcu/{examinationId:guid}/results")]
        [AccessAction("UploadMcuResults", "Mengunggah Hasil MCU", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OccupationalMedicalExamination", "UploadMcuResults")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<MedicalExaminationResponseDto>), 200)]
        public async Task<IActionResult> RecordMcuResults(Guid examinationId, [FromBody] RecordMcuResultsRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _mcuService.RecordMcuResultsAsync(examinationId, request, userId, cancellationToken);
            return Ok(ApiResponse<MedicalExaminationResponseDto>.Ok(result, "Hasil MCU terenkripsi berhasil dicatat dan disahkan."));
        }

        /// <summary>
        /// Catat impor data dosis radiasi TLD bulanan staf radiologi (INT-HR-91)
        /// </summary>
        [HttpPost("surveillance/radiation-dose")]
        [AccessAction("RecordHazardSurveillance", "Mencatat Surveilans Dosis Radiasi", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("OccupationalMedicalExamination", "RecordHazardSurveillance")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<HealthSurveillanceResponseDto>), 201)]
        public async Task<IActionResult> RecordRadiationDose([FromBody] RecordRadiationDoseRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _hazardService.RecordRadiationDoseAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<HealthSurveillanceResponseDto>.Ok(result, "Data dosis radiasi TLD berhasil dicatat."));
        }

        /// <summary>
        /// Radar pemantauan akumulasi dosis radiasi staf radiologi terhadap batas 20 mSv
        /// </summary>
        [HttpGet("surveillance/radiation-radar")]
        [AccessAction("Read", "Melihat MCU & Radar Surveilans", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OccupationalMedicalExamination", "Read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<RadiationSurveillanceRadarDto>>), 200)]
        public async Task<IActionResult> GetRadiationRadar(
            [FromQuery] int? year = null,
            [FromQuery] bool? isNearThresholdOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _hazardService.GetRadiationRadarAsync(year, isNearThresholdOnly, cancellationToken);
            return Ok(ApiResponse<List<RadiationSurveillanceRadarDto>>.Ok(result, "Radar akumulasi dosis radiasi nakes berhasil diambil."));
        }
    }
}
