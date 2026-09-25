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
    /// Pengelolaan data induk organisme Mikrobiologi (<c>LAB-DEC-084</c>, <c>r24</c> bagian
    /// 19.4).
    ///
    /// Dua audiens dilayani dari base URL yang sama. <c>GET /options</c> melayani analis yang
    /// mencatat isolat: ia <b>hanya</b> mengembalikan organisme aktif, sehingga kuman yang sudah
    /// ditarik dari daftar nol dapat dipilih (<c>VAL-85</c>). <c>GET /</c> melayani kepala
    /// instalasi dan memuat yang nonaktif juga, karena tanpa itu baris yang dinonaktifkan nol
    /// akan pernah dapat diaktifkan kembali.
    ///
    /// <b>Tidak ada penghapusan di sini, dan alasannya klinis.</b> Isolat yang sudah tercatat
    /// menunjuk ke baris ini; menghapusnya berarti menghapus temuan pasien. Penonaktifan lewat
    /// <c>PUT /{id}</c>.
    ///
    /// <b>Kelompok endpoint ini bagian dari definisi selesai, bukan pelengkap yang boleh
    /// menyusul.</b> <c>LAB-COORD-006</c> dan <c>MST-POS-WRITE</c> sudah dua kali membuktikan
    /// tabel data induk tanpa jalur tulis adalah kegagalan — satu masih terbuka sampai hari ini,
    /// satu lagi harus ditambal lewat SQL langsung ke basis data.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-organisms")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Organism",
        AreaName = "HealthServices",
        ControllerName = "LabOrganism",
        Description = "Pengelolaan data induk organisme Mikrobiologi",
        SortOrder = 16
    )]
    [Tags("Health Services / Laboratory Management / Lab Organism")]
    public class LabOrganismController : ControllerBase
    {
        private readonly LabOrganismService _labOrganismService;

        public LabOrganismController(LabOrganismService labOrganismService)
        {
            _labOrganismService = labOrganismService;
        }

        // Daftar organisme untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif juga.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabOrganismResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Organism", Description = "Melihat daftar, ringkasan, pilihan, dan detail organisme Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrganism", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabOrganismPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labOrganismService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabOrganismResponse>>.Ok(
                result, "Daftar organisme berhasil diambil."));
        }

        // Daftar pilihan untuk layar pencatatan isolat. Hanya organisme aktif (VAL-85), dan
        // bentuknya ringan karena dipakai sebagai isi kotak pilihan.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabOrganismOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Organism", Description = "Melihat daftar, ringkasan, pilihan, dan detail organisme Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrganism", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] LabOrganismOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labOrganismService.GetOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabOrganismOptionResponse>>.Ok(
                result, "Daftar pilihan organisme berhasil diambil."));
        }

        // Menambah organisme. Kode dinormalkan menjadi huruf kapital dan wajib unik (VAL-91).
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Organism", Description = "Menambah organisme Mikrobiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabOrganism", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabOrganismRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrganismService.CreateAsync(request, cancellationToken),
                "Organisme berhasil ditambahkan.");

        // Mengubah nama, keterangan, urutan, dan status aktif. Kode tidak ikut berubah: ia
        // penanda yang dirujuk integrasi dan pelaporan.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Lab Organism", Description = "Mengubah organisme Mikrobiologi beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrganism", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabOrganismRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrganismService.UpdateAsync(id, request, cancellationToken),
                "Organisme berhasil diubah.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        // =============================================================
        // Baseline data induk yang dilengkapi 2026-09-22 — `LAB-API-v1` r31.
        // =============================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Organism", Description = "Melihat daftar, ringkasan, pilihan, dan detail organisme Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrganism", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabOrganism();

            return Ok(ApiResponse<LabOrganismFilterMetadataResponse>.Ok(
                result, "Metadata penyaring organisme Mikrobiologi berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Organism", Description = "Melihat daftar, ringkasan, pilihan, dan detail organisme Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrganism", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _labOrganismService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabOrganismSummaryResponse>.Ok(
                result, "Ringkasan organisme Mikrobiologi berhasil diambil."));
        }

        // Jalur detail. Tanpa ini, formulir ubah yang dibuka lewat tautan langsung atau sesudah
        // halaman disegarkan nol punya cara memuat barisnya — dan gagalnya DIAM.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Organism", Description = "Melihat daftar, ringkasan, pilihan, dan detail organisme Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrganism", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrganismService.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabOrganismResponse>.Ok(
                    result, "Detail organisme Mikrobiologi berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // Penonaktifan, BUKAN penghapusan. Barisnya tetap terlihat beserta penandanya (AC-118).
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LabOrganismResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Organism", Description = "Mengubah organisme Mikrobiologi beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrganism", "Update")]
        public async Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] LabMicrobiologyMasterDataStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrganismService.SetStatusAsync(id, request.IsActive, cancellationToken);

                return Ok(ApiResponse<LabOrganismResponse>.Ok(
                    result,
                    request.IsActive ? "Organisme diaktifkan." : "Organisme dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabOrganismResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabOrganismResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabMicrobiologyMasterDataConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
