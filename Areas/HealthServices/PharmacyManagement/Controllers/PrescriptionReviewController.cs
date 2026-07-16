using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController, Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-reviews")]
    public class PrescriptionReviewController : ControllerBase
    {
        private readonly PrescriptionReviewService _service;
        public PrescriptionReviewController(PrescriptionReviewService service) => _service = service;

        [HttpGet("by-prescription/{prescriptionId:guid}")]
        public async Task<IActionResult> GetByPrescription(Guid prescriptionId, CancellationToken ct)
        {
            var result = await _service.GetActiveAsync(prescriptionId, ct);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(404, "Telaah resep belum tersedia."))
                : Ok(ApiResponse<PrescriptionReviewResponse>.Ok(result, "Telaah resep berhasil diambil."));
        }

        [HttpPost("by-prescription/{prescriptionId:guid}/start")]
        public async Task<IActionResult> Start(Guid prescriptionId, [FromBody] StartPrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.StartAsync(prescriptionId, GetCurrentUserId(), request.GeneralNote, ct), "Telaah resep berhasil dimulai.");

        [HttpPut("{reviewId:guid}/items")]
        public async Task<IActionResult> UpdateItems(Guid reviewId, [FromBody] UpdatePrescriptionReviewItemsRequest request, CancellationToken ct)
            => await Execute(() => _service.UpdateItemsAsync(reviewId, request, GetCurrentUserId(), ct), "Kriteria telaah berhasil disimpan.");

        [HttpPost("{reviewId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid reviewId, [FromBody] CompletePrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.CompleteAsync(reviewId, true, request.GeneralNote, GetCurrentUserId(), ct), "Telaah resep disetujui.");

        [HttpPost("{reviewId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid reviewId, [FromBody] CompletePrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.CompleteAsync(reviewId, false, request.GeneralNote, GetCurrentUserId(), ct), "Telaah resep ditolak.");

        [HttpPost("{reviewId:guid}/clarifications")]
        public async Task<IActionResult> CreateClarification(Guid reviewId, [FromBody] CreatePrescriptionClarificationRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.CreateClarificationAsync(reviewId, request, GetCurrentUserId(), ct);
                return Ok(ApiResponse<PrescriptionClarificationResponse>.Ok(result, "Klarifikasi berhasil dibuat."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPost("clarifications/{id:guid}/doctor-response")]
        public async Task<IActionResult> DoctorResponse(Guid id, [FromBody] DoctorClarificationResponseRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.RespondClarificationAsync(id, request, GetCurrentUserId(), ct);
                return Ok(ApiResponse<PrescriptionClarificationResponse>.Ok(result, "Jawaban dokter berhasil disimpan."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPost("clarifications/{id:guid}/close")]
        public async Task<IActionResult> CloseClarification(Guid id, [FromBody] ClosePrescriptionClarificationRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.CloseClarificationAsync(id, request, GetCurrentUserId(), ct);
                return Ok(ApiResponse<PrescriptionClarificationResponse>.Ok(result, "Klarifikasi berhasil ditutup."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        private async Task<IActionResult> Execute(Func<Task<PrescriptionReviewResponse>> action, string message)
        {
            try { return Ok(ApiResponse<PrescriptionReviewResponse>.Ok(await action(), message)); }
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
