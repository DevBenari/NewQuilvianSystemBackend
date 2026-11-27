using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/physical-inventory/dashboard")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_PHYSICAL_INVENTORY", "Corporate Asset Management Physical Inventory", "Asset Stock Take",
    AreaName = "Corporate", ControllerName = "AssetStockTake", Description = "Jadwal dan sesi inventarisasi fisik aset per unit beserta tim pemeriksa", SortOrder = 1)]
[Tags("Corporate / Asset Management / Physical Inventory / Dashboard")]
public sealed class AssetStockTakeDashboardController : AssetMasterControllerBase
{
    private readonly StockTakeQueryService _service;

    public AssetStockTakeDashboardController(StockTakeQueryService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary([FromQuery] StockTakeDashboardQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(request, cancellationToken), "Ringkasan inventaris tertunda berhasil diambil.");
}
