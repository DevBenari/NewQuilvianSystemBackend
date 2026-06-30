using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Middlewares;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using QuilvianSystemBackend.Services.Language;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        var configuration = context.Configuration;
        var environment = context.HostingEnvironment;

        var appName = configuration["AppInfo:Name"] ?? "Quilvian System Backend";
        var appVersion =
            configuration["AppInfo:BackendVersion"] ??
            configuration["AppInfo:Version"] ??
            "1.0.0";

        var serviceName = configuration["SerilogSettings:ServiceName"] ?? "quilvian-backend";
        var fileNamePattern = configuration["SerilogSettings:FileNamePattern"] ?? "quilvian-backend-.json";

        var configuredLogDirectory = configuration["SerilogSettings:LogDirectory"];
        var logDirectory = !string.IsNullOrWhiteSpace(configuredLogDirectory)
            ? configuredLogDirectory.Trim()
            : environment.IsProduction()
                ? "/app/logs"
                : Path.Combine(environment.ContentRootPath, "Logs");

        if (!Path.IsPathRooted(logDirectory))
        {
            logDirectory = Path.Combine(environment.ContentRootPath, logDirectory);
        }

        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, fileNamePattern);

        var minimumLevel = ParseLogEventLevel(
            configuration["SerilogSettings:MinimumLevel"],
            environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information
        );

        var microsoftLevel = ParseLogEventLevel(
            configuration["SerilogSettings:MicrosoftMinimumLevel"],
            LogEventLevel.Warning
        );

        var aspNetCoreLevel = ParseLogEventLevel(
            configuration["SerilogSettings:MicrosoftAspNetCoreMinimumLevel"],
            LogEventLevel.Warning
        );

        var systemLevel = ParseLogEventLevel(
            configuration["SerilogSettings:SystemMinimumLevel"],
            LogEventLevel.Warning
        );

        var retainedFileCountLimit = ParseInt(
            configuration["SerilogSettings:RetainedFileCountLimit"],
            environment.IsProduction() ? 30 : 7
        );

        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", microsoftLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", aspNetCoreLevel)
            .MinimumLevel.Override("System", systemLevel)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", appName)
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("BackendVersion", appVersion)
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedFileCountLimit,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1)
            );
    });

    // Paksa session/JWT/cookie expire 60 menit.
    // Nilai ini akan dibaca oleh AuthController lewat IConfiguration.
    const int AuthExpireMinutes = 60;
    builder.Configuration["Jwt:ExpireMinutes"] = AuthExpireMinutes.ToString();

    // Add services to the container.
    builder.Services.AddControllers();

    // SignalR untuk realtime antrean nurse station dan doctor queue.
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    });

    // Database PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")
        );
    });

    // ASP.NET Core Identity
    builder.Services
        .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Untuk tahap awal development.
            // Nanti production bisa diperketat.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = false;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });

    var allowedCorsOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendCorsPolicy", policy =>
        {
            policy
                .WithOrigins(allowedCorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // JWT Configuration
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("Jwt:Key belum dikonfigurasi di appsettings.json.");
    }

    var signingKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey)
    );

    var jwtCookieName = builder.Configuration["Jwt:CookieName"] ?? "quilvian_access_token";

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.SaveToken = true;

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token) &&
                        !string.IsNullOrWhiteSpace(token))
                    {
                        context.Token = token;
                        return Task.CompletedTask;
                    }

                    var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                    var requestPath = context.HttpContext.Request.Path;

                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        requestPath.StartsWithSegments("/hubs/queues"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<LanguageService>();
    builder.Services.AddScoped<LoggerService>();
    builder.Services.AddScoped<AccessPermissionService>();
    builder.Services.AddScoped<QueueVoiceService>();
    builder.Services.AddScoped<QueueRealtimeService>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("KioskRead", policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireAssertion(context =>
            {
                var user = context.User;

                return
                    user.IsInRole("SuperAdmin") ||
                    user.IsInRole("Administrator") ||
                    user.IsInRole("Kiosk") ||

                    // Kiosk berdasarkan UserType SystemUser
                    user.HasClaim("is_kiosk", "true") ||
                    user.HasClaim("user_type_id", "6") ||
                    user.HasClaim("user_type", "SystemUser") ||

                    // Kiosk berdasarkan hasil ResolveKioskLoginContextAsync
                    user.HasClaim("is_kiosk_account", "true") ||
                    user.HasClaim("profile_type", "KioskDevice");
            });
        });

        // Policy khusus akun display antrian.
        // Dipakai untuk endpoint runtime display supaya akun QueueDisplayDevice
        // tidak perlu lewat AccessPermission role/menu aplikasi umum.
        options.AddPolicy("QueueDisplayRuntimeRead", policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireAssertion(context =>
            {
                var user = context.User;

                return
                    user.IsInRole("SuperAdmin") ||
                    user.IsInRole("Administrator") ||
                    user.IsInRole("QueueDisplayDevice") ||
                    user.IsInRole("QueueDisplay") ||
                    user.IsInRole("DisplayQueue") ||

                    // Claim khusus dari AuthController saat login akun display antrian.
                    user.HasClaim("is_queue_display_account", "true") ||
                    user.HasClaim("profile_type", "QueueDisplayDevice") ||
                    user.HasClaim("queue_display_runtime_read", "true") ||
                    user.HasClaim("queue_display_read", "true") ||
                    user.HasClaim("can_access_queue_display_runtime", "true") ||

                    // Fallback claim identitas perangkat display.
                    user.HasClaim(claim => claim.Type == "queue_display_device_id" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "display_device_id" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "queue_display_code" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "display_code" && !string.IsNullOrWhiteSpace(claim.Value));
            });
        });

        // Alias jika nanti ada controller lain yang ingin memakai nama policy lebih umum.
        options.AddPolicy("QueueDisplayRead", policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireAssertion(context =>
            {
                var user = context.User;

                return
                    user.IsInRole("SuperAdmin") ||
                    user.IsInRole("Administrator") ||
                    user.IsInRole("QueueDisplayDevice") ||
                    user.IsInRole("QueueDisplay") ||
                    user.IsInRole("DisplayQueue") ||
                    user.HasClaim("is_queue_display_account", "true") ||
                    user.HasClaim("profile_type", "QueueDisplayDevice") ||
                    user.HasClaim("queue_display_runtime_read", "true") ||
                    user.HasClaim("queue_display_read", "true") ||
                    user.HasClaim("can_access_queue_display_runtime", "true") ||
                    user.HasClaim(claim => claim.Type == "queue_display_device_id" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "display_device_id" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "queue_display_code" && !string.IsNullOrWhiteSpace(claim.Value)) ||
                    user.HasClaim(claim => claim.Type == "display_code" && !string.IsNullOrWhiteSpace(claim.Value));
            });
        });
    });

    builder.Services.AddDistributedMemoryCache();

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();

    var appName = builder.Configuration["AppInfo:Name"] ?? "Quilvian System Backend API";
    var appVersion =
        builder.Configuration["AppInfo:BackendVersion"] ??
        builder.Configuration["AppInfo:Version"] ??
        "1.0.0";
    var apiVersion = builder.Configuration["AppInfo:ApiVersion"] ?? "v1";

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("auth", new OpenApiInfo
        {
            Title = $"{appName} - Authentication",
            Version = apiVersion,
            Description = $"Application Version {appVersion}"
        });

        options.SwaggerDoc("administrator", new OpenApiInfo
        {
            Title = $"{appName} - Administrator",
            Version = apiVersion,
            Description = $"Application Version {appVersion}"
        });

        options.SwaggerDoc("corporate", new OpenApiInfo
        {
            Title = $"{appName} - Corporate",
            Version = apiVersion,
            Description = $"Application Version {appVersion}"
        });

        options.SwaggerDoc("health-services", new OpenApiInfo
        {
            Title = $"{appName} - Health Services",
            Version = apiVersion,
            Description = $"Application Version {appVersion}"
        });

        options.SwaggerDoc("self-services", new OpenApiInfo
        {
            Title = $"{appName} - Self Services",
            Version = apiVersion,
            Description = $"Application Version {appVersion}"
        });

        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            var path = apiDesc.RelativePath?.ToLowerInvariant() ?? string.Empty;

            return docName switch
            {
                "auth" =>
                    path.StartsWith("api/v1/auth") ||
                    path.StartsWith("api/v1/version"),

                "administrator" =>
                    path.StartsWith("api/v1/administrator"),

                "corporate" =>
                    path.StartsWith("api/v1/corporate"),

                "health-services" =>
                    path.StartsWith("api/v1/health-services") ||
                    path.StartsWith("api/v1/healthservices") ||
                    path.StartsWith("api/v1/health-service"),

                "self-services" =>
                    path.StartsWith("api/v1/self-services") ||
                    path.StartsWith("api/v1/selfservices") ||
                    path.StartsWith("api/v1/profile"),

                _ => false
            };
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Masukkan token JWT dengan format: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddHealthChecks();

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost;

        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var dataProtectionKeysPath =
        builder.Configuration["DataProtection:KeysPath"] ??
        "/app/dataprotection-keys";

    var dataProtectionApplicationName =
        builder.Configuration["DataProtection:ApplicationName"] ??
        "QuilvianSystemBackend";

    Directory.CreateDirectory(dataProtectionKeysPath);

    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName(dataProtectionApplicationName);

    var app = builder.Build();

    Log.Information(
        "Starting {Application} {BackendVersion} in {Environment} environment.",
        appName,
        appVersion,
        app.Environment.EnvironmentName
    );

    var uploadRootPath = builder.Configuration["FileStorage:UploadRootPath"];
    var publicRequestPath = builder.Configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

    if (!string.IsNullOrWhiteSpace(publicRequestPath))
    {
        publicRequestPath = publicRequestPath.Replace("\\", "/").Trim();

        if (!publicRequestPath.StartsWith('/'))
        {
            publicRequestPath = "/" + publicRequestPath;
        }

        publicRequestPath = publicRequestPath.TrimEnd('/');
    }

    if (!string.IsNullOrWhiteSpace(uploadRootPath))
    {
        uploadRootPath = uploadRootPath.Trim();

        if (!Path.IsPathRooted(uploadRootPath))
        {
            uploadRootPath = Path.Combine(app.Environment.ContentRootPath, uploadRootPath);
        }

        uploadRootPath = Path.GetFullPath(uploadRootPath);

        Directory.CreateDirectory(uploadRootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadRootPath),
            RequestPath = publicRequestPath
        });
    }

    app.UseForwardedHeaders();

    var enableRequestLogging = builder.Configuration.GetValue<bool?>(
        "SerilogSettings:EnableRequestLogging"
    ) ?? true;

    if (enableRequestLogging)
    {
        var slowRequestMs = builder.Configuration.GetValue<double?>(
            "SerilogSettings:SlowRequestThresholdMs"
        ) ?? 3000;

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "{DisplayMessage}";

            options.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (exception != null || httpContext.Response.StatusCode >= 500)
                {
                    return Serilog.Events.LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 400)
                {
                    return Serilog.Events.LogEventLevel.Warning;
                }

                return Serilog.Events.LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var endpoint = httpContext.GetEndpoint();

                var userId =
                    httpContext.User.FindFirst("user_id")?.Value ??
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var userName =
                    httpContext.User.FindFirst("username")?.Value ??
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                var email =
                    httpContext.User.FindFirst("email")?.Value ??
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                var ip =
                    httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();

                if (string.IsNullOrWhiteSpace(ip))
                {
                    ip = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    ip = httpContext.Connection.RemoteIpAddress?.ToString();
                }

                var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "-";
                var lang = httpContext.Request.Headers["Accept-Language"].FirstOrDefault() ?? "-";
                var client = BuildClientInfo(userAgent);

                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RemoteIpAddress", ip);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserId", userId);
                diagnosticContext.Set("UserName", userName);
                diagnosticContext.Set("Email", email);
                diagnosticContext.Set("Client", client);
                diagnosticContext.Set("Lang", lang);
                diagnosticContext.Set("EndpointName", endpoint?.DisplayName);

                diagnosticContext.Set("LogType", "HttpRequest");
                diagnosticContext.Set("EventCode", "HTTP_REQUEST");
                diagnosticContext.Set("Module", "HTTP");
                diagnosticContext.Set("Action", httpContext.Request.Method);
                diagnosticContext.Set("Path", httpContext.Request.Path.ToString());
                diagnosticContext.Set("Method", httpContext.Request.Method);
                diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);

                // Elapsed asli tetap otomatis masuk dari Serilog sebagai property Elapsed.
                // Untuk DisplayMessage, kita isi 0 dulu karena nilai elapsed final dihitung internal middleware.
                // Di Grafana nanti tetap ada property Elapsed asli dari Serilog.
                diagnosticContext.Set("DisplayMessage", BuildHttpDisplayMessage(httpContext, 0));
            };
        });
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.MapHealthChecks("/health");

    await AppVersionSeeder.SeedAsync(app.Services);
    await DefaultWorkScheduleSeeder.SeedAsync(app.Services);
    await SuperAdminSeeder.SeedAsync(app.Services);
    await AccessMenuSeeder.SeedAsync(app.Services);

    // Seed Awal Saja
    // var icd10FolderPath = Path.Combine(
    //     app.Environment.ContentRootPath,
    //     "SeedData",
    //     "ICD10"
    // );

    // await Icd10DiagnosisSeeder.SeedAsync(app.Services, icd10FolderPath);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/auth/swagger.json", "01 - Authentication");
            options.SwaggerEndpoint("/swagger/administrator/swagger.json", "02 - Administrator");
            options.SwaggerEndpoint("/swagger/corporate/swagger.json", "03 - Corporate");
            options.SwaggerEndpoint("/swagger/health-services/swagger.json", "04 - Health Services");
            options.SwaggerEndpoint("/swagger/self-services/swagger.json", "05 - Self Services");

            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.DefaultModelsExpandDepth(-1);
            options.DisplayRequestDuration();
        });

        app.MapGet("/docs", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(BuildDocsIndexHtml());
        });

        app.MapGet("/docs/auth", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(BuildRedocHtml(
                "Quilvian Authentication API Documentation",
                "Dokumentasi API Authentication Quilvian System.",
                "/swagger/auth/swagger.json"
            ));
        });

        app.MapGet("/docs/administrator", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(BuildRedocHtml(
                "Quilvian Administrator API Documentation",
                "Dokumentasi API Administrator Quilvian System.",
                "/swagger/administrator/swagger.json"
            ));
        });

        app.MapGet("/docs/corporate", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(BuildRedocHtml(
                "Quilvian Corporate API Documentation",
                "Dokumentasi API Corporate Quilvian System.",
                "/swagger/corporate/swagger.json"
            ));
        });

        app.MapGet("/docs/health-services", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(BuildRedocHtml(
                "Quilvian Health Services API Documentation",
                "Dokumentasi API Health Services Quilvian System.",
                "/swagger/health-services/swagger.json"
            ));
        });

        app.MapGet("/docs/self-services", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(BuildRedocHtml(
                "Quilvian Self Services API Documentation",
                "Dokumentasi API Self Services Quilvian System.",
                "/swagger/self-services/swagger.json"
            ));
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("FrontendCorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<QueueHub>("/hubs/queues");

    app.Run();

    static string BuildHttpDisplayMessage(HttpContext httpContext, double elapsedMs)
    {
        var now = DateTime.Now;

        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();
        var statusCode = httpContext.Response.StatusCode;

        var level = statusCode >= 500 ? "ERR" :
                    statusCode >= 400 ? "WRN" :
                    "INF";

        var message = statusCode >= 500 ? "Request gagal karena server error." :
                      statusCode >= 400 ? "Request gagal karena client error." :
                      "Request berhasil.";

        var userId =
            httpContext.User.FindFirst("user_id")?.Value ??
            httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
            "-";

        var userName =
            httpContext.User.FindFirst("username")?.Value ??
            httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ??
            "-";

        var email =
            httpContext.User.FindFirst("email")?.Value ??
            httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ??
            "-";

        var ip =
            httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();

        if (string.IsNullOrWhiteSpace(ip))
        {
            ip = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
        }

        var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "-";
        var lang = httpContext.Request.Headers["Accept-Language"].FirstOrDefault() ?? "-";

        var client = BuildClientInfo(userAgent);

        return
            $"{now:yyyy-MM-dd HH:mm:ss} " +
            $"[{level}] HTTP_REQUEST | " +
            $"Module=\"HTTP\" " +
            $"Action=\"{SanitizeLogValue(method)}\" " +
            $"Message=\"{SanitizeLogValue(message)}\" " +
            $"Path=\"{SanitizeLogValue(path)}\" " +
            $"StatusCode=\"{statusCode}\" " +
            $"ElapsedMs=\"{elapsedMs:0.0000}\" " +
            $"| ------> " +
            $"UserId=\"{SanitizeLogValue(userId)}\" " +
            $"User=\"{SanitizeLogValue(userName)}\" " +
            $"Email=\"{SanitizeLogValue(email)}\" " +
            $"Ip=\"{SanitizeLogValue(ip)}\" " +
            $"Client=\"{SanitizeLogValue(client)}\" " +
            $"Lang=\"{SanitizeLogValue(lang)}\"";
    }

    static string BuildClientInfo(string userAgent)
    {
        var browser = "Unknown Browser";
        var os = "Unknown OS";
        var device = "Desktop";

        if (string.IsNullOrWhiteSpace(userAgent) || userAgent == "-")
        {
            return "-";
        }

        if (userAgent.Contains("Edg/"))
        {
            browser = "Microsoft Edge";
        }
        else if (userAgent.Contains("Chrome/"))
        {
            browser = "Google Chrome";
        }
        else if (userAgent.Contains("Firefox/"))
        {
            browser = "Mozilla Firefox";
        }
        else if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome/"))
        {
            browser = "Safari";
        }
        else if (userAgent.Contains("PostmanRuntime"))
        {
            browser = "Postman";
        }

        if (userAgent.Contains("Windows NT 10.0"))
        {
            os = "Windows 10 / Windows 11";
        }
        else if (userAgent.Contains("Windows"))
        {
            os = "Windows";
        }
        else if (userAgent.Contains("Android"))
        {
            os = "Android";
        }
        else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
        {
            os = "iOS / iPadOS";
        }
        else if (userAgent.Contains("Mac OS X"))
        {
            os = "macOS";
        }
        else if (userAgent.Contains("Linux"))
        {
            os = "Linux";
        }

        if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
        {
            device = "Mobile";
        }
        else if (userAgent.Contains("iPad") || userAgent.Contains("Tablet"))
        {
            device = "Tablet";
        }

        return $"{browser} / {os} / {device}";
    }

    static string SanitizeLogValue(string? value)
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
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static LogEventLevel ParseLogEventLevel(string? value, LogEventLevel defaultValue)
{
    return Enum.TryParse(value, ignoreCase: true, out LogEventLevel result)
        ? result
        : defaultValue;
}

static int ParseInt(string? value, int defaultValue)
{
    return int.TryParse(value, out var result)
        ? result
        : defaultValue;
}

static string BuildDocsIndexHtml()
{
    return @"<!doctype html>
<html>
<head>
    <title>Quilvian API Documentation</title>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <style>
        body {
            margin: 0;
            padding: 0;
            font-family: Arial, Helvetica, sans-serif;
            background: #f8fafc;
        }

        .docs-home {
            max-width: 980px;
            margin: 0 auto;
            padding: 48px 24px;
        }

        .docs-title {
            font-size: 32px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 8px;
        }

        .docs-subtitle {
            font-size: 16px;
            color: #64748b;
            margin-bottom: 32px;
            line-height: 1.6;
        }

        .docs-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
            gap: 16px;
        }

        .docs-card {
            display: block;
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            padding: 22px;
            color: #0f172a;
            text-decoration: none;
            box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
            transition: all 0.2s ease;
        }

        .docs-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 14px 32px rgba(15, 23, 42, 0.1);
            border-color: #38bdf8;
        }

        .docs-card-title {
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 8px;
        }

        .docs-card-desc {
            font-size: 14px;
            color: #64748b;
            line-height: 1.5;
        }
    </style>
</head>
<body>
    <main class=""docs-home"">
        <div class=""docs-title"">Quilvian System API Documentation</div>
        <div class=""docs-subtitle"">
            Dokumentasi API berbasis OpenAPI untuk kebutuhan internal developer dan integrasi vendor.
            Pilih modul dokumentasi di bawah ini.
        </div>

        <section class=""docs-grid"">
            <a class=""docs-card"" href=""/docs/auth"">
                <div class=""docs-card-title"">Authentication API</div>
                <div class=""docs-card-desc"">
                    Endpoint login, token, profile, dan autentikasi aplikasi.
                </div>
            </a>

            <a class=""docs-card"" href=""/docs/administrator"">
                <div class=""docs-card-title"">Administrator API</div>
                <div class=""docs-card-desc"">
                    Endpoint master data, akses, user, role, dan konfigurasi administrator.
                </div>
            </a>

            <a class=""docs-card"" href=""/docs/corporate"">
                <div class=""docs-card-title"">Corporate API</div>
                <div class=""docs-card-desc"">
                    Endpoint HR, workforce, employee, attendance, dan corporate module.
                </div>
            </a>

            <a class=""docs-card"" href=""/docs/health-services"">
                <div class=""docs-card-title"">Health Services API</div>
                <div class=""docs-card-desc"">
                    Endpoint layanan kesehatan, pasien, kiosk, antrian, klinik, dan clinical flow.
                </div>
            </a>

            <a class=""docs-card"" href=""/docs/self-services"">
                <div class=""docs-card-title"">Self Services API</div>
                <div class=""docs-card-desc"">
                    Endpoint self service, profile, kiosk, dan akses mandiri.
                </div>
            </a>
        </section>
    </main>
</body>
</html>";
}

static string BuildRedocHtml(
    string title,
    string description,
    string specUrl)
{
    var safeTitle = System.Net.WebUtility.HtmlEncode(title);
    var safeDescription = System.Net.WebUtility.HtmlEncode(description);
    var safeSpecUrl = System.Net.WebUtility.HtmlEncode(specUrl);

    return @"<!doctype html>
                <html>
                <head>
                    <title>{{TITLE}}</title>
                    <meta charset=""utf-8""/>
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
                    <meta name=""description"" content=""{{DESCRIPTION}}"">
                    <style>
                        body {
                            margin: 0;
                            padding: 0;
                        }

                        redoc {
                            display: block;
                        }
                    </style>
                </head>
                <body>
                    <redoc
                        spec-url=""{{SPEC_URL}}""
                        required-props-first=""true""
                        sort-props-alphabetically=""true""
                        hide-download-button=""false""
                        expand-responses=""200,201"">
                    </redoc>

                    <script src=""https://cdn.redoc.ly/redoc/latest/bundles/redoc.standalone.js""></script>
                </body>
                </html>"
        .Replace("{{TITLE}}", safeTitle)
        .Replace("{{DESCRIPTION}}", safeDescription)
        .Replace("{{SPEC_URL}}", safeSpecUrl);
}
