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
    /// Data induk Spesifik Specimen — 1.767 baris dari <c>LAB-EVD-007</c>
    /// (<c>LAB-API-v1</c> <c>r28</c> bagian 23.2 dan 23.3).
    ///
    /// <b>Nol endpoint yang membuat baris dari halaman hasil.</b> <c>LAB-DEC-098</c> butir 5
    /// menegakkan <c>LAB-DEC-040</c>: petugas memakai jalan keluar <c>Lainnya</c>, dan hanya
    /// kepala instalasi yang menaikkannya menjadi nilai tetap lewat <c>POST</c> di sini.
    /// Menyediakan jalan pintas dari halaman hasil akan mengubah tata kelolanya diam-diam.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-specimen-detail-types")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Specimen Detail Type",
        AreaName = "HealthServices",
        ControllerName = "LabSpecimenDetailType",
        Description = "Pengelolaan data induk Spesifik Specimen",
        SortOrder = 20
    )]
    [Tags("Health Services / Laboratory Management / Lab Specimen Detail Type")]
    public class LabSpecimenDetailTypeController : ControllerBase
    {
        private readonly LabSpecimenDetailTypeService _service;

        public LabSpecimenDetailTypeController(LabSpecimenDetailTypeService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Detail Type Summary", Description = "Ringkasan kemajuan penerjemahan Spesifik Specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenDetailType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(result, "Ringkasan Spesifik Specimen berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabSpecimenDetailTypeResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Detail Type", Description = "Melihat daftar Spesifik Specimen", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabSpecimenDetailType", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabSpecimenDetailTypePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabSpecimenDetailTypeResponse>>.Ok(
                result, "Daftar Spesifik Specimen berhasil diambil."));
        }

        // Pilihan untuk layar pengisian. WAJIB disaring per kelompok: daftar 1.601 baris tanpa
        // penyaring bukan bantuan bagi petugas, melainkan hambatan.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<LabSpecimenDetailTypeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Detail Type Options", Description = "Pilihan Spesifik Specimen per jenis specimen", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("LabSpecimenDetailType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid labSpecimenTypeId,
            CancellationToken cancellationToken = default)
        {
            if (labSpecimenTypeId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "Pilih jenis specimen lebih dulu."));
            }

            var result = await _service.GetOptionsAsync(labSpecimenTypeId, cancellationToken);

            return Ok(ApiResponse<List<LabSpecimenDetailTypeOptionResponse>>.Ok(
                result, "Pilihan Spesifik Specimen berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenDetailTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Specimen Detail Type Detail", Description = "Melihat satu Spesifik Specimen", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("LabSpecimenDetailType", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabSpecimenDetailTypeResponse>.Ok(
                    result, "Spesifik Specimen berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenDetailTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Specimen Detail Type", Description = "Menambah Spesifik Specimen", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("LabSpecimenDetailType", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabSpecimenDetailTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.CreateAsync(request, cancellationToken);

                return Ok(ApiResponse<LabSpecimenDetailTypeResponse>.Ok(
                    result, "Spesifik Specimen berhasil ditambahkan."));
            }
            catch (LabSpecimenDetailTypeConflictException exception)
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

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenDetailTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Specimen Detail Type", Description = "Mengubah Spesifik Specimen, termasuk mengisi terjemahannya", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LabSpecimenDetailType", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabSpecimenDetailTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabSpecimenDetailTypeResponse>.Ok(
                    result, "Spesifik Specimen berhasil diubah."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Deactivate Lab Specimen Detail Type", Description = "Menonaktifkan Spesifik Specimen", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("LabSpecimenDetailType", "Delete")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _service.DeactivateAsync(id, cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null!, "Spesifik Specimen berhasil dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
