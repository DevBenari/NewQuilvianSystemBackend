using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    /// <summary>
    /// Telaah obat akhir oleh apoteker sebelum obat diserahkan.
    /// </summary>
    /// <remarks>
    /// Gerbang wajib bagi seluruh resep. Penyiapan yang selesai membawa resep ke
    /// `AwaitingFinalCheck`; hanya telaah inilah yang boleh menetapkan `ReadyToDispense`.
    /// Telaah yang gagal mengembalikan resep ke `InPreparation` untuk disiapkan ulang.
    /// Penyerahannya sendiri ada pada `PrescriptionDispensing`.
    /// </remarks>
    [ApiController, Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-final-checks")]
    [Tags("Health Services / Pharmacy Management / Prescription Final Check")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
        moduleName: "Health Service Pharmacy Management",
        displayName: "Prescription Final Check",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionFinalCheck",
        Description = "Telaah obat akhir sebelum penyerahan",
        SortOrder = 11)]
    public class PrescriptionFinalCheckController : ControllerBase
    {
        private readonly PrescriptionFinalCheckService _service;
        public PrescriptionFinalCheckController(PrescriptionFinalCheckService service) => _service = service;

        /// <summary>
        /// Menyelesaikan telaah obat akhir.
        /// </summary>
        /// <remarks>
        /// Seluruh kriteria yang dinilai dikirim sekaligus. Telaah dinyatakan lolos hanya bila
        /// tidak ada satu pun kriteria yang tidak patuh. Setiap pemanggilan menyimpan satu
        /// percobaan baru; percobaan sebelumnya ditutup tetapi tidak dihapus.
        /// </remarks>
        [HttpPost("by-prescription/{prescriptionId:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionFinalCheckResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Complete", "Complete Prescription Final Check",
            Description = "Menyelesaikan telaah obat akhir sehingga resep siap diserahkan",
            AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("PrescriptionFinalCheck", "Complete")]
        public async Task<IActionResult> Complete(Guid prescriptionId, [FromBody] CompletePrescriptionFinalCheckRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.CompleteAsync(prescriptionId, request, GetCurrentUserId(), ct);
                var message = result.Status == Enums.PrescriptionFinalCheckStatus.Passed
                    ? "Telaah obat akhir lolos. Resep siap diserahkan."
                    : "Telaah obat akhir tidak lolos. Resep dikembalikan ke penyiapan.";
                return Ok(ApiResponse<PrescriptionFinalCheckResponse>.Ok(result, message));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(raw, out var id) || id == Guid.Empty)
                throw new UnauthorizedAccessException("User aktif tidak valid.");
            return id;
        }
    }
}
