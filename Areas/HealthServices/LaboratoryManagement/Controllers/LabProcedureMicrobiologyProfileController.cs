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
    /// Pemetaan pemeriksaan katalog yang memakai set bakteri (<c>LAB-API-v1</c> <c>r27</c>
    /// bagian 22.6, <c>LAB-DEC-125</c>).
    ///
    /// Hak tulisnya dipegang <b>kepala instalasi</b> — memetakan pemeriksaan adalah penataan
    /// katalog, dan ia nol mengubah arti satu pun hasil yang sudah ada. Berbeda dari rentang
    /// breakpoint yang dipegang wewenang klinis.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-procedure-microbiology-profiles")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Procedure Microbiology Profile",
        AreaName = "HealthServices",
        ControllerName = "LabProcedureMicrobiologyProfile",
        Description = "Pemetaan pemeriksaan katalog yang memakai set bakteri",
        SortOrder = 19
    )]
    [Tags("Health Services / Laboratory Management / Lab Procedure Microbiology Profile")]
    public class LabProcedureMicrobiologyProfileController : ControllerBase
    {
        private readonly LabProcedureMicrobiologyProfileService _service;

        public LabProcedureMicrobiologyProfileController(LabProcedureMicrobiologyProfileService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabProcedureMicrobiologyProfileResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Procedure Microbiology Profile", Description = "Melihat daftar pemetaan set bakteri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabProcedureMicrobiologyProfile", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabProcedureMicrobiologyProfilePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabProcedureMicrobiologyProfileResponse>>.Ok(
                result, "Daftar profil Mikrobiologi berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabProcedureMicrobiologyProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Procedure Microbiology Profile Detail", Description = "Melihat satu pemetaan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabProcedureMicrobiologyProfile", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabProcedureMicrobiologyProfileResponse>.Ok(
                    result, "Profil Mikrobiologi berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabProcedureMicrobiologyProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Procedure Microbiology Profile", Description = "Memetakan pemeriksaan katalog", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LabProcedureMicrobiologyProfile", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabProcedureMicrobiologyProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.CreateAsync(request, cancellationToken);

                return Ok(ApiResponse<LabProcedureMicrobiologyProfileResponse>.Ok(
                    result, "Profil Mikrobiologi berhasil ditambahkan."));
            }
            catch (LabProcedureMicrobiologyProfileConflictException exception)
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
        [ProducesResponseType(typeof(ApiResponse<LabProcedureMicrobiologyProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Procedure Microbiology Profile", Description = "Mengubah pemetaan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabProcedureMicrobiologyProfile", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabProcedureMicrobiologyProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabProcedureMicrobiologyProfileResponse>.Ok(
                    result, "Profil Mikrobiologi berhasil diubah."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Deactivate Lab Procedure Microbiology Profile", Description = "Menonaktifkan pemetaan", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LabProcedureMicrobiologyProfile", "Delete")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _service.DeactivateAsync(id, cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null!, "Profil Mikrobiologi berhasil dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
