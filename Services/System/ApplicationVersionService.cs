using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Services.System
{
    public class ApplicationVersionService
    {
        private const int AuthoritativeVersioningGeneration = 2;
        private const string UnknownCommitSha = "unknown";

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ApplicationVersionService> _logger;
        private readonly BackendVersionManifest _manifest;

        public ApplicationVersionService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            ILogger<ApplicationVersionService> logger,
            BackendVersionManifest manifest)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _manifest = manifest;
        }

        public ApplicationVersionInfoResponse GetRuntimeVersionInfo()
        {
            var buildMetadata = LoadBuildMetadata();
            return new ApplicationVersionInfoResponse
            {
                Application = ReadRequiredConfiguration("AppInfo:Name"),
                ReleaseVersion = _manifest.BackendVersion,
                BuildVersion = buildMetadata.BuildVersion ?? string.Empty,
                BuildNumber = buildMetadata.BuildNumber,
                CommitSha = buildMetadata.CommitSha,
                Branch = buildMetadata.Branch,
                Framework = $".NET {Environment.Version.Major}",
                Environment = _hostEnvironment.EnvironmentName,
                BuildDate = buildMetadata.BuildDate
            };
        }

        public async Task<AppVersionResponse> GetCurrentVersionAsync(
            CancellationToken cancellationToken = default)
        {
            var appName = ReadRequiredConfiguration("AppInfo:Name");
            SysAppVersion? currentVersion;

            try
            {
                currentVersion = await _dbContext.SysAppVersions
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.AppName == appName &&
                             x.VersioningGeneration == AuthoritativeVersioningGeneration &&
                             x.IsLatest &&
                             x.IsActive &&
                             !x.IsDelete,
                        cancellationToken);
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogCritical(
                    exception,
                    "Application version invariant is violated for {Application}: multiple current Versioning V2 rows exist.",
                    appName);
                throw;
            }

            if (currentVersion == null)
            {
                var exception = new InvalidOperationException(
                    $"Current Versioning V2 record was not found for application '{appName}'.");
                _logger.LogCritical(exception, "Current authoritative application version is unavailable.");
                throw exception;
            }

            if (string.IsNullOrWhiteSpace(currentVersion.MinimumSupportedFrontendVersion))
            {
                var exception = new InvalidOperationException(
                    $"Current Versioning V2 record for '{appName}' has no minimum supported frontend version.");
                _logger.LogCritical(exception, "Current authoritative application version is invalid.");
                throw exception;
            }

            return new AppVersionResponse
            {
                BackendVersion = currentVersion.BackendVersion,
                ApiVersion = currentVersion.ApiVersion,
                MinimumSupportedFrontendVersion = currentVersion.MinimumSupportedFrontendVersion
            };
        }

        public async Task<PagedResult<ApplicationReleaseHistoryResponse>> GetHistoryAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _dbContext.SysAppVersions
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.ReleaseDateTime)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApplicationReleaseHistoryResponse
                {
                    Application = x.AppName,
                    ReleaseVersion = x.BackendVersion,
                    ReleaseName = x.ReleaseName,
                    Description = x.Description,
                    MergeCommitSha = x.MergeCommitSha,
                    SourceBranch = x.SourceBranch,
                    TargetBranch = x.TargetBranch,
                    PullRequestNumber = x.PullRequestNumber,
                    ReleaseDate = x.ReleaseDateTime,
                    Builds = x.Builds
                        .Where(build => !build.IsDelete)
                        .OrderByDescending(build => build.BuildNumber)
                        .ThenByDescending(build => build.BuildDateTime)
                        .Select(build => new ApplicationBuildHistoryResponse
                        {
                            BuildVersion = build.BuildVersion,
                            BuildNumber = build.BuildNumber,
                            CommitSha = build.CommitSha,
                            Branch = build.BranchName,
                            BuildDate = build.BuildDateTime
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ApplicationReleaseHistoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task RegisterCurrentVersionAsync(CancellationToken cancellationToken = default)
        {
            var appName = ReadRequiredConfiguration("AppInfo:Name");
            var apiVersion = ReadRequiredConfiguration("AppInfo:ApiVersion");
            var minimumSupportedFrontendVersion =
                ReadRequiredConfiguration("FrontendCompatibility:MinimumSupportedVersion");

            if (!BackendVersionManifest.IsValidSemanticVersion(minimumSupportedFrontendVersion))
            {
                throw new InvalidOperationException(
                    "FrontendCompatibility:MinimumSupportedVersion must use MAJOR.MINOR.PATCH format.");
            }

            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                if (_dbContext.Database.IsNpgsql())
                {
                    await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $"SELECT pg_advisory_xact_lock(hashtext({appName})::bigint)",
                        cancellationToken);
                }

                var release = await _dbContext.SysAppVersions.SingleOrDefaultAsync(
                    x => x.AppName == appName &&
                         x.VersioningGeneration == AuthoritativeVersioningGeneration &&
                         x.BackendVersion == _manifest.BackendVersion &&
                         !x.IsDelete,
                    cancellationToken);

                var releaseId = release?.Id ?? Guid.NewGuid();

                await _dbContext.SysAppVersions
                    .Where(x => x.AppName == appName && x.Id != releaseId && x.IsLatest)
                    .ExecuteUpdateAsync(
                        updates => updates
                            .SetProperty(x => x.IsLatest, false)
                            .SetProperty(x => x.UpdateDateTime, DateTime.UtcNow),
                        cancellationToken);

                if (release == null)
                {
                    release = new SysAppVersion
                    {
                        Id = releaseId,
                        AppName = appName,
                        BackendVersion = _manifest.BackendVersion,
                        VersioningGeneration = AuthoritativeVersioningGeneration,
                        ReleaseDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = Guid.Empty
                    };
                    _dbContext.SysAppVersions.Add(release);
                }

                release.ApiVersion = apiVersion;
                release.MinimumSupportedFrontendVersion = minimumSupportedFrontendVersion;
                release.LegacyFrontendRecommendedVersion = null;
                release.ReleaseName = _manifest.ReleaseName;
                release.Description = _manifest.Description;
                release.IsLatest = true;
                release.IsActive = true;
                release.IsDelete = false;
                release.UpdateDateTime = release.UpdateDateTime ?? DateTime.UtcNow;

                var buildMetadata = LoadBuildMetadata();
                ApplyReliableReleaseMetadata(release, buildMetadata);

                if (ShouldPersistBuild(buildMetadata))
                {
                    var buildExists = await _dbContext.SysAppVersionBuilds.AnyAsync(
                        x => x.AppVersionId == release.Id &&
                             !x.IsDelete &&
                             (x.BuildVersion == buildMetadata.BuildVersion ||
                              x.CommitSha == buildMetadata.CommitSha),
                        cancellationToken);

                    if (!buildExists)
                    {
                        _dbContext.SysAppVersionBuilds.Add(new SysAppVersionBuild
                        {
                            Id = Guid.NewGuid(),
                            AppVersionId = release.Id,
                            BuildVersion = buildMetadata.BuildVersion!,
                            BuildNumber = buildMetadata.BuildNumber,
                            CommitSha = buildMetadata.CommitSha,
                            CommitMessage = GetEnvironmentValue("APP_COMMIT_MESSAGE"),
                            BranchName = buildMetadata.Branch,
                            BuildDateTime = buildMetadata.BuildDate ?? DateTime.UtcNow,
                            CreateDateTime = DateTime.UtcNow,
                            CreateBy = Guid.Empty
                        });
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "Skipping application build history for {BackendVersion}: reliable CommitSha, BuildNumber, and BuildVersion metadata are required.",
                        _manifest.BackendVersion);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Authoritative application version {BackendVersion} (generation {VersioningGeneration}) registration completed.",
                    _manifest.BackendVersion,
                    AuthoritativeVersioningGeneration);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(
                    exception,
                    "Authoritative application version registration failed. Application startup cannot continue.");
                throw;
            }
        }

        private void ApplyReliableReleaseMetadata(SysAppVersion release, BuildMetadata metadata)
        {
            if (!IsKnownCommit(metadata.CommitSha))
            {
                return;
            }

            release.MergeCommitSha = metadata.CommitSha;
            release.SourceBranch ??= GetEnvironmentValue("APP_SOURCE_BRANCH");
            release.TargetBranch = GetEnvironmentValue("APP_TARGET_BRANCH") ?? metadata.Branch;
            release.PullRequestNumber ??=
                ParseNullableInt(GetEnvironmentValue("APP_PULL_REQUEST_NUMBER"));
        }

        private string ReadRequiredConfiguration(string key)
        {
            var value = _configuration[key]?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Required application configuration '{key}' is missing.");
            }

            return value;
        }

        private static BuildMetadata LoadBuildMetadata()
        {
            return new BuildMetadata
            {
                BuildVersion = GetEnvironmentValue("APP_BUILD_VERSION"),
                BuildNumber = ParseBuildNumber(GetEnvironmentValue("APP_BUILD_NUMBER")),
                CommitSha = GetEnvironmentValue("APP_COMMIT_SHA") ?? UnknownCommitSha,
                Branch = GetEnvironmentValue("APP_BRANCH"),
                BuildDate = ParseBuildDate(GetEnvironmentValue("APP_BUILD_DATE"))
            };
        }

        private static string? GetEnvironmentValue(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static long ParseBuildNumber(string? value)
        {
            return long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0
                ? number
                : 0;
        }

        private static int? ParseNullableInt(string? value)
        {
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0
                ? number
                : null;
        }

        private static DateTime? ParseBuildDate(string? value)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var buildDate)
                ? buildDate.UtcDateTime
                : null;
        }

        private static bool ShouldPersistBuild(BuildMetadata metadata)
        {
            return IsKnownCommit(metadata.CommitSha) &&
                   metadata.BuildNumber > 0 &&
                   !string.IsNullOrWhiteSpace(metadata.BuildVersion);
        }

        private static bool IsKnownCommit(string commitSha)
        {
            return !string.IsNullOrWhiteSpace(commitSha) &&
                   !string.Equals(commitSha, UnknownCommitSha, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class BuildMetadata
        {
            public string? BuildVersion { get; init; }
            public long BuildNumber { get; init; }
            public string CommitSha { get; init; } = UnknownCommitSha;
            public string? Branch { get; init; }
            public DateTime? BuildDate { get; init; }
        }
    }
}
