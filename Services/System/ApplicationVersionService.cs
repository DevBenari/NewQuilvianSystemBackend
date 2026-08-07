using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Services.System
{
    public class ApplicationVersionService
    {
        private const string UnknownCommitSha = "unknown";

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ApplicationVersionService> _logger;
        private readonly ApplicationVersionInfoResponse _currentVersion;

        public ApplicationVersionService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            ILogger<ApplicationVersionService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
            _currentVersion = LoadCurrentVersion();
        }

        public ApplicationVersionInfoResponse GetCurrentVersion() => _currentVersion;

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

        public async Task RegisterCurrentBuildAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                var metadata = _currentVersion;
                var apiVersion = _configuration["AppInfo:ApiVersion"] ?? "v1";
                var release = await _dbContext.SysAppVersions.FirstOrDefaultAsync(
                    x => x.AppName == metadata.Application &&
                         x.BackendVersion == metadata.ReleaseVersion &&
                         !x.IsDelete,
                    cancellationToken);

                if (release == null)
                {
                    release = new SysAppVersion
                    {
                        Id = Guid.NewGuid(),
                        AppName = metadata.Application,
                        BackendVersion = metadata.ReleaseVersion,
                        ApiVersion = apiVersion,
                        FrontendMinimumVersion = _configuration["AppInfo:FrontendMinimumVersion"],
                        FrontendRecommendedVersion = _configuration["AppInfo:FrontendRecommendedVersion"],
                        ReleaseName = _configuration["AppInfo:ReleaseName"],
                        Description = _configuration["AppInfo:Description"],
                        IsLatest = true,
                        IsActive = true,
                        ReleaseDateTime = metadata.BuildDate ?? DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = Guid.Empty
                    };
                    _dbContext.SysAppVersions.Add(release);
                }
                else
                {
                    var frontendMinimumVersion = _configuration["AppInfo:FrontendMinimumVersion"];
                    var frontendRecommendedVersion = _configuration["AppInfo:FrontendRecommendedVersion"];
                    var releaseName = _configuration["AppInfo:ReleaseName"];
                    var description = _configuration["AppInfo:Description"];
                    var releaseChanged = release.ApiVersion != apiVersion ||
                                         release.FrontendMinimumVersion != frontendMinimumVersion ||
                                         release.FrontendRecommendedVersion != frontendRecommendedVersion ||
                                         release.ReleaseName != releaseName ||
                                         release.Description != description ||
                                         !release.IsLatest ||
                                         !release.IsActive;

                    if (releaseChanged)
                    {
                        release.ApiVersion = apiVersion;
                        release.FrontendMinimumVersion = frontendMinimumVersion;
                        release.FrontendRecommendedVersion = frontendRecommendedVersion;
                        release.ReleaseName = releaseName;
                        release.Description = description;
                        release.IsLatest = true;
                        release.IsActive = true;
                        release.UpdateDateTime = DateTime.UtcNow;
                    }
                }

                if (string.Equals(metadata.Branch, "master", StringComparison.OrdinalIgnoreCase))
                {
                    var mergeCommitSha = IsKnownCommit(metadata.CommitSha)
                        ? metadata.CommitSha
                        : release.MergeCommitSha;
                    var sourceBranch = release.SourceBranch ?? GetEnvironmentValue("APP_SOURCE_BRANCH");
                    var targetBranch = GetEnvironmentValue("APP_TARGET_BRANCH") ?? metadata.Branch;
                    var pullRequestNumber = release.PullRequestNumber ??
                                            ParseNullableInt(GetEnvironmentValue("APP_PULL_REQUEST_NUMBER"));

                    if (release.MergeCommitSha != mergeCommitSha ||
                        release.SourceBranch != sourceBranch ||
                        release.TargetBranch != targetBranch ||
                        release.PullRequestNumber != pullRequestNumber)
                    {
                        release.MergeCommitSha = mergeCommitSha;
                        release.SourceBranch = sourceBranch;
                        release.TargetBranch = targetBranch;
                        release.PullRequestNumber = pullRequestNumber;
                        release.UpdateDateTime = DateTime.UtcNow;
                    }
                }

                var oldLatestReleases = await _dbContext.SysAppVersions
                    .Where(x => x.Id != release.Id && x.IsLatest && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var oldRelease in oldLatestReleases)
                {
                    oldRelease.IsLatest = false;
                    oldRelease.UpdateDateTime = DateTime.UtcNow;
                }

                if (ShouldPersistBuild(metadata))
                {
                    var buildExists = await _dbContext.SysAppVersionBuilds.AnyAsync(
                        x => x.AppVersionId == release.Id &&
                             !x.IsDelete &&
                             (x.BuildVersion == metadata.BuildVersion || x.CommitSha == metadata.CommitSha),
                        cancellationToken);

                    if (!buildExists)
                    {
                        _dbContext.SysAppVersionBuilds.Add(new SysAppVersionBuild
                        {
                            Id = Guid.NewGuid(),
                            AppVersionId = release.Id,
                            BuildVersion = metadata.BuildVersion,
                            BuildNumber = metadata.BuildNumber,
                            CommitSha = metadata.CommitSha,
                            CommitMessage = GetEnvironmentValue("APP_COMMIT_MESSAGE"),
                            BranchName = metadata.Branch,
                            BuildDateTime = metadata.BuildDate ?? DateTime.UtcNow,
                            CreateDateTime = DateTime.UtcNow,
                            CreateBy = Guid.Empty
                        });
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Application release {ReleaseVersion} build {BuildVersion} registration completed.",
                    metadata.ReleaseVersion,
                    metadata.BuildVersion);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Application version history registration failed. Application startup will continue.");
            }
        }

        private ApplicationVersionInfoResponse LoadCurrentVersion()
        {
            var releaseVersion = GetEnvironmentValue("APP_RELEASE_VERSION")
                ?? LoadReleaseVersionFromFile()
                ?? _configuration["AppInfo:BackendVersion"]
                ?? "0.0.0";
            var buildVersion = GetEnvironmentValue("APP_BUILD_VERSION") ?? $"{releaseVersion}-local";

            return new ApplicationVersionInfoResponse
            {
                Application = _configuration["AppInfo:Name"] ?? "Quilvian System Backend",
                ReleaseVersion = releaseVersion,
                BuildVersion = buildVersion,
                BuildNumber = ParseBuildNumber(GetEnvironmentValue("APP_BUILD_NUMBER")),
                CommitSha = GetEnvironmentValue("APP_COMMIT_SHA") ?? UnknownCommitSha,
                Branch = GetEnvironmentValue("APP_BRANCH"),
                Framework = $".NET {Environment.Version.Major}",
                Environment = _hostEnvironment.EnvironmentName,
                BuildDate = ParseBuildDate(GetEnvironmentValue("APP_BUILD_DATE"))
            };
        }

        private string? LoadReleaseVersionFromFile()
        {
            try
            {
                var path = Path.Combine(_hostEnvironment.ContentRootPath, "version.json");
                if (!File.Exists(path))
                {
                    _logger.LogWarning(
                        "Version source file {VersionFile} was not found. Falling back to AppInfo:BackendVersion.",
                        path);
                    return null;
                }

                var version = JsonSerializer.Deserialize<VersionFile>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (version == null || version.Major < 0 || version.Minor < 0 || version.Patch < 0)
                {
                    _logger.LogWarning(
                        "Version source file {VersionFile} is invalid. Falling back to AppInfo:BackendVersion.",
                        path);
                    return null;
                }

                return $"{version.Major}.{version.Minor}.{version.Patch}";
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Version source file could not be read. Falling back to AppInfo:BackendVersion.");
                return null;
            }
        }

        private static string? GetEnvironmentValue(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static long ParseBuildNumber(string? value)
        {
            return long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number >= 0
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

        private static bool ShouldPersistBuild(ApplicationVersionInfoResponse metadata)
        {
            return IsKnownCommit(metadata.CommitSha) &&
                   !metadata.BuildVersion.EndsWith("-local", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownCommit(string commitSha)
        {
            return !string.IsNullOrWhiteSpace(commitSha) &&
                   !string.Equals(commitSha, UnknownCommitSha, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class VersionFile
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }
        }
    }
}
