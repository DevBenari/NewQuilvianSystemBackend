namespace QuilvianSystemBackend.DTOs.Auth
{
    public class FingerprintCandidateResponse
    {
        public Guid CredentialId { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string FingerPosition { get; set; } = string.Empty;
        public string TemplateFormat { get; set; } = string.Empty;
        public string? TemplateVersion { get; set; }
        public string FingerprintTemplateBase64 { get; set; } = string.Empty;
    }
}
