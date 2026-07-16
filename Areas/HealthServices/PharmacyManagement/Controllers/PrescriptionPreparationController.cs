using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController, Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-preparations")]
    public class PrescriptionPreparationController : ControllerBase
    {
        private readonly PrescriptionPreparationService _service;
        public PrescriptionPreparationController(PrescriptionPreparationService service) => _service = service;

        [HttpPost("by-prescription/{prescriptionId:guid}/start")]
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
