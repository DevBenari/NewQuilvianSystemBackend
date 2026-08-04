using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi triage, retriage, snapshot target waktu ATS/ESI, dan transisi status triage.
    /// </summary>
    public class EmergencyTriageService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmergencyTriageService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyTriageRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.TriageLevelId == Guid.Empty)
                return "TriageLevelId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (!Enum.IsDefined(typeof(EmergencyTriageStatus), request.TriageStatus))
                return "Nilai TriageStatus tidak valid.";

            var visitExists = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            if (!visitExists)
                return "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";

            var level = await _dbContext.Set<MstEmergencyTriageLevel>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.TriageLevelId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (level == null)
                return "TriageLevelId tidak ditemukan atau tidak aktif.";

            if (request.PatientVitalSignId.HasValue &&
                request.PatientVitalSignId.Value != Guid.Empty &&
                !await _dbContext.Set<TrxPatientVitalSign>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.PatientVitalSignId.Value && !x.IsDelete, cancellationToken))
                return "PatientVitalSignId tidak ditemukan.";

            if (request.IsRetriage &&
                (!request.PreviousTriageId.HasValue || request.PreviousTriageId.Value == Guid.Empty))
                return "PreviousTriageId wajib diisi untuk proses retriage.";

            if (request.PreviousTriageId.HasValue && request.PreviousTriageId.Value != Guid.Empty)
            {
                var previous = await _dbContext.Set<TrxEmergencyTriage>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.PreviousTriageId.Value && !x.IsDelete,
                        cancellationToken);

                if (previous == null)
                    return "PreviousTriageId tidak ditemukan.";

                if (previous.EmergencyVisitId != request.EmergencyVisitId)
                    return "PreviousTriageId harus berasal dari kunjungan IGD yang sama.";
            }

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyTriageRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyTriageRequest)request, cancellationToken);

        public async Task<MstEmergencyTriageLevel> GetTriageLevelAsync(
            Guid triageLevelId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<MstEmergencyTriageLevel>()
                .AsNoTracking()
                .FirstAsync(
                    x => x.Id == triageLevelId && !x.IsDelete && x.IsActive,
                    cancellationToken);
        }

        public bool CanTransition(EmergencyTriageStatus current, EmergencyTriageStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyTriageStatus.Draft => target is EmergencyTriageStatus.InProgress
                    or EmergencyTriageStatus.Completed
                    or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.InProgress => target is EmergencyTriageStatus.Completed
                    or EmergencyTriageStatus.Cancelled,
                EmergencyTriageStatus.Completed => target is EmergencyTriageStatus.Superseded,
                _ => false
            };
        }
    }
}
