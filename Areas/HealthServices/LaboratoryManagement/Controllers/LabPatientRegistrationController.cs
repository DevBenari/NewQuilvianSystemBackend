using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Pendaftaran pasien laboratorium (<c>FR-08.1</c> .. <c>FR-08.5</c>, <c>LAB-DEC-032</c>).
    ///
    /// <b>Layarnya milik Laboratorium, kunjungannya milik Registrasi.</b> Pasien yang hanya
    /// perlu satu pemeriksaan darah tidak lagi harus mengantre di loket pendaftaran lebih dulu
    /// — tetapi kunjungannya tetap dibuat modul Registrasi lewat <c>INT-05</c>. Ketiga endpoint
    /// di sini meneruskan isian, menunggu jawaban, lalu mengembalikan penunjuk kunjungan.
    ///
    /// Bila Registrasi menolak, penolakannya diteruskan apa adanya dan <b>tidak ada</b> data
    /// yang disimpan Laboratorium (<c>AC-45</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-patient-registrations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Patient Registration",
        AreaName = "HealthServices",
        ControllerName = "LabPatientRegistration",
        Description = "Pendaftaran pasien datang langsung dan rujukan luar dari laboratorium",
        SortOrder = 10
    )]
    [Tags("Health Services / Laboratory Management / Lab Patient Registration")]
    public class LabPatientRegistrationController : ControllerBase
    {
        private readonly LabPatientRegistrationService _labPatientRegistrationService;

        public LabPatientRegistrationController(
            LabPatientRegistrationService labPatientRegistrationService)
        {
            _labPatientRegistrationService = labPatientRegistrationService;
        }

        // Mencari pasien terdaftar sebelum mendaftarkan kunjungan baru.
        //
        // Baca saja. Gunanya mencegah pasien lama didaftarkan ulang sebagai pasien baru dan
        // berakhir punya dua nomor rekam medis.
        [HttpGet("patient-search")]
        [ProducesResponseType(typeof(ApiResponse<List<LabPatientSearchResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Patient Registration", Description = "Mencari pasien terdaftar dari layar pendaftaran laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabPatientRegistration", "Read")]
        public async Task<IActionResult> SearchPatients(
            [FromQuery] LabPatientSearchQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _labPatientRegistrationService.SearchPatientsAsync(query, cancellationToken);

            return Ok(ApiResponse<List<LabPatientSearchResponse>>.Ok(
                hasil, "Pencarian pasien berhasil dilakukan."));
        }

        // Mendaftarkan pasien yang datang langsung ke laboratorium.
        //
        // Kunjungannya dibuat Registrasi, bukan di sini. Kirim kunci idempotensi yang sama bila
        // permintaan diulang; yang kembali adalah kunjungan yang sama, bukan kunjungan kedua.
        [HttpPost("walk-in")]
        [ProducesResponseType(typeof(ApiResponse<LabRegistrationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Create", "Create Lab Patient Registration", Description = "Mendaftarkan pasien datang langsung dari laboratorium", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabPatientRegistration", "Create")]
        public async Task<IActionResult> RegisterWalkIn(
            [FromBody] RegisterLabWalkInRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hasil = await _labPatientRegistrationService.RegisterWalkInAsync(
                    request, cancellationToken);

                return Ok(ApiResponse<LabRegistrationResultResponse>.Ok(
                    hasil, PesanHasil(hasil)));
            }
            catch (Exception exception)
            {
                return MapFailure(exception);
            }
        }

        // Mendaftarkan pasien rujukan luar beserta instansi dan dokter perujuknya.
        //
        // Instansi dan dokter dikirim sebagai penunjuk ke data induk, bukan sebagai nama yang
        // diketik. Tanpa keduanya, laporan asal rujukan tidak akan pernah dapat dipercaya.
        [HttpPost("external-referral")]
        [ProducesResponseType(typeof(ApiResponse<LabRegistrationResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Create", "Create Lab Patient Registration", Description = "Mendaftarkan pasien rujukan luar dari laboratorium", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabPatientRegistration", "Create")]
        public async Task<IActionResult> RegisterExternalReferral(
            [FromBody] RegisterLabExternalReferralRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hasil = await _labPatientRegistrationService.RegisterExternalReferralAsync(
                    request, cancellationToken);

                return Ok(ApiResponse<LabRegistrationResultResponse>.Ok(
                    hasil, PesanHasil(hasil)));
            }
            catch (Exception exception)
            {
                return MapFailure(exception);
            }
        }

        private static string PesanHasil(LabRegistrationResultResponse hasil) =>
            hasil.IsReplay
                ? "Pendaftaran ini sudah tercatat sebelumnya. Kunjungan yang sama dikembalikan."
                : "Pendaftaran pasien laboratorium berhasil.";

        // Penolakan Registrasi diteruskan apa adanya, beserta status yang sudah disepakati
        // matriks validasi: VAL-40 dan VAL-43/VAL-44 menjadi 422, VAL-41 menjadi 403,
        // VAL-42 menjadi 503.
        private IActionResult MapFailure(Exception exception) => exception switch
        {
            EncounterIntakeForbiddenException =>
                StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, exception.Message)),

            LabRegistrationUnavailableException =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Fail(
                    StatusCodes.Status503ServiceUnavailable, exception.Message)),

            EncounterIntakeConflictException =>
                Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message)),

            LabPatientRegistrationValidationException or EncounterIntakeValidationException =>
                UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message)),

            _ => throw exception
        };
    }
}
