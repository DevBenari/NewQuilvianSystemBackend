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
[Route("api/v1/corporate/asset-management/movement/transfers")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MOVEMENT", "Corporate Asset Management Movement", "Asset Transfer",
    AreaName = "Corporate", ControllerName = "AssetTransfer", Description = "Mutasi aset antar unit dengan persetujuan unit asal, Bagian Aset, dan penerimaan unit tujuan", SortOrder = 1)]
[Tags("Corporate / Asset Management / Movement / Asset Transfer")]
public sealed class AssetTransfersController : AssetMasterControllerBase
{
    private readonly AssetTransferService _service;

    public AssetTransfersController(AssetTransferService service)
    {
        _service = service;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(AssetTransferService.GetMetadata()), "Metadata daftar mutasi berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan mutasi berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetTransferResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetTransferPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetTransfersAsync(request, cancellationToken), "Daftar mutasi berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetTransferAsync(id, cancellationToken), "Rincian mutasi berhasil diambil.");

    [HttpGet("{id:guid}/document")]
    [AccessAction("Read", "Read Asset Transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetTransfer", "Read")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _service.GetDocumentAsync(id, cancellationToken);
            return File(document.Content, "application/pdf", document.FileName);
        }
        catch (AssetManagementException exception)
        {
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { code = exception.Code }));
        }
    }

    [HttpPost]
    [AccessAction("Create", "Create Asset Transfer", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetTransfer", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetTransferCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, cancellationToken), "Mutasi berhasil diajukan.");

    [HttpPost("{id:guid}/approve-unit")]
    [AccessAction("ApproveUnit", "Approve Asset Transfer by Source Unit", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetTransfer", "ApproveUnit")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveUnit(Guid id, [FromBody] AssetTransferDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveUnitAsync(id, request, cancellationToken), request.Approve ? "Mutasi disetujui unit asal." : "Mutasi ditolak unit asal.");

    [HttpPost("{id:guid}/approve-asset")]
    [AccessAction("ApproveAsset", "Approve Asset Transfer by Asset Department", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetTransfer", "ApproveAsset")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveAsset(Guid id, [FromBody] AssetTransferDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveAssetAsync(id, request, cancellationToken), request.Approve ? "Mutasi disetujui Bagian Aset." : "Mutasi ditolak Bagian Aset.");

    [HttpPost("{id:guid}/receive")]
    [AccessAction("Receive", "Receive Asset Transfer", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetTransfer", "Receive")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Receive(Guid id, [FromBody] AssetTransferReceiveRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReceiveAsync(id, request, cancellationToken), "Mutasi diterima; penempatan aset sudah berpindah.");

    [HttpPost("{id:guid}/reject-receipt")]
    [AccessAction("Receive", "Receive Asset Transfer", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetTransfer", "Receive")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> RejectReceipt(Guid id, [FromBody] AssetTransferReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectReceiptAsync(id, request, cancellationToken), "Mutasi ditolak saat serah terima.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Asset Transfer", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetTransfer", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<AssetTransferResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] AssetTransferReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Mutasi dibatalkan.");
}
