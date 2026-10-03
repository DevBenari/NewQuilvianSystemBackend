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
    /// Daftar Pasien Rawat Jalan — kunjungan Rawat Jalan berklinik yang disaring menurut cakupan
    /// pengguna yang login (RJ-DOC-DEC-012..014, kontrak <c>RJ-DOC-ENCLIST-001@1.0.0</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe transaksi: worklist.</b> Tiga <c>GET</c> baca bercakupan dan satu aksi
    /// <c>PATCH /{id}/cancel</c> (RJ-DOC-REV-BE-010). Tidak ada <c>GET /options</c>,
    /// <c>PATCH /{id}/status</c>, maupun <c>DELETE</c>.
    /// </para>
    /// <para>
    /// Cakupan selalu ditegakkan di server oleh <see cref="ClinicalActorScopeService"/>; pengguna
    /// tanpa data dokter, cluster perawat, maupun <c>OutpatientEncounter : ReadAll</c> ditolak
    /// <c>403</c> (RJDP-VAL-001). Controller tidak menyentuh <c>ApplicationDbContext</c>
    /// (<c>QBE-SVC-001</c>).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/outpatient-encounters")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Outpatient Encounter",
        AreaName = "HealthServices",
        ControllerName = "OutpatientEncounter",
        Description = "Daftar Pasien Rawat Jalan bercakupan dokter/perawat dan pembatalan kunjungan menggantung",
        SortOrder = 3)]
    [Tags("Health Services / Registration Management / Outpatient Encounter")]
    public class OutpatientEncounterController : ControllerBase
    {
        private readonly OutpatientEncounterListService _listService;

        public OutpatientEncounterController(OutpatientEncounterListService listService)
        {
            _listService = listService;
        }

        /// <summary>Daftar kunjungan Rawat Jalan berklinik dalam cakupan pengguna, berhalaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OutpatientEncounterListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Outpatient Encounter", Description = "Melihat Daftar Pasien Rawat Jalan dalam cakupan pengguna", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientEncounter", "Read")]
        public Task<IActionResult> GetList(
            [FromQuery] OutpatientEncounterListQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<PagedResult<OutpatientEncounterListItemResponse>>.Ok(
                await _listService.GetPagedAsync(User, query, cancellationToken),
                "Daftar pasien rawat jalan berhasil diambil.")));

        /// <summary>Jumlah kunjungan per kelompok status untuk saringan yang sama.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientEncounterSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Outpatient Encounter", Description = "Melihat ringkasan Daftar Pasien Rawat Jalan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientEncounter", "Read")]
        public Task<IActionResult> GetSummary(
            [FromQuery] OutpatientEncounterListQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientEncounterSummaryResponse>.Ok(
                await _listService.GetSummaryAsync(User, query, cancellationToken),
                "Ringkasan pasien rawat jalan berhasil diambil.")));

        /// <summary>Cakupan pengguna dan opsi filter.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientEncounterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Outpatient Encounter", Description = "Melihat metadata filter Daftar Pasien Rawat Jalan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientEncounter", "Read")]
        public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientEncounterFilterMetadataResponse>.Ok(
                await _listService.GetFilterMetadataAsync(User, cancellationToken),
                "Metadata filter pasien rawat jalan berhasil diambil.")));

        /// <summary>Membatalkan satu kunjungan yang menggantung.</summary>
        /// <remarks>
        /// Boleh untuk status Draft sampai Menunggu Dokter, dan Sedang Konsultasi tanpa konsultasi
        /// aktif (RJ-DOC-DEC-016, RJ-DOC-DEC-021). Kunjungan di luar cakupan dijawab <c>404</c>.
        /// Bentuk <c>PATCH /{id}/cancel</c> mengikuti kontrak yang disetujui dan endpoint batal
        /// kunjungan yang sudah ada.
        /// </remarks>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientEncounterCancelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Cancel", "Cancel Outpatient Encounter", Description = "Membatalkan kunjungan rawat jalan yang menggantung dari Daftar Pasien Rawat Jalan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OutpatientEncounter", "Cancel")]
        public Task<IActionResult> Cancel(
            Guid id,
            [FromBody] OutpatientEncounterCancelRequest? request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientEncounterCancelResponse>.Ok(
                await _listService.CancelAsync(User, id, request, cancellationToken),
                "Kunjungan berhasil dibatalkan.")));

        private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (OutpatientEncounterNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
            catch (OutpatientEncounterScopeException exception)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (OutpatientEncounterValidationException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
