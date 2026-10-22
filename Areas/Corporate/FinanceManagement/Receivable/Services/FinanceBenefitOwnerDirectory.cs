using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Pemilik benefit piutang EMPLOYEE_BENEFIT untuk tampilan Finance. Identitas canonical adalah
/// <c>BenefitOwnerWorkforceProfileId</c>; piutang historis hanya membawa <c>BenefitOwnerId</c> (MstEmployee.Id)
/// dan diterjemahkan ke Workforce tanpa mengubah datanya. Semua lewat resolver HR bersama, sehingga piutang dokter
/// tetap menampilkan nama dan nomor dokter tanpa EmployeeId palsu dan tanpa mencampur piutang pegawai lain.
/// Memakai tiga kueri tetap untuk sekumpulan piutang (tidak ada N+1).
/// </summary>
public sealed class FinanceBenefitOwnerLookup
{
    public static readonly FinanceBenefitOwnerLookup Empty = new(
        new Dictionary<Guid, WorkforceBenefitOwner>(), new Dictionary<Guid, Guid>());

    private readonly IReadOnlyDictionary<Guid, WorkforceBenefitOwner> _byProfile;
    private readonly IReadOnlyDictionary<Guid, Guid> _profileByEmployee;

    internal FinanceBenefitOwnerLookup(
        IReadOnlyDictionary<Guid, WorkforceBenefitOwner> byProfile,
        IReadOnlyDictionary<Guid, Guid> profileByEmployee)
    {
        _byProfile = byProfile;
        _profileByEmployee = profileByEmployee;
    }

    /// <summary>Pemilik untuk satu piutang: WorkforceProfileId bila ada, kalau tidak diterjemahkan dari EmployeeId lama.</summary>
    public WorkforceBenefitOwner? Find(Guid? employeeId, Guid? workforceProfileId)
    {
        var profileId = workforceProfileId;
        if (!profileId.HasValue && employeeId.HasValue && _profileByEmployee.TryGetValue(employeeId.Value, out var mapped))
            profileId = mapped;
        return profileId.HasValue && _byProfile.TryGetValue(profileId.Value, out var owner) ? owner : null;
    }

    /// <summary>Identitas canonical pemilik untuk piutang ini (tanpa mengubah baris sumber).</summary>
    public Guid? ResolveProfileId(Guid? employeeId, Guid? workforceProfileId) =>
        workforceProfileId ?? (employeeId.HasValue && _profileByEmployee.TryGetValue(employeeId.Value, out var mapped) ? mapped : null);
}

public static class FinanceBenefitOwnerDirectory
{
    public static async Task<FinanceBenefitOwnerLookup> LoadAsync(
        ApplicationDbContext dbContext,
        IEnumerable<(Guid? EmployeeId, Guid? WorkforceProfileId)> keys,
        CancellationToken cancellationToken)
    {
        var list = keys.ToList();
        var employeeIds = list
            .Where(x => x.EmployeeId.HasValue && !x.WorkforceProfileId.HasValue)
            .Select(x => x.EmployeeId!.Value)
            .Distinct()
            .ToList();

        var profileByEmployee = employeeIds.Count == 0
            ? new Dictionary<Guid, Guid>()
            : await dbContext.MstEmployees.AsNoTracking()
                .Where(x => employeeIds.Contains(x.Id))
                .Select(x => new { x.Id, x.WorkforceProfileId })
                .ToDictionaryAsync(x => x.Id, x => x.WorkforceProfileId, cancellationToken);

        var profileIds = list
            .Where(x => x.WorkforceProfileId.HasValue)
            .Select(x => x.WorkforceProfileId!.Value)
            .Concat(profileByEmployee.Values)
            .Distinct()
            .ToList();
        if (profileIds.Count == 0) return new FinanceBenefitOwnerLookup(new Dictionary<Guid, WorkforceBenefitOwner>(), profileByEmployee);

        var owners = await new WorkforceBenefitOwnerResolver(dbContext).ResolveManyAsync(profileIds, cancellationToken);
        return new FinanceBenefitOwnerLookup(owners, profileByEmployee);
    }
}
