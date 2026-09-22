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
    /// Data induk rentang breakpoint uji kepekaan (<c>LAB-API-v1</c> <c>r27</c> bagian 22.5).
    ///
    /// <b>Hak tulis ketiga tindakannya dipegang wewenang klinis Mikrobiologi</b>, berbeda dari
    /// <c>LabOrganism</c> dan <c>LabAntibiotic</c> yang dipegang kepala instalasi
    /// (<c>LAB-PERM-v1</c> rev 9 bagian 11.2). Menentukan kuman apa yang dipanel adalah
    /// penataan laboratorium; menentukan angka yang memisahkan <c>Resistant</c> dari
    /// <c>Sensitive</c> adalah penilaian klinis.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-susceptibility-breakpoints")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Susceptibility Breakpoint",
        AreaName = "HealthServices",
        ControllerName = "LabSusceptibilityBreakpoint",
        Description = "Pengelolaan rentang breakpoint uji kepekaan Mikrobiologi",
        SortOrder = 18
    )]
    [Tags("Health Services / Laboratory Management / Lab Susceptibility Breakpoint")]
    public class LabSusceptibilityBreakpointController : ControllerBase
    {
        private readonly LabSusceptibilityBreakpointService _service;

        public LabSusceptibilityBreakpointController(LabSusceptibilityBreakpointService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabSusceptibilityBreakpointResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Susceptibility Breakpoint", Description = "Melihat daftar rentang breakpoint", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSusceptibilityBreakpoint", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabSusceptibilityBreakpointPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabSusceptibilityBreakpointResponse>>.Ok(
                result, "Daftar rentang breakpoint berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSusceptibilityBreakpointResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Susceptibility Breakpoint Detail", Description = "Melihat satu rentang breakpoint", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabSusceptibilityBreakpoint", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabSusceptibilityBreakpointResponse>.Ok(
                    result, "Rentang breakpoint berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabSusceptibilityBreakpointResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Susceptibility Breakpoint", Description = "Menambah rentang breakpoint", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LabSusceptibilityBreakpoint", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabSusceptibilityBreakpointRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.CreateAsync(request, cancellationToken);

                return Ok(ApiResponse<LabSusceptibilityBreakpointResponse>.Ok(
                    result, "Rentang breakpoint berhasil ditambahkan."));
            }
            catch (LabSusceptibilityBreakpointConflictException exception)
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
        [ProducesResponseType(typeof(ApiResponse<LabSusceptibilityBreakpointResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Susceptibility Breakpoint", Description = "Mengubah rentang breakpoint", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabSusceptibilityBreakpoint", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabSusceptibilityBreakpointRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabSusceptibilityBreakpointResponse>.Ok(
                    result, "Rentang breakpoint berhasil diubah."));
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

        // Menonaktifkan, BUKAN menghapus. Baris hasil lama menyimpan snapshot rentangnya, dan
        // riwayat perubahan rentang adalah riwayat keselamatan.
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Deactivate Lab Susceptibility Breakpoint", Description = "Menonaktifkan rentang breakpoint", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LabSusceptibilityBreakpoint", "Delete")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _service.DeactivateAsync(id, cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null!, "Rentang breakpoint berhasil dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
