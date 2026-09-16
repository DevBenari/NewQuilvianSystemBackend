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
    /// <b>Sejak <c>BE-BD-006</c></b> controller ini juga mengalokasikan kantong ke baris kebutuhan
    /// order dan membatalkan alokasi yang keliru, dijaga butir <c>BloodUnit : Allocate</c> —
    /// sengaja terpisah dari <c>Store</c>, karena mengikat kantong pada pasien adalah keputusan
    /// klinis sedangkan menaruhnya ke kulkas adalah pekerjaan gudang. Kedua tindakan itu memakai
    /// butir yang sama: yang membatalkan alokasi keliru adalah petugas yang berwenang
    /// mengalokasikan (<c>DEC-BD-029</c>).
    /// </para>
    /// <para>
    /// Tindakan lain lahir bersama task pemiliknya: bukti kecocokan dan pemberian pada
    /// <c>BE-BD-007</c>, jalur darurat pada <c>BE-BD-008</c>, penyelesaian <c>PendingReview</c> —
    /// termasuk <c>reallocate</c> — pada <c>BE-BD-009</c>, dan koreksi pada <c>BE-BD-010</c>.
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

        /// <summary>Mengalokasikan kantong ke satu baris kebutuhan order.</summary>
        /// <remarks>
        /// Membawa kantong <c>Available</c> → <c>Allocated</c> (<c>DEC-BD-003</c>,
        /// <c>DEC-BD-007</c>). Gagal bila kantong belum disimpan (<c>422 VAL-BD-063</c>), lokasi
        /// penyimpanannya sedang nonaktif (<c>422 VAL-BD-064</c>), kantongnya menunggu keputusan
        /// atau berlebih (<c>422 VAL-BD-033</c>), atau kantong sudah punya alokasi aktif
        /// (<c>409 VAL-BD-018c</c> — termasuk ketika dua petugas mengalokasikan kantong yang sama
        /// pada saat yang sama; tepat satu yang berhasil).
        /// </remarks>
        [HttpPost("{id:guid}/allocate")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Allocate", "Allocate Blood Unit", Description = "Mengalokasikan kantong darah ke baris kebutuhan order dan membatalkan alokasi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodUnit", "Allocate")]
        public async Task<IActionResult> Allocate(
            Guid id,
            [FromBody] AllocateUnitRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.AllocateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogAllocationAsync(
                "BloodUnit.Allocate",
                "Mengalokasikan kantong darah ke baris kebutuhan order.",
                result,
                request.BloodOrderLineId,
                reasonCode: null);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                await _bloodUnitService.GetDetailAsync(id, cancellationToken),
                result.Message));
        }

        /// <summary>Membatalkan alokasi kantong yang keliru, sebelum kantong diberikan.</summary>
        /// <remarks>
        /// Kantong kembali <c>Available</c> bila order asalnya masih berjalan, dan masuk
        /// <c>PendingReview</c> bila order atau kunjungan asalnya sudah berakhir
        /// (<c>DEC-BD-029</c>, <c>DEC-BD-014</c>). Alasan <b>wajib</b> dipilih dari daftar
        /// terkendali berkategori pembatalan alokasi (<c>400 VAL-BD-016</c>). Kantong yang sudah
        /// diberikan tidak dapat dibatalkan (<c>422 VAL-BD-023</c>) — jalurnya catatan koreksi.
        /// </remarks>
        [HttpPost("{id:guid}/cancel-allocation")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Allocate", "Allocate Blood Unit", Description = "Mengalokasikan kantong darah ke baris kebutuhan order dan membatalkan alokasi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodUnit", "Allocate")]
        public async Task<IActionResult> CancelAllocation(
            Guid id,
            [FromBody] CancelWithReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.CancelAllocationAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogAllocationAsync(
                "BloodUnit.CancelAllocation",
                "Membatalkan alokasi kantong darah.",
                result,
                bloodOrderLineId: null,
                reasonCode: request.ReasonCode);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                await _bloodUnitService.GetDetailAsync(id, cancellationToken),
                result.Message));
        }

        /// <summary>Mencatat hasil pemeriksaan kecocokan terhadap pasien tujuan alokasi aktif.</summary>
        /// <remarks>
        /// Hanya petugas dengan <c>BloodUnit : Compatibility</c> yang dapat menjalankan tindakan ini.
        /// Hasil <c>Incompatible</c> tetap disimpan sebagai rekam klinis, tetapi tidak membuka gerbang
        /// pemberian normal.
        /// </remarks>
        [HttpPost("{id:guid}/compatibility-evidence")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Compatibility", "Validate Blood Unit Compatibility", Description = "Mencatat hasil pemeriksaan kecocokan kantong darah terhadap pasien tujuan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(
            "BloodUnit",
            "Compatibility",
            DeniedCode = "VAL-BD-078",
            DeniedMessage =
                "Hanya petugas Bank Darah dengan kewenangan validasi yang boleh " +
                "menyatakan hasil pemeriksaan kecocokan.")]
        public async Task<IActionResult> CompatibilityEvidence(
            Guid id,
            [FromBody] RecordEvidenceRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.RecordCompatibilityEvidenceAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogBloodUnitActionAsync(
                "BloodUnit.CompatibilityEvidence",
                "Mencatat hasil pemeriksaan kecocokan kantong darah.",
                result,
                "Compatibility");

            var detail = await _bloodUnitService.GetDetailAsync(id, cancellationToken);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                detail,
                result.Message));
        }

        /// <summary>Memberikan kantong kepada pasien melalui jalur normal.</summary>
        /// <remarks>
        /// Gerbang pemberian dinilai ulang saat tindakan dilakukan. Kantong harus masih memiliki
        /// alokasi aktif, berada di lokasi aktif, dan memiliki bukti kecocokan yang valid untuk
        /// pasien tujuan. Pemberian bersifat terminal.
        /// </remarks>
        [HttpPost("{id:guid}/issue")]
        [ProducesResponseType(typeof(ApiResponse<BloodUnitDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Issue", "Issue Blood Unit", Description = "Memberikan kantong darah kepada pasien melalui jalur normal", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("BloodUnit", "Issue")]
        public async Task<IActionResult> Issue(
            Guid id,
            [FromBody] IssueUnitRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodUnitService.IssueAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodUnitOutcome.Success)
                return MapFailure(result);

            await LogBloodUnitActionAsync(
                "BloodUnit.Issue",
                "Memberikan kantong darah melalui jalur normal.",
                result,
                "Issue");

            var detail = await _bloodUnitService.GetDetailAsync(id, cancellationToken);

            return Ok(ApiResponse<BloodUnitDetailDto>.Ok(
                detail,
                result.Message));
        }

        /// <remarks>
        /// Log tindakan klinis sengaja hanya menyimpan identifier internal kantong dan status.
        /// Nomor kantong PMI, nama pasien, nomor rekam medis, dan PatientId tidak ditulis ke log.
        /// </remarks>
        private Task LogBloodUnitActionAsync(
            string eventName,
            string message,
            BloodUnitResult result,
            string action)
            => _loggerService.InfoAsync(
                LogCategory,
                eventName,
                message,
                new
                {
                    EntityId = result.Entity!.Id,
                    UnitStatus = result.Entity.UnitStatus.ToString(),
                    Controller = "BloodUnit",
                    Action = action
                });
        /// <remarks>
        /// Nomor kantong PMI dan nama pasien sensitif, dan sengaja tidak ditulis ke log. Kode
        /// alasan ditulis karena ia kode terkendali, bukan teks bebas berisi keterangan pasien.
        /// </remarks>
        private Task LogAllocationAsync(
            string eventName,
            string message,
            BloodUnitResult result,
            Guid? bloodOrderLineId,
            string? reasonCode)
            => _loggerService.InfoAsync(
                LogCategory,
                eventName,
                message,
                new
                {
                    EntityId = result.Entity!.Id,
                    BloodOrderLineId = bloodOrderLineId,
                    ReasonCode = reasonCode,
                    UnitStatus = result.Entity.UnitStatus.ToString(),
                    Controller = "BloodUnit",
                    Action = "Allocate"
                });

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
        /// <para>
        /// <c>Invalid</c> → <c>400</c> (<c>VAL-BD-016</c>); <c>NotFound</c> → <c>404</c>;
        /// <c>VersionConflict</c> dan <c>AllocationConflict</c> → <c>409</c>
        /// (<c>VAL-BD-018c</c>); <c>NotAllowedByState</c> → <c>422</c>
        /// (<c>VAL-BD-023/033/060/061/062/063/064</c>, master kosong).
        /// </para>
        /// <para>
        /// <b>Seluruh pemetaan berhenti pada pesan bisnis.</b> Tidak ada
        /// <c>DbUpdateException</c>, <c>PostgresException</c>, nama constraint, SQL, connection
        /// string, maupun stack trace yang sampai ke pengguna — penerjemahannya selesai di
        /// service.
        /// </para>
        /// </remarks>
        private IActionResult MapFailure(BloodUnitResult result)
        {
            // Slot errors hanya terisi ketika penolakannya memang bernomor pada
            // validation-matrix. Bila tidak, nilainya tetap null persis seperti sebelumnya,
            // sehingga seluruh endpoint kantong selain pemberian tidak berubah bentuk.
            object? errors =
                result.ValidationCode is null
                    ? null
                    : new { Code = result.ValidationCode };

            return result.Outcome switch
            {
                BloodUnitOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message, errors)),

                BloodUnitOutcome.VersionConflict or BloodUnitOutcome.AllocationConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message, errors)),

                BloodUnitOutcome.NotAllowedByState => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message, errors)),

                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message, errors))
            };
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
