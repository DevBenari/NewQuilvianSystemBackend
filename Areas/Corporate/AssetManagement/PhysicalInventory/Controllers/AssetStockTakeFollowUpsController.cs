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
[Route("api/v1/corporate/asset-management/physical-inventory/follow-ups")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_PHYSICAL_INVENTORY", "Corporate Asset Management Physical Inventory", "Asset Stock Take Follow Up",
    AreaName = "Corporate", ControllerName = "AssetStockTakeFollowUp", Description = "Usulan tindak lanjut selisih inventarisasi: diproses menjadi mutasi, penyerahan, laporan kondisi, atau draft aset; atau diabaikan dengan alasan", SortOrder = 2)]
[Tags("Corporate / Asset Management / Physical Inventory / Stock Take Follow Up")]
public sealed class AssetStockTakeFollowUpsController : AssetMasterControllerBase
{
    private readonly StockTakeFollowUpService _service;

    public AssetStockTakeFollowUpsController(StockTakeFollowUpService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Asset Stock Take Follow Up", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTakeFollowUp", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeFollowUpResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] StockTakeFollowUpQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFollowUpsAsync(request, cancellationToken), "Daftar usulan tindak lanjut berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Stock Take Follow Up", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTakeFollowUp", "Read")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeFollowUpResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFollowUpAsync(id, cancellationToken), "Rincian usulan tindak lanjut berhasil diambil.");

    [HttpPost("{id:guid}/process")]
    [AccessAction("Process", "Process Asset Stock Take Follow Up", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetStockTakeFollowUp", "Process")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeFollowUpResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Process(Guid id, [FromBody] StockTakeFollowUpProcessRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ProcessAsync(id, request, cancellationToken), "Usulan diproses; dokumen tujuan sudah dibuat.");

    [HttpPost("{id:guid}/dismiss")]
    [AccessAction("Dismiss", "Dismiss Asset Stock Take Follow Up", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetStockTakeFollowUp", "Dismiss")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeFollowUpResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Dismiss(Guid id, [FromBody] StockTakeDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DismissAsync(id, request, cancellationToken), "Usulan diabaikan.");
}
