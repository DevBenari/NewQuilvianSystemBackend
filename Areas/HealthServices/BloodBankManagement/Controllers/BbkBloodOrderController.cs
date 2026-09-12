using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodOrderPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodOrderListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Daftar kerja order darah — order dibuat elektronik maupun manual, order ganda tertahan,
    /// dan pembatalan menuntut alasan berkategori sesuai peran.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bentuk transaksi, bukan master data.</b> Karena itu sengaja tidak ada
    /// <c>GET /options</c>, tidak ada <c>PATCH /{id}/status</c> generik, dan tidak ada
    /// <c>DELETE /{id}</c>: order yang sudah terjadi tidak dihapus, dan statusnya berpindah
    /// karena kejadian bernama. Order dibatalkan lewat aksi <c>cancel</c>, dengan alasan yang
    /// tersimpan permanen.
    /// </para>
    ///
    /// <para>
    /// <b><c>BloodOrder : Cancel</c> adalah butir tersendiri, terpisah dari
    /// <c>BloodOrder : Update</c></b> (<c>DEC-BD-044</c>). Pemisahan ini memungkinkan wewenang
    /// membatalkan diberikan kepada dokter peminta <b>tanpa</b> ikut memberikan wewenang
    /// menyunting order secara umum. Dokter peminta dan petugas BDRS memakai butir yang
    /// <b>sama</b>; yang membedakan sebabnya pada rekam adalah <b>kategori alasan</b> yang
    /// wajib diisi — pembatalan klinis atau pembatalan operasional (<c>VAL-BD-083</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Kedaluwarsa order tidak punya endpoint</b>, dan itu disengaja: ia dipicu sistem dari
    /// sinyal berakhirnya kunjungan lewat <c>BbkEncounterStatusReader</c> (<c>DEC-BD-014</c>),
    /// bukan oleh seseorang yang menekan tombol.
    /// </para>
    ///
    /// <para>
    /// <b>Contoh alur nyata.</b> Pasien rawat inap sudah punya order PRC aktif pada kunjungan
    /// ini. Perawat membuat order PRC lagi: sistem menahannya dan meminta alasan tertulis
    /// (<c>VAL-BD-001</c>). Order trombosit untuk pasien yang sama <b>tidak</b> tertahan —
    /// deteksi ganda bekerja per komponen, bukan per pasien.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/blood-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Order",
        AreaName = "HealthServices",
        ControllerName = "BloodOrder",
        Description = "Membuat, memantau, dan membatalkan order darah",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Order")]
    public class BbkBloodOrderController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.BloodOrder";

        private readonly BbkBloodOrderService _bloodOrderService;
        private readonly LoggerService _loggerService;

        public BbkBloodOrderController(
            BbkBloodOrderService bloodOrderService,
            LoggerService loggerService)
        {
            _bloodOrderService = bloodOrderService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring dan pengurutan untuk daftar kerja order darah.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat konfigurasi penyaring order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodOrderFilterMetadataResponse>.Ok(
                BbkBloodOrderService.BuildFilterMetadata(),
                "Konfigurasi penyaring order darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah order darah per status.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat ringkasan order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _bloodOrderService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodOrderSummaryResponse>.Ok(
                summary,
                "Ringkasan order darah berhasil diambil."));
        }

        /// <summary>Daftar kerja order darah, dengan penyaring dan halaman (<c>DEC-BD-023</c>).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat daftar order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] BbkBloodOrderStatus? orderStatus,
            [FromQuery] BbkOrderSource? orderSource,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.GetPagedAsync(
                search,
                patientId,
                encounterId,
                serviceUnitId,
                orderStatus,
                orderSource,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodOrderPagedResult>.Ok(
                result,
                "Daftar order darah berhasil diambil."));
        }

        /// <summary>Detail satu order beserta baris, pemenuhan, dan riwayat perpindahan statusnya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat detail order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _bloodOrderService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                detail,
                "Detail order darah berhasil diambil."));
        }

        /// <summary>
        /// Ringkasan pemenuhan order: diminta, diberikan, dan sisa (<c>BD-DOM-17</c>).
        /// </summary>
        /// <remarks>
        /// Seluruh angkanya <b>dihitung saat ditanya</b> dari pemberian yang nyata, tidak pernah
        /// dibaca dari kolom yang dapat disunting, dan wajib menghormati catatan koreksi
        /// pemberian supaya satu pemberian tidak terhitung dua kali (<c>DEC-BD-030</c>).
        /// </remarks>
        [HttpGet("{id:guid}/fulfillment")]
        [ProducesResponseType(typeof(ApiResponse<FulfillmentSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat ringkasan pemenuhan order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetFulfillment(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var fulfillment = await _bloodOrderService.GetFulfillmentAsync(id, cancellationToken);

            if (fulfillment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<FulfillmentSummaryDto>.Ok(
                fulfillment,
                "Ringkasan pemenuhan order darah berhasil diambil."));
        }

        /// <summary>Riwayat perpindahan status order, terlama lebih dulu.</summary>
        /// <remarks>
        /// Dibaca dari <c>BbkTransitionHistory</c> yang hanya dapat ditambah. Setiap baris
        /// menyimpan pelaku, waktu, status asal dan tujuan, serta kode alasan beserta
        /// <b>salinan teksnya</b> pada saat kejadian — sehingga pembatalan lama tetap terbaca
        /// seperti saat diputuskan walau teks alasan di data induk kelak disunting
        /// (<c>INV-BD-035</c>).
        /// </remarks>
        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodOrderTransitionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat riwayat status order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetStatusHistory(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var history = await _bloodOrderService.GetStatusHistoryAsync(id, cancellationToken);

            if (history == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<List<BloodOrderTransitionDto>>.Ok(
                history,
                "Riwayat status order darah berhasil diambil."));
        }

        /// <summary>Membuat order darah elektronik dari unit pelayanan yang berwenang.</summary>
        /// <remarks>
        /// Pelaku input diturunkan dari pengguna terautentikasi, bukan dari isian permintaan —
        /// itulah cara <c>VAL-BD-011</c> ditegakkan tanpa dapat dilewati.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Membuat order darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "Create");
        }

        /// <summary>Membuat order darah manual yang diinput petugas Bank Darah.</summary>
        /// <remarks>
        /// <c>VAL-BD-010</c> menuntut kelengkapan pasien, kunjungan, dokter peminta, unit asal,
        /// dan petugas yang menginput. Petugasnya tidak pernah diterima dari isian permintaan.
        /// </remarks>
        [HttpPost("manual")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Membuat order darah manual", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> CreateManual(
            [FromBody] CreateManualBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CreateManualAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "CreateManual");
        }

        /// <summary>
        /// Melanjutkan order yang tertahan deteksi ganda, dengan alasan tertulis
        /// (<c>ASM-BD-001</c>).
        /// </summary>
        /// <remarks>
        /// <b>Bukan tombol "abaikan pemeriksaan".</b> Alasan tertulis wajib diisi dan tersimpan
        /// permanen pada ordernya, sehingga setiap order ganda yang tetap dibuat selalu dapat
        /// dijelaskan sebabnya.
        /// </remarks>
        [HttpPost("confirm-duplicate")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Melanjutkan order darah ganda dengan alasan tertulis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> ConfirmDuplicate(
            [FromBody] ConfirmDuplicateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.ConfirmDuplicateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "ConfirmDuplicate");
        }

        /// <summary>
        /// Membatalkan order dengan alasan terkendali — oleh dokter peminta atau petugas BDRS
        /// (<c>DEC-BD-044</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Butir hak akses ini terpisah dari <c>BloodOrder : Update</c></b>, supaya wewenang
        /// membatalkan dapat diberikan kepada dokter peminta tanpa ikut memberikan wewenang
        /// menyunting order.
        /// </para>
        /// <para>
        /// <b>Kategori alasan wajib sesuai pelakunya</b> (<c>VAL-BD-083</c>): alasan berkategori
        /// pembatalan klinis hanya sah bagi dokter peminta order ini — yang dinilai dari
        /// kepemilikan data, bukan dari nama peran — sedangkan pembatalan operasional dipakai
        /// petugas Bank Darah. Tidak ada pembatalan order tanpa jejak (<c>INV-BD-035</c>).
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Cancel", "Cancel Blood Order", Description = "Membatalkan order darah dengan alasan terkendali", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodOrder", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CancelAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodOrderOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodOrder.Cancel",
                "Membatalkan order darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.OrderNumber,
                    request.ReasonCode,
                    Controller = "BloodOrder",
                    Action = "Cancel"
                });

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, cancellationToken),
                result.Message));
        }

        // =================================================================
        // Penolong
        // =================================================================

        /// <summary>Jalur balasan bersama ketiga endpoint pembuatan order.</summary>
        private async Task<IActionResult> RespondToWriteAsync(BloodOrderResult result, string action)
        {
            if (result.Outcome != BloodOrderOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                $"BloodOrder.{action}",
                "Membuat order darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.OrderNumber,
                    result.Entity.PatientId,
                    result.Entity.OrderSource,
                    Controller = "BloodOrder",
                    Action = action
                });

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, HttpContext.RequestAborted),
                result.Message));
        }

        /// <summary>
        /// Memuat ulang detail lengkap supaya balasan memuat nama pasien, unit, dokter, dan
        /// riwayat perpindahan status — bukan hanya kolom yang sempat ter-tracking.
        /// </summary>
        private async Task<BloodOrderDetailDto> BuildDetailAsync(
            Models.BbkBloodOrder entity,
            CancellationToken cancellationToken)
            => await _bloodOrderService.GetDetailAsync(entity.Id, cancellationToken)
               ?? BbkBloodOrderService.ToDetail(entity);

        /// <remarks>
        /// <para>
        /// Pemetaan status mengikuti kontrak <c>v4</c>:
        /// </para>
        /// <list type="bullet">
        /// <item><c>DuplicateOrder</c> → <c>422 VAL-BD-001</c>.</item>
        /// <item><c>UnitNotAuthorized</c> → <c>403 VAL-BD-013</c>.</item>
        /// <item><c>VersionConflict</c> → <c>409</c>.</item>
        /// <item>
        /// <c>NotAllowedByState</c> → <c>422</c>, bukan <c>403</c>: pelakunya berwenang — kalau
        /// tidak, <c>AccessPermissionFilter</c> sudah menahannya lebih dulu — yang tidak
        /// memenuhi syarat adalah keadaan datanya.
        /// </item>
        /// </list>
        /// </remarks>
        private IActionResult MapFailure(BloodOrderResult result)
            => result.Outcome switch
            {
                BloodOrderOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                BloodOrderOutcome.UnitNotAuthorized => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, result.Message)),

                BloodOrderOutcome.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                BloodOrderOutcome.DuplicateOrder => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message)),

                BloodOrderOutcome.NotAllowedByState => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message)),

                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message))
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
