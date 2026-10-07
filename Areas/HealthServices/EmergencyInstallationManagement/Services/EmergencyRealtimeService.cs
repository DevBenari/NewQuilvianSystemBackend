using Microsoft.AspNetCore.SignalR;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Services.Logging;
using System;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    public class EmergencyRealtimeService
    {
        public const string RealtimeEventName = "EmergencyRealtimeEvent";
        public const string EmergencyQueueGroupName = "emergency:queue";

        private readonly IHubContext<QueueHub> _hubContext;
        private readonly LoggerService _loggerService;

        public EmergencyRealtimeService(
            IHubContext<QueueHub> hubContext,
            LoggerService loggerService)
        {
            _hubContext = hubContext;
            _loggerService = loggerService;
        }

        public async Task NotifyEmergencyPatientRegisteredAsync(
            Guid encounterId,
            Guid patientId,
            Guid serviceUnitId,
            Guid actorUserId,
            string? message = null)
        {
            await NotifyEmergencyQueueChangedAsync(
                eventType: "EmergencyPatientRegistered",
                encounterId: encounterId,
                visitId: null,
                patientId: patientId,
                serviceUnitId: serviceUnitId,
                visitStatus: null,
                registrationStatus: "Registered",
                actorUserId: actorUserId,
                message: message
            );
        }

        public async Task NotifyEmergencyVisitChangedAsync(
            string eventType,
            EmgVisit visit,
            Guid actorUserId,
            string? message = null)
        {
            await NotifyEmergencyQueueChangedAsync(
                eventType: eventType,
                encounterId: visit.EncounterId,
                visitId: visit.Id,
                patientId: visit.PatientId,
                serviceUnitId: visit.ServiceUnitId,
                visitStatus: visit.VisitStatus.ToString(),
                registrationStatus: visit.RegistrationStatus.ToString(),
                actorUserId: actorUserId,
                message: message
            );
        }

        public async Task NotifyEmergencyTriageChangedAsync(
            string eventType,
            EmgTriage triage,
            Guid actorUserId,
            string? message = null)
        {
            await NotifyEmergencyQueueChangedAsync(
                eventType: eventType,
                encounterId: null,
                visitId: triage.EmergencyVisitId,
                patientId: null,
                serviceUnitId: null,
                visitStatus: null,
                registrationStatus: null,
                actorUserId: actorUserId,
                message: message
            );
        }

        public async Task NotifyEmergencyQueueChangedAsync(
            string eventType,
            Guid? encounterId,
            Guid? visitId,
            Guid? patientId,
            Guid? serviceUnitId,
            string? visitStatus,
            string? registrationStatus,
            Guid actorUserId,
            string? message = null)
        {
            try
            {
                var payload = new EmergencyRealtimeEventResponse
                {
                    EventType = eventType,
                    EncounterId = encounterId,
                    VisitId = visitId,
                    PatientId = patientId,
                    ServiceUnitId = serviceUnitId,
                    VisitStatus = visitStatus,
                    RegistrationStatus = registrationStatus,
                    ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                    OccurredAt = DateTime.UtcNow,
                    Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim()
                };

                await _hubContext.Clients
                    .Group(EmergencyQueueGroupName)
                    .SendAsync(RealtimeEventName, payload);
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    "HealthServices.EmergencyInstallation.Realtime",
                    "EmergencyRealtime.NotifyEmergencyQueueChanged",
                    "Gagal mengirim event realtime IGD.",
                    ex
                );
            }
        }
    }
}
