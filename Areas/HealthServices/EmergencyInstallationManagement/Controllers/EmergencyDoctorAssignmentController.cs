using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Controllers
{
    /// <summary>
    /// Riwayat dokter penanggung jawab kunjungan IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-045</c>, API 0.7.0 bagian 3. Controller ini sengaja tipis: seluruh aturan
    /// bisnis ada pada <see cref="EmergencyDoctorAssignmentService"/>, dan di sini hanya
    /// penerjemahan hasil menjadi kode status. Nol akses langsung ke <c>DbContext</c>.
    ///
    /// <para>
    /// <b>Tidak ada endpoint pencabutan dokter tanpa pengganti</b> — validation bagian 3
    /// aturan 5. Dokter hanya berganti lewat pengalihan, supaya kunjungan yang masih berjalan
    /// tidak pernah kehilangan penanggung jawab.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/emergency-doctor-assignments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Doctor Assignment",
        AreaName = "HealthServices",
        ControllerName = "EmergencyDoctorAssignment",
        Description = "Mengelola riwayat dokter penanggung jawab kunjungan IGD",
        SortOrder = 11
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Doctor Assignment")]
    public class EmergencyDoctorAssignmentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly EmergencyDoctorAssignmentService _service;
        private readonly LoggerService _loggerService;

        public EmergencyDoctorAssignmentController(
            EmergencyDoctorAssignmentService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Riwayat penugasan dokter pada satu kunjungan IGD, urut waktu.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyDoctorAssignmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Emergency Doctor Assignment", Description = "Melihat riwayat dokter penanggung jawab IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDoctorAssignment", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid emergencyVisitId,
            CancellationToken cancellationToken = default)
        {
            if (emergencyVisitId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "EmergencyVisitId wajib diisi."));

            var data = await _service.AmbilRiwayatAsync(emergencyVisitId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<EmergencyDoctorAssignmentResponse>>.Ok(
                data, "Riwayat dokter penanggung jawab IGD berhasil diambil."));
        }

        /// <summary>
        /// Dokter penanggung jawab sekarang, atau pada waktu tertentu lewat query <c>at</c>.
        /// </summary>
        /// <remarks>
        /// <c>IGD-DEC-117</c>. Pertanyaan "siapa dokter penanggung jawab pasien ini pukul 10.30
        /// tadi" dijawab endpoint yang sama, bukan endpoint terpisah. Tidak ada dokter pada
        /// waktu yang ditanyakan menghasilkan <c>404</c>, bukan daftar kosong.
        /// </remarks>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDoctorAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Doctor Assignment", Description = "Melihat dokter penanggung jawab IGD yang aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDoctorAssignment", "Read")]
        public async Task<IActionResult> GetActive(
            [FromQuery] Guid emergencyVisitId,
            [FromQuery] DateTime? at,
            CancellationToken cancellationToken = default)
        {
            if (emergencyVisitId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "EmergencyVisitId wajib diisi."));

            var data = await _service.AmbilAktifAsync(emergencyVisitId, at, cancellationToken);
            if (data == null)
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    at.HasValue
                        ? "Tidak ada dokter penanggung jawab pada waktu yang ditanyakan."
                        : "Kunjungan ini belum memiliki dokter penanggung jawab."));

            return Ok(ApiResponse<EmergencyDoctorAssignmentResponse>.Ok(
                data, "Dokter penanggung jawab IGD berhasil diambil."));
        }

        /// <summary>
        /// Menetapkan dokter penanggung jawab pertama pada satu kunjungan IGD.
        /// </summary>
        /// <remarks>
        /// Kunjungan yang sudah punya dokter berjalan ditolak <c>409</c>; pengalihan wajib
        /// memakai aksi <c>handover</c> supaya baris lama memperoleh waktu berakhir dan
        /// alasannya tercatat.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDoctorAssignmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Doctor Assignment", Description = "Menetapkan dokter penanggung jawab IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyDoctorAssignment", "Create")]
        public async Task<IActionResult> Assign(
            [FromBody] AssignEmergencyDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.TetapkanAsync(request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode,
                    ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDoctorAssignment.Assign",
                "Menetapkan dokter penanggung jawab IGD.",
                new
                {
                    EntityId = hasil.Data!.Id,
                    hasil.Data.EmergencyVisitId,
                    hasil.Data.DoctorId,
                    Controller = "EmergencyDoctorAssignment",
                    Action = "Assign"
                });

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<EmergencyDoctorAssignmentResponse>.Ok(
                    hasil.Data, "Dokter penanggung jawab IGD berhasil ditetapkan."));
        }

        /// <summary>
        /// Mengalihkan penugasan berjalan ke dokter lain; alasan wajib.
        /// </summary>
        /// <remarks>
        /// Baris lama ditutup dan baris baru dibuka dalam satu transaksi, jadi riwayatnya tidak
        /// pernah berlubang dan tidak pernah memuat dua dokter berjalan.
        /// </remarks>
        [HttpPost("{id:guid}/handover")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDoctorAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Doctor Assignment", Description = "Mengalihkan dokter penanggung jawab IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDoctorAssignment", "Update")]
        public async Task<IActionResult> Handover(
            Guid id,
            [FromBody] HandoverEmergencyDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.AlihkanAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode,
                    ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyDoctorAssignment.Handover",
                "Mengalihkan dokter penanggung jawab IGD.",
                new
                {
                    EntityId = hasil.Data!.Id,
                    PenugasanSebelumnya = id,
                    hasil.Data.EmergencyVisitId,
                    hasil.Data.DoctorId,
                    Controller = "EmergencyDoctorAssignment",
                    Action = "Handover"
                });

            return Ok(ApiResponse<EmergencyDoctorAssignmentResponse>.Ok(
                hasil.Data, "Dokter penanggung jawab IGD berhasil dialihkan."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
