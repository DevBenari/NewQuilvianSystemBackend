namespace QuilvianSystemBackend.DTOs.System
{
    public class AppVersionResponse
    {
        public string BackendVersion { get; set; } = string.Empty;

        public string ApiVersion { get; set; } = string.Empty;

        public string MinimumSupportedFrontendVersion { get; set; } = string.Empty;

        // Deprecated compatibility alias. Remove after the frontend adopts MinimumSupportedFrontendVersion.
        public string FrontendMinimumVersion => MinimumSupportedFrontendVersion;

        // Deprecated compatibility alias only; this is not a separate recommended-version policy.
        public string FrontendRecommendedVersion => MinimumSupportedFrontendVersion;
    }
}
