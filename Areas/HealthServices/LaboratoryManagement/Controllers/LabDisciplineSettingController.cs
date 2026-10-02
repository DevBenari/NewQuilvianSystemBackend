using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Pengaturan footer cetak per disiplin (<c>LAB-API-v1</c> <c>r27</c> bagian 22.7,
    /// <c>LAB-DEC-119</c>, <c>LAB-DEC-127</c>).
    ///
    /// <b>Empat endpoint, nol <c>POST</c> dan nol <c>DELETE</c>.</b> Barisnya tetap tiga — satu
    /// per disiplin — dan hanya isinya yang berubah. Hak tulisnya dipegang <b>kepala
    /// instalasi</b>.
    ///
    /// Nol endpoint di sini mengubah pemegang wewenang klinis. Nama konsultan yang tercetak dan
    /// pemegang wewenang klinis adalah dua peran berbeda, dan <c>AC-182</c> mengujinya langsung.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-discipline-settings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Discipline Setting",
        AreaName = "HealthServices",
        ControllerName = "LabDisciplineSetting",
        Description = "Label konsultan, kalimat baku, dan awalan nomor cetak per disiplin",
        SortOrder = 21
    )]
    [Tags("Health Services / Laboratory Management / Lab Discipline Setting")]
    public class LabDisciplineSettingController : ControllerBase
    {
        private readonly LabDisciplineSettingService _service;

        public LabDisciplineSettingController(LabDisciplineSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LabDisciplineSettingResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Discipline Setting", Description = "Melihat pengaturan ketiga disiplin", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabDisciplineSetting", "Read")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAllAsync(cancellationToken);

            return Ok(ApiResponse<List<LabDisciplineSettingResponse>>.Ok(
                result, "Pengaturan disiplin berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<LabDisciplineOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Discipline Setting Options", Description = "Melihat pilihan disiplin beserta keadaan pengaturannya", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabDisciplineSetting", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetOptionsAsync(cancellationToken);

            return Ok(ApiResponse<List<LabDisciplineOptionResponse>>.Ok(
                result, "Pilihan disiplin berhasil diambil."));
        }

        [HttpGet("{discipline}")]
        [ProducesResponseType(typeof(ApiResponse<LabDisciplineSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Discipline Setting Detail", Description = "Melihat pengaturan satu disiplin", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("LabDisciplineSetting", "Read")]
        public async Task<IActionResult> GetByDiscipline(
            LabDiscipline discipline,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetByDisciplineAsync(discipline, cancellationToken);

                return Ok(ApiResponse<LabDisciplineSettingResponse>.Ok(
                    result, "Pengaturan disiplin berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        [HttpPut("{discipline}")]
        [ProducesResponseType(typeof(ApiResponse<LabDisciplineSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Lab Discipline Setting", Description = "Mengubah pengaturan satu disiplin", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabDisciplineSetting", "Update")]
        public async Task<IActionResult> Update(
            LabDiscipline discipline,
            [FromBody] LabDisciplineSettingUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(discipline))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "Disiplin itu tidak dikenal."));
            }

            var result = await _service.UpdateAsync(discipline, request, cancellationToken);

            return Ok(ApiResponse<LabDisciplineSettingResponse>.Ok(
                result, "Pengaturan disiplin berhasil disimpan."));
        }
    }
}
