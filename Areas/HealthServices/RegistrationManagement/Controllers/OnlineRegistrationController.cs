using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using OnlineRegistrationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.OnlineRegistrationListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Layar Pendaftaran Online untuk petugas admisi (kiosk Amendment PO, <c>KSK-DEC-050</c>,
    /// <c>KSK-CONTRACT-v2</c>). Task <c>BE-KSK-PO-009</c> memuat endpoint baca; ubah jadwal, ubah
    /// kontak, dan batal menyusul di <c>BE-KSK-PO-010</c> pada Resource yang sama.
    /// </summary>
    /// <remarks>
    /// Resource <c>OnlineRegistration</c> (<c>KSK-DSN-PO-017</c>) diberikan admin lewat layar
    /// Pengaturan → Manajemen Role → Akses Role, sama dengan Reservasi Prioritas. Kode pendaftaran
    /// tidak pernah dikembalikan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/online-registrations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Online Registration",
        AreaName = "HealthServices",
        ControllerName = "OnlineRegistration",
        Description = "Pendaftaran pasien online rawat jalan: daftar, detail, ubah, dan batal oleh petugas",
        SortOrder = 10
    )]
    [Tags("Health Services / Registration Management / Online Registration")]
    public class OnlineRegistrationController : ControllerBase
    {
        private readonly OnlineRegistrationService _service;

        public OnlineRegistrationController(OnlineRegistrationService service)
        {
            _service = service;
        }

        /// <summary>Daftar pendaftaran online per tanggal kunjungan, status, dan kata kunci.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Online Registration", Description = "Melihat daftar, detail, dan berkas pendaftaran pasien online", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnlineRegistration", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] OnlineRegistrationListQuery query,
            CancellationToken cancellationToken = default)
        {
            var (result, error) = await _service.GetPagedAsync(query, cancellationToken);

            if (error != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error));

            return Ok(ApiResponse<OnlineRegistrationPagedResult>.Ok(result!, "Daftar pendaftaran online berhasil diambil."));
        }

        /// <summary>Jumlah pendaftaran online per status pada rentang tanggal kunjungan.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Online Registration", Description = "Melihat daftar, detail, dan berkas pendaftaran pasien online", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnlineRegistration", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? visitDateFrom,
            [FromQuery] DateTime? visitDateTo,
            CancellationToken cancellationToken = default)
        {
            var (result, error) = await _service.GetSummaryAsync(visitDateFrom, visitDateTo, cancellationToken);

            if (error != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, error));

            return Ok(ApiResponse<OnlineRegistrationSummaryResponse>.Ok(result!, "Ringkasan pendaftaran online berhasil diambil."));
        }

        /// <summary>Detail pendaftaran online beserta riwayat dan daftar berkas rujukan.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OnlineRegistrationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Online Registration", Description = "Melihat daftar, detail, dan berkas pendaftaran pasien online", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnlineRegistration", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _service.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, OnlineRegistrationService.NotFoundMessage));

            return Ok(ApiResponse<OnlineRegistrationDetailResponse>.Ok(detail, "Detail pendaftaran online berhasil diambil."));
        }

        /// <summary>Membuka satu berkas surat rujukan yang diunggah pasien di halaman online.</summary>
        [HttpGet("{id:guid}/referral-documents/{documentId:guid}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Online Registration", Description = "Melihat daftar, detail, dan berkas pendaftaran pasien online", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnlineRegistration", "Read")]
        public async Task<IActionResult> OpenReferralDocument(
            Guid id,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var (error, notFound, content) = await _service.OpenDocumentAsync(id, documentId, cancellationToken);

            if (content == null)
            {
                return notFound
                    ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, error ?? OnlineRegistrationService.DocumentNotFoundMessage))
                    : StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, error ?? OnlineRegistrationService.DocumentNotFoundMessage));
            }

            Response.Headers.CacheControl = "no-store";
            return File(content.Stream, content.ContentType);
        }
    }
}
