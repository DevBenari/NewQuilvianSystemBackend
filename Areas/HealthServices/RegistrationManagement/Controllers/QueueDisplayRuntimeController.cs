using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/queue-display-runtime")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Queue Display Runtime",
        AreaName = "HealthServices",
        ControllerName = "QueueDisplayRuntime",
        Description = "Runtime display antrean berdasarkan device display dan cluster poli",
        SortOrder = 6
    )]
    [Tags("Health Services / Registration Management / Queue Display Runtime")]
    public class QueueDisplayRuntimeController : ControllerBase
    {
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public QueueDisplayRuntimeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayRuntimeCurrentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Queue Display Runtime", Description = "Melihat informasi display antrian login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayRuntime", "Read")]
        public async Task<IActionResult> GetCurrent()
        {
            var device = await ResolveCurrentDisplayDeviceAsync();
            if (device == null) return DisplayDeviceNotFound();

            device.LastOnlineDateTime = DateTime.UtcNow;
            device.LastErrorMessage = null;
            await _dbContext.SaveChangesAsync();

            var result = new QueueDisplayRuntimeCurrentResponse
            {
                DisplayDeviceId = device.Id,
                DisplayCode = device.DisplayCode,
                DisplayName = device.DisplayName,
                NurseStationClusterId = device.NurseStationClusterId,
                NurseStationClusterName = device.NurseStationCluster?.ClusterName ?? string.Empty,
                ServiceUnitId = device.ServiceUnitId,
                ServiceUnitName = device.ServiceUnit?.ServiceUnitName,
                LocationName = device.LocationName,
                FloorName = device.FloorName,
                RoomName = device.RoomName,
                EnableVoiceCalling = device.EnableVoiceCalling,
                ShowPatientName = device.ShowPatientName,
                ShowDoctorName = device.ShowDoctorName,
                ShowClinicName = device.ShowClinicName,
                RefreshIntervalSeconds = device.RefreshIntervalSeconds,
                ServerDateTime = DateTime.UtcNow
            };

            return Ok(ApiResponse<QueueDisplayRuntimeCurrentResponse>.Ok(result, "Informasi display antrian berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayRuntimeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Runtime", Description = "Melihat ringkasan display antrian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayRuntime", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? queueDate)
        {
            var device = await ResolveCurrentDisplayDeviceAsync();
            if (device == null) return DisplayDeviceNotFound();

            var clinicIds = await GetClinicIdsByClusterIdAsync(device.NurseStationClusterId);
            var query = BuildDisplayQueueQuery(queueDate, clinicIds, device.ServiceUnitId);

            var result = new QueueDisplayRuntimeSummaryResponse
            {
                TotalQueue = await query.CountAsync(),
                WaitingForNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForNurse),
                CalledByNurseQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.CalledByNurse),
                InNurseScreeningQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.InNurseScreening),
                WaitingForDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.WaitingForDoctor),
                CalledByDoctorQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.CalledByDoctor),
                InConsultationQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.InConsultation),
                CompletedQueue = await query.CountAsync(x => x.QueueStatus == QueueStatus.Completed || x.CompletedAt.HasValue)
            };

            return Ok(ApiResponse<QueueDisplayRuntimeSummaryResponse>.Ok(result, "Ringkasan display antrian berhasil diambil."));
        }

        [HttpGet("items")]
        [ProducesResponseType(typeof(ApiResponse<List<QueueDisplayRuntimeItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Runtime", Description = "Melihat item antrian untuk display", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayRuntime", "Read")]
        public async Task<IActionResult> GetItems(
            [FromQuery] DateTime? queueDate,
            [FromQuery] QueueStatus? queueStatus,
            [FromQuery] int take = 20)
        {
            var device = await ResolveCurrentDisplayDeviceAsync();
            if (device == null) return DisplayDeviceNotFound();

            take = take < 1 ? 20 : Math.Min(take, 100);

            var clinicIds = await GetClinicIdsByClusterIdAsync(device.NurseStationClusterId);
            var query = BuildDisplayQueueQuery(queueDate, clinicIds, device.ServiceUnitId);

            if (queueStatus.HasValue)
            {
                query = query.Where(x => x.QueueStatus == queueStatus.Value);
            }
            else
            {
                query = query.Where(x =>
                    x.QueueStatus == QueueStatus.WaitingForNurse ||
                    x.QueueStatus == QueueStatus.CalledByNurse ||
                    x.QueueStatus == QueueStatus.InNurseScreening ||
                    x.QueueStatus == QueueStatus.WaitingForDoctor ||
                    x.QueueStatus == QueueStatus.CalledByDoctor ||
                    x.QueueStatus == QueueStatus.InConsultation);
            }

            var items = await query
                .OrderByDescending(x => x.QueueStatus == QueueStatus.CalledByNurse || x.QueueStatus == QueueStatus.CalledByDoctor)
                .ThenByDescending(x => x.IsPriorityQueue)
                .ThenBy(x => x.QueueNumber)
                .Take(take)
                .Select(x => MapItemResponse(x, device))
                .ToListAsync();

            return Ok(ApiResponse<List<QueueDisplayRuntimeItemResponse>>.Ok(items, "Data item display antrian berhasil diambil."));
        }

        [HttpGet("called")]
        [ProducesResponseType(typeof(ApiResponse<QueueDisplayRuntimeCalledResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Display Runtime", Description = "Melihat panggilan antrean terakhir", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueDisplayRuntime", "Read")]
        public async Task<IActionResult> GetCalled([FromQuery] DateTime? queueDate)
        {
            var device = await ResolveCurrentDisplayDeviceAsync();
            if (device == null) return DisplayDeviceNotFound();

            var clinicIds = await GetClinicIdsByClusterIdAsync(device.NurseStationClusterId);
            var query = BuildDisplayQueueQuery(queueDate, clinicIds, device.ServiceUnitId)
                .Where(x => x.QueueStatus == QueueStatus.CalledByNurse || x.QueueStatus == QueueStatus.CalledByDoctor);

            var entity = await query
                .OrderByDescending(x => x.LastDoctorCalledAt ?? x.LastNurseCalledAt ?? x.CreateDateTime)
                .FirstOrDefaultAsync();

            var result = entity == null
                ? new QueueDisplayRuntimeCalledResponse()
                : new QueueDisplayRuntimeCalledResponse
                {
                    QueueId = entity.Id,
                    QueueCode = entity.QueueCode,
                    QueueStatus = entity.QueueStatus,
                    QueueStatusName = entity.QueueStatus.ToString(),
                    ClinicName = entity.Clinic != null ? entity.Clinic.ClinicName : null,
                    DoctorName = device.ShowDoctorName && entity.Doctor != null ? entity.Doctor.FullName : null,
                    CalledAt = entity.LastDoctorCalledAt ?? entity.LastNurseCalledAt,
                    DisplayText = BuildDisplayText(entity, device),
                    VoiceText = device.EnableVoiceCalling ? BuildVoiceText(entity, device) : null
                };

            return Ok(ApiResponse<QueueDisplayRuntimeCalledResponse>.Ok(result, "Data panggilan antrean terakhir berhasil diambil."));
        }

        private IQueryable<TrxQueue> BuildDisplayQueueQuery(DateTime? queueDate, List<Guid> clinicIds, Guid? serviceUnitId)
        {
            var selectedDate = queueDate?.Date ?? DateTime.UtcNow.Date;
            var query = _dbContext.Set<TrxQueue>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.QueueDate.Date == selectedDate && x.ClinicId.HasValue && clinicIds.Contains(x.ClinicId.Value));

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            }

            return query;
        }

        private async Task<MstQueueDisplayDevice?> ResolveCurrentDisplayDeviceAsync()
        {
            var displayDeviceIdClaim = User.FindFirstValue("display_device_id") ?? User.FindFirstValue("DisplayDeviceId");
            if (Guid.TryParse(displayDeviceIdClaim, out var displayDeviceId))
            {
                return await _dbContext.Set<MstQueueDisplayDevice>()
                    .Include(x => x.NurseStationCluster)
                    .Include(x => x.ServiceUnit)
                    .FirstOrDefaultAsync(x => x.Id == displayDeviceId && !x.IsDelete && x.IsActive);
            }

            var displayCodeClaim = User.FindFirstValue("display_code") ?? User.FindFirstValue("DisplayCode") ?? User.FindFirstValue("user_code") ?? User.FindFirstValue("UserCode");
            if (!string.IsNullOrWhiteSpace(displayCodeClaim))
            {
                var displayCode = displayCodeClaim.Trim().ToUpper();
                var deviceByClaim = await _dbContext.Set<MstQueueDisplayDevice>()
                    .Include(x => x.NurseStationCluster)
                    .Include(x => x.ServiceUnit)
                    .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.DisplayCode.ToUpper() == displayCode);

                if (deviceByClaim != null) return deviceByClaim;
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
            if (currentUser == null) return null;

            var userCode = (currentUser.UserCode ?? currentUser.UserName ?? string.Empty).Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(userCode)) return null;

            return await _dbContext.Set<MstQueueDisplayDevice>()
                .Include(x => x.NurseStationCluster)
                .Include(x => x.ServiceUnit)
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.DisplayCode.ToUpper() == userCode);
        }

        private async Task<List<Guid>> GetClinicIdsByClusterIdAsync(Guid clusterId)
        {
            return await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.NurseStationClusterId == clusterId)
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync();
        }

        private static QueueDisplayRuntimeItemResponse MapItemResponse(TrxQueue x, MstQueueDisplayDevice device)
        {
            var patientName = x.Patient != null ? x.Patient.FullName : string.Empty;
            return new QueueDisplayRuntimeItemResponse
            {
                QueueId = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = device.ShowPatientName ? patientName : string.Empty,
                MaskedPatientName = MaskPatientName(patientName),
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = device.ShowClinicName && x.Clinic != null ? x.Clinic.ClinicName : null,
                DoctorId = x.DoctorId,
                DoctorName = device.ShowDoctorName && x.Doctor != null ? x.Doctor.FullName : null,
                QueueDate = x.QueueDate,
                QueueNumber = x.QueueNumber,
                QueueCode = x.QueueCode,
                QueueStatus = x.QueueStatus,
                QueueStatusName = x.QueueStatus.ToString(),
                IsPriorityQueue = x.IsPriorityQueue,
                LastNurseCalledAt = x.LastNurseCalledAt,
                LastDoctorCalledAt = x.LastDoctorCalledAt,
                NurseCallExpiresAt = x.NurseCallExpiresAt,
                DoctorCallExpiresAt = x.DoctorCallExpiresAt,
                DisplayText = BuildDisplayText(x, device),
                VoiceText = device.EnableVoiceCalling ? BuildVoiceText(x, device) : null
            };
        }

        private static string BuildDisplayText(TrxQueue queue, MstQueueDisplayDevice device)
        {
            var destination = queue.QueueStatus switch
            {
                QueueStatus.CalledByNurse => "silakan ke nurse station",
                QueueStatus.CalledByDoctor => device.ShowDoctorName && queue.Doctor != null ? $"silakan ke {queue.Doctor.FullName}" : "silakan ke dokter",
                QueueStatus.InNurseScreening => "sedang screening",
                QueueStatus.WaitingForDoctor => "menunggu dokter",
                QueueStatus.InConsultation => "sedang diperiksa dokter",
                _ => "menunggu panggilan"
            };

            var clinic = device.ShowClinicName && queue.Clinic != null ? $" - {queue.Clinic.ClinicName}" : string.Empty;
            return $"{queue.QueueCode}{clinic} {destination}".Trim();
        }

        private static string BuildVoiceText(TrxQueue queue, MstQueueDisplayDevice device)
        {
            var destination = queue.QueueStatus switch
            {
                QueueStatus.CalledByNurse => "silakan menuju nurse station",
                QueueStatus.CalledByDoctor => device.ShowDoctorName && queue.Doctor != null ? $"silakan menuju ruang dokter {queue.Doctor.FullName}" : "silakan menuju ruang dokter",
                _ => "harap menunggu panggilan"
            };

            var clinic = device.ShowClinicName && queue.Clinic != null ? $", {queue.Clinic.ClinicName}" : string.Empty;
            return $"Nomor antrean {queue.QueueCode}{clinic}, {destination}.";
        }

        private static string MaskPatientName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(MaskWord));
        }

        private static string MaskWord(string word)
        {
            if (word.Length <= 1) return word;
            if (word.Length == 2) return word[0] + "*";
            return word[0] + new string('*', Math.Min(word.Length - 1, 5));
        }

        private IActionResult DisplayDeviceNotFound()
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Display antrian tidak ditemukan atau tidak aktif."));
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}
