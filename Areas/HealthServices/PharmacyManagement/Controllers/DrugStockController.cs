using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Pemantauan persediaan obat: saldo per lokasi, batch, kedaluwarsa, dan kartu stok.
/// </summary>
/// <remarks>
/// <para>
/// Seluruh endpoint di sini hanya membaca. Perubahan stok tidak pernah datang dari layar
/// pemantauan; ia lahir dari dokumen yang menyebabkannya — penyerahan permintaan, penerimaan
/// barang, pemakaian, atau retur — sehingga setiap pergerakan selalu punya sebab yang dapat
/// ditelusuri.
/// </para>
/// <para>
/// Istilah yang dipakai adalah <em>lokasi penyimpanan</em>, bukan depo. Penetapan lokasi mana
/// yang berstatus depo dengan saldo sendiri belum diputuskan, dan menuliskannya lebih awal di
/// nama endpoint akan mengunci arti yang belum tentu benar.
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/drug-stock")]
[Tags("Health Services / Pharmacy Management / Drug Stock")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Drug Stock",
    AreaName = "HealthServices",
    ControllerName = "DrugStock",
    Description = "Pemantauan persediaan obat",
    SortOrder = 2)]
public class DrugStockController : ControllerBase
{
    private readonly DrugStockService _service;

    public DrugStockController(DrugStockService service) => _service = service;

    /// <summary>
    /// Ringkasan stok per obat pada satu lokasi.
    /// </summary>
    /// <remarks>
    /// Satu baris untuk setiap obat, lokasi, dan status. Rincian batchnya dibaca lewat
    /// <c>GET /balances</c>.
    /// </remarks>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugStockSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat ringkasan stok obat per lokasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetSummary([FromQuery] DrugStockSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetSummaryAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugStockSummaryResponse>>.Ok(data,
            "Ringkasan stok obat berhasil diambil."));
    }

    /// <summary>
    /// Saldo stok per batch pada satu lokasi, lengkap dengan yang tertahan dan yang tersedia.
    /// </summary>
    /// <remarks>
    /// Jumlah tersedia dihitung backend sebagai saldo fisik dikurangi yang sedang ditahan,
    /// supaya layar tidak perlu menyalin rumusnya dan tidak dapat menyimpang darinya.
    /// </remarks>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugStockBalanceResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat saldo stok obat per batch dan lokasi", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetBalances([FromQuery] DrugStockBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetBalancesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugStockBalanceResponse>>.Ok(data,
            "Saldo stok obat berhasil diambil."));
    }

    /// <summary>Daftar batch obat beserta kedaluwarsa dan asal pemasoknya.</summary>
    [HttpGet("batches")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugBatchResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat daftar batch obat", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetBatches([FromQuery] DrugBatchQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetBatchesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugBatchResponse>>.Ok(data,
            "Daftar batch obat berhasil diambil."));
    }

    /// <summary>
    /// Stok yang mendekati atau sudah lewat kedaluwarsa.
    /// </summary>
    /// <remarks>
    /// Hanya menampilkan yang saldonya masih ada, karena batch kosong tidak menuntut tindakan
    /// apa pun. Tanpa <c>expiringWithinDays</c>, ambang bawaannya sembilan puluh hari.
    /// </remarks>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugStockBalanceResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Memantau stok obat mendekati kedaluwarsa", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetExpiring([FromQuery] DrugStockBalanceQuery query,
        CancellationToken cancellationToken)
    {
        query.OnlyInStock = true;
        if (query.OnlyExpired != true && !query.ExpiringWithinDays.HasValue)
            query.ExpiringWithinDays = 90;

        var data = await _service.GetBalancesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugStockBalanceResponse>>.Ok(data,
            "Pemantauan kedaluwarsa berhasil diambil."));
    }

    /// <summary>
    /// Kartu stok: seluruh pergerakan yang pernah terjadi.
    /// </summary>
    /// <remarks>
    /// Barisnya tidak pernah diubah dan tidak pernah dihapus. Kekeliruan tampak sebagai baris
    /// koreksi yang menunjuk baris aslinya, sehingga apa yang pernah tercatat tetap terbaca
    /// sebagaimana adanya saat itu.
    /// </remarks>
    [HttpGet("mutations")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugStockMutationResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat kartu stok obat", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetMutations([FromQuery] DrugStockMutationQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetMutationsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugStockMutationResponse>>.Ok(data,
            "Kartu stok berhasil diambil."));
    }

    // ======================================================== perintah persediaan

    /// <summary>
    /// Mencatat saldo pembuka satu batch pada satu lokasi.
    /// </summary>
    /// <remarks>
    /// Dipakai ketika stok yang sudah ada di rak pertama kali dicatat sistem. Ini bukan jalan
    /// masuk pembelian: barang dari pemasok wajib melalui penerimaan barang resmi, supaya
    /// nilai dan dokumennya ikut tercatat.
    /// </remarks>
    [HttpPost("opening-balance")]
    [ProducesResponseType(typeof(ApiResponse<DrugStockBalanceResponse>), StatusCodes.Status200OK)]
    [AccessAction("Adjust", "Adjust Drug Stock",
        Description = "Mencatat saldo pembuka stok obat", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("DrugStock", "Adjust")]
    public async Task<IActionResult> RecordOpeningBalance(
        [FromBody] RecordOpeningBalanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.RecordOpeningBalanceAsync(request, cancellationToken);
            return Ok(ApiResponse<DrugStockBalanceResponse>.Ok(data,
                "Saldo pembuka berhasil dicatat."));
        }
        catch (DrugStockForbiddenException ex) { return this.DrugStockForbidden(ex); }
        catch (DrugStockConflictException ex) { return this.DrugStockConflict(ex); }
        catch (DrugStockUnprocessableException ex) { return this.DrugStockUnprocessable(ex); }
    }

    /// <summary>
    /// Menyesuaikan saldo di luar transaksi normal, misalnya hasil stock opname.
    /// </summary>
    /// <remarks>
    /// Alasan wajib diisi. Penyesuaian tanpa alasan tidak dapat diperiksa siapa pun kelak,
    /// dan selisih stok adalah hal yang paling sering dipersoalkan.
    /// </remarks>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse<DrugStockBalanceResponse>), StatusCodes.Status200OK)]
    [AccessAction("Adjust", "Adjust Drug Stock",
        Description = "Menyesuaikan saldo stok obat", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("DrugStock", "Adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.AdjustAsync(request, cancellationToken);
            return Ok(ApiResponse<DrugStockBalanceResponse>.Ok(data,
                "Saldo stok berhasil disesuaikan."));
        }
        catch (DrugStockForbiddenException ex) { return this.DrugStockForbidden(ex); }
        catch (DrugStockConflictException ex) { return this.DrugStockConflict(ex); }
        catch (DrugStockUnprocessableException ex) { return this.DrugStockUnprocessable(ex); }
    }

    /// <summary>
    /// Memindahkan sebagian stok ke status lain, misalnya menahannya di karantina.
    /// </summary>
    /// <remarks>
    /// Barangnya tidak berpindah lokasi; yang berpindah adalah keadaannya. Stok di luar
    /// status siap pakai tidak pernah ikut dilayankan, tetapi tetap tercatat sehingga
    /// selisihnya tidak hilang diam-diam.
    /// </remarks>
    [HttpPost("change-status")]
    [ProducesResponseType(typeof(ApiResponse<List<DrugStockBalanceResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Adjust", "Adjust Drug Stock",
        Description = "Memindahkan status stok obat", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("DrugStock", "Adjust")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStockStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.ChangeStatusAsync(request, cancellationToken);
            return Ok(ApiResponse<List<DrugStockBalanceResponse>>.Ok(data,
                "Status stok berhasil dipindahkan."));
        }
        catch (DrugStockForbiddenException ex) { return this.DrugStockForbidden(ex); }
        catch (DrugStockConflictException ex) { return this.DrugStockConflict(ex); }
        catch (DrugStockUnprocessableException ex) { return this.DrugStockUnprocessable(ex); }
    }

    /// <summary>
    /// Mengoreksi satu baris kartu stok yang terlanjur salah.
    /// </summary>
    /// <remarks>
    /// Baris lama tidak disentuh. Yang ditambahkan adalah baris baru sebesar selisih antara
    /// nilai yang seharusnya dan yang terlanjur tercatat, menunjuk baris asalnya — sehingga
    /// saldo menjadi benar tanpa menghapus jejak bahwa pernah terjadi kekeliruan.
    /// </remarks>
    [HttpPost("mutations/correct")]
    [ProducesResponseType(typeof(ApiResponse<DrugStockMutationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Correct", "Correct Drug Stock Mutation",
        Description = "Mengoreksi baris kartu stok", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("DrugStock", "Correct")]
    public async Task<IActionResult> CorrectMutation([FromBody] CorrectMutationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CorrectMutationAsync(request, cancellationToken);
            return Ok(ApiResponse<DrugStockMutationResponse>.Ok(data,
                "Koreksi kartu stok berhasil dicatat."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (DrugStockForbiddenException ex) { return this.DrugStockForbidden(ex); }
        catch (DrugStockConflictException ex) { return this.DrugStockConflict(ex); }
        catch (DrugStockUnprocessableException ex) { return this.DrugStockUnprocessable(ex); }
    }
}

/// <summary>
/// Pemetaan tunggal kegagalan persediaan ke kode HTTP: `403` tidak berwenang, `409` benturan
/// atau keadaan tidak sah, `422` prasyarat aturan belum terpenuhi.
/// </summary>
internal static class DrugStockControllerResults
{
    public static ObjectResult DrugStockForbidden(this ControllerBase controller,
        DrugStockForbiddenException exception) =>
        controller.StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));

    public static ObjectResult DrugStockConflict(this ControllerBase controller,
        DrugStockConflictException exception) =>
        controller.StatusCode(StatusCodes.Status409Conflict,
            ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message,
                new { exception.Code }));

    public static ObjectResult DrugStockUnprocessable(this ControllerBase controller,
        DrugStockUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { exception.Code }));
}
