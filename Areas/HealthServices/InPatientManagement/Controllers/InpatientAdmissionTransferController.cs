using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Antrean dan eksekusi admisi rawat inap dari transfer IGD dan Poliklinik (BE-RWI-206, BE-RWI-207).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/admission-transfers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Admission Transfer",
        AreaName = "HealthServices",
        ControllerName = "InpatientAdmissionTransfer",
        Description = "Antrean dan eksekusi pendaftaran rawat inap dari transfer IGD dan Poliklinik",
        SortOrder = 16
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]
    public class InpatientAdmissionTransferController : ControllerBase
    {
        private readonly InpAdmissionTransferService _service;

        public InpatientAdmissionTransferController(InpAdmissionTransferService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mengambil daftar antrean transfer pasien IGD/Rawat Jalan yang menunggu tempat tidur rawat inap (BE-RWI-206).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<InpatientAdmissionTransferItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Admission Transfer", Description = "Melihat antrean transfer pasien IGD/Poli ke rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionTransfer", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] AdmissionTransferQuery request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedTransfersAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<InpatientAdmissionTransferItemResponse>>.Ok(
                result, "Daftar antrean transfer rawat inap berhasil diambil."));
        }

        /// <summary>
        /// Mengambil detail satu rujukan transfer IGD untuk mengisi awal stepper admisi (BE-RWI-206).
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientAdmissionTransferItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Admission Transfer", Description = "Melihat detail rujukan transfer pasien IGD ke rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionTransfer", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromQuery] string? sourceType = null,
            CancellationToken cancellationToken = default)
        {
            if (sourceType != null && sourceType is not ("Emergency" or "Outpatient"))
                return BadRequest(ApiResponse<object>.Fail(400, "Jenis sumber transfer tidak dikenali."));
            var result = await _service.GetTransferByIdAsync(id, cancellationToken, sourceType);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Permintaan rawat inap tidak ditemukan."))
                : Ok(ApiResponse<InpatientAdmissionTransferItemResponse>.Ok(result, "Detail rujukan transfer berhasil diambil."));
        }

        /// <summary>
        /// Menolak rujukan transfer pasien ke rawat inap dengan alasan penolakan wajib (BE-IGD-077).
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Reject Inpatient Admission Transfer", Description = "Menolak rujukan transfer pasien ke rawat inap", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("InpatientAdmissionTransfer", "Update")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectAdmissionTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Data penolakan tidak valid.", errors));
            }

            var actorUserId = User.GetUserId();
            var result = await _service.RejectAdmissionTransferAsync(id, request, actorUserId, cancellationToken);

            if (!result.Berhasil)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(
                    result.StatusCode,
                    result.Penolakan ?? "Permintaan transfer rawat inap gagal ditolak."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Permintaan transfer rawat inap berhasil ditolak."));
        }
    }
}
