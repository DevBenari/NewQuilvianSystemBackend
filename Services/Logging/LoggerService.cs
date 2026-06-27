using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Services.Logging
{
    public class LoggerService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<LoggerService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggerService(
            Microsoft.Extensions.Logging.ILogger<LoggerService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
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

        private Task WriteAsync(
            string level,
            string module,
            string action,
            string message,
            Exception? exception,
            object? data)
        {
            var now = DateTime.Now;
            var httpContext = _httpContextAccessor.HttpContext;

            var requestPath =
                GetValueFromData(data, "Path", "RequestPath") ??
                httpContext?.Request.Path.ToString() ??
                "-";

            var requestMethod =
                GetValueFromData(data, "Method", "RequestMethod") ??
                httpContext?.Request.Method ??
                "-";

            var ipAddress =
                GetValueFromData(data, "Ip", "IpAddress", "RemoteIpAddress") ??
                GetIpAddress(httpContext);

            var userAgent = httpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "-";
            var acceptLanguage = httpContext?.Request.Headers["Accept-Language"].FirstOrDefault() ?? "-";

            var browserInfo = ParseBrowser(userAgent);
            var osInfo = ParseOperatingSystem(userAgent);
            var deviceInfo = ParseDevice(userAgent);
            var clientInfo = $"{browserInfo} / {osInfo} / {deviceInfo}";

            var userId = GetClaimValue(httpContext, "user_id", ClaimTypes.NameIdentifier);
            var username = GetClaimValue(httpContext, "username", ClaimTypes.Name);
            var email = GetClaimValue(httpContext, "email", ClaimTypes.Email);

            userId = GetValueFromData(data, "UserId", "Id") ?? userId;
            username = GetValueFromData(data, "Username", "UserName", "Name") ?? username;
            email = GetValueFromData(data, "Email") ?? email;

            var eventName = BuildEventName(module, action, level);

            var cleanMessage = message;

            if (exception != null)
            {
                cleanMessage = $"{message} | Exception={exception.GetType().Name}: {exception.Message}";
            }

            var firstLine =
                $"{now:yyyy-MM-dd HH:mm:ss} " +
                $"[{level}] " +
                $"{eventName} | " +
                $"Module=\"{Sanitize(module)}\" " +
                $"Action=\"{Sanitize(action)}\" " +
                $"Message=\"{Sanitize(cleanMessage)}\" " +
                $"Path=\"{Sanitize(requestPath)}\"";

            var secondLine =
                $"UserId=\"{Sanitize(userId)}\" " +
                $"User=\"{Sanitize(username)}\" " +
                $"Email=\"{Sanitize(email)}\" " +
                $"Ip=\"{Sanitize(ipAddress)}\" " +
                $"Client=\"{Sanitize(clientInfo)}\" " +
                $"Lang=\"{Sanitize(acceptLanguage)}\"";

            // Satu baris agar enak dibaca di Grafana Loki.
            // Formatnya tetap mengikuti LogActivity_*.txt lama.
            var displayMessage = $"{firstLine} | ------> {secondLine}";

            var traceId = httpContext?.TraceIdentifier ?? "-";
            var logType = ResolveLogType(level, module, action);

            var scopeProperties = new Dictionary<string, object?>
            {
                ["DisplayMessage"] = displayMessage,
                ["LogType"] = logType,
                ["LogLevelCode"] = level,
                ["EventCode"] = eventName,
                ["Module"] = module,
                ["Action"] = action,
                ["LogMessage"] = cleanMessage,
                ["Path"] = requestPath,
                ["Method"] = requestMethod,
                ["UserId"] = userId,
                ["UserName"] = username,
                ["Email"] = email,
                ["Ip"] = ipAddress,
                ["Client"] = clientInfo,
                ["Browser"] = browserInfo,
                ["OperatingSystem"] = osInfo,
                ["Device"] = deviceInfo,
                ["Lang"] = acceptLanguage,
                ["TraceId"] = traceId
            };

            if (exception != null)
            {
                scopeProperties["ExceptionType"] = exception.GetType().Name;
                scopeProperties["ExceptionMessage"] = exception.Message;
            }

            using var scope = _logger.BeginScope(scopeProperties);

            if (level == "ERR")
            {
                if (exception != null)
                {
                    _logger.LogError(exception, "{DisplayMessage}", displayMessage);
                }
                else
                {
                    _logger.LogError("{DisplayMessage}", displayMessage);
                }
            }
            else if (level == "WRN")
            {
                _logger.LogWarning("{DisplayMessage}", displayMessage);
            }
            else
            {
                _logger.LogInformation("{DisplayMessage}", displayMessage);
            }

            return Task.CompletedTask;
        }

        private static string ResolveLogType(string level, string module, string action)
        {
            if (level == "ERR")
            {
                return "SystemError";
            }

            if (level == "AUD")
            {
                return "Audit";
            }

            if (string.Equals(module, "Auth", StringComparison.OrdinalIgnoreCase))
            {
                return "Auth";
            }

            if (string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase))
            {
                return "Auth";
            }

            return "UserAction";
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

            if (data is IDictionary<string, object?> dictionary)
            {
                foreach (var propertyName in propertyNames)
                {
                    foreach (var item in dictionary)
                    {
                        if (!string.Equals(item.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var text = item.Value?.ToString();

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            return text;
                        }
                    }
                }
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
    }
}
