using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/movement/reports")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Report",
    AreaName = "Corporate", ControllerName = "AssetReport", Description = "Laporan inventaris per unit, kondisi aset, dan History & Audit", SortOrder = 8)]
[Tags("Corporate / Asset Management / Movement / Report")]
public sealed class AssetMovementReportsController : AssetMasterControllerBase
{
    private readonly MovementQueryService _service;

    public AssetMovementReportsController(MovementQueryService service)
    {
        _service = service;
    }

    [HttpGet("transfers/filters/metadata")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTransferMetadata() =>
        ExecuteAsync(() => Task.FromResult(MovementQueryService.GetTransferReportMetadata()), "Metadata laporan mutasi berhasil diambil.");

    [HttpGet("transfers")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetTransferResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTransfers([FromQuery] AssetTransferPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetTransferReportAsync(request, cancellationToken), "Laporan mutasi berhasil diambil.");

    [HttpGet("transfers/export")]
    [AccessAction("Read", "Read Asset Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetReport", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, AssetReportWorkbook.ContentType)]
    public async Task<IActionResult> ExportTransfers([FromQuery] AssetTransferPagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _service.ExportTransferReportAsync(request, cancellationToken);
            return File(file.Content, AssetReportWorkbook.ContentType, file.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
