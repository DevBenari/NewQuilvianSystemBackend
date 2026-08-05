using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services
{
    public class OvertimeSelfServiceEmployeeContext
    {
        public Guid UserId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class OvertimeSelfServiceContextService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimeSelfServiceContextService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OvertimeSelfServiceServiceResult<OvertimeSelfServiceEmployeeContext>> ResolveAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                return OvertimeSelfServiceServiceResult<OvertimeSelfServiceEmployeeContext>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Id,
                    x.Email
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return OvertimeSelfServiceServiceResult<OvertimeSelfServiceEmployeeContext>.Fail(
                    StatusCodes.Status404NotFound,
                    "Akun login belum memiliki email yang dapat dipetakan ke workforce profile.");
            }

            var normalizedEmail = user.Email.Trim().ToLower();

            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.Email != null &&
                    x.Email.ToLower() == normalizedEmail)
                .Select(x => new ProfileLookup
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    Email = x.Email
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
            {
                profile = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.IsActive &&
                        x.Email.ToLower() == normalizedEmail &&
                        x.WorkforceProfile != null &&
                        !x.WorkforceProfile.IsDelete &&
                        !x.WorkforceProfile.IsCancel &&
                        x.WorkforceProfile.IsActive)
                    .Select(x => new ProfileLookup
                    {
                        Id = x.WorkforceProfileId,
                        ProfileCode = x.WorkforceProfile!.ProfileCode,
                        DisplayName = x.WorkforceProfile.DisplayName,
                        Email = x.WorkforceProfile.Email
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (profile == null)
            {
                return OvertimeSelfServiceServiceResult<OvertimeSelfServiceEmployeeContext>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile untuk akun login tidak ditemukan atau tidak aktif.");
            }

            var employee = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == profile.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeCode,
                    x.FullName,
                    x.EmployeeCategoryId,
                    x.EmploymentTypeId
                })
                .FirstOrDefaultAsync(cancellationToken);

            return OvertimeSelfServiceServiceResult<OvertimeSelfServiceEmployeeContext>.Ok(
                new OvertimeSelfServiceEmployeeContext
                {
                    UserId = user.Id,
                    WorkforceProfileId = profile.Id,
                    WorkforceProfileCode = profile.ProfileCode,
                    WorkforceDisplayName = profile.DisplayName,
                    EmployeeId = employee?.Id,
                    EmployeeCode = employee?.EmployeeCode,
                    EmployeeName = employee?.FullName,
                    EmployeeCategoryId = employee?.EmployeeCategoryId,
                    EmploymentTypeId = employee?.EmploymentTypeId,
                    Email = user.Email
                },
                "Konteks employee self service berhasil diselesaikan.");
        }

        private sealed class ProfileLookup
        {
            public Guid Id { get; set; }
            public string ProfileCode { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string? Email { get; set; }
        }
    }
}
