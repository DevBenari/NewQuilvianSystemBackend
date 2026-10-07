using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/clinical-management/transfusion-monitorings")]
[Tags("Health Services / Clinical Management / Transfusion Monitoring")]
[AccessController("HEALTH_SERVICE_CLINICAL", "Health Service Clinical", "Transfusion Monitoring", AreaName = "HealthServices", ControllerName = "TransfusionMonitoring")]
public class TransfusionMonitoringController(CliTransfusionMonitoringService service) : ControllerBase
{
    [HttpGet]
    [AccessAction("Read", "Read Transfusion Monitoring", AccessType = AccessTypes.Read), AccessPermission("TransfusionMonitoring", "Read")]
    public async Task<IActionResult> List([FromQuery] Guid episodeId, CancellationToken ct) => this.ToActionResult(await service.ListAsync(episodeId, ct));
    [HttpGet("selectable-units")]
    [AccessAction("Read", "Read Transfusion Monitoring", AccessType = AccessTypes.Read), AccessPermission("TransfusionMonitoring", "Read")]
    public async Task<IActionResult> Selectable([FromQuery] Guid episodeId, CancellationToken ct) => this.ToActionResult(await service.SelectableAsync(episodeId, ct));
    [HttpPost]
    [AccessAction("Create", "Create Transfusion Monitoring", AccessType = AccessTypes.Create), AccessPermission("TransfusionMonitoring", "Create")]
    public async Task<IActionResult> Start(StartTransfusionMonitoringRequest request, CancellationToken ct) => this.ToActionResult(await service.StartAsync(request, User, this.CurrentUserId(), ct));
    [HttpPut("{id:guid}/points/{pointType}")]
    [AccessAction("Update", "Update Transfusion Monitoring", AccessType = AccessTypes.Update), AccessPermission("TransfusionMonitoring", "Update")]
    public async Task<IActionResult> Point(Guid id, CliTransfusionPointType pointType, PutTransfusionPointRequest request, CancellationToken ct) => this.ToActionResult(await service.PutPointAsync(id, pointType, request, User, this.CurrentUserId(), ct));
    [HttpPost("{id:guid}/reactions")]
    [AccessAction("Update", "Update Transfusion Monitoring", AccessType = AccessTypes.Update), AccessPermission("TransfusionMonitoring", "Update")]
    public async Task<IActionResult> Reaction(Guid id, RecordTransfusionReactionRequest request, CancellationToken ct) => this.ToActionResult(await service.RecordReactionAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/stop"), HttpPost("{id:guid}/stop")]
    [AccessAction("Update", "Update Transfusion Monitoring", AccessType = AccessTypes.Update), AccessPermission("TransfusionMonitoring", "Update")]
    public async Task<IActionResult> Stop(Guid id, StopTransfusionRequest request, CancellationToken ct) => this.ToActionResult(await service.StopAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/complete"), HttpPost("{id:guid}/complete")]
    [AccessAction("Update", "Update Transfusion Monitoring", AccessType = AccessTypes.Update), AccessPermission("TransfusionMonitoring", "Update")]
    public async Task<IActionResult> Complete(Guid id, CompleteTransfusionRequest request, CancellationToken ct) => this.ToActionResult(await service.CompleteAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/cancel"), HttpPost("{id:guid}/cancel")]
    [AccessAction("Update", "Update Transfusion Monitoring", AccessType = AccessTypes.Update), AccessPermission("TransfusionMonitoring", "Update")]
    public async Task<IActionResult> Cancel(Guid id, CancelClinicalMeasurementRequest request, CancellationToken ct) => this.ToActionResult(await service.CancelAsync(id, request, User, this.CurrentUserId(), ct));
}
