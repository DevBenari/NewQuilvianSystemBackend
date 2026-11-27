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
    // BE-LAB-104 — LAB-API-v1 r51 bagian 46.3, LAB-PERM-v1 rev 19 bagian 21. Aktivasi penyerahan hasil
    // kepada pasien per disiplin dan daftar pemeriksaan sensitif. Izin tersendiri, tidak ikut
    // LabDisciplineSetting : Update (LAB-DEC-296). 409 = keadaan (VAL-178/179, membawa code);
    // 422 = isian.
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-patient-delivery-settings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Patient Delivery Setting",
        AreaName = "HealthServices",
        ControllerName = "LabPatientDeliverySetting",
        Description = "Aktivasi penyerahan hasil kepada pasien dan daftar pemeriksaan sensitif",
        SortOrder = 27
    )]
    [Tags("Health Services / Laboratory Management / Lab Patient Delivery Setting")]
    public class LabPatientDeliverySettingController : ControllerBase
    {
        private readonly LabPatientDeliverySettingService _service;

        public LabPatientDeliverySettingController(LabPatientDeliverySettingService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LabPatientDeliverySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Lab Patient Delivery Setting", Description = "Melihat aktivasi penyerahan hasil dan daftar pemeriksaan sensitif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPatientDeliverySetting", "Read")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAsync(cancellationToken);

            return Ok(ApiResponse<LabPatientDeliverySettingResponse>.Ok(
                result, "Setelan penyerahan hasil berhasil diambil."));
        }

        [HttpPut("{discipline}/activation")]
        [ProducesResponseType(typeof(ApiResponse<LabPatientDeliverySettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Change Lab Patient Delivery Activation", Description = "Mengaktifkan atau menonaktifkan penyerahan hasil kepada pasien per disiplin", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LabPatientDeliverySetting", "Update")]
        public Task<IActionResult> ChangeActivation(
            LabDiscipline discipline,
            [FromBody] ChangeLabPatientDeliveryActivationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                discipline,
                async () => Ok(ApiResponse<LabPatientDeliverySettingResponse>.Ok(
                    await _service.ChangeActivationAsync(discipline, request, cancellationToken),
                    "Aktivasi penyerahan hasil berhasil diubah.")));

        [HttpGet("{discipline}/activation-history")]
        [ProducesResponseType(typeof(ApiResponse<List<LabPatientDeliveryActivationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Patient Delivery Activation History", Description = "Melihat jejak aktivasi penyerahan hasil satu disiplin", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("LabPatientDeliverySetting", "Read")]
        public Task<IActionResult> GetActivationHistory(
            LabDiscipline discipline,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                discipline,
                async () => Ok(ApiResponse<List<LabPatientDeliveryActivationResponse>>.Ok(
                    await _service.GetActivationHistoryAsync(discipline, cancellationToken),
                    "Jejak aktivasi berhasil diambil.")));

        [HttpPost("sensitive-procedures")]
        [ProducesResponseType(typeof(ApiResponse<LabSensitiveProcedureResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Add Lab Sensitive Procedure", Description = "Menandai jenis pemeriksaan sebagai sensitif", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabPatientDeliverySetting", "Update")]
        public Task<IActionResult> AddSensitiveProcedure(
            [FromBody] AddLabSensitiveProcedureRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                null,
                async () => StatusCode(StatusCodes.Status201Created, ApiResponse<LabSensitiveProcedureResponse>.Ok(
                    await _service.AddSensitiveProcedureAsync(request, cancellationToken),
                    "Pemeriksaan berhasil ditandai sensitif.")));

        [HttpDelete("sensitive-procedures/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Remove Lab Sensitive Procedure", Description = "Mencabut penanda pemeriksaan sensitif, beralasan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabPatientDeliverySetting", "Update")]
        public Task<IActionResult> RemoveSensitiveProcedure(
            Guid id,
            [FromBody] RemoveLabSensitiveProcedureRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                null,
                async () =>
                {
                    await _service.RemoveSensitiveProcedureAsync(id, request, cancellationToken);
                    return Ok(ApiResponse<object>.Ok(null,"Penanda pemeriksaan sensitif berhasil dicabut."));
                });

        private async Task<IActionResult> ExecuteAsync(LabDiscipline? discipline, Func<Task<IActionResult>> action)
        {
            if (discipline.HasValue && !Enum.IsDefined(discipline.Value))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "Disiplin itu tidak dikenal."));
            }

            try
            {
                return await action();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (LabPatientResultConflictException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, ex.Message, new { code = ex.Code }));
            }
            catch (LabPatientResultValidationException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, ex.Message));
            }
        }
    }
}
