using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Perpindahan stok antar lokasi penyimpanan.
/// </summary>
/// <remarks>
/// Kedua ujung wajib lokasi penyimpanan yang diizinkan mengirim dan menerima. Unit pelayanan
/// seperti ICU atau kamar operasi bukan lokasi penyimpanan dan tidak pernah menjadi ujung
/// transfer; unit semacam itu memperoleh obat lewat permintaan kepada depo.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/stock-transfers")]
[Tags("Health Services / Pharmacy Management / Stock Transfer")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Stock Transfer",
    AreaName = "HealthServices",
    ControllerName = "StockTransfer",
    Description = "Transfer stok antar lokasi penyimpanan",
    SortOrder = 3)]
public class StockTransferController : ControllerBase
{
    private readonly StockTransferService _service;

    public StockTransferController(StockTransferService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockTransferSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Transfer",
        Description = "Melihat daftar transfer stok", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("StockTransfer", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] StockTransferPagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<StockTransferSummaryResponse>>.Ok(data,
            "Daftar transfer stok berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Stock Transfer",
        Description = "Melihat detail transfer stok", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("StockTransfer", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Transfer stok tidak ditemukan."))
            : Ok(ApiResponse<StockTransferDetailResponse>.Ok(data,
                "Detail transfer stok berhasil diambil."));
    }

    /// <summary>Membuat pengajuan transfer. Dibuat berstatus Draft.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Transfer",
        Description = "Membuat pengajuan transfer stok", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("StockTransfer", "Create")]
    public Task<IActionResult> Create([FromBody] CreateStockTransferRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, cancellationToken),
            "Transfer stok berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Transfer",
        Description = "Mengubah transfer stok yang masih draft", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("StockTransfer", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateStockTransferRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.UpdateAsync(id, request, cancellationToken),
            "Transfer stok berhasil diperbarui.");

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Transfer",
        Description = "Mengajukan transfer stok", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("StockTransfer", "Update")]
    public Task<IActionResult> Submit(Guid id, [FromBody] StockTransferCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.SubmitAsync(id, request, cancellationToken),
            "Transfer stok berhasil diajukan.");

    /// <summary>
    /// Menyetujui transfer dan menahan stok di lokasi asal.
    /// </summary>
    /// <remarks>
    /// Batch dipilih di sini menurut kedaluwarsa terdekat lalu ditahan, sehingga stok yang
    /// dijanjikan tidak dapat diambil proses lain selagi barangnya disiapkan.
    /// </remarks>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Transfer",
        Description = "Menyetujui atau menolak transfer stok", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("StockTransfer", "Approve")]
    public Task<IActionResult> Approve(Guid id, [FromBody] StockTransferCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.ApproveAsync(id, request, cancellationToken),
            "Transfer stok disetujui dan stok telah ditahan.");

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Transfer",
        Description = "Menolak transfer stok", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("StockTransfer", "Approve")]
    public Task<IActionResult> Reject(Guid id, [FromBody] StockTransferReasonRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.RejectAsync(id, request, cancellationToken),
            "Transfer stok berhasil ditolak.");

    /// <summary>Mengeluarkan barang dari lokasi asal; sesudah ini barang sedang di jalan.</summary>
    [HttpPost("{id:guid}/issue")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Issue", "Issue Stock Transfer",
        Description = "Mengeluarkan barang transfer dari lokasi asal", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("StockTransfer", "Issue")]
    public Task<IActionResult> Issue(Guid id, [FromBody] StockTransferCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.IssueAsync(id, request, cancellationToken),
            "Barang transfer berhasil dikeluarkan dari lokasi asal.");

    /// <summary>
    /// Mencatat penerimaan di lokasi tujuan, baris per baris.
    /// </summary>
    /// <remarks>
    /// Jumlah diterima boleh lebih kecil daripada yang dikirim; selisihnya adalah barang yang
    /// hilang atau rusak di perjalanan, dan sengaja dibiarkan terbaca.
    /// </remarks>
    [HttpPost("{id:guid}/receive")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Receive", "Receive Stock Transfer",
        Description = "Mencatat penerimaan transfer di lokasi tujuan", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("StockTransfer", "Receive")]
    public Task<IActionResult> Receive(Guid id, [FromBody] ReceiveStockTransferRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.ReceiveAsync(id, request, cancellationToken),
            "Penerimaan transfer berhasil dicatat.");

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<StockTransferDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Stock Transfer",
        Description = "Membatalkan transfer stok", AccessType = AccessTypes.Update, SortOrder = 10)]
    [AccessPermission("StockTransfer", "Cancel")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] StockTransferReasonRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request, cancellationToken),
            "Transfer stok berhasil dibatalkan.");

    /// <summary>
    /// Membungkus pemetaan kegagalan domain ke kode HTTP satu kali untuk seluruh perintah.
    /// </summary>
    /// <remarks>
    /// Sembilan perintah memakai pemetaan yang sama persis. Menuliskannya berulang di setiap
    /// aksi membuat salah satunya cepat atau lambat menyimpang tanpa ketahuan.
    /// </remarks>
    private async Task<IActionResult> Run(Func<Task<StockTransferDetailResponse>> action,
        string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<StockTransferDetailResponse>.Ok(data, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockTransferForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (StockTransferConflictException ex)
        {
            return StatusCode(StatusCodes.Status409Conflict,
                ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message,
                    new { ex.Code }));
        }
        catch (StockTransferUnprocessableException ex)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity,
                ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message,
                    new { ex.Code }));
        }
    }
}
