using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/register/handovers")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Placement",
    AreaName = "Corporate", ControllerName = "AssetPlacement", Description = "Penyerahan dan penempatan aset", SortOrder = 2)]
[Tags("Corporate / Asset Management / Register / Asset Handover")]
public sealed class AssetHandoversController : AssetMasterControllerBase
{
    private readonly AssetPlacementService _service;

    public AssetHandoversController(AssetPlacementService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetHandoverMetadata()), "Metadata penyerahan berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHandoverSummaryAsync(cancellationToken), "Ringkasan penyerahan berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetHandoverResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetPlacementPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHandoversAsync(request, cancellationToken), "Daftar penyerahan berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHandoverAsync(id, cancellationToken), "Rincian penyerahan berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Handover", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetPlacement", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetHandoverCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateHandoverAsync(request, cancellationToken), "Aset berhasil diserahkan dan menunggu konfirmasi penerima.");

    [HttpPost("{id:guid}/confirm")]
    [AccessAction("Confirm", "Confirm Asset Handover", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetPlacement", "Confirm")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ConfirmAsync(id, cancellationToken), "Penyerahan dikonfirmasi; penempatan aset diperbarui.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Confirm", "Confirm Asset Handover", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetPlacement", "Confirm")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Reject(Guid id, [FromBody] AssetHandoverDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(id, request, cancellationToken), "Penyerahan ditolak penerima.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Asset Handover", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetPlacement", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<AssetHandoverResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] AssetHandoverDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Penyerahan dibatalkan.");

    [HttpGet("{id:guid}/document")]
    [AccessAction("Read", "Read Asset Placement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetPlacement", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _service.GetHandoverDocumentAsync(id, cancellationToken);
            return File(document.Content, "application/pdf", document.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }
}
