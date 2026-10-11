using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

// Resource tetap InpatientEpisode:Create, dipisahkan agar AccessController dan AccessPermission cocok.
[ApiController, Authorize, ServiceFilter(typeof(InpBedOperationFilter))]
[Route("api/v1/health-services/inpatient-management/admission-transfers")]
[AccessController(moduleCode: "HEALTH_SERVICE_INPATIENT", moduleName: "Health Service Inpatient",
    displayName: "Inpatient Episode", AreaName = "HealthServices", ControllerName = "InpatientEpisode",
    Description = "Membuka episode dari transfer IGD", SortOrder = 10)]
[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]
public class InpatientAdmissionTransferAdmitController(InpAdmissionTransferService service, InpEpisodeService episodes) : ControllerBase
{
    [HttpPost("admit")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [AccessAction("Create", "Create Inpatient Episode from Transfer", AccessType = AccessTypes.Create)]
    [AccessPermission("InpatientEpisode", "Create")]
    public async Task<IActionResult> AdmitFromTransfer([FromBody] OpenAdmissionFromTransferRequest request, CancellationToken ct)
    {
        var result = await service.ExecuteTransferAdmissionAsync(request, User.GetUserId(), ct);
        var detail = await episodes.GetEpisodeDetailAsync(result.Episode!.Id, ct);
        if (detail != null) detail.OperationMeta = result.OperationMeta;
        return StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Ok(detail, result.Message));
    }
}

