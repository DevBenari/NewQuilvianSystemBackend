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
[Route("api/v1/corporate/asset-management/valuation")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_VALUATION", "Corporate Asset Management Valuation", "Subledger Setup",
    AreaName = "Corporate", ControllerName = "AssetSubledgerSetup", Description = "Akun kontrol subledger per kelompok aset dan pemetaan pusat biaya per unit", SortOrder = 3)]
[Tags("Corporate / Asset Management / Valuation / Subledger Setup")]
public sealed class AssetSubledgerSetupController : AssetMasterControllerBase
{
    private readonly AssetSubledgerSetupService _service;

    public AssetSubledgerSetupController(AssetSubledgerSetupService service) => _service = service;

    [HttpGet("subledger-account-setups")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<SubledgerAccountSetupResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAccountSetups(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAccountSetupsAsync(cancellationToken), "Akun kontrol per kelompok aset berhasil diambil.");

    [HttpPut("subledger-account-setups/{assetGroupId:guid}")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<SubledgerAccountSetupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> SetAccountSetup(Guid assetGroupId, [FromBody] SubledgerAccountSetupRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetAccountSetupAsync(assetGroupId, request, CurrentUserId(), cancellationToken), "Akun kontrol kelompok aset berhasil ditetapkan.");

    [HttpGet("unit-cost-centers")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<UnitCostCenterResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetUnitCostCenters([FromQuery] UnitCostCenterQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetUnitCostCentersAsync(request, cancellationToken), "Pusat biaya per unit berhasil diambil.");

    [HttpGet("unit-cost-centers/cost-center-options")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<CostCenterOptionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetCostCenterOptions(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetCostCenterOptionsAsync(cancellationToken), "Pilihan pusat biaya berhasil diambil.");

    [HttpPut("unit-cost-centers/{organizationUnitId:guid}")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<UnitCostCenterResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> SetUnitCostCenter(Guid organizationUnitId, [FromBody] UnitCostCenterRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SetUnitCostCenterAsync(organizationUnitId, request, CurrentUserId(), cancellationToken), "Pemetaan pusat biaya unit berhasil ditetapkan.");

    [HttpDelete("unit-cost-centers/{organizationUnitId:guid}")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("AssetSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public Task<IActionResult> DeleteUnitCostCenter(Guid organizationUnitId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteUnitCostCenterAsync(organizationUnitId, CurrentUserId(), cancellationToken), "Pemetaan dihapus; unit kembali memakai pusat biaya otomatis.");
}
