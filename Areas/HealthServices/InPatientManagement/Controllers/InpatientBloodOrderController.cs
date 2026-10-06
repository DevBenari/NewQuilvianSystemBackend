using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

[ApiController, Authorize]
[Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders")]
[Tags("Health Services / Inpatient Management / Inpatient Ancillary Order")]
[AccessController("HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT", "Health Service Blood Bank Management", "Blood Order",
    AreaName = "HealthServices", ControllerName = "BloodOrder",
    Description = "Membuat, memantau, dan membatalkan order darah", SortOrder = 1)]
public class InpatientBloodOrderController : ControllerBase
{
    private readonly InpAncillaryOrderAdapter _adapter;
    public InpatientBloodOrderController(InpAncillaryOrderAdapter adapter) => _adapter = adapter;

    [HttpPost("blood-orders")]
    [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Blood Order", AccessType = AccessTypes.Create)]
    [AccessPermission("BloodOrder", "Create")]
    public async Task<IActionResult> Create(Guid episodeId,
        [FromBody] CreateInpatientBloodOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await _adapter.CreateBloodOrderAsync(episodeId, request, User, cancellationToken),
                "Order darah berhasil dibuat."));
        }
        catch (InpAncillaryOrderException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message, ex.Errors));
        }
    }

    [HttpPost("blood-orders/confirm-duplicate")]
    [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Blood Order", AccessType = AccessTypes.Create)]
    [AccessPermission("BloodOrder", "Create")]
    public async Task<IActionResult> ConfirmDuplicate(Guid episodeId,
        [FromBody] ConfirmInpatientDuplicateBloodOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await _adapter.ConfirmDuplicateBloodOrderAsync(episodeId, request, User, cancellationToken),
                "Order darah berhasil dibuat dengan konfirmasi pesanan mirip."));
        }
        catch (InpAncillaryOrderException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message, ex.Errors));
        }
    }
}
