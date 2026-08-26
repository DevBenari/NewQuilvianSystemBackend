using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Adapter kewenangan klinis untuk penjadwalan operasi (OPS-DEC-017, OPS-CON kredensial).
/// Sumber data existing adalah <c>WfpClinicalPrivilege</c> milik CredentialingManagement.
/// Owner integrasi belum ditetapkan, sehingga tenaga tanpa data privilege dilaporkan
/// <see cref="OprCredentialCheckStatus.NotAvailable"/> dan tidak memblokir penjadwalan;
/// hanya privilege yang jelas diblokir/dicabut yang menolak jadwal.
/// </summary>
public sealed class OperatingRoomCredentialResolver(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyDictionary<Guid, OprCredentialCheckStatus>> ResolveAsync(
        IReadOnlyCollection<Guid> workforceIds, DateTime effectiveAt, CancellationToken cancellationToken = default)
    {
        var privileges = await dbContext.WfpClinicalPrivileges.AsNoTracking()
            .Where(x => workforceIds.Contains(x.WorkforceProfileId) && !x.IsDelete)
            .Select(x => new
            {
                x.WorkforceProfileId, x.PrivilegeStatus, x.IsSchedulingBlocked,
                x.EffectiveStartDate, x.EffectiveEndDate
            })
            .ToListAsync(cancellationToken);

        return workforceIds.Distinct().ToDictionary(id => id, id =>
        {
            var owned = privileges.Where(x => x.WorkforceProfileId == id).ToList();
            if (owned.Count == 0) return OprCredentialCheckStatus.NotAvailable;
            if (owned.Any(x => x.IsSchedulingBlocked ||
                x.PrivilegeStatus is ClinicalPrivilegeStatus.Suspended or ClinicalPrivilegeStatus.Revoked))
                return OprCredentialCheckStatus.Invalid;
            var valid = owned.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Active &&
                x.EffectiveStartDate <= effectiveAt &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= effectiveAt));
            if (valid) return OprCredentialCheckStatus.Valid;
            // Privilege kedaluwarsa tanpa pengganti aktif adalah penolakan eksplisit,
            // bukan dependency yang belum tersedia.
            return owned.Any(x => x.PrivilegeStatus == ClinicalPrivilegeStatus.Expired)
                ? OprCredentialCheckStatus.Invalid
                : OprCredentialCheckStatus.NotAvailable;
        });
    }
}
