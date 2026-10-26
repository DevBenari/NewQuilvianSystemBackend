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

        /// <summary>
        /// Event ringan untuk klinik lain dalam cluster yang sama: kunci panggilan dokter berubah.
        /// Menggantikan polling <c>GET doctor-queues/call-lock</c> di frontend.
        /// </summary>
        public const string DoctorCallLockChangedEventType = "DoctorCallLockChanged";

        /// <summary>Event dokter yang dapat membuat, memperpanjang, atau melepas kunci panggilan.</summary>
        private static readonly HashSet<string> DoctorCallLockEventTypes = new(StringComparer.Ordinal)
        {
            "QueueCalledByDoctor",
            "QueueConsultationStarted",
            "QueueConsultationFinished",
            "QueueSkippedByDoctor",
            "QueueNoShowByDoctor",
            "QueueRequeuedToDoctor",
            "QueueCancelled"
        };

        /// <summary>Event untuk layar display publik, terpisah dari event ruang kerja (RJ-PRI-DEC-025).</summary>
        public const string QueueDisplayEventName = "QueueDisplayChanged";

        private const string QueueDisplayClusterGroupPrefix = "queue-display-cluster";
        private const string NurseStationClusterGroupPrefix = "nurse-station-cluster";
        private const string DoctorQueueDoctorGroupPrefix = "doctor-queue-doctor";
        private const string DoctorQueueClinicGroupPrefix = "doctor-queue-clinic";
        private const string NurseStationAllGroupName = "nurse-station:all";
        private const string DoctorQueueAllGroupName = "doctor-queue:all";

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

        /// <summary>Group layar display publik per nurse station cluster (RJ-PRI-DEC-025).</summary>
        public static string BuildQueueDisplayClusterGroupName(Guid nurseStationClusterId)
        {
            return $"{QueueDisplayClusterGroupPrefix}:{nurseStationClusterId:D}";
        }

        /// <summary>Event daftar Reservasi Prioritas di layar admisi (RJ-PRI-DEC-014, 026).</summary>
        public const string PriorityReservationEventName = "PriorityReservationChanged";

        /// <summary>Satu group untuk semua pemegang <c>OutpatientPriorityReservation : Read</c>.</summary>
        public const string PriorityReservationBoardGroupName = "priority-reservation-board";

        /// <summary>
        /// Mengirim kabar perubahan reservasi tanpa data pribadi (INT-RP-07). Bersifat upaya terbaik:
        /// kegagalan kirim hanya dicatat, karena layar memuat ulang daftar saat dibuka atau ditekan ↻.
        /// </summary>
        public async Task NotifyPriorityReservationChangedAsync(
            RegPriorityReservation reservation,
            string eventCode,
            DateTime occurredAtUtc,
            DateTime? previousVisitDate = null)
        {
            var payload = new PriorityReservationRealtimeEventResponse
            {
                ReservationId = reservation.Id,
                VisitDate = reservation.VisitDate,
                PreviousVisitDate = previousVisitDate,
                Status = reservation.Status,
                EventCode = eventCode,
                OccurredAt = occurredAtUtc
            };

            try
            {
                await _hubContext.Clients
                    .Group(PriorityReservationBoardGroupName)
                    .SendAsync(PriorityReservationEventName, payload);
            }
            catch (Exception exception)
            {
                // Serilog langsung, bukan LoggerService: LoggerService menyertakan path permintaan, dan
                // path link publik memuat kode konfirmasi (INV-RP-05).
                Serilog.Log.Error(
                    exception,
                    "Kabar realtime reservasi gagal dikirim. EntityId={EntityId}; Event={EventCode}.",
                    reservation.Id,
                    eventCode);
            }
        }

        public static string BuildNurseStationAllGroupName()
        {
            return NurseStationAllGroupName;
        }

        public static string BuildDoctorQueueAllGroupName()
        {
            return DoctorQueueAllGroupName;
        }

        public static string BuildDoctorQueueDoctorGroupName(Guid doctorId)
        {
            return $"{DoctorQueueDoctorGroupPrefix}:{doctorId:D}";
        }

        public static string BuildDoctorQueueClinicGroupName(Guid clinicId)
        {
            return $"{DoctorQueueClinicGroupPrefix}:{clinicId:D}";
        }

        public Task NotifyQueueCreatedAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            // Display antrian bekerja berdasarkan nurse station cluster.
            // Karena itu event pembuatan antrean tetap dikirim ke group cluster,
            // termasuk untuk antrean yang langsung masuk dokter tanpa screening.
            var notifyDoctorQueue = queue.IsDoctorRequired || IsDoctorQueueStatus(queue.QueueStatus);

            return NotifyQueueChangedAsync(
                eventType: "QueueCreated",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: notifyDoctorQueue
            );
        }

        public Task NotifyQueueCalledByNurseAsync(RegQueue queue, Guid actorUserId, string? message = null)
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

        public Task NotifyQueueScreeningStartedAsync(RegQueue queue, Guid actorUserId, string? message = null)
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

        public Task NotifyQueueScreeningFinishedAsync(RegQueue queue, Guid actorUserId, string? message = null)
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

        public Task NotifyQueueSkippedByNurseAsync(RegQueue queue, Guid actorUserId, string? message = null)
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

        public Task NotifyQueueNoShowByNurseAsync(RegQueue queue, Guid actorUserId, string? message = null)
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

        public Task NotifyQueueCalledByDoctorAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueCalledByDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueConsultationStartedAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueConsultationStarted",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueConsultationFinishedAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueConsultationFinished",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueSkippedByDoctorAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueSkippedByDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueRequeuedToDoctorAsync(RegQueue queue, Guid actorUserId, string? message = null)
        {
            return NotifyQueueChangedAsync(
                eventType: "QueueRequeuedToDoctor",
                queue: queue,
                actorUserId: actorUserId,
                message: message,
                notifyNurseStation: true,
                notifyDoctorQueue: true
            );
        }

        public Task NotifyQueueCancelledAsync(RegQueue queue, Guid actorUserId, string? message = null)
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
            RegQueue queue,
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
                    sendTasks.Add(_hubContext.Clients
                        .Group(BuildNurseStationAllGroupName())
                        .SendAsync(RealtimeEventName, payload));

                    foreach (var nurseStationClusterId in nurseStationClusterIds)
                    {
                        sendTasks.Add(_hubContext.Clients
                            .Group(BuildNurseStationClusterGroupName(nurseStationClusterId))
                            .SendAsync(RealtimeEventName, payload));
                    }

                    // RJ-PRI-DEC-005/025: layar display publik hanya menerima payload ringkas, dan
                    // tidak menerima kabar antrean prioritas sama sekali.
                    if (!queue.IsPriorityQueue)
                    {
                        var displayPayload = BuildDisplayPayload(payload);

                        foreach (var nurseStationClusterId in nurseStationClusterIds)
                        {
                            sendTasks.Add(_hubContext.Clients
                                .Group(BuildQueueDisplayClusterGroupName(nurseStationClusterId))
                                .SendAsync(QueueDisplayEventName, displayPayload));
                        }
                    }
                }

                if (notifyDoctorQueue)
                {
                    sendTasks.Add(_hubContext.Clients
                        .Group(BuildDoctorQueueAllGroupName())
                        .SendAsync(RealtimeEventName, payload));

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

                    if (DoctorCallLockEventTypes.Contains(eventType))
                    {
                        sendTasks.AddRange(await BuildClusterCallLockNotificationsAsync(queue, nurseStationClusterIds));
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

        /// <summary>
        /// Kunci panggilan dokter berlaku per nurse station cluster, sedangkan dokter hanya
        /// bergabung ke grup klinik tempat ia berjadwal. Dokter di klinik lain dalam cluster yang
        /// sama karena itu diberi event ringan tanpa data pasien, cukup untuk membaca ulang
        /// kunci. Klinik antrean itu sendiri sudah menerima event lengkap.
        /// </summary>
        private async Task<List<Task>> BuildClusterCallLockNotificationsAsync(
            RegQueue queue,
            List<Guid> nurseStationClusterIds)
        {
            if (nurseStationClusterIds.Count == 0)
            {
                return new List<Task>();
            }

            var siblingClinicIds = await _dbContext.Set<MstNurseStationClusterClinic>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    nurseStationClusterIds.Contains(x.NurseStationClusterId) &&
                    x.ClinicId != queue.ClinicId)
                .Select(x => x.ClinicId)
                .Distinct()
                .ToListAsync();

            var lockPayload = new QueueRealtimeEventResponse
            {
                EventType = DoctorCallLockChangedEventType,
                ClinicId = queue.ClinicId,
                NurseStationClusterIds = nurseStationClusterIds,
                QueueDate = queue.QueueDate,
                DoctorCallExpiresAt = queue.DoctorCallExpiresAt,
                OccurredAt = DateTime.UtcNow
            };

            return siblingClinicIds
                .Select(clinicId => _hubContext.Clients
                    .Group(BuildDoctorQueueClinicGroupName(clinicId))
                    .SendAsync(RealtimeEventName, lockPayload))
                .ToList();
        }

        private async Task<List<Guid>> ResolveNurseStationClusterIdsAsync(RegQueue queue)
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
            RegQueue queue,
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
                QueueAudience = queue.QueueAudienceSnapshot,
                NurseCallExpiresAt = queue.NurseCallExpiresAt,
                DoctorCallExpiresAt = queue.DoctorCallExpiresAt,
                ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                OccurredAt = DateTime.UtcNow,
                Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim()
            };
        }

        private static QueueDisplayRealtimeEventResponse BuildDisplayPayload(QueueRealtimeEventResponse payload)
        {
            return new QueueDisplayRealtimeEventResponse
            {
                EventType = payload.EventType,
                QueueId = payload.QueueId,
                ServiceUnitId = payload.ServiceUnitId,
                ClinicId = payload.ClinicId,
                DoctorId = payload.DoctorId,
                NurseStationClusterIds = payload.NurseStationClusterIds,
                QueueDate = payload.QueueDate,
                QueueNumber = payload.QueueNumber,
                QueueCode = payload.QueueCode,
                QueueStatus = payload.QueueStatus,
                QueueStatusName = payload.QueueStatusName,
                NurseCallExpiresAt = payload.NurseCallExpiresAt,
                DoctorCallExpiresAt = payload.DoctorCallExpiresAt,
                OccurredAt = payload.OccurredAt
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
