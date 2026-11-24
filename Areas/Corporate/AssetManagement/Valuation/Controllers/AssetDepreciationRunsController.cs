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
[Route("api/v1/corporate/asset-management/valuation/depreciation-runs")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_VALUATION", "Corporate Asset Management Valuation", "Depreciation Run",
    AreaName = "Corporate", ControllerName = "AssetDepreciationRun", Description = "Pratinjau dan proses penyusutan bulanan aset", SortOrder = 4)]
[Tags("Corporate / Asset Management / Valuation / Depreciation Run")]
public sealed class AssetDepreciationRunsController : AssetMasterControllerBase
{
    private readonly AssetDepreciationRunService _service;

    public AssetDepreciationRunsController(AssetDepreciationRunService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Depreciation Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationRun", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DepreciationRunResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetRuns([FromQuery] DepreciationRunPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetRunsAsync(request, cancellationToken), "Daftar proses penyusutan berhasil diambil.");

    [HttpGet("preview")]
    [AccessAction("Read", "Read Depreciation Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationRun", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DepreciationPreviewResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Preview([FromQuery] DepreciationPreviewQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.PreviewAsync(request, cancellationToken), "Pratinjau penyusutan berhasil disusun.");

    [HttpPost]
    [AccessAction("Process", "Process Depreciation Run", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetDepreciationRun", "Process")]
    [ProducesResponseType(typeof(ApiResponse<DepreciationRunResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Process([FromBody] DepreciationRunCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ProcessAsync(request, CurrentUserId(), cancellationToken), "Penyusutan berhasil diproses.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Depreciation Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationRun", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DepreciationRunResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetRun(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetRunAsync(id, cancellationToken), "Rincian proses penyusutan berhasil diambil.");

    [HttpGet("{id:guid}/lines")]
    [AccessAction("Read", "Read Depreciation Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationRun", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DepreciationRunLineResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetLines(Guid id, [FromQuery] DepreciationRunLineQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLinesAsync(id, request, cancellationToken), "Baris proses penyusutan berhasil diambil.");
}
