using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Areas.SelfServices.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.SelfServices.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/fingerprint")]
    [AccessController(
        moduleCode: "SELF_SERVICES",
        moduleName: "Self Services",
        displayName: "Fingerprint",
        AreaName = "SelfServices",
        ControllerName = "Fingerprint",
        Description = "Self service fingerprint registration untuk employee dan permanent doctor",
        SortOrder = 10
    )]
    [Tags("Self Services / Fingerprint")]
    public class FingerprintController : ControllerBase
    {
        private const string LogCategory = "SelfServices.Fingerprint";
        private const int MaxTemplateSizeBytes = 1024 * 1024;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDataProtector _protector;
        private readonly LoggerService _loggerService;

        public FingerprintController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IDataProtectionProvider dataProtectionProvider,
            LoggerService loggerService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _protector = dataProtectionProvider.CreateProtector("Quilvian.Fingerprint.Template.v1");
            _loggerService = loggerService;
        }

        // =========================================================
        // METADATA
        // =========================================================

        [HttpGet("metadata")]
        [ProducesResponseType(typeof(ApiResponse<FingerprintMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Fingerprint",
            Description = "Melihat metadata fingerprint self service",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Fingerprint", "Read")]
        public async Task<IActionResult> GetMetadata()
        {
            var result = new FingerprintMetadataResponse
            {
                DefaultFingerPosition = "RightThumb",
                DefaultTemplateFormat = "DigitalPersona.SampleFormat5",
                AllowedUserTypes = new List<string>
                {
                    UserType.Employee.ToString(),
                    UserType.PermanentDoctor.ToString()
                },
                FingerPositions = new List<FingerprintOptionResponse>
                {
                    new() { Value = "RightThumb", Label = "Jempol Kanan" },
                    new() { Value = "LeftThumb", Label = "Jempol Kiri" },
                    new() { Value = "RightIndex", Label = "Telunjuk Kanan" },
                    new() { Value = "LeftIndex", Label = "Telunjuk Kiri" },
                    new() { Value = "RightMiddle", Label = "Jari Tengah Kanan" },
                    new() { Value = "LeftMiddle", Label = "Jari Tengah Kiri" }
                },
                TemplateFormats = new List<FingerprintOptionResponse>
                {
                    new() { Value = "DigitalPersona.SampleFormat5", Label = "DigitalPersona Sample Format 5" },
                    new() { Value = "DigitalPersona.FMD.ANSI", Label = "DigitalPersona FMD ANSI" },
                    new() { Value = "DigitalPersona.FMD.ISO", Label = "DigitalPersona FMD ISO" }
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Fingerprint.GetMetadata",
                "Mengambil metadata fingerprint self service.",
                result
            );

            return Ok(ApiResponse<FingerprintMetadataResponse>.Ok(
                result,
                "Metadata fingerprint berhasil diambil."
            ));
        }

        // =========================================================
        // STATUS
        // =========================================================

        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<FingerprintStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Read",
            "Read Fingerprint",
            Description = "Melihat status fingerprint user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Fingerprint", "Read")]
        public async Task<IActionResult> GetStatus()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var eligibility = await ValidateFingerprintRegistrationEligibilityAsync(user);

            var fingerprints = await _dbContext.ApplicationUserFingerprintCredentials
                .AsNoTracking()
                .Where(x =>
                    x.UserId == user.Id &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.RegisteredAt)
                .Select(x => new FingerprintCredentialResponse
                {
                    Id = x.Id,
                    FingerPosition = x.FingerPosition,
                    TemplateFormat = x.TemplateFormat,
                    TemplateVersion = x.TemplateVersion,
                    DeviceId = x.DeviceId,
                    DeviceModel = x.DeviceModel,
                    SampleFormat = x.SampleFormat,
                    QualityScore = x.QualityScore,
                    EnrollmentSampleCount = x.EnrollmentSampleCount,
                    IsPrimary = x.IsPrimary,
                    RegisteredAt = x.RegisteredAt
                })
                .ToListAsync();

            var result = new FingerprintStatusResponse
            {
                UserId = user.Id,
                UserType = user.UserType.ToString(),
                CanRegister = eligibility.IsAllowed,
                Message = eligibility.Message,
                HasFingerprint = fingerprints.Any(),
                FingerprintCount = fingerprints.Count,
                Fingerprints = fingerprints
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Fingerprint.GetStatus",
                "Mengambil status fingerprint user.",
                new
                {
                    user.Id,
                    user.UserType,
                    result.CanRegister,
                    result.HasFingerprint,
                    result.FingerprintCount
                }
            );

            return Ok(ApiResponse<FingerprintStatusResponse>.Ok(
                result,
                "Status fingerprint berhasil diambil."
            ));
        }

        // =========================================================
        // REGISTER
        // =========================================================

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Create",
            "Create Fingerprint",
            Description = "Register fingerprint user login melalui self service",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("Fingerprint", "Create")]
        public async Task<IActionResult> Register([FromBody] FingerprintRegisterRequest request)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var eligibility = await ValidateFingerprintRegistrationEligibilityAsync(user);

            if (!eligibility.IsAllowed)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Fingerprint.Register",
                    "Register fingerprint ditolak karena user tidak eligible.",
                    new
                    {
                        user.Id,
                        user.UserType,
                        user.EmployeeId,
                        user.DoctorId,
                        eligibility.Message
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    eligibility.Message
                ));
            }

            var templateBase64 = !string.IsNullOrWhiteSpace(request.FingerprintTemplateBase64)
                ? request.FingerprintTemplateBase64
                : request.FingerprintSample;

            if (string.IsNullOrWhiteSpace(templateBase64))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Template fingerprint wajib diisi."
                ));
            }

            byte[] templateBytes;

            try
            {
                templateBytes = Convert.FromBase64String(CleanBase64(templateBase64));
            }
            catch
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format template fingerprint tidak valid."
                ));
            }

            if (templateBytes.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Template fingerprint kosong."
                ));
            }

            if (templateBytes.Length > MaxTemplateSizeBytes)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ukuran template fingerprint terlalu besar."
                ));
            }

            var fingerPosition = NormalizeText(request.FingerPosition, "RightThumb");
            var templateFormat = NormalizeText(request.TemplateFormat, "DigitalPersona.SampleFormat5");
            var templateHash = Convert.ToHexString(SHA256.HashData(templateBytes));

            var duplicateOtherUser = await _dbContext.ApplicationUserFingerprintCredentials
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId != user.Id &&
                    x.TemplateHash == templateHash &&
                    x.IsActive &&
                    !x.IsDelete);

            if (duplicateOtherUser)
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    "Fingerprint.Register",
                    "Register fingerprint ditolak karena template sudah digunakan user lain.",
                    new
                    {
                        user.Id,
                        user.UserType,
                        FingerPosition = fingerPosition,
                        TemplateHash = templateHash
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Fingerprint tidak dapat digunakan."
                ));
            }

            var existingSameFinger = await _dbContext.ApplicationUserFingerprintCredentials
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.FingerPosition == fingerPosition &&
                    x.IsActive &&
                    !x.IsDelete);

            var wasPrimary = existingSameFinger?.IsPrimary == true;

            if (existingSameFinger != null)
            {
                existingSameFinger.IsActive = false;
                existingSameFinger.IsPrimary = false;
                existingSameFinger.RevokedAt = DateTime.UtcNow;
                existingSameFinger.RevokedByUserId = user.Id;
                existingSameFinger.RevokedReason = "Re-register fingerprint.";
                existingSameFinger.UpdateDateTime = DateTime.UtcNow;
                existingSameFinger.UpdateBy = user.Id;

                await _dbContext.SaveChangesAsync();
            }

            var hasOtherActiveFingerprint = await _dbContext.ApplicationUserFingerprintCredentials
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == user.Id &&
                    x.IsActive &&
                    !x.IsDelete);

            var encryptedTemplate = _protector.Protect(templateBytes);

            var credential = new ApplicationUserFingerprintCredential
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                WorkforceProfileId = user.WorkforceProfileId,
                EmployeeId = user.EmployeeId,
                DoctorId = user.DoctorId,

                FingerPosition = fingerPosition,
                TemplateFormat = templateFormat,
                TemplateVersion = NormalizeNullableText(request.TemplateVersion),
                TemplateDataEncrypted = encryptedTemplate,
                TemplateHash = templateHash,

                DeviceId = NormalizeNullableText(request.DeviceId),
                DeviceModel = NormalizeNullableText(request.DeviceModel),
                SampleFormat = request.SampleFormat,
                QualityScore = request.QualityScore,
                EnrollmentSampleCount = request.EnrollmentSampleCount <= 0
                    ? 1
                    : request.EnrollmentSampleCount,

                IsPrimary = wasPrimary || !hasOtherActiveFingerprint,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow,
                RegisteredByUserId = user.Id,
                RegisteredIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                RegisteredUserAgent = Request.Headers.UserAgent.ToString(),

                CreateDateTime = DateTime.UtcNow,
                CreateBy = user.Id,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.ApplicationUserFingerprintCredentials.Add(credential);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Fingerprint.Register",
                "Fingerprint berhasil diregistrasi.",
                new
                {
                    credential.Id,
                    credential.UserId,
                    credential.WorkforceProfileId,
                    credential.EmployeeId,
                    credential.DoctorId,
                    credential.FingerPosition,
                    credential.TemplateFormat,
                    credential.DeviceId,
                    credential.SampleFormat,
                    credential.QualityScore,
                    credential.EnrollmentSampleCount,
                    credential.IsPrimary
                }
            );

            return Ok(ApiResponse<object>.Ok(
                new FingerprintCredentialResponse
                {
                    Id = credential.Id,
                    FingerPosition = credential.FingerPosition,
                    TemplateFormat = credential.TemplateFormat,
                    TemplateVersion = credential.TemplateVersion,
                    DeviceId = credential.DeviceId,
                    DeviceModel = credential.DeviceModel,
                    SampleFormat = credential.SampleFormat,
                    QualityScore = credential.QualityScore,
                    EnrollmentSampleCount = credential.EnrollmentSampleCount,
                    IsPrimary = credential.IsPrimary,
                    RegisteredAt = credential.RegisteredAt
                },
                "Fingerprint berhasil diregistrasi."
            ));
        }

        // =========================================================
        // SET PRIMARY
        // =========================================================

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Fingerprint",
            Description = "Mengubah fingerprint utama user login",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Fingerprint", "Update")]
        public async Task<IActionResult> SetPrimary(Guid id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var credential = await _dbContext.ApplicationUserFingerprintCredentials
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id &&
                    x.IsActive &&
                    !x.IsDelete);

            if (credential == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Fingerprint tidak ditemukan."
                ));
            }

            var activeFingerprints = await _dbContext.ApplicationUserFingerprintCredentials
                .Where(x =>
                    x.UserId == user.Id &&
                    x.IsActive &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in activeFingerprints)
            {
                item.IsPrimary = item.Id == id;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = user.Id;
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Fingerprint.SetPrimary",
                "Fingerprint utama berhasil diperbarui.",
                new
                {
                    user.Id,
                    FingerprintCredentialId = id
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Fingerprint utama berhasil diperbarui."
            ));
        }

        // =========================================================
        // REVOKE
        // =========================================================

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Fingerprint",
            Description = "Menonaktifkan fingerprint user login",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Fingerprint", "Delete")]
        public async Task<IActionResult> Revoke(Guid id)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var credential = await _dbContext.ApplicationUserFingerprintCredentials
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == user.Id &&
                    x.IsActive &&
                    !x.IsDelete);

            if (credential == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Fingerprint tidak ditemukan."
                ));
            }

            var wasPrimary = credential.IsPrimary;

            credential.IsActive = false;
            credential.IsPrimary = false;
            credential.RevokedAt = DateTime.UtcNow;
            credential.RevokedByUserId = user.Id;
            credential.RevokedReason = "Dihapus oleh user melalui self service.";
            credential.UpdateDateTime = DateTime.UtcNow;
            credential.UpdateBy = user.Id;

            if (wasPrimary)
            {
                var nextPrimary = await _dbContext.ApplicationUserFingerprintCredentials
                    .Where(x =>
                        x.Id != id &&
                        x.UserId == user.Id &&
                        x.IsActive &&
                        !x.IsDelete)
                    .OrderByDescending(x => x.RegisteredAt)
                    .FirstOrDefaultAsync();

                if (nextPrimary != null)
                {
                    nextPrimary.IsPrimary = true;
                    nextPrimary.UpdateDateTime = DateTime.UtcNow;
                    nextPrimary.UpdateBy = user.Id;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Fingerprint.Revoke",
                "Fingerprint berhasil dinonaktifkan.",
                new
                {
                    user.Id,
                    FingerprintCredentialId = id
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Fingerprint berhasil dinonaktifkan."
            ));
        }

        // =========================================================
        // PRIVATE HELPERS
        // =========================================================

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdText, out var userId))
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId.ToString());
        }

        private async Task<(bool IsAllowed, string Message)> ValidateFingerprintRegistrationEligibilityAsync(
            ApplicationUser user)
        {
            if (!user.IsActive)
            {
                return (false, "Akun tidak aktif.");
            }

            if (user.UserType == UserType.Employee)
            {
                if (!user.EmployeeId.HasValue)
                {
                    return (false, "Akun employee belum terhubung dengan data karyawan.");
                }

                var employeeExists = await _dbContext.MstEmployees.AnyAsync(x =>
                    x.Id == user.EmployeeId.Value &&
                    x.IsActive &&
                    !x.IsDelete);

                if (!employeeExists)
                {
                    return (false, "Data karyawan tidak aktif atau tidak ditemukan.");
                }

                return (true, "Employee boleh register fingerprint.");
            }

            if (user.UserType == UserType.PermanentDoctor)
            {
                if (!user.DoctorId.HasValue)
                {
                    return (false, "Akun dokter belum terhubung dengan data dokter.");
                }

                var doctorExists = await _dbContext.MstDoctors.AnyAsync(x =>
                    x.Id == user.DoctorId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.DoctorType == DoctorType.PermanentDoctor);

                if (!doctorExists)
                {
                    return (false, "Hanya dokter permanent yang boleh register fingerprint.");
                }

                return (true, "Permanent doctor boleh register fingerprint.");
            }

            if (user.UserType == UserType.GuestDoctor)
            {
                return (false, "Dokter tamu tidak diizinkan register fingerprint.");
            }

            if (user.UserType == UserType.ExternalUser)
            {
                return (false, "External user tidak diizinkan register fingerprint.");
            }

            return (false, "User ini tidak diizinkan register fingerprint.");
        }

        private static string CleanBase64(string value)
        {
            var trimmed = value.Trim();

            var commaIndex = trimmed.IndexOf(',');

            if (commaIndex >= 0)
            {
                return trimmed[(commaIndex + 1)..];
            }

            return trimmed;
        }

        private static string NormalizeText(string? value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}