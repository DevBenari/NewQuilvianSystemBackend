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
    /// Stok yang sudah menyentuh reorder point pada kebijakan stok per lokasi.
    /// </summary>
    /// <remarks>
    /// Hanya membaca. Yang dibandingkan adalah saldo fisik berstatus siap pakai terhadap reorder
    /// point kebijakan aktif yang menunjuk satu lokasi. Kebijakan yang satuannya berbeda dari
    /// satuan stok obat ikut tampil dengan <c>isUnitMismatch = true</c> tanpa dibandingkan.
    /// </remarks>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugLowStockResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Memantau stok obat yang menyentuh reorder point", AccessType = AccessTypes.Read, SortOrder = 10)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetLowStock([FromQuery] DrugLowStockQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetLowStockAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugLowStockResponse>>.Ok(data,
            "Pemantauan stok di bawah reorder point berhasil diambil."));
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

    // ================================================ laporan Logistik (FIN-LGS-024)

    /// <summary>
    /// Laporan Persediaan: saldo saat ini per obat × lokasi, dengan kolom siap pakai, ditahan,
    /// karantina, kedaluwarsa, dan rusak dalam satuan stok obat.
    /// </summary>
    /// <remarks>
    /// Tanpa nilai rupiah: harga per batch belum disimpan (<c>LGS-CAP-32</c>). Ringkasan berupa
    /// hitungan baris, bukan penjumlahan jumlah lintas obat.
    /// </remarks>
    [HttpGet("reports/inventory")]
    [ProducesResponseType(typeof(ApiResponse<InventoryReportResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat laporan persediaan", AccessType = AccessTypes.Read, SortOrder = 11)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] InventoryReportQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetInventoryReportAsync(query, cancellationToken);
        return Ok(ApiResponse<InventoryReportResponse>.Ok(data, "Laporan persediaan berhasil diambil."));
    }

    /// <summary>
    /// Laporan Mutasi Barang: barang masuk, keluar, saldo pembuka, dan penyesuaian per obat × lokasi
    /// dalam periode (kosong = 30 hari terakhir, paling panjang 366 hari).
    /// </summary>
    [HttpGet("reports/mutations")]
    [ProducesResponseType(typeof(ApiResponse<MutationReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat laporan mutasi barang", AccessType = AccessTypes.Read, SortOrder = 12)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetMutationReport([FromQuery] MutationReportQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.GetMutationReportAsync(query, cancellationToken);
            return Ok(ApiResponse<MutationReportResponse>.Ok(data, "Laporan mutasi barang berhasil diambil."));
        }
        catch (LogisticsReportUnprocessableException ex) { return this.LogisticsReportUnprocessable(ex); }
    }

    /// <summary>
    /// Dashboard Penerimaan &amp; Pengeluaran: transaksi masuk/keluar per sumber dokumen dan per hari,
    /// serta obat dengan jumlah masuk/keluar terbesar.
    /// </summary>
    [HttpGet("reports/flow-summary")]
    [ProducesResponseType(typeof(ApiResponse<StockFlowSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat dashboard penerimaan dan pengeluaran", AccessType = AccessTypes.Read, SortOrder = 13)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetStockFlowSummary([FromQuery] StockFlowSummaryQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.GetStockFlowSummaryAsync(query, cancellationToken);
            return Ok(ApiResponse<StockFlowSummaryResponse>.Ok(data,
                "Ringkasan penerimaan dan pengeluaran berhasil diambil."));
        }
        catch (LogisticsReportUnprocessableException ex) { return this.LogisticsReportUnprocessable(ex); }
    }

    /// <summary>
    /// Pemeriksaan kelengkapan buku mutasi: saldo fisik tersimpan dibandingkan dengan jumlah semua
    /// mutasi per batch × status pada satu lokasi. Hanya membaca; tidak mengoreksi saldo.
    /// </summary>
    /// <remarks>Lokasi wajib (422 <c>PHM036</c>). Bawaan hanya mengembalikan baris yang tidak cocok.</remarks>
    [HttpGet("reports/ledger-consistency")]
    [ProducesResponseType(typeof(ApiResponse<LedgerConsistencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Memeriksa kelengkapan buku mutasi terhadap saldo", AccessType = AccessTypes.Read, SortOrder = 14)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetLedgerConsistency([FromQuery] LedgerConsistencyQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.GetLedgerConsistencyAsync(query, cancellationToken);
            return Ok(ApiResponse<LedgerConsistencyResponse>.Ok(data,
                "Pemeriksaan buku mutasi berhasil dijalankan."));
        }
        catch (LogisticsReportUnprocessableException ex) { return this.LogisticsReportUnprocessable(ex); }
    }

    /// <summary>
    /// Mutasi Saldo Persediaan (FIN-LGS-056): saldo awal, masuk, keluar, penyesuaian, dan saldo akhir
    /// per obat × lokasi dengan rincian batch. Saldo awal dihitung mundur dari saldo tersimpan, hanya
    /// untuk buku mutasi yang cocok; lainnya "Tidak dapat dihitung".
    /// </summary>
    /// <remarks>
    /// Lokasi wajib (422 <c>PHM036</c>); periode bawaan 30 hari, paling panjang 366 hari (422
    /// <c>PHM035</c>). Status "Valid" hanya untuk lokasi yang tercantum di konfigurasi
    /// <c>PharmacyManagement:Logistics:LedgerVerifiedStorageLocations</c> sesudah UAT buku mutasi.
    /// </remarks>
    [HttpGet("reports/balance-movement")]
    [ProducesResponseType(typeof(ApiResponse<BalanceMovementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat mutasi saldo persediaan", AccessType = AccessTypes.Read, SortOrder = 15)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetBalanceMovement([FromQuery] BalanceMovementQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.GetBalanceMovementAsync(query, cancellationToken);
            return Ok(ApiResponse<BalanceMovementResponse>.Ok(data,
                "Mutasi saldo persediaan berhasil diambil."));
        }
        catch (LogisticsReportUnprocessableException ex) { return this.LogisticsReportUnprocessable(ex); }
    }

    /// <summary>
    /// Rekomendasi Pengisian Ulang (FIN-LGS-057): usulan = maksimum − siap pakai (dibulatkan ke atas)
    /// untuk kebijakan stok yang valid dan siap pakainya ≤ titik pesan ulang. Hanya membaca; tidak
    /// membuat permintaan barang maupun PO.
    /// </summary>
    [HttpGet("reports/replenishment")]
    [ProducesResponseType(typeof(ApiResponse<ReplenishmentResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Melihat rekomendasi pengisian ulang", AccessType = AccessTypes.Read, SortOrder = 16)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> GetReplenishment([FromQuery] ReplenishmentQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetReplenishmentAsync(query, cancellationToken);
        return Ok(ApiResponse<ReplenishmentResponse>.Ok(data, "Rekomendasi pengisian ulang berhasil diambil."));
    }

    /// <summary>
    /// Hitung selisih Stock Opname (FIN-LGS-084): membandingkan hasil hitung fisik per batch × status
    /// dengan saldo saat ini pada satu lokasi. Hanya membaca.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Memakai POST hanya karena lembar hitung bisa sampai 2000 baris; tidak ada yang disimpan
    /// (<c>isPersisted</c> selalu false), tidak ada mutasi, dan saldo tidak berubah. Karena itu izinnya
    /// <c>DrugStock:Read</c>. Penyimpanan sesi, persetujuan, penyesuaian, dan berita acara menunggu
    /// tabel Stock Opname.
    /// </para>
    /// <para>
    /// Baris yang saldonya berubah sejak lembar hitung dimuat (jumlah berbeda, atau ada mutasi sesudah
    /// <c>snapshotTakenAtUtc</c>) dijawab <c>NeedsRecount</c> tanpa selisih. Galat 422: <c>PHM036</c>
    /// lokasi kosong/tidak terdaftar, <c>PHM037</c> isian lembar hitung tidak sah, <c>PHM038</c> saldo
    /// tidak ditemukan atau bukan milik lokasi itu.
    /// </para>
    /// </remarks>
    [HttpPost("opname/variance-preview")]
    [ProducesResponseType(typeof(ApiResponse<StockOpnameVariancePreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Drug Stock",
        Description = "Menghitung selisih stock opname tanpa menyimpan", AccessType = AccessTypes.Read, SortOrder = 17)]
    [AccessPermission("DrugStock", "Read")]
    public async Task<IActionResult> PreviewOpnameVariance(
        [FromBody] StockOpnameVariancePreviewRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.PreviewOpnameVarianceAsync(request, cancellationToken);
            return Ok(ApiResponse<StockOpnameVariancePreviewResponse>.Ok(data,
                "Selisih stock opname berhasil dihitung. Hasil ini tidak disimpan."));
        }
        catch (LogisticsReportUnprocessableException ex) { return this.LogisticsReportUnprocessable(ex); }
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

    /// <summary>Periode laporan Logistik yang tidak sah (<c>PHM035</c>); dipakai seluruh endpoint laporan.</summary>
    public static ObjectResult LogisticsReportUnprocessable(this ControllerBase controller,
        LogisticsReportUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { exception.Code }));
}
