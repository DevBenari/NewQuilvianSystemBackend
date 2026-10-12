using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using System.Linq.Expressions;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Satu-satunya definisi "mitra yang boleh dipilih" (<c>DEC-FRJ-001</c>), dipakai bersama
    /// daftar pilihan master dan validasi submit Registrasi (Kiosk dan Rawat Jalan).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Layak = belum dihapus, <c>IsPartner</c>, <c>IsActive</c>, <b>dan</b> punya perjanjian yang
    /// mencakup tanggal layanan (inklusif). Ketiga syarat itu sengaja tidak digabung menjadi satu
    /// kolom: menonaktifkan mitra, kontrak yang berakhir, dan data lama nonmitra adalah tiga
    /// keadaan berbeda dengan arti berbeda.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Klinik dengan PKS 2026-01-01 s.d. 2026-06-30 layak untuk kunjungan
    /// 2026-06-30, tetapi tidak untuk kunjungan 2026-07-01 — walaupun masih aktif dan mitra.
    /// </para>
    /// </remarks>
    public static class ReferralPartnerEligibility
    {
        /// <summary>Tanggal operasional hari ini (WIB) sebagai tanggal kalender.</summary>
        public static DateOnly Today()
            => DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

        public static Expression<Func<MstReferralInstitution, bool>> IsEligibleOn(DateOnly date)
            => x => !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.IsPartner &&
                    x.Agreements.Any(a => !a.IsDelete && a.StartDate <= date && a.EndDate >= date);

        /// <summary>
        /// Penyaring status kerja sama yang dapat diterjemahkan ke SQL. Hasilnya saling lepas dan
        /// mengikuti urutan penentuan pada <see cref="ReferralPartnershipStatus"/>.
        /// </summary>
        public static Expression<Func<MstReferralInstitution, bool>> HasStatus(
            ReferralPartnershipStatus status,
            DateOnly date)
            => status switch
            {
                ReferralPartnershipStatus.NonPartner => x => !x.IsPartner,
                ReferralPartnershipStatus.Inactive => x => x.IsPartner && !x.IsActive,
                ReferralPartnershipStatus.Active => x => x.IsPartner && x.IsActive &&
                    x.Agreements.Any(a => !a.IsDelete && a.StartDate <= date && a.EndDate >= date),
                ReferralPartnershipStatus.NotYetEffective => x => x.IsPartner && x.IsActive &&
                    !x.Agreements.Any(a => !a.IsDelete && a.StartDate <= date && a.EndDate >= date) &&
                    x.Agreements.Any(a => !a.IsDelete && a.StartDate > date),
                ReferralPartnershipStatus.Expired => x => x.IsPartner && x.IsActive &&
                    x.Agreements.Any(a => !a.IsDelete) &&
                    !x.Agreements.Any(a => !a.IsDelete && a.EndDate >= date),
                ReferralPartnershipStatus.NoAgreement => x => x.IsPartner && x.IsActive &&
                    !x.Agreements.Any(a => !a.IsDelete),
                _ => x => false
            };

        public static ReferralPartnershipStatus Resolve(
            bool isPartner,
            bool isActive,
            bool hasCurrentAgreement,
            bool hasFutureAgreement,
            bool hasAnyAgreement)
        {
            if (!isPartner) return ReferralPartnershipStatus.NonPartner;
            if (!isActive) return ReferralPartnershipStatus.Inactive;
            if (hasCurrentAgreement) return ReferralPartnershipStatus.Active;
            if (hasFutureAgreement) return ReferralPartnershipStatus.NotYetEffective;
            return hasAnyAgreement ? ReferralPartnershipStatus.Expired : ReferralPartnershipStatus.NoAgreement;
        }

        public static string Label(ReferralPartnershipStatus status) => status switch
        {
            ReferralPartnershipStatus.Active => "Aktif",
            ReferralPartnershipStatus.Inactive => "Nonaktif",
            ReferralPartnershipStatus.Expired => "Kontrak Berakhir",
            ReferralPartnershipStatus.NotYetEffective => "Belum Berlaku",
            ReferralPartnershipStatus.NoAgreement => "Tanpa Perjanjian",
            ReferralPartnershipStatus.NonPartner => "Nonmitra (Data Lama)",
            _ => status.ToString()
        };

        /// <summary>
        /// Mencari fasilitas layak beserta perjanjian yang membuatnya layak pada
        /// <paramref name="serviceDate"/>. <c>null</c> berarti tidak layak untuk transaksi baru.
        /// </summary>
        public static Task<EligiblePartnerSnapshot?> FindEligibleAsync(
            ApplicationDbContext dbContext,
            Guid institutionId,
            DateOnly serviceDate,
            CancellationToken cancellationToken)
            => dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => x.Id == institutionId)
                .Where(IsEligibleOn(serviceDate))
                .Select(x => new EligiblePartnerSnapshot(
                    x.Id,
                    x.InstitutionCode,
                    x.InstitutionName,
                    x.IsPartner,
                    x.Agreements
                        .Where(a => !a.IsDelete && a.StartDate <= serviceDate && a.EndDate >= serviceDate)
                        .OrderByDescending(a => a.StartDate)
                        .Select(a => a.AgreementNumber)
                        .FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken);
    }

    public sealed record EligiblePartnerSnapshot(
        Guid Id,
        string InstitutionCode,
        string InstitutionName,
        bool IsPartner,
        string? AgreementNumber);
}
