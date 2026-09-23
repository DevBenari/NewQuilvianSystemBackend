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
    /// Protokol standar sliding scale berversi — <c>BE-RWI-102</c>, api-contract 0.6.0 bagian 12.10.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe: aggregate ber-lifecycle</b> pada tingkat versi (<c>Draft</c> → <c>Approved</c> →
    /// <c>Retired</c>). Tidak ada <c>DELETE</c> dan tidak ada <c>PATCH /status</c> generik: pengesahan
    /// adalah aksi bernama <c>POST /versions/{versionId}/approve</c>.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>SlidingScaleTemplate : Read</c>, <c>: Update</c>, dan <b><c>: Approve</c></b>
    /// (permission-audit-matrix 0.6.0 bagian 6.1). <c>Approve</c> sengaja terpisah dari <c>Update</c>
    /// supaya pemisahan tugas pengubah dan pengesah diatur admin di layar Akses Role; aturan "pengesah
    /// bukan pengubah terakhir" dijaga service dari data versi.
    /// </para>
    /// <para>
    /// <b>Gerbang produksi <c>RWI-OQ-097</c> masih terbuka:</b> nama pengesah isi protokol belum ditunjuk.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/sliding-scale-templates")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Sliding Scale Template",
        AreaName = "HealthServices",
        ControllerName = "SlidingScaleTemplate",
        Description = "Protokol standar sliding scale insulin berversi yang disahkan",
        SortOrder = 12
    )]
    [Tags("Health Services / Pharmacy Management / Sliding Scale Template")]
    public class SlidingScaleTemplateController : ControllerBase
    {
        private readonly SlidingScaleTemplateService _templateService;

        public SlidingScaleTemplateController(SlidingScaleTemplateService templateService)
        {
            _templateService = templateService;
        }

        /// <summary>
        /// Daftar template beserta versi sah yang berlaku. Template tanpa versi sah ditandai
        /// <c>HasApprovedVersion = false</c> — keadaan layar "Belum ada protokol sliding scale yang
        /// disahkan", bukan galat (<c>VAL-DOK-55e</c>).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SlidingScaleTemplateListItem>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Sliding Scale Template", Description = "Melihat protokol sliding scale beserta versi sahnya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleTemplate", "Read")]
        public async Task<IActionResult> GetTemplates(
            [FromQuery] bool? isActive,
            CancellationToken cancellationToken)
        {
            var items = await _templateService.ListAsync(isActive, cancellationToken);

            return Ok(ApiResponse<List<SlidingScaleTemplateListItem>>.Ok(
                items,
                items.Any(x => x.HasApprovedVersion)
                    ? "Daftar protokol sliding scale berhasil diambil."
                    : "Belum ada protokol sliding scale yang disahkan."));
        }

        /// <summary>Template beserta seluruh versinya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleTemplateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Sliding Scale Template", Description = "Melihat protokol sliding scale beserta seluruh versinya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleTemplate", "Read")]
        public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
            => ToActionResult(await _templateService.GetAsync(id, cancellationToken));

        /// <summary>Membuat template baru tanpa versi.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleTemplateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Sliding Scale Template", Description = "Membuat dan mengubah draft protokol sliding scale", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("SlidingScaleTemplate", "Update")]
        public async Task<IActionResult> CreateTemplate(
            [FromBody] CreateSlidingScaleTemplateRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _templateService.CreateTemplateAsync(request, GetCurrentUserId(), cancellationToken));

        /// <summary>
        /// Membuat versi <c>Draft</c> baru beserta rentangnya. Rentang bertumpuk, berlubang, atau tidak
        /// menutup seluruh nilai ditolak <c>400</c>.
        /// </summary>
        [HttpPost("{id:guid}/versions")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleVersionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Sliding Scale Template", Description = "Membuat dan mengubah draft protokol sliding scale", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("SlidingScaleTemplate", "Update")]
        public async Task<IActionResult> CreateVersion(
            Guid id,
            [FromBody] SaveSlidingScaleVersionRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _templateService.CreateVersionAsync(id, request, GetCurrentUserId(), cancellationToken));

        /// <summary>
        /// Mengubah versi yang masih <c>Draft</c>; pengubah terakhir dicatat. Versi <c>Approved</c> atau
        /// <c>Retired</c> ditolak <c>409</c>.
        /// </summary>
        [HttpPut("versions/{versionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Sliding Scale Template", Description = "Membuat dan mengubah draft protokol sliding scale", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("SlidingScaleTemplate", "Update")]
        public async Task<IActionResult> UpdateVersion(
            Guid versionId,
            [FromBody] SaveSlidingScaleVersionRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _templateService.UpdateVersionAsync(versionId, request, GetCurrentUserId(), cancellationToken));

        /// <summary>
        /// Mengesahkan versi <c>Draft</c>. Pengesah wajib berbeda dari pengubah terakhir; versi sah
        /// sebelumnya menjadi <c>Retired</c> dalam transaksi yang sama.
        /// </summary>
        [HttpPost("versions/{versionId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleVersionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Approve", "Approve Sliding Scale Template", Description = "Mengesahkan versi protokol sliding scale; pengesah bukan pengubah terakhir", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SlidingScaleTemplate", "Approve")]
        public async Task<IActionResult> ApproveVersion(
            Guid versionId,
            [FromBody] ApproveSlidingScaleVersionRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _templateService.ApproveVersionAsync(versionId, request, GetCurrentUserId(), cancellationToken));

        private IActionResult ToActionResult<T>(SlidingScaleResult<T> result)
        {
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

            return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
