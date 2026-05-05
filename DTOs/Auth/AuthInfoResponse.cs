namespace QuilvianSystemBackend.DTOs.Auth
{
    public class AuthInfoResponse
    {
        public string Scheme { get; set; } = "Cookie";

        public string CookieName { get; set; } = string.Empty;

        public bool IsHttpOnly { get; set; } = true;

        public string SameSite { get; set; } = string.Empty;

        public bool Secure { get; set; }

        public int ExpiresInMinutes { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public string FrontendInstruction { get; set; } = string.Empty;
    }
}
