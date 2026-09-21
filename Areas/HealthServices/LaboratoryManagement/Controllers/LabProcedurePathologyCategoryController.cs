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
    /// Penggolongan jenis pemeriksaan katalog ke golongan Patologi Anatomi
    /// (<c>LAB-DEC-087</c>, <c>r25</c> bagian 20.3).
    ///
    /// <b>Isi tabel di balik controller ini menentukan halaman hasil Patologi Anatomi berisi
    /// atau kosong.</b> Jenis pemeriksaan yang belum digolongkan nol menyumbang ruas isian, dan
    /// <c>INV-39</c> melarang sistem menebak golongannya. Selama pengisian awal belum dikerjakan,
    /// formulir hasil Patologi Anatomi kosong sama sekali — keadaan hari pertama yang sudah
    /// diperingatkan sejak perancangan.
    ///
    /// Hak aksesnya memakai <c>LabPathologyCategory</c>, bukan resource tersendiri: penggolongan
    /// adalah cara sebuah golongan dipakai.
    ///
    /// <b><c>GET /suggestions</c> mengusulkan, tidak menyimpan.</b> Pemisahan itu bukan kerapian
    /// melainkan inti <c>LAB-DEC-087</c> — pencocokan kata kunci tidak tahu apa-apa soal
    /// patologi, dan manusia yang memeriksanya sebelum disimpan.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-procedure-pathology-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Procedure Pathology Category",
        AreaName = "HealthServices",
        ControllerName = "LabProcedurePathologyCategory",
        Description = "Penggolongan jenis pemeriksaan katalog ke golongan Patologi Anatomi",
        SortOrder = 14
    )]
    [Tags("Health Services / Laboratory Management / Lab Pathology Master Data")]
    public class LabProcedurePathologyCategoryController : ControllerBase
    {
        private readonly LabProcedurePathologyCategoryService _labProcedurePathologyCategoryService;

        public LabProcedurePathologyCategoryController(
            LabProcedurePathologyCategoryService labProcedurePathologyCategoryService)
        {
            _labProcedurePathologyCategoryService = labProcedurePathologyCategoryService;
        }

        // Daftar jenis pemeriksaan yang sudah digolongkan, beserta golongannya.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabProcedurePathologyCategoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Procedure Pathology Category", Description = "Melihat daftar penggolongan jenis pemeriksaan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyCategory", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabProcedurePathologyCategoryPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labProcedurePathologyCategoryService
                .GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabProcedurePathologyCategoryResponse>>.Ok(
                result,
                "Daftar penggolongan jenis pemeriksaan berhasil diambil."));
        }

        // Usulan penggolongan bagi jenis pemeriksaan Patologi Anatomi yang BELUM digolongkan.
        //
        // Jalur ini nol menyimpan apa pun: ia mencocokkan enam kata kunci pada nama pemeriksaan
        // lalu menyerahkan hasilnya untuk diperiksa manusia. Pemeriksaan yang nol cocok tetap
        // dikembalikan dengan usulan kosong — justru itu yang paling butuh perhatian manusia.
        //
        // Sesudah pengisian awal selesai, jalur ini boleh tidak dipakai lagi (LAB-DEC-087).
        [HttpGet("suggestions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabProcedurePathologyCategorySuggestionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Procedure Pathology Category", Description = "Melihat usulan penggolongan jenis pemeriksaan Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPathologyCategory", "Read")]
        public async Task<IActionResult> GetSuggestions(
            [FromQuery] LabProcedurePathologyCategorySuggestionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labProcedurePathologyCategoryService
                .GetSuggestionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabProcedurePathologyCategorySuggestionResponse>>.Ok(
                result,
                "Usulan penggolongan jenis pemeriksaan berhasil disusun."));
        }

        // Menggolongkan satu jenis pemeriksaan. Hanya pemeriksaan berdisiplin Patologi Anatomi
        // yang dapat digolongkan, dan satu jenis pemeriksaan tepat satu golongan.
        //
        // Hak aksesnya Update, bukan Create: kontrak r25 bagian 20.3 menetapkan pemetaan berjalan
        // di bawah LabPathologyCategory : Read/Update.
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabProcedurePathologyCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Procedure Pathology Category", Description = "Menggolongkan jenis pemeriksaan ke golongan Patologi Anatomi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyCategory", "Update")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabProcedurePathologyCategoryRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labProcedurePathologyCategoryService.CreateAsync(request, cancellationToken),
                "Jenis pemeriksaan berhasil digolongkan.");

        // Memindahkan satu jenis pemeriksaan ke golongan lain. Jenis pemeriksaannya tidak ikut
        // berubah: memindahkan baris ini ke pemeriksaan lain sama artinya dengan mencabut satu
        // penggolongan dan membuat penggolongan lain.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabProcedurePathologyCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Procedure Pathology Category", Description = "Memindahkan jenis pemeriksaan ke golongan Patologi Anatomi lain", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabPathologyCategory", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabProcedurePathologyCategoryRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labProcedurePathologyCategoryService.UpdateAsync(id, request, cancellationToken),
                "Golongan jenis pemeriksaan berhasil diubah.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabProcedurePathologyCategoryResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabProcedurePathologyCategoryResponse>.Ok(result, successMessage));
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
