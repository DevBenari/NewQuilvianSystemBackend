using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDoctorQueuePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.DoctorQueueResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/doctor-queues")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Doctor Queue",
        AreaName = "HealthServices",
        ControllerName = "DoctorQueue",
        Description = "Operasional antrean pemeriksaan dokter",
        SortOrder = 5
    )]
    [Tags("Health Services / Registration Management / Doctor Queue")]
    public class DoctorQueueController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DoctorQueueController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DoctorQueueFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Queue", Description = "Melihat metadata filter antrean dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorQueue", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new DoctorQueueFilterMetadataResponse
            {
                SortOptions = new List<DoctorQueueSortOptionResponse>
                {
                    new() { Value = "queueNumber", Label = "Nomor antrean" },
                    new() { Value = "queueDate", Label = "Tanggal antrean" },
                    new() { Value = "queueStatus", Label = "Status antrean" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "clinicName", Label = "Poli" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueueStatusOptions = BuildQueueStatusOptions(),
                ResetButtonLabel = "Reset"
            };

            return Ok(ApiResponse<DoctorQueueFilterMetadataResponse>.Ok(result, "Metadata filter antrean dokter berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DoctorQueueSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Queue", Description = "Melihat ringkasan antrean dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorQueue", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? queueDate, [FromQuery] Guid? doctorId)
        {
            var isSuperAdmin = await IsCurrentUserSuperAdminAsync();
            var allowedDoctorId = isSuperAdmin && (!doctorId.HasValue || doctorId.Value == Guid.Empty)
                ? null
                : await ResolveAllowedDoctorIdAsync(doctorId);

            if (!isSuperAdmin && !allowedDoctorId.HasValue) return Forbid();

            var query = BuildQueueBaseQuery(queueDate, allowedDoctorId);

            var result = new DoctorQueueSummaryResponse
            {
                TotalQueue = await query.CountAsync(),
                WaitingForDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForDoctor),
                CalledByDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.CalledByDoctor),
                InConsultationQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.InConsultation),
                CompletedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Completed || x.CompletedAt.HasValue),
                SkippedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Skipped),
                NoShowQueue = await query.CountAsync(x => x.NoShowAt.HasValue),
                PriorityQueue = await query.CountAsync(x => x.IsPriorityQueue)
            };

            return Ok(ApiResponse<DoctorQueueSummaryResponse>.Ok(result, "Ringkasan antrean dokter berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDoctorQueuePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Doctor Queue", Description = "Melihat antrean pasien dokter login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DoctorQueue", "Read")]
        public async Task<IActionResult> GetQueues(
            [FromQuery] DateTime? queueDate,
            [FromQuery] Guid? doctorId,
            [FromQuery] QueueStatus? queueStatus,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "queueNumber",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var isSuperAdmin = await IsCurrentUserSuperAdminAsync();
            var allowedDoctorId = isSuperAdmin && (!doctorId.HasValue || doctorId.Value == Guid.Empty)
                ? null
                : await ResolveAllowedDoctorIdAsync(doctorId);

            if (!isSuperAdmin && !allowedDoctorId.HasValue) return Forbid();

            var query = BuildQueueBaseQuery(queueDate, allowedDoctorId);
            query = ApplyStandardFilter(query, queueStatus, search);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapResponse(x))
                .ToListAsync();

            var result = new ResponseDoctorQueuePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDoctorQueuePagedResult>.Ok(result, "Data antrean dokter berhasil diambil."));
        }

        [HttpPost("{id:guid}/call")]
        [AccessAction("Update", "Call Doctor Queue", Description = "Memanggil pasien ke dokter", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("DoctorQueue", "Update")]
        public async Task<IActionResult> Call(Guid id)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;
            if (queue.QueueStatus != QueueStatus.WaitingForDoctor && queue.QueueStatus != QueueStatus.CalledByDoctor)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Antrean tidak dalam status menunggu dokter."));

            if (queue.QueueStatus == QueueStatus.CalledByDoctor && queue.DoctorCallExpiresAt.HasValue && queue.DoctorCallExpiresAt.Value > now)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Timer panggilan dokter masih berjalan."));

            if (queue.DoctorCallAttemptCount >= 2)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pasien sudah dipanggil 2 kali. Silakan lakukan skip."));

            var actorUserId = GetCurrentUserId();
            queue.QueueStatus = QueueStatus.CalledByDoctor;
            queue.DoctorCallAttemptCount += 1;
            queue.LastDoctorCalledAt = now;
            queue.LastDoctorCalledByUserId = actorUserId;
            queue.DoctorCallExpiresAt = now.AddMinutes(2);
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Pasien berhasil dipanggil oleh dokter."), "Pasien berhasil dipanggil oleh dokter."));
        }

        [HttpPost("{id:guid}/start-consultation")]
        [AccessAction("Update", "Start Doctor Consultation", Description = "Memulai konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DoctorQueue", "Update")]
        public async Task<IActionResult> StartConsultation(Guid id)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.CalledByDoctor && queue.QueueStatus != QueueStatus.WaitingForDoctor)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Antrean belum siap untuk konsultasi dokter."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.InConsultation;
            queue.ConsultationStartedAt ??= now;
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = EncounterStatus.InConsultation;
                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Konsultasi dokter dimulai."), "Konsultasi dokter dimulai."));
        }

        [HttpPost("{id:guid}/finish-consultation")]
        [AccessAction("Update", "Finish Doctor Consultation", Description = "Menyelesaikan konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("DoctorQueue", "Update")]
        public async Task<IActionResult> FinishConsultation(Guid id, [FromBody] DoctorQueueActionRequest? request = null)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.InConsultation)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Konsultasi dokter belum dimulai."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.Completed;
            queue.ConsultationCompletedAt = now;
            queue.CompletedAt = now;
            queue.CompletedByUserId = actorUserId;
            queue.Notes = MergeNotes(queue.Notes, request?.Notes);
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = EncounterStatus.Completed;
                queue.Encounter.CompletedAt = now;
                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Konsultasi dokter selesai."), "Konsultasi dokter selesai."));
        }

        [HttpPost("{id:guid}/skip")]
        [AccessAction("Update", "Skip Doctor Queue", Description = "Melewati pasien yang tidak hadir saat dipanggil dokter", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("DoctorQueue", "Update")]
        public async Task<IActionResult> Skip(Guid id, [FromBody] DoctorQueueActionRequest request)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;
            if (queue.QueueStatus != QueueStatus.CalledByDoctor || queue.DoctorCallAttemptCount < 2)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pasien hanya bisa di-skip setelah 2 kali panggilan dokter."));

            if (queue.DoctorCallExpiresAt.HasValue && queue.DoctorCallExpiresAt.Value > now)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Timer panggilan dokter masih berjalan."));

            var actorUserId = GetCurrentUserId();
            queue.QueueStatus = QueueStatus.Skipped;
            queue.SkipCount += 1;
            queue.LastSkippedAt = now;
            queue.LastSkippedByUserId = actorUserId;
            queue.SkipReason = NormalizeNullableText(request.Reason) ?? "Tidak hadir saat dipanggil dokter.";
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Pasien berhasil dilewati."), "Pasien berhasil dilewati."));
        }

        [HttpPost("{id:guid}/requeue")]
        [AccessAction("Update", "Requeue Doctor Queue", Description = "Mengembalikan pasien ke antrean dokter", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("DoctorQueue", "Update")]
        public async Task<IActionResult> Requeue(Guid id, [FromBody] DoctorQueueActionRequest request)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();
            if (queue.QueueStatus != QueueStatus.Skipped)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Hanya antrean skipped yang bisa dikembalikan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.WaitingForDoctor;
            queue.RequeueCount += 1;
            queue.LastRequeuedAt = now;
            queue.LastRequeuedByUserId = actorUserId;
            queue.RequeueReason = NormalizeNullableText(request.Reason);
            queue.DoctorCallAttemptCount = 0;
            queue.DoctorCallExpiresAt = null;
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Pasien berhasil dikembalikan ke antrean dokter."), "Pasien berhasil dikembalikan ke antrean dokter."));
        }

        private IQueryable<TrxQueue> BuildQueueBaseQuery(DateTime? queueDate, Guid? allowedDoctorId)
        {
            var selectedDate = AppDateTimeHelper.ResolveOperationalDate(queueDate);

            var query = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.QueueDate.Date == selectedDate);

            if (allowedDoctorId.HasValue && allowedDoctorId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DoctorId == allowedDoctorId.Value);
            }

            return query;
        }

        private static IQueryable<TrxQueue> ApplyStandardFilter(IQueryable<TrxQueue> query, QueueStatus? queueStatus, string? search)
        {
            if (queueStatus.HasValue) query = query.Where(x => x.QueueStatus == queueStatus.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.QueueCode.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<TrxQueue> ApplySorting(IQueryable<TrxQueue> query, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "queueNumber").Trim().ToLowerInvariant() switch
            {
                "queuedate" => isDescending ? query.OrderByDescending(x => x.QueueDate) : query.OrderBy(x => x.QueueDate),
                "queuestatus" => isDescending ? query.OrderByDescending(x => x.QueueStatus).ThenBy(x => x.QueueNumber) : query.OrderBy(x => x.QueueStatus).ThenBy(x => x.QueueNumber),
                "patientname" => isDescending ? query.OrderByDescending(x => x.Patient!.FullName) : query.OrderBy(x => x.Patient!.FullName),
                "clinicname" => isDescending ? query.OrderByDescending(x => x.Clinic!.ClinicName).ThenBy(x => x.QueueNumber) : query.OrderBy(x => x.Clinic!.ClinicName).ThenBy(x => x.QueueNumber),
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDescending ? query.OrderByDescending(x => x.IsPriorityQueue).ThenByDescending(x => x.QueueNumber) : query.OrderByDescending(x => x.IsPriorityQueue).ThenBy(x => x.QueueNumber)
            };
        }

        private async Task<bool> IsCurrentUserSuperAdminAsync()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                return true;
            }

            var roleClaims = User.FindAll(ClaimTypes.Role)
                .Concat(User.FindAll("role"))
                .Concat(User.FindAll("roles"))
                .Select(x => x.Value);

            if (roleClaims.Any(x => x.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var userTypeClaim =
                User.FindFirstValue("user_type") ??
                User.FindFirstValue("UserType") ??
                User.FindFirstValue("userType");

            if (IsSuperAdminValue(userTypeClaim))
            {
                return true;
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return false;
            }

            var currentUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive);

            if (currentUser == null)
            {
                return false;
            }

            var userTypeProperty = currentUser.GetType().GetProperty("UserType");
            var userTypeValue = userTypeProperty?.GetValue(currentUser);

            return IsSuperAdminValue(userTypeValue);
        }

        private static bool IsSuperAdminValue(object? value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is int intValue)
            {
                return intValue == 1;
            }

            if (value is long longValue)
            {
                return longValue == 1;
            }

            var valueType = value.GetType();
            if (valueType.IsEnum && Enum.TryParse(valueType, "SuperAdmin", true, out var superAdminValue))
            {
                return Equals(value, superAdminValue);
            }

            var text = value.ToString();
            return text == "1" || text?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task<Guid?> ResolveAllowedDoctorIdAsync(Guid? requestedDoctorId)
        {
            if (await IsCurrentUserSuperAdminAsync())
            {
                return requestedDoctorId.HasValue && requestedDoctorId.Value != Guid.Empty
                    ? requestedDoctorId.Value
                    : null;
            }

            var doctorIdClaim = User.FindFirstValue("doctor_id") ?? User.FindFirstValue("DoctorId");
            if (Guid.TryParse(doctorIdClaim, out var doctorId))
            {
                if (!requestedDoctorId.HasValue || requestedDoctorId.Value == Guid.Empty || requestedDoctorId.Value == doctorId)
                    return doctorId;
            }

            var workforceClaim = User.FindFirstValue("workforce_profile_id") ?? User.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                var doctorByWorkforce = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == workforceProfileId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (doctorByWorkforce != Guid.Empty && (!requestedDoctorId.HasValue || requestedDoctorId.Value == Guid.Empty || requestedDoctorId.Value == doctorByWorkforce))
                    return doctorByWorkforce;
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser?.Email != null)
            {
                var email = currentUser.Email.ToLower();
                var doctorByEmail = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (doctorByEmail != Guid.Empty && (!requestedDoctorId.HasValue || requestedDoctorId.Value == Guid.Empty || requestedDoctorId.Value == doctorByEmail))
                    return doctorByEmail;
            }

            return null;
        }

        private async Task<TrxQueue?> GetAllowedQueueWithEncounterAsync(Guid id)
        {
            var query = _dbContext.Set<TrxQueue>()
                .Include(x => x.Encounter)
                .Where(x => x.Id == id && !x.IsDelete && x.IsActive);

            if (await IsCurrentUserSuperAdminAsync())
            {
                return await query.FirstOrDefaultAsync();
            }

            var allowedDoctorId = await ResolveAllowedDoctorIdAsync(null);
            if (!allowedDoctorId.HasValue)
            {
                return null;
            }

            return await query.FirstOrDefaultAsync(x => x.DoctorId == allowedDoctorId.Value);
        }

        private static DoctorQueueResponse MapResponse(TrxQueue x)
        {
            return new DoctorQueueResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                QueueDate = x.QueueDate,
                QueueNumber = x.QueueNumber,
                QueueCode = x.QueueCode,
                QueueStatus = x.QueueStatus,
                QueueStatusName = x.QueueStatus.ToString(),
                DoctorCallAttemptCount = x.DoctorCallAttemptCount,
                LastDoctorCalledAt = x.LastDoctorCalledAt,
                DoctorCallExpiresAt = x.DoctorCallExpiresAt,
                ScreeningCompletedAt = x.ScreeningCompletedAt,
                ConsultationStartedAt = x.ConsultationStartedAt,
                ConsultationCompletedAt = x.ConsultationCompletedAt,
                SkipCount = x.SkipCount,
                RequeueCount = x.RequeueCount,
                IsPriorityQueue = x.IsPriorityQueue,
                IsDoctorRequired = x.IsDoctorRequired,
                Notes = x.Notes,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static DoctorQueueActionResponse BuildActionResponse(TrxQueue queue, string message)
        {
            return new DoctorQueueActionResponse
            {
                QueueId = queue.Id,
                EncounterId = queue.EncounterId,
                QueueStatus = queue.QueueStatus,
                QueueStatusName = queue.QueueStatus.ToString(),
                EncounterStatus = queue.Encounter?.EncounterStatus ?? EncounterStatus.Registered,
                EncounterStatusName = (queue.Encounter?.EncounterStatus ?? EncounterStatus.Registered).ToString(),
                DoctorCallAttemptCount = queue.DoctorCallAttemptCount,
                DoctorCallExpiresAt = queue.DoctorCallExpiresAt,
                ConsultationStartedAt = queue.ConsultationStartedAt,
                ConsultationCompletedAt = queue.ConsultationCompletedAt,
                Message = message
            };
        }

        private static List<DoctorQueueStatusOptionResponse> BuildQueueStatusOptions()
        {
            return Enum.GetValues<QueueStatus>()
                .Select(x => new DoctorQueueStatusOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = x.ToString() })
                .ToList();
        }

        private IActionResult QueueNotFound()
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Antrean tidak ditemukan atau tidak termasuk dokter login."));
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? MergeNotes(string? currentNotes, string? newNotes)
        {
            var normalized = NormalizeNullableText(newNotes);
            if (normalized == null) return currentNotes;
            return string.IsNullOrWhiteSpace(currentNotes) ? normalized : currentNotes.Trim() + Environment.NewLine + normalized;
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}
