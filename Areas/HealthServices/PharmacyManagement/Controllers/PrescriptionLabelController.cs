using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Bahan cetak etiket obat.
/// </summary>
/// <remarks>
/// Hanya membaca. Mencetak etiket tidak mengubah keadaan resep maupun stok; ia menyalin apa
/// yang sudah tercatat ke atas kertas.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/prescription-labels")]
[Tags("Health Services / Pharmacy Management / Prescription Label")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Prescription Label",
    AreaName = "HealthServices",
    ControllerName = "PrescriptionLabel",
    Description = "Etiket obat",
    SortOrder = 6)]
public class PrescriptionLabelController : ControllerBase
{
    private readonly PrescriptionLabelService _service;

    public PrescriptionLabelController(PrescriptionLabelService service) => _service = service;

    /// <summary>
    /// Bahan etiket untuk seluruh item satu resep.
    /// </summary>
    /// <remarks>
    /// Isinya berasal dari snapshot pada resep, bukan dibaca ulang dari master obat: etiket
    /// harus mencerminkan obat sebagaimana diresepkan saat itu.
    /// </remarks>
    [HttpGet("{prescriptionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionLabelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Prescription Label",
        Description = "Melihat dan mencetak etiket obat", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PrescriptionLabel", "Read")]
    public async Task<IActionResult> GetByPrescription(Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetAsync(prescriptionId, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Resep tidak ditemukan."))
            : Ok(ApiResponse<PrescriptionLabelResponse>.Ok(data,
                "Bahan etiket berhasil diambil."));
    }
}
