using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedClassComparisonService(ApplicationDbContext db, IOptionsMonitor<BedManagementOptions> options)
{
    private List<MstPatientClass>? _cachedClasses;
    private readonly Dictionary<(Guid, Guid), string> _compareCache = new();

    // Format digest: kelas aktif, Id:D|ClassLevel, urut Id; LF; pasangan equivalence ternormalisasi sesudah kelas.
    public async Task<string> CompareAsync(Guid sourceId, Guid targetId, CancellationToken ct = default)
    {
        if (_compareCache.TryGetValue((sourceId, targetId), out var cached)) return cached;
        _cachedClasses ??= await db.Set<MstPatientClass>().AsNoTracking().Where(x => x.IsActive && !x.IsDelete).ToListAsync(ct);
        var classes = _cachedClasses;
        var source = classes.SingleOrDefault(x => x.Id == sourceId);
        var target = classes.SingleOrDefault(x => x.Id == targetId);
        if (source == null || target == null) return "Unknown";
        if (sourceId == targetId) return "SameGrade";
        var profile = options.CurrentValue.ClassOrder;
        if (source.ClassLevel <= 0 || target.ClassLevel <= 0 || profile.Direction == null ||
            !Enum.IsDefined(profile.Direction.Value) || string.IsNullOrWhiteSpace(profile.EvidenceReference)) return "Unknown";
        var pairs = profile.EquivalencePairs.Select(x => x.ClassIdA.CompareTo(x.ClassIdB) < 0
            ? (A: x.ClassIdA, B: x.ClassIdB) : (A: x.ClassIdB, B: x.ClassIdA)).OrderBy(x => x.A).ThenBy(x => x.B).ToList();
        if (pairs.Any(x => x.A == x.B || !classes.Any(c => c.Id == x.A) || !classes.Any(c => c.Id == x.B)) || pairs.Distinct().Count() != pairs.Count) return "Unknown";
        var input = string.Join("\n", classes.OrderBy(x => x.Id).Select(x => $"{x.Id:D}|{x.ClassLevel}")) +
            "\n" + string.Join("\n", pairs.Select(x => $"{x.A:D}|{x.B:D}"));
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
        if (!string.Equals(digest, profile.ApprovedSourceDigest, StringComparison.OrdinalIgnoreCase)) return "Unknown";
        string result;
        if (source.ClassLevel == target.ClassLevel)
            result = pairs.Any(x => x.A == sourceId && x.B == targetId || x.A == targetId && x.B == sourceId) ? "SameGrade" : "Unknown";
        else
        {
            var higher = target.ClassLevel > source.ClassLevel;
            if (profile.Direction == BedClassOrderDirection.LowerValueIsHigherGrade) higher = !higher;
            result = higher ? "UpGrade" : "DownGrade";
        }
        _compareCache[(sourceId, targetId)] = result;
        return result;
    }
}

