using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Handoff pembayaran gaji ke Finance dan penerimaan hasil bank (CT-PAY-08, CT-PAY-09). Arketipe: sub-proses yang di-scope ke
/// payroll run induknya (standar transaksi 2.3). Status Dibayar hanya lahir dari aksi Acknowledge oleh pengguna yang diberi izin
/// itu lewat Akses Role: Finance dengan hasil bank (ConfirmationSource FINANCE_ACK, bawaan) atau, selama belum ada koneksi bank,
/// petugas Payroll berwenang lewat layar Konfirmasi Pembayaran Manual (ConfirmationSource MANUAL, PAY-BE-11). Aksi handoff dan
/// Acknowledge sebaiknya dipegang orang berbeda pada produksi (pemisahan tugas; keputusan pemberian hak ada pada admin). Sisi
/// Finance (jenis pembayaran PAYROLL, COA, jurnal) adalah NEEDS CONFIRMATION dan dijelaskan apa adanya pada respons.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payroll-runs/{runId:guid}")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_PAYMENT",
    moduleName: "Corporate Payroll Payment",
    displayName: "Payroll Payment",
    AreaName = "Corporate",
    ControllerName = "PayrollPayment",
    Description = "Instruksi pembayaran gaji ke Finance dan penerimaan hasil bank",
    SortOrder = 58)]
[Tags("Corporate / Payroll Management / Payment")]
public sealed class PayrollPaymentsController : ControllerBase
{
    private readonly PayrollPaymentService _service;

    public PayrollPaymentsController(PayrollPaymentService service)
    {
        _service = service;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    [HttpPost("finance-handoff")]
    [AccessAction("FinanceHandoff", "Send Payroll Payment Instructions To Finance", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("PayrollPayment", "FinanceHandoff")]
    public Task<IActionResult> FinanceHandoff(
        Guid runId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PayrollFinanceHandoffRequest? request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.HandoffAsync(runId, request ?? new PayrollFinanceHandoffRequest(), CurrentUserId(), cancellationToken),
            "Instruksi pembayaran dikirim ke Finance.");

    [HttpGet("payments")]
    [AccessAction("Read", "Read Payroll Payments", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollPayment", "Read")]
    public Task<IActionResult> GetPayments(Guid runId, [FromQuery] PayrollPaymentQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetPaymentsAsync(runId, query, cancellationToken), "Daftar pembayaran berhasil dimuat.");

    [HttpPost("payments/acknowledge")]
    [AccessAction("Acknowledge", "Acknowledge Payroll Payment Results", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PayrollPayment", "Acknowledge")]
    public Task<IActionResult> Acknowledge(Guid runId, [FromBody] PayrollPaymentAcknowledgeRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.AcknowledgeAsync(runId, request, CurrentUserId(), cancellationToken), "Hasil pembayaran diproses.");

    [HttpPost("payments/{paymentId:guid}/retry")]
    [AccessAction("Retry", "Retry Failed Payroll Payment", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("PayrollPayment", "Retry")]
    public Task<IActionResult> Retry(Guid runId, Guid paymentId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.RetryAsync(runId, paymentId, CurrentUserId(), cancellationToken), "Instruksi pembayaran dikirim ulang.");
}
