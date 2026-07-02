using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
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
        private const string DefaultDoctorProfilePhotoPathFallback = "/uploads/default-profile-photos/dokter.png";

        private static readonly QueueStatus[] DoctorWorkflowStatuses =
        {
            QueueStatus.WaitingForDoctor,
            QueueStatus.CalledByDoctor,
            QueueStatus.InConsultation,
            QueueStatus.Skipped,
            QueueStatus.NoShow,
            QueueStatus.Cancelled,
            QueueStatus.Completed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly QueueVoiceService _queueVoiceService;
        private readonly QueueRealtimeService _queueRealtimeService;

        public DoctorQueueController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            QueueVoiceService queueVoiceService,
            QueueRealtimeService queueRealtimeService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _queueVoiceService = queueVoiceService;
            _queueRealtimeService = queueRealtimeService;
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
            var queues = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await MapResponsesAsync(queues);

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
            await _queueRealtimeService.NotifyQueueCalledByDoctorAsync(queue, actorUserId, "Pasien berhasil dipanggil oleh dokter.");

            var voiceResult = await _queueVoiceService.GetOrCreateQueueCallAudioAsync(queue, QueueVoiceCallTypes.Doctor);

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dipanggil oleh dokter.", voiceResult),
                "Pasien berhasil dipanggil oleh dokter."
            ));
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
            await _queueRealtimeService.NotifyQueueConsultationStartedAsync(queue, actorUserId, "Konsultasi dokter dimulai.");

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
            await _queueRealtimeService.NotifyQueueConsultationFinishedAsync(queue, actorUserId, "Konsultasi dokter selesai.");

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
            await _queueRealtimeService.NotifyQueueSkippedByDoctorAsync(queue, actorUserId, "Pasien berhasil dilewati.");

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
            await _queueRealtimeService.NotifyQueueRequeuedToDoctorAsync(queue, actorUserId, "Pasien berhasil dikembalikan ke antrean dokter.");

            return Ok(ApiResponse<DoctorQueueActionResponse>.Ok(BuildActionResponse(queue, "Pasien berhasil dikembalikan ke antrean dokter."), "Pasien berhasil dikembalikan ke antrean dokter."));
        }

        private IQueryable<TrxQueue> BuildQueueBaseQuery(DateTime? queueDate, Guid? allowedDoctorId)
        {
            var selectedDate = AppDateTimeHelper.ResolveOperationalDate(queueDate);

            var query = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Include(x => x.Encounter)
                    .ThenInclude(x => x.PaymentMethod)
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsDoctorRequired &&
                    x.DoctorId.HasValue &&
                    x.QueueDate.Date == selectedDate &&
                    DoctorWorkflowStatuses.Contains(x.QueueStatus));

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
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
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

        private async Task<List<DoctorQueueResponse>> MapResponsesAsync(List<TrxQueue> queues)
        {
            if (queues.Count == 0)
            {
                return new List<DoctorQueueResponse>();
            }

            var patientIds = queues
                .Select(x => x.PatientId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var visitCounts = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && patientIds.Contains(x.PatientId))
                .GroupBy(x => x.PatientId)
                .Select(x => new
                {
                    PatientId = x.Key,
                    Count = x.Count()
                })
                .ToDictionaryAsync(x => x.PatientId, x => x.Count);

            var doctorIds = queues
                .Where(x => x.DoctorId.HasValue && x.DoctorId.Value != Guid.Empty)
                .Select(x => x.DoctorId!.Value)
                .Distinct()
                .ToList();

            var doctorPhotoRows = doctorIds.Count == 0
                ? new List<DoctorUserPhotoSnapshot>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.DoctorId.HasValue && doctorIds.Contains(x.DoctorId.Value))
                    .Select(x => new DoctorUserPhotoSnapshot
                    {
                        DoctorId = x.DoctorId!.Value,
                        ProfilePhotoPath = x.ProfilePhotoPath,
                        IsActive = x.IsActive,
                        LastUpdatedAt = x.UpdateDateTime ?? x.CreateDateTime
                    })
                    .ToListAsync();

            var doctorPhotoPaths = doctorPhotoRows
                .GroupBy(x => x.DoctorId)
                .ToDictionary(
                    x => x.Key,
                    x => NormalizeNullableText(
                        x.OrderByDescending(y => y.IsActive)
                            .ThenByDescending(y => y.LastUpdatedAt)
                            .Select(y => y.ProfilePhotoPath)
                            .FirstOrDefault()
                    ) ?? DefaultDoctorProfilePhotoPathFallback
                );

            var workforceProfileIds = queues
                .Select(x => x.Doctor?.WorkforceProfileId ?? Guid.Empty)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var doctorCredentialSnapshots = await BuildDoctorCredentialSnapshotsAsync(workforceProfileIds);

            return queues
                .Select(x => MapResponse(x, visitCounts, doctorPhotoPaths, doctorCredentialSnapshots))
                .ToList();
        }

        private static DoctorQueueResponse MapResponse(
            TrxQueue x,
            IReadOnlyDictionary<Guid, int> visitCounts,
            IReadOnlyDictionary<Guid, string> doctorPhotoPaths,
            IReadOnlyDictionary<Guid, DoctorWorkforceCredentialSnapshot> doctorCredentialSnapshots)
        {
            var encounter = x.Encounter;
            var doctorPhotoPath = ResolveDoctorPhotoPath(x.DoctorId, doctorPhotoPaths);
            var paymentType = encounter?.PaymentType ?? EncounterPaymentType.Cash;
            var primaryGuarantorName = NormalizeNullableText(encounter?.PrimaryGuarantorNameSnapshot);
            var totalVisitCount = visitCounts.TryGetValue(x.PatientId, out var count) ? count : 0;
            var workforceProfileId = x.Doctor?.WorkforceProfileId ?? Guid.Empty;
            var doctorCredential = workforceProfileId != Guid.Empty && doctorCredentialSnapshots.TryGetValue(workforceProfileId, out var snapshot)
                ? snapshot
                : DoctorWorkforceCredentialSnapshot.Empty;
            var primaryCredential = doctorCredential.Sip ?? doctorCredential.Str;

            return new DoctorQueueResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = encounter != null ? encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                DoctorNumber = x.Doctor != null ? x.Doctor.DoctorNumber : null,
                DoctorWorkforceProfileId = workforceProfileId == Guid.Empty ? null : workforceProfileId,
                DoctorSpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,
                DoctorSubSpecialistName = x.Doctor != null ? x.Doctor.SubSpecialistName : null,

                DoctorStrCredentialLicenseId = doctorCredential.Str?.Id,
                DoctorStrNumber = doctorCredential.Str?.LicenseNumber,
                DoctorStrIssueDate = doctorCredential.Str?.IssueDate,
                DoctorStrExpiredDate = doctorCredential.Str?.ExpiredDate,
                DoctorStrIsVerified = doctorCredential.Str?.IsVerified ?? false,
                DoctorStrIsCurrentlyValid = doctorCredential.Str?.IsCurrentlyValid ?? false,

                DoctorSipCredentialLicenseId = doctorCredential.Sip?.Id,
                DoctorSipNumber = doctorCredential.Sip?.LicenseNumber,
                DoctorSipIssueDate = doctorCredential.Sip?.IssueDate,
                DoctorSipExpiredDate = doctorCredential.Sip?.ExpiredDate,
                DoctorSipPracticeLocation = doctorCredential.Sip?.PracticeLocation,
                DoctorSipIsVerified = doctorCredential.Sip?.IsVerified ?? false,
                DoctorSipIsCurrentlyValid = doctorCredential.Sip?.IsCurrentlyValid ?? false,

                DoctorRegistrationNumber = doctorCredential.Str?.LicenseNumber,
                DoctorLicenseNumber = primaryCredential?.LicenseNumber,
                DoctorCredentialLicenseType = primaryCredential?.LicenseType,
                DoctorCredentialLicenseNumber = primaryCredential?.LicenseNumber,
                DoctorCredentialLicenseExpiredDate = primaryCredential?.ExpiredDate,
                DoctorCredentialLicenseIsVerified = primaryCredential?.IsVerified ?? false,
                DoctorCredentialLicenseIsCurrentlyValid = primaryCredential?.IsCurrentlyValid ?? false,
                DoctorProfilePhotoPath = doctorPhotoPath,
                DoctorProfilePhotoUrl = doctorPhotoPath,
                DoctorPhotoPath = doctorPhotoPath,
                DoctorPhotoUrl = doctorPhotoPath,
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
                PaymentType = paymentType,
                PaymentTypeName = BuildPaymentTypeDisplayName(paymentType, primaryGuarantorName, encounter?.PaymentMethod?.PaymentMethodName),
                PaymentMethodId = encounter?.PaymentMethodId,
                PaymentMethodName = encounter?.PaymentMethod?.PaymentMethodName,
                PrimaryGuarantorNameSnapshot = encounter?.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = encounter?.PrimaryGuarantorTypeSnapshot,
                PatientTotalVisitCount = totalVisitCount,
                PatientVisitNumber = totalVisitCount,
                ChiefComplaint = encounter?.ChiefComplaint,
                AgeTextAtEncounter = null,
                AgeCategoryCodeSnapshot = null,
                AgeCategoryNameSnapshot = null,
                Notes = x.Notes,
                CreateDateTime = x.CreateDateTime
            };
        }

        private async Task<IReadOnlyDictionary<Guid, DoctorWorkforceCredentialSnapshot>> BuildDoctorCredentialSnapshotsAsync(List<Guid> workforceProfileIds)
        {
            if (workforceProfileIds.Count == 0)
            {
                return new Dictionary<Guid, DoctorWorkforceCredentialSnapshot>();
            }

            var credentialRows = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    workforceProfileIds.Contains(x.WorkforceProfileId) &&
                    (x.LicenseType == "STR" || x.LicenseType == "SIP"))
                .Select(x => new DoctorCredentialLicenseSnapshot
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    LicenseType = x.LicenseType,
                    LicenseNumber = x.LicenseNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    PracticeLocation = x.PracticeLocation,
                    VerificationStatus = x.VerificationStatus,
                    IsVerified = x.IsVerified,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var today = AppDateTimeHelper.OperationalDate().Date;

            foreach (var item in credentialRows)
            {
                item.IsExpired = item.ExpiredDate.Date < today;
                item.IsCurrentlyValid = item.IsActive &&
                    item.IsVerified &&
                    item.VerificationStatus == CredentialVerificationStatus.Verified &&
                    item.ExpiredDate.Date >= today;
            }

            return credentialRows
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    x => x.Key,
                    x => new DoctorWorkforceCredentialSnapshot
                    {
                        Str = SelectPreferredCredential(x, "STR"),
                        Sip = SelectPreferredCredential(x, "SIP")
                    });
        }

        private static DoctorCredentialLicenseSnapshot? SelectPreferredCredential(
            IEnumerable<DoctorCredentialLicenseSnapshot> credentials,
            string licenseType)
        {
            return credentials
                .Where(x => x.LicenseType.Equals(licenseType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.IsCurrentlyValid)
                .ThenByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.IsExpired)
                .ThenByDescending(x => x.ExpiredDate)
                .ThenByDescending(x => x.IssueDate)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefault();
        }

        private static string ResolveDoctorPhotoPath(Guid? doctorId, IReadOnlyDictionary<Guid, string> doctorPhotoPaths)
        {
            if (doctorId.HasValue && doctorId.Value != Guid.Empty &&
                doctorPhotoPaths.TryGetValue(doctorId.Value, out var photoPath) &&
                !string.IsNullOrWhiteSpace(photoPath))
            {
                return photoPath.Trim();
            }

            return DefaultDoctorProfilePhotoPathFallback;
        }

        private static string BuildPaymentTypeDisplayName(
            EncounterPaymentType paymentType,
            string? primaryGuarantorName,
            string? paymentMethodName)
        {
            var guarantorName = NormalizeNullableText(primaryGuarantorName);
            if (guarantorName != null)
            {
                return guarantorName;
            }

            var methodName = NormalizeNullableText(paymentMethodName);
            if (methodName != null)
            {
                return methodName;
            }

            var normalizedPaymentType = paymentType.ToString().Trim().ToLowerInvariant();

            return normalizedPaymentType switch
            {
                "cash" => "Umum / Tunai",
                "patientcash" => "Umum / Tunai",
                "selfpay" => "Umum / Tunai",
                "insurance" => "Asuransi",
                "bpjs" => "BPJS",
                "company" => "Perusahaan",
                "corporate" => "Perusahaan",
                "membership" => "Membership",
                "mixed" => "Pembayaran Campuran",
                "mixedpayment" => "Pembayaran Campuran",
                _ => paymentType.ToString()
            };
        }

        private sealed class DoctorUserPhotoSnapshot
        {
            public Guid DoctorId { get; set; }
            public string? ProfilePhotoPath { get; set; }
            public bool IsActive { get; set; }
            public DateTime LastUpdatedAt { get; set; }
        }

        private sealed class DoctorWorkforceCredentialSnapshot
        {
            public static DoctorWorkforceCredentialSnapshot Empty { get; } = new();

            public DoctorCredentialLicenseSnapshot? Str { get; set; }
            public DoctorCredentialLicenseSnapshot? Sip { get; set; }
        }

        private sealed class DoctorCredentialLicenseSnapshot
        {
            public Guid Id { get; set; }
            public Guid WorkforceProfileId { get; set; }
            public string LicenseType { get; set; } = string.Empty;
            public string LicenseNumber { get; set; } = string.Empty;
            public DateTime IssueDate { get; set; }
            public DateTime ExpiredDate { get; set; }
            public string? PracticeLocation { get; set; }
            public CredentialVerificationStatus VerificationStatus { get; set; }
            public bool IsVerified { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsActive { get; set; }
            public bool IsExpired { get; set; }
            public bool IsCurrentlyValid { get; set; }
            public DateTime CreateDateTime { get; set; }
        }

        private static DoctorQueueActionResponse BuildActionResponse(TrxQueue queue, string message, QueueVoiceGenerateResponse? voiceResult = null)
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
                Message = message,
                VoiceEnabled = voiceResult?.Enabled ?? false,
                VoiceGenerated = voiceResult?.Generated ?? false,
                VoiceFromCache = voiceResult?.FromCache ?? false,
                VoiceText = voiceResult?.Text,
                VoiceAudioUrl = voiceResult?.AudioUrl,
                VoiceAudioDownloadUrl = voiceResult?.DownloadUrl,
                VoiceAudioFileName = voiceResult?.FileName,
                VoiceDateKey = voiceResult?.DateKey,
                VoiceContentType = voiceResult?.ContentType,
                VoiceErrorMessage = voiceResult?.ErrorMessage
            };
        }

        private static List<DoctorQueueStatusOptionResponse> BuildQueueStatusOptions()
        {
            return DoctorWorkflowStatuses
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
