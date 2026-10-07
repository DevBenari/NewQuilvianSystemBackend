using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/master-data/medical-equipments")]
[Tags("Health Services / Master Data / Medical Equipment")]
[AccessController("HEALTH_SERVICE_MASTER_DATA", "Health Service Master Data", "Medical Equipment", AreaName = "HealthServices", ControllerName = "MedicalEquipment")]
public class MedicalEquipmentController(MedicalEquipmentService service) : ControllerBase
{
    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Medical Equipment", AccessType = AccessTypes.Read), AccessPermission("MedicalEquipment", "Read")]
    public IActionResult Metadata() => Ok(ApiResponse<MedicalEquipmentFilterMetadata>.Ok(new(), "Metadata alat medis."));
    [HttpGet("summary")]
    [AccessAction("Read", "Read Medical Equipment", AccessType = AccessTypes.Read), AccessPermission("MedicalEquipment", "Read")]
    public async Task<IActionResult> Summary(CancellationToken ct) => Ok(ApiResponse<MedicalEquipmentSummary>.Ok(await service.SummaryAsync(ct), "Ringkasan alat medis."));
    [HttpGet("options")]
    [AccessAction("Read", "Read Medical Equipment", AccessType = AccessTypes.Read), AccessPermission("MedicalEquipment", "Read")]
    public async Task<IActionResult> Options([FromQuery] string? search, CancellationToken ct) => Ok(ApiResponse<List<MedicalEquipmentOption>>.Ok(await service.OptionsAsync(search, ct), "Pilihan alat aktif."));
    [HttpGet]
    [AccessAction("Read", "Read Medical Equipment", AccessType = AccessTypes.Read), AccessPermission("MedicalEquipment", "Read")]
    public async Task<IActionResult> List([FromQuery] MedicalEquipmentQuery request, CancellationToken ct) => Ok(ApiResponse<PagedResult<MedicalEquipmentResponse>>.Ok(await service.ListAsync(request, ct), "Daftar alat medis."));
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Medical Equipment", AccessType = AccessTypes.Read), AccessPermission("MedicalEquipment", "Read")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct) => this.ToActionResult(await service.DetailAsync(id, ct));
    [HttpPost]
    [AccessAction("Create", "Create Medical Equipment", AccessType = AccessTypes.Create), AccessPermission("MedicalEquipment", "Create")]
    public async Task<IActionResult> Create(CreateMedicalEquipmentRequest request, CancellationToken ct) => this.ToActionResult(await service.SaveAsync(null, request, this.CurrentUserId(), ct));
    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Medical Equipment", AccessType = AccessTypes.Update), AccessPermission("MedicalEquipment", "Update")]
    public async Task<IActionResult> Update(Guid id, UpdateMedicalEquipmentRequest request, CancellationToken ct) => this.ToActionResult(await service.SaveAsync(id, request, this.CurrentUserId(), ct));
    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Medical Equipment", AccessType = AccessTypes.Update), AccessPermission("MedicalEquipment", "Update")]
    public async Task<IActionResult> Status(Guid id, MedicalEquipmentStatusRequest request, CancellationToken ct) => this.ToActionResult(await service.SetStatusAsync(id, request, this.CurrentUserId(), ct));
    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Medical Equipment", AccessType = AccessTypes.Delete), AccessPermission("MedicalEquipment", "Delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => this.ToActionResult(await service.DeleteAsync(id, this.CurrentUserId(), ct));
}
