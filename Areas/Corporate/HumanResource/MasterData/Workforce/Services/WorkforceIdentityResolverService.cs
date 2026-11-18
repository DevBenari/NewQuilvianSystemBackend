using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    public class WorkforceIdentityResolverService : IWorkforceIdentityResolverService
    {
        private const string HospitalCode = "RSMMC";
        private readonly ApplicationDbContext _dbContext;

        public WorkforceIdentityResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string NormalizeIdentityNumber(string? identityType, string? identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                return string.Empty;

            var trimmed = identityNumber.Trim();
            var type = (identityType ?? string.Empty).Trim().ToUpperInvariant();

            if (type == "PASPOR" || type == "PASSPORT")
            {
                return Regex.Replace(trimmed, @"\s+", "").ToUpperInvariant();
            }

            // Default to NIK / KTP (digits only)
            return Regex.Replace(trimmed, @"\D", "");
        }

        public bool IsValidIdentity(string? identityType, string? identityNumber, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                errorMessage = "Nomor identitas wajib diisi.";
                return false;
            }

            var type = (identityType ?? string.Empty).Trim().ToUpperInvariant();
            var normalized = NormalizeIdentityNumber(identityType, identityNumber);

            if (type == "PASPOR" || type == "PASSPORT")
            {
                if (normalized.Length < 4)
                {
                    errorMessage = "Nomor Paspor tidak valid (minimal 4 karakter).";
                    return false;
                }
                errorMessage = null;
                return true;
            }

            // NIK WNI harus tepat 16 digit
            if (normalized.Length != 16)
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
                return new IdentityLookupResult(false, null, null, false, null, false, null, false, null, null, new List<string>());
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
                return new IdentityLookupResult(false, null, null, false, null, false, null, false, null, null, new List<string>());
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
            var normalized = NormalizeIdentityNumber(identityType, identityNumber);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new ArgumentException("Nomor identitas tidak boleh kosong.");
            }

            var match = await FindByIdentityAsync(identityType, identityNumber, cancellationToken);

            if (match.Found && match.WorkforceProfile != null)
            {
                // Check duplicate role
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

                // Tautkan peran ke workforce profile yang sama!
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

            // Buat workforce profile baru secara atomik
            var newProfile = new MstWorkforceProfile
            {
                Id = Guid.NewGuid(),
                ProfileCode = await GenerateWorkforceProfileCodeAsync(cancellationToken),
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

        private async Task<string> GenerateWorkforceProfileCodeAsync(CancellationToken cancellationToken)
        {
            const string prefix = $"WFP-{HospitalCode}-";
            var totalData = await _dbContext.Set<MstWorkforceProfile>()
                .IgnoreQueryFilters()
                .CountAsync(cancellationToken);

            var nextNumber = totalData + 1;
            while (true)
            {
                var candidate = $"{prefix}{nextNumber:D6}";
                var exists = await _dbContext.Set<MstWorkforceProfile>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.ProfileCode == candidate, cancellationToken);

                if (!exists)
                    return candidate;

                nextNumber++;
            }
        }
    }
}
