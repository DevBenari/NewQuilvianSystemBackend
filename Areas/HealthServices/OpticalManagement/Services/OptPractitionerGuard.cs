using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>Pemeriksa Optik yang lolos <c>OPT-VAL-001</c> dan <c>OPT-VAL-002</c> pada satu tanggal WIB.</summary>
    public sealed record OptPractitioner(
        Guid UserId,
        Guid WorkforceProfileId,
        string DisplayName,
        bool IsOphthalmologist,
        string SipNumber,
        string StrNumber);

    /// <summary>
    /// Penjaga kewenangan klinis Optik: hanya RO atau Sp.M dengan SIP <b>dan</b> STR berlaku yang boleh membuat atau
    /// merevisi pemeriksaan refraksi dan resep (<c>OPT-DEC-005</c>, <c>012</c>, <c>056</c>, <c>058</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Urutan pemeriksaan</b> (kalimat penolakan mengikuti <c>OPT-VAL-001</c>..<c>003</c>):
    /// akun → pegawai → profesi RO/Sp.M → SIP → STR. Penolakan pertama yang ditemui yang dikembalikan.
    /// </para>
    /// <para>
    /// <b>Profesi dari daftar kode konfigurasi</b> (<c>OPT-DEC-058</c>). RO = pegawai (<c>MstEmployee</c>) aktif yang kode
    /// profesinya tercantum di <c>OpticalManagement:Practitioner:RefractionistProfessionCodes</c>. Sp.M = dokter
    /// (<c>MstDoctor</c>) aktif yang kode spesialisasinya tercantum di
    /// <c>OpticalManagement:Practitioner:OphthalmologistSpecializationCodes</c>. Daftar boleh berupa larik JSON atau
    /// teks dipisah koma, tidak peka huruf besar. <b>Daftar kosong berarti semua ditolak</b>, sehingga salah konfigurasi
    /// tidak membuka akses.
    /// </para>
    /// </remarks>
    public class OptPractitionerGuard
    {
        public const string RefractionistCodesKey = "OpticalManagement:Practitioner:RefractionistProfessionCodes";
        public const string OphthalmologistCodesKey = "OpticalManagement:Practitioner:OphthalmologistSpecializationCodes";

        public const string HrUnavailableMessage = "Data izin praktik sedang tidak dapat diperiksa. Coba lagi beberapa saat.";

        private readonly ApplicationDbContext _dbContext;
        private readonly OptPractitionerLicenseReader _licenseReader;
        private readonly HashSet<string> _refractionistCodes;
        private readonly HashSet<string> _ophthalmologistCodes;

        public OptPractitionerGuard(ApplicationDbContext dbContext, OptPractitionerLicenseReader licenseReader, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _licenseReader = licenseReader;
            _refractionistCodes = ReadCodes(configuration, RefractionistCodesKey);
            _ophthalmologistCodes = ReadCodes(configuration, OphthalmologistCodesKey);
        }

        /// <summary>Memeriksa pemilik akun pada tanggal kalender WIB tertentu (biasanya tanggal operasional hari ini).</summary>
        public async Task<OptResult<OptPractitioner>> CheckAsync(Guid userId, DateTime operationalDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var workforceProfileId = await _dbContext.Users.AsNoTracking()
                    .Where(x => x.Id == userId)
                    .Select(x => x.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (workforceProfileId is not { } pegawaiId || pegawaiId == Guid.Empty)
                    return NotPractitioner();

                var isOphthalmologist = await IsOphthalmologistAsync(pegawaiId, cancellationToken);
                var isRefractionist = !isOphthalmologist && await IsRefractionistAsync(pegawaiId, cancellationToken);
                if (!isOphthalmologist && !isRefractionist)
                    return NotPractitioner();

                var sip = await _licenseReader.CheckSipAsync(userId, operationalDate, cancellationToken);
                if (!sip.IsValid)
                    return OptResult<OptPractitioner>.Forbidden(OptErrorCodes.Val002, sip.FailureMessage!);

                var str = await _licenseReader.CheckStrAsync(userId, operationalDate, cancellationToken);
                if (!str.IsValid)
                    return OptResult<OptPractitioner>.Forbidden(OptErrorCodes.Val002, str.FailureMessage!);

                return OptResult<OptPractitioner>.Ok(new OptPractitioner(
                    userId,
                    pegawaiId,
                    sip.License!.DisplayName,
                    isOphthalmologist,
                    sip.License.LicenseNumber,
                    str.License!.LicenseNumber));
            }
            catch (DbException)
            {
                return OptResult<OptPractitioner>.Unavailable(OptErrorCodes.Val003, HrUnavailableMessage);
            }
        }

        private async Task<bool> IsOphthalmologistAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (_ophthalmologistCodes.Count == 0)
                return false;

            var codes = await (
                    from doctor in _dbContext.MstDoctors.AsNoTracking()
                    join specialization in _dbContext.MstSpecializations.AsNoTracking() on doctor.SpecializationId equals (Guid?)specialization.Id
                    where doctor.WorkforceProfileId == workforceProfileId && doctor.IsActive && !doctor.IsDelete
                    select specialization.SpecializationCode)
                .ToListAsync(cancellationToken);

            return codes.Any(x => _ophthalmologistCodes.Contains(Normalize(x)));
        }

        private async Task<bool> IsRefractionistAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (_refractionistCodes.Count == 0)
                return false;

            var codes = await (
                    from employee in _dbContext.MstEmployees.AsNoTracking()
                    join profession in _dbContext.MstProfessions.AsNoTracking() on employee.ProfessionId equals (Guid?)profession.Id
                    where employee.WorkforceProfileId == workforceProfileId && employee.IsActive && !employee.IsDelete
                    select profession.ProfessionCode)
                .ToListAsync(cancellationToken);

            return codes.Any(x => _refractionistCodes.Contains(Normalize(x)));
        }

        private static OptResult<OptPractitioner> NotPractitioner() =>
            OptResult<OptPractitioner>.Forbidden(OptErrorCodes.Val001, OptPractitionerLicenseReader.NoPractitionerMessage);

        private static HashSet<string> ReadCodes(IConfiguration configuration, string key)
        {
            var section = configuration.GetSection(key);
            var values = section.GetChildren().Select(x => x.Value).ToList();
            if (!string.IsNullOrWhiteSpace(section.Value))
                values.AddRange(section.Value.Split(','));

            return values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Normalize(x!))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static string Normalize(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();
    }
}
