using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.DTOs.Auth;
using QuilvianSystemBackend.Enum;
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

            var email = request.Email.Trim();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                await _loggerService.WarningAsync(
                    "Auth",
                    "Login",
                    "Login gagal. Email tidak ditemukan.",
                    new
                    {
                        Email = email
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

            var geofenceValidation = ValidateLoginGeofence(user, request);

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
                var attendanceResult = await RecordAttendanceOnLoginAsync(
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
                var token = GenerateJwtToken(user, roles);

                SetAuthCookie(token);

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
                        Auth = BuildAuthInfoResponse(),
                        Endpoints = new AuthEndpointResponse(),
                        User = BuildUserResponse(user, roles)
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
                        Email = email,
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
                catch
                {
                    continue;
                }
            }

            await _loggerService.InfoAsync(
                "Auth",
                "Fingerprint.Candidates",
                "Mengambil kandidat fingerprint aktif.",
                new { Count = result.Count }
            );

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
            var token = GenerateJwtToken(user, roles);

            SetAuthCookie(token);

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
                    Auth = BuildAuthInfoResponse(),
                    Endpoints = new AuthEndpointResponse(),
                    User = BuildUserResponse(user, roles)
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
                BuildUserResponse(user, roles),
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
            var newToken = GenerateJwtToken(user, roles);

            SetAuthCookie(newToken);

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
                    Auth = BuildAuthInfoResponse(),
                    Endpoints = new AuthEndpointResponse(),
                    User = BuildUserResponse(user, roles)
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
                var attendanceDate = DateOnly.FromDateTime(nowJakarta);

                var attendance = await _dbContext.EmpAttendances
                    .FirstOrDefaultAsync(x =>
                        x.UserId == user.Id &&
                        x.AttendanceDate == attendanceDate &&
                        !x.IsDelete);

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

            var schedule = await ResolveWorkScheduleAsync(user, attendanceDate);

            var lateResult = CalculateLateStatus(
                attendanceDate,
                nowJakarta,
                schedule
            );

            var attendance = new EmpAttendance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EmployeeId = employeeId,
                DoctorId = doctorId,
                WorkScheduleId = schedule?.Id,

                AttendanceDate = attendanceDate,
                CheckInAt = DateTime.UtcNow,

                WorkStartTime = schedule?.WorkStartTime,
                WorkEndTime = schedule?.WorkEndTime,
                CheckInToleranceMinutes = schedule?.CheckInToleranceMinutes ?? 0,

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

                CreateDateTime = DateTime.UtcNow,
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
                    attendance.WorkScheduleId,
                    attendance.AttendanceDate,
                    attendance.CheckInAt,
                    attendance.WorkStartTime,
                    attendance.WorkEndTime,
                    attendance.CheckInToleranceMinutes,
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

        private async Task<MstWorkSchedule?> ResolveWorkScheduleAsync(
    ApplicationUser user,
    DateOnly attendanceDate)
        {
            var schedules = await _dbContext.MstWorkSchedules
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= attendanceDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= attendanceDate))
                .ToListAsync();

            return schedules
                .OrderByDescending(x => GetSchedulePriority(x, user))
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefault(x => GetSchedulePriority(x, user) > 0);
        }

        private static int GetSchedulePriority(MstWorkSchedule schedule, ApplicationUser user)
        {
            if (schedule.UserId.HasValue && schedule.UserId.Value == user.Id)
            {
                return 100;
            }

            if (schedule.DepartmentId.HasValue &&
                schedule.PositionId.HasValue &&
                schedule.UserType.HasValue &&
                schedule.DepartmentId.Value == user.PrimaryDepartmentId &&
                schedule.PositionId.Value == user.PrimaryPositionId &&
                schedule.UserType.Value == user.UserType)
            {
                return 80;
            }

            if (schedule.DepartmentId.HasValue &&
                schedule.PositionId.HasValue &&
                schedule.DepartmentId.Value == user.PrimaryDepartmentId &&
                schedule.PositionId.Value == user.PrimaryPositionId)
            {
                return 70;
            }

            if (schedule.UserType.HasValue &&
                schedule.UserType.Value == user.UserType)
            {
                return 60;
            }

            if (schedule.IsDefault)
            {
                return 10;
            }

            return 0;
        }

        private static (bool IsLate, int LateMinutes, string AttendanceStatus) CalculateLateStatus(DateOnly attendanceDate, DateTime checkInJakarta, MstWorkSchedule? schedule)
        {
            if (schedule == null)
            {
                return (false, 0, "PresentNoSchedule");
            }

            var scheduledStart = attendanceDate.ToDateTime(schedule.WorkStartTime);
            var allowedCheckInTime = scheduledStart.AddMinutes(schedule.CheckInToleranceMinutes);

            if (checkInJakarta <= allowedCheckInTime)
            {
                return (false, 0, "Present");
            }

            var lateMinutes = (int)Math.Ceiling(
                (checkInJakarta - allowedCheckInTime).TotalMinutes
            );

            return (true, lateMinutes, "Late");
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

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
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
                new Claim("department_id", user.PrimaryDepartmentId?.ToString() ?? string.Empty),
                new Claim("position_id", user.PrimaryPositionId?.ToString() ?? string.Empty),
                new Claim("primary_department_id", user.PrimaryDepartmentId?.ToString() ?? string.Empty),
                new Claim("primary_position_id", user.PrimaryPositionId?.ToString() ?? string.Empty),

                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var expires = DateTime.UtcNow.AddMinutes(GetJwtExpireMinutes());

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

        private void SetAuthCookie(string token)
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";
            var expireMinutes = GetJwtExpireMinutes();

            Response.Cookies.Append(
                cookieName,
                token,
                BuildAuthCookieOptions(expireMinutes)
            );
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

        private AuthInfoResponse BuildAuthInfoResponse()
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";
            var expireMinutes = GetJwtExpireMinutes();
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

        private static UserLoginResponse BuildUserResponse(ApplicationUser user, IList<string> roles)
        {
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
                DepartmentId = user.PrimaryDepartmentId,
                PositionId = user.PrimaryPositionId
            };
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
    }
}