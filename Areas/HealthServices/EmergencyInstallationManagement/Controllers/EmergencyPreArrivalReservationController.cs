using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
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
    [Route("api/v1/health-services/emergency-installation-management/pre-arrival-reservations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Pre-Arrival Reservation",
        AreaName = "HealthServices",
        ControllerName = "EmergencyPreArrivalReservation",
        Description = "Mengelola reservasi pra-kedatangan dan penjemputan ambulans pasien prioritas IGD",
        SortOrder = 14
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Pre-Arrival Reservation")]
    public class EmergencyPreArrivalReservationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";
        private const string NamaController = "EmergencyPreArrivalReservation";

        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyPreArrivalReservationService _service;
        private readonly LoggerService _loggerService;

        public EmergencyPreArrivalReservationController(
            ApplicationDbContext dbContext,
            EmergencyPreArrivalReservationService service,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Pre-Arrival Reservation", Description = "Melihat metadata filter reservasi pra-kedatangan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyPreArrivalReservation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new EmergencyPreArrivalReservationFilterMetadataResponse
            {
                DefaultFilter = new EmergencyPreArrivalReservationDefaultFilterResponse(),
                PageSizeOptions = new List<int> { 10, 20, 50, 100 },
                SortableFields = new List<string> { "estimatedarrivaldatetime", "createdatetime" }
            };

            return Ok(ApiResponse<EmergencyPreArrivalReservationFilterMetadataResponse>.Ok(result, "Metadata filter reservasi pra-kedatangan IGD berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Pre-Arrival Reservation", Description = "Melihat ringkasan reservasi pra-kedatangan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyPreArrivalReservation", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgPreArrivalReservation>().AsNoTracking().Where(x => !x.IsDelete);

            var summary = new EmergencyPreArrivalReservationSummaryResponse
            {
                TotalReservation = await query.CountAsync(cancellationToken),
                IncomingReservation = await query.CountAsync(x => x.Status == EmergencyPreArrivalStatus.PendingArrival, cancellationToken),
                CheckedInReservation = await query.CountAsync(x => x.Status == EmergencyPreArrivalStatus.Arrived, cancellationToken),
                CancelledReservation = await query.CountAsync(x => x.Status == EmergencyPreArrivalStatus.Cancelled, cancellationToken),
                ExpiredReservation = 0
            };

            return Ok(ApiResponse<EmergencyPreArrivalReservationSummaryResponse>.Ok(summary, "Ringkasan reservasi pra-kedatangan IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyPreArrivalReservationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Pre-Arrival Reservation Options", Description = "Melihat pilihan reservasi pra-kedatangan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyPreArrivalReservation", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgPreArrivalReservation>().AsNoTracking()
                .Include(x => x.Patient)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.ReservationNumber.ToLower().Contains(keyword)
                    || x.CallerName.ToLower().Contains(keyword)
                    || (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            var items = await query.OrderByDescending(x => x.EstimatedArrivalDateTime)
                .Take(100)
                .Select(x => new EmergencyPreArrivalReservationOptionResponse
                {
                    Id = x.Id,
                    ReservationNumber = x.ReservationNumber,
                    PatientName = x.Patient != null ? x.Patient.FullName : x.CallerName,
                    Status = x.Status,
                    EstimatedArrivalDateTime = x.EstimatedArrivalDateTime
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<EmergencyPreArrivalReservationOptionResponse>>.Ok(items, "Pilihan reservasi pra-kedatangan IGD berhasil diambil."));
        }
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyPreArrivalReservationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Pre-Arrival Reservation", Description = "Melihat data reservasi pra-kedatangan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyPreArrivalReservation", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] EmergencyPreArrivalStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetListAsync(search, status, fromDate, toDate, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<EmergencyPreArrivalReservationResponse>>.Ok(
                data, "Daftar reservasi pra-kedatangan IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Pre-Arrival Reservation", Description = "Melihat data reservasi pra-kedatangan IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyPreArrivalReservation", "Read")]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetDetailAsync(id, cancellationToken);
            if (data == null)
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, "Reservasi pra-kedatangan tidak ditemukan."));

            return Ok(ApiResponse<EmergencyPreArrivalReservationResponse>.Ok(
                data, "Detail reservasi pra-kedatangan berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Emergency Pre-Arrival Reservation", Description = "Membuat reservasi pra-kedatangan IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyPreArrivalReservation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmergencyPreArrivalReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var hasil = await _service.CreateReservationAsync(request, actorUserId, cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyPreArrivalReservation.Create",
                "Membuat data reservasi pra-kedatangan IGD.",
                new { EntityId = hasil.Data!.Id, Controller = NamaController, Action = "Create", hasil.Data.ReservationNumber }
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = hasil.Data.Id },
                ApiResponse<EmergencyPreArrivalReservationResponse>.Ok(hasil.Data, "Reservasi pra-kedatangan IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Pre-Arrival Reservation", Description = "Mengubah data reservasi pra-kedatangan IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyPreArrivalReservation", "Update")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateEmergencyPreArrivalReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var hasil = await _service.UpdateReservationAsync(id, request, actorUserId, cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyPreArrivalReservation.Update",
                "Memperbarui data reservasi pra-kedatangan IGD.",
                new { EntityId = id, Controller = NamaController, Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyPreArrivalReservationResponse>.Ok(
                hasil.Data!, "Reservasi pra-kedatangan IGD berhasil diperbarui."));
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Pre-Arrival Reservation", Description = "Mengubah data reservasi pra-kedatangan IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyPreArrivalReservation", "Update")]
        public async Task<IActionResult> Cancel(
            [FromRoute] Guid id,
            [FromBody] CancelEmergencyPreArrivalReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var hasil = await _service.CancelReservationAsync(id, request, actorUserId, cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyPreArrivalReservation.Cancel",
                "Membatalkan reservasi pra-kedatangan IGD.",
                new { EntityId = id, Controller = NamaController, Action = "Cancel", request.CancellationReason }
            );

            return Ok(ApiResponse<EmergencyPreArrivalReservationResponse>.Ok(
                hasil.Data!, "Reservasi pra-kedatangan IGD berhasil dibatalkan."));
        }

        [HttpPost("{id:guid}/check-in")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyPreArrivalReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Pre-Arrival Reservation", Description = "Mengubah data reservasi pra-kedatangan IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyPreArrivalReservation", "Update")]
        public async Task<IActionResult> CheckIn(
            [FromRoute] Guid id,
            [FromBody] CheckInEmergencyPreArrivalReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var hasil = await _service.CheckInReservationAsync(id, request, actorUserId, cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyPreArrivalReservation.CheckIn",
                "Melakukan check-in fisik kedatangan reservasi pra-kedatangan IGD.",
                new { EntityId = id, Controller = NamaController, Action = "CheckIn", hasil.Data!.ResultEmergencyVisitId, hasil.Data.EmergencyVisitNumber }
            );

            return Ok(ApiResponse<EmergencyPreArrivalReservationResponse>.Ok(
                hasil.Data, "Check-in reservasi berhasil. Kunjungan IGD aktif telah dibuat."));
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
        }
    }
}
