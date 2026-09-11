using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using BloodUnitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodUnitListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Kantong darah operasional — permukaan baca. Kantong lahir dari penerimaan pada
    /// <c>BbkProviderRequestController</c>, tidak pernah dari controller ini.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pada slice <c>BE-BD-004</c> seluruh endpoint di sini hanya membaca.</b> Tindakan atas
    /// kantong lahir bersama task pemiliknya: penyimpanan pada <c>BE-BD-015</c>, alokasi pada
    /// <c>BE-BD-006</c>, bukti kecocokan dan pemberian pada <c>BE-BD-007</c>, jalur darurat pada
    /// <c>BE-BD-008</c>, penyelesaian <c>PendingReview</c> pada <c>BE-BD-009</c>, dan koreksi pada
    /// <c>BE-BD-010</c>.
    /// </para>
    /// <para>
    /// <b>Tidak ada endpoint untuk menambah kantong.</b> Stok bertambah hanya setelah kantong
    /// diterima fisik (<c>VAL-BD-015</c>).
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
        Description = "Memantau kantong darah yang sudah diterima dari PMI",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Unit")]
    public class BbkBloodUnitController : ControllerBase
    {
        private const string NotFoundMessage = "Kantong darah tidak ditemukan atau sudah dihapus.";

        private readonly BbkBloodUnitService _bloodUnitService;

        public BbkBloodUnitController(BbkBloodUnitService bloodUnitService)
        {
            _bloodUnitService = bloodUnitService;
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
        /// (<c>DEC-BD-023</c>).
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

        /// <summary>Detail satu kantong beserta asal dan riwayat perpindahan statusnya.</summary>
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
    }
}
