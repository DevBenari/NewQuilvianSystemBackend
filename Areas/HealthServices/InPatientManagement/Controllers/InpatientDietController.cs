using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;
[ApiController, Authorize]
[Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/diets")]
[Tags("Health Services / Inpatient Management / Inpatient Diet")]
[AccessController("HEALTH_SERVICE_NUTRITION_MANAGEMENT", "Health Service Nutrition Management", "Patient Diet", AreaName = "HealthServices", ControllerName = "NutritionPatientDiet")]
public class InpatientDietController(InpDietOrderAdapter adapter) : ControllerBase
{
    [HttpPost]
    [AccessAction("Update", "Update Patient Diet", AccessType = AccessTypes.Update), AccessPermission("NutritionPatientDiet", "Update")]
    public Task<IActionResult> Prescribe(Guid episodeId, PrescribeInpatientDietRequest request, CancellationToken ct) => ExecuteAsync(() => adapter.PrescribeAsync(episodeId, request, User, this.CurrentUserId(), ct));
    [HttpPost("{dietId:guid}/stop")]
    [AccessAction("Update", "Update Patient Diet", AccessType = AccessTypes.Update), AccessPermission("NutritionPatientDiet", "Update")]
    public Task<IActionResult> Stop(Guid episodeId, Guid dietId, StopInpatientDietRequest request, CancellationToken ct) => ExecuteAsync(() => adapter.StopAsync(episodeId, dietId, request, User, this.CurrentUserId(), ct));
    private async Task<IActionResult> ExecuteAsync(Func<Task<GziPatientDietResponse>> action)
    {
        try { return Ok(ApiResponse<GziPatientDietResponse>.Ok(await action(), "Diet pasien tersimpan.")); }
        catch (InpAncillaryOrderException ex) { return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message, ex.Errors)); }
        catch (NutritionForbiddenException ex) { return StatusCode(403, ApiResponse<object>.Fail(403, ex.Message)); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
    }
}
