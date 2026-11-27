using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Transaksi Stock Opname Logistik (FIN-LGS-091): sesi, hasil hitung, temuan, pengajuan, keputusan,
/// dan berita acara.
/// </summary>
/// <remarks>
/// <para>
/// Alur: buat sesi (snapshot saldo lokasi) → isi hasil hitung → ajukan → setujui (penyesuaian diposting
/// sekali ke kartu stok) atau tolak (final) → berita acara. Setiap perintah tulis membawa
/// <c>expectedVersion</c> sesi.
/// </para>
/// <para>
/// Tabel <c>PhmStockOpname</c> dan <c>PhmStockOpnameLine</c> dibuat lewat migrasi oleh pihak
/// berwenang. Sebelum itu setiap endpoint (kecuali <c>availability</c>) menjawab <c>503</c> dengan kode
/// <c>PHM039</c> (BLOCKED_SCHEMA), dan halaman memakai mode terbatas.
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/stock-opnames")]
[Tags("Health Services / Pharmacy Management / Stock Opname")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Stock Opname",
    AreaName = "HealthServices",
    ControllerName = "StockOpname",
    Description = "Stock opname gudang dan lokasi penyimpanan",
    SortOrder = 17)]
public class StockOpnameController : ControllerBase
{
    private readonly StockOpnameService _service;

    public StockOpnameController(StockOpnameService service) => _service = service;

    // ======================================================================== baca

    /// <summary>Kesiapan penyimpanan Stock Opname (<c>READY</c> atau <c>BLOCKED_SCHEMA</c>).</summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameAvailabilityResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Opname",
        Description = "Melihat kesiapan penyimpanan stock opname", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("StockOpname", "Read")]
    public async Task<IActionResult> GetAvailability(CancellationToken cancellationToken)
    {
        var data = await _service.GetAvailabilityAsync(cancellationToken);
        return Ok(ApiResponse<StockOpnameAvailabilityResponse>.Ok(data, data.Message));
    }

    /// <summary>Daftar sesi Stock Opname (terbaru lebih dahulu).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockOpnameSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Opname",
        Description = "Melihat daftar sesi stock opname", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("StockOpname", "Read")]
    public Task<IActionResult> GetList([FromQuery] StockOpnameListQuery query, CancellationToken cancellationToken) =>
        Run(() => _service.GetListAsync(query, cancellationToken), "Daftar stock opname berhasil diambil.");

    /// <summary>Detail sesi beserta baris hitung, hasil terhadap saldo saat ini, dan tindakan yang tersedia.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Opname",
        Description = "Melihat detail sesi stock opname", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("StockOpname", "Read")]
    public Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken) =>
        Run(async () => await _service.GetDetailAsync(id, cancellationToken)
                        ?? throw new StockOpnameNotFoundException("Sesi Stock Opname tidak ditemukan."),
            "Detail stock opname berhasil diambil.");

    /// <summary>Berita acara; hanya untuk sesi berstatus Selesai (409 <c>PHM052</c> untuk status lain).</summary>
    [HttpGet("{id:guid}/report")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameReportResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Opname",
        Description = "Melihat berita acara stock opname", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("StockOpname", "Read")]
    public Task<IActionResult> GetReport(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.GetReportAsync(id, cancellationToken), "Berita acara stock opname berhasil diambil.");

    // ======================================================================== hitung

    /// <summary>Membuat sesi dan memotret seluruh saldo lokasi sebagai baris hitung. Satu sesi aktif per lokasi.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 5)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> Create([FromBody] CreateStockOpnameRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, cancellationToken), "Sesi stock opname berhasil dibuat.");

    /// <summary>Menyimpan hasil hitung baris yang dikirim.</summary>
    [HttpPut("{id:guid}/counts")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> SaveCounts(Guid id, [FromBody] SaveStockOpnameCountsRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.SaveCountsAsync(id, request, cancellationToken), "Hasil hitung berhasil disimpan.");

    /// <summary>Memotret ulang satu baris yang saldonya berubah; hasil hitung baris itu dikosongkan.</summary>
    [HttpPost("{id:guid}/lines/{lineId:guid}/refresh")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 7)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> RefreshLine(Guid id, Guid lineId, [FromBody] StockOpnameCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.RefreshLineAsync(id, lineId, request, cancellationToken), "Baris berhasil dimuat ulang.");

    /// <summary>Mencatat temuan tanpa saldo; tidak menambah stok.</summary>
    [HttpPost("{id:guid}/findings")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 8)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> AddFinding(Guid id, [FromBody] AddStockOpnameFindingRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.AddFindingAsync(id, request, cancellationToken), "Temuan berhasil dicatat.");

    /// <summary>Menghapus temuan yang salah catat selama sesi dihitung.</summary>
    [HttpDelete("{id:guid}/findings/{lineId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 9)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> RemoveFinding(Guid id, Guid lineId, [FromQuery] int? expectedVersion,
        CancellationToken cancellationToken) =>
        Run(() => _service.RemoveFindingAsync(id, lineId, expectedVersion, cancellationToken), "Temuan berhasil dihapus.");

    /// <summary>Mengajukan sesi untuk verifikasi (422 <c>PHM054</c> bila ada baris belum dihitung atau berubah).</summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 10)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> Submit(Guid id, [FromBody] StockOpnameCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.SubmitAsync(id, request, cancellationToken), "Stock opname berhasil diajukan.");

    /// <summary>Membatalkan sesi yang belum diajukan.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Opname",
        Description = "Membuat, menghitung, dan mengajukan stock opname", AccessType = AccessTypes.Create, SortOrder = 11)]
    [AccessPermission("StockOpname", "Create")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] StockOpnameCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request, cancellationToken), "Stock opname berhasil dibatalkan.");

    // ======================================================================== keputusan

    /// <summary>
    /// Menyetujui dan memposting setiap selisih ke kartu stok dalam satu transaksi. Penyetuju wajib
    /// memegang <c>StockOpname:Approve</c> secara eksplisit dan bukan pembuat, pengaju, maupun penghitung.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Opname",
        Description = "Menyetujui atau menolak stock opname", AccessType = AccessTypes.Update, SortOrder = 12)]
    [AccessPermission("StockOpname", "Approve")]
    public Task<IActionResult> Approve(Guid id, [FromBody] DecideStockOpnameRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.ApproveAsync(id, request, cancellationToken),
            "Stock opname disetujui dan penyesuaian diposting ke kartu stok.");

    /// <summary>Menolak sesi dengan alasan wajib; final.</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Opname",
        Description = "Menyetujui atau menolak stock opname", AccessType = AccessTypes.Update, SortOrder = 13)]
    [AccessPermission("StockOpname", "Approve")]
    public Task<IActionResult> Reject(Guid id, [FromBody] DecideStockOpnameRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.RejectAsync(id, request, cancellationToken), "Stock opname ditolak.");

    // ======================================================================== bantuan

    private async Task<IActionResult> Run<T>(Func<Task<T>> action, string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<T>.Ok(data, message));
        }
        catch (StockOpnameSchemaUnavailableException ex)
        {
            return Fail(StatusCodes.Status503ServiceUnavailable, ex.Message, ex.Code,
                StockOpnameAvailabilityStatuses.BlockedSchema);
        }
        catch (StockOpnameNotFoundException ex)
        {
            return Fail(StatusCodes.Status404NotFound, ex.Message, ex.Code);
        }
        catch (StockOpnameForbiddenException ex)
        {
            return Fail(StatusCodes.Status403Forbidden, ex.Message, ex.Code);
        }
        catch (StockOpnameConflictException ex)
        {
            return Fail(StatusCodes.Status409Conflict, ex.Message, ex.Code);
        }
        catch (StockOpnameUnprocessableException ex)
        {
            return Fail(StatusCodes.Status422UnprocessableEntity, ex.Message, ex.Code);
        }
        catch (DrugStockUnprocessableException ex)
        {
            // Aturan saldo saat posting: tidak boleh negatif (PHM028), tidak di bawah yang ditahan (PHM029).
            return Fail(StatusCodes.Status422UnprocessableEntity, ex.Message, ex.Code);
        }
        catch (DrugStockForbiddenException ex)
        {
            return Fail(StatusCodes.Status403Forbidden, ex.Message, null);
        }
    }

    private ObjectResult Fail(int statusCode, string message, string? code, string? status = null) =>
        StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message, new { Code = code, Status = status }));
}
