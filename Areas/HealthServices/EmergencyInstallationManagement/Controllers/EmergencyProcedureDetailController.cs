using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
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
    [Route("api/v1/health-services/emergency-installation-management/emergency-procedure-details")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Procedure Detail",
        AreaName = "HealthServices",
        ControllerName = "EmergencyProcedureDetail",
        Description = "Mengelola atribut khusus IGD untuk tindakan klinis pasien",
        SortOrder = 7
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Procedure Detail")]
    public class EmergencyProcedureDetailController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmergencyProcedureDetailController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyProcedureDetailResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Procedure Detail", Description = "Melihat data detail tindakan khusus IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyProcedureDetail", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? emergencyVisitId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] Guid? emergencyResuscitationId,
            [FromQuery] Guid? emergencyObservationId,
            [FromQuery] EmergencyProcedureDetailType? detailType,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default
        )
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            IQueryable<EmgProcedureDetail> query = _dbContext.Set<EmgProcedureDetail>()
                .AsNoTracking()
                .Include(x => x.PatientProcedure!)
                    .ThenInclude(p => p.Doctor)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.SkinTestResult != null && x.SkinTestResult.ToLower().Contains(keyword)) ||
                    (x.TetanusToxoidResult != null && x.TetanusToxoidResult.ToLower().Contains(keyword)) ||
                    (x.AntiTetanusSerumUnit != null && x.AntiTetanusSerumUnit.ToLower().Contains(keyword)) ||
                    (x.MedicationRoute != null && x.MedicationRoute.ToLower().Contains(keyword)) ||
                    (x.EmergencySpecificResult != null && x.EmergencySpecificResult.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            if (emergencyVisitId.HasValue && emergencyVisitId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyVisitId == emergencyVisitId.Value);

            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty)
                query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);

            if (emergencyResuscitationId.HasValue && emergencyResuscitationId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyResuscitationId == emergencyResuscitationId.Value);

            if (emergencyObservationId.HasValue && emergencyObservationId.Value != Guid.Empty)
                query = query.Where(x => x.EmergencyObservationId == emergencyObservationId.Value);

            if (detailType.HasValue)
                query = query.Where(x => x.DetailType == detailType.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
            };

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<EmergencyProcedureDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<EmergencyProcedureDetailResponse>>.Ok(result, "Data detail tindakan khusus IGD berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyProcedureDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Procedure Detail", Description = "Melihat detail detail tindakan khusus IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyProcedureDetail", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgProcedureDetail>()
                .AsNoTracking()
                .Include(x => x.PatientProcedure!)
                    .ThenInclude(p => p.Doctor)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail tindakan khusus IGD tidak ditemukan."));

            return Ok(ApiResponse<EmergencyProcedureDetailResponse>.Ok(ToResponse(entity), "Detail detail tindakan khusus IGD berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyProcedureDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Emergency Procedure Detail", Description = "Membuat detail tindakan khusus IGD", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmergencyProcedureDetail", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyProcedureDetailRequest request, CancellationToken cancellationToken = default)
        {
            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new EmgProcedureDetail
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                PatientProcedureId = request.PatientProcedureId,
                EmergencyResuscitationId = request.EmergencyResuscitationId,
                EmergencyObservationId = request.EmergencyObservationId,
                DetailType = request.DetailType,
                SkinTestResult = NormalizeText(request.SkinTestResult),
                TetanusToxoidResult = NormalizeText(request.TetanusToxoidResult),
                AntiTetanusSerumAmount = request.AntiTetanusSerumAmount,
                AntiTetanusSerumUnit = NormalizeText(request.AntiTetanusSerumUnit),
                MedicationRoute = NormalizeText(request.MedicationRoute),
                MedicationDateTime = request.MedicationDateTime,
                EmergencySpecificResult = NormalizeText(request.EmergencySpecificResult),
                Notes = NormalizeText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<EmgProcedureDetail>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail tindakan khusus IGD gagal disimpan karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyProcedureDetail.Create",
                "Membuat data Emergency Procedure Detail.",
                new { EntityId = entity.Id, Controller = "EmergencyProcedureDetail", Action = "Create" }
            );


            await _dbContext.Entry(entity).Reference(x => x.PatientProcedure).LoadAsync(cancellationToken);

            if (entity.PatientProcedure != null)
            {
                await _dbContext.Entry(entity.PatientProcedure).Reference(p => p.Doctor).LoadAsync(cancellationToken);
            }

            return Ok(ApiResponse<EmergencyProcedureDetailResponse>.Ok(ToResponse(entity), "Data detail tindakan khusus IGD berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyProcedureDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Emergency Procedure Detail", Description = "Mengubah detail tindakan khusus IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyProcedureDetail", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmergencyProcedureDetailRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgProcedureDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail tindakan khusus IGD tidak ditemukan."));

            var validationMessage = await ValidateRequestAsync(request, cancellationToken);
            if (validationMessage != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validationMessage));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.EmergencyVisitId = request.EmergencyVisitId;
            entity.PatientProcedureId = request.PatientProcedureId;
            entity.EmergencyResuscitationId = request.EmergencyResuscitationId;
            entity.EmergencyObservationId = request.EmergencyObservationId;
            entity.DetailType = request.DetailType;
            entity.SkinTestResult = NormalizeText(request.SkinTestResult);
            entity.TetanusToxoidResult = NormalizeText(request.TetanusToxoidResult);
            entity.AntiTetanusSerumAmount = request.AntiTetanusSerumAmount;
            entity.AntiTetanusSerumUnit = NormalizeText(request.AntiTetanusSerumUnit);
            entity.MedicationRoute = NormalizeText(request.MedicationRoute);
            entity.MedicationDateTime = request.MedicationDateTime;
            entity.EmergencySpecificResult = NormalizeText(request.EmergencySpecificResult);
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
                return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, "Data detail tindakan khusus IGD gagal diubah karena melanggar relasi atau data unik."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyProcedureDetail.Update",
                "Mengubah data Emergency Procedure Detail.",
                new { EntityId = id, Controller = "EmergencyProcedureDetail", Action = "Update" }
            );


            await _dbContext.Entry(entity).Reference(x => x.PatientProcedure).LoadAsync(cancellationToken);

            if (entity.PatientProcedure != null)
            {
                await _dbContext.Entry(entity.PatientProcedure).Reference(p => p.Doctor).LoadAsync(cancellationToken);
            }

            return Ok(ApiResponse<EmergencyProcedureDetailResponse>.Ok(ToResponse(entity), "Data detail tindakan khusus IGD berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Emergency Procedure Detail", Description = "Menghapus detail tindakan khusus IGD", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmergencyProcedureDetail", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgProcedureDetail>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data detail tindakan khusus IGD tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyProcedureDetail.Delete",
                "Menghapus data Emergency Procedure Detail.",
                new { EntityId = id, Controller = "EmergencyProcedureDetail", Action = "Delete" }
            );

            return Ok(ApiResponse<object>.Ok(null, "Data detail tindakan khusus IGD berhasil dihapus."));
        }

        private async Task<string?> ValidateRequestAsync(CreateEmergencyProcedureDetailRequest request, CancellationToken cancellationToken)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.PatientProcedureId == Guid.Empty)
                return "PatientProcedureId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyProcedureDetailType), request.DetailType))
                return "Nilai DetailType tidak valid.";

            if (!await _dbContext.Set<EmgVisit>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyVisitId && !x.IsDelete, cancellationToken))
                return "EmergencyVisitId tidak ditemukan.";

            if (!await _dbContext.Set<TrxPatientProcedure>().AsNoTracking().AnyAsync(x => x.Id == request.PatientProcedureId && !x.IsDelete, cancellationToken))
                return "PatientProcedureId tidak ditemukan.";

            if (request.EmergencyResuscitationId.HasValue && request.EmergencyResuscitationId.Value != Guid.Empty &&
                !await _dbContext.Set<EmgResuscitation>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyResuscitationId.Value && !x.IsDelete, cancellationToken))
                return "EmergencyResuscitationId tidak ditemukan.";

            if (request.EmergencyObservationId.HasValue && request.EmergencyObservationId.Value != Guid.Empty &&
                !await _dbContext.Set<EmgObservation>().AsNoTracking().AnyAsync(x => x.Id == request.EmergencyObservationId.Value && !x.IsDelete, cancellationToken))
                return "EmergencyObservationId tidak ditemukan.";

            return null;
        }

        private Task<string?> ValidateRequestAsync(UpdateEmergencyProcedureDetailRequest request, CancellationToken cancellationToken)
            => ValidateRequestAsync((CreateEmergencyProcedureDetailRequest)request, cancellationToken);

        private static EmergencyProcedureDetailResponse ToResponse(EmgProcedureDetail x)
        {
            return new EmergencyProcedureDetailResponse
            {
                Id = x.Id,
                EmergencyVisitId = x.EmergencyVisitId,
                PatientProcedureId = x.PatientProcedureId,
                ProcedureName = x.PatientProcedure != null
                    ? x.PatientProcedure.ProcedureNameSnapshot
                    : null,
                // Waktu selesai lebih dipercaya daripada waktu rencana. Urutannya turun
                // ke waktu mulai, lalu ke waktu tindakan, supaya baris yang belum tuntas
                // tetap punya waktu yang berarti alih-alih tampil kosong.
                PerformedAt = x.PatientProcedure != null
                    ? (x.PatientProcedure.CompletedAt
                        ?? x.PatientProcedure.StartedAt
                        ?? x.PatientProcedure.ProcedureDateTime)
                    : null,
                Quantity = x.PatientProcedure != null ? x.PatientProcedure.Quantity : null,
                PerformedByName = x.PatientProcedure != null && x.PatientProcedure.Doctor != null
                    ? x.PatientProcedure.Doctor.FullName
                    : null,
                EmergencyResuscitationId = x.EmergencyResuscitationId,
                EmergencyObservationId = x.EmergencyObservationId,
                DetailType = x.DetailType,
                SkinTestResult = x.SkinTestResult,
                TetanusToxoidResult = x.TetanusToxoidResult,
                AntiTetanusSerumAmount = x.AntiTetanusSerumAmount,
                AntiTetanusSerumUnit = x.AntiTetanusSerumUnit,
                MedicationRoute = x.MedicationRoute,
                MedicationDateTime = x.MedicationDateTime,
                EmergencySpecificResult = x.EmergencySpecificResult,
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
