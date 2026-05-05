using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Seeders
{
    public static class AppVersionSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var appName = configuration["AppInfo:Name"] ?? "Quilvian System Backend";
            var backendVersion = configuration["AppInfo:BackendVersion"] ?? "1.0.0";
            var apiVersion = configuration["AppInfo:ApiVersion"] ?? "v1";
            var frontendMinimumVersion = configuration["AppInfo:FrontendMinimumVersion"] ?? "1.0.0";
            var frontendRecommendedVersion = configuration["AppInfo:FrontendRecommendedVersion"] ?? "1.0.0";
            var releaseName = configuration["AppInfo:ReleaseName"] ?? "Initial Development Version";
            var description = configuration["AppInfo:Description"] ?? "Initial backend version.";

            var existingVersion = await dbContext.SysAppVersions
                .FirstOrDefaultAsync(x =>
                    x.AppName == appName &&
                    x.BackendVersion == backendVersion &&
                    x.ApiVersion == apiVersion &&
                    !x.IsDelete);

            if (existingVersion != null)
            {
                var hasChanges =
                    existingVersion.FrontendMinimumVersion != frontendMinimumVersion ||
                    existingVersion.FrontendRecommendedVersion != frontendRecommendedVersion ||
                    existingVersion.ReleaseName != releaseName ||
                    existingVersion.Description != description ||
                    !existingVersion.IsLatest ||
                    !existingVersion.IsActive;

                if (!hasChanges)
                {
                    return;
                }

                var latestVersions = await dbContext.SysAppVersions
                    .Where(x => x.IsLatest && !x.IsDelete)
                    .ToListAsync();

                foreach (var version in latestVersions)
                {
                    version.IsLatest = false;
                    version.UpdateDateTime = DateTime.UtcNow;
                }

                existingVersion.FrontendMinimumVersion = frontendMinimumVersion;
                existingVersion.FrontendRecommendedVersion = frontendRecommendedVersion;
                existingVersion.ReleaseName = releaseName;
                existingVersion.Description = description;
                existingVersion.IsLatest = true;
                existingVersion.IsActive = true;
                existingVersion.UpdateDateTime = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                return;
            }

            var oldLatestVersions = await dbContext.SysAppVersions
                .Where(x => x.IsLatest && !x.IsDelete)
                .ToListAsync();

            foreach (var version in oldLatestVersions)
            {
                version.IsLatest = false;
                version.UpdateDateTime = DateTime.UtcNow;
            }

            var appVersion = new SysAppVersion
            {
                Id = Guid.NewGuid(),
                AppName = appName,
                BackendVersion = backendVersion,
                ApiVersion = apiVersion,
                FrontendMinimumVersion = frontendMinimumVersion,
                FrontendRecommendedVersion = frontendRecommendedVersion,
                ReleaseName = releaseName,
                Description = description,
                IsLatest = true,
                IsActive = true,
                ReleaseDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Guid.Empty,
                UpdateBy = Guid.Empty,
                DeleteBy = Guid.Empty,
                CancelBy = Guid.Empty,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.SysAppVersions.Add(appVersion);

            await dbContext.SaveChangesAsync();
        }
    }
}