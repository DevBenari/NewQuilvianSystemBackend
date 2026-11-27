using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/register/dashboard")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Dashboard",
    AreaName = "Corporate", ControllerName = "AssetDashboard", Description = "Dashboard ringkasan, kondisi, dan sebaran aset per unit", SortOrder = 7)]
[Tags("Corporate / Asset Management / Register / Dashboard")]
public sealed class AssetDashboardController : AssetMasterControllerBase
{
    private readonly AssetDashboardQueryService _service;

    public AssetDashboardController(AssetDashboardQueryService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Dashboard", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDashboard", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDashboardSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary([FromQuery] AssetDashboardQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(request, cancellationToken), "Ringkasan dashboard aset berhasil diambil.");

    [HttpGet("by-unit")]
    [AccessAction("Read", "Read Asset Dashboard", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDashboard", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetUnitSummaryResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetByUnit(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByUnitAsync(cancellationToken), "Jumlah aset per unit berhasil diambil.");
}
