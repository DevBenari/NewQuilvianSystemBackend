using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.DTOs.Auth;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Language;
using QuilvianSystemBackend.Services.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuilvianSystemBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Tags("01-Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly LanguageService _languageService;
        private readonly LoggerService _loggerService;
        private readonly IDataProtector _fingerprintProtector;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            LanguageService languageService,
            LoggerService loggerService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _languageService = languageService;
            _loggerService = loggerService;
            _fingerprintProtector = dataProtectionProvider.CreateProtector("Quilvian.Fingerprint.Template.v1");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Email kosong."
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    _languageService.GetMessage(MessageKeys.AuthEmailRequired)
                ));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Password kosong.",
                    new
                    {
                        request.Email
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    _languageService.GetMessage(MessageKeys.AuthPasswordRequired)
                ));
            }

            var loginIdentifier = request.Email.Trim();

            var user =
                await _userManager.FindByEmailAsync(loginIdentifier) ??
                await _userManager.FindByNameAsync(loginIdentifier);

            if (user == null)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Email tidak ditemukan.",
                    new
                    {
                        Email = loginIdentifier
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthInvalidCredential)
                ));
            }

            if (!user.IsActive)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Akun tidak aktif.",
                    new
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccountInactive)
                ));
            }

            if (user.AccessValidUntil.HasValue && user.AccessValidUntil.Value < DateTime.UtcNow)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Masa akses akun sudah berakhir.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName,
                        user.AccessValidUntil
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccessExpired)
                ));
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true
            );

            if (signInResult.IsLockedOut)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Akun terkunci.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccountLocked)
                ));
            }

            if (!signInResult.Succeeded)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Password salah.",
                    new
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthInvalidCredential)
                ));
            }

            var kioskContext = await ResolveKioskLoginContextAsync(user);

            if (kioskContext.IsKioskAccount && !kioskContext.CanLogin)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login kiosk gagal. Akun/perangkat kiosk tidak dapat digunakan.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName,
                        kioskContext.KioskDeviceId,
                        kioskContext.DeviceCode,
                        kioskContext.DeviceName,
                        kioskContext.IsDeviceActive,
                        kioskContext.IsLoginCreated,
                        kioskContext.IsLoginEnabled,
                        kioskContext.IsLoginLocked,
                        kioskContext.BlockReason
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    kioskContext.BlockReason ?? "Akun kiosk tidak dapat digunakan."
                ));
            }

            var geofenceValidation = kioskContext.IsKioskAccount
                ? GeofenceValidationResult.Bypassed("Kiosk account bypass geolocation.")
                : ValidateLoginGeofence(user, request);

            if (!geofenceValidation.IsValid)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Lokasi di luar area yang diizinkan.",
                    new
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        request.Latitude,
                        request.Longitude,
                        request.AccuracyMeters,
                        geofenceValidation.DistanceMeters,
                        geofenceValidation.Message
                    }
                );

                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        geofenceValidation.Message
                    )
                );
            }

            try
            {
                var attendanceResult = kioskContext.IsKioskAccount
                    ? LoginAttendanceResult.Skipped("Login kiosk tidak mencatat attendance.")
                    : await RecordAttendanceOnLoginAsync(
                        user,
                        request,
                        geofenceValidation
                    );

                user.LastLoginAt = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var identityErrors = string.Join(
                        " | ",
                        updateResult.Errors.Select(x => $"{x.Code}: {x.Description}")
                    );

                    await _loggerService.ErrorAsync(
                        "Auth",
                        "Login",
                        $"Gagal update LastLoginAt user. Detail: {identityErrors}",
                        new Exception(identityErrors),
                        new
                        {
                            user.Id,
                            user.Email,
                            user.UserName
                        }
                    );

                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ApiResponse<object>.Fail(
                            StatusCodes.Status500InternalServerError,
                            $"Gagal update data login user: {identityErrors}"
                        )
                    );
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles, kioskContext);

                SetAuthCookie(token, user);

                await _loggerService.InfoAsync(
                    "Auth",
                    "Login",
                    "Login berhasil.",
                    new
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        request.Latitude,
                        request.Longitude,
                        request.AccuracyMeters,
                        geofenceValidation.DistanceMeters,
                        AttendanceRecorded = attendanceResult.IsRecorded,
                        AttendanceAlreadyExists = attendanceResult.IsAlreadyExists,
                        AttendanceMessage = attendanceResult.Message
                    }
                );

                return Ok(ApiResponse<LoginDataResponse>.Ok(
                    new LoginDataResponse
                    {
                        Auth = BuildAuthInfoResponse(user),
                        Endpoints = new AuthEndpointResponse(),
                        User = BuildUserResponse(user, roles, kioskContext)
                    },
                    _languageService.GetMessage(MessageKeys.AuthLoginSuccess)
                ));
            }
            catch (Exception ex)
            {
                var detailError = GetFullExceptionMessage(ex);

                await _loggerService.ErrorAsync(
                    "Auth",
                    "Login",
                    $"Terjadi error saat login. Detail: {detailError}",
                    ex,
                    new
                    {
                        Email = loginIdentifier,
                        request.Latitude,
                        request.Longitude,
                        request.AccuracyMeters
                    }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Terjadi error saat login: {detailError}"
                    )
                );
            }
        }

        [HttpGet("fingerprint/candidates")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<FingerprintCandidateResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFingerprintCandidates()
        {
            var credentials = await _dbContext.ApplicationUserFingerprintCredentials
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    x.User != null &&
                    x.User.IsActive)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.RegisteredAt)
                .ToListAsync();

            var result = new List<FingerprintCandidateResponse>();

            foreach (var credential in credentials)
            {
                try
                {
                    if (credential.TemplateDataEncrypted == null ||
                        credential.TemplateDataEncrypted.Length == 0)
                    {
                        continue;
                    }

                    var templateBytes = _fingerprintProtector.Unprotect(
                        credential.TemplateDataEncrypted
                    );

                    result.Add(new FingerprintCandidateResponse
                    {
                        CredentialId = credential.Id,
                        UserId = credential.UserId,
                        DisplayName = credential.User?.DisplayName ?? string.Empty,
                        FingerPosition = credential.FingerPosition,
                        TemplateFormat = credential.TemplateFormat,
                        TemplateVersion = credential.TemplateVersion,
                        FingerprintTemplateBase64 = Convert.ToBase64String(templateBytes)
                    });
                }
                catch (Exception ex)
                {
                    await _loggerService.ErrorAsync(
                        "Auth",
                        "Fingerprint.Candidates",
                        $"Gagal membuka template fingerprint. Detail: {GetFullExceptionMessage(ex)}",
                        ex,
                        new
                        {
                            CredentialId = credential.Id,
                            credential.UserId,
                            credential.TemplateFormat,
                            credential.TemplateVersion,
                            credential.SampleFormat,
                            credential.IsActive,
                            credential.IsDelete,
                            credential.RegisteredAt,
                            TemplateLength = credential.TemplateDataEncrypted == null
                                ? 0
                                : credential.TemplateDataEncrypted.Length
                        }
                    );

                    continue;
                }
            }

            return Ok(ApiResponse<List<FingerprintCandidateResponse>>.Ok(
                result,
                "Kandidat fingerprint berhasil diambil."
            ));
        }

        [HttpPost("fingerprint-login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> FingerprintLogin([FromBody] FingerprintLoginRequest request)
        {
            var credential = await _dbContext.ApplicationUserFingerprintCredentials
                .FirstOrDefaultAsync(x =>
                    x.Id == request.CredentialId &&
                    x.UserId == request.UserId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (credential == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Fingerprint tidak valid."
                ));
            }

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            if (!user.IsActive)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Akun tidak aktif."
                ));
            }

            if (user.AccessValidUntil.HasValue && user.AccessValidUntil.Value < DateTime.UtcNow)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Masa akses akun sudah berakhir."
                ));
            }

            var geofenceValidation = ValidateGeofence(
                user,
                request.Latitude,
                request.Longitude,
                request.AccuracyMeters
            );

            if (!geofenceValidation.IsValid)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        geofenceValidation.Message
                    )
                );
            }

            var loginLikeRequest = new LoginRequest
            {
                Email = user.Email ?? string.Empty,
                Password = string.Empty,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AccuracyMeters = request.AccuracyMeters
            };

            var attendanceResult = await RecordAttendanceOnLoginAsync(
                user,
                loginLikeRequest,
                geofenceValidation
            );

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var kioskContext = await ResolveKioskLoginContextAsync(user);
            var token = GenerateJwtToken(user, roles, kioskContext);

            SetAuthCookie(token, user);

            await _loggerService.InfoAsync(
                "Auth",
                "Fingerprint.Login",
                "Login fingerprint berhasil.",
                new
                {
                    user.Id,
                    user.Email,
                    CredentialId = credential.Id,
                    credential.FingerPosition,
                    request.Score,
                    request.DeviceId,
                    request.DeviceModel,
                    AttendanceRecorded = attendanceResult.IsRecorded,
                    AttendanceAlreadyExists = attendanceResult.IsAlreadyExists
                }
            );

            return Ok(ApiResponse<LoginDataResponse>.Ok(
                new LoginDataResponse
                {
                    Auth = BuildAuthInfoResponse(user),
                    Endpoints = new AuthEndpointResponse(),
                    User = BuildUserResponse(user, roles, kioskContext)
                },
                "Login fingerprint berhasil."
            ));
        }

        [HttpGet("me")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<UserLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdText, out var userId))
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Me",
                    "Gagal mengambil profile. Token tidak valid.",
                    new
                    {
                        UserIdText = userIdText
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthTokenInvalid)
                ));
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Me",
                    "Gagal mengambil profile. User tidak ditemukan.",
                    new
                    {
                        UserId = userId
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthUserNotFound)
                ));
            }

            if (!user.IsActive)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Me",
                    "Gagal mengambil profile. Akun tidak aktif.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccountInactive)
                ));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var kioskContext = await ResolveKioskLoginContextAsync(user);

            await _loggerService.InfoAsync(
                "Auth",
                "Me",
                "Profile user berhasil diambil.",
                new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.DisplayName,
                    user.UserType,
                    user.PrimaryDepartmentId,
                    user.PrimaryPositionId,
                    Roles = roles
                }
            );

            return Ok(ApiResponse<UserLoginResponse>.Ok(
                BuildUserResponse(user, roles, kioskContext),
                "User profile berhasil diambil."
            ));
        }

        [HttpPost("refresh")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<LoginDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdText, out var userId))
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Refresh",
                    "Gagal refresh session. Session tidak valid.",
                    new
                    {
                        UserIdText = userIdText
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthSessionInvalid)
                ));
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Refresh",
                    "Gagal refresh session. User tidak ditemukan.",
                    new
                    {
                        UserId = userId
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthUserNotFound)
                ));
            }

            if (!user.IsActive)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Refresh",
                    "Gagal refresh session. Akun tidak aktif.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccountInactive)
                ));
            }

            if (user.AccessValidUntil.HasValue && user.AccessValidUntil.Value < DateTime.UtcNow)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Refresh",
                    "Gagal refresh session. Masa akses akun sudah berakhir.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName,
                        user.AccessValidUntil
                    }
                );

                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    _languageService.GetMessage(MessageKeys.AuthAccessExpired)
                ));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var kioskContext = await ResolveKioskLoginContextAsync(user);
            var newToken = GenerateJwtToken(user, roles, kioskContext);
            
            SetAuthCookie(newToken, user);

            await _loggerService.InfoAsync(
                "Auth",
                "Refresh",
                "Session diperpanjang.",
                new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email
                }
            );

            return Ok(ApiResponse<LoginDataResponse>.Ok(
                new LoginDataResponse
                {
                    Auth = BuildAuthInfoResponse(user),
                    Endpoints = new AuthEndpointResponse(),
                    User = BuildUserResponse(user, roles, kioskContext)
                },
                _languageService.GetMessage(MessageKeys.AuthSessionRefreshed)
            ));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var username =
                User.FindFirstValue("username") ??
                User.FindFirstValue(ClaimTypes.Name);

            var email =
                User.FindFirstValue("email") ??
                User.FindFirstValue(ClaimTypes.Email);

            ClearAuthCookie();

            await _loggerService.InfoAsync(
                "Auth",
                "Logout",
                "Logout berhasil.",
                new
                {
                    UserId = userId,
                    Username = username,
                    Email = email
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                _languageService.GetMessage(MessageKeys.AuthLogoutSuccess)
            ));
        }

        [HttpPost("attendance/check-out")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AttendanceCheckOut([FromBody] AttendanceCheckoutRequest request)
        {
            try
            {
                var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdText, out var userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail(
                        StatusCodes.Status401Unauthorized,
                        "Token tidak valid."
                    ));
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail(
                        StatusCodes.Status401Unauthorized,
                        "User tidak ditemukan."
                    ));
                }

                if (!user.IsActive)
                {
                    return Unauthorized(ApiResponse<object>.Fail(
                        StatusCodes.Status401Unauthorized,
                        "Akun tidak aktif."
                    ));
                }

                if (!IsAttendanceUser(user))
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "User ini tidak termasuk employee atau doctor, sehingga tidak perlu absen pulang."
                    ));
                }

                var geofenceValidation = ValidateGeofence(
                    user,
                    request.Latitude,
                    request.Longitude,
                    request.AccuracyMeters
                );

                if (!geofenceValidation.IsValid)
                {
                    await _loggerService.WarningAsync(
                        "Auth",
                        "Attendance.CheckOut",
                        "Absen pulang gagal. Lokasi di luar area yang diizinkan.",
                        new
                        {
                            user.Id,
                            user.Email,
                            user.UserName,
                            request.Latitude,
                            request.Longitude,
                            request.AccuracyMeters,
                            geofenceValidation.DistanceMeters,
                            geofenceValidation.Message
                        }
                    );

                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        ApiResponse<object>.Fail(
                            StatusCodes.Status403Forbidden,
                            geofenceValidation.Message
                        )
                    );
                }

                var nowJakarta = GetSystemNow();

                var attendance = await ResolveOpenAttendanceForCheckOutAsync(
                    user.Id,
                    nowJakarta
                );

                if (attendance == null)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Absensi masuk hari ini belum tercatat."
                    ));
                }

                if (attendance.CheckOutAt.HasValue)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Absensi pulang hari ini sudah tercatat."
                    ));
                }

                var checkOutAtUtc = DateTime.UtcNow;

                attendance.CheckOutAt = checkOutAtUtc;
                attendance.CheckOutLatitude = request.Latitude;
                attendance.CheckOutLongitude = request.Longitude;
                attendance.CheckOutAccuracyMeters = request.AccuracyMeters;
                attendance.CheckOutDistanceMeters = geofenceValidation.DistanceMeters ?? 0;

                if (geofenceValidation.IsBypassed)
                {
                    attendance.IsGeofenceBypassed = true;
                    attendance.GeofenceBypassReason = geofenceValidation.BypassReason;
                }

                attendance.CheckOutSource = "ManualCheckOut";
                attendance.CheckOutIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                attendance.CheckOutUserAgent = Request.Headers.UserAgent.ToString();
                attendance.Status = "CheckedOut";
                attendance.WorkDurationMinutes = (int)Math.Max(
                    0,
                    (checkOutAtUtc - attendance.CheckInAt).TotalMinutes
                );
                attendance.UpdateDateTime = DateTime.UtcNow;
                attendance.UpdateBy = user.Id;

                await _dbContext.SaveChangesAsync();

                await _loggerService.InfoAsync(
                    "Auth",
                    "Attendance.CheckOut",
                    "Absensi pulang berhasil dicatat.",
                    new
                    {
                        AttendanceId = attendance.Id,
                        attendance.UserId,
                        attendance.EmployeeId,
                        attendance.DoctorId,
                        attendance.AttendanceDate,
                        attendance.CheckInAt,
                        attendance.CheckOutAt,
                        attendance.WorkDurationMinutes,
                        attendance.CheckOutDistanceMeters
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    new
                    {
                        attendance.Id,
                        attendance.AttendanceDate,
                        attendance.CheckInAt,
                        attendance.CheckOutAt,
                        attendance.WorkDurationMinutes,
                        attendance.Status
                    },
                    "Absensi pulang berhasil dicatat."
                ));
            }
            catch (Exception ex)
            {
                var detailError = GetFullExceptionMessage(ex);

                await _loggerService.ErrorAsync(
                    "Auth",
                    "Attendance.CheckOut",
                    $"Terjadi error saat absensi pulang. Detail: {detailError}",
                    ex,
                    new
                    {
                        request.Latitude,
                        request.Longitude,
                        request.AccuracyMeters
                    }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Terjadi error saat absensi pulang: {detailError}"
                    )
                );
            }
        }

        private static string GetFullExceptionMessage(Exception ex)
        {
            var messages = new List<string>();

            var current = ex;

            while (current != null)
            {
                if (!string.IsNullOrWhiteSpace(current.Message))
                {
                    messages.Add($"{current.GetType().Name}: {current.Message}");
                }

                current = current.InnerException;
            }

            return string.Join(" | ", messages);
        }

        private class ResolvedWorkScheduleResult
        {
            public WfpWorkScheduleAssignment? Assignment { get; set; }

            public MstWorkSchedule? WorkSchedule { get; set; }

            public DateOnly AttendanceDate { get; set; }

            public DateTime? ScheduledCheckInAtUtc { get; set; }

            public DateTime? ScheduledCheckOutAtUtc { get; set; }
        }

        private async Task<LoginAttendanceResult> RecordAttendanceOnLoginAsync(
    ApplicationUser user,
    LoginRequest request,
    GeofenceValidationResult geofenceValidation)
        {
            if (!IsAttendanceUser(user))
            {
                return LoginAttendanceResult.Skipped("User bukan employee atau doctor.");
            }

            if (!request.Latitude.HasValue || !request.Longitude.HasValue)
            {
                return LoginAttendanceResult.Skipped("Lokasi tidak tersedia.");
            }

            Guid? employeeId = null;
            Guid? doctorId = null;

            if (user.UserType == UserType.Employee)
            {
                employeeId = user.EmployeeId;
            }

            if (user.UserType == UserType.PermanentDoctor ||
                user.UserType == UserType.GuestDoctor)
            {
                doctorId = user.DoctorId;
            }

            if (!employeeId.HasValue && !doctorId.HasValue)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Attendance.Login",
                    "Absensi login dilewati. User tidak memiliki EmployeeId atau DoctorId.",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName,
                        user.UserType,
                        user.EmployeeId,
                        user.DoctorId
                    }
                );

                return LoginAttendanceResult.Skipped("Profile employee/doctor belum terhubung.");
            }

            var nowUtc = DateTime.UtcNow;
            var nowJakarta = GetSystemNow();
            var attendanceDate = DateOnly.FromDateTime(nowJakarta);

            var alreadyExists = await _dbContext.EmpAttendances
                .AnyAsync(x =>
                    x.UserId == user.Id &&
                    x.AttendanceDate == attendanceDate &&
                    !x.IsDelete);

            if (alreadyExists)
            {
                return LoginAttendanceResult.AlreadyExists("Absensi masuk hari ini sudah tercatat.");
            }

            var scheduleResult = await ResolveWorkScheduleAsync(user, attendanceDate);
            var schedule = scheduleResult.WorkSchedule;
            var assignment = scheduleResult.Assignment;

            var lateResult = CalculateLateStatus(
                nowUtc,
                scheduleResult.ScheduledCheckInAtUtc,
                schedule
            );

            var attendance = new EmpAttendance
            {
                Id = Guid.NewGuid(),

                UserId = user.Id,
                EmployeeId = employeeId,
                DoctorId = doctorId,
                WorkforceProfileId = user.WorkforceProfileId,

                WorkScheduleId = schedule?.Id,
                WorkScheduleAssignmentId = assignment?.Id,

                AttendanceDate = scheduleResult.AttendanceDate,
                CheckInAt = nowUtc,

                WorkStartTime = schedule?.WorkStartTime,
                WorkEndTime = schedule?.WorkEndTime,
                IsOvernightSchedule = schedule?.IsOvernight ?? false,
                ScheduledCheckInAt = scheduleResult.ScheduledCheckInAtUtc,
                ScheduledCheckOutAt = scheduleResult.ScheduledCheckOutAtUtc,

                CheckInToleranceMinutes = schedule?.CheckInToleranceMinutes ?? 0,
                CheckOutToleranceMinutes = schedule?.CheckOutToleranceMinutes ?? 0,

                IsLate = lateResult.IsLate,
                LateMinutes = lateResult.LateMinutes,
                AttendanceStatus = lateResult.AttendanceStatus,

                CheckInLatitude = request.Latitude.Value,
                CheckInLongitude = request.Longitude.Value,
                CheckInAccuracyMeters = request.AccuracyMeters,
                CheckInDistanceMeters = geofenceValidation.DistanceMeters ?? 0,

                IsGeofenceBypassed = geofenceValidation.IsBypassed,
                GeofenceBypassReason = geofenceValidation.BypassReason,

                UserType = user.UserType,
                CheckInSource = "Login",
                Status = "CheckedIn",

                CheckInIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CheckInUserAgent = Request.Headers.UserAgent.ToString(),

                CreateDateTime = nowUtc,
                CreateBy = user.Id,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.EmpAttendances.Add(attendance);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                "Auth",
                "Attendance.Login",
                "Absensi masuk berhasil dicatat.",
                new
                {
                    AttendanceId = attendance.Id,
                    attendance.UserId,
                    attendance.EmployeeId,
                    attendance.DoctorId,
                    attendance.WorkforceProfileId,
                    attendance.WorkScheduleId,
                    attendance.WorkScheduleAssignmentId,
                    attendance.AttendanceDate,
                    attendance.CheckInAt,
                    attendance.WorkStartTime,
                    attendance.WorkEndTime,
                    attendance.IsOvernightSchedule,
                    attendance.ScheduledCheckInAt,
                    attendance.ScheduledCheckOutAt,
                    attendance.CheckInToleranceMinutes,
                    attendance.CheckOutToleranceMinutes,
                    attendance.IsLate,
                    attendance.LateMinutes,
                    attendance.AttendanceStatus,
                    attendance.IsGeofenceBypassed,
                    attendance.GeofenceBypassReason
                }
            );

            return LoginAttendanceResult.Recorded("Absensi masuk berhasil dicatat.");
        }

        private static bool IsAttendanceUser(ApplicationUser user)
        {
            return user.UserType == UserType.Employee ||
                   user.UserType == UserType.PermanentDoctor ||
                   user.UserType == UserType.GuestDoctor;
        }

        private static bool IsKioskUser(ApplicationUser user)
        {
            return user.UserType == UserType.SystemUser;
        }

        private static DateTime GetSystemNow()
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }

        private class LoginAttendanceResult
        {
            public bool IsRecorded { get; set; }

            public bool IsAlreadyExists { get; set; }

            public string Message { get; set; } = string.Empty;

            public static LoginAttendanceResult Recorded(string message)
            {
                return new LoginAttendanceResult
                {
                    IsRecorded = true,
                    IsAlreadyExists = false,
                    Message = message
                };
            }

            public static LoginAttendanceResult AlreadyExists(string message)
            {
                return new LoginAttendanceResult
                {
                    IsRecorded = false,
                    IsAlreadyExists = true,
                    Message = message
                };
            }

            public static LoginAttendanceResult Skipped(string message)
            {
                return new LoginAttendanceResult
                {
                    IsRecorded = false,
                    IsAlreadyExists = false,
                    Message = message
                };
            }
        }

        private GeofenceValidationResult ValidateLoginGeofence(
            ApplicationUser user,
            LoginRequest request)
        {
            return ValidateGeofence(
                user,
                request.Latitude,
                request.Longitude,
                request.AccuracyMeters
            );
        }

        private GeofenceValidationResult ValidateGeofence(
            ApplicationUser user,
            double? latitude,
            double? longitude,
            double? accuracyMeters)
        {
            var geofenceEnabled = _configuration.GetValue<bool>("LoginGeofence:Enabled");

            if (!geofenceEnabled)
            {
                return GeofenceValidationResult.Ok(null);
            }

            if (IsKioskUser(user))
            {
                return GeofenceValidationResult.Bypassed("SystemUser kiosk bypass geolocation.");
            }

            var bypassActive = IsGeolocationBypassActive(user);

            if (bypassActive.IsBypass)
            {
                return GeofenceValidationResult.Bypassed(bypassActive.Reason);
            }

            var applyToSuperAdmin = _configuration.GetValue<bool>("LoginGeofence:ApplyToSuperAdmin");

            if (user.UserType == UserType.SuperAdmin && !applyToSuperAdmin)
            {
                return GeofenceValidationResult.Bypassed("SuperAdmin bypass geolocation.");
            }

            if (!latitude.HasValue || !longitude.HasValue)
            {
                return GeofenceValidationResult.Fail("Lokasi wajib diaktifkan.");
            }

            if (latitude.Value < -90 || latitude.Value > 90)
            {
                return GeofenceValidationResult.Fail("Latitude tidak valid.");
            }

            if (longitude.Value < -180 || longitude.Value > 180)
            {
                return GeofenceValidationResult.Fail("Longitude tidak valid.");
            }

            var hospitalLatitude = _configuration.GetValue<double?>("LoginGeofence:Latitude");
            var hospitalLongitude = _configuration.GetValue<double?>("LoginGeofence:Longitude");
            var allowedRadiusMeters = _configuration.GetValue<double>("LoginGeofence:AllowedRadiusMeters");
            var maxAccuracyMeters = _configuration.GetValue<double>("LoginGeofence:MaxAccuracyMeters");

            if (!hospitalLatitude.HasValue || !hospitalLongitude.HasValue)
            {
                return GeofenceValidationResult.Fail("Koordinat rumah sakit belum dikonfigurasi.");
            }

            if (allowedRadiusMeters <= 0)
            {
                allowedRadiusMeters = 1000;
            }

            if (maxAccuracyMeters > 0)
            {
                if (!accuracyMeters.HasValue)
                {
                    return GeofenceValidationResult.Fail(
                        "Akurasi lokasi tidak terbaca. Silakan aktifkan GPS/lokasi dengan akurasi tinggi."
                    );
                }

                if (accuracyMeters.Value > maxAccuracyMeters)
                {
                    return GeofenceValidationResult.Fail(
                        $"Akurasi lokasi terlalu rendah. Akurasi saat ini {accuracyMeters.Value:0} meter, maksimal {maxAccuracyMeters:0} meter."
                    );
                }
            }

            var distanceMeters = CalculateDistanceMeters(
                hospitalLatitude.Value,
                hospitalLongitude.Value,
                latitude.Value,
                longitude.Value
            );

            if (distanceMeters > allowedRadiusMeters)
            {
                return GeofenceValidationResult.Fail(
                    $"Lokasi Anda berada {distanceMeters:0} meter dari rumah sakit. Maksimal jarak yang diizinkan {allowedRadiusMeters:0} meter.",
                    distanceMeters
                );
            }

            return GeofenceValidationResult.Ok(distanceMeters);
        }

        private static (bool IsBypass, string? Reason) IsGeolocationBypassActive(ApplicationUser user)
        {
            if (!user.IsGeolocationBypassEnabled)
            {
                return (false, null);
            }

            if (user.GeolocationBypassUntil.HasValue &&
                user.GeolocationBypassUntil.Value < DateTime.UtcNow)
            {
                return (false, null);
            }

            var reason = string.IsNullOrWhiteSpace(user.GeolocationBypassReason)
                ? "User memiliki izin bypass geolocation."
                : user.GeolocationBypassReason;

            return (true, reason);
        }

        private async Task<ResolvedWorkScheduleResult> ResolveWorkScheduleAsync(
    ApplicationUser user,
    DateOnly attendanceDate)
        {
            WfpWorkScheduleAssignment? assignment = null;

            if (user.WorkforceProfileId.HasValue)
            {
                assignment = await _dbContext.Set<WfpWorkScheduleAssignment>()
                    .AsNoTracking()
                    .Include(x => x.WorkSchedule)
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == user.WorkforceProfileId.Value &&
                        x.ScheduleDate == attendanceDate &&
                        x.IsActive &&
                        !x.IsDelete);
            }

            if (assignment != null)
            {
                if (assignment.IsOffDay)
                {
                    return new ResolvedWorkScheduleResult
                    {
                        Assignment = assignment,
                        WorkSchedule = null,
                        AttendanceDate = attendanceDate,
                        ScheduledCheckInAtUtc = null,
                        ScheduledCheckOutAtUtc = null
                    };
                }

                if (assignment.WorkSchedule != null &&
                    assignment.WorkSchedule.IsActive &&
                    !assignment.WorkSchedule.IsDelete)
                {
                    var scheduled = BuildScheduledDateTimeUtc(
                        attendanceDate,
                        assignment.WorkSchedule
                    );

                    return new ResolvedWorkScheduleResult
                    {
                        Assignment = assignment,
                        WorkSchedule = assignment.WorkSchedule,
                        AttendanceDate = attendanceDate,
                        ScheduledCheckInAtUtc = scheduled.ScheduledCheckInAtUtc,
                        ScheduledCheckOutAtUtc = scheduled.ScheduledCheckOutAtUtc
                    };
                }
            }

            var defaultSchedule = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x =>
                    x.IsDefault &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync();

            if (defaultSchedule == null)
            {
                return new ResolvedWorkScheduleResult
                {
                    Assignment = null,
                    WorkSchedule = null,
                    AttendanceDate = attendanceDate,
                    ScheduledCheckInAtUtc = null,
                    ScheduledCheckOutAtUtc = null
                };
            }

            var defaultScheduled = BuildScheduledDateTimeUtc(
                attendanceDate,
                defaultSchedule
            );

            return new ResolvedWorkScheduleResult
            {
                Assignment = null,
                WorkSchedule = defaultSchedule,
                AttendanceDate = attendanceDate,
                ScheduledCheckInAtUtc = defaultScheduled.ScheduledCheckInAtUtc,
                ScheduledCheckOutAtUtc = defaultScheduled.ScheduledCheckOutAtUtc
            };
        }

        private async Task<EmpAttendance?> ResolveOpenAttendanceForCheckOutAsync(
            Guid userId,
            DateTime nowJakarta)
        {
            var today = DateOnly.FromDateTime(nowJakarta);
            var yesterday = today.AddDays(-1);

            var todayAttendance = await _dbContext.EmpAttendances
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.AttendanceDate == today &&
                    !x.IsDelete &&
                    !x.CheckOutAt.HasValue);

            if (todayAttendance != null)
            {
                return todayAttendance;
            }

            var overnightAttendance = await _dbContext.EmpAttendances
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.AttendanceDate == yesterday &&
                    x.IsOvernightSchedule &&
                    !x.IsDelete &&
                    !x.CheckOutAt.HasValue);

            return overnightAttendance;
        }

        private static (DateTime ScheduledCheckInAtUtc, DateTime ScheduledCheckOutAtUtc)
            BuildScheduledDateTimeUtc(DateOnly scheduleDate, MstWorkSchedule schedule)
        {
            var jakartaTimeZone = GetJakartaTimeZone();

            var scheduledCheckInLocal = scheduleDate.ToDateTime(schedule.WorkStartTime);

            var checkOutDate = schedule.IsOvernight
                ? scheduleDate.AddDays(1)
                : scheduleDate;

            var scheduledCheckOutLocal = checkOutDate.ToDateTime(schedule.WorkEndTime);

            scheduledCheckInLocal = DateTime.SpecifyKind(
                scheduledCheckInLocal,
                DateTimeKind.Unspecified
            );

            scheduledCheckOutLocal = DateTime.SpecifyKind(
                scheduledCheckOutLocal,
                DateTimeKind.Unspecified
            );

            var scheduledCheckInUtc = TimeZoneInfo.ConvertTimeToUtc(
                scheduledCheckInLocal,
                jakartaTimeZone
            );

            var scheduledCheckOutUtc = TimeZoneInfo.ConvertTimeToUtc(
                scheduledCheckOutLocal,
                jakartaTimeZone
            );

            return (scheduledCheckInUtc, scheduledCheckOutUtc);
        }

        private static TimeZoneInfo GetJakartaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        private static (bool IsLate, int LateMinutes, string AttendanceStatus) CalculateLateStatus(
     DateTime checkInAtUtc,
     DateTime? scheduledCheckInAtUtc,
     MstWorkSchedule? schedule)
        {
            if (schedule == null || !scheduledCheckInAtUtc.HasValue)
            {
                return (false, 0, "PresentNoSchedule");
            }

            if (schedule.ScheduleType.Equals("Off", StringComparison.OrdinalIgnoreCase))
            {
                return (false, 0, "OffDayAttendance");
            }

            if (schedule.ScheduleType.Equals("OnCall", StringComparison.OrdinalIgnoreCase))
            {
                return (false, 0, "OnCall");
            }

            var allowedCheckInTimeUtc = scheduledCheckInAtUtc.Value
                .AddMinutes(schedule.CheckInToleranceMinutes);

            if (checkInAtUtc <= allowedCheckInTimeUtc)
            {
                return (false, 0, "Present");
            }

            var lateMinutes = (int)Math.Ceiling(
                (checkInAtUtc - allowedCheckInTimeUtc).TotalMinutes
            );

            return (true, lateMinutes, "Late");
        }

        private async Task<KioskLoginContext> ResolveKioskLoginContextAsync(ApplicationUser user)
        {
            if (user == null)
            {
                return KioskLoginContext.None();
            }

            var userCode = (user.UserCode ?? string.Empty).Trim();
            var email = (user.Email ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(userCode) && string.IsNullOrWhiteSpace(email))
            {
                return KioskLoginContext.None();
            }

            MstKioskDevice? kioskDevice = null;

            if (!string.IsNullOrWhiteSpace(userCode))
            {
                kioskDevice = await _dbContext.MstKioskDevices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.DeviceCode == userCode
                    );
            }

            if (kioskDevice == null && !string.IsNullOrWhiteSpace(email))
            {
                var loweredEmail = email.ToLowerInvariant();

                if (loweredEmail.StartsWith("kiosk.") &&
                    loweredEmail.EndsWith("@kiosk.local"))
                {
                    var emailDeviceCode = loweredEmail
                        .Replace("kiosk.", string.Empty)
                        .Replace("@kiosk.local", string.Empty)
                        .ToUpperInvariant();

                    kioskDevice = await _dbContext.MstKioskDevices
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.DeviceCode.ToUpper() == emailDeviceCode
                        );
                }
            }

            if (kioskDevice == null)
            {
                return KioskLoginContext.None();
            }

            var isDeviceActive =
                kioskDevice.IsActive &&
                kioskDevice.DeviceStatus == KioskDeviceStatus.Active;

            var isLoginCreated = true;

            var isLoginEnabled = user.IsActive;

            var isLoginLocked =
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            var canLogin =
                isDeviceActive &&
                isLoginCreated &&
                isLoginEnabled &&
                !isLoginLocked;

            var blockReason = canLogin
                ? null
                : !isDeviceActive
                    ? "Perangkat kiosk tidak aktif."
                    : !isLoginCreated
                        ? "Akun login kiosk belum dibuat."
                        : !isLoginEnabled
                            ? "Login kiosk sedang dinonaktifkan."
                            : isLoginLocked
                                ? "Login kiosk sedang terkunci."
                                : "Akun kiosk tidak dapat digunakan.";

            return new KioskLoginContext
            {
                IsKioskAccount = true,

                KioskDeviceId = kioskDevice.Id,
                DeviceCode = kioskDevice.DeviceCode,
                DeviceName = kioskDevice.DeviceName,

                DeviceTypeName = kioskDevice.DeviceType.ToString(),
                DeviceStatusName = kioskDevice.DeviceStatus.ToString(),

                LocationName = kioskDevice.LocationName,
                FloorName = kioskDevice.FloorName,

                IsDeviceActive = isDeviceActive,
                IsLoginCreated = isLoginCreated,
                IsLoginEnabled = isLoginEnabled,
                IsLoginLocked = isLoginLocked,
                CanLogin = canLogin,
                BlockReason = blockReason,

                IsAllowWalkIn = kioskDevice.IsAllowWalkIn,
                IsAllowAppointment = kioskDevice.IsAllowAppointment,
                IsAllowInsuranceRegistration = kioskDevice.IsAllowInsuranceRegistration
            };
        }

        private static double CalculateDistanceMeters(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            const double earthRadiusMeters = 6371000;

            var dLat = ToRadians(latitude2 - latitude1);
            var dLon = ToRadians(longitude2 - longitude1);

            var lat1 = ToRadians(latitude1);
            var lat2 = ToRadians(latitude2);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusMeters * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles, KioskLoginContext? kioskContext = null)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("Jwt:Key belum dikonfigurasi.");
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),

                new Claim("user_id", user.Id.ToString()),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim("email", user.Email ?? string.Empty),
                new Claim("full_name", user.DisplayName ?? string.Empty),
                new Claim("user_type", user.UserType.ToString()),
                new Claim("user_type_id", ((int)user.UserType).ToString()),
                new Claim("is_kiosk", IsKioskUser(user) ? "true" : "false"),

                new Claim("department_id", user.PrimaryDepartmentId?.ToString() ?? string.Empty),
                new Claim("position_id", user.PrimaryPositionId?.ToString() ?? string.Empty),
                new Claim("primary_department_id", user.PrimaryDepartmentId?.ToString() ?? string.Empty),
                new Claim("primary_position_id", user.PrimaryPositionId?.ToString() ?? string.Empty),

                new Claim("workforce_profile_id", user.WorkforceProfileId?.ToString() ?? string.Empty),
                new Claim("employee_id", user.EmployeeId?.ToString() ?? string.Empty),
                new Claim("doctor_id", user.DoctorId?.ToString() ?? string.Empty),
                new Claim("external_user_id", user.ExternalUserId?.ToString() ?? string.Empty),

                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (kioskContext?.IsKioskAccount == true)
            {
                claims.Add(new Claim("is_kiosk_account", "true"));
                claims.Add(new Claim("profile_type", "KioskDevice"));
                claims.Add(new Claim("kiosk_device_id", kioskContext.KioskDeviceId?.ToString() ?? string.Empty));
                claims.Add(new Claim("kiosk_device_code", kioskContext.DeviceCode));
                claims.Add(new Claim("kiosk_device_name", kioskContext.DeviceName));
                claims.Add(new Claim("kiosk_can_walk_in", kioskContext.IsAllowWalkIn.ToString().ToLowerInvariant()));
                claims.Add(new Claim("kiosk_can_appointment", kioskContext.IsAllowAppointment.ToString().ToLowerInvariant()));
                claims.Add(new Claim("kiosk_can_insurance_registration", kioskContext.IsAllowInsuranceRegistration.ToString().ToLowerInvariant()));
            }
            else
            {
                claims.Add(new Claim("is_kiosk_account", "false"));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var expires = DateTime.UtcNow.AddMinutes(ResolveJwtExpireMinutes(user));

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetJwtExpireMinutes()
        {
            var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes");

            return expireMinutes <= 0 ? 60 : expireMinutes;
        }

        private void SetAuthCookie(string token, ApplicationUser user)
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";
            var expireMinutes = ResolveJwtExpireMinutes(user);

            Response.Cookies.Append(
                cookieName,
                token,
                BuildAuthCookieOptions(expireMinutes)
            );
        }

        private int GetKioskJwtExpireMinutes()
        {
            var expireMinutes = _configuration.GetValue<int>("Jwt:KioskExpireMinutes");

            return expireMinutes <= 0 ? 1440 : expireMinutes;
        }

        private int ResolveJwtExpireMinutes(ApplicationUser user)
        {
            return IsKioskUser(user)
                ? GetKioskJwtExpireMinutes()
                : GetJwtExpireMinutes();
        }

        private CookieOptions BuildAuthCookieOptions(int expireMinutes)
        {
            var sameSite = GetConfiguredSameSiteMode();

            var secure = _configuration.GetValue<bool?>("AuthCookie:Secure") ?? true;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes),
                MaxAge = TimeSpan.FromMinutes(expireMinutes),
                Path = "/"
            };

            var domain = _configuration["AuthCookie:Domain"];

            if (!string.IsNullOrWhiteSpace(domain))
            {
                cookieOptions.Domain = domain;
            }

            return cookieOptions;
        }

        private CookieOptions BuildDeleteCookieOptions()
        {
            var sameSite = GetConfiguredSameSiteMode();

            var secure = _configuration.GetValue<bool?>("AuthCookie:Secure") ?? true;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = sameSite,
                Path = "/"
            };

            var domain = _configuration["AuthCookie:Domain"];

            if (!string.IsNullOrWhiteSpace(domain))
            {
                cookieOptions.Domain = domain;
            }

            return cookieOptions;
        }

        private SameSiteMode GetConfiguredSameSiteMode()
        {
            var value = _configuration["AuthCookie:SameSite"];

            if (string.IsNullOrWhiteSpace(value))
            {
                return SameSiteMode.None;
            }

            return value.Trim().ToLowerInvariant() switch
            {
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                "none" => SameSiteMode.None,
                _ => SameSiteMode.None
            };
        }

        private void ClearAuthCookie()
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";

            Response.Cookies.Delete(
                cookieName,
                BuildDeleteCookieOptions()
            );
        }

        private AuthInfoResponse BuildAuthInfoResponse(ApplicationUser? user = null)
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";
            var expireMinutes = user == null
                ? GetJwtExpireMinutes()
                : ResolveJwtExpireMinutes(user);
            var sameSite = GetConfiguredSameSiteMode();
            var secure = _configuration.GetValue<bool?>("AuthCookie:Secure") ?? true;

            return new AuthInfoResponse
            {
                Scheme = "Cookie",
                CookieName = cookieName,
                IsHttpOnly = true,
                SameSite = sameSite.ToString(),
                Secure = secure,
                ExpiresInMinutes = expireMinutes,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expireMinutes),
                FrontendInstruction = "Gunakan credentials: 'include' atau withCredentials: true pada setiap request ke backend. Access token disimpan di HttpOnly cookie."
            };
        }

        private UserLoginResponse BuildUserResponse(
    ApplicationUser user,
    IList<string> roles,
    KioskLoginContext? kioskContext = null)
        {
            var hasWorkforceProfile = user.WorkforceProfileId.HasValue;
            var isKioskAccount = kioskContext?.IsKioskAccount == true;

            var profileType =
                isKioskAccount ? "KioskDevice" :
                user.EmployeeId.HasValue ? "Employee" :
                user.DoctorId.HasValue ? "Doctor" :
                user.ExternalUserId.HasValue ? "ExternalUser" :
                "AccountOnly";

            var workforceProfileBaseEndpoint = hasWorkforceProfile
                ? $"/api/v1/corporate/human-resource/workforce-profiles/{user.WorkforceProfileId}"
                : null;

            return new UserLoginResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.DisplayName,
                UserType = user.UserType.ToString(),
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,

                ProfilePhotoPath = user.ProfilePhotoPath,
                ProfilePhotoUrl = BuildPublicFileUrl(user.ProfilePhotoPath),

                DepartmentId = user.PrimaryDepartmentId,
                PositionId = user.PrimaryPositionId,

                PrimaryDepartmentId = user.PrimaryDepartmentId,
                PrimaryPositionId = user.PrimaryPositionId,

                WorkforceProfileId = user.WorkforceProfileId,
                EmployeeId = user.EmployeeId,
                DoctorId = user.DoctorId,
                ExternalUserId = user.ExternalUserId,

                HasWorkforceProfile = hasWorkforceProfile,
                ProfileType = profileType,

                IsKioskAccount = isKioskAccount,

                WorkforceContext = new UserWorkforceContextResponse
                {
                    UserId = user.Id,
                    WorkforceProfileId = user.WorkforceProfileId,
                    EmployeeId = user.EmployeeId,
                    DoctorId = user.DoctorId,
                    ExternalUserId = user.ExternalUserId,
                    CanAccessWorkforceModules = hasWorkforceProfile,
                    WorkforceProfileBaseEndpoint = workforceProfileBaseEndpoint
                },

                KioskContext = isKioskAccount && kioskContext?.KioskDeviceId.HasValue == true
                    ? new UserKioskContextResponse
                    {
                        UserId = user.Id,
                        KioskDeviceId = kioskContext.KioskDeviceId.Value,
                        DeviceCode = kioskContext.DeviceCode,
                        DeviceName = kioskContext.DeviceName,
                        DeviceTypeName = kioskContext.DeviceTypeName,
                        DeviceStatusName = kioskContext.DeviceStatusName,
                        LocationName = kioskContext.LocationName,
                        FloorName = kioskContext.FloorName,
                        IsDeviceActive = kioskContext.IsDeviceActive,
                        IsLoginCreated = kioskContext.IsLoginCreated,
                        IsLoginEnabled = kioskContext.IsLoginEnabled,
                        IsLoginLocked = kioskContext.IsLoginLocked,
                        CanLogin = kioskContext.CanLogin,
                        IsAllowWalkIn = kioskContext.IsAllowWalkIn,
                        IsAllowAppointment = kioskContext.IsAllowAppointment,
                        IsAllowInsuranceRegistration = kioskContext.IsAllowInsuranceRegistration
                    }
                    : null
            };
        }

        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var normalizedPath = filePath.Trim();

            if (normalizedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            var publicBaseUrl = _configuration["FileStorage:PublicBaseUrl"];

            if (!string.IsNullOrWhiteSpace(publicBaseUrl))
            {
                return $"{publicBaseUrl.TrimEnd('/')}/{normalizedPath.TrimStart('/')}";
            }

            return $"{Request.Scheme}://{Request.Host}/{normalizedPath.TrimStart('/')}";
        }

        private class GeofenceValidationResult
        {
            public bool IsValid { get; set; }

            public string Message { get; set; } = string.Empty;

            public double? DistanceMeters { get; set; }

            public bool IsBypassed { get; set; }

            public string? BypassReason { get; set; }

            public static GeofenceValidationResult Ok(double? distanceMeters)
            {
                return new GeofenceValidationResult
                {
                    IsValid = true,
                    DistanceMeters = distanceMeters
                };
            }

            public static GeofenceValidationResult Bypassed(string? reason)
            {
                return new GeofenceValidationResult
                {
                    IsValid = true,
                    IsBypassed = true,
                    BypassReason = reason,
                    DistanceMeters = null
                };
            }

            public static GeofenceValidationResult Fail(string message, double? distanceMeters = null)
            {
                return new GeofenceValidationResult
                {
                    IsValid = false,
                    Message = message,
                    DistanceMeters = distanceMeters
                };
            }
        }

        private class KioskLoginContext
        {
            public bool IsKioskAccount { get; set; }

            public Guid? KioskDeviceId { get; set; }

            public string DeviceCode { get; set; } = string.Empty;

            public string DeviceName { get; set; } = string.Empty;

            public string? DeviceTypeName { get; set; }

            public string? DeviceStatusName { get; set; }

            public string? LocationName { get; set; }

            public string? FloorName { get; set; }

            public bool IsDeviceActive { get; set; }

            public bool IsLoginCreated { get; set; }

            public bool IsLoginEnabled { get; set; }

            public bool IsLoginLocked { get; set; }

            public bool CanLogin { get; set; }

            public bool IsAllowWalkIn { get; set; }

            public bool IsAllowAppointment { get; set; }

            public bool IsAllowInsuranceRegistration { get; set; }

            public string? BlockReason { get; set; }

            public static KioskLoginContext None()
            {
                return new KioskLoginContext
                {
                    IsKioskAccount = false,
                    IsLoginCreated = false,
                    IsLoginEnabled = false,
                    IsLoginLocked = false,
                    CanLogin = false
                };
            }
        }
    }
}