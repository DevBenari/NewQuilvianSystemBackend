using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;

[ApiController, Authorize]
[Route("api/v1/health-services/clinical-management/wsd-drains")]
[Tags("Health Services / Clinical Management / WSD Observation")]
// Berbagi Resource FluidBalance (gate G-15); metadata disamakan dengan FluidBalanceController
// karena resource yang terbaca lebih dulu menentukan label di layar Akses Role.
[AccessController(
    moduleCode: "HEALTH_SERVICE_CLINICAL",
    moduleName: "Health Service Clinical",
    displayName: "Fluid Balance",
    AreaName = "HealthServices",
    ControllerName = "FluidBalance",
    Description = "Cairan masuk dan keluar pasien rawat inap beserta balance per shift dan 24 jam",
    SortOrder = 22)]
public class WsdObservationController(CliWsdObservationService service) : ControllerBase
{
    [HttpGet]
    [AccessAction("Read", "Read Fluid Balance", AccessType = AccessTypes.Read), AccessPermission("FluidBalance", "Read")]
    public async Task<IActionResult> List([FromQuery] Guid episodeId, [FromQuery] bool includeRemoved, CancellationToken ct) => this.ToActionResult(await service.ListAsync(episodeId, includeRemoved, ct));
    [HttpPost]
    [AccessAction("Create", "Create Fluid Balance", AccessType = AccessTypes.Create), AccessPermission("FluidBalance", "Create")]
    public async Task<IActionResult> Register(RegisterWsdDrainRequest request, CancellationToken ct) => this.ToActionResult(await service.RegisterDrainAsync(request, User, this.CurrentUserId(), ct));
    [HttpPut("{drainId:guid}")]
    [AccessAction("Update", "Update Fluid Balance", AccessType = AccessTypes.Update), AccessPermission("FluidBalance", "Update")]
    public async Task<IActionResult> CorrectDrain(Guid drainId, CorrectWsdDrainRequest request, CancellationToken ct) => this.ToActionResult(await service.CorrectDrainAsync(drainId, request, User, this.CurrentUserId(), ct));
    [HttpPatch("{drainId:guid}/remove"), HttpPost("{drainId:guid}/remove")]
    [AccessAction("Update", "Update Fluid Balance", AccessType = AccessTypes.Update), AccessPermission("FluidBalance", "Update")]
    public async Task<IActionResult> Remove(Guid drainId, RemoveWsdDrainRequest request, CancellationToken ct) => this.ToActionResult(await service.RemoveDrainAsync(drainId, request, User, this.CurrentUserId(), ct));
    [HttpGet("{drainId:guid}/readings")]
    [AccessAction("Read", "Read Fluid Balance", AccessType = AccessTypes.Read), AccessPermission("FluidBalance", "Read")]
    public async Task<IActionResult> Readings(Guid drainId, CancellationToken ct) => this.ToActionResult(await service.ReadingsAsync(drainId, ct));
    [HttpPost("{drainId:guid}/readings")]
    [AccessAction("Create", "Create Fluid Balance", AccessType = AccessTypes.Create), AccessPermission("FluidBalance", "Create")]
    public async Task<IActionResult> Record(Guid drainId, RecordWsdReadingRequest request, CancellationToken ct) => this.ToActionResult(await service.RecordReadingAsync(drainId, request, User, this.CurrentUserId(), ct));
    [HttpPut("readings/{readingId:guid}/correction")]
    [AccessAction("Update", "Update Fluid Balance", AccessType = AccessTypes.Update), AccessPermission("FluidBalance", "Update")]
    public async Task<IActionResult> Correct(Guid readingId, CorrectWsdReadingRequest request, CancellationToken ct) => this.ToActionResult(await service.CorrectLatestReadingAsync(readingId, request, User, this.CurrentUserId(), ct));
    [HttpPatch("readings/{readingId:guid}/cancel"), HttpPost("readings/{readingId:guid}/cancel")]
    [AccessAction("Update", "Update Fluid Balance", AccessType = AccessTypes.Update), AccessPermission("FluidBalance", "Update")]
    public async Task<IActionResult> Cancel(Guid readingId, CancelClinicalMeasurementRequest request, CancellationToken ct) => this.ToActionResult(await service.CancelLatestReadingAsync(readingId, request, User, this.CurrentUserId(), ct));
}
