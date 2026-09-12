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

using BloodUnitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodUnitListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Kantong darah operasional. Kantong lahir dari penerimaan pada
    /// <c>BbkProviderRequestController</c>, tidak pernah dari controller ini.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Sejak <c>BE-BD-015</c></b> controller ini juga menyimpan dan memindahkan kantong, dijaga
    /// butir <c>BloodUnit : Store</c> — sengaja terpisah dari <c>Allocate</c>: menaruh kantong ke
    /// kulkas adalah pekerjaan gudang, mengalokasikan adalah mengikat kantong pada pasien.
    /// </para>
    /// <para>
    /// Tindakan lain lahir bersama task pemiliknya: alokasi pada <c>BE-BD-006</c>, bukti kecocokan
    /// dan pemberian pada <c>BE-BD-007</c>, jalur darurat pada <c>BE-BD-008</c>, penyelesaian
    /// <c>PendingReview</c> pada <c>BE-BD-009</c>, dan koreksi pada <c>BE-BD-010</c>.
    /// </para>
    /// <para>
    /// <b>Tidak ada endpoint untuk menambah kantong</b> (<c>VAL-BD-015</c>), dan <b>tidak ada endpoint
    /// untuk memindahkan kantong secara massal</b> — petugas memindahkan satu per satu, dan setiap
    /// perpindahan menyimpan pelaku serta waktunya sendiri (<c>DEC-BD-037</c>).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/blood-units")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Unit",
        AreaName = "HealthServices",
        ControllerName = "BloodUnit",
        Description = "Memantau, menyimpan, dan memindahkan kantong darah yang sudah diterima dari PMI",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Unit")]
    public class BbkBloodUnitController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.BloodUnit";
        private const string NotFoundMessage = "Kantong darah tidak ditemukan atau sudah dihapus.";

        private readonly BbkBloodUnitService _bloodUnitService;
        private readonly LoggerService _loggerService;

        public BbkBloodUnitController(
            BbkBloodUnitService bloodUnitService,
            LoggerService loggerService)
        {
            _bloodUnitService = bloodUnitService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring dan pengurutan daftar kantong darah.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat konfigurasi penyaring kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodUnitFilterMetadataResponse>.Ok(
                BbkBloodUnitService.BuildFilterMetadata(),
                "Konfigurasi penyaring kantong darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah kantong per status, beserta jumlah kantong berlebih.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat ringkasan kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _bloodUnitService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodUnitSummaryResponse>.Ok(
                summary,
                "Ringkasan kantong darah berhasil diambil."));
        }

        /// <summary>
        /// Daftar kantong darah. Penyaring <c>unitStatus=PendingReview</c> menjadi daftar kerja #2
        /// (<c>DEC-BD-023</c>). Setiap baris membawa lokasi saat ini beserta penanda keaktifannya.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat daftar kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] BbkBloodUnitStatus? unitStatus,
            [FromQuery] bool? isExcess,
            [FromQuery] Guid? providerRequestId,
            [FromQuery] Guid? bloodComponentId,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.GetPagedAsync(
                search,
                unitStatus,
                isExcess,
                providerRequestId,
                bloodComponentId,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodUnitPagedResult>.Ok(
                result,
                "Daftar kantong darah berhasil diambil."));
        }

        /// <summary>Detail satu kantong beserta asal, lokasi saat ini, dan riwayat statusnya.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat detail kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _bloodUnitService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                detail,
                "Detail kantong darah berhasil diambil."));
        }

        /// <summary>Riwayat perpindahan status kantong, terlama lebih dulu.</summary>
        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodBankTransitionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat riwayat status kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public async Task<IActionResult> GetStatusHistory(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var history = await _bloodUnitService.GetStatusHistoryAsync(id, cancellationToken);

            if (history == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<List<BloodBankTransitionDto>>.Ok(
                history,
                "Riwayat status kantong darah berhasil diambil."));
        }

        /// <summary>Riwayat penempatan kantong: di kulkas mana, sejak kapan, oleh siapa.</summary>
        /// <remarks>
        /// Terlama lebih dulu. Daftar kosong berarti kantong belum pernah disimpan. Lokasi yang
        /// sudah dinonaktifkan tetap terbaca pada riwayat.
        /// </remarks>
        [HttpGet("{id:guid}/placements")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodUnitPlacementDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Unit", Description = "Melihat riwayat penempatan kantong darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodUnit", "Read")]
        public async Task<IActionResult> GetPlacements(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var placements = await _bloodUnitService.GetPlacementsAsync(id, cancellationToken);

            if (placements == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<List<BloodUnitPlacementDto>>.Ok(
                placements,
                placements.Count == 0
                    ? "Kantong belum pernah disimpan."
                    : "Riwayat penempatan kantong darah berhasil diambil."));
        }

        /// <summary>Menetapkan lokasi penyimpanan pertama kantong.</summary>
        /// <remarks>
        /// Membawa kantong <c>Received</c> → <c>Stored</c> → <c>Available</c>, atau
        /// <c>PendingReview</c> bila kantongnya berlebih atau permintaan asalnya sudah ditutup
        /// (<c>DEC-BD-036</c>). Gagal bila kantong sudah pernah ditempatkan (<c>VAL-BD-061</c>) atau
        /// lokasinya nonaktif (<c>VAL-BD-060</c>).
        /// </remarks>
        [HttpPost("{id:guid}/storage-location")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Store", "Store Blood Unit", Description = "Menetapkan lokasi penyimpanan pertama kantong darah", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("BloodUnit", "Store")]
        public async Task<IActionResult> AssignStorageLocation(
            Guid id,
            [FromBody] AssignStorageLocationRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.AssignStorageLocationAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogStorageAsync("BloodUnit.AssignStorageLocation", "Menetapkan lokasi penyimpanan pertama kantong darah.", result, request.StorageLocationId);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                await _bloodUnitService.GetDetailAsync(id, cancellationToken),
                result.Message));
        }

        /// <summary>Memindahkan kantong ke lokasi penyimpanan lain. Status kantong tidak berubah.</summary>
        /// <remarks>
        /// Tetap berlaku ketika lokasi asalnya sudah dinonaktifkan — inilah jalan keluar kantong dari
        /// kulkas yang rusak (<c>DEC-BD-037</c>). Gagal bila kantong belum pernah ditempatkan
        /// (<c>VAL-BD-062</c>) atau lokasi tujuannya nonaktif (<c>VAL-BD-060</c>).
        /// </remarks>
        [HttpPut("{id:guid}/storage-location")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Store", "Store Blood Unit", Description = "Memindahkan kantong darah ke lokasi penyimpanan lain", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("BloodUnit", "Store")]
        public async Task<IActionResult> MoveStorageLocation(
            Guid id,
            [FromBody] MoveStorageLocationRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.MoveStorageLocationAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogStorageAsync("BloodUnit.MoveStorageLocation", "Memindahkan kantong darah ke lokasi penyimpanan lain.", result, request.StorageLocationId);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                await _bloodUnitService.GetDetailAsync(id, cancellationToken),
                result.Message));
        }

        /// <remarks>Nomor kantong PMI sensitif dan sengaja tidak ditulis ke log.</remarks>
        private Task LogStorageAsync(string eventName, string message, BloodUnitResult result, Guid storageLocationId)
            => _loggerService.InfoAsync(
                LogCategory,
                eventName,
                message,
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.CurrentPlacementId,
                    StorageLocationId = storageLocationId,
                    UnitStatus = result.Entity.UnitStatus.ToString(),
                    Controller = "BloodUnit",
                    Action = "Store"
                });

        /// <remarks>
        /// <c>Invalid</c> → <c>400</c>; <c>NotFound</c> → <c>404</c>; <c>VersionConflict</c> →
        /// <c>409</c>; <c>NotAllowedByState</c> → <c>422</c> (<c>VAL-BD-060/061/062</c>, master kosong).
        /// </remarks>
        private IActionResult MapFailure(BloodUnitResult result)
            => result.Outcome switch
            {
                BloodUnitOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                BloodUnitOutcome.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                BloodUnitOutcome.NotAllowedByState => UnprocessableEntity(
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
