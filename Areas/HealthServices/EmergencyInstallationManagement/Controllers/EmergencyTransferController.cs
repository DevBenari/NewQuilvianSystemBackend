using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/emergency-transfers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Transfer",
        AreaName = "HealthServices",
        ControllerName = "EmergencyTransfer",
        Description = "Mengelola proses perpindahan dan serah terima pasien dari IGD",
        SortOrder = 9
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Transfer")]
    public class EmergencyTransferController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyTransferService _emergencyTransferService;

        public EmergencyTransferController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyTransferService emergencyService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _emergencyTransferService = emergencyService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyTransferResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Transfer", Description = "Melihat data transfer pasien IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTransfer", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] Guid? fromServiceUnitId,
            [FromQuery] Guid? toServiceUnitId,
            [FromQuery] EmergencyTransferStatus? transferStatus,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<TrxEmergencyTransfer> query = _dbContext.Set<TrxEmergencyTransfer>().AsNoTracking().Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.TransferNumber.ToLower().Contains(keyword) ||
                    (x.TransferReason != null && x.TransferReason.ToLower().Contains(keyword)) ||
                    (x.HandoverSummary != null && x.HandoverSummary.ToLower().Contains(keyword)) ||
                    (x.RejectionReason != null && x.RejectionReason.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (fromServiceUnitId.HasValue && fromServiceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.FromServiceUnitId == fromServiceUnitId.Value);

            if (toServiceUnitId.HasValue && toServiceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ToServiceUnitId == toServiceUnitId.Value);

            if (transferStatus.HasValue)
                query = query.Where(x => x.TransferStatus == transferStatus.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.RequestedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.RequestedAt < endDate.Value.Date.AddDays(1));

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "requestedat" => descending ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.RequestedAt) : query.OrderBy(x => x.RequestedAt)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyTransferResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyTransferResponse>>.Ok(result, "Data transfer pasien IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTransferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Transfer", Description = "Melihat detail transfer pasien IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyTransfer", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTransfer>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data transfer pasien IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyTransferResponse>.Ok(ToResponse(entity), "Detail transfer pasien IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTransferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Transfer", Description = "Membuat transfer pasien IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyTransfer", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await _emergencyTransferService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.TransferNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyTransfer>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TransferNumber == normalizedNumber, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "TransferNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new TrxEmergencyTransfer
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                TransferNumber = string.IsNullOrWhiteSpace(request.TransferNumber) ? _emergencyTransferService.GenerateNumber(now) : request.TransferNumber.Trim(),
                FromServiceUnitId = request.FromServiceUnitId,
                ToServiceUnitId = request.ToServiceUnitId,
                FromRoomId = request.FromRoomId,
                ToRoomId = request.ToRoomId,
                FromBedId = request.FromBedId,
                ToBedId = request.ToBedId,
                TransferStatus = request.TransferStatus,
                RequestedAt = request.RequestedAt == default ? now : request.RequestedAt,
                RequestedByUserId = request.RequestedByUserId == Guid.Empty ? actorUserId : request.RequestedByUserId,
                AcceptedAt = request.AcceptedAt,
                AcceptedByUserId = request.AcceptedByUserId ?? actorUserId,
                DepartedAt = request.DepartedAt,
                ArrivedAt = request.ArrivedAt,
                SendingNurseUserId = request.SendingNurseUserId,
                ReceivingNurseUserId = request.ReceivingNurseUserId,
                TransferReason = NormalizeText(request.TransferReason),
                HandoverSummary = NormalizeText(request.HandoverSummary),
                RejectionReason = NormalizeText(request.RejectionReason),
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxEmergencyTransfer>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data transfer pasien IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTransfer.Create",
                "Membuat data Emergency Transfer.",
                new { EntityId = entity.Id, Controller = "EmergencyTransfer", Action = "Create" }
            );

            return Ok(ApiResponse<EmergencyTransferResponse>.Ok(ToResponse(entity), "Data transfer pasien IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTransferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Transfer", Description = "Mengubah transfer pasien IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyTransfer", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTransfer>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data transfer pasien IGD tidak ditemukan."));

            if (entity.TransferStatus != EmergencyTransferStatus.Requested)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Transfer yang sudah diproses tidak dapat ditimpa. Gunakan aksi status atau jalur koreksi yang tercatat."));
            }

            var validationMessage = await _emergencyTransferService.ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var normalizedNumber = NormalizeText(request.TransferNumber);
            if (!string.IsNullOrWhiteSpace(normalizedNumber) && await _dbContext.Set<TrxEmergencyTransfer>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.TransferNumber == normalizedNumber && x.Id != id, cancellationToken))
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "TransferNumber sudah digunakan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.TransferNumber = string.IsNullOrWhiteSpace(request.TransferNumber) ? entity.TransferNumber : request.TransferNumber.Trim();
            entity.FromServiceUnitId = request.FromServiceUnitId;
            entity.ToServiceUnitId = request.ToServiceUnitId;
            entity.FromRoomId = request.FromRoomId;
            entity.ToRoomId = request.ToRoomId;
            entity.FromBedId = request.FromBedId;
            entity.ToBedId = request.ToBedId;
            entity.RequestedAt = request.RequestedAt;
            entity.RequestedByUserId = request.RequestedByUserId;
            entity.SendingNurseUserId = request.SendingNurseUserId;
            entity.ReceivingNurseUserId = request.ReceivingNurseUserId;
            entity.TransferReason = NormalizeText(request.TransferReason);
            entity.HandoverSummary = NormalizeText(request.HandoverSummary);
            entity.Notes = NormalizeText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data transfer pasien IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTransfer.Update",
                "Mengubah data Emergency Transfer.",
                new { EntityId = id, Controller = "EmergencyTransfer", Action = "Update" }
            );

            return Ok(ApiResponse<EmergencyTransferResponse>.Ok(ToResponse(entity), "Data transfer pasien IGD berhasil diubah."));
        }

        [HttpPatch("{id:guid}/transfer-status")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyTransferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Emergency Transfer TransferStatus", Description = "Mengubah status transfer pasien IGD", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmergencyTransfer", "Update")]
        public async Task<IActionResult> UpdateTransferStatus(Guid id, [FromBody] UpdateEmergencyTransferTransferStatusRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTransfer>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data transfer pasien IGD tidak ditemukan."));

            if (!_emergencyTransferService.CanTransition(entity.TransferStatus, request.TransferStatus))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, $"Perubahan status dari {entity.TransferStatus} ke {request.TransferStatus} tidak diperbolehkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            // AT-IGD-041: perpindahan harus diterima petugas unit tujuan, bukan pengajunya
            // sendiri. Pemeriksaan penuh terhadap unit tujuan belum dapat ditegakkan karena
            // belum ada relasi pengguna ke unit pelayanan; yang dapat dipastikan sekarang
            // adalah pengaju tidak boleh menjadi penerima.
            if (request.TransferStatus == EmergencyTransferStatus.Accepted &&
                entity.RequestedByUserId == actorUserId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Perpindahan harus diterima oleh petugas unit tujuan."));
            }

            entity.TransferStatus = request.TransferStatus;
            if (request.TransferStatus == EmergencyTransferStatus.Accepted)
            {
                entity.AcceptedAt ??= now;
                entity.AcceptedByUserId = actorUserId;
            }
            if (request.TransferStatus == EmergencyTransferStatus.InTransit)
                entity.DepartedAt ??= now;
            if (request.TransferStatus == EmergencyTransferStatus.Completed)
                entity.ArrivedAt ??= now;
            if (request.TransferStatus == EmergencyTransferStatus.Rejected)
                entity.RejectionReason = NormalizeText(request.Notes) ?? entity.RejectionReason;
            if (!string.IsNullOrWhiteSpace(request.Notes) && entity.GetType().GetProperty("Notes") != null)
            {
                entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTransfer.UpdateTransferStatus",
                "Memperbarui proses Emergency Transfer melalui aksi UpdateTransferStatus.",
                new { EntityId = id, Controller = "EmergencyTransfer", Action = "UpdateTransferStatus" }
            );

            return Ok(ApiResponse<EmergencyTransferResponse>.Ok(ToResponse(entity), "Status transfer pasien IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Transfer", Description = "Menghapus transfer pasien IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyTransfer", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxEmergencyTransfer>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data transfer pasien IGD tidak ditemukan."));

            if (entity.TransferStatus != EmergencyTransferStatus.Requested)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Transfer yang sudah diproses tidak dapat dihapus karena merupakan riwayat klinis."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyTransfer.Delete",
                "Menghapus data Emergency Transfer.",
                new { EntityId = id, Controller = "EmergencyTransfer", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data transfer pasien IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyTransferRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.ToServiceUnitId == Guid.Empty)
                return "ToServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTransferStatus), request.TransferStatus))
                return "Nilai TransferStatus tidak valid.";

            if (request.FromServiceUnitId.HasValue && request.FromServiceUnitId.Value == request.ToServiceUnitId)
                return "Unit tujuan transfer harus berbeda dengan unit asal.";

            if (!await _dbContext.Set<TrxEmergencyVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            if (request.FromServiceUnitId.HasValue && request.FromServiceUnitId.Value != Guid.Empty &&
                !await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.FromServiceUnitId.Value && !x.IsDelete, cancellationToken))
                return "FromServiceUnitId tidak ditemukan.";

            if (!await _dbContext.Set<MstServiceUnit>().AsNoTracking().AnyAsync(x => x.Id == request.ToServiceUnitId && !x.IsDelete, cancellationToken))
                return "ToServiceUnitId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyTransferRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyTransferRequest)request, cancellationToken);

        private static bool CanTransition(EmergencyTransferStatus current, EmergencyTransferStatus target)
        {
            if (current == target) return true;

            return current switch
            {
                EmergencyTransferStatus.Requested => target is EmergencyTransferStatus.Accepted or EmergencyTransferStatus.Rejected or EmergencyTransferStatus.Cancelled,
                EmergencyTransferStatus.Accepted => target is EmergencyTransferStatus.InTransit or EmergencyTransferStatus.Rejected or EmergencyTransferStatus.Cancelled,
                EmergencyTransferStatus.InTransit => target is EmergencyTransferStatus.Completed or EmergencyTransferStatus.Cancelled,
                EmergencyTransferStatus.Completed => false,
                EmergencyTransferStatus.Rejected => false,
                EmergencyTransferStatus.Cancelled => false,
                _ => false
            };
        }

        private static EmergencyTransferResponse ToResponse(TrxEmergencyTransfer x)
        {
            return new EmergencyTransferResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                TransferNumber = x.TransferNumber,
                FromServiceUnitId = x.FromServiceUnitId,
                ToServiceUnitId = x.ToServiceUnitId,
                FromRoomId = x.FromRoomId,
                ToRoomId = x.ToRoomId,
                FromBedId = x.FromBedId,
                ToBedId = x.ToBedId,
                TransferStatus = x.TransferStatus,
                RequestedAt = x.RequestedAt,
                RequestedByUserId = x.RequestedByUserId,
                AcceptedAt = x.AcceptedAt,
                AcceptedByUserId = x.AcceptedByUserId,
                DepartedAt = x.DepartedAt,
                ArrivedAt = x.ArrivedAt,
                SendingNurseUserId = x.SendingNurseUserId,
                ReceivingNurseUserId = x.ReceivingNurseUserId,
                TransferReason = x.TransferReason,
                HandoverSummary = x.HandoverSummary,
                RejectionReason = x.RejectionReason,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string GenerateDocumentNumber(string prefix, DateTime now)
            => $"{prefix}-{now:yyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
