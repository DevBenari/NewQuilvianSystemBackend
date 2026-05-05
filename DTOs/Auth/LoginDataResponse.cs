namespace QuilvianSystemBackend.DTOs.Auth
{
    public class LoginDataResponse
    {
        public AuthInfoResponse Auth { get; set; } = new();

        public AuthEndpointResponse Endpoints { get; set; } = new();

        public UserLoginResponse User { get; set; } = new();
    }
}
