using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Penyerahan obat resep kepada pasien.
/// </summary>
/// <remarks>
/// <para>
/// Dua langkah. <c>Prepare</c> menahan stok — barangnya belum berpindah, tetapi sudah tidak
/// dapat dijanjikan kepada pasien lain. <c>Dispense</c> baru mengurangi saldo fisiknya sambil
/// melepas tahanan itu.
/// </para>
/// <para>
/// Penyerahan bertahap didukung: satu baris resep boleh diserahkan beberapa kali sampai
/// sisanya nol. Jumlah sisa dan penanda <c>det</c>/<c>nedet</c> dihitung dari histori
/// penyerahan, tidak pernah disimpan sebagai status yang dapat diisi petugas.
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/prescriptions/{prescriptionId:guid}/dispensing")]
[Tags("Health Services / Pharmacy Management / Prescription Dispensing")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Prescription Dispensing",
    AreaName = "HealthServices",
    ControllerName = "PrescriptionDispensing",
    Description = "Penyerahan obat resep per baris",
    SortOrder = 7)]
public class PrescriptionDispensingController : ControllerBase
{
    private readonly PrescriptionDispensingService _service;

    public PrescriptionDispensingController(PrescriptionDispensingService service) =>
        _service = service;

    /// <summary>
    /// Rekapitulasi penyerahan seluruh baris resep.
    /// </summary>
    /// <remarks>
    /// Inilah bahan copy resep: jumlah diresepkan, sudah diserahkan, sisa, dan penanda
    /// <c>det</c>/<c>nedet</c> per baris, beserta histori penyerahannya.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionDispensingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Prescription Dispensing",
        Description = "Melihat rekapitulasi dan histori penyerahan obat resep",
        AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PrescriptionDispensing", "Read")]
    public async Task<IActionResult> GetSummary(Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetSummaryAsync(prescriptionId, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Resep tidak ditemukan."))
            : Ok(ApiResponse<PrescriptionDispensingSummaryResponse>.Ok(data,
                "Rekapitulasi penyerahan berhasil diambil."));
    }

    /// <summary>
    /// Menyiapkan penyerahan dan menahan stoknya.
    /// </summary>
    /// <remarks>
    /// Belum mengurangi saldo. Jumlah yang diminta tidak boleh melebihi sisa dikurangi yang
    /// sedang ditahan penyiapan lain.
    /// </remarks>
    [HttpPost("prepare")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionDispensingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Prepare", "Prepare Prescription Dispensing",
        Description = "Menyiapkan penyerahan dan menahan stok",
        AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("PrescriptionDispensing", "Prepare")]
    public Task<IActionResult> Prepare(Guid prescriptionId,
        [FromBody] PreparePrescriptionDispensingRequest request,
        CancellationToken cancellationToken) =>
        RunAsync(() => _service.PrepareAsync(prescriptionId, request, cancellationToken),
            "Penyiapan penyerahan berhasil disimpan.");

    /// <summary>
    /// Menyerahkan obat yang sudah disiapkan.
    /// </summary>
    /// <remarks>
    /// Di sinilah saldo fisik berkurang, tepat satu kali. Pemanggilan ulang atas penyerahan
    /// yang sudah tercatat mengembalikan keadaan apa adanya tanpa memotong stok lagi.
    /// </remarks>
    [HttpPatch("{drugUsageId:guid}/dispense")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionDispensingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Dispense", "Dispense Prescription",
        Description = "Menyerahkan obat resep dan mengurangi stok",
        AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PrescriptionDispensing", "Dispense")]
    public Task<IActionResult> Dispense(Guid prescriptionId, Guid drugUsageId,
        [FromBody] PrescriptionDispensingCommandRequest request,
        CancellationToken cancellationToken) =>
        RunAsync(() => _service.DispenseAsync(prescriptionId, drugUsageId, request, cancellationToken),
            "Obat berhasil diserahkan.");

    /// <summary>
    /// Membatalkan penyiapan dan melepas tahanan stoknya.
    /// </summary>
    /// <remarks>
    /// Hanya selama obat belum diserahkan. Yang sudah diserahkan dikembalikan lewat retur
    /// obat, karena stoknya sudah keluar dan kelayakannya wajib diperiksa apoteker.
    /// </remarks>
    [HttpPatch("{drugUsageId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionDispensingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Cancel", "Cancel Prescription Dispensing",
        Description = "Membatalkan penyiapan penyerahan",
        AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PrescriptionDispensing", "Cancel")]
    public Task<IActionResult> Cancel(Guid prescriptionId, Guid drugUsageId,
        [FromBody] CancelPrescriptionDispensingRequest request,
        CancellationToken cancellationToken) =>
        RunAsync(() => _service.CancelAsync(prescriptionId, drugUsageId, request, cancellationToken),
            "Penyiapan penyerahan berhasil dibatalkan.");

    /// <summary>
    /// Menerjemahkan kegagalan domain menjadi status HTTP yang sesuai.
    /// </summary>
    /// <remarks>
    /// Dikumpulkan di satu tempat supaya keempat endpoint di atas tidak masing-masing
    /// mengulang blok catch yang sama, dan supaya tidak ada kegagalan bisnis yang lolos
    /// menjadi `500`.
    /// </remarks>
    private async Task<IActionResult> RunAsync(
        Func<Task<PrescriptionDispensingSummaryResponse>> action, string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<PrescriptionDispensingSummaryResponse>.Ok(data, message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                exception.Message));
        }
        catch (PrescriptionDispensingConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                exception.Message, new { code = exception.Code }));
        }
        catch (PrescriptionDispensingUnprocessableException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { code = exception.Code }));
        }
        catch (PrescriptionDispensingForbiddenException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
        }
    }
}
