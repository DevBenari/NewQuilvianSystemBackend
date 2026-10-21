using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Hasil klasifikasi antrean satu pasien (RJ-DOC-DEC-055).
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
    /// Satu-satunya penentu klasifikasi antrean Rawat Jalan; frontend hanya menerima hasilnya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Member" memakai aturan tunggal milik CRM (<see cref="PatientMembershipStatusResolver"/>,
    /// crm-membership J1), bukan kueri sendiri (RJ-PRI-DEC-006, memenuhi BE-CRM-022). Kebijakan
    /// antrean tetap dibaca dari <see cref="MstMembershipTier"/>: <c>PriorityQueue</c>,
    /// <c>PriorityLevel</c>, <c>QueueAudience</c>, dan <c>PublicDisplayMode</c>.
    /// </para>
    /// <para>
    /// Tanggal penilaian adalah <b>hari pendaftaran</b> (kalender WIB), bukan tanggal kunjungan
    /// (RJ-PRI-DEC-007, CRM-DEC-005). Contoh: booking 1 Okt untuk kunjungan 20 Okt dinilai pada
    /// 1 Okt, sehingga tetap prioritas walau membership habis 13 Okt.
    /// </para>
    /// </remarks>
    public class OutpatientQueueClassificationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PatientMembershipStatusResolver _membershipStatusResolver;

        public OutpatientQueueClassificationService(
            ApplicationDbContext dbContext,
            PatientMembershipStatusResolver membershipStatusResolver)
        {
            _dbContext = dbContext;
            _membershipStatusResolver = membershipStatusResolver;
        }

        /// <summary>
        /// Mengklasifikasi pasien pada <paramref name="classificationDateWib"/>. Kosong berarti
        /// hari ini (WIB), yaitu hari pendaftaran.
        /// </summary>
        public async Task<OutpatientQueueClassification> ClassifyAsync(
            Guid patientId,
            DateTime? classificationDateWib = null,
            CancellationToken cancellationToken = default)
        {
            var active = await _membershipStatusResolver.GetActiveAsync(
                patientId,
                classificationDateWib,
                cancellationToken);

            if (active == null)
            {
                return OutpatientQueueClassification.Regular;
            }

            var tier = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => x.Id == active.MembershipTierId)
                .Select(x => new
                {
                    x.TierCode,
                    x.PriorityQueue,
                    x.PriorityLevel,
                    x.QueueAudience,
                    x.PublicDisplayMode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tier == null)
            {
                return OutpatientQueueClassification.Regular;
            }

            return Build(
                active.PatientMembershipId,
                active.MembershipTierId,
                tier.TierCode,
                tier.PriorityQueue,
                tier.PriorityLevel,
                tier.QueueAudience,
                tier.PublicDisplayMode);
        }

        /// <summary>
        /// Klasifikasi dari snapshot Reservasi Prioritas yang diambil saat reservasi dibuat
        /// (RJ-PRI-DEC-007). Satu-satunya pencabutan: membership snapshot sudah Dibatalkan, maka
        /// pasien diperlakukan Reguler (RJ-PRI-DEC-024).
        /// </summary>
        public static OutpatientQueueClassification FromReservationSnapshot(
            Guid? patientMembershipIdSnapshot,
            Guid? membershipTierIdSnapshot,
            string? membershipTierCodeSnapshot,
            bool isPrioritySnapshot,
            int queuePriorityLevelSnapshot,
            QueueAudience queueAudienceSnapshot,
            PublicDisplayMode publicDisplayModeSnapshot,
            bool isSnapshotMembershipCancelled)
        {
            if (isSnapshotMembershipCancelled || !patientMembershipIdSnapshot.HasValue)
            {
                return OutpatientQueueClassification.Regular;
            }

            return Build(
                patientMembershipIdSnapshot.Value,
                membershipTierIdSnapshot,
                membershipTierCodeSnapshot,
                isPrioritySnapshot,
                queuePriorityLevelSnapshot,
                queueAudienceSnapshot,
                publicDisplayModeSnapshot);
        }

        private static OutpatientQueueClassification Build(
            Guid patientMembershipId,
            Guid? membershipTierId,
            string? tierCode,
            bool priorityQueue,
            int priorityLevel,
            QueueAudience queueAudience,
            PublicDisplayMode publicDisplayMode)
        {
            return new OutpatientQueueClassification
            {
                IsMember = true,
                IsPriorityQueue = priorityQueue,
                PriorityLevel = priorityQueue ? priorityLevel : 0,
                QueueAudience = queueAudience,
                PublicDisplayMode = publicDisplayMode,
                PatientMembershipId = patientMembershipId,
                MembershipTierId = membershipTierId,
                MembershipTierCode = tierCode,
                PriorityReasonCode = priorityQueue
                    ? OutpatientQueueClassification.MembershipTierPriorityReason
                    : null
            };
        }
    }
}
