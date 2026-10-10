using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Aggregate ber-lifecycle (transaksi, bukan master data) — perpindahan status lewat aksi
/// POST /{id}/&lt;aksi&gt;, bukan PATCH /{id}/status generik (transaction-endpoint-standard.md).
/// BE-FIN-093, FIN-API-1.9 O.2. Dua bentuk route pada satu controller mengikuti kontrak apa
/// adanya: bersarang di bawah piutang untuk mengajukan/membaca riwayat, datar untuk worklist
/// dan aksi keputusan — base URL sama persis dengan FinanceReceivablesController.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_INSTALLMENT_PLAN", "Corporate Finance Management Receivable Installment Plan", "Receivable Installment Plan",
    AreaName = "Corporate", ControllerName = "FinanceReceivableInstallmentPlan", Description = "Perjanjian cicilan piutang manfaat karyawan", SortOrder = 32)]
[Tags("Corporate / Finance Management / Receivable / Installment Plan")]
public sealed class FinanceReceivableInstallmentPlansController : ControllerBase
{
    private readonly FinanceReceivableInstallmentPlanService _service;
    public FinanceReceivableInstallmentPlansController(FinanceReceivableInstallmentPlanService service) => _service = service;

    [HttpPost("receivables/{receivableId:guid}/installment-plans")]
    [AccessAction("Create", "Create Installment Plan", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Create")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(Guid receivableId, [FromBody] CreateInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(receivableId, request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<InstallmentPlanResponse>.Ok(result, "Perjanjian angsuran berhasil diajukan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("receivables/{receivableId:guid}/installment-plans")]
    [AccessAction("Read", "Read Installment Plan", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<InstallmentPlanResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid receivableId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetHistoryAsync(receivableId, cancellationToken);
            return Ok(ApiResponse<List<InstallmentPlanResponse>>.Ok(result, "Riwayat perjanjian angsuran berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("receivable-installment-plans")]
    [AccessAction("Read", "Read Installment Plan", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InstallmentPlanListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] InstallmentPlanQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<InstallmentPlanListResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar perjanjian angsuran berhasil diambil."));

    [HttpGet("receivable-installment-plans/{id:guid}")]
    [AccessAction("Read", "Read Installment Plan", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Read")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<InstallmentPlanDetailResponse>.Ok(result, "Rincian perjanjian angsuran berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    /// <summary>Menyetujui, membangkitkan seluruh jadwal angsuran sekaligus (FIN-VAL-233/234).</summary>
    [HttpPost("receivable-installment-plans/{id:guid}/approve")]
    [AccessAction("Approve", "Approve Installment Plan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentPlanDetailResponse>.Ok(result, "Perjanjian angsuran disetujui dan jadwal angsuran diterbitkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("receivable-installment-plans/{id:guid}/reject")]
    [AccessAction("Reject", "Reject Installment Plan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentPlanResponse>.Ok(result, "Perjanjian angsuran ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("receivable-installment-plans/{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Installment Plan", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinanceReceivableInstallmentPlan", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<InstallmentPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelInstallmentPlanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<InstallmentPlanResponse>.Ok(result, "Perjanjian angsuran dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        InstallmentPlanConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        InstallmentPlanValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or InstallmentPlanConflictException or InstallmentPlanValidationException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
