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
    /// Rekonsiliasi obat bawaan pasien rawat inap — <c>BE-RWI-101</c>, api-contract 0.6.0 bagian 12.8.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe transaksi: sub-proses ter-scope induk</b> (episode rawat inap). Tidak ada
    /// <c>DELETE</c>: baris salah catat dibatalkan lewat aksi <c>cancel</c> beralasan, dan keputusan
    /// dokter tidak pernah dihapus — penggantian menambah baris riwayat.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>MedicationReconciliation : Read</c>, <c>: Create</c>, <c>: Update</c>, dan
    /// <b><c>: Decide</c></b> — permission-audit-matrix 0.6.0 bagian 6.1. <c>Decide</c> sengaja terpisah
    /// dari <c>Create</c> supaya perawat yang memegang <c>Create</c> tidak dapat keliru diberi hak
    /// memutuskan lewat layar Akses Role. Di atasnya, service tetap memeriksa pencatat bertugas di unit
    /// episode dan pemutus adalah dokter berpenugasan aktif.
    /// </para>
    /// <para>
    /// Controller ini tidak menyentuh <c>ApplicationDbContext</c>; seluruh aturan berada pada
    /// <see cref="MedicationReconciliationService"/> (<c>QBE-SVC-001</c>).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/medication-reconciliations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Medication Reconciliation",
        AreaName = "HealthServices",
        ControllerName = "MedicationReconciliation",
        Description = "Rekonsiliasi obat bawaan pasien rawat inap: pencatatan perawat dan keputusan dokter",
        SortOrder = 11
    )]
    [Tags("Health Services / Pharmacy Management / Medication Reconciliation")]
    public class MedicationReconciliationController : ControllerBase
    {
        private readonly MedicationReconciliationService _reconciliationService;

        public MedicationReconciliationController(MedicationReconciliationService reconciliationService)
        {
            _reconciliationService = reconciliationService;
        }

        /// <summary>
        /// Daftar obat bawaan satu episode beserta keputusan terakhirnya.
        /// </summary>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<ReconciliationItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medication Reconciliation", Description = "Melihat obat bawaan pasien beserta keputusan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationReconciliation", "Read")]
        public async Task<IActionResult> GetByEpisode(Guid episodeId, CancellationToken cancellationToken)
        {
            var items = await _reconciliationService.GetByEpisodeAsync(episodeId, cancellationToken);

            return Ok(ApiResponse<List<ReconciliationItemResponse>>.Ok(
                items, "Daftar obat bawaan berhasil diambil."));
        }

        /// <summary>
        /// Perawat mencatat obat yang sedang dipakai pasien. <c>DrugId</c> wajib dari master obat.
        /// </summary>
        /// <remarks>
        /// Jawaban: <c>201</c> tercatat; <c>200</c> kiriman ulang berkunci sama; <c>400</c> obat atau cara
        /// pemberian belum dipilih; <c>403</c> tidak bertugas di unit pasien; <c>422</c> perawatan ditutup.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReconciliationItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ReconciliationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Medication Reconciliation", Description = "Perawat mencatat obat bawaan pasien saat admisi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicationReconciliation", "Create")]
        public async Task<IActionResult> RecordHomeMedication(
            [FromBody] CreateReconciliationItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _reconciliationService.RecordHomeMedicationAsync(
                request, User, GetCurrentUserId(), cancellationToken);

            return ToActionResult(result);
        }

        /// <summary>
        /// Membatalkan baris obat bawaan salah catat, sebelum ada keputusan dokter.
        /// </summary>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<ReconciliationItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medication Reconciliation", Description = "Membatalkan obat bawaan yang salah catat sebelum diputuskan dokter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicationReconciliation", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelReconciliationItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _reconciliationService.CancelAsync(
                id, request.Reason, User, GetCurrentUserId(), cancellationToken);

            return ToActionResult(result);
        }

        /// <summary>
        /// Dokter memutuskan satu obat bawaan: Lanjut Sama, Lanjut Ubah, atau Hentikan.
        /// </summary>
        /// <remarks>
        /// "Lanjut" mengisi butir <b>draft</b> resep. Mengganti keputusan sah selama resep hasil
        /// keputusan sebelumnya masih draft. Jawaban: <c>201</c> tersimpan; <c>403</c> bukan dokter yang
        /// merawat pasien (termasuk perawat); <c>409</c> resep hasil keputusan sebelumnya sudah aktif;
        /// <c>422</c> perawatan ditutup atau belum ada catatan dokter terbuka untuk draft resep baru.
        /// </remarks>
        [HttpPost("{id:guid}/decisions")]
        [ProducesResponseType(typeof(ApiResponse<ReconciliationDecisionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Decide", "Decide Medication Reconciliation", Description = "Dokter memutuskan obat bawaan pasien: lanjut sama, lanjut ubah, atau hentikan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicationReconciliation", "Decide")]
        public async Task<IActionResult> Decide(
            Guid id,
            [FromBody] CreateReconciliationDecisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _reconciliationService.DecideAsync(
                id, request, User, GetCurrentUserId(), cancellationToken);

            return ToActionResult(result);
        }

        /// <summary>
        /// Riwayat keputusan satu obat bawaan, termasuk keputusan yang sudah digantikan.
        /// </summary>
        [HttpGet("{id:guid}/decisions")]
        [ProducesResponseType(typeof(ApiResponse<List<ReconciliationDecisionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medication Reconciliation", Description = "Melihat riwayat keputusan satu obat bawaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationReconciliation", "Read")]
        public async Task<IActionResult> GetDecisions(Guid id, CancellationToken cancellationToken)
        {
            var result = await _reconciliationService.GetDecisionsAsync(id, cancellationToken);

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(MedicationReconciliationResult<T> result)
        {
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
