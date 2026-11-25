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
[Route("api/v1/corporate/asset-management/register/reports")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Report",
    AreaName = "Corporate", ControllerName = "AssetReport", Description = "Laporan inventaris per unit, kondisi aset, dan History & Audit", SortOrder = 8)]
[Tags("Corporate / Asset Management / Register / Report")]
public sealed class AssetReportsController : AssetMasterControllerBase
{
    private readonly AssetReportQueryService _service;

    public AssetReportsController(AssetReportQueryService service)
    {
        _service = service;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetReportMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(AssetReportQueryService.GetMetadata()), "Metadata laporan aset berhasil diambil.");

    [HttpGet("inventory-by-unit")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetInventoryByUnit([FromQuery] AssetReportQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetInventoryByUnitAsync(request, cancellationToken), "Daftar inventaris per unit berhasil diambil.");

    [HttpGet("condition")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetCondition([FromQuery] AssetReportQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetConditionAsync(request, cancellationToken), "Laporan kondisi aset berhasil diambil.");

    [HttpGet("history")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetHistoryReportItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory([FromQuery] AssetReportQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(request, cancellationToken), "History & Audit aset berhasil diambil.");

    [HttpGet("{reportCode}/export")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, AssetReportWorkbook.ContentType)]
    public async Task<IActionResult> Export(string reportCode, [FromQuery] AssetReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _service.ExportAsync(reportCode, request, cancellationToken);
            return File(file.Content, AssetReportWorkbook.ContentType, file.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
