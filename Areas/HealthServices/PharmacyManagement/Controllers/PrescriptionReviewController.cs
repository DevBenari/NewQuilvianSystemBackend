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
    /// Telaah resep oleh apoteker beserta klarifikasinya kepada dokter.
    /// </summary>
    /// <remarks>
    /// Menyetujui dan menolak dipisahkan sebagai dua aksi yang berbeda, bukan satu aksi dengan
    /// parameter. Keduanya berbeda akibatnya bagi pasien, dan izin untuk menyetujui tidak
    /// dengan sendirinya berarti izin untuk menolak.
    /// </remarks>
    [ApiController, Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-reviews")]
    [Tags("Health Services / Pharmacy Management / Prescription Review")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
        moduleName: "Health Service Pharmacy Management",
        displayName: "Prescription Review",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionReview",
        Description = "Telaah resep dan klarifikasi ke dokter",
        SortOrder = 10)]
    public class PrescriptionReviewController : ControllerBase
    {
        private readonly PrescriptionReviewService _service;
        public PrescriptionReviewController(PrescriptionReviewService service) => _service = service;

        [HttpGet("by-prescription/{prescriptionId:guid}")]
        [AccessAction("Read", "Read Prescription Review",
            Description = "Melihat telaah resep beserta klarifikasinya",
            AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionReview", "Read")]
        public async Task<IActionResult> GetByPrescription(Guid prescriptionId, CancellationToken ct)
        {
            var result = await _service.GetActiveAsync(prescriptionId, ct);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(404, "Telaah resep belum tersedia."))
                : Ok(ApiResponse<PrescriptionReviewResponse>.Ok(result, "Telaah resep berhasil diambil."));
        }

        [HttpPost("by-prescription/{prescriptionId:guid}/start")]
        [AccessAction("Start", "Start Prescription Review",
            Description = "Memulai telaah resep",
            AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PrescriptionReview", "Start")]
        public async Task<IActionResult> Start(Guid prescriptionId, [FromBody] StartPrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.StartAsync(prescriptionId, GetCurrentUserId(), request.GeneralNote, ct), "Telaah resep berhasil dimulai.");

        [HttpPut("{reviewId:guid}/items")]
        [AccessAction("Update", "Update Prescription Review Items",
            Description = "Menyimpan kriteria telaah per item resep",
            AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PrescriptionReview", "Update")]
        public async Task<IActionResult> UpdateItems(Guid reviewId, [FromBody] UpdatePrescriptionReviewItemsRequest request, CancellationToken ct)
            => await Execute(() => _service.UpdateItemsAsync(reviewId, request, GetCurrentUserId(), ct), "Kriteria telaah berhasil disimpan.");

        [HttpPost("{reviewId:guid}/approve")]
        [AccessAction("Approve", "Approve Prescription Review",
            Description = "Menyetujui telaah resep sehingga obat boleh disiapkan",
            AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PrescriptionReview", "Approve")]
        public async Task<IActionResult> Approve(Guid reviewId, [FromBody] CompletePrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.CompleteAsync(reviewId, true, request.GeneralNote, GetCurrentUserId(), ct), "Telaah resep disetujui.");

        [HttpPost("{reviewId:guid}/reject")]
        [AccessAction("Reject", "Reject Prescription Review",
            Description = "Menolak telaah resep",
            AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PrescriptionReview", "Reject")]
        public async Task<IActionResult> Reject(Guid reviewId, [FromBody] CompletePrescriptionReviewRequest request, CancellationToken ct)
            => await Execute(() => _service.CompleteAsync(reviewId, false, request.GeneralNote, GetCurrentUserId(), ct), "Telaah resep ditolak.");

        [HttpPost("{reviewId:guid}/clarifications")]
        [AccessAction("Clarify", "Create Prescription Clarification",
            Description = "Mengajukan klarifikasi resep kepada dokter",
            AccessType = AccessTypes.Create, SortOrder = 6)]
        [AccessPermission("PrescriptionReview", "Clarify")]
        public async Task<IActionResult> CreateClarification(Guid reviewId, [FromBody] CreatePrescriptionClarificationRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _service.CreateClarificationAsync(reviewId, request, GetCurrentUserId(), ct);
                return Ok(ApiResponse<PrescriptionClarificationResponse>.Ok(result, "Klarifikasi berhasil dibuat."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        // Yang menjawab klarifikasi adalah dokter, bukan apoteker. Izinnya karena itu dipisahkan
        // dari Clarify: yang mengajukan pertanyaan dan yang menjawabnya bukan orang yang sama.
        [HttpPost("clarifications/{id:guid}/doctor-response")]
        [AccessAction("Respond", "Respond Prescription Clarification",
            Description = "Menjawab klarifikasi resep sebagai dokter",
            AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("PrescriptionReview", "Respond")]
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
        [AccessAction("CloseClarification", "Close Prescription Clarification",
            Description = "Menutup klarifikasi resep",
            AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("PrescriptionReview", "CloseClarification")]
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
