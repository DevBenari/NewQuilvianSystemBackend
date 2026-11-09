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
    /// REST API Controller untuk Radar Imunisasi Staf, Log Batch Vaksin & Titer Protektif (Kontrak API v23 - Tag 29.4)
    /// </summary>
    [ApiController]
    [Route("api/v1/hrd/occupational-health")]
    [Tags("Corporate - HRD Staff Vaccination & Immunity Tracking")]
    public class StaffVaccinationsController : ControllerBase
    {
        private readonly IStaffVaccinationService _vaccinationService;

        public StaffVaccinationsController(IStaffVaccinationService vaccinationService)
        {
            _vaccinationService = vaccinationService ?? throw new ArgumentNullException(nameof(vaccinationService));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Catat riwayat penyuntikan vaksin wajib nakes (batch, dosis, tanggal)
        /// </summary>
        [HttpPost("vaccinations")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<VaccinationRecordDto>), 201)]
        public async Task<IActionResult> RecordVaccination([FromBody] RecordVaccinationRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _vaccinationService.RecordVaccinationAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<VaccinationRecordDto>.Ok(result, "Log vaksinasi nakes berhasil dicatat."));
        }

        /// <summary>
        /// Riwayat vaksinasi lengkap seorang pegawai
        /// </summary>
        [HttpGet("vaccinations/employee/{employeeId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<VaccinationRecordDto>>), 200)]
        public async Task<IActionResult> GetEmployeeVaccinations(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var result = await _vaccinationService.GetVaccinationsByEmployeeAsync(employeeId, cancellationToken);
            return Ok(ApiResponse<List<VaccinationRecordDto>>.Ok(result, "Riwayat vaksinasi pegawai berhasil diambil."));
        }

        /// <summary>
        /// Rekam hasil uji laboratorium titer antibodi protektif (anti-HBs)
        /// </summary>
        [HttpPost("vaccinations/titer-assessment")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<HealthSurveillanceResponseDto>), 201)]
        public async Task<IActionResult> RecordAntibodyTiter([FromBody] RecordAntibodyTiterRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _vaccinationService.RecordAntibodyTiterAsync(request, userId, cancellationToken);
            return StatusCode(201, ApiResponse<HealthSurveillanceResponseDto>.Ok(result, "Hasil uji titer antibodi protektif berhasil dicatat."));
        }

        /// <summary>
        /// Daftar nakes yang memerlukan vaksinasi booster atau ber-titer < 10 mIU/mL
        /// </summary>
        [HttpGet("vaccinations/booster-recall-list")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NakesVaccinationRecallDto>>), 200)]
        public async Task<IActionResult> GetBoosterRecallList(
            [FromQuery] string? vaccineType = null,
            [FromQuery] Guid? departmentId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _vaccinationService.GetBoosterRecallListAsync(vaccineType, departmentId, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<NakesVaccinationRecallDto>>.Ok(result, "Daftar pemanggilan booster vaksinasi nakes berhasil diambil."));
        }
    }
}
