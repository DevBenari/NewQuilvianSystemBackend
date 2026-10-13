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
    private readonly Dictionary<string, bool> _permissionCache = new();
    private readonly Dictionary<string, List<Guid>> _unitsCache = new();
    // BE-RWI-228: daftar unit aktif SuperAdmin sama untuk setiap operasi, jadi cukup dibaca
    // sekali per permintaan — sebelumnya dibaca ulang per operasi (4–5 kali per papan bed).
    private List<Guid>? _allActiveUnitsCache;
    private bool? _actorIsActive;
    private bool? _isSuperAdmin;

    public async Task<bool> IsSuperAdminAsync(CancellationToken ct = default)
    {
        if (_isSuperAdmin.HasValue) return _isSuperAdmin.Value;
        if (http.HttpContext?.User?.IsInRole("SuperAdmin") == true ||
            http.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true)
        {
            _isSuperAdmin = true;
            return true;
        }
        if (ActorId == Guid.Empty)
        {
            _isSuperAdmin = false;
            return false;
        }
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ActorId, ct);
        _isSuperAdmin = user != null && ((int)user.UserType == 1 ||
            await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id &&
                db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "SuperAdmin"), ct));
        return _isSuperAdmin.Value;
    }

    public async Task<bool> HasPermissionAsync(string operation)
    {
        if (_permissionCache.TryGetValue(operation, out var cached)) return cached;
        var parts = operation.Split(':');
        var allowed = parts.Length == 2 && http.HttpContext != null &&
            await permissions.HasAccessAsync(http.HttpContext.User, parts[0], parts[1]);
        _permissionCache[operation] = allowed;
        return allowed;
    }
    public async Task<List<Guid>> UnitsAsync(string operation, CancellationToken ct = default)
    {
        if (_unitsCache.TryGetValue(operation, out var cached)) return cached;
        if (ActorId == Guid.Empty) return [];
        if (!_actorIsActive.HasValue)
        {
            _actorIsActive = await db.Users.AnyAsync(x => x.Id == ActorId && x.IsActive, ct);
        }
        if (!_actorIsActive.Value || !await HasPermissionAsync(operation))
        {
            _unitsCache[operation] = [];
            return [];
        }
        var isSuperAdmin = await IsSuperAdminAsync(ct);
        if (isSuperAdmin)
        {
            _allActiveUnitsCache ??= await db.Set<MstServiceUnit>()
                .Where(x => !x.IsDelete && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);
            _unitsCache[operation] = _allActiveUnitsCache;
            return _allActiveUnitsCache;
        }
        var ids = Assignments.Where(x => x.Operations.Contains(operation) &&
            (!x.OperationalOnly || operation is "InpatientBedManagement:Read" or "InpatientBedManagement:StartCleaning" or "InpatientBedManagement:CompleteCleaning" or "InpatientBedOccupancy:Read"))
            .Select(x => x.ServiceUnitId).Distinct().ToList();
        var result = await db.Set<MstServiceUnit>().Where(x => ids.Contains(x.Id) && !x.IsDelete).Select(x => x.Id).ToListAsync(ct);
        _unitsCache[operation] = result;
        return result;
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
        var isSuperAdmin = await IsSuperAdminAsync(ct);
        if (episode == null || !units.Contains(episode.ServiceUnitId) ||
            (!isSuperAdmin && Assignments.Any(x => x.ServiceUnitId == episode.ServiceUnitId && x.OperationalOnly)) ||
            !await HasPermissionAsync("InpatientEpisode:Read"))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Tidak berwenang membaca episode ini.");
    }
    public async Task<bool> CanReadIdentityAsync(Guid episodeId, Guid unitId, CancellationToken ct = default)
    {
        var isSuperAdmin = await IsSuperAdminAsync(ct);
        if (!isSuperAdmin && (!Assignments.Any(x => x.ServiceUnitId == unitId) || Assignments.Any(x => x.ServiceUnitId == unitId && x.OperationalOnly)))
            return false;
        if (!await HasPermissionAsync("InpatientEpisode:Read")) return false;
        var units = await UnitsAsync("InpatientBedOccupancy:Read", ct);
        return await db.Set<InpEpisode>().AnyAsync(x => x.Id == episodeId && !x.IsDelete && units.Contains(x.ServiceUnitId), ct);
    }

    /// <summary>
    /// Aturan yang sama persis dengan <see cref="CanReadIdentityAsync"/>, tetapi memakai data
    /// episode yang sudah dimuat pemanggil. Dipakai proyeksi bed supaya pemeriksaan identitas
    /// tidak lagi satu query per bed terisi/dipesan (BE-RWI-228).
    /// </summary>
    public async Task<bool> CanReadLoadedIdentityAsync(Guid unitId, Guid episodeServiceUnitId, bool episodeIsDeleted, CancellationToken ct = default)
    {
        var isSuperAdmin = await IsSuperAdminAsync(ct);
        if (!isSuperAdmin && (!Assignments.Any(x => x.ServiceUnitId == unitId) || Assignments.Any(x => x.ServiceUnitId == unitId && x.OperationalOnly)))
            return false;
        if (!await HasPermissionAsync("InpatientEpisode:Read")) return false;
        if (episodeIsDeleted) return false;
        var units = await UnitsAsync("InpatientBedOccupancy:Read", ct);
        return units.Contains(episodeServiceUnitId);
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
        if (assignment != null) return assignment;

        if (http.HttpContext?.User?.IsInRole("SuperAdmin") == true ||
            http.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new BedUnitAssignment
            {
                UserId = ActorId,
                ServiceUnitId = unitId,
                Operations = [operation],
                AssignmentReference = options.CurrentValue.ReservationExpiryAssignmentReference ?? "SK-SUPERADMIN-FALLBACK",
                SopReference = "SOP-RWI-BED-001",
                OperationalOnly = false,
                IsReadinessVerifier = true
            };
        }

        throw new InpBedOperationException(503, "DEPENDENCY_UNAVAILABLE", "Bukti penugasan atau SOP belum tersedia.");
    }
}

