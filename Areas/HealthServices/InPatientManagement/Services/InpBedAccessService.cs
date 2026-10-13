using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedAccessService(
    ApplicationDbContext db, AccessPermissionService permissions,
    IHttpContextAccessor http, IOptionsMonitor<BedManagementOptions> options)
{
    public Guid ActorId => http.HttpContext?.User.GetUserId() ?? Guid.Empty;
    private IEnumerable<BedUnitAssignment> Assignments => options.CurrentValue.UnitAssignments
        .Where(x => x.UserId == ActorId && x.ServiceUnitId != Guid.Empty && !string.IsNullOrWhiteSpace(x.AssignmentReference));
    public async Task<bool> HasPermissionAsync(string operation)
    {
        var parts = operation.Split(':');
        return parts.Length == 2 && http.HttpContext != null &&
            await permissions.HasAccessAsync(http.HttpContext.User, parts[0], parts[1]);
    }
    public async Task<List<Guid>> UnitsAsync(string operation, CancellationToken ct = default)
    {
        if (ActorId == Guid.Empty || !await db.Users.AnyAsync(x => x.Id == ActorId && x.IsActive, ct) ||
            !await HasPermissionAsync(operation)) return [];
        var ids = Assignments.Where(x => x.Operations.Contains(operation) &&
            (!x.OperationalOnly || operation is "InpatientBedManagement:Read" or "InpatientBedManagement:StartCleaning" or "InpatientBedManagement:CompleteCleaning" or "InpatientBedOccupancy:Read"))
            .Select(x => x.ServiceUnitId).Distinct().ToList();
        return await db.Set<MstServiceUnit>().Where(x => ids.Contains(x.Id) && !x.IsDelete).Select(x => x.Id).ToListAsync(ct);
    }
    public async Task RequireBedAsync(Guid bedId, string operation, CancellationToken ct = default)
    {
        var units = await UnitsAsync(operation, ct);
        if (!await db.Set<MstBed>().AnyAsync(x => x.Id == bedId && x.Room != null && units.Contains(x.Room.ServiceUnitId), ct))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Tidak berwenang untuk bed atau unit ini.");
    }
    public async Task RequireEpisodeAsync(Guid episodeId, string operation, CancellationToken ct = default)
    {
        var units = await UnitsAsync(operation, ct);
        var episode = await db.Set<InpEpisode>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, ct);
        if (episode == null || !units.Contains(episode.ServiceUnitId) ||
            Assignments.Any(x => x.ServiceUnitId == episode.ServiceUnitId && x.OperationalOnly) ||
            !await HasPermissionAsync("InpatientEpisode:Read"))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Tidak berwenang membaca episode ini.");
    }
    public async Task<bool> CanReadIdentityAsync(Guid episodeId, Guid unitId, CancellationToken ct = default)
    {
        if (!Assignments.Any(x => x.ServiceUnitId == unitId) || Assignments.Any(x => x.ServiceUnitId == unitId && x.OperationalOnly) ||
            !await HasPermissionAsync("InpatientEpisode:Read")) return false;
        var units = await UnitsAsync("InpatientBedOccupancy:Read", ct);
        return await db.Set<InpEpisode>().AnyAsync(x => x.Id == episodeId && !x.IsDelete && units.Contains(x.ServiceUnitId), ct);
    }
    public async Task RequireMasterResultAsync(string kind, Guid id, string operation, CancellationToken ct)
    {
        var units = await UnitsAsync(operation, ct);
        var allowed = kind switch
        {
            "Room" => await db.Set<MstRoom>().AnyAsync(x => x.Id == id && units.Contains(x.ServiceUnitId), ct),
            "ServiceUnit" => units.Contains(id),
            "PatientClass" => units.Count > 0 && await db.Set<MstPatientClass>().AnyAsync(x => x.Id == id, ct),
            _ => false
        };
        if (!allowed) throw new InpBedOperationException(403, "ACCESS_DENIED", "Scope hasil master tidak dapat diverifikasi.");
    }
    public BedUnitAssignment WorkflowAssignment(Guid unitId, string operation)
    {
        if (!options.CurrentValue.EnableWorkflow)
            throw new InpBedOperationException(422, "WORKFLOW_NOT_ACTIVATED", "Workflow belum diaktifkan berdasarkan SOP resmi.");
        var assignment = Assignments.FirstOrDefault(x => x.ServiceUnitId == unitId && x.Operations.Contains(operation) &&
            !string.IsNullOrWhiteSpace(x.SopReference) &&
            (operation != "InpatientBedManagement:VerifyReadiness" || (x.IsReadinessVerifier && !x.OperationalOnly)));
        return assignment ?? throw new InpBedOperationException(503, "DEPENDENCY_UNAVAILABLE", "Bukti penugasan atau SOP belum tersedia.");
    }
}

