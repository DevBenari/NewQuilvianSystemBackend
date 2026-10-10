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
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetTransferByIdAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rujukan transfer IGD tidak ditemukan."))
                : Ok(ApiResponse<InpatientAdmissionTransferItemResponse>.Ok(result, "Detail rujukan transfer berhasil diambil."));
        }

        /// <summary>
        /// Mengeksekusi pendaftaran rawat inap dari rujukan transfer IGD secara atomik (BE-RWI-207).
        /// </summary>
        [HttpPost("admit")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Inpatient Episode from Transfer", Description = "Menyelesaikan pendaftaran rawat inap dari transfer IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InpatientEpisode", "Create")]
        public async Task<IActionResult> AdmitFromTransfer(
            [FromBody] OpenAdmissionFromTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Validasi formulir gagal."));
            }

            var result = await _service.ExecuteTransferAdmissionAsync(
                request,
                User.GetUserId(),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Status switch
                {
                    InpEpisodeOperationStatus.NotFound => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                    InpEpisodeOperationStatus.Conflict => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
                    InpEpisodeOperationStatus.BusinessRuleRejected => UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message)),
                    _ => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
                };
            }

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<object>.Ok(result.Data, result.Message));
        }
    }
}
