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
/// sehingga orang yang belum punya profil tetap tampil. Payroll hanya MEMBACA data HRD (penetapan gaji, rekening, penempatan).
/// Profil operasional payroll disiapkan otomatis saat payroll run dipersiapkan, tanpa impor; kedua GET di sini murni baca dan tidak
/// pernah membuat profil. Seluruh akses data lewat <see cref="PayrollDataReadinessService"/>.
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
    Description = "Kelengkapan data payroll pegawai dan dokter (profil, penetapan gaji, rekening dari HRD)",
    SortOrder = 65)]
[Tags("Corporate / Payroll Management / Payroll Data Readiness")]
public sealed class PayrollDataReadinessController : ControllerBase
{
    private readonly PayrollDataReadinessService _service;

    public PayrollDataReadinessController(PayrollDataReadinessService service)
    {
        _service = service;
    }

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
    /// Impor profil payroll dari berkas dipensiunkan: profil operasional kini disiapkan otomatis dari data HRD saat payroll run dibuka
    /// atau dihitung. Endpoint ini hanya menjawab 410 Gone supaya klien lama mendapat pesan yang jelas; ia tidak membaca isi kiriman,
    /// tidak membuat atau mengubah data apa pun. Atribut hak akses dipertahankan agar registri Akses Role tidak berubah mendadak.
    /// Proses lain yang namanya mirip, <c>POST payroll-runs/{id}/import-hrd</c> (kehadiran, cuti, lembur ke satu payroll run), tidak
    /// terkait dan tidak berubah.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
    [AccessAction("Import", "Import Payroll Data (dihentikan)", Description = "Dihentikan: profil payroll disiapkan otomatis dari data HRD saat payroll run dipersiapkan. Endpoint hanya menjawab 410 dan tidak menulis data", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollDataReadiness", "Import")]
    public IActionResult Import()
    {
        return StatusCode(StatusCodes.Status410Gone, ApiResponse<object>.Fail(
            StatusCodes.Status410Gone,
            "Impor profil payroll sudah tidak tersedia. Profil payroll disiapkan otomatis dari data HRD saat payroll run dipersiapkan; lengkapi penetapan gaji dan rekening di HRD."));
    }
}
