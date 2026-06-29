using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/queue-voice-profiles")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Queue Voice Profile",
        AreaName = "HealthServices",
        ControllerName = "QueueVoiceProfile",
        Description = "Master profil suara panggilan antrean",
        SortOrder = 8
    )]
    [Tags("Health Services / Registration Management / Queue Voice Profile")]
    public class QueueVoiceProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public QueueVoiceProfileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<QueueVoiceProfileResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Voice Profile", Description = "Melihat master profil suara panggilan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetProfiles([FromQuery] bool includeInactive = true)
        {
            var query = _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!includeInactive)
            {
                query = query.Where(x => x.IsActive);
            }

            var result = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.VoiceName)
                .Select(x => MapResponse(x))
                .ToListAsync();

            return Ok(ApiResponse<List<QueueVoiceProfileResponse>>.Ok(result, "Data profil suara berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Queue Voice Profile Detail", Description = "Melihat detail profil suara panggilan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("QueueVoiceProfile", "Read")]
        public async Task<IActionResult> GetProfileById(Guid id)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil suara tidak ditemukan."));
            }

            return Ok(ApiResponse<QueueVoiceProfileResponse>.Ok(MapResponse(entity), "Detail profil suara berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Queue Voice Profile", Description = "Membuat profil suara panggilan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("QueueVoiceProfile", "Create")]
        public async Task<IActionResult> CreateProfile([FromBody] QueueVoiceProfileRequest request)
        {
            var validation = await ValidateRequestAsync(request, null);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data profil suara tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                await UnsetOtherDefaultsAsync(null, now, actorUserId);
            }

            var entity = new MstQueueVoiceProfile
            {
                Id = Guid.NewGuid(),
                VoiceCode = NormalizeCode(request.VoiceCode),
                VoiceName = request.VoiceName.Trim(),
                Gender = NormalizeGender(request.Gender),
                Language = string.IsNullOrWhiteSpace(request.Language) ? "id-ID" : request.Language.Trim(),
                ModelPath = request.ModelPath.Trim(),
                LengthScale = Clamp(request.LengthScale, 0.70m, 1.50m),
                NoiseScale = Clamp(request.NoiseScale, 0.10m, 1.50m),
                NoiseW = Clamp(request.NoiseW, 0.10m, 1.50m),
                Volume = Clamp(request.Volume, 0.50m, 2.00m),
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstQueueVoiceProfile>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(ApiResponse<QueueVoiceProfileResponse>.Ok(MapResponse(entity), "Profil suara berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Queue Voice Profile", Description = "Mengubah profil suara panggilan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("QueueVoiceProfile", "Update")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] QueueVoiceProfileRequest request)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil suara tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(request, id);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data profil suara tidak valid."));
            }

            if (request.IsDefault && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Profil default harus aktif."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                await UnsetOtherDefaultsAsync(id, now, actorUserId);
            }

            entity.VoiceCode = NormalizeCode(request.VoiceCode);
            entity.VoiceName = request.VoiceName.Trim();
            entity.Gender = NormalizeGender(request.Gender);
            entity.Language = string.IsNullOrWhiteSpace(request.Language) ? "id-ID" : request.Language.Trim();
            entity.ModelPath = request.ModelPath.Trim();
            entity.LengthScale = Clamp(request.LengthScale, 0.70m, 1.50m);
            entity.NoiseScale = Clamp(request.NoiseScale, 0.10m, 1.50m);
            entity.NoiseW = Clamp(request.NoiseW, 0.10m, 1.50m);
            entity.Volume = Clamp(request.Volume, 0.50m, 2.00m);
            entity.IsDefault = request.IsActive && request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(ApiResponse<QueueVoiceProfileResponse>.Ok(MapResponse(entity), "Profil suara berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Queue Voice Profile", Description = "Menghapus profil suara panggilan", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("QueueVoiceProfile", "Delete")]
        public async Task<IActionResult> DeleteProfile(Guid id)
        {
            var entity = await _dbContext.Set<MstQueueVoiceProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil suara tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = GetCurrentUserId();
            entity.UpdateDateTime = now;
            entity.UpdateBy = entity.DeleteBy;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Profil suara berhasil dihapus."));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(QueueVoiceProfileRequest request, Guid? excludeId)
        {
            if (string.IsNullOrWhiteSpace(request.VoiceCode)) return (false, "Kode voice wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.VoiceName)) return (false, "Nama voice wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ModelPath)) return (false, "Model path wajib diisi.");

            var normalizedCode = NormalizeCode(request.VoiceCode);
            if (!Regex.IsMatch(normalizedCode, "^[a-zA-Z0-9_-]{3,80}$"))
            {
                return (false, "Kode voice hanya boleh berisi huruf, angka, underscore, atau strip.");
            }

            var duplicateQuery = _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.VoiceCode == normalizedCode);

            if (excludeId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateQuery.AnyAsync())
            {
                return (false, "Kode voice sudah digunakan.");
            }

            return (true, null);
        }

        private async Task UnsetOtherDefaultsAsync(Guid? exceptId, DateTime now, Guid actorUserId)
        {
            var query = _dbContext.Set<MstQueueVoiceProfile>()
                .Where(x => x.IsDefault && !x.IsDelete);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var profiles = await query.ToListAsync();
            foreach (var profile in profiles)
            {
                profile.IsDefault = false;
                profile.UpdateDateTime = now;
                profile.UpdateBy = actorUserId;
            }
        }

        private static QueueVoiceProfileResponse MapResponse(MstQueueVoiceProfile entity)
        {
            return new QueueVoiceProfileResponse
            {
                Id = entity.Id,
                VoiceCode = entity.VoiceCode,
                VoiceName = entity.VoiceName,
                Gender = entity.Gender,
                Language = entity.Language,
                ModelPath = entity.ModelPath,
                LengthScale = entity.LengthScale,
                NoiseScale = entity.NoiseScale,
                NoiseW = entity.NoiseW,
                Volume = entity.Volume,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                Source = "Database"
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }

        private static string NormalizeCode(string value)
        {
            return Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_-]", "_").Trim('_');
        }

        private static string NormalizeGender(string value)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.Equals(normalized, "Male", StringComparison.OrdinalIgnoreCase)) return "Male";
            if (string.Equals(normalized, "Female", StringComparison.OrdinalIgnoreCase)) return "Female";
            return "Unknown";
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
