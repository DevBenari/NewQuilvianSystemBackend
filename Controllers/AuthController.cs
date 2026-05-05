using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
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

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            LanguageService languageService,
            LoggerService loggerService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _languageService = languageService;
            _loggerService = loggerService;
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

            var attendanceResult = await RecordAttendanceOnLoginAsync(
                user,
                request,
                geofenceValidation.DistanceMeters
            );

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

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
                    user.FullName,
                    user.UserType,
                    user.DepartmentId,
                    user.PositionId,
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

        private async Task<LoginAttendanceResult> RecordAttendanceOnLoginAsync(
    ApplicationUser user,
    LoginRequest request,
    double? distanceMeters)
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

            var attendance = new EmpAttendance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EmployeeId = employeeId,
                DoctorId = doctorId,
                AttendanceDate = attendanceDate,
                CheckInAt = DateTime.UtcNow,
                CheckInLatitude = request.Latitude.Value,
                CheckInLongitude = request.Longitude.Value,
                CheckInAccuracyMeters = request.AccuracyMeters,
                CheckInDistanceMeters = distanceMeters ?? 0,
                PersonType = user.UserType.ToString(),
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

        private (bool IsValid, string Message, double? DistanceMeters) ValidateLoginGeofence(ApplicationUser user, LoginRequest request)
        {
            return ValidateGeofence(
                user,
                request.Latitude,
                request.Longitude,
                request.AccuracyMeters
            );
        }

        private (bool IsValid, string Message, double? DistanceMeters) ValidateGeofence(
            ApplicationUser user,
            double? latitude,
            double? longitude,
            double? accuracyMeters)
        {
            var geofenceEnabled = _configuration.GetValue<bool>("LoginGeofence:Enabled");

            if (!geofenceEnabled)
            {
                return (true, string.Empty, null);
            }

            var applyToSuperAdmin = _configuration.GetValue<bool>("LoginGeofence:ApplyToSuperAdmin");

            if (user.UserType == UserType.SuperAdmin && !applyToSuperAdmin)
            {
                return (true, string.Empty, null);
            }

            if (!latitude.HasValue || !longitude.HasValue)
            {
                return (false, "Lokasi wajib diaktifkan.", null);
            }

            if (latitude.Value < -90 || latitude.Value > 90)
            {
                return (false, "Latitude tidak valid.", null);
            }

            if (longitude.Value < -180 || longitude.Value > 180)
            {
                return (false, "Longitude tidak valid.", null);
            }

            var hospitalLatitude = _configuration.GetValue<double?>("LoginGeofence:Latitude");
            var hospitalLongitude = _configuration.GetValue<double?>("LoginGeofence:Longitude");
            var allowedRadiusMeters = _configuration.GetValue<double>("LoginGeofence:AllowedRadiusMeters");
            var maxAccuracyMeters = _configuration.GetValue<double>("LoginGeofence:MaxAccuracyMeters");

            if (!hospitalLatitude.HasValue || !hospitalLongitude.HasValue)
            {
                return (false, "Koordinat rumah sakit belum dikonfigurasi.", null);
            }

            if (allowedRadiusMeters <= 0)
            {
                allowedRadiusMeters = 1000;
            }

            if (maxAccuracyMeters > 0)
            {
                if (!accuracyMeters.HasValue)
                {
                    return (
                        false,
                        "Akurasi lokasi tidak terbaca. Silakan aktifkan GPS/lokasi dengan akurasi tinggi.",
                        null
                    );
                }

                if (accuracyMeters.Value > maxAccuracyMeters)
                {
                    return (
                        false,
                        $"Akurasi lokasi terlalu rendah. Akurasi saat ini {accuracyMeters.Value:0} meter, maksimal {maxAccuracyMeters:0} meter.",
                        null
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
                return (
                    false,
                    $"Lokasi Anda berada {distanceMeters:0} meter dari rumah sakit. Maksimal jarak yang diizinkan {allowedRadiusMeters:0} meter.",
                    distanceMeters
                );
            }

            return (true, string.Empty, distanceMeters);
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
                new Claim("full_name", user.FullName),
                new Claim("user_type", user.UserType.ToString()),
                new Claim("hospital_id", user.HospitalId?.ToString() ?? string.Empty),
                new Claim("department_id", user.DepartmentId?.ToString() ?? string.Empty),
                new Claim("position_id", user.PositionId?.ToString() ?? string.Empty),

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

            Response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = _environment.IsDevelopment()
                    ? SameSiteMode.Lax
                    : SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes),
                MaxAge = TimeSpan.FromMinutes(expireMinutes),
                Path = "/"
            });
        }

        private void ClearAuthCookie()
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";

            Response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = _environment.IsDevelopment()
                    ? SameSiteMode.Lax
                    : SameSiteMode.None,
                Path = "/"
            });
        }

        private AuthInfoResponse BuildAuthInfoResponse()
        {
            var cookieName = _configuration["Jwt:CookieName"] ?? "quilvian_access_token";
            var expireMinutes = GetJwtExpireMinutes();
            var isDevelopment = _environment.IsDevelopment();

            return new AuthInfoResponse
            {
                Scheme = "Cookie",
                CookieName = cookieName,
                IsHttpOnly = true,
                SameSite = isDevelopment ? "Lax" : "None",
                Secure = !isDevelopment,
                ExpiresInMinutes = expireMinutes,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expireMinutes),
                FrontendInstruction = "Gunakan credentials: 'include' pada setiap request ke backend. Access token tidak dikirim di response body karena disimpan di HttpOnly cookie."
            };
        }

        private static UserLoginResponse BuildUserResponse(ApplicationUser user, IList<string> roles)
        {
            return new UserLoginResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                UserType = user.UserType.ToString(),
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                HospitalId = user.HospitalId,
                DepartmentId = user.DepartmentId,
                PositionId = user.PositionId
            };
        }
    }
}