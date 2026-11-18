using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Permintaan stok barang dan obat dari unit layanan kepada gudang farmasi.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/stock-requests")]
[Tags("Health Services / Pharmacy Management / Stock Request")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Stock Request",
    AreaName = "HealthServices",
    ControllerName = "StockRequest",
    Description = "Permintaan stok barang dan obat",
    SortOrder = 1)]
public class StockRequestController : ControllerBase
{
    private readonly StockRequestService _service;

    public StockRequestController(StockRequestService service) => _service = service;

    /// <summary>
    /// Riwayat permintaan obat, dengan pencarian dan penyaringan.
    /// </summary>
    /// <remarks>
    /// Pencarian mengenai nomor permintaan, nama unit peminta, dan nama serta kode obat
    /// yang ada di dalamnya — sehingga petugas dapat menemukan permintaan lewat obat yang
    /// diingatnya, bukan hanya lewat nomor.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockRequestSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Request",
        Description = "Melihat riwayat permintaan obat", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("StockRequest", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] StockRequestPagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<StockRequestSummaryResponse>>.Ok(data,
            "Riwayat permintaan obat berhasil diambil."));
    }

    /// <summary>
    /// Cakupan kewenangan pengguna yang login: izin persetujuan eksplisit, unit layanan yang
    /// dipimpin, dan kewenangan CITO ("Unit yang Saya Pimpin").
    /// </summary>
    /// <remarks>
    /// Dihitung dari penempatan jabatan dan struktur organisasi pengguna sendiri; tidak menerima
    /// masukan unit dari klien dan tidak memuat identitas pegawai lain.
    /// </remarks>
    [HttpGet("approval-scope")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestApprovalScopeResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Request",
        Description = "Melihat cakupan kewenangan persetujuan sendiri", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("StockRequest", "Read")]
    public async Task<IActionResult> GetApprovalScope(CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.GetApprovalScopeAsync(cancellationToken);
            return Ok(ApiResponse<StockRequestApprovalScopeResponse>.Ok(data,
                "Cakupan kewenangan persetujuan berhasil diambil."));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
    }

    /// <summary>
    /// Kesiapan data organisasi dan konfigurasi persetujuan (hanya membaca): tautan unit,
    /// penempatan manajerial, izin jabatan, penunjukan jabatan CITO, batas aktivasi, dan tenggat.
    /// </summary>
    /// <remarks>
    /// Berisi jumlah dan identitas unit layanan, tanpa identitas pegawai. Izinnya terpisah
    /// (<c>StockRequest:ViewApprovalReadiness</c>) dan tidak diberikan kepada siapa pun secara
    /// bawaan.
    /// </remarks>
    [HttpGet("approval-readiness")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestApprovalReadinessResponse>), StatusCodes.Status200OK)]
    [AccessAction("ViewApprovalReadiness", "View Stock Request Approval Readiness",
        Description = "Melihat kesiapan data organisasi untuk persetujuan permintaan stok",
        AccessType = AccessTypes.Read, SortOrder = 14)]
    [AccessPermission("StockRequest", "ViewApprovalReadiness")]
    public async Task<IActionResult> GetApprovalReadiness(CancellationToken cancellationToken)
    {
        var data = await _service.GetApprovalReadinessAsync(cancellationToken);
        return Ok(ApiResponse<StockRequestApprovalReadinessResponse>.Ok(data,
            "Kesiapan data persetujuan berhasil diambil."));
    }

    /// <summary>Rincian satu permintaan beserta itemnya dan riwayat statusnya.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Stock Request",
        Description = "Melihat detail permintaan stok barang atau obat", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("StockRequest", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Permintaan stok tidak ditemukan."))
            : Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Detail permintaan stok berhasil diambil."));
    }

    /// <summary>Membuat permintaan beserta item dan jumlahnya. Dibuat berstatus Draft.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Request",
        Description = "Membuat permintaan stok barang atau obat", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("StockRequest", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CreateAsync(request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dibuat."));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Mengubah permintaan yang diizinkan, yaitu yang masih berstatus Draft.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Request",
        Description = "Mengubah permintaan stok barang atau obat", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("StockRequest", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>Mengirim permintaan ke gudang. Sesudah ini isinya tidak dapat diubah.</summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Request",
        Description = "Mengirim permintaan stok ke gudang", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("StockRequest", "Update")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.SubmitAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dikirim."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Mencatat penyerahan barang oleh gudang, baris per baris.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ini satu-satunya perintah milik gudang. Gudang tidak menyetujui dan tidak menolak
    /// permintaan; keputusan milik atasan unit atau petugas CITO berwenang. Sejak
    /// <c>LGS-DEC-002</c> gudang hanya dapat menyerahkan permintaan berstatus Disetujui;
    /// permintaan yang masih menunggu persetujuan ditolak (`409`, kode <c>PHM016</c>). Hak
    /// aksesnya dipisah dari hak akses peminta, supaya penyusun permintaan tidak dengan
    /// sendirinya boleh menutup permintaannya.
    /// </para>
    /// <para>
    /// Perintah ini mengurangi saldo gudang asal dan menulis kartu stok. Batch yang paling
    /// dekat kedaluwarsa diambil lebih dahulu. Bila stoknya tidak mencukupi, seluruh
    /// penyerahan dibatalkan dan permintaan tetap berstatus Disetujui — tidak ada permintaan
    /// yang tertutup padahal barangnya tidak pernah keluar.
    /// </para>
    /// <para>
    /// Ke mana barang mendarat belum dicatat sistem. Itu bagian dari kapabilitas transfer,
    /// yang menunggu penetapan lokasi mana saja yang memiliki saldo stok sendiri.
    /// </para>
    /// </remarks>
    [HttpPost("{id:guid}/fulfill")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Fulfill", "Fulfill Stock Request",
        Description = "Mencatat penyerahan barang atas permintaan stok", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("StockRequest", "Fulfill")]
    public async Task<IActionResult> Fulfill(Guid id, [FromBody] FulfillStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.FulfillAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Penyerahan barang berhasil dicatat."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Stock Request",
        Description = "Membatalkan permintaan stok", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("StockRequest", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CancelAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Persetujuan atasan unit atas permintaan yang menunggu persetujuan (<c>LGS-DEC-002</c>).
    /// </summary>
    /// <remarks>
    /// Izin <c>StockRequest:Approve</c> diperiksa filter di bawah (kebijakan platform) dan
    /// diperiksa ulang oleh service secara eksplisit terhadap penempatan jabatan, sehingga peran
    /// SuperAdmin maupun saklar pengembangan tidak melewatinya. Pemutus juga wajib atasan unit
    /// dari unit peminta: penempatan manajerial aktif pada unit organisasi yang ditautkan ke unit
    /// layanan itu (`403` bila tidak, termasuk bila unit layanan belum ditautkan). Pemohon tidak boleh
    /// menyetujui permintaannya sendiri (`403`). Keputusan hanya sah sekali (`409`, kode
    /// <c>PHM018</c>). Tidak ada mutasi stok pada langkah ini.
    /// </remarks>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Request",
        Description = "Menyetujui atau menolak permintaan stok sebagai atasan unit",
        AccessType = AccessTypes.Update, SortOrder = 10)]
    [AccessPermission("StockRequest", "Approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.ApproveAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok disetujui dan siap diproses gudang."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Penolakan atasan unit; alasannya wajib (`422`, kode <c>PHM015</c>). Kewenangannya sama
    /// dengan persetujuan: izin dan atasan unit dari unit peminta.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Request",
        Description = "Menyetujui atau menolak permintaan stok sebagai atasan unit",
        AccessType = AccessTypes.Update, SortOrder = 11)]
    [AccessPermission("StockRequest", "Approve")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.RejectAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok ditolak."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Otorisasi jalur CITO oleh petugas berwenang atas permintaan darurat yang menunggu.
    /// </summary>
    /// <remarks>
    /// Izin <c>StockRequest:AuthorizeEmergency</c> terpisah dari persetujuan biasa dan tidak
    /// dimiliki siapa pun sampai diberikan secara eksplisit. Filter di atas mengikuti kebijakan
    /// platform yang meloloskan SuperAdmin; service memeriksa ulang bahwa izin itu benar-benar
    /// diberikan kepada penempatan jabatan petugas, sehingga peran SuperAdmin saja ditolak
    /// (`403`). Alasan wajib; permintaan non-darurat ditolak (`422`, kode <c>PHM017</c>).
    /// </remarks>
    [HttpPost("{id:guid}/authorize-emergency")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("AuthorizeEmergency", "Authorize Emergency Stock Request",
        Description = "Mengotorisasi permintaan stok CITO lewat jalur darurat",
        AccessType = AccessTypes.Update, SortOrder = 12)]
    [AccessPermission("StockRequest", "AuthorizeEmergency")]
    public async Task<IActionResult> AuthorizeEmergency(Guid id,
        [FromBody] AuthorizeEmergencyStockRequestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.AuthorizeEmergencyAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok CITO diotorisasi dan diprioritaskan ke gudang."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Verifikasi administratif susulan atas permintaan CITO yang diotorisasi lewat jalur
    /// darurat (<c>LGS-DEC-005</c>).
    /// </summary>
    /// <remarks>
    /// Memakai izin persetujuan yang sama (<c>StockRequest:Approve</c>), karena verifikatornya
    /// atasan unit peminta. Service memeriksa ulang izin itu secara eksplisit, kewenangan atas
    /// unit, serta melarang pemohon dan petugas pengotorisasi CITO memverifikasi (`403`). Hasil
    /// <c>Deviation</c> wajib disertai catatan (`422`, <c>PHM008</c>). Verifikasi hanya sah sekali
    /// (`409`, <c>PHM019</c>); permintaan yang tidak lewat jalur CITO ditolak (`409`,
    /// <c>PHM010</c>). Tidak ada perubahan status maupun mutasi stok.
    /// </remarks>
    [HttpPost("{id:guid}/verify-emergency")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Approve", "Approve Stock Request",
        Description = "Menyetujui atau menolak permintaan stok sebagai atasan unit",
        AccessType = AccessTypes.Update, SortOrder = 13)]
    [AccessPermission("StockRequest", "Approve")]
    public async Task<IActionResult> VerifyEmergency(Guid id,
        [FromBody] VerifyEmergencyStockRequestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.VerifyEmergencyAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Verifikasi susulan CITO tercatat."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }
}

/// <summary>
/// Pemetaan tunggal kegagalan domain permintaan stok ke kode HTTP: `403` tidak berwenang,
/// `409` benturan atau transisi tidak sah, `422` prasyarat aturan.
/// </summary>
internal static class StockRequestControllerResults
{
    public static ObjectResult StockRequestForbidden(this ControllerBase controller,
        StockRequestForbiddenException exception) =>
        controller.StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));

    public static ObjectResult StockRequestConflict(this ControllerBase controller,
        StockRequestConflictException exception) =>
        controller.StatusCode(StatusCodes.Status409Conflict,
            ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message,
                new { exception.Code }));

    public static ObjectResult StockRequestUnprocessable(this ControllerBase controller,
        StockRequestUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { exception.Code }));
}
