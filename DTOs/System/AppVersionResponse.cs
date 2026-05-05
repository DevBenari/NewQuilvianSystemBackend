namespace QuilvianSystemBackend.DTOs.System
{
    public class AppVersionResponse
    {
        public string AppName { get; set; } = string.Empty;

        public string BackendVersion { get; set; } = string.Empty;

        public string ApiVersion { get; set; } = string.Empty;

        public string? FrontendMinimumVersion { get; set; }

        public string? FrontendRecommendedVersion { get; set; }

        public string? ReleaseName { get; set; }

        public string? Description { get; set; }

        public DateTime ReleaseDateTime { get; set; }

        public DateTime ServerDateTime { get; set; }
    }
}
