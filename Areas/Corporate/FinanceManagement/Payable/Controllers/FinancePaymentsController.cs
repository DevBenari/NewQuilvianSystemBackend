using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Controllers;

/// <summary>
/// Resource permission ("Payment") mengikuti pola penamaan singkat yang sudah berjalan nyata pada
/// `FinanceReceivablesController` ("Receivable"), bukan nama resource persis
/// `contracts/permission-audit-matrix.md` (`FinancePayment`).
/// Hanya endpoint yang sudah punya logika service nyata yang dibangun di sini (BE-FIN-020,
/// pembaruan 23 September 2026): rincian, susun/ubah draft, ajukan, setujui/tolak, tandai lunas,
/// batalkan. Potongan/tambahan disusun sebagai bagian body `Create`/`Update` — service ini tidak
/// punya method tulis terpisah untuk `POST/DELETE /payments/{id}/deductions`, jadi kedua endpoint
/// itu **tidak** dibangun di sini (dicatat sebagai gap terbuka, bukan dikarang). `GET /payments`
/// (daftar berpaging) pada `FIN-API-1.0` juga belum ada service-nya.
///
/// FIN-OQ-010 (ambang nominal approval berjenjang persis) **belum diratifikasi** Finance
/// Supervisor/Yasmin — `FinancePaymentService.ResolveApprovalTier` sudah memakai nilai placeholder
/// yang didokumentasikan eksplisit sebagai provisional (lihat komentarnya). Controller ini
/// mengekspos `ApprovalTier` hasil resolver itu apa adanya; tidak ada logika ambang baru
/// ditambahkan atau diasumsikan di sini.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/payments")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_PAYMENT", "Corporate Finance Management Payment", "Payment",
    AreaName = "Corporate", ControllerName = "Payment", Description = "Pembayaran keluar Finance — draft, pengajuan, persetujuan berjenjang, dan pelunasan", SortOrder = 41)]
[Tags("Corporate / Finance Management / Payment")]
public sealed class FinancePaymentsController : ControllerBase
{
    private readonly FinancePaymentService _service;
    public FinancePaymentsController(FinancePaymentService service) => _service = service;

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Payment", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Payment", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _service.GetByIdAsync(id, cancellationToken);
        if (payment is null) return NotFound(ApiResponse<object>.Fail(404, "Pembayaran tidak ditemukan."));
        return Ok(ApiResponse<PaymentDetailResponse>.Ok(Map(payment), "Detail pembayaran berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Payment", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("Payment", "Create")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var allocations = MapAllocationRequests(request.Allocations);
            var deductions = MapDeductionRequests(request.Deductions);
            var payment = await _service.CreateDraftAsync(
                request.PaymentType, request.PayeeReferenceId, request.BankAccountId, request.PaymentMethod, request.Notes,
                allocations, deductions, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Draft pembayaran berhasil disusun."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Payment", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("Payment", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var allocations = MapAllocationRequests(request.Allocations);
            var deductions = MapDeductionRequests(request.Deductions);
            var payment = await _service.UpdateDraftAsync(
                id, request.ExpectedRowVersion, request.BankAccountId, request.PaymentMethod, request.Notes,
                allocations, deductions, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Draft pembayaran berhasil diperbarui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payment", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("Payment", "Submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] PaymentRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _service.SubmitAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Pembayaran berhasil diajukan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payment", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("Payment", "Approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] PaymentRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _service.ApproveAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Pembayaran berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Payment", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("Payment", "Approve")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _service.RejectAsync(id, request.ExpectedRowVersion, request.RejectionReason, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Pembayaran berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/mark-paid")]
    [AccessAction("MarkPaid", "Mark Payment Paid", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("Payment", "MarkPaid")]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaymentPaidRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _service.MarkPaidAsync(id, request.ExpectedRowVersion, request.ReferenceNumber, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Pembayaran berhasil ditandai lunas."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Payment", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("Payment", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] PaymentRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PaymentResponse>.Ok(MapPayment(payment), "Pembayaran berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static List<PaymentAllocationRequest> MapAllocationRequests(List<PaymentAllocationRequestDto> dtos) =>
        (dtos ?? []).Select(a => new PaymentAllocationRequest(a.PayableType, a.PayableId, a.Amount)).ToList();

    private static List<PaymentDeductionRequest>? MapDeductionRequests(List<PaymentDeductionRequestDto>? dtos) =>
        dtos is null ? null : dtos.Select(d => new PaymentDeductionRequest(d.DeductionType, d.Direction, d.Amount, d.Reason, d.ReferenceNumber)).ToList();

    private static PaymentResponse MapPayment(FinPayment payment) => new()
    {
        Id = payment.Id,
        PaymentNumber = payment.PaymentNumber,
        PaymentType = payment.PaymentType,
        PayeeReferenceId = payment.PayeeReferenceId,
        BankAccountId = payment.BankAccountId,
        PaymentMethod = payment.PaymentMethod,
        TotalAmount = payment.TotalAmount,
        AllocatedAmount = payment.AllocatedAmount,
        DeductionAmount = payment.DeductionAmount,
        AdditionAmount = payment.AdditionAmount,
        NetTransferAmount = payment.NetTransferAmount,
        Status = payment.Status,
        ApprovalTier = payment.ApprovalTier,
        RequestedBy = payment.RequestedBy,
        RequestedAt = payment.RequestedAt,
        ApprovedBy = payment.ApprovedBy,
        ApprovedAt = payment.ApprovedAt,
        PaidAt = payment.PaidAt,
        ReferenceNumber = payment.ReferenceNumber,
        RejectionReason = payment.RejectionReason,
        Notes = payment.Notes,
        RowVersion = payment.RowVersion
    };

    private static PaymentDetailResponse Map(FinPayment payment)
    {
        var mapped = MapPayment(payment);
        return new PaymentDetailResponse
        {
            Id = mapped.Id,
            PaymentNumber = mapped.PaymentNumber,
            PaymentType = mapped.PaymentType,
            PayeeReferenceId = mapped.PayeeReferenceId,
            BankAccountId = mapped.BankAccountId,
            PaymentMethod = mapped.PaymentMethod,
            TotalAmount = mapped.TotalAmount,
            AllocatedAmount = mapped.AllocatedAmount,
            DeductionAmount = mapped.DeductionAmount,
            AdditionAmount = mapped.AdditionAmount,
            NetTransferAmount = mapped.NetTransferAmount,
            Status = mapped.Status,
            ApprovalTier = mapped.ApprovalTier,
            RequestedBy = mapped.RequestedBy,
            RequestedAt = mapped.RequestedAt,
            ApprovedBy = mapped.ApprovedBy,
            ApprovedAt = mapped.ApprovedAt,
            PaidAt = mapped.PaidAt,
            ReferenceNumber = mapped.ReferenceNumber,
            RejectionReason = mapped.RejectionReason,
            Notes = mapped.Notes,
            RowVersion = mapped.RowVersion,
            Allocations = payment.Allocations.Select(a => new PaymentAllocationResponse
            {
                Id = a.Id,
                PayableType = a.PayableType,
                SupplierPayableId = a.SupplierPayableId,
                MedicalServicePayableId = a.MedicalServicePayableId,
                Amount = a.Amount,
                IsReversal = a.IsReversal,
                ReversalOfAllocationId = a.ReversalOfAllocationId
            }).ToList(),
            Deductions = payment.Deductions.Select(d => new PaymentDeductionResponse
            {
                Id = d.Id,
                DeductionType = d.DeductionType,
                Direction = d.Direction,
                Amount = d.Amount,
                Reason = d.Reason,
                ReferenceNumber = d.ReferenceNumber
            }).ToList()
        };
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PaymentConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PaymentValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PaymentBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PaymentConflictException or PaymentValidationException or PaymentBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
