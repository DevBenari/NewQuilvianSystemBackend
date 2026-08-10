namespace QuilvianSystemBackend.DTOs.System
{
    public class ApplicationVersionInfoResponse
    {
        public string Application { get; set; } = string.Empty;
        public string ReleaseVersion { get; set; } = string.Empty;
        public string BuildVersion { get; set; } = string.Empty;
        public long BuildNumber { get; set; }
        public string CommitSha { get; set; } = "unknown";
        public string? Branch { get; set; }
        public string Framework { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime? BuildDate { get; set; }
    }

    public class ApplicationReleaseHistoryResponse
    {
        public string Application { get; set; } = string.Empty;
        public string ReleaseVersion { get; set; } = string.Empty;
        public string? ReleaseName { get; set; }
        public string? Description { get; set; }
        public string? MergeCommitSha { get; set; }
        public string? SourceBranch { get; set; }
        public string? TargetBranch { get; set; }
        public int? PullRequestNumber { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<ApplicationBuildHistoryResponse> Builds { get; set; } = new();
    }

    public class ApplicationBuildHistoryResponse
    {
        public string BuildVersion { get; set; } = string.Empty;
        public long BuildNumber { get; set; }
        public string CommitSha { get; set; } = string.Empty;
        public string? Branch { get; set; }
        public DateTime BuildDate { get; set; }
    }
}
