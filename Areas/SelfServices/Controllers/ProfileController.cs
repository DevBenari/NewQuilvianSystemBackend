using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.SelfServices.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
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
    [Route("api/v1/self-services/profile")]
    [AccessController(
        moduleCode: "SELF_SERVICES",
        moduleName: "Self Services",
        displayName: "Profile",
        AreaName = "SelfServices",
        ControllerName = "Profile",
        Description = "Self service profile user login dan penggantian foto profile",
        SortOrder = 11
    )]
    [Tags("Self Services / Profile")]
    public class ProfileController : ControllerBase
    {
        private const string LogCategory = "SelfServices.Profile";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _loggerService = loggerService;
        }

        // =========================================================
        // METADATA
        // =========================================================

        [HttpGet("metadata")]
        [ProducesResponseType(typeof(ApiResponse<SelfServiceProfileMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Profile",
            Description = "Melihat metadata profile self service",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Profile", "Read")]
        public async Task<IActionResult> GetMetadata()
        {
            var result = new SelfServiceProfileMetadataResponse
            {
                MaxProfilePhotoSizeMb = GetMaxProfilePhotoSizeMb(),
                AllowedProfilePhotoExtensions = GetAllowedProfilePhotoExtensions().ToList(),
                DefaultUserProfilePhotoPath = GetDefaultUserProfilePhotoPath(),
                DefaultDoctorProfilePhotoPath = GetDefaultDoctorProfilePhotoPath(),
                ProfilePhotoFolderName = GetProfilePhotoFolderName(),
                PublicRequestPath = GetPublicRequestPath(),
                PublicBaseUrl = _configuration["FileStorage:PublicBaseUrl"] ?? string.Empty
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.GetMetadata",
                "Mengambil metadata profile self service.",
                result
            );

            return Ok(ApiResponse<SelfServiceProfileMetadataResponse>.Ok(
                result,
                "Metadata profile berhasil diambil."
            ));
        }

        // =========================================================
        // PROFILE
        // =========================================================

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<SelfServiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Read",
            "Read Profile",
            Description = "Melihat profile user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Profile", "Read")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var result = await BuildProfileResponseAsync(user);

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.GetProfile",
                "Mengambil profile user login.",
                new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.UserType
                }
            );

            return Ok(ApiResponse<SelfServiceProfileResponse>.Ok(
                result,
                "Profile user berhasil diambil."
            ));
        }

        // =========================================================
        // UPDATE PHOTO
        // =========================================================

        [HttpPost("photo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<SelfServiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Update",
            "Update Profile Photo",
            Description = "Mengubah foto profile user login melalui self service",
            AccessType = AccessTypes.Update,
            SortOrder = 2
        )]
        [AccessPermission("Profile", "Update")]
        public async Task<IActionResult> UpdateProfilePhoto([FromForm] UpdateSelfServiceProfilePhotoRequest request)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            if (!user.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Akun tidak aktif."
                ));
            }

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "File foto profile wajib diupload."
                ));
            }

            var validation = ValidateProfilePhotoFile(request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var oldPhotoPath = user.ProfilePhotoPath;
            var newPhotoPath = await SaveProfilePhotoAsync(user, request.File);

            user.ProfilePhotoPath = newPhotoPath;
            user.UpdateDateTime = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                DeletePhysicalProfilePhotoIfCustom(newPhotoPath);

                var errors = string.Join(
                    " | ",
                    updateResult.Errors.Select(x => $"{x.Code}: {x.Description}")
                );

                await _loggerService.WarningAsync(
                    LogCategory,
                    "Profile.UpdateProfilePhoto",
                    "Gagal update path foto profile ke database.",
                    new
                    {
                        user.Id,
                        user.Email,
                        Errors = errors
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Gagal update foto profile: {errors}"
                ));
            }

            DeletePhysicalProfilePhotoIfCustom(oldPhotoPath);

            var result = await BuildProfileResponseAsync(user);

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.UpdateProfilePhoto",
                "Foto profile berhasil diperbarui.",
                new
                {
                    user.Id,
                    user.Email,
                    OldPhotoPath = oldPhotoPath,
                    NewPhotoPath = newPhotoPath
                }
            );

            return Ok(ApiResponse<SelfServiceProfileResponse>.Ok(
                result,
                "Foto profile berhasil diperbarui."
            ));
        }

        // =========================================================
        // RESET PHOTO
        // =========================================================

        [HttpDelete("photo")]
        [ProducesResponseType(typeof(ApiResponse<SelfServiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Delete",
            "Delete Profile Photo",
            Description = "Mengembalikan foto profile user login ke default",
            AccessType = AccessTypes.Delete,
            SortOrder = 3
        )]
        [AccessPermission("Profile", "Delete")]
        public async Task<IActionResult> ResetProfilePhoto()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var oldPhotoPath = user.ProfilePhotoPath;
            var defaultPhotoPath = GetDefaultProfilePhotoPath(user);

            user.ProfilePhotoPath = defaultPhotoPath;
            user.UpdateDateTime = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    updateResult.Errors.Select(x => $"{x.Code}: {x.Description}")
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Gagal reset foto profile: {errors}"
                ));
            }

            DeletePhysicalProfilePhotoIfCustom(oldPhotoPath);

            var result = await BuildProfileResponseAsync(user);

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.ResetProfilePhoto",
                "Foto profile berhasil dikembalikan ke default.",
                new
                {
                    user.Id,
                    user.Email,
                    OldPhotoPath = oldPhotoPath,
                    DefaultPhotoPath = defaultPhotoPath
                }
            );

            return Ok(ApiResponse<SelfServiceProfileResponse>.Ok(
                result,
                "Foto profile berhasil dikembalikan ke default."
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

        private async Task<SelfServiceProfileResponse> BuildProfileResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            string? departmentName = null;
            string? positionName = null;

            if (user.PrimaryDepartmentId.HasValue && user.PrimaryDepartmentId.Value != Guid.Empty)
            {
                departmentName = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .Where(x => x.Id == user.PrimaryDepartmentId.Value)
                    .Select(x => x.DepartmentName)
                    .FirstOrDefaultAsync();
            }

            if (user.PrimaryPositionId.HasValue && user.PrimaryPositionId.Value != Guid.Empty)
            {
                positionName = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .Where(x => x.Id == user.PrimaryPositionId.Value)
                    .Select(x => x.PositionName)
                    .FirstOrDefaultAsync();
            }

            return new SelfServiceProfileResponse
            {
                UserId = user.Id,
                UserCode = user.UserCode,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                UserType = user.UserType,
                UserTypeName = user.UserType.ToString(),
                Roles = roles.ToList(),

                WorkforceProfileId = user.WorkforceProfileId,
                EmployeeId = user.EmployeeId,
                DoctorId = user.DoctorId,
                ExternalUserId = user.ExternalUserId,

                PrimaryDepartmentId = user.PrimaryDepartmentId,
                PrimaryDepartmentName = departmentName,
                PrimaryPositionId = user.PrimaryPositionId,
                PrimaryPositionName = positionName,

                ProfilePhotoPath = user.ProfilePhotoPath,
                ProfilePhotoUrl = BuildPublicFileUrl(user.ProfilePhotoPath),

                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CanChangeProfilePhoto = user.IsActive,

                MaxProfilePhotoSizeMb = GetMaxProfilePhotoSizeMb(),
                AllowedProfilePhotoExtensions = GetAllowedProfilePhotoExtensions().ToList()
            };
        }

        private (bool IsValid, string Message) ValidateProfilePhotoFile(IFormFile file)
        {
            var maxSizeMb = GetMaxProfilePhotoSizeMb();
            var maxSizeBytes = (long)maxSizeMb * 1024 * 1024;

            if (file.Length > maxSizeBytes)
            {
                return (false, $"Ukuran foto profile maksimal {maxSizeMb} MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
            {
                return (false, "File wajib memiliki ekstensi.");
            }

            var allowedExtensions = GetAllowedProfilePhotoExtensions();

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return (false, $"Ekstensi file tidak valid. Gunakan: {string.Join(", ", allowedExtensions)}.");
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "File harus berupa gambar.");
            }

            return (true, "File valid.");
        }

        private async Task<string> SaveProfilePhotoAsync(ApplicationUser user, IFormFile file)
        {
            var uploadRootPath = _configuration["FileStorage:UploadRootPath"];

            if (string.IsNullOrWhiteSpace(uploadRootPath))
            {
                throw new InvalidOperationException("FileStorage:UploadRootPath belum dikonfigurasi.");
            }

            var publicRequestPath = GetPublicRequestPath();
            var profilePhotoFolderName = GetProfilePhotoFolderName();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";

            string relativeFolder;

            if (user.WorkforceProfileId.HasValue && user.WorkforceProfileId.Value != Guid.Empty)
            {
                relativeFolder = Path.Combine(
                    "workforce-profiles",
                    user.WorkforceProfileId.Value.ToString(),
                    profilePhotoFolderName
                );
            }
            else
            {
                relativeFolder = Path.Combine(
                    "users",
                    user.Id.ToString(),
                    profilePhotoFolderName
                );
            }

            var physicalFolder = Path.Combine(uploadRootPath, relativeFolder);
            Directory.CreateDirectory(physicalFolder);

            var physicalPath = Path.Combine(physicalFolder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"{publicRequestPath.TrimEnd('/')}/" +
                             Path.Combine(relativeFolder, fileName).Replace("\\", "/");

            return publicPath;
        }

        private string GetDefaultProfilePhotoPath(ApplicationUser user)
        {
            var isDoctor =
                user.UserType == UserType.PermanentDoctor ||
                user.UserType == UserType.GuestDoctor ||
                user.DoctorId.HasValue;

            return isDoctor
                ? GetDefaultDoctorProfilePhotoPath()
                : GetDefaultUserProfilePhotoPath();
        }

        private string GetDefaultUserProfilePhotoPath()
        {
            var value = _configuration["FileStorage:DefaultUserProfilePhotoPath"];

            return string.IsNullOrWhiteSpace(value)
                ? "/uploads/default-profile-photos/user.png"
                : value.Trim();
        }

        private string GetDefaultDoctorProfilePhotoPath()
        {
            var value = _configuration["FileStorage:DefaultDoctorProfilePhotoPath"];

            return string.IsNullOrWhiteSpace(value)
                ? "/uploads/default-profile-photos/dokter.png"
                : value.Trim();
        }

        private string GetPublicRequestPath()
        {
            var value = _configuration["FileStorage:PublicRequestPath"];

            return string.IsNullOrWhiteSpace(value)
                ? "/uploads"
                : value.Trim();
        }

        private string GetProfilePhotoFolderName()
        {
            var value = _configuration["FileStorage:ProfilePhotoFolderName"];

            return string.IsNullOrWhiteSpace(value)
                ? "profile-photo"
                : value.Trim();
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

        private void DeletePhysicalProfilePhotoIfCustom(string? photoPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(photoPath))
                {
                    return;
                }

                var normalizedPath = photoPath.Trim();

                if (normalizedPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    normalizedPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(normalizedPath, UriKind.Absolute, out var uri))
                    {
                        normalizedPath = uri.AbsolutePath;
                    }
                    else
                    {
                        return;
                    }
                }

                if (IsDefaultProfilePhotoPath(normalizedPath))
                {
                    return;
                }

                var publicRequestPath = GetPublicRequestPath();

                if (!normalizedPath.StartsWith(publicRequestPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var physicalPath = ResolveUploadPhysicalPath(normalizedPath);

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch
            {
                // Jangan gagalkan proses update profile hanya karena gagal hapus file lama.
            }
        }

        private bool IsDefaultProfilePhotoPath(string photoPath)
        {
            var defaultUserPhoto = GetDefaultUserProfilePhotoPath();
            var defaultDoctorPhoto = GetDefaultDoctorProfilePhotoPath();

            return string.Equals(photoPath, defaultUserPhoto, StringComparison.OrdinalIgnoreCase)
                || string.Equals(photoPath, defaultDoctorPhoto, StringComparison.OrdinalIgnoreCase)
                || photoPath.Contains("/uploads/default-profile-photos/", StringComparison.OrdinalIgnoreCase);
        }

        private string ResolveUploadPhysicalPath(string publicFilePath)
        {
            var uploadRootPath = _configuration["FileStorage:UploadRootPath"];

            if (string.IsNullOrWhiteSpace(uploadRootPath))
            {
                throw new InvalidOperationException("FileStorage:UploadRootPath belum dikonfigurasi.");
            }

            var publicRequestPath = GetPublicRequestPath();

            var normalizedPath = publicFilePath.Trim();

            if (normalizedPath.StartsWith(publicRequestPath, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath[publicRequestPath.Length..];
            }

            normalizedPath = normalizedPath
                .TrimStart('/', '\\')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            var rootFullPath = Path.GetFullPath(uploadRootPath);

            if (!rootFullPath.EndsWith(Path.DirectorySeparatorChar))
            {
                rootFullPath += Path.DirectorySeparatorChar;
            }

            var physicalPath = Path.GetFullPath(Path.Combine(rootFullPath, normalizedPath));

            if (!physicalPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Path file tidak valid.");
            }

            return physicalPath;
        }

        private int GetMaxProfilePhotoSizeMb()
        {
            return _configuration.GetValue<int?>("FileStorage:MaxProfilePhotoSizeMb") ?? 2;
        }

        private string[] GetAllowedProfilePhotoExtensions()
        {
            return _configuration
                .GetSection("FileStorage:AllowedProfilePhotoExtensions")
                .Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        }

        [HttpPut("password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<SelfServiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Update",
            "Change Profile Password",
            Description = "Mengubah password akun user login melalui self service",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("Profile", "Update")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeSelfServicePasswordRequest request)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            if (!user.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Akun tidak aktif."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Password lama wajib diisi."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Password baru wajib diisi."
                ));
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Konfirmasi password baru tidak sesuai."
                ));
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Password baru tidak boleh sama dengan password lama."
                ));
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    changePasswordResult.Errors.Select(x => $"{x.Code}: {x.Description}")
                );

                await _loggerService.WarningAsync(
                    LogCategory,
                    "Profile.ChangePassword",
                    "Gagal mengubah password user.",
                    new
                    {
                        user.Id,
                        user.Email,
                        Errors = errors
                    }
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Gagal mengubah password: {errors}"
                ));
            }

            user.MustChangePassword = false;
            user.UpdateDateTime = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    updateResult.Errors.Select(x => $"{x.Code}: {x.Description}")
                );

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Password berhasil diganti, tetapi gagal memperbarui status akun: {errors}"
                ));
            }

            var response = await BuildProfileResponseAsync(user);

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.ChangePassword",
                "Password user berhasil diubah.",
                new
                {
                    user.Id,
                    user.Email,
                    user.UserType
                }
            );

            return Ok(ApiResponse<SelfServiceProfileResponse>.Ok(
                response,
                "Password berhasil diubah."
            ));
        }

        [HttpPost("password/generate")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<GenerateSelfServicePasswordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Update",
            "Generate Profile Password",
            Description = "Generate password random berdasarkan username user login",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("Profile", "Update")]
        public async Task<IActionResult> GeneratePassword()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "User tidak ditemukan."
                ));
            }

            var usernameSource =
                !string.IsNullOrWhiteSpace(user.UserName)
                    ? user.UserName!
                    : !string.IsNullOrWhiteSpace(user.Email)
                        ? user.Email!
                        : user.DisplayName;

            var prefix = BuildPasswordPrefix(usernameSource);

            var suggestedPassword = GeneratePasswordFromPrefix(prefix);

            var result = new GenerateSelfServicePasswordResponse
            {
                UsernameSource = usernameSource,
                Prefix = prefix,
                SuggestedPassword = suggestedPassword,
                Pattern = "{usernamePrefix}-{4 uppercase}{5 digit}!@#$",
                GeneratedAt = DateTime.UtcNow
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Profile.GeneratePassword",
                "Generate password suggestion untuk user login.",
                new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    Prefix = prefix
                }
            );

            return Ok(ApiResponse<GenerateSelfServicePasswordResponse>.Ok(
                result,
                "Password random berhasil dibuat."
            ));
        }

        private static string BuildPasswordPrefix(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return "quilvian";
            }

            var normalizedSource = source.Trim();

            var atIndex = normalizedSource.IndexOf('@');

            if (atIndex > 0)
            {
                normalizedSource = normalizedSource[..atIndex];
            }

            var chars = normalizedSource
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray();

            var prefix = new string(chars);

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return "quilvian";
            }

            if (prefix.Length < 4)
            {
                prefix = $"{prefix}user";
            }

            if (prefix.Length > 20)
            {
                prefix = prefix[..20];
            }

            return prefix;
        }

        private static string GeneratePasswordFromPrefix(string prefix)
        {
            const string uppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digitChars = "23456789";

            var uppercasePart = GetSecureRandomString(uppercaseChars, 4);
            var digitPart = GetSecureRandomString(digitChars, 5);
            const string symbolPart = "!@#$";

            return $"{prefix}-{uppercasePart}{digitPart}{symbolPart}";
        }

        private static string GetSecureRandomString(string allowedChars, int length)
        {
            if (string.IsNullOrWhiteSpace(allowedChars))
            {
                throw new ArgumentException("Allowed chars tidak boleh kosong.", nameof(allowedChars));
            }

            if (length <= 0)
            {
                return string.Empty;
            }

            var result = new char[length];

            for (var i = 0; i < length; i++)
            {
                var index = RandomNumberGenerator.GetInt32(allowedChars.Length);
                result[i] = allowedChars[index];
            }

            return new string(result);
        }
    }
}