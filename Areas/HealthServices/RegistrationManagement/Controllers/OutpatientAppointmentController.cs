using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Tracking Jadwal Booking Rawat Jalan — daftar, detail, dan reschedule appointment
    /// (RJ-REG-REV-BE-005, RJ-REG-DEC-007).
    /// </summary>
    /// <remarks>
    /// <b>Arketipe transaksi: worklist + satu aksi.</b> Booking adalah kunjungan Rawat Jalan
    /// ber-<c>IsAppointment</c>; perpindahan jadwal memakai <c>POST /{id}/reschedule</c>.
    /// Tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>, maupun <c>DELETE</c> —
    /// pembatalan tetap lewat Daftar Pasien Rawat Jalan. Controller tidak menyentuh
    /// <c>ApplicationDbContext</c> (<c>QBE-SVC-001</c>).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/outpatient-appointments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Outpatient Appointment",
        AreaName = "HealthServices",
        ControllerName = "OutpatientAppointment",
        Description = "Tracking jadwal booking (appointment) Rawat Jalan dan reschedule",
        SortOrder = 4)]
    [Tags("Health Services / Registration Management / Outpatient Appointment")]
    public class OutpatientAppointmentController : ControllerBase
    {
        private readonly OutpatientAppointmentService _service;

        public OutpatientAppointmentController(OutpatientAppointmentService service)
        {
            _service = service;
        }

        /// <summary>Daftar booking Rawat Jalan berhalaman.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OutpatientAppointmentListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Outpatient Appointment", Description = "Melihat daftar jadwal booking rawat jalan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientAppointment", "Read")]
        public Task<IActionResult> GetList(
            [FromQuery] OutpatientAppointmentListQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<PagedResult<OutpatientAppointmentListItemResponse>>.Ok(
                await _service.GetPagedAsync(User, query, cancellationToken),
                "Daftar jadwal booking berhasil diambil.")));

        /// <summary>Opsi saringan: status booking, poliklinik, dan dokter.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientAppointmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Outpatient Appointment", Description = "Melihat metadata filter jadwal booking", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientAppointment", "Read")]
        public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientAppointmentFilterMetadataResponse>.Ok(
                await _service.GetFilterMetadataAsync(cancellationToken),
                "Metadata filter jadwal booking berhasil diambil.")));

        /// <summary>Detail booking beserta riwayat reschedule.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientAppointmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Outpatient Appointment", Description = "Melihat detail jadwal booking", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OutpatientAppointment", "Read")]
        public Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientAppointmentDetailResponse>.Ok(
                await _service.GetDetailAsync(User, id, cancellationToken),
                "Detail jadwal booking berhasil diambil.")));

        /// <summary>
        /// Menjadwal ulang appointment yang belum datang ke tanggal dan/atau jadwal dokter lain
        /// pada poliklinik yang sama. Antrean kunjungan dipindah ke tanggal baru dengan nomor baru
        /// dari alokator yang sama (ID antrean tetap); riwayat perubahan tersimpan.
        /// </summary>
        [HttpPost("{id:guid}/reschedule")]
        [ProducesResponseType(typeof(ApiResponse<OutpatientAppointmentRescheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Reschedule", "Reschedule Outpatient Appointment", Description = "Mengubah jadwal booking rawat jalan yang belum datang", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OutpatientAppointment", "Reschedule")]
        public Task<IActionResult> Reschedule(
            Guid id,
            [FromBody] OutpatientAppointmentRescheduleRequest? request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(async () => Ok(ApiResponse<OutpatientAppointmentRescheduleResponse>.Ok(
                await _service.RescheduleAsync(User, id, request, cancellationToken),
                "Jadwal booking berhasil diubah.")));

        private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (OutpatientEncounterNotFoundException)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Jadwal booking tidak ditemukan."));
            }
            catch (OutpatientAppointmentConflictException exception)
            {
                return StatusCode(
                    StatusCodes.Status409Conflict,
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
            }
            catch (OutpatientEncounterValidationException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
