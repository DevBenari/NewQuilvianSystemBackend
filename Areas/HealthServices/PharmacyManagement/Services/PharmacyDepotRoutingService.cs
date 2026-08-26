using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PharmacyDepotRoutingService
    {
        private const string EmergencyLocationType = "emergency";
        private const string PharmacyLocationType = "pharmacy";

        private readonly ApplicationDbContext _dbContext;

        public PharmacyDepotRoutingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PharmacyDepotRoutingResult> ResolveAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
            {
                return PharmacyDepotRoutingResult.Failure(
                    "PHA_ROUTE_ENCOUNTER_INVALID",
                    "Encounter pasien tidak dapat digunakan untuk menentukan Depo Farmasi.");
            }

            var encounter = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == encounterId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .Select(x => new
                {
                    x.EncounterType,
                    x.ServiceUnitId,
                    x.ClinicId
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (encounter == null)
            {
                return PharmacyDepotRoutingResult.Failure(
                    "PHA_ROUTE_ENCOUNTER_INVALID",
                    "Encounter pasien tidak dapat digunakan untuk menentukan Depo Farmasi.");
            }

            var eligibleLocations = _dbContext.Set<MstDrugStorageLocation>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsPharmacyLocation &&
                    x.IsAllowDispensing &&
                    !x.IsMainWarehouse &&
                    !x.IsQuarantineLocation);

            return encounter.EncounterType switch
            {
                EncounterType.Outpatient => await ResolveOutpatientAsync(
                    eligibleLocations,
                    encounter.ServiceUnitId,
                    encounter.ClinicId,
                    cancellationToken),
                EncounterType.Emergency => await ResolveSingleAsync(
                    eligibleLocations.Where(x =>
                        x.ServiceUnitId == encounter.ServiceUnitId &&
                        x.StorageLocationType.ToLower() == EmergencyLocationType),
                    cancellationToken),
                EncounterType.Inpatient => await ResolveSingleAsync(
                    eligibleLocations.Where(x =>
                        x.ServiceUnitId == encounter.ServiceUnitId &&
                        x.StorageLocationType.ToLower() == PharmacyLocationType),
                    cancellationToken),
                _ => PharmacyDepotRoutingResult.Failure(
                    "PHA_ROUTE_SERVICE_UNSUPPORTED",
                    "Layanan pasien belum memiliki aturan Depo Farmasi.")
            };
        }

        private static async Task<PharmacyDepotRoutingResult> ResolveOutpatientAsync(
            IQueryable<MstDrugStorageLocation> eligibleLocations,
            Guid serviceUnitId,
            Guid? clinicId,
            CancellationToken cancellationToken)
        {
            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
            {
                var clinicResult = await ResolveSingleAsync(
                    eligibleLocations.Where(x => x.ClinicId == clinicId.Value),
                    cancellationToken,
                    allowNotFoundFallback: true);

                if (clinicResult.Code != "PHA_ROUTE_NOT_FOUND")
                {
                    return clinicResult;
                }
            }

            return await ResolveSingleAsync(
                eligibleLocations.Where(x => x.ServiceUnitId == serviceUnitId),
                cancellationToken);
        }

        private static async Task<PharmacyDepotRoutingResult> ResolveSingleAsync(
            IQueryable<MstDrugStorageLocation> candidates,
            CancellationToken cancellationToken,
            bool allowNotFoundFallback = false)
        {
            var candidateIds = await candidates
                .Select(x => x.Id)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (candidateIds.Count == 1)
            {
                return PharmacyDepotRoutingResult.Success(candidateIds[0]);
            }

            if (candidateIds.Count > 1)
            {
                return PharmacyDepotRoutingResult.Failure(
                    "PHA_ROUTE_AMBIGUOUS",
                    "Konfigurasi Depo Farmasi ganda. Hubungi administrator.");
            }

            return PharmacyDepotRoutingResult.Failure(
                "PHA_ROUTE_NOT_FOUND",
                allowNotFoundFallback
                    ? "Depo Farmasi belum ditemukan pada prioritas klinik."
                    : "Depo Farmasi untuk layanan pasien belum dikonfigurasi.");
        }
    }
}
