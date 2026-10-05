using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/clinical-management/surgical-site-surveillances")]
[Tags("Health Services / Clinical Management / Surgical Site Surveillance")]
[AccessController("HEALTH_SERVICE_CLINICAL", "Health Service Clinical", "Surgical Site Surveillance", AreaName = "HealthServices", ControllerName = "SurgicalSiteSurveillance")]
public class SurgicalSiteSurveillanceController(CliSurgicalSiteSurveillanceService service) : ControllerBase
{
    [HttpGet]
    [AccessAction("Read", "Read Surgical Site Surveillance", AccessType = AccessTypes.Read), AccessPermission("SurgicalSiteSurveillance", "Read")]
    public async Task<IActionResult> List([FromQuery] SurveillanceQuery request, CancellationToken ct) => Ok(ApiResponse<PagedResult<SurveillanceListItem>>.Ok(await service.ListAsync(request, ct), "Daftar surveilans operasi."));
    [HttpGet("form-readiness")]
    [AccessAction("Read", "Read Surgical Site Surveillance", AccessType = AccessTypes.Read), AccessPermission("SurgicalSiteSurveillance", "Read")]
    public async Task<IActionResult> Readiness(CancellationToken ct) => Ok(ApiResponse<SurveillanceFormReadiness>.Ok(await service.ReadinessAsync(ct), "Kesiapan formulir surveilans."));
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Surgical Site Surveillance", AccessType = AccessTypes.Read), AccessPermission("SurgicalSiteSurveillance", "Read")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct) => this.ToActionResult(await service.DetailAsync(id, ct));
    [HttpPut("{id:guid}/entries/{dayNumber:int}")]
    [AccessAction("Update", "Update Surgical Site Surveillance", AccessType = AccessTypes.Update), AccessPermission("SurgicalSiteSurveillance", "Update")]
    public async Task<IActionResult> Entry(Guid id, int dayNumber, PutSurveillanceEntryRequest request, CancellationToken ct) => this.ToActionResult(await service.PutEntryAsync(id, dayNumber, request, User, this.CurrentUserId(), ct));
    [HttpPut("{id:guid}/summary")]
    [AccessAction("Update", "Update Surgical Site Surveillance", AccessType = AccessTypes.Update), AccessPermission("SurgicalSiteSurveillance", "Update")]
    public async Task<IActionResult> Summary(Guid id, PutSurveillanceSummaryRequest request, CancellationToken ct) => this.ToActionResult(await service.PutSummaryAsync(id, request, User, this.CurrentUserId(), ct));
    [HttpPost("{id:guid}/flag-suspected")]
    [AccessAction("Review", "Review Surgical Site Surveillance", AccessType = AccessTypes.Update), AccessPermission("SurgicalSiteSurveillance", "Review")]
    public async Task<IActionResult> Flag(Guid id, FlagSurveillanceSuspectedRequest request, CancellationToken ct) => this.ToActionResult(await service.FlagSuspectedAsync(id, request, this.CurrentUserId(), ct));
}
