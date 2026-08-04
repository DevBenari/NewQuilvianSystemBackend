using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi episode resusitasi dan transisi workflow resusitasi IGD.
    /// </summary>
    public class EmergencyResuscitationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyResuscitationService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyResuscitationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyResuscitationStatus), request.ResuscitationStatus))
                return "Nilai ResuscitationStatus tidak valid.";

            if (request.DefibrillationCount < 0)
                return "DefibrillationCount tidak boleh bernilai negatif.";

            if (request.WasCardiopulmonaryResuscitationPerformed &&
                !request.CardiopulmonaryResuscitationStartedAt.HasValue)
                return "CardiopulmonaryResuscitationStartedAt wajib diisi ketika CPR dilakukan.";

            if (request.ReturnOfSpontaneousCirculationAt.HasValue &&
                request.CardiopulmonaryResuscitationStartedAt.HasValue &&
                request.ReturnOfSpontaneousCirculationAt.Value < request.CardiopulmonaryResuscitationStartedAt.Value)
                return "Waktu ROSC tidak boleh lebih awal dari waktu mulai CPR.";

            if (request.CompletedAt.HasValue && request.CompletedAt.Value < request.StartedAt)
                return "CompletedAt tidak boleh lebih awal dari StartedAt.";

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
            UpdateEmergencyResuscitationRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyResuscitationRequest)request, cancellationToken);

        public bool CanTransition(
            EmergencyResuscitationStatus current,
            EmergencyResuscitationStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyResuscitationStatus.Planned => target is EmergencyResuscitationStatus.InProgress
                    or EmergencyResuscitationStatus.Cancelled,
                EmergencyResuscitationStatus.InProgress => target is EmergencyResuscitationStatus.Completed
                    or EmergencyResuscitationStatus.Stopped
                    or EmergencyResuscitationStatus.Cancelled,
                _ => false
            };
        }

        public string GenerateNumber(DateTime now)
            => _documentNumberService.Generate("RES", now);
    }
}
