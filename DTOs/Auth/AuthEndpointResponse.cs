namespace QuilvianSystemBackend.DTOs.Auth
{
    public class AuthEndpointResponse
    {
        public string Me { get; set; } = "/api/v1/Auth/me";

        public string Refresh { get; set; } = "/api/v1/Auth/refresh";

        public string Logout { get; set; } = "/api/v1/Auth/logout";
    }
}
