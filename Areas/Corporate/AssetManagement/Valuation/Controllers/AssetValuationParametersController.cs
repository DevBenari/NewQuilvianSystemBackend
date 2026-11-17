using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/valuation/parameters")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_VALUATION", "Corporate Asset Management Valuation", "Valuation Parameter",
    AreaName = "Corporate", ControllerName = "AssetValuationParameter", Description = "Parameter nilai aset per badan hukum", SortOrder = 1)]
[Tags("Corporate / Asset Management / Valuation / Valuation Parameter")]
public sealed class AssetValuationParametersController : AssetMasterControllerBase
{
    private readonly AssetValuationParameterService _service;

    public AssetValuationParametersController(AssetValuationParameterService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Valuation Parameter", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetValuationParameter", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ValuationParameterResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] ValuationPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetParametersAsync(request, cancellationToken), "Riwayat parameter nilai berhasil diambil.");

    [HttpGet("current")]
    [AccessAction("Read", "Read Valuation Parameter", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetValuationParameter", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ValuationParameterCurrentResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetCurrent(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCurrentParameterAsync(cancellationToken), "Parameter nilai yang berlaku berhasil diambil.");

    [HttpPost]
    [AccessAction("Update", "Update Valuation Parameter", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetValuationParameter", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ValuationParameterResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Set([FromBody] ValuationParameterSetRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetParameterAsync(request, CurrentUserId(), cancellationToken), "Parameter nilai berhasil ditetapkan.");
}
