using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Controllers;

/// <summary>
/// Batas Pembayaran Langsung (BE-FIN-076, FIN-API-1.6 F.4; FIN-BE-DPT-001). Resource tetap <c>MstDirectPaymentThreshold</c>
/// dan route tetap — yang berubah hanya istilah tampilan dan alur: nominal tidak lagi diubah langsung, melainkan lewat
/// pengajuan → persetujuan → aktivasi. Izin: <c>Read</c>, <c>Update</c> (= mengajukan), <c>Approve</c>, <c>Reject</c>,
/// <c>Activate</c>. Hak ditentukan admin lewat Akses Role; tidak ada nama peran yang ditulis di kode.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/master-data/direct-payment-threshold")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DIRECT_PAYMENT_THRESHOLD",
    "Corporate Finance Management Direct Payment Threshold", "Master Data",
    AreaName = "Corporate", ControllerName = "MstDirectPaymentThreshold",
    Description = "Batas pembayaran langsung beserta pengajuan, persetujuan, aktivasi, dan riwayatnya", SortOrder = 73)]
[Tags("Corporate / Finance Management / Master Data / Direct Payment Threshold")]
public sealed class DirectPaymentThresholdController : ControllerBase
{
    private readonly DirectPaymentThresholdService _service;
    private readonly DirectPaymentThresholdProposalService _proposalService;
    private readonly AccessPermissionService _accessPermissionService;

    public DirectPaymentThresholdController(
        DirectPaymentThresholdService service,
        DirectPaymentThresholdProposalService proposalService,
        AccessPermissionService accessPermissionService)
    {
        _service = service;
        _proposalService = proposalService;
        _accessPermissionService = accessPermissionService;
    }

    /// <summary>
    /// Batas yang BENAR-BENAR berlaku bagi pembayaran (hanya diubah oleh aktivasi), beserta alasan dan jejak perubahan terakhir.
    /// 404 bila belum pernah ditetapkan (fail-closed, FIN-DES-086). Pengajuan yang belum diaktifkan tidak pernah muncul di sini.
    /// </summary>
    [HttpGet]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetActiveAsync(cancellationToken);
            return Ok(ApiResponse<DirectPaymentThresholdResponse>.Ok(result, "Batas pembayaran langsung berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>
    /// JALUR LAMA, DINONAKTIFKAN (FIN-BE-DPT-001). Dahulu menimpa nominal seketika tanpa persetujuan. Kini TIDAK mengubah apa pun dan
    /// tidak membuat pengajuan diam-diam; klien diarahkan ke <c>POST proposals</c>. Izin <c>Update</c> tetap diperiksa.
    /// </summary>
    [HttpPut]
    [AccessPermission("MstDirectPaymentThreshold", "Update")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
    public IActionResult Update([FromBody] UpdateDirectPaymentThresholdRequest request) =>
        StatusCode(StatusCodes.Status410Gone, ApiResponse<object>.Fail(
            StatusCodes.Status410Gone,
            "Perubahan langsung batas pembayaran tidak lagi didukung. Ajukan perubahan lewat pengajuan, lalu tunggu persetujuan dan aktivasi."));

    /// <summary>Ringkasan pengajuan beserta versi batas yang sedang berlaku (sumber, tanggal berlaku, pengaktif).</summary>
    [HttpGet("proposals/summary")]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _proposalService.GetSummaryAsync(cancellationToken), "Ringkasan pengajuan batas pembayaran langsung berhasil diambil.");

    [HttpGet("proposals")]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DirectPaymentThresholdProposalResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProposals(
        [FromQuery] DirectPaymentThresholdProposalQuery request, CancellationToken cancellationToken)
    {
        var viewerIsSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.GetProposalsAsync(request, CurrentUserId(), viewerIsSuperAdmin, cancellationToken),
            "Pengajuan batas pembayaran langsung berhasil diambil.");
    }

    [HttpGet("proposals/{id:guid}")]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProposal(Guid id, CancellationToken cancellationToken)
    {
        var viewerIsSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.GetProposalAsync(id, CurrentUserId(), viewerIsSuperAdmin, cancellationToken),
            "Detail pengajuan batas pembayaran langsung berhasil diambil.");
    }

    /// <summary>Riwayat kejadian (pengajuan, persetujuan, penolakan, aktivasi, penggantian), terbaru lebih dulu.</summary>
    [HttpGet("history")]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DirectPaymentThresholdHistoryResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetHistory(
        [FromQuery] DirectPaymentThresholdHistoryQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _proposalService.GetHistoryAsync(request, cancellationToken), "Riwayat batas pembayaran langsung berhasil diambil.");

    /// <summary>
    /// Mengajukan penetapan atau perubahan batas. Tidak mengubah batas yang berlaku; status awal MENUNGGU. Izin <c>Update</c> kini
    /// berarti "boleh mengajukan" — bukan "boleh mengubah langsung".
    /// </summary>
    [HttpPost("proposals")]
    [AccessAction("Update", "Propose Direct Payment Threshold Change", Description = "Mengajukan penetapan atau perubahan batas pembayaran langsung",
        AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("MstDirectPaymentThreshold", "Update")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateProposal(
        [FromBody] CreateDirectPaymentThresholdProposalRequest request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.CreateAsync(request, CurrentUserId(), isSuperAdmin, cancellationToken),
            "Pengajuan batas pembayaran berhasil dibuat dan sedang menunggu persetujuan.");
    }

    /// <summary>Menyetujui pengajuan MENUNGGU. Belum mengaktifkan nominal.</summary>
    [HttpPost("proposals/{id:guid}/approve")]
    [AccessAction("Approve", "Approve Direct Payment Threshold Change", Description = "Menyetujui pengajuan perubahan batas pembayaran langsung",
        AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("MstDirectPaymentThreshold", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApproveDirectPaymentThresholdProposalRequest request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.ApproveAsync(id, request, CurrentUserId(), isSuperAdmin, cancellationToken),
            "Pengajuan disetujui. Batas baru belum berlaku sampai diaktifkan.");
    }

    /// <summary>Menolak pengajuan MENUNGGU; alasan penolakan wajib.</summary>
    [HttpPost("proposals/{id:guid}/reject")]
    [AccessAction("Reject", "Reject Direct Payment Threshold Change", Description = "Menolak pengajuan perubahan batas pembayaran langsung",
        AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("MstDirectPaymentThreshold", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectDirectPaymentThresholdProposalRequest request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.RejectAsync(id, request, CurrentUserId(), isSuperAdmin, cancellationToken),
            "Pengajuan ditolak.");
    }

    /// <summary>Mengaktifkan pengajuan DISETUJUI: batas lama digantikan secara atomik dan Tanggal Berlaku = waktu aktivasi.</summary>
    [HttpPost("proposals/{id:guid}/activate")]
    [AccessAction("Activate", "Activate Direct Payment Threshold Change", Description = "Mengaktifkan batas pembayaran langsung yang sudah disetujui",
        AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("MstDirectPaymentThreshold", "Activate")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdProposalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        Guid id, [FromBody] ActivateDirectPaymentThresholdProposalRequest request, CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _accessPermissionService.IsSuperAdminAsync(User);
        return await ExecuteAsync(
            () => _proposalService.ActivateAsync(id, request, CurrentUserId(), isSuperAdmin, cancellationToken),
            "Batas pembayaran langsung baru berhasil diaktifkan dan mulai berlaku.");
    }

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string successMessage)
    {
        try
        {
            return Ok(ApiResponse<T>.Ok(await command(), successMessage));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (DirectPaymentThresholdValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
        catch (DirectPaymentThresholdBadRequestException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
        // FIN-VAL-228: 409 HANYA untuk benturan versi/penyimpanan (data sudah diubah pihak lain). Pelanggaran aturan bisnis
        // (pengajuan terbuka lain, pengaju menyetujui sendiri) memakai 422, dan 403 hanya untuk izin Role Management.
        catch (DirectPaymentThresholdConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
