using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Display Antrean Admisi (rawat-jalan Cetak Antrean Admisi v1.1 bagian 8.5): nomor admisi
    /// yang sedang dipanggil dan loket tujuannya. Tidak menampilkan identitas pasien, No. RM,
    /// NIK, data medis, maupun nomor antrean poliklinik internal.
    /// </summary>
    /// <remarks>
    /// Dipanggil akun perangkat layar antrean, sehingga memakai policy
    /// <see cref="AuthorizationPolicies.QueueDisplayRuntimeRead"/> tanpa <c>[AccessPermission]</c>,
    /// sama seperti endpoint runtime display yang sudah ada.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/admission-queue-display")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Admission Queue Display",
        AreaName = "HealthServices",
        ControllerName = "AdmissionQueueDisplay",
        Description = "Display nomor antrean admisi yang dipanggil dan loket tujuannya",
        SortOrder = 12
    )]
    [Tags("Health Services / Registration Management / Admission Queue Display")]
    public class AdmissionQueueDisplayController : ControllerBase
    {
        private readonly AdmissionTicketService _ticketService;

        public AdmissionQueueDisplayController(AdmissionTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.QueueDisplayRuntimeRead)]
        [ProducesResponseType(typeof(ApiResponse<AdmissionQueueDisplayResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] string? queueGroupCode,
            CancellationToken cancellationToken = default)
        {
            return Ok(ApiResponse<AdmissionQueueDisplayResponse>.Ok(
                await _ticketService.GetDisplayAsync(queueGroupCode, cancellationToken),
                "Display antrean admisi berhasil diambil."));
        }
    }
}
