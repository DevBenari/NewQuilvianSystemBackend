using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
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
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/emergency-admission-requests")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Admission Request",
        AreaName = "HealthServices",
        ControllerName = "EmergencyAdmissionRequest",
        Description = "Mengelola Form Permintaan Rawat Inap dari IGD",
        SortOrder = 12
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Admission Request")]
    public class EmergencyAdmissionRequestController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";
        private const string NamaController = "EmergencyAdmissionRequest";

        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDispositionService _service;
        private readonly LoggerService _loggerService;

        public EmergencyAdmissionRequestController(
            ApplicationDbContext dbContext,
            EmergencyDispositionService service,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Melihat metadata filter permintaan rawat inap IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new EmergencyAdmissionRequestFilterMetadataResponse
            {
                DefaultFilter = new EmergencyAdmissionRequestDefaultFilterResponse(),
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortableFields = new List<string> { "createdatetime" }
            };

            return Ok(ApiResponse<EmergencyAdmissionRequestFilterMetadataResponse>.Ok(result, "Metadata filter permintaan rawat inap IGD berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Melihat ringkasan permintaan rawat inap IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgAdmissionRequest>().AsNoTracking().Where(x => !x.IsDelete);

            var summary = new EmergencyAdmissionRequestSummaryResponse
            {
                TotalAdmissionRequest = await query.CountAsync(cancellationToken),
                DraftAdmissionRequest = await query.CountAsync(x => x.Status == EmergencyAdmissionRequestStatus.Draft, cancellationToken),
                PendingSignatureAdmissionRequest = await query.CountAsync(x => x.Status == EmergencyAdmissionRequestStatus.AwaitingSignature, cancellationToken),
                SignedAdmissionRequest = await query.CountAsync(x => x.Status == EmergencyAdmissionRequestStatus.Signed, cancellationToken),
                CancelledAdmissionRequest = await query.CountAsync(x => x.Status == EmergencyAdmissionRequestStatus.Cancelled, cancellationToken)
            };

            return Ok(ApiResponse<EmergencyAdmissionRequestSummaryResponse>.Ok(summary, "Ringkasan permintaan rawat inap IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyAdmissionRequestOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Admission Request Options", Description = "Melihat pilihan Form Permintaan Rawat Inap IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? emergencyVisitId = null,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgAdmissionRequest>().AsNoTracking()
                .Where(x => !x.IsDelete);

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.ClinicalIndication != null && x.ClinicalIndication.ToLower().Contains(keyword));
            }

            var items = await query.OrderByDescending(x => x.CreateDateTime)
                .Take(100)
                .Select(x => new EmergencyAdmissionRequestOptionResponse
                {
                    Id = x.Id,
                    EmergencyVisitId = x.EmergencyVisitId,
                    RequestNumber = x.Id.ToString(),
                    RequestStatus = x.Status,
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<EmergencyAdmissionRequestOptionResponse>>.Ok(items, "Pilihan Form Permintaan Rawat Inap IGD berhasil diambil."));
        }
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyAdmissionRequestSummary>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Melihat Form Permintaan Rawat Inap IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetByVisit(
            [FromQuery] Guid emergencyVisitId,
            CancellationToken cancellationToken = default)
        {
            if (emergencyVisitId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "EmergencyVisitId wajib diisi."));

            var data = await _service.AmbilPermintaanRawatInapPerKunjunganAsync(emergencyVisitId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<EmergencyAdmissionRequestSummary>>.Ok(
                data, "Daftar Form Permintaan Rawat Inap berhasil diambil."));
        }

        [HttpGet("pending-signature")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyAdmissionRequestSummary>>), StatusCodes.Status200OK)]
        [AccessAction("Sign", "Sign Emergency Admission Request", Description = "Melihat daftar Perlu TTD, menandatangani, dan mengembalikan Form Permintaan Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyAdmissionRequest", "Sign")]
        public async Task<IActionResult> GetPendingSignature(CancellationToken cancellationToken = default)
        {
            var data = await _service.AmbilPerluTandaTanganAsync(GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<EmergencyAdmissionRequestSummary>>.Ok(
                data, "Daftar form yang perlu ditandatangani berhasil diambil."));
        }

        [HttpGet("inpatient-doctor-options")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmergencyInpatientDoctorOption>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Melihat pilihan DPJP rawat inap usulan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetInpatientDoctorOptions(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] string? search,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.AmbilPilihanDpjpRawatInapAsync(serviceUnitId, search, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<EmergencyInpatientDoctorOption>>.Ok(
                data, "Pilihan DPJP rawat inap berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Melihat rincian Form Permintaan Rawat Inap IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var hasil = await _service.AmbilPermintaanRawatInapAsync(id, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data!, "Rincian Form Permintaan Rawat Inap berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Admission Request", Description = "Membuat draf Form Permintaan Rawat Inap dari tindak lanjut rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyAdmissionRequest", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmergencyAdmissionRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.BuatPermintaanRawatInapAsync(request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("Create", "Membuat draf Form Permintaan Rawat Inap IGD.", hasil.Data!);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                    hasil.Data, "Draf Form Permintaan Rawat Inap berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Admission Request", Description = "Menyimpan isian draf dan meminta tanda tangan DPJP IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyAdmissionRequest", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEmergencyAdmissionRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.SimpanDrafPermintaanAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("Update", "Menyimpan draf Form Permintaan Rawat Inap IGD.", hasil.Data!);
            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data, "Draf Form Permintaan Rawat Inap berhasil disimpan."));
        }

        [HttpPost("{id:guid}/request-signature")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Admission Request", Description = "Menyimpan isian draf dan meminta tanda tangan DPJP IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyAdmissionRequest", "Update")]
        public async Task<IActionResult> RequestSignature(
            Guid id,
            [FromBody] EmergencyAdmissionRequestRowVersionRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.MintaTandaTanganAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("RequestSignature", "Meminta tanda tangan DPJP IGD atas Form Permintaan Rawat Inap.", hasil.Data!);
            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data, "Form Permintaan Rawat Inap dikirim ke DPJP IGD untuk ditandatangani."));
        }

        [HttpGet("{id:guid}/print-data")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestPrintResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Read", "Read Emergency Admission Request", Description = "Mengambil isi cetak Form Permintaan Rawat Inap yang sudah ditandatangani", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyAdmissionRequest", "Read")]
        public async Task<IActionResult> GetPrintData(Guid id, CancellationToken cancellationToken = default)
        {
            var hasil = await _service.AmbilDataCetakPermintaanAsync(id, cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            return Ok(ApiResponse<EmergencyAdmissionRequestPrintResponse>.Ok(
                hasil.Data!, "Isi cetak Form Permintaan Rawat Inap berhasil diambil."));
        }

        [HttpPost("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Sign", "Sign Emergency Admission Request", Description = "Melihat daftar Perlu TTD, menandatangani, dan mengembalikan Form Permintaan Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyAdmissionRequest", "Sign")]
        public async Task<IActionResult> Sign(
            Guid id,
            [FromBody] EmergencyAdmissionRequestRowVersionRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.TandatanganiPermintaanAsync(
                id,
                request,
                idempotencyKey,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                GetCurrentUserId(),
                cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("Sign", "Menandatangani Form Permintaan Rawat Inap; pasien masuk Antrean Transfer IGD/Poli.", hasil.Data!);
            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data, "Form Permintaan Rawat Inap ditandatangani. Pasien masuk Antrean Transfer IGD/Poli di Admisi Rawat Inap."));
        }

        [HttpPost("{id:guid}/return")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Sign", "Sign Emergency Admission Request", Description = "Melihat daftar Perlu TTD, menandatangani, dan mengembalikan Form Permintaan Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyAdmissionRequest", "Sign")]
        public async Task<IActionResult> Return(
            Guid id,
            [FromBody] ReturnEmergencyAdmissionRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.KembalikanPermintaanAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("Return", "Mengembalikan Form Permintaan Rawat Inap ke perawat IGD.", hasil.Data!);
            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data, "Form Permintaan Rawat Inap dikembalikan ke perawat."));
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyAdmissionRequestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel Emergency Admission Request", Description = "Membatalkan Form Permintaan Rawat Inap IGD dengan alasan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmergencyAdmissionRequest", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelEmergencyAdmissionRequestRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _service.BatalkanPermintaanAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (!hasil.Berhasil)
                return Gagal(hasil.StatusCode, hasil.Penolakan!);

            await CatatAsync("Cancel", "Membatalkan Form Permintaan Rawat Inap IGD.", hasil.Data!);
            return Ok(ApiResponse<EmergencyAdmissionRequestResponse>.Ok(
                hasil.Data, "Form Permintaan Rawat Inap berhasil dibatalkan."));
        }

        private IActionResult Gagal(int statusCode, string penolakan)
            => StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, penolakan));

        private Task CatatAsync(string action, string message, EmergencyAdmissionRequestResponse data)
            => _loggerService.InfoAsync(
                LogCategory,
                $"{NamaController}.{action}",
                message,
                new
                {
                    EntityId = data.Id,
                    data.EmergencyVisitId,
                    Status = data.Status.ToString(),
                    Controller = NamaController,
                    Action = action
                });

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
