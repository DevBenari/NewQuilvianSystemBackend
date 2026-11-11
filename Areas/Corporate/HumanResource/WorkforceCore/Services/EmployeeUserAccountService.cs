using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class EmployeeUserAccountService : IEmployeeUserAccountService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeUserAccountService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message, ApplicationUser? User, string? InitialPassword)> CreateForWorkforceProfileAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            bool isFingerprintRegistrationEnabled = false,
            string? fingerprintRegistrationReason = null,
            CancellationToken cancellationToken = default)
        {
            var workforceProfile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete, cancellationToken);

            if (workforceProfile == null)
            {
                return (false, "Profil tenaga kerja tidak ditemukan.", null, null);
            }

            var employee = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(e => e.WorkforceProfileId == workforceProfileId && !e.IsDelete, cancellationToken);

            var email = !string.IsNullOrWhiteSpace(employee?.Email)
                ? employee.Email.Trim().ToLower()
                : $"{workforceProfile.ProfileCode.ToLower()}@quilvian.local";

            var fullName = employee?.FullName ?? workforceProfile.DisplayName;
            var birthDate = employee?.BirthDate ?? new DateTime(1990, 1, 1);

            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail != null)
            {
                return (false, "Email sudah digunakan oleh akun login lain.", existingByEmail, null);
            }

            var existingByUserName = await _userManager.FindByNameAsync(email);
            if (existingByUserName != null)
            {
                return (false, "Username sudah digunakan oleh akun login lain.", existingByUserName, null);
            }

            var existingAccount = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.UserType == UserType.Employee, cancellationToken);

            if (existingAccount != null)
            {
                return (true, "Workforce profile ini sudah memiliki akun login.", existingAccount, null);
            }

            var initialPassword = $"Quilvian@{birthDate:ddMMyyyy}";
            var now = DateTime.UtcNow;

            var userCount = await _dbContext.Users.CountAsync(cancellationToken);
            var userCode = $"USR-{now:yyyyMM}-{(userCount + 1):D4}";

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserCode = userCode,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = fullName,
                UserType = UserType.Employee,
                WorkforceProfileId = workforceProfileId,
                EmployeeId = employee?.Id,
                DoctorId = null,
                ExternalUserId = null,
                PrimaryDepartmentId = employee?.PrimaryDepartmentId ?? Guid.Empty,
                PrimaryPositionId = employee?.PrimaryPositionId ?? Guid.Empty,
                IsGeolocationBypassEnabled = false,
                IsActive = false, // Mulai tidak aktif hingga hari pertama hadir (VR-ONB-10)
                MustChangePassword = true,
                CreateDateTime = now,
                IsFingerprintRegistrationEnabled = isFingerprintRegistrationEnabled,
                FingerprintRegistrationReason = isFingerprintRegistrationEnabled ? fingerprintRegistrationReason : null,
                FingerprintRegistrationEnabledAt = isFingerprintRegistrationEnabled ? now : null,
                FingerprintRegistrationEnabledByUserId = isFingerprintRegistrationEnabled ? actorUserId : null
            };

            var createResult = await _userManager.CreateAsync(user, initialPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
                return (false, $"Akun login gagal dibuat: {errors}", null, null);
            }

            return (true, "Akun login berhasil dibuat.", user, initialPassword);
        }

        public async Task<(bool Success, string Message)> ActivateAsync(
            Guid applicationUserId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(applicationUserId.ToString());
            if (user == null)
            {
                return (false, "Akun pengguna tidak ditemukan.");
            }

            user.IsActive = true;
            user.UpdateDateTime = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                return (false, $"Gagal mengaktifkan akun: {errors}");
            }

            // Update status pada HrdOnboardingAccessRequest bila ada
            var accessRequest = await _dbContext.HrdOnboardingAccessRequests
                .FirstOrDefaultAsync(r => r.ApplicationUserId == applicationUserId && !r.IsDelete, cancellationToken);

            if (accessRequest != null)
            {
                accessRequest.SelfServiceActivatedAt = DateTime.UtcNow;
                accessRequest.CompletedAt = DateTime.UtcNow;
                accessRequest.CompletedByUserId = actorUserId;
                accessRequest.RequestStatus = LifecycleManagement.Enums.OnboardingAccessRequestStatus.Completed;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return (true, "Akun berhasil diaktifkan untuk layanan mandiri.");
        }
    }
}
