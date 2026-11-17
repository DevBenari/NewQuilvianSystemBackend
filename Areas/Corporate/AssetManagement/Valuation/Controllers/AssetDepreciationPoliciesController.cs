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
[Route("api/v1/corporate/asset-management/valuation/depreciation-policies")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_VALUATION", "Corporate Asset Management Valuation", "Depreciation Policy",
    AreaName = "Corporate", ControllerName = "AssetDepreciationPolicy", Description = "Umur manfaat dan nilai residu per kelompok aset", SortOrder = 2)]
[Tags("Corporate / Asset Management / Valuation / Depreciation Policy")]
public sealed class AssetDepreciationPoliciesController : AssetMasterControllerBase
{
    private readonly AssetValuationParameterService _service;

    public AssetDepreciationPoliciesController(AssetValuationParameterService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Depreciation Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationPolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DepreciationPolicyGroupResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] ValuationPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPoliciesAsync(request, cancellationToken), "Kebijakan penyusutan berhasil diambil.");

    [HttpGet("{assetGroupId:guid}/history")]
    [AccessAction("Read", "Read Depreciation Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetDepreciationPolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<DepreciationPolicyResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory(Guid assetGroupId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPolicyHistoryAsync(assetGroupId, cancellationToken), "Riwayat kebijakan penyusutan berhasil diambil.");

    [HttpPost]
    [AccessAction("Update", "Update Depreciation Policy", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetDepreciationPolicy", "Update")]
    [ProducesResponseType(typeof(ApiResponse<DepreciationPolicyResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Set([FromBody] DepreciationPolicySetRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetPolicyAsync(request, CurrentUserId(), cancellationToken), "Kebijakan penyusutan berhasil ditetapkan.");
}
