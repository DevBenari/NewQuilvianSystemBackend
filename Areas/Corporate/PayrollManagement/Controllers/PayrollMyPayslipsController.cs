using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Slip gaji milik sendiri (CT-PAY-11). Pemilik DITURUNKAN dari pengguna yang login (ApplicationUser.WorkforceProfileId); tidak ada
/// parameter identitas dari klien. Hanya slip yang sudah diterbitkan; slip milik orang lain dijawab 404. Setiap unduhan dicatat audit.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payslips/me")]
[Route("api/v1/self-services/payroll/payslips")]
[AccessController(
    moduleCode: "PAYROLL_EMPLOYEE_SELF_SERVICE",
    moduleName: "Payroll Employee Self Service",
    displayName: "My Payslip",
    AreaName = "SelfServices",
    ControllerName = "MyPayslip",
    Description = "Penerima melihat dan mengunduh slip gaji miliknya sendiri",
    SortOrder = 60)]
[Tags("Self Services / Payroll / Payslip")]
public sealed class PayrollMyPayslipsController : ControllerBase
{
    private readonly PayrollPayslipService _service;

    public PayrollMyPayslipsController(PayrollPayslipService service)
    {
        _service = service;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    [HttpGet]
    [AccessAction("Read", "Read My Payslips", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MyPayslip", "Read")]
    public Task<IActionResult> GetMine([FromQuery] PayrollPayslipQuery query, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId == Guid.Empty) return Task.FromResult<IActionResult>(Unauthorized(ApiResponse<object>.Fail(401, "Identitas pengguna tidak valid.")));
        return PayrollControllerSupport.RunAsync(() => _service.GetMineAsync(userId, query, cancellationToken), "Slip gaji berhasil dimuat.");
    }

    [HttpGet("{id:guid}")]
    [AccessAction("ReadDetail", "Read My Payslip Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("MyPayslip", "ReadDetail")]
    public Task<IActionResult> GetMineDetail(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId == Guid.Empty) return Task.FromResult<IActionResult>(Unauthorized(ApiResponse<object>.Fail(401, "Identitas pengguna tidak valid.")));
        return PayrollControllerSupport.RunAsync(
            () => _service.GetDetailAsync(id, ownerUserId: userId, includeAmounts: true, cancellationToken), "Detail slip berhasil dimuat.");
    }

    [HttpGet("{id:guid}/download")]
    [AccessAction("Download", "Download My Payslip", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("MyPayslip", "Download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(401, "Identitas pengguna tidak valid."));
        try
        {
            var file = await _service.DownloadAsync(id, userId, ownerOnly: true, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception ex) when (PayrollControllerSupport.IsHandled(ex))
        {
            return PayrollControllerSupport.Failure(ex);
        }
    }
}
