using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
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
        private const int MaxNurseCallAttemptCount = 2;
        private const int NurseCallDurationSeconds = 60;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public NurseStationQueueController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
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

            var result = new NurseStationQueueSummaryResponse
            {
                TotalQueue = await query.CountAsync(),
                WaitingForNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForNurse),
                CalledByNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.CalledByNurse),
                InNurseScreeningQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.InNurseScreening),
                WaitingForDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForDoctor),
                CompletedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Completed || x.CompletedAt.HasValue),
                SkippedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Skipped),
                NoShowQueue = await query.CountAsync(x => x.NoShowAt.HasValue),
                PriorityQueue = await query.CountAsync(x => x.IsPriorityQueue)
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

            if (queue.NurseCallAttemptCount >= MaxNurseCallAttemptCount)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pasien sudah dipanggil 2 kali oleh perawat. Silakan lakukan lewati jika pasien belum datang."
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

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(
                BuildActionResponse(queue, $"Pasien berhasil dipanggil ke nurse station. Panggilan ke-{queue.NurseCallAttemptCount}, timer {NurseCallDurationSeconds} detik."),
                $"Pasien berhasil dipanggil ke nurse station. Panggilan ke-{queue.NurseCallAttemptCount}, timer {NurseCallDurationSeconds} detik."
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

            return Ok(ApiResponse<NurseStationQueueActionResponse>.Ok(BuildActionResponse(queue, "Screening perawat selesai dan pasien dikirim ke dokter."), "Screening perawat selesai dan pasien dikirim ke dokter."));
        }

        [HttpPost("{id:guid}/skip")]
        [AccessAction("Update", "Skip Nurse Station Queue", Description = "Melewati pasien yang tidak hadir setelah 2 kali panggilan perawat", AccessType = AccessTypes.Update, SortOrder = 5)]
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

            if (queue.NurseCallAttemptCount < MaxNurseCallAttemptCount)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pasien hanya bisa dilewati setelah 2 kali panggilan perawat."
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
                .Include(x => x.Patient)
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
            if (queueStatus.HasValue) query = query.Where(x => x.QueueStatus == queueStatus.Value);
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

            return new NurseStationQueueResponse
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
                CreateDateTime = x.CreateDateTime
            };
        }

        private static NurseStationQueueActionResponse BuildActionResponse(TrxQueue queue, string message)
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
                Message = message
            };
        }

        private static List<NurseStationQueueStatusOptionResponse> BuildQueueStatusOptions()
        {
            return Enum.GetValues<QueueStatus>()
                .Select(x => new NurseStationQueueStatusOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = BuildEnumLabel(x) })
                .ToList();
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
