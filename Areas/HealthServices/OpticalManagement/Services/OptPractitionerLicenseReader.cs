using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Satu izin praktik yang berlaku, beserta pegawai pemiliknya.</summary>
    public sealed record OptPractitionerLicense(
        Guid WorkforceProfileId,
        string DisplayName,
        string LicenseType,
        string LicenseNumber,
        DateTime ExpiredDate);

    /// <summary>
    /// Hasil pemeriksaan izin praktik pemeriksa Optik. <see cref="FailureMessage"/> berbunyi sama dengan
    /// <c>OPT-VAL-001</c>/<c>OPT-VAL-002</c> (<c>contracts/validation-matrix.md</c>).
    /// </summary>
    public sealed record OptLicenseCheckResult(
        bool IsValid,
        OptPractitionerLicense? License,
        string? FailureMessage)
    {
        public static OptLicenseCheckResult Valid(OptPractitionerLicense license) => new(true, license, null);

        public static OptLicenseCheckResult Invalid(string message) => new(false, null, message);
    }

    /// <summary>
    /// Adapter baca izin praktik (SIP) pemeriksa Optik dari Human Resource (<c>BE-OPT-004</c>, <c>INT-OPT-03</c>,
    /// <c>OPT-DEC-012</c>). Hanya membaca; tidak pernah menulis data HR.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Perbandingan per tanggal kalender WIB, bukan jam.</b> <c>WfpCredentialLicense.ExpiredDate</c>
    /// menyimpan tanggal kalender pada tengah malam berlabel UTC. Membandingkannya dengan
    /// <c>DateTime.UtcNow</c> — seperti <c>PrescriptionCopyService</c> — membuat SIP yang berakhir
    /// 31-10-2026 dianggap kedaluwarsa sejak 31-10 pukul 07.00 WIB. Di sini <c>ExpiredDate.Date</c>
    /// dibandingkan dengan tanggal operasional WIB (<see cref="AppDateTimeHelper.OperationalDate"/>),
    /// sama dengan <c>DoctorQueueController</c>. <b>Contoh:</b> SIP berakhir 03-11-2026 masih berlaku sampai
    /// 03-11-2026 pukul 23.59 WIB dan tidak berlaku sejak 04-11-2026 00.00 WIB.
    /// </para>
    /// <para>
    /// <b>Syarat berlaku:</b> baris aktif, tidak terhapus, tidak dicabut, terverifikasi
    /// (<c>IsVerified</c> dan <c>VerificationStatus = Verified</c>), sudah terbit, dan belum kedaluwarsa pada
    /// tanggal tersebut.
    /// </para>
    /// <para>
    /// <b>Jenis izin yang dipakai: SIP</b> (<c>LicenseType</c> berawalan <c>SIP</c>, tidak peka huruf besar, agar
    /// varian seperti <c>SIP-RO</c> ikut terbaca) dan <b>STR</b> (<see cref="CheckStrAsync"/>). Keduanya wajib berlaku
    /// (<c>OPT-DEC-056</c>); urutan dan profesi RO/Sp.M dirangkai <see cref="OptPractitionerGuard"/> (<c>BE-OPT-011</c>).
    /// </para>
    /// </remarks>
    public class OptPractitionerLicenseReader
    {
        public const string NoPractitionerMessage = "Anda tidak berwenang membuat resep atau pemeriksaan refraksi.";

        private readonly ApplicationDbContext _dbContext;

        public OptPractitionerLicenseReader(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Memeriksa SIP pemilik akun pada tanggal operasional WIB hari ini.</summary>
        public Task<OptLicenseCheckResult> CheckSipAsync(Guid userId, CancellationToken cancellationToken = default) =>
            CheckAsync(userId, "SIP", AppDateTimeHelper.OperationalDate(), cancellationToken);

        /// <summary>Memeriksa SIP pemilik akun pada tanggal kalender WIB tertentu, contoh tanggal pemeriksaan.</summary>
        public Task<OptLicenseCheckResult> CheckSipAsync(Guid userId, DateTime operationalDate, CancellationToken cancellationToken = default) =>
            CheckAsync(userId, "SIP", operationalDate, cancellationToken);

        /// <summary>Memeriksa STR pemilik akun pada tanggal kalender WIB tertentu (<c>OPT-DEC-056</c>).</summary>
        public Task<OptLicenseCheckResult> CheckStrAsync(Guid userId, DateTime operationalDate, CancellationToken cancellationToken = default) =>
            CheckAsync(userId, "STR", operationalDate, cancellationToken);

        /// <summary>Tanggal kalender WIB dari sebuah saat UTC. Contoh: 2026-11-03 17.00 UTC → 2026-11-04.</summary>
        public static DateTime ToOperationalDate(DateTimeOffset instant)
        {
            var wib = TimeZoneInfo.ConvertTime(instant, TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta"));
            return wib.Date;
        }

        private async Task<OptLicenseCheckResult> CheckAsync(
            Guid userId,
            string licenseTypePrefix,
            DateTime operationalDate,
            CancellationToken cancellationToken)
        {
            var tanggal = operationalDate.Date;

            var pegawai = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (pegawai is not { } workforceProfileId || workforceProfileId == Guid.Empty)
                return OptLicenseCheckResult.Invalid(NoPractitionerMessage);

            var displayName = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);

            if (displayName is null)
                return OptLicenseCheckResult.Invalid(NoPractitionerMessage);

            var awalan = licenseTypePrefix.ToUpper();
            var kandidat = (await _dbContext.Set<WfpCredentialLicense>().AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive)
                    .Select(x => new
                    {
                        x.LicenseType,
                        x.LicenseNumber,
                        x.IssueDate,
                        x.ExpiredDate,
                        x.IsPrimary,
                        x.IsVerified,
                        x.VerificationStatus,
                        x.IsRevoked,
                    })
                    .ToListAsync(cancellationToken))
                .Where(x => (x.LicenseType ?? string.Empty).Trim().ToUpper().StartsWith(awalan, StringComparison.Ordinal))
                .ToList();

            var berlaku = kandidat
                .Where(x => !x.IsRevoked
                    && x.IsVerified
                    && x.VerificationStatus == CredentialVerificationStatus.Verified
                    && x.IssueDate.Date <= tanggal
                    && x.ExpiredDate.Date >= tanggal)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.ExpiredDate)
                .FirstOrDefault();

            if (berlaku is not null)
            {
                return OptLicenseCheckResult.Valid(new OptPractitionerLicense(
                    workforceProfileId,
                    displayName,
                    berlaku.LicenseType,
                    berlaku.LicenseNumber,
                    berlaku.ExpiredDate.Date));
            }

            // Pesan menyebut tanggal berakhir terakhir bila ada, agar petugas tahu sejak kapan izinnya habis.
            var terakhir = kandidat
                .Where(x => !x.IsRevoked && x.ExpiredDate.Date < tanggal)
                .OrderByDescending(x => x.ExpiredDate)
                .FirstOrDefault();

            return OptLicenseCheckResult.Invalid(terakhir is not null
                ? $"{awalan} Anda tidak berlaku sejak {terakhir.ExpiredDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)}."
                : $"{awalan} Anda belum tercatat berlaku. Hubungi bagian SDM.");
        }
    }
}
