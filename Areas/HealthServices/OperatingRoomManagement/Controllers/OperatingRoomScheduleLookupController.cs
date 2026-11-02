using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Endpoint pencarian booking rencana operasi terkonfirmasi untuk dikonsumsi Admisi Rawat Inap (BE-OPR-013, OPS-DEC-040).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/schedules")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Schedule Lookup",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomScheduleLookup",
    Description = "Pencarian data booking jadwal operasi untuk integrasi admisi rawat inap",
    SortOrder = 2)]
[Tags("Health Services / Operating Room Management / Scheduling")]
public class OperatingRoomScheduleLookupController(OperatingRoomSchedulingService service) : ControllerBase
{
    /// <summary>
    /// Mencari data rencana operasi terkonfirmasi berdasarkan patientId atau bookingNumber.
    /// </summary>
    [HttpGet("lookup-by-patient")]
    [ProducesResponseType(typeof(ApiResponse<OprPatientBookingLookupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Booking Lookup", Description = "Melihat ringkasan data booking operasi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> LookupByPatient(
        [FromQuery] Guid? patientId = null,
        [FromQuery] string? bookingNumber = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.LookupBookingByPatientAsync(patientId, bookingNumber, cancellationToken);
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
