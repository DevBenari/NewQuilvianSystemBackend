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
    /// REST API Controller untuk Rekam Medis K3RS & Privasi Data Medis Pegawai (Kontrak API v23 - Tag 29.1)
    /// </summary>
    [ApiController]
    [Route("api/v1/hrd/occupational-health")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OCCUPATIONAL_HEALTH",
        moduleName: "Human Resource Occupational Health",
        displayName: "Rekam Kesehatan Kerja Staf",
        AreaName = "Corporate",
        ControllerName = "OccupationalHealthRecord",
        Description = "Rekam medis K3RS staf, kelaikan kerja, dan dasbor eksekutif kesehatan kerja",
        SortOrder = 10)]
    [Tags("Corporate - HRD Occupational Health & Medical Privacy")]
    public class HealthRecordsController : ControllerBase
    {
        private readonly IHealthRecordService _healthRecordService;

        public HealthRecordsController(IHealthRecordService healthRecordService)
        {
            _healthRecordService = healthRecordService ?? throw new ArgumentNullException(nameof(healthRecordService));
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

        private bool IsDoctorRole()
        {
            return User.IsInRole("DokterK3RS") || User.IsInRole("OccupationalHealthDoctor") || User.IsInRole("Admin");
        }

        /// <summary>
        /// Ambil data rekam medis klinis lengkap staf terenkripsi (khusus Dokter K3RS)
        /// </summary>
        [HttpGet("records/{employeeId:guid}/clinical")]
        [AccessAction("ViewClinicalDetail", "Membaca Rekam Medis Klinis Staf", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthRecord", "ViewClinicalDetail")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<HealthRecordClinicalDetailDto>), 200)]
        public async Task<IActionResult> GetClinicalDetail(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var isDoctor = IsDoctorRole();
            var result = await _healthRecordService.GetClinicalDetailAsync(employeeId, isDoctor, cancellationToken);
            return Ok(ApiResponse<HealthRecordClinicalDetailDto>.Ok(result, "Data rekam medis klinis staf berhasil diambil."));
        }

        /// <summary>
        /// Ambil ringkasan status kelayakan kerja operasional bebas diagnosis (untuk HR & Atasan)
        /// </summary>
        [HttpGet("records/{employeeId:guid}/fitness-summary")]
        [AccessAction("ViewSummary", "Melihat Ringkasan Kelaikan Kerja", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OccupationalHealthRecord", "ViewSummary")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<EmployeeFitnessSummaryDto>), 200)]
        public async Task<IActionResult> GetFitnessSummary(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var result = await _healthRecordService.GetFitnessSummaryAsync(employeeId, cancellationToken);
            return Ok(ApiResponse<EmployeeFitnessSummaryDto>.Ok(result, "Ringkasan status kelaikan kerja operasional berhasil diambil."));
        }

        /// <summary>
        /// Perbarui status kelaikan kerja staf dan durasi berlaku
        /// </summary>
        [HttpPut("records/{employeeId:guid}/fitness-status")]
        [AccessAction("AssessFitnessToWork", "Menetapkan Status Kelaikan Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OccupationalHealthRecord", "AssessFitnessToWork")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<EmployeeFitnessSummaryDto>), 200)]
        public async Task<IActionResult> UpdateFitnessStatus(Guid employeeId, [FromBody] UpdateFitnessStatusRequestDto dto, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var result = await _healthRecordService.UpdateFitnessStatusAsync(employeeId, dto, userId, cancellationToken);
            return Ok(ApiResponse<EmployeeFitnessSummaryDto>.Ok(result, "Status kelaikan kerja staf berhasil diperbarui."));
        }

        /// <summary>
        /// Portal ESS: Ambil ringkasan rekam kesehatan dan status imunisasi milik user login
        /// </summary>
        [HttpGet("my-health")]
        [AccessAction("ViewOwn", "Melihat Profil Kesehatan Sendiri", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("OccupationalHealthRecord", "ViewOwn")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<MyHealthProfileDto>), 200)]
        public async Task<IActionResult> GetMyHealth([FromQuery] Guid? employeeId = null, CancellationToken cancellationToken = default)
        {
            var empId = employeeId ?? GetCurrentUserId();
            var result = await _healthRecordService.GetMyHealthProfileAsync(empId, cancellationToken);
            return Ok(ApiResponse<MyHealthProfileDto>.Ok(result, "Profil kesehatan mandiri berhasil diambil."));
        }

        /// <summary>
        /// Ambil statistik kepatuhan MCU institusi, kasus K3RS aktif, dan jam hilang LTI
        /// </summary>
        [HttpGet("dashboard/executive")]
        [AccessAction("ViewSummary", "Melihat Ringkasan Kelaikan Kerja", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OccupationalHealthRecord", "ViewSummary")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OhsExecutiveDashboardDto>), 200)]
        public async Task<IActionResult> GetExecutiveDashboard([FromQuery] int? year = null, [FromQuery] Guid? departmentId = null, CancellationToken cancellationToken = default)
        {
            var result = await _healthRecordService.GetExecutiveDashboardAsync(year, departmentId, cancellationToken);
            return Ok(ApiResponse<OhsExecutiveDashboardDto>.Ok(result, "Dasbor eksekutif K3RS berhasil diambil."));
        }
    }
}
