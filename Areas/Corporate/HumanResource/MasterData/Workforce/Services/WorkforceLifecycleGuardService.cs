using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    /// <summary>
    /// Penjaga integrasi Data Induk (Menu 04) dengan Pemberhentian &amp; Pensiun (Menu 12) dan
    /// skorsing disiplin (BE-HRD-401).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purnatugas.</b> Pegawai yang pemberhentiannya sudah final (status <c>FinalizedCompleted</c>
    /// atau <c>Completed</c>, atau sudah ada <c>CompletedAt</c>) dan tanggal efektifnya sudah lewat
    /// tidak boleh diaktifkan kembali lewat form Data Induk. Penerimaan kembali melalui Rekrutmen dan
    /// Onboarding. Contoh: perawat A pensiun 31 Oktober 2026; pada 5 November 2026 petugas mengedit
    /// alamatnya dan tidak sengaja menyalakan "Aktif" — akun login A tidak boleh hidup kembali.
    /// </para>
    /// <para>
    /// <b>Skorsing.</b> Akun yang dikunci worker skorsing (<c>HrdSuspensionAccessAction</c> terakhir
    /// bertipe <c>Locked</c>) tidak boleh dibuka oleh edit Data Induk; pemulihan dilakukan worker
    /// saat skorsing berakhir.
    /// </para>
    /// </remarks>
    public class WorkforceLifecycleGuardService : IWorkforceLifecycleGuardService
    {
        private static readonly string[] FinalSeparationStatuses = { "FinalizedCompleted", "Completed" };

        private readonly ApplicationDbContext _dbContext;

        public WorkforceLifecycleGuardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsSeparationFinalizedAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return false;
            }

            var today = DateTime.UtcNow.Date;

            return await _dbContext.Set<TrxEmployeeSeparation>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    (x.CompletedAt != null || FinalSeparationStatuses.Contains(x.SeparationStatus)) &&
                    x.EffectiveSeparationDate <= today,
                    cancellationToken);
        }

        public async Task<bool> HasActiveSuspensionLockAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return false;
            }

            var lastActionType = await _dbContext.Set<HrdSuspensionAccessAction>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDelete)
                .OrderByDescending(x => x.ActionAt)
                .Select(x => x.ActionType)
                .FirstOrDefaultAsync(cancellationToken);

            return lastActionType == HrdSuspensionAccessAction.ActionLocked;
        }
    }
}
