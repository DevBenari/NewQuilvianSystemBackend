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
/// Data Penghasilan Pegawai dan Dokter (CT-PAY-02). Arketipe: monitoring read-only (standar transaksi 2.4) — tidak membuat atau
/// mengubah kompensasi; penetapan gaji, rekening, dan identitas tetap dikelola HRD lewat endpoint miliknya.
/// Nominal gaji hanya terisi bagi pemanggil yang berwenang ViewAmount; rekening dan NPWP selalu disamarkan.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/workforce-compensations")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_COMPENSATION",
    moduleName: "Corporate Payroll Compensation",
    displayName: "Payroll Compensation",
    AreaName = "Corporate",
    ControllerName = "PayrollCompensation",
    Description = "Data penghasilan pegawai dan dokter: profil penggajian, penetapan gaji berlaku, rekening, kelengkapan",
    SortOrder = 57)]
[Tags("Corporate / Payroll Management / Workforce Compensation")]
public sealed class PayrollCompensationsController : ControllerBase
{
    private const string Resource = "PayrollCompensation";

    private readonly PayrollCompensationService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollCompensationsController(PayrollCompensationService service, AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");

    [HttpGet]
    [AccessAction("Read", "Read Payroll Compensation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollCompensation", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PayrollCompensationQuery query, CancellationToken cancellationToken)
    {
        var includeAmounts = await CanViewAmountAsync();
        var result = await _service.GetPagedAsync(query, includeAmounts, cancellationToken);
        return Ok(ApiResponse<PagedResult<PayrollCompensationListItemResponse>>.Ok(result, "Data penghasilan berhasil dimuat."));
    }

    [HttpGet("{workforceProfileId:guid}")]
    [AccessAction("ReadDetail", "Read Payroll Compensation Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollCompensation", "ReadDetail")]
    public async Task<IActionResult> GetDetail(Guid workforceProfileId, CancellationToken cancellationToken)
    {
        var includeAmounts = await CanViewAmountAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetDetailAsync(workforceProfileId, includeAmounts, cancellationToken), "Detail penghasilan berhasil dimuat.");
    }

    /// <summary>
    /// Pintu izin nominal (pola ViewAmount di repo): endpoint ini sengaja ada agar kemampuan "melihat nominal gaji" muncul di layar
    /// Akses Role. Pemilik izin ini juga melihat nominal pada daftar dan detail; tanpa izin, kolom nominal null.
    /// </summary>
    [HttpGet("{workforceProfileId:guid}/amounts")]
    [AccessAction("ViewAmount", "View Payroll Compensation Amount", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("PayrollCompensation", "ViewAmount")]
    public Task<IActionResult> GetAmounts(Guid workforceProfileId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.GetDetailAsync(workforceProfileId, includeAmounts: true, cancellationToken),
            "Nominal penghasilan berhasil dimuat.");
}
