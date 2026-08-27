using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-departures")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Departure",
        AreaName = "HealthServices",
        ControllerName = "EmergencyDeparture",
        Description = "Mengelola kepergian pasien, serah terima, dan pesanan yang menyertainya",
        SortOrder = 9)]
    [Tags("Health Services / Emergency Installation Management / Emergency Departure")]
    public class EmergencyDepartureController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";
        private readonly EmergencyDepartureService _service;
        private readonly LoggerService _logger;

        public EmergencyDepartureController(EmergencyDepartureService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [AccessAction("Read", "Read Emergency Departure", Description = "Melihat daftar kepergian pasien IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDeparture", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] EmergencyPhysicalStatus? physicalStatus,
            [FromQuery] EmergencyHandoverStatus? handoverStatus,
            [FromQuery] Guid? toServiceUnitId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _service.Query();
            if (emergencyVisitId.HasValue) query = query.Where(x => x.EmergencyVisitId == emergencyVisitId);
            if (physicalStatus.HasValue) query = query.Where(x => x.PhysicalStatus == physicalStatus);
            if (handoverStatus.HasValue) query = query.Where(x => x.HandoverStatus == handoverStatus);
            if (toServiceUnitId.HasValue) query = query.Where(x => x.ToServiceUnitId == toServiceUnitId);
            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(x => x.RequestedAt)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return Ok(ApiResponse<PagedResult<EmergencyDepartureResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items.Select(EmergencyDepartureService.ToResponse).ToList()
            }, "Data kepergian pasien IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Emergency Departure", Description = "Melihat detail kepergian pasien IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDeparture", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _service.FindAsync(id, cancellationToken);
            return entity == null
                ? NotFound(ApiResponse<object>.Fail(404, "Data kepergian pasien IGD tidak ditemukan."))
                : Ok(ApiResponse<EmergencyDepartureResponse>.Ok(EmergencyDepartureService.ToResponse(entity), "Detail kepergian pasien IGD berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Emergency Departure", Description = "Membuat catatan kepergian pasien IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyDeparture", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyDepartureRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateAsync(request, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync("EmergencyDeparture.Create", result.Data!.Id);
            return StatusCode(201, ApiResponse<EmergencyDepartureResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Kepergian pasien IGD berhasil dibuat."));
        }

        [HttpGet("{id:guid}/order-items")]
        [AccessAction("Read", "Read Emergency Departure Orders", Description = "Melihat pesanan pada kepergian pasien IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyDeparture", "Read")]
        public async Task<IActionResult> GetOrderItems(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _service.FindAsync(id, cancellationToken);
            return entity == null
                ? NotFound(ApiResponse<object>.Fail(404, "Data kepergian pasien IGD tidak ditemukan."))
                : Ok(ApiResponse<List<EmergencyHandoverOrderItemResponse>>.Ok(
                    entity.OrderItems.OrderBy(x => x.ActionAt).Select(EmergencyDepartureService.ToResponse).ToList(),
                    "Daftar pesanan kepergian berhasil diambil."));
        }

        [HttpPost("{id:guid}/order-items")]
        [AccessAction("Update", "Add Emergency Departure Order", Description = "Mendaftarkan pesanan luar sistem", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public async Task<IActionResult> AddExternalOrder(Guid id, [FromBody] EmergencyHandoverOrderItemInput request, CancellationToken cancellationToken = default)
        {
            var result = await _service.AddExternalOrderAsync(id, request, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync("EmergencyDeparture.AddExternalOrder", result.Data!.Id);
            return StatusCode(201, ApiResponse<EmergencyHandoverOrderItemResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Pesanan luar sistem berhasil dicatat."));
        }

        [HttpPatch("{id:guid}/order-items/{itemId:guid}/action")]
        [AccessAction("Update", "Set Emergency Departure Order Action", Description = "Menetapkan sikap pesanan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public async Task<IActionResult> SetOrderAction(Guid id, Guid itemId, [FromBody] SetOrderItemActionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.SetOrderActionAsync(id, itemId, request.Item, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync("EmergencyDeparture.SetOrderAction", result.Data!.Id);
            return Ok(ApiResponse<EmergencyHandoverOrderItemResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Sikap pesanan berhasil dicatat."));
        }

        [HttpPost("{id:guid}/order-items/{itemId:guid}/accept")]
        [AccessAction("Update", "Accept Emergency Departure Order", Description = "Menerima pesanan yang diserahterimakan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> AcceptOrder(Guid id, Guid itemId, CancellationToken cancellationToken = default)
            => SetOrderAcceptance(id, itemId, EmergencyOrderAcceptanceStatus.Accepted, null, cancellationToken);

        [HttpPost("{id:guid}/order-items/{itemId:guid}/reject")]
        [AccessAction("Update", "Reject Emergency Departure Order", Description = "Menolak pesanan yang diserahterimakan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> RejectOrder(Guid id, Guid itemId, [FromBody] SetOrderItemAcceptanceRequest request, CancellationToken cancellationToken = default)
            => SetOrderAcceptance(id, itemId, EmergencyOrderAcceptanceStatus.Rejected, request.RejectionReason, cancellationToken);

        [HttpPost("{id:guid}/submit-handover")]
        [AccessAction("Update", "Submit Emergency Handover", Description = "Mengajukan dokumen serah terima", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> SubmitHandover(Guid id, CancellationToken cancellationToken = default)
            => ExecuteDeparture(id, "EmergencyDeparture.SubmitHandover",
                ct => _service.SubmitHandoverAsync(id, UserId(), ct), cancellationToken);

        [HttpPost("{id:guid}/depart")]
        [AccessAction("Update", "Depart Emergency Patient", Description = "Mencatat pasien meninggalkan IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> Depart(Guid id, [FromBody] DepartEmergencyDepartureRequest request, CancellationToken cancellationToken = default)
            => ExecuteDeparture(id, "EmergencyDeparture.Depart",
                ct => _service.DepartAsync(id, request, UserId(), ct), cancellationToken);

        [HttpPost("{id:guid}/arrive")]
        [AccessAction("Update", "Arrive Emergency Patient", Description = "Mencatat pasien tiba di unit tujuan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> Arrive(Guid id, [FromBody] ArriveEmergencyDepartureRequest request, CancellationToken cancellationToken = default)
            => ExecuteDeparture(id, "EmergencyDeparture.Arrive",
                ct => _service.ArriveAsync(id, request, UserId(), ct), cancellationToken);

        [HttpPost("{id:guid}/accept-handover")]
        [AccessAction("Update", "Accept Emergency Handover", Description = "Menerima dokumen serah terima", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> AcceptHandover(Guid id, [FromBody] UpdateEmergencyHandoverStatusRequest request, CancellationToken cancellationToken = default)
        {
            request.HandoverStatus = EmergencyHandoverStatus.Accepted;
            return ExecuteDeparture(id, "EmergencyDeparture.AcceptHandover",
                ct => _service.UpdateHandoverAsync(id, request, UserId(), ct), cancellationToken);
        }

        [HttpPost("{id:guid}/reject-handover")]
        [AccessAction("Update", "Reject Emergency Handover", Description = "Menolak dokumen serah terima", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> RejectHandover(Guid id, [FromBody] UpdateEmergencyHandoverStatusRequest request, CancellationToken cancellationToken = default)
        {
            request.HandoverStatus = EmergencyHandoverStatus.Rejected;
            return ExecuteDeparture(id, "EmergencyDeparture.RejectHandover",
                ct => _service.UpdateHandoverAsync(id, request, UserId(), ct), cancellationToken);
        }

        [HttpPost("{id:guid}/events/{eventId:guid}/amend")]
        [AccessAction("Update", "Amend Emergency Departure Event", Description = "Mengoreksi kejadian kepergian", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public async Task<IActionResult> Amend(Guid id, Guid eventId, [FromBody] AmendDepartureEventRequest request, CancellationToken cancellationToken = default)
        {
            request.EventId = eventId;
            var result = await _service.AmendEventAsync(id, eventId, request, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync("EmergencyDeparture.AmendEvent", result.Data!.Id);
            return Ok(ApiResponse<EmergencyDepartureEventResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Kejadian kepergian berhasil dikoreksi."));
        }

        [HttpPost("{id:guid}/events/{eventId:guid}/reverse")]
        [AccessAction("Approve", "Reverse Emergency Departure Event", Description = "Membalik kejadian kepergian", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("EmergencyDeparture", "Approve")]
        public async Task<IActionResult> Reverse(Guid id, Guid eventId, [FromBody] ReverseDepartureEventRequest request, CancellationToken cancellationToken = default)
        {
            request.EventId = eventId;
            var result = await _service.ReverseEventAsync(id, eventId, request, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync("EmergencyDeparture.ReverseEvent", result.Data!.Id);
            return Ok(ApiResponse<EmergencyDepartureEventResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Kejadian kepergian berhasil dibalik."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [AccessAction("Update", "Cancel Emergency Departure", Description = "Membatalkan kepergian pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyDeparture", "Update")]
        public Task<IActionResult> Cancel(Guid id, [FromBody] CancelEmergencyDepartureRequest request, CancellationToken cancellationToken = default)
            => ExecuteDeparture(id, "EmergencyDeparture.Cancel",
                ct => _service.CancelAsync(id, request, UserId(), ct), cancellationToken);

        private async Task<IActionResult> SetOrderAcceptance(Guid id, Guid itemId,
            EmergencyOrderAcceptanceStatus target, string? reason, CancellationToken cancellationToken)
        {
            var result = await _service.SetOrderAcceptanceAsync(id, itemId, target, reason, UserId(), cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync($"EmergencyDeparture.Order{target}", result.Data!.Id);
            return Ok(ApiResponse<EmergencyHandoverOrderItemResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data), "Penerimaan pesanan berhasil diperbarui."));
        }

        private async Task<IActionResult> ExecuteDeparture(Guid id, string action,
            Func<CancellationToken, Task<EmergencyDepartureService.Hasil<TrxEmergencyDeparture>>> operation,
            CancellationToken cancellationToken)
        {
            var result = await operation(cancellationToken);
            if (!result.Berhasil) return Failure(result.StatusCode, result.Penolakan!);
            await LogAsync(action, id);
            return Ok(ApiResponse<EmergencyDepartureResponse>.Ok(EmergencyDepartureService.ToResponse(result.Data!), "Proses kepergian pasien IGD berhasil diperbarui."));
        }

        private IActionResult Failure(int statusCode, string message)
            => StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message));

        private Task LogAsync(string action, Guid entityId)
            => _logger.InfoAsync(LogCategory, action, "Memperbarui proses Emergency Departure.", new { EntityId = entityId, Action = action });

        private Guid UserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
