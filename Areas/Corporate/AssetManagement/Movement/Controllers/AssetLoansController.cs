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
[Route("api/v1/corporate/asset-management/movement/loans")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_MOVEMENT", "Corporate Asset Management Movement", "Asset Loan",
    AreaName = "Corporate", ControllerName = "AssetLoan", Description = "Peminjaman aset sementara antar unit, pengembalian, dan perpanjangan dengan persetujuan Kepala Unit pemilik", SortOrder = 2)]
[Tags("Corporate / Asset Management / Movement / Asset Loan")]
public sealed class AssetLoansController : AssetMasterControllerBase
{
    private readonly AssetLoanService _service;

    public AssetLoansController(AssetLoanService service)
    {
        _service = service;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Loan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLoan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(AssetLoanService.GetMetadata()), "Metadata daftar pinjaman berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Loan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLoan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan pinjaman berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Loan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLoan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetLoanResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] AssetLoanPagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLoansAsync(request, cancellationToken), "Daftar pinjaman berhasil diambil.");

    [HttpGet("loanable-assets")]
    [AccessAction("Create", "Create Asset Loan", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetLoan", "Create")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssetLoanableAssetResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetLoanableAssets([FromQuery] AssetLoanableAssetQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLoanableAssetsAsync(request, cancellationToken), "Daftar aset yang dapat dipinjam berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Loan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLoan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetLoanAsync(id, cancellationToken), "Rincian pinjaman berhasil diambil.");

    [HttpGet("{id:guid}/document")]
    [AccessAction("Read", "Read Asset Loan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetLoan", "Read")]
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
    [AccessAction("Create", "Create Asset Loan", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetLoan", "Create")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] AssetLoanCreateRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, cancellationToken), "Pinjaman berhasil diajukan.");

    [HttpPost("{id:guid}/decide")]
    [AccessAction("Approve", "Approve Asset Loan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetLoan", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Decide(Guid id, [FromBody] AssetLoanDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DecideAsync(id, request, cancellationToken), request.Approve ? "Pinjaman disetujui." : "Pinjaman ditolak.");

    [HttpPost("{id:guid}/pickup")]
    [AccessAction("Receive", "Pick Up Asset Loan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("AssetLoan", "Receive")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Pickup(Guid id, [FromBody] AssetLoanStepRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.PickupAsync(id, request, cancellationToken), "Aset pinjaman sudah diambil.");

    [HttpPost("{id:guid}/return")]
    [AccessAction("Return", "Return Asset Loan", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetLoan", "Return")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Return(Guid id, [FromBody] AssetLoanStepRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReturnAsync(id, request, cancellationToken), "Aset pinjaman diserahkan kembali; menunggu konfirmasi unit pemilik.");

    [HttpPost("{id:guid}/confirm-return")]
    [AccessAction("ConfirmReturn", "Confirm Asset Loan Return", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("AssetLoan", "ConfirmReturn")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ConfirmReturn(Guid id, [FromBody] AssetLoanReturnRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ConfirmReturnAsync(id, request, cancellationToken), "Pengembalian aset dikonfirmasi.");

    [HttpPost("{id:guid}/extensions")]
    [AccessAction("Extend", "Request Asset Loan Extension", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("AssetLoan", "Extend")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> RequestExtension(Guid id, [FromBody] AssetLoanExtensionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RequestExtensionAsync(id, request, cancellationToken), "Permintaan perpanjangan dikirim.");

    [HttpPost("{id:guid}/extensions/{extensionId:guid}/decide")]
    [AccessAction("Approve", "Approve Asset Loan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetLoan", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> DecideExtension(Guid id, Guid extensionId, [FromBody] AssetLoanDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DecideExtensionAsync(id, extensionId, request, cancellationToken), request.Approve ? "Perpanjangan disetujui." : "Perpanjangan ditolak.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Asset Loan", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("AssetLoan", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<AssetLoanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] AssetLoanReasonRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Pinjaman dibatalkan.");
}
