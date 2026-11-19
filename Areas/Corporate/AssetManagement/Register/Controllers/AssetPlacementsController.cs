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
[Route("api/v1/corporate/asset-management/register/placements")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Placement",
    AreaName = "Corporate", ControllerName = "AssetPlacement", Description = "Penyerahan dan penempatan aset", SortOrder = 2)]
[Tags("Corporate / Asset Management / Register / Asset Placement")]
public sealed class AssetPlacementsController : AssetMasterControllerBase
{
    private readonly AssetPlacementService _service;

    public AssetPlacementsController(AssetPlacementService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetPlacementListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetPlacementPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPlacementsAsync(request, cancellationToken), "Penempatan aset berhasil diambil.");

    [HttpGet("history")]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetPlacementListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory([FromQuery] AssetPlacementPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPlacementHistoryAsync(request, cancellationToken), "Riwayat penempatan aset berhasil diambil.");
}
