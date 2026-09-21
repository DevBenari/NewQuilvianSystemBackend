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
    /// Pengelolaan golongan pemeriksaan Patologi Anatomi beserta <b>keberlakuan ruasnya</b>
    /// (<c>LAB-DEC-086</c>, <c>r25</c> bagian 20.3).
    ///
    /// Keberlakuan hidup sebagai sub-jalur di sini, bukan sebagai resource tersendiri: ia adalah
    /// cara sebuah golongan dipakai, bukan benda yang berdiri sendiri. Hak aksesnya pun ikut —
    /// menyusun keberlakuan menuntut <c>LabPathologyCategory : Update</c>.
    ///
    /// <b>Sub-jalur itulah yang membentuk formulir laporan.</b> Layar hasil Patologi Anatomi nol
    /// menebak ruas apa yang harus ditampilkannya; ia membacanya dari sini. Golongan yang nol
    /// punya keberlakuan menghasilkan formulir kosong, dan itulah sebab jumlah ruas ikut
    /// ditampilkan pada daftar golongan.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-pathology-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Pathology Category",
        AreaName = "HealthServices",
        ControllerName = "LabPathologyCategory",
        Description = "Pengelolaan golongan pemeriksaan Patologi Anatomi dan keberlakuan ruasnya",
        SortOrder = 13
    )]
    [Tags("Health Services / Laboratory Management / Lab Pathology Master Data")]
    public class LabPathologyCategoryController : ControllerBase
    {
        private readonly LabPathologyCategoryService _labPathologyCategoryService;

        public LabPathologyCategoryController(
            LabPathologyCategoryService labPathologyCategoryService)
        {
            _labPathologyCategoryService = labPathologyCategoryService;
        }

        // Daftar golongan untuk layar pengelolaan. Membawa jumlah ruas yang berlaku, karena
        // nilai 0 adalah keadaan berbahaya yang tidak terlihat dari mana pun: golongan tanpa
        // ruas menghasilkan formulir kosong bagi patolog.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabPathologyCategoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Category", Description = "Melihat daftar golongan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyCategory", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabPathologyCategoryPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labPathologyCategoryService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabPathologyCategoryResponse>>.Ok(
                result,
                "Daftar golongan Patologi Anatomi berhasil diambil."));
        }

        // Daftar pilihan golongan. Hanya golongan aktif yang dikembalikan, sehingga penggolongan
        // baru tidak dapat menunjuk golongan yang sudah ditarik dari peredaran.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabPathologyCategoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Pathology Category", Description = "Melihat daftar pilihan golongan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyCategory", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] LabPathologyCategoryOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labPathologyCategoryService.GetOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabPathologyCategoryOptionResponse>>.Ok(
                result,
                "Daftar pilihan golongan Patologi Anatomi berhasil diambil."));
        }

        // Ruas apa saja yang berlaku bagi satu golongan, berurut sesuai tampilnya pada formulir.
        //
        // Keberlakuan yang menunjuk parameter nonaktif tetap terbawa beserta penandanya;
        // menyembunyikannya akan membuat ruas lenyap dari formulir tanpa satu pun layar yang
        // dapat menjelaskan sebabnya.
        [HttpGet("{id:guid}/parameters")]
        [ProducesResponseType(typeof(ApiResponse<List<LabPathologyCategoryParameterResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Pathology Category", Description = "Melihat ruas isian yang berlaku bagi satu golongan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyCategory", "Read")]
        public async Task<IActionResult> GetCategoryParameters(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labPathologyCategoryService
                    .GetCategoryParametersAsync(id, cancellationToken);

                return Ok(ApiResponse<List<LabPathologyCategoryParameterResponse>>.Ok(
                    result,
                    "Ruas isian golongan Patologi Anatomi berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // Mengganti SELURUH daftar keberlakuan sebuah golongan sekaligus.
        //
        // Yang disusun kepala instalasi adalah bentuk formulir — satu benda utuh, bukan kumpulan
        // baris lepas. Baris yang hilang dari permintaan ditandai terhapus, bukan dibuang,
        // sehingga pasangan yang dicabut hari ini dapat dipasang kembali besok.
        //
        // Parameter nonaktif hanya ditolak bila ia BARU; pasangan lama yang menunjuk parameter
        // yang belakangan dinonaktifkan tetap dipertahankan (INV-37).
        [HttpPut("{id:guid}/parameters")]
        [ProducesResponseType(typeof(ApiResponse<List<LabPathologyCategoryParameterResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Pathology Category", Description = "Menyusun ruas isian yang berlaku bagi satu golongan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyCategory", "Update")]
        public async Task<IActionResult> ReplaceCategoryParameters(
            Guid id,
            [FromBody] ReplaceLabPathologyCategoryParametersRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labPathologyCategoryService
                    .ReplaceCategoryParametersAsync(id, request, cancellationToken);

                return Ok(ApiResponse<List<LabPathologyCategoryParameterResponse>>.Ok(
                    result,
                    "Ruas isian golongan Patologi Anatomi berhasil disusun."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabPathologyMasterDataValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        // Menambah golongan baru. Kode dinormalkan menjadi huruf kapital dan wajib unik
        // (VAL-101). Golongan baru lahir aktif dan tanpa satu pun ruas.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Pathology Category", Description = "Menambah golongan Patologi Anatomi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabPathologyCategory", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabPathologyCategoryRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labPathologyCategoryService.CreateAsync(request, cancellationToken),
                "Golongan Patologi Anatomi berhasil ditambahkan.");

        // Mengubah nama, urutan tampil, dan status aktif. Kode golongan tidak ikut berubah:
        // ia dirujuk seeder dan pemetaan jenis pemeriksaan yang sudah tersimpan.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabPathologyCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Pathology Category", Description = "Mengubah golongan Patologi Anatomi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyCategory", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabPathologyCategoryRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labPathologyCategoryService.UpdateAsync(id, request, cancellationToken),
                "Golongan Patologi Anatomi berhasil diubah.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabPathologyCategoryResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabPathologyCategoryResponse>.Ok(result, successMessage));
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
