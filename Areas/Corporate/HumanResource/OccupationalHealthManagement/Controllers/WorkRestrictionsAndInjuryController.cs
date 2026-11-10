using System;
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
    /// REST API Controller untuk Pembatasan Tugas Medis (Work Restriction), Return-to-Work & Klaim Kecelakaan Kerja BPJS JKK (Kontrak API v23 - Tag 29.5)
    /// </summary>
    [ApiController]
    [Route("api/v1/hrd/occupational-health")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OCCUPATIONAL_HEALTH",
        moduleName: "Human Resource Occupational Health",
        displayName: "Restriksi Kerja & Kecelakaan Kerja",
        AreaName = "Corporate",
        ControllerName = "WorkRestrictionAndInjury",
        Description = "Surat pembatasan tugas, return-to-work, laporan KAK/PAK, dan formulir BPJS JKK",
        SortOrder = 14)]
    [Tags("Corporate - HRD Work Restriction & Injury BPJS JKK")]
    public class WorkRestrictionsAndInjuryController : ControllerBase
    {
        private readonly IWorkRestrictionService _restrictionService;
        private readonly IReturnToWorkService _rtwService;
        private readonly IOccupationalInjuryService _injuryService;

        public WorkRestrictionsAndInjuryController(
            IWorkRestrictionService restrictionService,
            IReturnToWorkService rtwService,
            IOccupationalInjuryService injuryService)
        {
            _restrictionService = restrictionService ?? throw new ArgumentNullException(nameof(restrictionService));
            _rtwService = rtwService ?? throw new ArgumentNullException(nameof(rtwService));
            _injuryService = injuryService ?? throw new ArgumentNullException(nameof(injuryService));
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
        /// Terbitkan surat rekomendasi pembatasan tugas dinas medis (Work Restriction)
        /// </summary>
        [HttpPost("restrictions")]
        [AccessAction("IssueWorkRestriction", "Menerbitkan Restriksi Kerja", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("WorkRestrictionAndInjury", "IssueWorkRestriction")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<WorkRestrictionDto>), 201)]
        public async Task<IActionResult> IssueWorkRestriction([FromBody] IssueWorkRestrictionRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _restrictionService.IssueRestrictionAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<WorkRestrictionDto>.Ok(result, "Surat rekomendasi pembatasan tugas kerja berhasil diterbitkan. Hard-stop roster Sub-06 diaktifkan."));
        }

        /// <summary>
        /// Endpoint verifikasi hard-stop jadwal roster dinas Sub-06 (INT-HR-89)
        /// </summary>
        [HttpGet("restrictions/validate-schedule/{employeeId:guid}")]
        [AccessAction("Read", "Memeriksa Jadwal terhadap Restriksi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkRestrictionAndInjury", "Read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ScheduleValidationResultDto>), 200)]
        public async Task<IActionResult> ValidateSchedule(
            Guid employeeId,
            [FromQuery] DateTime shiftDate,
            [FromQuery] string shiftType,
            [FromQuery] Guid? unitId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _restrictionService.ValidateScheduleAsync(employeeId, shiftDate, shiftType, unitId, cancellationToken);
            return Ok(ApiResponse<ScheduleValidationResultDto>.Ok(result, "Hasil validasi jadwal terhadap restriksi K3RS berhasil dievaluasi."));
        }

        /// <summary>
        /// Asesmen pemeriksaan fisik kelaikan kembali bekerja penuh oleh dokter K3RS
        /// </summary>
        [HttpPost("restrictions/{restrictionId:guid}/return-to-work")]
        [AccessAction("AssessReturnToWork", "Menilai Kelaikan Kembali Bekerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkRestrictionAndInjury", "AssessReturnToWork")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ReturnToWorkAssessmentDto>), 200)]
        public async Task<IActionResult> RecordReturnToWork(Guid restrictionId, [FromBody] RecordReturnToWorkRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _rtwService.AssessReturnToWorkAsync(restrictionId, request, userId, cancellationToken);
            return Ok(ApiResponse<ReturnToWorkAssessmentDto>.Ok(result, "Asesmen kelaikan kembali bekerja (RTW) berhasil disahkan. Gembok restriksi jadwal dibuka."));
        }

        /// <summary>
        /// Registrasi laporan insiden kecelakaan kerja (KAK/PAK) dan investigasi CAPA
        /// </summary>
        [HttpPost("injuries")]
        [AccessAction("ReportOccupationalInjury", "Melaporkan Kecelakaan Kerja", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("WorkRestrictionAndInjury", "ReportOccupationalInjury")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<EmployeeInjuryResponseDto>), 201)]
        public async Task<IActionResult> ReportInjury([FromBody] ReportEmployeeInjuryRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _injuryService.ReportInjuryAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<EmployeeInjuryResponseDto>.Ok(result, "Laporan kecelakaan kerja (KAK/PAK) berhasil dicatat. Tiket klaim BPJS JKK Tahap I terdaftar."));
        }

        /// <summary>
        /// Generator berkas resmi pelaporan klaim JKK BPJS Ketenagakerjaan Tahap I/II
        /// </summary>
        [HttpGet("injuries/{injuryId:guid}/bpjs-jkk-form")]
        [AccessAction("PrepareBpjsClaimStage1", "Menyiapkan Formulir BPJS JKK Tahap I", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("WorkRestrictionAndInjury", "PrepareBpjsClaimStage1")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BpjsJkkClaimFormDto>), 200)]
        public async Task<IActionResult> GenerateBpjsForm(Guid injuryId, [FromQuery] int stage = 1, CancellationToken cancellationToken = default)
        {
            var result = await _injuryService.GenerateBpjsFormAsync(injuryId, stage, cancellationToken);
            return Ok(ApiResponse<BpjsJkkClaimFormDto>.Ok(result, $"Formulir klaim JKK BPJS Ketenagakerjaan Tahap {stage} berhasil dibuat."));
        }
    }
}
