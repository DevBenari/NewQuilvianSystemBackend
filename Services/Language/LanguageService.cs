using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Services.Language
{
    public class LanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetMessage(string key)
        {
            var language = GetCurrentLanguage();

            return language switch
            {
                "en" => GetEnglishMessage(key),
                _ => GetIndonesianMessage(key)
            };
        }

        private string GetCurrentLanguage()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var language =
                httpContext?.Request.Headers["X-Language"].FirstOrDefault()
                ?? httpContext?.Request.Headers["Accept-Language"].FirstOrDefault()
                ?? "id";

            language = language.ToLower();

            if (language.StartsWith("en"))
            {
                return "en";
            }

            return "id";
        }

        private static string GetIndonesianMessage(string key)
        {
            return key switch
            {
                MessageKeys.AuthLoginSuccess => "Login berhasil.",
                MessageKeys.AuthInvalidCredential => "Email atau password salah.",
                MessageKeys.AuthEmailRequired => "Email wajib diisi.",
                MessageKeys.AuthPasswordRequired => "Password wajib diisi.",
                MessageKeys.AuthAccountInactive => "Akun tidak aktif.",
                MessageKeys.AuthAccessExpired => "Masa akses akun sudah berakhir.",
                MessageKeys.AuthAccountLocked => "Akun terkunci sementara karena terlalu banyak percobaan login gagal.",
                MessageKeys.AuthTokenInvalid => "Token tidak valid.",
                MessageKeys.AuthUserNotFound => "User tidak ditemukan.",
                MessageKeys.AuthSessionInvalid => "Session tidak valid.",
                MessageKeys.AuthSessionRefreshed => "Session diperpanjang.",
                MessageKeys.AuthLogoutSuccess => "Logout berhasil.",
                _ => key
            };
        }

        private static string GetEnglishMessage(string key)
        {
            return key switch
            {
                MessageKeys.AuthLoginSuccess => "Login successful.",
                MessageKeys.AuthInvalidCredential => "Invalid email or password.",
                MessageKeys.AuthEmailRequired => "Email is required.",
                MessageKeys.AuthPasswordRequired => "Password is required.",
                MessageKeys.AuthAccountInactive => "Account is inactive.",
                MessageKeys.AuthAccessExpired => "Account access period has expired.",
                MessageKeys.AuthAccountLocked => "Account is temporarily locked due to too many failed login attempts.",
                MessageKeys.AuthTokenInvalid => "Invalid token.",
                MessageKeys.AuthUserNotFound => "User not found.",
                MessageKeys.AuthSessionInvalid => "Invalid session.",
                MessageKeys.AuthSessionRefreshed => "Session refreshed.",
                MessageKeys.AuthLogoutSuccess => "Logout successful.",
                _ => key
            };
        }
    }
}
