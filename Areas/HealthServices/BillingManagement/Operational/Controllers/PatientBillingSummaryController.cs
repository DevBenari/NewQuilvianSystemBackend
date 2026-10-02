using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Controllers
{
    /// <summary>
    /// Ringkasan tagihan pasien rawat inap untuk ruang kerja keperawatan — <c>BE-RWI-126</c>,
    /// <c>FR-KEP-082</c>, <c>RWI-DEC-137</c>, <c>RWI-DEC-154</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe transaksi: monitoring read-only.</b> Satu <c>GET</c>, nol endpoint tulis —
    /// kriteria 4. Tidak ada <c>POST</c>, <c>PUT</c>, <c>PATCH</c>, maupun <c>DELETE</c>, dan
    /// controller tidak menyentuh <c>ApplicationDbContext</c> (<c>QBE-SVC-001</c>).
    /// </para>
    /// <para>
    /// <b>Hak akses <c>PatientBillingSummary : Read</c> berdiri sendiri</b> — sengaja tidak menumpang
    /// <c>BillingFolio : Read</c> atau <c>BillingDeposit : Read</c>, supaya admin dapat memberinya
    /// kepada petugas bangsal yang ditunjuk tanpa ikut membuka folio berharga per item.
    /// Pengguna tanpa butir ini ditolak <c>403</c> oleh filter hak akses.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/billing-management/patient-billing-summaries")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
        moduleName: "Health Service Billing Management",
        displayName: "Patient Billing Summary",
        AreaName = "HealthServices",
        ControllerName = "PatientBillingSummary",
        Description = "Ringkasan tagihan pasien rawat inap baca-saja tanpa harga per item",
        SortOrder = 2)]
    [Tags("Health Services / Billing Management / Patient Billing Summary")]
    public class PatientBillingSummaryController : ControllerBase
    {
        private readonly PatientBillingSummaryService _summaryService;

        public PatientBillingSummaryController(PatientBillingSummaryService summaryService)
        {
            _summaryService = summaryService;
        }

        /// <summary>Ringkasan tagihan satu perawatan rawat inap.</summary>
        /// <remarks>
        /// <c>200</c> ringkasan; <c>403</c> tidak memegang <c>PatientBillingSummary : Read</c>;
        /// <c>404</c> perawatan tidak ditemukan; <c>422</c> data deposit tidak dapat dibaca. Tidak ada
        /// cache — setiap pembukaan menu membaca ulang (<c>INT-KEP-14</c>).
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientBillingSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Patient Billing Summary", Description = "Melihat ringkasan tagihan pasien rawat inap tanpa harga per item", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientBillingSummary", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _summaryService.GetByEpisodeAsync(episodeId, cancellationToken);

                if (result == null)
                {
                    return NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Perawatan rawat inap tidak ditemukan."));
                }

                return Ok(ApiResponse<PatientBillingSummaryResponse>.Ok(result, result.Message));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    exception.Message));
            }
            catch (BillingDepositValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Ringkasan tagihan tidak dapat dibaca: " + exception.Message));
            }
        }
    }
}
