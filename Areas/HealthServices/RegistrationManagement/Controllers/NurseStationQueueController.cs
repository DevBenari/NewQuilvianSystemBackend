using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using ResponseNurseStationQueuePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.NurseStationQueueResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/nurse-station-queues")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Nurse Station Queue",
        AreaName = "HealthServices",
        ControllerName = "NurseStationQueue",
        Description = "Operasional antrean nurse station berdasarkan cluster poli",
        SortOrder = 4
    )]
    [Tags("Health Services / Registration Management / Nurse Station Queue")]
    public class NurseStationQueueController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private const int NurseCallDurationSeconds = 30;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly QueueVoiceService _queueVoiceService;
        private readonly QueueRealtimeService _queueRealtimeService;

        public NurseStationQueueController(
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
        [ProducesResponseType(typeof(ApiResponse<NurseStationQueueFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Queue", Description = "Melihat metadata filter antrean nurse station", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationQueue", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new NurseStationQueueFilterMetadataResponse
            {
                SortOptions = new List<NurseStationQueueSortOptionResponse>
                {
                    new() { Value = "queueNumber", Label = "Nomor antrean" },
                    new() { Value = "queueDate", Label = "Tanggal antrean" },
                    new() { Value = "queueStatus", Label = "Status antrean" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "clinicName", Label = "Poli" },
                    new() { Value = "doctorName", Label = "Dokter" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueueStatusOptions = BuildQueueStatusOptions(),
                ResetButtonLabel = "Reset"
            };

            return Ok(ApiResponse<NurseStationQueueFilterMetadataResponse>.Ok(result, "Metadata filter antrean nurse station berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NurseStationQueueSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Queue", Description = "Melihat ringkasan antrean nurse station", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationQueue", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? queueDate, [FromQuery] Guid? nurseStationClusterId)
        {
            var clusterIds = await GetAllowedClusterIdsAsync(nurseStationClusterId);
            if (!clusterIds.Any()) return Forbid();

            var clinicIds = await GetClinicIdsByClusterIdsAsync(clusterIds);
            var query = BuildQueueBaseQuery(queueDate, clinicIds);
            var operationalQuery = ApplyNurseOperationalStatusFilter(query);

            var result = new NurseStationQueueSummaryResponse
            {
                TotalQueue = await operationalQuery.CountAsync(),
                WaitingForNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForNurse),
                CalledByNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.CalledByNurse),
                InNurseScreeningQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.InNurseScreening),
                WaitingForDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForDoctor),
                CompletedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Completed || x.CompletedAt.HasValue),
                SkippedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Skipped),
                NoShowQueue = await query.CountAsync(x => x.NoShowAt.HasValue),
                PriorityQueue = await operationalQuery.CountAsync(x => x.IsPriorityQueue)
            };

            return Ok(ApiResponse<NurseStationQueueSummaryResponse>.Ok(result, "Ringkasan antrean nurse station berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNurseStationQueuePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nurse Station Queue", Description = "Melihat antrean nurse station sesuai cluster petugas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NurseStationQueue", "Read")]
        public async Task<IActionResult> GetQueues(
            [FromQuery] DateTime? queueDate,
            [FromQuery] Guid? nurseStationClusterId,
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

            var clusterIds = await GetAllowedClusterIdsAsync(nurseStationClusterId);
            if (!clusterIds.Any()) return Forbid();

            var clinicIds = await GetClinicIdsByClusterIdsAsync(clusterIds);
            var clusterMap = await GetClusterMapByClinicIdsAsync(clinicIds);

            var query = BuildQueueBaseQuery(queueDate, clinicIds);
            query = ApplyStandardFilter(query, queueStatus, search);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new ResponseNurseStationQueuePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, clusterMap)).ToList()
            };

            return Ok(ApiResponse<ResponseNurseStationQueuePagedResult>.Ok(result, "Data antrean nurse station berhasil diambil."));
        }

        [HttpPost("{id:guid}/call")]
        [AccessAction("Update", "Call Nurse Station Queue", Description = "Memanggil pasien ke nurse station", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("NurseStationQueue", "Update")]
        public async Task<IActionResult> Call(Guid id)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();
            if (queue.QueueStatus != QueueStatus.WaitingForNurse && queue.QueueStatus != QueueStatus.CalledByNurse)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Antrean tidak dalam status menunggu perawat."));

            var now = DateTime.UtcNow;

            if (queue.QueueStatus == QueueStatus.CalledByNurse &&
                queue.NurseCallExpiresAt.HasValue &&
                queue.NurseCallExpiresAt.Value > now)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Timer panggilan perawat masih berjalan. Tunggu sampai timer selesai sebelum memanggil ulang."
                ));
            }

            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.CalledByNurse;
            queue.NurseCallAttemptCount += 1;
            queue.LastNurseCalledAt = now;
            queue.LastNurseCalledByUserId = actorUserId;
            queue.NurseCallExpiresAt = now.AddSeconds(NurseCallDurationSeconds);
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var message = $"Pasien berhasil dipanggil ke nurse station. Panggilan ke-{queue.NurseCallAttemptCount}, timer {NurseCallDurationSeconds} detik.";
            await _queueRealtimeService.NotifyQueueCalledByNurseAsync(queue, actorUserId, message);

            var voiceResult = await _queueVoiceService.GetOrCreateQueueCallAudioAsync(queue, QueueVoiceCallTypes.Nurse);

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(
                BuildActionResponse(queue, message, voiceResult),
                message
            ));
        }

        [HttpPost("{id:guid}/start-screening")]
        [AccessAction("Update", "Start Nurse Screening", Description = "Memulai screening perawat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NurseStationQueue", "Update")]
        public async Task<IActionResult> StartScreening(Guid id)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();
            if (queue.QueueStatus != QueueStatus.CalledByNurse && queue.QueueStatus != QueueStatus.WaitingForNurse)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Antrean belum siap untuk screening."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.InNurseScreening;
            queue.ScreeningStartedAt ??= now;
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = EncounterStatus.InNurseScreening;
                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();
            await _queueRealtimeService.NotifyQueueScreeningStartedAsync(queue, actorUserId, "Screening perawat dimulai.");

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(BuildActionResponse(queue, "Screening perawat dimulai."), "Screening perawat dimulai."));
        }

        [HttpPost("{id:guid}/finish-screening")]
        [AccessAction("Update", "Finish Nurse Screening", Description = "Menyelesaikan screening dan mengirim pasien ke dokter", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("NurseStationQueue", "Update")]
        public async Task<IActionResult> FinishScreening(Guid id, [FromBody] NurseStationQueueActionRequest? request = null)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();
            if (queue.QueueStatus != QueueStatus.InNurseScreening)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Screening belum dimulai."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.ScreeningCompletedAt = now;
            queue.QueueStatus = queue.IsDoctorRequired ? QueueStatus.WaitingForDoctor : QueueStatus.Completed;
            queue.Notes = MergeNotes(queue.Notes, request?.Notes);
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (!queue.IsDoctorRequired)
            {
                queue.CompletedAt = now;
                queue.CompletedByUserId = actorUserId;
            }

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = queue.IsDoctorRequired ? EncounterStatus.WaitingForDoctor : EncounterStatus.Completed;
                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
                if (!queue.IsDoctorRequired)
                {
                    queue.Encounter.CompletedAt = now;
                }
            }

            await _dbContext.SaveChangesAsync();
            await _queueRealtimeService.NotifyQueueScreeningFinishedAsync(queue, actorUserId, "Screening perawat selesai.");

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(BuildActionResponse(queue, "Screening perawat selesai dan pasien dikirim ke dokter."), "Screening perawat selesai dan pasien dikirim ke dokter."));
        }

        [HttpPost("{id:guid}/skip")]
        [AccessAction("Update", "Skip Nurse Station Queue", Description = "Melewati pasien dari daftar panggilan perawat", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("NurseStationQueue", "Update")]
        public async Task<IActionResult> Skip(Guid id, [FromBody] NurseStationQueueActionRequest? request = null)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (queue.QueueStatus != QueueStatus.CalledByNurse)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pasien hanya bisa dilewati setelah berada dalam status dipanggil perawat."
                ));
            }

            if (queue.NurseCallExpiresAt.HasValue && queue.NurseCallExpiresAt.Value > now)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Timer panggilan perawat masih berjalan."
                ));
            }

            queue.QueueStatus = QueueStatus.Skipped;
            queue.SkipCount += 1;
            queue.LastSkippedAt = now;
            queue.LastSkippedByUserId = actorUserId;
            queue.SkipReason = NormalizeNullableText(request?.Reason) ?? "Tidak hadir saat dipanggil perawat.";
            queue.Notes = MergeNotes(queue.Notes, request?.Notes);
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await _queueRealtimeService.NotifyQueueSkippedByNurseAsync(queue, actorUserId, "Pasien berhasil dilewati.");

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dilewati."),
                "Pasien berhasil dilewati."
            ));
        }

        [HttpPost("{id:guid}/no-show")]
        [AccessAction("Update", "No Show Nurse Station Queue", Description = "Menandai pasien tidak hadir di nurse station", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("NurseStationQueue", "Update")]
        public async Task<IActionResult> NoShow(Guid id, [FromBody] NurseStationQueueActionRequest request)
        {
            var queue = await GetAllowedQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            queue.QueueStatus = QueueStatus.NoShow;
            queue.NoShowAt = now;
            queue.NoShowByUserId = actorUserId;
            queue.NoShowReason = NormalizeNullableText(request.Reason) ?? "Pasien tidak hadir saat dipanggil nurse station.";
            queue.UpdateDateTime = now;
            queue.UpdateBy = actorUserId;

            if (queue.Encounter != null)
            {
                queue.Encounter.EncounterStatus = EncounterStatus.NoShow;
                queue.Encounter.NoShowAt = now;
                queue.Encounter.NoShowByUserId = actorUserId;
                queue.Encounter.NoShowReason = queue.NoShowReason;
                queue.Encounter.UpdateDateTime = now;
                queue.Encounter.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();
            await _queueRealtimeService.NotifyQueueNoShowByNurseAsync(queue, actorUserId, "Pasien berhasil ditandai tidak hadir.");

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(BuildActionResponse(queue, "Pasien berhasil ditandai tidak hadir."), "Pasien berhasil ditandai tidak hadir."));
        }

        private IQueryable<TrxQueue> BuildQueueBaseQuery(DateTime? queueDate, List<Guid> clinicIds)
        {
            var selectedDate = AppDateTimeHelper.ResolveOperationalDate(queueDate);
            var startDate = selectedDate.Date;
            var endDate = startDate.AddDays(1);

            return _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Include(x => x.Encounter)
                    .ThenInclude(x => x.PaymentMethod)
                .Include(x => x.Encounter)
                    .ThenInclude(x => x.EncounterGuarantors)
                        .ThenInclude(x => x.PaymentMethod)
                .Include(x => x.Encounter)
                    .ThenInclude(x => x.EncounterGuarantors)
                        .ThenInclude(x => x.InsuranceProvider)
                .Include(x => x.Encounter)
                    .ThenInclude(x => x.EncounterGuarantors)
                        .ThenInclude(x => x.CompanyGuarantor)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.Country)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.Province)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.City)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.District)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.PostalCode)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.DefaultMembershipTier)
                .Include(x => x.Patient)
                    .ThenInclude(x => x.MotherPatient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.QueueDate >= startDate &&
                    x.QueueDate < endDate &&
                    x.ClinicId.HasValue &&
                    clinicIds.Contains(x.ClinicId.Value));
        }

        private static IQueryable<TrxQueue> ApplyStandardFilter(IQueryable<TrxQueue> query, QueueStatus? queueStatus, string? search)
        {
            query = queueStatus.HasValue
                ? query.Where(x => x.QueueStatus == queueStatus.Value)
                : ApplyNurseOperationalStatusFilter(query);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.QueueCode.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IQueryable<TrxQueue> ApplyNurseOperationalStatusFilter(IQueryable<TrxQueue> query)
        {
            return query.Where(x =>
                x.QueueStatus == QueueStatus.WaitingForNurse ||
                x.QueueStatus == QueueStatus.CalledByNurse ||
                x.QueueStatus == QueueStatus.InNurseScreening);
        }

        private static IOrderedQueryable<TrxQueue> ApplySorting(IQueryable<TrxQueue> query, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "queueNumber").Trim().ToLowerInvariant() switch
            {
                "queuedate" => isDescending
                    ? query.OrderByDescending(x => x.QueueDate)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenByDescending(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenByDescending(x => x.QueueNumber)
                    : query.OrderBy(x => x.QueueDate)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                "queuestatus" => isDescending
                    ? query.OrderByDescending(x => x.QueueStatus)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber)
                    : query.OrderBy(x => x.QueueStatus)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                "patientname" => isDescending
                    ? query.OrderByDescending(x => x.Patient!.FullName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber)
                    : query.OrderBy(x => x.Patient!.FullName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                "clinicname" => isDescending
                    ? query.OrderByDescending(x => x.Clinic!.ClinicName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber)
                    : query.OrderBy(x => x.Clinic!.ClinicName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                "doctorname" => isDescending
                    ? query.OrderByDescending(x => x.Doctor!.FullName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber)
                    : query.OrderBy(x => x.Doctor!.FullName)
                        .ThenByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                        .ThenByDescending(x => x.QueueNumber)
                    : query.OrderBy(x => x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber),

                _ => isDescending
                    ? query.OrderByDescending(x => x.IsPriorityQueue)
                        .ThenByDescending(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenByDescending(x => x.QueueNumber)
                    : query.OrderByDescending(x => x.IsPriorityQueue)
                        .ThenBy(x => x.LastSkippedAt ?? x.CreateDateTime)
                        .ThenBy(x => x.QueueNumber)
            };
        }

        private async Task<List<Guid>> GetAllowedClusterIdsAsync(Guid? requestedClusterId)
        {
            if (User.IsInRole("SuperAdmin"))
            {
                var query = _dbContext.Set<MstNurseStationCluster>().AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
                if (requestedClusterId.HasValue && requestedClusterId.Value != Guid.Empty) query = query.Where(x => x.Id == requestedClusterId.Value);
                return await query.Select(x => x.Id).ToListAsync();
            }

            var employee = await ResolveCurrentEmployeeAsync();
            if (employee == null) return new List<Guid>();

            var clusterQuery = _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.EmployeeId == employee.Id);

            if (requestedClusterId.HasValue && requestedClusterId.Value != Guid.Empty)
            {
                clusterQuery = clusterQuery.Where(x => x.NurseStationClusterId == requestedClusterId.Value);
            }

            return await clusterQuery.Select(x => x.NurseStationClusterId).Distinct().ToListAsync();
        }

        private async Task<MstEmployee?> ResolveCurrentEmployeeAsync()
        {
            var employeeIdClaim = User.FindFirstValue("employee_id") ?? User.FindFirstValue("EmployeeId");
            if (Guid.TryParse(employeeIdClaim, out var employeeId))
            {
                return await _dbContext.Set<MstEmployee>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDelete && x.IsActive);
            }

            var workforceClaim = User.FindFirstValue("workforce_profile_id") ?? User.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                return await _dbContext.Set<MstEmployee>().AsNoTracking().FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive);
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser?.Email == null) return null;

            var email = currentUser.Email.ToLower();
            return await _dbContext.Set<MstEmployee>().AsNoTracking().FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email);
        }

        private async Task<List<Guid>> GetClinicIdsByClusterIdsAsync(List<Guid> clusterIds)
        {
            return await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && clusterIds.Contains(x.NurseStationClusterId))
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync();
        }

        private async Task<Dictionary<Guid, (Guid ClusterId, string ClusterName)>> GetClusterMapByClinicIdsAsync(List<Guid> clinicIds)
        {
            if (clinicIds == null || clinicIds.Count == 0)
            {
                return new Dictionary<Guid, (Guid ClusterId, string ClusterName)>();
            }

            var mappings = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && clinicIds.Contains(x.ClinicId))
                .Select(x => new
                {
                    x.ClinicId,
                    x.NurseStationClusterId,
                    ClusterName = x.NurseStationCluster != null
                        ? x.NurseStationCluster.ClusterName
                        : string.Empty
                })
                .ToListAsync();

            return mappings
                .GroupBy(x => x.ClinicId)
                .ToDictionary(
                    x => x.Key,
                    x =>
                    {
                        var first = x.First();
                        return (first.NurseStationClusterId, first.ClusterName);
                    });
        }

        private async Task<TrxQueue?> GetAllowedQueueWithEncounterAsync(Guid id)
        {
            var clusterIds = await GetAllowedClusterIdsAsync(null);
            var clinicIds = await GetClinicIdsByClusterIdsAsync(clusterIds);
            return await _dbContext.Set<TrxQueue>()
                .Include(x => x.Encounter)
                .Include(x => x.Patient)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && x.IsActive && x.ClinicId.HasValue && clinicIds.Contains(x.ClinicId.Value));
        }

        private static NurseStationQueueResponse MapResponse(TrxQueue x, IReadOnlyDictionary<Guid, (Guid ClusterId, string ClusterName)> clusterMap)
        {
            Guid? clusterId = null;
            string? clusterName = null;
            if (x.ClinicId.HasValue && clusterMap.TryGetValue(x.ClinicId.Value, out var cluster))
            {
                clusterId = cluster.ClusterId;
                clusterName = cluster.ClusterName;
            }

            var encounter = x.Encounter;
            var patient = x.Patient;
            var guarantors = encounter?.EncounterGuarantors?
                .Where(g => !g.IsDelete)
                .OrderBy(g => g.CoveragePriority)
                .ThenByDescending(g => g.IsPrimary)
                .Select(MapGuarantorResponse)
                .ToList() ?? new List<NurseStationQueueGuarantorResponse>();

            return new NurseStationQueueResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = encounter != null ? encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = patient != null ? patient.FullName : string.Empty,
                MedicalRecordNumber = patient != null ? patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                NurseStationClusterId = clusterId,
                NurseStationClusterName = clusterName,
                QueueDate = x.QueueDate,
                QueueNumber = x.QueueNumber,
                QueueCode = x.QueueCode,
                QueueStatus = x.QueueStatus,
                QueueStatusName = BuildEnumLabel(x.QueueStatus),
                NurseCallAttemptCount = x.NurseCallAttemptCount,
                LastNurseCalledAt = x.LastNurseCalledAt,
                NurseCallExpiresAt = x.NurseCallExpiresAt,
                ScreeningStartedAt = x.ScreeningStartedAt,
                ScreeningCompletedAt = x.ScreeningCompletedAt,
                SkipCount = x.SkipCount,
                RequeueCount = x.RequeueCount,
                NoShowAt = x.NoShowAt,
                IsPriorityQueue = x.IsPriorityQueue,
                IsScreeningRequired = x.IsScreeningRequired,
                IsDoctorRequired = x.IsDoctorRequired,
                Notes = x.Notes,
                CreateDateTime = x.CreateDateTime,

                ChiefComplaint = encounter?.ChiefComplaint,
                EncounterDate = encounter?.EncounterDate ?? default,
                RegisteredAt = encounter?.RegisteredAt ?? default,
                EncounterType = encounter?.EncounterType ?? default,
                EncounterTypeName = encounter != null ? BuildEnumLabel(encounter.EncounterType) : string.Empty,
                VisitType = encounter?.VisitType ?? default,
                VisitTypeName = encounter != null ? BuildEnumLabel(encounter.VisitType) : string.Empty,
                RegistrationSource = encounter?.RegistrationSource ?? default,
                RegistrationSourceName = encounter != null ? BuildEnumLabel(encounter.RegistrationSource) : string.Empty,
                EncounterStatus = encounter?.EncounterStatus ?? EncounterStatus.Registered,
                EncounterStatusName = encounter != null ? BuildEnumLabel(encounter.EncounterStatus) : string.Empty,
                PaymentType = encounter?.PaymentType ?? EncounterPaymentType.Cash,
                PaymentTypeName = encounter != null ? BuildEnumLabel(encounter.PaymentType) : string.Empty,
                IsInsurancePatient = encounter?.IsInsurancePatient ?? false,
                IsCompanyPatient = encounter?.IsCompanyPatient ?? false,
                IsMembershipPatient = encounter?.IsMembershipPatient ?? false,
                IsMixedPayment = encounter?.IsMixedPayment ?? false,
                PrimaryGuarantorNameSnapshot = encounter?.PrimaryGuarantorNameSnapshot,
                PrimaryGuarantorTypeSnapshot = encounter?.PrimaryGuarantorTypeSnapshot,
                IsEligibilityRequired = encounter?.IsEligibilityRequired ?? false,
                IsEligibilityCompleted = encounter?.IsEligibilityCompleted ?? false,
                IsReferral = encounter?.IsReferral ?? false,
                ReferralNumber = encounter?.ReferralNumber,
                EligibilityReferenceNumber = encounter?.EligibilityReferenceNumber,
                Guarantors = guarantors,

                PatientCode = patient?.PatientCode ?? string.Empty,
                NickName = patient?.NickName,
                BirthPlace = patient?.BirthPlace,
                BirthDate = patient?.BirthDate,
                GenderName = BuildOptionalLabel(patient?.Gender),
                ReligionName = BuildOptionalLabel(patient?.Religion),
                MaritalStatusName = BuildOptionalLabel(patient?.MaritalStatus),
                BloodTypeName = BuildOptionalLabel(patient?.BloodType),
                IdentityTypeName = BuildOptionalLabel(patient?.IdentityType),
                IdentityNumber = patient?.IdentityNumber,
                PhoneNumber = patient?.PhoneNumber,
                WhatsAppNumber = patient?.WhatsAppNumber,
                Email = patient?.Email,
                Address = BuildPatientAddress(patient),
                CountryName = patient?.Country?.CountryName,
                ProvinceName = patient?.Province?.ProvinceName,
                CityName = patient?.City?.CityName,
                DistrictName = patient?.District?.DistrictName,
                PostalCode = patient?.PostalCode?.PostalCode,
                IsMember = patient?.IsMember ?? false,
                ActivePatientMembershipId = patient?.ActivePatientMembershipId,
                DefaultMembershipTierId = patient?.DefaultMembershipTierId,
                DefaultMembershipTierName = patient?.DefaultMembershipTier?.TierName,
                IsNewborn = patient?.IsNewborn ?? false,
                MotherPatientId = patient?.MotherPatientId,
                MotherMedicalRecordNumber = patient?.MotherPatient?.MedicalRecordNumber,
                MotherPatientName = patient?.MotherPatient?.FullName
            };
        }

        private static NurseStationQueueGuarantorResponse MapGuarantorResponse(TrxPatientEncounterGuarantor guarantor)
        {
            return new NurseStationQueueGuarantorResponse
            {
                Id = guarantor.Id,
                EncounterGuarantorNumber = guarantor.EncounterGuarantorNumber,
                GuarantorType = guarantor.GuarantorType,
                GuarantorTypeName = BuildEnumLabel(guarantor.GuarantorType),
                GuarantorRole = guarantor.GuarantorRole,
                GuarantorRoleName = BuildEnumLabel(guarantor.GuarantorRole),
                GuarantorStatus = guarantor.GuarantorStatus,
                GuarantorStatusName = BuildEnumLabel(guarantor.GuarantorStatus),
                CoveragePriority = guarantor.CoveragePriority,
                IsPrimary = guarantor.IsPrimary,
                GuarantorNameSnapshot = guarantor.GuarantorNameSnapshot,
                PolicyNumberSnapshot = guarantor.PolicyNumberSnapshot,
                CardNumberSnapshot = guarantor.CardNumberSnapshot,
                MemberNumberSnapshot = guarantor.MemberNumberSnapshot,
                PlanNameSnapshot = guarantor.PlanNameSnapshot,
                ClassNameSnapshot = guarantor.ClassNameSnapshot,
                PaymentMethodName = guarantor.PaymentMethod?.PaymentMethodName,
                InsuranceProviderName = guarantor.InsuranceProvider?.InsuranceProviderName,
                CompanyGuarantorName = guarantor.CompanyGuarantor?.CompanyGuarantorName,
                IsEligibilityRequired = guarantor.IsEligibilityRequired,
                IsEligible = guarantor.IsEligible,
                IsVerified = guarantor.IsVerified,
                IsActive = guarantor.IsActive
            };
        }

        private static NurseStationQueueActionResponse BuildActionResponse(TrxQueue queue, string message, QueueVoiceGenerateResponse? voiceResult = null)
        {
            return new NurseStationQueueActionResponse
            {
                QueueId = queue.Id,
                EncounterId = queue.EncounterId,
                QueueStatus = queue.QueueStatus,
                QueueStatusName = BuildEnumLabel(queue.QueueStatus),
                EncounterStatus = queue.Encounter?.EncounterStatus ?? EncounterStatus.Registered,
                EncounterStatusName = BuildEnumLabel(queue.Encounter?.EncounterStatus ?? EncounterStatus.Registered),
                NurseCallAttemptCount = queue.NurseCallAttemptCount,
                NurseCallExpiresAt = queue.NurseCallExpiresAt,
                ScreeningStartedAt = queue.ScreeningStartedAt,
                ScreeningCompletedAt = queue.ScreeningCompletedAt,
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

        private static List<NurseStationQueueStatusOptionResponse> BuildQueueStatusOptions()
        {
            return Enum.GetValues<QueueStatus>()
                .Select(x => new NurseStationQueueStatusOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = BuildEnumLabel(x) })
                .ToList();
        }


        private static string? BuildPatientAddress(MstPatient? patient)
        {
            if (patient == null) return null;

            string? directAddress = patient.Address;
            if (!string.IsNullOrWhiteSpace(directAddress))
            {
                return directAddress.Trim();
            }

            var parts = new[]
            {
                patient.District?.DistrictName,
                patient.City?.CityName,
                patient.Province?.ProvinceName,
                patient.Country?.CountryName,
                patient.PostalCode?.PostalCode
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());

            var result = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        private static string? BuildOptionalLabel(object? value)
        {
            if (value == null) return null;
            if (value is Enum enumValue) return BuildEnumLabel(enumValue);
            var text = value.ToString();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string BuildEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            var displayAttribute = memberInfo?
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            return displayAttribute?.Name ?? SplitPascalCase(value.ToString());
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private IActionResult QueueNotFound()
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Antrean tidak ditemukan atau tidak termasuk cluster petugas."));
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
