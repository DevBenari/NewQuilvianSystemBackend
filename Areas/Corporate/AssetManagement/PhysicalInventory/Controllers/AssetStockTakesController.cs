using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/asset-management/physical-inventory/stock-takes")]
[AccessController("CORPORATE_ASSET_MANAGEMENT_PHYSICAL_INVENTORY", "Corporate Asset Management Physical Inventory", "Asset Stock Take",
    AreaName = "Corporate", ControllerName = "AssetStockTake", Description = "Jadwal dan sesi inventarisasi fisik aset per unit beserta tim pemeriksa", SortOrder = 1)]
[Tags("Corporate / Asset Management / Physical Inventory / Stock Take")]
public sealed class AssetStockTakesController : AssetMasterControllerBase
{
    private readonly StockTakeSessionService _service;
    private readonly StockTakeScanService _scanService;

    public AssetStockTakesController(StockTakeSessionService service, StockTakeScanService scanService)
    {
        _service = service;
        _scanService = scanService;
    }

    [HttpGet("{id:guid}/expected-items")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeExpectedItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetExpectedItems(Guid id, [FromQuery] StockTakeItemQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _scanService.GetExpectedItemsAsync(id, request, cancellationToken), "Daftar aset diharapkan berhasil diambil.");

    [HttpGet("{id:guid}/scans")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeScanResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetScans(Guid id, [FromQuery] StockTakeItemQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _scanService.GetScansAsync(id, request, cancellationToken), "Riwayat pindaian berhasil diambil.");

    [HttpGet("{id:guid}/results")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeResultResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetResults(Guid id, [FromQuery] StockTakeItemQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetResultsAsync(id, request, cancellationToken), "Hasil pencocokan berhasil diambil.");

    [HttpGet("{id:guid}/lookup")]
    [AccessAction("Scan", "Scan Asset Stock Take", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("AssetStockTake", "Scan")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeLookupResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Lookup(Guid id, [FromQuery] string? code, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _scanService.LookupAsync(id, code, cancellationToken), "Aset ditemukan.");

    [HttpPost("{id:guid}/scans")]
    [Consumes("multipart/form-data")]
    [AccessAction("Scan", "Scan Asset Stock Take", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("AssetStockTake", "Scan")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeScanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Scan(Guid id, [FromForm] StockTakeScanRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _scanService.ScanAsync(id, request, cancellationToken), "Pindaian tersimpan.");

    [HttpPost("{id:guid}/findings")]
    [Consumes("multipart/form-data")]
    [AccessAction("Scan", "Scan Asset Stock Take", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("AssetStockTake", "Scan")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeScanResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> RecordFinding(Guid id, [FromForm] StockTakeUnregisteredFindingRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _scanService.RecordFindingAsync(id, request, cancellationToken), "Temuan tidak terdaftar tersimpan.");

    [HttpPost("{id:guid}/close-counting")]
    [AccessAction("Close", "Close Asset Stock Take Counting", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetStockTake", "Close")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> CloseCounting(Guid id, [FromBody] StockTakeVersionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CloseCountingAsync(id, request, cancellationToken), "Pemindaian ditutup; hasil pencocokan sudah dihitung.");

    [HttpPost("{id:guid}/reopen-counting")]
    [AccessAction("Close", "Close Asset Stock Take Counting", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("AssetStockTake", "Close")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ReopenCounting(Guid id, [FromBody] StockTakeDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReopenCountingAsync(id, request, cancellationToken), "Pemindaian dibuka kembali.");

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AssetMasterFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetMetadata() =>
        ExecuteAsync(() => Task.FromResult(StockTakeSessionService.GetMetadata()), "Metadata daftar sesi inventarisasi berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan sesi inventarisasi berhasil diambil.");

    [HttpGet]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTakeSessionResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> Get([FromQuery] StockTakePagedQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSessionsAsync(request, cancellationToken), "Daftar sesi inventarisasi berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Asset Stock Take", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AssetStockTake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSessionAsync(id, cancellationToken), "Rincian sesi inventarisasi berhasil diambil.");

    [HttpPost]
    [AccessAction("Create", "Schedule Asset Stock Take", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetStockTake", "Create")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] StockTakeScheduleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ScheduleAsync(request, cancellationToken), "Sesi inventarisasi berhasil dijadwalkan.");

    [HttpPut("{id:guid}")]
    [AccessAction("Create", "Schedule Asset Stock Take", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("AssetStockTake", "Create")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] StockTakeScheduleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, cancellationToken), "Jadwal sesi inventarisasi berhasil diubah.");

    [HttpPost("{id:guid}/start")]
    [AccessAction("Start", "Start Asset Stock Take", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("AssetStockTake", "Start")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Start(Guid id, [FromBody] StockTakeVersionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.StartAsync(id, request, cancellationToken), "Sesi inventarisasi dimulai; daftar aset diharapkan sudah dibekukan.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Asset Stock Take", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("AssetStockTake", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<StockTakeSessionResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Cancel(Guid id, [FromBody] StockTakeDecisionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken), "Sesi inventarisasi dibatalkan.");
}
