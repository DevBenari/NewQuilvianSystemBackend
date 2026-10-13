using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Hasil klasifikasi antrean satu pasien pada tanggal kunjungan (RJ-DOC-DEC-055).
    /// Member, prioritas, dan privasi layar publik adalah tiga keputusan terpisah.
    /// </summary>
    public sealed class OutpatientQueueClassification
    {
        public const string MembershipTierPriorityReason = "MEMBERSHIP_TIER";

        public bool IsMember { get; init; }
        public bool IsPriorityQueue { get; init; }
        public int PriorityLevel { get; init; }
        public QueueAudience QueueAudience { get; init; } = QueueAudience.Regular;
        public PublicDisplayMode PublicDisplayMode { get; init; } = PublicDisplayMode.Default;
        public Guid? PatientMembershipId { get; init; }
        public Guid? MembershipTierId { get; init; }
        public string? MembershipTierCode { get; init; }
        public string? PriorityReasonCode { get; init; }

        public static OutpatientQueueClassification Regular { get; } = new();
    }

    /// <summary>
    /// Satu-satunya penentu klasifikasi antrean Rawat Jalan. Aturan berasal dari
    /// <c>MstPatientMembership</c> aktif dan policy <c>MstMembershipTier</c>; frontend
    /// hanya menerima hasilnya.
    /// </summary>
    public class OutpatientQueueClassificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public OutpatientQueueClassificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OutpatientQueueClassification> ClassifyAsync(
            Guid patientId,
            DateTime encounterDate,
            CancellationToken cancellationToken = default)
        {
            var date = encounterDate.Date;

            var activePatientMembershipId = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId)
                .Select(x => x.ActivePatientMembershipId)
                .FirstOrDefaultAsync(cancellationToken);

            // Membership aktif: status Active, belum dihapus, berlaku pada tanggal kunjungan,
            // dan tier-nya masih aktif. Membership kedaluwarsa tidak dianggap member.
            var membership = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.MembershipStatus == MembershipStatus.Active &&
                    x.JoinDate.Date <= date &&
                    (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= date) &&
                    x.MembershipTier != null &&
                    x.MembershipTier.IsActive &&
                    !x.MembershipTier.IsDelete)
                .OrderByDescending(x => activePatientMembershipId.HasValue && x.Id == activePatientMembershipId.Value)
                .ThenByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.MembershipTier!.PriorityLevel)
                .ThenByDescending(x => x.JoinDate)
                .Select(x => new
                {
                    x.Id,
                    x.MembershipTierId,
                    x.MembershipTier!.TierCode,
                    x.MembershipTier.PriorityQueue,
                    x.MembershipTier.PriorityLevel,
                    x.MembershipTier.QueueAudience,
                    x.MembershipTier.PublicDisplayMode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (membership == null)
            {
                return OutpatientQueueClassification.Regular;
            }

            return new OutpatientQueueClassification
            {
                IsMember = true,
                IsPriorityQueue = membership.PriorityQueue,
                PriorityLevel = membership.PriorityQueue ? membership.PriorityLevel : 0,
                QueueAudience = membership.QueueAudience,
                PublicDisplayMode = membership.PublicDisplayMode,
                PatientMembershipId = membership.Id,
                MembershipTierId = membership.MembershipTierId,
                MembershipTierCode = membership.TierCode,
                PriorityReasonCode = membership.PriorityQueue
                    ? OutpatientQueueClassification.MembershipTierPriorityReason
                    : null
            };
        }
    }
}
