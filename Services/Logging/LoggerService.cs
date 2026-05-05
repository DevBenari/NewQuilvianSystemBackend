using System.Reflection;

namespace QuilvianSystemBackend.Services.Logging
{
    public class LoggerService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggerService(
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task InfoAsync(string module, string action, string message, object? data = null)
        {
            return WriteAsync("INF", module, action, message, null, data);
        }

        public Task WarningAsync(string module, string action, string message, object? data = null)
        {
            return WriteAsync("WRN", module, action, message, null, data);
        }

        public Task ErrorAsync(string module, string action, string message, Exception? exception = null, object? data = null)
        {
            return WriteAsync("ERR", module, action, message, exception, data);
        }

        public Task AuditAsync(string module, string action, string message, object? data = null)
        {
            return WriteAsync("AUD", module, action, message, null, data);
        }

        private async Task WriteAsync(
    string level,
    string module,
    string action,
    string message,
    Exception? exception,
    object? data)
        {
            var now = DateTime.Now;
            var fileDateText = now.ToString("dd_MM_yyyy");

            var rootPath = _environment.ContentRootPath;
            var logDirectory = Path.Combine(rootPath, "Logs");

            Directory.CreateDirectory(logDirectory);

            var filePath = Path.Combine(logDirectory, $"LogActivity_{fileDateText}.txt");

            var httpContext = _httpContextAccessor.HttpContext;

            var path = httpContext?.Request.Path.ToString() ?? "-";
            var ipAddress = GetIpAddress(httpContext);

            var userAgent = httpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "-";
            var acceptLanguage = httpContext?.Request.Headers["Accept-Language"].FirstOrDefault() ?? "-";

            var browserInfo = ParseBrowser(userAgent);
            var osInfo = ParseOperatingSystem(userAgent);
            var deviceInfo = ParseDevice(userAgent);

            var userId = GetClaimValue(httpContext, "user_id", ClaimTypesNameIdentifier());
            var username = GetClaimValue(httpContext, "username", ClaimTypesName());
            var email = GetClaimValue(httpContext, "email", ClaimTypesEmail());

            userId = GetValueFromData(data, "UserId", "Id") ?? userId;
            username = GetValueFromData(data, "Username", "UserName") ?? username;
            email = GetValueFromData(data, "Email") ?? email;

            var eventName = BuildEventName(module, action, level);
            var clientInfo = $"{browserInfo} / {osInfo} / {deviceInfo}";

            var cleanMessage = message;

            if (exception != null)
            {
                cleanMessage = $"{message} | Exception={exception.GetType().Name}: {exception.Message}";
            }

            var separator = new string('=', 120);

            var firstLine =
                $"{now:yyyy-MM-dd HH:mm:ss} " +
                $"[{level}] " +
                $"{eventName} | " +
                $"Module=\"{Sanitize(module)}\" " +
                $"Action=\"{Sanitize(action)}\" " +
                $"Message=\"{Sanitize(cleanMessage)}\" " +
                $"Path=\"{Sanitize(path)}\"";

            var secondLine =
                $"------> " +
                $"UserId=\"{Sanitize(userId)}\" " +
                $"User=\"{Sanitize(username)}\" " +
                $"Email=\"{Sanitize(email)}\" " +
                $"Ip=\"{Sanitize(ipAddress)}\" " +
                $"Client=\"{Sanitize(clientInfo)}\" " +
                $"Lang=\"{Sanitize(acceptLanguage)}\"";

            var logText =
                firstLine + Environment.NewLine +
                secondLine + Environment.NewLine +
                separator + Environment.NewLine;

            await File.AppendAllTextAsync(filePath, logText);
        }

        private static string BuildEventName(string module, string action, string level)
        {
            var cleanModule = ToLogToken(module);
            var cleanAction = ToLogToken(action);

            if (string.IsNullOrWhiteSpace(cleanModule))
            {
                cleanModule = "APP";
            }

            if (string.IsNullOrWhiteSpace(cleanAction))
            {
                cleanAction = level;
            }

            return $"{cleanModule}_{cleanAction}";
        }

        private static string ToLogToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace(" ", "_")
                .Where(x => char.IsLetterOrDigit(x) || x == '_')
                .ToArray();

            return new string(chars).ToUpper();
        }

        private static string GetClaimValue(HttpContext? httpContext, params string[] claimTypes)
        {
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return "-";
            }

            foreach (var claimType in claimTypes)
            {
                var value = httpContext.User.FindFirst(claimType)?.Value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return "-";
        }

        private static string? GetValueFromData(object? data, params string[] propertyNames)
        {
            if (data == null)
            {
                return null;
            }

            var dataType = data.GetType();

            foreach (var propertyName in propertyNames)
            {
                var property = dataType.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(data);

                if (value == null)
                {
                    continue;
                }

                var text = value.ToString();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return null;
        }

        private static string GetIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null)
            {
                return "-";
            }

            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp;
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
        }

        private static string ParseBrowser(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent) || userAgent == "-")
            {
                return "-";
            }

            if (userAgent.Contains("Edg/"))
            {
                return "Microsoft Edge";
            }

            if (userAgent.Contains("OPR/") || userAgent.Contains("Opera"))
            {
                return "Opera";
            }

            if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edg/"))
            {
                return "Google Chrome";
            }

            if (userAgent.Contains("Firefox/"))
            {
                return "Mozilla Firefox";
            }

            if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/"))
            {
                return "Safari";
            }

            if (userAgent.Contains("PostmanRuntime"))
            {
                return "Postman";
            }

            if (userAgent.Contains("Swagger"))
            {
                return "Swagger";
            }

            return "Unknown Browser";
        }

        private static string ParseOperatingSystem(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent) || userAgent == "-")
            {
                return "-";
            }

            if (userAgent.Contains("Windows NT 10.0"))
            {
                return "Windows 10 / Windows 11";
            }

            if (userAgent.Contains("Windows"))
            {
                return "Windows";
            }

            if (userAgent.Contains("Android"))
            {
                return "Android";
            }

            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            {
                return "iOS / iPadOS";
            }

            if (userAgent.Contains("Mac OS X"))
            {
                return "macOS";
            }

            if (userAgent.Contains("Linux"))
            {
                return "Linux";
            }

            return "Unknown OS";
        }

        private static string ParseDevice(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent) || userAgent == "-")
            {
                return "-";
            }

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
            {
                return "Mobile";
            }

            if (userAgent.Contains("iPad") || userAgent.Contains("Tablet"))
            {
                return "Tablet";
            }

            return "Desktop";
        }

        private static string Sanitize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private static string ClaimTypesNameIdentifier()
        {
            return System.Security.Claims.ClaimTypes.NameIdentifier;
        }

        private static string ClaimTypesName()
        {
            return System.Security.Claims.ClaimTypes.Name;
        }

        private static string ClaimTypesEmail()
        {
            return System.Security.Claims.ClaimTypes.Email;
        }
    }
}