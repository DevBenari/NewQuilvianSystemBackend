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
    /// Penyiapan obat resep oleh farmasi.
    /// </summary>
    /// <remarks>
    /// Tahap sebelum penyerahan: resep yang sudah diverifikasi disiapkan, lalu dinyatakan siap
    /// diserahkan. Penyerahannya sendiri ada pada `PrescriptionDispensing`.
    /// </remarks>
    [ApiController, Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-preparations")]
    [Tags("Health Services / Pharmacy Management / Prescription Preparation")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
        moduleName: "Health Service Pharmacy Management",
        displayName: "Prescription Preparation",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionPreparation",
        Description = "Penyiapan obat resep",
        SortOrder = 9)]
    public class PrescriptionPreparationController : ControllerBase
    {
        private readonly PrescriptionPreparationService _service;
        public PrescriptionPreparationController(PrescriptionPreparationService service) => _service = service;

        [HttpPost("by-prescription/{prescriptionId:guid}/start")]
        [AccessAction("Start", "Start Prescription Preparation",
            Description = "Memulai penyiapan obat resep",
            AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("PrescriptionPreparation", "Start")]
        public async Task<IActionResult> Start(Guid prescriptionId, [FromBody] StartPrescriptionPreparationRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.StartAsync(prescriptionId, GetCurrentUserId(), request.PreparationNote, ct);
                return Ok(ApiResponse<PrescriptionPreparationResponse>.Ok(result, "Penyiapan obat berhasil dimulai."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPost("by-prescription/{prescriptionId:guid}/complete")]
        [AccessAction("Complete", "Complete Prescription Preparation",
            Description = "Menyelesaikan penyiapan sehingga resep siap diserahkan",
            AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PrescriptionPreparation", "Complete")]
        public async Task<IActionResult> Complete(Guid prescriptionId, [FromBody] CompletePrescriptionPreparationRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.CompleteAsync(prescriptionId, request, GetCurrentUserId(), ct);
                return Ok(ApiResponse<PrescriptionPreparationResponse>.Ok(result, "Penyiapan obat berhasil diselesaikan."));
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
