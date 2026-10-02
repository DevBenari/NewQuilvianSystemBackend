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
    /// Data induk aturan nilai kritis Mikrobiologi (<c>LAB-API-v1</c> <c>r26</c> bagian 21.6,
    /// <c>LAB-DEC-103</c>).
    ///
    /// <b>Hak tulis ketiga tindakannya dipegang wewenang klinis Mikrobiologi
    /// (<c>DR-LAB-002</c>)</b> — berbeda dari <c>LabOrganism</c> dan <c>LabAntibiotic</c> yang
    /// dipegang kepala instalasi. Menentukan kuman apa yang dipanel adalah penataan
    /// laboratorium; menentukan kombinasi mana yang membahayakan pasien adalah penilaian
    /// klinis.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-microbiology-critical-rules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Microbiology Critical Rule",
        AreaName = "HealthServices",
        ControllerName = "LabMicrobiologyCriticalRule",
        Description = "Pengelolaan aturan nilai kritis Mikrobiologi",
        SortOrder = 21
    )]
    [Tags("Health Services / Laboratory Management / Lab Microbiology Critical Rule")]
    public class LabMicrobiologyCriticalRuleController : ControllerBase
    {
        private readonly LabMicrobiologyCriticalRuleService _service;

        public LabMicrobiologyCriticalRuleController(LabMicrobiologyCriticalRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabMicrobiologyCriticalRuleResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Microbiology Critical Rule", Description = "Melihat daftar aturan nilai kritis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabMicrobiologyCriticalRule", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabMicrobiologyCriticalRulePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabMicrobiologyCriticalRuleResponse>>.Ok(
                result, "Daftar aturan nilai kritis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabMicrobiologyCriticalRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Microbiology Critical Rule Detail", Description = "Melihat satu aturan", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabMicrobiologyCriticalRule", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabMicrobiologyCriticalRuleResponse>.Ok(
                    result, "Aturan nilai kritis berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabMicrobiologyCriticalRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Lab Microbiology Critical Rule", Description = "Menambah aturan nilai kritis", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("LabMicrobiologyCriticalRule", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabMicrobiologyCriticalRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.CreateAsync(request, cancellationToken);

                return Ok(ApiResponse<LabMicrobiologyCriticalRuleResponse>.Ok(
                    result, "Aturan nilai kritis berhasil ditambahkan."));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabMicrobiologyCriticalRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Lab Microbiology Critical Rule", Description = "Mengubah aturan nilai kritis", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabMicrobiologyCriticalRule", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabMicrobiologyCriticalRuleRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabMicrobiologyCriticalRuleResponse>.Ok(
                    result, "Aturan nilai kritis berhasil diubah."));
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
        [AccessAction("Delete", "Deactivate Lab Microbiology Critical Rule", Description = "Menonaktifkan aturan nilai kritis", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LabMicrobiologyCriticalRule", "Delete")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _service.DeactivateAsync(id, cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null!, "Aturan nilai kritis berhasil dinonaktifkan."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }
    }
}
