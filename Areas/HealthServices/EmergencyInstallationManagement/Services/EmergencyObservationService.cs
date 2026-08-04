using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi periode observasi, konsistensi waktu, dan transisi status observasi IGD.
    /// </summary>
    public class EmergencyObservationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyObservationService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyObservationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyObservationStatus), request.ObservationStatus))
                return "Nilai ObservationStatus tidak valid.";

            if (request.EndedAt.HasValue && request.EndedAt.Value < request.StartedAt)
                return "EndedAt tidak boleh lebih awal dari StartedAt.";

            if (request.ObservationStatus == EmergencyObservationStatus.Completed && !request.EndedAt.HasValue)
                return "EndedAt wajib diisi ketika observasi selesai.";

            var visitExists = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            return visitExists
                ? null
                : "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyObservationRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyObservationRequest)request, cancellationToken);

        public bool CanTransition(EmergencyObservationStatus current, EmergencyObservationStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyObservationStatus.Active => target is EmergencyObservationStatus.Completed
                    or EmergencyObservationStatus.Escalated
                    or EmergencyObservationStatus.Cancelled,
                EmergencyObservationStatus.Escalated => target is EmergencyObservationStatus.Completed
                    or EmergencyObservationStatus.Cancelled,
                _ => false
            };
        }

        public string GenerateNumber(DateTime now)
            => _documentNumberService.Generate("OBS", now);
    }
}
