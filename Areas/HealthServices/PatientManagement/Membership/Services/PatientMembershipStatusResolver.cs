using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Membership Aktif seorang pasien pada satu tanggal WIB.</summary>
    public sealed record PatientActiveMembership(
        Guid PatientMembershipId,
        Guid PatientId,
        Guid MembershipTierId,
        string TierCode,
        string TierName,
        MembershipTierType TierType,
        string MemberNumber,
        DateTime PeriodStartDate,
        DateTime PeriodEndDate);

    /// <summary>
    /// Satu-satunya aturan "member aktif" (crm-membership J1, CRM-DEC-005): pada tanggal WIB X,
    /// pasien adalah member bila punya membership berstatus Aktif dengan
    /// <c>PeriodStartDate ≤ X ≤ ExpiredDate</c> dan tingkatnya aktif. Selain itu pasien Reguler.
    /// Menunggu Pembayaran, Terjadwal, dan Perlu Verifikasi tidak pernah terbaca member.
    /// </summary>
    /// <remarks>
    /// Membership Aktif tanpa periode lengkap tidak dianggap member (fail-closed). Proyeksi di
    /// <see cref="MstPatient"/> hanya ditulis lewat <see cref="SyncPatientProjectionAsync"/>.
    /// </remarks>
    public class PatientMembershipStatusResolver
    {
        public const string TimeZoneConfigKey = "PatientMembership:TimeZone";
        private const string DefaultTimeZoneId = "Asia/Jakarta";

        private readonly ApplicationDbContext _dbContext;
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeProvider _timeProvider;

        public PatientMembershipStatusResolver(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _timeZone = ResolveTimeZone(configuration[TimeZoneConfigKey]);
        }

        /// <summary>Zona waktu kalender membership (bawaan Asia/Jakarta).</summary>
        public TimeZoneInfo TimeZone => _timeZone;

        /// <summary>Tanggal kalender WIB hari ini (Kind Unspecified, cocok untuk kolom <c>date</c>).</summary>
        public DateTime GetTodayWib() => ToWibDate(_timeProvider.GetUtcNow());

        public DateTime ToWibDate(DateTimeOffset instant) =>
            DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(instant, _timeZone).Date, DateTimeKind.Unspecified);

        /// <summary>Membership yang terbaca Aktif pada <paramref name="dateWib"/>.</summary>
        public IQueryable<MstPatientMembership> QueryActiveOn(DateTime dateWib)
        {
            var date = DateTime.SpecifyKind(dateWib.Date, DateTimeKind.Unspecified);

            return _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.MembershipStatus == MembershipStatus.Active &&
                    x.PeriodStartDate != null &&
                    x.ExpiredDate != null &&
                    x.PeriodStartDate <= date &&
                    x.ExpiredDate >= date &&
                    x.MembershipTier != null &&
                    x.MembershipTier.IsActive &&
                    !x.MembershipTier.IsDelete);
        }

        public async Task<PatientActiveMembership?> GetActiveAsync(
            Guid patientId,
            DateTime? dateWib = null,
            CancellationToken cancellationToken = default)
        {
            var result = await GetActiveForPatientsAsync([patientId], dateWib, cancellationToken);
            return result.GetValueOrDefault(patientId);
        }

        public async Task<Dictionary<Guid, PatientActiveMembership>> GetActiveForPatientsAsync(
            IEnumerable<Guid> patientIds,
            DateTime? dateWib = null,
            CancellationToken cancellationToken = default)
        {
            var ids = patientIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            var rows = await QueryActiveOn(dateWib ?? GetTodayWib())
                .Where(x => ids.Contains(x.PatientId))
                .Select(x => new PatientActiveMembership(
                    x.Id,
                    x.PatientId,
                    x.MembershipTierId,
                    x.MembershipTier!.TierCode,
                    x.MembershipTier.TierName,
                    x.MembershipTier.TierType,
                    x.MemberNumber,
                    x.PeriodStartDate!.Value,
                    x.ExpiredDate!.Value))
                .ToListAsync(cancellationToken);

            // Index unik per status menjamin satu baris Aktif per pasien; GroupBy hanya pengaman.
            return rows
                .GroupBy(x => x.PatientId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.PeriodStartDate).First());
        }

        /// <summary>
        /// Menyelaraskan <c>IsMember</c>, <c>DefaultMembershipTierId</c>, dan
        /// <c>ActivePatientMembershipId</c> pasien dengan aturan di atas. Tidak memanggil
        /// <c>SaveChanges</c>; perubahan ikut transaksi pemanggil.
        /// </summary>
        /// <returns><c>true</c> bila proyeksi berubah.</returns>
        public async Task<bool> SyncPatientProjectionAsync(
            Guid patientId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var patient = await _dbContext.Set<MstPatient>()
                .FirstOrDefaultAsync(x => x.Id == patientId && !x.IsDelete, cancellationToken);

            if (patient == null)
            {
                return false;
            }

            var active = await GetActiveAsync(patientId, null, cancellationToken);
            var isMember = active != null;
            var tierId = active?.MembershipTierId;
            var membershipId = active?.PatientMembershipId;

            if (patient.IsMember == isMember &&
                patient.DefaultMembershipTierId == tierId &&
                patient.ActivePatientMembershipId == membershipId)
            {
                return false;
            }

            patient.IsMember = isMember;
            patient.DefaultMembershipTierId = tierId;
            patient.ActivePatientMembershipId = membershipId;
            patient.UpdateDateTime = _timeProvider.GetUtcNow().UtcDateTime;
            patient.UpdateBy = actorUserId;

            return true;
        }

        private static TimeZoneInfo ResolveTimeZone(string? configuredId)
        {
            var id = string.IsNullOrWhiteSpace(configuredId) ? DefaultTimeZoneId : configuredId.Trim();

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // WIB tidak mengenal musim panas; offset tetap +07:00 aman sebagai cadangan.
                return TimeZoneInfo.CreateCustomTimeZone(id, TimeSpan.FromHours(7), id, id);
            }
        }
    }
}
