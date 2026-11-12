using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    /// <summary>
    /// Daftar kerja Registrasi untuk pemantauan sinkronisasi kartu penjamin pasien (Benefit Karyawan RS)
    /// (BE-FIN-139, FIN-API-1.12 V.3, FIN-DES-120, FIN-DEC-255).
    /// Bersifat hanya-baca dan TIDAK membuka identitas rekam medis pasien kepada pemanggil (AC 4).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/employee-benefit-card-syncs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Employee Benefit Card Sync",
        AreaName = "HealthServices",
        ControllerName = "EmployeeBenefitCardSync",
        Description = "Daftar kerja sinkronisasi kartu penjamin pasien Benefit Karyawan RS",
        SortOrder = 20)]
    [Tags("Health Services / Patient Management / Master Data / Employee Benefit Card Sync")]
    public sealed class EmployeeBenefitCardSyncController : ControllerBase
    {
        private readonly EmployeeBenefitCardSyncService _syncService;

        public EmployeeBenefitCardSyncController(EmployeeBenefitCardSyncService syncService)
        {
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        }

        /// <summary>
        /// Daftar kerja sinkronisasi kartu penjamin internal pasien (BE-FIN-139, FIN-API-1.12 V.3, FIN-DES-120).
        /// Bawaan status bila kosong: WAITING_PATIENT,ON_HOLD (AC 1).
        /// Menolak nilai status tidak dikenal dengan HTTP 400 (AC 2, FIN-VAL-299).
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Read Employee Benefit Card Sync", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitCardSync", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeBenefitCardSyncListResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPaged([FromQuery] EmployeeBenefitCardSyncPagedQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _syncService.GetWorklistPagedAsync(query, cancellationToken);
                return Ok(ApiResponse<PagedResult<EmployeeBenefitCardSyncListResponse>>.Ok(
                    result, "Daftar kerja sinkronisasi kartu berhasil diambil."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Ringkasan jumlah baris sinkronisasi kartu per status (BE-FIN-139, FIN-API-1.12 V.3).
        /// </summary>
        [HttpGet("summary")]
        [AccessAction("Read", "Read Employee Benefit Card Sync", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitCardSync", "Read")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeBenefitCardSyncSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var summary = await _syncService.GetWorklistSummaryAsync(cancellationToken);
            return Ok(ApiResponse<EmployeeBenefitCardSyncSummaryResponse>.Ok(
                summary, "Ringkasan status sinkronisasi kartu berhasil diambil."));
        }
    }
}

