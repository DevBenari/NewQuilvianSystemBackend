using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

[ApiController, Authorize]
[Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders")]
[Tags("Health Services / Inpatient Management / Inpatient Ancillary Order")]
[AccessController("HEALTH_SERVICE_INPATIENT", "Health Service Inpatient", "Inpatient Episode",
    AreaName = "HealthServices", ControllerName = "InpatientEpisode",
    Description = "Mengelola episode rawat inap, dari admisi dibuka sampai admisi dibatalkan", SortOrder = 10)]
public class InpatientAncillaryOrderController : ControllerBase
{
    private readonly InpAncillaryOrderAdapter _adapter;
    public InpatientAncillaryOrderController(InpAncillaryOrderAdapter adapter) => _adapter = adapter;

    [HttpGet("coverage-status")]
    [ProducesResponseType(typeof(ApiResponse<List<CoverageStatusItem>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Inpatient Episode", AccessType = AccessTypes.Read)]
    [AccessPermission("InpatientEpisode", "Read")]
    public async Task<IActionResult> CoverageStatus(Guid episodeId,
        [FromQuery] AncillaryCoverageQuery query, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<List<CoverageStatusItem>>.Ok(
                await _adapter.GetCoverageStatusAsync(episodeId, query, User, cancellationToken),
                "Status tanggungan dan perkiraan harga berhasil diambil."));
        }
        catch (InpAncillaryOrderException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message, ex.Errors));
        }
    }
}
