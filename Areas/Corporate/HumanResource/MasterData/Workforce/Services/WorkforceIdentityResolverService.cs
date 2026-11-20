using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    /// <summary>
    /// Resolver identitas resmi lintas Pegawai, Dokter, dan Pengguna Eksternal (<c>HRD-DEC-051</c>,
    /// <c>BE-HRD-387</c>): satu orang dipetakan ke tepat satu <see cref="MstWorkforceProfile"/>
    /// berdasarkan NIK (WNI) atau nomor paspor (WNA).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Penjaga balapan.</b> Dua petugas yang mendaftarkan orang yang sama pada saat bersamaan —
    /// misalnya bagian SDM membuat data Pegawai dan bagian Komite Medik membuat data Dokter untuk
    /// dr. A dengan NIK yang sama — tidak boleh menghasilkan dua profil. Resolver memegang kunci
    /// penasihat PostgreSQL per nomor identitas (<c>pg_advisory_xact_lock</c>) di dalam transaksi
    /// pemanggil. Permintaan kedua menunggu sampai transaksi pertama di-commit, lalu menemukan
    /// peran yang baru tersimpan dan menautkan diri ke profil yang sama (atau ditolak
    /// <c>DUPLICATE_ROLE</c> bila perannya sama).
    /// </para>
    /// <para>
    /// <b>Kode profil.</b> Kode <c>WFP-RSMMC-000001</c> diterbitkan alokator number-series bersama
    /// yang atomik (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>), bukan dari <c>Count+1</c>. Kode
    /// tertinggi yang sudah ada dipakai sebagai batas bawah agar deret tidak bentrok dengan data lama.
    /// </para>
    /// </remarks>
    public class WorkforceIdentityResolverService : IWorkforceIdentityResolverService
    {
        private const string HospitalCode = "RSMMC";
        private const string ProfileCodeSequenceKey = "MST_WORKFORCE_PROFILE";
        private const string ProfileCodePrefix = "WFP-" + HospitalCode;
        private const string ProfileCodeLegacyPrefix = ProfileCodePrefix + "-";
        private const int ProfileCodeDigits = 6;

        private const int NikLength = 16;
        private const int PassportMinLength = 6;
        private const int PassportMaxLength = 16;

        private static readonly Regex PassportPattern = new("^[A-Z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex DigitsPattern = new("^[0-9]+$", RegexOptions.Compiled);

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public WorkforceIdentityResolverService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        public bool IsPassportType(string? identityType)
        {
            var type = (identityType ?? string.Empty).Trim().ToUpperInvariant();
            return type == "PASPOR" || type == "PASSPORT";
        }

        public string NormalizeIdentityNumber(string? identityType, string? identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                return string.Empty;

            var trimmed = identityNumber.Trim();

            if (IsPassportType(identityType))
            {
                // Paspor: huruf besar, tanpa spasi dan tanda hubung.
                return Regex.Replace(trimmed, @"[\s\-]+", string.Empty).ToUpperInvariant();
            }

            // NIK/KTP: hanya digit.
            return Regex.Replace(trimmed, @"\D", string.Empty);
        }

        public bool IsValidIdentity(string? identityType, string? identityNumber, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                errorMessage = "Nomor identitas wajib diisi.";
                return false;
            }

            if (IsPassportType(identityType))
            {
                var passport = NormalizeIdentityNumber(identityType, identityNumber);
                if (passport.Length < PassportMinLength
                    || passport.Length > PassportMaxLength
                    || !PassportPattern.IsMatch(passport))
                {
                    errorMessage = $"Nomor Paspor tidak valid: wajib {PassportMinLength}–{PassportMaxLength} karakter huruf atau angka.";
                    return false;
                }

                errorMessage = null;
                return true;
            }

            // NIK WNI: spasi, titik, dan tanda hubung pemisah boleh dipakai saat mengetik, tetapi
            // sisanya wajib tepat 16 digit. Huruf tidak dibuang diam-diam.
            var compact = Regex.Replace(identityNumber.Trim(), @"[\s.\-]+", string.Empty);
            if (compact.Length != NikLength || !DigitsPattern.IsMatch(compact))
            {
                errorMessage = "Nomor Induk Kependudukan (NIK) harus terdiri dari 16 digit angka.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public async Task<IdentityLookupResult> FindByIdentityAsync(
            string? identityType,
            string? identityNumber,
            CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeIdentityNumber(identityType, identityNumber);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return NotFound();
            }

            var existingEmployee = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdentityNumber == normalized && !x.IsDelete, cancellationToken);

            var existingDoctor = await _dbContext.MstDoctors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdentityNumber == normalized && !x.IsDelete, cancellationToken);

            var existingExternal = await _dbContext.MstExternalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdentityNumber == normalized && !x.IsDelete, cancellationToken);

            var targetProfileId = existingEmployee?.WorkforceProfileId
                ?? existingDoctor?.WorkforceProfileId
                ?? existingExternal?.WorkforceProfileId;

            if (!targetProfileId.HasValue || targetProfileId.Value == Guid.Empty)
            {
                return NotFound();
            }

            var profile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(x => x.Id == targetProfileId.Value && !x.IsDelete, cancellationToken);

            var roles = new List<string>();
            if (existingEmployee != null) roles.Add("Pegawai");
            if (existingDoctor != null) roles.Add("Dokter");
            if (existingExternal != null) roles.Add("Pengguna Eksternal");

            return new IdentityLookupResult(
                Found: true,
                WorkforceProfileId: targetProfileId.Value,
                WorkforceProfile: profile,
                HasEmployeeRole: existingEmployee != null,
                EmployeeId: existingEmployee?.Id,
                HasDoctorRole: existingDoctor != null,
                DoctorId: existingDoctor?.Id,
                HasExternalUserRole: existingExternal != null,
                ExternalUserId: existingExternal?.Id,
                DisplayName: profile?.DisplayName ?? existingEmployee?.FullName ?? existingDoctor?.FullName ?? existingExternal?.FullName,
                ExistingRoles: roles
            );
        }

        public async Task<ResolvedWorkforceProfileResult> ResolveOrCreateProfileAsync(
            string? identityType,
            string? identityNumber,
            string fullName,
            string? email,
            string? phoneNumber,
            string? whatsAppNumber,
            Guid? primaryDepartmentId,
            Guid? primaryPositionId,
            UserType userType,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // Tautan peran dan profil baru wajib satu transaksi dengan pembuatan peran oleh pemanggil,
            // supaya kegagalan di tengah tidak meninggalkan profil yatim (BE-HRD-388) dan supaya
            // kunci identitas bertahan sampai peran tersimpan.
            if (_dbContext.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException(
                    "IDENTITY_RESOLUTION_REQUIRES_TRANSACTION: resolver identitas harus dipanggil di dalam transaksi database pemanggil.");
            }

            var normalized = NormalizeIdentityNumber(identityType, identityNumber);

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                await AcquireIdentityLockAsync(normalized, cancellationToken);

                var match = await FindByIdentityAsync(identityType, identityNumber, cancellationToken);

                if (match.Found && match.WorkforceProfile != null)
                {
                    if (userType == UserType.Employee && match.HasEmployeeRole)
                    {
                        throw new InvalidOperationException("DUPLICATE_ROLE: Pegawai dengan nomor identitas ini sudah terdaftar.");
                    }
                    if ((userType == UserType.PermanentDoctor || userType == UserType.GuestDoctor) && match.HasDoctorRole)
                    {
                        throw new InvalidOperationException("DUPLICATE_ROLE: Dokter dengan nomor identitas ini sudah terdaftar.");
                    }
                    if (userType == UserType.ExternalUser && match.HasExternalUserRole)
                    {
                        throw new InvalidOperationException("DUPLICATE_ROLE: Pengguna eksternal dengan nomor identitas ini sudah terdaftar.");
                    }

                    // Tautkan peran baru ke profil yang sama; hanya isian profil yang masih kosong
                    // yang dilengkapi, isian yang sudah ada tidak ditimpa.
                    var profile = match.WorkforceProfile;
                    profile.UpdateDateTime = DateTime.UtcNow;
                    profile.UpdateBy = actorUserId;

                    if (string.IsNullOrWhiteSpace(profile.Email) && !string.IsNullOrWhiteSpace(email))
                        profile.Email = email.Trim().ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(profile.PhoneNumber) && !string.IsNullOrWhiteSpace(phoneNumber))
                        profile.PhoneNumber = Regex.Replace(phoneNumber, @"\D", "");

                    if (string.IsNullOrWhiteSpace(profile.WhatsAppNumber) && !string.IsNullOrWhiteSpace(whatsAppNumber))
                        profile.WhatsAppNumber = Regex.Replace(whatsAppNumber, @"\D", "");

                    if (!profile.PrimaryDepartmentId.HasValue && primaryDepartmentId.HasValue)
                        profile.PrimaryDepartmentId = primaryDepartmentId;

                    if (!profile.PrimaryPositionId.HasValue && primaryPositionId.HasValue)
                        profile.PrimaryPositionId = primaryPositionId;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return new ResolvedWorkforceProfileResult(profile, IsNewProfile: false, IsExistingLinked: true);
                }
            }

            // Profil baru. Bila nomor identitas tidak diisi (Dokter/Pengguna Eksternal boleh tanpa
            // identitas), profil tetap dibuat tetapi tidak dapat dicocokkan dengan peran lain.
            var newProfile = new MstWorkforceProfile
            {
                Id = Guid.NewGuid(),
                ProfileCode = await AllocateProfileCodeAsync(actorUserId, cancellationToken),
                UserType = userType,
                DisplayName = fullName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : Regex.Replace(phoneNumber, @"\D", ""),
                WhatsAppNumber = string.IsNullOrWhiteSpace(whatsAppNumber) ? null : Regex.Replace(whatsAppNumber, @"\D", ""),
                PrimaryDepartmentId = primaryDepartmentId,
                PrimaryPositionId = primaryPositionId,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkforceProfile>().Add(newProfile);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ResolvedWorkforceProfileResult(newProfile, IsNewProfile: true, IsExistingLinked: false);
        }

        /// <summary>
        /// Mengantrekan resolusi untuk nomor identitas yang sama sampai transaksi pemanggil selesai.
        /// </summary>
        /// <remarks>
        /// Kunci diturunkan dari nomor identitas yang sudah dinormalisasi, sehingga resolusi untuk
        /// orang lain tidak ikut menunggu. Di luar PostgreSQL langkah ini dilewati.
        /// </remarks>
        private async Task AcquireIdentityLockAsync(string normalizedIdentity, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            var lockKey = $"WFP_IDENTITY_{normalizedIdentity}";

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        private async Task<string> AllocateProfileCodeAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            var existingCodes = await _dbContext.Set<MstWorkforceProfile>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ProfileCode.StartsWith(ProfileCodeLegacyPrefix))
                .Select(x => x.ProfileCode)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = existingCodes
                .Select(code => long.TryParse(code[ProfileCodeLegacyPrefix.Length..], out var number) ? number : (long?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .DefaultIfEmpty(0)
                .Max();

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ProfileCodeSequenceKey,
                    Prefix: ProfileCodePrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: ProfileCodeDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        private static IdentityLookupResult NotFound()
            => new(false, null, null, false, null, false, null, false, null, null, new List<string>());
    }
}
