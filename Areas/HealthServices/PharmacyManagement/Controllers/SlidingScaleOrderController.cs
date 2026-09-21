using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    /// <summary>
    /// Protokol sliding scale per pasien — <c>BE-RWI-103</c>, api-contract 0.6.0 bagian 12.11.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe: aggregate ber-lifecycle</b> (<c>Active</c> → <c>Stopped</c>) dengan versi. Tidak ada
    /// <c>DELETE</c>: order dihentikan beralasan, dan penyesuaian menambah versi.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>SlidingScaleOrder : Read</c>, <c>: Create</c>, <c>: Update</c>. Di atasnya
    /// service memeriksa dokter tertaut akun login punya penugasan aktif pada episode.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/sliding-scale-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Sliding Scale Order",
        AreaName = "HealthServices",
        ControllerName = "SlidingScaleOrder",
        Description = "Protokol sliding scale insulin per pasien rawat inap",
        SortOrder = 13
    )]
    [Tags("Health Services / Pharmacy Management / Sliding Scale Order")]
    public class SlidingScaleOrderController : ControllerBase
    {
        private readonly SlidingScaleOrderService _orderService;

        public SlidingScaleOrderController(SlidingScaleOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Dokter memesan protokol pada butir insulin draft resep. Rentang versi template <b>disalin</b>;
        /// bila rentang atau dosis diubah, alasan wajib.
        /// </summary>
        /// <remarks>
        /// Jawaban: <c>201</c> dipesan; <c>400</c> rentang tidak sah atau alasan kosong; <c>403</c> bukan
        /// dokter yang merawat; <c>409</c> versi template belum disahkan, butir bukan insulin berdosis
        /// skala, atau butir sudah punya order; <c>422</c> perawatan ditutup.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleOrderResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Sliding Scale Order", Description = "Dokter memesan protokol sliding scale pada butir insulin", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SlidingScaleOrder", "Create")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateSlidingScaleOrderRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _orderService.CreateAsync(request, User, GetCurrentUserId(), cancellationToken));

        /// <summary>Order sliding scale satu episode.</summary>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<SlidingScaleOrderListItem>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Sliding Scale Order", Description = "Melihat protokol sliding scale pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleOrder", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] SlidingScaleOrderStatus? status,
            CancellationToken cancellationToken)
        {
            var items = await _orderService.GetByEpisodeAsync(episodeId, status, cancellationToken);

            return Ok(ApiResponse<List<SlidingScaleOrderListItem>>.Ok(
                items, "Daftar protokol sliding scale pasien berhasil diambil."));
        }

        /// <summary>Order beserta seluruh versi dan rentangnya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Sliding Scale Order", Description = "Melihat protokol sliding scale beserta seluruh versinya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SlidingScaleOrder", "Read")]
        public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAsync(id, cancellationToken));

        /// <summary>
        /// Menyesuaikan order → versi baru. <c>ExpectedVersionNumber</c> yang basi ditolak <c>409</c>.
        /// </summary>
        [HttpPost("{id:guid}/versions")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Sliding Scale Order", Description = "Dokter menyesuaikan atau menghentikan protokol sliding scale pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SlidingScaleOrder", "Update")]
        public async Task<IActionResult> AdjustOrder(
            Guid id,
            [FromBody] AdjustSlidingScaleOrderRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _orderService.AdjustAsync(id, request, User, GetCurrentUserId(), cancellationToken));

        /// <summary>Menghentikan order; pelaksanaan berikutnya ditolak. Alasan wajib.</summary>
        [HttpPatch("{id:guid}/stop")]
        [ProducesResponseType(typeof(ApiResponse<SlidingScaleOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Sliding Scale Order", Description = "Dokter menyesuaikan atau menghentikan protokol sliding scale pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SlidingScaleOrder", "Update")]
        public async Task<IActionResult> StopOrder(
            Guid id,
            [FromBody] StopSlidingScaleOrderRequest request,
            CancellationToken cancellationToken)
            => ToActionResult(await _orderService.StopAsync(id, request.Reason, User, GetCurrentUserId(), cancellationToken));

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
