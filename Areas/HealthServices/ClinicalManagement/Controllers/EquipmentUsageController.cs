using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
[ApiController, Authorize, Route("api/v1/health-services/clinical-management/equipment-usages")]
[Tags("Health Services / Clinical Management / Equipment Usage")]
[AccessController("HEALTH_SERVICE_CLINICAL", "Health Service Clinical", "Equipment Usage", AreaName = "HealthServices", ControllerName = "EquipmentUsage")]
public sealed class EquipmentUsageController(CliEquipmentUsageService service) : ControllerBase
{
    [HttpGet, AccessAction("Read", "Read Equipment Usage", AccessType = AccessTypes.Read), AccessPermission("EquipmentUsage", "Read")]
    public async Task<IActionResult> List([FromQuery] Guid episodeId, [FromQuery] CliEquipmentUsageStatus? status, CancellationToken ct) => this.ToActionResult(await service.ListAsync(episodeId, status, ct));
    [HttpPost, AccessAction("Create", "Start Equipment Usage", AccessType = AccessTypes.Create), AccessPermission("EquipmentUsage", "Create")]
    public async Task<IActionResult> Start(StartEquipmentUsageRequest request, CancellationToken ct) => this.ToActionResult(await service.StartAsync(request, User, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/finish"), HttpPost("{id:guid}/finish"), AccessAction("Update", "Finish Equipment Usage", AccessType = AccessTypes.Update), AccessPermission("EquipmentUsage", "Update")]
    public async Task<IActionResult> Finish(Guid id, FinishEquipmentUsageRequest request, CancellationToken ct) => this.ToActionResult(await service.FinishAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/cancel"), HttpPost("{id:guid}/cancel"), AccessAction("Cancel", "Cancel Equipment Usage", AccessType = AccessTypes.Update), AccessPermission("EquipmentUsage", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancelEquipmentUsageRequest request, CancellationToken ct) => this.ToActionResult(await service.CancelAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPut("{id:guid}/time-correction"), AccessAction("Correct", "Correct Equipment Usage", AccessType = AccessTypes.Update), AccessPermission("EquipmentUsage", "Correct")]
    public async Task<IActionResult> Correct(Guid id, CorrectEquipmentUsageRequest request, CancellationToken ct) => this.ToActionResult(await service.CorrectAsync(id, request, User, this.CurrentUserId(), ct));
}
