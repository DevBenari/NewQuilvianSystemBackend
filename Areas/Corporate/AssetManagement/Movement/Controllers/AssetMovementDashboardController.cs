using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/movement/dashboard")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MOVEMENT", "Corporate Asset Management Movement", "Asset Transfer",
    AreaName = "Corporate", ControllerName = "AssetTransfer", Description = "Mutasi aset antar unit dengan persetujuan unit asal, Bagian Aset, dan penerimaan unit tujuan", SortOrder = 1)]
[Tags("Corporate / Asset Management / Movement / Dashboard")]
public sealed class AssetMovementDashboardController : AssetMasterControllerBase
{
    private readonly MovementQueryService _service;

    public AssetMovementDashboardController(MovementQueryService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMovementSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary([FromQuery] AssetMovementSummaryQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(request, cancellationToken), "Ringkasan mutasi dan pinjaman berjalan berhasil diambil.");
}
