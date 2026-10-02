using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Surat dokter rawat jalan — RJ-DOC-REV-BE-004. Surat sakit, surat sehat, dan surat rujukan,
    /// beserta riwayatnya. Surat terbit tidak dihapus; ia dibatalkan lewat aksi <c>cancel</c>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/doctor-certificates")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Doctor Certificate",
        AreaName = "HealthServices",
        ControllerName = "DoctorCertificate",
        Description = "Surat sakit, surat sehat, dan surat rujukan dari dokter",
        SortOrder = 3
    )]
    [Tags("Health Services / Clinical Management / Doctor Certificate")]
    public class DoctorCertificateController : ControllerBase
    {
        private readonly DoctorCertificateService _service;

        public DoctorCertificateController(DoctorCertificateService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorCertificateResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Certificate", Description = "Melihat riwayat surat dokter pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorCertificate", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] string? certificateType,
            [FromQuery] bool includeCancelled = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            if (!patientId.HasValue && !encounterId.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pasien atau kunjungan wajib dipilih untuk melihat riwayat surat dokter."));
            }

            var result = await _service.GetListAsync(patientId, encounterId, certificateType, includeCancelled, pageNumber, pageSize, ct);
            return Ok(ApiResponse<PagedResult<DoctorCertificateResponse>>.Ok(result, "Riwayat surat dokter berhasil diambil."));
        }

        [HttpGet("referral-targets")]
        [ProducesResponseType(typeof(ApiResponse<List<DoctorCertificateReferralTargetOption>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Certificate", Description = "Melihat pilihan unit/tujuan rujukan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorCertificate", "Read")]
        public async Task<IActionResult> GetReferralTargets(
            [FromQuery] string? search,
            [FromQuery] int take = 30,
            CancellationToken ct = default)
        {
            var result = await _service.GetReferralTargetsAsync(search, take, ct);
            return Ok(ApiResponse<List<DoctorCertificateReferralTargetOption>>.Ok(result, "Pilihan tujuan rujukan berhasil diambil."));
        }

        [HttpGet("active-by-queue/{queueId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Doctor Certificate", Description = "Melihat surat dokter aktif pada antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorCertificate", "Read")]
        public async Task<IActionResult> GetActiveByQueue(Guid queueId, CancellationToken ct = default)
        {
            var result = await _service.GetActiveByQueueAsync(queueId, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Surat dokter untuk antrean ini belum ada."));
            }

            return Ok(ApiResponse<DoctorCertificateResponse>.Ok(result, "Surat dokter berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Doctor Certificate", Description = "Melihat detail surat dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorCertificate", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat dokter tidak ditemukan."));
            }

            return Ok(ApiResponse<DoctorCertificateResponse>.Ok(result, "Surat dokter berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DoctorCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Doctor Certificate", Description = "Menerbitkan surat dokter", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DoctorCertificate", "Create")]
        public async Task<IActionResult> Create([FromBody] SaveDoctorCertificateRequest request, CancellationToken ct = default)
        {
            var result = await _service.CreateAsync(request, GetCurrentUserId(), ct);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DoctorCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Doctor Certificate", Description = "Mengubah isi surat dokter yang belum dibatalkan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorCertificate", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveDoctorCertificateRequest request, CancellationToken ct = default)
        {
            var result = await _service.UpdateAsync(id, request, GetCurrentUserId(), ct);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<DoctorCertificateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Cancel", "Cancel Doctor Certificate", Description = "Membatalkan surat dokter beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("DoctorCertificate", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelDoctorCertificateRequest request, CancellationToken ct = default)
        {
            var result = await _service.CancelAsync(id, request, GetCurrentUserId(), ct);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(DoctorCertificateService.Result result)
        {
            if (result.Success)
                return Ok(ApiResponse<DoctorCertificateResponse>.Ok(result.Data!, result.Message));

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
