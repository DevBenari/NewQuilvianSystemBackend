using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>Kelas layanan menurut periode kamar; tidak mengubah kelas awal atau hak polis.</summary>
public sealed class InpServiceClassResolver(ApplicationDbContext db)
{
    private const string EmergencyPricingPatientClassName = "RAWAT JALAN";

    public async Task<(Guid? Id, string? Name)> ResolveAsync(
        Guid encounterId, DateTime serviceAt, CancellationToken cancellationToken = default)
    {
        serviceAt = serviceAt.Kind == DateTimeKind.Local ? serviceAt.ToUniversalTime() : DateTime.SpecifyKind(serviceAt, DateTimeKind.Utc);
        var encounter = await db.Set<RegPatientEncounter>().AsNoTracking()
            .Where(x => x.Id == encounterId && !x.IsDelete)
            .Select(x => new { x.EncounterType, x.PatientClassId, Name = x.PatientClass == null ? null : x.PatientClass.PatientClassName })
            .SingleOrDefaultAsync(cancellationToken);
        if (encounter == null) return (null, null);
        if (encounter.EncounterType != EncounterType.Inpatient)
        {
            if (encounter.EncounterType == EncounterType.Emergency && encounter.PatientClassId == null)
                return await ResolveEmergencyPricingClassAsync(cancellationToken);

            return (encounter.PatientClassId, encounter.Name);
        }

        var placement = await db.Set<InpBedPlacement>().AsNoTracking()
            .Where(x => x.Episode!.EncounterId == encounterId && !x.Episode.IsDelete && !x.IsDelete &&
                x.SupersededByCorrectionId == null && x.StartDateTime <= serviceAt &&
                (x.EndDateTime == null || serviceAt < x.EndDateTime))
            .OrderByDescending(x => x.StartDateTime).ThenByDescending(x => x.SequenceNumber).ThenBy(x => x.Id)
            .Select(x => new { x.PatientClassId, Name = x.PatientClass == null ? null : x.PatientClass.PatientClassName })
            .FirstOrDefaultAsync(cancellationToken);
        return placement == null ? (null, null) : (placement.PatientClassId, placement.Name);
    }

    private async Task<(Guid? Id, string? Name)> ResolveEmergencyPricingClassAsync(CancellationToken cancellationToken)
    {
        var activeClasses = await db.Set<MstPatientClass>().AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.PatientClassName })
            .ToListAsync(cancellationToken);

        var matches = activeClasses
            .Where(x => NormalizeLookupText(x.PatientClassName) == EmergencyPricingPatientClassName)
            .ToList();

        return matches.Count == 1 ? (matches[0].Id, matches[0].PatientClassName) : (null, null);
    }

    private static string NormalizeLookupText(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : string.Join(" ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToUpperInvariant();
}
