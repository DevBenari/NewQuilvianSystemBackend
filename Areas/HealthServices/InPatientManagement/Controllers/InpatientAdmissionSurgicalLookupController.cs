using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers;

/// <summary>
/// Endpoint handshake pembacaan jadwal rencana operasi untuk kebutuhan admisi rawat inap (BE-RWI-182, RWI-DEC-356, OPS-DEC-040).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/inpatient-management/admissions")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_INPATIENT_MANAGEMENT",
    moduleName: "Health Service Inpatient Management",
    displayName: "Inpatient Admission Surgical Lookup",
    AreaName = "HealthServices",
    ControllerName = "InpatientAdmissionSurgicalLookup",
    Description = "Lookup rencana operasi IBS untuk integrasi pendaftaran rawat inap",
    SortOrder = 5)]
[Tags("Health Services / Inpatient Management / Admissions")]
public class InpatientAdmissionSurgicalLookupController(OperatingRoomSchedulingService oprSchedulingService) : ControllerBase
{
    /// <summary>
    /// Mencari data rencana operasi terkonfirmasi dari modul IBS secara read-only.
    /// </summary>
    [HttpGet("surgical-booking-lookup")]
    [ProducesResponseType(typeof(ApiResponse<OprPatientBookingLookupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AccessAction("Read", "Read Surgical Booking Lookup", Description = "Membaca data booking operasi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("InpatientAdmission", "Read")]
    public async Task<IActionResult> LookupSurgicalBooking(
        [FromQuery] Guid? patientId = null,
        [FromQuery] string? bookingNumber = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await oprSchedulingService.LookupBookingByPatientAsync(patientId, bookingNumber, cancellationToken);
            if (result == null)
            {
                return Ok(ApiResponse<OprPatientBookingLookupResponse?>.Ok(null, "Tidak ada jadwal operasi aktif untuk pasien ini."));
            }

            return Ok(ApiResponse<OprPatientBookingLookupResponse>.Ok(result, "Data jadwal operasi pasien berhasil ditemukan."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
