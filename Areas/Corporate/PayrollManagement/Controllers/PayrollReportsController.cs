using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Laporan rekap dan rekonsiliasi penggajian (CT-PAY-15). Arketipe: monitoring read-only (standar transaksi 2.4). Rekap berisi agregat
/// tanpa gaji per orang; nominal agregat hanya bagi pemilik izin ViewAmount. Rekonsiliasi memuat nominal sehingga dijaga ViewAmount.
/// Tidak ada jurnal yang dibuat dari laporan ini (mapping COA Payroll belum disahkan, NEEDS CONFIRMATION).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/reports")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_REPORT",
    moduleName: "Corporate Payroll Report",
    displayName: "Payroll Report",
    AreaName = "Corporate",
    ControllerName = "PayrollReport",
    Description = "Rekap penggajian dan rekonsiliasi Payroll-Finance-Accounting",
    SortOrder = 63)]
[Tags("Corporate / Payroll Management / Report")]
public sealed class PayrollReportsController : ControllerBase
{
    private const string Resource = "PayrollReport";

    private readonly PayrollReportService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollReportsController(PayrollReportService service, AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    [HttpGet("recap")]
    [AccessAction("Read", "Read Payroll Recap", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollReport", "Read")]
    public async Task<IActionResult> GetRecap([FromQuery] PayrollRecapQuery query, CancellationToken cancellationToken)
    {
        var includeAmounts = await _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetRecapAsync(query, includeAmounts, cancellationToken), "Rekap penggajian berhasil dimuat.");
    }

    /// <summary>Rekonsiliasi memuat nominal run, instruksi, hasil bank, dan potongan piutang; karena itu dijaga izin ViewAmount.</summary>
    [HttpGet("reconciliation")]
    [AccessAction("ViewAmount", "View Payroll Reconciliation", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollReport", "ViewAmount")]
    public Task<IActionResult> GetReconciliation([FromQuery] Guid payrollRunId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetReconciliationAsync(payrollRunId, cancellationToken), "Rekonsiliasi penggajian berhasil dimuat.");
}
