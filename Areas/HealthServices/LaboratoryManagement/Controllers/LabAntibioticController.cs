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
    /// Pengelolaan panel uji kepekaan antibiotik (<c>LAB-DEC-084</c>, <c>r24</c> bagian 19.4).
    ///
    /// <b>Ini panel uji laboratorium, BUKAN formularium obat.</b> Kesamaan namanya dengan data
    /// induk farmasi menyesatkan, dan penautan keduanya adalah keputusan tersendiri yang
    /// <b>belum</b> diambil — nol boleh disimpulkan dari kemiripan nama.
    ///
    /// Pembagian audiens sama dengan <c>LabOrganism</c>: <c>GET /options</c> hanya mengembalikan
    /// antibiotik aktif bagi analis yang mencatat kepekaan (<c>VAL-86</c>), sedangkan <c>GET /</c>
    /// memuat yang nonaktif juga bagi kepala instalasi.
    ///
    /// <b>Tidak ada penghapusan.</b> Baris kepekaan yang sudah tercatat menunjuk ke sini.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-antibiotics")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Antibiotic",
        AreaName = "HealthServices",
        ControllerName = "LabAntibiotic",
        Description = "Pengelolaan panel uji kepekaan antibiotik laboratorium",
        SortOrder = 17
    )]
    [Tags("Health Services / Laboratory Management / Lab Antibiotic")]
    public class LabAntibioticController : ControllerBase
    {
        private readonly LabAntibioticService _labAntibioticService;

        public LabAntibioticController(LabAntibioticService labAntibioticService)
        {
            _labAntibioticService = labAntibioticService;
        }

        // Daftar antibiotik untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif juga.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabAntibioticResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Antibiotic", Description = "Melihat daftar, ringkasan, pilihan, dan detail antibiotik pada panel uji", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabAntibiotic", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabAntibioticPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labAntibioticService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabAntibioticResponse>>.Ok(
                result, "Daftar antibiotik berhasil diambil."));
        }

        // Daftar pilihan untuk layar pencatatan kepekaan. Hanya antibiotik aktif (VAL-86).
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabAntibioticOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Antibiotic", Description = "Melihat daftar, ringkasan, pilihan, dan detail antibiotik pada panel uji", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabAntibiotic", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] LabAntibioticOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labAntibioticService.GetOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabAntibioticOptionResponse>>.Ok(
                result, "Daftar pilihan antibiotik berhasil diambil."));
        }

        // Menambah antibiotik ke panel uji. Kode dinormalkan dan wajib unik (VAL-91).
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Antibiotic", Description = "Menambah antibiotik ke panel uji", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabAntibiotic", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabAntibioticRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labAntibioticService.CreateAsync(request, cancellationToken),
                "Antibiotik berhasil ditambahkan.");

        // Mengubah nama, keterangan, urutan, dan status aktif. Kode tidak ikut berubah.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Lab Antibiotic", Description = "Mengubah antibiotik pada panel uji beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabAntibiotic", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabAntibioticRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labAntibioticService.UpdateAsync(id, request, cancellationToken),
                "Antibiotik berhasil diubah.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        // =============================================================
        // Baseline data induk yang dilengkapi 2026-09-22 — `LAB-API-v1` r31.
        // =============================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Antibiotic", Description = "Melihat daftar, ringkasan, pilihan, dan detail antibiotik pada panel uji", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabAntibiotic", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabAntibiotic();

            return Ok(ApiResponse<LabAntibioticFilterMetadataResponse>.Ok(
                result, "Metadata penyaring antibiotik pada panel uji berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Antibiotic", Description = "Melihat daftar, ringkasan, pilihan, dan detail antibiotik pada panel uji", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabAntibiotic", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _labAntibioticService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabAntibioticSummaryResponse>.Ok(
                result, "Ringkasan antibiotik pada panel uji berhasil diambil."));
        }

        // Jalur detail. Tanpa ini, formulir ubah yang dibuka lewat tautan langsung atau sesudah
        // halaman disegarkan nol punya cara memuat barisnya — dan gagalnya DIAM.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Antibiotic", Description = "Melihat daftar, ringkasan, pilihan, dan detail antibiotik pada panel uji", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabAntibiotic", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labAntibioticService.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabAntibioticResponse>.Ok(
                    result, "Detail antibiotik pada panel uji berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // Penonaktifan, BUKAN penghapusan. Barisnya tetap terlihat beserta penandanya (AC-118).
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<LabAntibioticResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Antibiotic", Description = "Mengubah antibiotik pada panel uji beserta status aktifnya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabAntibiotic", "Update")]
        public async Task<IActionResult> SetStatus(
            Guid id,
            [FromBody] LabMicrobiologyMasterDataStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labAntibioticService.SetStatusAsync(id, request.IsActive, cancellationToken);

                return Ok(ApiResponse<LabAntibioticResponse>.Ok(
                    result,
                    request.IsActive ? "Antibiotik diaktifkan." : "Antibiotik dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabAntibioticResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabAntibioticResponse>.Ok(result, successMessage));
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
