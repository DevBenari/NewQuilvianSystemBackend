using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    public class QueueRealtimeService
    {
        public const string RealtimeEventName = "QueueRealtimeEvent";

        private const string NurseStationClusterGroupPrefix = "nurse-station-cluster";
        private const string DoctorQueueDoctorGroupPrefix = "doctor-queue-doctor";
        private const string DoctorQueueClinicGroupPrefix = "doctor-queue-clinic";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly LoggerService _loggerService;

        public QueueRealtimeService(
            ApplicationDbContext dbContext,
            IHubContext<QueueHub> hubContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _loggerService = loggerService;
        }

        public static string BuildNurseStationClusterGroupName(Guid nurseStationClusterId)
        {
            return $"{NurseStationClusterGroupPrefix}:{nurseStationClusterId:D}";
        }

        public static string BuildDoctorQueueDoctorGroupName(Guid doctorId)
        {
            return $"{DoctorQueueDoctorGroupPrefix}:{doctorId:D}";
        }

        public static string BuildDoctorQueueClinicGroupName(Guid clinicId)
        {
            return $"{DoctorQueueClinicGroupPrefix}:{clinicId:D}";
        }

        public Task NotifyQueueCreatedAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            var notifyNurseStation = IsNurseStationQueueStatus(queue.QueueStatus);
            var notifyDoctorQueue = IsDoctorQueueStatus(queue.QueueStatus);

            return NotifyQueueChangedAsync(
                eventType: "QueueCreated",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: notifyNurseStation,
                notifyDoctorQueue: notifyDoctorQueue
            );
        }

        public Task NotifyQueueCalledByNurseAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueCalledByNurse",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: false
            );
        }

        public Task NotifyQueueScreeningStartedAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueScreeningStarted",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: false
            );
        }

        public Task NotifyQueueScreeningFinishedAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueScreeningFinished",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: queue.IsDoctorRequired || IsDoctorQueueStatus(queue.QueueStatus)
            );
        }

        public Task NotifyQueueSkippedByNurseAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueSkippedByNurse",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: false
            );
        }

        public Task NotifyQueueNoShowByNurseAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueNoShowByNurse",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: false
            );
        }

        public Task NotifyQueueCalledByDoctorAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueCalledByDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: false,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueConsultationStartedAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueConsultationStarted",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: false,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueConsultationFinishedAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueConsultationFinished",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: false,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueSkippedByDoctorAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueSkippedByDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: false,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueRequeuedToDoctorAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueRequeuedToDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: false,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueCancelledAsync(TrxQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueCancelled",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public async Task NotifyQueueChangedAsync(
            string eventType,
            TrxQueue queue,
            Guid actorUserId,
            string? message = null,
            bool notifyNurseStation = true,
            bool notifyDoctorQueue = true)
        {
            try
            {
                var nurseStationClusterIds = await ResolveNurseStationClusterIdsAsync(queue);
                var payload = BuildPayload(eventType, queue, nurseStationClusterIds, actorUserId, message);
                var sendTasks = new List<Task>();

                if (notifyNurseStation)
                {
                    foreach (var nurseStationClusterId in nurseStationClusterIds)
                    {
                        sendTasks.Add(_hubContext.Clients
                            .Group(BuildNurseStationClusterGroupName(nurseStationClusterId))
                            .SendAsync(RealtimeEventName, payload));
                    }
                }

                if (notifyDoctorQueue)
                {
                    if (queue.DoctorId.HasValue && queue.DoctorId.Value != Guid.Empty)
                    {
                        sendTasks.Add(_hubContext.Clients
                            .Group(BuildDoctorQueueDoctorGroupName(queue.DoctorId.Value))
                            .SendAsync(RealtimeEventName, payload));
                    }

                    if (queue.ClinicId.HasValue && queue.ClinicId.Value != Guid.Empty)
                    {
                        sendTasks.Add(_hubContext.Clients
                            .Group(BuildDoctorQueueClinicGroupName(queue.ClinicId.Value))
                            .SendAsync(RealtimeEventName, payload));
                    }
                }

                if (sendTasks.Count > 0)
                {
                    await Task.WhenAll(sendTasks);
                }
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    "HealthServices.RegistrationManagement.Realtime",
                    "QueueRealtime.NotifyQueueChanged",
                    "Gagal mengirim event realtime antrean.",
                    ex
                );
            }
        }

        private async Task<List<Guid>> ResolveNurseStationClusterIdsAsync(TrxQueue queue)
        {
            if (!queue.ClinicId.HasValue || queue.ClinicId.Value == Guid.Empty)
            {
                return new List<Guid>();
            }

            return await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ClinicId == queue.ClinicId.Value)
                .Select(x => x.NurseStationClusterId)
                .Distinct()
                .ToListAsync();
        }

        private static QueueRealtimeEventResponse BuildPayload(
            string eventType,
            TrxQueue queue,
            List<Guid> nurseStationClusterIds,
            Guid actorUserId,
            string? message)
        {
            return new QueueRealtimeEventResponse
            {
                EventType = eventType,
                QueueId = queue.Id,
                EncounterId = queue.EncounterId,
                PatientId = queue.PatientId,
                ServiceUnitId = queue.ServiceUnitId,
                ClinicId = queue.ClinicId,
                DoctorId = queue.DoctorId,
                NurseStationClusterIds = nurseStationClusterIds,
                QueueDate = queue.QueueDate,
                QueueNumber = queue.QueueNumber,
                QueueCode = queue.QueueCode,
                QueueStatus = queue.QueueStatus,
                QueueStatusName = queue.QueueStatus.ToString(),
                IsScreeningRequired = queue.IsScreeningRequired,
                IsDoctorRequired = queue.IsDoctorRequired,
                IsPriorityQueue = queue.IsPriorityQueue,
                NurseCallExpiresAt = queue.NurseCallExpiresAt,
                DoctorCallExpiresAt = queue.DoctorCallExpiresAt,
                ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                OccurredAt = DateTime.UtcNow,
                Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim()
            };
        }

        private static bool IsNurseStationQueueStatus(QueueStatus status)
        {
            return status == QueueStatus.WaitingForNurse ||
                   status == QueueStatus.CalledByNurse ||
                   status == QueueStatus.InNurseScreening;
        }

        private static bool IsDoctorQueueStatus(QueueStatus status)
        {
            return status == QueueStatus.WaitingForDoctor ||
                   status == QueueStatus.CalledByDoctor ||
                   status == QueueStatus.InConsultation ||
                   status == QueueStatus.Skipped ||
                   status == QueueStatus.Completed;
        }
    }
}
