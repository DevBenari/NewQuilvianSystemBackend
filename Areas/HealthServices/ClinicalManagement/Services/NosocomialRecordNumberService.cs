using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class NosocomialRecordNumberService(ApplicationDbContext db, NumberSeriesAllocator allocator)
{
    public async Task<string> AllocateAsync(DateTime now, Guid actor, CancellationToken ct)
    {
        var prefix = $"NOS-{now:yyyyMMdd}-";
        var existing = await db.Set<TrxNosocomialInfection>().AsNoTracking().Where(x => x.NosocomialRecordNumber.StartsWith(prefix)).Select(x => x.NosocomialRecordNumber).ToListAsync(ct);
        var minimum = existing.Select(x => long.TryParse(x[prefix.Length..], out var n) ? n : 0).DefaultIfEmpty().Max();
        return await allocator.AllocateAsync(new NumberAllocationRequest("CLINICAL_NOSOCOMIAL", "NOS", "DAILY", 4, actor, new DateTimeOffset(now), minimum), ct);
    }
}
