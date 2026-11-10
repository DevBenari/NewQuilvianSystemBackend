using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Intake jasa medis ke jalur PAYROLL dengan jalur bayar tunggal (CT-PAY-14). Perhitungan dan aturan sharing tetap milik Medical Fee;
/// endpoint ini hanya menampilkan posisi jalur dan (bila kebijakan diaktifkan) mengklaim hasil jasa yang belum punya jalur lain.
/// Jalur PAYROLL nonaktif secara bawaan (NEEDS CONFIRMATION): klaim ditolak dengan 409 dan seluruh jasa medis dibayar lewat Finance AP.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/medical-fee-intake")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_MEDICAL_FEE_INTAKE",
    moduleName: "Corporate Payroll Medical Fee Intake",
    displayName: "Payroll Medical Fee Intake",
    AreaName = "Corporate",
    ControllerName = "PayrollMedicalFeeIntake",
    Description = "Posisi jalur bayar jasa medis (Payroll atau Finance AP) dan klaim ke jalur Payroll",
    SortOrder = 62)]
[Tags("Corporate / Payroll Management / Medical Fee Intake")]
public sealed class PayrollMedicalFeeIntakeController : ControllerBase
{
    private readonly PayrollMedicalFeeIntakeService _service;

    public PayrollMedicalFeeIntakeController(PayrollMedicalFeeIntakeService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Payroll Medical Fee Intake", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollMedicalFeeIntake", "Read")]
    public Task<IActionResult> GetCandidates([FromQuery] PayrollMedicalFeeQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetCandidatesAsync(query, cancellationToken), "Posisi jalur jasa medis berhasil dimuat.");

    [HttpPost]
    [AccessAction("Claim", "Claim Medical Fee Into Payroll", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollMedicalFeeIntake", "Claim")]
    public Task<IActionResult> Claim([FromBody] PayrollMedicalFeeIntakeRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.ClaimAsync(request, PayrollControllerSupport.CurrentUserId(User), cancellationToken), "Klaim jasa medis diproses.");
}
