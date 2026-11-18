using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Controllers;

/// <summary>
/// Konsolidasi kebutuhan unit per barang (BE-PBJ-011; PBJ-PP-API bagian Consolidation). Hanya baca.
/// </summary>
/// <remarks>
/// Memakai resource izin yang sama dengan <see cref="PackagePlansController"/>
/// (<c>ProcurementPackagePlan : Read</c>), mengikuti pola beberapa controller berbagi satu
/// <c>ControllerName</c> di repository (mis. <c>InpatientEpisode</c>).
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/plan-consolidations")]
[AccessController(ProcurementPermissions.PlanningModuleCode, ProcurementPermissions.PlanningModuleName, "Procurement Package Plan",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PackagePlanResource,
    Description = "Konsolidasi kebutuhan unit dan rencana paket pengadaan", SortOrder = 3)]
[Tags("Corporate / Procurement Management / Planning / Consolidation")]
public sealed class PlanConsolidationsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PackagePlanResource;
    private readonly ProcurementPlanConsolidationService _service;

    public PlanConsolidationsController(ProcurementPlanConsolidationService service) => _service = service;

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package Plan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<PlanConsolidationRow>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PlanConsolidationQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<PlanConsolidationRow>>.Ok(
            await _service.GetAsync(User, request, cancellationToken), "Konsolidasi kebutuhan berhasil diambil."));
}
