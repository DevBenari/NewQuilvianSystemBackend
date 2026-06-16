using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuilvianSystemBackend.Middlewares;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Seeders;
using QuilvianSystemBackend.Services.Language;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

var builder = WebApplication.CreateBuilder(args);

// Paksa session/JWT/cookie expire 10 menit.
// Nilai ini akan dibaca oleh AuthController lewat IConfiguration.
const int AuthExpireMinutes = 60;
builder.Configuration["Jwt:ExpireMinutes"] = AuthExpireMinutes.ToString();

// Add services to the container.
builder.Services.AddControllers();

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
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token))
                {
                    context.Token = token;
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("KioskRegionRead", policy =>
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
});

builder.Services.AddDistributedMemoryCache();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

var appName = builder.Configuration["AppInfo:Name"] ?? "Quilvian System Backend API";
var appVersion = builder.Configuration["AppInfo:Version"] ?? "1.0.0";
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

var uploadRootPath = builder.Configuration["FileStorage:UploadRootPath"];
var publicRequestPath = builder.Configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

if (!string.IsNullOrWhiteSpace(uploadRootPath))
{
    Directory.CreateDirectory(uploadRootPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRootPath),
        RequestPath = publicRequestPath
    });
}

app.UseForwardedHeaders();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapHealthChecks("/health");

await AppVersionSeeder.SeedAsync(app.Services);
await DefaultWorkScheduleSeeder.SeedAsync(app.Services);
await SuperAdminSeeder.SeedAsync(app.Services);
await AccessMenuSeeder.SeedAsync(app.Services);

//Seed Awal Saja
//var icd10FolderPath = Path.Combine(
//    app.Environment.ContentRootPath,
//    "SeedData",
//    "ICD10"
//);

//await Icd10DiagnosisSeeder.SeedAsync(app.Services, icd10FolderPath);

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
}

app.UseHttpsRedirection();

app.UseCors("FrontendCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();