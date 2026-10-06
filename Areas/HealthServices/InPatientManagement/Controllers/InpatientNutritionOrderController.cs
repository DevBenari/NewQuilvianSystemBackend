using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

[ApiController, Authorize]
[Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders")]
[Tags("Health Services / Inpatient Management / Inpatient Ancillary Order")]
[AccessController("HEALTH_SERVICE_NUTRITION_MANAGEMENT", "Health Service Nutrition Management", "Nutrition Order",
    AreaName = "HealthServices", ControllerName = "NutritionOrder", Description = "Asuhan gizi pasien rawat inap", SortOrder = 1)]
public class InpatientNutritionOrderController : ControllerBase
{
    private readonly InpAncillaryOrderAdapter _adapter;
    public InpatientNutritionOrderController(InpAncillaryOrderAdapter adapter) => _adapter = adapter;

    [HttpPost("nutrition-consultations")]
    [ProducesResponseType(typeof(ApiResponse<GziOrderDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Nutrition Order", AccessType = AccessTypes.Create)]
    [AccessPermission("NutritionOrder", "Create")]
    public async Task<IActionResult> Create(Guid episodeId,
        [FromBody] CreateInpatientNutritionConsultationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<GziOrderDetailResponse>.Ok(
                await _adapter.CreateNutritionConsultationAsync(episodeId, request, User, cancellationToken),
                "Order konsultasi gizi berhasil dibuat."));
        }
        catch (InpAncillaryOrderException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message, ex.Errors));
        }
    }
}
