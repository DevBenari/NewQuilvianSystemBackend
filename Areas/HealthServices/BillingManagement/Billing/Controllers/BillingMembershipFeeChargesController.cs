using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

/// <summary>
/// Tagihan iuran membership non-kunjungan (crm-membership CRM-DEC-019). Controller ini hanya membaca dan
/// mengelola tagihan; pembayaran memakai settlement Kasir yang sudah ada
/// (<c>POST patient-funds/settlements</c> dengan <c>Purpose = MEMBERSHIP_FEE</c>, lalu <c>.../tenders</c>).
/// Tidak ada endpoint yang mengubah membership menjadi Aktif: aktivasi hanya dari settlement sukses di server.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing/membership-fee-charges")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT", "Health Service Billing Management", "Billing Membership Fee Charges",
    AreaName = "HealthServices", ControllerName = "BillingMembershipFeeCharges",
    Description = "Tagihan iuran membership non-kunjungan yang dibayar di Kasir", SortOrder = 4)]
[Tags("Health Services / Billing Management / Billing / Membership Fee Charges")]
public sealed class BillingMembershipFeeChargesController : ControllerBase
{
    private readonly BillingMembershipFeeService _service;

    public BillingMembershipFeeChargesController(BillingMembershipFeeService service)
    {
        _service = service;
    }

    /// <summary>Daftar tagihan iuran (server-side search: nomor tagihan, nomor membership, No. RM, nama pasien).</summary>
    [HttpGet]
    [AccessAction("Read", "Read Membership Fee Charge", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingMembershipFeeCharges", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MembershipFeeChargeResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPaged([FromQuery] MembershipFeeChargeQuery query, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<PagedResult<MembershipFeeChargeResponse>>.Ok(
            await _service.GetPagedAsync(query, cancellationToken), "Daftar tagihan iuran membership berhasil diambil.")));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Membership Fee Charge", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingMembershipFeeCharges", "Read")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeChargeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<MembershipFeeChargeDetailResponse>.Ok(
            await _service.GetDetailAsync(id, cancellationToken), "Detail tagihan iuran membership berhasil diambil.")));

    /// <summary>Tagihan milik satu membership (dipakai CRM dan alur navigasi CRM → Kasir).</summary>
    [HttpGet("by-membership/{membershipId:guid}")]
    [AccessAction("Read", "Read Membership Fee Charge", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingMembershipFeeCharges", "Read")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeChargeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetByMembership(Guid membershipId, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<MembershipFeeChargeDetailResponse>.Ok(
            await _service.GetByMembershipAsync(membershipId, cancellationToken), "Tagihan iuran membership berhasil diambil.")));

    /// <summary>Riwayat pembayaran (tender) sebuah tagihan, termasuk yang gagal atau dibalik.</summary>
    [HttpGet("{id:guid}/payments")]
    [AccessAction("Read", "Read Membership Fee Charge", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingMembershipFeeCharges", "Read")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MembershipFeePaymentItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetPayments(Guid id, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<IReadOnlyList<MembershipFeePaymentItem>>.Ok(
            await _service.GetPaymentsAsync(id, cancellationToken), "Riwayat pembayaran iuran membership berhasil diambil.")));

    /// <summary>
    /// Membuat atau mengambil tagihan untuk membership Menunggu Pembayaran yang belum bertagihan. Idempotent:
    /// pengulangan mengembalikan tagihan yang sama, dengan nominal dari snapshot membership.
    /// </summary>
    [HttpPost]
    [AccessAction("Create", "Create Membership Fee Charge", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BillingMembershipFeeCharges", "Create")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeChargeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> EnsureForMembership([FromBody] EnsureMembershipFeeChargeRequest request, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<MembershipFeeChargeDetailResponse>.Ok(
            await _service.EnsureForMembershipAsync(request.MembershipId, CurrentUserId(), cancellationToken),
            "Tagihan iuran membership siap dibayar.")));

    /// <summary>
    /// Membatalkan tagihan mengikuti lifecycle CRM: hanya bila membership-nya sudah dibatalkan. Ditolak bila uang
    /// sudah diterima atau sedang diproses; refund tidak dilakukan otomatis.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Membership Fee Charge", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("BillingMembershipFeeCharges", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeChargeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<MembershipFeeChargeDetailResponse>.Ok(
            await _service.CancelForCancelledMembershipAsync(id, CurrentUserId(), cancellationToken),
            "Tagihan iuran membership dibatalkan.")));

    /// <summary>
    /// Mengulang konfirmasi pelunasan ke CRM untuk satu tagihan lunas yang belum diterapkan. Idempotent;
    /// hasil penolakan (mis. dibayar setelah batas bayar) dikembalikan apa adanya untuk ditinjau.
    /// </summary>
    [HttpPost("{id:guid}/reconcile")]
    [AccessAction("Reconcile", "Reconcile Membership Fee Charge", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("BillingMembershipFeeCharges", "Reconcile")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeChargeDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Reconcile(Guid id, CancellationToken cancellationToken) =>
        RunAsync(async () =>
        {
            await _service.ConfirmChargeAsync(id, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<MembershipFeeChargeDetailResponse>.Ok(
                await _service.GetDetailAsync(id, cancellationToken), "Rekonsiliasi tagihan iuran selesai."));
        });

    /// <summary>
    /// Laporan rekonsiliasi (INT-06): tagihan lunas yang belum mengaktifkan membership, membership aktif tanpa
    /// tagihan lunas (kecuali data lama), dan tagihan yang perlu ditinjau manual. Hanya membaca.
    /// </summary>
    [HttpGet("reconciliation")]
    [AccessAction("Reconcile", "Reconcile Membership Fee Charge", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("BillingMembershipFeeCharges", "Reconcile")]
    [ProducesResponseType(typeof(ApiResponse<MembershipFeeReconciliationResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetReconciliation(CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<MembershipFeeReconciliationResponse>.Ok(
            await _service.GetReconciliationAsync(cancellationToken), "Laporan rekonsiliasi iuran membership berhasil diambil.")));

    private async Task<IActionResult> RunAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingMembershipFeeConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingMembershipFeeValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
