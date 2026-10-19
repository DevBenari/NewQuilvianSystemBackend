using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Keadaan penahanan stok sebuah resep (<c>PHA-DEC-066</c>, <c>PHA-FE-001</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Hanya satu aksi, dan hanya membaca.</b> Tidak ada endpoint untuk menahan, melepas, atau
/// menimpa penahanan, dan itu bukan kelalaian: <c>PHA-DEC-066</c> butir 1 menetapkan penahanan
/// dibuat saat dokter memfinalkan resep, dan butir 5 menetapkan pelepasannya dilakukan sistem.
/// Petugas Farmasi tidak memegang kewenangan mengubahnya, sehingga tombolnya pun tidak boleh
/// ada.
/// </para>
/// <para>
/// Yang disediakan di sini adalah keterangan supaya petugas mengerti mengapa sebuah resep dapat
/// atau tidak dapat dikerjakan — bukan sarana menyiasatinya.
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/prescriptions/{prescriptionId:guid}/stock-reservation")]
[Tags("Health Services / Pharmacy Management / Prescription Stock Reservation")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Prescription Stock Reservation",
    AreaName = "HealthServices",
    ControllerName = "PrescriptionStockReservation",
    Description = "Keadaan penahanan stok resep",
    SortOrder = 8)]
public class PrescriptionStockReservationController : ControllerBase
{
    private readonly PrescriptionStockReservationService _service;

    public PrescriptionStockReservationController(PrescriptionStockReservationService service) =>
        _service = service;

    /// <summary>
    /// Keadaan penahanan stok resep beserta alasan bila pekerjaan tidak dapat dilanjutkan.
    /// </summary>
    /// <remarks>
    /// Resep yang tidak pernah memegang penahanan dijawab <c>200</c> dengan keadaan
    /// <c>NOT_RESERVED</c>, bukan <c>404</c>. Resepnya ada; yang tidak ada hanya penahanannya,
    /// dan membedakan keduanya penting bagi layar yang harus menjelaskan sebabnya.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionStockReservationStateResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Prescription Stock Reservation",
        Description = "Melihat keadaan penahanan stok resep",
        AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PrescriptionStockReservation", "Read")]
    public async Task<IActionResult> Get(Guid prescriptionId, CancellationToken cancellationToken)
    {
        var data = await _service.DescribeAsync(prescriptionId, DateTime.UtcNow, cancellationToken);

        return Ok(ApiResponse<PrescriptionStockReservationStateResponse>.Ok(data,
            "Keadaan penahanan stok resep berhasil diambil."));
    }
}
