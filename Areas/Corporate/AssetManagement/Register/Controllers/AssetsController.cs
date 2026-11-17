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
[Route("api/v1/corporate/asset-management/register/assets")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Register",
    AreaName = "Corporate", ControllerName = "AssetRegister", Description = "Pencatatan, persetujuan, dan penomoran aset", SortOrder = 1)]
[Tags("Corporate / Asset Management / Register / Asset")]
public sealed class AssetsController : AssetMasterControllerBase
{
    private readonly AssetRecordService _service;

    public AssetsController(AssetRecordService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetMetadata()), "Metadata daftar aset berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetRegisterSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan aset berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAssetsAsync(request, cancellationToken), "Daftar aset berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAssetAsync(id, cancellationToken), "Rincian aset berhasil diambil.");

    [HttpGet("{id:guid}/amounts")]
    [AccessAction("ViewAmount", "View Asset Amount", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("AssetRegister", "ViewAmount")]
    [ProducesResponseType(typeof(ApiResponse<AssetAmountResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAmounts(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetAmountsAsync(id, cancellationToken), "Nilai aset berhasil diambil.");

    [HttpGet("{id:guid}/history")]
    [AccessAction("Read", "Read Asset Register", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetRegister", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetHistoryItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory(Guid id, [FromQuery] AssetHistoryPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(id, request, cancellationToken), "Linimasa aset berhasil diambil.");

    [HttpPost("drafts")]
    [AccessAction("Create", "Create Asset Draft", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("AssetRegister", "Create")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetListItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> CreateDrafts([FromBody] AssetDraftCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateDraftsAsync(request, cancellationToken), "Draft aset berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Asset Draft", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetRegister", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetDraftUpdateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateDraftAsync(id, request, cancellationToken), "Draft aset berhasil diubah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Asset", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetRegister", "Submit")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(id, cancellationToken), "Aset berhasil diajukan.");

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Asset", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetRegister", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveAsync(id, cancellationToken), "Aset berhasil disetujui.");

    [HttpPost("approve-batch")]
    [AccessAction("Approve", "Approve Asset", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetRegister", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<List<AssetDecisionResultResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveBatch([FromBody] AssetBatchApproveRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ApproveBatchAsync(request, cancellationToken), "Persetujuan massal selesai diproses.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Asset", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetRegister", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Reject(Guid id, [FromBody] AssetDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(id, request, cancellationToken), "Aset dikembalikan ke draft.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Update", "Update Asset Draft", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetRegister", "Update")]
    [ProducesResponseType(typeof(ApiResponse<AssetDetailResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] AssetDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Draft aset berhasil dibatalkan.");
}
