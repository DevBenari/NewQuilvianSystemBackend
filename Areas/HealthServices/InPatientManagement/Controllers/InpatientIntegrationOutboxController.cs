using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/inpatient-management/integration-outbox")]
[Tags("Health Services / Inpatient Management / Integration Outbox")]
[AccessController("HEALTH_SERVICE_INPATIENT", "Health Service Inpatient", "Inpatient Integration Outbox",
    AreaName = "HealthServices", ControllerName = "InpatientIntegrationOutbox", SortOrder = 14)]
public sealed class InpatientIntegrationOutboxController(InpIntegrationReplayService service) : ControllerBase
{
    [HttpGet]
    [AccessAction("Read", "Read Integration Outbox", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("InpatientIntegrationOutbox", "Read")]
    public async Task<IActionResult> Get([FromQuery] OutboxMonitorQuery query, CancellationToken ct)
    {
        try { return Ok(ApiResponse<OutboxMonitorResponse>.Ok(await service.GetMonitorAsync(query, ct), "Antrean integrasi berhasil dibaca.")); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("replay")]
    [AccessAction("Replay", "Replay Integration Outbox", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("InpatientIntegrationOutbox", "Replay")]
    public async Task<IActionResult> Replay([FromBody] ReplayRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        if (!Guid.TryParse(userId, out var actorId) || actorId == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(401, "Identitas pengguna tidak valid."));
        try { return Ok(ApiResponse<ReplayResultResponse>.Ok(await service.ReplayAsync(request, actorId, ct), "Putar ulang integrasi selesai.")); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
