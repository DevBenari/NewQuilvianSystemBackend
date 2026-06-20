using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseQueuePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs.QueueResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/queues")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Queue",
        AreaName = "HealthServices",
        ControllerName = "Queue",
        Description = "Transaksi antrean pasien rawat jalan",
        SortOrder = 3
    )]
    [Tags("Health Services / Registration Management / Queue")]
    public class QueueController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private const string KioskReadPolicy = "KioskRead";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public QueueController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseQueuePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue", Description = "Melihat antrean pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Queue", "Read")]
        public async Task<IActionResult> GetQueues(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? doctorId,
            [FromQuery] QueueStatus? queueStatus,
            [FromQuery] DateTime? queueDate,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);

            var selectedDate = queueDate?.Date ?? DateTime.UtcNow.Date;

            var query = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.QueueDate == selectedDate);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (queueStatus.HasValue)
                query = query.Where(x => x.QueueStatus == queueStatus.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.QueueCode.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.IsPriorityQueue ? 0 : 1)
                .ThenBy(x => x.QueueNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            return Ok(ApiResponse<ResponseQueuePagedResult>.Ok(
                new ResponseQueuePagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data antrean berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Queue", Description = "Melihat detail antrean pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Queue", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new QueueDetailResponse
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
                    DoctorScheduleId = x.DoctorScheduleId,
                    QueueDate = x.QueueDate,
                    QueueNumber = x.QueueNumber,
                    QueueCode = x.QueueCode,
                    QueueStatus = x.QueueStatus,
                    NurseCallAttemptCount = x.NurseCallAttemptCount,
                    NurseCallExpiresAt = x.NurseCallExpiresAt,
                    DoctorCallAttemptCount = x.DoctorCallAttemptCount,
                    DoctorCallExpiresAt = x.DoctorCallExpiresAt,
                    LastNurseCalledAt = x.LastNurseCalledAt,
                    LastDoctorCalledAt = x.LastDoctorCalledAt,
                    ScreeningStartedAt = x.ScreeningStartedAt,
                    ScreeningCompletedAt = x.ScreeningCompletedAt,
                    ConsultationStartedAt = x.ConsultationStartedAt,
                    ConsultationCompletedAt = x.ConsultationCompletedAt,
                    SkipCount = x.SkipCount,
                    LastSkippedAt = x.LastSkippedAt,
                    SkipReason = x.SkipReason,
                    RequeueCount = x.RequeueCount,
                    LastRequeuedAt = x.LastRequeuedAt,
                    RequeueReason = x.RequeueReason,
                    NoShowAt = x.NoShowAt,
                    NoShowReason = x.NoShowReason,
                    CancelledAt = x.CancelledAt,
                    CancelReason = x.CancelReason,
                    CompletedAt = x.CompletedAt,
                    IsPriorityQueue = x.IsPriorityQueue,
                    IsScreeningRequired = x.IsScreeningRequired,
                    IsDoctorRequired = x.IsDoctorRequired,
                    Notes = x.Notes,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Antrean tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<QueueDetailResponse>.Ok(result, "Detail antrean berhasil diambil."));
        }

        [HttpPost("{id:guid}/call-nurse")]
        [Authorize(Policy = KioskReadPolicy)]
        [AccessAction("Update", "Call Nurse Queue", Description = "Memanggil pasien untuk screening perawat", AccessType = AccessTypes.Update, SortOrder = 2)]        
        public async Task<IActionResult> CallNurse(Guid id)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.WaitingForNurse && queue.QueueStatus != QueueStatus.CalledByNurse)
                return BadRequest(ApiResponse<object>.Fail(400, "Antrean tidak dalam status menunggu perawat."));

            var now = DateTime.UtcNow;
            queue.QueueStatus = QueueStatus.CalledByNurse;
            queue.NurseCallAttemptCount += 1;
            queue.LastNurseCalledAt = now;
            queue.LastNurseCalledByUserId = GetCurrentUserId();
            queue.NurseCallExpiresAt = now.AddMinutes(2);
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dipanggil oleh perawat."),
                "Pasien berhasil dipanggil oleh perawat."
            ));
        }

        [HttpPost("{id:guid}/start-screening")]
        [AccessAction("Update", "Start Screening Queue", Description = "Memulai screening perawat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> StartScreening(Guid id)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.CalledByNurse && queue.QueueStatus != QueueStatus.WaitingForNurse)
                return BadRequest(ApiResponse<object>.Fail(400, "Antrean belum siap untuk screening."));

            var now = DateTime.UtcNow;
            queue.QueueStatus = QueueStatus.InNurseScreening;
            queue.ScreeningStartedAt = now;
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            queue.Encounter!.EncounterStatus = EncounterStatus.InNurseScreening;
            queue.Encounter.UpdateDateTime = now;
            queue.Encounter.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Screening perawat dimulai."),
                "Screening perawat dimulai."
            ));
        }

        [HttpPost("{id:guid}/finish-screening")]
        [AccessAction("Update", "Finish Screening Queue", Description = "Menyelesaikan screening perawat", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> FinishScreening(Guid id)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.InNurseScreening)
                return BadRequest(ApiResponse<object>.Fail(400, "Screening belum dimulai."));

            var now = DateTime.UtcNow;
            queue.ScreeningCompletedAt = now;
            queue.QueueStatus = queue.IsDoctorRequired ? QueueStatus.WaitingForDoctor : QueueStatus.Completed;
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            queue.Encounter!.EncounterStatus = queue.IsDoctorRequired
                ? EncounterStatus.WaitingForDoctor
                : EncounterStatus.Completed;

            queue.Encounter.UpdateDateTime = now;
            queue.Encounter.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Screening perawat selesai."),
                "Screening perawat selesai."
            ));
        }

        [HttpPost("{id:guid}/call-doctor")]
        [AccessAction("Update", "Call Doctor Queue", Description = "Memanggil pasien ke dokter", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> CallDoctor(Guid id)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;

            if (queue.QueueStatus != QueueStatus.WaitingForDoctor && queue.QueueStatus != QueueStatus.CalledByDoctor)
                return BadRequest(ApiResponse<object>.Fail(400, "Antrean tidak dalam status menunggu dokter."));

            if (queue.QueueStatus == QueueStatus.CalledByDoctor &&
                queue.DoctorCallExpiresAt.HasValue &&
                queue.DoctorCallExpiresAt.Value > now)
            {
                return BadRequest(ApiResponse<object>.Fail(400, "Timer panggilan dokter masih berjalan."));
            }

            if (queue.DoctorCallAttemptCount >= 2)
            {
                return BadRequest(ApiResponse<object>.Fail(400, "Pasien sudah dipanggil 2 kali. Silakan lakukan skip."));
            }

            queue.QueueStatus = QueueStatus.CalledByDoctor;
            queue.DoctorCallAttemptCount += 1;
            queue.LastDoctorCalledAt = now;
            queue.LastDoctorCalledByUserId = GetCurrentUserId();
            queue.DoctorCallExpiresAt = now.AddMinutes(2);
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dipanggil oleh dokter."),
                "Pasien berhasil dipanggil oleh dokter."
            ));
        }

        [HttpPost("{id:guid}/start-consultation")]
        [AccessAction("Update", "Start Consultation Queue", Description = "Memulai konsultasi dokter", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> StartConsultation(Guid id)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.CalledByDoctor && queue.QueueStatus != QueueStatus.WaitingForDoctor)
                return BadRequest(ApiResponse<object>.Fail(400, "Antrean belum siap untuk konsultasi dokter."));

            var now = DateTime.UtcNow;
            queue.QueueStatus = QueueStatus.InConsultation;
            queue.ConsultationStartedAt = now;
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            queue.Encounter!.EncounterStatus = EncounterStatus.InConsultation;
            queue.Encounter.UpdateDateTime = now;
            queue.Encounter.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Konsultasi dokter dimulai."),
                "Konsultasi dokter dimulai."
            ));
        }

        [HttpPost("{id:guid}/skip")]
        [AccessAction("Update", "Skip Queue", Description = "Melewati pasien yang tidak hadir saat dipanggil", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> Skip(Guid id, [FromBody] QueueActionRequest request)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            var now = DateTime.UtcNow;

            if (queue.QueueStatus != QueueStatus.CalledByDoctor || queue.DoctorCallAttemptCount < 2)
                return BadRequest(ApiResponse<object>.Fail(400, "Pasien hanya bisa di-skip setelah 2 kali panggilan dokter."));

            if (queue.DoctorCallExpiresAt.HasValue && queue.DoctorCallExpiresAt.Value > now)
                return BadRequest(ApiResponse<object>.Fail(400, "Timer panggilan dokter masih berjalan."));

            queue.QueueStatus = QueueStatus.Skipped;
            queue.SkipCount += 1;
            queue.LastSkippedAt = now;
            queue.LastSkippedByUserId = GetCurrentUserId();
            queue.SkipReason = NormalizeNullableText(request.Reason) ?? "Tidak hadir saat dipanggil dokter.";
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dilewati."),
                "Pasien berhasil dilewati."
            ));
        }

        [HttpPost("{id:guid}/requeue")]
        [AccessAction("Update", "Requeue Queue", Description = "Mengembalikan pasien ke antrean dokter", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("Queue", "Update")]
        public async Task<IActionResult> Requeue(Guid id, [FromBody] QueueActionRequest request)
        {
            var queue = await GetQueueWithEncounterAsync(id);
            if (queue == null) return QueueNotFound();

            if (queue.QueueStatus != QueueStatus.Skipped)
                return BadRequest(ApiResponse<object>.Fail(400, "Hanya antrean skipped yang bisa dikembalikan."));

            var now = DateTime.UtcNow;
            queue.QueueStatus = QueueStatus.WaitingForDoctor;
            queue.RequeueCount += 1;
            queue.LastRequeuedAt = now;
            queue.LastRequeuedByUserId = GetCurrentUserId();
            queue.RequeueReason = NormalizeNullableText(request.Reason);
            queue.DoctorCallAttemptCount = 0;
            queue.DoctorCallExpiresAt = null;
            queue.UpdateDateTime = now;
            queue.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<QueueActionResponse>.Ok(
                BuildActionResponse(queue, "Pasien berhasil dikembalikan ke antrean dokter."),
                "Pasien berhasil dikembalikan ke antrean dokter."
            ));
        }

        private async Task<TrxQueue?> GetQueueWithEncounterAsync(Guid id)
        {
            return await _dbContext.Set<TrxQueue>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
        }

        private IActionResult QueueNotFound()
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Antrean tidak ditemukan."
            ));
        }

        private static QueueResponse ToResponse(TrxQueue x)
        {
            return new QueueResponse
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
                DoctorScheduleId = x.DoctorScheduleId,
                QueueDate = x.QueueDate,
                QueueNumber = x.QueueNumber,
                QueueCode = x.QueueCode,
                QueueStatus = x.QueueStatus,
                NurseCallAttemptCount = x.NurseCallAttemptCount,
                NurseCallExpiresAt = x.NurseCallExpiresAt,
                DoctorCallAttemptCount = x.DoctorCallAttemptCount,
                DoctorCallExpiresAt = x.DoctorCallExpiresAt,
                SkipCount = x.SkipCount,
                RequeueCount = x.RequeueCount,
                IsPriorityQueue = x.IsPriorityQueue,
                IsScreeningRequired = x.IsScreeningRequired,
                IsDoctorRequired = x.IsDoctorRequired,
                CreateDateTime = x.CreateDateTime
            };
        }

        private QueueActionResponse BuildActionResponse(TrxQueue queue, string message)
        {
            return new QueueActionResponse
            {
                QueueId = queue.Id,
                EncounterId = queue.EncounterId,
                QueueStatus = queue.QueueStatus,
                EncounterStatus = queue.Encounter?.EncounterStatus ?? EncounterStatus.Registered,
                NurseCallAttemptCount = queue.NurseCallAttemptCount,
                NurseCallExpiresAt = queue.NurseCallExpiresAt,
                DoctorCallAttemptCount = queue.DoctorCallAttemptCount,
                DoctorCallExpiresAt = queue.DoctorCallExpiresAt,
                Message = message
            };
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}