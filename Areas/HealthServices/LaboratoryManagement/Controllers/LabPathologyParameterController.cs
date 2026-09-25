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
    /// Pengelolaan data induk ruas isian laporan Patologi Anatomi (<c>LAB-DEC-086</c>,
    /// <c>r25</c> bagian 20.3).
    ///
    /// Dua audiens dilayani dari base URL yang sama. <c>GET /options</c> melayani layar yang
    /// menyusun keberlakuan: ia hanya mengembalikan parameter aktif, sehingga ruas yang sudah
    /// ditarik tidak dapat dipasang ke formulir baru. <c>GET /</c> melayani kepala instalasi dan
    /// memuat yang nonaktif juga, karena tanpa itu parameter yang dinonaktifkan tidak akan
    /// pernah dapat diaktifkan kembali.
    ///
    /// <b>Tidak ada penghapusan di sini, dan taruhannya lebih besar daripada pada data induk
    /// lain.</b> Nilai laporan menunjuk parameter; parameter yang lenyap membuat isi laporan
    /// diagnostik pasien kehilangan nama. Ia dinonaktifkan lewat <c>PUT /{id}</c>, dan laporan
    /// lama tetap terbaca utuh.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-pathology-parameters")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Pathology Parameter",
        AreaName = "HealthServices",
        ControllerName = "LabPathologyParameter",
        Description = "Pengelolaan data induk ruas isian laporan Patologi Anatomi",
        SortOrder = 12
    )]
    [Tags("Health Services / Laboratory Management / Lab Pathology Master Data")]
    public class LabPathologyParameterController : ControllerBase
    {
        private readonly LabPathologyParameterService _labPathologyParameterService;

        public LabPathologyParameterController(
            LabPathologyParameterService labPathologyParameterService)
        {
            _labPathologyParameterService = labPathologyParameterService;
        }

        // Daftar parameter untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif juga,
        // karena tanpa itu parameter yang dinonaktifkan tidak akan pernah dapat diaktifkan lagi.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabPathologyParameterResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Parameter", Description = "Melihat daftar, ringkasan, pilihan, dan detail ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyParameter", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabPathologyParameterPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labPathologyParameterService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabPathologyParameterResponse>>.Ok(
                result,
                "Daftar ruas isian Patologi Anatomi berhasil diambil."));
        }

        // Daftar pilihan saat menyusun keberlakuan. Hanya parameter aktif yang dikembalikan
        // (VAL-99), dan bentuknya ringan karena dipakai sebagai isi kotak pilihan.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabPathologyParameterOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Parameter", Description = "Melihat daftar, ringkasan, pilihan, dan detail ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyParameter", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] LabPathologyParameterOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labPathologyParameterService.GetOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabPathologyParameterOptionResponse>>.Ok(
                result,
                "Daftar pilihan ruas isian Patologi Anatomi berhasil diambil."));
        }

        // Menambah ruas isian baru. Kode dinormalkan menjadi huruf kapital dan wajib unik
        // (VAL-101). Parameter baru selalu lahir aktif.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Pathology Parameter", Description = "Menambah ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabPathologyParameter", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabPathologyParameterRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labPathologyParameterService.CreateAsync(request, cancellationToken),
                "Ruas isian Patologi Anatomi berhasil ditambahkan.");

        // Mengubah label, urutan tampil, dan status aktif.
        //
        // Kode parameter tidak ikut berubah: ia dirujuk seeder dan keberlakuan yang sudah
        // tersimpan. Penonaktifan berjalan lewat jalur ini karena kontrak r25 bagian 20.3
        // menyediakan tepat empat jalur bagi resource ini — nol DELETE, nol jalur aktivasi.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Pathology Parameter", Description = "Mengubah ruas isian laporan Patologi Anatomi beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyParameter", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabPathologyParameterRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labPathologyParameterService.UpdateAsync(id, request, cancellationToken),
                "Ruas isian Patologi Anatomi berhasil diubah.");

        // =============================================================
        // Baseline data induk yang dilengkapi 2026-09-23 — `LAB-API-v1` r32.
        // =============================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Parameter", Description = "Melihat daftar, ringkasan, pilihan, dan detail ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyParameter", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabPathologyParameter();

            return Ok(ApiResponse<LabPathologyParameterFilterMetadataResponse>.Ok(
                result, "Metadata penyaring ruas isian Patologi Anatomi berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Parameter", Description = "Melihat daftar, ringkasan, pilihan, dan detail ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyParameter", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _labPathologyParameterService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabPathologyParameterSummaryResponse>.Ok(
                result, "Ringkasan ruas isian Patologi Anatomi berhasil diambil."));
        }

        // Jalur detail. Tanpa ini, formulir ubah yang dibuka lewat tautan langsung atau sesudah
        // halaman disegarkan nol punya cara memuat barisnya — dan gagalnya DIAM.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Pathology Parameter", Description = "Melihat daftar, ringkasan, pilihan, dan detail ruas isian laporan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyParameter", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labPathologyParameterService.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabPathologyParameterResponse>.Ok(
                    result, "Detail ruas isian Patologi Anatomi berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // Penonaktifan, BUKAN penghapusan. Ruas yang ditarik akan mengosongkan laporan lama,
        // sehingga barisnya tetap terlihat beserta penandanya (AC-143).
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyParameterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Pathology Parameter", Description = "Mengubah ruas isian laporan Patologi Anatomi beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyParameter", "Update")]
        public async Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] LabPathologyMasterDataStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labPathologyParameterService.SetStatusAsync(
                    id, request.IsActive, cancellationToken);

                return Ok(ApiResponse<LabPathologyParameterResponse>.Ok(
                    result,
                    request.IsActive
                        ? "Ruas isian diaktifkan."
                        : "Ruas isian dinonaktifkan."));
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
            Func<Task<LabPathologyParameterResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabPathologyParameterResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabPathologyMasterDataConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabPathologyMasterDataValidationException exception)
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
