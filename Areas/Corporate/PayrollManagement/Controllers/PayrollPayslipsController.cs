using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Slip gaji untuk administrator Payroll (CT-PAY-10): buat dari snapshot run yang dibekukan, terbitkan menurut kebijakan akses slip,
/// lihat daftar/detail, dan unduh (beraudit). Penerima mengakses slip miliknya lewat <see cref="PayrollMyPayslipsController"/>.
/// Nominal pada daftar dan detail hanya bagi pemilik izin ViewAmount; unduhan administrator memuat nominal sehingga dijaga ViewAmount.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_PAYSLIP",
    moduleName: "Corporate Payroll Payslip",
    displayName: "Payroll Payslip",
    AreaName = "Corporate",
    ControllerName = "PayrollPayslip",
    Description = "Pembuatan, penerbitan, dan akses administrator atas slip gaji",
    SortOrder = 59)]
[Tags("Corporate / Payroll Management / Payslip")]
public sealed class PayrollPayslipsController : ControllerBase
{
    private const string Resource = "PayrollPayslip";

    private readonly PayrollPayslipService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollPayslipsController(PayrollPayslipService service, AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");

    [HttpGet("payroll-runs/{runId:guid}/payslips")]
    [AccessAction("Read", "Read Payroll Payslips", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollPayslip", "Read")]
    public async Task<IActionResult> GetByRun(Guid runId, [FromQuery] PayrollPayslipQuery query, CancellationToken cancellationToken)
    {
        var includeAmounts = await CanViewAmountAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetByRunAsync(runId, query, includeAmounts, cancellationToken), "Daftar slip berhasil dimuat.");
    }

    [HttpPost("payroll-runs/{runId:guid}/payslips/generate")]
    [AccessAction("Generate", "Generate Payroll Payslips", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollPayslip", "Generate")]
    public Task<IActionResult> Generate(Guid runId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GenerateAsync(runId, CurrentUserId(), cancellationToken), "Slip gaji dibuat.");

    [HttpPost("payroll-runs/{runId:guid}/payslips/publish")]
    [AccessAction("Publish", "Publish Payroll Payslips", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PayrollPayslip", "Publish")]
    public Task<IActionResult> Publish(Guid runId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.PublishAsync(runId, CurrentUserId(), cancellationToken), "Slip gaji diterbitkan.");

    [HttpGet("payslips/{id:guid}")]
    [AccessAction("ReadDetail", "Read Payroll Payslip Detail", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("PayrollPayslip", "ReadDetail")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var includeAmounts = await CanViewAmountAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetDetailAsync(id, ownerUserId: null, includeAmounts, cancellationToken), "Detail slip berhasil dimuat.");
    }

    /// <summary>Unduhan administrator memuat nominal gaji, sehingga dijaga izin ViewAmount. Setiap unduhan dicatat audit.</summary>
    [HttpGet("payslips/{id:guid}/download")]
    [AccessAction("ViewAmount", "Download Payroll Payslip With Amount", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("PayrollPayslip", "ViewAmount")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _service.DownloadAsync(id, CurrentUserId(), ownerOnly: false, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception ex) when (PayrollControllerSupport.IsHandled(ex))
        {
            return PayrollControllerSupport.Failure(ex);
        }
    }
}
