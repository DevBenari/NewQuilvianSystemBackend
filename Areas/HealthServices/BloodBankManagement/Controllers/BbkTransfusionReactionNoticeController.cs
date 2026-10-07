using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/blood-bank-management/transfusion-reaction-notices")]
[Tags("Health Services / Blood Bank Management / Transfusion Reaction Notice")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
    moduleName: "Health Service Blood Bank Management",
    displayName: "Transfusion Reaction Notice",
    AreaName = "HealthServices",
    ControllerName = "TransfusionReactionNotice",
    Description = "Kotak masuk reaksi transfusi dari bangsal dan tindak lanjut Bank Darah")]
public class BbkTransfusionReactionNoticeController(BbkTransfusionReactionNoticeService service) : ControllerBase
{
    [HttpGet]
    [AccessAction("Read", "Read Transfusion Reaction Notice", AccessType = AccessTypes.Read), AccessPermission("TransfusionReactionNotice", "Read")]
    public async Task<IActionResult> List([FromQuery] ReactionNoticeQuery request, CancellationToken ct) => Ok(ApiResponse<PagedResult<ReactionNoticeDetail>>.Ok(await service.ListAsync(request, ct), "Kotak masuk reaksi transfusi."));
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Transfusion Reaction Notice", AccessType = AccessTypes.Read), AccessPermission("TransfusionReactionNotice", "Read")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct) => this.ToActionResult(await service.DetailAsync(id, ct));
    [HttpPost("{id:guid}/acknowledge")]
    [AccessAction("Acknowledge", "Acknowledge Transfusion Reaction Notice", AccessType = AccessTypes.Update), AccessPermission("TransfusionReactionNotice", "Acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, AcknowledgeReactionNoticeRequest request, CancellationToken ct) => this.ToActionResult(await service.AcknowledgeAsync(id, request, this.CurrentUserId(), ct));
}
