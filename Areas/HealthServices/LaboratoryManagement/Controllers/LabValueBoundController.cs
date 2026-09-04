using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Pengelolaan batas nilai rujukan laboratorium.
    ///
    /// Enam endpoint di sini melayani kepala instalasi laboratorium. Batas normal, satuan,
    /// daftar pilihan, dan batas waktu cito dapat diubah langsung lewat sini. Batas kritis
    /// tidak — ia hanya berubah lewat pengajuan yang disetujui pihak klinis, dan upaya
    /// mengubahnya lewat jalur biasa ditolak <c>422</c> (<c>VAL-28</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-value-bounds")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Value Bound",
        AreaName = "HealthServices",
        ControllerName = "LabValueBound",
        Description = "Pengelolaan batas nilai rujukan laboratorium",
        SortOrder = 3
    )]
    [Tags("Health Services / Laboratory Management / Lab Value Bound")]
    public class LabValueBoundController : ControllerBase
    {
        private readonly LabValueBoundService _labValueBoundService;

        public LabValueBoundController(LabValueBoundService labValueBoundService)
        {
            _labValueBoundService = labValueBoundService;
        }

        // Keterangan bentuk layar batas nilai: pilihan bentuk hasil, jenis kelamin, urutan,
        // ukuran halaman, dan penanda bahwa batas kritis hanya berubah lewat pengajuan.
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Value Bound", Description = "Melihat daftar pilihan penyaring batas nilai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabValueBound", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _labValueBoundService.GetFilterMetadata();

            return Ok(ApiResponse<LabValueBoundFilterMetadataResponse>.Ok(
                result,
                "Metadata penyaring batas nilai laboratorium berhasil diambil."));
        }

        // Rekap batas nilai. Tanpa rentang waktu — ini data induk, bukan catatan kejadian.
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Value Bound", Description = "Melihat rekap batas nilai laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabValueBound", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _labValueBoundService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabValueBoundSummaryResponse>.Ok(
                result,
                "Rekap batas nilai laboratorium berhasil diambil."));
        }

        // Daftar batas nilai, dapat disaring per jenis pemeriksaan.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabValueBoundListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Value Bound", Description = "Melihat daftar batas nilai laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabValueBound", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabValueBoundPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labValueBoundService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabValueBoundListResponse>>.Ok(
                result,
                "Daftar batas nilai laboratorium berhasil diambil."));
        }

        // Detail satu batas nilai beserta daftar pilihannya.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Value Bound", Description = "Melihat detail batas nilai laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabValueBound", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _labValueBoundService.GetDetailAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Batas nilai tidak ditemukan."));
            }

            return Ok(ApiResponse<LabValueBoundDetailResponse>.Ok(
                result,
                "Detail batas nilai laboratorium berhasil diambil."));
        }

        // Membuat batas nilai baru untuk satu kelompok pasien.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Lab Value Bound", Description = "Membuat batas nilai laboratorium", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabValueBound", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabValueBoundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labValueBoundService.CreateAsync(request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    ApiResponse<LabValueBoundDetailResponse>.Ok(
                        result,
                        "Batas nilai laboratorium berhasil dibuat."));
            }
            catch (LabValueBoundConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabValueBoundValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        // Mengubah satuan, batas normal, batas waktu cito, dan daftar pilihan.
        //
        // Batas kritis TIDAK dapat diubah lewat sini. Permintaan yang memuat perubahan batas
        // kritis — termasuk penanda pilihan yang dianggap kritis — ditolak 422 (VAL-28).
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Value Bound", Description = "Mengubah batas nilai laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabValueBound", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabValueBoundRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labValueBoundService.UpdateAsync(id, request, cancellationToken),
                "Batas nilai laboratorium berhasil diubah.");

        // Menonaktifkan batas nilai. Batas aktif terakhir milik sebuah pemeriksaan tidak dapat
        // dinonaktifkan (VAL-30).
        [HttpPut("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<LabValueBoundDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Value Bound", Description = "Menonaktifkan batas nilai laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabValueBound", "Update")]
        public Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labValueBoundService.DeactivateAsync(id, cancellationToken),
                "Batas nilai laboratorium berhasil dinonaktifkan.");

        // Riwayat perubahan sebuah batas nilai, terbaru lebih dulu.
        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<LabValueBoundHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Value Bound", Description = "Melihat riwayat perubahan batas nilai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabValueBound", "Read")]
        public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labValueBoundService.GetHistoryAsync(id, cancellationToken);

                return Ok(ApiResponse<List<LabValueBoundHistoryResponse>>.Ok(
                    result,
                    "Riwayat perubahan batas nilai berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabValueBoundDetailResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabValueBoundDetailResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabValueBoundConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabValueBoundValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
