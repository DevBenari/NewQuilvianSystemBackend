using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodBankReasonPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BloodBankReasonResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengelola daftar alasan terkendali Bank Darah. Petugas BDRS menyusun pilihan alasan
    /// yang boleh dipakai saat membatalkan order, mengalihkan kantong, menetapkan kantong tidak
    /// layak, menempuh jalur darurat, dan tindakan berjejak lainnya.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa daftar ini ada.</b> <c>INV-BD-016</c> melarang alasan berupa teks bebas semata.
    /// Kotak teks bebas menghasilkan jawaban yang tidak dapat dikelompokkan, sehingga tinjauan
    /// berubah menjadi membaca ratusan kalimat satu per satu. Dengan daftar terkendali, alasan
    /// menjadi dapat dihitung dan dibandingkan.
    ///
    /// <b>Kategori bukan sekadar pengelompokan.</b> Pada pembatalan order, kategori alasan adalah
    /// satu-satunya yang membedakan pembatalan klinis dari pembatalan operasional — keduanya
    /// memakai butir hak akses yang sama (<c>DEC-BD-044</c>).
    ///
    /// <b>Contoh.</b> Dokter membatalkan order darah Ny. R karena operasinya ditunda, lalu memilih
    /// alasan berkategori pembatalan klinis. Di hari yang sama petugas BDRS menghapus satu order
    /// ganda yang ia buat sendiri lewat jalur manual, memilih alasan berkategori pembatalan
    /// operasional. Keduanya tercatat sebagai pembatalan, tetapi peninjau dapat membedakannya
    /// tanpa membaca kalimat satu per satu.
    ///
    /// <b>Kategori yang kosong menghentikan tindakannya.</b> Bila sebuah kategori tidak punya satu
    /// pun alasan aktif, petugas membuka kotak pilihan dan menemukannya kosong, sementara aturan
    /// menuntut alasan terkendali. Ringkasan di halaman index menandai keadaan itu.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/blood-bank-reasons")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Blood Bank Reason",
        AreaName = "HealthServices",
        ControllerName = "BloodBankReason",
        Description = "Mengelola daftar alasan terkendali Bank Darah",
        SortOrder = 45
    )]
    [Tags("Health Services / Master Data / Blood Bank Reason")]
    public class BloodBankReasonController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.BloodBank";

        private readonly BloodBankReasonService _reasonService;
        private readonly LoggerService _loggerService;

        public BloodBankReasonController(
            BloodBankReasonService reasonService,
            LoggerService loggerService)
        {
            _reasonService = reasonService;
            _loggerService = loggerService;
        }

        /// <summary>Konfigurasi penyaring, pilihan kategori, dan isian form.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Bank Reason", Description = "Melihat konfigurasi penyaring daftar alasan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankReason", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodBankReasonFilterMetadataResponse>.Ok(
                BloodBankReasonService.BuildFilterMetadata(),
                "Konfigurasi penyaring alasan Bank Darah berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah alasan, termasuk kategori yang belum terisi.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Bank Reason", Description = "Melihat ringkasan daftar alasan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankReason", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _reasonService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<BloodBankReasonSummaryResponse>.Ok(
                summary,
                summary.CategoryWithoutActiveReasonCount > 0
                    ? $"Ringkasan berhasil diambil. {summary.CategoryWithoutActiveReasonCount} kategori belum punya satu pun alasan aktif, sehingga tindakan yang memerlukannya belum dapat diselesaikan petugas."
                    : "Ringkasan alasan Bank Darah berhasil diambil."));
        }

        /// <summary>Daftar alasan, dengan pencarian, penyaringan kategori, dan halaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Bank Reason", Description = "Melihat daftar alasan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankReason", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? reasonCategory,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.GetPagedAsync(
                search,
                isActive,
                reasonCategory,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodBankReasonPagedResult>.Ok(
                result,
                "Daftar alasan Bank Darah berhasil diambil."));
        }

        /// <summary>Pilihan alasan aktif untuk sebuah kategori.</summary>
        /// <remarks>
        /// Dipanggil layar tindakan dengan <c>?category=</c> supaya hanya alasan yang sesuai
        /// konteks yang ditawarkan. Layar pembatalan order tidak boleh menawarkan alasan
        /// pengembalian ke PMI, dan sebaliknya.
        ///
        /// Kategori yang tidak dikenal memulangkan daftar kosong, **bukan** seluruh isi tabel —
        /// memulangkan semuanya justru menawarkan alasan yang salah konteks kepada petugas.
        /// </remarks>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodBankReasonOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Bank Reason", Description = "Melihat pilihan alasan Bank Darah per kategori", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankReason", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? category,
            [FromQuery] string? search,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.GetOptionsAsync(category, search, cancellationToken);

            return Ok(ApiResponse<List<BloodBankReasonOptionResponse>>.Ok(
                result,
                result.Count == 0
                    ? "Belum ada alasan aktif untuk kategori ini. Selama daftar ini kosong, tindakan yang menuntut alasan terkendali tidak dapat diselesaikan."
                    : "Pilihan alasan Bank Darah berhasil diambil."));
        }

        /// <summary>Detail satu alasan.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Bank Reason", Description = "Melihat detail alasan Bank Darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodBankReason", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _reasonService.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Alasan tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodBankReasonResponse>.Ok(
                BloodBankReasonService.ToResponse(entity),
                "Detail alasan Bank Darah berhasil diambil."));
        }

        /// <summary>Menambah alasan terkendali baru.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Blood Bank Reason", Description = "Menambah alasan Bank Darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodBankReason", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodBankReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodBankReasonStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodBankReason.Create",
                "Menambah alasan Bank Darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.ReasonCode,
                    result.Entity.ReasonCategory,
                    Controller = "BloodBankReason",
                    Action = "Create"
                });

            return Ok(ApiResponse<BloodBankReasonResponse>.Ok(
                BloodBankReasonService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengubah kode, teks, kategori, dan status alasan.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Blood Bank Reason", Description = "Mengubah alasan Bank Darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodBankReason", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBloodBankReasonRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.UpdateAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodBankReasonStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodBankReason.Update",
                "Mengubah alasan Bank Darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.ReasonCode,
                    result.Entity.ReasonCategory,
                    Controller = "BloodBankReason",
                    Action = "Update"
                });

            return Ok(ApiResponse<BloodBankReasonResponse>.Ok(
                BloodBankReasonService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Mengaktifkan atau menonaktifkan alasan.</summary>
        /// <remarks>
        /// Menonaktifkan alasan TIDAK menyentuh satu pun rekam tindakan yang sudah memakainya,
        /// karena rekam menyimpan salinan teks alasannya sendiri. Yang berubah hanya ke depan.
        /// </remarks>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<BloodBankReasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Blood Bank Reason Status", Description = "Mengubah status aktif alasan Bank Darah", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodBankReason", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateBloodBankReasonStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.UpdateStatusAsync(
                id,
                request.IsActive,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodBankReasonStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodBankReason.UpdateStatus",
                "Mengubah status alasan Bank Darah.",
                new
                {
                    EntityId = id,
                    request.IsActive,
                    Controller = "BloodBankReason",
                    Action = "UpdateStatus"
                });

            return Ok(ApiResponse<BloodBankReasonResponse>.Ok(
                BloodBankReasonService.ToResponse(result.Entity!),
                result.Message));
        }

        /// <summary>Menandai alasan terhapus. Tidak pernah menghapus baris fisik.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Blood Bank Reason", Description = "Menghapus alasan Bank Darah", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("BloodBankReason", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _reasonService.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != BloodBankReasonStatus.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodBankReason.Delete",
                "Menghapus alasan Bank Darah.",
                new
                {
                    EntityId = id,
                    result.Entity!.ReasonCode,
                    Controller = "BloodBankReason",
                    Action = "Delete"
                });

            return Ok(ApiResponse<bool>.Ok(true, result.Message));
        }

        private IActionResult MapFailure(BloodBankReasonResult result)
            => result.Status switch
            {
                BloodBankReasonStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),
                BloodBankReasonStatus.DuplicateCode => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),
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
