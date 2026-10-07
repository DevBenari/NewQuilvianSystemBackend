using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/nutrition-management/diets")]
[Tags("Health Services / Nutrition Management / Patient Diet")]
[AccessController("HEALTH_SERVICE_NUTRITION_MANAGEMENT", "Health Service Nutrition Management", "Patient Diet", AreaName = "HealthServices", ControllerName = "NutritionPatientDiet")]
public class NutritionDietVerificationController(NutritionDietService service, InpatientClinicalContextService clinical) : ControllerBase
{
    [HttpGet("instruction-verification-worklist")]
    [AccessAction("VerifyInstruction", "Verify Diet Instruction", AccessType = AccessTypes.Read), AccessPermission("NutritionPatientDiet", "VerifyInstruction")]
    public async Task<IActionResult> Worklist([FromQuery] GziNutritionPatientQuery request, CancellationToken ct)
    {
        try { return Ok(ApiResponse<PagedResult<GziPatientDietResponse>>.Ok(await service.GetDietVerificationWorklistAsync(request, clinical, ct), "Diet menunggu verifikasi.")); }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
    }
    [HttpPost("{dietId:guid}/verify-instruction")]
    [AccessAction("VerifyInstruction", "Verify Diet Instruction", AccessType = AccessTypes.Update), AccessPermission("NutritionPatientDiet", "VerifyInstruction")]
    public async Task<IActionResult> Verify(Guid dietId, VerifyNutritionInstructionRequest request, CancellationToken ct)
        => this.ToActionResult(await service.VerifyDietInstructionAsync(dietId, request, clinical, ct));
}
