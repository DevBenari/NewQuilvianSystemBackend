using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Kelengkapan data payroll pegawai dan dokter. Daftar bersumber dari master pegawai dan dokter di HRD, bukan dari profil payroll,
/// sehingga orang yang belum punya profil tetap tampil. Payroll hanya MEMBACA data HRD (penetapan gaji dan rekening); satu-satunya
/// yang ditulis adalah profil payroll lewat impor, yang mengambil struktur, grade, penetapan gaji, dan rekening dari data HRD dan
/// menolak baris yang data HRD-nya belum lengkap. Impor semua atau tidak sama sekali dan mendukung uji kering.
/// Seluruh akses data lewat <see cref="PayrollDataReadinessService"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payroll-data-readiness")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_DATA_READINESS",
    moduleName: "Corporate Payroll Data Readiness",
    displayName: "Payroll Data Readiness",
    AreaName = "Corporate",
    ControllerName = "PayrollDataReadiness",
    Description = "Kelengkapan data payroll pegawai dan dokter (profil, penetapan gaji, rekening dari HRD) beserta impor profil payroll",
    SortOrder = 65)]
[Tags("Corporate / Payroll Management / Payroll Data Readiness")]
public sealed class PayrollDataReadinessController : ControllerBase
{
    private readonly PayrollDataReadinessService _service;

    public PayrollDataReadinessController(PayrollDataReadinessService service)
    {
        _service = service;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    [HttpGet]
    [AccessAction("Read", "Read Payroll Data Readiness", Description = "Melihat kelengkapan data payroll pegawai dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollDataReadiness", "Read")]
    public async Task<IActionResult> List([FromQuery] PayrollDataReadinessQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PayrollDataReadinessItemResponse>>.Ok(result, "Kelengkapan data payroll berhasil dimuat."));
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Payroll Data Readiness", Description = "Melihat kelengkapan data payroll pegawai dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollDataReadiness", "Read")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _service.SummaryAsync(cancellationToken);
        return Ok(ApiResponse<PayrollDataReadinessSummaryResponse>.Ok(result, "Ringkasan kelengkapan data payroll berhasil dimuat."));
    }

    /// <summary>
    /// Impor profil payroll, semua atau tidak sama sekali: bila ada baris gagal, respons 400 membawa daftar baris di
    /// <c>errors.failedRows</c>. <c>DryRun = true</c> hanya memeriksa dan menghitung hasilnya tanpa menyimpan.
    /// </summary>
    [HttpPost("import")]
    [AccessAction("Import", "Import Payroll Data", Description = "Membuat profil payroll banyak pegawai sekaligus dari data HRD yang sudah lengkap (semua atau tidak sama sekali), termasuk uji kering", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollDataReadiness", "Import")]
    public async Task<IActionResult> Import([FromBody] ImportPayrollDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ImportAsync(request, CurrentUserId(), cancellationToken);
            var message = result.DryRun
                ? "Pemeriksaan lolos. Belum ada data yang tersimpan."
                : "Profil payroll berhasil dimuat.";
            return Ok(ApiResponse<PayrollDataImportResult>.Ok(result, message));
        }
        catch (PayrollDataImportException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, ex.Result));
        }
        catch (Exception ex) when (PayrollControllerSupport.IsHandled(ex))
        {
            return PayrollControllerSupport.Failure(ex);
        }
    }
}
