using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Aturan validasi setting operasional IGD dan pemeliharaan satu setting default aktif.
    /// </summary>
    public class EmergencySettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmergencySettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencySettingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return "Code wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name wajib diisi.";

            if (request.DefaultEmergencyServiceUnitId == Guid.Empty)
                return "DefaultEmergencyServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTriageSystem), request.TriageSystem))
                return "Nilai TriageSystem tidak valid.";

            if (string.IsNullOrWhiteSpace(request.TemporaryPatientNumberPrefix))
                return "TemporaryPatientNumberPrefix wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.EmergencyVisitNumberPrefix))
                return "EmergencyVisitNumberPrefix wajib diisi.";

            if (request.ImmediateCareLevelThreshold is < 1 or > 5)
                return "ImmediateCareLevelThreshold harus berada pada level 1 sampai 5.";

            if (request.RequireRegistrationBeforeTreatmentFromLevel is < 1 or > 5)
                return "RequireRegistrationBeforeTreatmentFromLevel harus berada pada level 1 sampai 5.";

            if (request.RequireRegistrationBeforeTreatmentFromLevel <= request.ImmediateCareLevelThreshold)
                return "RequireRegistrationBeforeTreatmentFromLevel harus lebih besar dari ImmediateCareLevelThreshold.";

            var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.DefaultEmergencyServiceUnitId && !x.IsDelete,
                    cancellationToken);

            return serviceUnitExists
                ? null
                : "DefaultEmergencyServiceUnitId tidak ditemukan.";
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencySettingRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencySettingRequest)request, cancellationToken);

        public async Task ClearOtherDefaultsAsync(
            Guid? exceptId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<MstEmergencySetting>()
                .Where(x => x.IsDefault && !x.IsDelete);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var existingDefaults = await query.ToListAsync(cancellationToken);
            foreach (var setting in existingDefaults)
            {
                setting.IsDefault = false;
                setting.UpdateDateTime = now;
                setting.UpdateBy = actorUserId;
            }
        }
    }
}
