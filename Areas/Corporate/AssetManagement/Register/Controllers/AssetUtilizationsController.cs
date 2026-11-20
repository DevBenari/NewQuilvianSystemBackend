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
[Route("api/v1/corporate/asset-management/register/utilizations")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_REGISTER", "Corporate Asset Management Register", "Asset Utilization",
    AreaName = "Corporate", ControllerName = "AssetUtilization", Description = "Pemanfaatan aset oleh pihak lain sesuai kewenangan", SortOrder = 5)]
[Tags("Corporate / Asset Management / Register / Asset Utilization")]
public sealed class AssetUtilizationsController : AssetMasterControllerBase
{
    private readonly AssetUtilizationService _service;

    public AssetUtilizationsController(AssetUtilizationService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Utilization", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetUtilization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(_service.GetMetadata()), "Metadata pemanfaatan berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Utilization", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetUtilization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan pemanfaatan berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Utilization", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetUtilization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetUtilizationResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetUtilizationPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetUtilizationsAsync(request, cancellationToken), "Daftar pemanfaatan berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Utilization", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetUtilization", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetUtilizationAsync(id, cancellationToken), "Rincian pemanfaatan berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Create Asset Utilization", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetUtilization", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetUtilizationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, cancellationToken), "Draft pemanfaatan berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [AccessAction("Create", "Create Asset Utilization", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetUtilization", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] AssetUtilizationUpsertRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, cancellationToken), "Draft pemanfaatan berhasil diubah.");

    [HttpPost("{id:guid}/agreement")]
    [Consumes("multipart/form-data")]
    [AccessAction("Create", "Create Asset Utilization", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetUtilization", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UploadAgreement(Guid id, [FromForm] AssetUtilizationAgreementRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UploadAgreementAsync(id, request, cancellationToken), "Berkas perjanjian berhasil diunggah.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Asset Utilization", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetUtilization", "Submit")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SubmitAsync(id, cancellationToken), "Pemanfaatan berhasil diajukan.");

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Asset Utilization", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetUtilization", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Approve(Guid id, [FromBody] AssetUtilizationDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.ApproveAsync(id, request, cancellationToken),
            request?.Forward == true ? "Pemanfaatan diteruskan ke Direktur." : "Pemanfaatan disetujui.");

    [HttpPost("{id:guid}/approve-director")]
    [AccessAction("ApproveDirector", "Approve Asset Utilization as Director", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetUtilization", "ApproveDirector")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ApproveDirector(Guid id, [FromBody] AssetUtilizationDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.ApproveDirectorAsync(id, request, cancellationToken),
            request?.Accept == false ? "Pemanfaatan ditolak Direktur." : "Pemanfaatan disetujui Direktur.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Asset Utilization", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetUtilization", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Reject(Guid id, [FromBody] AssetUtilizationDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(id, request, cancellationToken), "Pemanfaatan ditolak.");

    [HttpPost("{id:guid}/end")]
    [AccessAction("End", "End Asset Utilization", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetUtilization", "End")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> End(Guid id, [FromBody] AssetUtilizationDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.EndAsync(id, request, cancellationToken), "Pemanfaatan diakhiri.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Create", "Create Asset Utilization", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetUtilization", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetUtilizationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] AssetUtilizationDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Pemanfaatan dibatalkan.");
}
