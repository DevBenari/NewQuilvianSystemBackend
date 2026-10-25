using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Controllers;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

/// <summary>
/// Endpoint consumer event kelahiran bayi intraoperatif dari modul kamar operasi IBS (BE-RWI-184, OPS-DEC-042, RWI-DEC-357).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/inpatient-management/integration")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_INPATIENT_MANAGEMENT",
    moduleName: "Health Service Inpatient Management",
    displayName: "Inpatient Newborn Birth Event Consumer",
    AreaName = "HealthServices",
    ControllerName = "InpatientNewbornBirthEventConsumer",
    Description = "Consumer integrasi event kelahiran bayi intraoperatif meja bedah SC",
    SortOrder = 6)]
[Tags("Health Services / Inpatient Management / Integration")]
public class InpatientNewbornBirthEventConsumerController(
    InpatientNewbornBirthEventConsumerService consumerService) : ControllerBase
{
    /// <summary>
    /// Menerima dan memproses event operating-room.birth-event.recorded untuk membuka episode neonatus otomatis.
    /// </summary>
    [HttpPost("birth-event-consumer")]
    [ProducesResponseType(typeof(ApiResponse<NewbornBirthEventConsumerResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AccessAction("Create", "Consume Birth Event", Description = "Memproses event kelahiran bayi intraoperatif", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("InpatientAdmission", "Create")]
    public async Task<IActionResult> ConsumeBirthEvent(
        [FromBody] OprBirthRecordedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        // BaseController tidak ada di repository; aktor dibaca dari claim seperti controller Rawat Inap lain.
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        var actorUserId = Guid.TryParse(userIdText, out var parsedUserId) ? parsedUserId : Guid.Empty;
        var result = await consumerService.ConsumeAsync(@event, actorUserId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message));
        }

        return Ok(ApiResponse<NewbornBirthEventConsumerResult>.Ok(result, result.Message));
    }
}
