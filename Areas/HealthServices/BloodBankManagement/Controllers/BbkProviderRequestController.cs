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

using ProviderRequestPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.ProviderRequestListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    /// <summary>
    /// Permintaan darah ke PMI — permintaan dicatat, penerimaan kantong termasuk kelebihan
    /// dicatat, dan kantong lahir berstatus <c>Received</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bentuk transaksi, bukan master data.</b> Tidak ada <c>GET /options</c>, tidak ada
    /// <c>PATCH /{id}/status</c> generik, dan tidak ada <c>DELETE /{id}</c>: permintaan yang sudah
    /// terjadi tidak dihapus, statusnya berpindah karena penerimaan atau pembatalan bernama.
    /// </para>
    ///
    /// <para>
    /// <b>Pengirimannya manual di luar sistem</b> (<c>DEC-BD-002</c>). Quilvian mencatat permintaan
    /// dan kantong yang kemudian datang; stok bertambah hanya lewat penerimaan fisik.
    /// </para>
    ///
    /// <para>
    /// <b>Contoh alur nyata.</b> Order meminta PRC 2 kantong. Petugas BDRS membuat permintaan
    /// <c>PMI-00000001</c>. PMI mengirim 3 kantong: permintaan menjadi <c>Fulfilled</c> dengan sisa
    /// 0, ketiga kantong tercatat <c>Received</c>, kantong ketiga ditandai berlebih, dan balasan
    /// membawa peringatan <c>VAL-BD-014</c> — tetap <c>200</c>, karena kelebihan tidak ditolak.
    /// </para>
    ///
    /// <para>
    /// <b>Penutupan administratif (<c>ClosedEncounter</c>) tidak punya endpoint</b>, dan itu
    /// disengaja: ia dipicu sistem dari sinyal berakhirnya kunjungan (<c>DEC-BD-020</c>).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/provider-requests")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Provider Request",
        AreaName = "HealthServices",
        ControllerName = "BloodProviderRequest",
        Description = "Mencatat permintaan darah ke PMI dan penerimaan kantong",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Provider Request")]
    public class BbkProviderRequestController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.ProviderRequest";
        private const string NotFoundMessage = "Permintaan darah ke PMI tidak ditemukan atau sudah dihapus.";

        private readonly BbkProviderRequestService _providerRequestService;
        private readonly LoggerService _loggerService;

        public BbkProviderRequestController(
            BbkProviderRequestService providerRequestService,
            LoggerService loggerService)
        {
            _providerRequestService = providerRequestService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring dan pengurutan daftar permintaan darah ke PMI.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Provider Request", Description = "Melihat konfigurasi penyaring permintaan darah ke PMI", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodProviderRequest", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<ProviderRequestFilterMetadataResponse>.Ok(
                BbkProviderRequestService.BuildFilterMetadata(),
                "Konfigurasi penyaring permintaan darah ke PMI berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah permintaan per status.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Provider Request", Description = "Melihat ringkasan permintaan darah ke PMI", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodProviderRequest", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _providerRequestService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<ProviderRequestSummaryResponse>.Ok(
                summary,
                "Ringkasan permintaan darah ke PMI berhasil diambil."));
        }

        /// <summary>Daftar permintaan darah ke PMI, dengan penyaring dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Provider Request", Description = "Melihat daftar permintaan darah ke PMI", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodProviderRequest", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? bloodOrderId,
            [FromQuery] BbkProviderRequestStatus? requestStatus,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _providerRequestService.GetPagedAsync(
                search,
                patientId,
                bloodOrderId,
                requestStatus,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<ProviderRequestPagedResult>.Ok(
                result,
                "Daftar permintaan darah ke PMI berhasil diambil."));
        }

        /// <summary>Detail permintaan beserta pemenuhan per komponen, riwayat penerimaan, dan riwayat status.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Provider Request", Description = "Melihat detail permintaan darah ke PMI", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodProviderRequest", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var detail = await _providerRequestService.GetDetailAsync(id, cancellationToken);

            if (detail == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<ProviderRequestDetailDto>.Ok(
                detail,
                "Detail permintaan darah ke PMI berhasil diambil."));
        }

        /// <summary>Riwayat perpindahan status permintaan, terlama lebih dulu.</summary>
        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodBankTransitionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Provider Request", Description = "Melihat riwayat status permintaan darah ke PMI", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodProviderRequest", "Read")]
        public async Task<IActionResult> GetStatusHistory(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var history = await _providerRequestService.GetStatusHistoryAsync(id, cancellationToken);

            if (history == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, NotFoundMessage));

            return Ok(ApiResponse<List<BloodBankTransitionDto>>.Ok(
                history,
                "Riwayat status permintaan darah ke PMI berhasil diambil."));
        }

        /// <summary>Membuat permintaan darah ke PMI atas nama satu pasien, dari satu order aktif.</summary>
        /// <remarks>
        /// Satu order hanya boleh punya satu permintaan yang masih berjalan (<c>VAL-BD-006</c>).
        /// Jumlah yang diminta diturunkan dari baris order, dan nomornya diterbitkan backend.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Provider Request", Description = "Membuat permintaan darah ke PMI", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodProviderRequest", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProviderRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _providerRequestService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != ProviderRequestOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodProviderRequest.Create",
                "Membuat permintaan darah ke PMI.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.RequestNumber,
                    result.Entity.BloodOrderId,
                    Controller = "BloodProviderRequest",
                    Action = "Create"
                });

            return Ok(ApiResponse<ProviderRequestDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, cancellationToken),
                result.Message));
        }

        /// <summary>Mencatat penerimaan kantong dari PMI, termasuk kantong yang berlebih.</summary>
        /// <remarks>
        /// <para>
        /// <b>Kelebihan tidak pernah ditolak</b> (<c>DEC-BD-025</c>). Kantong yang melebihi jumlah
        /// diminta untuk komponennya tetap tercatat dan ditandai berlebih; balasannya <c>200</c>
        /// dengan peringatan <c>VAL-BD-014</c>. Sisa permintaan berhenti di nol.
        /// </para>
        /// <para>
        /// Petugas penerima diturunkan dari pengguna terautentikasi. Nomor kantong PMI tidak pernah
        /// ditulis ke log.
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/receipts")]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Process", "Record Blood Unit Receipt", Description = "Mencatat penerimaan kantong darah dari PMI", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodProviderRequest", "Process")]
        public async Task<IActionResult> RecordReceipt(
            Guid id,
            [FromBody] RecordReceiptRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _providerRequestService.RecordReceiptAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != ProviderRequestOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodProviderRequest.RecordReceipt",
                "Mencatat penerimaan kantong darah dari PMI.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.RequestNumber,
                    ReceivedQuantity = request.Units.Count,
                    result.ExcessUnitCount,
                    Controller = "BloodProviderRequest",
                    Action = "Process"
                });

            return Ok(ApiResponse<ProviderRequestDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, cancellationToken),
                result.Message));
        }

        /// <summary>Membatalkan permintaan dengan alasan terkendali (<c>VAL-BD-016</c>).</summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<ProviderRequestDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Cancel Blood Provider Request", Description = "Membatalkan permintaan darah ke PMI dengan alasan terkendali", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BloodProviderRequest", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelProviderRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _providerRequestService.CancelAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != ProviderRequestOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodProviderRequest.Cancel",
                "Membatalkan permintaan darah ke PMI.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.RequestNumber,
                    request.ReasonCode,
                    Controller = "BloodProviderRequest",
                    Action = "Update"
                });

            return Ok(ApiResponse<ProviderRequestDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, cancellationToken),
                result.Message));
        }

        // =================================================================
        // Penolong
        // =================================================================

        private async Task<ProviderRequestDetailDto> BuildDetailAsync(
            Models.BbkProviderRequest entity,
            CancellationToken cancellationToken)
            => await _providerRequestService.GetDetailAsync(entity.Id, cancellationToken)
               ?? BbkProviderRequestService.ToDetail(entity);

        /// <remarks>
        /// <c>DuplicateRequest</c> → <c>422 VAL-BD-006</c>; <c>DuplicateBagNumber</c> dan
        /// <c>NotAllowedByState</c> → <c>422</c>; <c>VersionConflict</c> → <c>409</c>.
        /// </remarks>
        private IActionResult MapFailure(ProviderRequestResult result)
            => result.Outcome switch
            {
                ProviderRequestOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                ProviderRequestOutcome.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                ProviderRequestOutcome.DuplicateRequest or
                ProviderRequestOutcome.DuplicateBagNumber or
                ProviderRequestOutcome.NotAllowedByState => UnprocessableEntity(
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
