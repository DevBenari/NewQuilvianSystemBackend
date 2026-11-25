using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/physical-inventory/reports")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Report",
    AreaName = "Corporate", ControllerName = "AssetReport", Description = "Laporan inventaris per unit, kondisi aset, dan History & Audit", SortOrder = 8)]
[Tags("Corporate / Asset Management / Physical Inventory / Report")]
public sealed class AssetStockTakeReportsController : AssetMasterControllerBase
{
    private readonly StockTakeQueryService _service;

    public AssetStockTakeReportsController(StockTakeQueryService service)
    {
        _service = service;
    }

    [HttpGet("stock-takes/filters/metadata")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetStockTakeMetadata() =>
        ExecuteAsync(() => Task.FromResult(StockTakeQueryService.GetReportMetadata()), "Metadata laporan inventaris berhasil diambil.");

    [HttpGet("stock-takes")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeSessionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetStockTakes([FromQuery] StockTakePagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetReportAsync(request, cancellationToken), "Laporan inventaris berhasil diambil.");

    [HttpGet("stock-takes/export")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, AssetReportWorkbook.ContentType)]
    public async Task<IActionResult> ExportStockTakes([FromQuery] StockTakePagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _service.ExportReportAsync(request, cancellationToken);
            return File(file.Content, AssetReportWorkbook.ContentType, file.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
