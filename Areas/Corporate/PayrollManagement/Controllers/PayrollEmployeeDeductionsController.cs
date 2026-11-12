using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Riwayat dan rekonsiliasi potongan cicilan piutang karyawan/dokter (CT-PAY-12, CT-PAY-13). Arketipe: monitoring read-only (standar
/// transaksi 2.4). TIDAK ADA endpoint yang memotong atau mengirim ulang hasil ke Finance di sini: pemotongan hanya lewat siklus
/// payroll run (hitung lalu sahkan). Rekonsiliasi membandingkan; ia tidak membayar ulang dan tidak mengubah saldo piutang.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/employee-receivable-deductions")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_EMPLOYEE_DEDUCTION",
    moduleName: "Corporate Payroll Employee Deduction",
    displayName: "Payroll Employee Receivable Deduction",
    AreaName = "Corporate",
    ControllerName = "PayrollEmployeeDeduction",
    Description = "Riwayat dan rekonsiliasi potongan cicilan piutang karyawan/dokter",
    SortOrder = 61)]
[Tags("Corporate / Payroll Management / Employee Receivable Deduction")]
public sealed class PayrollEmployeeDeductionsController : ControllerBase
{
    private readonly PayrollEmployeeDeductionService _service;

    public PayrollEmployeeDeductionsController(PayrollEmployeeDeductionService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Payroll Employee Receivable Deductions", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollEmployeeDeduction", "Read")]
    public Task<IActionResult> GetPaged([FromQuery] PayrollEmployeeDeductionQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetPagedAsync(query, cancellationToken), "Riwayat potongan cicilan berhasil dimuat.");

    [HttpGet("summary")]
    [AccessAction("ReadSummary", "Read Payroll Employee Receivable Deduction Summary", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollEmployeeDeduction", "ReadSummary")]
    public Task<IActionResult> GetSummary([FromQuery] Guid payrollPeriodId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetSummaryAsync(payrollPeriodId, cancellationToken), "Ringkasan potongan cicilan berhasil dimuat.");

    /// <summary>Menjalankan rekonsiliasi periode (hanya membandingkan; tanpa bayar ulang). Dicatat audit siapa yang menjalankan.</summary>
    [HttpPost("reconcile")]
    [AccessAction("Reconcile", "Reconcile Payroll Employee Receivable Deductions", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PayrollEmployeeDeduction", "Reconcile")]
    public Task<IActionResult> Reconcile([FromBody] PayrollDeductionReconcileRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.ReconcileAsync(request.PayrollPeriodId, PayrollControllerSupport.CurrentUserId(User), cancellationToken),
            "Rekonsiliasi potongan cicilan selesai.");
}
